using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DigitalPlatform.MarcDom
{
    /// <summary>
    /// FilterDocument 存储容器。
    /// 根据名字来管理 FilterDocument 对象
    /// </summary>
    public class FilterCollection
    {
        Hashtable _table = new Hashtable();

        public bool IgnoreCase = true;

        ReaderWriterLock _lock = new ReaderWriterLock();
        static int _nLockTimeout = 5000;	// 5000=5秒

        public int Max = 100;   // 每个List中对象数上限

        public FilterDocument GetFilter(string strName)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();

            FilterList filterlist = null;

            this._lock.AcquireWriterLock(_nLockTimeout);
            try
            {
                // 查看一个名字是否有对应的 FilterList 对象了
                filterlist = (FilterList)_table[strName];

                // 如果还没有，则创建一个新对象
                if (filterlist == null)
                {
                    filterlist = new FilterList();
                    filterlist.Container = this;
                    _table[strName] = filterlist;
                }
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }

            // 从 FilterList 对象中获取一个 FilterDocument 对象。
            // 注： FilterList 对象用于管理多个 FilterDocument 对象，其中被征用的暂时就不能被使用了，需要创建新对象
            FilterDocument filter = filterlist.GetFilter();
            return filter;
        }

        public void Clear()
        {
            this._lock.AcquireWriterLock(_nLockTimeout);
            try
            {
                this._table.Clear();
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        // 从集合中清除特定名字的 FilterList 对象
        public void ClearFilter(string strName)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();

            this._lock.AcquireWriterLock(_nLockTimeout);
            try
            {

                FilterList filterlist = (FilterList)_table[strName];
                if (filterlist != null)
                    _table.Remove(strName);
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public void SetFilter(string strName,
            FilterDocument filter)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();
            FilterList filterlist = null;

            this._lock.AcquireWriterLock(_nLockTimeout);
            try
            {
                filterlist = (FilterList)_table[strName];

                if (filterlist == null)
                {
                    filterlist = new FilterList();
                    filterlist.Container = this;
                    _table[strName] = filterlist;
                }
                Debug.Assert(filterlist != null, "");
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }

            filterlist.SetFilter(filter);
        }

        public int Count
        {
            get
            {
                return this._table.Count;
            }
        }

        public string Dump()
        {
            string strResult = "";

            strResult += "本集合中共用'" + Convert.ToString(this._table.Count) + "'个FilterList对象.\r\n";

            foreach (DictionaryEntry item in _table)
            {
                strResult += "  " + item.Key + "\r\n";

                FilterList list = (FilterList)item.Value;

                strResult += "    " + list.Dump();
            }

            return strResult;
        }
    }

    /// <summary>
    /// 管理 FilterDocument 对象的若干副本。
    /// 确保 FilterDocument 对象在征用期间被独占使用
    /// </summary>
    public class FilterList
    {
        List<FilterHolder> _list = new List<FilterHolder>();

        ReaderWriterLock _lock = new ReaderWriterLock();
        static int _nLockTimeout = 5000;	// 5000=5秒

        private FilterCollection _container = null;

        public FilterCollection Container
        {
            get
            {
                return _container;
            }

            set
            {
                _container = value;
            }
        }

        /// <summary>
        /// 获得一个尚未被征用的 FilterDocument 对象并立即征用它
        /// </summary>
        /// <returns>FilterDocument 对象</returns>
        public FilterDocument GetFilter()
        {
            this._lock.AcquireReaderLock(_nLockTimeout);
            try
            {
                foreach (FilterHolder item in _list)
                {
                    if (Interlocked.Increment(ref item.UsedCount) == 1)
                        return item.FilterDocument;

                    Interlocked.Decrement(ref item.UsedCount);
                }
                return null;
            }
            finally
            {
                this._lock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// 归还或者加入一个 FilterDocument 对象。
        /// 如果对象在 list 中已经存在，则归还它；如果在 list 中不存在，则新添加它进入 list，此时它是尚未被征用状态。
        /// 对 list 中最大对象数是进行了控制的
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>true: 成功; false: 没能加入 list，因为超过数量极限 Max 了</returns>
        public bool SetFilter(FilterDocument filter)
        {
            // string strMessage = "";

            this._lock.AcquireReaderLock(_nLockTimeout);
            try
            {
                foreach (FilterHolder item in _list)
                {
                    if (item.FilterDocument == filter)
                    {
                        int nValue = Interlocked.Decrement(ref item.UsedCount);
                        if (nValue < 0)
                        {
                            throw new Exception("还回后UsedCount小于0, 错误");
                        }

                        return true;
                    }
                }
            }
            finally
            {
                this._lock.ReleaseReaderLock();
            }

            return NewFilter(filter);
        }

#if NO  // 暂时没有用到
        public void ReturnFilter(FilterDocument filter)
        {
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    FilterHolder item = this.list[i];

                    if (item.FilterDocument == filter)
                    {
                        int nValue = Interlocked.Decrement(ref item.UsedCount);
                        if (nValue < 0)
                        {
                            throw new Exception("还回后UsedCount小于0, 错误");
                        } 
                        return;
                    }
                }
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            throw new Exception("还回的对象在数组中没有找到");
        }
#endif

        // 加入一个 FilterDocument 对象。
        // 如果 list 中的对象数量超过了 Max 上限，则不会加入
        // return:
        //      true    已经加入
        //      false   未加入
        public bool NewFilter(FilterDocument filter)
        {
            this._lock.AcquireWriterLock(_nLockTimeout);
            try
            {
                if (this._list.Count >= this._container.Max)
                    return false;

                FilterHolder item = new FilterHolder();
                item.FilterDocument = filter;
                this._list.Add(item);
                return true;
            }
            finally
            {
                this._lock.ReleaseWriterLock();
            }
        }

        public string Dump()
        {
            string strResult = "";

            strResult += "共用'" + Convert.ToString(this._list.Count) + "'个FilterDocumnet对象.\r\n";

            return strResult;
        }
    }

    /// <summary>
    /// 包裹 FilterDocument 的容器，并提供了使用计数
    /// </summary>
    public class FilterHolder
    {
        public FilterDocument FilterDocument = null;
        public int UsedCount = 0;
    }
}
