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

        public static void Initialize(StopManager stopManager)
        {
            _stopManager = stopManager;
        }

        public void Dispose()
        {
            if (stop != null)
            {
                stop.EndLoop();
                stop.OnStop -= _handler;
                stop.Initial("");
                stop.HideProgress();

                stop.Unregister();	// 和容器解除关联
                stop = null;
            }
        }

        public Looping(StopEventHandler handler,
            string text)
        {
            if (_stopManager == null)
                throw new ArgumentException("尚未初始化 _stopManager");

            stop = new Stop();
            stop.Register(_stopManager, true);	// 和容器关联

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

}
