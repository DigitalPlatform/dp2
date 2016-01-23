using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.ResultSet;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 结果集的集合
    /// </summary>
    public class ResultSetTable : Hashtable
    {
        public string ResultsetDir = "";

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        int m_nLockTimeout = 5 * 1000;

        int _nMaxCount = 10000;

        public bool IsFull
        {
            get
            {
                if (this.Count >= _nMaxCount)
                    return true;

                return false;
            }
        }

#if NO
        // 2012/12/11
        // 根据结果集名字找到一个全局结果集对象
        public DpResultSet GetGlobalResultSet(string strResultSetName)
        {
            strResultSetName = strResultSetName.ToLower();

            return (DpResultSet)this.ResultSets[strResultSetName];
        }
#endif

        public DpResultSet GetResultSet(string strResultSetName,
    bool bAutoCreate = true)
        {
            if (String.IsNullOrEmpty(strResultSetName) == true)
            {
                // strResultSetName = "default";
                throw new Exception("结果集名不应为空");
            }

            strResultSetName = strResultSetName.ToLower();

            DpResultSet resultset = null;
            if (this.m_lock.TryEnterReadLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 全局结果集集合 加读锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                resultset = (DpResultSet)this[strResultSetName];
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (resultset == null)
            {
                if (bAutoCreate == false)
                    return null;

                resultset = new DpResultSet(GetTempFileName);
                // 注：这里要特别注意在 resultset 对象销毁以前卸载事件
                resultset.GetTempFilename += new GetTempFilenameEventHandler(resultset_GetTempFilename);

                if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                    throw new ApplicationException("为 全局结果集集合 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
                try
                {
                    this[strResultSetName] = resultset;
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }
            }

            resultset.Touch();
            return resultset;
        }

        void resultset_GetTempFilename(object sender, GetTempFilenameEventArgs e)
        {
#if NO
            while (true)
            {
                string strFilename = PathUtil.MergePath(this.m_strTempDir, Guid.NewGuid().ToString());
                if (File.Exists(strFilename) == false)
                {
                    using (FileStream s = File.Create(strFilename))
                    {
                    }

                    e.TempFilename = strFilename;
                    return;
                }
            }
#endif
            e.TempFilename = GetTempFileName();
        }

        public string GetTempFileName()
        {
            Debug.Assert(string.IsNullOrEmpty(this.ResultsetDir) == false, "");
            while (true)
            {
                string strFilename = Path.Combine(this.ResultsetDir, Guid.NewGuid().ToString());
                if (File.Exists(strFilename) == false)
                {
                    using (FileStream s = File.Create(strFilename))
                    {
                    }
                    return strFilename;
                }
            }
        }

        // 记载一个全局结果集
        // parameters:
        //      strName 全局结果集名字。如果为空，表示希望函数自动发生一个结果集名
        //      resultset   要设置的结果集对象。如果为null，表示想从全局结果集中清除这个名字的结果集对象。如果strName和resultset参数都为空，表示想清除全部全局结果集对象
        // return:
        //      返回实际设置的结果集名字
        public string SetResultset(
            string strName,
            DpResultSet resultset)
        {
            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 全局结果集集合 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                if (string.IsNullOrEmpty(strName) == true)
                {
                    if (resultset == null)
                    {
                        foreach (string key in this.Keys)
                        {
                            // 2016/1/23
                            resultset = (DpResultSet)this[key];
                            if (resultset != null)
                            {
                                resultset.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);
                                resultset.Close();
                            }
                        }
                        this.Clear();
                        return null;
                    }
                    // 自动发生一个名字
                    strName = Guid.NewGuid().ToString();
                }
                if (resultset == null)
                {
                    // 2016/1/23
                    resultset = (DpResultSet)this[strName];
                    if (resultset != null)
                    {
                        resultset.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);
                        resultset.Close();
                    }

                    this.Remove(strName);
                    return strName;
                }

                resultset.Touch();
                this[strName] = resultset;
                return strName;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 清除最近没有使用过的 ResultSet 对象
        // parameters:
        //      delta   最近一次用过的时刻距离现在的时间长度。长于这个的对象才会被清除
        public void Clean(TimeSpan delta)
        {
            List<string> remove_keys = new List<string>();

            // 读锁定并不阻碍一般性访问
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    DpResultSet resultset = (DpResultSet)this[key];

                    if (resultset == null)
                        continue;

                    if ((DateTime.Now - resultset.LastUsedTime) >= delta)
                    {
                        remove_keys.Add(key);   // 这里暂时无法删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return;

            // 因为要删除某些元素，所以用写锁定
            List<DpResultSet> delete_resultsets = new List<DpResultSet>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in remove_keys)
                {
                    DpResultSet resultset = (DpResultSet)this[key];
                    if (resultset == null)
                        continue;

                    // 和 hashtable 脱离关系
                    this.Remove(key);

                    delete_resultsets.Add(resultset);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            foreach (DpResultSet resultset in delete_resultsets)
            {
                // 2016/1/23
                if (resultset != null)
                    resultset.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);

                resultset.Close();
            }
        }
    }
}
