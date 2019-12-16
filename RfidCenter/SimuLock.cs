using DigitalPlatform;
using DigitalPlatform.RFID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RfidDrivers.First.Driver1;

namespace RfidCenter
{
    // TODO: 需要改造
    /// <summary>
    /// 用于模拟门锁的类，以方便没有硬件的情况下进行开发调试
    /// </summary>
    public class SimuLock
    {
        public List<SimuLockState> LockStates = new List<SimuLockState>();

        // parameters:
        //      board_amount    锁控板数量
        //      lock_amount     每个锁控板能控制得锁数量
        public SimuLock(int board_amount, int lock_amount)
        {
            LockStates = new List<SimuLockState>();
            for (int j = 0; j < board_amount; j++)
            {
                for (int i = 0; i < lock_amount; i++)
                {
                    LockStates.Add(new SimuLockState
                    {
                        Path = $"simu.{j + 1}.{i + 1}",
                        State = "close"
                    });  // 初始时是关闭状态
                }
            }
        }

        // 开门
        public NormalResult OpenShelfLock(string lockNameParam, bool open = true)
        {
            var path = LockPath.Parse(lockNameParam);

            int count = 0;
            foreach (var card in path.CardNameList)
            {
                foreach (var number in path.NumberList)
                {
                    string search_path = $"{path.LockName}.{card}.{number}";
                    var state = FindLockState(search_path);
                    if (state == null)
                        return new GetLockStateResult
                        {
                            Value = -1,
                            ErrorInfo = $"当前不存在路径为 '{search_path}' 的模拟门锁对象",
                            ErrorCode = "lockNotFound"
                        };

                    state.State = open ? "open" : "close";
                    count++;
                }
            }

            return new NormalResult
            {
                Value = count
            };
        }

        /*
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
        */

        /*
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
                    Lock = "",
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
        */

        public GetLockStateResult GetShelfLockState(string lockNameParam)
        {
            var path = LockPath.Parse(lockNameParam);

            /*
            if (locks.Count == 0)
                return new GetLockStateResult
                {
                    Value = -1,
                    ErrorInfo = $"当前不存在名为 '{path.LockName}' 的模拟门锁对象",
                    ErrorCode = "lockNotFound"
                };
                */

            List<LockState> states = new List<LockState>();

            foreach (var card in path.CardNameList)
            {
                foreach (var number in path.NumberList)
                {
                    string search_path = $"{path.LockName}.{card}.{number}";
                    var state = FindLockState(search_path);
                    if (state == null)
                        return new GetLockStateResult
                        {
                            Value = -1,
                            ErrorInfo = $"当前不存在路径为 '{search_path}' 的模拟门锁对象",
                            ErrorCode = "lockNotFound"
                        };
                    var current_path = LockPath.Parse(state.Path);
                    states.Add(new LockState
                    {
                        // Path
                        Path = state.Path,
                        // 锁名字
                        Lock = current_path.LockName,
                        Board = Convert.ToInt32(current_path.CardNameList[0]),
                        Index = Convert.ToInt32(current_path.NumberList[0]),
                        State = state.State,
                    });
                }
            }

            return new GetLockStateResult
            {
                Value = 0,
                States = states
            };
        }

        /*
        List<SimuLockState> GetLocksByName(string lock_name)
        {
            List<SimuLockState> results = new List<SimuLockState>();
            foreach (var state in LockStates)
            {
                if (Reader.MatchReaderName(lock_name, state.Path, out string antenna_list))
                    results.Add(state);
            }

            return results;
        }
        */

        SimuLockState FindLockState(string pathParam)
        {
            var path = LockPath.Parse(pathParam);
            if (path.CardNameList.Count != 1)
                throw new Exception($"pathParam '{pathParam}' 不合法。第二段应该只包含一个数字");
            if (path.NumberList.Count != 1)
                throw new Exception($"pathParam '{pathParam}' 不合法。第三段应该只包含一个数字");

            foreach (var state in LockStates)
            {
                var current_path = LockPath.Parse(state.Path);
                if (current_path.LockName == "*")
                    throw new Exception("LockStates 中的 LockName 不允许使用星号");
                if (current_path.CardNameList.Count != 1)
                    throw new Exception($"LockStates 中的 Path '{state.Path}' 不合法。第二段应该只包含一个数字");
                if (current_path.NumberList.Count != 1)
                    throw new Exception($"LockStates 中的 Path '{state.Path}' 不合法。第三段应该只包含一个数字");
                if (path.LockName != "*"
                    && current_path.LockName != path.LockName)
                    continue;
                if (current_path.CardNameList[0] != path.CardNameList[0])
                    continue;
                if (current_path.NumberList[0] != path.NumberList[0])
                    continue;
                return state;
            }

            return null;
        }
    }

    public class SimuLockState
    {
        public string Path { get; set; }
        public string State { get; set; }

    }
}
