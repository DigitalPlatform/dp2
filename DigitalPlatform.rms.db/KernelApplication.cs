using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // 全局信息
    public partial class KernelApplication : IDisposable
    {
        public static string Version
        {
            get
            {
                Assembly assembly = Assembly.GetAssembly(typeof(KernelApplication));
                Version version = assembly.GetName().Version;
                return version.Major + "." + version.Minor;
            }
        }

        public static string FullVersion
        {
            get
            {
                Assembly assembly = Assembly.GetAssembly(typeof(KernelApplication));
                Version version = assembly.GetName().Version;
                return version.ToString();
            }
        }
        // private string m_strLogFileName = "";	//日志文件名称
        private string m_strDebugFileName = "";	// 
        public bool DebugMode = false;

        public string SessionDir = "";  // session临时数据目录
        public string LogDir = "";  // 日志文件目录
        public string DataDir = "";	// 程序目录路径
        public string ResultsetDir = "";    // 全局结果集目录

        // 数据库集合
        public DatabaseCollection Dbs = null;

        // 用户集合
        public UserCollection Users = null;

        // 防止试探密码攻击的设施
        public UserNameTable UserNameTable = new UserNameTable("dp2kernel");


        #region 工作线程

        Thread threadWorker = null;
        AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        AutoResetEvent eventActive = new AutoResetEvent(false);	// 通用激活信号
        AutoResetEvent eventCommit = new AutoResetEvent(false);	// Commit激活信号
        AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        private int PerTime = 5 * 60 * 1000;	// 5分钟?

        public void Dispose()
        {
            // TODO: 加入 this.Close()。还需要进行一些改造
            eventClose.Dispose();
            eventActive.Dispose();
            eventCommit.Dispose();
            eventFinished.Dispose();
        }

        // 启动工作线程
        public void StartWorkerThread()
        {
            this.threadWorker =
                new Thread(new ThreadStart(this.ThreadMain));
            this.threadWorker.Start();
        }

        // 激活工作线程
        public void ActivateWorker()
        {
            this.eventActive.Set();
        }

        // 激活Commit动作
        public void ActivateCommit()
        {
            this.eventCommit.Set();
        }

        // 工作线程
        // TODO: 要确保所有异常都被捕获。否则线程会被退出，不再具备监控能力
        public void ThreadMain()
        {
            try
            {
                WaitHandle[] events = new WaitHandle[3];

                events[0] = eventClose;
                events[1] = eventActive;
                events[2] = eventCommit;

                while (true)
                {
                    int index = WaitHandle.WaitAny(events, PerTime, false);

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // timeout
                        eventActive.Reset();
                        // 超时请况下做事
                        TryShrink();

                        TryCommit();

                        if (this.Dbs != null)
                        {
                            // 定时保存一下databases.xml的修改
                            try
                            {
                                this.Dbs.SaveXmlSafety(true);
                            }
                            catch (Exception ex)
                            {
                                this.WriteErrorLog("管理线程 保存databases.xml文件时遇到异常:" + ExceptionUtil.GetDebugText(ex));
                            }
                        }

                        if (this.ResultSets != null)
                        {
                            try
                            {
                                // TODO: 在日志中记载全局结果集个数
                                this.ResultSets.Clean(new TimeSpan(1, 0, 0));   // 一个小时
                                // this.ResultSets.Clean(new TimeSpan(0, 5, 0));   // 5 分钟
                            }
                            catch (Exception ex)
                            {
                                this.WriteErrorLog("管理线程中 ResultSets.Clean() 遇到异常:" + ExceptionUtil.GetDebugText(ex));
                            }
                        }

                        if (this.Dbs != null)
                        {
                            // 定时保存一下databases.xml的修改
                            try
                            {
                                this.Dbs.ClearStreamCache();
                            }
                            catch (Exception ex)
                            {
                                this.WriteErrorLog("管理线程 ClearStreamCache() 时遇到异常:" + ExceptionUtil.GetDebugText(ex));
                            }
                        }

                        TryVerifyTailNumber();
                    }
                    else if (index == 0)
                    {
                        // closing
                        break;
                    }
                    else if (index == 1)
                    {
                        // be activating
                        eventActive.Reset();

                        // 得到通知的情况下做事
                        TryShrink();

                        /// 
                        TryVerifyTailNumber();
                    }
                    else if (index == 2)
                    {
                        eventCommit.Reset();

                        TryCommit();
                    }

                }

                eventFinished.Set();
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("管理线程异常(线程已退出):" + ExceptionUtil.GetDebugText(ex));
            }
        }

        void TryCommit()
        {
            if (this.Dbs != null)
            {
                try
                {
                    this.Dbs.Commit();
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("管理线程中 Commmit() 遇到异常:" + ExceptionUtil.GetDebugText(ex));
                }
            }
        }

        void TryShrink()
        {
            if (this.Users != null)
            {
                try
                {
                    this.Users.Shrink();
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("管理线程中 Users.Shrink() 遇到异常:" + ExceptionUtil.GetDebugText(ex));
                }
            }
        }

        DateTime _lastVerifyTime = new DateTime(0); // 最近一次重试校验尾号的时刻
        int _retryVerifyCount = 0;  // 重试校验尾号的次数

        void TryVerifyTailNumber()
        {
            // 重试10次以后，每次重试间隔拉长到 20 分钟以上
            // 10次以内，是按需执行的
            if (this._retryVerifyCount > 10
                && DateTime.Now - this._lastVerifyTime < new TimeSpan(0, 20, 0))
                return;

            if (this.Dbs != null && this.Dbs.AllTailNoVerified == false)
            {
                try
                {
                    string strError = "";
                    int nRet = this.Dbs.CheckDbsTailNo(out strError);
                    if (nRet == -1)
                        this.WriteErrorLog("ERR002 重试校验数据库尾号发生错误:" + strError);
                    else
                        this.WriteErrorLog("INF001 重试校验数据库尾号成功。");

                    this._lastVerifyTime = DateTime.Now;
                    this._retryVerifyCount++;
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("管理线程 重试校验数据库尾号时遇到异常:" + ExceptionUtil.GetDebugText(ex));
                }
            }
        }

        #endregion

        #region 全局结果集管理

        // 全局结果集
        public ResultSetTable ResultSets = new ResultSetTable();

        public static bool IsGlobalResultSetName(string strResultSetName)
        {
            if (string.IsNullOrEmpty(strResultSetName) == false && strResultSetName[0] == '#')
                return true;
            return false;
        }
        #endregion

        // 把错误信息写到日志文件里
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
                // 有可能手工把文件删除了，导致文件不存在，抛出异常。
                // 要在安装程序中预先创建事件源

                EventLog Log = new EventLog();
                Log.Source = "dp2Kernel";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        public void MyWriteDebugInfo(string strTitle)
        {
            if (this.DebugMode == false)
                return;

            lock (this.m_strDebugFileName)
            {
                StreamUtil.WriteText(this.m_strDebugFileName, "-- " + DateTime.Now.ToString("u") + " " + strTitle + "\r\n");
            }
        }

        void CleanSessionDir(string strSessionDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strSessionDir);
                if (di.Exists == true)
                {
                    // 删除所有的下级目录
                    DirectoryInfo[] dirs = di.GetDirectories();
                    foreach (DirectoryInfo childDir in dirs)
                    {
                        Directory.Delete(childDir.FullName, true);
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("删除 session 下级目录时出错: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        // 初始化GlobalInfo
        // parameters:
        //		strDataDir	数据目录
        //		strError	out参数，返回出错信息
        // return:
        //		-1	error
        //		0	successed
        public int Initial(string strDataDir,
            string strBinDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            DateTime start = DateTime.Now;

            this.DataDir = strDataDir;

            this.SessionDir = PathUtil.MergePath(this.DataDir, "session");

            // 清除 session 目录中的全部下级目录

            // 日志文件
            string strLogDir = this.DataDir + "\\log";
            try
            {
                PathUtil.TryCreateDir(strLogDir);
            }
            catch (Exception ex)
            {
                DirectoryInfo di = new DirectoryInfo(this.DataDir);
                if (di.Exists == false)
                    strError = "创建日志目录出错: '" + ex.Message + "', 原因是上级目录 '" + this.DataDir + "' 不存在...";
                else
                    strError = "创建日志目录出错: " + ex.Message;
                return -1;
            }

            this.LogDir = strLogDir;

            // this.m_strLogFileName = strLogDir + "\\log.txt";
            this.m_strDebugFileName = strLogDir + "\\debug.txt";

            this.WriteErrorLog("kernel (" + KernelApplication.FullVersion + ") application 开始初始化");

            CleanSessionDir(this.SessionDir);

            // 全局结果集目录
            string strResultSetDir = Path.Combine(this.DataDir, "resultsets");
            try
            {
                PathUtil.TryCreateDir(strResultSetDir);
            }
            catch (Exception ex)
            {
                DirectoryInfo di = new DirectoryInfo(this.DataDir);
                if (di.Exists == false)
                    strError = "创建全局结果集目录出错: '" + ex.Message + "', 原因是上级目录 '" + this.DataDir + "' 不存在...";
                else
                    strError = "创建全局结果集目录出错: " + ex.Message;
                return -1;
            }

            // 清除以前遗留的结果集文件
            CleanResultSetDir(strResultSetDir);

            this.ResultsetDir = strResultSetDir;

            this.ResultSets.ResultsetDir = strResultSetDir;

            // 初始化数据库集合
            this.Dbs = new DatabaseCollection();
            try
            {
                nRet = this.Dbs.Initial(
                    this,
                    // strDataDir,
                    strBinDir,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            catch (Exception ex)
            {
                strError = "dbs的Initialize()抛出异常：" + ex.Message;
                return -1;
            }

            // 检验各个数据库记录尾号
            // return:
            //      -1  出错
            //      0   成功
            // 线：安全
            nRet = this.Dbs.CheckDbsTailNo(out strError);
            if (nRet == -1)
            {
                // 虽然发生错误，但是初始化过程继续进行
                this.WriteErrorLog("ERR001 首次校验数据库尾号发生错误:" + strError);
            }

            /*
            // 初始化用户库集合
            UserDatabaseCollection userDbs = new UserDatabaseCollection();
            // 初始化用户库集合对象
            // paramter:
            //		dbColl  总数据库集合
            //      strError    out参数，返回出错信息
            // return:
            //      -1  出错
            //      0   成功
            // 线：安全
            nRet = userDbs.Initial(this.Dbs,
                out strError);
            if (nRet == -1)
                return -1;
             */

            // 初始化用户集合
            this.Users = new UserCollection();
            nRet = this.Users.Initial(this,
                out strError);
            if (nRet == -1)
                return -1;

            // 把帐户集合对象的指针传给DatabaseCollection对象。
            // this.Dbs.UserColl = Users;

            // 启动工作线程
            StartWorkerThread();

            TimeSpan delta = DateTime.Now - start;
            this.WriteErrorLog("kernel application 成功初始化。初始化操作耗费时间 " + delta.TotalSeconds.ToString() + " 秒");
            return 0;
        }

        void CleanResultSetDir(string strResultSetDir)
        {
            if (PathUtil.TryClearDir(strResultSetDir) == false)
                this.WriteErrorLog("清除 结果集目录 " + strResultSetDir + " 时出错");
        }

        // 整数返回值转换为ErrorCode
        public static ErrorCodeValue Ret2ErrorCode(int nRet)
        {
            if (nRet == -1)
                return ErrorCodeValue.CommonError;
            else if (nRet == -2)
                return ErrorCodeValue.TimestampMismatch;
            else if (nRet == -3)
                return ErrorCodeValue.EmptyContent;
            else if (nRet == -4)
                return ErrorCodeValue.NotFound;
            else if (nRet == -5)
                return ErrorCodeValue.NotFoundDb;
            else if (nRet == -6)
                return ErrorCodeValue.NotHasEnoughRights;
            else if (nRet == -7)
                return ErrorCodeValue.PathError;
            else if (nRet == -8)
                return ErrorCodeValue.AlreadyExist;
            else if (nRet == -9)
                return ErrorCodeValue.AlreadyExistOtherType;
            else if (nRet == -10)
                return ErrorCodeValue.PartNotFound;
            else if (nRet == -11)
                return ErrorCodeValue.ExistDbInfo;
            else if (nRet == -100)
                return ErrorCodeValue.NotFoundObjectFile; // 对象文件不存在
            else
                return ErrorCodeValue.CommonError;
        }

        // 版本信息
        public string GetCopyRightString()
        {
            string strResult = "";

            strResult = "(C) 版权所有 2005-2015 数字平台(北京)软件有限责任公司\r\nDigital Platform (Beijing) Software Corp. Ltd.<br/>";

            Assembly myAssembly;

            myAssembly = Assembly.GetAssembly(this.GetType());
            strResult += "本机 .NET Framework 版本: " + myAssembly.ImageRuntimeVersion
                + "\r\n" + myAssembly.FullName;

            return strResult;
        }

        internal CancellationTokenSource _app_down = new CancellationTokenSource();

        // TODO: 改进为可以重复调用。然后被 Dispose() 调用
        // 关闭
        public void Close()
        {
            _app_down.Cancel();

            eventClose.Set();	// 令工作线程退出

            // 等待工作线程真正退出
            // 因为可能正在回写数据库
            eventFinished.WaitOne(5000, false); // 最多5秒

            if (this.Dbs != null)
            {
                try
                {
                    this.Dbs.Close();
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("Dbs Close() error : " + ex.Message);
                }
                // this.Dbs.WriteErrorLog("在GlobalInfo.Close()处保存database.xml");
            }

            try
            {
                if (this.Users != null)
                {
                    this.Users.Close();
                    this.Users = null;
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("Users Close() error : " + ex.Message);
            }

            if (this.ResultSets != null)
            {
                try
                {
                    this.ResultSets.Clean(new TimeSpan(0, 0, 0));   // 立即全部清除
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("释放 ResultSets 遇到异常:" + ExceptionUtil.GetDebugText(ex));
                }
            }


            this.WriteErrorLog("kernel application 成功降落。");
            this.Dbs = null;
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
            Log.Source = "dp2Kernel";
            Log.WriteEntry(strText, type);
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class KeyFrom
    {
        [DataMember]
        public string Logic = "";
        [DataMember]
        public string Key = "";
        [DataMember]
        public string From = "";
    }

    // 设计意图：记录对象，API用来传信息的类
    // 记录对象
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class Record
    {
        [DataMember]
        public string Path = "";      // 带库名的全路径 原来叫ID 2010/5/17 changed
        [DataMember]
        public KeyFrom[] Keys = null;     // 检索命中的key+from字符串数组 
        [DataMember]
        public string[] Cols = null;

        [DataMember]
        public RecordBody RecordBody = null;    // 记录体。2012/1/5
    }

    // 2012/1/5
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class RecordBody
    {
        [DataMember]
        public string Path = "";      // [in] 希望写入的记录路径 [out]实际写入的路径 2012/11/11
        [DataMember]
        public string Xml = "";
        [DataMember]
        public byte[] Timestamp = null;
        [DataMember]
        public string Metadata = "";

        [DataMember]
        public Result Result = new Result(); // 结果信息
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class RichRecord
    {
        [DataMember]
        public string Path = "";
        [DataMember]
        public string[] Cols = null;

        [DataMember]
        public string Xml = "";
        [DataMember]
        public byte[] baTimestamp = null;
        [DataMember]
        public string strMetadata = "";

        [DataMember]
        public Result Result = new Result(); // 结果信息
    }

#if NO
    // 2012/11/11
    // 用于传递写入记录信息的结构
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class UploadRecord
    {
        /*
        [DataMember]
        public byte[] OutputTimestamp = null;   // [out]写入后返回的最新时间戳
        [DataMember]
        public string OutputPath = "";      // [out]实际写入的记录路径
         * */

        [DataMember]
        public RecordBody RecordBody = null;    // 记录体
    }
#endif


    // 设计意图:KeyInfo对象，用来传信息
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class KeyInfo
    {
        [DataMember]
        public string ID;
        [DataMember]
        public string Key;
        [DataMember]
        public string KeyNoProcess; //未经处理的检索点
        [DataMember]
        public string FromName;  // 来源表名
        [DataMember]
        public string Num;
        [DataMember]
        public string FromValue; // 来源值
    }

    //结果对象
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class Result
    {
        [DataMember]
        public long Value = 0;	// 命中条数，>=0:正常;<0:出错

        [DataMember]
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;
        [DataMember]
        public string ErrorString = ""; // "错误信息未初始化...";

        public Result()
        {
        }

        public Result(
            string strError,
            ErrorCodeValue errorcode,
            int nValue)
        {
            this.ErrorCode = errorcode;
            this.ErrorString = strError;
            this.Value = nValue;
        }

        // 设置值
        public void SetValue(
            string strError = "",
            ErrorCodeValue errorcode = ErrorCodeValue.CommonError,
            int nValue = -1)
        {
            this.ErrorCode = errorcode;
            this.ErrorString = strError;
            this.Value = nValue;
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public enum ErrorCodeValue
    {
        [EnumMember]
        NoError = 0,	 // 没有错误
        [EnumMember]
        CommonError = 1, // 一般性错误   -1

        [EnumMember]
        NotLogin = 2,	// 尚未登录 (Dir/ListTask)
        [EnumMember]
        UserNameEmpty = 3,	// 用户名为空 (Login)
        [EnumMember]
        UserNameOrPasswordMismatch = 4,	// 用户名或者密码错误 (Login)

        //NoHasList = 5,     //没有列目录权限
        //NoHasRead = 6,     //没有读权限          
        //NoHasWrite = 7,    //没有写权限
        //NoHasManagement = 8, //没有管理员权限

        [EnumMember]
        NotHasEnoughRights = 5, // 没有足够的权限 -6

        [EnumMember]
        TimestampMismatch = 9,  //时间戳不匹配   -2
        [EnumMember]
        NotFound = 10, //没找到记录       -4
        [EnumMember]
        EmptyContent = 11,   //空记录  -3

        [EnumMember]
        NotFoundDb = 12,  // 没找到数据库 -5
        //OutOfRange = 13, // 范围越界
        [EnumMember]
        PathError = 14, // 路径不合法  -7

        [EnumMember]
        PartNotFound = 15, // 通过xpath未找到节点 -10

        [EnumMember]
        ExistDbInfo = 16,  //在新建库中，发现已经存在相同的信息 -11

        [EnumMember]
        AlreadyExist = 17,	//已经存在	-8
        [EnumMember]
        AlreadyExistOtherType = 18,		// 存在不同类型的项 -9

        [EnumMember]
        ApplicationStartError = 19,	//Application启动错误

        [EnumMember]
        NotFoundSubRes = 20,    // 部分下级资源记录不存在

        [EnumMember]
        Canceled = 21,    // 操作被放弃 2011/1/19

        [EnumMember]
        AccessDenied = 22,  // 权限不够 2011/2/11

        [EnumMember]
        PartialDenied = 23,  // 部分被拒绝 2012/10/9 本来是为了dp2library准备的

        [EnumMember]
        NotFoundObjectFile = 24,  // 对象文件不存在 -100

        [EnumMember]
        Compressed = 25,  // 返回的内容是压缩过的


        //

        [EnumMember]
        RequestError = 100,

        [EnumMember]
        RequestTimeOut = 112,   //请求超时 2016/1/27

    };

}

