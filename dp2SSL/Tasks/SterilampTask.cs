using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Media;

using FluentScheduler;

using static dp2SSL.LampPerdayTask;
using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.RFID;

namespace dp2SSL
{
    /// <summary>
    /// 每日紫外灯任务
    /// </summary>
    public static class SterilampTask
    {
        // 修改每日定时亮灯参数
        // -range:8:00,11:00 -weekday:*,-0,-6
        // -range:8:00,11:00 -weekday:+1,+2,+3,+4,+5
        public static NormalResult ChangePerdayTask(string param)
        {
            string time_range = null;
            string weekdays = null;

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

            // 每日亮灯时间点
            WpfClientInfo.Config.Set("tasks", "sterilamp", time_range);
            WpfClientInfo.Config.Set("tasks", "sterilamp_weekday", weekdays);
            return StartPerdayTask();
        }

        public static string GetPerdayTask()
        {
            // 每日亮灯时间点
            string time_range = WpfClientInfo.Config.Get("tasks", "sterilamp", null);
            string weekdays = WpfClientInfo.Config.Get("tasks", "sterilamp_weekday", null);

            /*
            List<string> results = new List<string>();
            if (time_range != null)
                results.Add(time_range);
            if (weekdays != null)
                results.Add(weekdays);
            return StringUtil.MakePathList(results, " ");
            */
            string result = "";
            if (string.IsNullOrEmpty(time_range) == false)
                result = time_range;
            if (string.IsNullOrEmpty(weekdays) == false)
                result += $" -weekday:{weekdays}";
            return result;
        }

        // 启动每日定时亮灯过程
        public static NormalResult StartPerdayTask()
        {
            // 初始化任务调度器
            TryInitialize();

            CancelSterilamp();
            WpfClientInfo.WriteInfoLog($"sterilamp 先关掉(可能亮着的)紫外灯");

            // 每日亮灯时间点
            string time_range = WpfClientInfo.Config.Get("tasks", "sterilamp", null);
            string weekday = WpfClientInfo.Config.Get("tasks", "sterilamp_weekday", null);

            WpfClientInfo.WriteInfoLog($"sterilamp time_range={time_range}");
            WpfClientInfo.WriteInfoLog($"sterilamp_weekday={weekday}");

            /*
            // testing
            string time_range = "1:00,9:00";
            string weekday = "*,-0,-6"; // +1,+2,+3,+4,+5
            */


            try
            {
                var times = ParseTimeList(time_range);

                WpfClientInfo.WriteInfoLog($"sterilamp time_list '{time_range}' 解析后得到 '{PerdayTime.ToString(times)}'");

                // 移除以前的全部同类任务
                {
                    List<string> names = new List<string>();
                    foreach (var s in JobManager.AllSchedules)
                    {
                        if (s.Name != null && s.Name.StartsWith("sterilamp"))
                            names.Add(s.Name);
                    }

                    foreach (var name in names)
                    {
                        JobManager.RemoveJob(name);
                    }
                }

                if (times.Count == 0)
                {
                    WpfClientInfo.WriteInfoLog($"sterilamp time_range 为空, 结束 StartPerdayTask()");
                    return new NormalResult();
                }

                // 安排任务
                int i = 0;
                foreach (var time in times)
                {
                    JobManager.AddJob(
                        () =>
                        {
                            WpfClientInfo.WriteInfoLog($"sterilamp time_range='{time_range}' weekday='{weekday}'");

                            if (InWeekday() == true)
                            {
                                _ = BeginSterilamp();
                                WpfClientInfo.WriteInfoLog("sterilamp job head 触发开灯");
                            }
                            else
                                WpfClientInfo.WriteInfoLog("sterilamp job head 触发，但因为当前时间不在 weekday 范围，未开灯");
                        },
                        s => s.WithName($"sterilamp{(i++).ToString()}").ToRunEvery(1).Days().At(time.Hour, time.Minute)
                    );
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"sterilamp 解析紫外灯时间点字符串时发现错误: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"sterilamp 解析紫外灯时间点字符串时发现错误: { ex.Message}"
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

        /* // 见 PerdayTime.ToString(times)
        public static string ToString(List<PerdayTime> times)
        {
            List<string> results = new List<string>();
            foreach (var time in times)
            {
                results.Add(time.ToString());
            }
            return StringUtil.MakePathList(results, ",");
        }
        */

        #region 紫外消毒

        static Task _sterilampTask = null;
        static CancellationTokenSource _cancelSterilamp = new CancellationTokenSource();

        // 开启紫外线灯
        public static bool BeginSterilamp()
        {
            if (_sterilampTask != null)
                return false;

            _cancelSterilamp?.Cancel();
            _cancelSterilamp?.Dispose();

            _cancelSterilamp = new CancellationTokenSource();
            _sterilampTask = SterilampAsync(_cancelSterilamp.Token);
            return true;
        }

        // 取消正在进行的紫外线灯
        public static void CancelSterilamp()
        {
            _cancelSterilamp?.Cancel();
        }

        // 紫外杀菌
        async static Task SterilampAsync(CancellationToken token)
        {
            ProgressWindow progress = null;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken, token))
            {
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "紫外线消毒";
                    progress.MessageText = "警告：紫外线对眼睛和皮肤有害";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "wide");
                    progress.BackColor = "yellow";
                    progress.Show();
                }));

                ShelfData.TrySetMessage(null, "即将开始紫外线消毒，正在倒计时 ...");

                try
                {
                    // 若当前为书柜界面，则需要检查是否有打开的门
                    if (ShelfData.OpeningDoorCount > 0)
                    {
                        var text = $"当前有 {ShelfData.OpeningDoorCount} 个柜门处于打开状态，暂时无法打开紫外灯";
                        App.CurrentApp.Speak(text);
                        return;
                    }

                    // 首先倒计时警告远离
                    App.CurrentApp.Speak("即将开始紫外线消毒，请马上远离书柜");
                    for (int i = 20; i > 0; i--)
                    {
                        if (cancel.Token.IsCancellationRequested)
                            return;
                        string text = $"({i}) 即将进行紫外线消毒，请迅速远离书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = text;
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                    }

                    App.Invoke(new Action(() =>
                    {
                        progress.BackColor = "red";
                        progress.MessageText = "正在进行紫外线消毒，请不要靠近书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                    }));

                    ShelfData.TrySetMessage(null, "正在进行紫外线消毒，请不要靠近书柜");

                    // TODO: 屏幕上可以显示剩余时间
                    // TODO: 背景色动画，闪动
                    RfidManager.TurnSterilamp("*", "turnOn");
                    DateTime end = DateTime.Now + TimeSpan.FromMinutes(10);
                    for (int i = 0; i < 3 * 10; i++)    // 10 分钟
                    {
                        App.CurrentApp.SpeakSequence("正在进行紫外线消毒，紫外灯对眼睛和皮肤有害，请不要靠近书柜");
                        if (cancel.Token.IsCancellationRequested)
                            break;
                        string timeText = $"剩余 {Convert.ToInt32((end - DateTime.Now).TotalMinutes)} 分钟";

                        if ((i % 3) == 0)
                            App.CurrentApp.SpeakSequence(timeText);

                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = $"({timeText}) 正在进行紫外线消毒，请不要靠近书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(20), cancel.Token);
                    }
                }
                finally
                {
                    RfidManager.TurnSterilamp("*", "turnOff");
                    App.CurrentApp.Speak("紫外灯已关闭");
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));

                    ShelfData.TrySetMessage(null, "紫外线消毒结束");

                    _sterilampTask = null;
                }
            }
        }

        #endregion


    }
}
