using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System.Reflection;

namespace DigitalPlatform.LibraryServer.Reporting
{
    /// <summary>
    /// 同步和复制功能
    /// </summary>
    public class Replication
    {

        // 初始化
        public int Initialize(
            LibraryChannel channel,
            out string strError)
        {
            strError = "";

            int nRet = CheckVersion(channel,
out strError);
            if (nRet == -1)
                return -1;

            nRet = GetDbFromInfos(channel,
    out strError);
            if (nRet == -1)
                return -1;

            nRet = GetAllDatabaseInfo(channel,
    out strError);
            if (nRet == -1)
                return -1;

            nRet = InitialBiblioDbProperties(channel,
    out strError);
            if (nRet == -1)
                return -1;
            return 0;
        }

        #region 初始化各种数据结构

        public string ServerVersion { get; set; }
        public string ServerUID { get; set; }

        public int CheckVersion(LibraryChannel channel,
    out string strError)
        {
            strError = "";

            long lRet = channel.GetVersion(null,
out string strVersion,
out string strUID,
out strError);
            if (lRet == -1)
            {
                strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                return -1;
            }

            this.ServerUID = strUID;

            if (string.IsNullOrEmpty(strVersion) == true)
                strVersion = "2.0";

            this.ServerVersion = strVersion;
            return 1;
        }

        /// <summary>
        /// 书目库检索路径信息集合
        /// </summary>
        public BiblioDbFromInfo[] BiblioDbFromInfos = null;   // 书目库检索路径信息

        public string Lang = "zh";

        // 初始化各种检索途径信息
        public int GetDbFromInfos(LibraryChannel channel,
            out string strError)
        {
            strError = "";
            // 获得书目库的检索途径
            BiblioDbFromInfo[] infos = null;

            long lRet = channel.ListDbFroms(null,
                "biblio",
                this.Lang,
                out infos,
                out strError);
            if (lRet == -1)
            {
                strError = "针对服务器 " + channel.Url + " 列出书目库检索途径过程发生错误：" + strError;
                return -1;
            }

            this.BiblioDbFromInfos = infos;

#if NO
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.6") >= 0)
            {
                infos = null;
                lRet = channel.ListDbFroms(Stop,
"authority",
this.Lang,
out infos,
out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出规范库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.AuthorityDbFromInfos = infos;
            }

            // 获得读者库的检索途径
            infos = null;
            lRet = channel.ListDbFroms(Stop,
"reader",
this.Lang,
out infos,
out strError);
            if (lRet == -1)
            {
                strError = "针对服务器 " + channel.Url + " 列出读者库检索途径过程发生错误：" + strError;
                goto ERROR1;
            }

            if (infos != null && this.BiblioDbFromInfos != null
                && infos.Length > 0 && this.BiblioDbFromInfos.Length > 0
                && infos[0].Caption == this.BiblioDbFromInfos[0].Caption)
            {
                // 如果第一个元素的caption一样，则说明GetDbFroms API是旧版本的，不支持获取读者库的检索途径功能
                this.ReaderDbFromInfos = null;
            }
            else
            {
                this.ReaderDbFromInfos = infos;
            }

            if (StringUtil.CompareVersion(this.ServerVersion, "2.11") >= 0)
            {
                // 获得实体库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "item",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出实体库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }
                this.ItemDbFromInfos = infos;

                // 获得期库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "issue",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出期库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }
                this.IssueDbFromInfos = infos;

                // 获得订购库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "order",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出订购库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }
                this.OrderDbFromInfos = infos;

                // 获得评注库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "comment",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出评注库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }
                this.CommentDbFromInfos = infos;
            }

            if (StringUtil.CompareVersion(this.ServerVersion, "2.17") >= 0)
            {
                // 获得发票库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "invoice",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出发票库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.InvoiceDbFromInfos = infos;

                // 获得违约金库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "amerce",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出违约金库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.AmerceDbFromInfos = infos;

            }

            if (StringUtil.CompareVersion(this.ServerVersion, "2.47") >= 0)
            {
                // 获得预约到书库的检索途径
                infos = null;
                lRet = channel.ListDbFroms(Stop,
    "arrived",
    this.Lang,
    out infos,
    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 列出预约到书库检索途径过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.ArrivedDbFromInfos = infos;
            }

            // 需要检查一下Caption是否有重复(但是style不同)的，如果有，需要修改Caption名
            this.CanonicalizeBiblioFromValues();
#endif
            return 0;
        }

        /// <summary>
        /// 书目库属性集合
        /// </summary>
        public static List<BiblioDbProperty> BiblioDbProperties = null;

        public int InitialBiblioDbProperties(LibraryChannel channel,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            BiblioDbProperties = new List<BiblioDbProperty>();
            // this.AuthorityDbProperties = new List<BiblioDbProperty>();

            if (this.AllDatabaseDom == null)
                return 0;

            {
                XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='biblio']");
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strType = DomUtil.GetAttr(node, "type");
                    // string strRole = DomUtil.GetAttr(node, "role");
                    // string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                    BiblioDbProperty property = new BiblioDbProperty();
                    BiblioDbProperties.Add(property);
                    property.DbName = DomUtil.GetAttr(node, "name");
                    property.ItemDbName = DomUtil.GetAttr(node, "entityDbName");
                    property.Syntax = DomUtil.GetAttr(node, "syntax");
                    property.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    property.OrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    property.CommentDbName = DomUtil.GetAttr(node, "commentDbName");
                    property.Role = DomUtil.GetAttr(node, "role");

                    bool bValue = true;
                    nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    property.InCirculation = bValue;
                }
            }

            /*
            {
                XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='authority']");
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strType = DomUtil.GetAttr(node, "type");

                    BiblioDbProperty property = new BiblioDbProperty();
                    this.AuthorityDbProperties.Add(property);
                    property.DbName = DomUtil.GetAttr(node, "name");
                    property.Syntax = DomUtil.GetAttr(node, "syntax");
                    property.Usage = DomUtil.GetAttr(node, "usage");
                }
            }
            */

            // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);

            return 0;
        }


        /// <summary>
        /// 表示当前全部数据库信息的 XmlDocument 对象
        /// </summary>
        public XmlDocument AllDatabaseDom = null;

        // 获取全部数据库定义
        public int GetAllDatabaseInfo(LibraryChannel channel,
            out string strError)
        {
            strError = "";

            long lRet = 0;

            this.AllDatabaseDom = null;

            lRet = channel.ManageDatabase(
null,
"getinfo",
"",
"",
"",
out string strValue,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ErrorCode.AccessDenied)
                {
                }

                strError = "针对服务器 " + channel.Url + " 获得全部数据库定义过程发生错误：" + strError;
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strValue);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            this.AllDatabaseDom = dom;

            return 0;
        }

#if NO

        // 记忆书目库的分类号 style 列表
        int MemoryClassFromStyles(XmlElement root,
            out string strError)
        {
            strError = "";
            int nRet = GetClassFromStyles(out List<BiblioDbFromInfo> styles,
            out strError);
            if (nRet == -1)
                return -1;
            if (styles.Count == 0)
            {
                strError = "书目库尚未配置分类号检索点";
                return 0;
            }

            XmlElement container = root.SelectSingleNode("classStyles") as XmlElement;
            if (container == null)
            {
                container = root.OwnerDocument.CreateElement("classStyles");
                root.AppendChild(container);
            }
            else
                container.RemoveAll();

            foreach (BiblioDbFromInfo info in styles)
            {
                XmlElement style_element = root.OwnerDocument.CreateElement("style");
                container.AppendChild(style_element);
                style_element.SetAttribute("style", info.Style);
                style_element.SetAttribute("caption", info.Caption);
            }

            return 1;
        }

        // 获得所有分类号检索途径 style
        internal int GetClassFromStyles(
            out List<BiblioDbFromInfo> styles,
            out string strError)
        {
            strError = "";
            styles = new List<BiblioDbFromInfo>();

            foreach (BiblioDbFromInfo info in this.BiblioDbFromInfos)
            {
                if (StringUtil.IsInList("__class", info.Style) == true)
                {
                    string strStyle = GetPureStyle(info.Style);
                    if (string.IsNullOrEmpty(strStyle) == true)
                    {
                        strError = "检索途径 " + info.Caption + " 的 style 值 '" + info.Style + "' 其中应该有至少一个不带 '_' 前缀的子串";
                        return -1;
                    }
                    BiblioDbFromInfo style = new BiblioDbFromInfo();
                    style.Caption = info.Caption;
                    style.Style = strStyle;
                    styles.Add(style);
                }
            }

            return 0;
        }

        // 从计划文件中获得所有分类号检索途径 style
        internal int GetClassFromStyles(
            XmlElement root,
            out List<string> styles,
            out string strError)
        {
            strError = "";
            styles = new List<string>();

            XmlNodeList nodes = root.SelectNodes("classStyles/style");
            foreach (XmlElement element in nodes)
            {
                styles.Add(element.GetAttribute("style"));
            }
            return 0;
        }


        // 获得不是 _ 和 __ 打头的 style 值
        static string GetPureStyle(string strText)
        {
            List<string> results = new List<string>();
            string[] parts = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                if (s[0] == '_')
                    continue;
                results.Add(s);
            }

            return StringUtil.MakePathList(results);
        }

#endif

        #endregion

        // 获得全部数据库定义
        public static int GetAllDatabaseInfo(
    LibraryChannel channel,
    out XmlDocument dom,
    out string strError)
        {
            strError = "";
            long lRet = 0;

            dom = null;

            lRet = channel.ManageDatabase(
    null,
    "getinfo",
    "",
    "",
    "",
    out string strValue,
    out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ErrorCode.AccessDenied)
                {
                }

                strError = "针对服务器 " + channel.Url + " 获得全部数据库定义过程发生错误：" + strError;
                return -1;
            }

            dom = new XmlDocument();
            try
            {
                dom.LoadXml(strValue);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        static List<string> GetBiblioDbNames(XmlDocument database_dom)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = database_dom.DocumentElement.SelectNodes("database[@type='biblio']");
            foreach (XmlElement node in nodes)
            {
                string strName = node.GetAttribute("name");

                if (string.IsNullOrEmpty(strName) == false)
                    results.Add(strName);
            }
            return results;
        }

        static List<string> GetItemDbNames(XmlDocument database_dom)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = database_dom.DocumentElement.SelectNodes("database[@type='biblio']");
            foreach (XmlElement node in nodes)
            {
                string strName = node.GetAttribute("name");
                string strType = node.GetAttribute("type");

                string itemDbName = node.GetAttribute("entityDbName");
                string syntax = node.GetAttribute("syntax");
                string issueDbName = node.GetAttribute("issueDbName");
                string orderDbName = node.GetAttribute("orderDbName");
                string commentDbName = node.GetAttribute("commentDbName");
                string role = node.GetAttribute("role");

                if (string.IsNullOrEmpty(itemDbName) == false)
                    results.Add(itemDbName);
#if NO
                bool bValue = true;
                nRet = DomUtil.GetBooleanParam(node,
                    "inCirculation",
                    true,
                    out bValue,
                    out strError);
                property.InCirculation = bValue;
#endif
            }
            return results;
        }

        string GetReaderDbLibraryCode(string strReaderDbName)
        {
            XmlNodeList nodes = this.AllDatabaseDom.DocumentElement.SelectNodes("database[@type='reader']");
            foreach (XmlElement node in nodes)
            {
                string dbName = node.GetAttribute("name");
                string libraryCode = node.GetAttribute("libraryCode");
                if (dbName == strReaderDbName)
                    return libraryCode;
            }
            return null;
        }

        static List<string> GetReaderDbNames(XmlDocument database_dom)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = database_dom.DocumentElement.SelectNodes("database[@type='reader']");
            foreach (XmlElement node in nodes)
            {
                string dbName = node.GetAttribute("name");
                string libraryCode = node.GetAttribute("libraryCode");

#if NO
                bool bValue = true;
                nRet = DomUtil.GetBooleanParam(node,
                                "inCirculation",
                                true,
                                out bValue,
                                out strError);
                property.InCirculation = bValue;
#endif
                if (string.IsNullOrEmpty(dbName) == false)
                    results.Add(dbName);
            }
            return results;
        }

        // 本地库的版本号
        // 0.01 (2014/4/30) 第一个版本
        // 0.02 (2014/5/6) item 表 增加了两个 index: item_itemrecpath_index 和 item_biliorecpath_index
        // 0.03 (2014/5/29) item 表增加了 borrower borrowtime borrowperiod returningtime 等字段
        // 0.04 (2014/6/2) breakpoint 文件根元素下增加了 classStyles 元素
        // 0.05 (2014/6/12) item 表增加了 price unit 列； operlogamerce 表的 price 列分开为 price 和 unit 列
        // 0.06 (2014/6/16) operlogxxx 表中增加了 subno 字段
        // 0.07 (2014/6/19) operlogitem 表增加了 itembarcode 字段
        // 0.08 (2014/11/6) reader 表增加了 state 字段 
        // 0.09 (2015/7/14) 增加了 operlogpassgate 和 operloggetres 表
        // 0.10 (2016/5/5) 给每个 operlogxxx 表增加了 librarycode 字段
        // 0.11 (2016/12/8) 以前版本 operlogamerce 中 action 为 undo 的行，price 字段内容都为空，会导致 472 报表中统计出来的实收金额偏大，这个版本修正了这个 bug
        static string _local_version = "0.11";

        public delegate void Delegate_showMessage(string text);

        // TODO: 最好把第一次初始化本地 sql 表的动作也纳入 XML 文件中，这样做单项任务的时候，就不会毁掉其他的表
        // 创建批处理计划
        // 根元素的 state 属性， 值为 first 表示正在进行首次创建，尚未完成; daily 表示已经创建完，进入每日同步阶段
        // parameters:
        //      strTypeList 类型列表。为 * user item reader biblio operlog accesslog 的组合
        public int BuildFirstPlan(string strTypeList,
            LibraryChannel channel,
            Delegate_showMessage func_showMessage,
            out XmlDocument task_dom,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            task_dom = new XmlDocument();
            task_dom.LoadXml("<root />");

            // 开始处理时的日期
            string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

            task_dom.DocumentElement.SetAttribute("version", _local_version);

            task_dom.DocumentElement.SetAttribute("state", "first");  // 表示首次创建尚未完成

            // 记载首次创建的结束时间点
            task_dom.DocumentElement.SetAttribute("end_date", strEndDate);

            // *** 创建用户表
            if (strTypeList == "*"
                || StringUtil.IsInList("user", strTypeList) == true)
            {

                XmlNode node = task_dom.CreateElement("user");
                task_dom.DocumentElement.AppendChild(node);
            }

            // *** 创建 item 表
            if (strTypeList == "*"
                || StringUtil.IsInList("item", strTypeList) == true)
            {
                // 获得全部实体库名
                List<string> item_dbnames = GetItemDbNames(this.AllDatabaseDom);

                // 获得每个实体库的尺寸
                foreach (string strItemDbName in item_dbnames)
                {
                    func_showMessage?.Invoke("正在计划任务 检索 " + strItemDbName + " ...");

                    // 此处检索仅获得命中数即可
                    lRet = channel.SearchItem(null,
        strItemDbName,
        "", // 
        -1,
        "__id",
        "left",
        "zh",
        null,   // strResultSetName
        "",    // strSearchStyle
        "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
        out strError);
                    if (lRet == -1)
                        return -1;

                    XmlElement node = task_dom.CreateElement("database");
                    task_dom.DocumentElement.AppendChild(node);

                    node.SetAttribute("name", strItemDbName);
                    node.SetAttribute("type", "item");
                    node.SetAttribute("count", lRet.ToString());
                }
            }

            // *** 创建 reader 表
            if (strTypeList == "*"
                || StringUtil.IsInList("reader", strTypeList) == true)
            {
                // 获得全部读者库名
                List<string> reader_dbnames = GetReaderDbNames(this.AllDatabaseDom);

                // 
                foreach (string strReaderDbName in reader_dbnames)
                {
                    func_showMessage?.Invoke("正在计划任务 检索 " + strReaderDbName + " ...");
                    // 此处检索仅获得命中数即可
                    lRet = channel.SearchReader(null,
        strReaderDbName,
        "", // 
        -1,
        "__id",
        "left",
        "zh",
        null,   // strResultSetName
        "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
        out strError);
                    if (lRet == -1)
                        return -1;

                    XmlElement node = task_dom.CreateElement("database");
                    task_dom.DocumentElement.AppendChild(node);

                    node.SetAttribute("name", strReaderDbName);
                    node.SetAttribute("type", "reader");
                    node.SetAttribute("count", lRet.ToString());
                }
            }

            // *** 创建 biblio 表
            // *** 创建 class 表
            if (strTypeList == "*"
                || StringUtil.IsInList("biblio", strTypeList) == true)
            {
                // 获得全部书目库名
                List<string> biblio_dbnames = GetBiblioDbNames(this.AllDatabaseDom);

#if NO

                    // 获得所有分类号检索途径 style
                    List<string> styles = new List<string>();
                    nRet = GetClassFromStyles(out styles,
                        out strError);
                    if (nRet == -1)
                        return -1;
#endif

#if NO
                // 记忆书目库的分类号 style 列表
                nRet = MemoryClassFromStyles(task_dom.DocumentElement,
        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    /*
                    DialogResult result = DialogResult.No;
                    string strText = strError;
                    this.Invoke((Action)(() =>
                    {
                        result = MessageBox.Show(this,
 strText + "\r\n\r\n建议先中断处理，配置好书目库的分类号检索点再重新创建本地存储。如果此时继续处理，则会无法同步分类号信息，以后也无法创建和分类号有关的报表。\r\n\r\n是否继续处理?",
 "ReportForm",
 MessageBoxButtons.YesNo,
 MessageBoxIcon.Question,
 MessageBoxDefaultButton.Button2);
                    }));
                    if (result == System.Windows.Forms.DialogResult.No)
                        return -1;  // 
                        */
                    return -1;
                }

                // 从计划文件中获得所有分类号检索途径 style
                List<string> styles = new List<string>();
                nRet = GetClassFromStyles(
                    task_dom.DocumentElement,
                    out styles,
                    out strError);
                if (nRet == -1)
                    return -1;

#endif

                //
                foreach (string strBiblioDbName in biblio_dbnames)
                {
                    func_showMessage?.Invoke("正在计划任务 检索 " + strBiblioDbName + " ...");
                    string strQueryXml = "";
                    // 此处检索仅获得命中数即可
                    lRet = channel.SearchBiblio(null,
                        strBiblioDbName,
                        "", // 
                        -1,
                        "recid",     // "__id",
                        "left",
                        "zh",
                        null,   // strResultSetName
                        "",    // strSearchStyle
                        "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                        "",
                        out strQueryXml,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    XmlElement node = task_dom.CreateElement("database");
                    task_dom.DocumentElement.AppendChild(node);

                    node.SetAttribute("name", strBiblioDbName);
                    node.SetAttribute("type", "biblio");
                    node.SetAttribute("count", lRet.ToString());

#if NO
                    foreach (string strStyle in styles)
                    {
                        func_showMessage?.Invoke("正在计划任务 检索 " + strBiblioDbName + " " + strStyle + " ...");
                        // 此处检索仅获得命中数即可
                        lRet = channel.SearchBiblio(null,
                            strBiblioDbName,
                            "", // 
                            -1,
                            strStyle,     // "__id",
                            "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                            "zh",
                            null,   // strResultSetName
                            "",    // strSearchStyle
                            "keyid", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                            "",
                            out strQueryXml,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ErrorCode.FromNotFound)
                                continue;
                            return -1;
                        }
                        string strClassTableName = "class_" + strStyle;

                        XmlElement class_node = task_dom.CreateElement("class");
                        node.AppendChild(class_node);

                        class_node.SetAttribute("from_style", strStyle);
                        class_node.SetAttribute("class_table_name", strClassTableName);
                        class_node.SetAttribute("count", lRet.ToString());
                    }

#endif
                }
            }



            // *** 创建日志表
            if (strTypeList == "*"
            || StringUtil.IsInList("operlog", strTypeList) == true)
            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetFirstOperLogDate(
                    channel,
                    LogType.OperLog | LogType.AccessLog,
                    out string strFirstDate,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得第一个操作日志文件日期时出错: " + strError;
                    return -1;
                }

                // 获得日志文件中记录的总数
                // parameters:
                //      strDate 日志文件的日期，8 字符
                // return:
                //      -2  此类型的日志在 dp2library 端尚未启用
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                long lCount = OperLogLoader.GetOperLogCount(
                    null,
                    channel,
                    strEndDate,
                    LogType.OperLog,
                    out strError);
                if (lCount < 0)
                    return -1;

                task_dom.DocumentElement.SetAttribute("index", lCount.ToString());

                if (nRet == 1)
                {
                    // 记载第一个日志文件日期
                    task_dom.DocumentElement.SetAttribute(
                        "first_operlog_date",
                        strFirstDate);

                    /*
                    Program.MainForm.AppInfo.SetString(GetReportSection(),
                        "daily_report_end_date",
                        strFirstDate);
                    Program.MainForm.AppInfo.Save();   // 为防止程序中途崩溃丢失记忆，这里预先保存一下
                    */

                    XmlElement node = task_dom.CreateElement("operlog");
                    task_dom.DocumentElement.AppendChild(node);

                    node.SetAttribute("start_date", strFirstDate);  // "20060101"
                    node.SetAttribute("end_date", strEndDate + ":0-" + (lCount - 1).ToString());
                }
            }

            // *** 创建访问日志表
            if (strTypeList == "*"
                || StringUtil.IsInList("accesslog", strTypeList) == true)
            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetFirstOperLogDate(
                    channel,
                    LogType.AccessLog,
                    out string strFirstDate,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得第一个访问日志文件日期时出错: " + strError;
                    return -1;
                }

                // 获得日志文件中记录的总数
                // parameters:
                //      strDate 日志文件的日期，8 字符
                // return:
                //      -2  此类型的日志在 dp2library 端尚未启用
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                long lCount = OperLogLoader.GetOperLogCount(
                    null,
                    channel,
                    strEndDate,
                    LogType.AccessLog,
                    out strError);
                if (lCount == -1)
                    return -1;

                if (nRet == 1 && lCount >= 0)
                {
                    // 记载第一个访问日志文件日期
                    task_dom.DocumentElement.SetAttribute(
                        "first_accesslog_date",
                        strFirstDate);

                    XmlElement node = task_dom.CreateElement("accesslog");
                    task_dom.DocumentElement.AppendChild(node);

                    node.SetAttribute("start_date", strFirstDate);  // "20060101"
                    node.SetAttribute("end_date", strEndDate + ":0-" + (lCount - 1).ToString());
                }
            }

            return 0;
        }

        // 获得第一个(实有的)日志文件日期
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetFirstOperLogDate(
            LibraryChannel channel,
            LogType logType,
            out string strFirstDate,
            out string strError)
        {
            strFirstDate = "";
            strError = "";

            DigitalPlatform.LibraryClient.localhost.OperLogInfo[] records = null;

            List<string> dates = new List<string>();
            List<string> styles = new List<string>();
            if ((logType & LogType.OperLog) != 0)
                styles.Add("getfilenames");
            if ((logType & LogType.AccessLog) != 0)
                styles.Add("getfilenames,accessLog");
            if (styles.Count == 0)
            {
                strError = "logStyle 参数值中至少要包含一种类型";
                return -1;
            }

            foreach (string style in styles)
            {
                // 获得日志
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围，本次调用无效
                long lRet = channel.GetOperLogs(
                    null,
                    "",
                    0,
                    -1,
                    1,
                    style,  // "getfilenames",
                    "", // strFilter
                    out records,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    continue;

                if (records == null || records.Length < 1)
                {
                    strError = "records error";
                    return -1;
                }

                if (string.IsNullOrEmpty(records[0].Xml) == true
                    || records[0].Xml.Length < 8)
                {
                    strError = "records[0].Xml error";
                    return -1;
                }

                dates.Add(records[0].Xml.Substring(0, 8));
            }

            if (dates.Count == 0)
                return 0;

            // 取较小的一个
            if (dates.Count > 1)
                dates.Sort();
            strFirstDate = dates[0];
            return 1;
        }

        ProgressEstimate _estimate = new ProgressEstimate();

        // 执行首次创建本地存储的计划
        // parameters:
        //      task_dom    存储了计划信息的 XMlDocument 对象。执行后，里面的信息会记载了断点信息等。如果完全完成，则保存前可以仅仅留下结束点信息
        public int RunFirstPlan(
            // DatabaseConfig config,
            LibraryChannel channel,
            ref XmlDocument task_dom,
            Delegate_showMessage func_showMessage,
            CancellationToken token,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            var context = new LibraryContext();
            try
            {
                // 初始化各种表，除了 operlogXXX 表以外
                string strInitilized = DomUtil.GetAttr(task_dom.DocumentElement,
                    "initial_tables");
                if (strInitilized != "finish")
                {
                    // stop.SetMessage("正在删除残余的数据库文件 ...");
                    /*
                    // 删除以前遗留的数据库文件
                    string strDatabaseFile = Path.Combine(GetBaseDirectory(), "operlog.bin");
                    if (File.Exists(strDatabaseFile) == true)
                    {
                        File.Delete(strDatabaseFile);
                    }

                    stop.SetMessage("正在初始化本地数据库 ...");
                    nRet = ItemLine.CreateItemTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = ReaderLine.CreateReaderTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = BiblioLine.CreateBiblioTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    {
                        List<string> styles = new List<string>();

                        // 获得所有分类号检索途径 style
                        nRet = GetClassFromStyles(
                            task_dom.DocumentElement,
                            out styles,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        foreach (string strStyle in styles)
                        {
                            nRet = ClassLine.CreateClassTable(
                                this._connectionString,
                                "class_" + strStyle,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                    }
                    */

                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }

                    DomUtil.SetAttr(task_dom.DocumentElement,
                        "initial_tables", "finish");
                }

                // 先累计总记录数，以便设置进度条
                long lTotalCount = 0;
                XmlNodeList nodes = task_dom.DocumentElement.SelectNodes("database/class | database");
                foreach (XmlElement node in nodes)
                {
                    string strState = node.GetAttribute("state");
                    if (strState == "finish")
                        continue;

                    nRet = DomUtil.GetIntegerParam(node,
                        "count",
                        0,
                        out long lCount,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    lTotalCount += lCount;
                }

                // stop.SetProgressRange(0, lTotalCount * 2); // 第一阶段，占据进度条一半
                long lProgress = 0;

                _estimate.SetRange(0, lTotalCount * 2);
                _estimate.StartEstimate();

                foreach (XmlNode node in task_dom.DocumentElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    if (node.Name == "database")
                    {
                        string strDbName = DomUtil.GetAttr(node, "name");
                        string strType = DomUtil.GetAttr(node, "type");
                        string strState = DomUtil.GetAttr(node, "state");

                        nRet = DomUtil.GetIntegerParam(node,
                            "index",
                            0,
                            out long lIndex,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        nRet = DomUtil.GetIntegerParam(node,
                            "count",
                            0,
                            out long lCurrentCount,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (strType == "item" && strState != "finish")
                        {
                            try
                            {
                                nRet = BuildItemRecords(
                                    ref context,
                                    channel,
            strDbName,
            lCurrentCount,
            func_showMessage,
            token,
            ref lProgress,
            ref lIndex,
            out strError);
                            }
                            catch
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                throw;
                            }
                            if (nRet == -1)
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }

                        if (strType == "reader" && strState != "finish")
                        {
                            try
                            {
                                nRet = BuildReaderRecords(
                                    ref context,
                                    channel,
            strDbName,
            lCurrentCount,
            func_showMessage,
            token,
            ref lProgress,
            ref lIndex,
            out strError);
                            }
                            catch
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                throw;
                            }
                            if (nRet == -1)
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }

                        if (strType == "biblio")
                        {
                            if (strState != "finish")
                            {
                                try
                                {
                                    nRet = BuildBiblioRecords(
                                    ref context,
                                    channel,
                strDbName,
                lCurrentCount,
            func_showMessage,
            token,
            ref lProgress,
            ref lIndex,
                out strError);
                                }
                                catch
                                {
                                    DomUtil.SetAttr(node, "index", lIndex.ToString());
                                    throw;
                                }
                                if (nRet == -1)
                                {
                                    DomUtil.SetAttr(node, "index", lIndex.ToString());
                                    return -1;
                                }
                                DomUtil.SetAttr(node, "state", "finish");
                            }

#if NO
                            XmlNodeList class_nodes = node.SelectNodes("class");
                            foreach (XmlNode class_node in class_nodes)
                            {
                                string strFromStyle = DomUtil.GetAttr(class_node, "from_style");
                                string strClassTableName = DomUtil.GetAttr(class_node, "class_table_name");
                                strState = DomUtil.GetAttr(class_node, "state");
                                lIndex = 0;
                                nRet = DomUtil.GetIntegerParam(class_node,
                                    "index",
                                    0,
                                    out lIndex,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                //
                                nRet = DomUtil.GetIntegerParam(class_node,
    "count",
    0,
    out lCurrentCount,
    out strError);
                                if (nRet == -1)
                                    return -1;

                                if (strState != "finish")
                                {
                                    try
                                    {
                                        nRet = BuildClassRecords(
                                            strDbName,
                                            strFromStyle,
                                            strClassTableName,
                                            lCurrentCount,
                                            ref lProgress,
                                            ref lIndex,
                                            out strError);
                                    }
                                    catch
                                    {
                                        DomUtil.SetAttr(class_node, "index", lIndex.ToString());
                                        throw;
                                    }
                                    if (nRet == -1)
                                    {
                                        DomUtil.SetAttr(class_node, "index", lIndex.ToString());
                                        return -1;
                                    }
                                    DomUtil.SetAttr(class_node, "state", "finish");
                                }
                            }

#endif
                        }

                    }


                    if (node.Name == "user")
                    {
                        string strState = DomUtil.GetAttr(node, "state");

                        if (strState != "finish")
                        {
                            nRet = DoCreateUserTable(
                                ref context,
                                channel,
                                func_showMessage,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            DomUtil.SetAttr(node, "state", "finish");
                        }
                    }

                    if (node.Name == "operlog")
                    {
                        string strTableInitilized = DomUtil.GetAttr(node,
    "initial_tables");

                        string strStartDate = DomUtil.GetAttr(node, "start_date");
                        string strEndDate = DomUtil.GetAttr(node, "end_date");
                        string strState = DomUtil.GetAttr(node, "state");

                        if (string.IsNullOrEmpty(strStartDate) == true)
                        {
                            // strStartDate = "20060101";
                            strError = "start_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }
                        if (string.IsNullOrEmpty(strEndDate) == true)
                        {
                            // strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);
                            strError = "end_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }

                        if (strTableInitilized != "finish")
                        {
                            // stop.SetMessage("正在初始化本地数据库的日志表 ...");
                            /*
                            nRet = CreateOperLogTables(out strError);
                            if (nRet == -1)
                                return -1;
                                */
                            DomUtil.SetAttr(node,
                                "initial_tables", "finish");
                        }

                        if (strState != "finish")
                        {
                            // TODO: 中断时断点记载
                            // TODO: 进度条应该是重新设置的
                            nRet = DoCreateOperLogTable(
                                ref context,
                                channel,
                                -1,
                                strStartDate,
                                strEndDate,
                                LogType.OperLog,
                                false,
                                func_showMessage,
                                token,
                                out string strLastDate,
                                out long lLastIndex,
                                out strError);
                            if (nRet == -1)
                            {
                                if (string.IsNullOrEmpty(strLastDate) == false)
                                    DomUtil.SetAttr(node, "start_date", strLastDate + ":" + lLastIndex.ToString() + "-");
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }
                    }

                    if (node.Name == "accesslog")
                    {
                        string strTableInitilized = DomUtil.GetAttr(node,
    "initial_tables");

                        string strStartDate = DomUtil.GetAttr(node, "start_date");
                        string strEndDate = DomUtil.GetAttr(node, "end_date");
                        string strState = DomUtil.GetAttr(node, "state");

                        if (string.IsNullOrEmpty(strStartDate) == true)
                        {
                            strError = "start_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }
                        if (string.IsNullOrEmpty(strEndDate) == true)
                        {
                            strError = "end_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }

                        if (strTableInitilized != "finish")
                        {
                            // 前面应该已经初始化过了
                            DomUtil.SetAttr(node,
                                "initial_tables", "finish");
                        }

                        if (strState != "finish")
                        {
                            string strLastDate = "";
                            long lLastIndex = 0;
                            // TODO: 中断时断点记载
                            // TODO: 进度条应该是重新设置的
                            nRet = DoCreateOperLogTable(
                                ref context,
                                channel,
                                -1,
                                strStartDate,
                                strEndDate,
                                LogType.AccessLog,
                                false,
                                func_showMessage,
                                token,
                                out strLastDate,
                                out lLastIndex,
                                out strError);
                            if (nRet == -1)
                            {
                                if (string.IsNullOrEmpty(strLastDate) == false)
                                    DomUtil.SetAttr(node, "start_date", strLastDate + ":" + lLastIndex.ToString() + "-");
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }
                    }
                }

                // TODO: 全部完成后，需要在 task_dom 中清除不必要的信息
                DomUtil.SetAttr(task_dom.DocumentElement,
                    "state", "daily");  // 表示首次创建已经完成，进入每日同步阶段
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
        }

        public static string GetResultSetName()
        {
            return "#" + Guid.NewGuid().ToString();
        }

        public void DeleteResultSet(
            LibraryChannel channel,
            string strResultSetName)
        {
            // 删除全局结果集对象
            // 管理结果集
            // parameters:
            //      strAction   share/remove 分别表示共享为全局结果集对象/删除全局结果集对象
            long lRet = channel.ManageSearchResult(
                null,
                "remove",
                "",
                strResultSetName,
                out string strError);
            /*
            if (lRet == -1)
            {
                AutoCloseMessageBox.Show(
                    this,
                    "删除全局结果集 '" + strResultSetName + "' 时出错" + strError,
                    10 * 1000,
                    "");
            }
            */
        }

        // 创建 user 表
        int DoCreateUserTable(
            ref LibraryContext context,
            LibraryChannel channel,
            Delegate_showMessage func_showMessage,
            out string strError)
        {
            strError = "";

            func_showMessage?.Invoke("创建用户表 ...");

            int nStart = 0;
            for (; ; )
            {
                long lRet = channel.GetUser(
                    null,
                    "list",
                    "",
                    nStart,
                    -1,
                    out UserInfo[] users,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                {
                    strError = "不存在用户信息。";
                    return 0;   // not found
                }

                Debug.Assert(users != null, "");

                List<object> lines = new List<object>();
                foreach (var info in users)
                {
                    User line = new User
                    {
                        ID = info.UserName,
                        Rights = info.Rights,
                        LibraryCodeList = "," + info.LibraryCode + ","
                    };
                    lines.Add(line);
                }

                // 插入一批用户记录
                SaveChanges(ref context, lines);

                nStart += users.Length;
                if (nStart >= lRet)
                    break;
            }

            return 0;
        }

        int BuildItemRecords(
    ref LibraryContext context,
    LibraryChannel channel,
    string strItemDbNameParam,
    long lOldCount,
    Delegate_showMessage func_showMessage,
    CancellationToken token,
    ref long lProgress,
    ref long lIndex,
    out string strError)
        {
            // 实体库名 --> 书目库名
            Hashtable dbname_table = new Hashtable();

            return BuildRecords(ref context,
                channel,
                (c, r) =>
                {
                    long lRet = c.SearchItem(null,
    strItemDbNameParam,
    "", // (lIndex+1).ToString() + "-", // 
    -1,
    "__id",
    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
    "zh",
    r,
    "",    // strSearchStyle
    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
    out string error);
                    return new NormalResult { Value = (int)lRet, ErrorInfo = error };
                },
                (searchresult) =>
                {
                    // 检查事项状态
                    if (searchresult.Cols == null)
                        throw new Exception($"浏览事项 Cols 为空: (recpath='{searchresult.Path}'  {DumpResultItem(searchresult)}");
                    if (searchresult.Cols.Length < 12)
                        return null;

                    Item line = new Item();
                    line.ItemRecPath = searchresult.Path;
                    line.ItemBarcode = searchresult.Cols[0];
                    // 2016/9/26
                    if (string.IsNullOrEmpty(line.ItemBarcode))
                        line.ItemBarcode = "@refID:" + searchresult.Cols[11];

                    line.Location = Item.CanonicalizeLocationString(searchresult.Cols[1]);
                    line.AccessNo = searchresult.Cols[2];

                    line.State = searchresult.Cols[4];

                    line.CreateTime = GetLocalTime(searchresult.Cols[5]);

                    line.Borrower = searchresult.Cols[6];
                    line.BorrowTime = GetLocalTime(searchresult.Cols[7]);
                    line.BorrowPeriod = searchresult.Cols[8];
                    // line.ReturningTime = ItemLine.GetLocalTime(searchresult.Cols[9]);

                    int nRet = 0;
                    if (line.BorrowTime != DateTime.MinValue)
                    {
                        // parameters:
                        //      strBorrowTime   借阅起点时间。u 格式
                        //      strReturningTime    返回应还时间。 u 格式
                        nRet = BuildReturingTimeString(line.BorrowTime,
            line.BorrowPeriod,
            out DateTime returningTime,
            out string error);
                        if (nRet == -1)
                        {
                            line.ReturningTime = DateTime.MinValue;
                        }
                        else
                            line.ReturningTime = returningTime;
                    }
                    else
                        line.ReturningTime = DateTime.MinValue;

                    string strPrice = searchresult.Cols[10];
                    nRet = ParsePriceString(strPrice,
            out decimal value,
            out string strUnit,
            out string error1);
                    if (nRet == -1)
                    {
                        line.Price = 0;
                        line.Unit = "";
                    }
                    else
                    {
                        line.Price = value;
                        line.Unit = strUnit;
                    }

                    string strItemDbName = GetDbName(searchresult.Path);
                    string strBiblioDbName = (string)dbname_table[strItemDbName];
                    if (string.IsNullOrEmpty(strBiblioDbName) == true)
                    {
                        strBiblioDbName = GetBiblioDbNameFromItemDbName(strItemDbName);
                        dbname_table[strItemDbName] = strBiblioDbName;
                    }

                    string strBiblioRecPath = strBiblioDbName + "/" + searchresult.Cols[3];

                    line.BiblioRecPath = strBiblioRecPath;
                    return line;
                },
                null,
                func_showMessage,
                "id,cols,format:@coldef:*/barcode|*/location|*/accessNo|*/parent|*/state|*/operations/operation[@name='create']/@time|*/borrower|*/borrowDate|*/borrowPeriod|*/returningDate|*/price|*/refID",
            lOldCount,
            token,
                ref lProgress,
                ref lIndex,
                out strError);
        }



        public static void ParseCalendarName(string strName,
    out string strLibraryCode,
    out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }

        delegate NormalResult Delegate_search(LibraryChannel channel, string resultsetName);
        delegate object Delegate_buildItem(
            DigitalPlatform.LibraryClient.localhost.Record record);
        delegate void Delegate_beforeSave(List<object> items);

        // BuildRecords() 每批记录个数
        const int BATCH_SIZE = 100;

        // parameters:
        //      lIndex  [in] 起点 index
        //              [out] 返回中断位置的 index
        int BuildRecords(
            ref LibraryContext context,
            LibraryChannel channel,
            Delegate_search func_search,
            Delegate_buildItem func_buildItem,
            Delegate_beforeSave func_beforeSave,
            Delegate_showMessage func_showMessage,
            // string strItemDbNameParam,
            string strStyle,
            long lOldCount,
            CancellationToken token,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            strError = "";
            lProgress += lIndex;

            // 采用全局结果集
            string strResultSetName = GetResultSetName();
            try
            {
                channel.Timeout = TimeSpan.FromMinutes(5); // 2018/5/10
                var result = func_search(channel, strResultSetName);
                strError = result.ErrorInfo;
                if (result.Value == -1)
                    return -1;
                if (result.Value == 0)
                    return 0;
                long lHitCount = result.Value;

                /*
                long lRet = channel.SearchItem(null,
                    strItemDbNameParam,
                    "", // (lIndex+1).ToString() + "-", // 
                    -1,
                    "__id",
                    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                    "zh",
                    strResultSetName,
                    "",    // strSearchStyle
                    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;
                */

                AdjustProgressRange(lOldCount, lHitCount);

#if NO
                string strStyle = "";

                {
                    strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/accessNo|*/parent|*/state|*/operations/operation[@name='create']/@time|*/borrower|*/borrowDate|*/borrowPeriod|*/returningDate|*/price|*/refID";
                }
#endif


                ResultSetLoader loader = new ResultSetLoader(channel,
null,
strResultSetName,
strStyle,
"zh");
                loader.Start = lIndex;

                // loader.Prompt += this.Loader_Prompt;

                // 处理浏览结果
                long i = lIndex;
                List<object> lines = new List<object>();
                foreach (DigitalPlatform.LibraryClient.localhost.Record searchresult in loader)
                {
                    // DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];
                    if (token.IsCancellationRequested)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    if (i < lIndex)
                        goto CONTINUE;

                    object line = func_buildItem(searchresult);
                    if (line == null)
                        goto CONTINUE;

                    lines.Add(line);
                    // context.Add(line);

                    if (lines.Count >= BATCH_SIZE)
                    {
                        func_beforeSave?.Invoke(lines);
                        SaveChanges(ref context, lines);

                        lIndex = i + 1;

                        lines.Clear();

                        func_showMessage?.Invoke($"正在创建 {searchresult.Path} ...");
                    }

                CONTINUE:
                    i++;
                }

                if (lines.Count > 0)
                {
                    func_beforeSave?.Invoke(lines);
                    SaveChanges(ref context, lines);
                }
            }
            finally
            {
                this.DeleteResultSet(channel, strResultSetName);
            }

            return 0;
        }

        public int SaveChangesOneByOne(ref LibraryContext context,
    List<object> lines)
        {
            int count = 0;
            foreach (var line in lines)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.Add(line);
                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        // Number = 1062 重复的 PrimaryKey

                        throw ex;
                    }
                }
            }


            context.Dispose();
            context = new LibraryContext();
            // DetachAll(context);

            return count;
        }

        public void SaveChanges(ref LibraryContext context,
            List<object> lines)
        {
            Exception exception = null;
            // 自动重试两次
            for (int i = 0; i < 2; i++)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.AddRange(lines);
                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Rollback() 本身也可能再次抛异常
                        dbContextTransaction.Rollback(); //Required according to MSDN article 
                        // TODO: 写入错误日志
                        exception = ex;
                    }
                }
            }

            if (exception != null)
            {
                SaveChangesOneByOne(ref context, lines);
                // throw exception;
            }

            context.Dispose();
            context = new LibraryContext();
            // DetachAll(context);
        }

        public void DetachAll(LibraryContext context)
        {
            foreach (EntityEntry entityEntry in context.ChangeTracker.Entries().ToArray())
            {
                if (entityEntry.Entity != null)
                {
                    entityEntry.State = EntityState.Detached;
                }
            }
        }

        // 复制读者记录
        int BuildReaderRecords(
    ref LibraryContext context,
    LibraryChannel channel,
            string strReaderDbNameParam,
            long lOldCount,
    Delegate_showMessage func_showMessage,
    CancellationToken token,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            Hashtable librarycode_table = new Hashtable();

            return BuildRecords(ref context,
                channel,
                (c, r) =>
                {
                    long lRet = c.SearchReader(null,
    strReaderDbNameParam,
    "", // (lIndex + 1).ToString() + "-", // 
    -1,
    "__id",
    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
    "zh",
    r,
    // "",    // strSearchStyle
    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
    out string error);
                    return new NormalResult { Value = (int)lRet, ErrorInfo = error };
                },
                (searchresult) =>
                {
                    // 检查事项状态
                    if (searchresult.Cols == null)
                        throw new Exception($"浏览事项 Cols 为空: (recpath='{searchresult.Path}'  {DumpResultItem(searchresult)}");

                    if (searchresult.Cols.Length < 5)
                        return null;

                    Patron line = new Patron();
                    line.RecPath = searchresult.Path;
                    line.Barcode = searchresult.Cols[0];
                    line.Department = searchresult.Cols[1];
                    line.ReaderType = searchresult.Cols[2];
                    line.Name = searchresult.Cols[3];
                    line.State = searchresult.Cols[4];

                    string strReaderDbName = GetDbName(searchresult.Path);
                    string strLibraryCode = (string)librarycode_table[strReaderDbName];
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                    {
                        strLibraryCode = GetReaderDbLibraryCode(strReaderDbName);
                        librarycode_table[strReaderDbName] = strLibraryCode;
                    }
                    line.LibraryCode = strLibraryCode;
                    return line;
                },
                null,
                func_showMessage,
                "id,cols,format:@coldef:*/barcode|*/department|*/readerType|*/name|*/state",
            lOldCount,
            token,
                ref lProgress,
                ref lIndex,
                out strError);
        }

        int BuildBiblioRecords(
ref LibraryContext context,
LibraryChannel channel,
    string strBiblioDbNameParam,
    long lOldCount,
    Delegate_showMessage func_showMessage,
    CancellationToken token,
    ref long lProgress,
    ref long lIndex,
    out string strError)
        {
            List<string> biblio_recpaths = new List<string>();

            return BuildRecords(ref context,
                channel,
                (c, r) =>
                {
                    long lRet = c.SearchBiblio(null,
    strBiblioDbNameParam,
    "", // (lIndex + 1).ToString() + "-", // 
    -1,
    "recid",     // "__id",
    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
    "zh",
    r,
    "",    // strSearchStyle
    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
    "",
    out string strQueryXml,
    out string error);
                    return new NormalResult { Value = (int)lRet, ErrorInfo = error };
                },
                (searchresult) =>
                {
                    Biblio line = new Biblio();
                    line.RecPath = searchresult.Path;
                    line.Xml = searchresult.RecordBody.Xml;
                    // 创建检索点和 Summary
                    line.Create(line.Xml, line.RecPath);
                    biblio_recpaths.Add(searchresult.Path);
                    return line;
                },
                (lines) =>
                {
#if NO
                    {
                        Debug.Assert(biblio_recpaths.Count == lines.Count, "");

                        TimeSpan old_timeout = channel.Timeout;
                        channel.Timeout = new TimeSpan(0, 0, 30);
                        try
                        {
                            // 获得书目摘要
                            BiblioLoader loader = new BiblioLoader();
                            loader.Channel = channel;
                            loader.Stop = null;
                            loader.Format = "summary";
                            loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                            loader.RecPaths = biblio_recpaths;

                            //loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                            //loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                            int i = 0;
                            foreach (BiblioItem item in loader)
                            {
                                // this.Progress.SetMessage("正在加入 " + (i + 1).ToString() + "/" + targetLeft.Count.ToString() + " 个书目摘要，可能需要较长时间 ...");

                                Biblio line = lines[i] as Biblio;
                                if (string.IsNullOrEmpty(item.Content) == false)
                                {
                                    if (item.Content.Length > 4000)
                                        line.Summary = item.Content.Substring(0, 4000);
                                    else
                                        line.Summary = item.Content;
                                }

                                i++;
                            }
                            biblio_recpaths.Clear();
                        }
                        finally
                        {
                            channel.Timeout = old_timeout;
                        }
                    }
#endif
                },
                func_showMessage,
                "id,xml",
                lOldCount,
                token,
                ref lProgress,
                ref lIndex,
                out strError);
        }

        static string DumpResultItem(DigitalPlatform.LibraryClient.localhost.Record searchresult)
        {
            if (searchresult.Cols == null)
                return "path=" + searchresult.Path + ";cols=[null]";
            return "path=" + searchresult.Path + ";cols(" + searchresult.Cols.Length.ToString() + ")=" + string.Join("|", searchresult.Cols);
        }

        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }

        public string GetBiblioDbNameFromItemDbName(string strItemDbName)
        {
            // 2008/11/28 
            // 实体库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (BiblioDbProperties != null)
            {
                for (int i = 0; i < BiblioDbProperties.Count; i++)
                {
                    if (BiblioDbProperties[i].ItemDbName == strItemDbName)
                        return BiblioDbProperties[i].DbName;
                }
            }

            return null;
        }

        public static bool IsBiblioDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (BiblioDbProperties != null)
            {
                for (int i = 0; i < BiblioDbProperties.Count; i++)
                {
                    if (BiblioDbProperties[i].DbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 将RFC1123时间字符串转换为本地表现形态字符串
        // 注意可能抛出异常
        public static DateTime GetLocalTime(string strTime)
        {
            if (String.IsNullOrEmpty(strTime) == true)
                return DateTime.MinValue;

            return System.TimeZoneInfo.ConvertTimeFromUtc(
                 DateTimeUtil.FromRfc1123DateTimeString(strTime),
                 System.TimeZoneInfo.Local
                 );
        }

        /*
        public static string GetLocalTime(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                return "";

            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strTime, "s");
            }
            catch
            {
                return "";
            }
        }
        */

        // parameters:
        //      strBorrowTime   借阅起点时间。u 格式
        //      strReturningTime    返回应还时间。 u 格式
        internal static int BuildReturingTimeString(DateTime borrowdate,
            string strBorrowPeriod,
            out DateTime timeEnd,
            out string strError)
        {
            strError = "";
            timeEnd = DateTime.MinValue;

            long lValue = 0;
            string strUnit = "";
            // 分析期限参数
            int nRet = StringUtil.ParsePeriodUnit(strBorrowPeriod,
                out lValue,
                out strUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "期限字符串 '" + strBorrowPeriod + "' 格式不合法: " + strError;
                return -1;
            }

            // 正规化时间
            nRet = RoundTime(strUnit,
                ref borrowdate,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta;

            if (strUnit == "day")
                delta = new TimeSpan((int)lValue, 0, 0, 0);
            else if (strUnit == "hour")
                delta = new TimeSpan((int)lValue, 0, 0);
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            timeEnd = borrowdate + delta;

            // 正规化时间
            nRet = RoundTime(strUnit,
                ref timeEnd,
                out strError);
            if (nRet == -1)
                return -1;

            // strReturningTime = timeEnd.ToString("s");

            return 0;
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        /// <summary>
        /// 按照时间基本单位，去掉零头，便于互相计算(整单位的)差额。
        /// </summary>
        /// <param name="strUnit">时间单位。day/hour之一。如果为空，相当于 day</param>
        /// <param name="time">要处理的时间。为 GMT 时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            // 算法是先转换为本地时间，去掉零头，再转换回 GMT 时间
            // time = time.ToLocalTime();
            if (strUnit == "day" || string.IsNullOrEmpty(strUnit) == true)
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            // time = time.ToUniversalTime();

            return 0;
        }


        internal static int ParsePriceString(string strPrice,
            out decimal value,
            out string strUnit,
            out string strError)
        {
            value = 0;
            strUnit = "";
            strError = "";

            if (string.IsNullOrEmpty(strPrice) == true)
                return 0;

#if NO

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";

            // 分析价格参数
            // 允许前面出现+ -号
            // return:
            //      -1  出错
            //      0   成功
            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;
            strUnit = strPrefix + strPostfix;
            decimal v = 0;
            if (decimal.TryParse(strValue, out v) == false)
            {
                strError = "金额字符串 '" + strPrice + "' 中数字部分 '" + strValue + "' 格式不正确";
                return -1;
            }
#endif
            CurrencyItem item = null;
            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out item,
                out strError);
            if (nRet == -1)
                return -1;

            strUnit = item.Prefix + item.Postfix;
            /*
            try
            {
                value = (long)(item.Value * 100);
            }
            catch (Exception ex)
            {
                // 2016/3/31
                strError = "元值 '" + item.Value.ToString() + "' 折算为分值的时候出现异常：" + ex.Message;
                return -1;
            }
            */
            value = item.Value;
            return 0;
        }


        // 根据当前命中数，调整进度条总范围
        void AdjustProgressRange(long lOldCount, long lNewCount)
        {
            //if (this.stop == null)
            //    return;

            long lDelta = lNewCount - lOldCount;
            if (lDelta != 0)
            {
                // this.stop.SetProgressRange(this.stop.ProgressMin, this.stop.ProgressMax + lDelta);
                if (this._estimate != null)
                    this._estimate.EndPosition += lDelta;
            }
        }

        // 根据 册/订购/期/评注 记录路径和 parentid 构造所从属的书目记录路径
        public static string BuildBiblioRecPath(string strDbType,
            string strItemRecPath,
            string strParentID)
        {
            if (string.IsNullOrEmpty(strParentID) == true)
                return null;

            string strItemDbName = GetDbName(strItemRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
                return null;

            string strBiblioDbName = GetBiblioDbNameFromItemDbName(strDbType, strItemDbName);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            return strBiblioDbName + "/" + strParentID;
        }

        public static string GetBiblioDbNameFromItemDbName(string strDbType,
    string strItemDbName)
        {
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (BiblioDbProperties != null)
            {
                if (strDbType == "item" || strDbType == "entity")
                {
                    foreach (BiblioDbProperty prop in BiblioDbProperties)
                    {
                        if (prop.ItemDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "order")
                {
                    foreach (BiblioDbProperty prop in BiblioDbProperties)
                    {
                        if (prop.OrderDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "issue")
                {
                    foreach (BiblioDbProperty prop in BiblioDbProperties)
                    {
                        if (prop.IssueDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "comment")
                {
                    foreach (BiblioDbProperty prop in BiblioDbProperties)
                    {
                        if (prop.CommentDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else
                    throw new Exception("无法处理数据库类型 '" + strDbType + "'");
            }

            return null;
        }


        // 根据日志文件创建本地 operlogxxx 表
        public int DoCreateOperLogTable(
            ref LibraryContext context,
            LibraryChannel channel,
            long lProgressStart,
            string strStartDate,
            string strEndDate,
            LogType logType,
            bool bTraceLogRecord, // 是否跟踪日志记录。第一阶段只创建 operlogxxx 表的时候，本参数应当为 false
            Delegate_showMessage func_showMessage,
            CancellationToken token,
            out string strLastDate,
            out long lLastIndex,
            out string strError)
        {
            strError = "";
            strLastDate = "";
            lLastIndex = 0;

            int nRet = 0;

            // strEndDate 里面可能会包含 ":0-99" 这样的附加成分
            string strLeft = "";
            string strEndRange = "";
            StringUtil.ParseTwoPart(strEndDate,
                ":",
                out strLeft,
                out strEndRange);
            strEndDate = strLeft;

            string strStartRange = "";
            StringUtil.ParseTwoPart(strStartDate,
                ":",
                out strLeft,
                out strStartRange);
            strStartDate = strLeft;

            // TODO: start 和 end 都有 range，而且 start 和 end 是同一天怎么办?

            List<string> filenames = null;

            string strWarning = "";

            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            nRet = OperLogLoader.MakeLogFileNames(strStartDate,
                strEndDate,
                true,  // true,
                out filenames,
                out strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(this, strWarning);
                }));
            }
            */

            if (filenames.Count > 0 && string.IsNullOrEmpty(strEndRange) == false)
            {
                filenames[filenames.Count - 1] = filenames[filenames.Count - 1] + ":" + strEndRange;
            }
            if (filenames.Count > 0 && string.IsNullOrEmpty(strStartRange) == false)
            {
                filenames[0] = filenames[0] + ":" + strStartRange;
            }

            channel.Timeout = new TimeSpan(0, 1, 0);   // 一分钟


            ProgressEstimate estimate = new ProgressEstimate();

            OperLogLoader loader = new OperLogLoader();
            loader.Channel = channel;
            loader.Stop = null;
            loader.Estimate = estimate;
            loader.Dates = filenames;
            loader.Level = 2;  //  Program.MainForm.OperLogLevel;
            loader.AutoCache = false;
            loader.CacheDir = "";
            loader.Filter = "borrow,return,setReaderInfo,setBiblioInfo,setEntity,setOrder,setIssue,setComment,amerce,passgate,getRes";
            loader.LogType = logType;

            loader.ProgressStart = lProgressStart;

            //loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
            //loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

            List<OperBase> opers = new List<OperBase>();
            try
            {
                string maxDate = GetMaxDate(context);

                int nProcessCount = 0;
                string prev_date = "";
                int nRecCount = 0;
                foreach (OperLogItem item in loader)
                {
                    if (token != null && token.IsCancellationRequested)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    if (prev_date != item.Date)
                    {
                        func_showMessage?.Invoke($"正在处理日志文件 {item.Date}");
                        prev_date = item.Date;
                    }

                    string strXml = item.Xml;

                    if (string.IsNullOrEmpty(strXml) == true)
                    {
                        nRecCount++;
                        continue;
                    }

                    {
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            // TODO: 记入错误日志，然后继续处理

                            strError = item.Date + " 中偏移为 " + item.Index.ToString() + " 的日志记录 XML 装载到 DOM 时出错: " + ex.Message;
                            /*
                            DialogResult result = DialogResult.No;

                            string strText = strError;
                            this.Invoke((Action)(() =>
                            {
                                result = MessageBox.Show(this,
 strText + "\r\n\r\n是否跳过此条记录继续处理?",
 "ReportForm",
 MessageBoxButtons.YesNo,
 MessageBoxIcon.Question,
 MessageBoxDefaultButton.Button1);
                            }));
                            if (result == DialogResult.No)
                                return -1;
                                */
                            continue;
                        }

                        string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                        string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

                        OperLogItem current_item = item;
                        if (StringUtil.CompareVersion(this.ServerVersion, "2.74") < 0
                            && strOperation == "amerce" && (strAction == "amerce" || strAction == "modifyprice"))
                        {
                            // 重新获得当前日志记录，用最详细级别
                            OperLogItem new_item = loader.LoadOperLogItem(item, 0);
                            if (new_item == null)
                            {
                                strError = "重新获取 OperLogItem 时出错";
                                return -1;
                            }
                            dom.LoadXml(new_item.Xml);
                            current_item = new_item;
                            /*
                            nRet = BuildOpers(
    strOperation,
    dom,
    new_item.Date,
    new_item.Index,
    out List<OperBase> lines,
    out strError);
                            if (lines != null)
                                opers.AddRange(lines);
                                */
                        }

                        {
                            // return:
                            //      -2  不能识别的 strOperation 类型
                            //      -1  出错
                            //      0   成功
                            nRet = BuildOpers(
                                context,
                                strOperation,
                                dom,
                                current_item.Date,
                                current_item.Index,
                                out List<OperBase> lines,
                                out strError);
                            if (lines != null)
                                opers.AddRange(lines);
                        }
                        if (nRet == -1)
                            return -1;
                        // -2 不要报错

                        if (bTraceLogRecord)
                        {
                            // 将一条日志记录中的动作兑现到 item reader biblio class_ 表
                            // return:
                            //      -1  出错
                            //      0   没有必要处理
                            //      1   完成
                            nRet = ProcessLogRecord(
                                context,
                                item,
                                dom,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;
                                // TODO: 写入错误日志
                            }
                            if (nRet == 1)
                                nProcessCount++;

                            /*
                            if (nProcessCount >= 1000)
                            {
                                context.SaveChanges();
                                context.Dispose();
                                context = new LibraryContext();
                                nProcessCount = 0;
                            }
                            */
                        }
                    }

                    if (opers.Count > 100) // 4000
                    {
                        // context.AddRange(opers);
                        SaveOpers(ref context, opers);
                        opers.Clear();

                        strLastDate = item.Date;
                        lLastIndex = item.Index + 1;
                        nRecCount = 0;
                    }

                    nRecCount++;
                } // end of foreach

                if (opers.Count > 0)
                {
                    // context.AddRange(opers);
                    SaveOpers(ref context, opers);
                    opers.Clear();
                }

                /*
                if (bTraceLogRecord)
                    context.SaveChanges();
                    */

                // 表示处理完成
                strLastDate = "";
                lLastIndex = 0;
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 获得已经存在的事项的最大 Date 值。如果不存在任何事项，则返回 null
        public string GetMaxDate(LibraryContext context)
        {
            var item = context.AmerceOpers
                .Select(oper => new { oper.Date })
                .OrderByDescending(x => x.Date)
                .Take(1)
                .FirstOrDefault();
            if (item == null)
                return null;
            return item.Date;
        }

        public void SaveOpers(ref LibraryContext context,
            List<OperBase> lines)
        {
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var line in lines)
                    {
                        context.AddOrUpdate(line);
                    }
                    context.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback() 本身也可能再次抛异常
                    dbContextTransaction.Rollback(); //Required according to MSDN article 
                                                     // TODO: 写入错误日志
                   throw ex;
                }
            }

            context.Dispose();
            context = new LibraryContext();
        }

        // 在内存中增加一行
        // return:
        //      -2  不能识别的 strOperation 类型
        //      -1  出错
        //      0   成功
        public int BuildOpers(
            LibraryContext context,
            string strOperation,
            XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
            out string strError)
        {
            lines = new List<OperBase>();
            DbSet<OperBase> set = null;

            OperBase line = null;
            if (strOperation == "borrow" || strOperation == "return")
            {
                // line = context.CircuOpers.Find(line.GetKeys()) ?? new CircuOper();
                line = new CircuOper();
                set = context.Set<CircuOper>();
            }
            else if (strOperation == "setReaderInfo")
            {
                line = context.PatronOpers.Find(line.GetKeys()) ?? new PatronOper();
            }
            else if (strOperation == "setBiblioInfo")
            {
                line = context.BiblioOpers.Find(line.GetKeys()) ?? new BiblioOper();
            }
            else if (strOperation == "setEntity")
            {
                line = context.ItemOpers.Find(line.GetKeys()) ?? new ItemOper();
            }
            else if (strOperation == "setOrder")
            {
                line = context.ItemOpers.Find(line.GetKeys()) ?? new ItemOper();
            }
            else if (strOperation == "setIssue")
            {
                line = context.ItemOpers.Find(line.GetKeys()) ?? new ItemOper();
            }
            else if (strOperation == "setComment")
            {
                line = context.ItemOpers.Find(line.GetKeys()) ?? new ItemOper();
            }
            else if (strOperation == "amerce")
            {
                line = context.AmerceOpers.Find(line.GetKeys()) ?? new AmerceOper();
            }
            else if (strOperation == "passgate")
            {
                line = context.PassGateOpers.Find(line.GetKeys()) ?? new PassGateOper();
            }
            else if (strOperation == "getRes")
            {
                line = context.GetResOpers.Find(line.GetKeys()) ?? new GetResOper();
            }
            else
            {
                strError = "不能识别的 strOperation '" + strOperation + "'";
                return -2;
            }
            int nRet = line.SetData(dom,
    strDate,
    lIndex,
    out List<OperBase> temp_lines,
    out strError);
            if (nRet == -1)
                return -1;
            lines.Add(line);
            if (temp_lines != null && temp_lines.Count > 0)
                lines.AddRange(temp_lines);

            var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == "test");

            var set = context.Set(type);
            return 0;
        }


        #region

        // 将一条日志记录中的动作兑现到 item reader biblio class_ 表
        // return:
        //      -1  出错
        //      0   没有必要处理
        //      1   完成
        int ProcessLogRecord(
            LibraryContext context,
            OperLogItem info,
            XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strOperation = DomUtil.GetElementText(dom.DocumentElement,
    "operation");
            if (strOperation == "setBiblioInfo")
            {
                // return:
                //     -1  出错
                //     0   没有必要处理
                //     1   已经处理
                nRet = this.TraceSetBiblioInfo(
                    context,
                    dom,
                    out strError);
            }
            else if (strOperation == "setEntity")
            {
                nRet = this.TraceSetEntity(
                    context,
                    dom,
                    out strError);
            }
            else if (strOperation == "setReaderInfo")
            {
                nRet = this.TraceSetReaderInfo(
                    context,
                    dom,
                    out strError);
            }
            else if (strOperation == "borrow")
            {
                nRet = this.TraceBorrow(
                    context,
                    dom,
                    out strError);
            }
            else if (strOperation == "return")
            {
                nRet = this.TraceReturn(
                    context,
                    dom,
                    out strError);
            }

            if (nRet == -1)
            {
                string strAction = DomUtil.GetElementText(dom.DocumentElement,
                        "action");
                strError = "operation=" + strOperation + ";action=" + strAction + ": " + strError;
                return -1;
            }

            return nRet;
        }

        // SetBiblioInfo() API 或 CopyBiblioInfo() API 的恢复动作
        // 函数内，使用return -1;还是goto ERROR1; 要看错误发生的时候，是否还有价值继续探索SnapShot重试。如果是，就用后者。
        /*
<root>
  <operation>setBiblioInfo</operation> 
  <action>...</action> 具体动作 有 new/change/delete/onlydeletebiblio/onlydeletesubrecord 和 onlycopybiblio/onlymovebiblio/copy/move
  <record recPath='中文图书/3'>...</record> 记录体 动作为new/change/ *move* / *copy* 时具有此元素(即delete时没有此元素)
  <oldRecord recPath='中文图书/3'>...</oldRecord> 被覆盖、删除或者移动的记录 动作为change/ *delete* / *move* / *copy* 时具备此元素
  <deletedEntityRecords> 被删除的实体记录(容器)。只有当<action>为delete时才有这个元素。
	  <record recPath='中文图书实体/100'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。
	  ...
  </deletedEntityRecords>
  <copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </copyEntityRecords>
  <moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </moveEntityRecords>
  <copyOrderRecords /> <moveOrderRecords />
  <copyIssueRecords /> <moveIssueRecords />
  <copyCommentRecords /> <moveCommentRecords />
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>

逻辑恢复delete操作的时候，检索出全部下属的实体记录删除。
快照恢复的时候，可以根据operlogdom直接删除记录了path的那些实体记录
         * */
        // return:
        //     -1  出错
        //     0   没有必要处理
        //     1   已经处理
        public int TraceSetBiblioInfo(
            LibraryContext context,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            using (var dbContextTransaction = context.Database.BeginTransaction())
            {

                //long lRet = 0;
                int nRet = 0;

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction == "new" || strAction == "change")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        return 0;   // 轮空

                    string strTimestamp = DomUtil.GetAttr(node, "timestamp");

                    // 把书目摘要写入 biblio 表

                    // 把分类号写入若干分类号表
                    try
                    {
                        Biblio.UpdateBiblioRecord(
                context,
                strRecPath,
                strRecord);
                    }
                    catch (Exception ex)
                    {
                        if (nRet == -1)
                        {
                            strError = ex.Message;
                            return -1;
                        }
                    }
                }
                else if (strAction == "onlymovebiblio"
                    || strAction == "onlycopybiblio"
                    || strAction == "move"
                    || strAction == "copy")
                {
                    string strTargetRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    string strTargetRecPath = DomUtil.GetAttr(node, "recPath");

                    if (string.IsNullOrEmpty(strTargetRecPath) == true)
                        return 0;

                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    bool bOldExist = DomUtil.GetBooleanParam(node, "exist", true);

                    string strMergeStyle = DomUtil.GetElementText(domLog.DocumentElement,
        "mergeStyle");

                    // 如果目标记录没有记载，就尽量用源记录
                    if (String.IsNullOrEmpty(strTargetRecord) == true)
                    {
                        if (String.IsNullOrEmpty(strOldRecord) == true)
                        {
                            if (bOldExist == true)
                            {
                                strError = "源记录 '" + strOldRecPath + "' 不存在，并且<record>元素无文本内容，这时<oldRecord>元素也无文本内容，无法获得要写入的记录内容";
                                return -1;
                            }
                        }
                        else
                            strTargetRecord = strOldRecord;
                    }

                    // 如果有“新记录”内容
                    if (string.IsNullOrEmpty(strTargetRecPath) == false
                        && String.IsNullOrEmpty(strTargetRecord) == false)
                    {
                        // 写入新的书目记录
                        try
                        {
                            Biblio.UpdateBiblioRecord(
                    context,
                    strTargetRecPath,
                    strTargetRecord);
                        }
                        catch (Exception ex)
                        {
                            if (nRet == -1)
                            {
                                strError = "TraceSetBiblioInfo() 出现异常: " + ex.Message;
                                return -1;
                            }
                        }
                    }

                    // 复制或者移动下级子记录
                    if (strAction == "move"
                    || strAction == "copy")
                    {
                        nRet = CopySubRecords(
                            context,
                            domLog,
                            strAction,
                            // string strSourceBiblioRecPath,
                            strTargetRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    if (strAction == "move" || strAction == "onlymovebiblio")
                    {
                        // 删除旧的书目记录
                        nRet = DeleteBiblioRecord(
                            context,
                            strOldRecPath,
                            true,
                            true,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
                else if (strAction == "delete"
                    || strAction == "onlydeletebiblio"
                    || strAction == "onlydeletesubrecord")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    if (string.IsNullOrEmpty(strRecPath) == false)
                    {
                        // 删除书目记录
                        nRet = DeleteBiblioRecord(
                            context,
                            strRecPath,
                            strAction == "delete" || strAction == "onlydeletebiblio" ? true : false,
                            strAction == "delete" || strAction == "onlydeletesubrecord" ? true : false,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                context.SaveChanges();
                dbContextTransaction.Commit();
            }
            return 1;
        }

        // TODO: 需要扩展为也能复制 order issue comment 记录
        int CopySubRecords(
            LibraryContext context,
            XmlDocument dom,
            string strAction,
            string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";

            if (dom == null || dom.DocumentElement == null)
                return 0;

            string strElement = "";
            if (strAction == "move")
                strElement = "moveEntityRecords";
            else if (strAction == "copy")
                strElement = "copyEntityRecords";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes(strElement + "/record");
            if (nodes.Count == 0)
                return 0;

            StringBuilder text = new StringBuilder(4096);

            // Debug.WriteLine("CopySubRecords() nodes.Count=" + nodes.Count);
            /*
<copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
<record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
...
</copyEntityRecords>
<moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
<record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
...
</moveEntityRecords>
* */
            foreach (XmlNode node in nodes)
            {
                string strSourceRecPath = DomUtil.GetAttr(node, "recPath");
                string strTargetRecPath = DomUtil.GetAttr(node, "targetRecPath");

                if (strAction == "copy")
                {
                    string strNewBarcode = DomUtil.GetAttr(node, "newBarcode");

                    var source_item = context.Items.SingleOrDefault(x => x.ItemRecPath == strSourceRecPath);
                    var target_item = context.Items.SingleOrDefault(x => x.ItemRecPath == strTargetRecPath);
                    if (target_item == null)
                    {
                        if (source_item != null)
                        {
                            target_item = source_item.Clone();
                            target_item.ClearBorrowInfo();
                            target_item.ItemRecPath = strTargetRecPath;
                        }
                        else
                        {
                            // 注意这时候新 new 的对象很多字段都是空，信息不全了
                            target_item = new Item
                            {
                                ItemRecPath = strTargetRecPath,
                                BiblioRecPath = strTargetBiblioRecPath,
                                CreateTime = DateTime.Now
                            };
                        }
                    }
                    target_item.BiblioRecPath = strTargetBiblioRecPath;
                    if (string.IsNullOrEmpty(strNewBarcode) == false)
                        target_item.ItemBarcode = strNewBarcode;
                    context.AddOrUpdate(target_item);
                }
                else
                {
                    // move
                    // 注意检查源和目标记录路径应该不同
                    // 删除源。修改目标


                    string strNewBarcode = DomUtil.GetAttr(node, "newBarcode");

                    var source_item = context.Items.SingleOrDefault(x => x.ItemRecPath == strSourceRecPath);
                    var target_item = context.Items.SingleOrDefault(x => x.ItemRecPath == strTargetRecPath);
                    if (target_item != null && source_item != null)
                    {
                        // 源和目标都已经存在。
                        context.Remove(source_item);
                        source_item.CopyTo(target_item);
                        Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");
                        target_item.ItemRecPath = strTargetRecPath;
                    }
                    else if (target_item == null && source_item != null)
                    {
                        // 源存在，目标不存在。
                        // 要把源修改为目标
                        target_item = source_item.Clone();
                        context.Remove(source_item);

                        Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");
                        target_item.ItemRecPath = strTargetRecPath;
                    }
                    else if (target_item != null && source_item == null)
                    {
                        // 源不存在，目标存在。
                        // 注：这种情况有潜在问题
                    }
                    else if (target_item == null)
                    {
                        // 源和目标都不存在
                        // 注：这种情况有潜在问题
                        target_item = new Item
                        {
                            ItemRecPath = strTargetRecPath,
                            CreateTime = DateTime.Now
                        };
                    }

                    target_item.ItemRecPath = strTargetRecPath;
                    target_item.BiblioRecPath = strTargetBiblioRecPath;
                    if (string.IsNullOrEmpty(strNewBarcode) == false)
                        target_item.ItemBarcode = strNewBarcode;
                    context.AddOrUpdate(target_item);
                }
            }

            return 0;
        }

        int DeleteBiblioRecord(
    LibraryContext context,
    string strBiblioRecPath,
    bool bDeleteBiblio,
    bool bDeleteSubrecord,
    out string strError)
        {
            strError = "";

            if (bDeleteBiblio == false
    && bDeleteSubrecord == false)
            {
                return 0;
            }

            if (bDeleteBiblio)
            {
                var biblio = context.Biblios.SingleOrDefault(x => x.RecPath == strBiblioRecPath);
                if (biblio != null)
                    context.Biblios.Remove(biblio);
            }

            if (bDeleteSubrecord)
            {
                var items = context.Items.Where(x => x.BiblioRecPath == strBiblioRecPath).ToList();
                if (items.Count > 0)
                    context.Items.RemoveRange(items);
            }

            return 1;
        }

        // SetEntities() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setEntity</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书实体/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书实体/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) <record>中的内容, 涉及到流通的<borrower><borrowDate><borrowPeriod>等, 在日志恢复阶段, 都应当无效, 这几个内容应当从当前位置库中记录获取, 和<record>中其他内容合并后, 再写入数据库
	3) 一次SetEntities()API调用, 可能创建多条日志记录。
         
         * */
        public int TraceSetEntity(
    LibraryContext context,
    XmlDocument domLog,
    out string strError)
        {
            strError = "";

            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                int nRet = 0;

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction == "new"
        || strAction == "change"
        || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        // 改为进行删除操作
                        strAction = "delete";
                        goto TRY_DELETE;
                    }

                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    // 
                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    }

                    string strCreateOperTime = "";

                    if (strAction == "new")
                        strCreateOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");

                    // 在 SQL item 库中写入一条册记录
                    nRet = WriteItemRecord(context,
                        strNewRecPath,
                        strRecord,
                        strCreateOperTime,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "写入册记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                    if (strAction == "move")
                    {
                        // 删除册记录
                        var item = context.Items.FirstOrDefault(x => x.ItemRecPath == strOldRecPath);
                        if (item != null)
                            context.Items.Remove(item);
                    }

                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }
            TRY_DELETE:
                if (strAction == "delete")
                {
                    XmlNode node = null;
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除册记录
                    var item = context.Items.FirstOrDefault(x => x.ItemRecPath == strRecPath);
                    if (item != null)
                        context.Items.Remove(item);

                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }

                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }
        }

        // 在 SQL item 库中写入一条册记录
        // parameters:
        //      strLogCreateTime    日志操作记载的创建时间。不是创建动作的其他时间，不要放在这里
        int WriteItemRecord(LibraryContext context,
            string strItemRecPath,
            string strItemXml,
            string strLogCreateTime,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 出错: " + ex.Message;
                return -1;
            }

            string strParentID = DomUtil.GetElementText(dom.DocumentElement,
                "parent");
            // 根据 册/订购/期/评注 记录路径和 parentid 构造所从属的书目记录路径
            string strBiblioRecPath = BuildBiblioRecPath("item",
                strItemRecPath,
                strParentID);
            if (string.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "根据册记录路径 '" + strItemRecPath + "' 和 parentid '" + strParentID + "' 构造书目记录路径出错";
                return 0;
            }

            var line = context.Items.FirstOrDefault(x => x.ItemRecPath == strItemRecPath);

            // 
            int nRet = Item.FromXml(dom,
            strItemRecPath,
            strBiblioRecPath,
            strLogCreateTime,
            ref line,
            out strError);
            if (nRet == -1)
                return -1;

            context.AddOrUpdate(line);
            return 0;
        }

        // SetReaderInfo() API 恢复动作
        /*
<root>
	<operation>setReaderInfo</operation> 操作类型
	<action>...</action> 具体动作。有new change delete move 4种
	<record recPath='...'>...</record> 新记录
    <oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
    <changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
	<operator>test</operator> 操作者
	<operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

         * */
        public int TraceSetReaderInfo(
LibraryContext context,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                int nRet = 0;

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                    "action");

                if (strAction == "new"
                    || strAction == "change"
                    || strAction == "move")
                {
                    string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "record",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<record>元素";
                        return -1;
                    }
                    string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                    string strOldRecord = "";
                    string strOldRecPath = "";
                    if (strAction == "move")
                    {
                        strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                            "oldRecord",
                            out node);
                        if (node == null)
                        {
                            strError = "日志记录中缺<oldRecord>元素";
                            return -1;
                        }

                        strOldRecPath = DomUtil.GetAttr(node, "recPath");
                        if (string.IsNullOrEmpty(strOldRecPath) == true)
                        {
                            strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                            return -1;
                        }

                        // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                        if (string.IsNullOrEmpty(strRecord) == true)
                            strRecord = strOldRecord;
                    }

                    // 在 Patrons 表中写入一条读者记录
                    nRet = WriteReaderRecord(context,
                        strNewRecPath,
                        strRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "写入读者记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                        return -1;
                    }

                    XmlNodeList nodes = domLog.DocumentElement.SelectNodes("changedEntityRecord");
                    foreach (XmlElement item in nodes)
                    {
                        string strItemBarcode = item.GetAttribute("itemBarcode");
                        string strItemRecPath = item.GetAttribute("recPath");
                        string strOldReaderBarcode = item.GetAttribute("oldBorrower");
                        string strNewReaderBarcode = item.GetAttribute("newBorrower");

                        nRet = TraceChangeBorrower(
                            context,
                            strItemBarcode,
                            strItemRecPath,
                            strNewReaderBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "修改册记录 '" + strItemRecPath + "' 的 borrower 字段时发生错误: " + strError;
                            return -1;
                        }
                    }

                    if (strAction == "move")
                    {
                        // 删除读者记录
                        var patron = context.Patrons.FirstOrDefault(x => x.RecPath == strOldRecPath);
                        if (patron != null)
                            context.Patrons.Remove(patron);
                    }

                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }
                else if (strAction == "delete")
                {
                    string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out XmlNode node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }
                    string strRecPath = DomUtil.GetAttr(node, "recPath");

                    // 删除读者记录
                    var patron = context.Patrons.FirstOrDefault(x => x.RecPath == strRecPath);
                    if (patron != null)
                        context.Patrons.Remove(patron);

                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }
                else
                {
                    strError = "无法识别的<action>内容 '" + strAction + "'";
                    return -1;
                }
            }
        }

        // 在 SQL reader 库中写入一条读者记录
        int WriteReaderRecord(LibraryContext context,
            string strReaderRecPath,
            string strReaderXml,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strReaderRecPath) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "WriteReaderRecord XML 装入 DOM 出错: " + ex.Message;
                return -1;
            }

            // 根据读者库名，得到馆代码
            string strReaderDbName = GetDbName(strReaderRecPath);

            string strLibraryCode = GetReaderDbLibraryCode(strReaderDbName);

            var line = context.Patrons.FirstOrDefault(x => x.RecPath == strReaderRecPath);
            // 根据 XML 记录建立
            int nRet = Patron.FromXml(dom,
                strReaderRecPath,
                strLibraryCode,
                ref line,
                out strError);
            if (nRet == -1)
                return -1;

            context.AddOrUpdate(line);
            return 0;
        }

        public int TraceChangeBorrower(
    LibraryContext context,
    string strItemBarcode,
    string strItemRecPath,
    string strNewBorrower,
    out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "strItemBarcode 参数不应为空";
                return -1;
            }

            var item = context.Items.FirstOrDefault(x => x.ItemRecPath == strItemRecPath);
            if (item != null)
            {
                item.ItemBarcode = strItemBarcode;
                item.Borrower = strNewBorrower;
                context.Items.Update(item);
            }
            else
            {
                // TODO 写入错误日志，表明没有找到这个路径的实体记录
            }

#if NO
            ItemLine line = new ItemLine();

            // line.Full = false;
            line.Level = 2;
            line.ItemBarcode = strItemBarcode;
            line.Borrower = strNewBorrower;
            line.ItemRecPath = strItemRecPath;

            this._updateItems.Add(line);
            if (this._updateItems.Count >= UPDATE_ITEMS_BATCHSIZE)
            {
                nRet = CommitUpdateItems(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif
            return 0;
        }

        // Borrow() API 恢复动作
        /*
<root>
  <operation>borrow</operation> 操作类型
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <itemBarcode>0000001</itemBarcode>  册条码号
  <borrowDate>Fri, 08 Dec 2006 04:17:31 GMT</borrowDate> 借阅日期
  <borrowPeriod>30day</borrowPeriod> 借阅期限
  <no>0</no> 续借次数。0为首次普通借阅，1开始为续借
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:31 GMT</operTime> 操作时间
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
</root>
         * */
        public int TraceBorrow(
LibraryContext context,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                int nRet = 0;

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
        "readerBarcode");
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    strError = "<readerBarcode>元素值为空";
                    return -1;
                }

                // 读入册记录
                string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                    "confirmItemRecPath");
                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                if (String.IsNullOrEmpty(strItemBarcode) == true)
                {
                    strError = "<strItemBarcode>元素值为空";
                    return -1;
                }

                var borrowDate = GetLocalTime(DomUtil.GetElementText(domLog.DocumentElement,
                    "borrowDate"));
                string strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
                    "borrowPeriod");
                //string strReturningDate = ItemLine.GetLocalTime(DomUtil.GetElementText(domLog.DocumentElement,
                //    "returningDate"));

                DateTime returningTime = DateTime.MinValue;

                if (borrowDate != DateTime.MinValue)
                {
                    // parameters:
                    //      strBorrowTime   借阅起点时间。u 格式
                    //      strReturningTime    返回应还时间。 u 格式
                    nRet = BuildReturingTimeString(borrowDate,
        strBorrowPeriod,
        out returningTime,
        out strError);
                    if (nRet == -1)
                    {
                        returningTime = DateTime.MinValue;
                    }
                }
                else
                    returningTime = DateTime.MinValue;


                Item item = null;

                if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    item = context.Items.FirstOrDefault(x => x.ItemRecPath == strConfirmItemRecPath);
                else
                    item = context.Items.FirstOrDefault(x => x.ItemBarcode == strItemBarcode);

                if (item != null)
                {
                    if (string.IsNullOrEmpty(item.ItemBarcode))
                        item.ItemBarcode = strItemBarcode;
                    if (string.IsNullOrEmpty(item.ItemRecPath))
                        item.ItemRecPath = strConfirmItemRecPath;
                    item.Borrower = strReaderBarcode;
                    item.BorrowTime = borrowDate;
                    item.BorrowPeriod = strBorrowPeriod;
                    item.ReturningTime = returningTime;

                    context.Items.Update(item);
                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }
                return 0;
            }
        }

        // Return() API 恢复动作
        /*
<root>
  <operation>return</operation> 操作类型
  <action>return</action> 动作。有 return/lost/inventory/read/boxing 几种。恢复动作目前仅恢复 return 和 lost 两种，其余会忽略
  <itemBarcode>0000001</itemBarcode> 册条码号
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
  <overdues>...</overdues> 超期信息 通常内容为一个字符串，为一个<overdue>元素XML文本片断
  
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
  
</root>

         * */
        public int TraceReturn(
LibraryContext context,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            using (var dbContextTransaction = context.Database.BeginTransaction())
            {

                string strAction = DomUtil.GetElementText(domLog.DocumentElement,
    "action");
                if (strAction != "return" && strAction != "lost")
                    return 0;   // 其余 inventory/read/boxing 动作并不会改变任何册记录，所以这里返回了

                string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
        "readerBarcode");
                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    strError = "<readerBarcode>元素值为空";
                    return -1;
                }

                // 读入册记录
                string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                    "confirmItemRecPath");
                string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                    "itemBarcode");
                if (String.IsNullOrEmpty(strItemBarcode) == true)
                {
                    strError = "<strItemBarcode>元素值为空";
                    return -1;
                }

                Item item = null;

                if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                    item = context.Items.FirstOrDefault(x => x.ItemRecPath == strConfirmItemRecPath);
                else
                    item = context.Items.FirstOrDefault(x => x.ItemBarcode == strItemBarcode);

                if (item != null)
                {
                    if (string.IsNullOrEmpty(item.ItemBarcode))
                        item.ItemBarcode = strItemBarcode;
                    if (string.IsNullOrEmpty(item.ItemRecPath))
                        item.ItemRecPath = strConfirmItemRecPath;
                    item.Borrower = null;
                    item.BorrowTime = DateTime.MinValue;
                    item.BorrowPeriod = null;
                    item.ReturningTime = DateTime.MinValue;

                    context.Items.Update(item);
                    context.SaveChanges();
                    dbContextTransaction.Commit();
                    return 1;
                }

                return 0;
            }
        }


        #endregion

#if NO
        #region 日志同步



        int CommitDeleteBiblios(
    SQLiteConnection connection,
    out string strError)
        {
            strError = "";
            //int nRet = 0;

            if (this._deleteBiblios.Count == 0)
                return 0;

            Debug.WriteLine("CommitDeleteBiblios() _deleteBiblios.Count=" + _deleteBiblios.Count);
#if NO
            List<BiblioDbFromInfo> styles = null;
            // 获得所有分类号检索途径 style
            nRet = GetClassFromStyles(out styles,
                out strError);
            if (nRet == -1)
                return -1;
#endif
            Debug.Assert(this._classFromStyles != null, "");

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {

                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (UpdateBiblio update in this._deleteBiblios)
                    {
                        string strBiblioRecPath = update.BiblioRecPath;
                        Debug.Assert(string.IsNullOrEmpty(strBiblioRecPath) == false, "");
                        if (update.DeleteBiblio)
                        {
                            foreach (BiblioDbFromInfo style in this._classFromStyles)
                            {
                                // 删除 class 记录
                                text.Append("delete from class_" + style.Style + " where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
                            }
                        }
                        SQLiteUtil.SetParameter(command,
        "@bibliorecpath" + i.ToString(),
        strBiblioRecPath);

                        if (update.DeleteBiblio)
                        {
                            // 删除 biblio 记录
                            text.Append("delete from biblio where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
                        }

                        if (update.DeleteSubrecord)
                        {
                            // 删除 item 记录
                            text.Append("delete from item where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

#if NO
                    // 删除 order 记录
                    text.Append("delete from order where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

                    // 删除 issue 记录
                    text.Append("delete from issue where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

                    // 删除 comment 记录
                    text.Append("delete from comment where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
#endif
                        }

                        i++;
                    }

                    if (text.Length > 0)
                    {
                        command.CommandText = text.ToString();
                        int nCount = command.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();
            }
            this._deleteBiblios.Clear();
            return 0;
        }

        const int DELETE_BIBLIOS_BATCHSIZE = 10;

        List<UpdateBiblio> _deleteBiblios = new List<UpdateBiblio>();

        // 应当是连续的 Update 操作，才能缓存。中间有 Delete 操作，就要把前面的缓存队先后，立即执行 Delete
        class UpdateBiblio
        {
            public string BiblioRecPath = "";
            public string BiblioXml = "";

            // 是否删除书目记录部分
            public bool DeleteBiblio = true;
            // 是否删除下级记录
            public bool DeleteSubrecord = true;

            public string Summary = ""; // [out]
            public string KeysXml = ""; // [out]
        }

        const int UPDATE_BIBLIOS_BATCHSIZE = 10;
        List<UpdateBiblio> _updateBiblios = new List<UpdateBiblio>();












        const int UPDATE_ITEMS_BATCHSIZE = 10;

        List<ItemLine> _updateItems = new List<ItemLine>();



        int CommitUpdateItems(
            SQLiteConnection connection,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._updateItems.Count == 0)
                return 0;

            Debug.WriteLine("CommitUpdateItems() _updateItems.Count=" + _updateItems.Count);

            // 插入一批册记录
            nRet = ItemLine.AppendItemLines(
                connection,
                _updateItems,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            this._updateItems.Clear();
            return 0;
        }

        const int UPDATE_READERS_BATCHSIZE = 10;

        List<ReaderLine> _updateReaders = new List<ReaderLine>();


        int CommitUpdateReaders(
    SQLiteConnection connection,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._updateReaders.Count == 0)
                return 0;

            Debug.WriteLine("CommitUpdateReaders() _updateReaders.Count=" + _updateReaders.Count);

            // 插入一批读者记录
            nRet = ReaderLine.AppendReaderLines(
                connection,
                this._updateReaders,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            this._updateReaders.Clear();
            return 0;
        }

        #endregion

#endif
    }


    /// <summary>
    /// 书目库属性
    /// </summary>
    public class BiblioDbProperty
    {
        /// <summary>
        /// 书目库名
        /// </summary>
        public string DbName = "";  // 书目库名
                                    /// <summary>
                                    /// 格式语法
                                    /// </summary>
        public string Syntax = "";  // 格式语法

        /// <summary>
        /// 实体库名
        /// </summary>
        public string ItemDbName = "";  // 对应的实体库名

        /// <summary>
        /// 期库名
        /// </summary>
        public string IssueDbName = ""; // 对应的期库名 2007/10/19 

        /// <summary>
        /// 订购库名
        /// </summary>
        public string OrderDbName = ""; // 对应的订购库名 2007/11/30 

        /// <summary>
        /// 评注库名
        /// </summary>
        public string CommentDbName = "";   // 对应的评注库名 2009/10/23 

        /// <summary>
        /// 角色
        /// </summary>
        public string Role = "";    // 角色 2009/10/23 

        /// <summary>
        /// 是否参与流通
        /// </summary>
        public bool InCirculation = true;  // 是否参与流通 2009/10/23 

        // 2018/9/25
        /// <summary>
        /// 用途
        /// </summary>
        public string Usage { get; set; }
    }

}
