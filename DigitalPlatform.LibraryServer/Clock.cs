using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;

using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    public class Clock
    {
        public ReaderWriterLock m_lock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        TimeSpan clockdelta = new TimeSpan(0);  // 逻辑上的一个“流通时钟”和当前服务器机器时钟的差额


        // 和本机时钟的偏移Ticks量
        public long Delta
        {
            get
            {
                return clockdelta.Ticks;
            }
            set
            {
                clockdelta = new TimeSpan(value);
            }
        }

        // 设置流通时钟
        // 外部使用
        // paramters:
        //      strTime RFC1123格式
        public int SetClock(string strTime,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strTime) == true)
            {
                this.m_lock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    // 消除差异
                    this.clockdelta = new TimeSpan(0);
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
                return 0;
            }

            DateTime time;

            try
            {
                time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
            }
            catch
            {
                strError = "日期时间字符串 '" + strTime + "' 格式错误。应符合RFC1123格式要求。";
                return -1;
            }

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.clockdelta = time - DateTime.UtcNow;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            return 0;
        }

        // 获得流通时钟 RFC1123格式 有时区信息
        // 外部使用
        public string GetClock()
        {
            DateTime time;
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {
                time = DateTime.Now + this.clockdelta;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            return DateTimeUtil.Rfc1123DateTimeStringEx(time);
        }

        public void Reset()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.clockdelta = new TimeSpan(0); 
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

        }

        // 本地时间
        public DateTime Now
        {
            get
            {
                this.m_lock.AcquireReaderLock(m_nLockTimeout);
                try
                {
                    return DateTime.Now + this.clockdelta;
                }
                finally
                {
                    this.m_lock.ReleaseReaderLock();
                }
            }
        }

        // UTC 时间
        public DateTime UtcNow
        {
            get
            {
                this.m_lock.AcquireReaderLock(m_nLockTimeout);
                try
                {
                    return DateTime.UtcNow + this.clockdelta;
                }
                finally
                {
                    this.m_lock.ReleaseReaderLock();
                }
            }
        }

    }
}
