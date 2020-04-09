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

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using static dp2SSL.LibraryChannelUtil;
using Microsoft.VisualStudio.Threading;

namespace dp2SSL
{
    /// <summary>
    /// 智能书架要用到的数据
    /// </summary>
    public static class ShelfData
    {
#if DOOR_MONITOR
        public static DoorMonitor DoorMonitor = null;
#endif

        public static CancellationToken CancelToken
        {
            get
            {
                return _cancel.Token;
            }
        }

        static CancellationTokenSource _cancel = new CancellationTokenSource();

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
                                App.CurrentApp.Speak($"{result.LockName} 打开");
                            else
                                App.CurrentApp.Speak($"{result.LockName} 关闭");
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

        // exception:
        //      可能会抛出异常
        public static void InitialShelf()
        {
            ShelfData.InitialDoors();

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

        // 单独对一个门关联的 RFID 标签进行一次 inventory，确保此前的标签变化情况没有被遗漏
        public static NormalResult RefreshInventory(DoorItem door)
        {
            // 获得和一个门相关的 readernamelist
            var list = GetReaderNameList(new List<DoorItem> { door }, null);
            string style = $"dont_delay";   // 确保 inventory 并立即返回

            // StringBuilder debugInfo = new StringBuilder();
            var result = RfidManager.CallListTags(list, style);
            // WpfClientInfo.WriteErrorLog($"RefreshInventory() list={list}, style={style}, result={result.ToString()}");
            try
            {
                RfidManager.TriggerListTagsEvent(list, result, true);
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
            public string ShelfNo { get; set; }
            public Operator Operator { get; set; }
        }

        public class SaveActionResult : NormalResult
        {
            public List<OperationInfo> Operations { get; set; }
            public List<ActionInfo> Actions { get; set; }
        }

        // 将暂存的信息保存为 Action。但并不立即提交
        // parameters:
        //      patronBarcode   读者证条码号。如果为 "*"，表示希望针对全部读者的都提交
        public static SaveActionResult SaveActions(
            // string patronBarcode,
            Delegate_getOperator func_getOperator)
        {
            List<OperationInfo> infos = new List<OperationInfo>();

            lock (_syncRoot_actions)
            {
                List<ActionInfo> actions = new List<ActionInfo>();
                List<Entity> processed = new List<Entity>();
                foreach (var entity in ShelfData.Adds)
                {
                    if (ShelfData.BelongToNormal(entity) == false)
                        continue;
                    var person = func_getOperator?.Invoke(entity);
                    if (person == null)
                        continue;
                    actions.Add(new ActionInfo
                    {
                        Entity = entity,
                        Action = "return",
                        Operator = person,
                    });
                    // 没有更新的，才进行一次 transfer。更新的留在后面专门做
                    // “更新”的意思是从这个门移动到了另外一个门
                    if (ShelfData.Find(ShelfData.Changes, (o) => o.UID == entity.UID).Count == 0)
                    {
                        string location = "";
                        // 工作人员身份，还可能要进行馆藏位置向内转移
                        if (person.IsWorker == true)
                        {
                            location = GetLocationPart(ShelfData.GetShelfNo(entity));
                        }
                        actions.Add(new ActionInfo
                        {
                            Entity = entity,
                            Action = "transfer",
                            TransferDirection = "in",
                            Location = location,
                            CurrentShelfNo = ShelfData.GetShelfNo(entity),
                            Operator = person
                        });
                    }

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
                            operation.Operation = "调入";
                        }
                        infos.Add(operation);
                    }

                    processed.Add(entity);

                    // 2020/4/2
                    ShelfData.Add("all", entity);

                    // 2020/4/2
                    // 还书操作前先尝试修改 EAS
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

                foreach (var entity in ShelfData.Changes)
                {
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
                        Entity = entity,
                        Action = "transfer",
                        TransferDirection = "in",
                        Location = location,
                        CurrentShelfNo = ShelfData.GetShelfNo(entity),
                        Operator = person
                    });

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

                    processed.Add(entity);
                }

                foreach (var entity in ShelfData.Removes)
                {
                    if (ShelfData.BelongToNormal(entity) == false)
                        continue;
                    var person = func_getOperator?.Invoke(entity);
                    if (person == null)
                        continue;
                    if (person.IsWorker == false)
                    {
                        // 只有读者身份才进行借阅操作
                        actions.Add(new ActionInfo
                        {
                            Entity = entity,
                            Action = "borrow",
                            Operator = person
                        });
                    }

                    //
                    if (person.IsWorker == true)
                    {
                        // 工作人员身份，还可能要进行馆藏位置向外转移
                        string location = "%checkout_location%";
                        actions.Add(new ActionInfo
                        {
                            Entity = entity,
                            Action = "transfer",
                            TransferDirection = "out",
                            Location = location,
                            // 注: ShelfNo 成员不使用。意在保持册记录中 currentLocation 元素不变
                            Operator = person
                        });
                    }

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
                            operation.Operation = "调出";
                        }
                        infos.Add(operation);
                    }

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
                    ShelfData.Remove("adds", processed);
                    ShelfData.Remove("removes", processed);
                    ShelfData.Remove("changes", processed);
                }

                // 2020/4/2
                ShelfData.RefreshCount();

                if (actions.Count == 0)
                    return new SaveActionResult
                    {
                        Actions = actions,
                        Operations = infos
                    };  // 没有必要处理
                ShelfData.PushActions(actions);
                return new SaveActionResult
                {
                    Actions = actions,
                    Operations = infos
                };
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

        public delegate void Delegate_removeAction(ActionInfo action);

        // 询问典藏移交的一些条件参数
        // parameters:
        //      actions     在本函数处理过程中此集合内的对象可能被修改，集合元素可能被移除
        public static void AskLocationTransfer(List<ActionInfo> actions,
            Delegate_removeAction func_removeAction)
        {
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
                    App.CurrentApp.Speak("典藏移交");
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
                        AskTransferWindow dialog = new AskTransferWindow();
                        dialog.TitleText = "向内移交";
                        dialog.SetBooks(collection);
                        dialog.Text = $"是否要针对以上放入书柜的图书进行典藏移交？";
                        dialog.Owner = App.CurrentApp.MainWindow;
                        dialog.BatchNo = batchNo;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        App.SetSize(dialog, "tall");

                        //dialog.Width = Math.Min(700, App.CurrentApp.MainWindow.ActualWidth);
                        //dialog.Height = Math.Min(900, App.CurrentApp.MainWindow.ActualHeight);
                        dialog.ShowDialog();
                        selection = dialog.Selection;
                        batchNo = dialog.BatchNo;
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
                    App.CurrentApp.Speak("典藏移交");

                    string batchNo = transferouts[0].Operator.GetWorkerAccountName() + "_" + DateTime.Now.ToShortDateString();

                    // TODO: 这个列表是否在程序初始化的时候得到?
                    var result = LibraryChannelUtil.GetLocationList();
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
                        AskTransferWindow dialog = new AskTransferWindow();
                        dialog.TitleText = "向外移交";
                        dialog.Mode = "out";
                        dialog.SetBooks(collection);
                        dialog.Text = $"是否要针对以上拿出书柜的图书进行典藏移交？";
                        dialog.target.ItemsSource = result.List;
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
                    }));

                    // 把 transfer 动作里的 Location 成员清除
                    if (selection == "not")
                    {
                        foreach (var action in transferouts)
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
                        foreach (var action in transferouts)
                        {
                            action.Location = target;
                            action.BatchNo = batchNo;
                        }
                    }
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

        public static IReadOnlyCollection<Entity> All
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

        public static IReadOnlyCollection<Entity> Adds
        {
            get
            {
                lock (_syncRoot_all)
                {
                    return new List<Entity>(_adds);
                }
            }
        }

        public static IReadOnlyCollection<Entity> Removes
        {
            get
            {
                lock (_syncRoot_all)
                {
                    return new List<Entity>(_removes);
                }
            }
        }

        public static IReadOnlyCollection<Entity> Changes
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
            _doors = DoorItem.BuildItems(_shelfCfgDom);
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

                var result = RfidManager.CallListTags(list, style);
                try
                {
                    RfidManager.TriggerListTagsEvent(list, result, true);
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

                i++;
            }

            if (func_cancelled() == true)
                return new InitialShelfResult();

            WpfClientInfo.WriteInfoLog("开始填充图书队列");
            func_display("正在填充图书队列 ...");

            List<string> warnings = new List<string>();

            List<Entity> all = new List<Entity>();
            // lock (_syncRoot_all)
            {
                // _all.Clear();

                var books = TagList.Books;
                WpfClientInfo.WriteErrorLog($"books count={books.Count}, ReaderNameList={RfidManager.ReaderNameList}(注：此时门应该都是关闭的，图书读卡器应该是停止盘点状态)");
                foreach (var tag in books)
                {
                    if (func_cancelled() == true)
                        return new InitialShelfResult();

                    WpfClientInfo.WriteErrorLog($" tag={tag.ToString()}");

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
                        var entity = NewEntity(tag);

                        func_display($"正在填充图书队列 ({GetPiiString(entity)})...");

                        all.Add(entity);

                        if (string.IsNullOrEmpty(entity.Error) == false)
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
                }

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
            }

            // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
            // TODO: 只刷新指定门的数字即可
            RefreshCount();

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
        static Entity NewEntity(TagAndData tag)
        {
            var result = new Entity
            {
                UID = tag.OneTag.UID,
                ReaderName = tag.OneTag.ReaderName,
                Antenna = tag.OneTag.AntennaID.ToString(),
                TagInfo = tag.OneTag.TagInfo,
            };

            // Exception:
            //      可能会抛出异常 ArgumentException 
            EntityCollection.SetPII(result);
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
        public static void RefreshCount()
        {
            lock (_syncRoot_all)
            {
                List<Entity> errors = GetErrors(_all, _adds, _removes);
                DoorItem.DisplayCount(_all, _adds, _removes, errors, Doors);
            }
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

        static List<Entity> Find(string name, TagAndData tag)
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
            if (Add(name, list) > 0)
                return true;
            return false;
        }

        // return:
        //      返回实际添加的个数
        internal static int Add(string name,
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
            Remove(name, list);
        }

        internal static void Remove(string name,
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
                entities.Add(NewEntity(tag));
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

        // 更新 Entity 信息
        static bool Update(string name, TagAndData tag)
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

        // 故意选择用到的天线编号加一的天线(用 ListTags() 实现)
        public static NormalResult SelectAntenna()
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
                var result = RfidManager.CallListTags($"{list.ReaderName}:{first_antenna}", "");
                if (result.Value == -1)
                    errors.Add($"CallListTags() 出错: {result.ErrorInfo}");
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
                _tagAdded = true;
            }

            if (e.UpdateBooks != null)
                tags.AddRange(e.UpdateBooks);

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

                // 拿走标签
                foreach (var tag in e.RemoveBooks)
                {
                    if (tag.OneTag.TagInfo == null)
                        continue;

                    if (tag.Type == "patron")
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

        static int[] tones = new int[] { 523, 659, 783 };
        /*
         *  C4: 261 330 392
            C5: 523 659 783
         * */
        public static void Sound(int tone, int count, string text)
        {
            _ = Task.Run(() =>
            {
                for (int i = 0; i < count; i++)
                    System.Console.Beep(tones[tone], 500);
                if (string.IsNullOrEmpty(text) == false)
                    App.CurrentApp.SpeakSequence(text); // 不打断前面的说话
            });
        }

        public class FillBookFieldsResult : NormalResult
        {
            public List<string> Errors { get; set; }
        }

        public static async Task<FillBookFieldsResult> FillBookFieldsAsync(// BaseChannel<IRfid> channel,
    IReadOnlyCollection<Entity> entities,
    CancellationToken token,
    bool refreshCount = true)
        {
            // int error_count = 0;
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
                if (string.IsNullOrEmpty(entity.Title)
                    && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                {
                    GetEntityDataResult result = await GetEntityDataAsync(entity.PII);
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
                        App.CurrentApp.SpeakSequence(error);
                        entity.SetError(result.ErrorInfo);
                        // 2020/4/8
                        if (result.ErrorCode == "RequestError" || result.ErrorCode == "RequestTimeOut")
                        {
                            // 如果是通讯失败导致的出错，应该有办法进行重试获取
                            entity.FillFinished = false;
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

                entity.SetError(null);
                entity.FillFinished = true;
            }

            if (token.IsCancellationRequested)
                return new FillBookFieldsResult
                {
                    Value = -1,
                    ErrorInfo = "中断",
                    ErrorCode = "cancelled"
                };

            if (refreshCount)
                ShelfData.RefreshCount();

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
        public static SubmitResult SubmitCheckInOut(
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

                    long lRet = 0;
                    ErrorCode error_code = ErrorCode.NoError;

                    string strError = "";
                    string[] item_records = null;
                    string[] biblio_records = null;
                    BorrowInfo borrow_info = null;

                    string strUserName = info.Operator?.GetWorkerAccountName();

                    int nRedoCount = 0;
                REDO:
                    entity.Waiting = true;
                    LibraryChannel channel = App.CurrentApp.GetChannel(strUserName);
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    try
                    {
                        if (action == "borrow" || action == "renew")
                        {
                            // TODO: 智能书柜要求强制借书。如果册操作前处在被其他读者借阅状态，要自动先还书再进行借书

                            lRet = channel.Borrow(null,
                                action == "renew",
                                info.Operator.PatronBarcode,
                                entity.PII,
                                entity.ItemRecPath,
                                false,
                                null,
                                "item,reader,biblio,overflowable", // style,
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
                                "item,reader,biblio", // style,
                                "xml", // item_format_list
                                out item_records,
                                "xml",
                                out string[] reader_records,
                                "summary",
                                out biblio_records,
                                out string[] dup_path,
                                out string output_reader_barcode,
                                out ReturnInfo return_info,
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
                                $"item,biblio,{StringUtil.MakePathList(commands, ",")}", // style,
                                "xml", // item_format_list
                                out item_records,
                                "xml",
                                out string[] reader_records,
                                "summary",
                                out biblio_records,
                                out string[] dup_path,
                                out string output_reader_barcode,
                                out ReturnInfo return_info,
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
    || error_code == ErrorCode.RequestTimeOut)
    && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    processed.Add(info);

                    /*
                    // testing
                    lRet = -1;
                    strError = "testing";
                    channel.ErrorCode = ErrorCode.AccessDenied;
                    */

                    func_setProgress?.Invoke(-1, -1, ++index, null);

                    if (biblio_records != null
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
                            if (error_code == ErrorCode.ItemBarcodeNotFound)
                                info.State = "dontsync";
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
                            entity.SetData(entity.ItemRecPath, item_records[0]);
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
                // TagList.ClearTagTable(uid);
                var result = RfidManager.SetEAS($"{uid}", antenna_id, enable);
                if (result.Value != -1)
                {
                    TagList.SetEasData(uid, enable);
                    // All.SetEasData(uid, enable);
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

        // static object _syncRoot_database = new object();

        // 限制数据库操作，同一时刻只能一个函数进入
        static AsyncSemaphore _databaseLimit = new AsyncSemaphore(1);

        // sync/commerror/normalerror/空
        // 同步成功/通讯出错/一般出错/从未同步过

        // 从外部存储中装载尚未同步的 Actions
        // 注意：这些 Actions 应该先按照 PII 排序分组以后，一组一组进行处理
        public static async Task<List<ActionInfo>> LoadRetryActionsFromDatabaseAsync()
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new MyContext())
                {
                    context.Database.EnsureCreated();
                    var items = context.Requests.Where(o => o.State != "sync" && o.State != "dontsync")
                        .OrderBy(o => o.ID).ToList();
                    var actions = FromRequests(items);
                    WpfClientInfo.WriteInfoLog($"从本地数据库装载 Actions 成功。内容如下：\r\n{ActionInfo.ToString(actions)}");
                    return actions;
                }
            }
        }

        static async Task ChangeDatabaseActionStateAsync(int id, ActionInfo action)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new MyContext())
                {
                    var item = context.Requests.FirstOrDefault(o => o.ID == id);
                    item.State = action.State;
                    item.SyncErrorInfo = action.SyncErrorInfo;
                    item.SyncErrorCode = action.SyncErrorCode;
                    item.SyncCount = action.SyncCount;
                    context.SaveChanges();
                }
            }
        }

        // 把 Actions 追加保存到本地数据库
        // 当本函数执行完以后，ActionInfo 对象的 ID 有了值，和数据库记录的 ID 对应
        public static async Task SaveActionsToDatabaseAsync(List<ActionInfo> actions)
        {
            try
            {
                using (var releaser = await _databaseLimit.EnterAsync())
                {
                    using (var context = new MyContext())
                    {
                        context.Database.EnsureCreated();

                        var requests = FromActions(actions);
                        foreach (var request in requests)
                        {
                            // 注：这样一个一个保存可以保持 ID 的严格从小到大。因为这些事项之间是有严格顺序关系的(借和还顺序不能颠倒)
                            await context.Requests.AddRangeAsync(request);
                            int nCount = await context.SaveChangesAsync();
                        }

                        Debug.Assert(requests.Count == actions.Count, "");
                        // 刷新 ActionInfo 对象的 ID
                        for (int i = 0; i < requests.Count; i++)
                        {
                            actions[i].ID = requests[i].ID;
                        }

                        WpfClientInfo.WriteInfoLog($"Actions 保存到本地数据库成功。内容如下：\r\n{ActionInfo.ToString(actions)}");
                    }
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SaveActionsToDatabase() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                throw ex;
            }
        }

        // 从操作日志数据库中把一些需要重试的事项移走
        // 原理：当首次初始化以后，已经初始化确认在书架内的图书，已经进行了还书操作，那么此前累积的需要重试借书或者还书的同步请求，都可以不执行了。这样不会造成图书丢失。但可能会丢掉一些中间操作信息
        // 改进：可以不删除，但把这些事项的状态标记为 “放弃重试”
        public static async Task RemoveRetryActionsFromDatabaseAsync(IEnumerable<string> piis)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                using (var context = new MyContext())
                {
                    context.Database.EnsureCreated();
                    foreach (var pii in piis)
                    {
                        var items = context.Requests.Where(o => o.PII == pii && o.State != "sync").ToList();
                        // context.Requests.RemoveRange(items);
                        items.ForEach(o => o.State = "dontsync");
                        context.SaveChanges();
                    }
                }
            }
        }

        static List<ActionInfo> FromRequests(List<RequestItem> requests)
        {
            List<ActionInfo> actions = new List<ActionInfo>();
            foreach (var request in requests)
            {
                ActionInfo action = new ActionInfo();
                action.Operator = request.OperatorString == null ? null :
                    JsonConvert.DeserializeObject<Operator>(request.OperatorString);
                // action.Operator = request.Operator;
                action.Entity = JsonConvert.DeserializeObject<Entity>(request.EntityString);
                // action.Entity = request.Entity;
                action.Action = request.Action;
                action.TransferDirection = request.TransferDirection;
                action.Location = request.Location;
                action.CurrentShelfNo = request.CurrentShelfNo;
                action.BatchNo = request.BatchNo;
                action.ID = request.ID;
                action.SyncCount = request.SyncCount;
                action.State = request.State;
                action.SyncErrorInfo = request.SyncErrorInfo;
                action.SyncErrorCode = request.SyncErrorCode;
                action.OperTime = request.OperTime;
                actions.Add(action);
            }

            return actions;
        }

        static List<RequestItem> FromActions(List<ActionInfo> actions)
        {
            List<RequestItem> requests = new List<RequestItem>();
            foreach (var action in actions)
            {
                RequestItem request = new RequestItem();
                request.PII = action.Entity?.PII;
                // TODO: 若 PII 为空，写入 UID?
                request.OperatorString = action.Operator == null ? null : JsonConvert.SerializeObject(action.Operator);
                request.EntityString = JsonConvert.SerializeObject(action.Entity);
                /*
                request.Operator = action.Operator.Clone();
                request.Entity = action.Entity.Clone();
                */
                request.Action = action.Action;
                request.TransferDirection = action.TransferDirection;
                request.Location = action.Location;
                request.CurrentShelfNo = action.CurrentShelfNo;
                request.BatchNo = action.BatchNo;
                request.SyncCount = action.SyncCount;
                request.State = action.State;
                request.SyncErrorInfo = action.SyncErrorInfo;
                request.SyncErrorCode = action.SyncErrorCode;
                if (action.OperTime == DateTime.MinValue)
                    request.OperTime = DateTime.Now;
                else
                    request.OperTime = action.OperTime;
                requests.Add(request);
            }

            return requests;
        }

        static Task _retryTask = null;

        /*
        static List<ActionInfo> _retryActions = new List<ActionInfo>();
        static object _syncRoot_retryActions = new object();
        */

        static AutoResetEvent _eventRetry = new AutoResetEvent(false);

        public static void ActivateRetry()
        {
            _eventRetry.Set();
        }

        /*
        // 从 _retryActions 中找到匹配的元素加以删除
        public static void RemoveFromRetryActions(List<Entity> entities)
        {
            lock (_syncRoot_actions)
            {
                List<ActionInfo> matched = new List<ActionInfo>();
                foreach (var action in _retryActions)
                {
                    string pii = action.Entity.PII;
                    var found = entities.Find((a) =>
                    {
                        if (a.PII == pii)
                            return true;
                        return false;
                    });
                    if (found != null)
                        matched.Add(action);
                }

                foreach (var action in matched)
                {
                    _retryActions.Remove(action);
                }
                RefreshRetryInfo();
            }
        }
        */

        public static void RefreshRetryInfo(List<ActionInfo> actions)
        {
            PageMenu.PageShelf?.SetRetryInfo(actions.Count == 0 ? "" : $"滞留:{actions.Count}");
        }

        public static bool PauseSubmit { get; set; }

        // 同步重试间隔时间
        static TimeSpan _idleLength = TimeSpan.FromSeconds(10);

        // 启动同步任务。此任务长期在后台运行
        public static void StartSyncTask()
        {
            if (_retryTask != null)
                return;

            CancellationToken token = _cancel.Token;

            token.Register(() =>
            {
                _eventRetry.Set();
            });

            // 启动重试专用线程
            _retryTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("重试专用线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // TODO: 无论是整体退出，还是需要激活，都需要能中断 Delay
                        // Task.Delay(TimeSpan.FromSeconds(10)).Wait(token);
                        _eventRetry.WaitOne(_idleLength);
                        token.ThrowIfCancellationRequested();

#if REMOVED
                        // 顺便检查和确保连接到消息服务器
                        App.CurrentApp.EnsureConnectMessageServer().Wait(token);
#endif

                        // 顺便关闭天线射频
                        if (_tagAdded)
                        {
                            _ = Task.Run(() =>
                            {
                                SelectAntenna();
                            });
                            _tagAdded = false;
                        }

                        if (PauseSubmit)
                            continue;

                        // TODO: 从本地数据库中装载需要同步的那些 Actions
                        List<ActionInfo> actions = await LoadRetryActionsFromDatabaseAsync();
                        if (actions.Count == 0)
                            continue;

                        // RefreshRetryInfo() ???

                        // 一般来说，只要 SubmitWindow 开着，就要显示请求情况结果。
                        // 特殊地，如果 SubmitWindow 没有开着，但本次至少有一个成功的请求结果了，那就专门打开 SubmitWindow 显示信息

                        int succeedCount = 0;   // 同步成功的事项数量
                        int newCount = 0;   // 首次进行同步的事项数量

                        // 排序和分组。按照分组提交给 dp2library 服务器
                        // TODO: 但进度显示不应该太细碎？应该按照总的进度来显示
                        var groups = GroupActions(actions);

                        // List<MessageItem> messages = new List<MessageItem>();

                        // 准备对话框
                        // SubmitWindow progress = PageMenu.PageShelf?.OpenProgressWindow();
                        SubmitWindow progress = PageMenu.PageShelf?.ProgressWindow;

                        int start = 0;  // 当前 group 开始的偏移
                        int total = actions.Count;
                        foreach (var group in groups)
                        {
                            int current_count = group.Count;    // 当前 group 包含的动作数量

                            var result = SubmitCheckInOut(
                            (min, max, value, text) =>
                            {
                                // 2020/4/2
                                // 修正三个值
                                if (max != -1)
                                    max = total;
                                //if (min != -1)
                                //    min += start;
                                if (value != -1)
                                    value += start;

                                if (progress != null)
                                {
                                    App.Invoke(new Action(() =>
                                    {
                                        if (min == -1 && max == -1 && value == -1
                                        && groups.IndexOf(group) == groups.Count - 1)   // 只有最后一次才隐藏进度条
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
                            group,
                            "auto_stop");

                            // TODO: 把 group 中报错的信息写入本地数据库的对应事项中

                            /*
                            // 把已经处理成功的 Action 对应在本地数据库中的事项的状态修改
                            List<ActionInfo> processed = new List<ActionInfo>();
                            if (result.RetryActions != null)
                            {
                                foreach (var action in group)
                                {
                                    if (result.RetryActions.IndexOf(action) == -1)
                                    {
                                        ChangeDatabaseActionState(action.ID, "sync");
                                        processed.Add(action);
                                    }
                                }
                            }
                            */
                            if (result.ProcessedActions != null)
                            {
                                // result.ProcessedActions.ForEach(o => { if (o.SyncCount == 0) newCount++; });

                                foreach (var action in result.ProcessedActions)
                                {
                                    if (action.State == "sync")
                                        succeedCount++;
                                    if (action.SyncCount == 1)
                                        newCount++;
                                    // sync/commerror/normalerror/空
                                    // 同步成功/通讯出错/一般出错/从未同步过
                                    await ChangeDatabaseActionStateAsync(action.ID, action);
                                }

                                MessageNotifyOverflow(result.ProcessedActions);
                            }

                            if (progress != null && progress.IsVisible)
                            {
                                // Thread.Sleep(3000);
                                // 刷新显示
                                App.Invoke(new Action(() =>
                                {
                                    progress?.Refresh(result.ProcessedActions);
                                }));
                            }

                            /*
                            if (result.MessageDocument != null)
                                messages.AddRange(result.MessageDocument.Items);
                                */
                            start += current_count;
                        }

                        // TODO: 更新每个事项的 RetryCount。如果超过 10 次，要把 State 更新为 fail

                        // 将 submit 情况写入日志备查
                        // WpfClientInfo.WriteInfoLog($"重试提交请求:\r\n{ActionInfo.ToString(actions)}\r\n返回结果:{result.ToString()}");



#if REMOVED

                        // 如果本轮有成功的请求，并且进度窗口没有打开，则补充打开进度窗口
                        if ((progress == null || progress.IsVisible == false)
                            && succeedCount > 0)
                            progress = PageMenu.PageShelf?.OpenProgressWindow();

                        // 把执行结果显示到对话框内
                        // 全部事项都重试失败的时候不需要显示
                        if (progress != null && progress.IsVisible
                            && (succeedCount > 0 || newCount > 0))
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                MessageDocument doc = new MessageDocument();
                                doc.AddRange(messages);
                                progress?.PushContent(doc);
                            }));

                            // 显示出来
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    progress?.ShowContent();
                                }));
                        }
#endif
                    }
                    _retryTask = null;

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"重试专用线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.CurrentApp?.SetError("sync", $"重试专用线程出现异常: {ex.Message}");
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

        static void MessageNotifyOverflow(List<ActionInfo> actions)
        {
            // 检查超额图书
            List<string> overflow_titles = new List<string>();
            int i = 1;
            actions.ForEach(item =>
            {
                if (item.Action == "borrow" && item.SyncErrorCode == "overflow")
                {
                    var pii = GetPiiString(item.Entity);
                    overflow_titles.Add($"{i++}) {SubmitDocument.ShortTitle(item.Entity.Title)} [{pii}]");
                }
            });
            if (overflow_titles.Count > 0)
            {
                PageShelf.TrySetMessage($"下列图书发生超额借阅：\r\n{StringUtil.MakePathList(overflow_titles, "\r\n")}");
            }
        }

        public static void CancelAll()
        {
            _cancel.Cancel();
        }

        // 对 Actions 按照 PII 进行分组
        static List<List<ActionInfo>> GroupActions(List<ActionInfo> actions)
        {
            // 按照 PII 分装
            // PII --> List<ActionInfo>
            Hashtable table = new Hashtable();
            foreach (var action in actions)
            {
                string pii = GetPiiString(action.Entity);
                List<ActionInfo> list = table[pii] as List<ActionInfo>;
                if (list == null)
                {
                    list = new List<ActionInfo>();
                    table[pii] = list;
                }
                list.Add(action);
            }

            return new List<List<ActionInfo>>(table.Values.Cast<List<ActionInfo>>());
            /*
            List<List<ActionInfo>> results = new List<List<ActionInfo>>();
            foreach(var key in table.Keys)
            {
                results.Add(table[key] as List<ActionInfo>);
            }

            return results;
            */
        }

        // 获得 PII 字符串。如果 PII 为空，会改取 UID 返回
        public static string GetPiiString(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.PII))
                return $"UID:{entity.UID}";
            return entity.PII;
        }

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
