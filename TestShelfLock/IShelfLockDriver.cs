using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;

namespace TestShelfLock
{
    public interface IShelfLockDriver
    {
        // 初始化时需要提供端口号等参数
        // parameters:
        //      style   附加的子参数 
        NormalResult InitializeDriver(LockProperty property, string style);

        NormalResult ReleaseDriver();

        NormalResult OpenShelfLock(string lockName, string style);

        GetLockStateResult GetShelfLockState(string lockNameList, string style);
    }

    // 初始化 锁控 环境参数
    public class LockProperty
    {
        public string SerialPort { get; set; }  // 串口端口号
        public int LockAmountPerBoard { get; set; } // 每个板子最多的锁数量
    }

    [Serializable()]
    public class GetLockStateResult : NormalResult
    {
        public List<LockState> States { get; set; }

        // 紧凑型返回值
        public List<byte> StateBytes { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append(base.ToString() + "\r\n");
            if (this.States == null)
                text.Append("States=null\r\n");
            else
            {
                int i = 0;
                text.Append($"States.Count={States.Count}\r\n");
                foreach (var state in this.States)
                {
                    text.Append($"{i + 1}) {state.ToString()}\r\n");
                    i++;
                }
            }
            return text.ToString();
        }
    }

    [Serializable()]
    public class LockState
    {
        public string Path { get; set; }
        public string Lock { get; set; }
        public int Board { get; set; }
        public int Index { get; set; }
        public string State { get; set; }

        public override string ToString()
        {
            return $"Path={Path},Lock={Lock},Board={Board},Index={Index},State={State}";
        }
    }

}
