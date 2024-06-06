using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryServer.Common;

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


        // 一次操作循环
        public override void Worker()
        {
            // 2024/5/13
            if (this.App.vdbs == null)
            {
                this.AppendErrorText("启动失败: 当前系统 app.vdbs 为 null，请先解决此问题，再重新启动日志恢复任务\r\n");
            }

            // 把系统挂起
            // this.App.HangupReason = HangupReason.LogRecover;
            this.App.AddHangup("LogRecover");
            try
            {

                BatchTaskStartInfo startinfo = this.StartInfo;
                if (startinfo == null)
                    startinfo = new BatchTaskStartInfo();   // 按照缺省值来

                int nRet = LogRecoverStart.ParseLogRecoverStart(startinfo.Start,
                    out long lStartIndex,// 开始位置
                    out string strStartFileName,// 开始文件名
                    out string strError);
                if (nRet == -1)
                {
                    this.AppendErrorText("启动失败: " + strError + "\r\n");
                    return;
                }

                //
                nRet = LogRecoverParam.ParseLogRecoverParam(startinfo.Param,
                    out string strDirectory,
                    out string strRecoverLevel,
                    out bool bClearFirst,
                    out bool bContinueWhenError,
                    out string strStyle,
                    out strError);
                if (nRet == -1)
                {
                    this.AppendErrorText("启动失败: " + strError + "\r\n");
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
                    this.AppendErrorText("启动失败: 启动参数Param中的recoverLevel枚举值 '" + strRecoverLevel + "' 错误: " + ex.Message + "\r\n");
                    return;
                }

                this.App.WriteErrorLog($"日志恢复 任务启动。恢复级别为: {this.RecoverLevel}");

                this.AppendResultText($"日志恢复级别为: {this.RecoverLevel}\r\n");

#if REMOVED
                // 当为容错恢复级别时，检查当前全部读者库的检索点是否符合要求
                if ((this.RecoverLevel & RecoverLevel.RobustMask) == RecoverLevel.RobustMask)
                {
                    // 检查全部读者库的检索途径，看是否满足都有“所借册条码号”这个检索途径的这个条件
                    // return:
                    //      -1  出错
                    //      0   不满足
                    //      1   满足
                    nRet = this.App.DetectReaderDbFroms(out strError);
                    if (nRet == -1)
                    {
                        this.AppendErrorText("检查读者库检索点时发生错误: " + strError + "\r\n");
                        return;
                    }
                    if (nRet == 0)
                    {
                        this.AppendErrorText("在容错恢复级别下，当前读者库中有部分或全部读者库缺乏“所借册条码号”检索点，无法进行日志恢复。请按照日志恢复要求，刷新所有读者库的检索点配置，然后再进行日志恢复\r\n");
                        return;
                    }
                }
#endif

                // TODO: 检查当前是否有 重建检索点 的后台任务正在运行，或者还有没有运行完的部分。
                // 要求重建检索点的任务运行完以后才能执行日志恢复任务

                if (bClearFirst == true)
                {
                    this.AppendResultText("清除全部数据库记录\r\n");

                    nRet = this.App.ClearAllDbs(this.RmsChannels,
                        out strError);
                    if (nRet == -1)
                    {
                        this.AppendErrorText("清除全部数据库记录时发生错误: " + strError + "\r\n");
                        return;
                    }
                }

                bool bStart = false;
                if (String.IsNullOrEmpty(strStartFileName) == true)
                {
                    // 做所有文件
                    bStart = true;
                }

                if (string.IsNullOrEmpty(strDirectory))
                {
                    strDirectory = this.App.OperLog.Directory;
                    this.AppendResultText($"日志文件目录为 dp2library 默认操作日志目录 {strDirectory}\r\n");
                }
                else
                    this.AppendResultText($"日志文件目录为前端指定的目录 {strDirectory}\r\n");


                // 列出所有日志文件
                DirectoryInfo di = new DirectoryInfo(strDirectory);

                // 注: GetFiles() 由于此方法检查具有 8.3 文件名格式和长文件名格式的文件名，因此类似于"*1.txt"的搜索模式可能会返回意外的 * 文件名。 例如，使用"1.txt"的搜索模式将返回 * * "longfilename.txt"，因为等效的 8.3 文件名格式将为"longf~1.txt"。
                FileInfo[] fis = di.GetFiles("*.log");

                // BUG!!! 以前缺乏排序。2008/2/1
                Array.Sort(fis, new FileInfoCompare());

                for (int i = 0; i < fis.Length; i++)
                {
                    if (this.Stopped == true)
                        break;

                    string strFileName = fis[i].Name;

                    // this.AppendResultText("检查文件 " + strFileName + "\r\n");

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
                        nRet = DoOneLogFile(
                            strDirectory,
                            strFileName,
                            lStartIndex,
                            bContinueWhenError,
                            strStyle,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lStartIndex = 0;    // 第一个文件以后的文件就全做了
                    }
                }

                this.AppendResultText("循环结束\r\n");

                this.App.WriteErrorLog("日志恢复 任务结束。");
                if (this.App.vdbs == null)
                    this.App.WriteErrorLog("*** 注意 app.vdbs 为 null，系统部分功能处于瘫痪状态，请及时解决此问题");
                // this.ErrorInfo = StringUtil.MakePathList(_errors, "\r\n");
                // TODO: 可以考虑从 result 文本文件中搜集所有错误信息行，放入 ErrorInfo 中，不过得有个极限行数限制
                return;

            ERROR1:
                // 2019/4/25
                this.AppendResultText($"{strError}\r\n");
                this.App.WriteErrorLog($"*** 日志恢复任务出错: {strError}");
                if (this.App.vdbs == null)
                    this.App.WriteErrorLog("*** 注意 app.vdbs 为 null，系统部分功能处于瘫痪状态，请及时解决此问题");

                this.ErrorInfo = strError;
                return;
            }
            catch (Exception ex)
            {
                // 2017/11/30
                string strError = "*** 日志恢复任务出现异常: " + ExceptionUtil.GetDebugText(ex);
                this.AppendErrorText(strError);
                this.App.WriteErrorLog(strError);
                this.ErrorInfo = strError;
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
        int DoOneLogFile(
            string strDirectory,
            string strFileName,
            long lStartIndex,
            bool bContinueWhenError,
            string strStyle,
            out string strError)
        {
            strError = "";

            this.AppendResultText("做文件 " + strFileName + "\r\n");

            Debug.Assert(this.App != null, "");
            var cache = new OperLogFileCache();
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

                        // long lAttachmentLength = 0;
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
                        int nRet = /*this.App.*/OperLogUtility.GetOperLog(
                            cache,
                            strDirectory,
                            //"*",
                            strFileName,
                            lIndex,
                            lHint,
                            "supervisor", // level-0
                            "", // strFilter
                            (ref string xml, out string error) =>
                            {
                                error = "";
                                // 限制记录观察范围
                                // 虽然是全局用户，也要限制记录尺寸
                                int ret = OperLog.ResizeXml(
                                    "supervisor",
                                    "",
                                    ref xml,
                                    out error);
                                if (ret == -1)
                                {
                                    ret = 1;   // 只好返回
                                }
                                return ret;
                            },
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
                        // TODO: 如何搜集出错信息并返回给前端？ 特别是测试时启动后台任务的情况
                        nRet = DoOperLogRecord(
                            this.RecoverLevel,
                            strXml,
                            attachment,
                            strStyle,
                            (warning) =>
                            {
                                this.AppendResultText($"*** 警告: {warning}\r\n");
                            },
                            out strError);
                        if (nRet == -1)
                        {
                            this.AppendErrorText("*** 做日志记录 " + strFileName + " " + (lIndex).ToString() + " 时发生错误：" + strError + "\r\n");

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
                cache.Dispose();
            }
        }

    }
}
