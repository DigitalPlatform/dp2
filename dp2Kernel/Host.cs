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
using System.IO;
using System.Collections;

using Microsoft.Win32;

using DigitalPlatform.rms;
using DigitalPlatform.IO;
using DigitalPlatform;
using DigitalPlatform.Install;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;

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
                ServerInfo.Host = new KernelServiceHost();
                ServerInfo.Host.ConsoleRun();
                // new KernelServiceHost().ConsoleRun();
            }
            else
            {
                ServerInfo.Host = new KernelServiceHost();
                ServiceBase.Run(ServerInfo.Host);

                // ServiceBase.Run(new KernelServiceHost());
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

        // 关闭一个指定的 host
        // return:
        //      true    成功关闭
        //      false   没有找到指定的 host
        public bool CloseHost(string strInstanceName)
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (info.InstanceName == strInstanceName)
                {
                    info.Dispose();
                    host.Extensions.Remove(info);
                    host.Close();
                    this.m_hosts.Remove(host);
                    return true;
                }
            }

            return false;
        }

        public ServiceHost FindHost(string strInstanceName)
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info == null)
                    continue;

                if (info.InstanceName == strInstanceName)
                    return host;
            }

            return null;
        }

        // 2017/2/9
        // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
        // return:
        //      -1  检查过程出错
        //      0   没有冲突
        //      1   发生了冲突。报错信息在 strError 中
        int CheckSqlDbNames(out string strError)
        {
            strError = "";

            Hashtable name_table = new Hashtable();     // sqldbname --> InstanceValue
            Hashtable prefix_table = new Hashtable();   // prefix --> InstanceValue

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

                // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
                // return:
                //      -1  检查过程出错
                //      0   没有冲突
                //      1   发生了冲突。报错信息在 strError 中
                int nRet = InstallHelper.CheckDatabasesXml(
                    strInstanceName,
                    strDataDir,
                    prefix_table,
                    name_table,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return 1;
#if NO
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
                    InstanceValue data = (InstanceValue)prefix_table[strInstancePrefix];
                    strError = "实例 '" + strInstanceName + "' (" + strFileName + ") 中 dbs 元素 instancename 属性值 '" + strInstancePrefix + "' 和实例 '" + data.Instance + "' 的用法重复了";
                    return 1;
                }
                else
                {
                    InstanceValue data = new InstanceValue();
                    data.Instance = strInstanceName;
                    data.Value = strInstancePrefix;
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
                        InstanceValue data = (InstanceValue)name_table[value];
                        strError = "实例 '" + strInstanceName + "' 中 SQL 数据库名 '" + value + "' 和实例 '" + data.Instance + "' 中另一 SQL 数据库名重复了";
                        return 1;
                    }

                    {
                        InstanceValue data = new InstanceValue();
                        data.Instance = strInstanceName;
                        data.Value = value;
                        name_table[value] = data;
                    }
                }
#endif
            }

            return 0;
        }

        void ThreadMain()
        {
            CloseHosts();

            List<string> errors = null;
            OpenHosts(null, out errors);

            this.Log.WriteEntry("dp2Kernel OnStart() end",
EventLogEntryType.Information);

            this.m_thread = null;
        }

        // 打开指定的 host
        // parameters:
        //      instance_names  要打开的实例的名字，数组。如果 == null，表示全部打开
        public int OpenHosts(List<string> instance_names,
            out List<string> errors)
        {
            // CloseHosts();

            string strError = "";
            errors = new List<string>();
            int nCount = 0;


            // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
            // return:
            //      -1  检查过程出错
            //      0   没有冲突
            //      1   发生了冲突。报错信息在 strError 中
            int nRet = CheckSqlDbNames(out strError);
            if (nRet != 0)
            {
                strError = "dp2Kernel 实例启动阶段发生严重错误: " + strError;
                errors.Add(strError);
                this.Log.WriteEntry(strError, EventLogEntryType.Error);
                return 0;
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

                if (instance_names != null && instance_names.IndexOf(strInstanceName) == -1)
                    continue;

                // 查重，看看 host 是否已经存在
                if (FindHost(strInstanceName) != null)
                {
                    errors.Add("实例 '" + strInstanceName + "' 调用前已经是启动状态，不能重复启动");
                    continue;
                }
#if NO
                string strWsHostUrl = FindUrl("http", existing_urls);

                string strNamedpipeHostUrl = FindUrl("net.pipe", existing_urls);
                string strNetTcpHostUrl = FindUrl("net.tcp", existing_urls);
#endif

                ServiceHost host = new ServiceHost(typeof(KernelService));
                this.m_hosts.Add(host);

                nCount++;

                HostInfo info = new HostInfo();
                info.DataDir = strDataDir;
                info.InstanceName = strInstanceName;
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
                        strError = "dp2Kernel OnStart() 警告：发现不正确的协议URL '" + url + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。";
                        this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                        errors.Add(strError);
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
                        strError = "dp2Kernel OnStart() 警告：发现不能支持的协议类型 '" + url + "'";
                        this.Log.WriteEntry(strError,
                            EventLogEntryType.Information);
                        errors.Add(strError);
                    }
                }

                // 如果具有ws1/ws2 binding，才启用证书
                if (bHasWsHttp == true/*String.IsNullOrEmpty(strWsHostUrl) == false*/)
                {
                    try
                    {
                        // host.Credentials.ServiceCertificate.Certificate = GetCertificate(strCertSN);
                        X509Certificate2 cert = GetCertificate(strCertSN,
                            out strError);
                        if (cert == null)
                        {
                            strError = "dp2Kernel OnStart() 准备证书 时发生错误: " + strError;
                            this.Log.WriteEntry(strError,
EventLogEntryType.Error);
                            errors.Add(strError);
                        }
                        else
                            host.Credentials.ServiceCertificate.Certificate = cert;

                    }
                    catch (Exception ex)
                    {
                        strError = "dp2Kernel OnStart() 获取证书时发生错误: " + ex.Message;
                        this.Log.WriteEntry(strError,
        EventLogEntryType.Error);
                        errors.Add(strError);
                        return nCount;
                    }
                }

                // 只有第一个host才有metadata能力
                if (// i == 0 
                    m_hosts.Count == 1
                    && host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
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

                    strError = "dp2Kernel OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ex.Message;
                    this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                    errors.Add(strError);
                    return nCount;
                }
            }

            return nCount;
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
            
            try
            {
                StartRemotingServer();
            }
            catch (Exception ex)
            {
                this.Log.WriteEntry("dp2Kernel StartRemotingServer() exception: " + ExceptionUtil.GetDebugText(ex),
EventLogEntryType.Error);
            }


            this.m_thread.Start();
        }

        void m_host_Closing(object sender, EventArgs e)
        {
#if NO
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    host.Extensions.Remove(info);
                    info.Dispose();
                }
            }
#endif
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

            EndRemotingServer();
        }

        #region Windows Service 控制命令设施

        IpcServerChannel m_serverChannel = null;

        void StartRemotingServer()
        {
            // http://www.cnblogs.com/gisser/archive/2011/12/31/2308989.html
            // https://stackoverflow.com/questions/7126733/ipcserverchannel-properties-problem
            // https://stackoverflow.com/questions/2400320/dealing-with-security-on-ipc-remoting-channel
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            Hashtable ht = new Hashtable();
            ht["portName"] = "dp2kernel_ServiceControlChannel";
            ht["name"] = "ipc";
            ht["authorizedGroup"] = "Administrators"; // "Everyone";
            m_serverChannel = new IpcServerChannel(ht, provider);

#if NO
            m_serverChannel = new IpcServerChannel(
                "dp2kernel_ServiceControlChannel");
#endif
            //Register the server channel.
            ChannelServices.RegisterChannel(m_serverChannel, false);

            RemotingConfiguration.ApplicationName = "dp2kernel_ServiceControlServer";

            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServiceControlServer),
                "dp2kernel_ServiceControlServer",
                WellKnownObjectMode.Singleton);
        }

        void EndRemotingServer()
        {
            if (m_serverChannel != null)
            {
                ChannelServices.UnregisterChannel(m_serverChannel);
                m_serverChannel = null;
            }
        }

        #endregion

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
