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
using DigitalPlatform.LibraryClient;

public partial class ChangeReaderPassword : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.PersonalInfo;
        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

        // 是否登录?
        // if (sessioninfo.UserID == "")
        if (loginstate == LoginState.NotLogin || loginstate == LoginState.Public)
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }

        string strSideBarFile = Server.MapPath("./personalinfo_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        // 2013/11/2
        if (sessioninfo.ReaderInfo == null
            && sessioninfo.IsReader == true)
        {
            string strError = "";
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
        }
        if (sessioninfo.ReaderInfo != null)
            this.TextBox_readerBarcode.Text = sessioninfo.ReaderInfo.ReaderDisplayKey;
    }

    protected void Button_changePassword_Click(object sender, EventArgs e)
    {
        if (this.TextBox_newPassword.Text != this.TextBox_confirmNewPassword.Text)
        {
            this.Label_message.Text = "新密码　和　再输入一遍新密码　不一致。请重新输入。";
            return;
        }

        LibraryChannel channel = sessioninfo.GetChannel(true);
        try
        {
            string strError = "";
            // Result.Value
            //      -1  出错
            //      0   旧密码不正确
            //      1   旧密码正确,已修改为新密码
            long lRet = // sessioninfo.Channel.
                channel.ChangeReaderPassword(
                null,
                this.TextBox_readerBarcode.Text,
                this.TextBox_oldPassword.Text,
                this.TextBox_newPassword.Text,
                out strError);
            if (lRet != 1)  // 2008/9/12 changed
            {
                this.Label_message.Text = strError;
                return;
            }
        }
        finally
        {
            sessioninfo.ReturnChannel(channel);
        }

        sessioninfo.Password = this.TextBox_newPassword.Text;
        // 注：这里登出，迫使读者重新登录，也是可以的做法

        this.Label_message.Text = "密码修改成功。";
    }


}