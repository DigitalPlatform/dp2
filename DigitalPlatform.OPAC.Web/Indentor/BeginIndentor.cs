using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:BeginIndentor runat=server></{0}:BeginIndentor>")]
    public class BeginIndentor : WebControl
    {
        public static void Write(HtmlTextWriter output)
        {
            output.Write("\r\n");
            for (int i = 0; i < output.Indent; i++)
            {
                output.Write("\t");
            }
            output.Indent++;
        }


        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write("\r\n");
            for (int i = 0; i < output.Indent; i++)
            {
                output.Write("\t");
            }

            output.Indent++;
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
