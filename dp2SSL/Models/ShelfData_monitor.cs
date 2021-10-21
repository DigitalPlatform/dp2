using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Deployment.Application;

using dp2SSL.Models;

using GreenInstall;
using static dp2SSL.Models.PatronReplication;
using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.WPF;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Install;
using FluentScheduler;

namespace dp2SSL
{
    public static partial class ShelfData
    {
        #region MonitorTask

        // 和 dp2library 服务器之间的通讯状况
        static string _libraryNetworkCondition = "OK";  // OK/Bad

        // 可以适当降低探测的频率。比如每五分钟探测一次
        // 两次检测网络之间的间隔
        static TimeSpan _networkDetectPeriodShort = TimeSpan.FromMinutes(5);
        static TimeSpan _networkDetectPeriodLong = TimeSpan.FromMinutes(20);

        // 最近一次检测网络的时间
        static DateTime _lastDetectTime;

        // 两次零星同步之间的间隔
        static TimeSpan _replicatePeriod = TimeSpan.FromMinutes(10);
        // 最近一次零星同步的时间
        static DateTime _lastReplicateTime;

        static Task _monitorTask = null;

        public static Task Task
        {
            get
            {
                return _monitorTask;
            }
        }

        // OK/Bad
        public static string LibraryNetworkCondition
        {
            get
            {
                return _libraryNetworkCondition;
            }
        }

#if REMOVED
        // 是否已经(升级)更新了
        static bool _updated = false;
        // 最近一次检查升级的时刻
        static DateTime _lastUpdateTime;
        // 检查升级的时间间隔
        static TimeSpan _updatePeriod = TimeSpan.FromMinutes(2 * 60); // 2*60 两个小时
#endif

        // 监控间隔时间
        static TimeSpan _monitorIdleLength = TimeSpan.FromSeconds(10);

        static AutoResetEvent _eventMonitor = new AutoResetEvent(false);

        static int _replicatePatronError = 0;

        static int _replicateEntityError = 0;

        // 激活 Monitor 任务
        public static void ActivateMonitor()
        {
            _eventMonitor.Set();
        }


        // 启动一般监控任务
        public static void StartMonitorTask()
        {
            if (_monitorTask != null)
                return;

            LampPerdayTask.StartPerdayTask();
            SterilampTask.StartPerdayTask();

            CancellationToken token = _cancel.Token;
            // bool download_complete = false;

            token.Register(() =>
            {
                _eventMonitor.Set();
            });

            _monitorTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("书柜监控专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(10));
                        _eventMonitor.WaitOne(_monitorIdleLength);

                        token.ThrowIfCancellationRequested();

                        // ***
                        // 关闭天线射频
                        if (_tagAdded)
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await SelectAntennaAsync();
                                }
                                catch (Exception ex)
                                {
                                    WpfClientInfo.WriteErrorLog($"关闭天线射频 SelectAntennaAsync() 时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                                }
                            });
                            _tagAdded = false;
                        }

                        // 检测网络状况
                        // 网络 OK 的时候检查间隔长；Bad 的时候检查间隔短
                        if (
                            (ShelfData.LibraryNetworkCondition == "OK" && DateTime.Now - _lastDetectTime > _networkDetectPeriodLong)
                            || (ShelfData.LibraryNetworkCondition != "OK" && DateTime.Now - _lastDetectTime > _networkDetectPeriodShort)
                            )
                        {
                            DetectLibraryNetwork();

                            _lastDetectTime = DateTime.Now;
                        }

                        // 提醒关门
                        WarningCloseDoor();

                        // TODO: 断网模式下是否需要语音警告，有全量同步操作被延迟
                        if (ShelfData.LibraryNetworkCondition == "OK")
                        {

                            // 下载或同步读者信息
                            string startDate = LoadStartDate();
                            if (/*download_complete == false || */
                            string.IsNullOrEmpty(startDate)
                            && _replicatePatronError == 0)
                            {
                                // 如果 Config 中没有记载断点位置，说明以前从来没有首次同步过。需要进行一次首次同步
                                if (string.IsNullOrEmpty(startDate))
                                {
                                    // SaveStartDate("");

                                    App.CurrentApp.SpeakSequence("开始下载全部读者记录到本地缓存");
                                    var repl_result = await PatronReplication.DownloadAllPatronRecordAsync(
                                        (text) =>
                                        {
                                            WpfClientInfo.WriteInfoLog(text);
                                            PageShelf.TrySetMessage(null, text);
                                        },
                                        token);
                                    if (repl_result.Value == -1)
                                    {
                                        // TODO: 判断通讯出错的错误码。如果是通讯出错，则稍后需要重试下载
                                        _replicatePatronError++;

                                        App.CurrentApp.SpeakSequence($"下载全部读者记录到本地缓存出错: {repl_result.ErrorInfo}");
                                    }
                                    else
                                    {
                                        SaveStartDate(repl_result.StartDate);

                                        App.CurrentApp.SpeakSequence("下载全部读者记录到本地缓存完成");
                                    }

                                    // 立刻允许接着做一次零星同步
                                    ActivateMonitor();
                                }
                                // download_complete = true;
                            }
                            else
                            {
                                // 进行零星同步
                                if (DateTime.Now - _lastReplicateTime > _replicatePeriod)
                                {
                                    // string startDate = LoadStartDate();

                                    // testing
                                    // startDate = "20200507:0-";

                                    if (string.IsNullOrEmpty(startDate) == false)
                                    {
                                        string endDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

                                        // parameters:
                                        //      strLastDate   处理中断或者结束时返回最后处理过的日期
                                        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
                                        // return:
                                        //      -1  出错
                                        //      0   中断
                                        //      1   完成
                                        ReplicationResult repl_result = await PatronReplication.DoReplication(
                                            startDate,
                                            endDate,
                                            LogType.OperLog,
                                            token);
                                        if (repl_result.Value == -1)
                                        {
                                            WpfClientInfo.WriteErrorLog($"同步出错: {repl_result.ErrorInfo}");
                                        }
                                        else if (repl_result.Value == 1)
                                        {
                                            string lastDate = repl_result.LastDate + ":" + repl_result.LastIndex + "-";    // 注意 - 符号不能少。少了意思就会变成每次只获取一条日志记录了
                                            SaveStartDate(lastDate);
                                        }

                                        _lastReplicateTime = DateTime.Now;
                                    }
                                }
                            }

                            // 下载册记录和书目摘要到本地缓存
                            bool downloaded = WpfClientInfo.Config.GetBoolean("entityReplication", "downloaded", false);
                            // testing 
                            // downloaded = false;

                            if (App.ReplicateEntities == true
                            && downloaded == false
                            && _replicateEntityError == 0)
                            {
                                List<string> unprocessed = new List<string>();
                                string unprocessed_list = WpfClientInfo.Config.Get("entityReplication", "unprocessed", null);
                                List<string> input_dbnames = null;
                                if (string.IsNullOrEmpty(unprocessed_list) == false)
                                    input_dbnames = StringUtil.SplitList(unprocessed_list);

                                App.CurrentApp.SpeakSequence("开始下载全部册记录到本地缓存");
                                var repl_result = await EntityReplication.DownloadAllEntityRecordAsync(
                                    input_dbnames,
                                    unprocessed,
                                    (text) =>
                                    {
                                        WpfClientInfo.WriteInfoLog(text);
                                        PageShelf.TrySetMessage(null, text);
                                        App.CurrentApp.SpeakSequence(text);
                                    },
                                    token);
                                if (repl_result.Value == -1)
                                {
                                    // TODO: 判断通讯出错的错误码。如果是通讯出错，则稍后需要重试下载
                                    _replicateEntityError++;
                                    WpfClientInfo.Config.Set("entityReplication", "unprocessed", StringUtil.MakePathList(unprocessed));

                                    App.CurrentApp.SpeakSequence($"下载全部册记录到本地缓存出错: {repl_result.ErrorInfo}");
                                }
                                else
                                {
                                    WpfClientInfo.Config.SetBoolean("entityReplication", "downloaded", true);
                                    WpfClientInfo.Config.Set("entityReplication", "unprocessed", null);

                                    App.CurrentApp.SpeakSequence("下载全部册记录到本地缓存完成");
                                }
                            }
                        }
                    }
                    _monitorTask = null;

                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"书柜监控专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("shelf_monitor", $"书柜监控专用线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("书柜监控专用线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default).Unwrap();
        }

        public static void RestartReplicateEntities()
        {
            WpfClientInfo.Config.SetBoolean("entityReplication", "downloaded", false);
            WpfClientInfo.Config.Set("entityReplication", "unprocessed", null);
            App.ReplicateEntities = true;
        }

        // 初始化阶段探测本地读者缓存数据是否存在，如果不存在则设法启动首次读者同步
        public static void DetectPatronLocalDatabase()
        {
            if (PatronReplication.PatronDataExists() == false)
                SaveStartDate(null);
        }

        // TODO: 如何显示进度信息？
        // 开始重做全量同步读者信息
        public static void RedoReplicatePatron()
        {
            SaveStartDate(null);
            ActivateMonitor();
        }

        static void SaveStartDate(string startDate)
        {
            WpfClientInfo.Config.Set("patronReplication", "startDate", startDate);
        }

        static string LoadStartDate()
        {
            return WpfClientInfo.Config.Get("patronReplication", "startDate", null);
        }

        public static void DetectLibraryNetwork()
        {
            // 探测和 dp2library 服务器的通讯是否正常
            // return.Value
            //      -1  本函数执行出现异常
            //      0   网络不正常
            //      1   网络正常
            var detect_result = LibraryChannelUtil.DetectLibraryNetwork();
            if (detect_result.Value == 1)
            {
                _libraryNetworkCondition = "OK";
                PageMenu.PageShelf?.SetBackColor(Brushes.Black);
            }
            else
            {
                _libraryNetworkCondition = "Bad";
                PageMenu.PageShelf?.SetBackColor(Brushes.DarkBlue);
            }
        }

        // 从打开门开始多少时间开始警告关门
        static DateTime _lastWarningTime = DateTime.MinValue;

        // static TimeSpan _warningDoorLength = TimeSpan.FromSeconds(15);  // 15
        static TimeSpan _warningDoorLength
        {
            get
            {
                return TimeSpan.FromSeconds(
        ShelfData.GetWarningCloseDoorLength().Item1
        );
            }
        }

        static TimeSpan _warningDoorRepeatLength
        {
            get
            {
                return TimeSpan.FromSeconds(
        ShelfData.GetWarningCloseDoorLength().Item2
        );
            }
        }

        static void WarningCloseDoor()
        {
            var now = DateTime.Now;

            // 控制进入本函数的频率
            if (_lastWarningTime > DateTime.MinValue
                && now - _lastWarningTime < _warningDoorRepeatLength/*TimeSpan.FromSeconds(10)*/)  // 10
                return;

            // 2020/11/20
            // 在密集进行兑现借书、还书过程中，要抑制这里的语音提醒
            var progress = PageMenu.PageShelf.ProgressWindow;
            if (progress != null && progress.IsVisible == true)
                return;

            _lastWarningTime = now;

            List<string> doors = new List<string>();
            foreach (var door in Doors)
            {
                if (door.State == "open" && now - door.OpenTime > _warningDoorLength)
                {
                    doors.Add(door.Name);
                }
            }

            if (doors.Count > 0)
                App.CurrentApp.SpeakSequence($"不要忘记关门 {GetDoorNameSpeakText(doors)}");
            else
                _lastWarningTime = DateTime.MinValue;
        }

        // 获得一个描述若干门名字的语句
        public static string GetDoorNameSpeakText(List<string> names)
        {
            if (names.Count > 2)
                return $"{names.Count} 个门";
            return StringUtil.MakePathList(names, ", ");
        }

        #endregion
    }
}
