using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

using System.Security.Cryptography.X509Certificates;
// using System.IdentityModel.Selectors;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

using Microsoft.Win32;

using DigitalPlatform.rms;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.IO;
using System.Collections;

namespace dp2Kernel
{
    public partial class KernelServiceHost : ServiceBase
    {
        // ServiceHost m_host = null;
        List<ServiceHost> m_hosts = new List<ServiceHost>();

        Thread m_thread = null;

        public EventLog Log = null;

        public KernelServiceHost()
        {
            InitializeComponent();

            // 初始化事件日志
            this.Log = new EventLog();
            this.Log.Source = "dp2Kernel";

            this.m_thread = new Thread(new ThreadStart(ThreadMain));
        }

        public static void Main(string[] args)
        {
            if (args.Length == 1 && args[0].Equals("console"))
            {
                new KernelServiceHost().ConsoleRun();
            }
            else
            {
                ServiceBase.Run(new KernelServiceHost());
            }
        }

        static CtrlEventHandler _handler;

        private bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    {
                        Debug.WriteLine("close ...");
                        Console.WriteLine("closing...");
                        CloseHosts();
                    }
                    return true;
                default:
                    break;
            }

            return false;
        }

        bool m_bConsoleRun = false;

        private void ConsoleRun()
        {
            this.m_bConsoleRun = true;

            // Some biolerplate to react to close window event
            _handler += new CtrlEventHandler(Handler);
            API.SetConsoleCtrlHandler(_handler, true);

            Console.WriteLine("{0}::starting...", GetType().FullName);

            OnStart(null);

            Console.WriteLine("{0}::ready (ENTER to exit)", GetType().FullName);

            Console.ReadLine();

            OnStop();
            Console.WriteLine("{0}::stopped", GetType().FullName);
        }


#if NO
        static string GetHostUrl()
        {
            using (RegistryKey digitalplatform = Registry.LocalMachine.CreateSubKey("SOFTWARE\\DigitalPlatform"))
            {
                using (RegistryKey dp2kernel = digitalplatform.CreateSubKey("dp2Kernel"))
                {
                    return (string)dp2kernel.GetValue("hosturl");
                }
            }
        }
#endif

        // 获得instance信息
        // parameters:
        //      urls 获得绑定的Urls
        // return:
        //      false   instance没有找到
        //      true    找到
        public static bool GetInstanceInfo(string strProductName,
            int nIndex,
            out string strInstanceName,
            out string strDataDir,
            out string[] urls,
            out string strCertificateSN)
        {
            strInstanceName = "";
            strDataDir = "";
            urls = null;
            strCertificateSN = "";

            string strLocation = "SOFTWARE\\DigitalPlatform";

            /*
            if (Environment.Is64BitProcess == true)
                strLocation = "SOFTWARE\\Wow6432Node\\DigitalPlatform";
             * */

            using (RegistryKey digitalplatform = Registry.LocalMachine.CreateSubKey(strLocation))
            {
                using (RegistryKey product = digitalplatform.CreateSubKey(strProductName))
                {
                    RegistryKey instance = product.OpenSubKey("instance_" + nIndex.ToString());
                    if (instance == null)
                        return false;   // not found

                    using (instance)
                    {
                        strInstanceName = (string)instance.GetValue("name");

                        strDataDir = (string)instance.GetValue("datadir");

                        strCertificateSN = (string)instance.GetValue("cert_sn");

                        urls = (string[])instance.GetValue("bindings");
                        if (urls == null)
                            urls = new string[0];

                        return true;    // found
                    }
                }
            }
        }

        // 根据协议名找到一个URL
        public static string FindUrl(string strProtocol,
            string[] urls)
        {
            for (int i = 0; i < urls.Length; i++)
            {
                string strUrl = urls[i].Trim();
                if (String.IsNullOrEmpty(strUrl) == true)
                    continue;

                try
                {
                    Uri uri = new Uri(strUrl);
                    if (uri.Scheme.ToLower() == strProtocol.ToLower())
                        return strUrl;
                }
                catch
                {
                }

            }

            return null;
        }

        void CloseHosts()
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

        class SqlDbNameData
        {
            public string Instance { get; set; }
            public string SqlDbName { get; set; }
        }

        // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
        // return:
        //      -1  检查过程出错
        //      0   没有冲突
        //      1   发生了冲突。报错信息在 strError 中
        int CheckSqlDbNames(out string strError)
        {
            strError = "";

            Hashtable name_table = new Hashtable(); // sqldbname --> SqlDbNameData
            Hashtable prefix_table = new Hashtable();// prefix --> SqlDbNameData

            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertSN = "";
                string[] existing_urls = null;
                bool bRet = GetInstanceInfo("dp2Kernel",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertSN);
                if (bRet == false)
                    break;
                if (string.IsNullOrEmpty(strDataDir))
                    continue;

                string strFileName = Path.Combine(strDataDir, "databases.xml");
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    strError = "文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                    return -1;
                }

                // 检查 dbs/@instancename
                string strInstancePrefix = "";
                XmlAttribute prefix = dom.DocumentElement.SelectSingleNode("dbs/@instancename") as XmlAttribute;
                if (prefix != null)
                    strInstancePrefix = prefix.Value;

                if (prefix_table.ContainsKey(strInstancePrefix))
                {
                    SqlDbNameData data = (SqlDbNameData)prefix_table[strInstancePrefix];
                    strError = "实例 '" + strInstanceName + "' (" + strFileName + ") 中 dbs 元素 instancename 属性值 '" + strInstancePrefix + "' 和实例 '" + data.Instance + "' 的用法重复了";
                    return 1;
                }
                else
                {
                    SqlDbNameData data = new SqlDbNameData();
                    data.Instance = strInstanceName;
                    data.SqlDbName = strInstancePrefix;
                    prefix_table[strInstancePrefix] = data;
                }

                // 检查 sqlserverdb/@name
                XmlNodeList name_nodes = dom.DocumentElement.SelectNodes("dbs/database/property/sqlserverdb/@name");
                foreach (XmlAttribute attr in name_nodes)
                {
                    string value = attr.Value;
                    if (string.IsNullOrEmpty(value))
                        continue;
                    value = value.ToLower();
                    if (name_table.ContainsKey(value))
                    {
                        SqlDbNameData data = (SqlDbNameData)name_table[value];
                        strError = "实例 '" + strInstanceName + "' 中 SQL 数据库名 '" + value + "' 和实例 '" + data.Instance + "' 中另一 SQL 数据库名重复了";
                        return 1;
                    }

                    {
                        SqlDbNameData data = new SqlDbNameData();
                        data.Instance = strInstanceName;
                        data.SqlDbName = value;
                        name_table[value] = data;
                    }
                }
            }

            return 0;
        }

        void ThreadMain()
        {
            // Debug.Assert(false, "");
            CloseHosts();

            // 2017/2/9
            string strError = "";
            // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
            // return:
            //      -1  检查过程出错
            //      0   没有冲突
            //      1   发生了冲突。报错信息在 strError 中
            int nRet = CheckSqlDbNames(out strError);
            if (nRet != 0)
            {
                this.Log.WriteEntry("dp2Kernel 实例启动阶段发生严重错误: " + strError, EventLogEntryType.Error);
                return;
            }

            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertSN = "";
                string[] existing_urls = null;
                bool bRet = GetInstanceInfo("dp2Kernel",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertSN);
                if (bRet == false)
                    break;

#if NO
                string strWsHostUrl = FindUrl("http", existing_urls);

                string strNamedpipeHostUrl = FindUrl("net.pipe", existing_urls);
                string strNetTcpHostUrl = FindUrl("net.tcp", existing_urls);
#endif

                ServiceHost host = new ServiceHost(typeof(KernelService));
                this.m_hosts.Add(host);

                HostInfo info = new HostInfo();
                info.DataDir = strDataDir;
                host.Extensions.Add(info);
                /// 

                bool bHasWsHttp = false;
                // 绑定协议
                foreach (string url in existing_urls)
                {
                    if (string.IsNullOrEmpty(url) == true)
                        continue;

                    Uri uri = null;
                    try
                    {
                        uri = new Uri(url);
                    }
                    catch (Exception ex)
                    {
                        this.Log.WriteEntry("dp2Kernel OnStart() 警告：发现不正确的协议URL '" + url + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。",
    EventLogEntryType.Error);
                        continue;
                    }

                    if (uri.Scheme.ToLower() == "http")
                    {
                        host.AddServiceEndpoint(typeof(IKernelService),
    CreateWsHttpBinding1(),
    url);
                        bHasWsHttp = true;
                    }
                    else if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        host.AddServiceEndpoint(typeof(IKernelService),
                CreateNamedpipeBinding0(),
                url);
                    }
                    else if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        host.AddServiceEndpoint(typeof(IKernelService),
            CreateNetTcpBinding0(),
            url);
                    }
                    else
                    {
                        // 警告不能支持的协议
                        this.Log.WriteEntry("dp2Kernel OnStart() 警告：发现不能支持的协议类型 '" + url + "'",
                            EventLogEntryType.Information);
                    }
                }

#if NO
                if (String.IsNullOrEmpty(strWsHostUrl) == false)
                {
                    host.AddServiceEndpoint(typeof(IKernelService),
                        CreateWsHttpBinding1(),
                        strWsHostUrl);
                }

                if (String.IsNullOrEmpty(strNamedpipeHostUrl) == false)
                {
                    host.AddServiceEndpoint(typeof(IKernelService),
            CreateNamedpipeBinding0(),
            strNamedpipeHostUrl);
                }
                if (String.IsNullOrEmpty(strNetTcpHostUrl) == false)
                {
                    host.AddServiceEndpoint(typeof(IKernelService),
        CreateNetTcpBinding0(),
        strNetTcpHostUrl);
                }
#endif

                // 如果具有ws1/ws2 binding，才启用证书
                if (bHasWsHttp == true/*String.IsNullOrEmpty(strWsHostUrl) == false*/)
                {
                    try
                    {
                        // host.Credentials.ServiceCertificate.Certificate = GetCertificate(strCertSN);
                        X509Certificate2 cert = GetCertificate(strCertSN,
                            out strError);
                        if (cert == null)
                            this.Log.WriteEntry("dp2Kernel OnStart() 准备证书 时发生错误: " + strError,
EventLogEntryType.Error);
                        else
                            host.Credentials.ServiceCertificate.Certificate = cert;

                    }
                    catch (Exception ex)
                    {
                        this.Log.WriteEntry("dp2Kernel OnStart() 获取证书时发生错误: " + ex.Message,
        EventLogEntryType.Error);
                        return;
                    }
                }

                /*
                 * 
                 * ws2 才启用
                m_host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                m_host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new MyUserNamePasswordValidator();
                */

                /*
                m_host.Credentials.ServiceCertificate.SetCertificate(
                    StoreLocation.CurrentUser,
                    StoreName.My,
                    X509FindType.FindBySubjectName,
                    "DigitalPlatform");
                 * */

                /*
                m_host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = 
                    System.ServiceModel.Security.X509CertificateValidationMode.Custom;
                m_host.Credentials.ClientCertificate.Authentication.CustomCertificateValidator =
        new MyValidator();
                 * */

                // 只有第一个host才有metadata能力
                if (i == 0 && host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                {
                    string strWsHostUrl = FindUrl("http", existing_urls);

                    string strMetadataUrl = strWsHostUrl;
                    if (String.IsNullOrEmpty(strMetadataUrl) == true)
                        strMetadataUrl = "http://localhost:8001/dp2kernel/";
                    if (strMetadataUrl[strMetadataUrl.Length - 1] != '/')
                        strMetadataUrl += "/";
                    strMetadataUrl += "metadata";

                    ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                    behavior.HttpGetEnabled = true;
                    behavior.HttpGetUrl = new Uri(strMetadataUrl);
                    host.Description.Behaviors.Add(behavior);
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
                    // 让调试器能感觉到
                    if (this.m_bConsoleRun == true)
                        throw ex;

                    this.Log.WriteEntry("dp2Kernel OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ex.Message,
    EventLogEntryType.Error);
                    return;
                }
            }
            this.Log.WriteEntry("dp2Kernel OnStart() end",
EventLogEntryType.Information);

            this.m_thread = null;
        }

        TimeSpan DefaultSendTimeout = new TimeSpan(0, 20, 0);
        TimeSpan DefaultRecieveTimeout = new TimeSpan(0, 40, 0); // 这里猜测，'Recieve' 是从前端的角度看，接收服务器响应阶段
        TimeSpan DefaultInactivityTimeout = new TimeSpan(0, 20, 0);

        // np0: namedpipe 
        System.ServiceModel.Channels.Binding CreateNamedpipeBinding0()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.Namespace = "http://dp2003.com/dp2kernel/";
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.SendTimeout = DefaultSendTimeout;
            binding.ReceiveTimeout = DefaultRecieveTimeout;
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        // nt0: net.tcp 
        System.ServiceModel.Channels.Binding CreateNetTcpBinding0()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Namespace = "http://dp2003.com/dp2kernel/";
            binding.Security.Mode = SecurityMode.None;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.SendTimeout = DefaultSendTimeout;
            binding.ReceiveTimeout = DefaultRecieveTimeout;
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        // ws0: windows -- ClientCredentitialType = Windows
        System.ServiceModel.Channels.Binding CreateWsHttpBinding0()
        {
            WSHttpBinding wshttp_binding = new WSHttpBinding();
            wshttp_binding.Namespace = "http://dp2003.com/dp2kernel/";
            wshttp_binding.Security.Mode = SecurityMode.Message;
            wshttp_binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            // wshttp_binding.Security.Message.NegotiateServiceCredential = false;
            // wshttp_binding.Security.Message.EstablishSecurityContext = false;

            wshttp_binding.MaxReceivedMessageSize = 1024 * 1024;
            wshttp_binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            wshttp_binding.ReaderQuotas = quotas;
            wshttp_binding.SendTimeout = DefaultSendTimeout;
            wshttp_binding.ReceiveTimeout = DefaultRecieveTimeout;
            // binding.ReliableSession.Enabled = false;

            wshttp_binding.ReliableSession.InactivityTimeout = DefaultInactivityTimeout;

            return wshttp_binding;
        }

        // ws1: anonymouse -- ClientCredentitialType = None
        System.ServiceModel.Channels.Binding CreateWsHttpBinding1()
        {
            WSHttpBinding wshttp_binding = new WSHttpBinding();
            wshttp_binding.Namespace = "http://dp2003.com/dp2kernel/";
            wshttp_binding.Security.Mode = SecurityMode.Message;
            wshttp_binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            wshttp_binding.MaxReceivedMessageSize = 1024 * 1024;
            wshttp_binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            wshttp_binding.ReaderQuotas = quotas;
            wshttp_binding.SendTimeout = DefaultSendTimeout;
            wshttp_binding.ReceiveTimeout = DefaultRecieveTimeout;
            // binding.ReliableSession.Enabled = false;

            wshttp_binding.ReliableSession.InactivityTimeout = DefaultInactivityTimeout;

            return wshttp_binding;
        }

        // ws2: username -- ClientCredentitialType = UserName
        System.ServiceModel.Channels.Binding CreateWsHttpBinding2()
        {
            WSHttpBinding wshttp_binding = new WSHttpBinding();
            wshttp_binding.Namespace = "http://dp2003.com/dp2kernel/";
            wshttp_binding.Security.Mode = SecurityMode.Message;
            wshttp_binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            // wshttp_binding.Security.Message.NegotiateServiceCredential = false;
            // wshttp_binding.Security.Message.EstablishSecurityContext = false;

            wshttp_binding.MaxReceivedMessageSize = 1024 * 1024;
            wshttp_binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            wshttp_binding.ReaderQuotas = quotas;
            wshttp_binding.SendTimeout = DefaultSendTimeout;
            wshttp_binding.ReceiveTimeout = DefaultRecieveTimeout;
            // binding.ReliableSession.Enabled = false;

            wshttp_binding.ReliableSession.InactivityTimeout = DefaultInactivityTimeout;

            return wshttp_binding;
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

            if (string.IsNullOrEmpty(strCertSN) == false)
            {
                X509Certificate2 cert = FindCertificate(
StoreLocation.LocalMachine,
StoreName.Root,
X509FindType.FindBySerialNumber,
strCertSN);
                if (cert == null)
                {
                    strError = "序列号为 '" + strCertSN + "' 的证书在 StoreLocation.LocalMachine | StoreLocation.CurrentUser / StoreName.Root 中不存在。";
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
#if NO
        X509Certificate2 GetCertificate()
        {
            string strCurrentDir = System.Reflection.Assembly.GetExecutingAssembly().Location;   //  Environment.CurrentDirectory;

            strCurrentDir = PathUtil.PathPart(strCurrentDir);

            string strCerFilename = PathUtil.MergePath(strCurrentDir, "digitalplatform.pfx");

            return new X509Certificate2(strCerFilename, "setupdp2");
        }
#endif


#if NO
        X509Certificate2 GetCertificate()
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, 
                DateTime.Now, false);

            if (fcollection.Count == null)
                return null;

            return fcollection[0];
        }
#endif

        protected override void OnStart(string[] args)
        {
            this.Log.WriteEntry("dp2Kernel OnStart() begin",
EventLogEntryType.Information);

            this.m_thread.Start();
        }

        void m_host_Closing(object sender, EventArgs e)
        {
            /*
            lock (GlobalVars.LockObject)
            {
                if (GlobalVars.KernelApplication != null)
                {
                    GlobalVars.KernelApplication.Close();
                }
                GlobalVars.KernelApplication = null;
            }
             * */

            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    host.Extensions.Remove(info);
                    info.Dispose();
                }
            }

        }

        void host_Opening(object sender, EventArgs e)
        {

        }

        protected override void OnStop()
        {
            if (this.m_thread != null)
            {
                this.m_thread.Abort();
                this.m_thread = null;
            }

            CloseHosts();
        }
    }

#if NO
    public class MyValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
        }
    }
#endif

    public class MyUserNamePasswordValidator : System.IdentityModel.Selectors.UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {

        }
    }


}
