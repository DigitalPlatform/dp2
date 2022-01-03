using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform
{
    /// <summary>
    /// 仿真 ReaderWriterLock 类的函数，实际用 ReaderWriterLockSlim 实现
    /// </summary>
    public class MyReaderWriterLock : ReaderWriterLockSlim
    {
        public void AcquireWriterLock(int nTimeOut)
        {
            if (this.TryEnterWriteLock(nTimeOut) == false)
                throw new ApplicationException("加写锁时失败。Timeout=" + nTimeOut.ToString());
        }

        public void ReleaseWriterLock()
        {
            this.ExitWriteLock();
        }

        public void AcquireReaderLock(int nTimeOut)
        {
            if (this.TryEnterReadLock(nTimeOut) == false)
                throw new ApplicationException("加读锁时失败。Timeout=" + nTimeOut.ToString());
        }

        public void ReleaseReaderLock()
        {
            this.ExitReadLock();
        }

        public void AcquireUpgradeableReaderLock(int nTimeOut)
        {
            if (this.TryEnterUpgradeableReadLock(nTimeOut) == false)
                throw new ApplicationException("加可升级读锁时失败。Timeout=" + nTimeOut.ToString());
        }

        public void ReleaseUpgradeableReaderLock()
        {
            this.ExitUpgradeableReadLock();
        }

        public LockCookie UpgradeToWriterLock(int nTimeOut)
        {
            if (this.TryEnterWriteLock(nTimeOut) == false)
                throw new ApplicationException("加写锁时失败。Timeout=" + nTimeOut.ToString());

            return new LockCookie();
        }

        public void DowngradeFromWriterLock(ref LockCookie lc)
        {
            this.ExitWriteLock();
        }
    }
}
