using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.Xml;

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
                }
                catch (Exception ex)
                {
                    App.CurrentApp.SpeakSequence("警告: 标签解析出错");
                    if (throw_exception == false)
                    {
                        entity.AppendError($"RFID 标签格式错误: {ex.Message}",
                            "red",
                            "parseTagError");
                    }
                    else
                        throw ex;
                }
            }

            // 2020/4/9
            if (type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                entity.PII = "(读者卡)" + entity.PII;
                entity.AppendError("读者卡误放入书柜", "red", "patronCard");
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
                    entity.AppendError("空白标签", "red", "blankTag");
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
                        entity.AppendError("没有 OI 或 AOI 字段", "red", "missingOI");
                }
            }
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
                }
                catch (Exception ex)
                {
                    App.CurrentApp.SpeakSequence("警告: 标签解析出错");
                    if (throw_exception == false)
                    {
                        result.AppendError($"RFID 标签格式错误: {ex.Message}",
                            "red",
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

            // 2020/4/9
            if (tag.Type == "patron")
            {
                // 避免被当作图书同步到 dp2library
                result.PII = "(读者卡)" + result.PII;
                result.AppendError("读者卡误放入书柜", "red", "patronCard");
            }

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
                    entity.AppendError("空白标签", "red", "blankTag");
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
                        result.AppendError("没有 OI 或 AOI 字段", "red", "missingOI");
                }
            }
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


        // Entity 附加的处理信息
        public class ProcessInfo
        {
            // 状态
            public string State { get; set; }

            // 是否为层架标？
            public bool IsLocation { get; set; }

            public string ItemXml { get; set; }

            // GetTagInfo() 出错的次数
            public int ErrorCount { get; set; }

            // 批次号
            public string BatchNo { get; set; }

            // 希望修改成的 currentLocation 字段内容
            public string TargetCurrentLocation { get; set; }
            // 希望修改成的 location 字段内容
            public string TargetLocation { get; set; }
            // 操作者(工作人员)用户名
            public string UserName { get; set; }
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

        static async Task ProcessingAsync()
        {
            var list = CopyList();
            foreach (var entity in list)
            {
                var info = entity.Tag as ProcessInfo;

                // 获得册记录和书目摘要
                // .Value
                //      -1  出错
                //      0   没有找到
                //      1   找到
                var result = await LibraryChannelUtil.GetEntityDataAsync(entity.PII, "network");

                /*
                // testing
                result.Value = -1;
                result.ErrorInfo = "获得册信息出错";
                */

                if (result.Value == -1)
                    entity.AppendError(result.ErrorInfo, "red", result.ErrorCode);
                else
                {
                    if (string.IsNullOrEmpty(result.Title) == false)
                        entity.Title = PageBorrow.GetCaption(result.Title);
                    if (string.IsNullOrEmpty(result.ItemXml) == false)
                    {
                        if (info != null)
                            info.ItemXml = result.ItemXml;
                        entity.SetData(result.ItemRecPath, result.ItemXml);
                    }
                }


                // 请求 dp2library Inventory()
                if (string.IsNullOrEmpty(entity.PII) == false
                    && info != null && info.IsLocation == false)
                {
                    _ = BeginInventoryAsync(entity, PageInventory.ActionMode);
                    /*
                    var info = entity.Tag as ProcessInfo;

                    var request_result = RequestInventory(entity.UID,
    entity.PII,
    info.TargetCurrentLocation,
    info.TargetLocation,
    info.BatchNo,
    info.UserName,
    PageInventory.ActionMode);
                    if (request_result.Value == -1)
                    {
                        // TODO: 语音提示引起操作者注意
                        entity.AppendError(request_result.ErrorInfo, "red", request_result.ErrorCode);
                    }
                    */
                }
            }

            // 把处理过的 entity 从 list 中移走
            RemoveList(list);
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

                WpfClientInfo.WriteInfoLog($"结束下载全部读者记录到本地缓存");
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

                // 设置 UID
                if (StringUtil.IsInList("setUID", actionMode)
                    && string.IsNullOrEmpty(info.ItemXml) == false)
                {
                    var request_result = RequestSetUID(entity.ItemRecPath,
                        info.ItemXml,
                        null,
                        entity.UID,
                        info.UserName,
                        "");
                    if (request_result.Value == -1)
                    {
                        // TODO: 语音提示引起操作者注意
                        // TODO: NotChanged 处理
                        entity.AppendError(request_result.ErrorInfo, "red", request_result.ErrorCode);
                    }
                    else
                    {
                        info.ItemXml = request_result.NewItemXml;
                    }
                }

                // 动作模式
                /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
                 * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
                 * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
                 * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
                 * */

                {
                    var request_result = RequestInventory(entity.UID,
    entity.PII,
    StringUtil.IsInList("setCurrentLocation", actionMode) ? info.TargetCurrentLocation : null,
    StringUtil.IsInList("setLocation", actionMode) ? info.TargetLocation : null,
    info.BatchNo,
    info.UserName,
    PageInventory.ActionMode);
                    if (request_result.Value == -1)
                    {
                        // TODO: 语音提示引起操作者注意
                        // TODO: NotChanged 处理
                        entity.AppendError(request_result.ErrorInfo, "red", request_result.ErrorCode);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(request_result.ItemXml) == false)
                            entity.SetData(entity.ItemRecPath, request_result.ItemXml);
                    }
                }
            }
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


    }
}
