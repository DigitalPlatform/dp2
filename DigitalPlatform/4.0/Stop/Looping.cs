using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public class Looping : IDisposable
    {
        public Stop stop { get; set; }
        StopEventHandler _handler = null;
        static StopManager _stopManager = null;

        // 2022/10/30
        public LoopingHost Host { get; set; }

        // 附加数据
        public object Tag { get; set; }

        public Action Closed;

        public static void Initialize(StopManager stopManager)
        {
            _stopManager = stopManager;
        }

        public void Dispose()
        {
            // 和 LoopingHost 解除关系
            if (Host != null)
            {
                Host.Remove(this);
                Host = null;
            }

            if (stop != null)
            {
                stop.EndLoop();
                stop.OnStop -= _handler;
                stop.Initial("");
                stop.HideProgress();

                stop.Unregister();	// 和容器解除关联
                stop = null;
            }

            Closed?.Invoke();
            Closed = null;
        }

        public Looping(StopEventHandler handler,
            string text,
            bool activate = true)
        {
            if (_stopManager == null)
                throw new ArgumentException("尚未初始化 _stopManager");

            stop = new Stop();
            stop.Register(_stopManager, activate);	// 和容器关联

            _handler = handler;
            stop.OnStop += handler;
            stop.Initial(text);
            stop.BeginLoop();
        }

        public bool Stopped
        {
            get
            {
                if (stop != null && stop.State != 0)
                    return true;
                return false;
            }
        }
    }

    public interface ILoopingHost
    {
        Looping BeginLoop(StopEventHandler handler,
    string text,
    string style = null);

        void EndLoop(Looping looping);

        bool HasLooping();

        Looping TopLooping { get; }
    }


    public class LoopingHost
    {
        List<Looping> _loopings = new List<Looping>();
        object _syncRoot_loopings = new object();

        public Looping BeginLoop(StopEventHandler handler,
            string text,
            string style = null)
        {
            var looping = new Looping(handler, text/*, _isActive*/);
            lock (_syncRoot_loopings)
            {
                _loopings.Add(looping);
            }

            // 2022/10/29
            if (style != null)
            {
                if (isInList("halfstop", style) == true)
                    looping.stop.Style = StopStyle.EnableHalfStop;
            }

            looping.Host = this;
            return looping;
        }

        public void EndLoop(Looping looping)
        {
            looping.Dispose();
        }

        public void Remove(Looping looping)
        {
            lock (_syncRoot_loopings)
            {
                _loopings.Remove(looping);
            }
        }

        public bool HasLooping()
        {
            lock (_syncRoot_loopings)
            {
                foreach (var looping in _loopings)
                {
                    if (looping.stop != null && looping.stop.State == 0)
                        return true;
                }
                return false;
            }
        }

        public Looping TopLooping
        {
            get
            {
                lock (_syncRoot_loopings)
                {
                    if (_loopings.Count == 0)
                        return null;
                    return (_loopings[_loopings.Count - 1]);
                }
            }
        }

        public static bool isInList(string sub, string list)
        {
            if (sub == null)
                throw new ArgumentException("sub 参数值不应为 null", "sub");
            if (list == null)
                throw new ArgumentException("list 参数值不应为 null", "list");

            if (sub == list)
                return true;
            if (list.StartsWith(sub + ","))
                return true;
            if (list.EndsWith("," + "sub"))
                return true;
            return list.IndexOf("," + sub + ",") != -1;
        }
    }
}
