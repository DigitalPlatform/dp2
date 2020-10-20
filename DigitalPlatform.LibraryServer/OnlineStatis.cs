using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 在线统计功能
    /// </summary>
    public static class OnlineStatis
    {
        // UID --> OnlineItem
        static Hashtable _itemTable = new Hashtable();

        const int MAX_ITEMS = 10000;

        // 激活一个事项
        public static long Activate(string uid, string entry)
        {
            lock (_itemTable.SyncRoot)
            {
                var item = _itemTable[uid] as OnlineItem;
                if (item == null)
                {
                    if (_itemTable.Count > MAX_ITEMS)
                        throw new Exception($"统计对象内存不足");

                    item = new OnlineItem { UID = uid, Entry = entry, LastTime = DateTime.Now };
                    _itemTable[uid] = item;
                }
                else
                    item.LastTime = DateTime.Now;

                return _getCount(entry);
            }
        }

        // 统计一个产品的当前在线数
        public static long _getCount(string entry)
        {
            long count = 0;
            foreach (OnlineItem item in _itemTable.Values)
            {
                if (item.Entry == entry)
                    count++;
            }

            return count;
        }

        public static long GetCount(string entry)
        {
            lock (_itemTable.SyncRoot)
            {
                return _getCount(entry);
            }
        }

        // 清除较旧的对象
        public static void ClearIdle(TimeSpan length)
        {
            lock (_itemTable.SyncRoot)
            {
                List<string> delete_keys = new List<string>();
                DateTime now = DateTime.Now;
                foreach (OnlineItem item in _itemTable.Values)
                {
                    if (now - item.LastTime > length)
                        delete_keys.Add(item.UID);
                }

                foreach (var key in delete_keys)
                {
                    _itemTable.Remove(key);
                }
            }
        }
    }

    public class OnlineItem
    {
        // 前端产品名称
        public string Entry { get; set; }
        // 每个前端代表自己的唯一 UID
        public string UID { get; set; }
        // 最近一次活动时间
        public DateTime LastTime { get; set; }
    }
}
