using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;

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

        GetLockStateResult GetShelfLockState(string lockNameList);
    }

    // 初始化 锁控 环境参数
    public class LockProperty
    {
        public string SerialPort { get; set; }  // 串口端口号
        public int LockAmountPerBoard { get; set; } // 每个板子最多的锁数量
    }
}
