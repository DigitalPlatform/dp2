// #define DUMP

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Threading;
using System.Resources;
using System.Globalization;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 标题条
    /// </summary>
    [ToolboxData("<{0}:TitleBarControl runat=server></{0}:TitleBarControl>")]
    public class TitleBarControl : WebControl, INamingContainer
    {
        public event RefreshingEventHandler Refreshing = null;
        public event EventHandler Refreshed = null;
        public event LibraryCodeChangedEventHandler LibraryCodeChanged = null;

        public override void Dispose()
        {
            this.Refreshing = null;
            this.Refreshed = null;
            this.LibraryCodeChanged = null;

            base.Dispose();
        }

        public string Dp2Sso = "";

        static string[] langs = new string[] {
            "简体中文", "zh-CN",
            "English", "en-US",
        };

        ResourceManager m_rm = null;

        public string SelControlID = "lang";

        public static string MatchLang(string strLang)
        {
            List<string> LangList = new List<string>();
            for (int i = 0; i < langs.Length / 2; i++)
            {
                LangList.Add(langs[i * 2 + 1]);
            }

            string strMatchLang = MatchLang(LangList, strLang);
            if (String.IsNullOrEmpty(strMatchLang) == false)
                return strMatchLang;

            return langs[1];
        }

        public static string GetLang(System.Web.UI.Page page,
            string strSelControlID)
        {
            string strLang = "";

            // lang下拉菜单
            strLang = page.Request.Form[strSelControlID];
            if (String.IsNullOrEmpty(strLang) == false)
                return strLang;

            // URL命令行lang参数
            strLang = page.Request.QueryString["lang"];
            if (String.IsNullOrEmpty(strLang) == false)
                return strLang;

            // Session
            strLang = (string)page.Session["lang"];
            if (String.IsNullOrEmpty(strLang) == false)
                return strLang;

            // browser's accept language
            if (page.Request.UserLanguages != null  // 2011/7/4
                && page.Request.UserLanguages.Length > 0)
            {
                strLang = MatchLang(page.Request.UserLanguages[0]);

                if (String.IsNullOrEmpty(strLang) == false)
                    return strLang;
            }

            return langs[1];
        }

        // 2024/2/20
        // 作为管理员身份此时要查看的读者键。注意，不是指管理员自己的读者键
        // 存储在Session中
        public string ReaderKey
        {
            get
            {
                /*
                object o = this.Page.Session[this.ID + "TitleBarControl_readerkey"];
                if (o == null)
                    return "";
                return (string)o;
                */
                return GetReaderKey(this);
            }

            set
            {
                /*
                // 清除 ReaderDom 缓存
                {
                    SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                    if (sessioninfo != null)
                        sessioninfo.RefreshLoginReaderDomCache(value);
                }
                this.Page.Session[this.ID + "TitleBarControl_readerkey"] = value;
                */
                SetReaderKey(this, value);
            }
        }

        public static string GetReaderKey(WebControl control)
        {
            var className = control.GetType().Name;
            object o = control.Page.Session[$"{control.ID}{className}_readerkey"];
            if (o == null)
                return "";
            return (string)o;
        }

        public static void SetReaderKey(WebControl control,
            string value)
        {
            // 清除 ReaderDom 缓存
            {
                SessionInfo sessioninfo = (SessionInfo)control.Page.Session["sessioninfo"];
                if (sessioninfo != null)
                    sessioninfo.RefreshLoginReaderDomCache(value);
            }
            var className = control.GetType().Name;
            control.Page.Session[$"{control.ID}{className}_readerkey"] = value;
        }

#if REMOVED
        // 作为管理员身份此时要查看的读者证条码号。注意，不是指管理员自己的读者证
        // 存储在Session中
        public string ReaderBarcode
        {
            get
            {
                object o = this.Page.Session[this.ID + "TitleBarControl_readerbarcode"];
                if (o == null)
                    return "";
                return (string)o;
            }

            set
            {
                // 2023/11/11
                // 清除 ReaderDom 缓存
                {
                    SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                    if (sessioninfo != null)
                        sessioninfo.RefreshLoginReaderDomCache(value);
                }
                this.Page.Session[this.ID + "TitleBarControl_readerbarcode"] = value;
            }
        }
#endif

        // 当前所在的栏目
        public TitleColumn CurrentColumn = TitleColumn.None;

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

#if NO
        // 未读的消息条数。存储在Session中
        public int UnreadMessageCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "TitleBarControl_inbox_unread_message_count"];
                if (o == null)
                    return 0;
                return (int)o;
            }

            set
            {
                this.Page.Session[this.ID + "TitleBarControl_inbox_unread_message_count"] = value;
            }
        }
#endif

        string UnmacroString(string strText)
        {

#if NO
            string strStyleDirName = "";
            string strTitleText = "";
            string strReaderName = "";
            GetMacroValue(out strStyleDirName,
                out strTitleText,
                out strReaderName);
#endif
            EnvValue env = GetMacroValue();

#if DUMP
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            app.WriteErrorLog("env.ReaderName [" + env.ReaderName + "] env.StyleDirName [" + env.StyleDirName + "] env.TitleText [" + env.TitleText + "]");
#endif

            if (String.IsNullOrEmpty(env.StyleDirName) == true)
                env.StyleDirName = "0";
            // 替换 %styledir% 宏
            strText = strText.Replace("%styledir%", env.StyleDirName);

            // 替换 %titletext% 宏
            strText = strText.Replace("%titletext%", env.TitleText);

            // 替换 %readername% 宏
            strText = strText.Replace("%readername%", env.ReaderName);
            // 替换 %name% 宏
            strText = strText.Replace("%name%", env.ReaderName);

            // 2017/10/27
            // 面板上选择的馆代码
            string strSelectedLibraryCode = (string)this.Page.Session["librarycode"];
            // 替换 %librarycode% 宏
            strText = strText.Replace("%librarycode%", strSelectedLibraryCode);


            return strText;
        }

        class EnvValue
        {
            public string StyleDirName = "";
            public string TitleText = "";
            public string ReaderName = "";
        }

        EnvValue GetEnvValue(OpacApplication app,
            string strLibraryStyleDir,
            string strTypeName,
            string strDefaultReaderName)
        {
            EnvValue env = new EnvValue();

            string strStyleDirName = "";

            string strTitleText = "";
            string strReaderName = "";

            XmlNode nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                "titleBarControl/userType[@type='"+strTypeName+"']");
            if (nodeUserType != null)
            {
                strStyleDirName = DomUtil.GetAttr(nodeUserType, "style");
                strStyleDirName = LinkControl.MakeDir(strLibraryStyleDir, strStyleDirName);

                if (nodeUserType.Attributes["titletext"] != null)
                {
                    strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                }
                else
                {
                    // 多语种
                    strTitleText = DomUtil.GetLangedNodeText(
                        this.Lang,
                        nodeUserType,
                        "titletext");
                }

                if (nodeUserType.Attributes["name"] != null)
                {
                    strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                }
                else
                {
                    // 多语种
                    strReaderName = DomUtil.GetLangedNodeText(
                        this.Lang,
                        nodeUserType,
                        "name");
                }
            }
            else
            {
                // 2015/1/26 给出一个缺省值
                strStyleDirName = LinkControl.MakeDir(strLibraryStyleDir, "0");
            }

            if (String.IsNullOrEmpty(strReaderName) == true)
                strReaderName = strDefaultReaderName; // this.GetString("quoted_未登录"); // "[未登录]"

            env.StyleDirName = strStyleDirName;
            env.ReaderName = strReaderName;
            env.TitleText = strTitleText;

            return env;
        }

        EnvValue GetMacroValue(
#if NO
            out string strStyleDirName,
            out string strTitleText,
            out string strReaderName
#endif
            )
        {
            string strError = "";
#if NO
            strStyleDirName = "";
            strTitleText = "";
            strReaderName = "";
#endif

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            // 面板上选择的馆代码
            string strSelectedLibraryCode = (string)this.Page.Session["librarycode"];

            string strLibraryStyleDir = ""; // 分馆的风格目录

            if (string.IsNullOrEmpty(strSelectedLibraryCode) == false)
            {
                XmlNode nodeLibrary = app.WebUiDom.DocumentElement.SelectSingleNode("libraries/library[@code='" + strSelectedLibraryCode + "']");
                if (nodeLibrary != null)
                {
                    strLibraryStyleDir = DomUtil.GetAttr(nodeLibrary, "style");
                    if (string.IsNullOrEmpty(strLibraryStyleDir) == true)
                        strLibraryStyleDir = DomUtil.GetAttr(nodeLibrary, "code");
                }
            }

            /* 单语种
		<userType type="librarian" titletext="一切为了读者" style="0" name="欢迎你:%userid%" />
		<userType type="notlogin" titletext="请先登录" style="0" />
		<userType type="public" titletext="图书馆是知识的海洋" style="0" />
             * */
            /* 多语种
<userType type="librarian" style="0" >
             <titletext lang='zh'>一切为了读者</titletext>
             <name lang='zh'>欢迎你:%userid%</name>
</userType>
<userType type="notlogin" style="0" >
             <titletext lang='zh'>请先登录</titletext>
</userType>
<userType type="public" style="0" >
             <titletext lang='zh'>图书馆是知识的海洋</titletext>
</userType>
             * * */
            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            if (loginstate == LoginState.NotLogin)
            {
#if NO
                XmlNode nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "titleBarControl/userType[@type='notlogin']");
                if (nodeUserType != null)
                {
                    strStyleDirName = DomUtil.GetAttr(nodeUserType, "style");
                    strStyleDirName = LinkControl.MakeDir(strLibraryStyleDir, strStyleDirName);

                    if (nodeUserType.Attributes["titletext"] != null)
                    {
                        strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                    }
                    else
                    {
                        // 多语种
                        strTitleText = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "titletext");
                    }

                    if (nodeUserType.Attributes["name"] != null)
                    {
                        strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                    }
                    else
                    {
                        // 多语种
                        strReaderName = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "name");
                    }
                }

                if (String.IsNullOrEmpty(strReaderName) == true)
                    strReaderName = this.GetString("quoted_未登录"); // "[未登录]"

                return;
#endif
                return GetEnvValue(app,
                    strLibraryStyleDir,
                    "notlogin",
                    this.GetString("quoted_未登录"));
            }

            if (loginstate == LoginState.Public)
            {
#if NO
                XmlNode nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "titleBarControl/userType[@type='public']");
                if (nodeUserType != null)
                {
                    strStyleDirName = DomUtil.GetAttr(nodeUserType, "style");
                    strStyleDirName = LinkControl.MakeDir(strLibraryStyleDir, strStyleDirName);

                    // strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                    // strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                    if (nodeUserType.Attributes["titletext"] != null)
                    {
                        strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                    }
                    else
                    {
                        // 多语种
                        strTitleText = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "titletext");
                    }

                    if (nodeUserType.Attributes["name"] != null)
                    {
                        strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                    }
                    else
                    {
                        // 多语种
                        strReaderName = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "name");
                    }
                }

                if (String.IsNullOrEmpty(strReaderName) == true)
                    strReaderName = this.GetString("quoted_访客");   // "[访客]"

                return;
#endif
                return GetEnvValue(app,
    strLibraryStyleDir,
    "public",
    this.GetString("quoted_访客"));
            }

            if (loginstate == LoginState.Librarian)
            {
#if NO
                XmlNode nodeUserType = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "titleBarControl/userType[@type='librarian']");
                if (nodeUserType != null)
                {
                    strStyleDirName = DomUtil.GetAttr(nodeUserType, "style");
                    strStyleDirName = LinkControl.MakeDir(strLibraryStyleDir, strStyleDirName);


                    //strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                    //strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                    if (nodeUserType.Attributes["titletext"] != null)
                    {
                        strTitleText = DomUtil.GetAttr(nodeUserType, "titletext");
                    }
                    else
                    {
                        // 多语种
                        strTitleText = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "titletext");
                    }

                    if (nodeUserType.Attributes["name"] != null)
                    {
                        strReaderName = DomUtil.GetAttr(nodeUserType, "name");
                    }
                    else
                    {
                        // 多语种
                        strReaderName = DomUtil.GetLangedNodeText(
                            this.Lang,
                            nodeUserType,
                            "name");
                    }
                }

                if (String.IsNullOrEmpty(strReaderName) == true)
                    strReaderName = this.GetString("quoted_图书馆员") // "[图书馆员]"
                        + sessioninfo.UserID;
                else
                {
                    // 配置的值中可以用%userid%宏
                    strReaderName = strReaderName.Replace("%userid%", sessioninfo.UserID);
                }
                return;
#endif
                EnvValue env = GetEnvValue(app,
strLibraryStyleDir,
"librarian",
"");
                if (String.IsNullOrEmpty(env.ReaderName) == true)
                    env.ReaderName = this.GetString("quoted_图书馆员") // "[图书馆员]"
                        + sessioninfo.UserID;
                else
                {
                    // 配置的值中可以用%userid%宏
                    env.ReaderName = env.ReaderName.Replace("%userid%", sessioninfo.UserID);
                }
                env.TitleText = env.TitleText.Replace("%userid%", sessioninfo.UserID);
                return env;
            }

            {
                EnvValue env = new EnvValue();

                // 获得当前session中已经登录的读者记录DOM
                // return:
                //      -2  当前登录的用户不是reader类型
                //      -1  出错
                //      0   尚未登录
                //      1   成功
                int nRet = sessioninfo.GetLoginReaderDom(
                    out XmlDocument readerdom,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;

                if (nRet == 0)
                {
                    goto ERROR1;
                }

                env.ReaderName = DomUtil.GetElementText(readerdom.DocumentElement,
                    "name");

                XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
                if (preference != null)
                {
                    XmlNode webui = preference.SelectSingleNode("webui");
                    if (webui != null)
                    {
                        string strStyleDirName = DomUtil.GetAttr(webui, "style");
                        env.StyleDirName = LinkControl.MakeDir(strLibraryStyleDir, strStyleDirName);
                         
                        env.TitleText = DomUtil.GetAttr(webui, "titletext");
                        return env;
                    }
                }

                env = GetEnvValue(app,
strLibraryStyleDir,
"reader",
"");

                string strReaderName = "";
                if (sessioninfo.ReaderInfo != null)
                {
                    strReaderName = sessioninfo.ReaderInfo.DisplayName;
                    if (string.IsNullOrEmpty(strReaderName) == true)
                        strReaderName = sessioninfo.ReaderInfo.Name;
                    if (string.IsNullOrEmpty(strReaderName) == true)
                        strReaderName = sessioninfo.UserID;
                }
                if (String.IsNullOrEmpty(env.ReaderName) == true)
                    env.ReaderName = strReaderName;
                else
                {
                    // 配置的值中可以用%userid%宏
                    env.ReaderName = env.ReaderName.Replace("%userid%", strReaderName);
                }
                env.TitleText = env.TitleText.Replace("%userid%", strReaderName);
                return env;
            }
            return new EnvValue();
        ERROR1:
            return new EnvValue();
        }

#if AUTO_RELOGIN
        public bool DoReLogin()
        {
            int nRet = 0;
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (sessioninfo.Account == null)
            {

                // 有可能是服务器重新启动导致
                string strReLogin = this.ReLoginString;
                // this.DebugInfo = "step1 -- strRelogin=["+strReLogin+"]";
                // 立即重新登录
                if (String.IsNullOrEmpty(strReLogin) == false)
                {
                    // this.DebugInfo = "step2 ";


                    Hashtable table = StringUtil.ParseParameters(strReLogin);
                    string strUserName = (string)table["username"];
                    if (String.IsNullOrEmpty(strUserName) == true)
                        return false;
                    string strToken = (string)table["token"];
                    string strType = (string)table["type"];

                    if (strType != "reader")
                    {
                        // 工作人员登录
                        nRet = sessioninfo.Login(strUserName,
                             null,
                             "#web",
                             false,
                             out strError);
                        // this.DebugInfo = "step 3 -- login return =["+nRet.ToString()+"]";
                        if (nRet != 1)
                            return false;

                    }
                    else
                    {
                        // 读者身份登录
                        nRet = app.LoginForReader(sessioninfo,
                                    strUserName,
                                    null,   // 表示不进行密码判断
                                     "#web",
                                    -1,
                                    out strError);
                        // this.DebugInfo = "step 3a -- login return =[" + nRet.ToString() + "]";
                        if (nRet == -1)
                        {
                            strError = "对图书馆读者帐户进行登录时出错：" + strError;
                            return false;
                        }
                        if (nRet > 1)
                        {
                            strError = "登录中发现有 " + nRet.ToString() + " 个账户符合条件，登录失败";
                            return false;
                        }
                    }

                    string strSHA1 = Cryptography.GetSHA1(sessioninfo.UserID + "|" + sessioninfo.Account.Password);
                    if (strSHA1 != strToken)
                    {
                        // logout
                        this.Page.Session.Abandon();
                        return false; // token不正确
                    }
                    Debug.Assert(sessioninfo.Account != null, "");
                    // this.DebugInfo = "relogin!";
                    return true;
                }


                return false;
            }
            else
            {
                // this.DebugInfo = "normal " + sessioninfo.UserID + " strRelogin=[" + this.ReLoginString + "]";
            }

            return false;
        }

        public void PrepareReLogin()
        {
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (sessioninfo.Account != null)
            {
                // 准备relogin
                string strSHA1 = Cryptography.GetSHA1(sessioninfo.UserID + "|" + sessioninfo.Account.Password);
                this.ReLoginString = "username=" + sessioninfo.UserID + ",token=" + strSHA1 + ",type=" + sessioninfo.Account.Type;
            }
        }

        public string ReLoginString
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("relogin");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("relogin");
                s.Value = value;
            }
        }
#endif

#if NO
        public string DebugInfo
        {
            get
            {
                this.EnsureChildControls();

                LiteralControl s = (LiteralControl)this.FindControl("debug___info");
                return s.Text;
            }
            set
            {
                this.EnsureChildControls();

                LiteralControl s = (LiteralControl)this.FindControl("debug___info");
                s.Text = value;
            }
        }
#endif

        protected override void CreateChildControls()
        {
            // 总表格开始
            AutoIndentLiteral literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><!-- TitleBarControl 开始-->"
                + "<%begin%><table id='outerframe' class='body' border='0' cellpadding='0' cellspacing='0'>";
            this.Controls.Add(literal);

            // 标题图像表格 开始
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><!-- 标题图像 -->"
                + "<%begin%><tr><td>"
                + "<%begin%><table class='title'>"  // 这个<table>本类自己管辖了
                + "<%begin%><tr class='title'>";
            this.Controls.Add(literal);

#if AUTO_RELOGIN
            // 用于重新登录的信息
            HiddenField relogin = new HiddenField();
            relogin.ID = "relogin";
            relogin.Value = "";
            this.Controls.Add(relogin);
#endif

            // 获得配置参数
            OpacApplication app = (OpacApplication)this.Page.Application["app"];


            // 左
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td class='left'>";
            this.Controls.Add(literal);

            LiteralControl lefthtml = new LiteralControl();
            lefthtml.ID = "lefthtml";
            this.Controls.Add(lefthtml);


            // 中
            literal = new AutoIndentLiteral();
            literal.Text = "</td> <%normal%><td class='center'>";
            this.Controls.Add(literal);

            LiteralControl debug___info = new LiteralControl();
            debug___info.ID = "debug___info";
            this.Controls.Add(debug___info);

            LiteralControl centerhtml = new LiteralControl();
            centerhtml.ID = "centerhtml";
            this.Controls.Add(centerhtml);

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            this.Controls.Add(literal);


            // 右
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td class='right'>";
            this.Controls.Add(literal);

            // 右上角的命令小表格
            CreateTopRightTable();

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%></td>";
            this.Controls.Add(literal);


            // 标题图像表格 结束
            literal = new AutoIndentLiteral();
            literal.Text = "<%end%></tr>"
                + "<%end%></table>"
                + "<%end%></td></tr>";
            this.Controls.Add(literal);

            // 栏目条表格 开始
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><!-- 栏目条 -->"
                + "<%begin%><tr><td>"
                + "<%begin%><table class='columnbar'>"
                + "<%begin%><tr class='columnbar'>";
            this.Controls.Add(literal);


            // 各个栏目
            CreateColumns();

            // 栏目条表格 结束
            literal = new AutoIndentLiteral();
            literal.Text = "<%end%></tr>"
                + "<%end%></table>"
                + "<%end%></td></tr>";
            this.Controls.Add(literal);

            // 主体内容开始
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><!-- 主体内容 -->"
                + "<%begin%><tr class='main'><td class='main'>"
                + "<%normal%><!-- TitleBarControl 结束 -->";
            this.Controls.Add(literal);

        }

        void CreateLangList(Control parent)
        {
            LiteralControl literal = new LiteralControl();

            literal.Text = "<select name='langlist' onchange=\"javascript:setTimeout(LangPostBack, 0)\" language=\"javascript\" id=\"langlist\" class=\"lang\">"
                + GetLangOptions()
                + "</select>";
            parent.Controls.Add(literal);
        }

        string GetLangOptions()
        {
            string strCurrentLang = Thread.CurrentThread.CurrentUICulture.Name;

            string strResult = "";
            for (int i = 0; i < langs.Length / 2; i++)
            {
                string strValue = langs[i * 2 + 1];
                string strCaption = langs[i * 2];
                strResult += "<option value='" + strValue + "' ";
                if (strCurrentLang.ToLower() == strValue.ToLower())
                    strResult += "SELECTED ";

                strResult += ">" + strCaption + "</option>";
            }

            return strResult;
        }

        static void FillLangList(DropDownList langlist)
        {
            langlist.Items.Clear();

            langlist.Items.Add(new ListItem("中文", "zh-CN"));
            langlist.Items.Add(new ListItem("English", "en-US"));
        }

        public string GetLoginUrl()
        {
            string strResult = "./login.aspx";    // "./login.aspx?lang=" + strLang;
            if (this.Dp2Sso == "first")
            {
                strResult += "?dp2sso=first&redirect=" + HttpUtility.UrlEncode(this.Page.Request.RawUrl);
            }

            return strResult;
        }

        void CreateTopRightTable()
        {
            // 表格开始
            AutoIndentLiteral literal = new AutoIndentLiteral();
            literal.Text = "<%begin%><table class='toprightcmd'>"
                + "<%begin%><tr class='toprightcmd'>";
            this.Controls.Add(literal);

            // login
            PlaceHolder loginholder = new PlaceHolder();
            loginholder.ID = "loginholder";
            this.Controls.Add(loginholder);

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td nowrap>";
            loginholder.Controls.Add(literal);

            string strLang = Thread.CurrentThread.CurrentUICulture.Name;

            HyperLink hyper = new HyperLink();
            hyper.ID = "login";
            hyper.Text = this.GetString("登录");
            hyper.NavigateUrl = GetLoginUrl();
#if NO
            hyper.NavigateUrl = "./login.aspx";    // "./login.aspx?lang=" + strLang;
            if (this.Dp2Sso == "first")
            {
                hyper.NavigateUrl += "?dp2sso=first&redirect=" + HttpUtility.UrlEncode(this.Page.Request.RawUrl);
            }
#endif

            loginholder.Controls.Add(hyper);
            

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            loginholder.Controls.Add(literal);


            // logout
            PlaceHolder logoutholder = new PlaceHolder();
            logoutholder.ID = "logoutholder";
            this.Controls.Add(logoutholder);

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td nowrap>";
            logoutholder.Controls.Add(literal);


            hyper = new HyperLink();
            hyper.ID = "logout";
            hyper.Text = this.GetString("登出");
            hyper.NavigateUrl = "./login.aspx?action=logout";   //  "./login.aspx?action=logout&lang=" + strLang;
            if (this.Dp2Sso == "first")
            {
                hyper.NavigateUrl += "&redirect=" + HttpUtility.UrlEncode(this.Page.Request.RawUrl);
            }
            logoutholder.Controls.Add(hyper);

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            logoutholder.Controls.Add(literal);


            // 刷新
            PlaceHolder refreshholder = new PlaceHolder();
            refreshholder.ID = "refreshholder";
            this.Controls.Add(refreshholder);

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td nowrap>";
            refreshholder.Controls.Add(literal);

            LinkButton refreshbutton = new LinkButton();
            refreshbutton.Text = this.GetString("刷新");
            refreshbutton.Click += new EventHandler(refreshbutton_Click);
            refreshholder.Controls.Add(refreshbutton);

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            refreshholder.Controls.Add(literal);

            ////

            // 馆代码
            PlaceHolder librarycodeholder = new PlaceHolder();
            librarycodeholder.ID = "librarycodeholder";
            this.Controls.Add(librarycodeholder);

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td nowrap>";
            librarycodeholder.Controls.Add(literal);

            DropDownList librarycodelist = new DropDownList();
            librarycodelist.ID = "librarycodelist";
            //librarycodelist.Width = new Unit("100%");
            librarycodelist.AutoPostBack = true;
            librarycodelist.CssClass = "librarycodelist";
            librarycodelist.SelectedIndexChanged += new EventHandler(librarycodelist_SelectedIndexChanged);
            librarycodeholder.Controls.Add(librarycodelist);

            if (this.Page.IsPostBack == false)
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                List<string> codes = app.GetAllLibraryCodes();

                // 限定馆代码
                string strLimit = app.LimitWebUiLibraryCode;
                if (string.IsNullOrEmpty(strLimit) == true)
                    codes = app.GetAllLibraryCodes();
                else
                    codes = StringUtil.SplitList(strLimit);

                if (codes.Count > 0)
                    FillLibraryCodeList(librarycodelist,
                        codes,
                        string.IsNullOrEmpty(strLimit));
                else
                {
                    librarycodeholder.Visible = false;
                }
            }

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            librarycodeholder.Controls.Add(literal);

            ////


            // UI语言
            PlaceHolder langholder = new PlaceHolder();
            langholder.ID = "langholder";
            this.Controls.Add(langholder);

            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td nowrap>";
            langholder.Controls.Add(literal);

            CreateLangList(langholder);

            /*
            DropDownList list = new DropDownList();
            list.ID = "langlist";
            //list.Width = new Unit("100%");
            list.AutoPostBack = true;
            list.CssClass = "lang";
            // list.SelectedIndexChanged += new EventHandler(list_SelectedIndexChanged);
            langholder.Controls.Add(list);
             * */

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            langholder.Controls.Add(literal);

            // FillLangList(list);

            /*
            // 内容
            literal = new LiteralControl();
            literal.Text = "<td>登录</td>"
                + "<td>登出</td>"
                + "<td>刷新</td>";
            this.Controls.Add(literal);
             * */

            /*
            // 登录
            linkbutton = new LinkButton();
            linkbutton.ID = "login";
            linkbutton.Text = "登录";
            linkbutton.Click +=new EventHandler(loginButton_Click);
            this.Controls.Add(linkbutton);

            // 登出
            linkbutton = new LinkButton();
            linkbutton.ID = "logout";
            linkbutton.Text = "登出";
            linkbutton.Click += new EventHandler(logoutButton_Click);
            this.Controls.Add(linkbutton);

             * */


            // 表格结束
            literal = new AutoIndentLiteral();
            literal.Text = "<%end%></tr>"
            + "<%end%></table>";
            this.Controls.Add(literal);
        }

        public string SelectedLibraryCode
        {
            get
            {
                this.EnsureChildControls();
                DropDownList list = (DropDownList)this.FindControl("librarycodelist");
                if (list == null)
                    return "";
                return list.SelectedValue;
            }
            set
            {
                this.EnsureChildControls();
                DropDownList list = (DropDownList)this.FindControl("librarycodelist");
                List<string> codes = StringUtil.SplitList(value);
                if (codes.Count > 0)
                {
                    list.SelectedValue = codes[0];
                    this.Page.Session["librarycode"] = codes[0];
                }
            }
        }

        void librarycodelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strOldLibraryCode = (string)this.Page.Session["librarycode"];

            DropDownList list = (DropDownList)sender;
            this.Page.Session["librarycode"] = list.SelectedValue;

            if (this.LibraryCodeChanged != null)
            {
                LibraryCodeChangedEventArgs e1 = new LibraryCodeChangedEventArgs();
                e1.OldLibraryCode = strOldLibraryCode;
                e1.NewLibraryCode = list.SelectedValue;
                this.LibraryCodeChanged(this, e1);
            }
        }

        void FillLibraryCodeList(DropDownList list,
            List<string> items,
            bool bAddFirstEntry)
        {
            if (list.Items.Count > 0)
                return;

            //    list.Items.Clear();

            if (items == null)
                return;

            if (bAddFirstEntry == true
                || items.IndexOf("[all]") != -1)
            {
                ListItem item = new ListItem();
                item.Text = this.GetString("[全部分馆]");
                item.Value = "";
                list.Items.Add(item);
            }
            foreach (string s in items)
            {
                if (s == "[all]")
                    continue;
                ListItem item = new ListItem();
                item.Text = s;
                item.Value = s;
                list.Items.Add(item);
            }
            string strSelectedValue = (string)this.Page.Session["librarycode"];
            if (string.IsNullOrEmpty(strSelectedValue) == false)
                list.SelectedValue = strSelectedValue;
        }

        /*
        void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList list = (DropDownList)sender;

            string lang = list.SelectedValue;

            Thread.CurrentThread.CurrentCulture =
    CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(lang);
        }*/

        void refreshbutton_Click(object sender, EventArgs e)
        {
            bool bCancel = false;
            if (this.Refreshing != null)
            {
                RefreshingEventArgs e1 = new RefreshingEventArgs();
                this.Refreshing(this, e1);
                bCancel = e1.Cancel;
            }

            if (bCancel == false)
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app != null)
                {
                    SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                    if (sessioninfo != null)
                        sessioninfo.Clear();
                }
            }

            if (this.Refreshed != null)
                this.Refreshed(this, e);
        }

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.TitleBarControl.cs",
                typeof(TitleBarControl).Module.Assembly);

            return this.m_rm;
        }

        string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name/*"en-US"*/);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        void CreateColumns()
        {
            // 获得配置参数
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            /*
             * 
	<titleBarControl>
		<leftAnchor lang='zh'>
			<a href="http://dp2003.com">图书馆主页</a>
		</leftAnchor>
		<leftAnchor lang='en'>
			<a href="http://dp2003.com">Library Homepage</a>
		</leftAnchor>        ...
             */
            string strLeftHtml = "";
            // XmlNode nodeLeftAnchor = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/leftAnchor");
            XmlNode parent = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl");
            if (parent != null)
            {
                // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode
                // parameters:
                //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
                XmlNode nodeLeftAnchor = DomUtil.GetLangedNode(
                    this.Lang,
                    parent,
                    "leftAnchor");
                if (nodeLeftAnchor != null)
                {
                    strLeftHtml = nodeLeftAnchor.InnerXml;
                }
            }

            // 左边第一个
            AutoIndentLiteral literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td class='left'>"
                + strLeftHtml
                + "</td>";
            this.Controls.Add(literal);

            // bool bPublic = IsPublic();
            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            // if (this.CurrentColumn != TitleColumn.None)
            {

                string strClass = "normal";

                HyperLink hyperlink = null;

                if (loginstate == LoginState.Librarian
                    && string.IsNullOrEmpty(this.ReaderKey) == false)
                {

                    // 其余栏目

                    // 读者的综合信息
                    if (this.CurrentColumn == TitleColumn.ReaderInfo)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "ReaderInfo";
                    hyperlink.Text = this.GetString("读者")
                        + " " + this.ReaderKey; // TODO: 可以改为更为友好的显示形态，比如优先证条码号
                    hyperlink.NavigateUrl = "./readerinfo.aspx?barcode=" + this.ReaderKey;
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }

                if (loginstate == LoginState.Reader)
                {
                    // 其余栏目

                    // 借阅信息
                    if (this.CurrentColumn == TitleColumn.BorrowInfo)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "BorrowInfo";
                    hyperlink.Text = this.GetString("借阅信息");
                    hyperlink.NavigateUrl = "./borrowinfo.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }

                if (loginstate == LoginState.Reader)
                {
                    // 预约
                    if (this.CurrentColumn == TitleColumn.ReservationInfo)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "ReservationInfo";
                    hyperlink.Text = this.GetString("预约");
                    hyperlink.NavigateUrl = "./reservationinfo.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }
#if NO
                if (loginstate == LoginState.Reader)
                {
                    // 违约
                    if (this.CurrentColumn == TitleColumn.FellBackInfo)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "FellBackInfo";
                    hyperlink.Text = this.GetString("违约_and_交费");   // "违约/交费"
                    hyperlink.NavigateUrl = "./fellbackinfo.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }
#endif

#if NO
                if (loginstate == LoginState.Reader)
                {
                    // 借阅历史
                    if (this.CurrentColumn == TitleColumn.BorrowHistory)
                        strClass = "active";
                    else
                        strClass = "normal";

                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "BorrowHistory";
                    hyperlink.Text = this.GetString("借阅历史");
                    hyperlink.NavigateUrl = "./borrowhistory.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }
#endif

                // 检索
                if (this.CurrentColumn == TitleColumn.Search)
                    strClass = "active";
                else
                    strClass = "normal";

                literal = new AutoIndentLiteral();
                literal.Text = "<%normal%><td class='" + strClass + "'>";
                this.Controls.Add(literal);

                hyperlink = new HyperLink();
                hyperlink.ID = "Search";
                hyperlink.Text = this.GetString("检索");
                hyperlink.NavigateUrl = "./searchbiblio.aspx";
                this.Controls.Add(hyperlink);

                literal = new AutoIndentLiteral();
                literal.Text = "</td>";
                this.Controls.Add(literal);

                // 浏览
                string strAspx = this.Page.Server.MapPath("./browse.aspx");
                string strBrowseXml = app.DataDir + "/browse/browse.xml";
                string strSidebarXml = app.DataDir + "/browse/browse_sidebar.xml";

                if (File.Exists(strAspx) == true
                    && File.Exists(strBrowseXml) == true)
                {
                    if (this.CurrentColumn == TitleColumn.Browse)
                        strClass = "active";
                    else
                        strClass = "normal";

                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "browse";
                    hyperlink.Text = this.GetString("浏览");
                    hyperlink.NavigateUrl = "./browse.aspx?datafile=browse.xml";
                    if (File.Exists(strSidebarXml) == true)
                        hyperlink.NavigateUrl += "&sidebar=browse_sidebar.xml";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }

                // 书评
                bool bEnableBookReview = true;
                strAspx = this.Page.Server.MapPath("./column.aspx");
                XmlNode nodeBookReview = app.WebUiDom.DocumentElement.SelectSingleNode("bookReview");
                if (nodeBookReview != null)
                {
                    string strError = "";
                    DomUtil.GetBooleanParam(nodeBookReview,
                        "enable",
                        true,
                        out bEnableBookReview,
                        out strError);
                }

                if (File.Exists(strAspx) == true
                    && bEnableBookReview == true)
                {
                    if (this.CurrentColumn == TitleColumn.BookReview)
                        strClass = "active";
                    else
                        strClass = "normal";

                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "bookReview";
                    hyperlink.Text = this.GetString("书评");
                    hyperlink.NavigateUrl = "./column.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }

                if (loginstate == LoginState.Librarian
                    || loginstate == LoginState.Reader)
                {

                    // 消息
                    if (this.CurrentColumn == TitleColumn.Message)
                        strClass = "active";
                    else
                        strClass = "normal";

                    // 这里可以做适当cache
                    int nUnreadCount = 0;

#if NO
                    if (this.GetInboxUnreadCount != null)
                    {
                        GetInboxUnreadCountEventArgs e = new GetInboxUnreadCountEventArgs();

                        this.GetInboxUnreadCount(this, e);
                        nUnreadCount = e.UnreadCount;
                    }

                    // 2007/7/7
                    // 如果和曾经记忆的不同
                    if (nUnreadCount != this.UnreadMessageCount)
                    {
                        SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                        // 触发清除读者记录缓存
                        // 这样做的目的是，假如读者接到了通知信件，那可能是读者记录发生了改变(例如预约到书等)，这里及时清除缓存，能确保读者读到的信件和预约状态等显示保持同步，防止出现迷惑读者的信息新旧状态不同的情况
                        // 当然，页面上的Refresh命令也能起到同样的作用
                        if (sessioninfo != null)
                            sessioninfo.Clear();

                        // 记忆新值
                        this.UnreadMessageCount = nUnreadCount;
                    }
#endif

                    string strText = this.GetString("消息");

                    if (nUnreadCount != 0)
                        strText += "(" + nUnreadCount.ToString() + ")";


                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + " messagecolumn'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "Message";
                    hyperlink.Text = strText;
                    hyperlink.NavigateUrl = "./mymessage.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }

                if (loginstate == LoginState.Reader)
                {
                    // 我的书架
                    if (this.CurrentColumn == TitleColumn.MyBookShelf)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "MyBookShelf";
                    hyperlink.Text = this.GetString("我的书架");
                    hyperlink.NavigateUrl = "./mybookshelf.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }

                if (loginstate == LoginState.Reader)
                {

                    // 个人信息
                    if (this.CurrentColumn == TitleColumn.PersonalInfo)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "PersonalInfo";
                    hyperlink.Text = this.GetString("个人信息");
                    hyperlink.NavigateUrl = "./PersonalInfo.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);

                }

                // 统计信息
                XmlNode nodeStatisColumn = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/statisColumn");
                string strStatisColumnVisible = "";
                if (nodeStatisColumn == null)
                {
                    // 元素缺乏时的缺省值
                    strStatisColumnVisible = "reader,librarian";
                }
                else
                {
                    // 一旦元素具备，就没有缺省值了
                    strStatisColumnVisible = DomUtil.GetAttr(nodeStatisColumn, "visible");
                }

                if (StringUtil.IsInList("all", strStatisColumnVisible) == true
                    || (loginstate == LoginState.Librarian && StringUtil.IsInList("librarian", strStatisColumnVisible) == true)
                    || (loginstate == LoginState.Reader && StringUtil.IsInList("reader", strStatisColumnVisible) == true)
                    || (loginstate == LoginState.Public && StringUtil.IsInList("public", strStatisColumnVisible) == true)
                    || (loginstate == LoginState.NotLogin && StringUtil.IsInList("notlogin", strStatisColumnVisible) == true)
                    )
                {

                    if (this.CurrentColumn == TitleColumn.Statis)
                        strClass = "active";
                    else
                        strClass = "normal";

                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "Statis";
                    hyperlink.Text = this.GetString("统计信息");
                    hyperlink.NavigateUrl = "./statis.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }

                if (loginstate == LoginState.Librarian
&& String.IsNullOrEmpty(this.ReaderKey) == true)
                {

                    // 工作人员的管理功能
                    if (this.CurrentColumn == TitleColumn.Management)
                        strClass = "active";
                    else
                        strClass = "normal";
                    literal = new AutoIndentLiteral();
                    literal.Text = "<%normal%><td class='" + strClass + "'>";
                    this.Controls.Add(literal);

                    hyperlink = new HyperLink();
                    hyperlink.ID = "Management";
                    hyperlink.Text = this.GetString("管理");
                    hyperlink.NavigateUrl = "./management.aspx";
                    this.Controls.Add(hyperlink);

                    literal = new AutoIndentLiteral();
                    literal.Text = "</td>";
                    this.Controls.Add(literal);
                }
            }

            // 右端占据空格的栏目
            literal = new AutoIndentLiteral();
            literal.Text = "<%normal%><td class='right'>";
            this.Controls.Add(literal);

            literal = new AutoIndentLiteral();
            literal.Text = "</td>";
            this.Controls.Add(literal);
        }

        int GetParentCount()
        {
            int nCount = 0;
            Control current = this;
            while (current != null)
            {
                current = current.Parent;
                nCount++;
            }

            return nCount;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int nParentCount = GetParentCount();
            writer.Indent += nParentCount;

            OpacApplication app = null;
            SessionInfo sessioninfo = null;

            // bool bLogin = false;

            string strErrorInfo = (string)this.Page.Application["errorinfo"];

            if (String.IsNullOrEmpty(strErrorInfo) == true)
            {
                app = (OpacApplication)this.Page.Application["app"];
                sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            }

#if NO
            if (app != null && sessioninfo != null)
            {
                // 是否登录?
                if (String.IsNullOrEmpty(sessioninfo.UserID) == false)
                {
                    bLogin = true;
                }
            }
#endif

            PlaceHolder loginholder = (PlaceHolder)this.FindControl("loginholder");
            PlaceHolder logoutholder = (PlaceHolder)this.FindControl("logoutholder");

            HyperLink login = (HyperLink)this.FindControl("login");
            HyperLink logout = (HyperLink)this.FindControl("logout");
            HyperLink visible = null;

#if NO
            if (bLogin == true)
            {
                loginholder.Visible = false;
                logoutholder.Visible = true;
                visible = logout;
            }
            else
            {
                loginholder.Visible = true;
                logoutholder.Visible = false;
                visible = login;
            }
#endif

            bool bLogin = false;
            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);
            string strState = "";
            if (loginstate == LoginState.Librarian)
            {
                strState = this.GetString("图书馆员");
                bLogin = true;
            }
            else if (loginstate == LoginState.NotLogin)
                strState = this.GetString("尚未登录");
            else if (loginstate == LoginState.Public)
                strState = this.GetString("访客");
            else if (loginstate == LoginState.Reader)
            {
                strState = this.GetString("读者");
                bLogin = true;
            }
            else if (loginstate == LoginState.OtherDomain)
            {
                strState = this.GetString("来自其它域");
                bLogin = true;
            }

            if (bLogin == true)
            {
                loginholder.Visible = false;
                logoutholder.Visible = true;
                visible = logout;
            }
            else
            {
                loginholder.Visible = true;
                logoutholder.Visible = false;
                visible = login;
            }

            visible.ToolTip = this.GetString("当前状态") + " - " + strState;

            /*
 * 
<titleBarControl>
<titleLeft lang='zh'>
<img src="./stylenew/logo_zh.gif" />
</titleLeft>
<titleLeft lang='en'>
<img src="./stylenew/logo_en.gif" />
</titleLeft>
...
 */
            string strLeftHtml = "";
            string strCenterHtml = "";
            XmlNode parent = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl");
            if (parent != null)
            {
                // titleLeft
                // XmlNode nodeTitleLeft = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/titleLeft");
                // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode
                // parameters:
                //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
                XmlNode nodeTitleLeft = DomUtil.GetLangedNode(
                    this.Lang,
                    parent,
                    "titleLeft");
                if (nodeTitleLeft != null)
                {
                    strLeftHtml = nodeTitleLeft.InnerXml;
                    strLeftHtml = UnmacroString(strLeftHtml);

#if DUMP
                    app = (OpacApplication)this.Page.Application["app"];
                    app.WriteErrorLog("strLeftHtml [" + nodeTitleLeft.InnerXml + "] --> [" + strLeftHtml + "]");
#endif
                }


                // titleCenter
                // XmlNode nodeTitleCenter = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/titleCenter");
                XmlNode nodeTitleCenter = DomUtil.GetLangedNode(
this.Lang,
parent,
"titleCenter");
                if (nodeTitleCenter != null)
                {
                    strCenterHtml = nodeTitleCenter.InnerXml;
                    strCenterHtml = UnmacroString(strCenterHtml);
                }

                LiteralControl lefthtml = (LiteralControl)this.FindControl("lefthtml");
                lefthtml.Text = strLeftHtml;

                LiteralControl centerhtml = (LiteralControl)this.FindControl("centerhtml");
                centerhtml.Text = strCenterHtml;

                // 2013/4/1
                bool bloginAnchorVisible = true;
                XmlNode nodeloginAnchor = DomUtil.GetLangedNode(
this.Lang,
parent,
"loginAnchor");
                if (nodeloginAnchor != null)
                {
                    string strError = "";
                    DomUtil.GetBooleanParam(nodeloginAnchor,
                        "visible",
                        true,
                        out bloginAnchorVisible,
                        out strError);
                    if (bloginAnchorVisible == false)
                    {
                        loginholder.Visible = false;
                        logoutholder.Visible = false;
                    }
                }
            }
            base.Render(writer);

            writer.Indent -= nParentCount;
        }

        // 匹配语言代码
        static string MatchLang(List<string> langs, string strLang)
        {
            string[] parts = strLang.Split(new char[] { '-' });

            for (int len = parts.Length; len >= 1; len--)
            {
                string strCurrentLang = GetPartsLang(strLang, len).ToLower();

                for (int j = 0; j < langs.Count; j++)
                {
                    string strTempLang = GetPartsLang(langs[j], len).ToLower();
                    if (strTempLang == strCurrentLang)
                        return langs[j];
                }
            }

            return null;    // 没有匹配
        }

        // 截取一段lang代码
        static string GetPartsLang(string strLang, int len)
        {
            string[] parts = strLang.Split(new char[] { '-' });
            string strCurrentLang = "";
            for (int i = 0; i < len; i++)
            {
                if (i >= parts.Length)
                    break;

                if (i > 0)
                    strCurrentLang += "-";
                strCurrentLang += parts[i];
            }

            return strCurrentLang;
        }
    }

    public enum TitleColumn
    {
        None = 0,
        BorrowInfo = 1, // 借阅信息
        ReservationInfo = 2,    // 预约
        MyBookShelf = 3,    // 我的书架
#if NO
        FellBackInfo = 3,   // 违约
        BorrowHistory = 4,  // 借阅历史
#endif
        Search = 5, // 检索
        Browse = 6, // 浏览
        BookReview = 7, // 书评
        Message = 8,    // 消息
        PersonalInfo = 9,   // 关于我 包括修改密码
        Statis = 10, // 统计信息
        ReaderInfo = 11, // 从管理员角度看的某读者信息
        Management = 12,    // 管理功能
    }

#if NO
    // 获得收件箱中未读信件数的事件
    public delegate void GetInboxUnreadCountEventHandler(object sender,
    GetInboxUnreadCountEventArgs e);

    public class GetInboxUnreadCountEventArgs : EventArgs
    {
        public bool First = false;
        public int UnreadCount = 0;
    }
#endif

    //
    public delegate void LibraryCodeChangedEventHandler(object sender,
    LibraryCodeChangedEventArgs e);

    public class LibraryCodeChangedEventArgs : EventArgs
    {
        public string OldLibraryCode = "";
        public string NewLibraryCode = "";
    }

    //
    public delegate void RefreshingEventHandler(object sender,
    RefreshingEventArgs e);

    public class RefreshingEventArgs : EventArgs
    {
        public bool Cancel = false; // [out]
    }
}
