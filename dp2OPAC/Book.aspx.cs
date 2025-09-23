using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Xml;
using System.Globalization;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
// using DigitalPlatform.CirculationClient;

public partial class Book : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

#if NO
    protected override void InitializeCulture()
    {
        WebUtil.InitLang(this);
        base.InitializeCulture();
    }
#endif

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        this.BiblioControl1.Wrapper = true;
        this.ItemsControl1.Wrapper = true;
        this.CommentsControl1.Wrapper = true;
        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.Search;

        this.BiblioControl1.WantFocus -= new WantFocusEventHandler(BiblioControl1_WantFocus);
        this.BiblioControl1.WantFocus += new WantFocusEventHandler(BiblioControl1_WantFocus);
        this.BiblioControl1.DisableAjax = true; // 禁止Ajax，让搜索网络爬虫进入后可以看到书目信息
        this.BiblioControl1.AutoSetPageTitle = true; // 自动根据题名设置HTML的<title>

        this.CommentsControl1.WantFocus -= new WantFocusEventHandler(CommentsControl1_WantFocus);
        this.CommentsControl1.WantFocus += new WantFocusEventHandler(CommentsControl1_WantFocus);
    }

    void CommentsControl1_WantFocus(object sender, WantFocusEventArgs e)
    {
        if (e.Focus == true)
            this.BiblioControl1.Active = false;
        else
            this.BiblioControl1.Active = true;
    }

    void BiblioControl1_WantFocus(object sender, WantFocusEventArgs e)
    {
        if (e.Focus == true)
            this.CommentsControl1.Active = false;
        else
            this.CommentsControl1.Active = true;
    }

#if NO
    protected void Page_Unload(object sender, EventArgs e)
    {
        this.BiblioControl1.WantFocus -= new WantFocusEventHandler(BiblioControl1_WantFocus);
        this.CommentsControl1.WantFocus -= new WantFocusEventHandler(CommentsControl1_WantFocus);
    }
#endif

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
#if REMOVED
            if (this.Page.Request["forcelogin"] == "on")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                return;
            }
            if (this.Page.Request["forcelogin"] == "userid")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx?loginstyle=librarian", true);
                return;
            }

            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
#endif
            var url = GetDefaultLoginUrl();
            if (url != null)
            {
                Response.Redirect(url, true);
                return;
            }
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        // TODO: 不登录也应能够使用。因为rss中会引用book.aspx

        // 2008/12/15
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        //            Page.Response.AddHeader("Cache-Control", "public");
        this.Response.AddHeader("Expires", "0");


        string strBarcode = this.Page.Request["barcode"];
        string strItemRecPath = this.Page.Request["itemrecpath"];
        string strBorrower = this.Page.Request["borrower"];
        string strBiblioRecPath = this.Page.Request["bibliorecpath"];
        string strCommentRecPath = this.Page.Request["commentrecpath"];

        this.ItemsControl1.Barcode = strBarcode;

        // 通过条码号查询而得到书目记录路径
        if (String.IsNullOrEmpty(strBarcode) == false
            && String.IsNullOrEmpty(strBiblioRecPath) == true)
        {
            // string strBiblioRecPath = "";
            string strError = "";
            string strWarning = "";

            string strSearchText = "";
            if (String.IsNullOrEmpty(strItemRecPath) == false)
            {
                if (strItemRecPath[0] == '@')
                    strSearchText = strItemRecPath;
                else
                    strSearchText = "@path:" + strItemRecPath;
            }
            else
                strSearchText = strBarcode;

            // TODO: 需要实现多个命中时根据borrower进行判断
            int nRet = this.ItemsControl1.GetBiblioRecPath(strSearchText,
                strBorrower,
                out strBiblioRecPath,
                out strWarning,
                out strError);
            if (nRet == -1)
            {
                this.Response.Write(strError);
                this.Response.End();
                return;
            }

            // this.ItemsControl1.WarningText = strWarning;
            // this.BiblioControl1.RecPath = strBiblioRecPath;
            // this.CommentsControl1.BiblioRecPath = strBiblioRecPath;
        }
        // 通过评注记录路径查询而得到书目记录路径
        else if (String.IsNullOrEmpty(strCommentRecPath) == false
            && String.IsNullOrEmpty(strBiblioRecPath) == true)
        {
            string strError = "";

            // 通过评注记录路径得知从属的种记录路径
            // parameters:
            // return:
            //      -1  error
            //      0   评注记录没有找到(strError中有说明信息)
            //      1   找到
            int nRet = app.GetBiblioRecPathByCommentRecPath(
            // sessioninfo,
            strCommentRecPath,
            out strBiblioRecPath,
            out strError);
            if (nRet == -1)
            {
                this.Response.Write(strError);
                this.Response.End();
                return;
            }
        }
        // 通过实体记录路径查询而得到书目记录路径
        else if (String.IsNullOrEmpty(strItemRecPath) == false
            && String.IsNullOrEmpty(strBiblioRecPath) == true)
        {
            string strError = "";

            // 通过册记录路径得知从属的种记录路径
            // parameters:
            // return:
            //      -1  error
            //      0   评注记录没有找到(strError中有说明信息)
            //      1   找到
            int nRet = app.GetBiblioRecPathByItemRecPath(
            // sessioninfo,
            strItemRecPath,
            out strBiblioRecPath,
            out strError);
            if (nRet == -1)
            {
                this.Response.Write(strError);
                this.Response.End();
                return;
            }
        }


        this.ItemsControl1.BiblioRecPath = strBiblioRecPath;
        this.CommentsControl1.BiblioRecPath = strBiblioRecPath;

        if (String.IsNullOrEmpty(strBiblioRecPath) == false)
        {
            this.BiblioControl1.RecPath = strBiblioRecPath;
        }

        if (this.IsPostBack == false)
        {
            this.CommentsControl1.FocusRecPath = strCommentRecPath;
            this.ItemsControl1.FocusRecPath = strItemRecPath;
        }

        if (this.IsPostBack == false)
        {
            string strAction = this.Page.Request["action"];
            if (strAction == "uploadcoverimage")
            {
                this.CommentsControl1.NewTitle = "#封面图像";
                this.CommentsControl1.NewState = "#封面图像";

            }
        }
    }
}