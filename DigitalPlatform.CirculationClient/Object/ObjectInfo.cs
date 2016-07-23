using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Range;
using DigitalPlatform.IO;

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

        string _metadata { get; set; }
        public string Metadata
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value;
            }
        }

        string GetMetadataField(string name)
        {
            string strError = "";
            // 取metadata值
            Hashtable values = StringUtil.ParseMedaDataXml(this.Metadata,
                out strError);
            if (values == null)
                throw new Exception(strError);
            return (string)values[name];
        }

        void SetMetadataField(string name, string value)
        {
            XmlDocument dom = new XmlDocument();
            if (string.IsNullOrEmpty(this.Metadata))
                dom.LoadXml("<file />");
            else
                dom.LoadXml(this.Metadata);

            dom.DocumentElement.SetAttribute(name, value);
            this.Metadata = dom.DocumentElement.OuterXml;
        }

        // 以下几项是从 metadata 里面析出的信息
        // 本地文件名
        public string FileName
        {
            get
            {
                return GetMetadataField("fileName");
            }
            set
            {
                SetMetadataField("fileName", value);
            }
        }

        public string Size
        {
            get
            {
                return GetMetadataField("size");
            }
            set
            {
                SetMetadataField("size", value);
            }
        }
        public string Mime
        {
            get
            {
                return GetMetadataField("mime");
            }
            set
            {
                SetMetadataField("mime", value);
            }
        }
        public string Timestamp
        {
            get
            {
                return GetMetadataField("timestamp");
            }
            set
            {
                SetMetadataField("timestamp", value);
            }
        }

        public LineState LineState = LineState.Normal;

        public bool ResChanged { get; set; }
        public bool XmlChanged { get; set; }

        public string ToXml()
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<object />");

            XmlElement root = dom.DocumentElement;
            root.SetAttribute("id", this.ID);
            root.SetAttribute("usage", this.Usage);
            root.SetAttribute("rights", this.Rights);
            root.SetAttribute("errorInfo", this.ErrorInfo);
            root.SetAttribute("metadata", this.Metadata);
#if NO
            root.SetAttribute("fileName", this.FileName);
            root.SetAttribute("size", this.Size);
            root.SetAttribute("mime", this.Mime);
            root.SetAttribute("timestamp", this.Timestamp);
#endif

            root.SetAttribute("lineState", this.LineState.ToString());
            root.SetAttribute("resChanged", this.ResChanged ? "true" : "false");
            root.SetAttribute("xmlChanged", this.XmlChanged ? "true" : "false");
            return dom.DocumentElement.OuterXml;
        }

        public static ObjectInfo FromXml(string strXml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            XmlElement root = dom.DocumentElement;
            ObjectInfo info = new ObjectInfo();
            info.ID = root.GetAttribute("id");
            info.Usage = root.GetAttribute("usage");
            info.Rights = root.GetAttribute("rights");
            info.ErrorInfo = root.GetAttribute("errorInfo");
            info.Metadata = root.GetAttribute("metadata");
            info.LineState = (LineState)Enum.Parse(typeof(LineState),
                root.GetAttribute("lineState"));
            info.ResChanged = DomUtil.IsBooleanTrue(root.GetAttribute("resChanged"));
            info.XmlChanged = DomUtil.IsBooleanTrue(root.GetAttribute("xmlChanged"));
            return info;
        }

        public bool IsLocal()
        {
            if (this.ResChanged)
                return true;
            return false;
        }

        public bool IsDeleted()
        {
            return this.LineState == CirculationClient.LineState.Deleted;
        }
    }

    /// <summary>
    /// 对象资源集合
    /// </summary>
    public class ObjectInfoCollection : List<ObjectInfo>
    {
        public string HostRecPath { get; set; }
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

            this.HostRecPath = strBiblioRecPath;

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

                string strResPath = this.HostRecPath + "/object/" + info.ID;
                strResPath = strResPath.Replace(":", "/");
                recpaths.Add(strResPath);
            }

            if (recpaths.Count > 0)
            {
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

#if NO
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
#endif

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

            if (String.IsNullOrEmpty(this.HostRecPath) == true)
            {
                strError = "尚未指定 BiblioRecPath";
                return -1;
            }

            if (IsNewPath(this.HostRecPath) == true)
            {
                strError = "宿主记录路径 '" + this.HostRecPath + "' 不是已保存的记录路径，无法用于对象资源上载";
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
            List<ObjectInfo> delete_objects = new List<ObjectInfo>();
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
                            delete_objects.Add(obj);
                        }

                        continue;
                    }

                    // string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);

                    string strResPath = this.HostRecPath + "/object/" + obj.ID;

                    byte[] timestamp = ByteArray.GetTimeStampByteArray(obj.Timestamp);
                    byte[] output_timestamp = null;

                    nUploadCount++;

                    if (bOnlyChangeMetadata)
                    {
                        long lRet = channel.SaveResObject(
    stop,
    strResPath,
    "",
    obj.FileName,
    obj.Mime,
    "", // range
    true,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
    timestamp,
    out output_timestamp,
    out strError);
                        timestamp = output_timestamp;
                        if (timestamp != null)
                            obj.Timestamp = ByteArray.GetHexTimeStampString(timestamp);
                        if (lRet == -1)
                            goto ERROR1;
                        Debug.Assert(timestamp != null, "");
                        // TODO: 出错的情况下是否要修改 timestamp 显示？是否应为非空才兑现显示
                    }
                    else
                    {
                        // 检测文件尺寸
                        FileInfo fi = new FileInfo(obj.FileName);

                        if (fi.Exists == false)
                        {
                            strError = "文件 '" + obj.FileName + "' 不存在...";
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
                                channel.UploadResChunkSize // 500 * 1024
                                );
                        }

                        // REDOWHOLESAVE:
                        string strWarning = "";

                        for (int j = 0; j < ranges.Length; j++)
                        {
                            // Application.DoEvents();	// 出让界面控制权

                            if (stop != null && stop.State != 0)
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

                            if (stop != null)
                                stop.SetMessage("正在上载 " + ranges[j] + "/"
                                    + Convert.ToString(fi.Length)
                                    + " " + strPercent + " " + obj.FileName + strWarning + strWaiting);

                            long lRet = 0;
                            TimeSpan old_timeout = channel.Timeout;
                            channel.Timeout = new TimeSpan(0, 5, 0);
                            try
                            {
                                lRet = channel.SaveResObject(
                                    stop,
                                    strResPath,
                                    obj.FileName,
                                    obj.FileName,
                                    obj.Mime,
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
                                obj.Timestamp = ByteArray.GetHexTimeStampString(timestamp);

                            strWarning = "";

                            if (lRet == -1)
                                goto ERROR1;
                        }
                    }

                    obj.LineState = LineState.Normal;
                    obj.XmlChanged = false;
                    obj.ResChanged = false;
                }

                this.Changed = false;
                return nUploadCount;
            ERROR1:
                return -1;
            }
            finally
            {
                if (stop != null)
                {
                    if (nUploadCount > 0)
                        stop.Initial("上载资源完成");
                    else
                        stop.Initial("");
                }
            }
        }

        public static ObjectInfoCollection FromXml(string strXml)
        {
            ObjectInfoCollection objects = new ObjectInfoCollection();

            if (string.IsNullOrEmpty(strXml))
                return objects;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name == "object")
                {
                    ObjectInfo info = ObjectInfo.FromXml(node.OuterXml);
                    objects.Add(info);
                }
            }

            objects.Changed = DomUtil.IsBooleanTrue(dom.DocumentElement.GetAttribute("changed"));
            objects.HostRecPath = dom.DocumentElement.GetAttribute("recPath");
            return objects;
        }

        public string ToXml()
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (ObjectInfo info in this)
            {
                XmlDocumentFragment frag = dom.CreateDocumentFragment();
                frag.InnerXml = info.ToXml();
                dom.DocumentElement.AppendChild(frag);
            }

            dom.DocumentElement.SetAttribute("changed", this.Changed ? "true" : "flase");
            dom.DocumentElement.SetAttribute("recPath", this.HostRecPath);
            return dom.DocumentElement.OuterXml;
        }

        public List<ObjectInfo> FindByUsage(string strUsage)
        {
            List<ObjectInfo> results = new List<ObjectInfo>();
            foreach (ObjectInfo info in this)
            {
                if (strUsage == "*" || info.Usage == strUsage)
                    results.Add(info);
            }

            return results;
        }

        string GetNewID()
        {
            List<string> ids = new List<string>();
            foreach (ObjectInfo info in this)
            {
                ids.Add(info.ID);
            }

            int nSeed = 0;
            string strID = "";
            for (; ; )
            {
                strID = Convert.ToString(nSeed++);
                if (ids.IndexOf(strID) == -1)
                    return strID;
            }
        }

        public int MaskDeleteObjects(List<ObjectInfo> infos)
        {
            bool bRemoved = false;   // 是否发生过物理删除listview item的情况
            int nMaskDeleteCount = 0;
            foreach (ObjectInfo info in infos)
            {
                // 如果本来就是已经标记删除的事项
                if (info.LineState == LineState.Deleted)
                    continue;

                // 如果本来就是新增事项，那么彻底从listview中移除
                if (info.LineState == LineState.New)
                {
                    bRemoved = true;
                    this.Remove(info);
                    nMaskDeleteCount++;
                    continue;
                }

                info.LineState = LineState.Deleted;

                this.Changed = true;
                nMaskDeleteCount++;
            }

#if NO
            if (bRemoved == true)
            {
                // 需要看看listview中是不是至少有一个需要保存的事项？否则Changed设为false
                if (IsChanged() == false)
                {
                    this.Changed = false;
                    return 0;
                }
            }
#endif

            if (nMaskDeleteCount > 0)
                this.Changed = true;
            return nMaskDeleteCount;
        }

        // 确认是否还有增删改的事项
        // 而this.Changed不是那么精确的
        bool IsChanged()
        {
            foreach (ObjectInfo info in this)
            {
                if (info.LineState == LineState.Changed
                    || info.LineState == LineState.Deleted
                    || info.LineState == LineState.New)
                    return true;
            }

            return false;
        }

        public int AppendNewItem(
    string strObjectFilePath,
    string strUsage,
    string strRights,
    out ObjectInfo info,
    out string strError)
        {
            strError = "";

            info = new ObjectInfo();
            info.LineState = LineState.New;
            info.XmlChanged = true;
            info.ResChanged = true;

            info.ID = GetNewID();
            info.Mime = PathUtil.MimeTypeFrom(strObjectFilePath);
            FileInfo fileInfo = new FileInfo(strObjectFilePath);
            info.FileName = fileInfo.FullName;
            info.Size = Convert.ToString(fileInfo.Length);
            info.Usage = strUsage;
            info.Rights = strRights;
            this.Add(info);

            this.Changed = true;
            return 0;
        }

        public int ChangeObjectFile(ObjectInfo info,
    string strObjectFilePath,
    string strUsage,
    string strRights,
    out string strError)
        {
            strError = "";

            if (this.IndexOf(info) == -1)
            {
                strError = "info 不是当前集合的元素之一";
                return -1;
            }

            if (info.LineState == LineState.Deleted)
            {
                strError = "对已经标记删除的对象不能进行修改...";
                return -1;
            }

            LineState old_state = info.LineState;
            string strOldUsage = info.Usage;
            string strOldRights = info.Rights;

            info.Mime = PathUtil.MimeTypeFrom(strObjectFilePath);
            FileInfo fileInfo = new FileInfo(strObjectFilePath);
            info.FileName = fileInfo.FullName;
            info.Size = Convert.ToString(fileInfo.Length);
            if (strUsage != null)
                info.Usage = strUsage;
            if (strRights != null)
                info.Rights = strRights;
            // info.Timestamp = null;   // 以前的时间戳不要修改

            if (old_state != LineState.New)
            {
                info.LineState = LineState.Changed;
                info.ResChanged = true;
            }
            else
            {
                info.ResChanged = true;
            }

            if (strOldRights != info.Rights
                || strOldUsage != info.Usage)
                info.XmlChanged = true;
            this.Changed = true;
            return 0;
        }

        public int SetObjectByUsage(
    string strFileName,
    string strUsage,
    out string strID,
    out string strError)
        {
            strError = "";
            strID = "";
            int nRet = 0;

            ObjectInfo info = null;
            List<ObjectInfo> infos = this.FindByUsage(strUsage);
            if (infos.Count == 0)
            {
                nRet = this.AppendNewItem(
                    strFileName,
                    strUsage,
                    null, // rights
                    out info,
                    out strError);
            }
            else
            {
                info = infos[0];

                nRet = this.ChangeObjectFile(info,
                    strFileName,
                    strUsage,
                    null,
                    out strError);
            }
            if (nRet == -1)
                return -1;

            strID = info.ID;
            return 0;
        }

        public void MaskDeleteCoverImageObject()
        {
            List<ObjectInfo> infos = new List<ObjectInfo>();
            foreach (ObjectInfo info in this)
            {
                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(info.Usage, "FrontCover.") == true
                    || info.Usage == "FrontCover")
                    infos.Add(info);
            }

            if (infos.Count > 0)
                MaskDeleteObjects(infos);
        }

        // 获得特定的数字对象
        public ObjectInfo GetCoverImageObject(string strPreferredType = "MediumImage")
        {
            ObjectInfo large = null;
            ObjectInfo medium = null;   // type:FrontCover.MediumImage
            ObjectInfo normal = null; // type:FronCover
            ObjectInfo small = null;

            foreach (ObjectInfo info in this)
            {
                string strID = info.ID;
                string strType = info.Usage;

                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(strType, "FrontCover." + strPreferredType) == true)
                    return info;

                if (StringUtil.HasHead(strType, "FrontCover.SmallImage") == true)
                    small = info;
                else if (StringUtil.HasHead(strType, "FrontCover.MediumImage") == true)
                    medium = info;
                else if (StringUtil.HasHead(strType, "FrontCover.LargeImage") == true)
                    large = info;
                else if (StringUtil.HasHead(strType, "FrontCover") == true)
                    normal = info;
            }

            if (large != null)
                return large;
            if (medium != null)
                return medium;
            if (normal != null)
                return normal;
            return small;
        }

        // 在 XmlDocument 对象中添加 <file> 元素。新元素加入在根之下
        public int AddFileFragments(ref XmlDocument domRecord,
            bool bRemoveOldFileElements,
            out string strError)
        {
            strError = "";

            if (bRemoveOldFileElements)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);

                // 清除以前的<dprms:file>元素
                XmlNodeList nodes = domRecord.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                foreach (XmlNode node in nodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            foreach (ObjectInfo info in this)
            {
                if (String.IsNullOrEmpty(info.ID) == true)
                    continue;

                LineState state = info.LineState;
                // 如果是已经标记删除的事项
                if (state == LineState.Deleted)
                    continue;

                XmlElement node = domRecord.CreateElement("dprms",
                    "file",
                    DpNs.dprms);
                domRecord.DocumentElement.AppendChild(node);

                node.SetAttribute("id", info.ID);
                if (string.IsNullOrEmpty(info.Usage) == false)
                    node.SetAttribute("usage", info.Usage);
                if (string.IsNullOrEmpty(info.Rights) == false)
                    node.SetAttribute("rights", info.Rights);
            }

            return 0;
        }
    }
}
