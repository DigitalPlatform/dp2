using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Diagnostics;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:CommentsControl runat=server></{0}:CommentsControl>")]
    public class CommentsControl : WebControl, INamingContainer
    {
        public string NewTitle = "";    // 填充评注编辑区的title内容
        public string NewState = "";    // 填充评注编辑区的state内容

        public bool MinimizeNewReviewEdtior = false;  // 初始时是否隐藏新创建评注编辑区域

        // List<string> m_recpathlist = new List<string>();

        // public bool Active = true;
        public event WantFocusEventHandler WantFocus = null;

        public string RefID = "";   // 参考ID。用于指定要显示的评注记录

        public string BiblioRecPath = "";
        public string WarningText = "";

        public bool Wrapper = false;

        public string FocusRecPath = "";    // 需要显示出来的焦点评注记录的路径

        public CommentDispStyle CommentDispStyle = CommentDispStyle.Comments;

        // ItemConverter ItemConverter = null;


        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.CommentsControl.cs",
                typeof(CommentsControl).Module.Assembly);

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

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

#if USE_LINECOUNT
        // 评注行数
        public int LineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "CommentsControl_LineCount"];
                return (o == null) ? 5 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "CommentsControl_LineCount"] = (object)value;
            }
        }
#endif


        public void Clear()
        {
            this.StartIndex = 0;
        }

        public int StartIndex
        {
            get
            {
                object o = ViewState[this.ID + "CommentsControl_StartIndex"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "CommentsControl_StartIndex"] = value;
            }
        }

        public int PageMaxLines
        {
            get
            {
                object o = ViewState[this.ID + "CommentsControl_PageMaxLines"];
                if (o == null)
                    return 10;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "CommentsControl_PageMaxLines"] = value;
            }
        }

        // 计算出页码总数
        public int GetPageCount(int nResultCount)
        {
            int nDelta = nResultCount % this.PageMaxLines;
            if (nDelta > 0)
                return (nResultCount / this.PageMaxLines) + 1;
            return (nResultCount / this.PageMaxLines);
        }

        // 设置结果集有关数量参数
        public void SetResultInfo(int nResultCount)
        {
            int nPageNo = this.StartIndex / this.PageMaxLines;

            int nPageCount = GetPageCount(nResultCount);

            LiteralControl resultinfo = (LiteralControl)this.FindControl("resultinfo");

            if (nResultCount != 0)
            {
                resultinfo.Text = string.Format(this.GetString("评注共n个"),  // "评注共 {0} 个"
                    nResultCount.ToString());
                this.SetInfo(resultinfo.Text);
            }
            else
            {
                resultinfo.Text = this.GetString("无评注");   // "(无评注)"
                this.SetInfo(resultinfo.Text);
            }
#if NO

            LiteralControl maxpagecount = (LiteralControl)this.FindControl("maxpagecount");
            maxpagecount.Text = string.Format(this.GetString("共n页"),  // (共 {0} 页)
                nPageCount.ToString());

            LiteralControl currentpageno = (LiteralControl)this.FindControl("currentpageno");
            currentpageno.Text = Convert.ToString(nPageNo + 1);
#endif
            PageSwitcherControl pager = (PageSwitcherControl)this.FindControl("pager");
            pager.CurrentPageNo = nPageNo;
            pager.TotalCount = nPageCount;
            if (nPageCount <= 1)
            {
                pager.Visible = false;
            }
            else
            {
                pager.Visible = true;
            }

            PlaceHolder pageswitcher = (PlaceHolder)this.FindControl("pageswitcher");
            if (nPageCount <= 1)
            {
                pageswitcher.Visible = false;
                resultinfo.Visible = false; // 2009/6/10
            }
            else
            {
                pageswitcher.Visible = true;
                resultinfo.Visible = true;
            }
        }

        void CreateInfoLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='infoline'><td colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "infotext";
            literal.Text = "";
            line.Controls.Add(literal);

#if NO

            PlaceHolder show_newreview_button_holder = new PlaceHolder();
            show_newreview_button_holder.ID = "show_newreview_button_holder";
            line.Controls.Add(show_newreview_button_holder);

            show_newreview_button_holder.Controls.Add(new LiteralControl("<div class='show_newreview'>"));

            Button button = new Button();
            button.Text = GetString("新评注");
            button.OnClientClick = "$(this).parents('.infoline').nextAll('TR.newreview').show('slow'); return cancelClick();";
            show_newreview_button_holder.Controls.Add(button);

            show_newreview_button_holder.Controls.Add(new LiteralControl("</div>"));
#endif

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void SetInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("infoline");
            LiteralControl text = (LiteralControl)line.FindControl("infotext");
            text.Text = "<div class='infotext'>" + strText + "<div>";
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

        void SetEditErrorInfo(string strSpanClass,
    string strText)
        {
            LiteralControl text = (LiteralControl)this.FindControl("edit_errorinfo");
            text.Visible = true;
            text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }

        void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            text.Text = strText;
        }

        void SetDebugInfo(string strSpanClass,
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

#if NO
        // [delete]
        void SetCommentInfo(string strSpanClass,
    string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("commentline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("comment");
            text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }
#endif

        public int ResultCount
        {
            get
            {
                return GetResultCount();
            }
            set
            {
                SetResultCount(value);
            }
        }

        void SetResultCount(int nResultCount)
        {
            this.EnsureChildControls();

            // PlaceHolder line = (PlaceHolder)FindControl("debugline");
            HiddenField hidden = (HiddenField)this.FindControl("resultcount");
            hidden.Value = nResultCount.ToString();
        }

        int GetResultCount()
        {
            this.EnsureChildControls();

            // PlaceHolder line = (PlaceHolder)FindControl("debugline");
            HiddenField hidden = (HiddenField)this.FindControl("resultcount");
            if (String.IsNullOrEmpty(hidden.Value) == true)
                return 0;

            return Convert.ToInt32(hidden.Value);
        }

        void CreateCmdLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<tr class='cmdline'><td colspan='2'>"));

            // LiteralControl literal = null;
            ///

            PlaceHolder pageswitcher = new PlaceHolder();
            pageswitcher.ID = "pageswitcher";
            line.Controls.Add(pageswitcher);

            // 信息文字

            pageswitcher.Controls.Add(new LiteralControl(
                "<div class='pager'> -- "
            ));


            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            pageswitcher.Controls.Add(resultinfo);

            pageswitcher.Controls.Add(new LiteralControl(
                " -- "
            ));

            PageSwitcherControl pager = new PageSwitcherControl();
            pager.Wrapper = false;  // 不要外围的<div>
            pager.ID = "pager";
            pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            pageswitcher.Controls.Add(pager);

            pageswitcher.Controls.Add(new LiteralControl(
    "</div>"));
            ///

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void pager_PageSwitch(object sender, PageSwitchEventArgs e)
        {
            /*
            if (String.IsNullOrEmpty(this.EditLineNumbers) == false)
            {
                // TODO: 需要先结束这个
                this.EditLineNumbers = "";
                this.EditAction = "";
                this.ClearEdit();
            }*/

            this.ClearEdit();

            this.FocusRecPath = "";

            this.StartIndex = this.PageMaxLines * e.GotoPageNo;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
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

        void CreateInputLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<tr class='newreview'"));


            LiteralControl newreview_editor_style = new LiteralControl();
            newreview_editor_style.ID = "newreview_editor_style";
            newreview_editor_style.Text = "";
            line.Controls.Add(newreview_editor_style);

            line.Controls.Add(new LiteralControl("><td colspan='2'>"));

            CommentControl editor = new CommentControl();
            editor.ID = "editor";
            editor.EditAction = "new";
            if (string.IsNullOrEmpty(this.NewTitle) == false)
                editor.EditTitle = this.NewTitle;
            if (string.IsNullOrEmpty(this.NewState) == false)
                editor.EditState = this.NewState;
            editor.WantFocus += new WantFocusEventHandler(editor_WantFocus);
            editor.Submited += new SumitedEventHandler(editor_Submited);
            if (this.MinimizeNewReviewEdtior == true)
                editor.Minimized = "true";
            line.Controls.Add(editor);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void editor_Submited(object sender, SubmitedEventArgs e)
        {
            if (this.MinimizeNewReviewEdtior == true)
            {
                // 创建操作完成后，还是把editor最小化
                if (e.Action == "new")
                {
                    CommentControl commentcontrol = (CommentControl)sender;
                    commentcontrol.Minimized = "true";
                }
            }
        }

        void editor_WantFocus(object sender, WantFocusEventArgs e)
        {
            if (this.MinimizeNewReviewEdtior == true)
            {
                // editor说它要放弃焦点
                // 那么就最小化它
                if (e.Focus == false)
                {
                    CommentControl commentcontrol = (CommentControl)sender;
                    commentcontrol.Minimized = "true";
                }
                // 继续发给父对象
                if (this.WantFocus != null)
                {
                    WantFocusEventArgs e1 = new WantFocusEventArgs();
                    e1.Focus = e.Focus;
                    this.WantFocus(this, e1);
                }
            }


        }

        // 放弃编辑
        void cancel_button_Click(object sender, EventArgs e)
        {
            // this.EditLineNumbers = "";
            // this.EditAction = "";
            this.ClearEdit();
        }

        void ClearEdit()
        {
            this.EnsureChildControls();

            CommentControl editor = (CommentControl)this.FindControl("editor");
            editor.Clear();

            /*
            TextBox edit_title = (TextBox)this.FindControl("edit_title");
            TextBox edit_content = (TextBox)this.FindControl("edit_content");
            // TextBox edit_recpath = (TextBox)this.FindControl("edit_recpath");
            LiteralControl recordinfo = (LiteralControl)this.FindControl("recordinfo");
            HiddenField comment_recpath = (HiddenField)this.FindControl("comment_recpath");
            edit_title.Text = "";
            edit_content.Text = "";
            recordinfo.Text = "";
            comment_recpath.Value = "";

            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_censor");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_locked");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            checkbox.Checked = false;
             * */
        }

        /*
        void CreateEditArea(PlaceHolder edit_holder, int i)
        {
            edit_holder.Controls.Clear();
            edit_holder.Controls.Add(new LiteralControl("<table class='edit'>"));

            // 标题
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("标题");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_title = new TextBox();
            edit_title.Text = "";
            edit_title.ID = "edit_title" + i.ToString();
            edit_title.CssClass = "title";
            edit_holder.Controls.Add(edit_title);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 正文
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("正文");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_content = new TextBox();
            edit_content.Text = "";
            edit_content.ID = "edit_content" + i.ToString();
            edit_content.CssClass = "content";
            edit_content.TextMode = TextBoxMode.MultiLine;
            edit_holder.Controls.Add(edit_content);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 创建者
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("创建者");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_creator = new TextBox();
            edit_creator.Text = "";
            edit_creator.ID = "edit_creator" + i.ToString();
            edit_creator.CssClass = "creator";
            edit_creator.ReadOnly = true;
            edit_holder.Controls.Add(edit_creator);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 提交
            edit_holder.Controls.Add(new LiteralControl("<tr><td colspan='2'>"));

            Button submit_button = new Button();
            submit_button.ID = "submit_button" + i.ToString();
            submit_button.Text = this.GetString("提交评注");
            submit_button.Click += new EventHandler(submit_button_Click);
            edit_holder.Controls.Add(submit_button);

            edit_holder.Controls.Add(new LiteralControl("</td></tr></table>"));
        }
         * */

        PlaceHolder NewLine(int index,
            Control insertbefore)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + Convert.ToString(index);

            if (insertbefore == null)
                this.Controls.Add(line);
            else
            {
                int pos = this.Controls.IndexOf(insertbefore);
                this.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(index) + "left";
            literal.Text = "<tr class='content'><td>";
            line.Controls.Add(literal);

            LiteralControl no = new LiteralControl();
            no.ID = "line" + Convert.ToString(index) + "_no";
            line.Controls.Add(no);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 中间文字
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(index) + "middle";
            literal.Text = "";
            line.Controls.Add(literal);

            // commentcontrol
            CommentControl commentcontrol = new CommentControl();
            commentcontrol.ID = "line_" + Convert.ToString(index) + "_comment";
            commentcontrol.WantFocus -= new WantFocusEventHandler(commentcontrol_WantFocus);
            commentcontrol.WantFocus += new WantFocusEventHandler(commentcontrol_WantFocus);
            line.Controls.Add(commentcontrol);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        void commentcontrol_WantFocus(object sender, WantFocusEventArgs e)
        {
            CommentControl source = (CommentControl)sender;
            if (e.Focus == true)
            {
                source.Active = true;

                List<Control> controls = BrowseSearchResultControl.FindControl(this,
    typeof(CommentControl));

                // 找到sender对象以外的其他CommentControl对象，把它们设置为Active = false的状态
                foreach (Control control in controls)
                {
                    CommentControl comment_control = (CommentControl)control;
                    if (comment_control == source)
                    {
                        comment_control.Active = true;
                    }
                    else
                    {
                        comment_control.Active = false;
                    }
                }
            }
            else
            {
                // 大家都为true
                List<Control> controls = BrowseSearchResultControl.FindControl(this,
    typeof(CommentControl));

                // 找到sender对象以外的其他CommentControl对象，把它们设置为Active = false的状态
                foreach (Control control in controls)
                {
                    CommentControl comment_control = (CommentControl)control;
                    comment_control.Active = true;
                }
            }

            /*
            CommentControl source = (CommentControl)sender;

            List<Control> controls = BrowseSearchResultControl.FindControl(this,
                typeof(CommentControl));

            // 找到sender对象以外的其他CommentControl对象，把它们设置为Active = false的状态
            foreach (Control control in controls)
            {
                CommentControl comment_control = (CommentControl)control;
                if (comment_control == source)
                {
                    comment_control.Active = true;
                }
                else
                {
                    comment_control.Active = false;
                }
            }

            // 把底部的editor隐藏
            CommentControl editor = (CommentControl)this.FindControl("editor");
            editor.Visible = false;
             * */

            // 继续发给父对象
            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                e1.Focus = e.Focus;
                this.WantFocus(this, e1);
            }
        }

        void SetControlActive()
        {
            if (this.Active == false)
            {
                // 如果本对象的Active == false，那么就要求包含的所有子对象的Active也要设置为false
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    CommentControl commentcontrol = (CommentControl)this.FindControl("line_" + Convert.ToString(i) + "_comment");
                    commentcontrol.Active = false;
                }

                // 把底部的editor隐藏
                PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
                inputline.Visible = false;
                return;
            }

            List<int> editmode_lineindexes = new List<int>();
            for (int i = 0; i < this.PageMaxLines; i++)
            {
                /*
                // 为每个checkbox设置正确的class
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                LiteralControl line_class = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_class");
                if (checkbox.Checked == true)
                    line_class.Text = " selected";
                else
                    line_class.Text = "";
                 * */

                CommentControl commentcontrol = (CommentControl)this.FindControl("line_" + Convert.ToString(i) + "_comment");
                // 检查是否出现至少一个编辑状态的CommentControl
                if (String.IsNullOrEmpty(commentcontrol.EditAction) == false)
                    editmode_lineindexes.Add(i);
            }

            int nEditorCount = 0;
            if (this.MinimizeNewReviewEdtior == true)
            {
                CommentControl editor = (CommentControl)this.FindControl("editor");

                if (editor.Minimized != "true")
                    nEditorCount = 1;
            }

            if (editmode_lineindexes.Count + nEditorCount > 0)
            {
                // 为每行的CommentControl控件设置Active
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    CommentControl commentcontrol = (CommentControl)this.FindControl("line_" + Convert.ToString(i) + "_comment");
                    if (editmode_lineindexes.IndexOf(i) == -1)
                    {
                        // 普通行
                        commentcontrol.Active = false;
                    }
                    else
                    {
                        // 编辑状态的行
                        commentcontrol.Active = true;
                    }
                }

                if (nEditorCount == 0)  // 如果是底部的Editor发起的，就不该隐藏它
                {
                    // 把底部的editor隐藏
                    PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
                    inputline.Visible = false;
                }
            }
            else
            {
                /*
                PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
                inputline.Visible = true;
                 * */

                // 为每行的CommentControl控件设置Active=true
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    CommentControl commentcontrol = (CommentControl)this.FindControl("line_" + Convert.ToString(i) + "_comment");
                    commentcontrol.Active = true;
                }
            }
        }

        // 布局控件
        protected override void CreateChildControls()
        {
            // 表格开头
            LiteralControl literal = new LiteralControl();
            literal.ID = "begin";
            if (this.Wrapper == true)
                literal.Text = this.GetPrefixString(
                    this.GetString("评注信息"),
                    "content_wrapper");

            string strClass = "comments";
            if (this.Wrapper == true)
                strClass += " wrapper";


            literal.Text += "<table class='" + strClass + "'>";    //  width='100%' cellspacing='1' cellpadding='4'
            this.Controls.Add(literal);

            /*
            HiddenField s = new HiddenField();
            s.ID = "editlinenumbers";
            this.Controls.Add(s);
             * */

            HiddenField active = new HiddenField();
            active.ID = "active";
            active.Value = "1";
            this.Controls.Add(active);

#if NO
            HiddenField recpathlist = new HiddenField();
            recpathlist.ID = "recpathlist";
            this.Controls.Add(recpathlist);
#endif

            // 隐藏字段，用来转值
            HiddenField hidden = new HiddenField();
            hidden.ID = "resultcount";
            this.Controls.Add(hidden);

            /*
            HiddenField editaction = new HiddenField();
            editaction.ID = "editaction";
            this.Controls.Add(editaction);
             * */


            // 信息行
            PlaceHolder infoline = new PlaceHolder();
            infoline.ID = "infoline";
            this.Controls.Add(infoline);

            CreateInfoLine(infoline);

            HiddenField biblio_recpath = new HiddenField();
            biblio_recpath.ID = "biblio_recpath";
            this.Controls.Add(biblio_recpath);

            // 正在编辑的评注记录路径
            HiddenField comment_recpath = new HiddenField();
            comment_recpath.ID = "comment_recpath";
            this.Controls.Add(comment_recpath);


            /*
            // 标题行
            PlaceHolder titleline = new PlaceHolder();
            titleline.ID = "titleline";
            this.Controls.Add(titleline);

            CreateTitleLine(titleline);
             * */


            // 每一行一个占位控件
            for (int i = 0; i < this.PageMaxLines/*this.LineCount*/; i++)
            {
                PlaceHolder line = NewLine(i, null);
                line.Visible = false;
            }

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "cmdline";
            this.Controls.Add(cmdline);

            CreateCmdLine(cmdline);


            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            debugline.Visible = false;
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);

            // 编辑行
            PlaceHolder inputline = new PlaceHolder();
            inputline.ID = "inputline";
            this.Controls.Add(inputline);

            CreateInputLine(inputline);


            // 表格结尾
            literal = new LiteralControl();
            literal.ID = "end";
            literal.Text = "</table>";
            if (this.Wrapper == true)
                literal.Text += this.GetPostfixString();

            this.Controls.Add(literal);
        }

        public bool Active
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active");
                if (String.IsNullOrEmpty(s.Value) == true)
                    return false;
                if (s.Value == "0")
                    return false;
                return true;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active");
                s.Value = value == true ? "1" : "0";
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.RefID) == true
                && String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "RefID和BiblioRecPath均为空, 无法显示";
                this.SetDebugInfo("errorinfo", strError);
                base.Render(output);
                return;
                // goto ERROR1;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strCommentDbName = "";
            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
            {
                string strBiblioDbName = ResPath.GetDbName(this.BiblioRecPath);
                // return:
                //      -1  出错
                //      0   没有找到(书目库)
                //      1   找到
                nRet = app.GetCommentDbName(strBiblioDbName,
            out strCommentDbName,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (String.IsNullOrEmpty(strCommentDbName) == true)
                {
                    this.Visible = false;
                    // base.Render(output);
                    return;
                }
            }

            HiddenField biblio_recpath = (HiddenField)this.FindControl("biblio_recpath");
            biblio_recpath.Value = this.BiblioRecPath;


            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
#if NO
            // PlaceHolder edit_holder = (PlaceHolder)this.FindControl("edit_holder");
            if (loginstate == LoginState.Public || loginstate == LoginState.NotLogin)
            {
                inputline.Visible = false;
            }
            else
            {
                inputline.Visible = true;

                if (String.IsNullOrEmpty(this.EditLineNumbers) == true)
                {
                    string strBiblioState = "";
                    {
                        string strOutputPath = "";
                        string strBiblioXml = "";
                        string strMetaData = "";
                        byte[] timestamp = null;
                        long lRet = channel.GetRes(this.BiblioRecPath,
                            out strBiblioXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            /*
                            strError = "获得种记录 '" + this.RecPath + "' 时出错: " + strError;
                            goto ERROR1;
                             * */
                        }
                        else
                        {
                            string strOutMarcSyntax = "";
                            string strMarc = "";
                            // 将MARCXML格式的xml记录转换为marc机内格式字符串
                            // parameters:
                            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                            nRet = MarcUtil.Xml2Marc(strBiblioXml,
                                true,
                                "", // this.CurMarcSyntax,
                                out strOutMarcSyntax,
                                out strMarc,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            strBiblioState = MarcDocument.GetFirstSubfield(strMarc,
                                "998",
                                "s");   // 状态
                        }
                    }

                    bool bOrderComment = false;
                    if (StringUtil.IsInList("订购征询", strBiblioState) == true)
                        bOrderComment = true;

                    HiddenField type = (HiddenField)this.FindControl("edit_type");
                    LiteralControl description = (LiteralControl)this.FindControl("edit_description");
                    PlaceHolder ordercomment_description_holder = (PlaceHolder)this.FindControl("ordercomment_description_holder");
                    // PlaceHolder recpath_holder = (PlaceHolder)this.FindControl("recpath_holder");


                    if (bOrderComment == true)
                    {
                        ordercomment_description_holder.Visible = true;
                        description.Text = this.GetString("本书正在征求订购意见") + "：";
                        type.Value = "订购征询";
                    }
                    else
                    {
                        ordercomment_description_holder.Visible = false;
                        description.Text = this.GetString("在此贡献您的书评")+"：";
                        type.Value = "书评";
                    }

                    if (sessioninfo.Account != null)
                    {
                        /*
                        TextBox edit_creator = (TextBox)this.FindControl("edit_creator");
                        edit_creator.Text = sessioninfo.Account.UserID;
                         * */
                        LiteralControl recordinfo = (LiteralControl)this.FindControl("recordinfo");
                        recordinfo.Text = this.GetString("创建者") + ": " + GetCurrentAccountDisplayName();
                        if (IsReaderHasnotDisplayName() == true)
                        {
                            recordinfo.Text += "<div class='comment'>" + this.GetString("若想以个性化的作者名字发表评注") + "，<a href='./personalinfo.aspx' target='_blank'>" + this.GetString("点这里立即添加我的显示名") + "</a></div>";
                        }
                    }

                    // recpath_holder.Visible = false;
                }

            }

#endif

            string strOutputCommentPath = "";
            string strCommentXml = "";
            string strBiblioRecPath = this.BiblioRecPath;
            byte[] comment_timestamp = null;

            // 如果this.BiblioRecPath为空, 并且要求显示同种全部评注
            // 那只能通过this.Barcode取出一个评注记录, 从中才能得知种记录的路径
            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                /*
                // 获得评注记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = app.GetCommentRecXml(
                    sessioninfo.Channels,
                    this.RefID,
                    out strCommentXml,
                    out strOutputCommentPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "参考ID为 '" + this.RefID + "' 的评注记录没有找到";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;
                 * */

                bool bGetItemXml = true;
                if ((this.CommentDispStyle & CommentDispStyle.Comments) == CommentDispStyle.Comments)
                   bGetItemXml = false;

                string strBiblio = "";

                long lRet = sessioninfo.Channel.GetCommentInfo(
                null,
                "@refid:" + this.RefID,
                // null,
                (bGetItemXml == true) ? "xml" : "", // strResultType
                out strCommentXml,
                out strOutputCommentPath,
                out comment_timestamp,
                "recpath",  // strBiblioType
                out strBiblio,
                out strBiblioRecPath,
                out strError);

                if (lRet == -1)
                    goto ERROR1;
                if (lRet > 1)
                {
                    strError = "参考ID '" + this.RefID + "' 命中 " + nRet.ToString() + " 条记录";
                    goto ERROR1;
                }

                this.BiblioRecPath = strBiblioRecPath;
            }


            // string strCommentDbName = "";  // 评注库名
            // string strBiblioRecID = ""; // 种记录id

            // 若需要取得种记录路径和id

#if NO

            // 如果需要从评注记录中获得种记录路径
            if (String.IsNullOrEmpty(this.BiblioRecPath) == true
                && strCommentXml != "")
            {
                strCommentDbName = ResPath.GetDbName(strOutputCommentPath);
                string strBiblioDbName = "";

                // 根据评注库名, 找到对应的书目库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = app.GetBiblioDbNameByCommentDbName(strCommentDbName,
                    out strBiblioDbName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "评注库 '" + strCommentDbName + "' 名在cfgs/global配置文件中没有找到对应的书目库名";
                    goto ERROR1;
                }

                // 获得评注记录中的<parent>字段
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strCommentXml);
                }
                catch (Exception ex)
                {
                    strError = "评注记录XML装载到DOM出错:" + ex.Message;
                    goto ERROR1;
                }

                strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "//parent");
                if (String.IsNullOrEmpty(strBiblioRecID) == true)
                {
                    strError = "评注记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                    goto ERROR1;
                }

                strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
            }
#endif

#if NO
            // 若需要知道评注库名
            if (String.IsNullOrEmpty(strCommentDbName) == true
                && String.IsNullOrEmpty(this.BiblioRecPath) == false)
            {
                // 根据书目库名, 找到对应的评注库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = app.GetCommentDbName(ResPath.GetDbName(this.BiblioRecPath),
                    out strCommentDbName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            if ((this.CommentDispStyle & CommentDispStyle.Comments) == CommentDispStyle.Comments)
            {
                // 已知种路径, 但不知评注库名
                strBiblioRecPath = this.BiblioRecPath;
                strBiblioRecID = ResPath.GetRecordId(this.BiblioRecPath);
            }

#endif
            // //
            //m_recpathlist.Clear();
            //this.RecPathList = "";

            long nHitCount = 0;

            if ((this.CommentDispStyle & CommentDispStyle.Comments) == CommentDispStyle.Comments)
            {


                // 检索出该种的所有评注
                sessioninfo.ItemLoad += new ItemLoadEventHandler(SessionInfo_CommentLoad);
                sessioninfo.SetStart += new SetStartEventHandler(sessioninfo_SetStart);
                // tempItemBarcodes = new List<string>();
                try
                {
                    long lRet = sessioninfo.SearchComments(
                        app,
                        strBiblioRecPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    nHitCount = lRet;

                    if (nHitCount > 0)
                    {
                        if (String.IsNullOrEmpty(this.FocusRecPath) == true)
                        {

                            nRet = sessioninfo.GetCommentsSearchResult(
                                app,
                                this.StartIndex,
                                this.PageMaxLines,
                                false,
                                this.Lang,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        else
                        {
                            int nFoundStart = -1;
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            nRet = sessioninfo.GetCommentsSearchResult(
                                app,
                                this.PageMaxLines,
                                this.FocusRecPath,
                                false,
                                this.Lang,
                                out nFoundStart,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                goto ERROR1;

                        }
                    }

                }
                finally
                {
                    sessioninfo.SetStart -= new SetStartEventHandler(sessioninfo_SetStart);
                    sessioninfo.ItemLoad -= new ItemLoadEventHandler(SessionInfo_CommentLoad);
                    //tempOutput = null;

                    // this.ItemConverter = null;
                }

                // this.ItemBarcodes = this.tempItemBarcodes;

                this.ResultCount = (int)nHitCount;

                SetResultInfo((int)nHitCount);

                bool bHasCommandButton = false;

                if (nHitCount == 0)
                {
                    // 如果一个评注也没有, 则不出现命令按钮
                    PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
                    if (cmdline != null)
                        cmdline.Visible = false;

                    // 也可用SetInfo()
                    // this.SetDebugInfo("none", this.GetString("无评注"));  // "(无评注)"
                    this.SetInfo(this.GetString("无评注"));  // "(无评注)"

                    // 不显示空的表格
                    if (this.DisplayBlankTable == false)
                    {
                        this.Visible = false;
                        return;
                    }
                }
                else if (bHasCommandButton == false
                    && this.ResultCount <= this.PageMaxLines)
                {
                    // 如果没有命令按钮，并且没有分页器, 则不出现命令按钮区域
                    PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
                    if (cmdline != null)
                        cmdline.Visible = false;

                }
            }
            else if ((this.CommentDispStyle & CommentDispStyle.Comment) == CommentDispStyle.Comment)
            {
                if (strCommentXml == "")
                    throw new Exception("评注记录尚未准备好");

                // this.tempItemBarcodes = new List<string>();

                ItemLoadEventArgs e = new ItemLoadEventArgs();
                e.Index = 0;
                e.Path = strOutputCommentPath;
                e.Count = 1;
                e.Xml = strCommentXml;
                SessionInfo_CommentLoad(this, e);

                //m_recpathlist.Add(e.Path + "|" + ByteArray.GetHexTimeStampString(e.Timestamp));

            }

            //this.RecPathList = StringUtil.MakePathList(this.m_recpathlist);

            if (String.IsNullOrEmpty(this.WarningText) == false)
                SetDebugInfo(this.WarningText);

            // 根据登录身份决定是否能发表新评注
            if (loginstate == LoginState.Public
                || loginstate == LoginState.NotLogin)
            {
                inputline.Visible = false;
            }
            else
            {
                inputline.Visible = true;

                string strBiblioState = "";
                {
                    string strBiblioXml = "";
                    /*
                    string strOutputPath = "";
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strStyle = LibraryChannel.GETRES_ALL_STYLE;

                    // TODO: 可以优化为从前面一次性获得
                    long lRet = sessioninfo.Channel.GetRes(
                        null,
                        this.BiblioRecPath,
                        strStyle,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                     * */
                    long lRet = sessioninfo.Channel.GetBiblioInfo(
    null,
    this.BiblioRecPath,
    "",
    "xml",
    out strBiblioXml,
    out strError);
                    if (lRet == -1)
                    {
                        /*
                        strError = "获得种记录 '" + this.RecPath + "' 时出错: " + strError;
                        goto ERROR1;
                         * */
                    }
                    else
                    {
                        string strOutMarcSyntax = "";
                        string strMarc = "";
                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strBiblioXml,
                            true,
                            "", // this.CurMarcSyntax,
                            out strOutMarcSyntax,
                            out strMarc,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        strBiblioState = MarcDocument.GetFirstSubfield(strMarc,
                            "998",
                            "s");   // 状态
                    }
                }

                bool bOrderComment = false;
                if (StringUtil.IsInList("订购征询", strBiblioState) == true)
                    bOrderComment = true;

                CommentControl editor = (CommentControl)this.FindControl("editor");
                editor.BiblioRecPath = strBiblioRecPath;

                if (bOrderComment == true)
                {
                    editor.EditType = "订购征询";
                }
                else
                {
                    editor.EditType = "书评";
                }

                editor.EditAction = "new";
                editor.RecPath = "";
            }

            /*
            if (this.Active == false)
            {
                inputline.Visible = false;
            }
             * */

            SetControlActive();

            /*
            LiteralControl newreview_editor_style = (LiteralControl)this.FindControl("newreview_editor_style");
            if (this.HideNewReviewEdtior == true)
                newreview_editor_style.Text = " style='DISPLAY:none'";
             * */

            base.Render(output);
            return;

        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void sessioninfo_SetStart(object sender, SetStartEventArgs e)
        {
            Debug.Assert(e.StartIndex != -1, "");
            this.StartIndex = e.StartIndex;
        }

        void SessionInfo_CommentLoad(object sender, ItemLoadEventArgs e)
        {
            int nRet = 0;
            string strError = "";

            // this.m_recpathlist.Add(e.Path + "|" + ByteArray.GetHexTimeStampString(e.Timestamp));

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            /*
            bool bManager = false;
            if (sessioninfo.Account == null
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;
             * */

            CommentControl commentcontrol = (CommentControl)this.FindControl("line_" + Convert.ToString(e.Index) + "_comment");

            if (this.Active == false)
                commentcontrol.Active = false;

            string strXml = "";
            byte[] timestamp = null;
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = commentcontrol.GetRecord(app,
                sessioninfo,
                e.Path,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            e.Xml = strXml;
            e.Timestamp = timestamp;

            XmlDocument dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(e.Xml) == false)
                    dom.LoadXml(e.Xml);
                else
                    dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(e.Index));
            if (line == null)
            {
                PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
                line = NewLine(e.Index, cmdline);
                // this.LineCount++;
            }
            line.Visible = true;

            LiteralControl left = (LiteralControl)line.FindControl("line" + Convert.ToString(e.Index) + "left");
            CheckBox checkbox = (CheckBox)line.FindControl("line" + Convert.ToString(e.Index) + "checkbox");
            LiteralControl no = (LiteralControl)this.FindControl("line" + Convert.ToString(e.Index) + "_no");
            LiteralControl middle = (LiteralControl)line.FindControl("line" + Convert.ToString(e.Index) + "middle");
            LiteralControl right = (LiteralControl)line.FindControl("line" + Convert.ToString(e.Index) + "right");
            // PlaceHolder editor = (PlaceHolder)this.FindControl("line_editarea_" + Convert.ToString(e.Index));

            checkbox.Visible = false;   // 暂时不用，因为没有cmdline

            string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement,
                "creator");

            string strResult = "";

            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");

            string strClass = "content";
            if (strRefID == this.RefID
                && String.IsNullOrEmpty(strRefID) == false)
            {
                strClass = "content active";
            }

            string strAnchor = "";
            if (e.Path == this.FocusRecPath)
            {
                strClass = "content active";
                strAnchor = "<a name='active'></a>";
            }

            strResult += "<tr class='" + strClass + "' >" + "<td class='no'>" + strAnchor;

            // 左
            left.Text = strResult;

            // checkbox.Text = Convert.ToString(e.TotalCount - (e.Index + this.StartIndex));

            string strNo = Convert.ToString(e.TotalCount - (e.Index + this.StartIndex));
            no.Text = "<div>" + strNo + "</div>";

            // 右开始
            strResult = "</td>";
            strResult += "<td class='content'>";
            middle.Text = strResult;

            commentcontrol.RecPath = e.Path;

            //

            /*
            strResult = "</div>";
            strResult += "</td>";
            strResult += "</tr>";

            right.Text = strResult;
             * */

            return;
        ERROR1:
            this.Page.Response.Write(strError);
        }


        // [配置参数] 是否要显示空的表格。如果不显示，就是完全隐藏
        bool DisplayBlankTable
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)
                {
                    return true;
                }
                XmlNode nodeItem = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "commentsControl/properties");

                if (nodeItem == null)
                    return true;

                string strError = "";
                bool bValue;
                DomUtil.GetBooleanParam(nodeItem,
                    "displayBlankTable",
                    true,
                    out bValue,
                    out strError);
                return bValue;
            }
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

    }



    /// <summary>
    /// 通知状态变化
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void WantFocusEventHandler(object sender,
    WantFocusEventArgs e);

    /// <summary>
    /// 通知状态变化事件的参数
    /// </summary>
    public class WantFocusEventArgs : EventArgs
    {
        public bool Focus = true;  // true 表示sender获得和希望独占Focus; false 表示sender不独占focus
    }

    public enum Text2HtmlStyle
    {
        BR = 0,
        P = 1,
    }
}
