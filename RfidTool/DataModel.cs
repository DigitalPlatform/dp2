using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;

using RfidDrivers.First;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;

namespace RfidTool
{
    /// <summary>
    /// 数据模型
    /// </summary>
    public static class DataModel
    {
        static RfidDriver1 _driver = new RfidDriver1();

        static CancellationTokenSource _cancelRfidManager = new CancellationTokenSource();

        public static NewTagList TagList = new NewTagList();

        /*
        static string _prev_uids = "";
        static int _prev_count = 0;
        */

        // 用于控制暂停的事件
#if EVENT_PAUSE
        static ManualResetEvent _eventPause = new ManualResetEvent(true);
#else
        static volatile bool _pause = false;
#endif

        static Task _task = null;

        public static InitializeDriverResult InitialDriver(
            bool reset_hint_table = false)
        {
            _cancelRfidManager?.Cancel();
            _cancelRfidManager?.Dispose();

            _cancelRfidManager = new CancellationTokenSource();
            var token = _cancelRfidManager.Token;
            var existing_hint_table = GetHintTable();

            // _driver.ReleaseDriver();
            string cfgFileName = Path.Combine(ClientInfo.UserDir, "readers.xml");
            var initial_result = _driver.InitializeDriver(
                    cfgFileName,
                    "", // style,
                    reset_hint_table ? null : existing_hint_table);
            if (initial_result.Value == -1)
                return initial_result;

            // 记忆
            if (reset_hint_table || existing_hint_table == null)
                SetHintTable(initial_result.HintTable);

            // 首次设置是否启用缓存
            TagList.EnableTagCache = EnableTagCache;
            TagList.NeedTid = true; // 差额运算中是否需要 TID

            _task = Task.Factory.StartNew(
                async () =>
                {
                    while (token.IsCancellationRequested == false)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100), token);

#if EVENT_PAUSE
                        int index = WaitHandle.WaitAny(new WaitHandle[] {
                            _eventPause,
                            token.WaitHandle,
                            });

                        if (index == 1)
                            return;
#else
                        if (_pause == true)
                            continue;
#endif



                        string readerNameList = "*";
                        var result = ListTags(readerNameList, "rssi");
                        if (result.Value == -1)
                            SetError?.Invoke(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                        {
                            SetError?.Invoke(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错
                        }

                        /*
                        // TODO: 归纳一下 UID 列表，如果不一样才继续往后处理
                        int count = result.Results.Count;
                        if (count == _prev_count)
                        {
                            string uids = GetUidString(result.Results);
                            if (uids == _prev_uids)
                                continue;
                            _prev_uids = uids;
                        }

                        _prev_count = result.Results.Count;
                        */

                        if (result.Results != null)
                            TagList.Refresh(// sender as BaseChannel<IRfid>,
                                readerNameList,
                                result.Results,
                                (readerName, uid, antennaID, protocol) =>
                                {
                                    InventoryInfo info = new InventoryInfo
                                    {
                                        Protocol = protocol,
                                        UID = uid,
                                        AntennaID = antennaID
                                    };
                                    return GetTagInfo(readerName, info, "tid"); // 获得 tid 速度较慢
                                },
                                (add_tags, update_tags, remove_tags) =>
                                {
                                    TagChanged?.Invoke(_driver, new NewTagChangedEventArgs
                                    {
                                        AddTags = add_tags,
                                        UpdateTags = update_tags,
                                        RemoveTags = remove_tags,
                                        Source = "driver",
                                    });
                                },
                                (type, text) =>
                                {
                                    SetError?.Invoke(null/*this*/, new SetErrorEventArgs { Error = text });
                                });

                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return initial_result;
        }

        // 暂停循环
        public static void PauseLoop()
        {
#if EVENT_PAUSE
            _eventPause.Reset();
#else
            _pause = true;
#endif
        }

        // 继续循环
        public static void ContinueLoop()
        {
#if EVENT_PAUSE
            _eventPause.Set();
#else
            _pause = false;
#endif
        }

        static List<HintInfo> GetHintTable()
        {
            string value = ClientInfo.Config.Get("readers", "hint_table");
            if (string.IsNullOrEmpty(value))
                return null;
            return JsonConvert.DeserializeObject<List<HintInfo>>(value);
        }

        static void SetHintTable(List<HintInfo> hint_table)
        {
            string value = JsonConvert.SerializeObject(hint_table);
            ClientInfo.Config.Set("readers", "hint_table", value);
        }

        public static void ReopenBluetoothReaders()
        {
            _driver.ReopenBluetoothReaders();
        }

        public static List<string> GetReadNameList(string style)
        {
            bool driver_version = StringUtil.IsInList("driverVersion", style);
            bool device_sn = StringUtil.IsInList("deviceSN", style);
            bool device_type = StringUtil.IsInList("deviceType", style);
            bool comm_type = StringUtil.IsInList("commType", style);

            List<string> results = new List<string>();
            foreach (var reader in _driver.Readers)
            {
                List<string> columns = new List<string>();
                columns.Add(reader.Name);
                if (driver_version)
                    columns.Add("固件版本号: " + reader.DriverVersion);
                if (device_sn)
                    columns.Add("设备序列号: " + reader.DeviceSN);
                if (device_type)
                    columns.Add("设备类型: " + reader.DriverName);
                if (comm_type)
                    columns.Add("通讯方式: " + reader.Type);
                results.Add(StringUtil.MakePathList(columns, ", "));
            }
            return results;
        }

        static string GetUidString(List<OneTag> tags)
        {
            StringBuilder result = new StringBuilder();
            foreach (var tag in tags)
            {
                result.Append($"{tag.UID},");
            }

            return result.ToString();
        }

        public static void ReleaseDriver()
        {
            _cancelRfidManager?.Cancel();
            try
            {
                _task?.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {

            }
            _driver.ReleaseDriver();
        }

        // 拟写入 RFID 标签的 OI 字符串
        public static string DefaultOiString
        {
            get
            {
                return ClientInfo.Config.Get("rfid", "default_oi", null);
            }
            set
            {
                ClientInfo.Config.Set("rfid", "default_oi", value);
            }
        }

        // 拟写入 RFID 标签的 AOI 字符串
        public static string DefaultAoiString
        {
            get
            {
                return ClientInfo.Config.Get("rfid", "default_aoi", null);
            }
            set
            {
                ClientInfo.Config.Set("rfid", "default_aoi", value);
            }
        }

        /*
高校联盟格式
国标格式
空白标签用高校联盟格式，其余依从原格式
空白标签用国标格式，其余依从原格式
        * */
        // 写入 UHF 标签时所用的数据格式
        public static string UhfWriteFormat
        {
            get
            {
                return ClientInfo.Config.Get("rfid", "uhf_write_format", "国标格式");
            }
            set
            {
                ClientInfo.Config.Set("rfid", "uhf_write_format", value);
            }
        }

        // 
        public static bool WarningWhenUhfFormatMismatch
        {
            get
            {
                return ClientInfo.Config.GetBoolean("rfid", "uhf_warningWhenFormatMismatch", true);
            }
            set
            {
                ClientInfo.Config.SetBoolean("rfid", "uhf_warningWhenFormatMismatch", value);
            }
        }

        public static bool WriteUhfUserBank
        {
            get
            {
                return ClientInfo.Config.GetBoolean("rfid", "uhf_writeUserBank", true);
            }
            set
            {
                ClientInfo.Config.SetBoolean("rfid", "uhf_writeUserBank", value);
            }
        }

        // 启用标签缓存
        // 缺省为 false (2021/5/12)
        public static bool EnableTagCache
        {
            get
            {
                return ClientInfo.Config.GetBoolean("general", "enableTagCache", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("general", "enableTagCache", value);
            }
        }

        // 扫描前倒计时秒数
        public static int BeforeScanSeconds
        {
            get
            {
                return ClientInfo.Config.GetInt("writeTag", "beforeScanSeconds", 5);
            }
            set
            {
                ClientInfo.Config.SetInt("writeTag", "beforeScanSeconds", value);
            }
        }

        // PII 号码校验规则
        public static string PiiVerifyRule
        {
            get
            {
                return ClientInfo.Config.Get("general", "pii_verify_rule", null);
            }
            set
            {
                ClientInfo.Config.Set("general", "pii_verify_rule", value);
            }
        }

        // 2025/9/30
        public static string GaoxiaoParameters
        {
            get
            {
                return ClientInfo.Config.Get("general", "gaoxiao_parameters", null);
            }
            set
            {
                ClientInfo.Config.Set("general", "gaoxiao_parameters", value);
            }
        }

        // 当写入标签的时候是否校验条码号
        public static bool VerifyPiiWhenWriteTag
        {
            get
            {
                return ClientInfo.Config.GetBoolean("writeTag", "verifyBarcode", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("writeTag", "verifyBarcode", value);
            }
        }

        // 使用本地存储(写入标签时会查找利用本地存储中的册记录)
        public static bool UseLocalStoreage
        {
            get
            {
                return ClientInfo.Config.GetBoolean("writeTag", "useLocalStorage", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("writeTag", "useLocalStorage", value);
            }
        }

        // 2022/7/23
        // 写入时是否把解析错误的标签当作空白标签直接覆盖
        public static bool ErrorContentAsBlank
        {
            get
            {
                return ClientInfo.Config.GetBoolean("writeTag", "errorContentAsBlank", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("writeTag", "errorContentAsBlank", value);
            }
        }

        public static GaoxiaoEpcInfo BuildGaoxiaoEpcInfo(bool lending = false)
        {
            var parameters = DataModel.GaoxiaoParameters;

            byte[] bytes = null;
            {
                var cphex = StringUtil.GetParameterByPrefix(parameters, "cphex");
                if (string.IsNullOrEmpty(cphex) == false)
                    bytes = ByteArray.GetTimeStampByteArray(cphex);
            }

            var version = StringUtil.GetParameterByPrefix(parameters, "version");
            int version_value = string.IsNullOrEmpty(version) ? 4 : int.Parse(version);

            var picking = StringUtil.GetParameterByPrefix(parameters, "picking");
            int picking_value = string.IsNullOrEmpty(picking) ? 0 : int.Parse(picking);

            var reserve = StringUtil.GetParameterByPrefix(parameters, "reserve");
            int reserve_value = string.IsNullOrEmpty(reserve) ? 0 : int.Parse(reserve);

            return new GaoxiaoEpcInfo
            {
                Version = version_value,
                Lending = lending,
                Picking = picking_value,
                Reserve = reserve_value,
                OverwriteContentParameterBytes = bytes,
            };
        }


        // 写入标签
        public static NormalResult WriteTagInfo(string one_reader_name,
            TagInfo old_tag_info,
            TagInfo new_tag_info)
        {
            _driver.IncApiCount();
            try
            {
                var result = _driver.WriteTagInfo(one_reader_name, old_tag_info, new_tag_info);

                // UHF 保存后 EPC 会发生变化，为了避免引起不必要的 PrepareTagInfo 动作，ClearTagTable() 时第二参数应该为 false
                bool clearTagInfo = (old_tag_info.Protocol == InventoryInfo.ISO15693 ? true : false);
                // 清除缓存
                TagList.ClearTagTable(old_tag_info.UID, clearTagInfo);
                if (old_tag_info.UID != new_tag_info.UID)
                    TagList.ClearTagTable(new_tag_info.UID, clearTagInfo);

                return result;
            }
            finally
            {
                _driver.DecApiCount();
            }
        }

        public static SetEasResult SetEAS(string reader_name,
    string uid,
    uint antenna_id,
    bool enable,
    string style)
        {
            _driver.IncApiCount();
            try
            {
                return _driver.SetEAS(reader_name,
                uid,
                antenna_id,
                enable,
                style);
            }
            finally
            {
                _driver.DecApiCount();
            }
        }

        /*
        public static void StartRfidManager(string url)
        {
            _cancelRfidManager?.Cancel();

            _cancelRfidManager = new CancellationTokenSource();
            RfidManager.Base.Name = "RFID 中心";
            RfidManager.Url = url;
            // RfidManager.AntennaList = "1|2|3|4";    // testing
            // RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;
            RfidManager.Start(_cancelRfidManager.Token);
        }
        */

        // parameters:
        //      reader_name_list    读卡器名字列表。形态为 "*" 或 "name1,name2" 或 "name1:1|2|3|4,name2"
        //      style   如果为 "getTagInfo"，表示要在结果中返回 TagInfo
        public static ListTagsResult ListTags(string reader_name_list, string style)
        {
            _driver.IncApiCount();
            try
            {
                // InventoryResult result = new InventoryResult();

                List<OneTag> tags = new List<OneTag>();

                // uid --> OneTag
                Hashtable uid_table = new Hashtable();

                foreach (Reader reader in _driver.Readers)
                {
                    // 顺便要从 reader_name_list 中解析出天线部分
                    if (Reader.MatchReaderName(reader_name_list, reader.Name, out string antenna_list) == false)
                        continue;

                    InventoryResult inventory_result = null;

                    inventory_result = _driver.Inventory(reader.Name,
                    antenna_list,
                    style   // ""
                    );

                    /*
                    // testing
                    inventory_result.Value = -1;
                    inventory_result.ErrorInfo = "模拟 inventory 出错";
                    inventory_result.ErrorCode = "test";
                    */

                    // TODO: 不要中断其他处理。可以设法警告或者报错
                    if (inventory_result.Value == -1)
                    {
                        /*
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
                        */

                        if (reader.Type == "BLUETOOTH")
                            continue;
                        return new ListTagsResult
                        {
                            Value = -1,
                            ErrorInfo = inventory_result.ErrorInfo,
                            ErrorCode = inventory_result.ErrorCode
                        };
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
                                RSSI = info.RSSI,   // 2025/9/30
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

                            result0 = _driver.GetTagInfo(reader.Name, info, "tid");    // 注： style 为 "tid" 可以获得 TID Bank 内容
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
                    }
                }

                return new ListTagsResult { Results = tags };
            }
            finally
            {
                _driver.DecApiCount();
            }
        }

        public static GetTagInfoResult GetTagInfo(
            string one_reader_name,
            InventoryInfo info,
            string style)
        {
            _driver.IncApiCount();
            try
            {
                return _driver.GetTagInfo(one_reader_name, info, style);
            }
            finally
            {
                _driver.DecApiCount();
            }
        }

        public static void IncApiCount()
        {
            _driver.IncApiCount();
        }

        public static void DecApiCount()
        {
            _driver.DecApiCount();
        }

        public static event SetErrorEventHandler SetError = null;

        public static event NewTagChangedEventHandler TagChanged = null;

#if REMOVED
        private static void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 标签总数显示
            if (e.Result.Results != null)
            {
                TagList.Refresh(// sender as BaseChannel<IRfid>,
                    e.ReaderNameList,
                    e.Result.Results,
                    (readerName, uid, antennaID) =>
                    {
                        var channel = sender as BaseChannel<IRfid>;
                        return channel.Object.GetTagInfo(readerName, uid, antennaID);
                    },
                    (add_tags, update_tags, remove_tags) =>
                    {
                        TagChanged?.Invoke(sender, new NewTagChangedEventArgs
                        {
                            AddTags = add_tags,
                            UpdateTags = update_tags,
                            RemoveTags = remove_tags,
                            Source = e.Source,
                        });
                    },
                    (type, text) =>
                    {
                        RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                    });
            }
        }

#endif

        /*
        public static void StopRfidManager()
        {
            _cancelRfidManager?.Cancel();
            RfidManager.Url = "";
            RfidManager.ListTags -= RfidManager_ListTags;
        }
        */

        #region  UID-->PII 对照关系日志文件

        // 去重用
        static Hashtable _uidTable = new Hashtable();

        static StreamWriter _uidWriter = null;

        // 写入 UID-->UII(OI.PII) 对照关系日志文件
        // TODO: 检查记事本打开文件的情况下，是否依然可以写入文件
        public static void WriteToUidLogFile(string uid,
            string uii)
        {
            // 2021/4/29
            if (uid != null && uid.Contains(" "))
            {
                string error = $"WriteToUidLogFile() uid='{uid}' 出现了意外的空格字符";
                ClientInfo.WriteErrorLog(error);
                throw new Exception(error);
            }

            if (uii != null && uii.Contains(" "))
            {
                string error = $"WriteToUidLogFile() uii='{uii}' 出现了意外的空格字符";
                ClientInfo.WriteErrorLog(error);
                throw new Exception(error);
            }

            lock (_uidTable.SyncRoot)
            {
                if (_uidTable.ContainsKey(uid) == true)
                {
                    string old_pii = (string)_uidTable[uid];
                    if (old_pii == uii)
                        return; // 去重
                }
                // 防止占用内存太大
                if (_uidTable.Count > 1000)
                    _uidTable.Clear();
                _uidTable[uid] = uii;
            }

            if (_uidWriter == null)
            {
                string fileName = Path.Combine(ClientInfo.UserDir, "uid.txt");
                _uidWriter = new StreamWriter(fileName, true, Encoding.ASCII);
            }

            _uidWriter.WriteLine($"{uid}\t{uii}");
        }

        public static void CloseUidLogFile()
        {
            try
            {
                if (_uidWriter != null)
                {
                    _uidWriter.Close();
                    _uidWriter = null;
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"关闭 UID 对照文件时出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        #endregion
    }

    public delegate void NewTagChangedEventHandler(object sender,
NewTagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class NewTagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddTags { get; set; }
        public List<TagAndData> UpdateTags { get; set; }
        public List<TagAndData> RemoveTags { get; set; }
        public string Source { get; set; }   // 触发者
    }

    public delegate void SetErrorEventHandler(object sender,
SetErrorEventArgs e);

    /// <summary>
    /// 设置出错信息事件的参数
    /// </summary>
    public class SetErrorEventArgs : EventArgs
    {
        public string Error { get; set; }
    }
}
