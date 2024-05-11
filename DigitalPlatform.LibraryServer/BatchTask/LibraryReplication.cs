using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// dp2Library 服务器之间的同步 批处理任务
    /// </summary>
    public class LibraryReplication : BatchTask
    {
        // 同步级别
        public ReplicationLevel ReplicationLevel = ReplicationLevel.Full;

        public LibraryReplication(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.PerTime = 5 * 60 * 1000;	// 5分钟

            this.Loop = true;   // 一直持续运行是常态
        }

        public override string DefaultName
        {
            get
            {
                return "dp2Library 同步";
            }
        }

        #region 参数字符串处理 
        // 这些函数也被 dp2Library 前端使用

        // 解析 开始 参数
        static int ParseStart(string strStart,
            out long index,
            out string strDate,
            out string strServer,
            out string strError)
        {
            strError = "";
            index = 0;
            strDate = "";
            strServer = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strStart);
            string strIndex = (string)table["index"];
            if (string.IsNullOrEmpty(strIndex) == true)
                index = 0;
            else
            {
                if (long.TryParse(strIndex, out index) == false)
                {
                    strError = "index 参数值 '"+strIndex+"' 不合法，应为纯数字";
                    return -1;
                }
            }

            strDate = (string)table["date"];

            strServer = (string)table["server"];

            return 0;
        }

        // 构造开始参数，也是断点字符串
        static string BuildStart(
            long index,
            string strDate,
            string strServer)
        {
            Hashtable table = new Hashtable();
            table["index"] = index.ToString();
            table["date"] = strDate;
            table["server"] = strServer;

            return StringUtil.BuildParameterString(table);
        }

#if NO
        // 解析 开始 参数
        static int ParseLogRecorverStart(string strStart,
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

#endif

        // 解析通用启动参数
        public static int ParseTaskParam(string strParam,
            out string strLevel,
            out bool bClearFirst,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strLevel = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strLevel = (string)table["level"];

            string strClearFirst = (string)table["clear_first"];
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            return 0;
        }

        static string BuildTaskParam(
            string strLevel,
            bool bClearFirst)
        {
            Hashtable table = new Hashtable();
            table["level"] = strLevel;
            table["clear_first"] = bClearFirst ? "yes" : "no";
            return StringUtil.BuildParameterString(table);
        }

#endregion

        // 补全所有服务器的断点信息
        int EnsureServers(
            BreakPointCollcation all_breakpoints,
            ref BreakPointCollcation breakpoints,
            out string strError)
        {
            strError = "";

            XmlNodeList server_nodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("center/server");
            if (server_nodes.Count == 0)
                return 0;

            foreach (XmlNode server in server_nodes)
            {
                string strServerName = DomUtil.GetAttr(server, "name");

                // 找到断点信息
                BreakPointInfo info = breakpoints.GetBreakPoint(strServerName);

                if (info == null)
                {
                    // 按照复制全库，然后从当前同步的办法处理
                    info = all_breakpoints.GetBreakPoint(strServerName);
                    if (info == null)
                    {
                        strError = "all_breakpoints 中没有找到名为 '" + strServerName + "' 的服务器断点信息";
                        return -1;
                    }
                    // 增补断点信息，便于后面存储
                    breakpoints.Add(info);
                }
            }

            // 去除空的 servername
            for (int i = 0; i < breakpoints.Count; i++)
            {
                BreakPointInfo info = breakpoints[i];

                if (string.IsNullOrEmpty(info.ServerName) == true)
                {
                    breakpoints.RemoveAt(i);
                    i--;
                }
            }

            return 0;
        }

        // 一次操作循环
        // TODO: 做完开始服务器后，其余服务器的 index 从 0 开始，filename 则是同一天的。然后顺次做后面的 filename
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            //if (this.App.HangupReason == HangupReason.LogRecover)
            //    return;
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            if (this.App.PauseBatchTask == true)
                return;

            try
            {
                string strError = "";

                if (this.App.LibraryCfgDom == null
                    || this.App.LibraryCfgDom.DocumentElement == null)
                    return;

                XmlNodeList server_nodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("center/server");
                if (server_nodes.Count == 0)
                    return;

                BatchTaskStartInfo startinfo = this.StartInfo;
                if (startinfo == null)
                    startinfo = new BatchTaskStartInfo();   // 按照缺省值来

                long lStartIndex = 0;   // 开始位置
                string strStartDate = "";   // 开始文件名
                string strStartServer = "";  // 开始服务器
                int nRet = ParseStart(startinfo.Start,
                    out lStartIndex,
                    out strStartDate,
                    out strStartServer,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                // 下一次 loop 进入的时候自动就是 continue
                startinfo.Start = "";


                string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

                //
                string strRecoverLevel = "";
                bool bClearFirst = false;
                nRet = ParseTaskParam(startinfo.Param,
                    out strRecoverLevel,
                    out bClearFirst,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                if (String.IsNullOrEmpty(strRecoverLevel) == true)
                    strRecoverLevel = "Full";

                // 下一次 loop 进入的时候什么动作有没有，避免重复前一次的清除数据库动作
                startinfo.Param = "";

                try
                {
                    this.ReplicationLevel = (ReplicationLevel)Enum.Parse(typeof(ReplicationLevel), strRecoverLevel, true);
                }
                catch (Exception ex)
                {
                    this.AppendResultText("启动失败: 启动参数Param中的recoverLevel枚举值 '" + strRecoverLevel + "' 错误: " + ex.Message + "\r\n");
                    return;
                }

                if (bClearFirst == true)
                {

                    // 清除全部同步的本地库

                }

                if (String.IsNullOrEmpty(strStartDate) == true)
                {
                    // 从断点继续循环
                    strStartDate = "continue";
                }

                // 构造用于复制然后同步的断点信息
                BreakPointCollcation all_breakpoints = null;
                try
                {
                    all_breakpoints = BuildCopyAndContinue(strEndDate);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                // 进行处理
                BreakPointCollcation breakpoints = null;

                this.AppendResultText("*********\r\n");

                if (strStartDate == "continue")
                {
                    // 按照断点信息处理
                    this.AppendResultText("从上次断点位置继续\r\n");

                    string strBreakPointString = "";
                    // 从断点记忆文件中读出信息
                    // return:
                    //      -1  error
                    //      0   file not found
                    //      1   found
                    nRet = this.App.ReadBatchTaskBreakPointFile(this.DefaultName,
                                    out strBreakPointString,
                                    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "启动失败。因当前还没有断点信息，请指定为其他方式运行";
                        goto ERROR1;
                    }

                    // 可能会抛出异常
                    breakpoints = BreakPointCollcation.Build(strBreakPointString);

                    // 补充新增的 (断点信息中没有包含的) 数据库的处理办法

                    // 补全所有服务器的断点信息
                    nRet = EnsureServers(
                        all_breakpoints,
                        ref breakpoints,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "补全所有服务器的断点信息时出错: " + strError;
                        goto ERROR1;
                    }
                }
                else if (strStartDate == "copy_and_continue")
                {
                    // 先从远端复制整个数据库，然后从开始复制时的日志末尾进行同步
                    this.AppendResultText("复制所有中心书目库\r\n");

                    // 采纳先前创建好的复制并继续的断点信息
                    breakpoints = all_breakpoints;

                    strStartDate = strEndDate;
                }
                else
                {
                    this.AppendResultText("从指定的日志文件开始同步\r\n");

                    breakpoints = new BreakPointCollcation();
                    bool bOn = false;
                    if (string.IsNullOrEmpty(strStartServer) == true)
                        bOn = true;

                    foreach (XmlNode server in server_nodes)
                    {
                        string strServerName = DomUtil.GetAttr(server, "name");

                        if (strStartServer == strServerName)
                            bOn = true;

                        if (bOn == true)
                        {
                            BreakPointInfo info = new BreakPointInfo();
                            info.ServerName = strServerName;
                            info.Date = strStartDate;
                            info.Index = lStartIndex;
                            breakpoints.Add(info);

                            lStartIndex = 0;    // 从此以后都是 0
                        }
                    }

                }

                Debug.Assert(breakpoints != null, "");

                this.AppendResultText("计划进行的处理是：\r\n---\r\n" + all_breakpoints.GetSummary() + "---\r\n\r\n");


                // 按照断点信息进行处理
                foreach (XmlNode server in server_nodes)
                {
                    string strServerName = DomUtil.GetAttr(server, "name");

                    // 找到断点信息
                    BreakPointInfo info = breakpoints.GetBreakPoint(strServerName);

#if NO
                    if (info == null)
                    {
                        // 按照复制全库，然后从当前同步的办法处理
                        info = all_breakpoints.GetBreakPoint(strServerName);
                        if (info == null)
                        {
                            strError = "all_breakpoints 中没有找到名为 '"+strServerName+"' 的服务器断点信息";
                            goto ERROR1;
                        }
                        // 增补断点信息，便于后面存储
                        breakpoints.Add(info);
                    }
#endif
                    if (info == null)
                        continue;

                    if (string.IsNullOrEmpty(info.BiblioDbName) == false)
                    {
                        // 从数据库复制

                        // 列出中心服务器的全部可用数据库，然后进行复制
                        // 断点书目库名表示从这个库开始向后复制

                        // 从远端复制一个数据库
                        // 函数返回后， info 信息可能会被改变，需要及时保存到断点文件中，便于以后重新启动批处理
                        // return:
                        //      -1  出错
                        //      0   中断
                        //      1   完成
                        nRet = CopyDatabase(server,
                            ref info,
                            out strError);

                        // 保存断点文件
                        SaveBreakPoint(breakpoints);

                        if (nRet == -1)
                            goto ERROR1;

                        if (nRet == 0)
                            goto STOP;

                        // 表示复制已经成功结束
                        info.BiblioDbName = "";
                        info.RecID = "";

                        // 保存断点文件
                        SaveBreakPoint(breakpoints);
                    }

                    if (string.IsNullOrEmpty(info.Date) == false)
                    {
                        string strLastDate = "";
                        long last_index = -1;

                        nRet = ProcessServer(server,
                            info.Date,
                            strEndDate,
                            info.Index,
                            out strLastDate,
                            out last_index,
                            out strError);
                        // 记忆
                        if (string.IsNullOrEmpty(strLastDate) == false)
                        {
                            Debug.Assert(last_index != -1, "");
                            info.Date = strLastDate;
                            info.Index = last_index;
                            // 注：从同步的角度来说，同步永远不会结束，所以不会清除 Date 和 Index
                        }
                        // 保存断点文件
                        SaveBreakPoint(breakpoints);

                        if (nRet == -1)
                        {
                            this.AppendResultText("发生错误：" + strError + "\r\n");
                        }
                        if (nRet == 0)
                            goto STOP;

                        SetProgressText("完成");
                    }
                }

                // TODO: 从断点信息中清除那些没有在配置中的服务器名。这是以前遗留下来的。

                this.AppendResultText("本轮处理结束\r\n");
                return;

            ERROR1:
                return;
            }
            finally
            {

            }

        STOP:
            if (this.App.PauseBatchTask == true)
            {
                this.Loop = true;   // 如果因为暂停而中断，既的后面还要重新开始
                SetProgressText("暂时中断");
                this.AppendResultText("暂时中断\r\n");
            }
            else
            {
                this.Loop = false;
                SetProgressText("中断");
                this.AppendResultText("中断\r\n");
            }
        }

        // 从远端复制一个数据库
        // 函数返回后， info 信息可能会被改变，需要及时保存到断点文件中，便于以后重新启动批处理
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int CopyDatabase(
            XmlNode server,
            ref BreakPointInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(info.ServerName) == true)
                return 0;

            // 把一个远端服务器的全部数据库排列起来，处理其中指定的几个
            List<ItemDbCfg> cfgs = this.App.FindReplicationItems(info.ServerName);
            if (cfgs.Count == 0)
                return 0;

#if NO
                        XmlNode server = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("center/server['"+strRemoteServer+"']");
            if (server == null)
            {
                strError = "名为 '"+strRemoteServer+"' 的 server 定义在 LibraryCfgDom 中没有找到";
                return -1;
            }
#endif

            string strUrl = DomUtil.GetAttr(server, "url");
            if (string.IsNullOrEmpty(strUrl) == true)
            {
                strError = "中心服务器配置片断 '" + server.OuterXml + "' 中缺乏有效的 url 属性值";
                return -1;
            }

            LibraryChannel channel = new LibraryChannel();
            channel.Url = strUrl;

            this.m_strUrl = strUrl;
            this.m_strUserName = DomUtil.GetAttr(server, "username");
            this.m_strPassword = LibraryApplication.DecryptPassword(DomUtil.GetAttr(server, "password"));

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            long lProcessCount = 0;

            try
            {

                bool bOn = false;   // 是否需要执行
                string strStartDbName = info.BiblioDbName;
                if (strStartDbName == "*")
                    bOn = true; // 全部都做

                string strStartRecID = info.RecID;
                if (strStartRecID == "*")
                    strStartRecID = "1";

                foreach (ItemDbCfg cfg in cfgs)
                {
                    if (strStartDbName == cfg.BiblioDbName)
                    {
                        bOn = true; // 从这里一直到末尾
                        strStartDbName = "";
                    }

                    if (bOn == true)
                    {
                        this.AppendResultText("从中心服务器 " + cfg.ReplicationServer + " 复制 " + cfg.ReplicationDbName + " 到 本服务器的书目库 " + cfg.BiblioDbName + "\r\n");

                        string strID = strStartRecID;
                        // 复制一个数据库
                        // return:
                        //      -1  出错
                        //      0   中断
                        //      1   全部处理结束
                        nRet = CopyOneDatabase(
                            channel,
                            cfg.ReplicationDbName,
                            cfg.BiblioDbName,
                            ref lProcessCount,
                            ref strID,
                            out strError);
                        // 及时存储断点信息
                        if (string.IsNullOrEmpty(strID) == false)
                        {
                            info.BiblioDbName = cfg.BiblioDbName;
                            info.RecID = strID;
                        }
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                            return 0;

                        strStartRecID = "1";    // 做过一个数据库以后，后面的都从头开始
                    }
                }

            }
            finally
            {
                channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                channel.Close();
                this.AppendResultText("    共复制 " + lProcessCount.ToString() + " 条书目记录\r\n");
            }

            return 1;
        }

        // 复制一个数据库
        // parameters:
        //      strStartID  [in]开始的 ID
        //                  [out]结束位置的下一条的 ID
        // return:
        //      -1  出错
        //      0   中断
        //      1   全部处理结束
        int CopyOneDatabase(
            LibraryChannel channel,
            string strRemoteDbName,
            string strLocalDbName,
            ref long lProcessCount,
            ref string strStartID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.Stopped == true)
                return 0;

            string strQueryWord = "";
            if (string.IsNullOrEmpty(strStartID) == false)
                strQueryWord = strStartID + "-";

            this.AppendResultText("从 " + strRemoteDbName + " 复制记录到 " + strLocalDbName + " " + strQueryWord + "\r\n");

            SetProgressText("正在检索 "+strRemoteDbName+" 记录范围 "+strQueryWord+"");
            this.AppendResultText("    正在检索 " + strRemoteDbName + " 记录范围 " + strQueryWord + "... ");

            string strOutputStyle = "";
            string strQueryXml = "";
            long lRet = channel.SearchBiblio(null,
                strRemoteDbName,
                strQueryWord,
                -1,  // 1000
                "recid",
                "left",
                "zh",
                null,   // strResultSetName
                "",    // strSearchStyle
                "",
                strOutputStyle,
                out strQueryXml,
                out strError);
            if (lRet == -1)
                return -1;
            
            long lHitCount = lRet;

            long lStart = 0;
            long lPerCount = Math.Min(500, lHitCount);
            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            List<long> recids = new List<long>();

            // 装入浏览格式
            for (; ; )
            {
                if (this.Stopped == true)
                    return 0;

                string strBrowseStyle = "id";
                lRet = channel.GetSearchResult(
                    null,   // stop,
                    null,   // strResultSetName
                    lStart,
                    lPerCount,
                    strBrowseStyle,
                    "zh",
                    out searchresults,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                {
                    strError = "GetSearchResult() return 0";
                    return -1;
                }

                // 处理浏览结果
                for (int i = 0; i < searchresults.Length; i++)
                {
                    DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                    // string strID = DigitalPlatform.CirculationClient.ResPath.GetRecordId(searchresult.Path);
                    string strID = StringUtil.GetRecordId(searchresult.Path);
                    long id = 0;
                    if (long.TryParse(strID, out id) == false)
                    {
                        strError = "检索得到的路径 '" + searchresult.Path + "' ID 部分 '"+strID+"' 不合法";
                        return -1;
                    }
                    recids.Add(id);
                }


                lStart += searchresults.Length;
                // lCount -= searchresults.Length;
                if (lStart >= lHitCount || lPerCount <= 0)
                    break;

                // this.m_lLoaded = lStart;
            }

            // ID 排序
            recids.Sort();

#if NO
            // 找到开始偏移
            long start = 0;
            if (string.IsNullOrEmpty(strStartID) == false)
            {
                if (long.TryParse(strStartID, out start) == false)
                {
                    strError = "strStartID 参数 '" + strStartID + "' 不合法";
                    return -1;
                }
            }

            // 最后一个记录 ID 比起始号码还小
            if (recids.Count > 0 && recids[recids.Count - 1] < start)
                return 0;
#endif

            this.AppendResultText(false, "  正在复制记录 ("+lHitCount.ToString()+")...\r\n");

            List<string> recpaths = new List<string>();
            foreach (long id in recids)
            {
#if NO
                if (id >= start)
                    recpaths.Add(strRemoteDbName + "/" + id.ToString());
#endif
                recpaths.Add(strRemoteDbName + "/" + id.ToString());

                if (recpaths.Count >= 100)
                {
                    if (this.Stopped == true)
                        return 0;
                    string strID = "";
                    // parameters:
                    //      strLastID   [out] 返回成功写入的最后一条记录的 ID
                    // return:
                    //      -1  出错
                    //      0   中断
                    //      1   完成
                    nRet = DumpRecords(
                        channel,
                        recpaths,
                        strLocalDbName,
                        ref lProcessCount,
                        out strID,
                        out strError);
                    if (string.IsNullOrEmpty(strID) == false)
                        strStartID = strID; // TODO: 其实是下一条?
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        return 0;
                    recpaths.Clear();
                }
            }

            if (recpaths.Count > 0)
            {
                if (this.Stopped == true)
                    return 0;

                string strID = "";
                // parameters:
                //      strLastID   [out] 返回成功写入的最后一条记录的 ID
                // return:
                //      -1  出错
                //      0   中断
                //      1   完成
                nRet = DumpRecords(
                    channel,
                    recpaths,
                    strLocalDbName,
                    ref lProcessCount,
                    out strID,
                    out strError);
                if (string.IsNullOrEmpty(strID) == false)
                    strStartID = strID; // TODO: 其实是下一条?
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 0;
            }

            return 1;
        }

        // 把一批远端书目记录复制到本地
        // parameters:
        //      recpaths    远端书目记录路径的集合
        //      strLastID   [out] 返回成功写入的最后一条记录的 ID
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int DumpRecords(
            LibraryChannel channel,
            List<string> recpaths,
            string strLocalDbName,
            ref long lProcessCount,
            out string strLastID,
            out string strError)
        {
            strError = "";
            strLastID = "";

            RmsChannel kernel_channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            BiblioLoader loader = new BiblioLoader();
            loader.Channel = channel;
            loader.Stop = null;
            loader.Format = "xml";
            loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp; // 附加信息只取得 timestamp

            loader.RecPaths = recpaths;

            var enumerator = loader.GetEnumerator();

            foreach (string strRecPath in recpaths)
            {
                if (this.Stopped == true)
                    return 0;

                bool bRet = enumerator.MoveNext();
                if (bRet == false)
                {
                    Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                    strError = "还没有到结尾, MoveNext() 不应该返回 false";
                    return -1;
                }

                BiblioItem biblio = (BiblioItem)enumerator.Current;
                Debug.Assert(biblio.RecPath == strRecPath, "loader 和 recpaths 的元素之间 记录路径存在严格的锁定对应关系");

                if (biblio.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                    continue;

                // string strID = DigitalPlatform.CirculationClient.ResPath.GetRecordId(strRecPath);
                string strID = StringUtil.GetRecordId(strRecPath);
                string strBiblioRecPath = strLocalDbName + "/" + strID;

                SetProgressText("复制 "+biblio.RecPath+" 到 " + strBiblioRecPath);

                byte [] baOutputTimestamp = null;
                string strOutputBiblioRecPath = "";
                long lRet = kernel_channel.DoSaveTextRes(
                    strBiblioRecPath,
                    biblio.Content,
                    false,
                    "content,ignorechecktimestamp,forcesettimestamp",
                    biblio.Timestamp,   // 这是远端书目记录的时间戳，强行设置给本地书目记录
                    out baOutputTimestamp,
                    out strOutputBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                strLastID = strID;
                lProcessCount++;
            }

            return 1;
        }

        // 完成一个 server 的处理
        // parameters:
        //      index   从指定的日志文件的什么记录偏移开始处理
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int ProcessServer(XmlNode server,
            string strStartDate,
            string strEndDate,
            long index,
            out string strLastDate,
            out long last_index,
            out string strError)
        {
            strError = "";
            strLastDate = "";
            last_index = -1;    // -1 表示尚未处理

            string strUrl = DomUtil.GetAttr(server, "url");
            if (string.IsNullOrEmpty(strUrl) == true)
            {
                strError = "中心服务器配置片断 '"+server.OuterXml+"' 中缺乏有效的 url 属性值";
                return -1;
            }

            string strServerName = DomUtil.GetAttr(server, "name");
            this.AppendResultText("从服务器 " + strServerName + " 同步\r\n");

            string strWarning = "";
            List<string> dates = null;
            int nRet = MakeLogFileNames(strStartDate,
                strEndDate,
                false,  // 是否包含扩展名 ".log"
                out dates,
                out strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            LibraryChannel channel = new LibraryChannel();
            channel.Url = strUrl;

            this.m_strUrl = strUrl;
            this.m_strUserName = DomUtil.GetAttr(server, "username");
            this.m_strPassword = LibraryApplication.DecryptPassword(DomUtil.GetAttr(server, "password"));

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            try
            {
                foreach (string strCurrentDate in dates)
                {
                    if (this.Stopped == true)
                        return 0;

                    this.AppendResultText("    日志文件 " + strCurrentDate + "  ");
                    long lProcessCount = 0;

                    // 记忆
                    strLastDate = strCurrentDate;
                    last_index = index;

                    long lIndex = index;
                    long lHint = -1;
                    for (; ; )
                    {
                        if (this.Stopped == true)
                            return 0;

                        DigitalPlatform.LibraryClient.localhost.OperLogInfo[] records = null;

                        // 获得日志
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   succeed
                        //      2   超过范围，本次调用无效
                        long lRet = channel.GetOperLogs(
                            null,
                            strCurrentDate + ".log",
                            lIndex,
                            lHint,
                            -1,
                            "level-0",
                            "setBiblioInfo",
                            out records,
                            out strError);
                        if (lRet == -1)
                        {
                            // return -1;
                            string strErrorText = "同步过程中 GetOperLogs " + strServerName + " " + strCurrentDate + " index=" + lIndex.ToString() + " error: " + strError + "\r\n";
                            this.AppendResultText(strErrorText);
                            this.App.WriteErrorLog(strErrorText);
                            break;
                        }
                        if (lRet == 0 || lRet == 2)
                            break;

                        if (records == null || records.Length == 0)
                        {
                            strError = "records error";
                            return -1;
                        }

                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.OperLogInfo record in records)
                        {
                            if (this.Stopped == true)
                                return 0;

                            if (string.IsNullOrEmpty(record.Xml) == true)
                                continue;

                            SetProgressText(strServerName + " " + strCurrentDate + " 记录" + (lIndex + i).ToString());

                            nRet = ProcessLogRecord(
                                strServerName,
                                record,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            lProcessCount++;

                            // 记忆
                            strLastDate = strCurrentDate;
                            last_index = lIndex + i + 1;
                        }

                        lHint = records[records.Length - 1].HintNext;
                        lIndex += records.Length;
                    }
                    index = 0;  // 第一个日志文件后面的，都从头开始了

                    this.AppendResultText(false, " (" + lProcessCount.ToString() + ")\r\n");
                }
            }
            finally
            {
                channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                channel.Close();
            }

            return 1;
        }

        string m_strUserName = "";
        string m_strPassword = "";
        string m_strUrl = "";

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = m_strUserName;
            e.Password = m_strPassword;

            e.Parameters = "";
            e.Parameters += ",client=dp2library|" + LibraryApplication.Version;

            e.LibraryServerUrl = m_strUrl;
        }

        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int ProcessLogRecord(
            string strServer,
            DigitalPlatform.LibraryClient.localhost.OperLogInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(info.Xml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(info.Xml);
            }
            catch (Exception ex)
            {
                strError = "日志记录装载到DOM时出错: " + ex.Message;
                return -1;
            }

            string strOperation = DomUtil.GetElementText(dom.DocumentElement,
    "operation");
            if (strOperation == "setBiblioInfo")
            {
                nRet = this.RecoverSetBiblioInfo(
                    strServer,
                    this.RmsChannels,
                    this.ReplicationLevel,
                    dom,
                    out strError);
            }

            if (nRet == -1)
            {
                string strAction = DomUtil.GetElementText(dom.DocumentElement,
                        "action");
                strError = "operation=" + strOperation + ";action=" + strAction + ": " + strError;
                return -1;
            }

            return 1;
        }



        // SetBiblioInfo() API 或 CopyBiblioInfo() API 的恢复动作
        // 函数内，使用return -1;还是goto ERROR1; 要看错误发生的时候，是否还有价值继续探索SnapShot重试。如果是，就用后者。
        /*
<root>
  <operation>setBiblioInfo</operation> 
  <action>...</action> 具体动作 有 new/change/delete/onlydeletebiblio 和 onlycopybiblio/onlymovebiblio/copy/move
  <record recPath='中文图书/3'>...</record> 记录体 动作为new/change/ *move* / *copy* 时具有此元素(即delete时没有此元素)
  <oldRecord recPath='中文图书/3'>...</oldRecord> 被覆盖、删除或者移动的记录 动作为change/ *delete* / *move* / *copy* 时具备此元素
  <deletedEntityRecords> 被删除的实体记录(容器)。只有当<action>为delete时才有这个元素。
	  <record recPath='中文图书实体/100'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。
	  ...
  </deletedEntityRecords>
  <copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </copyEntityRecords>
  <moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </moveEntityRecords>
  <copyOrderRecords /> <moveOrderRecords />
  <copyIssueRecords /> <moveIssueRecords />
  <copyCommentRecords /> <moveCommentRecords />
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>

逻辑恢复delete操作的时候，检索出全部下属的实体记录删除。
快照恢复的时候，可以根据operlogdom直接删除记录了path的那些实体记录
         * */
        public int RecoverSetBiblioInfo(
            string strServer,
            RmsChannelCollection Channels,
            ReplicationLevel level,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = Channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            // 快照恢复
            {
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                if (strAction == "new" || strAction == "change")
                {
                    XmlNode node = null;
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        goto ERROR1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 将远程书目库名替换为本地书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的本地书目库
                    //      1   找到，并已经替换
                    nRet = this.App.ReplaceBiblioRecPath(
                        strServer,
                        ref strRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    
                    if (string.IsNullOrEmpty(strRecPath) == true)
                        return 0;   // 轮空

                    string strTimestamp = DomUtil.GetAttr(node, "timestamp");

                    // TODO: 把书目记录写为指定的时间戳?
                    // 写书目记录
                    lRet = channel.DoSaveTextRes(strRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp,forcesettimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "写入书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }
                }
                else if (strAction == "onlymovebiblio"
                    || strAction == "onlycopybiblio"
                    || strAction == "move"
                    || strAction == "copy")
                {
                    XmlNode node = null;
                    string strTargetRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        goto ERROR1;
                    }
                    string strTargetRecPath = DomUtil.GetAttr(node, "recPath");

                    // 将远程书目库名替换为本地书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的本地书目库
                    //      1   找到，并已经替换
                    nRet = this.App.ReplaceBiblioRecPath(
                        strServer,
                        ref strTargetRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (string.IsNullOrEmpty(strTargetRecPath) == true)
                        return 0;

                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strOldRecPath = DomUtil.GetAttr(node, "recPath");

                    // 将远程书目库名替换为本地书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的本地书目库
                    //      1   找到，并已经替换
                    nRet = this.App.ReplaceBiblioRecPath(
                        strServer,
                        ref strOldRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    bool bSourceExist = true;
                    // 观察源记录是否存在
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                        bSourceExist = false;
                    else
                    {
                        string strMetaData = "";
                        string strXml = "";
                        byte[] temp_timestamp = null;

                        lRet = channel.GetRes(strOldRecPath,
                            out strXml,
                            out strMetaData,
                            out temp_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFoundOrDamaged())
                            {
                                bSourceExist = false;
                            }
                        }
                    }

                    if (bSourceExist == true
                        && string.IsNullOrEmpty(strTargetRecPath) == false)
                    {
                        // 注: 实际上是从本地复制到本地
                        // 复制书目记录
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strTargetRecPath,
                            strAction == "onlymovebiblio" ? true : false,   // bDeleteSourceRecord
                            "file_reserve_source",  // 2024/4/28
                            out _,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "DoCopyRecord() error :" + strError;
                            goto ERROR1;
                        }
                    }

                    /*
                    // 写书目记录
                    lRet = channel.DoSaveTextRes(strRecPath,
                        strRecord,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "复制书目记录 '" + strOldRecPath + "' 到 '" + strTargetRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }                     * */


                    // 准备需要写入目标位置的记录
                    if (bSourceExist == false)
                    {
                        if (String.IsNullOrEmpty(strTargetRecord) == true)
                        {
                            if (String.IsNullOrEmpty(strOldRecord) == true)
                            {
                                strError = "源记录 '" + strOldRecPath + "' 不存在，并且<record>元素无文本内容，这时<oldRecord>元素也无文本内容，无法获得要写入的记录内容";
                                return -1;
                            }

                            strTargetRecord = strOldRecord;
                        }
                    }

                    // 如果有“新记录”内容
                    if (string.IsNullOrEmpty(strTargetRecPath) == false
                        && String.IsNullOrEmpty(strTargetRecord) == false)
                    {

                        // 写书目记录
                        lRet = channel.DoSaveTextRes(strTargetRecPath,
                            strTargetRecord,
                            false,
                            "content,ignorechecktimestamp,forcesettimestamp",
                            timestamp,  // TODO: 这是哪条记录的时间戳?
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "写书目记录 '" + strTargetRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }

                    // 复制或者移动下级子记录
                    // 复制的情况下，远端涉及到修改复制后的目标实体记录，这些都是本地不可感知的，所以复制情况下
                    // 的子记录复制动作被省略了。
                    // 移动的情况下，要移动本地的下级子记录
                    if (strAction == "move"
                    || strAction == "copy")
                    {
                        string strWarning = "";

                        // parameters:
                        //      strAction   copy / move
                        // return:
                        //      -2  权限不够
                        //      -1  出错
                        //      0   成功
                        nRet = DoCopySubRecord(
                            channel,
                            strAction,
                            strOldRecPath,
                            strTargetRecPath,
                            out strWarning,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    // 2011/12/12
                    if (bSourceExist == true
                        && (strAction == "move" || strAction == "onlymovebiblio")
                        )
                    {
                        int nRedoCount = 0;
                    REDO_DELETE:
                        // 删除源书目记录
                        lRet = channel.DoDeleteRes(strOldRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                            {
                                // 记录本来就不存在
                            }
                            else if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO_DELETE;
                                }
                            }
                            else
                            {
                                strError = "删除书目记录 '" + strOldRecPath + "' 时发生错误: " + strError;
                                return -1;
                            }
                        }
                    }
                }
                else if (strAction == "delete"
                    || strAction == "onlydeletebiblio")
                {
                    XmlNode node = null;
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 将远程书目库名替换为本地书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的本地书目库
                    //      1   找到，并已经替换
                    nRet = this.App.ReplaceBiblioRecPath(
                        strServer,
                        ref strRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (string.IsNullOrEmpty(strRecPath) == false)
                    {
                        int nRedoCount = 0;
                    REDO:
                        // 删除书目记录
                        lRet = channel.DoDeleteRes(strRecPath,
                            timestamp,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.IsNotFound())
                                goto DO_DELETE_CHILD_ENTITYRECORDS;   // 记录本来就不存在
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount < 10)
                                {
                                    timestamp = output_timestamp;
                                    nRedoCount++;
                                    goto REDO;
                                }
                            }
                            strError = "删除书目记录 '" + strRecPath + "' 时发生错误: " + strError;
                            return -1;
                        }
                    }

                DO_DELETE_CHILD_ENTITYRECORDS:

                    if (strAction == "delete")
                    {
                        // 删除属于同一书目记录的全部实体记录
                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.App.DeleteBiblioChildEntities(channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的实体记录时出错: " + strError;
                            goto ERROR1;
                        }

                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.App.OrderItemDatabase.DeleteBiblioChildItems(
                            // Channels,
                            channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的订购记录时出错: " + strError;
                            goto ERROR1;
                        }

                        // return:
                        //      -1  error
                        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
                        //      >0  实际删除的实体记录数
                        nRet = this.App.IssueItemDatabase.DeleteBiblioChildItems(
                            // Channels,
                            channel,
                            strRecPath,
                            null,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "删除书目记录 '" + strRecPath + "' 下属的期记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }
                }

                return 0;
            }
            return 0;
        ERROR1:
            return -1;
        }

        // parameters:
        //      strAction   copy / move
        // return:
        //      -2  权限不够
        //      -1  出错
        //      0   成功
        int DoCopySubRecord(
            RmsChannel channel,
            string strAction,
            string strBiblioRecPath,
            string strNewBiblioRecPath,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 1)
            // 探测书目记录有没有下属的实体记录(也顺便看看实体记录里面是否有流通信息)?
            List<DeleteEntityInfo> entityinfos = null;
            long lHitCount = 0;

            // TODO: 只要获得记录路径即可，因为后面利用了CopyRecord复制
            // return:
            //      -2  not exist entity dbname
            //      -1  error
            //      >=0 含有流通信息的实体记录个数
            nRet = this.App.SearchChildEntities(
                null,
                channel,
                strBiblioRecPath,
                "count_borrow_info,return_record_xml",  // "count_borrow_info,return_record_xml",
                null,
                null,
                out lHitCount,
                out entityinfos,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == -2)
            {
                Debug.Assert(entityinfos.Count == 0, "");
            }

            int nBorrowInfoCount = nRet;

            // 2)
            // 探测书目记录有没有下属的订购记录
            List<DeleteEntityInfo> orderinfos = null;
            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.App.OrderItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml", // "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out orderinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(orderinfos.Count == 0, "");
            }

            // 3)
            // 探测书目记录有没有下属的期记录
            List<DeleteEntityInfo> issueinfos = null;

            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.App.IssueItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml", // "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out issueinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(issueinfos.Count == 0, "");
            }

            // 4)
            // 探测书目记录有没有下属的评注记录
            List<DeleteEntityInfo> commentinfos = null;
            // return:
            //      -1  error
            //      0   not exist entity dbname
            //      1   exist entity dbname
            nRet = this.App.CommentItemDatabase.SearchChildItems(
                null,
                channel,
                strBiblioRecPath,
                "return_record_xml", // "return_record_xml,check_circulation_info",
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out commentinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                Debug.Assert(commentinfos.Count == 0, "");
            }


            // ** 第二阶段
            string strTargetBiblioDbName = DigitalPlatform.rms.Client.ResPath.GetDbName(strNewBiblioRecPath);

            if (entityinfos != null && entityinfos.Count > 0)
            {
                // TODO: 如果是复制, 则要为目标实体记录的测条码号增加一个前缀。或者受到strStyle控制，能决定在source或者target中加入前缀

                // 复制属于同一书目记录的全部实体记录
                // parameters:
                //      strAction   copy / move
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.App.CopyBiblioChildEntities(channel,
                    strAction,
                    entityinfos,
                    strNewBiblioRecPath,
                    null,   // domOperLog,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                {
                    // TODO: 需要检查源实体记录中是否至少有一个包含流通信息。如果有，则这样丢失它们意味着流通信息的丢失，这是不能允许的
                    if (nBorrowInfoCount > 0
                        && strAction == "move")
                    {
                        strError = "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，(移动操作)将丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录。但这些实体记录中已经存在有 " + nBorrowInfoCount.ToString() + " 个流通信息，这意味着这些实体记录不能消失。因此移动操作被迫放弃";
                        goto ERROR1;
                    }

                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的实体库，已丢失来自源书目库下属的 " + entityinfos.Count + " 条实体记录; ";
                }
            }

            if (orderinfos != null && orderinfos.Count > 0)
            {
                // 复制订购记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.App.OrderItemDatabase.CopyBiblioChildItems(channel,
                strAction,
                orderinfos,
                strNewBiblioRecPath,
                null,   // domOperLog,
                out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的订购库，已丢失来自源书目库下属的 " + orderinfos.Count + " 条订购记录; ";
                }
            }

            if (issueinfos != null && issueinfos.Count > 0)
            {
                // 复制期记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.App.IssueItemDatabase.CopyBiblioChildItems(channel,
            strAction,
            issueinfos,
            strNewBiblioRecPath,
            null,   // domOperLog,
            out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    if (orderinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的期库，已丢失来自源书目库下属的 " + issueinfos.Count + " 条期记录; ";
                }
            }

            if (commentinfos != null && commentinfos.Count > 0)
            {
                // 复制评注记录
                // return:
                //      -2  目标实体库不存在，无法进行复制或者删除
                //      -1  error
                //      >=0  实际复制或者移动的实体记录数
                nRet = this.App.CommentItemDatabase.CopyBiblioChildItems(channel,
            strAction,
            commentinfos,
            strNewBiblioRecPath,
            null,   // domOperLog,
            out strError);
                if (nRet == -1)
                {
                    if (entityinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + entityinfos.Count.ToString() + " 个册记录已经无法恢复";
                    if (orderinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + orderinfos.Count.ToString() + " 个订购记录已经无法恢复";
                    if (issueinfos.Count > 0)
                        strError += "；\r\n刚" + strAction + "的 " + issueinfos.Count.ToString() + " 个期记录已经无法恢复";
                    goto ERROR1;
                }
                if (nRet == -2)
                {
                    strWarning += "目标书目库 '" + strTargetBiblioDbName + "' 没有下属的评注库，已丢失来自源书目库下属的 " + commentinfos.Count + " 条评注记录; ";
                }

            }

            return 0;
        ERROR1:
            return -1;
        }

        /// <summary>
        /// 根据日期范围，发生日志文件名
        /// </summary>
        /// <param name="strStartDate">起始日期。8字符</param>
        /// <param name="strEndDate">结束日期。8字符</param>
        /// <param name="bExt">是否包含扩展名 ".log"</param>
        /// <param name="LogFileNames">返回创建的文件名</param>
        /// <param name="strWarning">返回警告信息</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int MakeLogFileNames(string strStartDate,
            string strEndDate,
            bool bExt,  // 是否包含扩展名 ".log"
            out List<string> LogFileNames,
            out string strWarning,
            out string strError)
        {
            LogFileNames = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strLogFileName = strStartDate;

            for (; ; )
            {
                LogFileNames.Add(strLogFileName + (bExt == true ? ".log" : ""));

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
                {
                    if (String.Compare(strLogFileName, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                strLogFileName = strNextLogFileName;
                if (String.Compare(strLogFileName, strEndDate) > 0)
                    break;
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
                strError = "日期 " + strLogFileName + " 格式错误: " + ex.Message;
                return -1;
            }

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = DateTimeUtil.RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = DateTimeUtil.RoundTime("day",
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

        // 记忆一下断点，以备不测
        void SaveBreakPoint(BreakPointCollcation infos)
        {
            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.Name,
                infos.ToString());
        }

        // 构造用于复制然后同步的断点信息
        BreakPointCollcation BuildCopyAndContinue(string strEndDate)
        {
            BreakPointCollcation infos = new BreakPointCollcation();
            XmlNodeList nodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("center/server");
            foreach (XmlNode server in nodes)
            {
                string strServerName = DomUtil.GetAttr(server, "name");

                BreakPointInfo info = new BreakPointInfo();
                info.ServerName = strServerName;
                info.RecID = "1";

                info.BiblioDbName = "*";    // 表示从第一个库开始
                info.RecID = "*";   // 表示从第一条开始

                info.Date = strEndDate;

                string strError = "";
                // 探测此刻的日志文件总记录数
                // 获得日志文件中记录的总数
                // parameters:
                //      strDate 日志文件的日期，8 字符
                // return:
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                long lRet = GetOperLogCount(server,
                    info.Date,
                    out strError);
                if (lRet == -1)
                    throw new Exception(strError);

                info.Index = lRet;

                infos.Add(info);
            }

            return infos;
        }

        // 获得日志文件中记录的总数
        // parameters:
        //      strDate 日志文件的日期，8 字符
        // return:
        //      -1  出错
        //      0   日志文件不存在，或者记录数为 0
        //      >0  记录数
        long GetOperLogCount(XmlNode server,
            string strDate,
            out string strError)
        {
            strError = "";

            string strUrl = DomUtil.GetAttr(server, "url");
            if (string.IsNullOrEmpty(strUrl) == true)
            {
                strError = "中心服务器配置片断 '" + server.OuterXml + "' 中缺乏有效的 url 属性值";
                return -1;
            }

            LibraryChannel channel = new LibraryChannel();
            channel.Url = strUrl;

            this.m_strUrl = strUrl;
            this.m_strUserName = DomUtil.GetAttr(server, "username");
            this.m_strPassword = LibraryApplication.DecryptPassword(DomUtil.GetAttr(server, "password"));

            channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            try
            {
                string strXml = "";
                long lAttachmentTotalLength = 0;
                byte[] attachment_data = null;

                long lRecCount = 0;

                // 获得日志文件尺寸
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                long lRet = channel.GetOperLog(
                    null,
                    strDate + ".log",
                    -1,    // lIndex,
                    -1, // lHint,
                    "getcount",
                    "", // strFilter
                    out strXml,
                    out lRecCount,
                    0,  // lAttachmentFragmentStart,
                    0,  // nAttachmentFramengLength,
                    out attachment_data,
                    out lAttachmentTotalLength,
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
            finally
            {
                channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                channel.Close();
            }
        }
    }

    public enum ReplicationLevel
    {
        Full = 0,  // 全部同步
        Used = 1,   // 参与流通的记录才同步
    }

    // 一个服务器的断点信息
    class BreakPointInfo
    {
        public string ServerName = "";  // 中心服务器名

        public string BiblioDbName = "";    // 正在复制的书目库名。这是本地数据库名。如果为空表示不复制，而直接进入同步阶段
        public string RecID = "";       // 已经复制到的 ID

        public string Date = "";    // 8 字符
        public long Index = 0;      // 日志记录 index

        // 通过字符串构造
        public static BreakPointInfo Build(string strText)
        {
            Hashtable table = StringUtil.ParseParameters(strText);

            BreakPointInfo info = new BreakPointInfo();
            info.ServerName = (string)table["server"];
            info.BiblioDbName = (string)table["dbname"];
            info.RecID = (string)table["recid"];
            info.Date = (string)table["date"];
            string strIndex = (string)table["index"];
            long v = 0;
            if (long.TryParse(strIndex, out v) == false)
                throw new Exception("index 参数值 '" + strIndex + "' 格式不合法，应该为纯数字");
            info.Index = v;

            return info;
        }

        // 变换为字符串
        public override string ToString()
        {
            Hashtable table = new Hashtable();

            table["server"] = this.ServerName;
            table["dbname"] = this.BiblioDbName;
            table["recid"] = this.RecID;
            table["date"] = this.Date;
            table["index"] = this.Index.ToString();

            return StringUtil.BuildParameterString(table);
        }

        // 小结文字
        public string GetSummary()
        {
            string strResult = "";
            strResult += "针对服务器 " + this.ServerName + ": ";
            if (string.IsNullOrEmpty(this.BiblioDbName) == false)
            {
                string strRange = "";
                if (string.IsNullOrEmpty(this.RecID) == true || this.RecID == "*")
                    strRange = "全部";
                else
                    strRange = "从 ID " + this.RecID + "开始的";

                if (this.BiblioDbName == "*")
                    strResult += "\r\n  * 复制全部关联的书目库，每个库的范围是" + strRange + "记录";
                else
                    strResult += "\r\n  * 复制记录到本地书目库 " + this.BiblioDbName + " 开始的若干个关联书目库，第一个库范围是" + strRange + "记录，(若有)后继的库是全部记录";
            }

            if (string.IsNullOrEmpty(this.Date) == false)
            {
                strResult += "\r\n  * 同步日志文件 " + this.Date + "，从偏移 " + this.Index.ToString() + " 开始";
            }

            return strResult;
        }
    }

    // 若干服务器的断点信息
    class BreakPointCollcation : List<BreakPointInfo>
    {
        // 根据服务器名找到断点信息
        public BreakPointInfo GetBreakPoint(string strServerName)
        {
            foreach (BreakPointInfo info in this)
            {
                if (info.ServerName == strServerName)
                    return info;
            }

            return null;
        }

        // 通过字符串构造
        public static BreakPointCollcation Build(string strText)
        {
            BreakPointCollcation infos = new BreakPointCollcation();

            string[] segments = strText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in segments)
            {
                infos.Add(BreakPointInfo.Build(segment));
            }

            return infos;
        }

        // 变换为字符串
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            foreach (BreakPointInfo info in this)
            {
                text.Append(info.ToString() + ";");
            }

            return text.ToString();
        }

        // 小结文字
        public string GetSummary()
        {
            StringBuilder text = new StringBuilder();
            foreach (BreakPointInfo info in this)
            {
                text.Append(info.GetSummary() + "\r\n");
            }

            return text.ToString();
        }
    }
}
