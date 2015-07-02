using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.IO;
// using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

using DigitalPlatform.CirculationClient.localhost;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// Summary description for SessionInfo.
    /// </summary>
    public class SessionInfo
    {
        public OpacApplication App = null;
        public LibraryChannel Channel = new LibraryChannel();

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

        public string RightsOrigin
        {
            get
            {
                if (this.Channel == null)
                    return "";
                return this.Channel.Rights;
            }
        }

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

        public event ItemLoadEventHandler ItemLoad = null;
        public event SetStartEventHandler SetStart = null;

        public string ClientIP = "";    // 前端的 IP 地址

        public SessionInfo(OpacApplication app)
        {
            Debug.Assert(app != null, "");
            this.App = app;
            this.Channel.Url = app.WsUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this.m_strTempDir = PathUtil.MergePath(app.SessionDir, this.GetHashCode().ToString());

        }

        // 2015/1/22
        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
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

                    e.Parameters = StringUtil.BuildParameterString(parameters, ',', '=');

                    // e.Parameters = "location=#opac_token@" + this.ClientIP + ",index=-1,type=reader,simulate=yes,libraryCode=";
                    e.Password = this.App.ManagerUserName + "," + this.App.ManagerPassword + "|||" + m_strPassword;   // simulate登录的需要
                }
                else
                {
                    e.Parameters = "location=#opac@" + this.ClientIP;
                    if (m_bIsReader == true)
                        e.Parameters += ",type=reader,libraryCode=";    // TODO: 可以用一个参数设定馆代码限制范围
                }
            }

            e.LibraryServerUrl = App.WsUrl;
        }

        // 获得一个通道
        // 分为两种方式，一种是使用自己携带的通道，一种是从通道池中分配
        public LibraryChannel GetChannel(bool bPool, string strParam = "")
        {
            if (bPool == false)
                return this.Channel;

            LibraryChannel channel = this.App.ChannelPool.GetChannel(this.App.WsUrl, this.UserID);
            channel.Password = this.Password;

            // 2015/6/11
            if (string.IsNullOrEmpty(strParam) == true)
            {
                string strParameters = "location=#opac@" + this.ClientIP;
                if (m_bIsReader == true)
                    strParameters += ",type=reader,libraryCode=";    // TODO: 可以用一个参数设定馆代码限制范围
                channel.Param = strParameters;  // Tag
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
                if (this.Channel != null)
                {
                    this.Channel.Close();
                    this.Channel = null;
                }
            }
            catch
            {
            }
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
            CirculationClient.localhost.ErrorCodeValue kernel_errorcode = CirculationClient.localhost.ErrorCodeValue.NoError;

            // 注：保存读者记录本来是为上传透着个性头像，修改 preference 等用途提供的。如果用代理帐户做这个操作，就要求代理帐户具有修改读者记录的权限，同时修改哪些字段就得不到限制了。可以考虑在 dp2library，增加一种功能，在代理帐户修改读者记录的时候，模仿读者权限来进行限制？
                //TempChannel temp = GetTempChannel(this.Password);
                try
                {
                    lRet = this.Channel.SetReaderInfo(
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
                        if (this.Channel.ErrorCode == CirculationClient.localhost.ErrorCode.TimestampMismatch
                            || kernel_errorcode == CirculationClient.localhost.ErrorCodeValue.TimestampMismatch)
                            return -2;
                        return -1;
                    }
                }
                finally
                {
                    //ReleaseTempChannel(temp);
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
            /*
        ERROR1:
            return -1;
             * */
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
                //TempChannel temp = GetTempChannel(this.Password);
                try
                {
                    long lRet = this.Channel.GetReaderInfo(null,
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
                }

                if (results.Length != 1)
                {
                    strError = "results.Length error";
                    goto ERROR1;
                }

                strXml = results[0];
                // timestamp = ByteArray.GetTimeStampByteArray(results[1]);

                /*
                // 获得读者记录
                int nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    strBarcode,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    goto ERROR1;
                 * */

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

#if NO
        class TempChannel
        {
            public SessionInfo Session = null;
            public LibraryChannel Channel = null;
        }

        TempChannel GetTempChannel(string strPassword)
        {
            TempChannel temp = new TempChannel();
            if (StringUtil.HasHead(this.Password, "token:") == true)
            {
                // 临时的SessionInfo对象
                SessionInfo session = new SessionInfo(this.App);
                session.UserID = this.App.ManagerUserName;
                session.Password = this.App.ManagerPassword;
                session.IsReader = false;

                temp.Session = session;
                temp.Channel = session.Channel;
            }
            else
            {
                temp.Channel = this.Channel;
            }
                return temp;
        }

        static void ReleaseTempChannel(TempChannel temp)
        {
            if (temp.Session != null)
                temp.Session.CloseSession();
        }
#endif



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

                //TempChannel temp = GetTempChannel(this.Password);
                try
                {
                    long lRet = this.Channel.GetReaderInfo(null,
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
                }

                if (results.Length != 1)
                {
                    strError = "results.Length error";
                    goto ERROR1;
                }
                strXml = results[0];
                // timestamp = ByteArray.GetTimeStampByteArray(results[1]);

                /*
                // 获得读者记录
                int nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    strBarcode,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    goto ERROR1;
                 * */

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

        // 检索出册数据
        // 带有偏移量的版本
        // 2009/6/9 
        // return:
        //      -2  实体库没有定义
        //      -1  出错
        //      其他  命中的全部结果数量。
        //
        public int OpacSearchItems(
            OpacApplication app,
            string strBiblioRecPath,
            int nStart,
            int nMaxCount,
            string strLang,
            string strLibraryCode,
            out string strError)
        {
            strError = "";
            // string strXml = "";

            string strStyle = "opac";
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                strStyle += ",getotherlibraryitem";
            else
                strStyle += ",librarycode:" + strLibraryCode;

            long lStart = nStart;
            long lCount = nMaxCount;
            long lTotalCount = 0;
            for (; ; )
            {
                EntityInfo[] iteminfos = null;
                long lRet = this.Channel.GetEntities(
                    null,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    out iteminfos,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode == ErrorCode.ItemDbNotDef)
                        return -2;
                    return -1;
                }

                if (lRet == 0)
                {
                    strError = "没有找到";
                    return 0;
                }

                lTotalCount = lRet;

                if (lCount < 0)
                    lCount = lTotalCount - lStart;

                if (lStart + lCount > lTotalCount)
                    lCount = lTotalCount - lStart;

                // 处理
                for(int i=0;i<iteminfos.Length;i++)
                {
                    EntityInfo info = iteminfos[i];

                    if (this.ItemLoad != null)
                    {
                        ItemLoadEventArgs e = new ItemLoadEventArgs();
                        e.Path = info.OldRecPath;
                        e.Index = i;    // +nStart;
                        e.Count = nMaxCount;    // (int)lTotalCount - nStart;
                        e.Timestamp = info.OldTimestamp;
                        e.Xml = info.OldRecord;

                        this.ItemLoad(this, e);
                    }
                }

                lStart += iteminfos.Length;
                lCount -= iteminfos.Length;

                if (lStart >= lTotalCount)
                    break;
                if (lCount <= 0)
                    break;
            }

            return (int)lTotalCount;
        }

        // 检索出评注数据
        // return:
        //      命中的全部结果数量。
        public long SearchComments(
            OpacApplication app,
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            // string strXml = "";

            Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "");

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "从书目记录路径 '"+strBiblioRecPath+"' 中无法获得库名部分";
                return -1;
            }

            string strCommentDbName = "";
            // 根据书目库名, 找到对应的评注库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.App.GetCommentDbName(strBiblioDbName,
                out strCommentDbName,
                out strError);
            if (nRet == -1)
                return -1;

            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strCommentDbName + ":" + "父记录")       // 2007/9/14 
                + "'><item><order>DESC</order><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            long lRet = this.Channel.Search(
                null,
                strQueryXml,
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


        Hashtable FilterTasks = new Hashtable();

        public FilterTask FindFilterTask(string strName)
        {
            lock (this.FilterTasks)
            {
                return (FilterTask)this.FilterTasks[strName];
            }
        }

        public void SetFilterTask(string strName, FilterTask task)
        {
            lock (this.FilterTasks)
            {
                FilterTask old_task = (FilterTask)this.FilterTasks[strName];
                if (old_task == task)
                    return;

                // 删除任务所创建的结果集文件
                if (old_task != null)
                    old_task.DeleteTempFiles(this.GetTempDir());

                if (task == null)
                    this.FilterTasks.Remove(strName);
                else
                    this.FilterTasks[strName] = task;
            }
        }
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

    public enum TaskState
    {
        Processing = 0,
        Done = 1,
    }

    // 一个后台执行的剖析任务
    public class FilterTask
    {
        public List<NodeInfo> ResultItems = null;
        public TaskState TaskState = TaskState.Processing;
        public long ProgressRange = 0;
        public long ProgressValue = 0;
        public long HitCount = 0;   // 原始结果集中的记录总数
        public string ErrorInfo = "";

        // 删除所有结果集文件
        public void DeleteTempFiles(string strTempDir)
        {
            if (this.ResultItems != null)
            {
                foreach (NodeInfo info in this.ResultItems)
                {
                    if (string.IsNullOrEmpty(info.ResultSetPureName) == false)
                    {
                        try
                        {
                            File.Delete(PathUtil.MergePath(strTempDir, info.ResultSetPureName));
                        }
                        catch
                        {
                        }
                    }

                    if (string.IsNullOrEmpty(info.SubNodePureName) == false)
                    {
                        try
                        {
                            File.Delete(PathUtil.MergePath(strTempDir, info.SubNodePureName));
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public void _SetProgress(long lProgressRange, long lProgressValue)
        {
            this.ProgressRange = lProgressRange;
            this.ProgressValue = lProgressValue;
        }

        public void ThreadPoolCallBack(object context)
        {
            FilterTaskInput input = (FilterTaskInput)context;

            Hashtable result_table = null;
            string strError = "";

            // 临时的SessionInfo对象
            SessionInfo session = new SessionInfo(input.App);
            session.UserID = input.App.ManagerUserName;
            session.Password = input.App.ManagerPassword;
            session.IsReader = false;

            try
            {
                long lHitCount = 0;
                int nRet = ResultsetFilter.DoFilter(
                    input.App,
                    session.Channel,
                    input.ResultSetName,
                    input.FilterFileName,
                    input.MaxCount,
                    _SetProgress,
                    ref result_table,
                    out lHitCount,
                    out strError);
                if (nRet == -1)
                {
                    this.ErrorInfo = strError;
                    this.TaskState = TaskState.Done;
                    return;
                }

                this.HitCount = lHitCount;

                /*
                if (string.IsNullOrEmpty(strFacetDefXml) == false)
                {
                    this.DefDom = new XmlDocument();
                    try
                    {
                        this.DefDom.LoadXml(strFacetDefXml);
                    }
                    catch (Exception ex)
                    {
                        this.ErrorInfo = "strDefXml装入XMLDOM出错: " + ex.Message;
                        this.TaskState = TaskState.Done;
                        return;
                    }
                }
                 * */

                // 继续加工
                List<NodeInfo> output_items = null;
                nRet = ResultsetFilter.BuildResultsetFile(result_table,
                    input.DefDom,
                    // input.aggregation_names,
                    input.SessionInfo.GetTempDir(),
                    out output_items,
                    out strError);
                if (nRet == -1)
                {
                    this.ErrorInfo = strError;
                    this.TaskState = TaskState.Done;
                    return;
                }

                {
                    this.ResultItems = output_items;
                    this.TaskState = TaskState.Done;
                }

                if (input.ShareResultSet == true)
                {
                    // 删除全局结果集对象
                    // 管理结果集
                    // parameters:
                    //      strAction   share/remove 分别表示共享为全局结果集对象/删除全局结果集对象
                    long lRet = session.Channel.ManageSearchResult(
                        null,
                        "remove",
                        "",
                        input.ResultSetName,
                        out strError);
                    if (lRet == -1)
                        this.ErrorInfo = strError;

                    input.ShareResultSet = false;
                    input.ResultSetName = "";
                }
            }
            finally
            {
                session.CloseSession();
            }

            if (input.SessionInfo != null && string.IsNullOrEmpty(input.TaskName) == false)
                input.SessionInfo.SetFilterTask(input.TaskName, this);
        }
    }

    public class FilterTaskInput
    {
        public OpacApplication App = null;

        public string ResultSetName = "";   // 全局结果集的名字。第一字符 '#' 被省略了
        public bool ShareResultSet = false;   // 结果集是否被 Share 过。如果是，最后不要忘记 Remove

        public string FilterFileName = "";
        public SessionInfo SessionInfo = null;
        public string TaskName = "";
        // public List<string> aggregation_names = null;   // 那些需要创建二级节点的名称
        public XmlDocument DefDom = null;   // facetdef.xml
        public int MaxCount = 1000;   // 剖析的最多记录数
    }
}
