using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    // 事件日志
    public class OperLogFileCache : List<CacheFileItem>, IDisposable
    {
        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public void Dispose()
        {
            this.Close();
        }

        // 使用
        public CacheFileItem Open(string strFilename)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                CacheFileItem item = null;
                int i = 0;
                foreach (CacheFileItem cur_item in this)
                {
                    if (cur_item.Used == false
                        && cur_item.FileName == strFilename)
                    {
                        cur_item.LastTime = DateTime.Now;
                        cur_item.Used = true;

                        // 移动到数组的最前面
                        if (i > 10) // i > 10
                        {
                            this.Remove(cur_item);
                            this.Insert(0, cur_item);
                        }
                        return cur_item;
                    }

                    i++;
                }

                item = new CacheFileItem();

                item.FileName = strFilename;
                // 可能会抛出异常
                item.Stream = File.Open(
                    strFilename,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
                item.Used = true;
                this.Insert(0, item);   // 插入到最前面，希望后面被命中的概率大一些
                item.LastTime = DateTime.Now;
                return item;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 归还
        public void Close(CacheFileItem item)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                item.LastTime = DateTime.Now;
                item.Used = false;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 全部关闭
        public void Close()
        {
            this.m_lock.EnterWriteLock();
            try
            {
                foreach (CacheFileItem cur_item in this)
                {
                    if (cur_item.Stream != null)
                    {
                        cur_item.Stream.Close();
                        cur_item.Stream = null;
                    }
                }

                base.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 将不活跃的事项压缩
        public void Shrink(TimeSpan delta)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                DateTime now = DateTime.Now;
                for (int i = 0; i < this.Count; i++)
                {
                    CacheFileItem cur_item = this[i];

                    if (cur_item.Used == false)
                    {
                        if (now - cur_item.LastTime > delta)
                        {
                            if (cur_item.Stream != null)
                                cur_item.Stream.Close();
                            this.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }
    }

    public class CacheFileItem
    {
        public string FileName = "";
        public Stream Stream = null;    // !!! 谁来释放
        public bool Used = false;
        public DateTime LastTime = DateTime.Now;
    }
}