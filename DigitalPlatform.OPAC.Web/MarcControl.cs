using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;


using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Web
{

    [ToolboxData("<{0}:MarcControl runat=server></{0}:MarcControl>")]
    public class MarcControl : WebControl, INamingContainer
    {
        const string SubFieldChar = "‡";
        const string FieldEndChar = "¶";
        public string RecPath = "";

        public bool Wrapper = false;

        public bool SubfieldReturn = false;

        protected override void CreateChildControls()
        {
            // 种
            LiteralControl literal = new LiteralControl();
            literal.ID = "biblio";
            literal.Text = "";
            this.Controls.Add(literal);
        }


        public static string GetHtmlOfMarc(string strMARC, bool bSubfieldReturn)
        {
            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);
            for (int i = 0; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";

                int nRet = MarcUtil.GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet != 1)
                    break;

                string strLineClass = "";
                string strFieldName = "";
                string strIndicatior = "";
                string strContent = "";
                if (i != 0)
                {
                    // 取字段名
                    if (strField.Length < 3)
                    {
                        strFieldName = strField;
                        strField = "";
                    }
                    else
                    {
                        strFieldName = strField.Substring(0, 3);
                        strField = strField.Substring(3);
                    }

                    // 取指示符
                    if (FilterItem.IsControlFieldName(strFieldName) == true)
                    {
                        strLineClass = "controlfield";
                        strField = strField.Replace(' ', '_');
                    }
                    else
                    {
                        if (strField.Length < 2)
                        {
                            strIndicatior = strField;
                            strField = "";
                        }
                        else
                        {
                            strIndicatior = strField.Substring(0, 2);
                            strField = strField.Substring(2);
                        }
                        strIndicatior = strIndicatior.Replace(' ', '_');

                        strLineClass = "datafield";

                        // 1XX字段有定长内容
                        if (strFieldName.Length >= 1 && strFieldName[0] == '1')
                        {
                            strField = strField.Replace(' ', '_');
                            strLineClass += " fixedlengthsubfield";
                        }
                    }
                }
                else
                {
                    strLineClass = "header";
                    strField = strField.Replace(' ', '_');
                }

                /*
                strContent = strField.Replace(new string((char)31,1),
                    "<span>|</span>");
                 * */
                strContent = GetHtmlFieldContent(strField,
                    bSubfieldReturn);

                // 
                strResult.Append("\r\n<tr class='" + strLineClass + "'><td class='fieldname'>" + strFieldName + "</td>"
                    + "<td class='indicator'>" + strIndicatior + "</td>"
                    + "<td class='content'>" + strContent + "</td></tr>");
            }

            strResult.Append("\r\n</table>");

            return strResult.ToString();
        }

        static string GetHtmlFieldContent(string strContent,
            bool bSubfieldReturn)
        {
            StringBuilder result = new StringBuilder(1000);
            for (int i = 0; i < strContent.Length; i++)
            {
                char ch = strContent[i];
                if (ch == (char)31)
                {
                    if (result.Length > 0)
                    {
                        if (bSubfieldReturn == true)
                            result.Append("<br/>");
                        else
                            result.Append(" "); // 为了显示时候可以折行
                    }

                    result.Append("<span class='subfield'>");

                    // 2022/1/11
                    result.Append((char)0x200e);

                    result.Append(SubFieldChar);
                    if (i < strContent.Length - 1)
                    {
                        result.Append(strContent[i + 1]);
                        i++;
                    }
                    else
                        result.Append(SubFieldChar);

                    // 2022/1/11
                    // 为 $9 后面加一个空格。解决 Unicode bidi 问题
                    if (result.Length > 0 && char.IsDigit(result[result.Length - 1]))
                        result.Append(' ');

                    result.Append("</span>");
                    continue;
                }
                result.Append(ch);
            }

            result.Append("<span class='fieldend'>" + FieldEndChar + "</span>");

            return result.ToString();
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (this.RecPath == "")
            {
                output.Write("尚未指定记录路径");
                return;
            }

            int nRet = 0;
            long lRet = 0;
            string strError = "";
            // string strOutputPath = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strBiblioXml = "";
            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                lRet = // sessioninfo.Channel.
                    channel.GetBiblioInfo(
                    null,
                    this.RecPath,
                    "",
                    "xml",
                    out strBiblioXml,
                    out strError);
                /*
                string strMetaData = "";
                byte[] timestamp = null;
                lRet = channel.GetRes(this.RecPath,
                    out strBiblioXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                 * */
                if (lRet == -1)
                {
                    strError = "获得书目记录 '" + this.RecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // string strBiblioDbName = ResPath.GetDbName(this.RecPath);
            string strPrefix = "";
            if (this.Wrapper == true)
                strPrefix = this.GetPrefixString("MARC", "content_wrapper")
                    + "<div class='biblio_wrapper'>";

            string strPostfix = "";
            if (this.Wrapper == true)
                strPostfix = "</div>" + this.GetPostfixString();


            // output.Write(strBiblio);
            LiteralControl literal = (LiteralControl)this.FindControl("biblio");
            literal.Text = strPrefix + GetHtmlOfMarc(strMarc, this.SubfieldReturn) + strPostfix;

            base.Render(output);
            return;
        ERROR1:
            output.Write(strError);
        }

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            return "<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"    //  
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
