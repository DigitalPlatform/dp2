using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和 GetSystemParameter() 和 SetSystemParameter() 相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 判断 database/@biblioDbName 中的书目库名是否在当前账户的可访问范围内
        // return:
        //      -1  出错
        //      0   不可读
        //      1   可读
        int IsBiblioDbReadeable(
            SessionInfo sessioninfo,
            XmlElement database,
            out string strError)
        {
            strError = "";

            string strDbName = database.GetAttribute("biblioDbName");
            if (string.IsNullOrEmpty(strDbName))
                return 1;

            // 检查当前用户是否具备 GetBiblioInfo() API 的存取定义权限
            // parameters:
            //      check_normal_right 是否要连带一起检查普通权限？如果不连带，则本函数可能返回 "normal"，意思是需要追加检查一下普通权限
            // return:
            //      "normal"    (存取定义已经满足要求了，但)还需要进一步检查普通权限
            //      null    具备权限
            //      其它      不具备权限。文字是报错信息
            var error = CheckGetBiblioInfoAccess(
                sessioninfo,
                "biblio",
                strDbName,
                true,
                out _);
            if (error == null)
                return 1;
            return 0;
        }

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

        // 2019/7/12
        // library.xml 中是否定义了 barcodeValidation 元素
        public bool BarcodeValidation
        {
            get
            {
                XmlNode root = this.LibraryCfgDom?.DocumentElement?.SelectSingleNode("barcodeValidation");
                return root != null;
            }
        }

        /*
        static bool IsManaged(
            string strLibraryCode,
            string strAccountLibraryCodeList)
        {
            if (SessionInfo.IsGlobalUser(strAccountLibraryCodeList) == true)
                return true;

            if (StringUtil.IsInList(strLibraryCode, strAccountLibraryCodeList) == true)
                return true;

            return false;
        }
        */

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
                            if (channel.IsNotFound())
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

                // 2020/7/17
                // RFID 相关定义: 获得册记录(或者馆代码)关联的 OI
                // 注意，不能用于读者记录
                if (strCategory == "rfid/getOwnerInstitution")
                {
                    var rfid = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rfid") as XmlElement;
                    if (rfid == null)
                    {
                        strError = $"library.xml 中没有配置 rfid 元素";
                        nRet = 0;
                        goto END1;
                    }

                    try
                    {
                        // strName 是纯净的 location
                        // return:
                        //      true    找到。信息在 isil 和 alternative 参数里面返回
                        //      false   没有找到
                        // exception:
                        //      可能会抛出异常 Exception
                        var ret = LibraryServerUtil.GetOwnerInstitution(rfid,
                            strName,
                            "entity",
                            out string isil,
                            out string alternative);
                        if (ret == false)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                        {
                            strValue = isil + "|" + alternative;
                            nRet = 1;
                        }

                        strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                        goto NOTFOUND;
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        goto ERROR1;
                    }
                }

#if REMOVED
                // 2020/7/17
                // RFID 相关定义: 获得读者记录的 OI
                if (strCategory == "rfid/patronOI")
                {
                    var rfid = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rfid") as XmlElement;
                    if (rfid == null)
                    {
                        strError = $"library.xml 中没有配置 rfid 元素";
                        nRet = 0;
                        goto END1;
                    }
                    // strName 是馆代码
                    // return:
                    //      true    找到。信息在 isil 和 alternative 参数里面返回
                    //      false   没有找到
                    var ret = GetPatronOwnerInstitution(rfid,
                        strName,
                        out string isil,
                        out string alternative);
                    if (ret == false)
                    {
                        strValue = "";
                        nRet = 0;
                    }
                    else
                    {
                        strValue = isil + "|" + alternative;
                        nRet = 1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }
#endif

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
                            // for (int i = 0; i < nodes.Count; i++)
                            foreach (XmlElement nodeDatabase in nodes)
                            {
                                // XmlNode nodeDatabase = nodes[i];

                                // 2023/3/16
                                // 判断 database/@biblioDbName 中的书目库名是否在当前账户的可访问范围内
                                // return:
                                //      -1  出错
                                //      0   不可读
                                //      1   可读
                                nRet = IsBiblioDbReadeable(
                                    sessioninfo,
                                    nodeDatabase,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 0)
                                {
                                    nodeDatabase.ParentNode.RemoveChild(nodeDatabase);
                                    continue;
                                }

                                string strItemDbName = DomUtil.GetAttr(nodeDatabase, "name");
                                DomUtil.SetAttr(nodeDatabase, "name", null);
                                DomUtil.SetAttr(nodeDatabase, "itemDbName", strItemDbName);

                                // 2012/7/2
                                // 加入各个数据库的多语种名字

                                // 实体库
                                AppendCaptions(nodeDatabase, "itemDbName");

                                // 订购库
                                AppendCaptions(nodeDatabase, "orderDbName");

                                // 期库
                                AppendCaptions(nodeDatabase, "issueDbName");

                                // 评注库
                                AppendCaptions(nodeDatabase, "commentDbName");

                                // 书目库
                                AppendCaptions(nodeDatabase, "biblioDbName");
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
                        XmlNode nodeDatabase = this.LibraryCfgDom.SelectSingleNode("//opacServer");
                        if (nodeDatabase == null)
                        {
                            strValue = "";
                            nRet = 0;
                        }
                        else
                            strValue = DomUtil.GetAttr(nodeDatabase, "url");
                        */
                        strValue = this.OpacServerUrl;
                        goto END1;
                    }

                    strError = "category '" + strCategory + "' 中未知的 name '" + strName + "'";
                    goto NOTFOUND;
                }

                if (strCategory == "circulation")
                {
                    // 2025/15
                    if (strName == "canReserveOnshelf")
                    {
                        if (this.CanReserveOnshelf == true)
                            strValue = "true";
                        else
                            strValue = "false";
                        nRet = 1;
                        goto END1;
                    }

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
                    // 2024/5/10
                    this.CheckVdbsThrow();

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

        // 异常:
        //      可能会抛出异常
        public int SetSystemParameter(
            SessionInfo sessioninfo,
            string strCategory,
            string strName,
            string strValue,
            out bool succeed,
            out string strError)
        {
            strError = "";
            succeed = false;

            var app = this;

            app.LockForWrite();
            try
            {
                int nRet = 0;

                if (strCategory == "center")
                {
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改<center>元素定义";
                        goto ERROR1;
                    }

                    // 修改 <center> 内的定义
                    // return:
                    //      -1  error
                    //      0   not change
                    //      1   changed
                    nRet = app.SetCenterDef(strName,
                        strValue,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        app.Changed = true;
                        // app.ActivateManagerThread();
                    }
                    goto END1;
                }

                // 值列表
                // 2008/8/21 
                if (strCategory == "valueTable")
                {
                    // TODO: 需要进行针对分馆用户的改造
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改<valueTables>元素定义";
                        goto ERROR1;
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strValue);
                    }
                    catch (Exception ex)
                    {
                        strError = "strValue装入XMLDOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    string strNameParam = DomUtil.GetAttr(dom.DocumentElement, "name");
                    string strDbNameParam = DomUtil.GetAttr(dom.DocumentElement, "dbname");
                    string strValueParam = dom.DocumentElement.InnerText;

                    // 修改值列表
                    // 2008/8/21 
                    // parameters:
                    //      strAction   "new" "change" "overwirte" "delete"
                    // return:
                    //      -1  error
                    //      0   not change
                    //      1   changed
                    nRet = app.SetValueTable(strName,
                        strNameParam,
                        strDbNameParam,
                        strValueParam,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        app.Changed = true;
                        // app.ActivateManagerThread();
                    }
                    goto END1;
                }

                // 读者权限
                if (strCategory == "circulation")
                {
                    // 2021/8/6
                    // 临时修改内存中的 app.AcceptBlankReaderBarcode 值
                    if (strName == "?AcceptBlankReaderBarcode")
                    {
                        app.AcceptBlankReaderBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.AcceptBlankItemBarcode 值
                    if (strName == "?AcceptBlankItemBarcode")
                    {
                        app.AcceptBlankItemBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyBarcode 值
                    if (strName == "?VerifyBarcode")
                    {
                        app.VerifyBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.UpperCaseItemBarcode 值
                    if (strName == "?UpperCaseItemBarcode")
                    {
                        app.UpperCaseItemBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.UpperCaseReaderBarcode 值
                    if (strName == "?UpperCaseReaderBarcode")
                    {
                        app.UpperCaseReaderBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyBookType 值
                    if (strName == "?VerifyBookType")
                    {
                        app.VerifyBookType = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyReaderType 值
                    if (strName == "?VerifyReaderType")
                    {
                        app.VerifyReaderType = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.BorrowCheckOverdue 值
                    if (strName == "?BorrowCheckOverdue")
                    {
                        app.BorrowCheckOverdue = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.CirculationNotifyTypes 值
                    if (strName == "?CirculationNotifyTypes")
                    {
                        app.CirculationNotifyTypes = strValue;
                        goto END1;
                    }

                    // 临时修改内存中的 app.AcceptBlankRoomName 值
                    if (strName == "?AcceptBlankRoomName")
                    {
                        app.AcceptBlankRoomName = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyRegisterNoDup 值
                    if (strName == "?VerifyRegisterNoDup")
                    {
                        app.AcceptBlankRoomName = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.PatronAdditionalFroms 值
                    if (strName == "?PatronAdditionalFroms")
                    {
                        app.PatronAdditionalFroms = StringUtil.SplitList(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.PatronAdditionalFields 值
                    if (strName == "?PatronAdditionalFields")
                    {
                        app.PatronAdditionalFields = StringUtil.SplitList(strValue);
                        goto END1;
                    }

                    // 设置<valueTables>元素
                    // strValue中是下级片断定义，没有<valueTables>元素作为根。
                    if (strName == "valueTables")
                    {
                        nRet = app.SetValueTablesXml(
                            sessioninfo.LibraryCodeList,
                            strValue,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        app.Changed = true;
                        // app.ActivateManagerThread();
                        goto END1;
                    }

                    // 设置<rightsTable>元素
                    // strValue中是下级片断定义，没有<rightsTable>元素作为根。
                    if (strName == "rightsTable")
                    {
                        nRet = app.SetRightsTableXml(
    sessioninfo.LibraryCodeList,
    strValue,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 2022/3/8
                        app.SessionTable.RefreshExpandLibraryCodeList();

                        app.Changed = true;
                        // app.ActivateManagerThread();

                        goto END1;
                    }

                    // 2008/10/10 
                    // 设置<locationtypes>元素
                    // strValue中是下级片断定义，没有<locationTypes>元素作为根。
                    /*
                     *  <locationTypes>
                            <item canborrow="yes">流通库</item>
                            <item>阅览室</item>
                        </locationTypes>
                     * */
                    if (strName == "locationTypes")
                    {
#if NO
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes"); // 0.02前为locationtypes
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("locationTypes");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<locationTypes>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }
#endif
                        nRet = app.SetLocationTypesXml(
                            sessioninfo.LibraryCodeList,
                            strValue,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        app.Changed = true;
                        // app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2008/10/12 
                    // 设置<zhongcihao>元素
                    // strValue中是下级片断定义，没有<zhongcihao>元素作为根。
                    /*
                        <zhongcihao>
                            <nstable name="nstable">
                                <item prefix="marc" uri="http://dp2003.com/UNIMARC" />
                            </nstable>
                            <group name="中文书目" zhongcihaodb="种次号">
                                <database name="中文图书" leftfrom="索取类号" rightxpath="//marc:record/marc:datafield[@tag='905']/marc:subfield[@code='e']/text()" titlexpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='a']/text()" authorxpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f' or @code='g']/text()" />
                            </group>
                        </zhongcihao>
                     * */
                    if (strName == "zhongcihao")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = "分馆用户不允许修改<zhongcihao>元素定义";
                            goto ERROR1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("zhongcihao");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("zhongcihao");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<zhongcihao>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        // app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2009/2/18 
                    // 设置<callNumber>元素
                    // strValue中是下级片断定义，没有<callNumber>元素作为根。
                    /*
            <callNumber>
                <group name="中文" zhongcihaodb="种次号">
                    <location name="基藏库" />
                    <location name="流通库" />
                </group>
                <group name="英文" zhongcihaodb="新种次号库">
                    <location name="英文基藏库" />
                    <location name="英文流通库" />
                </group>
            </callNumber>             * */
                    if (strName == "callNumber")
                    {
                        // 分馆用户可以修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            // 修改 <callNumber> 元素定义。本函数专用于分馆用户。全局用户可以直接修改这个元素的 InnerXml 即可
                            nRet = app.SetCallNumberXml(
                                sessioninfo.LibraryCodeList,
                                strValue,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            // app.ActivateManagerThread();
                            goto END1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("callNumber");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("callNumber");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<callNumber>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        // app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2009/3/9 
                    // 设置<dup>元素
                    // strValue中是下级片断定义，没有<dup>元素作为根。
                    /*
         <dup>
                <project name="采购查重" comment="示例方案">
                    <database name="测试书目库" threshold="60">
                        <accessPoint name="著者" weight="50" searchStyle="" />
                        <accessPoint name="题名" weight="70" searchStyle="" />
                        <accessPoint name="索书类号" weight="10" searchStyle="" />
                    </database>
                    <database name="编目库" threshold="60">
                        <accessPoint name="著者" weight="50" searchStyle="" />
                        <accessPoint name="题名" weight="70" searchStyle="" />
                        <accessPoint name="索书类号" weight="10" searchStyle="" />
                    </database>
                </project>
                <project name="编目查重" comment="这是编目查重示例方案">
                    <database name="中文图书" threshold="100">
                        <accessPoint name="责任者" weight="50" searchStyle="" />
                        <accessPoint name="ISBN" weight="80" searchStyle="" />
                        <accessPoint name="题名" weight="20" searchStyle="" />
                    </database>
                    <database name="图书测试" threshold="100">
                        <accessPoint name="责任者" weight="50" searchStyle="" />
                        <accessPoint name="ISBN" weight="80" searchStyle="" />
                        <accessPoint name="题名" weight="20" searchStyle="" />
                    </database>
                </project>
                <default origin="中文图书" project="编目查重" />
                <default origin="图书测试" project="编目查重" />
            </dup>             * */
                    if (strName == "dup")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = "分馆用户不允许修改<dup>元素定义";
                            goto ERROR1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("dup");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("dup");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<dup>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        // app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2008/10/13 2019/5/31
                    // 设置 <script> 或 <barcodeValidation> 元素
                    // strValue中是下级片断定义，没有<script>元素作为根。
                    if (strName == "script" || strName == "barcodeValidation")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = $"分馆用户不允许修改<{strName}>元素定义";
                            goto ERROR1;
                        }

                        // 2021/10/9
                        // 先编译一次，如果报错则不兑现到 LibraryDom
                        if (strName == "script")
                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml("<script>" + strValue + "</script>");
                            }
                            catch (Exception ex)
                            {
                                strError = "脚本代码 XML 结构错误。保存失败";
                                goto ERROR1;
                            }

                            // 注意检测编译错误
                            // 初始化LibraryHostAssembly对象
                            // 必须在ReadersMonitor以前启动。否则其中用到脚本代码时会出错。2007/10/10 changed
                            // return:
                            //		-1	出错
                            //		0	脚本代码没有找到
                            //      1   成功
                            nRet = app.InitialLibraryHostAssembly(
                                new List<XmlElement> { dom.DocumentElement },
                                out strError);
                            if (nRet == -1)
                            {
                                /*
                                app.ActivateManagerThread(); // 促使尽快保存
                                app.WriteErrorLog(strError);
                                */
                                goto ERROR1;
                            }
                        }

                        bool changed = false;
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode(strName);
                        if (string.IsNullOrEmpty(strValue) == false)
                        {
                            if (root == null)
                            {
                                root = app.LibraryCfgDom.CreateElement(strName);
                                app.LibraryCfgDom.DocumentElement.AppendChild(root);
                                changed = true;
                            }
                        }
                        else
                        {
                            if (root != null)
                            {
                                root.ParentNode.RemoveChild(root);
                                changed = true;
                            }
                        }

                        try
                        {
                            root.InnerXml = ConvertCrLf(strValue);
                            changed = true;
                        }
                        catch (Exception ex)
                        {
                            strError = $"设置 <{strName}> 元素的 InnerXml 时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = changed;

#if REMOVED
                        if (strName == "script" && changed)
                        {
                            // 注意检测编译错误
                            // 初始化LibraryHostAssembly对象
                            // 必须在ReadersMonitor以前启动。否则其中用到脚本代码时会出错。2007/10/10 changed
                            // return:
                            //		-1	出错
                            //		0	脚本代码没有找到
                            //      1   成功
                            nRet = app.InitialLibraryHostAssembly(out strError);
                            if (nRet == -1)
                            {
                                app.ActivateManagerThread(); // 促使尽快保存
                                app.WriteErrorLog(strError);
                                goto ERROR1;
                            }

                        }
#endif

                        //if (changed)
                        //    app.ActivateManagerThread();
                        goto END1;
                    }

                    strError = "(strCategory为 '" + strCategory + "' 时)未知的strName值 '" + strName + "' ";
                    goto ERROR1;
                }

                // OPAC检索
                if (strCategory == "opac")
                {
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改OPAC查询参数定义";
                        goto ERROR1;
                    }

                    // 设置<virtualDatabases>元素
                    // strValue中是下级片断定义，没有<virtualDatabases>元素作为根。
                    if (strName == "databases")
                    {
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("virtualDatabases");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("virtualDatabases");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        string strOldInnerXml = root.InnerXml;
                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<virtualDatabases>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        // 重新初始化虚拟库定义
                        app.vdbs = null;
                        nRet = app.InitialVdbs(app.GetRmsChannel(sessioninfo),  // sessioninfo.Channels,
                            out strError);
                        if (nRet == -1)
                        {
                            // 2024/4/3
                            // Undo 刚才对 library.xml 的修改
                            root.InnerXml = strOldInnerXml;
                            // 2024/5/13
                            {
                                nRet = app.InitialVdbs(app.GetRmsChannel(sessioninfo),  // sessioninfo.Channels,
    out strError);
                                if (nRet != -1 && app.vdbs != null)
                                {
                                    // Undo 以后 vdbs 重新初始化成功，系统处于正常状态
                                }
                            }
                            strError = $"library.xml 中 virtualDatabases 元素内容被修改后，重新初始化 vdbs 时出错: {strError}。刚才对 virtualDatabases 元素内容的修改已被取消(注: 若要查看引发错误的内容，您不应该在 library.xml 中查看 virtualDatabase 元素内容，而要去查看 SetSystemParameter() API 的 strValue 参数值)";
                            if (app.vdbs == null)
                                this.WriteErrorLog($"*** SetSystemParameter() 过程中出现致命错误，app.vdbs 为 null 已处于不正常状态。请在日志恢复完成后，手动修正故障: {strError}");
                            goto ERROR1;
                        }

                        app.Changed = true;
                        //app.ActivateManagerThread();
                        goto END1;
                    }

                    // 设置<browseformats>元素
                    // strValue中是下级片断定义，没有<browseformats>元素作为根。
                    if (strName == "browseformats")
                    {
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("browseformats");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<browseformats>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        //app.ActivateManagerThread();

                        // TODO: 刷新OPAC界面中的浏览格式列表？

                        goto END1;
                    }

                    // 2011/2/15
                    if (strName == "serverDirectory")
                    {
                        /*
                        XmlNode node = app.LibraryCfgDom.SelectSingleNode("//opacServer");
                        if (node == null)
                        {
                            node = app.LibraryCfgDom.CreateElement("opacServer");
                            app.LibraryCfgDom.DocumentElement.AppendChild(node);
                        }

                        DomUtil.SetAttr(node, "url", strValue);
                         * */
                        app.OpacServerUrl = strValue;
                        app.Changed = true;
                        //app.ActivateManagerThread();
                        goto END1;
                    }

                    strError = "(strCategory为 '" + strCategory + "' 时)未知的strName值 '" + strName + "' ";
                    goto ERROR1;
                }

            END1:
                succeed = true;
                return nRet;
            ERROR1:
                return -1;
            }
            /*
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetSystemParameter() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);
                return -1;
            }
            */
            finally
            {
                app.UnlockForWrite();
            }
        }


    }
}
