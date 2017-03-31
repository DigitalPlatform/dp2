using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;

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
        static XmlNode CreateBiblioDatabaseNode(XmlDocument dom,
            string strDatabaseName,
            string strUsage,
            string strRole,
            string strSyntax,
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

            DomUtil.SetAttr(nodeDatabase, "entityDbName", strDatabaseName + "实体");
            DomUtil.SetAttr(nodeDatabase, "orderDbName", strDatabaseName + "订购");
            if (strUsage == "series")
                DomUtil.SetAttr(nodeDatabase, "issueDbName", strDatabaseName + "期");
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
                    true);
                biblio_dbnames.Add("中文图书");
                biblio_aliases.Add("cbook");

                CreateBiblioDatabaseNode(database_dom,
    "中文期刊",
    "series",
    "",
    "unimarc",
    true);
                biblio_dbnames.Add("中文期刊");
                biblio_aliases.Add("cseries");

                CreateBiblioDatabaseNode(database_dom,
    "西文图书",
    "book",
    "",
    "usmarc",
    true);
                biblio_dbnames.Add("西文图书");
                biblio_aliases.Add("ebook");

                CreateBiblioDatabaseNode(database_dom,
    "西文期刊",
    "series",
    "",
    "usmarc",
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
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    database_dom.OuterXml,
                    out strOutputInfo,
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

        // 创建一个书目库
        // parameters:
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
            out string strError)
        {
            strError = "";

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
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    database_dom.OuterXml,
                    out strOutputInfo,
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
    }
}
