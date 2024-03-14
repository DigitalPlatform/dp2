using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 正元一卡通读者信息同步 批处理任务
    /// </summary>
    public class ZhengyuanReplication : BatchTask
    {
        internal AutoResetEvent eventDownloadFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        bool DownloadCancelled = false;
        Exception DownloadException = null;

        public override void Dispose()
        {
            base.Dispose();

            eventDownloadFinished.Dispose();
        }

        // 构造函数
        public ZhengyuanReplication(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "正元一卡通读者信息同步";
            }
        }

        // 解析 开始 参数
        static int ParseZhengyuanReplicationStart(string strStart,
            out string strError)
        {
            strError = "";

            return 0;
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
        public static int ParseZhengyuanReplicationParam(string strParam,
            out bool bForceDumpAll,
            out bool bForceDumpDay,
            out bool bAutoDumpDay,
            out bool bClearFirst,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bForceDumpAll = false;
            bForceDumpDay = false;
            bAutoDumpDay = false;
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

            string strForceDumpAll = DomUtil.GetAttr(dom.DocumentElement,
                "forceDumpAll");
            if (strForceDumpAll.ToLower() == "yes"
                || strForceDumpAll.ToLower() == "true")
                bForceDumpAll = true;
            else
                bForceDumpAll = false;

            string strForceDumpDay = DomUtil.GetAttr(dom.DocumentElement,
    "forceDumpDay");
            if (strForceDumpDay.ToLower() == "yes"
                || strForceDumpDay.ToLower() == "true")
                bForceDumpDay = true;
            else
                bForceDumpDay = false;


            string strAutoDumpDay = DomUtil.GetAttr(dom.DocumentElement,
                "autoDumpDay");
            if (strAutoDumpDay.ToLower() == "yes"
                || strAutoDumpDay.ToLower() == "true")
                bAutoDumpDay = true;
            else
                bAutoDumpDay = false;


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

        public static string MakeZhengyuanReplicationParam(
            bool bForceDumpAll,
            bool bForceDumpDay,
            bool bAutoDumpDay,
            bool bClearFirst,
            bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "clearFirst",
                bClearFirst == true ? "yes" : "no");
            DomUtil.SetAttr(dom.DocumentElement, "forceDumpAll",
                bForceDumpAll == true ? "yes" : "no");

            DomUtil.SetAttr(dom.DocumentElement, "forceDumpDay",
                bForceDumpDay == true ? "yes" : "no");

            DomUtil.SetAttr(dom.DocumentElement, "autoDumpDay",
                bAutoDumpDay == true ? "yes" : "no");

            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }

#if NO
        // 本轮是不是逢上了每日启动时间(以后)?
        // parameters:
        //      strLastTime 最后一次执行过的时间 RFC1123格式
        int IsNowAfterPerDayStart(
            string strLastTime,
            out bool bRet,
            out string strError)
        {
            strError = "";

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhengyuan/dataCenter");

            if (node == null)
            {
                bRet = false;
                return 0;
            }

            string strStartTime = DomUtil.GetAttr(node, "startTime");
            if (String.IsNullOrEmpty(strStartTime) == true)
            {
                bRet = false;
                return 0;
            }

            string strHour = "";
            string strMinute = "";

            int nRet = strStartTime.IndexOf(":");
            if (nRet == -1)
            {
                strHour = strStartTime.Trim();
                strMinute = "00";
            }
            else
            {
                strHour = strStartTime.Substring(0, nRet).Trim();
                strMinute = strStartTime.Substring(nRet + 1).Trim();
            }

            int nHour = 0;
            int nMinute = 0;
            try
            {
                nHour = Convert.ToInt32(strHour);
                nMinute = Convert.ToInt32(strMinute);
            }
            catch
            {
                bRet = false;
                strError = "时间值 " + strStartTime + " 格式不正确。应为 hh:mm";
                return -1;   // 格式不正确
            }



            DateTime now1 = DateTime.Now;

            // 观察本日是否已经做过了
            if (String.IsNullOrEmpty(strLastTime) == false)
            {
                try
                {
                    DateTime lasttime = DateTimeUtil.FromRfc1123DateTimeString(strLastTime);

                    if (lasttime.Year == now1.Year
                        && lasttime.Month == now1.Month
                        && lasttime.Day == now1.Day)
                    {
                        bRet = false;   // 今天已经做过了
                        return 0;
                    }
                }
                catch
                {
                    bRet = false;
                    strError = "strLastTime " + strLastTime + " 格式错误";
                    return -1;
                }
            }

            DateTime now2 = new DateTime(now1.Year,
                now1.Month,
                now1.Day,
                nHour,
                nMinute,
                0);

            if (now1 >= now2)
                bRet = true;
            else
                bRet = false;

            return 0;
        }
#endif

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

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            int nRet = ParseZhengyuanReplicationStart(startinfo.Start,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            // 通用启动参数
            bool bForceDumpAll = false;
            bool bForceDumpDay = false;
            bool bAutoDumpDay = false;
            bool bClearFirst = false;
            bool bLoop = true;
            nRet = ParseZhengyuanReplicationParam(startinfo.Param,
                out bForceDumpAll,
                out bForceDumpDay,
                out bAutoDumpDay,
                out bClearFirst,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            if (bClearFirst == true)
            {
                // 删除读者库中所有没有借阅信息的读者信息？
            }

            if (bForceDumpAll == true)
            {
                // 更新卡户信息完整表(AccountsCompleteInfo_yyyymmdd.xml)
                string strDataFileName = "AccountsCompleteInfo_" + GetCurrentDate() + ".xml";
                string strLocalFilePath = PathUtil.MergePath(this.App.ZhengyuanDir, strDataFileName);

                try
                {
                    // return:
                    //      -1  出错
                    //      0   正常结束
                    //      1   被用户中断
                    nRet = DownloadDataFile(strDataFileName,
                        strLocalFilePath,
                        out strError);
                    if (nRet == -1)
                    {
                        string strErrorText = "下载数据文件" + strDataFileName + "失败: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        return;
                    }
                    if (nRet == 1)
                    {
                        this.AppendResultText("下载数据文件" + strDataFileName + "被中断\r\n");
                        this.Loop = false;
                        return;
                    }

                    // 把数据文件写入有映射关系的读者库
                    this.AppendResultText("同步数据文件 " + strDataFileName + " 开始\r\n");

                    // return:
                    //      -1  error
                    //      0   succeed
                    //      1   中断
                    nRet = WriteToReaderDb(strLocalFilePath,
                        out strError);
                    if (nRet == -1)
                    {
                        string strErrorText = "文件 " + strDataFileName + " 写入读者库: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        return;
                    }
                    else if (nRet == 1)
                    {
                        this.AppendResultText("同步数据文件 " + strDataFileName + "被中断\r\n");
                        return;
                    }
                    else
                    {
                        this.AppendResultText("同步数据文件 " + strDataFileName + "完成\r\n");

                        bForceDumpAll = false;
                        startinfo.Param = MakeZhengyuanReplicationParam(
                            bForceDumpAll,
                            bForceDumpDay,
                            bAutoDumpDay,
                            bClearFirst,
                            bLoop);

                    }

                }
                finally
                {
                    // 删除用过的数据文件? 还是保留用作调试观察?
                    File.Delete(strLocalFilePath);
                }
            }

            string strMonitorName = "zhengyuanReplication";

            if (bAutoDumpDay == true || bForceDumpDay == true)
            {

                string strLastTime = "";


                if (bForceDumpDay == false)
                {
                    Debug.Assert(bAutoDumpDay == true, ""); // 二者必有一个==true
                    nRet = ReadLastTime(
                        strMonitorName,
                        out strLastTime,
                        out strError);
                    if (nRet == -1)
                    {
                        string strErrorText = "从文件中获取每日启动时间时发生错误: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        return;
                    }

                    string strStartTimeDef = "";
                    bool bRet = false;
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
                        string strErrorText = "获取每日启动时间时发生错误: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        if (nRet == -2)
                        {
                            WriteLastTime(strMonitorName, "");
                        }
                        return;
                    }

                    if (bRet == false)
                        return; // 还没有到每日时间
                }


                // 更新卡户信息基本(每日)表(AccountsCompleteInfo_yyyymmdd.xml)
                string strDataFileName = "AccountsBasicInfo_" + GetCurrentDate() + ".xml";
                string strLocalFilePath = PathUtil.MergePath(this.App.ZhengyuanDir, strDataFileName);

                try
                {
                    // return:
                    //      -1  出错
                    //      0   正常结束
                    //      1   被用户中断
                    nRet = DownloadDataFile(strDataFileName,
                        strLocalFilePath,
                        out strError);
                    if (nRet == -1)
                    {
                        string strErrorText = "下载数据文件" + strDataFileName + "失败: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        return;
                    }
                    if (nRet == 1)
                    {
                        this.AppendResultText("下载数据文件" + strDataFileName + "被中断\r\n");
                        this.Loop = false;
                        return;
                    }

                    // 把数据文件写入有映射关系的读者库
                    this.AppendResultText("同步数据文件 " + strDataFileName + " 开始\r\n");

                    // return:
                    //      -1  error
                    //      0   succeed
                    //      1   中断
                    nRet = WriteToReaderDb(strLocalFilePath,
                        out strError);
                    if (nRet == -1)
                    {
                        string strErrorText = "文件 " + strDataFileName + " 写入读者库: " + strError;
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        return;
                    }
                    else if (nRet == 1)
                    {
                        this.AppendResultText("同步数据文件 " + strDataFileName + "被中断\r\n");
                        return;
                    }
                    else
                    {
                        this.AppendResultText("同步数据文件 " + strDataFileName + "完成\r\n");

                        Debug.Assert(this.App != null, "");

                        // 写入文件，记忆已经做过的当日时间
                        strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime()); // 2007/12/17 changed // DateTime.UtcNow
                        WriteLastTime(strMonitorName, strLastTime);

                        if (bForceDumpDay == true)
                        {

                            bForceDumpDay = false;
                            startinfo.Param = MakeZhengyuanReplicationParam(
                                bForceDumpAll,
                                bForceDumpDay,
                                bAutoDumpDay,
                                bClearFirst,
                                bLoop);
                        }

                    }

                }
                finally
                {
                    // 删除用过的数据文件? 还是保留用作调试观察?
                    File.Delete(strLocalFilePath);
                }
            }

        }

#if NO
        // 读取上次最后处理的时间
        int ReadLastTime(out string strLastTime,
            out string strError)
        {
            strError = "";
            strLastTime = "";

            string strFileName = PathUtil.MergePath(this.App.ZhengyuanDir, "lasttime.txt");

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(strFileName, Encoding.UTF8);
            }
            catch (FileNotFoundException /*ex*/)
            {
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "open file '" + strFileName + "' error : " + ex.Message;
                return -1;
            }
            try
            {
                strLastTime = sr.ReadLine();  // 读入时间行
            }
            finally
            {
                sr.Close();
            }

            return 1;
        }

        // 写入断点记忆文件
        public void WriteLastTime(string strLastTime)
        {
            string strFileName = PathUtil.MergePath(this.App.ZhengyuanDir, "lasttime.txt");

            // 删除原来的文件
            File.Delete(strFileName);

            // 写入新内容
            StreamUtil.WriteText(strFileName,
                strLastTime);
        }
#endif

        // 将更新卡户信息完整表(AccountsCompleteInfo_yyyymmdd.xml)写入读者库
        // return:
        //      -1  error
        //      0   succeed
        //      1   中断
        int WriteToReaderDb(string strLocalFilePath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhengyuan/replication");

            if (node == null)
            {
                strError = "尚未配置<zhangyuan><replication>参数";
                return -1;
            }

            string strMapDbName = DomUtil.GetAttr(node, "mapDbName");
            if (String.IsNullOrEmpty(strMapDbName) == true)
            {
                strError = "尚未配置<zhangyuan/replication>元素的mapDbName属性";
                return -1;
            }

            using (Stream file = File.Open(strLocalFilePath,
                FileMode.Open,
                FileAccess.Read))
            {

                if (file.Length == 0)
                    return 0;

                using (XmlTextReader reader = new XmlTextReader(file))
                {
                    bool bRet = false;

                    // 临时的SessionInfo对象
                    SessionInfo sessioninfo = new SessionInfo(this.App);

                    // 模拟一个账户
                    Account account = new Account();
                    account.LoginName = "replication";
                    account.Password = "";
                    account.Rights = "setreaderinfo";

                    account.Type = "";
                    account.Barcode = "";
                    account.Name = "replication";
                    account.UserID = "replication";
                    account.RmsUserName = this.App.ManagerUserName;
                    account.RmsPassword = this.App.ManagerPassword;

                    sessioninfo.Account = account;

                    // TODO: 要释放 sessioninfo

                    // 找到根
                    while (true)
                    {
                        try
                        {
                            bRet = reader.Read();
                        }
                        catch (Exception ex)
                        {
                            strError = "读XML文件发生错误: " + ex.Message;
                            return -1;
                        }

                        if (bRet == false)
                        {
                            strError = "没有根元素";
                            return -1;
                        }
                        if (reader.NodeType == XmlNodeType.Element)
                            break;
                    }

                    for (int i = 0; ; i++)
                    {
                        if (this.Stopped == true)
                            return 1;

                        bool bEnd = false;
                        // 第二级元素
                        while (true)
                        {
                            bRet = reader.Read();
                            if (bRet == false)
                            {
                                bEnd = true;  // 结束
                                break;
                            }
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        if (bEnd == true)
                            break;

                        this.AppendResultText("处理 " + (i + 1).ToString() + "\r\n");

                        // 记录体
                        string strXml = reader.ReadOuterXml();

                        // return:
                        //      -1  error
                        //      0   已经写入
                        //      1   没有必要写入
                        nRet = WriteOneReaderInfo(
                            sessioninfo,
                            strMapDbName,
                            strXml,
                            out strError);
                        if (nRet == -1)
                            return -1;

                    }

                    return 0;
                }
            }
        }

        // 写入一条数据
        // return:
        //      -1  error
        //      0   已经写入
        //      1   没有必要写入
        int WriteOneReaderInfo(
            SessionInfo sessioninfo,
            string strReaderDbName,
            string strZhengyuanXml,
            out string strError)
        {
            strError = "";

            XmlDocument zhengyuandom = new XmlDocument();

            try
            {
                zhengyuandom.LoadXml(strZhengyuanXml);
            }
            catch (Exception ex)
            {
                strError = "从正元数据中读出的XML片段装入DOM失败: " + ex.Message;
                return -1;
            }

            // AccType
            // 卡户类型
            // 1正式卡,2 临时卡
            string strAccType = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCTYPE");
            if (strAccType != "1")
            {
                return 1;
            }

            string strBarcode = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCNUM");
            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "缺乏<ACCNUM>元素";
                return -1;
            }

            strBarcode = strBarcode.PadLeft(10, '0');

            int nRet = 0;
            string strReaderXml = "";
            string strOutputPath = "";
            byte[] baTimestamp = null;

            // 加读锁
            // 可以避免拿到读者记录处理中途的临时状态
#if DEBUG_LOCK_READER
            this.App.WriteErrorLog("WriteOneReaderInfo 开始为读者加读锁 '" + strBarcode + "'");
#endif
            this.App.ReaderLocks.LockForRead(strBarcode);

            try
            {

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXml(
                    // this.RmsChannels, // sessioninfo.Channels,
                    channel,
                    strBarcode,
                    out strReaderXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);

            }
            finally
            {
                this.App.ReaderLocks.UnlockForRead(strBarcode);
#if DEBUG_LOCK_READER
                this.App.WriteErrorLog("WriteOneReaderInfo 结束为读者加读锁 '" + strBarcode + "'");
#endif
            }

            if (nRet == -1)
                return -1;
            if (nRet > 1)
            {
                strError = "条码号 " + strBarcode + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                return -1;
            }

            string strAction = "";
            string strRecPath = "";

            string strNewXml = "";  // 修改后的记录

            if (nRet == 0)
            {
                // 没有命中，创建新记录
                strAction = "new";
                strRecPath = strReaderDbName + "/?";
                strReaderXml = "";  // 2009/7/17 changed // "<root />";
            }
            else
            {
                Debug.Assert(nRet == 1, "");
                // 命中，修改后覆盖原记录

                strAction = "change";
                strRecPath = strOutputPath;
            }

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                return -1;
            }

            nRet = ModifyReaderRecord(ref readerdom,
                zhengyuandom,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)  // 没有必要写入
            {
                Debug.Assert(strAction == "change", "");
                return 1;
            }

            if (nRet == 2) // 没有必要写入
            {
                return 1;
            }

            strNewXml = readerdom.OuterXml;

            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            LibraryServerResult result = this.App.SetReaderInfo(
                    sessioninfo,
                    strAction,
                    strRecPath,
                    strNewXml,
                    strReaderXml,
                    baTimestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedRecPath,
                    out baNewTimestamp,
                    out kernel_errorcode);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                return -1;
            }



            return 0;   // 正常写入了
        }

        /*
        public static int Date8toRfc1123(string strOrigin,
out string strTarget,
out string strError)
        {
            strError = "";
            strTarget = "";

            strOrigin = strOrigin.Replace("-", "");

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

        // 根据正元数据修改或者创建记录
        // return:
        //      -1  error
        //      0   正常
        //      1   来自正元的信息readerdom中已经有了，和zhengyuandom中即将写入的一模一样
        //      2   没有必要写入的信息
        int ModifyReaderRecord(ref XmlDocument readerdom,
            XmlDocument zhengyuandom,
            out string strError)
        {
            strError = "";
            int nRet = 0;


            /*
    <Person>
        <ACCNUM>100</ACCNUM>
        <CARDID>3163593110</CARDID>
        <CARDCODE>1</CARDCODE>
        <ACCSTATUS>1</ACCSTATUS>
        <ACCTYPE>1</ACCTYPE>
        <PERCODE>a001</PERCODE>
        <AREANUM>1</AREANUM>
        <ACCNAME>卡户00100</ACCNAME>
        <DEPNUM>2</DEPNUM>
        <DEPNAME>人文学院</DEPNAME>
        <CLSNUM>2</CLSNUM>
        <CLSNAME>计划外本科</CLSNAME>
        <ACCSEX>0 </ACCSEX>
        <POSTDATE>2005-08-15</POSTDATE>
        <LOSTDATE>2009-08-14</LOSTDATE>
    </Person>
             * */

            // 看看来自正元的信息和记录中原有的正元信息是否一样，
            // 如果一样就不必保存了。

            XmlNode oldnode = readerdom.DocumentElement.SelectSingleNode("zhengyuan");
            if (oldnode != null)
            {
                if (oldnode.InnerXml == zhengyuandom.InnerXml)
                    return 1;   // 来自正元的信息已经有了，和即将写入的一模一样
            }

            // AccType
            // 卡户类型
            // 1正式卡,2 临时卡
            string strAccType = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCTYPE");
            if (strAccType != "1")
            {
                return 2;
            }


            // ACCNUM 帐号
            string strBarcode = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCNUM");

            // 确保为10位
            strBarcode = strBarcode.PadLeft(10, '0');

            DomUtil.SetElementText(readerdom.DocumentElement,
                "barcode",
                strBarcode);


            // AccStatus
            // 卡户状态
            // 0:已撤户,1:有效卡,2:挂失卡,3:冻结卡,4:预撤户
            string strAccStatus = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCSTATUS");
            // 修改读者状态
            if (strAccStatus != "1")
            {
                string strState = GetAccStatusString(strAccStatus);
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "state",
                    strState);
            }
            else
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "state",
                    "");    // 正常状态
            }



            // AccName
            // 卡户姓名
            // 8个汉字
            string strAccName = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCNAME");
            DomUtil.SetElementText(readerdom.DocumentElement,
                "name",
                strAccName);


            // DepName
            // 部门名称
            string strDepName = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "DEPNAME");
            DomUtil.SetElementText(readerdom.DocumentElement,
                "department",
                strDepName);

            // AccSex
            // 卡户性别
            // 男/女/""
            string strAccSex = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "ACCSEX");
            DomUtil.SetElementText(readerdom.DocumentElement,
                "gender",
                strAccSex);

            // mobileCode
            // 手机号码
            string strMobileCode = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "MOBILECODE");
            DomUtil.SetElementText(readerdom.DocumentElement,
                "tel",
                strMobileCode);

            // EMail
            // email地址
            string strEmail = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "EMAIL");
            if (string.IsNullOrEmpty(strEmail) == false)
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "email",
                    "email:" + strEmail);
            }

            string strRfcTime = "";

            // PostDate
            // 配卡日期
            string strPostDate = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "POSTDATE");

            if (String.IsNullOrEmpty(strPostDate) == false)
            {
                //  8位日期格式转换为GMT时间
                nRet = DateTimeUtil.Date8toRfc1123(strPostDate,
                    out strRfcTime,
                    out strError);
                if (nRet == -1)
                {
                    strError = "<POSTDATE>中的日期值 '" + strPostDate + "' 格式不正确: " + strError;
                    return -1;
                }
            }
            else
            {
                strRfcTime = "";
            }
            DomUtil.SetElementText(readerdom.DocumentElement,
                "createDate",
                strRfcTime);

            // LostDate
            // 失效日期
            string strLostDate = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "LOSTDATE");

            if (String.IsNullOrEmpty(strLostDate) == false)
            {
                //  8位日期格式转换为GMT时间
                nRet = DateTimeUtil.Date8toRfc1123(strLostDate,
                    out strRfcTime,
                    out strError);
                if (nRet == -1)
                {
                    strError = "<LOSTDATE>中的日期值 '" + strLostDate + "' 格式不正确: " + strError;
                    return -1;
                }
            }
            else
            {
                strRfcTime = "";
            }

            DomUtil.SetElementText(readerdom.DocumentElement,
                "expireDate",
                strRfcTime);


            // BirthDay
            // 生日
            string strBirthDay = DomUtil.GetElementText(zhengyuandom.DocumentElement,
                "BIRTHDAY");
            if (String.IsNullOrEmpty(strBirthDay) == false)
            {
                //  8位日期格式转换为GMT时间
                nRet = DateTimeUtil.Date8toRfc1123(strBirthDay,
                    out strRfcTime,
                    out strError);
                if (nRet == -1)
                {
                    strError = "<BIRTHDAY>中的日期值 '" + strBirthDay + "' 格式不正确: " + strError;
                    return -1;
                }
            }
            else
            {
                strRfcTime = "";
            }

            if (String.IsNullOrEmpty(strRfcTime) == false)
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "dateOfBirth",  // birthday
                    strRfcTime);
            }

            if (oldnode == null)
            {
                // 全部保存正元的数据
                oldnode = readerdom.CreateElement("zhengyuan");
                readerdom.DocumentElement.AppendChild(oldnode);
            }

            oldnode.InnerXml = zhengyuandom.DocumentElement.InnerXml;
            DomUtil.SetAttr(oldnode, "lastModified", DateTime.Now.ToString());  // 记载最后修改时间

            return 0;
        }

        static string GetAccStatusString(string strAccStatus)
        {
            if (strAccStatus == "0")
                return "已撤户";
            if (strAccStatus == "1")
                return "有效卡";
            if (strAccStatus == "2")
                return "挂失卡";
            if (strAccStatus == "3")
                return "冻结卡";
            if (strAccStatus == "4")
                return "预撤户";
            return strAccStatus;    // 不是预定义的值
        }

        static string GetCurrentDate()
        {
            DateTime now = DateTime.Now;

            return now.Year.ToString().PadLeft(4, '0')
            + now.Month.ToString().PadLeft(2, '0')
            + now.Day.ToString().PadLeft(2, '0');
        }

        // 获得数据中心配置参数
        int GetDataCenterParam(
            out string strServerUrl,
            out string strUserName,
            out string strPassword,
            out string strError)
        {
            strError = "";
            strServerUrl =
            strUserName = "";
            strPassword = "";

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhengyuan/dataCenter");

            if (node == null)
            {
                strError = "尚未配置<zhangyuan/dataCenter>元素";
                return -1;
            }

            strServerUrl = DomUtil.GetAttr(node, "url");
            strUserName = DomUtil.GetAttr(node, "username");
            strPassword = DomUtil.GetAttr(node, "password");

            return 0;
        }

        // 下载数据文件
        // parameters:
        //      strDataFileName 数据文件名。纯粹的文件名。
        //      strLocalFilePath    本地文件名
        // return:
        //      -1  出错
        //      0   正常结束
        //      1   被用户中断
        int DownloadDataFile(string strDataFileName,
            string strLocalFilePath,
            out string strError)
        {
            strError = "";

            string strServerUrl = "";
            string strUserName = "";
            string strPassword = "";

            // 获得数据中心配置参数
            int nRet = GetDataCenterParam(
                out strServerUrl,
                out strUserName,
                out strPassword,
                out strError);
            if (nRet == -1)
                return -1;

            string strPath = strServerUrl + "/" + strDataFileName;

            Uri serverUri = new Uri(strPath);

            /*
            // The serverUri parameter should start with the ftp:// scheme.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
            }
             * */


            // Get the object used to communicate with the server.
            WebClient request = new WebClient();

            this.DownloadException = null;
            this.DownloadCancelled = false;
            this.eventDownloadFinished.Reset();

            request.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(request_DownloadFileCompleted);
            request.DownloadProgressChanged += new DownloadProgressChangedEventHandler(request_DownloadProgressChanged);

            request.Credentials = new NetworkCredential(strUserName,
                strPassword);

            try
            {

                File.Delete(strLocalFilePath);

                request.DownloadFileAsync(serverUri,
                    strLocalFilePath);
            }
            catch (WebException ex)
            {
                strError = "下载数据文件 " + strPath + " 失败: " + ex.ToString();
                return -1;
            }

            // 等待下载结束

            WaitHandle[] events = new WaitHandle[2];

            events[0] = this.eventClose;
            events[1] = this.eventDownloadFinished;

            while (true)
            {
                if (this.Stopped == true)
                {
                    request.CancelAsync();
                }

                int index = WaitHandle.WaitAny(events, 1000, false);    // 每秒超时一次

                if (index == WaitHandle.WaitTimeout)
                {
                    // 超时
                }
                else if (index == 0)
                {
                    strError = "下载被关闭信号提前中断";
                    return -1;
                }
                else
                {
                    // 得到结束信号
                    break;
                }
            }

            if (this.DownloadCancelled == true)
                return 1;   // 被用户中断

            if (this.DownloadException != null)
            {
                strError = this.DownloadException.Message;
                if (this.DownloadException is WebException)
                {
                    WebException webex = (WebException)this.DownloadException;
                    if (webex.Response is FtpWebResponse)
                    {
                        FtpWebResponse ftpr = (FtpWebResponse)webex.Response;
                        if (ftpr.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            return -1;
                        }
                    }

                }
                return -1;
            }

            return 0;
        }

        void request_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if ((e.BytesReceived % 1024 * 100) == 0)
                this.AppendResultText("已下载: " + e.BytesReceived + "\r\n");
        }

        void request_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadException = e.Error;
            this.DownloadCancelled = e.Cancelled;
            this.eventDownloadFinished.Set();
        }

    }
}
