using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;

using DigitalPlatform.OPAC.Server;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 浏览一个信箱中邮件的Web控件
    /// </summary>
    [ToolboxData("<{0}:MessageListControl runat=server></{0}:MessageListControl>")]
    public class MessageListControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.MessageListControl.cs",
                typeof(MessageListControl).Module.Assembly);

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

        //        public string BoxName = "";
        public string UserID = "";

        public string Lang = "";

        #region 属性

        // 显示起始位置
        public int StartIndex
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageList_StartIndex"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_StartIndex"] = (object)value;
            }
        }

        // 本页最大显示行数
        public int PageMaxLines
        {
            get
            {

                object o = this.Page.Session[this.ID + "MessageList_PageMaxLines"];
                return (o == null) ? 10 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_PageMaxLines"] = (object)value;
            }
        }


        // 结果集中记录数
        public int ResultCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageList_ResultCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_ResultCount"] = (object)value;
            }
        }

        // 浏览内容行数
        public int LineCount
        {
            get
            {

                object o = this.Page.Session[this.ID + "MessageList_LineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_LineCount"] = (object)value;
            }
        }

        // 检索命中的结果集名
        public string ResultSetName
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageList_ResultSetName"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_ResultSetName"] = (object)value;
            }
        }

        // 当前信箱(类型)名
        public string CurrentBoxType
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageList_CurrentBox"];
                return (o == null) ? String.Empty : (string)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_CurrentBox"] = (object)value;
            }
        }

        /*
        // 当前信箱未读消息数
        public int CurrentBoxUntouchedCount
        {
            get
            {

                object o = this.Page.Session[this.ID + "MessageList_CurrentBoxUntouched"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_CurrentBoxUntouched"] = (object)value;
            }
        }

        // 信箱未读数列表
        public Hashtable UntouchedCountList
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageList_UntouchedList"];
                return (o == null) ? new Hashtable() : (Hashtable)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageList_UntouchedList"] = (object)value;
            }
        }
        */

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

        // ID列表
        public List<string> ItemIDs
        {
            get
            {
                object o = this.Page.Session[this.ID + "MessageControl_ItemIDs"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session[this.ID + "MessageControl_ItemIDs"] = (object)value;
            }
        }

        #endregion

        #region 创建子控件

        protected override void CreateChildControls()
        {
            // 总表格
            this.Controls.Add(new LiteralControl(
    "<table class='messagelist_total'>" //  width='100%' cellspacing='1' cellpadding='4'
    ));
            this.Controls.Add(new LiteralControl(
                "<tr class='messagelist_total'><td class='left' valign='top'>"  // width='1%' 
            ));

            // 左边信箱列表
            PlaceHolder boxlist = new PlaceHolder();
            boxlist.ID = "boxlist";
            this.Controls.Add(boxlist);

            CreateBoxListControls(boxlist);

            this.Controls.Add(new LiteralControl(
                "</td><td class='middle' valign='top'></td><td class='right' valign='top'>"  // width='99%'
            ));


            this.Controls.Add(new LiteralControl("<div class='" + "messagelist_wrapper" + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>"));

            // 标题文字
            LiteralControl messagelist_titletext = new LiteralControl();
            messagelist_titletext.ID = "messagelist_titletext";
            this.Controls.Add(messagelist_titletext);


            this.Controls.Add(new LiteralControl("</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>"));

            this.Controls.Add(new LiteralControl(
                "<table class='messagelist'>"   //  width='100%' cellspacing='1' cellpadding='4'
                ));

            // 信息行
            this.Controls.Add(new LiteralControl(
                "<tr class='info'><td colspan='6'>"
            ));

            // 信息文字
            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "info";
            this.Controls.Add(resultinfo);

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));

            // 标题行
            this.Controls.Add(new LiteralControl(
                "<tr class='columntitle'>"
                // + "<td width='1%' nowrap>序号</td><td class='sender' width='1%' nowrap>发件人</td><td class='recipient' width='10%' nowrap>收件人</td><td class='subject' width='50%' nowrap>主题</td><td class='date' width='13%' nowrap>日期</td><td class='size' width='7%' nowrap>尺寸</td></tr>"
                + "<td class='index'>"
                + this.GetString("序号")
                + "</td><td class='sender'>"
                + this.GetString("发件人")
                + "</td><td class='recipient'>"
                + this.GetString("收件人")
                + "</td><td class='subject'>"
                + this.GetString("主题")
                + "</td><td class='date'>"
                + this.GetString("日期")
                + "</td><td class='size'>"
                + this.GetString("尺寸")
                + "</td></tr>"
            ));


            // 内容代表
            PlaceHolder content = new PlaceHolder();
            content.ID = "content";
            this.Controls.Add(content);


            // 内容行
            for (int i = 0; i < this.LineCount; i++)
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
            CreateDebugLine(this);

            this.Controls.Add(new LiteralControl(
               "</table>" + this.GetPostfixString()
               ));

            // 总表格结束
            this.Controls.Add(new LiteralControl(
                "</td></tr></table>"
            ));
        }

        void CreateDebugLine(Control parent)
        {

            parent.Controls.Add(new LiteralControl(
                "<tr class='debugline'><td colspan='6'>"
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
            this.EnsureChildControls();

            LiteralControl text = (LiteralControl)this.FindControl("debuginfo");
            text.Text = strText;
        }

        void SetDebugInfo(string strSpanClass,
            string strText)
        {
            this.EnsureChildControls();

            LiteralControl text = (LiteralControl)this.FindControl("debuginfo");
            if (strSpanClass == "errorinfo")
                text.Text = "<div class='errorinfo-frame'><div class='" + strSpanClass + "'>" + strText + "</div></div>";
            else
                text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }

#if NO
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
#endif

        void CreateBoxListControls(PlaceHolder boxlist)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            bool bDetectFullCount = true;   // 是否首次就探测所有信箱的未读数字

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetPrefixString(
                this.GetString("信箱"),
                "boxes_wrapper");
            literal.Text += "<table class='boxes'>"; //  width='100%' cellspacing='1' cellpadding='4'
            boxlist.Controls.Add(literal);

            LinkButton linkbutton = null;

            int nInboxUntouchedCount = 0;   // 收件箱中的未读信件数目

            for (int i = 0; i < app.BoxesInfo.Boxes.Count; i++)
            {
                Box box = app.BoxesInfo.Boxes[i];

                string strClass = "box";

                if (/*box.Name == this.CurrentBoxType*/
                    box.Type == this.CurrentBoxType)
                    strClass = "box active";

                literal = new LiteralControl();
                literal.Text = "<tr class='" + strClass + "'><td class='" + strClass + "' nowrap>";
                boxlist.Controls.Add(literal);

                int nCount = 0;

                if (bDetectFullCount == true)
                {
                    nCount = sessioninfo.Channel.GetUntouchedMessageCount(
                        box.Name);
                    if (nCount != -1)
                    {
                        // untouchedcountlist[box.Name] = (object)nCount;

                    }

                    if (box.Type == BoxesInfo.INBOX)
                        nInboxUntouchedCount = nCount;
                }


                linkbutton = new LinkButton();
                linkbutton.ID = box.Name;
                string strCaption = app.BoxesInfo.GetString(box.Type);   // 获得信箱对于当前语言的名字 2009/7/14 changed
                if (nCount != 0)
                    linkbutton.Text = strCaption + "(" + Convert.ToString(nCount) + ")";
                else
                    linkbutton.Text = strCaption;

                linkbutton.Click += new EventHandler(linkbutton_Click);
                boxlist.Controls.Add(linkbutton);

                literal = new LiteralControl();
                literal.Text = "</td></tr>";
                boxlist.Controls.Add(literal);
            }

            // this.UntouchedCountList = untouchedcountlist;   // 更新内容


            literal = new LiteralControl();
            literal.Text = "<tr class='cmd'><td class='newmessage'>";
            boxlist.Controls.Add(literal);

            Button newmessagebutton = new Button();
            newmessagebutton.ID = "newmessage";
            newmessagebutton.Text = this.GetString("撰写消息");
            newmessagebutton.CssClass = "newmessage";
            newmessagebutton.Click += new EventHandler(newmessage_Click);
            boxlist.Controls.Add(newmessagebutton);

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            boxlist.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</table>" + this.GetPostfixString();
            boxlist.Controls.Add(literal);
        }


        void linkbutton_Click(object sender, EventArgs e)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            LinkButton button = (LinkButton)sender;
            string strBoxName = button.ID;

            // TODO: 需要将按钮上的文字名替换为boxtype值
            string strBoxType = app.BoxesInfo.GetBoxType(strBoxName);
            if (String.IsNullOrEmpty(strBoxType) == false)
            {
                this.Page.Response.Redirect("./mymessage.aspx?box=" + HttpUtility.UrlEncode(strBoxType));
                this.Page.Response.End();
            }
            else
            {
                this.Page.Response.Write("信箱名 '" + strBoxName + "' 无法转换为信箱类型字符串");
                this.Page.Response.End();
            }

        }

        void newmessage_Click(object sender, EventArgs e)
        {
            this.Page.Response.Redirect("./message.aspx?box=" + HttpUtility.UrlEncode("草稿"));
            this.Page.Response.End();
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

            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_classname";
            literal.Text = "content";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "'><td class='no'>"; // width='1%'
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "line" + Convert.ToString(nLineNo) + "_checkbox";
            line.Controls.Add(checkbox);

            // 序号
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_no";
            line.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td><td class='sender'>"; //  width='10%'
            line.Controls.Add(literal);

            // 发件人
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_sender";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</td><td class='recipient'>"; // width='10%'
            line.Controls.Add(literal);

            // 收件人
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_recipient";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</td><td class='subject'>"; // width='50%'
            line.Controls.Add(literal);

            // 主题
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_subject";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</td><td class='date'>"; // width='13%'
            line.Controls.Add(literal);

            // 日期
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_date";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</td><td class='size'>"; // width='7%'
            line.Controls.Add(literal);

            // 尺寸
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_size";
            line.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        void CreateCmdLine()
        {

            this.Controls.Add(new LiteralControl(
                "<tr class='cmdline'><td colspan='6'>"
            ));

            this.Controls.Add(new LiteralControl(
                "<table border='0' width='100%'><tr><td>"
            ));

            // 删除选定按钮
            Button deletebutton = new Button();
            deletebutton.ID = "delete";
            deletebutton.Text = this.GetString("删除选定的事项");
            deletebutton.CssClass = "delete";
            deletebutton.Click += new EventHandler(deletebutton_Click);
            this.Controls.Add(deletebutton);

            this.Controls.Add(new LiteralControl(
                "</td><td>"
            ));

            // 全部删除按钮 2007/7/15
            Button deleteallbutton = new Button();
            deleteallbutton.ID = "deleteall";
            deleteallbutton.Text = this.GetString("全部删除");
            deleteallbutton.CssClass = "deleteall";
            deleteallbutton.Click += new EventHandler(deleteallbutton_Click);
            this.Controls.Add(deleteallbutton);

            this.Controls.Add(new LiteralControl(
                "</td><td align='right'> "
            ));

            PlaceHolder pageswitcher = new PlaceHolder();
            pageswitcher.ID = "pageswitcher";
            this.Controls.Add(pageswitcher);


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

            LiteralControl literal = new LiteralControl();
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
            lastpage.Text = this.GetString("末页");
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
            literal.Text = " 页";
            pageswitcher.Controls.Add(literal);
             * */

            literal = new LiteralControl();
            literal.ID = "maxpagecount";
            literal.Text = " " + string.Format(this.GetString("maxpagecount"), this.PageCount.ToString());    // (共 {0} 页)
            pageswitcher.Controls.Add(literal);

            this.Controls.Add(new LiteralControl(
                "</td></tr></table>"
            ));

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));
        }

        // 删除全部的消息
        void deleteallbutton_Click(object sender, EventArgs e)
        {
            string strError = "";
            long nRet = 0;

            bool bMoveToRecycleBin = true;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (BoxesInfo.IsRecycleBin(this.CurrentBoxType) == true)
                bMoveToRecycleBin = false;
            else
                bMoveToRecycleBin = true;

            string strStyle = this.CurrentBoxType;
            if (bMoveToRecycleBin == true)
                strStyle += ",movetorecyclebin";


            MessageData[] output_messages = null;
            nRet = sessioninfo.Channel.SetMessage("deleteall",
                strStyle,
                null,
                out output_messages,
                out strError);
            if (nRet == -1)
            {
                this.SetDebugInfo("errorinfo", strError);
                return;
            }

            if (bMoveToRecycleBin == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo(
                    string.Format(this.GetString("已将s个消息移动到废件箱"),    // "已将 {0} 个消息移动到废件箱。"
                    nRet.ToString()));
                // "已将 " + nStart.ToString() + " 个消息移动到废件箱。"

            }
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(
                    string.Format(this.GetString("已将s个消息永久删除"),    // "已将 {0} 个消息永久删除。"
                    nRet.ToString()));

                // "已将 " + nStart.ToString() + " 个消息永久删除。"
            }

            this.RefreshList(); // 刷新当前结果集显示
        }

        // 删除选择的消息
        void deletebutton_Click(object sender, EventArgs e)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            List<string> ids = new List<string>();

            for (int i = 0; i < this.LineCount; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.ItemIDs.Count <= i)
                    {
                        // text-level: 内部错误
                        this.SetDebugInfo("errorinfo", "ItemIDs失效...");
                        return;
                    }
                    ids.Add(this.ItemIDs[i]);
                    checkbox.Checked = false;
                }
            }

            if (ids.Count == 0)
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("尚未选择任何消息"));
                return;
            }

            bool bMoveToRecycleBin = true;

            if (BoxesInfo.IsRecycleBin(this.CurrentBoxType) == true)
                bMoveToRecycleBin = false;
            else
                bMoveToRecycleBin = true;

            MessageData[] messages = new MessageData[ids.Count];
            for (int i = 0; i < ids.Count; i++)
            {
                messages[i] = new MessageData();
                messages[i].strRecordID = ids[i];
            }

            MessageData[] output_messages = null;
            string strError = "";
            long nRet = sessioninfo.Channel.SetMessage("delete",
                bMoveToRecycleBin == true ? "movetorecyclebin" : "",
                messages,
                out output_messages,
                out strError);
            if (nRet == -1)
                this.SetDebugInfo("errorinfo", strError);
            else
            {
                if (bMoveToRecycleBin == true)
                {
                    // text-level: 用户提示
                    this.SetDebugInfo(
                                            string.Format(this.GetString("已将s个消息移动到废件箱"),    // "已将 {0} 个消息移动到废件箱。"
                                            ids.Count.ToString()));

                    // "已将 " + ids.Count + " 个消息移动到废件箱。");
                }
                else
                {
                    // text-level: 用户提示
                    this.SetDebugInfo(
                                            string.Format(this.GetString("已将s个消息永久删除"),    // "已将 {0} 个消息永久删除。"
                                            ids.Count.ToString()));

                    // "已将 " + ids.Count + " 个消息永久删除。");
                }

                this.RefreshList(); // 刷新当前结果集显示
            }
        }

        // 重新检索结果集
        public void RefreshList()
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strError = "";
            int nTotalCount = 0;
            MessageData[] messages = null;
            if (String.IsNullOrEmpty(this.UserID) == true)
            {
                // text-level: 内部错误
                throw new Exception("UserID为空");
            }

            // 重新发起检索，但暂不获取
            long nRet = sessioninfo.Channel.ListMessage(
                "search",   // true,
                this.ResultSetName,
                this.CurrentBoxType,
                MessageLevel.Summary,
                0,
                0,
                out nTotalCount,
                out messages,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                this.SetDebugInfo("errorinfo", "刷新时检索失败: " + strError);
            }
            else
            {
                this.ResultCount = nTotalCount;
            }
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

        #endregion


        protected override void Render(HtmlTextWriter writer)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            long nRet = 0;

            int nPageNo = this.StartIndex / this.PageMaxLines;

            if (nPageNo >= this.PageCount)  // 如果超过最后一页
                lastpage_Click(null, null);

            SetResultInfo();

            string strError = "";

            List<string> tempids = new List<string>();

            if (this.ResultCount != 0)
            {

                int nTotalCount = 0;
                MessageData[] messages = null;
                nRet = sessioninfo.Channel.ListMessage(
                    "", // false,
                    this.ResultSetName,
                    this.CurrentBoxType,
                    MessageLevel.Summary,
                    this.StartIndex,
                    this.PageMaxLines,
                    out nTotalCount,
                    out messages,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }

                // 显示本页中的浏览行
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    MessageData data = null;
                    if (i < messages.Length)
                        data = messages[i];

                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                    {
                        PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                        PlaceHolder content = (PlaceHolder)this.FindControl("content");

                        line = this.NewContentLine(content, i, insertpoint);
                    }

                    LiteralControl no = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_no");
                    CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                    LiteralControl sender = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_sender");
                    LiteralControl recipient = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_recipient");
                    LiteralControl subject = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_subject");
                    LiteralControl date = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_date");
                    LiteralControl size = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_size");
                    LiteralControl classname = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_classname");


                    if (data == null)
                    {
                        checkbox.Visible = false;
                        subject.Text = "&nbsp;";
                        continue;
                    }

                    checkbox.Visible = true;

                    tempids.Add(data.strRecordID);

                    // 序号
                    string strNo = "&nbsp;";
                    strNo = Convert.ToString(i + this.StartIndex + 1);

                    no.Text = strNo;


                    string strDetailUrl = "./message.aspx?id=" + data.strRecordID;
                    if (data.strSubject == "")
                        data.strSubject = this.GetString("无");   // "(无)"

                    sender.Text = data.strSender;
                    recipient.Text = data.strRecipient;
                    subject.Text = "<a href='" + strDetailUrl + "'>" + data.strSubject + "</a>";
                    date.Text = DateTimeUtil.LocalTime(data.strCreateTime);
                    size.Text = data.strSize;

                    if (data.Touched == true)
                        classname.Text = "content";
                    else
                        classname.Text = "content new";

                } // end of for

                this.LineCount = Math.Max(this.LineCount, this.PageMaxLines);

            }
            else
            {
                // 显示空行
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                    {
                        PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                        PlaceHolder content = (PlaceHolder)this.FindControl("content");

                        line = this.NewContentLine(content, i, insertpoint);
                    }

                    line.Visible = true;

                    CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "_checkbox");
                    checkbox.Visible = false;

                    LiteralControl subject = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_subject");
                    subject.Text = "&nbsp;";

                }

            }

            this.ItemIDs = tempids;

            // 设置删除按钮文字
            Button deletebutton = (Button)this.FindControl("delete");

            if (BoxesInfo.IsRecycleBin(this.CurrentBoxType) == true)
                deletebutton.Text = this.GetString("永久删除选定的消息");
            else
                deletebutton.Text = this.GetString("将选定的消息移至废件箱");

            // 设置删除全部按钮文字
            Button deleteallbutton = (Button)this.FindControl("deleteall");

            if (BoxesInfo.IsRecycleBin(this.CurrentBoxType) == true)
                deleteallbutton.Text = this.GetString("永久删除全部消息");
            else
                deleteallbutton.Text = this.GetString("将全部消息移至废件箱");


            base.Render(writer);
        }

        // 设置结果集有关数量参数
        public void SetResultInfo()
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            LiteralControl messagelist_titletext = (LiteralControl)this.FindControl("messagelist_titletext");
            messagelist_titletext.Text = app.BoxesInfo.GetString(this.CurrentBoxType);

            int nPageNo = this.StartIndex / this.PageMaxLines;

            LiteralControl resultinfo = (LiteralControl)this.FindControl("info");
            if (this.ResultCount != 0)
            {
                // text-level: 界面提示
                resultinfo.Text =
                    string.Format(this.GetString("s内共有消息s条, 分s页显示, 当前为第s页"),
                    // "{0} 内共有消息 {1} 条, 分 {2} 页显示, 当前为第 {3} 页。"
                    app.BoxesInfo.GetString(this.CurrentBoxType),
                    Convert.ToString(this.ResultCount),
                    Convert.ToString(this.PageCount),
                    Convert.ToString(nPageNo + 1));

                // this.BoxesInfo.GetString(this.CurrentBoxType) + " 内共有消息 " + Convert.ToString(this.ResultCount) + " 条, 分 " + Convert.ToString(this.PageCount) + " 页显示, 当前为第 " + Convert.ToString(nPageNo + 1) + "页。";
            }
            else
            {
                // text-level: 界面提示
                resultinfo.Text =
                    string.Format(this.GetString("s为空"),  // "('{0}' 为空)"
                    app.BoxesInfo.GetString(this.CurrentBoxType));
                /*
                "('"
                + this.BoxesInfo.GetString(this.CurrentBoxType)
                + "' 为空)";
                 * */
            }

            LiteralControl maxpagecount = (LiteralControl)this.FindControl("maxpagecount");
            maxpagecount.Text =
                string.Format(this.GetString("maxpagecount"),   // (共 {0} 页)
                Convert.ToString(this.PageCount));
            // " (共 " + Convert.ToString(this.PageCount) + " 页)";

            LiteralControl currentpageno = (LiteralControl)this.FindControl("currentpageno");
            currentpageno.Text = Convert.ToString(nPageNo + 1);

            PlaceHolder pageswitcher = (PlaceHolder)this.FindControl("pageswitcher");
            if (this.PageCount <= 1)
                pageswitcher.Visible = false;
            else
                pageswitcher.Visible = true;
        }

#if NO
        // 获得一行信息
        int GetLineInfo(
            RmsChannel channel,
            string strPath,
            out string strSender,
            out string strRecipient,
            out string strSubject,
            out string strDate,
            out string strSize,
            out bool bTouched,
            out string strError)
        {
            strSender = "";
            strRecipient = "";
            strSubject = "";
            strDate = "";
            strSize = "";
            bTouched = false;
            strError = "";


            // 将种记录数据从XML格式转换为HTML格式
            string strMetaData = "";
            byte[] timestamp = null;
            string strXml = "";
            string strOutputPath = "";
            long lRet = channel.GetRes(strPath,
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // text-level: 内部错误
                strError = "获得消息记录 '" + strPath + "' 时出错: " + strError;
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                // text-level: 内部错误
                strError = "装载XML记录进入DOM时出错: " + ex.Message;
                return -1;
            }

            strSender = DomUtil.GetElementText(dom.DocumentElement,
                "sender");
            strRecipient = DomUtil.GetElementText(dom.DocumentElement,
                "recipient");
            strSubject = DomUtil.GetElementText(dom.DocumentElement,
                "subject");
            strDate = DomUtil.GetElementText(dom.DocumentElement,
                "date");
            strDate = DateTimeUtil.LocalTime(strDate);

            strSize = DomUtil.GetElementText(dom.DocumentElement,
                "size");
            string strTouched = DomUtil.GetElementText(dom.DocumentElement,
                "touched");
            if (strTouched == "1")
                bTouched = true;
            else
                bTouched = false;

            return 0;
        }

#endif

        // TODO: 检查boxtype
        // 装入一个信箱的信息
        public int LoadBox(
            string strUserID,
            string strBoxType,
            out string strError)
        {
            strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            // 首先默认控件的当前信箱
            if (String.IsNullOrEmpty(strBoxType) == true)
                strBoxType = this.CurrentBoxType;

            // 若还是空, 则默认INBOX
            if (String.IsNullOrEmpty(strBoxType) == true)
                strBoxType = BoxesInfo.INBOX;

            string strResultSetName = "messagelist_" + strBoxType;

            int nTotalCount = 0;
            MessageData[] messages = null;
            long nRet = sessioninfo.Channel.ListMessage(
                "search", // true,
                strResultSetName,
                strBoxType,
                MessageLevel.Summary,
                0,
                0,
                out nTotalCount,
                out messages,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }

            this.ResultSetName = strResultSetName;
            this.ResultCount = nTotalCount;
            this.CurrentBoxType = strBoxType;

            return 0;
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

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
    }
}
