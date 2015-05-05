using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace DigitalPlatform.OPAC.Server
{
    // IP -- Session 数量 对照表
    // TODO: 总量超了怎么办？不允许新增，或者再增加主动释放机制?
    public class IpTable : Hashtable
    {
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        int _nMaxCount = 10000;

        public int MAX_SESSIONS_PER_IP = 50;   // 每个 IP 地址最大的 Session 数量

        // 如果 IP 事项总数超过限额，会抛出异常
        public long IncIpCount(string strIP, int nDelta)
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                return _incIpCount(strIP, nDelta);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 增量 IP 统计数字
        // 如果 IP 事项总数超过限额，会抛出异常
        long _incIpCount(string strIP, int nDelta)
        {
            long v = 0;
            if (this.ContainsKey(strIP) == true)
                v = (long)this[strIP];
            else
            {
                if (this.Count > _nMaxCount
                    && v + nDelta != 0)
                    throw new Exception("IP 条目数量超过 " + _nMaxCount.ToString());
            }

            if (v + nDelta == 0)
                this.Remove(strIP); // 及时移走计数器为 0 的条目，避免 hashtable 尺寸太大
            else
                this[strIP] = v + nDelta;

            return v;   // 返回增量前的数字
        }
    }
}
