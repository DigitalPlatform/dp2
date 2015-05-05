using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Drawing;

using DigitalPlatform.Range;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;

using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.rms.Client
{
    public class ImportUtil
    {
        IWin32Window m_owner = null;
        public ApplicationInfo AppInfo = null;

        public string FileName = "";
        public ExportFileType FileType = ExportFileType.XmlFile;

        public string MarcSyntax = "";
        public Encoding Encoding = Encoding.UTF8;   // ISO2709文件的 encoding。其他类型的文件暂时不设置这个属性

        public Stream Stream = null;
        XmlTextReader reader = null;

        public int Index = 0;

        // 准备输入
        // return:
        //      -1  出错
        //      0   正常
        //      1   用户放弃
        public int Begin(
            IWin32Window owner,
            ApplicationInfo appInfo,
            string strInputFileName,
            out string strError)
        {
            strError = "";

            this.m_owner = owner;
            this.AppInfo = appInfo;
            this.Index = -1;

            if (string.IsNullOrEmpty(strInputFileName) == true)
            {
                strError = "输入文件名不能为空";
                return -1;
            }

            if (this.Stream != null)
            {
                this.Stream.Close();
                this.Stream = null;
                this.FileName = "";
            }

            string strExt = Path.GetExtension(strInputFileName);

            if (string.Compare(strExt, ".xml", true) == 0)
                this.FileType = ExportFileType.XmlFile;
            else if (string.Compare(strExt, ".dp2bak", true) == 0)
                this.FileType = ExportFileType.BackupFile;
            else if (string.Compare(strExt, ".iso", true) == 0
                || string.Compare(strExt, ".marc", true) == 0
                || string.Compare(strExt, ".mrc", true) == 0)
                this.FileType = ExportFileType.ISO2709File;
            else
            {
                strError = "无法根据文件扩展名 '" + strExt + "' 判断输入文件的格式";
                return -1;
            }

            this.FileName = strInputFileName;


            if (this.FileType == ExportFileType.XmlFile)
            {
                this.Stream = File.Open(strInputFileName,
    FileMode.Open,
    FileAccess.Read);

                this.reader = new XmlTextReader(Stream);

                bool bRet = false;

                // 移动到根元素
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

                // 移动到其下级第一个element
                while (true)
                {
                    bRet = reader.Read();
                    if (bRet == false)
                    {
                        strError = "没有第一个记录元素";
                        return -1;
                    }
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                }
            }
            else if (this.FileType == ExportFileType.BackupFile)
            {
                this.Stream = File.Open(strInputFileName,
    FileMode.Open,
    FileAccess.Read);
            }
            // ISO2709文件需要预先准备条件
            else if (this.FileType == ExportFileType.ISO2709File)
            {
                // 询问encoding和marcsyntax
                OpenMarcFileDlg dlg = new OpenMarcFileDlg();
                Font font = GuiUtil.GetDefaultFont();
                if (font != null)
                    dlg.Font = font;

                dlg.Text = "请指定要导入的 ISO2709 文件属性";
                dlg.FileName = strInputFileName;

                if (this.AppInfo != null)
                    this.AppInfo.LinkFormState(dlg, "restree_OpenMarcFileDlg_input_state");
                dlg.ShowDialog(this.m_owner);
                if (this.AppInfo != null)
                    this.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult != DialogResult.OK)
                {
                    strError = "用户取消";
                    return 1;
                }

                this.FileName = dlg.FileName;

                this.Stream = File.Open(this.FileName,
    FileMode.Open,
    FileAccess.Read);

                this.MarcSyntax = dlg.MarcSyntax;
                this.Encoding = dlg.Encoding;
            }

            return 0;
        }

        public void End()
        {
            if (this.FileType == ExportFileType.XmlFile)
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }

            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }
        }

        // 读入一条记录
        // return:
        //		-1	出错
        //		0	正常
        //		1	结束。此次API不返回有效的记录
        public int ReadOneRecord(out UploadRecord record,
            out string strError)
        {
            strError = "";
            record = null;
            int nRet = 0;

            this.Index++;

            if (this.FileType == ExportFileType.XmlFile)
            {
                string strXml = "";
                string strPath = "";
                string strTimestamp = "";

                // 读入一条XML记录
                // return:
                //		-1	出错
                //		0	正常
                //		1	结束。此次API不返回有效的XML记录
                nRet = ReadOneXmlRecord(out strXml,
                    out strPath,
                    out strTimestamp);
                if (nRet == -1)
                {
                    strError = "ReadOneXmlRecord() 出错";
                    return -1;
                }

                if (nRet == 1)
                    return 1;

                Debug.Assert(nRet == 0, "");

                ResPath respath = new ResPath(strPath);

                record = new UploadRecord();
                record.Url = respath.Url;
                record.RecordBody = new RecordBody();
                record.RecordBody.Xml = strXml;
                record.RecordBody.Path = respath.Path;  // 数据库名/ID
                record.RecordBody.Timestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
                return 0;
            }
            else if (this.FileType == ExportFileType.BackupFile)
            {
                List<OneRes> reslist = null;
                // 从 .dp2bak 文件中读出每个资源的主要信息
                // 本函数调用前，文件指针在整个记录的开始位置
                // return:
                //		-1	出错
                //		0	正常
                //		1	结束。此次API不返回有效的XML记录
                nRet = ReadResFrameInfo(
            out reslist,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return 1;

                if (reslist == null || reslist.Count == 0)
                {
                    strError = "二进制记录内没有包含任何资源";
                    return -1;
                }

                // 第一个资源
                Debug.Assert(reslist.Count > 0, "");

                OneRes first = reslist[0];
                record = new UploadRecord();
                record.ResList = reslist;

                ResPath respath = new ResPath(first.Path);
                record.Url = respath.Url;
                record.RecordBody = new RecordBody();
                record.RecordBody.Xml = "";
                record.RecordBody.Path = respath.Path;  // 数据库名/ID
                record.RecordBody.Timestamp = first.Timestamp;

                // 如果第一个资源的尺寸不是太大，还要取得 XML 字符串
                if (first.Length < 500 * 1024)
                {
                    long lSave = this.Stream.Position;
                    try
                    {
                        this.Stream.Seek(first.StartOffs, SeekOrigin.Begin);
                        byte[] baContent = new byte[(int)first.Length];
                        int nCount = this.Stream.Read(baContent, 0, baContent.Length);
                        if (nCount < baContent.Length)
                        {
                            strError = "读取XML字符串时超过文件尾部";
                            return -1;
                        }
                        // 转换成字符串
                        record.RecordBody.Xml = ByteArray.ToString(baContent);
                    }
                    finally
                    {
                        this.Stream.Seek(lSave, SeekOrigin.Begin);
                    }
                }

                return 0;
            }
            else if (this.FileType == ExportFileType.ISO2709File)
            {
                string strMARC = "";

                // 从ISO2709文件中读入一条MARC记录
                // return:
                //	-2	MARC格式错
                //	-1	出错
                //	0	正确
                //	1	结束(当前返回的记录有效)
                //	2	结束(当前返回的记录无效)
                nRet = MarcUtil.ReadMarcRecord(this.Stream,
                    this.Encoding,
                    true,	// bRemoveEndCrLf,
                    true,	// bForce,
                    out strMARC,
                    out strError);
                if (nRet == -2 || nRet == -1)
                {
                    strError = "读入MARC记录时出错: " + strError;
                    return -1;
                }
                if (nRet == 2)
                    return 1;

#if NO
                // 测试
                if (this.Index >= 50000)
                    return 1;
#endif

                Debug.Assert(nRet == 0 || nRet == 1, "");


                string strXml = "";

                // 将MARC记录转换为xml格式
                nRet = MarcUtil.Marc2Xml(strMARC,
                    this.MarcSyntax,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                record = new UploadRecord();
                record.RecordBody = new RecordBody();
                record.RecordBody.Xml = strXml;
                record.RecordBody.Path = "";
                record.RecordBody.Timestamp = null;

                return 0;
            }

            return 0;
        }

        // 将 XML 记录成批写入数据库
        // return:
        //      -1  出错
        //      >=0 本次已经写入的记录个数。本函数返回时 records 集合的元素数没有变化(但元素的Path和Timestamp会有变化)，如果必要调主可截取records集合中后面未处理的部分再次调用本函数
        public static int WriteRecords(
            IWin32Window owner,
            Stop stop,
            RmsChannel channel,
            bool bQuickMode,
            List<UploadRecord> records,
            // List<RecordBody> records,
            ref bool bDontPromptTimestampMismatchWhenOverwrite,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            int nProcessCount = 0;

            // TODO: 需要检查每个 Path 的服务器URL是完全一样的

            // 提交
            RecordBody[] inputs = new RecordBody[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                UploadRecord record = records[i];
                inputs[i] = record.RecordBody;
                Debug.Assert(record.RecordBody != null, "");
            }

        REDO:
            RecordBody[] results = null;
            long lRet = channel.DoWriteRecords(stop,
                inputs,
                bQuickMode == true ? "fastmode" : "", // strStyle,
                out results,
                out strError);
            if (lRet == -1)
                return -1;
            if (results == null)
            {
                strError = "results == null";
                return -1;
            }
            // TODO: 如何面对 Server 的 Quota Exceed 报错?
            // 出错后改用只提交一个记录的策略？或者更换为传统的保存方式?

            Debug.Assert(results.Length <= inputs.Length);

            List<RecordBody> redo_write_list = new List<RecordBody>();    // 需要重做写入的元素
            List<RecordBody> redo_timestamp_list = new List<RecordBody>();    // 因为时间戳不匹配，需要重做写入的元素
            // 询问时间戳不匹配等报错
            string strMessageError = "";
            string strMessageTimestamp = "";
            for (int i = 0; i < results.Length; i++)
            {
                RecordBody result = results[i];
                if (result.Result != null && result.Result.ErrorCode != ErrorCodeValue.NoError)
                {
                    if (result.Result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        strMessageTimestamp += "记录 " + result.Path + " 在覆盖保存过程中出错: " + result.Result.ErrorString + "\r\n";
                        redo_timestamp_list.Add(inputs[i]);
                    }
                    else
                    {
                        strMessageError += "记录 " + result.Path + " 保存过程中出错: " + result.Result.ErrorString + "\r\n";
                        redo_write_list.Add(inputs[i]);
                    }
                }
                else
                {
                    nProcessCount++;
                }
            }

            if (string.IsNullOrEmpty(strMessageTimestamp) == false)
            {
                if (bDontPromptTimestampMismatchWhenOverwrite == false)
                {
                    DialogResult result = MessageDlg.Show(owner,
                        strMessageTimestamp + "\r\n---\r\n\r\n是否重试以新时间戳强行覆盖保存?\r\n\r\n注：\r\n[重试] 重试强行覆盖\r\n[跳过] 忽略当前记录或资源保存，但继续后面的处理\r\n[中断] 中断整个批处理",
                        "导入数据",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxDefaultButton.Button1,
                        ref bDontPromptTimestampMismatchWhenOverwrite,
                        new string[] { "重试", "跳过", "中断" });
                    if (result == DialogResult.Cancel)
                    {
                        strError = strMessageTimestamp;
                        return -1;
                    }

                    if (result == DialogResult.No)
                    {
                        // 跳过
                        redo_timestamp_list.Clear();
                        if (redo_timestamp_list.Count + redo_write_list.Count == 0)
                            return results.Length;
                    }
                }
            }

            if (string.IsNullOrEmpty(strMessageError) == false)
            {
                bool bDontAsk = false;
                DialogResult result = MessageDlg.Show(owner,
                    strMessageError + "\r\n---\r\n\r\n是否重试保存?\r\n\r\n注：\r\n[重试] 重试保存\r\n[跳过] 忽略当前记录或资源保存，但继续后面的处理\r\n[中断] 中断整个批处理",
                    "导入数据",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxDefaultButton.Button1,
                    ref bDontAsk,
                    new string[] { "重试", "跳过", "中断" });
                if (result == DialogResult.Cancel)
                {
                    strError = strMessageError;
                    return -1;
                }

                if (result == DialogResult.No)
                {
                    // 跳过
                    redo_write_list.Clear();
                    if (redo_timestamp_list.Count + redo_write_list.Count == 0)
                        return results.Length;
                }
            }

            // 
            for (int i = 0; i < results.Length; i++)
            {
                RecordBody result = results[i];
                RecordBody record = inputs[i];
                record.Path = result.Path;  // 实际写入的路径
                record.Timestamp = result.Timestamp;    // 新的时间戳
            }

            if (redo_write_list.Count + redo_timestamp_list.Count > 0)
            {
                inputs = new RecordBody[redo_write_list.Count + redo_timestamp_list.Count];
                redo_write_list.CopyTo(inputs);
                redo_timestamp_list.CopyTo(inputs, redo_write_list.Count);
                goto REDO;
            }

            return nProcessCount;
        }

        // return:
        //      -1  出错
        //      0   成功
        public int UploadObjects(
            Stop stop,
            RmsChannel channel,
            List<UploadRecord> records,
            ref bool bDontPromptTimestampMismatchWhenOverwrite,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            foreach (UploadRecord record in records)
            {
                if (record.ResList == null
                    || record.ResList.Count <= 1)
                    continue;
                int i = 0;
                foreach (OneRes res in record.ResList)
                {
                    if (i == 0)
                    {
                        i++;
                        continue;
                    }

                    // record.RecordBody.Path;  // 注意检查里面不能有问号
                    ResPath temp = new ResPath(res.Path);   // 检查 object id
                    string strID = temp.GetObjectId();

                    string strObjectPath = record.RecordBody.Path + "/object/" + strID;

                    int nRedoCount = 0;
                REDO:
                    // 上载一个res
                    // parameters:
                    //      strRecordPath   主记录的路径
                    //		inputfile:   源流
                    //		bIsFirstRes: 是否是第一个资源(xml)
                    //		strError:    error info
                    // return:
                    //		-2	片断中发现时间戳不匹配。本函数调主可重上载整个资源
                    //		-1	error
                    //		0	successed
                    nRet = UploadOneRes(
                        this.m_owner,
                        stop,
                        channel,
                        ref strObjectPath,
                        this.Stream,
                        res,
                        false,
                        "", //  strCount,
                        ref bDontPromptTimestampMismatchWhenOverwrite,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == -2)
                    {
                        // TODO: 防止死循环
                        nRedoCount++;
                        if (nRedoCount > 3)
                        {
                            return -1;
                        }
                        goto REDO;
                    }

                    i++;
                }
            }

            return 0;
        }

        #region .dp2bak 具体操作

        // 上载一个res
        // parameters:
        //      strRecordPath   主记录的路径
        //		inputfile:   源流
        //		bIsFirstRes: 是否是第一个资源(xml)
        //		strError:    error info
        // return:
        //		-2	片断中发现时间戳不匹配。本函数调主可重上载整个资源
        //		-1	error
        //		0	successed
        public static int UploadOneRes(
            IWin32Window owner,
            Stop stop,
            RmsChannel channel,
            ref string strRecordPath,
            Stream inputfile,
            OneRes res,
            // ref DbNameMap map,
            bool bIsFirstRes,
            string strCount,
            ref bool bDontPromptTimestampMismatchWhenOverwrite,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            if (res.Length == 0)
            {
                Debug.Assert(false, "");
                return 0;	// 空包不需上载
            }

#if NO
                // 2.为上载做准备
                XmlDocument metadataDom = new XmlDocument();
                try
                {
                    metadataDom.LoadXml(res.MetadataXml);
                }
                catch (Exception ex)
                {
                    strError = "加载 metadataxml 到 DOM 时出错: " + ex.Message;
                    goto ERROR1;
                }

                XmlNode node = metadataDom.DocumentElement;

                string strResPath = DomUtil.GetAttr(node, "path");

                string strTargetPath = "";




                // string strLocalPath = DomUtil.GetAttr(node,"localpath");
                // string strMimeType = DomUtil.GetAttr(node,"mimetype");
                string strTimeStamp = DomUtil.GetAttr(node, "timestamp");
                // 注意,strLocalPath并不是要上载的body文件,它只用来作元数据\
                // body文件为strBodyTempFileName
#endif

            // string strTargetPath = strRecordPath;

            // 3.将body文件拆分成片断进行上载
            string[] ranges = null;

            if (res.Length == 0)
            {
                // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = "0-" + Convert.ToString(res.Length - 1);

                // 按照100K作为一个chunk
                ranges = RangeList.ChunkRange(strRange,
                    100 * 1024);
            }

            byte[] timestamp = res.Timestamp;
            byte[] output_timestamp = null;

        REDOWHOLESAVE:
            string strOutputPath = "";
            string strWarning = "";

            for (int j = 0; j < ranges.Length; j++)
            {
            REDOSINGLESAVE:

                Application.DoEvents();	// 出让界面控制权

                if (stop.State != 0)
                {
                    DialogResult result = MessageBox.Show(owner,
                        "确实要中断当前批处理操作?",
                        "导入数据",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }
                    else
                    {
                        stop.Continue();
                    }
                }


                string strWaiting = "";
                if (j == ranges.Length - 1)
                    strWaiting = " 请耐心等待...";

                string strPercent = "";
                RangeList rl = new RangeList(ranges[j]);
                if (rl.Count >= 1)
                {
                    double ratio = (double)((RangeItem)rl[0]).lStart / (double)res.Length;
                    strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                }

                if (stop != null)
                    stop.SetMessage("正在保存 " + ranges[j] + "/"
                        + Convert.ToString(res.Length)
                        + " " + strPercent + " " + strRecordPath + strWarning + strWaiting + " " + strCount);


                inputfile.Seek(res.StartOffs, SeekOrigin.Begin);

                long lRet = channel.DoSaveResObject(strRecordPath,
                    inputfile,
                    res.Length,
                    "",	// style
                    res.MetadataXml,
                    ranges[j],
                    j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);

                // stop.SetProgressValue(inputfile.Position);

                strWarning = "";

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        string strDisplayRecPath = strOutputPath;
                        if (string.IsNullOrEmpty(strDisplayRecPath) == true)
                            strDisplayRecPath = strRecordPath;

                        if (bDontPromptTimestampMismatchWhenOverwrite == true)
                        {
                            timestamp = new byte[output_timestamp.Length];
                            Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                            strWarning = " (时间戳不匹配, 自动重试)";
                            if (ranges.Length == 1 || j == 0)
                                goto REDOSINGLESAVE;
                            goto REDOWHOLESAVE;
                        }


                        DialogResult result = MessageDlg.Show(owner,
                            "保存 '" + strDisplayRecPath + "' (片断:" + ranges[j] + "/总尺寸:" + Convert.ToString(res.Length)
                            + ") 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
                            + strError + "\r\n---\r\n\r\n是否以新时间戳强行覆盖保存?\r\n注：\r\n[是] 强行覆盖保存\r\n[否] 忽略当前记录或资源保存，但继续后面的处理\r\n[取消] 中断整个批处理",
                            "导入数据",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxDefaultButton.Button1,
                            ref bDontPromptTimestampMismatchWhenOverwrite);
                        if (result == DialogResult.Yes)
                        {

                            if (output_timestamp != null)
                            {
                                timestamp = new byte[output_timestamp.Length];
                                Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                            }
                            else
                            {
                                timestamp = output_timestamp;
                            }
                            strWarning = " (时间戳不匹配, 应用户要求重试)";
                            if (ranges.Length == 1 || j == 0)
                                goto REDOSINGLESAVE;
                            goto REDOWHOLESAVE;
                        }

                        if (result == DialogResult.No)
                        {
                            return 0;	// 继续作后面的资源
                        }

                        if (result == DialogResult.Cancel)
                        {
                            strError = "用户中断";
                            goto ERROR1;	// 中断整个处理
                        }
                    }


                    goto ERROR1;
                }

                timestamp = output_timestamp;
            }

            // 考虑到保存第一个资源的时候，id可能为“?”，因此需要得到实际的id值
            if (bIsFirstRes)
                strRecordPath = strOutputPath;

            return 0;

        ERROR1:
            return -1;
        }


        // 从 .dp2bak 文件中读出每个资源的主要信息
        // 本函数调用前，文件指针在整个记录的开始位置
        // return:
        //		-1	出错
        //		0	正常
        //		1	结束。此次API不返回有效的XML记录
        int ReadResFrameInfo(
            out List<OneRes> reslist,
            out string strError)
        {
            strError = "";
            reslist = new List<OneRes>();

            long lStart = this.Stream.Position;

            byte[] data = new byte[8];
            int nRet = this.Stream.Read(data, 0, 8);
            if (nRet == 0)
                return 1;	// 已经结束
            if (nRet < 8)
            {
                strError = "read whole length error...";
                return -1;
            }

            // 毛长度
            long lLength = BitConverter.ToInt64(data, 0);

#if NO
                if (bSkip == true)
                {
                    file.Seek(lLength, SeekOrigin.Current);
                    return 0;
                }
#endif

            for (int i = 0; ; i++)
            {
                if (this.Stream.Position - lStart >= lLength + 8)
                    break;


                long lBodyStart = 0;
                long lBodyLength = 0;

                // 1. 从输入流中得到strMetadata,与body(body放到一个临时文件里)
                string strMetaDataXml = "";

                nRet = GetResInfo(this.Stream,
                    i == 0 ? true : false,
                    out strMetaDataXml,
                    out lBodyStart,
                    out lBodyLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (lBodyLength == 0)
                    continue;	// 空包不需上载

                OneRes res = new OneRes();
                res.MetadataXml = strMetaDataXml;
                res.StartOffs = lBodyStart;
                res.Length = lBodyLength;
                reslist.Add(res);

                // 取出原始记录路径
                XmlDocument metadataDom = new XmlDocument();
                try
                {
                    metadataDom.LoadXml(strMetaDataXml);
                }
                catch (Exception ex)
                {
                    strError = "加载 metadataxml 到 DOM 时出错:" + ex.Message;
                    return -1;
                }

                res.Path = DomUtil.GetAttr(metadataDom.DocumentElement, "path");
                string strTimeStamp = DomUtil.GetAttr(metadataDom.DocumentElement, "timestamp");
                res.Timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
            }

            return 0;
        }

        // 从输入流中得到一个res的metadata和body
        // parameter:
        //		inputfile:       源流
        //		bIsFirstRes:     是否是第一个资源
        //		strMetaDataXml:  返回metadata内容
        //		strError:        error info
        // return:
        //		-1: error
        //		0:  successed
        public static int GetResInfo(Stream inputfile,
            bool bIsFirstRes,
            out string strMetaDataXml,
            out long lBodyStart,
            out long lBodyLength,
            out string strError)
        {
            strMetaDataXml = "";
            strError = "";
            lBodyStart = 0;
            lBodyLength = 0;

            byte[] length = new byte[8];

            // 读入总长度
            int nRet = inputfile.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "读取res总长度部分出错...";
                return -1;
            }

            long lTotalLength = BitConverter.ToInt64(length, 0);

            // 读入metadata长度
            nRet = inputfile.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "读取metadata长度部分出错...";
                return -1;
            }

            long lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength >= 100 * 1024)
            {
                strError = "metadata数据长度超过100K，似不是正确格式...";
                return -1;
            }

            byte[] metadata = new byte[(int)lMetaDataLength];
            int nReadLength = inputfile.Read(metadata,
                0,
                (int)lMetaDataLength);
            if (nReadLength < (int)lMetaDataLength)
            {
                strError = "metadata声明的长度超过文件末尾，格式错误";
                return -1;
            }

            strMetaDataXml = Encoding.UTF8.GetString(metadata);	// ? 是否可能抛出异常

            // 读body部分的长度
            nRet = inputfile.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "读取body长度部分出错...";
                return -1;
            }

            lBodyStart = inputfile.Position;

            lBodyLength = BitConverter.ToInt64(length, 0);
            if (bIsFirstRes == true && lBodyLength >= 2000 * 1024)
            {
                strError = "第一个res中body的xml数据长度超过2000K，似不是正确格式...";
                return -1;
            }

            // 将文件指针移动到末尾
            inputfile.Seek(lBodyLength, SeekOrigin.Current);

            return 0;
        }

        #endregion

        // 读入一条XML记录
        // return:
        //		-1	出错
        //		0	正常
        //		1	结束。此次API不返回有效的XML记录
        public int ReadOneXmlRecord(out string strXml,
            out string strPath,
            out string strTimestamp)
        {
            strXml = "";
            strPath = "";
            strTimestamp = "";

            while (true)
            {
                if (reader.NodeType == XmlNodeType.Element)
                    break;
                bool bRet = reader.Read();
                if (bRet == false)
                    return 1;
            }

            // 直接读出记录路径和时间戳

            // reader.ReadAttributeValue();

            if (reader.HasAttributes == true)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    if (reader.NamespaceURI == DpNs.dprms)
                    {
                        if (reader.LocalName == "path")
                            strPath = reader.Value;
                        else if (reader.LocalName == "timestamp")
                            strTimestamp = reader.Value;
                    }
                }
                reader.MoveToElement();
            }

            strXml = reader.ReadOuterXml();
            return 0;
        }

        // 根据原始路径准备即将写入的路径
        // parameters:
        //      strSelectedLongPath 用户选定的默认的目标库长路径。将优先出现在询问对话框的目标中
        // return:
        //      -1  出错
        //      0   用户放弃
        //      1   成功
        //      2   要跳过本条
        public static int PrepareOverwritePath(
            IWin32Window owner,
            ServerCollection Servers,
            RmsChannelCollection Channels,
            ApplicationInfo AppInfo,
            long lIndex,
            string strSelectedLongPath,
            ref DbNameMap map,
            ref string strLongPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ResPath respath = new ResPath(strLongPath);
            respath.MakeDbName();
            string strSourceDbPath = respath.FullPath;

        REDO:

            DbNameMapItem mapItem = null;

            mapItem = map.MatchItem(strSourceDbPath);
            if (mapItem != null)
                goto MAPITEMOK;

            if (mapItem == null)
            {

                if (string.IsNullOrEmpty(strSourceDbPath) == true)
                {
                    string strText = "源数据文件中记录 " + lIndex.ToString() + " 没有来源数据库。\r\n请问对所有这样的数据，将作如何处理?";
                    // WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    nRet = DbNameMapItemDlg.AskNullOriginBox(
                        owner,
                        AppInfo,
                        Servers,
                        Channels,
                        strText,
                        strSelectedLongPath,
                        map);
                    // WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");

                    if (nRet == 0)
                    {
                        strError = "用户中断";
                        return 0;	// 中断整个处理
                    }

                    goto REDO;

                }
                else
                {
                    string strText = "源数据文件中记录 " + lIndex.ToString() + " 的来源数据库 '" + strSourceDbPath + "' 没有明确的对应规则。\r\n请问对所有这样的数据，将作如何处理?";    // 没有找到对应的目标库
                    // WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    nRet = DbNameMapItemDlg.AskNotMatchOriginBox(
                        owner,
                        AppInfo,
                        Servers,
                        Channels,
                        strText,
                        strSelectedLongPath,
                        strSourceDbPath/*strResPath*/,
                        map);
                    // WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (nRet == 0)
                    {
                        strError = "用户中断";
                        return 0;	// 中断整个处理
                    }

                    goto REDO;
                }
            }

        MAPITEMOK:

            if (mapItem.Style == "skip")
                return 2;

            // 构造目标路径

            // 1)从源路径中提取id。源路径来自备份文件数据
            respath = new ResPath(strLongPath);
            string strID = respath.GetRecordId();

            if (string.IsNullOrEmpty(strID) == true
                || mapItem.Style == "append")
            {
                strID = "?";	// 将来加一个对话框
            }

            // 2)用目标库路径构造完整的记录路径
            string strTargetFullPath = "";
            if (mapItem.Target == "*")
            {
                respath = new ResPath(strLongPath);
                respath.MakeDbName();
                strTargetFullPath = respath.FullPath;
            }
            else
            {
                strTargetFullPath = mapItem.Target;
            }

            respath = new ResPath(strTargetFullPath);
            respath.Path = respath.Path + "/" + strID;
            strLongPath = respath.FullPath;

            return 1;
        }

#if NO
            // 根据原始路径准备即将写入的路径
            // return:
            //      -1  出错
            //      0   用户放弃
            //      1   成功
            public static int PrepareOverwritePath(
                ServerCollection Servers,
		        RmsChannelCollection Channels,
                IWin32Window owner,
                ref DbNameMap map,
                ref string strLongPath,
                out string strError)
			{
                strError = "";

				// 从map中查询覆盖还是追加？
                ResPath respath = new ResPath(strLongPath);
				respath.MakeDbName();

			REDO:
				DbNameMapItem mapItem = (DbNameMapItem)map["*"];
				if (mapItem != null)
				{
				}
				else 
				{
					mapItem = (DbNameMapItem)map[respath.FullPath.ToUpper()];
				}

				if (mapItem == null) 
				{
					OriginNotFoundDlg dlg = new OriginNotFoundDlg();
                    Font font = GuiUtil.GetDefaultFont();
                    if (font != null)
                        dlg.Font = font;

					dlg.Message = "数据中声明的数据库路径 '" +respath.FullPath+ "' 在覆盖关系对照表中没有找到, 请选择覆盖方式: " ;
					dlg.Origin = respath.FullPath.ToUpper();
					dlg.Servers = Servers;
					dlg.Channels = Channels;
					dlg.Map = map;

                    dlg.StartPosition = FormStartPosition.CenterScreen;
					dlg.ShowDialog(owner);

					if (dlg.DialogResult != DialogResult.OK) 
					{
						strError = "用户中断...";
						return 0;
					}

					map = dlg.Map;
					goto REDO;
				}

				if (mapItem.Style == "skip")
					return 0;

				// 构造目标路径

				// 1)从源路径中提取id。源路径来自备份文件数据
                respath = new ResPath(strLongPath);
				string strID = respath.GetRecordId();

				if (string.IsNullOrEmpty(strID) == true
					|| mapItem.Style == "append")
				{
					strID = "?";	// 将来加一个对话框
				}

				// 2)用目标库路径构造完整的记录路径
				string strTargetFullPath = "";
				if (mapItem.Target == "*") 
				{
                    respath = new ResPath(strLongPath);
					respath.MakeDbName();
					strTargetFullPath = respath.FullPath;
				}
				else 
				{
					strTargetFullPath = mapItem.Target;
				}

				respath = new ResPath(strTargetFullPath);
				respath.Path = respath.Path + "/" + strID;
                strLongPath = respath.FullPath;

                return 1;
            }

#endif

        public void WriteLog(string strText)
        {
        }

        // 写入一条 XML 记录
        // return:
        //      -1  出错
        //      0   邀请中断整个处理
        //      1   成功
        //      2   跳过本条，继续处理后面的
        public static int WriteOneXmlRecord(
            IWin32Window owner,
            Stop stop,
            RmsChannel channel,
            UploadRecord record,
            ref bool bDontPromptTimestampMismatchWhenOverwrite,
            out string strError)
        {
            strError = "";

            string strWarning = "";

            // 询问库名映射关系

            string strTargetPath = record.RecordBody.Path;
            byte[] output_timestamp = null;
            string strOutputPath = "";
        REDOSAVE:
            // 保存Xml记录
            long lRet = channel.DoSaveTextRes(strTargetPath,
                record.RecordBody.Xml,
                false,	// bIncludePreamble
                "", //     bFastMode == true ? "fastmode" : "",//strStyle,
                record.RecordBody.Timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == -1)
            {
                if (stop != null)
                    stop.Continue();

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    string strDisplayRecPath = strOutputPath;
                    if (string.IsNullOrEmpty(strDisplayRecPath) == true)
                        strDisplayRecPath = strTargetPath;

                    if (bDontPromptTimestampMismatchWhenOverwrite == true)
                    {
                        record.RecordBody.Timestamp = output_timestamp;
                        strWarning = " (时间戳不匹配, 自动重试)";
                        // TODO: 如何防止死循环?
                        goto REDOSAVE;
                    }

                    string strText = "保存 '" + strDisplayRecPath
                        + " 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
                        + strError + "\r\n---\r\n\r\n是否以新时间戳强行覆盖保存?\r\n注：\r\n[是] 强行覆盖保存\r\n[否] 忽略当前记录或资源保存，但继续后面的处理\r\n[取消] 中断整个批处理";
                    //WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    DialogResult result = MessageDlg.Show(owner,
                        strText,
                        "dp2batch",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxDefaultButton.Button1,
                        ref bDontPromptTimestampMismatchWhenOverwrite);
                    //WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (result == DialogResult.Yes)
                    {
                        record.RecordBody.Timestamp = output_timestamp;
                        strWarning = " (时间戳不匹配, 应用户要求重试)";
                        goto REDOSAVE;
                    }

                    if (result == DialogResult.No)
                    {
                        return 2;	// 继续作后面的资源
                    }

                    if (result == DialogResult.Cancel)
                    {
                        strError = "用户中断";
                        return 0;	// 中断整个处理
                    }
                }

                // 询问是否重试
                {
                    string strText = "保存 '" + strTargetPath
                        + " 时发生错误。详细情况如下：\r\n---\r\n"
                        + strError + "\r\n---\r\n\r\n是否重试?\r\n注：(是)重试 (否)不重试，但继续后面的处理 (取消)中断整个批处理";
                    //WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");

                    DialogResult result1 = MessageBox.Show(owner,
                        strText,
                        "dp2batch",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    //WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (result1 == DialogResult.Yes)
                        goto REDOSAVE;
                    if (result1 == DialogResult.No)
                        return 2;	// 继续作后面的资源
                }

                return -1;
            }

            return 1;
        }

        // 根据当前需要保存的记录路径，自动准备 Channel
        // 尽量利用以前的 Channel，如果 Url 没有变化的话
        public static RmsChannel GetChannel(
            RmsChannelCollection Channels,
            Stop stop,
            string strUrl,
            RmsChannel cur_channel)
        {
            RmsChannel old_channel = cur_channel;

            // 准备 Channel
            if (cur_channel.Url != strUrl)
            {
                cur_channel.Close();
                cur_channel = Channels.CreateTempChannel(strUrl);

                if (old_channel != null)
                {
                    stop.OnStop -= (sender1, e1) =>
                    {
                        if (old_channel != null)
                            old_channel.Abort();
                    };
                }
                stop.OnStop += (sender1, e1) =>
                {
                    if (cur_channel != null)
                        cur_channel.Abort();
                };
            }

            return cur_channel;
        }
    }

    // 一个用于记录上载的结构
    public class UploadRecord
    {
        public string Url = ""; // dp2Kernel URL
        public RecordBody RecordBody = null;

        public List<OneRes> ResList = null; // 资源集合。第一个资源是主记录XML

        // 主记录是否过大?
        public bool TooLarge()
        {
            if (this.RecordBody != null
                && this.RecordBody.Xml.Length > 200 * 1024)
                return true;

            if (this.ResList != null
                && this.ResList.Count > 0
                && this.ResList[0].Length > 500 * 1024)
                return true;

            return false;
        }
    }

    // 一个独立的资源信息
    public class OneRes
    {
        public string MetadataXml = "";

        public string Path = "";    // 从 metadata 中取出的原始记录路径
        public byte[] Timestamp = null; // 从 metadata 中取出的原始时间戳

        public long StartOffs = -1; // 在二进制文件中的起始位置
        public long Length = 0;     // 在二进制文件中的长度
    }

}
