// #define TEST_LOCATION

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Threading;
using System.Resources;
using System.Globalization;
using System.Xml;

using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.Xml;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 书目检索 检索式输入界面
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BiblioSearchControl runat=server></{0}:BiblioSearchControl>")]
    public class BiblioSearchControl : WebControl, INamingContainer
    {
        public event SearchEventHandler Search;

        ResourceManager m_rm = null;

        public override void Dispose()
        {
            this.Search = null;

            base.Dispose();
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BiblioSearchControl.cs",
                typeof(BiblioSearchControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name/*"en-US"*/);

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


        string m_strDefaultVisibleMatchStyle = "";

        // 缺省的可见的匹配方式栏的值。在PanelStyle没有定义MatchStyleColumn的时候，相当重要。缺省为left(前方一致)
        public string DefaultVisibleMatchStyle
        {
            get
            {
                return this.m_strDefaultVisibleMatchStyle;
            }
            set
            {
                if (value != ""
                    && value != "left"
                    && value != "right"
                    && value != "middle"
                    && value != "exact")
                    throw new Exception("DefaultMatchStyle的值 '" + value + "' 不合法。必须为left middle right exact (空字符串)之一");

                this.m_strDefaultVisibleMatchStyle = value;
            }
        }

        string m_strDefaultHiddenMatchStyle = "";

        // 缺省的隐藏的匹配方式栏的值。在PanelStyle没有定义MatchStyleColumn的时候，相当重要。缺省为left(前方一致)
        public string DefaultHiddenMatchStyle
        {
            get
            {
                return this.m_strDefaultHiddenMatchStyle;
            }
            set
            {
                if (value != ""
                    && value != "left"
                    && value != "right"
                    && value != "middle"
                    && value != "exact")
                    throw new Exception("DefaultHiddenMatchStyle的值 '" + value + "' 不合法。必须为left middle right exact (空字符串)之一");

                this.m_strDefaultHiddenMatchStyle = value;
            }
        }


        // public string Lang = "zh";  // 内容语言?
        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        public string Location
        {
            get
            {
                this.EnsureChildControls();
#if TEST_LOCATION
                TextBox location = this.FindControl("location") as TextBox;
                if (location == null)
                    return "";
                return location.Text;
#else
                HiddenField location = this.FindControl("location") as HiddenField;
                if (location == null)
                    return "";
                return location.Value;
#endif
            }
            set
            {
                this.EnsureChildControls();
#if TEST_LOCATION
                TextBox location = this.FindControl("location") as TextBox;
                if (location != null)
                    location.Text = value;
#else
                HiddenField location = this.FindControl("location") as HiddenField;
                if (location != null)
                location.Value = value;
#endif
            }
        }

        public int LineCount
        {
            get
            {
                object o = ViewState["LineCount"];
                return (o == null) ? 4 : (int)o;
            }
            set
            {
                ViewState["LineCount"] = (object)value;
            }
        }

        public SearchPanelStyle SearchPanelStyle
        {
            get
            {
                object o = ViewState["PanelStyle"];
                return (o == null) ? (SearchPanelStyle.MatchStyleColumn
                    | SearchPanelStyle.PanelStyleSwitch
                    /*| SearchPanelStyle.Simplest
                    | SearchPanelStyle.Advance*/) : (SearchPanelStyle)o;
            }
            set
            {
                // 检查矛盾
                if ((value & SearchPanelStyle.Advance) == SearchPanelStyle.Advance
                    && (value & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                    throw new Exception("PanelStyle值中Advance和Simplest不应同时具备，只能有其中之一(或者都没有)");


                ViewState["PanelStyle"] = (object)value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }

        protected override void CreateChildControls()
        {
            // LoadDbNameInfo();

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            /*
            string strError = "";
            int nRet = app.InitialVdbs(
                out strError);
            if (nRet == -1)
            {
                this.Page.Response.Write(HttpUtility.HtmlEncode("InitialVdbs error : " + strError));
                return;
            }
             * */

            bool bSimplest = false;
            if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                bSimplest = true;
            else
                bSimplest = false;

            this.Controls.Add(new LiteralControl(
                this.GetPrefixString(
                this.GetString("检索式"),
                "content_wrapper")
                + "<table class='query'>" //  width='100%' cellspacing='1' cellpadding='4'
                ));

            // ***标题行
            PlaceHolder columntitle = new PlaceHolder();
            columntitle.ID = "columntitle";  // id用于render()时定位
            this.Controls.Add(columntitle);

            // 前段
            columntitle.Controls.Add(new LiteralControl(
                "<tr class='columntitle'><td width='10%'>"
                + this.GetString("逻辑操作")
                + "</td><td width='45%'>"
                + this.GetString("检索词")
                + "</td>"
            ));

            // 匹配方式列
            LiteralControl literal = new LiteralControl("<td width='10%'>"
                + this.GetString("匹配方式")
                + "</td>");
            columntitle.Controls.Add(literal);
            if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
                literal.Visible = true;
            else
                literal.Visible = false;

            // 后段
            columntitle.Controls.Add(new LiteralControl(
                "<td width='15%'>"
                + this.GetString("数据库")
                + "</td><td width='15%' nowrap>"
                + this.GetString("途径")
                + "</td></tr>"
            ));

            /*
            Panel panel = new Panel();
            panel.DefaultButton = "search_button";
            this.Controls.Add(panel);
             * */

            List<TextBox> textboxs = new List<TextBox>();

            for (int i = 0; i < this.LineCount; i++)
            {
                PlaceHolder line = new PlaceHolder();
                line.ID = "queryline_" + i.ToString();  // id用于render()时定位
                this.Controls.Add(line);

                // line为render()时显示和隐藏提供基础

                DropDownList list = null;

                // 文字part1
                LiteralControl part1 = new LiteralControl(
                    "<tr class='content'><td class='logic' width='10%'>");
                part1.ID = "queryline_" + i.ToString() + "_part1";  // id用于render()时定位
                line.Controls.Add(part1);

                // 逻辑运算符
                if (i == 0)
                {

                }
                else
                {
                    list = new DropDownList
                    {
                        ID = "logic" + Convert.ToString(i),
                        Width = new Unit("100%"),
                        CssClass = "logic"
                    };
                    line.Controls.Add(list);
                    FillLogicOperList(list);
                }

                // 文字part2
                LiteralControl part2 = new LiteralControl(
                    "</td><td class='word' width='45%'>"
                );
                part2.ID = "queryline_" + i.ToString() + "_part2";  // id用于render()时定位

                line.Controls.Add(part2);

                // 检索词
                TextBox box = new TextBox();
                box.ID = "word" + Convert.ToString(i);
                box.Width = new Unit("99%");
                box.CssClass = "word";
                textboxs.Add(box);
                line.Controls.Add(box);

                // 检索词后的注释
                PlaceHolder wordcomment = new PlaceHolder();
                wordcomment.ID = "queryline_" + i.ToString() + "_wordcomment";  // id用于render()时定位
                wordcomment.Visible = false;
                line.Controls.Add(wordcomment);

                //
                LiteralControl wordcommen_left = new LiteralControl(
                    "<div class='wordcomment'>"
                );
                wordcomment.Controls.Add(wordcommen_left);

                //
                LiteralControl wordcommen_text = new LiteralControl(
                    ""
                );
                wordcommen_text.ID = "queryline_" + i.ToString() + "_wordcomment_text";  // id用于render()时定位
                wordcomment.Controls.Add(wordcommen_text);

                //
                LiteralControl wordcommen_right = new LiteralControl(
                    "</div>"
                );
                wordcomment.Controls.Add(wordcommen_right);

                // 文字part3
                LiteralControl part3 = new LiteralControl(
                    "</td><td class='match' width='10%'>"
                );
                part3.ID = "queryline_" + i.ToString() + "_part3";  // id用于render()时定位
                line.Controls.Add(part3);

                // 匹配方式
                list = new DropDownList();
                list.ID = "match" + Convert.ToString(i);
                list.Width = new Unit("100%");
                list.CssClass = "match";
                line.Controls.Add(list);
                FillMatchStyleList(list);
                if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn
                    && bSimplest == false)
                {
                    // 2007/7/8
                    list.Text = this.DefaultVisibleMatchStyle == "" ? "left" : this.DefaultVisibleMatchStyle;
                }
                else
                {
                    // 列隐藏时候的值
                    // 2007/7/8
                    list.Text = this.DefaultHiddenMatchStyle == "" ? "left" : this.DefaultHiddenMatchStyle;
                }

                // 文字part4
                LiteralControl part4 = new LiteralControl("</td><td class='dbname' width='15%'>");
                part4.ID = "queryline_" + i.ToString() + "_part4";  // id用于render()时定位
                line.Controls.Add(part4);
                if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
                {
                    list.Visible = true;
                    part4.Visible = true;
                }
                else
                {
                    list.Visible = false;
                    part4.Visible = false;
                }

                // 数据库名
                DropDownList listDbName = new DropDownList();
                listDbName.ID = "db" + Convert.ToString(i);
                listDbName.Width = new Unit("100%");
                listDbName.CssClass = "dbname";

                // 数据库名字被重选后，回传，触发改变 Froms 列表内容
                listDbName.AutoPostBack = true;
                listDbName.TextChanged -= new EventHandler(this.DbNameListTextChanged);
                listDbName.TextChanged += new EventHandler(this.DbNameListTextChanged);

                line.Controls.Add(listDbName);
                FillDbNameList(listDbName);

                // 文字part5
                LiteralControl part5 = new LiteralControl(
                    "</td><td class='from' width='15%'>"
                );
                part5.ID = "queryline_" + i.ToString() + "_part5";  // id用于render()时定位
                line.Controls.Add(part5);

                // 检索途径
                list = new DropDownList();
                list.ID = "from" + Convert.ToString(i);
                list.Width = new Unit("100%");
                list.Text = "test";
                list.CssClass = "from";
                line.Controls.Add(list);

                // 2010/5/5
                HiddenField usedDbName = new HiddenField();
                usedDbName.ID = "useddbname" + i.ToString();
                usedDbName.Value = "";
                line.Controls.Add(usedDbName);

#if NO
                if (this.Page.IsPostBack == false)
                {
                    string strDbName = listDbName.Text;
                    // FillFromList(list, strDbName, usedDbName);
                    FillFromList(list);
                }
#endif
                FillFromList(list);

                // 文字part6
                LiteralControl part6 = new LiteralControl(
                    "</td></tr>"
                );
                part6.ID = "queryline_" + i.ToString() + "_part6";  // id用于render()时定位

                line.Controls.Add(part6);
            }

            // 计算出列数
            int nCols = 4;
            if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
            {
                nCols = 5;
            }

            // 检索按钮
            this.Controls.Add(new LiteralControl(
                "<tr class='cmdline'><td></td><td colspan='" + Convert.ToString(nCols - 1) + "'>"
            ));

            // Search Button
            Button button = new Button();
            button.ID = "search_button";
            button.Text = this.GetString("检索");
            button.CssClass = "search";
            button.Click += new EventHandler(this.SearchButton_Click);
            this.Controls.Add(button);

            // 增加前端事件代码,以便响应回车按钮
            for (int i = 0; i < textboxs.Count; i++)
            {
                TextBox box = textboxs[i];
                box.Attributes.Add("onkeydown", "if(event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('" + button.ClientID + "').click();return cancelClick();}} else {return true}; "); // 原来为 button.UniqueID 也可用
            }

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));

            // 调试信息行
            PlaceHolder resultline = new PlaceHolder();
            resultline.ID = "resultline";
            this.Controls.Add(resultline);

            CreateResultLine(resultline);

            {
                PlaceHolder line = (PlaceHolder)FindControl("resultline");
                TableCell cell = (TableCell)line.FindControl("resultinfo");
                /*
                button.Attributes.Add("onclick", "document.getElementById('" + cell.ClientID + "').innerText = '"
                    + this.GetString("正在检索...")
                    + "';return true;");
                 * */
                button.Attributes.Add("onclick", "document.getElementById('" + cell.ClientID + "').innerHTML = '<div>"
    + HttpUtility.HtmlEncode(this.GetString("正在检索..."))
    + "</div>';return true;");

            }

            // 面板风格选择列表
            literal = new LiteralControl("<tr class='panelstyle'><td colspan='" + Convert.ToString(nCols) + "'>"
                + this.GetString("检索面板风格")
                + " ");
            this.Controls.Add(literal);
            if ((this.SearchPanelStyle & SearchPanelStyle.PanelStyleSwitch) == SearchPanelStyle.PanelStyleSwitch)
                literal.Visible = true;
            else
                literal.Visible = false;

            DropDownList panelstyle = new DropDownList();
            panelstyle.ID = "panelstyle";
            panelstyle.CssClass = "panelstyle";
            // panelstyle.Width = new Unit("100%");
            panelstyle.AutoPostBack = true;
            panelstyle.TextChanged -= new EventHandler(panelstyle_TextChanged);
            panelstyle.TextChanged += new EventHandler(panelstyle_TextChanged);
            this.Controls.Add(panelstyle);
            if ((this.SearchPanelStyle & SearchPanelStyle.PanelStyleSwitch) == SearchPanelStyle.PanelStyleSwitch)
                panelstyle.Visible = true;
            else
                panelstyle.Visible = false;

            FillPanelStyleList(panelstyle);

            literal = new LiteralControl("</td></tr>");
            this.Controls.Add(literal);
            if ((this.SearchPanelStyle & SearchPanelStyle.PanelStyleSwitch) == SearchPanelStyle.PanelStyleSwitch)
                literal.Visible = true;
            else
                literal.Visible = false;

            // Xml检索式
            literal = new LiteralControl("<tr class='xml'><td colspan='" + Convert.ToString(nCols) + "'>");
            this.Controls.Add(literal);
            if ((this.SearchPanelStyle & SearchPanelStyle.QueryXml) == SearchPanelStyle.QueryXml)
                literal.Visible = true;
            else
                literal.Visible = false;

            TextBox xmlbox = new TextBox();
            xmlbox.ID = "queryxml";
            xmlbox.TextMode = TextBoxMode.MultiLine;
            xmlbox.Width = new Unit("100%");
            xmlbox.Rows = 5;
            this.Controls.Add(xmlbox);
            if ((this.SearchPanelStyle & SearchPanelStyle.QueryXml) == SearchPanelStyle.QueryXml)
                xmlbox.Visible = true;
            else
                xmlbox.Visible = false;

            // 2016/5/14
#if TEST_LOCATION
            TextBox location = new TextBox();
            location.ID = "location";
            this.Controls.Add(location);
#else
            HiddenField location = new HiddenField();
            location.ID = "location";
            this.Controls.Add(location);
#endif

            literal = new LiteralControl("</td></tr>");
            this.Controls.Add(literal);
            if ((this.SearchPanelStyle & SearchPanelStyle.QueryXml) == SearchPanelStyle.QueryXml)
                literal.Visible = true;
            else
                literal.Visible = false;

            this.Controls.Add(new LiteralControl(
               "</table>" + this.GetPostfixString()
               ));
        }

        void CreateResultLine(PlaceHolder line)
        {
            TableRow row = new TableRow();
            row.CssClass = "resultline";
            line.Controls.Add(row);

            TableCell cell = new TableCell();
            cell.ID = "resultinfo";
            cell.Text = "";
            cell.ColumnSpan = 13;
            row.Controls.Add(cell);
        }

        void SetResultInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("resultline");
            TableCell cell = (TableCell)line.FindControl("resultinfo");
            if (cell.Controls.Count > 0)
            {
                Control temp = cell.Controls[0];

                cell.Controls.RemoveAt(0);

                temp.Dispose(); // 2016/1/23
            }
            LiteralControl literal = new LiteralControl();
            literal.Text = "<div>" + HttpUtility.HtmlEncode(strText) + "</div>";
            cell.Controls.Add(literal);
        }

        void panelstyle_TextChanged(object sender, EventArgs e)
        {
            DropDownList panelstyle = (DropDownList)sender;

            string strStyleName = panelstyle.Text;

            if (strStyleName == "simplest")
            {
                if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == SearchPanelStyle.Advance)
                    this.SearchPanelStyle -= SearchPanelStyle.Advance;
                this.SearchPanelStyle |= SearchPanelStyle.Simplest;
            }
            else if (strStyleName == "simple")
            {
                if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == SearchPanelStyle.Advance)
                    this.SearchPanelStyle -= SearchPanelStyle.Advance;

                if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                    this.SearchPanelStyle -= SearchPanelStyle.Simplest;

            }
            else if (strStyleName == "advance")
            {
                if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                    this.SearchPanelStyle -= SearchPanelStyle.Simplest;

                this.SearchPanelStyle |= SearchPanelStyle.Advance;
            }
            else
            {
            }

            // 怎么刷新？
        }

        void DbNameListTextChanged(object sender, EventArgs e)
        {
            string strDbName = "";
            DropDownList fromlist = null;
            HiddenField usedDbName = null;

            // 定位到同一行的FromList Control
            for (int i = 0; i < this.LineCount; i++)
            {
                string strID = "db" + Convert.ToString(i);
                Control control = this.FindControl(strID);
                if (control == sender)
                {
                    strID = "from" + Convert.ToString(i);
                    fromlist = (DropDownList)this.FindControl(strID);
                    strDbName = ((DropDownList)control).Text;
                    usedDbName = (HiddenField)this.FindControl("useddbname" + Convert.ToString(i));
                    goto FOUND;
                }
            }

            return;
        FOUND:
            if (usedDbName.Value != strDbName)
                FillFromList(fromlist, strDbName, usedDbName);
        }

        void FillFromList(DropDownList fromlist,
            string strDbName,
            HiddenField usedDbName)
        {
            // string strError = "";
            usedDbName.Value = strDbName;

            // 填入fromlist
            fromlist.Items.Clear();
            fromlist.Items.Add(this.GetString("quoted_all"));   // "<全部>"或者"<all>"

            // 2007/10/9
            if (String.IsNullOrEmpty(strDbName) == true
                || strDbName.ToLower() == "<all>"
                || strDbName == "<全部>")
            {
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器

            // 获得特定语言下的From名称列表
            List<string> froms = vdb.GetFroms(this.Lang);
            // bool bFoundOldText = false;
            for (int i = 0; i < froms.Count; i++)
            {
                ListItem item = new ListItem(froms[i], froms[i]);
                fromlist.Items.Add(item);

                /*
                if (usedFromName.Value == froms[i])
                    item.Selected = true;
                 * */
            }

            return;
            /*
        ERROR1:
            TextBox xmlbox = (TextBox)FindControl("queryxml");
            if (xmlbox != null)
            {
                xmlbox.Text = strError;
            }
             */
        }

        void FillFromList(DropDownList fromlist)
        {
            // string strError = "";

            // 填入fromlist
            fromlist.Items.Clear();
            fromlist.Items.Add(this.GetString("quoted_all"));   // "<全部>"或者"<all>"

            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            string strError = "";
#if NO
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            BiblioDbFromInfo[] infos = null;
            long lRet = sessioninfo.Channel.ListDbFroms(null,
    "biblio",
    this.Lang,
    out infos,
    out strError);
            if (lRet == -1)
            {
                ListItem item = new ListItem(strError, strError);
                fromlist.Items.Add(item);
                return;
            }
#endif
            DbFromInfo[] infos = null;
            if (app.vdbs != null)
            {
                int nRet = app.vdbs.GetFroms(this.Lang,
                    out infos,
                    out strError);
                if (nRet == -1)
                {
                    ListItem item = new ListItem(strError, strError);
                    fromlist.Items.Add(item);
                    return;
                }
            }
            else   // 2015/1/26
            {
                ListItem item = new ListItem("vdbs == null", strError);
                fromlist.Items.Add(item);
                return;
            }

            foreach (DbFromInfo info in infos)
            {
                ListItem item = new ListItem(info.Caption, info.Style);
                fromlist.Items.Add(item);
            }

            return;
        }


        protected void OnSearch(object sender, SearchEventArgs e)
        {
            if (this.Search != null)
            {
                this.Search(sender, e);
                this.SetResultInfo(e.ErrorInfo);
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            int nSearchMaxResultCount = 5000;
            // 收集检索式
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app != null)
                nSearchMaxResultCount = app.SearchMaxResultCount;

            SearchEventArgs es = new SearchEventArgs();
            // es.QueryXml = "<test />";

            string strError = "";
            int nRet = BuildQueryXml(
                nSearchMaxResultCount,
                out es.QueryXml,
                out strError);
            if (nRet == -1)
            {
                this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
                this.Page.Response.End();
                return;
            }

            TextBox xmlbox = (TextBox)FindControl("queryxml");
            if (xmlbox != null)
            {
                xmlbox.Text = es.QueryXml;
            }


            OnSearch(sender, es);
        }

        /*
        Control FindControl(string strID)
        {
            foreach(Control control in this.Controls)
            {
                if (control.ID == strID)
                    return control;
            }

            return null;
        }
         */

        public string SearchStyle
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                XmlNode nodeBiblioSearch = app.WebUiDom.DocumentElement.SelectSingleNode("biblioSearch");
                if (nodeBiblioSearch != null)
                    return DomUtil.GetAttr(nodeBiblioSearch,
                        "searchStyle");
                return "";
            }
        }

        // 创建XML检索式
        // TODO: 需要改造为每个dbname都支持dbnamelist
        private int BuildQueryXml(
            int nMaxCount,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strLocationFilter = this.Location;  // (string)this.Page.Session["librarycode"];
            if (string.IsNullOrEmpty(strLocationFilter) == false
                && nMaxCount != -1)
                nMaxCount *= 10;    // 在有馆藏地过滤的情况下把极限值放大十倍 2016/5/15

            bool bSimple = false;
            if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == 0
                && (this.SearchPanelStyle & SearchPanelStyle.Simplest) == 0)
                bSimple = true;
            else
                bSimple = false;

            bool bSimplest = false;
            if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                bSimplest = true;
            else
                bSimplest = false;

            bool bDesc = StringUtil.IsInList("desc", this.SearchStyle);

            string strNotFound = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            int nUsed = 0;
            for (int i = 0; i < ((bSimple == true || bSimplest == true) ? 1 : this.LineCount); i++)
            {
                DropDownList list = null;

                // 数据库名
                string strDbName = "";
                string strID = "db" + Convert.ToString(i);
                list = (DropDownList)FindControl(strID);
                strDbName = list.Text;

                // 数据库是不是虚拟库?
                if (app.vdbs == null)   // 2015/1/26
                {
                    strError = "vdbs == null";
                    return -1;
                }

                VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器

                string strLogic = "";
                strID = "logic" + Convert.ToString(i);
                if (i != 0)
                {
                    list = (DropDownList)FindControl(strID);
                    strLogic = list.Text;
                }

                string strWord = "";
                strID = "word" + Convert.ToString(i);
                TextBox textbox = (TextBox)FindControl(strID);
                strWord = textbox.Text;

                string strMatchStyle = "";
                if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
                {
                    strID = "match" + Convert.ToString(i);
                    list = (DropDownList)FindControl(strID);
                    strMatchStyle = list.Text;
                }
                else
                {
                    // 2007/7/8
                    // strMatchStyle = this.DefaultHiddenMatchStyle;
                    strMatchStyle = this.DefaultHiddenMatchStyle == "" ? "left" : this.DefaultHiddenMatchStyle;
                }

                string strFrom = "";
                strID = "from" + Convert.ToString(i);
                list = (DropDownList)FindControl(strID);
                strFrom = list.SelectedItem.Value;  // list.Text;

                //

                if (String.IsNullOrEmpty(strWord) == true && i != 0)
                    continue;

                string strOneDbQuery = "";

                // 如果是虚拟库
                if (vdb != null && vdb.IsVirtual == true)
                {
                    int nRet = app.BuildVirtualQuery(
                        new List<VirtualDatabase> { vdb },
                        strWord,
                        strFrom,
                        strMatchStyle,
                        nMaxCount,
                        this.SearchStyle,
                        out strOneDbQuery,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    string strTargetList = "";

                    if (String.IsNullOrEmpty(strDbName) == true
                        || strDbName.ToLower() == "<all>"
                        || strDbName == "<全部>")
                    {
                        List<string> found_dup = new List<string>();    // 用于去重

                        // 2008/6/6
                        if (app.vdbs.Count == 0)
                        {
                            strError = "目前 opac.xml 中 <virtualDatabases> 内尚未具备检索目标。(注：需要在 dp2Library 服务器的 library.xml 中配置相关事项，然后重启 dp2OPAC)";
                            return -1;
                        }

                        // 所有虚拟库包含的去重后的物理库名 和全部物理名 (整体去重一次)
                        // 要注意检查特定的from名在物理库中是否存在，如果不存在则排除该库名
                        for (int j = 0; j < app.vdbs.Count; j++)
                        {
                            VirtualDatabase temp_vdb = app.vdbs[j];  // 需要增加一个索引器

                            // 忽略具有notInAll属性的库
                            if (temp_vdb.NotInAll == true)
                                continue;

                            List<string> realdbs = new List<string>();

                            // if (temp_vdb.IsVirtual == true)
                            realdbs = temp_vdb.GetRealDbNames();

                            for (int k = 0; k < realdbs.Count; k++)
                            {
                                // 数据库名
                                string strOneDbName = realdbs[k];

                                if (found_dup.IndexOf(strOneDbName) != -1)
                                    continue;

                                // 2012/11/14
                                // style名替换为caption名
                                string strFromCaptions = temp_vdb.BuildCaptionListByStyleList(strFrom, this.Lang);
                                if (String.IsNullOrEmpty(strFromCaptions) == true)
                                {
                                    strNotFound += "数据库 '" + strDbName + "' 中不存在检索途径 '" + strFrom + "'; ";
                                    continue;
                                }

                                strTargetList += StringUtil.GetXmlStringSimple(strOneDbName + ":" + strFromCaptions) + ";";

                                found_dup.Add(strOneDbName);
                            }
                        }

                    }
                    else
                    {
                        VirtualDatabase temp = app.vdbs[strDbName];
                        if (temp == null)
                        {
                            strNotFound += "库名 '" + strDbName + "' 没有找到; ";
                            continue;
                        }
                        string strFromCaptions = temp.BuildCaptionListByStyleList(strFrom, this.Lang);
                        if (String.IsNullOrEmpty(strFromCaptions) == true)
                        {
                            strNotFound += "数据库 '" + strDbName + "' 中不存在检索途径 '" + strFrom + "'; ";
                            continue;
                        }

                        strTargetList = StringUtil.GetXmlStringSimple(strDbName + ":" + strFromCaptions); // 2007/9/14
                    }

                    // 2008/6/6
                    if (String.IsNullOrEmpty(strTargetList) == true)
                    {
                        strError = "不具备检索目标";
                        return -1;
                    }

                    // 2011/11/23
                    // 检索词为空的时候对匹配方式"exact"要特殊处理
                    if (string.IsNullOrEmpty(strWord) == true
                        && strMatchStyle == "exact")
                        strMatchStyle = "left";

                    // 2012/5/16
                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (strWord.IndexOf("-") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                        else if (String.IsNullOrEmpty(strWord) == false)
                        {
                            // 2008/3/9
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                    }
                    else if (strFrom == "操作时间" || strFrom == "OperTime"
                        || strFrom == "出版时间" || strFrom == "Publish Time")
                    {
                        // 如果为范围式
                        if (strWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 2012/3/29
                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strWord = "~";
                            }
                        }

                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                    }

                    // strFrom为"<全部>"，表示利用全部途径检索，内核是支持这样使用的

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    strOneDbQuery = "<target list='"
                        + strTargetList
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                        + "<word>"
                        + StringUtil.GetXmlStringSimple(strWord)
                        + "</word><match>"
                        + StringUtil.GetXmlStringSimple(strMatchStyle) // 2007/9/14
                        + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType>"
                        + "<maxCount>" + nMaxCount.ToString() + "</maxCount></item><lang>zh</lang></target>";
                }

                if (i > 0)
                    strXml += "<operator value='" + strLogic + "'/>";

                strXml += strOneDbQuery;
                // strXml += "line "+Convert.ToString(i)+" logic:" + strLogic + ", word=" + strWord + ", matchstyle=" + strMatchStyle + ", db=" + strDbName + ", from=" + strFrom + "\r\n";

                nUsed++;
            }

            if (nUsed > 1)
            {
                strXml = "<group>" + strXml + "</group>";
            }

            if (string.IsNullOrEmpty(strXml) == true)
            {
                if (string.IsNullOrEmpty(strNotFound) == false)
                {
                    strError = strNotFound;
                    return -1;
                }
                strError = "检索式为空";
                return -1;
            }

            if (string.IsNullOrEmpty(strLocationFilter) == false)
            {
                string strLocationQueryXml = "<item resultset='#" + strLocationFilter + "' />";
                strXml = "<group>" + strXml + "<operator value='AND'/>" + strLocationQueryXml + "</group>";
            }

            // 2018/12/5
            // 对书目检索的额外过滤
            if (string.IsNullOrEmpty(app.BiblioFilter) == false)
            {
                string strOperator = "AND";
                string strFilter = app.BiblioFilter;
                if (strFilter.StartsWith("-"))
                {
                    strFilter = strFilter.Substring(1);
                    strOperator = "SUB";
                }
                string strSubQueryXml = "<item resultset='#" + strFilter + "' />";
                strXml = $"<group>{strXml}<operator value='{strOperator}'/>{strSubQueryXml}</group>";
            }

            return 0;
        }

        void FillPanelStyleList(DropDownList list)
        {
            /*
            if (list.Items.Count != 0)
                return;
             * */
            list.Items.Clear();

            ListItem item = null;

            // 最简单
            item = new ListItem(this.GetString("最简单"),
                "simplest");
            if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
            {
                item.Selected = true;
            }
            else
            {
                item.Selected = false;
            }
            list.Items.Add(item);

            // 简单
            item = new ListItem(this.GetString("简单"),
                "simple");
            if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == 0
                && (this.SearchPanelStyle & SearchPanelStyle.Simplest) == 0)
            {
                item.Selected = true;
            }
            else
            {
                item.Selected = false;
            }
            list.Items.Add(item);

            // 高级
            item = new ListItem(this.GetString("高级"),
                "advance");
            if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == SearchPanelStyle.Advance)
            {
                item.Selected = true;
            }
            else
            {
                item.Selected = false;
            }

            list.Items.Add(item);
        }

        void FillLogicOperList(DropDownList list)
        {
            if (list.Items.Count != 0)
                return;

            ListItem item = new ListItem(this.GetString("LogicOR"),   // "OR 或"
                "OR");
            list.Items.Add(item);
            item = new ListItem(this.GetString("LogicAND"), // "AND 与"
                "AND");
            list.Items.Add(item);
            item = new ListItem(this.GetString("LogicSUB"),   // "SUB 减"
                "SUB");
            list.Items.Add(item);
        }

        void FillMatchStyleList(DropDownList list)
        {
            /*
            if (list.Items.Count != 0)
                return; // 不能优化。优化后语言变化的时候不能刷新 2009/6/17
             * */

            list.Items.Clear(); // 2009/6/17

            // 匹配方式列是否可见?
            bool bVisible = (this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn;

            string strDefault = (bVisible == true ? this.DefaultVisibleMatchStyle : this.DefaultHiddenMatchStyle);

            ListItem item = new ListItem(this.GetString("前方一致"),
                "left");
            list.Items.Add(item);
            if (strDefault == "left")
                item.Selected = true;
            else
                item.Selected = false;

            item = new ListItem(this.GetString("精确一致"),
                "exact");
            list.Items.Add(item);
            if (strDefault == "exact")
                item.Selected = true;
            else
                item.Selected = false;

            item = new ListItem(this.GetString("后方一致"),
                "right");
            list.Items.Add(item);
            if (strDefault == "right")
                item.Selected = true;
            else
                item.Selected = false;


            item = new ListItem(this.GetString("中间一致"),
                "middle");
            list.Items.Add(item);
            if (strDefault == "middle")
                item.Selected = true;
            else
                item.Selected = false;

        }

        /*
        void LoadDbNameInfo()
        {
            string strError = "";

            if (this.rootitems != null)
                return;

            CirculationApplication app = (CirculationApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];


            Channel channel = sessioninfo.Channels.GetChannel(app.WsUrl);

            ResInfoItem[] tempitems = null;
            long lRet = channel.DoDir("",
                this.Lang,
                out tempitems,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            this.rootitems = tempitems;

            if (this.fromitems == null)
                this.fromitems = new Hashtable();

            foreach (ResInfoItem item in rootitems)
            {
                if (item.Type != ResTree.RESTYPE_DB)
                    continue;

                string strDbName = item.Name;


                ResInfoItem[] items = (ResInfoItem[])this.fromitems[strDbName];
                if (items == null)
                {
                    lRet = channel.DoDir(strDbName,
                        this.Lang,
                        out items,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    this.fromitems[strDbName] = items;
                }
            }

            return;
        ERROR1:
            throw new Exception(strError);
        }
         */

        // 填充数据库名列表
        void FillDbNameList(DropDownList list)
        {
            /*
            if (list.Items.Count != 0)
                return; // 不能优化。优化后语言发生变化时，列表不能刷新 2009/6/17
             * */

            list.Items.Clear(); // 2009/6/17

            /*
            CirculationApplication app = (CirculationApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];


            if (rootitems == null && this.Channels != null)
            {
            
                Channel channel = this.Channels.GetChannel(this.ServerUrl);

                long lRet = channel.DoDir("",
                    this.Lang,
                    out rootitems,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            
                Debug.Assert(false, "");
            }
             */

            /*
            if (rootitems != null)
            {
                for (int i = 0; i < rootitems.Length; i++)
                {
                    if (rootitems[i].Type != ResTree.RESTYPE_DB)
                        continue;

                    ListItem item = new ListItem(rootitems[i].Name, rootitems[i].Name);
                    list.Items.Add(item);
                }
            }
             */

            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            /*
            ResInfoItem[] rootitems = app.vdbs.root_dir_results;
            for (int i = 0; i < rootitems.Length; i++)
            {
                if (rootitems[i].Type != ResTree.RESTYPE_DB)
                    continue;

                ListItem item = new ListItem(rootitems[i].Name, rootitems[i].Name);
                list.Items.Add(item);
            }
             */

            list.Items.Add(this.GetString("quoted_all"));   // <全部> 2007/10/9

            if (app.vdbs != null)   // 2015/1/26
            {
                // for (int i = 0; i < app.vdbs.Count; i++)
                foreach(var database in app.vdbs)
                {
                    // string strName = app.vdbs[i].GetName(this.Lang);

                    // 2020/11/17
                    if (database.Visible == false)
                        continue;

                    string strName = database.GetName(this.Lang);

                    ListItem item = new ListItem(strName, strName);
                    list.Items.Add(item);
                }
            }
            else
            {
                ListItem item = new ListItem("vdbs == null");
                list.Items.Add(item);
            }
            return;
        }

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

        public string GetPostfixString()
        {
            return "</div>";
        }

        protected override void Render(HtmlTextWriter writer)
        {
            bool bSimple = false;
            if ((this.SearchPanelStyle & SearchPanelStyle.Advance) == 0
                && (this.SearchPanelStyle & SearchPanelStyle.Simplest) == 0)
                bSimple = true;
            else
                bSimple = false;

            bool bSimplest = false;
            if ((this.SearchPanelStyle & SearchPanelStyle.Simplest) == SearchPanelStyle.Simplest)
                bSimplest = true;
            else
                bSimplest = false;

            for (int i = 0; i < this.LineCount; i++)
            {
                PlaceHolder line = (PlaceHolder)this.FindControl("queryline_" + i.ToString());
                Debug.Assert(line != null, "");

                // 当bSimple或bSimplest为true的时候，从第二行起以后的控件都处于隐藏状态
                if ((bSimple == true || bSimplest == true)
                    && i != 0)
                    line.Visible = false;
                else
                    line.Visible = true;

                // 匹配方式
                DropDownList match = (DropDownList)this.FindControl("match" + i.ToString());
                Debug.Assert(match != null, "");

                if (this.Page.IsPostBack == false)  // 2008/1/13 防止postback的时候固执地重设为DefaultVisibleMatchStyle
                {
                    if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn
                        && bSimplest == false)
                    {
                        // 2007/7/8
                        match.Text = this.DefaultVisibleMatchStyle == "" ? "left" : this.DefaultVisibleMatchStyle;
                    }
                    else
                    {
                        // 列隐藏时候的值
                        // 2007/7/8
                        match.Text = this.DefaultHiddenMatchStyle == "" ? "left" : this.DefaultHiddenMatchStyle;
                    }
                }
            }

            // columntitle
            PlaceHolder columntitle = (PlaceHolder)this.FindControl("columntitle");
            Debug.Assert(columntitle != null, "");
            // columntitle.Controls.Clear();
            if (bSimple == true || bSimplest == true)
                columntitle.Visible = false;
            else
                columntitle.Visible = true;

            if (bSimple == true || bSimplest == true)
            {

                // part1
                LiteralControl part1 = (LiteralControl)this.FindControl("queryline_0_part1");
                Debug.Assert(part1 != null, "");
                // part1.Text = "\r\n<tr class='content'><td width='20%'>"+this.GetString("逻辑运算符")+"</td><td class='logic'>";
                part1.Visible = false;

                // "</td></tr>\r\n"

                // part2
                LiteralControl part2 = (LiteralControl)this.FindControl("queryline_0_part2");
                Debug.Assert(part2 != null, "");
                part2.Text = "<tr class='content'><td class='left' width='20%'>"
                    + this.GetString("检索词")
                    + "</td><td class='word'>";

                // 检索词
                TextBox box = (TextBox)this.FindControl("word0");
                Debug.Assert(box != null, "");
                box.Width = Unit.Empty;

                // 检索词后的注释
                PlaceHolder wordcomment = (PlaceHolder)this.FindControl("queryline_0_wordcomment");
                Debug.Assert(wordcomment != null, "");
                if (bSimplest == true)
                    wordcomment.Visible = true;
                else
                    wordcomment.Visible = false;





                // part3
                LiteralControl part3 = (LiteralControl)this.FindControl("queryline_0_part3");
                Debug.Assert(part3 != null, "");
                part3.Text = "</td></tr>\r\n<tr class='content'><td class='left' width='20%'>"
                    + this.GetString("匹配方式")
                    + "</td><td class='match'>";
                if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
                    part3.Visible = true;
                else
                    part3.Visible = false;

                if (bSimplest == true)
                    part3.Visible = false;


                // 匹配方式
                DropDownList match = (DropDownList)this.FindControl("match0");
                Debug.Assert(match != null, "");
                match.Width = Unit.Empty;
                if ((this.SearchPanelStyle & SearchPanelStyle.MatchStyleColumn) == SearchPanelStyle.MatchStyleColumn)
                    match.Visible = true;
                else
                    match.Visible = false;
                if (bSimplest == true)
                    match.Visible = false;


                // part4
                LiteralControl part4 = (LiteralControl)this.FindControl("queryline_0_part4");
                Debug.Assert(part4 != null, "");
                part4.Text = "</td></tr>\r\n<tr class='content'><td class='left' width='20%'>"
                    + this.GetString("数据库名")
                    + "</td><td class='dbname'>";
                part4.Visible = true;
                if (bSimplest == true)
                    part4.Visible = false;


                // 数据库名
                DropDownList dbname = (DropDownList)this.FindControl("db0");
                Debug.Assert(dbname != null, "");
                dbname.Width = Unit.Empty;
                if (bSimplest == true)
                    dbname.Visible = false;


                // part5
                LiteralControl part5 = (LiteralControl)this.FindControl("queryline_0_part5");
                Debug.Assert(part5 != null, "");
                part5.Text = "</td></tr>\r\n<tr class='content'><td class='left' width='20%'>"
                    + this.GetString("检索途径")
                    + "</td><td class='from'>";
                if (bSimplest == true)
                    part5.Visible = false;


                // 检索途径
                DropDownList from = (DropDownList)this.FindControl("from0");
                Debug.Assert(from != null, "");
                from.Width = Unit.Empty;
                if (bSimplest == true)
                    from.Visible = false;


                // part6不变


                // wordcomment_text
                LiteralControl wordcommen_text = (LiteralControl)this.FindControl("queryline_0_wordcomment_text");
                Debug.Assert(wordcommen_text != null, "");
                if (bSimplest == true)
                {
                    string strAll = this.GetString("quoted_all");

                    wordcommen_text.Text = HttpUtility.HtmlEncode(
                        string.Format(
                        this.GetString("search_parameters_list"),
                        String.IsNullOrEmpty(dbname.Text) == false ? dbname.Text : strAll,
                        String.IsNullOrEmpty(from.Text) == false ? from.Text : strAll,
                        GetMatchStyleCaption(match.Text))
                        );

                    // search_parameters_list
                    // "检索将按照后列参数进行: 数据库名: {0}; 检索途径: {1}; 匹配方式: {2}";

                }
            }

            base.Render(writer);
        }

        string GetMatchStyleCaption(string strStyle)
        {
            strStyle = strStyle.ToLower();

            if (strStyle == "left")
                return this.GetString("前方一致");
            if (strStyle == "exact")
                return this.GetString("精确一致");
            if (strStyle == "right")
                return this.GetString("后方一致");
            if (strStyle == "middle")
                return this.GetString("中间一致");

            return string.Format(this.GetString("unknown_match_style"),
                strStyle);

            // return "未知的匹配方式  " + strStyle;
        }

    }

    // 面板显示风格。可组合。
    public enum SearchPanelStyle
    {
        None = 0x00,  // 
        MatchStyleColumn = 0x01,  // 出现匹配方式列
        QueryXml = 0x02,   // 出现Xml检索式显示
        PanelStyleSwitch = 0x04,    // 出现面板风格选择下拉列表
        Advance = 0x10,    // 高级方式面板风格，多行允许逻辑检索。否则是单行的简单方式
        Simplest = 0x20,    // 最简单方式的面板风格。否则，根据是否具有Advance，如果没有，就是一般简单方式；否则就是高级方式
    }

    public delegate void SearchEventHandler(object sender,
        SearchEventArgs e);

    public class SearchEventArgs : EventArgs
    {
        public string QueryXml = "";

        // 2008/11/24
        public string ErrorInfo = "";   // 如果有值，通常表示检索出错
    }
}
