using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;

namespace dp2SSL
{
    // 用于防范登录攻击的类
    public class AttackManager
    {
        Hashtable _table = new Hashtable();

        // 多少次被当作攻击
        public int AttackCount = 5;

        // 限制最多条目数
        public int LimitCount = 5000;

        // 多长时间以上的旧条目要被清除
        public TimeSpan CleanLength = TimeSpan.FromMinutes(5);

        // 加入一个条目，或者增量已有条目的计数
        public void Increase(string title)
        {
            if (_table.Count > LimitCount)
                Clean(CleanLength);

            lock (_table.SyncRoot)
            {
                var entry = _table[title] as AttackEntry;
                if (entry == null)
                {
                    // 如果依然超过了限制条目数，只好不再增加新条目
                    if (_table.Count > LimitCount)
                        return;

                    entry = new AttackEntry
                    {
                        Title = title,
                        LastTime = DateTime.Now,
                        Count = 1
                    };
                    _table[title] = entry;
                }
                else
                {
                    entry.Count++;
                    entry.LastTime = DateTime.Now;
                }
            }
        }

        // 检索，查看指定的 title 是否已经存在对应的条目
        public NormalResult Search(string title)
        {
            lock (_table.SyncRoot)
            {
                var entry = _table[title] as AttackEntry;
                if (entry == null)
                    return new NormalResult();
                if (entry.Count < AttackCount)
                    return new NormalResult();
                return new NormalResult { Value = 1 };
            }
        }

        // 清理超过一定时间的条目
        public void Clean(TimeSpan length)
        {
            lock (_table.SyncRoot)
            {
                List<string> delete_keys = new List<string>();
                DateTime now = DateTime.Now;
                foreach (string key in _table.Keys)
                {
                    var entry = _table[key] as AttackEntry;
                    if (now - entry.LastTime > length)
                        delete_keys.Add(key);
                }

                foreach (string key in delete_keys)
                {
                    // TODO: 可以考虑把条目删除前记载到日志文件
                    _table.Remove(key);
                }
            }
        }
    }

    class AttackEntry
    {
        public string Title { get; set; }

        // 最后一次攻击的时间
        public DateTime LastTime { get; set; }

        // 攻击次数
        public long Count { get; set; }
    }
}
