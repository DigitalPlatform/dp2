using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;

namespace dp2SSL
{
    public class PerdayTask
    {
        public struct PerdayTime
        {
            public int Hour { get; set; }
            public int Minute { get; set; }
        }

        class ParseTimeResult : DigitalPlatform.NormalResult
        {
            public PerdayTime Time { get; set; }
        }

        static ParseTimeResult ParseTime(string strStartTime)
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
        static List<DayOfWeek> ParseDayOfWeekDef(string weekdays)
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

        // 启动每日定时亮灯过程
        public static NormalResult StartPerdayTask()
        {
            // 初始化任务调度器
            if (_initialized == false)
            {
                JobManager.Initialize();
                _initialized = true;
            }

            TurnBackLampOff();

            // 每日亮灯时段
            string time_range = WpfClientInfo.Config.Get("tasks", "lamp", null);
            string weekday = WpfClientInfo.Config.Get("tasks", "lamp_weekday", null);

            /*
            // testing
            string time_range = "1:00-9:00";
            string weekday = "*,-0,-6"; // +1,+2,+3,+4,+5
            */

            if (string.IsNullOrEmpty(time_range))
                return new NormalResult();

            try
            {
                var times = ParseTimeRange(time_range);
                var start = times[0];
                var end = times[1];

                // 专门检查是否在亮灯时间范围内
                DateTime now = DateTime.Now;
                DateTime current_start = GetTodayTime(start);
                DateTime current_end = GetTodayTime(end);
                if (now >= current_start && now <= current_end)
                {
                    if (InWeekday() == true)
                        TurnBackLampOn();
                }

                // 安排任务
                // 开灯
                JobManager.AddJob(
                    () =>
                    {
                        if (InWeekday() == true)
                            TurnBackLampOn();
                    },
                    s => s.ToRunEvery(1).Days().At(start.Hour, start.Minute)
                );
                // 关灯
                JobManager.AddJob(
                    () =>
                    {
                        if (InWeekday() == true)
                            TurnBackLampOff();
                    },
                    s => s.ToRunEvery(0).Days().At(end.Hour, end.Minute)
                );

                /*
                // Schedule a simple task to run at a specific time
                Schedule(() => { }).ToRunEvery(1).Days().At(21, 15);
                */
                return new NormalResult();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"解析每日亮灯时间范围字符串时发现错误: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"解析每日亮灯时间范围字符串时发现错误: { ex.Message}"
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
