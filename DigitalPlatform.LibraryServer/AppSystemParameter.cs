using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和 GetSystemParameter() 和 SetSystemParameter() 相关的代码
    /// </summary>
    public partial class LibraryApplication
    {

        void AppendCaptions(XmlNode node,
    string strAttrName)
        {
            string strDbName = DomUtil.GetAttr(node, strAttrName);
            if (string.IsNullOrEmpty(strDbName) == true)
                return;

            // 2013/2/26
            EnsureKdbs();

            KernelDbInfo db = this.kdbs.FindDb(strDbName);
            if (db != null)
            {
                XmlNode node_container = node.OwnerDocument.CreateElement(strAttrName);
                node.AppendChild(node_container);

                foreach (Caption caption in db.Captions)
                {
                    XmlNode node_caption = node.OwnerDocument.CreateElement("caption");
                    node_container.AppendChild(node_caption);

                    DomUtil.SetAttr(node_caption, "lang", caption.Lang);
                    node_caption.InnerText = caption.Value;
                }
            }
        }

        // TODO: 可以尝试用通用版本的 GetFileNames() 加上回调函数定制出本函数
        // 获得一个子目录内的所有文件名和所有下级子目录内的文件名
        // parameters:
        //      bLastWriteTime  是否在文件名后面附加此文件的最后修改时间
        static List<string> GetFilenames(string strDir,
            bool bLastWriteTime = false,
            bool bExcludeBackupFile = true)
        {
            List<string> results = new List<string>();

            DirectoryInfo di = new DirectoryInfo(strDir);
            FileSystemInfo[] subs = di.GetFileSystemInfos();

            for (int i = 0; i < subs.Length; i++)
            {
                FileSystemInfo sub = subs[i];
                if ((sub.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    results.AddRange(GetFilenames(sub.FullName, bLastWriteTime, bExcludeBackupFile));
                    continue;
                }

                if (bExcludeBackupFile == true && FileUtil.IsBackupFile(sub.FullName) == true)
                    continue;

                if (bLastWriteTime == true)
                    results.Add(sub.FullName + "|" + DateTimeUtil.Rfc1123DateTimeStringEx(sub.LastWriteTime));
                else
                    results.Add(sub.FullName);
            }

            return results;
        }

        static string MakeFileName(DirectoryInfo info)
        {
            return info.Name + "|" + info.LastWriteTime.ToString("u") + "|dir";
        }

        static string MakeFileName(FileInfo info)
        {
            return info.Name + "|" + info.LastWriteTime.ToString("u") + "|" + info.Length.ToString();
        }

        // return:
        //      -1  出错
        //      0   没有找到指定的参数
        //      1   找到指定的参数
        public int GetSystemParameter(
            SessionInfo sessioninfo,
            string strCategory,
            string strName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            this.LockForRead();
            try
            {
                int nRet = 1;

                // 实用功能
                if (strCategory == "utility")
                {
                    if (strName == "getClientIP")
                    {
                        strValue = sessioninfo.ClientIP;
                        goto END1;
                    }
                    // 用于日志记载的前端地址，包括 IP 和 Via 两个部分
                    if (strName == "getClientAddress")
                    {
                        strValue = sessioninfo.ClientAddress;
                        goto END1;
                    }
                }

                if (strCategory == "listUploadFileNames")
                {
                    try
                    {
                        string strDirectory = Path.Combine(this.DataDir, "upload/" + strName);

                        strDirectory = strDirectory.Replace("\\", "/");
                        if (strDirectory[strDirectory.Length - 1] != '/')
                            strDirectory += "/";

                        // 文件名之间的分隔符为 ||，文件名中，和最后修改时间用 | 间隔
                        List<string> filenames = new List<string>();
                        DirectoryInfo di = new DirectoryInfo(strDirectory);

                        // 列出所有目录名
                        DirectoryInfo[] subs = di.GetDirectories();
                        for (int i = 0; i < subs.Length; i++)
                        {
                            DirectoryInfo sub = subs[i];
                            filenames.Add(MakeFileName(sub));
                            // filenames.AddRange(GetFilenames(sub.FullName, true, true));
                        }

                        // 列出所有文件名
                        FileInfo[] fis = di.GetFiles();
                        foreach (FileInfo fi in fis)
                        {
                            filenames.Add(MakeFileName(fi));
                        }

                        StringBuilder text = new StringBuilder();
                        string strHead = strDirectory;
                        foreach (string strFilename in filenames)
                        {
                            if (text.Length > 0)
                                text.Append("||");

                            text.Append(strFilename);

                            // 只取出相对部分
                            // text.Append(strFilename.Substring(strHead.Length));
                        }

                        strValue = text.ToString();
                        goto END1;
                    }
                    catch (DirectoryNotFoundException /*ex*/)
                    {
                        strError = "目录 '" + strName + "' 不存在";
                        goto ERROR1;
                    }
                }

                if (strCategory == "cfgs")
                {
                    // 2015/4/30
                    if (strName == "getDataDir")
                    {
                        strValue = this.DataDir;
                        goto END1;
                    }
                    if (strName == "listFileNames")
                    {
                        List<string> filenames = new List<string>();
                        DirectoryInfo di = new DirectoryInfo(this.DataDir + "/cfgs");
                        DirectoryInfo[] subs = di.GetDirectories();
                        for (int i = 0; i < subs.Length; i++)
                        {
                            DirectoryInfo sub = subs[i];
                            filenames.AddRange(GetFilenames(sub.FullName, false, true));
                        }

                        string strHead = this.DataDir + "/cfgs/";
                        foreach (string strFilename in filenames)
                        {
                            if (string.IsNullOrEmpty(strValue) == false)
                                strValue += ",";
                            // 只取出相对部分
                            strValue += strFilename.Substring(strHead.Length);
                        }

                        goto END1;
                    }
                    if (strName == "listFileNamesEx")
                    {
                        // 文件名之间的分隔符为 ||，文件名中，和最后修改时间用 | 间隔
                        List<string> filenames = new List<string>();
                        DirectoryInfo di = new DirectoryInfo(Path.Combine(this.DataDir, "cfgs"));
                        DirectoryInfo[] subs = di.GetDirectories();
                        for (int i = 0; i < subs.Length; i++)
                        {
                            DirectoryInfo sub = subs[i];
                            filenames.AddRange(GetFilenames(sub.FullName, true, true));
                        }

                        StringBuilder text = new StringBuilder();
                        string strHead = Path.Combine(this.DataDir, "cfgs/");
                        foreach (string strFilename in filenames)
                        {
                            if (text.Length > 0)
                                text.Append("||");
                            // 只取出相对部分
                            text.Append(strFilename.Substring(strHead.Length));
                        }

                        strValue = text.ToString();
                        goto END1;
                    }
#if NO
                    // 取得文件内容
                    if (StringUtil.HasHead(strName, "getfile:") == true)
                    {
                        string strFileName = strName.Substring("getfile:".Length);

                        string strFilePath = this.DataDir + "/cfgs/" + strFileName;

                        Encoding encoding = null;
                        // return:
                        //      -1  出错
                        //      0   文件不存在
                        //      1   文件存在
                        //      2   读入的内容不是全部
                        nRet = FileUtil.ReadTextFileContent(strFilePath,
                            1024 * 1024,    // 1M
                            out strValue,
                            out encoding,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = "文件 '" + strFileName + "' 不存在";
                            goto ERROR1;
                        }
                        if (nRet == 2)
                        {
                            strError = "文件 '" + strFileName + "' 尺寸太大";
                            goto ERROR1;
                        }

                        nRet = 1;
                    }
#endif
                }

                // 获得内核配置文件的时间戳?
                if (strCategory == "cfgs/get_res_timestamps")
                {
                    string[] filenames = strName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); // RemoveEmptyEntries 2013/12/12
                    // TODO: 
                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }
                    StringBuilder text = new StringBuilder();
                    foreach (string filename in filenames)
                    {
                        string strXml = "";
                        string strMetaData = "";
                        byte[] timestamp = null;
                        string strOutputPath = "";
                        long lRet = channel.GetRes(filename,
        "timestamp",
        out strXml,
        out strMetaData,
        out timestamp,
        out strOutputPath,
        out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                continue;
                            goto ERROR1;
                        }
                        if (text.Length > 0)
                            text.Append(",");
                        text.Append(filename + "|" + ByteArray.GetHexTimeStampString(timestamp));
                    }
                    strValue = text.ToString();
                    goto END1;
                }

                if (strCategory == "center")
                {
                    if (strName == "def")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("center");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                        {
                            // 将密码变成明文
                            strValue = root.OuterXml;
                            if (string.IsNullOrEmpty(strValue) == false)
                            {
                                XmlDocument temp = new XmlDocument();
                                temp.LoadXml(strValue);
                                XmlNodeList nodes = temp.DocumentElement.SelectNodes("//server");
                                foreach (XmlNode node in nodes)
                                {
                                    string strPassword = DomUtil.GetAttr(node, "password");
                                    strPassword = LibraryApplication.DecryptPassword(strPassword);
                                    DomUtil.SetAttr(node, "password", strPassword);
                                }
                                strValue = temp.DocumentElement.OuterXml;
                            }
                        }

                        goto END1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }

                if (strCategory == "system")
                {
                    // 2019/1/11
                    // RFID 相关定义
                    if (strName == "rfid")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rfid");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = root.OuterXml;

                        goto END1;
                    }

                    // 2018/7/17
                    // 获得 dp2library 失效期
                    if (strName == "expire")
                    {
                        strValue = _expire.ToLongDateString();
                        goto END1;
                    }

                    // 2018/6/19
                    // 获得系统挂起状态
                    if (strName == "hangup")
                    {
                        strValue = StringUtil.MakePathList(this.HangupList);
                        goto END1;
                    }

                    // 2016/6/25
                    // MSMQ 队列名
                    if (strName == "outgoingQueue")
                    {
                        strValue = this.OutgoingQueue;
                        goto END1;
                    }

                    // 2016/6/25
                    // dp2library 版本号
                    if (strName == "version")
                    {
                        strValue = LibraryApplication.Version;
                        goto END1;
                    }

                    // 2016/4/6
                    // 获得系统的临时文件目录
                    if (strName == "systemTempDir")
                    {
                        string strTempFileName = Path.GetTempFileName();
                        File.Delete(strTempFileName);
                        strValue = Path.GetDirectoryName(strTempFileName);
                        goto END1;
                    }

                    if (strName == "libraryCodes")
                    {
                        List<string> librarycodes = new List<string>();
                        XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
                        foreach (XmlNode node in nodes)
                        {
                            string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");
                            if (string.IsNullOrEmpty(strLibraryCode) == true)
                                continue;
                            librarycodes.Add(strLibraryCode);
                        }

                        nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("rightsTable/library");
                        foreach (XmlNode node in nodes)
                        {
                            string strLibraryCode = DomUtil.GetAttr(node, "code");
                            if (string.IsNullOrEmpty(strLibraryCode) == true)
                                continue;
                            librarycodes.Add(strLibraryCode);
                        }

                        StringUtil.RemoveDupNoSort(ref librarycodes);
                        strValue = StringUtil.MakePathList(librarycodes);
                        goto END1;
                    }

                    if (strName == "arrived")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("arrived");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = root.OuterXml;

                        goto END1;
                    }

                    // 2009/10/23 
                    // 获得<itemdbgroup>元素下级XML
                    if (strName == "biblioDbGroup")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("itemdbgroup");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                            // 注: 返回值为0，字符串为空，错误码不是NotFound，表示想关节点找到了，但值为空
                        }
                        else
                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(root.OuterXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "<itemdbgroup>元素XML片段装入DOM时出错: " + ex.Message;
                                goto ERROR1;
                            }

                            strError = EnsureKdbs(false);
                            if (strError != null)
                                goto ERROR1;

                            // 将name属性名修改为itemDbName属性
                            // TODO: 将来library.xml格式修改后，这部分可以免去了
                            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
                            for (int i = 0; i < nodes.Count; i++)
                            {
                                XmlNode node = nodes[i];
                                string strItemDbName = DomUtil.GetAttr(node, "name");
                                DomUtil.SetAttr(node, "name", null);
                                DomUtil.SetAttr(node, "itemDbName", strItemDbName);

                                // 2012/7/2
                                // 加入各个数据库的多语种名字

                                // 实体库
                                AppendCaptions(node, "itemDbName");

                                // 订购库
                                AppendCaptions(node, "orderDbName");

                                // 期库
                                AppendCaptions(node, "issueDbName");

                                // 评注库
                                AppendCaptions(node, "commentDbName");

                                // 书目库
                                AppendCaptions(node, "biblioDbName");
                            }

                            strValue = dom.DocumentElement.InnerXml;
                        }

                        goto END1;
                    }

                    // 2012/9/12
                    // 获得<readerdbgroup>元素下级XML
                    if (strName == "readerDbGroup")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("readerdbgroup");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                            // 注: 返回值为0，字符串为空，错误码不是NotFound，表示相关节点找到了，但值为空
                            goto END1;
                        }

                        if (sessioninfo.GlobalUser == true)
                            strValue = root.InnerXml;
                        else
                        {
                            // 过滤掉当前用户不能管辖的读者库名
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(root.OuterXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "<readerdbgroup>元素XML片段装入DOM时出错: " + ex.Message;
                                goto ERROR1;
                            }

                            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
                            for (int i = 0; i < nodes.Count; i++)
                            {
                                XmlNode node = nodes[i];
                                string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                                if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                                {
                                    node.ParentNode.RemoveChild(node);
                                }
                            }

                            strValue = dom.DocumentElement.InnerXml;
                        }

                        goto END1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }

                // OPAC检索
                if (strCategory == "opac")
                {
                    // TODO: 和def重复了，需要合并
                    // 获得<virtualDatabases>元素下级XML
                    if (strName == "databases")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("virtualDatabases");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = root.InnerXml;

                        goto END1;
                    }

                    // 获得<browseformats>元素下级XML
                    if (strName == "browseformats")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
                        if (root == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = root.InnerXml;

                        goto END1;
                    }

                    // 2011/2/15
                    if (strName == "serverDirectory")
                    {
                        /*
                        XmlNode node = this.LibraryCfgDom.SelectSingleNode("//opacServer");
                        if (node == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = DomUtil.GetAttr(node, "url");
                        */
                        strValue = this.OpacServerUrl;
                        goto END1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }

                if (strCategory == "circulation")
                {
                    // 2016/1/1
                    if (strName == "chargingOperDatabase")
                    {
                        if (this.ChargingOperDatabase.Enabled == true)
                            strValue = "enabled";
                        else
                            strValue = "";
                        nRet = 1;
                        goto END1;
                    }

                    // <clientFineInterface>元素内容
                    // strValue中是OuterXml定义。
                    if (strName == "clientFineInterface")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("clientFineInterface");
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.OuterXml;
                        nRet = 1;
                        goto END1;
                    }

                    // <valueTables>元素内容
                    // strValue中是下级片断定义，没有<valueTables>元素作为根。
                    if (strName == "valueTables")
                    {
                        // 按照馆代码列表，返回<valueTables>内的适当片断
                        nRet = this.GetValueTablesXml(
                                sessioninfo.LibraryCodeList,
                                out strValue,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nRet = 1;
                        goto END1;
                    }

                    // <rightsTable>元素内容
                    // strValue中是下级片断定义，没有<rightsTable>元素作为根。
                    if (strName == "rightsTable")
                    {
#if NO
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable");   // 0.02前为rightstable
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
#endif
                        // 按照馆代码列表，返回<rightsTable>内的适当片断
                        nRet = this.GetRightsTableXml(
                                sessioninfo.LibraryCodeList,
                                out strValue,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nRet = 1;
                        goto END1;
                    }

                    // (当前<rightsTable>)权限表的HTML形态
                    if (strName == "rightsTableHtml")
                    {
                        nRet = this.GetRightTableHtml(
                            "",
                            sessioninfo.LibraryCodeList,
                            out strValue,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nRet = 1;
                        goto END1;
                    }

                    /*
                    // 2008/10/10 
                    // <readertypes>元素内容
                    // strValue中是下级片断定义，没有<readertypes>元素作为根。
                    if (strName == "readerTypes")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable/readerTypes");   // 0.02前为readertypes
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }

                    // 2008/10/10 
                    // <booktypes>元素内容
                    // strValue中是下级片断定义，没有<booktypes>元素作为根。
                    if (strName == "bookTypes")
                    {
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable/bookTypes"); // 0.02前为booktypes
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }*/

                    // 2008/10/10 
                    // <locationtypes>元素内容
                    // strValue中是下级片断定义，没有<locationTypes>元素作为根。
                    if (strName == "locationTypes")
                    {
#if NO
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes"); // 0.02前为locationtypes
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
#endif
                        // 按照馆代码列表，返回<locationTypes>内的适当片断
                        nRet = this.GetLocationTypesXml(
                                sessioninfo.LibraryCodeList,
                                out strValue,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nRet = 1;
                        goto END1;
                    }

                    // 2008/10/12 
                    // <zhongcihao>元素内容
                    // strValue中是下级片断定义，没有<zhongcihao>元素作为根。
                    if (strName == "zhongcihao")
                    {
                        // 分馆用户也能看到全部<zhongcihao>定义
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("zhongcihao");
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }

                    // 2009/2/18 
                    // <callNumber>元素内容
                    // strValue中是下级片断定义，没有<callNumber>元素作为根。
                    if (strName == "callNumber")
                    {
                        // 分馆用户可以看到全部定义
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("callNumber");
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }

                    // 2009/3/9 
                    // <dup>元素内容
                    // strValue中是下级片断定义，没有<dup>元素作为根。
                    if (strName == "dup")
                    {
                        // 分馆用户也能看到全部<dup>定义
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("dup");
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }

                    // 2008/10/13 2019/5/31
                    // <script> 或 <barcodeValidation> 元素内容
                    // strValue中是下级片断定义，没有<script>元素作为根。
                    if (strName == "script" || strName == "barcodeValidation")
                    {
                        // 分馆用户也能看到全部<script>定义
                        XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(strName);
                        if (root == null)
                        {
                            nRet = 0;
                            goto END1;
                        }

                        strValue = root.InnerXml;
                        nRet = 1;
                        goto END1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }

                // 根据前端在strName参数中提供的rightstable xml字符串，立即创建rightsTableHtml字符串
                if (strCategory == "instance_rightstable_html")
                {
                    nRet = this.GetRightTableHtml(
                        strName,
                        sessioninfo.LibraryCodeList,
                        out strValue,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nRet = 1;
                    goto END1;
                }

                // 获得内核数据库原始定义
                if (strCategory == "database_def")
                {
                    // strName参数不能为空。本功能只能得到一个数据库的定义，如果要得到全部数据库的定义，请使用ManageDatabase API的getinfo子功能
                    nRet = this.vdbs.GetDatabaseDef(
                        strName,
                        out strValue,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    goto END1;
                }

                // 实用库
                if (strCategory == "utilDb")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
                                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//utilDb/database");
                                for (int i = 0; i < nodes.Count; i++)
                                {
                                    string strDbName = DomUtil.GetAttr(nodes[i], "name");
                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
                            }
                            break;
                        case "types":
                            {
                                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//utilDb/database");
                                for (int i = 0; i < nodes.Count; i++)
                                {
                                    string strType = DomUtil.GetAttr(nodes[i], "type");
                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strType;
                                }
                            }
                            break;
                        default:
                            /*
                            nRet = 0;
                            break;
                             * */
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }

                    // 2009/10/23 
                    goto END1;
                }

                if (strCategory == "amerce")
                {
                    switch (strName)
                    {
                        case "dbname":
                            strValue = this.AmerceDbName;
                            break;
                        case "overduestyle":
                            strValue = this.OverdueStyle;
                            break;
                        default:
                            /*
                            nRet = 0;
                            break;
                             * */
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                // 2015/6/13
                if (strCategory == "arrived")
                {
                    switch (strName)
                    {
                        case "dbname":
                            strValue = this.ArrivedDbName;
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    goto END1;
                }

                if (strCategory == "biblio")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
                                for (int i = 0; i < this.ItemDbs.Count; i++)
                                {
                                    string strDbName = this.ItemDbs[i].BiblioDbName;

                                    // 即便数据库名为空，逗号也不能省略。主要是为了准确对位

                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
                            }
                            break;
                        case "syntaxs":
                            {
                                for (int i = 0; i < this.ItemDbs.Count; i++)
                                {
                                    string strSyntax = this.ItemDbs[i].BiblioDbSyntax;

                                    // 即便strSyntax为空，逗号也不能省略。主要是为了准确对位


                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strSyntax;
                                }
                            }
                            break;
                        default:
                            /*
                            nRet = 0;
                            break;
                             * */
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                if (strCategory == "virtual")
                {
                    switch (strName)
                    {
                        // 2011/1/21
                        case "def":
                            {
                                /*
                                // TODO: 把这个初始化放在正规的初始化中？
                                nRet = this.InitialVdbs(sessioninfo.Channels,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "InitialVdbs error : " + strError;
                                    goto ERROR1;
                                }
                                 * */


                                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
                                    "virtualDatabases");
                                if (root == null)
                                {
                                    strError = "尚未配置<virtualDatabases>元素";
                                    goto ERROR1;
                                }
                                strValue = root.OuterXml;
                            }
                            break;
                        case "dbnames":
                            {
                                /*
                                // TODO: 把这个初始化放在正规的初始化中？
                                nRet = this.InitialVdbs(sessioninfo.Channels,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "InitialVdbs error : " + strError;
                                    goto ERROR1;
                                }
                                 * */

                                if (this.vdbs != null)
                                {
                                    for (int i = 0; i < this.vdbs.Count; i++)
                                    {
                                        VirtualDatabase vdb = this.vdbs[i];
                                        if (vdb.IsVirtual == false)
                                            continue;

                                        if (String.IsNullOrEmpty(strValue) == false)
                                            strValue += ",";
                                        strValue += vdb.GetName("zh");
                                    }
                                }
                            }
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }


                if (strCategory == "item")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
                                for (int i = 0; i < this.ItemDbs.Count; i++)
                                {
                                    string strDbName = this.ItemDbs[i].DbName;

                                    // 即便strDbName为空，逗号也不能省略。主要是为了准确对位

                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
                            }
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                // 2007/10/19 
                if (strCategory == "issue")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
                                for (int i = 0; i < this.ItemDbs.Count; i++)
                                {
                                    string strDbName = this.ItemDbs[i].IssueDbName;

                                    // 即便strDbName为空，逗号也不能省略。主要是为了准确对位

                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
                            }
                            break;
                        default:
                             strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                // 2007/11/30 
                if (strCategory == "order")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
                                for (int i = 0; i < this.ItemDbs.Count; i++)
                                {
                                    string strDbName = this.ItemDbs[i].OrderDbName;

                                    // 即便strDbName为空，逗号也不能省略。主要是为了准确对位

                                    if (i != 0)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
                            }
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                if (strCategory == "reader")
                {
                    switch (strName)
                    {
                        case "dbnames":
                            {
#if NO
                                for (int i = 0; i < this.ReaderDbs.Count; i++)
                                {
                                    string strDbName = this.ReaderDbs[i].DbName;
                                    if (String.IsNullOrEmpty(strDbName) == true)
                                        continue;

                                    // 2012/9/7
                                    if (string.IsNullOrEmpty(sessioninfo.LibraryCode) == false)
                                    {
                                        string strLibraryCode = this.ReaderDbs[i].LibraryCode;
                                        // 匹配图书馆代码
                                        // parameters:
                                        //      strSingle   单个图书馆代码。空的总是不能匹配
                                        //      strList     图书馆代码列表，例如"第一个,第二个"，或者"*"。空表示都匹配
                                        // return:
                                        //      false   没有匹配上
                                        //      true    匹配上
                                        if (LibraryApplication.MatchLibraryCode(strLibraryCode, sessioninfo.LibraryCode) == false)
                                            continue;
                                    }

                                    if (String.IsNullOrEmpty(strValue) == false)
                                        strValue += ",";
                                    strValue += strDbName;
                                }
#endif
                                List<string> dbnames = this.GetCurrentReaderDbNameList(sessioninfo.LibraryCodeList);
                                strValue = StringUtil.MakePathList(dbnames);
                            }
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

                if (strCategory == "library")
                {
                    switch (strName)
                    {
                        case "name":
                            {
                                XmlNode node = this.LibraryCfgDom.SelectSingleNode("//libraryName");
                                if (node == null)
                                    strValue = "";
                                else
                                    strValue = node.InnerText;
                            }
                            break;
                        default:
                            strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                            goto NOTFOUND;
                    }
                    // 2009/10/23 
                    goto END1;
                }

            NOTFOUND:
                if (String.IsNullOrEmpty(strError) == true)
                    strError = "未知的 category '" + strCategory + "' 和 name '" + strName + "'";
                return 0;
            END1:
                return nRet;
            ERROR1:
                return -1;
            }
            finally
            {
                this.UnlockForRead();
            }
        }
    }
}
