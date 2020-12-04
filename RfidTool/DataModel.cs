using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        static Task _task = null;

        public static InitializeDriverResult InitialDriver()
        {
            var initial_result = _driver.InitializeDriver(
                    null,   // cfgFileName,
                    "", // style,
                    null/*hint_table*/);
            var token = _cancelRfidManager.Token;

            _task = Task.Factory.StartNew(
                () =>
                {
                    while (token.IsCancellationRequested == false)
                    {
                        Thread.Sleep(100);

                        string readerNameList = "*";
                        var result = _listTags(readerNameList, "");
                        if (result.Value == -1)
                            SetError?.Invoke(result,
                                new SetErrorEventArgs { Error = result.ErrorInfo });
                        else
                            SetError?.Invoke(result,
                                new SetErrorEventArgs { Error = null }); // 清除以前的报错

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

                        TagList.Refresh(// sender as BaseChannel<IRfid>,
                            readerNameList,
                            result.Results,
                            (readerName, uid, antennaID) =>
                            {
                                InventoryInfo info = new InventoryInfo
                                {
                                    UID = uid,
                                    AntennaID = antennaID
                                };
                                return _driver.GetTagInfo(readerName, info);
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
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return initial_result;
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
            _task?.Wait();
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


        // 写入标签
        public static NormalResult WriteTagInfo(string one_reader_name,
            TagInfo old_tag_info,
            TagInfo new_tag_info)
        {
            var result = _driver.WriteTagInfo(one_reader_name, old_tag_info, new_tag_info);

            // 清除缓存
            TagList.ClearTagTable(old_tag_info.UID);
            if (old_tag_info.UID != new_tag_info.UID)
                TagList.ClearTagTable(new_tag_info.UID);

            return result;
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
        static ListTagsResult _listTags(string reader_name_list, string style)
        {
            InventoryResult result = new InventoryResult();

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

                        result0 = _driver.GetTagInfo(reader.Name, info);
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

        public static SetErrorEventHandler SetError = null;

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
