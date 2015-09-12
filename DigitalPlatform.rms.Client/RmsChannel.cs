using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using System.ServiceModel;


using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.ServiceModel.Security.Tokens;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Range;
using DigitalPlatform.Xml;

using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.rms.Client
{
    public class dp2opacRecord
    {
        public Record Record;
        public long IndexOfResult;
    }

    public enum LoginStyle
    {
        None = 0x0,	// 什么风格也没有
        AutoLogin = 0x1,	// 第一次不出现对话框就先以缺省帐户试验登录一次。
        FillDefaultInfo = 0x2,	// 出现登录对话框时，是否填充缺省帐户名和密码信息。
    }

    public enum ChannelErrorCode
    {
        // 以下错误码为前端独有
        None = 0,
        RequestTimeOut = 1,	// 请求超时
        RequestCanceled = 2,	// 请求被中断
        RequestError = 3,	// 其他通讯错误
        RequestCanceledByEventClose = 4,	// 请求被中断，因为eventClose触发
        QuotaExceeded = 5,  // 超过通讯包尺寸配额
        OtherError = 6,	// 未翻译的错误码


        // 以下错误码和服务器对应
        TimestampMismatch = 10,	// 时间戳不匹配
        NotFound = 11,	// 未命中
        NotLogin = 12,	// 尚未登录
        EmptyRecord = 13,	// 空记录
        //NoHasManagement = 14,	// 不具备管理员权限
        NotHasEnoughRights = 15, // 没有足够的权限
        PartNotFound = 16,	// 记录局部没有找到
        AlreadyExist = 17,	// 要创建的同名对象已经存在
        AlreadyExistOtherType = 18,	// 要创建的同名对象已经存在，但是为不同类型

        ApplicationStartError = 24,	//Application启动错误

        NotFoundSubRes = 25,    // 部分下级资源记录不存在

        // LoginFail = 26, // dp2library向dp2Kernel登录失败。这意味着library.xml中的代理帐户有问题

    }


    // 一个通讯通道
    public class RmsChannel
    {
        /// <summary>
        /// RecieveTimeout
        /// </summary>
        public TimeSpan RecieveTimeout = new TimeSpan(0, 1, 0); // 40

        /// <summary>
        /// SendTimeout
        /// </summary>
        public TimeSpan SendTimeout = new TimeSpan(0, 1, 0);

        /// <summary>
        /// CloseTimeout
        /// </summary>
        public TimeSpan CloseTimeout = new TimeSpan(0, 0, 30);

        /// <summary>
        /// OpenTimeout
        /// </summary>
        public TimeSpan OpenTimeout = new TimeSpan(0, 1, 0);

        /// <summary>
        /// InactivityTimeout
        /// </summary>
        public TimeSpan InactivityTimeout = new TimeSpan(0, 20, 0);

        /// <summary>
        /// OperationTimeout
        /// </summary>
        public TimeSpan OperationTimeout = new TimeSpan(0, 40, 0);

        /// <summary>
        /// 获得或设置超时时间。相当于通道 的 OperationTimeout
        /// </summary>
        public TimeSpan Timeout
        {
            get
            {
                if (this.m_ws == null)
                    return this.OperationTimeout;

                return this.m_ws.InnerChannel.OperationTimeout;
            }
            set
            {
                if (this.m_ws == null)
                    this.OperationTimeout = value;
                else
                {
                    this.m_ws.InnerChannel.OperationTimeout = this.OperationTimeout;
                    this.OperationTimeout = value;
                }
            }
        }
        


        public const int MAX_RECEIVE_MESSAGE_SIZE = 2 * 1024 * 1024;
        public RmsChannelCollection Container = null;
        public string Url = "";

        KernelServiceClient m_ws = null;	// 拥有

        bool m_bStoped = false; // 检索是否被中断过一次
        int m_nInSearching = 0;
        int m_nRedoCount = 0;   // MessageSecurityException以后重试的次数
        // public AutoResetEvent eventFinish = new AutoResetEvent(false);

        public ChannelErrorCode ErrorCode = ChannelErrorCode.None;

        public ErrorCodeValue OriginErrorCode = ErrorCodeValue.NoError;

        public string ErrorInfo = "";

        // IAsyncResult soapresult = null;

        public event IdleEventHandler Idle = null;
        public object Param = null;

        // [NonSerialized]
        public CookieContainer Cookies = new System.Net.CookieContainer();

        static void SetTimeout(System.ServiceModel.Channels.Binding binding)
        {
            binding.SendTimeout = new TimeSpan(0, 20, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 20, 0);
            binding.CloseTimeout = new TimeSpan(0, 20, 0);
            binding.OpenTimeout = new TimeSpan(0, 20, 0);
        }

        // np0: namedpipe
        public static System.ServiceModel.Channels.Binding CreateNp0Binding()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            binding.MaxReceivedMessageSize = MAX_RECEIVE_MESSAGE_SIZE;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;

            SetTimeout(binding);
            //binding.ReliableSession.Enabled = true;

            return binding;
        }

        // nt0: net.tcp
        public static System.ServiceModel.Channels.Binding CreateNt0Binding()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            binding.MaxReceivedMessageSize = MAX_RECEIVE_MESSAGE_SIZE;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            //binding.ReliableSession.Enabled = true;

            return binding;
        }

        // ws0:windows
        public static System.ServiceModel.Channels.Binding CreateWs0Binding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;

            binding.MaxReceivedMessageSize = MAX_RECEIVE_MESSAGE_SIZE;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            //binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        // ws1:anonymouse
        public static System.ServiceModel.Channels.Binding CreateWs1Binding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = MAX_RECEIVE_MESSAGE_SIZE;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            //binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            // return binding;

#if NO
            //Clients are anonymous to the service.
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            //Secure conversation is turned off for simplification. If secure conversation is turned on, then 
            //you also need to set the IdentityVerifier on the secureconversation bootstrap binding.
            // binding.Security.Message.EstablishSecurityContext = false;

            // Get the SecurityBindingElement and cast to a SymmetricSecurityBindingElement to set the IdentityVerifier.
            BindingElementCollection outputBec = binding.CreateBindingElements();
            SymmetricSecurityBindingElement ssbe = (SymmetricSecurityBindingElement)outputBec.Find<SecurityBindingElement>();

            //Set the Custom IdentityVerifier.
            ssbe.LocalClientSettings.IdentityVerifier = new CustomIdentityVerifier();

            return new CustomBinding(outputBec);
#endif
            // Get the SecurityBindingElement and cast to a SymmetricSecurityBindingElement to set the IdentityVerifier.
            BindingElementCollection outputBec = binding.CreateBindingElements();
            SymmetricSecurityBindingElement ssbe = (SymmetricSecurityBindingElement)outputBec.Find<SecurityBindingElement>();

            //Set the Custom IdentityVerifier.
            ssbe.LocalClientSettings.IdentityVerifier = new CustomIdentityVerifier();

            //
            // Get the System.ServiceModel.Security.Tokens.SecureConversationSecurityTokenParameters 
            SecureConversationSecurityTokenParameters secureTokenParams =
                (SecureConversationSecurityTokenParameters)ssbe.ProtectionTokenParameters;
            // From the collection, get the bootstrap element.
            SecurityBindingElement bootstrap = secureTokenParams.BootstrapSecurityBindingElement;
            // Set the MaxClockSkew on the bootstrap element.
            bootstrap.LocalClientSettings.IdentityVerifier = new CustomIdentityVerifier();

            return new CustomBinding(outputBec);
        }

        // ws2:username
        public static System.ServiceModel.Channels.Binding CreateWs2Binding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            // binding.Security.Message.NegotiateServiceCredential = false;
            // binding.Security.Message.EstablishSecurityContext = false;

            binding.MaxReceivedMessageSize = MAX_RECEIVE_MESSAGE_SIZE;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            //binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        public KernelServiceClient ws
        {
            get
            {
                if (m_ws == null)
                {
                    bool bWs0 = false;
                    Uri uri = new Uri(this.Url);

                    if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        EndpointAddress address = new EndpointAddress(this.Url);

                        this.m_ws = new KernelServiceClient(CreateNp0Binding(), address);
                    }
                    else if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        EndpointAddress address = new EndpointAddress(this.Url);

                        this.m_ws = new KernelServiceClient(CreateNt0Binding(), address);
                    }
                    else
                    {
                        if (uri.AbsolutePath.ToLower().IndexOf("/ws0") != -1)
                            bWs0 = true;

                        if (bWs0 == false)
                        {
                            // ws1 
                            /*
                            EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("DigitalPlatform");
                            EndpointAddress address = new EndpointAddress(new Uri(this.Url),
                                identity, new AddressHeaderCollection());
                             * */
                            EndpointAddress address = null;
                            address = new EndpointAddress(this.Url);

                            this.m_ws = new KernelServiceClient(CreateWs1Binding(), address);

                            // this.m_ws.ClientCredentials.ClientCertificate.SetCertificate(
                            this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
                            this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CustomCertificateValidator =
            new MyValidator();
                        }
                        else
                        {
                            // ws0
                            EndpointAddress address = new EndpointAddress(this.Url);

                            this.m_ws = new KernelServiceClient(CreateWs0Binding(), address);
                            this.m_ws.ClientCredentials.UserName.UserName = "test";
                            this.m_ws.ClientCredentials.UserName.Password = "";
                        }
                    }

                }
                if (String.IsNullOrEmpty(this.Url) == true)
                {
                    throw(new Exception("Url值此时应当不等于空"));
                }
                Debug.Assert(this.Url != "", "Url值此时应当不等于空");

                // m_ws.Url = this.Url;
                // m_ws.CookieContainer = this.Cookies;

                // this.m_ws.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(20);
                this.m_ws.InnerChannel.OperationTimeout = this.OperationTimeout;

                return m_ws;
            }
        }

        void BeginSearch()
        {
            m_bStoped = false;
            m_nInSearching++;
        }

        void EndSearch()
        {
            m_nInSearching--;
        }

        /*
        public int Timeout
        {
            get
            {
                return ws.Timeout;
            }
            set
            {
                ws.Timeout = value;
            }
        }
         * */

        public void Abort()
        {
            if (m_nInSearching > 0)
            {
                if (this.m_ws != null)
                {
                    if (this.m_bStoped == false)
                    {
                        this.DoStop();
                        // TODO: 如果时间太长了不返回，则调用Abort()?
                        this.m_bStoped = true;
                        return;
                    }

                    // 否则，就走到Abort()那里
                }
            }

            if (this.m_ws != null)
                ws.Abort();

            // ws.servicepoint.CloseConnectionGroup(ws.ConnectionGroupName);
            /*
            if (soapresult != null)
                ((WebClientAsyncResult)soapresult).Abort();
            else
                ws.Abort();
             * */

            // 2011/1/7 add
            this.m_ws = null;
        }

#if NO
        public void Close()
        {
            if (this.m_ws != null)
            {
                this.m_ws.Close();
                this.m_ws = null;
            }
        }
#endif
        // 2015/5/4
        public void Close()
        {
            if (this.m_ws != null)
            {
                // TODO: Search()要单独处理
                try
                {
                    if (this.m_ws.State != CommunicationState.Faulted)
                        this.m_ws.Close();
                }
                catch
                {
                    this.m_ws.Abort();
                }
                this.m_ws = null;
            }
        }

        void ConvertErrorCode(Result result)
        {
            this.ClearRedoCount();

            this.OriginErrorCode = result.ErrorCode;

            if (result.ErrorCode == ErrorCodeValue.NoError)
            {
                this.ErrorCode = ChannelErrorCode.None; // 2008/7/29
            }
            else if (result.ErrorCode == ErrorCodeValue.NotFound)
            {
                this.ErrorCode = ChannelErrorCode.NotFound;
            }
            else if (result.ErrorCode == ErrorCodeValue.PartNotFound)
            {
                this.ErrorCode = ChannelErrorCode.PartNotFound;
            }
            else if (result.ErrorCode == ErrorCodeValue.EmptyContent)
            {
                this.ErrorCode = ChannelErrorCode.EmptyRecord;
            }
            else if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
            {
                this.ErrorCode = ChannelErrorCode.TimestampMismatch;
            }
            else if (result.ErrorCode == ErrorCodeValue.NotLogin)
            {
                this.ErrorCode = ChannelErrorCode.NotLogin;
            }
            /*
                        else if (result.ErrorCode == ErrorCodeValue.NoHasManagement)
                        {
                            this.ErrorCode = ChannelErrorCode.NoHasManagement;
                        }
            */
            else if (result.ErrorCode == ErrorCodeValue.NotHasEnoughRights)
            {
                this.ErrorCode = ChannelErrorCode.NotHasEnoughRights;
            }
            else if (result.ErrorCode == ErrorCodeValue.AlreadyExist)
            {
                this.ErrorCode = ChannelErrorCode.AlreadyExist;
            }
            else if (result.ErrorCode == ErrorCodeValue.AlreadyExistOtherType)
            {
                this.ErrorCode = ChannelErrorCode.AlreadyExistOtherType;
            }
            else if (result.ErrorCode == ErrorCodeValue.ApplicationStartError)
            {
                this.ErrorCode = ChannelErrorCode.ApplicationStartError;
            }
            else if (result.ErrorCode == ErrorCodeValue.NotFoundSubRes)
            {
                this.ErrorCode = ChannelErrorCode.NotFoundSubRes;
            }
            else if (result.ErrorCode == ErrorCodeValue.Canceled)
            {
                this.ErrorCode = ChannelErrorCode.RequestCanceled;
            }
            else
            {
                this.ErrorCode = ChannelErrorCode.OtherError;
            }
        }

        // return:
        //      0   主流程需返回-1
        //      1   需要重做API
        int ConvertWebError(Exception ex0,
            out string strError)
        {
            // 服务器重启后
            if (ex0 is System.ServiceModel.Security.MessageSecurityException)
            {
                System.ServiceModel.Security.MessageSecurityException ex = (System.ServiceModel.Security.MessageSecurityException)ex0;
                this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
                this.ErrorInfo = GetExceptionMessage(ex);
                this.m_ws = null;
                strError = this.ErrorInfo;
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            if (ex0 is CommunicationObjectFaultedException)
            {
                CommunicationObjectFaultedException ex = (CommunicationObjectFaultedException)ex0;
                this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
                this.ErrorInfo = GetExceptionMessage(ex);
                this.m_ws = null;
                strError = this.ErrorInfo;
                // 2011/7/2
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            if (ex0 is EndpointNotFoundException)
            {
                EndpointNotFoundException ex = (EndpointNotFoundException)ex0;
                this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
                this.ErrorInfo = "服务器 " + this.Url + " 没有响应";
                this.m_ws = null;
                strError = this.ErrorInfo;
                return 0;
            }

            /*
            if (ex0 is CommunicationException)
            {
                CommunicationException ex = (CommunicationException)ex0;

            }
             * */

            if (ex0 is WebException)
            {
                WebException ex = (WebException)ex0;

                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    this.ErrorCode = ChannelErrorCode.RequestTimeOut;
                    this.ErrorInfo = "请求超时";    // (当前超时设置为" + Convert.ToString(this.Timeout) + ")";
                    strError = this.ErrorInfo;
                    return 0;
                }
                if (ex.Status == WebExceptionStatus.RequestCanceled)
                {
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    this.ErrorInfo = "用户中断";
                    strError = this.ErrorInfo;
                    return 0;
                }

                this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
                this.ErrorInfo = GetExceptionMessage(ex);
                strError = this.ErrorInfo;
                return 0;
            }

            // 2013/1/11
            if (ex0 is System.ServiceModel.CommunicationException
                && ex0.InnerException is System.ServiceModel.QuotaExceededException)
            {
                this.ErrorCode = ChannelErrorCode.QuotaExceeded;
                this.ErrorInfo = GetExceptionMessage(ex0);
                strError = this.ErrorInfo;
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
            this.ErrorInfo = GetExceptionMessage(ex0);
            this.m_ws = null;   // 不知是否正确
            strError = this.ErrorInfo;
            return 0;
        }

        static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while(ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        void DoIdle()
        {
            bool bDoEvents = true;
            // 2012/3/18
            // 2012/11/28
            if (this.Container != null
                && this.Container.GUI == false)
                bDoEvents = false;

            // System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            if (this.Idle != null)
            {
                IdleEventArgs e = new IdleEventArgs();
                this.Idle(this, e);
                bDoEvents = e.bDoEvents;
            }

            if (bDoEvents == true)
            {
                try
                {
                    Application.DoEvents();	// 出让界面控制权
                }
                catch
                {
                }
            }

            // System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费
        }

        // 登录。如果必要，出现对话框
        // parameters:
        //		strPath	资源路径。不包含URL部分。
        //		bAutoLogin	是否不出现对话框先自动登录一次。
        // return:
        //		-1	error
        //		0	login failed，出错信息在strError中
        //		1	login succeed
        public int UiLogin(
            string strPath,
            out string strError)
        {
            return UiLogin(null,
                strPath,
                LoginStyle.AutoLogin | LoginStyle.FillDefaultInfo,
                out strError);
        }

        // 登录。如果必要，出现对话框
        // parameters:
        //		strPath	资源路径。不包含URL部分。
        //		bAutoLogin	是否不出现对话框先自动登录一次。
        // return:
        //		-1	error
        //		0	login failed，出错信息在strError中
        //		1	login succeed
        public int UiLogin(
            string strComment,
            string strPath,
            LoginStyle loginStyle,
            out string strError)
        {
            strError = "";

            /*
            if (this.Container.AskAccountInfo == null)
            {
                strError = "AskAccountInfo事件函数未设置";
                return -1;
            }
             */

            string strUserName;
            string strPassword;
            IWin32Window owner = null;

        REDOINPUT:

            // 获得缺省帐户信息
            // return:
            //		2	already login succeed
            //		1	dialog return OK
            //		0	dialog return Cancel
            //		-1	other error
            /*
            int nRet = this.Container.procAskAccountInfo(
                this.Container,
                strComment,
                this.Url,
                strPath,
                loginStyle,
                out owner,
                out strUserName,
                out strPassword);
             */
            AskAccountInfoEventArgs e = new AskAccountInfoEventArgs();
            e.Channels = this.Container;
            e.Comment = strComment;
            e.Url = this.Url;
            e.Path = strPath;
            e.LoginStyle = loginStyle;
            e.Channel = this;   // 2013/2/14

            this.Container.OnAskAccountInfo(this, e);

            owner = e.Owner;
            strUserName = e.UserName;
            strPassword = e.Password;

            if (e.Result == -1)
            {
                if (e.ErrorInfo == "")
                    strError = "procAskAccountInfo() error";
                else
                    strError = e.ErrorInfo;
                return -1;
            }


            if (e.Result == 2)
                return 1;

            if (e.Result == 1)
            {
                // 登录
                // return:
                //		-1	出错。错误信息在strError中
                //		0	登录失败。错误信息也在strError
                //		1	登录成功
                int nRet = this.Login(strUserName,
                    strPassword,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    if (this.Container.GUI == true)
                        MessageBox.Show(owner, strError);
                    else
                    {
                        return -1;
                    }
                    goto REDOINPUT;
                }
                // this.m_nRedoCount = 0;
                return 1;   // 登录成功,可以重做API功能了
            }

            if (e.Result == 0)
            {
                strError = "放弃登录";
                return -1;
            }

            strError = "UiLogin() 不应该走到这里";
            return -1;
        }


        // 登录(废弃)
        // return:
        //		-1	出错。错误信息在strError中
        //		0	登录失败。错误信息也在strError
        //		1	登录成功
        public int LoginOld(string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            REDO:
            Result result = null;
            try
            {
                result = this.ws.Login(strUserName, strPassword);
            }
            catch (Exception ex)
            {
                /*
                this.ErrorCode = ChannelErrorCode.RequestError;	// 一般错误
                this.ErrorInfo = ex.Message;
                strError = this.ErrorInfo;
                */
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

            if (result.Value == -1)
            {
                ConvertErrorCode(result);
                strError = result.ErrorString;
                return -1;
            }

            this.ClearRedoCount();

            if (result.Value == 0)
            {
                strError = result.ErrorString;
                return 0;
            }

            return 1;
        }

        // 获得dpKernel版本号
        // return:
        //		-1	出错。错误信息在strError中
        //		0	成功
        public int GetVersion(out string strVersion,
            out string strError)
        {
            strError = "";
            strVersion = "";

        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetVersion(
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                    /*
                    if (soapresult.IsCompleted)
                        break;
                     * */
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }

                Result result = this.ws.EndGetVersion(soapresult);

                if (result.Value == -1)
                {
                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                if (result.Value == 0)
                {
                    strVersion = result.ErrorString;
                    return 0;
                }

                this.ClearRedoCount();
                return 1;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 登录
        // return:
        //		-1	出错。错误信息在strError中
        //		0	登录失败。错误信息也在strError
        //		1	登录成功
        public int Login(string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginLogin(strUserName,
                    strPassword,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }

                Result result = this.ws.EndLogin(soapresult);

                if (result.Value == -1)
                {
                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                if (result.Value == 0)
                {
                    strError = result.ErrorString;
                    return 0;
                }

                this.ClearRedoCount();
                return 1;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 修改密码
        // 参数strUserName和 strOldPassword可以为null。这种情况下，
        // 本函数直接去修改密码，如果此时channel确实已经登录过，就正好不需要
        // 其他前提条件；如果此时channel尚未登录过，函数Login()的固有机制
        // 可能会弹出登录对话框。
        // parameters:
        //		bManager	是否以管理员身份进行修改。管理员方式的特点，是
        //				1) 必须用管理员帐户先登录(这个帐户和即将被修改的帐户可以没有联系)
        //				2) 用特殊的WebServiceAPI ChangeOtherPassword()
        // return:
        //		-1	出错。错误信息在strError中
        //		0	成功。
        public int ChangePassword(
            string strUserName,
            string strOldPassword,
            string strNewPassword,
            bool bManager,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            if (bManager == true)
            {
                if (strUserName == null)
                {
                    strError = "bManager参数为true时，strUserName参数不能为null...";
                    return -1;
                }
            }


            if (strUserName != null && strOldPassword != null
                && bManager == false)
            {
                // return:
                //		-1	出错。错误信息在strError中
                //		0	登录失败。错误信息也在strError
                //		1	登录成功
                nRet = this.Login(strUserName,
                    strOldPassword,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "原密码不正确";
                    return -1;
                }
            }


            REDO:
            try
            {
                IAsyncResult soapresult = null;

                if (bManager == false)
                {
                    soapresult = this.ws.BeginChangePassword(
                        strNewPassword,
                        null,
                        null);
                }
                else
                {
                    soapresult = this.ws.BeginChangeOtherPassword(
                        strUserName,
                        strNewPassword,
                        null,
                        null);
                }

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = null;

                if (bManager == false)
                {
                    result = this.ws.EndChangePassword(soapresult);
                }
                else
                {
                    result = this.ws.EndChangeOtherPassword(soapresult);
                }

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        nRet = this.UiLogin(
                            strUserName == null ?
                            ("密码修改操作被延迟。请先用旧密码 对 拟修改密码的帐户进行一次登录，以便修改密码操作自动继续进行...")
                            : ("请先用旧密码对帐户 '" + strUserName + "' 进行一次登录，以便修改密码操作顺利进行...")
                            ,
                            "",
                            LoginStyle.None,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.ErrorCode == ErrorCodeValue.NotHasEnoughRights)//ErrorCodeValue.NoHasManagement) 
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        nRet = this.UiLogin(
                            "请先用具备管理员权限的帐户登录，才能修改帐户 '" + strUserName + "' 密码...",
                            "",
                            LoginStyle.None,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    /*
                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                     * */
                    // 原来在这里，稍晚
                    return -1;
                }

                this.ClearRedoCount();
                return 0;

            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        void ClearRedoCount()
        {
            this.m_nRedoCount = 0;
        }

        // 初始化数据库
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口InitializeDb的返回值)
        public long DoInitialDB(string strDBName,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
            REDOINITIAL:
                IAsyncResult soapresult = this.ws.BeginInitializeDb(strDBName, null, null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndInitializeDb(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strDBName,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOINITIAL;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }


        // 2008/11/14
        // 刷新数据库定义
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口InitializeDb的返回值)
        public long DoRefreshDB(
            string strAction,
            string strDBName,
            bool bClearAllKeyTables,
            out string strError)
        {
            strError = "";
            /*
            int nOldTimeout = this.Timeout;
            this.Timeout = 20 * 60 * 1000;  // 加大超时时间
             * */
            REDO:
            try
            {
            REDO_REFRESH:
                IAsyncResult soapresult = this.ws.BeginRefreshDb(
                    strAction,
                    strDBName,
                    bClearAllKeyTables,
                    null,
                    null);
                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndRefreshDb(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strDBName,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO_REFRESH;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                // this.Timeout = nOldTimeout;
            }

        }

        // 删除数据库
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口DeleteDb的返回值)
        public long DoDeleteDB(string strDBName,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
            REDOINITIAL:
                IAsyncResult soapresult = this.ws.BeginDeleteDb(strDBName, null, null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndDeleteDb(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strDBName,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOINITIAL;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

        }

        // 创建数据库
        // parameters:
        //		logicNames	逻辑库名。ArrayList。每个元素为一个string[2]类型。其中第一个字符串为名字，第二个为语言代码
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口CreateDb的返回值)
        public long DoCreateDB(
            List<string[]> logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDef,
            string strBrowseDef,
            out string strError)
        {
            strError = "";

            LogicNameItem[] logicnames = new LogicNameItem[logicNames.Count];
            for (int i = 0; i < logicnames.Length; i++)
            {
                logicnames[i] = new LogicNameItem();
                string[] cols = (string[])logicNames[i];
                logicnames[i].Lang = cols[1];
                logicnames[i].Value = cols[0];
            }

        REDO:
            try
            {
            REDOCREATE:
                IAsyncResult soapresult = this.ws.BeginCreateDb(logicnames,
                    strType,
                    strSqlDbName,
                    strKeysDef,
                    strBrowseDef,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndCreateDb(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOCREATE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }



        }


        // 获得数据库信息
        // parameters:
        //		logicNames	逻辑库名。ArrayList。每个元素为一个string[2]类型。其中第一个字符串为名字，第二个为语言代码
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口CreateDb的返回值)
        public long DoGetDBInfo(
            string strDbName,
            string strStyle,
            out List<string[]> logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysDef,
            out string strBrowseDef,
            out string strError)
        {
            strError = "";
            logicNames = new List<string[]>();
            strType = "";
            strSqlDbName = "";
            strKeysDef = "";
            strBrowseDef = "";

            LogicNameItem[] logicnames = null;

        REDO:
            try
            {
            REDOCREATE:
                IAsyncResult soapresult = this.ws.BeginGetDbInfo(strDbName,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndGetDbInfo(
                    out logicnames,
                    out strType,
                    out strSqlDbName,
                    out strKeysDef,
                    out strBrowseDef,
                    soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOCREATE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }



                for (int i = 0; i < logicnames.Length; i++)
                {
                    string[] cols = new string[2];
                    cols[1] = logicnames[i].Lang;
                    cols[0] = logicnames[i].Value;
                    logicNames.Add(cols);
                }

                this.ClearRedoCount();
                return result.Value;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 修改数据库信息
        // parameters:
        //		logicNames	逻辑库名。ArrayList。每个元素为一个string[2]类型。其中第一个字符串为名字，第二个为语言代码
        // return:
        //		-1	出错
        //		0	成功(基于WebService接口CreateDb的返回值)
        public long DoSetDBInfo(
            string strOldDbName,
            List<string[]> logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDef,
            string strBrowseDef,
            out string strError)
        {
            strError = "";

            LogicNameItem[] logicnames = new LogicNameItem[logicNames.Count];
            for (int i = 0; i < logicnames.Length; i++)
            {
                logicnames[i] = new LogicNameItem();
                string[] cols = (string[])logicNames[i];
                logicnames[i].Lang = cols[1];
                logicnames[i].Value = cols[0];
            }

        REDO:
            try
            {
            REDOCREATE:
                IAsyncResult soapresult = this.ws.BeginSetDbInfo(strOldDbName,
                    logicnames,
                    strType,
                    strSqlDbName,
                    strKeysDef,
                    strBrowseDef,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndSetDbInfo(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOCREATE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }


        }


        // 登出
        // return:
        //		-1	出错
        //		0	成功
        public long DoLogout(out string strError)
        {
            strError = "";

        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginLogout(null, null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndLogout(soapresult);

                if (result.Value == -1)
                {
                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 包装后的版本
        public int DoCopyRecord(string strOriginRecordPath,
    string strTargetRecordPath,
    bool bDeleteOriginRecord,
    out byte[] baOutputTimeStamp,
    out string strOutputPath,
    out string strError)
        {
            string strIdChangeList = "";
            return DoCopyRecord(strOriginRecordPath,
                strTargetRecordPath,
                bDeleteOriginRecord,
                "",
                out strIdChangeList,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
        }

        // 复制记录
        // return:
        //		-1	出错。错误信息在strError中
        //		0或者其他		成功
        public int DoCopyRecord(string strOriginRecordPath,
            string strTargetRecordPath,
            bool bDeleteOriginRecord,
            string strMergeStyle,
            out string strIdChangeList,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {
            strIdChangeList = "";
            baOutputTimeStamp = null;
            strOutputPath = "";
            strError = "";

        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginCopyRecord(strOriginRecordPath,
                    strTargetRecordPath,
                    bDeleteOriginRecord,
                    strMergeStyle,
                    null,
                    null);

                for (; ; )
                {

                    /*
                    try 
                    {
                        Application.DoEvents();	// 出让界面控制权
                    }
                    catch
                    {
                    }
					

                    // System.Threading.Thread.Sleep(10);	// 避免CPU资源过度耗费
                     */
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndCopyRecord(
                    out strIdChangeList,
                    out strOutputPath,
                    out baOutputTimeStamp, soapresult);

                if (result.Value == -1)
                {
                    // 2011/4/21
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();

                if (result.Value == 0)
                {
                    strError = result.ErrorString;
                    return 0;
                }

                return 0;
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

        }

        // 批处理任务
        // return:
        //		-1	出错。错误信息在strError中
        //		0或者其他		成功
        public int DoBatchTask(string strName,
            string strAction,
            TaskInfo info,
            out TaskInfo [] results,
            out string strError)
        {
            results = null;
            strError = "";

        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginBatchTask(strName,
                    strAction,
                    info,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndBatchTask(
                    out results,
                    soapresult);

                if (result.Value == -1)
                {
                    // 2011/4/21
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();

                if (result.Value == 0)
                {
                    strError = result.ErrorString;
                    return 0;
                }

                return 0;

            }
            catch (Exception ex)
            {

                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 检索
        // 包装后的版本
        public long DoSearch(string strQueryXml,
            string strResultSetName,
            out string strError)
        {
            return this.DoSearch(strQueryXml,
                strResultSetName,
                "",
                out strError);
        }

#if NO
        object resultParam = null;
        AutoResetEvent eventComplete = new AutoResetEvent(false);

        // 检索
        // return:
        //		-1	error
        //		0	not found
        //		>=1	命中记录条数
        public long DoSearch(string strQueryXml,
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            try
            {
            REDOSEARCH:
                ws.SearchCompleted += new SearchCompletedEventHandler(ws_SearchCompleted);

                try
                {

                    this.eventComplete.Reset();
                    ws.SearchAsync(strQueryXml,
                        strResultSetName,
                        strOutputStyle);

                    while (true)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        bool bRet = this.eventComplete.WaitOne(10, true);
                        if (bRet == true)
                            break;
                    }

                }
                finally
                {
                    ws.SearchCompleted -= new SearchCompletedEventHandler(ws_SearchCompleted);
                }

                SearchCompletedEventArgs e = (SearchCompletedEventArgs)this.resultParam;

                if (e.Error != null)
                {
                    strError = e.Error.Message;
                    return -1;
                }

                /*
                if (e.Cancelled == true)
                    strError = "用户中断2";
                else
                    strError = e.Result.ErrorString;
                 * */

                Result result = e.Result;

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSEARCH;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;

            }
            catch (Exception ex)
            {
                strError = ConvertWebError(ex);
                return -1;
            }
            finally
            {
                soapresult = null;
            }
        }

        void ws_SearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            this.resultParam = e;
            this.eventComplete.Set();
        }
#endif

        // ( 扩展功能后的)检索
        // parameters:
        //		strQuery	XML检索式
        //      strResultSetName    结果集名
        //      strSearchStyle  检索风格
        //      lRecordCount    希望获得的记录数量。-1表示尽可能多。如果为0，表示不想获得任何记录
        //                      总是从偏移量0开始获得记录
        //      strRecordStyle  获得记录的风格。以逗号分隔，id表示取id,cols表示取浏览格式
        //                      xml timestamp metadata 分别表示要获取的记录体的XML字符串、时间戳、元数据
        // return:
        //		-1	error
        //		0	not found
        //		>=1	命中记录条数
        public long DoSearchEx(string strQueryXml,
            string strResultSetName,
            string strSearchStyle,
            long lRecordCount,
            string strLang,
            string strRecordStyle,
            out Record[] records,
            out string strError)
        {
            strError = "";
            records = null;

        REDO:
            this.BeginSearch();
            try
            {
            REDOSEARCH:
                IAsyncResult soapresult = this.ws.BeginSearchEx(
                    strQueryXml,
                    strResultSetName,
                    strSearchStyle,
                    lRecordCount,
                    strLang,
                    strRecordStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }

                Result result = this.ws.EndSearchEx(
                    out records,
                    soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSEARCH;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;

            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                this.EndSearch();
            }
        }

        // 检索
        // return:
        //		-1	error
        //		0	not found
        //		>=1	命中记录条数
        public long DoSearch(string strQueryXml,
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
            REDOSEARCH:
                IAsyncResult soapresult = this.ws.BeginSearch(
                    strQueryXml,
                    strResultSetName,
                    strOutputStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }

                Result result = this.ws.EndSearch(soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSEARCH;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;

            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                this.EndSearch();
            }
        }


        // 检索
        // return:
        //		-1	error
        //		0	not found
        //		>=1	命中记录条数
        public long DoSearchWithoutLoginDlg(
            string strQueryXml,	
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
                //REDOSEARCH:
                IAsyncResult soapresult = this.ws.BeginSearch(
                    strQueryXml,
                    strResultSetName,
                    strOutputStyle,
                    null, null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndSearch(soapresult);
                if (result.Value == -1)
                {
                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }
                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                this.EndSearch();
            }
        }

        // 2009/11/6
        // 根据指定的记录路径获得浏览格式记录
        // 浅包装版本
        // parameter:
        public long GetBrowseRecords(string[] paths,
            string strStyle,
            out Record[] searchresults,
            out string strError)
        {
            strError = "";
            searchresults = null;

                REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetBrowse(
                    paths,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Record[] records = null;
                Result result = this.ws.EndGetBrowse(
                    out records,soapresult);

                if (result.Value == -1)
                {
                    // 2011/4/21
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }
                else
                {
                    if (records == null)
                        throw new Exception("WebService GetBrowse() API record参数返回值不应为null");

                    //lTotalCount = result.Value;
                }

                // 将结果移出
                searchresults = new Record[records.Length]; // SearchResult
                for (int i = 0; i < records.Length; i++)
                {
                    searchresults[i] = records[i];
                    /*
                    SearchResult searchresult = new SearchResult();
                    searchresults[i] = searchresult;

                    Record record = records[i];

                    searchresult.Path = record.ID;
                    searchresult.Keys = record.Keys;
                    searchresult.Cols = record.Cols;
                     * */
                }
                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

        }

        // 根据指定的记录路径获得浏览格式记录
        // parameter:
        //		aRecord	返回的浏览记录信息。一个ArrayList数组。每个元素为一个string[]，所包含的内容
        //				根据strStyle而定。如果strStyle中有id，则aRecord每个元素中的string[]第一个字符串就是id，后面是各列内容。
        public int GetBrowseRecords(string[] paths,
            string strStyle,
            out ArrayList aRecord,
            out string strError)
        {
            strError = "";
            aRecord = new ArrayList();

            int nStart = 0;

            bool bIncludeID = StringUtil.IsInList("id", strStyle, true);

            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度
                try
                {
                    int nPerCount = paths.Length - nStart;

                    string[] temppaths = new string[nPerCount];
                    Array.Copy(paths, nStart, temppaths, 0, nPerCount);

                    IAsyncResult soapresult = this.ws.BeginGetBrowse(
                        temppaths,
                        strStyle,
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Record[] records = null;
                    Result result = this.ws.EndGetBrowse(
                        out records,
                        soapresult);

                    if (result.Value == -1)
                    {
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        if (records == null)
                            throw new Exception("WebService GetBrowse() API record参数返回值不应为null");

                        //lTotalCount = result.Value;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        Record record = records[i];

                        if (bIncludeID == true)
                        {

                            string[] cols = new string[record.Cols.Length + (bIncludeID == true ? 1 : 0)];

                            if (bIncludeID)
                                cols[0] = record.Path;

                            if (record.Cols.Length > 0)
                                Array.Copy(record.Cols, 0, cols, (bIncludeID == true ? 1 : 0), record.Cols.Length);

                            aRecord.Add(cols);
                        }
                        else
                        {
                            aRecord.Add(record.Cols);
                        }
                    }


                    nStart += records.Length;

                    if (nStart >= paths.Length)
                        break;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }

            } // end of for

            return 0;

        }


        // 获得浏览格式记录
        // parameter:
        //		nStart	起始序号
        //		nLength	长度
        public int GetRecords(
            string strResultSetName,
            long nStart,
            long nLength,
            string strLang,
            out ArrayList aRecord,
            out string strError)
        {
            strError = "";
            aRecord = new ArrayList();

            long nPerCount = -1;    // BUG? 这里有一点问题，可能获取过多的记录，超过nLength

            int nCount = 0;
            long lTotalCount = nLength;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        nStart,
                        nPerCount,
                        strLang,
                        "id,cols",
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Record[] records = null;
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        if (records == null)
                            throw new Exception("WebService GetRecords() API record参数返回值不应为null");

                        //lTotalCount = result.Value;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        Record record = records[i];

                        // 换成自己的类型，包含一个在结果集的序号
                        dp2opacRecord opacRecord = new dp2opacRecord();
                        opacRecord.Record = record;
                        opacRecord.IndexOfResult = nStart;
                        aRecord.Add(opacRecord);

                        nStart++;
                        nCount++;

                        if (lTotalCount != -1
                            && nCount >= lTotalCount)
                            break;
                    }

                    if (lTotalCount != -1
                        && nCount >= lTotalCount)
                        break;

                    if (nCount >= result.Value)
                        break;

                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }

            } // end of for

            return 0;

        }


        // 获得浏览格式记录
        // parameter:
        //		nStart	起始序号
        //		nLength	长度
        public int GetRichRecords(
            string strResultSetName,
            long nStart,
            long nLength,
            string strStyle,
            string strLang,
            out List<RichRecord> aRecord,
            out string strError)
        {
            strError = "";
            aRecord = new List<RichRecord>();

            long nPerCount = nLength;

            int nCount = 0;
            long lTotalCount = nLength;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度
                try
                {
                    string strRange = nStart.ToString() + "-" + (nStart + nPerCount - 1).ToString();

                    IAsyncResult soapresult = this.ws.BeginGetRichRecords(
                        strResultSetName,
                        strRange,
                        strLang,
                        strStyle,
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    RichRecord[] records = null;
                    Result result = this.ws.EndGetRichRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        if (records == null)
                            throw new Exception("WebService GetRichRecords() API record参数返回值不应为null");

                        //lTotalCount = result.Value;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        RichRecord record = records[i];

                        aRecord.Add(record);

                        nStart++;
                        nCount++;

                        if (lTotalCount != -1
                            && nCount >= lTotalCount)
                            break;
                    }

                    nPerCount -= records.Length;
                    if (nPerCount <= 0)
                        break;

                    if (lTotalCount != -1
                        && nCount >= lTotalCount)
                        break;

                    if (nCount >= result.Value)
                        break;

                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }
            } // end of for

            return 0;
        }

        public static string BuildDisplayKeyString(KeyFrom[] keys)
        {
            if (keys == null || keys.Length == 0)
                return "";
            string strResult = "";
            foreach (KeyFrom entry in keys)
            {
                if (String.IsNullOrEmpty(entry.Logic) == false)
                {
                    strResult += " " + entry.Logic + " ";
                }
                else if (String.IsNullOrEmpty(strResult) == false)
                    strResult += " | ";

                strResult += entry.Key + ":" + entry.From;
            }

            return strResult;
        }

        // 获取浏览记录
        public long DoBrowse(
            BrowseList listView,
            string strLang,
            DigitalPlatform.Stop stop,
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            Record[] records = null;

            long nStart = 0;
            long nPerCount = -1;

            int nCount = 0;

            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                nPerCount = -1; // 2013/2/12
            REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        nStart,
                        nPerCount,
                        strLang,
                        strOutputStyle, //"id,cols",
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }

                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;

                        if (nStart == 0 && stop != null)
                            stop.SetProgressRange(0, lTotalCount);
                    }

                    if (records != null)
                    {
                        nCount += records.Length;
                    }

                    listView.BeginUpdate();
                    try
                    {
                        // 做事
                        for (int i = 0; i < records.Length; i++)
                        {
                            DoIdle(); // 出让控制权，避免CPU资源耗费过度

                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }

                                stop.SetMessage("正在装入 " + Convert.ToString(nStart + i) + " / "
                                    + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)));
                            }

                            Record record = records[i];

                            string[] cols = null;
                            if (bOutputKeyID == true)
                            {
                                cols = new string[(record.Cols == null ? 0 : record.Cols.Length) + 1];
                                cols[0] = BuildDisplayKeyString(record.Keys);
                                if (cols.Length > 1)
                                    Array.Copy(record.Cols, 0, cols, 1, cols.Length - 1);
                            }
                            else
                                cols = record.Cols;

                            listView.NewLine(record.Path + " @" + this.Url,
                                cols);
                            // record.ID 放入第一列
                            // record.Cols 放入其他列(如果为keyid方式，key在这一群的第一列)
                        }

                        if (stop != null)
                            stop.SetProgressValue(nStart + records.Length);
                    }
                    finally
                    {
                        listView.EndUpdate();
                    }

                    if (nCount >= result.Value)
                        break;

                    if (records != null)
                    {
                        nStart += records.Length;
                    }

                }
                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    // 2013/2/11
                    if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                    {
                        if (nPerCount > 1 || nPerCount == -1)
                            nPerCount = 1;   // 修改为最小数量重做一次
                        else
                            return -1;
                    }
                    goto REDO;
                }
            }

            this.ClearRedoCount();
            if (stop != null)
                stop.HideProgress();
            return 0;
        }

        public long DoGetSearchResult(
            string strResultSetName,
            long lMax,
            string strLang,
            DigitalPlatform.Stop stop,
            out List<string> aPath,
            out string strError)
        {
            return DoGetSearchResult(
                strResultSetName,
                0,
                lMax,
                strLang,
                stop,
                out aPath,
                out strError);
        }

        // 希望逐步废止本函数
        // 获取检索命中结果
        // 只获取id列
        // return:
        //      -1  出错
        //      其他    aPath中的结果数目
        public long DoGetSearchResult(
            string strResultSetName,
            long lStart,
            long lMax,
            string strLang,
            DigitalPlatform.Stop stop,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            Record[] records = null;

            long nPerCount = lMax;	// -1;

            // int nCount = 0;

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        lStart,
                        nPerCount,
                        strLang,
                        "id",	// 不要cols
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;
                        if (lMax != -1)
                            lTotalCount = Math.Min(lTotalCount, lMax);
                    }

                    if (records != null)
                    {
                        lStart += records.Length;
                        // nCount += records.Length;
                        nPerCount = lTotalCount - lStart;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(lStart + i) + " / "
                                + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)));
                        }

                        Record record = records[i];
                        aPath.Add(record.Path);
                    }

                    // BUG修改 2010/11/16
                    if (/*lStart + nCount >= result.Value
                        || */ lStart >= lTotalCount)
                        break;
                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    // 2013/2/11
                    if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                    {
                        if (nPerCount > 1 || nPerCount == -1)
                            nPerCount = 1;   // 修改为最小数量重做一次
                        else
                            return -1;
                    }
                    goto REDO;
                }
            }

            this.ClearRedoCount();
            return aPath.Count;
        }

        // 改进版
        // 获取检索命中结果
        // 只获取id列
        // return:
        //      -1  出错
        //      其他    结果集内记录总数
        public long DoGetSearchResultEx(
            string strResultSetName,
            long lStart,
            long lMax,
            string strLang,
            DigitalPlatform.Stop stop,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            Record[] records = null;

            long nPerCount = lMax;	// -1;

            // int nCount = 0;

            long lResultTotalCount = -1;
            long lTempTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

            REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        lStart,
                        nPerCount,
                        strLang,
                        "id",	// 不要cols
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");

                        lResultTotalCount = result.Value;

                        lTempTotalCount = result.Value;
                        if (lMax != -1)
                            lTempTotalCount = Math.Min(lTempTotalCount, lMax);
                    }

                    if (records != null)
                    {
                        lStart += records.Length;
                        // nCount += records.Length;
                        nPerCount = lTempTotalCount - lStart;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(lStart + i) + " / "
                                + ((lTempTotalCount == -1) ? "?" : Convert.ToString(lTempTotalCount)));
                        }

                        Record record = records[i];

                        aPath.Add(record.Path);
                    }

                    // BUG修改 2010/11/16
                    if (/*lStart + nCount >= result.Value
                        || */
                             lStart >= lTempTotalCount)
                        break;
                }
                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    // 2013/2/11
                    if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                    {
                        if (nPerCount > 1 || nPerCount == -1)
                            nPerCount = 1;   // 修改为最小数量重做一次
                        else
                            return -1;
                    } 
                    
                    goto REDO;
                }
            }

            this.ClearRedoCount();
            return lResultTotalCount; // 结果集内的记录总数
        }

        // 获取检索命中结果
        // 获得每列详细信息的版本
        public long DoGetSearchFullResult(
            string strResultSetName,
            long lStart,
            long lMax,
            string strLang,
            DigitalPlatform.Stop stop,
            out ArrayList aLine,
            out string strError)
        {
            strError = "";
            aLine = new ArrayList();

            Record[] records = null;

            long nPerCount = lMax;	// -1;

            int nCount = 0;

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }


                    REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        lStart,
                        nPerCount,
                        strLang,
                        "id,cols",	// 不要cols
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;
                        if (lMax != -1)
                            lTotalCount = Math.Min(lTotalCount, lMax);
                    }

                    if (records != null)
                    {
                        lStart += records.Length;
                        nCount += records.Length;
                        nPerCount = lTotalCount - lStart;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(lStart + i) + " / "
                                + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)));
                        }



                        Record record = records[i];
                        string[] acol = new string[record.Cols.Length + 1];
                        acol[0] = record.Path;
                        for (int j = 0; j < record.Cols.Length; j++)
                        {
                            acol[j + 1] = record.Cols[j];
                        }

                        aLine.Add(acol);
                    }

                    if (nCount >= result.Value || nCount >= lTotalCount)
                        break;

                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;

                    // 2013/2/11
                    if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                    {
                        if (nPerCount > 1 || nPerCount == -1)
                            nPerCount = 1;   // 修改为最小数量重做一次
                        else
                            return -1;
                    } 
                    
                    goto REDO;
                }
            }

            this.ClearRedoCount();
            return 0;
        }

        // 2009/7/19
        // 获取检索命中结果
        // 获得某一列信息的版本
        public long DoGetSearchResultOneColumn(
            string strResultSetName,
            long lStart,
            long lMax,
            string strLang,
            DigitalPlatform.Stop stop,
            int nColumn,
            out List<string> aLine,
            out string strError)
        {
            strError = "";
            aLine = new List<string>();

            Record[] records = null;

            long nPerCount = lMax;	// -1;

            int nCount = 0;

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }


                    REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginGetRecords(
                        strResultSetName,
                        lStart,
                        nPerCount,
                        strLang,
                        "id,cols",	// 不要cols
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndGetRecords(
                        out records,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;
                        if (lMax != -1)
                            lTotalCount = Math.Min(lTotalCount, lMax);
                    }

                    if (records != null)
                    {
                        lStart += records.Length;
                        nCount += records.Length;
                        nPerCount = lTotalCount - lStart;
                    }

                    // 做事
                    for (int i = 0; i < records.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(lStart + i) + " / "
                                + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)));
                        }



                        Record record = records[i];
                        aLine.Add(record.Cols[nColumn]);
                    }

                    if (nCount >= result.Value || nCount >= lTotalCount)
                        break;

                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;

                    // 2013/2/11
                    if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                    {
                        if (nPerCount > 1 || nPerCount == -1)
                            nPerCount = 1;   // 修改为最小数量重做一次
                        else
                            return -1;
                    }

                    goto REDO;
                }
            }

            this.ClearRedoCount();
            return 0;
        }

        // 2012/11/11
        // 成批写入XML记录
        // 浅包装版本
        // 每个元素中Xml成员内放了一条完整的XML记录。如果记录不完整，请不要使用此API。
        // results中返回和inputs一样数目的元素，每个元素表示对应的inputs元素写入是否成功，返回时间戳和实际写入的路径
        // 在中途出错的情况下，results中的元素数目会比inputs中的少，但从前到后顺序是固定的，可以对应
        public long DoWriteRecords(
            DigitalPlatform.Stop stop,
            RecordBody[] inputs,
            string strStyle,
            out RecordBody[] results,
            out string strError)
        {
            strError = "";

            results = null;
        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginWriteRecords(
                    inputs,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                // Record[] records = null;

                Result result = this.ws.EndWriteRecords(
                    out results, soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;
                    return -1;
                }
                else
                {
                    // 可以在这里检查返回参数
                }

                this.ClearRedoCount();
                return result.Value;    // 结果集内总数
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 获得检索结果的浏览格式
        // 浅包装版本
        public long DoGetSearchResult(
            string strResultSetName,
            long lStart,
            long lMax,
            string strColumnStyle,
            string strLang,
            DigitalPlatform.Stop stop,
            out Record[] searchresults,
            out string strError)
        {
            strError = "";

            searchresults = null;

        REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetRecords(
                    strResultSetName,
                    lStart,
                    lMax,
                    strLang,
                    strColumnStyle, // "id,cols"
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                // Record[] records = null;

                Result result = this.ws.EndGetRecords(
                    out searchresults,  // records,
                    soapresult);

                if (result.Value == -1)
                {
                    // 2011/4/18
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin("",
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);

                    strError = result.ErrorString;
                    return -1;
                }
                else
                {
                    // Debug.Assert(records != null, "WebService GetRecords() API record参数返回值不应为null");
                }

#if NO
                // 将结果移出
                searchresults = new Record[records.Length]; // SearchResult
                for (int i = 0; i < records.Length; i++)
                {
                    searchresults[i] = records[i];
                }
#endif

                this.ClearRedoCount();
                return result.Value;    // 结果集内总数
            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;

                // 2013/2/11
                if (this.ErrorCode == ChannelErrorCode.QuotaExceeded)
                {
                    if (lMax > 1 || lMax == -1)
                        lMax = 1;   // 修改为最小数量重做一次
                    else
                        return -1;
                }

                goto REDO;
            }
        }

        // 模拟创建检索点
        public long DoGetKeys(
            string strRecPath,
            string strXmlBody,
            string strLang,
            // string strStyle,
            DigitalPlatform.Stop stop,
            out List<AccessKeyInfo> aLine,
            out string strError)
        {
            strError = "";
            aLine = null;

            if (strRecPath == "")
            {
                strError = "记录路径为空时无法模拟创建检索点";
                return -1;
            }

            KeyInfo[] keys = null;

            int nStart = 0;
            int nPerCount = -1;

            int nCount = 0;

            aLine = new List<AccessKeyInfo>();

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }


                    REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginCreateKeys(
                        strXmlBody,
                        strRecPath,
                        nStart,
                        nPerCount,
                        strLang,
                        // strStyle,
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndCreateKeys(
                        out keys,soapresult);

                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin(strRecPath,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }


                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(keys != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;
                    }

                    if (keys != null)
                    {
                        nStart += keys.Length;
                        nCount += keys.Length;
                    }

                    // 做事
                    for (int i = 0; i < keys.Length; i++)
                    {
                        /*
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null) 
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(nStart+i)+" / "
                                + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)) );
                        }
                        */
                        KeyInfo keyInfo = keys[i];

                        AccessKeyInfo info = new AccessKeyInfo();
                        info.FromValue = keyInfo.FromValue;
                        info.ID = keyInfo.ID;
                        info.Key = keyInfo.Key;
                        info.KeyNoProcess = keyInfo.KeyNoProcess;
                        info.Num = keyInfo.Num;
                        info.FromName = keyInfo.FromName;

                        aLine.Add(info);
                    }

                    if (nCount >= result.Value)
                        break;
                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }
            }

            this.ClearRedoCount();
            return 0;
        }

        // 模拟创建检索点
        public long DoGetKeys(
            string strRecPath,
            string strXmlBody,
            string strLang,
            // string strStyle,
            ViewAccessPointForm dlg,
            DigitalPlatform.Stop stop,
            out string strError)
        {
            strError = "";

            if (strRecPath == "")
            {
                strError = "记录路径为空时无法模拟创建检索点";
                return -1;
            }

            KeyInfo[] keys = null;

            int nStart = 0;
            int nPerCount = -1;

            int nCount = 0;

            dlg.Clear();

            long lTotalCount = -1;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }


                    REDO:
                try
                {
                    IAsyncResult soapresult = this.ws.BeginCreateKeys(
                        strXmlBody,
                        strRecPath,
                        nStart,
                        nPerCount,
                        strLang,
                        // strStyle,
                        null,
                        null);

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndCreateKeys(
                        out keys,soapresult);

                    if (result.Value == -1)
                    {
                        // 2011/4/21
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin("",
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    else
                    {
                        Debug.Assert(keys != null, "WebService GetRecords() API record参数返回值不应为null");

                        lTotalCount = result.Value;
                    }

                    if (keys != null)
                    {
                        nStart += keys.Length;
                        nCount += keys.Length;
                    }

                    // 做事
                    for (int i = 0; i < keys.Length; i++)
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            stop.SetMessage("正在装入 " + Convert.ToString(nStart + i) + " / "
                                + ((lTotalCount == -1) ? "?" : Convert.ToString(lTotalCount)));
                        }



                        KeyInfo keyInfo = keys[i];

                        dlg.NewLine(keyInfo);
                    }

                    if (nCount >= result.Value)
                        break;
                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }

            }

            this.ClearRedoCount();
            return 0;
        }


        // 列目录。返回字符串数组的简化版本
        // parameters:
        //      nType   只返回特定的资源类型
        public long DoDir(string strPath,
            string strLang,
            string strStyle,
            int nType,
            out string[] results,
            out string strError)
        {
            results = null;

            ResInfoItem[] results1 = null;
            long lRet = DoDir(strPath,
                strLang,
                strStyle,
                out results1,
                out strError);
            if (lRet == -1)
                return -1;
            int i = 0;
            ArrayList aResult = new ArrayList();
            for (i = 0; i < results1.Length; i++)
            {
                if (results1[i].Type != nType)
                    continue;
                aResult.Add(results1[i].Name);
            }

            results = new string[aResult.Count];
            for (i = 0; i < aResult.Count; i++)
            {
                results[i] = (string)aResult[i];
            }

            return lRet;
        }

        // 列资源目录
        public long DoDir(string strPath,
            string strLang,
            string strStyle,
            out ResInfoItem[] results,
            out string strError)
        {
            strError = "";
            results = null;

            ResInfoItem[] items = null;

            int nStart = 0;
            int nPerCount = -1;

            int nCount = 0;

            ArrayList aItem = new ArrayList();

            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

            REDO:
                try
                {
                REDODIR:
                    IAsyncResult soapresult = this.ws.BeginDir(strPath,
                        nStart,
                        nPerCount,
                        strLang,
                        strStyle,
                        null,
                        null);


                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndDir(
                        out items,soapresult);

                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin(strPath,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDODIR;
                        }

                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                        return -1;
                    }
                    if (items != null)
                    {
                        nStart += items.Length;
                        nCount += items.Length;
                    }

                    // 做事
                    for (int i = 0; i < items.Length; i++)
                    {
                        aItem.Add(items[i]);
                    }

                    if (nCount >= result.Value)
                        break;

                }
                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }
            } // end of for

            results = new ResInfoItem[aItem.Count];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = (ResInfoItem)aItem[i];
            }

            this.ClearRedoCount();
            return 0;
        }


        // 写入资源。原始版本。2007/5/27
        public long WriteRes(string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baContent,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            this.ErrorInfo = "";
            strError = "";
            strOutputResPath = "";
            baOutputTimestamp = null;

        REDO:
            try
            {
            REDOSAVE:
                IAsyncResult soapresult = this.ws.BeginWriteRes(strResPath,
                    strRanges,
                    lTotalLength,
                    baContent,
                    // null,	// attachmentid
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndWriteRes(
                    out strOutputResPath,
                    out baOutputTimestamp,soapresult);

                this.ErrorInfo = result.ErrorString;	// 无论是否返回错误，都将result的ErrorString放到Channel中

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strResPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSAVE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                        strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(baInputTimestamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(baOutputTimestamp) + "]";
                        return -1;
                    }

                    // 原来Convert....在这里，稍晚
                    return -1;
                }

                this.ClearRedoCount();
                return result.Value;
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 保存Xml记录
        public long DoSaveTextRes(string strPath,
            string strXml,
            bool bInlucdePreamble,
            string strStyle,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strOutputPath,
            out string strError)
        {
            this.ErrorInfo = "";
            strError = "";
            strOutputPath = "";
            output_timestamp = null;
            int nDoCount = 0;

            int nChunkMaxLength = 500 * 1024;	// chunk size。为了提升速度，应该尽量大。 原来是 4096

            int nStart = 0;

            byte[] baInputTimeStamp = null;
            //byte[] baOutputTimeStamp = null;
            output_timestamp = null;

            byte[] baPreamble = Encoding.UTF8.GetPreamble();

            byte[] baTotal = Encoding.UTF8.GetBytes(strXml);

            if (bInlucdePreamble == true
                && baPreamble != null && baPreamble.Length > 0)
            {
                byte[] temp = null;
                temp = ByteArray.Add(temp, baPreamble);
                baTotal = ByteArray.Add(temp, baTotal);
            }

            long lTotalLength = baTotal.Length;

            if (timestamp != null)
            {
                baInputTimeStamp = ByteArray.Add(baInputTimeStamp, timestamp);
            }

            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                // 切出chunk
                int nThisChunkSize = nChunkMaxLength;

                if (nThisChunkSize + nStart > lTotalLength)
                {
                    nThisChunkSize = (int)lTotalLength - nStart;	// 最后一次
                    if (nThisChunkSize <= 0 && nDoCount > 1)
                        break;
                }

                byte[] baChunk = new byte[nThisChunkSize];
                Array.Copy(baTotal, nStart, baChunk, 0, baChunk.Length);

            REDO:
                try
                {
                REDOSAVE:
                    string strMetadata = "";
                    string strRange = "";
                    int nEnd = nStart + baChunk.Length - 1;

                    // 2008/10/17 changed
                    if (nEnd >= nStart)
                        strRange = Convert.ToString(nStart) + "-" + Convert.ToString(nEnd);

                    IAsyncResult soapresult = this.ws.BeginWriteRes(strPath,
                        strRange,
                        //nStart,
                        lTotalLength,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                        baChunk,
                        // null,	// attachmentid
                        strMetadata,
                        strStyle,
                        baInputTimeStamp,
                        null,
                        null);
                    nDoCount++;

                    for (; ; )
                    {
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度
                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    Result result = this.ws.EndWriteRes(
                        out strOutputPath,
                        out output_timestamp/*baOutputTimeStamp*/,soapresult);

                    this.ErrorInfo = result.ErrorString;	// 无论是否返回错误，都将result的ErrorString放到Channel中
                    strError = result.ErrorString;  // 2007/6/28 服务于 带有局部path的保存中返回值放在strError的情况

                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin(strPath,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDOSAVE;
                        }

                        ConvertErrorCode(result);
                        // strError = result.ErrorString;


                        if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                        {
                            this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                            strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(baInputTimeStamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(output_timestamp/*baOutputTimeStamp*/) + "]";
                            return -1;
                        }

                        // 原来Convert....在这里，稍晚
                        return -1;
                    }

                    nStart += baChunk.Length;

                    if (nStart >= lTotalLength)
                        break;

                    Debug.Assert(strOutputPath != "", "outputpath不能为空");

                    strPath = strOutputPath;	// 如果第一次的strPath中包含'?'id, 必须用outputpath才能正确继续
                    baInputTimeStamp = output_timestamp;	//baOutputTimeStamp;

                }

                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }

            } // end of for

            // output_timestamp = baOutputTimeStamp;
            this.ClearRedoCount();
            return 0;
        }

        // 包装后的版本
                // 删除数据库记录
        public long DoDeleteRes(string strPath,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strError)
        {
            return DoDeleteRes(strPath,
                timestamp,
                "",
                out output_timestamp,
                out strError);
        }

        // 删除数据库记录
        public long DoDeleteRes(string strPath,
            byte[] timestamp,
            string strStyle,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

            /*
            if (timestamp == null)
            {
                Debug.Assert(true, "timestamp参数不能为null");
                strError = "timestamp参数不能为null";
                return -1;
            }
             */

            /*
            int nOldTimeout = this.Timeout;
            this.Timeout = 20 * 60 * 1000;
             * */

            // byte[] baOutputTimeStamp = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginDeleteRes(strPath,
                    timestamp,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndDeleteRes(
                    out output_timestamp,soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                        Debug.Assert(output_timestamp != null, "WebService API DeleteRes() TimestampMismatch时必须返回旧时间戳 ...");
                        strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(timestamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(output_timestamp) + "]";
                        return -1;
                    }

                    // 原来在这里，稍晚
                    return -1;
                }

            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                // this.Timeout = nOldTimeout;

            }

            this.ClearRedoCount();
            return 0;
        }

        // 刷新数据库记录keys
        // parameters:
        //      strStyle    next prev outputpath forcedeleteoldkeys
        //                  forcedeleteoldkeys 要在创建新keys前强制删除一下旧有的keys? 如果为包含，则强制删除原有的keys；如果为不包含，则试探着创建新的keys，如果有旧的keys和新打算创建的keys重合，那就不重复创建；如果旧的keys有残余没有被删除，也不管它们了
        //                          包含 一般用在单条记录的处理；不包含 一般用在预先删除了所有keys表的内容行以后在循环重建库中每条记录的批处理方式
        public long DoRebuildResKeys(string strPath,
            string strStyle,
            out string strOutputResPath,
            out string strError)
        {
            strError = "";
            strOutputResPath = "";

            /*
            int nOldTimeout = this.Timeout;
            this.Timeout = 20 * 60 * 1000;
            */

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginRebuildResKeys(strPath,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndRebuildResKeys(
                    out strOutputResPath,soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    /*
                    if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                        Debug.Assert(output_timestamp != null, "WebService API RebuildResKeys() TimestampMismatch时必须返回旧时间戳 ...");
                        strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(timestamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(output_timestamp) + "]";
                        return -1;
                    }*/

                    // 原来在这里，稍晚
                    return -1;
                }

            }

            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

            finally
            {
                // this.Timeout = nOldTimeout;
            }
            this.ClearRedoCount();

            return 0;
        }

        // 获得资源。返回字符串版本。适用于获得主记录体。
        // 可用来获得配置文件
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            CfgCache cache,
            string strPath,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {

            return GetRes(
                cache,
                strPath,
                "content,data,metadata,timestamp,outputpath",
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
        }


        // 获得资源。返回字符串版本。Cache版本。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            CfgCache cache,
            string strPath,
            string strStyle,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            // bool bExistInCache = false;

            string strFullPath = this.Url + "?" + strPath;


            if (StringUtil.IsInList("forceget", strStyle) == true)
            {
                // 强制获取

                StringUtil.RemoveFromInList("forceget",
                    true,
                    ref strStyle);
                goto GETDATA;
            }

            // 从cache中得到timestamp
            // return:
            //      -1  error
            //		0	not found
            //		1	found
            int nRet = cache.FindLocalFile(strFullPath,
                out strLocalName,
                out strTimeStamp);
            if (nRet == -1)
            {
                strResult = "";
                strMetaData = "";
                baOutputTimeStamp = null;
                strOutputResPath = "";
                strError = "CfgCache 尚未初始化";
                return -1;
            }
            if (nRet == 1)
            {
                Debug.Assert(strLocalName != "", "FindLocalFile()返回的strLocalName为空");

                if (strTimeStamp == "")
                    goto GETDATA;	// 时间戳不对, 那就只好重新获取服务器端内容

                Debug.Assert(strTimeStamp != "", "FindLocalFile()获得的strTimeStamp为空");
                cached_timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
                // bExistInCache = true;
            }
            else
                goto GETDATA;

            // 探测时间戳关系
            string strNewStyle = strStyle;

            /*
            StringUtil.RemoveFromInList("metadata",
                true,
                ref strNewStyle);	// 不要metadata
            */
            StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
true,
ref strNewStyle);	// 不要数据体和metadata

            long lRet = GetRes(strPath,
                strNewStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                StreamReader sr = null;

                try
                {
                    sr = new StreamReader(strLocalName, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }
                strResult = sr.ReadToEnd();
                sr.Close();

                return 0;	// 以无错误姿态返回
            }

        GETDATA:

            // 重新正式获取内容
            lRet = GetRes(strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputResPath,
                out strError);
            if (lRet == -1)
                return -1;

            // 因为时间戳不匹配而新获得了内容
            // 保存到cache
            cache.PrepareLocalFile(strFullPath, out strLocalName);
            Debug.Assert(strLocalName != "", "PrepareLocalFile()返回的strLocalName为空");

            // 写入文件,以便以后从cache获取
            StreamWriter sw = new StreamWriter(strLocalName,
                false,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strResult);
            sw.Close();
            sw = null;

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;


            return lRet;
        }

        // 获得资源。返回字符串版本。适用于获得主记录体。
        // 可用来获得配置文件
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(string strPath,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {

            return GetRes(strPath,
                "content,data,metadata,timestamp,outputpath",
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
        }

        // 获得资源。原始版本。2007/5/27
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(string strResPath,
            long lStart,
            int nLength,
            string strStyle,
            out byte[] baContent,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baContent = null;
            strMetadata = "";
            strError = "";
            strOutputResPath = "";
            baOutputTimestamp = null;

            // string strID = "";
            this.ErrorCode = ChannelErrorCode.None;
            this.ErrorInfo = "";

        REDO:
            try
            {

                // string strStyle = "content,data";
                IAsyncResult soapresult = this.ws.BeginGetRes(strResPath,
                    lStart,
                    nLength,
                    strStyle,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndGetRes(
                    out baContent,
                    // out strID,
                    out strMetadata,
                    out strOutputResPath,
                    out baOutputTimestamp,soapresult);

                // 即便不是返回-1,也可能有错误码和错误信息字符串
                ConvertErrorCode(result);
                strError = result.ErrorString;

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strResPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDO;
                    }
                    return -1;
                }


                this.ClearRedoCount();
                return result.Value;
            } // end try
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            // return 0;
        }

        // 获得资源。返回字符串版本。适用于获得主记录体。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(string strPath,
            string strStyle,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {

            strMetaData = "";
            strResult = "";
            strError = "";
            strOutputResPath = "";
            baOutputTimeStamp = null;

            this.ErrorCode = ChannelErrorCode.None;
            this.ErrorInfo = "";

            // string id = "";
            byte[] baContent = null;

            long lStart = 0;
            int nPerLength = -1;

            byte[] baTotal = null;

            // 2012/3/28
            // List<byte> bytes = new List<byte>();

            if (StringUtil.IsInList("attachmentid", strStyle) == true)
            {
                strError = "目前不支持 attachmentid";
                return -1;
            }

            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                REDO:
                try
                {

                    // string strStyle = "content,data";
                    IAsyncResult soapresult = this.ws.BeginGetRes(strPath,
                        lStart,
                        nPerLength,
                        strStyle,
                        null,
                        null);

                    for (; ; )
                    {

                        /*
                        try 
                        {
                            Application.DoEvents();	// 出让界面控制权
                        }
                        catch
                        {
                        }
					

                        // System.Threading.Thread.Sleep(10);	// 避免CPU资源过度耗费
                         */
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    // string strMetadata;
                    // string strOutputResPath;
                    Result result = this.ws.EndGetRes(
                        out baContent,
                        // out id,
                        out strMetaData,
                        out strOutputResPath,
                        out baOutputTimeStamp,soapresult);

                    // 即便不是返回-1,也可能有错误码和错误信息字符串
                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin(strPath,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }

                        /*
                        ConvertErrorCode(result);

                        strError = result.ErrorString;
                         */
                        return -1;
                    }



                    if (StringUtil.IsInList("data", strStyle) != true)
                        break;


                    baTotal = ByteArray.Add(baTotal, baContent);
                    // bytes.AddRange(baContent);

                    Debug.Assert(baContent.Length <= result.Value, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(result.Value) + "]");

                    lStart += baContent.Length;
                    if (lStart >= result.Value)
                        break;	// 结束

                    baContent = null;
                } // end try
                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }

            } // end of for


            this.ClearRedoCount();

            if (StringUtil.IsInList("data", strStyle) != true)
                return 0;

#if NO
            byte [] baTemp = new byte[bytes.Count];
            bytes.CopyTo(baTemp);

            strResult = Encoding.UTF8.GetString(baTemp);
#endif

            // 转换成字符串
            strResult = ByteArray.ToString(baTotal/*,
				Encoding.UTF8*/
                               );	// 将来做自动识别编码方式

            return 0;
        }


        // 获得资源。写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
        // parameters:
        //		strOutputFileName	输出文件名。可以为null。如果调用前文件已经存在, 会被覆盖。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(string strPath,
            string strOutputFileName,
            DigitalPlatform.Stop stop,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {
            FileStream fileTarget = null;

            string strStyle = "content,data,metadata,timestamp,outputpath";
            // string strStyle = "attachment,data,metadata,timestamp,outputpath";

            if (strOutputFileName != null)
                fileTarget = File.Create(strOutputFileName);
            else
            {
                strStyle = "metadata,timestamp,outputpath";
            }

            try
            {

                return GetRes(strPath,
                    fileTarget,
                    stop,
                    strStyle,
                    null,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
            }

            finally
            {
                if (fileTarget != null)
                    fileTarget.Close();
            }
        }

        /* 任延华加的，后来又去掉了，因为可以传WebPageStop了
                public long GetRes(string strPath,
                    Stream targetStream,
                    out string strMetaData,
                    out byte[] baOutputTimeStamp,
                    out string strOutputPath,
                    out string strError)
                {
                    string strStyle = "content,data,metadata,timestamp,outputpath";

                    return GetRes(strPath,
                        targetStream,
                        null, // stop
                        strStyle,
                        null, // baInputTimestamp
                        out strMetaData,
                        out baOutputTimeStamp,
                        out strOutputPath,
                        out strError);
                }
        */

        // 包装版本
        // 少了一个flushOutputMethod参数
        public long GetRes(string strPath,
    Stream fileTarget,
    DigitalPlatform.Stop stop,
    string strStyleParam,
    byte[] input_timestamp,
    out string strMetaData,
    out byte[] baOutputTimeStamp,
    out string strOutputPath,
    out string strError)
        {
            return GetRes(strPath,
            fileTarget,
            null,
            stop,
            strStyleParam,
            input_timestamp,
            out strMetaData,
            out baOutputTimeStamp,
            out strOutputPath,
            out strError);
        }

        // 获得资源。写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
        // parameters:
        //		fileTarget	文件。注意在调用函数前适当设置文件指针位置。函数只会在当前位置开始向后写，写入前不会主动改变文件指针。
        //		strStyleParam	一般设置为"content,data,metadata,timestamp,outputpath";
        //		input_timestamp	若!=null，则本函数会把第一个返回的timestamp和本参数内容比较，如果不相等，则报错
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(string strPath,
            Stream fileTarget,
			FlushOutput flushOutputMethod,
            DigitalPlatform.Stop stop,
            string strStyleParam,
            byte[] input_timestamp,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {
            strError = "";
            baOutputTimeStamp = null;
            strMetaData = "";
            strOutputPath = "";

            this.ErrorCode = ChannelErrorCode.None;
            this.ErrorInfo = "";

            string strStyle = strStyleParam;

            if (StringUtil.IsInList("attachment", strStyle) == true)
            {
                Debug.Assert(false, "attachment style暂时不能使用");
            }


            // 检查参数
            if (StringUtil.IsInList("data", strStyle) == false)
            {
                if (fileTarget != null)
                {
                    strError = "strStyle参数中若不包含data风格，则无法获得数据...";
                    return -1;
                }
            }
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (fileTarget == null)
                {
                    strError = "strStyle参数中若包含data风格，而fileTarget为null，会浪费通讯资源...";
                    return -1;
                }
            }

            bool bHasMetadataStyle = false;
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                bHasMetadataStyle = true;
            }

            // string id = "";
            byte[] baContent = null;

            long lStart = 0;
            int nPerLength = -1;

            byte[] old_timestamp = null;
            byte[] timestamp = null;

            long lTotalLength = -1;

            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                REDO:
                try
                {

                    string strMessage = "";

                    string strPercent = "";
                    if (lTotalLength != -1)
                    {
                        double ratio = (double)lStart / (double)lTotalLength;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (stop != null)
                    {
                        strMessage = "正在下载 " + Convert.ToString(lStart) + "-"
                            + (lTotalLength == -1 ? "?" : Convert.ToString(lTotalLength))
                            + " " + strPercent + " "
                            + strPath;
                        stop.SetMessage(strMessage);
                    }

                    IAsyncResult soapresult = this.ws.BeginGetRes(strPath,
                        fileTarget == null ? 0 : lStart,
                        fileTarget == null ? 0 : nPerLength,
                        strStyle,
                        null,
                        null);

                    for (; ; )
                    {

                        /*
                        try 
                        {
                            Application.DoEvents();	// 出让界面控制权
                        }
                        catch
                        {
                        }
					

                        // System.Threading.Thread.Sleep(10);	// 避免CPU资源过度耗费
                         */
                        DoIdle(); // 出让控制权，避免CPU资源耗费过度

                        bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                        if (bRet == true)
                            break;
                    }
                    if (this.m_ws == null)
                    {
                        strError = "用户中断";
                        this.ErrorCode = ChannelErrorCode.RequestCanceled;
                        return -1;
                    }
                    // string strOutputResPath;
                    Result result = this.ws.EndGetRes(
                        out baContent,
                        // out id,
                        out strMetaData,
                        out strOutputPath,
                        out timestamp,soapresult);

                    // 即便不是返回-1,也可能有错误码和错误信息字符串
                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                        {
                            // return:
                            //		-1	error
                            //		0	login failed
                            //		1	login succeed
                            int nRet = this.UiLogin(strPath,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                            {
                                return -1;
                            }

                            goto REDO;
                        }

                        /*
                        ConvertErrorCode(result);
                        strError = result.ErrorString;
                         */
                        return -1;
                    }

                    if (bHasMetadataStyle == true)
                    {
                        StringUtil.RemoveFromInList("metadata",
                            true,
                            ref strStyle);
                        bHasMetadataStyle = false;
                    }


                    lTotalLength = result.Value;


                    if (StringUtil.IsInList("timestamp", strStyle) == true
                        /*
                        && lTotalLength > 0
                         * */ )    // 2012/1/11
                    {
                        if (input_timestamp != null)
                        {
                            if (ByteArray.Compare(input_timestamp, timestamp) != 0)
                            {
                                strError = "下载过程中发现时间戳和input_timestamp参数中的时间戳不一致，下载失败 ...";
                                return -1;
                            }
                        }
                        if (old_timestamp != null)
                        {
                            if (ByteArray.Compare(old_timestamp, timestamp) != 0)
                            {
                                strError = "下载过程中发现时间戳变化，下载失败 ...";
                                return -1;
                            }
                        }
                    }

                    old_timestamp = timestamp;

                    if (fileTarget == null)
                        break;

                    // 写入文件
                    if (StringUtil.IsInList("attachment", strStyle) == true)
                    {
                        Debug.Assert(false, "attachment style暂时不能使用");
                        /*
						Attachment attachment = ws.ResponseSoapContext.Attachments[id];
						if (attachment == null)
						{
							strError = "id为 '" +id+ "' 的attachment在WebService响应中没有找到...";
							return -1;
						}
						StreamUtil.DumpStream(attachment.Stream, fileTarget);
						nStart += (int)attachment.Stream.Length;

						Debug.Assert(attachment.Stream.Length <= result.Value, "每次返回的包尺寸["+Convert.ToString(attachment.Stream.Length)+"]应当小于result.Value["+Convert.ToString(result.Value)+"]");
                         */

                    }
                    else
                    {
                        Debug.Assert(StringUtil.IsInList("content", strStyle) == true,
                            "不是attachment风格，就应是content风格");

                        Debug.Assert(baContent != null, "返回的baContent不能为null");
                        Debug.Assert(baContent.Length <= result.Value, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(result.Value) + "]");

                        fileTarget.Write(baContent, 0, baContent.Length);
                        if (flushOutputMethod != null)
                        {
                            if (flushOutputMethod() == false)
                            {
                                strError = "FlushOutputMethod()用户中断";
                                return -1;
                            }
                        } 
                        lStart += baContent.Length;
                    }

                    if (lStart >= result.Value)
                        break;	// 结束

                } // end try


                catch (Exception ex)
                {
                    /*
                    strError = ConvertWebError(ex);
                    return -1;
                     * */
                    int nRet = ConvertWebError(ex, out strError);
                    if (nRet == 0)
                        return -1;
                    goto REDO;
                }

            } // end of for

            baOutputTimeStamp = timestamp;
            this.ClearRedoCount();
            return 0;
        }

        string BuildMetadataXml(string strMime,
            string strLocalPath,
            string strLastModifyTime)
        {
            // string strMetadata = "<file mimetype='" + strMime + "' localpath='" + strLocalPath + "'/>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<file />");
            DomUtil.SetAttr(dom.DocumentElement, "mimetype", strMime);
            DomUtil.SetAttr(dom.DocumentElement, "localpath", strLocalPath);
            DomUtil.SetAttr(dom.DocumentElement, "lastmodifytime", strLastModifyTime);

            return dom.OuterXml;
        }

        // 保存资源记录
        // parameters:
        //		strPath	格式: 库名/记录号/object/对象xpath
        //		bTailHint	是否为最后一次写入操作。这是一个暗示参数，本函数将根据此参数为最后一次写入操作设置特殊的超时时间。
        //					假定有时整个资源尺寸很大，虽然每次局部写入耗时不多，但是最后一次写入因为服务器要执行整个资源转存
        //					的操作后API才返回，所以可能会耗费类似20分钟这样的长时间，导致WebService API超时失败。
        //					本参数是一个暗示操作(本函数也不担保一定要做什么操作)，如果调用者不清楚它的含义，可以使用false。
        public long DoSaveResObject(string strPath,
            string strObjectFileName,  // 任延华加,该参数代表存放对象数据的文件名
            string strLocalPath,       // 该参数代表本地文件名,有时会与strObjectFileName不同
            string strMime,
            string strLastModifyTime,   // 2007/12/13
            string strRange,
            bool bTailHint,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

            FileInfo fi = new FileInfo(strObjectFileName);
            if (fi.Exists == false)
            {
                strError = "文件 '" + strObjectFileName + "'不存在...";
                return -1;
            }

            byte[] baTotal = null;
            long lRet = RangeList.CopyFragment(
                strObjectFileName,
                strRange,
                out baTotal,
                out strError);
            if (lRet == -1)
                return -1;

            string strOutputPath = "";


            // int nOldTimeout = -1;
            if (bTailHint == true)
            {
                /*
                nOldTimeout = this.Timeout;
                this.Timeout = 20 * 60 * 1000;
                 * */
            }

        REDO:
            try
            {
            REDOSAVE:
                // string strMetadata = "<file mimetype='" + strMime + "' localpath='" + strLocalPath + "'/>";
                string strMetadata = BuildMetadataXml(strMime,
                    strLocalPath,
                    strLastModifyTime);

                IAsyncResult soapresult = this.ws.BeginWriteRes(strPath,
                    strRange,
                    fi.Length,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                    baTotal,
                    // null,	// attachmentid
                    strMetadata,
                    "",	// style
                    timestamp,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndWriteRes(
                    out strOutputPath,
                    out output_timestamp,soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSAVE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                        strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(timestamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(output_timestamp) + "]";
                        return -1;
                    }

                    // 原来在这里，稍晚
                    return -1;
                }
            }
            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                /*
                if (bTailHint == true)
                    this.Timeout = nOldTimeout;
                 * */

            }

        this.ClearRedoCount();
            return 0;
        }


        // 保存资源记录
        // parameters:
        //		strPath	格式: 库名/记录号/object/对象xpath
        //		strRange	本次想发送给服务器的局部。本函数将把这部分内容复制到内存byte[]结构中，
        //					因此，调用者必须考虑用合适的尺寸，避免超过内存极限引起进程被杀死。
        //		bTailHint	是否为最后一次写入操作。这是一个暗示参数，本函数将根据此参数为最后一次写入操作设置特殊的超时时间。
        //					假定有时整个资源尺寸很大，虽然每次局部写入耗时不多，但是最后一次写入因为服务器要执行整个资源转存
        //					的操作后API才返回，所以可能会耗费类似20分钟这样的长时间，导致WebService API超时失败。
        //					本参数是一个暗示操作(本函数也不担保一定要做什么操作)，如果调用者不清楚它的含义，可以使用false。
        public long DoSaveResObject(string strPath,
            Stream file,
            long lTotalLength,
            string strStyle,	// 2005/11/4
            string strMetadata,
            string strRange,
            bool bTailHint,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strOutputPath,
            out string strError)
        {
            //string strLocalPath,       // 该参数代表本地文件名,有时会与strObjectFileName不同
            //string strMime;

            this.ErrorCode = ChannelErrorCode.None;
            strError = "";
            output_timestamp = null;
            strOutputPath = "";

            byte[] baTotal = null;

            if (file != null)
            {

                if (file.Position + lTotalLength > file.Length)
                {
                    strError = "文件从当前位置 " + Convert.ToString(file.Position) + " 开始到末尾长度不足 " + Convert.ToString(lTotalLength);
                    return -1;
                }

                long lRet = RangeList.CopyFragment(
                    file,
                    lTotalLength,
                    strRange,
                    out baTotal,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            else
            {
                baTotal = new byte[0];	// 这是一个缺憾。应当许可为null
            }


            // int nOldTimeout = -1;
            if (bTailHint == true)
            {
                /*
                nOldTimeout = this.Timeout;
                this.Timeout = 20 * 60 * 1000;
                 * */
            }

        REDO:
            try
            {
            REDOSAVE:
                // string strMetadata = "<file mimetype='"+strMime+"' localpath='" + strLocalPath + "'/>";
                IAsyncResult soapresult = this.ws.BeginWriteRes(strPath,
                    strRange,
                    lTotalLength,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                    baTotal,
                    // null,	// attachmentid
                    strMetadata,
                    strStyle,	// style
                    timestamp,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }
                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = ChannelErrorCode.RequestCanceled;
                    return -1;
                }
                Result result = this.ws.EndWriteRes(
                    out strOutputPath,
                    out output_timestamp, soapresult);

                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCodeValue.NotLogin
                            && this.Container != null)
                    {
                        // return:
                        //		-1	error
                        //		0	login failed
                        //		1	login succeed
                        int nRet = this.UiLogin(strPath,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            return -1;
                        }

                        goto REDOSAVE;
                    }

                    ConvertErrorCode(result);
                    strError = result.ErrorString;

                    if (result.ErrorCode == ErrorCodeValue.TimestampMismatch)
                    {
                        this.ErrorCode = ChannelErrorCode.TimestampMismatch;
                        // output_timestamp 在出错情况下，也会返回服务器端希望的时间戳
                        strError = "时间戳不匹配。\r\n\r\n请求的时间戳 [" + ByteArray.GetHexTimeStampString(timestamp) + "] 响应的时间戳 [" + ByteArray.GetHexTimeStampString(output_timestamp) + "]";
                        return -1;
                    }

                    // 原来在这里，稍晚
                    return -1;
                }
            }



            catch (Exception ex)
            {
                /*
                strError = ConvertWebError(ex);
                return -1;
                 * */
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }

            finally
            {
                /*
                if (bTailHint == true)
                    this.Timeout = nOldTimeout;
                 * */

            }

            this.ClearRedoCount();
            return 0;
        }

        public void DoStop()
        {
            IAsyncResult result = this.ws.BeginStop(
                null,
                null);
        }

        public int DoTest(string strText)
        {
                IAsyncResult soapresult = this.ws.BeginDoTest(
                    strText,
                    null,
                    null);

                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度
                    bool bRet = soapresult.AsyncWaitHandle.WaitOne(100, false);
                    if (bRet == true)
                        break;
                }

                try
                {
                    return this.ws.EndDoTest(soapresult);
                }
                catch (WebException ex)
                {
                    return -1;
                }


        }

    }


    public class MyValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
        }
    }

    class CustomIdentityVerifier : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {

            foreach (ClaimSet claimset in authContext.ClaimSets)
            {
                if (claimset.ContainsClaim(identity.IdentityClaim))
                    return true;

                // string expectedSpn = null;
                if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                {
                    string strHost = (string)identity.IdentityClaim.Resource;

                    /*
                    expectedSpn = string.Format(CultureInfo.InvariantCulture, "host/{0}",
                        strHost);
                     * */
                    Claim claim = CheckDnsEquivalence(claimset, strHost);
                    if (claim != null)
                    {
                        return true;
                    }
                }
            }

            bool bRet = IdentityVerifier.CreateDefault().CheckAccess(identity, authContext);
            if (bRet == true)
                return true;

            return false;
        }

        Claim CheckDnsEquivalence(ClaimSet claimSet, string expedtedDns)
        {
            IEnumerable<Claim> claims = claimSet.FindClaims(ClaimTypes.Dns, Rights.PossessProperty);
            foreach (Claim claim in claims)
            {
                // 格外允许"localhost"
                if (expedtedDns.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    return claim;
                }

                string strCurrent = (string)claim.Resource;

                // 格外允许"DigitalPlatform"和任意出发字符串匹配
                if (strCurrent.Equals("DigitalPlatform", StringComparison.OrdinalIgnoreCase))
                    return claim;

                if (expedtedDns.Equals(strCurrent, StringComparison.OrdinalIgnoreCase))
                {
                    return claim;
                }
            }
            return null;
        }

        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            return IdentityVerifier.CreateDefault().TryGetIdentity(reference, out identity);
        }
    }

    public class OrgEndpointIdentity : EndpointIdentity
    {
        private string orgClaim;
        public OrgEndpointIdentity(string orgName)
        {
            orgClaim = orgName;
        }

        public string OrganizationClaim
        {
            get { return orgClaim; }
            set { orgClaim = value; }
        }
    }

#if NO
    public class OrgEndpointIdentity : EndpointIdentity
    {
        private string orgClaim;
        public OrgEndpointIdentity(string orgName)
        {
            orgClaim = orgName;
        }

        public string OrganizationClaim
        {
            get { return orgClaim; }
            set { orgClaim = value; }
        }
    }

    class CustomIdentityVerifier : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            bool returnvalue = false;

            foreach (ClaimSet claimset in authContext.ClaimSets)
            {
                foreach (Claim claim in claimset)
                {
                    if (claim.ClaimType == "http://schemas.microsoft.com/ws/2005/05/identity/claims/x500distinguishedname")
                    {
                        X500DistinguishedName name = (X500DistinguishedName)claim.Resource;
                        if (name.Name.Contains(((OrgEndpointIdentity)identity).OrganizationClaim))
                        {
                            //Console.WriteLine("Claim Type: {0}", claim.ClaimType);
                            //Console.WriteLine("Right: {0}", claim.Right);
                            //Console.WriteLine("Resource: {0}", claim.Resource);
                            //Console.WriteLine();
                            returnvalue = true;
                        }
                    }
                }

            }
            // return returnvalue;

            return true;
        }

        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            return IdentityVerifier.CreateDefault().TryGetIdentity(reference, out identity);
        }
    }

#endif

}
