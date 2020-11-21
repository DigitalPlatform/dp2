using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// 记忆门锁状态的类。
    /// 用于优化检测门锁状态过程。请求锁控探测状态是要消耗一定时间的，优化的策略是，关闭状态的门锁不必请求锁控，只有打开状态的门锁才有必要请求锁控探测
    /// </summary>
    public class LockStateMemory
    {
        // lock path --> bool(false 表示关闭，true 表示打开)
        // 没有包含的路径，对应锁状态为“未知”
        Hashtable _stateTable = new Hashtable();

        // 曾经打开过的标志表。
        // 每当前端 API 获取锁状态时使用一次并清除相关对象。也就是说确保锁被打开后，获取状态至少有一次是获得一次打开状态
        // lock path --> DateTime
        Hashtable _openedTable = new Hashtable();

        public void Clear()
        {
            lock (_stateTable.SyncRoot)
            {
                _stateTable.Clear();
            }

            lock (_openedTable.SyncRoot)
            {
                _openedTable.Clear();
            }
        }

        #region 记忆短暂打开过的机制

        // 记忆一次打开
        public void MemoryOpen(string path)
        {
            lock (_openedTable.SyncRoot)
            {
                _openedTable[path] = DateTime.Now;
            }
        }

        // 是否刚才打开过？(虽然判断的当时锁未必是打开状态，但曾经打开过，并且没有被查询过状态)
        public bool IsOpened(string path)
        {
            lock (_openedTable.SyncRoot)
            {
                bool opened = _openedTable.ContainsKey(path);
                if (opened)
                    _openedTable.Remove(path);
                return opened;
            }
        }

        #endregion

        // 设定是否追踪
        // parameters:
        //      path    锁路径
        //      state   锁状态，为 open/close/空 之一。空表示状态未知
        public void Set(string path, string state)
        {
            // 检查 path 中不应该包含星号
            CheckPath(path);

            lock (_stateTable.SyncRoot)
            {
                if (state == "open")
                    _stateTable[path] = true;
                else if (state == "close")
                    _stateTable[path] = false;
                else
                    _stateTable.Remove(path);
            }
        }

        void CheckPath(string path)
        {
            if (path.Contains("*"))
                throw new ArgumentException($"锁路径 '{path}' 不合法，不应包含星号");
        }

        // 返回锁的记忆状态。
        // 记忆状态是为了优化追踪过程。只有必要处于打开状态的锁。因为只有打开状态的锁才有必要追踪它是否被关闭
        public string GetState(string path)
        {
            // 检查 path 中不应该包含星号
            CheckPath(path);

            lock (_stateTable.SyncRoot)
            {
                if (_stateTable.ContainsKey(path) == false)
                    return null;
                bool ret = (bool)_stateTable[path];
                return ret ? "open" : "close";
            }
        }
    }
}
