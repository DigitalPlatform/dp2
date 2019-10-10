// #define TESTING  // 增加了一些专门的线程来不断密集收缩 _items。增加测试强度

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// Stream 对象的缓存。用于加快连续读、写时候打开文件和移动文件指针操作的速度
    /// </summary>
    public class StreamCache : IDisposable
    {
        int MAX_ITEMS = 100;

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        List<StreamItem> _items = new List<StreamItem>();

#if TESTING
        CancellationTokenSource _cancel = new CancellationTokenSource();
#endif

        public StreamCache(int nMaxItems)
        {
            MAX_ITEMS = nMaxItems;

#if TESTING
            CancellationToken token = _cancel.Token;
            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    Thread.Sleep(1);
                    this.ClearIdle(TimeSpan.FromSeconds(5));
                }
            });
#endif
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
        public void FileMove(string strSourceFileName,
            string strFileName,
            bool auto_retry)
        {
            this.ClearItems(strSourceFileName);
            this.ClearItems(strFileName);
            if (auto_retry == false)
                File.Move(strSourceFileName, strFileName);
            else
            {
                try
                {
                    File.Move(strSourceFileName, strFileName);
                }
                catch (System.IO.IOException)
                {
                    File.Delete(strFileName);
                    File.Move(strSourceFileName, strFileName);
                }
            }
        }

        // 改保守一点的版本
        // 注：本函数要慎用
        public void ClearItems(string strFilePath)
        {
            List<StreamItem> items = new List<StreamItem>();
            m_lock.EnterWriteLock();
            try
            {
                foreach (StreamItem item in _items)
                {
                    if (item.FilePath == strFilePath)
                    {
                        items.Add(item);
                    }
                }

                foreach (StreamItem item in items)
                {
                    _items.Remove(item);
                    item.Close();
                    item.Dispose();
                    /*
                    if (item.FileStream != null)
                    {
                        if ((item.FileAccess & FileAccess.Write) != 0)
                            item.FileStream.Flush();    // 2019/9/2
                        item.FileStream.Close();
                    }
                    */
                }
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }


#if OLD_CODE
        // 原来版本
        // 注：本函数要慎用
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
                m_lock.EnterWriteLock();    // TODO: 这样锁定可能会有问题。最好简化锁定，整个函数放在一个写锁定中
                try
                {
                    foreach (StreamItem item in items)
                    {
                        _items.Remove(item);
                        if (item.FileStream != null)
                        {
                            if ((item.FileAccess & FileAccess.Write) != 0)
                                item.FileStream.Flush();    // 2019/9/2
                            item.FileStream.Close();
                        }
                    }
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }

#endif

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
                ClearAll(true);

            StreamItem item = new StreamItem
            {
                Fly = !bAddToCollection,
                FileAccess = access
            };
            item.Touch();
            item.FilePath = strFilePath;

            int nRedoCount = 0;
            REDO:
            try
            {
                item.FileStream = File.Open(
        strFilePath,
        mode,   // FileMode.OpenOrCreate,
        access, // FileAccess.Write,
        FileShare.ReadWrite);
            }
            catch (DirectoryNotFoundException ex)
            {
                if ((item.FileAccess & FileAccess.Write) != 0
                    && nRedoCount == 0)
                {
                    // 创建中间子目录
                    PathUtil.TryCreateDir(PathUtil.PathPart(strFilePath));
                    nRedoCount++;
                    goto REDO;
                }
                throw new Exception(ex.Message, ex);
            }

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
            // testing
            // return GetStream(strFilePath, FileMode.OpenOrCreate, FileAccess.Write, false);
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
                /*
                if (item.FileStream != null)
                {
                    if ((item.FileAccess & FileAccess.Write) != 0)
                        item.FileStream.Flush();    // 2019/9/2
                    item.FileStream.Close();
                    item.FileStream = null;
                }
                */
                item.Close();
                item.DecUse();
                item.Dispose();
                return;
            }

            if (item.FileStream != null)
            {
                if ((item.FileAccess & FileAccess.Write) != 0)
                    item.FileStream.Flush();
            }

            item.DecUse();
        }

        // parameters:
        //      check_inUse   是否避免删除 InUse 状态的对象？
        public void ClearAll(bool check_inUse = false)
        {
            m_lock.EnterWriteLock();
            try
            {
                // 要保留的对象列表
                List<StreamItem> reserve_items = new List<StreamItem>();

                foreach (StreamItem item in _items)
                {
                    if (check_inUse)
                    {
                        int v = item.IncUse();
                        if (v == 1)
                        {
                            // 说明 inc 以前是 0。适合删除
                        }
                        else
                        {
                            item.DecUse();
                            reserve_items.Add(item);
                            continue;
                        }
                    }

                    /*
                    if ((item.FileAccess & FileAccess.Write) != 0)
                        item.FileStream.Flush();    // 2019/9/2
                    item.FileStream.Close();
                    item.FileStream = null;
                    */
                    item.Close();

                    if (check_inUse)
                        item.DecUse();

                    item.Dispose();
                }

                _items = reserve_items;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        // 改保守以后的版本
        // 清除闲置时间过长的事项
        public void ClearIdle(TimeSpan delta)
        {
            DateTime now = DateTime.Now;

            List<StreamItem> delete_items = new List<StreamItem>();
            List<StreamItem> all_items = new List<StreamItem>();

            // 加锁
            m_lock.EnterWriteLock();
            try
            {
                all_items.AddRange(_items);

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
                /*
                if ((item.FileAccess & FileAccess.Write) != 0)
                    item.FileStream.Flush();    // 2019/9/2
                item.FileStream.Close();
                item.FileStream = null;
                */
                item.Close();
                item.Dispose();
            }
        }

#if OLD_CODE
        // 原来版本
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
                if ((item.FileAccess & FileAccess.Write) != 0)
                    item.FileStream.Flush();    // 2019/9/2
                item.FileStream.Close();
                item.FileStream = null;
            }
        }

#endif

        public void Dispose()
        {
            ClearAll(false);
#if TESTING
            _cancel.Cancel();
#endif
        }
    }

    public class StreamItem : IDisposable
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

        public void Close()
        {
            if (this.FileStream != null)
            {
                if ((this.FileAccess & FileAccess.Write) != 0)
                    this.FileStream.Flush();    // 2019/9/2
                this.FileStream.Close();
                this.FileStream.Dispose();
                this.FileStream = null;
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
