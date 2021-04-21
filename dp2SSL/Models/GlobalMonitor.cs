using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DigitalPlatform;
using DigitalPlatform.WPF;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;

namespace dp2SSL.Models
{
    // 全局监控任务
    public static class GlobalMonitor
    {
        static Task _monitorTask = null;

        // 是否需要重启计算机
        static bool _needReboot = false;
        // 最近一次检查升级的时刻
        static DateTime _lastUpdateTime;
        // 检查升级的时间间隔
        static TimeSpan _updatePeriod = TimeSpan.FromMinutes(60); // 2*60 两个小时

        // 成功升级的次数
        static int _updateSucceedCount = 0;

        // 监控间隔时间
        static TimeSpan _monitorIdleLength = TimeSpan.FromSeconds(10);

        static AutoResetEvent _eventMonitor = new AutoResetEvent(false);

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

            CancellationToken token = App.CancelToken;

            token.Register(() =>
            {
                _eventMonitor.Set();
            });

            _monitorTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("全局监控专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(10));
                        _eventMonitor.WaitOne(_monitorIdleLength);

                        token.ThrowIfCancellationRequested();

                        // 清除 PasswordCache
                        PasswordCache.CleanIdlePassword(ToTimeLength(App.CacheWorkerPasswordLength));

                        // 检查小票打印机状态
                        var check_result = CheckPosPrinter();
                        if (check_result.Value == -1)
                            App.SetError("printer", "小票打印机状态异常");
                        else if (StringUtil.IsInList("paperout", check_result.ErrorCode))
                            App.SetError("printer", "小票打印机缺纸");
                        else if (ChargingData.GetPosPrintPaperWillOut()
                                && StringUtil.IsInList("paperwillout", check_result.ErrorCode))
                            App.SetError("printer", "小票打印机即将缺纸");
                        else
                            App.SetError("printer", null);

                        // 检查读者信息区是否固定时间太长？
                        await WarningFixedPatronInfoAsync();

                        // 检查升级绿色 dp2ssl
                        if (_needReboot == false
                        && StringUtil.IsDevelopMode() == false
                        && ApplicationDeployment.IsNetworkDeployed == false
                        && _updateSucceedCount == 0 // 一旦更新成功一次以后便不再更新
                        && DateTime.Now - _lastUpdateTime > _updatePeriod)
                        {
                            WpfClientInfo.WriteInfoLog("开始自动检查升级");
                            // result.Value:
                            //      -1  出错
                            //      0   经过检查发现没有必要升级
                            //      1   成功
                            //      2   成功，但需要立即重新启动计算机才能让复制的文件生效
                            var update_result = await GreenInstall.GreenInstaller.InstallFromWeb(GreenInstall.GreenInstaller.dp2ssl_weburl,  // "http://dp2003.com/dp2ssl/v1_dev",
                                "c:\\dp2ssl",
                                "delayExtract,updateGreenSetupExe,clearStateFile,debugInfo",
                                //true,
                                //true,
                                token,
                                null);
                            if (update_result.Value == -1)
                                WpfClientInfo.WriteErrorLog($"自动检查升级出错: {update_result.ErrorInfo}");
                            else
                                WpfClientInfo.WriteInfoLog($"结束自动检查升级 update_result:{update_result.ToString()}");

                            // 2020/9/1
                            WpfClientInfo.WriteInfoLog($"InstallFromWeb() 调试信息如下:\r\n{update_result.DebugInfo}");

                            if (update_result.Value == 1 || update_result.Value == 2)
                            {
                                _updateSucceedCount++;
                                if (update_result.Value == 1)
                                {
                                    App.TriggerUpdated("重启 dp2ssl(greensetup) 可使用新版本");
                                    PageShelf.TrySetMessage(null, "dp2SSL 升级文件已经下载成功，下次重启 dp2ssl(greensetup) 时可自动启用新版本");
                                }
                                else if (update_result.Value == 2)
                                {
                                    _needReboot = true;
                                    App.TriggerUpdated("重启计算机可使用新版本");
                                    PageShelf.TrySetMessage(null, "dp2SSL 升级文件已经下载成功，下次重启计算机时可自动升级到新版本");
                                }
                            }
                            _lastUpdateTime = DateTime.Now;
                        }

                        // 2020/9/15
                        // 检查升级 ClickOnce dp2ssl
                        if (StringUtil.IsDevelopMode() == false
                        && ApplicationDeployment.IsNetworkDeployed == true
                        && _updateSucceedCount == 0 // 一旦更新成功一次以后便不再更新
                        && DateTime.Now - _lastUpdateTime > _updatePeriod)
                        {
                            try
                            {
                                // result.Value:
                                //      -1  出错
                                //      0   没有发现新版本
                                //      1   发现新版本，重启后可以使用新版本
                                NormalResult result = WpfClientInfo.InstallUpdateSync();
                                WpfClientInfo.WriteInfoLog($"ClickOnce 后台升级 dp2ssl 返回: {result.ToString()}");
                                if (result.Value == -1)
                                    WpfClientInfo.WriteErrorLog($"升级出错: {result.ErrorInfo}");
                                else if (result.Value == 1)
                                {
                                    _updateSucceedCount++;
                                    WpfClientInfo.WriteInfoLog($"升级成功: {result.ErrorInfo}");
                                    App.TriggerUpdated(result.ErrorInfo);
                                    // MessageBox.Show(result.ErrorInfo);
                                }
                                else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                                {
                                    WpfClientInfo.WriteInfoLog($"{result.ErrorInfo}");
                                }
                            }
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"后台 ClickOnce 自动升级出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }

                            _lastUpdateTime = DateTime.Now;
                        }

                        // 每隔一段时间写入紧凑日志一次
                        if (DateTime.Now - _lastCompactTime > _compactLength)
                        {
                            FlushCompactLog();
                        }
                    }
                    _monitorTask = null;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"全局监控专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("global_monitor", $"全局监控专用线程出现异常: {ex.Message}");
                }
                finally
                {
                    FlushCompactLog();
                    WpfClientInfo.WriteInfoLog("全局监控专用线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        static void FlushCompactLog()
        {
            _compactLog?.WriteToLog((text) =>
            {
                WpfClientInfo.WriteErrorLog(text);
            });
            _lastCompactTime = DateTime.Now;
        }

        // 紧凑日志
        static CompactLog _compactLog = new CompactLog();
        public static CompactLog CompactLog
        {
            get
            {
                return _compactLog;
            }
        }

        static DateTime _lastCompactTime;
        static TimeSpan _compactLength = TimeSpan.FromHours(8);

        // https://mattyrowan.com/2008/01/01/parse-timespan-string/
        public static TimeSpan ParseTimeSpan(string s)
        {
            const string Quantity = "quantity";
            const string Unit = "unit";

            const string Days = @"(d(ays?)?)";
            const string Hours = @"(h((ours?)|(rs?))?)";
            const string Minutes = @"(m((inutes?)|(ins?))?)";
            const string Seconds = @"(s((econds?)|(ecs?))?)";

            Regex timeSpanRegex = new Regex(
                string.Format(@"\s*(?<{0}>\d+)\s*(?<{1}>({2}|{3}|{4}|{5}|\Z))",
                              Quantity, Unit, Days, Hours, Minutes, Seconds),
                              RegexOptions.IgnoreCase);
            MatchCollection matches = timeSpanRegex.Matches(s);

            TimeSpan ts = new TimeSpan();
            foreach (Match match in matches)
            {
                if (Regex.IsMatch(match.Groups[Unit].Value, @"\A" + Days))
                {
                    ts = ts.Add(TimeSpan.FromDays(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Hours))
                {
                    ts = ts.Add(TimeSpan.FromHours(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Minutes))
                {
                    ts = ts.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Seconds))
                {
                    ts = ts.Add(TimeSpan.FromSeconds(double.Parse(match.Groups[Quantity].Value)));
                }
                else
                {
                    // Quantity given but no unit, default to Hours
                    ts = ts.Add(TimeSpan.FromHours(double.Parse(match.Groups[Quantity].Value)));
                }
            }
            return ts;
        }

        static TimeSpan ToTimeLength(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "无")
                return TimeSpan.FromMinutes(0);

            try
            {
                return ParseTimeSpan(name);
            }
            catch
            {
                return TimeSpan.FromMinutes(1);
            }
        }

        // 立即进行升级
        public static void ActivateUpdate()
        {
            if (_updateSucceedCount > 0)
            {
                PageShelf.TrySetMessage(null, "稍早已经更新过了，现在重启 dp2ssl 可以使用此新版本");
                return;
            }

            PageShelf.TrySetMessage(null, "开始更新");
            _lastUpdateTime = DateTime.MinValue;
            ActivateMonitor();
        }

        // 检查小票打印机状态
        static DigitalPlatform.NormalResult CheckPosPrinter()
        {
            if (string.IsNullOrEmpty(App.PosPrintStyle)
                || App.PosPrintStyle == "不打印")
                return new DigitalPlatform.NormalResult();

            if (string.IsNullOrEmpty(App.RfidUrl)
                || string.IsNullOrEmpty(RfidManager.Url))
                return new DigitalPlatform.NormalResult();

            var result = RfidManager.PosPrint("getstatus", "", "");
            if (result.Value == 0)
            {
                return result;
                /*
                if (StringUtil.IsInList("paperout", result.ErrorCode))
                    return new NormalResult { ValueTask = -1, ErrorCode = result.ErrorCode };
                */
            }
            return result;
        }

        #region 警告固定读者信息太长时间

        static TimeSpan _warningFixedLength = TimeSpan.FromMinutes(10);   // TimeSpan.FromSeconds(25);

        // 警告固定读者信息太长时间
        static async Task WarningFixedPatronInfoAsync()
        {
            bool isShelf = App.IsPageShelfActive;
            bool isBorrow = App.IsPageBorrowActive;
            // bool isInventory = App.IsPageInventoryActive;

            if (isShelf && PageMenu.PageShelf != null)
            {
                if (ShelfData.OpeningDoorCount > 0)
                    return;
                var fill_time = PageMenu.PageShelf.GetFillTime();
                if (fill_time == DateTime.MinValue)
                    return;
                if (DateTime.Now - fill_time > _warningFixedLength)
                {
                    var result = await App.ConfirmAsync(new CancellationToken());
                    if (result == false)
                        PageMenu.PageShelf.PatronClear();
                    else
                        PageMenu.PageShelf.ResetFillTime();
                }
                //    App.CurrentApp.Speak("不要忘记清除读者信息");
            }

            if (isBorrow
                && PageMenu.PageBorrow != null
                && PageMenu.PageBorrow.IsVerticalCard())
            {
                var fill_time = PageMenu.PageBorrow.GetFillTime();
                if (fill_time == DateTime.MinValue)
                    return;
                if (DateTime.Now - fill_time > _warningFixedLength)
                {
                    var result = await App.ConfirmAsync(new CancellationToken());
                    if (result == false)
                        PageMenu.PageBorrow.PatronClear(false);
                    else
                        PageMenu.PageBorrow.ResetFillTime();
                }

            }
        }

        #endregion
    }
}
