using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryService;
using DigitalPlatform.Text;
using dp2Library;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace dp2LibraryXE
{
    public class LibraryHost : HostBase
    {
        public static string default_miniserver_urls = "http://localhost:8001/dp2library/xe;net.pipe://localhost/dp2library/xe;rest.http://localhost/dp2library/xe/rest";
        public static string default_single_url = "net.pipe://localhost/dp2library/xe";

        // ServiceHost _host = null;
        List<ServiceHost> m_hosts = new List<ServiceHost>();

        public string HostUrl = default_single_url; // "net.pipe://localhost/dp2library/xe";

        public override void ThreadMethod()
        {
            string strError = "";

            _running = true;

            int nRet = Start(this.DataDir, out strError);
            if (nRet == -1)
            {
                this.ErrorInfo = strError;
                // this._host = null;
                this.m_hosts.Clear();
            }

            this._eventStarted.Set();

            while (_running)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch
                {
                }
            }

            this.CloseHosts();
            this._thread = null;
            this._eventClosed.Set();
        }

#if NO
        void CloseHosts()
        {
            if (this._host != null)
            {
                HostInfo info = _host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Dispose();
                    _host.Extensions.Remove(info);
                }

                _host.Close();
                _host = null;
            }
        }
#endif
        public override void CloseHosts()
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    host.Extensions.Remove(info);
                    info.Dispose();
                }

                host.Close();
            }

            this.m_hosts.Clear();
        }

        int Start(string strDataDir,
            out string strError)
        {
            strError = "";

            CloseHosts();

            List<string> urls = StringUtil.SplitList(this.HostUrl, ';');

            ServiceHost host = new ServiceHost(typeof(LibraryService));
            this.m_hosts.Add(host);

            HostInfo info = new HostInfo();
            info.DataDir = strDataDir;
            host.Extensions.Add(info);

            bool bHasWsHttp = false;
            int i = 0;
            foreach (string strTempUrl in urls)
            {
                string strUrl = strTempUrl.Trim();

                if (string.IsNullOrEmpty(strUrl) == true)
                    continue;

                /// 
                // 绑定协议

                Uri uri = null;
                try
                {
                    uri = new Uri(strUrl);
                }
                catch (Exception ex)
                {
                    strError = "dp2Library OnStart() 警告：发现不正确的协议URL '" + strUrl + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。";
                    return -1;
                }

                if (uri.Scheme.ToLower() == "net.pipe")
                {
                    host.AddServiceEndpoint(typeof(ILibraryService),
                        CreateNamedpipeBinding0(),
                        strUrl);
                }
                else if (uri.Scheme.ToLower() == "net.tcp")
                {
                    host.AddServiceEndpoint(typeof(ILibraryService),
                        CreateNetTcpBinding0(),
                        strUrl);
                }
                else if (uri.Scheme.ToLower() == "http"
                    || uri.Scheme.ToLower() == "https")
                {
                    ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryService),
    uri.Scheme.ToLower() == "http" ? CreateWsHttpBinding1() : WcfBindings.CreateHttpsBinding1(),
    strUrl);
                    if (uri.Scheme.ToLower() == "http")
                        bHasWsHttp = true;
                }
                else if (uri.Scheme.ToLower() == "rest.http"
                    || uri.Scheme.ToLower() == "rest.https")
                {
                    ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryServiceREST),
CreateWebHttpBinding1(uri.Scheme.ToLower() == "rest.https"),
strUrl.Substring(5));   // rest. 这几个字符要去掉
                    if (endpoint.Behaviors.Find<WebHttpBehavior>() == null)
                    {
                        WebHttpBehavior behavior = new WebHttpBehavior();
                        behavior.DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped;
                        behavior.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                        behavior.AutomaticFormatSelectionEnabled = true;
                        behavior.HelpEnabled = true;
                        endpoint.Behaviors.Add(behavior);
                    }
                }
                else
                {
                    // 警告不能支持的协议
                    strError = "dp2Library OnStart() 警告：发现不能支持的协议类型 '" + strUrl + "'";
                    return -1;
                }

                info.Protocol = uri.Scheme.ToLower();

                // 只有第一个host才有metadata能力
                if (// i == 0 // 
                    uri.Scheme.ToLower() == "http"
                    && host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                {
                    string strMetadataUrl = strUrl;    //  "http://localhost:8001/dp2library/xe/";
                    if (strMetadataUrl[strMetadataUrl.Length - 1] != '/')
                        strMetadataUrl += "/";
                    strMetadataUrl += "$metadata";  // http://localhost/dp2library/xe/$metadata

                    ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                    behavior.HttpGetEnabled = true;
                    behavior.HttpGetUrl = new Uri(strMetadataUrl);
                    host.Description.Behaviors.Add(behavior);

                    this.MetadataUrl = strMetadataUrl;
                }

                i++;
            }

            // 如果具有ws1/ws2 binding，才启用证书
            if (bHasWsHttp == true)
            {
                try
                {
                    string strCertSN = "";
                    X509Certificate2 cert = GetCertificate(strCertSN,
                        out strError);
                    if (cert == null)
                    {
                        strError = "dp2Library OnStart() 准备证书 时发生错误: " + strError;
                        return -1;
                    }
                    else
                        host.Credentials.ServiceCertificate.Certificate = cert;

                }
                catch (Exception ex)
                {
                    strError = "dp2Library OnStart() 获取证书时发生错误: " + ExceptionUtil.GetExceptionMessage(ex);
                    return -1;
                }
            }

            if (host.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
            {
                ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior();
                behavior.MaxConcurrentCalls = 50;
                behavior.MaxConcurrentInstances = 1000;
                behavior.MaxConcurrentSessions = 1000;
                host.Description.Behaviors.Add(behavior);
            }

            // IncludeExceptionDetailInFaults
            ServiceDebugBehavior debug_behavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debug_behavior == null)
            {
                host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
            }
            else
            {
                if (debug_behavior.IncludeExceptionDetailInFaults == false)
                    debug_behavior.IncludeExceptionDetailInFaults = true;
            }

            host.Opening += new EventHandler(host_Opening);
            host.Closing += new EventHandler(m_host_Closing);

            try
            {
                host.Open();
            }
            catch (Exception ex)
            {
                string strInstanceName = "";
                strError = "dp2Library OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ExceptionUtil.GetExceptionMessage(ex);
                return -1;
            }

#if NO
            strError = "test error";
            return -1;
#endif

            return 0;
        }

        void host_Opening(object sender, EventArgs e)
        {

        }

        void m_host_Closing(object sender, EventArgs e)
        {
#if NO
            if (this._host != null)
            {
                HostInfo info = _host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Dispose();
                    _host.Extensions.Remove(info);
                }
            }
#endif
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Dispose();
                    host.Extensions.Remove(info);
                }
            }
        }

        public void SetTestMode(bool bTestMode)
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.TestMode = bTestMode;
                    if (info.App != null)
                        info.App.TestMode = bTestMode;
                }
            }
        }

        public void SetMaxClients(int nMaxClients)
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.MaxClients = nMaxClients;
                    if (info.App != null)
                        info.App.MaxClients = nMaxClients;
                }
            }
        }

        public void SetLicenseType(string strLicenseType)
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.LicenseType = strLicenseType;
                    if (info.App != null)
                        info.App.LicenseType = strLicenseType;
                }
            }
        }

        public void SetFunction(string strFunction)
        {
            if (string.IsNullOrEmpty(strFunction) == false
                && strFunction.IndexOf("|") != -1)
                throw new ArgumentException("strFunction 参数值中不允许包含字符 '|'。应改用逗号");

            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Function = strFunction;
                    if (info.App != null)
                        info.App.Function = strFunction;
                }
            }
        }



        // np0: namedpipe 
        System.ServiceModel.Channels.Binding CreateNamedpipeBinding0()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            WcfBindings.SetTimeout(binding);
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        // nt0: net.tcp 
        System.ServiceModel.Channels.Binding CreateNetTcpBinding0()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = SecurityMode.None;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            WcfBindings.SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        // ws1: anonymouse -- ClientCredentitialType = None
        System.ServiceModel.Channels.Binding CreateWsHttpBinding1()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            {
                binding.Security.Mode = SecurityMode.Message;
#if !USERNAME
                binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
#else
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
#endif
            }

            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            WcfBindings.SetTimeout(binding);

            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.Enabled = false;

            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            return binding;
        }

#if REMOVED
        // https://learn.microsoft.com/en-us/dotnet/framework/wcf/samples/ws-transport-security
        System.ServiceModel.Channels.Binding CreateWsHttpsBinding1()
        {
            var binding = new WSHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;

            /*
             // https://learn.microsoft.com/en-us/dotnet/framework/wcf/samples/custom-binding-reliable-session-over-https
            BindingElementCollection outputBec = new BindingElementCollection();   //binding.CreateBindingElements();
            var e = (HttpsTransportBindingElement)outputBec.Find<HttpsTransportBindingElement>();
            outputBec.Add(new ReliableSessionBindingElement());
            outputBec.Add(new HttpsTransportBindingElement());
            var b = new CustomBinding(outputBec);
            
            return b;
            */

            // https://stackoverflow.com/questions/2650738/how-to-enable-wcf-session-with-wshttpbidning-with-transport-only-security
            binding.ReliableSession.Enabled = true;
            binding.ReliableSession.Ordered = true;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            //binding.ReliableSession.Enabled = false;
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            return binding;
        }
#endif

        System.ServiceModel.Channels.Binding CreateWebHttpBinding1(bool isHTTPS = false)
        {
            WebHttpBinding binding = new WebHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            // binding.Security.Mode = WebHttpSecurityMode.None;
            binding.Security.Mode = isHTTPS ? WebHttpSecurityMode.Transport : WebHttpSecurityMode.None;
            // binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            WcfBindings.SetTimeout(binding);

            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        static X509Certificate2 FindCertificate(
StoreLocation location, StoreName name,
X509FindType findType, string findValue)
        {
            X509Store store = new X509Store(name, location);
            try
            {
                // create and open store for read-only access
                store.Open(OpenFlags.ReadOnly);

                // search store
                X509Certificate2Collection col = store.Certificates.Find(
                  findType, findValue, false);

                if (col.Count == 0)
                    return null;

                // return first certificate found
                return col[0];
            }
            // always close the store
            finally { store.Close(); }
        }


        X509Certificate2 GetCertificate(
    string strCertSN,
    out string strError)
        {
            strError = "";
            /*
            string strCertSN = GetProductString(
    "dp2Library",
    "cert_sn");
             * */
            if (string.IsNullOrEmpty(strCertSN) == false)
            {
                X509Certificate2 cert = FindCertificate(
StoreLocation.LocalMachine,
StoreName.My,   // .Root,
X509FindType.FindBySerialNumber,
strCertSN);
                if (cert == null)
                {
                    strError = "序列号为 '" + strCertSN + "' 的证书在 StoreLocation.LocalMachine | StoreLocation.CurrentUser / StoreName.My 中不存在。";
                    return null;
                }

                return cert;
            }

            // 缺省的SubjectName为DigitalPlatform的证书
            string strCurrentDir = System.Reflection.Assembly.GetExecutingAssembly().Location;   //  Environment.CurrentDirectory;

            strCurrentDir = PathUtil.PathPart(strCurrentDir);

            string strCerFilename = PathUtil.MergePath(strCurrentDir, "digitalplatform.pfx");

            return new X509Certificate2(strCerFilename, "setupdp2");
        }
    }
}
