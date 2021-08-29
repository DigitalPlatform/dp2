using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Messaging;

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

        MessageQueue _queue = null;

        // 一次操作循环
        // TODO: 是否需要对读者记录加锁？
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            // 2007/12/18
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";
            int nRet = 0;

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

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
                    string strErrorText = "获取 " + strMonitorName + " 每日启动时间时发生错误: " + strError;
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
                else if (nRet == 1 && startinfo.Start == "activate")
                {
                    // 2015/10/3
                    // 虽然 library.xml 中定义了每日定时启动，但被前端要求立即启动
                    this.AppendResultText("任务 '" + this.Name + "' 被立即启动\r\n");
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
                // 2021/3/16
                // 在错误日志中记一笔是否开启了以停代金功能
                if (StringUtil.IsInList("pauseBorrowing", this.App.OverdueStyle) == true)
                    this.App.WriteErrorLog("以停代金功能已经开启");
                else
                    this.App.WriteErrorLog("以停代金功能*尚未*开启");
            }

            this.AppendResultText("开始新一轮循环\r\n");


            recpath_table.Clear();

            if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false)
            {
                try
                {
                    _queue = new MessageQueue(this.App.OutgoingQueue);
                }
                catch (Exception ex)
                {
                    strError = "创建路径为 '" + this.App.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
                this._queue = null;

            List<string> bodytypes = new List<string>();

            string strBodyTypesDef = GetBodyTypesDef(); // 已经处理了默认值情况

#if NO
            if (string.IsNullOrEmpty(strBodyTypesDef) == true)
            {
                strBodyTypesDef = "dpmail,email";   // 空表示只使用两种保守的类型
                bodytypes = StringUtil.SplitList(strBodyTypesDef);
            }
            else 
#endif
            if (strBodyTypesDef == "[all]")
            {
                // 全部类型。包括 mq 和外部接口
                bodytypes.Add("dpmail");
                bodytypes.Add("email");
                if (string.IsNullOrEmpty(this.App.OutgoingQueue) == false)
                    bodytypes.Add("mq");    // MSMQ 消息队列
                if (this.App.m_externalMessageInterfaces != null)
                {
                    foreach (MessageInterface message_interface in this.App.m_externalMessageInterfaces)
                    {
                        bodytypes.Add(message_interface.Type);
                    }
                }
            }
            else
                bodytypes = StringUtil.SplitList(strBodyTypesDef);

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            int nTotalRecCount = 0;
            foreach (DigitalPlatform.LibraryServer.LibraryApplication.ReaderDbCfg cfg in this.App.ReaderDbs)
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

                string strReaderDbName = cfg.DbName;
                string strLibraryCode = cfg.LibraryCode;

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
                        if (channel.IsNotFound())
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

#if NO
                    string strLibraryCode = "";
                    nRet = this.App.GetLibraryCode(strOutputPath,
                        out strLibraryCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#endif

                    bFirst = false;

                    this.AppendResultText("正在处理" + (nOnePassRecCount + 1).ToString() + " " + strOutputPath + "\r\n");

                    // 把id解析出来
                    strID = ResPath.GetRecordId(strOutputPath);

                    int nRedoCount = 0;
                REDO:
                    // 处理
                    // parameters:
                    //      bChanged    [out] strReaderXml 是否发生过改变
                    // return:
                    //      -1  出错
                    //      0   正常
                    nRet = DoOneRecord(
                        bodytypes,
                        strOutputPath,
                        strLibraryCode,
                        ref strXmlBody,
                        nRedoCount,
                        baOutputTimeStamp,
                        out bool bChanged,
                        out strError);
                    if (nRet == -1)
                    {
                        AppendResultText("DoOneRecord() error : " + strError + "。\r\n");
                        // 循环并不停止
                    }
                    if (bChanged == true)
                    {
                        // return:
                        //      -2  保存时遇到时间戳冲突，已经重新装载读者记录返回于 strReaderXml 中，时间戳于 output_timestamp 中
                        //      -1  出错
                        //      0   保存成功
                        nRet = SavePatronRecord(channel,
    strOutputPath,
    ref strXmlBody,
    baOutputTimeStamp,
    out byte[] output_timestamp,
    out strError);
                        if (nRet == -1)
                        {
                            AppendResultText("SavePatronRecord() error : " + strError + "。\r\n");
                            // 循环并不停止
                        }
                        else if (nRet == -2)
                        {
                            if (nRedoCount > 10)
                            {
                                AppendResultText("SavePatronRecord() (遇到时间戳不匹配)重试十次以后依然出错，放弃重试。error : " + strError + "。\r\n");
                                // 循环并不停止
                            }
                            baOutputTimeStamp = output_timestamp;
                            goto REDO;
                        }
                    }

                CONTINUE:
                    continue;
                } // end of for

                AppendResultText("针对读者库 " + strReaderDbName + " 的循环结束。共处理 " + nOnePassRecCount.ToString() + " 条记录。\r\n");
            }

            recpath_table.Clear();

            AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");
            SetProgressText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。");

            // 2015/10/3
            // 让前端激活的任务，只执行一次。如果配置了每日激活时间，后面要再执行，除非是每日激活时间已到
            if (startinfo.Start == "activate")
                startinfo.Start = "";

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

        // 获得通知类型的定义
        // monitors/readersMonitor 元素的 types 属性值。缺省为"dpmail,email"
        string GetBodyTypesDef()
        {
            if (this.App.LibraryCfgDom == null || this.App.LibraryCfgDom.DocumentElement == null)
                return "";  // DOM 对象，或者根元素不存在
            XmlElement def_node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("monitors/readersMonitor") as XmlElement;
            if (def_node == null)
                return "dpmail,email";  // 属性缺省后，缺省值是 "dpmail,mail"
            return def_node.GetAttribute("types");
        }

        // 处理一条读者记录
        // parameters:
        //      bChanged    [out] strReaderXml 是否发生过改变
        // return:
        //      -1  出错
        //      0   正常
        int DoOneRecord(
            List<string> bodytypes,
            string strPath,
            string strLibraryCode,
            ref string strReaderXml,
            int nRedoCount,
            byte[] baTimeStamp,
            out bool bChanged,
            out string strError)
        {
            strError = "";
            bChanged = false;
            // long lRet = 0;
            int nRet = 0;

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            // int nRedoCount = 0;

            //REDO:
            //byte[] output_timestamp = null;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "装载 XML 到 DOM 出错: " + ex.Message;
                return -1;
            }

            // 2020/3/2
            string oldLibraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode");
            if (oldLibraryCode != strLibraryCode)
            {
                DomUtil.SetElementText(readerdom.DocumentElement, "libraryCode", strLibraryCode);
                bChanged = true;
            }

            // 2020/9/8
            string oldOI = DomUtil.GetElementText(readerdom.DocumentElement, "oi");
            App.AddPatronOI(readerdom, strLibraryCode);
            string newOI = DomUtil.GetElementText(readerdom.DocumentElement, "oi");
            if (oldOI != newOI)
                bChanged = true;

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            string strRefID = DomUtil.GetElementText(readerdom.DocumentElement,
                "refID");

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            // return:
            //      -1  出错
            //      0   没有找到日历
            //      1   找到日历
            nRet = this.App.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out Calendar calendar,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                // 注: 借书和还书是否需要工作日历参数? 如果也需要，则在相关读者借书或者还书时候就会遇到报错，管理员会及时处理
                // TODO: 将来在这里增加通知系统管理员的动作
                strError = "获得读者类型 '" + strReaderType + "' 的相关日历过程失败: " + strError;
                this.AppendResultText(strError + "\r\n");
                // return -1;
                calendar = null;
                // 继续往后运行。和 calendar 无关的功能还能起作用
            }

            // testing
            calendar = null;

            // 每种 bodytype 做一次
            for (int i = 0; i < bodytypes.Count; i++)
            {
                string strBodyType = bodytypes[i];

                string strReaderEmailAddress = "";
                if (strBodyType == "email")
                {
                    string strValue = DomUtil.GetElementText(readerdom.DocumentElement,
                        "email");
#if NO
                    // 注: email 元素内容，现在是存储 email 和微信号等多种绑定途径 2016/4/16
                    // return:
                    //      null    没有找到前缀
                    //      ""      找到了前缀，并且值部分为空
                    //      其他     返回值部分
                    strReaderEmailAddress = StringUtil.GetParameterByPrefix(strReaderEmailAddress,
            "email",
            ":");
                    // 读者记录中没有email地址，就无法进行email方式的通知了
                    if (String.IsNullOrEmpty(strReaderEmailAddress) == true)
                    {
                        if (strValue.IndexOf(":") != -1)
                            continue;
                        strReaderEmailAddress = strValue;
                    }
#endif
                    strReaderEmailAddress = LibraryServerUtil.GetEmailAddress(strValue);
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

                // List<string> wantNotifyBarcodes = null;

                // 保存调用脚本前的读者记录 
                string strOldReaderXml = readerdom.DocumentElement.OuterXml;

                if (calendar == null)
                    continue;

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
                        out int nResultValue,
                        out string strBody,
                        out string strMime,
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

                // nRet = 1;  // testing

                if (nResultValue == 0)
                {
                    // 不要发送邮件
                    continue;
                }

                bool bSendMessageError = false;

                if (nResultValue == 1 && nRedoCount == 0)   // 2008/5/27 changed 重做的时候，不再发送消息，以免消息库记录爆满
                {
                    // 发送邮件

                    // 2016/4/10
                    if (strBodyType == "mq" && this._queue != null)
                    {
                        string strRecipient = (string.IsNullOrEmpty(strRefID) ? strReaderBarcode : "!refID:" + strRefID)
                            + "@LUID:" + this.App.UID;
                        // 向 MSMQ 消息队列发送消息
                        // return:
                        //      -2  MSMQ 错误
                        //      -1  出错
                        //      0   成功
                        nRet = SendToQueue(this._queue,
                            strRecipient,
                            strMime,
                            strBody,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                        {
                            strError = "发送 MQ 出错: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(strLibraryCode,
                                "超期通知",
                                "MQ超期通知消息发送错误数",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else
                        {
                            // 2020/1/17
                            // 发送成功则记入错误日志，便于排查错误
                            this.App.WriteErrorLog($"成功发出 MQ 消息: recipient={strRecipient}, mime={strMime}, body={strBody}");

                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "超期通知",
                                "MQ超期通知人数",
                                1);
                        }
                    }

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
                            strError = "向读者 '" + strReaderBarcode + "' 发送" + external_interface.Type + " message时出错: " + strError;
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
                            strError = "发送 email 到 '" + strReaderEmailAddress + "' 出错: " + strError;
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
                    "", // 因为是机器触发，所以不记载IP地址
                    out strError);
                if (nRet == -1)
                {
                    strError = "在refresh以停代金的过程中发生错误: " + strError;
                    this.AppendResultText(strError + "\r\n");
                }

                if (nRet == 1)
                {
                    bChanged = true;
                }
            }

            // 给借阅信息和借阅历史中增加 biblioRecPath 属性
            // return:
            //      -1  出错
            //      0   读者记录没有改变
            //      1   读者记录发生改变
            nRet = AddBiblioRecPath(readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "在为读者记录添加 biblioRecPath 属性时发生错误: " + strError;
                this.AppendResultText(strError + "\r\n");
            }
            if (nRet == 1)
            {
                bChanged = true;
            }

            // 删除超过极限数量的 BorrowHistory 下级元素
            // return:
            //      -1  出错
            //      0   读者记录没有改变
            //      1   读者记录发生改变
            nRet = RemovePatronHistoryItems(readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "在为读者记录删除多余的 borrowHistoryborrow 元素时发生错误: " + strError;
                this.AppendResultText(strError + "\r\n");
            }
            if (nRet == 1)
            {
                bChanged = true;
            }

            // 2016/12/27
            // return:
            //      -1  出错
            //      0   读者记录没有改变
            //      1   读者记录发生改变
            nRet = ChangeEmailField(readerdom,
    out strError);
            if (nRet == -1)
            {
                strError = "在为读者记录修改 email 元素时发生错误: " + strError;
                this.AppendResultText(strError + "\r\n");
            }
            if (nRet == 1)
            {
                bChanged = true;
            }

            // 2016/4/10
            // 如果读者记录中没有 refID 元素，自动创建它
            // return:
            //      -1  出错
            //      0   读者记录没有改变
            //      1   读者记录发生改变
            nRet = AddRefID(readerdom,
    out strError);
            if (nRet == -1)
            {
                strError = "在为读者记录添加 refID 元素时发生错误: " + strError;
                this.AppendResultText(strError + "\r\n");
            }
            if (nRet == 1)
            {
                bChanged = true;
            }

            // 根据 library.xml 中 login/@patronPasswordExpireLength 和读者记录的 rights 元素，
            // 添加或者清除读者记录中 password/@expire 属性
            if (UpdatePasswordExpire(this.App, readerdom) == true)
                bChanged = true;

            if (bChanged)
                strReaderXml = readerdom.OuterXml;

#if NO
            // 修改读者记录后存回
            if (bChanged == true)
            {
                lRet = channel.DoSaveTextRes(strPath,
                    readerdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    baTimeStamp,
                    out output_timestamp,
                    out string strOutputPath,
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

                        // string strOutputPath = "";
                        lRet = channel.GetRes(strPath,
                            strStyle,
                            out strReaderXml,
                            out string strMetaData,
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

#endif
            return 0;
        }

        // 修改读者记录后存回
        // return:
        //      -2  保存时遇到时间戳冲突，已经重新装载读者记录返回于 strReaderXml 中，时间戳于 output_timestamp 中
        //      -1  出错
        //      0   保存成功
        int SavePatronRecord(RmsChannel channel,
            string strPath,
            ref string strReaderXml,
            byte[] baTimeStamp,
            out byte[] output_timestamp,
            out string strError)
        {
            long lRet = channel.DoSaveTextRes(strPath,
                strReaderXml,
                false,
                "content",  // ,ignorechecktimestamp
                baTimeStamp,
                out output_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // 时间戳冲突
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    string strStyle = "data,content,timestamp,outputpath";

                    lRet = channel.GetRes(strPath,
                        strStyle,
                        out strReaderXml,
                        out string strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);
                    output_timestamp = baTimeStamp;
                    if (lRet == -1)
                    {
                        strError = "写回读者库记录 '" + strPath + "' 时发生时间戳冲突，重装记录时又发生错误: " + strError;
                        return -1;
                    }

                    //nRedoCount++;
                    //goto REDO;
                    return -2;
                }

                strError = "写回读者库记录 '" + strPath + "' 时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        // 向 MSMQ 消息队列发送消息
        // parameters:
        //      strRecipient    消息最终接收者。常见的格式为 R0000001@LUID:xxxxxx 或者 !refID:xxxxxx@LUID:xxxxxx
        //                      应优先用读者记录的 refID 字段(格式为 @refID:xxxxxx)，如果没有则用 barcode 字段
        //                      如果是和微信绑定的读者，则只能从 strBody 中解析出读者记录的 email 元素内容了
        // return:
        //      -2  MSMQ 错误
        //      -1  出错
        //      0   成功
        public static int SendToQueue(MessageQueue myQueue,
            string strRecipient,
            string strMime,
            string strBody,
            out string strError)
        {
            strError = "";

            if (myQueue == null)
            {
                strError = "SendToQueue() 的 myQueue 参数值不能为 null";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            DomUtil.SetElementText(dom.DocumentElement, "type", "patronNotify");
            DomUtil.SetElementText(dom.DocumentElement, "recipient", strRecipient);
            DomUtil.SetElementText(dom.DocumentElement, "mime", strMime);
            DomUtil.SetElementText(dom.DocumentElement, "body", strBody);

            try
            {
                System.Messaging.Message myMessage = new System.Messaging.Message();
                myMessage.Body = dom.DocumentElement.OuterXml;
                myMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                myQueue.Send(myMessage);
                return 0;
            }
            catch (System.Messaging.MessageQueueException ex)
            {
                strError = "发送消息到 MQ 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -2;
            }
            catch (Exception ex)
            {
                strError = "发送消息到 MQ 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 根据 library.xml 中 login/@patronPasswordExpireLength 和读者记录的 rights 元素，
        // 添加或者清除读者记录中 password/@expire 属性
        public static bool UpdatePasswordExpire(
            LibraryApplication app,
            XmlDocument readerdom)
        {
            // 条件化的失效期，考虑了 rights 因素
            TimeSpan expireLength = app.GetConditionalPatronPasswordExpireLength(readerdom);

            XmlElement password_element = readerdom.DocumentElement.SelectSingleNode("password") as XmlElement;
            
            // 设置密码失效期
            return LibraryApplication.SetPatronPasswordExpire(password_element,
                expireLength,   // _patronPasswordExpirePeriod,
                DateTime.Now,
                out string _,
                true);
        }

        // 2016/12/27
        // 将旧形态的 email 元素内容修改为新的形态
        // return:
        //      -1  出错
        //      0   读者记录没有改变
        //      1   读者记录发生改变
        int ChangeEmailField(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string strEmail = DomUtil.GetElementText(readerdom.DocumentElement, "email");
            if (string.IsNullOrEmpty(strEmail) == false)
            {
                string[] parts = strEmail.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return 0;
                bool bChanged = false;
                List<string> results = new List<string>();
                foreach (string s in parts)
                {
                    if (s.IndexOf(":") == -1)
                    {
                        results.Add("email:" + s);
                        bChanged = true;
                    }
                    else
                        results.Add(s);
                }

                if (bChanged)
                {
                    DomUtil.SetElementText(readerdom.DocumentElement, "email", StringUtil.MakePathList(results));
                    return 1;
                }
                return 0;
            }

            return 0;
        }

        // 2016/4/10
        // 如果读者记录中没有 refID 元素，自动创建它
        // return:
        //      -1  出错
        //      0   读者记录没有改变
        //      1   读者记录发生改变
        int AddRefID(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string strRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
            {
                DomUtil.SetElementText(readerdom.DocumentElement, "refID", Guid.NewGuid().ToString());
                return 1;
            }

            return 0;
        }

        // 2015/10/9
        // 删除超过极限数量的 BorrowHistory/borrow 元素
        // return:
        //      -1  出错
        //      0   读者记录没有改变
        //      1   读者记录发生改变
        int RemovePatronHistoryItems(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrowHistory/borrow");
            if (nodes.Count > this.App.MaxPatronHistoryItems)
            {
                for (int i = this.App.MaxPatronHistoryItems; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    node.ParentNode.RemoveChild(node);
                }
                return 1;
            }

            return 0;
        }

        Hashtable recpath_table = new Hashtable();  // 册记录路径 --> 书目记录路径 对照表

        // 给借阅信息和借阅历史中增加 biblioRecPath 属性
        // return:
        //      -1  出错
        //      0   读者记录没有改变
        //      1   读者记录发生改变
        int AddBiblioRecPath(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            // 极限是一万个事项
            if (recpath_table.Count > 10000)
                recpath_table.Clear();

            bool bChanged = false;
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow | borrowHistory/borrow");
            foreach (XmlElement borrow in nodes)
            {
                string strItemRecPath = borrow.GetAttribute("recPath");
                string strBiblioRecPath = borrow.GetAttribute("biblioRecPath");
                if (string.IsNullOrEmpty(strBiblioRecPath) == true
                    && string.IsNullOrEmpty(strItemRecPath) == false)
                {
                    strBiblioRecPath = (string)recpath_table[strItemRecPath];
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strBiblioRecPath = GetBiblioRecPath(strItemRecPath);
                        if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                            recpath_table[strItemRecPath] = strBiblioRecPath;
                    }
                    if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                    {
                        borrow.SetAttribute("biblioRecPath", strBiblioRecPath);
                        bChanged = true;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(strBiblioRecPath) == false
                        && string.IsNullOrEmpty(strItemRecPath) == false)
                        recpath_table[strItemRecPath] = strBiblioRecPath;
                }
            }

            if (bChanged == true)
                return 1;
            return 0;
        }

        // 获得一个册记录从属的书目记录路径
        string GetBiblioRecPath(string strItemRecPath)
        {
            string strError = "";

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
                return null;
            string strItemXml = "";
            string strMetaData = "";
            string strOutputItemPath = "";
            byte[] item_timestamp = null;

            long lRet = channel.GetRes(strItemRecPath,
                out strItemXml,
                out strMetaData,
                out item_timestamp,
                out strOutputItemPath,
                out strError);
            if (lRet == -1)
                return null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch
            {
                return null;
            }

            string strParentID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
            if (String.IsNullOrEmpty(strParentID) == true)
                return null;

            string strBiblioRecPath = "";
            // return:
            //      -1  error
            //      1   找到
            int nRet = this.App.GetBiblioRecPathByItemRecPath(
                strItemRecPath,
                strParentID,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
                return null;
            return strBiblioRecPath;
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

            int nSegmentLength = nExtendCount + 3; // 原来是 2 // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else if (strBodyType == "mq")
            {
                index = 2;
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
                index += 3; // 原来是 2
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

            int nSegmentLength = nExtendCount + 3;  // 原来是 2 // 每个小部分的长度

            int index = -1; // 0: dpmail; 1: email; >=2: 其他扩充的消息接口方式
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else if (strBodyType == "mq")
            {
                index = 2;
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
                index += 3; // 原来是 2
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
            strText = strText.Insert(index, new string(ch, 1));
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
