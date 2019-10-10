using DigitalPlatform;
using DigitalPlatform.RFID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfidCenter
{
    /// <summary>
    /// 用于模拟门锁的类，以方便没有硬件的情况下进行开发调试
    /// </summary>
    public class SimuLock
    {
        public List<bool> LockStates = new List<bool>();

        public SimuLock(int amount)
        {
            LockStates = new List<bool>();
            for (int i = 0; i < amount; i++)
            {
                LockStates.Add(false);  // 初始时是关闭状态
            }
        }

        // 开门
        public NormalResult OpenShelfLock(string lockName,
            int index)
        {
            if (index < 0 || index >= LockStates.Count)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"index({index})越过可用范围"
                };
            LockStates[index] = true;
            return new NormalResult { Value = 1 };
        }

        public GetLockStateResult GetShelfLockState(string lockName,
    int index)
        {
            if (index < 0 || index >= LockStates.Count)
                return new GetLockStateResult
                {
                    Value = -1,
                    ErrorInfo = $"index({index})越过可用范围"
                };
            List<LockState> states = new List<LockState>
            {
                new LockState
                {
                    Name = "",
                    Index = index,
                    State = LockStates[index] == true ? "open" : "close"
                }
            };

            return new GetLockStateResult
            {
                Value = 0,
                States = states
            };
        }
    }
}
