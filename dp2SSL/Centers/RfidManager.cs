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
        static List<OneTag> _lastTags = null;

        // 以前获取过的 Tag 列表。当第一次添加 ListTags 事件时，要考虑立即处理一次这些以前的 Tag 列表
        public static List<OneTag> LastTags
        {
            get
            {
                return _lastTags;
            }
        }

        public static event ListTagsEventHandler ListTags = null;

        public static ManagerBase<IRfid> Base = new ManagerBase<IRfid>();

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

        public static void TriggerSetError(object sender, SetErrorEventArgs e)
        {
            Base.TriggerSetError(sender, e);
        }

        // 启动后台任务。
        // 后台任务负责监视 RFID 中心的标签
        public static void Start(
            CancellationToken token)
        {
            Base.ShortWaitTime = TimeSpan.FromMilliseconds(10);
            Base.LongWaitTime = TimeSpan.FromMilliseconds(2000);
            Base.Start((channel) =>
            {
                var result = channel.Object.GetState("");
                if (result.Value == -1)
                    throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                channel.Started = true;

                channel.Object.EnableSendKey(false);
            },
            () =>
            {
                if (string.IsNullOrEmpty(Base.Url))
                {
                    Base.TriggerSetError(null,
                        new SetErrorEventArgs
                        {
                            Error = "RFID 中心 URL 尚未配置(因此无法从 RFID 读卡器读取信息)"
                        });
                    return true;
                }
                return false;
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
                    // 先记忆
                    _lastTags = result.Results;

                    // 注意 result.Value == -1 时也会触发这个事件
                    ListTags(channel, new ListTagsEventArgs
                    {
                        Result = result
                    });
                }
                else
                    _lastTags = null;
            },
            token);

        }

        public static BaseChannel<IRfid> GetChannel()
        {
            return Base.GetChannel();
        }

        public static void ReturnChannel(BaseChannel<IRfid> channel)
        {
            Base.ReturnChannel(channel);
        }

        public static void TriggerLastListTags()
        {
            if (ListTags == null)
                return;

            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    if (ListTags != null)
                    {
                        ListTags(channel, new ListTagsEventArgs
                        {
                            Result = new ListTagsResult { Results = _lastTags }
                        });
                    }
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                Base.Clear();
                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
            }
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
                Base.Clear();
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
                // 因为 EnableSendkey 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

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
                Base.Clear();
                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = IsNotResponse(ex) ? $"RFID 中心({Base.Url})没有响应"
                        : $"RFID 中心出现异常: {ExceptionUtil.GetExceptionText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
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

        public static NormalResult GetState(string style)
        {
            try
            {
                // 因为 GetState 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IRfid> channel = Base.GetChannel();
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
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
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
