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

namespace dp2SSL
{
    public static partial class ShelfData
    {
        #region MonitorTask

        // 和 dp2library 服务器之间的通讯状况
        static string _libraryNetworkCondition = "OK";  // OK/Bad

        // 可以适当降低探测的频率。比如每五分钟探测一次
        // 两次检测网络之间的间隔
        static TimeSpan _detectPeriod = TimeSpan.FromMinutes(5);
        // 最近一次检测网络的时间
        static DateTime _lastDetectTime;

        // 两次零星同步之间的间隔
        static TimeSpan _replicatePeriod = TimeSpan.FromMinutes(1);
        // 最近一次零星同步的时间
        static DateTime _lastReplicateTime;

        static Task _monitorTask = null;

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

                        if (DateTime.Now - _lastDetectTime > _detectPeriod)
                        {
                            DetectLibraryNetwork();

                            _lastDetectTime = DateTime.Now;
                        }

                        // 提醒关门
                        WarningCloseDoor();

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
                                }
                                else
                                    SaveStartDate(repl_result.StartDate);

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
TaskScheduler.Default);
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
        static TimeSpan _warningDoorLength = TimeSpan.FromSeconds(15);  // 30
        static DateTime _lastWarningTime;

        static void WarningCloseDoor()
        {
            var now = DateTime.Now;

            // 控制进入本函数的频率
            if (now - _lastWarningTime < TimeSpan.FromSeconds(10))  // 20
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
                App.CurrentApp.SpeakSequence($"不要忘记关门 {StringUtil.MakePathList(doors, ",")}");
        }

        #endregion
    }
}
