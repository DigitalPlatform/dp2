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
using System.Xml;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.Xml;

namespace DigitalPlatform.OPAC.Web
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:FooterBarControl runat=server></{0}:FooterBarControl>")]
    public class FooterBarControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.FooterBarControl.cs",
                typeof(FooterBarControl).Module.Assembly);

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

        int GetParentCount()
        {
            int nCount = 0;
            Control current = this;
            while (current != null)
            {
                current = current.Parent;
                nCount++;
            }

            return nCount;
        }

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        void GetCustomHtml(out string strLeftHtml,
            out string strRightHtml,
            out string strTopHtml,
            out string strBottomHtml)
        {
            strLeftHtml = "";
            strRightHtml = "";
            strTopHtml = "";
            strBottomHtml = "";

            // 获得配置参数
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            /*
             * 
	<footerBarControl>
		<leftAnchor lang='zh'>
			<a href="http://dp2003.com">图书馆主页</a>
		</leftAnchor>
		<leftAnchor lang='en'>
			<a href="http://dp2003.com">Library Homepage</a>
		</leftAnchor>        ...
             */
            // XmlNode nodeLeftAnchor = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/leftAnchor");
            XmlNode parent = app.WebUiDom.DocumentElement.SelectSingleNode("footerBarControl");
            if (parent != null)
            {
                // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode
                // parameters:
                //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
                XmlNode nodeLeftAnchor = DomUtil.GetLangedNode(
                    this.Lang,
                    parent,
                    "leftHtml");
                if (nodeLeftAnchor != null)
                    strLeftHtml = nodeLeftAnchor.InnerXml;

                XmlNode nodeRightAnchor = DomUtil.GetLangedNode(
    this.Lang,
    parent,
    "rightHtml");
                if (nodeRightAnchor != null)
                    strRightHtml = nodeRightAnchor.InnerXml;

                XmlNode nodeTopAnchor = DomUtil.GetLangedNode(
    this.Lang,
    parent,
    "topHtml");
                if (nodeTopAnchor != null)
                    strTopHtml = nodeTopAnchor.InnerXml;

                XmlNode nodeBottomAnchor = DomUtil.GetLangedNode(
    this.Lang,
    parent,
    "bottomHtml");
                if (nodeBottomAnchor != null)
                    strBottomHtml = nodeBottomAnchor.InnerXml;

            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            string strLeftHtml = "";
            string strRightHtml = "";
            string strTopHtml = "";
            string strBottomHtml = "";
            GetCustomHtml(out strLeftHtml, out strRightHtml, out strTopHtml, out strBottomHtml);

            int nParentCount = GetParentCount();
            output.Indent += nParentCount;

            // 主体内容结束
            NormalIndentor.Write(output, -1);
            output.Write("<!-- FooterBarControl 开始 -->");

            EndIndentor.Write(output);
            output.Write("</td></tr>");

            // 底部图像表格 开始

            NormalIndentor.Write(output);
            output.Write("<!-- 底部图像开始 -->");

            BeginIndentor.Write(output);
            output.Write("<tr><td>");

            BeginIndentor.Write(output);
            output.Write("<table class='footer'>");

            BeginIndentor.Write(output);
            output.Write("<tr class='footer'>");

            // 左
            NormalIndentor.Write(output);
            output.Write("<td class='left'>" + strLeftHtml + "</td>");

            // 中
            NormalIndentor.Write(output);
#if NO
            output.Write("<td class='middle'>"
                + this.GetString("dp2图书馆集成系统")
                + " V2 - " + this.GetString("版权所有") + " © 2006-2015 <a href='http://dp2003.com'>"
                + this.GetString("数字平台(北京)软件有限责任公司")
                + "</a>"
                + "</td>");
#endif
            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            output.Write("<td class='middle'>"
                + strTopHtml
    + this.GetString("dp2图书馆集成系统")
    + " V3 - <a href='https://github.com/DigitalPlatform/dp2'>"
    + "开源的图书馆管理系统"
    + strBottomHtml
    + "</td>");

            // 右
            NormalIndentor.Write(output);
            output.Write("<td class='right'>" + strRightHtml + "</td>");

            EndIndentor.Write(output);
            output.Write("</tr>");
            EndIndentor.Write(output);
            output.Write("</table>");

            EndIndentor.Write(output);
            output.Write("</td></tr>");
            NormalIndentor.Write(output);
            output.Write("<!-- 底部图像结束 -->");

            // 总表格结束
            EndIndentor.Write(output);
            output.Write("</table>");
            NormalIndentor.Write(output);
            output.Write("<!-- FooterBarControl 结束 -->");

            if (string.IsNullOrEmpty(app.OutgoingQueue) == false)
                output.Write("<center><br/><br/><div><img src='" + MyWebPage.GetStylePath(app, "qrcode_ilovelibrary_258.jpg") + "'></img><br/><br/>用微信“扫一扫”，关注“我爱图书馆”公众号，可获得超期、借书还书等微信消息通知<br/><br/><br/><br/></div></center>");

            output.Indent -= nParentCount;
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
    }
}
