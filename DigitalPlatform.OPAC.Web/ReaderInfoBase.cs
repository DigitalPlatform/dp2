using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;

using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    public class ReaderInfoBase : WebControl, INamingContainer
    {
        // public string ReaderBarcode = "";

        public XmlDocument ReaderDom = null;

        public OpacApplication app = null;
        public SessionInfo sessioninfo = null;

        // 作为管理员身份此时要查看的读者证条码号。注意，不是指管理员自己的读者证
        // 存储在Session中
        public string ReaderBarcode
        {
            get
            {
                object o = this.Page.Session[this.ID + "ReaderInfoBase_readerbarcode"];
                if (o == null)
                    return "";
                return (string)o;
            }

            set
            {
                this.Page.Session[this.ID + "ReaderInfoBase_readerbarcode"] = value;
            }
        }

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        // return:
        //      -1  出错
        //      0   成功
        //      1   尚未登录
        public int LoadReaderXml(out string strError)
        {
            strError = "";

            app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(sessioninfo.UserID) == true)
            {
                return 1;
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
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
                return 1;

            if (nRet == -2)
            {
                if (String.IsNullOrEmpty(this.ReaderBarcode) == true)
                {
                    strError = "当前登录的用户不是reader类型，并且ReaderInfoBase.ReaderBarcode也为空";
                    goto ERROR1;
                }

                // TODO: 是否进一步判断Type
                // if (sessioninfo.Account.Type != "worreader")

                // 管理员获得特定证条码号的读者记录DOM
                // return:
                //      -2  当前登录的用户不是librarian类型
                //      -1  出错
                //      0   尚未登录
                //      1   成功
                nRet = sessioninfo.GetOtherReaderDom(
                    this.ReaderBarcode,
                    out readerdom,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;

                if (nRet == 0)
                    return 1;
            }

            this.ReaderDom = readerdom;

            return 0;
        ERROR1:
            return -1;
        }

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            if (String.IsNullOrEmpty(strWrapperClass) == true)
                strWrapperClass = "content_wrapper";

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

        public int PageMaxLines
        {
            get
            {
                object o = ViewState[this.ID + "Browse_PageMaxLines"];
                if (o == null)
                    return 10;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "Browse_PageMaxLines"] = value;
            }
        }

        public int LineCount
        {
            get
            {
                object o = ViewState[this.ID + "Browse_LineCount"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "Browse_LineCount"] = value;
            }
        }

        public int StartIndex
        {
            get
            {
                object o = ViewState[this.ID + "Browse_StartIndex"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "Browse_StartIndex"] = value;
            }
        }

    }
}
