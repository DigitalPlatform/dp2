using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    public static class DoorStateTask
    {
        #region 后台任务

        static Task _task = null;

        // 监控间隔时间
        static TimeSpan _idleLength = TimeSpan.FromSeconds(10);

        static AutoResetEvent _activate = new AutoResetEvent(false);

        // 激活任务
        public static void ActivateTask()
        {
            _activate.Set();
        }

        // 启动状态处理后台任务
        public static void StartTask()
        {
            if (_task != null)
                return;

            CancellationToken token = App.CancelToken;

            token.Register(() =>
            {
                _activate.Set();
            });

            _task = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("状态处理后台线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(10));
                        _activate.WaitOne(_idleLength);

                        token.ThrowIfCancellationRequested();

                        //
                        await ProcessingAsync();
                    }
                    _task = null;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"状态处理后台线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("inventory_worker", $"状态处理后台线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("状态处理后台线程结束");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // 从 _entityList 中取出一批事项进行处理。由于是复制出来处理的，所以整个处理过程中(除了取出和最后删除的瞬间)不用对 _entityList 加锁
        // 对每一个事项，要进行如下处理：
        //  1) 获得册记录和书目摘要
        //  2) 尝试请求还书
        //  3) 请求设置 UID
        //  4) 修改 currentLocation 和 location
        static async Task ProcessingAsync()
        {
            var list = CopyList();
            foreach (var state in list)
            {
                // state.State = "???";
                try
                {
                    if (state.NewState == "open")
                    {
                        continue;
                    }

                    Debug.Assert(state.NewState == "close");

                    List<ActionInfo> actions = null;

                    try
                    {
                        DateTime start = DateTime.Now;

                        var result = await ShelfData.RefreshInventoryAsync(state.Door);

                        WpfClientInfo.WriteInfoLog($"针对门 {state.Door.Name} 执行 RefreshInventoryAsync() 耗时 {(DateTime.Now - start).TotalSeconds.ToString()}");

                        start = DateTime.Now;

                        // 2020/4/21 把这两句移动到 try 范围内
                        await SaveDoorActions(state.Door, true);

                        WpfClientInfo.WriteInfoLog($"针对门 {state.Door.Name} 执行 SaveDoorActions() 耗时 {(DateTime.Now - start).TotalSeconds.ToString()}");

                        start = DateTime.Now;

                        actions = ShelfData.PullActions();
                        WpfClientInfo.WriteInfoLog($"针对门 {state.Door.Name} 执行 PullActions() 耗时 {(DateTime.Now - start).TotalSeconds.ToString()}");
                    }
                    finally
                    {
                        state.Door.DecWaiting();
                        WpfClientInfo.WriteInfoLog($"--decWaiting() door '{state.Door.Name}' in ProcessingAsync()");
                    }


                    // 注: 调用完成后门控件上的 +- 数字才会消失
                    var task = PageMenu.PageShelf.DoRequestAsync(actions, "");

                    App.SetError("processing", null);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"ProcessingAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("processing", $"ProcessingAsync() 出现异常: {ex.Message}");
                }
                finally
                {
                    // state.State = "";
                }
            }

            // 把处理过的 entity 从 list 中移走
            RemoveList(list);

            // 2020/11/21
            // 如果发现队列里面又有新的对象，则立即激活任务
            if (GetListCount() > 0)
                ActivateTask();
        }

        // 将指定门的暂存的信息保存为 Action。但并不立即提交
        public async static Task SaveDoorActions(DoorItem door, bool clearOperator)
        {
            var result = await ShelfData.SaveActions((entity) =>
            {
                var results = DoorItem.FindDoors(ShelfData.Doors, entity.ReaderName, entity.Antenna);
                // TODO: 如果遇到 results.Count > 1 是否抛出异常?
                if (results.Count > 1)
                {
                    WpfClientInfo.WriteErrorLog($"读卡器名 '{entity.ReaderName}' 天线编号 {entity.Antenna} 匹配上 {results.Count} 个门");
                    throw new Exception($"读卡器名 '{entity.ReaderName}' 天线编号 {entity.Antenna} 匹配上 {results.Count} 个门。请检查 shelf.xml 并修正配置此错误，确保只匹配一个门");
                }

                if (results.IndexOf(door) != -1)
                {
                    return door.Operator;
                    // return GetOperator(entity);
                }
                return null;
            });

            if (result.Value == -1)
            {
                SetGlobalError("save_actions", $"SaveDoorActions() 出错: {result.ErrorInfo}");
                PageShelf.TrySetMessage(null, $"SaveDoorActions() 出错: {result.ErrorInfo}。这是一个严重错误，请管理员及时介入处理");
            }
            else
            {
                SetGlobalError("save_actions", null);
            }

            // 2019/12/21
            if (clearOperator == true && door.State == "close")
                door.Operator = null; // 清掉门上的操作者名字
        }

        static void SetGlobalError(string type, string error)
        {
            App.SetError(type, error);
        }

        #endregion

        public class DoorStateChange
        {
            public DoorItem Door { get; set; }
            public string OldState { get; set; }
            public string NewState { get; set; }
        }

        // 状态变化集合
        static List<DoorStateChange> _changeList = new List<DoorStateChange>();
        static object _changeListSyncRoot = new object();

        // 复制列表
        public static List<DoorStateChange> CopyList()
        {
            lock (_changeListSyncRoot)
            {
                return new List<DoorStateChange>(_changeList);
            }
        }

        // 追加元素
        public static void AppendList(DoorStateChange state)
        {
            lock (_changeListSyncRoot)
            {
                _changeList.Add(state);
            }
        }

        public static void RemoveList(List<DoorStateChange> states)
        {
            lock (_changeListSyncRoot)
            {
                if (states == null)
                    _changeList.Clear();
                else
                {
                    foreach (var entity in states)
                    {
                        _changeList.Remove(entity);
                    }
                }
            }
        }

        public static int GetListCount()
        {
            lock (_changeListSyncRoot)
            {
                return _changeList.Count;
            }
        }

    }
}
