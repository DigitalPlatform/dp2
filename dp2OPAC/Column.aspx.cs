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
using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
// using DigitalPlatform.CirculationClient;

public partial class Column : MyWebPage
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

        if (string.IsNullOrEmpty(sessioninfo.UserID) == true
    || StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            this.Button_createColumnStorage.Visible = false;
        }

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
        string strSideBarFile = Server.MapPath("./column_sidebar.xml");
        if (File.Exists(strSideBarFile) == true && File.Exists(Server.MapPath("./chat.aspx")) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        this.ColumnControl1.CommentColumn = app.CommentColumn;

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.BookReview;

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // 是否登录?
        if (sessioninfo.UserID == "")
        {
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
        }

        string strConfirmText = (string)this.GetLocalResourceObject("确实要创建栏目缓存");
        this.Button_createColumnStorage.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");

        string strTitle = (string)this.GetLocalResourceObject("最新书评");
        this.ColumnControl1.Title = strTitle;
    }

    protected void Button_createColumnStorage_Click(object sender, EventArgs e)
    {
        string strError = "";

        int nRet = app.CreateCommentColumn(
            sessioninfo,
            this,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        this.ColumnControl1.CommentColumn = app.CommentColumn;
        this.Response.Write("完成");
        this.Response.End();
        return;
    ERROR1:
        this.Response.Write(strError);
        this.Response.End();
    }

}