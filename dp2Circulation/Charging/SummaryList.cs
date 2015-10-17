using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform;
using System.Threading;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    class SummaryTask
    {
        public string State
        {
            get;
            set;
        }

        public string Action
        {
            get;
            set;
        }

        public string ItemBarcode
        {
            get;
            set;
        }

        public string ConfirmItemRecPath
        {
            get;
            set;
        }

        public ChargingTask ChargingTask
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 用单独线程填写出纳任务事项的摘要
    /// </summary>
    internal class SummaryList : ThreadBase
    {
        public QuickChargingForm Container = null;

        List<SummaryTask> _tasks = new List<SummaryTask>();

        /// <summary>
        /// 通讯通道
        /// </summary>
        // public ExternalChannel Channel = null;
        public DigitalPlatform.Stop stop = null;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        internal void DoStop(object sender, StopEventArgs e)
        {
            
#if NO
            if (this.Channel != null && this.Channel.Channel != null)
                this.Channel.Channel.Abort();
#endif
            if (this.Container != null
                && this.Container._summaryChannel != null 
                && this.Container._summaryChannel.Channel != null)
                this.Container._summaryChannel.Channel.Abort();
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            int nOldCount = 0;
            List<SummaryTask> tasks = new List<SummaryTask>();
            List<SummaryTask> remove_tasks = new List<SummaryTask>();
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                nOldCount = this._tasks.Count;
                foreach (SummaryTask task in this._tasks)
                {
                        tasks.Add(task);
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (tasks.Count > 0)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("进行一轮任务处理...");
                stop.BeginLoop();
                try
                {

                    foreach (SummaryTask task in tasks)
                    {
                        if (this.Stopped == true)
                        {
                            // this.Container.SetColorList();  // 促使“任务已经暂停”显示出来
                            return;
                        }

                        if (stop != null && stop.State != 0)
                        {
                            this.Stopped = true;
                            // this.Container.SetColorList();  // 促使“任务已经暂停”显示出来
                            return;
                        }

                        if (task.State == "finish")
                            continue;

                        // bool bStop = false;
                        // 执行任务
                        if (task.Action == "get_item_summary")
                        {
                            LoadItemSummary(task);
                        }
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }

            //bool bChanged = false;
            if (remove_tasks.Count > 0)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
                try
                {
                    foreach (SummaryTask task in remove_tasks)
                    {
                        RemoveTask(task, false);
                    }
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }
            }
        }

        // 加入一个任务到列表中
        public void AddTask(SummaryTask task)
        {
            task.State = "";   // 表示等待处理
            if (this.m_lock.TryEnterWriteLock(500) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                this._tasks.Add(task);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 触发任务开始执行
            this.Activate();
        }

        public void RemoveTask(SummaryTask task, bool bLock = true)
        {
            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                this._tasks.Remove(task);
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ExitWriteLock();
            }
        }

        void LoadItemSummary(SummaryTask task)
        {
            string strError = "";
            string strSummary = "";
            int nRet = this.Container.GetBiblioSummary(task.ItemBarcode,
                task.ConfirmItemRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
            {
                strSummary = strError;
                task.State = "error";
            }
            else
                task.State = "finish";

            this.Container.AsyncFillItemSummary(task.ChargingTask, strSummary);
        }

        public void Close()
        {
            if (stop != null)
                stop.DoStop();
            this.eventClose.Set();
            Stopped = true;
        }

        public void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                this._tasks.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 清除和指定的ChargingTask关联的那些任务
        public void ClearRelativeTasks(List<ChargingTask> tasks)
        {
            List<SummaryTask> remove_list = new List<SummaryTask>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                foreach (SummaryTask task in this._tasks)
                {
                    if (tasks.IndexOf(task.ChargingTask) != -1)
                        remove_list.Add(task);
                }

                foreach(SummaryTask task in remove_list)
                {
                    this._tasks.Remove(task);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

    }
}
