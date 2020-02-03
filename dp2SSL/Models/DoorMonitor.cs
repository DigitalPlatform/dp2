using DigitalPlatform.RFID;
using DigitalPlatform.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
{
#if DOOR_MONITOR

    /// <summary>
    /// 监控门的状态变化。如果超时以后还没有到来期待的开门状态变化，则补做一个开门和关闭状态变化序列
    /// </summary>
    public class DoorMonitor
    {
        List<MonitorMessage> _messages = new List<MonitorMessage>();

        public TimeSpan TimeoutLength = TimeSpan.FromSeconds(5);

        public delegate void Delegate_tiggerSubmit(DoorItem door, bool clearOperator);

        object _syncRoot = new object();

        Delegate_tiggerSubmit _func_submit = null;

        // 初始化
        public void Initialize(Delegate_tiggerSubmit func_submit)
        {
            _func_submit = func_submit;
        }

        // 启动独立的线程进行监控
        public void Start(Delegate_tiggerSubmit func_submit,
            CancellationToken token)
        {
            _func_submit = func_submit;

            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    ProcessTimeout();
                    Task.Delay(TimeSpan.FromMinutes(1), token).Wait();
                }
            });
        }

        // 开始监控一个门
        public void BeginMonitor(DoorItem door)
        {
            lock (_syncRoot)
            {
                // 检查以前这个门是否已经有监控消息
                var message = _messages.Find((m) =>
                {
                    if (m.Door == door)
                        return true;
                    return false;
                });

                if (message != null)
                {
                    App.CurrentApp.Speak("补做遗留提交");
                    WpfClientInfo.WriteInfoLog($"对门 {message.Door.Name} 补做一次 submit (1)");

                    _func_submit?.Invoke(message.Door, false);
                    message.StartTime = DateTime.Now;   // 重新开始
                    message.HeartbeatTicks = RfidManager.LockHeartbeat;
                }
                else
                {
                    message = new MonitorMessage
                    {
                        Door = door,
                        StartTime = DateTime.Now,
                        HeartbeatTicks = RfidManager.LockHeartbeat
                    };
                    _messages.Add(message);
                }
            }
        }

        // 把不再需要监控的消息移走
        public void RemoveMessages(DoorItem door)
        {
            lock (_syncRoot)
            {
                List<MonitorMessage> delete_messages = new List<MonitorMessage>();
                foreach (var message in _messages)
                {
                    if (message.Door == door)
                    {
                        delete_messages.Add(message);
                    }
                }

                foreach (var message in delete_messages)
                {
                    _messages.Remove(message);
                }
            }
        }

        // 处理一次监控任务
        public void ProcessTimeout()
        {
            lock (_syncRoot)
            {
                List<MonitorMessage> delete_messages = new List<MonitorMessage>();
                foreach (var message in _messages)
                {
                    // if (DateTime.Now - message.StartTime > TimeoutLength)
                    if (RfidManager.LockHeartbeat - message.HeartbeatTicks >= 2)
                    {
                        App.CurrentApp.Speak("超时补做提交");
                        WpfClientInfo.WriteInfoLog($"超时情况下，对门 {message.Door.Name} 补做一次 submit");

                        _func_submit?.Invoke(message.Door, true);
                        delete_messages.Add(message);

                        message.Door.Waiting--;
                    }
                }

                foreach (var message in delete_messages)
                {
                    _messages.Remove(message);
                }
            }
        }
    }

    public class MonitorMessage
    {
        // 要监控的门对象
        public DoorItem Door { get; set; }
        public DateTime StartTime { get; set; }
        public long HeartbeatTicks { get; set; }
    }

#endif

}
