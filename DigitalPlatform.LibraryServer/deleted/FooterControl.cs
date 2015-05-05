using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.LibraryServer
{
#if NOOOO
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:FooterControl runat=server></{0}:FooterControl>")]
    public class FooterControl : WebControl, INamingContainer
    {

        protected override void RenderContents(HtmlTextWriter output)
        {
            /*
            output.Write(
                
        "        				</td>" +
		"		<td width='10' style='background-image: url(./shadow_right.gif); background-repeat: repeat-y;'></td>" +
		"	</tr>" +
		"	<tr>" +
		"		<td width='780' height='24' colspan='3' style='background-image: url(./shadow_bottom.gif); background-repeat: no-repeat;'></td>" +
		"	</tr>" + 
		"</table>"
            );
             * */

            // 主体内容结束
            output.Write("</td></tr>");


            // 总表格结束
            output.Write("</table>");
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
    }

#endif
}
