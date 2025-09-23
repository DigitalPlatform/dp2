using System;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Globalization;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using DigitalPlatform.IO;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.Drawing;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// Summary description for WebUtil
    /// </summary>
    public class WebUtil
    {
        // 文字图片
        public static MemoryStream TextImage(
            ImageFormat image_format,
            string strText,
            System.Drawing.Color text_color,
            System.Drawing.Color back_color,
            float fFontSize = 10,
            int nWidth = 300)
        {
            return ArtText.BuildArtText(
                strText,
                "Microsoft YaHei",
                fFontSize,
                FontStyle.Regular,
                text_color,
                back_color,
                Color.Gray,
                ArtEffect.None,
                image_format,
                nWidth);
        }

        public static void InitLang(Page page)
        {
            // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.chs/dv_aspnetcon/html/76091f86-f967-4687-a40f-de87bd8cc9a0.htm

            String selectedLanguage = TitleBarControl.GetLang(page, "langlist");

            page.UICulture = selectedLanguage;
            page.Culture = selectedLanguage;

            Thread.CurrentThread.CurrentCulture =
                CultureInfo.CreateSpecificCulture(selectedLanguage);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(selectedLanguage);

            page.Session["lang"] = Thread.CurrentThread.CurrentUICulture.Name;
        }

        public static bool PrepareEnvironment(Page page,
            ref OpacApplication app,
            ref SessionInfo sessioninfo)
        {
            string strErrorInfo = "";

            // 获得app对象
            // 并检查系统错误字符串

            // 2008/6/6
            // 错误信息采用两级存放策略。
            // 如果LibraryAppliation对象已经存在，则采用其ErrorInfo成员的值；
            // 否则，采用Application["errorinfo"]值
            app = (OpacApplication)page.Application["app"];
            if (app == null)
            {
                strErrorInfo = (string)page.Application["errorinfo"];

                if (String.IsNullOrEmpty(strErrorInfo) == true)
                    strErrorInfo = "app == null";

                page.Response.Write(HttpUtility.HtmlEncode(strErrorInfo).Replace("\r\n", "<br/>"));
                page.Response.End();
                return false;
            }
            else
            {
                strErrorInfo = app.GlobalErrorInfo;
                if (String.IsNullOrEmpty(strErrorInfo) == false)
                {
                    page.Response.Write(HttpUtility.HtmlEncode(strErrorInfo).Replace("\r\n", "<br/>"));
                    page.Response.End();
                    return false;
                }

                if (app.XmlLoaded == false)
                {
                    strErrorInfo =
                        "<html><body><pre>" +
                        HttpUtility.HtmlEncode("OPAC 初始化时装载 XmlDefs 失败，可能的原因是：OPAC 所依赖的 dp2Library 服务模块尚未启动，或 OPAC 代理帐户不存在、权限不够或密码被修改...。\r\n具体出错原因请察看 dp2OPAC 数据目录 log 子目录下的当日日志文件(log_????????.txt)，并及时排除故障。OPAC 系统将在稍后自动重试装载 XmlDefs。");
                    page.Response.Write(strErrorInfo);

                    if (page.Request.UserHostAddress == "localhost"
                        || page.Request.UserHostAddress == "::1")
                    {
                        page.Response.Write("\r\n\r\n");
                        // 输出当天日志文件内容
                        app.DumpErrorLog(page);
                    }

                    page.Response.Write("</pre></body></html>");
                    page.Response.End();
                    return false;
                }
            }

            /*
            string strErrorInfo = (string)page.Application["errorinfo"];

            if (String.IsNullOrEmpty(strErrorInfo) == false)
            {
                page.Response.Write(HttpUtility.HtmlEncode(strErrorInfo).Replace("\r\n","<br/>"));
                page.Response.End();
                return false;   // error
            }*/

            // 获得SessionInfo
            sessioninfo = (SessionInfo)page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                /*
                strErrorInfo = "sessioninfo == null";
                page.Response.Write(strErrorInfo);
                page.Response.End();
                return false;
                 * */

                // 2020/10/29
                string strClientIP = HttpContext.Current.Request.Headers.Get("X-Real-IP");

                if (strClientIP == null)
                {
                    // 2013/12/7
                    strClientIP = HttpContext.Current.Request.UserHostAddress.ToString();
                }

                // 增量计数
                if (app != null)
                {
                    long v = app.IpTable.IncIpCount(strClientIP, 1);
                    if (v >= app.IpTable.MAX_SESSIONS_PER_IP)
                    {
                        app.IpTable.IncIpCount(strClientIP, -1);
                        strErrorInfo = "同一 IP 前端数量超过配额 " + app.IpTable.MAX_SESSIONS_PER_IP.ToString() + "。请稍后再试。";
                        page.Response.StatusCode = 403;
                        // page.Response.StatusDescription = strErrorInfo;
                        page.Response.Write(strErrorInfo);
                        page.Response.End();
                        // TODO: 也可以 redirect 引导到一个说明性的页面
                        return false;
                    }
                }

                try
                {
                    sessioninfo = new SessionInfo(app);
                    sessioninfo.ClientIP = strClientIP;
                    page.Session["sessioninfo"] = sessioninfo;
                }
                catch (Exception ex)
                {
                    strErrorInfo = "PrepareEnvironment()创建Session出现异常: " + ExceptionUtil.GetDebugText(ex);
                    page.Response.Write(strErrorInfo);
                    page.Response.End();
                    return false;
                }
            }

            string strLang = (string)page.Session["lang"];
            sessioninfo.ChannelLang = strLang;

            if (page is MyWebPage)
            {
                (page as MyWebPage).LoadCookiesLoginToSession();
            }

            return true;
        }

        public static bool PrepareEnvironment(Page page,
    ref OpacApplication app)
        {
            // 检查系统错误字符串
            string strErrorInfo = (string)page.Application["errorinfo"];

            if (String.IsNullOrEmpty(strErrorInfo) == false)
            {
                page.Response.Write(strErrorInfo);
                page.Response.End();
                return false;   // error
            }

            // 获得app对象
            app = (OpacApplication)page.Application["app"];
            if (app == null)
            {
                strErrorInfo = "app == null";
                page.Response.Write(strErrorInfo);
                page.Response.End();
                return false;
            }

            return true;
        }
    }

    public class MyWebPage : System.Web.UI.Page
    {
        public OpacApplication app = null;
        public SessionInfo sessioninfo = null;

        public bool MyFlushOutput()
        {
            // 2016/1/28
            if (Response.IsClientConnected == false)
                return false;
            Response.Flush();
            return Response.IsClientConnected;
        }

        void PrepareSsoLogin()
        {
            HttpCookie cookie = this.Request.Cookies["dp2-sso"];
            if (cookie != null)
            {
                Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");
                string strDomain = (string)table["domain"];

                if (strDomain == "dp2opac")
                    return; // 如果原始域来自dp2opac，就没有必要处理了

                string strUserName = (string)table["username"];
                string strLoginState = (string)table["loginstate"]; // in/out
                string strPhotoUrl = (string)table["photourl"];
                string strRights = (string)table["rights"];

                // 经过验证后，转换为SessionInfo状态
                if (strLoginState == "in")
                {
                    if (sessioninfo.UserID != strUserName + "@" + strDomain)
                    {
                        sessioninfo.UserID = strUserName + "@" + strDomain;
                        sessioninfo.PhotoUrl = strPhotoUrl;
                        sessioninfo.ReaderInfo = null;
                        sessioninfo.SsoRights = strRights;
                    }
                }
                else
                {
                    // 及时反映登出状态
                    if (sessioninfo.UserID.IndexOf("@") != -1)
                        sessioninfo.UserID = "";
                }
            }
        }

        // 将 Cookies 里面的信息装入 SessionInfo
        // 如果 keeplogin 为 false，则不装入
        public void LoadCookiesLoginToSession()
        {
            if (string.IsNullOrEmpty(sessioninfo.UserID) == false)
                return;

            HttpCookie cookie = this.Request.Cookies["opac-login"];
            if (cookie != null)
            {
                bool bIsReader = true;
                Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");
                string strLoginName = (string)table["loginname"];
                if (StringUtil.HasHead(strLoginName, "WK:") == true)
                    bIsReader = false;
                string strID = (string)table["id"];

                string strKeepLogin = (string)table["keeplogin"];
                bool bKeepLogin = DomUtil.IsBooleanTrue(strKeepLogin, false);

                string strOnline = (string)table["online"];
                bool bOnline = DomUtil.IsBooleanTrue(strOnline, false);

                if (string.IsNullOrEmpty(strID) == false
                    && bOnline == true && bKeepLogin == true)
                {
#if NO
                    // 工作人员帐号
                    if (StringUtil.HasHead(strID, "WK:") == true)
                    {
                        strID = strID.Substring("WK:".Length);
                        sessioninfo.IsReader = false;
                    }
#endif
                    // TODO: _rights 没有恢复。
                    sessioninfo.IsReader = bIsReader;

                    sessioninfo.UserID = strID;
                    sessioninfo.Password = "token:" + (string)table["token"];
                }
            }
        }

        public void GetLoginPanelInfo(LoginControl control)
        {
            HttpCookie cookie = this.Request.Cookies["opac-login"];
            if (cookie != null)
            {
                Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");
#if NO
                string strID = (string)table["id"];
                if (string.IsNullOrEmpty(strID) == false)
                {
                    string strKeepLogin = (string)table["keeplogin"];
                    bool bKeepLogin = DomUtil.IsBooleanTrue(strKeepLogin, false);

                    string strPassword = "token:" + (string)table["token"];
                    if (bKeepLogin == false)
                        strPassword = "";

                    if (bKeepLogin == true)
                        control.SetValue(strID, strPassword, bKeepLogin);
                    return;
                }
#endif
                string strLoginName = (string)table["loginname"];
                if (string.IsNullOrEmpty(strLoginName) == false)
                {
                    string strKeepLogin = (string)table["keeplogin"];
                    bool bKeepLogin = DomUtil.IsBooleanTrue(strKeepLogin, false);

                    string strToken = (string)table["token"];

                    string strPassword = "";

#if NO
                    if (string.IsNullOrEmpty(strToken) == false)
                        strPassword = "token:" + (string)table["token"];
                    if (bKeepLogin == false)
                        strPassword = "";
#endif

                    if (bKeepLogin == true)
                        control.SetValue(strLoginName, strPassword, bKeepLogin);
                    return;
                }
            }
        }

        // return:
        //      不包含 token: 头部
        static string FindTokenString(string strRights)
        {
            string[] parts = strRights.Split(new char[] { ',' });
            foreach (string text in parts)
            {
                if (StringUtil.HasHead(text, "token:") == true)
                {
                    return text.Substring("token:".Length);
                }
            }

            return null;
        }

        static DateTime GetInstantExpireTime()
        {
            return DateTime.Now.AddHours(1);
            // return DateTime.Now;
        }

        public void ClearCookiesLogin(string strLevel)
        {
            // this.Response.Cookies.Remove("opac-login");
            if (strLevel == "all")
            {
                HttpCookie cookie = this.Response.Cookies["opac-login"];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-1);
                }
            }
            else if (StringUtil.IsInList("keeplogin", strLevel) == true
                || StringUtil.IsInList("password", strLevel) == true
                || StringUtil.IsInList("token", strLevel) == true
                || StringUtil.IsInList("online", strLevel) == true
                || StringUtil.IsInList("loginname", strLevel) == true)   // 只清除部分参数
            {
                DateTime expire_time = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0);  // 一年以后失效

                HttpCookie cookie = this.Response.Cookies["opac-login"];
                if (cookie == null || cookie.Value == null)
                {
                    cookie = this.Request.Cookies["opac-login"];
                    if (cookie != null)
                        this.Response.Cookies.Set(cookie);
                }

                if (cookie != null)
                {
                    Hashtable table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");

                    if (StringUtil.IsInList("keeplogin", strLevel) == true)
                    {
                        table.Remove("keeplogin");
                        expire_time = GetInstantExpireTime();
                    }
                    if (StringUtil.IsInList("password", strLevel) == true
                        || StringUtil.IsInList("token", strLevel) == true)
                        table.Remove("token");
                    if (StringUtil.IsInList("online", strLevel) == true)
                        table.Remove("online");
                    if (StringUtil.IsInList("loginname", strLevel) == true)
                        table.Remove("loginname");

                    string strValue = StringUtil.BuildParameterString(table, ',', '=', "url");

                    cookie.Value = strValue;
                    cookie.Expires = expire_time;
                }
            }
        }

        // parameters:
        //      strUserName 用户名。如果为 null，表示使用 sessioninfo.UserID
        //      nKeepLogin  1:设置为 ON  0:不变 -1：设置为 OFF
        public void SetCookiesLogin(
            string strLoginName,
            string strUserID,
            int nKeepLogin,
            int nOnline)
        {
            string strOptions = "";
            string strToken = FindTokenString(sessioninfo._rightsOrigin);
            if (string.IsNullOrEmpty(strToken) == true)
            {
                ClearCookiesLogin("all");
                return;
            }

            DateTime expire_time = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0);  // 一年以后失效
            if (nKeepLogin == -1)
                expire_time = GetInstantExpireTime();

            {
                Hashtable table = new Hashtable();

                if (strLoginName == null)
                    table["loginname"] = sessioninfo.UserID;
                else
                    table["loginname"] = strLoginName;

                if (strUserID == null)
                {
                    if (sessioninfo.IsReader == true
                        && sessioninfo.ReaderInfo != null
                        && string.IsNullOrEmpty(sessioninfo.ReaderInfo.ReaderKey) == false)
                        table["id"] = sessioninfo.ReaderInfo.ReaderKey;
                    else
                        table["id"] = sessioninfo.UserID;
                }
                else
                    table["id"] = strUserID;

                table["token"] = strToken;
                if (nKeepLogin != 0)
                    table["keeplogin"] = nKeepLogin == 1 ? "yes" : "no";
                if (nOnline != 0)
                    table["online"] = nOnline == 1 ? "yes" : "no";

                strOptions = StringUtil.BuildParameterString(table, ',', '=', "url");
            }

            HttpCookie cookie = this.Response.Cookies["opac-login"];
            if (cookie == null || cookie.Value == null)
            {
                cookie = this.Request.Cookies["opac-login"];
                if (cookie != null)
                    this.Response.Cookies.Set(cookie);
            }
            if (cookie == null)
            {
                cookie = new HttpCookie("opac-login", strOptions);
                cookie.Expires = expire_time;
                this.Response.Cookies.Add(cookie);
            }
            else
            {
                Hashtable table = StringUtil.ParseParameters(strOptions, ',', '=', "url");

                Hashtable old_table = StringUtil.ParseParameters(cookie.Value, ',', '=', "url");

                Hashtable new_table = StringUtil.MergeParametersTable(old_table, table);

                string strValue = StringUtil.BuildParameterString(new_table, ',', '=', "url");

                // 和现有的内容合并
                cookie.Value = strValue;
                cookie.Expires = expire_time;
            }

#if NO
            // test
            sessioninfo.UserID = "";
            sessioninfo.Password = "";
            sessioninfo.Parameters = "";
#endif
        }

        protected override void InitializeCulture()
        {
            WebUtil.InitLang(this);
            base.InitializeCulture();
        }

#if NO
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }
#endif

        protected void Page_Unload(object sender, EventArgs e)
        {
#if NO
            if (sessioninfo != null && sessioninfo.Channel != null)
                sessioninfo.Channel.Close();
#endif
            // 写入日志
            if (OpacApplication.LogWebRequest)
            {
                var id = this.Page.Session.SessionID;
                var length = DateTime.UtcNow - _startTime;
                OpacApplication.WriteInfoLog($"  {id} process time: {length}");
            }
        }

        DateTime _startTime;

        /*
        protected void Page_PreLoad(object sender, EventArgs e)
        {

        }
        */

        protected void Page_PreInit(object sender, EventArgs e)
        {
            _startTime = DateTime.UtcNow;
            // 写入日志
            if (OpacApplication.LogWebRequest)
            {
                var id = this.Page.Session.SessionID;
                string data = "";
                if (Request.ContentType == "application/x-www-form-urlencoded")
                    data = GetFormData(Request);
                OpacApplication.WriteInfoLog($"Request {id} {Request.HttpMethod} {Request.Url} {data}");
            }

            // 为了让ipad上运行的chrome表现正常
            if (Request.UserAgent != null && Request.UserAgent.IndexOf("crios", StringComparison.OrdinalIgnoreCase) > -1)
            {
                this.ClientTarget = "uplevel";
            }
        }

        static string GetFormData(HttpRequest request)
        {
            if (request.Form == null)
                return "";
            StringBuilder text = new StringBuilder();
            foreach (var key in request.Form.AllKeys)
            {
                if (key.StartsWith("_"))
                    continue;
                var value = request.Form[key];
                text.Append($"{key}={value}; ");
            }

            return "form data:" + text.ToString();
        }

        /*
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 DigitalPlatform.OPAC.Web.MyWebPage.Page_PreRenderComplete(Object sender, EventArgs e)
在 System.Web.UI.Page.ProcessRequestMain(Boolean includeStagesBeforeAsyncPoint, Boolean includeStagesAfterAsyncPoint)
         * */
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // 经过测试和上网找资料，我发现IE 8中Ajax功能不正常的原因是，我们的页面主要是<table>构成的，<td>元素下方的元素的.innerHTML在IE 8里面不能操作，基本上要抛出异常，我们所使用的jquery函数库来操作虽然还没有抛出异常但是功能不正常了。
            if (Request.UserAgent != null && Request.UserAgent.IndexOf("MSIE 8.", StringComparison.OrdinalIgnoreCase) > -1)
            {
                HtmlMeta meta = new HtmlMeta();
                meta.HttpEquiv = "X-UA-Compatible";
                meta.Content = "IE=7";

                // this.Page.Header.Controls.AddAt(0, meta);
                if (this.Header != null && this.Header.Controls != null)
                    this.Header.Controls.AddAt(0, meta);
            }
        }

        public static string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        public static string GetResultString(object obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());

            using (MemoryStream ms = new MemoryStream())
            {
                ser.WriteObject(ms, obj);
                string strResult = Encoding.UTF8.GetString(ms.ToArray());
                return strResult;
            }
        }

        public static long GetServerResultCount(
    SessionInfo sessioninfo,
    string strResultsetName)
        {
            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string strError = "";
                Record[] searchresults = null;
                long lRet = // sessioninfo.Channel.
                    channel.GetSearchResult(
                    null,
                    strResultsetName,
                    0,
                    0,
                    "id",
                    "zh",
                    out searchresults,
                    out strError);
                return lRet;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }
        }

        // 构造一个 style 目录中文件的路径
        public static string GetStylePath(OpacApplication app, string strFilename)
        {
            if (app != null && app.IsNewStyle == true)
                return "./stylenew/" + strFilename;
            return "./style/" + strFilename;
        }

        public void RedirectToLoginPage(bool push = true)
        {
            if (push)
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);

            string strUrl = GetDefaultLoginUrl();
            if (string.IsNullOrEmpty(strUrl) == false)
                this.Response.Redirect(strUrl, true);
            else
                this.Response.Redirect("login.aspx", true);
            this.Response.End();
            return;
        }

        // 2025/9/5
        // 当发现当前状态是未登录状态，需要进行的重定向处理
        public string GetDefaultLoginUrl()
        {
            var value = this.Page.Request["forcelogin"];
            if (value == "on")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                return "login.aspx";
            }
            else if (value == "userid")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                return "login.aspx?loginstyle=librarian";
            }

            // 如果 forcelogin 参数值为一个 URL，则重定向到这个 URL
            else if (value != null
                && value.StartsWith("url:"))
            {
                var url = value.Substring("url:".Length);
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                return url;
            }

            else if (string.IsNullOrEmpty(app.DefaultLoginUrl) == false)
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                return app.DefaultLoginUrl;
            }

            return null;
        }
    }
}
