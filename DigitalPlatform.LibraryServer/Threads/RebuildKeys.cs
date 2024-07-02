﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.rms.Client;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 重建 dp2kernel 检索点的批处理任务
    /// 也兼做创建和更新书目记录查重键、查重码的批处理任务
    /// </summary>
    public class RebuildKeys : BatchTask
    {
        public RebuildKeys(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.PerTime = 5 * 60 * 1000;	// 5分钟

            this.Loop = false;   // 只运行一次
        }

        public override string DefaultName
        {
            get
            {
                return "重建检索点";
            }
        }

#if REMOVED
        #region 参数字符串处理
        // 这些函数也被 dp2Library 前端使用

        // 解析 开始 参数
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        static int ParseStart(string strStart,
            out string strDbNameList,
            out string strError)
        {
            strError = "";
            strDbNameList = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strStart);
            strDbNameList = (string)table["dbnamelist"];
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace("|", ",");
            return 0;
        }

        // 构造开始参数，也是断点字符串
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        static string BuildStart(
            string strDbNameList)
        {
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace(",", "|");

            Hashtable table = new Hashtable();
            table["dbnamelist"] = strDbNameList;

            return StringUtil.BuildParameterString(table);
        }

        // 解析通用启动参数
        public static int ParseTaskParam(string strParam,
            out string strFunction,
            out bool bClearFirst,
            out bool quick_mode,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strFunction = "";
            quick_mode = false;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strFunction = (string)table["function"];
            quick_mode = (bool)table["quick"];
            string strClearFirst = (string)table["clear_first"];
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            return 0;
        }

        static string BuildTaskParam(
            string strFunction,
            bool bClearFirst,
            bool quick_mode)
        {
            Hashtable table = new Hashtable();
            table["function"] = strFunction;
            table["clear_first"] = bClearFirst ? "yes" : "no";
            table["quick"] = quick_mode;
            return StringUtil.BuildParameterString(table);
        }

        #endregion
#endif

        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            if (this.App.PauseBatchTask == true)
                return;

            REDO_TASK:
            try
            {
                string strError = "";

                if (this.App.LibraryCfgDom == null
                    || this.App.LibraryCfgDom.DocumentElement == null)
                    return;

                BatchTaskStartInfo startinfo = this.StartInfo;
                if (startinfo == null)
                    startinfo = new BatchTaskStartInfo();   // 按照缺省值来

                string strDbNameList = "";
                int nRet = RebuildKeysParam.ParseStart(startinfo.Start,
                    out strDbNameList,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                // 下一次 loop 进入的时候自动就是 continue (从断点继续)
                startinfo.Start = "";

                //
                string strFunction = "";
                bool bClearFirst = false;
                nRet = RebuildKeysParam.ParseTaskParam(startinfo.Param,
                    out strFunction,
                    out bClearFirst,
                    out bool quick_mode,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

#if NO
                // 下一次 loop 进入的时候什么动作有没有，避免重复前一次的清除数据库动作
                startinfo.Param = "";

                if (bClearFirst == true)
                {

                    // 清除全部同步的本地库

                }
#endif

                if (String.IsNullOrEmpty(strDbNameList) == true)
                {
                    // 从断点继续循环
                    strDbNameList = "continue";
                }

                // 构造用于复制然后同步的断点信息
                BreakPointCollection all_breakpoints =
                    BreakPointCollection.BuildFromDbNameList(
                        strDbNameList,
                        strFunction,
                        quick_mode);

                // 进行处理
                BreakPointCollection breakpoints = null;

                this.AppendResultText("*********\r\n");

                // 按照断点信息处理
                if (strDbNameList == "continue")
                {
                    // return:
                    //      -1  出错
                    //      0   没有发现断点信息
                    //      1   成功
                    nRet = ReadBreakPoint(out breakpoints,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        // 没有发现断点信息
                        this.AppendResultText("当前没有断点信息。本轮处理被忽略\r\n");
                        return;
                    }

                    this.AppendResultText("从上次断点位置继续\r\n");
                }
                else
                {
                    // 先从远端复制整个数据库，然后从开始复制时的日志末尾进行同步
                    this.AppendResultText("指定的数据库\r\n");

                    // 采纳先前创建好的复制并继续的断点信息
                    breakpoints = all_breakpoints;
                }

                Debug.Assert(breakpoints != null, "");

                this.AppendResultText("计划进行的处理：\r\n---\r\n" + breakpoints.GetSummary() + "\r\n---\r\n\r\n");
                if (this.StartInfos.Count > 0)
                    this.AppendResultText("等待队列：\r\n---\r\n" + GetSummary(this.StartInfos) + "\r\n---\r\n\r\n");

                m_nRecordCount = 0;

                for (int i = 0; i < breakpoints.Count; i++)
                {
                    BreakPointInfo info = breakpoints[i];

                    if (info.Function == "重建查重键")
                        nRet = RebuildUniformKeyDatabase(info,
                            out strError);
                    else if (info.Function == "重建检索点" || string.IsNullOrEmpty(info.Function))
                        nRet = RebuildKeyDatabase(info,
                            out strError);
                    else
                    {
                        strError = "未能识别的 info.Function 参数值 '" + info.Function + "'";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                    {
                        // 保存断点文件
                        SaveBreakPoint(breakpoints, true);
                        goto ERROR1;
                    }

                    breakpoints.Remove(info);
                    i--;

                    // 保存断点文件
                    SaveBreakPoint(breakpoints, false);
                }

                // TODO: 如果集合为空，需要删除断点信息文件
                // 正常结束，复位断点
                if (this.StartInfos.Count == 0)
                    this.App.RemoveBatchTaskBreakPointFile(this.Name);

                this.StartInfo.Start = "";

                // AppendResultText("针对消息库 " + strMessageDbName + " 的循环结束。共处理 " + nRecCount.ToString() + " 条记录。\r\n");

                // TODO: 在断点文件中记载 StartInfos 内容
#if NO
                // 按照断点信息进行处理
                foreach (XmlNode server in server_nodes)
                {
                    string strServerName = DomUtil.GetAttr(server, "name");

                    // 找到断点信息
                    BreakPointInfo info = breakpoints.GetBreakPoint(strServerName);


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
#endif

                this.AppendResultText("本轮处理结束\r\n");

                {
                    // return:
                    //      -1  出错
                    //      0   没有发现断点信息
                    //      1   成功
                    nRet = ReadBreakPoint(out breakpoints,
out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 如果有累积的任务，还需要继续执行
                    if (nRet == 1 && this.StartInfos.Count > 0)
                    {
                        this.StartInfo = this.StartInfos[0];
                        this.StartInfos.RemoveAt(0);
                        goto REDO_TASK;
                    }
                }

                return;

            ERROR1:
                this.AppendResultText(strError + "\r\n");
                return;
            }
            finally
            {

            }

#if NO
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
#endif
        }

        static string ToString(List<BatchTaskStartInfo> start_infos)
        {
            StringBuilder text = new StringBuilder();
            foreach (BatchTaskStartInfo info in start_infos)
            {
                if (text.Length > 0)
                    text.Append(";");
                text.Append(info.ToString());
            }

            return text.ToString();
        }

        static List<BatchTaskStartInfo> FromString(string strText)
        {
            List<BatchTaskStartInfo> results = new List<BatchTaskStartInfo>();
            string[] segments = strText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in segments)
            {
                BatchTaskStartInfo info = BatchTaskStartInfo.FromString(segment);
                results.Add(info);
            }

            return results;
        }

        internal static string GetSummary(BatchTaskStartInfo info)
        {
            return info.Start;
        }

        static string GetSummary(List<BatchTaskStartInfo> start_infos)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (BatchTaskStartInfo info in start_infos)
            {
                if (text.Length > 0)
                    text.Append("\r\n");
                text.Append((i + 1).ToString() + ") " + GetSummary(info));
            }

            return text.ToString();
        }


        // 读出断点信息，和恢复 this.StartInfos
        // return:
        //      -1  出错
        //      0   没有发现断点信息
        //      1   成功
        int ReadBreakPoint(out BreakPointCollection breakpoints,
            out string strError)
        {
            strError = "";
            breakpoints = null;
            List<BatchTaskStartInfo> start_infos = null;

            // 从断点记忆文件中读出信息
            // return:
            //      -1  error
            //      0   file not found
            //      1   found
            int nRet = this.App.ReadBatchTaskBreakPointFile(this.DefaultName,
                out string strText,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "启动失败。因当前还没有断点信息，请指定为其他方式运行";
                return 0;
            }

            string strStartInfos = "";
            string strBreakPoint = "";
            StringUtil.ParseTwoPart(strText,
                "|||",
                out strBreakPoint,
                out strStartInfos);

            // 可能会抛出异常
            breakpoints = BreakPointCollection.Build(strBreakPoint);
            start_infos = FromString(strStartInfos);

            if (start_infos != null)
                this.StartInfos = start_infos;

            return 1;
        }

        // 保存断点信息，并保存 this.StartInfos
        void SaveBreakPoint(BreakPointCollection infos,
            bool bClearStartInfos)
        {
            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.Name,
                infos.ToString() + "|||" + ToString(this.StartInfos));

            if (bClearStartInfos)
                this.StartInfos = new List<BatchTaskStartInfo>();   // 避免残余信息对后一轮运行发生影响
        }

        delegate int Delegate_beginLoop(RmsChannel channel,
            BreakPointInfo info,
            out string strError);

        // 2024/6/24
        delegate int Delegate_endLoop(RmsChannel channel,
    BreakPointInfo info,
    out string strError);

        // return:
        //      -1  出错
        //      0   结束
        //      1   继续
        delegate int Delegate_processRecord(RmsChannel channel,
            ref bool bFirst,
            string strRecPath,
            bool quick_mode,
            out string strNextRecPath,
            out string strError);

        int m_nRecordCount = 0;

        #region 重建检索点

        /*
         * beginfastappend 开始快速模式。注意此时并不清除 keys 表中的行，因此 dp2kernel 的这个库的所有记录还能正常检索和修改
         * initializekeystable 清掉 keys 表中的所有行
         *      DoRebuildResKeys(recpath='中文图书/1' style=rebuildkeys,fastmode)
         *      DoRebuildResKeys(recpath='中文图书/2' style=rebuildkeys,fastmode)
         *      DoRebuildResKeys(recpath='中文图书/3' style=rebuildkeys,fastmode)
         *      ... (若干次，直到对数据库中所有记录均调用完成)
         * start_endfastappend 进入收尾阶段
         *      detect_endfastappend
         *      detect_endfastappend
         *      detect_endfastappend
         *      ... (若干次，直到探测到收尾阶段完成)
         *      
         * 注1: 如果在 DoRebuildResKeys() 循环内中断，要调用一次
         *      stopfastappend，以便 dp2kernel 把数据库的一个计数器减一。
         *      正常情况如果到达收尾阶段(start_endfastappend进入)，这个计数器也会减一。
         *      beginfastappend 时这个计数器则会加一。
         * */

        int beginLoop(
            RmsChannel channel,
            BreakPointInfo info,
            out string strError)
        {
            string action = "begin";
            bool clear_all_keytables = false;
            if (info.QuickMode)
            {
                action = "beginfastappend";
                // clear_all_keytables = true;
            }
            // Refresh数据库定义
            long lRet = channel.DoRefreshDB(
                action,
                info.DbName,
                clear_all_keytables,  // bClearKeysAtBegin == true ? true : false,
                out strError);
            if (lRet == -1)
                return -1;
            return 0;
        }

        int stopLoop(
RmsChannel channel,
BreakPointInfo info,
out string strError)
        {
            strError = "";
            if (info.QuickMode == false)
            {

            }
            else
            {
                try
                {
                    long lRet = channel.DoRefreshDB(
    "stopfastappend",
    info.DbName,
    false,  // 此参数不使用
    out strError);
                    if (lRet == -1)
                    {
                        strError = $"中断快速模式时出错: {strError}";
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "在中断快速模式时出现异常: " + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }
            }

            return 0;
        }


        int endLoop(
    RmsChannel channel,
    BreakPointInfo info,
    out string strError)
        {
            if (info.QuickMode == false)
            {
                // Refresh数据库定义
                long lRet = channel.DoRefreshDB(
                    "end",
                    info.DbName,
                    false,  // bClearKeysAtBegin == true ? true : false,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            else
            {
                //LibraryChannelManager.Log?.Debug($"开始对数据库{url}进行快速导入模式的最后收尾工作");
                try
                {
                    // initializekeystable
                    long lRet = channel.DoRefreshDB(
    "initializekeystable",
    info.DbName,
    false,  // 此参数不使用
    out strError);
                    if (lRet == -1)
                    {
                        strError = $"快速导入的收尾阶段，初始化 keys 表时出错: {strError}";
                        return -1;
                    }

                    lRet = channel.DoRefreshDB(
"start_endfastappend",
info.DbName,
false,
out strError);
                    if (lRet == -1)
                    {
                        strError = $"快速导入的收尾阶段，start_endfastappend 时出错: {strError}";
                        return -1;
                    }
                    else if (lRet == 1)
                    {
                        while (true)
                        {
                            //                  detect_endfastappend 探寻任务的状态。返回 0 表示任务尚未结束; 1 表示任务已经结束
                            lRet = channel.DoRefreshDB(
"detect_endfastappend",
info.DbName,
false,
out strError);
                            if (lRet == -1)
                            {
                                strError = $"快速导入的收尾阶段，detect_endfastappend 时出错: {strError}";
                                return -1;
                            }
                            if (lRet == 1)
                                break;
                        }
                    }
                    else if (lRet == 2)
                    {
                        //      2   本次已经减少计数 1，但依然不够，还剩下一定的计数当重新请求直到计数为零才会自动启动“收尾快速导入”任务
                        // throw new Exception(strQuickModeError);
                        // TODO: 也可以尝试再次调用 ManageKeysIndex(url, "start_endfastappend",
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "在等待快速模式收尾的阶段出现异常: " + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }
                finally
                {
                    //LibraryChannelManager.Log?.Debug($"结束对数据库{url}进行快速导入模式的最后收尾工作");
                }
            }

            return 0;
        }


        // return:
        //      -1  出错
        //      0   结束处理。(本次调用没有处理记录)
        //      1   成功。后面可以继续向后处理(strNextRecPath返回了刚处理过的记录的路径)
        int processRecord(RmsChannel channel,
            ref bool bFirst,
            string strRecPath,
            bool quick_mode,
            out string strNextRecPath,
            out string strError)
        {
            strNextRecPath = "";
            strError = "";

            string strStyle = "";
            strStyle = "timestamp,outputpath";	// 优化

            if (quick_mode)
                strStyle += ",rebuildkeys,fastmode";
            else
                strStyle += ",forcedeleteoldkeys";

            if (bFirst == true)
            {
                // 注：如果不校验首号，只有强制循环的情况下，才能不需要next风格
            }
            else
            {
                strStyle += ",next";
            }

            // string strOutputPath = "";

            bool bFoundRecord = false;

            bool bNeedRetry = true;

            int nRedoCount = 0;
        REDO_REBUILD:
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = channel.DoRebuildResKeys(strRecPath,
                strStyle,
                out strNextRecPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.IsNotFound())
                {
                    if (bFirst == true)
                    {
                        // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                        // AutoCloseMessageBox.Show(this, "您为数据库 " + info.DbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...");
                        bFirst = false;
                        return 1;
                    }
                    else
                    {
                        Debug.Assert(bFirst == false, "");

                        if (bFirst == true)
                        {
                            strError = "记录 " + strRecPath + "(后一条) 不存在。处理结束。";
                        }
                        else
                        {
                            strError = "记录 " + strRecPath + " 是最末一条记录。处理结束。";
                        }

                        return 0;
                    }
                }
                else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                {
                    bFirst = false;
                    return 1;
                }

                // 允许重试
                if (bNeedRetry == true)
                {
                    if (nRedoCount < 10)
                    {
                        nRedoCount++;
                        goto REDO_REBUILD;
                    }
                }
                else
                {
                    strError = "处理记录 '" + strRecPath + "'(" + strStyle + ") 时出错: " + strError;
                    return -1;
                }
            } // end of nRet == -1

            bFirst = false;

            bFoundRecord = true;
            if (bFoundRecord == true)
                m_nRecordCount++;
            return 1;
        }

        int RebuildKeyDatabase(BreakPointInfo info,
            out string strError)
        {
            strError = "";

            /*
            // 删除所有检索途径检索到的，比 __id 途径检索到的多出来的 ID 对应的记录的检索点
            int nRet = DeleteOutofRangeKeys(info,
out strError);
            if (nRet == -1)
                return -1;
            */

            return ProcessDatabase(info,
                beginLoop,
                processRecord,
                endLoop,
                stopLoop,
                out strError);
        }

        // 删除所有检索途径检索到的，比 __id 途径检索到的多出来的 ID 对应的记录的检索点
        int DeleteOutofRangeKeys(BreakPointInfo info,
    out string strError)
        {
            strError = "";

            this.AppendResultText($"正在检索 {info.DbName} 中残留的记录 ID ...\r\n");

            // TODO: 针对 <全部途径> 检索
            string query1 = "<target list='"
    + StringUtil.GetXmlStringSimple(info.DbName)
    + ":" + "<全部>'><item><word></word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            string query2 = "<target list='"
+ StringUtil.GetXmlStringSimple("__id")
+ ":" + "<全部>'><item><word></word><match>exat</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            string strQueryXml = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";

            RmsChannel channel = RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string resultset_name = "_rest";
            var ret = channel.DoSearch(strQueryXml,
                resultset_name,
                out strError);
            if (ret == -1)
                return -1;

            if (ret == 0)
            {
                this.AppendResultText($"{info.DbName} 没有任何残留的记录 ID\r\n");
                return 0;
            }

            this.AppendResultText($"开始清除 {info.DbName} 中残留记录(共 {ret} 条)的检索点...\r\n");

            var loader = new SearchResultLoader(channel,
                null,
                resultset_name,
                "id");
            loader.ElementType = "Record";
            foreach (Record record in loader)
            {
                string path = record.Path;
                if (string.IsNullOrEmpty(path))
                    continue;

                this.AppendResultText($"正在清除残留记录 {path} 的检索点...\r\n");

                int nRedoCount = 0;
            REDO_REBUILD:

                // return:
                //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                //		0	成功
                ret = channel.DoRebuildResKeys(path,
                    "forcedeleteoldkeys",
                    out _,
                    out strError);
                if (ret == -1)
                {
                    if (nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO_REBUILD;
                    }
                    return -1;
                }
            }

            this.AppendResultText($"完成清除 {info.DbName} 中残留记录的检索点\r\n");
            return 0;
        }

#endregion

        #region 重建查重键

        int beginLoop1(
    RmsChannel channel,
    BreakPointInfo info,
    out string strError)
        {
            strError = "";
            // 检查 info.DbName 是否为书目库
            return 0;
        }

        int processBiblioRecord(RmsChannel channel,
    ref bool bFirst,
    string strRecPath,
    bool quick_mode,
    out string strNextRecPath,
    out string strError)
        {
            strNextRecPath = "";
            strError = "";

            string strStyle = "";

            //  "content,data,metadata,timestamp,outputpath",

            strStyle = "content,data,timestamp,outputpath";	// 优化
            if (bFirst == true)
            {
                // 注：如果不校验首号，只有强制循环的情况下，才能不需要next风格
            }
            else
            {
                strStyle += ",next";
            }

            // string strOutputPath = "";

            bool bFoundRecord = false;

            bool bNeedRetry = true;

            int nRedoCount = 0;
        REDO_REBUILD:
            string strResult = "";
            string strMetaData = "";
            byte[] timestamp = null;
            // 获得资源
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = channel.GetRes(strRecPath,
                strStyle,
                out strResult,
                out strMetaData,
                out timestamp,
                out strNextRecPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.IsNotFound())
                {
                    if (bFirst == true)
                    {
                        // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                        // AutoCloseMessageBox.Show(this, "您为数据库 " + info.DbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...");
                        bFirst = false;
                        return 1;
                    }
                    else
                    {
                        Debug.Assert(bFirst == false, "");

                        if (bFirst == true)
                        {
                            strError = "记录 " + strRecPath + "(后一条) 不存在。处理结束。";
                        }
                        else
                        {
                            strError = "记录 " + strRecPath + " 是最末一条记录。处理结束。";
                        }

                        return 0;
                    }

                }
                else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                {
                    bFirst = false;
                    return 1;
                }

                // 允许重试
                if (bNeedRetry == true)
                {
                    if (nRedoCount < 10)
                    {
                        nRedoCount++;
                        goto REDO_REBUILD;
                    }
                }
                else
                {
                    strError = "获取记录 '" + strRecPath + "'(" + strStyle + ") 时出错: " + strError;
                    return -1;
                }
            } // end of nRet == -1

            bFirst = false;

            bFoundRecord = true;
            if (bFoundRecord == true)
                m_nRecordCount++;

            // 重建查重键
            int nRet = LibraryServerUtil.CreateUniformKey(
                false,
                ref strResult,
                out strError);
            if (nRet == -1)
            {
                strError = "为记录 '" + strNextRecPath + "' 创建查重键时出错: " + strError;
                return -1;
            }

            byte[] output_timestamp = null;
            string strOutputPath = "";
            lRet = channel.DoSaveTextRes(strNextRecPath,
                strResult,
                false,
                "content",
                timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    && nRedoCount < 10)
                {
                    nRedoCount++;
                    this.AppendResultText("因时间戳不匹配，重试处理记录 '" + strNextRecPath + "'\r\n");
                    goto REDO_REBUILD;
                }
                strError = "保存记录 '" + strNextRecPath + "' 时出错: " + strError;
                return -1;
            }

            return 1;
        }

        int RebuildUniformKeyDatabase(BreakPointInfo info,
out string strError)
        {
            strError = "";

            return ProcessDatabase(info,
                beginLoop1,
                processBiblioRecord,
                null,
                null,
                out strError);
        }


        #endregion

        // TODO: 用两个回调函数嵌入
        int ProcessDatabase(BreakPointInfo info,
            Delegate_beginLoop procBeginLoop,
            Delegate_processRecord procProcessRecord,
            Delegate_beginLoop procEndLoop,
            Delegate_beginLoop procStopLoop,
            out string strError)
        {
            strError = "";

            RmsChannel channel = RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 恢复为最大范围
            string strStartNo = "1";
            string strEndNo = "9999999999";

            if (string.IsNullOrEmpty(info.RecID) == false)
                strStartNo = info.RecID;    // 注意 info.RecID 可能为 "32+" 形态

            // 校验起止号
            // return:
            //      0   不存在记录
            //      1   存在记录
            int nRet = VerifyRange(channel,
                info.DbName,
                strStartNo,
                strEndNo,
                out string strOutputStartNo,
                out string strOutputEndNo,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
                return 0;

            strStartNo = strOutputStartNo;
            strEndNo = strOutputEndNo;

            Int64 nStart;
            Int64 nEnd;
            Int64 nCur;

            if (Int64.TryParse(strStartNo, out nStart) == false)
            {
                strError = "数据库 '" + info.DbName + "' 起始记录 ID '" + strStartNo + "' 不合法";
                return -1;
            }
            if (Int64.TryParse(strEndNo, out nEnd) == false)
            {
                strError = "数据库 '" + info.DbName + "' 结束记录 ID '" + strEndNo + "' 不合法";
                return -1;
            }

#if NO
            // Refresh数据库定义
            long lRet = channel.DoRefreshDB(
                "begin",
                info.DbName,
                false,  // bClearKeysAtBegin == true ? true : false,
                out strError);
            if (lRet == -1)
                return -1;
#endif
            if (procBeginLoop != null)
            {
                nRet = procBeginLoop(channel, info, out strError);
                if (nRet == -1)
                    return -1;
            }

            string strID = strStartNo;

            bool bFirst = true;	// 是否为第一次取记录
            bool finish = false;    // 是否处理完成所有记录
            try
            {
                // 循环
                for (; ; )
                {
                    if (this.Stopped == true)
                    {
                        strError = $"中断。已经处理到记录 ID '{info.RecID}'";
                        return -1;
                    }

                    string strPath = info.DbName + "/" + strID;

                    // return:
                    //      -1  出错
                    //      0   结束处理。(本次调用没有处理记录)
                    //      1   成功。后面可以继续向后处理(strNextRecPath返回了刚处理过的记录的路径)
                    nRet = procProcessRecord(channel,
                        ref bFirst,
                        strPath,
                        info.QuickMode,
                        out string strOutputPath,
                        out strError);
                    //if (nRet == 1)
                    //    goto CONTINUE;
                    if (nRet == 0)
                        return 0;
                    if (nRet == -1)
                        return -1;
#if NO
                    // string strDirectionComment = "";

                    string strStyle = "";

                    strStyle = "timestamp,outputpath";	// 优化

                    strStyle += ",forcedeleteoldkeys";

                    if (bFirst == true)
                    {
                        // 注：如果不校验首号，只有强制循环的情况下，才能不需要next风格
                        strStyle += "";
                    }
                    else
                    {

                        strStyle += ",next";
                        // strDirectionComment = "的后一条记录";
                    }

                    string strPath = info.DbName + "/" + strID;
                    string strOutputPath = "";

                    bool bFoundRecord = false;

                    bool bNeedRetry = true;

                    int nRedoCount = 0;
                REDO_REBUILD:
                    // 获得资源
                    // return:
                    //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                    //		0	成功
                    lRet = channel.DoRebuildResKeys(strPath,
                        strStyle,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsEqualNotFound())
                        {
                            if (bFirst == true)
                            {
                                // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                                // AutoCloseMessageBox.Show(this, "您为数据库 " + info.DbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...");
                                bFirst = false;
                                goto CONTINUE;
                            }
                            else
                            {
                                Debug.Assert(bFirst == false, "");

                                if (bFirst == true)
                                {
                                    strError = "记录 " + strID + "(后一条) 不存在。处理结束。";
                                }
                                else
                                {
                                    strError = "记录 " + strID + " 是最末一条记录。处理结束。";
                                }

                                return 0;
                            }

                        }
                        else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                        {
                            bFirst = false;
                            // bFoundRecord = false;
                            // 把id解析出来
                            strID = ResPath.GetRecordId(strOutputPath);
                            goto CONTINUE;

                        }

                        // 允许重试
                        if (bNeedRetry == true)
                        {
                            if (nRedoCount < 10)
                            {
                                nRedoCount++;
                                goto REDO_REBUILD;
                            }
                        }
                        else
                            return -1;

                    } // end of nRet == -1

                    bFirst = false;

                    bFoundRecord = true;
#endif

                    // 把id解析出来
                    strID = ResPath.GetRecordId(strOutputPath);

                    info.RecID = strID; // 记忆
                    if (bFirst == false)
                        info.RecID += "+";

                    // 每 100 条显示一行
                    if ((m_nRecordCount % 100) == 0)
                        this.AppendResultText("已重建检索点 记录 " + strOutputPath + "  " + (m_nRecordCount + 1).ToString() + "\r\n");

                    // CONTINUE:

                    // 是否超过循环范围
                    if (Int64.TryParse(strID, out nCur) == false)
                    {
                        strError = "数据库 '" + info.DbName + "' 当前记录 ID '" + strID + "' 不合法";
                        return -1;
                    }
                    if (nCur > nEnd)
                        break;

                    //if (bFoundRecord == true)
                    //    m_nRecordCount++;

                    SetProgressText((nCur - nStart + 1).ToString());

                    // 对已经作过的进行判断
                    if (nCur >= nEnd)
                        break;
                }

                finish = true;
                nRet = 0;
                // 2024/6/24
                if (procEndLoop != null)
                {
                    this.AppendResultText($"开始收尾 {info.DbName}\r\n");
                    nRet = procEndLoop(channel, info, out strError);
                    if (nRet == -1)
                        return -1;
                    this.AppendResultText($"结束收尾 {info.DbName}\r\n");
                }
            }
            finally
            {
                // 2024/6/28
                if (finish == false)
                {
                    if (procStopLoop != null)
                    {
                        this.AppendResultText($"中断处理 {info.DbName}。已经处理到记录 ID '{info.RecID}'\r\n");
                        nRet = procStopLoop(channel, info, out strError);
                        //if (nRet == -1)
                        //    return -1;
                    }
                }
            }

            this.AppendResultText($"共处理记录 {m_nRecordCount} 条\r\n");
            if (nRet == -1)
                return -1;
            return 0;
        }

#if NO
        // TODO: 用两个回调函数嵌入
        int RebuildDatabase(BreakPointInfo info,
            out string strError)
        {
            strError = "";

            RmsChannel channel = RmsChannels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 恢复为最大范围
            string strStartNo = "1";
            string strEndNo = "9999999999";

            string strOutputStartNo = "";
            string strOutputEndNo = "";

            if (string.IsNullOrEmpty(info.RecID) == false)
                strStartNo = info.RecID;

            // 校验起止号
            // return:
            //      0   不存在记录
            //      1   存在记录
            int nRet = VerifyRange(channel,
                info.DbName,
                strStartNo,
                strEndNo,
                out strOutputStartNo,
                out strOutputEndNo,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
                return 0;

            strStartNo = strOutputStartNo;
            strEndNo = strOutputEndNo;

            Int64 nStart;
            Int64 nEnd;
            Int64 nCur;

            if (Int64.TryParse(strStartNo, out nStart) == false)
            {
                strError = "数据库 '"+info.DbName+"' 起始记录 ID '"+strStartNo+"' 不合法";
                return -1;
            }
            if (Int64.TryParse(strEndNo, out nEnd) == false)
            {
                strError = "数据库 '" + info.DbName + "' 结束记录 ID '" + strEndNo + "' 不合法";
                return -1;
            }

            // Refresh数据库定义
            long lRet = channel.DoRefreshDB(
                "begin",
                info.DbName,
                false,  // bClearKeysAtBegin == true ? true : false,
                out strError);
            if (lRet == -1)
                return -1;

            string strID = strStartNo;

            try
            {
                bool bFirst = true;	// 是否为第一次取记录

                // 循环
                for (; ; )
                {
                    if (this.Stopped == true)
                    {
                        strError = "中断";
                        return -1;
                    }
                    // string strDirectionComment = "";

                    string strStyle = "";

                    strStyle = "timestamp,outputpath";	// 优化

                    strStyle += ",forcedeleteoldkeys";

                    if (bFirst == true)
                    {
                        // 注：如果不校验首号，只有强制循环的情况下，才能不需要next风格
                        strStyle += "";
                    }
                    else
                    {

                        strStyle += ",next";
                        // strDirectionComment = "的后一条记录";
                    }

                    string strPath = info.DbName + "/" + strID;
                    string strOutputPath = "";

                    bool bFoundRecord = false;

                    bool bNeedRetry = true;

                    int nRedoCount = 0;
                REDO_REBUILD:
                    // 获得资源
                    // return:
                    //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                    //		0	成功
                    lRet = channel.DoRebuildResKeys(strPath,
                        strStyle,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsEqualNotFound())
                        {
                            if (bFirst == true)
                            {
                                // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                                // AutoCloseMessageBox.Show(this, "您为数据库 " + info.DbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...");
                                bFirst = false;
                                goto CONTINUE;
                            }
                            else
                            {
                                Debug.Assert(bFirst == false, "");

                                if (bFirst == true)
                                {
                                    strError = "记录 " + strID + "(后一条) 不存在。处理结束。";
                                }
                                else
                                {
                                    strError = "记录 " + strID + " 是最末一条记录。处理结束。";
                                }

                                return 0;
                            }

                        }
                        else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                        {
                            bFirst = false;
                            // bFoundRecord = false;
                            // 把id解析出来
                            strID = ResPath.GetRecordId(strOutputPath);
                            goto CONTINUE;

                        }

                        // 允许重试
                        if (bNeedRetry == true)
                        {
                            if (nRedoCount < 10)
                            {
                                nRedoCount++;
                                goto REDO_REBUILD;
                            }
                        }
                        else
                            return -1;

                    } // end of nRet == -1

                    bFirst = false;

                    bFoundRecord = true;

                    // 把id解析出来
                    strID = ResPath.GetRecordId(strOutputPath);

                    info.RecID = strID; // 记忆

                    // 每 100 条显示一行
                    if ((m_nRecordCount % 100) == 0)
                        this.AppendResultText("已重建检索点 记录 " + strOutputPath + "  " + (m_nRecordCount + 1).ToString() + "\r\n");

#if NO
                if (String.IsNullOrEmpty(strRealStartNo) == true)
                {
                    strRealStartNo = strID;
                }

                strRealEndNo = strID;
#endif

                CONTINUE:

                    // 是否超过循环范围
                    if (Int64.TryParse(strID, out nCur) == false)
                    {
                        strError = "数据库 '" + info.DbName + "' 当前记录 ID '" + strID + "' 不合法";
                        return -1;
                    }
#if NO
                    try
                    {
                        nCur = Convert.ToInt64(strID);
                    }
                    catch
                    {
                        // ???
                        nCur = 0;
                    }
#endif

                    if (nCur > nEnd)
                        break;

                    if (bFoundRecord == true)
                        m_nRecordCount++;

                    //
                    //


                    SetProgressText((nCur - nStart + 1).ToString());

                    // 对已经作过的进行判断
                    if (nCur >= nEnd)
                        break;
                }
            }
            finally
            {
#if NO
                    if (bClearKeysAtBegin == true)
                    {
                // 结束Refresh数据库定义
                lRet = channel.DoRefreshDB(
                    "end",
                    info.DbName,
                    false,  // 此参数此时无用
                    out strError);

                if (lRet == -1)
                    return -1;
                }
#endif
            }

            return 0;
        }
#endif

        // 校验起止号
        // parameters:
        //      strIntputStartNo    起始 ID。一般是一个数字。
        //                          特殊地，如果字符串末尾为 '+'，表示希望获得比这个号码更大的相邻记录 ID
        // return:
        //      0   不存在记录
        //      1   存在记录
        static int VerifyRange(RmsChannel channel,
            string strDbName,
            string strInputStartNo,
            string strInputEndNo,
            out string strOutputStartNo,
            out string strOutputEndNo,
            out string strError)
        {
            strError = "";
            strOutputStartNo = "";
            strOutputEndNo = "";

            bool bStartNotFound = false;
            bool bEndNotFound = false;

            // 如果输入参数中为空，则假定为“全部范围”
            if (string.IsNullOrEmpty(strInputStartNo) == true)
                strInputStartNo = "1";

            if (string.IsNullOrEmpty(strInputEndNo) == true)
                strInputEndNo = "9999999999";

            bool start_sibling = false; // 是否要获得起始号码的相邻的记录号码
            if (strInputStartNo.EndsWith("+"))
            {
                start_sibling = true;
                strInputStartNo = strInputStartNo.Substring(0, strInputStartNo.Length - 1);
            }

            bool bAsc = true;

            Int64 nStart = 0;
            Int64 nEnd = 9999999999;

            if (Int64.TryParse(strInputStartNo, out nStart)== false)
            {
                strError = $"起始号码 '{strInputStartNo}' 不合法。应为一个纯数字";
            }
            /*
            try
            {
                nStart = Convert.ToInt64(strInputStartNo);
            }
            catch
            {
            }
            */

            if (Int64.TryParse(strInputEndNo, out nEnd) == false)
            {
                strError = $"结束号码 '{strInputEndNo}' 不合法。应为一个纯数字";
            }
            /*
            try
            {
                nEnd = Convert.ToInt64(strInputEndNo);
            }
            catch
            {
            }
            */

            if (nStart > nEnd)
                bAsc = false;
            else
                bAsc = true;

            string strPath = strDbName + "/" + strInputStartNo;
            string strStyle = "outputpath";

            if (bAsc == true)
                strStyle += ",next";
            else
                strStyle += ",prev";

            // 2024/7/2
            if (start_sibling == false)
                strStyle += ",myself";

            string strResult;
            string strMetaData;
            byte[] baOutputTimeStamp;
            string strOutputPath;

            string strError0 = "";

            string strStartID = "";
            string strEndID = "";

            // 获得资源
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = channel.GetRes(strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError0);
            if (lRet == -1)
            {
                if (channel.IsNotFound())
                {
                    strStartID = strInputStartNo;
                    bStartNotFound = true;
                }
                else
                    strError += "校验startno时出错： " + strError0 + " ";

            }
            else
            {
                // 取得返回的id
                strStartID = ResPath.GetRecordId(strOutputPath);
            }

            if (strStartID == "")
            {
                strError = "strStartID为空..." + (string.IsNullOrEmpty(strError) == false ? " : " + strError : "");
                return -1;
            }

            strPath = strDbName + "/" + strInputEndNo;

            strStyle = "outputpath";
            if (bAsc == true)
                strStyle += ",prev,myself";
            else
                strStyle += ",next,myself";

            // 获得资源
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            lRet = channel.GetRes(strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError0);
            if (lRet == -1)
            {
                if (channel.IsNotFound())
                {
                    strEndID = strInputEndNo;
                    bEndNotFound = true;
                }
                else
                {
                    strError += "校验endno时出错： " + strError0 + " ";
                }
            }
            else
            {
                // 取得返回的id
                strEndID = ResPath.GetRecordId(strOutputPath);
            }

            if (strEndID == "")
            {
                strError = "strEndID为空..." + (string.IsNullOrEmpty(strError) == false ? " : " + strError : ""); ;
                return -1;
            }

            ///
            bool bSkip = false;

            Int64 nTemp = 0;
            try
            {
                nTemp = Convert.ToInt64(strStartID);
            }
            catch
            {
                strError = "strStartID值 '" + strStartID + "' 不是数字...";
                return -1;
            }

            if (bAsc == true)
            {
                if (nTemp > nEnd)
                {
                    bSkip = true;
                }
            }
            else
            {
                if (nTemp < nEnd)
                {
                    bSkip = true;
                }
            }

            if (bSkip == false)
            {
                strOutputStartNo = strStartID;
            }


            ///

            bSkip = false;

            try
            {
                nTemp = Convert.ToInt64(strEndID);
            }
            catch
            {
                strError = "strEndID值 '" + strEndID + "' 不是数字...";
                return -1;
            }
            if (bAsc == true)
            {
                if (nTemp < nStart)
                {
                    bSkip = true;
                }
            }
            else
            {
                if (nTemp > nStart)
                {
                    bSkip = true;
                }
            }

            if (bSkip == false)
            {
                strOutputEndNo = strEndID;
            }

            if (bStartNotFound == true && bEndNotFound == true)
                return 0;

            return 1;
        }

        // 一个服务器的断点信息
        class BreakPointInfo
        {
            public string DbName = "";    // 数据库名
            public string RecID = "";       // 已经处理到的 ID

            // 2017/1/11
            public string Function = "";    // 功能。重建检索点/重建查重键 空等于 "重建检索点"

            // 2024/6/24
            public bool QuickMode = false;

            // 通过字符串构造
            public static BreakPointInfo Build(string strText)
            {
                Hashtable table = StringUtil.ParseParameters(strText);

                BreakPointInfo info = new BreakPointInfo();
                info.DbName = (string)table["dbname"];
                info.RecID = (string)table["recid"];
                info.Function = (string)table["function"];
                info.QuickMode = DomUtil.IsBooleanTrue((string)table["quick"], false);
                return info;
            }

            // 变换为字符串
            public override string ToString()
            {
                Hashtable table = new Hashtable();
                table["dbname"] = this.DbName;
                table["recid"] = this.RecID;
                table["function"] = this.Function;
                if (this.QuickMode == true)
                    table["quick"] = this.QuickMode ? "true" : "false";
                return StringUtil.BuildParameterString(table);
            }

            // 小结文字
            public string GetSummary()
            {
                string strResult = "";
                strResult += this.DbName;
                strResult += "(功能=" + this.Function + ")";
                if (QuickMode)
                    strResult += "(快速模式)";
                if (string.IsNullOrEmpty(this.RecID) == false)
                {
                    strResult += " : 从ID " + this.RecID.ToString() + " 开始";
                }

                return strResult;
            }
        }

        // 若干服务器的断点信息
        class BreakPointCollection : List<BreakPointInfo>
        {
#if NO
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
#endif

            // 通过字符串构造
            public static BreakPointCollection Build(string strText)
            {
                BreakPointCollection infos = new BreakPointCollection();

                string[] segments = strText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string segment in segments)
                {
                    infos.Add(BreakPointInfo.Build(segment));
                }

                return infos;
            }

            // 通过数据库名列表字符串构造
            public static BreakPointCollection BuildFromDbNameList(string strText,
                string strFunction,
                bool quick_mode)
            {
                BreakPointCollection infos = new BreakPointCollection();

                string[] dbnames = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string dbname in dbnames)
                {
                    BreakPointInfo info = new BreakPointInfo();
                    info.DbName = dbname;
                    info.Function = strFunction;
                    info.QuickMode = quick_mode;
                    infos.Add(info);
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
                    if (text.Length > 0)
                        text.Append("\r\n");
                    text.Append(info.GetSummary());
                }

                return text.ToString();
            }
        }
    }
}
