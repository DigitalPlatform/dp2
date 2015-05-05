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


        protected override void RenderContents(HtmlTextWriter output)
        {
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
            output.Write("<td class='left'></td>");

            // 中
            NormalIndentor.Write(output);
            output.Write("<td class='middle'>"
                + this.GetString("dp2图书馆集成系统")
                + " V2 - " + this.GetString("版权所有") + " © 2006-2015 <a href='http://dp2003.com'>"
                + this.GetString("数字平台(北京)软件有限责任公司")
                + "</a>"
                + "</td>");

            // 右
            NormalIndentor.Write(output);
            output.Write("<td class='right'>");

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
