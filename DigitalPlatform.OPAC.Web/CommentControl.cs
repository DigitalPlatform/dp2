using System;
using System.Collections.Generic;
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
using DigitalPlatform.Text;
using DigitalPlatform.IO;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 用于处理一个评注记录的显示、编辑的控件
    /// </summary>
    [ToolboxData("<{0}:CommentControl runat=server></{0}:CommentControl>")]
    public class CommentControl : WebControl, INamingContainer
    {

        // public bool Active = true;
        public event WantFocusEventHandler WantFocus = null;

        public event SumitedEventHandler Submited = null;

        public string RecPath = ""; // 评注记录路径

        public bool Wrapper = false;    // 是否创建外围包裹的<div>

        ResourceManager m_rm = null;

        public override void Dispose()
        {
            this.WantFocus = null;
            this.Submited = null;

            base.Dispose();
        }

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.CommentControl.cs",
                typeof(CommentControl).Module.Assembly);

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
            catch (Exception)
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

        // "true"表示是；"false"或者空表示否
        public string Minimized
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("minimized");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("minimized");
                s.Value = value;
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
            this.Controls.Add(literal);

            string strClass = "review";
            if (this.Wrapper == true)
                strClass += " wrapper";

            Button minimize_button = new Button();
            minimize_button.ID = "minimize_button";
            minimize_button.Text = "显示 / 隐藏";
            minimize_button.CssClass = "minimize";
            minimize_button.Click += new EventHandler(minimize_button_Click);
            this.Controls.Add(minimize_button);

            HiddenField minimized = new HiddenField();
            minimized.ID = "minimized";
            this.Controls.Add(minimized);

            this.Controls.Add(new AutoIndentLiteral(
                "<%begin%><div class='" + strClass + "'"));

            LiteralControl frame_style = new LiteralControl();
            frame_style.ID = "frame_style";
            frame_style.Text = "";
            this.Controls.Add(frame_style);

            this.Controls.Add(new LiteralControl(">"));

            HiddenField editaction = new HiddenField();
            editaction.ID = "editaction";
            this.Controls.Add(editaction);

            // 信息行
            PlaceHolder infoline = new PlaceHolder();
            infoline.ID = "infoline";
            infoline.Visible = false;
            this.Controls.Add(infoline);

            CreateInfoLine(infoline);

            HiddenField biblio_recpath = new HiddenField();
            biblio_recpath.ID = "biblio_recpath";
            this.Controls.Add(biblio_recpath);

            // 正在编辑的评注记录路径
            HiddenField comment_recpath = new HiddenField();
            comment_recpath.ID = "comment_recpath";
            this.Controls.Add(comment_recpath);

            // TODO: 当控件为完全只读(不可编辑)时，这些东西都要省略，以节省资源
            HiddenField timestamp = new HiddenField();
            timestamp.ID = "edit_timestamp";
            timestamp.Value = "";
            this.Controls.Add(timestamp);

            HiddenField type = new HiddenField();
            type.ID = "edit_type";
            type.Value = "";
            this.Controls.Add(type);

            HiddenField active = new HiddenField();
            active.ID = "active";
            active.Value = "1";
            this.Controls.Add(active);

            /*
            // 用于重新登录的信息
            HiddenField relogin = new HiddenField();
            relogin.ID = "relogin";
            relogin.Value = "";
            this.Controls.Add(relogin);
             * */

            // 内容占位符
            PlaceHolder content = new PlaceHolder();
            content.ID = "content";
            this.Controls.Add(content);

            // 编辑行
            PlaceHolder inputline = new PlaceHolder();
            inputline.ID = "inputline";
            this.Controls.Add(inputline);

            CreateEditArea(inputline);


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

            this.Controls.Add(new AutoIndentLiteral(
    "<%end%></div>"));

            // 表格结尾
            literal = new LiteralControl();
            literal.ID = "end";
            if (this.Wrapper == true)
                literal.Text = this.GetPostfixString();
            this.Controls.Add(literal);
        }

        void minimize_button_Click(object sender, EventArgs e)
        {
            if (this.Minimized == "true")
                this.Minimized = "false";
            else
                this.Minimized = "true";

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                if (this.Minimized == "true")
                    e1.Focus = false;
                else
                    e1.Focus = true;

                this.WantFocus(this, e1);
            }
        }

        void CreateCmdLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<div class='cmdline' onmouseover='HilightCommentCmdline(this); return false;'>"));


            // change
            Button change_button = new Button();
            change_button.Text = this.GetString("编辑");
            change_button.ID = "changebutton";
            change_button.CssClass = "edit";
            change_button.Click += new EventHandler(change_button_Click);
            line.Controls.Add(change_button);

            // state
            Button state_button = new Button();
            state_button.Text = this.GetString("状态");
            state_button.ID = "statebutton";
            state_button.CssClass = "state";
            state_button.Click += new EventHandler(state_button_Click);
            line.Controls.Add(state_button);

            // delete
            Button delete_button = new Button();
            delete_button.Text = this.GetString("删除");
            delete_button.ID = "deletebutton";
            delete_button.CssClass = "delete";
            delete_button.Click += new EventHandler(delete_button_Click);
            line.Controls.Add(delete_button);

            string strConfirmText = this.GetString("确实要删除这条评注?");
            delete_button.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");



            line.Controls.Add(new LiteralControl("</div>"));
        }

        void state_button_Click(object sender, EventArgs e)
        {
            this.EditAction = "changestate";

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                this.WantFocus(this, e1);
            }
        }

        void delete_button_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (Delete(out strError) == -1)
                goto ERROR1;

            // this.Page.Response.Redirect(this.Page.Request.RawUrl, true);
            return;
            ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        public int Delete(out string strError)
        {
            strError = "";
            int nRet = 0;

            string strRecPath = this.CommentRecPath;
            string strTimestamp = this.Timestamp;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string strCommentRecPath = this.CommentRecPath;

                // 获得旧记录
                string strOldXml = "";
                byte[] timestamp = ByteArray.GetTimeStampByteArray(this.Timestamp);
                if (String.IsNullOrEmpty(strCommentRecPath) == false)
                {
                    string strOutputPath = "";
                    byte[] temp_timestamp = null;
                    string strBiblio = "";
                    string strTempBiblioRecPath = "";
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    //      >1  命中多于1条
                    long lRet = // sessioninfo.Channel.
                        channel.GetCommentInfo(
    null,
    "@path:" + strCommentRecPath,
    // null,
    "xml", // strResultType
    out strOldXml,
    out strOutputPath,
    out temp_timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strTempBiblioRecPath,
    out strError);
                    if (lRet == -1)
                    {
                        strError = "获得原有评注记录 '" + strCommentRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }
                    if (lRet == 0)
                    {
                        // 评注记录本来就不存在
                        goto END1;
                    }

                    if (ByteArray.Compare(temp_timestamp, timestamp) != 0)
                    {
                        strError = "删除被拒绝。因为记录 '" + strCommentRecPath + "' 在删除前已经被其他人修改过。请重新装载";
                        goto ERROR1;
                    }
                }

                XmlDocument dom = new XmlDocument();
                if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    try
                    {
                        dom.LoadXml(strOldXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载记录XML进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                }
                else
                    dom.LoadXml("<root/>");

                // 注：从dp2library角度，本来就限制了reader类型的帐户只能删除由自己创建的评注。
                // 而这里进一步限制了(即便是图书馆工作人员)只有managecomment权限的用户才能删除其他人创建的评注
                string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement,
        "creator");
                if (bManager == false
                    && strOriginCreator != sessioninfo.UserID)
                {
                    strError = "当前用户 '" + sessioninfo.UserID + "' 不能删除由另一用户 '" + strOriginCreator + "' 创建的评注记录";
                    goto ERROR1;
                }

                HiddenField biblio_recpath = (HiddenField)this.FindControl("biblio_recpath");

                // 删除一个评注记录
                nRet = DeleteCommentInfo(
                    this.Page,
                    channel,
                    this.BiblioRecPath,
                    strRecPath,
                    ByteArray.GetTimeStampByteArray(strTimestamp),
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            END1:
            // 修改评注记录后，更新栏目存储结构
            // parameters:
            //      strAction   动作。change/delete/new
            // return:
            //      -2   栏目缓存尚未创建,因此无从更新
            //		-1	error
            //		0	not found line object
            //		1	succeed
            nRet = app.UpdateLine(
                this.Page,
                "delete",
                strRecPath,
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
            ERROR1:
            return -1;
        }

        // 删除一个评注记录
        public static int DeleteCommentInfo(
            Page page,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strCommentRecPath,
            byte[] timestamp,
            out string strError)
        {
            strError = "";

            LoginState loginstate = GlobalUtil.GetLoginState(page);
            if (loginstate == LoginState.NotLogin)
            {
                strError = "尚未登录, 不能删除评注";
                return -1;
            }
            if (loginstate == LoginState.Public)
            {
                strError = "访客身份, 不能删除评注";
                return -1;
            }

            OpacApplication app = (OpacApplication)page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)page.Session["sessioninfo"];

            Debug.Assert(String.IsNullOrEmpty(sessioninfo.UserID) == false, "");

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            // string strTargetBiblioRecID = ResPath.GetRecordId(strBiblioRecPath);


            info.Action = "delete";
            info.NewRecPath = "";
            info.OldRecPath = strCommentRecPath;
            info.NewRecord = "";
            info.NewTimestamp = null;

            info.OldRecord = "";
            info.OldTimestamp = timestamp;

            // 
            EntityInfo[] comments = new EntityInfo[1];
            comments[0] = info;

            EntityInfo[] errorinfos = null;

            long lRet = // sessioninfo.Channel.
                channel.SetComments(
                null,
                strBiblioRecPath,
                comments,
                out errorinfos,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        //strNewCommentRecPath = error.NewRecPath;
                        //strNewXml = error.NewRecord;
                        //baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        void change_button_Click(object sender, EventArgs e)
        {
            this.EditAction = "change";

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                this.WantFocus(this, e1);
            }
        }

        void CreateInfoLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<div class='infoline'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "infotext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</div>"));
        }

        void SetInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("infoline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("infotext");
            text.Text = strText;
        }

        void CreateDebugLine(PlaceHolder line)
        {
            //line.Controls.Add(new AutoIndentLiteral("<%begin%><tr class='debugline'><td colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            //line.Controls.Add(new AutoIndentLiteral("</td><%end%></tr>"));
        }

        void SetCommentInfo(string strSpanClass,
string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("commentline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("comment");
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

        public string EditTitle
        {
            get
            {
                this.EnsureChildControls();

                TextBox edit_title = (TextBox)this.FindControl("edit_title");
                return edit_title.Text;
            }
            set
            {
                this.EnsureChildControls();

                TextBox edit_title = (TextBox)this.FindControl("edit_title");
                edit_title.Text = value;
            }
        }

        public string EditState
        {
            get
            {
                this.EnsureChildControls();

                HiddenField edit_state = (HiddenField)this.FindControl("edit_state");
                return edit_state.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField edit_state = (HiddenField)this.FindControl("edit_state");
                edit_state.Value = value;
            }
        }

        public string EditContent
        {
            get
            {
                this.EnsureChildControls();

                TextBox edit_content = (TextBox)this.FindControl("edit_content");
                return edit_content.Text;
            }
            set
            {
                this.EnsureChildControls();

                TextBox edit_content = (TextBox)this.FindControl("edit_content");
                edit_content.Text = value;
            }
        }

        public Button ButtonSubmit
        {
            get
            {
                this.EnsureChildControls();

                Button button = (Button)this.FindControl("submit_button");
                return button;
            }
        }

        public Button ButtonCancel
        {
            get
            {
                this.EnsureChildControls();

                Button button = (Button)this.FindControl("cancel_button");
                return button;
            }
        }

        public PlaceHolder OrderSuggestionHolder
        {
            get
            {
                this.EnsureChildControls();

                return (PlaceHolder)this.FindControl("ordercomment_description_holder");
            }
        }

        public LiteralControl EditDescription
        {
            get
            {
                this.EnsureChildControls();

                return (LiteralControl)this.FindControl("edit_description");
            }
        }

        void CreateEditArea(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<div class='inputline'>"));

            PlaceHolder edit_holder = new PlaceHolder();
            edit_holder.ID = "edit_holder";
            line.Controls.Add(edit_holder);

            edit_holder.Controls.Add(new LiteralControl("<table class='edit'>"));

            string strAnchor = "<a name='newreview'></a>";

            // 编辑区域中上部的容器。上部，不包含状态部分和下面的信息、按钮
            PlaceHolder edit_up_holder = new PlaceHolder();
            edit_up_holder.ID = "edit_up_holder";
            edit_holder.Controls.Add(edit_up_holder);


            // 注释行。提醒管理员修改别人的评注
            {
                PlaceHolder commentline = new PlaceHolder();
                commentline.ID = "commentline";
                commentline.Visible = false;
                edit_up_holder.Controls.Add(commentline);

                commentline.Controls.Add(new LiteralControl("<tr class='comment'><td class='comment' colspan='2'>"));

                LiteralControl comment = new LiteralControl();
                comment.ID = "comment";
                commentline.Controls.Add(comment);

                commentline.Controls.Add(new LiteralControl("</td></tr>"));
            }

            // 提示文字
            edit_up_holder.Controls.Add(new LiteralControl("<tr class='description suggestion'><td class='description suggestion' colspan='2'>" + strAnchor));

            LiteralControl description = new LiteralControl();
            description.ID = "edit_description";
            description.Text = "本书正在征求订购意见：";
            edit_up_holder.Controls.Add(description);

            edit_up_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 将前2行包括在一个Holder中，便于隐藏
            PlaceHolder ordercomment_description_holder = new PlaceHolder();
            ordercomment_description_holder.ID = "ordercomment_description_holder";
            edit_up_holder.Controls.Add(ordercomment_description_holder);

            // 是否建议订购
            ordercomment_description_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("是否订购");    //
            ordercomment_description_holder.Controls.Add(literal);

            ordercomment_description_holder.Controls.Add(new LiteralControl("</td><td>"));

            RadioButton edit_yes = new RadioButton();
            edit_yes.Text = this.GetString("订购本书");
            edit_yes.ID = "edit_yes";
            edit_yes.CssClass = "yes";
            edit_yes.Checked = true;
            edit_yes.GroupName = "yesno";
            ordercomment_description_holder.Controls.Add(edit_yes);

            ordercomment_description_holder.Controls.Add(new LiteralControl("<br/>"));

            RadioButton edit_no = new RadioButton();
            edit_no.Text = this.GetString("不要订购本书");
            edit_no.ID = "edit_no";
            edit_no.CssClass = "no";
            edit_no.GroupName = "yesno";
            edit_no.Checked = false;
            ordercomment_description_holder.Controls.Add(edit_no);

            ordercomment_description_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 对下部的提示
            ordercomment_description_holder.Controls.Add(new LiteralControl("<tr class='description detail'><td class='description detail' colspan='2'>"));

            LiteralControl detail_description = new LiteralControl();
            detail_description.ID = "edit_detail_description";
            detail_description.Text = this.GetString("还可详细阐述您的意见如下(可选)") + ":";
            ordercomment_description_holder.Controls.Add(detail_description);

            ordercomment_description_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 标题
            edit_up_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("标题");    //
            edit_up_holder.Controls.Add(literal);

            edit_up_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_title = new TextBox();
            edit_title.Text = "";
            edit_title.ID = "edit_title";
            edit_title.CssClass = "title";
            edit_up_holder.Controls.Add(edit_title);

            HiddenField edit_state = new HiddenField();
            edit_state.ID = "edit_state";
            edit_up_holder.Controls.Add(edit_state);

            edit_up_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 正文
            edit_up_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("正文");    //
            edit_up_holder.Controls.Add(literal);

            edit_up_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_content = new TextBox();
            edit_content.Text = "";
            edit_content.ID = "edit_content";
            edit_content.CssClass = "content";
            edit_content.TextMode = TextBoxMode.MultiLine;
            edit_up_holder.Controls.Add(edit_content);

            edit_up_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 上载图像
            edit_up_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("上传图像文件");    //
            edit_up_holder.Controls.Add(literal);

            edit_up_holder.Controls.Add(new LiteralControl("</td><td>"));

            FileUpload upload = new FileUpload();
            upload.ID = "upload";
            upload.CssClass = "content";
            edit_up_holder.Controls.Add(upload);

            edit_up_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 状态
            PlaceHolder state_holder = new PlaceHolder();
            state_holder.ID = "state_holder";
            state_holder.Visible = true;
            edit_holder.Controls.Add(state_holder);

            state_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("状态");    //
            state_holder.Controls.Add(literal);

            state_holder.Controls.Add(new LiteralControl("</td><td>"));

            // 屏蔽
            CheckBox screened = new CheckBox();
            screened.ID = "edit_screened";
            screened.Text = this.GetString("屏蔽");
            screened.CssClass = "screened";
            state_holder.Controls.Add(screened);

            // 审查
            CheckBox edit_censor = new CheckBox();
            edit_censor.ID = "edit_censor";
            edit_censor.Text = this.GetString("审查");
            edit_censor.CssClass = "censor";
            state_holder.Controls.Add(edit_censor);

            // 锁定
            CheckBox locked = new CheckBox();
            locked.ID = "edit_locked";
            locked.Text = this.GetString("锁定");
            locked.CssClass = "locked";
            state_holder.Controls.Add(locked);

            // 精品
            CheckBox valuable = new CheckBox();
            valuable.ID = "edit_valuable";
            valuable.Text = this.GetString("精品");
            valuable.CssClass = "valuable";
            state_holder.Controls.Add(valuable);

            state_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 信息
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("其他");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            LiteralControl info = new LiteralControl();
            info.ID = "recordinfo";
            edit_holder.Controls.Add(info);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 提交
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='submit' colspan='2'  onmouseover='HilightCommentCmdline(this); return false;'>"));

            Button submit_button = new Button();
            submit_button.ID = "submit_button";
            submit_button.CssClass = "submit";
            submit_button.Text = this.GetString("提交评注");
            submit_button.Click += new EventHandler(submit_button_Click);
            edit_holder.Controls.Add(submit_button);


            Button cancel_button = new Button();
            cancel_button.ID = "cancel_button";
            cancel_button.CssClass = "cancel";
            cancel_button.Text = this.GetString("取消");
            cancel_button.Click += new EventHandler(cancel_button_Click);
            string strConfirmText = this.GetString("确实要取消对这条评注的修改?");
            cancel_button.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");
            edit_holder.Controls.Add(cancel_button);

            LiteralControl edit_errorinfo = new LiteralControl();
            edit_errorinfo.ID = "edit_errorinfo";
            edit_errorinfo.Visible = false;
            edit_holder.Controls.Add(edit_errorinfo);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            edit_holder.Controls.Add(new LiteralControl("</table>"));

            line.Controls.Add(new LiteralControl("</div>"));
        }

        void GetStateValueFromControls(ref string strState)
        {
            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            StringUtil.SetInList(ref strState, "屏蔽", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_censor");
            StringUtil.SetInList(ref strState, "审查", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_locked");
            StringUtil.SetInList(ref strState, "锁定", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            StringUtil.SetInList(ref strState, "精品", checkbox.Checked);
        }

        // 设置或者刷新一个操作记载
        public static int SetOperation(
            OpacApplication app,
            ref XmlDocument dom,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "dom.DocumentElement == null";
                return -1;
            }

            XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
            if (nodeOperations == null)
            {
                nodeOperations = dom.CreateElement("operations");
                dom.DocumentElement.AppendChild(nodeOperations);
            }

            XmlNodeList nodes = nodeOperations.SelectNodes("operation[@name='" + strOperName + "']");
            if (bAppend == true)
            {
                // 删除多余9个的
                if (nodes.Count > 9)
                {
                    for (int i = 0; i < nodes.Count - 9; i++)
                    {
                        XmlNode node = nodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }
            else
            {
                if (nodes.Count > 1)
                {
                    for (int i = 0; i < nodes.Count - 1; i++)
                    {
                        XmlNode node = nodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            {
                XmlNode node = null;
                if (bAppend == true)
                {
                }
                else
                {
                    node = nodeOperations.SelectSingleNode("operation[@name='" + strOperName + "']");
                }


                if (node == null)
                {
                    node = dom.CreateElement("operation");
                    nodeOperations.AppendChild(node);
                    DomUtil.SetAttr(node, "name", strOperName);
                }


                string strTime = DateTimeUtil.Rfc1123DateTimeString(DateTime.UtcNow);// app.Clock.GetClock();

                DomUtil.SetAttr(node, "time", strTime);
                DomUtil.SetAttr(node, "operator", strOperator);
                if (String.IsNullOrEmpty(strComment) == false)
                    DomUtil.SetAttr(node, "comment", strComment);
            }

            return 0;
        }

        static void ModifyStateString(ref string strState,
            string strAddList,
            string strRemoveList)
        {
            string[] adds = strAddList.Split(new char[] { ',' });
            for (int i = 0; i < adds.Length; i++)
            {
                StringUtil.SetInList(ref strState, adds[i], true);
            }
            string[] removes = strRemoveList.Split(new char[] { ',' });
            for (int i = 0; i < removes.Length; i++)
            {
                StringUtil.SetInList(ref strState, removes[i], false);
            }
        }

        // 修改评注的状态
        // return:
        //       -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        public int ChangeState(
            string strAddList,
            string strRemoveList,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (string.IsNullOrEmpty(sessioninfo.UserID) == true)
            {
                strError = "尚未登录，不能进行修改记录状态的操作";
                goto ERROR1;
            }

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;


            if (bManager == false)
            {
                strError = "当前帐户不具备 managecomment 权限，不能进行修改记录状态的操作";
                goto ERROR1;
            }

            string strCommentRecPath = this.CommentRecPath;

            if (String.IsNullOrEmpty(strCommentRecPath) == true)
            {
                strError = "CommentRecPath为空，无法进行修改记录状态的操作";
                goto ERROR1;
            }

            string strBiblioRecPath = this.BiblioRecPath;
            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "strBiblioRecPath为空，无法进行修改记录状态的操作";
                goto ERROR1;
            }

            // 获得旧记录
            byte[] timestamp = ByteArray.GetTimeStampByteArray(this.Timestamp);
            string strOldXml = "";
            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {

                string strOutputPath = "";
                byte[] temp_timestamp = null;
                string strBiblio = "";
                string strTempBiblioRecPath = "";
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                //      >1  命中多于1条
                long lRet = // sessioninfo.Channel.
                    channel.GetCommentInfo(
    null,
    "@path:" + strCommentRecPath,
    // null,
    "xml", // strResultType
    out strOldXml,
    out strOutputPath,
    out temp_timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strTempBiblioRecPath,
    out strError);
                if (lRet == -1)
                {
                    strError = "获得原有评注记录 '" + strCommentRecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }
                if (lRet == 0)
                {
                    strError = "评注记录 '" + strCommentRecPath + "' 没有找到";
                    goto ERROR1;
                }

                if (ByteArray.Compare(temp_timestamp, timestamp) != 0)
                {
                    strError = "修改被拒绝。因为记录 '" + strCommentRecPath + "' 在保存前已经被其他人修改过。请重新装载";
                    goto ERROR1;
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(strOldXml) == false)
            {
                try
                {
                    dom.LoadXml(strOldXml);
                }
                catch (Exception ex)
                {
                    strError = "装载记录XML进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
                dom.LoadXml("<root/>");

            // 仅仅修改状态
            {
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");
                string strOldState = strState;

                ModifyStateString(ref strState,
    strAddList,
    strRemoveList);

                if (strState == strOldState)
                    return 0;

                DomUtil.SetElementText(dom.DocumentElement,
                    "state", strState);


                // 在<operations>中写入适当条目
                string strComment = "'" + strOldState + "' --> '" + strState + "'";
                nRet = SetOperation(
                    app,
                    ref dom,
                    "stateModified",
                    sessioninfo.UserID,
                    strComment,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string strNewCommentRecPath = "";
            string strNewXml = "";
            byte[] baNewTimestamp = null;

            {
                strNewCommentRecPath = strCommentRecPath;

                // 覆盖
                nRet = ChangeCommentInfo(
                    this.Page,
    strBiblioRecPath,
    strCommentRecPath,
    strOldXml,
    dom.DocumentElement.OuterXml,
    timestamp,
out strNewXml,
out baNewTimestamp,
out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.EditAction = "";
            }

            bool bUpdateColumnAfterModifyState = false;    // 修改状态时是否调整Column列表顺序
            XmlNode nodeBookReview = app.WebUiDom.DocumentElement.SelectSingleNode("bookReview");
            if (nodeBookReview != null)
            {
                DomUtil.GetBooleanParam(nodeBookReview,
                    "updateColumnAfterModifyState",
                    false,
                    out bUpdateColumnAfterModifyState,
                    out strError);
            }

            if (bUpdateColumnAfterModifyState == true)
            {
                // 修改评注记录后，更新栏目存储结构
                // parameters:
                //      strAction   动作。change/delete/new
                // return:
                //      -2   栏目缓存尚未创建,因此无从更新
                //		-1	error
                //		0	not found line object
                //		1	succeed
                nRet = app.UpdateLine(
                    this.Page,
                    "change",
                    strNewCommentRecPath,
                    strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return 1;
            ERROR1:
            return -1;
        }

        // TODO: 最好 redirect 到当前URL。但要注意跳转到接近的link
        void submit_button_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";

            int nRet = DoSubmit(
            out strWarning,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            string strAction = this.EditAction;
            if (String.IsNullOrEmpty(strAction) == true)
                strAction = "change";

            this.EditAction = "";

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                e1.Focus = false;   // 不再独占Focus
                this.WantFocus(this, e1);
            }

            if (this.Submited != null)
            {
                SubmitedEventArgs e1 = new SubmitedEventArgs();
                e1.Action = strAction;  // new / change
                this.Submited(this, e1);
            }

            if (String.IsNullOrEmpty(strWarning) == false)
                this.SetDebugInfo("warninginfo", strWarning);
            else
            {
                this.Page.Response.Redirect("./book.aspx?commentrecpath=" + HttpUtility.UrlEncode(this.RecPath) + "#active", true);
            }
            return;
            ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        public int DoSubmit(
            out string strWarning,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            strWarning = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

#if NO
            if (sessioninfo.Account == null)
            {
                // 有可能是服务器重新启动导致
                string strReLogin = this.ReLogin;

                // 立即重新登录
                if (String.IsNullOrEmpty(strReLogin) == false)
                {
                    Hashtable table = StringUtil.ParseParameters(strReLogin);
                    string strUserName = (string)table["username"];
                    if (String.IsNullOrEmpty(strUserName) == true)
                        goto SKIP1;
                    string strToken = (string)table["token"];
                    string strType = (string)table["type"];

                    if (strType != "reader")
                    {
                        // 工作人员登录
                        nRet = sessioninfo.Login(strUserName,
                             null,
                             "#web",
                             false,
                             out strError);
                        if (nRet != 1)
                        {
                            goto SKIP1;
                        }
                    }
                    else
                    {
                        // 读者身份登录
                        nRet = app.LoginForReader(sessioninfo,
                                    strUserName,
                                    null,   // 表示不进行密码判断
                                     "#web",
                                    -1,
                                    out strError);
                        if (nRet == -1)
                        {
                            strError = "对图书馆读者帐户进行登录时出错：" + strError;
                            goto SKIP1;
                        }
                        if (nRet > 1)
                        {
                            strError = "登录中发现有 " + nRet.ToString() + " 个账户符合条件，登录失败";
                            goto SKIP1;
                        }
                    }

                    string strSHA1 = Cryptography.GetSHA1(sessioninfo.UserID + "|" + sessioninfo.Account.Password);
                    if (strSHA1 != strToken)
                    {
                        // logout
                        this.Page.Session.Abandon();
                        goto SKIP1; // token不正确
                    }
                    Debug.Assert(sessioninfo.Account != null, "");
                    strWarning = "这是重新登录过的";
                    goto SKIP2;
                }

            SKIP1:
                strError = "尚未登录，不能进行修改记录的操作";
                goto ERROR1;
            }

            SKIP2:
#endif

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            string strCommentRecPath = this.CommentRecPath;

            HiddenField edit_type = (HiddenField)this.FindControl("edit_type");
            TextBox edit_title = (TextBox)this.FindControl("edit_title");
            TextBox edit_content = (TextBox)this.FindControl("edit_content");
            HiddenField biblio_recpath = (HiddenField)this.FindControl("biblio_recpath");
            RadioButton edit_yes = (RadioButton)this.FindControl("edit_yes");
            FileUpload upload = (FileUpload)this.FindControl("upload");

            // 先对输入内容进行检查
            if (String.IsNullOrEmpty(strCommentRecPath) == true)
            {
                if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
                {
                    strError = "BiblioRecPath 为空，无法创建评注记录";
                    goto ERROR1;
                }

                if (edit_type.Value == "书评")
                {
                    // 创建一个评注记录
                    if (String.IsNullOrEmpty(edit_title.Text) == true
                        && String.IsNullOrEmpty(edit_content.Text) == true)
                    {
                        strError = "尚未输入任何内容。没有创建新评注";
                        goto ERROR1;
                    }
                }

                // 创建 订购建议，title可以为空

            }
            else
            {

            }


            string strCreator = sessioninfo.UserID;

            // 获得旧记录
            string strOldXml = "";
            byte[] timestamp = ByteArray.GetTimeStampByteArray(this.Timestamp);
            if (String.IsNullOrEmpty(strCommentRecPath) == false)
            {
                LibraryChannel channel = sessioninfo.GetChannel(true);
                try
                {
                    string strOutputPath = "";
                    byte[] temp_timestamp = null;
                    string strBiblio = "";
                    string strTempBiblioRecPath = "";
                    long lRet = // sessioninfo.Channel.
                        channel.GetCommentInfo(
    null,
    "@path:" + strCommentRecPath,
    // null,
    "xml", // strResultType
    out strOldXml,
    out strOutputPath,
    out temp_timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strTempBiblioRecPath,
    out strError);
                    if (lRet == -1)
                    {
                        strError = "获得原有评注记录 '" + strCommentRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }

                    if (ByteArray.Compare(temp_timestamp, timestamp) != 0)
                    {
                        strError = "修改被拒绝。因为记录 '" + strCommentRecPath + "' 在保存前已经被其他人修改过。请重新装载";
                        goto ERROR1;
                    }
                }
                finally
                {
                    sessioninfo.ReturnChannel(channel);
                }
            }

            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(strOldXml) == false)
            {
                try
                {
                    dom.LoadXml(strOldXml);
                }
                catch (Exception ex)
                {
                    strError = "装载记录XML进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
                dom.LoadXml("<root/>");

            string strResTimeStamp = "";
            string strFileID = "";

            bool bChangeState = false;
            // 仅仅修改状态
            if (String.IsNullOrEmpty(strCommentRecPath) == false
                && this.EditAction == "changestate")
            {
                bChangeState = true;

                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");
                string strOldState = strState;
                this.GetStateValueFromControls(ref strState);
                DomUtil.SetElementText(dom.DocumentElement,
                    "state", strState);
                if (strState == strOldState)
                {
                    strError = "状态值没有发生改变，放弃保存评注记录";
                    goto ERROR1;
                }

                // 在<operations>中写入适当条目
                string strComment = "'" + strOldState + "' --> '" + strState + "'";
                nRet = SetOperation(
                    app,
                    ref dom,
                    "stateModified",
                    sessioninfo.UserID,
                    strComment,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 修改全部字段，但是不修改状态
                DomUtil.SetElementText(dom.DocumentElement,
                    "type", edit_type.Value);
                DomUtil.SetElementText(dom.DocumentElement,
                    "title", edit_title.Text);
                if (String.IsNullOrEmpty(strCommentRecPath) == false)
                {
                    // 覆盖存回
                    string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement,
                        "creator");
                    if (bManager == false
                        && strOriginCreator != sessioninfo.UserID)
                    {
                        strError = "当前用户 '" + sessioninfo.UserID + "' 不能修改由另一用户 '" + strOriginCreator + "' 创建的评注记录";
                        goto ERROR1;
                    }

                    nRet = SetOperation(
    app,
    ref dom,
    "lastContentModified",
    sessioninfo.UserID,
    "",
    false,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 新增记录
                    string strState = this.EditState;
                    if (string.IsNullOrEmpty(strState) == false)
                        DomUtil.SetElementText(dom.DocumentElement,
                        "state", strState);

                    // 创建者
                    XmlNode node = DomUtil.SetElementText(dom.DocumentElement,
                        "creator", strCreator);

                    // 如果有显示名
                    if (sessioninfo.ReaderInfo != null
                        && String.IsNullOrEmpty(sessioninfo.ReaderInfo.DisplayName) == false)
                    {
                        DomUtil.SetAttr(node, "displayName", sessioninfo.ReaderInfo.DisplayName);
                    }

                    // <operations>中会自动写入"create"条目
                }

                string strContent = edit_content.Text;
                strContent += strWarning;
                strContent = strContent.Replace("\r\n", "\\r");
                DomUtil.SetElementText(dom.DocumentElement,
                    "content", strContent);

                if (edit_type.Value == "订购征询")
                {
                    DomUtil.SetElementText(dom.DocumentElement,
                        "orderSuggestion", edit_yes.Checked == true ? "yes" : "no");
                    // 两个checked都是false，也算no?
                }

                // 图像
                if (upload.HasFile == true)
                {
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                    nsmgr.AddNamespace("dprms", DpNs.dprms);

                    // 全部<dprms:file>元素
                    XmlNodeList nodes = dom
                        .DocumentElement.SelectNodes("//dprms:file", nsmgr);

                    if (nodes.Count > 0)
                    {
                        // 找到第一个图像对象加以覆盖
                        foreach (XmlNode node in nodes)
                        {
                            // 只关注图像文件
                            string strMime = DomUtil.GetAttr(node, "__mime");
                            if (StringUtil.HasHead(strMime, "image/") == true)
                            {
                                strFileID = DomUtil.GetAttr(nodes[0], "id");
                                strResTimeStamp = DomUtil.GetAttr(nodes[0], "__timestamp");
                                break;
                            }
                        }
                    }

                    if (String.IsNullOrEmpty(strFileID) == true)
                    {
                        strFileID = FindNewFileID(dom);

                        XmlNode node = dom.CreateElement(
                            "dprms:file", DpNs.dprms);
                        dom.DocumentElement.AppendChild(node);

                        DomUtil.SetAttr(node, "id", strFileID);
                        DomUtil.SetAttr(node, "usage", "uploadimage");
                        // bXmlRecordChanged = true;
                    }
                }
            }

            string strNewCommentRecPath = "";
            string strNewXml = "";
            byte[] baNewTimestamp = null;

            string strAction = "";

            if (String.IsNullOrEmpty(strCommentRecPath) == true)
            {
                strAction = "new";

                // 创建一个评注记录
                nRet = CreateCommentInfo(
                    this.Page,
                    this.BiblioRecPath,
                    dom.DocumentElement.OuterXml,
                out strNewCommentRecPath,
                out strNewXml,
                out baNewTimestamp,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                // this.EditAction = "";
                this.ClearEdit();
            }
            else
            {
                strAction = "change";
                strNewCommentRecPath = strCommentRecPath;

                // 覆盖
                nRet = ChangeCommentInfo(
                    this.Page,
    biblio_recpath.Value,
    strCommentRecPath,
    strOldXml,
    dom.DocumentElement.OuterXml,
    timestamp,
out strNewXml,
out baNewTimestamp,
out strError);
                if (nRet == -1)
                    goto ERROR1;

                // this.EditAction = "";
            }

            if (upload.HasFile == true)
            {
                // 
                // 保存资源
                // 采用了代理帐户
                // return:
                //		-1	error
                //		0	发现上载的文件其实为空，不必保存了
                //		1	已经保存
                nRet = app.SaveUploadFile(
                    this.Page,
                    strNewCommentRecPath,
                    strFileID,
                    strResTimeStamp,
                    upload.PostedFile,
                    2048,
                    2048,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 2012/11/19
            this.RecPath = strNewCommentRecPath;

            bool bUpdateColumnAfterModifyState = false;    // 修改状态时是否调整Column列表顺序
            XmlNode nodeBookReview = app.WebUiDom.DocumentElement.SelectSingleNode("bookReview");
            if (nodeBookReview != null)
            {
                DomUtil.GetBooleanParam(nodeBookReview,
                    "updateColumnAfterModifyState",
                    false,
                    out bUpdateColumnAfterModifyState,
                    out strError);
            }

            if (bChangeState == false
                ||
                (bChangeState == true && bUpdateColumnAfterModifyState == true))
            {
                // 修改评注记录后，更新栏目存储结构
                // parameters:
                //      strAction   动作。change/delete/new
                // return:
                //      -2   栏目缓存尚未创建,因此无从更新
                //		-1	error
                //		0	not found line object
                //		1	succeed
                nRet = app.UpdateLine(
                    this.Page,
                    strAction,
                    strNewCommentRecPath,
                    strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return 0;
            ERROR1:
            return -1;
        }

        static string FindNewFileID(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // 全部<dprms:file>元素
            XmlNodeList nodes = dom
                .DocumentElement.SelectNodes("//dprms:file", nsmgr);
            if (nodes.Count > 0)
            {
                List<string> ids = new List<string>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    ids.Add(DomUtil.GetAttr(nodes[i], "id"));
                }

                ids.Sort();
                string strLastID = ids[ids.Count - 1];
                try
                {

                    Int64 v = Convert.ToInt64(strLastID);
                    return (v + 1).ToString();
                }
                catch
                {
                    return strLastID + "_1";
                }
            }

            return "0";
        }

        // 放弃编辑
        void cancel_button_Click(object sender, EventArgs e)
        {
            this.EditAction = "";
            this.ClearEdit();

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                e1.Focus = false;   // 不再独占Focus
                this.WantFocus(this, e1);
            }

            if (this.Submited != null)
            {
                SubmitedEventArgs e1 = new SubmitedEventArgs();
                e1.Action = "cancel";  // cancel
                this.Submited(this, e1);
            }
        }

        // 修改一个评注记录
        public static int ChangeCommentInfo(
            Page page,
            string strBiblioRecPath,
            string strCommentRecPath,
            string strOldXml,
            string strCommentXml,
            byte[] timestamp,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            strNewXml = "";
            baNewTimestamp = null;

            LoginState loginstate = GlobalUtil.GetLoginState(page);
            if (loginstate == LoginState.NotLogin)
            {
                strError = "尚未登录, 不能修改评注";
                return -1;
            }
            if (loginstate == LoginState.Public)
            {
                strError = "访客身份, 不能修改评注";
                return -1;
            }

            OpacApplication app = (OpacApplication)page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)page.Session["sessioninfo"];

            Debug.Assert(String.IsNullOrEmpty(sessioninfo.UserID) == false, "");

            EntityInfo info = new EntityInfo();

            // TODO: 是否应该从 strOldXml 中取得？
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = StringUtil.GetRecordId(strBiblioRecPath);

            XmlDocument comment_dom = new XmlDocument();
            try
            {
                comment_dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(comment_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "change";
            info.OldRecPath = strCommentRecPath;
            info.NewRecPath = strCommentRecPath;
            info.OldRecord = strOldXml;
            info.OldTimestamp = timestamp;
            info.NewRecord = comment_dom.OuterXml;
            info.NewTimestamp = null;

            // 
            EntityInfo[] comments = new EntityInfo[1];
            comments[0] = info;

            EntityInfo[] errorinfos = null;

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                long lRet = // sessioninfo.Channel.
                    channel.SetComments(
                    null,
                    strBiblioRecPath,
                    comments,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        // strNewCommentRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        // 创建一个评注记录
        public static int CreateCommentInfo(
            Page page,
            string strBiblioRecPath,
            string strCommentXml,
            out string strNewCommentRecPath,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            strNewCommentRecPath = "";
            strNewXml = "";
            baNewTimestamp = null;

            LoginState loginstate = GlobalUtil.GetLoginState(page);
            if (loginstate == LoginState.NotLogin)
            {
                strError = "尚未登录, 不能创建评注";
                return -1;
            }
            // TODO: webui.xml中配置访客是否可以创建评注
            if (loginstate == LoginState.Public)
            {
                strError = "访客身份, 不能创建评注";
                return -1;
            }

            OpacApplication app = (OpacApplication)page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)page.Session["sessioninfo"];

            Debug.Assert(string.IsNullOrEmpty(sessioninfo.UserID) == false, "");

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = StringUtil.GetRecordId(strBiblioRecPath);

            XmlDocument comment_dom = new XmlDocument();
            try
            {
                comment_dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(comment_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "new";
            info.NewRecPath = "";
            info.NewRecord = comment_dom.OuterXml;
            info.NewTimestamp = null;

            // 
            EntityInfo[] comments = new EntityInfo[1];
            comments[0] = info;

            EntityInfo[] errorinfos = null;

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                long lRet = // sessioninfo.Channel.
                    channel.SetComments(
        null,
        strBiblioRecPath,
        comments,
        out errorinfos,
        out strError);
                if (lRet == -1)
                {
                    return -1;
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        strNewCommentRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        public void Clear()
        {
            this.ClearEdit();
        }

        void ClearEdit()
        {
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
        }

        // ""/"change"/"changestate"/"new"
        public string EditAction
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("editaction");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("editaction");
                s.Value = value;
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

        public static int GetParentID(string strXml,
            out string strParentID,
            out string strError)
        {
            strError = "";
            strParentID = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            strParentID = DomUtil.GetElementText(dom.DocumentElement, "parent");
            return 0;
        }

        void SetStateValueToControls(string strState)
        {
            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            checkbox.Checked = (StringUtil.IsInList("屏蔽", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_censor");
            checkbox.Checked = (StringUtil.IsInList("审查", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_locked");
            checkbox.Checked = (StringUtil.IsInList("锁定", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            checkbox.Checked = (StringUtil.IsInList("精品", strState) == true);
        }

        string Timestamp
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_timestamp");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_timestamp");
                s.Value = value;
            }
        }

#if NO
        public string ReLogin
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("relogin");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("relogin");
                s.Value = value;
            }
        }
#endif


        public string EditType
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_type");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_type");
                s.Value = value;
            }
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


        public string BiblioRecPath
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("biblio_recpath");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("biblio_recpath");
                s.Value = value;
            }
        }

        public string CommentRecPath
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("comment_recpath");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("comment_recpath");
                s.Value = value;
            }
        }


        public static string GetBiblioRecPath(
            OpacApplication app,
            string strCommentRecPath,
            string strParentID)
        {
            string strCommentDbName = StringUtil.GetDbName(strCommentRecPath);
            string strBiblioDbName = GetBiblioDbName(app, strCommentDbName);

            return strBiblioDbName + "/" + strParentID;
        }

        static string GetBiblioDbName(OpacApplication app,
    string strCommentDbName)
        {
            string strBiblioDbName = "";
            string strError = "";
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = app.GetBiblioDbNameByCommentDbName(strCommentDbName,
            out strBiblioDbName,
            out strError);
            if (nRet != 1)
                return null;
            return strBiblioDbName;
        }

        // 准备编辑区域
        int PrepareEdit(
            OpacApplication app,
            SessionInfo sessioninfo,
            string strEditAction,
            string strRecPath,
            out string strError)
        {
            strError = "";

            Button minimize_button = (Button)this.FindControl("minimize_button");

            if (this.Minimized != "true")
                minimize_button.Visible = false;

            if (this.Active == false)
            {
                Button button = (Button)this.FindControl("changebutton");
                button.Enabled = false;
                button = (Button)this.FindControl("deletebutton");
                button.Enabled = false;
                button = (Button)this.FindControl("statebutton");
                button.Enabled = false;
                button = (Button)this.FindControl("submit_button");
                button.Enabled = false;
                button = (Button)this.FindControl("cancel_button");
                button.Enabled = false;

                minimize_button.Enabled = false;
            }

            PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
            PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");

            if (String.IsNullOrEmpty(strEditAction) == true)
            {
                inputline.Visible = false;

                // cmdline.Visible = false;
                return 0;
            }

            /*
            bool bManager = false;
            if (sessioninfo.Account == null
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;
             * */

            HiddenField edit_type = (HiddenField)this.FindControl("edit_type");
            TextBox edit_title = (TextBox)this.FindControl("edit_title");
            TextBox edit_content = (TextBox)this.FindControl("edit_content");
            HiddenField biblio_recpath = (HiddenField)this.FindControl("biblio_recpath");
            RadioButton edit_yes = (RadioButton)this.FindControl("edit_yes");
            RadioButton edit_no = (RadioButton)this.FindControl("edit_no");
            LiteralControl recordinfo = (LiteralControl)this.FindControl("recordinfo");
            HiddenField comment_recpath = (HiddenField)this.FindControl("comment_recpath");

            if (this.Active == false)
            {
                edit_title.Enabled = false;
                edit_content.Enabled = false;
                edit_yes.Enabled = false;
                edit_no.Enabled = false;
            }

            string strOrderSuggestion = "";

            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                strOrderSuggestion = "yes";

                recordinfo.Text = this.GetString("创建者") + ": " + GetCurrentAccountDisplayName();
            }
            else
            {
                string strXml;
                byte[] timestamp = null;

                if (String.IsNullOrEmpty(this.m_strXml) == true)
                {
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    int nRet = this.GetRecord(
                        app,
                        sessioninfo,
                        null,
                        strRecPath,
                        out strXml,
                        out timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    strXml = this.m_strXml;
                    timestamp = this.m_timestamp;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    if (string.IsNullOrEmpty(strXml) == false)
                        dom.LoadXml(strXml);
                    else
                        dom.LoadXml("<root />");
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                string strParentID = DomUtil.GetElementText(dom.DocumentElement, "parent");

                // 帖子状态：精华、...。
                string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

                // 帖子标题
                string strTitle = DomUtil.GetElementText(dom.DocumentElement, "title");

                // 作者
                string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement, "creator");

                string strOriginContent = DomUtil.GetElementText(dom.DocumentElement, "content");

                string strType = DomUtil.GetElementText(dom.DocumentElement, "type");
                strOrderSuggestion = DomUtil.GetElementText(dom.DocumentElement, "orderSuggestion");

                edit_title.Text = strTitle;
                string strContent = strOriginContent.Replace("\\r", "\r\n");
                edit_content.Text = strContent;
                edit_type.Value = strType;

                if (strEditAction == "changestate")
                {
                    this.SetStateValueToControls(strState);
                }

                // recpath_holder.Visible = true;
                recordinfo.Text = this.GetString("修改者") + ": " + GetCurrentAccountDisplayName() + "; " + this.GetString("记录路径") + ": " + strRecPath;
                comment_recpath.Value = strRecPath;

                // 修改他人的评注
                if (string.IsNullOrEmpty(sessioninfo.UserID) == false
                    && strOriginCreator != sessioninfo.UserID)
                {
                    // "您正在编辑他人的评注。原始创建者为 "+strOriginCreator+"。"
                    this.SetCommentInfo("comment",
                        string.Format(this.GetString("您正在编辑他人的评注。原始创建者为X。"), strOriginCreator)
                        );
                }
            }

            if (string.IsNullOrEmpty(sessioninfo.UserID) == false)
            {
                if (IsReaderHasnotDisplayName() == true)
                {
                    recordinfo.Text += "<div class='comment'>" + this.GetString("若想以个性化的作者名字发表评注") + "，<a href='./personalinfo.aspx' target='_blank'>" + this.GetString("点这里立即添加我的显示名") + "</a></div>";
                }

                /*
                // 准备relogin
                string strSHA1 = Cryptography.GetSHA1(sessioninfo.UserID + "|" + sessioninfo.Account.Password);
                this.ReLogin = "username=" + sessioninfo.UserID + ",token=" + strSHA1 + ",type=" + sessioninfo.Account.Type;
                 * */
            }

            bool bOrderComment = false;
            if (edit_type.Value == "订购征询")
            {
                if (strOrderSuggestion == "yes")
                    edit_yes.Checked = true;
                else
                    edit_no.Checked = true;
                bOrderComment = true;
            }

            PlaceHolder ordercomment_description_holder = (PlaceHolder)this.FindControl("ordercomment_description_holder");
            LiteralControl description = (LiteralControl)this.FindControl("edit_description");
            if (bOrderComment == true)
            {
                // ordercomment_description_holder.Visible = true;
                if (strEditAction == "new")
                    description.Text = this.GetString("本书正在征求订购意见") + "：";
                else
                    description.Text = this.GetString("订购建议") + "：";
                //type.Value = "订购征询";

                if (this.Minimized == "true")
                    minimize_button.Text = this.GetString("本书正在征求订购意见");
            }
            else
            {
                ordercomment_description_holder.Visible = false;
                if (strEditAction == "new")
                    description.Text = this.GetString("在此贡献您的书评") + "：";
                else
                    description.Text = this.GetString("评注") + "：";
                //type.Value = "书评";

                if (this.Minimized == "true")
                    minimize_button.Text = this.GetString("在此贡献您的书评");
            }

            // PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
            cmdline.Visible = false;

            PlaceHolder state_holder = (PlaceHolder)this.FindControl("state_holder");
            if (this.EditAction == "changestate")
            {
                // 仅仅修改状态。上面的编辑字段都不要出现，只出现状态编辑
                state_holder.Visible = true;
                PlaceHolder edit_up_holder = (PlaceHolder)this.FindControl("edit_up_holder");
                edit_up_holder.Visible = false;
            }
            else
            {
                // 修改内容。上面的编辑字段都出现，状态编辑部分不出现
                state_holder.Visible = false;
            }

            return 0;
        }

        // 是否属于 读者并且没有显示名的情况
        bool IsReaderHasnotDisplayName()
        {
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (string.IsNullOrEmpty(sessioninfo.UserID) == true)
                return false;

            if (sessioninfo.IsReader == false)
                return false;

            if (sessioninfo.ReaderInfo == null)
                return false;

            if (String.IsNullOrEmpty(sessioninfo.ReaderInfo.DisplayName) == true)
                return true;

            return false;
        }

        // 获得当前创建者的显示名
        // 如果是来自显示名，则有[]括住
        string GetCurrentAccountDisplayName()
        {
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (sessioninfo.ReaderInfo != null
                && String.IsNullOrEmpty(sessioninfo.ReaderInfo.DisplayName) == false)
                return "[ " + sessioninfo.ReaderInfo.DisplayName + " ]";

            return sessioninfo.UserID;
        }

        // 创建文章内容
        int BuildContent(
    OpacApplication app,
    SessionInfo sessioninfo,
    string strRecPath,
    out string strParentID,
    out string strResultParam,
    out byte[] timestamp,
    out string strError)
        {
            strError = "";
            strResultParam = "";
            strParentID = "";
            timestamp = null;

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            string strErrorComment = "";
            string strXml;
            if (String.IsNullOrEmpty(this.m_strXml) == true)
            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = GetRecord(
                    app,
                    sessioninfo,
                    null,
                    strRecPath,
                    out strXml,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    strErrorComment = strError;
            }
            else
            {
                strXml = this.m_strXml;
                timestamp = this.m_timestamp;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(strXml) == false)
                    dom.LoadXml(strXml);
                else
                    dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            strParentID = DomUtil.GetElementText(dom.DocumentElement, "parent");

            // 帖子状态：精华、...。
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

            // 帖子标题
            string strTitle = DomUtil.GetElementText(dom.DocumentElement, "title");

            if (string.IsNullOrEmpty(strXml) == true)
                strTitle = strErrorComment;

            // 作者
            string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement, "creator");

            string strOriginContent = DomUtil.GetElementText(dom.DocumentElement, "content");

            string strOperInfo = "";
            {
                string strFirstOperator = "";
                string strTime = "";

                XmlNode node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
                if (node != null)
                {
                    strFirstOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += " " + this.GetString("创建") + ": "
                        + GetUTimeString(strTime);
                }

                node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='lastContentModified']");
                if (node != null)
                {
                    string strLastOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += "<br/>" + this.GetString("最后修改") + ": "
                        + GetUTimeString(strTime);
                    if (strLastOperator != strFirstOperator)
                        strOperInfo += " (" + strLastOperator + ")";
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("operations/operation[@name='stateModified']");
                if (nodes.Count > 0)
                {
                    XmlNode tail = nodes[nodes.Count - 1];
                    string strLastOperator = DomUtil.GetAttr(tail, "operator");
                    strTime = DomUtil.GetAttr(tail, "time");
                    strOperInfo += "<br/>" + this.GetString("状态最后修改") + ": "
                        + GetUTimeString(strTime);
                    if (strLastOperator != strFirstOperator)
                        strOperInfo += " (" + strLastOperator + ")";
                }
            }
            /*
            strResult += "<div class='operinfo' title='" + this.GetString("记录路径") + ": " + strRecPath + "'>"
+ strOperInfo
+ "</div>";*/


            bool bDisplayOriginContent = false;
            string strDisplayState = "";

            string strStateImage = "";
            {
                if (StringUtil.IsInList("审查", strState) == true)
                {
                    strStateImage = "<img src='" + MyWebPage.GetStylePath(app, "censor.gif") + "'/>";
                    strDisplayState = this.GetString("审查");
                }
                if (StringUtil.IsInList("屏蔽", strState) == true)
                {
                    strStateImage = "<img src='" + MyWebPage.GetStylePath(app, "disable.gif") + "'/>";
                    if (String.IsNullOrEmpty(strDisplayState) == false)
                        strDisplayState += ",";
                    strDisplayState += this.GetString("屏蔽");
                }

                if (StringUtil.IsInList("审查", strState) == false
                    && StringUtil.IsInList("屏蔽", strState) == false)
                    bDisplayOriginContent = true;

                // 管理员和作者本人都能看到被屏蔽的内容
                if (bManager == true || strOriginCreator == sessioninfo.UserID)
                    bDisplayOriginContent = true;

            }

            if (string.IsNullOrEmpty(strXml) == true)
                strDisplayState = strErrorComment;

            StringBuilder strResult = new StringBuilder(4096);

            // headbar
            {
                string strUserInfoUrl = "";
                string strImageUrl = "";
                string strDisplayNameComment = "";  // 关于显示名的注释

                string strCreator = GetCreatorDisplayName(dom);
                if (bDisplayOriginContent == false || string.IsNullOrEmpty(strXml) == true)
                {
                    strTitle = new string('*', strTitle.Length);
                    strCreator = new string('*', strCreator.Length);
                    strUserInfoUrl = "";
                    strImageUrl = MyWebPage.GetStylePath(app, "nonephoto.png");  // TODO: 将来可以更换为表示屏蔽状态的头像
                }
                else
                {
                    strUserInfoUrl = "./userinfo.aspx?" + GetCreatorUrlParam(dom);
                    strImageUrl = "./getphoto.aspx?" + GetCreatorUrlParam(dom);
                }

                if (strCreator.IndexOf("[") != -1
                    && bManager == true)
                {
                    // 如果是显示名，并且当前为管理员状态
                    strDisplayNameComment = "管理员注: 该用户的证条码号(或帐户名)为 " + DomUtil.GetElementText(dom.DocumentElement, "creator")
                        + "\r\n(此信息为机密信息，普通用户看不到)";
                    strDisplayNameComment = HttpUtility.HtmlEncode(strDisplayNameComment);
                }

                strResult.Append("<div class='headbar'>");

                strResult.Append("<div class='creator_pic' title='" + this.GetString("作者头像") + "'>"
    + "<a href='" + strUserInfoUrl + "' target='_blank'><img border='0' width='64' height='64' src='" + strImageUrl + "' /></a>"
    + "</div>");
                // 
                strResult.Append("<div class='title_and_creator'>");

                if (String.IsNullOrEmpty(strTitle) == false)
                {
                    strResult.Append("<div class='title' title='" + this.GetString("文章标题") + "'>"
                        + HttpUtility.HtmlEncode(strTitle)
                        + "</div>");
                }

                if (String.IsNullOrEmpty(strCreator) == false)
                {
                    strResult.Append("<div class='creator' title='"
                        + (String.IsNullOrEmpty(strDisplayNameComment) == false ? strDisplayNameComment : this.GetString("作者"))
                        + "'>"
                        + "<a href='" + strUserInfoUrl + "' target='_blank'>"
                        + HttpUtility.HtmlEncode(
                        strCreator
                        // String.IsNullOrEmpty(strDisplayName) == false ? strDisplayName : strCreator
                        )
                        + "</a>"
            + "</div>");
                }

                strResult.Append("<div class='operinfo' title='" + this.GetString("记录路径") + ": " + strRecPath + "'>"
                    + strOperInfo
                    + "</div>");

                strResult.Append("</div>");  // of title_and_creator

                strResult.Append("<div class='path' title='" + this.GetString("记录路径") + "'>"
+ "<a href='./book.aspx?CommentRecPath=" + HttpUtility.UrlEncode(strRecPath) + "#active' target='_blank'>" + strRecPath + "</a>"
+ "</div>");

                strResult.Append("<div class='clear'> </div>");

                strResult.Append("</div>");  // of headbar
            }

            // 屏蔽原因
            if (String.IsNullOrEmpty(strDisplayState) == false)
            {
                strResult.Append("<div class='forbidden' title='" + this.GetString("屏蔽原因") + "'>"
                    + strStateImage
                    + (string.IsNullOrEmpty(strXml) == false ? string.Format(this.GetString("本评注目前为X状态"), strDisplayState) : strDisplayState)
                    + (strOriginCreator == sessioninfo.UserID ? "，" + this.GetString("其他人(非管理员)看不到下列内容") : "")
                    + "</div>");
            }

            // 精品和锁定状态
            {
                string strImage = "";

                if (StringUtil.IsInList("精品", strState) == true)
                {
                    strImage = "<img src='" + MyWebPage.GetStylePath(app, "valuable.gif") + "'/>";
                    strResult.Append("<div class='valuable' title='精品'>"
                        + strImage
    + this.GetString("精品")
    + "</div>");
                }
                if (StringUtil.IsInList("锁定", strState) == true)
                {
                    strImage = "<img src='" + MyWebPage.GetStylePath(app, "locked.gif") + "'/>";
                    strResult.Append("<div class='locked' title='锁定'>"
                        + strImage
    + this.GetString("锁定")
    + "</div>");
                }
            }

            if (bDisplayOriginContent == true)
            {
                string strType = DomUtil.GetElementText(dom.DocumentElement, "type");
                string strOrderSuggestion = DomUtil.GetElementText(dom.DocumentElement, "orderSuggestion");

                if (strType == "订购征询")
                {
                    string strOrderSuggestionText = this.GetString("建议不要订购本书");
                    string strYesOrNo = "no";
                    if (strOrderSuggestion == "yes")
                    {
                        strOrderSuggestionText = this.GetString("建议订购本书");
                        strYesOrNo = "yes";
                    }
                    strResult.Append("<div class='order_suggestion " + strYesOrNo + "' title='" + this.GetString("订购建议") + "'>"
                        + HttpUtility.HtmlEncode(strOrderSuggestionText)
                        + "</div>");
                }


                if (String.IsNullOrEmpty(strOriginContent) == false)
                {
                    string strContent = strOriginContent.Replace("\\r", "\r\n");
                    strContent = ParseHttpString(
                        this.Page,
                        strContent);

                    strContent = GetHtmlContentFromPureText(null, /*this.Page,*/
                        strContent,
                        Text2HtmlStyle.P);

                    strResult.Append("<div class='content' title='" + this.GetString("文章正文") + "'>"
        + strContent
        + "</div>");
                }

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);

                // 全部<dprms:file>元素
                XmlNodeList nodes = dom.DocumentElement.SelectNodes(
                    "//dprms:file", nsmgr);
                foreach (XmlNode node in nodes)
                {
                    string strMime = DomUtil.GetAttr(node, "__mime");
                    if (StringUtil.HasHead(strMime, "image/") == false)
                        continue;   // 只关注图像文件

                    string strID = DomUtil.GetAttr(node, "id");
                    string strUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strRecPath) + "/object/" + strID;
                    strResult.Append("<div class='image' title='" + this.GetString("图像文件") + "'>"
                        + "<img src='" + strUrl + "' />"
                        + "</div>");
                }
            }



            if (bManager == true)
            {
                string strText = GetStateModifiedHistory(dom);
                if (String.IsNullOrEmpty(strText) == false)
                    strResult.Append("<div class='historytitle'>" + this.GetString("状态修改史") + ":</div>"
                        + strText);
            }

            // 设置命令行 按钮状态
            PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
            string strEditAction = this.EditAction;

            if (String.IsNullOrEmpty(strEditAction) == false)
                cmdline.Visible = false;
            else
            {
                Button change = (Button)this.FindControl("changebutton");
                Button delete = (Button)this.FindControl("deletebutton");
                Button state = (Button)this.FindControl("statebutton");

                // 非编辑状态 

                bool bChangable = false;
                if (((string.IsNullOrEmpty(sessioninfo.UserID) == false
                    && strOriginCreator == sessioninfo.UserID)    // 创建者本人
                    || bManager == true)    // 管理者
                                            /*&& this.Active == true*/)
                    bChangable = true;
                else
                    bChangable = false;

                // 进一步根据状态进行限定
                if (bChangable == true)
                {
                    // 非管理者，在“锁定”状态下不能修改和删除
                    if (bManager == false
                        && StringUtil.IsInList("锁定", strState) == true)
                        bChangable = false;
                }

                if (bChangable == true)
                {
                    change.Visible = true;
                    delete.Visible = true;
                }
                else
                {
                    change.Visible = false;
                    delete.Visible = false;
                }

                if (bManager == true)
                    state.Visible = true;
                else
                    state.Visible = false;

                // 如果每个按钮都是不可见，则隐藏命令条
                if (change.Visible == false
                    && delete.Visible == false
                    && state.Visible == false)
                    cmdline.Visible = false;

            }

            strResultParam = strResult.ToString();
            return 0;
        }

        public int LoadRecord(string strCommentRecPath,
            out string strParentID,
            out string strError)
        {
            strError = "";
            strParentID = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            byte[] timestamp = null;
            string strXml = "";
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.GetRecord(
                app,
                sessioninfo,
                null,
                strCommentRecPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = CommentControl.GetParentID(strXml,
                out strParentID,
                out strError);
            if (nRet == -1)
                return -1;

            this.RecPath = strCommentRecPath;
            return 1;
        }

        // 预先取得，暂时存储
        private string m_strXml = "";
        private byte[] m_timestamp = null;

        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetRecord(
            OpacApplication app,
            SessionInfo sessioninfo,
            LibraryChannel channel_param,
            string strRecPath,
            out string strXml,
            out byte[] timestamp,
            out string strError)
        {
            strError = "";
            timestamp = null;
            strXml = "";

            LibraryChannel channel = null;

            if (channel_param != null)
                channel = channel_param;
            else
                channel = sessioninfo.GetChannel(true);
            try
            {
                string strOutputPath = "";
                string strBiblio = "";
                string strBiblioRecPath = "";
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                //      >1  命中多于1条
                long lRet = // sessioninfo.Channel.
                    channel.GetCommentInfo(
    null,
    "@path:" + strRecPath,
    // null,
    "xml", // strResultType
    out strXml,
    out strOutputPath,
    out timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strBiblioRecPath,
    out strError);
                if (lRet == -1)
                {
                    strError = "获取评注记录 '" + strRecPath + "' 时出错: " + strError;
                    return -1;
                }
                if (lRet == 0)
                {
                    strError = "评注记录 '" + strRecPath + "' 没有找到";
                    return 0;
                }

                m_strXml = strXml;
                m_timestamp = timestamp;
                return 1;
            }
            finally
            {
                if (channel_param == null)
                    sessioninfo.ReturnChannel(channel);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int nRet = 0;
            string strError = "";

            LiteralControl frame_style = (LiteralControl)this.FindControl("frame_style");
            if (this.Minimized == "true")
                frame_style.Text = " style='DISPLAY:none'";

            if (String.IsNullOrEmpty(this.RecPath) == true
                && String.IsNullOrEmpty(this.EditAction) == true)
            {
                this.Visible = false;
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (this.EditAction == "new")
            {
                // 准备编辑区域
                nRet = PrepareEdit(
                    app,
                    sessioninfo,
                    this.EditAction,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                base.Render(writer);
                return;
            }

            /*
            bool bManager = false;
            if (sessioninfo.Account == null
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;
             * */

            // 内容占位符
            PlaceHolder content = (PlaceHolder)this.FindControl("content");
            // 编辑区域占位符
            PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");

            string strParentID = "";
            string strResult = "";
            byte[] timestamp = null;
            nRet = BuildContent(
                app,
                sessioninfo,
                this.RecPath,
                out strParentID,
                out strResult,
                out timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            content.Controls.Add(new LiteralControl(strResult));

            // 准备编辑区域
            nRet = PrepareEdit(
                app,
                sessioninfo,
                this.EditAction,
                this.RecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.CommentRecPath = this.RecPath;
            this.Timestamp = ByteArray.GetHexTimeStampString(timestamp);
            this.BiblioRecPath = GetBiblioRecPath(
                app,
                this.RecPath,
                strParentID);

            base.Render(writer);
            return;
            ERROR1:
            this.SetDebugInfo("errorinfo", strError);
            base.Render(writer);
        }

        // 把纯文本变为适合html显示的格式
        public static string GetHtmlContentFromPureText(
            System.Web.UI.Page page,
            string strText,
            Text2HtmlStyle style)
        {
            string[] aLine = strText.Replace("\r", "").Split(new char[] { '\n' });
            string strResult = "";
            for (int i = 0; i < aLine.Length; i++)
            {
                string strLine = aLine[i];

                if (style == Text2HtmlStyle.BR)
                {
                    if (page == null)
                        strResult += strLine + "<br/>";
                    else
                        strResult += HttpUtility.HtmlEncode(strLine) + "<br/>";
                }
                else if (style == Text2HtmlStyle.P)
                {
                    if (String.IsNullOrEmpty(strLine) == true)
                        strResult += "<p>&nbsp;</p>";
                    else
                    {
                        if (page == null)
                            strResult += "<p>" + strLine + "</p>";
                        else
                            strResult += "<p>" + HttpUtility.HtmlEncode(strLine) + "</p>";
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(strLine) == true)
                        strResult += "<p>&nbsp;</p>";
                    else
                    {
                        if (page == null)
                            strResult += "<p>" + strLine + "</p>";
                        else
                            strResult += "<p>" + HttpUtility.HtmlEncode(strLine) + "</p>";
                    }
                }
            }

            return strResult;
        }

        // 将纯文本中的"http://"替换为<a>命令
        public static string ParseHttpString(
            System.Web.UI.Page page,
            string strText)
        {
            string strResult = "";
            int nCur = 0;
            for (; ; )
            {
                int nStart = strText.IndexOf("http://", nCur);
                if (nStart == -1)
                {
                    if (page == null)
                        strResult += strText.Substring(nCur);
                    else
                        strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur)));
                    break;
                }

                // 复制nCur到nStart一段
                if (page == null)
                    strResult += strText.Substring(nCur, nStart - nCur);
                else
                    strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur, nStart - nCur)));

                int nEnd = strText.IndexOfAny(new char[] { ' ', ',', ')', '(', '\r', '\n', '\"', '\'' },
                    nStart + 1);
                if (nEnd == -1)
                    nEnd = strText.Length;

                string strUrl = strText.Substring(nStart, nEnd - nStart);

                string strLeft = "<a href='" + strUrl + "' target='_blank'>";
                string strRight = "</a>";

                if (page == null)
                    strResult += strLeft + strUrl + strRight;
                else
                    strResult += strLeft + HttpUtility.HtmlEncode(strUrl) + strRight;

                nCur = nEnd;
            }

            return strResult;
        }

        // 把一个字符串开头的连续空白替换为&nbsp;
        public static string ReplaceLeadingBlank(string strText)
        {
            if (strText == "")
                return "";
            strText = strText.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            return strText;
            // return strText.Replace(" ", "&nbsp;");  // 2007/12/11 changed
        }

        public static string GetStateModifiedHistory(XmlDocument dom)
        {
            string strResult = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("operations/operation[@name='stateModified']");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strTime = DomUtil.GetAttr(node, "time");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strComment = DomUtil.GetAttr(node, "comment");

                strResult += "<div class='history'>"
                    + GetUTimeString(strTime)
                    + " ("
                    + strOperator
                    + ") : "
                    + strComment
                    + "</div>";
            }

            return strResult;
        }


        // 获得<creator>元素内的显示名
        // 如果是来自显示名，则有[]括住
        public static string GetCreatorDisplayName(XmlDocument dom)
        {
            XmlNode creator_node = dom.DocumentElement.SelectSingleNode("creator");
            if (creator_node == null)
                return "";

            string strDisplayName = DomUtil.GetAttr(creator_node, "displayName");
            if (String.IsNullOrEmpty(strDisplayName) == false)
                return "[ " + strDisplayName + " ]";

            return creator_node.InnerText;
        }

        // 获得适合于URL使用的参数字符串
        // 如果有显示名，则为 displayName=???&barcode=???形态；
        // 如果没有显示名，则为 barcode=???形态
        public static string GetCreatorUrlParam(XmlDocument dom)
        {
            XmlNode creator_node = dom.DocumentElement.SelectSingleNode("creator");
            if (creator_node == null)
                return "";


            string strDisplayName = DomUtil.GetAttr(creator_node, "displayName");
            if (String.IsNullOrEmpty(strDisplayName) == false)
            {
                string strEncrypt = OpacApplication.EncryptPassword(creator_node.InnerText);
                return "&displayName=" + HttpUtility.UrlEncode(strDisplayName) + "&encrypt_barcode=" + HttpUtility.UrlEncode(strEncrypt);
            }

            return "barcode=" + HttpUtility.UrlEncode(creator_node.InnerText);
        }

        public static string GetUTimeString(string strRfc1123TimeString)
        {
            if (String.IsNullOrEmpty(strRfc1123TimeString) == true)
                return "";

            DateTime time = new DateTime(0);
            try
            {
                time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123TimeString);
            }
            catch
            {
            }

            return time.ToLocalTime().ToString("u");
        }

    }

    public enum CommentDispStyle
    {
        Comment = 0x02,    // 显示当前评注
        Comments = 0x04,   // 显示同种的全部评注
    }

    public delegate void SumitedEventHandler(object sender,
SubmitedEventArgs e);

    /// <summary>
    /// 通知状态变化事件的参数
    /// </summary>
    public class SubmitedEventArgs : EventArgs
    {
        public string Action = "";  // new / change / cancel
    }
}
