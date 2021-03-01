using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentScheduler;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using static dp2SSL.LampPerdayTask;

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
            List<string> results = new List<string>();
            if (time_range != null)
                results.Add(time_range);
            if (weekdays != null)
                results.Add(weekdays);
            return StringUtil.MakePathList(results, " ");
        }

        // 启动每日定时亮灯过程
        public static NormalResult StartPerdayTask()
        {
            // 初始化任务调度器
            TryInitialize();

            App.CurrentApp.CancelSterilamp();
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
                                _ = App.CurrentApp.BeginSterilamp();
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
    }
}
