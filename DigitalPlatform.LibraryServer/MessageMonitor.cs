using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Web;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

namespace DigitalPlatform.LibraryServer
{
    public class MessageMonitor : BatchTask
    {
        public MessageMonitor(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "消息监控";
            }
        }

        // 构造断点字符串
        static string MakeBreakPointString(
            string strRecordID)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // <loop>
            XmlNode nodeLoop = dom.CreateElement("loop");
            dom.DocumentElement.AppendChild(nodeLoop);

            DomUtil.SetAttr(nodeLoop, "recordid", strRecordID);

            return dom.OuterXml;
        }

        // 解析 开始 参数
        // parameters:
        //      strStart    启动字符串。格式一般为index.offsetstring@logfilename
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParseMessageMonitorStart(string strStart,
            out string strRecordID,
            out string strError)
        {
            strError = "";
            strRecordID = "";

            int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                // strError = "启动参数不能为空";
                // return -1;
                strRecordID = "1";
                return 0;
            }

            if (strStart == "!breakpoint")
            {
                // 从断点记忆文件中读出信息
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = this.App.ReadBatchTaskBreakPointFile(
                    this.DefaultName,
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
                    strError = "当前服务器没有发现 "+this.DefaultName+" 断点信息，无法启动任务";
                    return -1;
                }

                Debug.Assert(nRet == 1, "");
                this.AppendResultText("服务器记忆的 "+this.DefaultName+" 上次断点字符串为: "
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

            XmlNode nodeLoop = dom.DocumentElement.SelectSingleNode("loop");
            if (nodeLoop != null)
            {
                strRecordID = DomUtil.GetAttr(nodeLoop, "recordid");
            }

            return 0;
        }

        public static string MakeMessageMonitorParam(
    bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }


        // 解析通用启动参数
        // 格式
        /*
         * <root loop='...'/>
         * loop缺省为true
         * 
         * */
        public static int ParseMessageMonitorParam(string strParam,
            out bool bLoop,
            out string strError)
        {
            strError = "";
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
            // 2007/12/18 new add
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;
            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            bool bFirst = true;
            string strError = "";
            int nRet = 0;

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 通用启动参数
            bool bLoop = true;
            nRet = ParseMessageMonitorParam(startinfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            string strID = "";
            nRet = ParseMessageMonitorStart(startinfo.Start,
                out strID,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                this.Loop = false;
                return;
            }

            // 
            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "messageMonitor";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strMonitorName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从文件中获取 " + strMonitorName + " 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                string strStartTimeDef = "";
                //      bRet    是否到了每日启动时间
                bool bRet = false;
                string strOldLastTime = strLastTime;

                // return:
                //      -1  error
                //      0   没有找到startTime配置参数
                //      1   找到了startTime配置参数
                nRet = IsNowAfterPerDayStart(
                    strMonitorName,
                    ref strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "获取 " + strMonitorName + " 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                // 如果nRet == 0，表示没有配置相关参数，则兼容原来的习惯，每次都作
                if (nRet == 0)
                {

                }
                else if (nRet == 1)
                {
                    if (bRet == false)
                    {
                        if (this.ManualStart == true)
                            this.AppendResultText("已试探启动任务 '" + this.Name + "'，但因没有到每日启动时间 " + strStartTimeDef + " 而未能启动。(上次任务处理结束时间为 " + DateTimeUtil.LocalTime(strLastTime) + ")\r\n");

                        // 2014/3/31
                        if (string.IsNullOrEmpty(strOldLastTime) == true
                            && string.IsNullOrEmpty(strLastTime) == false)
                        {
                            this.AppendResultText("史上首次启动此任务。已把当前时间当作上次任务处理结束时间 " + DateTimeUtil.LocalTime(strLastTime) + " 写入了断点记忆文件\r\n");
                            WriteLastTime(strMonitorName, strLastTime);
                        }

                        return; // 还没有到每日时间
                    }

                    bPerDayStart = true;
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + " 启动。");
            }

            AppendResultText("开始新一轮循环");

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            string strMessageDbName = this.App.MessageDbName;

            if (String.IsNullOrEmpty(strMessageDbName) == true)
            {
                AppendResultText("尚未配置消息库名(<message dbname='...' />)");
                this.Loop = false;
                return;
            }

            if (String.IsNullOrEmpty(this.App.MessageReserveTimeSpan) == true)
            {
                AppendResultText("尚未配置消息保留期限(<message reserveTimeSpan='...' />");
                this.Loop = false;
                return;
            }

            // 解析期限值
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            nRet = LibraryApplication.ParsePeriodUnit(
                this.App.MessageReserveTimeSpan,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "消息保留期限 值 '" + this.App.MessageReserveTimeSpan + "' 格式错误: " + strError;
                AppendResultText(strError);
                this.Loop = false;
                return;
            }

            AppendResultText("开始处理消息库 " + strMessageDbName + " 的循环");

            // string strID = "1";
            int nRecCount = 0;
            for (; ; nRecCount++)
            {
                // 系统挂起的时候，不运行本线程
                // 2008/2/4
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
                // 2012/2/4
                if (this.App.PauseBatchTask == true)
                    break;

                if (this.Stopped == true)
                    break;

                string strStyle = "";
                strStyle = "data,content,timestamp,outputpath";

                if (bFirst == true)
                    strStyle += "";
                else
                {
                    strStyle += ",next";
                }

                string strPath = strMessageDbName + "/" + strID;

                string strXmlBody = "";
                string strMetaData = "";
                string strOutputPath = "";
                byte[] baOutputTimeStamp = null;

                // 
                SetProgressText((nRecCount + 1).ToString() + " " + strPath);

                // 获得资源
                // return:
                //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                //		0	成功
                long lRet = channel.GetRes(strPath,
                    strStyle,
                    out strXmlBody,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        if (bFirst == true)
                        {
                            // 第一条没有找到, 但是要强制循环进行
                            bFirst = false;
                            goto CONTINUE;
                        }
                        else
                        {
                            if (bFirst == true)
                            {
                                strError = "数据库 " + strMessageDbName + " 记录 " + strID + " 不存在。处理结束。";

                            }
                            else
                            {
                                strError = "数据库 " + strMessageDbName + " 记录 " + strID + " 是最末一条记录。处理结束。";
                            }
                            break;
                        }

                    }
                    else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                    {
                        bFirst = false;
                        // 把id解析出来
                        strID = ResPath.GetRecordId(strOutputPath);
                        goto CONTINUE;

                    }

                    goto ERROR1;
                }

                bFirst = false;

                // 把id解析出来
                strID = ResPath.GetRecordId(strOutputPath);

                try
                {
                    // 处理
                    nRet = DoOneRecord(
                        lPeriodValue,
                        strPeriodUnit,
                        strOutputPath,
                        strXmlBody,
                        baOutputTimeStamp,
                        out strError);
                }
                catch (Exception ex)
                {
                    strError = "DoOneRecord exception: " + ExceptionUtil.GetDebugText(ex);
                    this.AppendResultText(strError + "\r\n");
                    this.SetProgressText(strError);
                    nRet = -1;
                }
                if (nRet == -1)
                {
                    AppendResultText("DoOneRecord() error : " + strError + "。\r\n");
                }


            CONTINUE:
                continue;

            } // end of for

            // 正常结束，复位断点
            this.App.RemoveBatchTaskBreakPointFile(this.Name);
            this.StartInfo.Start = "";

            AppendResultText("针对消息库 " + strMessageDbName + " 的循环结束。共处理 " + nRecCount.ToString() + " 条记录。\r\n");

            {

                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());  // 2007/12/17 changed // DateTime.UtcNow // 2012/5/27
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nRecCount.ToString() + " 个。";
                this.App.WriteErrorLog(strErrorText);

            }

            return;

        ERROR1:
            // 记忆断点
            this.StartInfo.Start = MemoBreakPoint(
                strID //strRecordID,
                );


            this.Loop = true;   // 便于稍后继续重新循环?
            startinfo.Param = MakeMessageMonitorParam(
                bLoop);


            AppendResultText("MessageMonitor thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("MessageMonitor thread error : " + strError + "\r\n");
            return;
        }

        // 记忆一下断点，以备不测
        string MemoBreakPoint(
            string strRecordID)
        {
            string strBreakPointString = "";

            strBreakPointString = MakeBreakPointString(
                strRecordID);

            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.Name,
                strBreakPointString);

            return strBreakPointString;
        }

        // 处理一条记录
        int DoOneRecord(
            long lPeriodValue,
            string strPeriodUnit,
            string strPath,
            string strRecXml,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strRecXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM出错: " + ex.Message;
                return -1;
            }

            string strDate = DomUtil.GetElementText(dom.DocumentElement,
                "date");

            bool bDelete = false;

            //
            DateTime date;

            try
            {
                date = DateTimeUtil.FromRfc1123DateTimeString(strDate);
            }
            catch
            {
                strError = "记录 "+strPath+" 消息日期值 '" + strDate + "' 格式错误";
                this.App.WriteErrorLog(strError);
                // 注意仍然要删除
                bDelete = true;
                goto DO_DELETE;
            }


            // 正规化时间date
            nRet = LibraryApplication.RoundTime(strPeriodUnit,
                ref date,
                out strError);
            if (nRet == -1)
            {
                strError = "正规化date时间 " +date.ToString()+ " (时间单位: "+strPeriodUnit+") 时出错: " + strError;
                return -1;
            }

            DateTime now = this.App.Clock.UtcNow;  //  DateTime.UtcNow;

            // 正规化时间now
            nRet = LibraryApplication.RoundTime(strPeriodUnit,
                ref now,
                out strError);
            if (nRet == -1)
            {
                strError = "正规化now时间 " + now.ToString() + " (时间单位: " + strPeriodUnit + ") 时出错: " + strError;
                return -1;
            }

            TimeSpan delta = now - date;

            long lDelta = 0;

            nRet = LibraryApplication.ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            if (lDelta >= lPeriodValue)
                bDelete = true;

        DO_DELETE:

            if (bDelete == true)
            {
                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

                byte[] output_timestamp = null;
                lRet = channel.DoDeleteRes(
    strPath,
    baTimeStamp,
    out output_timestamp,
    out strError);
                if (lRet == -1)
                {
                    // 可以这次不删除，以后还有机会
                    strError = "删除记录 " + strPath + "时出错: " + strError;
                    return -1;
                }

                // 这个指标没有按分馆来计算
                if (this.App.Statis != null)
                    this.App.Statis.IncreaseEntryValue(
                    "",
                    "消息监控",
                    "删除过期消息条数",
                    1);

            }

            return 0;
        }

    }
}
