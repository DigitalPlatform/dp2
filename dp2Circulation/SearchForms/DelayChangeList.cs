using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;

namespace dp2Circulation
{
    // 处理延迟执行的动作
    // 如果动作快速堆积，则中间的会被取消，每轮只执行最后的一个动作
    public class DelayChangeList
    {
        List<ActionItem> _actions = new List<ActionItem>();
        TimeSpan _actionDelayLength = TimeSpan.FromMilliseconds(500);

        public AutoResetEvent Event = new AutoResetEvent(true);

        public void DelayProcess(object info)
        {
            lock (_actions)
            {
                // 防止数组尺寸变得太大
                if (_actions.Count > 1000)
                    _actions.Clear();

                _actions.Add(new ActionItem
                {
                    Tag = info,
                    Time = DateTime.Now
                });
                Event.Set();
            }
        }

        public delegate void delegate_triggerAction(object info);

        public void ProcessChangeList(delegate_triggerAction func_tigger)
        {
            ActionItem last_time = new ActionItem
            {
                Time = DateTime.MinValue,
                Tag = null
            };

            DateTime now = DateTime.Now;
            lock (_actions)
            {
                if (_actions.Count == 0)
                    return;
                last_time = _actions[_actions.Count - 1];
                if (now - last_time.Time > _actionDelayLength)
                    _actions.Clear();
                else
                    return;
            }

            if (last_time.Tag != null)
            {
                // 触发动作
                func_tigger?.Invoke(last_time.Tag);
            }
            // 
        }
    }

    public class ActionItem
    {
        public DateTime Time { get; set; }

        public object Tag { get; set; }
    }
}
