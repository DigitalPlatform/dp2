using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:PanelControl runat=server></{0}:PanelControl>")]
    public class PanelControl : WebControl, INamingContainer
    {
        public string WrapperClass = "wrapper";
        public string Title = "title";
        public string TableClass = "panel";

        protected override void Render(HtmlTextWriter output)
        {
            output.Write(this.GetPrefixString(
                this.Title,
                this.WrapperClass)
                + "<table class='" + this.TableClass + "'>");
            this.RenderChildren(output);
            output.Write("</table>" + this.GetPostfixString());
        }

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            // 2010/10/3 add
            if (String.IsNullOrEmpty(strTitle) == true)
                strTitle = "&nbsp;";

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
