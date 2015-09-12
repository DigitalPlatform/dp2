using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.DTLP;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
#if NOOOOOOOOOO
    /// <summary>
    /// 跟踪DTLP数据库 批处理任务
    /// </summary>
    public class TraceDTLP : BatchTask
    {
        DtlpChannelArray DtlpChannels = new DtlpChannelArray();
        DtlpChannel DtlpChannel = null;

        // 临时记忆断点信息，避免繁琐参数传入多层调用
        string m_strStartFileName = "";
        int m_nStartIndex = 0;
        string m_strStartOffset = "";

        string m_strWarningFileName = "";


        // 构造函数
        public TraceDTLP(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            // this.App = app;

            this.PerTime = 1 * 60 * 1000;	// 1分钟

            this.Loop = true;

            this.DtlpChannels.GUI = false;

            this.DtlpChannels.AskAccountInfo -= new AskDtlpAccountInfoEventHandle(DtlpChannels_AskAccountInfo);
            this.DtlpChannels.AskAccountInfo += new AskDtlpAccountInfoEventHandle(DtlpChannels_AskAccountInfo);

            this.DtlpChannel = this.DtlpChannels.CreateChannel(0);

            // 警告信息文件。注意经常删除这个文件，否则会越来越大
            m_strWarningFileName = PathUtil.MergePath(app.LogDir, "dtlp_warning.txt");
        }

        void DtlpChannels_AskAccountInfo(object sender, AskDtlpAccountInfoEventArgs e)
        {
            e.Owner = null;

            string strUserName = "";
            string strPassword = "";
            string strError = "";
            int nRet = this.App.GetDtlpAccountInfo(e.Path,
                out strUserName,
                out strPassword,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                e.ErrorInfo = strError;
                e.Result = -1;
                return;
            }

            ///
            e.UserName = strUserName;
            e.Password = strPassword;
            e.Result = 1;
        }

        public override string DefaultName
        {
            get
            {
                return "跟踪DTLP数据库";
            }
        }

        // 构造断点字符串
        static string MakeBreakPointString(
            int indexLog,
            string strLogStartOffset,
            string strLogFileName,
            string strRecordID,
            string strOriginDbName)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // <dump>
            XmlNode nodeDump = dom.CreateElement("dump");
            dom.DocumentElement.AppendChild(nodeDump);

            DomUtil.SetAttr(nodeDump, "recordid", strRecordID);
            DomUtil.SetAttr(nodeDump, "origindbname", strOriginDbName);

            // <trace>
            XmlNode nodeTrace = dom.CreateElement("trace");
            dom.DocumentElement.AppendChild(nodeTrace);

            DomUtil.SetAttr(nodeTrace, "index", indexLog.ToString());
            DomUtil.SetAttr(nodeTrace, "startoffset", strLogStartOffset);
            DomUtil.SetAttr(nodeTrace, "logfilename", strLogFileName);

            return dom.OuterXml;
        }

        // 解析 开始 参数
        // 应该有一种办法，指定任务从上次中断位置重新开始，至于这个中断位置在哪里，靠服务器自己记忆
        // 这和Down机器记忆是采用同一功能
        // parameters:
        //      strStart    启动字符串。格式一般为index.offsetstring@logfilename
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParseTraceDtlpStart(string strStart,
            out int indexLog,
            out string strLogStartOffset,
            out string strLogFileName,
            out string strRecordID,
            out string strOriginDbName,
            out string strError)
        {
            strError = "";
            indexLog = 0;
            strLogFileName = "";
            strLogStartOffset = "";
            strRecordID = "";
            strOriginDbName = "";

            int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                strError = "启动参数不能为空";
                return -1;
            }

            if (strStart == "!breakpoint")
            {
                // 从断点记忆文件中读出信息
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = this.App.ReadBatchTaskBreakPointFile(
                    "跟踪DTLP数据库",
                    out strStart,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ReadBatchTaskBreakPointFile时出错：" + strError;
                    this.App.WriteErrorLog(strError);
                    return -1;
                }

                // 如果nRet == 0，表示没有断点文件存在，也就没有必要的参数来启动这个任务
                if (nRet == 0)
                {
                    strError = "当前服务器没有发现断点信息，无法启动任务";
                    return -1;
                }

                Debug.Assert(nRet == 1, "");
                this.AppendResultText("服务器记忆的上次断点字符串为: "
                    + HttpUtility.HtmlEncode(strStart)
                    + "\r\n");

            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strStart);
            }
            catch (Exception ex)
            {
                strError = "装载XML字符串进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode nodeDump = dom.DocumentElement.SelectSingleNode("dump");
            if (nodeDump != null)
            {
                strRecordID = DomUtil.GetAttr(nodeDump, "recordid");
                strOriginDbName = DomUtil.GetAttr(nodeDump, "origindbname");
            }

            XmlNode nodeTrace = dom.DocumentElement.SelectSingleNode("trace");
            if (nodeTrace != null)
            {
                string strIndex = DomUtil.GetAttr(nodeTrace, "index");
                if (String.IsNullOrEmpty(strIndex) == true)
                    indexLog = 0;
                else {
                try
                {
                    indexLog = Convert.ToInt32(strIndex);
                }
                catch
                {
                    strError = "<trace>元素中index属性值 '" +strIndex+ "' 格式错误，应当为纯数字";
                    return -1;
                }
                }

                strLogStartOffset = DomUtil.GetAttr(nodeTrace, "startoffs");
                strLogFileName = DomUtil.GetAttr(nodeTrace, "logfilename");
            }

            return 0;
        }

        /*
        int ParseTraceStartString(string strStart,
            out int index,
            out string strStartOffset,
            out string strFileName,
            out string strError)
        {
            nIndex = 0;
            strFileName = "";
            strStartOffset = "";

            string strIndex = "";

            nRet = strStart.IndexOf('@');
            if (nRet == -1)
            {
                nRet = strStart.IndexOf('.');
                if (nRet != -1)
                {
                    strIndex = strStart.Substring(0, nRet);
                    strStartOffset = strStart.Substring(nRet + 1);
                }
                else
                {
                    strIndex = strStart;
                    strStartOffset = "";
                }

                try
                {
                    index = Convert.ToInt32(strIndex);
                }
                catch (Exception)
                {
                    strError = "启动参数 '" + strIndex + "' 格式错误：" + "如果没有@，则应为纯数字。";
                    return -1;
                }
                return 0;
            }

            strIndex = strStart.Substring(0, nRet).Trim();
            strFileName = strStart.Substring(nRet + 1).Trim();

            nRet = strIndex.IndexOf('.');
            if (nRet != -1)
            {
                strStartOffset = strIndex.Substring(nRet + 1);
                strIndex = strIndex.Substring(0, nRet);
            }
            else
            {
                strStartOffset = "";
            }

            try
            {
                index = Convert.ToInt32(strIndex);
            }
            catch (Exception)
            {
                strError = "启动参数 '" + strIndex + "' 格式错误：'" + strIndex + "' 部分应当为纯数字。";
                return -1;
            }

            if (strFileName == "")
            {
                strError = "启动参数 '" + strStart + "' 格式错误：缺乏日志文件名";
                return -1;
            }

            return 0;
        }
         * */


        public static string MakeTraceDtlpParam(
            bool bDump,
            bool bClearFirst,
            bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "clearFirst",
                bClearFirst == true ? "yes" : "no");
            DomUtil.SetAttr(dom.DocumentElement, "dump",
                bDump == true ? "yes" : "no");
            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }


        // 解析通用启动参数
        // 格式
        /*
         * <root dump='...' clearFirst='...' loop='...'/>
         * dump缺省为false
         * clearFirst缺省为false
         * loop缺省为true
         * 
         * */
        public static int ParseTraceDtlpParam(string strParam,
            out bool bDump,
            out bool bClearFirst,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bDump = false;
            bLoop = true;

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


            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            string strDump = DomUtil.GetAttr(dom.DocumentElement,
    "dump");
            if (strDump.ToLower() == "yes"
                || strDump.ToLower() == "true")
                bDump = true;
            else
                bDump = false;

            // 缺省为true
            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
    "loop");
            if (strLoop.ToLower() == "no"
                || strLoop.ToLower() == "false")
                bLoop = false;
            else
                bLoop = true;

            return 0;
        }



        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            // 2007/12/18
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;

            string strError = "";
            int nRet = 0;

            // 每一次循环更换一个新的DtlpChannel，防止吊死在一棵树上
            this.DtlpChannel = this.DtlpChannels.CreateChannel(0);

            //

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 通用启动参数
            bool bDump = false;
            bool bClearFirst = false;
            bool bLoop = true;
            nRet = ParseTraceDtlpParam(startinfo.Param,
                out bDump,
                out bClearFirst,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;


            int nStartIndex = 0;// 开始位置
            string strStartFileName = "";// 开始文件名
            string strStartOffset = "";
            string strDumpRecordID = "";
            string strDumpOriginDbName = "";
            nRet = ParseTraceDtlpStart(startinfo.Start,
                out nStartIndex,
                out strStartOffset,
                out strStartFileName,
                out strDumpRecordID,
                out strDumpOriginDbName,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                this.Loop = false;
                return;
            }

            if (strDumpRecordID != "" && strDumpOriginDbName != "")
                bDump = true;

            if (bClearFirst == true)
            {
                nRet = ClearAllServerDbs(out strError);
                if (nRet == -1)
                {
                    string strErrorText = "初始化跟踪目标库失败: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.Loop = false;
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                this.AppendResultText("所有跟踪目标库已经被初始化\r\n");

                // 消除通用启动信息中的clearfirst值，以指导下次处理
                bClearFirst = false;
                startinfo.Param = MakeTraceDtlpParam(
                    bDump,
                    bClearFirst,
                    bLoop);

            }


            if (bDump == true)
            {
                // 重新启动的时候，要考察上次在哪个阶段中断的，要在正确的阶段重新执行

                this.AppendResultText("Dump开始\r\n");
                this.SetProgressText("Dump开始");

                // 写入文本文件
                if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                {
                    StreamUtil.WriteText(this.m_strWarningFileName,
                        "Dump开始。时间 "+DateTime.Now.ToString()+"\r\n");
                }

                string strBreakRecordID = "";
                string strBreakOriginDbName = "";

                // 临时记忆
                m_strStartFileName = strStartFileName;
                m_nStartIndex = nStartIndex;
                m_strStartOffset = strStartOffset;

                try
                {
                    nRet = DumpAllServerDbs(
                        strDumpRecordID,
                        strDumpOriginDbName,
                        out strBreakRecordID,
                        out strBreakOriginDbName,
                        out strError);
                }
                catch (Exception ex)
                {
                    strError = "DumpAllServerDbs exception: " + ExceptionUtil.GetDebugText(ex);
                    this.DtlpChannel = null;
                    this.AppendResultText(strError + "\r\n");
                    this.SetProgressText(strError);
                    // 写入文本文件
                    if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                    {
                        StreamUtil.WriteText(this.m_strWarningFileName,
                            strError + "\r\n");
                    }
                    nRet = -1;
                }

                if (nRet == -1)
                {
                    Debug.Assert(strBreakOriginDbName != "", "");
                    Debug.Assert(strBreakRecordID != "", "");
                    if (strBreakRecordID == ""
                        || strBreakOriginDbName == "")
                    {
                        strError = "dump出错的时候，strBreakRecordID[" + strBreakRecordID + "]和strBreakOriginDbName[" + strBreakOriginDbName + "]值均不应为空";
                        this.App.WriteErrorLog(strError);
                    }
                }

                if (nRet == 0)
                {
                    /*
                    Debug.Assert(strBreakOriginDbName == "", "");
                    Debug.Assert(strBreakRecordID == "", "");
                     * */
                    strBreakOriginDbName = "";
                    strBreakRecordID = "";
                }
                if (nRet == 1)
                {
                    Debug.Assert(strBreakOriginDbName != "", "");
                    Debug.Assert(strBreakRecordID != "", "");
                    if (strBreakRecordID == ""
                        || strBreakOriginDbName == "")
                    {
                        strError = "dump中断的时候，strBreakRecordID[" + strBreakRecordID + "]和strBreakOriginDbName[" + strBreakOriginDbName + "]值均不应为空";
                        this.App.WriteErrorLog(strError);
                    }

                }

                // 记忆断点
                this.StartInfo.Start = MemoBreakPoint(
                    strStartFileName,
                    nStartIndex,
                    strStartOffset,
                    strBreakRecordID, //strRecordID,
                    strBreakOriginDbName //strOriginDbName
                    );

                if (nRet == -1)
                {
                    string strErrorText = "Dump失败: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);

                    this.Loop = true;   // 便于稍后继续重新循环?
                    startinfo.Param = MakeTraceDtlpParam(
                        bDump,
                        bClearFirst,
                        bLoop);
                    return;
                }

                if (nRet == 1)
                {
                    this.AppendResultText("Dump中断。断点为"
                        + HttpUtility.HtmlEncode(this.StartInfo.Start)
                        + "\r\n");
                    this.SetProgressText("Dump中断。断点为"
                        + HttpUtility.HtmlEncode(this.StartInfo.Start));

                    this.Loop = false;
                    return;
                }
                else {
                    this.AppendResultText("Dump结束\r\n");
                    this.SetProgressText("Dump结束");

                    // 写入文本文件
                    if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                    {
                        StreamUtil.WriteText(this.m_strWarningFileName,
                            "Dump结束。时间 " + DateTime.Now.ToString() + "\r\n");
                        this.AppendResultText("警告信息已经写入文件 " + this.m_strWarningFileName + "\r\n");
                    }

                    /*
                    // 记忆断点，消除dump相关事项
                    this.StartInfo.Start = MemoBreakPoint(
                        strStartFileName,
                        nStartIndex,
                        strStartOffset,
                        "", //strRecordID,
                        "" //strOriginDbName
                        );
                     * */

                    // 消除通用启动信息中的dump值，以指导下次处理
                    bDump = false;
                    startinfo.Param = MakeTraceDtlpParam(
                        bDump,
                        bClearFirst,
                        bLoop);

                }
            } // end of -- if (bDump == true)

            //

            string strStartLogFileName = strStartFileName;

            string strFinishLogFileName = "";
            int nFinishIndex = -1;
            string strFinishOffset = "";

            // 获得服务器列表
            XmlNodeList originNodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("//traceDTLP/origin");

            for (int i = 0; i < originNodes.Count; i++)
            {
                if (this.Stopped == true)
                    break;


                XmlNode originNode = originNodes[i];
                string strServerAddr = DomUtil.GetAttr(originNode, "serverAddr");

                if (String.IsNullOrEmpty(strServerAddr) == true)
                    continue;

                // 跟踪这个服务器的日志，直到当天日志文件的末尾
                nRet = TraceOneServerLogs(strServerAddr,
                    strStartLogFileName,
                    nStartIndex,
                    strStartOffset,
                    out strFinishLogFileName,
                    out nFinishIndex,
                    out strFinishOffset,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "跟踪服务器 " + strServerAddr + " 失败: " + strError + "\r\n";
                    this.AppendResultText(strErrorText);

                    this.App.WriteErrorLog(strErrorText);
                }
            }

            if (strFinishLogFileName == "")
                strFinishLogFileName = strStartFileName;

            if (nFinishIndex == -1)
            {
                nFinishIndex = 0;
                strFinishOffset = "";
            }

            Debug.Assert(strFinishLogFileName != "", "");

            /*
            if (strFinishOffset != "")
                this.StartInfo.Start = nFinishIndex.ToString() + "." + strFinishOffset // .后面是偏移量暗示值
                    + "@" + strFinishLogFileName;  // 迫使下一轮循环采用新值 +1？
            else
                this.StartInfo.Start = nFinishIndex.ToString() + "@" + strFinishLogFileName;  // 迫使下一轮循环采用新值

            // 写入断点文件
            // 为了增强动态效果，可以在循环中途每处理多少条后写入一次
            this.App.WriteBatchTaskBreakPointFile(this.Name,
                this.StartInfo.Start);
             * */
            this.StartInfo.Start = MemoBreakPoint(
                strFinishLogFileName,
                nFinishIndex,
                strFinishOffset,
                "",
                "");


            if (this.Stopped == true)
            {
                this.AppendResultText("任务已停止，断点为 " 
                    + HttpUtility.HtmlEncode(this.StartInfo.Start) 
                    + "\r\n");
                this.ProgressText = "任务已停止";
            }
        }

        // 记忆一下断点，以备不测
        string MemoBreakPoint(
            string strFinishLogFileName,
            int nFinishIndex,
            string strFinishOffset,
            string strRecordID,
            string strOriginDbName)
        {
            string strBreakPointString = "";

            /*
            if (strFinishOffset != "")
                strBreakPointString = nFinishIndex.ToString() + "." + strFinishOffset // .后面是偏移量暗示值
                    + "@" + strFinishLogFileName;  // 迫使下一轮循环采用新值 +1？
            else
                strBreakPointString = nFinishIndex.ToString() + "@" + strFinishLogFileName;  // 迫使下一轮循环采用新值
             * */

            strBreakPointString = MakeBreakPointString(
                nFinishIndex,
                strFinishOffset,
                strFinishLogFileName,
                strRecordID,
                strOriginDbName);

            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.Name,
                strBreakPointString);

            return strBreakPointString;
        }

        // 跟踪一个服务器的日志，直到当天日志文件的末尾
        // parameters:
        //      strFinishLogFileName    结束时的一个文件名
        //      nFinishIndex    结束的记录偏移(已经成功处理的记录，这条已经成功处理)
        // return:
        //      -1  error
        //      0   succeed
        public int TraceOneServerLogs(string strServerAddr,
            string strStartLogFileName,
            int nStartIndex,
            string strStartOffset,
            out string strFinishLogFileName,
            out int nFinishIndex,
            out string strFinishOffset,
            out string strError)
        {
            strError = "";
            strFinishLogFileName = "";
            nFinishIndex = -1;
            strFinishOffset = "";

            int nRet = 0;

            string strLogFileName = strStartLogFileName;

            for (int i = 0; ; i++)
            {
                if (this.Stopped == true)
                    break;

                // 用临时变量的好处是，如果本次GetOneFileLogRecords()失败，
                // 上次遗留的finish参数还存在，这就保留了“最近成功的一次”的参数
                int nTempFinishIndex = -1;
                string strTempFinishOffset = "";

                // 获得一个特定日志文件内的全部日志记录
                // return:
                //      -1  出错
                //      0   日志文件不存在
                //      1   日志文件存在
                nRet = GetOneFileLogRecords(strServerAddr,
                    strLogFileName,
                    nStartIndex,
                    strStartOffset,
                    out nTempFinishIndex,
                    out strTempFinishOffset,
                    out strError);
                if (nRet == -1)
                    return -1;

                strFinishLogFileName = strLogFileName;
                strFinishOffset = strTempFinishOffset;
                nFinishIndex = nTempFinishIndex;

                if (nFinishIndex == -1)
                {
                    nFinishIndex = 0;
                    strFinishOffset = "";   // dt1500内核有bug，如果这里给出一个残余的很大值，超过实际文件的长度很多，就会导致内核垮掉
                }

                // 记忆一下断点，以备不测
                MemoBreakPoint(
                    strFinishLogFileName,
                    nFinishIndex,
                    strFinishOffset,
                    "",
                    "");


                // 下一个文件名

                string strNextLogFileName = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = NextLogFileName(strLogFileName,
                    out strNextLogFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                    break;

                nStartIndex = 0;    // 从第二个文件以后，起点为0
                strStartOffset = "";    // dt1500内核有bug，如果这里给出一个残余的很大值，超过实际文件的长度很多，就会导致内核垮掉

                strLogFileName = strNextLogFileName;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且strLogFileName已经是今天的日子了
        static int NextLogFileName(string strLogFileName,
            out string strNextLogFileName,
            out string strError)
        {
            strError = "";
            strNextLogFileName = "";
            int nRet = 0;

            string strYear = strLogFileName.Substring(0, 4);
            string strMonth = strLogFileName.Substring(4, 2);
            string strDay = strLogFileName.Substring(6, 2);

            int nYear = 0;
            int nMonth = 0;
            int nDay = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strYear + "' 部分格式错误";
                return -1;
            }

            try
            {
                nMonth = Convert.ToInt32(strMonth);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strMonth + "' 部分格式错误";
                return -1;
            }

            try
            {
                nDay = Convert.ToInt32(strDay);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strDay + "' 部分格式错误";
                return -1;
            }

            DateTime time = DateTime.Now;
            try
            {
                time = new DateTime(nYear, nMonth, nDay);
            }
            catch (Exception ex)
            {
                strError = "日期 "+strLogFileName+" 格式错误: " + ex.Message;
                return -1;
            }

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = LibraryApplication.RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = LibraryApplication.RoundTime("day",
                ref time,
                out strError);
            if (nRet == -1)
                return -1;

            bool bNow = false;
            if (time >= now)
                bNow = true;
            
            time = time + new TimeSpan(1, 0, 0, 0); // 后面一天

            strNextLogFileName = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }

        // 获得一个特定日志文件内的全部日志记录
        // return:
        //      -1  出错
        //      0   日志文件不存在
        //      1   日志文件存在
        int GetOneFileLogRecords(string strServerAddr,
            string strLogFileName,
            int nStartIndex,
            string strStartOffset,
            out int nFinishIndex,
            out string strFinishOffset,
            out string strError)
        {
            strError = "";
            nFinishIndex = -1;  // -1表示尚未处理
            strFinishOffset = "";

            string strPath = strServerAddr + "/log/" + strLogFileName + "/" + nStartIndex.ToString();

            if (strStartOffset != "")
                strPath += "@" + strStartOffset;

            bool bFirst = true;

            string strDate = "";
            int nRecID = -1;
            string strOffset = "";

            int nStyle = 0;

            if (nStartIndex < 0)
                nStartIndex = 0;

            for (nRecID = nStartIndex; ;)
            {
                if (this.Stopped == true)
                    break;

                byte[] baPackage = null;

                if (bFirst == true)
                {
                }
                else
                {
                    strPath = strServerAddr + "/log/" + strDate/*strLogFileName*/ + "/" + nRecID.ToString() + "@" + strOffset;
                }

                Encoding encoding = this.DtlpChannel.GetPathEncoding(strPath);

                this.AppendResultText("做 " + strPath + "\r\n");
                this.ProgressText = strPath;


                int nRet = this.DtlpChannel.Search(strPath,
                    DtlpChannel.RIZHI_STYLE | nStyle,
                    out baPackage);
                if (nRet == -1)
                {
                    int errorcode = this.DtlpChannel.GetLastErrno();
                    if (errorcode == DtlpChannel.GL_NOTEXIST)
                    {
                        if (bFirst == true)
                            break;
                    }
                    strError = "获取日志记录:\r\n"
                        + "路径: " + strPath + "\r\n"
                        + "错误码: " + errorcode + "\r\n"
                        + "错误信息: " + this.DtlpChannel.GetErrorString(errorcode) + "\r\n";
                    return -1;
                }


                // 解析出记录
                Package package = new Package();
                package.LoadPackage(baPackage,
                    encoding);
                package.Parse(PackageFormat.Binary);

                // 获得下一路径
                string strNextPath = "";
                strNextPath = package.GetFirstPath();
                if (String.IsNullOrEmpty(strNextPath) == true)
                {
                    if (bFirst == true)
                    {
                        strError = "文件 " + strLogFileName + "不存在";
                        return 0;
                    }
                    // strError = "检索 '" + strPath + "' 响应包中路径部分不存在 ...";
                    // return -1;
                    break;
                }

                // 获得记录内容
                byte[] baContent = null;
                nRet = package.GetFirstBin(out baContent);
                if (nRet != 1)
                {
                    baContent = null;	// 但是为空包
                }

                // 处理记录
                /*
                Debug.Assert(nRecID == i, "");
                if (nRecID != i)
                {
                    strError = "nRecID=" + nRecID.ToString() + " 和i=" + i.ToString() + " 不同步";
                    return -1;
                    // 是否修正错误后继续处理?
                }*/

                // 记忆参数
                nFinishIndex = nRecID + 1;  // 因为后面的offset是用来获取下一条的，所以应当和下一条的id配套
                strFinishOffset = strOffset;


                // 每处理100条记忆一下断点，以备不测
                if ((nRecID - nStartIndex) % 100 == 0)
                {
                    MemoBreakPoint(
                        strLogFileName,
                        nFinishIndex,
                        strFinishOffset,
                        "",
                        "");
                }



                string strMARC = DtlpChannel.GetDt1000LogRecord(baContent, encoding);

                string strOperCode = "";
                string strOperComment = "";
                string strOperPath = "";

                nRet = DtlpChannel.ParseDt1000LogRecord(strMARC,
                    out strOperCode,
                    out strOperComment,
                    out strOperPath,
                    out strError);
                if (nRet == -1)
                {
                    strOperComment = strError;
                }


                if (strOperCode == "00" || strOperCode == "02")
                {
                    try
                    {
                        // return:
                        //      -1  error
                        //      0   发现库名不在目标数据库之列
                        //      1   成功写入
                        nRet = WriteMarcRecord(
                            strOperCode,
                            strServerAddr,
                            strOperPath,
                            strMARC,
                            out strError);
                    }
                    catch (Exception ex)
                    {
                        strError = "\r\nstrOperCode='" + strOperCode + "'\r\n"
                            + "strServerAddr='" + strServerAddr + "'\r\n"
                            + "strOperPath='" + strOperPath + "'\r\n"
                            + "strMARC='" + strMARC + "'\r\n"
                            + "------\r\n";
                        this.App.WriteErrorLog("-- WriteMarcRecord()抛出异常: " + ex.Message + " --" + strError);
                        nRet = -2;
                    }

                    if (nRet == -2)
                    {
                        // 路径格式错误
                        // 继续
                    }
                    if (nRet == -1)
                    {
                        this.App.WriteErrorLog(strError);
                        // 报错后继续处理
                    }


                }
                else if (strOperCode == "12")
                {
                    nRet = InitialDB(
                        strServerAddr,
                        strOperPath,
                        out strError);
                    if (nRet == -1)
                    {
                        this.App.WriteErrorLog(strError);
                        // 报错后继续处理
                    }
                }


                // 将日志记录路径解析为日期、序号、偏移
                // 一个日志记录路径的例子为:
                // /ip/log/19991231/0@1234~5678
                // parameters:
                //		strLogPath		待解析的日志记录路径
                //		strDate			解析出的日期
                //		nRecID			解析出的记录号
                //		strOffset		解析出的记录偏移，例如1234~5678
                // return:
                //		-1		出错
                //		0		正确
                nRet = DtlpChannel.ParseLogPath(strNextPath,
                    out strDate,
                    out nRecID,
                    out strOffset,
                    out strError);
                if (nRet == -1)
                    return -1;

                // CONTINUE:

                bFirst = false;
            }


            return 1;   // 日志文件存在，已获得了记录
        }

        // 获得目标数据库名
        int GetTargetDbName(string strTargetServerAddr,
            string strOriginDbName,
            out string strTargetDbName,
            out string strMarcSyntax,
            out string strError)
        {
            strTargetDbName = "";
            strError = "";
            strMarcSyntax = "";

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//traceDTLP/origin[@serverAddr='" + strTargetServerAddr + "']/databaseMap/item[@originDatabase='"+strOriginDbName+"']");
            if (node == null)
                return 0;   // not found

            strTargetDbName = DomUtil.GetAttr(node, "targetDatabase");
            strMarcSyntax = DomUtil.GetAttr(node, "marcSyntax");
            return 1;   // found
        }

        // 初始化数据库
        // parameters:
        // return:
        //      -1  error
        //      0   发现库名不在目标数据库之列
        //      1   成功初始化
        int InitialDB(
            string strTargetServerAddr,
            string strOriginDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            // 映射到目标数据库名
            string strTargetDbName = "";
            string strMarcSyntax = "";

            // 获得目标数据库名
            nRet = GetTargetDbName(strTargetServerAddr,
                strOriginDbName,
                out strTargetDbName,
                out strMarcSyntax,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;   // 不在目标数据库之列

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }


            lRet = channel.DoInitialDB(strTargetDbName,
                out strError);
            if (lRet == -1)
            {
                strError = "channel.DoInitialDB() [dbname="+strTargetDbName+"] error :" + strError;
                return -1;
            }

            this.App.Statis.IncreaseEntryValue("跟踪DTLP", "初始化数据库次数", 1);

            return 0;
        }

        // 解析日志记录中的保存路径
        // return:
        //      -1  出错
        //      0   成功
        public static int ParseLogPath(string strPathParam,
            out string strDbName,
            out string strNumber,
            out string strError)
        {
            strError = "";
            strDbName = "";
            strNumber = "";

            // 检查参数
            if (String.IsNullOrEmpty(strPathParam) == true)
            {
                strError = "ParseLogPath()的strPathParam参数值不能为空";
                return -1;
            }

            string strPath = strPathParam;

            // 库名
            int nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strDbName = strPath;
                return 0;
            }

            strDbName = strPath.Substring(0, nRet);

            strPath = strPath.Substring(nRet + 1);


            string strTemp = "";

            // '记录索引号'汉字
            nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strTemp = strPath;
                return 0;
            }

            strTemp = strPath.Substring(0, nRet);

            if (strTemp != "ctlno" && strTemp != "记录索引号")
            {
                strError = "路径 '" + strPathParam + "' 格式不正确";
                return -1;
            }

            strPath = strPath.Substring(nRet + 1);

            // 号码
            nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strNumber = strPath;
                return 0;
            }

            strNumber = strPath.Substring(0, nRet);

            return 0;
        }


        // 写入或者删除记录
        // parameters:
        //      strOriginPath   原始MARC记录路径。形态为"图书编目/ctlno/0000001"。注意，没有服务器名部分。
        // return:
        //      -2  路径不正确
        //      -1  error
        //      0   发现库名不在目标数据库之列
        //      1   成功写入
        int WriteMarcRecord(
            string strOperCode,
            string strTargetServerAddr,
            string strOriginPath,
            string strMARC,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            // string strOriginServerAddr = "";
            string strOriginDbName = "";
            string strOriginNumber = "";

            // 解析保存路径
            nRet = ParseLogPath(strOriginPath,
                out strOriginDbName,
                out strOriginNumber,
                out strError);
            if (nRet == -1)
                return -2;
            if (strOriginDbName == "")
            {
                strError = "路径 '" + strOriginPath + "' 中缺乏数据库名";
                return -2;
            }
            if (strOriginNumber.Length != 7)
            {
                strError = "路径 '"+strOriginPath+"' 中原始索引号 '" +strOriginNumber+ "' 不是7位";
                return -2;
            }

            // 检查索引号是不是纯数字
            if (StringUtil.IsPureNumber(strOriginNumber) == false)
            {
                strError = "原始索引号 '" + strOriginNumber + "' 不是纯数字";
                return -2;
            }

            // 映射到目标数据库名
            string strTargetDbName = "";
            string strMarcSyntax = "";

            // 获得目标数据库名
            nRet = GetTargetDbName(strTargetServerAddr,
                strOriginDbName,
                out strTargetDbName,
                out strMarcSyntax,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;   // 不在目标数据库之列

            string strXml = "";
            if (strOperCode == "00")
            {
                // 将MARC格式转换为XML格式
                /*
                nRet = ConvertMarcToXml(
                    strMarcSyntax,
                    strMARC,
                    out strXml,
                    out strError);
                 * */
                // 2008/5/16 changed
                nRet = MarcUtil.Marc2Xml(
    strMARC,
    strMarcSyntax,
    out strXml,
    out strError);

                if (nRet == -1)
                    return -1;
            }

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            byte[] timestamp = null;
            byte[] output_timestamp = null;
            string strOutputPath = "";

            if (strOperCode == "00")
            {
                // 写记录
                lRet = channel.DoSaveTextRes(strTargetDbName + "/" + strOriginNumber,
                    strXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        // 不可能发生？
                        strError = "时间戳冲突（本不可能发生？）";
                        return -1;
                    }

                    strError = "channel.DoSaveTextRes() [path=" + strTargetDbName + "/" + strOriginNumber + "] error : " + strError;
                    return -1;
                }

                this.App.Statis.IncreaseEntryValue("跟踪DTLP", "覆盖记录条数", 1);

            }
            if (strOperCode == "02")
            {
                int nRedoCount = 0;
            REDO_DEL:
                lRet = channel.DoDeleteRes(strTargetDbName + "/" + strOriginNumber,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        // 这属于正常情况，不必在意
                        return 0;
                    }
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount >= 10)
                            return -1;
                        timestamp = output_timestamp;
                        nRedoCount++;
                        goto REDO_DEL;
                    }
                    strError = "channel.DoDeleteRes() [path=" + strTargetDbName + "/" + strOriginNumber + "] error : " + strError;
                    return -1;
                }

                this.App.Statis.IncreaseEntryValue("跟踪DTLP", "删除记录条数", 1);
            }

            return 0;
        }

#if NOOOOOOOOOOOOOOOOO
        // 从MARC记录中获得价格字符串
        // 困难是，需要知道USMARC的价格在哪个子字段，这个参数是否需要配置
        int GetTitlePrice(
            string strMarcSyntax,
            string strMARC,
            out string strPrice,
            out string strError)
        {

        }
#endif

        /*
        // 将MARC格式转换为XML格式
        int ConvertMarcToXml(
            string strMarcSyntax,
            string strMARC,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            int nRet = 0;

            MemoryStream s = new MemoryStream();

            MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            if (strMarcSyntax == "unimarc")
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else if (strMarcSyntax == "usmarc")
            {
                writer.MarcNameSpaceUri = Ns.usmarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else
            {
                strError = "strMarcSyntax值应当为unimarc和usmarc之一";
                return -1;
            }

            // string strDebug = strMARC.Replace((char)Record.FLDEND, '#');
            nRet = writer.WriteRecord(strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            writer.Flush();
            s.Flush();

            // strXml = Encoding.UTF8.GetString(s.ToArray()); // BUG!!! 这样的字符串如果用XmlDocument.LoadXml()装载，会报错

            // 2008/5/16 changed
            byte[] baContent = s.ToArray();
            strXml = ByteArray.ToString(baContent, Encoding.UTF8);

            return 0;
        }
         * */

        // 复制定义的所有源服务器的数据库到dp2这边来
        // return:
        //      -1  error
        //      0   正常结束
        //      1   被中断
        int DumpAllServerDbs(
            string strDumpStartRecordID,
            string strDumpStartOriginDbName,
            out string strBreakRecordID,
            out string strBreakOriginDbName,
            out string strError)
        {
            strError = "";
            strBreakRecordID = "";
            strBreakOriginDbName = "";

            // 获得服务器列表
            XmlNodeList originNodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("//traceDTLP/origin");

            for (int i = 0; i < originNodes.Count; i++)
            {
                XmlNode originNode = originNodes[i];
                string strOriginServerAddr = DomUtil.GetAttr(originNode, "serverAddr");

                if (String.IsNullOrEmpty(strOriginServerAddr) == true)
                    continue;

                // 不同的服务器之间有重名的数据库怎么办？目前尚未解决这个问题
                int nRet = DumpOneServerDbs(strOriginServerAddr,
                    strDumpStartRecordID,
                    strDumpStartOriginDbName,
                    out strBreakRecordID,
                    out strBreakOriginDbName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return 1;

            }


            return 0;
        }

        // 复制一个源服务器内的所有数据库
        // return:
        //      -1  error
        //      0   正常结束
        //      1   被中断
        int DumpOneServerDbs(string strOriginServerAddr,
            string strDumpStartRecordID,
            string strDumpStartOriginDbName,
            out string strBreakRecordID,
            out string strBreakOriginDbName,
            out string strError)
        {
            strError = "";
            strBreakRecordID = "";
            strBreakOriginDbName = "";


            // 获得数据库列表
            XmlNodeList itemNodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("//traceDTLP/origin[@serverAddr='"+strOriginServerAddr+"']/databaseMap/item");
            for (int i = 0; i < itemNodes.Count; i++)
            {
                XmlNode node = itemNodes[i];
                string strOriginDbName = DomUtil.GetAttr(node, "originDatabase");
                string strTargetDbName = DomUtil.GetAttr(node, "targetDatabase");
                string strMarcSyntax = DomUtil.GetAttr(node, "marcSyntax");
                string strTargetEntityDbName = DomUtil.GetAttr(node, "targetEntityDatabase");
                string strNoBiblio = DomUtil.GetAttr(node, "noBiblio");

                bool bNoBiblio = false;

                strNoBiblio = strNoBiblio.ToLower();

                if (strNoBiblio == "yes" || strNoBiblio == "true"
                    || strNoBiblio == "on")
                    bNoBiblio = true;

                if (bNoBiblio == true)
                {
                    this.AppendResultText("针对源数据库 " + strOriginDbName + "的dump操作将忽略书目信息写入目标库 " + strTargetDbName + "的操作。\r\n");
                }

                if (String.IsNullOrEmpty(strTargetEntityDbName) == false)
                {
                    this.AppendResultText("针对源数据库 " + strOriginDbName + "的dump操作将同时升级借还信息，写入实体库 " + strTargetEntityDbName + "。\r\n");
                }

                if (strMarcSyntax == "readerxml")
                {
                    this.AppendResultText("针对源数据库 " + strOriginDbName + "的dump操作将升级读者信息到dp2的XML格式，写入目标库 " + strTargetDbName + "。\r\n");
                }

                // 如果需要从指定库名开始
                if (String.IsNullOrEmpty(strDumpStartOriginDbName) == false)
                {
                    if (strOriginDbName != strDumpStartOriginDbName)
                        continue;
                }

                strDumpStartOriginDbName = "";  // 一旦找到这个库开始做，就要继续做完后面的所有库，所以需要清除这个起点参数，避免造成只做这一个库

                int nRet = DumpOneDb(strOriginServerAddr,
                    strOriginDbName,
                    strDumpStartRecordID,
                    strTargetDbName,
                    strMarcSyntax,
                    strTargetEntityDbName,
                    bNoBiblio,
                    out strBreakRecordID,
                    out strError);
                if (nRet == -1)
                {
                    strBreakOriginDbName = strOriginDbName;
                    return -1;
                }
                if (nRet == 1)
                {
                    strBreakOriginDbName = strOriginDbName;
                    return 1;
                }

                strDumpStartRecordID = "";   // 对后面的库就不起作用了
            }


            return 0;
        }

        // 复制一个数据库的全部记录
        // parameters:
        //      strTargetEntityDbName   目标实体库。当需要把MARC书目数据中的986等流通信息升级到dp2实体库时，才需要这个参数。如果==null，表示不作升级，只写入MARC书目数据
        //      bNoBiblio   是否不要写入书目库。如果等于true，表示不写入；如果等于false，表示要写入。
        // return:
        //      -1  error
        //      0   正常结束
        //      1   被中断
        int DumpOneDb(string strOriginServerAddr,
            string strOriginDbName,
            string strDumpStartRecordID,
            string strTargetDbName,
            string strMarcSyntax,
            string strTargetEntityDbName,
            bool bNoBiblio,
            out string strBreakRecordID,
            out string strError)
        {
            strError = "";
            strBreakRecordID = "";

            // 利用DumpIO类

            DtlpIO DumpRecord = new DtlpIO();

            int nRet = 0;

            if (String.IsNullOrEmpty(strDumpStartRecordID) == true)
            {
                nRet = DumpRecord.Initial(this.DtlpChannels,
                     strOriginServerAddr + "/" + strOriginDbName,
                     "0000001",
                     "9999999");
                strDumpStartRecordID = "0000001";
            }
            else
            {
                nRet = DumpRecord.Initial(this.DtlpChannels,
                     strOriginServerAddr + "/" + strOriginDbName,
                     strDumpStartRecordID,
                     "9999999");
            }

            strBreakRecordID = strDumpStartRecordID;

            // 校准转出范围的首尾号
            // return:
            //		-1	出错
            //		0	没有改变首尾号
            //		1	校准后改变了首尾号
            //		2	书目库中没有记录
            nRet = DumpRecord.VerifyRange(out strError);
            if (nRet == -1)
            {
                if (DumpRecord.ErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
                    this.DtlpChannel = null;    // 促使更换Channel

                return -1;
            }
            if (nRet == 2)
            {	
                // 书目库为空
                strError = "数据库 " +
                    strOriginDbName
                    + " 中没有记录...";
                return 0;
            }
            if (nRet == 1)
            {
                // 首尾号发生了改变
                strBreakRecordID = DumpRecord.m_strStartNumber;
            }

            int nRecordCount = -1;

            for (; ; )
            {


                if (this.Stopped == true)
                {
                    return 1;
                }

                try
                {

                    // 得到下一条记录
                    // return:
                    //		-1	出错
                    //		0	继续
                    //		1	到达末尾(超过m_strEndNumber)
                    //		2	没有找到记录
                    nRet = DumpRecord.NextRecord(ref nRecordCount,
                        out strError);
                    if (nRet == -1)
                    {
                        if (DumpRecord.ErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
                            this.DtlpChannel = null;

                        strError = "NextRecord error: " + strError;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "NextRecord exception: " + ExceptionUtil.GetDebugText(ex);
                    this.DtlpChannel = null;
                    return -1;
                }

                // 准备断点信息
                strBreakRecordID = DumpRecord.m_strCurNumber;

                if (String.IsNullOrEmpty(strBreakRecordID) == false)
                {
                    // 取得后一个号码
                    try
                    {
                        strBreakRecordID = (Convert.ToInt64(strBreakRecordID) + 1).ToString().PadLeft(strBreakRecordID.Length, '0');
                    }
                    catch
                    {
                    }
                }

                if (nRet == 1)
                    return 0;

                // 没有找到记录
                if (nRet == 2)
                {
                    // 不终止循环，试探性地读后面的记录
                    DumpRecord.m_strStartNumber = DumpRecord.m_strCurNumber;
                    /*
                    m_ValueStaticSTRINGMessage = "试探:" + pDumpRecord->m_strCurNumber;
                    UpdateData(FALSE);
                    */
                    nRecordCount = -1;
                    // 继续循环
                    continue;
                }

                string strCurRecordName = strOriginDbName + "//" + DumpRecord.m_strCurNumber;

                this.AppendResultText("处理: " + strCurRecordName + "\r\n");
                this.SetProgressText(DateTime.Now.ToString() + " 处理: " + strCurRecordName);

                string strXml = "";

                if (strMarcSyntax == "xmlreader")
                {
                    string strWarning = "";
                    // 将MARC格式转换为XML格式
                    nRet = ConvertMarcToReaderXml(
                        DumpRecord.m_strRecord,
                        /*
                        0,
                        0,
                         * */
                        out strXml,
                        out strWarning,
                        out strError);
                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        this.AppendResultText("转换警告: " + strWarning + "\r\n");

                        // 写入文本文件
                        if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                        {
                            StreamUtil.WriteText(this.m_strWarningFileName,
                                strCurRecordName + ": " + strWarning + "\r\n");
                        }
                    }
                }
                else
                {
                    Debug.Assert(strMarcSyntax == "unimarc" || strMarcSyntax == "usmarc", "");
                    // 将MARC格式转换为MARCXML格式
                    /*
                    nRet = ConvertMarcToXml(
                        strMarcSyntax,
                        DumpRecord.m_strRecord,
                        out strXml,
                        out strError);
                     * */
                    // 2008/5/16 changed
                    nRet = MarcUtil.Marc2Xml(
    DumpRecord.m_strRecord,
    strMarcSyntax,
    out strXml,
    out strError);

                }
                if (nRet == -1)
                    return -1;

                // 写入目标库

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                string strOutputPath = "";

                if (bNoBiblio == false)
                {

                    byte[] timestamp = null;
                    byte[] output_timestamp = null;

                    // 写记录
                    long lRet = channel.DoSaveTextRes(strTargetDbName + "/" + DumpRecord.m_strCurNumber,
                        strXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            // 不可能发生？
                            strError = "时间戳冲突（本不可能发生？）";
                            return -1;
                        }

                        strError = "channel.DoSaveTextRes() [path=" + strTargetDbName + "/" + DumpRecord.m_strCurNumber + "] error : " + strError;
                        return -1;
                    }
                }
                else
                {
                    strOutputPath = strTargetDbName + "/" + DumpRecord.m_strCurNumber;
                }


                if (String.IsNullOrEmpty(strTargetEntityDbName) == false)
                {
                    string strParentRecordID = ResPath.GetRecordId(strOutputPath);
                    int nThisEntityCount = 0;
                    string strWarning = "";

                    try
                    {
                        // 将一条MARC记录中包含的实体信息变成XML格式并上传
                        // parameters:
                        //      strEntityDbName 实体数据库名
                        //      strParentRecordID   父记录ID
                        //      strMARC 父记录MARC
                        nRet = DoEntityRecordsUpload(
                            channel,
                            strTargetEntityDbName,
                            strParentRecordID,
                            DumpRecord.m_strRecord,
                            /*
                            0,  // nEntityBarcodeLength,
                            0,  // nReaderBarcodeLength,
                             * */
                            out nThisEntityCount,
                            out strWarning,
                            out strError);
                    }
                    catch (Exception ex)
                    {
                        strError = strCurRecordName + " DoEntityRecordsUpload() exception: " + ExceptionUtil.GetDebugText(ex);
                        // 写入文本文件
                        if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                        {
                            StreamUtil.WriteText(this.m_strWarningFileName,
                                strCurRecordName + strError + "\r\n");
                        }
                        return -1;
                    }

                    if (nRet == -1)
                    {
                        strError = "转换实体记录时 [书目记录path=" + strTargetDbName + "/" + DumpRecord.m_strCurNumber + "] 发生错误 : " + strError;
                        return -1;
                    }
                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        this.AppendResultText("转换警告: " + strWarning + "\r\n");

                        // 写入文本文件
                        if (String.IsNullOrEmpty(this.m_strWarningFileName) == false)
                        {
                            StreamUtil.WriteText(this.m_strWarningFileName,
                                strCurRecordName + ": " + strWarning + "\r\n");
                        }
                    }

                    // 记忆断点
                    if ((nRecordCount % 100) == 0)
                    {
                        // 每隔100条记忆一次断点信息
                        // 2006/12/20
                        this.StartInfo.Start = MemoBreakPoint(
                            this.m_strStartFileName,
                            this.m_nStartIndex,
                            this.m_strStartOffset,
                            strBreakRecordID,
                            strOriginDbName
                            );
                    }
                }

            }

            // return 0;   // 正常结束
        }

        int ClearAllServerDbs(
            out string strError)
        {
            strError = "";

            // 获得服务器列表
            XmlNodeList originNodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("//traceDTLP/origin");

            for (int i = 0; i < originNodes.Count; i++)
            {
                XmlNode originNode = originNodes[i];
                string strOriginServerAddr = DomUtil.GetAttr(originNode, "serverAddr");

                if (String.IsNullOrEmpty(strOriginServerAddr) == true)
                    continue;

                int nRet = ClearOneServerDbs(strOriginServerAddr,
                    out strError);
                if (nRet == -1)
                    return -1;
            }


            return 0;
        }

        // 初始化一个服务器内的所有跟踪目标(而不是源)数据库
        int ClearOneServerDbs(string strOriginServerAddr,
            out string strError)
        {
            strError = "";

            // 获得数据库列表
            XmlNodeList itemNodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("//traceDTLP/origin[@serverAddr='" + strOriginServerAddr + "']/databaseMap/item");
            for (int i = 0; i < itemNodes.Count; i++)
            {
                XmlNode node = itemNodes[i];
                // string strOriginDbName = DomUtil.GetAttr(node, "originDatabase");
                string strTargetDbName = DomUtil.GetAttr(node, "targetDatabase");
                // string strMarcSyntax = DomUtil.GetAttr(node, "marcSyntax");
                string strTargetEntityDbName = DomUtil.GetAttr(node, "targetEntityDatabase");
                string strNoBiblio = DomUtil.GetAttr(node, "noBiblio");

                bool bNoBiblio = false;

                strNoBiblio = strNoBiblio.ToLower();

                if (strNoBiblio == "yes" || strNoBiblio == "true"
                    || strNoBiblio == "on")
                    bNoBiblio = true;

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                long lRet = 0;
                if (bNoBiblio == false)
                {
                    lRet = channel.DoInitialDB(strTargetDbName,
                        out strError);
                    if (lRet == -1)
                        return -1;
                    this.AppendResultText("初始化库 '" + strTargetDbName + "'。\r\n");
                }
                else
                {
                    // noBiblio参数是用来控制不要初始化书目库，单作实体库的
                    this.AppendResultText("*没有*初始化书目库 '" + strTargetDbName + "'。\r\n");
                }

                // 连带初始化实体库
                if (String.IsNullOrEmpty(strTargetEntityDbName) == false)
                {
                    lRet = channel.DoInitialDB(strTargetEntityDbName,
                        out strError);
                    if (lRet == -1)
                        return -1;
                    this.AppendResultText("初始化实体库 '" + strTargetEntityDbName + "'。\r\n");
                }
            }

            return 0;
        }

        #region 升级dt1000/dt1500读者数据到dp2相关功能

        // 将dt1000/dt1500读者MARC格式转换为dp2的读者XML格式
        /*
        int ConvertMarcToReaderXml(
            string strMARC,
            out string strXml,
            out string strError)
        {
            strError = "";

            return 0;
        }*/

        // return:
        //      -1  error
        //      0   OK
        //      1   Invalid
        int VerifyBarcode(
            bool bReader,
            string strBarcode,
            out string strError)
        {
            strError = "";

            int nResultValue = -1;
            // 执行脚本函数VerifyBarcode
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            int nRet = this.App.DoVerifyBarcodeScriptFunction(
                strBarcode,
                out nResultValue,
                out strError);
            if (nRet == -2)
                return 0;
            if (nRet == -1)
                return -1;
            if (nResultValue == 0)
            {
                return 1;
            }
            if (nRet == 1)
            {
                if (bReader == false)
                {
                    strError = "看起来是读者证条码号。";
                    return 1;
                }
            }

            if (nRet == 2)
            {
                if (bReader == true)
                {
                    strError = "看起来是册条码号。";
                    return 1;
                }
            }

            return 0;
        }

        // 把MARC读者记录转换为XML格式
        // parameters:
        //      nReaderBarcodeLength    读者证条码号长度。如果==0，表示不校验长度

        public int ConvertMarcToReaderXml(
            string strMARC,
            /*
            int nReaderBarcodeLength,
            int nEntityBarcodeLength,
             * */
            out string strXml,
            out string strWarning,
            out string strError)
        {
            strXml = "";
            strError = "";
            strWarning = "";

            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 读者证条码号
            string strBarcode = "";

            // 以字段/子字段名从记录中得到第一个子字段内容。
            // parameters:
            //		strMARC	机内格式MARC记录
            //		strFieldName	字段名。内容为字符
            //		strSubfieldName	子字段名。内容为1字符
            // return:
            //		""	空字符串。表示没有找到指定的字段或子字段。
            //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
            strBarcode = MarcUtil.GetFirstSubfield(strMARC,
                "100",
                "a");

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strWarning += "MARC记录中缺乏100$a读者证条码号; ";
            }
            else
            {
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    true,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "100$中的读者证条码号 '" + strBarcode + "' 不合法 -- "+strError+"; ";
                }
            }

            DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);


            // 密码
            string strPassword = "";
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
    "080",
    "a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                try
                {
                    strPassword = Cryptography.GetSHA1(strPassword);
                }
                catch
                {
                    strError = "将密码明文转换为SHA1时发生错误";
                    return -1;
                }

                DomUtil.SetElementText(dom.DocumentElement, "password", strPassword);
            }

            // 读者类型
            string strReaderType = "";
            strReaderType = MarcUtil.GetFirstSubfield(strMARC,
    "110",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "readerType", strReaderType);

            /*
            // 发证日期
            DomUtil.SetElementText(dom.DocumentElement, "createDate", strCreateDate);
             * */

            // 失效期
            string strExpireDate = "";
            strExpireDate = MarcUtil.GetFirstSubfield(strMARC,
    "110",
    "d");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                if (strExpireDate.Length != 8)
                {
                    strWarning += "110$d中的失效期  '" + strExpireDate + "' 应为8字符; ";
                }


                string strTarget = "";
                nRet = DateTimeUtil.Date8toRfc1123(strExpireDate,
                    out strTarget,
                    out strError);
                if (nRet == -1)
                {
                    strWarning += "MARC数据中110$d日期字符串转换格式为rfc1123时发生错误: " + strError;
                    strExpireDate = "";
                }
                else
                {
                    strExpireDate = strTarget;
                }

                DomUtil.SetElementText(dom.DocumentElement, "expireDate", strExpireDate);
            }

            // 停借原因
            string strState = "";
            strState = MarcUtil.GetFirstSubfield(strMARC,
    "982",
    "b");
            if (String.IsNullOrEmpty(strState) == false)
            {

                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            // 姓名
            string strName = "";
            strName = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "a");
            if (String.IsNullOrEmpty(strName) == true)
            {
                strWarning += "MARC记录中缺乏200$a读者姓名; ";
            }

            DomUtil.SetElementText(dom.DocumentElement, "name", strName);


            // 姓名拼音
            string strNamePinyin = "";
            strNamePinyin = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "A");
            if (String.IsNullOrEmpty(strNamePinyin) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "namePinyin", strNamePinyin);
            }

            // 性别
            string strGender = "";
            strGender = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "gender", strGender);

            /*
            // 生日
            string strBirthday = "";
            strBirthday = MarcUtil.GetFirstSubfield(strMARC,
    "???",
    "?");

            DomUtil.SetElementText(dom.DocumentElement, "birthday", strBirthday);
             * */

            // 身份证号

            // 单位
            string strDepartment = "";
            strDepartment = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "department", strDepartment);

            // 地址
            string strAddress = "";
            strAddress = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "address", strAddress);

            // 邮政编码
            string strZipCode = "";
            strZipCode = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "zipcode", strZipCode);

            // 电话
            string strTel = "";
            strTel = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "tel", strTel);

            // email

            // 所借阅的各册
            string strField986 = "";
            string strNextFieldName = "";
            // 从记录中得到一个字段
            // parameters:
            //		strMARC		机内格式MARC记录
            //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
            //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
            //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
            //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
            //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = MarcUtil.GetField(strMARC,
    "986",
    0,
    out strField986,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得986字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeBorrows = dom.CreateElement("borrows");
                nodeBorrows = dom.DocumentElement.AppendChild(nodeBorrows);

                string strWarningParam = "";
                nRet = CreateBorrowsNode(nodeBorrows,
                    strField986,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据986字段内容创建<borrows>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField988 = "";
            // 违约金记录
            nRet = MarcUtil.GetField(strMARC,
    "988",
    0,
    out strField988,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得988字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeOverdues = dom.CreateElement("overdues");
                nodeOverdues = dom.DocumentElement.AppendChild(nodeOverdues);

                string strWarningParam = "";
                nRet = CreateOverduesNode(nodeOverdues,
                    strField988,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据988字段内容创建<overdues>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField984 = "";
            // 预约信息
            nRet = MarcUtil.GetField(strMARC,
    "984",
    0,
    out strField984,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得984字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeReservations = dom.CreateElement("reservations");
                nodeReservations = dom.DocumentElement.AppendChild(nodeReservations);

                string strWarningParam = "";
                nRet = CreateReservationsNode(nodeReservations,
                    strField984,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据984字段内容创建<reservations>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }

            // 遮盖MARC记录中的808$a内容
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
"080",
"a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                MarcUtil.ReplaceField(ref strMARC,
                    "080",
                    0,
                    "080  " + new String(MarcUtil.SUBFLD, 1) + "a********");
            }

            // 保留原始记录供参考
            string strPlainText = strMARC.Replace(MarcUtil.SUBFLD, '$');
            strPlainText = strPlainText.Replace(new String(MarcUtil.FLDEND, 1), "#\r\n");
            if (strPlainText.Length > 24)
                strPlainText = strPlainText.Insert(24, "\r\n");

            DomUtil.SetElementText(dom.DocumentElement, "originMARC", strPlainText);

            strXml = dom.OuterXml;

            return 0;
        }

        /*
        public static int Date8toRfc1123(string strOrigin,
out string strTarget,
out string strError)
        {
            strError = "";
            strTarget = "";

            // strOrigin = strOrigin.Replace("-", "");

            // 格式为 20060625， 需要转换为rfc
            if (strOrigin.Length != 8)
            {
                strError = "源日期字符串 '" + strOrigin + "' 格式不正确，应为8字符";
                return -1;
            }


            IFormatProvider culture = new CultureInfo("zh-CN", true);

            DateTime time;
            try
            {
                time = DateTime.ParseExact(strOrigin, "yyyyMMdd", culture);
            }
            catch
            {
                strError = "日期字符串 '" + strOrigin + "' 字符串转换为DateTime对象时出错";
                return -1;
            }

            time = time.ToUniversalTime();
            strTarget = DateTimeUtil.Rfc1123DateTimeString(time);


            return 0;
        }
         * */

        // 创建<borrows>节点的下级内容
        int CreateBorrowsNode(XmlNode nodeBorrows,
            string strField986,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField986,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "986字段中 册条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }

                XmlNode nodeBorrow = nodeBorrows.OwnerDocument.CreateElement("borrow");
                nodeBorrow = nodeBorrows.AppendChild(nodeBorrow);

                DomUtil.SetAttr(nodeBorrow, "barcode", strBarcode);

                // borrowDate属性
                // 第一次借书日期
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "986$t子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "borrowDate", strBorrowDate);
                }

                // no属性
                // 从什么数字开始计数？
                string strNo = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "y",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strNo = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strNo) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "no", strNo);
                }




                // borrowPeriod属性

                // 根据应还日期计算出来?

                // 应还日期
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "v",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);

                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "986$v子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }
                else
                {
                    if (strBorrowDate != "")
                    {
                        strWarning += "986字段中子字段组 " + Convert.ToString(g + 1) + " 有 $t 子字段内容而没有 $v 子字段内容, 不正常; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$v日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strReturnDate) == false
                    && String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    // 计算差额天数
                    DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
                    DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                    TimeSpan delta = timeend - timestart;

                    string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                    DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                }

                // 续借的日期
                if (strNo != "")
                {
                    string strRenewDate = "";
                    nRet = MarcUtil.GetSubfield(strGroup,
                        ItemType.Group,
                        "x",
                        0,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (strSubfield.Length >= 1)
                    {
                        strRenewDate = strSubfield.Substring(1);

                        if (strRenewDate.Length != 8)
                        {
                            strWarning += "986$x子字段内容 '" + strRenewDate + "' 的长度不是8字符; ";
                        }

                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        string strTarget = "";
                        nRet = DateTimeUtil.Date8toRfc1123(strRenewDate,
                            out strTarget,
                            out strError);
                        if (nRet == -1)
                        {
                            strWarning += "986字段中$x日期字符串转换格式为rfc1123时发生错误: " + strError;
                            strRenewDate = "";
                        }
                        else
                        {
                            strRenewDate = strTarget;
                        }

                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        DomUtil.SetAttr(nodeBorrow, "borrowDate", strRenewDate);
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false
    && String.IsNullOrEmpty(strBorrowDate) == false)
                    {
                        // 重新计算差额天数
                        DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strRenewDate);
                        DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                        TimeSpan delta = timeend - timestart;

                        string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                        DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                    }

                }


            }

            return 0;
        }


        // 创建<overdues>节点的下级内容
        int CreateOverduesNode(XmlNode nodeOverdues,
            string strField988,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField988,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;


                if (nRet != 0)
                {
                    strWarning += "988字段中 册条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }

                string strCompleteDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "d",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strCompleteDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strCompleteDate) == false)
                    continue; // 如果已经交了罚金，这个子字段组就忽略了

                XmlNode nodeOverdue = nodeOverdues.OwnerDocument.CreateElement("overdue");
                nodeOverdue = nodeOverdues.AppendChild(nodeOverdue);

                DomUtil.SetAttr(nodeOverdue, "barcode", strBarcode);

                // borrowDate属性
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "e",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "988$e子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$e日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "borrowDate", strBorrowDate);
                }

                // returnDate属性
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);

                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "988$t子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "returnDate", strReturnDate);  // 2006/12/29 changed
                }

                // borrowPeriod未知
                //   DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strBorrowPeriod);

                // price和type属性是为兼容dt1000数据而设立的属性
                // 而over超期天数属性就空缺了

                // price属性
                string strPrice = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strPrice = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    // 是否需要转换为带货币单位的, 带小数部分的字符串?

                    DomUtil.SetAttr(nodeOverdue, "price", strPrice);
                }

                // type属性
                string strType = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strType = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strType) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "type", strType);
                }

                // 2007/9/27
                DomUtil.SetAttr(nodeOverdue, "id", "upgrade-" + this.App.GetOverdueID());   // 2008/2/8 "upgrade-"
            }

            return 0;
        }


        // 创建<reservations>节点的下级内容
        // 待做内容：
        // 1)如果实体库已经存在，这里需要增加相关操作实体库的代码。
        // 也可以专门用一个读者记录和实体记录对照修改的阶段，来处理相互的关系
        // 2)暂时没有处理已到的预约册的信息升级功能，而是丢弃了这些信息
        int CreateReservationsNode(XmlNode nodeReservations,
            string strField984,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField984,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "984字段中 册条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }

                string strArriveDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strArriveDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strArriveDate) == false)
                    continue; // 如果已经到书，这个子字段组就忽略了

                XmlNode nodeRequest = nodeReservations.OwnerDocument.CreateElement("request");
                nodeRequest = nodeReservations.AppendChild(nodeRequest);

                DomUtil.SetAttr(nodeRequest, "items", strBarcode);

                // requestDate属性
                string strRequestDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strRequestDate = strSubfield.Substring(1);

                    if (strRequestDate.Length != 8)
                    {
                        strWarning += "984$b子字段内容 '" + strRequestDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strRequestDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "984字段中$b日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strRequestDate = "";
                    }
                    else
                    {
                        strRequestDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    DomUtil.SetAttr(nodeRequest, "requestDate", strRequestDate);
                }

            }

            return 0;
        }

        #endregion


        #region 升级dt1000/dt1500书目数据到dp2实体库相关功能

        // 将一条MARC记录中包含的实体信息变成XML格式并上传
        // parameters:
        //      strEntityDbName 实体数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int DoEntityRecordsUpload(
            RmsChannel channel,
            string strEntityDbName,
            string strParentRecordID,
            string strMARC,
            /*
            int nEntityBarcodeLength,
            int nReaderBarcodeLength,
             * */
            out int nThisEntityCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            nThisEntityCount = 0;

            int nRet = 0;

            string strField906 = "";
            string strField986 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] {'0'});
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";


            // 获得906字段

            nRet = MarcUtil.GetField(strMARC,
                "906",
                0,
                out strField906,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得906字段时出错";
                return -1;
            }
            if (nRet == 0)
                strField906 = "";

            // 获得986字段



            // 从记录中得到一个字段
            // parameters:
            //		strMARC		机内格式MARC记录
            //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
            //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
            //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
            //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
            //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = MarcUtil.GetField(strMARC,
                "986",
                0,
                out strField986,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得986字段时出错";
                return -1;
            }

            if (nRet == 0)
            {
                // return 0;   // 没有找到986字段
                strField986 = "";
            }
            else
            {


                // 修正986字段内容
                if (strField986.Length <= 5 + 2)
                    strField986 = "";
                else
                {
                    string strPart = strField986.Substring(5, 2);

                    string strDollarA = new string(MarcUtil.SUBFLD, 1) + "a";

                    if (strPart != strDollarA)
                    {
                        strField986 = strField986.Insert(5, strDollarA);
                    }
                }

            }

            List<Group> groups = null;

            // 合并906和986字段内容
            nRet = MergField906and986(strField906,
            strField986,
            out groups,
            out strWarningParam,
            out strError);
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += strWarningParam + "; ";

            // 进行子字段组循环
            for (int g = 0; g < groups.Count; g++)
            {
                Group group = groups[g];

                string strGroup = group.strValue;

                // 处理一个item

                string strXml = "";

                // 构造实体XML记录
                // parameters:
                //      strParentID 父记录ID
                //      strGroup    待转换的图书种记录的986字段中某子字段组片断
                //      strXml      输出的实体XML记录
                // return:
                //      -1  出错
                //      0   成功
                nRet = BuildEntityXmlRecord(strParentRecordID,
                    strGroup,
                    strMARC,
                    group.strMergeComment,
                    // nReaderBarcodeLength,
                    out strXml,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建记录id " + strParentRecordID + " 之实体(序号) " + Convert.ToString(g + 1) + "时发生错误: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";
                string strTargetPath = strEntityDbName + "/?";

                // RmsChannel channel = this.MainForm.Channels.GetChannel(strServerUrl);

                // 保存Xml记录
                long lRet = channel.DoSaveTextRes(strTargetPath,
                    strXml,
                    false,	// bIncludePreamble
                    "",//strStyle,
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);

                if (lRet == -1)
                {
                    return -1;
                }

                nThisEntityCount++;

            }

            return 0;
        }

        // 构造实体XML记录
        // parameters:
        //      strParentID 父记录ID
        //      strGroup    待转换的图书种记录的986字段中某子字段组片断
        //      strXml      输出的实体XML记录
        // return:
        //      -1  出错
        //      0   成功
        int BuildEntityXmlRecord(string strParentID,
            string strGroup,
            string strMARC,
            string strMergeComment,
            // int nReaderBarcodeLength,
            out string strXml,
            out string strWarning,
            out string strError)
        {
            strXml = "";
            strWarning = "";
            strError = "";

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 父记录id
            DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

            // 册条码号

            string strSubfield = "";
            string strNextSubfieldName = "";
            // 从字段或子字段组中得到一个子字段
            // parameters:
            //		strText		字段内容，或者子字段组内容。
            //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
            //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
            //					形式为'a'这样的。
            //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
            //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
            //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中
            int nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "a",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strBarcode = strSubfield.Substring(1);

                DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);
            }


            // 登录号
            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "h",
    0,
    out strSubfield,
    out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strRegisterNo = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strRegisterNo) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "registerNo", strRegisterNo);
                }
            }



            // 状态?
            DomUtil.SetElementText(dom.DocumentElement, "state", "");

            // 馆藏地点

            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "b",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strLocation = strSubfield.Substring(1);

                DomUtil.SetElementText(dom.DocumentElement, "location", strLocation);
            }

            // 价格
            // 先找子字段组中的$d 找不到才找982$b

            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "d",
    0,
    out strSubfield,
    out strNextSubfieldName);
            string strPrice = "";

            if (strSubfield.Length >= 1)
            {
                strPrice = strSubfield.Substring(1);
            }

            // 如果从$d中获得的价格内容为空，则从982$b中获得
            if (String.IsNullOrEmpty(strPrice) == true)
            {
                // 以字段/子字段名从记录中得到第一个子字段内容。
                // parameters:
                //		strMARC	机内格式MARC记录
                //		strFieldName	字段名。内容为字符
                //		strSubfieldName	子字段名。内容为1字符
                // return:
                //		""	空字符串。表示没有找到指定的字段或子字段。
                //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
                strPrice = MarcUtil.GetFirstSubfield(strMARC,
                    "982",
                    "b");
            }

            DomUtil.SetElementText(dom.DocumentElement, "price", strPrice);

            // 图书册类型
            // 先找这里的$f 如果没有，再找982$a?
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"f",
0,
out strSubfield,
out strNextSubfieldName);
            string strBookType = "";
            if (strSubfield.Length >= 1)
            {
                strBookType = strSubfield.Substring(1);
            }

            // 如果从$f中获得的册类型为空，则从982$a中获得
            if (String.IsNullOrEmpty(strBookType) == true)
            {
                // 以字段/子字段名从记录中得到第一个子字段内容。
                // parameters:
                //		strMARC	机内格式MARC记录
                //		strFieldName	字段名。内容为字符
                //		strSubfieldName	子字段名。内容为1字符
                // return:
                //		""	空字符串。表示没有找到指定的字段或子字段。
                //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
                strBookType = MarcUtil.GetFirstSubfield(strMARC,
                    "982",
                    "a");
            }

            DomUtil.SetElementText(dom.DocumentElement, "bookType", strBookType);

            // 注释
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"z",
0,
out strSubfield,
out strNextSubfieldName);
            string strComment = "";
            if (strSubfield.Length >= 1)
            {
                strComment = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "comment", strComment);
            }

            // 借阅者
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"r",
0,
out strSubfield,
out strNextSubfieldName);
            string strBorrower = "";
            if (strSubfield.Length >= 1)
            {
                strBorrower = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strBorrower) == false)
            {
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    true,
                    strBorrower,
                    out strError);
                if (nRet == -1)
                    return -1;


                // 检查条码号长度
                if (nRet != 0)
                {
                    strWarning += "$r中读者证条码号 '" + strBorrower + "' 不合法 -- " + strError + "; ";
                }

                DomUtil.SetElementText(dom.DocumentElement, "borrower", strBorrower);
            }

            // 借阅日期
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"t",
0,
out strSubfield,
out strNextSubfieldName);
            string strBorrowDate = "";
            if (strSubfield.Length >= 1)
            {
                strBorrowDate = strSubfield.Substring(1);

                // 格式为 20060625， 需要转换为rfc
                if (strBorrowDate.Length == 8)
                {
                    /*
                    IFormatProvider culture = new CultureInfo("zh-CN", true);

                    DateTime time;
                    try
                    {
                        time = DateTime.ParseExact(strBorrowDate, "yyyyMMdd", culture);
                    }
                    catch
                    {
                        strError = "子字段组中$t内容中的借阅日期 '" + strBorrowDate + "' 字符串转换为DateTime对象时出错";
                        return -1;
                    }

                    time = time.ToUniversalTime();
                    strBorrowDate = DateTimeUtil.Rfc1123DateTimeString(time);
                     * */

                    string strTarget = "";

                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                    out strTarget,
                    out strError);
                    if (nRet == -1)
                    {
                        strWarning += "子字段组中$t内容中的借阅日期 '" + strBorrowDate + "' 格式出错: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }
                else if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    strWarning += "$t中日期值 '" + strBorrowDate + "' 格式错误，长度应为8字符 ";
                    strBorrowDate = "";
                }
            }

            if (String.IsNullOrEmpty(strBorrowDate) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "borrowDate", strBorrowDate);
            }

            // 借阅期限
            if (String.IsNullOrEmpty(strBorrowDate) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "borrowPeriod", "1day"); // 象征性地为1天。因为<borrowDate>中的值实际为应还日期
            }

            // 增补注释
            if (String.IsNullOrEmpty(strMergeComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "mergeComment", strMergeComment);
            }

            // 状态
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "s",
                0,
                out strSubfield,
                out strNextSubfieldName);
            string strState = "";
            if (strSubfield.Length >= 1)
            {
                strState = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strState) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            strXml = dom.OuterXml;

            return 0;
        }

        // 针对一个子字段组的描述
        public class Group
        {
            public string strBarcode = "";
            public string strRegisterNo = "";
            public string strValue = "";
            public string strMergeComment = ""; // 合并过程细节注释

            // 从另一Group对象中合并必要的子字段值过来
            // 2008/4/14
            public void MergeValue(Group group)
            {
                int nRet = 0;
                string strSubfieldNames = "b";  // 若干个需要合并的子字段名

                for (int i = 0; i < strSubfieldNames.Length; i++)
                {
                    char subfieldname = strSubfieldNames[i];

                    string strSubfieldName = new string (subfieldname, 1);

                    string strSubfield = "";
                    string strNextSubfieldName = "";

                    string strValue = "";

                    // 从字段或子字段组中得到一个子字段
                    // parameters:
                    //		strText		字段内容，或者子字段组内容。
                    //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                    //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                    //					形式为'a'这样的。
                    //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                    //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                    //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = MarcUtil.GetSubfield(this.strValue,
                        ItemType.Group,
                        strSubfieldName,
                        0,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (strSubfield.Length >= 1)
                    {
                        strValue = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                    }

                    // 如果为空，才需要看看增补
                    if (String.IsNullOrEmpty(strValue) == true)
                    {
                        string strOtherValue = "";

                        strSubfield = "";
                        nRet = MarcUtil.GetSubfield(group.strValue,
                            ItemType.Group,
                            strSubfieldName,
                            0,
                            out strSubfield,
                            out strNextSubfieldName);
                        if (strSubfield.Length >= 1)
                        {
                            strOtherValue = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                        }

                        if (String.IsNullOrEmpty(strOtherValue) == false)
                        {
                            // 替换字段中的子字段。
                            // parameters:
                            //		strField	[in,out]待替换的字段
                            //		strSubfieldName	要替换的子字段的名，内容为1字符。如果==null，表示任意子字段
                            //					形式为'a'这样的。
                            //		nIndex		要替换的子字段所在序号。如果为-1，将始终为在字段中追加新子字段内容。
                            //		strSubfield	要替换成的新子字段。注意，其中第一字符为子字段名，后面为子字段内容
                            // return:
                            //		-1	出错
                            //		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
                            //		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
                            nRet = MarcUtil.ReplaceSubfield(ref this.strValue,
                                strSubfieldName,
                                0,
                                strSubfieldName + strOtherValue);
                        }
                    }
                }


            }
        }

        // 根据一个MARC字段，创建Group数组
        public int BuildGroups(string strField,
            // int nEntityBarcodeLength,
            out List<Group> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            groups = new List<Group>();
            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                // 册条码号

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";
                string strRegisterNo = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                }

                if (String.IsNullOrEmpty(strBarcode) == false)
                {
                    // 去掉左边的'*'号 2006/9/2 add
                    if (strBarcode[0] == '*')
                        strBarcode = strBarcode.Substring(1);

                    // return:
                    //      -1  error
                    //      0   OK
                    //      1   Invalid
                    nRet = VerifyBarcode(
                        false,
                        strBarcode,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 检查册条码号长度
                    if (nRet != 0)
                    {
                        strWarning += "册条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                    }
                }


                // 登录号
                nRet = MarcUtil.GetSubfield(strGroup,
        ItemType.Group,
        "h",
        0,
        out strSubfield,
        out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strRegisterNo = strSubfield.Substring(1);
                }

                // TODO: 需要加入检查登录号长度的代码


                Group group = new Group();
                group.strValue = strGroup;
                group.strBarcode = strBarcode;
                group.strRegisterNo = strRegisterNo;

                groups.Add(group);
            }

            return 0;
        }
        // 合并906和986字段内容
        int MergField906and986(string strField906,
            string strField986,
            // int nEntityBarcodeLength,
            out List<Group> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            groups = null;
            strError = "";
            strWarning = "";

            int nRet = 0;

            List<Group> groups_906 = null;
            List<Group> groups_986 = null;

            string strWarningParam = "";

            nRet = BuildGroups(strField906,
                // nEntityBarcodeLength,
                out groups_906,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将906字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "906字段 " + strWarningParam + "; ";

            nRet = BuildGroups(strField986,
                // nEntityBarcodeLength,
                out groups_986,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将986字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "986字段 " + strWarningParam + "; ";


            List<Group> new_groups = new List<Group>(); // 新增部分

            for (int i = 0; i < groups_906.Count; i++)
            {
                Group group906 = groups_906[i];

                bool bFound = false;
                for (int j = 0; j < groups_986.Count; j++)
                {
                    Group group986 = groups_986[j];

                    if (group906.strBarcode != "")
                    {
                        if (group906.strBarcode == group986.strBarcode)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group906);

                            break;
                        }
                    }
                    else if (group906.strRegisterNo != "")
                    {
                        if (group906.strRegisterNo == group986.strRegisterNo)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group906);

                            break;
                        }
                    }
                }

                if (bFound == true)
                {
                    continue;
                }

                group906.strMergeComment = "从906字段中增补过来";
                new_groups.Add(group906);
            }

            groups = new List<Group>(); // 结果数组
            groups.AddRange(groups_986);    // 先加入986内的所有事项

            if (new_groups.Count > 0)
                groups.AddRange(new_groups);    // 然后加入新增事项


            return 0;
        }


        #endregion
    }
#endif
}
