//#define LOG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.Text;
//using DigitalPlatform.LibraryClient;
//using DigitalPlatform.RFID;

namespace DigitalPlatform.IO
{
    // 一些公共的成员
    // T: IFingerprint 或 IRfid
    public class ManagerBase<T>
    {
        // 2022/6/11
        Task _task = null;
        public Task Task
        {
            get
            {
                return _task;
            }
        }
        public TimeSpan ShortWaitTime = TimeSpan.FromMilliseconds(500);
        public TimeSpan LongWaitTime = TimeSpan.FromMilliseconds(2000);

        event SetErrorEventHandler SetError = null;

        public void AddSetErrorEvent(SetErrorEventHandler handler)
        {
            SetError += handler;
        }

        public void RemoveSetErrorEvent(SetErrorEventHandler handler)
        {
            SetError -= handler;
        }

        event EventHandler Loop = null;

        public void AddLoopEvent(EventHandler handler)
        {
            Loop += handler;
        }

        public void RemoveLoopEvent(EventHandler handler)
        {
            Loop -= handler;
        }

        // public string State = "";   // pause/空

        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public ChannelPool<BaseChannel<T>> Channels = new ChannelPool<BaseChannel<T>>();

        public string Url { get; set; }

        public string Name { get; set; }

        /*
        static bool _checkState = true;

        public bool CheckState
        {
            get
            {
                return _checkState;
            }
            set
            {
                _checkState = value;
            }
        }
        */

        // 注意有死锁的风险
        public void ClearChannels()
        {
            this.Lock.EnterWriteLock();
            try
            {
                _clear();
            }
            catch
            {

            }
            finally
            {
                this.Lock.ExitWriteLock();
            }
        }

        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                this.Lock.EnterWriteLock();
                try
                {
                    _clear();
                }
                catch
                {

                }
                finally
                {
                    this.Lock.ExitWriteLock();
                }
            });
        }

        public void Clear()
        {
            // 2020/6/22 放入独立的 Task 避免出现死锁
            Task.Run(() =>
            {
                this.Lock.EnterWriteLock();
                try
                {
                    _clear();
                }
                catch
                {

                }
                finally
                {
                    this.Lock.ExitWriteLock();
                }
            });

#if LOG
            LibraryChannelManager.Log?.Debug($"{this.Name} channels Clear() completed. IdleCount={this.Channels.IdleCount}, UsedCount={this.Channels.UsedCount}");
#endif
        }

        void _clear()
        {
            this.Channels.Close((channel) =>
            {
                ManagerBase<T>.EndChannel(channel);
            });
        }

        public void TriggerSetError(object sender, SetErrorEventArgs e)
        {
            if (SetError != null)
                SetError(sender, e);
        }

        public delegate_action _new_action = null;

        // TODO: 遇到 Server 连接失败情况，下次循环处理的等待时间要变长。也就是说只有 ListTags() API 成功情况下才能马上立即重新请求
        // 启动后台任务。
        // patameters:
        //      new_action  GetChannel() 遇到需要 new channel 时候调用的回调函数
        //      skip_action 循环中设置的一个检查点，若回调函数返回 true 则 continue
        //      loop_action 循环中每一轮需要用 channel 做的事情
        public void Start(
            delegate_action new_action,
            delegate_skip skip_func,
            delegate_action loop_action,
            CancellationToken token)
        {
            _new_action = new_action;

            _task = Task.Run(async () =>
            {
                TimeSpan wait_time = this.ShortWaitTime;
                while (token.IsCancellationRequested == false)
                {
                    // TODO: 中间进行配置的时候，确保暂停在这个位置
                    // 延时
                    try
                    {
                        await Task.Delay(// TimeSpan.FromMilliseconds(1000), 
                            wait_time,
                            token);
                    }
                    catch
                    {
                        return;
                    }

                    // Loop?.Invoke(this, new EventArgs());
                    if (skip_func == null)
                    {
                        if (string.IsNullOrEmpty(this.Url))
                        {
                            SetError?.Invoke(null,
                                new SetErrorEventArgs
                                {
                                    Error = null
                                });
                            continue;
                        }
                    }
                    else
                    {
                        // 2021/1/27
                        if (string.IsNullOrEmpty(this.Url))
                        {
                            SetError?.Invoke(null,
                                new SetErrorEventArgs
                                {
                                    Error = null
                                });
                            continue;
                        }

                        if (skip_func?.Invoke() == true)
                        {
                            /*
                            // 2021/11/1
                            await Task.Delay(this.LongWaitTime,
    token);
                            */
                            continue;
                        }
                    }

                    //if (this.State == "pause")
                    //    continue;

                    if (loop_action == null)
                        continue;

                    bool error = false;
                    this.Lock.EnterReadLock();  // 锁定范围以外，可以对通道进行 Clear()
                    try
                    {
                        try
                        {
                            var channel = GetChannel();
                            try
                            {
                                loop_action(channel, "");
                                wait_time = this.ShortWaitTime;
                            }
                            finally
                            {
                                ReturnChannel(channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            error = true;
                            wait_time = this.LongWaitTime;
                            SetError?.Invoke(ex,
                                new SetErrorEventArgs
                                {
                                    Error = $"{this.Name} 出现异常: {ExceptionUtil.GetAutoText(ex)}"
                                });
                        }
                    }
                    finally
                    {
                        this.Lock.ExitReadLock();
                    }

                    // 出过错以后就要清理通道集合
                    if (error)
                    {
                        // this.Clear();
                        _ = this.ClearAsync();
                    }
                }

                // App.CurrentApp.Speak("退出后台循环");
            });
        }

        public delegate /*Task*/void delegate_action(BaseChannel<T> channel, string style);
        public delegate bool delegate_skip();

        // parameters:
        //      style   (2021/3/29 增加)
        // Exception: 
        //      可能会抛出 Exception 异常
        public BaseChannel<T> GetChannel(string style = "")
        {
            // TODO: 这里需要一个特殊的异常类型
            if (string.IsNullOrEmpty(this.Url))
                throw new UrlEmptyException($"尚未配置 {this.Name} URL");

            return this.Channels.GetChannel(() =>
            {
#if LOG
                LibraryChannelManager.Log?.Debug($"beginof new {this.Name} channel, Url={this.Url}");
#endif
                var channel = StartChannel(
    this.Url,
    out string strError);
                if (channel == null)
                    throw new Exception(strError);
                try
                {
#if NO
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                    channel.Started = true;

                    channel.Object.EnableSendKey(false);
#endif
                    _new_action?.Invoke(channel, style);
                }
                catch (Exception ex)
                {
                    if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                        throw new NotResponseException($"启动 {this.Name} 通道时出错: “{this.Name}”({this.Url})没有响应", ex);
                    else
                        throw new Exception($"启动 {this.Name} 通道时出错(2): {ex.Message}", ex);
                }
#if LOG
                LibraryChannelManager.Log?.Debug($"endof new {this.Name} channel, Url={this.Url}");
#endif
                return channel;
            });
        }

        public BaseChannel<T> GetChannel(
            out string strError)
        {
            strError = "";

            try
            {
                return GetChannel();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }
        }

        public void ReturnChannel(BaseChannel<T> channel)
        {
            this.Channels.ReturnChannel(channel);
        }

        public static BaseChannel<T> StartChannel(
    string strUrl,
    out string strError)
        {
            strError = "";

            BaseChannel<T> result = new BaseChannel<T>();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (T)Activator.GetObject(typeof(T),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            catch (Exception ex)
            {
                strError = "StartChannel() 出现异常: " + ex.Message;
                return null;
            }
            finally
            {
                if (bDone == false)
                    EndChannel(result);
            }
        }

        public static void EndChannel(BaseChannel<T> channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }


    }

#if REMOVED
    public class BaseChannel<T>
    {
        public IpcClientChannel Channel { get; set; }
        public T Object { get; set; }
        // 通道已经成功启动。意思是已经至少经过一个 API 调用并表现正常
        public bool Started { get; set; }
    }
#endif

    // 验证异常
    public class UrlEmptyException : Exception
    {

        public UrlEmptyException(string s)
            : base(s)
        {
        }

    }

    public class NotResponseException : Exception
    {
        public static string GetErrorCode(Exception ex)
        {
            if (ex == null)
                return "";
            string error_code = ex.GetType().ToString();
            if (NotResponseException.IsNotResponse(ex))
                error_code = "notResponse";
            return error_code;
        }

        public static bool IsNotResponse(Exception ex)
        {
            if (ex == null)
                return false;

            if (_isNotResponse(ex))
                return true;
            if (ex.InnerException != null && _isNotResponse(ex.InnerException))
                return true;
            return false;
        }

        static bool _isNotResponse(Exception ex)
        {
            if (ex is NotResponseException)
                return true;

            return (ex is RemotingException && (uint)ex.HResult == 0x8013150b);
        }

        public NotResponseException(string s)
            : base(s)
        {
        }

        public NotResponseException(string s, Exception inner)
    : base(s, inner)
        {
        }
    }

    public delegate void SetErrorEventHandler(object sender,
SetErrorEventArgs e);

    /// <summary>
    /// 设置出错信息事件的参数
    /// </summary>
    public class SetErrorEventArgs : EventArgs
    {
        public string Error { get; set; }
    }
}
