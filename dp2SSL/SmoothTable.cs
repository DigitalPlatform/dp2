using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL
{
    // 平滑条码输入抖动
    public class SmoothTable
    {
        Hashtable _uidTable = new Hashtable();

        static TimeSpan _minDelay = TimeSpan.FromMilliseconds(2000);

        public void SetLastTime(string uid, DateTime now)
        {
            if (string.IsNullOrEmpty(uid))
                return;

            lock (_uidTable.SyncRoot)
            {
                if (_uidTable.Count > 1000)
                    _uidTable.Clear();  // TODO: 可以优化为每隔一段时间自动清除太旧的事项
                _uidTable[uid] = now;
            }
        }

        public DateTime GetLastTime(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return DateTime.MinValue;

            lock (_uidTable.SyncRoot)
            {
                if (_uidTable.ContainsKey(uid) == false)
                    return DateTime.MinValue;
                DateTime time = (DateTime)_uidTable[uid];
                return time;
            }
        }

        // 检查时间差额
        public bool Check(string uid)
        {
            DateTime now = DateTime.Now;
            DateTime last_time = GetLastTime(uid);
            if (now - last_time < _minDelay)
            {
                SetLastTime(uid, now);
                return false;   // 表示需要忽略这次输入
            }

            SetLastTime(uid, now);
            return true;
        }
    }
}
