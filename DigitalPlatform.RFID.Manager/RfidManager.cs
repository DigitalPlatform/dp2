using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;

namespace DigitalPlatform.RFID
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

        // 每次 ListTags() 请求所用到的 reader_name 参数值

        static string _readerNameList = "*";
        public static string ReaderNameList
        {
            get
            {
                return _readerNameList;
            }
            set
            {
                _readerNameList = value;
            }
        }

        // 附加的读卡器名字列表
        static string _base2ReaderNameList = "*";
        public static string Base2ReaderNameList
        {
            get
            {
                return _base2ReaderNameList;
            }
            set
            {
                _base2ReaderNameList = value;
            }
        }

        /*
        static string _antennaList = null;

        public static string AntennaList
        {
            get
            {
                return _antennaList;
            }
            set
            {
                _antennaList = value;
            }
        }
        */

        public static event ListLocksEventHandler ListLocks = null;

        public static event ListTagsEventHandler ListTags = null;

        public static ManagerBase<IRfid> Base = new ManagerBase<IRfid>();

        public static ManagerBase<IRfid> Base2 = null;

        public static event SetErrorEventHandler SetError
        {
            add
            {
                Base.AddSetErrorEvent(value);
                Base2?.AddSetErrorEvent(value);
            }
            remove
            {
                Base.RemoveSetErrorEvent(value);
                Base2?.RemoveSetErrorEvent(value);
            }
        }

        public static void Clear()
        {
            Base.Clear();
            Base2?.Clear();
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
                if (Base2 != null)
                    Base2.Url = value;
            }
        }

        public static void TriggerSetError(object sender, SetErrorEventArgs e)
        {
            Base.TriggerSetError(sender, e);
            Base2?.TriggerSetError(sender, e);
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

        static bool _checkState = true;

        /*
        public static string LockName = null;   // "*";
        public static string LockIndices = null; // "0,1,2,3";
        */
        public static string LockCommands = null;

        static bool _lockReady = false;

        // 门锁状态就绪
        public static bool LockReady
        {
            get
            {
                return _lockReady;
            }
        }

        public static void EnableBase2()
        {
            if (Base2 == null)
                Base2 = new ManagerBase<IRfid>();

            Base2.Name = Base.Name;
        }

        static object _syncRoot = new object();

        // 启动附加的监控线程
        public static void StartBase2(
    CancellationToken token)
        {
            Base2.ShortWaitTime = TimeSpan.FromMilliseconds(10);
            Base2.LongWaitTime = TimeSpan.FromMilliseconds(2000);
            Base2.Start((channel) =>
            {
                // TODO: 看调用栈，如果是上层 GetState() 调用，就要免去检查 State 这一步
                if (_checkState)
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }
                channel.Started = true;

                channel.Object.EnableSendKey(false);
            },
            () =>
            {
                if (_pause == true)
                {
                    Base2.TriggerSetError(null,
new SetErrorEventArgs
{
    Error = "RFID 功能已暂停"
});
                    return true;
                }

                if (string.IsNullOrEmpty(Base2.Url))
                {
                    Base2.TriggerSetError(null,
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
                string style = $"session:{Base2.GetHashCode()}";
                /*
                if (string.IsNullOrEmpty(_antennaList) == false)
                    style += ",antenna:" + _antennaList;
                    */

                // 从 readerNameList 中只取出属于 base2 的部分，用来发出请求

                var readerNameList = _base2ReaderNameList;
                if (string.IsNullOrEmpty(readerNameList))
                    return; // TODO: 是否需要特意延迟一下?
                var result = channel?.Object?.ListTags(readerNameList, style);
                if (result.Value == -1)
                    Base2.TriggerSetError(result,
                        new SetErrorEventArgs { Error = result.ErrorInfo });
                else
                    Base2.TriggerSetError(result,
                        new SetErrorEventArgs { Error = null }); // 清除以前的报错

                // TODO: 触发 ListTags 事件时要加锁
                lock (_syncRoot)
                {
                    if (ListTags != null)
                    {
                        // 先记忆
                        _lastTags = result.Results;

                        // 注意 result.Value == -1 时也会触发这个事件
                        ListTags(channel, new ListTagsEventArgs
                        {
                            ReaderNameList = readerNameList,
                            Result = result
                        });
                    }
                    else
                        _lastTags = null;
                }
                // base2 这里就不再具有门锁功能
            },
            token);
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
                // TODO: 看调用栈，如果是上层 GetState() 调用，就要免去检查 State 这一步
                if (_checkState)
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }
                channel.Started = true;

                channel.Object.EnableSendKey(false);
            },
            () =>
            {
                if (_pause == true)
                {
                    Base.TriggerSetError(null,
new SetErrorEventArgs
{
    Error = "RFID 功能已暂停"
});
                    return true;
                }

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
                string style = $"session:{Base.GetHashCode()}";
                /*
                if (string.IsNullOrEmpty(_antennaList) == false)
                    style += ",antenna:" + _antennaList;
                    */

                var readerNameList = _readerNameList;
                if (string.IsNullOrEmpty(readerNameList) == false)
                {

                    var result = channel?.Object?.ListTags(readerNameList, style);
                    if (result.Value == -1)
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    lock (_syncRoot)
                    {
                        if (ListTags != null)
                        {
                            // 先记忆
                            _lastTags = result.Results;

                            // 注意 result.Value == -1 时也会触发这个事件
                            ListTags(channel, new ListTagsEventArgs
                            {
                                ReaderNameList = readerNameList,
                                Result = result
                            });
                        }
                        else
                            _lastTags = null;
                    }

                }

                // 检查门状态
                if (LockCommands != null)
                {
                    List<GetLockStateResult> errors = new List<GetLockStateResult>();
                    List<LockState> states = new List<LockState>();
                    {
                        // parameters:
                        //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
                        //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
                        //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
                        //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
                        var lock_result = channel?.Object?.GetShelfLockState(LockCommands);
                        if (lock_result.Value == -1)
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = lock_result.ErrorInfo });
                        else
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                        if (lock_result.Value == -1)
                        {
                            // 注意 lock_result.Value == -1 时也会触发这个事件
                            ListLocks?.Invoke(channel, new ListLocksEventArgs
                            {
                                Result = lock_result
                            });
                        }
                        if (lock_result.States != null)
                            states.AddRange(lock_result.States);
                    }

                    if (states.Count > 0)
                    {
                        // 注意 lock_result.Value == -1 时也会触发这个事件
                        ListLocks?.Invoke(channel, new ListLocksEventArgs
                        {
                            Result = new GetLockStateResult { States = states }
                        });
                    }
                    // 门锁状态就绪
                    _lockReady = true;
                }
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

        public static GetTagInfoResult GetTagInfo(string reader_name,
            string uid,
            uint antenna_id)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetTagInfo(reader_name, uid, antenna_id);
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
                return new GetTagInfoResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static NormalResult WriteTagInfo(string reader_name,
            TagInfo oldTagInfo,
            TagInfo newTagInfo)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.WriteTagInfo(reader_name, oldTagInfo, newTagInfo);
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

        public static NormalResult SetEAS(string reader_name,
            string tag_name,
            uint antenna_id,
            bool enable)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.SetEAS(reader_name, tag_name, antenna_id, enable);
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

        public static NormalResult SetEAS(string uid, uint antenna_id, bool enable)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.SetEAS("*", $"uid:{uid}", antenna_id, enable);
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
                        Error = NotResponseException.IsNotResponse(ex) ? $"RFID 中心({Base.Url})没有响应"
                        : $"RFID 中心出现异常: {ExceptionUtil.GetExceptionText(ex)}"
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

                BaseChannel<IRfid> channel = null;
                bool old_checkState = _checkState;
                _checkState = false;
                try
                {
                    channel = Base.GetChannel();
                }
                finally
                {
                    _checkState = old_checkState;
                }

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
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        public static NormalResult ClearCache()
        {
            try
            {
                // 因为 GetState 是可有可无的请求，如果 Url 为空就算了
                if (string.IsNullOrEmpty(Base.Url))
                    return new NormalResult();

                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.GetState($"clearCache:{Base.GetHashCode()}");
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
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = NotResponseException.GetErrorCode(ex)
                };
            }
        }

        public static NormalResult ManageReader(string reader_name_list, string command)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.ManageReader(reader_name_list, command);
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


        // 打开门锁
        public static NormalResult OpenShelfLock(string lockName)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.OpenShelfLock(lockName);
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

        // 开关灯
        public static NormalResult TurnShelfLamp(string lampName, string action)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.TurnShelfLamp(lampName, action);
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

        public static NormalResult SelectAntenna(string reader_name,
            uint antenna_id)
        {
            return GetTagInfo(reader_name, "00000000", antenna_id);
        }

        public static ListTagsResult CallListTags(string reader_name, string style)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.ListTags(reader_name, style);
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
                return new ListTagsResult { Value = -1, ErrorInfo = ex.Message };
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
        // 本次 ListTags 请求所用的 reader_name 参数值
        public string ReaderNameList { get; set; }

        public ListTagsResult Result { get; set; }
    }

    public delegate void ListLocksEventHandler(object sender,
ListLocksEventArgs e);

    /// <summary>
    ///列出门锁状态事件的参数
    /// </summary>
    public class ListLocksEventArgs : EventArgs
    {
        public GetLockStateResult Result { get; set; }
    }


    public class LockCommand
    {
        public string LockName { get; set; }
        public string Indices { get; set; } // 0,1,2,3
    }
}
