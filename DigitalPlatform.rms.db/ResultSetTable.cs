using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;

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

        // 注意，没有锁定集合
        void FreeResultSet(string strOldName)
        {
            DpResultSet resultset = (DpResultSet)this[strOldName];
            if (resultset == null)
                return;
            this.Remove(strOldName);
            resultset.Close();
        }

        public bool RenameResultSet(string strOldName, string strNewName)
        {
            if (String.IsNullOrEmpty(strOldName) == true)
                throw new ArgumentException("结果集名不应为空", "strOldName");
            if (String.IsNullOrEmpty(strNewName) == true)
                throw new ArgumentException("结果集名不应为空", "strNewName");

            strOldName = strOldName.ToLower();
            strNewName = strNewName.ToLower();

            if (strOldName == strNewName)
                throw new ArgumentException("strOldName 和 strNewName 参数值不应相同");

            if (this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                throw new ApplicationException("为 全局结果集集合 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
            try
            {
                DpResultSet resultset = (DpResultSet)this[strOldName];
                if (resultset == null)
                    return false;

                // 如果新名字存在，则要释放已有对象
                FreeResultSet(strNewName);
                this[strNewName] = resultset;
                this.Remove(strOldName);
                return true;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // TODO: 似乎锁定不太严密。可以改用 UpgradeableReadLock
        // TODO: 全局结果集的名字可否就是文件名? 这样如果需要永久保持，下次启动的时候从文件系统就能列举出结果集名字
        public DpResultSet GetResultSet(string strResultSetName,
    bool bAutoCreate = true)
        {
            if (String.IsNullOrEmpty(strResultSetName) == true)
            {
                // strResultSetName = "default";
                throw new ArgumentException("结果集名不应为空");
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
        //                  注：当 resultset 为 null 时，strName 参数值可以为 "resultset_name1,resultset_name2" 形态
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
                    // strName 为空，resultset 为 null，表示清除全部结果集对象
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
#if NO
                    // 2018/9/20
                    // 如果名字的第一个字符为 #，则要去掉 #。这样调用者可以用也可以不用 # 在名字里面
                    if (strName.StartsWith("#"))
                        strName = strName.Substring(1);
#endif
                    List<string> names = StringUtil.SplitList(strName);

                    foreach (string name in names)
                    {
                        string current = name;

                        // 去掉全局结果集名字前面的符号 #
                        if (string.IsNullOrEmpty(name) == false
                            && name.StartsWith("#"))
                            current = name.Substring(1);

                        // 2016/1/23
                        resultset = (DpResultSet)this[current];
                        if (resultset != null)
                        {
                            resultset.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);
                            resultset.Close();
                        }

                        this.Remove(current);
                    }
                    return strName;
                }

                FreeResultSet(strName);

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

                    if (resultset.Permanent == true)
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
