using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Threading;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.rms;

namespace DigitalPlatform.LibraryServer
{
#if NO
    /// <summary>
    /// 负责创建缓存的线程
    /// </summary>
    public class CacheBuilder : BatchTask
    {
        public CacheBuilder(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "创建缓存";
            }
        }


        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;

            string strError = "";
            int nRet = 0;

            bool bPerDayStart = false;  // 是否为每日一次启动模式
            string strMonitorName = "cacheBuilder";
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
                bool bRet = false;
                nRet = IsNowAfterPerDayStart(
                    strMonitorName,
                    strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "获取 "+strMonitorName+" 每日启动时间时发生错误: " + strError;
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
                    bPerDayStart = true;

                    if (bRet == false)
                    {
                        if (this.ManualStart == true)
                            this.AppendResultText("已试探启动任务 '" + this.Name + "'，但因没有到每日启动时间 " + strStartTimeDef + " 而未能启动。(上次任务处理结束时间为 " + DateTimeUtil.LocalTime(strLastTime) + ")\r\n");

                        return; // 还没有到每日时间
                    }
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + " 启动。");
            }

            this.AppendResultText("开始新一轮循环\r\n");

            int nTotalRecCount = 0;
            for (int i = 0; ; i++)
            {
                // 系统挂起的时候，不运行本线程
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;

                if (this.m_bClosed == true)
                    break;

                if (this.Stopped == true)
                    break;

                string strLine = "";
                lock (this.App.PendingCacheFiles)
                {
                    if (this.App.PendingCacheFiles.Count == 0)
                        break;
                    strLine = this.App.PendingCacheFiles[0];

                    // 先不要从队列里面删除这个行，以保证和后面将要加入的相排斥
                }

                bool bDone = false;

                string strPart = "";
                try
                {
                    string[] parts = strLine.Split(new char[] { ':' });
                    if (parts.Length < 2)
                    {
                        strError = "parts format error";
                        goto ERROR1;
                    }

                    string strDataFile = "";
                    string strNodePath = "";

                    if (parts.Length > 0)
                        strDataFile = parts[0];
                    if (parts.Length > 1)
                        strNodePath = parts[1];
                    if (parts.Length > 2)
                        strPart = parts[2];


                    int nCount = 0;

                    this.AppendResultText("*** 处理事项" + " " + strLine + "\r\n");
                    nRet = BuildOneCache(
                        strDataFile,
                        strNodePath,
                        strPart,
                        out nCount,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    bDone = true;
                    nTotalRecCount += nCount;
                }
                finally
                {
                    // 从队列中删除刚才完成了的事项
                    lock (this.App.PendingCacheFiles)
                    {
                        if (this.App.PendingCacheFiles.Count != 0)
                        {
                            nRet = this.App.PendingCacheFiles.IndexOf(strLine);
                            if (nRet != -1)
                                this.App.PendingCacheFiles.RemoveAt(nRet);
                            // 如果完成了全部文件，则需要删除后面排队的单纯rss刷新请求。因为rss已经被连带刷新了。
                            if (bDone == true
                                && String.IsNullOrEmpty(strPart) == true)
                            {
                                nRet = this.App.PendingCacheFiles.IndexOf(strLine + ":rss");
                                if (nRet != -1)
                                    this.App.PendingCacheFiles.RemoveAt(nRet);
                            }
                        }
                    }

                    if (bDone == false)
                    {
                        string strErrorText = "队列事项 '"+strLine+"' 因为发生错误 '"+strError+"' 而不得不删除(避免短时间内反复重做)...";
                        this.App.WriteErrorLog(strErrorText);
                    }
                }
            }

            AppendResultText("循环结束。共处理 " + nTotalRecCount.ToString() + " 条记录。\r\n");

            {
                Debug.Assert(this.App != null, "");

                // 写入文件，记忆已经做过的当日时间
                string strLastTime = DateTimeUtil.Rfc1123DateTimeString(this.App.Clock.UtcNow);  // 2007/12/17 changed // DateTime.UtcNow
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(定时)" : "(不定时)") + strMonitorName + "结束。共处理记录 " + nTotalRecCount.ToString() + " 个。";
                this.App.WriteErrorLog(strErrorText);

            }
            return;
        ERROR1:
            AppendResultText("CacheBuilder thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("CacheBuilder thread error : " + strError + "\r\n");
            return;
        }

        // 比较文件创建时间和当前时间，看看是否超过重建周期
        public static bool HasExpired(string strFilename,
            string strBuildStyle)
        {
            if (String.IsNullOrEmpty(strBuildStyle) == true)
                return false;

            TimeSpan delta;
            try
            {
                FileInfo fi = new FileInfo(strFilename);
                delta = DateTime.Now - fi.LastWriteTime;
            }
            catch
            {
                return false;
            }

            if (strBuildStyle.ToLower() == "perhour")
            {
                if (delta.TotalHours >= 1)
                    return true;
            }
            else if (strBuildStyle.ToLower() == "perday")
            {
                if (delta.TotalDays >= 1)
                    return true;
            }
            else if (strBuildStyle.ToLower() == "perweek")
            {
                if (delta.TotalDays >= 7)
                    return true;
            }
            else if (strBuildStyle.ToLower() == "permonth")
            {
                if (delta.TotalDays >= 30)
                    return true;
            }
            else if (strBuildStyle.ToLower() == "peryear")
            {
                if (delta.TotalDays >= 365)
                    return true;
            }
            else
            {
                return false;   // 不支持的strBuildStyle
            }

            return false;
        }

        // 获得Build相关参数
        // parameters:
        //      strBuildStyle    创建风格 perday / perhour
        public static void GetBuildParam(XmlNode node,
            out string strBuildStyle)
        {
            strBuildStyle = "";

            string strBuild = DomUtil.GetAttr(node, "build");

            Hashtable param_table = StringUtil.ParseParameters(strBuild);

            // env_param_table 环境变量表
            Hashtable env_param_table = GetBuildEnvParamTable(node);

            // 合并两个参数表
            Hashtable result = MergeTwoTable(param_table, env_param_table);

            strBuildStyle = (string)result["autoUpdate"];
        }

        // 获得RSS相关参数
        // parameters:
        //      nMaxCount   -1表示无穷多
        //      strDirection    head/tail
        public static void GetRssParam(XmlNode node,
            out bool bEnable,
            out long nMaxCount,
            out string strDirection)
        {
            bEnable = false;
            nMaxCount = -1;
            strDirection = "head";

            string strRss = DomUtil.GetAttr(node, "rss");

            Hashtable param_table = StringUtil.ParseParameters(strRss);

            // env_param_table 环境变量表
            Hashtable env_param_table = GetRssEnvParamTable(node);

            // 合并两个参数表
            Hashtable result = MergeTwoTable(param_table, env_param_table);

            string strEnable = (string)result["enable"];
            if (strEnable != null)
                strEnable = strEnable.ToLower();
            if (strEnable == "on" || strEnable == "true")
                bEnable = true;

            string strMaxCount = (string)result["maxcount"];

            if (String.IsNullOrEmpty(strMaxCount) == true)
                return;

            if (strMaxCount[0] == '-')
            {
                strDirection = "tail";
                strMaxCount = strMaxCount.Substring(1);
            }
            else
                strDirection = "head";

            if (String.IsNullOrEmpty(strMaxCount) == true)
                nMaxCount = -1;
            else
                nMaxCount = Convert.ToInt64(strMaxCount);
        }

        // 创建一个缓存(一套)
        // return:
        //      -1  error
        //      0   成功
        //      1   相关文件被占用
        //      2   没有必要输出
        int BuildOneCache(
            string strDataFile,
            string strNode,
            string strPart,
            out int nCount,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            nCount = 0;

            string strDataFilePath = this.App.DataDir + "/browse/" + strDataFile;


            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strDataFilePath);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strDataFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            XmlNode node = GetDataNode(dom.DocumentElement,
                strNode);

            if (node == null)
            {
                strError = "路径 '" + strNode + "' 在文件 '" + strDataFile + "' 中没有找到对应的节点";
                return -1;
            }

            bool bRss = false;
            long nMaxCount = -1;
            string strDirection = "";
            // parameters:
            //      nMaxCount   -1表示无穷多
            //      strDirection    head/tail
            GetRssParam(node,
                out bRss,
                out nMaxCount,
                out strDirection);

            string strCommand = DomUtil.GetAttr(node, "command");
            string strPureCaption = DomUtil.GetAttr(node, "name");
            string strDescription = DomUtil.GetAttr(node, "description");

            if (strCommand == "~hidelist~")
            {
                strError = "此节点 ~hidelist~ 不必创建缓存";
                return 2;
            }

            if (strCommand == "~none~")
            {
                strError = "此节点 ~none~ 不必创建缓存";
                return 2;
            }

            // strDataFile 中为纯文件名

            string strPrefix = strNode;
            string strCacheDir = this.App.DataDir + "/browse/cache/" + strDataFile;

            PathUtil.CreateDirIfNeed(strCacheDir);
            string strResultsetFilename = strCacheDir + "/" + strPrefix;
            string strTempResultsetFilename = strCacheDir + "/_temp_" + strPrefix;

            string strRssString = "datafile=" + strDataFile + "&node=" + strPrefix;
            // this.HyperLink_rss.NavigateUrl = "browse.aspx?action=rss&" + strRssString;

            string strChannelLink = this.App.OpacServerUrl + "/browse.aspx?datafile=" + strDataFile
                + "&node=" + strNode;
            string strSelfLink = this.App.OpacServerUrl + "/browse.aspx?action=rss&datafile=" + strDataFile
                + "&node=" + strNode;

            /*
            // 如果文件已经存在，就不要从dp2kernel获取了
            if (File.Exists(strResultsetFilename) == true)
            {
                // 从结果集文件输出RSS内容
                nRet = BuildRss(strResultsetFilename,
                    strPureCaption,
                    strChannelLink,
                    strSelfLink,
                    strDescription,
                    out strError);
                if (nRet == -1)
                    return -1;
                return 0;
            }*/

            bool bError = false;

            // 如果文件已经存在，就不要从dp2kernel获取了
            if (File.Exists(strResultsetFilename) == true
                && strPart == "rss")
            {
                if (bRss == false)
                {
                    // TODO: 警告这种矛盾
                }

                // 复制文件，用临时文件为创建RSS文件的源用
                // 这样可以减少锁定冲突
                File.Copy(strResultsetFilename,
                    strTempResultsetFilename,
                    true);
                File.Copy(
                    strResultsetFilename + ".index",
                    strTempResultsetFilename + ".index",
                    true);
                bool bDone = false;
                try
                {
                    SetProgressText("创建RSS文件" + " " + strPureCaption + " -- "+ strResultsetFilename);
                    this.AppendResultText("创建RSS文件" + " " + strPureCaption + " -- " + strResultsetFilename + "\r\n");

                    // 从结果集文件输出RSS内容
                    nRet = BuildRssFile(strTempResultsetFilename,
                        nMaxCount,
                        strDirection,
                        strPureCaption,
                        strChannelLink,
                        strSelfLink,
                        strDescription,
                        strTempResultsetFilename + ".rss",
                        out nCount,
                        out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }
                    bDone = true;
                    this.AppendResultText("  包含记录 " + nCount.ToString() + " 条\r\n");
                }
                finally
                {
                    // 替换
                    try
                    {
                        this.App.ResultsetLocks.LockForWrite(strResultsetFilename + ".rss",
                            500);
                        try
                        {
                            if (bDone == true)
                            {
                                File.Delete(strResultsetFilename + ".rss");
                                File.Move(strTempResultsetFilename + ".rss", strResultsetFilename + ".rss");
                            }
                            // 删除临时文件
                            File.Delete(strTempResultsetFilename);
                            File.Delete(strTempResultsetFilename + ".index");
                        }
                        finally
                        {
                            this.App.ResultsetLocks.UnlockForWrite(strResultsetFilename + ".rss");
                        }
                    }
                    catch (System.ApplicationException)
                    {
                        bError = true;
                        strError = "相关文件被占用";
                        // TODO: 怎么善后?
                    }
                }

                if (bError == true)
                    return 1;
            }
            else
            {
                int nResultSetCount = 0;
                int nRssCount = 0;

                /*
                nRet = this.App.InitialVdbs(this.RmsChannels,
            out strError);
                if (nRet == -1)
                {
                    strError = "InitialVdbs error : " + strError;
                    return -1;
                }
                 * */
                if (this.App.vdbs == null)
                {
                    strError = "vdbs == null";
                    return -1;
                }

                string strXml = "";
                nRet = BuildXmlQuery(node,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                string strResultSetName = "opac_browse_" + strPureCaption;

                channel.Idle += new IdleEventHandler(channel_Idle);
                try
                {
                    long lRet = 0;
                    int nRedoCount = 0;

                REDO_SEARCH:

                    SetProgressText("检索" + " " + strPureCaption);
                    this.AppendResultText("检索" + " " + strPureCaption + "\r\n");

                    lRet = channel.DoSearch(strXml,
                        strResultSetName,
                        "", // strOutputStyle
                        out strError);
                    if (lRet == -1)
                    {
                        // 超时处理
                        if (channel.ErrorCode == ChannelErrorCode.RequestTimeOut
                            && nRedoCount < 5)
                        {
                            channel.Abort();
                            // channel.Timeout *= 10;
                            // TODO: 找到解决办法
                            nRedoCount++;
                            this.AppendResultText("警告：检索发生超时" + "\r\n");
                            goto REDO_SEARCH;
                        }


                        strError = "DoSearch() error : " + strError;
                        return -1;
                    }

                    this.AppendResultText("  命中记录 "+lRet.ToString()+" 条" + "\r\n");

                    SetProgressText("获得结果集文件" + " " + strPureCaption + " -- " + strResultsetFilename);
                    this.AppendResultText("获得结果集文件" + " " + strPureCaption + " -- " + strResultsetFilename + "\r\n");

                    nRet = GetResultset(channel,
                        strResultSetName,
                        strTempResultsetFilename,
                        out nResultSetCount,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (this.m_bClosed == true || this.Stopped == true)
                    {
                        strError = "中断";
                        return -1;
                    }

                    /*
                    // not found
                    if (lRet == 0)
                    {
                        return 0;
                    }*/
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_Idle);
                }

                // 替换两个文件
                try
                {
                    this.App.ResultsetLocks.LockForWrite(strResultsetFilename,
                        500);
                    try
                    {
                        File.Delete(strResultsetFilename);
                        File.Delete(strResultsetFilename + ".index");

                        if (bRss == true)
                        {
                            // 复制文件，留下临时文件为创建RSS文件作为源用
                            // 这样可以减少锁定冲突
                            File.Copy(strTempResultsetFilename, 
                                strResultsetFilename, 
                                true);
                            File.Copy(strTempResultsetFilename + ".index", 
                                strResultsetFilename + ".index",
                                true);
                        }
                        else
                        {
                            File.Move(strTempResultsetFilename, strResultsetFilename);
                            File.Move(strTempResultsetFilename + ".index", strResultsetFilename + ".index");
                        }
                    }
                    finally
                    {
                        this.App.ResultsetLocks.UnlockForWrite(strResultsetFilename);
                    }
                }
                catch (System.ApplicationException)
                {
                    strError = "相关文件被占用1";
                    // TODO: 怎么善后?
                    return 1;
                }

                if (bRss == true)
                {
                    bool bDone = false;
                    try
                    {
                        SetProgressText("创建RSS文件" + " " + strPureCaption + " -- " + strResultsetFilename);
                        this.AppendResultText("创建RSS文件" + " " + strPureCaption + " -- " + strResultsetFilename + "\r\n");

                        // 从结果集文件输出RSS内容
                        nRet = BuildRssFile(strTempResultsetFilename,
                        nMaxCount,
                        strDirection,
                            strPureCaption,
                            strChannelLink,
                            strSelfLink,
                            strDescription,
                            null,
                            out nRssCount,
                            out strError);
                        if (nRet == -1)
                        {
                            return -1;
                        }

                        bDone = true;
                        this.AppendResultText("  包含记录 "+nRssCount.ToString()+" 条\r\n");

                    }
                    finally
                    {
                        // 替换RSS文件
                        try
                        {
                            this.App.ResultsetLocks.LockForWrite(strResultsetFilename + ".rss",
                                500);
                            try
                            {
                                if (bDone == true)
                                {
                                    File.Delete(strResultsetFilename + ".rss");
                                    File.Move(strTempResultsetFilename + ".rss",
                                        strResultsetFilename + ".rss");
                                }

                                // 删除临时文件
                                File.Delete(strTempResultsetFilename);
                                File.Delete(strTempResultsetFilename + ".index");
                            }
                            finally
                            {
                                this.App.ResultsetLocks.UnlockForWrite(strResultsetFilename + ".rss");
                            }
                        }
                        catch (System.ApplicationException)
                        {
                            bError = true;
                            strError = "相关文件被占用2";
                            // TODO: 怎么善后?
                        }
                    }

                    if (bError == true)
                        return 1;

                }


                nCount = nResultSetCount + nRssCount;
            }


            return 0;
        }

        // 构造结果集文件名
        public static string MakeResultsetFilename(XmlNode node)
        {
            string strResult = "";
            while (node != null)
            {
                XmlNode parent = node.ParentNode;
                if (parent == null
                    || node == node.OwnerDocument.DocumentElement)
                {
                    if (String.IsNullOrEmpty(strResult) == true)
                        strResult = "0";
                    else
                        strResult = "0" + "_" + strResult;
                    return strResult;
                }

                for (int i = 0; i < parent.ChildNodes.Count; i++)
                {
                    if (parent.ChildNodes[i] == node)
                    {
                        if (String.IsNullOrEmpty(strResult) == true)
                            strResult = i.ToString();
                        else
                            strResult = i.ToString() + "_" + strResult;
                        break;
                    }
                }

                node = parent;
            }

            return strResult;
        }

        // parameters:
        //      bOnlyAppend ==true表示仅仅追究没有的问题，已经有的不再重复。==false，表示全部都刷新
        public static int RefreshAll(LibraryApplication app,
            string strDataFile,
            bool bOnlyAppend,
            out string strError)
        {
            strError = "";

            string strDataFilePath = app.DataDir + "/browse/" + strDataFile;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strDataFilePath);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strDataFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//*");
            List<string> lines = new List<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                if (node.Name == "_title")
                    continue;
                if (node.Name == "caption")
                    continue;

                bool bRss = false;
                long nMaxCount = -1;
                string strDirection = "";
                // parameters:
                //      nMaxCount   -1表示无穷多
                //      strDirection    head/tail
                GetRssParam(node,
                    out bRss,
                    out nMaxCount,
                    out strDirection);
                string strCommand = DomUtil.GetAttr(node, "command");
                if (strCommand == "~hidelist~")
                {
                    // strError = "此节点 ~hidelist~ 不必创建缓存";
                    continue;
                }

                if (strCommand == "~none~")
                {
                    // strError = "此节点 ~none~ 不必创建缓存";
                    continue;
                }

                /*
                string strPureCaption = DomUtil.GetAttr(node, "name");
                string strDescription = DomUtil.GetAttr(node, "description");
                */

                string strPrefix = MakeResultsetFilename(node);
                if (bOnlyAppend == true)
                {
                    // strDataFile 中为纯文件名
                    string strCacheDir = app.DataDir + "/browse/cache/" + strDataFile;

                    PathUtil.CreateDirIfNeed(strCacheDir);
                    string strResultsetFilename = strCacheDir + "/" + strPrefix;

                    string strRssString = "datafile=" + strDataFile + "&node=" + strPrefix;

                    if (File.Exists(strResultsetFilename) == true)
                    {
                        if (File.Exists(strResultsetFilename + ".index") == false)
                            goto DO_ADD;

                        if (File.Exists(strResultsetFilename + ".rss") == true)
                            continue;
                    }

                    string strLine = strDataFile + ":" + strPrefix + ":rss";
                    lines.Add(strLine.ToLower());
                    continue;
                }

            DO_ADD:
                {
                    string strLine = strDataFile + ":" + strPrefix;
                    lines.Add(strLine.ToLower());
                }
            }

                int nCount = 0;
            if (lines.Count > 0)
            {
                lock (app.PendingCacheFiles)
                {
                    foreach (string strLine in lines)
                    {
                        if (app.PendingCacheFiles.IndexOf(strLine) == -1)
                        {
                            nCount++;
                            app.PendingCacheFiles.Add(strLine);
                        }
                    }
                }
                /*
                if (nCount > 0)
                    app.ActivateCacheBuilder();
                 * */
            }

            // 无论如何都激活
            app.ActivateCacheBuilder();
            return nCount;
        }

        // return:
        //      0   没有创建新的队列事项
        //      1   创建了新的队列事项
        public static int AddToPendingList(LibraryApplication app,
            string strDataFile,
            string strNodePath,
            string strPart)
        {
            // 加入列表
            lock (app.PendingCacheFiles)
            {
                string strLine = strDataFile + ":" + strNodePath;
                if (String.IsNullOrEmpty(strPart) == false)
                    strLine += ":" + strPart;
                strLine = strLine.ToLower();
                if (app.PendingCacheFiles.IndexOf(strLine) == -1)
                {
                    app.PendingCacheFiles.Add(strLine);
                    app.ActivateCacheBuilder();
                    return 1;
                }
            }
            app.ActivateCacheBuilder();
            return 0;
        }

        public static XmlNode GetDataNode(XmlNode root,
    string strNodePath)
        {
            string[] path = strNodePath.Split(new char[] { '_' });
            XmlNode current_node = root;
            for (int i = 1; i < path.Length; i++)
            {
                string strNumber = path[i];
                int nNumber = Convert.ToInt32(strNumber);
                current_node = current_node.ChildNodes[nNumber];
            }

            return current_node;
        }

        public static long GetCount(LibraryApplication app,
            string strResultsetFilename)
        {
            try
            {
                app.ResultsetLocks.LockForRead(strResultsetFilename, 500);
                try
                {
                    return DpResultSet.GetCount(strResultsetFilename + ".index");
                }
                finally
                {
                    app.ResultsetLocks.UnlockForRead(strResultsetFilename);
                }
            }
            catch (System.ApplicationException)
            {
                return -1;
            }
        }

        // 创建XML检索式
        // parameters:
        int BuildXmlQuery(XmlNode node,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strCommand = DomUtil.GetAttr(node, "command");


            if (String.IsNullOrEmpty(strCommand) == true)
            {
                string strName = DomUtil.GetAttr(node, "name");

                // 截除第一个空格以后的部分
                int nRet = strName.IndexOf(" ");
                if (nRet != -1)
                    strName = strName.Substring(0, nRet);

                if (String.IsNullOrEmpty(strName) == false)
                    strCommand = "word=" + strName;
            }

            string[] parts = strCommand.Split(new char[] { ';' });

            int nCount = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();

                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                if (strPart == "(")
                {
                    strXml += "<group>";
                    continue;
                }

                if (strPart == ")")
                {
                    strXml += "</group>";
                    continue;
                }
                if (strPart.ToUpper() == "AND")
                {
                    strXml += "<operator value='AND' />";
                    continue;
                }
                if (strPart.ToUpper() == "OR")
                {
                    strXml += "<operator value='OR' />";
                    continue;
                }
                if (strPart.ToUpper() == "SUB")
                {
                    strXml += "<operator value='SUB' />";
                    continue;
                }

                Hashtable param_table = StringUtil.ParseParameters(strPart);

                //      env_param_table 环境变量表
                Hashtable env_param_table = GetCommandEnvParamTable(node);


                // 合并两个参数表
                Hashtable result = MergeTwoTable(param_table, env_param_table);

                // 2010/10/8 逗号也可以使用'|'代替
                string strDbName = (string)result["dbname"];
                strDbName = strDbName.Replace("|", ",");

                // 构造单个检索式
                string strSingle = "";
                // 根据检索参数创建XML检索式
                // return:
                //      -1  出错
                //      0   不存在所指定的数据库或者检索途径。一个都没有
                //      1   成功
                int nRet = LibraryApplication.BuildQueryXml(
                    this.App,
                    strDbName,
                    (string)result["word"],
                    (string)result["from"],
                    (string)result["matchstyle"],
                    (string)result["relation"],
                    (string)result["datatype"],
                    result["maxcount"] == null ? -1 : Convert.ToInt32((string)result["maxcount"]),
                    out strSingle,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return -1;  // TODO: OR情况下可以继续处理？ AND情况下只能当作空结果集了

                strXml += strSingle;

                nCount++;
            }

            // TODO: 如果最外围已经有了<group>元素就不要重复加了
            if (nCount > 1)
                strXml = "<group>" + strXml + "</group>";

            return 0;
        }

        void channel_Idle(object sender, IdleEventArgs e)
        {
            if (this.m_bClosed == true || this.Stopped == true)
            {
                RmsChannel channel = (RmsChannel)sender;
                channel.Abort();
            }

            e.bDoEvents = false;

            System.Threading.Thread.Sleep(100);	// 避免CPU资源过度耗费

        }

        public static string GetMyBookshelfFilename(
            LibraryApplication app,
            SessionInfo sessioninfo)
        {
            if (sessioninfo.Account == null)
                return null;

            string strUserID = sessioninfo.UserID;
            if (String.IsNullOrEmpty(strUserID) == true)
                return null;
            string strType = sessioninfo.Account.Type;
            if (String.IsNullOrEmpty(strType) == true)
                strType = "worker";
            string strDir = PathUtil.MergePath(app.DataDir + "/personaldata/" + strType, strUserID);
            PathUtil.CreateDirIfNeed(strDir);
            return PathUtil.MergePath(strDir, "mybookshelf.resultset");
        }

        // 外部调用
        public static int RemoveFromResultset(List<string> aPath,
            string strResultsetFilename,
            out string strError)
        {
            strError = "";
            if (File.Exists(strResultsetFilename) == false)
            {
                strError = "结果集文件 '" + strResultsetFilename + "' 不存在";
                return -1;
            }
            DpResultSet resultset = null;


            try
            {

                resultset = new DpResultSet(false, false);
                resultset.Attach(strResultsetFilename,
        strResultsetFilename + ".index");

            }
            catch (Exception ex)
            {
                strError = "打开结果集(文件为'" + strResultsetFilename + "')发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                for (int i = 0; i < resultset.Count; i++)
                {
                    DpRecord record = resultset[i];
                    int index = aPath.IndexOf(record.ID);
                    if (index != -1)
                    {
                        resultset.RemoveAt(i);
                        i--;
                    }
                }
            }
            finally
            {
                string strTemp1 = "";
                string strTemp2 = "";
                resultset.Detach(out strTemp1,
                    out strTemp2);
            }

            return 0;
        }

        // 外部调用
        public static int AddToResultset(List<string> aPath,
    string strResultsetFilename,
            bool bInsertAtFirst,
    out string strError)
        {
            strError = "";
            DpResultSet resultset = null;
            bool bCreate = false;
            try
            {
                if (File.Exists(strResultsetFilename) == true)
                {
                    resultset = new DpResultSet(false, false);
                    resultset.Attach(strResultsetFilename,
            strResultsetFilename + ".index");
                }
                else
                {
                    bCreate = true;
                    resultset = new DpResultSet(false, false);
                    resultset.Create(strResultsetFilename,
                        strResultsetFilename + ".index");

                }
            }
            catch (Exception ex)
            {
                strError = (bCreate == true ? "创建" : "打开")
                + "结果集(文件为'" + strResultsetFilename + "')发生错误: " + ex.Message;
                return -1;
            }

            bool bDone = false;
            try
            {
                for (int j = 0; j < aPath.Count; j++)
                {
                    Thread.Sleep(1);
                    DpRecord record = new DpRecord(aPath[j]);
                    if (bInsertAtFirst == true)
                        resultset.Insert(0, record);
                    else
                        resultset.Add(record);
                }

                bDone = true;
            }
            finally
            {
                if (bDone == true || bCreate == false)
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }
                else
                {
                    // 否则文件会被删除
                    resultset.Close();
                }
            }

            return 0;
        }

        // 从dp2kernel端获取结果集
        int GetResultset(RmsChannel channel,
            string strResultsetName,
            string strResultsetFilename,
            out int nCount,
            out string strError)
        {
            strError = "";
            nCount = 0;

            long lStart = 0;

            DpResultSet resultset = new DpResultSet(false, false);
            resultset.Create(strResultsetFilename,
                strResultsetFilename + ".index");

            bool bDone = false;
            try
            {

                for (; ; )
                {
                    if (this.m_bClosed == true || this.Stopped == true)
                    {
                        strError = "中断";
                        return -1;
                    }

                    Thread.Sleep(1);

                    List<string> aPath = null;

                    long lRet = channel.DoGetSearchResultEx(
                        strResultsetName,
                        lStart,
                        100,
                        "zh",
                        null,
                        out aPath,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    long lHitCount = lRet;

                    for (int j = 0; j < aPath.Count; j++)
                    {
                        DpRecord record = new DpRecord(aPath[j]);
                        resultset.Add(record);
                    }

                    if (aPath.Count <= 0  // < 100
                        )
                        break;

                    lStart += aPath.Count;
                    nCount += aPath.Count;

                    if (lStart >= lHitCount)
                        break;
                }

                bDone = true;
            }
            finally
            {
                if (bDone == true)
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }
                else
                {
                    // 否则文件会被删除
                    resultset.Close();
                }
            }

            return 0;
        }

        int BuildRssFile(string strResultsetFilename,
            long nMaxCount,
            string strDirection,
            string strChannelTitle,
            string strChannelLink,
            string strSelfLink,
            string strChannelDescription,
            string strOutputFilename,
            out int nOutputCount,
            out string strError)
        {
            strError = "";
            nOutputCount = 0;
            int nRet = 0;

            if (String.IsNullOrEmpty(strOutputFilename) == true)
                strOutputFilename = strResultsetFilename + ".rss";
            bool bDone = false;

            /*
            if (File.Exists(strOutputFilename) == true)
                return 0;
             * */


            XmlTextWriter writer = new XmlTextWriter(strOutputFilename,
                Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;

            DpResultSet resultset = null;
            this.App.ResultsetLocks.LockForRead(strResultsetFilename + ".rss", 500);

            try
            {
                resultset = new DpResultSet(false, false);
                try
                {
                    resultset.Attach(strResultsetFilename,
                        strResultsetFilename + ".index");
                }
                catch (Exception ex)
                {
                    strError = "打开结果集文件时出错: " + ex.Message;
                    return -1;
                }

                try
                {

                    writer.WriteStartDocument();
                    writer.WriteStartElement("rss");
                    writer.WriteAttributeString("version", "2.0");

                    writer.WriteAttributeString("xmlns", "dc", null,
                        "http://purl.org/dc/elements/1.1/");
                    writer.WriteAttributeString("xmlns", "atom", null,
                        "http://www.w3.org/2005/Atom");
                    writer.WriteAttributeString("xmlns", "content", null,
    "http://purl.org/rss/1.0/modules/content/");

                    writer.WriteStartElement("channel");

                    writer.WriteStartElement("title");
                    writer.WriteString(strChannelTitle);
                    writer.WriteEndElement();

                    writer.WriteStartElement("link");
                    writer.WriteString(strChannelLink);
                    writer.WriteEndElement();

                    writer.WriteStartElement("atom", "link",
                        "http://www.w3.org/2005/Atom");
                    writer.WriteAttributeString("href", strSelfLink);
                    writer.WriteAttributeString("rel", "self");
                    writer.WriteAttributeString("type", "application/rss+xml");
                    writer.WriteEndElement();

                    writer.WriteStartElement("description");
                    writer.WriteString(strChannelDescription);
                    writer.WriteEndElement();

                    writer.WriteStartElement("lastBuildDate");
                    writer.WriteString(DateTimeUtil.Rfc1123DateTimeString(DateTime.Now.ToUniversalTime()));
                    writer.WriteEndElement();

                    /*
                    // 如果尚未登录
                    if (sessioninfo.Account == null)
                    {
                        // 模拟一个具有getbibliosummary权限的用户
                        sessioninfo.Account = new Account();
                        sessioninfo.Account.Rights = "getbibliosummary";
                    }
                     * */

                    // 临时的SessionInfo对象
                    SessionInfo sessioninfo = new SessionInfo(this.App);

                    // 模拟一个账户
                    Account account = new Account();
                    account.LoginName = "CacheBuilder";
                    account.Password = "";
                    account.Rights = "getbibliosummary";

                    account.Type = "";
                    account.Barcode = "";
                    account.Name = "CacheBuilder";
                    account.UserID = "CacheBuilder";
                    account.RmsUserName = this.App.ManagerUserName;
                    account.RmsPassword = this.App.ManagerPassword;

                    sessioninfo.Account = account;

                    long nCount = resultset.Count;
                    long nStart = 0;

                    if (strDirection == "head" && nMaxCount != -1)
                        nCount = Math.Min(nMaxCount, nCount);
                    else if (strDirection == "tail" && nMaxCount != -1)
                    {
                        nStart = nCount - nMaxCount;
                        if (nStart < 0)
                            nStart = 0;
                    }

                    for (long i = nStart; i < nCount; i++)
                    {
                        if (this.m_bClosed == true || this.Stopped == true)
                        {
                            strError = "中断";
                            return -1;
                        }

                        DpRecord record = resultset[i];

                        string strPath = record.ID;

                        // TODO: 对于实体库记录或者评注库记录，可以检索其从属的书目记录来取得书名等？
                        // 或者评注库记录本身就有文章名
                        // 实体库记录可以link到 book.aspx?itemrecpath=???


                        string strBiblioDbName = "";
                        string strDbName = ResPath.GetDbName(strPath);
                        string strDbType = this.App.GetDbType(strDbName,
                            out strBiblioDbName);
                        if (strDbType == null)
                        {
                            strError = "数据库 '"+strDbName+"' 的类型无法识别";
                            return -1;
                        }


                        string strItemMetadata = "";
                        XmlDocument itemdom = null;
                        string strBiblioRecPath = "";
                        if (strDbType == "item" || strDbType == "comment")
                        {
                            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
                            if (channel == null)
                            {
                                strError = "get channel error";
                                return -1;
                            }
                            string strItemXml = "";
                            byte[] item_timestamp = null;
                            string strItemOutputPath = "";
                            long lRet = channel.GetRes(strPath,
                                out strItemXml,
                                out strItemMetadata,
                                out item_timestamp,
                                out strItemOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                strError = "获取记录 '"+strPath+"' 时发生错误: " + strError;
                                return -1;
                            }

                            nRet = LibraryApplication.LoadToDom(strItemXml,
                                out itemdom,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "装载记录 '"+strPath+"' 进入XML DOM时发生错误: " + strError;
                                return -1;
                            }

                            string strParentID = DomUtil.GetElementText(itemdom.DocumentElement,
                                "parent");
                            strBiblioRecPath = strBiblioDbName + "/" + strParentID;
                        }
                        else if (strDbType == "biblio")
                        {
                            strBiblioRecPath = strPath;
                        }
                        
                        // 从数据库中获取
                        string strBiblioXml = "";
                        byte[] timestamp = null;


                        string[] formats = new string[3];
                        formats[0] = "xml";
                        formats[1] = "summary";
                        formats[2] = "metadata";

                        string[] results = null;

                        LibraryServerResult result = this.App.GetBiblioInfos(
                            sessioninfo,
                            strBiblioRecPath,
                            formats,
                            out results,
                            out timestamp);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }

                        if (result.Value == 0)
                            continue;   // TODO: 产生一条占位记录?

                        if (results == null || results.Length != 3)
                        {
                            strError = "results error";
                            return -1;
                        }

                        strBiblioXml = results[0];
                        string strSummary = results[1];
                        string strBiblioMetaData = results[2];

                        string strMetaData = "";

                        if (strDbType == "biblio")
                            strMetaData = strBiblioMetaData;
                        else
                            strMetaData = strItemMetadata;

                        // 取metadata
                        Hashtable values = rmsUtil.ParseMedaDataXml(strMetaData,
                            out strError);
                        if (values == null)
                        {
                            strError = "parse metadata error: " + strError;
                            return -1;
                        }

                        string strPubDate = "";

                        string strLastModified = (string)values["lastmodified"];
                        if (String.IsNullOrEmpty(strLastModified) == false)
                        {
                            DateTime time = DateTime.Parse(strLastModified);
                            strPubDate = DateTimeUtil.Rfc1123DateTimeString(time.ToUniversalTime());
                        }

                        string strTitle = "";
                        string strLink = "";
                        List<string> authors = null;

                        if (strDbType == "biblio")
                            strLink = this.App.OpacServerUrl + "/book.aspx?BiblioRecPath=" + HttpUtility.UrlEncode(strBiblioRecPath);
                        else if (strDbType == "item")
                            strLink = this.App.OpacServerUrl + "/book.aspx?ItemRecPath=" + HttpUtility.UrlEncode(strPath) + "#active";
                        else if (strDbType == "comment")
                            strLink = this.App.OpacServerUrl + "/book.aspx?CommentRecPath=" + HttpUtility.UrlEncode(strPath) + "#active";

                        if (strDbType == "biblio"
                            || strDbType == "item")
                        {
                            nRet = GetBiblioInfos(
                                strBiblioXml,
                    out strTitle,
                    out authors,
                    out strError);
                        }
                        else if (strDbType == "comment")
                        {
                            nRet = GetCommentInfos(
    itemdom,
out strTitle,
out authors,
out strError);
                        }

                        string strItemSummary = "";
                        string strItemSummaryHtml = "";
                        if (strDbType == "item" || strDbType == "comment")
                        {
                                    // 获得实体记录或者评注记录的摘要
                            nRet = GetSummary(strDbType,
            itemdom,
            "text",
            out strItemSummary,
            out strError);
                            if (nRet == -1)
                            {
                                strError = "创建记录 '"+strPath+"' 的摘要信息时发生错误: " + strError;
                                return -1;
                            }
                            // 获得实体记录或者评注记录的摘要
                            nRet = GetSummary(strDbType,
            itemdom,
            "html",
            out strItemSummaryHtml,
            out strError);
                            if (nRet == -1)
                            {
                                strError = "创建记录 '" + strPath + "' 的摘要信息时发生错误: " + strError;
                                return -1;
                            }
                        }

                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("id", (i + 1).ToString());

                        writer.WriteStartElement("title");
                        writer.WriteString(strTitle);
                        writer.WriteEndElement();

                        foreach (string strAuthor in authors)
                        {
                            // writer.WriteStartElement("author");
                            writer.WriteStartElement("dc", "creator",
                                "http://purl.org/dc/elements/1.1/");
                            writer.WriteString(strAuthor);
                            writer.WriteEndElement();
                        }

                        if (String.IsNullOrEmpty(strSummary) == false
                            || String.IsNullOrEmpty(strItemSummary) == false)
                        {
                            writer.WriteStartElement("description");
                            if (String.IsNullOrEmpty(strItemSummary) == false)
                            {
                                if (strDbType == "comment")
                                    writer.WriteString(strItemSummary + "\r\n\r\n从属于： " + strSummary);
                                else
                                    writer.WriteString(strItemSummary + "\r\n\r\n" + strSummary);
                            }
                            else
                                writer.WriteString(strSummary);
                            writer.WriteEndElement();
                        }

                        // <content:encoded>
                        if (String.IsNullOrEmpty(strItemSummaryHtml) == false)
                        {
                            writer.WriteStartElement("content", "encoded", "http://purl.org/rss/1.0/modules/content/");
                            if (strDbType == "comment")
                                writer.WriteCData(strItemSummaryHtml + "<br/><br/>从属于： " + strSummary);
                            else
                                writer.WriteCData(strItemSummaryHtml + "<br/><br/>" + strSummary);
                            writer.WriteEndElement();
                        }

                        writer.WriteStartElement("link");
                        writer.WriteString(strLink);
                        writer.WriteEndElement();

                        writer.WriteStartElement("pubDate");
                        writer.WriteString(strPubDate);
                        writer.WriteEndElement();

                        writer.WriteEndElement();   // </item>

                        nOutputCount++;
                    }

                }
                finally
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1, out strTemp2);
                }


                writer.WriteEndElement();   // </channel>
                writer.WriteEndElement();   // </rss>
                writer.WriteEndDocument();
                bDone = true;
            }
            finally
            {
                this.App.EndLoop(strOutputFilename, true);

                this.App.ResultsetLocks.UnlockForRead(strResultsetFilename + ".rss");
                writer.Close();

                if (bDone == false)
                    File.Delete(strOutputFilename); // 不完整的文件要删除掉才行
            }

            return 0;
        }

        // 获得实体记录或者评注记录的摘要
        static int GetSummary(string strDbType,
            XmlDocument itemdom,
            string strFormat,
            out string strSummary,
            out string strError)
        {
            strError = "";
            strSummary = "";

            if (strDbType == "item")
            {
                string strBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                    "barcode");
                string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                    "location");
                string strAccessNo = DomUtil.GetElementText(itemdom.DocumentElement,
    "accessNo");
                if (strFormat == "html")
                    strSummary = "册条码号: " + strBarcode + "<br/>馆藏地: " + strLocation + "<br/>索取号: " + strAccessNo;
                else
                    strSummary = "册条码号: " + strBarcode + " \r\n馆藏地: " + strLocation + " \r\n索取号: " + strAccessNo;
                return 0;
            }

            if (strDbType == "comment")
            {
                string strType = DomUtil.GetElementText(itemdom.DocumentElement,
                    "type"); // 书评/订购征询
                string strOrderSuggestion = DomUtil.GetElementText(itemdom.DocumentElement,
                    "orderSuggestion");
                
                string strTitle = DomUtil.GetElementText(itemdom.DocumentElement,
                    "title");

                string strContent = DomUtil.GetElementText(itemdom.DocumentElement,
    "content");

                string strDisplayName = "";
                string strCreator = "";
                XmlNode nodeCreator = itemdom.DocumentElement.SelectSingleNode("creator");
                if (nodeCreator != null)
                {
                    strDisplayName = DomUtil.GetAttr(nodeCreator, "displayName");
                    strCreator = nodeCreator.InnerText;
                }

                if (String.IsNullOrEmpty(strDisplayName) == false)
                    strCreator = "[ " + strDisplayName +" ]";

                StringBuilder text = new StringBuilder(4096);

                if (strType == "订购征询")
                {
                    if (strOrderSuggestion == "yes")
                        text.Append("建议 订购 本书\r\n");
                    else
                        text.Append("建议 不要订购 本书\r\n");
                }

                if (String.IsNullOrEmpty(strTitle) == false)
                    text.Append("标题: " + strTitle + "\r\n");
                if (String.IsNullOrEmpty(strTitle) == false)
                    text.Append("作者: " + strCreator + "\r\n");
                if (String.IsNullOrEmpty(strContent) == false)
                    text.Append("正文: \r\n" + strContent.Replace("\\r", "\r\n") + "\r\n");

                string strOperInfo = "";

                string strFirstOperator = "";
                string strTime = "";

                XmlNode node = itemdom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
                if (node != null)
                {
                    strFirstOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += " " + "创建" + ": "
                        + GetUTimeString(strTime);
                }

                node = itemdom.DocumentElement.SelectSingleNode("operations/operation[@name='lastContentModified']");
                if (node != null)
                {
                    string strLastOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += " " + "最后修改" + ": "
                        + GetUTimeString(strTime);
                    if (strLastOperator != strFirstOperator)
                        strOperInfo += " (" + strLastOperator + ")";
                }

                if (String.IsNullOrEmpty(strOperInfo) == false)
                    text.Append(strOperInfo + "\r\n");

                if (strFormat == "html")
                    strSummary = text.ToString().Replace("\r\n", "<br/>");
                else
                    strSummary = text.ToString();
                return 0;
            }

            return 0;
        }

        public static string GetUTimeString(string strRfc1123TimeString)
        {
            if (String.IsNullOrEmpty(strRfc1123TimeString) == true)
                return "";

            DateTime time = new DateTime(0);
            try
            {
                time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123TimeString);
            }
            catch
            {
            }

            return time.ToLocalTime().ToString("u");
        }

        // 获得评注记录的标题和作者
        static int GetCommentInfos(
    XmlDocument itemdom,
    out string strTitle,
    out List<string> authors,
    out string strError)
        {
            strError = "";
            strTitle = "";
            authors = new List<string>();

            strTitle = DomUtil.GetElementText(itemdom.DocumentElement,
    "title");
            string strDisplayName = "";
            string strCreator = "";
            XmlNode nodeCreator = itemdom.DocumentElement.SelectSingleNode("creator");
            if (nodeCreator != null)
            {
                strDisplayName = DomUtil.GetAttr(nodeCreator, "displayName");
                strCreator = nodeCreator.InnerText;
            }

            if (String.IsNullOrEmpty(strDisplayName) == false)
                strCreator = "[ " + strDisplayName + " ]";

            authors.Add(strCreator);

            return 0;
        }

        // 获得书目记录的标题和作者
        static int GetBiblioInfos(
            string strBiblioXml,
            out string strTitle,
            out List<string> authors,
            out string strError)
        {
            strError = "";

            strTitle = "";
            authors = new List<string>();

            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                return -1;

            if (strOutMarcSyntax.ToLower() == "unimarc")
            {
                strTitle = MarcDocument.GetFirstSubfield(strMarc,
                    "200",
                    "a");

                for (int i = 0; ; i++)
                {
                    string strField = "";
                    string strNextFieldName = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的字段没有找到
                    //		1	找到。找到的字段返回在strField参数中
                    nRet = MarcDocument.GetField(strMarc,
                        null,
                        i,
                        out strField,
                        out strNextFieldName);
                    if (nRet != 1)
                        break;

                    if (StringUtil.HasHead(strField, "7") == true)
                    {
                        string strSubfield = "";
                        string strNextSubfieldName = "";
                        // return:
                        //		-1	出错
                        //		0	所指定的子字段没有找到
                        //		1	找到。找到的子字段返回在strSubfield参数中
                        nRet = MarcDocument.GetSubfield(strField,
                            DigitalPlatform.MarcDom.ItemType.Field,
                            "a",
                            0,
                            out strSubfield,
                            out strNextSubfieldName);
                        if (String.IsNullOrEmpty(strSubfield) == true)
                            continue;

                        authors.Add(strSubfield.Substring(1));
                    }

                }
            }
            else if (strOutMarcSyntax.ToLower() == "usmarc")
            {
                strTitle = MarcDocument.GetFirstSubfield(strMarc,
        "245",
        "a");
                for (int i = 0; ; i++)
                {
                    string strField = "";
                    string strNextFieldName = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的字段没有找到
                    //		1	找到。找到的字段返回在strField参数中
                    nRet = MarcDocument.GetField(strMarc,
                        null,
                        i,
                        out strField,
                        out strNextFieldName);
                    if (nRet != 1)
                        break;

                    if (StringUtil.HasHead(strField, "7") == true)
                    {
                        string strSubfield = "";
                        string strNextSubfieldName = "";
                        // return:
                        //		-1	出错
                        //		0	所指定的子字段没有找到
                        //		1	找到。找到的子字段返回在strSubfield参数中
                        nRet = MarcDocument.GetSubfield(strField,
                            DigitalPlatform.MarcDom.ItemType.Field,
                            "a",
                            0,
                            out strSubfield,
                            out strNextSubfieldName);
                        if (String.IsNullOrEmpty(strSubfield) == true)
                            continue;

                        authors.Add(strSubfield.Substring(1));
                    }

                }
            }

            return 0;
        }

        static Hashtable MergeTwoTable(Hashtable cmd, Hashtable env)
        {
            Hashtable result = new Hashtable();
            foreach (string key in cmd.Keys)
            {
                env.Remove(key);

                result.Add(key, cmd[key]);
            }

            foreach (string key in env.Keys)
            {
                result.Add(key, env[key]);
            }

            return result;
        }

        // 根据一个参数名获得default参数值
        static string GetEnvParamValue(XmlNode node,
            string strDefaultParam,
            string strName)
        {
            node = node.ParentNode; // 从父节点的default属性开始找

            while (node != null)
            {
                string strDefault = DomUtil.GetAttr(node, strDefaultParam);
                Hashtable param_table = StringUtil.ParseParameters(strDefault);
                if (param_table.Contains(strName) == true)
                    return (string)param_table[strName];

                node = node.ParentNode;
            }

            return null;
        }

        static Hashtable GetBuildEnvParamTable(XmlNode node)
        {
            Hashtable param_table = new Hashtable();

            // build
            string strBuild = GetEnvParamValue(node,
                "build",
                "");
            if (strBuild != null)
                param_table["build"] = strBuild;

            return param_table;
        }

        static Hashtable GetRssEnvParamTable(XmlNode node)
        {
            Hashtable param_table = new Hashtable();

            // enable
            string strEnable = GetEnvParamValue(node,
                "rssDefault",
                "enable");
            if (strEnable != null)
                param_table["enable"] = strEnable;

            // maxcount
            string strMaxCount = GetEnvParamValue(node,
                "rssDefault",
                 "maxcount");
            if (strMaxCount != null)
                param_table["maxcount"] = strMaxCount;


            return param_table;
        }

        static Hashtable GetCommandEnvParamTable(XmlNode node)
        {
            Hashtable param_table = new Hashtable();

            // dbname
            string strDbName = GetEnvParamValue(node,
                "default",
                "dbname");
            if (strDbName != null)
                param_table["dbname"] = strDbName;


            // word
            string strWord = GetEnvParamValue(node,
                "default",
                 "word");
            if (strWord != null)
                param_table["word"] = strWord;

            // from
            string strFrom = GetEnvParamValue(node,
                "default",
                 "from");
            if (strFrom != null)
                param_table["from"] = strFrom;

            // matchstyle
            string strMatchStyle = GetEnvParamValue(node,
                "default",
                 "matchstyle");
            if (strMatchStyle != null)
                param_table["matchstyle"] = strMatchStyle;


            // relation
            string strRelation = GetEnvParamValue(node,
                "default",
                "relation");
            if (strRelation != null)
                param_table["relation"] = strRelation;

            // datatype
            string strDataType = GetEnvParamValue(node,
                "default",
                "datatype");
            if (strDataType != null)
                param_table["datatype"] = strDataType;

            // maxcount
            string strMaxCount = GetEnvParamValue(node,
                "default",
                "maxcount");
            if (strMaxCount != null)
                param_table["maxcount"] = strMaxCount;

            return param_table;
        }
    }
#endif
}
