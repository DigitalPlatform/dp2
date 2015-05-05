using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// 缓存 T 类型的对象
    /// TODO: 需要实现最大元素数目限制功能
    /// </summary>
    public class ObjectCache<T>
    {
        Hashtable m_table = new Hashtable();

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public ObjectItem<T> FindObjectItem(string strPath)
        {
            this.m_lock.EnterReadLock();
            try
            {
                return (ObjectItem<T>)this.m_table[strPath];
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        // 包装后的版本
        public T FindObject(string strPath)
        {
            ObjectItem<T> item = FindObjectItem(strPath);
            if (item == null)
                return default(T);
            return item.Object;
        }

        public void SetObjectItem(string strPath,
            ObjectItem<T> object_item)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                if (object_item == null)
                {
                    this.m_table.Remove(strPath);
                    return;
                }
                this.m_table[strPath] = object_item;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 包装后的版本
        public void SetObject(string strPath,
            T obj,
            string strTimestamp = "")
        {
            if (obj == null)
            {
                SetObjectItem(strPath, null);
                return;
            }
            ObjectItem<T> item = new ObjectItem<T>();
            item.Object = obj;
            item.Timestamp = strTimestamp;
            SetObjectItem(strPath, item);
        }

        public void Clear()
        {
            this.m_lock.EnterWriteLock();
            try
            {
                this.m_table.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                return this.m_table.Count;
            }
        }

        public ICollection Keys
        {
            get
            {
                return this.m_table.Keys;
            }
        }

    }

    public class ObjectItem<T>
    {
        public T Object = default(T);
        public string Timestamp = "";
    }
}
