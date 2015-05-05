using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform
{
    /// <summary>
    /// 时间进程
    /// </summary>
    public class ProgressEstimate
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime start_time;

        /// <summary>
        /// 经过的时间
        /// </summary>
        public TimeSpan delta_passed;

        long _start = 0;
        /// <summary>
        /// 开始位置
        /// </summary>
        public long StartPosition
        {
            get
            {
                return this._start;
            }
            set
            {
                this._start = value;
            }
        }

        long _end = 0;
        /// <summary>
        /// 结束位置
        /// </summary>
        public long EndPosition
        {
            get
            {
                return this._end;
            }
            set
            {
                this._end = value;
            }
        }

        /// <summary>
        /// 最近一次Estimate()操作的时间
        /// </summary>
        public DateTime last_time;  // 最近一次Estimate()操作的时间

        /// <summary>
        /// 最近一次显示过的文字
        /// </summary>
        public string Text = "";

        /// <summary>
        /// 构建文字
        /// </summary>
        /// <param name="lProgressValue">进度量</param>
        /// <returns>文字</returns>
        public string BuildText(long lProgressValue)
        {
            this.Text = "剩余时间 " + ProgressEstimate.Format(this.Estimate(lProgressValue)) + " 已经过时间 " + ProgressEstimate.Format(this.delta_passed);
            return this.Text;
        }

        /// <summary>
        /// 返回到目前为止总共花费的时间
        /// </summary>
        /// <returns>时间长度</returns>
        public TimeSpan GetTotalTime()
        {
            return DateTime.Now - this.start_time;
        }

        /// <summary>
        /// 开始进程
        /// </summary>
        public void StartEstimate()
        {
            start_time = DateTime.Now;
        }

        /// <summary>
        /// 设置位置范围
        /// </summary>
        /// <param name="lStart">开始位置</param>
        /// <param name="lEnd">结束位置</param>
        public void SetRange(long lStart, long lEnd)
        {
            _start = lStart;
            _end = lEnd;
        }

        // 
        /// <summary>
        /// 距离最近一次Estimate()的时间过去了一秒以上么?
        /// </summary>
        /// <returns>true 表示过去了一秒以上</returns>
        public bool OneSecondPassed()
        {
            if (DateTime.Now - this.last_time > new TimeSpan(0, 0, 1))
                return true;
            return false;
        }

        // 
        /// <summary>
        /// 计算后面还需要多少时间
        /// </summary>
        /// <param name="lValue">进度量</param>
        /// <returns>时间长度</returns>
        public TimeSpan Estimate(long lValue)
        {
            // 已经用掉的时间
            this.delta_passed = DateTime.Now - start_time;

            // 已经用掉的数值
            long lPassedValue = lValue - this._start;

            if (lPassedValue == 0)
                return new TimeSpan(0);

            // 剩下的
            long lRestValue = this._end - lValue;

            Debug.Assert(lRestValue >= 0, "");

            // 2014/3/22
            if (lRestValue < 0)
                lRestValue = 0;

            long lTicks = (long)(((double)delta_passed.Ticks / (double)lPassedValue) * (double)lRestValue);

            this.last_time = DateTime.Now;

            return new TimeSpan(lTicks);
        }

        /// <summary>
        /// 输出时间长度字符串
        /// </summary>
        /// <param name="delta">时间长度</param>
        /// <returns>字符串</returns>
        public static string Format(TimeSpan delta)
        {
            string strResult = "";

            if (delta.Ticks < 0)
                return "";

            if (delta.Days != 0)
                strResult += delta.Days.ToString() + "天 ";
            if (delta.Hours != 0)
                strResult += delta.Hours.ToString() + "小时 ";
            if (delta.Minutes != 0)
                strResult += delta.Minutes.ToString() + "分 ";

            strResult += delta.Seconds.ToString() + "秒 ";

            return strResult;
        }

    }
}
