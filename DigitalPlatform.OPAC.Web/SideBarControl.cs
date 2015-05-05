using System;
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

using DigitalPlatform.Xml;

namespace DigitalPlatform.OPAC.Web
{
    // 布局风格枚举
    public enum SideBarLayoutStyle
    {
        Vertical = 0,
        Horizontal = 1,
    };

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:SideBarControl runat=server></{0}:SideBarControl>")]
    public class SideBarControl : WebControl
    {
        public bool Wrapper = false;

        public SideBarLayoutStyle LayoutStyle = SideBarLayoutStyle.Vertical;

        public string CfgFile = "";

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

        protected override void CreateChildControls()
        {

        }

#if NO
        static bool IsUrlEqual(string strBaseUrl,
            string url1,
            string url2)
        {
            if (strBaseUrl != null)
                strBaseUrl = strBaseUrl.ToLower();
            if (url1 != null)
                url1 = url1.ToLower();
            if (url2 != null)
                url2 = url2.ToLower();

            Uri baseuri = new Uri(strBaseUrl);
            Uri uri1 = new Uri(baseuri, url1);
            Uri uri2 = new Uri(baseuri, url2);

            string strPath1 = "";
            string strQuery1 = "";

            SplitPathAndQuery(uri1.PathAndQuery,
            out strPath1,
            out strQuery1);

            string strPath2 = "";
            string strQuery2 = "";

            SplitPathAndQuery(uri2.PathAndQuery,
            out strPath2,
            out strQuery2);

            if (strPath1 != strPath2)
                return false;

            string[] parameters1 = strQuery1.Split(new char[] { '&' });
            string[] parameters2 = strQuery2.Split(new char[] { '&' });

            if (parameters1.Length != parameters2.Length)
                return false;
            Array.Sort(parameters1);
            Array.Sort(parameters2);

            for (int i = 0; i < parameters1.Length; i++)
            {
                if (parameters1[i] != parameters2[i])
                    return false;
            }

            /*
            if (uri1.PathAndQuery != uri2.PathAndQuery)
                return false;
             * */

            return true;
        }
#endif

        // return:
        //      -1  path部分就不相等
        //      >= 0    匹配上的参数个数
        static int CompareUrl(string strBaseUrl,
    string url1,
    string url2)
        {
            if (strBaseUrl != null)
                strBaseUrl = strBaseUrl.ToLower();
            if (url1 != null)
                url1 = url1.ToLower();
            if (url2 != null)
                url2 = url2.ToLower();

            Uri baseuri = new Uri(strBaseUrl);
            Uri uri1 = new Uri(baseuri, url1);
            Uri uri2 = new Uri(baseuri, url2);

            string strPath1 = "";
            string strQuery1 = "";

            SplitPathAndQuery(uri1.PathAndQuery,
            out strPath1,
            out strQuery1);

            string strPath2 = "";
            string strQuery2 = "";

            SplitPathAndQuery(uri2.PathAndQuery,
            out strPath2,
            out strQuery2);

            if (strPath1 != strPath2)
                return -1;

            string[] parameters1 = strQuery1.Split(new char[] { '&' });
            string[] parameters2 = strQuery2.Split(new char[] { '&' });

            int nCount = 0;
            for (int i = 0; i < parameters1.Length; i++)
            {
                string strParameter1 = parameters1[i];
                for (int j = 0; j < parameters2.Length; j++)
                {
                    string strParameter2 = parameters2[j];
                    if (strParameter1 == strParameter2)
                    {
                        nCount++;
                        break;
                    }
                }
            }

            return nCount;
        }

        // query这里其实指?后面的参数部分
        static void SplitPathAndQuery(string strPathAndQuery,
            out string strPath,
            out string strQuery)
        {
            strPath = "";
            strQuery = "";
            int nRet = strPathAndQuery.IndexOf("?");
            if (nRet == -1)
            {
                strPath = strPathAndQuery;
                return;
            }

            strPath = strPathAndQuery.Substring(0, nRet);
            strQuery = strPathAndQuery.Substring(nRet + 1);
        }


        class CompareInfo
        {
            public int Count = 0;
            public XmlNode Node = null;
        }

        class CountCompare : IComparer<CompareInfo>
        {
            int IComparer<CompareInfo>.Compare(CompareInfo x, CompareInfo y)
            {
                return x.Count - y.Count;
            }

        }

        protected override void Render(HtmlTextWriter output)
        {
            string strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.Load(this.CfgFile);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + this.CfgFile + "' 时出错: " + ex.Message;
                goto ERROR1;
            }

            // 窗口标题
            string strTitle = DomUtil.GetCaption(this.Lang,
                dom.DocumentElement);

            if (this.Wrapper == true)
            {
                this.Controls.Add(new LiteralControl(
                    this.GetPrefixString(
                    strTitle,
                    "command_wrapper")));
            }

            this.Controls.Add(new LiteralControl("<table class='sidebar'>"
            ));

            if (this.LayoutStyle == SideBarLayoutStyle.Horizontal)
            {
                this.Controls.Add(new LiteralControl(
                    "<tr>"
                ));
            }

            string strBaseUrl = this.Page.Request.Url.ToString();

            HyperLink link = null;

            List<CompareInfo> compare_infos = new List<CompareInfo>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");

            // 挑选出 active node
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode nodeItem = nodes[i];
                string strUrl = DomUtil.GetAttr(nodeItem, "url");

                // 和当前page url比较
                string strTestUrl = this.Page.Request.RawUrl;

                int nRet = CompareUrl(strBaseUrl, strTestUrl, strUrl);
                if (nRet == -1)
                    continue;
                CompareInfo info = new CompareInfo();
                info.Count = nRet;
                info.Node = nodeItem;
                compare_infos.Add(info);
            }

            compare_infos.Sort(new CountCompare());
            XmlNode active_node = null;
            if (compare_infos.Count > 0)
                active_node = compare_infos[compare_infos.Count - 1].Node;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode nodeItem = nodes[i];
                string strUrl = DomUtil.GetAttr(nodeItem, "url");

                // 和当前page url比较
                string strTestUrl = this.Page.Request.RawUrl;

                string strTitleString = DomUtil.GetAttr(nodeItem, "title");
                if (string.IsNullOrEmpty(strTitleString) == false)
                    strTitleString = " title='" + HttpUtility.HtmlEncode(strTitleString) + "' ";

                // bool bSame = IsUrlEqual(strBaseUrl, strTestUrl, strUrl);
                string strCssClass = "cmd";

                string strClass = DomUtil.GetAttr(nodeItem, "class");
                if (string.IsNullOrEmpty(strClass) == false)
                    strCssClass += " " + strClass;
                /*
                if (bSame == true)
                    strCssClass += " active";
                 * */
                if (nodeItem == active_node)
                    strCssClass += " active";

                if (this.LayoutStyle == SideBarLayoutStyle.Vertical)
                {
                    this.Controls.Add(new LiteralControl(
                        "<tr><td class='" + strCssClass + "' " + strTitleString + ">"
                    ));
                }
                else if (this.LayoutStyle == SideBarLayoutStyle.Horizontal)
                {
                    this.Controls.Add(new LiteralControl(
                        "<td class='" + strCssClass + "' " + strTitleString + ">"
                    ));
                }


                string strItemTitle = DomUtil.GetCaption(this.Lang,
                    nodeItem);


                link = new HyperLink();
                link.Text = strItemTitle;
                link.NavigateUrl = strUrl;
                this.Controls.Add(link);

                XmlNode nodeTips = nodeItem.SelectSingleNode("tips");
                if (nodeTips != null)
                {
                    this.Controls.Add(new LiteralControl(
                        nodeTips.InnerXml ));
                }

                if (this.LayoutStyle == SideBarLayoutStyle.Vertical)
                {
                    this.Controls.Add(new LiteralControl(
                        "</td></tr>"
                    ));
                }
                else if (this.LayoutStyle == SideBarLayoutStyle.Horizontal)
                {
                    this.Controls.Add(new LiteralControl(
                        "</td>"
                    ));
                }
            }

            if (this.LayoutStyle == SideBarLayoutStyle.Horizontal)
            {
                this.Controls.Add(new LiteralControl(
                    "</tr>"
                ));
            }

            this.Controls.Add(new LiteralControl(
                "</table>"
            ));

            if (this.Wrapper == true)
                this.Controls.Add(new LiteralControl(
        this.GetPostfixString()
        ));

            base.Render(output);
            return;
        ERROR1:
            this.Controls.Add(new LiteralControl(strError));
            base.Render(output);
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

}

