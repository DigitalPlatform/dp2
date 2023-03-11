using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Web;
using System.Drawing;
using System.Resources;
using System.Globalization;
using System.Drawing.Imaging;
using System.Web.UI;

using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;

namespace DigitalPlatform.OPAC.Server
{
    public partial class OpacApplication
    {
        /// <summary>
        /// 读用途的日志，启用了哪些具体功能
        /// hitcount,log
        /// </summary>
        public string SearchLogEnable
        {
            get; set;
        }

        // 2017/12/12
        /// <summary>
        /// 要隐藏的数据库名列表。为逗号分隔的字符串。如果为 null，表示不隐藏任何数据库名
        /// </summary>
        public string HideDbNames
        {
            get; set;
        }

        /// <summary>
        /// 用于限制 SearchBiblio() API 命中范围的过滤条件。
        /// 例如，"-内部"。注意需要和 dp2library 的 library.xml 中的 globalResults 元素配合使用
        /// </summary>
        public string BiblioFilter
        {
            get; set;
        }

        /// <summary>
        /// 借还历史数据库类型。空表示没有启用。其他表示启用了，例如 enabled
        /// </summary>
        public string ChargingHistoryType
        {
            get;
            set;
        }

        /// <summary>
        /// dp2library 输出的 MSMQ 队列路径
        /// </summary>
        public string OutgoingQueue { get; set; }

        public LibraryChannelPool ChannelPool = new LibraryChannelPool();

        // public double dp2LibraryVersion = 0;
        public string dp2LibraryVersion = "0.0";

        // 2015/6/16
        public string dp2LibraryUID = "";

        // 按照 IP 限定 Session 数目
        public IpTable IpTable = new IpTable();

        public Hashtable ParamTable = new Hashtable();

        public object GetParam(string strName)
        {
            lock (this.ParamTable)
            {
                return this.ParamTable[strName];
            }
        }

        public void SetParam(string strName, object obj)
        {
            lock (this.ParamTable)
            {
                this.ParamTable[strName] = obj;
            }
        }

        // filename:nodepath --> count
        public Hashtable BrowseNodeCountTable = new Hashtable();

        // test leak
        public ChatRoomCollection ChatRooms = new ChatRoomCollection();

        public bool XmlLoaded = false;  // dp2library中的xml定义是否成功装载。报错信息不好在这里记载，因为可能在线程重试的过程中，内容尺寸太大。应该是从当日日志文件读取最好了

        public string GlobalErrorInfo = ""; // 存放全局出错信息。两级报错机制：当这里有值的时候，优先认这里的；否则，再看Application["errorinfo"]字符串
        const string EncryptKey = "dp2circulationpassword";
        // http://localhost/dp2bbs/passwordutil.aspx


        string m_strFileName = "";  // opac.xml配置文件全路径
        string m_strWebuiFileName = ""; // webui.xml配置文件全路径

        public string DataDir = "";
        public string HostDir = "";

        public string BinDir = "";	// bin目录
        public string CfgDir = "";  // cfg目录
        public string CfgMapDir = "";  // cfgmap目录
        public string LogDir = "";	// 事件日志目录
        public string StatisDir = "";   // 统计文件存放目录
        public string SessionDir = "";  // session临时文件
        public string ReportDir = "";   // 报表目录
        public string TempDir = ""; // 临时文件目录 2016/1/24

        public string WsUrl = "";	// dp2Library WebService URL

        public string ManagerUserName = ""; // dp2library超级用户，或者一个权限足够的的用户
        public string ManagerPassword = "";

        public string MongoDbConnStr = "";
        public string MongoDbInstancePrefix = ""; // MongoDB 的实例字符串。用于区分不同的 dp2OPAC 实例在同一 MongoDB 实例中创建的数据库名，这个实例名被用作数据库名的前缀字符串

        public bool DebugMode = false;
        public HangupReason HangupReason = HangupReason.None;

        // Application通用锁。可以用来管理GlobalCfgDom等
        public ReaderWriterLock m_lock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        public XmlDocument OpacCfgDom = null;   // opac.xml配置文件内容

        bool m_bChanged = false;

        FileSystemWatcher watcher = null;

#if NO
        FileSystemWatcher virtual_watcher = null;
#endif

        public string OpacServerUrl = "";

        public bool UseTransfer = false;    // 是否用 Server.Transfer() 代替 Response.Redirect()

        public BoxesInfo BoxesInfo = new BoxesInfo();

        // 为了兼容以前的脚本程序。过一段时间后删除这个
        public string LibraryServerUrl
        {
            get
            {
                return OpacServerUrl;
            }
        }

        public long m_lSeed = 0;

        // 本地结果集锁。避免多线程改写同一结果集
        public RecordLockCollection ResultsetLocks = new RecordLockCollection();

        public Hashtable StopTable = new Hashtable();

        // 等待处理的缓存文件
        public List<String> PendingCacheFiles = new List<string>();
        public CacheBuilder CacheBuilder = null;

        public int SearchMaxResultCount = 5000;

        public XmlDocument WebUiDom = null;   // webui.xml配置文件内容

        public VirtualDatabaseCollection vdbs = null;
        DefaultThread defaultManagerThread = null; // 缺省管理后台任务
        public BatchTaskCollection BatchTasks = new BatchTaskCollection();

        public CfgsMap CfgsMap = null;

        public string ArrivedDbName = "";   // 预约到书队列数据库名
        public string ArrivedReserveTimeSpan = "";  // 通知到书后的保留时间。含时间单位
        public int OutofReservationThreshold = 10;  // 预约到书多少不取次后，被惩罚禁止预约
        public bool CanReserveOnshelf = true;   // 是否可以预约在架图书

#if OPAC_SEARCH_LOG
        public SearchLog SearchLog = null;
#endif

        // 构造函数
        public OpacApplication()
        {
        }

        // Web 界面的限定的图书馆代码。可以是逗号分隔的列表
        public string LimitWebUiLibraryCode
        {
            get
            {
                if (this.WebUiDom == null || this.WebUiDom.DocumentElement == null)
                    return "";
                XmlNode node = this.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl");
                if (node != null)
                {
                    string strValue = DomUtil.GetAttr(node, "limitLibraryCode");
                    if (strValue == null)
                        return "";
                    return strValue;
                }

                return "";
            }
        }

        // parameters:
        //      strDataFileName 纯文件名
        public string GetBrowseNodeCount(string strDataFileName, string strNodePath)
        {
            if (string.IsNullOrEmpty(strDataFileName) == true)
                return null;

            strDataFileName = strDataFileName.ToLower();
            return (string)this.BrowseNodeCountTable[strDataFileName + ":" + strNodePath];
        }

        public void SetBrowseNodeCount(string strDataFileName, string strNodePath, string strCount)
        {
            lock (this.BrowseNodeCountTable)
            {
                string strPath = strDataFileName + ":" + strNodePath;
                this.BrowseNodeCountTable[strPath] = strCount;
            }
        }

        public void ClearBrowseNodeCount(string strDataFileName, string strNodePath)
        {
            lock (this.BrowseNodeCountTable)
            {
                if (string.IsNullOrEmpty(strDataFileName) == true)
                    this.BrowseNodeCountTable.Clear();

                List<string> keys = new List<string>();
                strDataFileName = strDataFileName.ToLower();
                string strPath = strDataFileName + ":" + strNodePath;
                foreach (string key in this.BrowseNodeCountTable.Keys)
                {
                    if (string.IsNullOrEmpty(strNodePath) == true)
                    {
                        if (StringUtil.HasHead(key, strDataFileName + ":") == true)
                            keys.Add(key);
                        continue;
                    }

                    if (key == strPath)
                        keys.Add(key);
                }

                foreach (string key in keys)
                {
                    this.BrowseNodeCountTable.Remove(key);
                }
            }
        }

        // 为了二次开发脚本使用
        public static string MakeObjectUrl(string strRecPath,
            string strUri)
        {
            if (string.IsNullOrEmpty(strUri) == true)
                return strUri;

            if (StringUtil.IsHttpUrl(strUri) == true)
                return strUri;

            if (StringUtil.HasHead(strUri, "uri:") == true)
                strUri = strUri.Substring(4).Trim();

            //string strDbName = ResPath.GetDbName(strRecPath);
            //string strRecID = ResPath.GetRecordId(strRecPath);
            string strDbName = StringUtil.GetDbName(strRecPath);
            string strRecID = StringUtil.GetRecordId(strRecPath);

            string strOutputUri = "";
            ReplaceUri(strUri,
                strDbName,
                strRecID,
                out strOutputUri);

            return strOutputUri;
        }

        // "object/1"
        // "1/object/1"
        // "库名/1/object/1"
        // return:
        //		false	没有发生替换
        //		true	替换了
        static bool ReplaceUri(string strUri,
            string strCurDbName,
            string strCurRecID,
            out string strOutputUri)
        {
            strOutputUri = strUri;
            string strTemp = strUri;
            // 看看第一部分是不是object
            string strPart = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart == "")
                return false;

            if (strTemp == "")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strPart;
                return true;
            }

            if (strPart == "object")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strTemp;
                return true;
            }

            string strPart2 = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart2 == "")
                return false;

            if (strPart2 == "object")
            {
                strOutputUri = strCurDbName + "/" + strPart + "/object/" + strTemp;
                return false;
            }

            string strPart3 = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart3 == "")
                return false;

            if (strPart3 == "object")
            {
                strOutputUri = strPart + "/" + strPart2 + "/object/" + strTemp;
                return true;
            }

            return false;
        }

        public bool IsNewStyle
        {
            get
            {
                string strValue = this.WebUiDom.DocumentElement.GetAttribute("newStyle");
                if (string.IsNullOrEmpty(strValue) == true)
                    return false;

                return DomUtil.IsBooleanTrue(strValue);
            }
        }

        /// <summary>
        /// 前端，也就是 dp2OPAC 的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        public int Load(
    bool bReload,
    string strDataDir,
    string strHostDir,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            ClientVersion = Assembly.GetAssembly(typeof(OpacApplication)).GetName().Version.ToString();

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                OpacApplication app = this;  // new CirculationApplication();

                this.DataDir = strDataDir;
                this.HostDir = strHostDir;

                app.m_strFileName = Path.Combine(strDataDir, "opac.xml");

                // 配置文件目录
                app.CfgDir = Path.Combine(strDataDir, "cfgs");

                // 本地映射配置文件目录
                app.CfgMapDir = Path.Combine(strDataDir, "cfgsmap");
                PathUtil.TryCreateDir(app.CfgMapDir);

                // 日志存储目录
                app.LogDir = Path.Combine(strDataDir, "log");
                PathUtil.TryCreateDir(app.LogDir);

                // session 临时文件目录
                app.SessionDir = Path.Combine(strDataDir, "session");
                PathUtil.TryCreateDir(app.SessionDir);

                if (PathUtil.TryClearDir(app.SessionDir) == false)
                    this.WriteErrorLog("清除 Session 文件目录 " + app.SessionDir + " 时出错");

                // 临时文件目录
                app.TempDir = Path.Combine(strDataDir, "temp");
                PathUtil.TryCreateDir(app.TempDir);

                if (PathUtil.TryClearDir(app.TempDir) == false)
                    this.WriteErrorLog("清除临时文件目录 " + app.TempDir + " 时出错");

                // 2018/10/23
                // 清除一些特殊的临时文件
                this.TryClearSpecialTempFiles();

                // bin dir
                app.BinDir = Path.Combine(strHostDir, "bin");

                nRet = 0;

                if (bReload == false)
                {
                    if (app.HasAppBeenKilled() == true)
                    {
                        app.WriteErrorLog("*** 发现opac service先前曾被意外终止 ***");
                    }
                }

                if (bReload == true)
                    app.WriteErrorLog("opac service 开始重新装载 " + this.m_strFileName);
                else
                    app.WriteErrorLog("opac service 开始启动。");

                //

                if (bReload == false)
                {
                    app.m_strWebuiFileName = PathUtil.MergePath(strDataDir, "webui.xml");
                    // string strWebUiFileName = PathUtil.MergePath(strDataDir, "webui.xml");
                    nRet = LoadWebuiCfgDom(out strError);
                    if (nRet == -1)
                    {
                        // strError = "装载配置文件-- '" + strWebUiFileName + "'时发生错误，原因：" + ex.Message;
                        app.WriteErrorLog(strError);
                        goto ERROR1;
                    }
                }

                //

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(this.m_strFileName);
                }
                catch (FileNotFoundException)
                {
                    strError = "file '" + this.m_strFileName + "' not found ...";
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "装载配置文件-- '" + this.m_strFileName + "' 时发生错误，错误类型：" + ex.GetType().ToString() + "，原因：" + ex.Message;
                    app.WriteErrorLog(strError);
                    // throw ex;
                    goto ERROR1;
                }

                app.OpacCfgDom = dom;

                // *** 进入内存的参数开始
                // 注意修改了这些参数的结构后，必须相应修改Save()函数的相关片断

                // 2011/1/7
                bool bValue = false;
                DomUtil.GetBooleanParam(app.OpacCfgDom.DocumentElement,
                    "debugMode",
                    false,
                    out bValue,
                    out strError);
                this.DebugMode = bValue;

                // 应用服务器参数
                // 元素<libraryServer>
                // 属性url/username/password
                XmlElement node = dom.DocumentElement.SelectSingleNode("libraryServer") as XmlElement;
                if (node != null)
                {
                    app.WsUrl = DomUtil.GetAttr(node, "url");

                    app.ReportDir = DomUtil.GetAttr(node, "reportDir");

                    app.ManagerUserName = DomUtil.GetAttr(node,
                        "username");

                    try
                    {
                        app.ManagerPassword = Cryptography.Decrypt(
                            DomUtil.GetAttr(node, "password"),
                            EncryptKey);
                    }
                    catch
                    {
                        strError = "<libraryServer>元素password属性中的密码设置不正确";
                        // throw new Exception();
                        goto ERROR1;
                    }

                    CfgsMap = new CfgsMap(this.CfgMapDir,
                        this.WsUrl);
                    CfgsMap.Clear();
                }

                if (this.ChannelPool != null)
                {
                    this.ChannelPool.Close();
                    this.ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(ChannelPool_BeforeLogin);
                    this.ChannelPool = null;
                }

                this.ChannelPool = new LibraryChannelPool();
                this.ChannelPool.MaxCount = 50;
                this.ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(ChannelPool_BeforeLogin);
                this.ChannelPool.BeforeLogin += new BeforeLoginEventHandle(ChannelPool_BeforeLogin);

                // OPAC服务器
                // 元素<opacServer>
                // 属性url
                node = dom.DocumentElement.SelectSingleNode("//opacServer") as XmlElement;
                if (node != null)
                {
                    app.OpacServerUrl = DomUtil.GetAttr(node, "url");

                    // 2015/10/14
                    app.UseTransfer = DomUtil.GetBooleanParam(node, "useTransfer", false);
                }

                node = dom.DocumentElement.SelectSingleNode("mongoDB") as XmlElement;
                if (node != null)
                {
                    app.MongoDbConnStr = DomUtil.GetAttr(node, "connectionString");
                    app.MongoDbInstancePrefix = node.GetAttribute("instancePrefix");
                }

                // databaseFilter
                {
                    if (this.OpacCfgDom.DocumentElement.SelectSingleNode("databaseFilter") is XmlElement nodeDatabaseFilter)
                    {
                        this.HideDbNames = nodeDatabaseFilter.GetAttribute("hide");
                        this.BiblioFilter = nodeDatabaseFilter.GetAttribute("biblioFilter");
                    }
                    else
                    {
                        this.HideDbNames = null;
                        this.BiblioFilter = "";
                    }
                }

                // //
                string strDebugInfo = "";
                // return:
                //      -2  dp2Library版本不匹配
                //      -1  出错
                //      0   成功
                nRet = GetXmlDefs(
                    false,
                    out strDebugInfo,
                    out strError);
                if (nRet != 0)
                {
                    app.WriteErrorLog("ERR001 首次初始化XmlDefs失败: " + strError);
                    // goto ERROR1;
                }
                else
                {
                    // 初始化虚拟库集合定义对象
                    nRet = InitialVdbs(
                        out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("ERR002 初始化vdbs失败: " + strError);
                        goto ERROR1;    // 这样的初始化失败不是因为通讯问题，而是数据本身的问题，所以不再继续load()函数后面的部分。应当在解决问题后重新启动opac
                    }

                    // <biblioDbGroup> 
                    nRet = app.LoadBiblioDbGroupParam(
                        out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("ERR005 初始化BiblioDbGroup失败: " + strError);
                        goto ERROR1;
                    }
                }

                // 

                // 初始化扩展SSO接口
                nRet = app.InitialExternalSsoInterfaces(
                out strError);
                if (nRet == -1)
                {
                    strError = "初始化扩展的SSO接口时出错: " + strError;
                    app.WriteErrorLog(strError);
                    // goto ERROR1;
                }

                // *** 进入内存的参数结束


                // 启动批处理任务
                if (bReload == false)
                {
                    // string strBreakPoint = "";

                    // 启动DefaultThread
                    try
                    {
                        DefaultThread defaultThread = new DefaultThread(this, null);
                        this.BatchTasks.Add(defaultThread);

                        defaultThread.StartWorkerThread();

                        this.defaultManagerThread = defaultThread;
                    }
                    catch (Exception ex)
                    {
                        app.WriteErrorLog("启动管理任务DefaultThread时出错：" + ex.Message);
                        goto ERROR1;
                    }

                    // 启动CacheBuilder
                    try
                    {
                        if (this.CacheBuilder == null)
                        {
                            this.CacheBuilder = new CacheBuilder(this, null);
                            this.BatchTasks.Add(this.CacheBuilder);

                            this.CacheBuilder.StartWorkerThread();
                        }
                    }
                    catch (Exception ex)
                    {
                        app.WriteErrorLog("启动批处理任务CacheBuilder时出错：" + ex.Message);
                        goto ERROR1;
                    }

                }

#if NO
                // chargingHistory
                XmlElement nodeCharingHistory = this.OpacCfgDom.DocumentElement.SelectSingleNode("chargingHistory") as XmlElement;
                if (nodeCharingHistory != null)
                {
                    this.ChargingHistoryType = nodeCharingHistory.GetAttribute("type");
                }
#endif

                // searchLog
                XmlElement nodeSearchLog = this.OpacCfgDom.DocumentElement.SelectSingleNode("searchLog") as XmlElement;
                if (nodeSearchLog != null)
                {
#if NO
                    string strEnable = nodeSearchLog.GetAttribute("enable");
                    // TODO: 如果以前已经有这个对象，需要先关闭它
                    // TODO: 如果因为MongoDB启动落后于dp2OPAC怎么办？ 是否需要重试?
                    this.SearchLog = new SearchLog();
                    nRet = this.SearchLog.Open(this, strEnable, out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("启动 SearchLog 时出错：" + strError);
                        this.SearchLog = null;
                    }
#endif
                    this.SearchLogEnable = nodeSearchLog.GetAttribute("enable");
                }

                // chat room
                if (this.ChatRooms != null)
                {
                    XmlNode nodeDef = this.OpacCfgDom.DocumentElement.SelectSingleNode("chatRoomDef");
                    nRet = this.ChatRooms.Initial(nodeDef,
                        PathUtil.MergePath(this.DataDir, "chatrooms"),
                        out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("初始化 ChatRooms 时出错：" + strError);
                        goto ERROR1;
                    }
                }

                // 公共查询最大命中数
                {
                    XmlNode nodeTemp = this.OpacCfgDom.DocumentElement.SelectSingleNode("//virtualDatabases");
                    if (nodeTemp != null)
                    {
                        try
                        {
                            string strMaxCount = DomUtil.GetAttr(nodeTemp, "searchMaxResultCount");
                            if (String.IsNullOrEmpty(strMaxCount) == false)
                                this.SearchMaxResultCount = Convert.ToInt32(strMaxCount);
                        }
                        catch
                        {
                        }
                    }
                }

                if (bReload == false)
                {
                    string strColumnDir = Path.Combine(strDataDir, "column");

                    PathUtil.TryCreateDir(strColumnDir);    // 确保目录创建

                    // return:
                    //      -1  出错
                    //      0   栏目存储文件没有找到
                    //      1   成功(文件已经 Attach)
                    nRet = LoadCommentColumn(
                                Path.Combine(strColumnDir, "comment"),
                                out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("装载栏目存储时出错: " + strError);
                    }
                    else if (nRet == 0)
                        app.WriteErrorLog("栏目存储尚未创建。请及时创建");

                }

                if (bReload == true)
                    app.WriteErrorLog("opac service结束重新装载 " + this.m_strFileName);
                else
                {
                    // var version = System.Reflection.Assembly.GetAssembly(typeof(OpacApplication)).GetName().Version;

                    app.WriteErrorLog("opac service 成功启动。版本: "
                        // + System.Reflection.Assembly.GetAssembly(typeof(OpacApplication)).GetName().ToString()
                        + ClientVersion
                        );

                    // 写入down机检测文件
                    app.WriteAppDownDetectFile("opac service启动。");

                    if (this.watcher == null)
                        BeginWatcher();

#if NO
                    if (this.virtual_watcher == null)
                        BeginVirtualDirWatcher();
#endif
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            return 0;
        ERROR1:
            if (bReload == false)
            {
                if (this.watcher == null)
                    BeginWatcher();

#if NO
                if (this.virtual_watcher == null)
                    BeginVirtualDirWatcher();
#endif
            }
            return -1;
        }

        // TODO: Login 的 parameters 中增加 clientip=
        public void ChannelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            LibraryChannel channel = sender as LibraryChannel;

            // 2016/1/25
            if (string.IsNullOrEmpty(channel.UserName) == true)
                throw new Exception("ChannelPool_BeforeLogin() channel.UserName is null");

            if (channel.UserName == this.ManagerUserName)
            {
                e.UserName = channel.UserName;
                e.Password = channel.Password;
                e.LibraryServerUrl = channel.Url;
                e.Parameters = "client=dp2OPAC|" + OpacApplication.ClientVersion;

#if NO
                if (channel.Param is string)
                    e.Parameters = (string)channel.Param;
#endif
                return;
            }

            if (StringUtil.HasHead(channel.Password, "token:") == true)
            {
                Hashtable parameters = new Hashtable();
                if (channel.Param is string)
                {
                    parameters = StringUtil.ParseParameters((string)channel.Param, ',', '=');
                }
                parameters["client"] = "dp2OPAC|" + OpacApplication.ClientVersion;
                parameters["index"] = "-1";
#if NO
                if (parameters.ContainsKey("type") == false)
                    parameters["type"] = "reader";
#endif
                parameters["simulate"] = "yes";

                e.UserName = channel.UserName;
                e.Parameters = StringUtil.BuildParameterString(parameters, ',', '=');
                e.Password = this.ManagerUserName + "," + this.ManagerPassword + "|||" + channel.Password;   // simulate登录的需要
            }
            else
            {
                e.UserName = channel.UserName;
                e.Password = channel.Password;
                e.LibraryServerUrl = channel.Url;
#if NO
            if (channel.Tag is string)
                e.Parameters = (string)channel.Tag;
#endif
                if (channel.Param is string)
                    e.Parameters = (string)channel.Param;
            }
        }

        public void TryClearSpecialTempFiles()
        {
            string strFileName = Path.Combine(this.DataDir, "cfgs\\statis_timerange.xml.1");
            try
            {
                File.Delete(strFileName);
            }
            catch
            {

            }
        }

        // 刷新来自外部的配置信息
        public int RefreshCfgs(
            out string strDebugInfo,
            out string strError)
        {
            strError = "";

            if (this.Filters != null)
                this.Filters.Clear();

            // ClearXml2HtmlAssembly();
            this.AssemblyCache.Clear();

            // 2012/10/26
            this.CfgsMap.Clear();

            // //
            // return:
            //      -2  dp2Library版本不匹配
            //      -1  出错
            //      0   成功
            int nRet = GetXmlDefs(
                true,
                out strDebugInfo,
                out strError);
            if (nRet != 0)
            {
                this.XmlLoaded = false; // 促使后面可以自动重试
                this.WriteErrorLog("ERR00? 刷新配置信息时，初始化XmlDefs失败: " + strError);
                return -1;
            }
            else
            {
                // 初始化虚拟库集合定义对象
                this.vdbs = null;
                nRet = InitialVdbs(
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("ERR00? 刷新配置信息时，初始化vdbs失败: " + strError);
                    return -1;
                }

                // <biblioDbGroup> 
                nRet = this.LoadBiblioDbGroupParam(
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("ERR00? 刷新配置信息时，初始化BiblioDbGroup失败: " + strError);
                    return -1;
                }
            }

#if NO
            // 复制css文件
            nRet = CopyCssFiles(out strError);
            if (nRet == -1)
                return -1;
#endif
            // 2016/1/27
            this.ChannelPool.Close();
            return 0;
        }

        // 获得图书馆信息
        // parameters:
        //      strLibraryCode  分馆代码或者 stylename
        public LibraryInfo GetLibraryInfo(string strLibraryCode)
        {
            LibraryInfo info = new LibraryInfo();
            XmlElement root = this.WebUiDom.DocumentElement.SelectSingleNode("libraries") as XmlElement;
            if (root == null)
                return null;

            XmlElement library = root.SelectSingleNode("library[@code='" + strLibraryCode + "']") as XmlElement;
            if (library == null)
                library = root.SelectSingleNode("library[@style='" + strLibraryCode + "']") as XmlElement;
            if (library == null)
                return null;

            info.StyleName = library.GetAttribute("style");
            info.LibraryCode = library.GetAttribute("code");
            info.LogoText = library.GetAttribute("logoText");

            return info;
        }

        // 构造 CSS 文件内容
        // parameters:
        //      strFilePath 返回文件的物理路径
        // return:
        //      -1  出错
        //      0   在 webui.xml 中没有找到映射关系
        //      1   找到了映射关系，并获得了温家内容在 strContent 中
        //      2   .css 文件已经存在。 strFilePath 中返回这个 .css 文件的物理路径
        public int BuildCssContent(string strLibraryCode,
            string strStyle,
            string strFileName,
            out string strContent,
            out string strFilePath,
            out string strError)
        {
            strContent = "";
            strError = "";
            strFilePath = "";

            XmlElement root = this.WebUiDom.DocumentElement.SelectSingleNode("libraries") as XmlElement;
            if (root == null)
            {
                strFilePath = Path.Combine(this.DataDir, "style/" + strStyle + "/" + strFileName);  // 2017/11/13
                return 0;
            }

            XmlElement library = root.SelectSingleNode("library[@code='" + strLibraryCode + "']") as XmlElement;
            if (library == null)
                library = root.SelectSingleNode("library[@style='" + strLibraryCode + "']") as XmlElement;
            if (library == null)
                library = root;

            // 构造 macro_table
            Hashtable macro_table = new Hashtable();
            XmlNodeList items = library.SelectNodes("cssMacroTable/item");
            foreach (XmlElement item in items)
            {
                string strName = item.GetAttribute("name");
                macro_table[strName] = item.InnerText.Trim();
            }

            string strCssFileName = Path.Combine(this.DataDir, "style/" + strStyle + "/" + strFileName);

            string strMacroFileName = strCssFileName + ".macro";

            // 如果  .macro 文件存在，则优先用 .macro 文件内容
            if (File.Exists(strMacroFileName) == true)
            {
                // 文件内容需要转换
                strFilePath = strMacroFileName;
            }
            else
            {
                // 否则使用 .css 文件。 .css 文件内容不需要转换
                strFilePath = strCssFileName;
                return 2;
            }

            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strMacroFileName,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;
            if (nRet == 2)
                return -1;

            strContent = StringUtil.MacroString(macro_table, strContent);
            return 1;
        }

#region 复制 CSS 文件

        int CopyCssFiles(out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlNode root = this.WebUiDom.DocumentElement.SelectSingleNode("libraries");
            if (root == null)
                return 0;

            string strSourceBaseDir = DomUtil.GetAttr(root, "cssSourceDirectory");
            if (string.IsNullOrEmpty(strSourceBaseDir) == true)
                return 0;

            Hashtable dir_macro_table = new Hashtable();
            dir_macro_table["%datadir%"] = this.DataDir;
            dir_macro_table["%hostdir%"] = this.HostDir;

            string strTargetBaseDir = DomUtil.GetAttr(root, "cssTargetDirectory");
            if (string.IsNullOrEmpty(strTargetBaseDir) == true)
                strTargetBaseDir = PathUtil.MergePath(this.HostDir, "style");
            else
            {
                strTargetBaseDir = StringUtil.MacroString(dir_macro_table, strTargetBaseDir);
            }

            strSourceBaseDir = StringUtil.MacroString(dir_macro_table, strSourceBaseDir);

            XmlNodeList library_nodes = root.SelectNodes("library");
            foreach (XmlNode library in library_nodes)
            {
                string strStyle = DomUtil.GetAttr(library, "style");
                if (string.IsNullOrEmpty(strStyle) == true)
                    continue;

                // 构造macro_table
                Hashtable macro_table = new Hashtable();
                XmlNodeList items = library.SelectNodes("cssMacroTable/item");
                foreach (XmlNode item in items)
                {
                    string strName = DomUtil.GetAttr(item, "name");
                    macro_table[strName] = item.InnerText.Trim();
                }

                string strTargetDir = PathUtil.MergePath(strTargetBaseDir, strStyle);
                nRet = CopyCssDirectory(strSourceBaseDir,
                    strTargetDir,
                    macro_table,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // 全局(不属于某个分馆)目录
            {

                // 构造macro_table
                Hashtable macro_table = new Hashtable();
                XmlNodeList items = root.SelectNodes("cssMacroTable/item");
                foreach (XmlNode item in items)
                {
                    string strName = DomUtil.GetAttr(item, "name");
                    macro_table[strName] = item.InnerText.Trim();
                }

                nRet = CopyCssDirectory(strSourceBaseDir,
                    strTargetBaseDir,
                    macro_table,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 将CSS文件从源目录复制到目标目录，复制前要进行宏替换
        // parameters:
        //      macro_table 宏列表。如果为null，表示不进行宏变换
        public int CopyCssDirectory(string strSourceDir,
            string strTargetDir,
            Hashtable macro_table,
            out string strError)
        {
            strError = "";

            int nRet = CopyDirectory(strSourceDir,
                strTargetDir,
                false,
                CopyOneFile,
                macro_table,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        static void CopyOneFile(string strSourceFile, string strTargetFile, object param)
        {
            if (param == null || String.Compare(Path.GetExtension(strSourceFile), ".css", true) != 0)
            {
                File.Copy(strSourceFile, strTargetFile, true);
                return;
            }


            Hashtable macro_table = (Hashtable)param;
            // 将源文件读入内存，进行宏变换，然后写入目标位置

            string strError = "";
            string strContent = "";
            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strSourceFile,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1 || nRet == 0)
                throw new Exception(strError);
            if (nRet == 2)
                throw new Exception("FileUtil.ReadTextFileContent() error");

            strContent = StringUtil.MacroString(macro_table, strContent);

            using (StreamWriter sw = new StreamWriter(strTargetFile, false, encoding))
            {
                sw.Write(strContent);
            }
        }

#if NO
        // 已经移动到 StringUtil 中
        // 兑现字符串中的宏值
        public static string MacroString(Hashtable macro_table,
            string strInputString)
        {
            foreach (string strMacroName in macro_table.Keys)
            {
                strInputString = strInputString.Replace(strMacroName, (string)macro_table[strMacroName]);
            }

            return strInputString;
        }
#endif

        public delegate void CopyOneFileCallBack(string strSourceFile, string strTargetFile, object param);

        // 拷贝目录
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            bool bDeleteTargetBeforeCopy,
            CopyOneFileCallBack func,
            object param,
            out string strError)
        {
            strError = "";

            try
            {

                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }

                PathUtil.TryCreateDir(strTargetDir);

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                for (int i = 0; i < subs.Length; i++)
                {
                    // 复制目录
                    if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(subs[i].FullName,
                            strTargetDir + "\\" + subs[i].Name,
                            bDeleteTargetBeforeCopy,
                            func,
                            param,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }
                    // 复制文件
                    if (func == null)
                        File.Copy(subs[i].FullName, strTargetDir + "\\" + subs[i].Name, true);
                    else
                        func(subs[i].FullName, strTargetDir + "\\" + subs[i].Name, param);
                }

            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

#endregion

        // 缓存的from信息
        Hashtable m_fromTable = new Hashtable();

        // (注：采用了代理账户)
        public int GetDbFroms(
            string strDbType,
            string strLang,
            out BiblioDbFromInfo[] infos,
            out string strError)
        {
            strError = "";
            infos = null;

            string strKey = strDbType + "|" + strLang;
            infos = (BiblioDbFromInfo[])this.m_fromTable[strKey];
            if (infos != null)
                return 1;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
#if NO
                // 临时的SessionInfo对象
                SessionInfo session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
#endif
                LibraryChannel channel = this.GetChannel();

                try
                {

                    // 获取虚拟库定义
                    long lRet = // session.Channel.
                        channel.ListDbFroms(null,
        strDbType,
        strLang,
        out infos,
        out strError);
                    if (lRet == -1)
                    {
                        // strError = "(" + session.UserID + ")获得from定义时发生错误: " + strError;
                        strError = "(" + channel.UserName + ")获得from定义时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
#if NO
                    session.CloseSession();
#endif
                    this.ReturnChannel(channel);
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            this.m_fromTable[strKey] = infos;
            return 0;
        ERROR1:
            return -1;
        }

        // 从dp2Library获得XML定义
        // (注：采用了代理账户)
        // return:
        //      -2  dp2Library版本不匹配
        //      -1  出错
        //      0   成功
        public int GetXmlDefs(
            bool bOuputDebugInfo,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            long lRet = 0;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

#if NO
                // 临时的SessionInfo对象
                SessionInfo session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
#endif
                LibraryChannel channel = this.GetChannel();

                try
                {
                    // 检查版本号
                    {
                        string strVersion = "";
                        string strUID = "";
                        lRet = // session.Channel.
                            channel.GetVersion(
                            null,
                            out strVersion,
                            out strUID,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "针对 dp2Library 服务器 "
                                // + session.Channel.Url 
                                + channel.Url
                                + " 获得版本号的过程发生错误：" + strError;
                            goto ERROR1;
                        }

                        // double value = 0;

                        if (string.IsNullOrEmpty(strVersion) == true)
                        {
                            strVersion = "0.0";
                            // strVersion = "2.0以下";
                            // value = 2.0;
                        }
                        else
                        {
#if NO
                            // 检查最低版本号
                            if (double.TryParse(strVersion, out value) == false)
                            {
                                strError = "dp2Library 版本号 '" + strVersion + "' 格式不正确";
                                goto ERROR1;
                            }
#endif
                        }

                        this.dp2LibraryVersion = strVersion;    //  value;
                        this.dp2LibraryUID = strUID;

#if NO
                        double base_version = 2.86; // 2.18
                        if (value < base_version)
                        {
                            strError = "当前 dp2OPAC 版本需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 '" + strVersion + "' )。请立即升级 dp2Library 到最新版本。";
                            return -2;
                        }
#endif

                        string base_version = "2.86";
                        if (StringUtil.CompareVersion(strVersion, base_version) < 0)
                        {
                            strError = "当前 dp2OPAC 版本需要和 dp2Library " + base_version + " 或以上版本配套使用 (而当前 dp2Library 版本号为 '" + strVersion + "' )。请立即升级 dp2Library 到最新版本。";
                            return -2;
                        }
                    }

                    lRet = // session.Channel.
                        channel.GetSystemParameter(
                        null,
                        "circulation",
                        "chargingOperDatabase",
                        out string strValue,
                        out strError);
                    this.WriteErrorLog("GetSystemParameter() circulation chargingOperDatabase return " + lRet + " , strValue '" + strValue + "', strError '" + strError + "'。(这是一条提示信息，不一定等于出错)");
                    if (strValue == "enabled")
                        this.ChargingHistoryType = strValue;
                    else
                        this.ChargingHistoryType = "";

                    // 2016/9/28
                    strValue = "";
                    lRet = channel.GetSystemParameter(
                        null,
            "system",
            "outgoingQueue",
            out strValue,
            out strError);
                    if (lRet == 1)
                        this.OutgoingQueue = strValue;
                    else
                        this.OutgoingQueue = "";

                    // 获取虚拟库定义
                    string strXml = "";
                    lRet = // session.Channel.
                        channel.GetSystemParameter(
                        null,
                        "virtual",
                        "def",
                        out strXml,
                        out strError);
                    if (lRet == -1)
                    {
                        // strError = "(" + session.UserID + ")获取虚拟库定义时发生错误: " + strError;
                        strError = "(" + channel.UserName + ")获取虚拟库定义时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (bOuputDebugInfo == true)
                        strDebugInfo += "*** virtual/def:\r\n" + strXml + "\r\n";

                    XmlElement node_virtual = this.OpacCfgDom.DocumentElement.SelectSingleNode("virtualDatabases") as XmlElement;
                    if (node_virtual == null)
                    {
                        node_virtual = this.OpacCfgDom.CreateElement("virtualDatabases");
                        this.OpacCfgDom.DocumentElement.AppendChild(node_virtual);
                    }

                    node_virtual = DomUtil.SetElementOuterXml(node_virtual, strXml);
                    Debug.Assert(node_virtual != null, "");

                    // 根据“禁用数据库名”列表，修改 virtualDatabases 元素内的细部
                    ModifyVirtualDatabases(node_virtual, this.HideDbNames);

                    // 获取<arrived>定义
                    lRet = // session.Channel.
                        channel.GetSystemParameter(
                        null,
                        "system",
                        "arrived",
                        out strXml,
                        out strError);
                    if (lRet == -1)
                    {
                        // strError = "(" + session.UserID + ")获取<arrived>定义时发生错误: " + strError;
                        strError = "(" + channel.UserName + ")获取<arrived>定义时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (bOuputDebugInfo == true)
                        strDebugInfo += "*** system/arrived:\r\n" + strXml + "\r\n";

                    XmlElement node_arrived = this.OpacCfgDom.DocumentElement.SelectSingleNode("arrived") as XmlElement;
                    if (node_arrived == null)
                    {
                        node_arrived = this.OpacCfgDom.CreateElement("arrived");
                        this.OpacCfgDom.DocumentElement.AppendChild(node_arrived);
                    }

                    node_arrived = DomUtil.SetElementOuterXml(node_arrived, strXml);
                    Debug.Assert(node_arrived != null, "");

                    // 获取<browseformats>定义
                    lRet = // session.Channel.
                        channel.GetSystemParameter(
                        null,
                        "opac",
                        "browseformats",
                        out strXml,
                        out strError);
                    if (lRet == -1)
                    {
                        // strError = "(" + session.UserID + ")获取<browseformats>定义时发生错误: " + strError;
                        strError = "(" + channel.UserName + ")获取<browseformats>定义时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (bOuputDebugInfo == true)
                        strDebugInfo += "*** opac/browseformats:\r\n" + strXml + "\r\n";

                    XmlNode node_browseformats = this.OpacCfgDom.DocumentElement.SelectSingleNode("browseformats");
                    if (node_browseformats == null)
                    {
                        node_browseformats = this.OpacCfgDom.CreateElement("browseformats");
                        this.OpacCfgDom.DocumentElement.AppendChild(node_browseformats);
                    }

                    node_browseformats.InnerXml = strXml;

                    // 获取<biblioDbGroup>定义
                    lRet = // session.Channel.
                        channel.GetSystemParameter(
                        null,
                        "system",
                        "biblioDbGroup",
                        out strXml,
                        out strError);
                    if (lRet == -1)
                    {
                        // strError = "(" + session.UserID + ")获取<biblioDbGroup>定义时发生错误: " + strError;
                        strError = "(" + channel.UserName + ")获取<biblioDbGroup>定义时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (bOuputDebugInfo == true)
                        strDebugInfo += "*** system/biblioDbGroup:\r\n" + strXml + "\r\n";

                    XmlNode node_biblioDbGroup = this.OpacCfgDom.DocumentElement.SelectSingleNode("biblioDbGroup");
                    if (node_biblioDbGroup == null)
                    {
                        node_biblioDbGroup = this.OpacCfgDom.CreateElement("biblioDbGroup");
                        this.OpacCfgDom.DocumentElement.AppendChild(node_biblioDbGroup);
                    }

                    node_biblioDbGroup.InnerXml = strXml;

                    ClearInvisibleDatabaseDef(ref node_biblioDbGroup, node_virtual);

                    // 2012/10/23
                    // 获取<readerDbGroup>定义
                    {
                        lRet = // session.Channel.
                            channel.GetSystemParameter(
                            null,
                            "system",
                            "readerDbGroup",
                            out strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            // strError = "(" + session.UserID + ")获取<readerDbGroup>定义时发生错误: " + strError;
                            strError = "(" + channel.UserName + ")获取<readerDbGroup>定义时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (bOuputDebugInfo == true)
                            strDebugInfo += "*** system/readerDbGroup:\r\n" + strXml + "\r\n";

                        XmlNode node_readerDbGroup = this.OpacCfgDom.DocumentElement.SelectSingleNode("readerDbGroup");
                        if (node_readerDbGroup == null)
                        {
                            node_readerDbGroup = this.OpacCfgDom.CreateElement("readerDbGroup");
                            this.OpacCfgDom.DocumentElement.AppendChild(node_readerDbGroup);
                        }

                        node_readerDbGroup.InnerXml = strXml;
                    }

                    this.Changed = true;
                    this.ActivateManagerThread();

                    // 检查 simulatereader 和 simulateworder 权限
                    if (StringUtil.IsInList("simulatereader", channel.Rights) == false
                        || StringUtil.IsInList("simulateworker", channel.Rights) == false)
                    {
                        strError = "OPAC 代理账户 '" + this.ManagerUserName + "' 缺乏 simulatereader 和 simulateworker 权限。请在 dp2library 中为它增配这两个权限";
                        return -1;
                    }

                    this.XmlLoaded = true;

                    // 预约到书
                    // 元素<arrived>
                    // 属性dbname/reserveTimeSpan/outofReservationThreshold/canReserveOnshelf
                    XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("//arrived");
                    if (node != null)
                    {
                        this.ArrivedDbName = DomUtil.GetAttr(node, "dbname");
                        this.ArrivedReserveTimeSpan = DomUtil.GetAttr(node, "reserveTimeSpan");

                        int nValue = 0;
                        int nRet = DomUtil.GetIntegerParam(node,
                            "outofReservationThreshold",
                            10,
                            out nValue,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "元素<arrived>属性outofReservationThreshold读入时发生错误: " + strError;
                            goto ERROR1;
                        }

                        this.OutofReservationThreshold = nValue;

                        bool bValue = false;
                        nRet = DomUtil.GetBooleanParam(node,
                            "canReserveOnshelf",
                            true,
                            out bValue,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "元素<arrived>属性canReserveOnshelf读入时发生错误: " + strError;
                            goto ERROR1;
                        }

                        this.CanReserveOnshelf = bValue;
                    }

                    return 0;
                }
                finally
                {
#if NO
                    session.CloseSession();
#endif
                    this.ReturnChannel(channel);
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

        ERROR1:
            return -1;
        }

        // 2012/11/29
        // 根据<virtualDatabases>定义，将<biblioDbGroup>中应不可见的数据库节点删除
        static void ClearInvisibleDatabaseDef(ref XmlNode node_biblioDbGroup,
            XmlNode node_virtual)
        {
            XmlNodeList databases = node_biblioDbGroup.SelectNodes("database");
            foreach (XmlNode database in databases)
            {
                string strName = DomUtil.GetAttr(database, "biblioDbName");
                if (string.IsNullOrEmpty(strName) == true)
                {
                    database.ParentNode.RemoveChild(database);
                    continue;
                }

                // 在<virtualDatabases>下找
                XmlNode node = node_virtual.SelectSingleNode("database[@name='" + strName + "']");
                if (node == null)
                {
                    database.ParentNode.RemoveChild(database);
                    continue;
                }
            }
        }


        // 检查全局配置参数是否基本正常
        public int Verify(out string strError)
        {
            strError = "";
            bool bError = false;
            if (this.WsUrl == "")
            {
                if (strError != "")
                    strError += ", ";

                strError += "<libraryServer>元素中url属性未定义";
                bError = true;
            }

            if (this.ManagerUserName == "")
            {
                if (strError != "")
                    strError += ", ";
                strError += "<libraryServer>元素中username属性未定义";
                bError = true;
            }

            if (bError == true)
                return -1;

            return 0;
        }


        public void RestartApplication()
        {
            try
            {
                // 往bin目录中写一个临时文件
                using (Stream stream = File.Open(this.BinDir + "\\temp.temp",
                    FileMode.Create))
                {
                }
                this.WriteErrorLog("opac service 被重新启动。");
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("opac service 重新启动时发生错误：" + ExceptionUtil.GetDebugText(ex));
            }
        }

#if NO
        public void WriteErrorLog(string strText)
        {
            try
            {
                string strTime = DateTime.Now.ToString();
                StreamUtil.WriteText(this.LogDir + "\\error.txt",
                    strTime + " " + strText + "\r\n");
            }
            catch
            {
                // TODO: 要在安装程序中预先创建事件源
                // 代码可以参考 unhandle.txt (在本project中)

                /*
                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists("dp2opac"))
                {
                    EventLog.CreateEventSource("dp2opac", "DigitalPlatform");
                }*/

                EventLog Log = new EventLog();
                Log.Source = "dp2opac";
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }
#endif
        static object logSyncRoot = new object();

        public void WriteErrorLog(string strText)
        {
            try
            {
                lock (logSyncRoot)
                {
                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = PathUtil.MergePath(this.LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                    string strTime = now.ToString();
                    StreamUtil.WriteText(strFilename,
                        strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = "dp2opac";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        public void DumpErrorLog(Page page)
        {
            string strFileName = PathUtil.MergePath(this.LogDir, "log_" + DateTimeUtil.DateTimeToString8(DateTime.Now) + ".txt");

            // TODO: 如何面对超大文件
            string strText = "";
            lock (this.LogDir)
            {
                using (StreamReader sr = new StreamReader(strFileName,
System.Text.Encoding.UTF8))
                {
                    if (sr.BaseStream.Length > 1000 * 1024)
                        strText = strFileName + " 文件太大，无法输出";
                    else
                        strText = sr.ReadToEnd();
                }
            }

            page.Response.Write(HttpUtility.HtmlEncode("--- 文件 " + strFileName + " 内容如下 ---\r\n"));
            page.Response.Write(HttpUtility.HtmlEncode(strText));
        }

        // 写入Windows系统日志
        public static void WriteWindowsLog(string strText)
        {
            WriteWindowsLog(strText, EventLogEntryType.Error);
        }

        // 写入Windows系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2opac";
            Log.WriteEntry(strText, type);
        }

        public static void WriteErrorLog(string strFileName,
            string strText)
        {
            try
            {
                string strTime = DateTime.Now.ToString();
                StreamUtil.WriteText(strFileName,
                    strTime + " " + strText + "\r\n");
            }
            catch
            {
                WriteWindowsLog(strText, EventLogEntryType.Error);
            }
        }

        public void WriteDebugInfo(string strTitle)
        {
            if (this.DebugMode == false)
                return;
            StreamUtil.WriteText(this.LogDir + "\\debug.txt", "-- " + DateTime.Now.ToString("u") + " " + strTitle + "\r\n");
        }

        public void WriteAppDownDetectFile(string strText)
        {
            string strTime = DateTime.Now.ToString();
            StreamUtil.WriteText(this.LogDir + "\\app_down_detect.txt",
                strTime + " " + strText + "\r\n");
        }

        public void RemoveAppDownDetectFile()
        {
            try
            {
                File.Delete(this.LogDir + "\\app_down_detect.txt");
            }
            catch
            {
            }
        }

        public bool HasAppBeenKilled()
        {
            try
            {
                FileInfo fi = new FileInfo(this.LogDir + "\\app_down_detect.txt");

                if (fi.Exists == true)
                    return true;

                return false;
            }
            catch
            {
                return true;    // 抛出异常时，宁可信其有
            }
        }

        public void Flush()
        {
            try
            {
                this.Save(null, true);
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("Flush()中俘获异常 " + ex.Message);
            }
        }


        // 保存
        // 其实,进入内存属性的XML片断,可以在this.OpacCfgDom中删除.最后直接合并保存这个dom即可.
        // parameters:
        //      bFlush  是否为刷新情形？如果是，则不写入错误日志
        public void Save(string strFileName,
            bool bFlush)
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                if (this.m_bChanged == false)
                {
                    /*
                    // 调试用
                    OpacApplication.WriteWindowsLog("没有进行Save()，因为m_bChanged==false", EventLogEntryType.Information);
                     * */

                    return;
                }


                // 关闭文件跟踪
                bool bOldState = false;
                if (this.watcher != null)
                {
                    bOldState = watcher.EnableRaisingEvents;
                    watcher.EnableRaisingEvents = false;
                }


                if (strFileName == null)
                    strFileName = m_strFileName;

                if (strFileName == null)
                {
                    throw (new Exception("m_strFileName为空"));
                }

                string strBackupFilename = strFileName + ".bak";

                if (FileUtil.IsFileExsitAndNotNull(strFileName) == true)
                {
                    this.WriteErrorLog("备份 " + strFileName + " 到 " + strBackupFilename);
                    File.Copy(strFileName, strBackupFilename, true);
                }

                if (bFlush == false)
                {
                    this.WriteErrorLog("开始 从内存写入 " + strFileName);
                }

                using (XmlTextWriter writer = new XmlTextWriter(strFileName,
                    Encoding.UTF8))
                {
                    // 缩进
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    writer.WriteStartDocument();

                    writer.WriteStartElement("root");
                    if (this.DebugMode == true)
                        writer.WriteAttributeString("debugMode", "true");

                    // <version>
                    {
                        XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode("version");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }
                    }

                    // 内核参数
                    // 元素<libraryServer>
                    // 属性url/username/password
                    writer.WriteStartElement("libraryServer");
                    writer.WriteAttributeString("url", this.WsUrl);
                    writer.WriteAttributeString("username", this.ManagerUserName);
                    writer.WriteAttributeString("password",
                        Cryptography.Encrypt(this.ManagerPassword, EncryptKey)
                        );
                    if (string.IsNullOrEmpty(this.ReportDir) == false)
                        writer.WriteAttributeString("reportDir", this.ReportDir);
                    writer.WriteEndElement();

                    // 图书馆业务服务器
                    // 元素<opacServer>
                    // 属性url
                    writer.WriteStartElement("opacServer");
                    writer.WriteAttributeString("url", this.OpacServerUrl);
                    writer.WriteAttributeString("useTransfer", this.UseTransfer ? "true" : "false");
                    writer.WriteEndElement();

                    // mongoDB 服务器
                    // 元素<mongoDB>
                    // 属性connectionString
                    writer.WriteStartElement("mongoDB");
                    writer.WriteAttributeString("connectionString", this.MongoDbConnStr);
                    writer.WriteAttributeString("instancePrefix", this.MongoDbInstancePrefix);
                    writer.WriteEndElement();

                    // 没有进入内存属性的其他XML片断
                    if (this.OpacCfgDom != null)
                    {
                        string[] elements = new string[]{
                            "yczb",
                            "monitors",
                            "libraryInfo",
                            "biblioDbGroup",
                            "arrived",
                            "browseformats",
                            "virtualDatabases",
                            "externalSsoInterface",
                            "dp2sso",
                            "searchLog",
                            "databaseFilter",
                        };

                        RestoreElements(writer, elements);

#if NO
                        // 2009/9/23
                        XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "//yczb");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <monitors>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "monitors");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // TODO: 暂时没有任何地方用到这个信息
                        // <libraryInfo>
                        // 注: <libraryName>元素在此里面
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "libraryInfo");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <biblioDbGroup>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "biblioDbGroup");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <arrived>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "arrived");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <browseformats>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "browseformats");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <virtualDatabases>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "//virtualDatabases");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // <externalSsoInterface>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "externalSsoInterface");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }



                        // 2012/5/23
                        // <dp2sso>
                        /*
        <dp2sso>
            <domain name='dp2bbs' loginUrl='http://dp2003.com/dp2bbs/login.aspx?redirect=%redirect%'  logoutUrl='' />
        </dp2sso>
                         * */
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "//dp2sso");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

#if NO
                        // <chargingHistory>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode("chargingHistory");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }
#endif

                        // <searchLog>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "searchLog");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

                        // 2017/12/12
                        // <databaseFilter>
                        node = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                            "databaseFilter");
                        if (node != null)
                        {
                            node.WriteTo(writer);
                        }

#endif

                        // chat room
                        if (this.ChatRooms != null)
                        {
                            string strError = "";
                            string strXml = "";
                            this.ChatRooms.GetDef(out strXml, out strError);
                            if (string.IsNullOrEmpty(strXml) == false)
                                writer.WriteRaw("\r\n" + strXml + "\r\n");
                        }

                    }

                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }

                if (bFlush == false)
                    this.WriteErrorLog("完成 从内存写入 " + strFileName);

                this.m_bChanged = false;

                if (this.watcher != null)
                {
                    watcher.EnableRaisingEvents = bOldState;
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

        }

        void RestoreElements(XmlTextWriter writer, string[] elements)
        {
            foreach (string element in elements)
            {
                XmlNode node = this.OpacCfgDom.DocumentElement.SelectSingleNode(element);
                if (node != null)
                    node.WriteTo(writer);
            }
        }


        public static bool ToBoolean(string strText,
    bool bDefaultValue)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return bDefaultValue;

            strText = strText.ToLower();

            if (strText == "true" || strText == "on" || strText == "yes")
                return true;

            return false;
        }

        // 激活管理后台任务。一般用于迫使写入cfgdom到xml文件
        public void ActivateManagerThread()
        {
            if (this.defaultManagerThread != null)
                this.defaultManagerThread.Activate();
        }

        // 当遇到 System.IO.IOException 的时候会自动重试几次
        int LoadWebuiCfgDom(out string strError)
        {
            strError = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (String.IsNullOrEmpty(this.m_strWebuiFileName) == true)
                {
                    strError = "m_strWebuiFileName尚未初始化，因此无法装载webui.xml配置文件到DOM";
                    return -1;
                }

                int nRedoCount = 0;
            REDO:
                XmlDocument webuidom = new XmlDocument();
                try
                {
                    webuidom.Load(this.m_strWebuiFileName);
                }
                catch (FileNotFoundException)
                {
                    webuidom.LoadXml("<root/>");
                }
                catch (System.IO.IOException ex)
                {
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(500);
                        nRedoCount++;
                        goto REDO;
                    }
                    else
                    {
                        strError = "装载配置文件-- '" + this.m_strWebuiFileName + "' 时出现异常(重试 5 次以后依然遇到异常)：" + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "装载配置文件-- '" + this.m_strWebuiFileName + "' 时出现异常：" + ExceptionUtil.GetDebugText(ex);
                    // this.WriteErrorLog(strError);
                    return -1;
                }

                this.WebUiDom = webuidom;
                return 0;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

#if NO
        void BeginVirtualDirWatcher()
        {
            virtual_watcher = new FileSystemWatcher();
            virtual_watcher.Path = this.HostDir;

            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            virtual_watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;

            virtual_watcher.Filter = "*.*"; // Path.GetFileName(this.m_strFileName);  //"*.*";
            virtual_watcher.IncludeSubdirectories = true;

            // Add event handlers.
            virtual_watcher.Changed -= new FileSystemEventHandler(watcher_appdir_Changed);
            virtual_watcher.Changed += new FileSystemEventHandler(watcher_appdir_Changed);

            // Begin watching.
            virtual_watcher.EnableRaisingEvents = true;

            /*
            // 停止 ASP.NET 监视 style 子目录
            {
                PropertyInfo p = typeof(System.Web.HttpRuntime).GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                object o = p.GetValue(null, null);
                FieldInfo f = o.GetType().GetField("_dirMonSubdirs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                object monitor = f.GetValue(o);
                MethodInfo m = monitor.GetType().GetMethod("StopMonitoring", BindingFlags.Instance | BindingFlags.NonPublic);
                m.Invoke(monitor, new object[] { });
            }
             * */

        }

        void EndVirtualWather()
        {
            if (this.virtual_watcher != null)
            {
                virtual_watcher.EnableRaisingEvents = false;
                virtual_watcher.Changed -= new FileSystemEventHandler(watcher_appdir_Changed);
                this.virtual_watcher.Dispose();
                this.virtual_watcher = null;
            }
        }

        // 虚拟目录内发生改变
        void watcher_appdir_Changed(object sender, FileSystemEventArgs e)
        {
#if NO
            string strError = "*** 虚拟目录内发生改变: name: " + e.Name.ToString()
                + "; changetype: " + e.ChangeType.ToString()
                + "; fullpath: " + e.FullPath.ToString();
            this.WriteErrorLog(strError);
#endif
        }
#endif

        void BeginWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(this.m_strFileName);

            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes;

            watcher.Filter = "*.*"; // Path.GetFileName(this.m_strFileName);  //"*.*";
            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed -= new FileSystemEventHandler(watcher_datadir_Changed);
            watcher.Changed += new FileSystemEventHandler(watcher_datadir_Changed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        void EndWather()
        {
            if (this.watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= new FileSystemEventHandler(watcher_datadir_Changed);
                this.watcher.Dispose();
                this.watcher = null;
            }
        }

        void watcher_datadir_Changed(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Changed) != WatcherChangeTypes.Changed)
                return;

            int nRet = 0;

            // this.WriteErrorLog("file1='"+this.m_strFileName+"' file2='" + e.FullPath + "'");
            if (PathUtil.IsEqual(this.m_strFileName, e.FullPath) == true)
            {
                string strError = "";

                // 稍微延时一下，避免很快地重装、正好和尚在改写 opac.xml 文件的的进程发生冲突
                Thread.Sleep(500);

                nRet = this.Load(
                    true,
                    this.DataDir,
                    this.HostDir,
                    out strError);
                if (nRet == -1)
                {
                    strError = "reload " + this.m_strFileName + " error: " + strError;
                    this.WriteErrorLog(strError);
                    this.GlobalErrorInfo = strError;
                }
                else
                {
                    this.GlobalErrorInfo = "";
                }
            }

            // 监视 webui.xml
            if (PathUtil.IsEqual(this.m_strWebuiFileName, e.FullPath) == true)
            {
                string strError = "";

                // 稍微延时一下，避免很快地重装、正好和尚在改写 webui.xml 文件的的进程发生冲突
                Thread.Sleep(500);

                nRet = this.LoadWebuiCfgDom(out strError);
                if (nRet == -1)
                {
                    strError = "reload " + this.m_strWebuiFileName + " error: " + strError;
                    this.WriteErrorLog(strError);
                    this.GlobalErrorInfo = strError;
                }
                else
                {
                    this.GlobalErrorInfo = "";
                }
            }
        }

        // reutrn:
        //      -1  error
        //      0   not found start.xml
        //      1   found start.xml
        public static int GetDataDir(string strStartXmlFileName,
            out string strDataDir,
            out string strError)
        {
            strError = "";
            strDataDir = "";

            if (File.Exists(strStartXmlFileName) == false)
            {
                strError = "文件 " + strStartXmlFileName + " 不存在...";
                return 0;
            }

            // 已存在start.xml文件
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strStartXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "加载start.xml到dom出错：" + ex.Message;
                return -1;
            }

            strDataDir = DomUtil.GetAttr(dom.DocumentElement, "datadir");
            if (strDataDir == "")
            {
                strError = "start.xml文件中根元素未定义'datadir'属性，或'datadir'属性值为空。";
                return -1;
            }

            if (Directory.Exists(strDataDir) == false)
            {
                strError = "start.xml文件中根元素'datadir'属性定义的数据目录 '" + strDataDir + "' 不存在。";
                return -1;
            }

            return 1;
        }


        public void Close()
        {
            try
            {
                // 停止所有长操作
                //this.StopAll();

                // 2015/1/22
                this.EndWather();

                // 2013/12/24
                this.BatchTasks.Close();

                if (this.ChatRooms != null)
                    this.ChatRooms.Close();

#if OPAC_SEARCH_LOG
                // 2012/12/17
                // 将内存中的检索日志对象写入数据库
                if (this.SearchLog != null)
                {
                    string strError = "";
                    this.SearchLog.Flush(out strError);
                }
#endif

                if (this.ChannelPool != null)
                {
                    this.ChannelPool.Close();
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("OpacApplication Close()俘获异常: " + ExceptionUtil.GetDebugText(ex));
            }

            this.WriteErrorLog("opac service被停止。");

            this.RemoveAppDownDetectFile();	// 删除检测文件
        }

        // 将XML装入DOM
        public static int LoadToDom(string strXml,
            out XmlDocument dom,
            out string strError)
        {
            strError = "";

            dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错：" + ex.Message;
                return -1;
            }

            return 0;
        }

        /*
    <virtualDatabases searchMaxResultCount="5000">
        <database name="中文图书" alias="cbook">
            <caption lang="zh">中文图书</caption>
            <from name="ISBN" style="isbn">
                <caption lang="zh-CN">ISBN</caption>
                <caption lang="en">ISBN</caption>
            </from>
         * */
        // 根据“禁用数据库名”列表，修改 virtualDatabases 元素内的细部
        void ModifyVirtualDatabases(XmlElement root,
            string strHideDbNames)
        {
            // Debug.Assert(false, "");

            List<string> hide_dbnames = StringUtil.SplitList(strHideDbNames);

            XmlNodeList databases = root.SelectNodes("database | virtualDatabase");
            foreach (XmlElement database in databases)
            {
                XmlNodeList captions = database.SelectNodes("caption");
                bool bFound = false;
                foreach (XmlNode caption in captions)
                {
                    if (hide_dbnames.IndexOf(caption.InnerText.Trim()) != -1)
                        bFound = true;
                }

#if NO
                if (bFound == false)
                    database.RemoveAttribute("hide");
                else
                    database.SetAttribute("hide", "true");
#endif
                if (bFound == true)
                    database.ParentNode.RemoveChild(database);
            }
        }

        // 初始化虚拟库集合定义对象
        public int InitialVdbs(out string strError)
        {
            strError = "";

            if (this.vdbs != null)
                return 0;   // 优化

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                XmlNode root = this.OpacCfgDom.DocumentElement.SelectSingleNode(
                    "virtualDatabases");
                if (root == null)
                {
                    strError = "尚未配置<virtualDatabases>元素";
                    return -1;
                }

                this.vdbs = new VirtualDatabaseCollection();
                int nRet = vdbs.Initial(root,
                    out strError);
                if (nRet == -1)
                {
                    this.vdbs = null;   // 2011/2/12
                    return -1;
                }

                return 0;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 构造虚拟库的XML检索式
        public int BuildVirtualQuery(
            List<VirtualDatabase> vdb_list, // 2011/9/15 改造为数组形式
            string strWord,
            string strVirtualFromName,
            string strMatchStyle,
            int nMaxCount,
            string strSearchStyle,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            // 2011/11/23
            // 检索词为空的时候对匹配方式"exact"要特殊处理
            if (string.IsNullOrEmpty(strWord) == true
                && strMatchStyle == "exact")
                strMatchStyle = "left";

            bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

            int nTotalUsed = 0;
            string strLogic = "OR";

            foreach (VirtualDatabase vdb in vdb_list)
            {
                List<string> realdbs = vdb.GetRealDbNames();

                if (realdbs.Count == 0)
                {
                    strError = "虚拟库 '" + vdb.GetName(null) + "' 下居然没有定义任何物理库";
                    return -1;
                }

                string strWarning = "";
                int nUsed = 0;

                for (int i = 0; i < realdbs.Count; i++)
                {
                    // 数据库名
                    string strDbName = realdbs[i];
                    string strDebugInfo = "";
                    string strFrom = vdb.GetRealFromName(
                        strDbName,
                        strVirtualFromName,
                        this.DebugMode,
                        out strDebugInfo);
                    if (this.DebugMode == true)
                    {
                        this.WriteDebugInfo(strDebugInfo);
                    }
                    if (strFrom == null)
                    {
                        strWarning += "虚拟库 '" + vdb.GetName(null) + "' 中针对物理库 '" + strDbName + "'(length=" + strDbName.Length + ") 中对虚拟From '" + strVirtualFromName + "'(length=" + strVirtualFromName.Length + ") 未找到对应的物理From名; ";
                        // strError = "虚拟库 '" + vdb.GetName(null) + " '中针对物理库 '" + strDbName + "' 中对虚拟From '" + strVirtualFromName + "' 未找到对应的物理From名";
                        // return -1;
                        continue;
                    }

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                        + "<word>"
                        + StringUtil.GetXmlStringSimple(strWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType>"
                        + "<maxCount>" + nMaxCount.ToString() + "</maxCount></item><lang>zh</lang></target>";

                    if (i > 0)
                        strXml += "<operator value='" + strLogic + "'/>";

                    strXml += strOneDbQuery;

                    nUsed++;
                }

                // 一个物理库也没有匹配上
                if (nUsed == 0)
                {
                    strError = strWarning;
                    return -1;
                }

                nTotalUsed += nUsed;
            }

            if (nTotalUsed > 1)
            {
                strXml = "<group>" + strXml + "</group>";
            }

            return 0;
        }

        // 根据检索参数创建XML检索式
        // return:
        //      -1  出错
        //      0   不存在所指定的数据库或者检索途径。一个都没有
        //      1   成功
        public static int BuildQueryXml(
            OpacApplication app,
            string strDbName,
            string strWord,
            string strFrom,
            string strMatchStyle,
            string strRelation,
            string strDataType,
            int nMaxCount,
            string strSearchStyle,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (app == null)
            {
                strError = "app == null";
                return -1;
            }

            if (app.vdbs == null)
            {
                strError = "app.vdbs == null";
                return -1;
            }

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "strDbName参数不能为空。";
                return -1;
            }

            bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

            if (String.IsNullOrEmpty(strMatchStyle) == true)
                strMatchStyle = "middle";

            if (String.IsNullOrEmpty(strRelation) == true)
                strRelation = "=";

            if (String.IsNullOrEmpty(strDataType) == true)
                strDataType = "string";

            string strOneDbQuery = "";

#if NO
            //
            // 数据库是不是虚拟库?
            VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器


            // 如果是虚拟库
            if (vdb != null && vdb.IsVirtual == true)
            {
                int nRet = BuildVirtualQuery(
                    vdb,
                    strWord,
                    strFrom,
                    strMatchStyle,
                    nMaxCount,
                    out strOneDbQuery,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif
            int nTotalUsed = 0;

            // *** 第一步 把虚拟库名和普通库名分离
            int nRet = SeperateVirtualDbs(
            app,
            strDbName,
            out List<VirtualDatabase> vdb_list,
            out string strNormalDbNameList,
            out strError);
            if (nRet == -1)
                return -1;

            // *** 第二步，把虚拟库名创建为检索式
            if (vdb_list.Count > 0)
            {
                nRet = app.BuildVirtualQuery(
                    vdb_list,
                    strWord,
                    strFrom,
                    strMatchStyle,
                    nMaxCount,
                    strSearchStyle,
                    out strOneDbQuery,
                    out strError);
                if (nRet == -1)
                    return -1;
                strXml += strOneDbQuery;
                nTotalUsed++;
            }

            // *** 第三步：把普通库名创建成检索式
            if (string.IsNullOrEmpty(strNormalDbNameList) == false)
            {
#if NO
                if (String.IsNullOrEmpty(strDbName) == true
                    || strDbName.ToLower() == "<all>"
                    || strDbName == "<全部>")
                {
                    List<string> found_dup = new List<string>();    // 用于去重

                    if (app.vdbs.Count == 0)
                    {
                        strError = "目前opac.xml中<virtualDatabases>内尚未具备检索目标。(注：需要在dp2Library服务器的library.xml中配置相关事项，然后重启dp2OPAC)";
                        return 0;
                    }


                    // 所有虚拟库包含的去重后的物理库名 和全部物理名 (整体去重一次)
                    // 要注意检查特定的from名在物理库中是否存在，如果不存在则排除该库名
                    for (int j = 0; j < app.vdbs.Count; j++)
                    {
                        VirtualDatabase temp_vdb = app.vdbs[j];  // 需要增加一个索引器

                        // 忽略具有notInAll属性的库
                        if (temp_vdb.NotInAll == true)
                            continue;

                        List<string> realdbs = new List<string>();

                        // if (temp_vdb.IsVirtual == true)
                        realdbs = temp_vdb.GetRealDbNames();

                        for (int k = 0; k < realdbs.Count; k++)
                        {
                            // 数据库名
                            string strOneDbName = realdbs[k];

                            if (found_dup.IndexOf(strOneDbName) != -1)
                                continue;

                            strTargetList += StringUtil.GetXmlStringSimple(strOneDbName + ":" + strFrom) + ";";

                            found_dup.Add(strOneDbName);
                        }
                    }
                }
                else if (String.IsNullOrEmpty(strDbName) == true
                    || strDbName.ToLower() == "<all items>"
                    || strDbName == "<全部实体>"
                    || strDbName.ToLower() == "<all comments>"
                    || strDbName == "<全部评注>")
                {
                    if (app.ItemDbs.Count == 0)
                    {
                        strError = "目前opac.xml中<biblioDbGroup>内尚未具有数据库。(注：需要在dp2Library服务器的library.xml中配置相关事项，然后重启dp2OPAC)";
                        return -1;
                    }

                    string strDbType = "";
                    if (strDbName.ToLower() == "<all items>"
                    || strDbName == "<全部实体>")
                        strDbType = "item";
                    else if (strDbName.ToLower() == "<all comments>"
                    || strDbName == "<全部评注>")
                        strDbType = "comment";
                    else
                    {
                        Debug.Assert(false, "");
                    }


                    for (int j = 0; j < app.ItemDbs.Count; j++)
                    {
                        ItemDbCfg cfg = app.ItemDbs[j];

                        string strOneDbName = "";

                        if (strDbType == "item")
                            strOneDbName = cfg.DbName;
                        else if (strDbType == "comment")
                            strOneDbName = cfg.CommentDbName;

                        if (String.IsNullOrEmpty(strOneDbName) == true)
                            continue;
                        strTargetList += StringUtil.GetXmlStringSimple(strOneDbName + ":" + strFrom) + ";";
                    }
                }
                else
                {
                    strTargetList = StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom);
                }

#endif

                // parameters:
                //      strDbNameList   数据库名列表。用逗号间隔每个数据库名。单个的数据库名位置可以用 <全部> <全部实体> <全部评注> 等代替。
                //      strFromList     检索途径名列表。用逗号间隔每个From名
                nRet = BuildTargetList(
            app,
            strNormalDbNameList,
            strFrom,
            out string strTargetList,
            out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strTargetList) == true)
                {
                    strError = "不具备任何检索目标";
                    return 0;
                }

                // 2011/12/9
                // 检索词为空的时候对匹配方式"exact"要特殊处理
                if (string.IsNullOrEmpty(strWord) == true
                    && strMatchStyle == "exact")
                    strMatchStyle = "left";

                strOneDbQuery = "<target list='"
                    + strTargetList
                    + "'><item>"
                    + (bDesc == true ? "<order>DESC</order>" : "")
                    + "<word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word><match>"
                    + StringUtil.GetXmlStringSimple(strMatchStyle)
                    + "</match>"
                    + "<relation>"
                    + StringUtil.GetXmlStringSimple(strRelation)
                    + "</relation>"
                    + "<dataType>"
                    + StringUtil.GetXmlStringSimple(strDataType)
                    + "</dataType>"
                    + "<maxCount>" + nMaxCount.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (string.IsNullOrEmpty(strXml) == false)
                    strXml += "<operator value='OR'/>";

                strXml += strOneDbQuery;
                nTotalUsed++;
            }

            if (nTotalUsed > 1)
            {
                strXml = "<group>" + strXml + "</group>";
            }

            // TODO: 增加 strLocationFilter 功能

            // 2018/12/5
            // 对书目检索的额外过滤
            if (string.IsNullOrEmpty(app.BiblioFilter) == false)
            {
                string strOperator = "AND";
                string strFilter = app.BiblioFilter;
                if (strFilter.StartsWith("-"))
                {
                    strFilter = strFilter.Substring(1);
                    strOperator = "SUB";
                }
                string strSubQueryXml = "<item resultset='#" + strFilter + "' />";
                strXml = $"<group>{strXml}<operator value='{strOperator}'/>{strSubQueryXml}</group>";
            }

            return 1;
        }

        // 将数据库名列表分离为虚拟库数组和普通库名列表两部分
        static int SeperateVirtualDbs(
            OpacApplication app,
            string strDbNameList,
            out List<VirtualDatabase> vdb_list,
            out string strNormalDbNameList,
            out string strError)
        {
            strError = "";
            vdb_list = new List<VirtualDatabase>();
            strNormalDbNameList = "";

            string[] dbnames = strDbNameList.Split(new char[] { ',' });
            for (int i = 0; i < dbnames.Length; i++)
            {
                string strDbName = dbnames[i].Trim();
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                //
                // 数据库是不是虚拟库?
                VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器

                // 2017/12/21
                // 既不是虚拟库也不是普通库
                // 2018/1/8 注: 特殊的名字，例如 "<全部实体>" 等要继续向后处理
                if (vdb == null && !(strDbName.StartsWith("<") && strDbName.EndsWith(">")))
                {
                    strError = "数据库名 '" + strDbName + "' 不存在";
                    return -1;
                }

                // 如果是虚拟库
                if (vdb != null && vdb.IsVirtual == true)
                    vdb_list.Add(vdb);
                else
                {
                    if (string.IsNullOrEmpty(strNormalDbNameList) == false)
                        strNormalDbNameList += ",";
                    strNormalDbNameList += strDbName;
                }
            }

            return 0;
        }

        // parameters:
        //      strDbNameList   数据库名列表。用逗号间隔每个数据库名。单个的数据库名位置可以用 <全部> <全部实体> <全部评注> 等代替。
        //      strFromList     检索途径名列表。用逗号间隔每个From名
        static int BuildTargetList(
            OpacApplication app,
            string strDbNameList,
            string strFromList,
            out string strTargetList,
            out string strError)
        {
            strError = "";
            strTargetList = "";

            string[] dbnames = strDbNameList.Split(new char[] { ',' });
            for (int i = 0; i < dbnames.Length; i++)
            {
                string strDbName = dbnames[i].Trim();
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                // 全部数据库是指<virutualDatabases>里面定义的数据库。可能比<biblioDbGroup>里面定义的范围要窄
                if (String.IsNullOrEmpty(strDbName) == true
                    || strDbName.ToLower() == "<all>"
                    || strDbName == "<全部>")
                {
                    List<string> found_dup = new List<string>();    // 用于去重

                    if (app.vdbs.Count == 0)
                    {
                        strError = "目前opac.xml中<virtualDatabases>内尚未具备检索目标。(注：需要在dp2Library服务器的library.xml中配置相关事项，然后重启dp2OPAC)";
                        return 0;
                    }

                    // 所有虚拟库包含的去重后的物理库名 和全部物理名 (整体去重一次)
                    // 要注意检查特定的from名在物理库中是否存在，如果不存在则排除该库名
                    for (int j = 0; j < app.vdbs.Count; j++)
                    {
                        VirtualDatabase temp_vdb = app.vdbs[j];  // 需要增加一个索引器

                        // 忽略具有notInAll属性的库
                        if (temp_vdb.NotInAll == true)
                            continue;

                        List<string> realdbs = new List<string>();

                        // if (temp_vdb.IsVirtual == true)
                        realdbs = temp_vdb.GetRealDbNames();
                        // TODO: 是否需要把物理数据库名列表去重?

                        for (int k = 0; k < realdbs.Count; k++)
                        {
                            // 数据库名
                            string strOneDbName = realdbs[k];

                            if (found_dup.IndexOf(strOneDbName) != -1)
                                continue;

                            strTargetList += StringUtil.GetXmlStringSimple(strOneDbName + ":" + strFromList) + ";";

                            found_dup.Add(strOneDbName);
                        }
                    }
                }
                // 全部实体，指的是<biblioDbGroup>中的全部实体库
                else if (String.IsNullOrEmpty(strDbName) == true
                    || strDbName.ToLower() == "<all items>"
                    || strDbName == "<全部实体>"
                    || strDbName.ToLower() == "<all comments>"
                    || strDbName == "<全部评注>")
                {
                    if (app.ItemDbs.Count == 0)
                    {
                        strError = "目前opac.xml中<biblioDbGroup>内尚未具有数据库。(注：需要在dp2Library服务器的library.xml中配置相关事项，然后重启dp2OPAC)";
                        return -1;
                    }

                    string strDbType = "";
                    if (strDbName.ToLower() == "<all items>"
                    || strDbName == "<全部实体>")
                        strDbType = "item";
                    else if (strDbName.ToLower() == "<all comments>"
                    || strDbName == "<全部评注>")
                        strDbType = "comment";
                    else
                    {
                        Debug.Assert(false, "");
                    }

                    for (int j = 0; j < app.ItemDbs.Count; j++)
                    {
                        ItemDbCfg cfg = app.ItemDbs[j];

                        string strOneDbName = "";

                        if (strDbType == "item")
                            strOneDbName = cfg.DbName;
                        else if (strDbType == "comment")
                            strOneDbName = cfg.CommentDbName;

                        if (String.IsNullOrEmpty(strOneDbName) == true)
                            continue;
                        strTargetList += StringUtil.GetXmlStringSimple(strOneDbName + ":" + strFromList) + ";";
                    }
                }
                else
                {
                    strTargetList += StringUtil.GetXmlStringSimple(strDbName + ":" + strFromList) + ";";
                }
            }

            return 0;
        }

        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;
            }
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Server.res.OpacApplication",
                typeof(OpacApplication).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception ex)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。" + ex.Message;
            }
        }

        // 把整个字符串中的时间单位变换为可读的形态
        // 语言相关的最新版本
        public string GetDisplayTimePeriodStringEx(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            strText = strText.Replace("day", this.GetString("天"));
            return strText.Replace("hour", this.GetString("小时"));
        }

        // 通过册条码号得知从属的种记录路径
        // (注：采用了代理账户)
        // parameters:
        //      strItemBarcode  册条码号
        //      strReaderBarcodeParam 借阅者证条码号。用于条码号重复的时候附加判断。
        // return:
        //      -1  error
        //      0   册记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPath(
            // SessionInfo sessioninfo,
            string strItemBarcode,
            // string strReaderBarcode,
            out string strBiblioRecPath,
            //out string strWarning,
            out string strError)
        {
            strError = "";
            // strWarning = "";
            strBiblioRecPath = "";
            // int nRet = 0;

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;
            string strBiblio = "";

            int nResultCount = 0;

            LibraryChannel channel = this.GetChannel();
            try
            {
                long lRet = // sessioninfo.Channel.
                    channel.GetItemInfo(
                    null,
                    strItemBarcode,
                    "", // strResultType
                    out strItemXml,
                    out strOutputItemPath,
                    out item_timestamp,
                    "recpath",  // strBiblioType
                    out strBiblio,
                    out strBiblioRecPath,
                    out strError);

                if (lRet == 0)
                {
                    strError = "册条码号为 '" + strItemBarcode + "' 的册记录没有找到";
                    return 0;
                }
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.AccessDenied)
                    {
                        strError = "用户身份 '" + channel.UserName + "' 获取册记录失败: " + strError;
                    }
                    return -1;
                }

                nResultCount = (int)lRet;
                return nResultCount;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 通过评注记录路径得知从属的种记录路径
        // (注：采用了代理账户)
        // parameters:
        // return:
        //      -1  error
        //      0   评注记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPathByCommentRecPath(
            // SessionInfo sessioninfo,
            string strCommentRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            // int nRet = 0;

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;

            {
                // string strCommentDbName0 = ResPath.GetDbName(strCommentRecPath);
                string strCommentDbName0 = StringUtil.GetDbName(strCommentRecPath);
                // 需要检查一下数据库名是否在允许的实体库名之列
                if (this.IsCommentDbName(strCommentDbName0) == false)
                {
                    strError = "评注记录路径 '" + strCommentRecPath + "' 中的数据库名 '" + strCommentDbName0 + "' 不在配置的评注库名之列，因此拒绝操作。";
                    return -1;
                }
            }

            LibraryChannel channel = this.GetChannel();
            try
            {
                string strBiblio = "";
                long lRet = // sessioninfo.Channel.
                    channel.GetCommentInfo(
    null,
    "@path:" + strCommentRecPath,
    // null,
    "xml", // strResultType
    out strItemXml,
    out strOutputItemPath,
    out item_timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strBiblioRecPath,
    out strError);
                if (lRet == -1)
                {
                    strError = "获取评注记录 '" + strCommentRecPath + "' 时出错: " + strError;
                    return -1;
                }

                return 1;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 通过册记录路径得知从属的种记录路径
        // (注：采用了代理账户)
        // parameters:
        // return:
        //      -1  error
        //      0   册记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPathByItemRecPath(
            // SessionInfo sessioninfo,
            string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;

            {
                // string strItemDbName0 = ResPath.GetDbName(strItemRecPath);
                string strItemDbName0 = StringUtil.GetDbName(strItemRecPath);
                // 需要检查一下数据库名是否在允许的实体库名之列
                if (this.IsItemDbName(strItemDbName0) == false)
                {
                    strError = "册记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName0 + "' 不在配置的实体库名之列，因此拒绝操作。";
                    return -1;
                }
            }

            LibraryChannel channel = this.GetChannel();
            try
            {
                string strBiblio = "";
                long lRet = // sessioninfo.Channel.
                    channel.GetItemInfo(
    null,
    "@path:" + strItemRecPath,
    "xml", // strResultType
    out strItemXml,
    out strOutputItemPath,
    out item_timestamp,
    "recpath",  // strBiblioType
    out strBiblio,
    out strBiblioRecPath,
    out strError);
                if (lRet == -1)
                {
                    strError = "获取册记录 '" + strItemRecPath + "' 时出错: " + strError;
                    return -1;
                }

                return 1;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        public void ActivateCacheBuilder()
        {
            // 启动CacheBuilder
            try
            {
                if (this.CacheBuilder == null)
                {
                    this.CacheBuilder = new CacheBuilder(this, null);
                    this.BatchTasks.Add(this.CacheBuilder);
                }
                this.CacheBuilder.StartWorkerThread();
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("启动批处理任务CacheBuilder时出错：" + ex.Message);
            }

            this.CacheBuilder.Activate();
        }

        // 加密明文
        public static string EncryptPassword(string PlainText)
        {
            return Cryptography.Encrypt(PlainText, EncryptKey);
        }

        // 解密加密过的文字
        public static string DecryptPassword(string EncryptText)
        {
            return Cryptography.Decrypt(EncryptText, EncryptKey);
        }

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(string strTimeString,
            string strPeriod)
        {
            string strError = "";
            long lValue = 0;
            string strUnit = "";
            int nRet = OpacApplication.ParsePeriodUnit(strPeriod,
                        out lValue,
                        out strUnit,
                        out strError);
            if (nRet == -1)
                strUnit = "day";
            if (strUnit == "day")
                return DateTimeUtil.LocalDate(strTimeString);

            return DateTimeUtil.LocalTime(strTimeString);
        }

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(DateTime time,
            string strPeriod)
        {
            string strError = "";
            long lValue = 0;
            string strUnit = "";
            int nRet = OpacApplication.ParsePeriodUnit(strPeriod,
                        out lValue,
                        out strUnit,
                        out strError);
            if (nRet == -1)
                strUnit = "day";
            if (strUnit == "day")
                return time.ToString("d");  // 精确到日

            return time.ToString("g");  // 精确到分钟。G精确到秒
            // http://www.java2s.com/Tutorial/CSharp/0260__Date-Time/UsetheToStringmethodtoconvertaDateTimetoastringdDfFgGmrstTuUy.htm
        }

        // 分析期限参数
        public static int ParsePeriodUnit(string strPeriod,
            out long lValue,
            out string strUnit,
            out string strError)
        {
            lValue = 0;
            strUnit = "";
            strError = "";

            strPeriod = strPeriod.Trim();

            if (String.IsNullOrEmpty(strPeriod) == true)
            {
                strError = "期限字符串为空";
                return -1;
            }

            string strValue = "";


            for (int i = 0; i < strPeriod.Length; i++)
            {
                if (strPeriod[i] >= '0' && strPeriod[i] <= '9')
                {
                    strValue += strPeriod[i];
                }
                else
                {
                    strUnit = strPeriod.Substring(i).Trim();
                    break;
                }
            }

            // 将strValue转换为数字
            try
            {
                lValue = Convert.ToInt64(strValue);
            }
            catch (Exception)
            {
                strError = "期限参数数字部分'" + strValue + "'格式不合法";
                return -1;
            }

            if (String.IsNullOrEmpty(strUnit) == true)
                strUnit = "day";   // 缺省单位为"天"

            strUnit = strUnit.ToLower();    // 统一转换为小写

            return 0;
        }

        // 获得读者证号二维码字符串
        // (注：采用了代理账户)
        public int GetPatronTempId(
            string strReaderBarcode,
            out string strCode,
            out string strError)
        {
            strError = "";
            strCode = "";

            LibraryChannel channel = this.GetChannel();
            try
            {

                // 读入读者记录
                long lRet = // session.Channel.
                    channel.VerifyReaderPassword(null,
                    "!getpatrontempid:" + strReaderBarcode,
                    null,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    // text-level: 内部错误
                    strError = "获得读者证号二维码时发生错误: " + strError;
                    return -1;
                }

                strCode = strError;
                return 0;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

#if NO
        // 获得读者证号二维码字符串
        public int GetPatronTempId(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            out string strCode,
            out string strError)
        {
            strError = "";
            strCode = "";

#if NO
            // 临时的SessionInfo对象
            SessionInfo session = sessioninfo;

            if (sessioninfo == null)
            {
                session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
            }
#endif
            LibraryChannel channel = null;
            if (sessioninfo == null)
                channel = this.GetChannel();
            else
                channel = sessioninfo.Channel;

            try
            {

                // 读入读者记录
                long lRet = // session.Channel.
                    channel.VerifyReaderPassword(null,
                    "!getpatrontempid:" + strReaderBarcode,
                    null,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    // text-level: 内部错误
                    strError = "获得读者证号二维码时发生错误: " + strError;
                    return -1;
                }

                strCode = strError;
                return 0;
            }
            finally
            {
#if NO
                if (sessioninfo == null)
                    session.CloseSession();
#endif
                if (sessioninfo == null)
                    this.ReturnChannel(channel);
            }
        }
#endif

        // 根据读者证条码号找到头像资源路径
        // (注：采用了代理账户)
        // parameters:
        //      strReaderBarcode    读者证条码号
        //      strEncryptBarcode   如果strEncryptBarcode有内容，则用它，而不用strReaderBarcode
        //      strDisplayName  供验证的显示名。可以为null，表示不验证
        // return:
        //      -1  出错
        //      0   没有找到。包括读者记录不存在，或者读者记录里面没有头像对象
        //      1   找到
        public int GetReaderPhotoPath(
            // SessionInfo sessioninfo,
            string strReaderBarcode,
            string strEncyptBarcode,
            string strDisplayName,
            out string strPhotoPath,
            out string strError)
        {
            strError = "";
            strPhotoPath = "";

            if (String.IsNullOrEmpty(strEncyptBarcode) == false)
            {
                string strTemp = OpacApplication.DecryptPassword(strEncyptBarcode);
                if (strTemp == null)
                {
                    strError = "strEncyptBarcode中包含的文字格式不正确";
                    return -1;
                }
                strReaderBarcode = strTemp;
            }

#if NO
            // 临时的SessionInfo对象
            SessionInfo session = new SessionInfo(this);
            session.UserID = this.ManagerUserName;
            session.Password = this.ManagerPassword;
            session.IsReader = false;
#endif
            LibraryChannel channel = this.GetChannel();

            try
            {
                // 读入读者记录
                string strReaderXml = "";
                byte[] reader_timestamp = null;
                string strOutputReaderRecPath = "";

                string strResultTypeList = "xml";
                string[] results = null;
                long lRet = // session.Channel.
                    channel.GetReaderInfo(null,
                    // this.dp2LibraryVersion >= 2.22 ?
                    StringUtil.CompareVersion(this.dp2LibraryVersion, "2.22") >= 0 ?
                    "@barcode:" + strReaderBarcode // dp2Library V2.22 及以后可以使用这个方法
                    : strReaderBarcode,
                    strResultTypeList,
                    out results,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (lRet == 0)
                {
                    strError = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    return 0;
                }
                if (lRet == -1)
                {
                    // text-level: 内部错误
                    strError = "读入读者记录时发生错误: " + strError;
                    return -1;
                }
                if (lRet > 1)
                {
                    // text-level: 内部错误
                    strError = "读入读者记录时，发现读者证条码号 '" + strReaderBarcode + "' 命中 " + lRet.ToString() + " 条，这是一个严重错误，请系统管理员尽快处理。";
                    return -1;
                }
                if (results.Length != 1)
                {
                    strError = "results.Length error";
                    return -1;
                }

                strReaderXml = results[0];

                XmlDocument readerdom = null;
                int nRet = OpacApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 验证显示名
                if (String.IsNullOrEmpty(strDisplayName) == false)
                {
                    string strDisplayNameValue = DomUtil.GetElementText(readerdom.DocumentElement,
                            "displayName");
                    if (strDisplayName.Trim() != strDisplayNameValue.Trim())
                    {
                        strError = "虽然读者记录找到了，但是显示名已经不匹配";
                        return 0;
                    }
                }

                // 看看是不是已经有图像对象

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);

                // 全部<dprms:file>元素
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("//dprms:file[@usage='photo']", nsmgr);

                if (nodes.Count == 0)
                {
                    strError = "读者记录中没有头像对象";
                    return 0;
                }

                strPhotoPath = strOutputReaderRecPath + "/object/" + DomUtil.GetAttr(nodes[0], "id");

                return 1;
            }
            finally
            {
#if NO
                session.CloseSession();
#endif
                this.ReturnChannel(channel);
            }
        }

        // 2021/7/8
        // 检查上载文件的格式是否为图像文件？
        // return:
        //      -1  出错
        //      0   不是图像文件
        //      1   是图像文件
        public static int VerifyImageFileType(HttpPostedFile postedFile,
    out string strError)
        {
            strError = "";

            try
            {
                string mime = API.GetMimeTypeFromFile(postedFile.InputStream);
                if (mime == null
                    || mime.StartsWith("image/") == false)
                {
                    strError = "上载的文件不是图像文件";
                    return 0;
                }
                return 1;
            }
            catch (Exception ex)
            {
                strError = $"验证上载文件格式时出错: {ex.Message}";
                return -1;
            }
            finally
            {
                postedFile.InputStream.Seek(0, SeekOrigin.Begin);
            }
        }

        // 保存上载的文件
        // (注：采用了代理账户)
        // 注: 本函数会检查上载的文件是否为图像文件，如果不是图像文件会报错
        public int SaveUploadFile(
    System.Web.UI.Page page,
    string strXmlRecPath,
    string strFileID,
    string strResTimeStamp,
    HttpPostedFile postedFile,
    int nLogoLimitW,
    int nLogoLimitH,
    out string strError)
        {
            strError = "";

            // 检查上载文件的格式是否为图像文件？
            // return:
            //      -1  出错
            //      0   不是图像文件
            //      1   是图像文件
            int nRet = VerifyImageFileType(postedFile,
        out strError);
            if (nRet != 1)
            {
                strError += "。上载失败";
                return -1;
            }
#if NO
            // 临时的SessionInfo对象
            SessionInfo session = new SessionInfo(this);
            session.UserID = this.ManagerUserName;
            session.Password = this.ManagerPassword;
            session.IsReader = false;
#endif
            LibraryChannel channel = this.GetChannel();

            try
            {
                return SaveUploadFile(page,
                    channel,    // session.Channel,
                    strXmlRecPath,
                    strFileID,
                    strResTimeStamp,
                    postedFile,
                    nLogoLimitW,
                    nLogoLimitH,
                    out strError);
            }
            finally
            {
#if NO
                session.CloseSession();
#endif
                this.ReturnChannel(channel);
            }
        }

        // 保存资源
        // return:
        //		-1	error
        //		0	发现上载的文件其实为空，不必保存了
        //		1	已经保存
        public static int SaveUploadFile(
            System.Web.UI.Page page,
            LibraryChannel channel,
            string strXmlRecPath,
            string strFileID,
            string strResTimeStamp,
            HttpPostedFile postedFile,
            int nLogoLimitW,
            int nLogoLimitH,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(postedFile.FileName) == true
                && postedFile.ContentLength == 0)
            {
                return 0;	// 没有必要保存
            }

            WebPageStop stop = new WebPageStop(page);

            string strResPath = strXmlRecPath + "/object/" + strFileID;

            string strLocalFileName = Path.GetTempFileName();
            try
            {
                using (Stream t = File.Create(strLocalFileName))
                {
                    // 缩小尺寸
                    // return:
                    //      -1  出错
                    //      0   没有必要缩放(oTarget未处理)
                    //      1   已经缩放
                    int nRet = GraphicsUtil.ShrinkPic(postedFile.InputStream,
                            postedFile.ContentType,
                            nLogoLimitW,
                            nLogoLimitH,
                            true,
                            t,
                            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)  // 没有必要缩放
                    {
                        postedFile.InputStream.Seek(0, SeekOrigin.Begin);
                        StreamUtil.DumpStream(postedFile.InputStream, t);
                    }
                }

                // t.Close();


                // 检测文件尺寸
                FileInfo fi = new FileInfo(strLocalFileName);

                if (fi.Exists == false)
                {
                    strError = "文件 '" + strLocalFileName + "' 不存在...";
                    return -1;
                }

                string[] ranges = null;

                if (fi.Length == 0)
                { // 空文件
                    ranges = new string[1];
                    ranges[0] = "";
                }
                else
                {
                    string strRange = "";
                    strRange = "0-" + Convert.ToString(fi.Length - 1);

                    // 按照100K作为一个chunk
                    ranges = RangeList.ChunkRange(strRange,
                        channel.UploadResChunkSize // 100 * 1024
                        );
                }

                byte[] timestamp = ByteArray.GetTimeStampByteArray(strResTimeStamp);
                byte[] output_timestamp = null;

                // 2007/12/13 
                string strLastModifyTime = DateTime.UtcNow.ToString("u");

                string strLocalPath = postedFile.FileName;

            // page.Response.Write("<br/>正在保存" + strLocalPath);

            REDOWHOLESAVE:
                string strWarning = "";

                for (int j = 0; j < ranges.Length; j++)
                {
                REDOSINGLESAVE:

                    // Application.DoEvents();	// 出让界面控制权

                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                        strWaiting = " 请耐心等待...";

                    string strPercent = "";
                    RangeList rl = new RangeList(ranges[j]);
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (stop != null)
                        stop.SetMessage("正在上载 " + ranges[j] + "/"
                            + Convert.ToString(fi.Length)
                            + " " + strPercent + " " + strLocalFileName + strWarning + strWaiting);

                    // page.Response.Write(".");	// 防止前端因等待过久而超时
                    long lRet = channel.SaveResObject(
stop,
strResPath,
                        strLocalFileName,
                        strLocalPath,
                        postedFile.ContentType,
                        ranges[j],
j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
timestamp,
out output_timestamp,
out strError);

                    /*
                    long lRet = channel.DoSaveResObject(strResPath,
                        strLocalFileName,
                        strLocalPath,
                        postedFile.ContentType,
                        strLastModifyTime,
                        ranges[j],
                        j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                        timestamp,
                        out output_timestamp,
                        out strError);
                     * */

                    timestamp = output_timestamp;

                    // DomUtil.SetAttr(node, "__timestamp",	ByteArray.GetHexTimeStampString(timestamp));

                    strWarning = "";

                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.TimestampMismatch)
                        {

                            timestamp = new byte[output_timestamp.Length];
                            Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                            strWarning = " (时间戳不匹配, 自动重试)";
                            if (ranges.Length == 1 || j == 0)
                                goto REDOSINGLESAVE;
                            goto REDOWHOLESAVE;
                        }

                        goto ERROR1;
                    }


                }


                return 1;   // 已经保存
            ERROR1:
                return -1;
            }
            finally
            {
                // 不要忘记删除临时文件
                File.Delete(strLocalFileName);
            }
        }

        // 获得计数器值
        // parameters:
        //      channel 通讯通道。如果为 null，则函数会自动使用管理员身份创建通道
        //      strName 名字。为 书目记录路径 + '|' + URL
        public long GetHitCount(LibraryChannel channel_param,
            string strName,
            out long lValue,
            out string strError)
        {
            lValue = 0;
            strError = "";

#if NO
            SessionInfo session = null;
            if (channel == null)
            {
                // 临时的SessionInfo对象
                // 用管理员身份
                session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
                channel = session.Channel;
            }
#endif
            LibraryChannel channel = null;
            if (channel_param != null)
                channel = channel_param;
            else
                channel = this.GetChannel();

            try
            {
                return channel.HitCounter("get",
                    strName,
                    "",
                    out lValue,
                    out strError);
            }
            finally
            {
#if NO
                if (session != null)
                    session.CloseSession();
#endif
                if (channel_param == null)
                    this.ReturnChannel(channel);
            }
        }

        public bool IncSesstionCounter(string strClientAddress)
        {
            if (this.WebUiDom != null && this.WebUiDom.DocumentElement != null)
            {
                XmlElement sessionCounter = this.WebUiDom.DocumentElement.SelectSingleNode("counters/sessionCounter") as XmlElement;
                if (sessionCounter == null)
                    return false;
                string name = sessionCounter.GetAttribute("name");
                if (string.IsNullOrEmpty(name))
                    return false;
                this.IncHitCount("", name, strClientAddress);
                return true;
            }
            return false;
        }

        public bool IncHitCount(string strBiblioRecPath,
            string strURI,
            string strUserHostAddress)
        {
            string strError = "";
            long lRet = this.IncHitCount(null,
strBiblioRecPath + "|" + strURI,
strUserHostAddress,
false,    // 是否要创建日志
out strError);
            if (lRet == -1)
                return false;
            return true;
        }

        // 增量计数器值
        // parameters:
        //      channel 通讯通道。如果为 null，则函数会自动使用管理员身份创建通道
        //      strName 名字。为 书目记录路径 + '|' + URL
        // return:
        //      -1  出错
        //      0   mongodb 没有启用
        //      1   成功
        public long IncHitCount(LibraryChannel channel_param,
            string strName,
            string strClientAddress,
            bool bLog,
            out string strError)
        {
            strError = "";

#if NO
            SessionInfo session = null;
            if (channel == null)
            {
                // 临时的SessionInfo对象
                // 用管理员身份
                session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
                channel = session.Channel;
            }
#endif
            LibraryChannel channel = null;
            if (channel_param != null)
                channel = channel_param;
            else
                channel = this.GetChannel();

            try
            {
                long lValue = 0;
                return channel.HitCounter(bLog ? "inc_and_log" : "inc",
                    strName,
                    strClientAddress,
                    out lValue,
                    out strError);
            }
            finally
            {
#if NO
                if (session != null)
                    session.CloseSession();
#endif
                if (channel_param == null)
                    this.ReturnChannel(channel);
            }
        }

        // parameters:
        //      channel 通讯通道。如果为 null，则函数会自动使用管理员身份创建通道
        public int DownloadObject(System.Web.UI.Page Page,
            LibraryChannel channel_param,
            string strPath,
            bool bSaveAs,
            string strStyle,
            out string strError)
        {
            strError = "";

#if NO
            if (channel == null)
            {
                // 临时的SessionInfo对象
                // 用管理员身份
                SessionInfo session = new SessionInfo(this);
                session.UserID = this.ManagerUserName;
                session.Password = this.ManagerPassword;
                session.IsReader = false;
                try
                {
                    return DownloadObject0(Page,
                        session.Channel,
                        strPath,
                        bSaveAs,
                        strStyle,
                        out strError);
                }
                finally
                {
                    session.CloseSession();
                }
            }
            else
            {
                return DownloadObject0(Page,
    channel,
    strPath,
    bSaveAs,
    strStyle,
    out strError);
            }
#endif
            LibraryChannel channel = null;
            if (channel_param != null)
                channel = channel_param;
            else
                channel = this.GetChannel();

            try
            {
                return DownloadObject0(Page,
channel,
strPath,
bSaveAs,
strStyle,
out strError);
            }
            finally
            {
                if (channel_param == null)
                    this.ReturnChannel(channel);
            }
        }

        public int OutputQrImage(
            System.Web.UI.Page Page,
            string strCode,
            string strCharset,
            int nWidth,
            int nHeight,
            bool bDisableECI,
            bool bSaveAs,
            out string strError)
        {
            strError = "";

            if (nWidth <= 0)
                nWidth = 300;
            if (nHeight <= 0)
                nHeight = 300;

            if (bSaveAs == true)
            {
                string strEncodedFileName = HttpUtility.UrlEncode("qr.png", Encoding.UTF8);
                Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            Page.Response.ContentType = "image/jpeg";
#if NO
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Page.Response.AddHeader("Content-Length", strSize);
            }
#endif

            if (string.IsNullOrEmpty(strCharset) == true)
                strCharset = "ISO-8859-1";

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                // Options = new EncodingOptions
                Options = new QrCodeEncodingOptions
                {
                    Height = nHeight,
                    Width = nWidth,
                    DisableECI = bDisableECI,
                    ErrorCorrection = ErrorCorrectionLevel.L,
                    CharacterSet = strCharset // "UTF-8"
                }
            };

            using (var bitmap = writer.Write(strCode))
            {
                bitmap.Save(Page.Response.OutputStream, ImageFormat.Jpeg);
            }

            return 0;
        }

        // 下载对象资源
        // parameters:
        //      strStyle    如果包含 hitcount，表示希望获取访问计数的数字，返回图像格式。否则是希望返回对象本身
        // return:
        //      -1  出错
        //      0   304返回
        //      1   200返回
        public int DownloadObject0(System.Web.UI.Page Page,
            LibraryChannel channel,
            string strPath,
            bool bSaveAs,
            string strStyle,
            out string strError)
        {
            strError = "";

            if (StringUtil.IsInList("hitcount", strStyle) == true
                && StringUtil.IsInList("hitcount", this.SearchLogEnable) == false)
            {
                OpacApplication.OutputImage(Page,
                    Color.FromArgb(200, Color.Blue),
                    "*"); // 星号表示尚未启用内部对象计数功能
                return 1;
            }

            WebPageStop stop = new WebPageStop(Page);

            // strPath = boards.GetCanonicalUri(strPath);

            // 获得资源。写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
            // parameters:
            //		fileTarget	文件。注意在调用函数前适当设置文件指针位置。函数只会在当前位置开始向后写，写入前不会主动改变文件指针。
            //		strStyleParam	一般设置为"content,data,metadata,timestamp,outputpath";
            //		input_timestamp	若!=null，则本函数会把第一个返回的timestamp和本参数内容比较，如果不相等，则报错
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            // 只获得媒体类型
            long lRet = channel.GetRes(
                stop,
                strPath,
                0,
                0,
                "metadata",
                out byte[] baContent,
                out string strMetaData,
                out string strOutputPath,
                out byte[] baOutputTimeStamp,
                out strError);
            if (lRet == -1)
            {
                if (StringUtil.IsInList("hitcount", strStyle))
                {
                    OutputImage(Page,
                        Color.FromArgb(100, Color.Red),
                        "?");
                    return 1;
                }

                if (channel.ErrorCode == ErrorCode.AccessDenied)
                {
                    // 权限不够
                    return -1;
                }

                strError = "GetRes() '" + strPath + "' (for metadata) Error : " + strError;
                return -1;
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            // 取 metadata 中的 mime 类型信息
            Hashtable values = StringUtil.ParseMetaDataXml(strMetaData,
                out strError);
            if (values == null)
            {
                strError = "ParseMedaDataXml() Error :" + strError;
                return -1;
            }

            if (StringUtil.IsInList("hitcount", strStyle))
            {
                string strReadCount = (string)values["readCount"];
                if (string.IsNullOrEmpty(strReadCount) == true)
                    strReadCount = "?";
                OutputImage(Page,
                    Color.FromArgb(200, Color.DarkGreen),
                    strReadCount);
                return 1;
            }
#if NO
            RETURN_IMAGE:
            if (StringUtil.IsInList("hitcount", strStyle))
            {
                string strReadCount = (string)values["readCount"];
                if (string.IsNullOrEmpty(strReadCount) == true)
                    strReadCount = "?";

                // 文字图片
                using (MemoryStream image = ArtText.BuildArtText(
                    strReadCount,
                    "Microsoft YaHei",
                    (float)12,
                                FontStyle.Regular,
                Color.Black,
                Color.White,
                Color.Gray,
                ArtEffect.None,
                ImageFormat.Jpeg,
                100))
                {
                    Page.Response.ContentType = "image/jpeg";
                    Page.Response.AddHeader("Content-Length", image.Length.ToString());

                    Page.Response.AddHeader("Pragma", "no-cache");
                    Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
                    Page.Response.AddHeader("Expires", "0");

                    // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                    image.Seek(0, SeekOrigin.Begin);
                    StreamUtil.DumpStream(image, Page.Response.OutputStream/*, flushdelegate*/);

                }
                Page.Response.Flush();
                return 1;
            }
#endif

            string strLastModifyTime = (string)values["lastmodifytime"];
            // 2018/7/22
            if (string.IsNullOrEmpty(strLastModifyTime))
                strLastModifyTime = (string)values["lastmodified"];
            if (String.IsNullOrEmpty(strLastModifyTime) == false)
            {
                DateTime lastmodified = DateTime.Parse(strLastModifyTime).ToUniversalTime();
                string strIfHeader = Page.Request.Headers["If-Modified-Since"];

                if (String.IsNullOrEmpty(strIfHeader) == false)
                {
                    DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader); // .ToLocalTime();

                    if (DateTimeUtil.CompareHeaderTime(isModifiedSince, lastmodified) != 0)
                    {
                        // 修改过
                    }
                    else
                    {
                        // 没有修改过
                        Page.Response.StatusCode = 304;
                        Page.Response.SuppressContent = true;
                        return 0;
                    }
                }

                Page.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()
                /*
                                Page.Response.Cache.SetLastModified(lastmodified);
                                Page.Response.Cache.SetCacheability(HttpCacheability.Public);
                 * */
            }

            string strMime = (string)values["mimetype"];
            string strClientPath = (string)values["localpath"];
            if (strClientPath != "")
                strClientPath = PathUtil.PureName(strClientPath);

            // TODO: 如果是非image/????类型，都要加入content-disposition
            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            /*
            Page.Response.AddHeader("Accept-Ranges", "bytes");
            Page.Response.AddHeader("Last-Modified", "Wed, 21 Nov 2007 07:10:54 GMT");
             * */

            // 用 text/plain IE XML 搜索google
            // http://support.microsoft.com/kb/329661
            // http://support.microsoft.com/kb/239750/EN-US/
            /*
To use this fix, you must add the following registry value to the key listed below: 
Key: HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings

Value name: IsTextPlainHonored
Value type: DWORD
Value data: HEX 0x1 
             * */

            /*

            Page.Response.CacheControl = "no-cache";    // 如果不用此句，text/plain会被当作xml文件打开
            Page.Response.AddHeader("Pragma", "no-cache");
            Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
//            Page.Response.AddHeader("Cache-Control", "public");
            Page.Response.AddHeader("Expires", "0");
            Page.Response.AddHeader("Content-Transfer-Encoding", "binary");
             * */

            // 设置媒体类型
            //if (strMime == "text/plain")
            //    strMime = "text";
            Page.Response.ContentType = strMime;

            string strSize = (string)values["size"];
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Page.Response.AddHeader("Content-Length", strSize);
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            string strGetStyle = "content,data,incReadCount";
            if (StringUtil.IsInList("log", this.SearchLogEnable) == false)
                strGetStyle += ",skipLog";
            else
                strGetStyle += ",clientAddress:" + Page.Request.UserHostAddress;

            // 传输数据
            lRet = channel.GetRes(
                stop,
                strPath,
                Page.Response.OutputStream,
                strGetStyle,
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // Page.Response.ContentType = "text/plain";    // 可能因为 Page.Response.OutputStream 已经写入了部分内容，这时候设置 ContentType 会抛出异常
                strError = "GetRes() (for res) Error : " + strError;
                return -1;
            }
            return 1;
        }

        // 输出 计数数字 图像
        public static void OutputImage(Page Page,
            Color text_color,
            string strReadCount,
            int nFontSize = 8)
        {
            if (string.IsNullOrEmpty(strReadCount) == true)
                strReadCount = "0";

            // TODO: 探测本机存在那些字体，才使用

            // 文字图片
            using (MemoryStream image = ArtText.BuildArtText(
                strReadCount,
                "Consolas", // "Microsoft YaHei",
                (float)nFontSize,
                FontStyle.Bold,
            text_color, // Color.FromArgb(100, Color.Black),
            Color.Transparent,
            Color.Gray,
            ArtEffect.None,
            ImageFormat.Png,    // TODO: 可否用 jpeg 格式?
            200))
            {
                Page.Response.ContentType = "image/png";
                Page.Response.AddHeader("Content-Length", image.Length.ToString());

                Page.Response.AddHeader("Pragma", "no-cache");
                Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
                Page.Response.AddHeader("Expires", "0");

                // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                Page.Response.BufferOutput = false; // 2016/8/31

                image.Seek(0, SeekOrigin.Begin);
                StreamUtil.DumpStream(image, Page.Response.OutputStream/*, flushdelegate*/);
            }
            Page.Response.Flush();
        }

        public int CreateChatRoom(string strRoomName,
    out string strError)
        {
            int nRet = this.ChatRooms.CreateChatRoom(
    strRoomName,
    out strError);
            if (nRet == -1)
                return -1;
            this.Changed = true;    // 促使library.xml文件尽快保存
            return nRet;
        }

        public int DeleteChatRoom(string strRoomName,
out string strError)
        {
            int nRet = this.ChatRooms.DeleteChatRoom(
    strRoomName,
    out strError);
            if (nRet == -1)
                return -1;
            this.Changed = true;    // 促使library.xml文件尽快保存
            return nRet;
        }

        // 察看一个用户是否为聊天室栏目编辑身份
        // return:
        //      -1  出错
        //      0   不是编辑身份
        //      1   是编辑身份
        public int IsEditor(
    string strRoomName,
    string strUserID,
    out string strError)
        {
            return this.ChatRooms.IsEditor(
       strRoomName,
       strUserID,
       out strError);
        }

        // parameters:
        //      bDisplayAllIP   是否显示全部IP地址。如果为false，表示只显示访客的IP地址，并且是掩盖部分的
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        public int GetChatInfo(
            string strRights,
            string strName,
            string strDate,
            long lStart,
            int nMaxLines,
            bool bDisplayAllIP,
            out ChatInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            ChatRoom room = this.ChatRooms.GetChatRoom(strRights,
                strName);

            if (room == null)
            {
                strError = "栏目 '" + strName + "' 不存在";
                return -1;
            }

            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            return room.GetInfo(
                strDate,
                lStart,
                nMaxLines,
                bDisplayAllIP,
                out info,
                out strError);
        }

        // parameters:
        //      bChangeVersion  是否修改版本号
        //      bNotify     是否要在数据文件中创建一条通知条目
        //      lNewVersion 返回修改后的新版本号。如果bChangeVersion==false, 此参数也返回没有发生过变化的版本号
        public int DeleteChatItem(
            string strRights,
            string strRoomName,
            string strDate,
            string strRefID,
            bool bChangeVersion,
            bool bNotify,
            string strOperatorUserID,
            out long lNewVersion,
            out string strError)
        {
            strError = "";
            lNewVersion = 0;

            ChatRoom room = this.ChatRooms.GetChatRoom(
                strRights,
                strRoomName);
            if (room == null)
            {
                strError = "栏目 '" + strRoomName + "' 不存在";
                return -1;
            }

            // TODO: 要禁止删除一个notify事项
            // 先取出信息，判断是否为notify事项。如果是，则不让删除
            // 另外也顺便把取出的信息摘要后写入notify
            string strContent = "";

            // parameters:
            //      -1  出错
            //      0   没有找到
            //      1   已经删除
            int nRet = room.DeleteItem(strRefID,
                strDate,
                bChangeVersion,
                out lNewVersion,
                out strContent,
                out strError);
            if (nRet != 1)
                return nRet;

            /*
            if (string.IsNullOrEmpty(strContent) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strContent);
                }
                catch (Exception ex)
                {
                    strError = "记录内容装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }
            }
             * */


            if (bNotify == true)
            {
                room.AppendText(strRefID,
        "<div class='notify itemdeleted'>"
        + "<div class='refid'>" + strRefID + "</div>"
        + "<div class='operator'>" + HttpUtility.HtmlEncode(strOperatorUserID) + "</div>"
        + "<div class='opertime'>" + DateTime.Now.ToString("u") + "</div>"
        + "<div class='deletedrecord'>" + HttpUtility.HtmlEncode(strContent) + "</div>"
        + "</div>");
            }

            return 1;
        }

        public string GetPhysicalImageFilename(
            string strRights,
            string strRoomName,
            string strFileName)
        {
            ChatRoom room = this.ChatRooms.GetChatRoom(
                strRights,
                strRoomName);
            if (room == null)
                return null;

            return PathUtil.MergePath(room.DataDirectory, strFileName);
        }

        // 获得栏目的名字，验证栏目是否存在
        public string GetRoomName(
            string strRights,
            string strRoomName)
        {
            ChatRoom room = this.ChatRooms.GetChatRoom(
                strRights,
                strRoomName);
            if (room == null)
                return null;

            return room.Name;
        }

        // 获得栏目的名字，验证栏目是否存在
        // return:
        //      -2  权限不够
        //      -1  出错
        //      0   栏目不存在
        //      1   栏目找到
        public int GetRoomName(
            string strRights,
            string strRoomName,
            out string strResultRoomName)
        {
            strResultRoomName = "";
            ChatRoom room = this.ChatRooms.__GetChatRoom(strRoomName, true);
            if (room == null)
                return 0;
            // 是否属于组
            if (ChatRoomCollection.MatchGroup(room.GroupList, strRights) == false)
                return -2;

            strResultRoomName = room.Name;
            return 1;
        }

        // 获得栏目定义
        public ChatRoom GetChatRoom(
            string strRights,
            string strRoomName)
        {
            ChatRoom room = this.ChatRooms.GetChatRoom(
                strRights,
                strRoomName);
            if (room == null)
                return null;

            return room;
        }

        public int CreateChatItem(
            string strRights,
            string strRoomName,
            string strUserID,
            string strDisplayName,
            string strIpAddress,
            string strText,
            string strStyle,
            string strPostedFileName,
            string strPhotoUrl,
            out string strError)
        {
            strError = "";

            ChatRoom room = this.ChatRooms.GetChatRoom(
                strRights,
                strRoomName);
            if (room == null)
            {
                strError = "栏目 '" + strRoomName + "' 不存在";
                return -1;
            }

            // 写入附件
            string strFileName = "";
            if (string.IsNullOrEmpty(strPostedFileName) == false)
            {
                strFileName = room.PrepareAttachFileName(Path.GetExtension(strPostedFileName));
                File.Copy(strPostedFileName, strFileName, true);
            }

            if (string.IsNullOrEmpty(strStyle) == false)
                strStyle = "line " + strStyle;
            else
                strStyle = "line default";

            string strRefID = Guid.NewGuid().ToString();

            bool bGuest = StringUtil.HasHead(strUserID, "(访客)");

            // 如果有显示名，则userid要加密
            if (string.IsNullOrEmpty(strDisplayName) == false)
                strUserID = "encrypt:" + OpacApplication.EncryptPassword(strUserID);

            // 如果没有提供photourl，说明是本系统，这里负责创建URL
            if (string.IsNullOrEmpty(strPhotoUrl) == true
                && bGuest == false)
            {
                if (String.IsNullOrEmpty(strDisplayName) == true)
                    strPhotoUrl = "./getphoto.aspx?userid=" + HttpUtility.UrlEncode(strUserID);
                else
                    strPhotoUrl = "./getphoto.aspx?userid=" + HttpUtility.UrlEncode(strUserID) + "&displayName=" + HttpUtility.UrlEncode(strDisplayName);
            }

            room.AppendText(strRefID,
                "<div class='" + strStyle + "'>"
                + "<div class='userid'"
                + (string.IsNullOrEmpty(strDisplayName) == false ? " displayName='" + HttpUtility.HtmlEncode(strDisplayName) + "' " : "")
                + (string.IsNullOrEmpty(strPhotoUrl) == false ? " photo='" + HttpUtility.HtmlEncode(strPhotoUrl) + "' " : "")
                + ">" + HttpUtility.HtmlEncode(strUserID) + "</div>"
                + (string.IsNullOrEmpty(strIpAddress) == false ?
                "<div class='ip'>" + HttpUtility.HtmlEncode(strIpAddress) + "</div>" : "")
                + "<div class='time'>" + DateTime.Now.ToString("u") + "</div>"
                + "<div class='text'>" + HttpUtility.HtmlEncode(strText).Replace("\n", "<br/>") + "</div>"
                + (string.IsNullOrEmpty(strPostedFileName) == false ?
                  "<div class='image'>" + Path.GetFileName(strFileName) + "</div>" : "")
                + "<div class='refid'>" + strRefID + "</div>"
                + "</div>");
            return 0;
        }
    }

    // 系统挂起的理由
    public enum HangupReason
    {
        None = 0,   // 没有挂起
        LogRecover = 1, // 日志恢复
        Backup = 2, // 大备份
        Normal = 3, // 普通维护
        OperLogError = 4,   // 操作日志错误（例如日志空间满）
        Exit = 5,  // 系统正在退出
        Expire = 6, // 因长期没有升级版本，当前版本已经失效
    }

    public class WebPageStop : Stop
    {
        System.Web.UI.Page Page = null;

        public WebPageStop(System.Web.UI.Page page)
        {
            this.Page = page;
        }

        public override int State
        {
            get
            {
                if (this.Page == null)
                    return -1;

                if (this.Page.Response.IsClientConnected == false)
                    return 2;

                return 0;
            }
        }

    }

    public class LibraryInfo
    {
        public string LogoText = "";
        public string LibraryCode = "";
        public string StyleName = "";
    }
}
