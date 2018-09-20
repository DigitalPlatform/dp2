// #define USERNAME

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using System.Collections;
using System.IO;

using System.Security.Cryptography.X509Certificates;
// using System.IdentityModel.Selectors;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.IdentityModel.Selectors;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;

using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Threading.Tasks;

namespace dp2Library
{
    public partial class LibraryServiceHost : ServiceBase
    {
        // ServiceHost m_host = null;
        List<ServiceHost> m_hosts = new List<ServiceHost>();

        Thread m_thread = null;

        public EventLog Log = null;

        public LibraryServiceHost()
        {
            InitializeComponent();

            // 初始化事件日志
            this.Log = new EventLog();
            this.Log.Source = "dp2Library";

            this.m_thread = new Thread(new ThreadStart(ThreadMain));
        }

        public static void Main(string[] args)
        {
            // Debugger.Launch();

            if (args.Length == 1 && args[0].Equals("console"))
            {
                ServerInfo.Host = new LibraryServiceHost();
                ServerInfo.Host.ConsoleRun();
                // new LibraryServiceHost().ConsoleRun();
            }
            else if (args.Length == 1 && args[0].Equals("cleartemp"))
            {
                // 清除临时文件目录中的临时文件
                ClearTempFiles();
            }
            else
            {
                ServerInfo.Host = new LibraryServiceHost();
                ServiceBase.Run(ServerInfo.Host);

                // ServiceBase.Run(new LibraryServiceHost());
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
        // 获得绑定的Urls
        public static string[] GetHostUrl(string strProductName)
        {
            // throw new Exception("test rollback");
            // Debug.Assert(false, "");
            using (RegistryKey digitalplatform = Registry.LocalMachine.CreateSubKey("SOFTWARE\\DigitalPlatform"))
            {
                using (RegistryKey dp2kernel = digitalplatform.CreateSubKey(strProductName))
                {
                    if (dp2kernel.GetValue("hosturl") == null)
                        return new string[0];

                    if (dp2kernel.GetValue("hosturl") is string)
                    {
                        string[] results = new string[1]; ;
                        results[0] = (string)dp2kernel.GetValue("hosturl");
                        return results;
                    }

                    return (string[])dp2kernel.GetValue("hosturl");
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
            out string strCertificateSN,
            out string strSerialNumber)
        {
            strInstanceName = "";
            strDataDir = "";
            urls = null;
            strCertificateSN = "";
            strSerialNumber = "";

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

                        strSerialNumber = (string)instance.GetValue("sn");
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

#if NO
        void CloseHosts()
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Dispose();
                    host.Extensions.Remove(info);
                }

                host.Close();
            }

            this.m_hosts.Clear();
        }
#endif

        void CloseHosts()
        {
            List<HostInfo> infos = new List<HostInfo>();
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    infos.Add(info);
                    host.Extensions.Remove(info);
                }

                host.Close();
            }

            this.m_hosts.Clear();

            // 用多线程集中 Dispose()
            if (infos.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (HostInfo info in infos)
                {
                    Task.Run(() => info.Dispose());
                }
                Task.WaitAll(tasks.ToArray());
            }
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
#if NO
        static string GetEnvironmentString(string strSerialCode, string strInstanceName)
        {
            Hashtable table = new Hashtable();
            table["mac"] = SerialCodeForm.GetMacAddress();
            table["time"] = SerialCodeForm.GetTimeRange();
            table["instance"] = strInstanceName;

            table["product"] = "dp2library";

            // string strSerialCode = this.LineInfo.SerialNumber;
            // 将 strSerialCode 中的扩展参数设定到 table 中
            SerialCodeForm.SetExtParams(ref table, strSerialCode);
            return StringUtil.BuildParameterString(table);
        }
#endif

#if SN
        static string GetMaxClients(string strSerialCode)
        {
            string strExtParams = SerialCodeForm.GetExtParams(strSerialCode);
            Hashtable table = StringUtil.ParseParameters(strExtParams);
            return (string)table["clients"];
        }

        static string GetLicenseType(string strSerialCode)
        {
            string strExtParams = SerialCodeForm.GetExtParams(strSerialCode);
            Hashtable table = StringUtil.ParseParameters(strExtParams);
            return (string)table["licensetype"];
        }

        static string GetFunction(string strSerialCode)
        {
            string strExtParams = SerialCodeForm.GetExtParams(strSerialCode);
            Hashtable table = StringUtil.ParseParameters(strExtParams);
            return CanonicalizeFunction((string)table["function"]);
        }

        // 将 function 参数值中的竖线替换为逗号
        static string CanonicalizeFunction(string text)
        {
            if (text == null)
                return "";
            return text.Replace("|", ",");
        }
        // 检查序列号中是否具备某个功能
        static bool CheckFunction(string strSerialCode, string strFunction)
        {
            string strExtParams = SerialCodeForm.GetExtParams(strSerialCode);
            Hashtable table = StringUtil.ParseParameters(strExtParams);
            string strFunctionList = (string)table["function"];
            return StringUtil.IsInList(strFunction, strFunctionList);
        }

        // 将本地字符串匹配序列号
        static bool MatchLocalString(string strSerialNumber,
            string strInstanceName,
            out string strDebugInfo)
        {
            strDebugInfo = "";

            StringBuilder debuginfo = new StringBuilder();
            debuginfo.Append("序列号 '" + strSerialNumber + "' 实例名 '" + strInstanceName + "'\r\n");

            List<string> macs = SerialCodeForm.GetMacAddress();
            debuginfo.Append("本机 MAC 地址: " + StringUtil.MakePathList(macs) + "\r\n");

            debuginfo.Append("第一轮比较:\r\n");

            foreach (string mac in macs)
            {
                string strLocalString = OneInstanceDialog.GetEnvironmentString(mac,
                    strSerialNumber,
                    strInstanceName);

                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                debuginfo.Append("MAC 地址 '" + mac + "' 环境字符串 '" + strLocalString + "' SHA '" + strSha1 + "'\r\n");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                {
                    debuginfo.Append("匹配\r\n");
                    return true;
                }
                debuginfo.Append("不匹配\r\n");
            }

            // 2014/12/19
            if (DateTime.Now.Month == 12)
            {
                debuginfo.Append("第二轮比较:\r\n");
                foreach (string mac in macs)
                {
                    string strLocalString = OneInstanceDialog.GetEnvironmentString(mac,
                        strSerialNumber,
                        strInstanceName,
                        true);
                    string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                    debuginfo.Append("MAC 地址 '" + mac + "' 环境字符串 '" + strLocalString + "' SHA '" + strSha1 + "'\r\n");
                    if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    {
                        debuginfo.Append("匹配\r\n");
                        return true;
                    }
                    debuginfo.Append("不匹配\r\n");
                }
            }

            debuginfo.Append("最后返回不匹配\r\n");

            strDebugInfo = debuginfo.ToString();
            return false;
        }
#endif

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

            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertSN = "";
                string[] existing_urls = null;
                string strSerialNumber = "";
                bool bRet = GetInstanceInfo("dp2Library",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertSN,
                    out strSerialNumber);
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

                int nMaxClients = 0;
                string strLicenseType = "";
                string strFunction = "";

#if SN
                //string strLocalString = OneInstanceDialog.GetEnvironmentString(strSerialNumber, strInstanceName);
                //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (String.IsNullOrEmpty(strSerialNumber) == true
                    || strSerialNumber == "community")
                {
                    nMaxClients = SessionInfo.DEFAULT_MAX_CLIENTS;
                }
                else if (strSerialNumber == "*")
                {
                    nMaxClients = 255;
                }
                else
                {
                    string strDebugInfo = "";
                    if (MatchLocalString(strSerialNumber, strInstanceName, out strDebugInfo) == false)
                    //if (strSha1 != SerialCodeForm.GetCheckCode(strSerialNumber))
                    {
                        strError = "dp2Library 实例 '" + strInstanceName + "' 序列号不合法，无法启动。\r\n调试信息如下：\r\n" + strDebugInfo;
                        this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                        errors.Add(strError);
                        continue;
                    }
                    string strMaxClients = GetMaxClients(strSerialNumber);
                    // 2015/1/7
                    if (string.IsNullOrEmpty(strMaxClients) == true)
                        nMaxClients = SessionInfo.DEFAULT_MAX_CLIENTS;
                    else
                    {
                        if (Int32.TryParse(strMaxClients, out nMaxClients) == false)
                        {
                            strError = "dp2Library 实例 '" + strInstanceName + "' 序列号 '" + strSerialNumber + "' 中 clients 参数值 '" + strMaxClients + "' 不合法，无法启动";
                            this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                            errors.Add(strError);
                            continue;
                        }
                    }

                    strLicenseType = GetLicenseType(strSerialNumber);
                    strFunction = GetFunction(strSerialNumber);
                }
#else
                nMaxClients = 100;
                strLicenseType = "server";
#endif

                ServiceHost host = new ServiceHost(typeof(LibraryService));
                this.m_hosts.Add(host);

                nCount++;

                HostInfo info = new HostInfo();
                info.InstanceName = strInstanceName;
                info.DataDir = strDataDir;
                info.MaxClients = nMaxClients;
                info.LicenseType = strLicenseType;
                info.Function = strFunction;
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
                        strError = "dp2Library OnStart() 警告：发现不正确的协议URL '" + url + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。";
                        this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                        errors.Add(strError);
                        continue;
                    }

                    if (uri.Scheme.ToLower() == "http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryService),
    CreateWsHttpBinding1(),
    url);
                        bHasWsHttp = true;
                    }
                    else if (uri.Scheme.ToLower() == "rest.http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryServiceREST),
CreateWebHttpBinding1(),
url.Substring(5));
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
                    else if (uri.Scheme.ToLower() == "basic.http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryServiceREST),
CreateBasicHttpBinding1(),
url.Substring(6));
#if NO
                        if (endpoint.Behaviors.Find<WebHttpBehavior>() == null)
                        {
                            WebHttpBehavior behavior = new WebHttpBehavior();
                            behavior.DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped;
                            behavior.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                            behavior.AutomaticFormatSelectionEnabled = true;
                            behavior.HelpEnabled = true;
                            endpoint.Behaviors.Add(behavior);
                        }
#endif
                    }
                    else if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        host.AddServiceEndpoint(typeof(ILibraryService),
                            CreateNamedpipeBinding0(),
                            url);
                    }
                    else if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        host.AddServiceEndpoint(typeof(ILibraryService),
            CreateNetTcpBinding0(),
            url);
                    }
                    else
                    {
                        // 警告不能支持的协议
                        strError = "dp2Library OnStart() 警告：发现不能支持的协议类型 '" + url + "'";
                        this.Log.WriteEntry(strError,
                            EventLogEntryType.Information);
                        errors.Add(strError);
                    }

                    info.Protocol = uri.Scheme.ToLower();
                }

                // 如果具有ws1/ws2 binding，才启用证书
                if (bHasWsHttp == true)
                {
                    try
                    {
                        X509Certificate2 cert = GetCertificate(strCertSN,
                            out strError);
                        if (cert == null)
                        {
                            strError = "dp2Library OnStart() 准备证书 时发生错误: " + strError;
                            this.Log.WriteEntry(strError,
EventLogEntryType.Error);
                            errors.Add(strError);
                        }
                        else
                            host.Credentials.ServiceCertificate.Certificate = cert;

                    }
                    catch (Exception ex)
                    {
                        strError = "dp2Library OnStart() 获取证书时发生错误: " + ExceptionUtil.GetExceptionMessage(ex);
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
                        strMetadataUrl = "http://localhost:8001/dp2library/";
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

#if NO
                host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new MyValidator();
#endif

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

                    strError = "dp2Library OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ExceptionUtil.GetExceptionMessage(ex);
                    this.Log.WriteEntry(strError,
    EventLogEntryType.Error);
                    errors.Add(strError);
                    return nCount;
                }
            }

            return nCount;
        }

        void ThreadMain()
        {
            CloseHosts();

            List<string> errors = null;
            OpenHosts(null, out errors);

#if NO
            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertSN = "";
                string[] existing_urls = null;
                string strSerialNumber = "";
                bool bRet = GetInstanceInfo("dp2Library",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertSN,
                    out strSerialNumber);
                if (bRet == false)
                    break;

                int nMaxClients = 0;
                string strLicenseType = "";
                string strFunction = "";

#if SN
                //string strLocalString = OneInstanceDialog.GetEnvironmentString(strSerialNumber, strInstanceName);
                //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (String.IsNullOrEmpty(strSerialNumber) == true
                    || strSerialNumber == "community")
                {
                    nMaxClients = SessionInfo.DEFAULT_MAX_CLIENTS;
                }
                else if (strSerialNumber == "*")
                {
                    nMaxClients = 255;
                }
                else
                {
                    string strDebugInfo = "";
                    if (MatchLocalString(strSerialNumber, strInstanceName, out strDebugInfo) == false)
                    //if (strSha1 != SerialCodeForm.GetCheckCode(strSerialNumber))
                    {
                        this.Log.WriteEntry("dp2Library 实例 '" + strInstanceName + "' 序列号不合法，无法启动。\r\n调试信息如下：\r\n" + strDebugInfo,
    EventLogEntryType.Error);
                        continue;
                    }
                    string strMaxClients = GetMaxClients(strSerialNumber);
                    // 2015/1/7
                    if (string.IsNullOrEmpty(strMaxClients) == true)
                        nMaxClients = SessionInfo.DEFAULT_MAX_CLIENTS;
                    else
                    {
                        if (Int32.TryParse(strMaxClients, out nMaxClients) == false)
                        {
                            this.Log.WriteEntry("dp2Library 实例 '" + strInstanceName + "' 序列号 '" + strSerialNumber + "' 中 clients 参数值 '" + strMaxClients + "' 不合法，无法启动",
    EventLogEntryType.Error);
                            continue;
                        }
                    }

                    strLicenseType = GetLicenseType(strSerialNumber);
                    strFunction = GetFunction(strSerialNumber);
                }
#else
                nMaxClients = 100;
                strLicenseType = "server";
#endif

                ServiceHost host = new ServiceHost(typeof(LibraryService));
                this.m_hosts.Add(host);

                HostInfo info = new HostInfo();
                info.InstanceName = strInstanceName;
                info.DataDir = strDataDir;
                info.MaxClients = nMaxClients;
                info.LicenseType = strLicenseType;
                info.Function = strFunction;
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
                        this.Log.WriteEntry("dp2Library OnStart() 警告：发现不正确的协议URL '" + url + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。",
    EventLogEntryType.Error);
                        continue;
                    }

                    if (uri.Scheme.ToLower() == "http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryService),
    CreateWsHttpBinding1(),
    url);
                        bHasWsHttp = true;
                    }
                    else if (uri.Scheme.ToLower() == "rest.http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryServiceREST),
CreateWebHttpBinding1(),
url.Substring(5));
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
                    else if (uri.Scheme.ToLower() == "basic.http")
                    {
                        ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(ILibraryServiceREST),
CreateBasicHttpBinding1(),
url.Substring(6));
#if NO
                        if (endpoint.Behaviors.Find<WebHttpBehavior>() == null)
                        {
                            WebHttpBehavior behavior = new WebHttpBehavior();
                            behavior.DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped;
                            behavior.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
                            behavior.AutomaticFormatSelectionEnabled = true;
                            behavior.HelpEnabled = true;
                            endpoint.Behaviors.Add(behavior);
                        }
#endif
                    }
                    else if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        host.AddServiceEndpoint(typeof(ILibraryService),
                            CreateNamedpipeBinding0(),
                            url);
                    }
                    else if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        host.AddServiceEndpoint(typeof(ILibraryService),
            CreateNetTcpBinding0(),
            url);
                    }
                    else
                    {
                        // 警告不能支持的协议
                        this.Log.WriteEntry("dp2Library OnStart() 警告：发现不能支持的协议类型 '" + url + "'",
                            EventLogEntryType.Information);
                    }

                    info.Protocol = uri.Scheme.ToLower();
                }

                // 如果具有ws1/ws2 binding，才启用证书
                if (bHasWsHttp == true)
                {
                    try
                    {
                        string strError = "";
                        X509Certificate2 cert = GetCertificate(strCertSN,
                            out strError);
                        if (cert == null)
                            this.Log.WriteEntry("dp2Library OnStart() 准备证书 时发生错误: " + strError,
EventLogEntryType.Error);
                        else
                            host.Credentials.ServiceCertificate.Certificate = cert;

                    }
                    catch (Exception ex)
                    {
                        this.Log.WriteEntry("dp2Library OnStart() 获取证书时发生错误: " + ExceptionUtil.GetExceptionMessage(ex),
        EventLogEntryType.Error);
                        return;
                    }
                }

                // 只有第一个host才有metadata能力
                if (i == 0 && host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                {
                    string strWsHostUrl = FindUrl("http", existing_urls);

                    string strMetadataUrl = strWsHostUrl;
                    if (String.IsNullOrEmpty(strMetadataUrl) == true)
                        strMetadataUrl = "http://localhost:8001/dp2library/";
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

#if NO
                host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new MyValidator();
#endif

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

                    this.Log.WriteEntry("dp2Library OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ExceptionUtil.GetExceptionMessage(ex),
    EventLogEntryType.Error);
                    return;
                }
            }
#endif

            this.Log.WriteEntry("dp2Library OnStart() end",
EventLogEntryType.Information);

            this.m_thread = null;
        }

        static void SetTimeout(System.ServiceModel.Channels.Binding binding)
        {
            binding.SendTimeout = new TimeSpan(0, 20, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 20, 0);    // 决定Session存活
            binding.CloseTimeout = new TimeSpan(0, 20, 0);
            binding.OpenTimeout = new TimeSpan(0, 20, 0);
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
            SetTimeout(binding);
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
            SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        // ws0: windows -- ClientCredentitialType = Windows
        System.ServiceModel.Channels.Binding CreateWsHttpBinding0()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            // binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        // ws1: anonymouse -- ClientCredentitialType = None
        System.ServiceModel.Channels.Binding CreateWsHttpBinding1()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = SecurityMode.Message;
#if !USERNAME
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
#else
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
#endif
            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.Enabled = false;

            binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        System.ServiceModel.Channels.Binding CreateWebHttpBinding1()
        {
            WebHttpBinding binding = new WebHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = WebHttpSecurityMode.None;
            // binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        // 2013/11/1
        // Basic HTTP
        System.ServiceModel.Channels.Binding CreateBasicHttpBinding1()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Namespace = "http://dp2003.com/dp2library/";
            binding.Security.Mode = BasicHttpSecurityMode.None;
            // binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            SetTimeout(binding);

            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.InactivityTimeout = new TimeSpan(0, 20, 0);

            return binding;
        }

        public static string GetProductString(string strProductName,
    string strEntryName)
        {
            // throw new Exception("test rollback");
            // Debug.Assert(false, "");
            using (RegistryKey digitalplatform = Registry.LocalMachine.CreateSubKey("SOFTWARE\\DigitalPlatform"))
            {
                using (RegistryKey product = digitalplatform.CreateSubKey(strProductName))
                {
                    if (product.GetValue(strEntryName) == null)
                        return null;

                    if (product.GetValue(strEntryName) is string)
                    {
                        return (string)product.GetValue(strEntryName);
                    }

                    return null;
                }
            }
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
#if NO
                X509Certificate2 cert = FindCertificate(
StoreLocation.LocalMachine,
StoreName.Root,
X509FindType.FindBySerialNumber,
strCertSN);
                if (cert != null)
                    return cert;

                cert = FindCertificate(
StoreLocation.CurrentUser,
StoreName.Root,
X509FindType.FindBySerialNumber,
strCertSN);
                if (cert == null)
                {
                    strError = "序列号为 '" + strCertSN + "' 的证书在 StoreLocation.LocalMachine | StoreLocation.CurrentUser / StoreName.Root 中不存在。";
                    return null;
                }

                return cert;
#endif
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

        protected override void OnStart(string[] args)
        {
            this.Log.WriteEntry("dp2Library OnStart() begin",
    EventLogEntryType.Information);

            try
            {
                StartRemotingServer();
            }
            catch (Exception ex)
            {
                this.Log.WriteEntry("dp2Library StartRemotingServer() exception: " + ExceptionUtil.GetDebugText(ex),
EventLogEntryType.Error);
            }

            this.m_thread.Start();
        }

        void m_host_Closing(object sender, EventArgs e)
        {
            // 错误地移走了全部 HostInfo
#if NO
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    info.Dispose();
                    host.Extensions.Remove(info);
                }
            }
#endif
        }

        // 设置各个 host 的最大前端数
        // parameters:
        //      strDefinition   前端数的定义字符串。格式为 'http:5,net.tcp:10,basic.http:20'。如果只绑定了一个协议，也可以定义为 '20'
        public int SetMaxClients()
        {
            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    if (info.App != null)
                        info.App.MaxClients = info.MaxClients;
                }
            }

            return 0;
        }

#if NO
        // 设置各个 host 的最大前端数
        // parameters:
        //      strDefinition   前端数的定义字符串。格式为 'http:5,net.tcp:10,basic.http:20'。如果只绑定了一个协议，也可以定义为 '20'
        public int SetMaxClients(string strDefinition,
            out string strError)
        {
            strError = "";

            foreach (ServiceHost host in this.m_hosts)
            {
                HostInfo info = host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    // 每取得一次，就从中删除已经取得的局部
                    // return:
                    //      -1  出错
                    //      其他 取得的数字
                    int nRet = GetMaxValue(info.Protocol,
            ref strDefinition,
            out strError);
                    if (nRet == -1)
                        return -1;

                    info.MaxClients = nRet;
                    if (info.App != null)
                        info.App.MaxClients = nRet;
                }
            }

            return 0;
        }

        // 每取得一次，就从中删除已经取得的局部
        // return:
        //      -1  出错
        //      其他 取得的数字
        static int GetMaxValue(string strProtocol, 
            ref string strDefinition,
            out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder();

            int nRet = -1;
            string[] segments = strDefinition.Split(new char[] { ',' });
            foreach (string s in segments)
            {
                string strSegment = s;
                if (string.IsNullOrEmpty(strSegment) == true)
                    continue;
                strSegment = strSegment.Trim();
                if (string.IsNullOrEmpty(strSegment) == true)
                    continue;
                string strProtocolPart = "";
                string strValuePart = "";
                StringUtil.ParseTwoPart(strSegment,
                    ":",
                    out strProtocolPart,
                    out strValuePart);
                if (strProtocol == strProtocolPart
                    && nRet == -1)
                {
                    if (Int32.TryParse(strValuePart, out nRet) == false)
                    {
                        strError = "通道数定义字符串 '"+strDefinition+"' 格式不合法。 '"+strValuePart+"' 应该是一个数字";
                        return -1;
                    }
                    continue;
                }

                if (text.Length > 0)
                    text.Append(",");
                text.Append(strSegment);
            }

            strDefinition = text.ToString();
            if (nRet == -1)
                return 0;

            return nRet;
        }
#endif

        void host_Opening(object sender, EventArgs e)
        {
#if NO
            lock (GlobalVars.LockObject)
            {
                if (GlobalVars.LibraryApplication == null)
                {
                    string strError = "";
                    int nRet = LibraryService.InitialApplication(out strError);
                    if (nRet == -1)
                    {
                        this.Log.WriteEntry("host 预先初始化 LibraryService 失败: " + strError, 
                            EventLogEntryType.Error);
                        GlobalVars.LibraryApplication = null;
                    }
                }
                // Debug.Assert(GlobalVars.LibraryApplication != null, "");
            }
#endif
        }

        protected override void OnStop()
        {
            CloseHosts();

            if (this.m_thread != null)
            {
                this.m_thread.Abort();
                this.m_thread = null;
            }

            EndRemotingServer();
        }

        // 清除以前残留的临时文件
        static void ClearTempFiles()
        {
            string strTempFileName = Path.GetTempFileName();
            File.Delete(strTempFileName);
            string strTempDir = Path.GetDirectoryName(strTempFileName);

            int nCount = 0;
            DirectoryInfo di = new DirectoryInfo(strTempDir);
            FileInfo[] fis = di.GetFiles("*.tmp");
            foreach (FileInfo fi in fis)
            {
                try
                {
                    Console.WriteLine(fi.FullName);
                    File.Delete(fi.FullName);
                    nCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("删除出现异常: " + ex.Message);
                }
            }
            Console.WriteLine("共删除 " + nCount.ToString() + " 个临时文件");
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
            ht["portName"] = "dp2library_ServiceControlChannel";
            ht["name"] = "ipc";
            ht["authorizedGroup"] = "Administrators"; // "Everyone";
            m_serverChannel = new IpcServerChannel(ht, provider);

#if NO
            m_serverChannel = new IpcServerChannel(
                "dp2library_ServiceControlChannel");
#endif
            //Register the server channel.
            ChannelServices.RegisterChannel(m_serverChannel, false);

            RemotingConfiguration.ApplicationName = "dp2library_ServiceControlServer";

            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServiceControlServer),
                "dp2library_ServiceControlServer",
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

    public class MyValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            int i = 0;
            i++;
        }
    }
}
