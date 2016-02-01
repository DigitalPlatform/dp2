using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Web;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 读者库数据同步 批处理任务
    /// 从卡中心同步读者数据
    /// </summary>
    public class PatronReplication : BatchTask
    {
        // 构造函数
        public PatronReplication(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;

            // 调试
            // this.PerTime = 5 * 60 * 1000;	// 5分钟
        }

        public override string DefaultName
        {
            get
            {
                return "读者信息同步";
            }
        }

#if NO
        // 解析 开始 参数
        // parameters:
        //      strStart    启动字符串。格式为XML
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParsePatronReplicationStart(string strStart,
            out string strRecordID,
            out string strError)
        {
            strError = "";
            strRecordID = "";

            // int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                // strError = "启动参数不能为空";
                // return -1;
                strRecordID = "1";
                return 0;
            }

            if (strStart == "!breakpoint")
            {

                strRecordID = strStart;
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strStart);
            }
            catch (Exception ex)
            {
                strError = "装载XML字符串 '" + strStart + "'进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode nodeLoop = dom.DocumentElement.SelectSingleNode("loop");
            if (nodeLoop != null)
            {
                strRecordID = DomUtil.GetAttr(nodeLoop, "recordid");
            }

            return 0;
        }

        // 解析通用启动参数
        // 格式
        /*
         * <root loop='...'/>
         * loop缺省为true
         * 
         * */
        public static int ParsePatronReplicationParam(string strParam,
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
                strError = "strParam参数 '" + strParam + "' 装入XML DOM时出错: " + ex.Message;
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
#endif

        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            if (DateTime.Now > new DateTime(2015,12,31) // 2016/1/1 以后可删除此语句
                && StringUtil.IsInList("patronReplication", this.App.Function) == false)
            {
                string strErrorText = "读者同步功能需要设置序列号才能运行";
                this.AppendResultText(strErrorText + "\r\n");
                // this.App.WriteErrorLog(strErrorText);
                return;
            }

            string strError = "";
            int nRet = 0;

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 获取配置参数
            // return:
            //      -1  出错
            //      0   尚未配置<patronReplication>参数
            //      1   成功
            nRet = GetConfigParameters(out strError);
            if (nRet == -1)
            {
                string strErrorText = "获取配置参数时出错: " + strError;
                this.AppendResultText(strErrorText + "\r\n");
                this.App.WriteErrorLog(strErrorText);
                return;
            }
            if (nRet == 0)
            {
                this.AppendResultText("library.xml中尚未配置<patronReplication>参数，读者信息同步任务没有被执行\r\n");
                return;
            }
#if NO
            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 通用启动参数
            bool bLoop = true;
            int nRet = ParsePatronReplicationParam(startinfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            string strID = "";
            nRet = ParsePatronReplicationStart(startinfo.Start,
                out strID,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                this.Loop = false;
                return;
            }


            if (strID == "!breakpoint")
            {
                string strLastNumber = "";
                bool bTempLoop = false;

                nRet = ReadLastNumber(
                    out bTempLoop,
                    out strLastNumber,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从断点文件中获取最大号码时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                strID = strLastNumber;

                if (string.IsNullOrEmpty(strLastNumber) == false)
                {
                    this.AppendResultText("从断点 "+strLastNumber+" 开始处理\r\n");
                }
            }
#endif
            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "patronReplication";
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
                if (nRet == 0 )
                {

                }
                else if (nRet == 1 && startinfo.Start == "activate")
                {
                    // 2015/8/4
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

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + this.Name + " 启动。");
            }

            this.AppendResultText("开始新一轮循环\r\n");
            int nTotalRecCount = 0;

            try
            {
                // 把数据文件写入有映射关系的读者库
                this.AppendResultText("*** 同步读者数据任务开始\r\n");

                string strXmlFilename = PathUtil.MergePath(this.App.PatronReplicationDir, "data.xml");

                // 从卡中心获取全部记录，写入一个XML文件
                nRet = GetAllRecordsFromCardCenter(
                    strXmlFilename,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从卡中心获取数据时出错: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                this.AppendResultText("检索读者库中现有的记录集合\r\n");

                List<string> current_ids = null;
                // 检索出读者库中全部ids
                // 通过特定检索途径获得读者记录
                // return:
                //      -1  error
                //      >=0 命中个数
                nRet = SearchAllIds(
                    this.RmsChannels,
                    this.PatronDbName,
                    this.From,
                    out current_ids,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "检索数据库 " + this.PatronDbName + " 中所有 " + this.From + " 的keys时出错: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                this.AppendResultText("刷新读者数据开始\r\n");

                List<string> ids = null;
                string strMaxNumber = "";   // 返回操作末尾的最大号
                bool bComplete = false;
                try
                {
                    // return:
                    //      -1  error
                    //      0   succeed
                    //      1   中断
                    nRet = WriteToReaderDb(
                        strXmlFilename,
                        "", // strID,
                        out strMaxNumber,
                        out ids,
                        out strError);
                    if (nRet == 0)
                        bComplete = true;
                }
                finally
                {
                    if (bComplete == false)
                    {
#if NO
                        // 写入文件，记忆已经做过的最大号码
                        // 要用bLoop，这是来自启动面板的值；不能用this.Loop 因为中断时其值已经被改变
                        if (String.IsNullOrEmpty(strMaxNumber) == true)
                        {
                            // 如果运行出错或者根本没有新源记录，连一条也没有成功作过，就保持原来的断点记录号
                            // 如果写入的断点记录号是空，下次运行的时候，将从'1'开始。这一般是不能接受的
                            WriteLastNumber(bLoop, strID);
                        }
                        else
                            WriteLastNumber(bLoop, strMaxNumber);
#endif
                    }
                    else
                    {
#if NO
                        WriteLastNumber(bLoop, ""); // 本次已经完成，下次从头开始
#endif

                        File.Delete(strXmlFilename);
                    }
                }

                if (nRet == -1)
                {
                    string strErrorText = "写入读者库时出错: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                else if (nRet == 1)
                {
                    this.AppendResultText("刷新读者数据被中断\r\n");
                    return;
                }
                else
                {
                    this.AppendResultText("刷新读者数据完成，共处理读者记录 "+ids.Count.ToString()+" 条\r\n");
                    Debug.Assert(this.App != null, "");
                }

                nTotalRecCount += ids.Count;

                // current_ids 和 ids 进行交叉运算
                this.AppendResultText("排序归并ID\r\n");
                current_ids.Sort();
                StringUtil.RemoveDup(ref current_ids);
                ids.Sort();
                StringUtil.RemoveDup(ref ids);

                List<string> targetLeft = new List<string>();
                List<string> targetMiddle = null;
                List<string> targetRight = null;
                string strDebugInfo = "";

                this.AppendResultText("逻辑运算ID\r\n");

                // 对两个已经排序的List进行逻辑运算
                // 注：sourceLeft和sourceRight在调用前应当已经排序，从小到大的方向
                // parameters:
                //		strLogicOper	运算风格 OR , AND , SUB
                //		sourceLeft	源左边结果集
                //		sourceRight	源右边结果集
                //		targetLeft	目标左边结果集
                //		targetMiddle	目标中间结果集
                //		targetRight	目标右边结果集
                //		bOutputDebugInfo	是否输出处理信息
                //		strDebugInfo	处理信息
                // return
                //		-1	出错
                //		0	成功
                nRet = StringUtil.LogicOper("SUB",
                    current_ids,
                    ids,
                    ref targetLeft,
                    ref targetMiddle,
                    ref targetRight,
                    false,
                    out strDebugInfo,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "逻辑运算集合时出错: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                this.AppendResultText("开始标记删除读者记录\r\n");

                // 从current_ids中去掉和ids重复的部分，剩下的需要作标记删除处理
                nRet = MaskDeleteRecords(
            targetLeft,
            out strError);
                if (nRet == -1)
                {
                    string strErrorText = "标记删除读者记录时出错: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                else if (nRet == 1)
                {
                    this.AppendResultText("标记删除读者记录被中断\r\n");
                    return;
                }
                else
                {
                    this.AppendResultText("标记删除读者记录完成，共处理记录 " + targetLeft.Count.ToString()+ " 条\r\n");
                    this.AppendResultText("*** 同步读者数据任务完成\r\n");

                    nTotalRecCount += targetLeft.Count;

                    Debug.Assert(this.App != null, "");
                }
            }
            finally
            {
#if NO
                this.StartInfo.Start = "!breakpoint"; // 自动循环的时候，没有号码，要从断点文件中取得
#endif
            }

            AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");

            // 2015/10/3
            // 让前端激活的任务，只执行一次。如果配置了每日激活时间，后面要再执行，除非是每日激活时间已到
            if (startinfo.Start == "activate")
                startinfo.Start = "";

            {
                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nTotalRecCount.ToString() + " 个。";
                this.App.WriteErrorLog(strErrorText);
            }

            return;
        ERROR1:
            AppendResultText("PatronReplication thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("PatronReplication thread error : " + strError + "\r\n");
            return;
        }

        IChannel m_cardCenterChannel = null;    // new IpcClientChannel();
        ICardCenter m_cardCenterObj = null;

        int StartChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            Uri uri = new Uri(strUrl);
            string strScheme = uri.Scheme.ToLower();

            if (strScheme == "ipc")
                m_cardCenterChannel = new IpcClientChannel();
            else if (strScheme == "tcp")
                m_cardCenterChannel = new TcpClientChannel();
            else if (strScheme == "http")
                m_cardCenterChannel = new HttpClientChannel();
            else
            {
                strError = "URL '"+strUrl+"' 中包含了无法识别的Scheme '" + strScheme + "'。只能使用 ipc tcp http 之一";
                return -1;
            }

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(m_cardCenterChannel, false);

            try
            {
                this.m_cardCenterObj = (ICardCenter)Activator.GetObject(typeof(ICardCenter),
                    strUrl);
                if (this.m_cardCenterObj == null)
                {
                    strError = "无法连接到 Remoting 服务器 " + strUrl;
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndChannel()
        {
            ChannelServices.UnregisterChannel(this.m_cardCenterChannel);
        }

        // 从卡中心获取全部记录，写入一个XML文件
        int GetAllRecordsFromCardCenter(
            string strXmlFilename,
            out string strError)
        {
            strError = "";

            Debug.Assert(string.IsNullOrEmpty(this.InterfaceUrl) == false, "");

            XmlTextWriter writer = null;

            try
            {
                writer = new XmlTextWriter(strXmlFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "创建文件 '" + strXmlFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            using (writer)
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                int nRet = StartChannel(
                    this.InterfaceUrl,
                    out strError);
                if (nRet == -1)
                    return -1;

                Debug.Assert(this.m_cardCenterObj != null, "");

                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("collection");

                    string strPosition = "";

                    for (; ; )
                    {
#if NO
                        // 2012/2/4
                        // 系统挂起的时候，不运行本线程
                        if (this.App.HangupReason == HangupReason.LogRecover)
                        {
                            strError = "因为系统挂起或暂停批处理而中断处理";
                            return -1;
                        }
#endif
                        if (this.Stopped == true)
                        {
                            strError = "中断处理";
                            return -1;
                        }

                        string[] records = null;
                        // 获得若干读者记录
                        // parameters:
                        //      strPosition 第一次调用前，需要将此参数的值清为空
                        //      records 读者XML记录字符串数组。注：读者记录中的某些字段卡中心可能缺乏对应字段，那么需要在XML记录中填入 <元素名 dprms:missing />，这样不至于造成同步时图书馆读者库中的这些字段被清除。至于读者借阅信息等字段，则不必操心
                        // return:
                        //      -1  出错
                        //      0   正常获得一批记录，但是尚未获得全部
                        //      1   正常获得最后一批记录
                        nRet = m_cardCenterObj.GetPatronRecords(ref strPosition,
                            out records,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (records != null)
                        {
                            int i = 0;
                            foreach (string strXml in records)
                            {
                                // 2012/11/2
                                if (string.IsNullOrEmpty(strXml) == true)
                                {
                                    strError = "ICardCenter.GetPatronRecords()方法(本次调用后 strPosition = '"+strPosition+"')所获得的 "+records.Length.ToString()+" 个字符串中, 第 "+i.ToString()+"个(从0开始计算)的值为空";
                                    return -1;
                                }
                                XmlDocument dom = new XmlDocument();
                                try
                                {
                                    dom.LoadXml(strXml);
                                }
                                catch (Exception ex)
                                {
                                    strError = "ICardCenter.GetPatronRecords()方法(本次调用后 strPosition = '" + strPosition + "')所获得的 " + records.Length.ToString() + " 个字符串中, 第 " + i.ToString() + "个(从0开始计算)的值装入DOM时发生错误:" + ex.Message;
                                    return -1;
                                }
                                dom.DocumentElement.WriteTo(writer);
                                i++;
                            }
                        }

                        if (nRet == 1)
                            break;
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                catch (Exception ex)
                {
                    strError = "从卡中心获取数据时发生错误: " + ex.Message;
                    return -1;
                }
                finally
                {
                    EndChannel();
                }
            }

            return 0;
        }

        // 根据元素名得到检索途径名
        static string GetFromName(string strIdElementName)
        {
            if (strIdElementName == "barcode")
                return "证条码";
            // 2012/11/12
            if (strIdElementName == "cardNumber")
                return "证号";

            return null;
        }

        // 获取配置参数
        // return:
        //      -1  出错
        //      0   尚未配置<patronReplication>参数
        //      1   成功
        int GetConfigParameters(out string strError)
        {
            strError = "";
            /*
<patronReplication 
interfaceUrl="ipc://CardCenterChannel/CardCenterServer" 
patronDbName="读者库"
idElementName="barcode"
/>
*/
            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//patronReplication");
            if (node == null)
            {
                strError = "尚未配置<patronReplication>参数";
                return 0;
            }

            this.InterfaceUrl = DomUtil.GetAttr(node, "interfaceUrl");
            if (string.IsNullOrEmpty(this.InterfaceUrl) == true)
            {
                strError = "<patronReplication> 元素内尚未配置 interfaceUrl 属性";
                return -1;
            }

            this.PatronDbName = DomUtil.GetAttr(node, "patronDbName");
            if (string.IsNullOrEmpty(this.PatronDbName) == true)
            {
                strError = "<patronReplication> 元素内尚未配置 patronDbName 属性";
                return -1;
            }
            this.IdElementName = DomUtil.GetAttr(node, "idElementName");
            if (string.IsNullOrEmpty(this.IdElementName) == true)
            {
                // strError = "<patronReplication> 元素内尚未配置 idElementName 属性";
                // return -1;
                this.IdElementName = "barcode";
            }

            this.From = GetFromName(this.IdElementName);
            if (string.IsNullOrEmpty(this.From) == true)
            {
                strError = "对于元素名 '" + this.IdElementName + "' 无法获得对应的检索点名定义";
                return -1;
            }

            this.ModifyOtherDbRecords = DomUtil.GetBooleanParam(node,
                "modifyOtherDbRecords",
                false);

            // 验证this.PatronDbName
            if (this.App.IsReaderDbName(this.PatronDbName) == false)
            {
                strError = "<patronReplication> 元素内 patronDbName 属性值 '"+this.PatronDbName+"' 并不是一个合法的读者库名";
                return -1;
            }

            return 1;
        }

        SessionInfo GetTempSessionInfo()
        {
            if (this.m_tempSessionInfo != null)
                return this.m_tempSessionInfo;

            // 临时的SessionInfo对象
            SessionInfo sessioninfo = new SessionInfo(this.App);

            // 模拟一个账户
            Account account = new Account();
            account.LoginName = "replication";
            account.Password = "";
            account.Rights = "setreaderinfo,devolvereaderinfo";

            account.Type = "";
            account.Barcode = "";
            account.Name = "replication";
            account.UserID = "replication";
            account.RmsUserName = this.App.ManagerUserName;
            account.RmsPassword = this.App.ManagerPassword;

            sessioninfo.Account = account;

            this.m_tempSessionInfo = sessioninfo;

            return sessioninfo;
        }

        SessionInfo m_tempSessionInfo = null;
        string PatronDbName = "";   // 同步的读者库名
        string From = "";           // id字段检索途径名
        string IdElementName = "";  // id字段元素名，缺省为 "barcode"
        string InterfaceUrl = "";   // 接口.net remoting server的URL
        bool ModifyOtherDbRecords = false;  // 同步过程是否修改指定的同步读者库以外的其它读者库中的(和卡中心记录中id匹配的那些)记录

        // 从XML文件中读取全部记录，同步到读者库中
        // parameters:
        //      strLastNumber   本次处理的起点号码。如果为空，表示全部处理
        //      ids   [out]顺便输出已经处理的id字符串数组
        // return:
        //      -1  error
        //      0   succeed
        //      1   中断
        int WriteToReaderDb(
            string strInputFileName,
            string strLastNumber,
            out string strMaxNumber,
            out List<string> ids,
            out string strError)
        {
            strError = "";
            strMaxNumber = "";
            ids = new List<string>();
            int nRet = 0;

            int nCreateCount = 0;   // 新创建的记录数
            int nChangedCount = 0;  // 修改的记录数
            int nNotChangedCount = 0;   // 没有修改的记录数

            Debug.Assert(string.IsNullOrEmpty(this.IdElementName) == false, "");
            Debug.Assert(string.IsNullOrEmpty(this.From) == false, "");
            Debug.Assert(string.IsNullOrEmpty(this.PatronDbName) == false, "");

            SessionInfo sessioninfo = GetTempSessionInfo();

            if (string.IsNullOrEmpty(strLastNumber) == true)
                strLastNumber = "-1";

            Int64 nStart = 0;

            if (Int64.TryParse(strLastNumber, out nStart) == false)
            {
                strError = "参数strLastNumber值 '" + strLastNumber + "' 错误，应当为纯数字";
                return -1;
            }

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            Stream file = null;

            try
            {
                file = File.Open(strInputFileName,
                    FileMode.Open,
                    FileAccess.Read);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strInputFileName + " 失败: " + ex.Message;
                return -1;
            }

            XmlTextReader reader = new XmlTextReader(file);

            try
            {
                bool bRet = false;

                while (true)
                {
                    bRet = reader.Read();
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
                    Thread.Sleep(1);    // 避免处理太繁忙

#if NO
                    // 2012/2/4
                    // 系统挂起的时候，不运行本线程
                    if (this.App.HangupReason == HangupReason.LogRecover)
                    {
                        strError = "因为系统挂起或暂停批处理而中断处理";
                        return -1;
                    }
#endif
                    if (this.Stopped == true)
                    {
                        strError = "中断处理";
                        return -1;
                    }

                    if (this.Stopped == true)
                    {
                        return 1;
                    }

                    while (true)
                    {
                        bRet = reader.Read();
                        if (bRet == false)
                            return 0;
                        if (reader.NodeType == XmlNodeType.Element)
                            break;
                    }

                    if (bRet == false)
                        return 0;	// 结束

                    string strSourceXml = reader.ReadOuterXml();

                    XmlDocument source_dom = new XmlDocument();
                    try
                    {
                        source_dom.LoadXml(strSourceXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strSourceXml字符串装入XMLDOM时出错: " + ex.Message;
                        return -1;
                    }

                    string strID = DomUtil.GetElementText(source_dom.DocumentElement,
                        this.IdElementName);
                    if (string.IsNullOrEmpty(strID) == true)
                    {
                        strError = "来自卡中心的XML记录 '" + strSourceXml + "' 中没有名为 " + this.IdElementName + " 的元素，或其值为空";
                        return -1;
                    }

                    ids.Add(strID);

                    // 进入范围才处理
                    if (i <= nStart)
                    {
                        continue;
                    }

                    int nRedoCount = 0;
                REDO:
                    string strExistingXml = "";
                    string strOutputPath = "";
                    byte[] baTimestamp = null;
                    // 检索所有读者库，看这条记录是否已经存在
                    // 通过特定检索途径获得读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.App.GetReaderRecXmlByFrom(
                        // this.RmsChannels,
                        channel,
                        null,   // this.PatronDbName,
                        strID,
                        this.From,
                        out strExistingXml,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取读者记录 '" + strID + "' (检索途径 '" + this.From + "') 时发生错误: " + strError;
                        return -1;
                    }

                    if (nRet > 1)
                    {
                        // 警告，检索命中不唯一
                        string strErrorText = "获取读者记录 '" + strID + "' (检索途径 '" + this.From + "') 时发现命中多条("+nRet.ToString()+")记录，这是一个严重错误，请系统管理员尽快检查修复";
                        this.AppendResultText(strErrorText + "\r\n");
                        this.App.WriteErrorLog(strErrorText);
                        continue;
                    }

                    if (nRet == 0)
                    {
                        string strNewXml = "";
                        // 没有命中。需要创建新的读者记录
                        nRet = BuildNewPatronXml(
        source_dom,
        out strNewXml,
        out strError);
                        if (nRet == -1)
                            return -1;

                        string strSavedXml = "";
                        string strSavedRecPath = "";
                        byte[] baNewTimestamp = null;
                        DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                        LibraryServerResult result = this.App.SetReaderInfo(
sessioninfo,
"new",
this.PatronDbName + "/?",
strNewXml,
null,
null,
out strExistingXml,
out strSavedXml,
out strSavedRecPath,
out baNewTimestamp,
out kernel_errorcode);
                        if (result.Value == -1)
                        {
                            strError = "在数据库 " + this.PatronDbName + " 中创建读者记录时出错: " + result.ErrorInfo;
                            return -1;
                        }

                        // this.AppendResultText("创建读者记录 " + strSavedRecPath + "\r\n");
                        this.SetProgressText("创建读者记录 " + strSavedRecPath);
                        nCreateCount++;
                    }
                    else
                    {
                        // 观察是否在同步的读者库中
                        string strDbName = ResPath.GetDbName(strOutputPath);
                        if (strDbName != this.PatronDbName
                            && this.ModifyOtherDbRecords == false)
                        {
                            nNotChangedCount++;
                            continue;
                        }

                        // 命中，需要检查和修改读者记录
                        XmlDocument exist_dom = new XmlDocument();
                        try
                        {
                            exist_dom.LoadXml(strExistingXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "strExistingXml字符串装入XMLDOM时出错: " + ex.Message;
                            return -1;
                        }

                        string strMergedXml = "";
                        // 检查记录有无修改
                        // return:
                        //      -1  出错
                        //      0   没有修改
                        //      1   有修改
                        nRet = MergePatronXml(
                            exist_dom,
                            source_dom,
                            out strMergedXml,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (nRet == 1)
                        {
                            string strSavedXml = "";
                            string strSavedRecPath = "";
                            byte[] baNewTimestamp = null;
                            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                            LibraryServerResult result = this.App.SetReaderInfo(
    sessioninfo,
    "change",
    strOutputPath,
    strMergedXml,
    strExistingXml,
    baTimestamp,
    out strExistingXml,
    out strSavedXml,
    out strSavedRecPath,
    out baNewTimestamp,
    out kernel_errorcode);
                            if (result.Value == -1)
                            {
                                // 时间戳不匹配，重试
                                if (nRedoCount < 10
                                    && kernel_errorcode == rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch)
                                {
                                    nRedoCount++;
                                    goto REDO;
                                }

                                strError = "修改保存读者记录 '" + strOutputPath + "' 时出错: " + result.ErrorInfo;
                                return -1;
                            }
                            // this.AppendResultText("更新读者记录 " + strSavedRecPath + "\r\n");
                            this.SetProgressText("更新读者记录 " + strSavedRecPath);
                            nChangedCount++;
                        }
                        else
                        {
                            nNotChangedCount++;
                        }

                    }

                    strMaxNumber = i.ToString();
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (file != null)
                    file.Close();

                this.SetProgressText("");
                this.AppendResultText("创建记录数 " + nCreateCount.ToString() + "; 修改记录数 " + nChangedCount.ToString() + "; 没有发生变化的记录数 " + nNotChangedCount.ToString() + "\r\n");
            }

            return 0;
        }

        // 根据两个集合的差异部分,标记删除卡中心全部记录中为包含的当前读者记录
        // return:
        //      -1  error
        //      0   succeed
        //      1   中断
        int MaskDeleteRecords(
            List<string> ids,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nDeleteCount = 0;   // 标记删除的记录数
            int nNotChangedCount = 0;   // 原先已经标记删除的记录数

            SessionInfo sessioninfo = GetTempSessionInfo();

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                Thread.Sleep(1);    // 避免处理太繁忙

#if NO
                // 2012/2/4
                // 系统挂起的时候，不运行本线程
                if (this.App.HangupReason == HangupReason.LogRecover)
                {
                    strError = "因为系统挂起或暂停批处理而中断处理";
                    return -1;
                }
#endif
                if (this.Stopped == true)
                {
                    strError = "中断处理";
                    return -1;
                }

                if (this.Stopped == true)
                {
                    return 1;
                }

                string strID = ids[i].Trim();
                if (string.IsNullOrEmpty(strID) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                int nRedoCount = 0;
            REDO:
                string strExistingXml = "";
                string strOutputPath = "";
                byte[] baTimestamp = null;
                // 检索读者库，看这条记录是否已经存在
                // 通过特定检索途径获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXmlByFrom(
                    // this.RmsChannels,
                    channel,
                    this.PatronDbName,
                    strID,
                    this.From,
                    out strExistingXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取读者记录 '" + strID + "' (检索途径 '" + this.From + "') 时发生错误: " + strError;
                    return -1;
                }
                if (nRet == 0)
                {
                    // 等到处理记录的时候，发现记录已经不存在
                    this.App.WriteErrorLog("PatronReplication: 在删除阶段发现ID为 '"+strID+"' 的读者记录已经不存在");
                    continue;
                }

                if (nRet > 1)
                {
                    // TODO: 警告，检索命中不唯一
                }

                {
                    // 命中，需要检查和修改读者记录
                    XmlDocument exist_dom = new XmlDocument();
                    try
                    {
                        exist_dom.LoadXml(strExistingXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistingXml字符串装入XMLDOM时出错: " + ex.Message;
                        return -1;
                    }

                    string strOutputXml = "";

                    // 准备要标记删除写回的XML记录
                    // return:
                    //      -1  出错
                    //      0   没有修改(本来就是标记删除状态)
                    //      1   有修改
                    nRet = BuildMaskDeleteXml(
                        exist_dom,
                        out strOutputXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (nRet == 1)
                    {
                        string strSavedXml = "";
                        string strSavedRecPath = "";
                        byte[] baNewTimestamp = null;
                        DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                        LibraryServerResult result = this.App.SetReaderInfo(
sessioninfo,
"change",
strOutputPath,
strOutputXml,
strExistingXml,
baTimestamp,
out strExistingXml,
out strSavedXml,
out strSavedRecPath,
out baNewTimestamp,
out kernel_errorcode);
                        if (result.Value == -1)
                        {
                            // 时间戳不匹配，重试
                            if (nRedoCount < 10
                                && kernel_errorcode == rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch)
                            {
                                nRedoCount++;
                                goto REDO;
                            }

                            strError = "修改保存读者记录 '" + strOutputPath + "' 时出错: " + result.ErrorInfo;
                            return -1;
                        }

                        // this.AppendResultText("标记删除读者记录 '" + strOutputPath + "'\r\n");
                        this.SetProgressText("标记删除读者记录 " + strOutputPath);
                        nDeleteCount++;
                    }
                    else
                    {
                        this.SetProgressText("发现已经标记删除读者记录 '" + strOutputPath);
                        nNotChangedCount++;
                    }
                }
            }

            this.SetProgressText("");
            this.AppendResultText("本次标记删除记录数 " + nDeleteCount.ToString() + "; 原先已经标记删除的记录数 " + nNotChangedCount.ToString() + "\r\n");

            return 0;
        }

        // 读者记录中 需要从卡中心同步的 元素名列表
        static string[] _patron_rep_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post", // 2009/7/17
                "address",
                "tel",
                "email",
                "comment",
                "cardNumber",   // 借书证号。为和原来的(100$b)兼容，也为了将来放RFID卡号 2008/10/14
                "nation",   // 2011/9/24
            };

        int BuildNewPatronXml(
            XmlDocument domNew,
            out string strOutputXml,
            out string strError)
        {
            strOutputXml = "";
            strError = "";

            string[] element_names = null;

            // 字段的定义，如果第一个元素为空，表示全部用定义的值；如果第一个元素不是空，则增补缺省的定义
            if (this.App.PatronReplicationFields == null
                || this.App.PatronReplicationFields.Count == 0)
            {
                element_names = _patron_rep_element_names;
                this.App.WriteDebugInfo("BuildNewPatronXml() 使用了缺省的字段名列表");
            }
            else if (string.IsNullOrEmpty(this.App.PatronReplicationFields[0]) == false)
            {
                element_names = StringUtil.Append(_patron_rep_element_names, this.App.PatronReplicationFields.ToArray());
                this.App.WriteDebugInfo("BuildNewPatronXml() 使用了扩展后的字段名列表 '" + string.Join(",", element_names) + "' 。扩展部分为 '" + StringUtil.MakePathList(this.App.PatronReplicationFields) + "'");
            }
            else
            {
                element_names = this.App.PatronReplicationFields.ToArray();
                this.App.WriteDebugInfo("BuildNewPatronXml() 使用了重新定义的字段名列表 '" + string.Join(",", element_names) + "' 。");
            }

            this.App.WriteDebugInfo("MergePatronXml() domNew='" + domNew.OuterXml + "'");

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            for (int i = 0; i < domNew.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode node = domNew.DocumentElement.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                // 2015/11/10
                if (Array.IndexOf(element_names, node.Name) == -1)
                {
                    // TODO: 似可以把被去掉的元素详情写入调试日志
                    domNew.DocumentElement.RemoveChild(node);
                    i--;
                    continue;
                }

                XmlNode attr = node.SelectSingleNode("@dprms:missing", nsmgr);
                if (attr != null)
                {
                    domNew.DocumentElement.RemoveChild(node);
                    i--;
                }
            }

            // 合成<state>元素内容
            if (Array.IndexOf(element_names, "state") != -1)
            {
                String strState = DomUtil.GetElementText(domNew.DocumentElement,
                    "state");
                List<string> source_list = StringUtil.SplitList(strState);

                // 否则需要将strTextExist中的非“卡中心”部分和new_list合并
                List<string> result_list = StringUtil.SplitList("待启用");
                foreach (string strText in source_list)
                {
                    result_list.Add("卡中心" + strText);
                }
                DomUtil.SetElementText(domNew.DocumentElement,
        "state",
        StringUtil.MakePathList(result_list));
            }

            strOutputXml = domNew.DocumentElement.OuterXml;

            this.App.WriteDebugInfo("MergePatronXml() strOutputXml='" + strOutputXml + "'");
            return 0;
        }

        // 从状态字符串中挑选出属于卡中心的那些
        // 所取出的字符串已经去掉了前面的“卡中心”部分
        static List<string> GetCardCenterState(string strState)
        {
            List<string> results = new List<string>();
            string[] parts = strState.Split(new char[] {','});
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                if (StringUtil.HasHead(strText, "卡中心") == true)
                    results.Add(strText.Substring("卡中心".Length));
            }

            return results;
        }

        // 从状态字符串中挑选出*不*属于卡中心的那些
        static List<string> GetNoneCardCenterState(string strState)
        {
            List<string> results = new List<string>();
            string[] parts = strState.Split(new char[] { ',' });
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                if (StringUtil.HasHead(strText, "卡中心") == true)
                {
                }
                else
                    results.Add(strText);
            }

            return results;
        }

        // TODO: 需要硬编码禁止覆盖一些流通专用的字段 borrows 等
        // TODO: <fprms:file> 元素应该不让覆盖
        // 检查记录有无修改
        // return:
        //      -1  出错
        //      0   没有修改
        //      1   有修改
        int MergePatronXml(
            XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strError = "";
            strMergedXml = "";

            string[] element_names = null;

            // 字段的定义，如果第一个元素为空，表示全部用定义的值；如果第一个元素不是空，则增补缺省的定义
            if (this.App.PatronReplicationFields == null
                || this.App.PatronReplicationFields.Count == 0)
            {
                element_names = _patron_rep_element_names;
                this.App.WriteDebugInfo("MergePatronXml() 使用了缺省的字段名列表");
            }
            else if (string.IsNullOrEmpty(this.App.PatronReplicationFields[0]) == false)
            {
                element_names = StringUtil.Append(_patron_rep_element_names, this.App.PatronReplicationFields.ToArray());
                this.App.WriteDebugInfo("MergePatronXml() 使用了扩展后的字段名列表 '" + string.Join(",", element_names) + "' 。扩展部分为 '" + StringUtil.MakePathList(this.App.PatronReplicationFields) + "'");
            }
            else
            {
                element_names = this.App.PatronReplicationFields.ToArray();
                this.App.WriteDebugInfo("MergePatronXml() 使用了重新定义的字段名列表 '" + string.Join(",", element_names) + "' 。");
            }

            this.App.WriteDebugInfo("MergePatronXml() domExist='"+domExist.OuterXml+"'");
            this.App.WriteDebugInfo("MergePatronXml() domNew='" + domNew.OuterXml + "'");

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            bool bChanged = false;

            for (int i = 0; i < element_names.Length; i++)
            {
                string strElementName = element_names[i];
                if (string.IsNullOrEmpty(strElementName) == true)
                    continue;

                XmlNode node = domNew.DocumentElement.SelectSingleNode(strElementName); // 2012/12/12 加入 DocumentElement
                if (node != null)
                {
                    XmlNode attr = node.SelectSingleNode("@dprms:missing", nsmgr);
                    if (attr != null)
                        continue;   // 依照domExist中的原始值
                }

                string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
    strElementName);
                string strTextExist = DomUtil.GetElementOuterXml(domExist.DocumentElement,
    strElementName);

                if (strElementName == "state")
                {
                    string strTextNew0 = DomUtil.GetElementText(domNew.DocumentElement,
strElementName);
                    string strTextExist0 = DomUtil.GetElementText(domExist.DocumentElement,
        strElementName);

                    List<string> exist_list = GetCardCenterState(strTextExist0);
                    List<string> new_list = StringUtil.SplitList(strTextNew0);
                    if (StringUtil.IsEqualList(exist_list, new_list) == true)
                        continue;

                    // 否则需要将 strTextExist 中的非“卡中心”部分和new_list合并
                    List<string> result_list = GetNoneCardCenterState(strTextExist0);
                    foreach (string strText in new_list)
                    {
                        result_list.Add("卡中心" + strText);
                    }
                    DomUtil.SetElementText(domExist.DocumentElement,
                        strElementName,
                        StringUtil.MakePathList(result_list));
                    bChanged = true;
                    continue;
                }

                if (strTextExist != strTextNew)
                {
                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        strElementName,
                        strTextNew);
                    bChanged = true;
                }
            }

            strMergedXml = domExist.OuterXml;

            this.App.WriteDebugInfo("MergePatronXml() strMergedXml='" + strMergedXml + "'");

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 准备要标记删除写回的XML记录
        // return:
        //      -1  出错
        //      0   没有修改(本来就是标记删除状态)
        //      1   有修改
        int BuildMaskDeleteXml(
            XmlDocument domExist,
            out string strOutputXml,
            out string strError)
        {
            strError = "";
            strOutputXml = "";

            string strState = DomUtil.GetElementText(domExist.DocumentElement,
                "state");
            if (StringUtil.IsInList("卡中心删除", strState) == true)
                return 0;

            StringUtil.SetInList(ref strState, "卡中心删除", true);
            DomUtil.SetElementText(domExist.DocumentElement,
                "state",
                strState);

            strOutputXml = domExist.DocumentElement.OuterXml;
            return 1;
        }

        // 通过特定检索途径获得读者记录的特定keys值
        // return:
        //      -1  error
        //      >=0 命中个数
        public int SearchAllIds(
            RmsChannelCollection channels,
            string strPatronDbName,
            string strFrom,
            out List<string> ids,
            out string strError)
        {
            strError = "";
            ids = new List<string>();

            Debug.Assert(String.IsNullOrEmpty(strPatronDbName) == false, "");
            // 构造检索式
            string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strPatronDbName + ":" + strFrom)
                    + "'><item><word></word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            RmsChannel channel = channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strOutputStyle = "keyid";
            string strBrowseStyle = "keyid,key,id,cols";


            long lRet = channel.DoSearch(strQueryXml,
                "default",
                strOutputStyle,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
                return 0;

            long lHitCount = lRet;
            long lStart = 0;
            for (; ; )
            {
#if NO
                // 2012/2/4
                // 系统挂起的时候，不运行本线程
                if (this.App.HangupReason == HangupReason.LogRecover)
                {
                    strError = "因为系统挂起或暂停批处理而中断处理";
                    return -1;
                }
#endif
                if (this.Stopped == true)
                {
                    strError = "中断处理";
                    return -1;
                }

                Record[] records = null;
                lRet = channel.DoGetSearchResult(
                    "default",
                    lStart,
                    -1,
                    strBrowseStyle,
                    "zh",
                    null,
                    out records,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                foreach (Record record in records)
                {
                    Debug.Assert(record.Keys != null && record.Keys.Length > 0, "");
                    if (record.Keys != null && record.Keys.Length > 0)
                        ids.Add(record.Keys[0].Key);
                }

                lStart += records.Length;
                if (lStart >= lHitCount)
                    break;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#if NO
        // 读取上次最后处理的号码
        // parameters:
        //
        // return:
        //      -1  出错
        //      0   没有找到断点信息
        //      1   找到了断点信息
        public int ReadLastNumber(
            out bool bLoop,
            out string strLastNumber,
            out string strError)
        {
            bLoop = false;
            strLastNumber = "";
            strError = "";

            string strBreakPointString = "";
            // 从断点记忆文件中读出信息
            // return:
            //      -1  error
            //      0   file not found
            //      1   found
            int nRet = this.App.ReadBatchTaskBreakPointFile(this.DefaultName,
                            out strBreakPointString,
                            out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            // return:
            //      -1  xml error
            //      0   not found
            //      1   found
            nRet = ParseBreakPointString(
                strBreakPointString,
                out bLoop,
                out strLastNumber);
            return 1;
        }

        // 构造断点字符串
        static string MakeBreakPointString(
            bool bLoop,
            string strRecordID)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                "recordID",
                strRecordID);
            DomUtil.SetElementText(dom.DocumentElement,
                "loop",
                bLoop == true ? "true" : "false");

            return dom.OuterXml;
        }

        // return:
        //      -1  xml error
        //      0   not found
        //      1   found
        static int ParseBreakPointString(
            string strBreakPointString,
            out bool bLoop,
            out string strRecordID)
        {
            bLoop = false;
            strRecordID = "";

            if (String.IsNullOrEmpty(strBreakPointString) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strBreakPointString);
            }
            catch
            {
                return -1;
            }

            string strLoop = DomUtil.GetElementText(dom.DocumentElement,
                "loop");
            if (strLoop == "true")
                bLoop = true;

            strRecordID = DomUtil.GetElementText(dom.DocumentElement,
                "recordID");

            return 1;
        }

        // new
        // 写入断点记忆文件
        public void WriteLastNumber(
            bool bLoop,
            string strLastNumber)
        {
            string strBreakPointString = MakeBreakPointString(bLoop, strLastNumber);

            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.DefaultName,
                strBreakPointString);
        }

#endif
    }
}
