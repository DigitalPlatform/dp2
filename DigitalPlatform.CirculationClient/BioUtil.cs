using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform.Interfaces;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 和生物识别有关的实用功能
    /// 主要是从读者记录中析出 fingerprint 和 face 信息
    /// </summary>
    public class BioUtil
    {
        public class ReplicationResult : NormalResult
        {
            public string LastDate { get; set; }
            public long LastIndex { get; set; }
        }

        // 同步
        // 注：中途遇到异常(例如 Loader 抛出异常)，可能会丢失 INSERT_BATCH 条以内的日志记录写入 operlog 表
        // parameters:
        //      strLastDate   处理中断或者结束时返回最后处理过的日期
        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        public static ReplicationResult DoReplication(
            LibraryChannel channel,
            string strStartDate,
            string strEndDate,
            LogType logType,
            CancellationToken token,
            MessagePromptEventHandler Loader_Prompt,
            Delegate_AddItems func)
        {
            string strLastDate = "";
            long last_index = -1;    // -1 表示尚未处理

            // bool bUserChanged = false;

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

            try
            {
                List<string> dates = null;
                int nRet = OperLogLoader.MakeLogFileNames(strStartDate,
                    strEndDate,
                    true,  // 是否包含扩展名 ".log"
                    out dates,
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

                channel.Timeout = new TimeSpan(0, 1, 0);   // 一分钟


                // using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
                {
                    ProgressEstimate estimate = new ProgressEstimate();

                    OperLogLoader loader = new OperLogLoader
                    {
                        Channel = channel,
                        Stop = null, //  this.Progress;
                                     // loader.owner = this;
                        Estimate = estimate,
                        Dates = dates,
                        Level = 2,  // Program.MainForm.OperLogLevel;
                        AutoCache = false,
                        CacheDir = "",
                        LogType = logType,
                        Filter = "setReaderInfo"
                    };

                    loader.Prompt += Loader_Prompt;
                    try
                    {
                        // int nRecCount = 0;

                        string strLastItemDate = "";
                        long lLastItemIndex = -1;
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

                                if (Loader_Prompt != null)
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
                                    throw new ChannelException(channel.ErrorCode, strError);

                            }

                            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                            if (strOperation == "setReaderInfo")
                            {
                                nRet = TraceSetReaderInfo(
                                    func,
                                    dom,
                                    out strError);
                            }
                            else
                                continue;

                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;

                                if (Loader_Prompt != null)
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
                                    throw new ChannelException(channel.ErrorCode, strError);
                            }

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
                        loader.Prompt -= Loader_Prompt;
                    }
                }

                return new ReplicationResult
                {
                    Value = last_index == -1 ? 0 : 1,
                    LastDate = strLastDate,
                    LastIndex = last_index
                };
            }
            catch (Exception ex)
            {
                string strError = "ReportForm DoReplication() exception: " + ExceptionUtil.GetDebugText(ex);
                return new ReplicationResult { Value = -1, ErrorInfo = strError };
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
        static int TraceSetReaderInfo(
            Delegate_AddItems func,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domLog.DocumentElement, "action");

            if (strAction == "new"
                || strAction == "change"
                || strAction == "move")
            {
                string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<record>元素";
                    return -1;
                }
                string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                string strOldRecord = "";
                string strOldRecPath = "";
                if (strAction == "move")
                {
                    strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }

                    strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                    {
                        strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                        return -1;
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    if (string.IsNullOrEmpty(strRecord) == true)
                        strRecord = strOldRecord;
                }

                // 删除旧记录对应的指纹缓存
                if (strAction == "move"
                    && string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(func, strOldRecord, out strError) == -1)
                        return -1;
                }

                if (AddFingerPrint(func, strRecord, out strError) == -1)
                    return -1;
            }
            else if (strAction == "delete")
            {
                string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "oldRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                if (string.IsNullOrEmpty(strOldRecord) == false)
                {
                    if (DeleteFingerPrint(func, strOldRecord, out strError) == -1)
                        return -1;
                }
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        public delegate int Delegate_AddItems(
    List<FingerprintItem> items,
    out string strError);

        // 写入新记录的指纹缓存
        static int AddFingerPrint(Delegate_AddItems func,
            string strRecord,
            out string strError)
        {
            strError = "";

            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(strRecord);

            string strReaderBarcode = GetReaderBarcode(new_dom);
            if (string.IsNullOrEmpty(strReaderBarcode))
                return 0;
            string strFingerPrintString = DomUtil.GetElementText(new_dom.DocumentElement, "fingerprint");

            // TODO: 看新旧记录之间 fingerprint 之间的差异。有差异才需要覆盖进入高速缓存
            FingerprintItem item = new FingerprintItem
            {
                FingerprintString = strFingerPrintString,
                ReaderBarcode = strReaderBarcode
            };
            // return:
            //      0   成功
            //      其他  失败。错误码
            int nRet = func(
                new List<FingerprintItem> { item },
                out strError);
            if (nRet != 0)
                return -1;

            return 1;
        }

        static int DeleteFingerPrint(Delegate_AddItems func,
            string strOldRecord,
            out string strError)
        {
            strError = "";
            XmlDocument old_dom = new XmlDocument();
            old_dom.LoadXml(strOldRecord);

            string strReaderBarcode = GetReaderBarcode(old_dom);
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                FingerprintItem item = new FingerprintItem
                {
                    FingerprintString = "",
                    ReaderBarcode = strReaderBarcode
                };
                // return:
                //      0   成功
                //      其他  失败。错误码
                int nRet = func(
                    new List<FingerprintItem> { item },
                    out strError);
                if (nRet != 0)
                    return -1;
            }

            return 0;
        }

        static string GetReaderBarcode(XmlDocument dom)
        {
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "barcode");
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
                return strReaderBarcode;

            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID))
                return "";
            return "@refID:" + strRefID;
        }


    }
}

