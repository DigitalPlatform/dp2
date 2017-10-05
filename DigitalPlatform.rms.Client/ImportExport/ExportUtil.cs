using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms.Client
{
    public enum ExportFileType
    {
        BackupFile = 0,
        XmlFile = 1,
        ISO2709File = 2,
    }

    public class ExportUtil
    {
        IWin32Window m_owner = null;

        public bool SafeMode { get; set; }
        public string TempDir { get; set; }

        public ExportFileType FileType = ExportFileType.XmlFile;

        public FileStream outputfile = null;	// Backup和Xml格式输出都需要这个

        public XmlTextWriter writer = null;   // Xml格式输出时需要这个

        bool bAppend = true;    // 本次输出是否追加在以前存在的文件末尾

        // 准备输出
        public int Begin(
            IWin32Window owner,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            this.m_owner = owner;

            if (string.IsNullOrEmpty(strOutputFileName) == true)
            {
                strError = "输出文件名不能为空";
                return -1;
            }

            string strExt = Path.GetExtension(strOutputFileName);
            if (string.Compare(strExt, ".xml", true) == 0)
                this.FileType = ExportFileType.XmlFile;
            else if (string.Compare(strExt, ".dp2bak", true) == 0)
                this.FileType = ExportFileType.BackupFile;
            else if (string.Compare(strExt, ".iso", true) == 0
                || string.Compare(strExt, ".marc", true) == 0)
                this.FileType = ExportFileType.ISO2709File;
            else
            {
                strError = "无法根据文件扩展名 '" + strExt + "' 判断输出文件的格式";
                return -1;
            }

            this.bAppend = true;

            if (String.IsNullOrEmpty(strOutputFileName) == false)
            {
                // 探测输出文件是否已经存在
                FileInfo fi = new FileInfo(strOutputFileName);
                bAppend = true;
                if (fi.Exists == true && fi.Length > 0)
                {
                    if (FileType == ExportFileType.BackupFile
                        || FileType == ExportFileType.ISO2709File)
                    {
                        if (owner != null)
                        {
                            DialogResult result = MessageBox.Show(owner,
                                "文件 '" + strOutputFileName + "' 已存在，是否追加?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)中断处理",
                                "导出数据",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (result == DialogResult.Yes)
                            {
                                bAppend = true;
                            }
                            if (result == DialogResult.No)
                            {
                                bAppend = false;
                            }
                            if (result == DialogResult.Cancel)
                            {
                                strError = "放弃处理...";
                                return -1;
                            }
                        }
                        else
                            bAppend = true;
                    }
                    else if (FileType == ExportFileType.XmlFile)
                    {
                        if (owner != null)
                        {
                            DialogResult result = MessageBox.Show(owner,
                                "文件 '" + strOutputFileName + "' 已存在，是否覆盖?\r\n\r\n--------------------\r\n注：(是)覆盖  (否)中断处理",
                                "导出数据",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result != DialogResult.Yes)
                            {
                                strError = "放弃处理...";
                                return -1;
                            }
                        }
                        else
                            bAppend = false;
                    }
                }

                // 打开文件
                if (FileType == ExportFileType.BackupFile
                    || FileType == ExportFileType.ISO2709File)
                {
                    outputfile = File.Open(
                        strOutputFileName,
                        FileMode.OpenOrCreate,	// 原来是Open，后来修改为OpenOrCreate。这样对临时文件被系统管理员手动意外删除(但是xml文件中仍然记载了任务)的情况能够适应。否则会抛出FileNotFoundException异常
                        FileAccess.Write,
                        FileShare.ReadWrite);
                }
                else if (FileType == ExportFileType.XmlFile)
                {
                    outputfile = File.Create(
                        strOutputFileName);

                    writer = new XmlTextWriter(outputfile, Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                }

            }

            if ((FileType == ExportFileType.BackupFile
                || FileType == ExportFileType.ISO2709File)
                && outputfile != null)
            {
                if (bAppend == true)
                    outputfile.Seek(0, SeekOrigin.End);	// 具有追加的能力
                else
                    outputfile.SetLength(0);
            }

            if (FileType == ExportFileType.XmlFile
&& writer != null)
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("dprms", "collection", DpNs.dprms);
                //writer.WriteStartElement("collection");
                //writer.WriteAttributeString("xmlns:marc",
                //	"http://www.loc.gov/MARC21/slim");
            }

            return 0;
        }


        public void End()
        {
            if (writer != null)
            {
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
                writer = null;
            }

            if (outputfile != null)
            {
                outputfile.Close();
                outputfile = null;
            }
        }

        // return:
        //      -1  出错
        //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
        //      1   导出了内容
        public int ExportOneRecord(
            RmsChannel channel,
            DigitalPlatform.Stop stop,
            string strServerUrl,
            string strRecPath,
            string strXmlBody,
            string strMetadata,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 2017/9/19
            if (string.IsNullOrEmpty(strXmlBody))
                return 0;

            if (this.FileType == ExportFileType.XmlFile)
            {
                XmlDocument dom = new XmlDocument();

                try
                {
                    dom.LoadXml(strXmlBody);

                    ResPath respathtemp = new ResPath();
                    respathtemp.Url = strServerUrl;
                    respathtemp.Path = strRecPath;

                    // DomUtil.SetAttr(dom.DocumentElement, "xmlns:dprms", DpNs.dprms);
                    // 给根元素设置几个参数
                    DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, respathtemp.FullPath);
                    DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(baTimestamp));

                    // DomUtil.SetAttr(dom.DocumentElement, "xmlns:marc", null);
                    dom.DocumentElement.WriteTo(writer);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

            if (this.FileType == ExportFileType.BackupFile)
            {
                Debug.Assert(channel != null, "");
                // 将主记录和相关资源写入备份文件
                if (this.SafeMode)
                {
                    // return:
                    //      -1  出错
                    //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
                    //      1   导出了内容
                    nRet = SafeWriteRecordToBackupFile(
        this.m_owner,
        channel,
        stop,
        this.outputfile,
        strRecPath, // 记录路径
        strMetadata,
        strXmlBody,
        baTimestamp,
        this.TempDir,
        out strError);
                }
                else
                {
                    // return:
                    //      -1  出错
                    //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
                    //      1   导出了内容
                    nRet = WriteRecordToBackupFile(
                        this.m_owner,
                        channel,
                        stop,
                        this.outputfile,
                        strRecPath, // 记录路径
                        strMetadata,
                        strXmlBody,
                        baTimestamp,
                        out strError);
                }
                if (nRet == -1)
                    return -1;

                return nRet;
            }
            return 1;
        }

        // 得到Xml记录中所有<file>元素的id属性值
        public static int GetFileIds(string strXml,
            out string[] ids,
            out string strError)
        {
            ids = null;
            strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载 XML 进入 DOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager mngr = new XmlNamespaceManager(dom.NameTable);
            mngr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", mngr);

            ids = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                ids[i] = DomUtil.GetAttr(node, "id");
            }
            return 0;
        }

        #region 写入 .dp2bak 文件

        // 本函数是将新的记录完整创建好以后再追加到 outputfile 末尾。能确保另一个并发的顺序读操作读入正确的内容
        // return:
        //      -1  出错
        //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
        //      1   导出了内容
        static int SafeWriteRecordToBackupFile(
            IWin32Window owner,
            RmsChannel channel,
            DigitalPlatform.Stop stop,
            Stream outputfile,
            string strPath, // 记录路径
            string strMetaData,
            string strXmlBody,
            byte[] body_timestamp,
            string strTempDir,
            out string strError)
        {
            if (string.IsNullOrEmpty(strTempDir))
            {
                strError = "strTempDir 参数值不应为空";
                return -1;
            }
            string strTempFileName = Path.Combine(strTempDir, "~" + Guid.NewGuid().ToString());
            try
            {
                using (FileStream temp_stream = File.Open(
                            strTempFileName,
                            FileMode.Create,
                            FileAccess.ReadWrite,
                            FileShare.None))
                {
                    // return:
                    //      -1  出错
                    //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
                    //      1   导出了内容
                    int nRet = WriteRecordToBackupFile(
                    owner,
                    channel,
                    stop,
                    temp_stream,
                    strPath, // 记录路径
                    strMetaData,
                    strXmlBody,
                    body_timestamp,
                    out strError);
                    if (nRet == -1)
                        return -1;

#if NO
                    if (nRet == 1)
                    {
                        // 记忆 dump 前 outputfile 的当前位置
                        long lSavePosition = outputfile.Position;
                        bool bDone = false;
                        try
                        {
                            temp_stream.Seek(0, SeekOrigin.Begin);
                            // TODO: Dump 中途是否允许灵敏中断？要注意中断以后截断目标文件
                            long lRet = StreamUtil.LockingDumpStream(temp_stream,
                                outputfile,
                                false,
                                () =>
                                {
                                    if (stop != null && stop.State != 0)
                                        return true;
                                    return false;
                                });
                            if (lRet == -1)
                            {
                                strError = "Dump 中途被用户中断";
                                return -1;
                            }
                            else
                                bDone = true;
                        }
                        finally
                        {
                            if (bDone == false)
                                outputfile.SetLength(lSavePosition);
                        }
                    }
#endif

                    if (nRet == 1)
                    {
                        temp_stream.Seek(0, SeekOrigin.Begin);
                        StreamUtil.LockingDumpStream(temp_stream,
                            outputfile,
                            false,
                            null);
                    }
                    return nRet;
                }
            }
            catch (Exception ex)
            {
                strError = "SafeWriteRecordToBackupFile() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                File.Delete(strTempFileName);
            }
        }

        // 将主记录和相关资源写入备份文件
        // 本函数如果失败，会自动把本次写入的局部内容从文件末尾抹去
        // TODO: 测试磁盘空间耗尽的情况
        // return:
        //      -1  出错
        //      0   因 strXmlBody 为空，忽略此记录，并没有导出任何内容
        //      1   导出了内容
        static int WriteRecordToBackupFile(
            IWin32Window owner,
            RmsChannel channel,
            DigitalPlatform.Stop stop,
            Stream outputfile,
            string strPath, // 记录路径
            string strMetaData,
            string strXmlBody,
            byte[] body_timestamp,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXmlBody))
            {
                strError = "strXmlBody 为空，忽略此记录";
                return 0;
            }

            Debug.Assert(String.IsNullOrEmpty(strXmlBody) == false, "strXmlBody不能为空");

            Debug.Assert(channel != null, "");
            // string strPath = strDbName + "/" + strID;

            long lStart = outputfile.Position;	// 记忆起始位置
            bool bDone = false;
            try
            {
                byte[] length = new byte[8];

                outputfile.LockingWrite(length, 0, 8);	// 临时写点数据,占据记录总长度位置

                ResPath respath = new ResPath();
                respath.Url = channel.Url;
                respath.Path = strPath;

                // 加工元数据
                StringUtil.ChangeMetaData(ref strMetaData,
                    null,
                    null,
                    null,
                    null,
                    respath.FullPath,
                    ByteArray.GetHexTimeStampString(body_timestamp));   // 2005/6/11

                // 向backup文件中保存第一个 res
                long lRet = Backup.WriteFirstResToBackupFile(
                    outputfile,
                    strMetaData,
                    strXmlBody);

                // 其余
                string[] ids = null;

                // 得到Xml记录中所有<file>元素的id属性值
                int nRet = GetFileIds(strXmlBody,
                    out ids,
                    out strError);
                if (nRet == -1)
                {
                    // outputfile.SetLength(lStart);	// 把本次追加写入的全部去掉
                    strError = "GetFileIds()出错，无法获得 XML 记录 (" + strPath + ") 中的 <dprms:file>元素的 id 属性， 因此保存记录失败，原因: " + strError;
                    goto ERROR1;
                }

                nRet = WriteResToBackupFile(
                    owner,
                    channel,
                    stop,
                    outputfile,
                    respath.Path,
                    ids,
                    out strError);
                if (nRet == -1)
                {
                    // outputfile.SetLength(lStart);	// 把本次追加写入的全部去掉
                    strError = "WriteResToBackupFile()出错，因此保存记录 (" + strPath + ") 失败，原因: " + strError;
                    goto ERROR1;
                }

                ///


                // 写入总长度
                long lTotalLength = outputfile.Position - lStart - 8;
                byte[] data = BitConverter.GetBytes(lTotalLength);

                // 返回记录最开头位置
                outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
                Debug.Assert(outputfile.Position == lStart, "");

                // outputfile.Seek(lStart, SeekOrigin.Begin);   // 文件大了以后这句话的性能会很差
                outputfile.LockingWrite(data, 0, 8);

                // 跳到记录末尾位置
                outputfile.Seek(lTotalLength, SeekOrigin.Current);
                bDone = true;
            }
            finally
            {
                if (bDone == false)
                {
                    outputfile.SetLength(lStart);	// 把本次追加写入的全部去掉
                    outputfile.Seek(0, SeekOrigin.End); // 把文件指针恢复到文件末尾位置，便于下次调用继续写入
                }
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 下载资源，保存到备份文件
        static int WriteResToBackupFile(
            IWin32Window owner,
            RmsChannel channel,
            DigitalPlatform.Stop stop,
            Stream outputfile,
            string strXmlRecPath,
            string[] res_ids,
            out string strError)
        {
            strError = "";

            long lRet;

            for (int i = 0; i < res_ids.Length; i++)
            {
                if (owner != null)
                    Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    if (owner != null)
                    {
                        DialogResult result = MessageBox.Show(owner,
                            "确实要中断当前批处理操作?",
                            "导出数据",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Yes)
                        {
                            strError = "用户中断";
                            return -1;
                        }
                        else
                        {
                            stop.Continue();
                        }
                    }
                    else
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                string strID = res_ids[i].Trim();

                if (string.IsNullOrEmpty(strID))
                    continue;

                string strResPath = strXmlRecPath + "/object/" + strID;

                string strMetaData;

                if (stop != null)
                    stop.SetMessage("正在下载 " + strResPath);

                byte[] baOutputTimeStamp = null;
                string strOutputPath;

            REDO_GETRES:
                lRet = channel.GetRes(strResPath,
                    (Stream)null,	// 故意不获取资源体
                    stop,
                    "metadata,timestamp,outputpath",
                    null,
                    out strMetaData,	// 但是要获得metadata
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.OriginErrorCode == rmsws_localhost.ErrorCodeValue.NotFoundObjectFile)
                        continue;   // TODO: 返回警告信息

                    if (owner != null)
                    {
                        // TODO: 允许重试
                        DialogResult redo_result = MessageBox.Show(owner,
                            "获取记录 '" + strResPath + "' 时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)",
                            "导出数据",
                            MessageBoxButtons.RetryCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (redo_result == DialogResult.Cancel)
                            return -1;
                        goto
                            REDO_GETRES;
                    }
                    else
                        return -1;
                }

                long lResStart = 0;
                // 写res的头。
                // 如果不能预先确知整个res的长度，可以用随便一个lTotalLength值调用本函数，
                // 但是需要记忆下函数所返回的lStart，最后调用EndWriteResToBackupFile()。
                // 如果能预先确知整个res的长度，则最后不必调用EndWriteResToBackupFile()
                long lRet0 = Backup.BeginWriteResToBackupFile(
                    outputfile,
                    0,	// 未知
                    out lResStart);

                byte[] timestamp = baOutputTimeStamp;

                ResPath respath = new ResPath();
                respath.Url = channel.Url;
                respath.Path = strOutputPath;	// strResPath;

                // strMetaData还要加入资源id?
                StringUtil.ChangeMetaData(ref strMetaData,
                    strID,
                    null,
                    null,
                    null,
                    respath.FullPath,
                    ByteArray.GetHexTimeStampString(baOutputTimeStamp));

                lRet = Backup.WriteResMetadataToBackupFile(outputfile,
                    strMetaData);
                if (lRet == -1)
                    return -1;

                long lBodyStart = 0;
                // 写res body的头。
                // 如果不能预先确知body的长度，可以用随便一个lBodyLength值调用本函数，
                // 但是需要记忆下函数所返回的lBodyStart，最后调用EndWriteResBodyToBackupFile()。
                // 如果能预先确知body的长度，则最后不必调用EndWriteResBodyToBackupFile()
                lRet = Backup.BeginWriteResBodyToBackupFile(
                    outputfile,
                    0, // 未知
                    out lBodyStart);
                if (lRet == -1)
                    return -1;

                if (stop != null)
                    stop.SetMessage("正在下载 " + strResPath + " 的数据体");

            REDO_GETRES_1:
                lRet = channel.GetRes(strResPath,
                    outputfile,
                    stop,
                    "content,data,timestamp", //"content,data,timestamp"
                    timestamp,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.EmptyRecord
                        || channel.ErrorCode == ChannelErrorCode.NotFoundObjectFile)    // 2017/7/13
                    {
                        // 空记录
                    }
                    else
                    {
                        if (owner != null)
                        {
                            // TODO: 允许重试
                            DialogResult redo_result = MessageBox.Show(owner,
                                "获取记录 '" + strResPath + "' 时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)",
                                "导出数据",
                                MessageBoxButtons.RetryCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (redo_result == DialogResult.Cancel)
                                return -1;
                            goto
                                REDO_GETRES_1;
                        }
                        else
                            return -1;
                    }
                }

                long lBodyLength = outputfile.Position - lBodyStart - 8;
                // res body收尾
                lRet = Backup.EndWriteResBodyToBackupFile(
                    outputfile,
                    lBodyLength,
                    lBodyStart);
                if (lRet == -1)
                    return -1;

                long lTotalLength = outputfile.Position - lResStart - 8;
                lRet = Backup.EndWriteResToBackupFile(
                    outputfile,
                    lTotalLength,
                    lResStart);
                if (lRet == -1)
                    return -1;

            }

            return 0;
        }

        #endregion
    }


}
