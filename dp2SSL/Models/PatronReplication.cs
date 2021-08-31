using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Collections;

using static dp2SSL.LibraryChannelUtil;
using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using DigitalPlatform.IO;

namespace dp2SSL.Models
{
    /// <summary>
    /// 读者库复制和同步功能
    /// </summary>
    public static class PatronReplication
    {
        public class ReplicationPlan : NormalResult
        {
            public string StartDate { get; set; }

            public override string ToString()
            {
                return base.ToString() + $",StartDate={StartDate}";
            }
        }

        // 整体获得全部读者记录以前，预备获得同步计划信息
        // 也就是第一次同步开始的位置信息
        public static ReplicationPlan GetReplicationPlan(LibraryChannel channel)
        {
            // 开始处理时的日期
            string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

            // 获得日志文件中记录的总数
            // parameters:
            //      strDate 日志文件的日期，8 字符
            // return:
            //      -2  此类型的日志在 dp2library 端尚未启用
            //      -1  出错
            //      0   日志文件不存在，或者记录数为 0
            //      >0  记录数
            long lCount = OperLogLoader.GetOperLogCount(
                null,
                channel,
                strEndDate,
                LogType.OperLog,
                out string strError);
            if (lCount < 0)
            {
                // errorCode: "RequestError" 服务器没有响应
                return new ReplicationPlan { Value = -1, ErrorInfo = strError, ErrorCode = channel.ErrorCode.ToString() };
            }
            return new ReplicationPlan { StartDate = strEndDate + ":" + lCount + "-" };
        }

        public delegate void Delegate_writeLog(string text);

        static int _inDownloadingPatron = 0;

        // 第一阶段：获得全部读者库记录，进入本地数据库
        // result.Value
        //      -1  出错
        //      >=0 实际获得的读者记录条数
        public static async Task<ReplicationPlan> DownloadAllPatronRecordAsync(
            Delegate_writeLog writeLog,
            CancellationToken token)
        {
            _inDownloadingPatron++;

            // 2020/9/26
            if (_inDownloadingPatron > 1)
            {
                _inDownloadingPatron--;
                return new ReplicationPlan
                {
                    Value = -1,
                    ErrorCode = "running",
                    ErrorInfo = "前一次的“下载全部读者记录到本地缓存”过程还在进行中，本次触发被放弃"
                };
            }

            writeLog?.Invoke($"开始下载全部读者记录到本地缓存");
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为读者记录检索需要一定时间
            try
            {

                ReplicationPlan plan = GetReplicationPlan(channel);

                writeLog?.Invoke($"GetReplicationPlan() return {plan.ToString()}");

                if (plan.Value == -1)
                    return plan;

                int nRedoCount = 0;
            REDO:
                if (token.IsCancellationRequested)
                    return new ReplicationPlan
                    {
                        Value = -1,
                        ErrorInfo = "用户中断"
                    };
                // 检索全部读者库记录
                long lRet = channel.SearchReader(null,  // stop,
"<all>",
"",
-1,
"__id",
"left",
"zh",
null,   // strResultSetName
"", // strOutputStyle
out string strError);
                if (lRet == -1)
                {
                    writeLog?.Invoke($"SearchReader() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                    // 一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    return new ReplicationPlan
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                long hitcount = lRet;

                writeLog?.Invoke($"共检索命中读者记录 {hitcount} 条");

                // 把超时时间改短一点
                channel.Timeout = TimeSpan.FromSeconds(20);

                DateTime search_time = DateTime.Now;

                Hashtable pii_table = new Hashtable();
                int skip_count = 0;
                int error_count = 0;

                // 获取和存储记录
                ResultSetLoader loader = new ResultSetLoader(channel,
    null,
    null,
    $"id,xml,timestamp",
    "zh");
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    context.Database.EnsureCreated();

                    // 删除 Patrons 里面的已有记录
                    context.Patrons.RemoveRange(context.Patrons.ToList());
                    await context.SaveChangesAsync(token);

                    // loader.Prompt += this.Loader_Prompt;
                    if (hitcount > 0)
                    {
                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                        {
                            if (token.IsCancellationRequested)
                                return new ReplicationPlan
                                {
                                    Value = -1,
                                    ErrorInfo = "用户中断"
                                };

                            PatronItem item = new PatronItem();

                            // result.Value:
                            //      -1  出错
                            //      0   需要跳过这条读者记录
                            //      1   成功
                            var result = Set(item, record, search_time);
                            if (result.Value == -1 || result.Value == 0)
                            {
                                // TODO: 是否汇总报错信息？

                                if (result.Value == -1)
                                {
                                    writeLog?.Invoke($"Set() ({item.RecPath}) 出错: {result.ErrorInfo}");
                                    error_count++;
                                }
                                if (result.Value == 0)
                                    skip_count++;
                                continue;
                            }

                            // 
                            if (pii_table.ContainsKey(result.PII))
                            {
                                string recpath = (string)pii_table[result.PII];
                                writeLog?.Invoke($"发现读者记录 {item.RecPath} 的 PII '{result.PII}' 和 {recpath} 的 PII 重复了。跳过它");
                                continue;
                            }

                            pii_table[result.PII] = item.RecPath;

                            // TODO: PII 应该是包含 OI 的严格形态
                            context.Patrons.Add(item);

                            if ((i % 10) == 0)
                                await context.SaveChangesAsync(token);

                            i++;
                        }

                        await context.SaveChangesAsync(token);
                    }
                }

                writeLog?.Invoke($"plan.StartDate='{plan.StartDate}'。skip_count={skip_count}, error_count={error_count}。返回");

                return new ReplicationPlan
                {
                    Value = (int)hitcount,
                    StartDate = plan.StartDate
                };
            }
            catch (Exception ex)
            {
                // 2020/9/26
                writeLog?.Invoke($"DownloadAllPatronRecord() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new ReplicationPlan
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllPatronRecord() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);

                writeLog?.Invoke($"结束下载全部读者记录到本地缓存");

                _inDownloadingPatron--;
            }
        }

        class SetResult : NormalResult
        {
            public string PII { get; set; }
        }

        // 设置 PatronItem 对象成员
        // result.Value:
        //      -1  出错
        //      0   需要跳过这条读者记录
        //      1   成功
        static SetResult Set(PatronItem patron,
            Record record,
            DateTime lastWriteTime)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(record.RecordBody.Xml);
            }
            catch (Exception ex)
            {
                return new SetResult
                {
                    Value = -1,
                    ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                    ErrorCode = "loadXmlError"
                };
            }

            string state = DomUtil.GetElementText(dom.DocumentElement, "state");
            if (string.IsNullOrEmpty(state) == false)
                return new SetResult
                {
                    Value = 0,
                    ErrorInfo = $"读者证状态为 '{state}'。即不为空"
                };

            // TODO: 如果 XML 记录尺寸太大，可以考虑删除一些无关紧要的元素以后进入 patron.Xml，避免溢出 SQLite 一条记录可以存储的最大尺寸

            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            if (string.IsNullOrEmpty(pii))
                return new SetResult
                {
                    Value = 0,
                    ErrorInfo = "读者证条码号为空"
                };

            if (string.IsNullOrEmpty(pii))
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");

            string cardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
            cardNumber = cardNumber.ToUpper();
            if (string.IsNullOrEmpty(cardNumber) == false)
                cardNumber = "," + cardNumber + ",";

            // 2020/7/17
            if (pii.StartsWith("@") == false)
            {
                string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
                if (ret == true)
                {
                    // 应该是 xxx.xxx 形态
                    if (string.IsNullOrEmpty(isil) == false)
                        pii = isil + "." + pii;
                    else if (string.IsNullOrEmpty(alternative) == false)
                        pii = alternative + "." + pii;
                }
            }

            patron.PII = pii;
            patron.RecPath = record.Path;
            patron.Bindings = cardNumber;
            patron.Xml = record.RecordBody.Xml;
            patron.Timestamp = record.RecordBody.Timestamp;
            patron.LastWriteTime = lastWriteTime;
            return new SetResult
            {
                Value = 1,
                PII = pii
            };
        }

        public class ReplicationResult : NormalResult
        {
            public string LastDate { get; set; }
            public long LastIndex { get; set; }

            // [out] 返回处理概述信息
            public ProcessInfo ProcessInfo { get; set; }
        }

        public class ProcessInfo
        {
            // 新创建数量
            public int NewCount { get; set; }
            // 删除数量
            public int DeleteCount { get; set; }
            // 修改数量
            public int ChangeCount { get; set; }

            public static void AddTo(ProcessInfo info1, ProcessInfo info2)
            {
                info2.NewCount += info1.NewCount;
                info2.DeleteCount += info1.DeleteCount;
                info2.ChangeCount += info1.ChangeCount;
            }
        }


        // 第二阶段：根据操作日志进行同步
        // 注：中途遇到异常(例如 Loader 抛出异常)，可能会丢失 INSERT_BATCH 条以内的日志记录写入 operlog 表
        // parameters:
        //      strLastDate   处理中断或者结束时返回最后处理过的日期
        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        public static async Task<ReplicationResult> DoReplication(
            string strStartDate,
            string strEndDate,
            LogType logType,
            // string serverVersion,
            CancellationToken token)
        {
            string strLastDate = "";
            long last_index = -1;    // -1 表示尚未处理

            // bool bUserChanged = false;
            ProcessInfo info = new ProcessInfo();

            // strStartDate 里面可能会包含 ":1-100" 这样的附加成分
            StringUtil.ParseTwoPart(strStartDate,
                ":",
                out string strLeft,
                out string strRight);
            strStartDate = strLeft;

            if (string.IsNullOrEmpty(strStartDate) == true)
            {
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = "DoReplication() 出错: strStartDate 参数值不应为空"
                };
            }

            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                int nRet = OperLogLoader.MakeLogFileNames(strStartDate,
                    strEndDate,
                    true,  // 是否包含扩展名 ".log"
                    out List<string> dates,
                    out string strWarning,
                    out string strError);
                if (nRet == -1)
                {
                    return new ReplicationResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }

                if (dates.Count > 0 && string.IsNullOrEmpty(strRight) == false)
                {
                    dates[0] = dates[0] + ":" + strRight;
                }


                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    context.Database.EnsureCreated();

                    ProgressEstimate estimate = new ProgressEstimate();

                    OperLogLoader loader = new OperLogLoader
                    {
                        Channel = channel,
                        Stop = null, //  this.Progress;
                                     // loader.owner = this;
                        Estimate = estimate,
                        Dates = dates,
                        Level = 0,
                        AutoCache = false,
                        CacheDir = "",
                        LogType = logType,
                        Filter = "setReaderInfo,borrow,return,setSystemParameter,writeRes,setEntity", // 借书还书时候都会修改读者记录
                        // ServerVersion = serverVersion
                    };

                    //TimeSpan old_timeout = channel.Timeout;
                    //channel.Timeout = new TimeSpan(0, 2, 0);   // 二分钟

                    // loader.Prompt += Loader_Prompt;
                    try
                    {
                        // int nRecCount = 0;

                        string strLastItemDate = "";
                        long lLastItemIndex = -1;
                        // TODO: 计算遍历耗费的时间。如果太短了，要想办法让调主知道这一点，放缓重新调用的节奏，以避免 CPU 和网络资源太高
                        foreach (OperLogItem item in loader)
                        {
                            token.ThrowIfCancellationRequested();

                            //if (stop != null)
                            //    stop.SetMessage("正在同步 " + item.Date + " " + item.Index.ToString() + " " + estimate.Text + "...");

                            if (string.IsNullOrEmpty(item.Xml) == true)
                                goto CONTINUE;

                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(item.Xml);
                            }
                            catch (Exception ex)
                            {
                                /*
                                if (this.HasLoaderPrompt())
                                {
                                    strError = logType.ToString() + "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Loader_Prompt(channel, e);
                                    if (e.ResultAction == "cancel")
                                        throw new ChannelException(channel.ErrorCode, strError);
                                    else if (e.ResultAction == "yes")
                                        continue;
                                    else
                                    {
                                        // no 也是抛出异常。因为继续下一批代价太大
                                        throw new ChannelException(channel.ErrorCode, strError);
                                    }
                                }
                                else
                                */
                                throw new ChannelException(channel.ErrorCode, strError);

                            }

                            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                            if (strOperation == "setReaderInfo")
                            {
                                var trace_result = TraceSetReaderInfo(
                                    dom,
                                    info);
                                if (trace_result.Value == -1)
                                    WpfClientInfo.WriteErrorLog("同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + trace_result.ErrorInfo);
                            }
                            else if (strOperation == "borrow" || strOperation == "return")
                            {
                                var trace_result = await TraceBorrowOrReturn(
                                    item,
                                    dom,
                                    info);
                                if (trace_result.Value == -1)
                                    WpfClientInfo.WriteErrorLog("同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + trace_result.ErrorInfo);
                            }
                            else if (strOperation == "setSystemParameter")
                            {
                                var trace_result = TraceSetSystemParameter(
                                    dom,
                                    info);
                                if (trace_result.Value == -1)
                                    WpfClientInfo.WriteErrorLog("同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + trace_result.ErrorInfo);
                            }
                            else if (strOperation == "writeRes")
                            {
                                var trace_result = TraceWriteRes(
                                    dom,
                                    info);
                                if (trace_result.Value == -1)
                                    WpfClientInfo.WriteErrorLog("同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + trace_result.ErrorInfo);
                            }
                            else if (strOperation == "setEntity"
                                && App.ReplicateEntities)
                            {
                                var trace_result = await EntityReplication.TraceSetEntity(
                                    dom,
                                    info);
                                if (trace_result.Value == -1)
                                    WpfClientInfo.WriteErrorLog("同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + trace_result.ErrorInfo);
                            }
                            else
                                continue;

#if NO
                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;

                                /*
                                if (this.HasLoaderPrompt())
                                {
                                    MessagePromptEventArgs e = new MessagePromptEventArgs
                                    {
                                        MessageText = strError + "\r\n\r\n是否跳过此条继续处理?\r\n\r\n(确定: 跳过;  取消: 停止全部操作)",
                                        IncludeOperText = true,
                                        // + "\r\n\r\n是否跳过此条继续处理?",
                                        Actions = "yes,cancel"
                                    };
                                    Loader_Prompt(channel, e);
                                    if (e.ResultAction == "cancel")
                                        throw new Exception(strError);
                                    else if (e.ResultAction == "yes")
                                        continue;
                                    else
                                    {
                                        // no 也是抛出异常。因为继续下一批代价太大
                                        throw new Exception(strError);
                                    }
                                }
                                else
                                */
                                throw new ChannelException(channel.ErrorCode, strError);
                            }
#endif

                            // lProcessCount++;
                            CONTINUE:
                            // 便于循环外获得这些值
                            strLastItemDate = item.Date;
                            lLastItemIndex = item.Index + 1;

                            // index = 0;  // 第一个日志文件后面的，都从头开始了
                        }
                        // 记忆
                        strLastDate = strLastItemDate;
                        last_index = lLastItemIndex;
                    }
                    finally
                    {
                        // loader.Prompt -= Loader_Prompt;
                        channel.Timeout = old_timeout;
                    }
                }

                return new ReplicationResult
                {
                    Value = last_index == -1 ? 0 : 1,
                    LastDate = strLastDate,
                    LastIndex = last_index,
                    ProcessInfo = info
                };
            }
            catch (ChannelException ex)
            {
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ProcessInfo = info
                };
            }
            catch (InterruptException ex)
            {
                // 2019/7/4
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ProcessInfo = info
                };
            }
            catch (Exception ex)
            {
                string strError = "DoReplication() exception: " + ExceptionUtil.GetDebugText(ex);
                return new ReplicationResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    ProcessInfo = info
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // Borrow() API 恢复动作
        /*
<root>
  <operation>borrow</operation> 操作类型
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <itemBarcode>0000001</itemBarcode>  册条码号
  <borrowDate>Fri, 08 Dec 2006 04:17:31 GMT</borrowDate> 借阅日期
  <borrowPeriod>30day</borrowPeriod> 借阅期限
  <no>0</no> 续借次数。0为首次普通借阅，1开始为续借
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:31 GMT</operTime> 操作时间
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
</root>
         * */
        // Return() API 恢复动作
        /*
<root>
  <operation>return</operation> 操作类型
  <action>return</action> 动作。有 return/lost/inventory/read/boxing 几种。恢复动作目前仅恢复 return 和 lost 两种，其余会忽略
  <itemBarcode>0000001</itemBarcode> 册条码号
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
  <overdues>...</overdues> 超期信息 通常内容为一个字符串，为一个<overdue>元素XML文本片断
  
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
  
</root>

         * */
        static async Task<NormalResult> TraceBorrowOrReturn(
            OperLogItem item,
            XmlDocument domLog,
            ProcessInfo info)
        {
            try
            {
                string strOperation = DomUtil.GetElementText(domLog.DocumentElement, "operation");

                string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
    "readerBarcode");
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "<readerBarcode>元素值为空"
                    };
                }

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");
                var operTime = DateTimeUtil.FromRfc1123DateTimeString(strOperTime);

                // 检查缓存的读者记录的最后更新时间
                var patron = LibraryChannelUtil.GetPatronItem(strReaderBarcode);
                if (patron == null || patron.LastWriteTime < operTime)
                {
                    DateTime now = /*DateTime*/ShelfData.Now;
                    var get_result = GetReaderInfo(strReaderBarcode);
                    if (get_result.Value == 1)
                    {
                        // parameters:
                        //          lastWriteTime   最后写入时间。采用服务器时间
                        UpdateLocalPatronRecord(get_result, now);
                    }
                }

                // 别处的还书动作兑现到 dp2ssl 本地动作库
                if (strOperation == "return" && strAction == "return")
                {
                    string borrowID = DomUtil.GetElementText(domLog.DocumentElement, "borrowID");
                    if (string.IsNullOrEmpty(borrowID) == false)
                    {
                        await ShelfData.ChangeDatabaseBorrowStateAsync(borrowID);
                    }
                    else
                    {
                        // 2021/8/17
                        // 提醒手动处理
                        WpfClientInfo.WriteErrorLog($"*** dp2library 操作日志({item.Date} {item.Index}) return 记录缺乏 borrowID 元素，请手动检查处理，在 dp2ssl 本地动作库内消除对应的借阅动作的在借状态");
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"TraceBorrowOrReturn() 出现异常: {ex.Message}"
                };
            }
            /*
            // 读入册记录
            string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                "confirmItemRecPath");
            string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                "itemBarcode");
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "<strItemBarcode>元素值为空"
                };
            }

            string strBorrowDate = SQLiteUtil.GetLocalTime(DomUtil.GetElementText(domLog.DocumentElement,
                "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowPeriod");
            */
        }

        // SetReaderInfo() API 恢复动作
        /*
<root>
    <operation>setReaderInfo</operation> 操作类型
    <action>...</action> 具体动作。有new change delete move 4种
    <record recPath='...'>...</record> 新记录
    <oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
    <changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
    <operator>test</operator> 操作者
    <operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

         * */
        static NormalResult TraceSetReaderInfo(
        XmlDocument domLog,
        ProcessInfo info)
        {
            try
            {
                string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

                string strOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");
                DateTime operTime = DateTimeUtil.FromRfc1123DateTimeString(strOperTime).ToLocalTime();

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strNewRecPath = "";
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        // 2019/11/5
                        // 注: move 操作，分馆账户获得日志记录时候可能会被 dp2library 滤除 record 元素。
                        // 此种情况可以理解为 delete 操作
                        if (strAction != "move")
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"日志记录中缺<record>元素。日志记录内容如下：{domLog.OuterXml}"
                            };
                        }
                    }
                    else
                    {
                        strNewRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    string strOldRecord = "";
                    string strOldRecPath = "";
                    // if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        /*
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }*/

                        if (node != null)
                        {
                            strOldRecPath = DomUtil.GetAttr(node, "recPath");
                            if (string.IsNullOrEmpty(strOldRecPath) == true)
                            {
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = "日志记录中<oldRecord>元素内缺recPath属性值"
                                };
                            }
                        }

                        // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                        // 注意：如果 record 元素都不存在，则应该理解为 delete。如果 record 元素存在，即 recPath 属性存在但 InnerText 不存在，则当作移动过程记录没有变化，即采用 oldRecord 的 InnerText 作为新记录内容
                        if (string.IsNullOrEmpty(strRecord) == true
                            && string.IsNullOrEmpty(strNewRecPath) == false)
                            strRecord = strOldRecord;
                    }

                    // TODO: change 动作也可能删除 face 元素

                    /*
                    // 删除旧记录对应的指纹缓存
                    if (strAction == "move"
                        && string.IsNullOrEmpty(strOldRecord) == false)
                    {
                        if (DeleteFingerPrint(strOldRecord, info, out strError) == -1)
                            return -1;
                    }
                    */


                    /*
                    if (ModifyFingerPrint(strOldRecord,
                        strRecord,
                        info,
                        out strError) == -1)
                        return -1;
                        */

                    // 把读者记录保存(更新)到本地数据库
                    // parameters:
                    //          lastWriteTime   最后写入时间。采用服务器时间
                    return LibraryChannelUtil.UpdateLocalPatronRecord(
                        new GetReaderInfoResult
                        {
                            RecPath = string.IsNullOrEmpty(strNewRecPath) ? null : strNewRecPath, // null 表示不加以修改
                            ReaderXml = strRecord,
                            Timestamp = null,
                        },
                        operTime);
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "日志记录中缺<oldRecord>元素"
                        };
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    /*
                    if (string.IsNullOrEmpty(strOldRecord) == false)
                    {
                        if (DeleteFingerPrint(strOldRecord, info, out strError) == -1)
                            return -1;
                    }
                    */

                    /*
                    if (ModifyFingerPrint(strOldRecord,
        "<root />",
        info,
        out strError) == -1)
                        return -1;
                        */
                    return LibraryChannelUtil.DeleteLocalPatronRecord(strOldRecord);
                }
                else
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "无法识别的<action>内容 '" + strAction + "'"
                    };
                }
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"TraceSetReaderInfo() 出现异常: {ex.Message}"
                };
            }
        }


        /*
<root>
  <operation>setSystemParameter</operation>
  <category>circulation</category>
  <name>rightsTable</name>
  <value>...</value>
  <libraryCodeList>
  </libraryCodeList>
  <operator>supervisor</operator>
  <operTime>Fri, 28 Aug 2020 12:02:28 +0800</operTime>
  <clientAddress via="net.pipe://localhost/dp2library/xe">localhost</clientAddress>
  <version>1.08</version>
</root>
         * */
        static NormalResult TraceSetSystemParameter(
XmlDocument domLog,
ProcessInfo info)
        {
            try
            {
                string strCategory = DomUtil.GetElementText(domLog.DocumentElement, "category");
                string strName = DomUtil.GetElementText(domLog.DocumentElement, "name");

                if (strCategory == "circulation" && strName == "rightsTable")
                {
                    // 获得读者借阅权限定义
                    var result = ShelfData.GetRightsTableFromServer();
                    if (result.Value == -1)
                    {
                        WpfClientInfo.WriteErrorLog($"同步获取读者借阅权限定义时出错: {result.ErrorInfo}");
                        // TODO: 延时后尝试重新获取?
                    }
                    else
                    {
                        string strOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");
                        // DateTime operTime = DateTimeUtil.FromRfc1123DateTimeString(strOperTime);

                        WpfClientInfo.WriteInfoLog($"更新读者权限定义。操作日志创建时间 {strOperTime}");
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"TraceSetSystemParameter() 出现异常: {ex.Message}"
                };
            }
        }

        public static bool PatronDataExists()
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    context.Database.EnsureCreated();
                    return context.Patrons.Any();
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"PatronDataExists() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return false;
            }
        }

        /*
<root>
  <operation>writeRes</operation>
  <operator>supervisor</operator>
  <operTime>Mon, 21 Sep 2020 11:01:56 +0800</operTime>
  <requestResPath>读者/1/object/0</requestResPath>
  <resPath>读者/0000000001/object/0</resPath>
  <ranges>0-72044</ranges>
  <totalLength>72045</totalLength>
  <metadata>&lt;file mimetype="image/pjpeg" localpath="a10d0d2e-c3e7-44f4-83ab-e31f7608d429" /&gt;</metadata>
  <style>
  </style>
  <clientAddress via="net.pipe://localhost/dp2library/xe">localhost</clientAddress>
  <version>1.06</version>
</root>
        * */
        static NormalResult TraceWriteRes(
XmlDocument domLog,
ProcessInfo info)
        {
            try
            {
                string strResPath = DomUtil.GetElementText(domLog.DocumentElement, "requestResPath");
                if (string.IsNullOrEmpty(strResPath) == false)
                {
                    var ret = PatronControl.ClearPhotoCacheFile(strResPath);
                    if (ret == true)
                        WpfClientInfo.WriteInfoLog($"路径 {strResPath} 对应的照片本地缓存文件被同步过程删除");
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"TraceWriteRes() 出现异常: {ex.Message}"
                };
            }
        }


    }
}
