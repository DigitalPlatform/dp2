using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Xml;
using System.Globalization;

using System.ServiceModel;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

public partial class Login : MyWebPage
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

        Thread.CurrentThread.CurrentUICulture = new
            CultureInfo(selectedLanguage);

        Thread.CurrentThread.CurrentCulture =
            CultureInfo.CreateSpecificCulture(selectedLanguage);

         * */

        WebUtil.InitLang(this);
        base.InitializeCulture();
    }
#endif

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref this.app,
ref this.sessioninfo) == false)
            return;

#if NO
        if (this.IsPostBack == false)
            this.GetLoginPanelInfo(this.LoginControl1);
#endif
    }

    protected void Page_Load(object sender, EventArgs e)
    {
#if NO
        this.LoginControl1.RestorePassword();

        if (this.LoginControl1.KeepLogin == false)
            this.SetCookiesLogin(null, null, -1, 0);
#endif
        if (WebUtil.PrepareEnvironment(this,
    ref app,
    ref sessioninfo) == false)
            return;

        string strError = "";

        if (this.IsPostBack == false)
        {
            string strLibraryCode = this.Request["library"];
            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // 强制设置图书馆代码
                this.TitleBarControl1.SelectedLibraryCode = strLibraryCode;
            }

            if (this.Request["action"] == "autologin"
                || this.Request.Form["action"] == "autologin")
            {
                string strUserName = this.Request["username"];
                if (string.IsNullOrEmpty(strUserName) == true)
                    strUserName = this.Request.Form["username"];

                // 2014/6/9
                if (string.IsNullOrEmpty(strUserName) == false)
                {
                    if (strUserName.IndexOf('%') != -1)
                        strUserName = HttpUtility.UrlDecode(strUserName);
                }

                // 2013/1/23
                if (string.IsNullOrEmpty(strUserName) == true)
                {
                    strError = "username参数不能为空";
                    goto ERROR1;
                }

                // 登录的途径 2013/1/28
                string strPrefix = this.Request["prefix"];
                if (string.IsNullOrEmpty(strPrefix) == true)
                    strPrefix = this.Request.Form["prefix"];

                if (string.IsNullOrEmpty(strPrefix) == false)
                    strUserName = strPrefix + strUserName;

                string strPassword = this.Request["password"];
                if (string.IsNullOrEmpty(strPassword) == true)
                    strPassword = this.Request.Form["password"];

                string strUserType = this.Request["usertype"];
                if (string.IsNullOrEmpty(strUserType) == true)
                    strUserType = this.Request.Form["usertype"];
                if (string.IsNullOrEmpty(strUserType) == true)
                    strUserType = "reader"; // 缺省为reader

                // parameters:
                //      strUserName 读者证条码号。或者 "NB:姓名|出生日期(8字符)" "EM:email地址" "TP:电话号码" "ID:身份证号"
                //      strUserType reader/librarian
                // return:
                //      -1  error
                //      0   成功
                int nRet = this.LoginControl1.DoLogin(
                   strUserName,
                   strPassword,
                   strUserType,
                   this.TitleBarControl1.SelectedLibraryCode,
                   out strError);
                if (nRet == -1)
                    return;

                // 登录成功后首次设置馆代码
                this.TitleBarControl1.SelectedLibraryCode = // sessioninfo.Channel.LibraryCodeList;
                    sessioninfo.LibraryCodeList;

                if (sessioninfo.LoginCallStack.Count != 0)
                {
                    string strUrl = (string)sessioninfo.LoginCallStack.Pop();
                    Redirect(strUrl);
                }
                else
                {
                    string strRedirect = Request.QueryString["redirect"];
                    if (string.IsNullOrEmpty(strRedirect) == true)
                        strRedirect = Request.Form["redirect"];
                    if (strRedirect == null || strRedirect == "")
                    {
                        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                        if (loginstate == LoginState.Public)
                            Redirect("searchbiblio.aspx");
                        else if (loginstate == LoginState.Reader)
                            Redirect("borrowinfo.aspx");	// 实在没有办法，就到主页面
                        else if (loginstate == LoginState.Librarian)
                            Redirect("searchbiblio.aspx");
                        else
                            Redirect("searchbiblio.aspx");
                    }
                    else
                        Redirect(strRedirect);
                }
                return;
            }

            if (this.Request["action"] == "logout")
            {
                string strSsoMainPageUrl = (string)this.Session["sso_mainpage_url"];

                // 先前用SSO接口登录的，现在要跳转到其登录页面
                if (string.IsNullOrEmpty(strSsoMainPageUrl) == false)
                {
                    Session.Abandon();
                    this.ClearCookiesLogin("online,token");
                    Response.Redirect(strSsoMainPageUrl, true);
                    this.Response.End();
                    return;
                }


                {
                    // 判断当前是不是yczb sso状态
                    HttpCookie cookie = Request.Cookies.Get("iPlanetDirectoryPro");
                    if (cookie == null)
                        goto DONE;

                    string strSsoToken = cookie.Value;
                    if (String.IsNullOrEmpty(strSsoToken) == true)
                        goto DONE;

                    string strSsoPageUrl = "";
                    // return:
                    //      -1  error
                    //      0   没有找到 ssoPageUrl 属性
                    //      1   找到
                    int nRet = GetYczbLoginPageUrl(out strSsoPageUrl,
                        out strError);
                    if (nRet == -1)
                    {
                        Session.Abandon();
                        /*
                        Response.Write(HttpUtility.HtmlEncode(strError));
                        this.Response.End();
                        return;
                         * */
                        goto ERROR1;
                    }

                    Session.Abandon();
                    if (nRet == 1)
                        Response.Redirect(strSsoPageUrl, true); // 2009/9/24 changed
                    this.Response.End();
                    return;
                }

            DONE:
                // LogoutSsoCookie(sessioninfo.UserID);

                Session.Abandon();
                this.ClearCookiesLogin("online,token");

                string strPureUserName = "";
                string strDomain = "";
                bool bRet = ParseUserName(sessioninfo.UserID,
        out strPureUserName,
        out  strDomain);
                // 如果是sso登录方式，需要转到相应domain的login.aspx，再做一次logout
                if (bRet == true
                    && HasConfigDp2Sso() == true)
                {
                    List<SsoInfo> infos = null;
                    // return:
                    //      -1  error
                    //      0   succeed
                    int nRet = GetDp2SSoInfos(
                        strDomain,
                        out infos,
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }

                    // 跳转到指定的URL进行登出
                    if (infos.Count > 0)
                    {
                        string strRedirectUrl = "";
                        if (sessioninfo.LoginCallStack.Count != 0)
                            strRedirectUrl = (string)sessioninfo.LoginCallStack.Pop();
                        else
                        {
                            strRedirectUrl = Request.QueryString["redirect"];
                            if (string.IsNullOrEmpty(strRedirectUrl) == true)
                                strRedirectUrl = Request.Form["redirect"];
                        }

                        string strUrl = infos[0].LogoutUrl;
                        strUrl = strUrl.Replace("%redirect%", HttpUtility.UrlEncode(strRedirectUrl));

                        Response.Redirect(strUrl, true);
                        this.Response.End();
                        return;
                    }

                }

                this.Redirect("login.aspx");
                this.Response.End();
                return;
            }

            if (this.Request["action"] == "ssologin")
            {
                string strType = this.Request["type"];
                string strSsoLibraryCode = this.Request["library"];

                if (string.IsNullOrEmpty(strType) == true)
                {
#if NO
                    // return:
                    //      -1  发生错误
                    //      1   成功
                    int nRet = DoSsoLogin(strSsoLibraryCode);
                    if (nRet == -1)
                        return;
#endif
                    throw new Exception("需要创建具体 type 的 SSO 接口");
                }
                else
                {
                    // return:
                    //      -1  发生错误
                    //      1   成功
                    int nRet = DoSsoLogin(strSsoLibraryCode, strType);
                    if (nRet == -1)
                        return;
                }

                Response.End();
                return;
            }

            // 访客登录
            if (this.Request["action"] == "publiclogin")
            {
                LoginControl1_AnonymouseLogin(this, new LoginEventArgs());
                return;
            }

            string strMessage = this.Request.QueryString["message"];
            if (String.IsNullOrEmpty(strMessage) == false)
            {
                this.LoginControl1.SetDebugInfo(strMessage);
            }

            if (String.IsNullOrEmpty(this.Request["loginstyle"]) == false)
            {
                this.LoginControl1.LoginStyle = LoginStyle.None;

                // bool bChanged = false;
                if (StringUtil.IsInList("librarian", this.Request["loginstyle"], true) == true)
                {
                    this.LoginControl1.LoginStyle |= LoginStyle.Librarian;
                    // bChanged = true;
                }

                if (StringUtil.IsInList("reader", this.Request["loginstyle"], true) == true)
                {
                    this.LoginControl1.LoginStyle |= LoginStyle.Reader;
                    // bChanged = true;
                }

#if NO
                if (bChanged == true)
                    this.LoginControl1.AdjustActiveColumn();
#endif
            }
            else
            {

                if (this.IsPostBack == false)
                    this.GetLoginPanelInfo(this.LoginControl1);
            }

            /*
            this.LoginControl1.Title = "数字平台公共查询";
            this.LoginControl1.TitleFontSize = 26.0F;
             * */
        }

        this.LoginControl1.RestorePassword();

        if (this.LoginControl1.KeepLogin == false)
            this.SetCookiesLogin(null, null, -1, 0);

        if (StringUtil.IsInList("librarian", this.Request["loginstyle"], true) == true)
        {
            return; // 不进行后面的IsSso()判断了
        }

        // 优先进行sso登录
        string strDp2Sso = Request.QueryString["dp2sso"];
        if (HasConfigDp2Sso() == true && strDp2Sso == "first")
        {
            List<SsoInfo> infos = null;
            // return:
            //      -1  error
            //      0   succeed
            int nRet = GetDp2SSoInfos(
                "*",
                out infos,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 跳转到指定的URL进行登录
            if (infos.Count > 0)
            {
                // TODO: 多于一个要允许选择
                string strRedirectUrl = "";
                if (sessioninfo.LoginCallStack.Count != 0)
                    strRedirectUrl = (string)sessioninfo.LoginCallStack.Pop();
                else
                {
                    strRedirectUrl = Request.QueryString["redirect"];
                    if (string.IsNullOrEmpty(strRedirectUrl) == true)
                        strRedirectUrl = Request.Form["redirect"];
                }

                string strUrl = infos[0].LoginUrl;
                strUrl = strUrl.Replace("%redirect%", HttpUtility.UrlEncode(strRedirectUrl));

                Response.Redirect(strUrl, true);
                this.Response.End();
                return;
            }
        }

        // 在要求统一认证的情况下，需要将页面重定向到统一认证的那个登录页面
        if (IsYczbSso() == true)
        {
            string strSsoPageUrl = "";
            // return:
            //      -1  error
            //      0   没有找到 ssoPageUrl 属性
            //      1   找到
            int nRet = GetYczbLoginPageUrl(out strSsoPageUrl,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                Response.Redirect(strSsoPageUrl, true); // 2009/9/24 changed
                this.Response.End();
                return;
            }
        }

        // 获得图书馆名 设置页面标题
        XmlNode node = app.OpacCfgDom.DocumentElement.SelectSingleNode("libraryInfo/libraryName");
        if (node != null)
        {
            string strCaption = DomUtil.GetCaption(Lang, node);
            if (string.IsNullOrEmpty(strCaption) == true)
                this.Page.Title += " " + node.InnerText;
            else
                this.Page.Title += " " + strCaption;
        }

        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        this.Response.End();
    }

    // return:
    //      false   不是SSO形态的usernname
    //      true    是SSO形式
    static bool ParseUserName(string strUserNameString,
        out string strPureUserName,
        out string strDomain)
    {
        strPureUserName = "";
        strDomain = "";

        int nRet = strUserNameString.IndexOf("@");
        if (nRet == -1)
        {
            strPureUserName = strUserNameString;
            return false;
        }
        strPureUserName = strUserNameString.Substring(0, nRet);
        strDomain = strUserNameString.Substring(nRet + 1);
        return true;
    }

#if NO
    // 登出后，对Cookie中的信息进行修改，以便让sso信息变成登出的状态
    // parameters:
    void LogoutSsoCookie(
        string strUserNameString)
    {
        int nRet = strUserNameString.IndexOf("@");
        if (nRet == -1)
            return;

        string strPureUserName = strUserNameString.Substring(0, nRet);
        string strCurrentDomain = strUserNameString.Substring(nRet + 1);

        HttpCookie cookie = this.Request.Cookies["dp2-sso"];
        if (cookie != null)
        {
            Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");
            string strDomain = (string)table["domain"];

            if (strDomain != strCurrentDomain)
                return; // 如果cookie中的域不是当前登出用户的域，就没有必要处理了

            string strUserName = (string)table["username"];
            if (strPureUserName != strUserName)
                return;

            string strLoginState = (string)table["loginstate"]; // in/out

            // 经过验证后，转换为SessionInfo状态
            if (strLoginState == "in")
            {
                table["loginstate"] = "out";
                string strValue = StringUtil.BuildParameterString(table, ',', '=', "url");
                HttpCookie new_cookie = new HttpCookie("dp2-sso", strValue);
                this.Response.SetCookie(new_cookie);
            }
        }
    }
#endif

    static void AppendParameter(ref string strUrl,
        string strParameter)
    {
        int nRet = strUrl.IndexOf("?");
        if (nRet == -1)
        {
            strUrl = strUrl + "?" + strParameter;
            return;
        }

        strUrl = strUrl + "&" + strParameter;
    }

    void Redirect(string strRedirect)
    {
        if (app.UseTransfer == true
            && strRedirect.ToLower() != "login.aspx")
        {
            // 注意，不允许 Transfer 到当前 aspx。这样会引起 IIS Express 崩溃
            Server.Transfer(strRedirect);
            return;
        }

        string serverName =
            HttpUtility.UrlEncode(Request.ServerVariables["SERVER_NAME"]);

        string serverPort =
            HttpUtility.UrlEncode(Request.ServerVariables["SERVER_PORT"]);

        string strPort = "";

        if (this.Request.IsSecureConnection == true)
            strPort = ":80";    // 最好在配置文件中定义http端口号
        else
        {
            if (serverPort != "80")
                strPort = ":" + serverPort;
        }

        string strUrl = "";

        string strLang = Thread.CurrentThread.CurrentUICulture.Name;

        if (strRedirect[0] == '/')
        {
            strUrl = "http://" + serverName + strPort + strRedirect;

            /*
            AppendParameter(ref strUrl, 
                "lang=" + strLang);
             * */

            // Response.Write("strPort=" + strPort + "<br/>");

        }
        else
        {
            string vdirName = Request.ApplicationPath;

            strUrl = "http://" + serverName + strPort
                + vdirName + "/" + strRedirect;

            /*
            AppendParameter(ref strUrl,
                "lang=" + strLang);
             * */

            // Response.Write("vdirName=" + vdirName + "<br/>");
        }

        Response.Redirect(strUrl, true);
        this.Response.End();
        /*
        Response.Write("url=" + strUrl);
        Response.End();
         * */
    }

    protected void LoginControl1_Login(object sender, LoginEventArgs e)
    {
        string strError = "";
        int nRet = this.LoginControl1.DoLogin(this.TitleBarControl1.SelectedLibraryCode,
            out strError);

        if (nRet == -1)
            return;

        // 首次设置馆代码
        this.TitleBarControl1.SelectedLibraryCode = // sessioninfo.Channel.LibraryCodeList;
            sessioninfo.LibraryCodeList;

        if (sessioninfo.LoginCallStack.Count != 0)
        {
            string strUrl = (string)sessioninfo.LoginCallStack.Pop();
            Redirect(strUrl);
        }
        else
        {
            string strRedirect = Request.QueryString["redirect"];
            if (strRedirect == null || strRedirect == "")
            {
                LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                if (loginstate == LoginState.Public)
                    Redirect("searchbiblio.aspx");
                else if (loginstate == LoginState.Reader)
                    Redirect("borrowinfo.aspx");	// 实在没有办法，就到主页面
                else if (loginstate == LoginState.Librarian)
                    Redirect("searchbiblio.aspx");
                else
                    Redirect("searchbiblio.aspx");
            }
            else
                Redirect(strRedirect);
            return;
        }
    }

    protected void LoginControl1_AnonymouseLogin(object sender, LoginEventArgs e)
    {
        string strError = "";
        int nRet = this.LoginControl1.DoAnonymouseLogin(out strError);

        if (nRet == -1)
            return;

        this.ClearCookiesLogin("all");

        if (sessioninfo.LoginCallStack.Count != 0)
        {
            string strUrl = (string)sessioninfo.LoginCallStack.Pop();
            Redirect(strUrl);
        }
        else
        {
            string strRedirect = Request.QueryString["redirect"];
            if (strRedirect == null || strRedirect == "")
            {
                LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                if (loginstate == LoginState.Public)
                    Redirect("searchbiblio.aspx");
                else if (loginstate == LoginState.Reader)
                    Redirect("borrowinfo.aspx");	// 实在没有办法，就到主页面
                else if (loginstate == LoginState.Librarian)
                    Redirect("searchbiblio.aspx");
                else
                    Redirect("searchbiblio.aspx");
            }
            else
                Redirect(strRedirect);
            return;
        }
    }

    // 当前是否为单点统一认证状态
    bool HasConfigDp2Sso()
    {
        XmlNode node = app.OpacCfgDom.DocumentElement.SelectSingleNode("dp2sso/domain");
        if (node == null)
            return false;

        return true;
    }

    /*
<dp2sso>
<domain name='dp2bbs' loginUrl='http://dp2003.com/dp2bbs/login.aspx?redirect=%redirect%' logoutUrl='' />
</dp2sso>
     * */
    // return:
    //      -1  error
    //      0   succeed
    int GetDp2SSoInfos(
        string strDomain,
        out List<SsoInfo> infos,
        out string strError)
    {
        strError = "";
        infos = new List<SsoInfo>();

        XmlNodeList nodes = app.OpacCfgDom.DocumentElement.SelectNodes("dp2sso/domain");
        if (nodes.Count == 0)
        {
            strError = "opac.xml中尚未配置<dp2sso/domain>元素";
            return 0;
        }

        foreach (XmlNode node in nodes)
        {
            string strCurrentDomain = DomUtil.GetAttr(node, "name");
            if (strDomain != "*" && strCurrentDomain != strDomain)
                continue;
            SsoInfo info = new SsoInfo();
            info.Domain = strCurrentDomain;
            info.LoginUrl = DomUtil.GetAttr(node, "loginUrl");
            info.LogoutUrl = DomUtil.GetAttr(node, "logoutUrl");
            infos.Add(info);
        }
        return infos.Count;
    }

    // 当前是否为单点统一认证状态
    bool IsYczbSso()
    {
        XmlNode node = app.OpacCfgDom.DocumentElement.SelectSingleNode("yczb/sso");
        if (node == null)
            return false;

        return true;
    }

    // return:
    //      -1  error
    //      0   没有找到 ssoPageUrl 属性
    //      1   找到
    int GetYczbLoginPageUrl(out string strSsoPageUrl,
        out string strError)
    {
        strError = "";
        strSsoPageUrl = "";

        XmlNode node = app.OpacCfgDom.DocumentElement.SelectSingleNode("yczb/sso");
        if (node == null)
        {
            strError = "opac.xml中尚未配置<yczb/sso>元素";
            return -1;
        }

        strSsoPageUrl = DomUtil.GetAttr(node, "ssoPageUrl");
        if (String.IsNullOrEmpty(strSsoPageUrl) == true)
        {
            // strError = "opac.xml中<yczb/sso>元素内缺乏ssoPageUrl属性";
            // return -1;
            return 0;
        }

        return 1;
    }

    // 单点登录
    // return:
    //      -1  发生错误
    //      1   成功
    int DoSsoLogin(string strLibraryCode, string strType)
    {
        string strError = "";
        int nRet = 0;
        string strSsoPageUrl = "";

        SsoInterface sso_interface = app.GetSsoInterface(strType);
        if (sso_interface == null)
        {
            strError = "当前没有配置类型为 '" + strType + "' 的 SSO 接口";
            goto ERROR1;
        }

        string strLoginName = "";
        nRet = sso_interface.HostObj.GetUserInfo(
            this.Page,
            out strLoginName,
            out strSsoPageUrl,
            out strError);
        if (nRet == -1)
        {
            strError = "GetUserInfo() error : " + strError;
            goto ERROR1;
        }

        if (string.IsNullOrEmpty(strLoginName) == true)
        {
            strError = "sso_interface.HostObj.GetUserInfo() 所获得的 strLoginName 不应为空";
            goto ERROR1;
        }

        string strParameters = "location=#opac_sso@" + sessioninfo.ClientIP + ",index=-1,type=reader,simulate=yes,libraryCode=" + LoginControl.GetLibraryCodeParam(strLibraryCode) + ",client=dp2OPAC|" + OpacApplication.ClientVersion;
        string strPassword = app.ManagerUserName + "," + app.ManagerPassword;   // simulate登录的需要
        // 读者身份登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        //      >1  有多个账户符合条件。
        long lRet = sessioninfo.Login(
                    strLoginName,
                    strPassword,
                    strParameters,
                    "",
                    out strError);


        // sessioninfo.Channel.Close();

        if (lRet == -1 || lRet == 0)    // lRet == 0 是增加的部分 2014/12/20
        {
            strError = "对图书馆读者帐户 '" + strLoginName + "' 进行登录时出错：" + strError;
            goto ERROR1;
        }
        if (lRet > 1)
        {
            strError = "登录中发现有 " + lRet.ToString() + " 个账户符合条件，登录失败";
            goto ERROR1;
        }

        // 表示 SSO 登录成功
        this.Session["sso_mainpage_url"] = strSsoPageUrl;

        if (sessioninfo.LoginCallStack.Count != 0)
        {
            string strUrl = (string)sessioninfo.LoginCallStack.Pop();
            Redirect(strUrl);
            return 1;
        }
        else
        {
            string strRedirect = Request.QueryString["redirect"];
            if (strRedirect == null || strRedirect == "")
            {
                LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                if (loginstate == LoginState.Public)
                    Redirect("searchbiblio.aspx");
                else if (loginstate == LoginState.Reader)
                    Redirect("borrowinfo.aspx");	// 实在没有办法，就到主页面
                else if (loginstate == LoginState.Librarian)
                    Redirect("searchbiblio.aspx");
                else
                    Redirect("searchbiblio.aspx");
            }
            else
                Redirect(strRedirect);
            return 1;
        }

    ERROR1:
        if (string.IsNullOrEmpty(strSsoPageUrl) == true)
        {
            Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p>"
                + "</body></html>");
            Response.End();
        }
        else
        {
            Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p>"
                + "<p><a href=" + strSsoPageUrl + " >回到统一认证登录页面</a></p>"
                + "</body></html>");
            Response.End();
        }
        return -1;
    }


#if NO 
    测试用
    int DoSsoLogin()
    {
        string strError = "";
        int nRet = 0;

        string strSsoPageUrl = "";
        string strCardNumber = "1234";

        string strParameters = "location=#web,index=-1,type=reader,simulate=yes";
        string strPassword = app.ManagerUserName + "," + app.ManagerPassword;   // simulate登录的需要
        // 读者身份登录
        long lRet = sessioninfo.Login(
                    "CN:" + strCardNumber,
                    strPassword,
                    strParameters,
                    out strError);
        if (lRet == -1)
        {
            strError = "对图书馆读者帐户进行登录时出错：" + strError;
            goto ERROR1;
        }
        if (lRet > 1)
        {
            strError = "登录中发现有 " + lRet.ToString() + " 个账户符合条件，登录失败";
            goto ERROR1;
        }

        if (sessioninfo.LoginCallStack.Count != 0)
        {
            string strUrl = (string)sessioninfo.LoginCallStack.Pop();
            Redirect(strUrl);
            return 1;
        }
        else
        {
            string strRedirect = Request.QueryString["redirect"];
            if (strRedirect == null || strRedirect == "")
            {
                LoginState loginstate = Global.GetLoginState(this.Page);

                if (loginstate == LoginState.Public)
                    Redirect("searchbiblio.aspx");
                else if (loginstate == LoginState.Reader)
                    Redirect("borrowinfo.aspx");	// 实在没有办法，就到主页面
                else if (loginstate == LoginState.Librarian)
                    Redirect("searchbiblio.aspx");
                else
                    Redirect("searchbiblio.aspx");
            }
            else
                Redirect(strRedirect);
            return 1;
        }

    ERROR1:
        Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p>"
            + "</body></html>");
        Response.End();
        return -1;
    ERROR2:
        Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p>"
            + "<p><a href=" + strSsoPageUrl + " >回到统一认证登录页面</a></p>"
            + "</body></html>");
        Response.End();
        return -1;
    }
#endif

    public static System.ServiceModel.Channels.Binding CreateBasicHttpBinding0()
    {
        BasicHttpBinding binding = new BasicHttpBinding();
        binding.Security.Mode = BasicHttpSecurityMode.None;
        /*
        binding.MaxReceivedMessageSize = 1024 * 1024;
        binding.MessageEncoding = WSMessageEncoding.Mtom;
        XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
        quotas.MaxArrayLength = 1024 * 1024;
        quotas.MaxStringContentLength = 1024 * 1024;
        binding.ReaderQuotas = quotas;
        binding.SendTimeout = new TimeSpan(0, 2, 0);
        binding.ReceiveTimeout = new TimeSpan(0, 2, 0);
        */
        return binding;
    }

    public class SsoInfo
    {
        public string Domain = "";
        public string LoginUrl = "";
        public string LogoutUrl = "";
    }
}