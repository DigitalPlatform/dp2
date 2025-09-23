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

public partial class BorrowInfo : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

#if NO
    protected override void InitializeCulture()
    {
        // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.chs/dv_aspnetcon/html/76091f86-f967-4687-a40f-de87bd8cc9a0.htm

        /*
        String selectedLanguage = TitleBarControl.GetLang(this, "langlist");

        this.UICulture = selectedLanguage;
        this.Culture = selectedLanguage;

        Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(selectedLanguage);
        Thread.CurrentThread.CurrentUICulture = new
            CultureInfo(selectedLanguage);
         * */
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

        ///
        this.TitleBarControl1.CurrentColumn = TitleColumn.BorrowInfo;

        if (app != null)
            this.BorrowHistoryControl1.DatabaseMode = string.IsNullOrEmpty(app.ChargingHistoryType) == false;
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
#if REMOEVD
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
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

        // 2011/9/2
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        string strBarcode = this.Request["barcode"];
        // if (string.IsNullOrEmpty(strBarcode) == false)
        {
            this.BorrowHistoryControl1.ReaderKey = strBarcode;
            this.BorrowInfoControl1.ReaderKey = strBarcode;
        }
    }
}