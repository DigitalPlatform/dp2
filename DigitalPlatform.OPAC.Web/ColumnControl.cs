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

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient;


using DigitalPlatform.CirculationClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:ColumnControl runat=server></{0}:ColumnControl>")]
    public class ColumnControl : WebControl, INamingContainer
    {
        // bool m_bButtonSetted = false;

        public ColumnStorage CommentColumn = null;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.ColumnControl.cs",
                typeof(ColumnControl).Module.Assembly);

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

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

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
                if (this.CommentColumn != null)
                    return (int)this.CommentColumn.Count;
                return 0;
            }

        }

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



        protected override void CreateChildControls()
        {
            CreatePrifix(String.IsNullOrEmpty(this.Title) == true ? this.GetString("最新书评") : this.Title,
                "content_wrapper");
            this.Controls.Add(new AutoIndentLiteral("<%begin%><table class='column'>"));

            // 信息行
            this.Controls.Add(new AutoIndentLiteral(
                "<%begin%><tr class='info'><td colspan='4'>"
            ));

            // 信息文字
            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            this.Controls.Add(resultinfo);

            PageSwitcherControl pager = new PageSwitcherControl();
            pager.ID = "pager_top";
            pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            this.Controls.Add(pager);

            this.Controls.Add(new AutoIndentLiteral(
                "</td><%end%></tr>"
            ));

            // 标题行
            this.Controls.Add(new AutoIndentLiteral(
                "<%begin%><tr class='columntitle'><td class='no' nowrap>"
                + this.GetString("序号")
                + "</td><td class='content' colspan='2'>"
                + this.GetString("内容")
                + "</td><%end%></tr>"
            ));

            // 内容代表
            PlaceHolder content = new PlaceHolder();
            content.ID = "content";
            this.Controls.Add(content);

            // 内容行
            for (int i = 0;
 i < this.PageMaxLines;
 i++)
            {
                PlaceHolder line = NewContentLine(content, i, null);
            }


            // 插入点
            PlaceHolder insertpoint = new PlaceHolder();
            insertpoint.ID = "insertpoint";
            content.Controls.Add(insertpoint);

            // 命令行
            PlaceHolder cmdline_holder = new PlaceHolder();
            cmdline_holder.ID = "cmdline_holder";
            this.Controls.Add(cmdline_holder);

            CreateCmdLine(cmdline_holder);

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            debugline.Visible = false;
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);

            this.Controls.Add(new AutoIndentLiteral(
                // "</table></div>"
               "<%end%></table>" + this.GetPostfixString()
               ));
        }

        void pager_PageSwitch(object sender, PageSwitchEventArgs e)
        {
            this.StartIndex = this.PageMaxLines * e.GotoPageNo;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }

            this.ClearAllChecked();
        }

        void lastpage_Click(object sender, EventArgs e)
        {
            int delta = this.ResultCount % this.PageMaxLines;
            if (delta > 0)
                this.StartIndex = (this.ResultCount / this.PageMaxLines) * this.PageMaxLines;
            else
                this.StartIndex = Math.Max(0, (this.ResultCount / this.PageMaxLines) * this.PageMaxLines - 1);

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
            AutoIndentLiteral auto_literal = new AutoIndentLiteral();
            auto_literal.Text = "<%begin%><tr class='content'><%begin%><td class='no";  // contentup
            line.Controls.Add(auto_literal);

            LiteralControl line_class = new LiteralControl();
            line_class.ID = "line" + Convert.ToString(nLineNo) + "_class";
            line.Controls.Add(line_class);

            line.Controls.Add(new LiteralControl("' >"));   // rowspan='2'

            // 序号
            LiteralControl literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_no";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "line" + Convert.ToString(nLineNo) + "_checkbox";
            checkbox.CssClass = "comment_checkbox";
            checkbox.Attributes.Add("onclick", "onColumnCheckboxClick(this);");
            line.Controls.Add(checkbox);

            auto_literal = new AutoIndentLiteral();
            auto_literal.Text = "<%end%></td>"; // "<%begin%><td class='path'>";
            line.Controls.Add(auto_literal);

            auto_literal = new AutoIndentLiteral();
            auto_literal.Text = "<%begin%><td class='review'>";
            line.Controls.Add(auto_literal);

            // 一个评注
            CommentControl comment = new CommentControl();
            comment.ID = "line" + Convert.ToString(nLineNo) + "_comment";
            comment.WantFocus -= new WantFocusEventHandler(commentcontrol_WantFocus);
            comment.WantFocus += new WantFocusEventHandler(commentcontrol_WantFocus);
            line.Controls.Add(comment);

            auto_literal = new AutoIndentLiteral();
            auto_literal.Text = "<%end%></td><%begin%><td class='biblio'>";
            line.Controls.Add(auto_literal);

            // 书目摘要
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_bibliosummary";
            line.Controls.Add(literal);

            // 创建新评注
            Button newreview = new Button();
            newreview.Text = this.GetString("新评注");
            newreview.ID = "line" + Convert.ToString(nLineNo) + "_newreview";
            newreview.CssClass = "newreview";
            newreview.ToolTip = this.GetString("为按钮下方的书目记录创建一条新评注");
            newreview.Visible = false;
            line.Controls.Add(newreview);

            PlaceHolder biblioinfo_holder = new PlaceHolder();
            biblioinfo_holder.ID = "line" + Convert.ToString(nLineNo) + "_biblioinfo_holder";
            line.Controls.Add(biblioinfo_holder);

            BiblioControl bibliocontrol = new BiblioControl();
            bibliocontrol.ID = "line_" + nLineNo.ToString() + "_bibliocontrol";
            bibliocontrol.WantFocus -= new WantFocusEventHandler(bibliocontrol_WantFocus);
            bibliocontrol.WantFocus += new WantFocusEventHandler(bibliocontrol_WantFocus);
            biblioinfo_holder.Controls.Add(bibliocontrol);


            auto_literal = new AutoIndentLiteral();
            auto_literal.Text = "<%end%></td><%end%></tr>";
            line.Controls.Add(auto_literal);

            return line;
        }

        void bibliocontrol_WantFocus(object sender, WantFocusEventArgs e)
        {
        }

        void commentcontrol_WantFocus(object sender, WantFocusEventArgs e)
        {
            // 最后有函数统一设置了，这里就不用作了
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

            // 找到所有BiblioControl对象
            controls = BrowseSearchResultControl.FindControl(this,
    typeof(BiblioControl));

            // 把它们设置为Active = false的状态
            foreach (Control control in controls)
            {
                BiblioControl biblio_control = (BiblioControl)control;
                biblio_control.Active = false;
            }

            // 隐藏底部的cmdline中的按钮
            EnableCmdButtons(false);
            m_bButtonSetted = true;
             * */

            /*
            // 把底部的editor隐藏
            CommentControl editor = (CommentControl)this.FindControl("editor");
            editor.Visible = false;

            // 继续发给父对象
            if (this.SetActive != null)
            {
                SetActiveEventArgs e1 = new SetActiveEventArgs();
                this.SetActive(this, e1);
            }
             * */
        }

        void EnableCmdButtons(bool bEnable)
        {
            Button button = (Button)this.FindControl("selectall_button");
            button.Enabled = bEnable;

            button = (Button)this.FindControl("unselectall_button");
            button.Enabled = bEnable;

            button = (Button)this.FindControl("delete_button");
            button.Enabled = bEnable;

            button = (Button)this.FindControl("open_modify_state_button");
            button.Enabled = bEnable;
        }

        void CreateCmdLine(PlaceHolder line)
        {

            line.Controls.Add(new AutoIndentLiteral(
                "<%begin%><tr class='cmdline'  onmouseover='HilightColumnCmdline(this); return false;'><td colspan='3'>"
            ));

            // begin of whole line
            line.Controls.Add(new AutoIndentLiteral("<%begin%><div class='cmdline'>"));

            /*
            this.Controls.Add(new LiteralControl(
                "<table border='0' width='100%'><tr><td>"
            ));


            this.Controls.Add(new LiteralControl(
                "</td><td align='right'> "
            ));
             * */
            PlaceHolder buttons_holder = new PlaceHolder();
            buttons_holder.ID = "buttons_holder";
            line.Controls.Add(buttons_holder);

            // begin of whole buttons
            buttons_holder.Controls.Add(new AutoIndentLiteral("<%begin%><div class='buttons'>"));

            // 全选 按钮
            Button selectall_button = new Button();
            selectall_button.ID = "selectall_button";
            selectall_button.CssClass = "selectall";
            selectall_button.Text = this.GetString("全选");
            selectall_button.Click += new EventHandler(selectall_button_Click);
            buttons_holder.Controls.Add(selectall_button);

            // 全清除 按钮
            Button unselectall_button = new Button();
            unselectall_button.ID = "unselectall_button";
            unselectall_button.CssClass = "unselectall";
            unselectall_button.Text = this.GetString("全清除");
            unselectall_button.Click += new EventHandler(unselectall_button_Click);
            buttons_holder.Controls.Add(unselectall_button);

            // 删除 按钮
            string strConfirmText = this.GetString("确实要删除所选定的评注");
            Button delete_button = new Button();
            delete_button.ID = "delete_button";
            delete_button.CssClass = "delete";
            delete_button.Text = this.GetString("删除");
            delete_button.Click += new EventHandler(delete_button_Click);
            delete_button.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");
            buttons_holder.Controls.Add(delete_button);

            // 打开 修改状态对话框 按钮
            Button open_modify_state_dialog_button = new Button();
            open_modify_state_dialog_button.ID = "open_modify_state_button";
            open_modify_state_dialog_button.CssClass = "openmodifystate";
            open_modify_state_dialog_button.OnClientClick = "$( '#modify-state-dialog-form' ).dialog({ modal: true }); return cancelClick();";
            open_modify_state_dialog_button.Text = this.GetString("修改状态") + " ...";
            // open_modify_state_dialog_button.Attributes.Add("onclick", "$( \"#modify-state-dialog-form\" ).dialog({ modal: true });");
            buttons_holder.Controls.Add(open_modify_state_dialog_button);

            /* return false; 依然不能阻止ASP.NET post back的问题：
http://forums.asp.net/p/1595733/4046908.aspx
IE has a long standing bug (back to 5.5) where sometimes "return false" is not honored.  the most annoying feature of this bug, is it appears in some IE installations and not others. you are the lucky winner (save your install as its great for testing).  anyway the fix is easy. add this routine:

function cancelClick()
{
  if (window.event)
      window.event.cancelBubble = true;
  return false;
}
  // yet another IE hack
  function cancelClick() {
     if (window.event) window.event.cancelBubble = true;
     return false;
  }

then anytime you want to cancel an event just:

   return cancelClick();

bruce (sqlwork.com)
             * 
             * */
            /*
            string strButton = "<button class='" + "openmodifystate" + "' onclick=\"$( '#modify-state-dialog-form' ).dialog({ modal: true }); return cancelClick();\">" + this.GetString("修改状态") + " ..." + "</button>";
            LiteralControl open_modify_state_button = new LiteralControl();
            open_modify_state_button.ID = "open_modify_state_button";
            open_modify_state_button.Text = strButton;
            buttons_holder.Controls.Add(open_modify_state_button);
             * */

            // 修改状态 对话框
            buttons_holder.Controls.Add(new AutoIndentLiteral("<%begin%><div id='modify-state-dialog-form' style='DISPLAY:NONE'>"));

            buttons_holder.Controls.Add(new AutoIndentLiteral("<%begin%>" + this.GetString("加") + ": "));

            {
                // 屏蔽
                CheckBox screened = new CheckBox();
                screened.ID = "add_screened";
                screened.Text = this.GetString("屏蔽");
                screened.CssClass = "screened";
                buttons_holder.Controls.Add(screened);

                // 审查
                CheckBox edit_censor = new CheckBox();
                edit_censor.ID = "add_censor";
                edit_censor.Text = this.GetString("审查");
                edit_censor.CssClass = "censor";
                buttons_holder.Controls.Add(edit_censor);

                // 锁定
                CheckBox locked = new CheckBox();
                locked.ID = "add_locked";
                locked.Text = this.GetString("锁定");
                locked.CssClass = "locked";
                buttons_holder.Controls.Add(locked);

                // 精品
                CheckBox valuable = new CheckBox();
                valuable.ID = "add_valuable";
                valuable.Text = this.GetString("精品");
                valuable.CssClass = "valuable";
                buttons_holder.Controls.Add(valuable);
            }

            buttons_holder.Controls.Add(new AutoIndentLiteral("<br/><%end%>"));
            buttons_holder.Controls.Add(new AutoIndentLiteral("<%begin%>" + this.GetString("减") + ": "));
            {
                // 屏蔽
                CheckBox screened = new CheckBox();
                screened.ID = "remove_screened";
                screened.Text = this.GetString("屏蔽");
                screened.CssClass = "screened";
                buttons_holder.Controls.Add(screened);

                // 审查
                CheckBox edit_censor = new CheckBox();
                edit_censor.ID = "remove_censor";
                edit_censor.Text = this.GetString("审查");
                edit_censor.CssClass = "censor";
                buttons_holder.Controls.Add(edit_censor);

                // 锁定
                CheckBox locked = new CheckBox();
                locked.ID = "remove_locked";
                locked.Text = this.GetString("锁定");
                locked.CssClass = "locked";
                buttons_holder.Controls.Add(locked);

                // 精品
                CheckBox valuable = new CheckBox();
                valuable.ID = "remove_valuable";
                valuable.Text = this.GetString("精品");
                valuable.CssClass = "valuable";
                buttons_holder.Controls.Add(valuable);
            }
            buttons_holder.Controls.Add(new AutoIndentLiteral("<br/><%end%>"));

            // 修改状态 按钮
            Button modify_state_button = new Button();
            modify_state_button.OnClientClick = "$( \"#modify-state-dialog-form\" ).parent().appendTo($(\"form:first\"));";
            modify_state_button.ID = "modify_state_button";
            modify_state_button.Text = this.GetString("修改状态");
            modify_state_button.Click += new EventHandler(modify_state_button_Click);
            buttons_holder.Controls.Add(modify_state_button);

            buttons_holder.Controls.Add(new AutoIndentLiteral("<%end%></div>"));

            // end of whole buttons
            buttons_holder.Controls.Add(new AutoIndentLiteral("<%end%></div>"));

            PageSwitcherControl pager = new PageSwitcherControl();
            pager.ID = "pager_bottom";
            pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            line.Controls.Add(pager);

            /*
            this.Controls.Add(new LiteralControl(
                "</td></tr></table>"
            ));
             * */
            // end of whole line
            line.Controls.Add(new AutoIndentLiteral("<%end%></div>"));


            line.Controls.Add(new AutoIndentLiteral(
                "</td><%end%></tr>"
            ));
        }

        void selectall_button_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        void unselectall_button_Click(object sender, EventArgs e)
        {
            SelectAll(false);
        }


        bool GetCheckBoxState(string strID)
        {
            CheckBox checkbox = (CheckBox)this.FindControl(strID);
            return checkbox.Checked;
        }

        string GetAddList()
        {
            string strState = "";

            bool bAddScreened = GetCheckBoxState("add_screened");
            if (bAddScreened == true)
                StringUtil.SetInList(ref strState, "屏蔽", true);

            bool bAddCensor = GetCheckBoxState("add_censor");
            if (bAddCensor == true)
                StringUtil.SetInList(ref strState, "审查", true);

            bool bAddLocked = GetCheckBoxState("add_locked");
            if (bAddLocked == true)
                StringUtil.SetInList(ref strState, "锁定", true);

            bool bAddValueable = GetCheckBoxState("add_valuable");
            if (bAddValueable == true)
                StringUtil.SetInList(ref strState, "精品", true);

            return strState;
        }

        string GetRemoveList()
        {
            string strState = "";

            bool bRemoveScreened = GetCheckBoxState("remove_screened");
            if (bRemoveScreened == true)
                StringUtil.SetInList(ref strState, "屏蔽", true);

            bool bRemoveCensor = GetCheckBoxState("remove_censor");
            if (bRemoveCensor == true)
                StringUtil.SetInList(ref strState, "审查", true);

            bool bRemoveLocked = GetCheckBoxState("remove_locked");
            if (bRemoveLocked == true)
                StringUtil.SetInList(ref strState, "锁定", true);

            bool bRemoveValueable = GetCheckBoxState("remove_valuable");
            if (bRemoveValueable == true)
                StringUtil.SetInList(ref strState, "精品", true);

            return strState;
        }

        void ClearAllChecked()
        {
            for (int i = 0; i < this.PageMaxLines; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                checkbox.Checked = false;
            }
        }

        void SelectAll(bool bSelect = true)
        {
            for (int i = 0; i < this.PageMaxLines; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                if (checkbox.Checked != bSelect)
                {
                    checkbox.Checked = bSelect;
                }

                LiteralControl line_class = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_class");
                if (bSelect == true)
                    line_class.Text = " selected";
                else
                    line_class.Text = "";
            }
        }

        void SetLineClassAndControlActive()
        {
            List<int> editmode_commentcontrol_lineindexes = new List<int>();
            List<int> editmode_bibliocontrol_lineindexes = new List<int>();
            for (int i = 0; i < this.PageMaxLines; i++)
            {
                // 为每个checkbox设置正确的class
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                LiteralControl line_class = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_class");
                if (checkbox.Checked == true)
                    line_class.Text = " selected";
                else
                    line_class.Text = "";

                // 检查是否出现编辑状态的CommentControl
                CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                if (String.IsNullOrEmpty(commentcontrol.EditAction) == false)
                    editmode_commentcontrol_lineindexes.Add(i);

                // 检查是否出现编辑状态的BiblioControl
                BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + i.ToString() + "_bibliocontrol");
                if (String.IsNullOrEmpty(bibliocontrol.EditAction) == false)
                    editmode_bibliocontrol_lineindexes.Add(i);
            }

            if (editmode_commentcontrol_lineindexes.Count
                + editmode_bibliocontrol_lineindexes.Count > 0)
            {
                // 为每行的CommentControl和BiblioControl控件设置Active
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                    if (editmode_commentcontrol_lineindexes.IndexOf(i) == -1)
                    {
                        // 普通行
                        commentcontrol.Active = false;
                    }
                    else
                    {
                        // 编辑状态的行
                        commentcontrol.Active = true;
                    }

                    BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + Convert.ToString(i) + "_bibliocontrol");
                    if (editmode_bibliocontrol_lineindexes.IndexOf(i) == -1)
                    {
                        // 普通行
                        bibliocontrol.Active = false;
                    }
                    else
                    {
                        // 编辑状态的行
                        bibliocontrol.Active = true;
                    }

                    /*
                    BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + i.ToString() + "_bibliocontrol");
                    bibliocontrol.Active = false;
                     * */
                }

                // 禁止整个底部的命令按钮
                this.EnableCmdButtons(false);
            }
            else
            {
                // 为每行的CommentControl和BiblioControl控件设置Active=true
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                    commentcontrol.Active = true;

                    BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + Convert.ToString(i) + "_bibliocontrol");
                    bibliocontrol.Active = true;
                }

                this.EnableCmdButtons(true);
            }
        }

        // 按下修改状态按钮。修改所选评注记录的状态
        void modify_state_button_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nCount = 0; // 做了多少个事项
            int nChangedCount = 0;  // 真正发生了修改的事项个数

            string strAddList = GetAddList();
            string strRemoveList = GetRemoveList();

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                if (checkbox.Checked == false)
                    continue;

                CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                if (String.IsNullOrEmpty(commentcontrol.CommentRecPath) == true)
                {
                    continue;
                }

                // 修改评注的状态
                // return:
                //       -1  出错
                //      0   没有发生修改
                //      1   发生了修改
                int nRet = commentcontrol.ChangeState(
                    strAddList,
                    strRemoveList,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    nChangedCount++;

                nCount++;
            }

            if (nCount == 0)
            {
                strError = "尚未选择要修改状态的事项...";
                goto ERROR1;
            }

            ClearAllChecked();
            this.SetDebugInfo("共修改了 " + nChangedCount.ToString() + " 个评注记录的状态");
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }


        // 按下删除按钮。删除所选择的若干个评注记录
        // TODO: 当最后一页全部删空以后，显示是不是会有问题?
        void delete_button_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nCount = 0; // 做了多少个事项

            for (int i = 0; i < this.PageMaxLines; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                Debug.Assert(checkbox != null, "");

                if (checkbox.Checked == false)
                    continue;

                CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                if (String.IsNullOrEmpty(commentcontrol.CommentRecPath) == true)
                {
                    continue;
                }

                // 删除一个评注记录
                int nRet = commentcontrol.Delete(
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nCount++;
            }

            if (nCount == 0)
            {
                strError = "尚未选择要删除的事项...";
                goto ERROR1;
            }

            ClearAllChecked();
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new AutoIndentLiteral("<%begin%><tr class='debugline'><td colspan='3'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new AutoIndentLiteral("</td><%end%></tr>"));
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
                    string.Format(this.GetString("hit_count_summary"),   // "书评 {0} 条, 分 {1} 页显示, 当前为第 {2} 页。"
                    this.ResultCount.ToString(),
                    this.PageCount.ToString(),
                    (nPageNo + 1).ToString())
                    + "</div>";
            }
            else
                resultinfo.Text = "<div class='info'>" + this.GetString("empty_resultset") + "</div>";   // "(结果集为空)"


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

        protected override void Render(HtmlTextWriter writer)
        {
            int nRet = 0;
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            bool bManager = false;
            if (String.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            bool bReader = false;
            if (sessioninfo.ReaderInfo != null
                && sessioninfo.IsReader == true && loginstate != LoginState.Public)
            {
                bReader = true;
            }

            if (bManager == false)
            {
                Button delete_button = (Button)this.FindControl("delete_button");
                delete_button.Visible = false;

                Button open_modify_state_button = (Button)this.FindControl("open_modify_state_button");
                open_modify_state_button.Visible = false;

                Button selectall_button = (Button)this.FindControl("selectall_button");
                selectall_button.Visible = false;

                Button unselectall_button = (Button)this.FindControl("unselectall_button");
                unselectall_button.Visible = false;
            }

            /*
            if (sessioninfo.Account == null)
            {
                // 临时的SessionInfo对象
                SessionInfo temp_sessioninfo = new SessionInfo(app);

                // 模拟一个账户
                Account account = new Account();
                account.LoginName = "opac_column";
                account.Password = "";
                account.Rights = "getbibliosummary";

                account.Type = "";
                account.Barcode = "";
                account.Name = "opac_column";
                account.UserID = "opac_column";
                account.RmsUserName = app.ManagerUserName;
                account.RmsPassword = app.ManagerPassword;

                temp_sessioninfo.Account = account;
                sessioninfo = temp_sessioninfo;
            }
             * */

            bool bUseBiblioSummary = false;    // 使用书目摘要(否则就是详细书目格式)
            bool bDitto = true; // 书目 同上...
            XmlNode nodeBookReview = app.WebUiDom.DocumentElement.SelectSingleNode("bookReview");
            if (nodeBookReview != null)
            {
                DomUtil.GetBooleanParam(nodeBookReview,
                    "ditto",
                    true,
                    out bDitto,
                    out strError);
                DomUtil.GetBooleanParam(nodeBookReview,
    "useBiblioSummary",
    false,
    out bUseBiblioSummary,
    out strError);
            }

            int nPageNo = this.StartIndex / this.PageMaxLines;

            SetTitle(String.IsNullOrEmpty(this.Title) == true ? this.GetString("栏目") : this.Title);

            SetResultInfo();

            if (this.CommentColumn == null
                || this.CommentColumn.Opened == false)
            {
                this.SetDebugInfo("errorinfo", "尚未创建栏目缓存...");
            }

            /*
            List<string> recpathlist = new List<string>();
            this.RecPathList = "";
             * */

            if (this.CommentColumn != null)
            {
                app.m_lockCommentColumn.AcquireReaderLock(app.m_nCommentColumnLockTimeout);
                try
                {
                    string strPrevBiblioRecPath = "";

                    for (int i = 0; i < this.PageMaxLines; i++)
                    {
                        PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                        if (line == null)
                        {
                            PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                            PlaceHolder content = (PlaceHolder)this.FindControl("content");

                            line = this.NewContentLine(content, i, insertpoint);
                        }

                        LiteralControl no = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_no");
                        HyperLink pathcontrol = (HyperLink)this.FindControl("line" + Convert.ToString(i) + "_path");
                        // LiteralControl contentcontrol = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_content");
                        CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");

                        LiteralControl bibliosummarycontrol = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_bibliosummary");
                        HyperLink bibliorecpathcontrol = (HyperLink)this.FindControl("line" + Convert.ToString(i) + "_bibliorecpath");
                        Button newreview = (Button)this.FindControl("line" + Convert.ToString(i) + "_newreview");
                        PlaceHolder biblioinfo_holder = (PlaceHolder)this.FindControl("line" + Convert.ToString(i) + "_biblioinfo_holder");
                        BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + i.ToString() + "_bibliocontrol");

                        CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                        if (bManager == false)
                            checkbox.Visible = false;

                        int index = this.StartIndex + i;
                        if (index >= this.CommentColumn.Count)
                        {
                            checkbox.Visible = false;
                            commentcontrol.Visible = false;
                            bibliocontrol.Visible = false;
                            continue;
                        }
                        TopArticleItem record = (TopArticleItem)this.CommentColumn[index];

                        // 序号
                        string strNo = "&nbsp;";
                        strNo = Convert.ToString(i + this.StartIndex + 1);

                        no.Text = "<div>" + strNo + "</div>";

                        // 路径
                        string strPath = record.Line.m_strRecPath;

                        // 2012/7/11
                        commentcontrol.RecPath = app.GetLangItemRecPath(
                            "comment",
                            this.Lang,
                            strPath);

                        byte[] timestamp = null;
                        string strXml = "";
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = commentcontrol.GetRecord(
                            app,
                            sessioninfo,
                            strPath,
                            out strXml,
                            out timestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                        }

                        string strBiblioRecPath = "";
                        if (string.IsNullOrEmpty(strXml) == false)
                        {
                            string strParentID = "";
                            nRet = CommentControl.GetParentID(strXml,
                                out strParentID,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            strBiblioRecPath = CommentControl.GetBiblioRecPath(
                                app,
                                strPath,
                                strParentID);
                        }
                        else
                            strBiblioRecPath = "";

                        //
                        if (bManager == true || bReader == true)
                        {
                            string strUrl = "./book.aspx?BiblioRecPath="
                                + HttpUtility.UrlEncode(strBiblioRecPath)
                                + "&CommentRecPath="
                                + HttpUtility.UrlEncode(strPath)
                                + "#newreview";
                            newreview.OnClientClick = "window.open('" + strUrl + "','_blank'); return cancelClick();";
                            // newreview.ToolTip = this.GetString("创建新的评注, 属于书目记录") + ":" + strBiblioRecPath;
                            // newreview.Attributes.Add("target", "_blank");
                            newreview.Visible = true;
                        }
                        else
                            newreview.Visible = false;

                        if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        {
                            biblioinfo_holder.Controls.Add(new LiteralControl("<div class='ditto'>" + this.GetString("无法定位书目记录") + "</div>"));
                            bibliocontrol.Visible = false;
                        }
                        else if (bDitto == true
                            && strBiblioRecPath == strPrevBiblioRecPath)
                        {
                            biblioinfo_holder.Controls.Add(new LiteralControl("<div class='ditto'>" + this.GetString("同上") + "</div>"));
                            bibliocontrol.Visible = false;
                        }
                        else
                        {
                            if (bUseBiblioSummary == true)
                            {
                                // 获得摘要
                                string strBarcode = "@bibliorecpath:" + strBiblioRecPath;
                                string strSummary = "";
                                string strOutputBiblioRecPath = "";
                                long lRet = sessioninfo.Channel.GetBiblioSummary(
                                    null,
                                    strBarcode,
                                    null,
                                    null,
                                    out strOutputBiblioRecPath,
                                    out strSummary,
                                    out strError);
                                if (lRet == -1 || lRet == 0)
                                    strSummary = strError;

                                /*
                                LibraryServerResult result = app.GetBiblioSummary(
                                    sessioninfo,
                                    strBarcode,
                                    null,
                                    null,
                                    out strOutputBiblioRecPath,
                                    out strSummary);
                                if (result.Value == -1 || result.Value == 0)
                                    strSummary = result.ErrorInfo;
                                 * */

                                bibliosummarycontrol.Text = strSummary;
                                bibliocontrol.Visible = false;
                            }
                            else
                            {
                                bibliocontrol.RecPath = strBiblioRecPath;
                                bibliocontrol.Visible = true;
                            }
                        }

                        strPrevBiblioRecPath = strBiblioRecPath;
                    }
                }
                finally
                {
                    app.m_lockCommentColumn.ReleaseReaderLock();
                }
            }
            else
            {
                // 显示空行
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                        continue;

                    CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                    checkbox.Visible = false;

                    CommentControl commentcontrol = (CommentControl)this.FindControl("line" + Convert.ToString(i) + "_comment");
                    commentcontrol.Visible = false;

                    BiblioControl bibliocontrol = (BiblioControl)this.FindControl("line_" + i.ToString() + "_bibliocontrol");
                    bibliocontrol.Visible = false;

                }
            }

            // this.RecPathList = StringUtil.MakePathList(recpathlist);

            this.SetLineClassAndControlActive();

            /*
            if (this.m_bButtonSetted == false)
                this.EnableCmdButtons(true);
             * */

            base.Render(writer);
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
            base.Render(writer);
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

        public string GetPostfixString()
        {
            return "</div>";
        }
    }
}

