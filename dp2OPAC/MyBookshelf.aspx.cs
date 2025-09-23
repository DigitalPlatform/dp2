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

public partial class MyBookshelf : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.MyBookShelf;

        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

        this.BrowseSearchResultControl1.DefaultFormatName = "详细";

        this.BrowseSearchResultControl1.EnableAddToMyBookshelf = false;
        this.BrowseSearchResultControl1.EnableRemoveFromMyBookshelf = true;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        this.BrowseSearchResultControl1.ResetAllItemsControlPager();
    }

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
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
#endif
            var url = GetDefaultLoginUrl();
            if (url != null)
            {
                Response.Redirect(url, true);
                return;
            }
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }

        if (this.IsPostBack == false)
        {
            string strResultsetFilename = CacheBuilder.GetMyBookshelfFilename(
                app,
                sessioninfo);
            if (File.Exists(strResultsetFilename) == true)
            {
                long lHitCount = CacheBuilder.GetCount(app, strResultsetFilename, true);

                this.BrowseSearchResultControl1.ResultsetFilename = strResultsetFilename;
                this.BrowseSearchResultControl1.ResultCount = (int)lHitCount;
            }
            else
            {
                this.BrowseSearchResultControl1.Title = "我的书架 目前 内容为空";
            }
        }

        this.BrowseSearchResultControl1.Title = (string)this.GetLocalResourceObject("我的书架");

    }
}