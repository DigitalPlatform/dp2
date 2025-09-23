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

public partial class PersonalInfo : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.PersonalInfo;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;

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

        // 2011/9/2
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");


        string strSideBarFile = Server.MapPath("./personalinfo_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        /*
        // 如果为单点统一认证状态，则需要禁止“修改密码”命令锚点
        if (IsSso() == true)
        {
            this.PersonalCommandControl1.DisableChangePassword = true;
        }
         * */

    }

#if NO
    // 当前是否为单点统一认证状态
    bool IsSso()
    {
        XmlNode node = app.OpacCfgDom.DocumentElement.SelectSingleNode("yczb/sso");
        if (node == null)
            return false;

        return true;
    }
#endif

}