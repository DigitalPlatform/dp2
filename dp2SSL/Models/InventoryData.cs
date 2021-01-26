using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Microsoft.VisualStudio.Threading;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;
using static dp2SSL.LibraryChannelUtil;

namespace dp2SSL
{
    /// <summary>
    /// 和盘点有关的数据结构
    /// </summary>
    public static class InventoryData
    {
        // 预先从全部实体记录中准备好 UID 到 PII 的对照关系。这一部分标签就不需要 GetTagData 了
        // UID --> PII
        static Hashtable _uidTable = new Hashtable();

        public static void SetUidTable(Hashtable table)
        {
            _uidTable = table;
        }

        // 检查是否存在 UID --> PII 事项
        public static bool UidExsits(string uid, out string pii)
        {
            pii = (string)_uidTable[uid];
            if (string.IsNullOrEmpty(pii) == false)
            {
                return true;
            }
            return false;
        }

        // 清除所有列表
        public static void Clear()
        {
            _uidTable.Clear();
            _entityTable.Clear();
            RemoveList(null);
            _errorEntities.Clear();

            // 清除 _tags 中的所有内容
            NewTagList2.Clear();
        }

        // UID --> entity
        static Hashtable _entityTable = new Hashtable();

        public static void RemoveEntity(Entity entity)
        {
            _entityTable.Remove(entity.UID);
        }

        public static Entity AddEntity(TagAndData tag, out bool isNewly)
        {
            if (_entityTable.ContainsKey(tag.OneTag.UID))
            {
                // TODO: 更新 tagInfo
                isNewly = false;
                Entity result = _entityTable[tag.OneTag.UID] as Entity;
                InventoryData.NewEntity(tag, result, false);
                return result;
            }

            var entity = InventoryData.NewEntity(tag, null, false);
            _entityTable[entity.UID] = entity;
            isNewly = true;
            return entity;
        }

        public static void UpdateEntity(Entity entity,
            TagInfo tagInfo,
            out string type)
        {
            type = "";

            entity.TagInfo = tagInfo;

            bool throw_exception = false;
            LogicChip chip = null;
            // string type = "";
            if (string.IsNullOrEmpty(type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                try
                {
                    ParseTagInfo(tagInfo,
out string pii,
out type,
out chip);
                    if (tagInfo != null)
                        entity.PII = pii;

                    entity.BuildError("parseTag", null, null);
                }
                catch (Exception ex)
                {
                    App.CurrentApp.SpeakSequence("警告: 标签解析出错");
                    if (throw_exception == false)
                    {
                        entity.BuildError(
                            "parseTag",
                            $"RFID 标签格式错误: {ex.Message}",
                            "parseTagError");
                    }
                    else
                        throw ex;
                }
            }

            if (type == "location")
            {
                entity.Title = $"(层架标) {entity.PII}";
            }

            // 2020/7/15
            // 获得图书 RFID 标签的 OI 和 AOI 字段
            if (type == "book")
            {
                if (chip == null)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    chip = LogicChip.From(tagInfo.Bytes,
            (int)tagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                }

                if (chip.IsBlank())
                {
                    entity.BuildError("checkTag", "空白标签", "blankTag");
                }
                else
                {
                    string oi = chip.FindElement(ElementOID.OI)?.Text;
                    string aoi = chip.FindElement(ElementOID.AOI)?.Text;

                    entity.OI = oi;
                    entity.AOI = aoi;

                    // 2020/8/27
                    // 严格要求必须有 OI(AOI) 字段
                    if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                        entity.BuildError("checkTag", "没有 OI 或 AOI 字段", "missingOI");
                    else
                        entity.BuildError("checkTag", null, null);
                }
            }
            else if (type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                entity.PII = "(读者卡)" + entity.PII;
                entity.BuildError("checkTag", "读者卡误放入书架", "patronCard");
                App.CurrentApp.SpeakSequence("读者卡误放入书架");
            }
            else
                entity.BuildError("checkTag", null, null);
        }

        // 解析标签内容，返回 PII 和 typeOfUsage。注：typeOfUsage ‘30’ 表示层架标
        static void ParseTagInfo(TagInfo tagInfo,
    out string pii,
    out string type,
    out LogicChip chip)
        {
            pii = null;
            chip = null;
            type = "";

            if (tagInfo == null)
                return;

            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            chip = LogicChip.From(tagInfo.Bytes,
    (int)tagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
            pii = chip.FindElement(ElementOID.PII)?.Text;

            var typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
            if (typeOfUsage == "30")
                type = "location";  // 层架标 2020/11/5
            else if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                type = "patron";
            else
                type = "book";
        }

        // 注：所创建的 Entity 对象其 Error 成员可能有值，表示有出错信息
        // Exception:
        //      可能会抛出异常 ArgumentException
        static Entity NewEntity(TagAndData tag,
            Entity entity,
            bool throw_exception = true)
        {
            Entity result = entity;
            if (result == null)
            {
                result = new Entity
                {
                    UID = tag.OneTag.UID,
                    ReaderName = tag.OneTag.ReaderName,
                    Antenna = tag.OneTag.AntennaID.ToString(),
                    TagInfo = tag.OneTag.TagInfo,
                };
            }

            LogicChip chip = null;
            if (string.IsNullOrEmpty(tag.Type))
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                try
                {
                    SetTagType(tag, out string pii, out chip);
                    if (tag.OneTag.TagInfo != null)
                        result.PII = pii;
                    else
                        result.PII = null;  // 2021/1/26

                    result.BuildError("parseTag", null, null);
                }
                catch (Exception ex)
                {
                    App.CurrentApp.SpeakSequence("警告: 标签解析出错");
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

#if NO
            // Exception:
            //      可能会抛出异常 ArgumentException 
            EntityCollection.SetPII(result, pii);
#endif

            // 2020/7/15
            // 获得图书 RFID 标签的 OI 和 AOI 字段
            if (tag.Type == "book")
            {
                if (chip == null)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    chip = LogicChip.From(tag.OneTag.TagInfo.Bytes,
            (int)tag.OneTag.TagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                }

                if (chip.IsBlank())
                {
                    entity.BuildError("checkTag", "空白标签", "blankTag");
                }
                else
                {
                    string oi = chip.FindElement(ElementOID.OI)?.Text;
                    string aoi = chip.FindElement(ElementOID.AOI)?.Text;

                    result.OI = oi;
                    result.AOI = aoi;

                    // 2020/8/27
                    // 严格要求必须有 OI(AOI) 字段
                    if (string.IsNullOrEmpty(oi) && string.IsNullOrEmpty(aoi))
                        result.BuildError("checkTag", "没有 OI 或 AOI 字段", "missingOI");
                    else
                        result.BuildError("checkTag", null, null);
                }
            }
            else if (tag.Type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                result.PII = "(读者卡)" + result.PII;
                result.BuildError("checkTag", "读者卡误放入书架", "patronCard");
                App.CurrentApp.SpeakSequence("读者卡误放入书架");
            }
            else
                result.BuildError("checkTag", null, null);

            return result;
        }

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

        // 任务完成情况
        public class TaskInfo
        {
            // 任务名
            public string Name { get; set; }
            // 执行结果。Value == 0 表示成功
            public NormalResult Result { get; set; }
        }

        // Entity 附加的处理信息
        public class ProcessInfo
        {
            // 状态
            public string State { get; set; }

            // 是否为层架标？
            public bool IsLocation { get; set; }

            // 是否为读者卡？
            public bool IsPatron { get; set; }

            public string ItemXml { get; set; }

            public string GetTagInfoError { get; set; }
            // GetTagInfo() 出错的次数
            public int ErrorCount { get; set; }

            // 批次号
            public string BatchNo { get; set; }

            // 希望修改成的 currentLocation 字段内容
            public string TargetCurrentLocation { get; set; }
            // 希望修改成的 location 字段内容
            public string TargetLocation { get; set; }
            // 希望修改成的 shelfNo 字段内容
            public string TargetShelfNo { get; set; }

            // 希望修改成的 EAS 内容。on/off/(null) 其中 (null) 表示不必进行修改
            public string TargetEas { get; set; }

            public List<TaskInfo> Tasks { get; set; }

            // 操作者(工作人员)用户名
            public string UserName { get; set; }

            // 设置任务信息
            // parameters:
            //      result  要设置的 NormalResult 对象。如果为 null，表示要删除这个任务条目
            public void SetTaskInfo(string name, NormalResult result)
            {
                if (Tasks == null)
                    Tasks = new List<TaskInfo>();
                var info = Tasks.Find((t) => t.Name == name);
                if (info == null)
                {
                    if (result == null)
                        return;
                    Tasks.Add(new TaskInfo
                    {
                        Name = name,
                        Result = result
                    });
                }
                else
                {
                    if (result == null)
                    {
                        Tasks.Remove(info);
                        return;
                    }
                    info.Result = result;
                }
            }

            // 检测一个任务是否已经完成
            public bool IsTaskCompleted(string name)
            {
                if (Tasks == null)
                    return false;

                var info = Tasks.Find((t) => t.Name == name);
                if (info == null)
                    return false;
                return info.Result.Value != -1;
            }

            // 探测是否包含指定名字的任务信息
            public bool ContainTask(string name)
            {
                if (Tasks == null)
                    return false;

                var info = Tasks.Find((t) => t.Name == name);
                return info != null;
            }
        }

        #region 处理列表

        // 正在获取册信息的 Entity 集合
        static List<Entity> _entityList = new List<Entity>();
        static object _entityListSyncRoot = new object();

        // 复制列表
        public static List<Entity> CopyList()
        {
            lock (_entityListSyncRoot)
            {
                return new List<Entity>(_entityList);
            }
        }

        // 追加元素
        public static void AppendList(Entity entity)
        {
            lock (_entityListSyncRoot)
            {
                _entityList.Add(entity);
            }
        }

        public static void RemoveList(List<Entity> entities)
        {
            lock (_entityListSyncRoot)
            {
                if (entities == null)
                    _entityList.Clear();
                else
                {
                    foreach (var entity in entities)
                    {
                        _entityList.Remove(entity);
                    }
                }
            }
        }



        #region GetTagInfo() 后出错状态的 Entity 集合

        static List<Entity> _errorEntities = new List<Entity>();

        public static List<Entity> ErrorEntities
        {
            get
            {
                return new List<Entity>(_errorEntities);
            }
        }

        public static int AddErrorEntity(Entity entity, out bool changed)
        {
            int old_count = _errorEntities.Count;
            if (_errorEntities.IndexOf(entity) == -1)
                _errorEntities.Add(entity);
            int new_count = _errorEntities.Count;
            changed = !(old_count == new_count);
            return _errorEntities.Count;
        }

        public static int RemoveErrorEntity(Entity entity, out bool changed)
        {
            int old_count = _errorEntities.Count;
            _errorEntities.Remove(entity);
            int new_count = _errorEntities.Count;
            changed = !(old_count == new_count);
            return _errorEntities.Count;
        }

        #endregion

        #endregion

        #region 后台任务

        static Task _inventoryTask = null;

        // 监控间隔时间
        static TimeSpan _inventoryIdleLength = TimeSpan.FromSeconds(10);

        static AutoResetEvent _eventInventory = new AutoResetEvent(false);

        // 激活任务
        public static void ActivateInventory()
        {
            _eventInventory.Set();
        }

        // 启动盘点后台任务
        public static void StartInventoryTask()
        {
            if (_inventoryTask != null)
                return;

            CancellationToken token = App.CancelToken;

            token.Register(() =>
            {
                _eventInventory.Set();
            });

            _inventoryTask = Task.Factory.StartNew(async () =>
            {
                WpfClientInfo.WriteInfoLog("盘点后台线程开始");
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(10));
                        _eventInventory.WaitOne(_inventoryIdleLength);

                        token.ThrowIfCancellationRequested();

                        //
                        await ProcessingAsync();
                    }
                    _inventoryTask = null;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"盘点后台线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("inventory_worker", $"盘点后台线程出现异常: {ex.Message}");
                }
                finally
                {
                    WpfClientInfo.WriteInfoLog("盘点后台线程结束");
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
            foreach (var entity in list)
            {
                var info = entity.Tag as ProcessInfo;
                info.State = "processing";
                try
                {
                    // throw new Exception("testing processing");

                    if (info.IsTaskCompleted("getItemXml") == false)
                    {
                        // 获得册记录和书目摘要
                        // .Value
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        GetEntityDataResult result = null;
                        if (App.Protocol == "sip")
                        {
                            bool isLocal = StringUtil.IsInList("inventory", SipLocalStore);

                            result = await SipChannelUtil.GetEntityDataAsync(entity.PII, isLocal ? "network,localInventory" : "network");
                            if (result.Value != -1)
                            {
                                // 顺便保存到本地数据库
                            }
                        }
                        else
                            result = await LibraryChannelUtil.GetEntityDataAsync(entity.PII, "network");

                        /*
                        // testing
                        result.Value = -1;
                        result.ErrorInfo = "获得册信息出错";
                        */
                        info.SetTaskInfo("getItemXml", result);
                        if (result.Value == -1)
                        {
                            // 2021/1/19
                            App.CurrentApp.SpeakSequence($"{entity.PII} 无法获得册信息");

                            entity.BuildError("getItemXml", result.ErrorInfo, result.ErrorCode);
                        }
                        else
                        {
                            entity.BuildError("getItemXml", null, null);

                            if (string.IsNullOrEmpty(result.Title) == false)
                                entity.Title = PageBorrow.GetCaption(result.Title);
                            if (string.IsNullOrEmpty(result.ItemXml) == false)
                            {
                                if (info != null)
                                    info.ItemXml = result.ItemXml;
                                entity.SetData(result.ItemRecPath, result.ItemXml);
                            }
                        }
                    }

                    // 请求 dp2library Inventory()
                    if (string.IsNullOrEmpty(entity.PII) == false
                        && info != null && info.IsLocation == false)
                    {
                        await BeginInventoryAsync(entity, PageInventory.ActionMode);
                    }

                    App.SetError("processing", null);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"ProcessingAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.SetError("processing", $"ProcessingAsync() 出现异常: {ex.Message}");
                }
                finally
                {
                    info.State = "";
                }
            }

            // 把处理过的 entity 从 list 中移走
            RemoveList(list);

            PageInventory.BeginUpdateStatis();
        }

        #endregion

        public delegate void delegate_showText(string text);

        // parameters:
        //      uid_table   返回 UID --> PII 对照表
        public static NormalResult DownloadUidTable(
            List<string> item_dbnames,
            Hashtable uid_table,
            delegate_showText func_showProgress,
            // Delegate_writeLog writeLog,
            CancellationToken token)
        {
            WpfClientInfo.WriteInfoLog($"开始下载全部册记录到本地缓存");
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                if (item_dbnames == null)
                {
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                foreach (string dbName in item_dbnames)
                {
                    func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...");

                    int nRedoCount = 0;
                REDO:
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断"
                        };
                    // 检索全部读者库记录
                    long lRet = channel.SearchItem(null,
    dbName, // "<all>",
    "",
    -1,
    "__id",
    "left",
    "zh",
    null,   // strResultSetName
    "", // strSearchStyle
    "", // strOutputStyle
    out string strError);
                    if (lRet == -1)
                    {
                        WpfClientInfo.WriteErrorLog($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                        // 一次重试机会
                        if (lRet == -1
                            && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                            && nRedoCount < 2)
                        {
                            nRedoCount++;
                            goto REDO;
                        }

                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }

                    long hitcount = lRet;

                    WpfClientInfo.WriteInfoLog($"{dbName} 共检索命中册记录 {hitcount} 条");

                    // 把超时时间改短一点
                    channel.Timeout = TimeSpan.FromSeconds(20);

                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                        // 获取和存储记录
                        ResultSetLoader loader = new ResultSetLoader(channel,
            null,
            null,
            strStyle,   // $"id,xml,timestamp",
            "zh");

                        // loader.Prompt += this.Loader_Prompt;
                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                        {
                            if (token.IsCancellationRequested)
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = "用户中断"
                                };

                            if (record.Cols != null)
                            {
                                string barcode = "";
                                if (record.Cols.Length > 0)
                                    barcode = record.Cols[0];
                                string location = "";
                                if (record.Cols.Length > 1)
                                    location = record.Cols[1];
                                string uid = "";
                                if (record.Cols.Length > 2)
                                    uid = record.Cols[2];
                                if (string.IsNullOrEmpty(barcode) == false
                                    && string.IsNullOrEmpty(uid) == false)
                                    uid_table[uid] = barcode;
                            }

                            i++;
                        }

                    }

                    WpfClientInfo.WriteInfoLog($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");

                }
                return new NormalResult
                {
                    Value = uid_table.Count,
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"DownloadItemRecordAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadItemRecordAsync() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);

                WpfClientInfo.WriteInfoLog($"结束下载全部册记录到本地缓存");
            }
        }

        // 显示对书柜门的 Iventory 操作，同一时刻只能一个函数进入
        static AsyncSemaphore _requestLimit = new AsyncSemaphore(1);

        public static async Task BeginInventoryAsync(Entity entity,
            string actionMode)
        {
            using (var releaser = await _requestLimit.EnterAsync().ConfigureAwait(false))
            {
                var info = entity.Tag as ProcessInfo;

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
                    && App.Protocol != "sip")
                {
                    var request_result = RequestReturn(
    entity.PII,
    entity.ItemRecPath,
    info.BatchNo,
    info.UserName,
    "");
                    info.SetTaskInfo("return", request_result);
                    if (request_result.Value == -1)
                    {
                        App.CurrentApp.SpeakSequence($"{entity.PII} 还书请求出错");
                        entity.BuildError("return", request_result.ErrorInfo, request_result.ErrorCode);
                    }
                    else
                    {
                        entity.BuildError("return", null, null);

                        // 提醒操作者发生了还书操作
                        App.CurrentApp.SpeakSequence($"还书成功 {entity.PII}");

                        if (string.IsNullOrEmpty(request_result.ItemXml) == false)
                            info.ItemXml = request_result.ItemXml;

                        // 标记，即将 VerifyEas
                        need_verifyEas = true;
#if NO
                        // 提请修改 EAS。可能会通过反复操作才能修改成功
                        // return:
                        //      1 为 on; 0 为 off; -1 表示不合法的值
                        var ret = GetEas(entity);
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
                        else if ( ret != 1)
                        {
                            info.TargetEas = "on";
                            info.SetTaskInfo("changeEAS", new NormalResult
                            {
                                Value = -1,
                                ErrorCode = "initial"   // 表示需要处理但尚未开始处理
                            });

                            // 如果 RFID 标签此时正好在读卡器上，则立即触发处理
                            // result.Value
                            //      -1  出错
                            //      0   标签不在读卡器上所有没有执行
                            //      1   成功执行修改
                            var result = await TryChangeEas(entity, true);

                            // TODO: 语音提醒，有等待处理的 EAS
                            if (result.Value != 1)
                            {
                                App.CurrentApp.SpeakSequence($"等待修改 EAS : {CutTitle(entity.Title)} ");
                            }
                        }
#endif
                    }
                }

                // 确保还书成功后，再执行 EAS 检查
                if (
                    (info.ContainTask("return") == false || info.IsTaskCompleted("return") == true)
                    && (need_verifyEas == true || StringUtil.IsInList("verifyEAS", actionMode))
                    && App.Protocol != "sip")
                {
                    await VerifyEasAsync(entity);
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
                if (App.Protocol == "sip")
                    isSipLocal = StringUtil.IsInList("inventory", SipLocalStore);

                // 设置 UID
                if (StringUtil.IsInList("setUID", actionMode)
                    && (string.IsNullOrEmpty(info.ItemXml) == false || isSipLocal == true)
                    && info.IsTaskCompleted("setUID") == false
                    )
                {
                    // TODO: SIP2 模式下，UID - PII 对照信息可以设置到 dp2ssl 本地数据库
                    RequestSetUidResult request_result = null;
                    if (App.Protocol == "sip")
                        request_result = await RequestSetUIDtoLocalAsync(
                            entity.UID,
                            entity.PII,
                            "");
                    else
                        request_result = RequestSetUID(entity.ItemRecPath,
                            info.ItemXml,
                            null,
                            entity.UID,
                            info.UserName,
                            "");
                    info.SetTaskInfo("setUID", request_result);
                    if (request_result.Value == -1)
                    {
                        App.CurrentApp.SpeakSequence($"{entity.UID} 设置 UID 请求出错");
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
                if (info.IsTaskCompleted("setLocation") == false
                    && info.IsTaskCompleted("getItemXml") == true   // 2021/1/26
                    && string.IsNullOrEmpty(info.ItemXml) == false)
                {
                    RequestInventoryResult request_result = null;
                    if (App.Protocol == "sip")
                    {
                        // bool isLocal = StringUtil.IsInList("inventory", SipLocalStore);
                        if (isSipLocal)
                            request_result = await RequestInventory_local(
                                info.ItemXml,
                                entity.UID,
                                entity.PII,
                                StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                                info.BatchNo,
                                info.UserName,
                                PageInventory.ActionMode);
                        else
                            request_result = await RequestInventory_sip2(entity.UID,
                                entity.PII,
                                StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                                StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                                info.BatchNo,
                                info.UserName,
                                PageInventory.ActionMode);
                    }
                    else
                        request_result = RequestInventory(entity.UID,
                            entity.PII,
                            StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
                            StringUtil.IsInList("setLocation", actionMode) ? info.TargetShelfNo : null,
                            info.BatchNo,
                            info.UserName,
                            PageInventory.ActionMode);
                    // 两个动作当作一个 setLocation 来识别
                    info.SetTaskInfo("setLocation", request_result);
                    if (request_result.Value == -1)
                    {
                        App.CurrentApp.SpeakSequence($"{entity.PII} 盘点请求出错");
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
                    }
                }

                // SetUID 和 Inventory 至少成功过一次，则发出成功的响声
                if (succeed_count > 0)
                    SoundMaker.SucceedSound();
            }
        }

        // 检测 RFID 标签 EAS 位是否正确
        // return.Value
        //      -1  出错
        //      0   没有进行验证(已经加入后台验证任务)
        //      1   已经成功进行验证
        public static async Task<NormalResult> VerifyEasAsync(Entity entity)
        {
            var info = entity.Tag as ProcessInfo;
            if (string.IsNullOrEmpty(info.ItemXml))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "因 ItemXml 为空，无法进行 EAS 验证"
                };
            }

            var borrowed = HasBorrowed(info.ItemXml);
            var ret = GetEas(entity);
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
                var result = await TryChangeEasAsync(entity, !borrowed);

                // TODO: 语音提醒，有等待处理的 EAS
                if (result.Value != 1)
                {
                    App.CurrentApp.SpeakSequence($"等待修改 EAS : {CutTitle(entity.Title)} ");
                    return new NormalResult();
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
        public static async Task<NormalResult> TryChangeEasAsync(Entity entity, bool enable)
        {
            using (var releaser = await _easLimit.EnterAsync().ConfigureAwait(false))
            {
                var info = entity.Tag as ProcessInfo;

                if (entity.TagInfo == null)
                {
                    // 标签正好在读卡器上，读 TagInfo 一次
                    if (TagOnReader(entity))
                    {
                        var get_result = RfidManager.GetTagInfo(entity.ReaderName, entity.UID, Convert.ToUInt32(entity.Antenna));
                        if (get_result.Value != -1)
                            entity.TagInfo = get_result.TagInfo;
                    }

                    if (entity.TagInfo == null)
                    {
                        info.GetTagInfoError = "errorGetTagInfo";    // 表示希望获得 TagInfo
                        int count = AddErrorEntity(entity, out bool changed);
                        if (changed == true)
                            App.CurrentApp.SpeakSequence(count.ToString());
                        return new NormalResult();  // 没有执行
                    }
                }

                // 如果 RFID 标签此时正好在读卡器上，则立即触发处理
                // var tag_data = NewTagList2.Tags.Find((t) => t.OneTag.UID == entity.UID);
                if (TagOnReader(entity))
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
                            App.CurrentApp.SpeakSequence($"修改 EAS 失败: {CutTitle(entity.Title)} ");
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
                            App.CurrentApp.SpeakSequence($"修改 EAS 成功: {CutTitle(entity.Title)} ");
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
#if OLD_TAGCHANGED

                    TagList.SetEasData(uid, enable);
#else
                    NewTagList2.SetEasData(uid, enable);
#endif
                }
                return result;
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
        }

        // 判断标签是否正好在读卡器上
        static bool TagOnReader(Entity entity)
        {
            // 如果 RFID 标签此时正好在读卡器上，则立即触发处理
            var tag_data = NewTagList2.Tags.Find((t) => t.OneTag.UID == entity.UID);
            return (tag_data != null);
        }

        // 判断当前 entity 对应的 RFID 标签的 EAS 状态
        // 注：通过 AFI 进行判断。0x07 为 on；0xc2 为 off
        // return:
        //      1 为 on; 0 为 off; -1 表示不合法的值; -2 表示 TagInfo 为 null 无法获得 AFI
        static int GetEas(Entity entity)
        {
            // tagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;
            var info = entity.Tag as ProcessInfo;

            // TagInfo 为 null ?
            if (entity.TagInfo == null)
            {
                // 标签正好在读卡器上，读 TagInfo 一次
                if (TagOnReader(entity))
                {
                    var get_result = RfidManager.GetTagInfo(entity.ReaderName, entity.UID, Convert.ToUInt32(entity.Antenna));
                    if (get_result.Value != -1)
                        entity.TagInfo = get_result.TagInfo;
                }

                if (entity.TagInfo == null)
                {
                    // 加入 error 队列，等待后面处理
                    info.GetTagInfoError = "errorGetTagInfo";    // 表示希望获得 TagInfo
                    int count = AddErrorEntity(entity, out bool changed);
                    if (changed == true)
                        App.CurrentApp.SpeakSequence(count.ToString());
                    return -2;
                }
            }

            var afi = entity.TagInfo.AFI;
            if (afi == 0x07)
                return 1;
            if (afi == 0xc2)
                return 0;
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

        // 向 SIP2 服务器发出盘点请求
        // 注意，不会考虑本地缓存的盘点信息
        public static async Task<RequestInventoryResult> RequestInventory_sip2(
            string uid,
            string pii,
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
    "", // oi,
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
            var get_result = await SipChannelUtil.GetEntityDataAsync(pii, "network");
            if (get_result.Value == -1)
            {
                // TODO: 如何报错？
            }
            return new RequestInventoryResult { ItemXml = get_result.ItemXml };
        }

        public class RequestInventoryResult : NormalResult
        {
            public string ItemXml { get; set; }
        }

        // 向 dp2library 服务器发出盘点请求
        public static RequestInventoryResult RequestInventory(string uid,
            string pii,
            string currentLocation,
            string location,
            string shelfNo,
            string batchNo,
            string strUserName,
            string style)
        {
            if (currentLocation == null && location == null)
                return new RequestInventoryResult { Value = 0 };    // 没有必要修改

            // TODO: 是否要用特定的工作人员身份进行盘点?
            LibraryChannel channel = App.CurrentApp.GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                // currentLocation 元素内容。格式为 馆藏地:架号
                // 注意馆藏地和架号字符串里面不应包含逗号和冒号
                List<string> commands = new List<string>();
                if (string.IsNullOrEmpty(currentLocation) == false)
                    commands.Add($"currentLocation:{StringUtil.EscapeString(currentLocation, ":,")}");
                if (string.IsNullOrEmpty(location) == false)
                    commands.Add($"location:{StringUtil.EscapeString(location, ":,")}");
                if (string.IsNullOrEmpty(shelfNo) == false)
                    commands.Add($"shelfNo:{StringUtil.EscapeString(shelfNo, ":,")}");
                if (string.IsNullOrEmpty(batchNo) == false)
                {
                    commands.Add($"batchNo:{StringUtil.EscapeString(batchNo, ":,")}");

                    /*
                    // 即便册记录没有发生修改，也要产生 transfer 操作日志记录。这样便于进行典藏移交清单统计打印
                    commands.Add("forceLog");
                    */
                }

                string strStyle = "item";

                int nRedoCount = 0;
            REDO:
                long lRet = channel.Return(null,
                    "transfer",
                    "", // _patron.Barcode,
                    pii,    // entity.PII,
                    null,   // entity.ItemRecPath,
                    false,
                    $"{strStyle},{StringUtil.MakePathList(commands, ",")}", // style,
                    "xml", // item_format_list
                    out string[] item_records,
                    "xml",
                    out string[] reader_records,
                    "summary",
                    out string[] biblio_records,
                    out string[] dup_path,
                    out string output_reader_barcode,
                    out ReturnInfo return_info,
                    out string strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotChanged)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestInventoryResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestInventoryResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                // 更新册记录
                string entity_xml = null;
                if (item_records?.Length > 0)
                    entity_xml = item_records[0];
                return new RequestInventoryResult { ItemXml = entity_xml };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // 当前层架标
        public static string CurrentShelfNo { get; set; }

        // 当前馆藏地。例如 “海淀分馆/阅览室”
        public static string CurrentLocation { get; set; }

        // 当前批次号
        public static string CurrentBatchNo { get; set; }

        public class RequestSetUidResult : NormalResult
        {
            public string NewItemXml { get; set; }
            public byte[] NewTimestamp { get; set; }
        }

        // 向 dp2library 服务器发出设置册记录 UID 的请求
        public static RequestSetUidResult RequestSetUID(
            string strRecPath,
            string strOldXml,
            byte[] old_timestamp,
            string uid,
            // string batchNo,
            string strUserName,
            string style)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strOldXml);

            string old_uid = DomUtil.GetElementText(dom.DocumentElement, "uid");
            if (old_uid == uid)
            {
                return new RequestSetUidResult { Value = 0 };    // 没有必要修改
            }
            DomUtil.SetElementText(dom.DocumentElement, "uid", uid);


            List<EntityInfo> entityArray = new List<EntityInfo>();

            {
                EntityInfo item_info = new EntityInfo();

                item_info.OldRecPath = strRecPath;
                item_info.Action = "setuid";
                item_info.NewRecPath = strRecPath;

                item_info.NewRecord = dom.OuterXml;
                item_info.NewTimestamp = null;

                item_info.OldRecord = strOldXml;
                item_info.OldTimestamp = old_timestamp;

                entityArray.Add(item_info);
            }

            // TODO: 是否要用特定的工作人员身份进行盘点?
            LibraryChannel channel = App.CurrentApp.GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.SetEntities(
                 null,
                 "",
                 entityArray.ToArray(),
                 out EntityInfo[] errorinfos,
                 out string strError);
                if (lRet == -1)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestSetUidResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestSetUidResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                if (errorinfos == null)
                    return new RequestSetUidResult { };

                List<string> errors = new List<string>();
                string strNewXml = "";
                byte[] baNewTimestamp = null;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    var info = errorinfos[i];

                    if (i == 0)
                    {
                        baNewTimestamp = info.NewTimestamp;
                        strNewXml = info.NewRecord;
                    }

                    // 正常信息处理
                    if (info.ErrorCode == ErrorCodeValue.NoError)
                        continue;

                    errors.Add(info.RefID + " 在提交保存过程中发生错误 -- " + info.ErrorInfo);
                }

                if (errors.Count > 0)
                    return new RequestSetUidResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, ";")
                    };

                return new RequestSetUidResult
                {
                    Value = 1,
                    NewItemXml = strNewXml,
                    NewTimestamp = baNewTimestamp
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // 向 dp2library 服务器发出还书请求
        public static RequestInventoryResult RequestReturn(
            string pii,
            string itemRecPath,
            string batchNo,
            string strUserName,
            string style)
        {
            // TODO: 是否要用特定的工作人员身份进行还书?
            LibraryChannel channel = App.CurrentApp.GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                string strStyle = "item";
                string operTimeStyle = "";

                int nRedoCount = 0;
            REDO:
                long lRet = channel.Return(null,
                    "return",
                    "", // _patron.Barcode,
                    pii,    // entity.PII,
                    itemRecPath,
                    false,
                    strStyle + operTimeStyle, // style,
                    "xml", // item_format_list
                    out string[] item_records,
                    "xml",
                    out string[] reader_records,
                    "summary",
                    out string[] biblio_records,
                    out string[] dup_path,
                    out string output_reader_barcode,
                    out ReturnInfo return_info,
                    out string strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotBorrowed)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestInventoryResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestInventoryResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                // 更新册记录
                string entity_xml = null;
                if (item_records?.Length > 0)
                    entity_xml = item_records[0];
                return new RequestInventoryResult { ItemXml = entity_xml };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
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

        public static XmlDocument GetInventoryDom()
        {
            string filename = Path.Combine(WpfClientInfo.UserDir, "inventory.xml");
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

                        func_showProgress?.Invoke($"{uid} --> {barcode} ...");

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
                WpfClientInfo.WriteErrorLog($"LoadUidTable() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

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

        /*
        // 更新本地库中的 UID --> PII 对照关系
        // TODO: 其实只要检索 hashtable 中存在对应关系，就表明本地库中已经有了，就不必真的更新本地库了
        static NormalResult UpdateUid(string barcode,
            string uid)
        {
            using (var context = new ItemCacheContext())
            {
                var item = context.Uids.Where(o => o.PII == barcode).FirstOrDefault();
                if (item == null)
                {
                    item = new UidEntry { PII = barcode, UID = uid };
                    context.Uids.Add(item);
                    context.SaveChanges();
                    return new NormalResult { Value = 1 };
                }

                if (item.UID != uid)
                {
                    item.UID = uid;
                    context.Uids.Update(item);
                    context.SaveChanges();
                    return new NormalResult { Value = 1 };
                }
                return new NormalResult();
            }
        }
        */

        // 请求设置 UID 到本地数据库
        public static async Task<RequestSetUidResult> RequestSetUIDtoLocalAsync(
    string uid,
    string pii,
    string style)
        {
            // TODO: 可以先检查 hashtable 中是否有了，有了则表示不用加入本地数据库了，这样可以优化速度

            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                UpdateUidEntry(context, pii, uid);
            }

            return new RequestSetUidResult
            {
                Value = 1,
            };
        }

        // 将原本要向 SIP2 服务器发出盘点请求写入本地(映射)数据库
        public static async Task<RequestInventoryResult> RequestInventory_local(
            string item_xml,
            string uid,
            string pii,
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
                var item = context.Items.Where(o => o.Barcode == pii).FirstOrDefault();
                if (item == null)
                {
                    item = new BookItem
                    {
                        Title = title,
                        Barcode = pii,
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
                    Barcode = pii,
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

        // 导入 UID PII 对照表文件
        public static async Task<ImportUidResult> ImportUidPiiTableAsync(
            string filename,
            CancellationToken token)
        {
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
                WpfClientInfo.WriteErrorLog($"ImportUidPiiTable() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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

#if NO
                var all = context.Uids.Where(o => string.IsNullOrEmpty(o.UID) == false && string.IsNullOrEmpty(o.PII) == false).ToList();
                List<BookItem> items = new List<BookItem>();
                foreach (var item in all)
                {
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = change_count,
                        };

                    {
                        item.UID = null;
                        items.Add(item);
                        change_count++;
                    }

                    if (items.Count > 100)
                    {
                        Save(context, items);
                        items.Clear();
                    }

                }

                if (items.Count > 0)
                {
                    Save(context, items);
                    items.Clear();
                }
#endif
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

        // 把从 SIP2 服务器得到的信息保存到本地数据库
        static async Task<NormalResult> SaveToLocal(string barcode,
            string item_xml)
        {
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                var item = context.Items.Where(o => o.Barcode == barcode).FirstOrDefault();
                if (item == null)
                {
                    item = new BookItem { Barcode = barcode };

                    FillBookItem(item, item_xml);
                    context.Items.Add(item);
                    await context.SaveChangesAsync();
                }
                else
                {
                    FillBookItem(item, item_xml);
                    context.Items.Update(item);
                    await context.SaveChangesAsync();
                }
                return new NormalResult();
            }
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

            App.PauseBarcodeScan();
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

                if (dlg.ShowDialog() == false)
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

                doc.SaveAs(dlg.FileName);
                doc.Dispose();

                try
                {
                    System.Diagnostics.Process.Start(dlg.FileName);
                }
                catch
                {
                }

                return new NormalResult();
            }
            finally
            {
                App.ContinueBarcodeScan();
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
        public static async Task<NormalResult> ExportAllItemToExcelAsync(
            List<InventoryColumn> columns,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            App.PauseBarcodeScan();
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

                if (dlg.ShowDialog() == false)
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
                    barcodes = context.Items.Select(o => o.Barcode).ToList();
                }

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

                    func_showProgress?.Invoke($"正在导出 {barcode} ...");

                    // 检查册记录是否存在
                    var result = await SipChannelUtil.GetEntityDataAsync(barcode, "network,localInventory,updateItemTitle");
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

                return new NormalResult();
            }
            finally
            {
                App.ContinueBarcodeScan();
            }

            string camel(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;
                return char.ToLower(text[0]) + text.Substring(1);
            }
        }

        #endregion
    }
}
