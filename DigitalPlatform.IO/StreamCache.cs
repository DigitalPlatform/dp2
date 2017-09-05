using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// Stream 对象的缓存。用于加快连续读、写时候打开文件和移动文件指针操作的速度
    /// </summary>
    public class StreamCache
    {
        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        List<StreamItem> _items = new List<StreamItem>();

        public StreamItem FindItem(string strFilePath)
        {
            m_lock.EnterReadLock();
            try
            {
                foreach (StreamItem item in _items)
                {
                    if (item.FilePath == strFilePath)
                    {
                        int v = item.IncUse();
                        if (v == 1)
                        {
                            return item;
                        }
                        else
                            item.DecUse();
                    }
                }
            }
            finally
            {
                m_lock.ExitReadLock();
            }

            return null;
        }

        public StreamItem NewItem(string strFilePath)
        {
            StreamItem item = new StreamItem();
            item.Touch();
            item.FilePath = strFilePath;
            item.FileStream = File.Open(
    strFilePath,
    FileMode.OpenOrCreate,
    FileAccess.Write,
    FileShare.ReadWrite);
            item.IncUse();
            m_lock.EnterWriteLock();
            try
            {
                _items.Add(item);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
            return item;
        }

        public StreamItem GetWriteStream(string strFilePath)
        {
            StreamItem item = this.FindItem(strFilePath);
            if (item != null)
            {
                item.Touch();
                return item;
            }

            return NewItem(strFilePath);
        }

        public void ReturnWriteStream(StreamItem item)
        {
            item.DecUse();
        }

        public void ClearAll()
        {
            foreach (StreamItem item in _items)
            {
                item.FileStream.Close();
                item.FileStream = null;
            }

            _items.Clear();
        }

        // 清除闲置时间过长的事项
        public void ClearIdle(TimeSpan delta)
        {
            DateTime now = DateTime.Now;

            List<StreamItem> delete_items = new List<StreamItem>();
            List<StreamItem> all_items = new List<StreamItem>();

            // 加锁
            m_lock.EnterReadLock();
            try
            {
                all_items.AddRange(_items);
            }
            finally
            {
                m_lock.ExitReadLock();
            }

            foreach (StreamItem item in all_items)
            {
                if (now - item.LastTime > delta)
                {
                    int v = item.IncUse();
                    if (v == 1)
                        delete_items.Add(item); // item 此时处于 InUse 状态，以后也不会有人用到它
                    else
                        item.DecUse();
                }
            }

            // 加锁
            m_lock.EnterWriteLock();
            try
            {
                foreach (StreamItem item in delete_items)
                {
                    _items.Remove(item);
                }
            }
            finally
            {
                m_lock.ExitWriteLock();
            }

            foreach (StreamItem item in delete_items)
            {
                item.FileStream.Close();
                item.FileStream = null;
            }
        }
    }

    public class StreamItem
    {
        public string FilePath { get; set; }
        public FileStream FileStream { get; set; }
        public DateTime LastTime { get; set; }

        int _useCount = 0;

        // return:
        //      操作后的值
        public int IncUse()
        {
            return Interlocked.Increment(ref _useCount);
        }

        // return:
        //      操作后的值
        public int DecUse()
        {
            return Interlocked.Decrement(ref _useCount);
        }

        public void Touch()
        {
            this.LastTime = DateTime.Now;
        }
    }
}
