using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Description;

using Microsoft.Win32;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;

namespace dp2ZServer
{
    public partial class Service : ServiceBase
    {
        ServiceHost m_hostUnionCatalog = null;
        Thread m_threadLoadUnionCatalog = null;
        bool m_bConsoleRun = false;

        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号

        private TcpListener Listener = null;

        private string m_IPAddress = "ALL";   // Holds IP Address, which to listen incoming calls.
        private int m_port = 210;      // 端口号
        private int m_nMaxThreads = -1;      // Holds maximum allowed Worker Threads.

        private Hashtable m_SessionTable = null;


        public XmlDocument CfgDom = null;   // z.xml配置文件内容
        public string LibraryServerUrl = "";
        public string ManagerUserName = ""; // 管理员用的用户名
        public string ManagerPassword = ""; // 管理员用的密码

        public string AnonymousUserName = "";   // 匿名登录用的用户名
        public string AnonymousPassword = "";   // 匿名登录用的密码

        string EncryptKey = "dp2zserver_password_key";

        public EventLog Log = null;


        // 专用来获得系统信息的dp2library通道
        public LibraryChannel Channel = new LibraryChannel();

        public List<BiblioDbProperty> BiblioDbProperties = null;

        // 所有库的总共检索命中记录条数极限
        public int MaxResultCount = -1;

        public Service()
        {
            InitializeComponent();

            // 初始化事件日志
            this.Log = new EventLog();
            this.Log.Source = "dp2ZServer";

            this.m_threadLoadUnionCatalog = new Thread(new ThreadStart(ThreadLoadUnionCatalog));
        }

        public static void Main(string[] args)
        {
            if (args.Length == 1 && args[0].Equals("console"))
            {
                new Service().ConsoleRun();
            }
            else
            {
                ServiceBase.Run(new Service());
            }
        }

        private void ConsoleRun()
        {
            this.m_bConsoleRun = true;

            Console.WriteLine("{0}::starting...", GetType().FullName);

            OnStart(null);

            Console.WriteLine("{0}::ready (ENTER to exit)", GetType().FullName);

            Console.ReadLine();

            OnStop();
            Console.WriteLine("{0}::stopped", GetType().FullName);
        }

        protected override void OnStart(string[] args)
        {
            this.Log.WriteEntry("dp2ZServer OnStart() begin",
    EventLogEntryType.Information);
            
            try
            {
                if (!this.DesignMode)
                {
                    m_SessionTable = new Hashtable();

                    Thread startZ3950Server = new Thread(new ThreadStart(Run));
                    startZ3950Server.Start();

                    Thread defaultManagerThread = new Thread(new ThreadStart(ManagerRun));
                    defaultManagerThread.Start();
                }
            }
            catch (Exception x)
            {
                Log.WriteEntry("dp2ZServer OnStart() error : " + x.Message, 
                    EventLogEntryType.Error);
            }
            /*
关于dp2Zserver自动启动在系统日志中报错的问题：
             * 
http://www.devnewsgroups.net/group/microsoft.public.dotnet.framework.windowsforms/topic15625.aspx
...
Hi,

We had the very same problem - .NET service failed to start
automatically during machine reboot (timeout) but started without
problems when started later manually.
This seems to be a potential problem for any .NET service on a slower
machine (or heavy loaded one) - it just takes some time for CLR to
translate from MSIL to native code and after restart many other
services are being started simultaneously, so sometimes the default
timeout of 30 seconds is not enough :(

The solution was quite simple: we increased the timeout in registry
(HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ServicesPipeTimeout) 注意这是一个DWORD类型事项
and since then our service starts without problems :)
             * */

            // UnionCatalog
            this.m_threadLoadUnionCatalog.Start();

            this.Log.WriteEntry("dp2ZServer OnStart() end",
EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            try
            {
                if (Listener != null)
                {
                    Listener.Stop();
                }
            }
            catch (Exception x)
            {
                Log.WriteEntry("dp2ZServer OnStop() error : " + x.Message,
                    EventLogEntryType.Error);
            }

            // UnionCatalog
            if (this.m_threadLoadUnionCatalog != null)
            {
                this.m_threadLoadUnionCatalog.Abort();
                this.m_threadLoadUnionCatalog = null;
            }

            if (this.m_hostUnionCatalog != null)
            {
                this.m_hostUnionCatalog.Close();
                this.m_hostUnionCatalog = null;
            }
        }

        #region UnionCatalog

        void ThreadLoadUnionCatalog()
        {
            if (this.m_hostUnionCatalog != null)
            {
                this.m_hostUnionCatalog.Close();
                this.m_hostUnionCatalog = null;
            }

            string strInstanceName = "";
            string strDataDir = "";
            string[] existing_urls = null;
            bool bRet = GetInstanceInfo("dp2ZServer",
                0,
                out strInstanceName,
                out strDataDir,
                out existing_urls);
            if (bRet == false)
            {
                /*
                this.Log.WriteEntry("dp2ZServer OnStart() 时发生错误: 注册表中找不到instance信息",
EventLogEntryType.Error);
                 * */
                return;
            }

            string strHttpHostUrl = FindUrl("http", existing_urls);
            if (string.IsNullOrEmpty(strHttpHostUrl) == true)
            {
                string strUrls = string.Join(";", existing_urls);
                this.Log.WriteEntry("dp2ZServer OnStart() 时发生错误: 协议绑定 '"+strUrls+"' 中，没有包含http协议，因此没有启动UnionCatalogService",
EventLogEntryType.Error);
                return;
            }

            this.m_hostUnionCatalog = new ServiceHost(typeof(UnionCatalogService));

            HostInfo info = new HostInfo();
            info.DataDir = strDataDir;
            this.m_hostUnionCatalog.Extensions.Add(info);

            if (String.IsNullOrEmpty(strHttpHostUrl) == false)
            {
                this.m_hostUnionCatalog.AddServiceEndpoint(typeof(IUnionCatalogService),
                    CreateBasicHttpBinding0(),
                    strHttpHostUrl);
            }

            // metadata能力
            if (this.m_hostUnionCatalog.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
            {
                string strMetadataUrl = strHttpHostUrl;
                if (String.IsNullOrEmpty(strMetadataUrl) == true)
                    strMetadataUrl = "http://localhost/unioncatalog/";
                if (strMetadataUrl[strMetadataUrl.Length - 1] != '/')
                    strMetadataUrl += "/";
                strMetadataUrl += "metadata";

                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                behavior.HttpGetEnabled = true;
                behavior.HttpGetUrl = new Uri(strMetadataUrl);
                this.m_hostUnionCatalog.Description.Behaviors.Add(behavior);
            }

            if (this.m_hostUnionCatalog.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
            {
                ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior();
                behavior.MaxConcurrentCalls = 50;
                behavior.MaxConcurrentInstances = 1000;
                behavior.MaxConcurrentSessions = 1000;
                this.m_hostUnionCatalog.Description.Behaviors.Add(behavior);
            }

            // IncludeExceptionDetailInFaults
            ServiceDebugBehavior debug_behavior = this.m_hostUnionCatalog.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debug_behavior == null)
            {
                this.m_hostUnionCatalog.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
            }
            else
            {
                if (debug_behavior.IncludeExceptionDetailInFaults == false)
                    debug_behavior.IncludeExceptionDetailInFaults = true;
            }

            this.m_hostUnionCatalog.Opening += new EventHandler(host_Opening);
            this.m_hostUnionCatalog.Closing += new EventHandler(m_host_Closing);

            try
            {
                this.m_hostUnionCatalog.Open();
            }
            catch (Exception ex)
            {
                // 让调试器能感觉到
                if (this.m_bConsoleRun == true)
                    throw ex;

                this.Log.WriteEntry("dp2ZServer OnStart() host.Open() 时发生错误: " + ex.Message,
EventLogEntryType.Error);
                return;
            }

            this.Log.WriteEntry("dp2ZServer OnStart() end",
EventLogEntryType.Information);

            this.m_threadLoadUnionCatalog = null;
        }

        void m_host_Closing(object sender, EventArgs e)
        {

        }

        void host_Opening(object sender, EventArgs e)
        {

        }

        // bs0: 
        System.ServiceModel.Channels.Binding CreateBasicHttpBinding0()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Namespace = "http://dp2003.com/unioncatalog/";
            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.SendTimeout = new TimeSpan(0, 20, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 20, 0);    // 决定Session存活

            return binding;
        }

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
            out string[] urls)
        {
            strInstanceName = "";
            strDataDir = "";
            urls = null;

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

        #endregion

        // 装载配置文件dp2zserver.xml
        int LoadCfgDom(out string strError)
        {
            lock (this)
            {
                strError = "";
                int nRet = 0;

                /*
                string strDir = Directory.GetCurrentDirectory();

                strDir = PathUtil.MergePath(strDir, "dp2zserver");
                 * */
                string strCurrentDir = System.Reflection.Assembly.GetExecutingAssembly().Location;   //  Environment.CurrentDirectory;

                strCurrentDir = PathUtil.PathPart(strCurrentDir);


                string strFileName = PathUtil.MergePath(strCurrentDir, "dp2zserver.xml");

                this.CfgDom = new XmlDocument();

                try
                {
                    this.CfgDom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    strError = "将配置文件 '" + strFileName + "' 装载到DOM时出错: " + ex.Message;
                    return -1;
                }


                // 取得网络参数
                XmlNode nodeNetwork = this.CfgDom.DocumentElement.SelectSingleNode("//network");
                if (nodeNetwork != null)
                {
                    // port

                    // 获得整数型的属性参数值
                    // return:
                    //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                    //      0   正常获得明确定义的参数值
                    //      1   参数没有定义，因此代替以缺省参数值返回
                    nRet = DomUtil.GetIntegerParam(nodeNetwork,
                        "port",
                        210,
                        out this.m_port,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "<network>元素" + strError;
                        return -1;
                    }

                    // maxSessions

                    // 获得整数型的属性参数值
                    // return:
                    //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                    //      0   正常获得明确定义的参数值
                    //      1   参数没有定义，因此代替以缺省参数值返回
                    nRet = DomUtil.GetIntegerParam(nodeNetwork,
                        "maxSessions",
                        -1,
                        out this.m_nMaxThreads,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "<network>元素" + strError;
                        return -1;
                    }

                }

                // 取出一些常用的指标

                // 1) 图书馆应用服务器URL
                // 2) 管理员用的帐户和密码
                XmlNode node = this.CfgDom.DocumentElement.SelectSingleNode("//libraryserver");
                if (node != null)
                {
                    this.LibraryServerUrl = DomUtil.GetAttr(node, "url");

                    this.ManagerUserName = DomUtil.GetAttr(node, "username");
                    string strPassword = DomUtil.GetAttr(node, "password");
                    this.ManagerPassword = DecryptPasssword(strPassword);

                    this.AnonymousUserName = DomUtil.GetAttr(node, "anonymousUserName");
                    strPassword = DomUtil.GetAttr(node, "anonymousPassword");
                    this.AnonymousPassword = DecryptPasssword(strPassword);
                }
                else
                {
                    this.LibraryServerUrl = "";

                    this.ManagerUserName = "";
                    this.ManagerUserName = "";

                    this.AnonymousUserName = "";
                    this.AnonymousPassword = "";
                }


                // 准备通道
                this.Channel.Url = this.LibraryServerUrl;

                this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

                //

                XmlNode nodeDatabases = this.CfgDom.DocumentElement.SelectSingleNode("databases");
                if (nodeDatabases != null)
                {
                    // maxResultCount

                    // 获得整数型的属性参数值
                    // return:
                    //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                    //      0   正常获得明确定义的参数值
                    //      1   参数没有定义，因此代替以缺省参数值返回
                    nRet = DomUtil.GetIntegerParam(nodeDatabases,
                        "maxResultCount",
                        -1,
                        out this.MaxResultCount,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "<databases>元素" + strError;
                        return -1;
                    }

                }

                return 0;
            }
        }

        // 获得一些比较耗时的配置参数。
        // return:
        //      -2  出错。但后面可以重试
        //      -1  出错，后面不再重试
        //      0   成功
        int GetSlowCfgInfo(out string strError)
        {
            lock (this)
            {
                strError = "";
                int nRet = 0;

                // 预先获得编目库属性列表
                nRet = GetBiblioDbProperties(out strError);
                if (nRet == -1)
                    return -2;

                // 为数据库属性集合中增补需要从xml文件中获得的其他属性
                nRet = AppendDbProperties(out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this.ManagerUserName;
                e.Password = this.ManagerPassword;
                e.Parameters = "location=z39.50 server manager,type=worker";
                /*
                e.IsReader = false;
                e.Location = "z39.50 server manager";
                 * */
                if (String.IsNullOrEmpty(e.UserName) == true)
                {
                    e.ErrorInfo = "没有指定管理用户名，无法自动登录";
                    e.Failed = true;
                    return;
                }

                return;
            }

            e.ErrorInfo = "z39.50 service first try失败后，无法自动登录";
            e.Failed = true;
            return;
        }

        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }

            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, this.EncryptKey);
        }

        DateTime m_lastRetryTime = DateTime.Now;
        int m_nRetryAfterMinutes = 5;   // 每间隔多少分钟以后重试一次

        private void ManagerRun()
        {
            REDO:
            try
            {
                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (true)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, 1000, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        return;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                    }
                    else if (index == 0)
                    {
                        // Close
                        return;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                    }

                    List<String> keys = new List<string>();

                    // 锁定的时间要短
                    lock (this.m_SessionTable)
                    {
                        foreach (string key in this.m_SessionTable.Keys)
                        {
                            keys.Add(key);
                        }
                    }

                    // 在锁定范围外从容处理
                    foreach (string key in keys)
                    {
                        Session session = (Session)this.m_SessionTable[key];
                        if (session == null)
                            continue;

                        TimeSpan delta = DateTime.Now - session.ActivateTime;
                        if (delta.TotalMinutes > 5) // 20
                            RemoveSession(key);
                    }

                    if (this.BiblioDbProperties == null
                        && (DateTime.Now - this.m_lastRetryTime).TotalMinutes >= m_nRetryAfterMinutes)  // 每间隔五分钟以上再重试一次
                    {
                        string strError = "";
                        // return:
                        //      -2  出错。但后面可以重试
                        //      -1  出错，后面不再重试
                        //      0   成功
                        int nRet = GetSlowCfgInfo(out strError);
                        if (nRet == -1 || nRet == -2)
                        {
                            Log.WriteEntry("ERR003 初始化信息失败(系统将定期重试): " + strError,
                                EventLogEntryType.Error);
                        }

                        m_lastRetryTime = DateTime.Now;
                        m_nRetryAfterMinutes++; // 将重试的间隔时逐渐变长。此举可以在重试多次不成功的情况下，避免在日志文件中写入过多的条目
                    }

                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Manager Thread exception: " + ex.Message,
                    EventLogEntryType.Error);
                goto REDO;
            }
        }

        private void Run()
        {
            eventClose.Reset();
            try
            {
                string strError = "";

                // Log.WriteEntry("dp2ZServer service start step 1");

                // 装载配置文件
                int nRet = LoadCfgDom(out strError);
                if (nRet == -1)
                {
                    Log.WriteEntry("dp2ZServer error : " + strError,
                        EventLogEntryType.Error);
                    return;
                }

                // return:
                //      -2  出错。但后面可以重试
                //      -1  出错，后面不再重试
                //      0   成功
                nRet = GetSlowCfgInfo(out strError);
                if (nRet == -1)
                {
                    Log.WriteEntry("ERR001 首次初始化信息失败(系统不再重试): " + strError,
                        EventLogEntryType.Error);
                    return;
                } 
                if (nRet == -2)
                {
                    Log.WriteEntry("ERR002 首次初始化信息失败(系统将定期重试): " + strError,
                        EventLogEntryType.Error);
                }

                // check which ip's to listen (all or assigned)
                if (m_IPAddress.ToLower().IndexOf("all") > -1)
                {
                    Listener = new TcpListener(IPAddress.Any, m_port);
                }
                else
                {
                    Listener = new TcpListener(IPAddress.Parse(m_IPAddress), m_port);
                }

                // Log.WriteEntry("dp2ZServer service start step 3");


                // Start listening
                Listener.Start();


                //-------- Main Server message loop --------------------------------//
                while (true)
                {
                    // Check if maximum allowed thread count isn't exceeded
                    if (this.m_nMaxThreads == -1
                        || m_SessionTable.Count <= this.m_nMaxThreads)
                    {

                        // Thread is sleeping, until a client connects
                        TcpClient client = Listener.AcceptTcpClient();

                        string sessionID = client.GetHashCode().ToString();

                        //****
                        // _LogWriter logWriter = new _LogWriter(this.SessionLog);
                        Session session = new Session(client, this, sessionID);

                        Thread clientThread = new Thread(new ThreadStart(session.Processing));

                        // Add session to session list
                        AddSession(sessionID, session);

                        // Start proccessing
                        clientThread.Start();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                // string dummy = e.Message;     // Neede for to remove compile warning

                Thread.CurrentThread.Abort();
            }
            catch (Exception x)
            {
                // 一个封锁操作被对 WSACancelBlockingCall 的调用中断
                if (x.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                {

                    Log.WriteEntry("dp2ZServer Exception Name: " + x.GetType().Name + ", Listener::Run() error : " + x.Message,
                        EventLogEntryType.Error);
                }
            }
            finally
            {
                eventClose.Set();
            }
        }

        /// <summary>
        /// Removes session.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="logWriter">Log writer.</param>
        internal void RemoveSession(string sessionID)
        {
            lock (m_SessionTable)
            {
                if (!m_SessionTable.Contains(sessionID))
                {
                    // OnSysError(new Exception("Session '" + sessionID + "' doesn't exist."),new System.Diagnostics.StackTrace());
                    return;
                }

                Session session = (Session)m_SessionTable[sessionID];
                if (session != null)
                {
                    session.Dispose();
                }

                m_SessionTable.Remove(sessionID);
            }

            /*
            if(m_LogCmds)
            {
                logWriter.AddEntry("//----- Sys: 'Session:'" + sessionID + " removed " + DateTime.Now);
            }
            */
        }

        /// <summary>
        /// Adds session.
        /// </summary>
        /// <param name="sessionID">Session ID.</param>
        /// <param name="session">Session object.</param>
        internal void AddSession(string sessionID,
            Session session)
        {
            lock (m_SessionTable)
            {
                m_SessionTable.Add(sessionID, session);
            }

            /*
            if(m_LogCmds)
            {
                logWriter.AddEntry("//----- Sys: 'Session:'" + sessionID + " added " + DateTime.Now);
            }
            */
        }


        // 获得编目库属性列表
        int GetBiblioDbProperties(out string strError)
        {
            strError = "";
            try
            {
                this.BiblioDbProperties = new List<BiblioDbProperty>();

                string strValue = "";
                long lRet = Channel.GetSystemParameter(null,
                    "biblio",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得编目库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] biblioDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < biblioDbNames.Length; i++)
                {
                    BiblioDbProperty property = new BiblioDbProperty();
                    property.DbName = biblioDbNames[i];
                    this.BiblioDbProperties.Add(property);
                }

                // 获得语法格式
                lRet = Channel.GetSystemParameter(null,
                    "biblio",
                    "syntaxs",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得编目库数据格式列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] syntaxs = strValue.Split(new char[] { ',' });

                if (syntaxs.Length != this.BiblioDbProperties.Count)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而数据格式为 " + syntaxs.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].Syntax = syntaxs[i];
                }


                ///

                // 获得对应的实体库名
                lRet = Channel.GetSystemParameter(null,
                    "item",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得实体库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] itemdbnames = strValue.Split(new char[] { ',' });

                if (itemdbnames.Length != this.BiblioDbProperties.Count)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而实体库名为 " + itemdbnames.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].ItemDbName = itemdbnames[i];
                }


                // 获得虚拟数据库名
                lRet = Channel.GetSystemParameter(null,
                    "virtual",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + Channel.Url + " 获得虚拟库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }
                string[] virtualDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < virtualDbNames.Length; i++)
                {
                    BiblioDbProperty property = new BiblioDbProperty();
                    property.DbName = virtualDbNames[i];
                    property.IsVirtual = true;
                    this.BiblioDbProperties.Add(property);
                }


            }
            finally
            {
            }

            return 0;
        ERROR1:
            this.BiblioDbProperties = null;
            return -1;
        }

        // 为数据库属性集合中增补需要从xml文件中获得的其他属性
        int AppendDbProperties(out string strError)
        {
            strError = "";

            // 增补MaxResultCount
            if (this.CfgDom == null)
            {
                strError = "调用 GetBiblioDbProperties()以前，需要先初始化和装载CfgDom";
                return -1;
            }

            Debug.Assert(this.CfgDom != null, "");

            for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty prop = this.BiblioDbProperties[i];

                string strDbName = prop.DbName;

                XmlNode nodeDatabase = this.CfgDom.DocumentElement.SelectSingleNode("//databases/database[@name='"+strDbName+"']");
                if (nodeDatabase == null)
                    continue;

                // maxResultCount

                // 获得整数型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                int nRet = DomUtil.GetIntegerParam(nodeDatabase,
                    "maxResultCount",
                    -1,
                    out prop.MaxResultCount,
                    out strError);
                if (nRet == -1)
                {
                    strError = "为数据库 '" + strDbName + "' 配置的<databases/database>元素的" + strError;
                    return -1;
                }

                // alias
                prop.DbNameAlias = DomUtil.GetAttr(nodeDatabase, "alias");


                // addField901
                // 2007/12/16
                nRet = DomUtil.GetBooleanParam(nodeDatabase,
                    "addField901",
                    false,
                    out prop.AddField901,
                    out strError);
                if (nRet == -1)
                {
                    strError = "为数据库 '" + strDbName + "' 配置的<databases/database>元素的" + strError;
                    return -1;
                }
            }


            return 0;
        }

        // 根据书目库名获得书目库属性对象
        public BiblioDbProperty GetDbProperty(string strBiblioDbName,
            bool bSearchAlias)
        {
            for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
                if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                {
                    return this.BiblioDbProperties[i];
                }

                if (bSearchAlias == true)
                {
                    if (this.BiblioDbProperties[i].DbNameAlias.ToLower() == strBiblioDbName.ToLower())
                    {
                        return this.BiblioDbProperties[i];
                    }
                }

            }

            return null;
        }


        // 根据书目库名获得MARC格式语法名
        public string GetMarcSyntax(string strBiblioDbName)
        {
            for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
                if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                {
                    string strResult = this.BiblioDbProperties[i].Syntax;
                    if (String.IsNullOrEmpty(strResult) == true)
                        strResult = "unimarc";  // 缺省为unimarc
                    return strResult;
                }
            }

            // 2007/8/9
            // 如果在this.BiblioDbProperties里面找不到，可以直接在xml配置的<database>元素中找
            XmlNode nodeDatabase = this.CfgDom.DocumentElement.SelectSingleNode("//databases/database[@name='" + strBiblioDbName + "']");
            if (nodeDatabase == null)
                return null;

            return DomUtil.GetAttr(nodeDatabase, "marcSyntax");
        }

        // 根据书目库名(或者别名)获得检索途径名
        // parameters:
        //      strOutputDbName 输出的数据库名。不是Z39.50服务器定义的别名，而是正规数据库名。
        public string GetFromName(string strDbNameOrAlias,
            long lAttributeValue,
            out string strOutputDbName,
            out string strError)
        {
            strError = "";
            strOutputDbName = "";

            // 因为XMLDOM中无法进行大小写不敏感的搜索，所以把搜索别名的这个任务交给properties
            Debug.Assert(this.CfgDom != null, "");
            BiblioDbProperty prop = this.GetDbProperty(strDbNameOrAlias, true);
            if (prop == null)
            {
                strError = "名字或者别名为 '" + strDbNameOrAlias + "' 的数据库不存在";
                return null;
            }

            strOutputDbName = prop.DbName;


            XmlNode nodeDatabase = this.CfgDom.DocumentElement.SelectSingleNode("//databases/database[@name='" + strOutputDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "名字为 '" + strOutputDbName + "' 的数据库不存在";
            }

            XmlNode nodeUse = nodeDatabase.SelectSingleNode("use[@value='"+lAttributeValue.ToString()+"']");
            if (nodeUse == null)
            {
                strError = "数据库 '" + strDbNameOrAlias + "' 中没有找到关于 '" + lAttributeValue.ToString() + "' 的检索途径定义";
                return null;
            }

            string strFrom =  DomUtil.GetAttr(nodeUse, "from");
            if (String.IsNullOrEmpty(strFrom) == true)
            {
                strError = "数据库 '" + strDbNameOrAlias + "' <database>元素中关于 '" + lAttributeValue.ToString() + "' 的<use>配置缺乏from属性值";
                return null;
            }

            return strFrom;
        }
    }


    // 书目库属性
    public class BiblioDbProperty
    {
        // dp2library定义的特性
        public string DbName = "";  // 书目库名
        public string Syntax = "";  // 格式语法
        public string ItemDbName = "";  // 对应的实体库名

        public bool IsVirtual = false;  // 是否为虚拟库

        // 在dp2zserver.xml中定义的特性
        public int MaxResultCount = -1; // 检索命中的最多条数
        public string DbNameAlias = ""; // 数据库别名

        public bool AddField901 = false;    // 是否在MARC字段中加入表示记录路径和时间戳的的901字段
    }
}
