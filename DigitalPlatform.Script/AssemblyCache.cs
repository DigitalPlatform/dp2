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

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ObjectItem<T> FindObjectItem(string strPath)
        {
            this._lock.EnterReadLock();
            try
            {
                return (ObjectItem<T>)this.m_table[strPath];
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        // 按照路径字符串来锁定。作用为在创建对象期间排斥相同名字的对象重叠创建
        RecordLockCollection _pathLock = new RecordLockCollection();

        // 获得一个对象。如果对象尚不存在，则用给定的 proc 方法创建它
        public T GetObject(string strPath, CreateItem<T> proc)
        {
            this._lock.EnterUpgradeableReadLock();
            try
            {
                ObjectItem<T> item = (ObjectItem<T>)this.m_table[strPath];
                if (item != null)
                    return item.Object;
                this._pathLock.LockForWrite(strPath);
                try
                {
                    // 再次确认。因为有可能一瞬间前别的线程刚好在写锁定过程中创建好了对象
                    item = (ObjectItem<T>)this.m_table[strPath];
                    if (item != null)
                        return item.Object;

                    item = new ObjectItem<T>();
                    item.Object = proc();
                    this.SetObjectItem(strPath, item); 
                    return item.Object;
                }
                finally
                {
                    this._pathLock.UnlockForWrite(strPath);
                }
            }
            finally
            {
                this._lock.ExitUpgradeableReadLock();
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
            this._lock.EnterWriteLock();
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
                this._lock.ExitWriteLock();
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

        public void Clear(string strPath)
        {
            SetObjectItem(strPath, null);
        }

        public void Clear()
        {
            this._lock.EnterWriteLock();
            try
            {
                this.m_table.Clear();
            }
            finally
            {
                this._lock.ExitWriteLock();
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

        // return:
        //      true    希望继续向后处理
        //      false   希望中断处理
        public delegate bool delegate_processItem(T item);
        
        // return:
        //      实际处理的事项个数
        public int ProcessAll(delegate_processItem func)
        {
            this._lock.EnterWriteLock();
            try
            {
                int count = 0;
                foreach (string path in this.m_table.Keys)
                {
                    var item = (ObjectItem<T>)this.m_table[path];
                    count++;
                    var ret = func?.Invoke(item.Object);
                    if (ret == false)
                        return count;
                }
                return count;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }
    }

    public class ObjectItem<T>
    {
        public T Object = default(T);
        public string Timestamp = "";
    }

    public delegate T CreateItem<T>();
}
