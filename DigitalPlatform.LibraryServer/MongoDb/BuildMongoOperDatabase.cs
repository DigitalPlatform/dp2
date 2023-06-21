using DigitalPlatform.IO;
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

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            long lStartIndex = 0;// 开始位置
            string strStartFileName = "";// 开始文件名
            int nRet = ParseLogRecoverStart(startinfo.Start,
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

            nRet = ParseLogRecoverParam(startinfo.Param,
                out bClearFirst,
                out bContinueWhenError,
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

            // 列出所有日志文件
            DirectoryInfo di = new DirectoryInfo(this.App.OperLog.Directory);

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

                    nRet = DoOneLogFile(strFileName,
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
        int DoOneLogFile(string strFileName,
            long lStartIndex,
            long lMax,
            bool bContinueWhenError,
            out string strError)
        {
            strError = "";

            this.AppendResultText("做文件 " + strFileName + "\r\n");

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
                int nRet = this.App.OperLog.GetOperLog(
                    "*",
                    strFileName,
                    lIndex,
                    lHint,
                    "", // level-0
                    "", // strFilter
                    out lHintNext,
                    out strXml,
                    out long lAttachmentLength, // attachment,
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

            return 0;
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
                        strError = $"AppendOperationBorrowReturn() 中 borrowDate 元素内容 '{ strOperTime}' 格式错误:{ ex.Message}\r\n日志记录 XML 如下:\r\n{domOperLog.OuterXml}";
                        app.WriteErrorLog(strError);
                    }
                }
            }

            app.ChargingOperDatabase.Add(item);
            return 0;
        }
    }

}
