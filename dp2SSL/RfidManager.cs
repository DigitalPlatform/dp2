using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;

namespace dp2SSL
{
    public static class RfidManager
    {
        public static event ListTagsEventHandler ListTags = null;

        // static ChannelPool<RfidChannel> _rfidChannels = new ChannelPool<RfidChannel>();

        public static ManagerBase<IRfid> Base = new ManagerBase<IRfid>();

        public static event SetErrorEventHandler SetError
        {
            add
            {
                Base.AddEvent(value);
            }
            remove
            {
                Base.RemoveEvent(value);
            }
        }

        public static void Clear()
        {
            Base.Clear();
        }

        public static string Url
        {
            get
            {
                return Base.Url;
            }
            set
            {
                Base.Url = value;
            }
        }

        // 启动后台任务。
        // 后台任务负责监视 RFID 中心的标签
        public static void Start(
            CancellationToken token)
        {
            Base.Start((channel) =>
            {
                var result = channel.Object.GetState("");
                if (result.Value == -1)
                    throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                channel.Started = true;

                channel.Object.EnableSendKey(false);
            },

            (channel) =>
            {
                var result = channel?.Object?.ListTags("*",
null);
                if (result.Value == -1)
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = result.ErrorInfo });
                else
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = null }); // 清除以前的报错

                if (ListTags != null)
                {
                    ListTags(channel, new ListTagsEventArgs
                    {
                        Result = result
                    });
                }
            },
            token);

#if NO
            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    // TODO: 中间进行配置的时候，确保暂停在这个位置
                    // 延时
                    Task.Delay(TimeSpan.FromMilliseconds(1000), token);

                    if (string.IsNullOrEmpty(App.RfidUrl))
                        continue;

                    Base.Lock.EnterReadLock();  // 锁定范围以外，可以对通道进行 Clear()
                    try
                    {
                        // 列举标签
                        try
                        {
                            var channel = GetRfidChannel();
                            try
                            {
                                var result = channel?.Object?.ListTags("*",
        null);
                                if (result.Value == -1)
                                    SetError(result,
                                        new SetErrorEventArgs { Error = result.ErrorInfo });
                                else
                                    SetError(result,
                                        new SetErrorEventArgs { Error = null }); // 清除以前的报错

                                if (ListTags != null)
                                {
                                    ListTags(channel, new ListTagsEventArgs
                                    {
                                        Result = result
                                    });
                                }
                            }
                            finally
                            {
                                ReturnRfidChannel(channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            SetError(ex,
                                new SetErrorEventArgs
                                {
                                    Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                                });

                            // 附加一个延时
                            // Task.Delay(TimeSpan.FromMinutes(1), token);
                        }
                    }
                    finally
                    {
                        Base.Lock.ExitReadLock();
                    }
                }
            });
#endif
        }

        public static NormalResult SetEAS(string uid, bool enable)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.SetEAS("*", $"uid:{uid}", enable);
                    if (result.Value == -1)
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    return result;
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult EnableSendkey(bool enable)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.EnableSendKey(enable);
                    if (result.Value == -1)
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    return result;
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetExceptionText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

#if NO
        // Exception: 
        //      可能会抛出 Exception 异常
        public static RfidChannel GetRfidChannel()
        {
            if (string.IsNullOrEmpty(App.RfidUrl))
                throw new Exception("尚未配置 RFID 中心 URL");

            return Base.Channels.GetChannel(() =>
            {
                LibraryChannelManager.Log.Debug($"beginof new RFID channel, App.RfidUrl={App.RfidUrl}");
                var channel = ManagerBase.StartChannel(
    App.RfidUrl,
    out string strError);
                if (channel == null)
                    throw new Exception(strError);
                try
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                    channel.Started = true;

                    channel.Object.EnableSendKey(false);
                }
                catch (Exception ex)
                {
                    if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                        throw new Exception($"启动 RFID 通道时出错: “指纹中心”({App.RfidUrl})没有响应", ex);
                    else
                        throw new Exception($"启动 RFID 通道时出错(2): {ex.Message}", ex);
                }
                LibraryChannelManager.Log.Debug($"endof new RFID channel, App.RfidUrl={App.RfidUrl}");
                return channel;
            });
        }

        public static RfidChannel GetRfidChannel(out string strError)
        {
            strError = "";

            try
            {
                return GetRfidChannel();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }
        }

        public static void ReturnRfidChannel(RfidChannel channel)
        {
            Base.Channels.ReturnChannel(channel);
        }

#endif
    }

    public delegate void ListTagsEventHandler(object sender,
ListTagsEventArgs e);

    /// <summary>
    ///列出标签事件的参数
    /// </summary>
    public class ListTagsEventArgs : EventArgs
    {
        public ListTagsResult Result { get; set; }
    }
}
