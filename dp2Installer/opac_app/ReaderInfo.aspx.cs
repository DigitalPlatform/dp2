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
using DigitalPlatform.CirculationClient;

public partial class ReaderInfo : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.ReaderInfo;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // 是否登录?
        if (sessioninfo.UserID == ""
            || (sessioninfo.ReaderInfo != null && sessioninfo.IsReader == true))   // 只准管理员身份使用
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx?loginstyle=librarian", true);
            return;
        }

        string strReaderBarcode = this.Request["barcode"];

        if (String.IsNullOrEmpty(strReaderBarcode) == true)
        {
            Response.Write("调用readerinfo.aspx时缺乏barcode参数");
            Response.End();
            return;
        }

        this.PersonalInfoControl1.ReaderBarcode = strReaderBarcode;
        this.BorrowInfoControl1.ReaderBarcode = strReaderBarcode;
        this.ReservationInfoControl1.ReaderBarcode = strReaderBarcode;
        this.FellBackInfoControl1.ReaderBarcode = strReaderBarcode;
        this.BorrowHistoryControl1.ReaderBarcode = strReaderBarcode;

        this.TitleBarControl1.ReaderBarcode = strReaderBarcode;

        // 防止前端浏览器缓存本页面
        this.Response.AddHeader("Cache-Control", "no-store");
        this.Response.AddHeader("Pragrma", "no-cache");
        this.Response.AddHeader("Expires", "0");

    }
}