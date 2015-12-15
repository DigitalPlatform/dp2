using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;
using System.Diagnostics;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.OPAC.Server;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 显示统计xml内容的控件
    /// </summary>
    [ToolboxData("<{0}:StatisViewControl runat=server></{0}:StatisViewControl>")]
    public class StatisViewControl : WebControl, INamingContainer
    {
        public bool IsRange = true;

        public string Xml = "";

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.StatisViewControl.cs",
                typeof(StatisViewControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        // 特性版本
        // 如果没有找到，就返回空
        public string GetStringEx(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            try
            {

                return GetRm().GetString(strID, ci);
            }
            catch
            {
                return null;
            }
        }

        public RangeStatisInfo RangeStatisInfo = null;
        public string DateRange = "";
        public string XmlFilename = "";

        XmlDocument dom = null;

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        /*
         * 最简形式
<root>
  <category name="*" role="all" >
  </category>
</root>
         * 一般形式
<root>
  <category name="超期通知" role="reader" >
    <item name="dpmail超期通知人数" role="reader" />
    <item name="email超期通知人数" role="all" />
  </category>
  <category name="出纳" role="all" >
    <item name="读者数" role="all" />
    <item name="借册" role="all" />
    <item name="借书遇册条码号重复次数" role="all" />
    <item name="还册" role="all" />
    <item name="还超期册" role="all" />
    <item name="当日内立即还册" role="all" />
    <item name="还书遇册条码号重复次数" role="all" />
    <item name="还书遇册条码号重复并无读者证条码号辅助判断次数" role="all" />
    <item name="预约次" role="all" />
    <item name="预约到书册" role="all" />
  </category>
  <category name="修改读者信息" role="all" >
    <item name="创建新记录数" role="all" />
    <item name="修改记录数" role="all" />
    <item name="删除记录数" role="all" />
  </category>
  <category name="违约金" role="all">
    <item name="给付次" role="all" />
    <item name="给付元" role="all" />
    <item name="取消次" role="all" />
    <item name="取消元" role="all" />
  </category>
  <category name="跟踪DTLP" role="all" >
    <item name="覆盖记录条数" role="all" />
    <item name="删除记录条数" role="all" />
  </category>
  <category name="修改读者信息之状态" role="all" >
    <item name="挂失" role="all" />
    <item name="注销" role="all" />
    <item name="" role="all" />
  </category>
  <category name="修复借阅信息" role="all" >
    <item name="读者侧次数" role="all" />
    <item name="实体侧次数" role="all" />
  </category>
  <category name="消息监控" role="all" >
    <item name="删除过期消息条数" role="all" />
  </category>
</root>
         **/

        // 利用模板文件过滤统计事项，并将事项按照模板顺序排序
        // parameters:
        //      strLibraryCode  图书馆代码。如果为""，表示察看根元素下的<category>
        static int FilterXmlFile(
            string strLibraryCode,
            string strRole, // notlogin public reader librarian
            string strSourceXml,
            string strTemplateFilename,
            bool bRemoveZeroValueItems,
            out string strTargetXml,
            // out List<string> exist_codes,
            out string strError)
        {
            strError = "";
            strTargetXml = "";
            // exist_codes = new List<string>();

            XmlDocument dom_data = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strSourceXml) == true)
                    dom_data.LoadXml("<root />");
                else
                    dom_data.LoadXml(strSourceXml);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载数据XML字符串到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

#if NO
            {
                // 列出全部<library>元素的code属性值
                XmlNodeList library_nodes = dom_data.DocumentElement.SelectNodes("library");
                foreach (XmlNode node in library_nodes)
                {
                    string strCode = DomUtil.GetAttr(node, "code");
                    exist_codes.Add(strCode);
                }
            }
#endif

            XmlDocument dom_template = new XmlDocument();
            try
            {
                dom_template.Load(strTemplateFilename);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载模板XML文件 " + strTemplateFilename + " 到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            // 确保dom_template的根存在
            if (dom_template.DocumentElement == null)
            {
                dom_template.LoadXml("<root />");
            }

            string strWildMatchCategoryRole = "";
            // 找到模板里面的通配符<category>元素
            XmlNode wildmatch_category = dom_template.DocumentElement.SelectSingleNode("category[@name='*']");
            if (wildmatch_category != null)
                strWildMatchCategoryRole = DomUtil.GetAttr(wildmatch_category, "role");

            XmlNodeList data_categorys = null;
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                data_categorys = dom_data.DocumentElement.SelectNodes("category");
            else
                data_categorys = dom_data.DocumentElement.SelectNodes("library[@code='" + strLibraryCode + "']/category");

            for (int i = 0; i < data_categorys.Count; i++)
            {
                XmlNode data_category = data_categorys[i];

                string strDataCategoryName = DomUtil.GetAttr(data_category, "name");

                bool bCategoryAllowed = false;  // <catagory>是否被通配符事项所允许?

                // 看看这个名字在dom_template中是否存在
                XmlNode template_category = dom_template.DocumentElement.SelectSingleNode("category[@name='" + strDataCategoryName + "']");
                if (template_category == null)
                {
                    // 看看有没有<catagory name="*">定义。
                    // 没有定义，就不被允许
                    if (String.IsNullOrEmpty(strWildMatchCategoryRole) == false)
                    {
                        // 看看这个事项是否允许？
                        if (StringUtil.IsInList("all", strWildMatchCategoryRole) == true
                            || StringUtil.IsInList(strRole, strWildMatchCategoryRole) == true)
                        {
                            // 如果不存在，就创建一个
                            template_category = dom_template.CreateElement("category");
                            dom_template.DocumentElement.AppendChild(template_category);
                            DomUtil.SetAttr(template_category, "name", strDataCategoryName);
                            bCategoryAllowed = true;
                        }
                    }

                }
                else
                {
                    string strTemplateRoles = DomUtil.GetAttr(template_category, "role");
                    if (String.IsNullOrEmpty(strTemplateRoles) == false)
                    {
                        // 看看这个目录是否允许？
                        if (StringUtil.IsInList("all", strTemplateRoles) == false
                            && StringUtil.IsInList(strRole, strTemplateRoles) == false)
                        {
                            template_category.ParentNode.RemoveChild(template_category);
                            continue;   // 整个<category>不被允许出现
                        }
                    }
                    else
                    {
                        template_category.ParentNode.RemoveChild(template_category);
                        continue;   // 整个<category>不被允许出现
                    }

                    // 清除<category>元素中的role属性
                    DomUtil.SetAttr(template_category, "role", null);

                    Debug.Assert(template_category.Attributes["role"] == null, "");
                }

                string strWildMatchItemRole = "";

                // 找到模板里面的通配符<item>元素
                if (bCategoryAllowed == true)
                    strWildMatchItemRole = strWildMatchCategoryRole;    // 继承<category name='*'>的role属性
                else if (template_category != null)
                {
                    XmlNode wildmatch_item = template_category.SelectSingleNode("item[@name='*']");
                    if (wildmatch_item != null)
                        strWildMatchItemRole = DomUtil.GetAttr(wildmatch_item, "role");
                }
                else
                {
                    strWildMatchItemRole = strWildMatchCategoryRole;    // 继承<category name='*'>的role属性
                }

                XmlNodeList data_items = data_category.SelectNodes("item");
                for (int j = 0; j < data_items.Count; j++)
                {
                    XmlNode data_item = data_items[j];

                    string strDataItemName = DomUtil.GetAttr(data_item, "name");
                    string strDataItemValue = DomUtil.GetAttr(data_item, "value");

                    XmlNode template_item = null;
                    // 看看这个名字在DOM2中是否存在
                    if (template_category != null)
                        template_item = template_category.SelectSingleNode("item[@name='" + strDataItemName + "']");

                    if (template_item == null)
                    {
                        // 看看有没有<item name="*">定义
                        // 没有定义，就不被允许
                        if (String.IsNullOrEmpty(strWildMatchItemRole) == false)
                        {
                            // 看看这个事项是否允许？
                            if (StringUtil.IsInList("all", strWildMatchItemRole) == true
                                || StringUtil.IsInList(strRole, strWildMatchItemRole) == true)
                            {
                                // 如果不存在，就创建一个
                                template_item = dom_template.CreateElement("item");
                                template_category.AppendChild(template_item);
                                DomUtil.SetAttr(template_item, "name", strDataItemName);
                                DomUtil.SetAttr(template_item, "value", strDataItemValue);
                            }
                        }
                    }
                    else
                    {
                        string strItemRoles = DomUtil.GetAttr(template_item, "role");
                        if (String.IsNullOrEmpty(strItemRoles) == false)
                        {
                            // 看看这个事项是否允许？
                            if (StringUtil.IsInList("all", strItemRoles) == false
                                && StringUtil.IsInList(strRole, strItemRoles) == false)
                            {
                                template_item.ParentNode.RemoveChild(template_item);
                                continue;   // <item>不被允许出现
                            }
                        }
                        else
                        {
                            template_item.ParentNode.RemoveChild(template_item);
                            continue;   // <item>不被允许出现
                        }

                        DomUtil.SetAttr(template_item, "value", strDataItemValue);
                        DomUtil.SetAttr(template_item, "role", null);

                        Debug.Assert(template_item.Attributes["role"] == null, "");
                    }
                }
            }

            // 将template_dom中所有具备role属性的<item>或<category>元素的value属性值设置为空
            XmlNodeList nodes = dom_template.DocumentElement.SelectNodes("//item | //category");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                XmlAttribute attr = node.Attributes["role"];
                if (attr != null)
                {
                    Debug.Assert(node.Attributes["value"] == null, "");

                    if (bRemoveZeroValueItems == true)
                        node.ParentNode.RemoveChild(node);
                    else
                    {
                        string strTempRole = attr.Value;   // DomUtil.GetAttr(node, "role");

                        // 看看这个事项是否允许？
                        if (StringUtil.IsInList("all", strTempRole) == true
                            || StringUtil.IsInList(strRole, strTempRole) == true)
                        {
                            DomUtil.SetAttr(node, "role", null);
                            DomUtil.SetAttr(node, "value", "");
                        }
                        else
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }
            }

            // 将name='*'之元素删除
            nodes = dom_template.DocumentElement.SelectNodes("//*[@name='*']");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            strTargetXml = dom_template.OuterXml;
            return 0;
        }

#if NO
        public DateTime CurrentDate()
        {
            if (String.IsNullOrEmpty(this.XmlFilename) == true)
                return new DateTime(0);

            string strDate = "";
            int nRet = this.XmlFilename.IndexOf('.');
            if (nRet == -1)
                strDate = this.XmlFilename;
            else
                strDate = this.XmlFilename.Substring(0, nRet);

            if (strDate.Length != 8)
                return new DateTime(0);
            Debug.Assert(strDate.Length == 8, "");

            return DateTimeUtil.Long8ToDateTime(strDate);
        }
#endif

        string GetCommentString(RangeStatisInfo info)
        {
            string strResult = "";

            strResult += "<span class='comment'>"
                + this.GetString("统计日期范围")
                + ": "
                + DateTimeUtil.AddHyphenToString8(info.StartDate, "/")
                + " - "
                + DateTimeUtil.AddHyphenToString8(info.EndDate, "/")
                + "</span><br/>";
            strResult += "<span class='comment'>"
                + this.GetString("所含天数")
                + ": " + info.Days.ToString() + "</span><br/>";
            strResult += "<span class='comment'>"
                + this.GetString("实际日期范围")
                + ": "
                + DateTimeUtil.AddHyphenToString8(info.RealStartDate, "/")
                + " - "
                + DateTimeUtil.AddHyphenToString8(info.RealEndDate, "/")
                + "</span><br/>";
            strResult += "<span class='comment'>"
                + this.GetString("实际所含天数")
                + ": " + info.RealDays.ToString() + "</span><br/>";

            return strResult;
        }

        protected override void CreateChildControls()
        {
#if NO
            Label label = new Label();
            label.ID = "label";
            label.CssClass = "librarycodelabel";
            label.Text = "分馆代码: ";
            this.Controls.Add(label);

            DropDownList list = new DropDownList();
            list.ID = "librarycode";
            // list.Width = new Unit("100%");
            list.CssClass = "librarycode";
            list.AutoPostBack = true;
            list.TextChanged -= new EventHandler(list_TextChanged);
            list.TextChanged += new EventHandler(list_TextChanged);
            this.Controls.Add(list);

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            List<string> codes = app.GetAllLibraryCodes();
            if (codes.Count > 0)
                FillList(list, codes);
            else
            {
                label.Visible = false;
                list.Visible = false;
            }
#endif

            LiteralControl table = new LiteralControl();
            table.ID = "table";  // id用于render()时定位
            this.Controls.Add(table);
        }

        void list_TextChanged(object sender, EventArgs e)
        {
        }

#if NO
        static void FillList(DropDownList list,
            List<string> items)
        {
            list.Items.Clear();

            if (items == null)
                return;

            list.Items.Add("<所有分馆>");
            foreach (string s in items)
            {
                list.Items.Add(s);
            }
        }
#endif

        protected override void Render(HtmlTextWriter writer)
        {
            string strError = "";
            int nRet = 0;

            LiteralControl table = (LiteralControl)this.FindControl("table");
#if NO
            DropDownList list = (DropDownList)this.FindControl("librarycode");
#endif

            if (String.IsNullOrEmpty(this.Xml) == true)
            {
                /*
                strError = "<span class='comment'>"
                    + this.GetString("尚未指定日期参数")  // "(尚未指定日期参数)"
                    + "</span>"; 
                goto ERROR1;
                 * */
                strError = this.DateRange + " " + this.GetString("没有统计信息");
                goto ERROR1;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strTemplateFilename = app.CfgDir + "\\statis_template.xml";
#if NO
            string strLibraryCode = list.Text;
            if (strLibraryCode == "<所有分馆>")
                strLibraryCode = "";
#endif
            string strLibraryCode = (string)this.Page.Session["librarycode"];

            List<string> codes = app.GetAllLibraryCodes();
            bool bExistsTemplateFile = false;
            if (File.Exists(strTemplateFilename) == true)
            {
                bExistsTemplateFile = true;

                string strTargetXml = "";
                LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                // 利用模板文件过滤统计事项，并将事项按照模板顺序排序
                nRet = StatisViewControl.FilterXmlFile(
                    strLibraryCode,
                    loginstate.ToString().ToLower(), // notlogin public reader librarian
                    this.Xml,
                    strTemplateFilename,
                    false,
                    out strTargetXml,
                    // out exist_codes,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                this.Xml = strTargetXml;
            }

            string strDate = "";
            strDate = this.DateRange;

            try
            {
                dom = new XmlDocument();
                dom.LoadXml(this.Xml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入DOM时出错：" + ex.Message;
                goto ERROR1;
            }

#if NO
            // 如果没有过滤，则需要在这里获得馆代码
            if (File.Exists(strTemplateFilename) == false)
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("library");
                foreach (XmlNode node in nodes)
                {
                    string strCode = DomUtil.GetAttr(node, "code");
                    exist_codes.Add(strCode);
                }
            }
#endif
            // 如果没有过滤，则需要在这里把特定馆代码的片段提升到根下
            if (bExistsTemplateFile == false
                && string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode node = dom.DocumentElement.SelectSingleNode("library[@code='"+strLibraryCode+"']");
                if (node != null)
                    dom.DocumentElement.InnerXml = node.InnerXml;
                else
                    dom.DocumentElement.InnerXml = "";
            }

            StringBuilder text = new StringBuilder(4096);

            text.Append("<div class='statisframe'><table class='statis' >");// border='1'

            string strLibraryCodeText = "";
            if (String.IsNullOrEmpty(strLibraryCode) == true)
            {
                if (codes.Count == 0)
                    strLibraryCodeText = "";
                else
                    strLibraryCodeText = this.GetString("全部分馆")+" ";
            }
            else
            {
                strLibraryCodeText = strLibraryCode + " ";
            }
            List<string> column_titles = new List<string>();

            DateTime start_date = DateTimeUtil.Long8ToDateTime(this.RangeStatisInfo.StartDate);
            DateTime end_date = DateTimeUtil.Long8ToDateTime(this.RangeStatisInfo.EndDate);
            DateTime now = DateTime.Now;
            if (this.IsRange == true)
            {
                DateTime current_date = start_date;
                while (current_date <= end_date
                    && current_date <= now)
                {
                    column_titles.Add(GetDateString(current_date, current_date == start_date));
                    current_date = current_date.AddDays(1);
                }
            }


            string strColspan = "";
            strColspan = " colspan='" + (column_titles.Count + 2).ToString() + "' ";

            // 标题行
            text.Append("<tr class='tabletitle'><td "+strColspan+">" + strLibraryCodeText + strDate + " "
                + this.GetString("实时统计信息")
                + "</td></tr>");

            // 注释行
            if (this.RangeStatisInfo != null)
            {
                string strComment = GetCommentString(this.RangeStatisInfo);
                text.Append("<tr class='comment'><td " + strColspan + ">" + strComment + "</td></tr>");
            }

            // 栏目标题行
            string strColumnLine = "";
            if (this.IsRange == true)
            {
                StringBuilder line = new StringBuilder(4096);
                foreach (string s in column_titles)
                {
                    line.Append("<td class='day'>" + s + "</td>");
                }
                strColumnLine = "<tr class='column'><td class='rowtitle'>统计指标</td><td class='rowtitle'>合计</td>"
                    + line.ToString()
                    + "</tr>";
            }
            else
            {
                strColumnLine = "<tr class='column'><td class='rowtitle'>统计指标</td><td class='rowtitle'>合计</td>"
                    + "</tr>";
            } 
            text.Append(strColumnLine);

            XmlNodeList categorys = this.dom.DocumentElement.SelectNodes("category");
            for (int i = 0; i < categorys.Count; i++)
            {
                XmlNode category = categorys[i];

                string strCategory = DomUtil.GetAttr(category, "name");

                // 2009/7/31
                string strCategoryCaption = this.GetStringEx(strCategory);
                if (String.IsNullOrEmpty(strCategoryCaption) == true)
                    strCategoryCaption = strCategory;

                text.Append("<tr class='category'><td " + strColspan + ">");
                text.Append(strCategoryCaption);
                text.Append("</td></tr>");

                XmlNodeList items = category.SelectNodes("item");

                for (int j = 0; j < items.Count; j++)
                {
                    XmlNode item = items[j];

                    string strItemName = DomUtil.GetAttr(item, "name");
                    string strItemValue = DomUtil.GetAttr(item, "value");

                    // 2009/7/31
                    string strItemNameCaption = this.GetStringEx(strItemName);
                    if (String.IsNullOrEmpty(strItemNameCaption) == true)
                        strItemNameCaption = strItemName;

                    if (this.IsRange == false)
                    {
                        text.Append("<tr class='content'><td class='name'>");
                        text.Append(strItemNameCaption);
                        text.Append("</td><td class='value'>");
                        text.Append(strItemValue);
                        text.Append("</td></tr>");
                    }
                    else
                    {
                        string[] values = strItemValue.Split(new char[] { ',' });
                        text.Append("<tr class='content'><td class='name'>");
                        text.Append(strItemNameCaption);
                        text.Append("</td>");

                        int k = 0;
                        DateTime current_date = start_date;
                        foreach (string v in values)
                        {
                            if (current_date <= end_date
    && current_date <= now)
                            {
                            }
                            else 
                                break;  // 比今天还要靠后的就不显示了

                            string strClass = "day";
                            if (k == 0)
                                strClass = "value";

                            text.Append("<td class='"+strClass+"'>");
                            text.Append(string.IsNullOrEmpty(v) == false ? v : "&nbsp;");
                            text.Append("</td>");

                            if (k != 0) // 第一次出现是合计值, 不是普通日子的值
                                current_date = current_date.AddDays(1);

                            k++;
                        }

                        while (current_date <= end_date
    && current_date <= now)
                        {
                            text.Append("<td class='day'>&nbsp;</td>");
                            current_date = current_date.AddDays(1);
                        }
                        text.Append("</tr>");
                    }
                }
            }

            if (this.IsRange == true && column_titles.Count > 3)
            {
                // 底部再来一次栏目标题行
                text.Append(strColumnLine);
            }

            text.Append("</table></div>");

            table.Text = text.ToString();
            base.Render(writer);
            return;
        ERROR1:
            table.Text = "<span class='comment'>"
                + HttpUtility.HtmlEncode(strError)
                + "</span>";
            base.Render(writer);
        }

        static string GetDateString(DateTime date,
            bool bFirst)
        {
            string strYear = date.Year.ToString().PadLeft(4, '0');

            string strLine12 = "";
            string strClass = "";
            if (date.Day == 1
                || bFirst == true)
                strClass = "";
            else
                strClass = " hidden";

                strLine12 = "<div class='d1"+strClass+"'>" + strYear.Substring(0, 2) + "</div>"
                + "<div class='d2" + strClass + "''>" + strYear.Substring(2, 2) + "</div>";


            string strLine3 = "";
            if (date.Day == 1 || bFirst == true)
                strClass = "";
            else
                strClass = " hidden";

            strLine3 = "<div class='d3" + strClass + "'>" + date.Month.ToString().PadLeft(2, ' ').Replace(" ", "&nbsp;") + "</div>";
            string strLine4 = "<div class='d4'>" + date.Day.ToString().PadLeft(2, ' ').Replace(" ", "&nbsp;") + "</div>";

            return strLine12 + strLine3 + strLine4;
        }

#if NO
        static string VertialText(string strText)
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (char c in strText)
            {
                result.Append(new string(c, 1) + "<br/>");
            }
            return result.ToString();
        }
#endif

#if NO111
        protected override void RenderContents(HtmlTextWriter output)
        {
            string strError = "";
            int nRet = 0;

#if NO
            if (this.XmlFilename == "")
            {
                strError = "<span class='comment'>"
                    + this.GetString("尚未指定日期参数")  // "(尚未指定日期参数)"
                    + "</span>";
                goto ERROR1;
            }
#endif
            if (String.IsNullOrEmpty(this.Xml) == true)
            {
                /*
                strError = "<span class='comment'>"
                    + this.GetString("尚未指定日期参数")  // "(尚未指定日期参数)"
                    + "</span>"; 
                goto ERROR1;
                 * */
                strError = "<span class='comment'>"
    + this.DateRange + " " + this.GetString("没有统计信息") 
    + "</span>";
                goto ERROR1;

            }


            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strTemplateFilename = app.CfgDir + "\\statis_template.xml";

            if (File.Exists(strTemplateFilename) == true)
            {
                string strTargetXml = "";

                LoginState loginstate = Global.GetLoginState(this.Page);

                // 利用模板文件过滤统计事项，并将事项按照模板顺序排序
                nRet = StatisViewControl.FilterXmlFile(
                    loginstate.ToString().ToLower(), // notlogin public reader librarian
                    this.Xml,
                    strTemplateFilename,
                    false,
                    out strTargetXml,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                this.Xml = strTargetXml;
            }



            string strDate = "";

            strDate = this.DateRange;

            try
            {
                dom = new XmlDocument();
                dom.LoadXml(this.Xml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入DOM时出错：" + ex.Message;
                goto ERROR1;
            }

#if NO
            if (nRet == 0)
            {
                strError = "<span class='comment'>" + strDate
                    + this.GetString("没有统计信息")
                    + "</span>";
                goto ERROR1;
            }
#endif

            output.Write("<table class='statis' >");// border='1'

            // 标题行
            output.Write("<tr class='tabletitle'><td colspan='2'>" + strDate + " "
                + this.GetString("实时统计信息")
                + "</td></tr>");

            // 注释行
            if (this.RangeStatisInfo != null)
            {
                string strComment = GetCommentString(this.RangeStatisInfo);
                output.Write("<tr class='comment'><td colspan='2'>" + strComment + "</td></tr>");
            }


            XmlNodeList categorys = this.dom.DocumentElement.SelectNodes("category");
            for (int i = 0; i < categorys.Count; i++)
            {
                XmlNode category = categorys[i];

                string strCategory = DomUtil.GetAttr(category, "name");

                // 2009/7/31
                string strCategoryCaption = this.GetStringEx(strCategory);
                if (String.IsNullOrEmpty(strCategoryCaption) == true)
                    strCategoryCaption = strCategory;

                output.Write("<tr class='category'><td colspan='2'>");
                output.Write(strCategoryCaption);
                output.Write("</td></tr>");

                XmlNodeList items = category.SelectNodes("item");

                for (int j = 0; j < items.Count; j++)
                {
                    XmlNode item = items[j];

                    string strItemName = DomUtil.GetAttr(item, "name");
                    string strItemValue = DomUtil.GetAttr(item, "value");

                    // 2009/7/31
                    string strItemNameCaption = this.GetStringEx(strItemName);
                    if (String.IsNullOrEmpty(strItemNameCaption) == true)
                        strItemNameCaption = strItemName;


                    output.Write("<tr class='content'><td class='name'>");
                    output.Write(strItemNameCaption);
                    output.Write("</td><td class='value'>");
                    output.Write(strItemValue);
                    output.Write("</td></tr>");
                }

            }
            output.Write("</table>");

            return;
        ERROR1:
            output.Write(strError);
        }
#endif

        int LoadDom(string strXmlFilename,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();

            try
            {
                this.dom.Load(strXmlFilename);
            }
            catch (FileNotFoundException)
            {
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "XML文件 " + strXmlFilename + " 装载到XMLDOM时出错" + ex.Message;
                return -1;
            }

            return 1;
        }


        #region 新的过滤统计指标的函数


        // 利用模板文件过滤统计事项，并将事项按照模板顺序排序
        // parameters:
        public static int FilterXmlFile(
    string strRole, // notlogin public reader librarian
    string strSourceXml,
    string strTemplateFilename,
    bool bRemoveZeroValueItems,
    out string strTargetXml,
    out string strError)
        {
            strError = "";
            strTargetXml = "";

            XmlDocument dom_data = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strSourceXml) == true)
                    dom_data.LoadXml("<root />");
                else
                    dom_data.LoadXml(strSourceXml);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载数据XML字符串到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlDocument dom_template = new XmlDocument();
            try
            {
                dom_template.Load(strTemplateFilename);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载模板文件 '" + strTemplateFilename + "' 到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }


            string strTempTargetXml = "";
            // 对根下处理一次
            int nRet = FilterXmlFile(
                strRole,
                dom_data.DocumentElement,
                dom_template.DocumentElement,
                bRemoveZeroValueItems,
                out strTempTargetXml,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument target_dom = new XmlDocument();
            target_dom.LoadXml("<root />");

            target_dom.DocumentElement.InnerXml = strTempTargetXml;

            // 处理每个 <library> 元素
            XmlNodeList nodes = dom_data.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");

                nRet = FilterXmlFile(
    strRole,
    node,
    dom_template.DocumentElement,
    bRemoveZeroValueItems,
    out strTempTargetXml,
    out strError);
                if (nRet == -1)
                    return -1;

                XmlNode library = target_dom.CreateElement("library");
                target_dom.DocumentElement.AppendChild(library);
                DomUtil.SetAttr(library, "code", strCode);
                library.InnerXml = strTempTargetXml;
            }

            strTargetXml = target_dom.DocumentElement.OuterXml;
            return 0;
        }

        // parameters:
        //      source_root 源根节点。<root>或<library>元素。当为<root>元素时、只处理其下直接的<category>元素。
        //      template_root   模板文件的根节点
        //      strTargetXml    InnerXml
        static int FilterXmlFile(
            string strRole, // notlogin public reader librarian
            XmlNode source_root,
            XmlNode template_root,
            bool bRemoveZeroValueItems,
            out string strTargetXml,
            out string strError)
        {
            strError = "";
            strTargetXml = "";
            // exist_codes = new List<string>();

#if NO
            {
                // 列出全部<library>元素的code属性值
                XmlNodeList library_nodes = dom_data.DocumentElement.SelectNodes("library");
                foreach (XmlNode node in library_nodes)
                {
                    string strCode = DomUtil.GetAttr(node, "code");
                    exist_codes.Add(strCode);
                }
            }
#endif

            XmlDocument dom_template = new XmlDocument();
            if (template_root != null)
            {
                try
                {
                    dom_template.LoadXml(template_root.OuterXml);
                }
                catch (Exception ex)
                {
                    // text-level: 内部错误
                    strError = "装载模板XML片断到XMLDOM时发生错误: " + ex.Message;
                    return -1;
                }
            }
            else
            {
                // 确保dom_template的根存在
                if (dom_template.DocumentElement == null)
                {
                    dom_template.LoadXml("<root />");
                }
            }

            string strWildMatchCategoryRole = "";
            // 找到模板里面的通配符<category>元素
            XmlNode wildmatch_category = dom_template.DocumentElement.SelectSingleNode("category[@name='*']");
            if (wildmatch_category != null)
                strWildMatchCategoryRole = DomUtil.GetAttr(wildmatch_category, "role");

            XmlNodeList data_categorys = source_root.SelectNodes("category");
            for (int i = 0; i < data_categorys.Count; i++)
            {
                XmlNode data_category = data_categorys[i];

                string strDataCategoryName = DomUtil.GetAttr(data_category, "name");

                bool bCategoryAllowed = false;  // <catagory>是否被通配符事项所允许?

                // 看看这个名字在dom_template中是否存在
                XmlNode template_category = dom_template.DocumentElement.SelectSingleNode("category[@name='" + strDataCategoryName + "']");
                if (template_category == null)
                {
                    // 看看有没有<catagory name="*">定义。
                    // 没有定义，就不被允许
                    if (String.IsNullOrEmpty(strWildMatchCategoryRole) == false)
                    {
                        // 看看这个事项是否允许？
                        if (StringUtil.IsInList("all", strWildMatchCategoryRole) == true
                            || StringUtil.IsInList(strRole, strWildMatchCategoryRole) == true)
                        {
                            // 如果不存在，就创建一个
                            template_category = dom_template.CreateElement("category");
                            dom_template.DocumentElement.AppendChild(template_category);
                            DomUtil.SetAttr(template_category, "name", strDataCategoryName);
                            bCategoryAllowed = true;
                        }
                    }

                }
                else
                {
                    string strTemplateRoles = DomUtil.GetAttr(template_category, "role");
                    if (String.IsNullOrEmpty(strTemplateRoles) == false)
                    {
                        // 看看这个目录是否允许？
                        if (StringUtil.IsInList("all", strTemplateRoles) == false
                            && StringUtil.IsInList(strRole, strTemplateRoles) == false)
                        {
                            template_category.ParentNode.RemoveChild(template_category);
                            continue;   // 整个<category>不被允许出现
                        }
                    }
                    else
                    {
                        template_category.ParentNode.RemoveChild(template_category);
                        continue;   // 整个<category>不被允许出现
                    }

                    // 清除<category>元素中的role属性
                    DomUtil.SetAttr(template_category, "role", null);

                    Debug.Assert(template_category.Attributes["role"] == null, "");
                }

                string strWildMatchItemRole = "";

                // 找到模板里面的通配符<item>元素
                if (bCategoryAllowed == true)
                    strWildMatchItemRole = strWildMatchCategoryRole;    // 继承<category name='*'>的role属性
                else if (template_category != null)
                {
                    XmlNode wildmatch_item = template_category.SelectSingleNode("item[@name='*']");
                    if (wildmatch_item != null)
                        strWildMatchItemRole = DomUtil.GetAttr(wildmatch_item, "role");
                }
                else
                {
                    strWildMatchItemRole = strWildMatchCategoryRole;    // 继承<category name='*'>的role属性
                }

                XmlNodeList data_items = data_category.SelectNodes("item");
                for (int j = 0; j < data_items.Count; j++)
                {
                    XmlNode data_item = data_items[j];

                    string strDataItemName = DomUtil.GetAttr(data_item, "name");
                    string strDataItemValue = DomUtil.GetAttr(data_item, "value");

                    XmlNode template_item = null;
                    // 看看这个名字在DOM2中是否存在
                    if (template_category != null)
                        template_item = template_category.SelectSingleNode("item[@name='" + strDataItemName + "']");

                    if (template_item == null)
                    {
                        // 看看有没有<item name="*">定义
                        // 没有定义，就不被允许
                        if (String.IsNullOrEmpty(strWildMatchItemRole) == false)
                        {
                            // 看看这个事项是否允许？
                            if (StringUtil.IsInList("all", strWildMatchItemRole) == true
                                || StringUtil.IsInList(strRole, strWildMatchItemRole) == true)
                            {
                                // 如果不存在，就创建一个
                                template_item = dom_template.CreateElement("item");
                                template_category.AppendChild(template_item);
                                DomUtil.SetAttr(template_item, "name", strDataItemName);
                                DomUtil.SetAttr(template_item, "value", strDataItemValue);
                            }
                        }
                    }
                    else
                    {
                        string strItemRoles = DomUtil.GetAttr(template_item, "role");
                        if (String.IsNullOrEmpty(strItemRoles) == false)
                        {
                            // 看看这个事项是否允许？
                            if (StringUtil.IsInList("all", strItemRoles) == false
                                && StringUtil.IsInList(strRole, strItemRoles) == false)
                            {
                                template_item.ParentNode.RemoveChild(template_item);
                                continue;   // <item>不被允许出现
                            }
                        }
                        else
                        {
                            template_item.ParentNode.RemoveChild(template_item);
                            continue;   // <item>不被允许出现
                        }

                        DomUtil.SetAttr(template_item, "value", strDataItemValue);
                        DomUtil.SetAttr(template_item, "role", null);

                        Debug.Assert(template_item.Attributes["role"] == null, "");
                    }
                }
            }

            // 将template_dom中所有具备role属性的<item>或<category>元素的value属性值设置为空
            XmlNodeList nodes = dom_template.DocumentElement.SelectNodes("//item | //category");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                XmlAttribute attr = node.Attributes["role"];
                if (attr != null)
                {
                    Debug.Assert(node.Attributes["value"] == null, "");

                    if (bRemoveZeroValueItems == true)
                        node.ParentNode.RemoveChild(node);
                    else
                    {
                        string strTempRole = attr.Value;   // DomUtil.GetAttr(node, "role");

                        // 看看这个事项是否允许？
                        if (StringUtil.IsInList("all", strTempRole) == true
                            || StringUtil.IsInList(strRole, strTempRole) == true)
                        {
                            DomUtil.SetAttr(node, "role", null);
                            DomUtil.SetAttr(node, "value", "");
                        }
                        else
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }
            }

            // 将name='*'之元素删除
            nodes = dom_template.DocumentElement.SelectNodes("//*[@name='*']");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            strTargetXml = dom_template.DocumentElement.InnerXml;
            return 0;
        }

        #endregion
    }

}
