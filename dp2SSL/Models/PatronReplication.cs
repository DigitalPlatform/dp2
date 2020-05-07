using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using static dp2SSL.LibraryChannelUtil;
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
        }

        // 整体获得全部读者记录以前，预备获得同步计划信息
        // 也就是第一次同步开始的位置信息
        static ReplicationPlan GetReplicationPlan(LibraryChannel channel)
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


        // 第一阶段：获得全部读者库记录，进入本地数据库
        // result.Value
        //      -1  出错
        //      >=0 实际获得的读者记录条数
        public static async Task<ReplicationPlan> DownloadAllPatronRecordAsync(CancellationToken token)
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为读者记录检索需要一定时间
            try
            {

                ReplicationPlan plan = GetReplicationPlan(channel);
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

                // 把超时时间改短一点
                channel.Timeout = TimeSpan.FromSeconds(20);

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

                            var result = Set(item, record);
                            if (result.Value == -1)
                            {
                                // TODO: 是否汇总报错信息？
                                continue;
                            }
                            context.Patrons.Add(item);

                            if ((i % 10) == 0)
                                await context.SaveChangesAsync(token);

                            i++;
                        }

                        await context.SaveChangesAsync(token);
                    }
                }

                return new ReplicationPlan
                {
                    Value = (int)hitcount,
                    StartDate = plan.StartDate
                };
            }
            catch (Exception ex)
            {
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
            }
        }

        static NormalResult Set(PatronItem patron,
            Record record)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(record.RecordBody.Xml);
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                    ErrorCode = "loadXmlError"
                };
            }

            // TODO: 如果 XML 记录尺寸太大，可以考虑删除一些无关紧要的元素以后进入 patron.Xml，避免溢出 SQLite 一条记录可以存储的最大尺寸

            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");

            string cardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
            cardNumber = cardNumber.ToUpper();
            if (string.IsNullOrEmpty(cardNumber) == false)
                cardNumber = "," + cardNumber + ",";

            patron.PII = pii;
            patron.RecPath = record.Path;
            patron.Bindings = cardNumber;
            patron.Xml = record.RecordBody.Xml;
            patron.Timestamp = record.RecordBody.Timestamp;

            return new NormalResult();
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
        public static ReplicationResult DoReplication(
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
                        Filter = "setReaderInfo",
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
            string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

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
                return LibraryChannelUtil.UpdateLocalPatronRecord(
                    new GetReaderInfoResult
                    {
                        RecPath = string.IsNullOrEmpty(strNewRecPath) ? null : strNewRecPath, // null 表示不加以修改
                        ReaderXml = strRecord,
                        Timestamp = null,
                    });
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


    }
}
