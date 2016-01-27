// #define USE_SESSION_CHANNEL

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Collections;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// Summary description for SessionInfo.
    /// </summary>
    public class SessionInfo
    {
        public OpacApplication App = null;
#if USE_SESSION_CHANNEL
        public LibraryChannel Channel = new LibraryChannel();
#endif

        private string m_strTempDir = "";	// 临时文件目录 2008/3/31

        public PostedFileInfo PostedFileInfo = null;

        //public string UserName = "";
        //public string Rights = "";

        //string m_strDp2UserName = "";
        //string m_strDp2Password = "";

        // public int Step = 0;
        string m_strUserName = "";
        public string UserID
        {
            get
            {
                return this.m_strUserName;
            }
            set
            {
                this.m_strUserName = value;
            }
        }

        string m_strPassword = "";

        public string Password
        {
            get
            {
                return this.m_strPassword;
            }
            set
            {
                this.m_strPassword = value;
            }
        }

        string m_strParameters = "";

        public string Parameters
        {
            get
            {
                return this.m_strParameters;
            }
            set
            {
                this.m_strParameters = value;
            }
        }

#if USE_SESSION_CHANNEL
        public string RightsOrigin
        {
            get
            {
                if (this.Channel == null)
                    return "";
                return this.Channel.Rights;
            }
        }
#else
        string _rights = "";    // 登录成功后这里记忆权限值
        public string RightsOrigin
        {
            get
            {
                return _rights;
            }
        }
#endif

        bool m_bIsReader = true;

        public bool IsReader
        {
            get
            {
                return this.m_bIsReader;
            }
            set
            {
                this.m_bIsReader = value;
            }
        }

        string m_strChannelLang = "zh-CN";
        public string ChannelLang
        {
            get
            {
                return this.m_strChannelLang;
            }
            set
            {
                if (value == null)
                    return; // null 什么也无所指，就不必执行了

                string strOldValue = this.m_strChannelLang;
                this.m_strChannelLang = value;

                // TODO: 如何设置 channel 的语言 ?
#if USE_SESSION_CHANNEL
                if (strOldValue != value
                    && (string.IsNullOrEmpty(strOldValue) == false || string.IsNullOrEmpty(value) == false))    // 两个值至少一个非空
                {
                    string strError = "";
                    string strOldLang = "";
                    this.Channel.SetLang(null,
                        value,
                        out strOldLang,
                        out strError);
                }
#endif
            }
        }

        // 头像URL。这是指社区头像，而不是证件照片
        string m_strPhotoUrl = "";
        public string PhotoUrl
        {
            get
            {
                return this.m_strPhotoUrl;
            }
            set
            {
                this.m_strPhotoUrl = value;
            }
        }

        // SSO权限
        string m_strSsoRights = "";
        public string SsoRights
        {
            get
            {
                return this.m_strSsoRights;
            }
            set
            {
                this.m_strSsoRights = value;
            }
        }

        public Stack LoginCallStack = new Stack();

        public ReaderInfo ReaderInfo = null;

#if NO
        public event ItemLoadEventHandler ItemLoad = null;
        public event SetStartEventHandler SetStart = null;
#endif

        public string ClientIP = "";    // 前端的 IP 地址

        public SessionInfo(OpacApplication app)
        {
            Debug.Assert(app != null, "");
            this.App = app;
#if USE_SESSION_CHANNEL
            this.Channel.Url = app.WsUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);
#endif
            this.m_strTempDir = PathUtil.MergePath(app.SessionDir, this.GetHashCode().ToString());
        }

#if USE_SESSION_CHANNEL
        // 2015/1/22
        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            if (string.IsNullOrEmpty(channel.UserName) == true)
                throw new Exception("Channel_AfterLogin() channel.UserName 为空 (此时 SessionInfo.m_strUserName 为 '"+this.m_strUserName+"')");
            this.m_strUserName = channel.UserName;
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = m_strUserName;
            e.Password = m_strPassword;

            if (string.IsNullOrEmpty(m_strParameters) == false)
                e.Parameters = m_strParameters; // 2014/12/23 SSO Login 会这么用
            else
            {
                if (StringUtil.HasHead(m_strPassword, "token:") == true)
                {
                    Hashtable parameters = new Hashtable();
                    parameters["index"] = "-1";
                    if (this.m_bIsReader == true)
                        parameters["type"] = "reader";
                    parameters["simulate"] = "yes";
                    parameters["location"] = "#opac_token@" + this.ClientIP;
                    parameters["client"] = "dp2OPAC|" + OpacApplication.ClientVersion;

                    e.Parameters = StringUtil.BuildParameterString(parameters, ',', '=');

                    // e.Parameters = "location=#opac_token@" + this.ClientIP + ",index=-1,type=reader,simulate=yes,libraryCode=";
                    e.Password = this.App.ManagerUserName + "," + this.App.ManagerPassword + "|||" + m_strPassword;   // simulate登录的需要
                }
                else
                {
                    e.Parameters = "location=#opac@" + this.ClientIP + ",client=dp2OPAC|" + OpacApplication.ClientVersion;
                    if (m_bIsReader == true)
                        e.Parameters += ",type=reader,libraryCode=";    // TODO: 可以用一个参数设定馆代码限制范围
                }
            }

            e.LibraryServerUrl = App.WsUrl;
        }
#endif

        // 获得一个通道
        // 分为两种方式，一种是使用自己携带的通道，一种是从通道池中分配
        public LibraryChannel GetChannel(bool bPool, string strParam = "")
        {
#if USE_SESSION_CHANNEL
            if (bPool == false)
                return this.Channel;
#endif

            LibraryChannel channel = this.App.ChannelPool.GetChannel(this.App.WsUrl, this.UserID);
            channel.Password = this.Password;
            if (channel.UserName != this.UserID)    // 有可能会从 pool 中拿到 UserName 为空的 channel 2016/1/25
                channel.UserName = this.UserID;

            // 2015/6/11
            if (string.IsNullOrEmpty(strParam) == true)
            {
                if (string.IsNullOrEmpty(this.m_strParameters) == false)
                    channel.Param = this.m_strParameters;   // 2015/11/20 SSO Login 会这么用
                else
                {
                    string strParameters = "location=#opac@" + this.ClientIP + ",client=dp2OPAC|" + OpacApplication.ClientVersion;
                    if (m_bIsReader == true)
                        strParameters += ",type=reader,libraryCode=";    // TODO: 可以用一个参数设定馆代码限制范围
                    channel.Param = strParameters;  // Tag
                }
            }
            else
                channel.Param = strParam;

            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this.App.ChannelPool.ReturnChannel(channel);
        }

        public string GetTempDir()
        {
            Debug.Assert(this.m_strTempDir != "", "");

            PathUtil.CreateDirIfNeed(this.m_strTempDir);	// 确保目录创建
            return this.m_strTempDir;
        }

        public void CloseSession()
        {
            try
            {
#if USE_SESSION_CHANNEL
                if (this.Channel != null)
                {
                    this.Channel.Close();
                    this.Channel = null;
                }
#endif
            }
            catch
            {
            }
            // this.ClearFilterTask(); 后面一句 ClearTempFiles() 会删除所有临时文件
            this.ClearTempFiles();
        }

        void ClearTempFiles()
        {
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
        }

        // 重新登录
        // parameters:
        // return:
        //      -1  出错
        //      0   成功
        public int ReLogin(out string strError)
        {
#if USE_SESSION_CHANNEL
            // 尽量用已有的设施实现功能
            BeforeLoginEventArgs e = new BeforeLoginEventArgs();
            e.FirstTry = true;
            Channel_BeforeLogin(this, e);

            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            //      >1  有多个账户符合条件。
            long lRet = this.Channel.Login(e.UserName,
                e.Password,
                e.Parameters,
                out strError);
            if (lRet == -1 || lRet == 0)
                return -1;
            return 0;
#else
            LibraryChannel channel = this.GetChannel(true); //  this.GetChannel(true, this.m_strParameters);
            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            //      >1  有多个账户符合条件。
            long lRet = channel.Login(channel.UserName,
                channel.Password,
                this.m_strParameters,
                out strError);
            if (lRet == -1 || lRet == 0)
                return -1;
            _rights = channel.Rights;
            return 0;
#endif
        }

        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        //      >1  有多个账户符合条件。
        public long Login(string strUserName,
            string strPassword,
            string strParameters,
            string strTokenTimeRange,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strUserName) == true)
            {
                strError = "SessionInfo.Login() 的 strUserName 参数不应为空";
                return -1;
            }

            if (String.IsNullOrEmpty(this.ChannelLang) == false)
            {
#if NO
                string strOldLang = "";
                string strError_1 = "";
                this.Channel.SetLang(null,
                    this.ChannelLang,
                    out strOldLang,
                    out strError_1);
#endif
                Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                parameters["lang"] = this.ChannelLang;
                strParameters = StringUtil.BuildParameterString(parameters);
            }

            // gettoken 参数
            if (string.IsNullOrEmpty(strTokenTimeRange) == false)
            {
                Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                parameters["gettoken"] = strTokenTimeRange; //  "month";
                strParameters = StringUtil.BuildParameterString(parameters);
            }

#if USE_SESSION_CHANNEL
                            // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                //      >1  有多个账户符合条件。
                long lRet = this.Channel.Login(strUserName,
                    strPassword,
                    strParameters,
                    out strError);
                if (lRet == 1)
                {
                    if (string.IsNullOrEmpty(this.Channel.UserName) == true)
                        throw new Exception("SessionInfo.Login() this.Channel.UserName 为空 (此时 SessionInfo.m_strUserName 为 '" + this.m_strUserName + "')");

                    this.m_strUserName = this.Channel.UserName; // 2011/7/29
                    this.m_strPassword = strPassword;

                    Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                    // string strLocation = (string)parameters["location"];

                    bool bReader = false;
                    string strType = (string)parameters["type"];
                    if (strType == null)
                        strType = "";
                    if (strType.ToLower() == "reader")
                        bReader = true;

                    // 2014/12/23
                    string strSimulate = (string)parameters["simulate"];
                    if (strSimulate == null)
                        strSimulate = "";
                    if (strSimulate == "yes")
                        this.m_strParameters = strParameters;
                    else
                        this.m_strParameters = "";

                    this.m_bIsReader = bReader;
                }

                return lRet;
#else
            this.m_strUserName = strUserName;   // 为了从 channelpool 中得到先前用过的此用户名的通道
            LibraryChannel channel = this.GetChannel(true);
            try
            {
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                //      >1  有多个账户符合条件。
                long lRet = //this.Channel.
                    channel.Login(strUserName,
                    strPassword,
                    strParameters,
                    out strError);
                if (lRet == 1)
                {
                    if (string.IsNullOrEmpty(//this.Channel.
                        channel.UserName) == true)
                        throw new Exception("SessionInfo.Login() this.Channel.UserName 为空 (此时 SessionInfo.m_strUserName 为 '" + this.m_strUserName + "')");

                    this.m_strUserName = //this.Channel.
                        channel.UserName; // 2011/7/29
                    this.m_strPassword = strPassword;

                    this._libraryCodeList = channel.LibraryCodeList;
                    this._rights = channel.Rights;

                    Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                    // string strLocation = (string)parameters["location"];

                    bool bReader = false;
                    string strType = (string)parameters["type"];
                    if (strType == null)
                        strType = "";
                    if (strType.ToLower() == "reader")
                        bReader = true;

                    // 2014/12/23
                    string strSimulate = (string)parameters["simulate"];
                    if (strSimulate == null)
                        strSimulate = "";
                    if (strSimulate == "yes")
                        this.m_strParameters = strParameters;
                    else
                        this.m_strParameters = "";

                    this.m_bIsReader = bReader;
                }
                else
                {
                    // 2016/1/26
                    this.m_strUserName = "";
                    this.m_strPassword = "";
                    this.m_strParameters = "";
                    this._libraryCodeList = "";
                    this._rights = "";
                }
                return lRet;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
#endif
        }

        /// <summary>
        /// 根据一个馆代码列表字符串，判断这个字符串是否代表了全局用户
        /// </summary>
        /// <param name="strLibraryCodeList">馆代码列表字符串</param>
        /// <returns>是否</returns>
        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            return false;
        }

#if USE_SESSION_CHANNEL
        public bool GlobalUser
        {
            get
            {
                if (this.Channel == null)
                    return false;
                if (string.IsNullOrEmpty(this.UserID) == true)
                    return false;
                return IsGlobalUser(this.Channel.LibraryCodeList);
            }
        }
#else
        string _libraryCodeList = "";   // TODO: 登录成功后存储馆代码列表值

        public string LibraryCodeList
        {
            get
            {
                return _libraryCodeList;
            }
        }

        public bool GlobalUser
        {
            get
            {
                if (string.IsNullOrEmpty(this.UserID) == true)
                    return false;
                return IsGlobalUser(this._libraryCodeList);
            }
        }
#endif

        // readerdom发生变化后，刷新相关域
        public static void RefreshReaderAccount(ref ReaderInfo readerinfo,
            XmlDocument readerdom)
        {
            readerinfo.Barcode = DomUtil.GetElementText(readerdom.DocumentElement,
"barcode");
            readerinfo.Name = DomUtil.GetElementText(readerdom.DocumentElement,
"name");
            readerinfo.DisplayName = DomUtil.GetElementText(readerdom.DocumentElement,
"displayName");

            readerinfo.ReaderDomLastTime = DateTime.Now;
        }

        public void SetLoginReaderDomChanged()
        {
            if (String.IsNullOrEmpty(this.UserID) == true)
            {
                throw new Exception("尚未登录");
            }

            // TODO: 工作人员登录时ReaderInfo也可能有值阿？
            if (this.IsReader == false)
            {
                throw new Exception("sessioninfo.IsReader == false");
            }

            if (this.ReaderInfo != null)
                this.ReaderInfo.ReaderDomChanged = true;
        }

        // 保存修改后的读者记录DOM
        // return:
        //      -2  时间戳冲突
        //      -1  error
        //      0   没有必要保存(changed标志为false)
        //      1   成功保存
        public int SaveLoginReaderDom(
            out string strError)
        {
            strError = "";

            if (this.ReaderInfo == null)
            {
                strError = "尚未装载过ReaderInfo";
                return 0;
            }

            if (string.IsNullOrEmpty(this.UserID) == true)
            {
                strError = "尚未登录";
                return 0;
            }

            if (this.IsReader == false)
            {
                strError = "当前登录的用户不是读者类型";
                return -2;
            }

            if (this.ReaderInfo.ReaderDomChanged == false)
                return 0;

            XmlDocument readerdom = this.ReaderInfo.ReaderDom;

            byte[] output_timestamp = null;
            string strOutputPath = "";
            string strOutputXml = "";

            long lRet = 0;

            string strExistingXml = "";
            ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;

            // 注：保存读者记录本来是为上传透着个性头像，修改 preference 等用途提供的。如果用代理帐户做这个操作，就要求代理帐户具有修改读者记录的权限，同时修改哪些字段就得不到限制了。可以考虑在 dp2library，增加一种功能，在代理帐户修改读者记录的时候，模仿读者权限来进行限制？
            LibraryChannel channel = this.GetChannel(true); // this.GetChannel(true, this.m_strParameters);
            try
            {
                lRet = // this.Channel.
                    channel.SetReaderInfo(
                    null,
                    "change",
                    this.ReaderInfo.ReaderDomPath,
                    readerdom.OuterXml,
                    "", // sessioninfo.Account.ReaderDomOldXml,    // strOldXml
                    this.ReaderInfo.ReaderDomTimestamp,
                    out strExistingXml,
                    out strOutputXml,
                    out strOutputPath,
                    out output_timestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    if (// this.Channel.
                        channel.ErrorCode == ErrorCode.TimestampMismatch
                        || kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                        return -2;
                    return -1;
                }
            }
            finally
            {
                //ReleaseTempChannel(temp);
                this.ReturnChannel(channel);
            }

            int nRet = OpacApplication.LoadToDom(strOutputXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }
            this.ReaderInfo.ReaderDom = readerdom;
            RefreshReaderAccount(ref this.ReaderInfo, readerdom);

            this.ReaderInfo.ReaderDomChanged = false;
            this.ReaderInfo.ReaderDomTimestamp = output_timestamp;
            return 1;
        }

        // 管理员获得特定证条码号的读者记录DOM
        // return:
        //      -2  当前登录的用户不是librarian类型
        //      -1  出错
        //      0   尚未登录
        //      1   成功
        public int GetOtherReaderDom(
            string strReaderBarcode,
            out XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            readerdom = null;

            if (string.IsNullOrEmpty(this.UserID) == true)
            {
                strError = "尚未登录";
                return 0;
            }

            if (this.IsReader == true)
            {
                strError = "当前登录的用户不是工作人员类型";
                return -2;
            }

            if (this.ReaderInfo == null)
                this.ReaderInfo = new ReaderInfo();

            // 看看缓存的readerdom是否失效
            TimeSpan delta = DateTime.Now - this.ReaderInfo.ReaderDomLastTime;
            if (delta.TotalSeconds > 60
                && this.ReaderInfo.ReaderDomChanged == false)
            {
                this.ReaderInfo.ReaderDom = null;
            }

            // 如果语言不同
            if (this.ChannelLang != this.ReaderInfo.Lang)
                this.ReaderInfo.ReaderDom = null;

            if (this.ReaderInfo.ReaderDom == null)
            {
                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;

                string strResultTypeList = "advancexml";
                string[] results = null;
                LibraryChannel channel = this.GetChannel(true); // this.GetChannel(true, this.m_strParameters);
                try
                {
                    long lRet = // this.Channel.
                        channel.GetReaderInfo(null,
                    strReaderBarcode,
                    strResultTypeList,
                    out results,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                    if (lRet == -1) // TODO: 0?
                        goto ERROR1;
                    // 2011/11/22
                    if (lRet == 0)
                    {
                        strError = "证条码号为 '" + strReaderBarcode + "' 的读者记录没有找到...";
                        goto ERROR1;
                    }
                }
                finally
                {
                    //ReleaseTempChannel(temp);
                    this.ReturnChannel(channel);
                }

                if (results.Length != 1)
                {
                    strError = "results.Length error";
                    goto ERROR1;
                }

                strXml = results[0];
                readerdom = new XmlDocument();
                try
                {
                    readerdom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "装载读者XML记录进入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                this.ReaderInfo.ReaderDomPath = strOutputPath;
                this.ReaderInfo.ReaderDomTimestamp = timestamp;
                this.ReaderInfo.ReaderDom = readerdom;
                this.ReaderInfo.ReaderDomLastTime = DateTime.Now;
                this.ReaderInfo.Lang = this.ChannelLang;

                RefreshReaderAccount(ref this.ReaderInfo, readerdom);
            }
            else
            {
                readerdom = this.ReaderInfo.ReaderDom;  // 沿用cache中的
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 获得当前session中已经登录的读者记录DOM
        // return:
        //      -2  当前登录的用户不是reader类型
        //      -1  出错
        //      0   尚未登录
        //      1   成功
        public int GetLoginReaderDom(
            out XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            readerdom = null;

            if (string.IsNullOrEmpty(this.UserID) == true)
            {
                strError = "尚未登录";
                return 0;
            }

            if (this.IsReader == false)
            {
                strError = "当前登录的用户不是读者类型";
                return -2;
            }

            // 2015/11/20
            if (this.UserID.IndexOf(":") != -1)
            {
                strError = "this.UserID '" + this.UserID + "' 中不应包含冒号";
                return -1;
            }

            Debug.Assert(this.UserID.IndexOf(":") == -1, "UserID中不能包含冒号");

            if (this.ReaderInfo == null)
                this.ReaderInfo = new ReaderInfo();

            // 看看缓存的readerdom是否失效
            TimeSpan delta = DateTime.Now - this.ReaderInfo.ReaderDomLastTime;
            if (delta.TotalSeconds > 60
                && this.ReaderInfo.ReaderDomChanged == false)
            {
                this.ReaderInfo.ReaderDom = null;
            }

            // 如果语言不同
            if (this.ChannelLang != this.ReaderInfo.Lang)
                this.ReaderInfo.ReaderDom = null;

            if (this.ReaderInfo.ReaderDom == null)
            {
                string strBarcode = "";

                strBarcode = this.UserID;
                if (strBarcode == "")
                {
                    strError = "帐户信息中读者证条码号为空，无法定位读者记录。";
                    goto ERROR1;
                }

                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;

                string strResultTypeList = "advancexml";
                string[] results = null;

                LibraryChannel channel = this.GetChannel(true); //  this.GetChannel(true, this.m_strParameters);
                try
                {
                    long lRet = //this.Channel.
                        channel.GetReaderInfo(null,
                        strBarcode,
                        strResultTypeList,
                        out results,
                        out strOutputPath,
                        out timestamp,
                        out strError);
                    if (lRet == -1) // TODO: 0?
                        goto ERROR1;
                    // 2011/11/22
                    if (lRet == 0)
                    {
                        strError = "证条码号为 '" + strBarcode + "' 的读者记录没有找到...";
                        goto ERROR1;
                    }
                }
                finally
                {
                    //ReleaseTempChannel(temp);
                    this.ReturnChannel(channel);
                }

                if (results.Length != 1)
                {
                    strError = "results.Length error";
                    goto ERROR1;
                }
                strXml = results[0];
                readerdom = new XmlDocument();
                try
                {
                    readerdom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "装载读者XML记录进入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                this.ReaderInfo.ReaderDomPath = strOutputPath;
                this.ReaderInfo.ReaderDomTimestamp = timestamp;
                this.ReaderInfo.ReaderDom = readerdom;
                this.ReaderInfo.ReaderDomLastTime = DateTime.Now;
                this.ReaderInfo.Lang = this.ChannelLang;

                RefreshReaderAccount(ref this.ReaderInfo, readerdom);
            }
            else
            {
                readerdom = this.ReaderInfo.ReaderDom;  // 沿用cache中的
            }

            return 1;
        ERROR1:
            return -1;
        }

        public void Clear()
        {
            this.ClearLoginReaderDomCache();
            this.ClearTempFiles();
        }

        // 清除当前已经登录的读者类型用户的读者记录DOM cache
        public void ClearLoginReaderDomCache()
        {
            /*
            if (this.IsReader == false)
                return;
             * */
            // 工作人员也可代替读者进行操作,所以工作人员身份也应该可以Clear...Cache

            if (this.ReaderInfo == null)
                return;

            // 内存中内容已经被修改，要先保存DOM到数据库
            if (this.ReaderInfo.ReaderDomChanged == true)
            {
                // 此处的自动保存，和保存按钮矛盾了 -- 一旦刷新，就会自动保存。
                string strError = "";
                // 保存修改后的读者记录DOM
                // return:
                //      -1  error
                //      0   没有必要保存(changed标志为false)
                //      1   成功保存
                int nRet = SaveLoginReaderDom(
                    out strError);
                // 遇到错误，如何报错?
            }

            this.ReaderInfo.ReaderDom = null;
        }

#if NO
        // TODO: 结果集是和 channel 在一起的。如果 channel 不确定，就需要用全局结果集
        // 获得一定范围的检索命中结果
        // return:
        public int GetCommentsSearchResult(
            OpacApplication app,
            int nStart,
            int nMaxCount,
            bool bGetRecord,
            string strLang, // 2012/7/9
            out string strError)
        {
            strError = "";

            List<string> aPath = null;
            long lRet = this.Channel.GetSearchResult(
                null,
                "default",
                nStart, // 0,
                nMaxCount, // -1,
                strLang,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            long lHitCount = lRet;

            if (aPath.Count == 0)
            {
                strError = "GetSearchResult aPath error";
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
                    string strStyle = LibraryChannel.GETRES_ALL_STYLE;

                    lRet = this.Channel.GetRes(
                        null,
                        aPath[i],
                        strStyle,
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
#endif


#if NO
        // 根据特性的评注记录路径，获得一定范围的检索命中结果
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetCommentsSearchResult(
            OpacApplication app,
            int nPerCount,
            string strCommentRecPath,
            bool bGetRecord,
            string strLang, // 2012/7/9
            out int nStart,
            out string strError)
        {
            strError = "";
            nStart = -1;

            long lHitCount = 0;

            bool bFound = false;
            List<string> aPath = null;
            for (int j = 0; ; j++)
            {
                nStart = j * nPerCount;
                // 只获得路径。确保所要的lStart lCount范围全部获得
                long lRet = this.Channel.GetSearchResult(
                    null,
                    "default",
                    nStart, // 0,
                    nPerCount, // -1,
                    strLang,
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
                        string strStyle = LibraryChannel.GETRES_ALL_STYLE;

                        long lRet = this.Channel.GetRes(
                            null,
                            aPath[i],
                            strStyle,
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
            strError = "路径为 '" + strCommentRecPath + "' 的记录在结果集中没有找到";
            return 0;   // 没有找到
        ERROR1:
            return -1;
        }
#endif
    }

    public class ReaderInfo
    {
        // //
        public XmlDocument ReaderDom = null;    // 如果是读者帐户，这里是读者记录DOM
        public string ReaderDomBarcode = "";   // 缓冲的DOM代表的读者证条码号
        public byte[] ReaderDomTimestamp = null;    // 读者记录时间戳
        public string ReaderDomPath = "";   // 读者记录路径
        public DateTime ReaderDomLastTime = new DateTime((long)0);  // 最近装载的时间
        public bool ReaderDomChanged = false;

        public string Name = "";
        public string DisplayName = "";
        public string Barcode = "";

        public string Lang = "";
    }

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

        public LibraryChannel Channel = null;   // 2016/1/26
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

    public class PostedFileInfo
    {
        public string FileName = "";
    }
}
