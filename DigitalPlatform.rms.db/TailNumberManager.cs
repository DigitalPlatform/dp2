using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 尾号管理
    /// </summary>
    public class TailNumberManager
    {
        Dictionary<Database, Int64> _table = new Dictionary<Database, Int64>();
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void SetTailNumber(Database database, Int64 id)
        {
            _lock.EnterWriteLock();
            try
            {
                _table[database] = id;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 如果比当前尾号大，则推动尾号
        public bool PushTailNumber(Database database, Int64 push_id)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_table.ContainsKey(database) == false)
                {
                    _table[database] = push_id;
                    return true;
                }
                Int64 id = _table[database];
                if (push_id > id)
                {
                    _table[database] = push_id;
                    return true;
                }

                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 得到当前尾号，并自动增量尾号
        public Int64 NewTailNumber(Database database, Int64 first_value = 1)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_table.ContainsKey(database) == false)
                {
                    _table[database] = first_value;
                    // return first_value;
                }
                Int64 id = _table[database];
                id++;
                _table[database] = id;
                return id;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 得到当前尾号，不自动增量
        public Int64 GetTailNumber(Database database, Int64 first_value = 1)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_table.ContainsKey(database) == false)
                {
                    _table[database] = first_value;
                    return first_value;
                }
                return _table[database];
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
