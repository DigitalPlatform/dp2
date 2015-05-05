using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Permissions;

namespace DigitalPlatform
{
    /// <summary>
    /// 线程基础类
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class ThreadBase
    {
        private bool m_bStopThread = true;
        protected Thread _thread = null;

        public AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        public AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        // internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        public int PerTime = 1000;   // 1 秒 5 * 60 * 1000;	// 5 分钟

#if NO
        public virtual void Clear()
        {
        }
#endif

        void ThreadMain()
        {
            m_bStopThread = false;
            try
            {
                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (m_bStopThread == false)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, PerTime, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        break;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();

                    }
                    else if (index == 0)
                    {
                        break;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();
                    }
                }

                return;
            }
            finally
            {
                m_bStopThread = true;
                this._thread = null;
            }
        }

        public virtual void Worker()
        {
        }

        public bool Stopped
        {
            get
            {
                return m_bStopThread;
            }
            set
            {
                m_bStopThread = value;
            }
        }

        public virtual void StopThread(bool bForce)
        {
            if (this._thread == null)
                return;

            // 如果以前在做，立即停止
            // this.Clear();

            m_bStopThread = true;
            this.eventClose.Set();

            if (bForce == true)
            {
                if (this._thread != null)
                {
                    if (!this._thread.Join(2000))
                        this._thread.Abort();
                    this._thread = null;
                }
            }
        }

        public void BeginThread()
        {
            if (this._thread != null)
                return;

            // 如果以前在做，立即停止
            StopThread(true);

            this.eventActive.Set();
            this.eventClose.Reset();

            this._thread = new Thread(new ThreadStart(this.ThreadMain));
            this._thread.Start();
        }

        public void Activate()
        {
            eventActive.Set();
        }

    }
}
