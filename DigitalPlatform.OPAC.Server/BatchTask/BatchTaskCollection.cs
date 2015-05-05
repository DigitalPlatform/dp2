using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DigitalPlatform.OPAC.Server
{
    public class BatchTaskCollection : List<BatchTask>
    {
        // 数组锁
        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒


        public new void Clear()
        {
            this.Close();
        }

        public void Close()
        {
            for (int i = 0; i < this.Count; i++)
            {
                BatchTask task = this[i];
                task.Close();
            }

            base.Clear();
        }

        // 根据任务名获得一个任务对象
        // 包装版本
        // 锁定安全版本
        // 多线程：安全
        public BatchTask GetBatchTask(string strName)
        {
            return GetBatchTask(strName, true);
        }

        // 内部版本
        internal BatchTask GetBatchTask(string strName,
            bool bLock)
        {
            if (bLock == true)
                this.m_lock.AcquireReaderLock(m_nLockTimeout);

            try
            {

                for (int i = 0; i < this.Count; i++)
                {
                    BatchTask task = this[i];
                    if (task.Name == strName)
                        return task;
                }

                return null;
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ReleaseReaderLock();
            }
        }

        // 加入一个任务对象到容器
        // 多线程：安全
        public new void Add(BatchTask task)
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            if (GetBatchTask(task.Name, false) != null)
                throw new Exception("任务 '" + task.Name + "' 被重复加入容器。");

            try
            {
                base.Add(task);
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }
    }
}
