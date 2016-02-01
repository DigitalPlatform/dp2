// #define OLD_LOCK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DigitalPlatform
{
    // 记录锁集合
    public class RecordLockCollection
    {
#if OLD_LOCK
        ReaderWriterLock m_lock = new ReaderWriterLock();
#else
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
#endif

        static int m_nLockTimeout = 5000;	// 5000=5秒

        Hashtable RecordLocks = new Hashtable();

        public RecordLockCollection()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public int LockTimeout
        {
            get
            {
                return m_nLockTimeout;
            }
            set
            {
                m_nLockTimeout = value;
            }
        }

        public int Count
        {
            get
            {
                return this.RecordLocks.Count;
            }
        }

        void _lockForWrite(int nTimeout)
        {
#if OLD_LOCK
            this.m_lock.AcquireWriterLock(nTimeout);
#else
            if (this.m_lock.TryEnterWriteLock(nTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
#endif
        }

        void _unlockForWrite()
        {
#if OLD_LOCK
            this.m_lock.ReleaseWriterLock();
#else
            this.m_lock.ExitWriteLock();
#endif
        }

        void _lockForRead(int nTimeout)
        {
#if OLD_LOCK
            this.m_lock.AcquireWriterLock(nTimeout);
#else
            if (this.m_lock.TryEnterReadLock(nTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
#endif
        }

        void _unlockForRead()
        {
#if OLD_LOCK
            this.m_lock.ReleaseReaderLock();
#else
            this.m_lock.ExitReadLock();
#endif
        }

        // 获得锁对象
        RecordLock GetLock(string strID,
            bool bAutoCreate = true)
        {
            // 加写锁
            this._lockForWrite(m_nLockTimeout);
            try
            {
                RecordLock reclock = (RecordLock)RecordLocks[strID];

                if (reclock == null)
                {
                    if (bAutoCreate == true)
                    {
                        reclock = new RecordLock();
                        reclock.m_strID = strID;
                        RecordLocks.Add(strID, reclock);
                    }
                    else
                        return null;
                }

                Interlocked.Increment(ref reclock.m_nUseCount);
                // Debug.WriteLine("record lock count " + RecordLocks.Count);
                return reclock;
            }
            finally
            {
                this._unlockForWrite();
            }
        }

        // 试图移走锁对象
        // 应当在RecordLock:m_lock解锁以后进行
        void TryRemoveLock(RecordLock reclock)
        {
            // 加写锁
            this._lockForWrite(m_nLockTimeout);
            try
            {
                int nRet = Interlocked.Increment(ref reclock.m_nUseCount);
                if (nRet == 1) // 说明增量以前为0
                {
                    this.RecordLocks.Remove(reclock.m_strID);
                }
                else
                {
                    Interlocked.Decrement(ref reclock.m_nUseCount);
                }
            }
            finally
            {
                this._unlockForWrite();
            }


        }

        // 读锁定
        public void LockForRead(string strID)
        {
            LockForRead(strID, RecordLock.m_nLockTimeout);
        }

        // 读锁定
        public void LockForRead(string strID,
            int nTimeOut)
        {
            RecordLock reclock = GetLock(strID);

            // Interlocked.Increment(ref reclock.m_nUseCount);

            // 加读锁
            try
            {
                reclock._lockForRead(nTimeOut);
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref reclock.m_nUseCount);
                // 是否还要删除?

                throw ex;
            }
        }

        public void UnlockForRead(string strID)
        {
            RecordLock reclock = GetLock(strID, false);

            if (reclock == null)
                throw new Exception("id '" + strID + "' 没有找到对应的记录锁");

            try
            {
                reclock._unlockForRead();
            }
            finally
            {

                Interlocked.Decrement(ref reclock.m_nUseCount);
                Interlocked.Decrement(ref reclock.m_nUseCount);

                TryRemoveLock(reclock);
            }
        }

        // 写锁定一批 ID
        // parameters:
        //      nTimeOut    如果 == 0，表示使用缺省的 timeout 值
        public void LockForWrite(ref List<string> ids,
    int nTimeOut = 0)
        {
            if (nTimeOut == 0)
                nTimeOut = RecordLock.m_nLockTimeout;

            ids.Sort();
            List<string> succeeds = new List<string>();
            bool bSucceed = false;
            try
            {
                foreach (string id in ids)
                {
                    LockForWrite(id, nTimeOut);
                    succeeds.Add(id);
                }
                bSucceed = true;
            }
            finally
            {
                // 如果中途遇到异常，要把已经枷锁成功的部分解锁
                if (bSucceed == false)
                {
                    foreach (string id in succeeds)
                    {
                        UnlockForWrite(id);
                    }
                }
            }
        }

        public void UnlockForWrite(List<string> ids)
        {
            foreach (string id in ids)
            {
                UnlockForWrite(id);
            }
        }

        // 写锁定
        public void LockForWrite(string strID)
        {
            LockForWrite(strID, RecordLock.m_nLockTimeout);
        }

        // 写锁定
        public void LockForWrite(string strID,
            int nTimeOut)
        {
            RecordLock reclock = GetLock(strID);

            // Interlocked.Increment(ref reclock.m_nUseCount);

            // 加写锁
            try
            {
                reclock._lockForWrite(nTimeOut);
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref reclock.m_nUseCount);
                // 是否还要删除?

                throw ex;
            }
        }

        public void UnlockForWrite(string strID)
        {
            RecordLock reclock = GetLock(strID, false);

            if (reclock == null)
                throw new Exception("id '"+strID+"' 没有找到对应的记录锁");

            try
            {
                reclock._unlockForWrite();
            }
            finally
            {
                Interlocked.Decrement(ref reclock.m_nUseCount);
                Interlocked.Decrement(ref reclock.m_nUseCount);

                TryRemoveLock(reclock);
            }
        }

        // 输出到文本
        public string DumpText()
        {
            // 加读锁
            this._lockForRead(m_nLockTimeout);
            try
            {
                string strResult = "";

                foreach (string key in RecordLocks.Keys)
                {
                    RecordLock onelock = (RecordLock)this.RecordLocks[key];

                    strResult += "id='" + onelock.m_strID + "' usecount='" + Convert.ToString(onelock.m_nUseCount) + "' hashcode='" + onelock.GetLockHashCode() + "'\r\n";
                }

                return strResult;
            }
            finally
            {
                this._unlockForRead();
            }
        }

    }

    /// <summary>
    /// 记录锁
    /// </summary>
    public class RecordLock
    {
#if OLD_LOCK
        private ReaderWriterLock m_lock = new ReaderWriterLock();
#else
        private ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
#endif
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        internal string m_strID;
        internal int m_nUseCount = 0;

        public RecordLock()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        internal void _lockForWrite(int nTimeout)
        {
#if OLD_LOCK
            this.m_lock.AcquireWriterLock(nTimeout);
#else
            if (this.m_lock.TryEnterWriteLock(nTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
#endif
        }

        internal void _unlockForWrite()
        {
#if OLD_LOCK
            this.m_lock.ReleaseWriterLock();
#else
            this.m_lock.ExitWriteLock();
#endif
        }

        internal void _lockForRead(int nTimeout)
        {
#if OLD_LOCK
            this.m_lock.AcquireWriterLock(nTimeout);
#else
            if (this.m_lock.TryEnterReadLock(nTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
#endif
        }

        internal void _unlockForRead()
        {
#if OLD_LOCK
            this.m_lock.ReleaseReaderLock();
#else
            this.m_lock.ExitReadLock();
#endif
        }

        public int GetLockHashCode()
        {
            return this.m_lock.GetHashCode();
        }
    }
}
