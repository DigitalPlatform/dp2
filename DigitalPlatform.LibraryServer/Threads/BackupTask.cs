using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.IO;
using Ionic.Zip;

namespace DigitalPlatform.LibraryServer
{
    public class BackupTask : BatchTask
    {
        public string OutputFileNames { get; set; }  // 备份文件、数据库定义文件的逻辑路径列表。即，从前端角度看，可以用 GetRes() API 来获取文件的参数路径

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

#if NO
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

            if (strStart == "continue")
            {
                strDbNameList = "continue";
                return 0;
            }

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

#endif

        // 一次操作循环
        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            if (this.App.ContainsHangup("LogRecover") == true)
                return;

            if (this.App.PauseBatchTask == true)
                return;

            // REDO_TASK:
            try
            {
                string strError = "";
                int nRet = 0;

                if (this.App.LibraryCfgDom == null
                    || this.App.LibraryCfgDom.DocumentElement == null)
                    return;

                BatchTaskStartInfo startinfo = this.StartInfo;
                if (startinfo == null)
                    startinfo = new BatchTaskStartInfo();   // 按照缺省值来

                BackupTaskStart param = BackupTaskStart.FromString(startinfo.Start);
                string strDbNameList = param.DbNameList;
#if NO
                string strDbNameList = "";
                int nRet = ParseStart(startinfo.Start,
                    out strDbNameList,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }
#endif

                // 下一次 loop 进入的时候自动就是 continue (从断点继续)
                startinfo.Start = "";

#if NO
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
#endif

                // if (String.IsNullOrEmpty(strDbNameList) == true)
                if (strDbNameList == "continue")
                {
                    // 从断点继续循环
                    strDbNameList = "continue";
                }

                string strRecPathFileName = Path.Combine(this.App.BackupDir, "recpath.txt");
                string strBackupFileName = "";

                if (string.IsNullOrEmpty(param.BackupFileName) == true)
                    strBackupFileName = Path.Combine(this.App.BackupDir, this.App.LibraryName + "_" + BackupTaskStart.GetDefaultBackupFileName());
                else
                    strBackupFileName = Path.Combine(this.App.BackupDir, CanonializeBackupFileName(param.BackupFileName));

                // 构造用于复制然后同步的断点信息
                // BreakPointCollection all_breakpoints = BreakPointCollection.BuildFromDbNameList(strDbNameList, strFunction);

                // 进行处理
                BreakPointInfo breakpoint = null;

                this.AppendResultText("*********\r\n");

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

                if (strDbNameList == "continue")
                {
                    // 按照断点信息处理
                    this.AppendResultText("从上次断点位置继续\r\n");

                    // return:
                    //      -1  出错
                    //      0   没有发现断点信息
                    //      1   成功
                    nRet = ReadBreakPoint(out breakpoint,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        // return;
                        // TODO: 此时如何让等待结束？并且返回启动出错信息
                        goto ERROR1;
                    }

                    strBackupFileName = breakpoint.BackupFileName;
                    if (string.IsNullOrEmpty(strBackupFileName))
                    {
                        strError = "从上次断点开始运行时，发现 BackupFileName 为空，只好放弃运行";
                        goto ERROR1;
                    }

                    // WriteStateFile(strBackupFileName, "creating");  // 表示文件正在创建中
                    PrepareBackupFileName(strBackupFileName, true);
                    this.AppendResultText("(从断点继续)本次大备份文件为 " + strBackupFileName + "\r\n");
                }
                else
                {
                    {
                        BreakPointInfo temp_breakpoint = null;
                        // 删除上次的大备份文件
                        // return:
                        //      -1  出错
                        //      0   没有发现断点信息
                        //      1   成功
                        nRet = ReadBreakPoint(out temp_breakpoint,
                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                        {
                            string strLastBackupFileName = temp_breakpoint.BackupFileName;
                            if (string.IsNullOrEmpty(strLastBackupFileName) == false)
                            {
                                DeleteDataFiles(strLastBackupFileName);

                                this.AppendResultText("自动删除上次创建的大备份文件 " + strLastBackupFileName + "\r\n");
                            }
                        }
                        temp_breakpoint = null;
                    }

                    // 先从远端复制整个数据库，然后从开始复制时的日志末尾进行同步
                    this.AppendResultText("指定的数据库\r\n");

                    DeleteDataFiles(strBackupFileName);

                    PrepareBackupFileName(strBackupFileName, false);
                    this.AppendResultText("本次大备份文件为 " + strBackupFileName + "\r\n");

                    // 采纳先前创建好的复制并继续的断点信息
                    // breakpoints = all_breakpoints;

                    // 建立数据库定义文件
                    {
                        string strDefFileName = GetDatabaseDefFileName(strBackupFileName);
                        nRet = CreateDatabaseDefFile(channel,
                            strDbNameList,
                            strDefFileName,
                            out strError);
                        if (nRet == -1)
                        {
                            this.AppendResultText("创建数据库定义文件失败: " + strError + "\r\n");
                            WriteStateFile(strDefFileName, null);  // 表示文件创建出错
                            return;
                        }
                    }

                    // 建立要获取的记录路径文件
                    nRet = CreateRecPathFile(channel,
                        strDbNameList,
                        strRecPathFileName,
                        out strError);
                    if (nRet == -1)
                    {
                        this.AppendResultText("创建记录路径文件失败: " + strError + "\r\n");
                        WriteStateFile(strBackupFileName, null);  // 表示文件创建出错
                        return;
                    }
                }

                // Debug.Assert(breakpoints != null, "");

                this.AppendResultText("计划进行的处理：\r\n---\r\n"
                    + (breakpoint == null ? "备份全部数据库" : breakpoint.GetSummary())
                    + "\r\n---\r\n\r\n");

                m_nRecordCount = 0;

                BreakPointInfo output_breakpoint = null;

                // for (int i = 0; i < breakpoints.Count; i++)
                {
                    // BreakPointInfo info = breakpoints[i];

                    // return:
                    //      -1  出错
                    //      0   处理被中断
                    //      1   成功
                    nRet = BackupDatabase(
                        channel,
                        strRecPathFileName,
                        breakpoint,
                        strBackupFileName,
                        out output_breakpoint,
                        out strError);

                    if (nRet == -1 || nRet == 0)
                    {
                        // 保存断点文件
                        SaveBreakPoint(output_breakpoint, true);
                        goto ERROR1;
                    }

                    // breakpoints.Remove(info);
                    // i--;

                    // 保存断点文件
                    SaveBreakPoint(output_breakpoint, false);

                    try
                    {
                        File.Delete(strRecPathFileName);
                    }
                    catch
                    {

                    }
                }

                // TODO: 如果集合为空，需要删除断点信息文件
                // 正常结束，复位断点
                if (this.StartInfos.Count == 0)
                {
                    // this.App.RemoveBatchTaskBreakPointFile(this.Name);
                    this.ClearTask();
                }

                this.StartInfo.Start = "";

                // AppendResultText("针对消息库 " + strMessageDbName + " 的循环结束。共处理 " + nRecCount.ToString() + " 条记录。\r\n");

                // TODO: 在断点文件中记载 StartInfos 内容

                this.AppendResultText("大备份结束。结果在文件 " + strBackupFileName + " 中\r\n");

#if NO
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
#endif

                DoPendingCommands(strBackupFileName);
                return;
            ERROR1:
                this.ErrorInfo = strError;
                this.AppendResultText(strError + "\r\n");
                this.SetProgressText(strError);
                DoPendingCommands(strBackupFileName);
                return;
            }
            finally
            {

            }

        }

        void DoPendingCommands(string strBackupFileName)
        {
            if (string.IsNullOrEmpty(strBackupFileName))
                return;

            // 看看是否有延迟的命令需要执行
            if (this._pendingCommands.Count > 0)
            {
                if (this._pendingCommands.IndexOf("abort") != -1)
                {
                    this.ClearTask();
                    // 删除大备份文件
                    DeleteDataFiles(strBackupFileName);
                    this.AppendResultText("大备份文件 " + strBackupFileName + " 被清除\r\n");
                }
                this._pendingCommands.Clear();
            }
        }

        // 删除 .dp2bak 文件和 .dbdef.zip 文件。包括状态文件
        static void DeleteDataFiles(string strFileName)
        {
            DeleteDataFile(strFileName);

            DeleteDataFile(GetDatabaseDefFileName(strFileName));
        }

        // 删除一个数据文件和它的状态文件
        static void DeleteDataFile(string strFileName)
        {
            try
            {
                File.Delete(strFileName);
            }
            catch
            {

            }

            try
            {
                File.Delete(strFileName + LibraryServerUtil.STATE_EXTENSION);
            }
            catch
            {

            }
        }

        // 根据备份文件名，构造数据库定义文件名
        static string GetDatabaseDefFileName(string strBackupFileName)
        {
            return Path.GetDirectoryName(strBackupFileName) + "\\" + Path.GetFileNameWithoutExtension(strBackupFileName) + ".dbdef.zip";
        }

        // 如果没有扩展名部分，要自动加上 .dp2bak
        static string CanonializeBackupFileName(string strFileName)
        {
            string ext = Path.GetExtension(strFileName);
            if (string.IsNullOrEmpty(ext))
                return strFileName + ".dp2bak";
            return strFileName;
        }

        // parameters:
        //      bContinue   是否为从断点位置继续模式。
        void PrepareBackupFileName(string strBackupFileName,
            bool bContinue)
        {
            List<string> filenames = new List<string>();

            {
                string strDefFileName = GetDatabaseDefFileName(strBackupFileName);
                if (bContinue == false)
                    WriteStateFile(strDefFileName, "creating");  // 表示文件正在创建中

                filenames.Add(GetLogicPath(strDefFileName));
            }

            {
                WriteStateFile(strBackupFileName, "creating");  // 表示文件正在创建中

                filenames.Add(GetLogicPath(strBackupFileName));
            }

            this.OutputFileNames = StringUtil.MakePathList(filenames);
#if NO
            this.OutputFileNames = GetLogicPath(strDefFileName)
                + ","
                + GetLogicPath(strBackupFileName);
#endif

            this.eventStarted.Set();
        }

        string GetLogicPath(string strBackupFileName)
        {
            string strBackupFilePath = strBackupFileName.Substring(this.App.DataDir.Length).Replace("\\", "/");
            if (strBackupFilePath.Length > 0 && strBackupFilePath[0] == '/')
                strBackupFilePath = "!" + strBackupFilePath.Substring(1);

            return strBackupFilePath;
        }

        // parameters:
        //      strState    finish/abort/error 表示文件写入完成
        //                  creating 表示文件正在创建中
        static void WriteStateFile(string strBackupFileName, string strState)
        {
            string strFileName = strBackupFileName + LibraryServerUtil.STATE_EXTENSION;
            if (strState == null || strState == "finish")
                File.Delete(strFileName);
            else
                File.WriteAllText(strFileName, strState);
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
        int ReadBreakPoint(out BreakPointInfo breakpoint,
            out string strError)
        {
            strError = "";
            breakpoint = null;

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
                strError = "从断点位置启动失败，因当前还没有断点信息。请改以从头方式来启动";
                return 0;
            }

            // 可能会抛出异常
            breakpoint = BreakPointInfo.Build(strText);
            return 1;
        }

        // 保存断点信息
        void SaveBreakPoint(BreakPointInfo info,
            bool bClearStartInfos)
        {
            if (info == null
                || (info != null && string.IsNullOrEmpty(info.DbName) == true && string.IsNullOrEmpty(info.RecID) == true))
            {
                this.App.RemoveBatchTaskBreakPointFile(this.Name);
            }
            else
            {
                // 写入断点文件
                this.App.WriteBatchTaskBreakPointFile(this.Name,
                    info.ToString());
            }

            if (bClearStartInfos)
                this.StartInfos = new List<BatchTaskStartInfo>();   // 避免残余信息对后一轮运行发生影响
        }

        int m_nRecordCount = 0;

        const int BATCH_SIZE = 1000;

        // parameters:
        //      strBackupFileName   备份文件名。扩展名应为 .dp2bak
        // return:
        //      -1  出错
        //      0   处理被中断
        //      1   成功
        int BackupDatabase(
            RmsChannel channel,
            string strRecPathFileName,
            BreakPointInfo info,
            string strBackupFileName,
            out BreakPointInfo breakpoint,
            out string strError)
        {
            strError = "";

            breakpoint = new BreakPointInfo();
            breakpoint.BackupFileName = strBackupFileName;

            try
            {
                ExportUtil export_util = new ExportUtil();
                export_util.SafeMode = true;
                export_util.TempDir = this.App.TempDir;
                int nRet = export_util.Begin(null,
    strBackupFileName,
    out strError);
                if (nRet == -1)
                    return -1;

                try
                {

                    List<string> lines = new List<string>();
                    using (StreamReader sr = new StreamReader(strRecPathFileName, Encoding.UTF8))
                    {
                        long lTotalLength = sr.BaseStream.Length;

                        // 跳过断点位置前，以前已经处理过的行
                        if (info != null
                            && string.IsNullOrEmpty(info.DbName) == false
                            && string.IsNullOrEmpty(info.RecID) == false)
                        {
                            while (true)
                            {
                                if (this.Stopped == true)
                                {
                                    strError = "中断";
                                    WriteStateFile(strBackupFileName, "abort");  // 表示文件创建过程被中断。文件内容不完整
                                    return 0;
                                }

                                string line = sr.ReadLine();
                                if (line == null)
                                    break;
                                string strDbName = ResPath.GetDbName(line);
                                string strID = ResPath.GetRecordId(line);

                                if (info.DbName == strDbName && info.RecID == strID)
                                    break;
                            }
                        }

                        this.AppendResultText("开始写入大备份文件" + strBackupFileName + "\r\n");

                        while (true)
                        {
                            if (this.Stopped == true)
                            {
                                strError = "中断";
                                WriteStateFile(strBackupFileName, "abort");  // 表示文件创建过程被中断。文件内容不完整
                                return 0;
                            }

                            string line = sr.ReadLine();
                            if (line != null)
                                lines.Add(line);

                            if (lines.Count >= BATCH_SIZE
                                || (line == null && lines.Count > 0))
                            {
                                RmsBrowseLoader loader = new RmsBrowseLoader();
                                loader.Channel = channel;
                                loader.Format = "id,xml,timestamp,metadata";
                                loader.RecPaths = lines;

                                foreach (Record record in loader)
                                {
                                    if (this.Stopped == true)
                                    {
                                        strError = "中断";
                                        WriteStateFile(strBackupFileName, "abort");  // 表示文件创建过程被中断。文件内容不完整
                                        return 0;
                                    }

                                    // TODO: 检查 RecordBody 是否为 null
                                    nRet = export_util.ExportOneRecord(
        channel,
        null,
        this.App.WsUrl,
        record.Path,
        record.RecordBody.Xml,
        record.RecordBody.Metadata,
        record.RecordBody.Timestamp,
        out strError);
                                    if (nRet == -1)
                                    {
                                        WriteStateFile(strBackupFileName, "error");  // 表示文件创建过程出错
                                        return -1;
                                    }

                                    breakpoint.DbName = ResPath.GetDbName(record.Path);
                                    breakpoint.RecID = ResPath.GetRecordId(record.Path);

                                    long lCurrent = sr.BaseStream.Position;

                                    SetProgressText(m_nRecordCount.ToString() + " " + record.Path + " " + GetPercent((double)lCurrent, lTotalLength));

                                    // 每 100 条显示一行
                                    if ((m_nRecordCount % 100) == 0)
                                        this.AppendResultText("已输出记录 " + record.Path + "  " + (m_nRecordCount + 1).ToString() + "\r\n");
                                    m_nRecordCount++;

                                }

                                lines.Clear();
                            }

                            if (line == null)
                                break;
                        }

                        Debug.Assert(lines.Count == 0, "");
                        breakpoint = null;
                    }
                }
                finally
                {
                    export_util.End();
                }

                WriteStateFile(strBackupFileName, "finish");  // 表示文件已经创建完成
                return 1;
            }
            catch (Exception ex)
            {
                strError = "BackupDatabase() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                WriteStateFile(strBackupFileName, "error");  // 表示文件创建过程出错
                return -1;
            }
        }

        static string GetPercent(double v1, double v2)
        {
            double ratio = v1 / v2;
            // return String.Format("{0,3:N}", ratio * (double)100) + "%";
            return String.Format("{0:0%}", ratio);
        }

        // 一个服务器的断点信息
        class BreakPointInfo
        {
            public string DbName = "";    // 数据库名
            public string RecID = "";       // 已经处理到的 ID

            public string Function = "";    // 功能。

            public string BackupFileName = "";

            // 通过字符串构造
            public static BreakPointInfo Build(string strText)
            {
                Hashtable table = StringUtil.ParseParameters(strText);

                BreakPointInfo info = new BreakPointInfo();
                info.DbName = (string)table["dbname"];
                info.RecID = (string)table["recid"];
                info.Function = (string)table["function"];
                info.BackupFileName = (string)table["backup_filename"];
                return info;
            }

            // 变换为字符串
            public override string ToString()
            {
                Hashtable table = new Hashtable();
                table["dbname"] = this.DbName;
                table["recid"] = this.RecID;
                table["function"] = this.Function;
                table["backup_filename"] = this.BackupFileName;
                return StringUtil.BuildParameterString(table);
            }

            // 小结文字
            public string GetSummary()
            {
                string strResult = "";
                if (string.IsNullOrEmpty(this.RecID) == false)
                {
                    strResult += "从断点 " + this.DbName + "/" + this.RecID.ToString() + " 开始";
                }
                strResult += " (备份文件名=" + this.BackupFileName + ")";

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
            public static BreakPointCollection BuildFromDbNameList(string strText,
                string strFunction,
                string strBackupFileName)
            {
                BreakPointCollection infos = new BreakPointCollection();

                string[] dbnames = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string dbname in dbnames)
                {
                    BreakPointInfo info = new BreakPointInfo();
                    info.DbName = dbname;
                    info.Function = strFunction;
                    info.BackupFileName = strBackupFileName;
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

        // 压缩一批文件 .zip 文件
        // parameters:
        //      filenames   要压缩的源文件路径，数组
        //      shortfilenames 在 .zip 文件中的文件路径，数组
        static int CompressFiles(
            List<string> filenames,
            List<string> shortfilenames,
            string strZipFileName,
            Encoding encoding,
            bool bAppend,
            out string strError)
        {
            strError = "";

            if (filenames.Count != shortfilenames.Count)
            {
                strError = "filenames.Count != shortfilenames.Count";
                return -1;
            }

            if (bAppend == false)
            {
                if (File.Exists(strZipFileName) == true)
                {
                    try
                    {
                        File.Delete(strZipFileName);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    catch (DirectoryNotFoundException)
                    {

                    }
                }
            }

            // string strHead = Path.GetDirectoryName(strDirectory);
            // Console.WriteLine("head=["+strHead+"]");

            using (ZipFile zip = new ZipFile(strZipFileName, encoding))
            {
                // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                // https://dotnetzip.codeplex.com/workitem/14087
                // uncommenting the following line can be used as a work-around
                zip.ParallelDeflateThreshold = -1;

                for (int i = 0; i < filenames.Count; i++)
                {
                    string filename = filenames[i];
                    string shortfilename = shortfilenames[i];
                    string directoryPathInArchive = Path.GetDirectoryName(shortfilename);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save(strZipFileName);
            }

            return filenames.Count;
        }

        int AppendLibraryDataFiles(string strOutputFileName,
            out string strError)
        {
            strError = "";

#if NO
            string strFileName = Path.Combine(this.App.DataDir, "library.xml");
            string strTargetPath = Path.Combine("_datadir", "library.xml");
#endif

            List<string> filenames = new List<string>();
            filenames.Add(Path.Combine(this.App.DataDir, "library.xml"));

            filenames.AddRange(PathUtil.GetFileNames(Path.Combine(this.App.DataDir, "cfgs"),
    (fi) =>
    {
        return true;
    }
    ));
            List<string> shortfilenames = new List<string>();
            foreach (string filename in filenames)
            {
                shortfilenames.Add("_datadir" + filename.Substring(this.App.DataDir.Length));
            }

            return CompressFiles(
                filenames,
                shortfilenames,
                strOutputFileName,
                Encoding.UTF8,
                true,
                out strError);
        }

        // 建立数据库定义文件
        // 注：调用前要确保 .~state 文件已经存在，内容为 creating。这样可以保证前端在下载过程中会一直等待内容文件创建
        // 注：由于 Ionic 的库函数在追加写入 .zip 文件过程中，可能会随时修改已经写入的部分内容，所以需要在本函数的处理过程中，
        // 先整体写入临时文件，最后再拷贝到目标文件。
        int CreateDatabaseDefFile(RmsChannel channel,
    string strDbNameList,
    string strOutputFileNameParam,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            this.AppendResultText("正在准备数据库定义文件\r\n");

            string strTempFilePath = strOutputFileNameParam + ".tmp";

            try
            {
                if (AppendLibraryDataFiles(strTempFilePath, out strError) == -1)
                    return -1;

                List<string> dbnames = StringUtil.SplitList(strDbNameList);

                StringUtil.RemoveBlank(ref dbnames);
                StringUtil.RemoveDupNoSort(ref dbnames);

                if (dbnames.Count == 0)
                    dbnames.Add("*");

                {
                    List<string> results = new List<string>();
                    foreach (string dbname in dbnames)
                    {
                        if (string.IsNullOrEmpty(dbname) == true || dbname == "*")
                        {
                            // 如果数据库名为 *，表示希望获取所有的数据库
                            List<string> temp = null;
                            // 获得所有数据库名
                            nRet = GetAllDbNames(out temp,
                    out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            results.AddRange(temp);
                        }
                        else
                            results.Add(dbname);
                    }

                    StringUtil.RemoveDupNoSort(ref results);
                    dbnames = results;
                }

                nRet = this.App.BackupDatabaseDefinition(
                    channel,
                    StringUtil.MakePathList(dbnames),
                    strTempFilePath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                CopyFile(strTempFilePath, strOutputFileNameParam);

                WriteStateFile(strOutputFileNameParam, "finish");  // 表示文件已经创建完成
                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                File.Delete(strTempFilePath);
                this.AppendResultText("结束准备数据库定义\r\n");
            }

        ERROR1:
            WriteStateFile(strOutputFileNameParam, "error");
            return -1;
        }

        // 特指的拷贝文件的功能。确保打开写入目标文件的中途，其他线程依然可以共享打开同一文件
        static void CopyFile(string strSourceFileName,
            string strTargetFileName)
        {
            using (FileStream outputfile = File.Open(
    strTargetFileName,
    FileMode.Create,
    FileAccess.Write,
    FileShare.ReadWrite))
            using (FileStream fileSource = File.Open(
                strSourceFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                DumpStream(fileSource, outputfile, true);
            }
        }

        // 写入的时候对 FileStream 加了 Lock
        public static long DumpStream(FileStream streamSource,
    FileStream streamTarget,
    bool bFlush)
        {
            int nChunkSize = 8192;
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)
                {
                    long lock_start = streamTarget.Position;
                    streamTarget.Lock(lock_start, n);
                    try
                    {
                        streamTarget.Write(bytes, 0, n);
                    }
                    finally
                    {
                        streamTarget.Unlock(lock_start, n);
                    }

                    if (bFlush == true)
                        streamTarget.Flush();
                }

                if (n <= 0)
                    break;

                lLength += n;
            }

            return lLength;
        }

        // TODO: 中途要显示进度信息
        int CreateRecPathFile(RmsChannel channel,
            string strDbNameList,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            int nRecordCount = 0;

            List<string> dbnames = StringUtil.SplitList(strDbNameList);

            StringUtil.RemoveBlank(ref dbnames);
            StringUtil.RemoveDupNoSort(ref dbnames);

            if (dbnames.Count == 0)
                dbnames.Add("*");

            {
                List<string> results = new List<string>();
                foreach (string dbname in dbnames)
                {
                    if (string.IsNullOrEmpty(dbname) == true || dbname == "*")
                    {
                        // 如果数据库名为 *，表示希望获取所有的数据库
                        List<string> temp = null;
                        // 获得所有数据库名
                        int nRet = GetAllDbNames(out temp,
                out strError);
                        if (nRet == -1)
                            return -1;
                        results.AddRange(temp);
                    }
                    else
                        results.Add(dbname);
                }

                StringUtil.RemoveDupNoSort(ref results);
                dbnames = results;
            }

            this.AppendResultText("正在准备路径\r\n");

            try
            {
                using (StreamWriter sw = new StreamWriter(strOutputFileName, false, Encoding.UTF8))
                {
                    foreach (string dbname in dbnames)
                    {
                        string strQueryXml = "<target list='"
                            + dbname
                            + ":" + "__id'><item><word>1-9999999999</word><match>exact</match><relation>range</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                        long lRet = channel.DoSearch(strQueryXml, "default", out strError);
                        if (lRet == -1)
                            return -1;
                        if (lRet == 0)
                            continue;

                        SearchResultLoader loader = new SearchResultLoader(channel,
                        null,
                        "default",
                        "id");
                        loader.ElementType = "Record";

                        foreach (Record record in loader)
                        {
                            if (record.RecordBody != null && record.RecordBody.Result != null
                                && record.RecordBody.Result.ErrorCode == ErrorCodeValue.NotFound)
                                continue;
                            sw.WriteLine(record.Path);

                            SetProgressText(nRecordCount.ToString());

#if NO
                            // 每 1000 条显示一行
                            if ((nRecordCount % 1000) == 0)
                                this.AppendResultText("已准备路径 " + record.Path + "  " + (nRecordCount + 1).ToString() + "\r\n");
#endif
                            nRecordCount++;
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                this.AppendResultText("结束准备路径。共 " + nRecordCount + "\r\n");
            }
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
