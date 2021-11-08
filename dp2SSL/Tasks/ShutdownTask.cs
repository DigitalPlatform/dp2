using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using FluentScheduler;
using static dp2SSL.LampPerdayTask;

namespace dp2SSL
{
    /// <summary>
    /// 每日定时关机任务
    /// </summary>
    public static class ShutdownTask
    {
        public static void ParseParam(
            string param,
            bool validate,
            out string time_range,
            out string weekdays)
        {
            time_range = null;
            weekdays = null;

            List<string> parameters = StringUtil.SplitList(param, " ");
            foreach (string parameter in parameters)
            {
                string name = "";
                string value = "";
                if (parameter.StartsWith("-"))
                {
                    var parts = StringUtil.ParseTwoPart(parameter.Substring(1), ":");
                    name = parts[0].ToLower();
                    value = parts[1];
                }
                else
                {
                    name = "range";
                    value = parameter;
                }

                if (name == "range")
                    time_range = value;
                else if (name == "weekday")
                    weekdays = value;
                else
                {
                    throw new Exception($"无法识别子参数名 '{name}'");
                }
            }

            // 验证参数合法性
            if (validate)
            {
                if (string.IsNullOrEmpty(time_range) == false)
                {
                    var times = ParseTimeList(time_range);
                }

                if (string.IsNullOrEmpty(weekdays) == false)
                {
                    var days = LampPerdayTask.ParseDayOfWeekDef(weekdays);
                }
            }
        }

        // 修改每日定时关灯参数
        // -range:8:00,11:00 -weekday:*,-0,-6
        // -range:8:00,11:00 -weekday:+1,+2,+3,+4,+5
        public static NormalResult ChangePerdayTask(string param)
        {
            try
            {
                ParseParam(
        param,
        true,
        out string time_range,
        out string weekdays);

                // 每日关机时间点
                WpfClientInfo.Config.Set("tasks", "shutdown", time_range);
                WpfClientInfo.Config.Set("tasks", "shutdown_weekday", weekdays);
                return StartPerdayTask();
            }
            catch (ArgumentException ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
        }

        public static string GetPerdayTask()
        {
            // 每日关机时间点
            string time_range = WpfClientInfo.Config.Get("tasks", "shutdown", null);
            string weekdays = WpfClientInfo.Config.Get("tasks", "shutdown_weekday", null);
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(time_range) == false)
                results.Add(time_range);
            if (string.IsNullOrEmpty(weekdays) == false)
                results.Add(weekdays);
            return StringUtil.MakePathList(results, " ");
        }

        // 启动每日定时关机任务
        public static NormalResult StartPerdayTask()
        {
            // 初始化任务调度器
            TryInitialize();

            // Cancel 可能正在提示的对话框
            CancelShutdown();
            // WpfClientInfo.WriteInfoLog($"sterilamp 先关掉(可能亮着的)紫外灯");

            // 每日关机时间点
            string time_range = WpfClientInfo.Config.Get("tasks", "shutdown", null);
            string weekday = WpfClientInfo.Config.Get("tasks", "shutdown_weekday", null);

            WpfClientInfo.WriteInfoLog($"shutdown time_range={time_range}");
            WpfClientInfo.WriteInfoLog($"shutdown_weekday={weekday}");

            /*
            // testing
            string time_range = "1:00,9:00";
            string weekday = "*,-0,-6"; // +1,+2,+3,+4,+5
            */

            try
            {
                var times = ParseTimeList(time_range);

                WpfClientInfo.WriteInfoLog($"shutdown time_list '{time_range}' 解析后得到 '{PerdayTime.ToString(times)}'");

                // 移除以前的全部同类任务
                {
                    List<string> names = new List<string>();
                    foreach (var s in JobManager.AllSchedules)
                    {
                        if (s.Name != null && s.Name.StartsWith("shutdown"))
                            names.Add(s.Name);
                    }

                    foreach (var name in names)
                    {
                        JobManager.RemoveJob(name);
                    }
                }

                if (times.Count == 0)
                {
                    WpfClientInfo.WriteInfoLog($"shutdown time_range 为空, 结束 StartPerdayTask()");
                    return new NormalResult();
                }

                // 安排任务
                int i = 0;
                foreach (var time in times)
                {
                    JobManager.AddJob(
                        () =>
                        {
                            WpfClientInfo.WriteInfoLog($"shutdown time_range='{time_range}' weekday='{weekday}'");

                            if (InWeekday() == true)
                            {
                                WpfClientInfo.WriteInfoLog("shutdown job head 触发执行关机");
                                _ = BeginShutdown();
                            }
                            else
                                WpfClientInfo.WriteInfoLog("shutdown job head 触发，但因为当前时间不在 weekday 范围，未执行关机");
                        },
                        s => s.WithName($"shutdown{(i++).ToString()}").ToRunEvery(1).Days().At(time.Hour, time.Minute)
                    );
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"shutdown 解析关机时间点字符串时发现错误: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"shutdown 解析关机时间点字符串时发现错误: { ex.Message}"
                };
            }

            // 检查当前日期是否在允许之列
            bool InWeekday()
            {
                if (string.IsNullOrEmpty(weekday))
                    return true;
                DateTime now = DateTime.Now;
                var days = LampPerdayTask.ParseDayOfWeekDef(weekday);
                if (days.IndexOf(now.DayOfWeek) == -1)
                    return false;
                return true;
            }
        }

        // 解析时间点参数
        static List<PerdayTime> ParseTimeList(string range)
        {
            var times = StringUtil.SplitList(range, ",");
            List<PerdayTime> results = new List<PerdayTime>();
            foreach (var time in times)
            {
                var result1 = LampPerdayTask.ParseTime(time);
                if (result1.Value == -1)
                    throw new ArgumentException(result1.ErrorInfo);
                results.Add(result1.Time);
            }
            return results;
        }

        #region 定时关机

        static Task _shutdownTask = null;
        static CancellationTokenSource _cancelShutdown = new CancellationTokenSource();

        // 开始执行关机
        public static bool BeginShutdown()
        {
            if (_shutdownTask != null)
                return false;

            _cancelShutdown?.Cancel();
            _cancelShutdown?.Dispose();

            _cancelShutdown = new CancellationTokenSource();
            _shutdownTask = ShutdownAsync(_cancelShutdown.Token);
            return true;
        }

        // 取消正在进行的关机操作
        public static void CancelShutdown()
        {
            _cancelShutdown?.Cancel();
        }

        // 关机
        async static Task ShutdownAsync(CancellationToken token)
        {
            ProgressWindow progress = null;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken, token))
            {
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "自动关机";
                    progress.MessageText = "警告：即将关机";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "取消";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "wide");
                    progress.BackColor = "yellow";
                    progress.Show();
                }));

                PageShelf.TrySetMessage(null, "即将自动关机，正在倒计时 ...");

                try
                {
                    // 若当前为书柜界面，则需要检查是否有打开的门
                    // TODO: 如果本次尝试失败，后面隔一会儿是否还会继续尝试关机
                    if (ShelfData.OpeningDoorCount > 0)
                    {
                        var text = $"当前有 {ShelfData.OpeningDoorCount} 个柜门处于打开状态，暂时无法进行自动关机";
                        App.CurrentApp.Speak(text);
                        return;
                    }

                    // 首先倒计时警告即将关机
                    App.CurrentApp.Speak("即将自动关机，若不想关机可以按“取消”按钮");
                    for (int i = 20; i > 0; i--)
                    {
                        if (cancel.Token.IsCancellationRequested)
                            return;
                        string text = $"({i}) 即将自动关机，若不想关机可以按“取消”按钮\r\n\r\n";
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = text;
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                    }

                    App.Invoke(new Action(() =>
                    {
                        progress.BackColor = "red";
                        progress.MessageText = "正在关机 ...";
                    }));

                    {
                        // 关闭电脑
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                // 为 dp2ssl 开机后自动重启预先设定好 cmdlineparam.txt 文件
                                WriteParameterFile();

                                await Task.Delay(1000);
                                WpfClientInfo.WriteInfoLog($"自动关机执行");
                                ShutdownUtil.DoExitWindows(ShutdownUtil.ExitWindows.ShutDown);
                            }
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"自动关机过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }
                        });
                        PageShelf.TrySetMessage(null, "Windows 将在一秒后关机");
                    }
                }
                finally
                {
                    // App.CurrentApp.Speak("已经关机");
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));

                    // PageShelf.TrySetMessage(null, "已经关机");

                    _shutdownTask = null;
                }
            }
        }

        // 准备命令行参数文件
        public static bool WriteParameterFile()
        {
            try
            {
                string binDir = "c:\\dp2ssl";
                if (Directory.Exists(binDir))
                {
                    string fileName = System.IO.Path.Combine(binDir, "cmdlineparam.txt");
                    File.WriteAllText(fileName, "silently");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"准备命令行参数文件时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return false;
            }
        }


        #endregion

    }

    // https://stackoverflow.com/questions/4396205/implementing-validations-in-wpf-propertygrid
    public class ShutdownParamValidator
    {
        public static ValidationResult Validate(string text)
        {
            try
            {
                ShutdownTask.ParseParam(text,
                    true,
                    out string time_range,
                    out string weekdays);
            }
            catch (Exception ex)
            {
                return new ValidationResult(ex.Message);
            }

            return ValidationResult.Success;
        }
    }
}
