// #define OPTIMIZE_API
#define LOG_INFO

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Messaging;
using System.Security.Principal;
using System.Reflection;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;

using Newtonsoft.Json.Linq;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Core;
using DigitalPlatform.Marc;
using System.Text.RegularExpressions;

namespace DigitalPlatform.LibraryServer
{
    public class StopState
    {
        public bool Stopped = false;

        public void Stop()
        {
            this.Stopped = true;
        }
    }

    /// <summary>
    /// 存放应用程序全局信息的类
    /// 建议用partial class改写为几个文件，减小每个文件的尺寸
    /// </summary>
    public partial class LibraryApplication : IDisposable
    {
        // public static string Version = "3.4";
        public static string Version
        {
            get
            {
                Assembly assembly = Assembly.GetAssembly(typeof(LibraryApplication));
                Version version = assembly.GetName().Version;
                return version.Major + "." + version.Minor;
            }
        }

        public static string FullVersion
        {
            get
            {
                Assembly assembly = Assembly.GetAssembly(typeof(LibraryApplication));
                Version version = assembly.GetName().Version;
                return version.ToString();
            }
        }

        public DailyItemCountTable DailyItemCountTable = new DailyItemCountTable();

        internal static DateTime _expire = new DateTime(2021, 9, 15); // 上一个版本是 2021/7/15 2021/3/15 2020/11/15 2020/7/15 2019/2/15 2019/10/15 2019/7/15 2019/5/15 2019/2/15 2018/11/15 2018/9/15 2018/7/15 2018/5/15 2018/3/15 2017/1/15 2017/12/1 2017/9/1 2017/6/1 2017/3/1 2016/11/1

#if NO
        int m_nRefCount = 0;
        public int AddRef()
        {
            int v = m_nRefCount;
            m_nRefCount++;

            return v;
        }

        public int GetRef()
        {
            return m_nRefCount;
        }

        public int ReleaseRef()
        {
            m_nRefCount--;

            return m_nRefCount;
        }
#endif
        public const string qrkey = "dpqrhello";

        // (登录用的手机短信)验证码存储数组
        public TempCodeCollection TempCodeTable = new TempCodeCollection();

        /// <summary>
        /// 是否为评估状态
        /// </summary>
        public bool TestMode = false;

        /// <summary>
        /// 在登录阶段是否强制检查前端的版本号？(对几个特殊的代理账户不做此项检查)
        /// </summary>
        public bool CheckClientVersion = false;

        // 负责存储统计日志的 UID 的 Hashtable。用途是防止重复写入 UID 相同的日志记录
        // uid --> true
        public UidTable StatisLogUidTable = new UidTable();

        /// <summary>
        /// 在登录阶段要给所有账户都添加的权限列表。用逗号分隔的字符串
        /// </summary>
        public string GlobalAddRights { get; set; }

        string _outgoingQueue = "";

        /// <summary>
        /// dp2library 用于输出消息的队列路径。
        /// </summary>
        public string OutgoingQueue
        {
            get
            {
                return this._outgoingQueue;
            }
            set
            {
                this._outgoingQueue = value;
                this.MsmqInitialized = false;   // 迫使重新执行 InitialMsmq()
            }
        }

        // 存储各种参数信息
        // 为C#脚本所准备
        public Hashtable ParamTable = new Hashtable();

        // 防止试探密码攻击的设施
        public UserNameTable UserNameTable = new UserNameTable("dp2library");

        // Session集合
        public SessionTable SessionTable = new SessionTable();

        /// <summary>
        /// 最多允许访问 dp2Library 的前端机器数量
        /// </summary>
        public int MaxClients
        {
            get
            {
                return this.SessionTable.MaxClients;
            }
            set
            {
                this.SessionTable.MaxClients = value;
            }
        }

        /// <summary>
        /// 许可类型
        /// "server" 表示服务器验证服务器自己的序列号，就不要求前端验证前端自己的序列号了
        /// </summary>
        public string LicenseType
        {
            get;
            set;
        }

        /// <summary>
        /// 许可的功能列表。patronReplication 表示读者同步功能
        /// </summary>
        public string Function
        {
            get;
            set;
        }

        static string GetFunctionDescription(string strFunction)
        {
            if (string.IsNullOrEmpty(strFunction) == false
    && strFunction.IndexOf("|") != -1)
                throw new ArgumentException("strFunction 参数值中不允许包含字符 '|'。应改用逗号");

            List<string> results = new List<string>();
            if (StringUtil.IsInList("objectRights", strFunction) == true)
                results.Add("+下载权限");
            else
                results.Add("-下载权限");

            if (StringUtil.IsInList("pdfPreview", strFunction) == true)
                results.Add("+PDF预览");
            else
                results.Add("-PDF预览");

            return StringUtil.MakePathList(results, ", ");
        }

        public string LibraryName
        {
            get
            {
                if (this.LibraryCfgDom == null || this.LibraryCfgDom.DocumentElement == null)
                    return "";
                XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("libraryInfo/libraryName");
                if (node == null)
                    return "";
                return node.InnerText.Trim();
            }
        }

        /// <summary>
        /// 失效的前端 MAC 地址集合
        /// Key 为 MAC 地址。大写。如果 Key 在 Hashtable 中已经存在，则表示这个 MAC 地址已经失效了
        /// </summary>
        public Hashtable ExpireMacTable = new Hashtable();

        /// <summary>
        /// 跟踪读者入馆状态的类
        /// </summary>
        public Garden Garden = new Garden();

        public IssueItemDatabase IssueItemDatabase = null;
        public OrderItemDatabase OrderItemDatabase = null;
        public CommentItemDatabase CommentItemDatabase = null;

        public HitCountDatabase HitCountDatabase = new HitCountDatabase();
        public AccessLogDatabase AccessLogDatabase = new AccessLogDatabase();
        public ChargingOperDatabase ChargingOperDatabase = new ChargingOperDatabase();

        public Semaphore PictureLimit = new Semaphore(10, 10);

        // public HangupReason HangupReason = HangupReason.None;
        public List<string> HangupList = new List<string>();    // 具体的挂起原因。因为初始化和各种环节，可能会有不止一种令系统挂起的原因。如果在重试某些操作以后希望解除挂起状态，则需要检查是否消除了全部因素，才能决定是否解除

        public List<string> SystemMessages = new List<string>();    // 系统消息。希望前端看到的报错或者通知消息

        public bool PauseBatchTask = false; // 是否暂停后台任务

        public string DataDir = "";

        public string HostDir = "";

        public string GlobalErrorInfo = ""; // 存放全局出错信息。两级报错机制：当这里有值的时候，优先认这里的；否则，再看Application["errorinfo"]字符串
        const string EncryptKey = "dp2circulationpassword";
        // http://localhost/dp2bbs/passwordutil.aspx

        string m_strFileName = "";  // library.xml配置文件全路径

        // string m_strWebuiFileName = ""; // webui.xml配置文件全路径

        public string BinDir = "";	// bin目录

        public string CfgDir = "";  // cfg目录

        public string CfgMapDir = "";  // cfgmap目录

        public string LogDir = "";	// 事件日志目录

        // public string OperLogDir = "";  // 操作日志目录

        public string ZhengyuanDir = "";    // 正元一卡通数据目录
        public string DkywDir = "";    // 迪科远望一卡通数据目录
        public string PatronReplicationDir = "";    // 通用 读者信息同步 目录

        public string StatisDir = "";   // 统计文件存放目录

        public string SessionDir = "";  // session临时文件

        public string TempDir = "";  // 各种通用临时文件 2014/12/5

        public string BackupDir = "";   // 大备份目录 2017/7/13

        public string WsUrl = "";	// dp2rms WebService URL

        public string ManagerUserName = "";
        public string ManagerPassword = "";

        public bool DebugMode = false;
        public string UID = "";

        // 预约到书队列库的检索途径信息 2015/5/7
        public BiblioDbFromInfo[] ArrivedDbFroms = null;

        public string ArrivedDbName = "";   // 预约到书队列数据库名
        public string ArrivedReserveTimeSpan = "";  // 通知到书后的保留时间。含时间单位
        public int OutofReservationThreshold = 10;  // 预约到书多少不取次后，被惩罚禁止预约
        public bool CanReserveOnshelf = true;   // 是否可以预约在架图书
        public string NotifyDef = "";       // 提醒通知的定义。"15day,50%,70%"
        public string ArrivedNotifyTypes = "dpmail,email";   // 到书通知的类型

        DefaultThread defaultManagerThread = null; // 缺省管理后台任务

        public OperLogThread operLogThread = null; // 操作日志辅助线程

        // 全部读者库集合(包括不参与流通的读者库)
        public List<ReaderDbCfg> ReaderDbs = new List<ReaderDbCfg>();

        public List<ItemDbCfg> ItemDbs = null;

        // Application通用锁。可以用来管理GlobalCfgDom等
        public ReaderWriterLock m_lock = new ReaderWriterLock();
        //public ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        public static int m_nLockTimeout = 10 * 1000;	// 5000=5秒 2021/8/3 从 5 秒改为 10 秒

        // 读者记录锁。避免多线程改写同一读者记录造成的故障
        public RecordLockCollection ReaderLocks = new RecordLockCollection();

        private XmlDocument _libraryCfgDom = null;   // library.xml配置文件内容
        internal ReaderWriterLockSlim _lockLibraryCfgDom = new ReaderWriterLockSlim();

        public XmlDocument LibraryCfgDom
        {
            get
            {
                _lockLibraryCfgDom.EnterReadLock();
                try
                {
                    return _libraryCfgDom;
                }
                finally
                {
                    _lockLibraryCfgDom.ExitReadLock();
                }
            }
            set
            {
                _lockLibraryCfgDom.EnterWriteLock();
                try
                {
                    _libraryCfgDom = value;
                }
                finally
                {
                    _lockLibraryCfgDom.ExitWriteLock();
                }
            }
        }

        public Clock Clock = new Clock();

        bool m_bChanged = false;

        FileSystemWatcher watcher = null;

        public CfgsMap CfgsMap = null;

        public BatchTaskCollection BatchTasks = new BatchTaskCollection();

        public MessageCenter MessageCenter = null;
        // public string MessageDbName = "";
        string m_strMessageDbName = "";
        public string MessageDbName
        {
            get
            {
                return m_strMessageDbName;
            }
            set
            {
                m_strMessageDbName = value;
                if (this.MessageCenter != null)
                    this.MessageCenter.MessageDbName = value;
            }
        }


        public string MessageReserveTimeSpan = "365day";  // 消息在信箱中的保留期限。含时间单位。缺省为一年

        public string OpacServerUrl = "";

        // 将来会废止这个变量
        public string LibraryServerUrl
        {
            get
            {
                return this.OpacServerUrl;
            }
        }

        public AccountTable AccountTable = new AccountTable();

        public VirtualDatabaseCollection vdbs = null;

        public OperLog OperLog = new OperLog();

        public long m_lSeed = 0;

        public string InvoiceDbName = "";   // 发票库名 2012/11/6

        public string AmerceDbName = "";    // 违约金库名

        public string OverdueStyle = "";    // 超期罚款计算办法 <amerce overdueStyle="..." />

        public KernelDbInfoCollection kdbs = null;

        // 实体记录锁。避免多线程改写同一实体记录, 并且锁定条码号查重过程
        public RecordLockCollection EntityLocks = new RecordLockCollection();

        // 书目记录锁。避免多线程改写同一书目记录及其下属实体记录
        public RecordLockCollection BiblioLocks = new RecordLockCollection();

        // 本地结果集锁。避免多线程改写同一结果集
        // public RecordLockCollection ResultsetLocks = new RecordLockCollection();

        public Hashtable StopTable = new Hashtable();

        // 等待处理的缓存文件
        public List<String> PendingCacheFiles = new List<string>();

        // public CacheBuilder CacheBuilder = null;

        public int SearchMaxResultCount = 5000;

        public Statis Statis = null;

        // public XmlDocument WebUiDom = null;   // webui.xml配置文件内容

        public bool PassgateWriteToOperLog = true;

        // GetRes() API 获取对象的动作是否写入操作日志
        public bool GetObjectWriteToOperLog = false;

        // 访问日志每天允许创建的最多条目数
        public int AccessLogMaxCountPerDay = 10000;

        // 2018/5/12
        // 用于出纳操作的辅助性的检索途径
        // 不要试图在运行中途修改它。它不会回写到 library.xml 中
        public List<string> ItemAdditionalFroms = new List<string>();


        // 2013/5/24
        // 用于出纳操作的辅助性的检索途径
        // 不要试图在运行中途修改它。它不会回写到 library.xml 中
        public List<string> PatronAdditionalFroms = new List<string>();

        // 2015/9/17
        // 读者记录中的扩展字段
        // 不要试图在运行中途修改它。它不会回写到 library.xml 中
        public List<string> PatronAdditionalFields = new List<string>();

        // 2015/10/16
        // 用于读者同步的字段列表。如果第一个元素为 空，表示替换原有列表；否则是追加补充字段定义的意思
        public List<string> PatronReplicationFields = new List<string>();

        // 2020/9/7
        // 册记录中的扩展字段
        // 不要试图在运行中途修改它。它不会回写到 library.xml 中
        public List<string> ItemAdditionalFields = new List<string>();

        // 2021/8/18
        // 针对读者记录字段进行马赛克的方法定义
        public string PatronMaskDefinition = null;

        // 缺省的 读者记录马赛克方法
        public static string DefaultPatronMaskDefinition = "name:1|0,tel:3|3,*:2|0";

        // 构造函数
        public LibraryApplication()
        {
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~LibraryApplication()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.

                    // 这里有一点问题：可能析构函数调不了Close()
                    // this.Close();
                }

                this.Close();   // 2007/6/8 移动到这里的

                _physicalFileCache?.Dispose();

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;            
                */
            }
            disposed = true;
        }

        // TODO: 是否需要锁定?
        // TODO: 同时要在错误日志里面写入一笔，以便系统管理员了解情况
        public void AddHangup(string strText)
        {
            if (ContainsHangup(strText) == true)
                return;
            this.HangupList.Add(strText);

            // 2017/11/29
            if (strText != "Exit")
                this.WriteErrorLog("*** 当前系统已经被挂起 " + strText);
        }

        public bool ContainsHangup(string strText)
        {
            return this.HangupList.IndexOf(strText) != -1;
        }

        public void ClearHangup(string strText)
        {
            for (int i = 0; i < this.HangupList.Count; i++)
            {
                if (this.HangupList[i] == strText)
                {
                    this.HangupList.RemoveAt(i);
                    i--;
                }
            }

            this.WriteErrorLog("*** 系统已解除 " + strText + " 挂起状态");
        }

        public int LoadCfg(
            bool bReload,
            string strDataDir,
            string strHostDir,  // 为了脚本编译时候获得dll目录
            out string strError)
        {
            strError = "";
            int nRet = 0;
            LibraryApplication app = this;  // new CirculationApplication();

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                string strLogDir = PathUtil.MergePath(strDataDir, "log");
                // log
                app.LogDir = strLogDir; // 日志存储目录
                PathUtil.TryCreateDir(app.LogDir);  // 确保目录创建

                this.WriteErrorLog("*********");
                this.WriteErrorLog($"LoadCfg() Begin. bReload={bReload}");

                // 装载配置文件的过程，只能消除以前的 StartError 挂起状态，其他状态是无法消除的
                // 本函数过程也约定好，只进行 StartError 挂起，不做其他挂起
#if NO
            if (app.HangupReason == LibraryServer.HangupReason.StartingError)
                app.HangupReason = LibraryServer.HangupReason.None;
#endif
                if (app.HangupList.Count > 0)
                {
                    ClearHangup("StartingError");
                }
                try
                {
                    DateTime start = DateTime.Now;

                    this.DataDir = strDataDir;
                    this.HostDir = strHostDir;

                    string strFileName = PathUtil.MergePath(strDataDir, "library.xml");
                    string strBinDir = strHostDir;  //  PathUtil.MergePath(strHostDir, "bin");
                    string strCfgDir = PathUtil.MergePath(strDataDir, "cfgs");
                    string strCfgMapDir = PathUtil.MergePath(strDataDir, "cfgsmap");
                    string strOperLogDir = PathUtil.MergePath(strDataDir, "operlog");
                    string strZhengyuanDir = PathUtil.MergePath(strDataDir, "zhengyuan");
                    string strDkywDir = PathUtil.MergePath(strDataDir, "dkyw");
                    string strPatronReplicationDir = PathUtil.MergePath(strDataDir, "patronreplication");
                    string strStatisDir = PathUtil.MergePath(strDataDir, "statis");
                    string strSessionDir = PathUtil.MergePath(strDataDir, "session");
                    string strColumnDir = PathUtil.MergePath(strDataDir, "column");
                    string strTempDir = PathUtil.MergePath(strDataDir, "temp");
                    string strBackupDir = Path.Combine(strDataDir, "backup");

                    app.m_strFileName = strFileName;

                    app.CfgDir = strCfgDir;

                    app.CfgMapDir = strCfgMapDir;
                    PathUtil.TryCreateDir(app.CfgMapDir);	// 确保目录创建

                    // zhengyuan 一卡通
                    app.ZhengyuanDir = strZhengyuanDir;
                    PathUtil.TryCreateDir(app.ZhengyuanDir);	// 确保目录创建

                    // dkyw 一卡通
                    app.DkywDir = strDkywDir;
                    PathUtil.TryCreateDir(app.DkywDir);	// 确保目录创建

                    // patron replication
                    app.PatronReplicationDir = strPatronReplicationDir;
                    PathUtil.TryCreateDir(app.PatronReplicationDir);	// 确保目录创建

                    // statis 统计文件
                    app.StatisDir = strStatisDir;
                    PathUtil.TryCreateDir(app.StatisDir);	// 确保目录创建

                    // session临时文件
                    app.SessionDir = strSessionDir;
                    PathUtil.TryCreateDir(app.SessionDir);	// 确保目录创建

                    if (bReload == false)
                        CleanSessionDir(this.SessionDir);

                    // 各种临时文件
                    app.TempDir = strTempDir;
                    PathUtil.TryCreateDir(app.TempDir);

                    // 大备份目录
                    app.BackupDir = strBackupDir;
                    PathUtil.TryCreateDir(app.BackupDir);

                    if (bReload == false)
                    {

#if LOG_INFO
                        app.WriteErrorLog($"INFO: 清除临时文件目录 {app.TempDir}");
#endif
                        try
                        {
                            PathUtil.ClearDir(app.TempDir);
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog("清除临时文件目录 " + app.TempDir + " 时出现异常: " + ExceptionUtil.GetDebugText(ex));
                        }
                    }

                    this.InitialLoginCache();
                    // this.InitialBiblioSummaryCache();

                    if (bReload == false)
                    {
                        if (app.HasAppBeenKilled() == true)
                        {
                            app.WriteErrorLog("*** 发现 LibraryService 先前曾被意外终止 ***");
                        }
                    }

                    this.WriteErrorLog("序列号许可的功能: '" + this.Function + "' (" + GetFunctionDescription(this.Function) + ")");

                    if (bReload == true)
                        app.WriteErrorLog("library (" + FullVersion + ") application 开始重新装载 " + this.m_strFileName);
                    else
                        app.WriteErrorLog("library (" + FullVersion + ") application 开始初始化。");

#if LOG_INFO
                    app.WriteErrorLog("INFO: 开始装载 " + strFileName + " 到 XMLDOM");
#endif


                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.Load(strFileName);
                    }
                    catch (FileNotFoundException)
                    {
                        strError = "file '" + strFileName + "' not found ...";
                        goto ERROR1;
                    }
                    catch (Exception ex)
                    {
                        strError = "装载配置文件-- '" + strFileName + "' 时发生异常：" + ExceptionUtil.GetDebugText(ex);
                        app.WriteErrorLog(strError);
                        // throw ex;
                        goto ERROR1;
                    }

                    // TODO: 此处可能会有并发问题。需要锁定一段时间，让使用 LibraryCfgDom 的地方暂时等待

                    app.LibraryCfgDom = dom;

#if LOG_INFO
                    app.WriteErrorLog("INFO: 初始化内存参数");
#endif

                    // *** 进入内存的参数开始
                    // 注意修改了这些参数的结构后，必须相应修改Save()函数的相关片断

                    // 2011/1/7
                    bool bValue = false;
                    DomUtil.GetBooleanParam(dom.DocumentElement,
                        "debugMode",
                        false,
                        out bValue,
                        out strError);
                    this.DebugMode = bValue;
                    WriteErrorLog("是否为调试态: " + this.DebugMode);

                    // 2013/4/10 
                    // uid
                    this.UID = dom.DocumentElement.GetAttribute("uid");
                    if (string.IsNullOrEmpty(this.UID) == true)
                    {
                        this.UID = Guid.NewGuid().ToString();
                        this.Changed = true;
                        WriteErrorLog("自动为 library.xml 添加 uid '" + this.UID + "'");
                    }

                    // 内核参数
                    // 元素<rmsserver>
                    // 属性url/username/password
                    XmlElement node = dom.DocumentElement.SelectSingleNode("rmsserver") as XmlElement;
                    if (node != null)
                    {
                        app.WsUrl = DomUtil.GetAttr(node, "url");

                        if (app.WsUrl.IndexOf(".asmx") != -1)
                        {
                            strError = "装载配置文件 '" + strFileName + "' 过程中发生错误: <rmsserver>元素url属性中的 dp2内核 服务器URL '" + app.WsUrl + "' 不正确，应当为非.asmx形态的地址...";
                            app.WriteErrorLog(strError);
                            goto ERROR1;
                        }

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
                            strError = "<rmsserver>元素password属性中的密码设置不正确";
                            // throw new Exception();
                            goto ERROR1;
                        }

                        CfgsMap = new CfgsMap(this.CfgMapDir/*,
                        this.WsUrl*/);
                        CfgsMap.Clear();
                    }
                    else
                    {
                        app.WsUrl = "";
                        app.ManagerUserName = "";
                        app.ManagerPassword = "";
                    }

                    // 元素 <mongoDB>
                    // 属性 connectionString / instancePrefix
                    node = dom.DocumentElement.SelectSingleNode("mongoDB") as XmlElement;
                    if (node != null)
                    {
                        this.MongoDbConnStr = DomUtil.GetAttr(node, "connectionString");
                        this.MongoDbInstancePrefix = node.GetAttribute("instancePrefix");
                    }
                    else
                    {
                        this.MongoDbConnStr = "";
                        this.MongoDbInstancePrefix = "";
                        this.AccessLogDatabase = new AccessLogDatabase();
                        this.HitCountDatabase = new HitCountDatabase();
                        this.ChargingOperDatabase = new ChargingOperDatabase();
                    }

                    // 预约到书
                    // 元素<arrived>
                    // 属性dbname/reserveTimeSpan/outofReservationThreshold/canReserveOnshelf/notifyTypes
                    node = dom.DocumentElement.SelectSingleNode("arrived") as XmlElement;
                    if (node != null)
                    {
                        app.ArrivedDbName = DomUtil.GetAttr(node, "dbname");
                        app.ArrivedReserveTimeSpan = DomUtil.GetAttr(node, "reserveTimeSpan");

                        int nValue = 0;
                        nRet = DomUtil.GetIntegerParam(node,
                            "outofReservationThreshold",
                            10,
                            out nValue,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("元素<arrived>属性outofReservationThreshold读入时发生错误: " + strError);
                            goto ERROR1;
                        }

                        app.OutofReservationThreshold = nValue;

                        bValue = false;
                        nRet = DomUtil.GetBooleanParam(node,
                            "canReserveOnshelf",
                            true,
                            out bValue,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("元素<arrived>属性canReserveOnshelf读入时发生错误: " + strError);
                            goto ERROR1;
                        }

                        app.CanReserveOnshelf = bValue;

                        // 没有这个属性的时候，默认 "dpmail,email"，否则依其值，哪怕为 ""
                        if (node.GetAttributeNode("notifyTypes") == null)
                            app.ArrivedNotifyTypes = "dpmail,email";
                        else
                            app.ArrivedNotifyTypes = node.GetAttribute("notifyTypes");

                    }
                    else
                    {
                        app.ArrivedDbName = "";
                        app.ArrivedReserveTimeSpan = "";
                        app.OutofReservationThreshold = 10;
                        app.CanReserveOnshelf = true;
                        app.ArrivedNotifyTypes = "dpmail,email";
                    }

                    // 2021/6/29
                    // accounts/@passwordExpireLength 属性
                    // accounts/@passwordStyle 属性
                    _passwordExpirePeriod = TimeSpan.MaxValue;
                    _passwordStyle = "";
                    node = dom.DocumentElement.SelectSingleNode("accounts") as XmlElement;
                    if (node != null)
                    {
                        string passwordExpireLength = node.GetAttribute("passwordExpireLength");
                        try
                        {
                            if (string.IsNullOrEmpty(passwordExpireLength) == false)
                                _passwordExpirePeriod = ParseTimeSpan(passwordExpireLength);
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog($"library.xml 中 accounts/@passwordExpireLength 属性值 '{passwordExpireLength}' 格式不合法: {ex.Message}");
                        }

                        _passwordStyle = node.GetAttribute("passwordStyle");
                    }

                    // 2021/7/4
                    // 根据 account/@passwordExpireLength 参数，重建或者清除 account 密码失效期
                    if (bReload == false)
                        CreateOrDeletePasswordExpire(dom);

                    // 2013/9/24
                    // 借期提醒通知定义
                    // 元素 <monitors/readersMonitor>
                    // 属性 notifyDef
                    node = dom.DocumentElement.SelectSingleNode("monitors/readersMonitor") as XmlElement;
                    if (node != null)
                    {
                        // 提醒通知的定义
                        app.NotifyDef = DomUtil.GetAttr(node, "notifyDef");
                    }
                    else
                    {
                        app.NotifyDef = "";
                    }

                    // <login>
                    // login/@checkClientVersion
                    // login/@globalAddRights
                    // login/@patronPasswordExpireLength 属性
                    // login/@patronPasswordStyle 属性
                    // login/@tempPasswordExpireLength 属性
                    _patronPasswordExpirePeriod = TimeSpan.MaxValue;
                    _patronPasswordStyle = "";
                    _tempPasswordExpirePeriod = new TimeSpan(1, 0, 0); // 一小时
                    node = dom.DocumentElement.SelectSingleNode("login") as XmlElement;
                    if (node != null)
                    {
                        this.CheckClientVersion = DomUtil.GetBooleanParam(node,
                            "checkClientVersion",
                            false);
                        // 2017/10/13
                        this.GlobalAddRights = node.GetAttribute("globalAddRights");

                        // 2021/7/4
                        string patronPasswordExpireLength = node.GetAttribute("patronPasswordExpireLength");
                        try
                        {
                            if (string.IsNullOrEmpty(patronPasswordExpireLength) == false)
                            {
                                _patronPasswordExpirePeriod = ParseTimeSpan(patronPasswordExpireLength);
                            }
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog($"library.xml 中 login/@patronPasswordExpireLength 属性值 '{patronPasswordExpireLength}' 格式不合法: {ex.Message}");
                        }

                        _patronPasswordStyle = node.GetAttribute("patronPasswordStyle");

                        string tempPasswordExpireLength = node.GetAttribute("tempPasswordExpireLength");
                        try
                        {
                            if (string.IsNullOrEmpty(tempPasswordExpireLength) == false)
                            {
                                _tempPasswordExpirePeriod = ParseTimeSpan(tempPasswordExpireLength);
                            }
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog($"library.xml 中 login/@tempPasswordExpireLength 属性值 '{patronPasswordExpireLength}' 格式不合法: {ex.Message}");
                        }
                    }
                    else
                    {
                        this.CheckClientVersion = false;
                        this.GlobalAddRights = "";
                    }

                    // <circulation>
                    node = dom.DocumentElement.SelectSingleNode("circulation") as XmlElement;
                    if (node != null)
                    {
                        {
                            string strList = DomUtil.GetAttr(node, "patronAdditionalFroms");
                            if (string.IsNullOrEmpty(strList) == false)
                                this.PatronAdditionalFroms = StringUtil.SplitList(strList);
                            else
                                this.PatronAdditionalFroms = new List<string>();
                        }

                        {
                            string strList = DomUtil.GetAttr(node, "patronAdditionalFields");
                            if (string.IsNullOrEmpty(strList) == false)
                                this.PatronAdditionalFields = StringUtil.SplitList(strList);
                            else
                                this.PatronAdditionalFields = new List<string>();
                        }

                        {
                            string strList = DomUtil.GetAttr(node, "patronReplicationFields");
                            if (string.IsNullOrEmpty(strList) == false)
                                this.PatronReplicationFields = StringUtil.SplitList(strList);
                            else
                                this.PatronReplicationFields = StringUtil.SplitList(strList);
                        }

                        nRet = DomUtil.GetIntegerParam(node,
                            "maxPatronHistoryItems",
                            10, // 100,
                            out int v,
                            out strError);
                        if (nRet == -1)
                            app.WriteErrorLog(strError);
                        this.MaxPatronHistoryItems = v;

                        // 2018/5/12
                        {
                            string strList = DomUtil.GetAttr(node, "itemAdditionalFroms");
                            if (string.IsNullOrEmpty(strList) == false)
                                this.ItemAdditionalFroms = StringUtil.SplitList(strList);
                            else
                                this.ItemAdditionalFroms = new List<string>();
                        }

                        // 2020/9/7
                        {
                            string strList = DomUtil.GetAttr(node, "itemAdditionalFields");
                            if (string.IsNullOrEmpty(strList) == false)
                                this.ItemAdditionalFields = StringUtil.SplitList(strList);
                            else
                                this.ItemAdditionalFields = new List<string>();
                        }

                        nRet = DomUtil.GetIntegerParam(node,
        "maxItemHistoryItems",
        10, // 100,
        out v,
        out strError);
                        if (nRet == -1)
                            app.WriteErrorLog(strError);
                        this.MaxItemHistoryItems = v;

                        this.VerifyBarcode = DomUtil.GetBooleanParam(node, "verifyBarcode", false);

                        this.AcceptBlankItemBarcode = DomUtil.GetBooleanParam(node, "acceptBlankItemBarcode", true);

                        this.AcceptBlankReaderBarcode = DomUtil.GetBooleanParam(node, "acceptBlankReaderBarcode", true);

                        // 2017/5/4
                        this.UpperCaseItemBarcode = DomUtil.GetBooleanParam(node, "upperCaseItemBarcode", true);
                        this.UpperCaseReaderBarcode = DomUtil.GetBooleanParam(node, "upperCaseReaderBarcode", true);

                        this.VerifyBookType = DomUtil.GetBooleanParam(node, "verifyBookType", false);
                        this.VerifyReaderType = DomUtil.GetBooleanParam(node, "verifyReaderType", false);
                        this.BorrowCheckOverdue = DomUtil.GetBooleanParam(node, "borrowCheckOverdue", true);

                        this.CirculationNotifyTypes = node.GetAttribute("notifyTypes");

                        this.AcceptBlankRoomName = DomUtil.GetBooleanParam(node, "acceptBlankRoomName", false);

                        this.VerifyRegisterNoDup = DomUtil.GetBooleanParam(node, "verifyRegisterNoDup", true);

                        if (node.HasAttribute("patronMaskDefinition"))
                            this.PatronMaskDefinition = node.GetAttribute("patronMaskDefinition");
                        else
                            this.PatronMaskDefinition = DefaultPatronMaskDefinition;
                    }
                    else
                    {
                        this.PatronAdditionalFroms = new List<string>();
                        this.PatronAdditionalFields = new List<string>();
                        this.MaxPatronHistoryItems = DEFAULT_MAXPATRONHITSTORYITEMS;
                        this.MaxItemHistoryItems = DEFAULT_MAXITEMHISTORYITEMS;
                        this.VerifyBarcode = false;
                        this.AcceptBlankItemBarcode = true;
                        this.AcceptBlankReaderBarcode = true;

                        // 2017/5/4
                        this.UpperCaseItemBarcode = true;
                        this.UpperCaseReaderBarcode = true;

                        this.VerifyBookType = false;
                        this.VerifyReaderType = false;
                        this.BorrowCheckOverdue = true;
                        this.CirculationNotifyTypes = "";
                        this.AcceptBlankRoomName = false;

                        this.VerifyRegisterNoDup = true;

                        this.ItemAdditionalFroms = new List<string>();
                        this.ItemAdditionalFields = new List<string>();

                        this.PatronMaskDefinition = DefaultPatronMaskDefinition;
                    }

                    // <channel>
                    node = dom.DocumentElement.SelectSingleNode("channel") as XmlElement;
                    if (node != null)
                    {
                        nRet = DomUtil.GetIntegerParam(node,
                            "maxChannelsPerIP",
                            50,
                            out int v,
                            out strError);
                        if (nRet == -1)
                            app.WriteErrorLog(strError);
                        if (this.SessionTable != null)
                            this.SessionTable.MaxSessionsPerIp = v;

                        nRet = DomUtil.GetIntegerParam(node,
        "maxChannelsLocalhost",
        150,
        out v,
        out strError);
                        if (nRet == -1)
                            app.WriteErrorLog(strError);
                        if (this.SessionTable != null)
                            this.SessionTable.MaxSessionsLocalHost = v;

                        string strList = DomUtil.GetStringParam(node,
                            "privilegedIpList",
                            "");
                        this.SessionTable.SpecialIpList = StringUtil.SplitList(strList, ',');
                    }
                    else
                    {
                        if (this.SessionTable != null)
                        {
                            this.SessionTable.MaxSessionsPerIp = 50;
                            this.SessionTable.MaxSessionsLocalHost = 150;
                            this.SessionTable.SpecialIpList = null;
                        }
                    }

                    // <cataloging>
                    node = dom.DocumentElement.SelectSingleNode("cataloging") as XmlElement;
                    if (node != null)
                    {
                        // 是否允许删除带有下级记录的书目记录
                        bValue = true;
                        nRet = DomUtil.GetBooleanParam(node,
                            "deleteBiblioSubRecords",
                            true,
                            out bValue,
                            out strError);
                        if (nRet == -1)
                            app.WriteErrorLog(strError);
                        this.DeleteBiblioSubRecords = bValue;

                        // 2019/4/30
                        var value = node.GetAttribute("biblioSearchMaxCount");
                        if (string.IsNullOrEmpty(value))
                            this.BiblioSearchMaxCount = -1;
                        else
                        {
                            if (Int64.TryParse(value, out Int64 nValue) == false)
                            {
                                app.WriteErrorLog($"cataloging/@biblioSearchMaxCount 值'{value}' 格式错误。应为一个整数");
                                this.BiblioSearchMaxCount = -1;
                            }
                            this.BiblioSearchMaxCount = nValue;
                        }
                    }
                    else
                    {
                        this.DeleteBiblioSubRecords = true;
                        this.BiblioSearchMaxCount = -1;
                    }

                    // 入馆登记
                    // 元素<passgate>
                    // 属性writeOperLog
                    node = dom.DocumentElement.SelectSingleNode("passgate") as XmlElement;
                    if (node != null)
                    {
                        string strWriteOperLog = DomUtil.GetAttr(node, "writeOperLog");

                        this.PassgateWriteToOperLog = ToBoolean(strWriteOperLog,
                            true);
                    }
                    else
                    {
                        this.PassgateWriteToOperLog = true;
                    }

                    // 对象管理
                    // 元素<object>
                    // 属性 writeOperLog
                    node = dom.DocumentElement.SelectSingleNode("object") as XmlElement;
                    if (node != null)
                    {
                        string strWriteOperLog = DomUtil.GetAttr(node, "writeGetResOperLog");

                        this.GetObjectWriteToOperLog = ToBoolean(strWriteOperLog,
                            false);
                    }
                    else
                    {
                        this.GetObjectWriteToOperLog = false;
                    }

                    // 2015/11/26
                    // 日志特性
                    // 元素<log>
                    node = dom.DocumentElement.SelectSingleNode("log") as XmlElement;
                    if (node != null)
                    {
                        DomUtil.GetIntegerParam(node,
                            "accessLogMaxCountPerDay",
                            10000,
                            out int nValue,
                            out strError);
                        this.AccessLogMaxCountPerDay = nValue;
                    }
                    else
                    {
                        this.AccessLogMaxCountPerDay = 10000;
                    }

                    // 消息
                    // 元素<message>
                    // 属性dbname/reserveTimeSpan/defaultQueue
                    node = dom.DocumentElement.SelectSingleNode("message") as XmlElement;
                    if (node != null)
                    {
                        this.MessageDbName = DomUtil.GetAttr(node, "dbname");
                        this.MessageReserveTimeSpan = DomUtil.GetAttr(node, "reserveTimeSpan");
                        // 2016/4/10
                        this.OutgoingQueue = DomUtil.GetAttr(node, "defaultQueue");

                        // 2010/12/31 add
                        if (String.IsNullOrEmpty(this.MessageReserveTimeSpan) == true)
                            this.MessageReserveTimeSpan = "365day";
                    }
                    else
                    {
                        this.MessageDbName = "";
                        this.MessageReserveTimeSpan = "365day";
                        this.OutgoingQueue = "";
                    }

                    // OPAC服务器
                    // 元素<opacServer>
                    // 属性url
                    node = dom.DocumentElement.SelectSingleNode("opacServer") as XmlElement;
                    if (node != null)
                    {
                        app.OpacServerUrl = DomUtil.GetAttr(node, "url");
                    }
                    else
                    {
                        app.OpacServerUrl = "";
                    }

                    // 违约金
                    // 元素<amerce>
                    // 属性 @dbname @overdueStyle
                    node = dom.DocumentElement.SelectSingleNode("amerce") as XmlElement;
                    if (node != null)
                    {
                        app.AmerceDbName = DomUtil.GetAttr(node, "dbname");
                        app.OverdueStyle = DomUtil.GetAttr(node, "overdueStyle");
                    }
                    else
                    {
                        app.AmerceDbName = "";
                        app.OverdueStyle = "";
                    }

                    // 发票
                    // 元素<invoice>
                    // 属性dbname
                    node = dom.DocumentElement.SelectSingleNode("invoice") as XmlElement;
                    if (node != null)
                    {
                        app.InvoiceDbName = DomUtil.GetAttr(node, "dbname");
                    }
                    else
                    {
                        app.InvoiceDbName = "";
                    }

                    // 拼音
                    // 元素<pinyin>
                    // 属性dbname
                    node = dom.DocumentElement.SelectSingleNode("pinyin") as XmlElement;
                    if (node != null)
                    {
                        app.PinyinDbName = DomUtil.GetAttr(node, "dbname");
                    }
                    else
                    {
                        app.PinyinDbName = "";
                    }

                    // GCAT
                    // 元素<gcat>
                    // 属性dbname
                    node = dom.DocumentElement.SelectSingleNode("gcat") as XmlElement;
                    if (node != null)
                    {
                        app.GcatDbName = DomUtil.GetAttr(node, "dbname");
                    }
                    else
                    {
                        app.GcatDbName = "";
                    }

                    // 词
                    // 元素<word>
                    // 属性dbname
                    node = dom.DocumentElement.SelectSingleNode("word") as XmlElement;
                    if (node != null)
                    {
                        app.WordDbName = DomUtil.GetAttr(node, "dbname");
                    }
                    else
                    {
                        app.WordDbName = "";
                    }

                    // *** 进入内存的参数结束

                    // bin dir
                    app.BinDir = strBinDir;

                    nRet = 0;

                    {
#if LOG_INFO
                        app.WriteErrorLog("INFO: LoadReaderDbGroupParam");
#endif
                        // <readerdbgroup>
                        app.LoadReaderDbGroupParam(dom);

#if LOG_INFO
                        app.WriteErrorLog("INFO: LoadItemDbGroupParam");
#endif

                        // <itemdbgroup> 
                        nRet = app.LoadItemDbGroupParam(dom,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog(strError);
                            goto ERROR1;
                        }

                        // 临时的SessionInfo对象
                        SessionInfo session = new SessionInfo(this);
                        try
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: InitialKdbs");
#endif

                            // 初始化kdbs
                            nRet = InitialKdbs(session.Channels,
                                out strError);
                            if (nRet == -1)
                            {
                                app.WriteErrorLog("ERR001 首次初始化kdbs失败: " + strError);
                                // DefaultThread可以重试初始化

                                // session.Close();
                                // goto ERROR1;
                            }
                            else
                            {
#if LOG_INFO
                                app.WriteErrorLog("INFO: CheckKernelVersion");
#endif

                                // 检查 dpKernel 版本号
                                nRet = CheckKernelVersion(session.Channels,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }

#if LOG_INFO
                            app.WriteErrorLog("INFO: InitialVdbs");
#endif

                            // 2008/6/6  重新初始化虚拟库定义
                            // 这样，其他地方调用的InitialVdbs()就可以去除了
                            // TODO: 为了提高运行速度，可以优化为，只有当<virtualDatabases>元素下的内容有改变时，才重新进行这个初始化
                            this.vdbs = null;
                            nRet = app.InitialVdbs(session.Channels,
                                out strError);
                            if (nRet == -1)
                            {
                                app.WriteErrorLog("ERR002 首次初始化vdbs失败: " + strError);
                            }

                        }
                        finally
                        {
                            session.CloseSession();
                            session = null;

#if LOG_INFO
                            app.WriteErrorLog("INFO: 临时 session 使用完毕");
#endif
                        }

                    }

                    // 时钟
                    string strClock = DomUtil.GetElementText(dom.DocumentElement, "clock");
                    try
                    {
                        this.Clock.Delta = Convert.ToInt64(strClock);
                    }
                    catch
                    {
                        // TODO: 写入错误日志
                    }

                    // *** 初始化操作日志环境
                    if (bReload == false)   // 2014/4/2
                    {
                        // this.OperLogDir = strOperLogDir;    // 2006/12/7 
#if LOG_INFO
                        app.WriteErrorLog("INFO: OperLog.Initial");
#endif

                        // 注：OperLog 对象在 Initial() 之前，应该处于不可用状态。这样可以避免修复以前写入内容造成混乱
                        nRet = this.OperLog.Initial(this,
                            strOperLogDir,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog(strError);
                            goto ERROR1;
                        }
                    }

                    // *** 初始化统计对象
                    // if (bReload == false)   // 2014/4/2
                    {
#if LOG_INFO
                        app.WriteErrorLog("INFO: Statis.Initial");
#endif

                        this.Statis = new Statis();
                        nRet = this.Statis.Initial(this, out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog(strError);
                            goto ERROR1;
                        }
                    }

#if LOG_INFO
                    app.WriteErrorLog("INFO: InitialLibraryHostAssembly");
#endif
                    // 初始化LibraryHostAssembly对象
                    // 必须在ReadersMonitor以前启动。否则其中用到脚本代码时会出错。2007/10/10 changed
                    // return:
                    //		-1	出错
                    //		0	脚本代码没有找到
                    //      1   成功
                    nRet = this.InitialLibraryHostAssembly(out strError);
                    if (nRet == -1)
                    {
                        app.WriteErrorLog(strError);
                        goto ERROR1;
                    }

#if LOG_INFO
                    app.WriteErrorLog("INFO: InitialExternalMessageInterfaces");
#endif

                    // 初始化扩展消息接口
                    nRet = app.InitialExternalMessageInterfaces(
                    out strError);
                    if (nRet == -1)
                    {
                        strError = "初始化扩展的消息接口时出错: " + strError;
                        app.WriteErrorLog(strError);
                        // goto ERROR1;
                    }

                    // 创建 MSMQ 消息队列
#if LOG_INFO
                    app.WriteErrorLog("INFO: Message Queue");
#endif

                    ///
                    app.InitialMsmq();
                    if (string.IsNullOrEmpty(this.OutgoingQueue))
                    {
#if LOG_INFO
                        app.WriteErrorLog("INFO: Message Queue 未被启用");
#endif
                    }

                    // 初始化 mongodb 相关对象
                    nRet = InitialMongoDatabases(out strError);
                    if (nRet == -1)
                    {
                        // app.HangupReason = LibraryServer.HangupReason.StartingError;
                        app.WriteErrorLog("ERR002 首次初始化 mongodb database 失败: " + strError);
                        app.AddHangup("ERR002");
                    }

#if LOG_INFO
                    app.WriteErrorLog("INFO: 准备下属数据库对象");
#endif
                    //
                    this.IssueItemDatabase = new IssueItemDatabase(this);
                    this.OrderItemDatabase = new OrderItemDatabase(this);
                    this.CommentItemDatabase = new CommentItemDatabase(this);

#if LOG_INFO
                    app.WriteErrorLog("INFO: MessageCenter");
#endif
                    // 
                    this.MessageCenter = new MessageCenter();
                    this.MessageCenter.ServerUrl = this.WsUrl;
                    this.MessageCenter.MessageDbName = this.MessageDbName;

                    this.MessageCenter.VerifyAccount -= new VerifyAccountEventHandler(MessageCenter_VerifyAccount); // 2008/6/6 
                    this.MessageCenter.VerifyAccount += new VerifyAccountEventHandler(MessageCenter_VerifyAccount);

                    if (this.BatchTasks == null)
                        this.BatchTasks = new BatchTaskCollection();    // Close() 的时候会设置为 null。因此这里要准备重新 new

                    // 启动批处理任务
                    // TODO: 这一段考虑分离到一个函数中
                    if (bReload == false)
                    {
                        string strBreakPoint = "";

#if LOG_INFO
                        app.WriteErrorLog("INFO: DefaultThread");
#endif
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
                            app.WriteErrorLog("启动后台任务 DefaultThread 时出错：" + ex.Message);
                            goto ERROR1;
                        }

#if LOG_INFO
                        app.WriteErrorLog("INFO: OperLogThread");
#endif
                        // 启动 OperLogThread
                        try
                        {
                            OperLogThread thread = new OperLogThread(this, null);
                            this.BatchTasks.Add(thread);

                            thread.StartWorkerThread();

                            this.operLogThread = thread;
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog("启动后台任务 OperLogThread 时出错：" + ex.Message);
                            goto ERROR1;
                        }

#if LOG_INFO
                        app.WriteErrorLog("INFO: ArriveMonitor");
#endif
                        // 启动ArriveMonitor
                        try
                        {
                            ArriveMonitor arriveMonitor = new ArriveMonitor(this, null);
                            this.BatchTasks.Add(arriveMonitor);

                            arriveMonitor.StartWorkerThread();
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog("启动批处理任务ArriveMonitor时出错：" + ex.Message);
                            goto ERROR1;
                        }

#if LOG_INFO
                        app.WriteErrorLog("INFO: ReadersMonitor");
#endif
                        // 启动ReadersMonitor
                        try
                        {
                            ReadersMonitor readersMonitor = new ReadersMonitor(this, null);
                            this.BatchTasks.Add(readersMonitor);

                            readersMonitor.StartWorkerThread();
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog("启动批处理任务ReadersMonitor时出错：" + ex.Message);
                            goto ERROR1;
                        }

#if LOG_INFO
                        app.WriteErrorLog("INFO: MessageMonitor");
#endif
                        // 启动MessageMonitor
                        try
                        {
                            MessageMonitor messageMonitor = new MessageMonitor(this, null);
                            this.BatchTasks.Add(messageMonitor);

                            // 从断点记忆文件中读出信息
                            // return:
                            //      -1  error
                            //      0   file not found
                            //      1   found
                            nRet = ReadBatchTaskBreakPointFile(messageMonitor.DefaultName,
                                out strBreakPoint,
                                out strError);
                            if (nRet == -1)
                            {
                                app.WriteErrorLog("ReadBatchTaskBreakPointFile时出错：" + strError);
                            }

                            if (messageMonitor.StartInfo == null)
                                messageMonitor.StartInfo = new BatchTaskStartInfo();   // 按照缺省值来

                            // 如果需要从断点启动
                            if (nRet == 1)
                                messageMonitor.StartInfo.Start = "!breakpoint";  //strBreakPoint;

                            messageMonitor.ClearProgressFile();   // 清除进度文件内容
                            messageMonitor.StartWorkerThread();
                        }
                        catch (Exception ex)
                        {
                            app.WriteErrorLog("启动批处理任务MessageMonitor时出错：" + ex.Message);
                            goto ERROR1;
                        }

                        // 启动DkywReplication
                        // <dkyw>
                        node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("dkyw") as XmlElement;
                        if (node != null)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: DkywReplication");
#endif
                            try
                            {
                                DkywReplication dkyw = new DkywReplication(this, null);
                                this.BatchTasks.Add(dkyw);

                                /*
                                // 从断点记忆文件中读出信息
                                // return:
                                //      -1  error
                                //      0   file not found
                                //      1   found
                                nRet = ReadBatchTaskBreakPointFile(dkyw.DefaultName,
                                    out strBreakPoint,
                                    out strError);
                                if (nRet == -1)
                                {
                                    app.WriteErrorLog("ReadBatchTaskBreakPointFile时出错：" + strError);
                                }
                                 * */
                                bool bLoop = false;
                                string strLastNumber = "";

                                // return:
                                //      -1  出错
                                //      0   没有找到断点信息
                                //      1   找到了断点信息
                                nRet = dkyw.ReadLastNumber(
                                    out bLoop,
                                    out strLastNumber,
                                    out strError);
                                if (nRet == -1)
                                {
                                    app.WriteErrorLog("ReadLastNumber时出错：" + strError);
                                }

                                if (dkyw.StartInfo == null)
                                    dkyw.StartInfo = new BatchTaskStartInfo();   // 按照缺省值来

                                if (bLoop == true)
                                {
                                    // 需要从断点启动
                                    if (nRet == 1)
                                        dkyw.StartInfo.Start = "!breakpoint";  //strBreakPoint;

                                    dkyw.ClearProgressFile();   // 清除进度文件内容
                                    dkyw.StartWorkerThread();
                                }
                            }
                            catch (Exception ex)
                            {
                                app.WriteErrorLog("启动批处理任务DkywReplication时出错：" + ex.Message);
                                goto ERROR1;
                            }
                        }

                        // 启动PatronReplication
                        // <patronReplication>
                        // 读者库数据同步 批处理任务
                        // 从卡中心同步读者数据
                        node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("patronReplication") as XmlElement;
                        if (node != null)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: PatronReplication");
#endif
                            try
                            {
                                PatronReplication patron_rep = new PatronReplication(this, null);
                                this.BatchTasks.Add(patron_rep);

                                patron_rep.StartWorkerThread();
                            }
                            catch (Exception ex)
                            {
                                app.WriteErrorLog("启动批处理任务 PatronReplication 时出错：" + ex.Message);
                                goto ERROR1;
                            }
                        }

#if NO  // 暂时不允许使用这个验证性的功能 2017/6/8
                        // 启动 LibraryReplication

#if LOG_INFO
                        app.WriteErrorLog("INFO: LibraryReplication ReadBatchTaskBreakPointFile");
#endif
                        // 从断点记忆文件中读出信息
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   found
                        nRet = ReadBatchTaskBreakPointFile("dp2Library 同步",
                            out strBreakPoint,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("ReadBatchTaskBreakPointFile() 时出错：" + strError);
                        }
                        // 如果nRet == 0，表示没有断点文件存在，也就不必自动启动这个任务

                        // strBreakPoint 并未被使用。而是断点文件是否存在，这一信息有价值。

                        if (nRet == 1)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: LibraryReplication");
#endif
                            try
                            {

                                // 从断点文件中取出断点字符串
                                // 断点字符串格式：序号.偏移量@日志文件名
                                //  或者：序号@日志文件名
                                // 获得断点信息的整个过程的代码，是否适宜归入TraceDTLP类？
                                // 如果成熟，可以归纳作为BatchTask基类的一个特性。

                                LibraryReplication replication = new LibraryReplication(this, null);
                                this.BatchTasks.Add(replication);

                                if (replication.StartInfo == null)
                                    replication.StartInfo = new BatchTaskStartInfo();   // 按照缺省值来
                                replication.StartInfo.Start = "date=continue";  // 从断点开始做
                                replication.ClearProgressFile();   // 清除进度文件内容
                                replication.StartWorkerThread();
                            }
                            catch (Exception ex)
                            {
                                app.WriteErrorLog("启动批处理任务时出错：" + ex.Message);
                                goto ERROR1;
                            }
                        }

#endif

                        // 启动 RebuildKeys

#if LOG_INFO
                        app.WriteErrorLog("INFO: RebuildKeys ReadBatchTaskBreakPointFile");
#endif
                        // 从断点记忆文件中读出信息
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   found
                        nRet = ReadBatchTaskBreakPointFile("重建检索点",
                            out strBreakPoint,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("ReadBatchTaskBreakPointFile() 时出错：" + strError);
                        }
                        // 如果nRet == 0，表示没有断点文件存在，也就不必自动启动这个任务

                        // strBreakPoint 并未被使用。而是断点文件是否存在，这一信息有价值。

                        if (nRet == 1)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: RebuildKeys");
#endif
                            try
                            {

                                // 从断点文件中取出断点字符串
                                RebuildKeys replication = new RebuildKeys(this, null);
                                this.BatchTasks.Add(replication);

                                if (replication.StartInfo == null)
                                    replication.StartInfo = new BatchTaskStartInfo();   // 按照缺省值来
                                replication.StartInfo.Start = "dbnamelist=continue";  // 从断点开始做
                                replication.ClearProgressFile();   // 清除进度文件内容
                                replication.StartWorkerThread();
                            }
                            catch (Exception ex)
                            {
                                app.WriteErrorLog("启动批处理任务时出错：" + ex.Message);
                                goto ERROR1;
                            }
                        }


                        // 启动 BackupTask

#if LOG_INFO
                        app.WriteErrorLog("INFO: BackupTask ReadBatchTaskBreakPointFile");
#endif
                        // 从断点记忆文件中读出信息
                        // return:
                        //      -1  error
                        //      0   file not found
                        //      1   found
                        nRet = ReadBatchTaskBreakPointFile("大备份",
                            out strBreakPoint,
                            out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("ReadBatchTaskBreakPointFile() 时出错：" + strError);
                        }
                        // 如果nRet == 0，表示没有断点文件存在，也就不必自动启动这个任务

                        // strBreakPoint 并未被使用。而是断点文件是否存在，这一信息有价值。

                        if (nRet == 1)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: BackupTask");
#endif
                            try
                            {

                                // 从断点文件中取出断点字符串
                                BackupTask backup = new BackupTask(this, null);
                                this.BatchTasks.Add(backup);

                                if (backup.StartInfo == null)
                                    backup.StartInfo = new BatchTaskStartInfo();   // 按照缺省值来
                                backup.StartInfo.Start = "dbnamelist=continue";  // 从断点开始做
                                backup.ClearProgressFile();   // 清除进度文件内容
                                backup.StartWorkerThread();
                            }
                            catch (Exception ex)
                            {
                                app.WriteErrorLog("启动批处理任务时出错：" + ex.Message);
                                goto ERROR1;
                            }
                        }

                    }

                    // 公共查询最大命中数
                    {
                        XmlNode nodeTemp = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//virtualDatabases");
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


#if NO
            if (bReload == false)
            {
                PathUtil.CreateDirIfNeed(strColumnDir);	// 确保目录创建
                nRet = LoadCommentColumn(
                    PathUtil.MergePath(strColumnDir, "comment"),
                    out strError);
                if (nRet == -1)
                {
                    app.WriteErrorLog("装载栏目存储时出错: " + strError);
                }
            }
#endif

                    // 升级 library.xml 文件版本
                    if (bReload == false)
                    {
#if LOG_INFO
                        app.WriteErrorLog("INFO: UpgradeLibraryXml");
#endif
                        nRet = this.UpgradeLibraryXml(out strError);
                        if (nRet == -1)
                        {
                            app.WriteErrorLog("升级library.xml时出错：" + strError);
                        }
                    }

                    if (bReload == true)
                        app.WriteErrorLog("LibraryService 结束重新装载 " + this.m_strFileName);
                    else
                    {
                        TimeSpan delta = DateTime.Now - start;
                        app.WriteErrorLog("LibraryService 成功初始化。初始化操作耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

                        // 写入down机检测文件
                        app.WriteAppDownDetectFile("LibraryService 成功初始化。");

                        if (this.watcher == null)
                        {
#if LOG_INFO
                            app.WriteErrorLog("INFO: BeginWatcher");
#endif

                            BeginWatcher();
#if LOG_INFO
                            app.WriteErrorLog("INFO: End BeginWatcher");
#endif
                        }

#if NO
                if (this.virtual_watcher == null)
                    BeginVirtualDirWatcher();
#endif
                    }

                    if (this.MaxClients != 255) // 255 通道情况下不再检查版本失效日期 2016/11/3
                    {
                        // DateTime expire = new DateTime(2018, 9, 15); // 上一个版本是 2018/7/15 2018/5/15 2018/3/15 2017/1/15 2017/12/1 2017/9/1 2017/6/1 2017/3/1 2016/11/1
                        if (DateTime.Now > _expire)
                        {
                            if (this.MaxClients == 255)
                            {
                                this.WriteErrorLog("*** 当前 dp2library 版本已于 " + _expire.ToLongDateString() + " 失效。请系统管理员注意主动升级 dp2library");
                            }
                            else
                            {
                                // 通知系统挂起
                                // this.HangupReason = HangupReason.Expire;
                                this.WriteErrorLog("*** 当前 dp2library 版本因为长期没有升级，已经失效。系统被挂起。请立即升级 dp2library 到最新版本");
                                app.AddHangup("Expire");
                            }
                        }
                    }
                    else
                        this.WriteErrorLog("*** 特殊版本不检查失效日期。请系统管理员注意每隔半年主动升级一次 dp2library");

                    // 2013/4/10
                    if (this.Changed == true)
                        this.ActivateManagerThread();
                }
                catch (Exception ex)
                {
                    strError = "LoadCfg() 抛出异常: " + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                // 2019/4/26
                this.WriteErrorLog($"LoadCfg() 出现异常: {ExceptionUtil.GetExceptionText(ex)}");
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }
        // 2008/10/13 
        ERROR1:
            if (bReload == false)
            {
                if (this.watcher == null)
                {
#if LOG_INFO
                    app.WriteErrorLog("INFO: BeginWatcher");
#endif

                    BeginWatcher();
#if LOG_INFO
                    app.WriteErrorLog("INFO: End BeginWatcher");
#endif
                }
#if NO
                if (this.virtual_watcher == null)
                    BeginVirtualDirWatcher();
#endif

            }

            if (bReload == true)
            {
                app.WriteErrorLog("LibraryService 重新装载 " + this.m_strFileName + " 的过程发生严重错误 [" + strError + "]，服务处于残缺状态，请及时排除故障后重新启动");
                // 2021/8/27
                // TODO: 要想办法让前端可以感知错误，但不影响修改 kernel 配置文件的 API 执行
            }
            else
            {
                // app.HangupReason = LibraryServer.HangupReason.StartingError;
                app.WriteErrorLog("LibraryService 初始化过程发生严重错误 [" + strError + "]，当前此服务处于残缺状态，请及时排除故障后重新启动");
                app.AddHangup("StartingError");
            }
            return -1;
        }

        void CleanSessionDir(string strSessionDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strSessionDir);

                // 删除所有的下级目录
                DirectoryInfo[] dirs = di.GetDirectories();
                foreach (DirectoryInfo childDir in dirs)
                {
                    Directory.Delete(childDir.FullName, true);
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("删除 session 下级目录时出错: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        public string GetTempFileName(string strPrefix)
        {
            return Path.Combine(this.TempDir, "~" + strPrefix + "_" + Guid.NewGuid().ToString());
        }

        public int CheckKernelVersion(RmsChannelCollection Channels,
    out string strError)
        {
            strError = "";

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            string strVersion = "";
            long lRet = channel.GetVersion(out strVersion,
                out strError);
            if (lRet == -1)
            {
                strError = "获取 dpKernel 版本号过程发生错误：" + strError;
                return -1;
            }

            // 检查最低版本号
            try
            {
                Version version = new Version(strVersion);
                Version dp2kernel_base_version = new Version("3.1");
                if (version.CompareTo(dp2kernel_base_version) < 0)
                {
                    strError = "当前 dp2Library 版本需要和 dp2Kernel " + dp2kernel_base_version + " 以上版本配套使用(然而当前 dp2Kernel 版本号为 " + version + ")。请立即升级 dp2Kernel 到最新版本。";
                    return -1;
                }
#if NO
                double value = 0;
                if (double.TryParse(strVersion, out value) == false)
                {
                    strError = "dp2Kernel版本号 '" + strVersion + "' 格式不正确";
                    return -1;
                }

                double base_version = 2.63;

                if (value < base_version)
                {
                    strError = "当前 dp2Library 版本需要和 dp2Kernel " + base_version + " 以上版本配套使用(然而当前 dp2Kernel 版本号为 " + value + ")。请立即升级 dp2Kernel 到最新版本。";
                    return -1;
                }
#endif

                return 0;
            }
            catch (Exception ex)
            {
                strError = "比较 dp2kernel 版本号的过程发生错误: " + ex.Message;
                return -1;
            }
        }

#if NO
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
#endif

        int UpgradeLibraryXml(out string strError)
        {
            strError = "";
            bool bChanged = false;

            // 找到<version>元素
            XmlNode nodeVersion = this.LibraryCfgDom.DocumentElement.SelectSingleNode("version");
            if (nodeVersion == null)
            {
                nodeVersion = this.LibraryCfgDom.CreateElement("version");

                /*
                 * 没有必要，因为save()时会重新排列位置
                // 尽量插入到第一个的位置
                if (this.LibraryCfgDom.DocumentElement.ChildNodes.Count > 0)
                    this.LibraryCfgDom.DocumentElement.InsertBefore(nodeVersion,
                        this.LibraryCfgDom.DocumentElement.ChildNodes[0]);
                else
                 * */
                this.LibraryCfgDom.DocumentElement.AppendChild(nodeVersion);

                nodeVersion.InnerText = "0.01";    // 从未有过<version>元素的library.xml版本，被认为是0.01版
                bChanged = true;
            }

            string strVersion = nodeVersion.InnerText;
            if (String.IsNullOrEmpty(strVersion) == true)
                strVersion = "0.01";

            double version = 0.01;
            try
            {
                version = Convert.ToDouble(strVersion);
            }
            catch
            {
                version = 0.01;
            }

            // 从0.01版升级
            if (version == 0.01)
            {
                /*
                 * 从下列片断中抽出<group>元素的zhongcihaodb属性值，去重，然后加入<utilDb>元素内
    <zhongcihao>
        <nstable name="nstable">
            <item prefix="marc" uri="http://dp2003.com/UNIMARC" />
        </nstable>
        <group name="中文书目" zhongcihaodb="种次号">
            <database name="中文图书" leftfrom="索取类号" rightxpath="//marc:record/marc:datafield[@tag='905']/marc:subfield[@code='e']/text()" titlexpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='a']/text()" authorxpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f' or @code='g']/text()" />
        </group>
    </zhongcihao>
                 */
                List<string> dbnames = new List<string>();
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("zhongcihao/group");
                for (int i = 0; i < nodes.Count; i++)
                {
                    string strDbName = DomUtil.GetAttr(nodes[i], "zhongcihaodb");
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;
                    if (dbnames.IndexOf(strDbName) != -1)
                        continue;
                    dbnames.Add(strDbName);
                }

                XmlNode nodeUtilDb = this.LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb");
                if (nodeUtilDb == null)
                {
                    nodeUtilDb = this.LibraryCfgDom.CreateElement("utilDb");
                    this.LibraryCfgDom.DocumentElement.AppendChild(nodeUtilDb);
                    bChanged = true;
                }

                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];
                    // 看看<utilDb>中是否已经有了
                    XmlNode nodeExist = this.LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strDbName + "']");
                    if (nodeExist != null)
                    {
                        string strType = DomUtil.GetAttr(nodeExist, "type");
                        if (strType != "zhongcihao")
                        {
                            strError = "<utilDb>下name属性值为'" + strDbName + "'的<database>元素，其type属性值不为'zhongcihao'(而是'" + strType + "')，这和<zhongcihao>元素下的初始定义矛盾。请系统管理员在了解这个库的真实类型后，手动对配置文件进行修改。";
                            return -1;
                        }
                        continue;
                    }

                    XmlNode nodeDatabase = this.LibraryCfgDom.CreateElement("database");
                    nodeUtilDb.AppendChild(nodeDatabase);

                    DomUtil.SetAttr(nodeDatabase, "name", strDbName);
                    DomUtil.SetAttr(nodeDatabase, "type", "zhongcihao");

                    bChanged = true;
                }

                // 升级完成后，修改版本号
                nodeVersion.InnerText = "0.02";
                bChanged = true;
                WriteErrorLog("自动升级library.xml v0.01到v0.02");
                version = 0.02;
            }

            // 2009/3/10
            // 从0.02版升级
            if (version == 0.02)
            {
                // 将<rightstable>元素名修改为<rightsTable>
                XmlNode nodeRightsTable = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightstable");
                if (nodeRightsTable != null)
                {
                    // 创建一个新元素
                    XmlNode nodeNew = this.LibraryCfgDom.CreateElement("rightsTable");
                    this.LibraryCfgDom.DocumentElement.InsertAfter(nodeNew, nodeRightsTable);

                    nodeNew.InnerXml = nodeRightsTable.InnerXml;

                    // 删除旧元素
                    nodeRightsTable.ParentNode.RemoveChild(nodeRightsTable);

                    nodeRightsTable = nodeNew;
                }
                else
                {
                    nodeRightsTable = this.LibraryCfgDom.CreateElement("rightsTable");
                    this.LibraryCfgDom.DocumentElement.AppendChild(nodeRightsTable);
                }

                // 将根下的<readertypes>和<booktypes>移动到<rightsTable>元素下，并且把元素名修改为<readerTypes>和<bookTypes>
                XmlNode nodeReaderTypes = this.LibraryCfgDom.DocumentElement.SelectSingleNode("readertypes");
                if (nodeReaderTypes != null)
                {
                    // 创建一个新元素
                    XmlNode nodeNew = this.LibraryCfgDom.CreateElement("readerTypes");
                    nodeRightsTable.AppendChild(nodeNew);

                    nodeNew.InnerXml = nodeReaderTypes.InnerXml;
                    nodeReaderTypes.ParentNode.RemoveChild(nodeReaderTypes);
                }

                XmlNode nodeBookTypes = this.LibraryCfgDom.DocumentElement.SelectSingleNode("booktypes");
                if (nodeBookTypes != null)
                {
                    // 创建一个新元素
                    XmlNode nodeNew = this.LibraryCfgDom.CreateElement("bookTypes");
                    nodeRightsTable.AppendChild(nodeNew);

                    nodeNew.InnerXml = nodeBookTypes.InnerXml;
                    nodeBookTypes.ParentNode.RemoveChild(nodeBookTypes);
                }

                // 将<locationtypes>元素名修改为<locationTypes>
                XmlNode nodeLocationTypes = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationtypes");
                if (nodeLocationTypes != null)
                {
                    // 创建一个新元素
                    XmlNode nodeNew = this.LibraryCfgDom.CreateElement("locationTypes");
                    this.LibraryCfgDom.DocumentElement.InsertAfter(nodeNew, nodeLocationTypes);

                    nodeNew.InnerXml = nodeLocationTypes.InnerXml;
                }

                // 升级完成后，修改版本号
                nodeVersion.InnerText = "0.03";
                bChanged = true;
                WriteErrorLog("自动升级library.xml v0.02到v0.03");
                version = 0.03;
            }



#if NO
            // 从 2.00 版升级
            // 2013/12/10
            if (version <= 2.00)
            {
                // bool bChanged = false;
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("accounts/account");
                foreach (XmlNode node in nodes)
                {
                    string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                    {
                        DomUtil.SetAttr(node, "libraryCode", "<global>");
                        // bChanged = true;
                    }
                }

                // 升级完成后，修改版本号
                nodeVersion.InnerText = "2.01";
                bChanged = true;
                WriteErrorLog("自动升级 library.xml v2.00 到 v2.01");
                version = 2.01;
            }
#endif

            // 2015/5/20
            // 从2.00版升级
            if (version <= 2.00)
            {
                // 升级 library.xml 中的用户账户相关信息
                // 文件格式 0.03-->0.04
                // accounts/account 中 password 存储方式改变
                XmlDocument temp = this.LibraryCfgDom;
                int nRet = LibraryServerUtil.UpgradeLibraryXmlUserInfo(
                    EncryptKey,
                    ref temp,
                    out strError);
                if (nRet == -1)
                    WriteErrorLog("自动升级 library.xml v2.00(或以下)到v2.01 时出错: " + strError + "。为了修复这个问题，请系统管理员重设所有工作人员账户的密码");

                this.LibraryCfgDom = temp;

                // 升级完成后，修改版本号
                nodeVersion.InnerText = "2.01";
                bChanged = true;
                WriteErrorLog("自动升级 library.xml v2.00(或以下)到v2.01");
                version = 2.01;
            }

            // 2021/6/29
            // 从 2.01 版升级
            if (version < 3.00)
            {
                var now = DateTime.Now;
                // 将 account 元素中的 password 属性转移到 password 元素(innerText)中
                XmlNodeList account_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("accounts/account");
                foreach (XmlElement account in account_nodes)
                {
                    if (account.HasAttribute("password") == false)
                        continue;
                    string password_text = account.GetAttribute("password");
                    XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
                    if (password_element == null)
                    {
                        password_element = this.LibraryCfgDom.CreateElement("password");
                        password_element = account.AppendChild(password_element) as XmlElement;
                    }
                    password_element.InnerText = password_text;

                    string userName = account.GetAttribute("name");
                    string rights = account.GetAttribute("rights");

                    // 设置密码失效时刻
                    // 注意: 特殊代理账户和权限中包含 neverExpire 的用户，其密码永远不失效？
                    if (LibraryServerUtil.IsSpecialUserName(userName) == false
                        && StringUtil.IsInList("neverexpire", rights) == false
                        && _passwordExpirePeriod != TimeSpan.MaxValue)
                    {
                        string strExpireTime = DateTimeUtil.Rfc1123DateTimeStringEx(now + _passwordExpirePeriod); // 本地时间
                        password_element.SetAttribute("expire", strExpireTime);
                    }

                    account.RemoveAttribute("password");
                }

                // 升级完成后，修改版本号
                nodeVersion.InnerText = "3.00";
                bChanged = true;
                WriteErrorLog($"自动升级 library.xml v{version.ToString()}到v3.00");
                version = 3.00;
            }

            if (bChanged == true)
            {
                this.Changed = true;
                this.ActivateManagerThread();   // 2009/3/10 
            }

            return 0;
        }

        // 根据 account/@passwordExpireLength 参数，重建或者清除 account 密码失效期
        // 如果先前有 expire，后来修改了 expire 长度，本函数不负责修改 account 中的 expire 时间。本函数只负责响应创建和清除
        void CreateOrDeletePasswordExpire(XmlDocument cfg_dom)
        {
            // 2021/7/7
            string version = cfg_dom.DocumentElement.SelectSingleNode("version/text()")?.Value;
            if (string.IsNullOrEmpty(version)
                || StringUtil.CompareVersion(version, "3.0") < 0)
                return;

            var create = _passwordExpirePeriod != TimeSpan.MaxValue;

            bool changed = false;
            var now = DateTime.Now;
            XmlNodeList account_nodes = cfg_dom.DocumentElement.SelectNodes("accounts/account");
            foreach (XmlElement account in account_nodes)
            {
                var userName = account.GetAttribute("name");
                if (create && LibraryServerUtil.IsSpecialUserName(userName) == false)
                {
                    if (SetPasswordExpire(account, _passwordExpirePeriod, now, true) == true)  // 增补。但不修改已有的 exipre 属性值
                        changed = true;
                }
                else
                {
                    if (ClearPasswordExpire(account) == true)
                        changed = true;
                }
            }

            if (changed == true)
            {
                this.Changed = true;
            }
        }

        // 工作人员密码失效时间长度。例如 90days
        internal TimeSpan _passwordExpirePeriod = TimeSpan.MaxValue;  // 默认为不失效
        // 工作人员密码的风格。目前有 style-1 一种
        internal string _passwordStyle = "";

        // 读者密码失效时间长度。例如 90days
        internal TimeSpan _patronPasswordExpirePeriod = TimeSpan.MaxValue;  // 默认为不失效
        // 读者密码的风格。目前有 style-1 一种
        internal string _patronPasswordStyle = "";
        // 临时密码(转为正式密码后的)失效时间长度。例如 6mins
        internal TimeSpan _tempPasswordExpirePeriod = new TimeSpan(1, 0, 0); // 一小时

        // 临时密码失效周期
        // static TimeSpan _tempPasswordExpirePeriod = new TimeSpan(1, 0, 0); // 一小时



        // 2008/5/8
        // return:
        //      -1  出错
        //      0   成功
        public int InitialKdbs(
            RmsChannelCollection Channels,
            out string strError)
        {
            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
#if NO
                this.kdbs = new KernelDbInfoCollection();
                int nRet = this.kdbs.Initial(Channels,
                            this.WsUrl,
                            "zh",
                            out strError);
                if (nRet == -1)
                {
                    // this.vdbs = null;   // BUG!!!
                    this.kdbs = null;
                    return -1;
                }
#endif

                // kdbs 初始化的过程是需要耗费时间的，如果在这中间访问，可能有些信息来不及初始化，找不到
                // 所以这里先整个初始化好了以后，然后才挂接到 this.kdbs 上
                // 有另外一个方法是所有使用的地方都 利用 this.m_lock 读锁定，但缺点是太麻烦
                this.kdbs = null;
                KernelDbInfoCollection kdbs = new KernelDbInfoCollection();
                // return:
                //      -1  出错
                //      0   成功
                int nRet = kdbs.Initial(Channels,
                            this.WsUrl,
                            "zh",
                            out strError);
                if (nRet == -1)
                    return -1;

                this.kdbs = kdbs;

                // 2015/5/7
                BiblioDbFromInfo[] infos = null;
                // 列出某类数据库的检索途径信息
                // return:
                //      -1  出错
                //      0   没有定义
                //      1   成功
                nRet = this.ListDbFroms("arrived",
                    "zh",
                    "",
                    out infos,
                    out strError);
                this.ArrivedDbFroms = infos;
                return 0;
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }
        }

        // MSMQ 环境是否初始化成功过至少一次
        internal bool MsmqInitialized = false;

        // 尝试初始化 MSMQ 环境
        public void InitialMsmq()
        {
            if (MsmqInitialized == true)
                return;

            if (string.IsNullOrEmpty(this.OutgoingQueue) == true)
            {
                // 清除 Hangup 状态
                if (this.ContainsHangup("MessageQueueCreateFail") == true)
                {
                    this.ClearHangup("MessageQueueCreateFail");
                    //this.WriteErrorLog("*** 系统已解除 MessageQueueCreateFail 挂起状态");
                }
                return;
            }

            try
            {
#if NO
                            if (MessageQueue.Exists(this.OutgoingQueue))
                            {
                                MessageQueue.Delete(this.OutgoingQueue);
                            }
#endif

                if (!MessageQueue.Exists(this.OutgoingQueue))
                {
                    MessageQueue queue = MessageQueue.Create(this.OutgoingQueue);

#if NO
                                // Create an AccessControlList.
                                AccessControlList list = new AccessControlList();

                                // Create a new trustee to represent the "Everyone" user group.
                                Trustee tr = new Trustee("Everyone");

                                // Create an AccessControlEntry, granting the trustee read access to
                                // the queue.
                                AccessControlEntry entry = new AccessControlEntry(
                                    tr, GenericAccessRights.Read,
                         StandardAccessRights.Read,
                                    AccessControlEntryType.Allow);

                                // Add the AccessControlEntry to the AccessControlList.
                                list.Add(entry);


                                // Apply the AccessControlList to the queue.
                                queue.SetPermissions(list);
#endif

                    var wi = WindowsIdentity.GetCurrent();
                    if (wi.IsSystem == true)
                    {
                        // 当前用户已经是 LocalSystem 了，需要额外给 Everyone 添加权限，以便让 dp2Capo 的控制台方式运行能访问这个 Queue
                        queue.SetPermissions(@"Everyone",
    MessageQueueAccessRights.ReceiveMessage
    | MessageQueueAccessRights.DeleteMessage
    | MessageQueueAccessRights.PeekMessage
    | MessageQueueAccessRights.GenericRead);
                    }

                    // 如果当前是 Administrator，表示可能是 dp2libraryxe 启动的方式，那么需要专门给 LocalSystem 操作 Queue 的权限，以便 Windows Service 方式的 dp2Capo 能访问 Queue
                    var wp = new WindowsPrincipal(wi);
                    if (wp.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        queue.SetPermissions(@"NT AUTHORITY\System",
                            MessageQueueAccessRights.FullControl);
                    }

                    this.WriteErrorLog("首次创建 MSMQ 队列 '" + this.OutgoingQueue + "' 成功");
                }

                MsmqInitialized = true;
                // 清除 Hangup 状态
                if (this.ContainsHangup("MessageQueueCreateFail") == true)
                {
                    this.ClearHangup("MessageQueueCreateFail");
                    //this.WriteErrorLog("*** 系统已解除 MessageQueueCreateFail 挂起状态");
                }
            }
            catch (Exception ex)
            {
                if (this.ContainsHangup("MessageQueueCreateFail") == true)
                {
                    this.WriteErrorLog("*** 重试探测和尝试创建 MSMQ 队列 '" + this.OutgoingQueue + "' 失败: " + ExceptionUtil.GetExceptionMessage(ex)
    + " 系统仍处于挂起状态。");
                }
                else
                {
                    this.WriteErrorLog("*** 探测和尝试创建 MSMQ 队列 '" + this.OutgoingQueue + "' 时出现异常: " + ExceptionUtil.GetDebugText(ex)
                        + " 系统已被挂起。");
                    this.AddHangup("MessageQueueCreateFail");
                }
            }
        }

        public void LockForWrite()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
        }

        public void UnlockForWrite()
        {
            this.m_lock.ReleaseWriterLock();
        }

        public void LockForRead()
        {
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
        }

        public void UnlockForRead()
        {
            this.m_lock.ReleaseReaderLock();
        }

        const string mongodb_base_version = "3.0";  // "4.2.0"

        // 初始化和 mongodb 有关的数据库
        public int InitialMongoDatabases(out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(this.MongoDbConnStr) == true)
                return 0;

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                try
                {
                    this._mongoClient = new MongoClient(this.MongoDbConnStr);
                    // TODO: 如何检测连接是否出错?
                    //var server = this._mongoClient.GetServer();
                    //server.Connect();


                }
                catch (Exception ex)
                {
                    this._mongoClient = null;
                    strError = "初始化 MongoClient 时出错: " + ex.Message;
                    return -1;
                }

                // 检查版本
                try
                {
                    // https://docs.mongodb.com/manual/reference/command/serverStatus/#dbcmd.serverStatus
                    var db = this._mongoClient.GetDatabase("test");
                    var command = new BsonDocument { { "serverStatus", 1 } };
                    var result = db.RunCommand<BsonDocument>(command);
                    // https://grokbase.com/t/gg/mongodb-csharp/12bkre3b6e/deserializing-dynamic-data-numberlong-issue
                    string jsonText = result.ToJson(new JsonWriterSettings
                    {
                        OutputMode = JsonOutputMode.Strict
                    });
                    JObject json = JObject.Parse(jsonText);
                    var version = (string)json["version"];
                    if (StringUtil.CompareVersion(version, mongodb_base_version) < 0)
                    {
                        this._mongoClient = null;
                        strError = $"当前 MongoDB 版本太旧({version})。请升级到 {mongodb_base_version} 以上版本";
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this._mongoClient = null;
                    strError = "检查 MongoDB 版本号时出错: " + ex.Message;
                    return -1;
                }

                int nRet = 0;
                if (this._mongoClient != null)
                {
#if LOG_INFO
                    this.WriteErrorLog("INFO: OpenSummaryStorage");
#endif
                    nRet = OpenSummaryStorage(out strError);
                    if (nRet == -1)
                    {
                        strError = "启动书目摘要库时出错: " + strError;
                        this.WriteErrorLog(strError);
                    }

#if LOG_INFO
                    this.WriteErrorLog("INFO: Open HitCountDatabase");
#endif
                    nRet = this.HitCountDatabase.Open(//this.MongoDbConnStr,
                        this._mongoClient,
                        this.MongoDbInstancePrefix,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "启动计数器库时出错: " + strError;
                        this.WriteErrorLog(strError);
                    }

#if LOG_INFO
                    this.WriteErrorLog("INFO: Open AccessLogDatabase");
#endif
                    nRet = this.AccessLogDatabase.Open(// this.MongoDbConnStr,
                        this._mongoClient,
                        this.MongoDbInstancePrefix,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "启动访问日志库时出错: " + strError;
                        this.WriteErrorLog(strError);
                    }

#if LOG_INFO
                    this.WriteErrorLog("INFO: Open ChargingOperDatabase");
#endif
                    nRet = this.ChargingOperDatabase.Open(
                        this._mongoClient,
                        this.MongoDbInstancePrefix,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "启动出纳操作库时出错: " + strError;
                        this.WriteErrorLog(strError);
                    }
                }

                return 0;
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }
        }

        // 激活管理后台任务。一般用于迫使写入cfgdom到xml文件
        public void ActivateManagerThread()
        {
            if (this.defaultManagerThread != null)
                this.defaultManagerThread.Activate();
        }

        // 激活管理后台任务。一般用于迫使立即重新初始化kdbs和vdbs
        public void ActivateManagerThreadForLoad()
        {
            if (this.defaultManagerThread != null)
            {
                this.defaultManagerThread.ClearRetryDelay();
                this.defaultManagerThread.Activate();
            }
        }



#if NO
        // 2007/7/11 
        int LoadWebuiCfgDom(out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.m_strWebuiFileName) == true)
            {
                strError = "m_strWebuiFileName尚未初始化，因此无法装载webui.xml配置文件到DOM";
                return -1;
            }

            XmlDocument webuidom = new XmlDocument();
            try
            {
                webuidom.Load(this.m_strWebuiFileName);
            }
            catch (FileNotFoundException)
            {
                /*
                strError = "file '" + strWebUiFileName + "' not found ...";
                return -1;
                 * */
                webuidom.LoadXml("<root/>");
            }
            catch (Exception ex)
            {
                strError = "装载配置文件-- '" + this.m_strWebuiFileName + "'时发生错误，原因：" + ex.Message;
                // app.WriteErrorLog(strError);
                return -1;
            }

            this.WebUiDom = webuidom;
            return 0;
        }
#endif

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

        void MessageCenter_VerifyAccount(object sender, VerifyAccountEventArgs e)
        {
            string strError = "";

            if (e.Name == "public")
            {
                e.Exist = false;
                e.Error = true;
                e.ErrorInfo = "系统禁止对 public 用户发消息。";
                return;
            }

            RmsChannel channel = e.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                e.Exist = false;
                e.Error = true;
                e.ErrorInfo = "get channel error";
                return;
            }

            // 检查读者账号是否存在
            // return:
            //      -1  error
            //      0   不存在
            //      1   存在
            //      >1  多于一个
            int nRet = VerifyReaderAccount(channel, // e.Channels,
                e.Name,
                out strError);
            if (nRet == -1 || nRet > 1)
            {
                e.Exist = false;
                e.Error = true;
                e.ErrorInfo = strError;
                return;
            }
            if (nRet == 1)
            {
                e.Exist = true;
                return;
            }

            // 检查工作人员账号

            /*
            if (e.Name == "public")
            {
            }*/

            // 从library.xml文件定义 获得一个帐户的信息
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = this.GetAccount(e.Name,
                out Account account,
                out strError);
            if (nRet == -1)
            {
                e.Exist = false;
                e.Error = true;
                e.ErrorInfo = strError;
                return;
            }
            if (nRet == 0)
            {
                e.Exist = false;
                e.Error = false;
                e.ErrorInfo = "用户名 '" + e.Name + "' 不存在。";
                return;
            }

            e.Exist = true;
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
            virtual_watcher.Changed -= new FileSystemEventHandler(virtual_watcher_Changed);
            virtual_watcher.Changed += new FileSystemEventHandler(virtual_watcher_Changed);

            // Begin watching.
            virtual_watcher.EnableRaisingEvents = true;

        }


        void virtual_watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string strError = "*** 虚拟目录内发生改变: name: " + e.Name.ToString()
                + "; changetype: " + e.ChangeType.ToString()
                + "; fullpath: " + e.FullPath.ToString();
            this.WriteErrorLog(strError);
        }

#endif

        // 监视library.xml文件变化
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
            watcher.Changed -= new FileSystemEventHandler(watcher_Changed);
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

        }

        void EndWather()
        {
            if (this.watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= new FileSystemEventHandler(watcher_Changed);
                this.watcher.Dispose();
                this.watcher = null;
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Changed) != WatcherChangeTypes.Changed)
                return;

            int nRet = 0;

            // this.WriteErrorLog("file1='"+this.m_strFileName+"' file2='" + e.FullPath + "'");
            if (PathUtil.IsEqual(this.m_strFileName, e.FullPath) == true)
            {
                // string strError = "";

                // 稍微延时一下，避免很快地重装、正好和 尚在改写library.xml文件的的进程发生冲突
                Thread.Sleep(500);

                this.WriteErrorLog("watcher 触发了 LoadCfg() ...");

                nRet = this.LoadCfg(
                    true,
                    this.DataDir,
                    this.HostDir,
                    out string strError);
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

            nRet = e.FullPath.IndexOf(".fltx");
            if (nRet != -1)
            {
                this.Filters.ClearFilter(e.FullPath);
            }
        }

        // 读入<readerdbgroup>相关配置
        // return:
        //      <readerdbgroup>元素下<database>元素的个数。如果==0，表示配置不正常
        int LoadReaderDbGroupParam(XmlDocument dom)
        {
            this.ReaderDbs = new List<ReaderDbCfg>();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//readerdbgroup/database");

            if (nodes.Count == 0)
                return 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                ReaderDbCfg item = new ReaderDbCfg();

                item.DbName = DomUtil.GetAttr(node, "name");

                int nRet = DomUtil.GetBooleanParam(node,
                    "inCirculation",
                    true,
                    out bool bValue,
                    out string strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("元素<//readerdbgroup/database>属性inCirculation读入时发生错误: " + strError);
                    bValue = true;
                }

                item.InCirculation = bValue;

                item.LibraryCode = DomUtil.GetAttr(node, "libraryCode");

                this.ReaderDbs.Add(item);
            }

            return nodes.Count;
        }

        // 写入<readerdbgroup>相关配置信息
        void WriteReaderDbGroupParam(XmlTextWriter writer)
        {
            writer.WriteStartElement("readerdbgroup");
            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                writer.WriteStartElement("database");

                writer.WriteAttributeString("name", this.ReaderDbs[i].DbName);

                // 2008/6/3 
                writer.WriteAttributeString("inCirculation", this.ReaderDbs[i].InCirculation == true ? "true" : "false");

                // 2012/9/7
                writer.WriteAttributeString("libraryCode", this.ReaderDbs[i].LibraryCode);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        // 读入<itemdbgroup>相关配置
        // return:
        //      <itemdbgroup>元素下<database>元素的个数。如果==0，表示配置不正常
        int LoadItemDbGroupParam(XmlDocument dom,
            out string strError)
        {
            strError = "";

            /*
            if (this.GlobalCfgDom == null)
            {
                strError = "LoadItemDbGroupParam()失败, 因为GlobalCfgDom尚未初始化";
                return -1;
            }*/

            this.ItemDbs = new List<ItemDbCfg>();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//itemdbgroup/database");

            if (nodes.Count == 0)
                return 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                ItemDbCfg item = new ItemDbCfg();

                item.DbName = DomUtil.GetAttr(node, "name");

                item.BiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
                if (String.IsNullOrEmpty(item.BiblioDbName) == true)
                {
                    strError = "<itemdbgroup>中，实体库 '" + item.DbName + "' <database>元素中biblioDbName属性没有配置";
                    return -1;
                }

                item.BiblioDbSyntax = DomUtil.GetAttr(node, "syntax");

                item.IssueDbName = DomUtil.GetAttr(node, "issueDbName");

                item.OrderDbName = DomUtil.GetAttr(node, "orderDbName");

                item.CommentDbName = DomUtil.GetAttr(node, "commentDbName");

                item.UnionCatalogStyle = DomUtil.GetAttr(node, "unionCatalogStyle");

                item.Replication = DomUtil.GetAttr(node, "replication");

                {
                    Hashtable table = StringUtil.ParseParameters(item.Replication);
                    item.ReplicationServer = (string)table["server"];
                    item.ReplicationDbName = (string)table["dbname"];
                }


                // 2008/6/4 
                bool bValue = true;
                int nRet = DomUtil.GetBooleanParam(node,
                    "inCirculation",
                    true,
                    out bValue,
                    out strError);
                if (nRet == -1)
                {
                    strError = "元素<//itemdbgroup/database>属性inCirculation读入时发生错误: " + strError;
                    return -1;
                }

                item.InCirculation = bValue;

                item.Role = DomUtil.GetAttr(node, "role");

                this.ItemDbs.Add(item);
            }

            return nodes.Count;
        }

        // 写入<itemdbgroup>相关配置信息
        void WriteItemDbGroupParam(XmlTextWriter writer)
        {
            writer.WriteStartElement("itemdbgroup");
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];

                writer.WriteStartElement("database");

                writer.WriteAttributeString("name", cfg.DbName);

                writer.WriteAttributeString("biblioDbName", cfg.BiblioDbName);  // 2005/5/25 

                // 以下两行缺，成为BUG
                if (String.IsNullOrEmpty(cfg.IssueDbName) == false)
                    writer.WriteAttributeString("issueDbName", cfg.IssueDbName);  // 2007/10/22 
                if (String.IsNullOrEmpty(cfg.BiblioDbSyntax) == false)
                    writer.WriteAttributeString("syntax", cfg.BiblioDbSyntax);   // 2007/10/22 

                if (String.IsNullOrEmpty(cfg.OrderDbName) == false)
                    writer.WriteAttributeString("orderDbName", cfg.OrderDbName);  // 2007/11/27 

                if (String.IsNullOrEmpty(cfg.CommentDbName) == false)
                    writer.WriteAttributeString("commentDbName", cfg.CommentDbName);  // 2008/12/8 

                if (String.IsNullOrEmpty(cfg.UnionCatalogStyle) == false)
                    writer.WriteAttributeString("unionCatalogStyle", cfg.UnionCatalogStyle);  // 2007/12/15 

                // 2008/6/4 
                writer.WriteAttributeString("inCirculation", cfg.InCirculation == true ? "true" : "false");

                if (String.IsNullOrEmpty(cfg.Role) == false)
                    writer.WriteAttributeString("role", cfg.Role);  // 2009/10/23 

                if (String.IsNullOrEmpty(cfg.Replication) == false)
                    writer.WriteAttributeString("replication", cfg.Replication);

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /*
        void SaveReaderDbGrouParam(XmlDocument dom)
        {
            XmlNode node = dom.DocumentElement.SelectSingleNode("//readerdbgroup");
            if (node == null)
            {
                node = (XmlNode)dom.CreateElement("readerdbgroup");
                node = dom.DocumentElement.AppendChild(node);
            }

            node.InnerXml = ""; // 删除原有全部子元素

            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                XmlElement newnode = dom.CreateElement("database");
                node.AppendChild(newnode);

                newnode.SetAttribute("name", this.ReaderDbs[i].DbName);
            }
        }
         */

        // 检查全局配置参数是否基本正常
        public int Verify(out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();
            if (this.WsUrl == "")
                errors.Add("root/@wsurl 属性未定义");

            if (this.ManagerUserName == "")
                errors.Add("root/@managerusername 属性未定义");

            // 2018/10/26
            // 检查 unique 元素是否多于一个
            //XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("unique");
            //if (nodes.Count > 1)
            //    errors.Add("根元素下 unique 元素定义超过一个。请删除多余的，只保留一个即可");

            // 2018/11/5
            // 检查 unique 元素是否多于一个
            // 检查 maps_856u 元素是否多于一个
            string[] unique_containers = new string[]{
                            "rightsTable",
                            "locationTypes",
                            "accounts",
                            "browseformats",
                            "foregift",
                            "virtualDatabases",
                            "valueTables",
                            "calendars",
                            "traceDTLP",
                            "zhengyuan",
                            "dkyw",
                            "patronReplication",
                            "clientFineInterface",
                            "yczb",
                            "script",
                            "mailTemplates",
                            "smtpServer",
                            "externalMessageInterface",
                            "zhongcihao",
                            "callNumber",
                            "monitors",
                            "dup",
                            "unique",
                            "utilDb",
                            "libraryInfo",
                            "login",
                            "circulation",
                            "channel",
                            "cataloging",
                            "serverReplication",
                            "authdbgroup",
                            "maps_856u",
                            "globalResults",    // 2018/12/3
                            "rfid", // 2019/1/11
                            "barcodeValidation", // 2019/5/31
                        };

            foreach (string element_name in unique_containers)
            {
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(element_name);
                if (nodes.Count > 1)
                    errors.Add("根元素下 " + element_name + " 元素定义超过一个。请删除多余的，只保留一个即可");
            }

            /*
            // 对 maps_856u 的特殊检查
            // 序列号允许的功能如果不含有 maps856u，则 maps_856u/item@type 属性值不允许重复
            if (StringUtil.IsInList("maps856u", this.Function) == false)
            {
                List<string> types = new List<string>();
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("maps_856u/item");
                foreach (XmlElement item in nodes)
                {
                    string type = item.GetAttribute("type");
                    if (types.IndexOf(type) != -1)
                        errors.Add("序列号未许可 maps856u 功能的情况下，maps_856u/item@type 属性值不允许重复(出现了重复值'" + type + "')");

                    types.Add(type);
                }
            }
            */

            // 2019/7/24
            // 检查两个保留 account 元素的 type 属性
            {
                XmlNodeList accounts = this.LibraryCfgDom.DocumentElement.SelectNodes("accounts/account[@name='reader' or @name='public']");
                foreach (XmlElement account in accounts)
                {
                    string type = account.GetAttribute("type");
                    if (string.IsNullOrEmpty(type))
                    {
                        // errors.Add($"name属性值为 '{account.GetAttribute("name")}' 的 account 元素，其 type 属性值('')错误，必须为 'reader'");

                        // 强制修改
                        account.SetAttribute("type", "reader");
                        this.Changed = true;
                    }
                }
            }

            // 检查图书馆名
            string libraryName = DomUtil.GetElementText(this.LibraryCfgDom.DocumentElement, "libraryInfo/libraryName");
            if (string.IsNullOrEmpty(libraryName) == false && libraryName.IndexOfAny(new char[] { '/', '\\' }) != -1)
                errors.Add($"libraryInfo/libraryName 元素中的图书馆名 '{libraryName}' 不合法");

            // 2020/9/10
            // 检查 circulation/@itemAdditionalFields 中的元素名是否和册记录核心元素名重复了
            if (this.ItemAdditionalFields != null)
            {
                foreach (var name in this.ItemAdditionalFields)
                {
                    if (Array.IndexOf(core_entity_element_names, name) != -1)
                        errors.Add($"circulation/@itemAdditionalFields 属性中的名字 '{name}' 和册记录基本元素名字冲突了");
                }
            }

            // 2020/7/1
            if (StringUtil.IsInList("skipVirusCheck", this.Function) == false)
            {
                // 2019/11/27
                if (DetectVirus.DetectXXX() || DetectVirus.DetectGuanjia())
                    errors.Add("dp2library 被木马软件干扰，无法启动");
            }

            if (errors.Count > 0)
            {
                strError = "library.xml 发现下列配置错误: " + StringUtil.MakePathList(errors, "; ");
                strError = "LibraryService 初始化过程发生严重错误 [" + strError + "]，当前此服务处于残缺状态，请及时排除故障后重新启动";
                this.WriteErrorLog(strError);
                this.AddHangup("StartingError");
                return -1;
            }

            // 2019/7/24
            if (this.Changed == true)
                this.Flush();
            return 0;
        }

        public void RestartApplication()
        {
            try
            {
                // 往bin目录中写一个临时文件
                using (Stream stream = File.Open(Path.Combine(this.BinDir, "temp.temp"),
                    FileMode.Create))
                {

                }

                // stream.Close();

                this.WriteErrorLog("LibraryService 被重新初始化。");
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("LibraryService 重新初始化时发生错误：" + ExceptionUtil.GetDebugText(ex));
            }
        }

        public void WriteErrorLog(string strText)
        {
            try
            {
                lock (this.LogDir)
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
                // TODO: 要在安装程序中预先创建事件源
                // 代码可以参考 unhandle.txt (在本project中)

                /*
                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists("dp2library"))
                {
                    EventLog.CreateEventSource("dp2library", "DigitalPlatform");
                }*/

                EventLog Log = new EventLog();
                Log.Source = "dp2library";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
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
            Log.Source = "dp2library";
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

        /*
        // 写入系统日志
        public static void WriteWindowsErrorLog(string strText)
        {
            // Create the source, if it does not already exist.
            if (!EventLog.SourceExists("dp2library"))
            {
                EventLog.CreateEventSource("dp2library", "DigitalPlatform");
            }

            EventLog Log = new EventLog();
            Log.Source = "dp2library";
            Log.WriteEntry(strText, EventLogEntryType.Error);

        }
         * */

        public void WriteDebugInfo(string strTitle)
        {
            if (this.DebugMode == false)
                return;
            // 写入一个恒定名字的文件。TODO: 也可以直接写入普通的错误日志文件？
            StreamUtil.WriteText(Path.Combine(this.LogDir, "debug.txt"), "-- " + DateTime.Now.ToString("u") + " " + strTitle + "\r\n");
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

        // 异常：不会抛出异常
        public void Flush()
        {
            try
            {
                this.Save(null, true);
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("Flush()中俘获异常 " + ExceptionUtil.GetDebugText(ex));
            }
        }

        // 保存
        // 其实,进入内存属性的XML片断,可以在this.LibraryCfgDom中删除.最后直接合并保存这个dom即可.
        // parameters:
        //      bFlush  是否为刷新情形？如果是，则不写入错误日志
        public void Save(string strFileName,
            bool bFlush)
        {
            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                if (this.m_bChanged == false)
                {
                    /*
                    // 调试用
                    LibraryApplication.WriteWindowsLog("没有进行Save()，因为m_bChanged==false", EventLogEntryType.Information);
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

                XmlDocument cfg_dom = this.LibraryCfgDom;

                // 写入 XML 文件中途不允许使用和修改 XmlDocument
                _lockLibraryCfgDom.EnterWriteLock();
                try
                {
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

                        if (string.IsNullOrEmpty(this.UID) == false)
                            writer.WriteAttributeString("uid", this.UID);

                        // 2008/6/6 nwe add
                        // <version>
                        {
                            XmlNode node = cfg_dom.DocumentElement.SelectSingleNode("version");
                            if (node != null)
                            {
                                node.WriteTo(writer);
                            }
                        }

                        // 内核参数
                        // 元素<rmsserver>
                        // 属性url/username/password
                        writer.WriteStartElement("rmsserver");
                        writer.WriteAttributeString("url", this.WsUrl);
                        writer.WriteAttributeString("username", this.ManagerUserName);
                        writer.WriteAttributeString("password",
                            Cryptography.Encrypt(this.ManagerPassword, EncryptKey)
                            );
                        writer.WriteEndElement();

                        //2015/10/2
                        // <mongoDB>
                        {
                            XmlNode node = cfg_dom.DocumentElement.SelectSingleNode("mongoDB");
                            if (node != null)
                            {
                                node.WriteTo(writer);
                            }
                        }

                        //2013/11/18
                        // <center>
                        {
                            XmlNode node = cfg_dom.DocumentElement.SelectSingleNode("center");
                            if (node != null)
                            {
                                node.WriteTo(writer);
                            }
                        }

                        // 预约到书
                        // 元素<arrived>
                        // 属性dbname/reserveTimeSpan/outofReservationThreshold/canReserveOnshelf
                        writer.WriteStartElement("arrived");
                        writer.WriteAttributeString("dbname", this.ArrivedDbName);
                        writer.WriteAttributeString("reserveTimeSpan", this.ArrivedReserveTimeSpan);

                        // 2007/11/5 
                        writer.WriteAttributeString("outofReservationThreshold", this.OutofReservationThreshold.ToString());
                        writer.WriteAttributeString("canReserveOnshelf", this.CanReserveOnshelf == true ? "true" : "false");
                        writer.WriteAttributeString("notifyTypes", this.ArrivedNotifyTypes);

                        writer.WriteEndElement();

                        /*
                        // <arrived>
                        node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//arrived");
                        if (node != null)
                        {
                            //writer.WriteRaw(node.OuterXml);
                            node.WriteTo(writer);
                        }*/

                        // -----------
                        // 2007/11/5 
                        // 入馆登记
                        // 元素<passgate>
                        // 属性writeOperLog
                        writer.WriteStartElement("passgate");
                        writer.WriteAttributeString("writeOperLog", this.PassgateWriteToOperLog == true ? "true" : "false");
                        writer.WriteEndElement();

                        // -----------
                        // 2015/7/14
                        // 对象管理
                        // 元素<object>
                        // 属性 writeOperLog
                        writer.WriteStartElement("object");
                        writer.WriteAttributeString("writeGetResOperLog", this.GetObjectWriteToOperLog == true ? "true" : "false");
                        writer.WriteEndElement();

                        // -----------
                        // 2015/11/26
                        // 日志
                        // 元素<log>
                        writer.WriteStartElement("log");
                        writer.WriteAttributeString("accessLogMaxCountPerDay", this.AccessLogMaxCountPerDay.ToString());
                        writer.WriteEndElement();

                        // 消息
                        // 元素<message>
                        // 属性dbname/reserveTimeSpan
                        writer.WriteStartElement("message");
                        writer.WriteAttributeString("dbname", this.MessageDbName);
                        writer.WriteAttributeString("reserveTimeSpan", this.MessageReserveTimeSpan);    // 2007/11/5 
                        if (string.IsNullOrEmpty(this.OutgoingQueue) == false)
                            writer.WriteAttributeString("defaultQueue", this.OutgoingQueue);
                        writer.WriteEndElement();

                        // 拼音
                        // 元素<pinyin>
                        // 属性dbname
                        writer.WriteStartElement("pinyin");
                        writer.WriteAttributeString("dbname", this.PinyinDbName);
                        writer.WriteEndElement();

                        // GCAT
                        // 元素<gcat>
                        // 属性dbname
                        writer.WriteStartElement("gcat");
                        writer.WriteAttributeString("dbname", this.GcatDbName);
                        writer.WriteEndElement();

                        // 词
                        // 元素<word>
                        // 属性dbname
                        writer.WriteStartElement("word");
                        writer.WriteAttributeString("dbname", this.WordDbName);
                        writer.WriteEndElement();

                        /*
                        // 图书馆业务服务器
                        // 元素<libraryserver>
                        // 属性url
                        writer.WriteStartElement("libraryserver");
                        writer.WriteAttributeString("url", this.LibraryServerUrl);
                        writer.WriteEndElement();
                         * */

                        // OPAC服务器
                        // 元素<opacServer>
                        // 属性url
                        writer.WriteStartElement("opacServer");
                        writer.WriteAttributeString("url", this.OpacServerUrl);
                        writer.WriteEndElement();

                        // 违约金
                        // 元素<amerce>
                        // 属性dbname/overdueStyle
                        writer.WriteStartElement("amerce");
                        writer.WriteAttributeString("dbname", this.AmerceDbName);
                        writer.WriteAttributeString("overdueStyle", this.OverdueStyle); // 2007/11/5 
                        writer.WriteEndElement();

                        // 发票
                        // 元素<invoice>
                        // 属性dbname
                        writer.WriteStartElement("invoice");
                        writer.WriteAttributeString("dbname", this.InvoiceDbName);
                        writer.WriteEndElement();

                        WriteReaderDbGroupParam(writer);

                        WriteItemDbGroupParam(writer);

                        // TODO: 把这些语句都写入一个函数
                        // 没有进入内存属性的其他XML片断
                        if (cfg_dom != null)
                        {
                            string[] elements = new string[]{
                            "//rightsTable",       // 0.02以前为rightstable
                            "//locationTypes",  // 0.02以前为locationtypes
                            "accounts",
                            "//browseformats",
                            "//foregift",
                            "//virtualDatabases",
                            "//valueTables",
                            "//calendars",
                            "traceDTLP",
                            "zhengyuan",
                            "dkyw",
                            "patronReplication",  // "//patronReplication", 2018/9/5
                            "//clientFineInterface",
                            "yczb",
                            "script",
                            "mailTemplates",
                            "smtpServer",
                            "externalMessageInterface",
                            "zhongcihao",
                            "callNumber",
                            "monitors",
                            "dup",
                            "unique",
                            "utilDb",
                            "libraryInfo",
                            "login",
                            "circulation",
                            "channel",
                            "cataloging",
                            "serverReplication",
                            "authdbgroup",  // 2018/9/2
                            "maps_856u",    // 2018/10/24
                            "globalResults",    // 2018/12/3
                            "rfid", // 2019/1/11
                            "barcodeValidation", // 2019/5/31
                        };

                            RestoreElements(cfg_dom, writer, elements);
                        }

                        // 时钟
                        writer.WriteElementString("clock", Convert.ToString(this.Clock.Delta));

                        writer.WriteEndElement();

                        writer.WriteEndDocument();
                    }
                    // writer.Close();

                    if (bFlush == false)
                        this.WriteErrorLog("完成 从内存写入 " + strFileName);

                    this.m_bChanged = false;

                    /*
                    // 2017/11/25
                    {
                        if (this.LibraryCfgDom == null)
                            this.LibraryCfgDom = new XmlDocument();
                        this.LibraryCfgDom.Load(strFileName);
                    }
                    */

                    // 2021/7/24
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.Load(strFileName);

                        this._libraryCfgDom = dom;
                    }
                }
                finally
                {
                    _lockLibraryCfgDom.ExitWriteLock();
                }

                if (this.watcher != null)
                {
                    watcher.EnableRaisingEvents = bOldState;
                }

            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }
        }

        static void RestoreElements(
            XmlDocument cfg_dom,
            XmlTextWriter writer,
            string[] elements)
        {
            foreach (string element in elements)
            {
                XmlNode node = cfg_dom.DocumentElement.SelectSingleNode(element);
                if (node != null)
                    node.WriteTo(writer);
            }
        }

#if OLD_CODE
        public void StopAll()
        {
            // 停止所有长操作
            lock (this.StopTable)
            {
                foreach (string key in this.StopTable.Keys)
                {
                    StopState stop = (StopState)this.StopTable[key];
                    if (stop != null)
                        stop.Stop();
                }
            }
        }

        public void StopHead(string strHead)
        {
            // 停止所有key匹配的长操作
            lock (this.StopTable)
            {
                foreach (string key in this.StopTable.Keys)
                {
                    if (StringUtil.HasHead(key, strHead) == false)
                        continue;
                    StopState stop = (StopState)this.StopTable[key];
                    stop.Stop();
                }
            }
        }

        public void Stop(string strName)
        {
            // 停止所有key匹配的长操作
            lock (this.StopTable)
            {
                foreach (string key in this.StopTable.Keys)
                {
                    if (key != strName)
                        continue;
                    StopState stop = (StopState)this.StopTable[key];
                    stop.Stop();
                }
            }
        }

        public StopState BeginLoop(string strTitle)
        {
            lock (this.StopTable)
            {
                StopState stop = (StopState)this.StopTable[strTitle];
                if (stop == null)
                {
                    stop = new StopState();
                    this.StopTable[strTitle] = stop;
                }

                stop.Stopped = false;

                return stop;
            }
        }

        public StopState EndLoop(string strTitle,
            bool bRemoveObject)
        {
            lock (this.StopTable)
            {
                if (this.StopTable.Contains(strTitle) == false)
                    return null;
                StopState stop = (StopState)this.StopTable[strTitle];
                stop.Stopped = true;

                if (bRemoveObject == true)
                    this.StopTable.Remove(strTitle);

                return stop;
            }
        }
#endif

        public CancellationToken AppDownToken
        {
            get
            {
                return _app_down.Token;
            }
        }

        internal CancellationTokenSource _app_down = new CancellationTokenSource();

        public void Close()
        {
            // 切断所有正在请求中的 RmsChannel
            _slowChannelList.Disabled = true;   // 先禁用
            _slowChannelList.Abort();

            _app_down.Cancel();

            this.EndWather();

            //this.HangupReason = LibraryServer.HangupReason.Exit;    // 阻止后继 API 访问

            this.WriteErrorLog("LibraryService 开始下降");
            this.AddHangup("Exit");

            DateTime start = DateTime.Now;
            try
            {
#if OLD_CODE
                // 停止所有长操作
                this.StopAll();
#endif

                // 2014/12/3
                this.Flush();

                if (this.OperLog != null)
                {
                    this.OperLog.Close(true);   // 自动进入小文件模式，对象依然有效
                    // this.OperLog = null; // 对象不要释放，依然起作用
                }

                if (this.Garden != null)
                {
                    // 紧急写入所有统计指标
                    this.Garden.CleanPersons(new TimeSpan(0, 0, 0), this.Statis);
                    this.Garden = null;
                }

                if (this.Statis != null)
                {
                    this.Statis.Close();
                    this.Statis = null;
                }

                /*
                if (this.ArriveMonitor != null)
                    this.ArriveMonitor.Close();
                 * */
                if (this.BatchTasks != null)
                {
                    this.BatchTasks.Close();
                    this.BatchTasks = null;
                }

                // 2019/4/26
                this._physicalFileCache.Dispose();
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("LibraryApplication Close() 捕获异常: " + ExceptionUtil.GetDebugText(ex));
            }

            TimeSpan delta = DateTime.Now - start;
            this.WriteErrorLog("LibraryApplication 被停止。停止操作耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

            this.RemoveAppDownDetectFile();	// 删除检测文件

            disposed = true;
        }

        // 兼容以前用法
        public int InitialVdbs(
    RmsChannelCollection Channels,
    out string strError)
        {
            string strWarning = "";
            int nRet = InitialVdbs(
    Channels,
    out strWarning,
    out strError);
            if (string.IsNullOrEmpty(strWarning) == false)
            {
                strError = strWarning;
                return -1;
            }

            return nRet;
        }

        // 初始化虚拟库集合定义对象
        public int InitialVdbs(
            RmsChannelCollection Channels,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            if (this.vdbs != null)
                return 0;   // 优化

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
                    "virtualDatabases");
                if (root == null)
                {
                    strError = "尚未配置<virtualDatabases>元素";
                    return -1;
                }

                XmlNode biblio_dbs_root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
                    "itemdbgroup");
                /*
                if (root == null)
                {
                    strError = "尚未配置<itemdbgroup>元素";
                    return -1;
                }
                 * */
                this.vdbs = new VirtualDatabaseCollection();
                int nRet = vdbs.Initial(root,
                    Channels,
                    this.WsUrl,
                    biblio_dbs_root,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                {
                    this.vdbs = null;   // 2011/1/29
                    return -1;
                }

                return 0;
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }
        }


        /*
        // 判断一个数据库名是不是合法的实体库名
        public bool IsItemDbName(string strItemDbName)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@name='" + strItemDbName + "']");

            if (node == null)
                return false;

            return true;
        }*/

        // 是否在配置的读者库名之列?
        // 注：参与和不参与流通的读者库都算在列
        public bool IsReaderDbName(string strReaderDbName)
        {
            // 2014/11/6
            if (string.IsNullOrEmpty(strReaderDbName) == true)
                return false;

            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                if (strReaderDbName == this.ReaderDbs[i].DbName)
                    return true;
            }

            // 2012/7/10
            // 可能是其他语言的读者库名
            if (this.kdbs != null)
            {
                for (int i = 0; i < this.ReaderDbs.Count; i++)
                {
                    KernelDbInfo db = this.kdbs.FindDb(this.ReaderDbs[i].DbName);
                    if (db == null)
                        continue;
                    foreach (Caption caption in db.Captions)
                    {
                        if (strReaderDbName == caption.Value)
                            return true;
                    }
                }
            }

            return false;
        }

        // 包装版本
        public bool IsReaderDbName(string strReaderDbName,
    out bool IsInCirculation)
        {
            string strLibraryCode = "";
            return IsReaderDbName(strReaderDbName,
                out IsInCirculation,
                out strLibraryCode);
        }

        // 包装版本
        public bool IsReaderDbName(string strReaderDbName,
    out string strLibraryCode)
        {
            bool IsInCirculation = false;
            return IsReaderDbName(strReaderDbName,
                out IsInCirculation,
                out strLibraryCode);
        }

        // 是否在配置的读者库名之列?
        // 另一版本，返回是否参与流通
        public bool IsReaderDbName(string strReaderDbName,
            out bool IsInCirculation,
            out string strLibraryCode)
        {
            IsInCirculation = false;
            strLibraryCode = "";

            // 2016/11/7
            if (string.IsNullOrEmpty(strReaderDbName))
                return false;

            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                if (strReaderDbName == this.ReaderDbs[i].DbName)
                {
                    IsInCirculation = this.ReaderDbs[i].InCirculation;
                    strLibraryCode = this.ReaderDbs[i].LibraryCode;
                    return true;
                }
            }

            return false;
        }

        // 获得(书目库相关角色)数据库的类型，顺便返回所从属的书目库名
        public string GetDbType(string strDbName,
            out string strBiblioDbName)
        {
            strBiblioDbName = "";

            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                strBiblioDbName = cfg.BiblioDbName;

                if (strDbName == cfg.DbName)
                    return "item";
                if (strDbName == cfg.BiblioDbName)
                    return "biblio";
                if (strDbName == cfg.IssueDbName)
                    return "issue";
                if (strDbName == cfg.OrderDbName)
                    return "order";
                if (strDbName == cfg.CommentDbName)
                    return "comment";
            }

            // 2012/7/10
            // 可能是其他语言的数据库名
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                strBiblioDbName = cfg.BiblioDbName;

                if (IsOtherLangName(strDbName, cfg.DbName) == true)
                    return "item";
                if (IsOtherLangName(strDbName, cfg.BiblioDbName) == true)
                    return "biblio";
                if (IsOtherLangName(strDbName, cfg.IssueDbName) == true)
                    return "issue";
                if (IsOtherLangName(strDbName, cfg.OrderDbName) == true)
                    return "order";
                if (IsOtherLangName(strDbName, cfg.CommentDbName) == true)
                    return "comment";
            }

            strBiblioDbName = "";
            return null;
        }

        // 获得数据库的类型
        public string GetDbType(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strDbName == this.ItemDbs[i].DbName)
                    return "item";
                if (strDbName == this.ItemDbs[i].BiblioDbName)
                    return "biblio";
                if (strDbName == this.ItemDbs[i].IssueDbName)
                    return "issue";
                if (strDbName == this.ItemDbs[i].OrderDbName)
                    return "order";
                if (strDbName == this.ItemDbs[i].CommentDbName)
                    return "comment";
            }

            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                if (strDbName == this.ReaderDbs[i].DbName)
                    return "reader";
            }

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("utilDb/database[@name='" + strDbName + "']");
            if (node != null)
                return "util";

            return null;
        }

        // 2012/7/6
        // 检测是否为其他语言的等同库名
        // parameters:
        //      strDbName   要检测的数据库名
        //      strNeutralDbName    已知的中立语言数据库名
        public bool IsOtherLangName(string strDbName,
            string strNeutralDbName)
        {
            if (this.kdbs == null)
                return false;

            KernelDbInfo db = this.kdbs.FindDb(strNeutralDbName);
            if (db == null)
                return false;

            if (db != null)
            {
                foreach (Caption caption in db.Captions)
                {
                    if (strDbName == caption.Value)
                        return true;
                }
            }

            return false;
        }

        // 是否在配置的实体库名之列?
        public bool IsItemDbName(string strItemDbName)
        {
            // 2008/10/16 
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strItemDbName == this.ItemDbs[i].DbName)
                    return true;

                // 2012/7/6
                // 可能是其他语言的库名
                if (IsOtherLangName(strItemDbName, this.ItemDbs[i].DbName) == true)
                    return true;
            }

            return false;
        }

        // 是否在配置的实体库名之列?
        // 另一版本，返回是否参与流通
        public bool IsItemDbName(string strItemDbName,
            out bool IsInCirculation)
        {
            IsInCirculation = false;

            // 2008/10/16 
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strItemDbName == this.ItemDbs[i].DbName)
                {
                    IsInCirculation = this.ItemDbs[i].InCirculation;
                    return true;
                }

                // 2012/7/6
                // 可能是其他语言的库名
                if (IsOtherLangName(strItemDbName, this.ItemDbs[i].DbName) == true)
                    return true;
            }

            return false;
        }

        // 是否在配置的书目库名之列?
        public ItemDbCfg GetBiblioDbCfg(string strBiblioDbName)
        {
            // 2008/10/16 
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (strBiblioDbName == this.ItemDbs[i].BiblioDbName)
                    return cfg;
            }
            return null;
        }

        // 是否在配置的书目库名之列?
        public ItemDbCfg GetAuthorityDbCfg(string strBiblioDbName)
        {
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            XmlElement database = this.LibraryCfgDom.DocumentElement.SelectSingleNode("authdbgroup/database[@name='" + strBiblioDbName + "']") as XmlElement;
            if (database == null)
                return null;
            return new ItemDbCfg
            {
                DbName = database.GetAttribute("name"),
                BiblioDbSyntax = database.GetAttribute("syntax")
            };
        }

        public List<string> GetAuthorityDbNames()
        {
            List<string> results = new List<string>();
            XmlNodeList databases = this.LibraryCfgDom.DocumentElement.SelectNodes("authdbgroup/database");
            foreach (XmlElement database in databases)
            {
                results.Add(database.GetAttribute("name"));
            }
            return results;
        }

        // 是否具有orderWork角色
        public bool IsOrderWorkBiblioDb(string strBiblioDbName)
        {
            // 2008/10/16 
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (strBiblioDbName == this.ItemDbs[i].BiblioDbName)
                    return StringUtil.IsInList("orderWork", cfg.Role);
            }
            return false;
        }

        // 是否在配置的期库名之列?
        public bool IsIssueDbName(string strIssueDbName)
        {
            // 2008/10/16 
            if (String.IsNullOrEmpty(strIssueDbName) == true)
                return false;


            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strIssueDbName == this.ItemDbs[i].IssueDbName)
                    return true;

                // 2012/7/6
                // 可能是其他语言的库名
                if (IsOtherLangName(strIssueDbName, this.ItemDbs[i].IssueDbName) == true)
                    return true;
            }

            return false;
        }

        // 是否在配置的订购库名之列?
        public bool IsOrderDbName(string strOrderDbName)
        {
            // 2008/10/16 
            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return false;


            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strOrderDbName == this.ItemDbs[i].OrderDbName)
                    return true;

                // 2012/7/6
                // 可能是其他语言的库名
                if (IsOtherLangName(strOrderDbName, this.ItemDbs[i].OrderDbName) == true)
                    return true;

            }

            return false;
        }

        // 是否在配置的评注库名之列?
        // 2008/12/8 
        public bool IsCommentDbName(string strCommentDbName)
        {
            if (String.IsNullOrEmpty(strCommentDbName) == true)
                return false;

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                if (strCommentDbName == this.ItemDbs[i].CommentDbName)
                    return true;

                // 2012/7/6
                // 可能是其他语言的库名
                if (IsOtherLangName(strCommentDbName, this.ItemDbs[i].CommentDbName) == true)
                    return true;
            }

            return false;
        }

        // 2012/7/2
        // (通过其他语言的书目库名)获得配置文件中所使用的那个书目库名
        public string GetCfgBiblioDbName(string strBiblioDbName)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node != null)
                return strBiblioDbName;

            // 然后关注别名
            if (this.kdbs == null)
                return null;

            // 2012/7/2
            KernelDbInfo db = this.kdbs.FindDb(strBiblioDbName);
            if (db != null)
            {
                foreach (Caption caption in db.Captions)
                {
                    node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + caption.Value + "']");
                    if (node != null)
                        return caption.Value;
                }
            }

            return null;
        }

        // 判断一个数据库名是不是合法的书目库名
        public bool IsBiblioDbName(string strBiblioDbName)
        {
            if (GetCfgBiblioDbName(strBiblioDbName) == null)
                return false;
            return true;
        }

        // (通过其他语言的书目库名)获得配置文件中所使用的那个规范库名
        public string GetCfgAuthorityDbName(string strAuthorityDbName)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("authdbgroup/database[@name='" + strAuthorityDbName + "']");

            if (node != null)
                return strAuthorityDbName;

            // 然后关注别名
            if (this.kdbs == null)
                return null;

            KernelDbInfo db = this.kdbs.FindDb(strAuthorityDbName);
            if (db != null)
            {
                foreach (Caption caption in db.Captions)
                {
                    node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("authdbgroup/database[@name='" + caption.Value + "']");
                    if (node != null)
                        return caption.Value;
                }
            }

            return null;
        }

        // 判断一个数据库名是不是合法的规范库名
        public bool IsAuthorityDbName(string strAuthorityDbName)
        {
            if (GetCfgAuthorityDbName(strAuthorityDbName) == null)
                return false;
            return true;
        }

#if NO
        // 判断一个数据库名是不是合法的书目库名
        public bool IsBiblioDbName(string strBiblioDbName)
        {
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node != null)
                return true;

            // 然后关注别名
            if (this.kdbs == null)
                return false;

            // 2012/7/2
            KernelDbInfo db = this.kdbs.FindDb(strBiblioDbName);
            foreach (Caption caption in db.Captions)
            {
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + caption.Value + "']");
                if (node != null)
                    return true;
            }

            return false;
        }
#endif

        // TODO: 多语言改造
        // 根据书目下属库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByChildDbName(string strChildDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            string[] names = new string[] { "name", "orderDbName", "issueDbName", "commentDbName" };

            XmlNode node = null;

            foreach (string strName in names)
            {
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@" + strName + "='" + strChildDbName + "']");
                if (node != null)
                    goto FOUND;
            }

            strError = "没有找到名为 '" + strChildDbName + "' 的种下属库";
            return 0;

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据实体库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByItemDbName(string strItemDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            // 2007/5/25 new changed
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@name='" + strItemDbName + "']");

            if (node == null)
            {
                // 2012/7/9
                // 然后关注别名
                if (this.kdbs != null)
                {
                    KernelDbInfo db = this.kdbs.FindDb(strItemDbName);
                    if (db != null)
                    {
                        foreach (Caption caption in db.Captions)
                        {
                            node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@name='" + caption.Value + "']");
                            if (node != null)
                                goto FOUND;
                        }
                    }
                }

                strError = "没有找到名为 '" + strItemDbName + "' 的实体库";
                return 0;
            }

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;

            /*
            if (this.GlobalCfgDom == null)
            {
                strError = "GlobalCfgDom尚未初始化";
                return -1;
            }

            XmlNodeList nodes = this.GlobalCfgDom.DocumentElement.SelectNodes("//dblink");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBiblioDb = DomUtil.GetAttr(node, "bibliodb");

                string strItemDb = DomUtil.GetAttr(node, "itemdb");

                if (strItemDbName == strItemDb)
                {
                    strBiblioDbName = strBiblioDb;
                    return 1;
                }
            }

            return 0;
             * */
        }

        // 根据评注库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2009/10/18 
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByCommentDbName(string strCommentDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@commentDbName='" + strCommentDbName + "']");

            if (node == null)
            {
                // 2012/7/9
                // 然后关注别名
                if (this.kdbs != null)
                {
                    KernelDbInfo db = this.kdbs.FindDb(strCommentDbName);
                    if (db != null)
                    {
                        foreach (Caption caption in db.Captions)
                        {
                            node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@commentDbName='" + caption.Value + "']");
                            if (node != null)
                                goto FOUND;
                        }
                    }
                }

                strError = "没有找到名为 '" + strCommentDbName + "' 的评注库";
                return 0;
            }

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据订购库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2008/8/28 
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByOrderDbName(string strOrderDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@orderDbName='" + strOrderDbName + "']");

            if (node == null)
            {
                // 2012/7/9
                // 然后关注别名
                if (this.kdbs != null)
                {
                    KernelDbInfo db = this.kdbs.FindDb(strOrderDbName);
                    if (db != null)
                    {
                        foreach (Caption caption in db.Captions)
                        {
                            node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@orderDbName='" + caption.Value + "']");
                            if (node != null)
                                goto FOUND;
                        }
                    }
                }

                strError = "没有找到名为 '" + strOrderDbName + "' 的订购库";
                return 0;
            }

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 根据期库名, 找到对应的书目库名
        // 注意，返回1的时候，strBiblioDbName也有可能为空
        // 2009/2/2 
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBiblioDbNameByIssueDbName(string strIssueDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@issueDbName='" + strIssueDbName + "']");

            if (node == null)
            {
                // 2012/7/9
                // 然后关注别名
                if (this.kdbs != null)
                {
                    KernelDbInfo db = this.kdbs.FindDb(strIssueDbName);
                    if (db != null)
                    {
                        foreach (Caption caption in db.Captions)
                        {
                            node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@issueDbName='" + caption.Value + "']");
                            if (node != null)
                                goto FOUND;
                        }
                    }
                }

                strError = "没有找到名为 '" + strIssueDbName + "' 的期库";
                return 0;
            }

        FOUND:
            strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
            return 1;
        }

        // 获得荐购存储库名列表
        // 所谓荐购存储库，就是用来存储读者推荐的新书目记录的目标库
        public List<string> GetOrderRecommendStoreDbNames()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                if (StringUtil.IsInList("orderRecommendStore", cfg.Role) == true)
                    results.Add(cfg.BiblioDbName);
            }
            return results;
        }

        // 根据书目库名, 找到对应的实体库名
        // 注：返回1的时候strItemDbName依然可能为空。1只是表示找到了书目库定义，但是不确保有实体库定义
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到(书目库定义，但是不确保实体库存在)
        public int GetItemDbName(string strBiblioDbName,
            out string strItemDbName,
            out string strError)
        {
            strError = "";
            strItemDbName = "";

            // 2007/5/25 new changed
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node == null)
            {
                // 如果没有找到，则找<caption>
                VirtualDatabase vdb = this.vdbs[strBiblioDbName];
                if (vdb == null)
                    return 0;

                strBiblioDbName = vdb.GetName("zh");
                if (String.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "数据库 " + vdb.GetName(null) + " 居然没有 zh 语言的名字";
                    return -1;
                }

                // 再次获得
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

                if (node == null)
                    return 0;
            }
            strItemDbName = DomUtil.GetAttr(node, "name");
            return 1;

            /*
            if (this.GlobalCfgDom == null)
            {
                strError = "GlobalCfgDom尚未初始化";
                return -1;
            }

            XmlNodeList nodes = this.GlobalCfgDom.DocumentElement.SelectNodes("//dblink");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBiblioDb = DomUtil.GetAttr(node, "bibliodb");

                string strItemDb = DomUtil.GetAttr(node, "itemdb");

                if (strBiblioDbName == strBiblioDb)
                {
                    strItemDbName = strItemDb;
                    return 1;
                }
            }
            return 0;
             * */

        }

        // 根据书目库名, 找到对应的期库名
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetIssueDbName(string strBiblioDbName,
            out string strIssueDbName,
            out string strError)
        {
            strError = "";
            strIssueDbName = "";

            // 2007/5/25 new changed
            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node == null)
            {
                // 如果没有找到，则找<caption>
                VirtualDatabase vdb = this.vdbs[strBiblioDbName];
                if (vdb == null)
                    return 0;

                strBiblioDbName = vdb.GetName("zh");
                if (String.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "数据库 " + vdb.GetName(null) + " 居然没有 zh 语言的名字";
                    return -1;
                }

                // 再次获得
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

                if (node == null)
                    return 0;
            }

            strIssueDbName = DomUtil.GetAttr(node, "issueDbName");
            return 1;   // 注意有时虽然找到了书目库，但是issueDbName属性缺省或者为空
        }

        // 根据书目库名, 找到对应的订购库名
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetOrderDbName(string strBiblioDbName,
            out string strOrderDbName,
            out string strError)
        {
            strError = "";
            strOrderDbName = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node == null)
            {
                // 如果没有找到，则找<caption>
                VirtualDatabase vdb = this.vdbs[strBiblioDbName];
                if (vdb == null)
                    return 0;

                strBiblioDbName = vdb.GetName("zh");
                if (String.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "数据库 " + vdb.GetName(null) + " 居然没有 zh 语言的名字";
                    return -1;
                }

                // 再次获得
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

                if (node == null)
                    return 0;
            }

            strOrderDbName = DomUtil.GetAttr(node, "orderDbName");
            return 1;   // 注意有时虽然找到了书目库，但是orderDbName属性缺省或者为空
        }

        // 根据书目库名, 找到对应的评注库名
        // 2008/12/8
        // return:
        //      -1  出错
        //      0   没有找到(书目库)
        //      1   找到
        public int GetCommentDbName(string strBiblioDbName,
            out string strCommentDbName,
            out string strError)
        {
            strError = "";
            strCommentDbName = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

            if (node == null)
            {
                // 如果没有找到，则找<caption>
                VirtualDatabase vdb = this.vdbs[strBiblioDbName];
                if (vdb == null)
                    return 0;

                strBiblioDbName = vdb.GetName("zh");
                if (String.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "数据库 " + vdb.GetName(null) + " 居然没有 zh 语言的名字";
                    return -1;
                }

                // 再次获得
                node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//itemdbgroup/database[@biblioDbName='" + strBiblioDbName + "']");

                if (node == null)
                    return 0;
            }
            strCommentDbName = DomUtil.GetAttr(node, "commentDbName");
            return 1;   // 注意有时虽然找到了书目库，但是commentDbName属性缺省或者为空
        }

        // 在未指定语言的情况下获得全部<caption>名
        public static List<string> GetAllNames(XmlNode parent)
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = parent.SelectNodes("caption");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(nodes[i].InnerText);
            }

            return results;
        }

        static string m_strKernelBrowseFomatsXml =
            "<formats> "
            + "<format name='browse' type='kernel'>"
            + "    <caption lang='zh-cn'>浏览</caption>"
            + "    <caption lang='en'>Browse</caption>"
            + "</format>"
            + "<format name='MARC' type='kernel'>"
            + "    <caption lang='zh-cn'>MARC</caption>"
            + "    <caption lang='en'>MARC</caption>"
            + "</format>"
            + "</formats>";

        // 2011/1/2
        // 是否为内置格式名
        // paramters:
        //      strNeutralName  语言中立的名字。例如 browse / MARC。大小写不敏感
        public static bool IsKernelFormatName(string strName,
            string strNeutralName)
        {
            if (strName.ToLower() == strNeutralName.ToLower())
                return true;

            // 先从内置的格式里面找
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(m_strKernelBrowseFomatsXml);

            XmlNodeList format_nodes = dom.DocumentElement.SelectNodes("format");
            for (int j = 0; j < format_nodes.Count; j++)
            {
                XmlNode node = format_nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strName) == -1)
                    continue;

                if (DomUtil.GetAttr(node, "name").ToLower() == strNeutralName.ToLower())
                    return true;
            }

            return false;
        }

        // 2011/1/2
        // 获得特定语言的格式名
        // 包括内置的格式
        public string GetBrowseFormatName(string strName,
            string strLang)
        {
            // 先从内置的格式里面找
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(m_strKernelBrowseFomatsXml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("format");
            string strFormat = GetBrowseFormatName(
                nodes,
                strName,
                strLang);
            if (String.IsNullOrEmpty(strFormat) == false)
                return strFormat;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                // string strError = "<browseformats>元素尚未配置...";
                // TODO: 抛出异常?
                return null;
            }

            // 然后从用户定义的格式里面找
            nodes = root.SelectNodes("database/format");
            return GetBrowseFormatName(
                nodes,
                strName,
                strLang);
        }

        // 2011/1/2
        static string GetBrowseFormatName(
            XmlNodeList format_nodes,
            string strName,
            string strLang)
        {

            for (int j = 0; j < format_nodes.Count; j++)
            {
                XmlNode node = format_nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strName) == -1)
                    continue;

                string strFormatName = DomUtil.GetCaption(strLang, node);
                if (String.IsNullOrEmpty(strFormatName) == false)
                    return strFormatName;
            }

            return null;    // not found
        }

#if NO
        // 获得特定语言的格式名
        public string GetBrowseFormatName(
            string strName,
            string strLang)
        {
            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                // string strError = "<browseformats>元素尚未配置...";
                // TODO: 抛出异常?
                return null;
            }

            XmlNodeList dbnodes = root.SelectNodes("database");
            for (int i = 0; i < dbnodes.Count; i++)
            {
                XmlNode nodeDatabase = dbnodes[i];

                string strDbName = DomUtil.GetAttr(nodeDatabase, "name");


                XmlNodeList nodes = nodeDatabase.SelectNodes("format");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];

                    List<string> captions = GetAllNames(node);
                    if (captions.IndexOf(strName) == -1)
                        continue;

                    string strFormatName = DomUtil.GetCaption(strLang, node);
                    if (String.IsNullOrEmpty(strFormatName) == false)
                        return strFormatName;
                }
            }

            return null;    // not found
        }
#endif

        // 获得一些数据库的全部浏览格式配置信息
        // parameters:
        //      dbnames 要列出哪些数据库的浏览格式？如果==null, 则表示列出全部可能的格式名
        // return:
        //      -1  出错
        //      >=0 formatname个数
        public int GetBrowseFormatNames(
            string strLang,
            List<string> dbnames,
            out List<string> formatnames,
            out string strError)
        {
            strError = "";
            formatnames = new List<string>();

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                strError = "<browseformats>元素尚未配置...";
                return -1;
            }

            XmlNodeList dbnodes = root.SelectNodes("database");
            for (int i = 0; i < dbnodes.Count; i++)
            {
                XmlNode nodeDatabase = dbnodes[i];

                string strDbName = DomUtil.GetAttr(nodeDatabase, "name");

                // dbnames如果==null, 则表示列出全部可能的格式名
                if (dbnames != null)
                {
                    if (dbnames.IndexOf(strDbName) == -1)
                        continue;
                }

                XmlNodeList nodes = nodeDatabase.SelectNodes("format");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];

                    string strFormatName = DomUtil.GetCaption(strLang, node);
                    if (String.IsNullOrEmpty(strFormatName) == true)
                        strFormatName = DomUtil.GetAttr(node, "name");

                    /*
                    if (String.IsNullOrEmpty(strFormatName) == true)
                    {
                        strError = "格式配置片断 '" + node.OuterXml + "' 格式不正确...";
                        return -1;
                    }*/

                    if (formatnames.IndexOf(strFormatName) == -1)
                        formatnames.Add(strFormatName);
                }

            }

            // 2011/1/2
            // 从内置的格式里面找
            // TODO: 对一些根本不是MARC格式的数据库，排除"MARC"格式名
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(m_strKernelBrowseFomatsXml);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("format");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];

                    string strFormatName = DomUtil.GetCaption(strLang, node);
                    if (String.IsNullOrEmpty(strFormatName) == true)
                        strFormatName = DomUtil.GetAttr(node, "name");

                    if (formatnames.IndexOf(strFormatName) == -1)
                        formatnames.Add(strFormatName);
                }
            }

            return formatnames.Count;
        }

        // 获得一个数据库的全部浏览格式配置信息
        // return:
        //      -1  出错
        //      0   没有配置。具体原因在strError中
        //      >=1 format个数
        public int GetBrowseFormats(string strDbName,
            out List<BrowseFormat> formats,
            out string strError)
        {
            strError = "";
            formats = null;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
            if (root == null)
            {
                strError = "<browseformats>元素尚未配置...";
                return -1;
            }

            XmlNode node = root.SelectSingleNode("database[@name='" + strDbName + "']");
            if (node == null)
            {
                strError = "针对数据库 '" + strDbName + "' 没有在<browseformats>下配置<database>参数";
                return 0;
            }

            formats = new List<BrowseFormat>();

            XmlNodeList nodes = node.SelectNodes("format");
            for (int i = 0; i < nodes.Count; i++)
            {
                node = nodes[i];
                BrowseFormat format = new BrowseFormat();
                format.Name = DomUtil.GetAttr(node, "name");
                format.Type = DomUtil.GetAttr(node, "type");
                format.ScriptFileName = DomUtil.GetAttr(node, "scriptfile");
                formats.Add(format);
            }

            if (nodes.Count == 0)
            {
                strError = "数据库 '" + strDbName + "' 在<browseformats>下的<database>元素下，一个<format>元素也未配置。";
            }

            return nodes.Count;
        }

        // 获得一个数据库的一个浏览格式配置信息
        // parameters:
        //      strDbName   "zh"语言的数据库名。也就是<browseformats>下<database>元素的name属性内的数据库名。
        //      strFormatName   界面上选定的格式名。注意，不一定是正好属于this.Lang语言的
        // return:
        //      0   没有配置
        //      1   成功
        public int GetBrowseFormat(string strDbName,
            string strFormatName,
            out BrowseFormat format,
            out string strError)
        {
            strError = "";
            format = null;

            // 先从全部<format>元素下面的全部<caption>中找
            XmlNode nodeDatabase = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
                "browseformats/database[@name='" + strDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "数据库名 '" + strDbName + "' 在<browseformats>元素下没有找到匹配的<database>元素";
                return -1;
            }

            XmlNode nodeFormat = null;

            XmlNodeList nodes = nodeDatabase.SelectNodes("format");
            for (int j = 0; j < nodes.Count; j++)
            {
                XmlNode node = nodes[j];

                List<string> captions = GetAllNames(node);
                if (captions.IndexOf(strFormatName) != -1)
                {
                    nodeFormat = node;
                    break;
                }
            }

            // 再从<format>元素的name属性中找
            if (nodeFormat == null)
            {
                nodeFormat = nodeDatabase.SelectSingleNode(
                    "format[@name='" + strFormatName + "']");
                if (nodeFormat == null)
                {
                    return 0;
                }
            }

            format = new BrowseFormat();
            format.Name = DomUtil.GetAttr(nodeFormat, "name");
            format.Type = DomUtil.GetAttr(nodeFormat, "type");
            format.ScriptFileName = DomUtil.GetAttr(nodeFormat, "scriptfile");

            return 1;
        }

        // 从library.xml文件定义 获得一个帐户的信息
        // TODO: 多文种提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetAccount(string strUserID,
            out Account account,
            out string strError)
        {
            strError = "";
            account = null;

            // 2015/9/9
            if (this.LibraryCfgDom == null || LibraryCfgDom.DocumentElement == null)
            {
                strError = "LibraryCfgDom 尚未初始化。请检查 dp2library 错误日志";
                return -1;
            }

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts");
            if (root == null)
            {
                strError = "<accounts>元素尚未配置...";
                return -1;
            }

            XmlElement node = root.SelectSingleNode("account[@name='" + strUserID + "']") as XmlElement;
            if (node == null)
            {
                strError = "用户 '" + strUserID + "' 不存在 (5)";
                return 0;
            }

            account = new Account();
            account._xmlNode = node;
            account.LoginName = node.GetAttribute("name");
            account.UserID = node.GetAttribute("name");

            try
            {
                /*
                string strText = "";
                strText = node.GetAttribute("password");
                if (String.IsNullOrEmpty(strText) == true)
                    account.Password = "";
                else
                {
                    // 现在的做法，取出密码的 hashed 字符串
                    account.Password = strText;
                }
                */
                // 2021/6/29
                string value = GetPasswordValue(node as XmlElement, out string type);
                if (string.IsNullOrEmpty(value))
                    account.Password = "";
                else
                    account.Password = value;

                // 2021/8/29
                account.PasswordType = type;
            }
            catch
            {
                strError = "用户名为 '" + strUserID + "' 的<account> password参数值错误";
                return -1;
            }

            // 2021/7/3
            account.PasswordExpire = GetPasswordExpire(node as XmlElement);

            account.Type = DomUtil.GetAttr(node, "type");
            account.Rights = DomUtil.GetAttr(node, "rights");
            account.AccountLibraryCode = DomUtil.GetAttr(node, "libraryCode");

            account.Access = DomUtil.GetAttr(node, "access");
            account.RmsUserName = DomUtil.GetAttr(node, "rmsUserName");

            // 2016/10/26
            account.Binding = node.GetAttribute("binding");

            try
            {
                string strText = DomUtil.GetAttr(node, "rmsPassword");
                if (String.IsNullOrEmpty(strText) == true)
                    account.RmsPassword = "";
                else
                {
                    account.RmsPassword = Cryptography.Decrypt(
                              strText,
                              EncryptKey);
                }
            }
            catch
            {
                strError = "用户名为 '" + strUserID + "' 的<account> rmsPassword参数值错误";
                return -1;
            }

            return 1;
        }

        // TODO：判断strItemBarcode是否为空
        // 获得预约到书队列记录
        // parameters:
        //      strItemBarcodeParam  册条码号。可以使用 @itemRefID: 前缀表示册参考ID。
        //                          @notifyID: 是通知记录本身的参考ID
        //                          @patronRefID: 是读者记录的参考ID
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetArrivedQueueRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strItemBarcodeParam,
            out string strXml,
            out byte[] timestamp,
            out string strOutputPath,
            out string strError)
        {
            strOutputPath = "";
            strXml = "";
            strError = "";
            timestamp = null;

            // 2016/12/4
            // 兼容以前的用法。以前曾经有一段 @refID: 表示册参考ID
            strItemBarcodeParam = strItemBarcodeParam.Replace("@refID:", "@itemRefID:");

            LibraryApplication app = this;
            string strFrom = "册条码";

            // 注：旧的，也就是 2015/5/7 以前的 预约到书队列库里面并没有 册参考ID 检索点，所以直接用带着 @refID 前缀的字符串进行检索即可。
            // 等队列库普遍刷新检索点以后，改为使用下面一段代码
            if (this.ArrivedDbKeysContainsRefIDKey() == true)
            {
                string strHead = "@itemRefID:";

                if (StringUtil.HasHead(strItemBarcodeParam, strHead, true) == true)
                {
                    strFrom = "册参考ID";
                    strItemBarcodeParam = strItemBarcodeParam.Substring(strHead.Length).Trim();
                    if (string.IsNullOrEmpty(strItemBarcodeParam) == true)
                    {
                        strError = "参数 strItemBarcodeParam 值中参考ID部分不应为空";
                        return -1;
                    }
                }
            }

            if (strItemBarcodeParam.StartsWith("@notifyID:"))
            {
                strFrom = "参考ID";
                strItemBarcodeParam = strItemBarcodeParam.Substring("@notifyID:".Length);
            }
            else if (strItemBarcodeParam.StartsWith("@patronRefID:"))
            {
                strFrom = "读者参考ID";
                strItemBarcodeParam = strItemBarcodeParam.Substring("@patronRefID:".Length);
            }

            // 构造检索式
            // 2007/4/5 改造 加上了 GetXmlStringSimple()
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(app.ArrivedDbName + ":" + strFrom)
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strItemBarcodeParam)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                1,
                "zh",
                null,
                out List<string> aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 2007/6/27
        // 获得通用记录
        // 本函数可获得超过1条以上的路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetRecXml(
            RmsChannel channel,
            string strQueryXml,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = new List<string>();

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "没有命中记录";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                Math.Min(nMax, lHitCount),
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            Debug.Assert(aPath != null, "");

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 包装后的版本
        public int GetReaderRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out string strError)
        {
            byte[] timestamp = null;

            return GetReaderRecXml(
                // channels,
                channel,
                strBarcode,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strError);
        }

        // TODO: 判断strBorrowItemBarcode是否为空
        // 通过“所借册条码号”获得读者记录
        // 本函数可获得超过1条以上的路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strBorrowItemBarcode,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = new List<string>();

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            // 构造检索式
            string strQueryXml = "";
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                string strDbName = app.ReaderDbs[i].DbName;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "所借册条码")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBorrowItemBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                    strQueryXml += "<operator value='OR'/>";

                strQueryXml += strOneDbQuery;
            }

            if (app.ReaderDbs.Count > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "所借册条码号 '" + strBorrowItemBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            // List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                Math.Min(nMax, lHitCount),
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            Debug.Assert(aPath != null, "");

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            // byte[] timestamp = null;
            string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#if SLOWLY
        // TODO： 判断strBarcode是否为空
        // 通过读者证条码号获得读者记录
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXml(
            RmsChannelCollection channels,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            timestamp = null;
            strXml = "";
            strError = "";

            LibraryApplication app = this;

            int nInCount = 0;   // 参与流通的读者库个数

            // 构造检索式
            string strQueryXml = "";
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                nInCount++;

                string strDbName = app.ReaderDbs[i].DbName;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "证条码")       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
            }

            if (nInCount == 0)
            {
                strError = "当前尚没有配置读者库";
                return -1;
            }

            if (app.ReaderDbs.Count > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者证条码号 '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;


#if OPTIMIZE_API
            List<RichRecord> records = null;
            lRet = channel.GetRichRecords(
                "default",
                0,
                1,
                "path,xml,timestamp",
                "zh",
                out records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (records == null)
            {
                strError = "records == null";
                goto ERROR1;
            }

            if (records.Count < 1)
            {
                strError = "records.Count < 1";
                goto ERROR1;
            }

            strXml = records[0].Xml;
            timestamp = records[0].baTimestamp;
            strOutputPath = records[0].Path;
#else

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            // byte[] timestamp = null;
            // string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
#endif

            return (int)lHitCount;
        ERROR1:
            return -1;
        }
#endif

        // 2015/9/29
        // 尝试从读者库获得记录。先按照分馆所属读者库尝试检索，若未能命中，再扩大到全部读者库
        // 注: 本函数返回 1，不一定等于所有读者库中只命中了这一条。有可能还有其他重复的记录没有检索在内
        public int TryGetReaderRecXml(
            RmsChannel channel,
            string strBarcode,
            string strLibraryCodeList,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            List<string> recpaths = null;
            int nRet = GetReaderRecXml(
            channel,
            strBarcode,
            1,
            strLibraryCodeList,
            out recpaths,
            out strXml,
            out timestamp,
            out strError);
            if (recpaths != null && recpaths.Count > 0)
                strOutputPath = recpaths[0];
            if (nRet == -1 || nRet > 0
                || string.IsNullOrEmpty(strLibraryCodeList) == true
                || this.ReaderDbs.Count == 1)
                return nRet;

            Debug.Assert(nRet == 0, "");

            // 再尝试检索全部读者库一次
            nRet = GetReaderRecXml(
// channels,
channel,
strBarcode,
1,
"",
out recpaths,
out strXml,
out timestamp,
out strError);
            if (recpaths != null && recpaths.Count > 0)
                strOutputPath = recpaths[0];

            return nRet;
        }

        // 2013/5/23
        // 包装以后的版本
        public int GetReaderRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
    out string strError)
        {
            strOutputPath = "";
            List<string> recpaths = null;
            int nRet = GetReaderRecXml(
            // channels,
            channel,
            strBarcode,
            1,
            "",
            out recpaths,
            out strXml,
            out timestamp,
            out strError);
            if (recpaths != null && recpaths.Count > 0)
                strOutputPath = recpaths[0];

            return nRet;
        }

        // 2012/1/5 改造为PiggyBack检索
        // 2013/5/23 改造为可以返回所有命中的 记录路径
        // TODO： 判断strBarcode是否为空
        // 通过读者证条码号获得读者记录
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strBarcodeParam,
            int nMax,
            string strLibraryCodeList,
            out List<string> recpaths,
            out string strXml,
            // out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            // strOutputPath = "";
            timestamp = null;
            strXml = "";
            strError = "";
            int nRet = 0;

            recpaths = new List<string>();

            LibraryApplication app = this;

            string strBarcode = strBarcodeParam;
            string strHead = "@refID:";
            string strFrom = "证条码";
            if (StringUtil.HasHead(strBarcode, strHead, true) == true)
            {
                strFrom = "参考ID";
                strBarcode = strBarcode.Substring(strHead.Length).Trim();
                if (string.IsNullOrEmpty(strBarcode) == true)
                {
                    strError = "字符串 '" + strBarcodeParam + "' 中 参考ID 部分不应为空";
                    return -1;
                }
            }

            List<string> dbnames = new List<string>();
            // 获得读者库名列表
            // parameters:
            //      strReaderDbNames    库名列表字符串。如果为空，则表示全部读者库
            // return:
            //      -1  出错
            //      >=0 dbnames 中包含的读者库名数量
            nRet = GetDbNameList("",
                strLibraryCodeList,
                out dbnames,
                out strError);
            if (nRet == -1)
                return -1;

            // 构造检索式
            string strQueryXml = "";
            // int nInCount = 0;   // 参与流通的读者库个数
            foreach (string strDbName in dbnames)
            {
                // string strDbName = app.ReaderDbs[i].DbName;

                if (string.IsNullOrEmpty(strDbName) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)  // "证条码"  // TODO: 将来统一修改为“证条码号”     // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                if (String.IsNullOrEmpty(strQueryXml) == false) // i > 0
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;

                // nInCount++;
            }

            if (string.IsNullOrEmpty(strQueryXml) == true /*nInCount == 0*/)
            {
                if (app.ReaderDbs.Count == 0)
                    strError = "当前尚没有配置读者库";
                else
                    strError = "当前没有可以操作的读者库";
                return -1;
            }

            if (dbnames.Count > 0/*nInCount > 0*/)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOuputStyle
                1,
                "zh",
                "id,xml,timestamp",
                out Record[] records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者证条码号 '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }
            Record record = records[0];

            Debug.Assert(record.RecordBody != null, "");

            // 2019/10/10
            if (record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
            {
                strError = record.RecordBody.Result.ErrorString;
                return -1;
            }

            // strOutputPath = records[0].Path;
            if (nMax >= 1)
                recpaths.Add(record.Path);
            strXml = record.RecordBody.Xml;
            timestamp = record.RecordBody.Timestamp;

            // 如果命中结果多于一条，则继续获得第一条以后的各条的path
            if (lHitCount > 1 && nMax > 1)
            {
                // List<string> temp = null;
                lRet = channel.DoGetSearchResult(
                    "default",
                    0,
                    Math.Min(nMax, lHitCount),
                    "zh",
                    null,
                    out recpaths,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Debug.Assert(recpaths != null, "");

                if (recpaths.Count == 0)
                {
                    strError = "DoGetSearchResult recpaths error";
                    goto ERROR1;
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 包装后版本
        // TODO: 判断strDisplayName是否为空
        // 通过读者显示名获得读者记录
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXmlByDisplayName(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strDisplayName,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            return GetReaderRecXmlByFrom(
            // channels,
            channel,
            strDisplayName,
            "显示名",
            out strXml,
            out strOutputPath,
            out timestamp,
            out strError);
        }

        // 包装后的版本
        public int GetReaderRecXmlByFrom(
            // RmsChannelCollection channels,
            RmsChannel channel,
    string strWord,
    string strFrom,
    out string strXml,
    out string strOutputPath,
    out byte[] timestamp,
    out string strError)
        {
            return GetReaderRecXmlByFrom(
    // channels,
    channel,
    null,
    strWord,
    strFrom,
    out strXml,
    out strOutputPath,
    out timestamp,
    out strError);
        }

#if SLOWLY
        // TODO: 判断strWord是否为空
        // 通过特定检索途径获得读者记录
        // parameters:
        //      strReaderDbNames    读者库名列表。如果为空，表示采用当前配置的全部读者库
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXmlByFrom(
            RmsChannelCollection channels,
            string strReaderDbNames,
            string strWord,
            string strFrom,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            timestamp = null;
            strXml = "";
            strError = "";

            LibraryApplication app = this;

            List<string> dbnames = new List<string>();
            if (string.IsNullOrEmpty(strReaderDbNames) == true)
            {
                for (int i = 0; i < app.ReaderDbs.Count; i++)
                {
                    string strDbName = app.ReaderDbs[i].DbName;

                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "当前尚没有配置读者库";
                    return -1;
                }
            }
            else
            {
                dbnames = StringUtil.SplitList(strReaderDbNames);
                StringUtil.RemoveBlank(ref dbnames);

                if (dbnames.Count == 0)
                {
                    strError = "参数strReaderDbNames值 '" + strReaderDbNames + "' 中没有包含有效的读者库名";
                    return -1;
                }
            }

            // 构造检索式
            string strQueryXml = "";
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
            }

            if (dbnames.Count > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者"+strFrom+" '" + strWord + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }
#endif

        // 2013/5/21
        // 包装后的版本
        public int GetReaderRecXmlByFrom(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strReaderDbNames,
            string strWord,
            string strFrom,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            List<string> recpaths = null;
            int nRet = GetReaderRecXmlByFrom(
                // channels,
                channel,
                strReaderDbNames,
                strWord,
                strFrom,
                1,
                "",
                out recpaths,
                out strXml,
                out timestamp,
                out strError);
            if (recpaths != null && recpaths.Count > 0)
                strOutputPath = recpaths[0];

            return nRet;
        }

        // 获得读者库名列表
        // parameters:
        //      strReaderDbNames    库名列表字符串。如果为空，则表示全部读者库
        // return:
        //      -1  出错
        //      >=0 dbnames 中包含的读者库名数量
        int GetDbNameList(string strReaderDbNames,
            string strLibraryCodeList,
            out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            if (this.ReaderDbs == null)
            {
                // 2015/9/9
                strError = "ReaderDbs 尚未初始化";
                return -1;
            }

            if (string.IsNullOrEmpty(strReaderDbNames) == true)
            {
                for (int i = 0; i < this.ReaderDbs.Count; i++)
                {
                    string strDbName = this.ReaderDbs[i].DbName;

                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "当前尚没有配置读者库";
                    return 0;
                }
            }
            else
            {
                dbnames = StringUtil.SplitList(strReaderDbNames);
                StringUtil.RemoveBlank(ref dbnames);

                if (dbnames.Count == 0)
                {
                    strError = "参数 strReaderDbNames 值 '" + strReaderDbNames + "' 中没有包含有效的读者库名";
                    return -1;
                }
            }

            // 过滤
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                List<string> results = new List<string>();
                foreach (string s in dbnames)
                {
                    if (IsCurrentChangeableReaderPath(s + "/?", strLibraryCodeList) == false)
                        continue;
                    results.Add(s);
                }
                dbnames = results;
            }

            return dbnames.Count;
        }

        // 2012/1/6 改造为PiggyBack检索
        // TODO: 判断strWord是否为空
        // 通过特定检索途径获得读者记录
        // parameters:
        //      strReaderDbNames    读者库名列表。如果为空，表示采用当前配置的全部读者库
        //      nMax                希望在 recpaths 中最多返回多少个记录路径
        //      strLibraryCodeList  馆代码列表，仅返回属于这个列表管辖的读者库的记录和路径。如果为空，表示不过滤
        //      recpaths        [out]返回命中的记录路径。如果发生重复，这里会返回多于一个路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetReaderRecXmlByFrom(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strReaderDbNames,
            string strWord,
            string strFrom,
            int nMax,
            string strLibraryCodeList,
            out List<string> recpaths,
            out string strXml,
            // out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            // strOutputPath = "";
            timestamp = null;
            strXml = "";
            strError = "";
            int nRet = 0;

            recpaths = new List<string>();

            LibraryApplication app = this;
#if NO
            List<string> dbnames = new List<string>();
            if (string.IsNullOrEmpty(strReaderDbNames) == true)
            {
                for (int i = 0; i < app.ReaderDbs.Count; i++)
                {
                    string strDbName = app.ReaderDbs[i].DbName;

                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false
                        && IsCurrentChangeableReaderPath(strDbName + "/?",
                            strLibraryCodeList) == false)
                        continue;

                    dbnames.Add(strDbName);
                }

                if (dbnames.Count == 0)
                {
                    strError = "当前尚没有配置读者库";
                    return -1;
                }
            }
            else
            {
                dbnames = StringUtil.SplitList(strReaderDbNames);
                StringUtil.RemoveBlank(ref dbnames);

                if (dbnames.Count == 0)
                {
                    strError = "参数strReaderDbNames值 '" + strReaderDbNames + "' 中没有包含有效的读者库名";
                    return -1;
                }
            }
#endif
            List<string> dbnames = new List<string>();
            // 获得读者库名列表
            // parameters:
            //      strReaderDbNames    库名列表字符串。如果为空，则表示全部读者库
            // return:
            //      -1  出错
            //      >=0 dbnames 中包含的读者库名数量
            nRet = GetDbNameList(strReaderDbNames,
                strLibraryCodeList,
                out dbnames,
                out strError);
            if (nRet == -1)
                return -1;

            if (dbnames.Count == 0)
            {
                if (app.ReaderDbs.Count == 0)
                    strError = "当前尚没有配置读者库";
                else
                    strError = "当前没有可以操作的读者库";
                return -1;
            }

            // 构造检索式
            string strQueryXml = "";
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
            }

            if (dbnames.Count > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            Record[] records = null;
            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOuputStyle
                1,
                "zh",
                "id,xml,timestamp",
                out records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者" + strFrom + " '" + strWord + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }

            Debug.Assert(records[0].RecordBody != null, "");

            // strOutputPath = records[0].Path;
            if (nMax >= 1)
                recpaths.Add(records[0].Path);
            strXml = records[0].RecordBody.Xml;
            timestamp = records[0].RecordBody.Timestamp;

            // 如果命中结果多于一条，则继续获得第一条以后的各条的path
            if (lHitCount > 1 && nMax > 1)
            {
                // List<string> temp = null;
                lRet = channel.DoGetSearchResult(
                    "default",
                    0,
                    Math.Min(nMax, lHitCount),
                    "zh",
                    null,
                    out recpaths,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Debug.Assert(recpaths != null, "");

                if (recpaths.Count == 0)
                {
                    strError = "DoGetSearchResult aPath error";
                    goto ERROR1;
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        /*
        // 正在使用的 读者证条码号查重结果集名
        List<string> m_searchReaderDupResultsetNames = new List<string>();
         * 
        // 获得一个尚未使用的 读者证条码号查重结果集名
        string GetSearchReaderDupResultsetName()
        {
            lock (this.m_searchReaderDupResultsetNames)
            {
                for (int i = 0; ; i++)
                {
                    string strResultSetName = "search_reader_dup_" + i.ToString();

                    int index = this.m_searchReaderDupResultsetNames.IndexOf(strResultSetName);
                    if (index == -1)
                    {
                        this.m_searchReaderDupResultsetNames.Add(strResultSetName);
                        return strResultSetName;
                    }
                }
            }
        }

        // 释放一个 读者证条码号查重结果集 名
        void ReleaseSearchReaderDupResultsetName(string strResultSetName)
        {
            lock (this.m_searchReaderDupResultsetNames)
            {
                this.m_searchReaderDupResultsetNames.Remove(strResultSetName);
            }
        }
         * */

#if REMOVED
        // 根据读者证条码号对读者库进行查重
        // 本函数只负责查重, 并不获得记录体
        // parameters:
        //      strBarcode  读者证条码号
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderRecDup(
            RmsChannel channel,
            string strBarcode,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            Debug.Assert(String.IsNullOrEmpty(strBarcode) == false, "");

            LibraryApplication app = this;

            // 构造检索式
            // 查重要针对全部读者库进行，而不光是当前用户能管辖的库
            string strQueryXml = "";
            int nCount = 0;
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                string strDbName = app.ReaderDbs[i].DbName;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                if (nCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "证条码")       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";
                nCount++;

                strQueryXml += strOneDbQuery;
            }

            if (nCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            string strResultSetName = "search_reader_dup_001";

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者证条码号 '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                strResultSetName,
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // TODO: 判断strDisplayName是否为空
        // 根据显示名对读者库进行查重
        // 本函数只负责查重, 并不获得记录体
        // parameters:
        //      strBarcode  读者证条码号
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderDisplayNameDup(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strDisplayName,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            LibraryApplication app = this;

            Debug.Assert(String.IsNullOrEmpty(strDisplayName) == false, "");

            // 构造检索式
            // 查重要针对全部读者库进行
            string strQueryXml = "";
            int nCount = 0;
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                string strDbName = app.ReaderDbs[i].DbName;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                if (nCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                string strOneDbQuery = "<target list='"
        + StringUtil.GetXmlStringSimple(strDbName + ":" + "显示名")
        + "'><item><word>"
        + StringUtil.GetXmlStringSimple(strDisplayName)
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";
                nCount++;

                strQueryXml += strOneDbQuery;
            }

            if (nCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            string strResultSetName = "search_reader_dup_001";

            // TODO: 两种检索点的结果不会产生重复吧，需要测试验证

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "显示名 '" + strDisplayName + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                strResultSetName,
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#endif

        // 根据读者证条码号对读者库进行查重
        // 本函数只负责查重, 并不获得记录体
        // parameters:
        //      strBarcode  读者证条码号
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderRecDup(
            RmsChannel channel,
            string strBarcode,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            return SearchReaderDup(
                channel,
                strBarcode,
                "证条码",
                nMax,
                out aPath,
                out strError);
        }

        // TODO: 判断strDisplayName是否为空
        // 根据显示名对读者库进行查重
        // 本函数只负责查重, 并不获得记录体
        // parameters:
        //      strBarcode  读者证条码号
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderDisplayNameDup(
            RmsChannel channel,
            string strDisplayName,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            return SearchReaderDup(
    channel,
    strDisplayName,
    "显示名",
    nMax,
    out aPath,
    out strError);
        }

        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderRefIdDup(
            RmsChannel channel,
            string strRefID,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            return SearchReaderDup(
    channel,
    strRefID,
    "参考ID",
    nMax,
    out aPath,
    out strError);
        }

        // parameters:
        //      strFrom 显示名/证条码/参考ID
        public int SearchReaderDup(
    RmsChannel channel,
    string strDisplayName,
    string strFrom,
    int nMax,
    out List<string> aPath,
    out string strError)
        {
            strError = "";
            aPath = new List<string>();

            LibraryApplication app = this;

            Debug.Assert(String.IsNullOrEmpty(strDisplayName) == false, "");

            // 构造检索式
            // 查重要针对全部读者库进行
            string strQueryXml = "";
            int nCount = 0;
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                string strDbName = app.ReaderDbs[i].DbName;

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                if (nCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                string strOneDbQuery = "<target list='"
        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom/*"显示名"*/)
        + "'><item><word>"
        + StringUtil.GetXmlStringSimple(strDisplayName)
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";
                nCount++;

                strQueryXml += strOneDbQuery;
            }

            if (nCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            string strResultSetName = "search_reader_dup_001";

            // TODO: 两种检索点的结果不会产生重复吧，需要测试验证

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = $"{strFrom} '{ strDisplayName }' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                strResultSetName,
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 根据读者证状态对读者库进行检索
        // parameters:
        //      strMatchStyle   匹配方式 left exact right middle
        //      strState  读者证状态
        //      bOnlyIncirculation  是否仅仅包括参与流通的数据库? true ：仅仅包括； false : 包括全部
        //      bGetPath    == true 获得path; == false 获得barcode
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchReaderState(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strState,
            string strMatchStyle,
            bool bOnlyIncirculation,
            bool bGetPath,
            int nMax,
            out List<string> aPathOrBarcode,
            out string strError)
        {
            strError = "";
            aPathOrBarcode = null;

            LibraryApplication app = this;

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ReaderDbs.Count; i++)
            {
                string strDbName = app.ReaderDbs[i].DbName;

                if (bOnlyIncirculation == true)
                {
                    if (app.ReaderDbs[i].InCirculation == false)
                        continue;
                }

                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "状态")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strState)
                    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }
            else
            {
                strError = "目前尚没有参与流通的读者库";
                return -1;
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            string strResultSetName = "search_reader_state_001";

            long lRet = channel.DoSearch(strQueryXml,
                strResultSetName,
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "读者证状态 '" + strState + "' (匹配方式: " + strMatchStyle + ") 没有命中";
                return 0;
            }

            long lHitCount = lRet;

            if (bGetPath == true)
            {
                lRet = channel.DoGetSearchResult(
                    strResultSetName,
                    0,
                    nMax,
                    "zh",
                    null,
                    out aPathOrBarcode,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 获取检索命中结果
                // 获得某一列信息的版本
                lRet = channel.DoGetSearchResultOneColumn(
                    strResultSetName,
                    0,
                    nMax,
                    "zh",
                    null,
                    0,  // nColumn,
                    out aPathOrBarcode,
                    out strError);
            }

            if (aPathOrBarcode.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#if NO
        // 获得册记录(包装后的版本)
        // 本函数为了执行效率方面的原因, 不去获得超过1条以上的路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
        public int GetItemRecXml(
            RmsChannelCollection channels,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out string strError)
        {
            byte [] timestamp = null;

            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            return GetItemRecXml(
                channel,
                strBarcode,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strError);
        }
#endif

        public int GetItemRecXml(
            RmsChannel channel,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out string strError)
        {
            byte[] timestamp = null;

            return GetItemRecXml(
                channel,
                strBarcode,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strError);
        }


#if SLOWLY
        // TODO: 判断strBarcode是否为空
        // 获得册记录
        // 本函数为了执行效率方面的原因, 不去获得超过1条以上的路径。所返回的重复条数最大为1000
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
        public int GetItemRecXml(
            RmsChannelCollection channels,
            string strBarcode,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].DbName;

                // 2008/10/16 
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "册条码")       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode) + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                // 1000 2011/9/5

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "册条码号 '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                1,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            // byte[] timestamp = null;
            // string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }
#endif
        public int GetItemRecParent(
    RmsChannel channel,
    string strBarcode,
    string strOwnerInstitution,
    out string strParentID,
    out string strOutputPath,
    out string strError)
        {
            byte[] timestamp = null;

            return GetItemRecXml(
channel,
strBarcode,
strOwnerInstitution,
"parent",
out strParentID,
out strOutputPath,
out timestamp,
out strError);
        }

        public int GetItemRecXml(
    RmsChannel channel,
    string strBarcodeParam,
    out string strXml,
    out string strOutputPath,
    out byte[] timestamp,
    out string strError)
        {
            return GetItemRecXml(
    channel,
    strBarcodeParam,
    null,
    "",
    out strXml,
    out strOutputPath,
    out timestamp,
    out strError);
        }

        // 2014/9/19 strBarcode 可以包含 @refID: 前缀了
        // 2012/1/5 改造为PiggyBack检索
        // TODO: 判断strBarcode是否为空
        // 获得册记录
        // 本函数为了执行效率方面的原因, 不去获得超过1条以上的路径。所返回的重复条数最大为1000
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
        public int GetItemRecXml(
            RmsChannel channel,
            string strBarcodeParam,
            string strOwnerInstitution,
            string strStyle,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            string strBarcode = strBarcodeParam;
            string strHead = "@refID:";

            string strFrom = "册条码";
            if (StringUtil.HasHead(strBarcode, strHead, true) == true)
            {
                strFrom = "参考ID";
                strBarcode = strBarcode.Substring(strHead.Length).Trim();
                if (string.IsNullOrEmpty(strBarcode) == true)
                {
                    strError = "字符串 '" + strBarcodeParam + "' 中 参考ID 部分不应为空";
                    return -1;
                }
            }

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].DbName;

                // 2008/10/16 
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode) + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                // 1000 2011/9/5

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            string strBrowseStyle = "id,xml,timestamp";
            if (strStyle == "parent" && strOwnerInstitution == null)
                strBrowseStyle = "id,cols,format:@coldef://parent";

            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOuputStyle
                1,
                "zh",
                strBrowseStyle, // "id,xml,timestamp",
                out Record[] records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "册条码号 '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }

            string parent_id = "";
            if (strStyle == "parent")
            {
                strOutputPath = records[0].Path;
                if (strOwnerInstitution == null)
                    parent_id = records[0].Cols[0];
                else
                {
                    Debug.Assert(records[0].RecordBody != null, "");

                    strXml = records[0].RecordBody.Xml;
                    timestamp = records[0].RecordBody.Timestamp;

                    {
                        XmlDocument itemdom = new XmlDocument();
                        try
                        {
                            itemdom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = $"册记录 '{strOutputPath}' 装载到 XMLDOM 时出错: {ex.Message}";
                            goto ERROR1;
                        }
                        parent_id = DomUtil.GetElementText(itemdom.DocumentElement, "parent");
                    }
                }
            }
            else
            {
                Debug.Assert(records[0].RecordBody != null, "");

                strOutputPath = records[0].Path;
                strXml = records[0].RecordBody.Xml;
                timestamp = records[0].RecordBody.Timestamp;
            }

            if (strOwnerInstitution != null
                && string.IsNullOrEmpty(strXml) == false)
            {
                // return:
                //      -1  出错
                //      0   没有通过较验
                //      1   通过了较验
                int nRet = VerifyItemOI(strOutputPath,
                    strXml,
                    strOwnerInstitution,
                    out strError);
                if (nRet == 0)
                    return 0;
                if (nRet != 1)
                    return -1;
            }

            if (strStyle == "parent")
                strXml = parent_id;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#if SLOWLY
        // TODO: 判断strBarcode是否为空
        // 获得册记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      strBarcode  册条码号。也可以为 "@refID:值" 形态
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetItemRecXml(
            RmsChannelCollection channels,
            string strBarcode,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = null;

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            string strHead = "@refID:";

            string strFrom = "册条码";
            if (StringUtil.HasHead(strBarcode, strHead) == true)
            {
                strFrom = "参考ID";
                strBarcode = strBarcode.Substring(strHead.Length);
            }

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].DbName;

                // 2008/10/16 
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>"+nMax.ToString()+"</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = strFrom + " '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            // List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                Math.Min(nMax, lHitCount),
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            Debug.Assert(aPath != null, "");

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            // byte[] timestamp = null;
            string strOutputPath = "";

            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

#endif

        // 包装后的版本
        public int GetItemRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strBarcode,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            return GetItemRecXml(// channels,
                channel,
                strBarcode,
                "",
                out strXml,
                nMax,
                out aPath,
                out timestamp,
                out strError);
        }

        // 兼容以前的版本，包装后的形态
        // 获得册记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      strBarcode  册条码号。也可以为 "@refID:值" 形态
        //      strStyle    如果包含 withresmetadata ,表示要在XML记录中返回<dprms:file>元素内的 __xxx 属性 2012/11/19
        //                  如果包含 noxml， 则表示不返回 XML 记录体
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetItemRecXml(
            RmsChannel channel,
            string strBarcodeParam,
            string strStyle,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            string strBarcode = strBarcodeParam;
            string strHead = "@refID:";

            string strFrom = "册条码";
            if (StringUtil.HasHead(strBarcode, strHead, true) == true)
            {
                strFrom = "参考ID";
                strBarcode = strBarcode.Substring(strHead.Length).Trim();
                if (string.IsNullOrEmpty(strBarcode) == true)
                {
                    strError = "字符串 '" + strBarcodeParam + "' 中 参考ID 部分不应为空";
                    aPath = new List<string>();
                    timestamp = null;
                    strXml = "";
                    return -1;
                }
            }

            return GetOneItemRec(
                // channels,
                channel,
                "item",
                strBarcode,
                strFrom,
                strStyle + ",xml,timestamp",
                out strXml,
                nMax,
                out aPath,
                out timestamp,
                out strError);
        }

#if NO
        // 被更通用的版本 GetOneItemRec() 替代 
        // 2012/11/27改造为可以不获得XML和时间戳
        // 2012/1/5改造为PiggyBack检索
        // TODO: 判断strBarcode是否为空
        // 获得册记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      strBarcode  册条码号。也可以为 "@refID:值" 形态
        //      strStyle    如果包含 withresmetadata ,表示要在XML记录中返回<dprms:file>元素内的 __xxx 属性 2012/11/19
        //                  如果包含 xml， 则表示返回 XML 记录体
        //                  如果包含 timestamp, 则表示返回时间戳
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetItemRec(
            RmsChannelCollection channels,
            string strBarcode,
            string strStyle,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = null;

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            string strHead = "@refID:";

            string strFrom = "册条码";
            if (StringUtil.HasHead(strBarcode, strHead) == true)
            {
                strFrom = "参考ID";
                strBarcode = strBarcode.Substring(strHead.Length);
            }

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].DbName;

                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            /*
            string strGetStyle = "id,xml,timestamp";

            if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                strGetStyle += ",withresmetadata";
            */

            Record[] records = null;
            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOuputStyle
                1,
                "zh",
                strStyle + ",id",    // "id,xml,timestamp",
                out records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = strFrom + " '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }

#if DEBUG
            if (StringUtil.IsInList("xml", strStyle) == true
                || StringUtil.IsInList("timestamp", strStyle) == true)
            {
                Debug.Assert(records[0].RecordBody != null, "");
            }
#endif

            aPath = new List<string>();
            aPath.Add(records[0].Path);
            if (records[0].RecordBody != null)
            {
                strXml = records[0].RecordBody.Xml;
                timestamp = records[0].RecordBody.Timestamp;
            }

            // 如果命中结果多余一条，则继续获得第一条以后的各条的path
            if (lHitCount > 1)
            {
                // List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "default",
                    0,
                    Math.Min(nMax, lHitCount),
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Debug.Assert(aPath != null, "");

                if (aPath.Count == 0)
                {
                    strError = "DoGetSearchResult aPath error";
                    goto ERROR1;
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }
#endif

        // 检索一个册记录或者评注\订购\期记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      strBarcode  册条码号。也可以为 "@refID:值" 形态
        //      strStyle    如果包含 withresmetadata ,表示要在XML记录中返回<dprms:file>元素内的 __xxx 属性 2012/11/19
        //                  如果包含 xml， 则表示返回 XML 记录体
        //                  如果包含 timestamp, 则表示返回时间戳
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetOneItemRec(
            RmsChannel channel,
            string strDbType,
            string strBarcode,
            string strFrom,
            string strStyle,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = new List<string>();

            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            List<string> dbnames = null;
            int nRet = app.GetDbNames(
    strDbType,
    out dbnames,
    out strError);
            if (nRet == -1)
                return -1;

#if NO
            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }
#endif
            string strHint = "";
            if (StringUtil.IsInList("first", strStyle) == true)
            {
                strHint = " hint='first' ";
                StringUtil.SetInList(ref strStyle, "first", false); // 去掉 first 子串。因为 strStyle 这个用法只有在本函数有意义
            }

            string strQueryXml = "";
            {
                // 构造检索式
                // 新方法只用一个 item 元素，把各个库的 dbname 和 from 都拍紧到同一个 targetlist 中
                StringBuilder targetList = new StringBuilder();
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;
                    if (targetList.Length > 0)
                        targetList.Append(";");
                    targetList.Append(strDbName + ":" + strFrom);
                }

                if (targetList.Length == 0)
                {
                    strError = "没有任何可检索的目标数据库";
                    return -1;
                }

                strQueryXml = "<target list='"
        + StringUtil.GetXmlStringSimple(targetList.ToString())
        + "' " + strHint + "><item><word>"
        + StringUtil.GetXmlStringSimple(strBarcode)
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";
            }

            if (channel == null)
                throw new ArgumentException("channel 参数不应为空", "channel");

            string strBrowseStyle = strStyle + ",id";
            if (strStyle == "parent")
                strBrowseStyle = "id,cols,format:@coldef://parent";


            Record[] records = null;
            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOuputStyle
                1,
                "zh",
                strBrowseStyle,    // "id,xml,timestamp",
                out records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = strFrom + " '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }

#if DEBUG
            if (StringUtil.IsInList("xml", strStyle) == true
                || StringUtil.IsInList("timestamp", strStyle) == true)
            {
                Debug.Assert(records[0].RecordBody != null, "");
            }
#endif

            aPath = new List<string>();
            aPath.Add(records[0].Path);

            if (records[0].RecordBody != null)
            {
                strXml = records[0].RecordBody.Xml;
                timestamp = records[0].RecordBody.Timestamp;
            }

            // 2018/10/21
            if (strStyle == "parent")
                strXml = records[0].Cols[0];

            // 如果命中结果多于一条，则继续获得第一条以后的各条的path
            if (lHitCount > 1)  // TODO: && nMax > 1
            {
                lRet = channel.DoGetSearchResult(
                    "default",
                    0,
                    Math.Min(nMax, lHitCount),
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Debug.Assert(aPath != null, "");

                if (aPath.Count == 0)
                {
                    strError = "DoGetSearchResult aPath error";
                    goto ERROR1;
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 获得数据库类型的中文名称
        public static string GetDbTypeName(string strDbType)
        {
            if (strDbType == "biblio")
            {
                return "书目";
            }
            else if (strDbType == "reader")
            {
                return "读者";
            }
            else if (strDbType == "item")
            {
                return "实体";
            }
            else if (strDbType == "issue")
            {
                return "期";
            }
            else if (strDbType == "order")
            {
                return "订购";
            }
            else if (strDbType == "comment")
            {
                return "评注";
            }
            else if (strDbType == "invoice")
            {
                return "发票";
            }
            else if (strDbType == "amerce")
            {
                return "违约金";
            }
            else
            {
                return null;
            }
        }

        // 根据特定数据库类型，列出所有数据库名
        // 不包括读者库
        public int GetDbNames(
            string strDbType,
            out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            if (strDbType == "biblio")
            {
                for (int i = 0; i < this.ItemDbs.Count; i++)
                {
                    // 实体库对应的书目库名
                    string strBiblioDbName = this.ItemDbs[i].BiblioDbName;

                    if (String.IsNullOrEmpty(strBiblioDbName) == false)
                        dbnames.Add(strBiblioDbName);
                }
            }
            else if (strDbType == "authority")
            {
                // 2018/9/26
                dbnames = this.GetAuthorityDbNames();
            }
            else if (strDbType == "item")
            {
                for (int i = 0; i < this.ItemDbs.Count; i++)
                {
                    // 实体库名
                    string strItemDbName = this.ItemDbs[i].DbName;

                    if (String.IsNullOrEmpty(strItemDbName) == false)
                        dbnames.Add(strItemDbName);
                }
            }
            else if (strDbType == "issue")
            {
                for (int i = 0; i < this.ItemDbs.Count; i++)
                {
                    // 期库名
                    string strIssueDbName = this.ItemDbs[i].IssueDbName;

                    if (String.IsNullOrEmpty(strIssueDbName) == false)
                        dbnames.Add(strIssueDbName);
                }
            }
            else if (strDbType == "order")
            {
                for (int i = 0; i < this.ItemDbs.Count; i++)
                {
                    // 订购库名
                    string strOrderDbName = this.ItemDbs[i].OrderDbName;

                    if (String.IsNullOrEmpty(strOrderDbName) == false)
                        dbnames.Add(strOrderDbName);
                }
            }
            else if (strDbType == "comment")
            {
                for (int i = 0; i < this.ItemDbs.Count; i++)
                {
                    // 实体库名
                    string strCommentDbName = this.ItemDbs[i].CommentDbName;

                    if (String.IsNullOrEmpty(strCommentDbName) == false)
                        dbnames.Add(strCommentDbName);
                }
            }
            else if (strDbType == "invoice")
            {
                if (string.IsNullOrEmpty(this.InvoiceDbName) == false)
                    dbnames.Add(this.InvoiceDbName);
            }
            else if (strDbType == "amerce")
            {
                if (string.IsNullOrEmpty(this.AmerceDbName) == false)
                    dbnames.Add(this.AmerceDbName);
            }
            else if (strDbType == "arrived")
            {
                if (string.IsNullOrEmpty(this.ArrivedDbName) == false)
                    dbnames.Add(this.ArrivedDbName);
            }
            else
            {
                strError = "未知的数据库类型 '" + strDbType + "'。应为biblio reader item issue order comment invoice amerce arrived之一";
                return -1;
            }

            return 0;
        }

        // 检查确保 kdbs != null
        public string EnsureKdbs(bool bThrowException = true)
        {
            if (this.kdbs == null)
            {
                this.ActivateManagerThreadForLoad();
                string strError = "app.kdbs == null。故障原因请检查dp2Library日志，或稍后重试操作";
                if (bThrowException == true)
                    throw new Exception(strError);

                return strError;
            }

            return null;    // 没有出错
        }

        // 检查全部读者库的检索途径，看是否满足都有“所借册条码号”这个检索途径的这个条件
        // return:
        //      -1  出错
        //      0   不满足
        //      1   满足
        public int DetectReaderDbFroms(
    out string strError)
        {
            strError = "";

            strError = EnsureKdbs(false);
            if (strError != null)
                return -1;

            // 获得全部读者库名
            List<string> dbnames = this.GetCurrentReaderDbNameList("");

            StringUtil.RemoveDupNoSort(ref dbnames);

            if (dbnames.Count == 0)
            {
                strError = "当前系统中没有定义此类数据库，所以无法获知其检索途径信息";
                return 0;
            }

            foreach (string strDbName in dbnames)
            {
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                KernelDbInfo db = this.kdbs.FindDb(strDbName);

                if (db == null)
                {
                    strError = "kdbs中没有关于读者库 '" + strDbName + "' 的信息";
                    return -1;
                }

                bool bFound = false;
                foreach (From from in db.Froms)
                {
                    if (StringUtil.IsInList("borrowitem", from.Styles) == true)
                    {
                        bFound = true;
                        break;
                    }
#if NO
                    Caption caption = from.GetCaption("zh");
                    if (caption != null
                        && (caption.Value == "所借册条码号" || caption.Value == "所借册条码"))
                    {
                        bFound = true;
                        break;
                    }
#endif
                }

                if (bFound == false)
                    return 0;
            }

            return 1;
        }


        // 列出某类数据库的检索途径信息
        // parameters:
        //          strLibraryCodeList  当前用户管辖的馆代码列表。这决定了能列出哪些读者库的检索途径。如果调用时 strDbType 不是 "reader"，本参数设为空即可 
        // return:
        //      -1  出错
        //      0   没有定义
        //      1   成功
        public int ListDbFroms(string strDbType,
            string strLang,
            string strLibraryCodeList,
            out BiblioDbFromInfo[] infos,
            out string strError)
        {
            infos = null;
            strError = "";

            strError = EnsureKdbs(false);
            if (strError != null)
                goto ERROR1;

            if (string.IsNullOrEmpty(strDbType) == true)
                strDbType = "biblio";

            // long lRet = 0;

            List<string> dbnames = null;
            if (strDbType == "reader")
            {
                dbnames = this.GetCurrentReaderDbNameList(strLibraryCodeList);    // sessioninfo.LibraryCodeList
            }
            else
            {
                int nRet = this.GetDbNames(
                    strDbType,
                    out dbnames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

#if NO
                List<string> dbnames = new List<string>();

                string strDbTypeName = "";

                if (strDbType == "biblio")
                {
                    strDbTypeName = "书目";
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        // 实体库对应的书目库名
                        string strBiblioDbName = app.ItemDbs[i].BiblioDbName;

                        if (String.IsNullOrEmpty(strBiblioDbName) == false)
                            dbnames.Add(strBiblioDbName);
                    }
                }
                else if (strDbType == "reader")
                {
                    strDbTypeName = "读者";
                    dbnames = app.GetCurrentReaderDbNameList(sessioninfo.LibraryCodeList);
                }
                else if (strDbType == "item")   // 2012/5/5
                {
                    strDbTypeName = "实体";
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        // 实体库名
                        string strItemDbName = app.ItemDbs[i].DbName;

                        if (String.IsNullOrEmpty(strItemDbName) == false)
                            dbnames.Add(strItemDbName);
                    }
                }
                else if (strDbType == "issue")   // 2012/5/5
                {
                    strDbTypeName = "期";
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        // 期库名
                        string strIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strIssueDbName) == false)
                            dbnames.Add(strIssueDbName);
                    }
                }
                else if (strDbType == "order")   // 2012/5/5
                {
                    strDbTypeName = "订购";
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        // 订购库名
                        string strOrderDbName = app.ItemDbs[i].OrderDbName;

                        if (String.IsNullOrEmpty(strOrderDbName) == false)
                            dbnames.Add(strOrderDbName);
                    }
                }
                else if (strDbType == "comment")   // 2012/5/5
                {
                    strDbTypeName = "评注";
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        // 实体库名
                        string strCommentDbName = app.ItemDbs[i].CommentDbName;

                        if (String.IsNullOrEmpty(strCommentDbName) == false)
                            dbnames.Add(strCommentDbName);
                    }
                }
                else if (strDbType == "invoice")
                {
                    strDbTypeName = "发票";
                    if (string.IsNullOrEmpty(app.InvoiceDbName) == false)
                        dbnames.Add(app.InvoiceDbName);
                }
                else if (strDbType == "amerce")
                {
                    strDbTypeName = "违约金";
                    if (string.IsNullOrEmpty(app.AmerceDbName) == false)
                        dbnames.Add(app.AmerceDbName);
                }
                else
                {
                    strError = "未知的数据库类型 '"+strDbType+"'。应为biblio reader item issue order comment invoice amerce之一";
                    goto ERROR1;
                }
#endif

            StringUtil.RemoveDupNoSort(ref dbnames);

            if (dbnames.Count == 0)
            {
                strError = "当前系统中没有定义此类数据库，所以无法获知其检索途径信息";
                return 0;
            }

            // 可以当时现列出，并不存储?
            // 不存储的缺点是，等到发出检索式的时候，就不知道哪个库有哪些style值了。
            // 后退一步：caption可以现列出，但是style值需要预先初始化和存储起来，供检索时构造检索式用
            List<From> froms = new List<From>();

            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                if (String.IsNullOrEmpty(strDbName) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                /*
                // 2011/12/17
                if (app.kdbs == null)
                {
                    app.ActivateManagerThreadForLoad();
                    strError = "app.kdbs == null。故障原因请检查dp2Library日志";
                    goto ERROR1;
                }
                 * */

                KernelDbInfo db = this.kdbs.FindDb(strDbName);

                if (db == null)
                {
                    strError = "kdbs中没有关于" + LibraryApplication.GetDbTypeName(strDbType) + "数据库 '" + strDbName + "' 的信息";
                    goto ERROR1;
                }

                // 把所有库的from累加起来
                froms.AddRange(db.Froms);
            }

            // 根据style值去重
            if (dbnames.Count > 1)
            {
                if (strDbType != "biblio")
                    KernelDbInfoCollection.RemoveDupByCaption(ref froms,
                        strLang);
                else
                    KernelDbInfoCollection.RemoveDupByStyle(ref froms);
            }

            List<BiblioDbFromInfo> info_list = new List<BiblioDbFromInfo>();

            int nIndexOfID = -1;    // __id途径所在的下标

            for (int i = 0; i < froms.Count; i++)
            {
                From from = froms[i];

                Caption caption = from.GetCaption(strLang);
                if (caption == null)
                {
                    caption = from.GetCaption(null);
                    if (caption == null)
                    {
                        strError = "有一个from事项的captions不正常";
                        goto ERROR1;
                    }
                }

                if (caption.Value == "__id")
                    nIndexOfID = i;

                BiblioDbFromInfo info = new BiblioDbFromInfo();
                info.Caption = caption.Value;
                info.Style = from.Styles;

                info_list.Add(info);
            }

            // 如果曾经出现过 __id caption
            if (nIndexOfID != -1)
            {
                BiblioDbFromInfo temp = info_list[nIndexOfID];
                info_list.RemoveAt(nIndexOfID);
                info_list.Add(temp);
            }

            infos = new BiblioDbFromInfo[info_list.Count];
            info_list.CopyTo(infos);

            return infos.Length;
        ERROR1:
            return -1;
        }

        // 将一个检索词列表中的，带有 @refID 的部分检索词拆为另外一个 list
        static void SplitWordList(string strWordList,
            out string strNormalList,
            out string strRefIDList)
        {
            List<string> refids = new List<string>();
            List<string> normals = new List<string>();
            string[] list = strWordList.Split(new char[] { ',' });
            foreach (string word in list)
            {
                if (word != null && word.StartsWith("@refID:") == true)
                    refids.Add(word.Substring("@refID:".Length));
                else
                    normals.Add(word);
            }

            strNormalList = string.Join(",", normals);
            strRefIDList = string.Join(",", refids);
        }

        // 一次检索多个检索词
        // "册条码";
        // "参考ID";
        // parameters:
        //      bMixRefID   是否在条码号中间杂了 @refID: 前缀的元素?
        // return:
        //      -1  出错
        //      0   一个也没有命中
        //      >0  命中的总个数。注意，这不一定是results中返回的元素个数。results中返回的个数还要受到nMax的限制，不一定等于全部命中个数
        public int GetItemRec(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strDbType,
            string strWordList,
            string strFrom,
            int nMax,
            string strStyle,
            out bool bMixRefID,
            out List<Record> results,
            out string strError)
        {
            strError = "";
            bMixRefID = false;

            results = new List<Record>();

            LibraryApplication app = this;

            List<string> dbnames = null;
            int nRet = app.GetDbNames(
    strDbType,
    out dbnames,
    out strError);
            if (nRet == -1)
                return -1;

            string strRefIDList = "";

            if (strFrom == "册条码" || strFrom == "册条码号")
            {
                string strNormalList = "";
                // 将一个检索词列表中的，带有 @refID 的部分检索词拆为另外一个 list
                SplitWordList(strWordList,
                    out strNormalList,
                    out strRefIDList);
                strWordList = strNormalList;
            }

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                if (string.IsNullOrEmpty(strWordList) == false)
                {
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple(strWordList)
                        + "</word><match>exact</match><relation>list</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                    if (nDbCount > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }
                    strQueryXml += strOneDbQuery;
                    nDbCount++;
                }

                if (string.IsNullOrEmpty(strRefIDList) == false)
                {
                    bMixRefID = true;
                    string strOneDbQuery = "<target list='"
    + StringUtil.GetXmlStringSimple(strDbName + ":" + "参考ID")
    + "'><item><word>"
    + StringUtil.GetXmlStringSimple(strRefIDList)
    + "</word><match>exact</match><relation>list</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                    if (nDbCount > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }
                    strQueryXml += strOneDbQuery;
                    nDbCount++;
                }
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif
            if (channel == null)
                throw new ArgumentException("channel 参数值不能为空", "channel");

            Record[] records = null;
            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                strStyle, // strOuputStyle
                nMax,
                "zh",
                strStyle + ",id",    // "id,xml,timestamp",
                out records,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "所有检索词一个也没有命中";
                return 0;
            }

            long lHitCount = lRet;
            if (nMax == -1)
                nMax = (int)lHitCount;
            else
            {
                if (nMax > lHitCount)
                    nMax = (int)lHitCount;
            }

            if (records == null || records.Length == 0)
            {
                strError = "records error";
                return -1;
            }

            results.AddRange(records);

            if (results.Count == lHitCount)
                return (int)lHitCount;

            // 如果第一次没有取完，需要继续取得
            if (nMax > records.Length)
            {
                long lStart = records.Length;
                long lCount = nMax - lStart;
                for (; ; )
                {
                    lRet = channel.DoGetSearchResult(
                    "default",
                    lStart,
                    lCount,
                    strStyle + ",id",    // "id,xml,timestamp",
                    "zh",
                    null,
                    out records,
                    out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    Debug.Assert(records != null, "");

                    if (records.Length == 0)
                    {
                        strError = "DoGetSearchResult records error";
                        goto ERROR1;
                    }

                    results.AddRange(records);
                    lStart += records.Length;
                    if (lStart >= lHitCount
                        || lStart >= nMax)
                        break;
                    lCount -= records.Length;
                }
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 获得评注记录(包装后的版本)
        // 本函数为了执行效率方面的原因, 不去获得超过1条以上的路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
        public int GetCommentRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strRefID,
            out string strXml,
            out string strOutputPath,
            out string strError)
        {
            byte[] timestamp = null;

            return GetCommentRecXml(
                // channels,
                channel,
                strRefID,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strError);
        }

        // TODO：判断strRedID是否为空
        // 获得评注记录
        // 本函数为了执行效率方面的原因, 不去获得超过1条以上的路径
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
        public int GetCommentRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strRefID,
            out string strXml,
            out string strOutputPath,
            out byte[] timestamp,
            out string strError)
        {
            strOutputPath = "";
            strXml = "";
            strError = "";
            timestamp = null;

            LibraryApplication app = this;

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].CommentDbName;

                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "参考ID")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strRefID) + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                strError = "参考ID '" + strRefID + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                1,
                "zh",
                null,
                out List<string> aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            lRet = channel.GetRes(aPath[0],
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // TODO: 判断strBarcode是否为空
        // 根据册条码号对实体库进行查重
        // 本函数只负责查重, 并不获得记录体
        // parameters:
        //      strFrom 检索途径
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchItemRecDup(
            RmsChannel channel,
            string strBarcode,
            string strFrom,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            if (strFrom == "册条码号")
                strFrom = "册条码";    // 兼容最老的 keys 定义。可考虑在适当时候全部修改为 "册条码号"

            LibraryApplication app = this;

            /* 导致内核出问题但是没有strError内容的式子
<group>
	<operator value='OR'/>
	<target list='图书编目实体:册条码'>
		<item><word>0000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>
        <lang>zh</lang>
    </target>
</group>             * */

            // 构造检索式
            string strQueryXml = "";
            int nDbCount = 0;
            for (int i = 0; i < app.ItemDbs.Count; i++)
            {
                string strDbName = app.ItemDbs[i].DbName;

                // 2008/10/16 
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;


                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":"
                    + strFrom/*"册条码"*/)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strBarcode)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            /*
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
             * */
            Debug.Assert(channel != null, "");

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                // TODO: 为了跟踪问题的方便，可以在strError中加上strQueryXml内容
                strError = "SearchItemRecDup() DoSearch() error: " + strError;
                goto ERROR1;
            }

            // not found
            if (lRet == 0)
            {
                strError = strFrom + " '" + strBarcode + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "SearchItemRecDup() DoGetSearchResult() error: " + strError;
                goto ERROR1;
            }

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        public string GetInventoryDbName()
        {
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("utilDb/database[@type='inventory']");
            if (nodes.Count == 0)
                return null;
            foreach (XmlElement node in nodes)
            {
                return node.GetAttribute("name");
            }

            return null;
        }

        // 根据馆代码、批次号和册条码号对盘点库进行查重
        // 本函数只负责查重, 并不获得记录体
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchInventoryRecDup(
            RmsChannel channel,
            string strLibraryCode,
            string strBatchNo,
            string strBarcode,
            string strRefID,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = new List<string>();

            string strInventoryDbName = GetInventoryDbName();

            if (string.IsNullOrEmpty(strInventoryDbName) == true)
            {
                strError = "当前尚未配置盘点库，因此无法对盘点库进行查重";
                return -1;
            }

            string strKey = "";

            if (string.IsNullOrEmpty(strBarcode) == false)
                strKey = strLibraryCode + "|" + strBatchNo + "|" + strBarcode;
            else
                strKey = strLibraryCode + "|" + strBatchNo + "|@refID:" + strRefID;

            // 构造检索式
            string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strInventoryDbName + ":" + "查重键")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strKey)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

            Debug.Assert(channel != null, "");

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                // TODO: 为了跟踪问题的方便，可以在strError中加上strQueryXml内容
                strError = "SearchInventoryRecDup() DoSearch() error: " + strError;
                goto ERROR1;
            }

            // not found
            if (lRet == 0)
            {
                strError = "查重键 '" + strKey + "' 没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "SearchInventoryRecDup() DoGetSearchResult() error: " + strError;
                goto ERROR1;
            }

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 将登录名切割为前缀和名字值两个部分
        void SplitLoginName(string strLoginName,
            out string strPrefix,
            out string strName)
        {
            int nRet = 0;

            strLoginName = strLoginName.Trim();

            List<string> prefixes = new List<string>();
            prefixes.Add("NB:");
            prefixes.Add("EM:");
            prefixes.Add("TP:");
            prefixes.Add("ID:");    // 2009/9/22 
            prefixes.Add("CN:");    // 2012/11/7
            prefixes.Add("RI:");    // 2020/3/4

            for (int i = 0; i < prefixes.Count; i++)
            {
                nRet = strLoginName.ToUpper().IndexOf(prefixes[i]);
                if (nRet == 0)
                {
                    strPrefix = prefixes[i];
                    strName = strLoginName.Substring(nRet + prefixes[i].Length).Trim();
                    return;
                }
            }

            strPrefix = "";
            strName = strLoginName;
        }

        // 获得读者记录, 为登录用途。注意，本函数不检查是否符合。
        // 该函数的特殊性在于，它可以用多种检索入口，而不仅仅是条码号
        // parameters:
        //      strQueryWord 登录名
        //          0) 如果以"RI:"开头，表示利用 参考ID 进行检索
        //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
        //          2) 如果以"EM:"开头，表示利用email地址进行检索。注意 email 本身应该是 email:xxxx 这样的形态。也就是说，整个加起来是 EM:email:xxxxx
        //          3) 如果以"TP:"开头，表示利用电话号码进行检索
        //          4) 如果以"ID:"开头，表示利用身份证号进行检索
        //          5) 如果以"CN:"开头，表示利用证件号码进行检索
        //          6) 否则用证条码号进行检索
        // return:
        //      -2  当前没有配置任何读者库，或者可以操作的读者库
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        int GetReaderRecXmlForLogin(
            RmsChannel channel,
            string strLibraryCodeList,
            string strQueryWord,
            int nMaxHitCount,
            string strFormatList,
            out List<KernelRecord> records,
            out string strError)
        {
            strError = "";
            records = new List<KernelRecord>();

            int nRet = 0;
            LibraryApplication app = this;
            string strFrom = "证条码";
            string strMatch = "exact";

            // 构造检索式
            string strQueryXml = "";

            strQueryWord = strQueryWord.Trim();

            string strPrefix = "";
            string strName = "";

            SplitLoginName(strQueryWord, out strPrefix, out strName);

            bool bBarcode = false;

            // 注意如果这里增补新的prefix， 函数 SplitLoginName() 也要同步修改
            // 没有前缀
            if (strPrefix == "")
            {
                bBarcode = true;
                strFrom = "证条码";
                strMatch = "exact";
            }
            else if (strPrefix == "NB:")
            {
                bBarcode = false;
                strFrom = "姓名生日";
                strMatch = "left";
                strQueryWord = strName;
            }
            else if (strPrefix == "EM:")
            {
                bBarcode = false;
                strFrom = "Email";
                strMatch = "exact";
                strQueryWord = strName;  // 2016/4/11 注 strName 内容应为 email:xxxxx
            }
            else if (strPrefix == "TP:")
            {
                bBarcode = false;
                strFrom = "电话";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "ID:")
            {
                bBarcode = false;
                strFrom = "身份证号";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "CN:")
            {
                bBarcode = false;
                strFrom = "证号";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "RI:")
            {
                bBarcode = false;
                strFrom = "参考ID";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else
            {
                strError = "未知的登录名前缀 '" + strPrefix + "'";
                return -1;
            }

            List<string> dbnames = new List<string>();
            // 获得读者库名列表
            // parameters:
            //      strReaderDbNames    库名列表字符串。如果为空，则表示全部读者库
            // return:
            //      -1  出错
            //      >=0 dbnames 中包含的读者库名数量
            nRet = GetDbNameList("",
                strLibraryCodeList,
                out dbnames,
                out strError);
            if (nRet == -1)
                return -1;

            if (dbnames.Count == 0)
            {
                if (app.ReaderDbs.Count == 0)
                    strError = "当前尚没有配置读者库";
                else
                    strError = "当前没有可以操作的读者库";
                return -2;
            }

            {
                int i = 0;
                foreach (string strDbName in dbnames)
                {
                    if (string.IsNullOrEmpty(strDbName) == true)
                        continue;

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    // 最多100条
                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatch + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMaxHitCount + "</maxCount></item><lang>zh</lang></target>";

                    if (string.IsNullOrEmpty(strQueryXml) == false)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                    i++;
                }

                if (i > 1)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }

            if (String.IsNullOrEmpty(strQueryXml) == true)
            {
                strError = "尚未配置读者库";
                return -1;
            }

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "channel.DoSearch() error : " + strError;
                return -1;
            }

            // not found
            if (lRet == 0)
            {
                strError = "没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (lHitCount > 1 && bBarcode == true)
            {
                strError = "系统错误: 证条码号为 '" + strQueryWord + "' 的读者记录多于一个";
                return -1;
            }

            try
            {
                SearchResultLoader loader = new SearchResultLoader(channel,
                null,
                "default",
                strFormatList);

                foreach (KernelRecord record in loader)
                {
                    records.Add(record);
                    if (nMaxHitCount >= 0 && records.Count >= nMaxHitCount)
                        break;
                }

                return records.Count;
            }
            catch (Exception ex)
            {
                strError = "GetReaderRecForLogin() 出现异常: " + ex.Message;
                return -1;
            }
        }

        // 获得读者记录, 并检查密码是否符合。为登录用途
        // 该函数的特殊性在于，它可以用多种检索入口，而不仅仅是条码号
        // parameters:
        //      strQueryWord 登录名
        //          0) 如果以"RI:"开头，表示利用 参考ID 进行检索
        //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
        //          2) 如果以"EM:"开头，表示利用email地址进行检索。注意 email 本身应该是 email:xxxx 这样的形态。也就是说，整个加起来是 EM:email:xxxxx
        //          3) 如果以"TP:"开头，表示利用电话号码进行检索
        //          4) 如果以"ID:"开头，表示利用身份证号进行检索
        //          5) 如果以"CN:"开头，表示利用证件号码进行检索
        //          6) 否则用证条码号进行检索
        //      strPassword 密码。如果为null，表示不进行密码判断。注意，不是""
        //      strGetToken 是否要获得 token ，和有效期。 空 / day / month / year
        // return:
        //      -2  当前没有配置任何读者库，或者可以操作的读者库
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        int GetReaderRecXmlForLogin(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strLibraryCodeList,
            string strQueryWord,
            string strPassword,
            int nIndex,
            string strClientIP,
            string strGetToken,
            out bool passwordExpired,
            out bool bTempPassword,
            out string strXml,
            out string strOutputPath,
            out byte[] output_timestamp,
            out string strToken,
            out string strError)
        {
            strOutputPath = "";
            strXml = "";
            strError = "";
            output_timestamp = null;
            bTempPassword = false;
            strToken = "";
            passwordExpired = false;

            int nRet = 0;
            LibraryApplication app = this;
            string strFrom = "证条码";
            string strMatch = "exact";

            // 构造检索式
            string strQueryXml = "";

            // int nRet = 0;
            strQueryWord = strQueryWord.Trim();

            string strPrefix = "";
            string strName = "";

            SplitLoginName(strQueryWord, out strPrefix, out strName);

            bool bBarcode = false;

            // 注意如果这里增补新的prefix， 函数 SplitLoginName() 也要同步修改
            // 没有前缀
            if (strPrefix == "")
            {
                bBarcode = true;
                strFrom = "证条码";
                strMatch = "exact";
            }
            else if (strPrefix == "NB:")
            {
                bBarcode = false;
                strFrom = "姓名生日";
                strMatch = "left";
                strQueryWord = strName;
            }
            else if (strPrefix == "EM:")
            {
                bBarcode = false;
                strFrom = "Email";
                strMatch = "exact";
                strQueryWord = strName;  // 2016/4/11 注 strName 内容应为 email:xxxxx
            }
            else if (strPrefix == "TP:")
            {
                bBarcode = false;
                strFrom = "电话";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "ID:")
            {
                bBarcode = false;
                strFrom = "身份证号";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "CN:")
            {
                bBarcode = false;
                strFrom = "证号";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else if (strPrefix == "RI:")
            {
                // 2016/4/11
                bBarcode = false;
                strFrom = "参考ID";
                strMatch = "exact";
                strQueryWord = strName;
            }
            else
            {
                strError = "未知的登录名前缀 '" + strPrefix + "'";
                return -1;
            }

            List<string> dbnames = new List<string>();
            // 获得读者库名列表
            // parameters:
            //      strReaderDbNames    库名列表字符串。如果为空，则表示全部读者库
            // return:
            //      -1  出错
            //      >=0 dbnames 中包含的读者库名数量
            nRet = GetDbNameList("",
                strLibraryCodeList,
                out dbnames,
                out strError);
            if (nRet == -1)
                return -1;

            if (dbnames.Count == 0)
            {
                if (app.ReaderDbs.Count == 0)
                    strError = "当前尚没有配置读者库";
                else
                    strError = "当前没有可以操作的读者库";
                return -2;
            }

            {
                int i = 0;
                foreach (string strDbName in dbnames)
                {
                    if (string.IsNullOrEmpty(strDbName) == true)
                        continue;

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    // 最多100条
                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatch + "</match><relation>=</relation><dataType>string</dataType><maxCount>100</maxCount></item><lang>zh</lang></target>";

                    if (string.IsNullOrEmpty(strQueryXml) == false)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                    i++;
                }

                if (i > 1)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }

#if NO
            if (app.ReaderDbs.Count == 0)
            {
                strError = "尚未配置读者库";
                return -1;
            }

            {
                for (int i = 0; i < app.ReaderDbs.Count; i++)
                {
                    string strDbName = app.ReaderDbs[i].DbName;

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    // 最多100条
                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>"+strMatch+"</match><relation>=</relation><dataType>string</dataType><maxCount>100</maxCount></item><lang>zh</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (app.ReaderDbs.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }
#endif

            if (String.IsNullOrEmpty(strQueryXml) == true)
            {
                strError = "尚未配置读者库";
                return -1;
            }

#if NO
            RmsChannel channel = channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "channel.DoSearch() error : " + strError;
                goto ERROR1;
            }

            // not found
            if (lRet == 0)
            {
                // WriteErrorLog($"检索读者记录没有找到(strError='{strError}')。检索式='{strQueryXml}'");
                strError = "没有找到";
                return 0;
            }

            long lHitCount = lRet;

            if (lHitCount > 1 && bBarcode == true)
            {
                strError = "系统错误: 证条码号为 '" + strQueryWord + "' 的读者记录多于一个";
                return -1;
            }

            lHitCount = Math.Min(lHitCount, 100);

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                lHitCount,
                "zh",
                null,
                out List<string> aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            /*
            // 只命中一个
            if (aPath.Count == 1)
                goto LOADONE;
             * */

            // 排除掉证状态挂失的那些
            List<string> aPathNew = new List<string>();
            List<string> aXml = new List<string>();
            List<string> aOutputPath = new List<string>();
            List<byte[]> aTimestamp = new List<byte[]>();

            for (int i = 0; i < aPath.Count; i++)
            {
                string strMetaData = "";
                byte[] timestamp = null;

                lRet = channel.GetRes(aPath[i],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                XmlDocument readerdom = null;
                if (strPassword != null)
                {
                    nRet = LibraryApplication.LoadToDom(strXml,
                        out readerdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录 '" + aPath[i] + "' 进入XML DOM时发生错误: " + strError;
                        return -1;
                    }

                    /*
                    string strState = DomUtil.GetElementText(readerdom.DocumentElement,
                        "state");
                     * */

                    if (strPassword != null)    // 2009/9/22 
                    {
                        StringBuilder debugInfo = null; // new StringBuilder();
                        // 验证读者密码
                        // return:
                        //      -1  error
                        //      0   密码不正确
                        //      1   密码正确
                        nRet = VerifyReaderPassword(
                            strClientIP,
                            readerdom,
                            strPassword,
                            this.Clock.Now,
                            debugInfo,
                            out bTempPassword,
                            out passwordExpired,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            // WriteErrorLog($"VerifyReaderPassword() return 0. (strError='{strError}')。strPassword='{strPassword}', strDebugInfo='{debugInfo?.ToString()}'");
                            continue;
                        }

                        // 原来在这里

                        // 2021/7/16
                        // *** 检查密码强度
                        if (StringUtil.IsInList("login", this._patronPasswordStyle) == true)
                        {
                            // return:
                            //      -1  出错
                            //      0   不合法(原因在 strError 中返回)
                            //      1   合法
                            nRet = LibraryApplication.ValidatePatronPassword(
                                readerdom.DocumentElement,
                                strPassword,
                                this._patronPasswordStyle,
                                false,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 0)
                            {
                                // passwordExpired = true;
                                strError = $"现有密码强度不够，请修改密码后重新登录: {strError}";
                                return -1;
                            }
                        }

                    }
                }

                if (string.IsNullOrEmpty(strGetToken) == false)
                {
                    if (readerdom == null)
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载读者记录 '" + aPath[i] + "' 进入XML DOM时发生错误: " + strError;
                            return -1;
                        }
                    }

                    StringBuilder debugInfo = null; // new StringBuilder();
                    string strHashedPassword = DomUtil.GetElementInnerText(readerdom.DocumentElement, "password");
                    nRet = MakeToken(strClientIP,
                        GetTimeRangeByStyle(strGetToken),
                        strHashedPassword,
                        debugInfo,
                        out strToken,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    // WriteErrorLog($"MakeToken() return {nRet}, strDebugInfo='{debugInfo?.ToString()}'");
                }

                aPathNew.Add(aPath[i]);
                aXml.Add(strXml);
                aOutputPath.Add(strOutputPath);
                aTimestamp.Add(timestamp);
            }

            // 过滤后，却又发现一个都没有了。凑合着给过滤前的第一个?
            if (aPathNew.Count == 0)
            {
                // WriteErrorLog($"aPathNew.Count == 0. strQueryWord='{strQueryWord}'");
                return 0;
            }

            if (nIndex >= aXml.Count)
            {
                strError = "选择索引值 " + nIndex.ToString() + " 越过范围。";
                return -1;
            }

            if (aXml.Count == 1 && nIndex == -1)
                nIndex = 0;

            if (nIndex != -1)
            {
                strXml = aXml[nIndex];
                strOutputPath = aOutputPath[nIndex];
                output_timestamp = aTimestamp[nIndex];
            }

            // WriteErrorLog($"return. aPathNew.Count={aPathNew.Count}");
            return aPathNew.Count;
        ERROR1:
            return -1;
            /*
        LOADONE:
        {
                string strMetaData = "";
                byte[] timestamp = null;

                lRet = channel.GetRes(aPath[0],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            return (int)lHitCount;
             * */
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
        public int ConvertReaderXmlToHtml(
            string strXml,
            OperType opertype,
            string[] saBorrowedItemBarcode,
            string strCurrentItemBarcode,
            out string strResult,
            out string strError)
        {
            return ConvertReaderXmlToHtml(
                this.CfgDir + "\\readerxml2html.cs",
                this.CfgDir + "\\readerxml2html.cs.ref",
                strXml,
                opertype,
                saBorrowedItemBarcode,
                strCurrentItemBarcode,
                out strResult,
                out strError);
        }
         */

        // 筛选读者记录
        // parameters:
        // return:
        //      -2  尚未注册手机号
        //      -1  出错
        //      0   完成筛选
        static int FilterPatron(List<KernelRecord> records,
            string strName,
            string strPatronBarcode,
            string strTel,
            out List<KernelRecord> results,
            out string strError)
        {
            strError = "";
            results = new List<KernelRecord>();

            foreach (KernelRecord record in records)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(record.Xml);
                }
                catch (Exception ex)
                {
                    strError = "读者记录 '" + record.RecPath + "' XML 装入 DOM 时出错: " + ex.Message;
                    return -1;
                }

                if (string.IsNullOrEmpty(strName) == false)
                {
                    if (strName != DomUtil.GetElementText(dom.DocumentElement, "name"))
                        continue;
                }

                if (string.IsNullOrEmpty(strPatronBarcode) == false)
                {
                    if (strPatronBarcode != DomUtil.GetElementText(dom.DocumentElement, "barcode"))
                        continue;
                }

                if (string.IsNullOrEmpty(strTel) == false)
                {
                    string strTelList = DomUtil.GetElementText(dom.DocumentElement, "tel");

                    if (string.IsNullOrEmpty(strTelList) == true
                        && records.Count == 1)
                    {
                        strError = "尚未注册手机号码";
                        return -2;
                    }

                    if (MatchTel(strTelList, strTel) == false)
                        continue;
                }

                results.Add(record);
            }

            return 0;
        }

        // 匹配电话号码
        static bool MatchTel(string strTelList, string strOneTel)
        {
            if (string.IsNullOrEmpty(strTelList))
                return false;

            if (string.IsNullOrEmpty(strOneTel))
                return false;

            strOneTel = strOneTel.Trim();
            string[] tels = strTelList.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tel in tels)
            {
                string one = tel.Trim();
                if (one == strOneTel)
                    return true;
            }
            return false;
        }

        // 临时密码重发最短周期
        static TimeSpan _tempPasswordRetryPeriod = new TimeSpan(0, 5, 0); // 五分钟

        // 重设密码
        // parameters:
        //      strParameters   参数字符串。如果 queryword 使用 NB: 形态，注意要这样使用 NB:姓名|，因为这是采用前方一致检索的，如果没有竖线部分，可能会匹配上不该命中的较长的名字
        //      strMessageTempate   消息文字模板。其中可以使用 %name% %barcode% %temppassword% %expiretime% %period% 等宏
        //      strMessage  返回拟发送给读者的消息文字
        // return:
        //      -2  读者的图书馆账户尚未注册手机号
        //      -1  出错
        //      0   因为条件不具备功能没有成功执行
        //      1   功能成功执行
        public int ResetPassword(
            // string strLibraryCodeList,
            string strParameters,
            string strMessageTemplate,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";

            MessageInterface external_interface = this.GetMessageInterface("sms");

            Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
            string strQueryWord = (string)parameters["queryword"];
            string strPatronBarcodeParam = (string)parameters["barcode"];
            string strNameParam = (string)parameters["name"];
            string strTelParam = (string)parameters["tel"];
            string strLibraryCodeList = (string)parameters["librarycode"];  // 控制检索读者记录的范围

            int nMaxHitCount = 10;

            bool bReturnMessage = false;
            string strStyle = (string)parameters["style"];
            if (StringUtil.IsInList("returnMessage", strStyle) == true)
            {
                // 直接给调用者返回拟发送到手机短信的内容。要求调用者具有特殊权限才行，要求在调用本函数前判断好。
                bReturnMessage = true;
            }
            else
            {
                if (string.IsNullOrEmpty(this.OutgoingQueue) == true && external_interface == null)
                {
                    strError = "当前系统尚未配置短消息 (sms) 接口，也没有配置 MSMQ 消息队列，无法进行重设密码的操作";
                    return -1;
                }
            }

            if (string.IsNullOrEmpty(strQueryWord) == true)
            {
                strError = "缺乏 queryword 参数";
                return -1;
            }
            if (bReturnMessage == false && string.IsNullOrEmpty(strNameParam) == true)
            {
                strError = "缺乏 name 参数";
                return -1;
            }
            if (string.IsNullOrEmpty(strTelParam) == true)
            {
                strError = "缺乏 tel 参数";
                return -1;
            }

            // 判断电话号码是否为手机号码
            if (strTelParam.Length != 11)
            {
                strError = "所提供的电话号码应该是 11 位的手机号码";
                return 0;
            }

            // 临时的SessionInfo对象
            SessionInfo sessioninfo = new SessionInfo(this);
            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                // 获得读者记录
                // return:
                //      -2  当前没有配置任何读者库，或者可以操作的读者库
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetReaderRecXmlForLogin(
                    channel,
                    strLibraryCodeList,
                    strQueryWord,
                    nMaxHitCount,
                    "id,xml,timestamp",
                    out List<KernelRecord> records,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    strError = "以登录名 '" + strQueryWord + "' 检索读者记录出错: " + strError;
                    return -1;
                }
                if (nRet == 0)
                {
                    strError = "读者帐户 '" + strQueryWord + "' 不存在";
                    return 0;
                }
#if NO
                if (nRet > 1)
                {
                    strError = "登录名 '" + strLoginName + "' 所匹配的帐户多于一个";
                    return 0;
                }
#endif

                // 筛选读者记录
                // return:
                //      -2  尚未注册手机号
                //      -1  出错
                //      0   完成筛选
                nRet = FilterPatron(records,
            strNameParam,
            strPatronBarcodeParam,
            strTelParam,
            out List<KernelRecord> results,
            out strError);
                if (nRet == -1)
                    return -1;

                if (results.Count == 0)
                {
                    if (nRet == -2)
                    {
                        strError = "读者帐户 '" + strQueryWord + "' " + strError;
                        return -2;
                    }

                    strError = "符合条件的读者帐户 '" + strQueryWord + "' 不存在";
                    return 0;
                }

                XmlDocument output_dom = new XmlDocument();
                output_dom.LoadXml("<root />");

                foreach (KernelRecord record in results)
                {
                    // 获得读者库的馆代码
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = GetLibraryCode(
                        record.RecPath,
                        out string strLibraryCode,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    XmlDocument readerdom = null;
                    nRet = LibraryApplication.LoadToDom(record.Xml,
                        out readerdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                        return -1;
                    }

                    // 检查读者权限
                    // 合成读者记录的最终权限
                    nRet = GetReaderRights(
                        readerdom,
                        out string rights,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获得读者记录权限时发生错误: " + strError;
                        return -1;
                    }

                    // 2021/8/2
                    if (StringUtil.IsInList("denyresetmypassword", rights))
                    {
                        strError = "读者权限中包含 denyresetmypassword，不允许重设密码";
                        return -1;
                    }

                    // 观察 password 元素的 lastResetTime 属性，需在规定的时间长度以外才能再次进行重设
#if NO
                    // 观察在 password 元素 tempPasswordExpire 属性中残留的失效期，必须在这个时间以后才能进行本次操作
                    // parameters:
                    //      now 当前时间。本地时间
                    // return:
                    //      -1  出错
                    //      0   已经过了失效期
                    //      1   还在失效期以内
                    nRet = CheckOldExpireTime(readerdom,
                        this.Clock.Now,
                        out end,
                        out strError);
#endif

                    // return:
                    //      -1  出错
                    //      0   已经过了禁止期，可以重试了
                    //      1   还在重试禁止期以内
                    nRet = CheckRetryStartTime(readerdom,
    DateTime.Now,
    out DateTime end,
    out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        strError = "本次重设密码的操作距离上次操作间隔不足" + _tempPasswordRetryPeriod.TotalMinutes + "分钟，操作被拒绝。请在 " + end.ToShortTimeString() + " 以后再进行操作";
                        return 0;
                    }

                    string strBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
                    string strName = DomUtil.GetElementText(readerdom.DocumentElement, "name");

                    // 重新设定一个密码
                    Random rnd = new Random();
                    string strReaderTempPassword = rnd.Next(1, 999999).ToString();

                    DateTime expire = DateTime.Now + _tempPasswordExpirePeriod;    // new TimeSpan(1, 0, 0);   // 本地时间
                    string strExpireTime = DateTimeUtil.Rfc1123DateTimeStringEx(expire);

                    if (bReturnMessage == true)
                    {
                        XmlElement node = output_dom.CreateElement("patron");
                        output_dom.DocumentElement.AppendChild(node);

                        // 直接给调用者返回消息内容。消息内容中有临时密码，属于敏感信息，要求调用者具有特殊权限才行。

                        DomUtil.SetElementText(node, "tel", strTelParam);
                        DomUtil.SetElementText(node, "barcode", strBarcode);
                        DomUtil.SetElementText(node, "name", strName);
                        DomUtil.SetElementText(node, "tempPassword", strReaderTempPassword);
                        DomUtil.SetElementText(node, "expireTime", expire.ToLongTimeString());
                        DomUtil.SetElementText(node, "period",
                            _tempPasswordExpirePeriod.TotalMinutes.ToString() + "分钟"
                            // "一小时"
                            );
                        DomUtil.SetElementText(node, "refID", strRefID);    // 在所提供的姓名或者电话号码命中不止一条读者记录的情形，调用者后面使用读者记录的 refID 来绑定特别重要。
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(strMessageTemplate) == true)
                            strMessageTemplate = "%name% 您好！\n您的读者帐户(证条码号为 %barcode%)已设临时密码 %temppassword%，在 %period% 内登录会成为正式密码";

                        string strBody = strMessageTemplate.Replace("%barcode%", strBarcode)
                            .Replace("%name%", strName)
                            .Replace("%temppassword%", strReaderTempPassword)
                            .Replace("%expiretime%", expire.ToLongTimeString())
                            .Replace("%period%", _tempPasswordExpirePeriod.TotalMinutes.ToString() + "分钟"
                            //"一小时"
                            );
                        // string strBody = "读者(证条码号) " + strBarcode + " 的帐户密码已经被重设为 " + strReaderNewPassword + "";

                        if (string.IsNullOrEmpty(this.OutgoingQueue) == false)
                        {
                            // 2018/11/8
                            // 通过 MSMQ 发送手机短信
                            // parameters:
                            //      strUserName 账户名，或者读者证件条码号，或者 "@refID:xxxx"
                            nRet = SendSms(
                            sessioninfo.Account == null ? "[none]" : sessioninfo.Account.UserID,
                            strTelParam,
                            strBody,
                            out strError);
                            if (nRet == -1)
                            {
                                strError = "向读者 '" + strBarcode + "' 发送 SMS 时出错: " + strError;
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "重设密码通知",
                                    "SMS message 重设密码通知消息发送错误数",
                                    1);
                                this.WriteErrorLog(strError);
                                return -1;
                            }
                            else
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                strLibraryCode,
                "重设密码通知",
                "SMS message 重设密码通知消息发送数",
                nRet);  // 短信条数可能多于次数
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(strLibraryCode,
                                    "重设密码通知",
                                    "SMS message 重设密码通知人数",
                                    1);

                                // 2021/7/13
                                // 成功的情况也记入错误日志，便于分析
                                this.WriteErrorLog($"向 MSMQ 队列 '{this.OutgoingQueue}' 发送重设密码消息成功，strTelParam='{strTelParam}'");
                            }
                        }
                        else
                        // 向手机号码发送短信
                        {
                            // 发送消息
                            try
                            {
                                // 发送一条消息
                                // parameters:
                                //      strPatronBarcode    读者证条码号
                                //      strPatronXml    读者记录XML字符串。如果需要除证条码号以外的某些字段来确定消息发送地址，可以从XML记录中取
                                //      strMessageText  消息文字
                                //      strError    [out]返回错误字符串
                                // return:
                                //      -1  发送失败
                                //      0   没有必要发送
                                //      >=1   发送成功，返回实际发送的消息条数
                                nRet = external_interface.HostObj.SendMessage(
                                    strBarcode,
                                    readerdom.DocumentElement.OuterXml,
                                    strBody,
                                    strLibraryCode,
                                    out strError);
                            }
                            catch (Exception ex)
                            {
                                strError = external_interface.Type + " 类型的外部消息接口Assembly中SendMessage()函数抛出异常: " + ex.Message;
                                nRet = -1;
                            }
                            if (nRet == -1)
                            {
                                strError = "向读者 '" + strBarcode + "' 发送" + external_interface.Type + " message时出错: " + strError;
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                                    strLibraryCode,
                                    "重设密码通知",
                                    external_interface.Type + " message 重设密码通知消息发送错误数",
                                    1);
                                this.WriteErrorLog(strError);
                                return -1;
                            }
                            else
                            {
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(
                strLibraryCode,
                "重设密码通知",
                external_interface.Type + " message 重设密码通知消息发送数",
                nRet);  // 短信条数可能多于次数
                                if (this.Statis != null)
                                    this.Statis.IncreaseEntryValue(strLibraryCode,
                                    "重设密码通知",
                                    external_interface.Type + " message 重设密码通知人数",
                                    1);
                            }
                        }
                    }

                    nRet = ChangeReaderTempPassword(
            sessioninfo,
            record.RecPath,
            readerdom,
            strReaderTempPassword,
            // strExpireTime,
            record.Timestamp,
            out byte[] output_timestamp,
            out strError);
                    if (nRet == -1)
                        return -1;  // 此时短信已经发出，但临时密码并未修改成功
                }

                if (StringUtil.IsInList("returnMessage", strStyle) == true)
                    strMessage = output_dom.DocumentElement.OuterXml;
            }
            finally
            {
                sessioninfo.CloseSession();
                sessioninfo = null;
            }

            if (bReturnMessage == false)
                strError = "临时密码已通过短信方式发送到手机 " + strTelParam + "。请按照手机短信提示进行操作";
            return 1;
        }

        // 构造拟发送给读者的消息 XML
        static string BuildMessageXml(Hashtable table)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (string key in table.Keys)
            {
                string value = (string)table[key];
                DomUtil.SetElementText(dom.DocumentElement, key, value);
            }

            return dom.DocumentElement.OuterXml;
        }

        // 观察在 password 元素 tempPasswordCreateTime 属性中的时间加上重发周期长度，必须在这个时间以后才能进行本次操作
        // parameters:
        //      now 当前时间。本地时间
        //      retry_start  可以重发的最早时间。本地时间
        // return:
        //      -1  出错
        //      0   已经过了禁止期，可以重试了
        //      1   还在重试禁止期以内
        static int CheckRetryStartTime(XmlDocument readerdom,
            DateTime now,
            out DateTime retry_start,
            out string strError)
        {
            strError = "";
            retry_start = new DateTime(0);

            XmlNode node = readerdom.DocumentElement.SelectSingleNode("password");
            if (node == null)
                return 0;

            string strCreateTime = DomUtil.GetAttr(node,
"tempPasswordCreateTime");
            if (string.IsNullOrEmpty(strCreateTime) == true)
                return 0;

            try
            {
                retry_start = DateTimeUtil.FromRfc1123DateTimeString(strCreateTime).ToLocalTime() + _tempPasswordRetryPeriod;   // 2021/7/13 增加 + ...

                if (now > retry_start)
                {
                    // 禁止期已经过了
                    return 0;
                }
            }
            catch (Exception)
            {
                strError = "临时密码创建时间字符串 '" + strCreateTime + "' 格式不正确，应为 RFC1123 格式";
                return -1;
            }

            return 1;   // 尚在禁止期以内
        }

        // 观察在 password 元素 tempPasswordExpire 属性中残留的失效期，必须在这个时间以后才能进行本次操作
        // parameters:
        //      now 当前时间。本地时间
        //      expire  失效期末端时间。本地时间
        // return:
        //      -1  出错
        //      0   已经过了失效期
        //      1   还在失效期以内
        static int CheckOldExpireTime(XmlDocument readerdom,
            DateTime now,
            out DateTime expire,
            out string strError)
        {
            strError = "";
            expire = new DateTime(0);

            XmlNode node = readerdom.DocumentElement.SelectSingleNode("password");
            if (node == null)
                return 0;

            string strExpireTime = DomUtil.GetAttr(node,
"tempPasswordExpire");
            if (string.IsNullOrEmpty(strExpireTime) == true)
                return 0;

            try
            {
                expire = DateTimeUtil.FromRfc1123DateTimeString(strExpireTime).ToLocalTime();

                if (now > expire)
                {
                    // 失效期已经过了
                    return 0;
                }
            }
            catch (Exception)
            {
                strError = "临时密码失效期时间字符串 '" + strExpireTime + "' 格式不正确，应为 RFC1123 格式";
                return -1;
            }

            return 1;   // 尚在失效期以内
        }

        // 检查读者账号是否存在
        // return:
        //      -1  error
        //      0   不存在
        //      1   存在
        //      >1  多于一个
        public int VerifyReaderAccount(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strLoginName,
            out string strError)
        {
            strError = "";
            string strXml = "";
            string strOutputPath = "";

            byte[] timestamp = null;
            bool bTempPassword = false;
            string strToken = "";

            // 获得读者记录
            // return:
            //      -2  当前没有配置任何读者库，或者可以操作的读者库
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            int nRet = this.GetReaderRecXmlForLogin(
                // channels,
                channel,
                "",    // 全部分馆的读者库
                strLoginName,
                null,
                -1,
                null,
                null,
                out bool passwordExpired,
                out bTempPassword,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strToken,
                out strError);
            if (nRet == -1)
            {
                strError = "以登录名 '" + strLoginName + "' 检索读者记录出错: " + strError;
                return -1;
            }
            if (nRet > 1)
            {
                strError = "登录名 '" + strLoginName + "' 所匹配的帐户多于一个";
                return nRet;
            }

            /*
            if (passwordExpired)
            {
                strError = "帐户 '" + strLoginName + "' 密码已经失效";
                return 0;
            }
            */

            if (nRet == 0 || nRet == -2)
            {
                strError = "帐户 '" + strLoginName + "' 不存在";
                return 0;
            }

            Debug.Assert(nRet == 1);
            return 1;
        }

        // xxxx|||xxxx 右边部分是 timerange
        static string GetTimeRangeFromToken(string strToken)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strToken, "|||", out strLeft, out strRight);
            return strRight;
        }

        // parameters:
        //      strStyle 时间范围风格。 空 / day / month / year
        //              空等于 day
        public static string GetTimeRangeByStyle(string strStyle)
        {
            if (string.IsNullOrEmpty(strStyle) == true)
                return DateTimeUtil.DateTimeToString8(DateTime.Now);
            if (strStyle == "day")
                return DateTimeUtil.DateTimeToString8(DateTime.Now);
            if (strStyle == "month")
            {
                return DateTimeUtil.DateTimeToString8(DateTime.Now) + "-" + DateTimeUtil.DateTimeToString8(DateTime.Now.AddDays(31));
            }
            if (strStyle == "year")
            {
                return DateTimeUtil.DateTimeToString8(DateTime.Now) + "-" + DateTimeUtil.DateTimeToString8(DateTime.Now.AddDays(365));
            }

            // default
            return DateTimeUtil.DateTimeToString8(DateTime.Now);
        }

        // 获得一个 token
        // TODO: 要解决 localhost 和 127.0.0.1 和 ::1 和具体四段 IP 地址之间的等同关系判断问题
        // 创建 token
        public static int MakeToken(string strClientIP,
            string strTimeRange,
            string strHashedPassword,
            StringBuilder debugInfo,
            out string strToken,
            out string strError)
        {
            strError = "";
            strToken = "";

            debugInfo?.AppendLine($"enter MakeToken() strClientIP={strClientIP},strTimeRange={strTimeRange}, strHashedPassword={strHashedPassword}");

            if (string.IsNullOrEmpty(strTimeRange) == true)
                strTimeRange = GetTimeRangeByStyle(null);

            debugInfo?.AppendLine($"strTimeRange={strTimeRange}");

            string strHashed = "";
            string strPlainText = strClientIP + strHashedPassword + strTimeRange;
            try
            {
                debugInfo?.AppendLine($"strPlainText={strPlainText}");

                strHashed = Cryptography.GetSHA1(strPlainText);

                debugInfo?.AppendLine($"strHashed={strHashed}");
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            strToken = strHashed.Replace(",", "").Replace("=", "") + "|||" + strTimeRange;
            debugInfo?.AppendLine($"加工后的 strToken={strHashed}");
            return 0;
        }

        static bool IsInTimeRange(DateTime now,
            string strTimeRange)
        {
            int nRet = strTimeRange.IndexOf("-");
            if (nRet == -1)
            {
                if (strTimeRange == DateTimeUtil.DateTimeToString8(now))
                    return true;
                return false;
            }

            try
            {
                string strStart = "";
                string strEnd = "";
                StringUtil.ParseTwoPart(strTimeRange, "-", out strStart, out strEnd);
                DateTime start = DateTimeUtil.Long8ToDateTime(strStart);
                DateTime end = DateTimeUtil.Long8ToDateTime(strEnd);
                if (now > start && now < end)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        // TODO: 要解决 localhost 和 127.0.0.1 和 ::1 和具体四段 IP 地址之间的等同关系判断问题
        // 验证 TOKEN
        // Token 的发生规则为： client ip + hashed password + time range 然后 Hash。 Hash 完以后， time range 字符串再放在外面一份
        // return:
        //      -1  出错
        //      0   验证不匹配
        //      1   验证匹配
        public static int VerifyToken(
            string strClientIP,
            string strToken,
            string strHashedPassword,
            StringBuilder debugInfo,
            out string strError)
        {
            strError = "";

            debugInfo?.AppendLine($"enter VerifyToken() strClientIP={strClientIP},strToken={strToken}, strHashedPassword={strHashedPassword}");

            string strTimeRange = GetTimeRangeFromToken(strToken);
            if (string.IsNullOrEmpty(strTimeRange) == true)
                strTimeRange = GetTimeRangeByStyle(null);

            debugInfo?.AppendLine($"strTimeRange={strTimeRange}");

            // 看看时间是否失效
            if (IsInTimeRange(DateTime.Now, strTimeRange) == false)
            {
                strError = "token 已经失效";
                debugInfo?.AppendLine($"IsInTimeRange() return false");
                return 0;
            }

            string strHashed = "";
            string strPlainText = strClientIP + strHashedPassword + strTimeRange;
            try
            {
                debugInfo?.AppendLine($"strPlainText={strPlainText}");

                strHashed = Cryptography.GetSHA1(strPlainText);

                debugInfo?.AppendLine($"strHashed={strHashed}");
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }
            strHashed = strHashed.Replace(",", "").Replace("=", "");
            strHashed += "|||" + strTimeRange;

            debugInfo?.AppendLine($"加工后的 strHashed={strHashed}");

            if (strHashed == strToken)
            {
                debugInfo?.AppendLine($"匹配");
                return 1;   // 匹配
            }

            debugInfo?.AppendLine($"strHashed 和 strToken={strToken} 不匹配");
            return 0;   // 不匹配
        }

        // 公共查询读者登录
        // text-level: 用户提示
        // parameters:
        //      strLoginName 登录名
        //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
        //          2) 如果以"EM:"开头，表示利用email地址进行检索
        //          3) 如果以"TP:"开头，表示利用电话号码进行检索
        //          4) 否则用证条码号进行检索
        //      strPassword 密码。如果为null，表示不进行密码判断。注意，不是""
        //              还可以为 token: 形态
        //      nIndex  如果有多个匹配的读者记录，此参数表示要选择其中哪一个。
        //              如果为-1，表示首次调用此函数，还不知如何选择。如此时命中多个，函数会返回>1的值
        //      strGetToken 是否要获得 token ，和有效期。 空 / day / month / year
        //      alter_type_list 已经实施的绑定验证类型和尚未实施的类型列表
        //      strOutputUserName   返回读者证条码号
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        //      >1  有多个账户符合条件。
        public int LoginForReader(SessionInfo sessioninfo,
            string strLoginName,
            string strPassword,
            string strLocation,
            string strLibraryCodeList,
            int nIndex,
            string strGetToken,
            out bool passwordExpired,
            out List<string> alter_type_list,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            // 读者身份登录
            string strXml = "";
            string strOutputPath = "";
            byte[] timestamp = null;
            strRights = "";
            strOutputUserName = "";
            strLibraryCode = "";
            alter_type_list = new List<string>();
            passwordExpired = false;

            // 2009/9/22 
            if (String.IsNullOrEmpty(strLoginName) == true)
            {
                strError = "参数 strLoginName 不能为空";
                return -1;
            }

            if (this.LoginCache != null)
            {
                Account temp_account = this.LoginCache.Get(strLoginName) as Account;
                if (temp_account != null)
                {
                    if (strPassword != null)    // 2014/12/20
                    {
                        if (temp_account.Password != strPassword)
                        {
                            bool bIsToken1 = StringUtil.HasHead(strPassword, "token:");
                            bool bIsToken2 = StringUtil.HasHead(temp_account.Password, "token:");

                            if (bIsToken1 == bIsToken2)
                            {
                                // text-level: 用户提示
                                strError = this.GetString("帐户不存在或密码不正确") + " .";    // "帐户不存在或密码不正确"
                                return -1;
                            }
                            else
                                goto DO_LOGIN;  // 继续作普通登录
                        }
                    }

                    // 2021/7/5
                    // 检查密码失效期
                    if (this._patronPasswordExpirePeriod != TimeSpan.MaxValue)
                    {
                        DateTime expire = temp_account.PasswordExpire;
                        if (DateTime.Now > expire)
                        {
                            //strError = "密码已经失效";
                            //return -1;
                            passwordExpired = true;
                        }
                    }

                    sessioninfo.Account = temp_account;

                    strRights = temp_account.RightsOrigin;
                    strOutputUserName = temp_account.UserID;
                    strLibraryCode = temp_account.AccountLibraryCode;   // 2016/1/17
                    return 1;
                }
            }

        DO_LOGIN:

            bool bTempPassword = false;
            string strToken = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 获得读者记录, 并检查密码是否符合。为登录用途
            // 该函数的特殊性在于，它可以用多种检索入口，而不仅仅是条码号
            // parameters:
            //      strQueryWord 登录名
            //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
            //          2) 如果以"EM:"开头，表示利用email地址进行检索
            //          3) 如果以"TP:"开头，表示利用电话号码进行检索
            //          4) 如果以"ID:"开头，表示利用身份证号进行检索
            //          5) 如果以"CN:"开头，表示利用证件号码进行检索
            //          6) 否则用证条码号进行检索
            //      strPassword 密码。如果为null，表示不进行密码判断。注意，不是""
            //      strGetToken 是否要获得 token ，和有效期。 空 / day / month / year
            // return:
            //      -2  当前没有配置任何读者库，或者可以操作的读者库
            //      -1  error
            //      0   not found
            //      1   命中1条
            //      >1  命中多于1条
            int nRet = this.GetReaderRecXmlForLogin(
                // sessioninfo.Channels,
                channel,
                strLibraryCodeList,
                strLoginName,
                strPassword,
                nIndex,
                sessioninfo.ClientIP,
                strGetToken,
                out passwordExpired,
                out bTempPassword,
                out strXml,
                out strOutputPath,
                out timestamp,
                out strToken,
                out strError);
            if (nRet == -1 || nRet == -2)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("以登录名s登录时, 检索读者帐户记录出错s"),  // "以登录名 '{0}' 登录时, 检索读者帐户记录出错: {1}";
                    strLoginName,
                    strError);

                // "以登录名 '" + strLoginName + "' 登录时, 检索读者帐户记录出错: " + strError;
                return -1;
            }

            if (nRet == 0)
            {
                // text-level: 用户提示
                strError = this.GetString("帐户不存在或密码不正确") + " ..";    // "帐户不存在或密码不正确"
                return 0;   // 2015/12/4 注：这里不应返回 -1。因为返回 -1，会导致调主不去判断探测密码攻击
            }

            if (nRet > 1)
            {
                // 若未加以选择
                if (nIndex == -1)
                {
                    // text-level: 用户提示
                    strError = string.Format(this.GetString("以登录名s登录时, 因所匹配的帐户多于一个，无法登录"),  // "以登录名 '{0}' 登录时, 因所匹配的帐户多于一个，无法登录。可用证条码号作为登录名重新进行登录。"
                        strLoginName);
                    // "以登录名 '" + strLoginName + "' 登录时, 因所匹配的帐户多于一个，无法登录。可用证条码号作为登录名重新进行登录。";
                    return nRet;
                }
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                return -1;
            }

            // 检查 access 元素里面的星号
            string strAccess = DomUtil.GetElementText(readerdom.DocumentElement, "access");
            if (strAccess != null && strAccess.Trim() == "*")
            {
                strError = "读者记录中的存取定义(access 元素值)不允许使用 * 形态";
                return -1;
            }

            // 获得一个参考帐户
            Account accountref = null;
            // 从library.xml文件定义 获得一个帐户的信息
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetAccount("reader",
                out accountref,
                out strError);
            if (nRet == -1)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("获得reader参考帐户时出错s"),    // "获得reader参考帐户时出错: {0}"
                    strError);
                // "获得reader参考帐户时出错: " + strError;
                return -1;
            }

            if (nRet == 0)
                accountref = null;
            else
            {
                // 匹配 IP 地址
                if (string.IsNullOrEmpty(sessioninfo.ClientIP) == false)    // 2016/11/2
                {
                    List<string> temp = new List<string>();

                    bool bRet = accountref.MatchClientIP(sessioninfo.ClientIP,
                        ref temp,
                        out strError);
                    if (bRet == false)
                    {
                        if (temp.Count == 0)
                            return -1;

                        // 不允许，又没有可以替代的方式，就要返回出错了
                        if (Account.HasAlterBindingType(temp) == false)
                            return -1;

                        // 有替代的验证方式，先继续完成登录
                        alter_type_list.AddRange(temp);
                    }
                    else
                        alter_type_list.Add("ip");
                }

                // 星号表示不进行 router client ip 检查
                if (sessioninfo.RouterClientIP != "*"
                    // && alter_type_list.Count == 0
                    )
                {
                    List<string> temp = new List<string>();

                    // 匹配 dp2Router 前端的 IP 地址
                    bool bRet = accountref.MatchRouterClientIP(sessioninfo.RouterClientIP,
                        ref temp,
                        out strError);
                    if (bRet == false)
                    {
                        if (temp.Count == 0)
                            return -1;

                        // 不允许，又没有可以替代的方式，就要返回出错了
                        if (Account.HasAlterBindingType(temp) == false)
                            return -1;

                        // 否则继续完成登录
                        alter_type_list.AddRange(temp);
                    }
                    else
                        alter_type_list.Add("router_ip");
                }
            }

            Account account = new Account();
            account.LoginName = strLoginName;
            account.Password = strPassword; // TODO: 如果本函数用 strPassword == null 来调用，这里的 null 就不是实际的密码字符串了
            account.Rights = "";    // 是否需要缺省值?
            account.AccountLibraryCode = "";
            account.Access = "";
            if (accountref != null)
            {
                account.Rights = accountref.Rights;
                // account.LibraryCode = accountref.LibraryCode;
                account.Access = accountref.Access;
            }

            // 2021/7/5
            account.PasswordExpire = GetPasswordExpire(readerdom.DocumentElement);

            // 追加读者记录中定义的权限值
            string strAddRights = DomUtil.GetElementText(readerdom.DocumentElement, "rights");
            if (string.IsNullOrEmpty(strAddRights) == false)
            {
                // account.Rights += "," + strAddRights;
                // 2021/7/30
                account.Rights = MergeRights(account.Rights, strAddRights);
            }

            {
                // 2016/6/7
                // 如果读者记录状态有值，则需要从 account.Rights 中删除 patron 权限值
                // 反之则增加 patron 值
                string strState = DomUtil.GetElementText(readerdom.DocumentElement, "state");
                string strTemp = account.Rights;
                StringUtil.SetInList(ref strTemp, "patron", string.IsNullOrEmpty(strState));
                account.Rights = strTemp;
            }

            /*
            // 中途添加的一些权限内容
            List<string> adds = new List<string>();
            */

            // 2015/1/15
            if (string.IsNullOrEmpty(strToken) == false)
            {
                account.Rights += ",token:" + strToken;  // 如果保存在缓存中，如何决定何时失效?
                // adds.Add($"token:{strToken}");
            }

            // 追加读者记录中定义的存取定义
            string strAddAccess = DomUtil.GetElementText(readerdom.DocumentElement, "access");
            if (string.IsNullOrEmpty(strAddAccess) == false)
            {
                // TODO: 可以优化为，如果发现前一个字符串末尾已经有 ';' 就不加 ';' 了。
                account.Access += ";" + strAddAccess;
            }

            account.Type = "reader";
            account.Barcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");
            account.UserID = account.Barcode;

            // 2012/9/8
            // string strLibraryCode = "";
            nRet = this.GetLibraryCode(strOutputPath,
                    out strLibraryCode,
                    out strError);
            if (nRet == -1)
                return -1;
            account.AccountLibraryCode = strLibraryCode;

            // 2009/9/26 
            if (String.IsNullOrEmpty(account.Barcode) == true)
            {
                /*
                // 2020/3/4
                var refID = DomUtil.GetElementText(readerdom.DocumentElement,
"refID");
                if (string.IsNullOrEmpty(refID))
                {
                    strError = "读者记录中证条码号内容为空，并且参考 ID 内容也为空，登录失败";
                    return -1;
                }
                account.Barcode = "@refID:" + refID;
                account.UserID = account.Barcode;
                */
                // text-level: 用户提示
                strError = string.Format(this.GetString("读者记录中证条码号内容为空，登录失败"),    // "读者记录中证条码号内容为空，登录失败"
                    strError);
                return -1;
            }

            account.Name = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");
            // 2010/11/11
            account.DisplayName = DomUtil.GetElementText(readerdom.DocumentElement,
"displayName");
            account.PersonalLibrary = DomUtil.GetElementText(readerdom.DocumentElement,
"personalLibrary");


            // 2007/2/15 
            account.PatronDom = readerdom;
            account.ReaderDomLastTime = DateTime.Now;


            account.Location = strLocation;
            account.ReaderDomPath = strOutputPath;
            account.ReaderDomTimestamp = timestamp;

            sessioninfo.Account = account;

            strRights = account.RightsOrigin;   // 这里似乎可用 .Rights。原先这里用 .RightOrigin 的原因是不想带进去读者记录中添加的附加权限?
            /*
            if (adds.Count > 0)
                strRights += "," + StringUtil.MakePathList(adds, ",");
            */

            strOutputUserName = account.UserID; // 2011/7/29 读者证条码号

            // 把临时密码设置为正式密码
            if (bTempPassword == true)
            {
                // 不验证临时密码的合法性
                // 修改读者密码
                nRet = ChangeReaderPassword(
                    sessioninfo,
                    strOutputPath,
                    ref readerdom,
                    strPassword,    // TODO: 如果 strPassword == null 会怎么样？
                    _tempPasswordExpirePeriod,
                    false,
                    timestamp,
                    out byte[] output_timestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                timestamp = output_timestamp;

                account.PatronDom = readerdom;
                account.ReaderDomTimestamp = timestamp;
                // 2021/7/5
                account.PasswordExpire = GetPasswordExpire(readerdom.DocumentElement);
            }

            if (this.LoginCache != null && string.IsNullOrEmpty(account.LoginName) == false
                && account.Password != null)    // 防止 null password 进入 cache 引起其他问题 2014/12/20
            {
                DateTimeOffset offset = DateTimeOffset.Now.AddMinutes(20);
                this.LoginCache.Set(account.Barcode, account, offset);
            }

            return 1;
        }

        // readerdom发生变化后，刷新相关域
        public static void RefreshReaderAccount(ref Account account,
            XmlDocument readerdom)
        {
            account.DisplayName = DomUtil.GetElementText(readerdom.DocumentElement,
"displayName");
            account.PersonalLibrary = DomUtil.GetElementText(readerdom.DocumentElement,
"personalLibrary");
            account.ReaderDomLastTime = DateTime.Now;

        }

        // 清除当前已经登录的读者类型用户的读者记录DOM cache
        public void ClearLoginReaderDomCache(SessionInfo sessioninfo)
        {
            if (sessioninfo == null)
                return;

            if (sessioninfo.Account == null)
                return;

            if (sessioninfo.UserType != "reader")
                return;

            // 内存中内容已经被修改，要先保存DOM到数据库
            if (sessioninfo.Account.ReaderDomChanged == true)
            {
                // 此处的自动保存，和保存按钮矛盾了 -- 一旦刷新，就会自动保存。
                string strError = "";
                // 保存修改后的读者记录DOM
                // return:
                //      -1  error
                //      0   没有必要保存(changed标志为false)
                //      1   成功保存
                int nRet = SaveLoginReaderDom(sessioninfo,
                    out strError);
                // 遇到错误，如何报错?
            }

            sessioninfo.Account.PatronDom = null;
        }


        public void SetLoginReaderDomChanged(SessionInfo sessioninfo)
        {
            if (sessioninfo == null)
            {
                throw new Exception("sessioninfo = null");
            }

            if (sessioninfo.Account == null)
            {
                throw new Exception("sessioninfo.Account = null");
            }

            if (sessioninfo.Account.Type != "reader")
            {
                throw new Exception("sessioninfo.Account.Type != \"reader\"");
            }

            sessioninfo.Account.ReaderDomChanged = true;
        }

        // 保存修改后的读者记录DOM
        // return:
        //      -2  时间戳冲突
        //      -1  error
        //      0   没有必要保存(changed标志为false)
        //      1   成功保存
        public int SaveLoginReaderDom(SessionInfo sessioninfo,
            out string strError)
        {
            strError = "";
            if (sessioninfo == null)
            {
                strError = "sessioninfo = null";
                return -1;
            }

            if (sessioninfo.Account == null)
            {
                strError = "sessioninfo.Account = null";
                return -1;
            }

            if (sessioninfo.Account.Type != "reader")
            {
                strError = "sessioninfo.Account.Type != \"reader\"";
                return -1;
            }

            if (sessioninfo.Account.ReaderDomChanged == false)
                return 0;

            XmlDocument readerdom = sessioninfo.Account.PatronDom;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

#if NO
            int nRedoCount = 0;
            REDOSAVE:
#endif
            byte[] output_timestamp = null;
            string strOutputPath = "";
            string strOutputXml = "";

            long lRet = 0;

            /*
            // 保存读者记录
            lRet = channel.DoSaveTextRes(sessioninfo.Account.ReaderDomPath,
                readerdom.OuterXml,
                false,
                "content",
                sessioninfo.Account.ReaderDomTimestamp,   // timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            */
            string strExistingXml = "";
            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
            LibraryServerResult result = this.SetReaderInfo(sessioninfo,
                "change",
                sessioninfo.Account.ReaderDomPath,
                readerdom.OuterXml,
                "", // sessioninfo.Account.ReaderDomOldXml,    // strOldXml
                sessioninfo.Account.ReaderDomTimestamp,
                out strExistingXml,
                out strOutputXml,
                out strOutputPath,
                out output_timestamp,
                out kernel_errorcode);
            strError = result.ErrorInfo;
            lRet = result.Value;




            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    || kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    return -2;

#if NO
                // TODO: 重试不应该在这里做，而应该在调主那里做
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    && nRedoCount < 10)
                {
                    // 保存<preference>元素innerxml
                    string strPreferenceInnerXml = "";
                    XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
                    if (preference != null)
                        strPreferenceInnerXml = preference.InnerXml;

                    // 重新获得读者记录
                    // return:
                    //      -2  当前登录的用户不是reader类型
                    //      -1  出错
                    //      0   尚未登录
                    //      1   成功
                    int nRet = GetLoginReaderDom(sessioninfo,
                        out readerdom,
                        out strError);
                    if (nRet != 1)
                    {
                        strError = "保存读者记录时发现时间戳冲突，重新获取读者记录时又出错: " + strError;
                        return -1;
                    }

                    // 修改<preference>元素
                    if (String.IsNullOrEmpty(strPreferenceInnerXml) == false)
                    {
                        preference = readerdom.DocumentElement.SelectSingleNode("preference");
                        if (preference == null)
                        {
                            preference = readerdom.CreateElement("preference");
                            readerdom.DocumentElement.AppendChild(preference);
                        }

                        preference.InnerXml = strPreferenceInnerXml;
                    }

                    // 重新保存
                    nRedoCount++;
                    goto REDOSAVE;
                }
#endif

                return -1;
            }

            int nRet = LibraryApplication.LoadToDom(strOutputXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }
            sessioninfo.Account.PatronDom = readerdom;
            RefreshReaderAccount(ref sessioninfo.Account, readerdom);

            sessioninfo.Account.ReaderDomChanged = false;
            sessioninfo.Account.ReaderDomTimestamp = output_timestamp;

            return 1;
            /*
        ERROR1:
            return -1;
             * */
        }

        // 管理员身份保存修改后的读者记录DOM
        // return:
        //      -2  时间戳冲突
        //      -1  error
        //      0   没有必要保存(changed标志为false)
        //      1   成功保存
        public int SaveOtherReaderDom(SessionInfo sessioninfo,
            out string strError)
        {
            strError = "";
            if (sessioninfo == null)
            {
                strError = "sessioninfo = null";
                return -1;
            }

            if (sessioninfo.Account == null)
            {
                strError = "sessioninfo.Account = null";
                return -1;
            }

            if (sessioninfo.Account.Type == "reader")
            {
                strError = "sessioninfo.Account.Type == \"reader\"，而不是工作人员身份";
                return -1;
            }

            if (sessioninfo.Account.ReaderDomChanged == false)
                return 0;

            XmlDocument readerdom = sessioninfo.Account.PatronDom;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

#if NO
            int nRedoCount = 0;
        REDOSAVE:
#endif
            byte[] output_timestamp = null;
            string strOutputPath = "";
            string strOutputXml = "";

            long lRet = 0;


            /*
            // 保存读者记录
            lRet = channel.DoSaveTextRes(sessioninfo.Account.ReaderDomPath,
                readerdom.OuterXml,
                false,
                "content",
                sessioninfo.Account.ReaderDomTimestamp,   // timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
             * */
            string strExistingXml = "";
            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
            LibraryServerResult result = this.SetReaderInfo(sessioninfo,
                "change",
                sessioninfo.Account.ReaderDomPath,
                readerdom.OuterXml,
                "", // sessioninfo.Account.ReaderDomOldXml,    // strOldXml
                sessioninfo.Account.ReaderDomTimestamp,
                out strExistingXml,
                out strOutputXml,
                out strOutputPath,
                out output_timestamp,
                out kernel_errorcode);
            strError = result.ErrorInfo;
            lRet = result.Value;


            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    || kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    return -2;
#if NO
                // TODO: 重试不应该在这里做，而应该在调主那里做

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    && nRedoCount < 10)
                {
                    // 保存<preference>元素innerxml
                    string strPreferenceInnerXml = "";
                    XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
                    if (preference != null)
                        strPreferenceInnerXml = preference.InnerXml;

                    // 重新获得读者记录
                    // return:
                    //      -2  当前登录的用户不是reader类型
                    //      -1  出错
                    //      0   尚未登录
                    //      1   成功
                    int nRet = GetLoginReaderDom(sessioninfo,
                        out readerdom,
                        out strError);
                    if (nRet != 1)
                    {
                        strError = "保存读者记录时发现时间戳冲突，重新获取读者记录时又出错: " + strError;
                        return -1;
                    }

                    // 修改<preference>元素
                    if (String.IsNullOrEmpty(strPreferenceInnerXml) == false)
                    {
                        preference = readerdom.DocumentElement.SelectSingleNode("preference");
                        if (preference == null)
                        {
                            preference = readerdom.CreateElement("preference");
                            readerdom.DocumentElement.AppendChild(preference);
                        }

                        preference.InnerXml = strPreferenceInnerXml;
                    }

                    // 重新保存
                    nRedoCount++;
                    goto REDOSAVE;
                }
#endif

                return -1;
            }

            int nRet = LibraryApplication.LoadToDom(strOutputXml,
    out readerdom,
    out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }
            sessioninfo.Account.PatronDom = readerdom;
            RefreshReaderAccount(ref sessioninfo.Account, readerdom);

            sessioninfo.Account.ReaderDomChanged = false;
            sessioninfo.Account.ReaderDomTimestamp = output_timestamp;

            return 1;
            /*
        ERROR1:
            return -1;
             * */
        }


        // 获得当前session中已经登录的读者记录DOM
        // return:
        //      -2  当前登录的用户不是reader类型
        //      -1  出错
        //      0   尚未登录
        //      1   成功
        public int GetLoginReaderDom(SessionInfo sessioninfo,
            out XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            readerdom = null;

            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            if (sessioninfo.Account == null)
            {
                strError = "尚未登录";
                return 0;
            }

            if (sessioninfo.Account.Type != "reader")
            {
                strError = "当前登录的用户不是读者类型";
                return -2;
            }

            // 看看缓存的readerdom是否失效
            TimeSpan delta = DateTime.Now - sessioninfo.Account.ReaderDomLastTime;
            if (delta.TotalSeconds > 60
                && sessioninfo.Account.ReaderDomChanged == false)
            {
                sessioninfo.Account.PatronDom = null;
            }

            if (sessioninfo.Account.PatronDom == null)
            {
                string strBarcode = "";

                strBarcode = sessioninfo.Account.Barcode;
                if (strBarcode == "")
                {
                    strError = "帐户信息中读者证条码号为空，无法定位读者记录。";
                    goto ERROR1;
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;
                // 获得读者记录
                int nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strBarcode,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    goto ERROR1;

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

                sessioninfo.Account.ReaderDomPath = strOutputPath;
                sessioninfo.Account.ReaderDomTimestamp = timestamp;
                sessioninfo.Account.PatronDom = readerdom;
                sessioninfo.Account.ReaderDomLastTime = DateTime.Now;
            }
            else
            {
                readerdom = sessioninfo.Account.PatronDom;  // 沿用cache中的
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 管理员获得特定证条码号的读者记录DOM
        // return:
        //      -2  当前登录的用户不是librarian类型
        //      -1  出错
        //      0   尚未登录
        //      1   成功
        public int GetOtherReaderDom(SessionInfo sessioninfo,
            string strReaderBarcode,
            out XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            readerdom = null;

            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            if (sessioninfo.Account == null)
            {
                strError = "尚未登录";
                return 0;
            }

            if (sessioninfo.Account.Type == "reader")
            {
                strError = "当前登录的用户不是工作人员类型";
                return -2;
            }

            // 看看缓存的readerdom是否失效
            TimeSpan delta = DateTime.Now - sessioninfo.Account.ReaderDomLastTime;
            if (delta.TotalSeconds > 60
                && sessioninfo.Account.ReaderDomChanged == false)
            {
                sessioninfo.Account.PatronDom = null;
            }

            if (sessioninfo.Account.PatronDom == null
                || sessioninfo.Account.ReaderDomBarcode != strReaderBarcode)
            {
                string strBarcode = "";

                strBarcode = strReaderBarcode;
                if (strBarcode == "")
                {
                    strError = "strReaderBarcode参数为空，无法定位读者记录。";
                    goto ERROR1;
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;
                // 获得读者记录
                int nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strBarcode,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    goto ERROR1;

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

                sessioninfo.Account.ReaderDomBarcode = strReaderBarcode;
                sessioninfo.Account.ReaderDomPath = strOutputPath;
                sessioninfo.Account.ReaderDomTimestamp = timestamp;
                sessioninfo.Account.PatronDom = readerdom;
                sessioninfo.Account.ReaderDomLastTime = DateTime.Now;
            }
            else
            {
                Debug.Assert(strReaderBarcode == sessioninfo.Account.ReaderDomBarcode, "");
                readerdom = sessioninfo.Account.PatronDom;  // 沿用cache中的
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 验证读者密码。包括普通密码和临时密码
        // parameters:
        //      bTempPassword   [out] 是否为临时密码匹配成功
        // return:
        //      -1  error
        //      0   密码不正确
        //      1   密码正确
        public int VerifyReaderPassword(
            string strClientIP,
            XmlDocument readerdom,
            string strPassword,
            DateTime now,
            StringBuilder debugInfo,
            out bool bTempPassword,
            out bool passwordExpired,
            out string strError)
        {
            bTempPassword = false;
            int nRet = VerifyReaderNormalPassword(
                strClientIP,
                readerdom,
                strPassword,
                debugInfo,
                out passwordExpired,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                return 1;
            nRet = VerifyReaderTempPassword(readerdom,
                strPassword,
                now,
                out strError);
            if (nRet == 1)
                bTempPassword = true;
            return nRet;
        }

        // 验证读者密码。包括普通密码和临时密码，或者 token
        // return:
        //      -1  error
        //      0   密码不正确
        //      1   密码正确
        public int VerifyReaderPassword(
            string strClientIP,
            XmlDocument readerdom,
            string strPassword,
            DateTime now,
            StringBuilder debugInfo,
            out bool passwordExpired,
            out string strError)
        {
            int nRet = VerifyReaderNormalPassword(
                strClientIP,
                readerdom,
                strPassword,
                debugInfo,
                out passwordExpired,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                return 1;
            return VerifyReaderTempPassword(readerdom,
                strPassword,
                now,
                out strError);
        }

        // 验证读者普通密码或者 token
        // return:
        //      -1  error
        //      0   密码不正确
        //      1   密码正确
        public int VerifyReaderNormalPassword(
            string strClientIP,
            XmlDocument readerdom,
            string strPassword,
            StringBuilder debugInfo,
            out bool passwordExpired,
            out string strError)
        {
            strError = "";
            passwordExpired = false;

            debugInfo?.AppendLine($"enter VerifyReaderNormalPassword() strPassword={strPassword}");

            if (strPassword == null)
            {
                strError = "strPassword 参数值不应为 null";
                return -1;
            }

            // 2021/7/5
            // 检查密码失效期
            if (this._patronPasswordExpirePeriod != TimeSpan.MaxValue)
            {
                DateTime expire = GetPasswordExpire(readerdom.DocumentElement);
                if (DateTime.Now > expire)
                {
                    //strError = "密码已经失效";
                    //return -1;
                    passwordExpired = true;
                }
            }

            // 验证密码
            string type = null;
            string strSha1Text = DomUtil.GetElementText(readerdom.DocumentElement,
                "password",
                out XmlNode node);
            if (node != null)
                type = (node as XmlElement).GetAttribute("type");

            if (StringUtil.HasHead(strPassword, "token:") == true)
            {
                string strToken = strPassword.Substring("token:".Length);
                debugInfo?.AppendLine($"token={strToken}");
                return VerifyToken(
                    strClientIP,
                    strToken,
                    strSha1Text,
                    debugInfo,
                    out strError);
            }

            /*
            // 允许读者记录中有明文空密码
            if (String.IsNullOrEmpty(strSha1Text) == true)
            {
                if (strPassword != strSha1Text)
                {
                    strError = "密码不正确";
                    return 0;
                }

                return 1;
            }

            try
            {
                strPassword = Cryptography.GetSHA1(strPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            if (strPassword != strSha1Text)
            {
                strError = "密码不正确";
                return 0;
            }

            return 1;
            */
            // 2021/8/28
            // return:
            //      -1  出错
            //      0   不匹配
            //      1   匹配
            return LibraryServerUtil.MatchUserPassword(
                type,
                strPassword,
                strSha1Text,
                true,
                out strError);
        }

        // 验证读者临时密码
        // parameters:
        //      now 当前时间。本地时间
        // return:
        //      -1  error
        //      0   密码不正确
        //      1   密码正确
        public static int VerifyReaderTempPassword(
            XmlDocument readerdom,
            string strPassword,
            DateTime now,
            out string strError)
        {
            strError = "";

            XmlNode node = readerdom.DocumentElement.SelectSingleNode("password");
            if (node == null)
                return 0;

            // 失效期
            string strExpireTime = DomUtil.GetAttr(node,
                "tempPasswordExpire");
            if (string.IsNullOrEmpty(strExpireTime) == true)
                return 0;   // 不允许不使用失效期

            try
            {
                DateTime expire = DateTimeUtil.FromRfc1123DateTimeString(strExpireTime).ToLocalTime();

                if (now > expire)
                {
                    // 临时密码已经失效
                    return 0;
                }
            }
            catch (Exception)
            {
                strError = "临时密码失效期时间字符串 '" + strExpireTime + "' 格式不正确，应为 RFC1123 格式";
                return -1;
            }

            // 验证密码
            string strSha1Text = DomUtil.GetAttr(node,
                "tempPassword");
            /*
            // 允许读者记录中有明文空密码
            if (String.IsNullOrEmpty(strSha1Text) == true)
            {
                if (strPassword != strSha1Text)
                {
                    strError = "密码不正确";
                    return 0;
                }

                return 1;
            }

            try
            {
                strPassword = Cryptography.GetSHA1(strPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            if (strPassword != strSha1Text)
            {
                strError = "密码不正确";
                return 0;
            }

            return 1;
            */
            // 2021/8/28
            // return:
            //      -1  出错
            //      0   不匹配
            //      1   匹配
            return LibraryServerUtil.MatchUserPassword(
                null,
                strPassword,
                strSha1Text,
                true,
                out strError);
        }

        // 修改读者密码
        // return:
        //      -1  error
        //      0   成功
        public static int ChangeReaderPassword(
            XmlDocument readerdom,
            string strNewPassword,
            TimeSpan expireLength,
            ref XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (strNewPassword == null)
            {
                strError = "strNewPassword 参数值不应为 null。如果要设为空密码，可以使用 \"\"。";
                return -1;
            }

            /*
            try
            {
                strNewPassword = Cryptography.GetSHA1(strNewPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }
            */
            string new_type = "bcrypt";
            // 2021/8/28
            // return:
            //      -1  出错
            //      0   成功
            int nRet = LibraryServerUtil.SetUserPassword(
                new_type,
                strNewPassword,
                out strNewPassword,
                out strError);
            if (nRet == -1)
                return -1;

            XmlElement node = DomUtil.SetElementText(readerdom.DocumentElement,
                "password", strNewPassword);
            // 2013/11/2
            if (node != null)
            {
                // 清理临时密码
                DomUtil.SetAttr(node, "tempPassword", null);
                // 但失效期不清除

                node.SetAttribute("type", new_type);
            }

            // 2021/7/4
            // 设置密码失效期
            SetPatronPasswordExpire(node,
                expireLength,   // _patronPasswordExpirePeriod,
                DateTime.Now,
                out string strExpireTime);

            if (domOperLog != null)
            {
                Debug.Assert(domOperLog.DocumentElement != null, "");

                // 在日志中保存的是SHA1形态的密码。这样可以防止密码泄露。
                // 在日志恢复阶段, 把这个密码直接写入读者记录即可, 不需要再加工
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "newPassword", strNewPassword);

                // 2021/7/5
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "expire", strExpireTime);
            }

            return 0;
        }

        // 2021/7/5
        // 设置读者密码的 expire 属性
        // parameters:
        //      append  是否为追加方式。
        //              如果 == true，表示只有当 expire 属性不存在的时候才设置它
        //              如果 == false，表示无论如何都会重设它
        // return:
        //      false   没有发生修改
        //      true    发生了修改
        public static bool SetPatronPasswordExpire(XmlElement password_element,
            TimeSpan passwordExpirePeriod,
            DateTime now,
            out string strExpireTime,
            bool append = false)
        {
            strExpireTime = "";

            bool changed = false;
            if (passwordExpirePeriod == TimeSpan.MaxValue)
            {
                if (password_element.HasAttribute("expire"))
                {
                    password_element.RemoveAttribute("expire");
                    changed = true;
                }
            }
            else
            {
                var old_expire_value = password_element.GetAttribute("expire");
                if (append == true && string.IsNullOrEmpty(old_expire_value) == false)
                {
                    // (当 now == DateTime.MinValue 时)如果 expire 属性中已经有了值，不会修改
                }
                else
                {
                    strExpireTime = DateTimeUtil.Rfc1123DateTimeStringEx(now + passwordExpirePeriod); // 本地时间
                    password_element.SetAttribute("expire", strExpireTime);
                    changed = true;
                }
            }

            return changed;
        }

        // 修改读者临时密码
        // return:
        //      -1  error
        //      0   成功
        public int ChangeReaderTempPassword(
            XmlDocument readerdom,
            string strTempPassword,
            // string strExpireTime,
            ref XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            /*
            try
            {
                strTempPassword = Cryptography.GetSHA1(strTempPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }
            */
            string type = null;
            // 2021/8/28
            // return:
            //      -1  出错
            //      0   成功
            int nRet = LibraryServerUtil.SetUserPassword(
                type,
                strTempPassword,
                out strTempPassword,
                out strError);
            if (nRet == -1)
                return -1;

            XmlElement node = readerdom.DocumentElement.SelectSingleNode("password") as XmlElement;
            if (node == null)
            {
                node = readerdom.CreateElement("password");
                readerdom.DocumentElement.AppendChild(node);
            }

            string strCreateTime = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now);
            string strExpireTime = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now + _tempPasswordExpirePeriod); // 本地时间

            DomUtil.SetAttr(node,
                "tempPassword", strTempPassword);
            node.SetAttribute("tempPasswordCreateTime", strCreateTime); // 2017/10/27
            DomUtil.SetAttr(node,
                "tempPasswordExpire", strExpireTime);

            // 在日志中保存的是SHA1形态的密码。这样可以防止密码泄露。
            // 在日志恢复阶段, 把这个密码直接写入读者记录即可, 不需要再加工
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "tempPassword", strTempPassword);
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "tempPasswordExpire", strExpireTime);

            return 0;
        }

        #region 实用功能

        // 通过册条码号得知从属的种记录路径
        // parameters:
        //      strItemBarcode  册条码号
        //      strReaderBarcodeParam 借阅者证条码号。用于条码号重复的时候附加判断。
        // return:
        //      -1  error
        //      0   册记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPath(
            SessionInfo sessioninfo,
            string strItemBarcode,
            string strReaderBarcode,
            out string strBiblioRecPath,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            strBiblioRecPath = "";
            int nRet = 0;

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;

            int nResultCount = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // strItemBarcode命令状态 2006/12/24 
            if (strItemBarcode[0] == '@')
            {
                // 获得册记录，通过册记录路径

                string strLead = "@path:";
                if (strItemBarcode.Length <= strLead.Length)
                {
                    strError = "错误的检索词格式: '" + strItemBarcode + "'";
                    return -1;
                }
                string strPart = strItemBarcode.Substring(0, strLead.Length);

                if (strPart != strLead)
                {
                    strError = "不支持的检索词格式: '" + strItemBarcode + "'。目前仅支持'@path:'引导的检索词";
                    return -1;
                }

                string strItemRecPath = strItemBarcode.Substring(strLead.Length);

                {
                    string strItemDbName0 = ResPath.GetDbName(strItemRecPath);
                    // 需要检查一下数据库名是否在允许的实体库名之列
                    if (this.IsItemDbName(strItemDbName0) == false)
                    {
                        strError = "册记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName0 + "' 不在配置的实体库名之列，因此拒绝操作。";
                        return -1;
                    }
                }

                string strMetaData = "";
                // string strTempOutputPath = "";

                long lRet = channel.GetRes(strItemRecPath,
                    out strItemXml,
                    out strMetaData,
                    out item_timestamp,
                    out strOutputItemPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获取册记录 " + strItemRecPath + " 时发生错误: " + strError;
                    return -1;
                }
            }
            else // 普通条码号
            {

                // 获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strItemBarcode,
                    out strItemXml,
                    100,
                    out List<string> aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "册条码号为 '" + strItemBarcode + "' 的册记录没有找到";
                    return 0;
                }
                if (nRet == -1)
                    return -1;

                if (aPath.Count > 1)
                {
                    // bItemBarcodeDup = true; // 此时已经需要设置状态。虽然后面可以进一步识别出真正的册记录

                    // 构造strDupBarcodeList
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                    string strDupPathList = String.Join(",", pathlist);
                     * */
                    string strDupPathList = StringUtil.MakePathList(aPath);

                    List<string> aFoundPath = null;
                    List<byte[]> aTimestamp = null;
                    List<string> aItemXml = null;

                    if (String.IsNullOrEmpty(strReaderBarcode) == true)
                    {
                        // 如果没有给出读者证条码号参数
                        /*
                        strError = "册条码号为 '" + strItemBarcode + "' 册记录有 " + aPath.Count.ToString() + " 条，无法定位册记录。";

                        return -1;
                         * */
                        strOutputItemPath = aPath[0];
                        nResultCount = aPath.Count;
                        strWarning = "册条码号为 '" + strItemBarcode + "' 册记录有 " + aPath.Count.ToString() + " 条(记录路径为 " + strDupPathList + " )，在没有提供附加信息的情况下，无法准确定位册记录。现权且取出其中的第一条(" + aPath[0] + ")。";
                        goto GET_BIBLIO;
                    }

#if NO
                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        return -1;
                    }
#endif

                    // 从若干重复条码号的册记录中，选出其中符合当前读者证条码号的
                    // return:
                    //      -1  出错
                    //      其他    选出的数量
                    nRet = FindItem(
                        channel,
                        strReaderBarcode,
                        aPath,
                        true,   // 优化
                        out aFoundPath,
                        out aItemXml,
                        out aTimestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "选择重复条码号的册记录时发生错误: " + strError;
                        return -1;
                    }

                    if (nRet == 0)
                    {
                        strError = "册条码号 '" + strItemBarcode + "' 检索出的 " + aPath.Count + " 条记录(记录路径为 " + strDupPathList + " )中，没有任何一条其<borrower>元素表明了被读者 '" + strReaderBarcode + "' 借阅。";
                        return -1;
                    }

                    if (nRet > 1)
                    {
                        /*
                        string[] pathlist1 = new string[aFoundPath.Count];
                        aFoundPath.CopyTo(pathlist1);
                        string strDupPathList1 = String.Join(",", pathlist1);
                         * */
                        string strDupPathList1 = StringUtil.MakePathList(aFoundPath);

                        strError = "册条码号为 '" + strItemBarcode + "' 并且<borrower>元素表明为读者 '" + strReaderBarcode + "' 借阅的册记录有 " + aFoundPath.Count.ToString() + " 条(记录路径为 " + strDupPathList1 + " )，无法定位册记录。";
                        return -1;
                    }

                    Debug.Assert(nRet == 1, "");

                    strOutputItemPath = aFoundPath[0];
                    item_timestamp = aTimestamp[0];
                    strItemXml = aItemXml[0];

                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");
                    if (nRet == 1)
                    {
                        strOutputItemPath = aPath[0];
                        nResultCount = 1;
                        // strItemXml已经有册记录了
                    }
                }
            }

        GET_BIBLIO:

            string strItemDbName = "";  // 实体库名
            string strBiblioRecID = ""; // 种记录id

            // 如果需要从册记录中获得种记录路径

            /*
            // 准备工作: 映射数据库名
            nRet = this.GetGlobalCfg(sessioninfo.Channels,
                out strError);
            if (nRet == -1)
                return -1;
             * */

            strItemDbName = ResPath.GetDbName(strOutputItemPath);
            string strBiblioDbName = "";

            // 最好在应用启动时就做了？
            // 根据实体库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                return -1;
            }

            // 获得册记录中的<parent>字段
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录XML装载到DOM出错:" + ex.Message;
                return -1;
            }

            strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
            if (String.IsNullOrEmpty(strBiblioRecID) == true)
            {
                strError = "册记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                return -1;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;

            return nResultCount;
        }

        // 通过评注记录路径得知从属的种记录路径
        // parameters:
        // return:
        //      -1  error
        //      0   评注记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPathByCommentRecPath(
            SessionInfo sessioninfo,
            string strCommentRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            int nRet = 0;

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;

            {
                string strCommentDbName0 = ResPath.GetDbName(strCommentRecPath);
                // 需要检查一下数据库名是否在允许的实体库名之列
                if (this.IsCommentDbName(strCommentDbName0) == false)
                {
                    strError = "评注记录路径 '" + strCommentRecPath + "' 中的数据库名 '" + strCommentDbName0 + "' 不在配置的评注库名之列，因此拒绝操作。";
                    return -1;
                }
            }

            string strMetaData = "";
            // string strTempOutputPath = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.GetRes(strCommentRecPath,
                out strItemXml,
                out strMetaData,
                out item_timestamp,
                out strOutputItemPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取评注记录 " + strCommentRecPath + " 时发生错误: " + strError;
                return -1;
            }

            string strCommentDbName = "";  // 实体库名
            string strBiblioRecID = ""; // 种记录id

            // 如果需要从评注记录中获得种记录路径
            strCommentDbName = ResPath.GetDbName(strOutputItemPath);
            string strBiblioDbName = "";

            // 最好在应用启动时就做了？
            // 根据实体库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByCommentDbName(strCommentDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "评注库名 '" + strCommentDbName + "' 没有找到对应的书目库名";
                return -1;
            }

            // 获得评注记录中的<parent>字段
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "评注记录XML装载到DOM出错:" + ex.Message;
                return -1;
            }

            strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
            if (String.IsNullOrEmpty(strBiblioRecID) == true)
            {
                strError = "评注记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                return -1;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
            return 1;
        }

        // 2011/9/5
        // 通过册记录路径和parentid得知从属的种记录路径
        // parameters:
        // return:
        //      -1  error
        //      1   找到
        public int GetBiblioRecPathByItemRecPath(
            string strItemRecPath,
            string strParentID,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            int nRet = 0;

            {
                string strItemDbName0 = ResPath.GetDbName(strItemRecPath);
                // 需要检查一下数据库名是否在允许的实体库名之列
                if (this.IsItemDbName(strItemDbName0) == false)
                {
                    strError = "册记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName0 + "' 不在配置的实体库名之列，因此拒绝操作。";
                    return -1;
                }
            }

            string strItemDbName = "";  // 实体库名

            // 如果需要从册记录中获得种记录路径
            strItemDbName = ResPath.GetDbName(strItemRecPath);
            string strBiblioDbName = "";

            // 最好在应用启动时就做了？
            // 根据实体库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                return -1;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strParentID;
            return 1;
        }

        // 通过册记录路径得知从属的种记录路径
        // parameters:
        // return:
        //      -1  error
        //      0   册记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPathByItemRecPath(
            SessionInfo sessioninfo,
            string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            int nRet = 0;

            string strOutputItemPath = "";
            string strItemXml = "";
            byte[] item_timestamp = null;

            {
                string strItemDbName0 = ResPath.GetDbName(strItemRecPath);
                // 需要检查一下数据库名是否在允许的实体库名之列
                if (this.IsItemDbName(strItemDbName0) == false)
                {
                    strError = "册记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName0 + "' 不在配置的实体库名之列，因此拒绝操作。";
                    return -1;
                }
            }

            string strMetaData = "";
            // string strTempOutputPath = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.GetRes(strItemRecPath,
                out strItemXml,
                out strMetaData,
                out item_timestamp,
                out strOutputItemPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获取评注记录 " + strItemRecPath + " 时发生错误: " + strError;
                return -1;
            }

            string strItemDbName = "";  // 实体库名
            string strBiblioRecID = ""; // 种记录id

            // 如果需要从册记录中获得种记录路径
            strItemDbName = ResPath.GetDbName(strOutputItemPath);
            string strBiblioDbName = "";

            // 最好在应用启动时就做了？
            // 根据实体库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = this.GetBiblioDbNameByItemDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                return -1;
            }

            // 获得册记录中的<parent>字段
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "实体记录XML装载到DOM出错:" + ex.Message;
                return -1;
            }

            strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
            if (String.IsNullOrEmpty(strBiblioRecID) == true)
            {
                strError = "实体记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                return -1;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
            return 1;
        }

        #endregion

        // 包装版本
        // 检查路径中的库名，是不是实体库名
        // return:
        //      -1  error
        //      0   不是实体库名
        //      1   是实体库名
        public int CheckItemRecPath(string strItemRecPath,
            out string strError)
        {
            return CheckRecPath(strItemRecPath,
                "item",
                out strError);
        }

        // 检查路径中的库名，是不是特定类型的数据库名
        // return:
        //      -1  error
        //      0   不是所要求类型的
        //      1   是要求类型的
        public int CheckRecPath(string strItemRecPath,
            string strDbTypeList,
            out string strError)
        {
            strError = "";

            string strTempDbName = ResPath.GetDbName(strItemRecPath);

            // 2008/10/16 
            if (String.IsNullOrEmpty(strTempDbName) == true)
            {
                strError = "从路径 '" + strItemRecPath + "' 中无法抽出库名部分...";
                return -1;
            }

            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                // item
                if (strTempDbName == this.ItemDbs[i].DbName)
                {
                    if (StringUtil.IsInList("item", strDbTypeList) == true)
                        return 1;
                }

                // order
                if (strTempDbName == this.ItemDbs[i].OrderDbName)
                {
                    if (StringUtil.IsInList("order", strDbTypeList) == true)
                        return 1;
                }

                // issue
                if (strTempDbName == this.ItemDbs[i].IssueDbName)
                {
                    if (StringUtil.IsInList("issue", strDbTypeList) == true)
                        return 1;
                }

                // comment
                if (strTempDbName == this.ItemDbs[i].CommentDbName)
                {
                    if (StringUtil.IsInList("comment", strDbTypeList) == true)
                        return 1;
                }

                // biblio
                if (strTempDbName == this.ItemDbs[i].BiblioDbName)
                {
                    if (StringUtil.IsInList("biblio", strDbTypeList) == true)
                        return 1;
                }
            }
            strError = "路径 '" + strItemRecPath + "' 中包含的数据库名 '" + strTempDbName + "' 不在已定义的类型 " + strDbTypeList + " 库名之列。";
            return 0;
        }

        #region APIs



        // 看看文件名是不是以.cs结尾
        public static bool IsCsFileName(string strFileName)
        {
            strFileName = strFileName.Trim().ToLower();
            int nRet = strFileName.LastIndexOf(".cs");
            if (nRet == -1)
                return false;
            if (nRet + 3 == strFileName.Length)
                return true;
            return false;
        }



#if NOOOOOOOOOOOOOO

        // 暂没有用
        // 在字符串末尾追加一个新的操作者事项
        // 操作者1:操作类型1<操作时间1>;操作者2:操作类型2<操作时间2>;...
        // parameters:
        //      strOpertimeRfc1123 操作时间。必须为RFC1123形态。如果为null，表示自动取当前时间
        public static int AppendOperatorHistory(ref string strValue,
            string strOperator,
            string strOperation,
            string strOpertimeRfc1123,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strOperator) == true)
            {
                strError = "strOperator参数不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strOperation) == true)
            {
                strError = "strOperation参数不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strValue) == true)
                strValue = "";
            else
                strValue += ";";

            strValue += strOperator;
            strValue += ":";
            strValue += strOperation;
            strValue += "<";
            if (String.IsNullOrEmpty(strOpertimeRfc1123) == true)
            {
                strValue += DateTimeUtil.Rfc1123DateTimeString(? this.Clock().UtcNow /*DateTime.UtcNow*/);
            }
            else
            {
                strValue += strOpertimeRfc1123;
            }
            strValue += ">";

            return 0;
        }
#endif


        // 修改读者密码
        // Result.Value -1出错 0旧密码不正确 1旧密码正确,已修改为新密码
        // 权限: 
        //		工作人员或者读者，必须有changereaderpassword权限
        //		如果为读者, 附加限制还只能修改属于自己的密码
        public LibraryServerResult ChangeReaderPassword(
            SessionInfo sessioninfo,
            string strReaderBarcode,
            string strReaderOldPassword,
            string strReaderNewPassword)
        {
            LibraryServerResult result = new LibraryServerResult();

            // 是否已经登录？
            bool loggedIn = false;
            if (sessioninfo != null && sessioninfo.UserID == "")
                loggedIn = false;
            else
                loggedIn = true;

            // 权限判断
            if (loggedIn)
            {
                // 权限字符串
                if (StringUtil.IsInList("changereaderpassword", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改读者密码被拒绝。不具备 changereaderpassword 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    if (strReaderBarcode != sessioninfo.Account.Barcode)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者密码被拒绝。作为读者只能修改自己的密码";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            string strError = "";

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("ChangeReaderPassword 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;

                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "证条码号为 '" + strReaderBarcode + "' 的读者不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "获得证条码号为 '" + strReaderBarcode + "' 的读者记录时出错: " + strError;
                    goto ERROR1;
                }

                if (nRet > 1)
                {
                    strError = "系统错误: 证条码号为 '" + strReaderBarcode + "' 的读者记录多于一个";
                    goto ERROR1;
                }

                string strLibraryCode = "";

                if (loggedIn)
                {
                    // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                    if (String.IsNullOrEmpty(strOutputPath) == false)
                    {
                        // 检查当前操作者是否管辖这个读者库
                        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                        if (this.IsCurrentChangeableReaderPath(strOutputPath,
                sessioninfo.LibraryCodeList,
                out strLibraryCode) == false)
                        {
                            strError = "读者记录路径 '" + strOutputPath + "' 从属的读者库不在当前用户管辖范围内";
                            goto ERROR1;
                        }
                    }
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 2021/7/8
                // 合成读者记录的最终权限
                nRet = GetReaderRights(
                    readerdom,
                    out string rights,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strExistingBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");

                // 如果是读者身份, 或者通过参数 strReaderOldPassword (非null)要求，需要验证旧密码
                if ((loggedIn && sessioninfo.UserType == "reader")
                    || loggedIn == false
                    || strReaderOldPassword != null)
                {


                    string strClientIP = sessioninfo.ClientIP;
                    nRet = this.UserNameTable.BeforeLogin(strExistingBarcode,
strClientIP,
out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 验证读者密码
                    // return:
                    //      -1  error
                    //      0   密码不正确
                    //      1   密码正确
                    nRet = this.VerifyReaderPassword(
                        sessioninfo.ClientIP,
                        readerdom,
                        strReaderOldPassword,
                        this.Clock.Now,
                        null,
                        out bool passwordExpired,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0 || nRet == 1)
                    {
                        // parameters:
                        //      nLoginResult    1:成功 0:用户名或密码不正确 -1:出错
                        string strLogText = this.UserNameTable.AfterLogin(strExistingBarcode,
                            strClientIP,
                            nRet);
                        if (string.IsNullOrEmpty(strLogText) == false)
                            this.WriteErrorLog("!!! " + strLogText);
                    }


                    if (nRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "旧密码不正确。";
                        return result;
                    }
                    else
                    {
                        result.Value = 1;
                    }

                    // 2021/7/30
                    // 权限字符串
                    if (StringUtil.IsInList("changereaderpassword", rights) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者密码被拒绝。不具备 changereaderpassword 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2021/7/8
                    if (StringUtil.IsInList("denychangemypassword", rights))
                    {
                        result.Value = -1;
                        result.ErrorInfo = "读者 " + strReaderBarcode + " 因被设定了 denychangemypassword 权限，不能修改自己的密码";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                /*
                TimeSpan expireLength = _patronPasswordExpirePeriod;
                if (StringUtil.IsInList("neverexpire", rights))
                    expireLength = TimeSpan.MaxValue;
                */
                // 条件化的失效期，考虑了 rights 因素
                TimeSpan expireLength = GetConditionalPatronPasswordExpireLength(readerdom);

                byte[] output_timestamp = null;
                nRet = ChangeReaderPassword(
                    sessioninfo,
                    strOutputPath,
                    ref readerdom,
                    strReaderNewPassword,
                    expireLength,
                    true,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 清除 LoginCache
                this.ClearLoginCache(strExistingBarcode);

                result.Value = 1;   // 成功
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("ChangeReaderPassword 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 修改读者密码
        // parameters:
        //      readerdom [in,out] 读者记录 XMLDOM，可能会因为时间戳不匹配而被重新装载
        int ChangeReaderPassword(
            SessionInfo sessioninfo,
            string strReaderRecPath,
            ref XmlDocument readerdom,
            string strReaderNewPassword,
            TimeSpan expireLength,
            bool validatePassword,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

#if NO
            if (strReaderNewPassword == null)
            {
                strError = "strReaderNewPassword 参数值不应为 null。如果要设为空密码，可以使用 \"\"。";
                return -1;
            }
#endif

            int nRet = 0;

            string strLibraryCode = "";

            // 获得读者库的馆代码
            // return:
            //      -1  出错
            //      0   成功
            nRet = GetLibraryCode(
                strReaderRecPath,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;

            if (validatePassword)
            {
                // 2021/7/5
                // 验证读者密码是否合法
                // return:
                //      -1  出错
                //      0   不合法(原因在 strError 中返回)
                //      1   合法
                nRet = ValidatePatronPassword(
            readerdom.DocumentElement,
            strReaderNewPassword,
            _patronPasswordStyle,
            true,
            out strError);
                if (nRet != 1)
                    return -1;
            }

            // 准备日志DOM
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // 读者所在的馆代码
            DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                "changeReaderPassword");

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            int nRedoCount = 0;
        REDO:

            // 修改读者密码
            // return:
            //      -1  error
            //      0   成功
            nRet = LibraryApplication.ChangeReaderPassword(
                readerdom,
                strReaderNewPassword,
                expireLength,
                ref domOperLog,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // byte[] output_timestamp = null;

            // 保存读者记录
            long lRet = channel.DoSaveTextRes(strReaderRecPath,
                readerdom.OuterXml,
                false,
                "content", // "content,ignorechecktimestamp",
                timestamp,   // timestamp,
                out output_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                    && nRedoCount < 10)
                {
                    // 重新装载读者记录
                    timestamp = null;

                    lRet = channel.GetRes(strReaderRecPath,
                        out string strXml,
                        out string strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "保存读者记录 '" + strReaderRecPath + "' 时遇到时间戳不匹配，重新装载的时候又遇到出错: " + strError;
                        goto ERROR1;
                    }

                    readerdom = new XmlDocument();
                    try
                    {
                        readerdom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "重新装载读者记录进入 XMLDOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }
                    nRedoCount++;
                    goto REDO;
                }
                goto ERROR1;
            }

            // ChangeReaderPassword() API 恢复动作
            /*
    <root>
      <operation>changeReaderPassword</operation> 
      <readerBarcode>...</readerBarcode>	读者证条码号
      <newPassword>5npAUJ67/y3aOvdC0r+Dj7SeXGE=</newPassword> 
      <operator>test</operator> 
      <operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 
      <readerRecord recPath='...'>...</readerRecord>	最新读者记录
    </root>
             * */

            // 写入日志
            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode"); // 2019/4/25 修改 bug。原来为 domOperLog.Document
            string strNewPassword = DomUtil.GetElementText(readerdom.DocumentElement, "password"); // 2019/4/25 增加

            // 读者证条码号
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "readerBarcode", strReaderBarcode);

            // 新密码(hash 形态)
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "newPassword", strNewPassword);

            // 读者记录
            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "readerRecord",
                readerdom.OuterXml);
            // 读者记录路径
            DomUtil.SetAttr(node, "recPath", strOutputPath);

            string strOperTime = this.Clock.GetClock();
            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);   // 操作者
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTime);   // 操作时间

            nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "ChangeReaderPassword() API 写入日志时发生错误: " + strError;
                goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
        }

        /*
        // 验证读者密码是否合法
        // return:
        //      -1  出错
        //      0   不合法(原因在 strError 中返回)
        //      1   合法
        public static int ValidatePatronPassword(
    XmlElement root,
    string password,
    out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(this._patronPasswordStyle))
                return 1;
            return ValidatePatronPassword(
    root,
    password,
    this._patronPasswordStyle,
    out strError);
        }
        */

        // 验证读者密码字符串的合法性
        // parameters:
        //      style   风格。style-1 为第一种密码风格
        // return:
        //      -1  出错
        //      0   不合法(原因在 strError 中返回)
        //      1   合法
        public static int ValidatePatronPassword(
            XmlElement root,
            string password,
            string style,
            bool check_old_password,
            out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();

            // 风格 1
            /*
1. 8个字符，且不能是顺序、逆序或相同
2. 数字加字母组合
3. 密码和用户名不可以一样
4. 临时密码不可以当做正式密码使用
5. 新旧密码不能一样
             * */
            if (StringUtil.IsInList("style-1", style))
            {
                if (string.IsNullOrEmpty(password))
                {
                    errors.Add("密码不允许为空");
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(password) == true
                    || password.Length < 8)
                    errors.Add("密码字符数不能小于 8");

                if (IsSequence(password))
                    errors.Add("密码内容不能为顺序字符");

                if (ContainsDigit(password) == false || ContainsLetter(password) == false)
                    errors.Add("密码内容必须同时包含数字和字母");

                var userNameList = GetUserNameValues(root);

                if (userNameList.IndexOf(password) != -1)
                    errors.Add("密码不能和证条码号、姓名等相同");

                if (check_old_password)
                {
                    // 和当前存在的旧密码比较
                    var old_password_hashed = GetPasswordValue(root, out string type);
                    if (string.IsNullOrEmpty(old_password_hashed) == false)
                    {
                        // 验证密码
                        // return:
                        //      -1  出错
                        //      0   不匹配
                        //      1   匹配
                        int nRet = LibraryServerUtil.MatchUserPassword(
                            type,
                            password,
                            old_password_hashed,
                            true,
                            out _);
                        if (nRet == 1)
                            errors.Add("密码不能和旧密码相同");
                    }
                }

                if (errors.Count > 0)
                    goto ERROR1;
            }

            strError = "密码合法";
            return 1;
        ERROR1:
            strError = $"密码不合法: {StringUtil.MakePathList(errors, "; ")}";
            return 0;
        }

        public static List<string> GetUserNameValues(XmlElement root)
        {
            List<string> names = new List<string>();
            string name = DomUtil.GetElementText(root, "name");
            if (string.IsNullOrEmpty(name) == false)
                names.Add(name);

            string barcode = DomUtil.GetElementText(root, "barcode");
            if (string.IsNullOrEmpty(barcode) == false)
                names.Add(barcode);

            return names;
        }

        // 修改读者临时密码
        // parameters:
        //      timeExpire  临时密码失效时间
        //      readerdom [in,out] 读者记录 XMLDOM，可能会因为时间戳不匹配而被重新装载
        int ChangeReaderTempPassword(
            SessionInfo sessioninfo,
            string strReaderRecPath,
            XmlDocument readerdom,
            string strReaderTempPassword,
            // string strExpireTime,
            byte[] timestamp,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

            int nRet = 0;


            // 获得读者库的馆代码
            // return:
            //      -1  出错
            //      0   成功
            nRet = GetLibraryCode(
                strReaderRecPath,
                out string strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;

            // 准备日志DOM
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // 读者所在的馆代码

            DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                "changeReaderTempPassword");

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            int nRedoCount = 0;
        REDO:

            // 修改读者临时密码
            // return:
            //      -1  error
            //      0   成功
            nRet = this.ChangeReaderTempPassword(
                readerdom,
                strReaderTempPassword,
                // strExpireTime,
                ref domOperLog,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // byte[] output_timestamp = null;
            string strOutputPath = "";

            // 保存读者记录
            long lRet = channel.DoSaveTextRes(strReaderRecPath,
                readerdom.OuterXml,
                false,
                "content", // "content,ignorechecktimestamp",
                timestamp,   // timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
    && nRedoCount < 10)
                {
                    // 重新装载读者记录
                    string strXml = "";
                    string strMetaData = "";
                    timestamp = null;

                    lRet = channel.GetRes(strReaderRecPath,
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "保存读者记录 '" + strReaderRecPath + "' 时遇到时间戳不匹配，重新装载的时候又遇到出错: " + strError;
                        goto ERROR1;
                    }

                    readerdom = new XmlDocument();
                    try
                    {
                        readerdom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "重新装载读者记录进入 XMLDOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }
                    nRedoCount++;
                    goto REDO;
                }

                goto ERROR1;
            }

            // 写入日志
            string strReaderBarcode = DomUtil.GetElementText(domOperLog.DocumentElement, "barcode");

            // 读者证条码号
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "readerBarcode", strReaderBarcode);

            // 读者记录
            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "readerRecord",
                readerdom.OuterXml);

            // 读者记录路径
            DomUtil.SetAttr(node, "recPath", strOutputPath);

            string strOperTime = this.Clock.GetClock();
            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);   // 操作者
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTime);   // 操作时间

            nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "ResetPassword() API 写入日志时发生错误: " + strError;
                goto ERROR1;
            }

            // this.LoginCache.Remove(strReaderBarcode);   // 及时失效登录缓存
            this.ClearLoginCache(strReaderBarcode);   // 及时失效登录缓存
            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        // 展开权限字符串为原始权限定义形态
        public static string ExpandRightString(string strOriginRight)
        {
            string strResult = strOriginRight;

            return strResult;
        }

        // 包装版本
        public string GetBarcodesSummary(SessionInfo sessioninfo,
            string strBarcodes,
            string strStyle,
            string strOtherParams)
        {
            return GetBarcodesSummary(
            sessioninfo,
            strBarcodes,
            "",
            strStyle,
            strOtherParams);
        }

        // 获得一系列册的摘要字符串
        // 这是满足本地WebControl的版本
        // paramters:
        //      strStyle    风格。逗号间隔的列表。html text
        //      strOtherParams  暂时没有使用
        public string GetBarcodesSummary(
            SessionInfo sessioninfo,
            string strBarcodes,
            string strArrivedItemBarcode,
            string strStyle,
            string strOtherParams)
        {
            string strSummary = "";

            if (strOtherParams == null)
                strOtherParams = "";

            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";

            string strPrevBiblioRecPath = "";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int j = 0; j < barcodes.Length; j++)
            {
                string strBarcode = barcodes[j];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // 获得摘要
                string strOneSummary = "";
                string strBiblioRecPath = "";

                // 2012/3/28
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    return "get channel error";
                }

                LibraryServerResult result = this.GetBiblioSummary(sessioninfo,
                    channel,
    strBarcode,
    null,
    strPrevBiblioRecPath,   // 前一个path
    out strBiblioRecPath,
    out strOneSummary);
                if (result.Value == -1 || result.Value == 0)
                    strOneSummary = result.ErrorInfo;

                if (strOneSummary == ""
                    && strPrevBiblioRecPath == strBiblioRecPath)
                    strOneSummary = "(同上)";

                if (StringUtil.IsInList("html", strStyle) == true)
                {
                    /*
                    string strBarcodeLink = "<a href='" + this.OpacServerUrl + "/book.aspx?barcode=" + strBarcode +
                        (bForceLogin == true ? "&forcelogin=userid" : "")
                        + "' " + strOtherParams + " >" + strBarcode + "</a>";
                    */

                    string strBarcodeLink = "<a "
    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
    + " href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\"  onmouseover=\"window.external.HoverItemProperty(this.innerText);\">" + strBarcode + "</a>";


                    strSummary += strBarcodeLink + " : " + strOneSummary + "<br/>";
                }
                else
                {
                    strSummary += strBarcode + " : " + strOneSummary + "<br/>";
                }

                strPrevBiblioRecPath = strBiblioRecPath;
            }

            return strSummary;
        }

#if NO
        // 获得一系列册的摘要字符串
        // 
        // paramters:
        //      strStyle    风格。逗号间隔的列表。如果包含html text表示格式。forcelogin
        //      strOtherParams  <a>命令中其余的参数。例如" target='_blank' "可以用来打开新窗口
        public string GetBarcodesSummary(
            SessionInfo sessioninfo,
            string strBarcodes,
            string strArrivedItemBarcode,
            string strStyle,
            string strOtherParams)
        {
            string strSummary = "";

            if (strOtherParams == null)
                strOtherParams = "";

            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";

            bool bForceLogin = false;
            if (StringUtil.IsInList("forcelogin", strStyle) == true)
                bForceLogin = true;

            string strPrevBiblioRecPath = "";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int j = 0; j < barcodes.Length; j++)
            {
                string strBarcode = barcodes[j];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // 获得摘要
                string strOneSummary = "";
                string strBiblioRecPath = "";

                LibraryServerResult result = this.GetBiblioSummary(sessioninfo,
    strBarcode,
    null,
    strPrevBiblioRecPath,   // 前一个path
    out strBiblioRecPath,
    out strOneSummary);
                if (result.Value == -1 || result.Value == 0)
                    strOneSummary = result.ErrorInfo;

                if (strOneSummary == ""
                    && strPrevBiblioRecPath == strBiblioRecPath)
                    strOneSummary = "(同上)";

                if (StringUtil.IsInList("html", strStyle) == true)
                {
                    /*
                    string strBarcodeLink = "<a href='" + this.OpacServerUrl + "/book.aspx?barcode=" + strBarcode +
                        (bForceLogin == true ? "&forcelogin=userid" : "")
                        + "' " + strOtherParams + " >" + strBarcode + "</a>";
                    */

                    string strBarcodeLink = "<a "
    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
    + " href='" + this.OpacServerUrl + "/book.aspx?barcode=" + strBarcode +
    (bForceLogin == true ? "&forcelogin=userid" : "")
    + "' " + strOtherParams + " >" + strBarcode + "</a>";


                    strSummary += strBarcodeLink + " : " + strOneSummary + "<br/>";
                }
                else
                {
                    strSummary += strBarcode + " : " + strOneSummary + "<br/>";
                }

                strPrevBiblioRecPath = strBiblioRecPath;
            }

            return strSummary;
        }

#endif

        static List<XmlNode> MatchTableNodes(XmlNode root,
            string strName,
            string strDbName)
        {
            List<XmlNode> results = new List<XmlNode>();

            XmlNodeList nodes = root.SelectNodes("table[@name='" + strName + "']");
            if (nodes.Count == 0)
                return results;

            for (int i = 0; i < nodes.Count; i++)
            {
                string strCurDbName = DomUtil.GetAttr(nodes[i], "dbname");
                if (String.IsNullOrEmpty(strCurDbName) == true
                    && String.IsNullOrEmpty(strDbName) == true)
                {
                    results.Add(nodes[i]);
                    continue;
                }

                if (strCurDbName == strDbName)
                    results.Add(nodes[i]);
            }

            return results;
        }

        // TODO: 需要进行针对分馆用户的改造
        // 修改值列表
        // 2008/8/21 
        // parameters:
        //      strAction   "new" "change" "overwirte" "delete"
        // return:
        //      -1  error
        //      0   not change
        //      1   changed
        public int SetValueTable(string strAction,
            string strName,
            string strDbName,
            string strValue,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strName) == true)
            {
                strError = "strName参数值不能为空";
                return -1;
            }
            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//valueTables");
            if (root == null)
            {
                root = this.LibraryCfgDom.CreateElement("valueTables");
                this.LibraryCfgDom.DocumentElement.AppendChild(root);
                this.Changed = true;
            }

            if (strAction == "new")
            {
                List<XmlNode> nodes = MatchTableNodes(root,
                    strName,
                    strDbName);
                if (nodes.Count > 0)
                {
                    strError = "name为 '" + strName + "' dbname为 '" + strDbName + "' 的值列表事项已经存在";
                    return -1;
                }

                XmlNode new_node = root.OwnerDocument.CreateElement("table");
                root.AppendChild(new_node);

                DomUtil.SetAttr(new_node, "name", strName);
                DomUtil.SetAttr(new_node, "dbname", strDbName);

                new_node.InnerText = strValue;
                this.Changed = true;
                return 1;
            }
            else if (strAction == "delete")
            {
                List<XmlNode> nodes = MatchTableNodes(root,
                    strName,
                    strDbName);
                if (nodes.Count == 0)
                {
                    strError = "name为 '" + strName + "' dbname为 '" + strDbName + "' 的值列表事项不存在";
                    return 0;
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].ParentNode.RemoveChild(nodes[i]);
                }

                this.Changed = true;
                return 1;
            }
            else if (strAction == "change")
            {
                List<XmlNode> nodes = MatchTableNodes(root,
                    strName,
                    strDbName);
                if (nodes.Count == 0)
                {
                    strError = "name为 '" + strName + "' dbname为 '" + strDbName + "' 的值列表事项不存在";
                    return 0;
                }

                XmlNode exist_node = nodes[0];
                for (int i = 1; i < nodes.Count; i++)
                {
                    nodes[i].ParentNode.RemoveChild(nodes[i]);
                }

                exist_node.InnerText = strValue;
                this.Changed = true;
                return 1;
            }
            else if (strAction == "overwrite")
            {
                List<XmlNode> nodes = MatchTableNodes(root,
                    strName,
                    strDbName);
                if (nodes.Count == 0)
                {
                    XmlNode new_node = root.OwnerDocument.CreateElement("table");
                    root.AppendChild(new_node);

                    DomUtil.SetAttr(new_node, "name", strName);
                    DomUtil.SetAttr(new_node, "dbname", strDbName);

                    new_node.InnerText = strValue;
                }
                else
                {
                    XmlNode exist_node = nodes[0];
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        nodes[i].ParentNode.RemoveChild(nodes[i]);
                    }

                    exist_node.InnerText = strValue;
                }
                this.Changed = true;
                return 1;
            }
            else
            {
                strError = "未知的strAction值 '" + strAction + "'";
                return -1;
            }
        }

        // 从字符串列表中，过滤出那些属于指定馆代码范围的字符串
        static string[] FilterValues(string strLibraryCodeList,
            string strValueList)
        {
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                return strValueList.Trim().Split(new char[] { ',' });
            }

            List<string> results = new List<string>();
            List<string> values = StringUtil.FromListString(strValueList);
            foreach (string s in values)
            {
                string strLibraryCode = "";
                string strPureName = "";

                // 解析日历名
                ParseCalendarName(s,
            out strLibraryCode,
            out strPureName);

                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    continue;

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == true)
                    results.Add(s);
            }

            if (results.Count == 0)
                return new string[0];

            string[] array = new string[results.Count];
            results.CopyTo(array);
            return array;
        }

#if NO
        // 获得值列表
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strTableName    表名。如果为空，表示任意name参数值均匹配
        //      strDbName   数据库名。如果为空，表示任意dbname参数值均匹配。
        public string[] GetValueTable(
            string strLibraryCodeList,
            string strTableNameParam,
            string strDbNameParam)
        {
            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//valueTables");
            if (root == null)
                return null;

            if (strTableNameParam == "location")
            {
            }
            else
            {
                // 不过滤
                strLibraryCodeList = "";
            }

            // 2009/2/15 changed
            if (String.IsNullOrEmpty(strDbNameParam) == false)
            {
                XmlNode default_node = null;

                XmlNodeList nodes = root.SelectNodes("table[@name='" + strTableNameParam + "']");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode table = nodes[i];

                    // string strName = DomUtil.GetAttr(table, "name");
                    string strDbName = DomUtil.GetAttr(table, "dbname");

                    if (String.IsNullOrEmpty(strDbName) == true
                        && default_node == null)    // 认排列在最前面的一个缺省元素
                    {
                        default_node = table;
                        continue;
                    }


                    if (StringUtil.IsInList(strDbNameParam, strDbName) == true)
                    {
                        // 命中
                        // return table.InnerText.Trim().Split(new char[] { ',' });
                                // 从字符串列表中，过滤出那些属于指定馆代码范围的字符串
                        return FilterValues(strLibraryCodeList,
                                table.InnerText);
                    }
                }

                // 虽然"dbname"没有命中，但是可以返回缺省的值(dbname属性为空的)
                if (default_node != null)
                {
                    // return default_node.InnerText.Trim().Split(new char[] { ',' });
                    return FilterValues(strLibraryCodeList,
        default_node.InnerText);
                }

                return null;
            }
            else
            {
                // 没有dbname参数的情形
                XmlNodeList nodes = root.SelectNodes("table[@name='" + strTableNameParam + "']");
                if (nodes.Count == 0)
                    return null;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode table = nodes[i];

                    // string strName = DomUtil.GetAttr(table, "name");
                    string strDbName = DomUtil.GetAttr(table, "dbname");

                    // 优先选择一个dbname属性为空的元素
                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        // 命中
                        // return table.InnerText.Trim().Split(new char[] { ',' });
                        return FilterValues(strLibraryCodeList,
                            table.InnerText);
                    }
                }

                // 否则返回“没有找到”，尽管还有其他dbname属性有值的元素
                return null;
            }


            /*
            XmlNodeList nodes = root.SelectNodes("table");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode table = nodes[i];

                string strName = DomUtil.GetAttr(table, "name");
                string strDbName = DomUtil.GetAttr(table, "dbname");

                if (String.IsNullOrEmpty(strTableNameParam) == false)
                {
                    if (String.IsNullOrEmpty(strName) == false
                        && strTableNameParam != strName)
                        continue;
                }
                if (String.IsNullOrEmpty(strDbNameParam) == false)
                {
                    if (String.IsNullOrEmpty(strDbName) == false
                        && strDbNameParam != strDbName)
                        continue;
                }

                // 命中
                string strValue = table.InnerText.Trim();
                return strValue.Split(new char[] {','});
            }
             * */

            // return null;    // not found
        }
#endif
        // 获得一个图书馆代码下的值列表
        // parameters:
        //      strLibraryCode  馆代码
        //      strTableName    表名。如果为空，表示任意name参数值均匹配
        //      strDbName   数据库名。如果为空，表示任意dbname参数值均匹配。
        public List<string> GetOneLibraryValueTable(
            string strLibraryCode,
            string strTableNameParam,
            string strDbNameParam)
        {
            List<string> results = new List<string>();

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//valueTables");
            if (root == null)
                return results;

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return results;
                root = temp;
            }
            else
            {
                // TODO: 如果有一个以上的<library>元素，则需要复制出一个新的DOM，然后把<library>元素全部删除干净
                strFilter = "[count(ancestor::library) = 0]";
            }

            // 2009/2/15 changed
            if (String.IsNullOrEmpty(strDbNameParam) == false)
            {
                XmlNode default_node = null;

                XmlNodeList nodes = root.SelectNodes("table[@name='" + strTableNameParam + "']" + strFilter);
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode table = nodes[i];

                    string strDbName = DomUtil.GetAttr(table, "dbname");

                    if (String.IsNullOrEmpty(strDbName) == true
                        && default_node == null)    // 认排列在最前面的一个缺省元素
                    {
                        default_node = table;
                        continue;
                    }

                    if (StringUtil.IsInList(strDbNameParam, strDbName) == true)
                    {
                        // 命中
                        return StringUtil.FromListString(table.InnerText.Trim(), ',', false);   // 要返回空字符串成员
                    }
                }

                // 虽然"dbname"没有命中，但是可以返回缺省的值(dbname属性为空的)
                if (default_node != null)
                {
                    return StringUtil.FromListString(default_node.InnerText.Trim(), ',', false);   // 要返回空字符串成员
                }

                return results;
            }
            else
            {
                // 没有dbname参数的情形
                XmlNodeList nodes = root.SelectNodes("table[@name='" + strTableNameParam + "']" + strFilter);
                if (nodes.Count == 0)
                    return results; // return null;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode table = nodes[i];

                    // string strName = DomUtil.GetAttr(table, "name");
                    string strDbName = DomUtil.GetAttr(table, "dbname");

                    // 优先选择一个dbname属性为空的元素
                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        // 命中
                        return StringUtil.FromListString(table.InnerText.Trim(), ',', false);   // 要返回空字符串成员
                    }
                }

                // 否则返回“没有找到”，尽管还有其他dbname属性有值的元素
                return results;
            }
        }

        // 2014/9/7
        // 给值列表加上 {} 部分
        static List<string> ConvertValueList(string strLibraryCode,
            List<string> values)
        {
            Debug.Assert(values != null, "");

            if (string.IsNullOrEmpty(strLibraryCode) == true)
                return values;

            List<string> results = new List<string>();
            foreach (string s in values)
            {
                if (s.IndexOf('{') == -1)
                    results.Add("{" + strLibraryCode + "} " + s);
                else
                    results.Add(s); // 如果本来就有 {} 部分，就不再另加了
            }

            return results;
        }

        // 获得值列表
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strTableName    表名。如果为空，表示任意name参数值均匹配
        //      strDbName   数据库名。如果为空，表示任意dbname参数值均匹配。
        public string[] GetValueTable(
            string strLibraryCodeList,
            string strTableNameParam,
            string strDbNameParam,
            bool addBraces = true)
        {
            List<string> librarycodes = new List<string>();
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
#if NO
                // 获得当前<valueTables>元素下所有<library>元素中的图书馆代码
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("valueTables/library");
                foreach (XmlNode node in nodes)
                {
                    string strLibraryCode = DomUtil.GetAttr(node, "code");
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                        continue;
                    librarycodes.Add(strLibraryCode);
                }
                librarycodes.Insert(0, "");
#endif
                // 获得当前<readerdbgroup>元素下所有<database>元素中的图书馆代码
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
                foreach (XmlNode node in nodes)
                {
                    string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                        continue;
                    librarycodes.Add(strLibraryCode);
                }
                librarycodes.Insert(0, "");
            }
            else
            {
                librarycodes = StringUtil.FromListString(strLibraryCodeList);
            }

            List<string> results = new List<string>();
            foreach (string strLibraryCode in librarycodes)
            {
                List<string> temp = GetOneLibraryValueTable(
                    strLibraryCode,
                    strTableNameParam,
                    strDbNameParam);
                // 如果没有找到
                if (temp == null || temp.Count == 0)
                {
                    if (strTableNameParam == "location")
                    {
                        // 改为从 <locationTypes> 中寻找
                        temp = GetOneLibraryLocationValueList(strLibraryCode);
                    }
                    else if (strTableNameParam == "bookType"
                        || strTableNameParam == "readerType")
                    {
                        // 改为从 <rightsTable>元素下的<readerTypes>或<bookTypes> 中寻找
                        temp = GetOneLibraryBookReaderTypeValueList(strLibraryCode,
                            strTableNameParam);
                    }
                }

                // 加上 {} 部分
                if (addBraces
                    && strTableNameParam != "location"
                    && temp != null)
                {
                    temp = ConvertValueList(strLibraryCode, temp);
                }

                if (temp == null || temp.Count == 0)
                    continue;

                results.AddRange(temp);
            }

            if (results.Count == 0)
                return new string[0];

            StringUtil.RemoveDupNoSort(ref results);

            string[] array = new string[results.Count];
            results.CopyTo(array);
            return array;
        }

#if NO
        // 从 <locationTypes> 元素中获得值列表
        public string[] GetLocationValueList(string strLibraryCodeList)
        {
            List<string> librarycodes = new List<string>();
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
#if NOOO
                // 获得当前<valueTables>元素下所有<library>元素中的图书馆代码
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("valueTables/library");
                foreach (XmlNode node in nodes)
                {
                    string strLibraryCode = DomUtil.GetAttr(node, "code");
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                        continue;
                    librarycodes.Add(strLibraryCode);
                }
                librarycodes.Insert(0, "");
#endif
                // 获得当前<readerdbgroup>元素下所有<database>元素中的图书馆代码
                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
                foreach (XmlNode node in nodes)
                {
                    string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");
                    if (string.IsNullOrEmpty(strLibraryCode) == true)
                        continue;
                    librarycodes.Add(strLibraryCode);
                }
                librarycodes.Insert(0, "");
            }
            else
            {
                librarycodes = StringUtil.FromListString(strLibraryCodeList);
            }

            List<string> results = new List<string>();
            foreach (string strLibraryCode in librarycodes)
            {

                List<string> temp = GetOneLibraryLocationValueList(
                    strLibraryCode);
                if (temp == null)
                    continue;
                if (temp.Count == 0)
                    continue;
                results.AddRange(temp);
            }

            if (results.Count == 0)
                return new string[0];

            StringUtil.RemoveDupNoSort(ref results);

            string[] array = new string[results.Count];
            results.CopyTo(array);
            return array;
        }
#endif

        // 获得一个图书馆代码下的 <locationTypes> 内 <item> 元素
        // parameters:
        //      strLibraryCode  馆代码
        public XmlElement GetLocationItemElement(
            string strLibraryCode,
            string strPureName)
        {
            List<string> results = new List<string>();

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes");
            if (root == null)
                return null;

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return null;
                root = temp;
            }
            else
            {
                // TODO: 如果有一个以上的<library>元素，则需要复制出一个新的DOM，然后把<library>元素全部删除干净
                strFilter = "[count(ancestor::library) = 0]";
            }

            // 2017/1/20 允许 item 元素的文本为空
            if (string.IsNullOrEmpty(strPureName) == true)
                return (XmlElement)root.SelectSingleNode("item[not(text())]" + strFilter);

            return (XmlElement)root.SelectSingleNode("item[text()='" + strPureName + "']" + strFilter);
        }

        // 获得一个图书馆代码下的 <locationTypes> 内 <item> 值列表
        // parameters:
        //      strLibraryCode  馆代码
        public List<string> GetOneLibraryLocationValueList(
            string strLibraryCode)
        {
            List<string> results = new List<string>();

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes");
            if (root == null)
                return results;

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return results;
                root = temp;
            }
            else
            {
                // TODO: 如果有一个以上的<library>元素，则需要复制出一个新的DOM，然后把<library>元素全部删除干净
                strFilter = "[count(ancestor::library) = 0]";
            }

            XmlNodeList nodes = root.SelectNodes("item" + strFilter);
            if (nodes.Count == 0)
                return results; // return null;
            foreach (XmlElement item in nodes)
            {
                string strValue = "";
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    strValue = item.InnerText.Trim();
                else
                    strValue = strLibraryCode + "/" + item.InnerText.Trim();

                results.Add(strValue);
            }
            return results;
        }

        // 获得一个图书馆代码下的 <rightsTable>元素下的<readerTypes>或<bookTypes> 值列表
        // parameters:
        //      strLibraryCode  馆代码
        public List<string> GetOneLibraryBookReaderTypeValueList(
            string strLibraryCode,
            string strTableName)
        {
            List<string> results = new List<string>();

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable");
            if (root == null)
                return results;

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("descendant::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return results;
                root = temp;
            }
            else
            {
                // TODO: 如果有一个以上的<library>元素，则需要复制出一个新的DOM，然后把<library>元素全部删除干净
                strFilter = "[count(ancestor::library) = 0]";
            }

            string strTypesElementName = "bookTypes";
            if (strTableName == "readerType")
                strTypesElementName = "readerTypes";

            Debug.Assert(strTableName == "bookType" || strTableName == "readerType", "");

            XmlNodeList nodes = root.SelectNodes(strTypesElementName + "/item" + strFilter);
            if (nodes.Count == 0)
                return results; // return null;
            foreach (XmlElement item in nodes)
            {
                string strValue = "";
                strValue = item.InnerText.Trim();
                if (strValue == "[空]")
                    strValue = "";
#if NO
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    strValue = item.InnerText.Trim();
                else
                    strValue = strLibraryCode + "/" + item.InnerText.Trim();
#endif
                results.Add(strValue);
            }
            return results;
        }

        // 2016/12/31
        // 获得当前已经定义的全部图书馆代码。不包括 ""
        public List<string> GetLibraryCodes()
        {
            List<string> librarycodes = new List<string>();
            if (this.LibraryCfgDom == null || this.LibraryCfgDom.DocumentElement == null)
                return librarycodes;

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
            foreach (XmlElement node in nodes)
            {
                string strLibraryCode = node.GetAttribute("libraryCode");
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    continue;
                librarycodes.Add(strLibraryCode);
            }

            return librarycodes;
        }

        // 检查一个馆代码是否为合法的馆代码
        public bool IsValidLibraryCode(string strLibraryCode)
        {
            if (string.IsNullOrEmpty(strLibraryCode))
                return true;

            // 目前本函数仅支持判断一个馆代码
            if (strLibraryCode.IndexOf(",") != -1)
                throw new ArgumentException("strLibraryCode 参数中不允许包含逗号", "strLibraryCode");

            if (GetLibraryCodes().IndexOf(strLibraryCode) != -1)
                return true;
            return false;
        }

        // 获得library.xml中配置的dtlp帐户信息
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetDtlpAccountInfo(string strPath,
            out string strUserName,
            out string strPassword,
            out string strError)
        {
            strError = "";
            strUserName = "";
            strPassword = "";

            XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//traceDTLP");
            if (node == null)
            {
                strError = "尚未配置<traceDTLP>";
                return -1;
            }

            // 从路径中析出服务器名部分
            int nRet = strPath.IndexOf("/");
            if (nRet != -1)
                strPath = strPath.Substring(0, nRet);

            node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//traceDTLP/origin[@serverAddr='" + strPath + "']");
            if (node == null)
            {
                strError = "不存在地址为 '" + strPath + "' 的DTLP服务器<origin>配置参数...";
                return 0;
            }

            strUserName = DomUtil.GetAttr(node, "UserName");
            strPassword = DomUtil.GetAttr(node, "Password");

            try
            {
                strPassword = DecryptPassword(strPassword);
            }
            catch
            {
                strPassword = "errorpassword";
            }

            return 1;
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

        // 映射内核脚本配置文件到本地
        // return:
        //      -1  error
        //      0   成功，为.cs文件
        //      1   成功，为.fltx文件
        public int MapKernelScriptFile(
            SessionInfo sessioninfo,
            string strBiblioDbName,
            string strScriptFileName,
            out string strLocalPath,
            out string strError)
        {
            strError = "";
            strLocalPath = "";
            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 将种记录数据从XML格式转换为HTML格式
            // 需要从内核映射过来文件
            // string strScriptFileName = "./cfgs/loan_biblio.fltx";
            // 将脚本文件名正规化
            // 因为在定义脚本文件的时候, 有一个当前库名环境,
            // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
            // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
            string strRemotePath = LibraryApplication.CanonicalizeScriptFileName(
                strBiblioDbName,
                strScriptFileName);

            // TODO: 还可以考虑支持http://这样的配置文件。

            nRet = this.CfgsMap.MapFileToLocal(
                // sessioninfo.Channels,
                channel,
                strRemotePath,
                out strLocalPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "内核配置文件 " + strRemotePath + "没有找到，因此无法获得书目html格式数据";
                goto ERROR1;
            }

            bool bFltx = false;
            // 如果是一般.cs文件, 还需要获得.cs.ref配置文件
            if (LibraryApplication.IsCsFileName(
                strScriptFileName) == true)
            {
                string strTempPath = "";
                nRet = this.CfgsMap.MapFileToLocal(
                    // sessioninfo.Channels,
                    channel,
                    strRemotePath + ".ref",
                    out strTempPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "内核配置文件 " + strRemotePath + ".ref" + "没有找到，因此无法获得书目html格式数据";
                    goto ERROR1;
                }

                bFltx = false;
            }
            else
            {
                bFltx = true;
            }

            if (bFltx == true)
                return 1;   // 为.fltx文件

            return 0;
        ERROR1:
            return -1;
        }

        // 将脚本文件名正规化
        // 因为在定义脚本文件的时候, 有一个当前库名环境,
        // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
        // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
        public static string CanonicalizeScriptFileName(string strDbName,
            string strScriptFileNameParam)
        {
            int nRet = 0;
            nRet = strScriptFileNameParam.IndexOf("./");
            if (nRet == 0)  // != -1   2006/12/24 changed
            {
                // 认为是当前库下
                return strDbName + strScriptFileNameParam.Substring(1);
            }

            nRet = strScriptFileNameParam.IndexOf("/");
            if (nRet == 0)  // != -1   2006/12/24 changed
            {
                // 认为从根开始
                return strScriptFileNameParam.Substring(1);
            }

            return strScriptFileNameParam;  // 保持原样
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

        // 清除各种缓存
        public void ClearCache()
        {
#if NO
            this.m_lockXml2HtmlAssemblyTable.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.Xml2HtmlAssemblyTable.Clear();
            }
            finally
            {
                this.m_lockXml2HtmlAssemblyTable.ReleaseWriterLock();
            }
#endif
            this.AssemblyCache.Clear();

            this.Filters.Clear();

        }



        // 构造虚拟库的XML检索式
        public static int BuildVirtualQuery(
            Hashtable db_dir_results,
            VirtualDatabase vdb,
            string strWord,
            string strVirtualFromName,
            string strMatchStyle,
            int nMaxCount,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            int nUsed = 0;

            string strLogic = "OR";

            List<string> realdbs = vdb.GetRealDbNames();

            if (realdbs.Count == 0)
            {
                strError = "虚拟库 '" + vdb.GetName(null) + "' 下居然没有定义任何物理库";
                return -1;
            }

            string strWarning = "";

            for (int i = 0; i < realdbs.Count; i++)
            {

                // 数据库名
                string strDbName = realdbs[i];


                string strFrom = vdb.GetRealFromName(
                    db_dir_results,
                    strDbName,
                    strVirtualFromName);
                if (strFrom == null)
                {
                    strWarning += "虚拟库 '" + vdb.GetName(null) + " '中针对物理库 '" + strDbName + "' 中对虚拟From '" + strVirtualFromName + "' 未找到对应的物理From名; ";
                    // strError = "虚拟库 '" + vdb.GetName(null) + " '中针对物理库 '" + strDbName + "' 中对虚拟From '" + strVirtualFromName + "' 未找到对应的物理From名";
                    // return -1;
                    continue;
                }

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType>"
                    + "<maxCount>" + nMaxCount.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                    strXml += "<operator value='" + strLogic + "'/>";

                strXml += strOneDbQuery;

                nUsed++;
            }

            if (nUsed > 0)
            {
                strXml = "<group>" + strXml + "</group>";
            }

            // 一个物理库也没有匹配上
            if (nUsed == 0)
            {
                strError = strWarning;
                return -1;
            }

            return 0;
        }


        // 根据检索参数创建XML检索式
        // return:
        //      -1  出错
        //      0   不存在所指定的数据库或者检索途径。一个都没有
        //      1   成功
        public static int BuildQueryXml(
            LibraryApplication app,
            string strDbName,
            string strWord,
            string strFrom,
            string strMatchStyle,
            string strRelation,
            string strDataType,
            int nMaxCount,
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

            if (String.IsNullOrEmpty(strMatchStyle) == true)
                strMatchStyle = "middle";

            if (String.IsNullOrEmpty(strRelation) == true)
                strRelation = "=";

            if (String.IsNullOrEmpty(strDataType) == true)
                strDataType = "string";

            //
            // 数据库是不是虚拟库?
            VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器

            string strOneDbQuery = "";

            // 如果是虚拟库
            if (vdb != null && vdb.IsVirtual == true)
            {
                int nRet = BuildVirtualQuery(
                    app.vdbs.db_dir_results,
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
            else
            {
                /*
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strWord) + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";
                 * */

                string strTargetList = "";

                if (String.IsNullOrEmpty(strDbName) == true
                    || strDbName.ToLower() == "<all>"
                    || strDbName == "<全部>")
                {
                    List<string> found_dup = new List<string>();    // 用于去重

                    if (app.vdbs.Count == 0)
                    {
                        strError = "目前library.xml中<virtualDatabases>内尚未配置检索目标";
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
                        strError = "目前library.xml中<itemdbgroup>内尚未配置数据库";
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

                if (String.IsNullOrEmpty(strTargetList) == true)
                {
                    strError = "不具备任何检索目标";
                    return 0;
                }

                strOneDbQuery = "<target list='"
                    + strTargetList
                    + "'><item><word>"
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
                    + "<maxCount>" + (-1).ToString() + "</maxCount></item><lang>zh</lang></target>";

            }

            strXml = strOneDbQuery;

            return 1;
        }

#if NOOOOOOOOOOOOOOOOOOOOO
        // 根据检索参数创建XML检索式
        public static int BuildQueryXml(
            LibraryApplication app,
            string strDbName,
            string strWord,
            string strFrom,
            string strMatchStyle,
            int nMaxCount,
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

            //
            // 数据库是不是虚拟库?
            VirtualDatabase vdb = app.vdbs[strDbName];  // 需要增加一个索引器

            if (vdb == null)
            {
                strError = "书目库名 '" + strDbName + "' 不存在。";
                return -1;
            }

            string strOneDbQuery = "";

            // 如果是虚拟库
            if (vdb.IsVirtual == true)
            {
                int nRet = BuildVirtualQuery(
                    app.vdbs.db_dir_results,
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
            else
            {
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strWord) + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";
            }

            strXml = strOneDbQuery;

            return 0;
        }
#endif

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





        // 获得当前全部读者库中使用过的馆代码列表
        public List<string> GetAllLibraryCode()
        {
            List<string> results = new List<string>();
            bool bBlank = false;    // 是否至少出现过一次空的馆代码
            foreach (ReaderDbCfg item in this.ReaderDbs)
            {
                if (string.IsNullOrEmpty(item.LibraryCode) == true)
                {
                    bBlank = true;
                    continue;
                }
                results.Add(item.LibraryCode);
            }

            if (bBlank == true)
                results.Insert(0, "");

            return results;
        }

        // 获得配置文件片断中所有下级<library>元素的code属性。并未去重
        public List<string> GetAllLibraryCode(XmlNode root)
        {
            List<string> all_librarycodes = new List<string>();
            XmlNodeList nodes = root.SelectNodes("descendant::library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                if (string.IsNullOrEmpty(strCode) == true)
                    continue;

                all_librarycodes.Add(strCode);
            }
            return all_librarycodes;
        }

        // 获得权限定义表HTML字符串
        // parameters:
        //      strSource   可能会包含<readerTypes>和<bookTypes>参数
        //      strLibraryCodeList  当前用户管辖的分馆代码列表
        public int GetRightTableHtml(
            string strSource,
            string strLibraryCodeList,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            XmlDocument cfg_dom = null;
            if (String.IsNullOrEmpty(strSource) == true)
                cfg_dom = this.LibraryCfgDom;
            else
            {
                cfg_dom = new XmlDocument();
                try
                {
                    cfg_dom.LoadXml("<rightsTable>" + strSource + "</rightsTable>");
                }
                catch (Exception ex)
                {
                    strError = "strSource内容(外加根元素后)装入XMLDOM时出现错误: " + ex.Message;
                    return -1;
                }
            }

            List<string> librarycodes = new List<string>();
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                // XML代码中的全部馆代码
                librarycodes = GetAllLibraryCode(cfg_dom.DocumentElement);
                StringUtil.RemoveDupNoSort(ref librarycodes);   // 去重

                // 读者库中用过的全部馆代码
                List<string> temp = GetAllLibraryCode();
                if (temp.Count > 0 && temp[0] == "")
                    librarycodes.Insert(0, "");
            }
            else
            {
                librarycodes = StringUtil.FromListString(strLibraryCodeList);
            }

            return LoanParam.GetRightTableHtml(
                cfg_dom,
                // strLibraryCodeList,
                librarycodes,
                out strResult,
                out strError);
        }

        public bool ClearCacheCfgs(string strResPath)
        {
            // 2016/12/27
            if (string.IsNullOrEmpty(strResPath) == true)
            {
                this.CfgsMap.Clear();
                this.Filters.Clear();
                this.AssemblyCache.Clear();
                return true;
            }

            string strPath = strResPath;

            string strDbName = StringUtil.GetFirstPartPath(ref strPath);
            string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

            if (strDbName == "cfgs" || strFirstPart == "cfgs")
            {
                string strLocalPath = this.CfgsMap.Clear(strResPath);
                this.Filters.ClearFilter(strLocalPath);
                this.AssemblyCache.Clear(strLocalPath);
                return true;
            }

            return false;
        }

        // 检查用户使用 WriteRes API 的权限
        // TODO: 需要把写入和删除的权限分开处理
        // 注： 
        //      writetemplate 写入模板配置文件 template 所需要的权限; 
        //      writeobject 写入对象所需要的权限; 
        //      writerecord 写入数据库文件所需要的权限
        //      writeres 写入数据库记录、配置文件、对象等所需要的笼统的权限
        // parameters:
        //      strLibraryCodeList  当前用户所管辖的馆代码列表
        //      strLibraryCode  [out]如果是写入读者库，这里返回实际写入的读者库的馆代码。如果不是写入读者库，则返回空
        // return:
        //      -1  error
        //      0   不具备权限
        //      1   具备权限
        public int CheckWriteResRights(
            string strLibraryCodeList,
            string strRights,
            string strResPath,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";

            string strPath = strResPath;

            // 写入 dp2library 本地文件
            if (string.IsNullOrEmpty(strPath) == false
                && strPath[0] == '!')
            {
                strPath = strPath.Substring(1);

                string strTargetDir = this.DataDir;
                string strFilePath = Path.Combine(strTargetDir, strPath);

                string strFirstLevel = StringUtil.GetFirstPartPath(ref strPath);

                if (string.Compare(strFirstLevel, "backup", true) == 0
                    || string.Compare(strFirstLevel, "cfgs", true) == 0)
                {
                    if (StringUtil.IsInList("backup,managedatabase", strRights) == false)
                    {
                        strError = "写入文件 " + strResPath + " 被拒绝。不具备 backup 或 managedatabase 权限";
                        return 0;
                    }
                }
                else if (string.Compare(strFirstLevel, "upload", true) == 0)
                {
                    if (StringUtil.IsInList("upload,managedatabase", strRights) == false)
                    {
                        strError = "写入文件 " + strResPath + " 被拒绝。不具备 upload 或 managedatabase 权限";
                        return 0;
                    }
                }
                else if (string.Compare(strFirstLevel, "library.xml", true) == 0
                    && string.IsNullOrEmpty(strPath))
                {
                    if (StringUtil.IsInList("managedatabase", strRights) == false)
                    {
                        strError = "写入文件 " + strResPath + " 被拒绝。不具备 managedatabase 权限";
                        return 0;
                    }
                }
                else
                {
                    strError = "第一级目录名必须为 'upload' 或者 'backup'";
                    return -1;
                }

                // 用于限定的根目录
                string strLimitDir = Path.Combine(strTargetDir, strFirstLevel);
                if (PathUtil.IsChildOrEqual(strFilePath, strLimitDir) == false)
                {
                    strError = "路径 '" + strResPath + "' 越过了限定的范围，无法访问";
                    return 0;
                }
                return 1;
            }

            if (StringUtil.IsInList("managedatabase", strRights))
                return 1;

            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            // 书目库
            if (this.IsBiblioDbName(strDbName) == true)
            {
                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strFirstPart == "cfgs")
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    if (strFirstPart == "template")
                    {
                        if (StringUtil.IsInList("writetemplate", strRights) == false)
                        {
                            strError = "写入模板配置文件 " + strResPath + " 被拒绝。不具备 writetemplate 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writetemplate权限，就不再需要writeres权限
                    }
                }

                // 记录ID
                if (StringUtil.IsPureNumber(strFirstPart) == true
                    || strFirstPart == "?")
                {
                    // 只到记录ID这一层
                    if (strPath == "")
                    {
                        if (StringUtil.IsInList("writerecord", strRights) == false)
                        {
                            strError = "直接写入记录 " + strResPath + " 被拒绝。不具备 writerecord 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writerecord权限，就不再需要writeres权限
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strFirstPart == "object")
                    {
                        if (StringUtil.IsInList("writeobject", strRights) == false)
                        {
                            strError = "写入对象资源 " + strResPath + " 被拒绝。不具备 writeobject 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writeobject权限，就不再需要writeres权限
                    }
                }

                if (StringUtil.IsInList("writeres", strRights) == false)
                {
                    strError = "写入资源 " + strResPath + " 被拒绝。不具备 writeres 权限";
                    return 0;
                }
            }

            // 读者库
            if (this.IsReaderDbName(strDbName, out strLibraryCode) == true)
            {
                // 2012/9/22
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "写入资源 " + strResPath + " 被拒绝。读者库 '" + strDbName + "' 不在当前用户的管辖范围 '" + strLibraryCodeList + "' 内";
                        return 0;
                    }
                }

                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strFirstPart == "cfgs")
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    if (strFirstPart == "template")
                    {
                        if (StringUtil.IsInList("writetemplate", strRights) == false)
                        {
                            strError = "写入模板配置文件 " + strResPath + " 被拒绝。不具备 writetemplate 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writetemplate权限，就不再需要writeres权限
                    }
                }

                // 记录ID
                if (StringUtil.IsPureNumber(strFirstPart) == true
                    || strFirstPart == "?")
                {
                    // 只到记录ID这一层
                    if (strPath == "")
                    {
                        /*
                        if (StringUtil.IsInList("writerecord", strRights) == false)
                        {
                            strError = "直接写入记录 " + strResPath + " 被拒绝。不具备writerecord权限。";
                            return 0;
                        }
                        return 1;   // 如果有了writerecord权限，就不再需要writeres权限
                         * */
                        strError = "不允许使用WriteRes()写入读者库记录";
                        return 0;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strFirstPart == "object")
                    {
                        if (StringUtil.IsInList("writeobject", strRights) == false)
                        {
                            strError = "写入对象资源 " + strResPath + " 被拒绝。不具备 writeobject 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writeobject权限，就不再需要writeres权限
                    }
                }

                if (StringUtil.IsInList("writeres", strRights) == false)
                {
                    strError = "写入资源 " + strResPath + " 被拒绝。不具备 writeres 权限";
                    return 0;
                }
            }


            /*
            if (StringUtil.IsInList("writeres", strRights) == false)
            {
                strError = "写入资源 " + strResPath + " 被拒绝。不具备writeres权限。";
                return 0;
            }*/

            // 评注库等
            if (this.IsCommentDbName(strDbName) == true
                || this.IsItemDbName(strDbName) == true
                || this.IsIssueDbName(strDbName) == true
                || this.IsOrderDbName(strDbName) == true)
            {
                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strFirstPart == "cfgs")
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    if (strFirstPart == "template")
                    {
                        if (StringUtil.IsInList("writetemplate", strRights) == false)
                        {
                            strError = "写入模板配置文件 " + strResPath + " 被拒绝。不具备 writetemplate 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writetemplate权限，就不再需要writeres权限
                    }
                }

                // 记录ID
                if (StringUtil.IsPureNumber(strFirstPart) == true
                    || strFirstPart == "?")
                {
                    // 只到记录ID这一层
                    if (strPath == "")
                    {
                        strError = "不允许使用WriteRes()写入评注库(等类型的书目下级)记录";
                        return 0;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strFirstPart == "object")
                    {
                        if (StringUtil.IsInList("writeobject", strRights) == false)
                        {
                            strError = "写入对象资源 " + strResPath + " 被拒绝。不具备 writeobject 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writeobject权限，就不再需要writeres权限
                    }
                }

                if (StringUtil.IsInList("writeres", strRights) == false)
                {
                    strError = "写入资源 " + strResPath + " 被拒绝。不具备 writeres 权限";
                    return 0;
                }
            }

            // 实用库 2013/10/30
            if (ServerDatabaseUtility.IsUtilDbName(this.LibraryCfgDom, strDbName) == true)
            {
                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strFirstPart == "cfgs")
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    if (strFirstPart == "template")
                    {
                        if (StringUtil.IsInList("writetemplate", strRights) == false)
                        {
                            strError = "写入模板配置文件 " + strResPath + " 被拒绝。不具备 writetemplate 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writetemplate权限，就不再需要writeres权限
                    }

                }

                // 记录ID
                if (StringUtil.IsPureNumber(strFirstPart) == true
                    || strFirstPart == "?")
                {
                    // 只到记录ID这一层
                    if (strPath == "")
                    {
                        if (StringUtil.IsInList("writerecord", strRights) == false)
                        {
                            strError = "直接写入记录 " + strResPath + " 被拒绝。不具备 writerecord 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writerecord权限，就不再需要writeres权限
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strFirstPart == "object")
                    {
                        if (StringUtil.IsInList("writeobject", strRights) == false)
                        {
                            strError = "写入对象资源 " + strResPath + " 被拒绝。不具备 writeobject 权限";
                            return 0;
                        }
                        return 1;   // 如果有了writeobject权限，就不再需要writeres权限
                    }
                }

                if (StringUtil.IsInList("writeres", strRights) == false)
                {
                    strError = "写入资源 " + strResPath + " 被拒绝。不具备 writeres 权限";
                    return 0;
                }
            }

            strError = "写入资源 " + strResPath + " 被拒绝。不具备特定的权限";
            return 0;
        }

        // 检查用户使用 GetRes API 的权限
        // parameters:
        //      strLibraryCodeList  当前用户所管辖的馆代码列表
        //      strRights   访问者的权限
        //      strLibraryCode  [out]如果是访问读者库，这里返回实际访问的读者库的馆代码。如果不是访问读者库，则返回空
        //      strFilePath  [out]物理文件路径
        // return:
        //      -1  error
        //      0   不具备权限
        //      1   具备权限
        public int CheckGetResRights(
            SessionInfo sessioninfo,
            string strLibraryCodeList,
            string strRights,
            string strResPath,
            out string strLibraryCode,
            out string strFilePath,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";
            strFilePath = "";

            string strPath = strResPath;

            // 读取 dp2library 本地文件
            if (string.IsNullOrEmpty(strPath) == false
                && strPath[0] == '!')
            {
                strPath = strPath.Substring(1);

                string strTargetDir = this.DataDir;
                strFilePath = Path.Combine(strTargetDir, strPath);

                // 注意： strPath 中的斜杠应该是 '/'
                string strFirstLevel = StringUtil.GetFirstPartPath(ref strPath);
                if (StringUtil.IsInList("managedatabase,backup", strRights) == false)
                {
                    if (string.Compare(strFirstLevel, "upload", true) != 0)
                    {
                        strError = "因当前用户不具备权限 managedatabase，能列出的第一级目录名被限定为 'upload'";
                        return -1;
                    }
                }

                if (StringUtil.IsInList("download,backup", strRights) == false)
                {
                    strError = "读取文件 " + strResPath + " 被拒绝。不具备 download 或 backup 权限";
                    return 0;
                }
                // 用于限定的根目录
                string strLimitDir = Path.Combine(strTargetDir, strFirstLevel);
                if (PathUtil.IsChildOrEqual(strFilePath, strLimitDir) == false)
                {
                    strError = "路径 '" + strResPath + "' 越过了限定的范围，无法访问";
                    return 0;
                }
                return 1;
            }

            // 如果具备 writeobject 权限，则具备所有对象的读取权限了
            if (StringUtil.IsInList("writeobject", strRights) == true
                || StringUtil.IsInList("writeres", strRights) == true)
                return 1;

            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            // 书目库
            if (this.IsBiblioDbName(strDbName) == true)
            {
                string strRecordID = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strRecordID == "cfgs")
                {
                    return 1;   // 书目库下属的配置文件
                }

                // 记录ID
                if (StringUtil.IsPureNumber(strRecordID) == true
                    || strRecordID == "?")
                {
                    // 只到记录ID这一层
                    if (string.IsNullOrEmpty(strPath) == true)
                    {
                        return 1;
                    }

                    string strObject = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strObject == "object")
                    {
                        string strObjectID = StringUtil.GetFirstPartPath(ref strPath);

                        string strPartCmd = "";
                        if (string.IsNullOrEmpty(strPath) == false)
                            strPartCmd = strPath;

                        // 根据 ID 得到权限定义进行判断
                        string strXmlRecordPath = strDbName + "/" + strRecordID;

                        // 获得对象的 rights 属性
                        // 需要先获得元数据 XML，然后从中得到 file 元素的 rights 属性
                        // return:
                        //      -1  出错
                        //      0   没有找到 object id 相关的信息
                        //      1   找到
                        int nRet = GetObjectRights(
            sessioninfo,
            strXmlRecordPath,
            strObjectID,
            out string strObjectRights,
            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                            goto ALLOW_ACCESS;   // TODO: 此时是否允许访问?

                        // 2018/9/15
                        if (StringUtil.IsInList("objectRights", this.Function) == false)
                            goto ALLOW_ACCESS;   // 如果 dp2library 没有许可 objectRights 功能，是允许任何访问者来获取的。即，不限制任何下载权限

                        if (string.IsNullOrEmpty(strObjectRights) == true)
                            goto ALLOW_ACCESS;   // 没有定义 rights 的对象是允许任何访问者来获取的

                        string strOperation = "download";
                        if (string.IsNullOrEmpty(strPartCmd) == false)
                            strOperation = "preview";
                        if (CanGet(strOperation, strRights, strObjectRights) == true)
                            goto ALLOW_ACCESS;

                        strError = "读取资源 " + strResPath + " 被拒绝。不具备相应的权限";
                        return 0;
                    ALLOW_ACCESS:
                        // 2018/8/12
                        // 判断 dp2library 序列号是否许可进行下载
                        if (string.IsNullOrEmpty(strPartCmd) == false)
                        {
                            // 没有许可 pdfPreiew 功能时，允许前面若干页，不允许后面的页。要解析 strCmd
                            if (StringUtil.IsInList("pdfPreview", this.Function) == false
                                && PageInRange(strPartCmd) == false)
                            {
                                strError = "PDF 预览功能需要设置序列号并许可此功能，才允许查看后面的页";
                                return 0;
                            }
                        }
                        return 1;
                    }
                }

                strError = "读取资源 " + strResPath + " 被拒绝。不具备相应的权限";
                return 0;
            }

#if NO
            // 读者库
            if (this.IsReaderDbName(strDbName, out strLibraryCode) == true)
            {
                // 2012/9/22
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "写入资源 " + strResPath + " 被拒绝。读者库 '" + strDbName + "' 不在当前用户的管辖范围 '" + strLibraryCodeList + "' 内";
                        return 0;
                    }
                }

                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // cfgs
                if (strFirstPart == "cfgs")
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    if (strFirstPart == "template")
                    {
                        if (StringUtil.IsInList("writetemplate", strRights) == false)
                        {
                            strError = "写入模板配置文件 " + strResPath + " 被拒绝。不具备writetemplate权限";
                            return 0;
                        }
                        return 1;   // 如果有了writetemplate权限，就不再需要writeres权限
                    }

                }

                // 记录ID
                if (StringUtil.IsPureNumber(strFirstPart) == true
                    || strFirstPart == "?")
                {
                    // 只到记录ID这一层
                    if (strPath == "")
                    {
                        /*
                        if (StringUtil.IsInList("writerecord", strRights) == false)
                        {
                            strError = "直接写入记录 " + strResPath + " 被拒绝。不具备writerecord权限。";
                            return 0;
                        }
                        return 1;   // 如果有了writerecord权限，就不再需要writeres权限
                         * */
                        strError = "不允许使用WriteRes()写入读者库记录";
                        return 0;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                    // 对象资源
                    if (strFirstPart == "object")
                    {
                        if (StringUtil.IsInList("writeobject", strRights) == false)
                        {
                            strError = "写入对象资源 " + strResPath + " 被拒绝。不具备writeobject权限";
                            return 0;
                        }
                        return 1;   // 如果有了writeobject权限，就不再需要writeres权限
                    }
                }

                if (StringUtil.IsInList("writeres", strRights) == false)
                {
                    strError = "写入资源 " + strResPath + " 被拒绝。不具备writeres权限";
                    return 0;
                }
            }
#endif

            return 1;
#if NO
            strError = "获取资源 " + strResPath + " 被拒绝。不具备特定的权限";
            return 0;
#endif
        }

        static bool PageInRange(string strPartCmd)
        {
            // page:1
            Hashtable parameters = StringUtil.ParseParameters(strPartCmd, ',', ':', "");
            string strPage = (string)parameters["page"];

            if (Int32.TryParse(strPage, out int nPageNo) == false)
                return true;
            if (nPageNo >= 1 && nPageNo <= 10)
                return true;
            return false;
        }

        // 对象是否允许执行某个操作?
        // parameters:
        //      strOperation   要查询的操作。为 download preview write 之一。默认 download
        //      strObjectRights 对象权限定义。原始定义
        //              简单用法: user1,user2   代表 user1 和 user2 同时具备所有操作(例如 download 和 preview 等)权限
        //              详尽用法: download:user1,user2;preview:user3,user4
        public static bool CanGet(string strOperation,
            string strGroupOrLevels,
            string strObjectRights)
        {
            // strRights 存放(从原始定义)过滤以后的权限，即针对特定操作的权限
            string strRights = strObjectRights;
            // 如果是详尽用法，要
            // 把权限字符串中其它无关 strOperation 的部分滤除，只剩下需要关注的部分
            if (strRights.IndexOf(":") != -1)
            {
                // return:
                //      null    没有找到前缀
                //      ""      找到了前缀，并且值部分为空
                //      其他     返回值部分
                strRights = StringUtil.GetParameterByPrefixEnvironment(strObjectRights, strOperation, ":", ';');
                if (strRights == null)
                    strRights = "";
            }

            string[] groups = strGroupOrLevels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] objects = strRights.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string o in objects)
            {
                if (IndexOf(groups, o) != -1)
                    return true;
                if (StringUtil.HasHead(o, "level-") == true)
                {
                    if (HasLevel(o, groups) == true)
                        return true;
                }
            }

            return false;
        }

        // strList 中是否包含了高于或者等于 strLevel 要求的字符串?
        public static bool HasLevel(string strLevel, string[] list)
        {
            int level = GetLevelNumber(strLevel);

            // string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string o in list)
            {
                if (StringUtil.HasHead(o, "level-") == true)
                {
                    int current = GetLevelNumber(o);
                    if (current >= level)
                        return true;
                }
            }

            return false;
        }

        // 获得 "level-10" 字符串中的数字值
        static int GetLevelNumber(string strText)
        {
            int nRet = strText.IndexOf("-");
            if (nRet == -1)
                return -1;
            strText = strText.Substring(nRet + 1);
            int number = -1;
            int.TryParse(strText, out number);
            return number;
        }

        static int IndexOf(string[] strings, string s)
        {
            int i = 0;
            foreach (string o in strings)
            {
                if (s == o)
                    return i;
                i++;
            }
            return -1;
        }

        // 获得对象的 rights 属性
        // 需要先获得元数据 XML，然后从中得到 file 元素的 rights 属性
        // return:
        //      -1  出错
        //      0   没有找到 object id 相关的信息
        //      1   找到
        int GetObjectRights(
            SessionInfo sessioninfo,
            string strXmlRecordPath,
            string strObjectID,
            out string strRights,
            out string strError)
        {
            strError = "";
            strRights = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            long lRet = channel.GetRes(strXmlRecordPath,
                out string strXml,
                out string strMetaData,
                out byte[] timestamp,
                out string strTempOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获得元数据记录 '" + strXmlRecordPath + "' 时出错: " + strError;
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "元数据记录 XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            if (!(dom.DocumentElement.SelectSingleNode("//dprms:file[@id='" + strObjectID + "']",
                nsmgr) is XmlElement node))
                return 0;
            strRights = node.GetAttribute("rights");
            return 1;
        }

        public class ReaderDbCfg
        {
            public string DbName = "";
            public bool InCirculation = true;   // 2008/6/3 

            public string LibraryCode = "";     // 2012/9/7
        }

        public enum ResPathType
        {
            None = 0,
            Record = 1,
            CfgFile = 2,
            Object = 3,
        }

        // 判断一个路径是否为对象路径
        public static ResPathType GetResPathType(string strPath)
        {
            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

            // cfgs
            if (strFirstPart == "cfgs")
            {
                return ResPathType.CfgFile;
            }

            // 记录ID
            if (StringUtil.IsPureNumber(strFirstPart) == true
                || strFirstPart == "?")
            {
                // 只到记录ID这一层
                if (String.IsNullOrEmpty(strPath) == true)
                {
                    return ResPathType.Record;
                }

                strFirstPart = StringUtil.GetFirstPartPath(ref strPath);

                // 对象资源
                if (strFirstPart == "object")
                {
                    return ResPathType.Object;
                }
            }

            return ResPathType.None;
        }

        // 发送 MQ 消息
        public int SendMessageQueue(
            string strRecipient,
            string strMime,
            string strBody,
            out string strError)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(this.OutgoingQueue))
                {
                    // 向 MSMQ 消息队列发送消息
                    // return:
                    //      -2  MSMQ 错误
                    //      -1  出错
                    //      0   成功
                    int nRet = ReadersMonitor.SendToQueue(queue,
                        strRecipient,
                        strMime,
                        strBody,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        return -1;
                    return 0;
                }
            }
            catch (Exception ex)
            {
                strError = "创建路径为 '" + this.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 通过 MSMQ 发送手机短信
        // parameters:
        //      strUserName 账户名，或者读者证件条码号，或者 "@refID:xxxx"
        public int SendSms(
            string strUserName,
            string strPhoneNumber,
            string strText,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.OutgoingQueue))
            {
                strError = "消息队列尚未被 dp2library 启用";
                return -1;
            }

            try
            {
                MessageQueue queue = new MessageQueue(this.OutgoingQueue);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                /* 元素名
 * type 消息类型。登录验证码
 * userName 用户名
 * phoneNumber 手机号码
 * text 短信消息内容
 */
                DomUtil.SetElementText(dom.DocumentElement, "type", "登录验证码");
                DomUtil.SetElementText(dom.DocumentElement, "userName", strUserName);
                DomUtil.SetElementText(dom.DocumentElement, "phoneNumber", strPhoneNumber);
                DomUtil.SetElementText(dom.DocumentElement, "text", strText);

                // 向 MSMQ 消息队列发送消息
                // return:
                //      -2  MSMQ 错误
                //      -1  出错
                //      0   成功
                int nRet = ReadersMonitor.SendToQueue(queue,
                    strUserName + "@LUID:" + this.UID,
                    "xml",
                    dom.DocumentElement.OuterXml,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    strError = "发送 MQ 消息时出错: " + strError;
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError += "创建路径为 '" + this.OutgoingQueue + "' 的 MessageQueue 对象失败: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 2018/6/21
        // 为了兼容旧的脚本文件 table_unimarc.fltx。时间长了以后可以考虑删除本函数
        public static string BuildTableXml(List<NameValueLine> lines)
        {
            return NameValueLine.BuildTableXml(lines, "");
        }

        // https://mattyrowan.com/2008/01/01/parse-timespan-string/
        public static TimeSpan ParseTimeSpan(string s)
        {
            const string Quantity = "quantity";
            const string Unit = "unit";

            const string Days = @"(d(ays?)?)";
            const string Hours = @"(h((ours?)|(rs?))?)";
            const string Minutes = @"(m((inutes?)|(ins?))?)";
            const string Seconds = @"(s((econds?)|(ecs?))?)";

            Regex timeSpanRegex = new Regex(
                string.Format(@"\s*(?<{0}>\d+)\s*(?<{1}>({2}|{3}|{4}|{5}|\Z))",
                              Quantity, Unit, Days, Hours, Minutes, Seconds),
                              RegexOptions.IgnoreCase);
            MatchCollection matches = timeSpanRegex.Matches(s);

            TimeSpan ts = new TimeSpan();
            foreach (Match match in matches)
            {
                if (Regex.IsMatch(match.Groups[Unit].Value, @"\A" + Days))
                {
                    ts = ts.Add(TimeSpan.FromDays(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Hours))
                {
                    ts = ts.Add(TimeSpan.FromHours(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Minutes))
                {
                    ts = ts.Add(TimeSpan.FromMinutes(double.Parse(match.Groups[Quantity].Value)));
                }
                else if (Regex.IsMatch(match.Groups[Unit].Value, Seconds))
                {
                    ts = ts.Add(TimeSpan.FromSeconds(double.Parse(match.Groups[Quantity].Value)));
                }
                else
                {
                    // Quantity given but no unit, default to Hours
                    ts = ts.Add(TimeSpan.FromHours(double.Parse(match.Groups[Quantity].Value)));
                }
            }
            return ts;
        }

        static TimeSpan ToTimeLength(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "无")
                return TimeSpan.FromMinutes(0);

            return ParseTimeSpan(name);
        }

    }

#if NO
    // 系统挂起的理由
    public enum HangupReason
    {
        None = 0,   // 没有挂起
        LogRecover = 1, // 日志恢复
        Backup = 2, // 大备份
        Normal = 3, // 普通维护
        StartingError = 4, // 启动过程发生严重错误
        OperLogError = 5,   // 操作日志错误（例如日志空间满）
        Exit = 6,  // 系统正在退出
        Expire = 7, // 因长期没有升级版本，当前版本已经失效
    }
#endif

    // API错误码
    public enum ErrorCode
    {
        NoError = 0,
        SystemError = 1,    // 系统错误。指application启动时的严重错误。
        NotFound = 2,   // 没有找到
        ReaderBarcodeNotFound = 3,  // 读者证条码号不存在
        ItemBarcodeNotFound = 4,  // 册条码号不存在
        Overdue = 5,    // 还书过程发现有超期情况（已经按还书处理完毕，并且已经将超期信息记载到读者记录中，但是需要提醒读者及时履行超期违约金等手续）
        NotLogin = 6,   // 尚未登录
        DupItemBarcode = 7, // 预约中本次提交的某些册条码号被本读者先前曾预约过 TODO: 这个和 ItemBarcodeDup 是否要合并?
        InvalidParameter = 8,   // 不合法的参数
        ReturnReservation = 9,    // 还书操作成功, 因属于被预约图书, 请放入预约保留架
        BorrowReservationDenied = 10,    // 借书操作失败, 因属于被预约(到书)保留的图书, 非当前预约者不能借阅
        RenewReservationDenied = 11,    // 续借操作失败, 因属于被预约的图书
        AccessDenied = 12,  // 存取被拒绝
        // ChangePartDenied = 13,    // 部分修改被拒绝
        ItemBarcodeDup = 14,    // 册条码号重复
        Hangup = 15,    // 系统挂起
        ReaderBarcodeDup = 16,  // 读者证条码号重复(以后将改用 BarcodeDup)
        HasCirculationInfo = 17,    // 包含流通信息(不能删除)
        SourceReaderBarcodeNotFound = 18,  // 源读者证条码号不存在
        TargetReaderBarcodeNotFound = 19,  // 目标读者证条码号不存在
        FromNotFound = 20,  // 检索途径(from caption或者style)没有找到
        ItemDbNotDef = 21,  // 实体库没有定义
        IdcardNumberDup = 22,   // 身份证号检索点命中读者记录不唯一。因为无法用它借书还书。但是可以用证条码号来进行
        IdcardNumberNotFound = 23,  // 身份证号不存在
        PartialDenied = 24,  // 有部分修改被拒绝
        ChannelReleased = 25,   // 通道先前被释放过，本次操作失败
        OutofSession = 26,   // 通道达到配额上限
        InvalidReaderBarcode = 27,  // 读者证条码号不合法
        InvalidItemBarcode = 28,    // 册条码号不合法
        NeedSmsLogin = 29,  // 需要改用短信验证码方式登录
        RetryLogin = 30,    // 需要补充验证码再次登录
        TempCodeMismatch = 31,  // 验证码不匹配
        BiblioDup = 32,     // 书目记录发生重复
        Borrowing = 33,    // 图书尚未还回(盘点前需修正此问题)
        ClientVersionTooOld = 34, // 前端版本太旧
        NotBorrowed = 35,   // 册记录处于未被借出状态 2017/6/20
        NotChanged = 36,    // 没有发生修改 2019/11/10
        ServerTimeout = 37, // 服务器发生 ApplicationException 超时
        AlreadyBorrowed = 38,   // 已经被当前读者借阅 2020/3/26
        AlreadyBorrowedByOther = 39,    // 已经被其他读者借阅 2020/3/26
        SyncDenied = 40,    // 同步操作被拒绝(因为实际操作时间之后又发生过借还操作) 2020/3/27
        PasswordExpired = 41,   // 密码已经失效 2021/7/4
        BarcodeDup = 42,        // 条码号重复了 2021/8/9
        DisplayNameDup = 43,  // 显示名重复了 2021/8/9
        RefIdDup = 44,    // 参考 ID 重复了 2021/8/9

        // 以下为兼容内核错误码而设立的同名错误码
        AlreadyExist = 100, // 兼容
        AlreadyExistOtherType = 101,
        ApplicationStartError = 102,
        EmptyRecord = 103,
        // None = 104, 采用了NoError
        NotFoundSubRes = 105,
        NotHasEnoughRights = 106,
        OtherError = 107,
        PartNotFound = 108,
        RequestCanceled = 109,
        RequestCanceledByEventClose = 110,
        RequestError = 111,
        RequestTimeOut = 112,
        TimestampMismatch = 113,
        Compressed = 114,   // 2017/10/7
        NotFoundObjectFile = 115, // 2019/10/7
    }

    // API函数结果
    public class LibraryServerResult
    {
        public long Value = 0;
        public string ErrorInfo = "";
        public ErrorCode ErrorCode = ErrorCode.NoError;

        public LibraryServerResult Clone()
        {
            LibraryServerResult other = new LibraryServerResult();
            other.Value = this.Value;
            other.ErrorCode = this.ErrorCode;
            other.ErrorInfo = this.ErrorInfo;
            return other;
        }

        // 把内核错误码转换为 dp2library 错误码
        public static ErrorCode FromErrorValue(DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue error_code,
            bool throw_exception = false)
        {
            string text = error_code.ToString();
            if (Enum.TryParse<ErrorCode>(text, out ErrorCode result) == false)
            {
                if (throw_exception == true)
                    throw new Exception("无法将字符串 '" + text + "' 转换为 LibraryServer.ErrorCode 类型");
                else
                    return ErrorCode.SystemError;
            }
            return result;
        }
    }

    // 帐户信息
    public class Account
    {
        private string location = "";
        public string Location { get => location; set => location = value; }

        internal XmlElement _xmlNode = null;  // library.xml 配置文件中相关小节

        private string loginName = "";
        // 登录名 带有前缀的各种渠道的登录名字
        public string LoginName { get => loginName; set => loginName = value; }

        private string password = "";
        public string Password { get => password; set => password = value; }

        // 2021/8/29
        private string passwordType = "";
        public string PasswordType { get => passwordType; set => passwordType = value; }

        // 2021/7/3
        private DateTime passwordExpire = DateTime.MaxValue;
        public DateTime PasswordExpire { get => passwordExpire; set => passwordExpire = value; }

        private string type = "";
        public string Type { get => type; set => type = value; }

        string m_strRights = "";
        public string Rights
        {
            get
            {
                return this.m_strRights;
            }
            set
            {
                this.m_strRights = value;

                this.m_rightsOriginList.Text = LibraryApplication.ExpandRightString(value);
            }
        }

        QuickList m_rightsOriginList = new QuickList();

        public QuickList RightsOriginList
        {
            get
            {
                return this.m_rightsOriginList;
            }
        }

        private string accountLibraryCode = "";
        // 2007/12/15 
        public string AccountLibraryCode { get => accountLibraryCode; set => accountLibraryCode = value; }

        private string access = "";
        // 存取权限代码 2008/2/28 
        public string Access { get => access; set => access = value; }

        private string userID = "";
        // 用户唯一标识。对于读者，这就是证条码号
        public string UserID { get => userID; set => userID = value; }

        private string rmsUserName = "";
        public string RmsUserName { get => rmsUserName; set => rmsUserName = value; }

        private string rmsPassword = "";
        public string RmsPassword { get => rmsPassword; set => rmsPassword = value; }

        private string binding = ""; // 2016/10/26
        public string Binding { get => binding; set => binding = value; }

        private string barcode = ""; // 证条码号。对于读者型的帐户有意义。特殊情况下，内容可能是"@refID:xxxxx"
        public string Barcode { get => barcode; set => barcode = value; }

        private string name = "";    // 姓名。对于读者型的帐户有意义
        public string Name { get => name; set => name = value; }

        private string displayName = ""; // 显示名。对于读者型的帐户有意义
        public string DisplayName { get => displayName; set => displayName = value; }

        private string personalLibrary;  // 书斋名。对于读者型的帐户有意义
        public string PersonalLibrary { get => personalLibrary; set => personalLibrary = value; }

        private string token = "";   // 随机创建的标记
        public string Token { get => token; set => token = value; }

        private XmlDocument patronDom = null;    // 如果是读者帐户，这里是读者记录DOM
        public XmlDocument PatronDom { get => patronDom; set => patronDom = value; }

        private string readerDomBarcode = "";   // 缓冲的DOM代表的读者证条码号
        public string ReaderDomBarcode { get => readerDomBarcode; set => readerDomBarcode = value; }

        private byte[] readerDomTimestamp = null;    // 读者记录时间戳
        public byte[] ReaderDomTimestamp { get => readerDomTimestamp; set => readerDomTimestamp = value; }

        private string readerDomPath = "";   // 读者记录路径
        public string ReaderDomPath { get => readerDomPath; set => readerDomPath = value; }

        private DateTime readerDomLastTime = new DateTime((long)0);  // 最近装载的时间
        public DateTime ReaderDomLastTime { get => readerDomLastTime; set => readerDomLastTime = value; }

        private bool readerDomChanged = false;
        public bool ReaderDomChanged { get => readerDomChanged; set => readerDomChanged = value; }

        #region 手机短信验证码

        // 竖线间隔的手机号码列表
        // return:
        //      null    没有找到前缀
        //      ""      找到了前缀，并且值部分为空
        //      其他     返回值部分
        public string GetPhoneNumberBindingString()
        {
            // 看看绑定信息里面是否有对应的手机号码
            // 注: email 元素内容，现在是存储 email 和微信号等多种绑定途径 2016/4/16
            // return:
            //      null    没有找到前缀
            //      ""      找到了前缀，并且值部分为空
            //      其他     返回值部分
            return StringUtil.GetParameterByPrefix(this.Binding,
    "sms",
    ":");
        }

        // 验证码多长时间过期
        public static TimeSpan TempCodeExpireLength = TimeSpan.FromHours(48);   // TimeSpan.FromMinutes(10);   // 10 分钟

        // 准备手机短信验证登录的第一阶段：产生验证码
        // return:
        //      -1  出错
        //      0   沿用以前的验证码
        //      1   用新的验证码
        public int PrepareTempPassword(
            TempCodeCollection table,
            string strClientIP,
            string strPhoneNumber,
            out TempCode code,
            out string strError)
        {
            strError = "";
            code = null;

            if (string.IsNullOrEmpty(strPhoneNumber))
            {
                strError = "strPhoneNumber 参数值不应为空";
                return -1;
            }

            strPhoneNumber = strPhoneNumber.Trim();
            if (string.IsNullOrEmpty(strPhoneNumber))
            {
                strError = "strPhoneNumber 参数值不应为空(1)";
                return -1;
            }

            string strList = GetPhoneNumberBindingString();
            if (string.IsNullOrEmpty(strList))
            {
                strError = "当前账号未曾做过手机短信方式(sms:)绑定";
                return -1;   // 没有做过 sms: 绑定
            }

            List<string> list = StringUtil.SplitList(strList, '|');
            if (list.IndexOf(strPhoneNumber) == -1)
            {
                strError = "所提供的电话号码 '" + strPhoneNumber + "' 不在手机绑定号码列表中";
                return -1;   // 电话号码没有在列表中
            }

            // 检索看看是否有已经存在的密码
            bool bExist = false;
            DateTime now = DateTime.Now;
            string strKey = this.UserID + "|" + strPhoneNumber + "|" + strClientIP;
            code = table.FindTempCode(strKey);
            if (code != null)
            {
                if (code.ExpireTime < now)
                    code = null;    // 迫使重新取号
                else
                {
                    // 失效期还没有到。主动延长一次失效期
                    code.ExpireTime = DateTime.Now + TempCodeExpireLength;
                    bExist = true;
                }
            }

            if (code == null)
            {
                // 重新设定一个密码
                Random rnd = new Random();
                code = new TempCode();
                code.Key = strKey;
                code.Code = rnd.Next(1, 999999).ToString();
                code.ExpireTime = DateTime.Now + TempCodeExpireLength;
            }

            table.SetTempCode(code.Key, code);
            // strTempCode = code.Code;
            if (bExist)
                return 0;
            return 1;
        }

        // 准备手机短信验证登录的第二阶段：匹配验证码
        public bool MatchTempPassword(
            TempCodeCollection table,
            string strPhoneNumber,
            string strClientIP,
            string strPassword,
            out string strError)
        {
            strError = "";

            string strKey = this.UserID + "|" + strPhoneNumber + "|" + strClientIP;

            TempCode code = table.FindTempCode(strKey);
            if (code == null)
            {
                strError = "当前用户的验证码尚未初始化";
                return false;
            }

            if (DateTime.Now > code.ExpireTime)
            {
                strError = "验证码已经过期失效";
                return false;
            }

            if (strPassword != code.Code)
            {
                strError = "验证码匹配失败";
                return false;
            }

            return true;
        }

        #endregion

        public Account()
        {
            Random random = new Random(unchecked((int)DateTime.Now.Ticks));
            long number = random.Next(0, 9999);	// 4位数字

            Token = Convert.ToString(DateTime.Now.Ticks) + "__" + Convert.ToString(number);
        }

        // 最原始的权限定义
        public string RightsOrigin
        {
            get
            {
                return LibraryApplication.ExpandRightString(this.Rights);
            }
        }





















        // 匹配 IP 地址
        // parameters:
        //      alter_type_list 返回处理结果列表。只有当 alter_type_list != null 并且没有匹配上真实 IP 地址时，才在本参数中返回值
        // return:
        //      true    允许
        //      false   禁止
        public bool MatchClientIP(string strClientIP,
            ref List<string> alter_type_list,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.Binding) == true)
                return true;
            string list = StringUtil.GetParameterByPrefix(this.Binding, "ip");
            if (string.IsNullOrEmpty(list))
                return true;    // 没有绑定 ip: ，表示不进行任何 IP 限制

            if (StringUtil.MatchIpAddressList(list, strClientIP) == false)
            {
                if (alter_type_list != null)
                {
                    alter_type_list.Add("!ip"); // 表示虽然处理了，但是没有匹配
                    alter_type_list.AddRange(GetAlterBinding(list));
                }

                strError = "前端 IP 地址 '" + strClientIP + "' 不在当前账户的 ip: 白名单中，访问被拒绝";
                return false;
            }

            if (alter_type_list != null)
                alter_type_list.Add("ip"); // 表示已经验证了 ip: 绑定
            return true;
        }

        // 匹配 dp2Router 的前端 IP 地址
        // parameters:
        //      alter_type_list 返回替代类型列表。只有当 alter_type_list != null 并且没有匹配上真实 IP 地址时，才在本参数中返回值
        // return:
        //      true    允许
        //      false   禁止
        public bool MatchRouterClientIP(string strRouterClientIP,
            ref List<string> alter_type_list,
            out string strError)
        {
            strError = "";

            if (strRouterClientIP == null)
                return true;

            if (string.IsNullOrEmpty(this.Binding) == true)
            {
                strError = "当前账户未绑定 router_ip，不允许前端经由 dp2Router 访问";
                return false;
            }

            string list = StringUtil.GetParameterByPrefix(this.Binding, "router_ip");
            if (string.IsNullOrEmpty(list))
            {
                strError = "当前账户未绑定 router_ip，不允许前端经由 dp2Router 访问 .";
                return false;
            }

            if (list == "*")
                goto END1;
            if (strRouterClientIP != "*" && StringUtil.MatchIpAddressList(list, strRouterClientIP) == false)
            {
                if (alter_type_list != null)
                {
                    alter_type_list.Add("!router_ip"); // 表示虽然处理了，但是没有匹配
                    alter_type_list.AddRange(GetAlterBinding(list));    // 加入替代的绑定类型
                }

                strError = "前端 IP 地址 '" + strRouterClientIP + "' 不在当前账户的 router_ip: 白名单中，访问被拒绝";
                return false;
            }

        END1:
            if (alter_type_list != null)
                alter_type_list.Add("router_ip"); // 表示已经验证了 router_ip: 绑定
            return true;
        }

        // 合并抵消带有星号和和不带星号的同类 type。返回剩下的(曾经)带有星号的类型。这样可以报错给用户，说哪些绑定类型不满足
        // 注: 返回的时候已经去掉了星号
        public static List<string> MergeBindingType(List<string> alter_type_list)
        {
            List<string> results = new List<string>();

            List<string> type_list1 = new List<string>();   // 没有星号的
            List<string> type_list2 = new List<string>();   // 有星号的

            // 取出没有星号和有星号的两个 list
            foreach (string name in alter_type_list)
            {
                if (string.IsNullOrEmpty(name))
                    continue;
                if (name[0] == '!')
                    continue;
                if (name[0] == '*')
                    type_list2.Add(name.Substring(1));
                else
                    type_list1.Add(name);
            }

            if (type_list2.Count == 0)
                return results;

            StringUtil.RemoveDupNoSort(ref type_list1);
            StringUtil.RemoveDupNoSort(ref type_list2);

            foreach (string name in type_list1)
            {
                // name 为没有星号的类型

                if (type_list2.IndexOf(name) != -1)
                    type_list2.Remove(name);
            }

            return type_list2;
        }

        public static bool RemoveAlertBindingType(ref List<string> alter_type_list,
            string type)
        {
            if (alter_type_list == null)
                return false;

            bool bChanged = false;
            for (int i = 0; i < alter_type_list.Count; i++)
            {
                string name = alter_type_list[i];
                if (string.IsNullOrEmpty(name))
                    continue;
                if (name[0] != '*')
                    continue;
                if (name.Substring(1) == type)
                {
                    alter_type_list.RemoveAt(i);
                    i--;
                    bChanged = true;
                }
            }

            return bChanged;
        }

        public static bool HasAlterBindingType(List<string> alter_type_list)
        {
            if (alter_type_list == null)
                return false;
            foreach (string s in alter_type_list)
            {
                if (string.IsNullOrEmpty(s) == false && s[0] == '*')
                    return true;
            }

            return false;
        }

        // 从 IP 列表中获得(不满足时)替代绑定类型列表。注意第一字符为 *
        // '192.168.0.1|*sms' 中 *sms 就是替代绑定类型。
        static List<string> GetAlterBinding(string strText)
        {
            List<string> results = new List<string>();
            List<string> list = StringUtil.SplitList(strText, '|');
            foreach (string s in list)
            {
                if (string.IsNullOrEmpty(s) == false && s[0] == '*')
                {
#if NO
                    string name = s.Substring(1);
                    if (string.IsNullOrEmpty(name) == false)
                        results.Add(name);
#endif
                    results.Add(s); // 注意第一字符为 *
                }
            }

            return results;
        }

        // 从绑定字符串中解析出所有需要登录时候验证的绑定类型
        public static List<string> GetBindingTypes(string strText)
        {
            List<string> results = new List<string>();
            List<string> list = StringUtil.SplitList(strText, ',');
            foreach (string s in list)
            {
                List<string> parts = StringUtil.ParseTwoPart(s, ":");
                string type = parts[0];
                if (type == "ip" || type == "router_ip" || type == "sms")
                    results.Add(type);
            }

            return results;
        }

        // 从 list1 中减去 list2
        public static List<string> Sub(List<string> list1, List<string> list2)
        {
            List<string> results = new List<string>();
            foreach (string s in list1)
            {
                if (list2.IndexOf(s) == -1)
                    results.Add(s);
            }

            return results;
        }

        // 获得处理过的元素。也就是 !ip 和 ip 这样的类型。(即，不是 *ip 这样的类型)
        public static List<string> GetProcessed(List<string> list)
        {
            List<string> results = new List<string>();
            foreach (string s in list)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                if (s[0] != '*')
                {
                    if (s[0] == '!')
                        results.Add(s.Substring(1));
                    else
                        results.Add(s);
                }
            }

            return results;
        }

        // 获得匹配过的元素。也就是 ip 这样的类型。(即，不是 *ip !ip 这样的类型)
        public static List<string> GetMatched(List<string> list)
        {
            List<string> results = new List<string>();
            foreach (string s in list)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                if (s[0] != '*' && s[0] != '!')
                    results.Add(s);
            }

            return results;
        }

        // 从 types 列表中移走那些验证过的可以替代的类型
        // parameters:
        //      types   需要检查的类型
        //      matched   已经验证过的类型
        public static void RemoveAlterTypes(ref List<string> types,
            List<string> matched,
            string strBinding)
        {
            if (matched.Count == 0)
                return;
            List<string> results = new List<string>();
            foreach (string type in types)
            {
                string list = StringUtil.GetParameterByPrefix(strBinding, type);
                if (string.IsNullOrEmpty(list))
                    continue;
                {
                    List<string> alters = GetAlterBinding(list);
                    foreach (string alter in alters)
                    {
                        string name = alter;
                        if (string.IsNullOrEmpty(alter) == false && alter[0] == '*')
                            name = alter.Substring(1);
                        if (matched.IndexOf(name) != -1)
                            goto FOUND;
                    }
                }

                results.Add(type);  // 没有替代
                continue;
            FOUND:
                // 发现了可以替代的
                continue;
            }

            types = results;
        }
    }

    public class BrowseFormat
    {
        public string Name = "";
        public string ScriptFileName = "";
        public string Type = "";


        // 将脚本文件名正规化
        // 因为在定义脚本文件的时候, 有一个当前库名环境,
        // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
        // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
        public static string CanonicalizeScriptFileName(string strDbName,
            string strScriptFileNameParam)
        {
            int nRet = 0;
            nRet = strScriptFileNameParam.IndexOf("./");
            if (nRet != -1)
            {
                // 认为是当前库下
                return strDbName + strScriptFileNameParam.Substring(1);
            }

            nRet = strScriptFileNameParam.IndexOf("/");
            if (nRet != -1)
            {
                // 认为从根开始
                return strScriptFileNameParam.Substring(1);
            }

            return strScriptFileNameParam;  // 保持原样
        }
    }


    // 日历信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class CalenderInfo
    {
        [DataMember]
        public string Name = "";    // 日历名。可以是全局的，例如“基本日历”，也可以是两段式“海淀分馆/基本日历”。分馆用户只能修改属于自己分馆的日历，但可以看到全部日历
        [DataMember]
        public string Range = "";
        [DataMember]
        public string Content = "";
        [DataMember]
        public string Comment = "";
    }

    // 日历对象。用于确定哪些日子是工作日
    public class Calendar
    {
        public string Name = "";
        RangeList m_range = null;

        public Calendar(string strName,
            string strData)
        {
            this.Name = strName;
            this.m_range = new RangeList(strData);
            this.m_range.Sort();
            this.m_range.Merge();
        }


        // 检测一个时间值是否处于非工作日内？
        // 如果是，同时返回最近的下一个工作日的时刻（如果不是，则不返回）
        public bool IsInNonWorkingDay(DateTime time,
            out DateTime nextWorkingDay)
        {
            nextWorkingDay = DateTime.MinValue;

            long lDay = DateTimeUtil.DateTimeToLong8(time);

            bool bFound = false;

            long lNextWorkingDay = 0;

            for (int i = 0; i < this.m_range.Count; i++)
            {
                RangeItem item = (RangeItem)this.m_range[i];

                Debug.Assert(item.lLength >= 1, "");

                if (bFound == false)
                {
                    if (lDay >= item.lStart
                        && lDay < item.lStart + item.lLength)
                    {
                        // 本item末端时间
                        long lEndDay = item.lStart + item.lLength - 1;

                        DateTime t = DateTimeUtil.Long8ToDateTime(lEndDay);

                        // 24小时后的时间
                        TimeSpan delta = new TimeSpan(24, 0, 0);
                        nextWorkingDay = t + delta;
                        lNextWorkingDay = DateTimeUtil.DateTimeToLong8(nextWorkingDay);
                        bFound = true;
                    }
                }
                else // bFound == true
                {
                    if (lNextWorkingDay >= item.lStart
                        && lNextWorkingDay < item.lStart + item.lLength)
                    {
                        long lEndDay = item.lStart + item.lLength - 1;

                        // 说明预测的非工作日是在下一段非工作日范围内，那么就还要向后继续找断点
                        DateTime t = DateTimeUtil.Long8ToDateTime(lEndDay);
                        TimeSpan delta = new TimeSpan(24, 0, 0);    // 24小时
                        nextWorkingDay = t + delta;
                        lNextWorkingDay = DateTimeUtil.DateTimeToLong8(nextWorkingDay);
                    }
                    else
                    {
                        // 找到断点了，结束
                        return true;
                    }
                }
            }

            if (bFound == false)
                return false;

            return true;
        }

        // 排除非工作日，获得和起点时间相隔一段距离的末端时间值
        public DateTime GetEndTime(DateTime start,
            TimeSpan distance)
        {
            Debug.Assert(distance.Ticks >= 0, "distance必须为正值");

            // long lDay = DateTimeToLong8(start);

            long nDeltaDays = (long)distance.TotalDays;

            long nDayCount = 0;

            DateTime curDay = start;

            //    DateTime curDay = Long8ToDateTime(lDay);
            for (; ; )
            {
                bool bNon = IsNonWorkingDay(DateTimeUtil.DateTimeToLong8(curDay));

                if (bNon == true)   // BUG !!! 2007/1/15
                    goto CONTINUE;

                if (nDayCount >= nDeltaDays)
                    break;

                nDayCount++;


            CONTINUE:
                TimeSpan delta = new TimeSpan(24, 0, 0);    // 24小时
                curDay = curDay + delta;
            }

            return curDay;
        }

        // 是不是 非工作日?
        public bool IsNonWorkingDay(long lDay)
        {
            for (int i = 0; i < this.m_range.Count; i++)
            {
                RangeItem item = (RangeItem)this.m_range[i];

                Debug.Assert(item.lLength >= 1, "");

                if (lDay >= item.lStart
    && lDay < item.lStart + item.lLength)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ItemDbCfg
    {
        public string DbName = "";  // 实体库名
        public string BiblioDbName = "";    // 书目库名
        public string BiblioDbSyntax = "";  // 书目库MARC语法

        public string IssueDbName = ""; // 期库
        public string OrderDbName = ""; // 订购库 2007/11/27 
        public string CommentDbName = "";   // 评注库 2008/12/8 

        public string UnionCatalogStyle = "";   // 联合编目特性 905  // 2007/12/15 

        public string Replication = "";   // 复制  // 2013/11/19
        public string ReplicationServer = "";   // 复制-服务器名 用于加速访问
        public string ReplicationDbName = "";   // 复制-书目库名 用于加速访问

        public bool InCirculation = true;   // 2008/6/4 

        public string Role = "";    // 角色 biblioSource/orderWork // 2009/10/23 
    }

    // API ListBiblioDbFroms()所使用的结构
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class BiblioDbFromInfo
    {
        [DataMember]
        public string Caption = ""; // 字面标签
        [DataMember]
        public string Style = "";   // 角色
    }

    // API ListFile()所使用的结构
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class FileItemInfo
    {
        [DataMember]
        public string Name = ""; // 文件(或目录)名
        [DataMember]
        public string CreateTime = "";   // 创建时间。本地时间 "u" 字符串

        // 2017/4/8
        [DataMember]
        public string LastWriteTime = "";   // 本地时间 "u" 字符串
        [DataMember]
        public string LastAccessTime = "";   // 本地时间 "u" 字符串

        [DataMember]
        public long Size = 0;   // 尺寸。-1 表示这是目录对象
    }

    public class RemoteAddress
    {
        public string ClientIP { get; set; }
        public string Via { get; set; }
        public string Type { get; set; }

        public RemoteAddress()
        {

        }

        public RemoteAddress(string ip, string via, string type)
        {
            this.ClientIP = StringUtil.CanonicalizeIP(ip);
            this.Via = via;
            this.Type = type;
        }

        public static RemoteAddress FindClientAddress(List<RemoteAddress> list, string type)
        {
            if (list == null)
                return null;
            foreach (RemoteAddress address in list)
            {
                if (address.Type == type)
                    return address;
            }

            return null;
        }
    }

}
