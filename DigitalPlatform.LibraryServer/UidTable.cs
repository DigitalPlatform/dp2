using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryServer
{
    // 防止 UID 重复的 Hashtable
    public class UidTable
    {
        public const int MAX_ITEMS = 1000;

        Hashtable _table = new Hashtable();

        object _syncRoot = new object();

        public bool Contains(string uid)
        {
            lock (_syncRoot)
            {
                return _table.ContainsKey(uid);
            }
        }

        public void Set(string uid)
        {
            lock (_syncRoot)
            {
                if (_table.Count > MAX_ITEMS)
                    _table.Clear();

                _table[uid] = true;
            }
        }
    }
}
