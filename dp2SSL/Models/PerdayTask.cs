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
        public static NormalResult ChangePerdayTask(string time_range)
        {
            // 每日亮灯时段
            WpfClientInfo.Config.Set("tasks", "lamp", time_range);
            return StartPerdayTask();
        }

        public static string GetPerdayTask()
        {
            // 每日亮灯时段
            return WpfClientInfo.Config.Get("tasks", "lamp", null);
        }

        static DateTime GetTodayTime(PerdayTime time)
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Minute, 0);
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
                    TurnBackLampOn();
                }

                // 安排任务
                // 开灯
                JobManager.AddJob(
                    () =>
                    {
                        TurnBackLampOn();
                    },
                    s => s.ToRunEvery(1).Days().At(start.Hour, start.Minute)
                );
                // 关灯
                JobManager.AddJob(
                    () =>
                    {
                        TurnBackLampOff();
                    },
                    s => s.ToRunEvery(1).Days().At(end.Hour, end.Minute)
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
        }
    }
}
