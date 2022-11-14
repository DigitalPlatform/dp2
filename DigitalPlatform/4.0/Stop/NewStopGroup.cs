using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    /// <summary>
    /// Stop 对象集合
    /// </summary>
    public class StopGroup : IEnumerable<Stop>
    {
        // 组名
        public string Name { get; set; }

        // 存储附加数据
        public object Tag { get; set; }

        private List<Stop> _stops = new List<Stop>();

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        TimeSpan _lockTimeout = TimeSpan.FromSeconds(5);

        public StopGroup(string name)
        {
            Name = name;
        }

        public void Add(Stop stop)
        {
            _lock.EnterWriteLock();
            try
            {
                stop.Group = this;
                _stops.Add(stop);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(Stop stop)
        {
            _lock.EnterWriteLock();
            try
            {
                _stops.Remove(stop);
                stop.Group = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 计算出工具条停止按钮的 Enabled 状态
        // 算法是，只要有一个 looping level 大于 0 的按钮，则算 true
        public bool GetStopButtonEnableState()
        {
            _lock.EnterReadLock();
            try
            {
                return _stops.Where(o => o.IsInLoop).Any();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        // 计算出状态行文本 Label 需要显示的内容
        // 算法是，最后一个 looping level 大于 0 的 Stop 对象的 Message。如果没有找到，就是 null
        public string GetStatusMessage()
        {
            _lock.EnterReadLock();
            try
            {
                return _stops.Where(o => o.IsInLoop).LastOrDefault()?.Message;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        // 获得最顶层一个活动的 Stop 对象
        // 算法是，最后一个 looping level 大于 0 的 Stop 对象
        public Stop GetActiveStop()
        {
            _lock.EnterReadLock();
            try
            {
                return _stops.Where(o => o.IsInLoop).LastOrDefault();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        // 把一个 Stop 对象移动到顶层位置(也就是数组的末尾)
        public bool MoveToTop(Stop stop)
        {
            if (stop == null)
                throw new ArgumentException("stop 参数值不应为 null");
            if (stop.Group == null)
                throw new ArgumentException($"Stop 对象 '{stop.Name}' 的 Group 成员不应为 null");
            if (stop.Group != this)
                throw new Exception($"Stop 对象 '{stop.Name}' 的 Group '{stop.Group.Name}' 没有指向本 Group '{this.Name}'");
            _lock.EnterWriteLock();
            try
            {
                if (_stops.Contains(stop) == false)
                    return false;
                _stops.Remove(stop);
                _stops.Add(stop);

                // 注意本函数效果是不完满的：
                // stop 有可能成为 _surfaceStop
                // 这需要在 StopManager 中继续处理此状态变化
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerator<Stop> GetEnumerator()
        {
            return _stops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_stops).GetEnumerator();
        }
    }
}
