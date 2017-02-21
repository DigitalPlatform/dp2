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
            LibraryChannel Channel,
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

            TimeSpan old_timeout = Channel.Timeout;
            Channel.Timeout = new TimeSpan(0, 10, 0);
            try
            {
                string strOutputInfo = "";
                long lRet = Channel.ManageDatabase(
                    Stop,
                    "create",
                    "",
                    database_dom.OuterXml,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                lRet = Channel.SetSystemParameter(
    Stop,
    "opac",
    "databases",
    opac_dom.DocumentElement.InnerXml,
    out strError);
                if (lRet == -1)
                    return -1;

                lRet = Channel.SetSystemParameter(
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
                Channel.Timeout = old_timeout;
            }
        }
    }
}
