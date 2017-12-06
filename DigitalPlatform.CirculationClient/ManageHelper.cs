using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;

using Ionic.Zip;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 对数据库和其他配置信息进行管理的帮助类。可用于刚安装好 dp2Library 服务器以后，创建一些缺省的书目库
    /// </summary>
    public class ManageHelper
    {
        // parameters:
        //      strUsage    book/series
        //      strRole     orderWork/orderRecommendStore/biblioSource/catalogWork/catalogTarget
        //      strSyntax   unimarc/usmarc
        //      strSubTypeList  要创建的下级数据库的类型列表。* 代表 entity,order,issue,comment
        //                      注意当 strSyntax 为 "series" 时，issue 下级库是必须创建的。而当 strSyntax 为 "book" 时，虽然要求创建 issue 也会被忽略
        static XmlNode CreateBiblioDatabaseNode(XmlDocument dom,
            string strDatabaseName,
            string strUsage,
            string strRole,
            string strSyntax,
            string strSubTypeList,
            bool bInCirculation)
        {
            XmlNode nodeDatabase = dom.CreateElement("database");
            dom.DocumentElement.AppendChild(nodeDatabase);

            // type
            DomUtil.SetAttr(nodeDatabase, "type", "biblio");

            // syntax
            DomUtil.SetAttr(nodeDatabase, "syntax", strSyntax);

            // usage
            DomUtil.SetAttr(nodeDatabase, "usage", strUsage);

            // role
            DomUtil.SetAttr(nodeDatabase, "role", strRole);

            // inCirculation
            string strInCirculation = "true";
            if (bInCirculation == true)
                strInCirculation = "true";
            else
                strInCirculation = "false";

            DomUtil.SetAttr(nodeDatabase, "inCirculation", strInCirculation);

            DomUtil.SetAttr(nodeDatabase, "name", strDatabaseName);

            if (StringUtil.IsInList("entity", strSubTypeList))
                DomUtil.SetAttr(nodeDatabase, "entityDbName", strDatabaseName + "实体");

            if (StringUtil.IsInList("order", strSubTypeList))
                DomUtil.SetAttr(nodeDatabase, "orderDbName", strDatabaseName + "订购");

            if (strUsage == "series")
            {
                DomUtil.SetAttr(nodeDatabase, "issueDbName", strDatabaseName + "期");
            }

            if (StringUtil.IsInList("comment", strSubTypeList))
                DomUtil.SetAttr(nodeDatabase, "commentDbName", strDatabaseName + "评注");

            return nodeDatabase;
        }

        // 创建读者库的定义结点
        static XmlNode CreateReaderDatabaseNode(XmlDocument dom,
            string strDatabaseName,
            string strLibraryCode,
            bool bInCirculation)
        {
            XmlNode nodeDatabase = dom.CreateElement("database");
            dom.DocumentElement.AppendChild(nodeDatabase);

            // type
            DomUtil.SetAttr(nodeDatabase, "type", "reader");

            // inCirculation
            string strInCirculation = "true";
            if (bInCirculation == true)
                strInCirculation = "true";
            else
                strInCirculation = "false";

            DomUtil.SetAttr(nodeDatabase, "inCirculation", strInCirculation);

            DomUtil.SetAttr(nodeDatabase, "name", strDatabaseName);

            DomUtil.SetAttr(nodeDatabase, "libraryCode",
                strLibraryCode);

            return nodeDatabase;
        }

        // 创建普通数据库的定义结点
        static XmlNode CreateSimpleDatabaseNode(XmlDocument dom,
    string strDatabaseName,
    string strType)
        {
            XmlNode nodeDatabase = dom.CreateElement("database");
            dom.DocumentElement.AppendChild(nodeDatabase);

            // type
            DomUtil.SetAttr(nodeDatabase, "type", strType);

            DomUtil.SetAttr(nodeDatabase, "name", strDatabaseName);

            return nodeDatabase;
        }

        // 创建修改简单数据库的定义结点
        static XmlNode ChangeSimpleDatabaseNode(XmlDocument dom,
            string strOldDatabaseName,
            string strType,
            string strNewDatabaseName)
        {
            XmlNode nodeDatabase = dom.CreateElement("database");
            dom.DocumentElement.AppendChild(nodeDatabase);

            // type
            DomUtil.SetAttr(nodeDatabase, "type", strType);

            DomUtil.SetAttr(nodeDatabase, "name", strNewDatabaseName);

            return nodeDatabase;
        }

        // 出现提示
        // return:
        //      true    继续
        //      false   放弃
        public delegate bool Delegate_prompt(string strText);

        // 创建缺省的几个数据库
        // TODO: 把过程显示在控制台
        // parameters:
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public static int CreateDefaultDatabases(
            LibraryChannel channel,
            Stop Stop,
            Delegate_prompt procPrompt,
            out string strError)
        {
            strError = "";

            string strStyle = "";

            // 创建书目库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            List<string> biblio_dbnames = new List<string>();
            List<string> biblio_aliases = new List<string>();

            // 创建书目库
            {
                // parameters:
                //      strUsage    book/series
                //      strSyntax   unimarc/usmarc
                CreateBiblioDatabaseNode(database_dom,
                    "中文图书",
                    "book",
                    "orderRecommendStore,catalogTarget",    // 2015/7/6 增加 catalogTarget
                    "unimarc",
                    "*",
                    true);
                biblio_dbnames.Add("中文图书");
                biblio_aliases.Add("cbook");

                CreateBiblioDatabaseNode(database_dom,
    "中文期刊",
    "series",
    "",
    "unimarc",
    "*",
    true);
                biblio_dbnames.Add("中文期刊");
                biblio_aliases.Add("cseries");

                CreateBiblioDatabaseNode(database_dom,
    "西文图书",
    "book",
    "",
    "usmarc",
        "*",
    true);
                biblio_dbnames.Add("西文图书");
                biblio_aliases.Add("ebook");

                CreateBiblioDatabaseNode(database_dom,
    "西文期刊",
    "series",
    "",
    "usmarc",
    "*",
    true);
                biblio_dbnames.Add("西文期刊");
                biblio_aliases.Add("eseries");
            }

            // 创建读者库
            CreateReaderDatabaseNode(database_dom,
                "读者",
                "",
                true);

            // 预约到书
            CreateSimpleDatabaseNode(database_dom,
    "预约到书",
    "arrived");

            // 违约金
            CreateSimpleDatabaseNode(database_dom,
                "违约金",
                "amerce");

            // 出版者
            CreateSimpleDatabaseNode(database_dom,
    "出版者",
    "publisher");

            // 消息
            CreateSimpleDatabaseNode(database_dom,
    "消息",
    "message");

            // 创建 OPAC 数据库的定义
            XmlDocument opac_dom = new XmlDocument();
            opac_dom.LoadXml("<virtualDatabases />");

            Debug.Assert(biblio_aliases.Count == biblio_dbnames.Count, "");

            int i = 0;
            foreach (string dbname in biblio_dbnames)
            {
                string alias = biblio_aliases[i];

                XmlElement node = opac_dom.CreateElement("database");
                opac_dom.DocumentElement.AppendChild(node);
                node.SetAttribute("name", dbname);
                node.SetAttribute("alias", alias);
                i++;
            }

            // 浏览格式
            // 插入格式节点
            XmlDocument browse_dom = new XmlDocument();
            browse_dom.LoadXml("<browseformats />");

            foreach (string dbname in biblio_dbnames)
            {
                XmlElement database = browse_dom.CreateElement("database");
                browse_dom.DocumentElement.AppendChild(database);
                database.SetAttribute("name", dbname);

                XmlElement format = browse_dom.CreateElement("format");
                database.AppendChild(format);
                format.SetAttribute("name", "详细");
                format.SetAttribute("type", "biblio");
                format.InnerXml = "<caption lang=\"zh-CN\">详细</caption><caption lang=\"en\">Detail</caption>";
            }

            // 询问是否要创建?
            if (procPrompt != null)
            {
                string strText = "创建下列书目库: " + StringUtil.MakePathList(biblio_dbnames);
                if (procPrompt(strText) == false)
                {
                    strError = "放弃创建";
                    return 0;
                }
            }

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    database_dom.OuterXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                lRet = channel.SetSystemParameter(
    Stop,
    "opac",
    "databases",
    opac_dom.DocumentElement.InnerXml,
    out strError);
                if (lRet == -1)
                    return -1;

                lRet = channel.SetSystemParameter(
    Stop,
    "opac",
    "browseformats",
    browse_dom.DocumentElement.InnerXml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        // 包装后的版本，少了 strRequestXml 这个参数
        public static int CreateBiblioDatabase(
    LibraryChannel channel,
    Stop Stop,
    string strBiblioDbName,
    string strUsage,
    string strSyntax,
    string strSubTypeList,
    string strStyle,
    out string strError)
        {
            return CreateBiblioDatabase(
    channel,
    Stop,
    strBiblioDbName,
    strUsage,
    strSyntax,
    strSubTypeList,
    strStyle,
    out string strRequestXml,
    out strError);
        }

        // 创建一个书目库
        // parameters:
        //      strRequestXml   返回对 dp2library 发出的请求 XML
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public static int CreateBiblioDatabase(
            LibraryChannel channel,
            Stop Stop,
            string strBiblioDbName,
            string strUsage,
            string strSyntax,
            string strSubTypeList,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建书目库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            List<string> biblio_dbnames = new List<string>();
            //List<string> biblio_aliases = new List<string>();

            // 创建书目库
            {
                // parameters:
                //      strUsage    book/series
                //      strSyntax   unimarc/usmarc
                CreateBiblioDatabaseNode(database_dom,
                    strBiblioDbName,
                    strUsage,   // "book",
                    "", // "orderRecommendStore,catalogTarget",    // 2015/7/6 增加 catalogTarget
                    strSyntax,  // "unimarc",
                    strSubTypeList,
                    true);
                biblio_dbnames.Add(strBiblioDbName);
                // biblio_aliases.Add("cbook");
            }

            // 创建 OPAC 数据库的定义
            XmlDocument opac_dom = new XmlDocument();
            opac_dom.LoadXml("<virtualDatabases />");

            // Debug.Assert(biblio_aliases.Count == biblio_dbnames.Count, "");

            int i = 0;
            foreach (string dbname in biblio_dbnames)
            {
                //string alias = biblio_aliases[i];

                XmlElement node = opac_dom.CreateElement("database");
                opac_dom.DocumentElement.AppendChild(node);
                node.SetAttribute("name", dbname);
                //node.SetAttribute("alias", alias);
                i++;
            }

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                strRequestXml = database_dom.OuterXml;
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    strRequestXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        public static int CreateReaderDatabase(LibraryChannel channel,
    Stop Stop,
    string strDbName,
    string strLibraryCode,
    bool bInCirculation,
    string strStyle,
    out string strError)
        {
            return CreateReaderDatabase(channel,
    Stop,
    strDbName,
    strLibraryCode,
    bInCirculation,
    strStyle,
    out string strRequestXml,
    out strError);
        }

        // 创建一个读者库
        // parameters:
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public static int CreateReaderDatabase(LibraryChannel channel,
            Stop Stop,
            string strDbName,
            string strLibraryCode,
            bool bInCirculation,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            {
                // 创建读者库
                CreateReaderDatabaseNode(database_dom,
                    strDbName,
                    strLibraryCode,
                    bInCirculation);
            }

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                strRequestXml = database_dom.OuterXml;
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    strRequestXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        public static int CreateSimpleDatabase(
    LibraryChannel channel,
    Stop Stop,
    string strDbName,
    string strType,
    string strStyle,
    out string strError)
        {
            return CreateSimpleDatabase(
    channel,
    Stop,
    strDbName,
    strType,
    strStyle,
    out string strRequestXml,
    out strError);
        }

        // 创建一个简单库
        // parameters:
        // return:
        //      -1  出错
        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
        //      1   成功创建
        public static int CreateSimpleDatabase(
            LibraryChannel channel,
            Stop Stop,
            string strDbName,
            string strType,
            string strStyle,
            out string strRequestXml,
            out string strError)
        {
            strError = "";
            strRequestXml = "";

            // 创建库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            {
                CreateSimpleDatabaseNode(database_dom,
                    strDbName,
                    strType);
            }

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                strRequestXml = database_dom.OuterXml;
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    strRequestXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        public class LocationItem
        {
            public string LibraryCode { get; set; }
            public string Room { get; set; }
            public bool CanBorrow { get; set; }
            public bool ItemBarcodeNullable { get; set; }

            public LocationItem(string strLibraryCode,
                string strRoom,
                bool bCanBorrow,
                bool bItemBarcodeNullable)
            {
                this.LibraryCode = LibraryCode;
                this.Room = strRoom;
                this.CanBorrow = bCanBorrow;
                this.ItemBarcodeNullable = bItemBarcodeNullable;
            }
        }

        /*
    <locationTypes>
        <item canborrow="no" itemBarcodeNullable="yes">保存本库</item>
        <item canborrow="no" itemBarcodeNullable="yes">阅览室</item>
        <item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
        <item canborrow="yes" itemBarcodeNullable="yes">测试库</item>
        <library code="海淀分馆">
            <item canborrow="no" itemBarcodeNullable="no">流通库</item>
            <item canborrow="yes" itemBarcodeNullable="no">班级书架</item>
            <item canborrow="yes" itemBarcodeNullable="no"></item>
        </library>
    </locationTypes>
         * */
        // 为系统添加新的馆藏地定义
        // parameters:
        //      strAction   add/remove
        public static int AddLocationTypes(
            LibraryChannel channel,
            Stop stop,
            string strAction,
            List<LocationItem> items,
            out string strError)
        {
            strError = "";

            string strOutputInfo = "";

            long lRet = channel.GetSystemParameter(
    stop,
    "circulation",
    "locationTypes",
    out strOutputInfo,
    out strError);
            if (lRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                strError = "fragment XML 装入 XmlDocumentFragment 时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            if (strAction == "add")
            {
                foreach (LocationItem item in items)
                {
                    // 删除可能存在的同名定义
                    RemoveLocationItem(dom, item);

                    XmlElement new_item = dom.CreateElement("item");
                    new_item.SetAttribute("canborrow", item.CanBorrow ? "yes" : "no");
                    new_item.SetAttribute("itemBarcodeNullable", item.ItemBarcodeNullable ? "yes" : "no");
                    if (string.IsNullOrEmpty(item.Room) == false)
                        new_item.InnerText = item.Room;

                    if (string.IsNullOrEmpty(item.LibraryCode) == false)
                    {
                        XmlElement library = null;
                        library = dom.CreateElement("library");
                        dom.DocumentElement.AppendChild(library);
                        library.SetAttribute("code", item.LibraryCode);
                        library.AppendChild(new_item);
                    }
                    else
                        dom.DocumentElement.AppendChild(new_item);
                }
            }
            else if (strAction == "remove")
            {
                foreach (LocationItem item in items)
                {
                    RemoveLocationItem(dom, item);
                }
            }
            else
            {
                strError = "未知的 strAction '" + strAction + "'";
                return -1;
            }

            lRet = channel.SetSystemParameter(
    stop,
    "circulation",
    "locationTypes",
    dom.DocumentElement.InnerXml,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        static void RemoveLocationItem(XmlDocument dom, LocationItem item)
        {
            string strXPath = "library[@code='" + item.LibraryCode + "']/item[text()='" + item.Room + "']";
            if (string.IsNullOrEmpty(item.LibraryCode))
                strXPath = "item[text()='" + item.Room + "']";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes(strXPath);
            foreach (XmlElement node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }
        }

        /*
    <callNumber>
        <group name="中图法" classType="中图法" qufenhaoType="GCAT,Cutter-Sanborn Three-Figure" zhongcihaodb="" callNumberStyle="索取类号+区分号">
            <location name="" />
            <location name="保存本库" />
            <location name="流通库" />
        </group>
        <group name="种次号" classType="中图法" qufenhaoType="种次号" zhongcihaodb="" callNumberStyle="索取类号+区分号">
            <location name="阅览室" />
            <location name="测试库" />
        </group>
    </callNumber>
         * */
        // 修改排架体系定义。具体来说就是增补一个 group 元素片段
        public static int ChangeCallNumberDef(
            LibraryChannel channel,
            Stop stop,
            string strGroupFragment,
            out string strOldXml,
            out string strError)
        {
            strError = "";
            strOldXml = "";

            string strArrangementXml = "";
            long lRet = channel.GetSystemParameter(
    stop,
    "circulation",
    "callNumber",
    out strArrangementXml,
    out strError);
            if (lRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<callNumber />");

            {
                XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = strArrangementXml;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                dom.DocumentElement.AppendChild(fragment);
            }

            strOldXml = dom.DocumentElement.OuterXml;

            {
                XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = strGroupFragment;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML 装入 XmlDocumentFragment 时出错: " + ex.Message;
                    return -1;
                }

                dom.DocumentElement.AppendChild(fragment);
            }

            lRet = channel.SetSystemParameter(
    stop,
    "circulation",
    "callNumber",
    dom.DocumentElement.InnerXml,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // 恢复以前保存的排架体系定义
        public static int RestoreCallNumberDef(
    LibraryChannel channel,
    Stop stop,
    string strOldXml,
    out string strError)
        {
            strError = "";

            XmlDocument dom = null;

            if (string.IsNullOrEmpty(strOldXml) == false)
            {
                dom = new XmlDocument();
                dom.LoadXml(strOldXml);
            }

            long lRet = channel.SetSystemParameter(
    stop,
    "circulation",
    "callNumber",
    dom == null ? "" : dom.DocumentElement.InnerXml,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // 修改一个简单库
        // parameters:
        // return:
        //      -1  出错
        //      0   没有找到源数据库定义
        //      1   成功修改
        public static int ChangeSimpleDatabase(
            LibraryChannel channel,
            Stop Stop,
            string strDbName,
            string strType,
            string strNewDbName,
            string strStyle,
            out string strError)
        {
            strError = "";

            // 创建库的定义
            XmlDocument database_dom = new XmlDocument();
            database_dom.LoadXml("<root />");

            {
                ChangeSimpleDatabaseNode(database_dom,
                    strDbName,
                    strType,
                    strNewDbName);
            }

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                long lRet = channel.ManageDatabase(
                    Stop,
                    "change",
                    strDbName,
                    database_dom.OuterXml,
                    strStyle,
                    out string strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }
        }

        public class DatabaseInfo
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Syntax { get; set; }

            public string Usage { get; set; }
            public string Role { get; set; }
            public bool InCirculation { get; set; }

            public string EntityDbName { get; set; }
            public string IssueDbName { get; set; }
            public string OrderDbName { get; set; }
            public string CommentDbName { get; set; }

            public string Replication { get; set; }
            public string UnionCatalogStyle { get; set; }

            public string LibraryCode { get; set; }


            public DatabaseInfo()
            {

            }

            public DatabaseInfo(XmlElement database)
            {
                this.Name = database.GetAttribute("name");
                this.Type = database.GetAttribute("type");

                if (this.Type == "reader")
                {
                    this.InCirculation = DomUtil.GetBooleanParam(database, "inCirculation", true);

                    this.LibraryCode = database.GetAttribute("libraryCode");
                }

                if (this.Type == "biblio")
                {
                    this.Syntax = database.GetAttribute("syntax");
                    if (string.IsNullOrEmpty(this.Syntax))
                        this.Syntax = "unimarc";

                    this.EntityDbName = database.GetAttribute("entityDbName");

                    this.OrderDbName = database.GetAttribute("orderDbName");

                    this.IssueDbName = database.GetAttribute("issueDbName");

                    this.CommentDbName = database.GetAttribute("commentDbName");

                    this.Usage = database.GetAttribute("usage");
                    if (string.IsNullOrEmpty(this.Usage))
                        this.Usage = "book";

                    this.Role = database.GetAttribute("role");

                    this.InCirculation = DomUtil.GetBooleanParam(database, "inCirculation", true);

                    this.Replication = database.GetAttribute("replication");
                    this.UnionCatalogStyle = database.GetAttribute("unionCatalogStyle");
                }
            }

            // 和日志 XML 记录中的 databases/database 元素比较，看信息是否完全一致
            public bool CompareToOperLogDatabaseElement(XmlElement database,
                out string strInfo)
            {
                strInfo = "";

                DatabaseInfo other = new DatabaseInfo(database);

                if (CompareString(other.Name, this.Name) == false)
                {
                    strInfo = string.Format("name 属性值({0})和 this.Name({1}) 不一致", other.Name, this.Name);
                    return false;
                }

                if (CompareString(other.Type, this.Type) == false)
                {
                    strInfo = string.Format("type 属性值({0})和 this.Type({1}) 不一致", other.Type, this.Type);
                    return false;
                }

                if (this.Type == "reader")
                {
                    if (other.InCirculation != this.InCirculation)
                    {
                        strInfo = string.Format("inCirculation 属性值({0})和 this.InCirculation({1}) 不一致", other.InCirculation, this.InCirculation);
                        return false;
                    }

                    if (CompareString(other.LibraryCode, this.LibraryCode) == false)
                    {
                        strInfo = string.Format("libraryCode 属性值({0})和 this.LibraryCode({1}) 不一致", other.LibraryCode, this.LibraryCode);
                        return false;
                    }
                }

                if (this.Type == "biblio")
                {
                    if (CompareString(other.Syntax, this.Syntax) == false)
                    {
                        strInfo = string.Format("syntax 属性值({0})和 this.Syntax({1}) 不一致", other.Syntax, this.Syntax);
                        return false;
                    }

                    if (CompareString(other.EntityDbName, this.EntityDbName) == false)
                    {
                        strInfo = string.Format("entityDbName 属性值({0})和 this.EntityDbName({1}) 不一致", other.EntityDbName, this.EntityDbName);
                        return false;
                    }

                    if (CompareString(other.OrderDbName, this.OrderDbName) == false)
                    {
                        strInfo = string.Format("orderDbName 属性值({0})和 this.OrderDbName({1}) 不一致", other.OrderDbName, this.OrderDbName);
                        return false;
                    }

                    if (CompareString(other.IssueDbName, this.IssueDbName) == false)
                    {
                        strInfo = string.Format("issueDbName 属性值({0})和 this.IssueDbName({1}) 不一致", other.IssueDbName, this.IssueDbName);
                        return false;
                    }

                    if (CompareString(other.CommentDbName, this.CommentDbName) == false)
                    {
                        strInfo = string.Format("commentDbName 属性值({0})和 this.CommentDbName({1}) 不一致", other.CommentDbName, this.CommentDbName);
                        return false;
                    }

                    // Usage 可以通过是否具备期库来判断
#if NO
                    if (CompareString(other.Usage, this.Usage) == false)
                    {
                        strInfo = string.Format("usage 属性值({0})和 this.Usage({1}) 不一致", other.Usage, this.Usage);
                        return false;
                    }
#endif

                    if (CompareString(other.Role, this.Role) == false)
                    {
                        strInfo = string.Format("role 属性值({0})和 this.Role({1}) 不一致", other.Role, this.Role);
                        return false;
                    }

                    if (other.InCirculation != this.InCirculation)
                    {
                        strInfo = string.Format("inCirculation 属性值({0})和 this.InCirculation({1}) 不一致", other.InCirculation, this.InCirculation);
                        return false;
                    }

                    if (CompareString(other.Replication, this.Replication) == false)
                    {
                        strInfo = string.Format("replication 属性值({0})和 this.Replication({1}) 不一致", other.Replication, this.Replication);
                        return false;
                    }

                    if (CompareString(other.UnionCatalogStyle, this.UnionCatalogStyle) == false)
                    {
                        strInfo = string.Format("unionCatalogStyle 属性值({0})和 this.UnionCatalogStyle({1}) 不一致", other.UnionCatalogStyle, this.UnionCatalogStyle);
                        return false;
                    }
                }
                return true;
            }

            // return:
            //      true    一致
            //      false   不一致
            static bool CompareString(string s1, string s2)
            {
                if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                    return true;
                if (s1 == s2)
                    return true;
                return false;
            }
        }

        public static int GetDatabaseInfo(
            Stop stop,
            LibraryChannel channel,
            out List<DatabaseInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<DatabaseInfo>();

            long lRet = channel.ManageDatabase(
                stop,
                "getinfo",
                "",
                "",
                "",
                out string AllDatabaseInfoXml,
                out strError);
            if (lRet == -1)
                return -1;

            if (String.IsNullOrEmpty(AllDatabaseInfoXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            foreach (XmlElement database in nodes)
            {
                DatabaseInfo info = new DatabaseInfo(database);
                infos.Add(info);
#if NO
                DatabaseInfo info = new DatabaseInfo();
                info.Type = strType;
                info.Name = strName;
                infos.Add(info);

                if (strType == "biblio")
                {
                    string strSyntax = DomUtil.GetAttr(node, "syntax");
                    if (String.IsNullOrEmpty(strSyntax) == true)
                        strSyntax = "unimarc";

                    info.Syntax = strSyntax;
                }
#endif
            }

            return 1;
        }

        // return:
        //      -2  日志记录不存在
        //      -1  出错
        //      >=0 附件文件的长度
        public static long GetOneOperLog(
            LibraryChannel channel,
            Stop stop,
            string strDate,
            long lIndex,
            string strAttachmentFileName,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            OperLogItemLoader loader = new OperLogItemLoader
            {
                Channel = channel,
                Stop = stop,
                Date = strDate,
                Range = lIndex.ToString()
            };

            foreach (OperLogItem item in loader)
            {
                if (item.AttachmentLength == 0)
                    return 0;
                strXml = item.Xml;
                // return:
                //      -1  出错
                //      0   没有找到日志记录
                //      >0  附件总长度
                long lRet = channel.DownloadOperlogAttachment(
                    stop,   // stop,
                    strDate + ".log",
                    lIndex,
                    -1, // lHint,
                    strAttachmentFileName,
                    out long lHintNext,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return -2;
                return lRet;
            }
            strError = "日志记录 '" + strDate + "' (Offset=" + lIndex + ") 不存在";
            return -2;
        }

        /*
<root>
  <operation>manageDatabase</operation>
  <action>createDatabase</action>
  <databases>
    <database type="biblio" syntax="unimarc" usage="series" role="" inCirculation="true" name="test_biblio" issueDbName="test_biblio期" commentDbName="test_biblio评注" />
  </databases>
  <operator>supervisor</operator>
  <operTime>Tue, 05 Dec 2017 10:13:42 +0800</operTime>
  <clientAddress via="net.pipe://localhost/dp2library/XE">localhost</clientAddress>
  <version>1.06</version>
</root>
<root>
  <operation>manageDatabase</operation>
  <action>createDatabase</action>
  <databases>
    <database type="reader" inCirculation="true" name="test_reader" libraryCode="_测试分馆" />
  </databases>
  <operator>supervisor</operator>
  <operTime>Tue, 05 Dec 2017 09:47:15 +0800</operTime>
  <clientAddress via="net.pipe://localhost/dp2library/XE">localhost</clientAddress>
  <version>1.06</version>
</root>
<root>
  <operation>manageDatabase</operation>
  <action>createDatabase</action>
  <databases>
    <database type="message" name="test_message" />
  </databases>
  <operator>supervisor</operator>
  <operTime>Tue, 05 Dec 2017 10:14:51 +0800</operTime>
  <clientAddress via="net.pipe://localhost/dp2library/XE">localhost</clientAddress>
  <version>1.06</version>
</root>
* */
        // 根据日志记录 XML，比较当前数据库状态
        // parameters:
        //      strFunction creating/deleting 之一
        //      dbnames 顺便从日志 XML 中提取数据库名返回。注意，当函数返回值不是 0 时，列表中内容可能不全
        // return:
        //      -1  出错
        //      0   比较发现完全一致
        //      1   比较中发现(至少一处)不一致
        public static int CompareOperLog(Stop stop,
     LibraryChannel channel,
     string strXml,
     string strFunction,
     out List<string> dbnames,
     out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            // 获得当前的所有数据库信息
            List<DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo> infos = null;
            int nRet = ManageHelper.GetDatabaseInfo(
        stop,
        channel,
        out infos,
        out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            XmlNodeList databases = dom.DocumentElement.SelectNodes("databases/database");
            foreach (XmlElement database in databases)
            {
                string strType = database.GetAttribute("type");
                if (string.IsNullOrEmpty(strType))
                {
                    strError = "日志记录中 databases/database/@type 属性值为空，这是不允许的";
                    return -1;
                }

                string strName = database.GetAttribute("name");
                if (string.IsNullOrEmpty(strName))
                {
                    strError = "日志记录中 databases/database/@name 属性值为空，这是不允许的";
                    return -1;
                }

                dbnames.Add(strName);

                List<DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo> results =
                    infos.FindAll((DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo info) =>
                    {
                        if (info.Type == strType
                        && info.Name == strName)
                            return true;
                        return false;
                    });
                if (results.Count == 0)
                {
                    strError = "当前从服务器获得的全部数据库信息中，没有发现类型为 '" + strType + "' 名字为 '" + strName + "' 的数据库信息";
                    return -1;
                }
                if (results.Count > 1)
                {
                    strError = "当前从服务器获得的全部数据库信息中，发现类型为 '" + strType + "' 名字为 '" + strName + "' 的数据库信息多于一个 (" + results.Count + ")";
                    return -1;
                }
                // 核对
                DatabaseInfo result = results[0];
                if (result.CompareToOperLogDatabaseElement(database, out strError) == false)
                {
                    strError = "日志记录中的 databases/database 元素信息和从服务器返回的信息不一致: " + strError;
                    return 1;
                }
            }
            return 0;
        }

        // TODO: 传入从操作日志记录 XML 中提取的数据库名列表，要求 .zip 中的数据库名和这个列表一致
        // 根据 .zip 定义文件，比较当前数据库状态
        // parameters:
        //      strFunction creating/deleting 之一
        // return:
        //      -1  出错
        //      0   比较发现完全一致
        //      1   比较中发现(至少一处)不一致
        public static int CompareDefinition(
     Stop stop,
     LibraryChannel channel,
     string strDbDefFileName,
     string strTempDir,
     string strFunction,
     out string strError)
        {
            strError = "";
            int nRet = 0;

            // 展开压缩文件
            if (stop != null)
                stop.SetMessage("正在展开压缩文件 " + strDbDefFileName);
            try
            {
                using (ZipFile zip = ZipFile.Read(strDbDefFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        // Debug.WriteLine(e.ToString());
                        e.Extract(strTempDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "展开压缩文件 '" + strDbDefFileName + "' 到目录 '" + strTempDir + "' 时出现异常: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            DirectoryInfo di = new DirectoryInfo(strTempDir);

            DirectoryInfo[] dis = di.GetDirectories();

            int i = 0;
            foreach (DirectoryInfo info in dis)
            {
                // 跳过 '_datadir'
                if (info.Name == "_datadir")
                    continue;

                string strCfgsDir = Path.Combine(info.FullName, "cfgs");
                string strDatabaseName = info.Name;
                // 数据库是否已经存在？
                // return:
                //      -1  error
                //      0   not exist
                //      1   exist
                //      2   其他类型的同名对象已经存在
                nRet = IsDatabaseExist(
                    channel,
                    strDatabaseName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (strFunction == "creating")
                {
                    if (nRet != 1)
                    {
                        strError = "数据库 '" + strDatabaseName + "' 在服务器端不存在，这不符合创建后应有的状态";
                        return 1;
                    }
                }
                else if (strFunction == "deleting")
                {
                    if (nRet != 0)
                    {
                        strError = "数据库 '" + strDatabaseName + "' 在服务器端已经存在，这不符合删除后应有的状态";
                        return 1;
                    }
                    continue;
                }
                else
                {
                    strError = "未知的 strFunction '" + strFunction + "'";
                    return -1;
                }

                // 比较每个配置文件
                // 比较一个本地目录里面的每个配置文件，和 dp2library 一端
                // return:
                //      -1  出错
                //      0   比较发现完全一致
                //      1   比较中发现(至少一处)不一致
                nRet = CompareCfgs(
        stop,
        channel,
        strDatabaseName,
        strCfgsDir,
        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return 1;

                i++;
            }

            return 0;
        }

        // 比较一个本地目录里面的每个配置文件，和 dp2library 一端
        // parameters:
        //      strCfgsDir  存储有若干配置文件的本地目录
        // return:
        //      -1  出错
        //      0   比较发现完全一致
        //      1   比较中发现(至少一处)不一致
        public static int CompareCfgs(
Stop stop,
LibraryChannel channel,
string strDbName,
string strCfgsDir,
out string strError)
        {
            strError = "";

            DirectoryInfo di = new DirectoryInfo(strCfgsDir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo info in fis)
            {
                string strLocalFilePath = info.FullName;
                string strTempFileName = Path.GetTempFileName();
                try
                {
                    string strRemotePath = strDbName + "/cfgs/" + info.Name;
                    long lRet = channel.GetRes(stop,
                        strRemotePath,
                        strTempFileName,
                        "data,content",
                        out string strMetaData,
                        out byte[] output_timestamp,
                        out string strOutputResPath,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    // 比较两个文本文件的内容是否一致
                    // return:
                    //      -1  出错
                    //      0   两个文件内容一样
                    //      1   两个文件内容不一样
                    int nRet = FileUtil.CompareTwoTextFile(strLocalFilePath, strTempFileName, out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet != 0)
                    {
                        strError = "数据库配置文件 '" + strRemotePath + "' 和本地文件比较发现不一致";
                        return 1;
                    }

                }
                finally
                {
                    File.Delete(strTempFileName);
                }
            }

            return 0;
        }

        // 数据库是否已经存在？
        // return:
        //      -1  error
        //      0   not exist
        //      1   exist
        //      2   其他类型的同名对象已经存在
        public static int IsDatabaseExist(
            LibraryChannel channel,
            string strDatabaseName,
            out string strError)
        {
            strError = "";

            DirItemLoader loader = new DirItemLoader(channel,
                null,
                "", // strDatabaseName, 
                "");
            foreach (ResInfoItem item in loader)
            {
                if (item.Name == strDatabaseName)
                {
                    if (item.Type == dp2ResTree.RESTYPE_DB)
                    {
                        strError = "数据库 " + strDatabaseName + " 已经存在。";
                        return 1;
                    }
                    else
                    {
                        strError = "和数据库 " + strDatabaseName + " 同名的非数据库类型对象已经存在。";
                        return 2;
                    }
                }
            }

            return 0;
        }

    }


}
