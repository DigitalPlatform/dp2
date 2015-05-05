using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:NormalIndentor runat=server></{0}:NormalIndentor>")]
    public class NormalIndentor : WebControl
    {
        public int ExtraTabs = 0;

        public NormalIndentor(int nExtraTabs)
        {
            this.ExtraTabs = nExtraTabs;
        }

        public NormalIndentor()
        {
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write("\r\n");
            for (int i = 0; i < output.Indent + this.ExtraTabs; i++)
            {
                output.Write("\t");
            }
        }

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        public static void Write(HtmlTextWriter output)
        {
            Write(output, 0);
        }

        public static void Write(HtmlTextWriter output, int nDelta)
        {
            output.Write("\r\n");
            for (int i = 0; i < output.Indent + nDelta; i++)
            {
                output.Write("\t");
            }
        }

    }
}
