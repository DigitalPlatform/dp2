using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Diagnostics;
using System.Collections;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Xml;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;

using DigitalPlatform.OPAC.Server;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BrowseSearchResultControl runat=server></{0}:BrowseSearchResultControl>")]
    public class BrowseSearchResultControl : WebControl, INamingContainer
    {
        public bool PageNoUrlMode = false;  // pager 是否采用URL方式定位 pageno

        public bool EnableAddToMyBookshelf = true;  // 允许"加入我的书架"按钮
        public bool EnableRemoveFromMyBookshelf = false;  // 允许"从我的书架移除"按钮
        public bool EnableExport = true;    // 允许"导出"按钮

        public bool MinimizeNewReviewEdtior = true;  // 初始时是否隐藏新创建评注编辑区域

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BrowseSearchResultControl.cs",
                typeof(BrowseSearchResultControl).Module.Assembly);

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

        // public string Lang = "";
        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        string m_strDefaultFormatName = "brief";    // browse

        // 缺省的浏览格式
        public string DefaultFormatName
        {
            get
            {
                return this.m_strDefaultFormatName;
            }
            set
            {
                this.m_strDefaultFormatName = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

#if NO
        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
#endif

        /*
        public int StartIndex
        {
            get
            {

                object o = this.Page.Session[this.ID + "Browse_StartIndex"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "Browse_StartIndex"] = (object)value;
            }
        }
         * */
        public int StartIndex
        {
            get
            {
                object o = ViewState[this.ID + "Browse_StartIndex"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "Browse_StartIndex"] = value;
            }
        }

        public int PageMaxLines
        {
            get
            {
                object o = ViewState[this.ID + "Browse_PageMaxLines"];
                if (o == null)
                    return 10;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "Browse_PageMaxLines"] = value;
            }
        }

        public int ResultCount
        {
            get
            {
                object o = ViewState[this.ID + "_ResultCount"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "_ResultCount"] = value;
            }
        }

        // 2010/10/11
        // 结果集文件名
        public string ResultsetFilename
        {
            get
            {
                String s = (String)ViewState[this.ID + "ResultsetFilename"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState[this.ID + "ResultsetFilename"] = value;
            }
        }

        // 2012/12/20
        // 结果集内偏移量
        public string ResultsetOffset
        {
            get
            {
                String s = (String)ViewState[this.ID + "ResultsetOffset"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState[this.ID + "ResultsetOffset"] = value;
            }
        }

        // 2006/12/24 changed
        public string ResultSetName
        {
            get
            {
                String s = (String)ViewState[this.ID + "ResultSetName"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState[this.ID + "ResultSetName"] = value;
            }
        }

#if NO
        public string FormatName
        {
            get
            {
                String s = (String)ViewState[this.ID + "Browse_FormatName"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState[this.ID + "Browse_FormatName"] = value;
            }
        }
#endif
        public string FormatName = "";

        public string CurrentFormat
        {
            get
            {
                this.EnsureChildControls();
                TabControl format_control = (TabControl)this.FindControl("format_control");
                return format_control.ActiveTab;
            }
        }

        // 计算出页码总数
        public int PageCount
        {
            get
            {
                int nDelta = this.ResultCount % this.PageMaxLines;
                if (nDelta > 0)
                    return (this.ResultCount / this.PageMaxLines) + 1;
                return (this.ResultCount / this.PageMaxLines);
            }
        }

        // 本来就只能保持一个Render周期，因此不必在使用前清空了
        // 数据库名 --> List<string> 
        Hashtable m_usedColumnCaptionTable = new Hashtable();

        string BuildBrowseContent(
            OpacApplication app,
            SessionInfo sessioninfo,
            string strDbName,
            string[] cols)
        {
            string strError = "";
            List<string> captions = null;

            // 先从catch中找
            captions = (List<string>)this.m_usedColumnCaptionTable[strDbName];
            if (captions == null)
            {

                // 获得一个库的浏览列标题
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = app.GetBrowseColumnCaptions(
                sessioninfo,
                strDbName,
                this.Lang,
                out captions,
                out strError);
                if (nRet == -1)
                    return StringUtil.MakePathList(cols).Replace(",", "<br/>") + "<br/>" + strError;
                if (nRet == 0 || captions == null || captions.Count == 0)
                    return StringUtil.MakePathList(cols).Replace(",", "<br/>");

                this.m_usedColumnCaptionTable[strDbName] = captions;
            }

            StringBuilder result = new StringBuilder(4096);
            result.Append("<table class='brief_content'>");
            for (int i = 1; i < cols.Length; i++)
            {
                result.Append("<tr><td class='name'>");
                string strName = "";

                if (i - 1 < captions.Count)
                    strName = captions[i - 1];

                result.Append(strName + "</td><td class='value'>" + cols[i]);

                result.Append("</td></tr>");
            }
            result.Append("</table>");

            return result.ToString();
        }

        public void SetErrorInfo(string strText)
        {
            LiteralControl resultinfo = (LiteralControl)this.FindControl("resultinfo");
            resultinfo.Text = strText;
        }

        // 设置结果集有关数量参数
        public void SetResultInfo()
        {
            int nPageNo = this.StartIndex / this.PageMaxLines;

            LiteralControl resultinfo = (LiteralControl)this.FindControl("resultinfo");
            if (this.ResultCount != 0)
            {
                // resultinfo.Text = "共命中记录 " + Convert.ToString(this.ResultCount) + " 条, 分 " + Convert.ToString(this.PageCount) + " 页显示, 当前为第 " + Convert.ToString(nPageNo + 1) + "页。";
                resultinfo.Text =
                    "<div class='info'>" +
                    string.Format(this.GetString("hit_count_summary"),   // "共命中记录 {0} 条, 分 {1} 页显示, 当前为第 {2} 页。"
                    this.ResultCount.ToString(),
                    this.PageCount.ToString(),
                    (nPageNo + 1).ToString())
                    + "</div>";
            }
            else
                resultinfo.Text = "<div class='info'>" + this.GetString("empty_resultset") + "</div>";   // "(结果集为空)"

            /*
            LiteralControl maxpagecount = (LiteralControl)this.FindControl("maxpagecount");
            maxpagecount.Text = " " + string.Format(this.GetString("page_count_summary"), // "(共 {0} 页)"
                this.PageCount.ToString());


            LiteralControl currentpageno = (LiteralControl)this.FindControl("currentpageno");
            currentpageno.Text = Convert.ToString(nPageNo + 1);

            PlaceHolder pageswitcher = (PlaceHolder)this.FindControl("pageswitcher");
            if (this.PageCount <= 1)
                pageswitcher.Visible = false;
            else
                pageswitcher.Visible = true;
             * */
            PageSwitcherControl pager_top = (PageSwitcherControl)this.FindControl("pager_top");
            PageSwitcherControl pager_bottom = (PageSwitcherControl)this.FindControl("pager_bottom");
            if (this.PageCount <= 1)
            {
                pager_top.Visible = false;
                pager_bottom.Visible = false;
            }
            else
            {
                pager_top.Visible = true;
                pager_bottom.Visible = true;
            }


            pager_top.CurrentPageNo = nPageNo;
            pager_top.TotalCount = this.PageCount;

            pager_bottom.CurrentPageNo = nPageNo;
            pager_bottom.TotalCount = this.PageCount;
        }

        // public string Title = "";
        public string Title
        {
            get
            {
                String s = (String)ViewState[this.ID + "Title"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState[this.ID + "Title"] = value;
            }
        }

        void FillFormatControl(TabControl tabcontrol)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            string strError = "";
            List<string> formatnames = null;
            int nRet = app.GetBrowseFormatNames(
                this.Lang,
                null,
                out formatnames,
                out strError);
            if (nRet == -1)
            {
                // throw new Exception(strError);
                this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
                this.Page.Response.End();
                return;
            }

            string strDefaultFormatName = this.DefaultFormatName;

            // 换算为语言相关的字符串
            string strLangName = app.GetBrowseFormatName(strDefaultFormatName,
                this.Lang);
            if (String.IsNullOrEmpty(strLangName) == true)
                strLangName = strDefaultFormatName; // 只好还是用不适合的语言的

            if (formatnames.IndexOf(strLangName) == -1)
                formatnames.Insert(0, strLangName);

            FillTabList(tabcontrol, formatnames);

            /*
            if (string.IsNullOrEmpty(tabcontrol.ActiveTab) == true)
                tabcontrol.ActiveTab = strLangName;
             * */
        }

        static void FillTabList(TabControl tabcontrol,
    List<string> formatnames)
        {
            StringUtil.RemoveDupNoSort(ref formatnames);

            tabcontrol.Columns.Clear();
            for (int i = 0; i < formatnames.Count; i++)
            {
                TabColumn column = new TabColumn();
                column.Name = formatnames[i];
                tabcontrol.Columns.Add(column);
            }
        }

        public void Clear()
        {
            this.EnsureChildControls();

            this.StartIndex = 0;

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                if (line == null)
                    continue;

                BiblioControl bibliocontrol = (BiblioControl)line.FindControl("line" + Convert.ToString(i) + "_biblio");
                MarcControl marccontrol = (MarcControl)line.FindControl("line" + Convert.ToString(i) + "_marc");
                ItemsControl itemscontrol = (ItemsControl)line.FindControl("line" + Convert.ToString(i) + "_items");
                CommentsControl commentscontrol = (CommentsControl)line.FindControl("line" + Convert.ToString(i) + "_comments");
                ItemControl itemcontrol = (ItemControl)line.FindControl("line" + Convert.ToString(i) + "_item");
                CommentControl commentcontrol = (CommentControl)line.FindControl("line" + Convert.ToString(i) + "_comment");

                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");


                /*
                if (bibliocontrol != null)
                    bibliocontrol.Clear();
                if (marccontrol != null)
                    marccontrol.Clear();
                if (itemcontrol != null)
                    itemcontrol.Clear();
                if (commentcontrol != null)
                    commentcontrol.Clear();
                 * */

                if (itemscontrol != null)
                    itemscontrol.Clear();
                if (commentscontrol != null)
                    commentscontrol.Clear();


                if (checkbox != null)
                    checkbox.Checked = false;
            }

        }

        protected override void CreateChildControls()
        {
            CreatePrifix(String.IsNullOrEmpty(this.Title) == true ? this.GetString("命中结果") : this.Title,
                "content_wrapper");
            this.Controls.Add(new LiteralControl("<table class='browse'>"));

            HiddenField recpathlist = new HiddenField();
            recpathlist.ID = "recpathlist";
            this.Controls.Add(recpathlist);

            // 信息行
            /*
            int nPageCount = this.ResultCount / this.PageMaxLines;
            if ((this.ResultCount % this.PageMaxLines) > 0)
                nPageCount ++;
             */
            this.Controls.Add(new LiteralControl(
                "<tr class='info'><td colspan='4'>"
            ));

            // 信息文字
            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            this.Controls.Add(resultinfo);

            PageSwitcherControl pager = new PageSwitcherControl();
            pager.ID = "pager_top";
            if (this.PageNoUrlMode == false)
                pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            else
            {
                pager.EventMode = false;
                pager.GetBaseUrl += new GetBaseUrlEventHandler(pager_GetBaseUrl);
            }
            this.Controls.Add(pager);

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));

            // tabcontrol
            this.Controls.Add(new LiteralControl(
    "<tr class='format'><td colspan='4'>"
));
            TabControl format_control = new TabControl();
            format_control.ID = "format_control";
            this.Controls.Add(format_control);
            this.Controls.Add(new LiteralControl(
     "</td></tr>"
 ));
            format_control.Description = this.GetString("显示格式");
            FillFormatControl(format_control);

            // 标题行
            this.Controls.Add(new LiteralControl(
                "<tr class='columntitle'><td class='no' nowrap>"
                + this.GetString("序号")
                + "</td><td class='content'>"
                + this.GetString("内容")
                + "</td></tr>"
            ));

            // 内容代表
            PlaceHolder content = new PlaceHolder();
            content.ID = "content";
            this.Controls.Add(content);

            // 内容行
            for (int i = 0;
#if USE_LINECOUNT
                i < this.LineCount;
#else
 i < this.PageMaxLines;
#endif
 i++)
            {
                PlaceHolder line = NewContentLine(content, i, null);
            }

            // 插入点
            PlaceHolder insertpoint = new PlaceHolder();
            insertpoint.ID = "insertpoint";
            content.Controls.Add(insertpoint);

            // 命令行
            CreateCmdLine();

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            debugline.Visible = false;
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);

            this.Controls.Add(new LiteralControl(
               // "</table></div>"
               "</table>" + this.GetPostfixString()
               ));
        }

        void pager_GetBaseUrl(object sender, GetBaseUrlEventArgs e)
        {
            e.BaseUrl = this.GetBaseUrl();
        }

        string GetBaseUrl()
        {
            TabControl format_control = (TabControl)this.FindControl("format_control");
            string strFormat = format_control.ActiveTab;
            string strUrl = PageSwitcherControl.RemoveParameter(this.Page.Request.RawUrl, "format");
            if (string.IsNullOrEmpty(strFormat) == false)
            {
                if (strUrl.IndexOf("?") != -1)
                    strUrl += "&format=" + HttpUtility.UrlEncode(strFormat);
                else
                    strUrl += "?format=" + HttpUtility.UrlEncode(strFormat);
            }
            return strUrl;
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new AutoIndentLiteral("<%begin%><tr class='debugline'><td colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new AutoIndentLiteral("</td><%end%></tr>"));
        }

        public void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            text.Text = strText;
        }

        public void SetDebugInfo(string strSpanClass,
            string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            if (strSpanClass == "errorinfo")
                text.Text = "<div class='errorinfo-frame'><div class='" + strSpanClass + "'>" + strText + "</div></div>";
            else
                text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }

        void pager_PageSwitch(object sender, PageSwitchEventArgs e)
        {
            this.StartIndex = this.PageMaxLines * e.GotoPageNo;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }

            SelectAll(false);
            ResetAllItemsControlPager();
        }

        public void SelectAll(bool bEnable)
        {
            this.EnsureChildControls();

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                if (checkbox.Checked != bEnable)
                {
                    checkbox.Checked = bEnable;
                }
            }
        }

        // 2012/11/11
        // 将每个记录显示区的ItemsControl的分页页码复位
        public void ResetAllItemsControlPager()
        {
            this.EnsureChildControls();

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                ItemsControl itemscontrol = (ItemsControl)this.FindControl("line" + Convert.ToString(i) + "_items");
                if (itemscontrol == null)
                    continue;
                if (itemscontrol.StartIndex != 0)
                    itemscontrol.StartIndex = 0;
            }
        }

        public List<string> GetCheckedPath()
        {
            this.EnsureChildControls();

            List<string> results = new List<string>();

            string[] paths = this.RecPathList.Split(new char[] { ',' });

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                if (i >= paths.Length)
                    break;

                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                if (checkbox != null
                    && checkbox.Checked == true)
                {
                    string strPath = paths[i];
                    if (String.IsNullOrEmpty(strPath) == false)
                        results.Add(strPath);
                }
            }

            return results;
        }

        // 新创建内容行
        PlaceHolder NewContentLine(Control content,
            int nLineNo,
            Control insertpos)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + Convert.ToString(nLineNo);

            if (insertpos != null)
            {
                int index = content.Controls.IndexOf(insertpos);
                content.Controls.AddAt(index, line);
            }
            else
            {
                content.Controls.Add(line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='contentup'><td class='no' rowspan='2'>";
            line.Controls.Add(literal);

            // 序号
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_no";
            line.Controls.Add(literal);

            CheckBox checkbox = new CheckBox();
            checkbox.ID = "line" + Convert.ToString(nLineNo) + "_checkbox";
            line.Controls.Add(checkbox);

            literal = new LiteralControl();
            literal.Text = "</td><td class='path'>";
            line.Controls.Add(literal);

            // 记录标题
            Panel title = new Panel();
            title.ID = "line" + Convert.ToString(nLineNo) + "_title";
            title.CssClass = "title";
            line.Controls.Add(title);

            // 路径
            HyperLink hyper = new HyperLink();
            hyper.ID = "line" + Convert.ToString(nLineNo) + "_path";
            hyper.CssClass = "path";
            hyper.ToolTip = "书目记录路径";
            line.Controls.Add(hyper);

            literal = new LiteralControl();
            literal.Text = "</td></tr><tr class='contentdown'><td class='content'>";
            line.Controls.Add(literal);

            // 内容
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_content";
            line.Controls.Add(literal);

            PlaceHolder layout_holder = new PlaceHolder();
            layout_holder.ID = "line" + Convert.ToString(nLineNo) + "_layout";
            line.Controls.Add(layout_holder);

            // 必要的控件
            BiblioControl bibliocontrol = new BiblioControl();
            bibliocontrol.ID = "line" + Convert.ToString(nLineNo) + "_biblio";
            line.Controls.Add(bibliocontrol);

            MarcControl marccontrol = new MarcControl();
            marccontrol.ID = "line" + Convert.ToString(nLineNo) + "_marc";
            line.Controls.Add(marccontrol);

            /*
            literal = new LiteralControl();
            sep.Text = "<div class='sep'>";
            this.Controls.Add(sep);
             * */

            ItemsControl itemscontrol = new ItemsControl();
            itemscontrol.ID = "line" + Convert.ToString(nLineNo) + "_items";
            line.Controls.Add(itemscontrol);

            /*
            literal = new LiteralControl();
            sep.Text = "<div class='sep'>";
            this.Controls.Add(sep);
             * */

            CommentsControl commentscontrol = new CommentsControl();
            commentscontrol.MinimizeNewReviewEdtior = this.MinimizeNewReviewEdtior;
            commentscontrol.ID = "line" + Convert.ToString(nLineNo) + "_comments";
            commentscontrol.WantFocus -= new WantFocusEventHandler(commentscontrol_WantFocus);
            commentscontrol.WantFocus += new WantFocusEventHandler(commentscontrol_WantFocus);
            line.Controls.Add(commentscontrol);

            ItemControl itemcontrol = new ItemControl();
            itemcontrol.ID = "line" + Convert.ToString(nLineNo) + "_item";
            line.Controls.Add(itemcontrol);

            CommentControl commentcontrol = new CommentControl();
            commentcontrol.ID = "line" + Convert.ToString(nLineNo) + "_comment";
            line.Controls.Add(commentcontrol);
            /*
            literal = new LiteralControl();
            literal.Text = "</td><td width='10%'>";
            line.Controls.Add(literal);

            // 操作
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_oper";
            line.Controls.Add(literal);
             */

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);

            return line;
        }

        void commentscontrol_WantFocus(object sender, WantFocusEventArgs e)
        {
            CommentsControl source = (CommentsControl)sender;

            List<Control> comments_controls = FindControl(this,
                typeof(CommentsControl));

            List<Control> items_controls = FindControl(this,
    typeof(ItemsControl));

            if (e.Focus == true)
            {
                source.Active = true;

                // 把整个浏览控件中的除了sender以外的其他CommentsControl对象设置为Active = false
                foreach (Control control in comments_controls)
                {
                    CommentsControl comment_control = (CommentsControl)control;
                    if (comment_control == source)
                    {
                        comment_control.Active = true;
                    }
                    else
                    {
                        comment_control.Active = false;
                    }
                }

                // 把整个浏览控件中的ItemsControl对象设置为Active = false
                foreach (Control control in items_controls)
                {
                    ItemsControl items_control = (ItemsControl)control;
                    items_control.Active = false;
                }
            }
            else
            {
                // 没有人独占，大家都为 true
                foreach (Control control in comments_controls)
                {
                    CommentsControl comment_control = (CommentsControl)control;
                    comment_control.Active = true;
                }

                foreach (Control control in items_controls)
                {
                    ItemsControl items_control = (ItemsControl)control;
                    items_control.Active = true;
                }
            }
        }

        public bool MarcVisible
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                bool bMarcVisible = true;
                XmlNode nodeMarcControl = app.WebUiDom.DocumentElement.SelectSingleNode("marcControl");
                if (nodeMarcControl != null)
                {
                    bMarcVisible = DomUtil.GetBooleanParam(nodeMarcControl,
                        "visible",
                        true);
                }

                return bMarcVisible;
            }
        }

        public bool AllowExportMarc
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                bool bAllowExportMarc = true;
                XmlNode nodeMarcControl = app.WebUiDom.DocumentElement.SelectSingleNode("marcControl");
                if (nodeMarcControl != null)
                {
                    bAllowExportMarc = DomUtil.GetBooleanParam(nodeMarcControl,
                        "export",
                        true);
                }

                return bAllowExportMarc;
            }
        }

        // 2014/4/24
        // 是否允许导出结果集中"全部"记录
        public bool AllowExportAllMarc
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                bool bAllowExportAllMarc = true;
                XmlNode nodeMarcControl = app.WebUiDom.DocumentElement.SelectSingleNode("marcControl");
                if (nodeMarcControl != null)
                {
                    bAllowExportAllMarc = DomUtil.GetBooleanParam(nodeMarcControl,
                        "exportAll",
                        true);
                }

                return bAllowExportAllMarc;
            }
        }

        void CreateCmdLine()
        {
            this.Controls.Add(new LiteralControl(
                "<tr class='cmdline'><td colspan='2'>"
            ));

            this.Controls.Add(new LiteralControl(
                "<table border='0' width='100%'><tr>"
            ));

            this.Controls.Add(new LiteralControl(
                "<td>"
            ));

            Button add_to_mybookshelf = new Button();
            add_to_mybookshelf.ID = "add_to_mybookshelf";
            add_to_mybookshelf.Text = this.GetString("加入我的书架");
            add_to_mybookshelf.Click += new EventHandler(add_to_mybookshelf_Click);
            this.Controls.Add(add_to_mybookshelf);

            Button remove_from_mybookshelf = new Button();
            remove_from_mybookshelf.ID = "remove_from_mybookshelf";
            remove_from_mybookshelf.Text = this.GetString("从我的书架移除");
            remove_from_mybookshelf.Click += new EventHandler(remove_from_mybookshelf_Click);
            this.Controls.Add(remove_from_mybookshelf);

            Button open_export_dialog = new Button();
            open_export_dialog.ID = "open_export_dialog";
            open_export_dialog.Text = this.GetString("导出") + "...";
            open_export_dialog.CssClass = "open_export_dialog";
            open_export_dialog.OnClientClick = "$( '#export-dialog-form' ).dialog({ modal: true }); return cancelClick();";
            this.Controls.Add(open_export_dialog);

            // 修改状态 对话框
            this.Controls.Add(new AutoIndentLiteral("<%begin%><div id='export-dialog-form' title='" + this.GetString("请指定导出特性") + "' style='DISPLAY:NONE'>"));

            {
                this.Controls.Add(new AutoIndentLiteral("<%begin%><div>" + this.GetString("导出范围") + ": "));

                RadioButton selected = new RadioButton();
                selected.ID = "selected";
                selected.Text = this.GetString("选择的事项");
                selected.CssClass = "selected";
                selected.GroupName = "range";
                selected.Checked = true;
                this.Controls.Add(selected);

                RadioButton all = new RadioButton();
                all.ID = "all";
                all.Text = this.GetString("全部事项");
                all.CssClass = "all";
                all.GroupName = "range";
                this.Controls.Add(all);

                if (this.AllowExportAllMarc == false)
                    all.Enabled = false;

                this.Controls.Add(new AutoIndentLiteral("</div><%end%>"));
            }

            {
                this.Controls.Add(new AutoIndentLiteral("<%begin%><div>" + this.GetString("文件格式") + ": "));

                bool bAllowExportMarc = this.AllowExportMarc;

                if (bAllowExportMarc == true)
                {
                    RadioButton iso2709 = new RadioButton();
                    iso2709.ID = "iso2709";
                    iso2709.Text = "MARC(ISO2709)";
                    iso2709.CssClass = "iso2709";
                    iso2709.Checked = true;
                    iso2709.GroupName = "file_format";
                    this.Controls.Add(iso2709);
                }

                RadioButton pathfile = new RadioButton();
                pathfile.ID = "pathfile";
                pathfile.Text = this.GetString("记录路径文件");
                pathfile.CssClass = "pathfile";
                pathfile.GroupName = "file_format";
                if (bAllowExportMarc == false)
                    pathfile.Checked = true;

                this.Controls.Add(pathfile);

                this.Controls.Add(new AutoIndentLiteral("</div><%end%>"));
            }

            {
                this.Controls.Add(new AutoIndentLiteral("<%begin%><div>" + this.GetString("编码方式") + ": "));

                RadioButton utf8 = new RadioButton();
                utf8.ID = "utf8";
                utf8.Text = "UTF-8";
                utf8.CssClass = "utf8";
                utf8.Checked = true;
                utf8.GroupName = "encoding";
                this.Controls.Add(utf8);

                RadioButton gb2312 = new RadioButton();
                gb2312.ID = "gb2312";
                gb2312.Text = "GB-2312";
                gb2312.CssClass = "gb2312";
                gb2312.GroupName = "encoding";
                this.Controls.Add(gb2312);

                this.Controls.Add(new AutoIndentLiteral("</div><%end%>"));
            }

            // 导出 按钮
            Button export_button = new Button();
            export_button.OnClientClick = "$( \"#export-dialog-form\" ).parent().appendTo($(\"form:first\"));$( \"#export-dialog-form\" ).dialog('close'); ";
            export_button.ID = "export_button";
            export_button.Text = this.GetString("导出");
            export_button.Click += new EventHandler(export_Click);
            this.Controls.Add(export_button);

            this.Controls.Add(new AutoIndentLiteral("<%end%></div>"));

#if NO
            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("显示格式")+":";
            this.Controls.Add(literal);

            // 显示格式
            DropDownList list = new DropDownList();
            list.ID = "formatlist";
            list.AutoPostBack = true;
            list.CssClass = "formatlist";
            // list.Text = this.Formats[0];
            this.Controls.Add(list);

            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                string strError = "";
                List<string> formatnames = null;
                int nRet = app.GetBrowseFormatNames(
                    this.Lang,
                    null,
                    out formatnames,
                    out strError);
                if (nRet == -1)
                {
                    // throw new Exception(strError);
                    this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
                    this.Page.Response.End();
                    return;
                }

                string strDefaultFormatName = this.DefaultFormatName;

                // 换算为语言相关的字符串
                string strLangName = app.GetBrowseFormatName(strDefaultFormatName,
                    this.Lang);
                if (String.IsNullOrEmpty(strLangName) == true)
                    strLangName = strDefaultFormatName; // 只好还是用不适合的语言的

                if (formatnames.IndexOf(strLangName) == -1)
                    formatnames.Insert(0, strLangName);

                // 2009/6/23
                formatnames.Add(this.GetString("浏览"));
                formatnames.Add("MARC");

                FillFormatList(list, formatnames);
                if (String.IsNullOrEmpty(this.FormatName) == false)
                    list.Text = this.FormatName;
            }
#endif

            this.Controls.Add(new LiteralControl(
                "</td>"
            ));

            this.Controls.Add(new LiteralControl(
                "<td align='right'> "
            ));
#if NO
            PlaceHolder pageswitcher = new PlaceHolder();
            pageswitcher.ID = "pageswitcher";
            this.Controls.Add(pageswitcher);

            /*
            if (this.PageCount <= 1)
                pageswitcher.Visible = false;
            else
                pageswitcher.Visible = true;
             */


            LinkButton firstpage = new LinkButton();
            firstpage.ID = "first";
            firstpage.Text = this.GetString("首页");
            firstpage.CssClass = "firstpage";
            firstpage.Click += new EventHandler(firstpage_Click);
            pageswitcher.Controls.Add(firstpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton prevpage = new LinkButton();
            prevpage.ID = "prev";
            prevpage.Text = this.GetString("前页");
            prevpage.CssClass = "prevpage";
            prevpage.Click += new EventHandler(prevpage_Click);
            pageswitcher.Controls.Add(prevpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            literal = new LiteralControl();
            literal.ID = "currentpageno";
            literal.Text = "";
            pageswitcher.Controls.Add(literal);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));


            LinkButton nextpage = new LinkButton();
            nextpage.ID = "next";
            nextpage.Text = this.GetString("后页");
            nextpage.CssClass = "nextpage";
            nextpage.Click += new EventHandler(nextpage_Click);
            pageswitcher.Controls.Add(nextpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton lastpage = new LinkButton();
            lastpage.ID = "last";
            lastpage.Text = GetString("末页");
            lastpage.CssClass = "lastpage";
            lastpage.Click += new EventHandler(lastpage_Click);
            pageswitcher.Controls.Add(lastpage);

            literal = new LiteralControl();
            literal.Text = "  ";
            pageswitcher.Controls.Add(literal);

            Button gotobutton = new Button();
            gotobutton.ID = "gotobutton";
            gotobutton.Text = this.GetString("跳到");
            gotobutton.CssClass = "goto";
            gotobutton.Click += new EventHandler(gotobutton_Click);
            pageswitcher.Controls.Add(gotobutton);

            literal = new LiteralControl();
            literal.Text = " " + this.GetString("第") + " ";    // " 第 "
            pageswitcher.Controls.Add(literal);


            TextBox textbox = new TextBox();
            textbox.ID = "gotopageno";
            textbox.Width = new Unit("40");
            textbox.CssClass = "gotopageno";
            pageswitcher.Controls.Add(textbox);

            /*
            literal = new LiteralControl();
            literal.Text = this.GetString("页");
            pageswitcher.Controls.Add(literal);
             * */

            literal = new LiteralControl();
            literal.ID = "maxpagecount";
            literal.Text = " " + string.Format(this.GetString("maxpagecount"), this.PageCount.ToString());    // (共 {0} 页)
            pageswitcher.Controls.Add(literal);

#endif
            PageSwitcherControl pager = new PageSwitcherControl();
            pager.ID = "pager_bottom";
            if (this.PageNoUrlMode == false)
                pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            else
            {
                pager.EventMode = false;
                pager.GetBaseUrl += new GetBaseUrlEventHandler(pager_GetBaseUrl);
            }
            this.Controls.Add(pager);

            this.Controls.Add(new LiteralControl(
                "</td></tr></table>"
            ));

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));
        }

        void export_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            RadioButton iso2709 = (RadioButton)this.FindControl("iso2709");
            RadioButton pathfile = (RadioButton)this.FindControl("pathfile");

            RadioButton gb2312 = (RadioButton)this.FindControl("gb2312");
            RadioButton utf8 = (RadioButton)this.FindControl("utf8");

            RadioButton selected = (RadioButton)this.FindControl("selected");
            RadioButton all = (RadioButton)this.FindControl("all");

            List<string> paths = null;

            // 检查是否允许导出 MARC 文件
            if (pathfile.Checked == false)
            {
                bool bAllowExportMarc = this.AllowExportMarc;

                if (bAllowExportMarc == false)
                {
                    strError = "当前系统不允许导出 MARC 文件";
                    goto ERROR1;
                }
            }

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {

                // 如果都没有选，则认selected items
                if (all.Checked == false)
                {
                    paths = this.GetCheckedPath();
                    if (paths.Count == 0)
                    {
                        strError = "尚未选择要导出的事项";
                        goto ERROR1;
                    }
                }
                else
                {
                    if (this.AllowExportAllMarc == false)
                    {
                        strError = "不允许导出结果集的全部记录";
                        goto ERROR1;
                    }

                    paths = new List<string>();

                    string strResultsetFilename = this.ResultsetFilename;
                    if (String.IsNullOrEmpty(strResultsetFilename) == false)
                    {
                        app.ResultsetLocks.LockForRead(strResultsetFilename, 500);
                        try
                        {
                            DpResultSet resultset = new DpResultSet(false, false);
                            try
                            {
                                resultset.Attach(this.ResultsetFilename,
                                    this.ResultsetFilename + ".index");
                            }
                            catch (Exception ex)
                            {
                                this.SetErrorInfo(ex.Message); // 显示出错信息
                                goto ERROR1;
                            }

                            try
                            {
                                for (int i = 0; i < resultset.Count; i++)
                                {
                                    Thread.Sleep(1);
                                    if (this.Page.Response.IsClientConnected == false)
                                    {
                                        strError = "中断";
                                        goto ERROR1;
                                    }
                                    paths.Add(resultset[i].ID);
                                }
                            }
                            finally
                            {
                                string strTemp1 = "";
                                string strTemp2 = "";
                                resultset.Detach(out strTemp1, out strTemp2);
                            }
                        }
                        finally
                        {
                            app.ResultsetLocks.UnlockForRead(strResultsetFilename);
                        }
                    }
                    else
                    {
                        Record[] records = null;
                        long lStart = 0;
                        for (; ; )
                        {
                            Thread.Sleep(1);
                            if (this.Page.Response.IsClientConnected == false)
                            {
                                strError = "中断";
                                goto ERROR1;
                            }
                            long lRet = // sessioninfo.Channel.
                                channel.GetSearchResult(
                                null,
                                this.ResultSetName,
                                lStart,
                                100,
                                "id",
                                this.Lang,
                                out records,
                                out strError);
                            if (lRet == -1)
                            {
                                strError = "获得结果集时出错: " + strError;
                                goto ERROR1;
                            }

                            for (int i = 0; i < records.Length; i++)
                            {
                                paths.Add(records[i].Path);

                            }
                            lStart += records.Length;
                            if (lStart >= lRet)
                                break;

                        }
                    }

                    if (paths.Count == 0)
                    {
                        strError = "结果集为空，放弃导出";
                        goto ERROR1;
                    }
                }

                // 不让浏览器缓存页面
                this.Page.Response.AddHeader("Pragma", "no-cache");
                this.Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
                this.Page.Response.AddHeader("Expires", "0");

                if (pathfile.Checked == false)
                {
                    // ISO2709文件
                    this.Page.Response.ContentType = "application/iso2709";
                    string strEncodedFileName = HttpUtility.UrlEncode("书目.mrc", Encoding.UTF8);
                    this.Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);

                    Encoding targetEncoding = null;

                    // 如果都没有选，则认utf-8
                    if (gb2312.Checked == true)
                    {
                        targetEncoding = Encoding.GetEncoding(936);
                        this.Page.Response.Charset = "gb2312";
                    }
                    else
                    {
                        targetEncoding = Encoding.UTF8;
                        this.Page.Response.Charset = "utf-8";
                    }

                    for (int i = 0; i < paths.Count; i++)
                    {
                        if (this.Page.Response.IsClientConnected == false)
                            break;

                        string strPath = paths[i];

                        string strDbName = StringUtil.GetDbName(strPath);
                        if (app.IsBiblioDbName(strDbName) == false)
                            continue;

                        string[] formats = new string[1];
                        formats[0] = "xml";
                        string[] results = null;
                        byte[] timestamp = null;

                        long lRet = // sessioninfo.Channel.
                            channel.GetBiblioInfos(
                            null,
                            strPath,
                            "",
                            formats,
                            out results,
                            out timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "获得书目记录 '" + strPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (lRet == 0)
                            continue;

                        if (results == null || results.Length < 1)
                        {
                            strError = "results error {837C0AC5-F257-45F6-BABC-1495F5243D85}";
                            goto ERROR1;
                        }

                        string strXml = results[0];

                        // 将XML书目记录转换为MARC格式
                        string strMarc = "";
                        string strOutMarcSyntax = "";

                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strXml,
                            false,
                            "", // strMarcSyntax
                            out strOutMarcSyntax,
                            out strMarc,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "记录从XML格式转换为MARC格式时出错: " + strError;
                            goto ERROR1;
                        }

                        byte[] baResult = null;
                        // 将MARC机内格式转换为ISO2709格式
                        // parameters:
                        //      strSourceMARC   [in]机内格式MARC记录。
                        //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                        //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                        //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = MarcUtil.CvtJineiToISO2709(
                            strMarc,
                            strOutMarcSyntax,
                            targetEncoding,
                            "",     // 2019/6/11
                            out baResult,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "将MARC字符串转换为ISO2709记录时出错: " + strError;
                            goto ERROR1;
                        }

                        this.Page.Response.OutputStream.Write(baResult, 0, baResult.Length);
                    }
                }
                else
                {
                    // 记录路径文件
                    this.Page.Response.ContentType = "text/recpath";
                    string strEncodedFileName = HttpUtility.UrlEncode("记录路径.txt", Encoding.UTF8);
                    this.Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);

                    this.Page.Response.Charset = "utf-8";

                    for (int i = 0; i < paths.Count; i++)
                    {
                        if (this.Page.Response.IsClientConnected == false)
                            break;

                        string strPath = paths[i];
                        byte[] baResult = Encoding.UTF8.GetBytes(strPath + (i < paths.Count - 1 ? "\r\n" : ""));

                        this.Page.Response.OutputStream.Write(baResult, 0, baResult.Length);
                    }
                }

                this.Page.Response.End();
                return;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }
        ERROR1:
            this.Page.Response.ContentType = "text/html";
            this.Page.Response.Charset = "utf-8";
            this.SetDebugInfo("errorinfo", strError);
        }

        void remove_from_mybookshelf_Click(object sender, EventArgs e)
        {
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (string.IsNullOrEmpty(sessioninfo.UserID) == true)
            {
                strError = "请先登录，然后才能使用 '从我的书架移除' 功能";
                goto ERROR1;
            }

            List<string> recpathlist = this.GetCheckedPath();
            if (recpathlist.Count == 0)
            {
                strError = "尚未选择要从我的书架移除的事项";
                goto ERROR1;
            }

            string strResultsetFilename = CacheBuilder.GetMyBookshelfFilename(
        app,
        sessioninfo);
            if (String.IsNullOrEmpty(strResultsetFilename) == true)
            {
                strError = "获得我的书架结果集文件失败";
                goto ERROR1;
            }
            int nRet = CacheBuilder.RemoveFromResultset(recpathlist,
        strResultsetFilename,
        out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            this.SelectAll(false);

            //this.SetDebugInfo("succeedinfo", "共有 " + recpathlist.Count.ToString()
            //    + " 个事项成功从“我的书架”中移除。");
            this.SetDebugInfo("succeedinfo",
                string.Format(this.GetString("共有n个事项成功从我的书架中移除"), recpathlist.Count.ToString()));
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void add_to_mybookshelf_Click(object sender, EventArgs e)
        {
            /*
            if (this.AddToMyshelf != null)
                this.AddToMyshelf(this, e);
             * */
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (string.IsNullOrEmpty(sessioninfo.UserID) == true)
            {
                strError = "请先登录，然后才能使用 '加入我的书架' 功能";
                goto ERROR1;
            }

            List<string> recpathlist = this.GetCheckedPath();
            if (recpathlist.Count == 0)
            {
                strError = "尚未选择要加入我的书架的事项";
                goto ERROR1;
            }

            string strResultsetFilename = CacheBuilder.GetMyBookshelfFilename(
        app,
        sessioninfo);
            if (String.IsNullOrEmpty(strResultsetFilename) == true)
            {
                strError = "获得我的书架结果集文件失败";
                goto ERROR1;
            }
            int nRet = CacheBuilder.AddToResultset(recpathlist,
        strResultsetFilename,
        true,
        out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            //this.SetDebugInfo("succeedinfo", "共有 " + recpathlist.Count.ToString()
            //    + " 个事项成功加入“我的书架”。<a href='./mybookshelf.aspx'>点这里可去“我的书架”</a>");
            string strText = string.Format(this.GetString("共有n个事项成功加入我的书架"),
                recpathlist.Count.ToString());
            strText += "<a href='./mybookshelf.aspx'>" + this.GetString("点这里可去我的书架") + "</a>";
            this.SetDebugInfo("succeedinfo",
                strText);
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        // 跳到指定的页号
        void gotobutton_Click(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)this.FindControl("gotopageno");

            int nPageNo = 0;

            try
            {
                nPageNo = Convert.ToInt32(textbox.Text);
            }
            catch
            {
                return;
            }

            if (nPageNo < 1)
                nPageNo = 1;
            this.StartIndex = this.PageMaxLines * (nPageNo - 1);
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

#if NO
        void FillFormatList(DropDownList formatlist,
            List<string> formatnames)
        {
            formatlist.Items.Clear();
            for (int i = 0; i < formatnames.Count; i++)
            {
                ListItem item = new ListItem(formatnames[i], formatnames[i]);
                formatlist.Items.Add(item);
            }
        }
#endif

        public static List<Control> FindControl(Control start,
            Type type)
        {
            List<Control> results = new List<Control>();
            foreach (Control control in start.Controls)
            {
                if (control.GetType().Equals(type))
                    results.Add(control);

                results.AddRange(FindControl(control, type));
            }

            return results;
        }
        /*
        Control FindControl(string strID)
        {
            foreach (Control control in this.Controls)
            {
                if (control.ID == strID)
                    return control;
            }

            return null;
        }
         */

        int FillBrowseCols(LibraryChannel channel,
            ref ArrayList aLine,
            out string strError)
        {
            strError = "";

            string[] paths = new string[aLine.Count];
            for (int i = 0; i < aLine.Count; i++)
            {
                paths[i] = ((string[])aLine[i])[0];
            }

            // TODO: 一次调用的数目不一定能满足
            Record[] search_results = null;
            long lRet = channel.GetBrowseRecords(
                null,
                paths,
                "id,cols", // strStyle,
                out search_results,
                out strError);
            if (lRet == -1)
                return -1;

            ArrayList results = new ArrayList();
            for (int i = 0; i < search_results.Length; i++)
            {
                Record record = search_results[i];
                string[] cols = new string[record.Cols.Length + 1];
                cols[0] = record.Path;
                Array.Copy(record.Cols,
                    0,
                    cols,
                    1,
                    record.Cols.Length);
                results.Add(cols);
            }

            aLine = results;
            return 0;
        }

        // 汇总数据库名列表
        static void BuildDbNameList(
            string strDbName,
            ref List<string> dbnames)
        {
            for (int i = 0; i < dbnames.Count; i++)
            {
                if (dbnames[i] == strDbName)
                    return; // 已经在里面
            }
            dbnames.Add(strDbName);

        }

        // 逗号分割的列表。每个事项格式为 路径 + "|" + 时间戳
        string RecPathList
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("recpathlist");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("recpathlist");
                s.Value = value;
            }
        }

#if NO
        SessionInfo m_managerSession = null;

        SessionInfo GetManagerSession(OpacApplication app)
        {
            // 临时的SessionInfo对象
            if (m_managerSession == null)
            {
                // SessionInfo session
                m_managerSession = new SessionInfo(app);
                m_managerSession.UserID = app.ManagerUserName;
                m_managerSession.Password = app.ManagerPassword;
                m_managerSession.IsReader = false;
            }
            return m_managerSession;
        }

        void CloseManagerSession()
        {
            if (m_managerSession != null)
            {
                m_managerSession.CloseSession();
                m_managerSession = null;
            }
        }
#endif

        public static void ParseOffsetString(string strText,
            out int nStart,
            out int nLength)
        {
            nStart = 0;
            nLength = -1;
            if (string.IsNullOrEmpty(strText) == true)
                return;

            string[] parts = strText.Split(new char[] { ',' });
            if (parts.Length >= 1)
            {
                Int32.TryParse(parts[0], out nStart);
            }

            if (parts.Length >= 2)
            {
                Int32.TryParse(parts[1], out nLength);
            }
        }

        // 兑现获取浏览记录
        protected override void Render(HtmlTextWriter writer)
        {
            string strError = "";
            int nRet = 0;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            int nPageNo = this.StartIndex / this.PageMaxLines;

            SetTitle(String.IsNullOrEmpty(this.Title) == true ? this.GetString("命中结果") : this.Title);

            SetResultInfo();

            List<string> recpathlist = new List<string>();

            /*
            DropDownList formatlist = (DropDownList)FindControl("formatlist");
            string strFormat = formatlist.Text;
             * */
            TabControl format_control = (TabControl)this.FindControl("format_control");

            // 2012/11/25
            if (string.IsNullOrEmpty(this.FormatName) == false)
                format_control.ActiveTab = this.FormatName;
            if (String.IsNullOrEmpty(format_control.ActiveTab) == true)
                format_control.ActiveTab = this.DefaultFormatName;

            // 换算为语言相关的字符串
            if (String.IsNullOrEmpty(format_control.ActiveTab) == false)
            {
                format_control.ActiveTab = app.GetBrowseFormatName(format_control.ActiveTab,
                    this.Lang);
            }
            // FillTabControl(format_control);

            string strFormat = format_control.ActiveTab;

            if (String.IsNullOrEmpty(strFormat) == true)
                strFormat = "brief";

            bool bMarcVisible = this.MarcVisible;

#if NO
            if (bMarcVisible == false && strFormat == "MARC")
            {
                base.Render(writer);
                return;
            }
#endif

            if (this.ResultCount != 0)
            {
                // string strHash = channel.GetHashCode().ToString();
                ArrayList aLine = null;    // 各个记录的浏览列。数组的每个元素是string [] 类型
                List<string> titles = new List<string>();   // 各个记录的标题

                long lRet = 0;
                bool bFillBrowse = false;   // 浏览列是否填充过

            REDO:

                string strResultsetFilename = this.ResultsetFilename;
                if (String.IsNullOrEmpty(strResultsetFilename) == false)
                    app.ResultsetLocks.LockForRead(strResultsetFilename, 500);
                LibraryChannel channel = sessioninfo.GetChannel(true);
                try
                {
                    DpResultSet resultset = null;

                    try
                    {
                        if (String.IsNullOrEmpty(this.ResultsetFilename) == false)
                        {
                            int nOffsetStart = 0;
                            int nOffsetLength = -1;
                            ParseOffsetString(this.ResultsetOffset,
            out nOffsetStart,
            out nOffsetLength);
                            resultset = new DpResultSet(false, false);
                            try
                            {
                                resultset.Attach(this.ResultsetFilename,
                                    this.ResultsetFilename + ".index");
                            }
                            catch (Exception ex)
                            {
                                this.SetErrorInfo(ex.Message); // 显示出错信息
                                this.ResultsetFilename = "";

                                // 2016/1/23
                                resultset.Dispose();
                                resultset = null;
                                goto REDO;
                            }

                            int nLength = 0;
                            if (nOffsetLength == -1)
                                nLength = (int)resultset.Count - nOffsetStart;
                            else
                                nLength = nOffsetLength;

                            aLine = new ArrayList();
                            for (int i = 0; i < Math.Min(nLength, this.PageMaxLines); i++)
                            {
                                int index = this.StartIndex + i + nOffsetStart;
                                if (index >= resultset.Count)
                                    break;
                                DpRecord record = resultset[index];

                                titles.Add(record.BrowseText);

                                string[] cols = new string[1];
                                cols[0] = record.m_id;

                                // 翻译数据库名

                                aLine.Add(cols);
                            }
                        }
                        else
                        {
                            aLine = new ArrayList();
                            Record[] searchresults = null;
                            long lStart = this.StartIndex;
                            long lCount = this.PageMaxLines;
                            long lTotalCount = 0;
                            for (; ; )
                            {
                                lRet = // sessioninfo.Channel.
                                    channel.GetSearchResult(
                                    null,
                                    this.ResultSetName,
                                    this.StartIndex,
                                    this.PageMaxLines,
                                    "id,cols",
                                    this.Lang,
                                    out searchresults,
                                    out strError);
                                if (lRet == -1 && searchresults == null)
                                    goto ERROR1;

                                if (lRet != -1)
                                    lTotalCount = lRet;

                                for (int k = 0; k < searchresults.Length; k++)
                                {
                                    Record record = searchresults[k];

                                    // 2025/5/17
                                    if (record.Cols == null)
                                        record.Cols = new string[0];

                                    string[] cols = new string[record.Cols.Length + 1];
                                    cols[0] = record.Path;
                                    Array.Copy(record.Cols,
                                        0,
                                        cols,
                                        1,
                                        record.Cols.Length);
                                    aLine.Add(cols);
                                }

                                lStart += searchresults.Length;
                                lCount -= searchresults.Length;
                                if (lStart >= lTotalCount)
                                    break;
                                if (lStart + lCount > lTotalCount)
                                    lCount = lTotalCount - lStart;
                                if (lCount <= 0)
                                    break;
                            }
                        }

                        // 本页出现的数据库名字列表
                        // List<string> dbnames = new List<string>();

                        // 显示本页中的浏览行
                        for (int i = 0; i < this.PageMaxLines; i++)
                        {
                            if (this.Page.Response.IsClientConnected == false)
                                return;

                            string[] cols = null;
                            if (i < aLine.Count)
                                cols = (string[])aLine[i];

                            if (cols != null)
                                recpathlist.Add(cols[0]);

                            string strTitle = "";
                            if (i < titles.Count)
                                strTitle = titles[i];

                            PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                            if (line == null)
                            {
                                PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                                PlaceHolder content = (PlaceHolder)this.FindControl("content");

                                line = this.NewContentLine(content, i, insertpoint);
                            }

                            BiblioControl bibliocontrol = (BiblioControl)line.FindControl("line" + Convert.ToString(i) + "_biblio");
                            MarcControl marccontrol = (MarcControl)line.FindControl("line" + Convert.ToString(i) + "_marc");
                            ItemsControl itemscontrol = (ItemsControl)line.FindControl("line" + Convert.ToString(i) + "_items");
                            CommentsControl commentscontrol = (CommentsControl)line.FindControl("line" + Convert.ToString(i) + "_comments");

                            ItemControl itemcontrol = (ItemControl)line.FindControl("line" + Convert.ToString(i) + "_item");
                            CommentControl commentcontrol = (CommentControl)line.FindControl("line" + Convert.ToString(i) + "_comment");

                            PlaceHolder layout_holder = (PlaceHolder)this.FindControl("line" + Convert.ToString(i) + "_layout");

                            LiteralControl no = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_no");
                            Panel title = (Panel)this.FindControl("line" + Convert.ToString(i) + "_title");
                            HyperLink pathcontrol = (HyperLink)this.FindControl("line" + Convert.ToString(i) + "_path");
                            LiteralControl contentcontrol = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_content");

                            CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");

                            // 序号
                            string strNo = "&nbsp;";
                            if (cols != null)
                                strNo = Convert.ToString(i + this.StartIndex + 1);

                            no.Text = "<div>" + strNo + "</div>";

                            string strContent = "&nbsp;";

                            // 看看路径是不是实体库路径
                            if (cols != null)
                            {
                                string strItemPath = cols[0];

                                string strItemDbName = StringUtil.GetDbName(strItemPath);
                                if (app.IsItemDbName(strItemDbName) == true)
                                {
                                    // 插入控件
                                    line.Controls.Add(new LiteralControl(
                                        "<td class='item'>"
                                    ));

                                    // return:
                                    //      -1  出错
                                    //      0   本册已经隐藏显示
                                    //      1   成功
                                    nRet = itemcontrol.LoadRecord(strItemPath,
                                        out string strParentID,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        strContent = "ERROR : " + strError;
                                        goto SKIP0;
                                    }

                                    if (nRet == 0)
                                        line.Visible = false;

                                    string strBiblioDbName = "";

                                    // return:
                                    //      -1  出错
                                    //      0   没有找到
                                    //      1   找到
                                    nRet = app.GetBiblioDbNameByItemDbName(strItemDbName,
                                         out strBiblioDbName,
                                         out strError);
                                    if (nRet == -1)
                                    {
                                        strContent = "ERROR : " + strError;
                                        goto SKIP0;
                                    }
                                    if (nRet == 0 || String.IsNullOrEmpty(strBiblioDbName) == true)
                                    {
                                        if (String.IsNullOrEmpty(strError) == false)
                                            strContent = "ERROR : " + strError;
                                        else
                                            strContent = "ERROR : 没有找到和实体库名 '" + strItemDbName + "' 对应的书目库名";
                                        goto SKIP0;
                                    }

                                    bibliocontrol.Visible = true;
                                    bibliocontrol.RecPath = strBiblioDbName + "/" + strParentID;

                                    // 重新布局
                                    this.Controls.Remove(itemcontrol);
                                    this.Controls.Remove(bibliocontrol);

                                    layout_holder.Controls.Add(new LiteralControl("<table class='item_and_biblio'><tr><td class='item'>"));
                                    layout_holder.Controls.Add(itemcontrol);
                                    layout_holder.Controls.Add(new LiteralControl("</td><td class='biblio'>"));
                                    layout_holder.Controls.Add(bibliocontrol);
                                    layout_holder.Controls.Add(new LiteralControl("</td></tr></table>"));

                                    marccontrol.Visible = false;
                                    commentcontrol.Visible = false;

                                    itemscontrol.Visible = false;
                                    commentscontrol.Visible = false;
                                    goto SKIP1;
                                }
                            }

                            // 看看路径是不是评注库路径
                            if (cols != null)
                            {
                                string strItemPath = cols[0];

                                string strItemDbName = StringUtil.GetDbName(strItemPath);
                                if (app.IsCommentDbName(strItemDbName) == true)
                                {
                                    // 插入控件
                                    line.Controls.Add(new LiteralControl(
                                        "<td class='comment'>"
                                    ));

                                    string strParentID = "";
                                    nRet = commentcontrol.LoadRecord(strItemPath,
                                        out strParentID,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        strContent = "ERROR : " + strError;
                                        goto SKIP0;
                                    }

                                    string strBiblioDbName = "";

                                    // return:
                                    //      -1  出错
                                    //      0   没有找到
                                    //      1   找到
                                    nRet = app.GetBiblioDbNameByCommentDbName(strItemDbName,
                                         out strBiblioDbName,
                                         out strError);
                                    if (nRet == -1)
                                    {
                                        strContent = "ERROR : " + strError;
                                        goto SKIP0;
                                    }

                                    bibliocontrol.Visible = true;
                                    bibliocontrol.RecPath = strBiblioDbName + "/" + strParentID;

                                    // 重新布局
                                    this.Controls.Remove(commentcontrol);
                                    this.Controls.Remove(bibliocontrol);

                                    layout_holder.Controls.Add(new LiteralControl("<table class='comment_and_biblio'><tr><td class='comment'>"));
                                    layout_holder.Controls.Add(commentcontrol);
                                    layout_holder.Controls.Add(new LiteralControl("</td><td class='biblio'>"));
                                    layout_holder.Controls.Add(bibliocontrol);
                                    layout_holder.Controls.Add(new LiteralControl("</td></tr></table>"));

                                    marccontrol.Visible = false;
                                    itemcontrol.Visible = false;

                                    itemscontrol.Visible = false;
                                    commentscontrol.Visible = false;
                                    goto SKIP1;
                                }
                            }

                            bool bIsBiblioType = false;
                            if (cols != null)
                            {
                                string strDbName = StringUtil.GetDbName(cols[0]);

                                // 2012/7/9
                                string strTempName = app.GetCfgBiblioDbName(strDbName);
                                if (string.IsNullOrEmpty(strTempName) == true)
                                {
                                    strError = "数据库 '" + strDbName + "' 没有定义";
                                    strContent = "ERROR : " + strError;
                                    goto SKIP0;
                                }

                                strDbName = strTempName;

                                // 获得显示格式
                                BrowseFormat format = null;
                                nRet = app.GetBrowseFormat(
                                    strDbName,
                                    "详细",
                                    out format,
                                    out strError);
                                if (nRet == -1 || nRet == 0)
                                {
                                    bIsBiblioType = false;
                                }

                                if (format != null
                                    && format.Type == "biblio")
                                {
                                    bIsBiblioType = true;
                                }
                            }

                            // title
                            title.Controls.Add(new LiteralControl(strTitle));

                            // 路径
                            string strPath = "&nbsp;";
                            if (cols != null)
                                strPath = cols[0];
                            pathcontrol.Text = strPath;
                            // 只有书目库才给出锚点
                            if (bIsBiblioType == true)
                                pathcontrol.NavigateUrl = "./book.aspx?BiblioRecPath=" + HttpUtility.UrlEncode(strPath);
                            else
                                pathcontrol.NavigateUrl = "";

                            // 内容

                            if (cols != null)
                            {
                                string strDbName = StringUtil.GetDbName(cols[0]);
                                string strLang = "";

                                // 2012/7/9
                                string strTempName = app.GetCfgBiblioDbName(strDbName, out strLang);
                                if (string.IsNullOrEmpty(strTempName) == true)
                                {
                                    strError = "数据库 '" + strDbName + "' 没有定义";
                                    strContent = "ERROR : " + strError;
                                    goto SKIP0;
                                }
                                strDbName = strTempName;

                                string strBiblioRecPath = cols[0];
                                if (strLang != this.Lang)
                                {
                                    strBiblioRecPath = app.GetLangBiblioRecPath(this.Lang,
                                        strBiblioRecPath);
                                    pathcontrol.Text = strBiblioRecPath;
                                }

                                if (OpacApplication.IsKernelFormatName(strFormat,
                                    "brief") == true)
                                {
                                    if (resultset != null
                                        && bFillBrowse == false)
                                    {
                                        nRet = FillBrowseCols(//sessioninfo.Channel,
                                            channel,
                                            ref aLine,
                                            out strError);
                                        if (nRet == -1)
                                            goto SKIP0;
                                        if (i < aLine.Count)
                                            cols = (string[])aLine[i];
                                        bFillBrowse = true;
                                    }


                                    strContent = BuildBrowseContent(app, sessioninfo,
                                        strDbName, cols);
                                }
                                else if (OpacApplication.IsKernelFormatName(strFormat,
                                    "MARC") == true)
                                {
                                    // 插入控件
                                    line.Controls.Add(new LiteralControl(
                                        "<td class='marc'>"
                                    ));

                                    bibliocontrol.Visible = false;
                                    if (bMarcVisible == false)
                                        marccontrol.Visible = false;
                                    else
                                        marccontrol.RecPath = cols[0];
                                    itemscontrol.Visible = false;
                                    commentscontrol.Visible = false;
                                    itemcontrol.Visible = false;
                                    commentcontrol.Visible = false;
                                    goto SKIP1;
                                }
                                else
                                {
                                    // 获得显示格式
                                    BrowseFormat format = null;
                                    nRet = app.GetBrowseFormat(
                                        strDbName,
                                        strFormat,
                                        out format,
                                        out strError);
                                    if (nRet == -1)
                                        goto SKIP0;

                                    if (nRet == 0)
                                    {
                                        if (resultset != null
                                            && bFillBrowse == false)
                                        {
                                            nRet = FillBrowseCols(// sessioninfo.Channel,
                                                channel,
                                                ref aLine,
                                                out strError);
                                            if (nRet == -1)
                                                goto SKIP0;
                                            if (i < aLine.Count)
                                                cols = (string[])aLine[i];
                                            bFillBrowse = true;
                                        }

                                        // 显示格式不存在, 只好用浏览格式了
                                        strContent = BuildBrowseContent(app, sessioninfo,
                                            strDbName,
                                            cols);
                                        goto SKIP0;
                                    }

                                    if (format.Type == "biblio")
                                    {
                                        // 插入控件
                                        line.Controls.Add(new LiteralControl(
                                            "<td class='biblio'>"
                                        ));

                                        /*
                                        BookControl bookcontrol = new BookControl();
                                        placeholder.Controls.Add(bookcontrol);
                                        bookcontrol.BiblioRecPath = cols[0];
                                         */

                                        bibliocontrol.RecPath = strBiblioRecPath;   // cols[0];
                                        marccontrol.Visible = false;
                                        itemcontrol.Visible = false;
                                        commentcontrol.Visible = false;
                                        itemscontrol.BiblioRecPath = strBiblioRecPath; // cols[0];
                                        commentscontrol.BiblioRecPath = strBiblioRecPath; // cols[0];

                                        {
                                            string strBiblioDbName = StringUtil.GetDbName(cols[0]);
                                            string strCommentDbName = "";
                                            // return:
                                            //      -1  出错
                                            //      0   没有找到(书目库)
                                            //      1   找到
                                            nRet = app.GetCommentDbName(strBiblioDbName,
                                                out strCommentDbName,
                                                out strError);
                                            if (nRet == -1)
                                                goto SKIP0;
                                            if (String.IsNullOrEmpty(strCommentDbName) == true)
                                                commentscontrol.Visible = false;
                                        }

                                        goto SKIP1;
                                    }

                                    // 读者记录怎么办? 显示MyLibrary控件?

                                    // 获得本地配置文件
                                    string strLocalPath = "";

                                    string strRemotePath = BrowseFormat.CanonicalizeScriptFileName(
                                        strDbName,
                                        format.ScriptFileName);

                                    nRet = app.CfgsMap.MapFileToLocal(
                                        channel,    // GetManagerSession(app).Channel,
                                        strRemotePath,
                                        out strLocalPath,
                                        out strError);
                                    if (nRet == -1)
                                        goto SKIP0;
                                    if (nRet == 0)
                                    {
                                        // 配置文件不存在, 只好用浏览格式了
                                        strContent = BuildBrowseContent(app, sessioninfo,
                                            strDbName, cols);
                                        goto SKIP0;
                                    }

                                    bool bFltx = false;
                                    // 如果是一般.cs文件, 还需要获得.cs.ref配置文件
                                    if (OpacApplication.IsCsFileName(
                                        format.ScriptFileName) == true)
                                    {

                                        string strTempPath = "";
                                        nRet = app.CfgsMap.MapFileToLocal(
                                            channel,    // GetManagerSession(app).Channel,
                                            strRemotePath + ".ref",
                                            out strTempPath,
                                            out strError);
                                        if (nRet == -1)
                                            goto SKIP0;
                                        bFltx = false;
                                    }
                                    else
                                    {
                                        bFltx = true;
                                    }

                                    // 将种记录数据从XML格式转换为HTML格式
                                    lRet = // sessioninfo.Channel.
                                        channel.GetBiblioInfo(
                                       null,
                                       cols[0],
                                       "",
                                       "xml",
                                       out string strBiblioXml,
                                       out strError);
                                    if (lRet == -1)
                                    {
                                        strError = "获得种记录 '" + cols[0] + "' 时出错: " + strError;
                                        strContent = strError;
                                        goto SKIP0;
                                    }

                                    if (bFltx == true)
                                    {
                                        // string strFilterFileName = app.CfgDir + "\\opacdetail.fltx";
                                        nRet = app.ConvertBiblioXmlToHtml(
                                                strLocalPath,
                                                strBiblioXml,
                                                cols[0],
                                                out strContent,
                                                out KeyValueCollection result_params,
                                                out strError);
                                    }
                                    else
                                    {
                                        nRet = app.ConvertRecordXmlToHtml(
                                            strLocalPath,
                                            strLocalPath + ".ref",
                                            strBiblioXml,
                                            cols[0],    // 2009/10/18
                                            out strContent,
                                            out strError);

                                        if (nRet == -2)
                                        {
                                            nRet = app.ConvertReaderXmlToHtml(
                                                sessioninfo,
                                                strLocalPath,
                                                strLocalPath + ".ref",
                                                strBiblioXml,
                                                cols[0],    // 2009/10/18
                                                OperType.None,
                                                null,
                                                "",
                                                out strContent,
                                                out strError);
                                        }
                                    }
                                    if (nRet == -1)
                                    {
                                        strContent = "ERROR : " + strError;
                                        goto SKIP0;
                                    }

                                }

                            }

                        SKIP0:

                            contentcontrol.Text = strContent;
                            bibliocontrol.Visible = false;
                            marccontrol.Visible = false;
                            itemscontrol.Visible = false;
                            commentscontrol.Visible = false;
                            itemcontrol.Visible = false;
                            commentcontrol.Visible = false;

                        SKIP1:

                            if (cols == null)
                                checkbox.Visible = false;
                            continue;
                        } // end of for
                    }
                    finally
                    {
                        if (resultset != null)
                        {
                            string strTemp1 = "";
                            string strTemp2 = "";
                            resultset.Detach(out strTemp1, out strTemp2);
                        }
                    }
                }
                finally
                {
                    if (String.IsNullOrEmpty(strResultsetFilename) == false)
                        app.ResultsetLocks.UnlockForRead(strResultsetFilename);

#if NO
                    CloseManagerSession();
#endif
                    sessioninfo.ReturnChannel(channel);
                }

#if USE_LINECOUNT
                this.LineCount = Math.Max(this.LineCount, this.PageMaxLines);
#endif

                /*
                // 得到显示格式列表
                List<string> formatnames = null;
                nRet = app.GetBrowseFormatNames(dbnames,
                    out formatnames,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                formatnames.Insert(0, "浏览");

               // DropDownList formatlist = (DropDownList)FindControl("formatlist");

                FillFormatList(formatlist, formatnames);
                 */

            }
            else
            {
                // 显示空行
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                        continue;

                    BiblioControl bibliocontrol = (BiblioControl)line.FindControl("line" + Convert.ToString(i) + "_biblio");
                    MarcControl marccontrol = (MarcControl)line.FindControl("line" + Convert.ToString(i) + "_marc");
                    ItemsControl itemscontrol = (ItemsControl)line.FindControl("line" + Convert.ToString(i) + "_items");
                    CommentsControl commentscontrol = (CommentsControl)line.FindControl("line" + Convert.ToString(i) + "_comments");
                    ItemControl itemcontrol = (ItemControl)line.FindControl("line" + Convert.ToString(i) + "_item");
                    CommentControl commentcontrol = (CommentControl)line.FindControl("line" + Convert.ToString(i) + "_comment");

                    CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");

                    if (bibliocontrol != null)
                        bibliocontrol.Visible = false;
                    if (marccontrol != null)
                        marccontrol.Visible = false;
                    if (itemscontrol != null)
                        itemscontrol.Visible = false;
                    if (commentscontrol != null)
                        commentscontrol.Visible = false;
                    if (itemcontrol != null)
                        itemcontrol.Visible = false;
                    if (commentcontrol != null)
                        commentcontrol.Visible = false;

                    if (checkbox != null)
                        checkbox.Visible = false;
                }

            }

            this.RecPathList = StringUtil.MakePathList(recpathlist);

            /*
            Button add_to_mybookshelf = (Button)this.FindControl("add_to_mybookshelf");
            if (this.AddToMyshelf == null)
                add_to_mybookshelf.Visible = false;
             * */
            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);
            if (this.EnableAddToMyBookshelf == false
                || loginstate == LoginState.NotLogin
                || loginstate == LoginState.Public)
                this.Button_AddToMyBookshelf.Visible = false;
            if (this.EnableRemoveFromMyBookshelf == false
    || loginstate == LoginState.NotLogin
    || loginstate == LoginState.Public)
                this.Button_RemoveFromMyBookshelf.Visible = false;

            base.Render(writer);
            return;
        ERROR1:
            SetDebugInfo("errorinfo", strError);
            base.Render(writer);    // 注: base.Render() 要放在使用 LibraryChannel 的 try finally 括号外边。因为 Render 页面中的其他控件的时候需要 GetChannel()。放在外边可以避免叠加获取通道导致的占用通道过多情况
        }

        public Button Button_AddToMyBookshelf
        {
            get
            {
                this.EnsureChildControls();

                Button add_to_mybookshelf = (Button)this.FindControl("add_to_mybookshelf");
                return add_to_mybookshelf;
            }
        }
        public Button Button_RemoveFromMyBookshelf
        {
            get
            {
                this.EnsureChildControls();

                Button remove_from_mybookshelf = (Button)this.FindControl("remove_from_mybookshelf");
                return remove_from_mybookshelf;
            }
        }

        void lastpage_Click(object sender, EventArgs e)
        {
            int delta = this.ResultCount % this.PageMaxLines;
            if (delta > 0)
                this.StartIndex = (this.ResultCount / this.PageMaxLines) * this.PageMaxLines;
            else
                this.StartIndex = Math.Max(0, (this.ResultCount / this.PageMaxLines) * this.PageMaxLines - 1);
        }

        void nextpage_Click(object sender, EventArgs e)
        {
            this.StartIndex += this.PageMaxLines;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        void prevpage_Click(object sender, EventArgs e)
        {
            this.StartIndex -= this.PageMaxLines;
            if (this.StartIndex < 0)
                this.StartIndex = 0;
        }

        void firstpage_Click(object sender, EventArgs e)
        {
            this.StartIndex = 0;
        }

        void SetTitle(string strTitle)
        {
            LiteralControl literal = (LiteralControl)this.FindControl("wrapper_title");
            literal.Text = strTitle;
        }

        void CreatePrifix(string strTitle,
string strWrapperClass)
        {
            LiteralControl literal = new LiteralControl("<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>");
            this.Controls.Add(literal);

            literal = new LiteralControl(strTitle);
            literal.ID = "wrapper_title";
            this.Controls.Add(literal);

            literal = new LiteralControl("</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>");
            this.Controls.Add(literal);
        }

        /*
        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            return "<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>" + strTitle + "</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>";
        }
         * */

        public string GetPostfixString()
        {
            return "</div>";
        }
    }
}

