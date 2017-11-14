using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 日志恢复 批处理任务
    /// </summary>
    public class OperLogRecover : BatchTask
    {
        // 日志恢复级别
        public RecoverLevel RecoverLevel = RecoverLevel.Snapshot;

        public OperLogRecover(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.PerTime = 0;
        }

        public override string DefaultName
        {
            get
            {
                return "日志恢复";
            }
        }

        // 是否应该停止处理，用于日志恢复任务
        public override bool Stopped
        {
            get
            {
                return this.m_bClosed;
            }
        }

        // 解析 开始 参数
        static int ParseLogRecoverStart(string strStart,
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

        // 解析通用启动参数
        // 格式
        /*
         * <root recoverLevel='...' clearFirst='...' continueWhenError='...'/>
         * recoverLevel 缺省为 Snapshot
         * clearFirst 缺省为 false
         * continueWhenError 缺省值为 false
         * */
        public static int ParseLogRecoverParam(string strParam,
            out string strRecoverLevel,
            out bool bClearFirst,
            out bool bContinueWhenError,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strRecoverLevel = "";
            bContinueWhenError = false;

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

            /*
            Logic = 0,  // 逻辑操作
            LogicAndSnapshot = 1,   // 逻辑操作，若失败则转用快照恢复
            Snapshot = 3,   // （完全的）快照
            Robust = 4,
             * */

            strRecoverLevel = DomUtil.GetAttr(dom.DocumentElement,
                "recoverLevel");
            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            // 2016/3/8
            bContinueWhenError = DomUtil.GetBooleanParam(dom.DocumentElement,
                "continueWhenError",
                false);

            return 0;
        }

        // 一次操作循环
        public override void Worker()
        {
            // 把系统挂起
            // this.App.HangupReason = HangupReason.LogRecover;
            this.App.AddHangup("LogRecover");
            try
            {
                string strError = "";

                BatchTaskStartInfo startinfo = this.StartInfo;
                if (startinfo == null)
                    startinfo = new BatchTaskStartInfo();   // 按照缺省值来

                long lStartIndex = 0;// 开始位置
                string strStartFileName = "";// 开始文件名
                int nRet = ParseLogRecoverStart(startinfo.Start,
                    out lStartIndex,
                    out strStartFileName,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                //
                string strRecoverLevel = "";
                bool bClearFirst = false;
                bool bContinueWhenError = false;

                nRet = ParseLogRecoverParam(startinfo.Param,
                    out strRecoverLevel,
                    out bClearFirst,
                    out bContinueWhenError,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("启动失败: " + strError + "\r\n");
                    return;
                }

                if (String.IsNullOrEmpty(strRecoverLevel) == true)
                    strRecoverLevel = "Snapshot";

                try
                {
                    this.RecoverLevel = (RecoverLevel)Enum.Parse(typeof(RecoverLevel), strRecoverLevel, true);
                }
                catch (Exception ex)
                {
                    this.AppendResultText("启动失败: 启动参数Param中的recoverLevel枚举值 '" + strRecoverLevel + "' 错误: " + ex.Message + "\r\n");
                    return;
                }

                this.App.WriteErrorLog("日志恢复 任务启动。");

                // 当为容错恢复级别时，检查当前全部读者库的检索点是否符合要求
                if (this.RecoverLevel == LibraryServer.RecoverLevel.Robust)
                {
                    // 检查全部读者库的检索途径，看是否满足都有“所借册条码号”这个检索途径的这个条件
                    // return:
                    //      -1  出错
                    //      0   不满足
                    //      1   满足
                    nRet = this.App.DetectReaderDbFroms(out strError);
                    if (nRet == -1)
                    {
                        this.AppendResultText("检查读者库检索点时发生错误: " + strError + "\r\n");
                        return;
                    }
                    if (nRet == 0)
                    {
                        this.AppendResultText("在容错恢复级别下，当前读者库中有部分或全部读者库缺乏“所借册条码号”检索点，无法进行日志恢复。请按照日志恢复要求，刷新所有读者库的检索点配置，然后再进行日志恢复\r\n");
                        return;
                    }
                }

                // TODO: 检查当前是否有 重建检索点 的后台任务正在运行，或者还有没有运行完的部分。
                // 要求重建检索点的任务运行完以后才能执行日志恢复任务

                if (bClearFirst == true)
                {
                    nRet = this.App.ClearAllDbs(this.RmsChannels,
                        out strError);
                    if (nRet == -1)
                    {
                        this.AppendResultText("清除全部数据库记录时发生错误: " + strError + "\r\n");
                        return;
                    }
                }

                bool bStart = false;
                if (String.IsNullOrEmpty(strStartFileName) == true)
                {
                    // 做所有文件
                    bStart = true;
                }


                // 列出所有日志文件
                DirectoryInfo di = new DirectoryInfo(this.App.OperLog.Directory);

                FileInfo[] fis = di.GetFiles("*.log");

                // BUG!!! 以前缺乏排序。2008/2/1
                Array.Sort(fis, new FileInfoCompare());

                for (int i = 0; i < fis.Length; i++)
                {
                    if (this.Stopped == true)
                        break;

                    string strFileName = fis[i].Name;

                    this.AppendResultText("检查文件 " + strFileName + "\r\n");

                    if (bStart == false)
                    {
                        // 从特定文件开始做
                        if (string.CompareOrdinal(strStartFileName, strFileName) <= 0)  // 2015/9/12 从等号修改为 Compare
                        {
                            bStart = true;
                            if (lStartIndex < 0)
                                lStartIndex = 0;
                            // lStartIndex = Convert.ToInt64(startinfo.Param);
                        }
                    }

                    if (bStart == true)
                    {
                        nRet = DoOneLogFile(strFileName,
                            lStartIndex,
                            bContinueWhenError,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lStartIndex = 0;    // 第一个文件以后的文件就全做了
                    }

                }

                this.AppendResultText("循环结束\r\n");

                this.App.WriteErrorLog("日志恢复 任务结束。");

                return;

            ERROR1:
                this.ErrorInfo = strError;
                return;
            }
            finally
            {
                // this.App.HangupReason = HangupReason.None;
                this.App.ClearHangup("LogRecover");

                string strFinish = "批处理任务结束";
                if (string.IsNullOrEmpty(this.ErrorInfo) == false)
                    strFinish += ":" + this.ErrorInfo;
                SetProgressText(strFinish); 
            }
        }

        public class FileInfoCompare : IComparer
        {

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                return ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
            }

        }

        // 处理一个日志文件的恢复任务
        // parameters:
        //      strFileName 纯文件名
        //      lStartIndex 开始的记录（从0开始计数）
        // return:DoOperLogRecord(
        //      -1  error
        //      0   file not found
        //      1   succeed
        int DoOneLogFile(string strFileName,
            long lStartIndex,
            bool bContinueWhenError,
            out string strError)
        {
            strError = "";

            this.AppendResultText("做文件 " + strFileName + "\r\n");

            Debug.Assert(this.App != null, "");
            string strTempFileName = this.App.GetTempFileName("logrecover");    // Path.GetTempFileName();
            try
            {
                long lIndex = 0;
                long lHint = -1;
                long lHintNext = -1;

                for (lIndex = lStartIndex; ; lIndex++)
                {
                    if (this.Stopped == true)
                        break;

                    string strXml = "";

                    if (lIndex != 0)
                        lHint = lHintNext;

                    SetProgressText(strFileName + " 记录" + (lIndex + 1).ToString());

                    using (Stream attachment = File.Create(strTempFileName))
                    {
                        // Debug.Assert(!(lIndex == 182 && strFileName == "20071225.log"), "");

                        long lAttachmentLength = 0;
                        // 获得一个日志记录
                        // parameters:
                        //      strFileName 纯文件名,不含路径部分
                        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
                        //              目前的含义是记录起始位置。
                        //      attachment  承载输出的附件部分的 Stream 对象。如果为 null，表示不输出附件部分
                        //                  本函数返回后，attachment 的文件指针在文件末尾。调用时需引起注意
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   succeed
                        //      2   超过范围
                        int nRet = this.App.OperLog.GetOperLog(
                            "*",
                            strFileName,
                            lIndex,
                            lHint,
                            "supervisor", // level-0
                            "", // strFilter
                            out lHintNext,
                            out strXml,
                            // ref attachment,
                            attachment,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                            return 0;
                        if (nRet == 2)
                        {
                            // 最后一条补充提示一下
                            if (((lIndex - 1) % 100) != 0)
                                this.AppendResultText("做日志记录 " + strFileName + " " + (lIndex).ToString() + "\r\n");
                            break;
                        }

                        // 处理一个日志记录

                        if ((lIndex % 100) == 0)
                            this.AppendResultText("做日志记录 " + strFileName + " " + (lIndex + 1).ToString() + "\r\n");

                        /*
                        // 测试时候在这里安排跳过
                        if (lIndex == 1 || lIndex == 2)
                            continue;
 * */

                        nRet = DoOperLogRecord(
                            this.RecoverLevel,
                            strXml,
                            attachment,
                            out strError);
                        if (nRet == -1)
                        {
                            this.AppendResultText("*** 做日志记录 " + strFileName + " " + (lIndex).ToString() + " 时发生错误：" + strError + "\r\n");

                            // 2007/6/25
                            // 如果为纯逻辑恢复(并且 bContinueWhenError 为 false)，遇到错误就停下来。这便于进行测试。
                            // 若不想停下来，可以选择“逻辑+快照”型，或者设置 bContinueWhenError 为 true
                            if (this.RecoverLevel == RecoverLevel.Logic
                                && bContinueWhenError == false)
                                return -1;
                        }
                    }
                }

                return 0;
            }
            finally
            {
                File.Delete(strTempFileName);
            }
        }

    }
}
