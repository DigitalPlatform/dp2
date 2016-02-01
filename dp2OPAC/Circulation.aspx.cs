using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
// using DigitalPlatform.CirculationClient;

namespace WebApplication1
{
    public partial class Circulation : MyWebPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (WebUtil.PrepareEnvironment(this,
    ref app,
    ref sessioninfo) == false)
                return;

            this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.BorrowInfo;
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

                // sessioninfo.UserID = "public";
                // sessioninfo.IsReader = false;

                // 迫使用馆员身份登录
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx?loginstyle=librarian", true);
                return;
            }


        }
    }
}