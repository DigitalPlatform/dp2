using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml;

using Newtonsoft.Json;

using Microsoft.VisualStudio.Threading;

using static dp2SSL.LibraryChannelUtil;

using DigitalPlatform;
using DigitalPlatform.WPF;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer;
using System.Windows.Markup;
using DigitalPlatform.Xml;

namespace dp2SSL
{
    /// <summary>
    /// 智能书架要用到的数据
    /// </summary>
    public static partial class ShelfData
    {
#if DOOR_MONITOR
        public static DoorMonitor DoorMonitor = null;
#endif
        #region CancellationTokenSource

        static CancellationTokenSource _cancel = new CancellationTokenSource();

        public static CancellationToken CancelToken
        {
            get
            {
                return _cancel.Token;
            }
        }

        public static void CancelAll()
        {
            _cancel.Cancel();
        }

        #endregion

        public static event OpenCountChangedEventHandler OpenCountChanged;

        public static event DoorStateChangedEventHandler DoorStateChanged;

        /*
        public static event BookChangedEventHandler BookChanged;

        public static void TriggerBookChanged(BookChangedEventArgs e)
        {
            BookChanged?.Invoke(null, e);
        }
        */

        // 读者证读卡器名字。在 shelf.xml 中配置
        static string _patronReaderName = "";

        public static string PatronReaderName
        {
            get
            {
                return _patronReaderName;
            }
        }


        // 图书读卡器名字列表(也就是柜门里面的那些读卡器)
        static string _allDoorReaderName = "";

        public static string DoorReaderName
        {
            get
            {
                return _allDoorReaderName;
            }
        }

        // 当前处于打开状态的门的个数
        public static int OpeningDoorCount
        {
            get
            {
                return _openingDoorCount;
            }
        }

        /*
        public static void ProcessOpenCommand(DoorItem door, string comment)
        {
            // 切换所有者
            var command = ShelfData.PopCommand(door, comment);
            if (command != null)
            {
                door.DecWaiting();
                //WpfClientInfo.WriteInfoLog($"--decWaiting() door '{door.Name}' pop command");
                door.Operator = command.Parameter as Operator;
            }
            else
            {
                WpfClientInfo.WriteErrorLog($"!!! 门 {door.Name} PopCommand() 时候没有找到命令对象");
            }
        }
        */

        static int _openingDoorCount = -1; // 当前处于打开状态的门的个数。-1 表示个数尚未初始化


        public static void RfidManager_ListLocks(object sender, ListLocksEventArgs e)
        {
            if (e.Result.Value == -1)
            {
                // TODO: 注意这里的信息量很大，需要防止错误日志空间被耗尽
                //WpfClientInfo.WriteErrorLog($"RfidManager ListLocks error: {e.Result.ErrorInfo}");
                return;
            }

            List<DoorItem> processed = new List<DoorItem>();
            // bool triggerAllClosed = false;
            {
                int count = 0;
                foreach (var state in e.Result.States)
                {
                    if (state.State == "open")
                        count++;

                    // 刷新门锁对象的 State 状态
                    var results = DoorItem.SetLockState(ShelfData.Doors, state);
                    // 注：有可能一个锁和多个门关联
                    foreach (LockChanged result in results)
                    {
                        if (result.NewState != result.OldState
                            && string.IsNullOrEmpty(result.OldState) == false)
                        {
                            // 触发单独一个门被关闭的事件
                            // 注意此时 door 对象的 State 状态已经变化为新状态了
                            DoorStateChanged?.Invoke(null, new DoorStateChangedEventArgs
                            {
                                Door = result.Door,
                                OldState = result.OldState,
                                NewState = result.NewState
                            });

                            processed.Add(result.Door);

                            if (result.NewState == "open")
                                App.CurrentApp.SpeakSequence($"{result.LockName} 打开");
                            else
                                App.CurrentApp.SpeakSequence($"{result.LockName} 关闭");
                        }
                    }
                }

                //if (_openingDoorCount > 0 && count == 0)
                //    triggerAllClosed = true;

                SetOpenCount(count);
            }

#if DOOR_MONITOR
            ShelfData.DoorMonitor?.ProcessTimeout();
#endif

#if REMOVED
            // TODO: 如果刚才已经获得了一个门锁的关门信号，则后面不要重复触发 DoorStateChanged 

            // 2019/12/16
            // 对可能遗漏 Pop 的 命令进行检查
            {
                // 检查命令队列中可能被 getLockState 轮询状态所遗漏的命令
                var missing_commands = CheckCommands(RfidManager.LockHeartbeat);
                if (missing_commands.Count > 0)
                {
                    foreach (var command in missing_commands)
                    {
                        // 如果此门状态不是关闭状态，则不需要进行修补处理
                        if (command.Door.State != "close")
                            continue;

                        // 2019/12/18
                        // 前面正常处理流程已经触发过这个门的状态变化事件了
                        if (processed.IndexOf(command.Door) != -1)
                            continue;

                        // ProcessOpenCommand(command.Door);
                        // 补一次门状态变化? --> open --> close
                        // 触发单独一个门被关闭的事件
                        DoorStateChanged?.Invoke(null, new DoorStateChangedEventArgs
                        {
                            Door = command.Door,
                            OldState = "close",
                            NewState = "open",
                            Comment = $"补做。Heartbeat={RfidManager.LockHeartbeat}",
                        });
                        DoorStateChanged?.Invoke(null, new DoorStateChangedEventArgs
                        {
                            Door = command.Door,
                            OldState = "open",
                            NewState = "close",
                            Comment = $"补做。Heartbeat={RfidManager.LockHeartbeat}",
                        });
                        App.CurrentApp.Speak("补做检查");   // 测试完成后可以取消这个语音
                        WpfClientInfo.WriteInfoLog($"提醒：检查过程为门 '{command.Door.Name}' 补做了一次 open 和 一次 close。Heartbeat:{RfidManager.LockHeartbeat}");
                    }
                }
            }
#endif
        }

        // 设置打开门数量
        static void SetOpenCount(int count)
        {
            int oldCount = _openingDoorCount;

            _openingDoorCount = count;

            // 打开门的数量发生变化
            if (oldCount != _openingDoorCount)
            {
                OpenCountChanged?.Invoke(null, new OpenCountChangedEventArgs
                {
                    OldCount = oldCount,
                    NewCount = count
                });

                // 
                RefreshReaderNameList();
            }
        }



        // 保存一个已经打开的灯的门名字表。只要有一个以上事项，就表示要开灯；如果一个事项也没有，就表示要关灯
        // 门名字 --> bool
        static Hashtable _lampTable = new Hashtable();

        // parameters:
        //      style   on 或者 off
        //              delay   表示延迟关灯
        //              skip 表示不真正开关物理灯，只是改变 hashtable 里面计数
        public static void TurnLamp(string doorName, string style)
        {
            bool on = StringUtil.IsInList("on", style);
            int oldCount = _lampTable.Count;

            if (on)
                _lampTable[doorName] = true;
            else
                _lampTable.Remove(doorName);

            int newCount = _lampTable.Count;

            if (oldCount == 0 && newCount > 0)
            {
                if (StringUtil.IsInList("skip", style) == false)
                {
                    // 用控件模拟灯亮灭，便于调试
                    PageMenu.PageShelf?.SimulateLamp(true);
                    RfidManager.TurnShelfLamp("*", "turnOn");   // TODO: 遇到出错如何报错?
                }
            }
            else if (oldCount > 0 && newCount == 0)
            {
                if (StringUtil.IsInList("delay", style))
                    BeginDelayTurnOffTask();
                else
                {
                    // 用控件模拟灯亮灭，便于调试
                    PageMenu.PageShelf.SimulateLamp(false);
                    RfidManager.TurnShelfLamp("*", "turnOff");
                }
            }
        }

        #region 延迟关灯

        static DelayAction _delayTurnOffTask = null;

        public static void CancelDelayTurnOffTask()
        {
            if (_delayTurnOffTask != null)
            {
                _delayTurnOffTask.Cancel.Cancel();
                _delayTurnOffTask = null;
            }
        }

        public static void BeginDelayTurnOffTask()
        {
            CancelDelayTurnOffTask();

            // 让灯继续亮着
            ShelfData.TurnLamp("~", "on,skip");

            // TODO: 开始启动延时自动清除读者信息的过程。如果中途门被打开，则延时过程被取消(也就是说读者信息不再会被自动清除)
            _delayTurnOffTask = DelayAction.Start(
                20,
                () =>
                {
                    ShelfData.TurnLamp("~", "off");
                },
                (seconds) =>
                {
                    /*
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (seconds > 0)
                            this.clearPatron.Content = $"({seconds.ToString()} 秒后自动) 清除读者信息";
                        else
                            this.clearPatron.Content = $"清除读者信息";
                    }));
                    */
                });
        }

        #endregion


        public static void RefreshReaderNameList()
        {
            if (_openingDoorCount == 0)
            {
                /*
                // 关闭图书读卡器(只使用读者证读卡器)
                if (string.IsNullOrEmpty(_patronReaderName) == false
                    && RfidManager.ReaderNameList != _patronReaderName)
                {
                    // RfidManager.ReaderNameList = _patronReaderName;
                    RfidManager.ReaderNameList = "";    // 图书读卡器全部停止盘点。此处假定读者证读卡器在第二线程遍历
                    RfidManager.ClearCache();
                    //App.CurrentApp.SpeakSequence("静止");
                }
                */
                // 关闭图书读卡器(只使用读者证读卡器)
                if (RfidManager.ReaderNameList != "")
                {
                    // RfidManager.ReaderNameList = _patronReaderName;
                    RfidManager.ReaderNameList = "";    // 图书读卡器全部停止盘点。此处假定读者证读卡器在第二线程遍历
                    RfidManager.ClearCache();
                    //App.CurrentApp.SpeakSequence("静止");
                }

            }
            else
            {
                string list = "";
                if (App.DetectBookChange == true)
                    list = GetReaderNameList(Doors,
                        (d) =>
                        {
                            return (d.State == "open");
                        });

                // 打开图书读卡器(同时也使用读者证读卡器)
                if (RfidManager.ReaderNameList != list)
                {
                    RfidManager.ReaderNameList = list;
                    RfidManager.ClearCache();
                    //App.CurrentApp.SpeakSequence("活动");
                }
            }
        }

        static List<string> _locationList = null;

        static string _rightTableXml = null;

        // exception:
        //      可能会抛出异常
        public static NormalResult InitialShelf()
        {
            try
            {
                ShelfData.InitialDoors();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"InitialDoors() 出现异常: {ex.Message}"
                };
            }

            {
                // 获得馆藏地列表
                GetLocationListResult result = null;
                if (App.StartNetworkMode == "local")
                {
                    result = LibraryChannelUtil.GetLocationListFromLocal();
                    if (result.Value == 0)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "本地没有馆藏地定义信息。需要联网以后重新启动"
                        };
                }
                else
                    result = LibraryChannelUtil.GetLocationList();

                if (result.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"获得馆藏地列表时出错: {result.ErrorInfo}"
                    };
                else
                    _locationList = result.List;
            }

            if (App.StartNetworkMode == "local")
            {
                _rightTableXml = WpfClientInfo.Config.Get("cache", "rightsTable", null);
                if (string.IsNullOrEmpty(_rightTableXml))
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "本地没有读者借阅权限定义信息。需要联网以后重新启动"
                    };
            }
            else
            {
                // 获得读者借阅权限定义
                GetRightsTableResult get_result = LibraryChannelUtil.GetRightsTable();
                if (get_result.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"获得读者借阅权限定义 XML 时出错: {get_result.ErrorInfo}"
                    };
                _rightTableXml = get_result.Xml;
                // 顺便保存起来
                WpfClientInfo.Config.Set("cache", "rightsTable", _rightTableXml);
            }

            // 要在初始化以前设定好
            _patronReaderName = GetAllReaderNameList("patron");
            WpfClientInfo.WriteInfoLog($"patron ReaderNameList '{_patronReaderName}'");

            RfidManager.Base2ReaderNameList = _patronReaderName;    // 2019/11/18
            // RfidManager.LockThread = "base2";   // 使用第二个线程来监控门锁

            _allDoorReaderName = GetAllReaderNameList("doors");
            WpfClientInfo.WriteInfoLog($"doors ReaderNameList '{_allDoorReaderName}'");

#if OLD_VERSION
            RfidManager.ReaderNameList = _allDoorReaderName;
#else
            RfidManager.ReaderNameList = "";    // 假定一开始门是关闭的
#endif

            RfidManager.LockCommands = StringUtil.MakePathList(ShelfData.GetLockCommands());

            WpfClientInfo.WriteInfoLog($"LockCommands '{RfidManager.LockCommands}'");

            // _patronReaderName = GetPatronReaderName();
            return new NormalResult();
        }

        // 从 shelf.xml 配置文件中获得读者证读卡器名
        public static string GetPatronReaderName()
        {
            if (ShelfCfgDom == null)
                return "";

            XmlElement patron = ShelfCfgDom.DocumentElement.SelectSingleNode("patron") as XmlElement;
            if (patron == null)
                return "";

            return patron.GetAttribute("readerName");


            /*
            string cfg_filename = ShelfData.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.Load(cfg_filename);

                XmlElement patron = cfg_dom.DocumentElement.SelectSingleNode("patron") as XmlElement;
                if (patron == null)
                    return "";

                return patron.GetAttribute("readerName");
            }
            catch (FileNotFoundException)
            {
                return "";
            }
            catch (Exception ex)
            {
                this.SetError("cfg", $"装载配置文件 shelf.xml 时出现异常: {ex.Message}");
                return "";
            }
            */
        }


        // 从 shelf.xml 中归纳出每个 door 读卡器的天线编号列表
        public static List<AntennaList> GetAntennaTable()
        {
            if (ShelfCfgDom == null)
                return new List<AntennaList>();

            // 读卡器名字 --> List<int> (天线列表)
            Hashtable name_table = new Hashtable();

            {
                XmlNodeList doors = ShelfCfgDom.DocumentElement.SelectNodes("shelf/door");
                foreach (XmlElement door in doors)
                {
                    DoorItem.ParseReaderString(door.GetAttribute("antenna"),
                        out string readerName,
                        out int antenna);

                    // 禁止使用 * 作为读卡器名字
                    if (readerName == "*")
                        throw new Exception($"antenna属性值中读卡器名字部分不应使用 * ({door.OuterXml})");

                    // 跳过空读卡器名
                    if (string.IsNullOrEmpty(readerName))
                        continue;

                    AddToTable(name_table, readerName, antenna);
                }
            }

            List<AntennaList> results = new List<AntennaList>();
            foreach (string key in name_table.Keys)
            {
                List<int> list = name_table[key] as List<int>;
                list.Sort();

                results.Add(new AntennaList
                {
                    ReaderName = key,
                    Antennas = list
                });
            }

            return results;
        }


        // 从 shelf.xml 配置文件中归纳出所有的读卡器名，包括天线编号部分
        // parameters:
        //      style   patron/doors
        public static string GetAllReaderNameList(string style)
        {
            if (ShelfCfgDom == null)
                return "*";

            // 读卡器名字 --> List<int> (天线列表)
            Hashtable name_table = new Hashtable();

            if (StringUtil.IsInList("doors", style))
            {
                XmlNodeList doors = ShelfCfgDom.DocumentElement.SelectNodes("shelf/door");
                foreach (XmlElement door in doors)
                {
                    DoorItem.ParseReaderString(door.GetAttribute("antenna"),
                        out string readerName,
                        out int antenna);

                    // 禁止使用 * 作为读卡器名字
                    if (readerName == "*")
                        throw new Exception($"antenna属性值中读卡器名字部分不应使用 * ({door.OuterXml})");

                    // 跳过空读卡器名
                    if (string.IsNullOrEmpty(readerName))
                        continue;

                    AddToTable(name_table, readerName, antenna);
                }
            }

            if (StringUtil.IsInList("patron", style))
            {
                XmlElement patron = ShelfCfgDom.DocumentElement.SelectSingleNode("patron") as XmlElement;
                if (patron != null)
                {
                    string readerName = patron.GetAttribute("readerName");
                    AddToTable(name_table, readerName, -1);
                }
            }

            StringBuilder result = new StringBuilder();
            int i = 0;
            foreach (string key in name_table.Keys)
            {
                List<int> list = name_table[key] as List<int>;
                list.Sort();

                if (i > 0)
                    result.Append(",");
                if (list.Count == 0)
                    result.Append(key);
                else
                    result.Append($"{key}:{Join(list, "|")}");
                i++;
            }

            return result.ToString();
        }

        // return:
        //      true 选中
        //      false 希望跳过
        public delegate bool Delegate_selectDoor(DoorItem door);

        // 获得处于打开状态的门的读卡器名字符串
        // parameters:
        public static string GetReaderNameList(List<DoorItem> _doors,
            Delegate_selectDoor func_select)
        {
            // 读卡器名字 --> List<int> (天线列表)
            Hashtable name_table = new Hashtable();
            foreach (var door in _doors)
            {
                var readerName = door.ReaderName;
                var antenna = door.Antenna;

                // 跳过空读卡器名
                if (string.IsNullOrEmpty(readerName))
                    continue;

                // 禁止使用 * 作为读卡器名字
                if (readerName == "*")
                    throw new Exception($"antenna属性值中读卡器名字部分不应使用 *");

                /*
                if (door.State != "open")
                    continue;
                    */
                if (func_select?.Invoke(door) == false)
                    continue;

                AddToTable(name_table, readerName, antenna);
            }

            StringBuilder result = new StringBuilder();
            int i = 0;
            foreach (string key in name_table.Keys)
            {
                List<int> list = name_table[key] as List<int>;
                list.Sort();

                if (i > 0)
                    result.Append(",");
                if (list.Count == 0)
                    result.Append(key);
                else
                    result.Append($"{key}:{Join(list, "|")}");
                i++;
            }

            return result.ToString();
        }

        static void AddToTable(Hashtable name_table, string readerName, int antenna)
        {
            List<int> list = new List<int>();
            if (name_table.ContainsKey(readerName) == false)
            {
                name_table[readerName] = list;
            }
            else
                list = name_table[readerName] as List<int>;

            if (antenna != -1)
            {
                if (list.IndexOf(antenna) == -1)
                    list.Add(antenna);
            }
        }

        static string Join(List<int> list, string sep)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var v in list)
            {
                if (i > 0)
                    text.Append(sep);
                text.Append(v.ToString());
                i++;
            }
            return text.ToString();
        }

        // 显示对书柜门的 Iventory 操作，同一时刻只能一个函数进入
        static AsyncSemaphore _inventoryLimit = new AsyncSemaphore(1);

        // 单独对一个门关联的 RFID 标签进行一次 inventory，确保此前的标签变化情况没有被遗漏
        public static async Task<NormalResult> RefreshInventoryAsync(DoorItem door)
        {
            // 获得和一个门相关的 readernamelist
            var list = GetReaderNameList(new List<DoorItem> { door }, null);
            string style = $"dont_delay";   // 确保 inventory 并立即返回

            using (var releaser = await _inventoryLimit.EnterAsync().ConfigureAwait(false))
            {
                // StringBuilder debugInfo = new StringBuilder();
                var result = RfidManager.CallListTags(list, style);
                // WpfClientInfo.WriteErrorLog($"RefreshInventory() list={list}, style={style}, result={result.ToString()}");

                try
                {
                    await RfidManager.TriggerListTagsEvent(list, result, true);
                    return new NormalResult();
                }
                catch (TagInfoException ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"对门 {door.Name} 内的全部标签进行盘点时，发现无法解析的标签(UID:{ex.TagInfo.UID})",
                        ErrorCode = "tagParseError"
                    };
                }
                catch (Exception ex)
                {
                    // WpfClientInfo.WriteErrorLog($"RefreshInventory() TriggerListTagsEvent() 异常:{ExceptionUtil.GetDebugText(ex)}\r\ndebugInfo={debugInfo.ToString()}");
                    WpfClientInfo.WriteErrorLog($"RefreshInventory() TriggerListTagsEvent() list='{list}' 异常:{ExceptionUtil.GetDebugText(ex)}");
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"RefreshInventory() 出现异常(门:{door.Name}): {ex.Message}",
                        ErrorCode = ex.GetType().ToString()
                    };
                }
            }
        }

        static XmlDocument _shelfCfgDom = null;

        public static XmlDocument ShelfCfgDom
        {
            get
            {
                return _shelfCfgDom;
            }
        }

        public static string ShelfFilePath
        {
            get
            {
                string cfg_filename = System.IO.Path.Combine(WpfClientInfo.UserDir, "shelf.xml");
                return cfg_filename;
            }
        }

        static List<DoorItem> _doors = new List<DoorItem>();
        public static List<DoorItem> Doors
        {
            get
            {
                return _doors;
            }
        }

        // 累积的全部 action
        static List<ActionInfo> _actions = new List<ActionInfo>();
        public static IReadOnlyCollection<ActionInfo> Actions
        {
            get
            {
                lock (_syncRoot_actions)
                {
                    return new List<ActionInfo>(_actions);
                    // return _actions;
                }
            }
        }

        // 获得 Actions，并同时从 _actions 中移走
        public static List<ActionInfo> PullActions()
        {
            lock (_syncRoot_actions)
            {
                var results = new List<ActionInfo>(_actions);
                _actions.Clear();
                return results;
            }
        }

        // 用于保护 Actions 数据结构的锁对象
        static object _syncRoot_actions = new object();

        public delegate Operator Delegate_getOperator(Entity entity);

        public class OperationInfo
        {
            // 操作名称
            public string Operation { get; set; }
            public Entity Entity { get; set; }
            public string Location { get; set; }    // 目标馆藏地(调拨)
            public string ShelfNo { get; set; }     // 目标架位(上下架)
            public Operator Operator { get; set; }
        }

        // 根据 ActionInfo 对象构建 OperationInfo 对象
        public static List<OperationInfo> BuildOperationInfos(List<ActionInfo> actions)
        {
            List<OperationInfo> results = new List<OperationInfo>();
            foreach (var action in actions)
            {
                if (action.Action == "return")
                {
                    var operation = new OperationInfo
                    {
                        Operation = "还书",
                        Entity = action.Entity,
                        Operator = action.Operator,
                        ShelfNo = ShelfData.GetShelfNo(action.Entity),
                    };
                    /*
                    if (action.Operator.IsWorker == true)
                    {
                        operation.Operation = "转入";
                    }
                    */
                    results.Add(operation);
                }

                if (action.Action == "borrow")
                {
                    var operation = new OperationInfo
                    {
                        Operation = "借书",
                        Entity = action.Entity,
                        Operator = action.Operator,
                        ShelfNo = ShelfData.GetShelfNo(action.Entity),
                    };
                    /*
                    if (action.Operator.IsWorker == true)
                    {
                        operation.Operation = "转出";
                    }
                    */
                    results.Add(operation);
                }

                if (action.Action == "transfer" && action.TransferDirection == "in")
                {
                    string name = "上架";
                    if (string.IsNullOrEmpty(action.Location) == false)
                        name = "上架+调入";
                    var operation = new OperationInfo
                    {
                        Operation = name,
                        Entity = action.Entity,
                        Operator = action.Operator,
                        Location = action.Location,
                        ShelfNo = action.CurrentShelfNo,
                    };

                    results.Add(operation);
                }

                if (action.Action == "transfer" && action.TransferDirection == "out")
                {
                    string name = "下架";
                    if (string.IsNullOrEmpty(action.Location) == false)
                        name = "下架+调出";

                    var operation = new OperationInfo
                    {
                        Operation = name,
                        Entity = action.Entity,
                        Operator = action.Operator,
                        Location = action.Location,
                        ShelfNo = action.CurrentShelfNo,
                    };

                    results.Add(operation);
                }
            }
            return results;
        }

        public class SaveActionResult : NormalResult
        {
            // public List<OperationInfo> Operations { get; set; }
            public List<ActionInfo> Actions { get; set; }
        }

        // 将暂存的信息保存为 Action。但并不立即提交
        // parameters:
        //      patronBarcode   读者证条码号。如果为 "*"，表示希望针对全部读者的都提交
        public static SaveActionResult SaveActions(
            // string patronBarcode,
            Delegate_getOperator func_getOperator)
        {
            // List<OperationInfo> infos = new List<OperationInfo>();
            try
            {
                lock (_syncRoot_actions)
                {
                    List<ActionInfo> actions = new List<ActionInfo>();
                    List<Entity> processed = new List<Entity>();
                    foreach (var entity in ShelfData.l_Adds)
                    {
                        // Debug.Assert(string.IsNullOrEmpty(entity.PII) == false, "");

                        if (ShelfData.BelongToNormal(entity) == false)
                            continue;
                        var person = func_getOperator?.Invoke(entity);
                        if (person == null)
                            continue;


                        actions.Add(new ActionInfo
                        {
                            Entity = entity.Clone(),
                            Action = "return",
                            Operator = person,
                        });
                        // 没有更新的，才进行一次 transfer。更新的留在后面专门做
                        // “更新”的意思是从这个门移动到了另外一个门
                        if (ShelfData.Find(ShelfData.l_Changes, (o) => o.UID == entity.UID).Count == 0)
                        {
                            string location = "";
                            // 工作人员身份，还可能要进行馆藏位置向内转移
                            if (person.IsWorker == true)
                            {
                                location = GetLocationPart(ShelfData.GetShelfNo(entity));
                            }
                            actions.Add(new ActionInfo
                            {
                                Entity = entity.Clone(),
                                Action = "transfer",
                                TransferDirection = "in",
                                Location = location,
                                CurrentShelfNo = ShelfData.GetShelfNo(entity),
                                Operator = person
                            });
                        }

                        /*
                        // 用于显示的操作信息
                        {
                            var operation = new OperationInfo
                            {
                                Operation = "还书",
                                Entity = entity,
                                Operator = person,
                                ShelfNo = ShelfData.GetShelfNo(entity),
                            };
                            if (person.IsWorker == true)
                            {
                                operation.Operation = "转入";
                            }
                            infos.Add(operation);
                        }
                        */

                        processed.Add(entity);

                        // 2020/4/2
                        ShelfData.Add("all", entity);

                        // 2020/4/2
                        // 还书操作前先尝试修改 EAS
                        if (entity.Error == null && entity.ErrorCode != "patronCard")
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, false);
                            if (result.Value == -1)
                            {
                                string text = $"修改 EAS 动作失败: {result.ErrorInfo}";
                                entity.SetError(text, "yellow");

                                // 写入错误日志
                                WpfClientInfo.WriteInfoLog($"修改册 '{entity.PII}' 的 EAS 失败: {result.ErrorInfo}");
                            }
                        }

                    }

                    foreach (var entity in ShelfData.l_Changes)
                    {
                        // Debug.Assert(string.IsNullOrEmpty(entity.PII) == false, "");

                        if (ShelfData.BelongToNormal(entity) == false)
                            continue;
                        var person = func_getOperator?.Invoke(entity);
                        if (person == null)
                            continue;

                        string location = "";
                        // 工作人员身份，还可能要进行馆藏位置转移
                        if (person.IsWorker == true)
                        {
                            location = GetLocationPart(ShelfData.GetShelfNo(entity));
                        }
                        // 更新
                        actions.Add(new ActionInfo
                        {
                            Entity = entity.Clone(),
                            Action = "transfer",
                            TransferDirection = "in",
                            Location = location,
                            CurrentShelfNo = ShelfData.GetShelfNo(entity),
                            Operator = person
                        });

                        /*
                        // 用于显示的操作信息
                        {
                            var operation = new OperationInfo
                            {
                                Operation = "调整位置",
                                Entity = entity,
                                Operator = person,
                                ShelfNo = ShelfData.GetShelfNo(entity),
                            };

                            infos.Add(operation);
                        }
                        */

                        processed.Add(entity);
                    }

                    // int borrowed_count = 0;
                    List<string> borrowed_piis = new List<string>();
                    foreach (var entity in ShelfData.l_Removes)
                    {
                        // Debug.Assert(string.IsNullOrEmpty(entity.PII) == false, "");

                        if (ShelfData.BelongToNormal(entity) == false)
                            continue;
                        var person = func_getOperator?.Invoke(entity);
                        if (person == null)
                            continue;

                        // 2020/4/19
                        // 检查一下 actions 里面是否已经有了针对同一个 PII 的 return 动作。
                        // 如果已经有了，则删除 return 动作，并且也忽略新的 borrow 动作
                        var returns = actions.FindAll(o => o.Action == "return" && o.Entity.PII == entity.PII);
                        if (returns.Count > 0)
                        {
                            foreach (var r in returns)
                            {
                                actions.Remove(r);
                            }
                            continue;
                        }

                        if (person.IsWorker == false)
                        {
                            // 只有读者身份才进行借阅操作
                            actions.Add(new ActionInfo
                            {
                                Entity = entity.Clone(),
                                Action = "borrow",
                                Operator = person,
                                ActionString = BuildBorrowInfo(person.PatronBarcode, entity, borrowed_piis), // borrowed_count++
                            });

                            borrowed_piis.Add(entity.PII);
                        }

                        //
                        if (person.IsWorker == true)
                        {
                            // 工作人员身份，还可能要进行馆藏位置向外转移
                            string location = "%checkout_location%";
                            actions.Add(new ActionInfo
                            {
                                Entity = entity.Clone(),
                                Action = "transfer",
                                TransferDirection = "out",
                                Location = location,
                                // 注: ShelfNo 成员不使用。意在保持册记录中 currentLocation 元素不变
                                Operator = person
                            });
                        }

                        /*
                        // 用于显示的操作信息
                        {
                            var operation = new OperationInfo
                            {
                                Operation = "借书",
                                Entity = entity,
                                Operator = person,
                                ShelfNo = ShelfData.GetShelfNo(entity),
                            };
                            if (person.IsWorker == true)
                            {
                                operation.Operation = "转出";
                            }
                            infos.Add(operation);
                        }
                        */

                        processed.Add(entity);

                        // 2020/4/2
                        ShelfData.Remove("all", entity);
                    }

                    /*
                    foreach (var entity in processed)
                    {
                        ShelfData.Remove("all", entity);
                        ShelfData.Remove("adds", entity);
                        ShelfData.Remove("removes", entity);
                        ShelfData.Remove("changes", entity);
                    }
                    */
                    {
                        // ShelfData.Remove("all", processed);
                        ShelfData.l_Remove("adds", processed);
                        ShelfData.l_Remove("removes", processed);
                        ShelfData.l_Remove("changes", processed);
                    }

                    // 2020/4/2
                    ShelfData.l_RefreshCount();

                    if (actions.Count == 0)
                        return new SaveActionResult
                        {
                            Actions = actions,
                            //Operations = infos
                        };  // 没有必要处理
                    ShelfData.PushActions(actions);
                    return new SaveActionResult
                    {
                        Actions = actions,
                        //Operations = infos
                    };
                }
            }
            catch (Exception ex)
            {
                // 2020/6/10
                WpfClientInfo.WriteErrorLog($"SaveActions() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new SaveActionResult
                {
                    Value = -1,
                    ErrorInfo = $"SaveActions() 出现异常: {ex.Message}"
                };
            }
        }

        static int max_items = 5;  // 一个读者最多能同时借阅的册数
        static int max_period = 31; // 读者借阅期限天数

        // 构造 BorrowInfo 字符串
        // 用于在同步之前，为本地数据库记录临时模拟出 BorrowInfo。这样当长期断网的情况下，dp2ssl 能用它进行本地借书权限的判断(判断是否超期、超额)
        // parameters:
        //      delta_piis   尚未来得及保存到数据库的已借册的 PII 列表。注意里面的 PII 有可能是空字符串
        static string BuildBorrowInfo(string patron_pii,
            Entity entity,
            List<string> delta_piis)
        {
            BorrowInfo borrow_info = new BorrowInfo();

            string patron_type = GetPatronType(patron_pii);
            if (patron_type == null)
                goto DEFAULT;

            // TODO: 如何判断本册借阅时候是否已经超额？
            var piis = GetBorrowItems(patron_pii);
            piis.AddRange(delta_piis);

            // 当前册的图书类型
            var info_result = GetBookInfo(entity.PII);
            if (info_result.Value == -1)
            {
                // 如果得不到图书类型，建议按照默认的权限参数处理
                goto DEFAULT;
            }
            // 计算已经借阅的册中和当前册类型相同的册数
            int thisTypeCount = 0;
            foreach (string pii in piis)
            {
                if (GetBookType(pii) == info_result.BookType)
                    thisTypeCount++;
            }

            var max_result = GetTypeMax(info_result.LibraryCode,
    patron_type,
    info_result.BookType);

            bool overflow = false;
            // 图书类型限额超过了
            if (thisTypeCount + 1 > max_result.Max)
            {
                borrow_info.Overflows = new string[] { $"读者 '{ patron_pii}' 所借 '{ info_result.BookType }' 类图书数量将超过 馆代码 '{ info_result.LibraryCode}' 中 该读者类型 '{ patron_type }' 对该图书类型 '{ info_result.BookType }' 的最多 可借册数 值 '{max_result.Max}'" };
                // 一天以后还书
                SetReturning(1, "day");
                overflow = true;
            }
            else
            {
                var total_max_result = GetTotalMax(info_result.LibraryCode,
    patron_type);
                // 读者类型限额超过了
                if (piis.Count + 1 > total_max_result.Max)
                {
                    borrow_info.Overflows = new string[] { $"读者 '{ patron_pii}' 所借图书数量将超过 馆代码 '{ info_result.LibraryCode}' 中 该读者类型 '{ patron_type }' 对所有图书类型的最多 可借册数 值 '{total_max_result.Max}'" };
                    // 一天以后还书
                    SetReturning(1, "day");
                    overflow = true;
                }
            }

            if (overflow == false)
            {
                // 获得借期
                var period_result = GetPeriod(info_result.LibraryCode,
    patron_type,
    info_result.BookType);
                if (period_result.Value == -1)
                {
                    // 一个月以后还书
                    SetReturning(max_period, "day");
                    // TODO: 写入错误日志
                }
                else
                {
                    int nRet = DateTimeUtil.ParsePeriodUnit(
    period_result.ErrorCode,
    "day",
    out long lPeriodValue,
    out string strPeriodUnit,
    out string strError);
                    if (nRet == -1)
                    {
                        // 只好按照一个月以后还书来处理
                        SetReturning(max_period, "day");
                        // 写入错误日志
                        WpfClientInfo.WriteErrorLog($"解析时间段字符串 '{period_result.ErrorCode}' 时发生错误: {strError}");
                    }
                    else
                    {
                        string error = SetReturning((int)lPeriodValue, strPeriodUnit);
                        // 2020/6/10
                        if (error != null)
                        {
                            // 只好按照一个月以后还书来处理
                            SetReturning(max_period, "day");

                            WpfClientInfo.WriteErrorLog($"时间段字符串 '{period_result.ErrorCode}' 格式错误: {error}");
                        }
                    }
                }
            }

            goto END;

        DEFAULT:
            int item_count = GetBorrowItems(patron_pii).Count;
            if (item_count + delta_piis.Count >= max_items)
            {
                borrow_info.Overflows = new string[] { $"超过额度 {max_items} 册" };
                // 一天以后还书
                SetReturning(1, "day");
            }
            else
            {
                // 一个月以后还书
                SetReturning(max_period, "day");
                //borrow_info.Period = $"{max_period}day";
                //borrow_info.LatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now.AddDays(max_period));
            }

        END:
            if (entity != null)
                borrow_info.ItemBarcode = entity.PII;
            return JsonConvert.SerializeObject(borrow_info);

            // 设置 BorrowInfo 里面和还书时间有关的两个成员 Period 和 LatestReturnTime
            string SetReturning(int days, string unit)
            {
                // 检查 unit
                if (unit != "day" && unit != "hour")
                {
                    string error = $"出现了无法理解的时间单位字符串 '{unit}'";
                    WpfClientInfo.WriteErrorLog(error);
                    return error;
                }

                borrow_info.Period = $"{days}{unit}";
                DateTime returning = DateTime.Now.AddDays(days);
                if (unit == "hour")
                    returning = DateTime.Now.AddHours(days);
                // 正规化时间
                returning = RoundTime(unit, returning);
                borrow_info.LatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringEx(returning);
                return null;
            }
        }

        static string GetBookType(string pii)
        {
            var result = GetBookInfo(pii);
            if (result.Value == -1)
                return null;
            return result.BookType;
        }

        // 
        public class GetBookInfoResult : NormalResult
        {
            public string BookType { get; set; }
            public string LibraryCode { get; set; }
        }

        // 获得册信息
        static GetBookInfoResult GetBookInfo(string pii)
        {
            var result = LibraryChannelUtil.LocalGetEntityData(pii);
            if (result.Value == -1)
                return new GetBookInfoResult
                {
                    Value = -1,
                    ErrorInfo = result.ErrorInfo
                };
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(result.ItemXml);
            }
            catch (Exception ex)
            {
                return new GetBookInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"册记录 XML 格式不正确: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }

            string bookType = DomUtil.GetElementText(dom.DocumentElement, "bookType");
            string location = DomUtil.GetElementText(dom.DocumentElement, "location");
            location = StringUtil.GetPureLocationString(location);

            // 获得 location 中馆代码部分
            dp2StringUtil.ParseCalendarName(location,
    out string strLibraryCode,
    out string strPureName);
            return new GetBookInfoResult
            {
                Value = 1,
                BookType = bookType,
                LibraryCode = strLibraryCode
            };
        }

        static string GetPatronType(string patron_pii)
        {
            var result = LibraryChannelUtil.GetReaderInfoFromLocal(patron_pii, false);
            if (result.Value == -1)
                return null;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(result.ReaderXml);
            }
            catch
            {
                return null;
            }

            return DomUtil.GetElementText(dom.DocumentElement, "readerType");
        }

        // 包装后的版本
        // 获得流通参数
        // parameters:
        //      strLibraryCode  图书馆代码, 如果为空,表示使用<library>元素以外的片段
        // return:
        //      reader和book类型均匹配 算4分
        //      只有reader类型匹配，算3分
        //      只有book类型匹配，算2分
        //      reader和book类型都不匹配，算1分
        static int GetLoanParam(
            string strLibraryCode,
            string strReaderType,
            string strBookType,
            string strParamName,
            out string strParamValue,
            out MatchResult matchresult,
#if DEBUG_LOAN_PARAM
            out string strDebug,
#endif
            out string strError)
        {
            strParamValue = "";
            strError = "";
            matchresult = MatchResult.None;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(_rightTableXml);
            }
            catch (Exception ex)
            {
                strError = $"读者借阅权限 XML 装入 DOM 时出错: {ex.Message}";
                return -1;
            }


            XmlNode root = dom.DocumentElement;

            return LoanParam.GetLoanParam(
                root,    // this.LibraryCfgDom,
                strLibraryCode,
                strReaderType,
                strBookType,
                strParamName,
                out strParamValue,
                out matchresult,
#if DEBUG_LOAN_PARAM
                out strDebug,
#endif
                out strError);
        }

        public class GetTypeMaxResult : NormalResult
        {
            public string PatronType { get; set; }
            public string BookType { get; set; }
            // 指定图书类型(对于指定读者类型)的允许借阅最大册数
            public int Max { get; set; }
        }

        // 获得特定图书类型的最大可借册数
        static GetTypeMaxResult GetTypeMax(string strLibraryCode,
            string strReaderType,
            string strBookType)
        {
            // 得到该类图书的册数限制配置
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                strBookType,
                "可借册数",
                out string strParamValue,
                out MatchResult matchresult,
                out string strError);
            if (nRet == -1 || nRet < 4)
            {
                strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 图书类型 '" + strBookType + "' 尚未定义 可借册数 参数";
                return new GetTypeMaxResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // 看看是此类否超过册数限制
            int nThisTypeMax = 0;
            try
            {
                nThisTypeMax = Convert.ToInt32(strParamValue);
            }
            catch
            {
                strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 图书类型 '" + strBookType + "' 的 可借册数 参数值 '" + strParamValue + "' 格式有问题";
                return new GetTypeMaxResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            return new GetTypeMaxResult
            {
                Value = 0,
                Max = nThisTypeMax,
                PatronType = strReaderType,
                BookType = strBookType
            };
        }

        // 获得特定读者类型的最大可借册数
        public static GetTypeMaxResult GetTotalMax(string strLibraryCode,
            string strReaderType)
        {

            // 得到该读者类型针对所有类型图书的总册数限制配置
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "可借总册数",
                out string strParamValue,
                out MatchResult matchresult,
                out string strError);
            if (nRet == -1)
            {
                strError = "在获取馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 可借总册数 参数过程中出错: " + strError + "。";
                return new GetTypeMaxResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            if (nRet < 3)
            {
                strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 尚未定义 可借总册数 参数";
                return new GetTypeMaxResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // 然后看看总册数是否已经超过限制
            int nMax = 0;
            try
            {
                nMax = Convert.ToInt32(strParamValue);
            }
            catch
            {
                strError = "馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 的 可借总册数 参数值 '" + strParamValue + "' 格式有问题";
                return new GetTypeMaxResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            return new GetTypeMaxResult
            {
                Value = 0,
                Max = nMax,
                PatronType = strReaderType,
            };
        }

        // 获得借期
        static NormalResult GetPeriod(string strLibraryCode,
            string strReaderType,
            string strBookType)
        {
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = GetLoanParam(
            //null,
            strLibraryCode,
            strReaderType,
            strBookType,
            "借期",
            out string strBorrowPeriodList,
            out MatchResult matchresult,
            out string strError);
            if (nRet == -1)
            {
                strError = "借阅失败。获得 馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数时发生错误: " + strError;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            if (nRet < 4)  // nRet == 0
            {
                strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数无法获得: " + strError;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // 按照逗号分列值，需要根据序号取出某个参数
            string[] aPeriod = strBorrowPeriodList.Split(new char[] { ',' });

            if (aPeriod.Length == 0)
            {
                strError = "借阅失败。馆代码 '" + strLibraryCode + "' 中 读者类型 '" + strReaderType + "' 针对图书类型 '" + strBookType + "' 的 借期 参数 '" + strBorrowPeriodList + "'格式错误";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            return new NormalResult
            {
                Value = 0,
                ErrorCode = aPeriod[0]
            };
        }

        // 注意 time 中的时间应该是本地时间
        static DateTime RoundTime(string strUnit,
        DateTime time)
        {
            if (strUnit == "day" || string.IsNullOrEmpty(strUnit) == true)
            {
                return new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                return new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                throw new ArgumentException("未知的时间单位 '" + strUnit + "'");
            }
        }

#if NO
        // 获得一个读者当前的在借册册数
        static int GetBorrowItemCount(string pii)
        {
            using (var context = new RequestContext())
            {
                // 该读者的在借册册数
                return context.Requests
                    .Where(o => o.OperatorID == pii && o.Action == "borrow" && o.LinkID == null)
                    .OrderBy(o => o.ID).Count();
            }
        }
#endif

        // 获得一个读者当前的在借册的 PII 列表
        static List<string> GetBorrowItems(string pii)
        {
            using (var context = new RequestContext())
            {
                // 该读者的在借册册数
                return context.Requests
                    .Where(o => o.OperatorID == pii && o.Action == "borrow" && o.LinkID == null
                    && o.State != "dontsync")   // 2020/6/17 注：dontsync 表示同步时候实际上另外已经有前端对本册进行了操作(若能操作成功可以推测是还书操作)，所以这一册实际上已经换了，不要计入在借册列表中
                    .Select(o => o.PII).ToList();
                // .OrderBy(o => o.ID).Count();
            }
        }

        // 从 "阅览室:1-1" 中析出 "阅览室" 部分
        static string GetLocationPart(string shelfNo)
        {
            return StringUtil.ParseTwoPart(shelfNo, ":")[0];
        }

        public static string GetLibraryCode(string shelfNo)
        {
            string location = GetLocationPart(shelfNo);
            ParseLocation(location, out string libraryCode, out string room);
            return libraryCode;
        }

        static void ParseLocation(string strName,
        out string strLibraryCode,
        out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }

        // 将 actions 保存起来
        public static void PushActions(List<ActionInfo> actions)
        {
            lock (_syncRoot_actions)
            {
                _actions.AddRange(actions);
            }
        }


        // 限制询问对话框，同一时刻只能打开一个对话框
        static AsyncSemaphore _askLimit = new AsyncSemaphore(1);


        public delegate void Delegate_removeAction(ActionInfo action);

        // 询问典藏移交的一些条件参数
        // parameters:
        //      actions     在本函数处理过程中此集合内的对象可能被修改，集合元素可能被移除
        // return:
        //      false   没有发生询问
        //      true    发生了询问
        public static async Task<bool> AskLocationTransferAsync(List<ActionInfo> actions,
            Delegate_removeAction func_removeAction)
        {
            bool bAsked = false;
            // 1) 搜集信息。观察是否有需要询问和兑现的参数
            {
                List<ActionInfo> transferins = new List<ActionInfo>();
                foreach (var action in actions)
                {
                    if (action.Action == "transfer"
                        && action.TransferDirection == "in"
                        && string.IsNullOrEmpty(action.Location) == false)
                    {
                        transferins.Add(action);
                    }
                }

                // 询问放入的图书是否需要移交到当前书柜馆藏地
                if (transferins.Count > 0)
                {
                    using (var releaser = await _askLimit.EnterAsync())
                    {
                        bAsked = true;
                        App.CurrentApp.Speak("上架");
                        string batchNo = transferins[0].Operator.GetWorkerAccountName() + "_" + DateTime.Now.ToShortDateString();
                        /*
                        EntityCollection collection = new EntityCollection();
                        foreach (var action in transferins)
                        {
                            Entity dup = action.Entity.Clone();
                            dup.Container = collection;
                            dup.Waiting = false;
                            collection.Add(dup);
                        }
                        */
                        EntityCollection collection = BuildEntityCollection(transferins);
                        string selection = "";
                        App.Invoke(new Action(() =>
                        {
                            App.PauseBarcodeScan();
                            try
                            {
                                var door_names = StringUtil.MakePathList(GetDoorName(transferins), ",");
                                AskTransferWindow dialog = new AskTransferWindow();
                                dialog.TitleText = $"上架({door_names})";
                                dialog.TransferButtonText = "上架+调入";
                                dialog.NotButtonText = "普通上架";
                                dialog.SetBooks(collection);
                                dialog.Text = $"如何处理以上放入 {door_names} 的 {collection.Count} 册图书？";
                                dialog.Owner = App.CurrentApp.MainWindow;
                                dialog.BatchNo = batchNo;
                                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                App.SetSize(dialog, "tall");

                                //dialog.Width = Math.Min(700, App.CurrentApp.MainWindow.ActualWidth);
                                //dialog.Height = Math.Min(900, App.CurrentApp.MainWindow.ActualHeight);
                                dialog.ShowDialog();
                                selection = dialog.Selection;
                                batchNo = dialog.BatchNo;
                            }
                            finally
                            {
                                App.ContinueBarcodeScan();
                            }
                        }));

                        // 把 transfer 动作里的 Location 成员清除
                        if (selection == "not")
                        {
                            foreach (var action in transferins)
                            {
                                action.Location = "";

                                // 把不需要操作的 ActionInfo 删除
                                if (string.IsNullOrEmpty(action.Location)
                                    && string.IsNullOrEmpty(action.CurrentShelfNo))
                                {
                                    actions.Remove(action);
                                    func_removeAction?.Invoke(action);
                                }
                            }
                        }
                        else
                        {
                            foreach (var action in transferins)
                            {
                                action.BatchNo = batchNo;
                            }
                        }
                    }
                }
            }

            // 2) 搜集信息。观察是否有移交出
            {
                List<ActionInfo> transferouts = new List<ActionInfo>();
                foreach (var action in actions)
                {
                    if (action.Action == "transfer"
                        && action.TransferDirection == "out"
                        && string.IsNullOrEmpty(action.Location) == false)
                    {
                        transferouts.Add(action);
                    }
                }

                // 询问放入的图书是否需要移交到当前书柜馆藏地
                if (transferouts.Count > 0)
                {
                    using (var releaser = await _askLimit.EnterAsync())
                    {
                        bAsked = true;
                        App.CurrentApp.Speak("下架");

                        string batchNo = transferouts[0].Operator.GetWorkerAccountName() + "_" + DateTime.Now.ToShortDateString();

                        // TODO: 这个列表是否在程序初始化的时候得到?
                        // var result = LibraryChannelUtil.GetLocationList();
                        /*
                        EntityCollection collection = new EntityCollection();
                        foreach (var action in transferouts)
                        {
                            Entity dup = action.Entity.Clone();
                            dup.Container = collection;
                            dup.Waiting = false;
                            collection.Add(dup);
                        }
                        */
                        EntityCollection collection = BuildEntityCollection(transferouts);
                        string selection = "";
                        string target = "";
                        App.Invoke(new Action(() =>
                        {
                            App.PauseBarcodeScan();
                            try
                            {
                                var door_names = StringUtil.MakePathList(GetDoorName(transferouts), ",");
                                AskTransferWindow dialog = new AskTransferWindow();
                                dialog.TitleText = $"下架({door_names})";
                                dialog.TransferButtonText = "下架+调出";
                                dialog.NotButtonText = "普通下架";
                                dialog.Mode = "out";
                                dialog.SetBooks(collection);
                                dialog.Text = $"如何处理以上从 {door_names} 取走的 {collection.Count} 册图书？";
                                dialog.target.ItemsSource = _locationList;  // result.List;
                                dialog.BatchNo = batchNo;
                                dialog.Owner = App.CurrentApp.MainWindow;
                                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                App.SetSize(dialog, "tall");

                                //dialog.Width = Math.Min(700, App.CurrentApp.MainWindow.ActualWidth);
                                //.Height = Math.Min(900, App.CurrentApp.MainWindow.ActualHeight);
                                dialog.ShowDialog();
                                selection = dialog.Selection;
                                target = dialog.Target;
                                batchNo = dialog.BatchNo;
                            }
                            finally
                            {
                                App.ContinueBarcodeScan();
                            }
                        }));

                        // 把 transfer 动作里的 Location 成员清除
                        if (selection == "not")
                        {
                            foreach (var action in transferouts)
                            {
                                // 修改 Action
                                action.Location = "";
                                // 注: action.CurrentShelfNo 也为空
                                // 注: action.TransferDirection 为 "out"

                                /*
                                // 把不需要操作的 ActionInfo 删除
                                if (string.IsNullOrEmpty(action.Location)
                                    && string.IsNullOrEmpty(action.CurrentShelfNo))
                                {
                                    actions.Remove(action);
                                    func_removeAction?.Invoke(action);
                                }
                                */

                            }
                        }
                        else
                        {
                            foreach (var action in transferouts)
                            {
                                action.Location = target;
                                action.BatchNo = batchNo;
                            }
                        }
                    }
                }
            }

            return bAsked;
        }

        // 概括门名字
        public static List<string> GetDoorName(List<ActionInfo> actions_param)
        {
            List<DoorItem> results = new List<DoorItem>();
            foreach (var action in actions_param)
            {
                var doors = DoorItem.FindDoors(ShelfData.Doors, action.Entity.ReaderName, action.Entity.Antenna);
                Add(results, doors);
            }

            List<string> names = new List<string>();
            foreach (var door in results)
            {
                names.Add(door.Name);
            }

            return names;

            void Add(List<DoorItem> target, List<DoorItem> doors)
            {
                foreach (var door in doors)
                {
                    if (target.IndexOf(door) == -1)
                        target.Add(door);
                }
            }
        }

        static EntityCollection BuildEntityCollection(List<ActionInfo> actions)
        {
            EntityCollection collection = new EntityCollection();
            foreach (var action in actions)
            {
                Entity dup = action.Entity.Clone();
                dup.Container = collection;
                dup.Waiting = false;
                // testing
                // dup.Title = null;
                dup.FillFinished = false;
                collection.Add(dup);
            }

            return collection;
        }

        // 用于保护 _all _adds _removes _changes 的锁对象
        static object _syncRoot_all = new object();

        static List<Entity> _all = new List<Entity>();  // 累积的全部图书
        static List<Entity> _adds = new List<Entity>(); // 临时区 放入的图书
        static List<Entity> _removes = new List<Entity>();  // 临时区 取走的图书
        static List<Entity> _changes = new List<Entity>();  // 临时区 天线编号、门位置发生过变化的图书

        public static IReadOnlyCollection<Entity> l_All
        {
            get
            {
                lock (_syncRoot_all)
                {
                    List<Entity> results = new List<Entity>(_all);
                    return results;
                    // return _all.AsReadOnly();
                }
            }
        }

        public static IReadOnlyCollection<Entity> l_Adds
        {
            get
            {
                lock (_syncRoot_all)
                {
                    return new List<Entity>(_adds);
                }
            }
        }

        public static IReadOnlyCollection<Entity> l_Removes
        {
            get
            {
                lock (_syncRoot_all)
                {
                    return new List<Entity>(_removes);
                }
            }
        }

        public static IReadOnlyCollection<Entity> l_Changes
        {
            get
            {
                lock (_syncRoot_all)
                {
                    return new List<Entity>(_changes);
                }
            }
        }

        /*
        static Operator _operator = null;   // 当前控制临时区的读者身份

        public static Operator Operator
        {
            get
            {
                return _operator;
            }
        }
        */

        // 初始化门控件定义。包括初始化 ShelfCfgDom
        // 异常：
        //      可能会抛出 Exception 异常
        public static void InitialDoors()
        {
            {
                string cfg_filename = ShelfFilePath;
                XmlDocument cfg_dom = new XmlDocument();
                cfg_dom.Load(cfg_filename);

                _shelfCfgDom = cfg_dom;
            }

            // 2019/12/22
            if (_doors != null)
                _doors.Clear();
            _doors = DoorItem.BuildItems(_shelfCfgDom, out List<string> errors);

            if (errors.Count > 0)
                throw new Exception(StringUtil.MakePathList(errors, "; "));
        }

        static bool _firstInitial = false;

        public static bool FirstInitialized
        {
            get
            {
                return _firstInitial;
            }
            set
            {
                _firstInitial = value;
            }
        }

        public static async Task<NormalResult> WaitLockReadyAsync(
            Delegate_displayText func_display,
            Delegate_cancelled func_cancelled)
        {
            WpfClientInfo.WriteInfoLog("等待锁控就绪");
            func_display("等待锁控就绪 ...");
            bool ret = await Task.Run(() =>
            {
                while (true)
                {
                    if (OpeningDoorCount != -1)
                        return true;
                    if (func_cancelled() == true)
                        return false;
                    Thread.Sleep(100);
                }
            });

            if (ret == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "用户中断",
                    ErrorCode = "cancelled"
                };

            return new NormalResult();
        }

        public delegate void Delegate_displayText(string text);
        public delegate bool Delegate_cancelled();

        public class InitialShelfResult : NormalResult
        {
            public List<string> Warnings { get; set; }
            public List<Entity> All { get; set; }
        }

        // 首次初始化智能书柜所需的标签相关数据结构
        // 初始化开始前，要先把 RfidManager.ReaderNameList 设置为 "*"
        // 初始化完成前，先不要允许(开关门变化导致)修改 RfidManager.ReaderNameList
        public static async Task<InitialShelfResult> newVersion_InitialShelfEntitiesAsync(
            List<DoorItem> doors_param,
            bool silently,
            Delegate_displayText func_display,
            Delegate_cancelled func_cancelled)
        {
            // TODO: 出现“正在初始化”的对话框。另外需要注意如果 DataReady 信号永远来不了怎么办
            WpfClientInfo.WriteInfoLog("开始初始化图书信息");
            func_display("开始初始化图书信息 ...");

            // 一个一个门地填充图书信息
            int i = 0;
            foreach (var door in doors_param)
            {
                if (func_cancelled() == true)
                    return new InitialShelfResult();

                // 获得和一个门相关的 readernamelist
                var list = GetReaderNameList(new List<DoorItem> { door }, null);
                string style = $"dont_delay";   // 确保 inventory 并立即返回

                func_display($"{i + 1}/{Doors.Count} 门 {door.Name} ({list}) ...");

                using (var releaser = await _inventoryLimit.EnterAsync().ConfigureAwait(false))
                {
                    var result = RfidManager.CallListTags(list, style);
                    try
                    {
                        await RfidManager.TriggerListTagsEvent(list, result, true);
                    }
                    catch (TagInfoException ex)
                    {
                        // 2020/4/9
                        string error = $"出现无法解析的标签 UID:{ex.TagInfo.UID}";
                        WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 异常: {error} 门:{door.Name}");
                        return new InitialShelfResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    }
                }

                i++;
            }

            if (func_cancelled() == true)
                return new InitialShelfResult();

            WpfClientInfo.WriteInfoLog("开始填充图书队列");
            func_display("正在填充图书队列 ...");

            List<string> warnings = new List<string>();

            List<Entity> all = new List<Entity>();
            lock (_syncRoot_all)
            {
                // _all.Clear();

#if OLD_TAGCHANGED
                var books = TagList.Books;
#else
                var books = NewTagList.Tags;
                // TODO: 注意里面也包含了读者卡，需要过滤一下
#endif

                WpfClientInfo.WriteErrorLog($"books count={books.Count}, ReaderNameList={RfidManager.ReaderNameList}(注：此时门应该都是关闭的，图书读卡器应该是停止盘点状态)");
                foreach (var tag in books)
                {
                    if (func_cancelled() == true)
                        return new InitialShelfResult();

                    WpfClientInfo.WriteErrorLog($" tag={tag.ToString()}");

                    // 跳过读者读卡器上的标签
                    if (tag.OneTag.ReaderName == _patronReaderName)
                        continue;

                    // 2019/12/17
                    // 判断一下 tag 是否属于已经定义的门范围
                    var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                    if (doors.Count == 0)
                    {
                        WpfClientInfo.WriteInfoLog($"tag (UID={tag.OneTag?.UID},Antenna={tag.OneTag.AntennaID}) 不属于任何已经定义的门，没有被加入 _all 集合。\r\ntag 详情：{tag.ToString()}");
                        continue;
                    }

                    // 不属于本函数当前关注的门范围
                    if (Cross(doors_param, doors) == false)
                        continue;


                    try
                    {
                        // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
                        // Exception:
                        //      可能会抛出异常 ArgumentException
                        var entity = NewEntity(tag, false);

                        func_display($"正在填充图书队列 ({GetPiiString(entity)})...");

                        all.Add(entity);

                        if (silently == false
                            && string.IsNullOrEmpty(entity.Error) == false)
                        {
                            warnings.Add($"UID 为 '{tag.OneTag?.UID}' 的标签解析出错: {entity.Error}");
                            WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 遇到 tag (UID={tag.OneTag?.UID}) 解析出错: {entity.Error}\r\ntag 详情：{tag.ToString()}");
                        }
                    }
                    catch (TagDataException ex)
                    {
                        warnings.Add($"UID 为 '{tag.OneTag?.UID}' 的标签出现数据格式错误: {ex.Message}");
                        WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 遇到 tag (UID={tag.OneTag?.UID}) 数据格式出错：{ex.Message}\r\ntag 详情：{tag.ToString()}");
                    }

                    /*
                    // 对读者卡进行判断(注：这些都是在书柜门以内的读卡器上的读者卡)
                    // 属于本函数当前关注的门范围
                    if (tag.Type == "patron")
                    {
                        warnings.Add($"出现读者证标签。UID={tag.OneTag?.UID} Protocol={tag.OneTag?.Protocol}");
                        WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 出现读者证标签。门={doors[0].Name},UID={tag.OneTag?.UID} Protocol={tag.OneTag?.Protocol}\r\ntag 详情：{tag.ToString()}");
                    }
                    */
                }

#if OLD_TAGCHANGED

                // 2020/4/9
                // 检查放在柜门内的 ISO15693 读者卡
                var patrons = TagList.Patrons;
                foreach (var tag in patrons)
                {
                    if (func_cancelled() == true)
                        return new InitialShelfResult();

                    WpfClientInfo.WriteErrorLog($" (读者卡)tag={tag.ToString()}");

                    // 判断一下 tag 是否属于已经定义的门范围
                    var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                    if (doors.Count == 0)
                    {
                        // 这是正常情况：读者卡所放的读卡器不是柜门读卡器
                        continue;
                    }

                    // 属于本函数当前关注的门范围
                    if (Cross(doors_param, doors) == true)
                    {
                        warnings.Add($"出现读者证标签。UID={tag.OneTag?.UID} Protocol={tag.OneTag?.Protocol}");
                        WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 出现读者证标签。门={doors[0].Name},UID={tag.OneTag?.UID} Protocol={tag.OneTag?.Protocol}\r\ntag 详情：{tag.ToString()}");
                    }
                }
#endif
            }

            // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
            // TODO: 只刷新指定门的数字即可
            l_RefreshCount();

            // TryReturn(progress, _all);
            // _firstInitial = true;   // 第一次初始化已经完成

            /* 这一段可以在函数返回后做
            func_display("获取图书册记录信息 ...");

            var task = Task.Run(async () =>
            {
                CancellationToken token = CancelToken;
                await FillBookFields(All, token);
                await FillBookFields(Adds, token);
                await FillBookFields(Removes, token);
            });
            */

            return new InitialShelfResult
            {
                Warnings = warnings,
                All = all
            };

            // 观察两个集合是否有交集
            bool Cross(List<DoorItem> doors1, List<DoorItem> doors2)
            {
                foreach (var door1 in doors1)
                {
                    if (doors2.IndexOf(door1) != -1)
                        return true;
                }

                return false;
            }
        }

        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        static void SetTagType(TagAndData data, out string pii)
        {
            pii = null;

            if (data.OneTag.Protocol == InventoryInfo.ISO14443A)
            {
                data.Type = "patron";
                return;
            }

            if (data.OneTag.TagInfo == null)
            {
                data.Type = ""; // 表示类型不确定
                return;
            }

            if (string.IsNullOrEmpty(data.Type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                LogicChip chip = LogicChip.From(data.OneTag.TagInfo.Bytes,
        (int)data.OneTag.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                pii = chip.FindElement(ElementOID.PII)?.Text;

                var typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                    data.Type = "patron";
                else
                    data.Type = "book";
            }
        }

#if NO

        // 首次初始化智能书柜所需的标签相关数据结构
        // 初始化开始前，要先把 RfidManager.ReaderNameList 设置为 "*"
        // 初始化完成前，先不要允许(开关门变化导致)修改 RfidManager.ReaderNameList
        public static async Task<InitialShelfResult> InitialShelfEntities(
            Delegate_displayText func_display,
            Delegate_cancelled func_cancelled)
        {
            // TODO: 出现“正在初始化”的对话框。另外需要注意如果 DataReady 信号永远来不了怎么办
            WpfClientInfo.WriteInfoLog("开始初始化图书信息");

            func_display("等待读卡器就绪 ...");
            bool ret = await Task.Run(() =>
            {
                while (true)
                {
                    if (RfidManager.TagsReady == true)
                        return true;
                    if (func_cancelled() == true)
                        return false;
                    Thread.Sleep(100);
                }
            });

            if (ret == false)
                return new InitialShelfResult();

            // 使用全部读卡器、全部天线进行初始化。即便门是全部关闭的(注：一般情况下，当门关闭的时候图书读卡器是暂停盘点的)
            WpfClientInfo.WriteInfoLog("开始启用全部读卡器和天线");

            func_display("启用全部读卡器和天线 ...");
            ret = await Task.Run(() =>
            {
                // 使用全部读卡器，全部天线
                RfidManager.Pause = true;

                // TODO: 这里并不是马上能停下来呀？是否要等待停下来
                // 否则探测到 TagsReady == true 可能是上一轮延迟到来的结果
                // 可以考虑给 TagsReady 变成一个字符串值，内容是每一轮请求的 session_id，这样就可以确认是哪一次的返回了

                //RfidManager.Pause2 = true;  // 暂停 Base2 线程
                RfidManager.ReaderNameList = _allDoorReaderName;
                RfidManager.TagsReady = false;
                RfidManager.Pause = false;
                // 注意此时 Base 线程依然是暂停状态
                RfidManager.ClearCache();   // 迫使立即重新请求 Inventory
                while (true)
                {
                    if (RfidManager.TagsReady == true)
                        return true;
                    if (func_cancelled() == true)
                        return false;
                    Thread.Sleep(100);
                }
            });

            if (ret == false)
            {
                WpfClientInfo.WriteErrorLog($"waiting DataReady cancelled");
                return new InitialShelfResult();
            }

            WpfClientInfo.WriteInfoLog("开始填充图书队列");
            func_display("正在填充图书队列 ...");

            List<string> warnings = new List<string>();

            lock (_syncRoot_all)
            {
                _all.Clear();
                var books = TagList.Books;
                WpfClientInfo.WriteErrorLog($"books count={books.Count}, ReaderNameList={RfidManager.ReaderNameList}");
                foreach (var tag in books)
                {
                    WpfClientInfo.WriteErrorLog($" tag={tag.ToString()}");

                    try
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        _all.Add(NewEntity(tag));
                    }
                    catch (TagDataException ex)
                    {
                        warnings.Add($"UID 为 '{tag.OneTag?.UID}' 的标签出现数据格式错误: {ex.Message}");
                        WpfClientInfo.WriteErrorLog($"InitialShelfEntities() 遇到 tag (UID={tag.OneTag?.UID}) 数据格式出错：{ex.Message}\r\ntag 详情：{tag.ToString()}");
                    }
                }
            }


            {
                WpfClientInfo.WriteInfoLog("等待锁控就绪");
                func_display("等待锁控就绪 ...");
                // 恢复 Base2 线程运行
                // RfidManager.Pause2 = false;
                ret = await Task.Run(() =>
                {
                    while (true)
                    {
                        if (OpeningDoorCount != -1)
                            return true;
                        if (func_cancelled() == true)
                            return false;
                        Thread.Sleep(100);
                    }
                });

                if (ret == false)
                    return new InitialShelfResult();
            }


            // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
            RefreshCount();

            // TryReturn(progress, _all);
            _firstInitial = true;   // 第一次初始化已经完成

            var task = Task.Run(async () =>
            {
                CancellationToken token = CancelToken;
                await FillBookFields(All, token);
                await FillBookFields(Adds, token);
                await FillBookFields(Removes, token);
            });

            return new InitialShelfResult { Warnings = warnings };
        }


#endif

        // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
        // Exception:
        //      可能会抛出异常 ArgumentException
        static Entity NewEntity(TagAndData tag, bool throw_exception = true)
        {
            var result = new Entity
            {
                UID = tag.OneTag.UID,
                ReaderName = tag.OneTag.ReaderName,
                Antenna = tag.OneTag.AntennaID.ToString(),
                TagInfo = tag.OneTag.TagInfo,
            };

            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            try
            {
                SetTagType(tag, out string pii);
                result.PII = pii;
            }
            catch (Exception ex)
            {
                if (throw_exception == false)
                {
                    result.AppendError($"RFID 标签格式错误: {ex.Message}",
                        "red",
                        "parseTagError");
                }
                else
                    throw ex;
            }

#if NO
            // Exception:
            //      可能会抛出异常 ArgumentException 
            EntityCollection.SetPII(result, pii);
#endif

            // 2020/4/9
            if (tag.Type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                result.PII = "(读者卡)" + result.PII;
                result.AppendError("读者卡误放入书柜", "red", "patronCard");
            }
            return result;
        }

        // 检查一本图书是否处在普通(非 free) 类型的门内
        public static bool BelongToNormal(Entity entity)
        {
            var doors = DoorItem.FindDoors(_doors, entity.ReaderName, entity.Antenna);
            int count = 0;
            foreach (DoorItem door in doors)
            {
                if (door.Type == "free")
                    return false;
                count++;
            }
            return count > 0;
        }

        public static string GetShelfNo(Entity entity)
        {
            var doors = DoorItem.FindDoors(_doors, entity.ReaderName, entity.Antenna);
            if (doors.Count == 0)
                return "";
            return doors[0].ShelfNo;
        }

        // 刷新门内图书数字显示
        public static void l_RefreshCount()
        {
            List<Entity> errors = null;
            List<Entity> all = null;
            List<Entity> adds = null;
            List<Entity> removes = null;

            lock (_syncRoot_all)
            {
                all = new List<Entity>(_all);
                adds = new List<Entity>(_adds);
                removes = new List<Entity>(_removes);
                errors = GetErrors(_all, _adds, _removes);
            }
            DoorItem.DisplayCount(all, adds, removes, errors, Doors);
        }

        // 注意，没有加锁
        public static List<Entity> GetErrors(List<Entity> all,
            List<Entity> adds,
            List<Entity> removes)
        {
            List<Entity> errors = new List<Entity>();
            List<Entity> list = new List<Entity>(all);
            list.AddRange(adds);
            list.AddRange(removes);
            foreach (var entity in list)
            {
                if (entity.Error != null && entity.ErrorColor == "red")
                {
                    if (errors.IndexOf(entity) == -1)
                        internalAdd(errors, entity);
                }
            }

            return errors;
        }

        public static List<string> GetLockCommands()
        {
            /*
            string cfg_filename = App.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            */
            return GetLockCommands(ShelfCfgDom);
        }

        // 构造锁命令字符串数组
        public static List<string> GetLockCommands(XmlDocument cfg_dom)
        {
            // lockName --> bool
            Hashtable table = new Hashtable();
            XmlNodeList doors = cfg_dom.DocumentElement.SelectNodes("//door");
            foreach (XmlElement door in doors)
            {
                string lockDef = door.GetAttribute("lock");
                if (string.IsNullOrEmpty(lockDef))
                    continue;

                string lockName = DoorItem.NormalizeLockName(lockDef);
                // DoorItem.ParseReaderString(lockDef, out string lockName, out int lockIndex);
                if (string.IsNullOrEmpty(lockName))
                    continue;

                if (table.ContainsKey(lockName) == false)
                {
                    table[lockName] = true;
                }
                else
                    continue;
            }

            /*
            List<LockCommand> results = new List<LockCommand>();
            foreach (string key in table.Keys)
            {
                StringBuilder text = new StringBuilder();
                int i = 0;
                foreach (var v in table[key] as List<int>)
                {
                    if (i > 0)
                        text.Append(",");
                    text.Append(v);
                    i++;
                }
                results.Add(new LockCommand
                {
                    LockName = key,
                    Indices = text.ToString()
                });
            }
            */
            List<string> lock_names = new List<string>();
            foreach (string s in table.Keys)
            {
                lock_names.Add(s);
            }
            lock_names.Sort();

            return lock_names;
        }

        public delegate bool Delegate_match(Entity entity);

        public static List<Entity> Find(IReadOnlyCollection<Entity> entities,
            Delegate_match func_match)
        {
            List<Entity> results = new List<Entity>();
            /*
            entities.ForEach((o) =>
            {
                if (o.UID == uid)
                    results.Add(o);
            });
            */
            foreach (var o in entities)
            {
                if (func_match(o) == true)
                    results.Add(o);
            }
            return results;
        }

#if NO
        public static List<Entity> Find(IReadOnlyCollection<Entity> entities,
            string uid)
        {
            List<Entity> results = new List<Entity>();
            /*
            entities.ForEach((o) =>
            {
                if (o.UID == uid)
                    results.Add(o);
            });
            */
            foreach (var o in entities)
            {
                if (o.UID == uid)
                    results.Add(o);
            }
            return results;
        }
#endif

        static List<Entity> l_Find(string name, TagAndData tag)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                List<Entity> results = new List<Entity>();
                entities.ForEach((o) =>
                {
                    if (o.UID == tag.OneTag.UID)
                        results.Add(o);
                });
                return results;
            }
        }

        // 注意：这是不加锁的版本
        static List<Entity> Find(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            return results;
        }

        // return:
        //      false   实际上没有添加(对象以前已经在集合中存在)
        //      true    发生了添加
        internal static bool Add(string name, Entity entity)
        {
            var list = new List<Entity>();
            list.Add(entity);
            if (l_Add(name, list) > 0)
                return true;
            return false;
        }

        static void l_ReplaceOrAdd(List<Entity> entities, TagAndData tag)
        {
            lock (_syncRoot_all)
            {
                var found = entities.FindAll((o) => o.UID == tag.OneTag.UID);
                if (found.Count > 0)
                {
                    foreach (var o in found)
                    {
                        entities.Remove(o);
                    }
                }
                entities.Add(NewEntity(tag, false));
            }
        }

        // 2020/4/19
        // 替换集合中 UID 相同的 Entity 对象。如果没有找到则添加 entity 进入集合
        static void l_ReplaceOrAdd(List<Entity> entities, Entity entity)
        {
            lock (_syncRoot_all)
            {
                var found = entities.FindAll((o) => o.UID == entity.UID);
                if (found.Count > 0)
                {
                    foreach (var o in found)
                    {
                        entities.Remove(o);
                    }
                }
                entities.Add(entity);
            }
        }

        // return:
        //      返回实际添加的个数
        internal static int l_Add(string name,
            IReadOnlyCollection<Entity> adds)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                int count = 0;
                foreach (var entity in adds)
                {
                    Debug.Assert(entity != null, "");
                    Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

                    List<Entity> results = new List<Entity>();
                    entities.ForEach((o) =>
                    {
                        if (o.UID == entity.UID)
                            results.Add(o);
                    });
                    if (results.Count > 0)
                        continue;
                    entities.Add(entity);
                    count++;
                }

                return count;
            }
        }


        /*
        internal static bool Add(string name, Entity entity)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                Debug.Assert(entity != null, "");
                Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

                List<Entity> results = new List<Entity>();
                entities.ForEach((o) =>
                {
                    if (o.UID == entity.UID)
                        results.Add(o);
                });
                if (results.Count > 0)
                    return false;
                entities.Add(entity);
                return true;
            }
        }
        */

        // 注意，没有加锁
        internal static bool internalAdd(List<Entity> entities, Entity entity)
        {
            Debug.Assert(entity != null, "");
            Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
                return false;
            entities.Add(entity);
            return true;
        }

        internal static void Remove(string name, Entity entity)
        {
            var list = new List<Entity>();
            list.Add(entity);
            l_Remove(name, list);
        }

        internal static void l_Remove(string name,
            IReadOnlyCollection<Entity> removes)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                int count = 0;
                foreach (var entity in removes)
                {
                    Debug.Assert(entity != null, "");
                    Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

                    List<Entity> results = new List<Entity>();
                    entities.ForEach((o) =>
                    {
                        if (o.UID == entity.UID)
                            results.Add(o);
                    });
                    if (results.Count > 0)
                    {
                        foreach (var o in results)
                        {
                            entities.Remove(o);
                            count++;
                        }
                    }
                }
            }
        }

        /*
        internal static bool Remove(string name, Entity entity)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                Debug.Assert(entity != null, "");
                Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

                List<Entity> results = new List<Entity>();
                entities.ForEach((o) =>
                {
                    if (o.UID == entity.UID)
                        results.Add(o);
                });
                if (results.Count > 0)
                {
                    foreach (var o in results)
                    {
                        entities.Remove(o);
                    }
                    return true;
                }
                return false;
            }
        }

        */

        /*
        internal static bool Remove(List<Entity> entities, Entity entity)
        {
            lock (_syncRoot_all)
            {
                Debug.Assert(entity != null, "");
                Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

                List<Entity> results = new List<Entity>();
                entities.ForEach((o) =>
                {
                    if (o.UID == entity.UID)
                        results.Add(o);
                });
                if (results.Count > 0)
                {
                    foreach (var o in results)
                    {
                        entities.Remove(o);
                    }
                    return true;
                }
                return false;
            }
        }
        */

        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        static bool Add(List<Entity> entities, Entity entity)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count == 0)
            {
                // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
                // Exception:
                //      可能会抛出异常 ArgumentException
                entities.Add(entity);
                return true;
            }
            return false;
        }

        static void CheckPII(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.PII == null)
                {
                    Debug.Assert(false, "PII 不应为 null");
                }
            }
        }

        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        static bool Add(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count == 0)
            {
                // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
                // Exception:
                //      可能会抛出异常 ArgumentException
                entities.Add(NewEntity(tag, false));
                return true;
            }
            return false;
        }

        static bool Remove(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
            {
                foreach (var o in results)
                {
                    entities.Remove(o);
                }
                return true;
            }
            return false;
        }

        static List<Entity> LinkByName(string name)
        {
            List<Entity> entities = null;
            switch (name)
            {
                case "all":
                    entities = _all;
                    break;
                case "adds":
                    entities = _adds;
                    break;
                case "removes":
                    entities = _removes;
                    break;
                case "changes":
                    entities = _changes;
                    break;
                default:
                    throw new ArgumentException($"无法识别的 name 参数值 '{name}'");
            }

            return entities;
        }

        // 2020/4/13
        // 更新 entity 里面的读者记录相关数据
        static bool l_UpdateEntityXml(string name,
            string uid,
            string entity_xml)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                bool changed = false;
                foreach (var entity in entities)
                {
                    if (entity.UID == uid)
                    {
                        entity.SetData(entity.ItemRecPath, entity_xml);
                    }
                }
                return changed;
            }
        }

        // 更新 Entity 信息
        static bool l_Update(string name, TagAndData tag)
        {
            lock (_syncRoot_all)
            {
                List<Entity> entities = LinkByName(name);

                bool changed = false;
                foreach (var entity in entities)
                {
                    if (entity.UID == tag.OneTag.UID)
                    {
                        if (entity.ReaderName != tag.OneTag.ReaderName)
                        {
                            entity.ReaderName = tag.OneTag.ReaderName;
                            changed = true;
                        }
                        if (entity.Antenna != tag.OneTag.AntennaID.ToString())
                        {
                            entity.Antenna = tag.OneTag.AntennaID.ToString();
                            changed = true;
                        }
                        // 2019/11/26
                        if (entity.TagInfo != null && tag.OneTag.TagInfo != null
                            && entity.TagInfo.EAS != tag.OneTag.TagInfo.EAS)
                        {
                            entity.TagInfo.EAS = tag.OneTag.TagInfo.EAS;
                            // changed = true;
                        }
                    }
                }
                return changed;
            }
        }


        // 注意：这是不加锁的版本
        static bool Update(List<Entity> entities, TagAndData tag)
        {
            bool changed = false;
            foreach (var entity in entities)
            {
                if (entity.UID == tag.OneTag.UID)
                {
                    if (entity.ReaderName != tag.OneTag.ReaderName)
                    {
                        entity.ReaderName = tag.OneTag.ReaderName;
                        changed = true;
                    }
                    if (entity.Antenna != tag.OneTag.AntennaID.ToString())
                    {
                        entity.Antenna = tag.OneTag.AntennaID.ToString();
                        changed = true;
                    }
                    // 2019/11/26
                    if (entity.TagInfo != null && tag.OneTag.TagInfo != null
                        && entity.TagInfo.EAS != tag.OneTag.TagInfo.EAS)
                    {
                        entity.TagInfo.EAS = tag.OneTag.TagInfo.EAS;
                        // changed = true;
                    }
                }
            }
            return changed;
        }

#if NEW_VERSION

        public class UpdateResult : NormalResult
        {
            // 变化前的 Entity 内容
            public List<Entity> OldEntities { get; set; }
            // 变化后的 Entity 内容
            public List<Entity> NewEntities { get; set; }
        }

        // 注意：这是不加锁的版本
        static UpdateResult new_Update(List<Entity> entities, TagAndData tag)
        {
            List<Entity> old_entities = new List<Entity>();
            List<Entity> new_entities = new List<Entity>();

            // bool changed = false;
            foreach (var entity in entities)
            {
                if (entity.UID == tag.OneTag.UID)
                {
                    if (entity.ReaderName != tag.OneTag.ReaderName
                        || entity.Antenna != tag.OneTag.AntennaID.ToString()
                        || (entity.TagInfo != null && tag.OneTag.TagInfo != null
                        && entity.TagInfo.EAS != tag.OneTag.TagInfo.EAS))
                    {
                        old_entities.Add(entity.Clone());

                        entity.ReaderName = tag.OneTag.ReaderName;
                        entity.Antenna = tag.OneTag.AntennaID.ToString();
                        if (entity.TagInfo != null && tag.OneTag.TagInfo != null)
                            entity.TagInfo.EAS = tag.OneTag.TagInfo.EAS;

                        new_entities.Add(entity);
                        // changed = true;
                    }
                }
            }

            // return changed;
            return new UpdateResult
            {
                OldEntities = old_entities,
                NewEntities = new_entities
            };
        }

#endif

        // 故意选择用到的天线编号加一的天线(用 ListTags() 实现)
        public static async Task<NormalResult> SelectAntennaAsync()
        {
            StringBuilder text = new StringBuilder();
            List<string> errors = new List<string>();
            List<AntennaList> table = ShelfData.GetAntennaTable();
            foreach (var list in table)
            {
                if (list.Antennas == null || list.Antennas.Count == 0)
                    continue;
                // uint antenna = (uint)(list.Antennas[list.Antennas.Count - 1] + 1);
                int first_antenna = list.Antennas[0];
                text.Append($"readerName[{list.ReaderName}], antenna[{first_antenna}]\r\n");
                using (var releaser = await _inventoryLimit.EnterAsync().ConfigureAwait(false))
                {
                    try
                    {
                        var result = RfidManager.CallListTags($"{list.ReaderName}:{first_antenna}", "");
                        if (result.Value == -1)
                            errors.Add($"CallListTags() 出错: {result.ErrorInfo}");
                    }
                    catch (Exception ex)
                    {
                        // 2020/4/17
                        errors.Add($"CallListTags() 出现异常: {ex.Message}");
                        WpfClientInfo.WriteErrorLog($"SelectAntennaAsync() 中 CallListTags() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }
            }
            if (errors.Count > 0)
            {
                // this.SetGlobalError("InitialShelfEntities", $"SelectAntenna() 出错: {StringUtil.MakePathList(errors, ";")}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"SelectAntenna() 出错: {StringUtil.MakePathList(errors, ";")}"
                };
            }
            return new NormalResult
            {
                Value = 0,
                ErrorInfo = text.ToString()
            };
        }

        static bool _tagAdded = false;

        static SpeakList _speakList = new SpeakList();

        public delegate void Delagate_booksChanged();

        // 新版本事件
        // 跟随事件动态更新列表
        // Add: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        // Remove: 检查列表中是否存在这个 PII，如果存在，则修改状态为 不在架
        //      如果不存在这个 PII，则不做任何动作
        // Update: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        public static async Task ChangeEntitiesAsync(BaseChannel<IRfid> channel,
            SeperateResult e,
            Delagate_booksChanged func_booksChanged)
        {
            if (ShelfData.FirstInitialized == false)
                return;

            // 开门状态下，动态信息暂时不要合并
            bool changed = false;

            List<TagAndData> tags = new List<TagAndData>();
            if (e.add_books != null)
            {
                tags.AddRange(e.add_books);
            }

            if (e.updated_books != null)
            {
                tags.AddRange(e.updated_books);
            }

            // 2020/4/17
            // 忽略其他读卡器上的标签
            {
                var filtered = tags.FindAll(tag =>
                {
                    if (tag.OneTag.Protocol == InventoryInfo.ISO15693
                        && tag.OneTag.TagInfo == null)
                        return false;   // 忽略还没有 TagInfo 的那些超前的通知

                    // 判断一下 tag 是否属于已经定义的门范围
                    var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                    if (doors.Count > 0)
                        return true;
                    return false;
                });

                tags = filtered;
            }

            // 延时触发 SelectAntenna()
            if (tags.Count > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // 延时设置
                        await Task.Delay(TimeSpan.FromSeconds(10), App.CancelToken);
                        _tagAdded = true;
                    }
                    catch
                    {

                    }
                });
            }

            List<string> add_uids = new List<string>();
            int removeBooksCount = 0;
            lock (_syncRoot_all)
            {
                // 新添加标签(或者更新标签信息)
                foreach (var tag in tags)
                {
                    // 没有 TagInfo 信息的先跳过
                    if (tag.OneTag.TagInfo == null)
                        continue;

                    add_uids.Add(tag.OneTag.UID);

                    // 看看 _all 里面有没有
                    var results = Find(_all, tag);
                    if (results.Count == 0)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        if (Add(_adds, tag) == true)
                        {
                            changed = true;

                            // 刚刚增加的 patron 的 UID，记忆下来
                            //if (tag.Type == "patron")
                            //    new_patron_uids.Add(tag.OneTag.UID);
                        }
                        if (Remove(_removes, tag) == true)
                            changed = true;
                    }
                    else
                    {
                        bool processed = false;
                        /*
                        // var old_entities = Find(_all, o => o.UID == tag.OneTag.UID);
                        // 找到以前的对象
                        if (results.Count > 0)
                        {
                            var old_entity = results[0];
                            var old_doors = DoorItem.FindDoors(ShelfData.Doors, old_entity.ReaderName, old_entity.Antenna);
                            var new_doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());

                            // 如果新旧对象所在的门发生了转移
                            if (old_doors.Count > 0 && new_doors.Count > 0
        && old_doors[0] != new_doors[0])
                            {
                                // 新门
                                ReplaceOrAdd(_adds, tag);

                                // 旧门
                                ReplaceOrAdd(_removes, old_entity);
                                changed = true;

                                processed = true;
                            }

                            // 更新 _all 里面的信息
                            if (Update(_all, tag) == true)
                            {
                                tag.Type = null;    // 令 NewEntity 重新解析标签
                                // Exception:
                                //      可能会抛出异常 ArgumentException TagDataException
                                Add(_changes, tag);
                            }
                        }
                        */

                        if (processed == false)
                        {
                            // 更新 _all 里面的信息
                            if (Update(_all, tag) == true)
                            {
                                tag.Type = null;    // 令 NewEntity 重新解析标签

                                // Exception:
                                //      可能会抛出异常 ArgumentException TagDataException
                                Add(_changes, tag);
                            }

                            // 要把 _adds 和 _removes 里面都去掉
                            if (Remove(_adds, tag) == true)
                                changed = true;
                            if (Remove(_removes, tag) == true)
                                changed = true;
                        }
                    }
                }

                List<TagAndData> removes = null;
                {
                    // 2020/4/9
                    // 把书柜读卡器上的(ISO15693)读者卡也计算在内
                    removes = e.removed_books?.FindAll(tag =>
                    {
                        // 判断一下 tag 是否属于已经定义的门范围
                        var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                        if (doors.Count > 0)
                            return true;
                        return false;
                    });
                }

                // 拿走标签
                foreach (var tag in removes)
                {
                    if (tag.OneTag.TagInfo == null)
                        continue;

                    // 刚添加过的标签，这里就不要去移走了。即，添加比移除要优先
                    if (add_uids.IndexOf(tag.OneTag.UID) != -1)
                        continue;

                    // TODO: 特别注意，对于书柜门内的标签，要所属门完全一致才允许 remove

                    // 看看 _all 里面有没有
                    var results = l_Find("all", tag);
                    if (results.Count > 0)
                    {
                        if (Remove(_adds, tag) == true)
                            changed = true;
                        if (Remove(_changes, tag) == true)
                            changed = true;
                        /*
                        if (Add(_removes, tag) == true)
                        {
                            changed = true;
                        }
                        */
                        // 2020/4/5
                        // 这样可以利用 All 里面的 Entity 对象，通常其 Title 属性已经有值
                        if (Add("removes", results[0]) == true)
                            changed = true;
                    }
                    else
                    {
                        // _all 里面没有，很奇怪(是否写入错误日志？)。但，
                        // 要把 _adds 和 _removes 里面都去掉
                        if (Remove(_adds, tag) == true)
                            changed = true;
                        if (Remove(_removes, tag) == true)
                            changed = true;
                        if (Remove(_changes, tag) == true)
                            changed = true;
                    }

                    removeBooksCount++;
                }
            }

            // TODO: 把 add remove error 动作分散到每个门，然后再触发 ShelfData.BookChanged 事件

            if (changed == true)
            {
                // DoorItem.DisplayCount(_all, _adds, _removes, ShelfData.Doors);
                ShelfData.l_RefreshCount();
                func_booksChanged?.Invoke();
            }

            /*
            CheckPII(_all);
            CheckPII(_adds);
            CheckPII(_removes);
            CheckPII(_changes);
            */

            // TODO: 平时可以建立一个 cache，以后先从 cache 里面取书目摘要字符串
            _ = Task.Run(async () =>
            {
                try
                {
                    CancellationToken token = CancelToken;
                    await FillBookFieldsAsync(l_All, token, "refreshCount");
                    await FillBookFieldsAsync(l_Adds, token, "refreshCount");
                    await FillBookFieldsAsync(l_Removes, token, "refreshCount");
                    await FillBookFieldsAsync(l_Changes, token, "refreshCount");
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
        }

        #region 分离图书和读者标签的算法

        static object _syncRoot_patronTags = new object();
        static List<TagAndData> _patronTags = null;

        public static List<TagAndData> PatronTags
        {
            get
            {
                lock (_syncRoot_patronTags)
                {
                    return new List<TagAndData>(_patronTags);
                }
            }
        }

        static List<TagAndData> _bookTags = null;

        public static List<TagAndData> BookTags
        {
            get
            {
                lock (_syncRoot_patronTags)
                {
                    return new List<TagAndData>(_bookTags);
                }
            }
        }

        // 用 UID 找到，并移走
        static List<TagAndData> Remove(List<TagAndData> list,
            string uid,
            string reader_name,
            uint antenna)
        {
            List<TagAndData> found = list.FindAll((tag) =>
            {
                // 2020/4/29
                // TODO: 在添加到集合的地方进行检查，确保 .OneTag 不为 null
                if (tag.OneTag == null)
                    return false;
                return (tag.OneTag.UID == uid
                && tag.OneTag.ReaderName == reader_name
                && tag.OneTag.AntennaID == antenna);
            });
            foreach (var tag in found)
            {
                list.Remove(tag);
            }
            return found;
        }

        // 更新同 UID 的事项。如果没有找到，则在末尾添加
        // return:
        //      返回被替换掉的，以前的对象
        static List<TagAndData> Update(List<TagAndData> list, TagAndData tag)
        {
            List<TagAndData> found = list.FindAll((t) =>
            {
                return (t.OneTag.UID == tag.OneTag.UID);
            });
            foreach (var t in found)
            {
                list.Remove(t);
            }
            list.Add(tag);

            return found;
        }

        static bool Add(List<TagAndData> list, TagAndData tag)
        {
            List<TagAndData> found = list.FindAll((t) =>
            {
                return (t.OneTag.UID == tag.OneTag.UID);
            });
            if (found.Count > 0)
                return false;
            list.Add(tag);
            return true;
        }

        public class SeperateResult : NormalResult
        {
            public List<TagAndData> add_books { get; set; }
            public List<TagAndData> add_patrons { get; set; }
            public List<TagAndData> updated_books { get; set; }
            public List<TagAndData> updated_patrons { get; set; }
            public List<TagAndData> removed_books { get; set; }
            public List<TagAndData> removed_patrons { get; set; }
        }

        // 探测标签的类型。返回 "book" 或者 "patron" 或者 "other"。
        // 特殊地，.TagInfo 为 null 的 ISO15693 会暂时被当作 "book"
        public delegate string Delegate_detectType(OneTag tag);

        // 初始化 _patronTags 和 bookTags 两个集合
        public static void InitialPatronBookTags(Delegate_detectType func_detectType)
        {
            lock (_syncRoot_patronTags)
            {
                _patronTags = new List<TagAndData>();
                _bookTags = new List<TagAndData>();
                if (func_detectType != null)
                {
                    NewTagList.Tags.ForEach((tag) =>
                    {
                        var type = func_detectType(tag.OneTag);
                        if (type == "patron")
                            _patronTags.Add(tag);
                        else if (type == "book")
                            _bookTags.Add(tag);

                    /*
                    try
                    {
                        SetTagType(tag, out string pii);
                    }
                    catch (Exception ex)
                    {
                        tag.Error += ($"RFID 标签格式错误: {ex.Message}");
                    }
                    */
                    });
                }
            }
        }

        // 更新 _patronTags 和 _bookTags 集合
        // 要返回新增加的两类标签的数目
        // TODO: 要能处理 ISO15693 图书标签放到读者读卡器上的动作。可以弹出一个窗口显示这一本图书的信息
        public static async Task<SeperateResult> SeperateTagsAsync(BaseChannel<IRfid> channel,
            NewTagChangedEventArgs e,
            Delegate_detectType func_detectType)
        {
            lock (_syncRoot_patronTags)
            {
                // 2020/7/13
                // 临时初始化一下
                if (_patronTags == null || _bookTags == null)
                    InitialPatronBookTags(null);
#if NO
                // ***
                // 初始化
                if (_patronTags == null || _bookTags == null)
                {
                    _patronTags = new List<TagAndData>();
                    _bookTags = new List<TagAndData>();
                    NewTagList.Tags.ForEach((tag) =>
                    {
                        var type = func_detectType(tag.OneTag);
                        if (type == "patron")
                            _patronTags.Add(tag);
                        else if (type == "book")
                            _bookTags.Add(tag);

                        /*
                        try
                        {
                            SetTagType(tag, out string pii);
                        }
                        catch (Exception ex)
                        {
                            tag.Error += ($"RFID 标签格式错误: {ex.Message}");
                        }
                        */
                    });
                }

#endif

                List<TagAndData> add_books = new List<TagAndData>();
                List<TagAndData> add_patrons = new List<TagAndData>();
                List<TagAndData> updated_books = new List<TagAndData>();
                List<TagAndData> updated_patrons = new List<TagAndData>();
                List<TagAndData> removed_books = new List<TagAndData>();
                List<TagAndData> removed_patrons = new List<TagAndData>();

                // ****
                // 处理需要添加的对象
                List<TagAndData> tags = new List<TagAndData>();
                if (e.AddTags != null && e.AddTags.Count > 0)
                {
                    // 分离新添加的标签
                    e.AddTags.ForEach((tag) =>
                    {
                        // 对于 .TagInfo == null 的 ISO15693 标签不敏感
                        if (tag.OneTag.TagInfo == null
                && tag.OneTag.Protocol == InventoryInfo.ISO15693)
                            return;

                        var type = func_detectType(tag.OneTag);
                        if (type == "patron")
                        {
                            var ret = Add(_patronTags, tag);
                            if (ret == true)
                                add_patrons.Add(tag);
                        }
                        else if (type == "book")
                        {
                            var ret = Add(_bookTags, tag);
                            if (ret == true)
                                add_books.Add(tag);
                        }
                    });
                }

                // *** 
                // 处理更新了的对象
                if (e.UpdateTags != null && e.UpdateTags.Count > 0)
                {
                    // 分离更新了的标签
                    e.UpdateTags.ForEach((tag) =>
                    {
                        // 对于 .TagInfo == null 的 ISO15693 标签不敏感
                        if (tag.OneTag.TagInfo == null
                && tag.OneTag.Protocol == InventoryInfo.ISO15693)
                            return;

                        var type = func_detectType(tag.OneTag);
                        if (type == "patron")
                        {
                            var one_tag = tag.OneTag;
                            // TODO: 尝试从 _bookTags 里面移走
                            removed_books.AddRange(Remove(_bookTags, one_tag.UID, one_tag.ReaderName, one_tag.AntennaID));
                            Update(_patronTags, tag);
                            updated_patrons.Add(tag);
                        }
                        else if (type == "book")
                        {
                            var one_tag = tag.OneTag;
                            // TODO: 尝试从 _patronTags 里面移走
                            removed_patrons.AddRange(Remove(_patronTags, one_tag.UID, one_tag.ReaderName, one_tag.AntennaID));
                            Update(_bookTags, tag);
                            updated_books.Add(tag);
                        }
                    });
                }

                // ***
                // 处理移走了的对象
                if (e.RemoveTags != null && e.RemoveTags.Count > 0)
                {
                    // 分离移走了的标签
                    e.RemoveTags.ForEach((tag) =>
                    {
                        var one_tag = tag.OneTag;
                        var type = func_detectType(one_tag);
                        if (type == "patron" || type == "book")
                        {
                            // 注意，只有当 UID 和 读卡器名字 和 天线编号都相同才予以删除
                            removed_books.AddRange(Remove(_bookTags, one_tag.UID, one_tag.ReaderName, one_tag.AntennaID));
                            removed_patrons.AddRange(Remove(_patronTags, one_tag.UID, one_tag.ReaderName, one_tag.AntennaID));
                        }
                    });
                }

                /*
                {
                    var filtered = tags.FindAll(tag =>
                    {
                        if (tag.OneTag.ReaderName != _patronReaderName)
                            return false;
                        // 暂时忽略 .TagInfo 为空的那些 ISO15693 的标签
                        if (tag.OneTag.Protocol == InventoryInfo.ISO15693
                        && tag.OneTag.TagInfo == null)
                            return false;
                        try
                        {
                            SetTagType(tag, out string pii);
                        }
                        catch (Exception ex)
                        {
                            tag.Error += ($"RFID 标签格式错误: {ex.Message}");
                        }
                        if (tag.Type == "book")
                            return false;
                        return true;
                    });

                    tags = filtered;
                }

                lock (_syncRoot_patronTags)
                {
                    foreach (var tag in tags)
                    {
                        var found = _patronTags.FindAll(o =>
                        {
                            return o.OneTag.UID == tag.OneTag.UID;
                        });

                        if (found.Count > 0)
                        {
                            // 替换
                            int index = _patronTags.IndexOf(found[0]);
                            _patronTags[index] = tag;
                            count++;
                        }
                        else
                        {
                            _patronTags.Add(tag);
                            // 2020/4/17
                            // 如果是 ISO15693 并且 tagInfo 为 null，则不记入新添加的 count 计数
                            if (!(tag.OneTag.Protocol == InventoryInfo.ISO15693
        && tag.OneTag.TagInfo == null))
                                count++;
                        }
                    }
                }
                */

                // 2020/4/19
                foreach (var tag in updated_books)
                {
                    tag.Type = null;    // 迫使 NewEntity 重新解析标签
                }
                foreach (var tag in updated_patrons)
                {
                    tag.Type = null;    // 迫使 NewEntity 重新解析标签
                }

                return new SeperateResult
                {
                    add_books = add_books,
                    add_patrons = add_patrons,
                    updated_books = updated_books,
                    updated_patrons = updated_patrons,
                    removed_books = removed_books,
                    removed_patrons = removed_patrons,
                };
            }
        }


        // 更新 _patronTags 集合
        // TODO: 要能处理 ISO15693 图书标签放到读者读卡器上的动作。可以弹出一个窗口显示这一本图书的信息
        public static async Task<NormalResult> ChangePatronTagsAsync(BaseChannel<IRfid> channel,
            NewTagChangedEventArgs e)
        {
            int count = 0;
            // 初始化
            if (_patronTags == null)
            {
                lock (_syncRoot_patronTags)
                {
                    _patronTags = NewTagList.Tags.FindAll((tag) =>
                    {
                        if (tag.OneTag.ReaderName != _patronReaderName)
                            return false;
                        // TODO: ISO15693 的 .TagInfo 是否可能为 null?
                        // 排除 ISO15693 的图书标签
                        try
                        {
                            SetTagType(tag, out string pii);
                        }
                        catch (Exception ex)
                        {
                            tag.Error += ($"RFID 标签格式错误: {ex.Message}");
                        }
                        if (tag.Type == "book")
                            return false;
                        return true;
                    });
                }
                return new NormalResult { Value = 1 };
            }

            // ****
            // 处理需要添加的对象
            List<TagAndData> tags = new List<TagAndData>();
            if (e.AddTags != null)
            {
                tags.AddRange(e.AddTags);
                /*
                // 延时触发 SelectAntenna()
                if (e.AddTags.Count > 0)
                    _tagAdded = true;
                    */
            }

            if (e.UpdateTags != null)
            {
                tags.AddRange(e.UpdateTags);
                /*
                // 2020/4/15
                if (e.UpdateTags.Count > 0)
                    _tagAdded = true;
                    */
            }

            {
                var filtered = tags.FindAll(tag =>
                {
                    if (tag.OneTag.ReaderName != _patronReaderName)
                        return false;
                    // 暂时忽略 .TagInfo 为空的那些 ISO15693 的标签
                    if (tag.OneTag.Protocol == InventoryInfo.ISO15693
            && tag.OneTag.TagInfo == null)
                        return false;
                    try
                    {
                        SetTagType(tag, out string pii);
                    }
                    catch (Exception ex)
                    {
                        tag.Error += ($"RFID 标签格式错误: {ex.Message}");
                    }
                    if (tag.Type == "book")
                        return false;
                    return true;
                });

                tags = filtered;
            }

            lock (_syncRoot_patronTags)
            {
                foreach (var tag in tags)
                {
                    var found = _patronTags.FindAll(o =>
                    {
                        return o.OneTag.UID == tag.OneTag.UID;
                    });

                    if (found.Count > 0)
                    {
                        // 替换
                        int index = _patronTags.IndexOf(found[0]);
                        _patronTags[index] = tag;
                        count++;
                    }
                    else
                    {
                        _patronTags.Add(tag);
                        // 2020/4/17
                        // 如果是 ISO15693 并且 tagInfo 为 null，则不记入新添加的 count 计数
                        if (!(tag.OneTag.Protocol == InventoryInfo.ISO15693
        && tag.OneTag.TagInfo == null))
                            count++;
                    }
                }
            }

            // ****
            // 处理需要移走的对象
            List<TagAndData> removes = null;
            {
                // 2020/4/9
                // 把书柜读卡器上的(ISO15693)读者卡也计算在内
                removes = e.RemoveTags?.FindAll(tag =>
                {
                    if (tag.OneTag.ReaderName != _patronReaderName)
                        return false;
                    return true;
                });
            }

            if (removes.Count > 0)
            {
                lock (_syncRoot_patronTags)
                {
                    foreach (var tag in removes)
                    {
                        Remove(_patronTags, tag.OneTag.UID);
                        // count++;
                    }
                }
            }
            return new NormalResult { Value = count };

            void Remove(List<TagAndData> collection, string uid)
            {
                var found = collection.FindAll(o =>
                {
                    return o.OneTag.UID == uid;
                });
                foreach (var tag in found)
                {
                    collection.Remove(tag);
                }
            }
        }

        #endregion

#if OLD_TAGCHANGED
        // 跟随事件动态更新列表
        // Add: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        // Remove: 检查列表中是否存在这个 PII，如果存在，则修改状态为 不在架
        //      如果不存在这个 PII，则不做任何动作
        // Update: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        public static async Task ChangeEntitiesAsync(BaseChannel<IRfid> channel,
            TagChangedEventArgs e,
            Delagate_booksChanged func_booksChanged)
        {
            if (ShelfData.FirstInitialized == false)
                return;

            // 开门状态下，动态信息暂时不要合并
            bool changed = false;

            List<TagAndData> tags = new List<TagAndData>();
            if (e.AddBooks != null)
            {
                tags.AddRange(e.AddBooks);
                // 延时触发 SelectAntenna()
                if (e.AddBooks.Count > 0)
                    _tagAdded = true;
            }

            if (e.UpdateBooks != null)
                tags.AddRange(e.UpdateBooks);

            // 2020/4/9
            // 把书柜读卡器上的(ISO15693)读者卡也计算在内
            {
                List<TagAndData> temp = new List<TagAndData>();
                if (e.AddPatrons != null)
                    temp.AddRange(e.AddPatrons);
                if (e.UpdatePatrons != null)    // 因为有两阶段通知的问题，所以 update 的也应该考虑在内
                    temp.AddRange(e.UpdatePatrons);
                var patrons = temp.FindAll(tag =>
                {
                    if (tag.OneTag.Protocol != InventoryInfo.ISO15693)
                        return false;
                    if (tag.OneTag.TagInfo == null)
                        return false;   // 忽略还没有 TagInfo 的那些超前的通知

                    // 判断一下 tag 是否属于已经定义的门范围
                    var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                    if (doors.Count > 0)
                        return true;
                    return false;
                });
                /*
                foreach (var patron in patrons)
                {
                    var type = patron.Type;
                }
                */
                tags.AddRange(patrons);
            }

            // List<string> new_patron_uids = new List<string>();

            List<string> add_uids = new List<string>();
            int removeBooksCount = 0;
            lock (_syncRoot_all)
            {
                // 新添加标签(或者更新标签信息)
                foreach (var tag in tags)
                {
                    // 没有 TagInfo 信息的先跳过
                    if (tag.OneTag.TagInfo == null)
                        continue;

                    add_uids.Add(tag.OneTag.UID);

                    // 看看 _all 里面有没有
                    var results = Find(_all, tag);
                    if (results.Count == 0)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        if (Add(_adds, tag) == true)
                        {
                            changed = true;

                            // 刚刚增加的 patron 的 UID，记忆下来
                            //if (tag.Type == "patron")
                            //    new_patron_uids.Add(tag.OneTag.UID);
                        }
                        if (Remove(_removes, tag) == true)
                            changed = true;
                    }
                    else
                    {
                        // 更新 _all 里面的信息
                        if (Update(_all, tag) == true)
                        {
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            Add(_changes, tag);
                        }

                        // 要把 _adds 和 _removes 里面都去掉
                        if (Remove(_adds, tag) == true)
                            changed = true;
                        if (Remove(_removes, tag) == true)
                            changed = true;
                    }
                }

                var removes = e.RemoveBooks;
                {
                    // 2020/4/9
                    // 把书柜读卡器上的(ISO15693)读者卡也计算在内
                    var remove_patrons = e.RemovePatrons?.FindAll(tag =>
                    {
                        if (tag.OneTag.Protocol != InventoryInfo.ISO15693)
                            return false;
                        // 判断一下 tag 是否属于已经定义的门范围
                        var doors = DoorItem.FindDoors(ShelfData.Doors, tag.OneTag.ReaderName, tag.OneTag.AntennaID.ToString());
                        if (doors.Count > 0)
                            return true;
                        return false;
                    });
                    if (remove_patrons != null)
                        removes.AddRange(remove_patrons);
                }

                // 拿走标签
                foreach (var tag in removes)
                {
                    if (tag.OneTag.TagInfo == null)
                        continue;

                    //if (tag.Type == "patron")
                    //    continue;

                    /*
                    // 2020/4/10
                    // 刚增加的 patron，这里就不要去移走了
                    if (new_patron_uids.IndexOf(tag.OneTag.UID) != -1)
                        continue;
                        */

                    // 2020/4/10
                    // 刚添加过的标签，这里就不要去移走了。即，添加比移除要优先
                    if (add_uids.IndexOf(tag.OneTag.UID) != -1)
                        continue;

                    // 看看 _all 里面有没有
                    var results = Find("all", tag);
                    if (results.Count > 0)
                    {
                        if (Remove(_adds, tag) == true)
                            changed = true;
                        if (Remove(_changes, tag) == true)
                            changed = true;
                        /*
                        if (Add(_removes, tag) == true)
                        {
                            changed = true;
                        }
                        */
                        // 2020/4/5
                        // 这样可以利用 All 里面的 Entity 对象，通常其 Title 属性已经有值
                        if (Add("removes", results[0]) == true)
                            changed = true;
                    }
                    else
                    {
                        // _all 里面没有，很奇怪(是否写入错误日志？)。但，
                        // 要把 _adds 和 _removes 里面都去掉
                        if (Remove(_adds, tag) == true)
                            changed = true;
                        if (Remove(_removes, tag) == true)
                            changed = true;
                        if (Remove(_changes, tag) == true)
                            changed = true;
                    }

                    removeBooksCount++;
                }

            }

            StringUtil.RemoveDup(ref add_uids, false);
            int add_count = add_uids.Count;
            int remove_count = 0;
            if (e.RemoveBooks != null)
                remove_count = removeBooksCount; // 注： e.RemoveBooks.Count 是不准确的，有时候会把 ISO15693 的读者卡判断时作为 remove 信号

#if REMOVED
            if (remove_count > 0)
            {
                // App.CurrentApp.SpeakSequence($"取出 {remove_count} 本");
                Sound(1, remove_count, "取出");
                /*
                _speakList.Speak("取出 {0} 本",
                    remove_count,
                    (s) =>
                    {
                        App.CurrentApp.SpeakSequence(s);
                    });
                    */
            }
            if (add_count > 0)
            {
                Sound(2, add_count, "放入");
                /*
                // App.CurrentApp.SpeakSequence($"放入 {add_count} 本");
                _speakList.Speak("放入 {0} 本",
    add_count,
    (s) =>
    {
        App.CurrentApp.SpeakSequence(s);
    });
    */
            }
#endif

            // TODO: 把 add remove error 动作分散到每个门，然后再触发 ShelfData.BookChanged 事件

            if (changed == true)
            {
                // DoorItem.DisplayCount(_all, _adds, _removes, ShelfData.Doors);
                ShelfData.RefreshCount();
                func_booksChanged?.Invoke();
            }

            // TODO: 平时可以建立一个 cache，以后先从 cache 里面取书目摘要字符串
            var task = Task.Run(async () =>
            {
                CancellationToken token = CancelToken;
                await FillBookFieldsAsync(All, token);
                await FillBookFieldsAsync(Adds, token);
                await FillBookFieldsAsync(Removes, token);
            });
        }

#endif

        static int[] tones = new int[] { 523, 659, 783 };
        /*
         *  C4: 261 330 392
            C5: 523 659 783
         * */
        public static void Sound(int tone, int count, string text)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < count; i++)
                        System.Console.Beep(tones[tone], 500);
                    if (string.IsNullOrEmpty(text) == false)
                        App.CurrentApp.SpeakSequence(text); // 不打断前面的说话
                }
                catch
                {

                }
            });
        }

        public class FillBookFieldsResult : NormalResult
        {
            public List<string> Errors { get; set; }
        }

        /*
         * FillBookFieldsAsync() 遇到的报错类型有两种：1) RFID 标签解析出错；2) 在获取册记录信息的过程中，通讯出错，或者册记录没有找到
         * */
        // TODO: 刷新 data 以前，是否先把有关字段都设置为 ?，避免观看者误会
        // TODO: 获取册记录，优先从缓存中获取。注意借书、还书、转移等同步操作后，要及时更新或者废止缓存内容
        public static async Task<FillBookFieldsResult> FillBookFieldsAsync(// BaseChannel<IRfid> channel,
        IReadOnlyCollection<Entity> entities,
        CancellationToken token,
        string style/*,
    bool refreshCount = true*/)
        {
            // 是否重新获得册记录?
            bool refresh_data = StringUtil.IsInList("refreshData", style);
            // 是否刷新门上的数字
            bool refreshCount = StringUtil.IsInList("refreshCount", style);

            bool localGetEntityInfo = StringUtil.IsInList("localGetEntityInfo", style);

            // int error_count = 0;
            int request_error_count = 0;    // 请求因为通讯失败的次数
            List<string> errors = new List<string>();
            foreach (Entity entity in entities)
            {
                if (token.IsCancellationRequested)
                    return new FillBookFieldsResult
                    {
                        Value = -1,
                        ErrorInfo = "中断",
                        ErrorCode = "cancelled"
                    };
                /*
                if (_cancel == null
                    || _cancel.IsCancellationRequested)
                    return;
                    */
                if (entity.FillFinished == true)
                    continue;

                //if (string.IsNullOrEmpty(entity.Error) == false)
                //    continue;

                // 获得 PII
                // 注：如果 PII 为空，文字中要填入 "(空)"
                if (string.IsNullOrEmpty(entity.PII))
                {
                    if (entity.TagInfo == null)
                        continue;

                    Debug.Assert(entity.TagInfo != null);
                    LogicChip chip = null;
                    try
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        chip = LogicChip.From(entity.TagInfo.Bytes,
        (int)entity.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"解析 RFID 标签(UID:{entity.TagInfo.UID})时出现异常 {ex.Message}");
                        continue;
                    }

                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    if (string.IsNullOrEmpty(pii))
                    {
                        // 报错
                        App.CurrentApp.SpeakSequence($"警告：发现 PII 字段为空的标签");
                        entity.SetError($"PII 字段为空");
                        entity.FillFinished = true;
                        // error_count++;
                        errors.Add($"标签 PII 字段为空(UID={entity.TagInfo.UID})");
                        continue;
                    }

                    entity.PII = PageBorrow.GetCaption(pii);
                }

                // 获得 Title
                // 注：如果 Title 为空，文字中要填入 "(空)"
                if ((string.IsNullOrEmpty(entity.Title) || refresh_data)
                    && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                {
                    GetEntityDataResult result = null;
                    if (localGetEntityInfo)
                    {
                        // 只从本地数据库中获取
                        result = LocalGetEntityData(entity.PII);
                        if (string.IsNullOrEmpty(result.Title) == false)
                            entity.Title = PageBorrow.GetCaption(result.Title);
                        if (string.IsNullOrEmpty(result.ItemXml) == false)
                            entity.SetData(result.ItemRecPath, result.ItemXml);
                    }
                    else
                    {
                        result = await GetEntityDataAsync(entity.PII, "");
                        if (result.Value == -1 || result.Value == 0)
                        {
                            // TODO: 条码号没有找到的错误码要单独记下来
                            // 报错
                            string error = $"警告：PII 为 {entity.PII} 的标签出错: {result.ErrorInfo}";
                            if (result.ErrorCode == "NotFound")
                                error = $"警告：PII 为 {entity.PII} 的图书没有找到记录";

                            // 2020/3/5
                            WpfClientInfo.WriteErrorLog($"GetEntityData() error: {error}");

                            // TODO: 如果发现当前一直是通讯中断的情况，要避免语音念太多报错
                            // App.CurrentApp.SpeakSequence(error);
                            entity.SetError(result.ErrorInfo);
                            // 2020/4/8
                            if (result.ErrorCode == "RequestError" || result.ErrorCode == "RequestTimeOut")
                            {
                                // 如果是通讯失败导致的出错，应该有办法进行重试获取
                                entity.FillFinished = false;
                                // 统计通讯失败次数
                                request_error_count++;
                            }
                            else
                                entity.FillFinished = true;
                            // error_count++;
                            errors.Add(error);
                            continue;
                        }
                        entity.Title = PageBorrow.GetCaption(result.Title);
                        entity.SetData(result.ItemRecPath, result.ItemXml);
                    }
                }

                // entity.SetError(null);
                entity.FillFinished = true;

                if (request_error_count >= 2)
                {
                    /*
                    if (App.TrySwitchToLocalMode() == true)
                        localGetEntityInfo = true;
                    */
                    if (localGetEntityInfo == false)
                        return new FillBookFieldsResult
                        {
                            Value = -1,
                            ErrorInfo = "请求 dp2library 时通讯失败",
                            ErrorCode = "requestError"
                        };
                }
            }

            if (token.IsCancellationRequested)
                return new FillBookFieldsResult
                {
                    Value = -1,
                    ErrorInfo = "中断",
                    ErrorCode = "cancelled"
                };

            if (refreshCount)
                ShelfData.l_RefreshCount();

            return new FillBookFieldsResult { Errors = errors };
            /*
            }
            catch (Exception ex)
            {
                //LibraryChannelManager.Log?.Error($"FillBookFields() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                //SetGlobalError("current", $"FillBookFields() 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
            }
            */
        }

        static string GetRandomString()
        {
            Random rnd = new Random();
            return rnd.Next(1, 999999).ToString();
        }

        // 限制获取摘要时候可以并发使用的 LibraryChannel 通道数
        // static Semaphore _limit = new Semaphore(1, 1);

        // public delegate void Delegate_showDialog();

        // -1 -1 n only change progress value
        // -1 -1 -1 hide progress bar
        public delegate void Delegate_setProgress(double min, double max, double value, string text);

        // TODO: 结果似乎可以考虑直接设置 ActionInfo 的 State 成员？这样返回后直接写入数据库即可
        // TODO: 无法进行重试的错误，应该尝试在本地 SQLite 数据库中建立借还信息，以便日后追查
        // 提交请求到 dp2library 服务器
        // parameters:
        //      actions 要处理的 Action 集合。每个 Action 对象处理完以后，会自动从 _actions 中移除
        //      style   "auto_stop" 遇到报错就停止处理后面部分
        // result.Value
        //      -1  出错(要用对话框显示结果)
        //      0   没有必要处理
        //      1   已经完成处理(要用对话框显示结果)
        public static async Task<SubmitResult> SubmitCheckInOutAsync(
            Delegate_setProgress func_setProgress,
            IReadOnlyCollection<ActionInfo> actions,
            string style)
        {
            // TODO: 如果当前没有读者身份，则当作初始化处理，将书柜内的全部图书做还书尝试；被拿走的图书记入本地日志(所谓无主操作)
            // TODO: 注意还书，也就是往书柜里面放入图书，是不需要具体读者身份就可以提交的

            // TODO: 属于 free 类型的门里面的图书不要参与处理

            // ProgressWindow progress = null;
            //string patron_name = "";
            //patron_name = _patron.PatronName;

            // 先尽量执行还书请求，再报错说无法进行借书操作(记入错误日志)
            MessageDocument doc = new MessageDocument();

            /*
            // 限制同时能进入临界区的线程个数
            // TODO: 如果另一个并发的 submit 过程时间较长，导致这里超时了，应该需要自动重试
            // true if the current instance receives a signal; otherwise, false.
            if (_limit.WaitOne(TimeSpan.FromSeconds(10)) == false)
                return new SubmitResult
                {
                    Value = -1,
                    ErrorInfo = "获得资源过程中超时",
                    ErrorCode = "limitTimeout",
                    RetryActions = new List<ActionInfo>(actions),
                };
                */

            try
            {
                // ClearEntitiesError();

                /*
                if (progress != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value = 0;
                        progress.ProgressBar.Minimum = 0;
                        progress.ProgressBar.Maximum = actions.Count;
                    }));
                }
                */
                int index = 0;
                func_setProgress?.Invoke(0, actions.Count, index, "正在处理，请稍候 ...");

                // TODO: 准备工作：把涉及到的 Entity 对象的字段填充完整
                // 检查 PII 是否都具备了

                // xml 发生改变了的那些实体记录
                List<Entity> updates = new List<Entity>();

                List<ActionInfo> processed = new List<ActionInfo>();

                // 出错了的，需要重做请求 dp2library 的那些 Action
                // List<ActionInfo> retry_actions = new List<ActionInfo>();

                // 出错了的，但无法进行重试的那些 Action
                List<ActionInfo> error_actions = new List<ActionInfo>();

                foreach (ActionInfo info in actions)
                {
                    // testing 
                    // Thread.Sleep(1000);

                    string action = info.Action;
                    Entity entity = info.Entity;

                    // 2020/4/27
                    info.SyncOperTime = DateTime.Now;

                    // 2020/4/8
                    // 如果 PII 为空
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        info.State = "dontsync";
                        info.SyncErrorCode = "PiiEmpty";
                        info.SyncErrorInfo = $"UID 为 {entity.UID} 的标签 PII 为空，不再进行同步(标签原出错信息 '{entity.Error}')";
                        processed.Add(info);
                        error_actions.Add(info);
                        continue;
                    }

                    // 2020/7/14
                    // 如果误放入了读者卡
                    if (entity.ErrorCode == "patronCard")
                    {
                        info.State = "dontsync";
                        info.SyncErrorCode = "PatronCard";
                        info.SyncErrorInfo = $"UID 为 {entity.UID} 的标签是误放入书柜的读者卡，不再进行同步(标签原出错信息 '{entity.Error}')";
                        processed.Add(info);
                        error_actions.Add(info);
                        continue;
                    }

                    if (info.Action == "transfer"
                        && info.TransferDirection == "out"
                        && string.IsNullOrEmpty(info.Location) == true
                        && string.IsNullOrEmpty(info.CurrentShelfNo) == true)
                    {
                        info.State = "dontsync";
                        info.SyncErrorCode = "NotSupport";  // 目前暂不支持同步此请求
                        info.SyncErrorInfo = $"(无目标式)下架请求暂不支持同步到 dp2library";
                        processed.Add(info);
                        error_actions.Add(info);
                        continue;
                    }
#if REMOVED
                    string action_name = "借书";
                    if (action == "return")
                        action_name = "还书";
                    else if (action == "renew")
                        action_name = "续借";
                    else if (action == "transfer")
                        action_name = "转移";
#endif

                    // 借书操作必须要有读者身份的请求者
                    if (action == "borrow")
                    {
                        if (string.IsNullOrEmpty(info.Operator.PatronBarcode)
                            || info.Operator.IsWorker == true)
                        {
                            MessageItem error = new MessageItem
                            {
                                SyncCount = info.SyncCount,
                                Operator = info.Operator,
                                OperTime = info.OperTime,
                                Operation = action,
                                ResultType = "error",
                                ErrorCode = "InvalidOperator",
                                ErrorInfo = "缺乏请求者",
                                Entity = entity,
                            };
                            doc.Add(error);
                            // 写入错误日志
                            WpfClientInfo.WriteInfoLog($"册 '{GetPiiString(entity)}' 因缺乏请求者无法进行借书请求");
                            continue;
                        }
                    }

#if REMOVED
                    // 2019/11/25
                    // 还书操作前先尝试修改 EAS
                    if (action == "return")
                    {
                        var result = SetEAS(entity.UID, entity.Antenna, false);
                        if (result.Value == -1)
                        {
                            string text = $"修改 EAS 动作失败: {result.ErrorInfo}";
#if REMOVED
                            entity.SetError(text, "yellow");
#endif

                            MessageItem error = new MessageItem
                            {
                                SyncCount = info.SyncCount,
                                Operator = info.Operator,
                                Operation = "changeEAS",
                                ResultType = "error",
                                ErrorCode = "ChangeEasFail",
                                ErrorInfo = text,
                                Entity = entity,
                            };
                            doc.Add(error);

                            // 写入错误日志
                            WpfClientInfo.WriteInfoLog($"修改册 '{entity.PII}' 的 EAS 失败: {result.ErrorInfo}");
                        }
                    }
#endif

                    // 实际操作时间
                    string operTimeStyle = "";
                    if (info.OperTime > DateTime.MinValue)
                        operTimeStyle = $",operTime:{StringUtil.EscapeString(DateTimeUtil.Rfc1123DateTimeStringEx(info.OperTime), ",:")}";

                    long lRet = 0;
                    ErrorCode error_code = ErrorCode.NoError;

                    string strError = "";
                    string[] item_records = null;
                    string[] biblio_records = null;
                    BorrowInfo borrow_info = null;
                    ReturnInfo return_info = null;

                    string strUserName = info.Operator?.GetWorkerAccountName();

                    int nRedoCount = 0;
                REDO:
                    entity.Waiting = true;
                    LibraryChannel channel = App.CurrentApp.GetChannel(strUserName);
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    try
                    {
                        string strStyle = "item";   //  "item,reader";
                        if (entity.Title == null)
                            strStyle += ",biblio";

                        if (action == "borrow" || action == "renew")
                        {
                            if (string.IsNullOrEmpty(info.ActionString) == false)
                            {
                                var old_borrow_info = JsonConvert.DeserializeObject<BorrowInfo>(info.ActionString);
                                if (old_borrow_info.Overflows != null && old_borrow_info.Overflows.Length > 0)
                                {
                                    string value = StringUtil.EscapeString(string.Join("; ", old_borrow_info.Overflows), ":,");
                                    strStyle += $",overflow:{value}";
                                }
                                else if (string.IsNullOrEmpty(old_borrow_info.Period) == false)
                                {
                                    string value = StringUtil.EscapeString(old_borrow_info.Period, ":,");
                                    strStyle += $",requestPeriod:{value}";
                                }
                            }
                            // TODO: 智能书柜要求强制借书。如果册操作前处在被其他读者借阅状态，要自动先还书再进行借书

                            lRet = channel.Borrow(null,
                                action == "renew",
                                info.Operator.PatronBarcode,
                                entity.PII,
                                entity.ItemRecPath,
                                false,
                                null,
                                strStyle + ",overflowable" + operTimeStyle, // style,
                                "xml", // item_format_list
                                out item_records,
                                "xml",
                                out string[] reader_records,
                                "summary",
                                out biblio_records,
                                out string[] dup_path,
                                out string output_reader_barcode,
                                out borrow_info,
                                out strError);
                        }
                        else if (action == "return")
                        {
                            /*
                            // TODO: 增加检查 EAS 现有状态功能，如果已经是 true 则不用修改，后面 API 遇到出错后也不要回滚 EAS
                            // return 操作，提前修改 EAS
                            // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                            {
                                var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                                if (result.Value == -1)
                                {
                                    entity.SetError($"{action_name}时修改 EAS 动作失败: {result.ErrorInfo}", "red");
                                    errors.Add($"册 '{entity.PII}' {action_name}时修改 EAS 动作失败: {result.ErrorInfo}");
                                    continue;
                                }
                            }
                            */
                            // 智能书柜不使用 EAS 状态。可以考虑统一修改为 EAS Off 状态？

                            lRet = channel.Return(null,
                                "return",
                                "", // _patron.Barcode,
                                entity.PII,
                                entity.ItemRecPath,
                                false,
                                strStyle + operTimeStyle, // style,
                                "xml", // item_format_list
                                out item_records,
                                "xml",
                                out string[] reader_records,
                                "summary",
                                out biblio_records,
                                out string[] dup_path,
                                out string output_reader_barcode,
                                out return_info,
                                out strError);
                        }
                        else if (action == "transfer")
                        {
                            // currentLocation 元素内容。格式为 馆藏地:架号
                            // 注意馆藏地和架号字符串里面不应包含逗号和冒号
                            List<string> commands = new List<string>();
                            if (string.IsNullOrEmpty(info.CurrentShelfNo) == false)
                                commands.Add($"currentLocation:{StringUtil.EscapeString(info.CurrentShelfNo, ":,")}");
                            if (string.IsNullOrEmpty(info.Location) == false)
                                commands.Add($"location:{StringUtil.EscapeString(info.Location, ":,")}");
                            if (string.IsNullOrEmpty(info.BatchNo) == false)
                                commands.Add($"batchNo:{StringUtil.EscapeString(info.BatchNo, ":,")}");

                            // string currentLocation = GetRandomString(); // testing
                            // TODO: 如果先前 entity.Title 已经有了内容，就不要在本次 Return() API 中要求返 biblio summary
                            lRet = channel.Return(null,
                                "transfer",
                                "", // _patron.Barcode,
                                entity.PII,
                                entity.ItemRecPath,
                                false,
                                $"{strStyle},{StringUtil.MakePathList(commands, ",")}" + operTimeStyle, // style,
                                "xml", // item_format_list
                                out item_records,
                                "xml",
                                out string[] reader_records,
                                "summary",
                                out biblio_records,
                                out string[] dup_path,
                                out string output_reader_barcode,
                                out return_info,
                                out strError);
                        }

                        error_code = channel.ErrorCode; // 保存下来，避免被 ReturnChannel 以后破坏
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        App.CurrentApp.ReturnChannel(channel);
                        entity.Waiting = false;
                    }


                    // 2020/3/7
                    if ((error_code == ErrorCode.RequestError
        || error_code == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            if (StringUtil.IsInList("network_sensitive", style))
                                return new SubmitResult
                                {
                                    Value = -1,
                                    ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                    ErrorCode = "requestError"
                                };
                        }
                    }

                    processed.Add(info);

                    if (lRet != -1)
                    {
                        if (info.Action == "borrow")
                        {
                            if (borrow_info == null)
                                info.ActionString = null;
                            else
                                info.ActionString = JsonConvert.SerializeObject(borrow_info);
                        }
                        else
                        {
                            if (return_info == null)
                                info.ActionString = null;
                            else
                                info.ActionString = JsonConvert.SerializeObject(return_info);
                        }
                    }

                    /*
                    // testing
                    lRet = -1;
                    strError = "testing";
                    channel.ErrorCode = ErrorCode.AccessDenied;
                    */

                    func_setProgress?.Invoke(-1, -1, ++index, null);

                    if (entity.Title == null
                        && biblio_records != null
                        && biblio_records.Length > 0
                        && string.IsNullOrEmpty(biblio_records[0]) == false)
                        entity.Title = biblio_records[0];

                    string title = GetPiiString(entity);
                    if (string.IsNullOrEmpty(entity.Title) == false)
                        title += " (" + entity.Title + ")";

#if REMOVED
                    // TODO: 其实 SaveActions 里面已经处理了 all adds removes changed 数组，这里似乎不需要再处理了
                    if (action == "borrow" || action == "return")
                    {
                        // 把 _adds 和 _removes 归入 _all
                        // 一边处理一边动态修改 _all?
                        if (action == "return")
                            ShelfData.Add("all", entity);
                        else
                            ShelfData.Remove("all", entity);

                        ShelfData.Remove("adds", entity);
                        ShelfData.Remove("removes", entity);
                    }

                    if (action == "transfer")
                        ShelfData.Remove("changes", entity);
#endif

                    string resultType = "succeed";
                    if (lRet == -1)
                        resultType = "error";
                    else if (lRet == 1)
                        resultType = "information";
                    string direction = "";
                    if (string.IsNullOrEmpty(info.Location) == false)
                        direction = $"家({info.Location})";
                    if (string.IsNullOrEmpty(info.CurrentShelfNo) == false)
                        direction += $" 当前位置({info.CurrentShelfNo})";
                    MessageItem messageItem = new MessageItem
                    {
                        SyncCount = info.SyncCount,
                        Operator = info.Operator,
                        OperTime = info.OperTime,
                        Operation = action,
                        ResultType = resultType,
                        ErrorCode = error_code.ToString(),
                        ErrorInfo = strError,
                        Entity = entity,
                        Direction = $"-->{direction}",
                    };
                    doc.Add(messageItem);

                    {
                        info.SyncErrorInfo = strError;
                        if (error_code != ErrorCode.NoError)
                            info.SyncErrorInfo += $"[{error_code}]";
                        info.SyncErrorCode = error_code.ToString();
                        info.SyncCount++;
                    }

                    // 微调
                    if (lRet == 0 && action == "return")
                        messageItem.ErrorInfo = "";

                    // sync/commerror/normalerror/空
                    // 同步成功/通讯出错/一般出错/从未同步过
                    info.State = "sync";

                    if (lRet == -1)
                    {
                        /*
                        // return 操作如果 API 失败，则要改回原来的 EAS 状态
                        if (action == "return")
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, false);
                            if (result.Value == -1)
                                strError += $"\r\n并且复原 EAS 状态的动作也失败了: {result.ErrorInfo}";
                        }
                        */

                        if (action == "borrow")
                        {
                            // TODO: ErrorCode.AlreadyBorrowedByOther 应该补一个还书动作然后重试借书?
                            if (error_code == ErrorCode.AlreadyBorrowed)
                            {
                                messageItem.ResultType = "information";
                                WpfClientInfo.WriteInfoLog($"读者 {info.Operator.PatronName} {info.Operator.PatronBarcode} 尝试借阅册 '{title}' 时: {strError}");
#if REMOVED
                                entity.SetError(null);
#endif
                                continue;
                            }
                        }

                        if (action == "return")
                        {
                            if (error_code == ErrorCode.NotBorrowed)
                            {
                                messageItem.ResultType = "information";
                                WpfClientInfo.WriteInfoLog($"读者 {info.Operator.PatronName} {info.Operator.PatronBarcode} 尝试还回册 '{title}' 时: {strError}");
                                // TODO: 这里也要修改 EAS
#if REMOVED
                                entity.SetError(null);
#endif
                                continue;
                            }

                            // 2020/4/29
                            if (error_code == ErrorCode.SyncDenied)
                            {
                                messageItem.ResultType = "information";
                            }
                        }

                        if (action == "transfer")
                        {
                            if (error_code == ErrorCode.NotChanged)
                            {
                                // 不出现在结果中
                                // doc.Remove(messageItem);

                                // 改为警告
                                messageItem.ResultType = "information";
                                // messageItem.ErrorCode = channel.ErrorCode.ToString();
                                // 界面警告
                                //warnings.Add($"册 '{title}' (尝试转移时发现没有发生修改): {strError}");
                                // 写入错误日志
                                WpfClientInfo.WriteInfoLog($"转移册 '{title}' 时: {strError}");
#if REMOVED
                                entity.SetError(null);
#endif
                                continue;
                            }
                        }

                        error_actions.Add(info);

                        // 如果是通讯出错，要加入 retry_actions
                        if (error_code == ErrorCode.RequestError
                            || error_code == ErrorCode.RequestTimeOut
                            || error_code == ErrorCode.RequestCanceled
                            )
                        {
                            // retry_actions.Add(info);
                            info.State = "commerror";
                        }
                        else
                        {
                            if (error_code == ErrorCode.ItemBarcodeNotFound
                                || error_code == ErrorCode.SyncDenied)  // 2020/4/24
                                info.State = "dontsync";    // 注: borrow 类型的此种 dontsync 可以理解为读者在其他地方已经还书了。在断网情况下此种动作不要计入未还书列表
                            else
                                info.State = "normalerror";
                        }

                        if (StringUtil.IsInList("auto_stop", style))
                            break;

                        WpfClientInfo.WriteErrorLog($"请求失败。action:{action},PII:{entity.PII}, 错误信息:{strError}, 错误码:{error_code.ToString()}");

#if REMOVED
                        entity.SetError($"{action_name}操作失败: {strError}", "red");
#endif
                        continue;
                    }

                    if (action == "borrow")
                    {
                        if (borrow_info.Overflows != null && borrow_info.Overflows.Length > 0)
                        {
                            // 界面警告
                            // TODO: 可以考虑归入 overflows 单独语音警告处理。语音要简洁。详细原因可出现在文字警告中
                            // warnings.Add($"册 '{title}' (借书操作发生溢出，请于当日内还书): {string.Join("; ", borrow_info.Overflows)}");

                            // TODO: 详细原因文字可否用稍弱的字体效果来显示？
                            messageItem.ErrorInfo = $"借书操作超越许可，请将本册放回书柜。详细原因： {string.Join("; ", borrow_info.Overflows)}";
                            messageItem.ResultType = "warning";
                            messageItem.ErrorCode = "overflow";
                            // 写入错误日志
                            WpfClientInfo.WriteInfoLog($"读者 {info.Operator.PatronName} {info.Operator.PatronBarcode} 借阅 '{title}' 时发生超越许可: {strError}");
#if REMOVED
                            entity.SetError(null);
#endif
                            info.SyncErrorCode = "overflow";
                            {
                                if (string.IsNullOrEmpty(info.SyncErrorInfo) == false)
                                    info.SyncErrorInfo += "; ";
                                info.SyncErrorInfo += $"借书超额，请将本册放回书柜。详细原因： {string.Join("; ", borrow_info.Overflows)}";
                            }
                        }
                    }

                    //if (action == "borrow")
                    //    borrows.Add(title);
                    //if (action == "return")
                    //    returns.Add(title);

                    /*
                    // borrow 操作，API 之后才修改 EAS
                    // 注: 如果 API 成功但修改 EAS 动作失败(可能由于读者从读卡器上过早拿走图书导致)，读者会无法把本册图书拿出门禁。遇到此种情况，读者回来补充修改 EAS 一次即可
                    if (action == "borrow")
                    {
                        var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                        if (result.Value == -1)
                        {
                            entity.SetError($"虽然{action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}", "yellow");
                            errors.Add($"册 '{entity.PII}' {action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}");
                        }
                    }
                    */

                    // 刷新显示
                    {
                        if (item_records?.Length > 0)
                        {
                            // TODO: 这里更新 entity 后，那些克隆的 entity 何时更新呢？可否现在存入缓存备用?
                            string entity_xml = item_records[0];
                            entity.SetData(entity.ItemRecPath, entity_xml);
                            // 2020/4/13
                            l_UpdateEntityXml("all", entity.UID, entity_xml);

                            // 2020/4/26
                            // result.Value
                            //      0   没有找到记录。没有发生更新
                            //      1   成功更新
                            var result = await LibraryChannelUtil.UpdateEntityXmlAsync(entity.PII,
                                entity_xml,
                                null);

                            updates.Add(entity);
                        }

                        //if (entity.Error != null)
                        //    continue;

#if REMOVED
                        string message = $"{action_name}成功";
                        if (lRet == 1 && string.IsNullOrEmpty(strError) == false)
                            message = strError;
                        entity.SetError(message,
                            lRet == 1 ? "yellow" : "green");
#endif

                        // TODO: 刷新读者信息显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                func_setProgress?.Invoke(-1, -1, -1, "处理完成");   // hide progress bar

                {
                    /*
                     * 
        ERROR dp2SSL 2020-03-05 16:49:47,472 - 重试专用线程出现异常: Type: System.Threading.Tasks.TaskCanceledException
        Message: 已取消一个任务。
        Stack:
        在 System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
        在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
        在 System.Windows.Threading.DispatcherOperation.Wait(TimeSpan timeout)
        在 System.Windows.Threading.Dispatcher.InvokeImpl(DispatcherOperation operation, CancellationToken cancellationToken, TimeSpan timeout)
        在 System.Windows.Threading.Dispatcher.Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken, TimeSpan timeout)
        在 System.Windows.Threading.Dispatcher.Invoke(Action callback)
        在 dp2SSL.DoorItem.DisplayCount(List`1 entities, List`1 adds, List`1 removes, List`1 errors, List`1 _doors)
        在 dp2SSL.ShelfData.RefreshCount()
        在 dp2SSL.ShelfData.SubmitCheckInOut(Delegate_setProgress func_setProgress, IReadOnlyCollection`1 actions)
        在 dp2SSL.ShelfData.<>c__DisplayClass119_0.<StartRetryTask>b__1()                     * 
                     * */
                    // 重新装载读者信息和显示
                    try
                    {
                        // ShelfData.RefreshCount();
                        DoorItem.RefreshEntity(updates, ShelfData.Doors);
                        // App.CurrentApp.Speak(speak);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"SubmitChechInOut() 中的 RefreshCount() 出现异常: {ExceptionUtil.GetDebugText(ex)}。为了避免破坏流程，这里截获了异常，让后续处理正常进行");
                    }
                }

                // TODO: 遇到通讯出错的请求，是否放入一个永久保存的数据结构里面，自动在稍后进行重试请求？
                // 把处理过的移走
                lock (_syncRoot_actions)
                {
                    foreach (var info in actions)
                    {
                        _actions.Remove(info);
                    }
                }

                return new SubmitResult
                {
                    Value = 1,
                    MessageDocument = doc,
                    // RetryActions = retry_actions,
                    ProcessedActions = processed,
                    ErrorActions = error_actions,
                };
            }
            finally
            {
                // _limit.Release();
            }
        }

        public static NormalResult SetEAS(string uid, string antenna, bool enable)
        {
            try
            {
                if (uint.TryParse(antenna, out uint antenna_id) == false)
                    antenna_id = 0;
                var result = RfidManager.SetEAS($"{uid}", antenna_id, enable);
                if (result.Value != -1)
                {
#if OLD_TAGCHANGED

                    TagList.SetEasData(uid, enable);
#else
                    NewTagList.SetEasData(uid, enable);
#endif
                }
                return result;
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        /*
        static Operator OperatorFromRequest(RequestItem request)
        {
            if (request.PatronName == null
                && request.PatronBarcode == null)
                return null;
            return new Operator
            {
                PatronName = request.PatronName,
                PatronBarcode = request.PatronBarcode,
            };
        }

        static Entity EntityFromRequest(RequestItem request)
        {
            return new Entity
            {
                UID = request.UID,
                ReaderName = request.ReaderName,
                Antenna = request.Antenna,
                PII = request.PII,
                ItemRecPath = request.ItemRecPath,
                Title = request.Title,
                Location = request.ItemLocation,
                CurrentLocation = request.ItemCurrentLocation,
                ShelfNo = request.ShelfNo,
                State = request.State
            };
        }
        */

#if NO
        // 从外部存储中装载以前遗留的 Actions
        public static void LoadRetryActions()
        {
            using (var context = new MyContext())
            {
                context.Database.EnsureCreated();
                var items = context.Requests.ToList();
                AddRetryActions(FromRequests(items));

                WpfClientInfo.WriteInfoLog($"从本地数据库装载 RetryActions 成功。内容如下：\r\n{ActionInfo.ToString(_retryActions)}");
            }
        }

        public static void SaveRetryActions()
        {
            try
            {
                using (var context = new MyContext())
                {
                    // context.Database.EnsureDeleted();
                    // context.Database.EnsureCreated();

                    context.Database.EnsureCreated();
                    {
                        var allRec = context.Requests;
                        context.Requests.RemoveRange(allRec);
                        context.SaveChanges();
                    }

                    lock (_syncRoot_retryActions)
                    {
                        context.Requests.AddRange(FromActions(_retryActions));
                    }
                    context.SaveChanges();

                    WpfClientInfo.WriteInfoLog($"RetryActions 保存到本地数据库成功。内容如下：\r\n{ActionInfo.ToString(_retryActions)}");
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SaveRetryActions() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

#endif






#if NO
        // 启动重试任务。此任务长期在后台运行
        public static void StartRetryTask()
        {
            if (_retryTask != null)
                return;

            CancellationToken token = _cancel.Token;

            token.Register(() =>
            {
                _eventRetry.Set();
            });

            // 启动重试专用线程
            _retryTask = Task.Factory.StartNew(() =>
                {
                    WpfClientInfo.WriteInfoLog("重试专用线程开始");
                    try
                    {
                        while (token.IsCancellationRequested == false)
                        {
                            // TODO: 无论是整体退出，还是需要激活，都需要能中断 Delay
                            // Task.Delay(TimeSpan.FromSeconds(10)).Wait(token);
                            _eventRetry.WaitOne(TimeSpan.FromSeconds(10));
                            token.ThrowIfCancellationRequested();

                            List<ActionInfo> actions = null;
                            lock (_syncRoot_retryActions)
                            {
                                actions = new List<ActionInfo>(_retryActions);
                            }

                            if (actions.Count == 0)
                                continue;

                            // 准备对话框
                            SubmitWindow progress = PageMenu.PageShelf?.OpenProgressWindow();

                            var result = SubmitCheckInOut(
                            (min, max, value, text) =>
                            {
                                if (progress != null)
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        if (min == -1 && max == -1 && value == -1)
                                            progress.ProgressBar.Visibility = Visibility.Collapsed;
                                        else
                                            progress.ProgressBar.Visibility = Visibility.Visible;

                                        if (text != null)
                                            progress.TitleText = text;

                                        if (min != -1)
                                            progress.ProgressBar.Minimum = min;
                                        if (max != -1)
                                            progress.ProgressBar.Maximum = max;
                                        if (value != -1)
                                            progress.ProgressBar.Value = value;
                                    }));
                                }
                            },
                            actions);

                            // 将 submit 情况写入日志备查
                            WpfClientInfo.WriteInfoLog($"重试提交请求:\r\n{ActionInfo.ToString(actions)}\r\n返回结果:{result.ToString()}");

                            List<ActionInfo> processed = new List<ActionInfo>();
                            if (result.RetryActions != null)
                            {
                                foreach (var action in actions)
                                {
                                    if (result.RetryActions.IndexOf(action) == -1)
                                        processed.Add(action);
                                }
                            }

                            // TODO: 保存到数据库。这样不怕中途断电或者异常退出

                            // 把处理掉的 ActionInfo 对象移走
                            lock (_syncRoot_retryActions)
                            {
                                foreach (var action in processed)
                                {
                                    _retryActions.Remove(action);
                                }

                                RefreshRetryInfo();
                            }

                            // 把执行结果显示到对话框内
                            // 全部事项都重试失败的时候不需要显示
                            if (processed.Count > 0 && progress != null)
                            {
                                if (result.Value == -1)
                                    progress?.PushContent(result.ErrorInfo, "red");
                                else if (result.Value == 1 && result.MessageDocument != null)
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        progress?.PushContent(result.MessageDocument);
                                    }));
                                }

                                // 显示出来
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    progress?.ShowContent();
                                }));
                            }
                        }
                        _retryTask = null;

                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"重试专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                    finally
                    {
                        WpfClientInfo.WriteInfoLog("重试专用线程结束");
                    }
                },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

#endif

        /*
        public static void AddRetryActions(List<ActionInfo> actions)
        {
            lock (_syncRoot_retryActions)
            {
                _retryActions.AddRange(actions);
                RefreshRetryInfo();
            }
        }
        */

#if NO
        public static void ClearRetryActions()
        {
            lock (_syncRoot_retryActions)
            {
                _retryActions.Clear();
                RefreshRetryInfo();
            }
        }
#endif

        /*
    public static int RetryActionsCount
    {
        get
        {
            lock (_syncRoot_retryActions)
            {
                return _retryActions.Count;
            }
        }
    }
    */


#if NO
        // 把动作写入本地操作日志
        // parameters:
        //      initial 是否为书柜启动时候的初始化操作
        public static async Task SaveOperations(List<ActionInfo> actions,
            bool initial)
        {
            try
            {
                using (var context = new MyContext())
                {
                    foreach (var action in actions)
                    {
                        var operation = FromAction(action, initial);
                        context.Operations.Add(operation);
                    }
                    int count = await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO: 出现此错误，要把应用挂起，显示警告请管理员介入处理
                WpfClientInfo.WriteErrorLog($"SaveOperations() 出现异常：{ExceptionUtil.GetDebugText(ex)}");
                throw ex;
            }
        }

        // TODO: 省略记载 transfer 操作？但记载工作人员典藏移交的操作
        static Operation FromAction(ActionInfo action, bool initial)
        {
            Operation result = new Operation();
            if (action.Action == "borrow")
                result.Action = "checkout";
            else if (action.Action == "return")
            {
                if (initial)
                    result.Action = "inventory";
                else
                    result.Action = "checkin";
            }
            else if (action.Action == "transfer")
                result.Action = "transfer";
            else
                result.Action = "~" + action.Action;

            if (initial)
                result.Condition = "initial";   // 表示这是书柜启动时候的初始化操作

            result.UID = action.Entity?.UID;
            result.PII = action.Entity?.PII;
            result.Antenna = action.Entity?.Antenna;
            result.Title = action.Entity?.Title;
            result.Operator = GetOperatorString(action.Operator);
            result.OperTime = DateTime.Now;

            if (action.Action == "transfer")
            {
                result.Parameter = JsonConvert.SerializeObject(new
                {
                    batchNo = action.BatchNo,
                    location = action.Location,
                    currentShelfNo = action.CurrentShelfNo,
                    direction = action.TransferDirection,
                });
            }
            return result;

            string GetOperatorString(Operator person)
            {
                if (person == null)
                    return null;
                return JsonConvert.SerializeObject(new
                {
                    name = person.PatronName,
                    barcode = person.PatronBarcode,
                });
            }
        }

#endif

#if REMOVED
        #region 门命令延迟执行

        // 门命令(延迟执行)队列。开门时放一个命令进入队列。等得到门开信号的时候再取出这个命令
        static List<CommandItem> _commandQueue = new List<CommandItem>();
        static object _syncRoot_commandQueue = new object();

        public static void PushCommand(DoorItem door,
            Operator person,
            long heartbeat)
        {
            CommandItem command = new CommandItem
            {
                Command = "setOwner",
                Door = door,
                Parameter = person,
                Heartbeat = heartbeat,
            };

            lock (_syncRoot_commandQueue)
            {
                if (_commandQueue.Count > 1000)
                {
                    _commandQueue.Clear();
                    WpfClientInfo.WriteErrorLog("_commandQueue 元素个数超过 1000。为保证安全自动清除了全部元素");
                }
                _commandQueue.Add(command);
                WpfClientInfo.WriteInfoLog($"PushCommand {command.ToString()}");
            }
        }

        public static CommandItem PopCommand(DoorItem door, string comment = "")
        {
            lock (_syncRoot_commandQueue)
            {
                CommandItem result = null;
                foreach (var command in _commandQueue)
                {
                    if (command.Door == door)
                    {
                        result = command;
                        break;
                    }
                }

                if (result == null)
                {
                    WpfClientInfo.WriteInfoLog($"PopCommand (door={door.Name} 时间={RfidManager.LockHeartbeat}) ({comment}) not found command");
                    return null;
                }
                _commandQueue.Remove(result);
                WpfClientInfo.WriteInfoLog($"PopCommand (door={door.Name} 时间={RfidManager.LockHeartbeat}) ({comment}) {result.ToString()}");
                return result;
            }
        }

        // 检查命令队列。观察是否有超过合理时间的命令滞留，如果有就返回它们
        public static List<CommandItem> CheckCommands(long currentHeartbeat)
        {
            List<CommandItem> results = new List<CommandItem>();
            lock (_syncRoot_commandQueue)
            {
                foreach (var command in _commandQueue)
                {
                    // 和当初 push 时候间隔了多个心跳
                    if (currentHeartbeat >= command.Heartbeat + 1)  // +1
                    {
                        results.Add(command);
                    }
                }
            }

            return results;
        }

        public static string CommandToString()
        {
            lock (_syncRoot_commandQueue)
            {
                StringBuilder text = new StringBuilder();
                int i = 0;
                foreach (var command in _commandQueue)
                {
                    text.AppendLine($"{i + 1}) {command.ToString()}");
                    i++;
                }
                return text.ToString();
            }
        }


        #endregion
#endif
    }

    // 操作者
    public class Operator
    {
        public string PatronName { get; set; }
        public string PatronBarcode { get; set; }

        public Operator Clone()
        {
            Operator dup = new Operator();
            dup.PatronName = this.PatronName;
            dup.PatronBarcode = this.PatronBarcode;
            return dup;
        }

        public override string ToString()
        {
            return $"PatronName:{PatronName}, PatronBarcode:{PatronBarcode}";
        }

        public string GetDisplayString()
        {
            if (string.IsNullOrEmpty(PatronName) == false)
                return PatronName;
            return PatronBarcode;
        }

        public static bool IsPatronBarcodeWorker(string patronBarcode)
        {
            if (string.IsNullOrEmpty(patronBarcode))
                return false;
            return patronBarcode.StartsWith("~");
        }

        public static string BuildWorkerAccountName(string text)
        {
            return text.Substring(1);
        }

        public bool IsWorker
        {
            get
            {
                return IsPatronBarcodeWorker(this.PatronBarcode);
            }
        }

        public string GetWorkerAccountName()
        {
            if (this.IsWorker == true)
                return BuildWorkerAccountName(this.PatronBarcode);
            return "";
        }
    }

    public class ActionInfo
    {
        public Operator Operator { get; set; }  // 提起请求的读者
        public DateTime OperTime { get; set; }  // 首次操作的时间
        public Entity Entity { get; set; }
        public string Action { get; set; }  // borrow/return/transfer
        public string TransferDirection { get; set; } // in/out 典藏移交的方向
        public string Location { get; set; }    // 所有者馆藏地。transfer 动作会用到
        public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
        public string BatchNo { get; set; } // 批次号。transfer 动作会用到。建议可以用当前用户名加上日期构成

        // 状态 
        // sync/dontsync/commerror/normalerror/空
        // 对应于: 同步成功/不再同步/通讯出错/一般出错/从未同步过
        public string State { get; set; }
        public string SyncErrorInfo { get; set; }   // 最近一次同步操作的报错信息
        public string SyncErrorCode { get; set; }   // 最近一次同步操作的错误码
        public int SyncCount { get; set; } // 已经进行过的同步重试次数
        public int ID { get; set; } // 日志数据库中对应的记录 ID

        public DateTime SyncOperTime { get; set; }  // 最后一次同步操作的时间
        public string ActionString { get; set; }    // 存储 BorrowInfo 或者 ReturnInfo 的 JSON 化字符串

        public override string ToString()
        {
            return $"Action={Action},TransferDirection={TransferDirection},Location={Location},CurrentShelfNo={CurrentShelfNo},Operator=[{Operator}],Entity=[{ToString(this.Entity)}],BatchNo={BatchNo}";
        }

        static string ToString(Entity entity)
        {
            return $"PII:{entity.PII},UID:{entity.UID},Title:{entity.Title},ItemRecPath:{entity.ItemRecPath},ReaderName:{entity.ReaderName},Antenna:{entity.Antenna}";
        }

        public static string ToString(List<ActionInfo> actions)
        {
            if (actions == null)
                return "(null)";
            StringBuilder text = new StringBuilder();
            text.AppendLine($"ActionInfo 对象共 {actions.Count} 个:");
            int i = 0;
            foreach (var action in actions)
            {
                text.AppendLine($"{(i + 1)}) {action.ToString()}");
                i++;
            }

            return text.ToString();
        }
    }



    public class SubmitResult : NormalResult
    {
        // [out]
        public MessageDocument MessageDocument { get; set; }

        // [out]
        // 发生了错误，但不需要后面重试提交的 ActionInfo 对象集合
        public List<ActionInfo> ErrorActions { get; set; }

        // [out]
        // 处理过的 ActionInfo 对象集合。这里面包含成功的，和失败的
        public List<ActionInfo> ProcessedActions { get; set; }


        // [out]
        // 发生了错误，需要后面重试提交的 ActionInfo 对象集合
        // public List<ActionInfo> RetryActions { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(base.ToString());
            if (ErrorActions != null && ErrorActions.Count > 0)
            {
                text.AppendLine($"发生了错误(但不需要重试)的 ActionInfo:({ErrorActions.Count})");
                text.AppendLine(ActionInfo.ToString(ErrorActions));
            }
            /*
            if (RetryActions != null && RetryActions.Count > 0)
            {
                text.AppendLine($"需要重试的 ActionInfo:({RetryActions.Count})");
                text.AppendLine(ActionInfo.ToString(RetryActions));
            }
            */
            if (ProcessedActions != null && ProcessedActions.Count > 0)
            {
                text.AppendLine($"处理过的 ActionInfo:({ProcessedActions.Count})");
                text.AppendLine(ActionInfo.ToString(ProcessedActions));
            }
            return text.ToString();
        }
    }

    public class AntennaList
    {
        public string ReaderName { get; set; }
        public List<int> Antennas { get; set; }
    }

    public class CommandItem
    {
        public DoorItem Door { get; set; }
        public string Command { get; set; }
        public object Parameter { get; set; }
        public long Heartbeat { get; set; }

        // TODO: 是否增加一个时间成员，用以测算 item 在 queue 中的留存时间？时间太长了说明不正常，需要排除故障

        public override string ToString()
        {
            return $"DoorName:{Door.Name}, Command:{Command}, Parameter:{Parameter}, Heartbeat:{Heartbeat}";
        }
    }
}
