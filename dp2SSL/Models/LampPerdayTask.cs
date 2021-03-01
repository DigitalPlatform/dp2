using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;

using FluentScheduler;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// 每日亮灯任务
    /// </summary>
    public class LampPerdayTask
    {
        public struct PerdayTime
        {
            public int Hour { get; set; }
            public int Minute { get; set; }

            public override string ToString()
            {
                return $"{Hour}:{Minute}";
            }

            // 2021/3/1
            public static string ToString(List<PerdayTime> times)
            {
                List<string> results = new List<string>();
                foreach(var time in times)
                {
                    results.Add(time.ToString());
                }

                return StringUtil.MakePathList(results, ",");
            }
        }

        public class ParseTimeResult : DigitalPlatform.NormalResult
        {
            public PerdayTime Time { get; set; }
        }

        public static ParseTimeResult ParseTime(string strStartTime)
        {
            string strHour = "";
            string strMinute = "";

            int nRet = strStartTime.IndexOf(":");
            if (nRet == -1)
            {
                strHour = strStartTime.Trim();
                strMinute = "00";
            }
            else
            {
                strHour = strStartTime.Substring(0, nRet).Trim();
                strMinute = strStartTime.Substring(nRet + 1).Trim();
            }

            PerdayTime time = new PerdayTime();
            try
            {
                time.Hour = Convert.ToInt32(strHour);
                time.Minute = Convert.ToInt32(strMinute);
            }
            catch
            {
                return new ParseTimeResult
                {
                    Value = -1,
                    ErrorInfo = "时间值 " + strStartTime + " 格式不正确。应为 hh:mm"
                };
            }

            return new ParseTimeResult
            {
                Time = time,
            };
        }

        static List<PerdayTime> ParseTimeRange(string range)
        {
            var parts = StringUtil.ParseTwoPart(range, "-");
            if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                throw new ArgumentException($"时间范围字符串 '{range}' 不合法。横杠左右两侧不应为空");
            var result1 = ParseTime(parts[0]);
            var result2 = ParseTime(parts[1]);

            if (result1.Value == -1)
                throw new ArgumentException(result1.ErrorInfo);
            if (result2.Value == -1)
                throw new ArgumentException(result2.ErrorInfo);

            List<PerdayTime> results = new List<PerdayTime>();
            results.Add(result1.Time);
            results.Add(result2.Time);
            return results;
        }

        public static bool GetBackLampState()
        {
            return ShelfData.GetLampState("back");
        }

        public static void TurnBackLampOn()
        {
            ShelfData.TurnLamp("back", "on");
        }

        public static void TurnBackLampOff()
        {
            ShelfData.TurnLamp("back", "off");
        }

        static bool _initialized = false;

        // 修改每日定时亮灯参数
        // -range:8:00-11:00 -weekday:*,-0,-6
        // -range:8:00-11:00 -weekday:+1,+2,+3,+4,+5
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

            // 每日亮灯时段
            WpfClientInfo.Config.Set("tasks", "lamp", time_range);
            WpfClientInfo.Config.Set("tasks", "lamp_weekday", weekdays);
            return StartPerdayTask();
        }

        public static string GetPerdayTask()
        {
            // 每日亮灯时段
            string time_range = WpfClientInfo.Config.Get("tasks", "lamp", null);
            string weekdays = WpfClientInfo.Config.Get("tasks", "lamp_weekday", null);
            List<string> results = new List<string>();
            if (time_range != null)
                results.Add(time_range);
            if (weekdays != null)
                results.Add(weekdays);
            return StringUtil.MakePathList(results, " ");
        }

        static DateTime GetTodayTime(PerdayTime time)
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Minute, 0);
        }

        /*
        //
        // 摘要:
        //     表示星期日。
        Sunday = 0,
        //
        // 摘要:
        //     表示星期一。
        Monday = 1,
        //
        // 摘要:
        //     表示星期二。
        Tuesday = 2,
        //
        // 摘要:
        //     表示星期三。
        Wednesday = 3,
        //
        // 摘要:
        //     表示星期四。
        Thursday = 4,
        //
        // 摘要:
        //     表示星期五。
        Friday = 5,
        //
        // 摘要:
        //     表示星期六。
        Saturday = 6
        * */
        // 将周内日期定义转换为一个描述应包含日期的集合
        public static List<DayOfWeek> ParseDayOfWeekDef(string weekdays)
        {
            List<DayOfWeek> results = new List<DayOfWeek>();
            List<string> parts = StringUtil.SplitList(weekdays, ',');
            foreach (string part in parts)
            {
                string s = part.Trim();
                if (s == "*")
                {
                    // 星号代表一周内所有日子
                    for (int i = 0; i < 7; i++)
                    {
                        results.Add((DayOfWeek)i);
                    }
                    continue;
                }

                char action = '+';
                // 先分离加减号
                if (s[0] == '+' || s[0] == '-')
                {
                    action = s[0];
                    s = s.Substring(1);
                }
                DayOfWeek day;
                if (s.Length == 1 && char.IsDigit(s[0]))
                {
                    if (int.TryParse(s, out int v) == false)
                        throw new Exception($"'{s}' 是无法识别的周内日期名。应在 0-6 范围");

                    day = (DayOfWeek)v;
                }
                else if (Enum.TryParse<DayOfWeek>(s, out day) == false)
                    throw new Exception($"'{s}' 是无法识别的周内日期名");

                if (action == '+')
                {
                    if (results.IndexOf(day) == -1)
                        results.Add(day);
                }
                else
                {
                    // '-'
                    if (results.IndexOf(day) != -1)
                        results.Remove(day);
                }
            }

            return results;
        }

        public static void TryInitialize()
        {
            // 初始化任务调度器
            if (_initialized == false)
            {
                JobManager.Initialize();
                _initialized = true;
                WpfClientInfo.WriteInfoLog($"lamp 初始化 JobManager");
            }
        }

        // 启动每日定时亮灯过程
        public static NormalResult StartPerdayTask()
        {
            TryInitialize();

            TurnBackLampOff();
            WpfClientInfo.WriteInfoLog($"lamp 先关掉书柜灯");

            // 每日亮灯时段
            string time_range = WpfClientInfo.Config.Get("tasks", "lamp", null);
            string weekday = WpfClientInfo.Config.Get("tasks", "lamp_weekday", null);

            WpfClientInfo.WriteInfoLog($"lamp time_range={time_range}");
            WpfClientInfo.WriteInfoLog($"lamp weekday={weekday}");

            /*
            // testing
            string time_range = "1:00-9:00";
            string weekday = "*,-0,-6"; // +1,+2,+3,+4,+5
            */

            // 移除以前的全部同类任务
            {
                List<string> names = new List<string>();
                foreach (var s in JobManager.AllSchedules)
                {
                    if (s.Name != null && s.Name.StartsWith("lamp_"))
                        names.Add(s.Name);
                }

                foreach (var name in names)
                {
                    JobManager.RemoveJob(name);
                }
            }

            if (string.IsNullOrEmpty(time_range))
            {
                WpfClientInfo.WriteInfoLog($"lamp time_range 为空, 结束 StartPerdayTask()");
                return new NormalResult();
            }

            try
            {
                var times = ParseTimeRange(time_range);
                var start = times[0];
                var end = times[1];

                WpfClientInfo.WriteInfoLog($"lamp time_range '{time_range}' 解析后得到 start='{start.ToString()}' end='{end.ToString()}'");

                // 专门检查是否在亮灯时间范围内
                DateTime now = DateTime.Now;
                DateTime current_start = GetTodayTime(start);
                DateTime current_end = GetTodayTime(end);
                if (now >= current_start && now <= current_end)
                {
                    WpfClientInfo.WriteInfoLog($"lamp 当前时间 '{now.ToString()}' 处在 current_start '{current_start.ToString()}' 和 current_end '{current_end.ToString()}' 之间，下面进一步判断 weekday");
                    if (InWeekday() == true)
                    {
                        TurnBackLampOn();
                        WpfClientInfo.WriteInfoLog("lamp 当前时间在 weekday 范围内，因此开灯");
                    }
                    else
                    {
                        WpfClientInfo.WriteInfoLog("lamp 当前时间不在 weekday 范围内，因此未开灯");
                    }
                }

                // 安排任务
                // 开灯
                JobManager.AddJob(
                    () =>
                    {
                        WpfClientInfo.WriteInfoLog($"lamp time_range='{time_range}' weekday='{weekday}'");

                        if (InWeekday() == true)
                        {
                            TurnBackLampOn();
                            WpfClientInfo.WriteInfoLog("lamp job head 触发开灯");
                        }
                        else
                            WpfClientInfo.WriteInfoLog("lamp job head 触发，但因为当前时间不在 weekday 范围，未开灯");
                    },
                    s => s.WithName("lamp_on").ToRunEvery(1).Days().At(start.Hour, start.Minute)
                );
                // 关灯
                JobManager.AddJob(
                    () =>
                    {
                        WpfClientInfo.WriteInfoLog($"lamp time_range='{time_range}' weekday='{weekday}'");

                        if (InWeekday() == true)
                        {
                            TurnBackLampOff();
                            WpfClientInfo.WriteInfoLog("lamp job tail 触发关灯");
                        }
                        else
                            WpfClientInfo.WriteInfoLog("lamp job tail 触发，但因为当前时间不在 weekday 范围，未关灯");

                    },
                    s => s.WithName("lamp_off").ToRunEvery(1).Days().At(end.Hour, end.Minute)
                );

                /*
                // Schedule a simple task to run at a specific time
                Schedule(() => { }).ToRunEvery(1).Days().At(21, 15);
                */
                return new NormalResult();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"lamp 解析每日亮灯时间范围字符串时发现错误: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"lamp 解析每日亮灯时间范围字符串时发现错误: { ex.Message}"
                };
            }

            // 检查当前日期是否在允许之列
            bool InWeekday()
            {
                if (string.IsNullOrEmpty(weekday))
                    return true;
                DateTime now = DateTime.Now;
                var days = ParseDayOfWeekDef(weekday);
                if (days.IndexOf(now.DayOfWeek) == -1)
                    return false;
                return true;
            }
        }
    }
}
