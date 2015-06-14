#define NEWLOCK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace dp2Circulation
{
    /// <summary>
    /// 字符串高速缓存
    /// </summary>
    public class StringCache
    {
        /// <summary>
        /// 最多可以容纳的事项个数
        /// </summary>
        public int MaxItems = 1000;
        Hashtable items = new Hashtable();
#if NEWLOCK
        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
#else
        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒
#endif

        /// <summary>
        /// 检索一个事项
        /// </summary>
        /// <param name="strEntry">事项名</param>
        /// <returns>值</returns>
        public StringCacheItem SearchItem(string strEntry)
        {
#if NEWLOCK
            this.m_lock.EnterReadLock();
#else
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
#endif

            try
            {
                // return null; // 测试
                return (StringCacheItem)items[strEntry];
            }
            finally
            {
#if NEWLOCK
                this.m_lock.ExitReadLock();
#else
                this.m_lock.ReleaseReaderLock();
#endif
            }
        }

        // 得到行对象。如果不存在，则临时创建一个
        /// <summary>
        /// 得到行对象。如果不存在，则临时创建一个
        /// </summary>
        /// <param name="strEntry">事项名</param>
        /// <returns>已经存在的或者新创建的 StringCacheItem 对象</returns>
        public StringCacheItem EnsureItem(string strEntry)
        {
#if NEWLOCK
            this.m_lock.EnterWriteLock();
#else
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
#endif

            try
            {
                if (items.Count > MaxItems)
                    this.items.Clear();

                // 检查line事项是否存在
                StringCacheItem item = (StringCacheItem)items[strEntry];

                if (item == null)
                {
                    item = new StringCacheItem();
                    item.Key = strEntry;

                    items.Add(strEntry, item);
                }

                Debug.Assert(item != null, "line在这里应该!=null");

                return item;
            }
            finally
            {
#if NEWLOCK
                this.m_lock.ExitWriteLock();
#else
                this.m_lock.ReleaseWriterLock();
#endif
            }
        }

        /// <summary>
        /// 移走所有内容
        /// </summary>
        public void RemoveAll()
        {
#if NEWLOCK
            this.m_lock.EnterWriteLock();
#else
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
#endif

            try
            {
                items.Clear();
            }
            finally
            {
#if NEWLOCK
                this.m_lock.ExitWriteLock();
#else
                this.m_lock.ReleaseWriterLock();
#endif
            }
        }
    }

    /// <summary>
    /// 字符串缓存对象
    /// </summary>
    public class StringCacheItem
    {
        /// <summary>
        /// 名
        /// </summary>
        public string Key = "";
        /// <summary>
        /// 值
        /// </summary>
        public string Content = "";
    }
}
