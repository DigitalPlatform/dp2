using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 用于构造HTML页面中<link>局部
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:TreeControl runat=server></{0}:TreeControl>")]
    public class TreeControl : WebControl, INamingContainer
    {
        public bool EventMode = false;
        public event TreeItemClickEventHandler TreeItemClick = null;

        public event GetNodeDataEventHandler GetNodeData = null;

        public string XmlFileName = "";

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Ul;
            }
        }

#if NO
        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("id", this.Attributes["id"]);
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
        }
#endif
        public string SelectedNodePath
        {
            get
            {
                this.EnsureChildControls();
                HiddenField selected_node = (HiddenField)this.FindControl("selected-data");
                return selected_node.Value;
            }
            set
            {
                this.EnsureChildControls();
                HiddenField selected_node = (HiddenField)this.FindControl("selected-data");
                selected_node.Value = value;
            }
        }

        protected override void CreateChildControls()
        {
            HiddenField selected_node = new HiddenField();
            selected_node.ID = "selected-data";
            this.Controls.Add(selected_node);

            if (this.EventMode == true)
            {
                Button button = new Button();
                button.ID = "button";
                button.CssClass = "treebutton hidden";
                button.Click += new EventHandler(button_Click);
                this.Controls.Add(button);

                /*
                HiddenField value = new HiddenField();
                value.ID = "selected-data";
                this.Controls.Add(value);
                 * */
            }

            LiteralControl content = new LiteralControl();
            content.ID = "content";
            this.Controls.Add(content);
        }

        void button_Click(object sender, EventArgs e)
        {
            HiddenField value = (HiddenField)this.FindControl("selected-data");

            if (this.TreeItemClick != null)
            {
                TreeItemClickEventArgs e1 = new TreeItemClickEventArgs();
                e1.Url = value.Value;
                this.TreeItemClick(this, e1);
            }
        }

        HiddenField m_value = null;
        Button m_button = null;

        protected override void Render(HtmlTextWriter writer)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(this.XmlFileName) == true)
                goto END1;

            LiteralControl content = (LiteralControl)this.FindControl("content");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom = new XmlDocument();
                dom.Load(this.XmlFileName);
            }
            catch (Exception ex)
            {
                strError = "将文件 '" + this.XmlFileName + "' 装入 DOM 时出错：" + ex.Message;
                goto ERROR1;
            }


            // 2014/12/2
            // 兑现宏
            nRet = CacheBuilder.MacroDom(dom,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.EventMode == true)
            {
                m_value = (HiddenField)this.FindControl("selected-data");
                m_button = (Button)this.FindControl("button");
            }

            StringBuilder text = new StringBuilder(4096);
            BuildOneLevel(text, dom.DocumentElement);

            content.Text = text.ToString();
        END1:
            base.Render(writer);
            return;
        ERROR1:
            content.Text = "<span class='comment'>"
                + HttpUtility.HtmlEncode(strError)
                + "</span>";
            base.Render(writer);
        }

        void BuildOneLevel(StringBuilder text,
            XmlNode parent)
        {
            string strName = "";
            string strCount = "";
            string strUrl = "";
            bool bSelected = false;
            bool bClosed = true;
            if (this.GetNodeData != null)
            {
                GetNodeDataEventArgs e = new GetNodeDataEventArgs();
                e.Node = parent;
                this.GetNodeData(this, e);
                strName = e.Name;
                strCount = e.Count;
                strUrl = e.Url;
                bSelected = e.Seletected;
                bClosed = e.Closed;
            }
            else
            {
                strName = DomUtil.GetAttr(parent, "name");
                strUrl = DomUtil.GetAttr(parent, "url");
            }


            if (string.IsNullOrEmpty(strName) == false)
            {
                List<string> li_classes = new List<string>(); ;
                if (bClosed == true)
                    li_classes.Add("closed");
                string strLiClass = "";
                if (li_classes.Count > 0)
                    strLiClass = " class='" + StringUtil.MakePathList(li_classes, " ") + "' ";

                text.Append("<li" + strLiClass + ">");

                List<string> text_classes = new List<string>();
                if (bSelected == true)
                    text_classes.Add("selected");

                {
                    // text_classes.Add("name");

                    string strTextClass = "";
                    if (text_classes.Count > 0)
                        strTextClass = " class='" + StringUtil.MakePathList(text_classes, " ") + "' ";

                    if (this.EventMode == true)
                    {
                        string strValueID = this.m_value.ClientID;
                        string strButtonID = this.m_button.ClientID;
                        // text.Append("<a " + strTextClass + " data-url='"+HttpUtility.HtmlAttributeEncode(strUrl)+"' href='#' onclick='javascript:$(\"input[id="+strValueID+"]\").val($(this).data(\"url\"));$(\"#"+strButtonID+"\").trigger(\"click\");'>" + HttpUtility.HtmlEncode(strName) + "</a>");
                        text.Append("<a " + strTextClass + " data-url='" + HttpUtility.HtmlAttributeEncode(strUrl) + "' href='#' onclick='$(\"#" + strValueID + "\").val($(this).data(\"url\"));$(\"#" + strButtonID + "\").trigger(\"click\");'>" + HttpUtility.HtmlEncode(strName) + "</a>");
                        // text.Append("<a " + strTextClass + " data-name='" + HttpUtility.HtmlAttributeEncode("test name") + "' href='#' onclick='javascript:OnTreeClick(this);'>" + HttpUtility.HtmlEncode(strName) + "</a>");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(strUrl) == false)
                            text.Append("<a " + strTextClass + " href='" + strUrl + "'>" + HttpUtility.HtmlEncode(strName) + "</a>");
                        else
                            text.Append("<span " + strTextClass + ">" + HttpUtility.HtmlEncode(strName) + "</span>");
                    }

                    /*
                    if (string.IsNullOrEmpty(strCount) == false)
                        text.Append("<span class='count'>"+HttpUtility.HtmlEncode(strCount)+"</span>");
                     * */
                }

                if (string.IsNullOrEmpty(strCount) == false)
                {
                    text_classes.Remove("name");
                    text_classes.Add("count");

                    string strTextClass = "";
                    if (text_classes.Count > 0)
                        strTextClass = " class='" + StringUtil.MakePathList(text_classes, " ") + "' ";

                    text.Append("<span " + strTextClass + ">" + HttpUtility.HtmlEncode(strCount) + "</span>");
                }

            }

            StringBuilder inner_text = new StringBuilder(4095);
            foreach (XmlNode node in parent.ChildNodes)
            {
                BuildOneLevel(inner_text, node);
            }

            if (inner_text.Length > 0)
            {
                text.Append("<ul>");
                text.Append(inner_text);
                text.Append("</ul>");
            }

            if (string.IsNullOrEmpty(strName) == false)
            {
                text.Append("</li>");
            }
        }
    }

    public delegate void GetNodeDataEventHandler(object sender,
        GetNodeDataEventArgs e);

    public class GetNodeDataEventArgs : EventArgs
    {
        public XmlNode Node = null; // [in]

        public string Name = "";    // [out]
        public string Count = "";   // [out]
        public string Url = ""; // [out]
        public bool Seletected = false; // [out]
        public bool Closed = true;  // [out]
    }

    ///
    public delegate void TreeItemClickEventHandler(object sender,
    TreeItemClickEventArgs e);

    public class TreeItemClickEventArgs : EventArgs
    {
        public string Url = "";
    }
}