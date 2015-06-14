using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DigitalPlatform.IO;

namespace DigitalPlatform.MarcDom
{
    public class FilterCollection
    {
        Hashtable table = new Hashtable();

        public bool IgnoreCase = true;

        public ReaderWriterLock m_lock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        public int Max = 100;   // 每个List中对象数上限


        public FilterDocument GetFilter(string strName)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();

            FilterList filterlist = null;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                filterlist = (FilterList)table[strName];


                if (filterlist == null)
                {
                    filterlist = new FilterList();
                    filterlist.Container = this;
                    table[strName] = filterlist;
                }

            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            FilterDocument filter = filterlist.GetFilter();

            return filter;
        }

        // 2007/1/8
        public void Clear()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.table.Clear();
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }


        // 从集合中清除特定名字的filterlist
        public void ClearFilter(string strName)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();

            FilterList filterlist = null;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                filterlist = (FilterList)table[strName];

                if (filterlist != null)
                {
                    table.Remove(strName);
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        public void SetFilter(string strName,
            FilterDocument filter)
        {
            if (IgnoreCase == true)
                strName = strName.ToLower();
            FilterList filterlist = null;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                filterlist = (FilterList)table[strName];

                if (filterlist == null)
                {
                    filterlist = new FilterList();
                    filterlist.Container = this;
                    table[strName] = filterlist;
                }

            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            filterlist.SetFilter(filter);
        }

        public int Count
        {
            get 
            {
                return this.table.Count;
            }
        }

        public string Dump()
        {
            string strResult = "";

            strResult += "本集合中共用'" + Convert.ToString(this.table.Count)+ "'个FilterList对象.\r\n";

            foreach (DictionaryEntry item in table)
            {
                strResult += "  " + item.Key + "\r\n";

                FilterList list = (FilterList)item.Value;

                strResult += "    " + list.Dump();
            }

            return strResult;
        }
    }

    public class FilterList
    {
        List<FilterHolder> list = new List<FilterHolder>();

        public ReaderWriterLock m_lock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        public FilterCollection Container = null;

        public FilterDocument GetFilter()
        {
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    FilterHolder item = this.list[i];

                    if (Interlocked.Increment(ref item.UsedCount) == 1)
                    {
                        return item.FilterDocument;
                    }

                    Interlocked.Decrement(ref item.UsedCount);
                }

                return null;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }
        }

        public bool SetFilter(FilterDocument filter)
        {
            // string strMessage = "";

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

                        return true;
                    }

                }
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            return NewFilter(filter);
        }

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

        // return:
        //      true    已经加入
        //      false   未加入
        public bool NewFilter(FilterDocument filter)
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {


                if (this.list.Count >= this.Container.Max)
                    return false;


                FilterHolder item = new FilterHolder();
                item.FilterDocument = filter;

                this.list.Add(item);

                return true;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }


        public string Dump()
        {
            string strResult = "";

            strResult += "共用'" + Convert.ToString(this.list.Count)+ "'个FilterDocumnet对象.\r\n";

            return strResult;
        }
    }


    public class FilterHolder
    {
        public FilterDocument FilterDocument = null;
        public int UsedCount = 0;
    }

}
