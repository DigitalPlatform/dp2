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
using DigitalPlatform.LibraryClient;

namespace dp2SSL
{
    // 一些公共的成员
    // T: FingerprintChannel; T1: IFingerprint
    public class ManagerBase<T>
    {
        event SetErrorEventHandler SetError = null;

        public void AddEvent(SetErrorEventHandler handler)
        {
            SetError += handler;
        }

        public void RemoveEvent(SetErrorEventHandler handler)
        {
            SetError -= handler;
        }

        public ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public ChannelPool<BaseChannel<T>> Channels = new ChannelPool<BaseChannel<T>>();

        public string Url { get; set; }

        public string Name { get; set; }

        public void Clear()
        {
            this.Lock.EnterWriteLock();
            try
            {
                _clear();
            }
            finally
            {
                this.Lock.ExitWriteLock();
            }

            LibraryChannelManager.Log.Debug($"{this.Name} channels Clear() completed. IdleCount={this.Channels.IdleCount}, UsedCount={this.Channels.UsedCount}");
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

        // 启动后台任务。
        public void Start(
            delegate_action new_action,
            delegate_action loop_action,
            CancellationToken token)
        {
            _new_action = new_action;

            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    // TODO: 中间进行配置的时候，确保暂停在这个位置
                    // 延时
                    Task.Delay(TimeSpan.FromMilliseconds(1000), token);

                    if (string.IsNullOrEmpty(this.Url))
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
                                loop_action(channel);
                            }
                            finally
                            {
                                ReturnChannel(channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            error = true;
                            SetError(ex,
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
                        this.Clear();
                }

                App.CurrentApp.Speak("退出后台循环");
            });
        }

        public delegate void delegate_action(BaseChannel<T> channel);

        // Exception: 
        //      可能会抛出 Exception 异常
        public BaseChannel<T> GetChannel()
        {
            // TODO: 这里需要一个特殊的异常类型
            if (string.IsNullOrEmpty(this.Url))
                throw new UrlEmptyException($"尚未配置 {this.Name} URL");

            return this.Channels.GetChannel(() =>
            {
                LibraryChannelManager.Log.Debug($"beginof new {this.Name} channel, Url={this.Url}");
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
                    _new_action?.Invoke(channel);
                }
                catch (Exception ex)
                {
                    if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                        throw new Exception($"启动 {this.Name} 通道时出错: “{this.Name}”({this.Url})没有响应", ex);
                    else
                        throw new Exception($"启动 {this.Name} 通道时出错(2): {ex.Message}", ex);
                }
                LibraryChannelManager.Log.Debug($"endof new {this.Name} channel, Url={this.Url}");
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

    public class BaseChannel<T>
    {
        public IpcClientChannel Channel { get; set; }
        public T Object { get; set; }
        // 通道已经成功启动。意思是已经至少经过一个 API 调用并表现正常
        public bool Started { get; set; }
    }

    // 验证异常
    public class UrlEmptyException : Exception
    {

        public UrlEmptyException(string s)
            : base(s)
        {
        }

    }

}
