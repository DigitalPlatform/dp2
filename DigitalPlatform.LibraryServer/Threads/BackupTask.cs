using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer
{
    public class BackupTask : BatchTask
    {
        public BackupTask(LibraryApplication app,
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
                return "大备份";
            }
        }

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
            // out bool bClearFirst,
            out string strError)
        {
            strError = "";
            // bClearFirst = false;
            strFunction = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strFunction = (string)table["function"];

            return 0;
        }

        static string BuildTaskParam(
            string strFunction)
        {
            Hashtable table = new Hashtable();
            table["function"] = strFunction;
            // table["clear_first"] = bClearFirst ? "yes" : "no";
            return StringUtil.BuildParameterString(table);
        }

        #endregion

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
                int nRet = ParseStart(startinfo.Start,
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
                nRet = ParseTaskParam(startinfo.Param,
                    out strFunction,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                if (String.IsNullOrEmpty(strDbNameList) == true)
                {
                    // 从断点继续循环
                    strDbNameList = "continue";
                }

                // 构造用于复制然后同步的断点信息
                BreakPointCollection all_breakpoints = BreakPointCollection.BuildFromDbNameList(strDbNameList, strFunction);

                // 进行处理
                BreakPointCollection breakpoints = null;

                this.AppendResultText("*********\r\n");

                if (strDbNameList == "continue")
                {
                    // 按照断点信息处理
                    this.AppendResultText("从上次断点位置继续\r\n");

                    // return:
                    //      -1  出错
                    //      0   没有发现断点信息
                    //      1   成功
                    nRet = ReadBreakPoint(out breakpoints,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                        return;
                }
                else
                {
                    // 先从远端复制整个数据库，然后从开始复制时的日志末尾进行同步
                    this.AppendResultText("指定的数据库\r\n");

                    // 采纳先前创建好的复制并继续的断点信息
                    breakpoints = all_breakpoints;

                    // 建立要获取的记录路径文件

                }

                Debug.Assert(breakpoints != null, "");

                this.AppendResultText("计划进行的处理：\r\n---\r\n" + breakpoints.GetSummary() + "\r\n---\r\n\r\n");
                if (this.StartInfos.Count > 0)
                    this.AppendResultText("等待队列：\r\n---\r\n" + GetSummary(this.StartInfos) + "\r\n---\r\n\r\n");

                m_nRecordCount = 0;

                for (int i = 0; i < breakpoints.Count; i++)
                {
                    BreakPointInfo info = breakpoints[i];

                    nRet = BackupDatabase(info,
                        out strError);

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

            string strText = "";
            // 从断点记忆文件中读出信息
            // return:
            //      -1  error
            //      0   file not found
            //      1   found
            int nRet = this.App.ReadBatchTaskBreakPointFile(this.DefaultName,
                out strText,
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

        int m_nRecordCount = 0;

        int BackupDatabase(BreakPointInfo info,
out string strError)
        {
            strError = "";

            return 0;
        }

        // 一个服务器的断点信息
        class BreakPointInfo
        {
            public string DbName = "";    // 数据库名
            public string RecID = "";       // 已经处理到的 ID

            public string Function = "";    // 功能。

            // 通过字符串构造
            public static BreakPointInfo Build(string strText)
            {
                Hashtable table = StringUtil.ParseParameters(strText);

                BreakPointInfo info = new BreakPointInfo();
                info.DbName = (string)table["dbname"];
                info.RecID = (string)table["recid"];
                info.Function = (string)table["function"];
                return info;
            }

            // 变换为字符串
            public override string ToString()
            {
                Hashtable table = new Hashtable();
                table["dbname"] = this.DbName;
                table["recid"] = this.RecID;
                table["function"] = this.Function;
                return StringUtil.BuildParameterString(table);
            }

            // 小结文字
            public string GetSummary()
            {
                string strResult = "";
                strResult += this.DbName;
                strResult += "(功能=" + this.Function + ")";
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
            public static BreakPointCollection BuildFromDbNameList(string strText, string strFunction)
            {
                BreakPointCollection infos = new BreakPointCollection();

                string[] dbnames = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string dbname in dbnames)
                {
                    BreakPointInfo info = new BreakPointInfo();
                    info.DbName = dbname;
                    info.Function = strFunction;
                    infos.Add(info);
                }

                return infos;
            }

            // 变换为字符串
            public override string ToString()
            {
                StringBuilder text = new StringBuilder(4096);
                foreach (BreakPointInfo info in this)
                {
                    text.Append(info.ToString() + ";");
                }

                return text.ToString();
            }

            // 小结文字
            public string GetSummary()
            {
                StringBuilder text = new StringBuilder(4096);
                foreach (BreakPointInfo info in this)
                {
                    if (text.Length > 0)
                        text.Append("\r\n");
                    text.Append(info.GetSummary());
                }

                return text.ToString();
            }
        }

        int CreateRecPathFile(RmsChannel channel,
            BreakPointCollection infos,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            List<string> dbnames = new List<string>();


            foreach (BreakPointInfo info in infos)
            {
                string strDbName = info.DbName;
                if (strDbName == "*")
                {
                    // 如果数据库名为 *，表示希望获取所有的数据库
                    List<string> temp = null;
                    // 获得所有数据库名
                    int nRet = GetAllDbNames(out temp,
            out strError);
                    if (nRet == -1)
                        return -1;
                    dbnames.AddRange(temp);
                }
                else if (string.IsNullOrEmpty(strDbName) == false)
                    dbnames.Add(strDbName);
            }

            foreach(string dbname in dbnames)
            {

            }

            return 0;
        }

        // 获得所有数据库名
        int GetAllDbNames(out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            // 书目库
            foreach (ItemDbCfg cfg in this.App.ItemDbs)
            {
                if (string.IsNullOrEmpty(cfg.BiblioDbName) == false)
                    dbnames.Add(cfg.BiblioDbName);

                if (string.IsNullOrEmpty(cfg.DbName) == false)
                    dbnames.Add(cfg.DbName);

                if (string.IsNullOrEmpty(cfg.OrderDbName) == false)
                    dbnames.Add(cfg.OrderDbName);

                if (string.IsNullOrEmpty(cfg.IssueDbName) == false)
                    dbnames.Add(cfg.IssueDbName);

                if (string.IsNullOrEmpty(cfg.CommentDbName) == false)
                    dbnames.Add(cfg.CommentDbName);
            }

            // 读者库
            foreach (DigitalPlatform.LibraryServer.LibraryApplication.ReaderDbCfg cfg in this.App.ReaderDbs)
            {
                dbnames.Add(cfg.DbName);
            }

            // 其他库
            if (string.IsNullOrEmpty(this.App.AmerceDbName) == false)
                dbnames.Add(this.App.AmerceDbName);

            if (string.IsNullOrEmpty(this.App.ArrivedDbName) == false)
                dbnames.Add(this.App.ArrivedDbName);

            if (string.IsNullOrEmpty(this.App.InvoiceDbName) == false)
                dbnames.Add(this.App.InvoiceDbName);

            if (string.IsNullOrEmpty(this.App.MessageDbName) == false)
                dbnames.Add(this.App.MessageDbName);

            if (string.IsNullOrEmpty(this.App.PinyinDbName) == false)
                dbnames.Add(this.App.PinyinDbName);

            if (string.IsNullOrEmpty(this.App.GcatDbName) == false)
                dbnames.Add(this.App.GcatDbName);

            if (string.IsNullOrEmpty(this.App.WordDbName) == false)
                dbnames.Add(this.App.WordDbName);

            // 实用库
            if (this.App.LibraryCfgDom != null
                && this.App.LibraryCfgDom.DocumentElement != null)
            {
                XmlNodeList nodes = this.App.LibraryCfgDom.DocumentElement.SelectNodes("utilDb/database/@name");
                foreach (XmlNode node in nodes)
                {
                    dbnames.Add(node.Value);
                }
            }
            return 0;
        }

    }
}
