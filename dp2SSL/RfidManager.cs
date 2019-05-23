using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.RFID;

namespace dp2SSL
{
    public static class RfidManager
    {
        public static event ListTagsEventHandler ListTags = null;

        public static event SetErrorEventHandler SetError = null;

        static ChannelPool<RfidChannel> _rfidChannels = new ChannelPool<RfidChannel>();

        // 启动后台任务。
        // 后台任务负责监视 RFID 中心的标签
        public static void Start(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    // 延时
                    Task.Delay(TimeSpan.FromMilliseconds(1000), token);

                    if (string.IsNullOrEmpty(App.FingerprintUrl))
                        continue;

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

                            ListTags(channel, new ListTagsEventArgs
                            {
                                Result = result
                            });
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
                                Error = $"RFID 中心出现异常: {ex.Message}"
                            });

                        // 附加一个延时
                        // Task.Delay(TimeSpan.FromMinutes(1), token);
                    }
                }
            });
        }

        public static NormalResult SetEAS(string uid, bool enable)
        {
            try
            {
                RfidChannel channel = GetRfidChannel();
                try
                {
                    var result = channel.Object.SetEAS("*", $"uid:{uid}", enable);
                    if (result.Value == -1)
                        SetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        SetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    return result;
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
                        Error = $"RFID 中心出现异常: {ex.Message}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult EnableSendkey(bool enable)
        {
            try
            {
                RfidChannel channel = GetRfidChannel();
                try
                {
                    var result = channel.Object.EnableSendKey(enable);
                    if (result.Value == -1)
                        SetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        SetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    return result;
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
                        Error = $"RFID 中心出现异常: {ex.Message}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // Exception: 
        //      可能会抛出 Exception 异常
        public static RfidChannel GetRfidChannel()
        {
            if (string.IsNullOrEmpty(App.RfidUrl))
                throw new Exception("尚未配置 RFID 中心 URL");

            return _rfidChannels.GetChannel(() =>
            {
                var channel = RFID.StartRfidChannel(
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
                        throw new Exception($"启动 RFID 通道时出错: “指纹中心”({App.FingerprintUrl})没有响应", ex);
                    else
                        throw new Exception($"启动 RFID 通道时出错(2): {ex.Message}", ex);
                }
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
            _rfidChannels.ReturnChannel(channel);
        }

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
