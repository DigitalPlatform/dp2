using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Threading;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using System.Runtime.Serialization;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// Summary description for SessionInfo.
    /// </summary>
    public class SessionInfo
    {
#if NO
        public int LoginErrorCount = 0; // 最近连续密码错误的次数
#endif

        public const int DEFAULT_MAX_CLIENTS = 5;

        bool _closed = false;

        public bool Closed
        {
            get
            {
                return this._closed;
            }
        }

        /// <summary>
        /// 是否为评估模式
        /// </summary>
        public bool TestMode
        {
            get;
            set;
        }

        public string RouterClientIP = null;  // 访问 dp2Router 的前端 IP 地址。null 表示请求中没有 _dp2router_clientip 头字段；"" 表示有这个头字段，但 value 为 ""
        public string ClientIP = "";  // 前端 IP 地址
        public string Via = ""; // 经由什么协议

        public string SessionID = "";   // Session 唯一的 ID

        public bool NeedAutoClean = true;   // 是否需要自动清除
        public long CallCount = 0;

        public int Used = 0;

        public SessionTime SessionTime = null;

        public string Lang = "";

        // 刚才做过的最近一次Amerce的ID列表
        public List<string> AmerceIds = null;
        public string AmerceReaderBarcode = ""; // 注：可能包含机构代码部分

        // TODO: 所创建的临时文件要在规定的目录中
        // TODO: 观察它是否释放
        public DupResultSet DupResultSet = null;

        public LibraryApplication App = null;
        public RmsChannelCollection Channels = new RmsChannelCollection();

        private string m_strTempDir = "";	// 临时文件目录 2008/3/31
        public string TempDir
        {
            get
            {
                return m_strTempDir;
            }
        }

        //public string UserName = "";
        //public string Rights = "";

        //string m_strDp2UserName = "";
        //string m_strDp2Password = "";

        // public string GlobalErrorInfo = "";

#if NO
        public QuestionCollection Questions = new QuestionCollection();

        public int Step = 0;
#endif

        public Account Account = null;

        // public Stack LoginCallStack = new Stack();

        // public event ItemLoadEventHandler ItemLoad = null;
        // public event SetStartEventHandler SetStart = null;

        public string Dp2UserName
        {
            get
            {
                if (this.Account != null)
                {
                    if (this.Account.RmsUserName != "")
                        return this.Account.RmsUserName;
                }

                return App.ManagerUserName;
            }
        }

        public string Dp2Password
        {
            get
            {
                if (this.Account != null)
                {
                    if (this.Account.RmsUserName != "")
                        return this.Account.RmsPassword;
                }

                return App.ManagerPassword;
            }
        }

        public string UserID
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.UserID;
            }
        }

        public string UserType
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.Type;
            }
        }

        public string Rights
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.Rights;
            }
        }

        // 2010/10/27
        public string RightsOrigin
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.RightsOrigin;
            }
        }

        // 2012/9/24
        public QuickList RightsOriginList
        {
            get
            {
                if (this.Account == null)
                    return new QuickList();
                return this.Account.RightsOriginList;
            }
        }

        public string LibraryCodeList
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.AccountLibraryCode;
            }
        }

        string _expandLibraryCodeList = null;

        // 2022/3/6
        // 馆际互借涉及到的馆代码列表。可能比 LibraryCodeList 范围要大
        public string ExpandLibraryCodeList
        {
            get
            {
                if (_expandLibraryCodeList == null)
                {
                    string strExpandCodeList = this.LibraryCodeList;

                    int nRet = this.App.GetExpandCodeList(this,
        out strExpandCodeList,
        out string strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                    _expandLibraryCodeList = strExpandCodeList;
                    if (_expandLibraryCodeList == null)
                        _expandLibraryCodeList = "";
                }
                return _expandLibraryCodeList;
            }
        }

        // 是否为全局用户? 所谓全局用户就是管辖所有馆代码的用户
        public bool GlobalUser
        {
            get
            {
                return IsGlobalUser(this.LibraryCodeList);
            }
        }

        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            /*
            if (strLibraryCodeList == "*" || strLibraryCodeList == "<global>")
                return true;
            */

            return false;
        }

        public string Access
        {
            get
            {
                if (this.Account == null)
                    return "";
                return this.Account.Access;
            }
        }

        /// <summary>
        /// 用于日志记载的前端地址，包括 IP 和 Via 两个部分
        /// </summary>
        public string ClientAddress
        {
            get
            {
                return this.ClientIP + "@" + this.Via;
            }
        }

        public SessionInfo(LibraryApplication app,
            string strSessionID = "",
            List<RemoteAddress> address_list = null
            /*string strIP = "",
            string strVia = ""*/)
        {
            this.App = app;
            this.Channels.GUI = false;

            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(Channels_AskAccountInfo);
            this.Channels.AskAccountInfo += new AskAccountInfoEventHandle(Channels_AskAccountInfo);
            // this.Channels.procAskAccountInfo = new Delegate_AskAccountInfo(this.AskAccountInfo);

            this.m_strTempDir = PathUtil.MergePath(app.SessionDir, this.GetHashCode().ToString());

            this.SessionID = strSessionID;

            RemoteAddress nearest = RemoteAddress.FindClientAddress(address_list, "");
            if (nearest != null)
            {
                this.ClientIP = nearest.ClientIP;
                this.Via = nearest.Via;
            }

            RemoteAddress router = RemoteAddress.FindClientAddress(address_list, "dp2Router");
            if (router != null)
            {
                // dp2Router 的 Via 放在前面；原有 IP 和 Via 放在后面，构成新的 Via
                this.Via = router.ClientIP + "@dp2Router; " + this.ClientIP + "@" + this.Via;

                this.RouterClientIP = router.ClientIP;
            }
            else
                this.RouterClientIP = null;

            // TODO: 这里要改造，允许显示多个 client ip 在通道管理窗
        }

        public string GetTempDir()
        {
            Debug.Assert(this.m_strTempDir != "", "");

            PathUtil.TryCreateDir(this.m_strTempDir);	// 确保目录创建
            return this.m_strTempDir;
        }

        public void CloseSession()
        {
            if (this._closed == true)
                return;

            this._closed = true;

            // 2021/9/12
            this.App?.RemoveSesssionMemorySet(this);

            if (String.IsNullOrEmpty(this.m_strTempDir) == false)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(this.m_strTempDir);
                    if (di.Exists == true)
                        di.Delete(true);
                }
                catch
                {
                }
            }

            if (this.Channels != null)
                this.Channels.Dispose();

            // 2016/4/5
            if (this.DupResultSet != null)
                this.DupResultSet.Dispose();

            this.ClientIP = "";
        }

        void Channels_AskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            e.Owner = null;

            ///
            e.UserName = this.Dp2UserName;
            e.Password = this.Dp2Password;
            e.Result = 1;
        }

        // 登录
        // TODO: 多文种提示
        // parameters:
        //      strPassword 如果为null，表示不验证密码。因此需要格外注意，即便是空密码，如果要验证也需要使用""
        //      alter_type_list 已经实施的绑定验证类型和尚未实施的类型列表
        // return:
        //      -1  error
        //      0   user not found, or password error
        //      1   succeed
        public int Login(
            string strUserID,
            string strPassword,
            string strLocation,
            bool bPublicError,  // 是否模糊用户名和密码不匹配提示?
            string strClientIP,
            string strRouterClientIP,   // 2016/10/30
            string strGetToken,
            out bool passwordExpired,
            out List<string> alter_type_list,
            out string strRights,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strRights = "";
            strLibraryCode = "";
            alter_type_list = new List<string>();
            passwordExpired = false;

            if (this.App == null)
            {
                strError = "App == null";
                return -1;
            }

            Account account = null;

            int nRet = this.App.GetAccount(strUserID,
                out account,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                if (bPublicError == true)
                    strError = this.App.GetString("帐户不存在或密码不正确") + " session 1";
                return 0;
            }

            // 匹配 IP 地址
            if (string.IsNullOrEmpty(strClientIP) == false) // 2016/11/2
            {
                List<string> temp = new List<string>();
                bool bRet = account.MatchClientIP(strClientIP,
                    ref temp,
                    out strError);
                if (bRet == false)
                {
                    if (temp.Count == 0)
                        return -1;

                    // 不允许，又没有可以替代的方式，就要返回出错了
                    if (Account.HasAlterBindingType(temp) == false)
                        return -1;

                    // 有替代的验证方式，先继续完成登录
                    alter_type_list.AddRange(temp);
                }
                else
                    alter_type_list.AddRange(temp);

            }

            // 星号表示不进行 router client ip 检查
            // == null 表示当前前端不是通过 dp2Router 访问的，因而也就没有必要验证 router_ip 绑定了
            if (strRouterClientIP != null
                && strRouterClientIP != "*"
                )
            {
                List<string> temp = new List<string>();

                // 匹配 dp2Router 前端的 IP 地址
                bool bRet = account.MatchRouterClientIP(strRouterClientIP,
                    ref temp,
                    out strError);
                if (bRet == false)
                {
                    if (temp.Count == 0)
                        return -1;

                    // 不允许，又没有可以替代的方式，就要返回出错了
                    if (Account.HasAlterBindingType(temp) == false)
                        return -1;

                    // 否则继续完成登录
                    alter_type_list.AddRange(temp);
                }
                else
                    alter_type_list.AddRange(temp);

            }

            if (strPassword != null)
            {
                if (StringUtil.HasHead(strPassword, "token:") == true)
                {
                    string strToken = strPassword.Substring("token:".Length);
                    string strHashedPassword = "";
                    try
                    {
                        strHashedPassword = Cryptography.GetSHA1(account.Password);
                    }
                    catch
                    {
                        strError = "内部错误";
                        return -1;
                    }
                    // return:
                    //      -1  出错
                    //      0   验证不匹配
                    //      1   验证匹配
                    nRet = LibraryApplication.VerifyToken(
                        strClientIP,
                        strToken,
                        strHashedPassword,
                        (StringBuilder)null,
                        out strError);
                    if (nRet != 1)
                        return nRet;
                }
#if NO
                // 以前的做法
                else if (strPassword != account.Password)
                {
                    if (bPublicError == true)
                        strError = this.App.GetString("帐户不存在或密码不正确");
                    else
                        strError = this.App.GetString("密码不正确");
                    return 0;
                }
#endif
                else
                {
                    // 2021/7/3
                    // 检查密码失效期
                    if (App._passwordExpirePeriod != TimeSpan.MaxValue)
                    {
                        if (DateTime.Now > account.PasswordExpire)
                            passwordExpired = true;
                    }

                    nRet = LibraryServerUtil.MatchUserPassword(
                        account.PasswordType,
                        strPassword,
                        account.Password,
                        true, out strError);
                    if (nRet == -1)
                    {
                        strError = "MatchUserPassword() error: " + strError;
                        return -1;
                    }
                    if (nRet == 0)
                    {
                        if (bPublicError == true)
                            strError = this.App.GetString("帐户不存在或密码不正确") + " session 2";
                        else
                            strError = this.App.GetString("密码不正确");
                        return 0;
                    }

                    // 2021/7/16
                    // *** 检查密码强度
                    if (StringUtil.IsInList("login", this.App._passwordStyle) == true
                        && StringUtil.IsInList(strUserID, "reader,public,opac", false) == false)
                    {
                        var account_element = this.App.FindUserAccount(strUserID,
    out strError);
                        if (account_element == null)
                            return -1;
                        // return:
                        //      -1  出错
                        //      0   不合法(原因在 strError 中返回)
                        //      1   合法
                        nRet = LibraryApplication.ValidateUserPassword(
                            account_element,
                            strPassword,
                            this.App._passwordStyle,
                            false,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            // passwordExpired = true;
                            strError = $"账户现有密码强度不够，请修改密码后重新登录: {strError}";
                            return -1;
                        }
                    }
                }
            }

            this.Account = account;

            if (this.Account != null)
            {
                // 2022/2/25
                // 参数 strLocation 如果不为空，会覆盖 this.Account.Location
                if (string.IsNullOrEmpty(strLocation) == false)
                    this.Account.Location = strLocation;

                // 2016/6/7 给工作人员账户权限补上 librarian
                // 2017/1/16 加入 special_usernames 判断
                // if (Array.IndexOf(special_usernames, this.Account.UserID) == -1)
                if (LibraryServerUtil.IsSpecialUserName(this.Account.UserID) == false)
                {
                    string strTemp = this.Account.Rights;
                    StringUtil.SetInList(ref strTemp, "librarian", true);
                    this.Account.Rights = strTemp;
                }
            }

            strRights = this.RightsOrigin;
            strLibraryCode = this.LibraryCodeList;

            if (string.IsNullOrEmpty(strGetToken) == false)
            {
                string strHashedPassword = "";
                try
                {
                    strHashedPassword = Cryptography.GetSHA1(account.Password);
                }
                catch
                {
                    strError = "内部错误";
                    return -1;
                }
                string strToken = "";
                StringBuilder debugInfo = null; // new StringBuilder();
                nRet = LibraryApplication.MakeToken(strClientIP,
                    LibraryApplication.GetTimeRangeByStyle(strGetToken),
                    strHashedPassword,
                    debugInfo,
                    out strToken,
                    out strError);
                if (nRet == -1)
                    return -1;
                // this.App.WriteErrorLog($"(sessioninfo) MakeToken() return {nRet}, debugInfo='{debugInfo?.ToString()}'");
                if (string.IsNullOrEmpty(strToken) == false)
                    strRights += ",token:" + strToken;
            }

            if (this.App.CheckClientVersion == true
                && strUserID != "reader" && strUserID != "public" && strUserID != "opac")
                strRights += ",checkclientversion";

            // 2017/10/13
            if (string.IsNullOrEmpty(this.App.GlobalAddRights) == false)
                strRights += "," + this.App.GlobalAddRights;
            return 1;
        }

        /*
		// 获得缺省帐户信息
		// return:
		//		2	already login succeed
		//		1	dialog return OK
		//		0	dialog return Cancel
		//		-1	other error
		public int AskAccountInfo(ChannelCollection Channels, 
			string strComment,
			string strUrl,
			string strPath,
			LoginStyle loginStyle,
			out IWin32Window owner,	// 如果需要出现对话框，这里返回对话框的宿主Form
			out string strUserName,
			out string strPassword)
		{
			owner = null;

			///
			strUserName = this.Dp2UserName;
			strPassword = this.Dp2Password;

			return 1;
		}
         */

#if NO
        // 检索出册数据
        public int SearchItems(
            LibraryApplication app,
            string strItemDbName,
            string strBiblioRecId,
            out string strError)
        {
            strError = "";
            string strXml = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录")       // 2007/9/14
                + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            RmsChannel channel = this.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "没有找到";
                return 0;
            }

            long lHitCount = lRet;

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                -1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            for (int i = 0; i < aPath.Count; i++)
            {
                string strMetaData = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                lRet = channel.GetRes(aPath[i],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (this.ItemLoad != null)
                {
                    ItemLoadEventArgs e = new ItemLoadEventArgs();
                    e.Path = aPath[i];
                    e.Index = i;
                    e.Count = aPath.Count;
                    e.Xml = strXml;

                    this.ItemLoad(this, e);
                }
            }

            return aPath.Count;
        ERROR1:
            return -1;
        }
#endif

#if NO
        // 检索出册数据
        // 带有偏移量的版本
        // 2009/6/9
        // return:
        //      命中的全部结果数量。
        public int SearchItems(
            LibraryApplication app,
            string strItemDbName,
            string strBiblioRecId,
            int nStart,
            int nMaxCount,
            out string strError)
        {
            strError = "";
            string strXml = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录")       // 2007/9/14
                + "'><item><order>DESC</order><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            RmsChannel channel = this.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "没有找到";
                return 0;
            }

            long lHitCount = lRet;

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                nStart, // 0,
                nMaxCount, // -1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            for (int i = 0; i < aPath.Count; i++)
            {
                string strMetaData = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                lRet = channel.GetRes(aPath[i],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (this.ItemLoad != null)
                {
                    ItemLoadEventArgs e = new ItemLoadEventArgs();
                    e.Path = aPath[i];
                    e.Index = i;
                    e.Count = aPath.Count;
                    e.Xml = strXml;

                    this.ItemLoad(this, e);
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }
#endif

#if NO
        // 检索出评注数据
        // return:
        //      命中的全部结果数量。
        public long SearchComments(
            LibraryApplication app,
            string strCommentDbName,
            string strBiblioRecId,
            out string strError)
        {
            strError = "";
            // string strXml = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strCommentDbName + ":" + "父记录")       // 2007/9/14
                + "'><item><order>DESC</order><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            RmsChannel channel = this.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                return -1;

            // not found
            if (lRet == 0)
            {
                strError = "没有找到";
                return 0;
            }

            return lRet;
        }
#endif

#if NO
        // 获得一定范围的检索命中结果
        // return:
        public int GetCommentsSearchResult(
            LibraryApplication app,
            int nStart,
            int nMaxCount,
            bool bGetRecord,
            out string strError)
        {
            strError = "";

            RmsChannel channel = this.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            List<string> aPath = null;
            long lRet = channel.DoGetSearchResultEx(
                "default",
                nStart, // 0,
                nMaxCount, // -1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            long lHitCount = lRet;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            for (int i = 0; i < aPath.Count; i++)
            {

                if (bGetRecord == true)
                {
                    string strXml = "";
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (this.ItemLoad != null)
                    {
                        ItemLoadEventArgs e = new ItemLoadEventArgs();
                        e.Path = aPath[i];
                        e.Index = i;
                        e.Count = aPath.Count;
                        e.Xml = strXml;
                        e.Timestamp = timestamp;
                        e.TotalCount = (int)lHitCount;

                        this.ItemLoad(this, e);
                    }
                }
                else
                {

                    if (this.ItemLoad != null)
                    {
                        ItemLoadEventArgs e = new ItemLoadEventArgs();
                        e.Path = aPath[i];
                        e.Index = i;
                        e.Count = aPath.Count;
                        e.Xml = "";
                        e.Timestamp = null;
                        e.TotalCount = (int)lHitCount;

                        this.ItemLoad(this, e);
                    }
                }
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 根据特性的评注记录路径，获得一定范围的检索命中结果
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetCommentsSearchResult(
            LibraryApplication app,
            int nPerCount,
            string strCommentRecPath,
            bool bGetRecord,
            out int nStart,
            out string strError)
        {
            strError = "";
            nStart = -1;

            RmsChannel channel = this.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lHitCount = 0;

            bool bFound = false;
            List<string> aPath = null;
            for (int j = 0; ; j++)
            {
                nStart = j * nPerCount;
                long lRet = channel.DoGetSearchResultEx(
                    "default",
                    nStart, // 0,
                    nPerCount, // -1,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lHitCount = lRet;

                if (lHitCount == 0)
                    return 0;

                if (aPath.Count == 0)
                    break;

                for (int i = 0; i < aPath.Count; i++)
                {
                    if (aPath[i] == strCommentRecPath)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == true)
                    break;

                if (nStart >= lHitCount)
                    break;
            }

            if (bFound == true)
            {
                if (this.SetStart != null)
                {
                    SetStartEventArgs e = new SetStartEventArgs();
                    e.StartIndex = nStart;

                    this.SetStart(this, e);
                }

                for (int i = 0; i < aPath.Count; i++)
                {
                    if (bGetRecord == true)
                    {
                        string strXml = "";
                        string strMetaData = "";
                        byte[] timestamp = null;
                        string strOutputPath = "";

                        long lRet = channel.GetRes(aPath[i],
                            out strXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (this.ItemLoad != null)
                        {
                            ItemLoadEventArgs e = new ItemLoadEventArgs();
                            e.Path = aPath[i];
                            e.Index = i;
                            e.Count = aPath.Count;
                            e.Xml = strXml;
                            e.Timestamp = timestamp;
                            e.TotalCount = (int)lHitCount;

                            this.ItemLoad(this, e);
                        }
                    }
                    else
                    {
                        if (this.ItemLoad != null)
                        {
                            ItemLoadEventArgs e = new ItemLoadEventArgs();
                            e.Path = aPath[i];
                            e.Index = i;
                            e.Count = aPath.Count;
                            e.Xml = "";
                            e.Timestamp = null;
                            e.TotalCount = (int)lHitCount;

                            this.ItemLoad(this, e);
                        }
                    }
                }

                return 1;   // 找到
            }

            nStart = -1;
            strError = "路径为 '"+strCommentRecPath+"' 的记录在结果集中没有找到";
            return 0;   // 没有找到
        ERROR1:
            return -1;
        }
#endif
    }

#if NO
    /// <summary>
    /// 册信息到来事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ItemLoadEventHandler(object sender,
ItemLoadEventArgs e);

    /// <summary>
    /// 册信息到来事件的参数
    /// </summary>
    public class ItemLoadEventArgs : EventArgs
    {
        /// <summary>
        /// 记录全路径。
        /// </summary>
        public string Path = "";

        public int Index = -1;  // 在若干册记录中的顺序,从0开始计数

        public int Count = 0;   // 册总数 本次Index涉及的范围

        public int TotalCount = 0;  // 检索命中的总事项数// 2010/11/9

        public string Xml = ""; // 记录

        public byte[] Timestamp = null; // 2010/11/8
    }

    public delegate void SetStartEventHandler(object sender,
SetStartEventArgs e);

    /// <summary>
    /// 册信息到来事件的参数
    /// </summary>
    public class SetStartEventArgs : EventArgs
    {
        public int StartIndex = -1; 
    }
#endif

    public class SessionTime
    {
        public DateTime CreateTime = DateTime.Now;
        public DateTime LastUsedTime = DateTime.Now;
        // public string SessionID = "";
        // TODO: 是否登录过，也决定了通道的空闲生存时间
    }

    /// <summary>
    /// SessionInfo 存储结构
    /// SessionTable 本身是个 Hashtable，以 Session ID 字符串 --> SessionInfo 方式存储了所有 SessionInfo
    /// </summary>
    public class SessionTable : Hashtable
    {
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        int _nMaxCount = 10000;

        /// <summary>
        /// 最多允许访问 dp2Library 的前端机器数量
        /// </summary>
        public int MaxClients = SessionInfo.DEFAULT_MAX_CLIENTS; // -1 表示不限制 (0 表示除了 localhost 其他一概不许连接)

        Hashtable _ipTable = new Hashtable();   // IP -- Session 数量 对照表

#if NO
        Hashtable _ipNullTable = new Hashtable();   // IP -- (没有Session的)Channel 数量 对照表

        public void IncNullIpCount(string strIP, int nDelta)
        {
            lock (_ipNullTable)
            {
                long v = 0;
                if (this._ipNullTable.ContainsKey(strIP) == true)
                    v = (long)this._ipNullTable[strIP];
                this._ipNullTable[strIP] = v + nDelta;
            }
        }
#endif

        // TODO: 不知此函数为何被跳过 2016/10/29
        public void IncNullIpCount(string strIP, int nDelta)
        {
            return;

#if NO
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                _incIpCount(strIP, nDelta);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
#endif
        }


#if NO
        public void PrepareNullSession( // LibraryApplication app,
    string strSessionID,
    string strIP,
    string strVia)
        {
            if (strSessionID == null)
                return;

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                if (this.ContainsKey(strSessionID) == true)
                    return;
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (this.Count > _nMaxCount)
                throw new ApplicationException("Session 数量超过 " + _nMaxCount.ToString());

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                this[strSessionID] = null;
                IncIpCount(strIP, 1);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void DeleteSession(string strSessionID, string strIP)
        {
            if (strSessionID == null)
                return;

            SessionInfo sessioninfo = null;

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                // this.Remove(sessioninfo.SessionTime.SessionID);
                sessioninfo = (SessionInfo)this[strSessionID];
                this.Remove(strSessionID);
                IncIpCount(strIP, -1);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            if (sessioninfo != null)
                sessioninfo.CloseSession();
        }
#endif
        public int MaxSessionsPerIp = 50;
        public int MaxSessionsLocalHost = 150;

        // 2017/10/20
        // 特殊的，需要放开通道数限制到 150 个的那些 IP 地址
        public List<string> SpecialIpList { get; set; }

        // parameters:
        //      bAutoCreate 是否自动创建 SessionInfo 对象? true 表示自动创建; false 表示不自动创建(只返回已存在的 SessionInfo 对象)
        public SessionInfo PrepareSession(LibraryApplication app,
            string strSessionID,
            List<RemoteAddress> address_list,
            bool bAutoCreate = true
            /*string strIP,
            string strVia*/)
        {
            SessionInfo sessioninfo = null;

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                sessioninfo = (SessionInfo)this[strSessionID];
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (sessioninfo != null)
            {
                Debug.Assert(sessioninfo.SessionTime != null, "");
                sessioninfo.SessionTime.LastUsedTime = DateTime.Now;
#if NO
                if (sessioninfo.SessionTime.SessionID != strSessionID)
                {
                    Debug.Assert(false, "");
                    sessioninfo.SessionTime.SessionID = strSessionID;
                }
#endif
                if (sessioninfo.SessionID != strSessionID)
                {
                    Debug.Assert(false, "");
                    sessioninfo.SessionID = strSessionID;
                }
                return sessioninfo;
            }

            if (bAutoCreate == false)
            {
                // Debug.WriteLine("session '" + strSessionID + "' not found");
                return null;
            }

            if (this.Count > _nMaxCount)
                throw new ApplicationException("Session 数量超过 " + _nMaxCount.ToString());

            sessioninfo = new SessionInfo(app, strSessionID, address_list);
            sessioninfo.SessionTime = new SessionTime();
#if NO
            sessioninfo.SessionTime.SessionID = strSessionID;
#endif

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                string strIP = "";
                RemoteAddress address = RemoteAddress.FindClientAddress(address_list, "");
                if (address != null)
                    strIP = address.ClientIP;

                long v = _incIpCount(strIP, 1);

                int nMax = this.MaxSessionsPerIp;
                // if (strIP == "::1" || strIP == "127.0.0.1" || strIP == "localhost")
                if (IsLocalhost(strIP) == true)
                    nMax = this.MaxSessionsLocalHost;
                else if (IsSpecialIp(strIP) == true)
                    nMax = this.MaxSessionsLocalHost;

                if (v >= nMax)
                {
                    // 注意 Session 是否 Dispose() ?
                    _incIpCount(strIP, -1);
                    throw new OutofSessionException("Session 资源不足，通道创建失败。(配额值 " + nMax.ToString() + ")");
                }

                // 没有超过配额的才加入
                this[strSessionID] = sessioninfo;

                return sessioninfo;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 过滤 SessionInfo
        // return:
        //      true    命中
        //      false   未命中
        public delegate bool Delegate_filterSession(SessionInfo info);

        // 通用的选择性关闭 Session 函数
        public int CloseSessionBy(Delegate_filterSession filter_func)
        {
            List<string> remove_keys = new List<string>();

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    SessionInfo info = (SessionInfo)this[key];

                    if (info == null)
                        continue;

                    if (filter_func(info))
                    {
                        remove_keys.Add(key);   // 这里不能删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return 0;   // 没有找到

            int nCount = 0;
            List<SessionInfo> delete_sessions = new List<SessionInfo>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in remove_keys)
                {
                    SessionInfo sessioninfo = (SessionInfo)this[key];
                    if (sessioninfo == null)
                        continue;

                    // DeleteSession(sessioninfo, false);

                    // 和 sessionid 的 hashtable 脱离关系
                    this.Remove(key);

                    delete_sessions.Add(sessioninfo);

                    if (string.IsNullOrEmpty(sessioninfo.ClientIP) == false)
                    {
                        _incIpCount(sessioninfo.ClientIP, -1);
                        sessioninfo.ClientIP = "";
                    }

                    nCount++;
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 把 CloseSession 放在锁定范围外面，主要是想尽量减少锁定的时间
            foreach (SessionInfo info in delete_sessions)
            {
                info.CloseSession();
            }
            return nCount;
        }

        public int CloseSessionByUserID(string strUserID)
        {
            return CloseSessionBy((info) =>
            {
                if (info == null)
                    return false;
                return (info.UserID == strUserID);
            });
        }

        public int CloseSessionByClientIP(string strClientIP)
        {
            return CloseSessionBy((info) =>
            {
                if (info == null)
                    return false;
                return (info.ClientIP == strClientIP);
            });
        }
#if NO
        public int CloseSessionByClientIP(string strClientIP)
        {
            List<string> remove_keys = new List<string>();

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    SessionInfo info = (SessionInfo)this[key];

                    if (info == null)
                        continue;

                    if (info.ClientIP == strClientIP)
                    {
                        remove_keys.Add(key);   // 这里不能删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return 0;   // 没有找到

            int nCount = 0;
            List<SessionInfo> delete_sessions = new List<SessionInfo>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in remove_keys)
                {
                    SessionInfo sessioninfo = (SessionInfo)this[key];
                    if (sessioninfo == null)
                        continue;

                    // DeleteSession(sessioninfo, false);

                    // 和 sessionid 的 hashtable 脱离关系
                    this.Remove(key);

                    delete_sessions.Add(sessioninfo);

                    if (string.IsNullOrEmpty(sessioninfo.ClientIP) == false)
                    {
                        _incIpCount(sessioninfo.ClientIP, -1);
                        sessioninfo.ClientIP = "";
                    }

                    nCount++;
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 把 CloseSession 放在锁定范围外面，主要是想尽量减少锁定的时间
            foreach (SessionInfo info in delete_sessions)
            {
                info.CloseSession();
            }
            return nCount;
        }
#endif
        public bool CloseSessionBySessionID(string strSessionID)
        {
            SessionInfo sessioninfo = null;

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                sessioninfo = (SessionInfo)this[strSessionID];
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (sessioninfo == null)
                return false;   // 没有找到

            DeleteSession(sessioninfo);

            return true;
        }

        public int CloseSessionByReaderBarcode(string strReaderBarcode)
        {
            return CloseSessionBy((info) =>
            {
                if (info != null && info.Account != null
                    && info.Account.Barcode == strReaderBarcode
                    && info.Account.Barcode == info.Account.UserID)
                    return true;
                return false;
            });
        }

#if NO
        // 读者记录发生修改后，要把和该读者登录的 Session 给清除，这样就避免后面用到旧的读者权限
        public int CloseSessionByReaderBarcode(string strReaderBarcode)
        {
            List<string> remove_keys = new List<string>();

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    SessionInfo info = (SessionInfo)this[key];

                    if (info == null)
                        continue;

                    if (info.Account == null)
                        continue;

                    if (info.Account.Barcode == strReaderBarcode
                        && info.Account.Barcode == info.Account.UserID)
                        remove_keys.Add(key);   // 这里不能删除，因为 foreach 还要用枚举器
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return 0;   // 没有找到

            int nCount = 0;
            List<SessionInfo> delete_sessions = new List<SessionInfo>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in remove_keys)
                {
                    SessionInfo sessioninfo = (SessionInfo)this[key];
                    if (sessioninfo == null)
                        continue;

                    // 和 sessionid 的 hashtable 脱离关系
                    this.Remove(key);

                    delete_sessions.Add(sessioninfo);

                    if (string.IsNullOrEmpty(sessioninfo.ClientIP) == false)
                    {
                        _incIpCount(sessioninfo.ClientIP, -1);
                        sessioninfo.ClientIP = "";
                    }

                    nCount++;
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 把 CloseSession 放在锁定范围外面，主要是想尽量减少锁定的时间
            foreach (SessionInfo info in delete_sessions)
            {
                info.CloseSession();
            }
            return nCount;
        }
#endif

        public void DeleteSession(SessionInfo sessioninfo,
            bool bLock = true)
        {
            if (sessioninfo == null)
                return;

            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
            }
            try
            {
                // this.Remove(sessioninfo.SessionTime.SessionID);
                this.Remove(sessioninfo.SessionID);
                if (string.IsNullOrEmpty(sessioninfo.ClientIP) == false)
                {
                    _incIpCount(sessioninfo.ClientIP, -1);
                    sessioninfo.ClientIP = "";   // 避免后面多次调用时重复减去 ip 计数
                }
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ExitWriteLock();
            }

            sessioninfo.CloseSession();
        }

        public bool IsFull
        {
            get
            {
                if (this.Count >= _nMaxCount)
                    return true;

                return false;
            }
        }

        public void CleanSessions(TimeSpan delta)
        {
            List<string> remove_keys = new List<string>();

            // 读锁定并不阻碍一般性访问
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    SessionInfo info = (SessionInfo)this[key];

                    if (info == null)
                        continue;

                    if (info.NeedAutoClean == false)
                        continue;

                    if (info.SessionTime == null)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    if ((DateTime.Now - info.SessionTime.LastUsedTime) >= delta)
                    {
                        // 2017/5/7 正在使用中的 SessionInfo 不要清除
                        if (info.Used == 0)
                            remove_keys.Add(key);   // 这里不能删除，因为 foreach 还要用枚举器
                    }
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (remove_keys.Count == 0)
                return;

            // 因为要删除某些元素，所以用写锁定
            List<SessionInfo> delete_sessions = new List<SessionInfo>();
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                // 2013.11.1
                foreach (string key in remove_keys)
                {
                    SessionInfo info = (SessionInfo)this[key];
                    if (info == null)
                        continue;   // sessionid 没有找到对应的 Session 对象

                    // 和 sessionid 的 hashtable 脱离关系
                    this.Remove(key);

                    delete_sessions.Add(info);

                    if (string.IsNullOrEmpty(info.ClientIP) == false)
                    {
                        _incIpCount(info.ClientIP, -1);
                        info.ClientIP = "";
                    }
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 把 CloseSession 放在锁定范围外面，主要是想尽量减少锁定的时间
            foreach (SessionInfo info in delete_sessions)
            {
                info.CloseSession();
            }
        }

        // 如果 IP 事项总数超过限额，会抛出异常
        public long IncIpCount(string strIP, int nDelta)
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                return _incIpCount(strIP, nDelta);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public static bool IsLocalhost(string strIP)
        {
            if (strIP == "::1" || strIP == "127.0.0.1" || strIP == "localhost")
                return true;
            return false;
        }

        public bool IsSpecialIp(string strIP)
        {
            if (this.SpecialIpList == null || this.SpecialIpList.Count == 0)
                return false;
            if (this.SpecialIpList.IndexOf(strIP) != -1)
                return true;
            return false;
        }

        // 增量 IP 统计数字
        // 如果 IP 事项总数超过限额，会抛出异常
        // parameters:
        //      strIP   前端机器的 IP 地址。还用于辅助判断是否超过 MaxClients。localhost 是不计算在内的
        long _incIpCount(string strIP, int nDelta)
        {
            // this.MaxClients = 0;    // test

            long v = 0;
            if (this._ipTable.ContainsKey(strIP) == true)
                v = (long)this._ipTable[strIP];
            else
            {
                if (this.Count > _nMaxCount
                    && v + nDelta != 0)
                    throw new OutofSessionException("IP 条目数量超过 " + _nMaxCount.ToString());

                // 判断前端机器台数是否超过限制数额 2014/8/23
                if (this.MaxClients != -1
                    && IsLocalhost(strIP) == false
                    && this.GetClientIpAmount() >= this.MaxClients
                    && v + nDelta != 0)
                    throw new OutofClientsException("前端机器数量已经达到 " + this.GetClientIpAmount().ToString() + " 个 ( 现有IP: " + StringUtil.MakePathList(GetIpList(), ", ") + " 试图申请的IP: " + strIP + ")。请先释放出通道然后重新访问");

            }

            if (v + nDelta == 0)
                this._ipTable.Remove(strIP); // 及时移走计数器为 0 的条目，避免 hashtable 尺寸太大
            else
                this._ipTable[strIP] = v + nDelta;

            return v;   // 返回增量前的数字
        }

        // 获得当前除了 localhost 以外的 IP 总数
        int GetClientIpAmount()
        {
            if (this._ipTable.Count == 0)
                return 0;

            // 排除 localhost
            int nDelta = 0;
            if (this._ipTable.ContainsKey("::1") == true)
                nDelta -= 1;
            if (this._ipTable.ContainsKey("127.0.0.1") == true)
                nDelta -= 1;
            if (this._ipTable.ContainsKey("localhost") == true)
                nDelta -= 1;

            return this._ipTable.Count + nDelta;
        }

        // 获得当前正在使用的 IP 列表，为报错显示用途。其中 localhost 会标出 (未计入)
        List<string> GetIpList()
        {
            List<string> results = new List<string>();
            foreach (string ip in this._ipTable.Keys)
            {
                if (IsLocalhost(ip) == true)
                    results.Add("localhost(未计入)");
                else
                    results.Add(ip);
            }

            return results;
        }

        // 根据 ip 地址聚集其它字段信息
        void GatherFields(ref List<ChannelInfo> infos)
        {
            List<string> ips = new List<string>();
            foreach (ChannelInfo info in infos)
            {
                ips.Add(info.ClientIP);
            }

            List<ChannelInfo> results = GatherFields(ips);
            int i = 0;
            foreach (string ip in ips)
            {
                ChannelInfo info = infos[i];
                foreach (ChannelInfo result in results)
                {
                    if (result.ClientIP == ip)
                    {
                        result.Count = info.Count;
                        infos[i] = result;
                        break;
                    }
                }

                i++;
            }

        }

        public class ChannelInfoComparer : IComparer<ChannelInfo>
        {
            int IComparer<ChannelInfo>.Compare(ChannelInfo x, ChannelInfo y)
            {
                // 如果权值相同，则依据序号。序号小的更靠前
                return string.Compare(x.ClientIP, y.ClientIP);
            }
        }

        List<ChannelInfo> GatherFields(List<string> ips)
        {
            List<ChannelInfo> results = new List<ChannelInfo>();

            List<ChannelInfo> infos = new List<ChannelInfo>();

#if NO
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
#endif
            foreach (string sessionid in this.Keys)
            {
                SessionInfo session = (SessionInfo)this[sessionid];
                if (session == null)
                    continue;

                if (ips.IndexOf(session.ClientIP) == -1)
                    continue;

                ChannelInfo info = new ChannelInfo();
                info.SessionID = session.SessionID;
                info.ClientIP = session.ClientIP;
                info.UserName = session.UserID;
                info.LibraryCode = session.LibraryCodeList;
                info.Via = session.Via;
                info.Count = 1;
                info.CallCount = session.CallCount;
                info.Lang = session.Lang;
                if (session.Account != null)
                    info.Location = session.Account.Location;

                infos.Add(info);
            }
#if NO
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
#endif

            // 按照 IP 地址排序
            infos.Sort(new ChannelInfoComparer());

            List<string> usernames = new List<string>();
            List<string> locations = new List<string>();
            List<string> librarycodes = new List<string>();
            List<string> vias = new List<string>();
            List<string> langs = new List<string>();
            ChannelInfo current = null;
            foreach (ChannelInfo info in infos)
            {
                if (current != null && info.ClientIP != current.ClientIP)
                {
                    // 输出一个结果
                    ChannelInfo result = new ChannelInfo();
                    result.ClientIP = current.ClientIP;

                    StringUtil.RemoveDupNoSort(ref usernames);
                    result.UserName = StringUtil.MakePathList(usernames);

                    StringUtil.RemoveDupNoSort(ref locations);
                    result.Location = StringUtil.MakePathList(locations);

                    StringUtil.RemoveDupNoSort(ref librarycodes);
                    result.LibraryCode = StringUtil.MakePathList(librarycodes);

                    StringUtil.RemoveDupNoSort(ref vias);
                    result.Via = StringUtil.MakePathList(vias);

                    StringUtil.RemoveDupNoSort(ref langs);
                    result.Lang = StringUtil.MakePathList(langs);

                    results.Add(result);

                    current = info;

                    usernames.Clear();
                    locations.Clear();
                    librarycodes.Clear();
                    vias.Clear();
                    langs.Clear();
                }

                usernames.Add(info.UserName);
                locations.Add(info.Location);
                librarycodes.Add(info.LibraryCode);
                vias.Add(info.Via);
                langs.Add(info.Lang);

                if (current == null)
                    current = info;
            }

            if (current != null && usernames.Count > 0)
            {
                // 最后输出一次
                ChannelInfo result = new ChannelInfo();
                result.ClientIP = current.ClientIP;

                StringUtil.RemoveDupNoSort(ref usernames);
                result.UserName = StringUtil.MakePathList(usernames);

                StringUtil.RemoveDupNoSort(ref locations);
                result.Location = StringUtil.MakePathList(locations);

                StringUtil.RemoveDupNoSort(ref librarycodes);
                result.LibraryCode = StringUtil.MakePathList(librarycodes);

                StringUtil.RemoveDupNoSort(ref vias);
                result.Via = StringUtil.MakePathList(vias);

                StringUtil.RemoveDupNoSort(ref langs);
                result.Lang = StringUtil.MakePathList(langs);

                results.Add(result);
            }

            return results;
        }

        // 列出指定的通道信息
        public int ListChannels(
            string strClientIP,
            string strUserName,
            string strStyle,
            out List<ChannelInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<ChannelInfo>();

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                // 按照 IP 地址聚集
                // strClientIP 参数可用于筛选
                if (strStyle == "ip-count")
                {

                    foreach (string ip in this._ipTable.Keys)
                    {
                        if (string.IsNullOrEmpty(strClientIP) == true
                            || strClientIP == "*")
                        {
                        }
                        else if (ip != strClientIP)
                            continue;

                        ChannelInfo info = new ChannelInfo();
                        info.ClientIP = ip;
                        // TODO: UserName 可以累积所有用过的
                        info.Count = (long)this._ipTable[ip];

                        infos.Add(info);
                    }

                    // 根据 ip 地址聚集其它字段信息
                    if (infos.Count > 0)
                        GatherFields(ref infos);


#if NO
                    // 列出没有分配 Session 的通道数量
                    foreach (string ip in this._ipNullTable.Keys)
                    {
                        if (string.IsNullOrEmpty(strClientIP) == true
                            || strClientIP == "*")
                        {
                        }
                        else if (ip != strClientIP)
                            continue;
                        ChannelInfo info = new ChannelInfo();
                        info.ClientIP = ip;
                        info.Location = "<null>";
                        // TODO: UserName 可以累积所有用过的
                        info.Count = (long)this._ipNullTable[ip];

                        infos.Add(info);
                    }
#endif

                    return 0;
                }

                // 全部列出
                // strClientIP strUserName 参数可用于筛选
                if (string.IsNullOrEmpty(strStyle) == true)
                {
                    foreach (string sessionid in this.Keys)
                    {
                        SessionInfo session = (SessionInfo)this[sessionid];

                        if (session == null)
                        {
#if NO
                            ChannelInfo info = new ChannelInfo();
                            info.SessionID = sessionid;
                            info.Location = "<null>";

                            infos.Add(info);
#endif
                            continue;
                        }

                        if (string.IsNullOrEmpty(strClientIP) == true
                            || strClientIP == "*")
                        {
                        }
                        else if (session.ClientIP != strClientIP)
                            continue;

                        if (string.IsNullOrEmpty(strUserName) == true
    || strUserName == "*")
                        {
                        }
                        else if (session.UserID != strUserName)
                            continue;

                        ChannelInfo info = new ChannelInfo();
                        info.SessionID = session.SessionID;
                        info.ClientIP = session.ClientIP;
                        info.UserName = session.UserID;
                        info.LibraryCode = session.LibraryCodeList;
                        info.Via = session.Via;
                        info.Count = 1;
                        info.CallCount = session.CallCount;
                        info.Lang = session.Lang;
                        if (session.Account != null)
                            info.Location = session.Account.Location;

                        infos.Add(info);
                    }

                    return 0;
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            return 0;
        }
    }

    /// <summary>
    /// Session 资源不足异常
    /// </summary>
    public class OutofSessionException : Exception
    {
        public OutofSessionException(string strText)
            : base(strText)
        {
        }
    }

    /// <summary>
    /// 超过规定前端机器台数异常
    /// </summary>
    public class OutofClientsException : Exception
    {
        public OutofClientsException(string strText)
            : base(strText)
        {
        }
    }

    /// <summary>
    /// 通讯通道信息
    /// </summary>
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ChannelInfo
    {
        [DataMember]
        public string SessionID = "";    // Session id， Session 唯一的标识

        [DataMember]
        public string UserName = "";    // 用户名

        [DataMember]
        public string ClientIP = "";    // 前端 IP

        [DataMember]
        public string Via = "";  // 经由什么协议

        [DataMember]
        public long Count = 0;    // 此项数目

        [DataMember]
        public string LibraryCode = ""; // 图书馆代码

        [DataMember]
        public string Location = "";    // 前端地点注释

        [DataMember]
        public long CallCount = 0;    // 通道迄今被调用的次数

        [DataMember]
        public string Lang = "";  // 语言代码
    }
}
