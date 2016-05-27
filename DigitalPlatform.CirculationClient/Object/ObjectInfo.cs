using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 管理对象资源的内存对象
    /// </summary>
    public class ObjectInfo
    {
        public string ID { get; set; }
        public string Usage { get; set; }
        public string Rights { get; set; }

        public string ErrorInfo { get; set; }

        public string Metadata { get; set; }
        // 以下几项是从 metadata 里面析出的信息
        public string FileName { get; set; }    // 本地文件名
        public string Size { get; set; }
        public string Mime { get; set; }
        public string Timestamp { get; set; }

        // public LineState LineState = LineState.Normal;
    }

    /// <summary>
    /// 对象资源集合
    /// </summary>
    public class ObjectInfoCollection : List<ObjectInfo>
    {
        public string BiblioRecPath { get; set; }
        public bool Changed { get; set; }

        // 装载一条元数据下属的全部对象信息
        // TODO: 可否预先要求 dp2library 在多少以上?
        public int Load(
            LibraryChannel channel,
            Stop stop,
            string strBiblioRecPath,
            string strXml,
            string dp2library_version,
            out string strError)
        {
            strError = "";
            this.Clear();

            strError = "";

            if (String.IsNullOrEmpty(strXml) == true)
            {
                this.Changed = false;
                return 0;
            }

            this.BiblioRecPath = strBiblioRecPath;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML 记录装载到 DOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);

            return LoadObject(
                channel,
                stop,
                nodes,
                dp2library_version,
                out strError);
        }

        // return:
        //      -1  error
        //      0   没有填充任何内容，列表为空
        //      1   已经填充了内容
        public int LoadObject(
            LibraryChannel channel,
            Stop stop,
            XmlNodeList nodes,
            string dp2library_version,
            out string strError)
        {
            strError = "";

            this.Clear();

            List<string> recpaths = new List<string>();
            // 第一阶段，把来自 XML 记录中的 <file> 元素信息填入。
            // 这样就保证了至少可以在保存书目记录阶段能还原 XML 记录中的相关部分
            foreach (XmlElement node in nodes)
            {
                ObjectInfo info = new ObjectInfo();
                info.ID = node.GetAttribute("id");
                info.Usage = node.GetAttribute("usage");
                info.Rights = node.GetAttribute("rights");

                this.Add(info);

                string strResPath = this.BiblioRecPath + "/object/" + info.ID;
                strResPath = strResPath.Replace(":", "/");
                recpaths.Add(strResPath);
            }

            if (StringUtil.CompareVersion(dp2library_version, "2.58") >= 0)
            {
                // 新方法，速度快
                if (stop != null)
                    stop.Initial("正在下载对象的元数据");

                try
                {
                    BrowseLoader loader = new BrowseLoader();
                    loader.Channel = channel;
                    loader.Stop = stop;
                    loader.RecPaths = recpaths;
                    loader.Format = "id,metadata,timestamp";

                    int i = 0;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        Debug.Assert(record.Path == recpaths[i], "");
                        ObjectInfo info = this[i];

                        if (record.RecordBody.Result != null
                            && record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                        {
                            info.ErrorInfo = record.RecordBody.Result.ErrorString;
                            i++;
                            continue;
                        }

                        string strMetadataXml = record.RecordBody.Metadata;

                        info.Metadata = strMetadataXml;

                        byte[] baMetadataTimestamp = record.RecordBody.Timestamp;
                        //Debug.Assert(baMetadataTimestamp != null, "");

                        // 取metadata值
                        Hashtable values = StringUtil.ParseMedaDataXml(strMetadataXml,
                            out strError);
                        if (values == null)
                        {
                            info.ErrorInfo = strError;
                            continue;
                        }

                        // localpath
                        info.FileName = (string)values["localpath"];

                        // size
                        info.Size = (string)values["size"];

                        // mime
                        info.Mime = (string)values["mimetype"];

                        // tiemstamp
                        string strTimestamp = ByteArray.GetHexTimeStampString(baMetadataTimestamp);
                        info.Timestamp = strTimestamp;

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    // TODO: 出现异常后，是否改为用原来的方法一个一个对象地获取 metadata?
                    strError = ex.Message;
                    return -1;
                }
                finally
                {
                    stop.Initial("");
                }
            }
            else
            {
                strError = "请升级 dp2library 到 2.58 以上版本";
                return -1;
            }

            this.Changed = false;

            if (this.Count > 0)
                return 1;

            return 0;
        }


        // 从路径中取出记录号部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

        // 是否为新增记录的路径
        public static bool IsNewPath(string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return true;    //???? 空路径当作新路径?

            string strID = GetRecordID(strPath);

            if (strID == "?"
                || String.IsNullOrEmpty(strID) == true) // 2008/11/28 
                return true;

            return false;
        }

#if NO
        // 保存资源到服务器
        // return:
        //		-1	error
        //		>=0 实际上载的资源对象数
        public int Save(
            LibraryChannel channel,
            Stop stop,
            string dp2library_version,
            out string strError)
        {
            strError = "";

            if (this.Count == 0)
                return 0;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未指定 BiblioRecPath";
                return -1;
            }

            if (IsNewPath(this.BiblioRecPath) == true)
            {
                strError = "书目记录路径 '" + this.BiblioRecPath + "' 不是已保存的记录路径，无法用于对象资源上载";
                return -1;
            }

            StopStyle old_stop_style = StopStyle.None;

            if (stop != null)
            {
                old_stop_style = stop.Style;
                stop.Style = StopStyle.EnableHalfStop;

                stop.Initial("正在上载资源 ...");
            }

            int nUploadCount = 0;   // 实际上载的资源个数

            try
            {
                foreach (ObjectInfo obj in this)
                {
                    // LineInfo info = (LineInfo)item.Tag;

                    LineState state = obj.LineState;
                    bool bOnlyChangeMetadata = false;
                    if (state == LineState.Changed ||
                        state == LineState.New)
                    {
                        if (state == LineState.Changed)
                        {
                            if (obj != null
                                && obj.ResChanged == false)
                            {
                                if (StringUtil.CompareVersion(dp2library_version, "2.59") < 0)
                                {
                                    strError = "单独修改对象 metadata 的操作需要连接的 dp2library 版本在 2.59 以上 (然而当前 dp2library 版本为 " + dp2library_version + ")";
                                    return -1;
                                }
                                // 这种情况应该是 metadata 修改过
                                bOnlyChangeMetadata = true;
#if NO
                                SetLineInfo(item,
                                    // strUsage, 
                                    LineState.Normal);
                                SetXmlChanged(item, false);
                                SetResChanged(item, false);
                                continue;   // 资源没有修改的，则跳过上载
#endif
                            }
                        }
                    }
                    else
                    {
                        // 标记删除的事项，只要书目XML重新构造的时候
                        // 不包含其ID，书目XML保存后，就等于删除了该事项。
                        // 所以本函数只是简单Remove这样的listview事项即可
                        if (state == LineState.Deleted)
                        {
                            this.ListView.Items.Remove(item);
                            i--;
                        }

                        continue;
                    }

                    string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);

                    string strID = ListViewUtil.GetItemText(item, COLUMN_ID);
                    string strResPath = this.BiblioRecPath + "/object/" + ListViewUtil.GetItemText(item, COLUMN_ID);
                    string strLocalFilename = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);
                    string strMime = ListViewUtil.GetItemText(item, COLUMN_MIME);
                    string strTimestamp = ListViewUtil.GetItemText(item, COLUMN_TIMESTAMP);

                    byte[] timestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
                    byte[] output_timestamp = null;

                    nUploadCount++;

                    if (bOnlyChangeMetadata)
                    {
                        long lRet = channel.SaveResObject(
    Stop,
    strResPath,
    "",
    strLocalFilename,
    strMime,
    "", // range
    true,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
    timestamp,
    out output_timestamp,
    out strError);
                        timestamp = output_timestamp;
                        if (timestamp != null)
                            ListViewUtil.ChangeItemText(item,
                                COLUMN_TIMESTAMP,
                                ByteArray.GetHexTimeStampString(timestamp));
                        if (lRet == -1)
                            goto ERROR1;
                        Debug.Assert(timestamp != null, "");
                        // TODO: 出错的情况下是否要修改 timestamp 显示？是否应为非空才兑现显示
                    }
                    else
                    {
                        // 检测文件尺寸
                        FileInfo fi = new FileInfo(strLocalFilename);

                        if (fi.Exists == false)
                        {
                            strError = "文件 '" + strLocalFilename + "' 不存在...";
                            return -1;
                        }

                        string[] ranges = null;

                        if (fi.Length == 0)
                        {
                            // 空文件
                            ranges = new string[1];
                            ranges[0] = "";
                        }
                        else
                        {
                            string strRange = "";
                            strRange = "0-" + Convert.ToString(fi.Length - 1);

                            // 按照100K作为一个chunk
                            // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                            ranges = RangeList.ChunkRange(strRange,
                                500 * 1024);
                        }

                        // REDOWHOLESAVE:
                        string strWarning = "";

                        for (int j = 0; j < ranges.Length; j++)
                        {
                            // REDOSINGLESAVE:

                            Application.DoEvents();	// 出让界面控制权

                            if (Stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }

                            string strWaiting = "";
                            if (j == ranges.Length - 1)
                                strWaiting = " 请耐心等待...";

                            string strPercent = "";
                            RangeList rl = new RangeList(ranges[j]);
                            if (rl.Count >= 1)
                            {
                                double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                                strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                            }

                            if (Stop != null)
                                Stop.SetMessage("正在上载 " + ranges[j] + "/"
                                    + Convert.ToString(fi.Length)
                                    + " " + strPercent + " " + strLocalFilename + strWarning + strWaiting);

                            long lRet = 0;
                            TimeSpan old_timeout = channel.Timeout;
                            channel.Timeout = new TimeSpan(0, 5, 0);
                            try
                            {
                                lRet = channel.SaveResObject(
                                    Stop,
                                    strResPath,
                                    strLocalFilename,
                                    strLocalFilename,
                                    strMime,
                                    ranges[j],
                                    j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                                    timestamp,
                                    out output_timestamp,
                                    out strError);
                            }
                            finally
                            {
                                channel.Timeout = old_timeout;
                            }
                            timestamp = output_timestamp;

                            if (timestamp != null)
                                ListViewUtil.ChangeItemText(item,
                                COLUMN_TIMESTAMP,
                                ByteArray.GetHexTimeStampString(timestamp));

                            strWarning = "";

                            if (lRet == -1)
                            {
                                /*
                                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                                {

                                    if (this.bNotAskTimestampMismatchWhenOverwrite == true)
                                    {
                                        timestamp = new byte[output_timestamp.Length];
                                        Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                                        strWarning = " (时间戳不匹配, 自动重试)";
                                        if (ranges.Length == 1 || j == 0)
                                            goto REDOSINGLESAVE;
                                        goto REDOWHOLESAVE;
                                    }


                                    DialogResult result = MessageDlg.Show(this,
                                        "上载 '" + strLocalFilename + "' (片断:" + ranges[j] + "/总尺寸:" + Convert.ToString(fi.Length)
                                        + ") 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
                                        + strError + "\r\n---\r\n\r\n是否以新时间戳强行上载?\r\n注：(是)强行上载 (否)忽略当前记录或资源上载，但继续后面的处理 (取消)中断整个批处理",
                                        "dp2batch",
                                        MessageBoxButtons.YesNoCancel,
                                        MessageBoxDefaultButton.Button1,
                                        ref this.bNotAskTimestampMismatchWhenOverwrite);
                                    if (result == DialogResult.Yes)
                                    {
                                        timestamp = new byte[output_timestamp.Length];
                                        Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                                        strWarning = " (时间戳不匹配, 应用户要求重试)";
                                        if (ranges.Length == 1 || j == 0)
                                            goto REDOSINGLESAVE;
                                        goto REDOWHOLESAVE;
                                    }

                                    if (result == DialogResult.No)
                                    {
                                        goto END1;	// 继续作后面的资源
                                    }

                                    if (result == DialogResult.Cancel)
                                    {
                                        strError = "用户中断";
                                        goto ERROR1;	// 中断整个处理
                                    }
                                }
                                 * */
                                goto ERROR1;
                            }
                        }
                    }

                    SetLineInfo(item,
                        // strUsage, 
                        LineState.Normal);
                    SetXmlChanged(item, false);
                    SetResChanged(item, false);
                }

                this.DeleteTempFiles();

                this.Changed = false;
                return nUploadCount;
            ERROR1:
                return -1;
            }
            finally
            {
                if (Stop != null)
                {
#if NO
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    if (nUploadCount > 0)
                        Stop.Initial("上载资源完成");
                    else
                        Stop.Initial("");
                    Stop.Style = old_stop_style;
#endif
                    if (nUploadCount > 0)
                        Stop.Initial("上载资源完成");
                    else
                        Stop.Initial("");
                }
            }
        }

#endif

        public static ObjectInfoCollection FromXml(string strXml)
        {
            return null;
        }

        public string ToXml()
        {
            return "";
        }
    }
}
