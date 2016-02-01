using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;

namespace DigitalPlatform.Message
{
    /// <summary>
    /// 显示、编辑一条消息
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:MessageControl runat=server></{0}:MessageControl>")]
    public class MessageControl : WebControl, INamingContainer
    {
        public override void Dispose()
        {
            if (this.Channels != null)
                this.Channels.Dispose();

            base.Dispose();
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.Message.res.MessageControl.cs",
                typeof(MessageControl).Module.Assembly);

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
            catch (Exception /* ex */)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        public MessageCenter MessageCenter = null;

        public MessageData MessageData = null;

        public RmsChannelCollection Channels = null;

        public string UserID = "";

        //
        public string Mode
        {
            get
            {
                object o = ViewState["Mode"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                ViewState["Mode"] = (object)value;
            }
        }

        // 来源记录id
        public string RecordID
        {
            get
            {
                object o = ViewState["RecordID"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                ViewState["RecordID"] = (object)value;
            }
        }

        // 来源记录的时间戳
        public byte [] TimeStamp
        {
            get
            {
                object o = ViewState["TimeStamp"];
                return (o == null) ? (byte[])null : (byte[])o;
            }
            set
            {
                ViewState["TimeStamp"] = (object)value;
            }
        }

        // 来源记录所从属的邮箱
        public string BoxName
        {
            get
            {
                object o = ViewState["BoxName"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                ViewState["BoxName"] = (object)value;
            }
        }

        // 来源记录id集合
        public List<string> RecordIDs
        {
            get
            {
                object o = ViewState["RecordIDs"];
                return (o == null) ? (List<string>)null : (List<string>)o;
            }
            set
            {
                ViewState["RecordIDs"] = (object)value;
            }
        }

        // 上述集合中的当前位置
        public int RecordIDsIndex
        {
            get
            {
                object o = ViewState["RecordIDsIndex"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                ViewState["RecordIDsIndex"] = (object)value;
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

        public string Sender
        {
            get
            {
                this.EnsureChildControls();

                TextBox sender = (TextBox)this.FindControl("sender");
                return sender.Text;
            }
            set
            {
                this.EnsureChildControls();

                TextBox sender = (TextBox)this.FindControl("sender");
                sender.Text = value;
            }
        }

        public string Recipient
        {
            get
            {
                this.EnsureChildControls();

                TextBox sender = (TextBox)this.FindControl("recipient");
                return sender.Text;
            }
            set
            {
                this.EnsureChildControls();

                TextBox sender = (TextBox)this.FindControl("recipient");
                sender.Text = value;
            }
        }

        protected override void CreateChildControls()
        {
            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetPrefixString(
                this.GetString("消息"),
                "content_wrapper");
            literal.Text += "<table class='message'>"    //  width='100%' cellspacing='1' cellpadding='4'
                +"";
            this.Controls.Add(literal);

            PlaceHolder edit = new PlaceHolder();
            edit.ID = "edit";
            this.Controls.Add(edit);

            // 综合信息
            literal = new LiteralControl();
            literal.Text = "<tr class='info'>"
                + "<td class='info' colspan='2'>";
            edit.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "infotext";
            edit.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);


            // 发件人
            literal = new LiteralControl();
            literal.Text = "<tr class='sender'>"
                +"<td class='name' nowrap width='10%'>"
                + this.GetString("发件人")
                + "</td><td class='content' width='90%'>";
            edit.Controls.Add(literal);

            TextBox sender = new TextBox();
            sender.ID = "sender";
            sender.Width = new Unit("99%");
            edit.Controls.Add(sender);

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);

            // 收件人
            literal = new LiteralControl();
            literal.Text = "<tr class='recipient'>"
                + "<td class='name' nowrap width='10%'>"
                + this.GetString("收件人")
                + "</td><td class='content' width='90%' width='90%'>";
            edit.Controls.Add(literal);

            TextBox recipient = new TextBox();
            recipient.ID = "recipient";
            recipient.Width = new Unit("99%");
            edit.Controls.Add(recipient);

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);

            // 主题
            literal = new LiteralControl();
            literal.Text = "<tr class='subject'>"
                + "<td class='name' nowrap width='10%'>"
                + this.GetString("主题")
                + "</td><td class='content' width='90%'>";
            edit.Controls.Add(literal);

            TextBox subject = new TextBox();
            subject.ID = "subject";
            subject.Width = new Unit("99%");
            edit.Controls.Add(subject);

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);

            // 正文
            literal = new LiteralControl();
            literal.Text = "<tr class='content'>"
                + "<td class='name' nowrap width='10%'>"
                + this.GetString("正文")
                + "</td><td class='content' width='90%'>";
            edit.Controls.Add(literal);


            TextBox content = new TextBox();
            content.ID = "content";
            content.Width = new Unit("99%");
            content.Rows = 10;
            content.TextMode = TextBoxMode.MultiLine;
            edit.Controls.Add(content);

            // html正文
            PlaceHolder html_content_container = new PlaceHolder();
            html_content_container.ID = "html_content_container";
            edit.Controls.Add(html_content_container);


            literal = new LiteralControl();
            literal.Text = "<div>";
            html_content_container.Controls.Add(literal);


            literal = new LiteralControl();
            literal.ID = "html_content";
            literal.Text = "";
            html_content_container.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</div>";
            html_content_container.Controls.Add(literal);

            // 


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);

            // 时间
            literal = new LiteralControl();
            literal.Text = "<tr class='date'>"
                + "<td class='name' nowrap width='10%'>"
                + this.GetString("时间")
                + "</td><td class='content' width='90%'>";
            edit.Controls.Add(literal);

            TextBox date = new TextBox();
            date.ID = "date";
            date.Width = new Unit("99%");
            edit.Controls.Add(date);

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            edit.Controls.Add(literal);


            CreateCmdLine(edit);

            CreateDebugLine(edit);

            PlaceHolder endinfoline = new PlaceHolder();
            endinfoline.ID = "endinfoline";
            this.Controls.Add(endinfoline);

            CreateEndInfo(endinfoline);


            literal = new LiteralControl();
            literal.Text = "</table>" + this.GetPostfixString();
            this.Controls.Add(literal);


            if (this.RecordIDs != null)
            {
                // 装入一条记录
            }

            if (this.MessageData != null)
            {
                this.SetMessageData(this.MessageData);
                this.SetState(this.MessageData.strBoxType);
            }
            else
            {
                this.SetState(null);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string strMode;
            if (String.IsNullOrEmpty(this.Mode) == true)
            {
                strMode = this.Mode;
            }
            else
            {
                strMode = this.Mode;
            }

            PlaceHolder endinfoline = (PlaceHolder)this.FindControl("endinfoline");
            PlaceHolder edit = (PlaceHolder)this.FindControl("edit");

            if (strMode == "end")
            {
                endinfoline.Visible = true;
                edit.Visible = false;
            }
            else
            {
                endinfoline.Visible = false;
                edit.Visible = true;
            }

            LiteralControl infotext = (LiteralControl)this.FindControl("infotext");
            if (String.IsNullOrEmpty(this.BoxName) == true)
            {
                // text-level: 界面提示
                infotext.Text = this.GetString("新消息");
            }
            else
            {
                // text-level: 界面提示
                infotext.Text =
                    string.Format(this.GetString("来自s"),  // "来自 {0}"
                    this.MessageCenter.GetString(this.BoxName)  // 将信箱名类型转换为当前语言的信箱名字符串
                    );
                    // "来自 " + this.BoxName;
            }

            base.Render(writer);
        }


        void CreateCmdLine(Control parent)
        {

            parent.Controls.Add(new LiteralControl(
                "<tr class='cmdline'><td colspan='2'>"
            ));


            // 保存按钮
            Button savebutton = new Button();
            savebutton.ID = "save";
            savebutton.Text = this.GetString("保存");
            savebutton.CssClass = "savebutton";
            savebutton.Click += new EventHandler(savebutton_Click);
            parent.Controls.Add(savebutton);

            parent.Controls.Add(new LiteralControl(
                " "
            ));

            // 发送按钮
            Button sendbutton = new Button();
            sendbutton.ID = "send";
            sendbutton.Text = this.GetString("发送");
            sendbutton.CssClass = "sendbutton";
            sendbutton.Click += new EventHandler(sendbutton_Click);
            parent.Controls.Add(sendbutton);

            parent.Controls.Add(new LiteralControl(
    " "
));

            // 回复按钮
            Button replybutton = new Button();
            replybutton.ID = "reply";
            replybutton.Text = this.GetString("回复");
            replybutton.CssClass = "replybutton";
            replybutton.Click +=new EventHandler(replybutton_Click);
            parent.Controls.Add(replybutton);

            // 转发按钮
            Button forwardbutton = new Button();
            forwardbutton.ID = "forward";
            forwardbutton.Text = this.GetString("转发");
            forwardbutton.CssClass = "forwardbutton";
            forwardbutton.Click +=new EventHandler(forwardbutton_Click);
            parent.Controls.Add(forwardbutton);

            parent.Controls.Add(new LiteralControl(
    " "
));

            // 删除按钮
            Button deletebutton = new Button();
            deletebutton.ID = "delete";
            deletebutton.Text = this.GetString("删除");
            deletebutton.CssClass = "deletebutton";
            deletebutton.Click +=new EventHandler(deletebutton_Click);;
            parent.Controls.Add(deletebutton);

            parent.Controls.Add(new LiteralControl(
" "
));

            /*
            // 前一消息
            LinkButton prevbutton = new LinkButton();
            prevbutton.ID = "prev";
            prevbutton.Text = "前一消息";
            prevbutton.Click += new EventHandler(prevbutton_Click);
            parent.Controls.Add(prevbutton);

            parent.Controls.Add(new LiteralControl(
" "
));

            // 后一消息
            LinkButton nextbutton = new LinkButton();
            nextbutton.ID = "next";
            nextbutton.Text = "后一消息";
            nextbutton.Click += new EventHandler(nextbutton_Click);
            parent.Controls.Add(nextbutton);
             * */

            parent.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));

        }

        // 后一消息
        void nextbutton_Click(object sender, EventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // 前一消息
        void prevbutton_Click(object sender, EventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }



        // 每行前面加上>符号
        string MakeQuoteContent(string strContent)
        {
            StringReader sr= new StringReader(strContent);
            string strResult = "";

            for (; ; )
            {
                string strLine = sr.ReadLine();
                if (strLine == null)
                    break;
                strResult += "> " + strLine + "\r\n";
            }

            sr.Close();

            return strResult;
        }

        void CreateDebugLine(Control parent)
        {

            parent.Controls.Add(new LiteralControl(
                "<tr class='debugline'><td colspan='2'>"
            ));


            // 调试信息
            LiteralControl text = new LiteralControl();
            text.ID = "debuginfo";
            parent.Controls.Add(text);

            parent.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));
        }

        void SetDebugInfo(string strText)
        {
            LiteralControl text = (LiteralControl)this.FindControl("debuginfo");
            text.Text = strText;
        }

        void CreateEndInfo(Control parent)
        {

            parent.Controls.Add(new LiteralControl(
                "<tr class='endinfo'><td colspan='2'>"
            ));


            // 最后的显示信息
            LiteralControl text = new LiteralControl();
            text.ID = "endinfotext";
            parent.Controls.Add(text);

            text = new LiteralControl();
            text.Text = "   ";
            parent.Controls.Add(text);


            // 返回锚点
            HyperLink hyperlink = new HyperLink();
            hyperlink.ID = "backlink";
            hyperlink.Text = this.GetString("返回");
            hyperlink.NavigateUrl = "./mymessage.aspx";
            parent.Controls.Add(hyperlink);

            parent.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));
        }

        // parameters:
        //      strBackToBox    boxtype值
        void SetEndInfo(string strText,
            string strBackToBox)
        {
            LiteralControl text = (LiteralControl)this.FindControl("endinfotext");
            text.Text = strText;

            this.Mode = "end";

            // 制定要跳到哪个信箱
            if (String.IsNullOrEmpty(strBackToBox) == false)
            {
                HyperLink hyperlink = (HyperLink)this.FindControl("backlink");
                hyperlink.NavigateUrl = "./mymessage.aspx?box=" + HttpUtility.UrlEncode(strBackToBox);
            }

        }

        public void SetMessageData(MessageData data)
        {
            TextBox recipient = (TextBox)this.FindControl("recipient");
            TextBox sender = (TextBox)this.FindControl("sender");
            TextBox subject = (TextBox)this.FindControl("subject");
            TextBox content = (TextBox)this.FindControl("content");
            LiteralControl html_content = (LiteralControl)this.FindControl("html_content");
            PlaceHolder html_content_container = (PlaceHolder)this.FindControl("html_content_container");
            TextBox date = (TextBox)this.FindControl("date");

            recipient.Text = data.strRecipient;
            sender.Text = data.strSender;
            subject.Text = data.strSubject;
            if (data.strMime == "html")
            {
                html_content_container.Visible = true;
                content.Visible = false;

                html_content.Text = data.strBody;
            }
            else
            {
                html_content_container.Visible = false;
                content.Visible = true;

                content.Text = data.strBody;
            }
            date.Text = DateTimeUtil.LocalTime(data.strCreateTime);

            this.RecordID = data.strRecordID;
            this.TimeStamp = data.TimeStamp;
            this.BoxName = data.strBoxType;

            return;
        }

        // 设置按钮和编辑域状态
        public void SetState(string strOriginBox)
        {
            TextBox recipient = (TextBox)this.FindControl("recipient");
            TextBox sender = (TextBox)this.FindControl("sender");
            TextBox subject = (TextBox)this.FindControl("subject");
            TextBox content = (TextBox)this.FindControl("content");
            TextBox date = (TextBox)this.FindControl("date");

            Button savebutton = (Button)this.FindControl("save");
            Button sendbutton = (Button)this.FindControl("send");
            Button replybutton = (Button)this.FindControl("reply");
            Button forwardbutton = (Button)this.FindControl("forward");
            Button deletebutton = (Button)this.FindControl("delete");

            //  如果是窗口内新记录
            if (String.IsNullOrEmpty(strOriginBox) == true)
            {
                savebutton.Enabled = true;
                sendbutton.Enabled = true;
                replybutton.Enabled = false;
                forwardbutton.Enabled = false;
                deletebutton.Enabled = false;

                if (String.IsNullOrEmpty(sender.Text) == true)
                {
                    // 2006/11/25 neew add
                    sender.Text = this.UserID;
                }
                sender.ReadOnly = true;

                recipient.ReadOnly = false;
                subject.ReadOnly = false;
                content.ReadOnly = false;    // 让修改
                date.ReadOnly = true;
            }

            //  如果是来自收件箱的记录
            else if (MessageCenter.IsInBox(strOriginBox) == true)
            {
                savebutton.Enabled = false;
                sendbutton.Enabled = false;
                replybutton.Enabled = true;
                forwardbutton.Enabled = true;
                deletebutton.Enabled = true;

                sender.ReadOnly = true;
                recipient.ReadOnly = true;
                subject.ReadOnly = true;
                content.ReadOnly = true;    // 不让修改
                date.ReadOnly = true;
            }
            //  如果是来自草稿的记录
            else if (MessageCenter.IsTemp(strOriginBox) == true)
            {
                savebutton.Enabled = true;
                sendbutton.Enabled = true;
                replybutton.Enabled = false;
                forwardbutton.Enabled = false;
                deletebutton.Enabled = true;

                sender.Text = this.UserID;
                sender.ReadOnly = true;

                recipient.ReadOnly = false;
                subject.ReadOnly = false;

                content.ReadOnly = false;    // 让修改

                date.ReadOnly = true;
            }
            //  如果是来自 已发送 的记录
            else if (MessageCenter.IsOutbox(strOriginBox) == true)
            {
                savebutton.Enabled = false;
                sendbutton.Enabled = true;  // 再次发送?
                replybutton.Enabled = false;
                forwardbutton.Enabled = true;
                deletebutton.Enabled = true;

                // 全部不让修改
                sender.ReadOnly = true;
                recipient.ReadOnly = true;
                subject.ReadOnly = true;
                content.ReadOnly = true;    // 不让修改
                date.ReadOnly = true;
            }
            //  如果是来自 废件箱 的记录
            else if (MessageCenter.IsRecycleBin(strOriginBox) == true)
            {
                savebutton.Enabled = false;
                sendbutton.Enabled = false;  
                replybutton.Enabled = true;
                forwardbutton.Enabled = true;
                deletebutton.Enabled = true;

                // 全部不让修改
                sender.ReadOnly = true;
                recipient.ReadOnly = true;
                subject.ReadOnly = true;
                content.ReadOnly = true;    // 不让修改
                date.ReadOnly = true;
            }

            // 如果是废件箱内的消息, 彻底删除
            if (MessageCenter.IsRecycleBin(this.BoxName) == true)
            {
                deletebutton.Text = this.GetString("永久删除");
            }
            else
            {
                deletebutton.Text = this.GetString("移至废件箱");
            }

        }

        // 保存到草稿箱
        int SaveToTemp(out string strError)
        {
            strError = "";

            string strOldRecordID = "";
            byte[] baOldTimeStamp = null;

            //  如果是来自草稿的记录
            if (MessageCenter.IsTemp(this.BoxName) == true)
            {
                strOldRecordID = this.RecordID;
                baOldTimeStamp = this.TimeStamp;
            }

            TextBox recipient = (TextBox)this.FindControl("recipient");
            TextBox subject = (TextBox)this.FindControl("subject");
            TextBox sender = (TextBox)this.FindControl("sender");
            sender.Text = this.UserID;

            TextBox content = (TextBox)this.FindControl("content");

            byte[] baOutputTimeStamp = null;
            string strOutputID = "";

            int nRet = this.MessageCenter.SaveMessage(
                this.Channels,
                recipient.Text,
                sender.Text,
                subject.Text,
                "text",
                content.Text,
                strOldRecordID,   // string strOldRecordID,
                baOldTimeStamp,   // byte [] baOldTimeStamp,
                out baOutputTimeStamp,
                out strOutputID,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }

            // 为了便于再次修改后保存
            this.RecordID = strOutputID;
            this.TimeStamp = baOutputTimeStamp;

            return 0;
        }

        // 发送
        int Send(out string strError)
        {
            strError = "";

            TextBox recipient = (TextBox)this.FindControl("recipient");
            if (String.IsNullOrEmpty(recipient.Text) == true)
            {
                // text-level: 用户提示
                strError = this.GetString("尚未填写收件人");    // "尚未填写收件人"
                return -1;
            }

            TextBox subject = (TextBox)this.FindControl("subject");
            if (String.IsNullOrEmpty(subject.Text) == true)
            {
                // text-level: 用户提示
                strError = this.GetString("尚未填写主题");  // "尚未填写主题"
                return -1;
            }

            TextBox sender = (TextBox)this.FindControl("sender");
            sender.Text = this.UserID;

            TextBox content = (TextBox)this.FindControl("content");

            int nRet = this.MessageCenter.SendMessage(this.Channels,
                recipient.Text,
                sender.Text,
                subject.Text,
                "text",
                content.Text,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            // 如果来自草稿箱, 则需要从中永久删除
            if (MessageCenter.IsTemp(this.BoxName) == true)
            {
                nRet = this.MessageCenter.DeleteMessage(
                    false,
                    this.Channels,
                    this.RecordID,
                    this.TimeStamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                this.BoxName = null;    // 现在不属于任何信箱

            }

            return 0;
        }

        // 删除
        int Delete(
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(this.RecordID) == false)
            {
                // 如果是废件箱内的消息, 彻底删除
                if (MessageCenter.IsRecycleBin(this.BoxName) == true)
                {
                    nRet = this.MessageCenter.DeleteMessage(
                        false,
                        this.Channels,
                        this.RecordID,
                        this.TimeStamp,
                        out strError);
                }
                else
                {
                    // 否则移动到废件箱
                    nRet = this.MessageCenter.DeleteMessage(
                        true,
                        this.Channels,
                        this.RecordID,
                        this.TimeStamp,
                        out strError);
                }
                if (nRet == -1)
                    return -1;
                this.BoxName = null;    // 现在不属于任何信箱
            }

            return 0;
        }

        #region 按钮响应

        // 删除
        void deletebutton_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bDelete = false;
            // 如果是废件箱内的消息, 彻底删除
            if (MessageCenter.IsRecycleBin(this.BoxName) == true)
            {
                bDelete = true;
            }

            int nRet = Delete(out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                this.SetDebugInfo("删除消息失败: " + strError);
            }
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("消息删除成功"));  // "消息删除成功"
                if (bDelete == true)
                {
                    // 返回到当前所在信箱
                    this.SetEndInfo(this.GetString("消息删除成功"), null);
                }
                else
                {
                    // 返回到废件箱
                    this.SetEndInfo(this.GetString("消息已被移到废件箱"), // "消息已被移到废件箱。"
                        MessageCenter.TEMP
                        );
                }
            }
        }

        // 转发
        // 实际上是刷新窗口内容到适合转发的状态
        void forwardbutton_Click(object senderparam, EventArgs e)
        {
            TextBox recipient = (TextBox)this.FindControl("recipient");
            TextBox sender = (TextBox)this.FindControl("sender");
            TextBox subject = (TextBox)this.FindControl("subject");
            TextBox content = (TextBox)this.FindControl("content");
            TextBox date = (TextBox)this.FindControl("date");

            string strRecipient = "";
            string strSender = this.UserID;
            string strSubject = "转发: " + subject.Text;
            string strContent = ":\r\n你好!"
            + "\r\n\r\n\r\n\r\n> === 以下引用 " + sender.Text + " 于 " + date.Text + " 发送给 " + recipient.Text + " 的内容 ===\r\n" + MakeQuoteContent(content.Text) + "\r\n> ======";

            recipient.Text = strRecipient;
            sender.Text = strSender;
            subject.Text = strSubject;
            content.Text = strContent;

            date.Text = "";

            content.ReadOnly = false;    // 让修改

            this.MessageData = null;
            this.RecordID = null;
            this.TimeStamp = null;
            this.BoxName = null;
        }

        // 回复
        // 实际上是刷新窗口内容到适合回复的状态
        void replybutton_Click(object senderparam, EventArgs e)
        {
            TextBox recipient = (TextBox)this.FindControl("recipient");
            TextBox sender = (TextBox)this.FindControl("sender");
            TextBox subject = (TextBox)this.FindControl("subject");
            TextBox content = (TextBox)this.FindControl("content");
            TextBox date = (TextBox)this.FindControl("date");

            string strRecipient = sender.Text;
            string strSender = recipient.Text;
            string strSubject = "回复: " + subject.Text;
            string strContent = sender.Text + ":\r\n你好!"
            + "\r\n\r\n\r\n\r\n> === 以下引用 " + sender.Text + " 于 " + date.Text + " 发送给 " + recipient.Text + " 的内容 ===\r\n" + MakeQuoteContent(content.Text) + "\r\n> ======";

            recipient.Text = strRecipient;
            sender.Text = strSender;
            subject.Text = strSubject;
            content.Text = strContent;

            date.Text = "";

            content.ReadOnly = false;    // 让修改

            this.MessageData = null;
            this.RecordID = null;
            this.TimeStamp = null;
            this.BoxName = null;
        }

        // 发送
        void sendbutton_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = Send(out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                this.SetDebugInfo("发送消息失败: " + strError);
            }
            else
            {
                this.SetDebugInfo(this.GetString("消息发送成功"));  // "消息发送成功"
                this.SetEndInfo(this.GetString("消息发送成功"),
                    MessageCenter.OUTBOX
                    );
            }

        }

        // 保存
        void savebutton_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = SaveToTemp(out strError);
            if (nRet == -1)
            {
                // text-level: 用户提示
                this.SetDebugInfo(
                    string.Format(this.GetString("保存消息到s失败，原因s"),   // "保存消息到 {0} 失败，原因: {1}"
                    this.MessageCenter.GetString("草稿"),
                    strError)
                    /*
                    "保存消息到 "
                    + this.MessageCenter.GetString("草稿")
                    + " 失败: " + strError*/
                    );
            }
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("消息保存成功"));  // "消息保存成功"
                this.SetEndInfo(this.GetString("消息保存成功"),
                    MessageCenter.TEMP
                    );
            }
        }

        #endregion
    }
}
