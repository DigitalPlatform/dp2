using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.Web;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Collections;


namespace DigitalPlatform.LibraryServer
{
    /// 预约到书监控任务
    /// 荐购到书通知也在这里进行
    public class ArriveMonitor : BatchTask
    {

        public ArriveMonitor(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "预约到书管理";
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
        //      strStart    启动字符串。格式为XML
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParseArriveMonitorStart(string strStart,
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

        public static string MakeArriveMonitorParam(
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
        public static int ParseArriveMonitorParam(string strParam,
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

            /*
            // 代为刷新
            if (this.App.Statis != null)
            {
                this.App.Statis.Flush();

                // 2008/3/27 new add
                if (this.App.Changed == true)
                {
                    this.App.Flush();
                }
            }*/

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 通用启动参数
            bool bLoop = true;
            nRet = ParseArriveMonitorParam(startinfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            string strID = "";
            nRet = ParseArriveMonitorStart(startinfo.Start,
                out strID,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                this.Loop = false;
                return;
            }

            ////

            //
            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "arriveMonitor";
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

            this.AppendResultText("开始新一轮循环\r\n");

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            this._calendarTable.Clear();

            int nRecCount = 0;
            for (; ; nRecCount ++)
            {
#if NO
                // 系统挂起的时候，不运行本线程
                // 2008/2/4
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
#endif

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

                string strPath = this.App.ArrivedDbName + "/" + strID;

                string strXmlBody = "";
                string strMetaData = "";
                string strOutputPath = "";
                byte[] baOutputTimeStamp = null;

                // 
                this.SetProgressText((nRecCount + 1).ToString() + " " + strPath);
                this.AppendResultText("正在处理 " + (nRecCount + 1).ToString() + " " + strPath + "\r\n");

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
                                strError = "记录 " + strID + " 不存在。处理结束。";
                            }
                            else
                            {
                                strError = "记录 " + strID + " 是最末一条记录。处理结束。";
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

#if NO
                string strLibraryCode = "";
                nRet = this.App.GetLibraryCode(strOutputPath,   // ???? BUG
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#endif


                bFirst = false;

                // 把id解析出来
                strID = ResPath.GetRecordId(strOutputPath);

                // 处理
                nRet = DoOneRecord(
                    // calendar,
                    strOutputPath,
                    strXmlBody,
                    baOutputTimeStamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            CONTINUE:
                continue;

            } // end of for

            this.AppendResultText("循环结束。共处理 " + nRecCount.ToString() + " 条记录。\r\n");

            {
                Debug.Assert(this.App != null);

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime()); // 2007/12/17 changed // DateTime.UtcNow // 2012/5/27
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nRecCount.ToString() + " 个。";
                this.App.WriteErrorLog(strErrorText);

            }

            return;

        ERROR1:
            this.AppendResultText("arrivethread error : " + strError + "\r\n");
            this.App.WriteErrorLog("arrivethread error : " + strError);
            return;
        }

        // 判断是否超过保留期限
        // return:
        //      -1  error
        //      0   没有超过
        //      1   已经超过
        int CheckeOutOfReservation(
            Calendar calendar,
            XmlDocument queue_rec_dom,
            out string strError)
        {
            strError = "";

            string strState = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
    "state");

            // 对通知完成后的记录, 循环中不必处理
            if (StringUtil.IsInList("outof", strState) == true)
                return 0;

            string strNotifyDate = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "notifyDate");

            /*
            string strItemBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "readerBarcode");
             * */


            // 解析期限值
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = LibraryApplication.ParsePeriodUnit(
                this.App.ArrivedReserveTimeSpan,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "预约保留期限 值 '" + this.App.ArrivedReserveTimeSpan + "' 格式错误: " + strError;
                return -1;
            }

            //
            DateTime notifydate;

            try
            {
                notifydate = DateTimeUtil.FromRfc1123DateTimeString(strNotifyDate);
            }
            catch
            {
                strError = "通知日期值 '" + strNotifyDate + "' 格式错误";
                return -1;
            }


            DateTime timeEnd = DateTime.MinValue;

            nRet = LibraryApplication.GetOverTime(
                calendar,
                notifydate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out strError);
            if (nRet == -1)
            {
                strError = "计算保留期过程发生错误: " + strError;
                return -1;
            }

            DateTime now = this.App.Clock.UtcNow;  //  DateTime.UtcNow;

            // 正规化时间
            nRet = LibraryApplication.RoundTime(strPeriodUnit,
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now - timeEnd;

            long lDelta = 0;

            nRet = LibraryApplication.ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            if (lDelta > 0)
                return 1;

            return 0;
        }

        // 获得读者类型
        // return:
        //      -1  出错
        //      0   没有找到读者记录
        //      1   找到
        int GetReaderType(string strReaderBarcode,
            out string strReaderType,
            out string strError)
        {
            strError = "";
            strReaderType = "";

            if (string.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "strReaderBarcode 不能为空";
                return -1;
            }

            // 读入读者记录
            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            byte[] reader_timestamp = null;

            int nRet = this.App.GetReaderRecXml(
                this.RmsChannels,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                return 0;
            }
            if (nRet == -1)
            {
                strError = "读入读者记录时发生错误: " + strError;
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            strReaderType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");
            return 1;
        }

        // string m_strCalendarLibraryCode = null;

        // 日历对象的 cache。 馆代码 + | + 读者类型 --> 日历对象
        Hashtable _calendarTable = new Hashtable();

        // 处理一条记录
        // parameters:
        //      strQueueRecPath 预约到书队列记录的路径
        int DoOneRecord(
            // Calendar calendar,
            string strQueueRecPath,
            string strQueueRecXml,
            byte[] baQueueRecTimeStamp,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strQueueRecXml);
            }
            catch (Exception ex)
            {
                strError = "装载对列记录XML到DOM出错: " + ex.Message;
                return -1;
            }

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            // 对通知完成后的记录, 循环中不必处理
            if (StringUtil.IsInList("outof", strState) == true)
                return 0;

            // TODO: 读者的馆代码
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement,
    "libraryCode");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "readerBarcode");

            // 2015/5/20
            // 通过读者证条码号获得读者类型
            string strReaderType = "";
            // 获得读者类型
            // return:
            //      -1  出错
            //      0   没有找到读者记录
            //      1   找到
            nRet = GetReaderType(strReaderBarcode,
        out strReaderType,
        out strError);
            if (nRet == -1)
                strReaderType = "";

            string strKey = strLibraryCode + "|" + strReaderType;
            Calendar calendar = (Calendar)_calendarTable[strKey];
            if (calendar == null)
            {
                // return:
                //      -1  出错
                //      0   没有找到日历
                //      1   找到日历
                nRet = this.App.GetReaderCalendar(strReaderType,
                    strLibraryCode,
                    out calendar,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    calendar = null;

                if (calendar != null && _calendarTable.Count < 10000)
                    _calendarTable[strKey] = calendar;
            }
#if NO
            if (this.m_calendar == null
                || this.m_strCalendarLibraryCode != strLibraryCode
                )
            {
                // return:
                //      -1  出错
                //      0   没有找到日历
                //      1   找到日历
                nRet = this.App.GetReaderCalendar(null,
                    strLibraryCode,
                    out m_calendar,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    this.m_strCalendarLibraryCode = strLibraryCode;
                else
                {
                    m_calendar = null;
                    this.m_strCalendarLibraryCode = "";
                }
            }
#endif

            // 判断是否超过保留期限
            // return:
            //      -1  error
            //      0   没有超过
            //      1   已经超过
            nRet = CheckeOutOfReservation(
                    calendar,
                    dom,
                    out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "itemBarcode");

                // 通知当前读者，他超过了取书的保留期限
                string strNotifyDate = DomUtil.GetElementText(dom.DocumentElement,
                    "notifyDate");
                nRet = AddReaderOutOfReservationInfo(
                        this.RmsChannels,
                        strReaderBarcode,
                        strItemBarcode,
                        strNotifyDate,
                        out strError);
                if (nRet == -1)
                {
                    this.App.WriteErrorLog("AddReaderOutOfReservationInfo() error: " + strError);
                }

                // 已经超过保留期限，要通知下一位预约者
                nRet = this.App.DoNotifyNext(
                        this.RmsChannels,
                        strQueueRecPath,
                        dom,
                        baQueueRecTimeStamp,
                        out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    // 需要归架
                    // 册记录中<location>需要去掉#reservation，相关<request>元素也需要删除

                    nRet = RemoveEntityReservationInfo(strItemBarcode,
                        strReaderBarcode,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "RemoveEntityReservationInfo() error: " + strError;
                        return -1;
                    }
                }
            }

            return 0;
        }

        // 给读者记录里加上预约到书后超期不取的状态
        int AddReaderOutOfReservationInfo(
            RmsChannelCollection channels,
            string strReaderBarcode,
            string strItemBarcode,
            string strNotifyDate,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            int nRedoCount = 0;

            REDO_MEMO:
            // 加读者记录锁
            this.App.ReaderLocks.LockForWrite(strReaderBarcode);

            try // 读者记录锁定范围开始
            {

                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.App.GetReaderRecXml(
                    channels,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    return -1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                XmlNode root = readerdom.DocumentElement.SelectSingleNode("outofReservations");
                if (root == null)
                {
                    root = readerdom.CreateElement("outofReservations");
                    readerdom.DocumentElement.AppendChild(root);
                }

                // 累计次数
                string strCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strCount) == true)
                    strCount = "0";
                int nCount = 0;
                try
                {
                    nCount = Convert.ToInt32(strCount);
                }
                catch
                {
                }
                nCount++;
                DomUtil.SetAttr(root, "count", nCount.ToString());

                // 追加<request>元素
                XmlNode request = readerdom.CreateElement("request");
                root.AppendChild(request);
                DomUtil.SetAttr(request, "itemBarcode", strItemBarcode);
                DomUtil.SetAttr(request, "notifyDate", strNotifyDate);

                byte[] output_timestamp = null;
                string strOutputPath = "";

                RmsChannel channel = channels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                // 写回读者记录
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "写回读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                            return -1;
                        }
                        goto REDO_MEMO;
                    }
                    return -1;
                }


            } // 读者记录锁定范围结束
            finally
            {
                this.App.ReaderLocks.UnlockForWrite(strReaderBarcode);
            }

            return 0;
        }

        // 去除册记录中过时的预约信息
        // 册记录中<location>需要去掉#reservation，相关<request>元素也需要删除
        // 锁定：本函数对EntityLocks加了锁定
        int RemoveEntityReservationInfo(string strItemBarcode,
            string strReaderBarcode,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "册条码号不能为空。";
                return -1;
            }
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "读者证条码号不能为空。";
                return -1;
            }

            // 加册记录锁
            this.App.EntityLocks.LockForWrite(strItemBarcode);

            try // 册记录锁定范围开始
            {
                // 从册条码号获得册记录

                int nRedoCount = 0;

            REDO_CHANGE:
                List<string> aPath = null;
                string strItemXml = "";
                byte[] item_timestamp = null;
                string strOutputItemRecPath = "";

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetItemRecXml(
                    this.RmsChannels,
                    strItemBarcode,
                    out strItemXml,
                    100,
                    out aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号 '" + strItemBarcode + "' 不存在";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "读入册记录时发生错误: " + strError;
                    return -1;
                }

                if (aPath.Count > 1)
                {
                    strError = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修改册记录的操作。";
                    return -1;
                }
                else
                {

                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");

                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                    }
                }

                XmlDocument itemdom = null;
                nRet = LibraryApplication.LoadToDom(strItemXml,
                    out itemdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载册记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 修改册记录

                // 册记录中<location>需要去掉#reservation，相关<request>元素也需要删除
                string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                    "location");
                // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
                strLocation = StringUtil.GetPureLocationString(strLocation);
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "location", strLocation);

                XmlNode nodeRequest = itemdom.DocumentElement.SelectSingleNode("reservations/request[@reader='" + strReaderBarcode + "']");
                if (nodeRequest != null)
                {
                    nodeRequest.ParentNode.RemoveChild(nodeRequest);
                }

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);


                // 写回册记录
                byte[] output_timestamp = null;
                string strOutputPath = "";
                long lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "写回册记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                            return -1;
                        }
                        goto REDO_CHANGE;
                    }
                }

            } // 册记录锁定范围结束
            finally
            {
                // 解册记录锁
                this.App.EntityLocks.UnlockForWrite(strItemBarcode);
            }


            return 0;
        }





    }
}
