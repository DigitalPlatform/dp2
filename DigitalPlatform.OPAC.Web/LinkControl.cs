using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 用于构造HTML页面中<link>局部
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:LinkControl runat=server></{0}:LinkControl>")]
    public class LinkControl : WebControl
    {
        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            string strHref = this.Attributes["href"];


            string strStyleDirName = GetStyleDirName();
            if (this.IsNewStyle == true)
                output.Write("<LINK href='"
                    + "./stylenew/" + strStyleDirName + "/"
                    + strHref
                    + "' type='text/css' rel='stylesheet' />");
            else
                output.Write("<LINK href='"
    + "./style/" + strStyleDirName + "/"
    + strHref
    + "' type='text/css' rel='stylesheet' />");
#if NO
            string strStyle = "";
            string strLibraryCode = "";
            GetStyleDirName(out strStyle,
            out strLibraryCode);

            output.Write("<LINK href='./css.aspx?style="
                + HttpUtility.UrlEncode(strStyle)
                + "&librarycode="
                + HttpUtility.UrlEncode(strLibraryCode)
                + "&name="
                + HttpUtility.UrlEncode(strHref)
                + "' type='text/css' rel='stylesheet' />");
#endif
        }

        public bool IsNewStyle
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)

                    return false;

#if NO
                string strValue = app.WebUiDom.DocumentElement.GetAttribute("newStyle");
                if (string.IsNullOrEmpty(strValue) == true)
                    return false;

                return DomUtil.IsBooleanTrue(strValue);
#endif
                return app.IsNewStyle;
            }
        }

        public static string MakeDir(string strLibraryDirName,
            string strDirName)
        {
            if (string.IsNullOrEmpty(strDirName) == true)
                strDirName = "0"; 
            if (string.IsNullOrEmpty(strLibraryDirName) == true)
                return strDirName;
            return strLibraryDirName + "/" + strDirName;
        }

#if NO
        void GetStyleDirName(out string strStyle,
            out string strLibraryCode)
        {
            strStyle = "";
            strLibraryCode = "";

            // 面板上选择的馆代码
            strLibraryCode = (string)this.Page.Session["librarycode"];
        }

#endif
        string GetStyleDirName()
        {
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            string strLibraryCodeList = ""; // 当前用户管辖的馆代码列表
            if (sessioninfo.Channel != null)
                strLibraryCodeList = sessioninfo.Channel.LibraryCodeList;

            // 面板上选择的馆代码
            string strSelectedLibraryCode = (string)this.Page.Session["librarycode"];

            string strLibraryStyleDir = ""; // 分馆的风格目录

            if (string.IsNullOrEmpty(strSelectedLibraryCode) == false)
            {
                XmlNode nodeLibrary = app.WebUiDom.DocumentElement.SelectSingleNode("libraries/library[@code='" + strSelectedLibraryCode + "']");
                if (nodeLibrary != null)
                {
                    strLibraryStyleDir = DomUtil.GetAttr(nodeLibrary, "style");
                    if (string.IsNullOrEmpty(strLibraryStyleDir) == true)
                        strLibraryStyleDir = DomUtil.GetAttr(nodeLibrary, "code");
                }
            }

            // 2007/7/11
            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            XmlNode nodeUserType = null;
            if (loginstate == LoginState.NotLogin)
            {
                nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                   "titleBarControl/userType[@type='notlogin']");
            }
            else if (loginstate == LoginState.Public)
            {
                nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "titleBarControl/userType[@type='public']");
            }
            else if (loginstate == LoginState.Librarian)
            {
                nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "titleBarControl/userType[@type='librarian']");

            }

            if (nodeUserType != null)
            {
                string strStyleDirName = DomUtil.GetAttr(nodeUserType, "style");
                return MakeDir(strLibraryStyleDir, strStyleDirName);
#if NO
                if (String.IsNullOrEmpty(strStyleDirName) == true)
                    return "0"; // 缺省值
                return strStyleDirName;
#endif
            }


            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
            {
                goto ERROR1;
            }

            // return PreferenceControl.GetReaderSelectedStyleDir(readerdom);
            return MakeDir(strLibraryStyleDir, PreferenceControl.GetReaderSelectedStyleDir(readerdom));

        ERROR1:
            return "0";
        }
    }
}
