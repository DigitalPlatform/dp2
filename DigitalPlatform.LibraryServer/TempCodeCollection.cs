using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 存储短信验证码的集合
    /// </summary>
    public class TempCodeCollection
    {
        // 事项数配额
        static int MaxItems = 1000;

        Hashtable _table = new Hashtable();

        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public TempCode FindTempCode(string strKey)
        {
            _lock.EnterReadLock();
            try
            {
                return (TempCode)_table[strKey];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SetTempCode(string strKey, TempCode code)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_table.ContainsKey(strKey) == false)
                {
                    // 即将增加
                    if (_table.Count >= MaxItems)
                        throw new Exception("TempCodeCollection 事项数目已经超出配额。请稍后再重试操作");
                }
                _table[strKey] = code;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 清除已经失效的那些事项
        public void CleanExpireItems()
        {
            List<string> remove_keys = new List<string>();

            // 读锁定并不阻碍一般性访问
            _lock.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach (string key in this._table)
                {
                    TempCode code = (TempCode)this._table[key];
                    if (code == null || code.ExpireTime < now)
                    {
                        remove_keys.Add(key);   // 这里暂时无法删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return;

            // 因为要删除某些元素，所以用写锁定
            _lock.EnterWriteLock();
            try
            {
                foreach (string key in remove_keys)
                {
                    // 和 hashtable 脱离关系
                    _table.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(string strKey)
        {
            _lock.EnterWriteLock();
            try
            {
                _table.Remove(strKey);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

    }

    // 一个验证码事项
    public class TempCode
    {
        // 键。一般由用户名 + 电话号码 + 前端 IP 地址组成
        public string Key { get; set; }
        // 验证码
        public string Code { get; set; }
        // 失效时间
        public DateTime ExpireTime { get; set; }
    }
}
