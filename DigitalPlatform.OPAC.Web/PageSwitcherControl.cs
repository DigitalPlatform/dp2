using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:PageSwitcherControl runat=server></{0}:PageSwitcherControl>")]
    public class PageSwitcherControl : WebControl, INamingContainer
    {
        public bool EventMode = true;   // 是否为事件模式.true 事件模式 / false URL模式

#if NO
        // 基本的URL。本控件在此基础上增加 参数，形成跳转的URL
        string m_strBaseUrl = "";
        public string BaseUrl
        {
            get
            {
                return this.m_strBaseUrl;
            }
            set
            {
                this.m_strBaseUrl = value;
            }
        }
#endif

        public string PageNoParaName = "pageno";

        public event GetBaseUrlEventHandler GetBaseUrl = null;
        public event PageSwitchEventHandler PageSwitch = null;

        public int TotalCount = 0;
        public int CurrentPageNo = 0;

        public bool Wrapper = true;

        public string PageNoList
        {
            get
            {
                if (this.EventMode == false)
                    throw new Exception("URL模式下不能使用 PageNoList 属性");

                this.EnsureChildControls();
                HiddenField pagenolist = (HiddenField)this.FindControl("pagenolist");
                return pagenolist.Value;
            }

            set
            {
                if (this.EventMode == false)
                    throw new Exception("URL模式下不能使用 PageNoList 属性");

                this.EnsureChildControls();
                HiddenField pagenolist = (HiddenField)this.FindControl("pagenolist");
                pagenolist.Value = value;
            }
        }

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        protected override void CreateChildControls()
        {
            LiteralControl literal = null;
            if (this.Wrapper == true)
            {
                literal = new LiteralControl("<div class='pager'>");
                this.Controls.Add(literal);
            }

            if (this.EventMode == true)
            {
                HiddenField pagenolist = new HiddenField();
                pagenolist.ID = "pagenolist";
                this.Controls.Add(pagenolist);

                ButtonBegin("first");
                LinkButton link = new LinkButton();
                link.ID = "first";
                link.Text = "1...";
                link.CssClass = "first pager";
                link.Click += new EventHandler(link_Click);
                this.Controls.Add(link);
                ButtonEnd();

                ButtonBegin("prev");
                link = new LinkButton();
                link.ID = "prev";
                link.Text = "<";
                link.CssClass = "prev pager";
                this.Controls.Add(link);
                link.Click += new EventHandler(link_Click);
                ButtonEnd();


                for (int i = 0; i < 11; i++)
                {
                    ButtonBegin("goto");
                    link = new LinkButton();
                    link.ID = "goto_" + i.ToString();
                    link.Text = i.ToString();
                    link.CssClass = "goto pager";
                    this.Controls.Add(link);
                    link.Click += new EventHandler(link_Click);
                    ButtonEnd();
                }

                ButtonBegin("next");
                link = new LinkButton();
                link.ID = "next";
                link.Text = ">";
                link.CssClass = "next pager";
                this.Controls.Add(link);
                link.Click += new EventHandler(link_Click);
                ButtonEnd();
            }
            else
            {
                ButtonBegin("first");
                HyperLink link = new HyperLink();
                link.ID = "first";
                link.Text = "1...";
                link.CssClass = "first pager";
                this.Controls.Add(link);
                ButtonEnd();

                ButtonBegin("prev");
                link = new HyperLink();
                link.ID = "prev";
                link.Text = "<";
                link.CssClass = "prev pager";
                this.Controls.Add(link);
                ButtonEnd();


                for (int i = 0; i < 11; i++)
                {
                    ButtonBegin("goto");
                    link = new HyperLink();
                    link.ID = "goto_" + i.ToString();
                    link.Text = i.ToString();
                    link.CssClass = "goto pager";
                    this.Controls.Add(link);
                    ButtonEnd();
                }

                ButtonBegin("next");
                link = new HyperLink();
                link.ID = "next";
                link.Text = ">";
                link.CssClass = "next pager";
                this.Controls.Add(link);
                ButtonEnd();
            }

            if (this.Wrapper == true)
            {
                literal = new LiteralControl("</div/>");
                this.Controls.Add(literal);
            }
        }

        void link_Click(object sender, EventArgs e)
        {
            PageSwitchEventArgs e1 = new PageSwitchEventArgs();

            string[] numbers = this.PageNoList.Split(new char[] { ',' });

            LinkButton link = (LinkButton)sender;
            if (link.ID == "first")
                e1.GotoPageNo = 0;
            else if (link.ID == "prev")
                e1.GotoPageNo = Math.Max(Convert.ToInt32(numbers[0]), 0);
            else if (link.ID == "next")
                e1.GotoPageNo = Math.Max(Convert.ToInt32(numbers[1]), 0);
            else
            {
                int index = Convert.ToInt32(link.Text);
                // 这是CreateChildControls时的数字，从0计数
                if (index >= numbers.Length)
                {

                }

                e1.GotoPageNo = Convert.ToInt32(numbers[index + 2]);
            }

            e1.TotalCount = this.TotalCount;

            OnPageSwitch(this, e1);
        }

        void ButtonBegin(string strClass)
        {
            /*
            LiteralControl literal = new LiteralControl("<div class='"+strClass+"'>");
            this.Controls.Add(literal);
             * */
        }

        void ButtonEnd()
        {
            /*
            LiteralControl literal = new LiteralControl("</div>");
            this.Controls.Add(literal);
             * */
        }


        protected override void Render(HtmlTextWriter output)
        {
            if (this.EventMode == true)
            {
                LinkButton first = (LinkButton)this.FindControl("first");
                if (this.CurrentPageNo == 0)
                    first.Visible = false;

                LinkButton prev = (LinkButton)this.FindControl("prev");
                if (this.CurrentPageNo == 0)
                    prev.Visible = false;

                ButtonBegin("first");
                LinkButton next = (LinkButton)this.FindControl("next");
                if (this.CurrentPageNo >= this.TotalCount - 1)
                    next.Visible = false;

                int nStart = 0;
                int nEnd = 0;
                nStart = this.CurrentPageNo - 4;
                nEnd = this.CurrentPageNo + 4;

                bool bHead = false;
                if (nStart < 0)
                {
                    nStart = 0;
                    nEnd = nStart + 9;
                    bHead = true;
                }

                if (nStart == 0)
                    first.Visible = false;

                if (nEnd > this.TotalCount - 1)
                {
                    nEnd = this.TotalCount - 1;
                    if (bHead == false)
                    {
                        nStart = nEnd - 9;
                        if (nStart < 0)
                            nStart = 0;
                    }
                }

                int nLength = nEnd - nStart + 1;
                List<int> pagenos = new List<int>();

                for (int i = 0; i < 10; i++)
                {
                    string strID = "goto_" + i.ToString();
                    LinkButton link = (LinkButton)this.FindControl(strID);
                    if (i >= nLength)
                    {
                        link.Text = "";
                        link.Visible = false;
                        pagenos.Add(-1);
                    }
                    else
                    {
                        if (this.CurrentPageNo == nStart + i)
                            link.CssClass += " current";
                        link.Text = (nStart + i + 1).ToString();
                        link.Visible = true;
                        pagenos.Add(nStart + i);
                    }
                }

                // 最后一个页
                {
                    string strID = "goto_10";
                    LinkButton link = (LinkButton)this.FindControl(strID);
                    if (nEnd != this.TotalCount - 1)
                    {
                        link.Text =
                            ((nEnd != this.TotalCount - 2) ? "..." : "")
                            + (this.TotalCount - 1 + 1).ToString();
                        link.Visible = true;
                        pagenos.Add(this.TotalCount - 1);
                    }
                    else
                    {
                        link.Visible = false;
                        pagenos.Add(-1);
                    }
                }

                string strLine = (this.CurrentPageNo - 1).ToString()
                    + "," + (this.CurrentPageNo + 1).ToString();
                for (int i = 0; i < pagenos.Count; i++)
                {
                    strLine += "," + pagenos[i].ToString();
                }
                this.PageNoList = strLine;

                base.Render(output);
            }
            else
            {
                // URL模式

                HyperLink first = (HyperLink)this.FindControl("first");
                if (this.CurrentPageNo == 0)
                    first.Visible = false;

                HyperLink prev = (HyperLink)this.FindControl("prev");
                if (this.CurrentPageNo == 0)
                    prev.Visible = false;

                HyperLink next = (HyperLink)this.FindControl("next");
                if (this.CurrentPageNo >= this.TotalCount - 1)
                    next.Visible = false;

                int nStart = 0;
                int nEnd = 0;
                nStart = this.CurrentPageNo - 4;
                nEnd = this.CurrentPageNo + 4;

                bool bHead = false;
                if (nStart < 0)
                {
                    nStart = 0;
                    nEnd = nStart + 9;
                    bHead = true;
                }

                if (nStart == 0)
                    first.Visible = false;

                if (nEnd > this.TotalCount - 1)
                {
                    nEnd = this.TotalCount - 1;
                    if (bHead == false)
                    {
                        nStart = nEnd - 9;
                        if (nStart < 0)
                            nStart = 0;
                    }
                }

                GetBaseUrlEventArgs e = new GetBaseUrlEventArgs();
                this.GetBaseUrl(this, e);
                string strBaseUrl = RemoveParameter(e.BaseUrl, this.PageNoParaName);

                int nLength = nEnd - nStart + 1;
                List<int> pagenos = new List<int>();

                for (int i = 0; i < 10; i++)
                {
                    string strID = "goto_" + i.ToString();
                    HyperLink link = (HyperLink)this.FindControl(strID);
                    if (i >= nLength)
                    {
                        link.Text = "";
                        link.Visible = false;
                        link.NavigateUrl = "";
                        //pagenos.Add(-1);
                    }
                    else
                    {
                        if (this.CurrentPageNo == nStart + i)
                            link.CssClass += " current";
                        link.Text = (nStart + i + 1).ToString();
                        link.Visible = true;
                        link.NavigateUrl = GetUrl(nStart + i, strBaseUrl, this.PageNoParaName);
                        //pagenos.Add(nStart + i);
                    }
                }

                // 最后一个页
                {
                    string strID = "goto_10";
                    HyperLink link = (HyperLink)this.FindControl(strID);
                    if (nEnd != this.TotalCount - 1)
                    {
                        link.Text =
                            ((nEnd != this.TotalCount - 2) ? "..." : "")
                            + (this.TotalCount - 1 + 1).ToString();
                        link.Visible = true;
                        link.NavigateUrl = GetUrl(this.TotalCount - 1, strBaseUrl, this.PageNoParaName);
                        //pagenos.Add(this.TotalCount - 1);
                    }
                    else
                    {
                        link.Visible = false;
                        link.NavigateUrl = "";
                        //pagenos.Add(-1);
                    }
                }

                if (first.Visible == true)
                    first.NavigateUrl = GetUrl(0, strBaseUrl, this.PageNoParaName);
                if (prev.Visible == true)
                    prev.NavigateUrl = GetUrl(CurrentPageNo - 1, strBaseUrl, this.PageNoParaName);
                if (next.Visible == true)
                    next.NavigateUrl = GetUrl(CurrentPageNo + 1, strBaseUrl, this.PageNoParaName);

                base.Render(output);
            }
        }

        public static string RemoveParameter(string strRawUrl, string strParaName)
        {
            // 去掉baseurl中原有的 &pageno 部分
            string strUrl = "";
            int nStart = strRawUrl.IndexOf("&" + strParaName + "=");
            if (nStart != -1)
            {
                int nEnd = strRawUrl.IndexOf("&", nStart + 1);
                if (nEnd == -1)
                    strUrl = strRawUrl.Substring(0, nStart);
                else
                    strUrl = strRawUrl.Remove(nStart, nEnd - nStart);
            }
            else
            {
                nStart = strRawUrl.IndexOf("?" + strParaName + "=");
                if (nStart != -1)
                {
                    int nEnd = strRawUrl.IndexOf("&", nStart + 1);
                    if (nEnd == -1)
                        strUrl = strRawUrl.Substring(0, nStart + 1);
                    else
                        strUrl = strRawUrl.Remove(nStart + 1, nEnd - nStart);
                }
                else
                    strUrl = strRawUrl;
            }

            return strUrl;
        }

        // parameters:
        //      nPageIndex  从0开始计算的页码数字
        static string GetUrl(int nPageIndex, string strBaseUrl, string strParamName)
        {
            return strBaseUrl + "&" + strParamName + "=" + (nPageIndex + 1).ToString();
        }

        protected void OnPageSwitch(object sender, PageSwitchEventArgs e)
        {
            if (this.PageSwitch != null)
            {
                this.PageSwitch(sender, e);
            }
        }
    }

    public delegate void PageSwitchEventHandler(object sender,
    PageSwitchEventArgs e);

    public class PageSwitchEventArgs : EventArgs
    {
        public int GotoPageNo = -1;
        public int TotalCount = 0;
    }

    public delegate void GetBaseUrlEventHandler(object sender,
GetBaseUrlEventArgs e);

    public class GetBaseUrlEventArgs : EventArgs
    {
        public string BaseUrl = ""; // [out]
    }
}
