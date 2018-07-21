using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        int MAX_ITEMS = 100;

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        List<StreamItem> _items = new List<StreamItem>();

        public StreamCache(int nMaxItems)
        {
            MAX_ITEMS = nMaxItems;
        }

        public void FileDelete(string filename)
        {
            this.ClearItems(filename);
            File.Delete(filename);
        }

        public void FileDeleteIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                this.ClearItems(filename);
                File.Delete(filename);
            }
        }

        // 改名
        public void FileMove(string strSourceFileName, string strFileName)
        {
            this.ClearItems(strSourceFileName);
            this.ClearItems(strFileName);
            File.Move(strSourceFileName, strFileName);
        }

        public void ClearItems(string strFilePath)
        {
            List<StreamItem> items = new List<StreamItem>();
            m_lock.EnterReadLock();
            try
            {
                foreach (StreamItem item in _items)
                {
                    if (item.FilePath == strFilePath)
                    {
                        items.Add(item);
                    }
                }
            }
            finally
            {
                m_lock.ExitReadLock();
            }

            if (items.Count > 0)
            {
                m_lock.EnterWriteLock();
                try
                {
                    foreach (StreamItem item in items)
                    {
                        _items.Remove(item);
                        if (item.FileStream != null)
                            item.FileStream.Close();
                    }
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }

        public StreamItem FindItem(string strFilePath, FileAccess access)
        {
            m_lock.EnterReadLock();
            try
            {
                foreach (StreamItem item in _items)
                {
                    if (item.FilePath == strFilePath && item.FileAccess == access)
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

        public StreamItem NewItem(string strFilePath,
            FileMode mode,
            FileAccess access,
            bool bAddToCollection = true)
        {
            // 防备尺寸过大
            if (bAddToCollection && _items.Count > MAX_ITEMS)
                ClearAll();

            StreamItem item = new StreamItem();
            item.Fly = !bAddToCollection;
            item.FileAccess = access;
            item.Touch();
            item.FilePath = strFilePath;
            item.FileStream = File.Open(
    strFilePath,
    mode,   // FileMode.OpenOrCreate,
    access, // FileAccess.Write,
    FileShare.ReadWrite);
            item.IncUse();

            if (bAddToCollection)
            {
                m_lock.EnterWriteLock();
                try
                {
                    _items.Add(item);
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
            Debug.Assert(item.FileStream != null, "");
            return item;
        }

        public StreamItem GetStream(string strFilePath,
            FileMode mode,
            FileAccess access,
            bool bAddToCollection = true)
        {
            if (bAddToCollection == false)
            {
                StreamItem item = NewItem(strFilePath, mode, access, false);
                Debug.Assert(item.FileStream != null, "");
                return item;
            }

            {
                StreamItem item = this.FindItem(strFilePath, access);
                if (item != null)
                {
                    item.Touch();
                    Debug.Assert(item.FileStream != null, "");
                    return item;
                }

                item = NewItem(strFilePath, mode, access);
                Debug.Assert(item.FileStream != null, "");
                return item;
            }
        }

        public StreamItem GetWriteStream(string strFilePath, bool bAddToCollection = true)
        {
            return GetStream(strFilePath, FileMode.OpenOrCreate, FileAccess.Write, bAddToCollection);
        }
#if NO
        public StreamItem GetWriteStream(string strFilePath)
        {
            FileAccess access = FileAccess.Write;
            StreamItem item = this.FindItem(strFilePath, access);
            if (item != null)
            {
                item.Touch();
                return item;
            }

            return NewItem(strFilePath, FileMode.OpenOrCreate, access);
        }
#endif

        public void ReturnStream(StreamItem item)
        {
            if (item.Fly)
            {
                if (item.FileStream != null)
                {
                    item.FileStream.Close();
                    item.FileStream = null;
                }
                item.DecUse();
                return;
            }

            if (item.FileStream != null)
                item.FileStream.Flush();

            item.DecUse();
        }

        public void ClearAll()
        {
            m_lock.EnterWriteLock();
            try
            {
                foreach (StreamItem item in _items)
                {
                    item.FileStream.Close();
                    item.FileStream = null;
                }

                _items.Clear();
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
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
        public FileAccess FileAccess { get; set; }
        public FileStream FileStream { get; set; }
        public DateTime LastTime { get; set; }

        public bool Fly { get; set; }   // 是否为不属于集合管理的模式

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
