using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public class BaseChannel<T>
    {
        public IpcClientChannel Channel { get; set; }
        public T Object { get; set; }
        // 通道已经成功启动。意思是已经至少经过一个 API 调用并表现正常
        public bool Started { get; set; }
    }

}
