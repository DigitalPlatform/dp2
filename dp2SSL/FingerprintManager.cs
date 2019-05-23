using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.Interfaces;

namespace dp2SSL
{
    /// <summary>
    /// 指纹通道集中管理
    /// </summary>
    public static class FingerprintManager
    {
        public static event TouchedEventHandler Touched = null;

        public static event SetErrorEventHandler SetError = null;

        static ChannelPool<FingerprintChannel> _fingerprintChannels = new ChannelPool<FingerprintChannel>();

        // static CancellationTokenSource _cancel = new CancellationTokenSource();

        // public delegate void delegate_setError(string error);

        // 启动后台任务。
        // 后台任务负责监视 指纹中心 里面新到的 message
        public static void Start(
            CancellationToken token)
        {
            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    // 延时
                    Task.Delay(TimeSpan.FromMilliseconds(1000), token);

                    if (string.IsNullOrEmpty(App.FingerprintUrl))
                        continue;

                    // 获得消息
                    try
                    {
                        FingerprintChannel channel = GetFingerprintChannel();
                        try
                        {
                            var result = channel.Object.GetMessage("");
                            if (result.Value == -1)
                                SetError(result,
                                    new SetErrorEventArgs { Error = result.ErrorInfo });
                            else
                                SetError(result,
                                    new SetErrorEventArgs { Error = null }); // 清除以前的报错

                            if (result.Value != -1 && result.Message == null)
                            {
                                // 没有消息的时候不用惊扰事件订阅者
                            }
                            else
                            {
                                Touched(result, new TouchedEventArgs
                                {
                                    Message = result.Message,
                                    ErrorOccur = result.Value == -1,
                                    Result = result
                                });
                            }
                        }
                        finally
                        {
                            ReturnFingerprintChannel(channel);
                        }

                    }
                    catch (Exception ex)
                    {
                        SetError(ex,
                            new SetErrorEventArgs
                            {
                                Error = $"指纹中心出现异常: {ex.Message}"
                            });

                        // 附加一个延时
                        // Task.Delay(TimeSpan.FromMinutes(1), token);
                    }
                }
            });
        }

        public static NormalResult EnableSendkey(bool enable)
        {
            try
            {
                FingerprintChannel channel = GetFingerprintChannel();
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
                    ReturnFingerprintChannel(channel);
                }

            }
            catch (Exception ex)
            {
                SetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"指纹中心出现异常: {ex.Message}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message};
            }
        }

        // Exception: 
        //      可能会抛出 Exception 异常
        public static FingerprintChannel GetFingerprintChannel()
        {
            if (string.IsNullOrEmpty(App.FingerprintUrl))
                throw new Exception("尚未配置 指纹中心 URL");

            return _fingerprintChannels.GetChannel(() =>
            {
                var channel = FingerPrint.StartFingerprintChannel(
    App.FingerprintUrl,
    out string strError);
                if (channel == null)
                    throw new Exception(strError);
                try
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"指纹中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                    channel.Started = true;

                    channel.Object.EnableSendKey(false);
                }
                catch (Exception ex)
                {
                    if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                        throw new Exception($"启动指纹通道时出错: “指纹中心”({App.FingerprintUrl})没有响应", ex);
                    else
                        throw new Exception($"启动指纹通道时出错(2): {ex.Message}", ex);
                }
                return channel;
            });
        }

        public static FingerprintChannel GetFingerprintChannel(out string strError)
        {
            strError = "";

            try
            {
                return GetFingerprintChannel();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }
        }

        public static void ReturnFingerprintChannel(FingerprintChannel channel)
        {
            _fingerprintChannels.ReturnChannel(channel);
        }

    }

    public delegate void TouchedEventHandler(object sender,
TouchedEventArgs e);

    /// <summary>
    /// 触摸事件的参数
    /// </summary>
    public class TouchedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool ErrorOccur { get; set; }

        public GetMessageResult Result { get; set; }
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
