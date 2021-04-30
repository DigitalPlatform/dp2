using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Net.Http;

using Microsoft.VisualStudio.Threading;

using ClosedXML.Excel;
using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using static dp2Inventory.LibraryChannelUtil;
using dp2Inventory.InventoryAPI;

namespace dp2Inventory
{
    public class InventoryData
    {

#if REMOVED
        // 从 inventory.xml 获得馆藏地列表(不访问 dp2library 服务器)
        // result.Value
        //      -1  出错
        //      0   文件或者列表定义没有找到
        //      1   找到
        public static GetLocationListResult sip_GetLocationListFromLocal()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return new GetLocationListResult
                {
                    Value = 0,
                    ErrorCode = "fileNotFound",
                    List = new List<string>()
                };
            var attr = dom.DocumentElement.SelectSingleNode("library/@locationList");
            if (attr == null)
                return new GetLocationListResult
                {
                    List = new List<string>()
                };

            return new GetLocationListResult
            {
                Value = 1,
                List = StringUtil.SplitList(attr.Value)
            };
        }

#endif

        public static void SpeakSequence(string text)
        {
            FormClientInfo.Speak(text, false, false);
        }

        public static void Speak(string text)
        {
            FormClientInfo.Speak(text, false, true);
        }

        #region Entity

        // 根据 UID 和 UII 创建 Entity 对象
        public static Entity NewEntity(OneTag tag,
            string uii)
        {
            Entity result = new Entity
            {
                UID = tag.UID,
                ReaderName = tag.ReaderName,
                Antenna = tag.AntennaID.ToString(),
                TagInfo = null,
            };

            ParseOiPii(uii, out string pii, out string oi);
            result.OI = oi;
            result.PII = pii;
            return result;
        }

        // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
        // Exception:
        //      可能会抛出异常 ArgumentException
        public static Entity NewEntity(OneTag tag,
            Entity entity,
            out string tou,
            bool throw_exception = true)
        {
            tou = "";
            Entity result = entity;
            if (result == null)
            {
                result = new Entity
                {
                    UID = tag.UID,
                    ReaderName = tag.ReaderName,
                    Antenna = tag.AntennaID.ToString(),
                    TagInfo = tag.TagInfo,
                };
            }

            LogicChip chip = null;
#if REMOVED
            if (string.IsNullOrEmpty(tag.Type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                try
                {
                    SetTagType(tag, out string pii, out chip);
                    if (tag.OneTag.TagInfo != null)
                        result.PII = pii;
                    else if (result.PII != null && result.PII.StartsWith("(读者卡)"))
                        result.PII = null;  // 2021/1/26

                    result.BuildError("parseTag", null, null);
                }
                catch (Exception ex)
                {
                    SpeakSequence("警告: 标签解析出错");
                    if (throw_exception == false)
                    {
                        result.BuildError(
                            "parseTag",
                            $"RFID 标签格式错误: {ex.Message}",
                            "parseTagError");
                    }
                    else
                        throw ex;
                }
            }
#endif

            // 2020/7/15
            // 获得图书 RFID 标签的 OI 和 AOI 字段
            // if (tag.Type == "book")
            {
                if (chip == null)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    chip = LogicChip.From(tag.TagInfo.Bytes,
            (int)tag.TagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                }

                if (chip.IsBlank())
                {
                    entity.BuildError("checkTag", "空白标签", "blankTag");
                }
                else
                {
                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    string oi = chip.FindElement(ElementOID.OI)?.Text;
                    string aoi = chip.FindElement(ElementOID.AOI)?.Text;

                    result.PII = pii;
                    result.OI = oi;
                    result.AOI = aoi;

                    // 10 图书; 80 读者证; 30 层架标
                    tou = chip.FindElement(ElementOID.TU)?.Text;

                    // 2020/8/27
                    // 图书标签严格要求必须有 OI(AOI) 字段
                    if ((tou != null && tou.StartsWith("1"))
                        && string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                        result.BuildError("checkTag", "没有 OI 或 AOI 字段", "missingOI");
                    else
                        result.BuildError("checkTag", null, null);
                }
            }
#if REMOVED
            else if (tag.Type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                result.PII = "(读者卡)" + result.PII;
                result.BuildError("checkTag", "读者卡误放入书架", "patronCard");
                SpeakSequence("读者卡误放入书架");
            }
            else
                result.BuildError("checkTag", null, null);
#endif
            return result;
        }

#if REMOVED
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        static void SetTagType(TagAndData data,
            out string pii,
            out LogicChip chip)
        {
            pii = null;
            chip = null;

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
                chip = LogicChip.From(data.OneTag.TagInfo.Bytes,
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
#endif

        #endregion

        public delegate void delegate_writeHistory(ProcessInfo info, string action);

        // Inventory 操作，同一时刻只能一个函数进入
        static AsyncSemaphore _requestLimit = new AsyncSemaphore(1);

        public static async Task BeginInventoryAsync(
            ProcessInfo info,
            string actionMode,
            delegate_writeHistory writeHistory)
        {
            Debug.Assert(info != null);
            Entity entity = info.Entity;
            using (var releaser = await _requestLimit.EnterAsync().ConfigureAwait(false))
            {
                // var info = entity.Tag as ProcessInfo;

                // 是否校验 EAS。临时决定
                bool need_verifyEas = false;

                int succeed_count = 0;

                // 还书
                if (info != null
                    && (StringUtil.IsInList("setLocation", actionMode)
                    || StringUtil.IsInList("setCurrentLocation", actionMode)
                    || StringUtil.IsInList("verifyEAS", actionMode))
                    && HasBorrowed(info.ItemXml)
                    && info.IsTaskCompleted("return") == false
                    /*&& DataModel.Protocol != "sip"*/)
                {
                    RequestInventoryResult request_result = null;
                    if (DataModel.Protocol == "sip")
                    {
                        var return_result = await SipChannelUtil.ReturnAsync(entity.PII);
                        if (return_result.Value == -1)
                            request_result = new RequestInventoryResult
                            {
                                Value = -1,
                                ErrorInfo = return_result.ErrorInfo,
                                ErrorCode = return_result.ErrorCode
                            };
                        else
                        {
                            // 重新获得 ItemXml
                            var get_result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
    entity.GetOiOrAoi(),
    DataModel.sipLocalStore ? "network,localInventory" : "network");
                            if (get_result.Value == -1)
                            {
                                request_result = new RequestInventoryResult
                                {
                                    Value = -1,
                                    ErrorInfo = "还书成功，但随后重新获取册记录时失败: " + get_result.ErrorInfo,
                                    ErrorCode = get_result.ErrorCode
                                };
                            }
                            else
                            {
                                request_result = new RequestInventoryResult
                                {
                                    Value = 0,
                                    ItemXml = get_result.ItemXml,
                                };
                            }
                        }
                    }
                    else
                        request_result = RequestReturn(
    entity.PII,
    entity.ItemRecPath,
    info.BatchNo,
    info.UserName,
    "");
                    info.SetTaskInfo("return", request_result);
                    if (request_result.Value == -1)
                    {
                        SpeakSequence($"{entity.PII} 还书请求出错");
                        entity.BuildError("return", request_result.ErrorInfo, request_result.ErrorCode);
                    }
                    else
                    {
                        entity.BuildError("return", null, null);

                        writeHistory?.Invoke(info, "还书");

                        // 提醒操作者发生了还书操作
                        await FormClientInfo.Speaking($"还书成功 {entity.PII}", false, default);

                        if (string.IsNullOrEmpty(request_result.ItemXml) == false)
                        {
                            info.ItemXml = request_result.ItemXml;
                            // 2021/1/29
                            entity.SetData(entity.ItemRecPath, request_result.ItemXml);
                        }

                        // 标记，即将 VerifyEas
                        need_verifyEas = true;
                    }
                }

                // 确保还书成功后，再执行 EAS 检查
                if (
                    (info.ContainTask("return") == false || info.IsTaskCompleted("return") == true)
                    && (need_verifyEas == true || StringUtil.IsInList("verifyEAS", actionMode))
                    /*&& DataModel.Protocol != "sip"*/)
                {
                    await VerifyEasAsync(info, writeHistory);
                }

                /*
                // 如果有以前尚未执行成功的修改 EAS 的任务，则尝试再执行一次
                if (info.TargetEas != null
                    && info.ContainTask("changeEAS") == true
                    && info.IsTaskCompleted("changeEAS") == false)
                {
                    await TryChangeEas(entity, info.TargetEas == "on");
                }
                */

                bool isSipLocal = false;
                if (DataModel.Protocol == "sip")
                {
                    // isSipLocal = StringUtil.IsInList("inventory", SipLocalStore);
                    isSipLocal = DataModel.sipLocalStore;
                }

                // 设置 UID
                if (info.FoundUii == false
                    && StringUtil.IsInList("setUID", actionMode)
                    && (string.IsNullOrEmpty(info.ItemXml) == false || isSipLocal == true)
                    && info.IsTaskCompleted("setUID") == false
                    )
                {
                    // TODO: SIP2 模式下，UID - PII 对照信息可以设置到 dp2ssl 本地数据库
                    RequestSetUidResult request_result = null;
                    if (DataModel.Protocol == "sip")
                        request_result = await RequestSetUIDtoLocalAsync(
                            entity.UID,
                            entity.PII,
                            entity.GetOiOrAoi(),
                            "");
                    else
                    {
                        // TODO: 如果 entity 本身是从对照表得到的，就可以跳过 SetUID 这一步
                        request_result = RequestSetUID(entity.ItemRecPath,
                            info.ItemXml,
                            null,
                            entity.UID,
                            info.UserName,
                            "");
                    }
                    info.SetTaskInfo("setUID", request_result);
                    if (request_result.Value == -1)
                    {
                        SpeakSequence($"{entity.UID} 设置 UID 请求出错");
                        // TODO: NotChanged 处理
                        entity.BuildError("setUID", request_result.ErrorInfo, request_result.ErrorCode);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(request_result.NewItemXml) == false)
                            info.ItemXml = request_result.NewItemXml;

                        entity.BuildError("setUID", null, null);
                        succeed_count++;
                    }
                }

                // 动作模式
                /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
                 * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
                 * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
                 * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
                 * */

                // 修改 currentLocation 和 location
                if (StringUtil.IsInList("setLocation,setCurrentLocation", actionMode)
                    && info.IsTaskCompleted("setLocation") == false
                    && info.IsTaskCompleted("getItemXml") == true   // 2021/1/26
                    && string.IsNullOrEmpty(info.ItemXml) == false)
                {
                    List<string> actions = new List<string>();
                    if (StringUtil.IsInList("setCurrentLocation", actionMode))
                        actions.Add("修改当前位置为 " + info.TargetCurrentLocation);
                    if (StringUtil.IsInList("setLocation", actionMode))
                        actions.Add("修改永久位置为 " + info.TargetLocation + ":" + info.TargetShelfNo);

                    RequestInventoryResult request_result = null;
                    if (DataModel.Protocol == "sip")
                    {
                        // bool isLocal = StringUtil.IsInList("inventory", SipLocalStore);
                        if (isSipLocal)
                            request_result = await RequestInventory_local(
                                info.ItemXml,
                                entity.UID,
                                entity.GetOiPii(),
                                StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                                info.BatchNo,
                                info.UserName,
                                actionMode);
                        else
                            request_result = await RequestInventory_sip2(entity.UID,
                                entity.PII,
                                entity.GetOiOrAoi(),
                                StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                                info.BatchNo,
                                info.UserName,
                                actionMode);
                    }
                    else
                        request_result = RequestInventory(entity.UID,
                            entity.PII,
                            StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                            info.BatchNo,
                            info.UserName,
                            actionMode);

                    /*
#if AUTO_TEST
                    request_result.Value = -1;
                    request_result.ErrorInfo = "测试 setLocation 请求错误";
#endif
                    */

                    // 两个动作当作一个 setLocation 来识别
                    info.SetTaskInfo("setLocation", request_result);
                    if (request_result.Value == -1)
                    {
                        SpeakSequence($"{entity.PII} 盘点请求出错");
                        // TODO: NotChanged 处理
                        entity.BuildError("setLocation", request_result.ErrorInfo, request_result.ErrorCode);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(request_result.ItemXml) == false)
                            entity.SetData(entity.ItemRecPath, request_result.ItemXml);

                        // TODO: info.ItemXml 是否需要被改变?
                        entity.BuildError("setLocation", null, null);
                        succeed_count++;

                        // 立即刷新当前架位和永久架位的显示
                        InventoryDialog.RefreshLocations(info);

                        if (actions.Count > 0)
                            writeHistory?.Invoke(info, StringUtil.MakePathList(actions, "; "));
                    }

                    // 2021/3/24
                    // 上传
                    if (StringUtil.IsInList("setLocation,setCurrentLocation", actionMode)
                        && info.IsTaskCompleted("setLocation"))
                    {
                        var upload_result = await RequestInventoryUploadAsync(
                            info.ItemXml,
                            entity.UID,
                            entity.GetOiPii(),
                            StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                            info.BatchNo,
                            info.UserName,
                            actionMode);
                        if (upload_result.Value == -1)
                        {
                            SpeakSequence($"{entity.PII} 上传请求出错");
                            entity.BuildError("upload", upload_result.ErrorInfo, upload_result.ErrorCode);
                        }
                        else
                        {
                            entity.BuildError("upload", null, null);
                            succeed_count++;
                        }
                    }
                }

                // SetUID 和 Inventory 至少成功过一次，则发出成功的响声
                if (succeed_count > 0)
                    SoundMaker.SucceedSound();
            }
        }

        // 判断当前 entity 对应的 RFID 标签的 EAS 状态
        // 注：通过 AFI 进行判断。0x07 为 on；0xc2 为 off
        // return:
        //      1 为 on; 0 为 off; -1 表示不合法的值; -2 表示 TagInfo 为 null 无法获得 AFI
        static int GetEas(ProcessInfo info)
        {
            Entity entity = info.Entity;
            Debug.Assert(entity != null);
            // tagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;
            // var info = entity.Tag as ProcessInfo;

            // TagInfo 为 null ?
            if (entity.TagInfo == null)
            {
                // 标签正好在读卡器上，读 TagInfo 一次
                // if (TagOnReader(entity))
                {
                    var get_result = RfidManager.GetTagInfo(entity.ReaderName, entity.UID, Convert.ToUInt32(entity.Antenna));
                    if (get_result.Value != -1)
                        entity.TagInfo = get_result.TagInfo;
                }

                if (entity.TagInfo == null)
                {
                    /*
                    // 加入 error 队列，等待后面处理
                    info.GetTagInfoError = "errorGetTagInfo";    // 表示希望获得 TagInfo
                    int count = AddErrorEntity(entity, out bool changed);
                    if (changed == true)
                        App.CurrentApp.SpeakSequence(count.ToString());
                    */
                    return -2;
                }
            }

            var afi = entity.TagInfo.AFI;
            if (afi == 0x07)
            {
                // 2021/1/29
                if (entity.TagInfo.EAS == false)
                    return -1;
                return 1;
            }
            if (afi == 0xc2)
            {
                // 2021/1/29
                if (entity.TagInfo.EAS == true)
                    return -1;
                return 0;
            }
            return -1;   // -1 表示不合法的值
        }


        // 观察册记录 XML 中是否有 borrower 元素
        static bool HasBorrowed(string item_xml)
        {
            if (string.IsNullOrEmpty(item_xml))
                return false;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(item_xml);
            }
            catch
            {
                return false;
            }

            string borrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(borrower) == false)
                return true;
            return false;
        }

        // 检测 RFID 标签 EAS 位是否正确
        // return.Value
        //      -1  出错
        //      0   没有进行验证(已经加入后台验证任务)
        //      1   已经成功进行验证
        public static async Task<NormalResult> VerifyEasAsync(
            ProcessInfo info,
            delegate_writeHistory writeHistory)
        {
            Entity entity = info.Entity;
            Debug.Assert(entity != null);
            if (string.IsNullOrEmpty(info.ItemXml))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "因 ItemXml 为空，无法进行 EAS 验证"
                };
            }

            var borrowed = HasBorrowed(info.ItemXml);
            var ret = GetEas(info);
            if (ret == -2)
            {
                // 当前无法判断，需要等 GetTagInfo() 以后再重试
                info.TargetEas = "?";
                info.SetTaskInfo("changeEAS", new NormalResult
                {
                    Value = -1,
                    ErrorCode = "initial"   // 表示需要处理但尚未开始处理
                });
            }
            else if (ret == -1
                || (ret == 1 && borrowed == true)
                || (ret == 0 && borrowed == false))
            {
                info.TargetEas = borrowed ? "off" : "on";
                info.SetTaskInfo("changeEAS", new NormalResult
                {
                    Value = -1,
                    ErrorCode = "initial"   // 表示需要处理但尚未开始处理
                });

                // result.Value
                //      -1  出错
                //      0   标签不在读卡器上所有没有执行
                //      1   成功执行修改
                var result = await TryChangeEasAsync(info, !borrowed);

                // TODO: 语音提醒，有等待处理的 EAS
                if (result.Value != 1)
                {
                    SpeakSequence($"等待修改 EAS : {CutTitle(entity.Title)} ");
                    return new NormalResult();
                }
                else
                {
                    writeHistory?.Invoke(info, "修改 EAS");
                }

                return new NormalResult { Value = 1 };
            }

            return new NormalResult();
        }

        static AsyncSemaphore _easLimit = new AsyncSemaphore(1);

        // 尝试修改 RFID 标签的 EAS
        // result.Value
        //      -1  出错
        //      0   标签不在读卡器上所有没有执行
        //      1   成功执行修改
        public static async Task<NormalResult> TryChangeEasAsync(
            ProcessInfo info,
            bool enable)
        {
            Entity entity = info.Entity;
            Debug.Assert(entity != null);

            using (var releaser = await _easLimit.EnterAsync().ConfigureAwait(false))
            {
                // var info = entity.Tag as ProcessInfo;

                if (entity.TagInfo == null)
                {
                    // 标签正好在读卡器上，读 TagInfo 一次
                    // if (TagOnReader(entity))
                    {
                        var get_result = RfidManager.GetTagInfo(entity.ReaderName, entity.UID, Convert.ToUInt32(entity.Antenna));
                        if (get_result.Value != -1)
                            entity.TagInfo = get_result.TagInfo;
                    }

                    if (entity.TagInfo == null)
                    {
                        /*
                        info.GetTagInfoError = "errorGetTagInfo";    // 表示希望获得 TagInfo
                        int count = AddErrorEntity(entity, out bool changed);
                        if (changed == true)
                            App.CurrentApp.SpeakSequence(count.ToString());
                        */
                        return new NormalResult();  // 没有执行
                    }
                }

                // 如果 RFID 标签此时正好在读卡器上，则立即触发处理
                // if (TagOnReader(entity))
                {
                    if (entity.TagInfo.EAS == enable)  // EAS 状态已经到位，不必真正修改
                    {
                        info.SetTaskInfo("changeEAS", new NormalResult());

                        info.TargetEas = null;  // 表示任务成功执行完成。后面看到 TargetEas 为 null 则不会再执行
                        // App.CurrentApp.SpeakSequence($"修改 EAS 成功: {CutTitle(entity.Title)} ");
                        return new NormalResult { Value = 1 };  // 返回成功
                    }
                    else
                    {
                        var set_result = SetEAS(entity.UID,
                            entity.Antenna,
                            enable);
                        info.SetTaskInfo("changeEAS", set_result);
                        if (set_result.Value == -1)
                        {
                            // TODO: 是否在界面显示失败？
                            // 声音提示失败
                            SoundMaker.ErrorSound();
                            SpeakSequence($"修改 EAS 失败: {CutTitle(entity.Title)} ");
                            return set_result;
                        }
                        else
                        {
                            // 修改成功后处理

                            SetTagInfoEAS(entity.TagInfo, enable);

                            // 检查 tag_data
                            if (entity.TagInfo.EAS != enable)
                                throw new Exception("EAS 修改后检查失败");

                            info.TargetEas = null;  // 表示任务成功执行完成。后面看到 TargetEas 为 null 则不会再执行
                            SpeakSequence($"修改 EAS 成功: {CutTitle(entity.Title)} ");
                            return new NormalResult { Value = 1 };  // 返回成功
                        }
                    }
                }

                return new NormalResult();  // 没有执行
            }
        }

        // 单独修改 TagInfo 里面的 AFI 和 EAS 成员
        public static void SetTagInfoEAS(TagInfo tagInfo, bool enable)
        {
            tagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;
            tagInfo.EAS = enable;
        }

        public static NormalResult SetEAS(string uid, string antenna, bool enable)
        {
            try
            {
                // testing
                // return new NormalResult { Value = -1, ErrorInfo = "修改 EAS 失败，测试" };

                if (uint.TryParse(antenna, out uint antenna_id) == false)
                    antenna_id = 0;
                var result = RfidManager.SetEAS($"{uid}", antenna_id, enable);
                if (result.Value != -1)
                {
                    // NewTagList2.SetEasData(uid, enable);
                    InventoryDialog.ClearCacheTagTable(uid);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        public static string CutTitle(string title)
        {
            if (title == null)
                return null;

            int index = title.IndexOf("/");
            if (index != -1)
                title = title.Substring(0, index).Trim();

            if (title.Length > 20)
                return title.Substring(0, 20);

            return title;
        }

        #region SIP 特殊功能

        // 限制本地数据库操作，同一时刻只能一个函数进入
        static AsyncSemaphore _cacheLimit = new AsyncSemaphore(1);

#if REMOVED
        public static XmlDocument GetInventoryDom()
        {
            string filename = Path.Combine(ClientInfo.UserDir, "inventory.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(filename);
                return dom;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
#endif

#if REMOVED
        static string _sipLocalStore = null;

        public static string SipLocalStore
        {
            get
            {
                if (_sipLocalStore == null)
                    _sipLocalStore = GetSipLocalStoreDef();

                return _sipLocalStore;
            }
        }
#endif

        /*
        static string _sipEncoding = "utf-8";

        public static string SipEncoding
        {
            get
            {
                if (_sipEncoding == null)
                    _sipEncoding = GetSipEncoding();

                return _sipEncoding;
            }
        }
        */

#if REMOVED
        // 获得 inventory.xml 中 sip/@localStore 参数
        public static string GetSipLocalStoreDef()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return "";
            var attr = dom.DocumentElement.SelectSingleNode("sip/@localStore");
            if (attr == null)
                return "";
            return attr.Value;
        }

#endif

#if REMOVED
        // 2021/4/22
        // 获得 inventory.xml 中 settings/key[@key="RPAN图书标签和层架标状态切换"] 参数
        public static bool GetRPanTagTypeSwitch()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return true;
            var value = dom.DocumentElement.SelectSingleNode("settings/key[@name='RPAN图书标签和层架标状态切换']/@value")?.Value;
            if (string.IsNullOrEmpty(value))
                value = "true";

            return value == "true";
        }
#endif

        // 获得 inventory.xml 中的 barcodeValidation/validator (OuterXml)定义
        public static string GetBarcodeValidatorDef()
        {
            /*
            var dom = GetInventoryDom();
            if (dom == null)
                return "";
            var validator = dom.DocumentElement.SelectSingleNode("barcodeValidation/validator") as XmlElement;
            if (validator == null)
                return "";
            return validator.OuterXml;
            */
            return DataModel.PiiVerifyRule.Trim(new char[] { '\r', '\n', ' ', '\t' });
        }

        public static void ClearVarcodeValidator()
        {
            _validator = null;
        }

        static BarcodeValidator _validator = null;

        public static ValidateResult ValidateBarcode(string type, string barcode)
        {
            // 无论如何，先检查是否为空
            if (string.IsNullOrEmpty(barcode))
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = "条码号不应为空"
                };

            if (_validator == null)
            {
                var def = GetBarcodeValidatorDef();
                if (string.IsNullOrEmpty(def))
                    _validator = new BarcodeValidator();
                else
                {
                    try
                    {
                        _validator = new BarcodeValidator(def);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"dp2Inventory 前端校验条码号规则 XML 定义不合法: {ex.Message}", ex);
                    }
                }
            }
            if (_validator.IsEmpty() == true)
            {
                return new ValidateResult { OK = true };
            }

            var result = _validator.ValidateByType(type, barcode);
            if (result.OK == false)
                result.ErrorInfo = $"{result.ErrorInfo} (dp2Inventory 前端校验)";
            return result;
        }

        /*
        // 获得 inventory.xml 中 sip/@encoding 参数
        public static string GetSipEncoding()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return "utf-8";
            var attr = dom.DocumentElement.SelectSingleNode("sip/@encoding");
            if (attr == null)
                return "utf-8";
            return attr.Value;
        }
        */

#if REMOVED
        // 从 inventory.xml 获得馆藏地列表(不访问 dp2library 服务器)
        // result.Value
        //      -1  出错
        //      0   文件或者列表定义没有找到
        //      1   找到
        public static GetLocationListResult sip_GetLocationListFromLocal()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return new GetLocationListResult
                {
                    Value = 0,
                    ErrorCode = "fileNotFound",
                    List = new List<string>()
                };
            var attr = dom.DocumentElement.SelectSingleNode("library/@locationList");
            if (attr == null)
                return new GetLocationListResult
                {
                    List = new List<string>()
                };

            return new GetLocationListResult
            {
                Value = 1,
                List = StringUtil.SplitList(attr.Value)
            };
        }
#endif

        // 从本地数据库中装载 uid 对照表
        public static async Task<NormalResult> LoadUidTableAsync(Hashtable uid_table,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            try
            {
                using (var releaser = await _cacheLimit.EnterAsync())
                using (var context = new ItemCacheContext())
                {
                    context.Database.EnsureCreated();
                    // var all = context.Uids.Where(o => string.IsNullOrEmpty(o.PII) == false && string.IsNullOrEmpty(o.UID) == false);
                    foreach (var item in context.Uids)
                    {
                        if (token.IsCancellationRequested)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = "中断"
                            };

                        string uid = item.UID;
                        string barcode = item.PII;

                        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(barcode))
                            continue;

                        // 2021/1/31
                        // 跳过那些没有 OI 的
                        ParseOiPii(barcode, out string pii, out string oi);
                        if (string.IsNullOrEmpty(oi))
                            continue;

                        func_showProgress?.Invoke($"{uid} --> {barcode} ...", -1, -1);

                        uid_table[uid] = barcode;
                    }

                    return new NormalResult
                    {
                        Value = uid_table.Count,
                    };
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"LoadUidTable() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"LoadUidTable() 出现异常：{ex.Message}"
                };
            }
        }

        public static async Task<BookItem> FindBookItemAsync(string barcode)
        {
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                return context.Items.Where(o => o.Barcode == barcode).FirstOrDefault();
            }
        }

        // 请求设置 UID 到本地数据库
        public static async Task<RequestSetUidResult> RequestSetUIDtoLocalAsync(
    string uid,
    string pii,
    string oi,
    string style)
        {
            // TODO: 可以先检查 hashtable 中是否有了，有了则表示不用加入本地数据库了，这样可以优化速度

            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                UpdateUidEntry(context, MakeOiPii(pii, oi), uid);
            }

            return new RequestSetUidResult
            {
                Value = 1,
            };
        }

        public static void ParseOiPii(string text,
            out string pii,
            out string oi)
        {
            pii = "";
            oi = "";

            if (string.IsNullOrEmpty(text))
                return;

            if (text.Contains(".") == false)
            {
                pii = text;
                oi = "";
                return;
            }

            var parts = StringUtil.ParseTwoPart(text, ".");
            oi = parts[0];
            pii = parts[1];
        }

        public static string MakeOiPii(string pii, string oi)
        {
            if (string.IsNullOrEmpty(oi))
                return pii;
            return oi + "." + pii;
        }

        // 将原本要向 SIP2 服务器发出盘点请求写入本地(映射)数据库
        public static async Task<RequestInventoryResult> RequestInventory_local(
            string item_xml,
            string uid,
            string oi_pii,
            string currentLocationString,
            string location,
            string shelfNo,
            string batchNo,
            string strUserName,
            string style)
        {
            if (currentLocationString == null && location == null)
                return new RequestInventoryResult { Value = 0 };    // 没有必要修改

            if (string.IsNullOrEmpty(item_xml))
                return new RequestInventoryResult
                {
                    Value = -1,
                    ErrorInfo = "未提供册记录 XML，无法进行盘点写入操作"
                };

            string currentLocation = null;
            string currentShelfNo = null;

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(item_xml);
            }
            catch (Exception ex)
            {
                return new RequestInventoryResult
                {
                    Value = -1,
                    ErrorInfo = $"册记录 XML 解析异常: {ex.Message}"
                };
            }

            string title = DomUtil.GetElementText(dom.DocumentElement, "title");

            // 保存册记录和日志到本地数据库
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                var item = context.Items.Where(o => o.Barcode == oi_pii).FirstOrDefault();
                if (item == null)
                {
                    item = new BookItem
                    {
                        Title = title,
                        Barcode = oi_pii,
                        // UID = uid,
                        CurrentLocation = currentLocation,
                        CurrentShelfNo = currentShelfNo,
                        Location = location,
                        ShelfNo = shelfNo,
                        InventoryTime = DateTime.Now,
                    };
                    await context.Items.AddAsync(item);
                }
                else
                {
                    /*
                    if (string.IsNullOrEmpty(uid) == false)
                        item.UID = uid;
                    */
                    if (currentLocation != null)
                        item.CurrentLocation = currentLocation;
                    if (currentShelfNo != null)
                        item.CurrentShelfNo = currentShelfNo;
                    if (location != null)
                        item.Location = location;
                    if (shelfNo != null)
                        item.ShelfNo = shelfNo;
                    item.InventoryTime = DateTime.Now;
                    context.Items.Update(item);
                }
                // await context.SaveChangesAsync();

                // 2021/1/19
                // 写入本地操作日志库
                await context.Logs.AddAsync(new InventoryLogItem
                {
                    Title = title,
                    Barcode = oi_pii,
                    Location = location,
                    ShelfNo = shelfNo,
                    CurrentLocation = currentLocation,
                    CurrentShelfNo = currentShelfNo,
                    WriteTime = DateTime.Now,
                    BatchNo = batchNo,
                });
                await context.SaveChangesAsync();
            }

            // TODO: 修改 XML

            if (location != null)
                DomUtil.SetElementText(dom.DocumentElement,
                    "location",
                    location);
            if (shelfNo != null)
                DomUtil.SetElementText(dom.DocumentElement,
    "shelfNo",
    shelfNo);

            // currentLocation
            // 取出以前的值，然后按照冒号左右分别按需替换
            string oldCurrentLocationString = DomUtil.GetElementText(dom.DocumentElement, "currentLocation");
            string newCurrentLocationString = ReplaceCurrentLocationString(oldCurrentLocationString, currentLocation, currentShelfNo);
            if (oldCurrentLocationString != newCurrentLocationString)
                DomUtil.SetElementText(dom.DocumentElement,
                    "currentLocation",
                    newCurrentLocationString);

            return new RequestInventoryResult { ItemXml = dom.DocumentElement.OuterXml };
        }

        static string ReplaceCurrentLocationString(string currentLocationString,
            string newCurrentLocation,
            string newCurrentShelfNo)
        {
            string currentLocation = "";
            string currentShelfNo = "";

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }

            if (newCurrentLocation != null)
                currentLocation = newCurrentLocation;
            if (newCurrentShelfNo != null)
                currentShelfNo = newCurrentShelfNo;

            return currentLocation + ":" + currentShelfNo;
        }

        // TODO: 同时进入 hashtable
        // 导入 UID PII 对照表文件
        public static async Task<ImportUidResult> ImportUidPiiTableAsync(
            string filename,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            bool sip = DataModel.Protocol == "sip";
            try
            {
                long total_linecount = 0;
                // 先统计总行数
                using (var reader = new StreamReader(filename, Encoding.ASCII))
                {
                    while (token.IsCancellationRequested == false)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null)
                            break;
                        total_linecount++;
                    }
                }

                using (var reader = new StreamReader(filename, Encoding.ASCII))
                {
                    int line_count = 0;
                    int new_count = 0;
                    int change_count = 0;
                    int delete_count = 0;
                    int error_count = 0;

                    List<string> lines = new List<string>();
                    int i = 0;
                    string processed_line = "";
                    while (token.IsCancellationRequested == false)
                    {
                        if (total_linecount > 0)
                            func_showProgress?.Invoke($"正在导入 {i}/{total_linecount} {processed_line.Replace("\t", "-->")}", i, total_linecount);
                        i++;
                        var line = await reader.ReadLineAsync();
                        if (line == null)
                            break;
                        if (string.IsNullOrEmpty(line))
                            continue;

                        processed_line = line;  // .TrimEnd(new char[] { '\r', '\n' });

                        var parts = StringUtil.ParseTwoPart(line, "\t");
                        string uid = parts[0].Trim(new char[] {' '});
                        string barcode = parts[1].Trim(new char[] { ' ' });
                        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(barcode))
                        {
                            // error_count++;
                            // continue;
                            return new ImportUidResult
                            {
                                ErrorInfo = $"出现了不合法的行 '{line}'，导入过程出错",
                                Value = -1,
                            };
                        }

                        // 2021/4/1
                        ParseOiPii(barcode, out string pii, out string oi);
                        if (string.IsNullOrEmpty(oi))
                            return new ImportUidResult
                            {
                                ErrorInfo = $"出现了没有 OI 的行 '{line}'，导入过程出错",
                                Value = -1,
                            };

                        if (sip == false)
                        {
                            // .Value
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            var get_result = await LibraryChannelUtil.GetEntityDataAsync(barcode,
                                "network,skip_biblio");
                            if (get_result.Value == -1)
                                return new ImportUidResult
                                {
                                    ErrorInfo = $"GetEntityDataAsync() error: {get_result.ErrorInfo}",
                                    Value = -1,
                                };
                            if (get_result.Value == 0)
                            {
                                ClientInfo.WriteErrorLog($"ImportUidPiiTableAsync() dp2library 服务器中册记录 '{barcode}' 没有找到: {get_result.ErrorInfo}");
                                error_count++;
                                continue;
                            }
                            var set_result = RequestSetUID(
    get_result.ItemRecPath,
    get_result.ItemXml,
    get_result.ItemTimestamp,
    uid,
    null,
    "");
                            if (set_result.Value == -1)
                            {
                                ClientInfo.WriteErrorLog($"ImportUidPiiTableAsync() 中 RequestSetUID(itemRecPath={get_result.ItemRecPath},barcode={barcode},uid={uid}) error: {set_result.ErrorInfo}");
                                error_count++;
                                continue;
                            }
                            if (set_result.Value == 1)
                                change_count++;
                            line_count++;
                            continue;
                        }

                        lines.Add(line);
                        if (lines.Count > 100)
                        {
                            var result = await SaveLinesAsync(lines, token);
                            lines.Clear();

                            line_count += result.LineCount;
                            new_count += result.NewCount;
                            change_count += result.ChangeCount;
                            delete_count += result.DeleteCount;
                        }
                    }
                    if (lines.Count > 0)
                    {
                        var result = await SaveLinesAsync(lines, token);
                        line_count += result.LineCount;
                        new_count += result.NewCount;
                        change_count += result.ChangeCount;
                        delete_count += result.DeleteCount;
                    }

                    return new ImportUidResult
                    {
                        Value = line_count,
                        LineCount = line_count,
                        NewCount = new_count,
                        ChangeCount = change_count,
                        DeleteCount = delete_count,
                        ErrorCount = error_count,
                    };
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ImportUidPiiTable() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new ImportUidResult
                {
                    Value = -1,
                    ErrorInfo = $"ImportUidPiiTable() 出现异常: {ex.Message}"
                };
            }
        }

        public class ImportUidResult : NormalResult
        {
            public int LineCount { get; set; }
            public int NewCount { get; set; }
            public int ChangeCount { get; set; }
            public int DeleteCount { get; set; }
            public int ErrorCount { get; set; }
        }

#if REMOVED
        // TODO: 同时进入 hashtable
        // 导入 UID PII 对照表文件
        public static async Task<ImportUidResult> ImportUidPiiTableAsync(
            string filename,
            CancellationToken token)
        {
            bool sip = DataModel.Protocol == "sip";
            try
            {
                using (var reader = new StreamReader(filename, Encoding.ASCII))
                {
                    int line_count = 0;
                    int new_count = 0;
                    int change_count = 0;
                    int delete_count = 0;
                    List<string> lines = new List<string>();
                    while (token.IsCancellationRequested == false)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null)
                            break;
                        if (string.IsNullOrEmpty(line))
                            continue;

                        var parts = StringUtil.ParseTwoPart(line, "\t");
                        string uid = parts[0];
                        string barcode = parts[1];
                        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(barcode))
                            continue;

                        // 2021/4/1
                        ParseOiPii(barcode, out string pii, out string oi);
                        if (string.IsNullOrEmpty(oi))
                            return new ImportUidResult
                            {
                                ErrorInfo = $"出现了没有 OI 的行 '{line}'，导入过程出错",
                                Value = -1,
                            };

                        if (sip == false)
                        {
                            // .Value
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            var get_result = await LibraryChannelUtil.GetEntityDataAsync(barcode,
                                "network,skip_biblio");
                            if (get_result.Value == -1)
                                return new ImportUidResult
                                {
                                    ErrorInfo = $"GetEntityDataAsync() error: {get_result.ErrorInfo}",
                                    Value = -1,
                                };
                            if (get_result.Value == 0)
                                return new ImportUidResult
                                {
                                    ErrorInfo = $"册记录 {uid} 没有找到: {get_result.ErrorInfo}",
                                    Value = -1,
                                };
                            var set_result = RequestSetUID(
    get_result.ItemRecPath,
    get_result.ItemXml,
    get_result.ItemTimestamp,
    uid,
    null,
    "");
                            if (set_result.Value == -1)
                                return new ImportUidResult
                                {
                                    ErrorInfo = $"RequestSetUID() error: {set_result.ErrorInfo}",
                                    Value = -1,
                                };
                            if (set_result.Value == 1)
                                change_count++;
                            line_count++;
                            continue;
                        }

                        lines.Add(line);
                        if (lines.Count > 100)
                        {
                            var result = await SaveLinesAsync(lines, token);
                            lines.Clear();

                            line_count += result.LineCount;
                            new_count += result.NewCount;
                            change_count += result.ChangeCount;
                            delete_count += result.DeleteCount;
                        }
                    }
                    if (lines.Count > 0)
                    {
                        var result = await SaveLinesAsync(lines, token);
                        line_count += result.LineCount;
                        new_count += result.NewCount;
                        change_count += result.ChangeCount;
                        delete_count += result.DeleteCount;
                    }

                    return new ImportUidResult
                    {
                        Value = line_count,
                        LineCount = line_count,
                        NewCount = new_count,
                        ChangeCount = change_count,
                        DeleteCount = delete_count,
                    };
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"ImportUidPiiTable() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new ImportUidResult
                {
                    Value = -1,
                    ErrorInfo = $"ImportUidPiiTable() 出现异常: {ex.Message}"
                };
            }
        }

        public class ImportUidResult : NormalResult
        {
            public int LineCount { get; set; }
            public int NewCount { get; set; }
            public int ChangeCount { get; set; }
            public int DeleteCount { get; set; }
        }

#endif

        static async Task<ImportUidResult> SaveLinesAsync(List<string> lines,
            CancellationToken token)
        {
            int line_count = 0;
            int new_count = 0;
            int change_count = 0;
            int delete_count = 0;
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                context.Database.EnsureCreated();

                foreach (var line in lines)
                {
                    if (token.IsCancellationRequested)
                        return new ImportUidResult
                        {
                            LineCount = line_count,
                            NewCount = new_count,
                            ChangeCount = change_count,
                            DeleteCount = delete_count,
                        };

                    if (string.IsNullOrEmpty(line))
                        continue;
                    var parts = StringUtil.ParseTwoPart(line, "\t");
                    string uid = parts[0];
                    string barcode = parts[1];
                    if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(barcode))
                        continue;

                    var result = UpdateUidEntry(context, barcode, uid);
                    new_count += result.NewCount;
                    change_count += result.ChangeCount;
                    delete_count += result.DeleteCount;

                    line_count++;
                    await context.SaveChangesAsync();
                }
            }
            return new ImportUidResult
            {
                LineCount = line_count,
                NewCount = new_count,
                ChangeCount = change_count,
                DeleteCount = delete_count,
            };
        }

        static ImportUidResult UpdateUidEntry(ItemCacheContext context,
            string barcode,
            string uid)
        {
            int new_count = 0;
            int change_count = 0;
            int delete_count = 0;
            {
                // TODO:
                var item = context.Uids.Where(o => o.PII == barcode).FirstOrDefault();
                if (item == null)
                {
                    item = new UidEntry { PII = barcode, UID = uid };
                    context.Uids.Add(item);
                    new_count++;
                }
                else if (item.UID != uid)
                {
                    item.UID = uid;
                    context.Uids.Update(item);
                    change_count++;
                }
            }

            // 删除其余用到这个 UID 的字段
            {
                var items = context.Uids.Where(o => o.UID == uid && o.PII != barcode).ToList();
                foreach (var item in items)
                {
                    context.Uids.Remove(item);
                    /*
                    item.UID = null;
                    context.Uids.Update(item);
                    */
                    delete_count++;
                }
            }

            if (new_count > 0 || change_count > 0 || delete_count > 0)
                context.SaveChanges();

            return new ImportUidResult
            {
                NewCount = new_count,
                ChangeCount = change_count,
                DeleteCount = delete_count,
            };
        }

        // 清除本地数据库中的 UID --> PII 对照关系
        public static async Task<NormalResult> ClearUidPiiLocalCacheAsync(CancellationToken token)
        {
            int change_count = 0;
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                context.Database.EnsureCreated();

                var list = context.Uids.ToList();
                change_count += list.Count;
                if (change_count > 0)
                {
                    context.Uids.RemoveRange(list);
                    await context.SaveChangesAsync(token);
                }
            }
            return new NormalResult
            {
                Value = change_count,
            };

            /*
            void Save(ItemCacheContext context,
                List<BookItem> items)
            {
                context.Items.UpdateRange(items);
                context.SaveChanges();
                foreach (var item in items)
                {
                    context.Entry(item).State = EntityState.Detached;
                }
            }
            */
        }

        static void FillBookItem(BookItem item, string item_xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(item_xml);

            item.Barcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            item.Xml = item_xml;
            item.Location = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            item.ShelfNo = DomUtil.GetElementText(dom.DocumentElement,
                "shelfNo");

            string currentLocationString = DomUtil.GetElementText(dom.DocumentElement,
                "currentLocation");
            item.CurrentLocation = "";  // 左侧
            item.CurrentShelfNo = "";   // 右侧

            /*
            item.UID = DomUtil.GetElementText(dom.DocumentElement,
                "uid");
            */
        }

        public static NormalResult ExportToExcel(
            List<InventoryColumn> columns,
            List<Entity> items,
            string fileName,
            bool launch,
            CancellationToken token)
        {
            if (items == null || items.Count == 0)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "items == null || items.Count == 0"
                };
            }

            if (string.IsNullOrEmpty(fileName) == true)
            {
            }

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    // 询问文件名
                    SaveFileDialog dlg = new SaveFileDialog
                    {
                        Title = "请指定要输出的 Excel 文件名",
                        CreatePrompt = false,
                        OverwritePrompt = true,
                        // dlg.FileName = this.ExportExcelFilename;
                        // dlg.InitialDirectory = Environment.CurrentDirectory;
                        Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",

                        RestoreDirectory = true
                    };

                    if (dlg.ShowDialog() == DialogResult.Cancel)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "放弃",
                            ErrorCode = "cancel"
                        };
                    fileName = dlg.FileName;
                }

                XLWorkbook doc = null;

                try
                {
                    doc = new XLWorkbook(XLEventTracking.Disabled);
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"创建 Excel 文件时出现异常:{ex.Message}"
                    };
                }

                IXLWorksheet sheet = null;
                sheet = doc.Worksheets.Add("表格");
                // sheet.Style.Font.FontName = this.Font.Name;

                // 每个列的最大字符数
                List<int> column_max_chars = new List<int>();

                List<XLAlignmentHorizontalValues> alignments = new List<XLAlignmentHorizontalValues>();
                foreach (var header in columns)
                {
                    if (header.TextAlign == "center")
                        alignments.Add(XLAlignmentHorizontalValues.Center);
                    else if (header.TextAlign == "right")
                        alignments.Add(XLAlignmentHorizontalValues.Right);
                    else
                        alignments.Add(XLAlignmentHorizontalValues.Left);

                    column_max_chars.Add(0);
                }

                Debug.Assert(alignments.Count == columns.Count, "");

                // string strFontName = list.Font.FontFamily.Name;

                int nRowIndex = 1;
                int nColIndex = 1;
                foreach (var header in columns)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(header.Caption, '*'));
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    // cell.Style.Font.FontName = strFontName;
                    cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                }
                nRowIndex++;

                foreach (var item in items)
                {
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "中断",
                            ErrorCode = "cancel"
                        };

                    nColIndex = 1;
                    foreach (var column in columns)
                    {
                        string value = GetPropertyOrField(item, column.Property);

                        // 统计最大字符数
                        // int nChars = column_max_chars[nColIndex - 1];
                        if (value != null)
                        {
                            SetMaxChars(/*ref*/ column_max_chars, nColIndex - 1, value.Length);
                        }
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(value, '*'));
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // cell.Style.Font.FontName = strFontName;
                        // 2020/1/6 增加保护代码
                        if (nColIndex - 1 < alignments.Count)
                            cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                        else
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        nColIndex++;
                    }

                    nRowIndex++;
                }

                /*
                double char_width = GetAverageCharPixelWidth(list);

                // 字符数太多的列不要做 width auto adjust
                const int MAX_CHARS = 30;   // 60
                int i = 0;
                foreach (IXLColumn column in sheet.Columns())
                {
                    // int nChars = column_max_chars[i];
                    int nChars = GetMaxChars(column_max_chars, i);

                    if (nChars < MAX_CHARS)
                        column.AdjustToContents();
                    else
                    {
                        int nColumnWidth = 100;
                        // 2020/1/6 增加保护判断
                        if (i >= 0 && i < list.Columns.Count)
                            nColumnWidth = list.Columns[i].Width;
                        column.Width = (double)nColumnWidth / char_width;  // Math.Min(MAX_CHARS, nChars);
                    }
                    i++;
                }
                */

                sheet.Columns().AdjustToContents();

                // sheet.Rows().AdjustToContents();

                doc.SaveAs(fileName);
                doc.Dispose();

                if (launch)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(fileName);
                    }
                    catch
                    {
                    }
                }

                return new NormalResult();
            }
            finally
            {
            }
        }

        static string GetPropertyOrField(object obj, string name)
        {
            var pi = obj.GetType().GetProperty(name);
            if (pi != null)
                return pi.GetValue(obj)?.ToString();

            var fi = obj.GetType().GetField(name);
            if (fi == null)
                return null;
            return fi.GetValue(obj)?.ToString();
        }

        public static int GetMaxChars(List<int> column_max_chars, int index)
        {
            if (index < 0)
                throw new ArgumentException($"index 参数必须大于等于零 (而现在是 {index})");

            if (index >= column_max_chars.Count)
                return 0;
            return column_max_chars[index];
        }

        public static void SetMaxChars(/*ref*/ List<int> column_max_chars, int index, int chars)
        {
            // 确保空间足够
            while (column_max_chars.Count < index + 1)
            {
                column_max_chars.Add(0);
            }

            // 统计最大字符数
            int nOldChars = column_max_chars[index];
            if (chars > nOldChars)
            {
                column_max_chars[index] = chars;
            }
        }


        public class InventoryColumn
        {
            // 列名
            public string Caption { get; set; }

            // 要导出的数据成员名
            public string Property { get; set; }

            // 文字对齐方向
            public string TextAlign { get; set; }   // left/right/center。 默认 left
        }

        // 导出所有的本地册记录到 Excel 文件
        // result.Value
        //      -1  出错
        //      >=0  共导出多少行
        public static async Task<NormalResult> ExportAllItemToExcelAsync(
            List<InventoryColumn> columns,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            try
            {
                // 询问文件名
                SaveFileDialog dlg = new SaveFileDialog
                {
                    Title = "请指定要输出的 Excel 文件名",
                    CreatePrompt = false,
                    OverwritePrompt = true,
                    // dlg.FileName = this.ExportExcelFilename;
                    // dlg.InitialDirectory = Environment.CurrentDirectory;
                    Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",

                    RestoreDirectory = true
                };

                if (dlg.ShowDialog() == DialogResult.Cancel)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "放弃",
                        ErrorCode = "cancel"
                    };

                XLWorkbook doc = null;

                try
                {
                    doc = new XLWorkbook(XLEventTracking.Disabled);
                    File.Delete(dlg.FileName);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"创建 Excel 文件时出现异常:{ex.Message}"
                    };
                }

                IXLWorksheet sheet = null;
                sheet = doc.Worksheets.Add("表格");

                // 每个列的最大字符数
                List<int> column_max_chars = new List<int>();

                List<XLAlignmentHorizontalValues> alignments = new List<XLAlignmentHorizontalValues>();
                foreach (var header in columns)
                {
                    if (header.TextAlign == "center")
                        alignments.Add(XLAlignmentHorizontalValues.Center);
                    else if (header.TextAlign == "right")
                        alignments.Add(XLAlignmentHorizontalValues.Right);
                    else
                        alignments.Add(XLAlignmentHorizontalValues.Left);

                    column_max_chars.Add(0);
                }

                Debug.Assert(alignments.Count == columns.Count, "");

                // string strFontName = list.Font.FontFamily.Name;

                int nRowIndex = 1;
                int nColIndex = 1;
                foreach (var header in columns)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(header.Caption, '*'));
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    // cell.Style.Font.FontName = strFontName;
                    cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                }
                nRowIndex++;

                List<string> barcodes = null;
                using (var releaser = await _cacheLimit.EnterAsync())
                using (var context = new ItemCacheContext())
                {
                    context.Database.EnsureCreated();

                    barcodes = context.Items.Select(o => o.Barcode).ToList();
                }

                int count = 0;
                foreach (var barcode in barcodes)
                {
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "中断",
                            ErrorCode = "cancel"
                        };

                    if (string.IsNullOrEmpty(barcode))
                        continue;

                    func_showProgress?.Invoke($"正在导出 {barcode} ...", -1, -1);

                    ParseOiPii(barcode, out string pii, out string oi);

                    // 检查册记录是否存在
                    var result = await SipChannelUtil.GetEntityDataAsync(pii,
                        oi,
                        "network,localInventory,updateItemTitle");
                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == "itemNotFound")
                            continue;
                        return result;
                    }

                    var item = result.BookItem;
                    if (item == null)
                        continue;

                    if (string.IsNullOrEmpty(result.ItemXml))
                        continue;
                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        itemdom.LoadXml(result.ItemXml);
                    }
                    catch
                    {
                        continue;
                    }

                    /*
                    string item_barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
                    if (string.IsNullOrEmpty(item_barcode))
                        continue;
                    */

                    // 把 currentLocation 调整为 currentLocation 和 currentShelfNo
                    {
                        string currentLocationString = DomUtil.GetElementText(itemdom.DocumentElement, "currentLocation");
                        var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                        DomUtil.SetElementText(itemdom.DocumentElement, "currentLocation", parts[0]);
                        DomUtil.SetElementText(itemdom.DocumentElement, "currentShelfNo", parts[1]);
                    }

                    {
                        // XML 记录中的 state 元素要转化为界面显示的状态值，然后用于导出 
                        Entity entity = new Entity();
                        entity.SetData(null, result.ItemXml);
                        DomUtil.SetElementText(itemdom.DocumentElement, "state", entity.State);
                    }

                    nColIndex = 1;
                    foreach (var column in columns)
                    {
                        string value = GetPropertyOrField(item, column.Property);
                        if (value == null)
                        {
                            value = DomUtil.GetElementText(itemdom.DocumentElement, camel(column.Property));
                            if (string.IsNullOrEmpty(value) == false)
                                value = $"({value})";
                        }

                        // 统计最大字符数
                        // int nChars = column_max_chars[nColIndex - 1];
                        if (value != null)
                        {
                            SetMaxChars(/*ref*/ column_max_chars, nColIndex - 1, value.Length);
                        }
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(value, '*'));
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        // cell.Style.Font.FontName = strFontName;
                        // 2020/1/6 增加保护代码
                        if (nColIndex - 1 < alignments.Count)
                            cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                        else
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        nColIndex++;
                    }

                    nRowIndex++;
                    count++;
                }

                sheet.Columns().AdjustToContents();

                doc.SaveAs(dlg.FileName);
                doc.Dispose();

                try
                {
                    System.Diagnostics.Process.Start(dlg.FileName);
                }
                catch
                {
                }

                return new NormalResult { Value = count };
            }
            finally
            {
            }

            string camel(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;
                return char.ToLower(text[0]) + text.Substring(1);
            }
        }

        #endregion

        // 向 SIP2 服务器发出盘点请求
        // 注意，不会考虑本地缓存的盘点信息
        public static async Task<RequestInventoryResult> RequestInventory_sip2(
            string uid,
            string pii,
            string oi,
            string currentLocationString,
            string location,
            string shelfNo,
            string batchNo,
            string strUserName,
            string style)
        {
            if (currentLocationString == null && location == null)
                return new RequestInventoryResult { Value = 0 };    // 没有必要修改

            string currentLocation = null;
            string currentShelfNo = null;

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }
            var update_result = await SipChannelUtil.UpdateItemStatusAsync(
    oi,
    pii,
    location,
    currentLocation,
    shelfNo,
    currentShelfNo);
            if (update_result.Value == -1)
                return new RequestInventoryResult
                {
                    Value = -1,
                    ErrorInfo = update_result.ErrorInfo,
                    ErrorCode = update_result.ErrorCode
                };

            // 重新获得册记录 XML
            var get_result = await SipChannelUtil.GetEntityDataAsync(pii,
                oi,
                "network");
            if (get_result.Value == -1)
            {
                // TODO: 如何报错？
            }
            return new RequestInventoryResult { ItemXml = get_result.ItemXml };
        }

        #region 上传接口

        public class UploadInterfaceInfo
        {
            public string BaseUrl { get; set; }
            public string Protocol { get; set; }
        }

        public static UploadInterfaceInfo GetUploadInterface()
        {
            var url = DataModel.uploadInterfaceUrl;
            if (string.IsNullOrEmpty(url))
                return null;
            return new UploadInterfaceInfo
            {
                BaseUrl = url,
                Protocol = "",
            };
        }

#if REMOVED
        // 获得 inventory.xml 中 uploadInterface 参数
        public static UploadInterfaceInfo GetUploadInterface()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return null;
            var uploadInterface = dom.DocumentElement.SelectSingleNode("uploadInterface") as XmlElement;
            if (uploadInterface == null)
                return null;
            return new UploadInterfaceInfo
            {
                BaseUrl = uploadInterface.GetAttribute("baseUrl"),
                Protocol = uploadInterface.GetAttribute("protocol")
            };
        }

        static UploadInterfaceInfo _uploadInterfaceInfo = null;
#endif

        // 利用 uploadInterface 发出盘点请求
        public static async Task<RequestInventoryResult> RequestInventoryUploadAsync(
            string item_xml,
            string uid,
            string oi_pii,
            string currentLocationString,
            string location,
            string shelfNo,
            string batchNo,
            string strUserName,
            string style)
        {
            if (currentLocationString == null && location == null)
                return new RequestInventoryResult { Value = 0 };    // 没有必要修改

            var _uploadInterfaceInfo = GetUploadInterface();
            /*
            if (_uploadInterfaceInfo == null)
            {
                _uploadInterfaceInfo = GetUploadInterface();
                if (_uploadInterfaceInfo == null)
                {
                    _uploadInterfaceInfo = new UploadInterfaceInfo { BaseUrl = null };
                }
            }
            */

            if (_uploadInterfaceInfo == null
                || _uploadInterfaceInfo.BaseUrl == null)
                return new RequestInventoryResult
                {
                    Value = 0,
                    ErrorInfo = "没有定义 uploadInterface 接口"
                };

            // currentLocation 元素内容。格式为 馆藏地:架号
            string currentLocation = null;
            string currentShelfNo = null;

            if (currentLocationString != null)
            {
                // 分解 currentLocation 字符串
                var parts = StringUtil.ParseTwoPart(currentLocationString, ":");
                currentLocation = parts[0];
                currentShelfNo = parts[1];
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(item_xml) == false)
                    dom.LoadXml(item_xml);
                else
                    dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                return new RequestInventoryResult
                {
                    Value = -1,
                    ErrorInfo = $"册记录 XML 解析异常: {ex.Message}"
                };
            }

            string title = DomUtil.GetElementText(dom.DocumentElement, "title");

            UploadItem record = new UploadItem
            {
                title = title,
                uii = oi_pii,
                barcode = InventoryDialog.GetPiiPart(oi_pii),
                batchNo = batchNo,
                shelfNo = shelfNo,
                currentShelfNo = currentShelfNo,
                location = location,
                currentLocation = currentLocation,
                operatorPerson = strUserName,
                operatorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")
            };
            string data = JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);

            var item = new Item
            {
                Action = "update",
                Format = "json",
                Data = data,
            };
            var request = new SetItemsRequest { Items = new List<Item>() { item } };

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_uploadInterfaceInfo.BaseUrl);
                    var client = new InventoryAPIV1Client(httpClient);
                    var result = await client.SetItemsAsync(request);
                    if (result.Result == null)
                        return new RequestInventoryResult
                        {
                            Value = -1,
                            ErrorInfo = "upload error: result.Result == null"
                        };
                    // 注: result.Value 如果 >=0，一定是完全成功。如果是部分成功，.Value 应该是 -1
                    if (result.Result.Value < 0)
                        return new RequestInventoryResult
                        {
                            Value = (int)result.Result.Value,
                            ErrorInfo = result.Result.ErrorInfo,
                            ErrorCode = result.Result.ErrorCode
                        };
                    return new RequestInventoryResult { ItemXml = null };
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"RequestInventoryUploadAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");
                return new RequestInventoryResult
                {
                    Value = -1,
                    ErrorInfo = $"RequestInventoryUploadAsync() 出现异常：{ex.Message}"
                };
            }
        }

        public class UploadItem
        {
            public string title { get; set; }
            public string batchNo { get; set; }
            public string uii { get; set; }     // 格式为 OI.PII
            public string barcode { get; set; } // PII
            public string location { get; set; }
            public string shelfNo { get; set; }
            public string currentLocation { get; set; }
            public string currentShelfNo { get; set; }
            public string operatorPerson { get; set; }
            public string operatorTime { get; set; }    // 时间格式为 "yyyy-MM-dd HH:mm:ss.ffff"
        }

        #endregion
    }
}
