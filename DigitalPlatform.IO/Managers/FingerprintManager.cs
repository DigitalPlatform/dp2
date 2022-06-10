using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Interfaces;
using DigitalPlatform.Text;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// 指纹通道集中管理
    /// </summary>
    public static class FingerprintManager
    {
        static string _state = "ok";    // ok/error

        public static event TouchedEventHandler Touched = null;

        public static ManagerBase<IFingerprint> Base = new ManagerBase<IFingerprint>();

        public static event SetErrorEventHandler SetError
        {
            add
            {
                Base.AddSetErrorEvent(value);
            }
            remove
            {
                Base.RemoveSetErrorEvent(value);
            }
        }

        public static void Clear()
        {
            // Base.Clear();
            _ = Base.ClearAsync();
        }

        public static Task ClearAsync()
        {
            return Base.ClearAsync();
        }

        // 注意有死锁的风险
        public static void ClearChannels()
        {
            Base.ClearChannels();
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

        static bool _pause = false;

        public static bool Pause
        {
            get
            {
                return _pause;
            }
            set
            {
                _pause = value;
            }
        }

        public static string SessionID { get; set; }

        // 启动后台任务。
        // 后台任务负责监视 指纹中心 里面新到的 message
        public static void Start(
            CancellationToken token)
        {
            // App.CurrentApp.Speak("启动后台线程");
            Base.Start((channel, style) =>
            {
                if (StringUtil.IsInList("skip_check_state", style) == false)
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"{Name}当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }

                channel.Started = true;

                channel.Object.EnableSendKey(false);
                //return null;
            },
            () =>
            {
                if (_pause == true)
                {
                    Base.TriggerSetError(null,
new SetErrorEventArgs
{
    Error = $"{Name}功能已暂停"
});
                    return true;
                }
                /*
                if (string.IsNullOrEmpty(Base.Url))
                {
                    Base.TriggerSetError(null,
                        new SetErrorEventArgs
                        {
                            Error = "RFID 中心 URL 尚未配置(因此无法从 RFID 读卡器读取信息)"
                        });
                    return true;
                }
                */
                return false;
            },
            (channel, loop_style) =>
            {
                string style = "wait:500"; // 2021/11/1 增加 wait 参数
                if (string.IsNullOrEmpty(SessionID) == false)
                    style += $",session:{SessionID}";
                var result = channel.Object.GetMessage(style);
                if (result.Value == -1)
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = result.ErrorInfo });
                else
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = null }); // 清除以前的报错

                if (result.Value != -1 && result.Message == null)
                {
                    // 状态转向 ok，需要补充触发一次
                    if (_state != "ok")
                    {
                        Touched?.Invoke(result, new TouchedEventArgs
                        {
                            Message = result.Message,
                            Quality = result.Quality,
                            ErrorOccur = result.Value == -1,
                            Result = result
                        });
                    }

                    // 没有消息的时候不用惊扰事件订阅者
                    _state = "ok";
                }
                else
                {
                    if (result.Value == -1)
                        _state = "error";
                    else
                        _state = "ok";

                    // 注： result.Value == -1 的时候，SetError 也触发了，Touched 也触发了。
                    // 如果应用已经挂接了 SetError 事件，建议 Touched 里面可以忽略 result.Value == -1 的情况
                    Touched?.Invoke(result, new TouchedEventArgs
                    {
                        Message = result.Message,
                        Quality = result.Quality,
                        ErrorOccur = result.Value == -1,
                        Result = result
                    });
                }
                //return null;
            },
            token);
        }

        public static NormalResult EnableSendkey(bool enable)
        {
            try
            {
                // 因为 EnableSendkey 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IFingerprint> channel = Base.GetChannel();
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
                        Error = NotResponseException.IsNotResponse(ex)
                        ? $"{Name}({Base.Url})没有响应"
                        : $"{Name}出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        static string Name
        {
            get
            {
                string result = Base.Name;
                if (string.IsNullOrEmpty(result))
                    return "指纹中心";
                return result;
            }
        }


        public static NormalResult GetState(string style)
        {
            try
            {
                // 因为 GetState 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IFingerprint> channel = Base.GetChannel("skip_check_state");
                try
                {
                    var result = channel.Object.GetState(style);
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
                        Error = $"{Name}出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        public class GetVersionResult : NormalResult
        {
            public string Version { get; set; }
            public string CfgInfo { get; set; }
        }

        public static GetVersionResult GetVersion()
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new NormalResult();

                BaseChannel<IFingerprint> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetVersion(out string version,
                        out string cfg_info,
                        out string strError);
                    if (result == -1)
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = strError });
                    else
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    return new GetVersionResult
                    {
                        Version = version,
                        CfgInfo = cfg_info,
                        Value = result,
                        ErrorInfo = strError
                    };
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
                        Error = $"{Name}出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetVersionResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        // 清除以前积累未读的全部消息
        public static GetMessageResult ClearMessage()
        {
            return GetMessage("clear");
        }

        public static GetMessageResult GetMessage(string style)
        {
            try
            {
                // 因为 GetMessage 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new GetMessageResult();

                BaseChannel<IFingerprint> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetMessage(style);
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
                        Error = $"{Name}出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetMessageResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        // 2021/10/29
        public static GetImageResult GetImage(string style)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new GetImageResult();

                BaseChannel<IFingerprint> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetImage(style);
                    // TODO: 是否可以考虑不显示错误信息
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
                        Error = $"{Name}出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetImageResult { Value = -1, ErrorInfo = ex.Message };
            }
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
        public int Quality { get; set; } // 2019/9/4 指纹图象质量，100 为满分
        public bool ErrorOccur { get; set; }

        public GetMessageResult Result { get; set; }

        public override string ToString()
        {
            return $"Message={Message},Quality={Quality},ErrorOccur={ErrorOccur},Result={Result.ToString()}";
        }
    }
}
