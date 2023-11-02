// #define SENDKEY

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Serilog;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace RfidCenter
{
    public class RfidServer : MarshalByRefObject, IRfid, IDisposable
    {
        // 紧凑日志
        static CompactLog _compactLog = new CompactLog();

        // 模拟门锁
        static SimuLock _simuLock = new SimuLock(2, 24); // 2 块锁控版，每块上面控制 24 个门

        // 模拟读卡器
        static SimuReader _simuReader = new SimuReader();

        public void Dispose()
        {
#if SENDKEY
            _cancelInventory?.Cancel();
            _cancelInventory?.Dispose();
#endif
        }

        public static void CreateSimuReaders(List<string> names)
        {
            _simuReader.Create(names);
        }

        // 获得门锁状态
        public GetLockStateResult GetShelfLockState(string lockNameList)
        {
            GetLockStateResult total_result = new GetLockStateResult();

            List<string> warnings = new List<string>();
            List<string> errorcodes = new List<string>();

            List<LockState> states = new List<LockState>();
            string[] list = lockNameList.Split(new char[] { ',' });
            foreach (var one in list)
            {
                // 探测锁状态
                // parameters:
                // parameters:
                //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
                //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
                //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
                //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
                GetLockStateResult result = null;

                if (Program.MainForm.InSimuLock)
                    result = _simuLock.GetShelfLockState(one);
                else
                {
#if OLD_SHELFLOCK
                    result = Program.Rfid.GetShelfLockState(one);
#else
                    result = Program.ShelfLock.GetShelfLockState(one, "");
#endif
                }

                if (result.Value == -1)
                    return result;
                if (result.ErrorInfo != null)
                    warnings.Add(result.ErrorInfo);
                if (result.ErrorCode != null)
                    errorcodes.Add(result.ErrorCode);
                states.AddRange(result.States);
            }

            if (warnings.Count > 0)
            {
                StringUtil.RemoveDupNoSort(ref warnings);
                total_result.ErrorInfo = StringUtil.MakePathList(warnings, "; ");
            }
            if (errorcodes.Count > 0)
            {
                StringUtil.RemoveDupNoSort(ref errorcodes);
                total_result.ErrorCode = StringUtil.MakePathList(errorcodes, "; ");
            }

            total_result.States = states;
            return total_result;
            // return new GetLockStateResult { Value = 0, States = states };
        }

        // 开锁。包括真实开锁和模拟开锁功能
        public NormalResult OpenShelfLock(string lockName)
        {
            if (Program.MainForm.InSimuLock)
                return _simuLock.OpenShelfLock(lockName, "");

#if OLD_SHELFLOCK
            // parameters:
            //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
            //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
            //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
            //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
            return Program.Rfid.OpenShelfLock(lockName, "");
#else
            return Program.ShelfLock.OpenShelfLock(lockName, "");
#endif
        }

        // 新版本
        public NormalResult OpenShelfLock(string lockName, string style)
        {
            if (Program.MainForm.InSimuLock)
                return _simuLock.OpenShelfLock(lockName, style);

#if OLD_SHELFLOCK

            // parameters:
            //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
            //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
            //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
            //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
            return Program.Rfid.OpenShelfLock(lockName, style);
#else
            return Program.ShelfLock.OpenShelfLock(lockName, style);
#endif
        }

        // 模拟关门
        public NormalResult CloseShelfLock(string lockName)
        {
            if (Program.MainForm.InSimuLock)
                return _simuLock.OpenShelfLock(lockName, "", false);

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = "CloseShelfLock() 只能在 InSimuLock == true 状态时调用"
            };

            // parameters:
            //      lockNameParam   为 "锁控板名字.卡编号.锁编号"。
            //                      其中卡编号部分可以是 "1" 也可以是 "1|2" 这样的形态
            //                      其中锁编号部分可以是 "1" 也可以是 "1|2|3|4" 这样的形态
            //                      如果缺乏卡编号和锁编号部分，缺乏的部分默认为 "1"
        }

        // 开关紫外灯
        public NormalResult TurnSterilamp(string lampName, string action)
        {
            return Program.Rfid.TurnSterilamp(lampName, action);
        }

        // 开关灯
        public NormalResult TurnShelfLamp(string lampName, string action)
        {
            return Program.Rfid.TurnShelfLamp(lampName, action);
        }

        public NormalResult GetState(string style)
        {
            if (style.StartsWith("clearCache"))
            {
                string session_id = StringUtil.GetParameterByPrefix(style, "clearCache");
                if (string.IsNullOrEmpty(session_id))
                    ClearLastUidTable();
                else
                    SetLastUids(session_id, "");
                return new NormalResult();
            }

            if (style == "getVersion")
            {
                return new NormalResult
                {
                    Value = 0,
                    ErrorCode = $"{ClientInfo.ClientVersion}"
                };
            }


            if (style == "restart")
            {
                Program.MainForm.Restart();
                return new NormalResult();
            }

            if (Program.MainForm.ErrorState == "normal")
            {
                var result = ListReaders();
                if (result.Readers.Length == 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorCode = "noReaders",
                        ErrorInfo = "没有任何连接的读卡器"
                    };
                return new NormalResult
                {
                    Value = 0,
                    ErrorCode = Program.MainForm.ErrorState,
                    ErrorInfo = Program.MainForm.ErrorStateInfo
                };
            }

            return new NormalResult
            {
                Value = -1,
                ErrorCode = Program.MainForm.ErrorState,
                ErrorInfo = Program.MainForm.ErrorStateInfo
            };
        }

        public NormalResult ActivateWindow()
        {
            Program.MainForm.ActivateWindow();
            return new NormalResult();
        }

        // 列出当前可用的 reader
        public ListReadersResult ListReaders()
        {
            // 选出已经成功打开的部分 Reader 返回
            List<string> readers = new List<string>();
            foreach (Reader reader in this.Readers)
            {
                if (reader.Result.Value == 0)
                    readers.Add(reader.Name);
            }
            return new ListReadersResult { Readers = readers.ToArray() };
        }

        // static string _lastUids = "";

        // session_id --> lastUids 对照表
        static Hashtable _lastUidTable = new Hashtable();
        // static object _sync_lastuids = new object();

        // 构造用于比较的 uid 字符串
        static string BuildUids(List<OneTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return "";

            StringBuilder current = new StringBuilder();
            foreach (OneTag tag in tags)
            {
                current.Append($"{tag.UID}|{tag.AntennaID},");
            }
            return current.ToString();
        }

        // 2019/12/4
        // 构造用于比较的锁状态字符串
        static string BuildStateString(List<LockState> states)
        {
            if (states == null || states.Count == 0)
                return "";

            StringBuilder current = new StringBuilder();
            foreach (var state in states)
            {
                current.Append($"{state.Path}|{state.State},");
            }
            return current.ToString();
        }

        // 清除 Hashtable
        static void ClearLastUidTable()
        {
            lock (_lastUidTable.SyncRoot)
            {
                _lastUidTable.Clear();
            }
        }

        static void SetLastUids(string session_id, string value)
        {
            /*
            lock (_sync_lastuids)
            {
                _lastUids = value;
            }
            */
            if (session_id == null)
                session_id = "";
            lock (_lastUidTable.SyncRoot)
            {
                // 防止 Hashtable 太大
                if (_lastUidTable.Count > 1000)
                    _lastUidTable.Clear();
                _lastUidTable[session_id] = value;
            }
        }

        // return:
        //      true    不一致
        //      false   一致
        static bool CompareLastUids(string session_id, string value)
        {
            /*
            lock (_sync_lastuids)
            {
                if (_lastUids != value)
                    return true;
                return false;
            }
            */
            if (session_id == null)
                session_id = "";
            string lastUids = "";
            lock (_lastUidTable.SyncRoot)
            {
                // TODO: 是否可以改为一次动作？
                if (_lastUidTable.ContainsKey(session_id))
                {
                    lastUids = (string)_lastUidTable[session_id];
                }
            }

            if (lastUids != value)
                return true;
            return false;
        }

        // 增加了无标签时延迟等待功能。敏捷响应
        // parameters:
        //      style   风格。逗号间隔的字符串内容
        //              session:会话ID
        //              dont_delay  不根据 session 来进行探测、延迟。也就是说确保要做一次 invetnory 并且立即返回
        public ListTagsResult ListTags(string reader_name, string style)
        {
            // Debug.Assert(false);

            if (Program.Rfid.Pause)
                return new ListTagsResult
                {
                    Value = -1,
                    ErrorInfo = "RFID 功能处于暂停状态",
                    ErrorCode = "paused"
                };

            string lockNameList = StringUtil.GetParameterByPrefix(style, "getLockState");
            if (lockNameList != null)
                lockNameList = StringUtil.UnescapeString(lockNameList);

            bool output_time = false;
            Program.Rfid.IncApiCount();
            try
            {
                if (Program.Rfid.Pause)
                    return new ListTagsResult
                    {
                        Value = -1,
                        ErrorInfo = "RFID 功能处于暂停状态",
                        ErrorCode = "paused"
                    };

                // 2019/12/15
                bool dont_delay = StringUtil.IsInList("dont_delay", style);

                string session_id = StringUtil.GetParameterByPrefix(style, "session");

                if (output_time)
                    Program.MainForm.OutputHistory($"session start: {session_id}, reader_name={reader_name}, style={style}");

                TimeSpan length = TimeSpan.FromSeconds(2);

                ListTagsResult result = null;
                GetLockStateResult lock_result = null;

                string current_uids = "";
                DateTime start = DateTime.Now;
                while (DateTime.Now - start < length
                    || result == null)
                {
                    Stopwatch watch = null;

                    // 执行 inventory
                    if (string.IsNullOrEmpty(reader_name) == false)
                    {
                        if (output_time)
                            watch = new Stopwatch();
                        else
                            watch = null;
                        watch?.Start();
                        result = _listTags(reader_name, style);
                        // testing
                        //if (reader_name.StartsWith("M201"))
                        //    Thread.Sleep(5000);
                        watch?.Stop();
                        if (output_time)
                            Program.MainForm.OutputHistory($"{session_id} inventory time:{watch.Elapsed.TotalSeconds}, count:{result.Results?.Count}");
                    }
                    else
                    {
                        if (output_time)
                            Program.MainForm.OutputHistory($"{session_id} inventory skipped");
                        result = new ListTagsResult();
                    }

                    // 执行门锁状态获取
                    // 目前是 1:1 次数分配
                    if (lockNameList != null)
                    {
                        Thread.Sleep(100);

                        if (output_time)
                            watch = new Stopwatch();
                        else
                            watch = null;
                        watch?.Start();
                        lock_result = GetShelfLockState(lockNameList);
                        watch?.Stop();
                        if (output_time)
                            Program.MainForm.OutputHistory($"{session_id} getLockState time:{watch.Elapsed.TotalSeconds}, count:{lock_result.States?.Count}");
                        // 从此开始 result.GetLockStateResult 就有值了
                        result.GetLockStateResult = lock_result;
                    }
                    else
                    {
                        if (output_time)
                            Program.MainForm.OutputHistory($"{session_id} getLockState skipped");
                    }

                    // 判断 inventory 结果
                    if (string.IsNullOrEmpty(reader_name) == false)
                    {
                        // 立即返回
                        if (dont_delay)
                            return result;

                        if (result != null && result.Results != null)
                            current_uids = BuildUids(result.Results);
                        else
                            current_uids = "";

                        // TODO: 这里的比较应该按照 Session 来进行
                        // 只要本次和上次 tag 数不同，立刻就返回
                        if (CompareLastUids(session_id, current_uids))
                        {
                            SetLastUids(session_id, current_uids);
                            return result;
                        }

                        if (result.Value == -1)
                        {
                            return result;
                        }
                    }


                    // 判断门锁状态
                    if (lockNameList != null)
                    {
                        // 这里的疑问是，如果 _listTags 没有出错，是否应该坚持返回正确结果？
                        if (lock_result.Value != -1)
                        {
                            string current_states = BuildStateString(lock_result.States);
                            if (CompareLastUids(session_id + "_lock", current_states))
                            {
                                SetLastUids(session_id + "_lock", current_states);
                                return result;
                            }
                        }
                    }

                    /*
                    // TODO: 如果本次和上次都是 2，是否立即返回？可否先对比一下 uid，有差别再返回?
                    if (result.Results != null
                        && result.Results.Count > 0)
                    {
                        SetLastUids(current_uids);
                        return result;
                    }
                    */
                    Thread.Sleep(10);  // 10? 200?
                }

                SetLastUids(session_id, current_uids);
                return result;
            }
            catch (Exception ex)
            {
                return new ListTagsResult
                {
                    Value = -1,
                    // TODO: 如何返回异常信息?
                    ErrorInfo = $"ListTags() 出现异常:{ExceptionUtil.GetDebugText(ex)}"
                };
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        List<Reader> Readers
        {
            get
            {
                if (Program.MainForm.InSimuReader)
                    return _simuReader.Readers;
                return Program.Rfid.Readers;
            }
        }

        //static uint _currenAntenna = 1;
        //DateTime _lastTime;

        static DateTime _lastCompactTime;
        static TimeSpan _compactLength = TimeSpan.FromMinutes(10);
        static int _inventoryErrorCount = 0;

        // parameters:
        //      reader_name_list    读卡器名字列表。形态为 "*" 或 "name1,name2" 或 "name1:1|2|3|4,name2"
        //      style   如果为 "getTagInfo"，表示要在结果中返回 TagInfo
        ListTagsResult _listTags(string reader_name_list, string style)
        {
            InventoryResult result = new InventoryResult();

            if (Program.MainForm.ErrorState != "normal")
                return new ListTagsResult
                {
                    Value = -1,
                    ErrorInfo = $"{Program.MainForm.ErrorStateInfo}",
                    ErrorCode = $"state:{Program.MainForm.ErrorState}"
                };

            List<OneTag> tags = new List<OneTag>();

            // uid --> OneTag
            Hashtable uid_table = new Hashtable();

            foreach (Reader reader in this.Readers)
            {
#if NO
                if (reader_name == "*" || reader.Name == reader_name)
                {

                }
                else
                    continue;
#endif

                // 顺便要从 reader_name_list 中解析出天线部分
                if (Reader.MatchReaderName(reader_name_list, reader.Name, out string antenna_list) == false)
                    continue;

                InventoryResult inventory_result = null;

                if (Program.MainForm.InSimuReader)
                    inventory_result = _simuReader.Inventory(reader.Name,
                    antenna_list,
                    style);
                else
                    inventory_result = Program.Rfid.Inventory(reader.Name,
                    antenna_list,
                    style   // ""
                    );



                /*
                // testing
                inventory_result.Value = -1;
                inventory_result.ErrorInfo = "模拟 inventory 出错";
                inventory_result.ErrorCode = "test";
                */

                if (inventory_result.Value == -1)
                {
                    // TODO: 统计单位时间内出错的总数，如果超过一定限度则重新初始化全部读卡器
                    _ = _compactLog.Add("inventory 出错: {0}", new object[] { inventory_result.ErrorInfo });
                    _inventoryErrorCount++;

                    // 每隔一段时间写入日志一次
                    if (DateTime.Now - _lastCompactTime > _compactLength)
                    {
                        _compactLog?.WriteToLog((text) =>
                        {
                            Log.Logger.Error(text);
                            Program.MainForm.OutputHistory(text, 2);
                        });
                        _lastCompactTime = DateTime.Now;

                        if (_inventoryErrorCount > 10)
                        {
                            // 发出信号，重启
                            Program.MainForm.RestartRfidDriver($"因最近阶段内 inventory 出错次数为 {_inventoryErrorCount}");
                            _inventoryErrorCount = 0;
                        }
                    }

                    return new ListTagsResult { Value = -1, ErrorInfo = inventory_result.ErrorInfo, ErrorCode = inventory_result.ErrorCode };
                }

                foreach (InventoryInfo info in inventory_result.Results)
                {
                    OneTag tag = null;
                    if (uid_table.ContainsKey(info.UID))
                    {
                        // 重复出现的，追加 读卡器名字
                        tag = (OneTag)uid_table[info.UID];
                        tag.ReaderName += "," + reader.Name;
                    }
                    else
                    {
                        // 首次出现
                        tag = new OneTag
                        {
                            Protocol = info.Protocol,
                            ReaderName = reader.Name,
                            UID = info.UID,
                            DSFID = info.DsfID,
                            AntennaID = info.AntennaID, // 2019/9/25
                            // InventoryInfo = info    // 有些冗余的字段
                        };

                        /*
                        // testing
                        tag.AntennaID = _currenAntenna;
                        if (DateTime.Now - _lastTime > TimeSpan.FromSeconds(5))
                        {
                            _currenAntenna++;
                            if (_currenAntenna > 50)
                                _currenAntenna = 1;
                            _lastTime = DateTime.Now;
                        }
                        */

                        uid_table[info.UID] = tag;
                        tags.Add(tag);
                    }

                    if (StringUtil.IsInList("getTagInfo", style)
                        && tag.TagInfo == null)
                    {
                        // TODO: 这里要利用 Hashtable 缓存
                        GetTagInfoResult result0 = null;
                        if (Program.MainForm.InSimuReader)
                            result0 = _simuReader.GetTagInfo(reader.Name, info);
                        else
                            result0 = Program.Rfid.GetTagInfo(reader.Name, info);
                        if (result0.Value == -1)
                        {
                            tag.TagInfo = null;
                            // TODO: 如何报错？写入操作历史?
                            // $"读取标签{info.UID}信息时出错:{result0.ToString()}"
                        }
                        else
                        {
                            tag.TagInfo = result0.TagInfo;
                        }
                    }
#if NO
                            GetTagInfoResult result0 = Program.Rfid.GetTagInfo(reader.Name, info);
                            if (result0.Value == -1)
                            {
                                // TODO: 如何报错？写入操作历史?
                                Program.MainForm.OutputText($"读取标签{info.UID}信息时出错:{result0.ToString()}", 2);
                                continue;
                            }

                            LogicChip chip = LogicChip.From(result0.TagInfo.Bytes,
                                (int)result0.TagInfo.BlockSize,
                                "" // result0.TagInfo.LockStatus
                                );
                            Element pii = chip.FindElement(ElementOID.PII);
                            if (pii == null)
                            {
                                Program.MainForm.Invoke((Action)(() =>
                                {
                                    // 发送 UID
                                    SendKeys.SendWait($"uid:{info.UID}\r");
                                }));
                            }
                            else
                            {
                                Program.MainForm.Invoke((Action)(() =>
                                {
                                    // 发送 PII
                                    SendKeys.SendWait($"pii:{pii.Text}\r");
                                }));
                            }
#endif
                }
            }

            return new ListTagsResult { Results = tags };

#if NO
            InventoryResult result = new InventoryResult();
            List<OneTag> tags = new List<OneTag>();
            _lockTagList.EnterReadLock();
            try
            {
                foreach (OneTag tag in _tagList)
                {
                    if (reader_name == "*" || tag.ReaderName == reader_name)
                        tags.Add(tag);
                }
                return new ListTagsResult { Results = tags };
            }
            finally
            {
                _lockTagList.ExitReadLock();
            }
#endif
        }

        // 2019/9/25
        // 新版本。根据 InventoryInfo 获得标签详细信息
        // result.Value
        //      -1
        //      0
        public GetTagInfoResult GetTagInfo(string reader_name,
            InventoryInfo info)
        {
            if (Program.MainForm.ErrorState != "normal")
                return new GetTagInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"{Program.MainForm.ErrorStateInfo}",
                    ErrorCode = $"state:{Program.MainForm.ErrorState}"
                };

            Program.Rfid.IncApiCount();
            try
            {
                List<GetTagInfoResult> errors = new List<GetTagInfoResult>();
                foreach (Reader reader in this.Readers)
                {
                    if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list) == false)
                        continue;

                    // result.Value
                    //      -1
                    //      0
                    GetTagInfoResult result0 = null;
                    if (Program.MainForm.InSimuReader)
                        result0 = _simuReader.GetTagInfo(reader.Name, info);
                    else
                        result0 = Program.Rfid.GetTagInfo(reader.Name, info);

                    // 继续尝试往后寻找
                    if (result0.Value == -1
                        // && result0.ErrorCode == "errorFromReader=4"
                        )
                    {
                        errors.Add(result0);
                        continue;
                    }

                    if (result0.Value == -1)
                        return result0;

                    // found
                    return result0;
                }

                if (errors.Count > 0)
                    return errors[0];

                return new GetTagInfoResult { ErrorCode = "notFoundReader" };
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        // TODO: 即将废除
        // 2019/9/27 增加的 antenna_id
        // result.Value
        //      -1
        //      0
        public GetTagInfoResult GetTagInfo(string reader_name,
            string uid,
            uint antenna_id)
        {
            if (Program.MainForm.ErrorState != "normal")
                return new GetTagInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"{Program.MainForm.ErrorStateInfo}",
                    ErrorCode = $"state:{Program.MainForm.ErrorState}"
                };

            Program.Rfid.IncApiCount();
            try
            {

                List<GetTagInfoResult> errors = new List<GetTagInfoResult>();
                foreach (Reader reader in this.Readers)
                {
#if NO
                if (reader_name == "*" || reader.Name == reader_name)
                {

                }
                else
                    continue;
#endif
                    if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list) == false)
                        continue;

                    string protocol = InventoryInfo.ISO15693;
                    if (StringUtil.IsInList(InventoryInfo.ISO18000P6C, reader.Protocols) == true)
                        protocol = InventoryInfo.ISO18000P6C;

                    InventoryInfo info = new InventoryInfo
                    {
                        Protocol = protocol,
                        UID = uid,
                        AntennaID = antenna_id
                    };

                    // result.Value
                    //      -1
                    //      0
                    GetTagInfoResult result0 = null;
                    if (Program.MainForm.InSimuReader)
                        result0 = _simuReader.GetTagInfo(reader.Name, info);
                    else
                        result0 = Program.Rfid.GetTagInfo(reader.Name, info);

                    // 继续尝试往后寻找
                    if (result0.Value == -1
                        // && result0.ErrorCode == "errorFromReader=4"
                        )
                    {
                        errors.Add(result0);
                        continue;
                    }

                    if (result0.Value == -1)
                        return result0;

                    // found
                    return result0;
                }

                // 2019/2/13
                if (errors.Count > 0)
                    return errors[0];

                return new GetTagInfoResult { ErrorCode = "notFoundReader" };
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        // 2020/10/10
        // 旧版本接口
        public GetTagInfoResult GetTagInfo(string reader_name,
    string uid,
    uint antenna_id,
    string style)
        {
            return GetTagInfo(reader_name,
                uid,
                antenna_id,
                null,
                style);
        }

        // 2020/12/14
        // 最新版接口
        public GetTagInfoResult GetTagInfo(string reader_name,
    string uid,
    uint antenna_id,
    string protocol_param,
    string style)
        {
            if (Program.MainForm.ErrorState != "normal")
                return new GetTagInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"{Program.MainForm.ErrorStateInfo}",
                    ErrorCode = $"state:{Program.MainForm.ErrorState}"
                };

            Program.Rfid.IncApiCount();
            try
            {
                List<GetTagInfoResult> errors = new List<GetTagInfoResult>();
                foreach (Reader reader in this.Readers)
                {
                    if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list) == false)
                        continue;

                    string protocol = InventoryInfo.ISO15693;
                    if (string.IsNullOrEmpty(protocol_param))
                    {
                        if (StringUtil.IsInList(InventoryInfo.ISO18000P6C, reader.Protocols) == true)
                            protocol = InventoryInfo.ISO18000P6C;
                    }
                    else
                        protocol = protocol_param;

                    InventoryInfo info = new InventoryInfo
                    {
                        Protocol = protocol,
                        UID = uid,
                        AntennaID = antenna_id
                    };

                    // result.Value
                    //      -1
                    //      0
                    GetTagInfoResult result0 = null;
                    if (Program.MainForm.InSimuReader)
                        result0 = _simuReader.GetTagInfo(reader.Name, info, style);
                    else
                        result0 = Program.Rfid.GetTagInfo(reader.Name, info, style);

                    // 继续尝试往后寻找
                    if (result0.Value == -1
                        // && result0.ErrorCode == "errorFromReader=4"
                        )
                    {
                        errors.Add(result0);
                        continue;
                    }

                    if (result0.Value == -1)
                        return result0;

                    // found
                    return result0;
                }

                // 2019/2/13
                if (errors.Count > 0)
                    return errors[0];

                return new GetTagInfoResult { ErrorCode = "notFoundReader" };
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        public NormalResult WriteTagInfo(
    string reader_name,
    TagInfo old_tag_info,
    TagInfo new_tag_info)
        {
            // TODO: 对 old_tag_info 和 new_tag_info 合法性进行一系列检查
            Program.Rfid.IncApiCount();
            try
            {
                foreach (Reader reader in this.Readers)
                {
#if NO
                if (reader_name == "*" || reader.Name == reader_name)
                {

                }
                else
                    continue;
#endif

                    if (Reader.MatchReaderName(reader_name, reader.Name, out string antenna_list) == false)
                        continue;


                    InventoryInfo info = new InventoryInfo
                    {
                        Protocol = old_tag_info.Protocol,
                        UID = old_tag_info.UID,
                        AntennaID = old_tag_info.AntennaID  // 2019/9/27
                    };
                    GetTagInfoResult result0 = null;
                    if (Program.MainForm.InSimuReader)
                        result0 = _simuReader.GetTagInfo(reader.Name, info);
                    else
                        result0 = Program.Rfid.GetTagInfo(reader.Name, info);

                    if (result0.Value == -1 && result0.ErrorCode == "errorFromReader=4")
                        continue;

                    if (result0.Value == -1)
                        return new NormalResult(result0);

                    // TODO: 是否对照检查 old_tag_info 和 result0.TagInfo ?

                    if (Program.MainForm.InSimuReader)
                        return _simuReader.WriteTagInfo(reader.Name,
                        old_tag_info,
                        new_tag_info);
                    else
                        return Program.Rfid.WriteTagInfo(reader.Name,
                            old_tag_info,
                            new_tag_info);
                }

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到 UID 为 {old_tag_info.UID} 的标签",
                    ErrorCode = "notFound"
                };
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        /*
        // 兼容以前的 API
        public NormalResult SetEAS(
string reader_name,
string tag_name,
bool enable)
        {
            return SetEAS(reader_name, tag_name, 0, enable);
        }
        */

        // 兼容以前的版本
        public NormalResult SetEAS(
string reader_name,
string tag_name,
uint antenna_id,
bool enable)
        {
            return SetEAS(reader_name,
                tag_name,
                antenna_id,
                enable,
                "");
        }

        // 兼容原先的 API
        public NormalResult SetEAS(
string reader_name,
string tag_name,
uint antenna_id,
bool enable,
string style)
        {
            var result = SetEAS1(
reader_name,
tag_name,
antenna_id,
enable,
style);
            return new NormalResult
            {
                Value = result.Value,
                ErrorCode = result.ErrorCode,
                ErrorInfo = result.ErrorInfo
            };
        }

        // parameters:
        //      reader_name 读卡器名字。也可以为 "*"，表示所有读卡器
        //      tag_name    标签名字。为 pii:xxxx 或者 uid:xxxx 形态。若没有冒号，则默认为是 UID
        //      style   处理风格。如果包含 "detect"，表示修改之前会先读出，如果没有必要修改则不会执行修改
        // return result.Value:
        //      -1  出错
        //      0   没有找到指定的标签
        //      1   找到，并成功修改 EAS
        public SetEasResult SetEAS1(
string reader_name,
string tag_name,
uint antenna_id,
bool enable,
string style)
        {
            Program.Rfid.IncApiCount();
            try
            {

                string uid = "";
                List<string> parts = StringUtil.ParseTwoPart(tag_name, ":");
                if (parts[0] == "pii")
                {
                    // 2019/9/24
                    // 天线列表
                    // 1|2|3|4 这样的形态
                    FindTagResult result = null;
                    if (Program.MainForm.InSimuReader)
                        result = _simuReader.FindTagByPII(
                            reader_name,
                            InventoryInfo.ISO15693 + "," + InventoryInfo.ISO18000P6C, // 只有 ISO15693 才有 EAS (2019/8/28) UHF 也有 EAS (2023/11/1)
                            antenna_id.ToString(),
                            parts[1]);
                    else
                        result = Program.Rfid.FindTagByPII(
                            reader_name,
                            InventoryInfo.ISO15693 + "," + InventoryInfo.ISO18000P6C, // 只有 ISO15693 才有 EAS (2019/8/28) UHF 也有 EAS (2023/11/1)
                            antenna_id.ToString(),
                            parts[1]);

                    if (result.Value != 1)
                        return new SetEasResult
                        {
                            Value = result.Value,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    uid = result.UID;
                    reader_name = result.ReaderName;    // 假如最初 reader_name 为 '*'，此处可以改为具体的读卡器名字，会加快后面设置的速度
                }
                else if (parts[0] == "uid" || string.IsNullOrEmpty(parts[0]))
                    uid = parts[1];
                else
                    return new SetEasResult
                    {
                        Value = -1,
                        ErrorInfo = $"未知的 tag_name 前缀 '{parts[0]}'",
                        ErrorCode = "unknownPrefix"
                    };

                {
                    // TODO: 检查 uid 字符串内容是否合法。应为 hex 数字

                    // return result.Value
                    //      -1  出错
                    //      0   成功
                    SetEasResult result = null;
                    if (Program.MainForm.InSimuReader)
                        result = _simuReader.SetEAS(
    reader_name,
    uid,
    antenna_id,
    enable,
    style);
                    else
                        result = Program.Rfid.SetEAS(
        reader_name,
        uid,
        antenna_id,
        enable,
        style);
                    if (result.Value == -1)
                        return result;
                    return new SetEasResult
                    {
                        Value = 1,
                        ChangedUID = result.ChangedUID
                    };
                }
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        public NormalResult ChangePassword(string reader_name,
    string uid,
    string type,
    uint old_password,
    uint new_password)
        {
            Program.Rfid.IncApiCount();
            try
            {
                return Program.Rfid.ChangePassword(
    reader_name,
    uid,
    type,
    old_password,
    new_password);
            }
            finally
            {
                Program.Rfid.DecApiCount();
            }
        }

        // 和模拟标签有关的功能
        public NormalResult SimuTagInfo(string action,
            List<TagInfo> tags,
            string style)
        {
            if (action == "switchToRealMode")
            {
                try
                {
                    // 退出模拟读卡器状态
                    Program.MainForm.InSimuReader = false;

                    // 也要退出模拟锁状态
                    Program.MainForm.InSimuLock = false;
                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"switchToSimuMode 出现异常: {ex.Message}"
                    };
                }
            }

            if (action == "switchToSimuMode")
            {
                try
                {
                    // 进入模拟读卡器状态
                    Program.MainForm.InSimuReader = true;

                    // 重新创建模拟读卡器对象
                    var readerNameList = StringUtil.GetParameterByPrefix(style, "readerNameList");
                    _simuReader.Create(StringUtil.SplitList(readerNameList, '|'));

                    // 也要进入模拟锁状态
                    Program.MainForm.InSimuLock = true;
                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"switchToSimuMode 出现异常: {ex.Message}"
                    };
                }
            }

            if (Program.MainForm.InSimuReader)
                return _simuReader.SimuTagInfo(action,
                tags,
                style);
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "因当前不在模拟标签状态，无法使用设置模拟标签的功能"
                };
        }

#if SENDKEY

        #region Tag List

        // 当前在读卡器探测范围内的标签
        static List<OneTag> _tagList = new List<OneTag>();
        static internal ReaderWriterLockSlim _lockTagList = new ReaderWriterLockSlim();

        bool AddToTagList(string reader_name,
            string uid,
            byte dsfid,
            string protocol)
        {
            OneTag tag = FindTag(uid);
            if (tag != null)
                return false;
            _lockTagList.EnterWriteLock();
            try
            {
                tag = new OneTag
                {
                    Protocol = protocol,
                    ReaderName = reader_name,
                    UID = uid,
                    LastActive = DateTime.Now,
                    DSFID = dsfid
                };
                tag.LastActive = DateTime.Now;
                _tagList.Add(tag);
            }
            finally
            {
                _lockTagList.ExitWriteLock();
            }

            // 触发通知动作
            // TODO: 通知以后，最好把标签内容信息给存储起来，这样 Inventory 的时候可以直接使用
            if (_sendKeyEnabled.Value == true)
                Notify(tag.ReaderName, tag.UID, tag.Protocol);
            return true;
        }

        OneTag FindTag(string uid)
        {
            _lockTagList.EnterReadLock();
            try
            {
                foreach (OneTag tag in _tagList)
                {
                    if (tag.UID == uid)
                    {
                        tag.LastActive = DateTime.Now;
                        return tag;
                    }
                }
                return null;
            }
            finally
            {
                _lockTagList.ExitReadLock();
            }
        }

        void ClearIdleTag(TimeSpan delta)
        {
            List<OneTag> delete_tags = new List<OneTag>();
            _lockTagList.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach (OneTag tag in _tagList)
                {
                    if (now - tag.LastActive >= delta)
                        delete_tags.Add(tag);
                }
            }
            finally
            {
                _lockTagList.ExitReadLock();
            }

            if (delete_tags.Count > 0)
            {
                _lockTagList.EnterWriteLock();
                try
                {
                    foreach (OneTag tag in delete_tags)
                    {
                        _tagList.Remove(tag);
                    }
                }
                finally
                {
                    _lockTagList.ExitWriteLock();
                }
            }
        }

        void Notify(string reader_name, string uid, string protocol)
        {
            Task.Run(() =>
            {
                bool succeed = false;
                for (int i = 0; i < 10; i++)
                {
                    succeed = NotifyTag(reader_name, uid, protocol);
                    if (succeed == true)
                        break;
                    Thread.Sleep(100);
                }
                if (succeed == false)
                    Program.MainForm.OutputHistory($"读卡器{reader_name}读取标签{uid}详细信息时出错", 1);
            });
        }

        #endregion

#endif

        static private AtomicBoolean _sendKeyEnabled = new AtomicBoolean(false);

        public NormalResult EnableSendKey(bool enable)
        {
            // 如果和以前的值相同
            bool old_value = _sendKeyEnabled.Value;
            if (old_value == enable)
                return new NormalResult();

            if (enable == true)
                _sendKeyEnabled.FalseToTrue();
            else
                _sendKeyEnabled.TrueToFalse();

            string message = "";
            if (enable)
                message = "RFID 发送打开";
            else
                message = "RFID 发送关闭";

            Task.Run(() =>
            {
                Program.MainForm?.OutputHistory(message, 0);
                Program.MainForm?.Speak(message);
            });

            return new NormalResult();
        }

        // 开始或者结束捕获标签
        public NormalResult BeginCapture(bool begin)
        {
#if SENDKEY
            StartInventory(begin);
#endif
            return new NormalResult();
        }

        // 管理读卡器
        public NormalResult ManageReader(string reader_name_list, string command)
        {
            return Program.Rfid.ManageReader(reader_name_list, command);
        }

        // 2020/7/1
        public NormalResult LedDisplay(string ledName,
            string text,
            int x,
            int y,
            DisplayStyle property,
            string style)
        {
            return Program.Led.Display(ledName, text, x, y, property, style);
        }

        // 2020/8/19
        // 小票打印
        // parameters:
        //      style   附加的子参数 
        public NormalResult PosPrint(
            string action,
            string text,
            string style)
        {
            if (action == "getstatus")
                return Program.Printer.GetStatus(style);

            return Program.Printer.Print(action, text, style);
        }

#if SENDKEY
        // 启动或者停止自动盘点
        void StartInventory(bool start)
        {
            // TODO: 是否要加锁，让本函数不能并行执行？
            if (start)
            {
                _cancelInventory?.Cancel();
                while (_cancelInventory != null)
                {
                    Task.Delay(500).Wait();
                }

                var task = DoInventory();
                // Task.Run(() => { DoInventory(); });
            }
            else
            {
                _cancelInventory?.Cancel();
                while (_cancelInventory != null)
                {
                    Task.Delay(500).Wait();
                }
            }
        }

        static CancellationTokenSource _cancelInventory = null;

        async Task DoInventory()
        {
            Program.MainForm.OutputHistory("开始捕获", 0);

            /*
            if (Program.Rfid.Readers.Count == 0)
                Program.MainForm.OutputHistory("当前没有可用的读卡器", 2);
            else
            {
                List<string> names = new List<string>();
                Program.Rfid.Readers.ForEach((o) => names.Add(o.Name));
                Program.MainForm.OutputHistory($"当前读卡器数量 {Program.Rfid.Readers.Count}。包括: \r\n{StringUtil.MakePathList(names, "\r\n")}", 0);
            }
            */

            if (Program.Rfid.ShelfLocks.Count > 0)
            {
                List<string> names = new List<string>();
                Program.Rfid.ShelfLocks.ForEach((o) => names.Add(o.Name));
                Program.MainForm.OutputHistory($"当前锁控数量 {Program.Rfid.ShelfLocks.Count}。包括: \r\n{StringUtil.MakePathList(names, "\r\n")}", 0);
            }

            _cancelInventory = new CancellationTokenSource();
            bool bFirst = true;
            try
            {
                // uid --> Driver Name
                // Hashtable uid_table = new Hashtable();
                while (_cancelInventory.IsCancellationRequested == false)
                {
                    await Task.Delay(200, _cancelInventory.Token).ConfigureAwait(false);

                    ClearIdleTag(TimeSpan.FromSeconds(1));  // 1 秒的防误触发时间

                    FlushCompactLog();

                    //if (_captureEnabled.Value == false)
                    //    continue;

                    // uid_table.Clear();
                    foreach (Reader reader in Program.Rfid.Readers)
                    {
                        if (reader == null)
                            continue;

                        if (Program.Rfid.Pause)
                            continue;

                        if (string.IsNullOrEmpty(Program.Rfid.State) == false)
                            break;

                        InventoryResult inventory_result = null;
                        //Program.Rfid.IncApiCount();
                        try
                        {
                            inventory_result = Program.Rfid.Inventory(
      reader.Name, bFirst ? "" : "only_new");
                            // bFirst = false;
                        }
                        finally
                        {
                            //Program.Rfid.DecApiCount();
                        }

                        if (inventory_result.Value == -1)
                        {
                            _compactLog?.Add("*** 读卡器 {0} 点选标签时出错: {1}",
                                new object[] { reader.Name, inventory_result.ToString() }
                                );
                            continue;
                            // ioError 要主动卸载有问题的 reader?
                            // 如何报错？写入操作历史？
                            // Program.MainForm.OutputHistory($"读卡器{reader.Name}点选标签时出错:{inventory_result.ToString()}\r\n已停止捕获过程", 2);
                            // return;
                        }

                        foreach (InventoryInfo info in inventory_result.Results)
                        {
                            //if (uid_table.ContainsKey(info.UID))
                            //    continue;
                            //uid_table[info.UID] = reader.Name;
                            AddToTagList(reader.Name, info.UID, info.DsfID, info.Protocol);
                        }

                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                _cancelInventory = null;
                Program.MainForm.OutputHistory("结束捕获", 0);
            }
        }


        static DateTime _lastFlushTime = DateTime.Now;
        static int _lastErrorCount = 0;

        void FlushCompactLog()
        {
            if (_compactLog == null)
                return;

            int minutes = 10;    // 分钟数
            TimeSpan delta = TimeSpan.FromMinutes(minutes);   // 10

            if (DateTime.Now - _lastFlushTime > delta)
            {
                _lastErrorCount += _compactLog.WriteToLog((text) =>
                {
                    Program.MainForm.OutputHistory(text, 2);
                });
                _lastFlushTime = DateTime.Now;

                if (_lastErrorCount > 200 * minutes)  // 200 相当于一分钟连续报错的量
                {
                    // 触发重启全部读卡器
                    Program.MainForm?.BeginRefreshReaders("connected", new CancellationToken());
                    Program.MainForm?.Speak("尝试重新初始化全部读卡器");
                    _lastErrorCount = 0;
                }
            }
        }

        bool NotifyTag(string reader_name, string uid, string protocol)
        {
            if (_sendKeyEnabled.Value == false)
                return false;

            // 2019/2/24
            if (protocol == InventoryInfo.ISO14443A)
            {
                Program.MainForm.Invoke((Action)(() =>
                {
                    // 发送 UID
                    SendKeys.SendWait($"uid:{uid},tou:80\r");
                }));
                Program.MainForm?.Speak("发送");
                return true;
            }

            InventoryInfo info = new InventoryInfo { UID = uid };
            GetTagInfoResult result0 = Program.Rfid.GetTagInfo(reader_name, info);
            if (result0.Value == -1)
            {
                // TODO: 如何报错？写入操作历史?
                // Program.MainForm.OutputText($"读取标签{info.UID}信息时出错:{result0.ToString()}", 2);
                return false;
            }

            LogicChip chip = LogicChip.From(result0.TagInfo.Bytes,
                (int)result0.TagInfo.BlockSize,
                "" // result0.TagInfo.LockStatus
                );
            Element pii = chip.FindElement(ElementOID.PII);
            Element typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage);

            StringBuilder text = new StringBuilder();
            if (pii == null)
                text.Append($"uid:{info.UID}");
            else
                text.Append($"pii:{pii.Text}");
            if (typeOfUsage != null)
                text.Append($",tou:{typeOfUsage.Text}");

            Program.MainForm.Invoke((Action)(() =>
            {
                // 发送 UID
                SendKeys.SendWait($"{text}\r");
            }));
            Program.MainForm?.Speak("发送");

            return true;
        }

#endif

    }
}
