using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
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

    // 解析锁名称字符串以后得到的细部结构
    public class LockPath
    {
        public string LockName { get; set; }
        public List<string> CardNameList { get; set; }
        public List<string> NumberList { get; set; }

        public static LockPath Parse(string text,
            List<string> default_cardNameList = null,
            List<string> default_numberList = null)
        {
            LockPath result = new LockPath();

            result.LockName = "*";
            result.CardNameList = default_cardNameList == null ? new List<string> { "1" } : default_cardNameList;
            result.NumberList = default_numberList == null ? new List<string> { "1" } : default_numberList;

            string[] parts = text.Split(new char[] { '.' });

            if (parts.Length > 0)
                result.LockName = parts[0];
            if (parts.Length > 1 && string.IsNullOrEmpty(parts[1]) == false
                && parts[1] != "*")
                result.CardNameList = StringUtil.SplitList(parts[1], '|');
            if (parts.Length > 2 && string.IsNullOrEmpty(parts[2]) == false
                && parts[2] != "*")
                result.NumberList = StringUtil.SplitList(parts[2], '|');

            return result;
        }
    }

}
