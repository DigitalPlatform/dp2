using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 全局函数
    /// </summary>
    public class GlobalUtil
    {
        public static LoginState GetLoginState(Page page)
        {
            SessionInfo sessioninfo = (SessionInfo)page.Session["sessioninfo"];

            if (sessioninfo == null)
                return LoginState.NotLogin;

            if (String.IsNullOrEmpty(sessioninfo.UserID) == true)
                return LoginState.NotLogin;

            if (sessioninfo.UserID == "public")
                return LoginState.Public;

            if (sessioninfo.IsReader == true)
                return LoginState.Reader;

            // 避免这个“其它域”(xxx@xxx)和 @refID:xxx 形态混淆
            if (sessioninfo.UserID.StartsWith("@refID:") == false
                && sessioninfo.UserID.IndexOf("@") > 0/*!= -1*/)
                return LoginState.OtherDomain;

            return LoginState.Librarian;
        }
    }

    // 登录身份状态
    public enum LoginState
    {
        NotLogin = 0,   // 尚未登录
        Public = 1, // 访客客身份
        Reader = 2, // 读者
        Librarian = 3,  // 图书馆员
        OtherDomain = 4,  // 来自其他域的SSO登录用户 
    }
}
