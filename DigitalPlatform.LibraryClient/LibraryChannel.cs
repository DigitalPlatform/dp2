#define BASIC_HTTP

using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using System.IO;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.ServiceModel.Description;
using System.Web;

using DigitalPlatform.Range;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    /// <summary>
    /// 登录失败的原因
    /// </summary>
    public enum LoginFailCondition
    {
        /// <summary>
        /// 没有出错
        /// </summary>
        None = 0,   // 没有出错
        /// <summary>
        /// 一般错误
        /// </summary>
        NormalError = 1,    // 一般错误
        /// <summary>
        /// 密码不正确
        /// </summary>
        PasswordError = 2,  // 密码不正确

        /// <summary>
        /// 前端版本太旧
        /// </summary>
        ClientVersionTooOld = 3,    // 前端版本太旧

        NeedSmsLogin = 4,

        RetryLogin = 5,

        TempCodeMismatch = 6,
    }

    /// <summary>
    /// 通讯通道
    /// </summary>
    public class LibraryChannel : IDisposable
    {
        //internal ReaderWriterLock m_lock = new ReaderWriterLock();
        //internal static int m_nLockTimeout = 5000;	// 5000=5秒
        private static readonly Object syncRoot = new Object();

        /// <summary>
        /// dp2Library 服务器的 URL
        /// </summary>
        public string Url = "";

        /// <summary>
        /// 发出 dp2library 请求位置的上一个节点的 IP。格式为 节点名:IP 地址
        /// </summary>
        public string ClientIP { get; set; }

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
        public TimeSpan CloseTimeout = new TimeSpan(0, 0, 5);

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

#if BASIC_HTTP
                return GetInnerChannelOperationTimeout();
#else
                return this.m_ws.InnerChannel.OperationTimeout;
#endif
            }
            set
            {
                if (this.m_ws == null)
                    this.OperationTimeout = value;
                else
                {
                    // this.m_ws.InnerChannel.OperationTimeout = this.OperationTimeout; // BUG!!! 2015/12/3
                    this.OperationTimeout = value;

#if BASIC_HTTP
                    SetInnerChannelOperationTimeout(value);
#else
                    this.m_ws.InnerChannel.OperationTimeout = value;
#endif
                    // RefreshContext();
                }
            }
        }

#if BASIC_HTTP
        // localhost.dp2libraryRESTClient m_ws = null;	// 拥有
        dynamic m_ws = null;	// 拥有
#else
        localhost.dp2libraryClient m_ws = null;	// 拥有
#endif

        bool m_bStoped = false; // 检索是否被中断过一次
        int m_nInSearching = 0;
        int m_nRedoCount = 0;   // MessageSecurityException以后重试的次数

        /// <summary>
        /// 最近一次登录时用到的用户名
        /// </summary>
        public string UserName = "";
        /// <summary>
        /// 最近一次登录时用到的密码
        /// </summary>
        public string Password = "";

        /// <summary>
        /// 当前用户的权限字符串。最近一次登录成功后从服务器返回的
        /// </summary>
        public string Rights = "";

        /// <summary>
        /// 当前已登录用户所管辖的馆代码(列表)
        /// </summary>
        public string LibraryCodeList = ""; // 当前已登录用户所管辖的馆代码 2012/9/19

        /// <summary>
        /// 通道所使用的语言代码
        /// </summary>
        public string Lang
        {
            get;
            set;
        }

#if NO
        /// <summary>
        /// 当前通道所使用的 HTTP Cookies
        /// </summary>
        public CookieContainer Cookies = new System.Net.CookieContainer();
#endif

        /// <summary>
        /// 当前通道的登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;
        /// <summary>
        /// 当前通道的登录后事件
        /// </summary>
        public event AfterLoginEventHandle AfterLogin;

        /// <summary>
        /// 空闲事件
        /// </summary>
        public event IdleEventHandler Idle = null;

        /// <summary>
        /// 当前通道对象携带的扩展参数
        /// </summary>
        public object Param = null;

        /// <summary>
        /// 最近一次调用从 dp2Library 返回的错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;

        /// <summary>
        /// 最近一次调用 WCF 所产生的 Exception
        /// </summary>
        public Exception WcfException = null;  // 最近一次的Exception 2012/5/7

#if NO
        // 请改用 Param
        /// <summary>
        /// 当前对象可以携带的扩展参数
        /// </summary>
        public object Tag = null;   // 2008/10/28 //
#endif

        /// <summary>
        /// 最大接收消息的尺寸
        /// </summary>
        public int MaxReceivedMessageSize = 1024 * 1024;

        public void Dispose()
        {
            this.Close();

            // this.DisposeContext();

            this.BeforeLogin = null;
            this.AfterLogin = null;
            this.Idle = null;
        }

        // np0: namedpipe
        System.ServiceModel.Channels.Binding CreateNp0Binding()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            //binding.ReliableSession.Enabled = true;

            return binding;
        }

        // basic0: basic http
        System.ServiceModel.Channels.Binding CreateBasic0Binding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.AllowCookies = true;
            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            return binding;
        }

        // rest: rest http
        System.ServiceModel.Channels.Binding CreateRestBinding()
        {
            WebHttpBinding binding = new WebHttpBinding();
            binding.Security.Mode = System.ServiceModel.WebHttpSecurityMode.None;
            binding.AllowCookies = true;
            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            return binding;
        }

        // nt0: net.tcp
        System.ServiceModel.Channels.Binding CreateNt0Binding()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = this.InactivityTimeout;

            //binding.ReliableSession.Enabled = true;

            return binding;
        }

        // ws0:windows
        System.ServiceModel.Channels.Binding CreateWs0Binding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;

            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = this.InactivityTimeout;

            //binding.ReliableSession.Enabled = true;

            return binding;
        }

        void SetTimeout(System.ServiceModel.Channels.Binding binding)
        {
            binding.SendTimeout = this.SendTimeout;
            binding.ReceiveTimeout = this.RecieveTimeout;
            binding.CloseTimeout = this.CloseTimeout;
            binding.OpenTimeout = this.OpenTimeout;
        }

        // ws1:anonymouse
        System.ServiceModel.Channels.Binding CreateWs1Binding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            // binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;


            binding.MaxReceivedMessageSize = MaxReceivedMessageSize;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;

            SetTimeout(binding);

            // binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout = this.InactivityTimeout;

            // binding.Security.Message.EstablishSecurityContext = false;

            // return binding;

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

        // return:
        //      -1  error
        //      0   dp2Library的版本号过低。警告信息在strError中
        //      1   dp2Library版本号符合要求
        public static int GetServerVersion(
            LibraryChannel channel,
            Stop stop,
            out string strVersion,
            out string strError)
        {
            strError = "";
            strVersion = "0.0";

            // string strVersion = "";
            string strUID = "";
            long lRet = channel.GetVersion(stop,
out strVersion,
out strUID,
out strError);
            if (lRet == -1)
            {
                if (channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                {
                    // 2017/5/5
                    // 通讯安全性问题，时钟问题
                    // 极少可能是很早的 dp2library 版本不具备 GetVersion() API
                    strError = strError + "\r\n\r\n有可能是前端机器时钟和服务器时钟差异过大造成的(也有极少可能是用了很旧的 dp2library 版本)";
                    return -1;
#if NO
                    // 原来的dp2Library不具备GetVersion() API，会走到这里
                    strVersion = "0.0";
                    strError = "dp2 前端需要和 dp2Library 2.1 或以上版本配套使用 (而当前 dp2Library 版本号为 '2.0或以下' )。请升级 dp2Library 到最新版本。";
                    return 0;
#endif
                }

                strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strVersion) == true)
            {
                strVersion = "2.0";
            }

            string base_version = "2.60"; // 2.60 2015/12/8 开始
            if (StringUtil.CompareVersion(strVersion, base_version) < 0)   // 2.12
            {
                strError = "dp2 前端需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请尽快升级 dp2Library 到最新版本。";
                return 0;
            }

            return 1;
        }

#if NO
        // return:
        //      -1  error
        //      0   dp2Library的版本号过低。警告信息在strError中
        //      1   dp2Library版本号符合要求
        public static int GetServerVersion(
            LibraryChannel channel,
            Stop stop,
            out double version,
            out string strError)
        {
            strError = "";
            version = 0;

            string strVersion = "";
            string strUID = "";
            long lRet = channel.GetVersion(stop,
out strVersion,
out strUID,
out strError);
            if (lRet == -1)
            {
                if (channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                {
                    // 原来的dp2Library不具备GetVersion() API，会走到这里
                    version = 0;
                    strError = "dp2 前端需要和 dp2Library 2.1 或以上版本配套使用 (而当前 dp2Library 版本号为 '2.0或以下' )。请升级 dp2Library 到最新版本。";
                    return 0;
                }

                strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                return -1;
            }

            double value = 0;

            if (string.IsNullOrEmpty(strVersion) == true)
            {
                strVersion = "2.0以下";
                value = 2.0;
            }
            else
            {
                // 检查最低版本号
                if (double.TryParse(strVersion, out value) == false)
                {
                    strError = "dp2Library 版本号 '" + strVersion + "' 格式不正确";
                    return -1;
                }
            }

            version = value;

            double base_version = 2.60; // 2.60 2015/12/8 开始
            if (value < base_version)   // 2.12
            {
                strError = "dp2 前端需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 " + strVersion + " )。\r\n\r\n请尽快升级 dp2Library 到最新版本。";
                return 0;
            }

#if NO
                if (this.TestMode == true && this.Version < 2.34)
                {
                    strError = "dp2Circulation 的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 " + this.Version.ToString() + ")";
                    return -1;
                }
#endif

            return 1;
        }

#endif

        static string USER_AGENT = "User-Agent";
        static string DP2LIBRARYCLIENT = "dp2LibraryClient";

        static string TIMEOUT_HEADER = "_timeout";

        static string CLIENT_IP = "_clientip";

        // public localhost.LibraryWse ws
        /// <summary>
        /// 获取 localhost.dp2libraryClient 对象。这是 WCF 层的通道对象
        /// </summary>
#if BASIC_HTTP
        // localhost.dp2libraryRESTClient 
        dynamic
#else
        localhost.dp2libraryClient
#endif
 ws
        {
            get
            {
                if (m_ws == null)
                {
                    string strUrl = this.Url;

                    bool bWs0 = false;
                    Uri uri = new Uri(strUrl);

                    if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        EndpointAddress address = new EndpointAddress(strUrl);

                        this.m_ws = new localhost.dp2libraryClient(CreateNp0Binding(), address);
                    }
                    else if (uri.Scheme.ToLower() == "basic.http")
                    {
                        EndpointAddress address = new EndpointAddress(strUrl.Substring(6));

#if BASIC_HTTP
                        this.m_ws = new localhost.dp2libraryRESTClient(CreateBasic0Binding(), address);
#else
                            throw new Exception("当前条件编译版本不支持 basic.http 协议方式");
#endif

                        {
                            HttpUserAgentEndpointBehavior behavior = new HttpUserAgentEndpointBehavior(
                                () =>
                                {
                                    Dictionary<string, string> results = new Dictionary<string, string>();
                                    results.Add(USER_AGENT, DP2LIBRARYCLIENT);
                                    results.Add(TIMEOUT_HEADER, this.Timeout.ToString());
                                    if (string.IsNullOrEmpty(this.ClientIP) == false)
                                        results.Add(CLIENT_IP, this.ClientIP);
                                    return results;
                                }
                                );
                            this.m_ws.Endpoint.Behaviors.Add(behavior);
                        }

                        // RefreshContext();
                    }
                    else if (uri.Scheme.ToLower() == "rest.http")
                    {
                        EndpointAddress address = new EndpointAddress(strUrl.Substring(5));

                        // http://stackoverflow.com/questions/15401738/custom-endpoint-behavior-not-being-used-in-wcf-client-with-service-reference
                        var factory = new ChannelFactory<localhost.dp2libraryREST>(CreateRestBinding(), address);
                        if (factory.Endpoint.Behaviors.Find<WebHttpBehavior>() == null)
                        {
                            WebHttpBehavior behavior = new WebHttpBehavior();
                            behavior.DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped;
                            behavior.DefaultOutgoingRequestFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                            behavior.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                            behavior.AutomaticFormatSelectionEnabled = true;   // true
                            behavior.HelpEnabled = true;
                            factory.Endpoint.Behaviors.Add(behavior);
                        }

                        {
                            HttpUserAgentEndpointBehavior behavior = new HttpUserAgentEndpointBehavior(
                                () =>
                                {
                                    Dictionary<string, string> results = new Dictionary<string, string>();
                                    results.Add(USER_AGENT, DP2LIBRARYCLIENT);
                                    results.Add(TIMEOUT_HEADER, this.Timeout.ToString());
                                    if (string.IsNullOrEmpty(this.ClientIP) == false)
                                        results.Add(CLIENT_IP, this.ClientIP);
                                    return results;
                                }
                                );
                            factory.Endpoint.Behaviors.Add(behavior);
                        }


#if BASIC_HTTP
                        this.m_ws = factory.CreateChannel();
                        // this.m_ws = new localhost.dp2libraryRESTClient(CreateRestBinding(), address);
#else
                            throw new Exception("当前条件编译版本不支持 rest.http 协议方式");
#endif

                        // RefreshContext();
                    }
                    else if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        EndpointAddress address = new EndpointAddress(strUrl);

                        this.m_ws = new localhost.dp2libraryClient(CreateNt0Binding(), address);
                    }
                    else
                    {
                        if (uri.AbsolutePath.ToLower().IndexOf("/ws0") != -1)
                            bWs0 = true;

                        if (bWs0 == false)
                        {
                            // ws1 
                            EndpointAddress address = null;

                            {
                                address = new EndpointAddress(strUrl);
                                this.m_ws = new localhost.dp2libraryClient(CreateWs1Binding(), address);

                                this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
                                this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CustomCertificateValidator =
                new MyValidator();

#if NO
                                ////
                                this.m_ws.ClientCredentials.UserName.UserName = "test";
                                this.m_ws.ClientCredentials.UserName.Password = "";
#endif
                            }

                            /*
                            {

                                EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("DigitalPlatform");
                                address = new EndpointAddress(new Uri(strUrl),
                                    identity, new AddressHeaderCollection());
                                this.m_ws = new localhost.dp2libraryClient(CreateWs1Binding(), address);

                                // this.m_ws.ClientCredentials.ClientCertificate.SetCertificate(
                                this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
                                this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CustomCertificateValidator =
                new MyValidator();
                            }
                             * */

                        }
                        else
                        {
                            // ws0
                            EndpointAddress address = new EndpointAddress(strUrl);

                            this.m_ws = new localhost.dp2libraryClient(CreateWs0Binding(), address);
                            this.m_ws.ClientCredentials.UserName.UserName = "test";
                            this.m_ws.ClientCredentials.UserName.Password = "";
                        }
                    }

                    /*
#if BASIC_HTTP
                    if (this.m_ws == null)
                        throw new Exception("当前编译版本只能使用 basic.http 绑定方式");
#endif
                     * */

                    // 2017/7/5
#if BASIC_HTTP
                    SetInnerChannelOperationTimeout(this.OperationTimeout);
#else
                    this.m_ws.InnerChannel.OperationTimeout = this.OperationTimeout;
#endif
                }
                if (String.IsNullOrEmpty(this.Url) == true)
                {
                    throw (new Exception("Url值此时应当不等于空"));
                }
                Debug.Assert(this.Url != "", "Url值此时应当不等于空");

#if NO
                if (m_ws == null)
                {

                    /*
                    EndpointAddress address = new EndpointAddress(this.Url);
                    this.m_ws = new localhost.dp2libraryClient(binding, address);
                     * */
                    EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("DigitalPlatform");
                    EndpointAddress address = new EndpointAddress(new Uri(this.Url),
                        identity, new AddressHeaderCollection());
                    this.m_ws = new localhost.dp2libraryClient(binding, address);

                    // this.m_ws.ClientCredentials.ClientCertificate.SetCertificate(
                    this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = 
                        System.ServiceModel.Security.X509CertificateValidationMode.Custom;
                    this.m_ws.ClientCredentials.ServiceCertificate.Authentication.CustomCertificateValidator =
    new MyValidator();


                }
                if (String.IsNullOrEmpty(this.Url) == true)
                {
                    throw (new Exception("Url值此时应当不等于空"));
                }
                Debug.Assert(this.Url != "", "Url值此时应当不等于空");
#endif

                // m_ws.Url = this.Url;
                // m_ws.CookieContainer = this.Cookies;

#if BASIC_HTTP

#if NO
                if (this.m_ws is localhost.dp2libraryClient)
                {
                    localhost.dp2libraryClient temp = this.m_ws;
                    temp.InnerChannel.OperationTimeout = this.OperationTimeout;
                }
                else if (this.m_ws is localhost.dp2libraryRESTClient)
                {
                    localhost.dp2libraryRESTClient temp = this.m_ws;
                    temp.InnerChannel.OperationTimeout = this.OperationTimeout;
                }

#endif
                SetInnerChannelOperationTimeout(this.OperationTimeout);
#else
                this.m_ws.InnerChannel.OperationTimeout = this.OperationTimeout;
#endif

                this.WcfException = null;

                return m_ws;
            }
        }

#if NO
        OperationContextScope _context = null;

        void DisposeContext()
        {
            if (this._context != null)
            {
                this._context.Dispose();
                this._context = null;
            }
        }

        void RefreshContext()
        {
            if (this.m_ws is localhost.dp2libraryClient)
            {
                localhost.dp2libraryClient client = (this.m_ws as localhost.dp2libraryClient);
                this.DisposeContext();
                if (_context == null)
                    _context = new OperationContextScope(client.InnerChannel);
            }
            else
            {
                IContextChannel channel = ((IContextChannel)this.m_ws);

                this.DisposeContext();
                if (_context == null)
                    _context = new OperationContextScope(channel);

                {
                    // Add a HTTP Header to an outgoing request
                    HttpRequestMessageProperty requestMessage = null;
                    if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
                        requestMessage = OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                    if (requestMessage == null)
                    {
                        requestMessage = new HttpRequestMessageProperty();
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
                    }
                    requestMessage.Headers[USER_AGENT] = DP2LIBRARYCLIENT;
                    requestMessage.Headers[TIMEOUT_HEADER] = this.OperationTimeout.ToString();
                }
#if NO
                using (new OperationContextScope(channel))
                {
                    // Add a HTTP Header to an outgoing request
                    HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                    requestMessage.Headers["timeout"] = timeout.ToString();
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
                }
#endif
            }
        }
#endif

        /// <summary>
        /// 是否正在进行检索
        /// </summary>
        public int IsInSearching
        {
            get
            {
                return m_nInSearching;
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

        // 发送异常报告
        public static int CrashReport(
            string strSender,
            string strSubject,  // 一般为 "dp2circulation"
            string strContent,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = new LibraryChannel();
            channel.Url = "http://dp2003.com/dp2library";
            // channel.Url = "http://localhost:8001/dp2library";    // 测试用
            channel.Timeout = new TimeSpan(0, 1, 0);
            try
            {
                long lRet = channel.Login("public",
                    "",
                    "client=crashReport|0.01",
                    out strError);
                if (lRet != 1)
                    return -1;

                MessageData[] messages = new MessageData[1];
                MessageData[] output_messages = null;

                messages[0] = new MessageData();
                messages[0].strRecipient = "crash";
                messages[0].strSender = strSender;
                messages[0].strSubject = strSubject;
                messages[0].strMime = "text";
                messages[0].strBody = strContent;
                //messages[0].strRecordID = strOldRecordID;   // string strOldRecordID,
                //messages[0].TimeStamp = baOldTimeStamp;   // byte [] baOldTimeStamp,

                lRet = channel.SetMessage(
                    "send",
                    "",
                    messages,
                    out output_messages,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                channel.Close();
            }
        }

        /*
        // 2007/11/20
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

        /// <summary>
        /// 登录。
        /// 本方法由 dp2Library API Login() 浅包装而成。
        /// 请参考关于 dp2Library API Login() 的详细说明。
        /// 登录成功后，会自动设置好 Rights UserName LibraryCodeList 这几个成员
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassword">密码</param>
        /// <param name="strParameters">登录参数。这是一个逗号间隔的列表字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    登录未成功</para>
        /// <para>1:    登录成功</para>
        /// </returns>
        public long IdleLogin(string strUserName,
    string strPassword,
    string strParameters,
    out string strError)
        {
            string strRights = "";
            string strOutputUserName = "";
            string strLibraryCode = "";

            if (string.IsNullOrEmpty(this.Lang) == false)
            {
                Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                parameters["lang"] = this.Lang;
                strParameters = StringUtil.BuildParameterString(parameters);
            }

            long lRet = this.IdleLogin(
                strUserName,
                strPassword,
                strParameters,
                out strOutputUserName,
                out strRights,
                out strLibraryCode,
                out strError);
            this.Rights = strRights;
            this.UserName = strOutputUserName;    // 2011/7/29
            this.LibraryCodeList = strLibraryCode;
            return lRet;
        }

        // 尽量用这个版本
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        /// <summary>
        /// 登录。
        /// 本方法由 dp2Library API Login() 浅包装而成。
        /// 请参考关于 dp2Library API Login() 的详细说明。
        /// 登录成功后，会自动设置好 Rights UserName LibraryCodeList 这几个成员
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassword">密码</param>
        /// <param name="strParameters">登录参数。这是一个逗号间隔的列表字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    登录未成功</para>
        /// <para>1:    登录成功</para>
        /// </returns>
        public long Login(string strUserName,
    string strPassword,
    string strParameters,
    out string strError)
        {
            string strRights = "";
            string strOutputUserName = "";
            string strLibraryCode = "";

            if (string.IsNullOrEmpty(this.Lang) == false)
            {
                Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
                parameters["lang"] = this.Lang;
                strParameters = StringUtil.BuildParameterString(parameters);
            }

            long lRet = this.Login(
                strUserName,
                strPassword,
                strParameters,
                out strOutputUserName,
                out strRights,
                out strLibraryCode,
                out strError);
            this.Rights = strRights;
            this.UserName = strOutputUserName;    // 2011/7/29
            this.LibraryCodeList = strLibraryCode;
            return lRet;
        }

        // 异步的版本，里面用到 DoIdle
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        /// <summary>
        /// 登录。
        /// 请参考关于 dp2Library API Login() 的详细说明。
        /// 这是比较底层的版本。不会设置 Rights UserName LibraryCodeList 这几个成员。请慎用
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassword">密码</param>
        /// <param name="strParameters">登录参数。这是一个逗号间隔的列表字符串</param>
        /// <param name="strOutputUserName">返回实际登录的用户名</param>
        /// <param name="strRights">返回用户的权限字符串</param>
        /// <param name="strLibraryCode">返回用户所管辖的图书馆代码列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    登录未成功</para>
        /// <para>1:    登录成功</para>
        /// </returns>
        public long IdleLogin(string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strRights = "";
            strOutputUserName = "";
            strLibraryCode = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginLogin(
                    strUserName,
                    strPassword,
                    strParameters,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndLogin(
                    out strOutputUserName,
                    out strRights,
                    out strLibraryCode,
                    soapresult);
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

#if NO
            try
            {
                LibraryServerResult result = ws.Login(out strOutputUserName,
                    out strRights,
                    out strLibraryCode,
                    strUserName,
                    strPassword,
                    strParameters
                    );

                strError = result.ErrorInfo;
                return result.Value;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
#endif
        }

        // 不是异步的版本。不调用 DoIdle()
        // 一般不要用这个版本
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        /// <summary>
        /// 登录。
        /// 请参考关于 dp2Library API Login() 的详细说明。
        /// 这是比较底层的版本。不会设置 Rights UserName LibraryCodeList 这几个成员。请慎用
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassword">密码</param>
        /// <param name="strParameters">登录参数。这是一个逗号间隔的列表字符串</param>
        /// <param name="strOutputUserName">返回实际登录的用户名</param>
        /// <param name="strRights">返回用户的权限字符串</param>
        /// <param name="strLibraryCode">返回用户所管辖的图书馆代码列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    登录未成功</para>
        /// <para>1:    登录成功</para>
        /// </returns>
        public long Login(string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strRights = "";
            strOutputUserName = "";
            strLibraryCode = "";

            REDO:
            TimeSpan old_timeout = this.Timeout;
            if (this.Timeout < TimeSpan.FromSeconds(10))
                this.Timeout = TimeSpan.FromSeconds(10);
            try
            {
                LibraryServerResult result = ws.Login(out strOutputUserName,
                    out strRights,
                    out strLibraryCode,
                    strUserName,
                    strPassword,
                    strParameters
                    );

                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;  // 2016/7/2
                return result.Value;
            }
            catch (Exception ex)
            {
#if NO
                // TODO: 是否显示 InnerException? 本地时钟不对怎么办?
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
#endif
                // 2015/12/4
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            finally
            {
                if (this.Timeout != old_timeout)
                    this.Timeout = old_timeout;
            }
        }

        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 登出。
        /// 请参考 dp2Library API Logout() 的详细说明
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long Logout(out string strError)
        {
            strError = "";

            LibraryServerResult result = ws.Logout();

            strError = result.ErrorInfo;
            this.ErrorCode = result.ErrorCode;  // 2016/7/2
            return result.Value;
        }

#if NO
        // true 停止
        // false 继续
        bool DoIdle(Stop stop)
        {
            if (stop != null)
            {
                if (stop.State != 0)
                    return true;
            }

            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            try
            {
                Application.DoEvents();	// 出让界面控制权
            }
            catch
            {
            }

            return false;
        }
#endif
        void DoIdle()
        {
            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            // bool bDoEvents = true;
            if (this.Idle != null)
            {
                IdleEventArgs e = new IdleEventArgs();
                this.Idle(this, e);
                // bDoEvents = e.bDoEvents;
            }

#if NO
            if (bDoEvents == true)
            {
                Application.DoEvents();	// 出让界面控制权
            }
#endif

            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费
        }

        // return:
        //      0   主流程需返回-1
        //      1   需要重做API
        int ConvertWebError(Exception ex0,
            out string strError)
        {
            strError = "";

            this.WcfException = ex0;

            // 2017/10/15
            if (ex0 is InterruptException)
            {
                throw ex0;
            }

            // System.TimeoutException
            if (ex0 is System.TimeoutException)
            {
                this.ErrorCode = ErrorCode.RequestTimeOut;
                this.TryAbortIt();
                strError = GetExceptionMessage(ex0);
                return 0;
            }

            if (ex0 is System.ServiceModel.Security.MessageSecurityException)
            {
                System.ServiceModel.Security.MessageSecurityException ex = (System.ServiceModel.Security.MessageSecurityException)ex0;
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.TryAbortIt();
                // return ex.Message + (ex.InnerException != null ? " InnerException: " + ex.InnerException.Message : "") ;
                strError = GetExceptionMessage(ex);
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
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.TryAbortIt();
                strError = GetExceptionMessage(ex);
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
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.TryAbortIt();
                strError = "服务器 " + this.Url + " 没有响应";
                return 0;
            }

            if (ex0 is System.ServiceModel.CommunicationException
                && ex0.InnerException is System.ServiceModel.QuotaExceededException)
            {
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.MaxReceivedMessageSize *= 2;    // 下次扩大一倍
                this.TryAbortIt();
                strError = GetExceptionMessage(ex0);
                if (this.m_nRedoCount == 0
                    && this.MaxReceivedMessageSize < 1024 * 1024 * 10)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            // 2016/5/5
            if (ex0 is System.ServiceModel.CommunicationException
    && ex0.InnerException is System.IO.PipeException)
            {
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.TryAbortIt();
                strError = GetExceptionMessage(ex0);
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            /*
            if (ex0 is CommunicationException)
            {
                CommunicationException ex = (CommunicationException)ex0;

            }
             * */

            this.ErrorCode = ErrorCode.RequestError;	// 一般错误
            if (this.m_ws != null)
            {
                this.TryAbortIt();
                // 一般来说异常都需要重新分配Client()。如果有例外，可以在前面分支
            }
            strError = GetExceptionMessage(ex0);
            return 0;
        }

        static string GetExceptionMessage(Exception ex)
        {
            if (ex is NullReferenceException)
                return ExceptionUtil.GetDebugText(ex);  // 2015/11/8

            {
                string strResult = ex.GetType().ToString() + ":" + GetMessage(ex);
                while (ex != null)
                {
                    if (ex.InnerException != null)
                    {
                        strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": "
                            + GetMessage(ex.InnerException);
                    }

                    ex = ex.InnerException;
                }

                return strResult;
            }
        }

        static string GetMessage(Exception ex)
        {
            // 因为 Router 模块可能在 HTTP Reason-Phrase 里面用 UrlEncode 的汉字
            if (ex is CommunicationException)
                return HttpUtility.UrlDecode(ex.Message);
            return ex.Message;
        }

        // 检索读者信息
        // parameters:
        //      stop    停止对象
        //      strReaderDbNames    读者库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 <全部>/<all> 之一，表示全部读者库。
        //      strQueryWord    检索词
        //      nPerMax 一批检索命中的最大记录数。-1表示不限制。
        //      strFrom 检索途径
        //      strMatchStyle   匹配方式。值为left/right/exact/middle之一。
        //      strLang 界面语言代码。一般为"zh"。
        //      strResultSetName    结果集名。可使用null。而指定有区分的结果集名，可让两批以上不同目的的检索结果及互相不冲突。
        // 权限：
        //      读者不能检索任何人的读者记录，包括他自己的；
        //      工作人员需要 searchreader 权限
        // return:
        //      -1  error
        //      >=0 命中结果记录总数
        /// <summary>
        /// 检索读者记录。
        /// 请参考 dp2Library API SearchReader() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strReaderDbNames">读者库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 &lt;全部&gt;/&lt;all&gt; 之一，表示全部读者库。</param>
        /// <param name="strQueryWord">检索词</param>
        /// <param name="nPerMax">一批检索命中的最大记录数。-1表示不限制</param>
        /// <param name="strFrom">检索途径</param>
        /// <param name="strMatchStyle">匹配方式。值为left/right/exact/middle之一</param>
        /// <param name="strLang">界面语言代码。例如 "zh"</param>
        /// <param name="strResultSetName">结果集名。可使用null，等同于 "default"。而指定有区分的结果集名，可让两批以上不同目的的检索结果集可以共存</param>
        /// <param name="strOutputStyle">输出风格。keyid / keycount 之一。缺省为 keyid</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  检索命中的记录数</para>
        /// </returns>
        public long SearchReader(
            DigitalPlatform.Stop stop,
            string strReaderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchReader(
                    strReaderDbNames,
                        strQueryWord,
                        nPerMax,
                        strFrom,
                        strMatchStyle,
                        strLang,
                        strResultSetName,
                        strOutputStyle,
                        null,
                        null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchReader(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        /// <summary>
        /// 设置好友关系
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strAction">动作</param>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strComment">注释</param>
        /// <param name="strStyle">风格</param>
        /// <param name="strError">返回出错信息</param>
        /// <para>-1:   出错</para>
        /// <para>0:  请求成功(注意，并不代表对方同意)</para>
        /// <para>1:  请求前已经是好友关系了，没有必要重复请求</para>
        /// <para>2:  已经成功添加</para>
        public long SetFriends(
            DigitalPlatform.Stop stop,
            string strAction,
            string strReaderBarcode,
            string strComment,
            string strStyle,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetFriends(
                    strAction,
                    strReaderBarcode,
                    strComment,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetFriends(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得dp2Library版本号
        /// <summary>
        /// 获得 dp2Library 服务器版本号。
        /// 请参考 dp2Library API GetVersion() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strVersion">返回版本号</param>
        /// <param name="strUID">返回UID</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0: 成功</returns>
        public long GetVersion(
            DigitalPlatform.Stop stop,
            out string strVersion,
            out string strUID,
            out string strError)
        {
            strError = "";
            strVersion = "";
            strUID = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetVersion(
                        null,
                        null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetVersion(out strUID,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
                this.ClearRedoCount();

                strVersion = result.ErrorInfo;
                return result.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        // 设置通道语言
        /// <summary>
        /// 设置通道当前语言。
        /// 请参考 dp2Library API SetLang() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="strOldLang">返回本次调用前本通道使用的语言代码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long SetLang(
    DigitalPlatform.Stop stop,
    string strLang,
    out string strOldLang,
    out string strError)
        {
            strError = "";
            strOldLang = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetLang(
                    strLang,
                        null,
                        null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetLang(
                    out strOldLang,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
                this.ClearRedoCount();

                if (result.Value != -1)
                    this.Lang = strLang;
                return result.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        void ClearRedoCount()
        {
            this.m_nRedoCount = 0;
        }

        // 检索册信息
        // parameters:
        //      stop    停止对象
        //      strItemDbNames  实体库名的列表。可以包含多个库名，库名之间用逗号(半角)分隔
        //      strQueryWord    检索词
        //      nPerMax 一批检索命中的最大记录数。-1表示不限制。
        //      strFrom 检索途径
        //      strMatchStyle   匹配方式。值为left/right/exact/middle之一。
        //      strLang 界面语言代码。一般为"zh"。
        //      strResultSetName    结果集名。可使用null。而指定有区分的结果集名，可让两批以上不同目的的检索结果及互相不冲突。
        // 权限: 
        //      需要 searchitem 权限
        // return:
        //      -1  error
        //      >=0 命中结果记录总数
        // 注：
        //      实体库的数据格式都是统一的，检索途径可以穷举为：册条码号/批次号/登录号 ...
        /// <summary>
        /// 检索实体库记录。
        /// 请参考 dp2Library API SearchItem() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strItemDbNames">实体库名列表</param>
        /// <param name="strQueryWord">检索词</param>
        /// <param name="nPerMax">最大命中数。-1 表示不限制</param>
        /// <param name="strFrom">检索途径</param>
        /// <param name="strMatchStyle">匹配方式</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="strResultSetName">结果集名</param>
        /// <param name="strSearchStyle">检索方式。为 asc / desc之一，缺省为 asc</param>
        /// <param name="strOutputStyle">输出方式。keyid / keycount 之一。缺省为 keyid</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  检索命中的记录数</para>
        /// </returns>
        public long SearchItem(
            DigitalPlatform.Stop stop,
            string strItemDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchItem(
                    strItemDbNames,
                    strQueryWord,
                    nPerMax,
                    strFrom,
                    strMatchStyle,
                    strLang,
                    strResultSetName,
                    strSearchStyle,
                    strOutputStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchItem(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        /// <summary>
        /// 将 KeyFrom 数组构造为适合显示的字符串
        /// </summary>
        /// <param name="keys">检索词数组。即 KeyFrom 对象数组</param>
        /// <returns>字符串</returns>
        public static string BuildDisplayKeyString(DigitalPlatform.LibraryClient.localhost.KeyFrom[] keys)
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

        // 包装后版本
        // 只获得路径。确保所要的lStart lCount范围全部获得
        /// <summary>
        /// 获得检索结果。
        /// 本方法由 dp2Library API GetSearchResult() 浅包装而成。
        /// 请参考关于 dp2Library API GetSearchResult() 的介绍
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strResultSetName">结果集名</param>
        /// <param name="lStart">起始索引</param>
        /// <param name="lCount">要获得的数量</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="paths">返回命中结果的记录路径数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  结果集内的记录数。注意，不是本次调用返回的结果数</para>
        /// </returns>
        public long GetSearchResult(
    DigitalPlatform.Stop stop,
    string strResultSetName,
    long lStart,
    long lCount,
    string strLang,
    out List<string> paths,
    out string strError)
        {
            strError = "";
            paths = new List<string>();

            long _lStart = lStart;
            long _lCount = lCount;
            long lHitCount = 0;
            for (; ; )
            {
                Record[] searchresults = null;
                long lRet = GetSearchResult(
            stop,
            strResultSetName,
            _lStart,
            _lCount,
            "id",
            strLang,
            out searchresults,
            out strError);
                if (lRet == -1)
                    return -1;
                lHitCount = lRet;
                if (_lCount == -1)
                    _lCount = lHitCount - _lStart;

                for (int j = 0; j < searchresults.Length; j++)
                {
                    Record record = searchresults[j];
                    paths.Add(record.Path);
                }

                _lStart += searchresults.Length;
                _lCount -= searchresults.Length;

                if (_lStart >= lHitCount)
                    break;
                if (_lCount <= 0)
                    break;
            }

            return lHitCount;
        }

        // 管理结果集
        // parameters:
        //      strAction   share/remove 分别表示共享为全局结果集对象/删除全局结果集对象
        /// <summary>
        /// 管理结果集。
        /// 本方法实际上是由 dp2Library API GetSearchResult() 包装而来。请参考其详细介绍。
        /// strAction 为 "share" 时，strResultSetName 内为要共享出去的通道结果集名，strGlobalResultName 为要共享成的全局结果集名；
        /// strAction 为 "remove" 时，strResultSetName 参数不使用(设置为空即可)，strGlobalResultName 为要删除的颧骨结果集名
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strAction">动作。为 share / remove 之一</param>
        /// <param name="strResultSetName">(当前通道)结果集名</param>
        /// <param name="strGlobalResultName">全局结果集名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long ManageSearchResult(
            DigitalPlatform.Stop stop,
            string strAction,
            string strResultSetName,
            string strGlobalResultName,
            out string strError)
        {
            Record[] searchresults = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetSearchResult(
                    strResultSetName,
                    0,
                    0,
                    "@" + strAction + ":" + strGlobalResultName,
                    "zh",
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetSearchResult(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        }

        // 获得检索命中的结果集信息
        // parameters:
        //      strResultSetName    结果集名。如果为空，表示使用当前缺省结果集"default"
        //      lStart  要获取的开始位置。从0开始计数
        //      lCount  要获取的个数
        //      strBrowseInfoStyle  所返回的SearchResult中包含哪些信息。为逗号分隔的字符串列表值，取值可为 id/cols 之一。例如，"id,cols"表示同时获取id和浏览信息各列，而"id"表示仅取得id列。
        //      strLang 语言代码。一般为"zh"
        //      searchresults   返回包含记录信息的SearchResult对象数组
        // rights:
        //      没有限制
        // return:
        //      -1  出错
        //      >=0 结果集内记录的总数(注意，并不是本批返回的记录数)
        /// <summary>
        /// 获得检索结果。
        /// 请参考关于 dp2Library API GetSearchResult() 的介绍
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strResultSetName">结果集名</param>
        /// <param name="lStart">起始索引</param>
        /// <param name="lCount">要获得的数量</param>
        /// <param name="strBrowseInfoStyle">返回信息的方式。
        /// id / cols / xml / timestamp / metadata / keycount / keyid 的组合。keycount 和 keyid 二者只能使用一个，缺省为 keyid。
        /// 还可以组合使用 format:???? 这样的子串，表示使用特定的浏览列格式
        /// </param>
        /// <param name="strLang">语言代码</param>
        /// <param name="searchresults">返回 Record 对象数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  结果集内的记录数。注意，不是本次调用返回的结果数</para>
        /// </returns>
        public long GetSearchResult(
            DigitalPlatform.Stop stop,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults,
            out string strError)
        {
            searchresults = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetSearchResult(
                    strResultSetName,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetSearchResult(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        }

        // 2009/11/6
        // 获得指定记录的浏览信息
        // parameters:
        // rights:
        //      没有限制
        // return:
        //      -1  出错
        /// <summary>
        /// 获得指定记录的浏览或详细信息。
        /// 请参考 dp2Library API GetBrowseRecords() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="paths">记录路径字符串数组</param>
        /// <param name="strBrowseInfoStyle">返回信息的方式。
        /// id / cols / xml / timestamp / metadata 的组合。
        /// 还可以组合使用 format:???? 这样的子串，表示使用特定的浏览列格式
        /// </param>
        /// <param name="searchresults">返回 Record 数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long GetBrowseRecords(
            DigitalPlatform.Stop stop,
            string[] paths,
            string strBrowseInfoStyle,
            out Record[] searchresults,
            out string strError)
        {
            searchresults = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetBrowseRecords(
                    paths,
                    strBrowseInfoStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetBrowseRecords(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得数据库记录
        // 本函数用来获得实体、书目等类型以外其他记录
        // parameters:
        //      stop    停止对象
        //      strPath 记录路径
        //      timestamp   返回记录的时间戳
        //      strXml  返回记录的XML字符串
        /// <summary>
        /// 获得数据库记录。
        /// 请参考 dp2Library API GetRecord() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strPath">记录路径</param>
        /// <param name="timestamp">返回时间戳</param>
        /// <param name="strXml">返回记录 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long GetRecord(
            DigitalPlatform.Stop stop,
            string strPath,
            out byte[] timestamp,
            out string strXml,
            out string strError)
        {
            timestamp = null;
            strXml = "";
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetRecord(
                    strPath,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetRecord(
                    out timestamp,
                    out strXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 保存读者记录
        /// <summary>
        /// 写入读者记录。
        /// 请参考 dp2Library API SetReaderInfo() 的详细信息
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strAction">动作。为 new / change / delete /changestate / changeforegift 之一</param>
        /// <param name="strRecPath">记录路径</param>
        /// <param name="strNewXml">新记录 XML</param>
        /// <param name="strOldXml">旧记录 XML</param>
        /// <param name="baOldTimestamp">时间戳</param>
        /// <param name="strExistingXml">返回数据库中已经存在的记录的 XML</param>
        /// <param name="strSavedXml">返回实际保存的记录 XML</param>
        /// <param name="strSavedRecPath">返回实际保存记录的路径</param>
        /// <param name="baNewTimestamp">返回最新时间戳</param>
        /// <param name="kernel_errorcode">内核错误码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// <para>1:    成功，但部分字段被拒绝</para>
        /// </returns>
        public long SetReaderInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            byte[] baOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out ErrorCodeValue kernel_errorcode,
            out string strError)
        {
            strError = "";

            strExistingXml = "";
            strSavedXml = "";
            strSavedRecPath = "";
            baNewTimestamp = null;
            kernel_errorcode = ErrorCodeValue.NoError;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetReaderInfo(
                    strAction,
                    strRecPath,
                    strNewXml,
                    strOldXml,
                    baOldTimestamp,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetReaderInfo(
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedRecPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 包装后的版本
        /// <summary>
        /// 获得读者记录。
        /// 本方法是对 dp2Library API GetReaderInfo() 的浅包装。
        /// 请参考 dp2Library API GetReaderInfo() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBarcode">读者证条码号，或者命令参数</param>
        /// <param name="strResultTypeList">希望获得的返回结果类型的列表。为 xml / html / text / calendar / advancexml / timestamp 的组合</param>
        /// <param name="results">返回结果信息的字符串数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到读者记录</para>
        /// <para>1:    找到读者记录</para>
        /// <para>&gt;>1:   找到多于一条读者记录，返回值是找到的记录数，这是一种不正常的情况</para>
        /// </returns>
        public long GetReaderInfo(
            DigitalPlatform.Stop stop,
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strError)
        {
            byte[] baTimestamp = null;
            string strRecPath = "";

            return GetReaderInfo(stop,
                strBarcode,
                strResultTypeList,
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
        }

        // 获得读者记录
        /// <summary>
        /// 获得读者记录
        /// 请参考 dp2Library API GetReaderInfo() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBarcode">读者证条码号，或者命令参数</param>
        /// <param name="strResultTypeList">希望获得的返回结果类型的列表。为 xml / html / text / calendar / advancexml / timestamp 的组合</param>
        /// <param name="results">返回结果信息的字符串数组</param>
        /// <param name="strRecPath">返回实际获取的记录的路径</param>
        /// <param name="baTimestamp">返回时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到读者记录</para>
        /// <para>1:    找到读者记录</para>
        /// <para>&gt;>1:   找到多于一条读者记录，返回值是找到的记录数，这是一种不正常的情况</para>
        /// </returns>
        public long GetReaderInfo(
            DigitalPlatform.Stop stop,
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp,
            out string strError)
        {
            results = null;
            strError = "";
            strRecPath = "";
            baTimestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetReaderInfo(
                    strBarcode,
                    strResultTypeList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetReaderInfo(
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // TODO: 可以增加一个时间参数，在特定时间范围内看次数。超过这一段时间后，重新计算次数。也就是说防范短期内密集重复登录
        int _loginCount = 0;

        // 处理登录事宜
        /// <summary>
        /// 处理登录事宜
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 登录成功</returns>
        public int DoNotLogin(ref string strError)
        {
            this.ClearRedoCount();

            if (this.BeforeLogin != null)
            {
                BeforeLoginEventArgs ea = new BeforeLoginEventArgs();
                ea.LibraryServerUrl = this.Url;
                ea.FirstTry = true;
                ea.ErrorInfo = strError;

                REDOLOGIN:
                this.BeforeLogin(this, ea);

                if (ea.Cancel == true)
                {
                    if (String.IsNullOrEmpty(ea.ErrorInfo) == true)
                        strError = "用户放弃登录";
                    else
                        strError = ea.ErrorInfo;
                    this.ErrorCode = localhost.ErrorCode.NotLogin;
                    return -1;
                }

                if (ea.Failed == true)
                {
                    strError = ea.ErrorInfo;
                    return -1;
                }

                // 2006/12/30
                if (this.Url != ea.LibraryServerUrl)
                {
                    this.Close();   // 迫使重新构造m_ws 2011/11/22
                    this.Url = ea.LibraryServerUrl;
                }

                string strMessage = "";
                if (ea.FirstTry == true)
                    strMessage = strError;

                if (_loginCount > 100)
                {
                    strError = "重新登录次数太多，超过 100 次，请检查登录 API 是否出现了逻辑问题";
                    _loginCount = 0;    // 重新开始计算
                    return -1;
                }

                _loginCount++;
                long lRet = this.Login(ea.UserName,
                    ea.Password,
                    ea.Parameters,
                    out strError);
                if (lRet != 1)   // lRet == -1 || lRet == 0 || lRet == 2
                {
                    if (this.ErrorCode == localhost.ErrorCode.ClientVersionTooOld)
                        return -1;

                    if (String.IsNullOrEmpty(strMessage) == false)
                        ea.ErrorInfo = strMessage + "\r\n\r\n首次自动登录报错: ";
                    else
                        ea.ErrorInfo = "";
                    ea.ErrorInfo += strError;
                    ea.FirstTry = false;
                    if (this.ErrorCode == localhost.ErrorCode.RetryLogin)
                        ea.LoginFailCondition = LoginFailCondition.RetryLogin;
                    else if (this.ErrorCode == localhost.ErrorCode.NeedSmsLogin)
                        ea.LoginFailCondition = LoginFailCondition.NeedSmsLogin;
                    else if (this.ErrorCode == localhost.ErrorCode.TempCodeMismatch)
                        ea.LoginFailCondition = LoginFailCondition.TempCodeMismatch;
                    else
                        ea.LoginFailCondition = LoginFailCondition.PasswordError;
                    goto REDOLOGIN;
                }

                // this.m_nRedoCount = 0;
                if (this.AfterLogin != null)
                {
                    AfterLoginEventArgs e1 = new AfterLoginEventArgs();
                    this.AfterLogin(this, e1);
                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        strError = e1.ErrorInfo;
                        return -1;
                    }
                }
                return 1;   // 登录成功,可以重做API功能了
            }

            return -1;
        }

        // 获得实体记录(简装版本，省掉3个输出参数)
        public long GetItemInfo(
            DigitalPlatform.Stop stop,
            string strBarcode,
            string strResultType,
            out string strResult,
            string strBiblioType,
            out string strBiblio,
            out string strError)
        {
            string strItemRecPath = "";
            string strBiblioRecPath = "";
            byte[] item_timestamp = null;

            return GetItemInfo(
                stop,
                "item",
                strBarcode,
                "",
                strResultType,
                out strResult,
                out strItemRecPath,
                out item_timestamp,
                strBiblioType,
                out strBiblio,
                out strBiblioRecPath,
                out strError);
        }

        // 包装后的版本
        public long GetItemInfo(
            DigitalPlatform.Stop stop,
            string strBarcode,
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strBiblioRecPath,
            out string strError)
        {
            return GetItemInfo(
                stop,
                "item",
                strBarcode,
                "",
                strResultType,
                out strResult,
                out strItemRecPath,
                out item_timestamp,
                strBiblioType,
                out strBiblio,
                out strBiblioRecPath,
                out strError);
        }

        // 获得实体记录
        // Result.Value -1出错 0册记录没有找到 1册记录找到 >1册记录命中多于1条
        /// <summary>
        /// 获得实体记录
        /// 请参考 dp2Library API GetItemInfo() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strItemDbType">数据库的类型</param>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="strItemXml">册记录XML。用于需要前端提交内容的场合</param>
        /// <param name="strResultType">希望在 strResult 参数中返回的册记录信息类型。值为 xml text html 之一</param>
        /// <param name="strResult">返回册记录的信息</param>
        /// <param name="strItemRecPath">返回册记录的路径</param>
        /// <param name="item_timestamp">返回册记录的时间戳</param>
        /// <param name="strBiblioType">希望在 strBiblio 参数中返回的书目信息类型。值为 xml text html 之一</param>
        /// <param name="strBiblio">返回(册记录所从属的)书目记录的信息</param>
        /// <param name="strBiblioRecPath">返回(册记录所从属的)书目记录的路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到册记录</para>
        /// <para>1:    找到册记录</para>
        /// <para>&gt;1:   找到多于一条册记录，返回值是找到的记录数，这是一种不正常的情况</para>
        /// </returns>
        public long GetItemInfo(
            DigitalPlatform.Stop stop,
            string strItemDbType,
            string strBarcode,
            string strItemXml,
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strBiblioRecPath,
            out string strError)
        {
            strResult = "";
            strBiblio = "";
            strError = "";

            strBiblioRecPath = "";
            strItemRecPath = "";

            item_timestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetItemInfo(
                    strItemDbType,
                    strBarcode,
                    strItemXml,
                    strResultType,
                    strBiblioType,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetItemInfo(
                    out strResult,
                    out strItemRecPath,
                    out item_timestamp,
                    out strBiblio,
                    out strBiblioRecPath,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 借书
        /// <summary>
        /// 借书或续借
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="bRenew">是否为续借。true 表示xujie；false 表示普通借阅</param>
        /// <param name="strReaderBarcode">读者证条码号，或读者身份证号</param>
        /// <param name="strItemBarcode">要借阅的册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认册记录的路径</param>
        /// <param name="bForce">此参数目前未使用，设为 false 即可</param>
        /// <param name="saBorrowedItemBarcode">针对同一读者的连续操作中已经借阅的册条码号数组。用于在读者信息 HTML 界面上为这些册的信息行设置特殊背景色</param>
        /// <param name="strStyle">操作风格</param>
        /// <param name="strItemFormatList">指定在 item_records 参数中返回信息的格式列表</param>
        /// <param name="item_records">返回册相关的信息数组</param>
        /// <param name="strReaderFormatList">指定在 reader_records 参数中返回信息的各式列表</param>
        /// <param name="reader_records">返回读者相关的信息数组</param>
        /// <param name="strBiblioFormatList">指定在 biblio_records 参数中返回信息的格式列表</param>
        /// <param name="biblio_records">返回书目相关的信息数组</param>
        /// <param name="aDupPath">如果发生条码号重复，这里返回了相关册记录的路径</param>
        /// <param name="strOutputReaderBarcode">返回实际操作针对的读者证条码号</param>
        /// <param name="borrow_info">返回 BorrowInfo 结构对象，里面是一些关于借阅的详细信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    操作成功</para>
        /// </returns>
        public long Borrow(
            DigitalPlatform.Stop stop,
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            bool bForce,
            string[] saBorrowedItemBarcode,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,
            string strBiblioFormatList,
            out string[] biblio_records,
            out string[] aDupPath,
            out string strOutputReaderBarcode,
            out BorrowInfo borrow_info,
            out string strError)
        {
            reader_records = null;
            item_records = null;
            biblio_records = null;
            aDupPath = null;
            strOutputReaderBarcode = "";
            borrow_info = null;
            strError = "";


            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginBorrow(
                    bRenew,
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    bForce,
                    saBorrowedItemBarcode,
                    strStyle,
                    strItemFormatList,
                    strReaderFormatList,
                    strBiblioFormatList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndBorrow(
                    out item_records,
                    out reader_records,
                    out biblio_records,
                    out borrow_info,
                    out aDupPath,
                    out strOutputReaderBarcode,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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


        }

        // 还书
        // return:
        //      -1  出错
        //      0   正常
        //      1   有超期情况
        /// <summary>
        /// 还书或声明丢失
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strAction">动作参数。为 return lost 之一</param>
        /// <param name="strReaderBarcode">读者证条码号，或读者身份证号</param>
        /// <param name="strItemBarcode">要还回或声明丢失的册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认册记录的路径</param>
        /// <param name="bForce">此参数目前未使用，设为 false 即可</param>
        /// <param name="strStyle">操作风格</param>
        /// <param name="strItemFormatList">指定在 item_records 参数中返回信息的格式列表</param>
        /// <param name="item_records">返回册相关的信息数组</param>
        /// <param name="strReaderFormatList">指定在 reader_records 参数中返回信息的各式列表</param>
        /// <param name="reader_records">返回读者相关的信息数组</param>
        /// <param name="strBiblioFormatList">指定在 biblio_records 参数中返回信息的格式列表</param>
        /// <param name="biblio_records">返回书目相关的信息数组</param>
        /// <param name="aDupPath">如果发生条码号重复，这里返回了相关册记录的路径</param>
        /// <param name="strOutputReaderBarcode">返回实际操作针对的读者证条码号</param>
        /// <param name="return_info">返回 ReturnInfo 结构对象，里面是一些关于还书的详细信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    操作成功</para>
        /// <para>1:    操作成功，并且有值得操作人员留意的情况。提示信息在 strError 中</para>
        /// </returns>
        public long Return(
            DigitalPlatform.Stop stop,
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            bool bForce,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,
            string strBiblioFormatList,
            out string[] biblio_records,
            out string[] aDupPath,
            out string strOutputReaderBarcode,
            out ReturnInfo return_info,
            out string strError)
        {
            item_records = null;
            reader_records = null;
            biblio_records = null;
            strError = "";
            aDupPath = null;
            strOutputReaderBarcode = "";
            return_info = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginReturn(
                    strAction,
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    bForce,
                    strStyle,
                    strItemFormatList,
                    strReaderFormatList,
                    strBiblioFormatList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndReturn(
                    out item_records,
                    out reader_records,
                    out biblio_records,
                    out aDupPath,
                    out strOutputReaderBarcode,
                    out return_info,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 交费
        // return:
        //      -1  出错
        //      0   正常
        //      1   部分成功
        /// <summary>
        /// 违约金/押金处理
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strFunction">动作参数</param>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="amerce_items">费用信息数组</param>
        /// <param name="failed_items">返回操作失败的费用信息数组</param>
        /// <param name="strReaderXml">返回改变后的读者记录 XML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    操作成功</para>
        /// <para>1:    部分成功。提示信息在 strError 中</para>
        /// </returns>
        public long Amerce(
            DigitalPlatform.Stop stop,
            string strFunction,
            string strReaderBarcode,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,  // 2011/6/27
            out string strReaderXml,
            out string strError)
        {
            strReaderXml = "";
            strError = "";
            failed_items = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginAmerce(
                    strFunction,
                    strReaderBarcode,
                    amerce_items,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndAmerce(
                    out failed_items,
                    out strReaderXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得实体信息
        // return:
        //      -1  出错
        //      0   正常
        /// <summary>
        /// 获得同一书目记录下的若干册记录信息
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="lStart">从何处开始获取</param>
        /// <param name="lCount">获取多少个册信息</param>
        /// <param name="strStyle">获取方式</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="entityinfos">返回册记录信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到</para>
        /// <para>&gt;=1:    成功。返回值是否找到的册记录总数。本次返回的册记录信息元素个数则通过 entity_infos.Length 反映</para>
        /// </returns>
        public long GetEntities(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] entityinfos,
            out string strError)
        {
            entityinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetEntities(
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetEntities(
                    out entityinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置实体信息
        // return:
        //      -1  出错
        //      0   正常
        /// <summary>
        /// 设置同一书目记录下的若干册记录信息
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="entityinfos">要设置的册信息数组</param>
        /// <param name="errorinfos">返回操作中发生错误的，或者虽然成功但需要进一步信息的册记录信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:    成功。返回值是 errorinfos 中元素的个数</para>
        /// </returns>
        public long SetEntities(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            EntityInfo[] entityinfos,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            errorinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetEntities(
                    strBiblioRecPath,
                    entityinfos,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetEntities(
                    out errorinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 列出书目库检索途径信息
        // parameters:
        //      stop    停止对象
        //      strLang 语言代码。一般为"zh"
        //      infos   返回检索途径信息数组
        // rights:
        //      需要 listbibliodbfroms (或者listdbfroms) 权限
        // return:
        //      -1  出错
        //      0   正常
        /// <summary>
        /// 获得各类数据库的检索途径信息
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strDbType">数据库类型</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="infos">返回检索途径信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    当前系统中没有此类数据库，所以无法获得其检索途径信息</para>
        /// <para>1:    成功</para>
        /// </returns>
        public long ListDbFroms(
            DigitalPlatform.Stop stop,
            string strDbType,
            string strLang,
            out BiblioDbFromInfo[] infos,
            out string strError)
        {
            infos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginListBiblioDbFroms(
                    strDbType,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndListBiblioDbFroms(
                    out infos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 检索书目信息
        // parameters:
        //      strBiblioDbNames    书目库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 <全部>/<all> 之一，表示全部书目库。
        //      strQueryWord    检索词
        //      nPerMax 一次命中结果的最大数。如果为-1，表示不限制。
        //      strFromStyle 检索途径角色值。
        //      strMathStyle    匹配方式 exact left right middle
        //      strLang 语言代码。一般为"zh"
        //      strResultSetName    结果集名。
        //      strQueryXml 返回数据库内核层所使用的XML检索式，便于进行调试
        // rights:
        //      需要 searchbiblio 权限
        // return:
        //      -1  出错
        //      >=0 命中结果条数
        /// <summary>
        /// 检索书目库
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBiblioDbNames">书目库名列表</param>
        /// <param name="strQueryWord">检索词</param>
        /// <param name="nPerMax">最大命中数。-1 表示不限制</param>
        /// <param name="strFromStyle">检索途径角色。可以是多个角色值的列举</param>
        /// <param name="strMatchStyle">匹配方式</param>
        /// <param name="strLang">语言代码</param>
        /// <param name="strResultSetName">结果集名</param>
        /// <param name="strSearchStyle">检索方式。如果包含子串"desc"表示命中结果按照降序排列；包含子串"asc"表示按照升序排列。缺省为升序排列</param>
        /// <param name="strOutputStyle">输出方式</param>
        /// <param name="strQueryXml">返回 dp2Library 所创建的检索式 XML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有命中</para>
        /// <para>&gt;=1:   命中。值为命中的记录条数</para>
        /// </returns>
        public long SearchBiblio(
            DigitalPlatform.Stop stop,
            string strBiblioDbNames,
            string strQueryWord,
            int nPerMax,
            string strFromStyle,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            string strLocationFilter,
            out string strQueryXml,
            out string strError)
        {
            strError = "";
            strQueryXml = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchBiblio(
                    strBiblioDbNames,
                    strQueryWord,
                    nPerMax,
                    strFromStyle,
                    strMatchStyle,
                    strLang,
                    strResultSetName,
                    strSearchStyle,
                    strOutputStyle,
                    strLocationFilter,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchBiblio(
                    out strQueryXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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


        // 获得书目记录
        /// <summary>
        /// 获得书目记录
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strBiblioXml">XML 格式的书目记录内容。这是前端向服务器提交的</param>
        /// <param name="strBiblioType">要获取的信息格式类型</param>
        /// <param name="strBiblio">返回信息内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到指定路径的书目记录</para>
        /// <para>1:    成功</para>
        /// </returns>
        public long GetBiblioInfo(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            string strBiblioXml,
            string strBiblioType,
            out string strBiblio,
            out string strError)
        {
            strBiblio = "";
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetBiblioInfo(
                    strBiblioRecPath,
                    strBiblioXml,
                    strBiblioType,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetBiblioInfo(
                    out strBiblio,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // *** 此API已经废止 ***
#if NO
        // 册条码号查重
        public long SearchItemDup(
            DigitalPlatform.Stop stop,
            string strBarcode,
            int nMax,
            out string [] paths,
            out string strError)
        {
            paths = null;
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchItemDup(
                    strBarcode,
                    nMax,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchItemDup(
                    out paths,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
#endif

        // 获得值列表
        /// <summary>
        /// 获得值列表
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strTableName">值列表的名字</param>
        /// <param name="strDbName">数据库名。可以为空</param>
        /// <param name="values">返回值列表，字符串数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long GetValueTable(
            DigitalPlatform.Stop stop,
            string strTableName,
            string strDbName,
            out string[] values,
            out string strError)
        {
            values = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetValueTable(
                    strTableName,
                    strDbName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetValueTable(
                    out values,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        void WaitComplete(IAsyncResult soapresult)
        {
            TimeSpan timeout = this.SendTimeout + this.Timeout;
            DateTime start = DateTime.Now;
            for (; ; )
            {
                DoIdle(); // 出让控制权，避免CPU资源耗费过度

                if (soapresult.IsCompleted)
                    break;

                if (DateTime.Now - start > timeout)
                    throw new TimeoutException("通道超时 " + timeout.ToString());
            }
        }

        // result.Value
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        /// <summary>
        /// 获得操作日志。一次获得多条操作日志
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strFileName">日志文件名，形态为"20120101.log"</param>
        /// <param name="lIndex">日志记录序号，从 0 开始计数</param>
        /// <param name="lHint">暗示参数</param>
        /// <param name="nCount">要获得的日志记录数量</param>
        /// <param name="strStyle">获取风格</param>
        /// <param name="strFilter">过滤风格</param>
        /// <param name="records">返回日志记录信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    指定的日志文件没有找到</para>
        /// <para>1:    成功</para>
        /// <para>2:    超过范围</para>
        /// </returns>
        public long GetOperLogs(
            DigitalPlatform.Stop stop,
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records,
            out string strError)
        {
            strError = "";
            records = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetOperLogs(
                    strFileName,
                    lIndex,
                    lHint,
                    nCount,
                    strStyle,
                    strFilter,
                    null,
                    null);

                WaitComplete(soapresult);

#if NO
                for (; ; )
                {
                    DoIdle(); // 出让控制权，避免CPU资源耗费过度

                    if (soapresult.IsCompleted)
                        break;
                }
#endif

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetOperLogs(
                    out records,
                    soapresult);
                strError = result.ErrorInfo;
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    strError = result.ErrorInfo;    // 2013/11/20
                    return -1;
                }
                this.ErrorCode = result.ErrorCode;
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
        }

        // return:
        //      -1  出错
        //      0   没有找到日志记录，或者附件不存在
        //      >0  附件总长度
        public long DownloadOperlogAttachment(
            DigitalPlatform.Stop stop,
            string strFileName,
            long lIndex,
            long lHint,
            string strOutputFileName,
            out long lHintNext,
            out string strError)
        {
            strError = "";
            lHintNext = -1;

            long lRet = -1;

            using (Stream stream = File.Create(strOutputFileName))
            {
                long lAttachmentFragmentStart = 0;
                int nAttachmentFragmentLength = -1;
                byte[] attachment_data = null;
                long lAttachmentTotalLength = 0;

                for (; ; )
                {
                    string strXml = "";
                    lRet = this.GetOperLog(
                        stop,
                        strFileName,
                        lIndex,
                        lHint,
                "dont_return_xml", // strStyle,
                "", // strFilter,
                out strXml,
                out lHintNext,
                lAttachmentFragmentStart,
                nAttachmentFragmentLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
                    if (lRet == -1)
                    {
                        goto DELETE_AND_RETURN;
                    }
                    // 日志记录不存在
                    if (lRet == 0)
                    {
                        lRet = 0;
                        goto DELETE_AND_RETURN;
                    }
                    // 没有附件
                    if (lAttachmentTotalLength == 0)
                    {
                        lRet = 0;
                        strError = "附件不存在";
                        goto DELETE_AND_RETURN;
                    }
                    if (attachment_data == null || attachment_data.Length == 0)
                    {
                        lRet = -1;
                        strError = "attachment_data == null || attachment_data.Length == 0";
                        goto DELETE_AND_RETURN;
                    }
                    stream.Write(attachment_data, 0, attachment_data.Length);
                    lAttachmentFragmentStart += attachment_data.Length;
                    if (lAttachmentFragmentStart >= lAttachmentTotalLength)
                        return lAttachmentTotalLength;
                }
            }
            DELETE_AND_RETURN:
            File.Delete(strOutputFileName);
            return lRet;
        }

        //
        // 获得日志
        // result.Value
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        /// <summary>
        /// 获得日志
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strFileName">日志文件名，形态为"20120101.log"</param>
        /// <param name="lIndex">日志记录序号，从 0 开始计数</param>
        /// <param name="lHint">暗示参数</param>
        /// <param name="strStyle">获取风格</param>
        /// <param name="strFilter">过滤风格</param>
        /// <param name="strXml">返回日志记录 XML</param>
        /// <param name="lHintNext">返回下一个日志记录的暗示参数</param>
        /// <param name="lAttachmentFragmentStart">附件片段开始位置</param>
        /// <param name="nAttachmentFragmentLength">要获取的附件片断长度</param>
        /// <param name="attachment_data">返回附件数据</param>
        /// <param name="lAttachmentTotalLength">返回附件总长度</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    指定的日志文件没有找到</para>
        /// <para>1:    成功</para>
        /// <para>2:    超过范围</para>
        /// </returns>
        public long GetOperLog(
            DigitalPlatform.Stop stop,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strFilter,
            out string strXml,
            out long lHintNext,
            long lAttachmentFragmentStart,
            int nAttachmentFragmentLength,
            out byte[] attachment_data,
            out long lAttachmentTotalLength,
            out string strError)
        {
            strError = "";

            strXml = "";
            lHintNext = -1;

            attachment_data = null;
            lAttachmentTotalLength = 0;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetOperLog(
                    strFileName,
                    lIndex,
                    lHint,
                    strStyle,
                    strFilter,
                    lAttachmentFragmentStart,
                    nAttachmentFragmentLength,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetOperLog(
                    out strXml,
                    out lHintNext,
                    out attachment_data,
                    out lAttachmentTotalLength,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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




        }

        // 获得日历
        /// <summary>
        /// 获得流通日历
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="strName">日历名</param>
        /// <param name="nStart">要获得的元素开始位置。从 0 开始计数</param>
        /// <param name="nCount">要获得的元素数量。若为 -1 表示希望获得尽可能多的元素</param>
        /// <param name="contents">返回的日历信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:   结果数量</para>
        /// </returns>
        public long GetCalendar(
            DigitalPlatform.Stop stop,
            string strAction,
            string strName,
            int nStart,
            int nCount,
            out CalenderInfo[] contents,
            out string strError)
        {
            strError = "";

            contents = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetCalendar(
                    strAction,
                    strName,
                    nStart,
                    nCount,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetCalendar(
                    out contents,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置、修改日历
        /// <summary>
        /// 设置流通日历
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="info">日历信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long SetCalendar(
    DigitalPlatform.Stop stop,
    string strAction,
    CalenderInfo info,
    out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetCalendar(
                    strAction,
                    info,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetCalendar(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 批处理任务
        /// <summary>
        /// 批处理任务
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strName">批处理任务名</param>
        /// <param name="strAction">动作参数</param>
        /// <param name="info">任务信息</param>
        /// <param name="resultInfo">返回任务信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0 或 1:    成功</para>
        /// </returns>
        public long BatchTask(
            DigitalPlatform.Stop stop,
            string strName,
            string strAction,
            BatchTaskInfo info,
            out BatchTaskInfo resultInfo,
            out string strError)
        {
            strError = "";
            resultInfo = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginBatchTask(
                    strName,
                    strAction,
                    info,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndBatchTask(
                    out resultInfo,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 检索读者信息
        /// <summary>
        /// 直接用 XML 检索式进行检索
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strQueryXml">检索式。采用 dp2Kernel 所定义的 XML 检索式格式</param>
        /// <param name="strResultSetName">结果集名</param>
        /// <param name="strOutputStyle">输出方式</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有命中</para>
        /// <para>&gt;=1:   命中。返回值为命中的记录条数</para>
        /// </returns>
        public long Search(
            DigitalPlatform.Stop stop,
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
                IAsyncResult soapresult = this.ws.BeginSearch(
                    strQueryXml,
                    strResultSetName,
                    strOutputStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearch(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        // 获得书目摘要
        /// <summary>
        /// 获得书目摘要
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认册记录的路径</param>
        /// <param name="strBiblioRecPathExclude">希望排除掉的书目记录路径。形式为逗号间隔的多个记录路径</param>
        /// <param name="strBiblioRecPath">返回书目记录路径</param>
        /// <param name="strSummary">返回书目摘要内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    指定的册记录或者书目记录没有找到</para>
        /// <para>1:    成功</para>
        /// </returns>
        public long GetBiblioSummary(
            DigitalPlatform.Stop stop,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strSummary = "";
#if NO
            // 测试
            strSummary = "test";
            return 0;
#endif
            TimeSpan old_timeout = this.Timeout;
            this.Timeout = new TimeSpan(0, 0, 5);
            try
            {
                while (true)
                {
                    this.WcfException = null;
                    this.BeginSearch();
                    try
                    {
                        IAsyncResult soapresult = this.ws.BeginGetBiblioSummary(
                            strItemBarcode,
                            strConfirmItemRecPath,
                            strBiblioRecPathExclude,
                            null,
                            null);

                        WaitComplete(soapresult);

                        if (this.m_ws == null)
                        {
                            strError = "用户中断";
                            this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                            return -1;
                        }

                        LibraryServerResult result = this.ws.EndGetBiblioSummary(
                            out strBiblioRecPath,
                            out strSummary,
                            soapresult);
                        if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                        {
                            if (DoNotLogin(ref strError) == 1)
                                continue;   // goto REDO;
                            return -1;
                        }
                        strError = result.ErrorInfo;
                        this.ErrorCode = result.ErrorCode;
                        this.ClearRedoCount();
                        return result.Value;
                    }
                    catch (Exception ex)
                    {
                        int nRet = ConvertWebError(ex, out strError);
                        if (nRet == 0)
                            return -1;
                        continue;   // goto REDO;
                    }
                    finally
                    {
                        this.EndSearch();
                        if (this.WcfException is TimeoutException)
                            strError = "通讯超时。";
                    }
                }
            }
            finally
            {
                this.Timeout = old_timeout;
            }
        }

        // 设置时钟
        // return:
        //      -1  出错
        //      0   正常
        /// <summary>
        /// 设置系统当前时钟
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strTime">要设置的当前时间。格式为 RFC1123</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long SetClock(
            DigitalPlatform.Stop stop,
            string strTime,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetClock(
                    strTime,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetClock(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得时钟
        /// <summary>
        /// 获得系统当前时间
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strTime">返回系统的当前时间。格式为 RFC1123</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long GetClock(
            DigitalPlatform.Stop stop,
            out string strTime,
            out string strError)
        {
            strTime = "";
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetClock(
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetClock(
                    out strTime,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 验证读者密码
        /// <summary>
        /// 验证读者帐户的密码
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strReaderPassword">要验证的读者帐户密码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   验证过程出错</para>
        /// <para>0:    密码不正确</para>
        /// <para>1:    密码正确</para>
        /// </returns>
        public long VerifyReaderPassword(
            DigitalPlatform.Stop stop,
            string strReaderBarcode,
            string strReaderPassword,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginVerifyReaderPassword(
                    strReaderBarcode,
                    strReaderPassword,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndVerifyReaderPassword(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 修改读者密码
        /// <summary>
        /// 修改读者帐户的密码
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strReaderOldPassword">读者帐户的旧密码</param>
        /// <param name="strReaderNewPassword">要修改成的新密码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    旧密码不正确</para>
        /// <para>1:    旧密码正确,已修改为新密码</para>
        /// </returns>
        public long ChangeReaderPassword(
            DigitalPlatform.Stop stop,
            string strReaderBarcode,
            string strReaderOldPassword,
            string strReaderNewPassword,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginChangeReaderPassword(
                    strReaderBarcode,
                    strReaderOldPassword,
                    strReaderNewPassword,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndChangeReaderPassword(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 重设读者密码
        // 其实这个 API 不需要登录
        /// <summary>
        /// 重设读者密码。本 API 不需要登录即可调用
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strParameters">参数字符串。内容为 tel=?????,barcode=?????,name=????? 或 email=?????,barcode=??????,name=?????? 或 librarycode=????</param>
        /// <param name="strMessageTemplate">消息文字模板。其中可以使用 %name% %barcode% %temppassword% %expiretime% %period% 等宏</param>
        /// <param name="strMessage">返回拟发送给读者的消息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    因为条件不具备功能没有成功执行</para>
        /// <para>1:     功能成功执行</para>
        /// </returns>
        public long ResetPassword(
            DigitalPlatform.Stop stop,
            string strParameters,
            string strMessageTemplate,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginResetPassword(
                    strParameters,
                    strMessageTemplate,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndResetPassword(out strMessage,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long BindPatron(
            DigitalPlatform.Stop stop,
            string strAction,
            string strQueryWord,
            string strPassword,
            string strBindingID,
            string strStyle,
            string strResultTypeList,
            out string[] results,
            out string strError)
        {
            strError = "";
            results = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginBindPatron(
                    strAction,
                    strQueryWord,
                    strPassword,
                    strBindingID,
                    strStyle,
                    strResultTypeList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndBindPatron(
                    out results,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 清除所有数据库内的数据
        /// <summary>
        /// 清除所有数据库的记录
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long ClearAllDbs(
            DigitalPlatform.Stop stop,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginClearAllDbs(
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndClearAllDbs(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 兼容以前用法
        public long ManageDatabase(
    DigitalPlatform.Stop stop,
    string strAction,
    string strDatabaseName,
    string strDatabaseInfo,
    out string strOutputInfo,
    out string strError)
        {

            return ManageDatabase(
                stop,
                strAction,
                strDatabaseName,
                strDatabaseInfo,
                "",
                out strOutputInfo,
                out strError);
        }

        // 管理数据库
        /// <summary>
        /// 管理数据库
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="strDatabaseName">数据库名</param>
        /// <param name="strDatabaseInfo">数据库信息</param>
        /// <param name="strStyle">风格</param>
        /// <param name="strOutputInfo">返回操作后的数据库信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0 或 1:    成功</para>
        /// </returns>
        public long ManageDatabase(
            DigitalPlatform.Stop stop,
            string strAction,
            string strDatabaseName,
            string strDatabaseInfo,
            string strStyle,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";
            strOutputInfo = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginManageDatabase(
                    strAction,
                    strDatabaseName,
                    strDatabaseInfo,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndManageDatabase(
                    out strOutputInfo,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得用户信息
        /// <summary>
        /// 获得用户信息
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="strName">用户名</param>
        /// <param name="nStart">要获取用户信息元素的开始位置</param>
        /// <param name="nCount">要获取的用户信息元素的个数。-1 表示希望获取尽可能多的元素</param>
        /// <param name="contents">返回用户信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:   相关用户信息元素的总数</para>
        /// </returns>
        public long GetUser(
            DigitalPlatform.Stop stop,
            string strAction,
            string strName,
            int nStart,
            int nCount,
            out UserInfo[] contents,
            out string strError)
        {
            strError = "";

            contents = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetUser(
                    strAction,
                    strName,
                    nStart,
                    nCount,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetUser(
                    out contents,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置用户信息
        /// <summary>
        /// 设置用户信息
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="info">用户信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long SetUser(
            DigitalPlatform.Stop stop,
            string strAction,
            UserInfo info,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetUser(
                    strAction,
                    info,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetUser(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 
        /// <summary>
        /// 获得通道信息
        /// </summary>
        /// <param name="stop">停止对象</param>
        /// <param name="strQuery">检索式。例如 "ip=...,username=..."</param>
        /// <param name="strStyle">风格。"ip-count"，或者空</param>
        /// <param name="nStart">开始偏移，从 0 开始计数</param>
        /// <param name="nCount">希望最多获取多少个事项。-1 表示想尽可能多地获取</param>
        /// <param name="contents">返回通道信息数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 其他: 本次请求的总数量，可能会大于返回的 contents 中包含的数量</returns>
        public long GetChannelInfo(
            DigitalPlatform.Stop stop,
            string strQuery,
            string strStyle,
            int nStart,
            int nCount,
            out ChannelInfo[] contents,
            out string strError)
        {
            strError = "";

            contents = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetChannelInfo(
                    strQuery,
                    strStyle,
                    nStart,
                    nCount,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetChannelInfo(
                    out contents,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 管理通道
        /// <summary>
        /// 管理通道
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strAction">动作参数</param>
        /// <param name="strStyle">风格</param>
        /// <param name="requests">请求数组</param>
        /// <param name="results">返回结果数据</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:   返回总结果数量</para>
        /// </returns>
        public long ManageChannel(
    DigitalPlatform.Stop stop,
    string strAction,
    string strStyle,
    ChannelInfo[] requests,
    out ChannelInfo[] results,
    out string strError)
        {
            strError = "";

            results = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginManageChannel(
                    strAction,
                    strStyle,
                    requests,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndManageChannel(
                    out results,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 移动读者记录
        // return:
        //      -1  error
        //      0   已经成功移动
        /// <summary>
        /// 移动读者记录
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strSourceRecPath">源记录路径</param>
        /// <param name="strTargetRecPath">调用前设置目标记录路径；调用后返回实际移动到的目标记录路径</param>
        /// <param name="target_timestamp">返回目标记录的新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long MoveReaderInfo(
            DigitalPlatform.Stop stop,
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte[] target_timestamp,
            out string strError)
        {
            strError = "";
            target_timestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginMoveReaderInfo(
                    strSourceRecPath,
                    ref strTargetRecPath,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndMoveReaderInfo(
                    ref strTargetRecPath,
                    out target_timestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // DevolveReaderInfo
        /// <summary>
        /// 转移借阅信息
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strSourceReaderBarcode">源证条码号</param>
        /// <param name="strTargetReaderBarcode">目标证条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有必要转移。即源读者记录中没有需要转移的借阅信息</para>
        /// <para>1:    已经成功转移</para>
        /// </returns>
        public long DevolveReaderInfo(
            DigitalPlatform.Stop stop,
            string strSourceReaderBarcode,
            string strTargetReaderBarcode,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginDevolveReaderInfo(
                    strSourceReaderBarcode,
                    strTargetReaderBarcode,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndDevolveReaderInfo(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 修改自己的密码
        /// <summary>
        /// 修改自己的密码
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strUserName">用户名</param>
        /// <param name="strOldPassword">旧密码</param>
        /// <param name="strNewPassword">新密码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// </returns>
        public long ChangeUserPassword(
            DigitalPlatform.Stop stop,
            string strUserName,
            string strOldPassword,
            string strNewPassword,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginChangeUserPassword(
                    strUserName,
                    strOldPassword,
                    strNewPassword,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndChangeUserPassword(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 兼容以前用法
        public long VerifyBarcode(
    DigitalPlatform.Stop stop,
    string strLibraryCode,
    string strBarcode,
    out string strError)
        {
            string strOutputBarcode = "";
            return VerifyBarcode(
    stop,
    "",
    strLibraryCode,
    strBarcode,
    out strOutputBarcode,
    out strError);
        }

        // 校验条码
        /// <summary>
        /// 校验条码号
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strLibraryCode">馆代码</param>
        /// <param name="strBarcode">条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0/1/2:    分别对应“不合法的标码号”/“合法的读者证条码号”/“合法的册条码号”</para>
        /// </returns>
        public long VerifyBarcode(
            DigitalPlatform.Stop stop,
            string strAction,
            string strLibraryCode,
            string strBarcode,
            out string strOutputBarcode,
            out string strError)
        {
            strError = "";
            strOutputBarcode = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginVerifyBarcode(
                    strAction,
                    strLibraryCode,
                    strBarcode,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndVerifyBarcode(
                    out strOutputBarcode,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long ListFile(
            DigitalPlatform.Stop stop,
            string strAction,
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            out FileItemInfo[] infos,
            out string strError)
        {
            strError = "";
            infos = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginListFile(
                    strAction,
                    strCategory,
                    strFileName,
                    lStart,
                    lLength,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndListFile(
                    out infos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得系统配置文件
        // parameters:
        //      strCategory 文件分类。目前只能使用 cfgs
        //      lStart  需要获得文件内容的起点。如果为-1，表示(baContent中)不返回文件内容
        //      lLength 需要获得的从lStart开始算起的byte数。如果为-1，表示希望尽可能多地取得(但是不能保证一定到尾)
        // rights:
        //      需要 getsystemparameter 权限
        // return:
        //      result.Value    -1 错误；其他 文件的总长度
        /// <summary>
        /// 获得系统配置文件
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strCategory">文件分类</param>
        /// <param name="strFileName">文件名</param>
        /// <param name="lStart">希望返回的文件内容的起始位置</param>
        /// <param name="lLength">希望返回的文件内容的长度</param>
        /// <param name="baContent">返回文件内容</param>
        /// <param name="strFileTime">返回文件的最后修改时间。RFC1123 格式</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:   成功。值为所指定文件的长度</para>
        /// </returns>
        public long GetFile(
            DigitalPlatform.Stop stop,
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            string strStyle,
            out byte[] baContent,
            out string strFileTime,
            out string strError)
        {
            strError = "";
            strFileTime = "";
            baContent = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetFile(
                    strCategory,
                    strFileName,
                    lStart,
                    lLength,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetFile(
                    out baContent,
                    out strFileTime,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }

                // 2017/10/7
                if (StringUtil.IsInList("gzip", strStyle)
                    && result.ErrorCode == localhost.ErrorCode.Compressed
                    && baContent != null && baContent.Length > 0)
                {
                    baContent = ByteArray.DecompressGzip(baContent);
                }

                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得系统参数
        // parameters:
        //      stop    停止对象
        //      strCategory 参数所在目录
        //      strName 参数名
        //      strValue    返回参数值
        // rights:
        //      需要 getsystemparameter 权限
        // return:
        //      -1  错误
        //      0   没有得到所要求的参数值
        //      1   得到所要求的参数值
        public long GetSystemParameter(
            DigitalPlatform.Stop stop,
            string strCategory,
            string strName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetSystemParameter(
                    strCategory,
                    strName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetSystemParameter(
                    out strValue,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置系统参数
        // parameters:
        //      stop    停止对象
        //      strCategory 参数所在目录
        //      strName 参数名
        //      strValue    参数值
        // rights:
        //      需要 setsystemparameter 权限
        // return:
        //      -1  错误
        //      0   成功
        public long SetSystemParameter(
            DigitalPlatform.Stop stop,
            string strCategory,
            string strName,
            string strValue,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetSystemParameter(
                    strCategory,
                    strName,
                    strValue,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetSystemParameter(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 紧急恢复
        public long UrgentRecover(
            DigitalPlatform.Stop stop,
            string strXML,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginUrgentRecover(
                    strXML,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndUrgentRecover(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long RepairBorrowInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            int nStart,   // 2008/10/27
            int nCount,   // 2008/10/27
            out int nProcessedBorrowItems,   // 2008/10/27
            out int nTotalBorrowItems,   // 2008/10/27
            out string strOutputReaderBarcode,
            out string[] aDupPath,
            out string strError)
        {
            strError = "";
            nProcessedBorrowItems = 0;
            nTotalBorrowItems = 0;
            aDupPath = null;
            strOutputReaderBarcode = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginRepairBorrowInfo(
                    strAction,
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    nStart,
                    nCount,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndRepairBorrowInfo(
                    out nProcessedBorrowItems,
                    out nTotalBorrowItems,
                    out strOutputReaderBarcode,
                    out aDupPath,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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


        }

        // 获得书目记录信息(一次可以获得多种)
        // parameters:
        //      strBiblioRecPath    书目记录的路径
        //      formats 格式列表。可以用后列的多种格式：xml html text @??? summary
        //      results 返回的结果字符串数组
        //      baTimestamp 返回的记录时间戳
        // rights:
        //      需要 getbiblioinfo 权限
        //      如果formats中包含了"summary"格式，还需要 getbibliosummary 权限
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public long GetBiblioInfos(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            string strBiblioXml,    // 2013/3/6
            string[] formats,
            out string[] results,
            out byte[] baTimestamp,
            out string strError)
        {
            results = null;
            baTimestamp = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetBiblioInfos(
                    strBiblioRecPath,
                    strBiblioXml,
                    formats,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetBiblioInfos(
                    out results,
                    out baTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 包装后的版本，兼容以前参数
        public long SetBiblioInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            return SetBiblioInfo(
            stop,
            strAction,
            strBiblioRecPath,
            strBiblioType,
            strBiblio,
            baTimestamp,
            strComment,
            "",
            out strOutputBiblioRecPath,
            out baOutputTimestamp,
            out strError);
        }

        // 设置书目信息
        public long SetBiblioInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            string strStyle,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetBiblioInfo(
                    strAction,
                    strBiblioRecPath,
                    strBiblioType,
                    strBiblio,
                    baTimestamp,
                    strComment,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetBiblioInfo(
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 复制书目信息
        public long CopyBiblioInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strNewBiblioRecPath,
            string strNewBiblio,
            string strMergeStyle,
            out string strOutputBiblio,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;
            strOutputBiblio = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginCopyBiblioInfo(
                    strAction,
                    strBiblioRecPath,
                    strBiblioType,
                    strBiblio,
                    baTimestamp,
                    strNewBiblioRecPath,
                    strNewBiblio,
                    strMergeStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndCopyBiblioInfo(
                    out strOutputBiblio,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 入馆登记
        public long PassGate(
            DigitalPlatform.Stop stop,
            string strReaderBarcode,
            string strGateName,
            string strResultTypeList,
            out string[] results,
            out string strError)
        {
            strError = "";
            results = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginPassGate(
                    strReaderBarcode,
                    strGateName,
                    strResultTypeList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndPassGate(
                    out results,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }


        // 创建押金交费(或者退费)请求
        // parameters:
        //      strAction   值为foregift return之一
        public long Foregift(
            DigitalPlatform.Stop stop,
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID,
            out string strError)
        {
            strOutputReaderXml = "";
            strOutputID = "";

            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginForegift(
                    strAction,
                    strReaderBarcode,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndForegift(out strOutputReaderXml,
                    out strOutputID,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 创建租金交费请求
        public long Hire(
            DigitalPlatform.Stop stop,
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID,
            out string strError)
        {
            strOutputReaderXml = "";
            strOutputID = "";
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginHire(
                    strAction,
                    strReaderBarcode,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndHire(out strOutputReaderXml,
                    out strOutputID, soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 结算
        public long Settlement(
            DigitalPlatform.Stop stop,
            string strAction,
            string[] ids,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSettlement(
                    strAction,
                    ids,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSettlement(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // (根据一定排架体系)检索出某一类的同类书的索取号
        public long SearchOneClassCallNumber(
            DigitalPlatform.Stop stop,
            string strArrangeGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml,
            out string strError)
        {
            strError = "";
            strQueryXml = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchOneClassCallNumber(
                    strArrangeGroupName,
                    strClass,
                    strResultSetName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchOneClassCallNumber(
                    out strQueryXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }


        // 获得索取号检索命中信息
        public long GetCallNumberSearchResult(
            DigitalPlatform.Stop stop,
            string strArrangeGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out CallNumberSearchResult[] searchresults,
            out string strError)
        {
            strError = "";
            searchresults = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetCallNumberSearchResult(
                    strArrangeGroupName,
                    strResultSetName,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetCallNumberSearchResult(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long GetOneClassTailNumber(
    DigitalPlatform.Stop stop,
    string strArrangeGroupName,
    string strClass,
    out string strTailNumber,
    out string strError)
        {
            strError = "";
            strTailNumber = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetOneClassTailNumber(
                    strArrangeGroupName,
                    strClass,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetOneClassTailNumber(
                    out strTailNumber,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置种次号尾号
        public long SetOneClassTailNumber(
            DigitalPlatform.Stop stop,
            string strAction,
            string strArrangeGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strError = "";
            strOutputNumber = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetOneClassTailNumber(
                    strAction,
                    strArrangeGroupName,
                    strClass,
                    strTestNumber,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetOneClassTailNumber(
                    out strOutputNumber,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 检索同类书记录，返回种次号和摘要信息
        public long SearchUsedZhongcihao(
            DigitalPlatform.Stop stop,
            string strZhongcihaoGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml,
            out string strError)
        {
            strError = "";
            strQueryXml = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchUsedZhongcihao(
                    strZhongcihaoGroupName,
                    strClass,
                    strResultSetName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchUsedZhongcihao(
                    out strQueryXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得种次号检索命中信息
        public long GetZhongcihaoSearchResult(
            DigitalPlatform.Stop stop,
            string strZhongcihaoGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out ZhongcihaoSearchResult[] searchresults,
            out string strError)
        {
            strError = "";
            searchresults = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetZhongcihaoSearchResult(
                    strZhongcihaoGroupName,
                    strResultSetName, lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetZhongcihaoSearchResult(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long GetZhongcihaoTailNumber(
            DigitalPlatform.Stop stop,
            string strZhongcihaoGroupName,
            string strClass,
            out string strTailNumber,
            out string strError)
        {
            strError = "";
            strTailNumber = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetZhongcihaoTailNumber(
                    strZhongcihaoGroupName,
                    strClass,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetZhongcihaoTailNumber(
                    out strTailNumber,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置种次号尾号
        public long SetZhongcihaoTailNumber(
            DigitalPlatform.Stop stop,
            string strAction,
            string strZhongcihaoGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strError = "";
            strOutputNumber = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetZhongcihaoTailNumber(
                    strAction,
                    strZhongcihaoGroupName,
                    strClass,
                    strTestNumber,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetZhongcihaoTailNumber(
                    out strOutputNumber,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 查重
        // parameters:
        //      strUsedProjectName  实际使用的查重方案名
        public long SearchDup(
            DigitalPlatform.Stop stop,
            string strOriginBiblioRecPath,
            string strOriginBiblioRecXml,
            string strProjectName,
            string strStyle,
            out string strUsedProjectName,
            out string strError)
        {
            strError = "";
            strUsedProjectName = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchDup(
                    strOriginBiblioRecPath,
                    strOriginBiblioRecXml,
                    strProjectName,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchDup(
                    out strUsedProjectName,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        // 列出查重方案信息
        public long ListDupProjectInfos(
            DigitalPlatform.Stop stop,
            string strOriginBiblioDbName,
            out DupProjectInfo[] results,
            out string strError)
        {
            strError = "";
            results = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginListDupProjectInfos(
                    strOriginBiblioDbName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndListDupProjectInfos(
                    out results,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获得查重检索命中结果
        public long GetDupSearchResult(
            DigitalPlatform.Stop stop,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            out DupSearchResult[] searchresults,
            out string strError)
        {
            strError = "";
            searchresults = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetDupSearchResult(
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetDupSearchResult(
                    out searchresults,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long GetUtilInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strDbName,
            string strFrom,
            string strKey,
            string strValueAttrName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetUtilInfo(
                    strAction,
                    strDbName,
                    strFrom,
                    strKey,
                    strValueAttrName,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetUtilInfo(
                    out strValue,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long SetUtilInfo(
            DigitalPlatform.Stop stop,
            string strAction,
            string strDbName,
            string strFrom,
            string strRootElementName,
            string strKeyAttrName,
            string strValueAttrName,
            string strKey,
            string strValue,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetUtilInfo(
                    strAction,
                    strDbName,
                    strFrom,
                    strRootElementName,
                    strKeyAttrName,
                    strValueAttrName,
                    strKey,
                    strValue,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetUtilInfo(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 获取资源
        // parameters:
        //      strResPath  资源的路径。一般数据库记录为"数据库名/1"形态；而数据库记录所属的对象资源，则为"数据库名/object/0"形态
        //      nStart  本次要获得的byte开始位置
        //      nLength 本次要获得的byte个数
        //      strStyle    风格列表，为逗号分隔的字符串值列表。取值为data/metadata/timestamp/outputpath之一
        //                  data表示要在baContent参数内返回资源本体内容
        //                  metadata表示要在strMetadata参数内返回元数据内容
        //                  timestamp表示要在baOutputTimestam参数内返回资源的时间戳内容
        //                  outputpath表示要在strOutputResPath参数内返回实际记录路径内容
        //      baContent   返回的byte数组
        //      strMetadata 返回的元数据内容
        //      strOutputResPath    返回的实际记录路径
        //      baOutputTimestamp   返回的资源时间戳
        // rights:
        //      需要 getres 权限
        // return:
        //      -1  出错
        //      0   成功
        public long GetRes(
            DigitalPlatform.Stop stop,
            string strResPath,
            long lStart,
            int nLength,
            string strStyle,
            out byte[] baContent,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baContent = null;
            strMetadata = "";
            strOutputResPath = "";
            baOutputTimestamp = null;

            /*
            if (stop != null)
            {
                Debug.Assert(stop.State == -1 || stop.State == 0, "");
            }*/

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetRes(
                    strResPath,
                    lStart,
                    nLength,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetRes(
                    out baContent,
                    out strMetadata,
                    out strOutputResPath,
                    out baOutputTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }

                // 2017/10/7
                if (StringUtil.IsInList("gzip", strStyle)
                    && result.ErrorCode == localhost.ErrorCode.Compressed
                    && baContent != null && baContent.Length > 0)
                {
                    baContent = ByteArray.DecompressGzip(baContent);
                }

                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public const string GETRES_ALL_STYLE = "content,data,metadata,timestamp,outputpath";

        // 获得资源。包装版本 -- 返回字符串版本。
        // return:
        //		strStyle	一般设置为"content,data,metadata,timestamp,outputpath";
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            DigitalPlatform.Stop stop,
            string strPath,
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

            byte[] baContent = null;

            int nStart = 0;
            int nPerLength = -1;

            byte[] baTotal = null;

            for (; ; )
            {
                if (stop != null)
                {
                    DoIdle();
                    // Application.DoEvents();	// 出让界面控制权
                }

                long lRet = this.GetRes(stop,
                        strPath,
                        nStart,
                        nPerLength,
                        strStyle,
                        out baContent,
                        out strMetaData,
                        out strOutputResPath,
                        out baOutputTimeStamp,
                        out strError);
                if (lRet == -1)
                    return -1;

                if (StringUtil.IsInList("data", strStyle) != true)
                    break;

                // 2011/1/22
                if (StringUtil.IsInList("content", strStyle) == false)
                    return lRet;

                baTotal = ByteArray.Add(baTotal, baContent);

                Debug.Assert(baContent != null, "");
                Debug.Assert(baContent.Length <= (int)lRet, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(lRet) + "]");

                nStart += baContent.Length;
                if (nStart >= (int)lRet)
                    break;	// 结束
            } // end of for

            if (StringUtil.IsInList("data", strStyle) != true)
                return 0;


            // 转换成字符串
            strResult = ByteArray.ToString(baTotal/*,
				Encoding.UTF8*/
                               );	// 将来做自动识别编码方式

            return 0;   // TODO: return lRet?
        }

#if NNNNNNNNO
        // 获得资源。包装版本 -- 返回字符串版本、Cache版本。
        // parameters:
        //      remote_timestamp    远端时间戳。如果为 null，表示要从服务器实际获取时间戳
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            byte[] remote_timestamp,
            out string strResult,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strError = "";
            strResult = "";
            strMetaData = "";
            baOutputTimeStamp = null;
            strOutputResPath = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            long lRet = 0;

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

            if (remote_timestamp == null)
            {
                // 探测时间戳关系
                string strNewStyle = strStyle;

                StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
                    true,
                    ref strNewStyle);	// 不要数据体和metadata

                lRet = GetRes(stop,
                    strPath,
                    strNewStyle,
                    out strResult,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputResPath,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            else
                baOutputTimeStamp = remote_timestamp;

            // 如果证明timestamp没有变化, 但是本次并未返回内容,则从cache中取原来的内容

            if (ByteArray.Compare(baOutputTimeStamp, cached_timestamp) == 0)	// 时间戳相等
            {
                Debug.Assert(strLocalName != "", "strLocalName不应为空");

                try
                {
                    using (StreamReader sr = new StreamReader(strLocalName, Encoding.UTF8))
                    {
                        strResult = sr.ReadToEnd();
                        return 0;	// 以无错误姿态返回
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

        GETDATA:

            // 重新正式获取内容
            lRet = GetRes(
                stop,
                strPath,
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
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            return lRet;
        }

        // 获得资源。包装版本 -- 返回本地映射文件、Cache版本。
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetResLocalFile(
            DigitalPlatform.Stop stop,
            CfgCache cache,
            string strPath,
            string strStyle,
            out string strOutputFilename,
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputResPath,
            out string strError)
        {
            strOutputFilename = "";

            byte[] cached_timestamp = null;
            string strTimeStamp;
            string strLocalName;
            string strResult = "";

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
                strOutputResPath = "";
                strMetaData = "";
                baOutputTimeStamp = null;
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
            StringUtil.RemoveFromInList("data",
                true,
                ref strNewStyle);	// 不要数据体
             * */
            StringUtil.RemoveFromInList("content,data,metadata",    // 2012/12/31 BUG 以前忘记了加入content
    true,
    ref strNewStyle);	// 不要数据体和metadata

            long lRet = GetRes(stop,
                strPath,
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

                strOutputFilename = strLocalName;
                return 0;	// 以无错误姿态返回
            }

        GETDATA:

            // 重新正式获取内容
            lRet = GetRes(
                stop,
                strPath,
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
            using (StreamWriter sw = new StreamWriter(strLocalName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strResult);
            }

            Debug.Assert(baOutputTimeStamp != null, "下层GetRes()返回的baOutputTimeStamp为空");
            nRet = cache.SetTimeStamp(strFullPath,
                ByteArray.GetHexTimeStampString(baOutputTimeStamp),
                out strError);
            if (nRet == -1)
                return -1;

            strOutputFilename = strLocalName;

            return lRet;
        }
#endif

        // TODO: timestamp 只需要获得第一次的，和最后一次的，然后比较即可。中间次数不需要获得它。这样可以提高下载速度
        // 获得资源。包装版本 -- 写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
        // parameters:
        //		fileTarget	文件。注意在调用函数前适当设置文件指针位置。函数只会在当前位置开始向后写，写入前不会主动改变文件指针。
        //		strStyleParam	一般设置为"content,data,metadata,timestamp,outputpath";
        //		input_timestamp	若!=null，则本函数会把第一个返回的timestamp和本参数内容比较，如果不相等，则报错
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            DigitalPlatform.Stop stop,
            string strPath,
            Stream fileTarget,
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
                // Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                // REDO:

                string strMessage = "";

                string strPercent = "";
                if (lTotalLength != -1)
                {
                    double ratio = (double)lStart / (double)lTotalLength;
                    strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                }

                if (stop != null)
                {
                    strMessage = "正在下载 " + StringUtil.GetLengthText(lStart) + " / "
                        + (lTotalLength == -1 ? "?" : StringUtil.GetLengthText(lTotalLength))
                        + " " + strPercent + " "
                        + strPath;
                    stop.SetMessage(strMessage);
                }

                long lRet = this.GetRes(stop,
                    strPath,
                    fileTarget == null ? 0 : lStart,
                    fileTarget == null ? 0 : nPerLength,
                    strStyle,
                    out baContent,
                    // out id,
                    out strMetaData,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (bHasMetadataStyle == true)
                {
                    StringUtil.RemoveFromInList("metadata",
                        true,
                        ref strStyle);
                    bHasMetadataStyle = false;
                }

                lTotalLength = lRet;

                // TODO: 似乎也没有必要每轮都检查 timestamp。首尾各获得一次并对比即可
                if (StringUtil.IsInList("timestamp", strStyle) == true)
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
                }
                else
                {
                    Debug.Assert(StringUtil.IsInList("content", strStyle) == true,
                        "不是attachment风格，就应是content风格");

                    Debug.Assert(baContent != null, "返回的baContent不能为null");
                    Debug.Assert(baContent.Length <= lRet, "每次返回的包尺寸[" + Convert.ToString(baContent.Length) + "]应当小于result.Value[" + Convert.ToString(lRet) + "]");

                    fileTarget.Write(baContent, 0, baContent.Length);
                    fileTarget.Flush(); // 2013/5/17
                    lStart += baContent.Length;

                    if (lRet > 0)
                    {
                        // 2012/8/26
                        Debug.Assert(baContent.Length > 0, "");
                    }
                }

                if (lStart >= lRet)
                    break;	// 结束

            } // end of for

            baOutputTimeStamp = timestamp;
            return 0;
        }

        // 获得资源。包装版本 -- 写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
        // parameters:
        //		strOutputFileName	输出文件名。可以为null。如果调用前文件已经存在, 会被覆盖。
        //      strStyle    风格。如果为空，函数会自动设置一个默认值
        // return:
        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
        //		0	成功
        public long GetRes(
            DigitalPlatform.Stop stop,
            string strPath,
            string strOutputFileName,
            string strStyle,    // 2017/10/7
            out string strMetaData,
            out byte[] baOutputTimeStamp,
            out string strOutputPath,
            out string strError)
        {
            FileStream fileTarget = null;

            if (string.IsNullOrEmpty(strStyle))
                strStyle = "content,data,metadata,timestamp,outputpath";

            if (String.IsNullOrEmpty(strOutputFileName) == false)
                fileTarget = File.Create(strOutputFileName);
            else
            {
                strStyle = "metadata,timestamp,outputpath";
            }

            try
            {
                return GetRes(
                    stop,
                    strPath,
                    fileTarget,
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

        // 写入资源
        public long WriteRes(
            DigitalPlatform.Stop stop,
            string strResPath,
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
            strError = "";
            strOutputResPath = "";
            baOutputTimestamp = null;

            // 2017/10/7
            if (StringUtil.IsInList("gzip", strStyle)
                && baContent != null && baContent.Length > 0)
            {
                baContent = ByteArray.CompressGzip(baContent);
            }

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginWriteRes(
                    strResPath,
                    strRanges,
                    lTotalLength,
                    baContent,
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndWriteRes(
                    out strOutputResPath,
                    out baOutputTimestamp,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 保存Xml记录。包装版本。用于保存文本类型的资源。
        public long WriteRes(
            DigitalPlatform.Stop stop,
            string strPath,
            string strXml,
            bool bInlucdePreamble,
            string strStyle,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strOutputPath,
            out string strError)
        {
            strError = "";
            strOutputPath = "";
            output_timestamp = null;

            int nChunkMaxLength = 4096;	// chunk

            int nStart = 0;

            byte[] baInputTimeStamp = null;

            byte[] baPreamble = Encoding.UTF8.GetPreamble();

            byte[] baTotal = Encoding.UTF8.GetBytes(strXml);

            if (bInlucdePreamble == true
                && baPreamble != null && baPreamble.Length > 0)
            {
                byte[] temp = null;
                temp = ByteArray.Add(temp, baPreamble);
                baTotal = ByteArray.Add(temp, baTotal);
            }

            int nTotalLength = baTotal.Length;

            if (timestamp != null)
            {
                baInputTimeStamp = ByteArray.Add(baInputTimeStamp, timestamp);
            }

            for (; ; )
            {
                // Application.DoEvents();	// 出让界面控制权
                DoIdle();

                // Debug.Assert(false, "");

                // 切出chunk
                int nThisChunkSize = nChunkMaxLength;

                if (nThisChunkSize + nStart > nTotalLength)
                {
                    nThisChunkSize = nTotalLength - nStart;	// 最后一次
                    if (nThisChunkSize <= 0)
                        break;
                }

                byte[] baChunk = new byte[nThisChunkSize];
                Array.Copy(baTotal, nStart, baChunk, 0, baChunk.Length);

                string strMetadata = "";
                string strRange = Convert.ToString(nStart) + "-" + Convert.ToString(nStart + baChunk.Length - 1);
                long lRet = this.WriteRes(stop,
                    strPath,
                    strRange,
                    nTotalLength,
                    baChunk,
                    strMetadata,
                    strStyle,
                    baInputTimeStamp,
                    out strOutputPath,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                    return -1;
                /*
            REDOSAVE:
                soapresult = this.ws.BeginWriteRes(strPath,
                    strRange,
                    //nStart,
                    nTotalLength,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                    baChunk,
                    null,	// attachmentid
                    strMetadata,
                    strStyle,
                    baInputTimeStamp,
                    null,
                    null);

                for (; ; )
                {
                    Idle(); // 出让控制权，避免CPU资源耗费过度
                    if (soapresult.IsCompleted)
                        break;
                }

                Result result = this.ws.EndWriteRes(soapresult,
                    out strOutputPath,
                    out output_timestamp);

                this.ErrorInfo = result.ErrorString;	// 无论是否返回错误，都将result的ErrorString放到Channel中
                 * */

                nStart += baChunk.Length;

                if (nStart >= nTotalLength)
                    break;

                Debug.Assert(strOutputPath != "", "outputpath不能为空");

                strPath = strOutputPath;	// 如果第一次的strPath中包含'?'id, 必须用outputpath才能正确继续
                baInputTimeStamp = output_timestamp;	//baOutputTimeStamp;
            } // end of for
            return 0;
        }

        // 2016/9/26 增加 if xxx != null 判断
        public static string BuildMetadata(string strMime,
            string strLocalPath)
        {
            // string strMetadata = "<file mimetype='" + strMime + "' localpath='" + strLocalPath + "'/>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<file />");
            if (strMime != null)
                dom.DocumentElement.SetAttribute(
                    "mimetype",
                    strMime);
            if (strLocalPath != null)
                dom.DocumentElement.SetAttribute(
                    "localpath",
                    strLocalPath);
            return dom.DocumentElement.OuterXml;
        }

        // 2017/9/15
        // 保存资源记录
        // parameters:
        //		strPath	格式: 库名/记录号/object/对象xpath
        //      file    文件 Stream。注意：本函数调用中，文件指针会被自然改变
        //      length  要保存部分的长度。如果为 -1，在函数内会用 file.Length 代替
        public long SaveResObject(
            DigitalPlatform.Stop stop,
            string strPath,
            Stream file,  // 该参数代表存放对象数据的本地文件
            long length,
#if NO
            string strLocalPath,       // 该参数为拟放入 metadata 中的本地文件名
            string strMime,
#endif
 string strMetadata,
            string strRange,
            byte[] timestamp,
            string strStyle,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;
            long lRet = 0;

            if (length == -1)
                length = file.Length;

            byte[] baTotal = null;
            if (file != null)
            {
                lRet = RangeList.CopyFragmentNew(
                    file,
                    length,
                    strRange,
                    out baTotal,
                    out strError);
                if (lRet == -1)
                    return -1;
            }


            // string strMetadata = BuildMetadata(strMime, strLocalPath);

            // 写入资源
            lRet = WriteRes(
                stop,
                strPath,
                strRange,
                length,	// 这是整个对象文件尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                baTotal,
                strMetadata,
                strStyle,
                timestamp,
                out string strOutputResPath,
                out output_timestamp,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // TODO: 需要淘汰本函数，改用 FileStream 的版本
        // 注：本函数的缺点是，在反复调用的过程中，要多次打开文件和移动文件指针，效率不高
        // 2014/3/6
        // 保存资源记录
        // parameters:
        //		strPath	格式: 库名/记录号/object/对象xpath
        public long SaveResObject(
            DigitalPlatform.Stop stop,
            string strPath,
            string strObjectFileName,  // 该参数代表存放对象数据的文件名
            string strLocalPath,       // 该参数代表本地文件名,有时会与strObjectFileName不同
            string strMime,
            string strRange,
            byte[] timestamp,
            string strStyle,
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

            // 
            string strOutputResPath = "";

            string strMetadata = BuildMetadata(strMime, strLocalPath);
            // string strMetadata = "<file mimetype='" + strMime + "' localpath='" + strLocalPath + "'/>";

            // 写入资源
            lRet = WriteRes(
                stop,
                strPath,
                strRange,
                fi.Length,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                baTotal,
                strMetadata,
                strStyle,
                timestamp,
                out strOutputResPath,
                out output_timestamp,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // 包装后的版本。少了一个 strStyle 参数
        public long SaveResObject(
DigitalPlatform.Stop stop,
string strPath,
string strObjectFileName,  // 该参数代表存放对象数据的文件名
string strMetadata,
string strRange,
bool bTailHint,
byte[] timestamp,
// string strStyle,
out byte[] output_timestamp,
out string strError)
        {
            return SaveResObject(
        stop,
        strPath,
        strObjectFileName,  // 该参数代表存放对象数据的文件名
        strMetadata,
        strRange,
        bTailHint,
        timestamp,
        "",
        out output_timestamp,
        out strError);
        }

        // 2016/10/16
        public long SaveResObject(
    DigitalPlatform.Stop stop,
    string strPath,
    string strObjectFileName,  // 该参数代表存放对象数据的文件名
    string strMetadata,
    string strRange,
    bool bTailHint,
    byte[] timestamp,
            string strStyle,
    out byte[] output_timestamp,
    out string strError)
        {
            strError = "";
            output_timestamp = null;
            long lRet = 0;

            byte[] baTotal = null;
            long lTotalLength = 0;
            if (string.IsNullOrEmpty(strObjectFileName) == false)
            {
                FileInfo fi = new FileInfo(strObjectFileName);
                if (fi.Exists == false)
                {
                    strError = "文件 '" + strObjectFileName + "'不存在...";
                    return -1;
                }

                lRet = RangeList.CopyFragment(
                    strObjectFileName,
                    strRange,
                    out baTotal,
                    out strError);
                if (lRet == -1)
                    return -1;

                lTotalLength = fi.Length;
            }
            else
                lTotalLength = -1;

            // string strOutputPath = "";

            //int nOldTimeout = -1;
            if (bTailHint == true)
            {
                /*
                nOldTimeout = this.Timeout;
                // TODO: 建议通过文件尺寸来估算
                this.Timeout = 40 * 60 * 1000;  // 40分钟
                 * */
            }

            // 
            string strOutputResPath = "";

            // 写入资源
            lRet = WriteRes(
                stop,
                strPath,
                strRange,
                lTotalLength,   // fi.Length,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                baTotal,
                strMetadata,
                strStyle,
                timestamp,
                out strOutputResPath,
                out output_timestamp,
                out strError);
            if (lRet == -1)
                return -1;

            if (bTailHint == true)
            {
                /*
                this.Timeout = nOldTimeout;
                 * */
            }

            return 0;
        }

        // 保存资源记录
        // parameters:
        //		strPath	格式: 库名/记录号/object/对象xpath
        //      strObjectFileName   对象文件名。如果为空，表示本次调用仅根据 strLocalPath 和 strMime 修改 对象的 metadata 部分
        //		bTailHint	是否为最后一次写入操作。这是一个暗示参数，本函数将根据此参数为最后一次写入操作设置特殊的超时时间。
        //					假定有时整个资源尺寸很大，虽然每次局部写入耗时不多，但是最后一次写入因为服务器要执行整个资源转存
        //					的操作后API才返回，所以可能会耗费类似20分钟这样的长时间，导致WebService API超时失败。
        //					本参数是一个暗示操作(本函数也不担保一定要做什么操作)，如果调用者不清楚它的含义，可以使用false。
        public long SaveResObject(
            DigitalPlatform.Stop stop,
            string strPath,
            string strObjectFileName,  // 该参数代表存放对象数据的文件名
            string strLocalPath,       // 该参数代表本地文件名,有时会与strObjectFileName不同
            string strMime,
            string strRange,
            bool bTailHint,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;
            long lRet = 0;

            byte[] baTotal = null;
            long lTotalLength = 0;
            if (string.IsNullOrEmpty(strObjectFileName) == false)
            {
                FileInfo fi = new FileInfo(strObjectFileName);
                if (fi.Exists == false)
                {
                    strError = "文件 '" + strObjectFileName + "'不存在...";
                    return -1;
                }

                lRet = RangeList.CopyFragment(
                    strObjectFileName,
                    strRange,
                    out baTotal,
                    out strError);
                if (lRet == -1)
                    return -1;

                lTotalLength = fi.Length;
            }
            else
                lTotalLength = -1;

            // string strOutputPath = "";

            //int nOldTimeout = -1;
            if (bTailHint == true)
            {
                /*
                nOldTimeout = this.Timeout;
                // TODO: 建议通过文件尺寸来估算
                this.Timeout = 40 * 60 * 1000;  // 40分钟
                 * */
            }

            // 
            string strOutputResPath = "";

            string strMetadata = BuildMetadata(strMime, strLocalPath);
            // string strMetadata = "<file mimetype='" + strMime + "' localpath='" + strLocalPath + "'/>";

            // 写入资源
            lRet = WriteRes(
                stop,
                strPath,
                strRange,
                lTotalLength,   // fi.Length,	// 这是整个包尺寸，不是本次chunk的尺寸。因为服务器显然可以从baChunk中看出其尺寸，不必再专门用一个参数表示这个尺寸了
                baTotal,
                strMetadata,
                "", // strStyle,
                timestamp,
                out strOutputResPath,
                out output_timestamp,
                out strError);
            if (lRet == -1)
                return -1;

            if (bTailHint == true)
            {
                /*
                this.Timeout = nOldTimeout;
                 * */
            }

            return 0;
        }

        /// *** 期相关功能

        // 获得期信息
        // return:
        //      -1  出错
        //      0   正常
        public long GetIssues(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] issueinfos,
            out string strError)
        {
            issueinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetIssues(
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetIssues(
                    out issueinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置期信息
        // return:
        //      -1  出错
        //      0   正常
        public long SetIssues(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            EntityInfo[] issueinfos,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            errorinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetIssues(
                    strBiblioRecPath,
                    issueinfos,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetIssues(
                    out errorinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // *** 此API已经废止 ***
#if NO
        // (期库)出版日期查重
        public long SearchIssueDup(
            DigitalPlatform.Stop stop,
            string strPublishTime,
            string strBiblioRecPath,
            int nMax,
            out string[] paths,
            out string strError)
        {
            paths = null;
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchIssueDup(
                    strPublishTime,
                    strBiblioRecPath,
                    nMax,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchIssueDup(
                    out paths,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
#endif

        // 包装后的版本
        public long GetIssueInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strResultType,
    out string strResult,
    out string strIssueRecPath,
    out byte[] issue_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetIssueInfo(
            stop,
            strRefID,
            "",
            strResultType,
            out strResult,
            out strIssueRecPath,
            out issue_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }

#if NEW_API
        // 新用法
        // 调用 GetItemInfo 来实现
        public long GetIssueInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strItemXml,
    string strResultType,
    out string strResult,
    out string strIssueRecPath,
    out byte[] issue_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetItemInfo(
                stop,
                "issue",
                strRefID,
                strItemXml,
                strResultType,
            out strResult,
            out strIssueRecPath,
            out issue_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }
#else
        // 故意不用这个 API 了
        // 获得期记录
        public long GetIssueInfo(
            DigitalPlatform.Stop stop,
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
            string strResultType,
            out string strResult,
            out string strIssueRecPath,
            out byte[] issue_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath,
            out string strError)
        {
            strResult = "";
            strBiblio = "";
            strOutputBiblioRecPath = "";
            strError = "";

            strIssueRecPath = "";
            issue_timestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetIssueInfo(
                    strRefID,
                    // strBiblioRecPath,
                    strItemXml,
                    strResultType,
                    strBiblioType,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetIssueInfo(
                    out strResult,
                    out strIssueRecPath,
                    out issue_timestamp,
                    out strBiblio,
                    out strOutputBiblioRecPath,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }
#endif

        // 2009/2/2
        // 检索期信息
        // parameters:
        //      stop    停止对象
        //      strIssueDbNames  期库名的列表。可以包含多个库名，库名之间用逗号(半角)分隔。<全部> <all>表示全部期库
        //      strQueryWord    检索词
        //      nPerMax 一批检索命中的最大记录数。-1表示不限制。
        //      strFrom 检索途径
        //      strMatchStyle   匹配方式。值为left/right/exact/middle之一。
        //      strLang 界面语言代码。一般为"zh"。
        //      strResultSetName    结果集名。可使用null。而指定有区分的结果集名，可让两批以上不同目的的检索结果及互相不冲突。
        // 权限: 
        //      需要 searchissue 权限
        // return:
        //      -1  error
        //      >=0 命中结果记录总数
        // 注：
        //      实体库的数据格式都是统一的，检索途径可以穷举为：册条码号/批次号/登录号
        public long SearchIssue(
            DigitalPlatform.Stop stop,
            string strIssueDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchIssue(
                    strIssueDbNames,
                    strQueryWord,
                    nPerMax,
                    strFrom,
                    strMatchStyle,
                    strLang,
                    strResultSetName,
                    strSearchStyle,
                    strOutputStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchIssue(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        //  *** 订购相关功能

        // 获得订购信息
        // return:
        //      -1  出错
        //      0   正常
        public long GetOrders(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
            out EntityInfo[] orderinfos,
            out string strError)
        {
            orderinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetOrders(
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetOrders(
                    out orderinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }


        // 设置订购信息
        // return:
        //      -1  出错
        //      0   正常
        public long SetOrders(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            EntityInfo[] orderinfos,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            errorinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetOrders(
                    strBiblioRecPath,
                    orderinfos,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetOrders(
                    out errorinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // *** 此API已经废止 ***
#if NO
        // (订购库)编号查重
        public long SearchOrderDup(
            DigitalPlatform.Stop stop,
            string strIndex,
            string strBiblioRecPath,
            int nMax,
            out string[] paths,
            out string strError)
        {
            paths = null;
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchOrderDup(
                    strIndex,
                    strBiblioRecPath,
                    nMax,
                    null,
                    null);

                        WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchOrderDup(
                    out paths,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
#endif

        // 包装后的版本
        public long GetOrderInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strResultType,
    out string strResult,
    out string strItemRecPath,
    out byte[] item_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetOrderInfo(
            stop,
            strRefID,
            "",
            strResultType,
            out strResult,
            out strItemRecPath,
            out item_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }

#if NEW_API
        // 新用法
        // 调用 GetItemInfo 来实现
        public long GetOrderInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strItemXml,
    string strResultType,
    out string strResult,
    out string strItemRecPath,
    out byte[] item_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetItemInfo(
                stop,
                "order",
                strRefID,
                strItemXml,
                strResultType,
            out strResult,
            out strItemRecPath,
            out item_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }
#else
        // 故意不用这个 API 了
        // 获得订购记录
        // parameters:
        //      strIndex  编号。特殊情况下，可以使用"@path:"引导的订购记录路径(只需要库名和id两个部分)作为检索入口。
        //      strBiblioRecPath    指定书目记录路径
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        //      strOutputBiblioRecPath  输出的书目记录路径。当strIndex的第一字符为'@'时，strBiblioRecPath必须为空，函数返回后，strOutputBiblioRecPath中会包含从属的书目记录路径
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        //      >1  命中多于1条
        public long GetOrderInfo(
            DigitalPlatform.Stop stop,
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath,
            out string strError)
        {
            strResult = "";
            strBiblio = "";
            strOutputBiblioRecPath = "";
            strError = "";

            strItemRecPath = "";
            item_timestamp = null;
            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetOrderInfo(
                    strRefID,
                    // strBiblioRecPath,
                    strItemXml,
                    strResultType,
                    strBiblioType,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetOrderInfo(
                    out strResult,
                    out strItemRecPath,
                    out item_timestamp,
                    out strBiblio,
                    out strOutputBiblioRecPath,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }
#endif


        // 检索订购信息
        // parameters:
        //      stop    停止对象
        //      strOrderDbNames  订购库名的列表。可以包含多个库名，库名之间用逗号(半角)分隔。<全部> <all>表示全部订购库(包括图书和期刊的)，<全部图书> <all book>为全部图书类型的订购库，<全部期刊> <all series>为全部期刊类型的订购库
        //      strQueryWord    检索词
        //      nPerMax 一批检索命中的最大记录数。-1表示不限制。
        //      strFrom 检索途径
        //      strMatchStyle   匹配方式。值为left/right/exact/middle之一。
        //      strLang 界面语言代码。一般为"zh"。
        //      strResultSetName    结果集名。可使用null。而指定有区分的结果集名，可让两批以上不同目的的检索结果及互相不冲突。
        // 权限: 
        //      需要 searchorder 权限
        // return:
        //      -1  error
        //      >=0 命中结果记录总数
        // 注：
        //      实体库的数据格式都是统一的，检索途径可以穷举为：册条码号/批次号/登录号
        public long SearchOrder(
            DigitalPlatform.Stop stop,
            string strOrderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchOrder(
                    strOrderDbNames,
                    strQueryWord,
                    nPerMax,
                    strFrom,
                    strMatchStyle,
                    strLang,
                    strResultSetName,
                    strSearchStyle,
                    strOutputStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchOrder(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        //  *** 评注相关功能

        // 获得评注信息
        // return:
        //      -1  出错
        //      0   正常
        public long GetComments(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
            out EntityInfo[] commentinfos,
            out string strError)
        {
            commentinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetComments(
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetComments(
                    out commentinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 设置评注信息
        // return:
        //      -1  出错
        //      0   正常
        public long SetComments(
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            EntityInfo[] commentinfos,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            errorinfos = null;
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetComments(
                    strBiblioRecPath,
                    commentinfos,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetComments(
                    out errorinfos,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 包装后的版本
        public long GetCommentInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strResultType,
    out string strResult,
    out string strCommentRecPath,
    out byte[] comment_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetCommentInfo(
            stop,
            strRefID,
            "",
            strResultType,
            out strResult,
            out strCommentRecPath,
            out comment_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }

#if NEW_API
        // 新用法
        // 调用 GetItemInfo 来实现
        public long GetCommentInfo(
    DigitalPlatform.Stop stop,
    string strRefID,
    string strItemXml,
    string strResultType,
    out string strResult,
    out string strCommentRecPath,
    out byte[] comment_timestamp,
    string strBiblioType,
    out string strBiblio,
    out string strOutputBiblioRecPath,
    out string strError)
        {
            return GetItemInfo(
                stop,
                "comment",
                strRefID,
                strItemXml,
                strResultType,
            out strResult,
            out strCommentRecPath,
            out comment_timestamp,
            strBiblioType,
            out strBiblio,
            out strOutputBiblioRecPath,
            out strError);
        }
#else
        // 故意不用这个 API 了
        // 获得评注记录
        // parameters:
        //      strIndex  编号。特殊情况下，可以使用"@path:"引导的订购记录路径(只需要库名和id两个部分)作为检索入口。
        //      strBiblioRecPath    指定书目记录路径
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        //      strOutputBiblioRecPath  输出的书目记录路径。当strIndex的第一字符为'@'时，strBiblioRecPath必须为空，函数返回后，strOutputBiblioRecPath中会包含从属的书目记录路径
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        //      >1  命中多于1条
        public long GetCommentInfo(
            DigitalPlatform.Stop stop,
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
            string strResultType,
            out string strResult,
            out string strCommentRecPath,
            out byte[] comment_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath,
            out string strError)
        {
            strResult = "";
            strBiblio = "";
            strOutputBiblioRecPath = "";
            strError = "";

            strCommentRecPath = "";
            comment_timestamp = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetCommentInfo(strRefID,
                    // strBiblioRecPath,
                    strItemXml,
                    strResultType,
                    strBiblioType,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetCommentInfo(
                    out strResult,
                    out strCommentRecPath,
                    out comment_timestamp,
                    out strBiblio,
                    out strOutputBiblioRecPath,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }
#endif

        // *** 此API已经废止 ***
#if NO
        // (评注库)编号查重
        public long SearchCommentDup(
            DigitalPlatform.Stop stop,
            string strIndex,
            string strBiblioRecPath,
            int nMax,
            out string[] paths,
            out string strError)
        {
            paths = null;
            strError = "";

        REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchCommentDup(strIndex,
                    strBiblioRecPath,
                    nMax,
                    null,
                    null);

                        WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchCommentDup(
                    out paths,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
#endif

        // 2011/1/21
        // 预约
        // parameters:
        //      strItemBarcodeList  册条码号列表，逗号间隔
        // 权限：需要有reservation权限
        public long Reservation(
            DigitalPlatform.Stop stop,
            string strFunction,
            string strReaderBarcode,
            string strItemBarcodeList,
            out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginReservation(
                    strFunction,
                    strReaderBarcode,
                    strItemBarcodeList,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndReservation(
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // 检索评注信息
        // parameters:
        //      stop    停止对象
        //      strCommentDbName  评注库名的列表。可以包含多个库名，库名之间用逗号(半角)分隔。<全部> <all>表示全部评注库(包括图书和期刊的)
        //      strQueryWord    检索词
        //      nPerMax 一批检索命中的最大记录数。-1表示不限制。
        //      strFrom 检索途径
        //      strMatchStyle   匹配方式。值为left/right/exact/middle之一。
        //      strLang 界面语言代码。一般为"zh"。
        //      strResultSetName    结果集名。可使用null。而指定有区分的结果集名，可让两批以上不同目的的检索结果及互相不冲突。
        // 权限: 
        //      需要 searchorder 权限
        // return:
        //      -1  error
        //      >=0 命中结果记录总数
        public long SearchComment(
            DigitalPlatform.Stop stop,
            string strCommentDbName,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchComment(
                    strCommentDbName,
                    strQueryWord,
                    nPerMax,
                    strFrom,
                    strMatchStyle,
                    strLang,
                    strResultSetName,
                    strSearchStyle,
                    strOutputStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchComment(soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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


        public long GetMessage(
            string[] message_ids,
            MessageLevel messagelevel,
            out MessageData[] messages,
            out string strError)
        {
            strError = "";
            messages = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetMessage(
                    message_ids,
                    messagelevel,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetMessage(
                    out messages,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long ListMessage(
            string strStyle,
            string strResultsetName,
            string strBoxType,
            MessageLevel messagelevel,
            int nStart,
            int nCount,
            out int nTotalCount,
            out MessageData[] messages,
            out string strError)
        {
            strError = "";
            messages = null;
            nTotalCount = 0;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginListMessage(
                    strStyle,
            strResultsetName,
            strBoxType,
            messagelevel,
            nStart,
            nCount,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndListMessage(
                    out nTotalCount,
                    out messages,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        // return:
        //      -1  出错
        //      >=0 未读过的消息条数
        public int GetUntouchedMessageCount(string strBoxType)
        {
            string strError = "";
            int nTotalCount = 0;
            MessageData[] messages = null;
            long lRet = ListMessage(
                "search,untouched",
                "message_untouched",
                strBoxType,
                MessageLevel.ID,
                0,
                0,
                out nTotalCount,
                out messages,
                out strError);
            if (lRet == -1)
                return -1;
            return nTotalCount;
        }


        public long SetMessage(string strAction,
            string strStyle,
            MessageData[] messages,
            out MessageData[] output_messages,
    out string strError)
        {
            strError = "";
            output_messages = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetMessage(
                    strAction,
                    strStyle,
                    messages,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetMessage(
                    out output_messages,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }


        public long GetStatisInfo(string strDateRangeString,
            string strStyle,
            out RangeStatisInfo info,
            out string strXml,
            out string strError)
        {
            strError = "";
            info = null;
            strXml = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetStatisInfo(
                    strDateRangeString,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetStatisInfo(
                    out info,
                    out strXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long ExistStatisInfo(string strDateRangeString,
            out DateExist[] dates,
            out string strError)
        {
            strError = "";
            dates = null;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginExistStatisInfo(
                    strDateRangeString,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndExistStatisInfo(
                    out dates,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long HitCounter(string strAction,
            string strName,
            string strClientAddress,
            out long lValue,
            out string strError)
        {
            strError = "";
            lValue = 0;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginHitCounter(
                    strAction,
                    strName,
                    strClientAddress,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndHitCounter(
                    out lValue,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long SearchCharging(
            DigitalPlatform.Stop stop,
            string patronBarcode,
            string timeRange,
            string actions,
            string order,
            long start,
            long count,
            out ChargingItemWrapper[] results,
            out string strError)
        {
            strError = "";
            results = null;

            REDO:
            this.BeginSearch();
            try
            {
                IAsyncResult soapresult = this.ws.BeginSearchCharging(
                    patronBarcode,
                        timeRange,
                        actions,
                        order,
                        start,
                        count,
                        null,
                        null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSearchCharging(
                    out results,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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

        // 获得借阅历史
        // parameters:
        //      nPageNo 页号
        //      nItemsPerPage    每页的事项个数。如果为 -1，表示希望从头获取全部内容
        // return:
        //      -1  出错
        //      其它  符合条件的事项总数
        public long LoadChargingHistory(
            DigitalPlatform.Stop stop,
            string strBarcode,
            string strActions,
            int nPageNo,
            int nItemsPerPage,
            out List<ChargingItemWrapper> results,
            out string strError)
        {
            strError = "";
            results = new List<ChargingItemWrapper>();

            long lHitCount = 0;

            long lLength = 0;
            long lStart = 0;
            if (nItemsPerPage == -1)
                lLength = -1;
            else
            {
                lStart = nPageNo * nItemsPerPage;
                lLength = nItemsPerPage;
            }
            int nGeted = 0;
            for (; ; )
            {
                ChargingItemWrapper[] temp_results = null;
                long lRet = this.SearchCharging(
                    stop,
                    strBarcode,
                    "~",
                    strActions, // "return,lost",
                    "descending",
                    lStart + nGeted,
                    lLength,
                    out temp_results,
                    out strError);
                if (lRet == -1)
                    return -1;
                lHitCount = lRet;
                if (temp_results == null || temp_results.Length == 0)
                    break;
                results.AddRange(temp_results);

                // 修正 lLength
                if (lLength != -1 && lHitCount < lStart + nGeted + lLength)
                    lLength -= lStart + nGeted + lLength - lHitCount;

                if (results.Count >= lHitCount - lStart)
                    break;

                nGeted += temp_results.Length;
                if (lLength != -1)
                    lLength -= temp_results.Length;

                if (lLength <= 0 && lLength != -1)
                    break;
            }

            return lHitCount;
        }

        // 2016/9/19
        public long Dir(string strResPath,
    long lStart,
    long lLength,
    string strLang,
    string strStyle,
    out ResInfoItem[] items,
    out ErrorCodeValue kernel_errorcode,
            out string strError)
        {
            strError = "";
            items = null;
            kernel_errorcode = ErrorCodeValue.NoError;

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginDir(
                    strResPath,
                    lStart,
                    lLength,
                    strLang,
                    strStyle,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndDir(
                    out items,
                    out kernel_errorcode,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long GetAuthorNumber(
    string strAuthor,
    bool bSelectPinyin,
    bool bSelectEntry,
    bool bOutputDebugInfo,
    ref Question[] questions,
    out string strNumber,
    out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            strNumber = null;

            REDO:
            try
            {
                // localhost.dp2libraryClient ws1 = this.ws;
                IAsyncResult soapresult = this.ws.BeginGetAuthorNumber(
                    strAuthor,
                    bSelectPinyin,
                    bSelectEntry,
                    bOutputDebugInfo,
                    ref questions,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetAuthorNumber(
                    ref questions,
                    out strNumber,
                    out strDebugInfo,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long GetPinyin(
string strText,
out string strPinyinXml,
    out string strError)
        {
            strError = "";
            strPinyinXml = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginGetPinyin(
                    strText,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndGetPinyin(
                    out strPinyinXml,
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public long SetPinyin(
string strPinyinXml,
    out string strError)
        {
            strError = "";

            REDO:
            try
            {
                IAsyncResult soapresult = this.ws.BeginSetPinyin(
                    strPinyinXml,
                    null,
                    null);

                WaitComplete(soapresult);

                if (this.m_ws == null)
                {
                    strError = "用户中断";
                    this.ErrorCode = localhost.ErrorCode.RequestCanceled;
                    return -1;
                }

                LibraryServerResult result = this.ws.EndSetPinyin(
                    soapresult);
                if (result.Value == -1 && result.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = result.ErrorInfo;
                this.ErrorCode = result.ErrorCode;
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
        }

        public void DoStop()
        {
            // 2015/7/30 增加捕获异常语句
            try
            {
                IAsyncResult result = this.ws.BeginStop(
                    null,
                    null);
            }
            catch (CommunicationObjectFaultedException)
            {
                // 这里是不用处理的
            }
            catch
            {
                // TODO: 这里最好探究一下什么原因引起的
            }
        }

        // 异常:可能会抛出异常
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
            {
#if NO
                // TODO: Search()要单独处理
                // this.m_ws.Abort();
                this.AbortIt();
#endif
                this.Close();
            }
#if NO
            // ws.Abort();
            if (String.IsNullOrEmpty(this.Url) == false)
            {
                ws.CancelAsync(null);

                /*
                // 2011/1/7 add
                this.m_ws = null;
                 * */
            }
#endif
        }

        /*
crashReport -- 异常报告 
主题 dp2circulation 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 DigitalPlatform.CirculationClient.LibraryChannel.Close()
在 dp2Circulation.WebExternalHost.Destroy()
在 dp2Circulation.EntityBarcodeFoundDupDlg.EntityBarcodeFoundDupDlg_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.CheckCloseDialog(Boolean closingOnly)

 
操作时间 2015/7/21 14:19:54 (Tue, 21 Jul 2015 14:19:54 +0800) 
前端地址 117.10.161.38经由 http://dp2003.com/dp2library 
         * */
        // Close() 和 AbortIt() 很可能被不同的线程调用，其中一个设置 m_ws 为 null 可能会导致另外一个方法抛出异常
        public void Close()
        {
            lock (syncRoot)
            {
                if (this.m_ws != null)
                {
                    // TODO: Search()要单独处理
                    try
                    {
                        // this.Timeout = new TimeSpan(0,0,4); // 2015/11/28
                        if (this.m_ws.State != CommunicationState.Faulted)
                            WsClose();  // this.m_ws.Close();  // 如果长时间不返回怎么办？
                    }
                    catch
                    {
                        if (this.m_ws != null)
                            WsAbort();  // this.m_ws.Abort();
                    }
                    this.m_ws = null;
                }
            }
        }

        /// <summary>
        /// 立即放弃通讯。而 Abort() 要文雅一些
        /// </summary>
        public void TryAbortIt()
        {
            lock (syncRoot)
            {
                if (this.m_ws != null)
                {
                    WsAbort();  // this.m_ws.Abort();  // TODO: 是否因为这里没有调用 .Close() 导致通道泄露？
                    WsClose();  // this.m_ws.Close();  // 2015/12/31
                    this.m_ws = null;
                }
            }
        }

        void WsAbort()
        {
            try // 2017/10/14
            {
                if (this.m_ws is IClientChannel)
                    ((IClientChannel)this.m_ws).Abort();
                else
                    this.m_ws.Abort();
            }
            catch
            {

            }
        }

        void WsClose()
        {
            // 2016/11/19
            // rest 和 basic 协议，都要明确 Logout()，才不会在 dp2library 一侧残留通道
            if (this.m_ws != null && this.m_ws is localhost.dp2libraryRESTClient)
            {
                try
                {
                    IAsyncResult soapresult = this.ws.BeginLogout(null, null);
                    Thread.Sleep(100);
                }
                catch
                {
                }
            }

            if (this.m_ws is IClientChannel)
                ((IClientChannel)this.m_ws).Close();
            else
                this.m_ws.Close();
        }

        void SetInnerChannelOperationTimeout(TimeSpan timeout)
        {
            if (this.m_ws is localhost.dp2libraryClient)
                (this.m_ws as localhost.dp2libraryClient).InnerChannel.OperationTimeout = timeout;
#if BASIC_HTTP
            else if (this.m_ws is localhost.dp2libraryRESTClient)
                (this.m_ws as localhost.dp2libraryRESTClient).InnerChannel.OperationTimeout = timeout;
#endif
            else
            {
                // http://stackoverflow.com/questions/4695656/add-operationtimeout-to-channel-implemented-in-code
                ((IContextChannel)this.m_ws).OperationTimeout = timeout;
            }
        }

        TimeSpan GetInnerChannelOperationTimeout()
        {
            if (this.m_ws is localhost.dp2libraryClient)
                return (this.m_ws as localhost.dp2libraryClient).InnerChannel.OperationTimeout;
#if BASIC_HTTP
            else if (this.m_ws is localhost.dp2libraryRESTClient)
                return (this.m_ws as localhost.dp2libraryRESTClient).InnerChannel.OperationTimeout;
#endif

            return ((IContextChannel)this.m_ws).OperationTimeout;
        }

        long _uploadResChunkSize = 0;

        public long UploadResChunkSize
        {
            get
            {
                if (_uploadResChunkSize == 0)
                {
                    if (this.m_ws is localhost.dp2libraryClient)
                        return 500 * 1024;  // other
                    else if (this.m_ws is localhost.dp2libraryRESTClient)
                        return 500 * 1024;  // basic.http

                    return 250 * 1024;  // restful
                }
                return _uploadResChunkSize;
            }
            set
            {
                _uploadResChunkSize = value;    // 如果设置为 0，则表示软件自动决定 chunksize
            }
        }

        // 提供给
        //             stop.OnStop += new StopEventHandler(this.DoStop);
        public void DoStop(object sender, StopEventArgs e)
        {
            this.Abort();
        }
    }

    /// <summary>
    /// 登录事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void LoginEventHandle(object sender,
        LoginEventArgs e);

    /// <summary>
    /// LoginEventHandle 事件的参数
    /// </summary>
    public class LoginEventArgs : EventArgs
    {
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;

        /// <summary>
        /// 是否要放弃登录
        /// </summary>
        public bool Cancel = false;

        /// <summary>
        /// 出错信息
        /// </summary>
        public string ErrorInfo = "";
    }

    public class MyValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
            return;
        }
    }

    class CustomIdentityVerifier : IdentityVerifier
    {
        public override bool CheckAccess(EndpointIdentity identity, AuthorizationContext authContext)
        {
            LibraryChannelManager.Log?.Debug("Enter CheckAccess().");
            try
            {
                foreach (ClaimSet claimset in authContext.ClaimSets)
                {
                    LibraryChannelManager.Log?.Debug("loop.");

                    if (claimset.ContainsClaim(identity.IdentityClaim))
                    {
                        LibraryChannelManager.Log?.Debug("ContainsClaim.");
                        return true;
                    }

                    // string expectedSpn = null;
                    if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                    {
                        string strHost = (string)identity.IdentityClaim.Resource;
                        LibraryChannelManager.Log?.Debug("strHost='" + strHost + "'");

                        /*
                        expectedSpn = string.Format(CultureInfo.InvariantCulture, "host/{0}",
                            strHost);
                         * */
                        Claim claim = CheckDnsEquivalence(claimset, strHost);
                        if (claim != null)
                        {
                            LibraryChannelManager.Log?.Debug("CheckDnsEquivalence() return not null.");
                            return true;
                        }
                    }
                }

                //Stopwatch stopwath = new Stopwatch();
                //stopwath.Start();

                LibraryChannelManager.Log?.Debug("IdentityVerifier.CreateDefault().CheckAccess()");

                bool bRet = IdentityVerifier.CreateDefault().CheckAccess(identity, authContext);

                LibraryChannelManager.Log?.Debug("return bRet='" + bRet + "'");

                //stopwath.Stop();
                //Debug.WriteLine("CheckAccess " + stopwath.Elapsed.ToString());

                if (bRet == true)
                    return true;

                return false;
            }
            finally
            {
                LibraryChannelManager.Log?.Debug("Exit CheckAccess().");
            }
        }

        static string ToString(ClaimSet claimSet)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (Claim claim in claimSet)
            {
                text.Append((i + 1).ToString() + "\r\n");
                text.Append("ClaimType=" + claim.ClaimType + "\r\n");
                text.Append("Right=" + claim.Right + "\r\n");
                try
                {
                    text.Append("Resource=" + claim.Resource.ToString() + "\r\n");
                }
                catch
                {

                }
                i++;
            }

            return text.ToString();
        }

        Claim CheckDnsEquivalence(ClaimSet claimSet, string expectedDns)
        {
            LibraryChannelManager.Log?.Debug("Enter CheckDnsEquivalence(). expectedDns='" + expectedDns + "'");
            try
            {
                LibraryChannelManager.Log?.Debug("claimSet.Count='" + claimSet.Count + "' elements:\r\n" + ToString(claimSet));
                IEnumerable<Claim> claims = claimSet.FindClaims(ClaimTypes.Dns, Rights.PossessProperty);
                // 可能找不到结果
                foreach (Claim claim in claims)
                {
                    // 格外允许"localhost"
                    if (expectedDns.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        LibraryChannelManager.Log?.Debug("expectedDns Match 'localhost'");
                        return claim;
                    }

                    string strCurrentDns = (string)claim.Resource;

                    LibraryChannelManager.Log?.Debug("strCurrentDns='" + strCurrentDns + "'");

                    // 格外允许"DigitalPlatform"和任意出发字符串匹配
                    if (strCurrentDns.Equals("DigitalPlatform", StringComparison.OrdinalIgnoreCase))
                    {
                        LibraryChannelManager.Log?.Debug("strCurrentDns Match 'DigitalPlatform'");
                        return claim;
                    }

                    if (expectedDns.Equals(strCurrentDns, StringComparison.OrdinalIgnoreCase))
                    {
                        LibraryChannelManager.Log?.Debug("strCurrentDns Match expectedDns");
                        return claim;
                    }
                }

                // 2018/6/26
                // 如果 Dns 事项没有找到，再尝试找 Name 事项。这一般是有问题的 Windows 7 换进该
                claims = claimSet.FindClaims(ClaimTypes.Name, Rights.PossessProperty);
                foreach (Claim claim in claims)
                {
                    string strCurrentName = (string)claim.Resource;

                    LibraryChannelManager.Log?.Debug("strCurrentName='" + strCurrentName + "'");

                    // 格外允许"DigitalPlatform"和任意出发字符串匹配
                    if (strCurrentName.Equals("DigitalPlatform", StringComparison.OrdinalIgnoreCase))
                    {
                        LibraryChannelManager.Log?.Debug("strCurrentName Match 'DigitalPlatform'");
                        return claim;
                    }
                }

                return null;
            }
            finally
            {
                LibraryChannelManager.Log?.Debug("Exit CheckDnsEquivalence().");
            }
        }

        public override bool TryGetIdentity(EndpointAddress reference, out EndpointIdentity identity)
        {
            LibraryChannelManager.Log?.Debug("Enter TryGetIdentity().");
            try
            {
                return IdentityVerifier.CreateDefault().TryGetIdentity(reference, out identity);
            }
            finally
            {
                LibraryChannelManager.Log?.Debug("Exit TryGetIdentity().");
            }
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
#endif

    /// <summary>
    /// 登录前的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void BeforeLoginEventHandle(object sender,
    BeforeLoginEventArgs e);

    /// <summary>
    /// 登录前事件的参数
    /// </summary>
    public class BeforeLoginEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 是否为第一次触发
        /// </summary>
        public bool FirstTry = true;    // [in] 是否为第一次触发
        /// <summary>
        /// [in] 图书馆应用服务器 URL
        /// </summary>
        public string LibraryServerUrl = "";    // [in] 图书馆应用服务器URL
        /// <summary>
        /// [out] 用户名
        /// </summary>
        public string UserName = "";    // [out] 用户名
        /// <summary>
        /// [out] 密码
        /// </summary>
        public string Password = "";    // [out] 密码
        /// <summary>
        /// [out] 工作台号
        /// </summary>
        public string Parameters = "";    // [out] 工作台号
        // public bool IsReader = false;   // [out] 登录者是否为读者
        /// <summary>
        /// [out] 短期保存密码
        /// </summary>
        public bool SavePasswordShort = false;  // [out] 短期保存密码
        /// <summary>
        /// [out] 长期保存密码
        /// </summary>
        public bool SavePasswordLong = false;   // [out] 长期保存密码
        /// <summary>
        /// [out] 事件调用是否失败
        /// </summary>
        public bool Failed = false; // [out] 事件调用是否失败
        /// <summary>
        /// [out] 事件调用是否被放弃
        /// </summary>
        public bool Cancel = false; // [out] 事件调用是否被放弃
        /// <summary>
        /// [in, out] 事件调用错误信息
        /// </summary>
        public string ErrorInfo = "";   // [in, out] 事件调用错误信息
        /// <summary>
        /// [in, out] 前次登录失败的原因，本次登录失败的原因
        /// </summary>
        public LoginFailCondition LoginFailCondition = LoginFailCondition.NormalError;  // [in, out] 前次登录失败的原因，本次登录失败的原因
    }

    /// <summary>
    /// 登录后的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AfterLoginEventHandle(object sender,
    AfterLoginEventArgs e);

    /// <summary>
    /// 登录后事件的参数
    /// </summary>
    public class AfterLoginEventArgs : EventArgs
    {
        public string ErrorInfo = "";
        // public bool Canceled = false;
    }
}
