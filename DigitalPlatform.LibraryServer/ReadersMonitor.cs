using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 监控读者信息的线程 例如监控即将发生的超期未还、刷新以停代金信息
    /// </summary>
    public class ReadersMonitor : BatchTask
    {
        public ReadersMonitor(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "超期通知";
            }
        }

        // 一次操作循环
        // TODO: 是否需要对读者记录加锁？
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            // 2007/12/18
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;
            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";
            int nRet = 0;

            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "readersMonitor";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strMonitorName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从文件中获取 "+strMonitorName+" 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                string strStartTimeDef = "";
                //      bRet    是否到了每日启动时间
                bool bRet = false;
                string strOldLastTime = strLastTime;

                // return:
                //      -2  strLastTime 格式错误
                //      -1  一般错误
                //      0   没有找到startTime配置参数
                //      1   找到了startTime配置参数
                nRet = IsNowAfterPerDayStart(
                    strMonitorName,
                    ref strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    string strErrorText = "获取 "+strMonitorName+" 每日启动时间时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    if (nRet == -2)
                    {
                        WriteLastTime(strMonitorName, "");
                    }
                    return;
                }

                // 如果nRet == 0，表示没有配置相关参数，则兼容原来的习惯，每次都作
                if (nRet == 0)
                {

                }
                else if (nRet == 1)
                {
                    bPerDayStart = true;

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
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + " 启动。");
            }

            this.AppendResultText("开始新一轮循环\r\n");

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            int nTotalRecCount = 0;

            for (int i = 0; i < this.App.ReaderDbs.Count; i++)
            {
#if NO
                // 系统挂起的时候，不运行本线程
                // 2008/5/27
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
                // 2012/2/4
                if (this.App.PauseBatchTask == true)
                    break;
#endif
                if (this.Stopped == true)
                    break;

                if (this.Stopped == true)
                    break;

                string strReaderDbName = this.App.ReaderDbs[i].DbName;

                AppendResultText("开始处理读者库 " + strReaderDbName + " 的循环\r\n");

                bool bFirst = true; // 2008/5/27 moved
                string strID = "1";
                int nOnePassRecCount = 0;
                for (; ; nOnePassRecCount++, nTotalRecCount++)
                {
#if NO
                    // 系统挂起的时候，不运行本线程
                    // 2008/2/4
                    if (this.App.HangupReason == HangupReason.LogRecover)
                        break;
                    // 2012/2/4
                    if (this.App.PauseBatchTask == true)
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

                    string strPath = strReaderDbName + "/" + strID;

                    string strXmlBody = "";
                    string strMetaData = "";
                    string strOutputPath = "";
                    byte[] baOutputTimeStamp = null;

                    // 
                    SetProgressText((nOnePassRecCount + 1).ToString() + " " + strPath);

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
                                    strError = "数据库 " + strReaderDbName + " 记录 " + strID + " 不存在。处理结束。";
                                }
                                else
                                {
                                    strError = "数据库 " + strReaderDbName + " 记录 " + strID + " 是最末一条记录。处理结束。";
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

                    string strLibraryCode = "";
                    nRet = this.App.GetLibraryCode(strOutputPath,
                        out strLibraryCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    bFirst = false;

                    this.AppendResultText("正在处理" + (nOnePassRecCount + 1).ToString() + " " + strOutputPath + "\r\n");

                    // 把id解析出来
                    strID = ResPath.GetRecordId(strOutputPath);

                    // 处理
                    nRet = DoOneRecord(
                        strOutputPath,
                        strLibraryCode,
                        strXmlBody,
                        baOutputTimeStamp,
                        out strError);
                    if (nRet == -1)
                    {
                        AppendResultText("DoOneRecord() error : " + strError + "。\r\n");
                        // 循环并不停止
                    }

                CONTINUE:
                    continue;

                } // end of for

                AppendResultText("针对读者库 " + strReaderDbName + " 的循环结束。共处理 " + nOnePassRecCount.ToString() + " 条记录。\r\n");

            }
            AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");
            SetProgressText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。");

            {
                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());  // 2007/12/17 changed // DateTime.UtcNow
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nTotalRecCount.ToString() + " 个。";
                this.App.WriteErrorLog(strErrorText);

            }

            return;
        ERROR1:
            AppendResultText("ReadersMonitor thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("ReadersMonitor thread error : " + strError + "\r\n");
            return;
        }

        // 处理一条记录
        int DoOneRecord(
            string strPath,
            string strLibraryCode,
            string strReaderXml,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            int nRedoCount = 0;

            REDO:

            byte[] output_timestamp = null;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM出错: " + ex.Message;
                return -1;
            }

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            Calendar calendar = null;
            // return:
            //      -1  出错
            //      0   没有找到日历
            //      1   找到日历
            nRet = this.App.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out calendar,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                strError = "获得读者类型 '"+strReaderType+"' 的相关日历过程失败: " + strError;
                return -1;
            }

            bool bChanged = false;

            List<string> bodytypes = new List<string>();
            bodytypes.Add("dpmail");
            bodytypes.Add("email");
            if (this.App.m_externalMessageInterfaces != null)
            {
                foreach(MessageInterface message_interface in this.App.m_externalMessageInterfaces)
                {
                    bodytypes.Add(message_interface.Type);
                }
            }

            // 每种 bodytype 做一次
            for (int i = 0; i < bodytypes.Count; i++)
            {
                string strBodyType = bodytypes[i];

                string strReaderEmailAddress = "";
                if (strBodyType == "email")
                {
                    strReaderEmailAddress = DomUtil.GetElementText(readerdom.DocumentElement,
                        "email");
                    // 读者记录中没有email地址，就无法进行email方式的通知了
                    if (String.IsNullOrEmpty(strReaderEmailAddress) == true)
                        continue;
                }

                if (strBodyType == "dpmail")
                {
                    if (this.App.MessageCenter == null)
                    {
                        continue;
                    }
                }

#if NO
                List<string> notifiedBarcodes = new List<string>();


                // 获得特定类型的已通知过的册条码号列表
                // return:
                //      -1  error
                //      其他    notifiedBarcodes中条码号个数
                nRet = GetNotifiedBarcodes(readerdom,
                    strBodyType,
                    out notifiedBarcodes,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif

                int nResultValue = 0;
                string strBody = "";
                // List<string> wantNotifyBarcodes = null;
                string strMime = "";

                // 保存调用脚本前的读者记录 
                string strOldReaderXml = readerdom.DocumentElement.OuterXml;

                // 执行脚本函数NotifyReader
                // parameters:
                // return:
                //      -2  not found script
                //      -1  出错
                //      0   成功
                // nResultValue
                //      -1  出错
                //      0   没有必要发送
                //      1   需要发送
                nRet = this.App.DoNotifyReaderScriptFunction(
                        readerdom,
                        calendar,
                        // notifiedBarcodes,
                        strBodyType,
                        out nResultValue,
                        out strBody,
                        out strMime,
                        // out wantNotifyBarcodes,
                        out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("DoNotifyReaderScriptFunction [barcode=" + strReaderBarcode + "] error: " + strError + "\r\n");
                    continue;
                }

                // 2010/12/18
                // 不可能发生。因为基类有NotifyReader()函数了
                if (nRet == -2)
                    break;

                if (nResultValue == -1)
                {
                    this.AppendResultText("DoNotifyReaderScriptFunction [strReaderBarcode=" + strReaderBarcode + "] nResultValue == -1, errorinfo: " + strError + "\r\n");
                    continue;
                }

                if (nResultValue == 0)
                {
                    // 不要发送邮件
                    continue;
                }

                bool bSendMessageError = false;

                if (nResultValue == 1 && nRedoCount == 0)   // 2008/5/27 changed 重做的时候，不再发送消息，以免消息库记录爆满
                {
                    // 发送邮件

                    if (strBodyType == "dpmail")
                    {
                        // 发送消息
                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = this.App.MessageCenter.SendMessage(
                            this.RmsChannels,
                            strReaderBarcode,
                            "图书馆",
                            "借阅信息提示",
                            strMime,    // "text",
                            strBody,
                            false,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "发送dpmail出错: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(strLibraryCode,
                                "超期通知",
                                "dpmail message 超期通知消息发送错误数",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "超期通知",
                                "dpmail超期通知人数",
                                1);
                        }
                    }

                    MessageInterface external_interface = this.App.GetMessageInterface(strBodyType);

                    if (external_interface != null)
                    {
                        // 发送消息
                        try
                        {
                            // 发送一条消息
                            // parameters:
                            //      strPatronBarcode    读者证条码号
                            //      strPatronXml    读者记录XML字符串。如果需要除证条码号以外的某些字段来确定消息发送地址，可以从XML记录中取
                            //      strMessageText  消息文字
                            //      strError    [out]返回错误字符串
                            // return:
                            //      -1  发送失败
                            //      0   没有必要发送
                            //      >=1   发送成功，返回实际发送的消息条数
                            nRet = external_interface.HostObj.SendMessage(
                                strReaderBarcode,
                                readerdom.DocumentElement.OuterXml,
                                strBody,
                                strLibraryCode,
                                out strError);
                        }
                        catch (Exception ex)
                        {
                            strError = external_interface.Type + " 类型的外部消息接口Assembly中SendMessage()函数抛出异常: " + ex.Message;
                            nRet = -1;
                        }
                        if (nRet == -1)
                        {
                            strError = "向读者 '"+strReaderBarcode+"' 发送" + external_interface.Type + " message时出错: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "超期通知",
                                external_interface.Type + " message 超期通知消息发送错误数",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else if (nRet >= 1)
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(strLibraryCode,
                                "超期通知", 
                                external_interface.Type + " message 超期通知人数",
                                1);
                        }
                    }

                    if (strBodyType == "email")
                    {
                        // 发送email
                        // return:
                        //      -1  error
                        //      0   not found smtp server cfg
                        //      1   succeed
                        nRet = this.App.SendEmail(strReaderEmailAddress,
                            "借阅信息提示",
                            strBody,
                            strMime,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "发送email出错: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "超期通知",
                                "email message 超期通知消息发送错误数",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else if (nRet == 1)
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "超期通知",
                                "email超期通知人数", 
                                1);
                        }
                    }

                }

                if (bSendMessageError == false)
                {
#if NO
                    // 在读者记录中标记出那些已经发送过通知的册，避免以后重复通知
                    // return:
                    //      -1  error
                    //      0   没有修改
                    //      1   发生过修改
                    nRet = MaskSendItems(
                        ref readerdom,
                        strBodyType,
                        wantNotifyBarcodes,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (nRet == 1)
                        bChanged = true;
#endif

                    bChanged = true;
                }
            } // end of for

            // 刷新以停代金情况
            // 2007/12/17
            if (StringUtil.IsInList("pauseBorrowing", this.App.OverdueStyle) == true)
            {
                //
                // 处理以停代金功能
                // return:
                //      -1  error
                //      0   readerdom没有修改
                //      1   readerdom发生了修改
                nRet = this.App.ProcessPauseBorrowing(
                    strLibraryCode,
                    ref readerdom,
                    strPath,
                    "#readersMonitor",
                    "refresh",
                    "", // 因为是机触发，所以不记载IP地址
                    out strError);
                if (nRet == -1)
                {
                    strError = "在refresh以停代金的过程中发生错误: " + strError;
                    this.AppendResultText(strError + "\r\n");
                }

                if (nRet == 1)
                    bChanged = true;
            }

            // 修改读者记录后存回
            if (bChanged == true)
            {
                string strOutputPath = "";
                lRet = channel.DoSaveTextRes(strPath,
                    readerdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    baTimeStamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // 时间戳冲突
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount > 10)    // 2008/5/27
                        {
                            strError = "ReadersMonitor 写回读者库记录 '" + strPath + "' 时发生时间戳冲突，重试10次后仍发生时间戳冲突: " + strError;
                            return -1;
                        }

                        string strStyle = "data,content,timestamp,outputpath";

                        string strMetaData = "";
                        // string strOutputPath = "";
                        lRet = channel.GetRes(strPath,
                            strStyle,
                            out strReaderXml,
                            out strMetaData,
                            out baTimeStamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "写回读者库记录 '" + strPath + "' 时发生时间戳冲突，重装记录时又发生错误: " + strError;
                            return -1;
                        }

                        nRedoCount++;
                        goto REDO;
                    }
                    
                    strError = "写回读者库记录 '" + strPath + "' 时发生错误: " + strError;
                    return -1;
                }
            }

            return 0;
        }

#if NO
        // 获得特定类型的已通知过的册条码号列表
        // return:
        //      -1  error
        //      其他    notifiedBarcodes中条码号个数
        int GetNotifiedBarcodes(XmlDocument readerdom,
            string strBodyType,
            out List<string> notifiedBarcodes,
            out string strError)
        {
            strError = "";
            notifiedBarcodes = new List<string>();

            // 列出全部借阅的册
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strItemBarcode = DomUtil.GetAttr(node, "barcode");

                if (String.IsNullOrEmpty(strItemBarcode) == true)
                    continue;

                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                bool bNotified = IsNotified(strBodyType,
                    strHistory);
                if (bNotified == false)
                    continue;

                notifiedBarcodes.Add(strItemBarcode);
            }

            return notifiedBarcodes.Count;
        }
#endif

#if NO
        // 在读者记录中标记出那些已经发送过通知的册，避免以后重复通知
        // return:
        //      -1  error
        //      0   没有修改
        //      1   发生过修改
        int MaskSendItems(
            ref XmlDocument readerdom,
            string strBodyType,
            List<string> wantNotifyBarcodes,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            bool bChanged = false;

            for (int i = 0; i < wantNotifyBarcodes.Count; i++)
            {
                string strItemBarcode = wantNotifyBarcodes[i];

                XmlNode node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='"+strItemBarcode+"']");
                if (node == null)
                {
                    strError = "册条码号 '" + strItemBarcode + "' 在读者记录中居然没有找到对应的<borrows/borrow>元素。";
                    return -1;
                }

                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                nRet = ModifyHistoryString(strBodyType,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ModifyHistoryString() error : " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    bChanged = true;
                    DomUtil.SetAttr(node, "notifyHistory", strHistory);
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // 获得一种 body type 的全部通知字符
        public static string GetNotifiedChars(LibraryApplication app,
            string strBodyType,
            string strHistory)
        {
            int nExtendCount = 0;   // 扩展接口的个数
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    // strError = "不能识别的 message type '" + strBodyType + "'";
                    // return -1;
                    return null;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    // strError = "external_interface (type '" + external_interface.Type + "') 没有在 m_externalMessageInterfaces 数组中找到";
                    // return -1;
                    return null;
                }
                index += 2;
            }

            string strResult = "";
            for (int i = 0; i < strHistory.Length / nSegmentLength; i++)
            {
                int nStart = i * nSegmentLength;
                int nLength = nSegmentLength;
                if (nStart + nLength > strHistory.Length)
                    nLength = strHistory.Length - nStart;

                string strSegment = strHistory.Substring(nStart, nLength);
                if (index < strSegment.Length)
                    strResult += strSegment[index];
                else
                    strResult += 'n';
            }

            return strResult;
        }

        // 合并设置一种 body type 的全部通知字符
        // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
        public static int SetNotifiedChars(LibraryApplication app,
            string strBodyType,
            string strChars,
            ref string strHistory,
            out string strError)
        {
            strError = "";

            int nExtendCount = 0;   // 扩展接口的个数
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "不能识别的 message type '" + strBodyType + "'";
                    return -1;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') 没有在 m_externalMessageInterfaces 数组中找到";
                    return -1;
                }
                index += 2;
            }

            for (int i = 0; i < strChars.Length; i++)
            {
                char ch = strChars[i];
                if (ch == 'n')
                    continue;

                int nLength = (i + 1) * nSegmentLength;
                if (strHistory.Length < nLength)
                    strHistory = strHistory.PadRight(nLength, 'n');
                int nOffs = i * nSegmentLength + index;
                strHistory = strHistory.Remove(nOffs, 1);
                strHistory = strHistory.Insert(nOffs, "y");
            }

            return 0;
        }

        /// <summary>
        /// 修改一个字符位
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <param name="index">要设置的位置。从 0 开始计算</param>
        /// <param name="ch">要设置的字符</param>
        /// <param name="chBlank">空白字符。扩展字符串长度的时候，填充这个字符</param>
        public static void SetChar(ref string strText,
            int index,
            char ch,
            char chBlank = 'n')
        {
            if (strText.Length < index + 1)
                strText = strText.PadRight(index + 1, chBlank);

            strText = strText.Remove(index, 1);
            strText = strText.Insert(index, new string(ch,1));
        }

#if NO
        // 观察历史字符串的某位的 'y'/'n' 状态
        // parameters:
        //      strBodyType 通知消息的接口 (媒体) 类型
        //      nTimeIndex  2013/9/24 催还的次数下标。0 表示已经超期时的催还，1 等以后的值表示配置字符串中定义的特定次数的提醒通知，也就是尚未超期时候的提醒
        public static bool IsNotified(
            LibraryApplication app,
            string strBodyType,
            int nTimeIndex,
            string strHistory)
        {
            Debug.Assert(nTimeIndex >= 0, "");

            int nExtendCount = 0;   // 扩展接口的个数
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    // strError = "不能识别的 message type '" + strBodyType + "'";
                    // return -1;
                    return false;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    // strError = "external_interface (type '" + external_interface.Type + "') 没有在 m_externalMessageInterfaces 数组中找到";
                    // return -1;
                    return false;
                }
                index += 2; 
            }

            // 计算在整体中的偏移
            index = (nSegmentLength * nTimeIndex) + index;

            if (strHistory.Length < index + 1)
                return false;

            if (strHistory[index] == 'y')
                return true;

            return false;
        }

        // 设置历史字符串的某位的 'y' 状态
        public static int SetNotified(
            LibraryApplication app,
            string strBodyType,
            int nTimeIndex,
            ref string strHistory,
            out string strError)
        {
            strError = "";
            Debug.Assert(nTimeIndex >= 0, "");

            int nExtendCount = 0;   // 扩展接口的个数
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "不能识别的 message type '" + strBodyType + "'";
                    return -1;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') 没有在 m_externalMessageInterfaces 数组中找到";
                    return -1;
                }
                index += 2;
            }

            // 计算在整体中的偏移
            index = (nSegmentLength * nTimeIndex) + index;

            if (strHistory.Length < index + 1)
                strHistory = strHistory.PadRight(index + 1, 'n');

            strHistory = strHistory.Remove(index, 1);
            strHistory = strHistory.Insert(index, "y");
            return 1;
        }
#endif

#if NO
        // 修改通知历史字符串，把特定的位设置为'y'
        // return:
        //      -1  error
        //      0   没有发生修改
        //      1   发生了修改
        int ModifyHistoryString(string strBodyType,
            ref string strHistory,
            out string strError)
        {
            strError = "";
            int index = -1;

            bool bChanged = false;

            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = this.App.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "不能识别的 message type '" + strBodyType + "'";
                    return -1;
                }

                index = this.App.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') 没有在 m_externalMessageInterfaces 数组中找到";
                    return -1;
                }
                index += 2;
            }

            if (strHistory.Length < index + 1)
            {
                strHistory = strHistory.PadRight(index + 1, 'n');
                bChanged = true;
            }

            if (strHistory[index] != 'y')
            {
                strHistory = strHistory.Remove(index);
                strHistory = strHistory.Insert(index, "y");
                // strHistory[index] = 'y';
                bChanged = true;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // 检查当前是否有潜在的超期册
        // return:
        //      -1  error
        //      0   没有超期册
        //      1   有超期册
        int CheckOverdue(
            Calendar calendar,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            int nOverCount = 0;
            int nRet = 0;

            LibraryApplication app = this.App;

            string strOverdueItemBarcodeList = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            XmlNode node = null;
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                    string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strOperator = DomUtil.GetAttr(node, "operator");

                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 2009/3/13
                    nRet = app.CheckPeriod(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "读者记录中 有关册 '" + strBarcode + "' 的借阅期限信息检查出现错误：" + strError;
                    }

                    if (nRet == 1)
                    {
                        if (strOverdueItemBarcodeList != "")
                            strOverdueItemBarcodeList += ",";
                        strOverdueItemBarcodeList += strBarcode;
                        nOverCount++;
                    }


                }

                // 发现未归还的册中出现了超期情况
                if (nOverCount > 0)
                {
                    strError = "该读者当前有 " + Convert.ToString(nOverCount) + " 个未还超期册: " + strOverdueItemBarcodeList + ""; 
                    return 1;
                }
            }

            return 0;
        }
    }
}
