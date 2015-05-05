using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:AutoIndentLiteral runat=server></{0}:AutoIndentLiteral>")]
    public class AutoIndentLiteral : WebControl
    {
        public string Text = "";

        public AutoIndentLiteral(string strText)
        {
            this.Text = strText;
        }
        public AutoIndentLiteral()
        {
        }

        /*
        protected override void CreateChildControls()
        {
            string strText = this.Text;
            LiteralControl literal = null;
            for (; ; )
            {
                int nRet = strText.IndexOf("<%");
                if (nRet == -1)
                {
                    literal = new LiteralControl(strText);
                    this.Controls.Add(literal);
                    break;
                }

                if (nRet > 0)
                {
                    literal = new LiteralControl(strText.Substring(0, nRet));
                    this.Controls.Add(literal);
                    strText = strText.Substring(nRet);
                }

                if (strText.Length == 0)
                    break;

                if (strText.Length >= "<%begin%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%begin%>".Length);
                    if (strTemp == "<%begin%>")
                    {
                        BeginIndentor begin = new BeginIndentor();
                        this.Controls.Add(begin);
                        strText = strText.Substring("<%begin%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%end%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%end%>".Length);
                    if (strTemp == "<%end%>")
                    {
                        EndIndentor end = new EndIndentor();
                        this.Controls.Add(end);
                        strText = strText.Substring("<%end%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%normal%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%normal%>".Length);
                    if (strTemp == "<%normal%>")
                    {
                        NormalIndentor end = new NormalIndentor();
                        this.Controls.Add(end);
                        strText = strText.Substring("<%normal%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%normal(1)%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%normal(1)%>".Length);
                    if (strTemp == "<%normal(1)%>")
                    {
                        NormalIndentor end = new NormalIndentor();
                        this.Controls.Add(end);
                        strText = strText.Substring("<%normal(1)%>".Length);
                        continue;
                    }
                }

                nRet = strText.IndexOf("%>", 2);
                if (nRet == -1)
                {
                    literal = new LiteralControl(strText);
                    this.Controls.Add(literal);
                    break;
                }

                literal = new LiteralControl(strText.Substring(0,nRet+2));
                this.Controls.Add(literal);

                strText = strText.Substring(nRet + 2);
            }
        }
         * */

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            string strText = this.Text;
            for (; ; )
            {
                int nRet = strText.IndexOf("<%");
                if (nRet == -1)
                {
                    writer.Write(strText);
                    break;
                }

                if (nRet > 0)
                {
                    writer.Write(strText.Substring(0, nRet));
                    strText = strText.Substring(nRet);
                }

                if (strText.Length == 0)
                    break;

                if (strText.Length >= "<%begin%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%begin%>".Length);
                    if (strTemp == "<%begin%>")
                    {
                        BeginIndentor.Write(writer);
                        strText = strText.Substring("<%begin%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%end%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%end%>".Length);
                    if (strTemp == "<%end%>")
                    {
                        EndIndentor.Write(writer);
                        strText = strText.Substring("<%end%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%normal%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%normal%>".Length);
                    if (strTemp == "<%normal%>")
                    {
                        NormalIndentor.Write(writer);
                        strText = strText.Substring("<%normal%>".Length);
                        continue;
                    }
                }

                if (strText.Length >= "<%normal(1)%>".Length)
                {
                    string strTemp = strText.Substring(0, "<%normal(1)%>".Length);
                    if (strTemp == "<%normal(1)%>")
                    {
                        NormalIndentor.Write(writer, 1);
                        strText = strText.Substring("<%normal(1)%>".Length);
                        continue;
                    }
                }

                nRet = strText.IndexOf("%>", 2);
                if (nRet == -1)
                {
                    writer.Write(strText);
                    break;
                }

                writer.Write(strText.Substring(0, nRet + 2));

                strText = strText.Substring(nRet + 2);
            }

            base.Render(writer);
        }
    }
}
