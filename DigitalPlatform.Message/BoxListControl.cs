using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;
using System.Resources;
using System.Globalization;

namespace DigitalPlatform.Message
{
    /// <summary>
    /// 列出所有信箱的Web控件
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BoxListControl runat=server></{0}:BoxListControl>")]
    public class BoxListControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.Message.res.BoxListControl.cs",
                typeof(BoxListControl).Module.Assembly);

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

        public MessageCenter MessageCenter = null;

        public string GetPrefixString(string strTitle)
        {
            return "<div class='content_wrapper'>"
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

        protected override void CreateChildControls()
        {
            if (this.MessageCenter == null)
                return;

            LiteralControl literal = new LiteralControl();
            literal.Text = GetPrefixString(this.GetString("信箱"));
            literal.Text += "<table class='boxes'><tr class='content'>";    // width='100%' cellspacing='1' cellpadding='4'
            this.Controls.Add(literal);

            LinkButton linkbutton = null;

            for (int i = 0; i < this.MessageCenter.Boxes.Count; i++)
            {
                literal = new LiteralControl();
                literal.Text = "<td class='content'>";
                this.Controls.Add(literal);


                Box box= this.MessageCenter.Boxes[i];

                linkbutton = new LinkButton();
                linkbutton.ID = box.Name;
                linkbutton.Text = box.Name;
                linkbutton.Click += new EventHandler(linkbutton_Click);
                this.Controls.Add(linkbutton);

                literal = new LiteralControl();
                literal.Text = "</td>";
                this.Controls.Add(literal);
            }

            literal = new LiteralControl();
            literal.Text = "<td class='newmessage'>";
            this.Controls.Add(literal);

            linkbutton = null;
            linkbutton = new LinkButton();
            linkbutton.ID = "newmessage";
            linkbutton.Text = this.GetString("撰写消息");
            linkbutton.CssClass = "newmessage";
            linkbutton.Click += new EventHandler(newmessage_Click);
            this.Controls.Add(linkbutton);

            literal = new LiteralControl();
            literal.Text = "</td>";
            this.Controls.Add(literal);



            literal = new LiteralControl();
            literal.Text = "</tr></table>" + GetPostfixString();
            this.Controls.Add(literal);
        }

        void linkbutton_Click(object sender, EventArgs e)
        {
            LinkButton button = (LinkButton)sender;
            string strBoxName = button.Text;

            // TODO: 需要将按钮上的文字名替换为boxtype值
            string strBoxType = this.MessageCenter.GetBoxType(strBoxName);
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

    }
}
