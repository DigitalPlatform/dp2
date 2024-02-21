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

public partial class ReservationInfo : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.ReservationInfo;
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
            this.ReservationInfoControl1.ReaderKey = strBarcode;
            this.FellBackInfoControl1.ReaderKey = strBarcode;
        }
    }

}