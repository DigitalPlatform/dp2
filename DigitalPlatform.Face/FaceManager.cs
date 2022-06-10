using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Interfaces;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.Face
{
    /// <summary>
    /// 人脸通道集中管理
    /// </summary>
    public static class FaceManager
    {
        // static string _state = "ok";    // ok/error

        // public static event TouchedEventHandler Touched = null;

        public static ManagerBase<IBioRecognition> Base = new ManagerBase<IBioRecognition>();

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
        // 后台任务负责监视 人脸中心 里面新到的 message
        public static void Start(
            CancellationToken token)
        {
            Base.ShortWaitTime = TimeSpan.FromSeconds(5);
            Base.LongWaitTime = TimeSpan.FromSeconds(5);

            // App.CurrentApp.Speak("启动后台线程");
            Base.Start((channel, style) =>
            {
                if (StringUtil.IsInList("skip_check_state", style) == false)
                {
                    var result = channel.Object.GetState("camera");
                    if (result.Value == -1)
                        throw new Exception($"人脸中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }

                channel.Started = true;

                channel.Object.EnableSendKey(false);
                //return null;
            },
            null,
            (channel, loop_style) =>
            {
                var result = channel.Object.GetState("camera");
                if (result.Value == -1)
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = result.ErrorInfo });
                else
                    Base.TriggerSetError(result,
                        new SetErrorEventArgs { Error = null }); // 清除以前的报错
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

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
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
                        ? $"人脸中心({Base.Url})没有响应"
                        : $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult GetState(string style)
        {
            try
            {
                // 因为 GetState 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel("skip_check_state");
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        public static RecognitionFaceResult RecognitionFace(string style, bool setGlobalError = false)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new RecognitionFaceResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.RecognitionFace(style);
                    if (setGlobalError)
                    {
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                    }
                    return result;
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }

            }
            catch (Exception ex)
            {
                if (setGlobalError)
                    Base.TriggerSetError(ex,
                        new SetErrorEventArgs
                        {
                            Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                        });
                return new RecognitionFaceResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static GetImageResult GetImage(string style)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new RecognitionFaceResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetImageResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult CancelRecognitionFace()
        {
            try
            {
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.CancelRecognitionFace();
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static GetFeatureStringResult GetFeatureString(byte[] imageData,
            string strExcludeBarcodes,
            string strStyle,
            bool setGlobalError = false)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new GetFeatureStringResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetFeatureString(imageData,
                        strExcludeBarcodes,
                        strStyle);
                    if (setGlobalError)
                    {
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                    }
                    return result;
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }

            }
            catch (Exception ex)
            {
                if (setGlobalError)
                    Base.TriggerSetError(ex,
                        new SetErrorEventArgs
                        {
                            Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                        });
                return new GetFeatureStringResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult CancelGetFeatureString()
        {
            try
            {
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.CancelGetFeatureString();
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult Notify(string event_name)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new NormalResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.Notify(event_name);
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult RegisterFeatureString(byte[] imageData,
    string strBarcode,
    string strStyle,
    bool setGlobalError = false)
        {
            try
            {
                //if (string.IsNullOrEmpty(Base.Url))
                //    return new GetFeatureStringResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.RegisterFeatureString(imageData,
                        strBarcode,
                        strStyle);
                    if (setGlobalError)
                    {
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                    }
                    return result;
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }

            }
            catch (Exception ex)
            {
                if (setGlobalError)
                    Base.TriggerSetError(ex,
                        new SetErrorEventArgs
                        {
                            Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                        });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult CancelRegisterFeatureString()
        {
            try
            {
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IBioRecognition> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.CancelRegisterFeatureString();
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
                        Error = $"人脸中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

    }

}
