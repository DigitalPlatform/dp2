using DigitalPlatform.IO;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Xml;
using Jint.Parser.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DigitalPlatform.LibraryServer
{
    // TODO: 中断了以后，再次启动如何自动从断点位置开始?
    /// <summary>
    /// 根据日志文件创建 mongodb 日志库的批处理任务
    /// </summary>
    public class BuildMongoOperDatabase : BatchTask
    {
        public BuildMongoOperDatabase(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.PerTime = 0;
        }

        public override string DefaultName
        {
            get
            {
                return "创建 MongoDB 日志库";
            }
        }

        // 是否应该停止处理
        public override bool Stopped
        {
            get
            {
                return this.m_bClosed;
            }
        }

#if REMOVED

        // 解析 开始 参数
        static int ParseLogRecoverStart(string strStart,
            out long index,
            out string strFileName,
            out string strError)
        {
            strError = "";
            index = 0;
            strFileName = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            int nRet = strStart.IndexOf('@');
            if (nRet == -1)
            {
                try
                {
                    index = Convert.ToInt64(strStart);
                }
                catch (Exception)
                {
                    strError = "启动参数 '" + strStart + "' 格式错误：" + "如果没有@，则应为纯数字。";
                    return -1;
                }
                return 0;
            }

            try
            {
                index = Convert.ToInt64(strStart.Substring(0, nRet).Trim());
            }
            catch (Exception)
            {
                strError = "启动参数 '" + strStart + "' 格式错误：'" + strStart.Substring(0, nRet).Trim() + "' 部分应当为纯数字。";
                return -1;
            }

            strFileName = strStart.Substring(nRet + 1).Trim();

            // 如果文件名没有扩展名，自动加上
            if (String.IsNullOrEmpty(strFileName) == false)
            {
                nRet = strFileName.ToLower().LastIndexOf(".log");
                if (nRet == -1)
                    strFileName = strFileName + ".log";
            }

            return 0;
        }

        // 解析通用启动参数
        // 格式
        /*
         * <root clearFirst='...' continueWhenError='...' />
         * clearFirst 缺省为 false
         * continueWhenError 缺省值为 false
         * */
        public static int ParseLogRecoverParam(string strParam,
            out bool bClearFirst,
            out bool bContinueWhenError,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bContinueWhenError = false;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数装入XML DOM时出错: " + ex.Message;
                return -1;
            }

            bClearFirst = DomUtil.GetBooleanParam(dom.DocumentElement,
                "clearFirst",
                false);
            // 2016/3/8
            bContinueWhenError = DomUtil.GetBooleanParam(dom.DocumentElement,
                "continueWhenError",
                false);
            return 0;
        }

#endif

        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            //if (this.App.HangupReason == HangupReason.LogRecover)
            //    return;
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";

            if (this.App.ChargingOperDatabase.Enabled == false)
            {
                this.AppendResultText("启动失败: 当前尚未启用 mongodb 各项数据库\r\n");
                return;
            }

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            long lStartIndex = 0;// 开始位置
            string strStartFileName = "";// 开始文件名
            int nRet = LogRecoverStart.ParseLogRecoverStart(startinfo.Start,
                out lStartIndex,
                out strStartFileName,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            //
            bool bClearFirst = false;
            bool bContinueWhenError = false;

            nRet = LogRecoverParam.ParseLogRecoverParam(startinfo.Param,
                out string strDirectory,
                out string strRecoverLevel,
                out bClearFirst,
                out bContinueWhenError,
                out string style,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            // 开始处理时的日期
            string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

            // 记忆当前最后一条操作日志记录的位置
            // return:
            //      -1  出错
            //      0   日志文件不存在，或者记录数为 0
            //      >0  记录数
            long lRecCount = GetOperLogCount(strEndDate,
                out strError);
            if (lRecCount == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.App.WriteErrorLog(this.Name + " 任务启动。");

            if (bClearFirst == true)
            {
                nRet = this.App.ChargingOperDatabase.Clear(out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("清除 ChargingOperDatabase 中全部记录时发生错误: " + strError + "\r\n");
                    return;
                }
            }

            bool bStart = false;
            if (String.IsNullOrEmpty(strStartFileName) == true)
            {
                // 做所有文件
                bStart = true;
            }

            if (string.IsNullOrEmpty(strDirectory))
            {
                strDirectory = this.App.OperLog.Directory;
                this.AppendResultText($"日志文件目录为 dp2library 默认操作日志目录 {strDirectory}\r\n");
            }
            else
                this.AppendResultText($"日志文件目录为前端指定的目录 {strDirectory}\r\n");


            // 列出所有日志文件
            DirectoryInfo di = new DirectoryInfo(strDirectory/*this.App.OperLog.Directory*/);

            FileInfo[] fis = di.GetFiles("*.log");

            Array.Sort(fis, (x, y) =>
            {
                return ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
            });

            foreach (FileInfo info in fis)
            {
                if (this.Stopped == true)
                    break;

                string strFileName = info.Name;

                this.AppendResultText("检查文件 " + strFileName + "\r\n");

                if (bStart == false)
                {
                    // 从特定文件开始做
                    if (string.CompareOrdinal(strStartFileName, strFileName) <= 0)  // 2015/9/12 从等号修改为 Compare
                    {
                        bStart = true;
                        if (lStartIndex < 0)
                            lStartIndex = 0;
                        // lStartIndex = Convert.ToInt64(startinfo.Param);
                    }
                }

                if (bStart == true)
                {
                    long lMax = -1;
                    if (strEndDate + ".log" == strFileName)
                        lMax = lRecCount;

                    nRet = DoOneLogFile(
                        strDirectory,
                        strFileName,
                        lStartIndex,
                        lMax,
                        bContinueWhenError,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    lStartIndex = 0;    // 第一个文件以后的文件就全做了
                }
            }

            this.AppendResultText("循环结束\r\n");
            this.App.WriteErrorLog(this.Name + "恢复 任务结束。");
            return;
        ERROR1:
            return;
        }

        // 获得一个日志文件中记录的总数
        // parameters:
        //      strDate 日志文件的日期，8 字符
        // return:
        //      -1  出错
        //      0   日志文件不存在，或者记录数为 0
        //      >0  记录数
        long GetOperLogCount(string strDate,
            out string strError)
        {
            strError = "";

            string strXml = "";
            long lRecCount = 0;

            string strStyle = "getcount";
            long lAttachmentLength = 0;
            long lRet = this.App.OperLog.GetOperLog(
                null,
                "*",
                strDate + ".log",
                -1,    // lIndex,
                -1, // lHint,
                strStyle,
                "", // strFilter
                out lRecCount,
                out strXml,
                out lAttachmentLength,
                out strError);
            if (lRet == 0)
            {
                lRecCount = 0;
                return 0;
            }
            if (lRet != 1)
                return -1;
            Debug.Assert(lRecCount >= 0, "");
            return lRecCount;
        }

        // 处理一个日志文件的恢复任务
        // parameters:
        //      strFileName 纯文件名
        //      lStartIndex 开始的记录（从0开始计数）
        //      lMax    最多处理多少个记录。-1 表示全部处理
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        int DoOneLogFile(
            string strDirectory,
            string strFileName,
            long lStartIndex,
            long lMax,
            bool bContinueWhenError,
            out string strError)
        {
            strError = "";

            this.AppendResultText("做文件 " + strFileName + "\r\n");

            var cache = new OperLogFileCache();
            string strTempFileName = this.App.GetTempFileName("logrecover");    // Path.GetTempFileName();
            try
            {
                Debug.Assert(this.App != null, "");
                long lIndex = 0;
                long lHint = -1;
                long lHintNext = -1;
                for (lIndex = lStartIndex; ; lIndex++)
                {
                    if (this.Stopped == true)
                        break;

                    if (lMax != -1 && lIndex >= lMax)
                        break;

                    string strXml = "";

                    if (lIndex != 0)
                        lHint = lHintNext;

                    SetProgressText(strFileName + " 记录" + (lIndex + 1).ToString());

                    using (Stream attachment = File.Create(strTempFileName))
                    {
                        // 获得一个日志记录
                        // parameters:
                        //      strFileName 纯文件名,不含路径部分
                        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
                        //              目前的含义是记录起始位置。
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   succeed
                        //      2   超过范围
                        int nRet = /*this.App.*/OperLogUtility.GetOperLog(
                            cache,
                            strDirectory,
                            //"*",
                            strFileName,
                            lIndex,
                            lHint,
                            "supervisor", // level-0
                            "", // strFilter // TODO: 可以考虑用过滤方式加快速度
                            (ref string xml, out string error) =>
                            {
                                error = "";
                                // 限制记录观察范围
                                // 虽然是全局用户，也要限制记录尺寸
                                int ret = OperLog.ResizeXml(
                                    "supervisor",
                                    "",
                                    ref xml,
                                    out error);
                                if (ret == -1)
                                {
                                    ret = 1;   // 只好返回
                                }
                                return ret;
                            },
                            out lHintNext,
                            out strXml,
                            attachment,
                            // out long lAttachmentLength, // attachment,
                            out strError);
                        if (nRet == -1)
                        {
                            // 2017/5/9
                            this.AppendResultText("*** 获得日志记录 " + strFileName + " " + (lIndex).ToString() + " 时发生错误：" + strError + "\r\n");
                            if (bContinueWhenError == false)
                                return -1;
                        }
                        if (nRet == 0)
                            return 0;
                        if (nRet == 2)
                        {
                            // 最后一条补充提示一下
                            if (((lIndex - 1) % 100) != 0)
                                this.AppendResultText("做日志记录 " + strFileName + " " + (lIndex).ToString() + "\r\n");
                            break;
                        }

                        // 处理一个日志记录

                        if ((lIndex % 100) == 0)
                            this.AppendResultText("做日志记录 " + strFileName + " " + (lIndex + 1).ToString() + "\r\n");

                        /*
                        // 测试时候在这里安排跳过
                        if (lIndex == 1 || lIndex == 2)
                            continue;
                        * */

                        nRet = DoOperLogRecord(strXml,
                            // attachment,
                            out strError);
                        if (nRet == -1)
                        {
                            this.AppendResultText("*** 做日志记录 " + strFileName + " " + (lIndex).ToString() + " 时发生错误：" + strError + "\r\n");
                            if (bContinueWhenError == false)
                                return -1;
                        }
                    }
                }

                return 0;
            }
            finally
            {
                File.Delete(strTempFileName);
                cache.Dispose();
            }
        }

        // 执行一个日志记录的动作
        int DoOperLogRecord(string strXml,
            // Stream attachment,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlDocument domOperLog = new XmlDocument();
            try
            {
                domOperLog.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "日志记录装载到 DOM 时出错: " + ex.Message;
                return -1;
            }

            string strOperation = DomUtil.GetElementText(domOperLog.DocumentElement,
                "operation");
            if (strOperation == "borrow"
                || strOperation == "return"
                || strOperation == "lost"/*2023/6/20*/)
            {
                nRet = AppendOperationBorrowReturn(this.App,
                    domOperLog,
                    strOperation,
                    out strError);
            }

            // 2024/1/19
            if (strOperation == "setReaderInfo")
            {
                nRet = TraceOperationSetReaderInfo(this.App,
    domOperLog,
    strOperation,
    out strError);
            }
            // 2024/2/6
            if (strOperation == "setEntity")
            {
                nRet = TraceOperationSetEntity(this.App,
    domOperLog,
    strOperation,
    out strError);
            }

            if (nRet == -1)
            {
                string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                        "action");
                strError = "operation=" + strOperation + ";action=" + strAction + ": " + strError;
                return -1;
            }

            return 0;
        }

        // 将一条 borrow 或 return 操作日志信息加入 mongodb 日志库
        // mongodb 日志库的意义在于提供借阅历史检索功能
        // 日志记录格式见:
        // https://github.com/DigitalPlatform/dp2/issues/1184
        public static int AppendOperationBorrowReturn(
            LibraryApplication app,
            XmlDocument domOperLog,
            string strOperation,
            out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            ChargingOperItem item = new ChargingOperItem();
            item.Operation = strOperation;
            item.Action = strAction;
            item.LibraryCode = DomUtil.GetElementText(domOperLog.DocumentElement,
                "libraryCode");
            item.ItemBarcode = DomUtil.GetElementText(domOperLog.DocumentElement,
                "itemBarcode");
            item.PatronBarcode = DomUtil.GetElementText(domOperLog.DocumentElement,
                "readerBarcode");

            // 2024/2/6
            string itemRefID = DomUtil.GetElementText(domOperLog.DocumentElement,
    "itemRefID");
            string readerRefID = DomUtil.GetElementText(domOperLog.DocumentElement,
                "readerRefID");
            if (string.IsNullOrEmpty(itemRefID) == false)
                item.ItemBarcode = $"@refID:{itemRefID}";
            else
            {
                var refID = GetRefID(domOperLog, "itemRecord");
                if (string.IsNullOrEmpty(refID) == false)
                    item.ItemBarcode = $"@refID:{refID}";
            }

            if (string.IsNullOrEmpty(readerRefID) == false)
                item.PatronBarcode = $"@refID:{readerRefID}";
            else
            {
                var refID = GetRefID(domOperLog, "readerRecord");
                if (string.IsNullOrEmpty(refID) == false)
                    item.PatronBarcode = $"@refID:{refID}";
            }

            {
                string strBiblioRecPath = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "biblioRecPath");
                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                    item.BiblioRecPath = strBiblioRecPath;
            }

            if (strOperation == "borrow")
            {
                item.Period = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "borrowPeriod");
                item.No = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "no");
            }

            // 2017/5/22
            string strVolume = DomUtil.GetElementText(domOperLog.DocumentElement,
    "volume");
            if (string.IsNullOrEmpty(strVolume) == false)
                item.Volume = strVolume;

#if NO
            if (strOperation == "return" && strAction == "read")
            {
                // no 用作卷册信息 ???
                item.No = DomUtil.GetElementText(domOperLog.DocumentElement,
    "no");
            }
#endif

            item.ClientAddress = DomUtil.GetElementText(domOperLog.DocumentElement,
                "clientAddress");
            item.Operator = DomUtil.GetElementText(domOperLog.DocumentElement,
                "operator");
            string strOperTime = DomUtil.GetElementText(domOperLog.DocumentElement,
                "operTime");
            try
            {
                item.OperTime = DateTimeUtil.FromRfc1123DateTimeString(strOperTime).ToLocalTime();
            }
            catch (Exception ex)
            {
                strError = "operTime 元素内容 '" + strOperTime + "' 格式错误:" + ex.Message;
                return -1;
            }

            // 2023/6/20
            // 从日志记录中提取 borrowDate 元素
            if (strAction == "return" || strAction == "lost")
            {
                string strBorrowDate = DomUtil.GetElementText(domOperLog.DocumentElement,
    "borrowDate");
                if (string.IsNullOrEmpty(strBorrowDate) == false)
                {
                    try
                    {
                        item.BorrowDate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate).ToLocalTime();
                    }
                    catch (Exception ex)
                    {
                        strError = $"AppendOperationBorrowReturn() 中 borrowDate 元素内容 '{strOperTime}' 格式错误:{ex.Message}\r\n日志记录 XML 如下:\r\n{domOperLog.OuterXml}";
                        app.WriteErrorLog(strError);
                    }
                }
            }

            app.ChargingOperDatabase.Add(item);
            return 0;
        }

        // 获得 readerRecord 元素或者 itemRecord 元素中的记录内容内的 refID 元素内容
        /*
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
         * 
         * */
        static string GetRefID(XmlDocument domOperLog,
            string elementName)
        {
            string record = DomUtil.GetElementText(domOperLog.DocumentElement,
                elementName);
            if (string.IsNullOrEmpty(record))
                return null;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(record);
            }
            catch
            {
                return null;
            }

            return DomUtil.GetElementText(dom.DocumentElement,
                "refID");
        }

        // 2024/1/19
        // 将一条 setreaderinfo 操作日志信息兑现到 mongodb 日志库。
        // 具体来说就是修改 mongodb 日志库中相关记录的证条码号
        // mongodb 日志库的意义在于提供借阅历史检索功能
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
        public static int TraceOperationSetReaderInfo(
            LibraryApplication app,
            XmlDocument domOperLog,
            string strOperation,
            out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            if (strAction == "new"
|| strAction == "change"
|| strAction == "move")
            {
                string strNewRecPath = "";
                string strRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "record",
                    out XmlNode node);
                if (node == null)
                {
                    // 注: move 操作，分馆账户获得日志记录时候可能会被 dp2library 滤除 record 元素。
                    // 此种情况可以理解为 delete 操作
                    if (strAction != "move")
                    {
                        strError = $"日志记录中缺<record>元素。日志记录内容如下：{domOperLog.OuterXml}";
                        return -1;
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
                    strOldRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node != null)
                    {
                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                        if (string.IsNullOrEmpty(strOldRecPath) == true)
                        {
                            strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                            return -1;
                        }
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    // 注意：如果 record 元素都不存在，则应该理解为 delete。如果 record 元素存在，即 recPath 属性存在但 InnerText 不存在，则当作移动过程记录没有变化，即采用 oldRecord 的 InnerText 作为新记录内容
                    if (string.IsNullOrEmpty(strRecord) == true
                        && string.IsNullOrEmpty(strNewRecPath) == false)
                        strRecord = strOldRecord;
                }

                // *** 新旧证条码号发生变化
                string old_barcode = GetPatronBarcode(strOldRecord);
                string new_barcode = GetPatronBarcode(strRecord);

                if (string.IsNullOrEmpty(old_barcode) == false
                    && string.IsNullOrEmpty(new_barcode) == false
                    && old_barcode != new_barcode)
                {
                    // 修改出纳历史库里面的全部证条码号
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangePatronBarcode(old_barcode, new_barcode);
                }

                // *** 新旧参考 ID 发生变化
                string old_refid = GetRecordRefID(strOldRecord);
                string new_refid = GetRecordRefID(strRecord);

                if (string.IsNullOrEmpty(old_refid) == false
                    && string.IsNullOrEmpty(new_refid) == false
                    && old_refid != new_refid)
                {
                    // 修改出纳历史库里面的全部册条码号
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangePatronBarcode($"@refID:{old_refid}", $"@refID:{new_refid}");
                }

                // *** 版本升级之前(mongodb中)的证条码号，替换为 @refID:xxx 形态
                // 把出纳历史库里面可能的册条码号修改为 @refID:形态
                if (string.IsNullOrEmpty(new_barcode) == false
                    && string.IsNullOrEmpty(old_refid)
                    && string.IsNullOrEmpty(new_refid) == false)
                {
                    if (app.ChargingOperDatabase != null
    && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangePatronBarcode(new_barcode, $"@refID:{new_refid}");
                }

            }
            else if (strAction == "delete")
            {
                string strOldRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "oldRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                string old_barcode = GetPatronBarcode(strOldRecord);

                // 删除出纳历史库里面的全部相关记录
                if (string.IsNullOrEmpty(old_barcode) == false)
                {
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.DeletePatronBarcode(old_barcode);
                }

                string old_refid = GetRecordRefID(strOldRecord);
                if (string.IsNullOrEmpty(old_refid) == false)
                {
                    if (app.ChargingOperDatabase != null
        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.DeletePatronBarcode($"@refID:{old_refid}");
                }
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        static string GetPatronBarcode(string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return null;
            }

            return DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
        }

        static string GetRecordRefID(string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return null;
            }

            return DomUtil.GetElementText(dom.DocumentElement,
                "refID");
        }


        // 2024/2/6
        // 将一条 setEntity 操作日志信息兑现到 mongodb 日志库。
        // 具体来说就是修改 mongodb 日志库中相关记录的册条码号
        // mongodb 日志库的意义在于提供借阅历史检索功能
        /* 日志记录格式
<root>
  <operation>setEntity</operation> 操作类型
  <action>new</action> 具体动作。有new change delete setuid transfer move。2019/7/30 增加 transfer，transfer 行为和 change 相似
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书实体/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书实体/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) <record>中的内容, 涉及到流通的<borrower><borrowDate><borrowPeriod>等, 在日志恢复阶段, 都应当无效, 这几个内容应当从当前位置库中记录获取, 和<record>中其他内容合并后, 再写入数据库
	3) 一次SetEntities()API调用, 可能创建多条日志记录。
         
         * */
        public static int TraceOperationSetEntity(
    LibraryApplication app,
    XmlDocument domOperLog,
    string strOperation,
    out string strError)
        {
            strError = "";

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            if (strAction == "new"
|| strAction == "change"
|| strAction == "move"
|| strAction == "transfer"
|| strAction == "setuid")
            {
                string strNewRecPath = "";
                string strRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "record",
                    out XmlNode node);
                if (node == null)
                {
                    // 注: move 操作，分馆账户获得日志记录时候可能会被 dp2library 滤除 record 元素。
                    // 此种情况可以理解为 delete 操作
                    if (strAction != "move")
                    {
                        strError = $"日志记录中缺<record>元素。日志记录内容如下：{domOperLog.OuterXml}";
                        return -1;
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
                    strOldRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node != null)
                    {
                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                        if (string.IsNullOrEmpty(strOldRecPath) == true)
                        {
                            strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                            return -1;
                        }
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    // 注意：如果 record 元素都不存在，则应该理解为 delete。如果 record 元素存在，即 recPath 属性存在但 InnerText 不存在，则当作移动过程记录没有变化，即采用 oldRecord 的 InnerText 作为新记录内容
                    if (string.IsNullOrEmpty(strRecord) == true
                        && string.IsNullOrEmpty(strNewRecPath) == false)
                        strRecord = strOldRecord;
                }

                // *** 新旧册条码号发生变化
                string old_barcode = GetPatronBarcode(strOldRecord);
                string new_barcode = GetPatronBarcode(strRecord);

                if (string.IsNullOrEmpty(old_barcode) == false
                    && string.IsNullOrEmpty(new_barcode) == false
                    && old_barcode != new_barcode)
                {
                    // 修改出纳历史库里面的全部册条码号
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangeItemBarcode(old_barcode, new_barcode);
                }

                // *** 新旧参考 ID 发生变化
                string old_refid = GetRecordRefID(strOldRecord);
                string new_refid = GetRecordRefID(strRecord);

                if (string.IsNullOrEmpty(old_refid) == false
                    && string.IsNullOrEmpty(new_refid) == false
                    && old_refid != new_refid)
                {
                    // 修改出纳历史库里面的全部册条码号
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangeItemBarcode($"@refID:{old_refid}", $"@refID:{new_refid}");
                }

                // *** 版本升级之前(mongodb中)的册条码号，替换为 @refID:xxx 形态
                // 把出纳历史库里面可能的册条码号修改为 @refID:形态
                if (string.IsNullOrEmpty(new_barcode) == false
                    && string.IsNullOrEmpty(old_refid)
                    && string.IsNullOrEmpty(new_refid) == false)
                {
                    if (app.ChargingOperDatabase != null
    && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.ChangeItemBarcode(new_barcode, $"@refID:{new_refid}");
                }
            }
            else if (strAction == "delete")
            {
                string strOldRecord = DomUtil.GetElementText(domOperLog.DocumentElement,
                    "oldRecord",
                    out XmlNode node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                /*
                string old_barcode = GetPatronBarcode(strOldRecord);

                // 删除出纳历史库里面的全部相关记录
                if (string.IsNullOrEmpty(old_barcode) == false)
                {
                    if (app.ChargingOperDatabase != null
                    && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.DeleteItemBarcode(old_barcode);
                }

                string old_refid = GetRecordRefID(strOldRecord);

                // 删除出纳历史库里面的全部相关记录
                if (string.IsNullOrEmpty(old_refid) == false)
                {
                    if (app.ChargingOperDatabase != null
                        && app.ChargingOperDatabase.Enabled)
                        app.ChargingOperDatabase.DeleteItemBarcode($"@refID:{old_refid}");
                }
                */
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

    }

}
