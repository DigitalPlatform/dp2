using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryClient
{
    public class ChargingHistoryLoader : IEnumerable
    {
        public string PatronBarcode { get; set; }
        public string TimeRange { get; set; }
        public string Actions { get; set; }
        public string Order { get; set; }
        public long Start = 0;
        public long Length = -1;

        // 注: SearchCharging() API 需要较长的 channel 超时参数，比如 30 秒
        public LibraryChannel Channel
        {
            get;
            set;
        }

        public Stop Stop
        {
            get;
            set;
        }

        // 获得满足条件的事项总数
        public long GetCount()
        {
            string strError = "";
            ChargingItemWrapper[] temp_results = null;
            long lRet = this.Channel.SearchCharging(
                this.Stop,
                this.PatronBarcode,
                this.TimeRange,
                this.Actions,
                this.Order,
                0,
                0,
                out temp_results,
                out strError);
            if (lRet == -1)
                throw new ChannelException(this.Channel.ErrorCode, strError);
            return lRet;
        }

        public IEnumerator GetEnumerator()
        {
            long lStart = this.Start;
            long lLength = this.Length;
            long lHitCount = 0;

            int nGeted = 0;
            for (; ; )
            {
                string strError = "";
                ChargingItemWrapper[] temp_results = null;
                // 注: SearchCharging() API 需要较长的 channel 超时参数，比如 30 秒
                long lRet = this.Channel.SearchCharging(
                    this.Stop,
                    this.PatronBarcode,
                    this.TimeRange,
                    this.Actions,
                    this.Order,
                    lStart + nGeted,
                    lLength,
                    out temp_results,
                    out strError);
                if (lRet == -1)
                    throw new ChannelException(this.Channel.ErrorCode, strError);
                lHitCount = lRet;
                if (temp_results == null || temp_results.Length == 0)
                    break;

                foreach (ChargingItemWrapper wrapper in temp_results)
                {
                    yield return wrapper;
                }

                // 修正 lLength
                if (lLength != -1 && lHitCount < lStart + nGeted + lLength)
                    lLength -= lStart + nGeted + lLength - lHitCount;

                nGeted += temp_results.Length;
                if (nGeted >= lHitCount - lStart)
                    yield break;
                if (lLength != -1)
                    lLength -= temp_results.Length;

                if (lLength <= 0 && lLength != -1)
                    yield break;
            }
        }

        // 将 20120101 - 20151231 这样的日期字符串形态转换为 SearchCharging() API 能接受的时间范围字符串形态
        // 注意 SearchCharging() API 要的时间范围字符串，中间是一个波浪号而不是横杠
        public static string GetTimeRange(string strText)
        {
            string strStart = "";
            string strEnd = "";
            StringUtil.ParseTwoPart(strText, "-", out strStart, out strEnd);
            strStart = strStart.Trim();
            strEnd = strEnd.Trim();
            if (string.IsNullOrEmpty(strStart) == false)
            {
                DateTime time = DateTimeUtil.Long8ToDateTime(strStart);
                strStart = time.ToString("G");
            }
            if (string.IsNullOrEmpty(strEnd) == false)
            {
                DateTime time = DateTimeUtil.Long8ToDateTime(strEnd);
                strEnd = time.AddDays(1).ToString("G"); // 注意 ~ 后面是 < 的意思，所以要放到第二天的零点
            }

            return strStart + "~" + strEnd;
        }

    }
}
