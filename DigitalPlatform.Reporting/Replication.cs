using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DigitalPlatform.Reporting
{
#if REMOVED
    /// <summary>
    /// 同步和复制功能
    /// </summary>
    public class Replication
    {
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

        // TODO: 最好把第一次初始化本地 sql 表的动作也纳入 XML 文件中，这样做单项任务的时候，就不会毁掉其他的表
        // 创建批处理计划
        // 根元素的 state 属性， 值为 first 表示正在进行首次创建，尚未完成; daily 表示已经创建完，进入每日同步阶段
        int BuildPlan(string strTypeList,
            LibraryChannel channel,
            XmlDocument database_dom,
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
                List<string> item_dbnames = GetItemDbNames(database_dom);

                // 获得每个实体库的尺寸
                foreach (string strItemDbName in item_dbnames)
                {
                    // stop.SetMessage("正在计划任务 检索 " + strItemDbName + " ...");

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
                List<string> reader_dbnames = GetReaderDbNames(database_dom);

                // 
                foreach (string strReaderDbName in reader_dbnames)
                {
                    // stop.SetMessage("正在计划任务 检索 " + strReaderDbName + " ...");
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
                List<string> biblio_dbnames = GetBiblioDbNames(database_dom);

#if NO

                    // 获得所有分类号检索途径 style
                    List<string> styles = new List<string>();
                    nRet = GetClassFromStyles(out styles,
                        out strError);
                    if (nRet == -1)
                        return -1;
#endif
                // 记忆书目库的分类号 style 列表
                nRet = MemoryClassFromStyles(task_dom.DocumentElement,
        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
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
                }

                // 从计划文件中获得所有分类号检索途径 style
                List<string> styles = new List<string>();
                nRet = GetClassFromStyles(
                    task_dom.DocumentElement,
                    out styles,
                    out strError);
                if (nRet == -1)
                    return -1;

                //
                foreach (string strBiblioDbName in biblio_dbnames)
                {
                    stop.SetMessage("正在计划任务 检索 " + strBiblioDbName + " ...");
                    string strQueryXml = "";
                    // 此处检索仅获得命中数即可
                    lRet = this.Channel.SearchBiblio(stop,
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

                    XmlNode node = task_dom.CreateElement("database");
                    task_dom.DocumentElement.AppendChild(node);

                    DomUtil.SetAttr(node, "name", strBiblioDbName);
                    DomUtil.SetAttr(node, "type", "biblio");
                    DomUtil.SetAttr(node, "count", lRet.ToString());

                    foreach (string strStyle in styles)
                    {
                        stop.SetMessage("正在计划任务 检索 " + strBiblioDbName + " " + strStyle + " ...");
                        // 此处检索仅获得命中数即可
                        lRet = this.Channel.SearchBiblio(stop,
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
                            if (this.Channel.ErrorCode == ErrorCode.FromNotFound)
                                continue;
                            return -1;
                        }
                        string strClassTableName = "class_" + strStyle;

                        XmlNode class_node = task_dom.CreateElement("class");
                        node.AppendChild(class_node);

                        DomUtil.SetAttr(class_node, "from_style", strStyle);
                        DomUtil.SetAttr(class_node, "class_table_name", strClassTableName);
                        DomUtil.SetAttr(class_node, "count", lRet.ToString());
                    }
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
                    stop,
                    this.Channel,
                    strEndDate,
                    LogType.OperLog,
                    out strError);
                if (lCount < 0)
                    return -1;

                DomUtil.SetAttr(task_dom.DocumentElement, "index", lCount.ToString());

                if (nRet == 1)
                {
                    // 记载第一个日志文件日期
                    DomUtil.SetAttr(task_dom.DocumentElement,
                        "first_operlog_date",
                        strFirstDate);

                    Program.MainForm.AppInfo.SetString(GetReportSection(),
                        "daily_report_end_date",
                        strFirstDate);
                    Program.MainForm.AppInfo.Save();   // 为防止程序中途崩溃丢失记忆，这里预先保存一下

                    XmlNode node = task_dom.CreateElement("operlog");
                    task_dom.DocumentElement.AppendChild(node);

                    DomUtil.SetAttr(node, "start_date", strFirstDate);  // "20060101"
                    DomUtil.SetAttr(node, "end_date", strEndDate + ":0-" + (lCount - 1).ToString());
                }
            }

            // *** 创建访问日志表
            if (strTypeList == "*"
                || StringUtil.IsInList("accesslog", strTypeList) == true)
            {
                string strFirstDate = "";
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetFirstOperLogDate(
                    LogType.AccessLog,
                    out strFirstDate,
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
                    stop,
                    this.Channel,
                    strEndDate,
                    LogType.AccessLog,
                    out strError);
                if (lCount == -1)
                    return -1;

                if (nRet == 1 && lCount >= 0)
                {
                    // 记载第一个访问日志文件日期
                    DomUtil.SetAttr(task_dom.DocumentElement,
                        "first_accesslog_date",
                        strFirstDate);

                    XmlNode node = task_dom.CreateElement("accesslog");
                    task_dom.DocumentElement.AppendChild(node);

                    DomUtil.SetAttr(node, "start_date", strFirstDate);  // "20060101"
                    DomUtil.SetAttr(node, "end_date", strEndDate + ":0-" + (lCount - 1).ToString());
                }
            }

            return 0;

        }

    }

#endif
}
