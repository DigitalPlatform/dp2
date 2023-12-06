#define SYNC_ROOT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace DigitalPlatform.RFID
{
    public static class RfidManager
    {
        // 是否要在 Inventory 阶段获得 UHF 标签的 RSSI
        public static bool GetRSSI { get; set; }

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

        // 探测门锁状态使用哪个线程？
        // 空表示使用主要线程，"base2" 表示使用 Base2 线程
        static string _lockThread = "";
        public static string LockThread
        {
            get
            {
                return _lockThread;
            }
            set
            {
                _lockThread = value;
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
            //Base.Clear();
            _ = Base.ClearAsync();

            // Base2?.Clear();
            _ = Base2?.ClearAsync();
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

        // 注意，Pause 不负责 Base2。Pause2 才是负责 Base2 的暂停的
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

        // Pause2 才是负责 Base2 的暂停的
        static bool _pause2 = false;

        public static bool Pause2
        {
            get
            {
                return _pause2;
            }
            set
            {
                _pause2 = value;
            }
        }

        // 已经移动到 Base 中
        // static bool _checkState = true;

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

        static bool _tagsReady = false;

        // 线程1的标签准备就绪
        public static bool TagsReady
        {
            get
            {
                return _tagsReady;
            }
            set
            {
                _tagsReady = value;
            }
        }

        static bool _base2TagsReady = false;

        // 线程1的标签准备就绪
        public static bool Base2TagsReady
        {
            get
            {
                return _base2TagsReady;
            }
            set
            {
                _base2TagsReady = value;
            }
        }

        public static void EnableBase2()
        {
            if (Base2 == null)
                Base2 = new ManagerBase<IRfid>();

            Base2.Name = Base.Name;
        }

#if SYNC_ROOT

        static object _syncRoot = new object();

        public static object SyncRoot
        {
            get
            {
                return _syncRoot;
            }
        }

#endif

        // static AsyncSemaphore _limit = new AsyncSemaphore(1);

        // 启动附加的监控线程
        public static void StartBase2(
    CancellationToken token)
        {
            Base2.ShortWaitTime = TimeSpan.FromMilliseconds(10);
            Base2.LongWaitTime = TimeSpan.FromMilliseconds(2000);
            Base2.Start((channel, style) =>
            {
                // 看调用栈，如果是上层 GetState() 调用，就要免去检查 State 这一步
                if (StringUtil.IsInList("skip_check_state", style) == false)
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }
                channel.Started = true;

                channel.Object.EnableSendKey(false);

                //return null;
            },
            () =>
            {
                if (_pause2 == true)
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
            (channel, loop_style) =>
            {
                /*
                if (string.IsNullOrEmpty(_antennaList) == false)
                    style += ",antenna:" + _antennaList;
                    */

                // 从 readerNameList 中只取出属于 base2 的部分，用来发出请求

                GetLockStateResult lock_result = null;
                var readerNameList = _base2ReaderNameList;
                if (string.IsNullOrEmpty(readerNameList) == false
                || (_lockThread == "base2"
                        && string.IsNullOrEmpty(LockCommands) == false))
                {
                    string style = $"session:{Base2.GetHashCode()}";

                    if (GetRSSI)
                        style += ",rssi";

                    // 2019/12/6
                    if (_lockThread == "base2"
                        && string.IsNullOrEmpty(LockCommands) == false)
                        style += ",getLockState:" + StringUtil.EscapeString(LockCommands, ":,");

                    var result = channel?.Object?.ListTags(readerNameList, style);
                    if (result.Value == -1)
                        Base2.TriggerSetError(result,
                            new SetErrorEventArgs { Error = result.ErrorInfo });
                    else
                        Base2.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    lock_result = result.GetLockStateResult;

                    IncLockHeartbeat();

                    // 触发 ListTags 事件时要加锁
                    if (string.IsNullOrEmpty(readerNameList) == false)
                    {
#if SYNC_ROOT
                        lock (_syncRoot)
#endif
                        // using(var releaser = await _limit.EnterAsync().ConfigureAwait(false))
                        {
                            if (ListTags != null)
                            {
                                // 先记忆
                                _lastTags = result.Results;

                                // 注意 result.Value == -1 时也会触发这个事件
                                ListTags(channel, new ListTagsEventArgs
                                {
                                    ReaderNameList = readerNameList,
                                    Result = result,
                                    Source = "base2",
                                });
                            }
                            else
                                _lastTags = null;

                            _base2TagsReady = true;
                        }
                    }
                }

                // base2 这里负责探索门锁状态
                // 检查门状态
                if (_lockThread == "base2"
                && LockCommands != null)
                {

                    // List<LockState> states = new List<LockState>();
                    {
                        // parameters:
                        //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
                        //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
                        //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
                        //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
                        if (lock_result.Value == -1)
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = lock_result.ErrorInfo });
                        else
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                        /*
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
                        */

                        // 注意 lock_result.Value == -1 时也会触发这个事件
                        ListLocks?.Invoke(channel, new ListLocksEventArgs
                        {
                            Result = lock_result
                        });

                    }

                    /*
                    if (states.Count > 0)
                    {
                        // 注意 lock_result.Value == -1 时也会触发这个事件
                        ListLocks?.Invoke(channel, new ListLocksEventArgs
                        {
                            Result = new GetLockStateResult { States = states }
                        });
                    }
                    */

                    // 门锁状态就绪
                    _lockReady = true;
                }
            },
            token);
        }

        // 最近一次活动的时间。所谓活动，指发生了标签放入、拿走、更新的动作
        static DateTime _lastActivateTime = DateTime.MinValue;

        public static void Touch()
        {
            _lastActivateTime = DateTime.Now;
        }

#if REMOVED
        // 是否处在不活跃的阶段?
        static bool IsIdle()
        {
            // 和最近一次活动间隔 阈值 长度以上，那就算是不活跃时段
            if (DateTime.Now - _lastActivateTime > TimeSpan.FromMinutes(1))
                return true;
            return false;
        }
#endif

        // 获得当前间隔时间
        static TimeSpan GetStandardLength()
        {
            /*
            // testing
            return TimeSpan.FromMinutes(1);
            // TODO: 在系统参数对话框上设置一个控制 standart length 的参数，用于测试验证
            */

            var delta = DateTime.Now - _lastActivateTime;
            if (delta > TimeSpan.FromMinutes(2))
                return TimeSpan.FromSeconds(1); // 慢速 CPU 5%
            if (delta > TimeSpan.FromSeconds(30))
                return TimeSpan.FromMilliseconds(500);  // 快速 CPU 10%
            return TimeSpan.FromMilliseconds(100);  // 急迫 CPU 20%
        }

        // 启动后台任务。
        // 后台任务负责监视 RFID 中心的标签
        public static void Start(
            CancellationToken token)
        {
            Base.ShortWaitTime = TimeSpan.FromMilliseconds(10);
            Base.LongWaitTime = TimeSpan.FromMilliseconds(2000);
            Base.Start((channel, style) =>
            {
                // TODO: 看调用栈，如果是上层 GetState() 调用，就要免去检查 State 这一步
                if (StringUtil.IsInList("skip_check_state", style) == false)
                {
                    var result = channel.Object.GetState("");
                    if (result.Value == -1)
                        throw new Exception($"RFID 中心当前处于 {result.ErrorCode} 状态({result.ErrorInfo})");
                }
                channel.Started = true;

                channel.Object.EnableSendKey(false);

                // return null;
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
            (channel, loop_style) =>
            {
                _callLoop(channel, loop_style);
#if REMOVED
                /*
                if (string.IsNullOrEmpty(_antennaList) == false)
                    style += ",antenna:" + _antennaList;
                    */

                GetLockStateResult lock_result = null;
                var readerNameList = _readerNameList;
                if (string.IsNullOrEmpty(readerNameList) == false
                || (_lockThread != "base2" && string.IsNullOrEmpty(LockCommands) == false))
                {
                    string style = $"session:{Base.GetHashCode()}";

                    if (GetRSSI)
                        style += ",rssi";

                    // 2019/12/4
                    if (_lockThread != "base2"
                        && string.IsNullOrEmpty(LockCommands) == false)
                        style += ",getLockState:" + StringUtil.EscapeString(LockCommands, ":,");

                    // 2023/11/25
                    if (SyncSetEAS)
                        style += ",dont_delay";

                    DateTime start_time = DateTime.Now;

                    object __lockObj = _syncRoot;
                    bool __lockWasTaken = false;
                    try
                    {
                        // 外层加锁
                        if (SyncSetEAS)
                            System.Threading.Monitor.Enter(__lockObj, ref __lockWasTaken);

                        var result = channel?.Object?.ListTags(readerNameList, style);
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错

                        lock_result = result.GetLockStateResult;

                        IncLockHeartbeat();

                        if (string.IsNullOrEmpty(readerNameList) == false)
                        {
                            // using (var releaser = await _limit.EnterAsync().ConfigureAwait(false))
                            try
                            {
#if SYNC_ROOT
                                // 内层加锁
                                if (SyncSetEAS == false)
                                    System.Threading.Monitor.Enter(__lockObj, ref __lockWasTaken);
#endif

                                if (ListTags != null)
                                {
                                    // 先记忆
                                    _lastTags = result.Results;

                                    // 注意 result.Value == -1 时也会触发这个事件
                                    ListTags(channel, new ListTagsEventArgs
                                    {
                                        ReaderNameList = readerNameList,
                                        Result = result,
                                        Source = "base",
                                    });
                                }
                                else
                                    _lastTags = null;

                                _tagsReady = true;
                            }
                            finally
                            {
                                if (SyncSetEAS == false && __lockWasTaken)
                                    System.Threading.Monitor.Exit(__lockObj);

                            }
                        }

                    }
                    finally
                    {
#if SYNC_ROOT
                        if (SyncSetEAS && __lockWasTaken)
                            System.Threading.Monitor.Exit(__lockObj);
#endif
                    }

                    // 补充延时
                    if (SyncSetEAS)
                    {
                        // 标准间隔
                        TimeSpan standard_length = GetStandardLength();
                        // 上一轮盘点实际使用的时间
                        var length = DateTime.Now - start_time;
                        var delta = standard_length - length;
                        // 补足差额
                        if (delta.Milliseconds > 0)
                            Thread.Sleep(delta.Milliseconds);
                    }
                }

                // 检查门状态
                if (_lockThread != "base2"
                && lock_result != null)
                {
                    // List<LockState> states = new List<LockState>();
                    {
                        // parameters:
                        //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
                        //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
                        //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
                        //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
                        if (lock_result.Value == -1)
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = lock_result.ErrorInfo });
                        else
                            Base.TriggerSetError(lock_result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                        /*
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
                        */
                        // 注意 lock_result.Value == -1 时也会触发这个事件
                        ListLocks?.Invoke(channel, new ListLocksEventArgs
                        {
                            Result = lock_result
                        });
                    }

                    /*
                    if (states.Count > 0)
                    {
                        // 注意 lock_result.Value == -1 时也会触发这个事件
                        ListLocks?.Invoke(channel, new ListLocksEventArgs
                        {
                            Result = new GetLockStateResult { States = states }
                        });
                    }
                    */

                    // 门锁状态就绪
                    _lockReady = true;
                }

#endif
            },
            token);
        }

        public static int InventoryIdleSeconds { get; set; }

        // parameters:
        //      sleep   是否需要补充延时
        static void _callLoop(BaseChannel<IRfid> channel,
            string loop_style,
            bool sleep = true)
        {
            /*
            if (string.IsNullOrEmpty(_antennaList) == false)
                style += ",antenna:" + _antennaList;
                */

            GetLockStateResult lock_result = null;
            var readerNameList = _readerNameList;
            if (string.IsNullOrEmpty(readerNameList) == false
            || (_lockThread != "base2" && string.IsNullOrEmpty(LockCommands) == false))
            {
                string style = $"session:{Base.GetHashCode()}";

                if (GetRSSI)
                    style += ",rssi";

                // 2019/12/4
                if (_lockThread != "base2"
                    && string.IsNullOrEmpty(LockCommands) == false)
                    style += ",getLockState:" + StringUtil.EscapeString(LockCommands, ":,");

                // 2023/11/25
                if (SyncSetEAS)
                    style += ",dont_delay";

                DateTime start_time = DateTime.Now;

                object __lockObj = _syncRoot;
                bool __lockWasTaken = false;
                try
                {
                    // 外层加锁
                    if (SyncSetEAS)
                        System.Threading.Monitor.Enter(__lockObj, ref __lockWasTaken);

                    var result = channel?.Object?.ListTags(readerNameList, style);
                    if (result.Value == -1)
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs
                            {
                                Error = result.ErrorInfo
                            });
                    else
                        Base.TriggerSetError(result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错

                    lock_result = result.GetLockStateResult;

                    IncLockHeartbeat();

                    if (string.IsNullOrEmpty(readerNameList) == false)
                    {
                        // using (var releaser = await _limit.EnterAsync().ConfigureAwait(false))
                        try
                        {
#if SYNC_ROOT
                            // 内层加锁
                            if (SyncSetEAS == false)
                                System.Threading.Monitor.Enter(__lockObj, ref __lockWasTaken);
#endif

                            if (ListTags != null)
                            {
                                // 先记忆
                                _lastTags = result.Results;

                                // 注意 result.Value == -1 时也会触发这个事件
                                ListTags(channel, new ListTagsEventArgs
                                {
                                    ReaderNameList = readerNameList,
                                    Result = result,
                                    Source = "base",
                                });
                            }
                            else
                                _lastTags = null;

                            _tagsReady = true;
                        }
                        finally
                        {
                            if (SyncSetEAS == false && __lockWasTaken)
                                System.Threading.Monitor.Exit(__lockObj);

                        }
                    }

                }
                finally
                {
#if SYNC_ROOT
                    if (SyncSetEAS && __lockWasTaken)
                        System.Threading.Monitor.Exit(__lockObj);
#endif
                }

                // 补充延时
                if (sleep && SyncSetEAS)
                {
                    // 标准间隔
                    TimeSpan standard_length = GetStandardLength() + TimeSpan.FromSeconds(InventoryIdleSeconds);
                    // 上一轮盘点实际使用的时间
                    var length = DateTime.Now - start_time;
                    var delta = standard_length - length;
                    // 补足差额
                    if (delta.TotalMilliseconds > 0)
                        Thread.Sleep((int)delta.TotalMilliseconds);
                }
            }

            // 检查门状态
            if (_lockThread != "base2"
            && lock_result != null)
            {
                // List<LockState> states = new List<LockState>();
                {
                    // parameters:
                    //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
                    //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
                    //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
                    //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
                    if (lock_result.Value == -1)
                        Base.TriggerSetError(lock_result,
                            new SetErrorEventArgs { Error = lock_result.ErrorInfo });
                    else
                        Base.TriggerSetError(lock_result,
                            new SetErrorEventArgs { Error = null }); // 清除以前的报错
                    /*
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
                    */
                    // 注意 lock_result.Value == -1 时也会触发这个事件
                    ListLocks?.Invoke(channel, new ListLocksEventArgs
                    {
                        Result = lock_result
                    });
                }

                /*
                if (states.Count > 0)
                {
                    // 注意 lock_result.Value == -1 时也会触发这个事件
                    ListLocks?.Invoke(channel, new ListLocksEventArgs
                    {
                        Result = new GetLockStateResult { States = states }
                    });
                }
                */

                // 门锁状态就绪
                _lockReady = true;
            }
        }


        static long _lockHeartbeat = 0;

        // 增量心跳计数
        static void IncLockHeartbeat()
        {
            Interlocked.Increment(ref _lockHeartbeat);
        }

        public static long LockHeartbeat
        {
            get
            {
                return Interlocked.Read(ref _lockHeartbeat);
            }
        }

        public static BaseChannel<IRfid> GetChannel()
        {
            return Base.GetChannel();
        }

        public static void ReturnChannel(BaseChannel<IRfid> channel)
        {
            Base.ReturnChannel(channel);
        }

        // parameters:
        //      source  触发者来源信息
        public static async Task TriggerListTagsEvent(
            string reader_name_list,
            ListTagsResult result,
            string source/*,
            StringBuilder debugInfo*/,
            bool throwException)
        {
            if (ListTags == null)
                return;

            try
            {
                //debugInfo.AppendLine("1");

                BaseChannel<IRfid> channel = Base.GetChannel();
                //debugInfo.AppendLine("2");
                try
                {
                    //debugInfo.AppendLine("3");
#if SYNC_ROOT
                    lock (_syncRoot)
#endif
                    // using (var releaser = await _limit.EnterAsync().ConfigureAwait(false))
                    {
                        //debugInfo.AppendLine("4");

                        ListTags?.Invoke(channel, new ListTagsEventArgs
                        {
                            ReaderNameList = reader_name_list,
                            // Result = new ListTagsResult { Results = _lastTags }
                            Result = result,
                            Source = source,    // 可能是 base/base2/initial/refresh
                        });

                        //debugInfo.AppendLine("5");
                    }
                }
                finally
                {
                    //debugInfo.AppendLine("6");

                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                // Base.Clear();
                await Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });

                if (throwException)
                    throw ex;
            }
        }


#if NO
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

#endif

        // 旧版本
        public static GetTagInfoResult GetTagInfo(string reader_name,
            string uid,
            uint antenna_id)
        {
            /*
            if (uid == "00000000")
                throw new Exception($"uid 错误！");
            */
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    // lock (GetLockObject(""))
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
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetTagInfoResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2023/11/11
        // 最新版
        public static GetTagInfoResult GetTagInfo(
            string reader_name,
            string uid,
            uint antenna_id,
            string protocol,
            string style)
        {
            /*
            if (uid == "00000000")
                throw new Exception($"uid 错误！");
            */
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    // lock (GetLockObject(style))
                    {
                        var result = channel.Object.GetTagInfo(reader_name,
                        uid,
                        antenna_id,
                        protocol,
                        style);
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错

                        return result;
                    }
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new GetTagInfoResult { Value = -1, ErrorInfo = ex.Message };
            }
        }


        // 2023/11/6
        // 校验超高频标签的 User Bank 新旧尺寸。要求新尺寸不应大于旧尺寸
        public static string VerifyOldNewUserBankCapacity(TagInfo oldTagInfo,
            TagInfo newTagInfo)
        {
            if (oldTagInfo.Protocol != InventoryInfo.ISO18000P6C)
                return null;
            if (newTagInfo.Bytes == null)
                return null;

            // 当 old_bytes_length == 0 时，无法判断当前标签 User Bank 能容纳的最大字节数
            // TODO: 或者，更加准确地可能要从 EPC PC 中的 UMI 位来判断
            if (oldTagInfo.Bytes == null)
                return null;

            if (newTagInfo.Bytes.Length > oldTagInfo.Bytes.Length)
                return $"UHF 标签的即将写入的 User Bank 内容字节数 {newTagInfo.Bytes.Length} 大于最大字节数 {oldTagInfo.Bytes.Length}，无法写入";
            return null;
        }

        public static NormalResult WriteTagInfo(string reader_name,
            TagInfo oldTagInfo,
            TagInfo newTagInfo,
            bool verify_user_bank_capacity = true)
        {
            if (verify_user_bank_capacity)
            {
                var error = VerifyOldNewUserBankCapacity(oldTagInfo, newTagInfo);
                if (error != null)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = error,
                        ErrorCode = "userBankOverflow"
                    };
            }
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
                // Base.Clear();   // ??
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2022/11/25
        // 是否要把 SetEAS() 和 ListTag 动作按照先后协调运作?
        public static bool SyncSetEAS { get; set; }

        static object GetLockObject(string style)
        {
            if (SyncSetEAS || StringUtil.IsInList("sync", style))
                return SyncRoot;
            return new object();
        }


        public static SetEasResult SetEAS(string reader_name,
            string tag_name,
            uint antenna_id,
            bool enable,
            string style = "")
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    lock (GetLockObject(style))
                    {
                        var result = channel.Object.SetEAS1(reader_name, tag_name, antenna_id, enable, style);
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                        return result;
                    }
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new SetEasResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static SetEasResult SetEAS(string uid,
            uint antenna_id,
            bool enable,
            string style = "")
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    lock (GetLockObject(style))
                    {
                        var result = channel.Object.SetEAS1("*", $"uid:{uid}", antenna_id, enable, style);
                        if (result.Value == -1)
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            Base.TriggerSetError(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错

                        return result;
                    }
                }
                finally
                {
                    Base.ReturnChannel(channel);
                }
            }
            catch (Exception ex)
            {
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new SetEasResult { Value = -1, ErrorInfo = ex.Message };
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
                // Base.Clear();
                _ = Base.ClearAsync();

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
                /*
                bool old_checkState = Base.CheckState;
                Base.CheckState = false;
                try
                {
                    channel = Base.GetChannel();
                }
                finally
                {
                    Base.CheckState = old_checkState;
                }
                */

                channel = Base.GetChannel("skip_check_state");

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
                // Base.Clear();
                _ = Base.ClearAsync();

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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 新版本 2020/11/21
        // 打开门锁
        public static NormalResult OpenShelfLock(string lockName, string style)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.OpenShelfLock(lockName, style);
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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 模拟关门。仅用于模拟测试
        public static NormalResult CloseShelfLock(string lockName)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.CloseShelfLock(lockName);
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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2020/4/8
        // 开关紫外灯
        public static NormalResult TurnSterilamp(string lampName, string action)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.TurnSterilamp(lampName, action);
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
                // Base.Clear();
                _ = Base.ClearAsync();

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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2020/8/19
        public static NormalResult PosPrint(string action,
            string text,
            string style)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.PosPrint(action, text, style);
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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2020/7/1
        public static NormalResult LedDisplay(string ledName,
            string text,
            int x,
            int y,
            DisplayStyle property,
            string style)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.LedDisplay(ledName, text, x, y, property, style);
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
                // Base.Clear();
                _ = Base.ClearAsync();

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
            // 特殊用法
            return GetTagInfo(reader_name, "00000000", antenna_id);
        }

        // 2023/11/28
        // 立即盘点一次。
        // 注: 一般情况下，RfidManager 的后台线程会驱动一轮一轮不停盘点。
        public static void CallInventory(string loop_style)
        {
            BaseChannel<IRfid> channel = Base.GetChannel();
            try
            {
                _callLoop(channel, loop_style, false);
            }
            finally
            {
                Base.ReturnChannel(channel);
            }
        }


        public static ListTagsResult CallListTags(string reader_name, string style)
        {
            if (GetRSSI)
                style += ",rssi";

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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new ListTagsResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        // 2020/11/13
        public static NormalResult SimuTagInfo(string action,
            List<TagInfo> tags,
            string style)
        {
            try
            {
                BaseChannel<IRfid> channel = Base.GetChannel();
                try
                {
                    var result = channel.Object.SimuTagInfo(action, tags, style);
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
                // Base.Clear();
                _ = Base.ClearAsync();

                Base.TriggerSetError(ex,
                    new SetErrorEventArgs
                    {
                        Error = $"RFID 中心出现异常: {ExceptionUtil.GetAutoText(ex)}"
                    });
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 2023/11/30
        // 注意有死锁的风险
        public static void ClearChannels()
        {
            Base.ClearChannels();
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

        public string Source { get; set; }   // 触发者
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
