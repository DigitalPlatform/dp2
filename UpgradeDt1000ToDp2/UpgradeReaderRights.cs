using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

// using DigitalPlatform.CirculationClient.localhost;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 和 升级读者权限配置参数 有关的代码
    /// </summary>
    public partial class MainForm : Form
    {
        // 设置OPAC数据库定义
        // return:
        //      -1  error
        //      0   没有设置
        //      1   已经设置
        int SetOpacDatabaseDef(out string strError)
        {
            strError = "";

            List<string> biblio_dbnames = null;
            GetBiblioDbNames(out biblio_dbnames);

            if (biblio_dbnames.Count == 0)
                return 0;

            AppendHtml(
    "====================<br/>"
    + "设置OPAC数据库定义<br/>"
    + "====================<br/><br/>");


            string strDatabaseDef = "";
            int nRet = BuildOpacDatabaseDef(biblio_dbnames,
                out strDatabaseDef,
                out strError);
            if (nRet == -1)
                return -1;



            nRet = SetAllOpacDatabaseInfo(strDatabaseDef,
                out strError);
            if (nRet == -1)
                return -1;

            string strFormatDef = "";
            nRet = BuildOpacBrowseFormatDef(biblio_dbnames,
                out strFormatDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.SetAllOpacBrowseFormatsDef(strFormatDef,
                out strError);
            if (nRet == -1)
                return -1;


            return 1;
        }

        // 构造OPAC数据库定义的XML片段
        // 注意strDatabaseDef中返回的是下级片断定义，没有<virtualDatabases>元素作为根。
        int BuildOpacDatabaseDef(
            List<string> biblioDbnames,
            out string strDatabaseDef,
            out string strError)
        {
            strError = "";
            strDatabaseDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<virtualDatabases />");

            for (int i = 0; i < biblioDbnames.Count; i++)
            {
                string strName = biblioDbnames[i];

                XmlNode node = dom.CreateElement("database");
                dom.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "name", strName);
            }

            string strHtml = XmlToHtml(dom);
            AppendHtml(
"OPAC数据库定义如下XML片段如下:<br/>" + strHtml + "<br/><br/>");

            strDatabaseDef = dom.DocumentElement.InnerXml;

            return 0;
        }


        // 修改/设置全部OPAC数据库定义
        int SetAllOpacDatabaseInfo(string strDatabaseDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置全部OPAC数据库定义 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "opac",
                    "databases",
                    strDatabaseDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 构造OPAC记录显示格式定义的XML片段
        // 注意strFormatDef中返回的是下级片断定义，没有<browseformats>元素作为根。
        int BuildOpacBrowseFormatDef(
            List<string> biblioDbnames,
            out string strFormatDef,
            out string strError)
        {
            strError = "";
            strFormatDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<browseformats />");

            for (int i = 0; i < biblioDbnames.Count; i++)
            {
                string strDatabaseName = biblioDbnames[i];

                XmlNode database_node = dom.CreateElement("database");
                DomUtil.SetAttr(database_node, "name", strDatabaseName);

                dom.DocumentElement.AppendChild(database_node);

                // 包含一个普通书目格式节点
                {
                    XmlNode format_node = dom.CreateElement("format");
                    database_node.AppendChild(format_node);

                    DomUtil.SetAttr(format_node, "name", "详细");
                    DomUtil.SetAttr(format_node, "type", "biblio");
                }
            }

            string strHtml = XmlToHtml(dom);
            AppendHtml(
"OPAC数据库显示格式定义XML片段如下:<br/>" + strHtml + "<br/><br/>");


            strFormatDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 修改/设置全部OPAC记录显示格式定义
        int SetAllOpacBrowseFormatsDef(string strDatabaseDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置全部OPAC记录显示格式定义 ...");
            stop.BeginLoop();

            this.Update();
            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "opac",
                    "browseformats",
                    strDatabaseDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLtqxCfgFilename(out string strLtqxCfgFilename,
            out string strError)
        {
            strError = "";
            strLtqxCfgFilename = "";

            if (String.IsNullOrEmpty(this.textBox_gisIniFilePath.Text) == true)
            {
                strError = "因为没有指定gis*.ini配置文件的路径，所以无法获知ltqx*.cfg配置文件的路径";
                return 0;   // 无法获得gis.ini所在的路径
            }

            Debug.Assert(this.GisIniFilePath != "", "");

            StringBuilder s = new StringBuilder(255, 255);
            int nRet = API.GetPrivateProfileString("系统参数",
                "流通权限",
                "!!!null",
                s,
                255,
                this.GisIniFilePath);
            string strLine = s.ToString();
            if (nRet <= 0
                || strLine == "!!!null")
            {
                strError = " 文件中，没有配置[系统参数]流通权限=? 参数";
                return 0;   // not found
            }

            string strPath = Path.GetDirectoryName(this.textBox_gisIniFilePath.Text);
            strLtqxCfgFilename = PathUtil.MergePath(strPath, strLine);

            return 1;   // found
        }

        int UpgradeCalendar(out string strError)
        {
            strError = "";
            int nRet = 0;

            Debug.Assert(this.LtqxCfgFilePath != "", "本函数应当在UpgradeReaderRightsParam()之后调用");

            // 读出节假日
            List<string> days_1 = new List<string>();
            for (int i = 0; ; i++)
            {
                string strEntry = "日期" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("节假日",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                days_1.Add(strLine);
            }

            // 读出闭馆日期
            List<string> days_2 = new List<string>();
            for (int i = 0; ; i++)
            {
                string strEntry = "日期" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("闭馆日期",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                days_2.Add(strLine);
            }

            string strWeekEnds = "";
            {

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("闭馆日期",
                    "星期",
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                {

                }
                else
                {
                    strWeekEnds = strLine;
                }
            }

            // 当年年份
            DateTime now = DateTime.Now;
            string strYear = now.Year.ToString().PadLeft(4, '0');


            List<string> days = new List<string>();
            days.AddRange(days_1);
            days.AddRange(days_2);

            // 排序去重
            days.Sort();
            StringUtil.RemoveDup(ref days);

            string strWarning = "";
            // 把dt1000 ltqx(2k).cfg中4字符的日期升级为dp28字符的形态
            // return:
            //      -1  error
            //      0   succeed
            //      1   has warnings
            nRet = UpgradeDays(
                strYear,
                ref days,
                out strWarning);
            if (nRet == -1)
            {
                strError = strWarning;
                return -1;
            }

            if (nRet == 1)
            {
                AppendHtml(
                    "升级闭馆日期过程中有如下警告:<br/>" + HttpUtility.HtmlEncode(strWarning) + "<br/><br/>");
            }


            // 排除周末
            List<string> weekenddays = new List<string>();
            if (String.IsNullOrEmpty(strWeekEnds) == false)
            {

                // 将ltqx2k.cfg中
                // [闭馆日期]
                // 星期=06
                // 的字符串变换为dp2能接受的calendarstring形态
                // parameters:
                //      strSource   类似"06"这样的形态。0表示星期天
                nRet = GetWeekdayString(strWeekEnds,
                    strYear,
                    out weekenddays,
                    out strError);
                if (nRet == -1)
                    return -1;
                days.AddRange(weekenddays);
                // 再次排序去重
                days.Sort();
                StringUtil.RemoveDup(ref days);
            }

            string strCalendarContent = Global.MakeListString(days, ",");
            string strRange = strYear + "0101-" + strYear + "1231"; // 一年的范围

            // 创建到服务器
            // return:
            //      -1  出错
            //      0   成功
            nRet = SetCalendarContent(
                "overwrite",
                "基本日历",
                strRange,
                strCalendarContent,
                "从dt1000升级过来的开馆日历",
                out strError);
            if (nRet == -1)
                return -1;

            AppendHtml(
                "创建了 基本日历 如下:<br/>" + HttpUtility.HtmlEncode(strCalendarContent) + "<br/><br/>");


            return 0;
        }

        // 将ltqx2k.cfg中
        // [闭馆日期]
        // 星期=06
        // 的字符串变换为dp2能接受的calendarstring形态
        // parameters:
        //      strSource   类似"06"这样的形态。0表示星期天
        int GetWeekdayString(string strSource,
            string strYear,
            out List<string> days,
            out string strError)
        {
            strError = "";
            days = new List<string>();

            int nYear = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch (Exception ex)
            {
                strError = "strYear参数中年份字符串 '" + strYear + "' 不正确: " + ex.Message;
                return -1;
            }

            List<int> dayofweeks = new List<int>();
            for (int i = 0; i < strSource.Length; i++)
            {
                try
                {
                    int v = Convert.ToInt32(new string(strSource[i], 1));
                    if (v < 0 || v > 6)
                        continue;
                    dayofweeks.Add(v);
                }
                catch
                {
                    continue;
                }
            }

            DateTime start = new DateTime(nYear, 1, 1);
            for (int i = 0; ; i++)
            {

                if (dayofweeks.IndexOf((int)start.DayOfWeek) != -1)
                {
                    days.Add(DateTimeUtil.DateTimeToString8(start));
                }

                start = start + new TimeSpan(1, 0, 0, 0);   // 增量一天
                if (start.Year > nYear)
                    break;
            }

            return 0;
        }

        // 保存、创建、删除日历
        // return:
        //      -1  出错
        //      0   成功
        int SetCalendarContent(
            string strAction,
            string strName,
            string strRange,
            string strContent,
            string strComment,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对日历 '" + strName + "' 进行 " + strAction + " 操作 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                CalenderInfo info = new CalenderInfo();
                info.Name = strName;
                info.Range = strRange;
                info.Comment = strComment;
                info.Content = strContent;

                long lRet = Channel.SetCalendar(
                    stop,
                    strAction,
                    info,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);

            }

            return 0;
        ERROR1:
            return -1;
        }

        // 把dt1000 ltqx(2k).cfg中4字符的日期升级为dp2的8字符的形态
        // return:
        //      -1  error
        //      0   succeed
        //      1   has warnings
        static int UpgradeDays(
            string strYear,
            ref List<string> days,
            out string strWarning)
        {
            strWarning = "";

            if (strYear.Length != 4)
            {
                strWarning = "strYear参数值'" + strYear + "' 应为4字符";
                return -1;
            }

            for (int i = 0; i < days.Count; i++)
            {
                string strLine = days[i].Trim();
                if (strLine.Length == 4)
                {
                    days[i] = strYear + strLine;
                    continue;
                }
                else if (strLine.Length == 9)
                {
                    int nRet = strLine.IndexOf("-");
                    if (nRet == -1)
                    {
                        strWarning += "日期字符串 '" + strLine + "' 格式不正确，缺乏'-'; ";
                        days.RemoveAt(i);
                        i--;
                        continue;
                    }
                    string strLeft = strLine.Substring(0, nRet).Trim();
                    string strRight = strLine.Substring(nRet + 1).Trim();

                    if (strLeft.Length != 4)
                    {
                        strWarning += "日期字符串 '" + strLine + "' 格式不正确，左边部分应当为4字符; ";
                        days.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (strRight.Length != 4)
                    {
                        strWarning += "日期字符串 '" + strLine + "' 格式不正确，右边部分应当为4字符; ";
                        days.RemoveAt(i);
                        i--;
                        continue;
                    }
                    days[i] = strYear + strLeft + "-" + strYear + strRight;
                }
                else
                {
                    strWarning += "日期字符串 '" + strLine + "' 格式不正确，应为4或9字符; ";
                    days.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            if (String.IsNullOrEmpty(strWarning) == false)
                return 1;

            return 0;
        }

        // 获得gis2000.ini配置文件中的 图书停借原因 字符串列表
        int GetBookStateTypes(
            out List<string> book_states,
            out string strError)
        {
            strError = "";
            book_states = new List<string>();

            for (int i = 0; ; i++)
            {
                string strEntry = (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                int nRet = API.GetPrivateProfileString("图书停借原因",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.GisIniFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                book_states.Add(strLine);
            }

            return 0;
        }

        // 获得gis2000.ini配置文件中的 读者停借原因 字符串列表
        int GetReaderStateTypes(
            out List<string> reader_states,
            out string strError)
        {
            strError = "";
            reader_states = new List<string>();

            for (int i = 0; ; i++)
            {
                string strEntry = (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                int nRet = API.GetPrivateProfileString("读者停借原因",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.GisIniFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                reader_states.Add(strLine);
            }

            return 0;
        }

        // 升级种次号配置
        int UpgradeZhongcihaoParam(out string strError)
        {
            strError = "";

            int nRet = 0;

            // 将升级过来的每个书目库，得到其原始dt1000数据库名，
            // 然后对这些书目库获得dt1000的auto.cfg文件，进行分析，
            // 看对应的是哪些种次号库(或者叫前缀字符串)
            List<ZhongcihaoInfo> infos = new List<ZhongcihaoInfo>();
            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strDatabaseName = dtlp_item.Text;
                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                // 只关心书目库
                if (StringUtil.IsInList("书目库", strCreatingType) == false)
                    continue;

                ZhongcihaoInfo info = new ZhongcihaoInfo();
                info.BiblioDbName = strDatabaseName;
                if (StringUtil.IsInList("unimarc", strCreatingType) == true)
                    info.Syntax = "unimarc";
                else if (StringUtil.IsInList("usmarc", strCreatingType) == true)
                    info.Syntax = "usmarc";
                else
                    info.Syntax = "unimarc";

                infos.Add(info);
            }

            if (infos.Count == 0)
            {
                return 0;
            }

            AppendHtml(
    "====================<br/>"
    + "升级种次号参数<br/>"
    + "====================<br/><br/>");


            for (int i = 0; i < infos.Count; i++)
            {
                ZhongcihaoInfo info = infos[i];

                string strContent = "";
                // 从dt1000服务器某数据库的配置文件
                // return:
                //      -1  出错
                //      0   文件不存在
                //      1   成功
                nRet = GetDtlpDbCfgFile(info.BiblioDbName,
                    "auto.cfg",
                    out strContent,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    infos.RemoveAt(i);
                    i--;
                    continue;
                }

                string strZhongcihaoName = "";
                // 从auto.cfg文件内容中，找到种次号名
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetZhongcihaoName(strContent,
                    out strZhongcihaoName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                Debug.Assert(nRet == 1, "");

                Debug.Assert(String.IsNullOrEmpty(strZhongcihaoName) == false, "");
                if (String.IsNullOrEmpty(strZhongcihaoName) == true)
                {
                    infos.RemoveAt(i);
                    i--;
                    continue;
                }

                info.ZhongcihaoName = strZhongcihaoName;
            }

            if (infos.Count == 0)
            {
                AppendHtml("没有找到任何种次号名。<br/>");
                return 0;
            }

            // REDO_ZHONGCIHAO:

            string strZhongcihaoXml = "";
            List<string> zhongcihao_dbnames = null;

            // 创建<zhongcihao> XML定义片断。返回值不包含根元素
            nRet = BuildZhongcihaoXml(infos,
                out strZhongcihaoXml,
                out zhongcihao_dbnames,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> error_databasename = null;
            // 创建dp2系统的种次号库
            // parameters:
            //      error_databasename  类型不符合的、已经存在重名的数据库
            // return:
            //      -1  error
            //      0   suceed。不过error_databasename中可能返回因重名(并且类型不同)而未能创建的数据库名
            nRet = CreateDp2ZhongcihaoDatabases(zhongcihao_dbnames,
                out error_databasename,
                out strError);
            if (nRet == -1)
                return -1;

            if (error_databasename.Count > 0)
            {
                /*
                // 把infos中重名的换名，然后重做
                Debug.Assert(false, "尚未实现");
                goto REDO_ZHONGCIHAO;
                 * */
                strError = "种次号库名中出现和其他类型数据库重名的情况";
                return -1;
            }

            XmlDocument temp_dom = new XmlDocument();
            try
            {
                temp_dom.LoadXml("<zhongcihao />");
                temp_dom.DocumentElement.InnerXml = strZhongcihaoXml;
            }
            catch (Exception ex)
            {
                strError = "种次号XML设置到InnerXml时发生错误: " + ex.Message;
                return -1;
            }

            string strHtml = XmlToHtml(temp_dom);
            AppendHtml(
                "升级后的种次号定义XML片段如下:<br/>" + strHtml + "<br/><br/>");

            // 保存种次号定义
            // parameters:
            //      strZhongcihaoXml   脚本定义XML。注意，没有根元素
            nRet = SetZhongcihaoDef(strZhongcihaoXml,
                out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }

        // 保存种次号定义
        // parameters:
        //      strZhongcihaoXml   脚本定义XML。注意，没有根元素
        int SetZhongcihaoDef(string strZhongcihaoXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存种次号定义 ...");
            stop.BeginLoop();

            this.Update();
            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "zhongcihao",
                    strZhongcihaoXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        class ZhongcihaoInfo
        {
            public string BiblioDbName = "";
            public string ZhongcihaoName = "";
            public string Syntax = "";
        }

        // 创建<zhongcihao> XML定义片断。返回值不包含根元素
        int BuildZhongcihaoXml(List<ZhongcihaoInfo> infos,
            out string strXml,
            out List<string> zhongcihao_dbnames,
            out string strError)
        {
            strError = "";
            strXml = "";

            zhongcihao_dbnames = new List<string>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(
    "<zhongcihao>" +
    "    <nstable name=\"nstable\">" +
    "        <item prefix=\"unimarc\" uri=\"http://dp2003.com/UNIMARC\" />" +
    "        <item prefix=\"usmarc\" uri=\"http://www.loc.gov/MARC21/slim\" />" +
    "    </nstable>" +
    "</zhongcihao>");

            for (int i = 0; i < infos.Count; i++)
            {
                ZhongcihaoInfo info = infos[i];

                string strZhongcihaoName = info.ZhongcihaoName;

                Debug.Assert(String.IsNullOrEmpty(strZhongcihaoName) == false, "");

                XmlNode group_node = dom.CreateElement("group");
                dom.DocumentElement.AppendChild(group_node);

                DomUtil.SetAttr(group_node, "name", strZhongcihaoName + "组");
                DomUtil.SetAttr(group_node, "zhongcihaodb", strZhongcihaoName);

                zhongcihao_dbnames.Add(strZhongcihaoName);

                // 创建<database>元素
                for (int j = i; j < infos.Count; j++)
                {
                    ZhongcihaoInfo temp_info = infos[j];

                    if (temp_info.ZhongcihaoName == strZhongcihaoName)
                    {
                        string strDatabaseName = temp_info.BiblioDbName;
                        string strSyntax = temp_info.Syntax;

                        XmlNode database_node = dom.CreateElement("database");
                        group_node.AppendChild(database_node);  // 2008/10/23 changed

                        DomUtil.SetAttr(database_node, "name", strDatabaseName);
                        DomUtil.SetAttr(database_node, "leftfrom", "索书类号");

                        if (strSyntax == "unimarc")
                        {
                            string strPrefix = "unimarc";
                            DomUtil.SetAttr(database_node,
                                                "rightxpath",
                                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                            DomUtil.SetAttr(database_node,
                                "titlexpath",
                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='a']/text()");
                            DomUtil.SetAttr(database_node,
                                "authorxpath",
                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='f' or @code='g']/text()");
                        }
                        else if (strSyntax == "usmarc")
                        {
                            string strPrefix = "usmarc";

                            DomUtil.SetAttr(database_node,
                                "rightxpath",
                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                            DomUtil.SetAttr(database_node,
                                "titlexpath",
                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='a']/text()");
                            DomUtil.SetAttr(database_node,
                                "authorxpath",
                                "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='c']/text()");
                        }
                        else
                        {
                            strError = "目前暂时不能处理syntax为 '" + strSyntax + "' 的书目库节点创建...";
                            return -1;
                        }

                        if (j > i)  // 正好在i位置的，不删除。这样可以避免复杂化外层循环
                        {
                            infos.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }


            strXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        /*


[Actions]
entry1 = 010$ato102$b:102$*
entry2 = 010$ato210**:210$*
entry3 = 60Xlunpai:60***
entry4 = 200$fto7X1$a:701$a,711$a
entry5 = 200$gto7X2$a:702$*,712$*
entry6 = 225$ato410$a:410**,225**
entry7 = 69X$ato905$d:905$d
entry8 = 905$bto906$h:905$b,906**
entry9= zhongcihao:905$e
entry10= 960$lto905$b:905$b,960**
entry11 = AddIsbnPublisher:010$a
entry12= AddIsbn102:010$a
entry13= gaizhongcihao:905$e
entry14= gaidengluhao:905$b
entry15= 7XX$ato905$e:701$a,711$a,905$e

[200$fto7X1$a]
title = 7*1$a   <--    200$f
macro = {MoveTo 200$f}{CopySubfield}{moveto %%%$a|701**|711**}{ReplaceSubfield $a}

[200$gto7X2$a]
title = 7*2$a   <--    200$g
macro = {MoveTo 200$g}{CopySubfield}{moveto %%%$a|702**|712**}{ReplaceSubfield $a}

[69X$ato905$d]
title = 905$d   <--    69*$a
macro = {MoveTo 690$a|692$a|694$a}{CopySubField}{MoveTo %%%%%|905$d|905**}{ReplaceSubfield $d}

[60Xlunpai]
title = 60*            复制并轮排
macro = {At 60***}{GetPos X}{CopyField}{LunPai}{InsertField}

[010$ato210**]
title = 210     <--    010$a
macro = {Moveto 010$a}{CopySubField}{CutISBN}{SMDCvt ISBN}{Moveto %%%%%|210**}{InsertText}

[010$ato102$b]
title = 102$b   <--    010$a
macro = {Moveto 010$a}{CopySubField}{CutISBN}{SMDCvt 102}{Moveto %%%$b|102$b|102**}{ReplaceSubField $b}

[AddIsbn102]
title = 102$b    -->   辅助库(102)
macro = {Moveto 010$a}{CopySubField}{CutISBN}{CpyStr _Clipboard _1}{Moveto 102$b}{CopySubfield}{CpyStr _Clipboard _2}{SMDWrite _1 _2 102}

[AddIsbnPublisher]
title = 010$a    -->   辅助库(ISBN)
macro = {Moveto 010$a}{CopySubField}{CutISBN}{CpyStr _Clipboard _1}{Moveto 210$a}{CopySubfield}{CpyStr _Clipboard _2}{Moveto 210$c}{CopySubfield}{CatStr _Clipboard _2}{SMDWrite _1 _2 ISBN}

[905$bto906$h]
title = 906$h   <--    905$b
macro = {SepNumber 905$b 906$h}

[225$ato410$a]
title = 410$a   <--    225$a
macro = {MoveTo 225$a}{CopySubfield}{CpyStr _Clipboard _1}{CpyStr "\$12001 " _2}{CpyStr _2 _Clipboard}{CatStr _1 _Clipboard}{moveto %%%%%|410**}{InsertText}

[zhongcihao]
title = 905$e   自动加种次号
macro = {Moveto %%%%%|905$d}{CopySubField}{CpyStr _Clipboard _1}{SMDCvt 种次号}{IncNumber}{SMDWrite _1 _Clipboard 种次号}{Moveto %%%%%|905$e|905**}{ReplaceSubfield $e}

[gaizhongcihao]
title = 905$d$e  -->  辅助库(种次号)
macro = {Moveto %%%$d|905$d}{CopySubField}{CpyStr _Clipboard _1}{Moveto %%%$e|905$e}{CopySubfield}{CpyStr _Clipboard _2}{SMDWrite _1 _2 种次号}

[960$lto905$b]
title = 905$b<--960$l 自动加登录号
macro = {Moveto %%%%%|960$l}{CopySubField}{CpyStr _Clipboard _1}{CpyStr "登录号" _Clipboard}{SMDCvt 种次号}{CpyStr _Clipboard _3}{MakeRange _1 _Clipboard}{AddNumber _1 _3}{SMDWrite "登录号" _3 种次号}{Moveto 905$b|905**}{InsertSubfield $b}

[gaidengluhao]
title = 905$b  -->  辅助库(登录号)
macro = {Moveto %%%$b|905$b}{CopySubField}{CpyStr _Clipboard _1}{TailNumber _1}{IncNumber _1}{SMDWrite "登录号" _1 种次号}

[7XX$ato905$e]
title = 905$e<--7XX$a 自动加著者号
macro = {Moveto %%%$a|701**|711**}{CopySubField}{AuthNumber UTIL}{Moveto %%%$e|905$e}{ReplaceSubField $e}

[defaults]

entry1 = 200$a:this is 200 $a
entry2 = 200$f:this is 200 $f

[font]
//facename = Courier New
//Size = 10


         * */
        // 从auto.cfg文件内容中，找到种次号名
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetZhongcihaoName(string strContent,
            out string strZhongcihaoName,
            out string strError)
        {
            strError = "";
            strZhongcihaoName = "";

            // 创建临时文件
            string strTempFilename = PathUtil.MergePath(this.DataDir, "temp_auto.cfg");
            try
            {
                StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.GetEncoding(936));
                sw.Write(strContent);
                sw.Close();
            }
            catch (Exception ex)
            {
                strError = "文件 " + strTempFilename + " 创建过程发生错误: " + ex.Message;
                return -1;
            }

            // '[zhongcihao] macro=' section
            StringBuilder s = new StringBuilder(4096, 4096);
            int nRet = API.GetPrivateProfileString("zhongcihao",
                "macro",
                "!!!null",
                s,
                4096,
                strTempFilename);
            string strLine = s.ToString();
            if (nRet <= 0
                || strLine == "!!!null")
                return 0;   // not found

            // 找到"{SMDCvt 种次号}"片断
            nRet = strLine.IndexOf("{SMDCvt ", 0);
            if (nRet == -1)
                return 0;

            int nStart = nRet + "{SMDCvt ".Length;

            int nTail = strLine.IndexOf("}", nStart);
            if (nTail == -1)
                nTail = strLine.Length;

            strZhongcihaoName = strLine.Substring(nStart, nTail - nStart).Trim();
            if (strZhongcihaoName.Length > 50)
                return 0;   // 太长，可能是一个错误

            Debug.Assert(String.IsNullOrEmpty(strZhongcihaoName) == false, "");
            return 1;
        }


        int UpgradeReaderRightsParam(out string strError)
        {
            strError = "";

            int nRet = 0;

            AppendHtml(
                "====================<br/>"
                + "升级流通权限参数<br/>"
                + "====================<br/><br/>");


            this.LtqxCfgFilePath = "";
            if (this.textBox_rights_ltxqCfgFilePath.Text == "")
            {
                Debug.Assert(this.textBox_rights_ltqxCfgContent.Text != "", "ltqx*.cfg文件内容是必须有的");
                // 如果没有ltqx*.cfg文件名而有文件内容
                // 则将文件内容写入一个临时文件，以便GetPrivateProfileString() API能够执行
                this.LtqxCfgFilePath = PathUtil.MergePath(this.DataDir, "temp_ltqx.cfg");

                try
                {
                    StreamWriter sw = new StreamWriter(this.LtqxCfgFilePath, false, Encoding.GetEncoding(936));
                    sw.Write(this.textBox_rights_ltqxCfgContent.Text);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    strError = "文件 " + this.LtqxCfgFilePath + " 创建过程发生错误: " + ex.Message;
                    return -1;
                }
            }
            else
            {
                this.LtqxCfgFilePath = this.textBox_rights_ltxqCfgFilePath.Text;
            }

            // 读出全部读者类型
            List<string> reader_types = new List<string>();
            for (int i = 0; ; i++)
            {
                string strEntry = "类型" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("读者类型",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                reader_types.Add(strLine);
            }

            // 缺省读者类型
            string strDefaultReaderType = "";
            {
                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("读者类型",
                    "缺省",
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                {
                }
                else
                {
                    strDefaultReaderType = strLine;
                }

            }

            // 读者类型的第一个，为缺省类型
            reader_types.Insert(0, strDefaultReaderType);

            // 读出全部图书类型
            List<string> book_types = new List<string>();
            for (int i = 0; ; i++)
            {
                string strEntry = "类型" + (i + 1).ToString();

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("图书类型",
                    strEntry,
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                    break;

                book_types.Add(strLine);
            }


            // 缺省图书类型
            string strDefaultBookType = "";
            {
                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString("图书类型",
                    "缺省",
                    "!!!null",
                    s,
                    255,
                    this.LtqxCfgFilePath);
                string strLine = s.ToString();
                if (nRet <= 0
                    || strLine == "!!!null")
                {
                }
                else
                {
                    strDefaultBookType = strLine;
                }
            }

            // 图书类型的第一个，为缺省类型
            book_types.Insert(0, strDefaultBookType);


            XmlDocument rights_dom = new XmlDocument();
            rights_dom.LoadXml("<rightsTable/>");

            // 两层循环
            for (int i = 0; i < reader_types.Count; i++)
            {
                string strReaderType = reader_types[i];

                XmlNode reader_type_node = rights_dom.CreateElement("type");
                rights_dom.DocumentElement.AppendChild(reader_type_node);

                DomUtil.SetAttr(reader_type_node, "reader", (i == 0 ? "*" : strReaderType));

                // 可预约册数
                List<string> reserve_items = new List<string>();
                // 可借总册数
                List<string> borrow_items = new List<string>();

                for (int j = 0; j < book_types.Count; j++)
                {
                    string strBookType = book_types[j];

                    StringBuilder s = new StringBuilder(255, 255);
                    nRet = API.GetPrivateProfileString(strReaderType,
                        strBookType,
                        "!!!null",
                        s,
                        255,
                        this.LtqxCfgFilePath);
                    string strLine = s.ToString();

                    string strParamLine = "";

                    if (nRet <= 0
                        || strLine == "!!!null")
                    {
                    }
                    else
                    {
                        strParamLine = strLine;
                    }

                    string strMaxBorrowItems = "";
                    string strBorrowPeriod = "";
                    string strRenewPeriod = "";
                    string strMaxReserveItems = "";
                    string strReserveDays = "";
                    string strOverduePrice = "";
                    string strLeasePrice = "";
                    string strSpecialBorrowDays = "";
                    string strStopBorrowDays = "";


                    nRet = ParseRightsLine(strParamLine,
                        out strMaxBorrowItems,
                        out strBorrowPeriod,
                        out strRenewPeriod,
                        out strMaxReserveItems,
                        out strReserveDays,
                        out strOverduePrice,
                        out strLeasePrice,
                        out strSpecialBorrowDays,
                        out strStopBorrowDays,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    reserve_items.Add(strMaxReserveItems);
                    borrow_items.Add(strMaxBorrowItems);

                    XmlNode book_type_node = rights_dom.CreateElement("type");
                    reader_type_node.AppendChild(book_type_node);

                    DomUtil.SetAttr(book_type_node, "book", (j == 0 ? "*" : strBookType));

                    // 可借册数
                    XmlNode param_node = rights_dom.CreateElement("param");
                    book_type_node.AppendChild(param_node);
                    DomUtil.SetAttr(param_node, "name", "可借册数");
                    DomUtil.SetAttr(param_node, "value", strMaxBorrowItems);

                    // 借期
                    string strValue = strBorrowPeriod + "day," + strRenewPeriod + "day";
                    param_node = rights_dom.CreateElement("param");
                    book_type_node.AppendChild(param_node);
                    DomUtil.SetAttr(param_node, "name", "借期");
                    DomUtil.SetAttr(param_node, "value", strValue);

                    // 超期违约金因子
                    double dValue = 0;
                    try
                    {
                        dValue = Convert.ToDouble(strOverduePrice);
                    }
                    catch
                    {
                    }
                    strValue = "CNY" + (dValue / 100).ToString() + "/day";  // 2008/10/10 changed
                    param_node = rights_dom.CreateElement("param");
                    book_type_node.AppendChild(param_node);
                    DomUtil.SetAttr(param_node, "name", "超期违约金因子");
                    DomUtil.SetAttr(param_node, "value", strValue);

                    // 丢失违约金因子
                    // 暂时设置为10倍，需要提醒升级者最后自行修改
                    strValue = "10.0";
                    param_node = rights_dom.CreateElement("param");
                    book_type_node.AppendChild(param_node);
                    DomUtil.SetAttr(param_node, "name", "丢失违约金因子");
                    DomUtil.SetAttr(param_node, "value", strValue);


                }

                // 可预约册数取reserve_items最大值
                XmlNode reader_param_node = rights_dom.CreateElement("param");
                reader_type_node.InsertBefore(reader_param_node, reader_type_node.FirstChild);
                DomUtil.SetAttr(reader_param_node, "name", "可预约册数");
                DomUtil.SetAttr(reader_param_node, "value", GetMaxValue(reserve_items).ToString());

                // 可借总册数取borrow_items最大值
                reader_param_node = rights_dom.CreateElement("param");
                reader_type_node.InsertBefore(reader_param_node, reader_type_node.FirstChild);
                DomUtil.SetAttr(reader_param_node, "name", "可借总册数");
                DomUtil.SetAttr(reader_param_node, "value", GetMaxValue(borrow_items).ToString());

                // 工作日历名
                // 暂时设置为“基本日历”，需要提醒升级者最后自行修改
                reader_param_node = rights_dom.CreateElement("param");
                reader_type_node.InsertBefore(reader_param_node, reader_type_node.FirstChild);
                DomUtil.SetAttr(reader_param_node, "name", "工作日历名");
                DomUtil.SetAttr(reader_param_node, "value", "基本日历");

            }

            /*
            XmlViewerForm view = new XmlViewerForm();
            view.MainForm = this;
            view.XmlString = rights_dom.OuterXml;
            view.StartPosition = FormStartPosition.CenterScreen;
            view.ShowDialog(this);
             * */

            // 加入<readerTypes>和<bookTypes>
            // rights_dom
            XmlNode nodeContainer = rights_dom.CreateElement("readerTypes");
            rights_dom.DocumentElement.AppendChild(nodeContainer);
            for (int i = 1; i < reader_types.Count; i++)
            {
                XmlNode node = rights_dom.CreateElement("item");
                nodeContainer.AppendChild(node);
                node.InnerText = reader_types[i];
            }

            nodeContainer = rights_dom.CreateElement("bookTypes");
            rights_dom.DocumentElement.AppendChild(nodeContainer);
            for (int i = 1; i < book_types.Count; i++)
            {
                XmlNode node = rights_dom.CreateElement("item");
                nodeContainer.AppendChild(node);
                node.InnerText = book_types[i];
            }

            /*
            // reader types
            XmlDocument reader_type_dom = new XmlDocument();
            reader_type_dom.LoadXml("<readertypes/>");

            for (int i = 1; i < reader_types.Count; i++)
            {
                XmlNode node = reader_type_dom.CreateElement("item");
                reader_type_dom.DocumentElement.AppendChild(node);
                node.InnerText = reader_types[i];
            }

            string strHtml = XmlToHtml(reader_type_dom);
            AppendHtml(
                "升级后的读者类型XML片段如下:<br/>" + strHtml + "<br/><br/>");

            // book types
            XmlDocument book_type_dom = new XmlDocument();
            book_type_dom.LoadXml("<booktypes/>");

            for (int i = 1; i < book_types.Count; i++)
            {
                XmlNode node = book_type_dom.CreateElement("item");
                book_type_dom.DocumentElement.AppendChild(node);
                node.InnerText = book_types[i];
            }

            strHtml = XmlToHtml(book_type_dom);
            AppendHtml(
                "升级后的图书类型XML片段如下:<br/>" + strHtml + "<br/><br/>");
             * */


            nRet = SaveRightsTable(rights_dom.DocumentElement.InnerXml,
                //reader_type_dom.DocumentElement.InnerXml,
                //book_type_dom.DocumentElement.InnerXml,
                out strError);
            if (nRet == -1)
                return -1;

            // 显示升级后的权限配置XML
            string strHtml = XmlToHtml(rights_dom);

            AppendHtml(
                "升级后的读者权限XML片段如下:<br/>" + strHtml + "<br/><br/>");

            // 显示读者权限表 rightable.aspx
            nRet = GetRightsTableHtml(
                out strHtml,
                out strError);
            if (nRet == -1)
                return -1;
            AppendHtml(
                "读者权限对照表:<br/>" + strHtml + "<br/><br/>");

            bool bHasBlankLocation = false; // 是否含有空的馆藏地点字符串?

            // 累积起来的馆藏位置值列表
            List<string> origin_book_locations = null;
            // 把Hashtable中的key转储到List<string>中
            PutValueTable(this.m_locations,
                out origin_book_locations,
                out bHasBlankLocation);

            /*
            List<string> origin_book_locations = new List<string>();
            foreach (string key in this.m_locations.Keys)
            {
                string strValue = key.Trim();
                if (String.IsNullOrEmpty(strValue) == true)
                    bHasBlankLocation = true;

                origin_book_locations.Add(strValue);
            }
             * */

            origin_book_locations.Sort();

            List<string> book_states = null;
            // 获得gis2000.ini配置文件中的 图书停借原因 字符串列表
            nRet = GetBookStateTypes(
                out book_states,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> reader_states = null;
            // 获得gis2000.ini配置文件中的 读者停借原因 字符串列表
            nRet = GetReaderStateTypes(
                out reader_states,
                out strError);
            if (nRet == -1)
                return -1;

            // 允许编辑馆藏地点
            LocationStringDialog dlg = new LocationStringDialog();
            dlg.Text = "馆藏地点列表";
            dlg.Comment = "这里是从册信息中累积获得的馆藏地点。请编辑修改。将被用来作为dp2系统中辅助输入的事项列表。";
            dlg.Locations = origin_book_locations;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            // 用于<valueTable>的馆藏地点
            List<string> input_book_locations = new List<string>();

            if (dlg.DialogResult == DialogResult.OK)
                input_book_locations = dlg.Locations;


            // 对馆藏地点数组去除空字符串项
            List<string> book_locations_1 = new List<string>();
            book_locations_1.AddRange(input_book_locations);

            RemoveEmpty(ref book_locations_1);

            /*
            nRet = SaveValueTable(reader_types,
                book_types,
                book_locations_1,
                book_states,
                reader_states,
                out strError);
            if (nRet == -1)
                return -1;
             * */
            nRet = SaveValueTable("readerType",
                "读者类型值列表",
                reader_types,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SaveValueTable("bookType",
                "图书类型值列表",
                book_types,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SaveValueTable("location",
                "馆藏位置值列表",
                book_locations_1,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SaveValueTable("state",
                "图书类型值列表",
                book_states,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SaveValueTable("readerState",
                "读者状态值列表",
                reader_states,
                out strError);
            if (nRet == -1)
                return -1;

            // 让操作者编辑馆藏地点表
            if (origin_book_locations.Count > 0)
            {
                List<string> borrow_book_locations = new List<string>();

                if (bHasBlankLocation == true)
                    borrow_book_locations.Add("");

                // 没有采用最原始的列表，而是采用编辑后的列表
                borrow_book_locations.AddRange(book_locations_1);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<locationTypes />");
                for (int i = 0; i < borrow_book_locations.Count; i++)
                {
                    XmlNode node = dom.CreateElement("item");
                    dom.DocumentElement.AppendChild(node);
                    node.InnerText = borrow_book_locations[i];
                }

                LocationCanBorrowDialog location_dlg = new LocationCanBorrowDialog();

                location_dlg.Comment = "这里是从数据中累积获得的馆藏地点事项，不一定全面，或需要增补新项。另外请配置它们关于是否允许外借的特性。";
                location_dlg.Xml = dom.DocumentElement.OuterXml;

                location_dlg.StartPosition = FormStartPosition.CenterScreen;
                location_dlg.ShowDialog(this);

                if (location_dlg.DialogResult == DialogResult.OK)
                {
                    try
                    {
                        dom.LoadXml(location_dlg.Xml);
                    }
                    catch (Exception ex)
                    {
                        strError = "经过编辑的locationTypes XML装入DOM时失败: " + ex.Message;
                        return -1;
                    }
                    // 修改/设置全部馆藏地定义<locationTypes>
                    // paramters:
                    //      strLocationDef  没有根
                    nRet = SetAllLocationTypesInfo(dom.DocumentElement.InnerXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strHtml = XmlToHtml(dom);
                    AppendHtml(
                        "升级后的馆藏地点可否外借定义XML片段如下:<br/>" + strHtml + "<br/><br/>");

                }
            }

            // 累积起来的资金来源值列表
            bool bHasBlankValue = false;

            if (this.m_sources.Count > 0)
            {
                List<string> origin_sources = null;
                // 把Hashtable中的key转储到List<string>中
                PutValueTable(this.m_sources,
                    out origin_sources,
                    out bHasBlankValue);
                origin_sources.Sort();

                LocationStringDialog value_dlg = new LocationStringDialog();
                value_dlg.Text = "资金来源列表";
                value_dlg.Comment = "这里是从采购信息中累积获得的资金来源列表。请编辑修改。将被用来作为dp2系统中辅助输入的事项列表。";
                value_dlg.Locations = origin_sources;
                value_dlg.StartPosition = FormStartPosition.CenterScreen;

                value_dlg.ShowDialog(this);
                if (value_dlg.DialogResult == DialogResult.OK)
                {
                    origin_sources = value_dlg.Locations;

                    RemoveEmpty(ref origin_sources);

                    if (origin_sources.Count > 0)
                    {
                        nRet = SaveValueTable("orderSource",
                            "资金来源值列表",
                            origin_sources,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

            if (this.m_sellers.Count > 0)
            {
                List<string> origin_sellers = null;
                // 把Hashtable中的key转储到List<string>中
                PutValueTable(this.m_sellers,
                    out origin_sellers,
                    out bHasBlankValue);
                origin_sellers.Sort();

                LocationStringDialog value_dlg = new LocationStringDialog();
                value_dlg.Text = "书商名称列表";
                value_dlg.Comment = "这里是从采购信息中累积获得的书商名称列表。请编辑修改。将被用来作为dp2系统中辅助输入的事项列表。";
                value_dlg.Locations = origin_sellers;
                value_dlg.StartPosition = FormStartPosition.CenterScreen;

                value_dlg.ShowDialog(this);
                if (value_dlg.DialogResult == DialogResult.OK)
                {

                    origin_sellers = value_dlg.Locations;

                    RemoveEmpty(ref origin_sellers);

                    if (origin_sellers.Count > 0)
                    {
                        nRet = SaveValueTable("orderSeller",
                            "书商名称值列表",
                            origin_sellers,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

            }

            if (this.m_orderclasses.Count > 0)
            {
                List<string> origin_classes = null;
                // 把Hashtable中的key转储到List<string>中
                PutValueTable(this.m_orderclasses,
                    out origin_classes,
                    out bHasBlankValue);
                origin_classes.Sort();

                LocationStringDialog value_dlg = new LocationStringDialog();
                value_dlg.Text = "采购类别列表";
                value_dlg.Comment = "这里是从采购信息中累积获得的采购类别列表。请编辑修改。将被用来作为dp2系统中辅助输入的事项列表。";
                value_dlg.Locations = origin_classes;
                value_dlg.StartPosition = FormStartPosition.CenterScreen;

                value_dlg.ShowDialog(this);

                if (value_dlg.DialogResult == DialogResult.OK)
                {

                    origin_classes = value_dlg.Locations;

                    RemoveEmpty(ref origin_classes);

                    if (origin_classes.Count > 0)
                    {
                        nRet = SaveValueTable("orderClass",
                            "采购类别值列表",
                            origin_classes,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

            }

            return 0;
        }

        // 把Hashtable中的key转储到List<string>中
        static void PutValueTable(Hashtable table,
            out List<string> list,
            out bool bHasBlankValue)
        {
            list = new List<string>();
            bHasBlankValue = false;

            foreach (string key in table.Keys)
            {
                string strValue = key.Trim();
                if (String.IsNullOrEmpty(strValue) == true)
                    bHasBlankValue = true;

                list.Add(strValue);
            }
        }

        // 去掉数组中的空字符串值
        static void RemoveEmpty(ref List<String> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (String.IsNullOrEmpty(list[i]) == true)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        public void AppendHtml(string strText)
        {
            Global.WriteHtml(this.webBrowser_info,
                strText);
            API.PostMessage(this.Handle, WM_SCROLLHTMLTOEND, 0, 0);
        }

        static string XmlToHtml(XmlDocument dom)
        {
            string strHtml = DomUtil.GetIndentXml(dom);
            strHtml = HttpUtility.HtmlEncode(strHtml);
            return strHtml.Replace(" ", "&nbsp;").Replace("\r\n", "<br/>");
        }

        int GetRightsTableHtml(
            out string strHtml,
            out string strError)
        {
            strError = "";
            strHtml = "";

            if (String.IsNullOrEmpty(this.Channel.Url) == true)
            {
                strError = "尚未经过 输入 dp2library 服务器信息 属性页，无法操作服务器";
                return -1;
            }


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取读者权限参数表HTML ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                long lRet = Channel.GetSystemParameter(stop,
                    "circulation",
                    "rightsTableHtml",
                    out strHtml,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        int SaveRightsTable(string strRightsTableXml,
            //string strReaderTypeXml,
            //string strBookTypeXml,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.Channel.Url) == true)
            {
                strError = "尚未经过 输入 dp2library 服务器信息 属性页，无法操作服务器";
                return -1;
            }


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者权限参数 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                // <rightsTable>
                long lRet = Channel.SetSystemParameter(stop,
                    "circulation",
                    "rightsTable",
                    strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                /*
                // <readertypes>
                lRet = Channel.SetSystemParameter(stop,
                    "circulation",
                    "readerTypes",
                    strReaderTypeXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // <booktypes>
                lRet = Channel.SetSystemParameter(stop,
                    "circulation",
                    "bookTypes",
                    strBookTypeXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                */
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 保存单个值列表
        int SaveValueTable(
            string strName,
            string strDisplayName,
            List<string> list,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.Channel.Url) == true)
            {
                strError = "尚未经过 输入 dp2library 服务器信息 属性页，无法操作服务器";
                return -1;
            }

            //
            string strXml = "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", strName);
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(list, ",");

            strXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            string strHtml = XmlToHtml(dom);
            AppendHtml(
                strDisplayName + "XML片段如下:<br/>" + strHtml + "<br/><br/>");

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存" + strDisplayName + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                long lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        ERROR1:
            return -1;
        }

        /*
        int SaveValueTable(
            List<string> reader_types,
            List<string> book_types,
            List<string> book_locations,
            List<string> book_states,
            List<string> reader_states,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.Channel.Url) == true)
            {
                strError = "尚未经过 输入 dp2library 服务器信息 属性页，无法操作服务器";
                return -1;
            }

            string strReaderTypeXml = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", "readerType");
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(reader_types, ",");

            strReaderTypeXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            string strHtml = XmlToHtml(dom);
            AppendHtml(
                "读者类型值列表XML片段如下:<br/>" + strHtml + "<br/><br/>");


            // 
            string strBookTypeXml = "";

            dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", "bookType");
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(book_types, ",");

            strBookTypeXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            strHtml = XmlToHtml(dom);
            AppendHtml(
                "图书类型值列表XML片段如下:<br/>" + strHtml + "<br/><br/>");

            //
            string strBookLocationXml = "";
            dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", "location");
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(book_locations, ",");

            strBookLocationXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            strHtml = XmlToHtml(dom);
            AppendHtml(
                "馆藏位置值列表XML片段如下:<br/>" + strHtml + "<br/><br/>");


            //
            string strBookStatesXml = "";
            dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", "state");
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(book_states, ",");

            strBookStatesXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            strHtml = XmlToHtml(dom);
            AppendHtml(
                "册状态值列表XML片段如下:<br/>" + strHtml + "<br/><br/>");


            //
            string strReaderStatesXml = "";
            dom = new XmlDocument();
            dom.LoadXml("<table />");

            DomUtil.SetAttr(dom.DocumentElement, "name", "readerState");
            DomUtil.SetAttr(dom.DocumentElement, "dbname", "");

            dom.DocumentElement.InnerText = Global.MakeListString(reader_states, ",");

            strReaderStatesXml = dom.DocumentElement.OuterXml;

            // 显示值列表
            strHtml = XmlToHtml(dom);
            AppendHtml(
                "读者状态值列表XML片段如下:<br/>" + strHtml + "<br/><br/>");


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者和图书类型值列表、馆藏位置参数 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                // <rightstable>
                long lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strReaderTypeXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // <readertypes>
                lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strBookTypeXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strBookLocationXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strBookStatesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;


                lRet = Channel.SetSystemParameter(stop,
                    "valueTable",
                    "overwrite",
                    strReaderStatesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        ERROR1:
            return -1;
        }
         * */

        // 修改/设置全部馆藏地定义<locationTypes>
        // paramters:
        //      strLocationDef  没有根
        int SetAllLocationTypesInfo(string strLocationDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置<locationTypes>定义 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "locationTypes",
                    strLocationDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 取最大值
        static int GetMaxValue(List<string> values)
        {
            int nMaxValue = 0;
            for (int i = 0; i < values.Count; i++)
            {
                int nValue = 0;
                try
                {
                    nValue = Convert.ToInt32(values[i]);
                }
                catch
                {
                }
                if (nMaxValue < nValue)
                    nMaxValue = nValue;
            }

            return nMaxValue;
        }

        // parameters:
        //      strMaxBorrowItems   (特定读者类型/图书类型的)可借册数
        //      strBorrowPeriod     借期天数
        //      strRenewPeriod      续借天数
        //      strMaxReserveItems  可预约册数
        //      strReserveDays      预约书到馆后保留的天数
        //      strOverduePrice     借书过期一天的罚款金额（分）
        //      strLeasePrice       租借图书每天租金（分），为0表示正常借阅
        //      strSpecialBorrowDays    特殊借阅的借期(天数)，为0表示不能进行特殊借阅
        //      strStopBorrowDays   以停代罚天数。为0表示必须交罚金才能恢复借阅
        static int ParseRightsLine(string strLine,
            out string strMaxBorrowItems,
            out string strBorrowPeriod,
            out string strRenewPeriod,
            out string strMaxReserveItems,
            out string strReserveDays,
            out string strOverduePrice,
            out string strLeasePrice,
            out string strSpecialBorrowDays,
            out string strStopBorrowDays,
            out string strError)
        {
            strError = "";

            strMaxBorrowItems = "";
            strBorrowPeriod = "";
            strRenewPeriod = "";
            strMaxReserveItems = "";
            strReserveDays = "";
            strOverduePrice = "";
            strLeasePrice = "";
            strSpecialBorrowDays = "";
            strStopBorrowDays = "";


            string[] parts = strLine.Split(new char[] { ',' });

            // 可借册数
            if (parts.Length > 0)
                strMaxBorrowItems = parts[0].Trim();

            // 借期天数
            if (parts.Length > 1)
                strBorrowPeriod = parts[1].Trim();

            // 续借天数
            if (parts.Length > 2)
                strRenewPeriod = parts[2].Trim();

            // 可预约册数
            if (parts.Length > 3)
                strMaxReserveItems = parts[3].Trim();


            // 预约书到馆后保留的天数。
            if (parts.Length > 4)
                strReserveDays = parts[4].Trim();

            // 借书过期一天的罚款金额（分）。
            if (parts.Length > 5)
                strOverduePrice = parts[5].Trim();

            // 租借图书每天租金（分），为0表示正常借阅。
            if (parts.Length > 6)
                strLeasePrice = parts[6].Trim();

            // 特殊借阅的借期(天数)，为0表示不能进行特殊借阅。
            if (parts.Length > 7)
                strSpecialBorrowDays = parts[7].Trim();

            // 以停代罚天数。为0表示必须交罚金才能恢复借阅
            if (parts.Length > 8)
                strStopBorrowDays = parts[8].Trim();

            return 0;
        }

        int LoadLtqxCfgFileContent(string strLtqxCfgFilePath,
out string strError)
        {
            strError = "";

            try
            {

                StreamReader sr = new StreamReader(strLtqxCfgFilePath,
                    Encoding.GetEncoding(936));

                this.textBox_rights_ltqxCfgContent.Text = sr.ReadToEnd();

                sr.Close();
            }
            catch (Exception ex)
            {
                strError = "打开或读入文件 " + strLtqxCfgFilePath + " 时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }
    }
}
