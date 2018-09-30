// #define DEBUG_LOCK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Web;
using System.Runtime.Serialization;

using System.Data.SqlClient;
using System.Data.SQLite;

using MySql.Data;
using MySql.Data.MySqlClient;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

using Ghostscript.NET;

using DigitalPlatform;
using DigitalPlatform.ResultSet;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Range;

namespace DigitalPlatform.rms
{
    // 数据库集合
    // TODO: 全局 static 使用
    public class DatabaseCollection : List<Database>
    {
        public TailNumberManager TailNumberManager = new TailNumberManager();

        public DelayTableCollection DelayTables = null;

        Hashtable m_logicNameTable = new Hashtable();

        // SQL服务器类型
        public string SqlServerTypeString = "";
        // SQL服务器名
        public string SqlServerName = "";

        // SQL服务器类型
        public SqlServerType SqlServerType
        {
            get
            {
                if (this.SqlServerTypeString == "SQLite")
                    return SqlServerType.SQLite;
                if (this.SqlServerTypeString == "MS SQL Server")
                    return SqlServerType.MsSqlServer;
                if (this.SqlServerTypeString == "MySQL Server")
                    return SqlServerType.MySql;
                if (this.SqlServerTypeString == "Oracle")
                    return SqlServerType.Oracle;
                if (string.Compare(this.SqlServerName, "~sqlite", true) == 0)
                    return SqlServerType.SQLite;

                return SqlServerType.MsSqlServer;
            }
        }

        bool m_bAllTailNoVerified = false;  // 是否全部数据库的尾号都被校验过了

        public bool AllTailNoVerified
        {
            get
            {
                return this.m_bAllTailNoVerified;
            }
        }

        public KernelApplication KernelApplication = null;

        public void ActivateCommit()
        {
            if (this.KernelApplication != null)
                this.KernelApplication.ActivateCommit();
        }

        // 帐户集合指针,用于修改帐户库记录时，刷新当前帐户
        public UserCollection UserColl
        {
            get
            {
                // 注：需要等KernelApplication初始化完Users，本成员才能使用
                return this.KernelApplication.Users;
            }
        }

        public string DataDir
        {
            get
            {
                return this.KernelApplication.DataDir;
            }
        }

        public bool Changed = false;	//内容是否发生改变

        // public XmlNode NodeDbs = null;  //<dbs>节点
        public XmlNode NodeDbs
        {
            get
            {
                if (this.m_dom == null)
                    return null;

                return this.m_dom.SelectSingleNode(@"/root/dbs");
                /*
                if (this.NodeDbs == null)
                {
                    strError = "databases.xml配置文件中不存在<dbs>节点，文件不合法，必须至少存在的一个用户库。";
                    return -1;
                }
                 * */
            }
        }

        // public string SessionDir = "";  // session临时数据目录
        public string InstanceName = ""; // 服务器实例名

        public string BinDir = "";//Bin目录，为脚本引用dll服务 2006/3/21加

        public string ObjectDir = "";   // 数据文件目录。2012/1/21

        public string TempDir = "";     // 临时文件目录。2013/2/19

        // 容器本身的锁
        private MyReaderWriterLock m_container_lock = new MyReaderWriterLock();
        private int m_nContainerLockTimeOut = 1000 * 60;	//1分钟


        // 为配置文件专用的锁
        private MyReaderWriterLock m_cfgfile_lock = new MyReaderWriterLock();
        private int m_nCfgFileLockTimeOut = 1000 * 60;	//1分钟

        private string m_strDbsCfgFilePath = "";	// 库配置文件名
        private XmlDocument m_dom = null;	// 库配置文件dom

        public XmlDocument CfgDom
        {
            get
            {
                return this.m_dom;
            }
        }

        public static GhostscriptVersionInfo gvi = null;

        // parameter:
        //		strDataDir	data目录
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        // 线: 安全的
        // 锁：写锁
        public int Initial(
            KernelApplication app,
            // string strDataDir,
            string strBinDir,
            out string strError)
        {
            strError = "";

            this.m_logicNameTable.Clear();

            this.KernelApplication = app;

            if (String.IsNullOrEmpty(strBinDir) == true)
            {
                strError = "DatabaeCollection::Initial()，strBinDir参数值不能为null或空字符串。";
                return -1;
            }
            this.BinDir = strBinDir;

            string path = Path.Combine(this.BinDir, "gsdll32.dll");
            gvi = new GhostscriptVersionInfo(path);


            if (String.IsNullOrEmpty(this.DataDir) == true)
            {
                strError = "DatabaeCollection::Initial()，this.DataDir参数值不能为null或空字符串。";
                return -1;
            }

            Debug.Assert(string.IsNullOrEmpty(this.DataDir) == false, "");
            // this.SessionDir = PathUtil.MergePath(this.DataDir, "session");


            // 对象文件目录
            string strObjectDir = this.DataDir + "\\object";
            try
            {
                PathUtil.TryCreateDir(strObjectDir);
            }
            catch (Exception ex)
            {
                strError = "创建数据对象目录出错: " + ex.Message;
                return -1;
            }
            this.ObjectDir = strObjectDir;

            // 临时文件目录
            string strTempDir = Path.Combine(this.DataDir, "temp");

#if NO
            // 先删除这个目录，然后创建，可以清理以前残留的临时文件
            try
            {
                PathUtil.DeleteDirectory(strTempDir);   // 2013/12/5
            }
            catch
            {
            }
#endif

            try
            {
                PathUtil.TryCreateDir(strTempDir);
            }
            catch (Exception ex)
            {
                strError = "创建(DatabaseCollection)临时文件目录出错: " + ex.Message;
                return -1;
            }

            if (PathUtil.TryClearDir(strTempDir) == false)
                this.KernelApplication.WriteErrorLog("清除临时文件目录 " + strTempDir + " 时出错");

            this.TempDir = strTempDir;

            //**********对库集合加写锁****************
            m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("Initial()，对库集合加写锁。");
#endif
            try
            {


                // databases.xml配置文件
                this.m_strDbsCfgFilePath = this.DataDir + "\\databases.xml";

                this.m_dom = new XmlDocument();
                //this.m_dom.PreserveWhitespace = true; //保存空白
                try
                {
                    this.m_dom.Load(this.m_strDbsCfgFilePath);
                }
                catch (Exception ex)
                {
                    strError = "加载" + this.m_strDbsCfgFilePath + "到dom时出错 " + ex.Message;
                    return -1;
                }

                // 2011/1/7
                bool bValue = false;
                DomUtil.GetBooleanParam(this.m_dom.DocumentElement,
                    "debugMode",
                    false,
                    out bValue,
                    out strError);
                this.KernelApplication.DebugMode = bValue;

                // 检验
                {
                    XmlNode temp = m_dom.SelectSingleNode(@"/root/dbs");
                    if (temp == null)
                    {
                        strError = "databases.xml配置文件中不存在<dbs>节点，文件不合法，必须至少存在的一个用户库。";
                        return -1;
                    }
                }
                /*
                this.NodeDbs = m_dom.SelectSingleNode(@"/root/dbs");
                if (this.NodeDbs == null)
                {
                    strError = "databases.xml配置文件中不存在<dbs>节点，文件不合法，必须至少存在的一个用户库。";
                    return -1;
                }*/

                this.InstanceName = DomUtil.GetAttr(this.NodeDbs, "instancename");

                // 2012/2/18
                XmlNode nodeDataSource = this.m_dom.DocumentElement.SelectSingleNode("datasource");
                if (nodeDataSource == null)
                {
                    strError = "服务器配置文件不合法，未在根元素下定义<datasource>元素";
                    return -1;
                }
                this.SqlServerTypeString = DomUtil.GetAttr(nodeDataSource, "servertype").Trim();
                if (string.IsNullOrEmpty(this.SqlServerTypeString) == false)
                {
                    if (this.SqlServerTypeString != "MS SQL Server"
                        && this.SqlServerTypeString != "MySQL Server"
                        && this.SqlServerTypeString != "Oracle"
                        && this.SqlServerTypeString != "SQLite")
                    {
                        strError = "服务器配置文件不合法，根元素下级的<datasource>元素的'servertype'属性值 '" + this.SqlServerTypeString + "' 不合法。应当为 MS SQL Server/MySQL Server/Oracle SQL Server/SQLite 之一(缺省为 'MS SQL Server')。";
                        return -1;
                    }
                }

                this.SqlServerName = DomUtil.GetAttr(nodeDataSource, "servername").Trim();
                if (string.IsNullOrEmpty(this.SqlServerName) == true)
                {
                    strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'servername'属性，或'servername'属性值为空。";
                    return -1;
                }

                // 先清空
                this.Clear();

                // 根据<database>节点创建Database对象
                int nRet = 0;
                XmlNodeList listDb = this.NodeDbs.SelectNodes("database");
                foreach (XmlNode nodeDb in listDb)
                {
                    // return:
                    //      -1  出错
                    //      0   成功
                    // 线：不安全
                    nRet = this.AddDatabase(nodeDb,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                this.KernelApplication.WriteErrorLog("初始化数据库内存对象完毕。");

                /*
                // 检验各个数据库记录尾号
                // return:
                //      -1  出错
                //      0   成功
                // 线：不安全
                nRet = this.CheckDbsTailNo(out strError);
                if (nRet == -1)
                    return -1;
                 * */

                return 0;
            }
            finally
            {
                //***********对库集合解写锁****************
                m_container_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("Initial()，对库集合解写锁。");
#endif
            }
        }

        public string GetTempFileName()
        {
            Debug.Assert(string.IsNullOrEmpty(this.TempDir) == false, "");
            while (true)
            {
                string strFilename = PathUtil.MergePath(this.TempDir, Guid.NewGuid().ToString());
                if (File.Exists(strFilename) == false)
                {
                    using (FileStream s = File.Create(strFilename))
                    {
                    }
                    return strFilename;
                }
            }
        }

        // 根据node节点创建Database数据库对象，加到集合里
        // parameters:
        //      node    <database>节点
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全
        public int AddDatabase(XmlNode node,
            out string strError)
        {
            Debug.Assert(node != null, "AddDatabase()调用错误，node参数值为能为null。");
            Debug.Assert(String.Compare(node.Name, "database", true) == 0, "AddDatabase()调用错误，node参数值必须为<database>节点。");

            strError = "";

            string strType = DomUtil.GetAttr(node, "type").Trim();

            Database db = null;

            // file类型创建为FileDatabase对象，其它创建为SqlDatabase对象
            if (StringUtil.IsInList("file", strType, true) == true)
                db = new FileDatabase(this);
            else
                db = new SqlDatabase(this);

            // return:
            //		-1  出错
            //		0   成功
            int nRet = db.Initial(node,
                out strError);
            if (nRet == -1)
                return -1;

            this.Add(db);
            this.m_logicNameTable.Clear();
            return 0;
        }

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        // 析构函数
        ~DatabaseCollection()
        {
            /*
            this.Close();
            this.WriteErrorLog("析构DatabaseCollection对象完成。");
             */
        }
#endif

        public void Commit()
        {
            // 2012/2/21
            foreach (Database db in this)
            {
                db.Commit();
            }
        }

        public void Close()
        {
            if (this.DelayTables != null && this.DelayTables.Count != 0)
            {
                try
                {
                    string strError = "";
                    List<RecordBody> results = null;
                    int nRet = this.API_WriteRecords(
                        null,
                        null,
                        "flushkeys",
                        out results,
                        out strError);
                    if (nRet == -1)
                    {
                        this.KernelApplication.WriteErrorLog("DatabaseCollection.Close() flushkeys 出错：" + strError);
                    }
                }
                catch (Exception ex)
                {
                    this.KernelApplication.WriteErrorLog("DatabaseCollection.Close() flushkeys 抛出异常：" + ex.Message);
                }
            }

            // 2012/2/21
            foreach (Database db in this)
            {
                db.Close();
            }
            // 保存内存对象到文件
            this.SaveXmlSafety(true);
        }

        // 把错误信息写到日志文件里
        public void WriteDebugInfo(string strText)
        {
            string strTime = DateTime.Now.ToString();

            StreamUtil.WriteText(this.DataDir + "\\debug.txt",
                 strTime + " " + strText + "\r\n");
        }

        public void ClearStreamCache()
        {
            //**********对库集合加读锁****************
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("ClearStreamCache()，对库集合加读锁。");
#endif
            try
            {
                foreach (Database db in this)
                {
                    if (db is SqlDatabase)
                    {
                        SqlDatabase sql_db = (SqlDatabase)db;
                        sql_db._streamCache.ClearIdle(TimeSpan.FromSeconds(60));

                        sql_db._pageCache.Clean(
                            true,
                            TimeSpan.FromMinutes(2),
                            (filename) =>
                            {
                                sql_db._streamCache.FileDelete(filename);
                            }
                            );
                    }
                }
            }
            finally
            {
                //***********对库集合解读锁****************
                m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("ClearStreamCache()，对库集合解读锁。");
#endif
            }
        }

        // 检验各个数据库记录尾号
        // return:
        //      -1  出错
        //      0   成功
        // 线：安全
        // 异常：可能会抛出异常
        public int CheckDbsTailNo(out string strError)
        {
            strError = "";

            if (this.m_bAllTailNoVerified == true)
                return 0;

            //**********对库集合加写锁****************
            m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("CheckDbsTailNo()，对库集合加写锁。");
#endif
            try
            {
                this.KernelApplication.WriteErrorLog("开始校验数据库尾号。");

                int nRet = 0;
                try
                {
                    int nFailCount = 0;
                    for (int i = 0; i < this.Count; i++)
                    {
                        Database db = (Database)this[i];
                        string strTempError = "";
                        // return:
                        //      -2  连接错误
                        //      -1  出错
                        //      0   成功
                        nRet = db.CheckTailNo(out strTempError);
                        if (nRet < 0)
                        {
                            nFailCount++;
                            strError += strTempError + ";\r\n";
                            if (nRet == -2)
                                return -1;  // 如果是连接出错，没有必要一个一个数据库地试了
                            // 否则继续校验其他数据库
                        }
                    }

                    if (nFailCount == 0)
                        this.m_bAllTailNoVerified = true;

                    // 保存内存对象
                    this.SaveXml();

                    if (nFailCount > 0)
                        return -1;
                }
                catch (Exception ex)
                {
                    strError = "CheckDbsTailNo()抛出异常，原因：" + ex.Message;
                    return -1;
                }

                return 0;
            }
            finally
            {
                //***********对库集合解写锁****************
                m_container_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("CheckDbsTailNo()，对库集合解写锁。");
#endif
            }
        }


        // 把内存dom保存到databases.xml配置文件
        // 一部分节点不变，一部分节点被覆盖
        // 线: 不安全
        // 异常：可能会抛出异常。超时未能锁ApplicationException，IOException
        public void SaveXml()
        {
            if (this.Changed == false)
                return;

            this.m_cfgfile_lock.AcquireWriterLock(this.m_nCfgFileLockTimeOut);
            try
            {
                // 预先保留一个备份文件
                string strBackupFilename = this.m_strDbsCfgFilePath + ".bak";

                if (FileUtil.IsFileExsitAndNotNull(this.m_strDbsCfgFilePath) == true)
                {
                    this.KernelApplication.WriteErrorLog("备份 " + this.m_strDbsCfgFilePath + " 到 " + strBackupFilename);
                    File.Copy(this.m_strDbsCfgFilePath, strBackupFilename, true);
                }

                using (XmlTextWriter w = new XmlTextWriter(this.m_strDbsCfgFilePath,
                    Encoding.UTF8))
                {
                    w.Formatting = Formatting.Indented;
                    w.Indentation = 4;
                    m_dom.WriteTo(w);
                }

                this.Changed = false;

                this.KernelApplication.WriteErrorLog("完成保存内存dom到 '" + this.m_strDbsCfgFilePath + "' 文件。");
            }
            finally
            {
                this.m_cfgfile_lock.ReleaseWriterLock();
            }
        }

        // SaveXml()的安全版本
        public void SaveXmlSafety(bool bNeedLock)
        {
            if (this.Changed == false)
                return;

            if (bNeedLock == true)
            {
                //******************对库集合加读锁******
                m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("SaveXmlSafety()，对库集合加读锁。");
#endif
            }

            try
            {
                this.SaveXml();
            }
            finally
            {
                if (bNeedLock == true)
                {

                    m_container_lock.ReleaseReaderLock();
                    //*************对库集合解读锁***********
#if DEBUG_LOCK
                    this.WriteDebugInfo("SaveXmlSafety()，对库集合解读锁。");
#endif
                }
            }
        }

        // 获得一个用户拥有的(dbo)全部数据库名
        public int GetOwnerDbNames(
            bool bNeedLock,
            string strUserName,
            out List<string> aOwnerDbName,
            out string strError)
        {
            strError = "";

            aOwnerDbName = new List<string>();

            if (bNeedLock == true)
            {
                //******************对库集合加读锁******
                this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("GetOwnerDbNames()，对库集合加读锁。");
#endif
            }

            try
            {

                foreach (Database db in this)
                {
                    if (db.DboSafety == strUserName)
                    {
                        aOwnerDbName.Add(db.GetCaptionSafety(null));
                    }
                }

                return 0;
            }
            finally
            {
                if (bNeedLock == true)
                {
                    this.m_container_lock.ReleaseReaderLock();
                    //*****************对库集合解读锁*************
#if DEBUG_LOCK
                    this.WriteDebugInfo("GetOwnerDbNames()，对库集合解读锁。");
#endif
                }
            }

        }

        // 新建数据库
        // parameter:
        //		user	            帐户对象
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout
        //		strSqlDbName    	指定的Sql数据库名称。可以为 null，系统会自动生成一个,，如果数据库为文件型数据库，则认作数据源目录的名称
        //		strKeysDefault  	keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //      -3	在新建库中，发现已经存在同名数据库, 本次不能创建
        //      -2	没有足够的权限
        //      -1	一般性错误，例如输入参数不合法等
        //      0	操作成功
        // 加锁：写锁
        public int API_CreateDb(User user,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDefault,
            string strBrowseDefault,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strKeysDefault) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strKeysDefault);
                }
                catch (Exception ex)
                {
                    strError = "加载keys配置文件内容到dom出错(2)，原因:" + ex.Message;
                    return -1;
                }
            }

            if (String.IsNullOrEmpty(strBrowseDefault) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strBrowseDefault);
                }
                catch (Exception ex)
                {
                    strError = "加载browse配置文件内容到dom出错，原因:" + ex.Message;
                    return -1;
                }
            }

            string strEnLoginName = "";

            // TODO: 这一段拼接 XML 字符串的过程最好在 XmlDocument 中完成，以避免XML非法字符问题
            // 可以一个逻辑库名也没有，不出错
            string strLogicNames = "";
            for (int i = 0; i < logicNames.Length; i++)
            {
                string strLang = logicNames[i].Lang;
                string strLogicName = logicNames[i].Value;

                // TODO: 这个判断有问题，可以研究一下
                if (strLang.Length != 2
                    && strLang.Length != 5)
                {
                    strError = "语言版本字符串长度只能是2位或者5位,'" + strLang + "'语言版本不合法";
                    return -1;
                }

                if (this.IsExistLogicName(strLogicName, null) == true)
                {
                    strError = "数据库中已存在 '" + strLogicName + "' 逻辑库名";
                    return -3;  // 已存在相同数据库名
                }

                strLogicNames += "<caption lang='" + strLang + "'>" + strLogicName + "</caption>";
                if (String.Compare(logicNames[i].Lang.Substring(0, 2), "en", true) == 0)
                    strEnLoginName = strLogicName;
            }

            strLogicNames = "<logicname>" + strLogicNames + "</logicname>";

            // 检查当前帐户是否有创建数据库的权限
            string strTempDbName = "test";
            if (logicNames.Length > 0)
                strTempDbName = logicNames[0].Value;
            string strExistRights = "";
            bool bHasRight = user.HasRights(strTempDbName,
                ResType.Database,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对数据库没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                return -2;  // 权限不够
            }

            //**********对库集合加写锁****************
            m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("CreateDb()，对库集合加写锁。");
#endif
            try
            {
                if (strType == null)
                    strType = "";

                // 得到库的ID
                string strDbID = Convert.ToString(this.GetNewDbID());

                string strPureCfgsDir = "";
                string strTempSqlDbName = "";
                if (strEnLoginName != "")
                {
                    // TODO: 这里要注意是否有SQL数据库名中不允许的字符？

                    if (this.SqlServerType == rms.SqlServerType.Oracle)
                    {
                        if (strEnLoginName.Length > 3)
                            strEnLoginName = strEnLoginName.Substring(0, 3);
                        strTempSqlDbName = strEnLoginName;
                    }
                    else
                        strTempSqlDbName = strEnLoginName + "_db";

                    strPureCfgsDir = strEnLoginName + "_cfgs";
                }
                else
                {
                    if (this.SqlServerType == rms.SqlServerType.Oracle)
                        strTempSqlDbName = "db_" + strDbID;
                    else
                        strTempSqlDbName = "dprms_" + strDbID + "_db";

                    strPureCfgsDir = "dprms_" + strDbID + "_cfgs";
                }

                if (String.IsNullOrEmpty(strSqlDbName) == true)
                    strSqlDbName = strTempSqlDbName;
                else
                {
                    if (this.SqlServerType == rms.SqlServerType.Oracle
                        && strSqlDbName.Length > 3)
                    {
                        strError = "所指定的 SQL 数据库名 '" + strSqlDbName + "' 不应超过3字符";
                        return -1;
                    }
                }

                if (StringUtil.IsInList("file", strType, true) == false)
                {
                    // TODO: 最好在这里增加检查SQL Sever中已有数据库名的功能
                    strSqlDbName = this.GetFinalSqlDbName(strSqlDbName);

                    if (this.SqlServerType != rms.SqlServerType.Oracle)
                    {
                        // 2007/7/20
                        if (this.InstanceName != "")
                            strSqlDbName = this.InstanceName + "_" + strSqlDbName;
                    }

                    // TODO: 这一步似乎是多余的，因为GetFinalSqlDbName()中已经判断过了
                    if (this.IsExistSqlName(strSqlDbName) == true)
                    {
                        strError = "服务器中已存在SQL库名 '" + strSqlDbName + "'，创建数据库失败。请更换一个新的SQL库名重新创建，或指定一个空的SQL库名令服务器自动发生SQL库名。";
                        return -1;
                    }
                }

                string strDataSource = "";
                if (StringUtil.IsInList("file", strType, true) == true)
                {
                    strDataSource = strSqlDbName;

                    strDataSource = this.GetFinalDataSource(strDataSource);

                    if (this.IsExistFileDbSource(strDataSource) == true)
                    {
                        strError = "不可能的情况，数据库中已存在 '" + strDataSource + "' 文件数据目录";
                        return -1;
                    }

                    string strDataDir = this.DataDir + "\\" + strDataSource;
                    if (Directory.Exists(strDataDir) == true)
                    {
                        strError = "不可能的情况，本地不会有重名的目录。";
                        return -1;
                    }

                    Directory.CreateDirectory(strDataDir);
                }

                strPureCfgsDir = this.GetFinalCfgsDir(strPureCfgsDir);
                // 把配置文件目录自动创建好
                string strCfgsDir = this.DataDir + "\\" + strPureCfgsDir + "\\cfgs";
                if (Directory.Exists(strCfgsDir) == true)
                {
                    strError = "服务器已存在'" + strPureCfgsDir + "'配置文件目录，请指定其它的英文逻辑库名。";
                    return -1;
                }

                Directory.CreateDirectory(strCfgsDir);

                string strPureKeysLocalName = "keys.xml";
                string strPureBrowseLocalName = "browse.xml";

                int nRet = 0;

                // 写keys配置文件
                nRet = DatabaseUtil.CreateXmlFile(strCfgsDir + "\\" + strPureKeysLocalName,
                    strKeysDefault,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 写browse配置文件
                nRet = DatabaseUtil.CreateXmlFile(strCfgsDir + "\\" + strPureBrowseLocalName,
                    strBrowseDefault,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (StringUtil.IsInList("file", strType) == true)
                    strSqlDbName = "";

                // TODO: 这里发生xml片断可能会有小问题，应当用XmlTextWriter来发生?
                string strDbXml = "<database type='" + strType + "' id='" + strDbID + "' localdir='" + strPureCfgsDir
                    + "' dbo='" + user.Name + "'>"  // dbo参数为2006/7/4增加
                    + "<property>"
                    + strLogicNames
                    + "<datasource>" + strDataSource + "</datasource>"
                    + "<seed>0</seed>"
                    + "<sqlserverdb name='" + strSqlDbName + "'/>"
                    + "</property>"
                    + "<dir name='cfgs' localdir='cfgs'>"
                    + "<file name='keys' localname='" + strPureKeysLocalName + "'/>"
                    + "<file name='browse' localname='" + strPureBrowseLocalName + "'/>"
                    + "</dir>"
                    + "</database>";

                this.NodeDbs.InnerXml = this.NodeDbs.InnerXml + strDbXml;

                XmlNodeList nodeListDb = this.NodeDbs.SelectNodes("database");
                if (nodeListDb.Count == 0)
                {
                    strError = "刚新建数据库，不可能一个数据库都不存在。";
                    return -1;
                }

                // 最后一个库为新建的数据库，加到集合里
                XmlNode nodeDb = nodeListDb[nodeListDb.Count - 1];
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.AddDatabase(nodeDb,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 及时加入dbo特性
                user.AddOwnerDbName(strTempDbName);

                // 及时保存到database.xml
                this.Changed = true;
                this.SaveXml();
            }
            finally
            {
                m_container_lock.ReleaseWriterLock();
                //***********对库集合解写锁****************
#if DEBUG_LOCK
				this.WriteDebugInfo("CreateDb()，对库集合解写锁。");
#endif
            }
            return 0;
        }


        // 规范sql数据库名称，只保存数字，大小写字线，下划线。
        // 为GetFinalSqlDbName()编的内部函数
        private void CanonicalizeSqlDbName(ref string strSqlDbName)
        {
            if (strSqlDbName == null)
                strSqlDbName = "";

            for (int i = 0; i < strSqlDbName.Length; i++)
            {
                char myChar = strSqlDbName[i];
                if (myChar == '_')
                    continue;

                if (myChar <= '9' && myChar >= '0')
                    continue;

                if (myChar <= 'z' && myChar >= 'a')
                    continue;

                if (myChar <= 'Z' && myChar >= 'A')
                    continue;

                strSqlDbName = strSqlDbName.Remove(i, 1);
                i--;
            }
        }

        // 得到最终的sql数据库名称
        private string GetFinalSqlDbName(string strSqlDbName)
        {
            if (strSqlDbName == null)
                strSqlDbName = "";

            string strRealSqlDbName = strSqlDbName;

            // 规范化Sql数据库名称
            this.CanonicalizeSqlDbName(ref strRealSqlDbName);


            for (int i = 0; ; i++)
            {
                if (strRealSqlDbName == "")
                {
                    strRealSqlDbName = "dprms_db_" + Convert.ToString(i);
                }

                // 看看是否和当前系统中的已有的sql库名相重
                // 不过，并没有看SQL Server中的实际情况
                if (this.IsExistSqlName(strRealSqlDbName) == false)
                    return strRealSqlDbName;
                else
                    strRealSqlDbName = strRealSqlDbName + Convert.ToString(i);
            }
        }

        // 规范化DataSource目录名
        // 为GetFinalDataSource()编的内部函数
        private void CanonicalizeDir(ref string strDataSource)
        {
            if (strDataSource == null)
                strDataSource = "";

            for (int i = 0; i < strDataSource.Length; i++)
            {
                char myChar = strDataSource[i];

                if (myChar == '\\'
                    || myChar == '/'
                    || myChar == ':'
                    || myChar == '*'
                    || myChar == '?'
                    || myChar == '<'
                    || myChar == '>'
                    || myChar == '|')
                {
                    strDataSource = strDataSource.Remove(i, 1);
                    i--;
                }
            }
        }

        // 得到最终的文件库使用的数据目录
        private string GetFinalDataSource(string strDataSource)
        {
            if (strDataSource == null)
                strDataSource = "";

            string strRealDataSource = strDataSource;

            this.CanonicalizeDir(ref strRealDataSource);

            for (int i = 0; ; i++)
            {
                if (strRealDataSource == "")
                {
                    strRealDataSource = "dprms_db_" + Convert.ToString(i);
                }

                if (this.IsExistFileDbSource(strRealDataSource) == false
                    && Directory.Exists(this.DataDir + "\\" + strRealDataSource) == false)
                {
                    return strRealDataSource;
                }
                else
                {
                    strRealDataSource = strRealDataSource + Convert.ToString(i);
                }
            }
        }

        // 得到最终的数据库使用的配置目录
        private string GetFinalCfgsDir(string strCfgsDir)
        {
            if (strCfgsDir == null)
                strCfgsDir = "";

            string strRealCfgsDir = strCfgsDir;

            this.CanonicalizeDir(ref strRealCfgsDir);

            for (int i = 0; ; i++)
            {
                if (strRealCfgsDir == "")
                {
                    strRealCfgsDir = "dprms_" + Convert.ToString(i) + "_cfgs";
                }

                if (this.IsExistCfgsDir(strRealCfgsDir, null) == false
                    && Directory.Exists(this.DataDir + "\\" + strRealCfgsDir) == false)
                {
                    return strRealCfgsDir;
                }
                else
                {
                    strRealCfgsDir = strRealCfgsDir + Convert.ToString(i);
                }
            }
        }

        // 检查其它库是否已存在相同的sql库名称
        internal bool IsExistSqlName(string strSqlName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database tempDb = (Database)this[i];
                if (!(tempDb is SqlDatabase))
                    continue;

                SqlDatabase sqlDb = (SqlDatabase)tempDb;
                string strDbSqlName = sqlDb.GetSourceName();// 得到Sql数据库名称
                if (String.Compare(strSqlName, strDbSqlName, true) == 0)
                    return true;
            }
            return false;
        }

        // 新得一个可用的数据库ID
        // return:
        //		新ID
        // 说明: 该函数在将字符型ID转换成数值ID时，如果转换不成功，会抛出异常
        private int GetNewDbID()
        {
            int nId = 0;
            // 遍历现有的数据库id，然后得到一个最大值
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                int nDbId = Convert.ToInt32(db.PureID);
                if (nId < nDbId)
                    nId = nDbId;
            }
            nId = nId + 1;
            return nId;
        }

        // 检查其它的库所有语言版本中是否存在相同的逻辑名
        internal bool IsExistLogicName(string strLogicName,
            Database exceptDb)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (exceptDb != null)
                {
                    if (db == exceptDb)
                        continue;
                }
                string strDbAllLogicName = db.GetAllCaption();
                if (StringUtil.IsInList(strLogicName, strDbAllLogicName, true) == true)
                    return true;
            }
            return false;
        }

        // 检索数据库对应的配置目录是否重复
        // parameters:
        //      strCfgsDir  目录名，相对目录
        //      exceptDb    不参考比较的数据库对象
        // return:
        //      true    有重复
        //      false   无重复
        internal bool IsExistCfgsDir(string strCfgsDir,
            Database exceptDb)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (exceptDb != null)
                {
                    if (db == exceptDb)
                        continue;
                }
                string strDbCfgsDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                    db.m_selfNode);

                if (String.Compare(strCfgsDir, strDbCfgsDir, true) == 0)
                    return true;
            }
            return false;
        }

        // 检查是否已存在相同的sql库名称
        internal bool IsExistFileDbSource(string strSource)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (!(db is FileDatabase))
                    continue;
                string strDbSource = ((FileDatabase)db).m_strPureSourceDir;
                if (String.Compare(strSource, strDbSource, true) == 0)
                    return true;
            }
            return false;
        }


        // 删除数据库
        // parameters:
        //		strDbName	数据库名称，可以是各种语言版本的逻辑名，也可以是id号
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //      -4  数据库不存在  2008/4/27
        //      -5  未找到数据库
        //		-6	无足够的权限
        //		0	成功
        // 加锁：写锁
        public int API_DeleteDb(User user,
            string strDbName,
            out string strError)
        {
            strError = "";

            if (user == null)
            {
                strError = "DeleteDb()调用错误，user参数不能为null。";
                return -1;
            }
            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "DeleteDb()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            //**********对库集合加写锁****************
            m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("DeleteDb()，对库集合加写锁。");
#endif
            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库";
                    return -4;
                }

                // 检查当前帐户是否有写权限
                string strExistRights = "";
                bool bHasRight = user.HasRights(db.GetCaption("zh-CN"),
                    ResType.Database,
                    "delete",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // 调database的Delete()函数，删除该库使用的配置文件，与物理数据库
                // return:
                //      -1  出错
                //      0   成功
                int nRet = db.Delete(out strError);
                if (nRet == -1)
                    return -1;

                //this.m_nodeDbs.RemoveChild(db.m_selfNode);
                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDbName);
                if (nodes.Count != 1)
                {
                    strError = "未找到名为'" + db.GetCaption("zh") + "'的数据库。";
                    return -5;
                }
                this.NodeDbs.RemoveChild(nodes[0]);

                // 删除内存对象
                this.Remove(db);
                this.m_logicNameTable.Clear();


                // 及时除去dbo特性
                user.RemoveOwerDbName(strDbName);


                // 及时保存到database.xml
                this.Changed = true;
                this.SaveXml();

                return 0;
            }
            finally
            {
                m_container_lock.ReleaseWriterLock();
                //***********对库集合解写锁****************
#if DEBUG_LOCK
				this.WriteDebugInfo("DeleteDb()，对库集合解写锁。");
#endif
            }
        }

        // 获得数据定义方面的信息
        // parameters:
        //      strStyle            获得那些输出参数? all表示全部 分别指定则是logicnames/type/sqldbname/keystext/browsetext
        // return:
        //      -1  一般性错误
        //      -5  未找到数据库对象
        //      -6  没有足够的权限
        //      0   成功
        public int API_GetDbInfo(
            bool bNeedLock,
            User user,
            string strDbName,
            string strStyle,
            out LogicNameItem[] logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysText,
            out string strBrowseText,
            out string strError)
        {
            strError = "";

            logicNames = null;
            strType = "";
            strSqlDbName = "";
            strKeysText = "";
            strBrowseText = "";

            Debug.Assert(user != null, "GetDbInfo()调用错误，user参数不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "GetDbInfo()调用不合法，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 检查当前帐户是否有显示权限
            string strExistRights = "";
            bool bHasRight = user.HasRights(strDbName,
                ResType.Database,
                "read",
                out strExistRights);

            if (bNeedLock == true)
            {
                //******************对库集合加读锁******
                this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("GetDbInfo()，对库集合加读锁。");
#endif
            }

            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库。";
                    return -5;
                }

                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'读(read)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1	出错
                //		0	正常
                return db.GetInfo(
                    strStyle,
                    out logicNames,
                    out strType,
                    out strSqlDbName,
                    out strKeysText,
                    out strBrowseText,
                    out strError);
            }
            finally
            {
                if (bNeedLock == true)
                {
                    this.m_container_lock.ReleaseReaderLock();
                    //*****************对库集合解读锁*************
#if DEBUG_LOCK
                    this.WriteDebugInfo("GetDbInfo()，对库集合解读锁。");
#endif
                }
            }
        }




        // 设置数据库基本信息
        // parameter:
        //		strDbName	        数据库名称
        //		strLang	            对应的语言版本，如果语言版本为null或者为空字符串，则从所有的语言版本中找
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout，目前无效，因为涉及到是文件库，还是sql库的问题
        //		strSqlDbName	    指定的新Sql数据库名称, 目前无效
        //		strKeysDefault	    keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //      -1  一般性错误
        //      -2  已存在同名的数据库
        //      -5  未找到数据库对象
        //      -6  没有足够的权限
        //      0   成功
        // 加锁：读锁
        public int API_SetDbInfo(User user,
            string strDbName,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysText,
            string strBrowseText,
            out string strError)
        {
            strError = "";

            Debug.Assert(user != null, "SetDbInfo()调用错误，user参数不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "SetDbInfo()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            this.m_logicNameTable.Clear();

            // 为避免死锁的问题，将查看权限的函数放在外面了
            // 检查当前帐户是否有覆盖数据库结构的权限
            string strExistRights = "";
            bool bHasRight = user.HasRights(strDbName,
                ResType.Database,
                "overwrite",
                out strExistRights);

            //******************对库集合加读锁******
            this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("SetDbInfo()，对库集合加读锁。");
#endif
            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库。";
                    return -5;
                }

                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1	出错
                //      -2  已存在同名的数据库
                //		0	成功
                int nRet = db.SetInfo(logicNames,
                    strType,
                    strSqlDbName,
                    strKeysText,
                    strBrowseText,
                    out strError);
                if (nRet <= -1)
                    return nRet;

                // 及时保存databases.xml
                this.Changed = true;
                this.SaveXml();

                return 0;
            }
            finally
            {
                this.m_container_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("SetDbInfo()，对库集合解读锁。");
#endif
            }

        }


        // ???对库集合加读锁
        // 初始化数据库
        // parameters:
        //      user    帐户对象
        //      strDbName   数据库名称
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      -5  数据库不存在
        //      -6  权限不够
        //      0   成功
        // 线：安全 代码没跟上？？？
        public int API_InitializePhysicalDatabase(User user,
            string strDbName,
            out string strError)
        {
            strError = "";
            Debug.Assert(user != null, "InitializeDb()调用错误，user参数值不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "InitializeDb()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 1.得到数据库
            Database db = this.GetDatabaseSafety(strDbName);
            if (db == null)
            {
                strError = "没有找到名为'" + strDbName + "'的数据库";
                return -5;
            }

            string strExistRights = "";
            bool bHasRight = user.HasRights(db.GetCaption("zh-CN"),
                ResType.Database,
                "clear",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'初始化(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            // 3.初始化
            // return:
            //		-1  出错
            //		0   成功
            return db.InitialPhysicalDatabase(out strError);
        }

        // 刷新数据库定义
        // parameters:
        //      user    帐户对象
        //      strAction   动作。begin为开始刷新。end为结束刷新
        //      strDbName   数据库名称
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      -5  数据库不存在
        //      -6  权限不够
        //      0   成功
        // 线：安全 代码没跟上？？？
        public int API_RefreshPhysicalDatabase(
            // SessionInfo sessioninfo,
            User user,
            string strAction,
            string strDbName,
            bool bClearAllKeyTables,
            out string strError)
        {
            strError = "";
            Debug.Assert(user != null, "RefreshDb()调用错误，user参数值不能为null。");

#if NO
            if (strAction != "begin"
                && strAction != "end"
                && strAction != "beginfastappend"
                && strAction != "endfastappend"
                && strAction != "flushpendingkeys")
            {
                strError = "strAction参数值必须为 begin/end/beginfastappend/endfastappend/flushpendingkeys 之一";
                return -1;
            }
#endif

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "RefreshDb()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 1.得到数据库
            Database db = this.GetDatabaseSafety(strDbName);
            if (db == null)
            {
                strError = "没有找到名为 '" + strDbName + "' 的数据库";
                return -5;
            }

            string strExistRights = "";
            bool bHasRight = user.HasRights(db.GetCaption("zh-CN"),
                ResType.Database,
                "clear",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为 '" + user.Name + "'，对 '" + strDbName + "' 数据库没有'初始化或刷新定义(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            if (strAction == "begin")
            {
                // 2009/7/19
                if (bClearAllKeyTables == true)
                {
                    db.InRebuildingKey = true;
                }

                // 3.刷新定义
                // return:
                //		-1  出错
                //		0   成功
                return db.RefreshPhysicalDatabase(bClearAllKeyTables, out strError);
            }
            else if (strAction == "end")
            {
                Debug.Assert(strAction == "end", "");

                db.InRebuildingKey = false;
                return 0;
            }
            else if (strAction == "deletekeysindex")
            {
                return db.ManageKeysIndex(
                    "delete",
                    out strError);
            }
            else if (strAction == "createkeysindex")
            {
                return db.ManageKeysIndex(
                    "create",
                    out strError);
            }
            else if (strAction == "disablekeysindex")
            {
                return db.ManageKeysIndex(
                    "disable",
                    out strError);
            }
            else if (strAction == "rebuildkeysindex")
            {
                return db.ManageKeysIndex(
                    "rebuild",
                    out strError);
            }
            else if (strAction == "beginfastappend")
            {
                db.FastAppendTaskCount++;
                if (db.FastAppendTaskCount > 1)
                    return 0;

                Debug.Assert(db.FastAppendTaskCount == 1, "");

                // 准备好刷新检索点的 ID 存储机制
                if (db.RebuildIDs == null)
                {
                    db.RebuildIDs = new RecordIDStorage();
                    if (db.RebuildIDs.Open(this.GetTempFileName(), out strError) == -1)
                        return -1;
                }

#if NO
                // 管理keys表的index
                // parameters:
                //      strAction   delete/create
                return db.ManageKeysIndex(
                    "delete",
                    out strError);
#endif
                return 0;
            }
            else if (strAction == "endfastappend")
            {
                int nRet = 0;

                if (db.FastAppendTaskCount == 0)
                {
                    strError = "对数据库 '" + db.GetCaption("zh-CN") + "' endfastappend 动作的次数多于 beginfastappend 的次数，本次 endfastappend 操作被拒绝";
                    return -1;
                }
                db.FastAppendTaskCount--;
                if (db.FastAppendTaskCount > 0)
                    return 0;

                Debug.Assert(db.FastAppendTaskCount == 0, "");

                // 如果需要刷新检索点
                // 这是因为快速模式中间如果遇到覆盖的情况，当时不方便处理检索点，所以存储下来ID最后处理
                if (db.RebuildIDs != null && db.RebuildIDs.Count > 0)
                {
                    nRet = db.RebuildKeys(
                        "fastmode", // 不需要 deletekeys，因为每条的过程中已经把旧记录的 keys 都删除过了
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 将所有延迟堆积的行成批写入相关 keys 表
                    int nKeysCount = nRet;
                }

                // 将所有延迟堆积的行成批写入相关 keys 表
                // TODO: 根据是否有 delaytable 来决定 Buikcopy 是否进行。因为删除 B+ 树然后 Buikcopy 动作较大(特别是原有库中记录很多但本次追加的其实不多的情况)，如果可能应尽量避免
                // 可以考虑一个算法，根据转入前数据库中已有的记录数量和本次追加的 keys 数量进行比较，如果追加的数量很少，就不值得删除 B+ 树然后重建
                {
                    bool bNeedDropIndex = false;
                    long lSize = db.BulkCopy(
                        // sessioninfo,
                        "getdelaysize",
                        out strError);
                    if (lSize == -1)
                        return -1;

                    if (lSize > 10 * 1024 * 1024)   // 10 M
                        bNeedDropIndex = true;

                    // bNeedDropTree = true;   // testing

                    if (bNeedDropIndex == true)
                    {
                        nRet = db.ManageKeysIndex(
            "disable",
            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    long lRet = db.BulkCopy(
                        // sessioninfo,
                        "",
                        out strError);
                    if (lRet == -1)
                        return -1;  // TODO: 是否出错后继续完成后面的操作?

                    if (bNeedDropIndex == true)
                    {
                        // 管理keys表的index
                        // parameters:
                        //      strAction   delete/create
                        nRet = db.ManageKeysIndex(
                            "rebuild",
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    // db.IsDelayWriteKey = false;
                }

                return 0;
            }
            else if (strAction == "flushpendingkeys")
            {
                // 将所有延迟堆积的行成批写入相关 keys 表
                long lRet = db.BulkCopy(
                    // sessioninfo,
                    "",
                    out strError);
                return (int)lRet;
            }

            strError = "API_RefreshPhysicalDatabase() 未知的 strAction 参数值 '" + strAction + "'";
            return -1;
        }

        // 得到key的长度
        // parameters:
        //      nKeySize    out参数，返回检索点长度
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线: 不安全
        public int InternalGetKeySize(
            out int nKeySize,
            out string strError)
        {
            nKeySize = 0;
            strError = "";

            Debug.Assert(this.m_dom != null, "InternalGetKeySize()里发现this.m_dom为null，异常");

            XmlNode nodeKeySize = this.m_dom.DocumentElement.SelectSingleNode("keysize");
            if (nodeKeySize == null)
            {
                strError = "服务器配置文件不合法,未在根下定义<keysize>元素";
                return -1;
            }

            string strKeySize = nodeKeySize.InnerText.Trim(); // 2012/2/16
            try
            {
                nKeySize = Convert.ToInt32(strKeySize);
            }
            catch (Exception ex)
            {
                strError = "服务器配置文件不合法，根下的<keysize>元素的内容不能为'" + strKeySize + "',必须为数字格式。" + ex.Message;
                return -1;
            }

            return 0;
        }

        // 本函数可以自动分析数据库名称格式，找到对应数据库
        // strName: 数据库名 格式为"库名" 或 "@id" 或 "@id[库名]"
        // 线: 安全的
        // 加锁：读锁
        public Database GetDatabaseSafety(
            string strDbName)
        {
            //******************对库集合加读锁******
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetDatabaseSafety()，对库集合加读锁。");
#endif
            try
            {
                return this.GetDatabase(strDbName);
            }
            finally
            {
                m_container_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("GetDatabaseSafety()，对库集合解读锁。");
#endif
            }
        }

        // 根据指定义语言版本的逻辑名找数据库
        // parameters:
        //		strLogicName	逻辑库名
        //		strLang	语言版本
        // return:
        //		找到返回Database对象
        //		没找到返回null
        // 线: 安全的
        public Database GetDatabaseByLogicNameSafety(string strDbName,
            string strLang)
        {
            //******************对库集合加读锁******
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetDatabaseByLogicNameSafety()，对库集合加读锁。");
#endif
            try
            {
                return this.GetDatabaseByLogicName(strDbName,
                    strLang);
            }
            finally
            {
                m_container_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*********
#if DEBUG_LOCK
				this.WriteDebugInfo("GetDatabaseByLogicNameSafety()，对库集合解读锁。");
#endif
            }
        }

        // 根据名称得到一个数据库
        // parameters:
        //		strName	数据库名称，也可以是ID(前面加@)
        // 线: 不安全
        public Database GetDatabase(string strName)
        {
            if (String.IsNullOrEmpty(strName) == true)
            {
                return null;
                // throw new Exception("数据库名不能为空");
            }

            Debug.Assert(String.IsNullOrEmpty(strName) == false, "GetDatabase()调用错误，strName参数值不能为null或空字符串。");

            string strFirst = "";
            string strSecond = "";
            int nPosition = strName.LastIndexOf("[");
            if (nPosition >= 0)
            {
                strFirst = strName.Substring(0, nPosition);
                strSecond = strName.Substring(nPosition + 1);
            }
            else
            {
                strFirst = strName;
            }
            Database db = null;
            if (string.IsNullOrEmpty(strFirst) == false)
            {
                // if (strFirst.Substring(0, 1) == "@")
                if (strFirst[0] == '@')
                    db = GetDatabaseByID(strFirst);
                else
                    db = GetDatabaseByLogicName(strFirst);
            }
            else if (string.IsNullOrEmpty(strSecond) == false)
            {
                // if (strSecond.Substring(0, 1) == "@")

                if (strSecond[0] == '@')
                    db = GetDatabaseByID(strSecond);
                else
                    db = GetDatabaseByLogicName(strSecond);
            }
            return db;
        }


        // 根据逻辑名找数据库，任何语言版本都可以
        // 线: 不安全
        private Database GetDatabaseByLogicName(string strLogicName)
        {
            Debug.Assert(String.IsNullOrEmpty(strLogicName) == false, "GetDatabaseByLogicName()调用错误，strLogicName参数值不能为null或空字符串。");

            // 先从缓存中找
            Database database = (Database)this.m_logicNameTable[strLogicName];
            if (database != null)
                return database;

            foreach (Database db in this)
            {
                if (StringUtil.IsInList(strLogicName,
                    db.GetCaptionsSafety()) == true)
                {
                    this.m_logicNameTable[strLogicName] = db;   // 存入缓存
                    return db;
                }
            }
            return null;
        }

        // 根据指定义语言版本的逻辑名找数据库
        // parameters:
        //		strLogicName	逻辑库名
        //		strLang	语言版本
        // return:
        //		找到返回Database对象
        //		没找到返回null
        // 线: 不安全
        private Database GetDatabaseByLogicName(string strLogicName,
            string strLang)
        {
            // 先从缓存中找
            Database database = (Database)this.m_logicNameTable[strLogicName + "|" + strLang];
            if (database != null)
                return database;

            foreach (Database db in this)
            {
                if (String.Compare(strLogicName, db.GetCaptionSafety(strLang)) == 0)
                {
                    this.m_logicNameTable[strLogicName + "|" + strLang] = db;   // 存入缓存
                    return db;
                }
            }
            return null;
        }

        // 通过数据库ID找到指定的数据库，注意这里的ID带@
        // 线: 不安全
        private Database GetDatabaseByID(string strDbID)
        {
            foreach (Database db in this)
            {
                if (db.FullID == strDbID)
                {
                    return db;
                }
            }
            return null;
        }

        // 检索
        // parameter:
        //		strQuery	检索式XML字符串
        //		resultSet	结果集,用于存放检索结果
        //		oUser	    帐户对象,用于检索该帐户对某库是否有读权限
        //  				为null,则不进行权限的检查，即按有权限算
        //		isConnected	delegate对象,用于通讯是否连接正常
        //					为null，则不调delegate函数
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //      -6  权限不够
        //		0	成功
        // 线: 安全的
        public int API_Search(
            SessionInfo sessioninfo,
            string strQuery,
            ref DpResultSet resultSet,
            User oUser,
            // Delegate_isConnected isConnected,
            ChannelHandle handle,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            DateTime start = DateTime.Now;

            this.Commit();

            //对库集合加读锁*********************************
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("Search()，对库集合加读锁。");
#endif
            try
            {
                if (String.IsNullOrEmpty(strQuery) == true)
                {
                    strError = "Search()调用错误，strQuery不能为null或空字符串";
                    return -1;
                }

                // 一进来先给结果集的m_strQuery成员赋值，
                // 不管是否是合法的XML，在用结果集的时候再判断
                XmlDocument dom = new XmlDocument();
                dom.PreserveWhitespace = true; //设PreserveWhitespace为true
                try
                {
                    dom.LoadXml(strQuery);
                }
                catch (Exception ex)
                {
                    strError += "检索式XML加载到DOM时出错，原因：" + ex.Message + "\r\n"
                        + "检索式内容如下:\r\n"
                        + strQuery;
                    return -1;
                }

                //创建Query对象
                Query query = new Query(this,
                    oUser,
                    dom);

                // 进行检索
                // return:
                //		-1	出错
                //		-6	无权限
                //		0	成功
                int nRet = query.DoQuery(
                    sessioninfo,
                    strOutputStyle,
                    dom.DocumentElement,
                    ref resultSet,
                    handle,
                    // isConnected,
                    out strError);
                if (resultSet != null)
                    resultSet.m_strQuery = strQuery;

                // testing
                // Thread.Sleep(6000);

                // 记载慢速的检索
                TimeSpan length = DateTime.Now - start;
                if (length >= slow_length)
                    KernelApplication.WriteErrorLog("检索式 '" + strQuery + "' 耗时 " + length.ToString() + " (命中条数 " + nRet + ")，超过慢速阈值 " + slow_length.ToString());

                if (nRet <= -1)
                    return nRet;
                return 0;
            }
            finally
            {
                //****************对库集合解读锁**************
                m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("Search()，对库集合解读锁。");
#endif
            }
        }

        // 慢速检索阈值
        static TimeSpan slow_length = TimeSpan.FromSeconds(5);

        #region CopyRecord() 下级函数

        // 判断一个路径是否为追加方式的路径
        bool IsAppendPath(string strResPath)
        {
            string strPath = strResPath;
            string strDbName = StringUtil.GetFirstPartPath(ref strPath);
            //***********吃掉第1层*************
            // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
            if (strPath == "")
                return false;

#if NO
            // 找到数据库对象
            Database db = this.GetDatabase(strDbName);    // 外面已加锁
            if (db == null)
            {
                strError = "名为 '" + strDbName + "' 的数据库不存在。";
                return -5;
            }
#endif

            string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
            //***********吃掉第2层*************
            // 到此为止，strPath不含记录号层了，下级分情况判断
            string strRecordID = strFirstPart;
            // 只到记录号层的路径
            if (strPath == "")
            {
                if (strRecordID == "?"
                    || string.IsNullOrEmpty(strRecordID) == true)
                    return true;
                return false;
            }

            return false;
        }

        static List<string> GetIdList(XmlDocument dom)
        {
            List<string> results = new List<string>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement file in fileList)
            {
                string strObjectID = file.GetAttribute("id");
                if (string.IsNullOrEmpty(strObjectID) == false)
                    results.Add(strObjectID);
            }

            return results;
        }

        // 获得一个未曾用过的 id
        static string GetNewID(List<string> existing_ids)
        {
            for (int i = 0; ; i++)
            {
                string strID = i.ToString();
                if (existing_ids.IndexOf(strID) == -1)
                    return strID;
            }
        }

        // 获得所有 file 元素的 OuterXml
        static List<string> GetFileOuterXmls(XmlDocument dom)
        {
            List<string> results = new List<string>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement file in fileList)
            {
                results.Add(file.OuterXml);
            }

            return results;
        }

        // 获得一个 file 元素的 OuterXml
        static string GetFileOuterXml(XmlDocument dom, string strID)
        {
            List<string> results = new List<string>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNode file = dom.DocumentElement.SelectSingleNode("//dprms:file[@id='" + strID + "']", nsmgr);
            if (file != null)
                return file.OuterXml;
            return null;
        }

        static void RemoveFiles(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement file in fileList)
            {
                file.ParentNode.RemoveChild(file);
            }
        }

        // 加入若干 file 元素
        static void AddFiles(XmlDocument dom,
            List<string> outerxmls)
        {
            if (dom.DocumentElement == null)
                dom.LoadXml("<root />");
            foreach (string outerxml in outerxmls)
            {
                XmlDocumentFragment frag = dom.CreateDocumentFragment();
                frag.InnerXml = outerxml;

                dom.DocumentElement.AppendChild(frag);
            }
        }

        // 加入一个 files 元素
        static void AddFile(XmlDocument dom,
            string strFragment,
            string strNewID = null)
        {
            if (dom.DocumentElement == null)
                dom.LoadXml("<root />");
            {
                XmlDocumentFragment frag = dom.CreateDocumentFragment();
                frag.InnerXml = strFragment;

                XmlNode new_node = dom.DocumentElement.AppendChild(frag);
                if (strNewID != null)
                    (new_node as XmlElement).SetAttribute("id", strNewID);
            }
        }

        class ChangeID
        {
            public string OldID = "";
            public string NewID = "";

            public ChangeID(string strOldID, string strNewID)
            {
                this.OldID = strOldID;
                this.NewID = strNewID;
            }
        }

        // 将 source_dom 中 file 元素加入 target_dom。如果 ID 已经存在，则更换 ID
        static void AddFiles(XmlDocument source_dom,
            ref XmlDocument target_dom,
            out List<ChangeID> change_list)
        {
            change_list = new List<ChangeID>();
            List<string> source_ids = GetIdList(source_dom);
            if (source_ids.Count != 0)
            {
                List<string> writed_ids = GetIdList(target_dom);
                foreach (string id in source_ids)
                {
                    string strFragment = GetFileOuterXml(source_dom, id);
                    Debug.Assert(string.IsNullOrEmpty(strFragment) == false, "");
                    if (string.IsNullOrEmpty(strFragment) == true)
                        continue;

                    if (writed_ids.IndexOf(id) != -1)
                    {
                        string newid = GetNewID(writed_ids);
                        writed_ids.Add(newid);
                        change_list.Add(new ChangeID(id, newid));
                        AddFile(target_dom, strFragment, newid);
                    }
                    else
                        AddFile(target_dom, strFragment);
                }
            }
        }

        // 获得修改后的 id 字符串
        static string GetChangedID(List<ChangeID> change_list, string strID)
        {
            foreach (ChangeID changed in change_list)
            {
                if (changed.OldID == strID)
                    return changed.NewID;
            }

            return strID;
        }

        #endregion

        // 拷贝一条源记录到目标记录，要求对源记录有读权限，对目标记录有写权限
        // 关键点是锁的问题
        // Parameter:
        //      user                    用户对象
        //		strOriginRecordPath	    源记录路径
        //		strTargetRecordPath	    目标记录路径
        //		bDeleteOriginRecord	    是否删除源记录
        //      strMergeStyle           如何合并两条记录的 XML 部分和下属对象?
        //                              关于 XML 部分: reserve_source / reserve_target 之一。 缺省两者，则表示 reserve_source
        //                              关于下属对象部分：file_reserve_source 和 file_reserve_target 组合使用。如果两者都没有出现，表示最后的目标记录中会被去掉所有 file 元素。这是 2017/4/19 新增的参数值。以前版本都是自动合并源和目标的全部 files 元素
        //      strOutputRecordPath     返回目标记录的路径，用于目标记录是新建一条记录
        //      baOutputRecordTimestamp 返回目标记录的时间戳
        //      strChangeList           返回 id 修改的状况
        //		strError	出错信息
        // return:
        //		-1	一般性错误
        //      -4  未找到记录
        //      -5  未找到数据库
        //      -6  没有足够的权限
        //      -7  路径不合法
        //		0	成功
        public int API_CopyRecord(User user,
            string strOriginRecordPath,
            string strTargetRecordPath,
            bool bDeleteOriginRecord,
            string strMergeStyle,
            out string strIdChangeList,
            out string strTargetRecordOutputPath,
            out byte[] baOutputRecordTimestamp,
            out string strError)
        {
            Debug.Assert(user != null, "CopyRecord()调用错误，user对象不能为null。");

            // this.WriteErrorLog("走到CopyRecord(),strOriginRecordPath='" + strOriginRecordPath + "' strTargetRecordPath='" + strTargetRecordPath + "'");
            strIdChangeList = "";
            strTargetRecordOutputPath = "";
            baOutputRecordTimestamp = null;
            strError = "";

            if (String.IsNullOrEmpty(strOriginRecordPath) == true)
            {
                strError = "CopyRecord() 调用错误，strOriginRecordPath 参数值不能空";
                return -1;
            }
            if (String.IsNullOrEmpty(strTargetRecordPath) == true)
            {
                strError = "CopyRecord() 调用错误，strTargetRecordPath 参数值不能为空";
                return -1;
            }

            bool bSimulate = StringUtil.IsInList("simulate", strMergeStyle);

            // 检查目标路径，必须是记录路径形态，而不能是其他例如配置文件资源的形态
            bool bRecordPath = IsRecordPath(strTargetRecordPath);
            if (bRecordPath == false)
            {
                strError = "复制操作被拒绝，因为目标记录路径 '" + strTargetRecordPath + "' 不合法(必须是记录路径形态)";
                return -1;
            }

            long nRet = 0;

            // 得到源记录的xml
            string strOriginRecordStyle = "data,metadata,timestamp";
            byte[] baOriginRecordData = null;
            string strOriginRecordMetadata = "";
            string strOriginRecordOutputPath = "";
            byte[] baOriginRecordOutputTimestamp = null;

            int nAdditionError = 0;
            // return:
            //		-1	一般性错误
            //		-4	未找到路径指定的资源
            //		-5	未找到数据库
            //		-6	没有足够的权限
            //		-7	路径不合法
            //		-10	未找到记录xpath对应的节点  // 此次调用不可能出现这种情况
            //		>= 0	成功，返回最大长度
            nRet = this.API_GetRes(strOriginRecordPath,
                0,
                -1,
                strOriginRecordStyle,
                user,
                -1,
                out baOriginRecordData,
                out strOriginRecordMetadata,
                out strOriginRecordOutputPath,
                out baOriginRecordOutputTimestamp,
                out nAdditionError,
                out strError);
            if (nRet <= -1)
                return (int)nRet;

            // 读取目标记录
            // 要了解原来目标记录中是否有 <fprms:file>，如果有则需要保留

            XmlDocument existing_dom = null;    // 已经存在原记录的 XMLDOM
            List<string> existing_ids = new List<string>();
            if (IsAppendPath(strTargetRecordPath) == false)
            {
                byte[] baTempRecordData = null;
                string strTempRecordMetadata = "";
                byte[] baTargetRecordOutputTimestamp = null;

                // return:
                //		-1	一般性错误
                //		-4	未找到路径指定的资源
                //		-5	未找到数据库
                //		-6	没有足够的权限
                //		-7	路径不合法
                //		-10	未找到记录xpath对应的节点  // 此次调用不可能出现这种情况
                //		>= 0	成功，返回最大长度
                nRet = this.API_GetRes(strTargetRecordPath,
                    0,
                    -1,
                    "data,metadata,timestamp",
                    user,
                    -1,
                    out baTempRecordData,
                    out strTempRecordMetadata,
                    out strTargetRecordOutputPath,
                    out baTargetRecordOutputTimestamp,
                    out nAdditionError,
                    out strError);
                if (nRet == -4)
                {
                    // 目标记录不存在
                }
                else if (nRet <= -1)
                    return (int)nRet;
                else
                {
                    existing_dom = new XmlDocument();
                    byte[] baPreamble;
                    string strXml = DatabaseUtil.ByteArrayToString(baTempRecordData,
                        out baPreamble);
                    existing_dom.PreserveWhitespace = true; //设PreserveWhitespace为true
                    try
                    {
                        existing_dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "加载记录 '" + strTargetRecordPath + "' 到 XMLDOM 时出错，原因：" + ex.Message;
                        return -1;
                    }

                    existing_ids = GetIdList(existing_dom);
                }
            }

            XmlDocument source_dom = new XmlDocument(); // 源记录的 XMLDOM
            {
                byte[] baPreamble;
                string strXml = "";
                strXml = DatabaseUtil.ByteArrayToString(baOriginRecordData,
                     out baPreamble);
                source_dom.PreserveWhitespace = true; //设PreserveWhitespace为true
                try
                {
                    source_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "加载记录 '" + strOriginRecordPath + "' 到 XMLDOM 时出错，原因：" + ex.Message;
                    return -1;
                }
            }

            // strMergeStyle    file_reserve_source 和 file_reserve_target 组合使用。如果两者都没有出现，表示最后的目标记录中被去掉所有 file 元素

            List<ChangeID> change_list = new List<ChangeID>();  // 发生过变动的 id

            XmlDocument target_dom = null;  // 即将写入目标位置的记录
            if (StringUtil.IsInList("reserve_source", strMergeStyle) == true
                || (StringUtil.IsInList("reserve_source", strMergeStyle) == false && StringUtil.IsInList("reserve_target", strMergeStyle) == false)
                || existing_dom == null)
            {
                target_dom = new XmlDocument();
                target_dom.LoadXml(source_dom.OuterXml);  // target_dom 是来自源记录的记录内容

                if (existing_dom == null)
                {
                }
                else
                {
                    if (StringUtil.IsInList("file_reserve_source", strMergeStyle)
                        && StringUtil.IsInList("file_reserve_target", strMergeStyle))
                    {
                        // 目标记录中保留原来目标记录中的 file 元素，再合并上源记录的 file 元素
                        // 算法是，先删除全部 file 元素，重新加入 existing_dom 里面的全部 file 元素，并新加入 source_dom 中的 file 元素(id可能因为冲突而发生变化)
                        List<string> file_outerxmls = GetFileOuterXmls(existing_dom);
                        if (file_outerxmls.Count > 0)
                        {
                            RemoveFiles(target_dom);
                            AddFiles(target_dom, file_outerxmls);

                            // 将 source_dom 中 file 元素加入 target_dom。如果 ID 已经存在，则更换 ID
                            AddFiles(source_dom,
                        ref target_dom,
                        out change_list);
                        }
                    }
                    else if (StringUtil.IsInList("file_reserve_source", strMergeStyle))
                    {
                        // 目标记录中只保留源记录中的 file 元素
                        RemoveFiles(target_dom);

                        // 将 source_dom 中 file 元素加入 target_dom。如果 ID 已经存在，则更换 ID
                        AddFiles(source_dom,
                    ref target_dom,
                    out change_list);
                    }
                    else if (StringUtil.IsInList("file_reserve_target", strMergeStyle))
                    {
                        // 目标记录中只保留原来目标记录中的 file 元素

                        // 避免后面从 source 中复制对象
                        RemoveFiles(source_dom);
                    }
                    else
                    {
                        // 所有 file 元素都不要
                        RemoveFiles(target_dom);

                        // 避免后面从 source 中复制对象
                        RemoveFiles(source_dom);
                    }
                }

                // 注：至此 target_dom 中有即将被覆盖记录的全部 file 元素，和来自 source_dom 的全部 file 元素
            }
            else
            {
                Debug.Assert(existing_dom != null, "");

                Debug.Assert(StringUtil.IsInList("reserve_target", strMergeStyle) == true, "");

                target_dom = new XmlDocument();
                target_dom.LoadXml(existing_dom.OuterXml);  // target_dom 依然是目标位置的记录内容，意思就是目标位置元数据记录不会被源参数所提供的内容覆盖

                if (StringUtil.IsInList("file_reserve_source", strMergeStyle)
    && StringUtil.IsInList("file_reserve_target", strMergeStyle))
                {
                    // 目标记录中保留原来目标记录中的 file 元素，再合并上源记录的 file 元素

                    // existing_dom 里面的全部 file 元素已经存在，需要新加入 source_dom 中的 file 元素

                    // 将 source_dom 中 file 元素加入 target_dom。如果 ID 已经存在，则更换 ID
                    AddFiles(source_dom,
                ref target_dom,
                out change_list);

                    // 注：虽然此时目标位置记录基本内容不会被覆盖，但加入了来自源参数记录的 files 元素
                }
                else if (StringUtil.IsInList("file_reserve_source", strMergeStyle))
                {
                    // 目标记录中只保留源记录中的 file 元素
                    RemoveFiles(target_dom);

                    // 将 source_dom 中 file 元素加入 target_dom。如果 ID 已经存在，则更换 ID
                    AddFiles(source_dom,
                ref target_dom,
                out change_list);
                }
                else if (StringUtil.IsInList("file_reserve_target", strMergeStyle))
                {
                    // 目标记录中只保留原来目标记录中的 file 元素

                    // 避免后面从 source 中复制对象
                    RemoveFiles(source_dom);
                }
                else
                {
                    // 所有 file 元素都不要
                    RemoveFiles(target_dom);

                    // 避免后面从 source 中复制对象
                    RemoveFiles(source_dom);
                }

            }

            Debug.Assert(target_dom != null, "");

            // 写目标记录xml
#if NO
            long lTargetRecordTotalLength = baOriginRecordData.Length;
            byte[] baTargetRecordData = baOriginRecordData;
#endif
            byte[] baTargetRecordData = Encoding.UTF8.GetBytes(target_dom.OuterXml);
            long lTargetRecordTotalLength = baTargetRecordData.Length;
            string strTargetRecordRanges = "0-" + (lTargetRecordTotalLength - 1).ToString();

            string strTargetRecordMetadata = strOriginRecordMetadata;

            // TODO: 还要修改 lastmodiefied 时间
            // return:
            //		-1	出错
            //		0	成功
            nRet = DatabaseUtil.MergeMetadata(strOriginRecordMetadata,
                "",
                lTargetRecordTotalLength,
                "",
                out strTargetRecordMetadata,
                out strError);
            if (nRet == -1)
            {
                strError = "修改 metadata 时发生错误: " + strError;
                return -1;
            }

            string strTargetRecordStyle = "ignorechecktimestamp";
            if (bSimulate)
                strTargetRecordStyle += ",simulate";

            // byte[] baTargetRecordOutputTimestamp = null;
            string strTargetRecordOutputValue = "";

#if NO
            if (strTargetRecordPath == "test111/186769")
            {
                Debug.Assert(false, "");
            }
#endif

            // return:
            //		-1	一般性错误
            //		-2	时间戳不匹配    // 此处调用不可能出现这种情况
            //		-4	未找到路径指定的资源
            //		-5	未找到数据库
            //		-6	没有足够的权限
            //		-7	路径不合法
            //		-8	已经存在同名同类型的项  // 此处调用不可能出现这种情况
            //		-9	已经存在同名但不同类型的项  // 此处调用不可能出现这种情况
            //		0	成功
            nRet = this.API_WriteRes(strTargetRecordPath,
                strTargetRecordRanges,
                lTargetRecordTotalLength,
                baTargetRecordData,
                // null, //streamSource
                strTargetRecordMetadata,
                strTargetRecordStyle,
                null, //baInputTimestamp
                user,
                out strTargetRecordOutputPath,
                out baOutputRecordTimestamp,    // out baTargetRecordOutputTimestamp,
                out strTargetRecordOutputValue,
                out strError);
            if (nRet <= -1)
                return (int)nRet;

            // 处理资源


#if NO
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
#endif

            if (bSimulate == false)
            {
                // 复制对象资源
                List<string> source_ids = GetIdList(source_dom);

                foreach (string strObjectID in source_ids)
                {
                    string strOriginObjectPath = strOriginRecordPath + "/object/" + strObjectID;
                    string strTargetObjectPath = strTargetRecordOutputPath + "/object/" + GetChangedID(change_list, strObjectID);

                    int nStart = 0;
                    int nChunkSize = 1024 * 100;    // 100K
                    long lTotalLength = 0;

                    // 分片获取和写入资源内容
                    for (; ; )
                    {
                        // 获取源资源内容
                        byte[] baOriginObjectData = null;
                        string strOriginObjectMetadata = "";
                        string strOriginObjectOutputPath = "";
                        byte[] baOriginObjectOutputTimestamp = null;

                        // int nAdditionError = 0;
                        // return:
                        //		-1	一般性错误
                        //		-4	未找到路径指定的资源
                        //		-5	未找到数据库
                        //		-6	没有足够的权限
                        //		-7	路径不合法
                        //		-10	未找到记录xpath对应的节点
                        //		>= 0	成功，返回最大长度
                        nRet = this.API_GetRes(strOriginObjectPath,
                            nStart,
                            nChunkSize,
                            "data,metadata",
                            user,
                            -1,
                            out baOriginObjectData,
                            out strOriginObjectMetadata,
                            out strOriginObjectOutputPath,
                            out baOriginObjectOutputTimestamp,
                            out nAdditionError,
                            out strError);
                        if (nRet <= -1)
                            return (int)nRet;

                        lTotalLength = nRet;

                        // 写目标资源对象
                        long lTargetObjectTotalLength = baOriginObjectData.Length;
                        string strTargetObjectMetadata = strOriginObjectMetadata;
                        string strTargetObjectStyle = "ignorechecktimestamp";
                        string strTargetObjectOutputPath = "";
                        byte[] baTargetObjectOutputTimestamp = null;
                        string strTargetObjectOutputValue = "";

                        string strRange = nStart.ToString() + "-" + (nStart + baOriginObjectData.Length - 1).ToString();

                        if (lTotalLength == 0)
                            strRange = "";

                        // this.WriteErrorLog("走到CopyRecord(),写资源，目标路径='" + strTargetObjectPath + "'");

                        // return:
                        //		-1	一般性错误
                        //		-2	时间戳不匹配
                        //		-4	未找到路径指定的资源
                        //		-5	未找到数据库
                        //		-6	没有足够的权限
                        //		-7	路径不合法
                        //		-8	已经存在同名同类型的项
                        //		-9	已经存在同名但不同类型的项
                        //		0	成功
                        nRet = this.API_WriteRes(strTargetObjectPath,
                            strRange,
                            lTotalLength,
                            baOriginObjectData,
                            // null,
                            strTargetObjectMetadata,
                            strTargetObjectStyle,
                            null,
                            user,
                            out strTargetObjectOutputPath,
                            out baTargetObjectOutputTimestamp,
                            out strTargetObjectOutputValue,
                            out strError);
                        if (nRet <= -1)
                            return (int)nRet;

                        nStart += baOriginObjectData.Length;
                        if (nStart >= lTotalLength)
                            break;
                    }
                }
            }

            // 删除源记录
            if (bDeleteOriginRecord == true)
            {
                // return:
                //      -1	一般性错误，例如输入参数不合法等
                //      -2	时间戳不匹配    // 建议忽略时间戳，不应出现这种情况
                //      -4	未找到路径对应的资源
                //      -5	未找到数据库
                //      -6	没有足够的权限
                //      -7	路径不合法
                //      0	操作成功
                nRet = this.API_DeleteRes(strOriginRecordPath,
                    user,
                    baOriginRecordOutputTimestamp,
                    bSimulate ? "simulate" : "",
                    out baOriginRecordOutputTimestamp,
                    out strError);
                if (nRet <= -1)
                    return (int)nRet;
            }

#if NO
            // 取出目标记录的最终时间戳
            // return:
            //		-1  出错
            //		-4  未找到记录
            //      0   成功
            nRet = this.GetTimestampFromDb(
                strTargetRecordOutputPath,
                out baOutputRecordTimestamp,
                out strError);
            if (nRet <= -1)
            {
                strError = "拷贝记录完成，但获取目标记录的时间戳时出错：" + strError;
                return -1;
            }
#endif

            return 0;
        }

#if NO
        // 获取记录的时间戳
        // parameters:
        //      strRecordPath   记录路径
        //      baOutputTimestamp   out参数，返回时间戳
        //      strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        public int GetTimestampFromDb(string strRecordPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            Debug.Assert(strRecordPath != null && strRecordPath != "", "GetTimestampFromDb()调用错误，strRecordPath参数值不能为null或空字符串。");

            DbPath dbpath = new DbPath(strRecordPath);
            Database db = this.GetDatabase(dbpath.Name);
            if (db == null)
            {
                strError = "未找到名为'" + dbpath.Name + "'的数据库。";
                return -1;
            }

            // return:
            //		-1  出错
            //		-4  未找到记录
            //      0   成功
            int nRet = db.GetTimestampFromDb(dbpath.ID,
                out baOutputTimestamp,
                out strError);

            return nRet;
        }
#endif

        // 清空目录配置事项
        // parameters:
        //		strDirCfgItemPath	配置目录的路径
        //		nodeDir	            dir节点，如果为null，则根据路径来找
        //		strError        	out参数，返回出错信息
        // return:
        //		-1	出错
        //      -4  未指定路径对应的对象
        //		0	成功
        // 清空dir配置事项，包括所有下级及属性，也删除下级对应的物理文件
        public int ClearDirCfgItem(string strDirCfgItemPath,
            XmlNode nodeDir,
            out string strError)
        {
            strError = "";
            if (nodeDir == null)
            {
                if (String.IsNullOrEmpty(strDirCfgItemPath) == true)
                {
                    strError = "ClearDirCfgItem()调用错误，strDirCfgItemPath参数不能为null或者空字符串。";
                    return -1;
                }

                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count == 0)
                {
                    strError = "ClearDirCfgItem()，未找到路径为'" + strDirCfgItemPath + "'的配置事项。";
                    return -4;
                }

                if (nodes.Count > 1)
                {
                    strError = "ClearDirCfgItem()，路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，databases.xml配置文件不合法。";
                    return -1;
                }

                nodeDir = nodes[0];
            }

            // 删除定义的本地目录
            string strLocalDir = "";
            strLocalDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                nodeDir).Trim();

            string strDir = "";
            if (strLocalDir != "")
                strDir = this.DataDir + "\\" + strLocalDir + "\\";
            else
                strDir = this.DataDir + "\\";

            DirectoryInfo di = new DirectoryInfo(strDir);

            // 删除所有的下级目录
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo childDir in dirs)
            {
                Directory.Delete(childDir.FullName, true);
            }

            // 删除所有的下级文件
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo childFile in files)
            {
                File.Delete(childFile.FullName);
            }

            // 移出内存对象
            nodeDir.RemoveAll();

            this.Changed = true;

            return 0;
        }


        // 给内存对象新设一个配置事项
        // parameters:
        //		strParentPath	父亲路径 如果为null或空字符串，则直接在objects下级新建
        //		strName	自己的名称，不能为null或空字符串
        //		bDir	是否是路径
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int SetFileCfgItem(
            bool bNeedLock,
            string strParentPath,
            XmlNode nodeParent,
            string strName,
            out string strError)
        {
            strError = "";

            if (bNeedLock == true)
            {
                //**********对数据库集合加写锁**************
                this.m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("SetCfgItem()，对数据集合加写锁。");
#endif
            }

            try
            {
                if (String.IsNullOrEmpty(strName) == true)
                {
                    strError = "SetCfgItem()调用错误，strName参数值不能为null或空字符串。";
                    return -1;
                }

                if (nodeParent == null)
                {
                    if (strParentPath == "" || strParentPath == null)
                    {
                        nodeParent = this.NodeDbs;
                    }
                    else
                    {
                        List<XmlNode> parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                            strParentPath);
                        if (parentNodes.Count > 1)
                        {
                            strError = "在<objects>下级路径为'" + strParentPath + "'配置事项有'" + Convert.ToString(parentNodes.Count) + "'个，配置文件不合法。。";
                            return -1;
                        }
                        if (parentNodes.Count == 0)
                        {
                            strError = "在<objects>下级未找到路径为'" + strParentPath + "'配置事项。";
                            return -1;
                        }

                        nodeParent = parentNodes[0];
                    }
                }

                string strCfgItemOuterXml = "";
                string strLocalName = strName + ".xml";
                strCfgItemOuterXml = "<file name='" + strName + "' localname='" + strLocalName + "'/>";

                nodeParent.InnerXml = nodeParent.InnerXml + strCfgItemOuterXml;

                this.Changed = true;

                return 0;
            }
            finally
            {
                if (bNeedLock == true)
                {

                    //***********对数据库集合解写锁***************
                    this.m_container_lock.ReleaseWriterLock();
#if DEBUG_LOCK
                    this.WriteDebugInfo("SetCfgItem()，对数据库集合解写锁。");
#endif
                }
            }
        }


        // 自动创建目录配置事项
        // parameters:
        //		strParentPath	父亲路径 如果为null或空字符串，则直接在objects下级新建
        //		strName	自己的名称，不能为null或空字符串
        //		bDir	是否是路径
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int AutoCreateDirCfgItem(
            bool bNeedLock,
            string strDirCfgItemPath,
            out string strError)
        {
            strError = "";

            if (bNeedLock == true)
            {
                //**********对数据库集合加写锁**************
                this.m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("AutoCreateDirCfgItem()，对数据库集合加写锁。");
#endif
            }
            try
            {
                if (String.IsNullOrEmpty(strDirCfgItemPath) == true)
                {
                    strError = "AutoCreateDirCfgItem()调用错误，strDirCfgItemPath参数值不能为null或空字符串。";
                    return -1;
                }

                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count > 1)
                {
                    strError = "路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，服务器配置文件不合法。";
                    return -1;
                }
                if (nodes.Count == 1)
                {
                    strError = "AutoCreateDirCfgItem()调用错误，已存在路径为'" + strDirCfgItemPath + "'的配置目录。";
                    return -1;
                }

                XmlDocument dom = this.NodeDbs.OwnerDocument;
                if (dom == null)
                {
                    strError = "AutoCreateDirCfgItem()里不可能找不到dom。";
                    return -1;
                }

                //把strpath用'/'分开
                string[] paths = strDirCfgItemPath.Split(new char[] { '/' });
                if (paths.Length == 0)
                {
                    strError = "AutoCreateDirCfgItem()里paths长度不可能为0。";
                    return -1;
                }

                int i = 0;
                if (paths[0] == "")
                    i = 1;
                XmlNode nodeCurrent = this.NodeDbs;
                XmlNode temp = null;
                for (; i < paths.Length; i++)
                {
                    string strDirName = paths[i];

                    if (nodeCurrent == this.NodeDbs)
                    {
                        //XmlNode temp = null;
                        foreach (XmlNode tempChild in nodeCurrent.ChildNodes)
                        {
                            if (tempChild.Name == "database")
                            {
                                string strAllCaption = DatabaseUtil.GetAllCaption(tempChild);
                                if (StringUtil.IsInList(strDirName, strAllCaption, true) == true)
                                {
                                    temp = tempChild;
                                    break;
                                }
                            }
                            else
                            {
                                string strTempName = DomUtil.GetAttr(tempChild, "name");
                                if (String.Compare(strTempName, strDirName, true) == 0)
                                {
                                    temp = tempChild;
                                    break;
                                }
                            }
                        }

                        if (temp == null)
                        {
                            temp = dom.CreateElement("dir");
                            DomUtil.SetAttr(temp, "name", strDirName);
                            DomUtil.SetAttr(temp, "localdir", strDirName);
                            nodeCurrent.AppendChild(temp);
                        }

                        nodeCurrent = temp;
                    }
                    else
                    {
                        string strTempXpath = "dir[@name='" + strDirName + "']";
                        temp = nodeCurrent.SelectSingleNode(strTempXpath);
                        if (temp == null)
                        {
                            temp = dom.CreateElement("dir");
                            DomUtil.SetAttr(temp, "name", strDirName);
                            DomUtil.SetAttr(temp, "localdir", strDirName);
                            nodeCurrent.AppendChild(temp);
                        }
                        nodeCurrent = temp;
                    }
                }

                nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count > 1)
                {
                    strError = "经过自动创建，路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，绝对不可能的情况。";
                    return -1;
                }
                if (nodes.Count == 0)
                {
                    strError = "AutoCreateDirCfgItem()已自动创建'" + strDirCfgItemPath + "'配置目录内存对象完毕，不可能还是不存在。";
                    return -1;
                }
                XmlNode node = nodes[0];

                string strDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                    node);
                strDir = this.DataDir + "\\" + strDir;
                PathUtil.TryCreateDir(strDir);

                this.Changed = true;

                return 0;
            }
            finally
            {
                if (bNeedLock == true)
                {

                    //***************对数据库集合解写锁************
                    this.m_container_lock.ReleaseWriterLock();
#if DEBUG_LOCK
                    this.WriteDebugInfo("AutoCreateDirCfgItem()，对数据库集合解写锁。");
#endif
                }
            }
        }

        // int m_testCount = 0;

        // 写入若干 XML 记录
        public int API_WriteRecords(
            // SessionInfo sessioninfo,
            User user,
            RecordBody[] inputs,
            string strStyle,
            out List<RecordBody> results,
            out string strError)
        {
            strError = "";
            results = new List<RecordBody>();

            int nRet = 0;
            // bool bIfNotExist = StringUtil.IsInList("ifnotexist", strStyle);

            if (StringUtil.IsInList("flushkeys", strStyle) == true)
            {
                //**********对库集合加写锁****************
                // flushkeys操作是互相排斥的，不能并发进行
                m_container_lock.AcquireWriterLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("API_WriteRecords()，对库集合加写锁。");
#endif
                try
                {
                    foreach (Database db in this)
                    {
                        long lRet = db.BulkCopy(// sessioninfo,
                            "",
                            out strError);
                        if (lRet == -1)
                            return -1;
                    }
                    if (inputs == null || inputs.Length == 0)
                        return 0;
                }
                finally
                {
                    //**********对库集合解写锁****************
                    m_container_lock.ReleaseWriterLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("API_WriteRecords()，对库集合解写锁。");
#endif
                }
            }

            if (user == null)
            {
                strError = "API_WriteRecords()调用错误，user对象不能为null";
                return -1;
            }

            //**********对库集合加读锁****************
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("API_WriteRecords()，对库集合加读锁。");
#endif
            try
            {
                // 把要写入的事项按数据库分开成若干个数组
                // Hashtable database_table = new Hashtable(); // 数据库对象 --> List<RecordBody>
                Dictionary<Database, List<RecordBody>> database_table = new Dictionary<Database, List<RecordBody>>();

                foreach (RecordBody record in inputs)
                {
                    record.Result = new Result();

                    // 检查路径是否为空
                    if (String.IsNullOrEmpty(record.Path) == true)
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "Path 不能为空";
                        record.Result.ErrorCode = ErrorCodeValue.PathError; // -7;
                        continue;
                    }

                    // 检查路径类型
                    bool bRecordPath = IsRecordPath(record.Path);
                    if (bRecordPath == false)
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "Path 目前只允许使用数据库记录类型的路径";
                        record.Result.ErrorCode = ErrorCodeValue.CommonError;
                        continue;
                    }

                    // 解析路径的数据库名部分

                    string strPath = record.Path;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第1层*************
                    // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
                    if (strPath == "")
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "资源路径 '" + record.Path + "' 不合法，未指定库的下级";
                        record.Result.ErrorCode = ErrorCodeValue.PathError; // -7;
                        continue;
                    }

                    // 找到数据库对象
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "名为 '" + strDbName + "' 的数据库不存在。";
                        record.Result.ErrorCode = ErrorCodeValue.NotFoundDb; // -5;
                        continue;
                    }

                    List<RecordBody> records = null;
                    if (database_table.ContainsKey(db) == true)
                        records = (List<RecordBody>)database_table[db];
                    if (records == null)
                    {
                        records = new List<RecordBody>();
                        database_table[db] = records;
                    }

                    records.Add(record);
                }

                // 对每个数据库进行一次批写入
                bool bError = false;
                List<RecordBody> temp_results = new List<RecordBody>();
                foreach (Database db in database_table.Keys)
                {
                    List<RecordBody> records = database_table[db];
                    List<RecordBody> outputs = null;
                    nRet = db.WriteRecords(
                        // sessioninfo,
                        user,
                        records,
                        strStyle,
                        out outputs,
                        out strError);
                    if (outputs != null)
                        temp_results.AddRange(outputs); // outputs 中的元素顺序相对于 records 中可能已经打乱，并且 outputs 中元素个数可能偏少，有些没有被处理
                    if (nRet == -1)
                    {
                        bError = true;
                        // 注意此后 strError 不应被使用
                        break;
                    }
                }

                // 按照原始 inputs 中的顺序，创建返回结果集
                foreach (RecordBody record in inputs)
                {
                    if (temp_results.IndexOf(record) != -1)
                    {
                        results.Add(record);
                    }
                    else
                    {
                        record.Result = new Result();
                        record.Result.Value = -1;
                        record.Result.ErrorCode = ErrorCodeValue.CommonError;
                        record.Result.ErrorString = "没有处理";
                        record.Xml = "";
                        record.Metadata = "";
                        record.Timestamp = null;
                        results.Add(record);
                    }
                }
                // TODO: 把后面连续没有处理的元素都删除?

                if (bError == true)
                {
                    // 已经处理的 results 还可以返回
                    return -1;
                }

            }
            finally
            {
                //**********对库集合解读锁****************
                m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("API_WriteRecords()，对库集合解读锁。");
#endif
            }
            return 0;
        }

        // 写资源
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		strRanges		目标的位置,多个range用逗号分隔,null认为是空字符串，空字符串认为是0-(lTotalLength-1)
        //		lTotalLength	资源总长度,可以为0
        //		baContent		用byte[]数据传送的资源内容，如果为null则表示是0字节的数组
        //		streamContent	内容流
        //		strMetadata		元数据内容，null认为是空字符串，注:有些元数据虽然传过来，但服务器不认，比如长度
        //		strStyle		风格,null认为是空字符串
        //						ignorechecktimestamp 忽略时间戳;
        //						createdir,创建目录,路径表示待创建的目录路径
        //						autocreatedir	自动创建中间层的目录
        //						content	数据放在baContent参数里
        //						attachment	数据放在附件里
        //		baInputTimestamp	输入的时间戳,对于创建目录，不检查时间戳
        //		user	帐户对象，不能为null
        //		strOutputResPath	返回的资源路径
        //							比如追加记录时，返回实际的路径
        //							其它资源返回的路径与输入的路径相同
        //		baOutputTimestamp	返回时间戳
        //							当为目录时，返回的时间戳为null
        //		strOutputValue	返回的值，比如做累加计算时
        //		strError	出错信息
        // 说明：
        //		本函数实际代表了两种情况，新建资源，覆盖资源
        //		baContent，strAttachmentID只能使用一个，与strStyle配置使用
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		-4	未找到路径指定的资源
        //		-5	未找到数据库
        //		-6	没有足够的权限
        //		-7	路径不合法
        //		-8	已经存在同名同类型的项
        //		-9	已经存在同名但不同类型的项
        //		0	成功
        // 线：安全
        // 加锁：读锁
        public int API_WriteRes(
            string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            User user,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out string strOutputValue,
            out string strError)
        {
            baOutputTimestamp = null;
            strOutputResPath = strResPath;
            strOutputValue = "";
            strError = "";
            int nRet = 0;

            // 2006/12/18 从写锁改为读锁
            //**********对库集合加读锁****************
            m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("WriteRes()，对库集合加读锁。");
#endif
            try
            {
                //------------------------------------------------
                //检查输入参数是否合法，并规范输入参数
                //---------------------------------------------------
                if (user == null)
                {
                    strError = "WriteRes()调用错误，user 对象不能为 null";
                    return -1;
                }
                if (String.IsNullOrEmpty(strResPath) == true)
                {
                    strError = "资源路径'" + strResPath + "'不合法，不能为null或空字符串";
                    return -7;
                }

                if (lTotalLength == -1)
                {
                    if (baSource != null && baSource.Length > 0)
                    {
                        strError = "当参数 lTotalLength 为 -1 的时候，参数 baSource 的值必须为空";
                        return -1;
                    }
                }
                else
                {
                    if (lTotalLength < 0)
                    {
                        strError = "WriteRes()，lTotalLength不能为'" + Convert.ToString(lTotalLength) + "'，必须 >= 0";
                        return -1;
                    }
                }

                if (strRanges == null) //里面的函数，会处理成代表的范围
                    strRanges = "";
                if (strMetadata == null)
                    strMetadata = "";
                if (strStyle == null)
                    strStyle = "";

                /*
                if (baSource == null && streamSource == null)
                {
                    strError = "WriteRes()调用错误，baSource参数与streamSource参数不能同时为null。";
                    return -1;
                }
                if (baSource != null && streamSource != null)
                {
                    strError = "WriteRes()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                    return -1;
                }
                 * */
                if (lTotalLength != -1 && baSource == null)
                {
                    strError = "WriteRes()调用错误，baSource 参数不能为 null (当 lTotalLength 不为 -1 时)";
                    return -1;
                }

                //------------------------------------------------
                //分析出资源的类型
                //---------------------------------------------------
                if (string.IsNullOrEmpty(strResPath) == false
                    && strResPath.StartsWith(KernelServerUtil.LOCAL_PREFIX) == true)
                {
                    strError = "dp2Kernel 目前不支持修改本地文件或者目录";
                    return -6;
                }

                bool bRecordPath = IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    // 检查路径中是否有非法字符
                    if (strResPath.IndexOfAny(new char[] { '?', '*', '？', '＊' }) != -1)
                    {
                        strError = "路径 '" + strResPath + "' 格式不合法。表示目录和文件资源的路径字符串中不能包含符号 ? *";
                        return -1;
                    }

                    // 关于配置目录
                    if (StringUtil.IsInList("createdir", strStyle, true) == true)
                    {
                        // return:
                        //      -1  一般性错误
                        //		-4	未指定路径对应的对象
                        //		-6	权限不够
                        //		-8	目录已存在
                        //		-9	存在其它类型的事项
                        //		0	成功
                        nRet = this.WriteDirCfgItem(
                            false,
                            strResPath,
                            strStyle,
                            user,
                            out strError);
                    }
                    else
                    {
                        // return:
                        //      -1  一般性错误
                        //      -2  时间戳不匹配
                        //      -4  自动创建目录时，未找到上级
                        //		-6	权限不够
                        //		-9	存在其它类型的事项
                        //		0	成功
                        nRet = this.WriteFileCfgItem(
                            false,
                            strResPath,
                            strRanges,
                            lTotalLength,
                            baSource,
                            // streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            user,
                            out baOutputTimestamp,
                            out strError);
                    }

                    strOutputResPath = strResPath;

                    // 保存database.xml文件
                    if (this.Changed == true)
                        this.SaveXml();  // 外面已经加锁
                }
                else
                {
                    bool bObject = false;
                    string strRecordID = "";
                    string strObjectID = "";
                    string strXPath = "";

                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第1层*************
                    // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'路径不合法，未指定库的下级。";
                        return -7;
                    }
                    // 找到数据库对象
                    Database db = this.GetDatabase(strDbName);    // 外面已加锁
                    if (db == null)
                    {
                        strError = "名为 '" + strDbName + "' 的数据库不存在。";
                        return -5;
                    }

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含记录号层了，下级分情况判断

                    strRecordID = strFirstPart;
                    // 只到记录号层的路径
                    if (strPath == "")
                    {
                        bObject = false;
                        goto DOWRITE;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath

                    if (strFirstPart != "object"
                        && strFirstPart != "xpath")
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,第3级必须是'object'或'xpath'";
                        return -7;
                    }
                    if (strPath == "")  //object或xpath下级必须有值
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,当第3级是'object'或'xpath'，第4级必须有内容。";
                        return -7;
                    }

                    if (strFirstPart == "object")
                    {
                        strObjectID = strPath;
                        bObject = true;
                    }
                    else
                    {
                        strXPath = strPath;
                        bObject = false;
                    }

                    //------------------------------------------------
                    //开始处理资源
                    //---------------------------------------------------

                    DOWRITE:

                    // ****************************************
                    string strOutputRecordID = "";
                    nRet = db.CanonicalizeRecordID(strRecordID,
                        out strOutputRecordID,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法，原因：记录号不能为'" + strRecordID + "'";
                        return -1;
                    }


                    // ************************************
                    // 处理记录和记录里的对象
                    if (bObject == true)  //对像
                    {
                        if (strOutputRecordID == "-1")
                        {
                            strError = "资源路径 '" + strResPath + "' 不合法,原因：保存对象资源时,记录号不能为'" + strRecordID + "'。";
                            return -1;
                        }
                        strRecordID = strOutputRecordID;

                        // return:
                        //		-1  出错
                        //		-2  时间戳不匹配
                        //      -4  记录或对象资源不存在
                        //      -6  权限不够
                        //		0   成功
                        nRet = db.WriteObject(user,
                            strRecordID,
                            strObjectID,
                            strRanges,
                            lTotalLength,
                            baSource,
                            // streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            out baOutputTimestamp,
                            out strError);

                        strOutputResPath = strDbName + "/" + strRecordID + "/object/" + strObjectID;
                    }
                    else  // 记录体
                    {
                        strRecordID = strOutputRecordID;

                        string strOutputID = "";
                        // return:
                        //		-1  出错
                        //		-2  时间戳不匹配
                        //      -4  记录不存在
                        //      -6  权限不够
                        //		0   成功
                        nRet = db.WriteXml(user,
                            strRecordID,
                            strXPath,
                            strRanges,
                            lTotalLength,
                            baSource,
                            // streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            out baOutputTimestamp,
                            out strOutputID,
                            out strOutputValue,
                            true,
                            out strError);

                        strRecordID = strOutputID;

                        if (strXPath == "")
                            strOutputResPath = strDbName + "/" + strRecordID;
                        else
                            strOutputResPath = strDbName + "/" + strRecordID + "/xpath/" + strXPath;

                    }
                }

                // return nRet;
            }
            finally
            {
                //**********对库集合解写锁****************
                m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("WriteRes()，对库集合解读锁。");
#endif
            }

            if (StringUtil.IsInList("flush", strStyle) == true)
            {
                this.Commit();
            }

            return nRet;
        }

        // 写目录配置事项
        // parameters:
        //		strResPath	资源路径带库名
        //					原来是没有这个参数，为什么加上呢？
        //					是为报错时忠于原路径。如果为null或空字符串，则改为:库名路径/strCfgItemPath
        //		strStyle	风格 null认为是空字符串
        //					clear	表示清除下级
        //					autocreatedir	表示自动创建缺省的目录
        //		user	User对象，用来判断是否有权限，不能为null
        //		strCfgItemPath	配置事项路径，不带库名，不能为null或空字符串。???可以与strResPath一起用，但易乱
        //		strError	out参数，返回出错信息
        // return:
        //      -1  一般性错误
        //		-4	未指定路径对应的对象
        //		-6	权限不够
        //		-8	目录已存在
        //		-9	存在其它类型的事项
        //		0	成功
        public int WriteDirCfgItem(
            bool bNeedLock,
            string strCfgItemPath,
            string strStyle,
            User user,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strCfgItemPath) == true)
            {
                strError = "WriteDirCfgItem()调入错误，strCfgItemPath不能为null或空字符串。";
                return -1;
            }

            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count > 1)
            {
                strError = "服务器总配置文件不合法，路径为'" + strCfgItemPath + "'的配置事项对应的节点有'" + Convert.ToString(list.Count) + "'个。";
                return -1;
            }

            string strExistRights = "";
            bool bHasRight = false;

            // 已存在同名配置事项的情况
            if (list.Count == 1)
            {
                XmlNode node = list[0];
                if (node.Name == "file")
                {
                    strError = "服务器已存在路径为'" + strCfgItemPath + "'的配置文件，不能用目录覆盖文件。";
                    return -9;
                }
                if (node.Name == "database")
                {
                    strError = "服务器已存在名为'" + strCfgItemPath + "'的数据库，不能用目录覆盖数据库。";
                    return -9;
                }

                if (StringUtil.IsInList("clear", strStyle) == true)
                {
                    // 如果配置事项已存在，则检索是否有clear权限
                    string strPathForRights = strCfgItemPath;
                    bHasRight = user.HasRights(strPathForRights,
                        ResType.Directory,
                        "clear",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'清空下级(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    // 清空目录
                    // return:
                    //		-1	出错
                    //      -4  未指定路径对应的对象
                    //		0	成功
                    return this.ClearDirCfgItem(strCfgItemPath,
                        node,
                        out strError);
                }
                else
                {
                    strError = "服务器已存在路径为'" + strCfgItemPath + "'的配置目录。";
                    return -8;
                }
            }


            //***************************************

            bHasRight = user.HasRights(strCfgItemPath,
                ResType.Directory,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'清空下级(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            // return:
            //		-1	出错
            //		0	成功
            nRet = this.AutoCreateDirCfgItem(
                bNeedLock,
                strCfgItemPath,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 写文件配置事项
        // return:
        //      -1  一般性错误
        //      -2  时间戳不匹配
        //      -4  自动创建目录时，未找到上级
        //		-6	权限不够
        //		-9	存在其它类型的事项
        //		0	成功
        internal int WriteFileCfgItem(
            bool bNeedLock,
            string strCfgItemPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            User user,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            int nRet = 0;

            Debug.Assert(user != null, "WriteFileCfgItem()调用错误，user对象不能为null");

            //------------------------------------------------
            // 检查输入参数，并规范化输入参数
            //--------------------------------------------------
            if (lTotalLength <= -1)
            {
                strError = "WriteFileCfgItem()调用错误，lTotalLength值为'" + Convert.ToString(lTotalLength) + "'不合法，必须大于等于0。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";
            if (strRanges == null)
                strRanges = null;
            if (strMetadata == null)
                strMetadata = "";

            /*
            if (baSource == null && streamSource == null)
            {
                strError = "WriteFileCfgItem()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteFileCfgItem()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
             * */
            if (baSource == null)
            {
                strError = "WriteFileCfgItem()调用错误，baSource参数不能为null。";
                return -1;
            }

            if (strCfgItemPath == null || strCfgItemPath == "")
            {
                strError = "WriteFileCfgItem()调用错误，strResPath不能为null或空字符串。";
                return -1;
            }

            //------------------------------------------------
            // 开始做事情
            //--------------------------------------------------

            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count > 1)
            {
                strError = "服务器总配置文件不合法，路径为'" + strCfgItemPath + "'的配置事项对应的节点有'" + Convert.ToString(list.Count) + "'个。";
                return -1;
            }

            string strExistRights = "";
            bool bHasRight = false;


            //------------------------------------------------
            // 已存在同名配置事项的情况
            //--------------------------------------------------

            if (list.Count == 1)
            {
                XmlNode node = list[0];
                if (node.Name == "dir")
                {
                    strError = "服务器已存在路径为 '" + strCfgItemPath + "' 的配置目录，不能用文件覆盖目录。";
                    return -9;
                }
                if (node.Name == "database")
                {
                    strError = "服务器已存在名为 '" + strCfgItemPath + "' 的数据库，不能用文件覆盖数据库。";
                    return -9;
                }

                // 如果配置事项已存在，则检索是否有overwrite权限
                string strPathForRights = strCfgItemPath;
                bHasRight = user.HasRights(strPathForRights,
                    ResType.File,
                    "overwrite",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // 如果按正规的渠道创建配置文件，
                // 则内存对象中已存在，那么物理文件名一定存在，则物理文件一定存在
                string strLocalPath = "";
                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                nRet = this.GetFileCfgItemLocalPath(strCfgItemPath,
                    out strLocalPath,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet != -4)
                        return -1;
                }

                goto DOWRITE;
            }


            //------------------------------------------------
            // 不存在配置事项的情况
            //--------------------------------------------------


            string strParentCfgItemPath = ""; //父亲的路径
            string strThisCfgItemName = ""; //本配置事项的名称
            int nIndex = strCfgItemPath.LastIndexOf('/');
            if (nIndex != -1)
            {
                strParentCfgItemPath = strCfgItemPath.Substring(0, nIndex);
                strThisCfgItemName = strCfgItemPath.Substring(nIndex + 1);
            }
            else
            {
                strThisCfgItemName = strCfgItemPath;
            }

            XmlNode nodeParent = null;
            // 对上级路径进行检查
            if (strParentCfgItemPath != "")
            {
                List<XmlNode> parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strParentCfgItemPath);
                if (parentNodes.Count > 1)
                {
                    nIndex = strCfgItemPath.LastIndexOf("/");
                    string strTempParentPath = strCfgItemPath.Substring(0, nIndex);
                    strError = "服务器端路径为 '" + strTempParentPath + "' 的配置事项有'" + Convert.ToString(parentNodes.Count) + "'个，配置文件不合法。";
                    return -1;
                }

                if (parentNodes.Count == 1)
                {
                    nodeParent = parentNodes[0];
                }
                else
                {

                    if (StringUtil.IsInList("autocreatedir", strStyle, true) == false)
                    {
                        nIndex = strCfgItemPath.LastIndexOf("/");
                        string strTempParentPath = strCfgItemPath.Substring(0, nIndex);
                        strError = "未找到路径为 '" + strTempParentPath + "' 的配置事项，无法创建下级文件。";
                        return -4;
                    }

                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.AutoCreateDirCfgItem(
                        bNeedLock,
                        strParentCfgItemPath,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                        strParentCfgItemPath);
                    if (parentNodes.Count != 1)
                    {
                        strError = "WriteFileCfgItem()，自动创建好上级目录了，此时不可能找不到路径为'" + strParentCfgItemPath + "'的配置事项了。";
                        return -1;
                    }

                    nodeParent = parentNodes[0];
                }
            }
            else
            {
                nodeParent = this.NodeDbs;
            }


            // 检查上级是否有指定权限
            bHasRight = user.HasRights(strCfgItemPath,
                ResType.File,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "',对'" + strCfgItemPath + "',没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }


            // return:
            //		-1	出错
            //		0	成功
            nRet = this.SetFileCfgItem(
                bNeedLock,
                strParentCfgItemPath,
                nodeParent,
                strThisCfgItemName,
                out strError);
            if (nRet == -1)
                return -1;


            DOWRITE:

            string strFilePath = "";//GetCfgItemLacalPath(strCfgItemPath);
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            nRet = this.GetFileCfgItemLocalPath(strCfgItemPath,
                out strFilePath,
                out strError);
            if (nRet != 0)
            {
                if (nRet != -4)
                    return -1;
            }

            string strTempPath = strCfgItemPath;
            string strFirstPart = StringUtil.GetFirstPartPath(ref strTempPath);
            Database db = this.GetDatabase(strFirstPart);
            if (db != null)
            {

                // return:
                //		-1  一般性错误
                //      -2  时间戳不匹配
                //		0	成功
                return db.WriteFileForCfgItem(
                    bNeedLock,
                    strCfgItemPath,
                    strFilePath,
                     strRanges,
                     lTotalLength,
                     baSource,
                     // streamSource,
                     strMetadata,
                     strStyle,
                     baInputTimestamp,
                     out baOutputTimestamp,
                     out strError);
            }
            else
            {
                // 不从属于某一个数据库的配置文件
                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		0	成功
                return this.WriteFileForCfgItem(strFilePath,
                    strRanges,
                    lTotalLength,
                    baSource,
                    // streamSource,
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    out baOutputTimestamp,
                    out strError);
            }
        }

        // 为文件配置事项写文件
        // parameters:
        //		strFilePath 目标文件路径，不能为null或空字符串
        //		strRanges	存放区域，可以为null或""表示0-sourceBuffer.Length-1的区域
        //		nTotalLength	总长度，可以为0
        //		baSource	内容字节数组，可以为null
        //		streamSource	内容流，可以为null
        //		strMetadata	元数据信息，可以为null或""
        //		inputTimestamp	输入的时间戳，可以为null
        //		outputTimestamp	out参数，返回实际的时间戳
        //		strError	out参数，返回出错信息
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		0	成功
        // 线: 不安全
        // 说明: 这种函数的执行过程会首先检查一下本次是不是一次发来
        // 全部的内容，如果是，则直接写目标文件，不再使用临时文件
        // 如果不是才使用临时文件，并且判断ranges是否以满，再做相应的处理
        // 也有可能是新建一个文件
        internal int WriteFileForCfgItem(string strFilePath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            // --------------------------------------------------------
            // 检查输入参数，并规范化输入参数
            // --------------------------------------------------------
            if (String.IsNullOrEmpty(strFilePath) == true)
            {
                strError = "WriteFileForCfgItem()调用错误，strFilePath参数不能为空";
                return -1;
            }
            if (lTotalLength <= -1)
            {
                strError = "WriteFileForCfgItem()调用错误，lTotalLength参数的值不能为 '" + Convert.ToString(lTotalLength) + "', 必须大于等于0";
                return -1;
            }

            if (strStyle == null)
                strStyle = "";
            if (strMetadata == null)
                strMetadata = "";

            /*
            if (baSource == null && streamSource == null)
            {
                strError = "WriteFileForCfgItem()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteFileForCfgItem()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
             * */
            if (baSource == null)
            {
                strError = "WriteFileForCfgItem()调用错误，baSource参数不能为null。";
                return -1;
            }


            // --------------------------------------------------------
            // 检查输入参数，并规范化输入参数
            // --------------------------------------------------------

            string strNewFilePath = DatabaseUtil.GetNewFileName(strFilePath);

            //*************************************************
            // 检查时间戳,当有当配置文件存在时
            if (File.Exists(strFilePath) == true
                || File.Exists(strNewFilePath) == true)
            {
                if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                {
                    if (File.Exists(strNewFilePath) == true)
                        baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFilePath);
                    else
                        baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                    if (ByteArray.Compare(baOutputTimestamp, baInputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }
            }
            else
            {
                using (FileStream s = File.Create(strFilePath))
                {

                }
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
            }


            //**************************************************
            long lCurrentLength = 0;

            //if (lTotalLength == 0)
            //	goto END1;

            /*
            if (baSource != null)
             * */
            {
                if (baSource.Length == 0)
                {
                    if (strRanges != "")
                    {
                        strError = "WriteCfgFileByRange()，当baSource参数的长度为0时，strRanges的值却为'" + strRanges + "'，不匹配，应为空字符串。";
                        return -1;
                    }
                    //把写到metadata里的尺寸设好
                    FileInfo fi = new FileInfo(strFilePath);
                    lCurrentLength = fi.Length;
                    fi = null;

                    //goto END1;
                }
            }
            /*
            else
            {
                if (streamSource.Length == 0)
                {
                    if (strRanges != "")
                    {
                        strError = "WriteCfgFileByRange()，当streamSource参数长度为0时，strRanges的值却为'" + strRanges + "'，不匹配，应为空字符串。";
                        return -1;
                    }
                    //把写到metadata里的尺寸设好
                    FileInfo fi = new FileInfo(strFilePath);
                    lCurrentLength = fi.Length;
                    fi = null;

                    //goto END1;
                }
            }
             * */

            //******************************************
            // 写数据
            if (string.IsNullOrEmpty(strRanges) == true)
            {
                if (lTotalLength > 0)
                    strRanges = "0-" + Convert.ToString(lTotalLength - 1);
                else
                    strRanges = "";
            }
            string strRealRanges = strRanges;

            // 检查本次传来的范围是否是完整的文件。
            bool bIsComplete = false;
            if (lTotalLength == 0)
                bIsComplete = true;
            else
            {
                //		-1	出错 
                //		0	还有未覆盖的部分 
                //		1	本次已经完全覆盖
                int nState = RangeList.MergeContentRangeString(strRanges,
                    "",
                    lTotalLength,
                    out strRealRanges,
                    out strError);
                if (nState == -1)
                {
                    strError = "MergeContentRangeString() error 1 : " + strError + " (strRanges='" + strRanges + "' lTotalLength=" + lTotalLength.ToString() + ")";
                    return -1;
                }
                if (nState == 1)
                    bIsComplete = true;
            }


            if (bIsComplete == true)
            {
                /*
                if (baSource != null)
                 * */
                {
                    if (baSource.Length != lTotalLength)
                    {
                        strError = "范围'" + strRanges + "'与数据字节数组长度'" + Convert.ToString(baSource.Length) + "'不符合。";
                        return -1;
                    }
                }
                /*
                else
                {
                    if (streamSource.Length != lTotalLength)
                    {
                        strError = "范围'" + strRanges + "'与流长度'" + Convert.ToString(streamSource.Length) + "'不符合。";
                        return -1;
                    }
                }
                 * */
            }


            RangeList rangeList = new RangeList(strRealRanges);

            // 开始写数据
            Stream target = null;
            if (bIsComplete == true)
                target = File.Create(strFilePath);  //一次性发完，直接写到文件
            else
                target = File.Open(strNewFilePath, FileMode.OpenOrCreate);
            try
            {
                int nStartOfBuffer = 0;
                for (int i = 0; i < rangeList.Count; i++)
                {
                    RangeItem range = (RangeItem)rangeList[i];
                    // int nStartOfTarget = (int)range.lStart;
                    int nLength = (int)range.lLength;
                    if (nLength == 0)
                        continue;

                    // 移动目标流的指针到指定位置
                    target.Seek(range.lStart,   // nStartOfTarget,
                        SeekOrigin.Begin);

                    /*
                    if (baSource != null)
                     * */
                    {
                        target.Write(baSource,
                            nStartOfBuffer,
                            nLength);


                        nStartOfBuffer += nLength; //2005.11.11加
                    }
                    /*
                    else
                    {
                        StreamUtil.DumpStream(streamSource,
                            target,
                            nLength);
                    }
                     * */
                }
            }
            finally
            {
                target.Close();
            }

            string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);

            // 如果一次性写满的情况，需要做下列几件事情:
            // 1.时间戳以目标文件计算
            // 2.写到metadata的长度为目标文件总长度
            // 3.如果存在临时辅助文件，则删除这些文件。
            if (bIsComplete == true)
            {
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                lCurrentLength = lTotalLength;

                // 删除辅助文件
                if (File.Exists(strNewFilePath) == true)
                    File.Delete(strNewFilePath);
                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);

                goto END1;
            }


            //****************************************
            //处理辅助文件
            bool bFull = false;
            string strResultRange = "";
            if (strRanges == "" || strRanges == null)
            {
                bFull = true;
            }
            else
            {
                string strOldRanges = "";
                if (File.Exists(strRangeFileName) == true)
                    strOldRanges = FileUtil.File2StringE(strRangeFileName);
                int nState1 = RangeList.MergeContentRangeString(strRanges,
                    strOldRanges,
                    lTotalLength,
                    out strResultRange,
                    out strError);
                if (nState1 == -1)
                {
                    strError = "MergeContentRangeString() error 2 : " + strError + " (strRanges='" + strRanges + "' strOldRanges='" + strOldRanges + "' ) lTotalLength=" + lTotalLength.ToString() + "";
                    return -1;
                }
                if (nState1 == 1)
                    bFull = true;
            }

            // 如果文件已满，需要做下列几件事情:
            // 1.按最大长度截临时文件 
            // 2.将临时文件拷到目标文件
            // 3.删除new,range辅助文件
            // 4.时间戳以目标文件计算
            // 5.metadata的长度为目标文件的总长度
            if (bFull == true)
            {
                using (Stream s = new FileStream(strNewFilePath,
                    FileMode.OpenOrCreate))
                {

                    s.SetLength(lTotalLength);
                }

                // 用.new临时文件替换直接文件
                File.Copy(strNewFilePath,
                    strFilePath,
                    true);

                File.Delete(strNewFilePath);

                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);

                lCurrentLength = lTotalLength;
            }
            else
            {

                //如果文件未满，需要做下列几件事情：
                // 1.把目前的range写到range辅助文件
                // 2.时间戳以临时文件计算
                // 3.metadata的长度为-1，即未知的情况

                FileUtil.String2File(strResultRange,
                    strRangeFileName);

                lCurrentLength = -1;

                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFilePath);
            }

            END1:

            // 写metadata
            if (strMetadata != "")
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);

                // 取出旧的数据进行合并
                string strOldMetadata = "";
                if (File.Exists(strMetadataFileName) == true)
                    strOldMetadata = FileUtil.File2StringE(strMetadataFileName);
                if (strOldMetadata == "")
                    strOldMetadata = "<file/>";

                string strResultMetadata;
                // return:
                //		-1	出错
                //		0	成功
                int nRet = DatabaseUtil.MergeMetadata(strOldMetadata,
                    strMetadata,
                    lCurrentLength,
                    "",
                    out strResultMetadata,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把合并的新数据写到文件里
                FileUtil.String2File(strResultMetadata,
                    strMetadataFileName);
            }
            return 0;
        }

        public class PathInfo
        {
            public string OriginPath = "";

            public bool IsLocalPath = false;    // 是否为本地文件或目录路径? 2016/11/7
            public bool IsConfigFilePath = false;   // 是否为配置文件路径?

            public string DbName = "";
            public string RecordID = "";
            public string ObjectID = "";
            public string XPath = "";

            public Database Database = null;

            public bool IsObjectPath
            {
                get
                {
                    if (string.IsNullOrEmpty(this.ObjectID) == false)
                        return true;
                    return false;
                }
            }

            public string RecordID10
            {
                get
                {
                    return DbPath.GetID10(this.RecordID);
                }
            }

        }

        // 解析资源路径
        // return:
        //      -1  一般性错误
        //		-5	未找到数据库
        //		-7	路径不合法
        //      0   成功
        public int ParsePath(string strResPath,
    out PathInfo info,
    out string strError)
        {
            info = new PathInfo();
            strError = "";

            info.OriginPath = strResPath;

            if (string.IsNullOrEmpty(strResPath) == false
                && strResPath.StartsWith(KernelServerUtil.LOCAL_PREFIX) == true)
            {
                info.IsLocalPath = true;
                return 0;
            }

            bool bRecordPath = IsRecordPath(strResPath);
            if (bRecordPath == false)
            {
                info.IsConfigFilePath = true;
                return 0;
            }

            // 判断资源类型
            string strPath = strResPath;
            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            //***********吃掉第1层*************
            // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
            if (string.IsNullOrEmpty(strPath) == true)
            {
                strError = "资源路径 '" + strResPath + "' 不合法: 未指定库的下级";
                return -7;
            }

            // 根据资源类型，写资源
            info.Database = this.GetDatabase(strDbName);
            if (info.Database == null)
            {
                strError = "未找到'" + strDbName + "'库";
                return -5;
            }

            // bool bObject = false;
            string strRecordID = "";
            string strObjectID = "";
            string strXPath = "";

            string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
            //***********吃掉第2层*************
            // 到此为止，strPath记录号层了，下级分情况判断

            strRecordID = strFirstPart;
            // 只到记录号层的路径
            if (strPath == "")
            {
                // bObject = false;
                info.DbName = strDbName;
                info.RecordID = strRecordID;
                return 0;
            }

            strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
            //***********吃掉第2层*************
            // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath
            if (strFirstPart != "object"
                && strFirstPart != "xpath")
            {
                strError = "资源路径 '" + strResPath + "' 不合法,第3级必须是'object'或'xpath'";
                return -7;
            }
            if (strPath == "")  //object或xpath下级必须有值
            {
                strError = "资源路径 '" + strResPath + "' 不合法,当第3级是'object'或'xpath'，第4级必须有内容。";
                return -7;
            }

            if (strFirstPart == "object")
            {
                strObjectID = strPath;
                // bObject = true;
            }
            else
            {
                strXPath = strPath;
                // bObject = false;
            }

            if (strObjectID.IndexOf("/") != -1)
            {
                // 有可能 strObjectID 是 0/page:1 这样的形态
                strObjectID = StringUtil.GetFirstPartPath(ref strPath);

                strXPath = StringUtil.GetFirstPartPath(ref strPath);
#if NO
                if (strXPath.StartsWith("page:") == false)
                {
                    strError = "资源路径 '" + strResPath + "' 不合法,第 5 级必须是 'page:xxx' 形态";
                    return -7;
                }
#endif
            }

            info.DbName = strDbName;
            info.RecordID = strRecordID;
            info.XPath = strXPath;
            info.ObjectID = strObjectID;
            info.IsConfigFilePath = false;
            return 0;
        }


        // GetRes()用range不太好实现,因为原来当请求的长度超过允许的长度时,长度会自动为截取
        // 而如果用range来表示,则不知该截短哪部分好。
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		lStart	起始长度
        //		lLength	总长度,-1:从start到最后
        //		strStyle	取资源的风格，以逗豆间隔的字符串
        /*
        strStyle用法

        1.控制数据存放的位置
        content		把返回的数据放到字节数组参数里
        attachment	把返回的数据放到附件中,并返回附件的id

        2.控制返回的数据
        metadata	返回metadata信息
        timestamp	返回timestamp
        length		数据总长度，始终都有值
        data		返回数据体
        respath		返回记录路径,目前始终都有值
        all			返回所有值

        3.控制记录号
        prev		前一条
        prev,myself	自己或前一条
        next		下一条
        next,myself	自己或下一条
        放到strOutputResPath参数里

        */
        //		baContent	用content字节数组返回资源内容
        //		strAttachmentID	用附件返回资源内容
        //		strMetadata	返回的metadata内容
        //		strOutputResPath	返回的资源路径
        //		baTimestamp	返回的资源时间戳
        // return:
        //		-1	一般性错误
        //		-4	未找到路径指定的资源
        //		-5	未找到数据库
        //		-6	没有足够的权限
        //		-7	路径不合法
        //		-10	未找到记录xpath对应的节点
        //		>= 0	成功，返回最大长度
        //      nAdditionError -50 有一个以上下级资源记录不存在
        // 线：安全
        public long API_GetRes(string strResPath,
            long lStart,
            int nLength,
            string strStyle,
            User user,
            int nMaxLength,
            out byte[] baData,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out int nAdditionError, // 附加的错误码
            out string strError)
        {
            baData = null;
            strMetadata = "";
            strOutputResPath = "";
            baOutputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            //------------------------------------------------
            //检查输入参数是否合法，并规范输入参数
            //---------------------------------------------------

            Debug.Assert(user != null, "GetRes()调用错误，user对象不能为null。");

            if (user == null)
            {
                strError = "GetRes()调用错误，user对象不能为null。";
                return -1;
            }
            if (String.IsNullOrEmpty(strResPath) == true)
            {
                strError = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                return -7;
            }
            if (lStart < 0)
            {
                strError = "GetRes()调用错误，lStart不能小于0。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";

            //------------------------------------------------
            // 开始做事情
            //---------------------------------------------------

            //******************加库集合加读锁******
            this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);

#if DEBUG_LOCK
			this.WriteDebugInfo("GetRes()，对库集合加读锁。");
#endif
            try
            {
                long lRet = 0;

                PathInfo info = null;
                // 解析资源路径
                // return:
                //      -1  一般性错误
                //		-5	未找到数据库
                //		-7	路径不合法
                //      0   成功
                int nRet = ParsePath(strResPath,
    out info,
    out strError);
                if (nRet < 0)
                    return nRet;

                if (info.IsLocalPath == true)
                {
                    if (IsServerManager(user) == false)
                    {
                        strError = "必须是对 Server 有 management 权限的用户才能获得 " + KernelServerUtil.LOCAL_PREFIX + " 下级对象的内容";
                        return -6;
                    }
                    string strPhysicalPath = Path.Combine(this.DataDir, strResPath.Substring(KernelServerUtil.LOCAL_PREFIX.Length));

                    // 限制 strPhysicalPath 不要越过 this.DataDir
                    if (PathUtil.IsChildOrEqual(strPhysicalPath, this.DataDir) == false)
                    {
                        strError = "路径 '" + strResPath + "' 越过许可范围";
                        return -7;
                    }

                    lRet = GetFile(
    strPhysicalPath,
    lStart,
    nLength,
    nMaxLength,
    strStyle,
    out baData,
    // out strMetadata,
    out baOutputTimestamp,
    out strError);
                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputResPath = strResPath;
                    }
                    return lRet;
                }

                if (info.IsConfigFilePath == true)
                {
                    //当配置事项处理
                    // return:
                    //		-1  一般性错误
                    //		-4	未找到路径对应的对象
                    //		-6	没有足够的权限
                    //		>= 0    成功 返回最大长度
                    lRet = this.GetFileCfgItem(
                        false,
                        strResPath,
                        lStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        user,
                        out baData,
                        out strMetadata,
                        out baOutputTimestamp,
                        out strError);
                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputResPath = strResPath;
                    }
                    return lRet;
                }

#if NO
                bool bRecordPath = this.IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    //当配置事项处理
                    // return:
                    //		-1  一般性错误
                    //		-4	未找到路径对应的对象
                    //		-6	没有足够的权限
                    //		>= 0    成功 返回最大长度
                    lRet = this.GetFileCfgItem(
                        false,
                        strResPath,
                        lStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        user,
                        out baData,
                        out strMetadata,
                        out baOutputTimestamp,
                        out strError);


                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputResPath = strResPath;
                    }
                }
                else
                {

                    // 判断资源类型
                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第1层*************
                    // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'路径不合法，未指定库的下级。";
                        return -7;
                    }

                    // 从这里区别是数据库还是服务器端配置文件

                    // 根据资源类型，写资源
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        strError = "未找到'" + strDbName + "'库";
                        return -5;
                    }

                    bool bObject = false;
                    string strRecordID = "";
                    string strObjectID = "";
                    string strXPath = "";

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath记录号层了，下级分情况判断

                    strRecordID = strFirstPart;
                    // 只到记录号层的路径
                    if (strPath == "")
                    {
                        bObject = false;
                        goto DOGET;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath
                    if (strFirstPart != "object"
                        && strFirstPart != "xpath")
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,第3级必须是'object'或'xpath'";
                        return -7;
                    }
                    if (strPath == "")  //object或xpath下级必须有值
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,当第3级是'object'或'xpath'，第4级必须有内容。";
                        return -7;
                    }

                    if (strFirstPart == "object")
                    {
                        strObjectID = strPath;
                        bObject = true;
                    }
                    else
                    {
                        strXPath = strPath;
                        bObject = false;
                    }

#endif
                ///////////////////////////////////
                ///开始做事情
                //////////////////////////////////////////

                // DOGET:
                // 检查对数据库中记录的权限
                string strExistRights = "";
                bool bHasRight = user.HasRights(info.DbName + "/" + info.RecordID,
                    ResType.Record,
                    "read",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + info.DbName + "'库没有'读记录(read)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                if (info.IsObjectPath == true)  // 对象
                {
                    //		-1  出错
                    //		-4  记录不存在
                    //		>=0 资源总长度
                    lRet = info.Database.GetObject(info.RecordID,
                        info.ObjectID,
                        info.XPath,
                        lStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        out baData,
                        out strMetadata,
                        out baOutputTimestamp,
                        out strError);

                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        // TODO: 当获得 PDF 单页图像的时候，这里返回的路径应该比 object 要深一层
                        strOutputResPath = info.DbName + "/" + info.RecordID + "/object/" + info.ObjectID;
                    }
                }
                else
                {
                    string strOutputID;
                    // return:
                    //		-1  出错
                    //		-4  未找到记录
                    //      -10 记录局部未找到
                    //		>=0 资源总长度
                    //      nAdditionError -50 有一个以上下级资源记录不存在
                    lRet = info.Database.GetXml(info.RecordID,
                        info.XPath,
                        lStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        out baData,
                        out strMetadata,
                        out strOutputID,
                        out baOutputTimestamp,
                        true,
                        out nAdditionError,
                        out strError);
                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        // strRecordID = strOutputID;
                        if (string.IsNullOrEmpty(info.XPath) == true)
                            strOutputResPath = info.DbName + "/" + strOutputID;
                        else
                            strOutputResPath = info.DbName + "/" + strOutputID + "/xpath/" + info.XPath;
                    }
                }

                return lRet;
            }
            finally
            {
                //******************对库集合解读锁******
                this.m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("GetRes()，对库集合解读锁。");
#endif
            }
        }

        // 检查一个路径是否是数据库记录路径
        internal static bool IsRecordPath(string strResPath)
        {
            string[] paths = strResPath.Split(new char[] { '/' });
            if (paths.Length >= 2)
            {
                if (StringUtil.IsPureNumber(paths[1]) == true
                    || paths[1] == "?"
                    || paths[1] == "-1")
                {
                    return true;
                }
            }
            return false;
        }


        // 按指定范围读配置文件
        // strRoleName:  角色名,大小写均可
        // 其它参数同GetXml(),无strOutputResPath参数
        // 线: 安全的
        // return:
        //		-1  一般性错误
        //		-4	未找到路径对应的对象
        //		-6	没有足够的权限
        //		>= 0    成功 返回最大长度
        // 线：安全
        public long GetFileCfgItem(
            bool bNeedLock,
            string strCfgItemPath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            User user,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] outputTimestamp,
            out string strError)
        {
            strMetadata = "";
            destBuffer = null;
            outputTimestamp = null;
            strError = "";

            if (bNeedLock == true)
            {
                //**********对数据库集合加读锁**************
                this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
                this.WriteDebugInfo("GetCfgFile()，对数据库集合加读锁。");
#endif
            }

            try
            {
                // 检查当前帐户对配置事项的权限，暂时不报权限的错，检查完对象是否存在，再报错
                string strExistRights = "";
                bool bHasRight = user.HasRights(strCfgItemPath,
                    ResType.File,
                    "read",
                    out strExistRights);

                string strFilePath = "";//this.GetCfgItemLacalPath(strCfgItemPath);
                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                int nRet = this.GetFileCfgItemLocalPath(strCfgItemPath,
                    out strFilePath,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet == -2)
                        return -4;
                    return -1;
                }

                // 此时再报权限的错
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'读(read)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1      出错
                //		>= 0	成功，返回最大长度
                return DatabaseCollection.GetFileForCfgItem(strFilePath,
                    lStart,
                    nLength,
                    nMaxLength,
                    strStyle,
                    out destBuffer,
                    out strMetadata,
                    out outputTimestamp,
                    out strError);
            }
            catch (PathErrorException ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                if (bNeedLock == true)
                {
                    //****************对数据库集合解读锁**************
                    this.m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
                    this.WriteDebugInfo("GetCfgFile()，对数据库集合解读锁。");
#endif
                }
            }
        }

        // 为GetCfgItem服务器的内部函数
        // return:
        //		-1      出错
        //		>= 0	成功，返回最大长度
        public static long GetFileForCfgItem(string strFilePath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] outputTimestamp,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            outputTimestamp = null;
            strError = "";

            long lTotalLength = 0;
            FileInfo file = new FileInfo(strFilePath);
            if (file.Exists == false)
            {
                strError = "服务器不存在物理路径为'" + strFilePath + "'的文件。";
                return -1;
            }

            // 1.取时间戳
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);
                if (File.Exists(strNewFileName) == true)
                {
                    outputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFileName);
                }
                else
                {
                    outputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                }
            }

            // 2.取元数据
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == true)
                {
                    strMetadata = FileUtil.File2StringE(strMetadataFileName);
                }
            }

            // 3.取range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == true)
                {
                    string strRange = FileUtil.File2StringE(strRangeFileName);
                }
            }

            // 4.长度
            lTotalLength = file.Length;

            // 5.有data风格时,才会取数据
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (nLength == 0)  // 取0长度
                {
                    destBuffer = new byte[0];
                    return lTotalLength;
                }
                // 检查范围是否合法
                long lOutputLength;
                // return:
                //		-1  出错
                //		0   成功
                int nRet = ConvertUtil.GetRealLength(lStart,
                    nLength,
                    lTotalLength,
                    nMaxLength,
                    out lOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                using (FileStream s = new FileStream(strFilePath,
                    FileMode.Open))
                {
                    destBuffer = new byte[lOutputLength];
                    s.Seek(lStart, SeekOrigin.Begin);
                    s.Read(destBuffer,
                        0,
                        (int)lOutputLength);
                }
            }
            return lTotalLength;
        }

        // 得到一个文件配置事项的本地文件绝对路径
        // parameters:
        //		strFileCfgItemPath	文件配置事项的路径，格式为'dir1/dir2/file'
        //		strLocalPath	out参数，返回对应的本地文件绝对路径	
        //		strError	out参数，返回出错信息
        // return:
        //		-1	一般性错误，比如调用错误，参数不合法等
        //		-2	没找到节点
        //		-3	localname属性未定义或为值空
        //		-4	localname在本地不存在
        //		-5	存在多个节点
        //		0	成功
        // 线：不安全
        public int GetFileCfgItemLocalPath(string strFileCfgItemPath,
            out string strLocalPath,
            out string strError)
        {
            strLocalPath = "";
            strError = "";

            if (strFileCfgItemPath == ""
                || strFileCfgItemPath == null)
            {
                strError = "GetCfgItemLacalPath()的strPath参数值不能为null或空字符串";
                return -1;
            }
            List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                strFileCfgItemPath);
            if (nodes.Count == 0)
            {
                strError = "dp2Kernel 服务器上未定义路径为 '" + strFileCfgItemPath + "' 的配置文件";
                return -2;
            }
            if (nodes.Count > 1)
            {
                strError = "dp2Kernel 服务器上路径为 '" + strFileCfgItemPath + "' 的配置事项有 " + Convert.ToString(nodes.Count) + " 个，配置文件不合法";
                return -5;
            }

            XmlNode nodeFile = nodes[0];

            string strPureFileName = DomUtil.GetAttr(nodeFile, "localname");
            if (strPureFileName == "")
            {
                strError = "dp2Kernel 服务器上路径为 '" + strFileCfgItemPath + "' 的文件配置事项未定义对应的物理文件";
                return -3;
            }

            string strLocalDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                nodeFile.ParentNode);

            string strRealPath = "";
            if (strLocalDir == "")
                strRealPath = this.DataDir + "\\" + strPureFileName;
            else
                strRealPath = this.DataDir + "\\" + strLocalDir + "\\" + strPureFileName;

            strLocalPath = strRealPath;
            if (File.Exists(strRealPath) == false)
            {
                strError = "dp2Kernel 服务器上路径为 '" + strFileCfgItemPath + "' 的文件配置事项对应的物理文件在本地不存在";
                return -4;
            }
            return 0;
        }


        // 删除资源，可以是记录 或 配置事项，不支持对象资源或部分记录体
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录
        //						配置事项: 库名/配置事项路径
        //						记录: 库名/记录号
        //		user	当前帐户对象，不能为null
        //		baInputTimestamp	输入的时间戳
        //		baOutputTimestamp	out参数，返回时间戳
        //		strError	out参数，返回出错信息
        // return:
        //      -1	一般性错误，例如输入参数不合法等
        //      -2	时间戳不匹配
        //      -4	未找到路径对应的资源
        //      -5	未找到数据库
        //      -6	没有足够的权限
        //      -7	路径不合法
        //      0	操作成功
        // 说明: 
        // 1)删除需要当前帐户对将被删除的记录的有delete权限		
        // 2)删除记录的明确含义是删除记录体，并且删除该记录包含的所有对象资源
        // 3)删除配置目录不要求时间戳,同时baOutputTimestamp也是null
        // 锁：要加读锁
        public int API_DeleteRes(string strResPath,
            User user,
            byte[] baInputTimestamp,
            string strStyle,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            //-----------------------------------------
            //对输入参数做例行检查
            //---------------------------------------
            if (strResPath == null || strResPath == "")
            {
                strError = "DeleteRes()调用错误，strResPath参数不能为null或空字符串。";
                return -1;
            }
            if (user == null)
            {
                strError = "DeleteRes()调用错误，user参数不能为null。";
                return -1;
            }

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            //---------------------------------------
            //开始做事情 
            //---------------------------------------

            //******************加库集合加读锁******
            this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);

#if DEBUG_LOCK
            this.WriteDebugInfo("API_DeleteRes()，对库集合加读锁。");
#endif
            try
            {
                int nRet = 0;

                PathInfo info = null;
                // 解析资源路径
                // return:
                //      -1  一般性错误
                //		-5	未找到数据库
                //		-7	路径不合法
                //      0   成功
                nRet = ParsePath(strResPath,
    out info,
    out strError);
                if (nRet < 0)
                    return nRet;

                //bool bRecordPath = IsRecordPath(strResPath);
                //if (bRecordPath == false)
                if (info.IsConfigFilePath)
                {
                    // 也可能是数据库对象

                    if (bSimulate == false)
                    {
                        // 删除实际的物理文件
                        //      -1  一般性错误
                        //      -2  时间戳不匹配
                        //      -4  未找到路径对应的资源
                        //      -6  没有足够的权限
                        //      0   成功
                        nRet = this.DeleteCfgItem(user,
                            strResPath,
                            baInputTimestamp,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet <= -1)
                            return nRet;
                    }

                    goto CHECK_CHANGED;
                }
                else
                {
#if NO
                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'不合法，未指定库的下级。";
                        return -7;
                    }

                    // 根据资源类型，写资源
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        strError = "没找到名为'" + strDbName + "'的数据库。";
                        return -5;
                    }

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含cfgs或记录号层了，下级分情况判断
                    // strFirstPart可能是为cfg或记录号

                    string strRecordID = strFirstPart;
#endif
                    string strRecordID = info.RecordID;
                    Database db = info.Database;
                    string strDbName = info.DbName;

                    // 检查当前帐户是否有删除记录
                    bool bHasRight = user.HasRights(strResPath,//db.GetCaption("zh-CN"),
                        ResType.Record,
                        "delete",
                        out string strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'删除记录(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    if (bSimulate == false)
                    {
                        // return:
                        //		-1  一般性错误
                        //		-2  时间戳不匹配
                        //      -4  未找到记录
                        //		0   成功
                        nRet = db.DeleteRecord(strRecordID,
                            info.ObjectID,
                            baInputTimestamp,
                            strStyle,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet <= -1)
                            return nRet;
                    }

                    return 0;
                }
            }
            finally
            {
                m_container_lock.ReleaseReaderLock();
                //*************对库集合解读锁***********
#if DEBUG_LOCK
                this.WriteDebugInfo("API_DeleteRes()，对库集合解读锁。");
#endif
            }

            CHECK_CHANGED:
            //及时保存database.xml // 是用加锁的函数吗？
            if (this.Changed == true)
                this.SaveXmlSafety(true);

            return 0;
        }

        // 重建记录的keys
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						记录: 库名/记录号
        //		user	当前帐户对象，不能为null
        //		strError	out参数，返回出错信息
        // return:
        //      -1	一般性错误，例如输入参数不合法等
        //      -2	时间戳不匹配
        //      -4	未找到路径对应的资源
        //      -5	未找到数据库
        //      -6	没有足够的权限
        //      -7	路径不合法
        //      0	操作成功
        // 说明: 
        // 1)删除需要当前帐户对将被删除的记录的有overwrite权限		
        // 锁：要加读锁
        public int API_RebuildResKeys(string strResPath,
            User user,
            string strStyle,
            out string strOutputResPath,
            out string strError)
        {
            strError = "";
            strOutputResPath = "";

            //-----------------------------------------
            //对输入参数做例行检查
            //---------------------------------------
            if (String.IsNullOrEmpty(strResPath) == true)
            {
                strError = "RebuildResKeys()调用错误，strResPath参数不能为null或空字符串。";
                return -1;
            }

            if (user == null)
            {
                strError = "RebuildResKeys()调用错误，user参数不能为null。";
                return -1;
            }

            if (strStyle == null)
                strStyle = "";

            //-----------------------------------------
            //开始做事情 
            //---------------------------------------

            //******************加库集合加读锁******
            this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);

#if DEBUG_LOCK
            this.WriteDebugInfo("API_RebuildResKeys()，对库集合加读锁。");
#endif
            try
            {
                int nRet = 0;

                bool bRecordPath = IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    strError = "不支持对 '" + strResPath + "' 对象的重建keys操作";
                    return -1;
                    // 也可能是数据库对象
                }

                {

                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'不合法，未指定库的下级。";
                        return -7;
                    }

                    // 根据资源类型，写资源
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        strError = "没找到名为'" + strDbName + "'的数据库。";
                        return -5;
                    }

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含cfgs或记录号层了，下级分情况判断
                    // strFirstPart可能是为cfg或记录号

                    string strRecordID = strFirstPart;

                    // 检查当前帐户是否有删除记录
                    string strExistRights = "";
                    bool bHasRight = user.HasRights(strResPath,//db.GetCaption("zh-CN"),
                        ResType.Record,
                        "overwrite",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'改写记录(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    string strOutputID = "";
                    // return:
                    //		-1  一般性错误
                    //		-2  时间戳不匹配
                    //      -4  未找到记录
                    //		0   成功
                    nRet = db.RebuildRecordKeys(strRecordID,
                        strStyle,
                        out strOutputID,
                        out strError);

                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputResPath = strDbName + "/" + strOutputID;
                    }

                    if (nRet <= -1)
                        return nRet;
                }
            }
            finally
            {
                m_container_lock.ReleaseReaderLock();
                //*************对库集合解读锁***********
#if DEBUG_LOCK
                this.WriteDebugInfo("API_RebuildResKeys()，对库集合解读锁。");
#endif
            }

            /*
            //及时保存database.xml // 是用加锁的函数吗？
            if (this.Changed == true)
                this.SaveXmlSafety(true);
             * */

            return 0;
        }

        // 删除一个配置事项，可以是目录，也可以是文件
        // return:
        //      -1  一般性错误
        //      -2  时间戳不匹配
        //      -4  未找到路径对应的资源
        //      -6  没有足够的权限
        //      0   成功
        public int DeleteCfgItem(User user,
            string strCfgItemPath,
            byte[] intputTimestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            if (strCfgItemPath == null
                || strCfgItemPath == "")
            {
                strError = "DeleteCfgItem()调用错误，strCfgItemPath参数值不能为null或空字符串。";
                return -1;
            }

            List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (nodes.Count == 0)
            {
                strError = "服务器不存在路径为'" + strCfgItemPath + "'的配置事项。";
                return -4;
            }
            if (nodes.Count != 1)
            {
                strError = "dp2Kernel 服务器上路径为 '" + strCfgItemPath + "' 的配置事项个数为 '" + Convert.ToString(nodes.Count) + "'，database.xml 配置文件异常。";
                return -1;
            }


            string strExistRights = "";
            bool bHasRight = false;

            XmlNode node = nodes[0];

            if (node.Name == "dir")
            {
                // 检查当前帐户是否有删除记录'
                bHasRight = user.HasRights(strCfgItemPath,
                    ResType.Directory,
                    "delete",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strCfgItemPath + "'配置事项没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }
                string strDir = DatabaseUtil.GetLocalDir(this.NodeDbs, node).Trim();
                Directory.Delete(this.DataDir + "\\" + strDir, true);
                node.ParentNode.RemoveChild(node);
                return 0;
            }
            else if (String.Compare(node.Name, "database", true) == 0)
            {

            }


            // 检查当前帐户是否有删除记录'
            bHasRight = user.HasRights(strCfgItemPath,
                ResType.File,
                "delete",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对'" + strCfgItemPath + "'配置事项没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            string strFilePath = "";//GetCfgItemLacalPath(strCfgItemPath);
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            int nRet = this.GetFileCfgItemLocalPath(strCfgItemPath,
                out strFilePath,
                out strError);
            if (nRet != 0)
            {
                if (nRet == -1 || nRet == -5)
                    return -1;

            }
            if (strFilePath != "")
            {
                string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);

                if (File.Exists(strFilePath) == true)
                {

                    byte[] oldTimestamp = null;
                    if (File.Exists(strNewFileName) == true)
                        oldTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFileName);
                    else
                        oldTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);

                    outputTimestamp = oldTimestamp;
                    if (ByteArray.Compare(oldTimestamp, intputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }

                File.Delete(strNewFileName);
                File.Delete(strFilePath);

                string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == false)
                    File.Delete(strRangeFileName);

                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == false)
                    File.Delete(strMetadataFileName);
            }
            node.ParentNode.RemoveChild(node);

            this.Changed = true;
            this.SaveXml();

            return 0;
        }



        // 根据服务器上的指定路径列出其下级的事项
        // parameters:
        //		strPath	路径,不带服务器部分，
        //				格式为: "数据库名/下级名/下级名",
        //				当为null或者为""时，表示列出该服务器下所有的数据库
        //		lStart	起始位置,从0开始 ,不能小于0
        //		lLength	长度 -1表示从lStart到最后
        //		strLang	语言版本 用标准字母表示法，如zh-CN
        //      strStyle    是否要列出所有语言的名字? "alllang"表示要列出全部语言
        //		items	 out参数，返回下级事项数组
        // return:
        //		-1  出错
        //      -4  strResPath 对应的对象没有找到
        //      -6  权限不够
        //		0   正常
        // 说明	只有当前帐户对事项有"list"权限时，才能列出来。
        //		如果列本服务器的数据库时，对所有的数据库都没有list权限，都按错误处理，与没有数据库事项区分开。
        public int API_Dir(string strResPath,
            long lStart,
            long lLength,
            long lMaxLength,
            string strLang,
            string strStyle,
            User user,
            out ResInfoItem[] items,
            out int nTotalLength,
            out string strError)
        {
            items = new ResInfoItem[0];
            nTotalLength = 0;

            List<ResInfoItem> aItem = new List<ResInfoItem>();
            strError = "";
            int nRet = 0;
            //******************加库集合加读锁******
            this.m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);

#if DEBUG_LOCK
			this.WriteDebugInfo("Dir()，对库集合加读锁。");
#endif
            try
            {
                if (string.IsNullOrEmpty(strResPath))
                {
                    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    // 1.取服务器下的数据库

                    nRet = this.GetDirableChildren(user,
                        strLang,
                        strStyle,
                        out aItem,
                        out strError);
                    if (this.Count > 0 && aItem.Count == 0)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对所有的数据库都没有'显示(list)'权限。";
                        return -6;
                    }
                }
                else
                {
                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);

                    // 可以是数据库也可以是配置事项
                    if (strPath == "")
                    {
                        Database db = this.GetDatabase(strDbName);
                        if (db != null)
                        {
                            // return:
                            //		-1	出错
                            //		0	成功
                            nRet = db.GetDirableChildren(user,
                                strLang,
                                strStyle,
                                out aItem,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            goto END1;
                        }
                    }

                    if (strResPath.StartsWith(KernelServerUtil.LOCAL_PREFIX))
                    {
                        if (IsServerManager(user) == false)
                        {
                            strError = "必须是对 Server 有 management 权限的用户才能列出 " + KernelServerUtil.LOCAL_PREFIX + " 下级对象";
                            return -6;
                        }
                        string strPhysicalPath = Path.Combine(this.DataDir, strResPath.Substring(KernelServerUtil.LOCAL_PREFIX.Length));

                        // 限制 strPhysicalPath 不要越过 this.DataDir
                        if (PathUtil.IsChildOrEqual(strPhysicalPath, this.DataDir) == false)
                        {
                            strError = "路径 '" + strResPath + "' 越过许可范围";
                            return -7;
                        }

                        // return:
                        //      -1  出错
                        //      其他  列出的事项总数。注意，不是 lLength 所指出的本次返回数
                        nRet = ListFile(
                            this.DataDir,   // TODO: 可以根据账户权限不同控制起点的不同
                            strPhysicalPath,
                            "",
                            lStart,
                            lLength,
                            out aItem,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    {
                        // return:
                        //		-1	出错
                        //      -4  strCfgItemPath 对应的对象没有找到
                        //		0	成功
                        nRet = this.DirCfgItem(user,
                            strResPath,
                            out aItem,
                            out strError);
                        //if (nRet == -1)
                        //    return -1;
                        if (nRet < 0)
                            return nRet;
                    }
                }

            }
            finally
            {
                m_container_lock.ReleaseReaderLock();
                //*************对库集合解读锁***********
#if DEBUG_LOCK
				this.WriteDebugInfo("Dir()，对库集合解读锁。");
#endif
            }

            END1:
            // 列出实际需要的项
            nTotalLength = aItem.Count;
            long lOutputLength;
            // return:
            //		-1  出错
            //		0   成功
            nRet = ConvertUtil.GetRealLength((int)lStart,
                (int)lLength,
                nTotalLength,
                (int)lMaxLength,
                out lOutputLength,
                out strError);
            if (nRet == -1)
                return -1;

            items = new ResInfoItem[(int)lOutputLength];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = aItem[i + (int)lStart];
            }

            return 0;
        }

        bool IsServerManager(User user)
        {
            // 检查当前帐户是否有写权限
            string strExistRights = "";
            bool bHasRight = user.HasRights("",
                ResType.Server,
                "management",
                out strExistRights);
            return bHasRight;
        }

        // 得到某一指定路径strPath的可以显示的下级
        // parameters:
        //		oUser	当前帐户
        //		db	当前数据库
        //		strPath	配置事项的路径
        //		strLang	语言版本
        //		aItem	out参数，返回可以显示的下级
        //		strError	out参数，出错信息
        // return:
        //		-1	出错
        //      -4  strCfgItemPath 对应的对象没有找到
        //		0	成功
        private int DirCfgItem(User user,
            string strCfgItemPath,
            out List<ResInfoItem> aItem,
            out string strError)
        {
            strError = "";
            aItem = new List<ResInfoItem>();

            if (this.NodeDbs == null)
            {
                strError = "服务器配置文件未定义<dbs>元素";
                return -1;
            }
            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count == 0)
            {
                strError = "未找到路径 '" + strCfgItemPath + "' 对应的事项";
                // return -1;
                return -4;  // 2017/6/7
            }

            if (list.Count > 1)
            {
                strError = "服务器端总配置文件不合法，检查到路径为'" + strCfgItemPath + "'对应的节点有'" + list.Count + "'个，有且只能有一个。";
                return -1;
            }
            XmlNode node = list[0];

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode child = node.ChildNodes[i];
                string strChildName = DomUtil.GetAttr(child, "name");
                if (strChildName == "")
                    continue;

                string strTempPath = strCfgItemPath + "/" + strChildName;
                string strExistRights;
                bool bHasRight = false;

                ResInfoItem resInfoItem = new ResInfoItem();
                resInfoItem.Name = strChildName;
                if (child.Name == "dir")
                {
                    bHasRight = user.HasRights(strTempPath,
                     ResType.Directory,
                     "list",
                     out strExistRights);
                    if (bHasRight == false)
                        continue;

                    resInfoItem.HasChildren = true;
                    resInfoItem.Type = 4;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");    // xietao 2006/6/5 add
                }
                else
                {
                    bHasRight = user.HasRights(strTempPath,
                        ResType.File,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = false;
                    resInfoItem.Type = 5;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");    // xietao 2006/6/5 add

                }
                aItem.Add(resInfoItem);
            }
            return 0;
        }

        // parameters:
        //      strCurrentDirectory 当前路径。物理路径
        // return:
        //      -1  出错
        //      其他  列出的事项总数。注意，不是 lLength 所指出的本次返回数
        public static int ListFile(
            string strRootPath,
            string strCurrentDirectory,
            string strPattern,
            long lStart,
            long lLength,
            out List<ResInfoItem> infos,
            out string strError)
        {
            strError = "";
            infos = new List<ResInfoItem>();

            int MAX_ITEMS = 100;    // 一次 API 最多返回的事项数量

            try
            {
                FileSystemLoader loader = new FileSystemLoader(strCurrentDirectory, strPattern);

                int i = 0;
                int count = 0;
                foreach (FileSystemInfo si in loader)
                {
                    // 检查文件或目录必须在根以下。防止漏洞
                    if (PathUtil.IsChildOrEqual(si.FullName, strRootPath) == false)
                        continue;

                    // 列文件自己的特性，这个功能不需要了
                    if (PathUtil.IsEqualEx(si.FullName, strCurrentDirectory) == true)
                        continue;

                    if (i < lStart)
                        goto CONTINUE;
                    if (lLength != -1 && count > lLength)
                        goto CONTINUE;

                    if (count >= MAX_ITEMS)
                        goto CONTINUE;

                    ResInfoItem info = new ResInfoItem();
                    infos.Add(info);
                    info.Name = si.Name;
                    info.TypeString = "createTime:" + si.CreationTime.ToString("u");

                    if (si is DirectoryInfo)
                    {
                        info.HasChildren = true;
                        info.Type = 4;
                    }

                    if (si is FileInfo)
                    {
                        info.HasChildren = false;
                        info.Type = 5;

                        FileInfo fi = si as FileInfo;
                        info.TypeString += ",size:" + fi.Length;
                    }

                    count++;

                    CONTINUE:
                    i++;
                }

                return i;
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
        }

        // 下载本地文件
        // TODO: 限制 nMaxLength 最大值
        // return:
        //      -2      文件不存在
        //		-1      出错
        //		>= 0	成功，返回最大长度
        public static long GetFile(
            string strFilePath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out byte[] outputTimestamp,
            out string strError)
        {
            destBuffer = null;
            outputTimestamp = null;
            strError = "";

            long lTotalLength = 0;
            FileInfo file = new FileInfo(strFilePath);
            if (file.Exists == false)
            {
                strError = " dp2Library 服务器不存在物理路径为 '" + strFilePath + "' 的文件";
                return -2;
            }

            // 1.取时间戳
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                outputTimestamp = FileUtil.GetFileTimestamp(strFilePath);
            }

#if NO
            // 2.取元数据
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == true)
                {
                    strMetadata = FileUtil.File2StringE(strMetadataFileName);
                }
            }
#endif

#if NO
            // 3.取range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                string strRangeFileName = GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == true)
                {
                    string strText = FileUtil.File2StringE(strRangeFileName);
                    string strTotalLength = "";
                    string strRange = "";
                    StringUtil.ParseTwoPart(strText, "|", out strRange, out strTotalLength);
                }
            }
#endif

            // 4.长度
            lTotalLength = file.Length;

            // 5.有data风格时,才会取数据
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (nLength == 0)  // 取0长度
                {
                    destBuffer = new byte[0];
                    return lTotalLength;
                }
                // 检查范围是否合法
                long lOutputLength;
                // return:
                //		-1  出错
                //		0   成功
                int nRet = ConvertUtil.GetRealLength(lStart,
                    nLength,
                    lTotalLength,
                    nMaxLength,
                    out lOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                using (FileStream s = new FileStream(strFilePath,
                    FileMode.Open))
                {
                    destBuffer = new byte[lOutputLength];

                    Debug.Assert(lStart >= 0, "");

                    s.Seek(lStart, SeekOrigin.Begin);
                    s.Read(destBuffer,
                        0,
                        (int)lOutputLength);
                }
            }
            return lTotalLength;
        }

        // 列出服务器下当前帐户有显示权限的数据库
        // 线：不安全的
        // parameters:
        //      strStyle    是否要列出所有语言的名字? "alllang"表示要列出所有语言的名字
        public int GetDirableChildren(User user,
            string strLang,
            string strStyle,
            out List<ResInfoItem> aItem,
            out string strError)
        {
            aItem = new List<ResInfoItem>();
            strError = "";

            if (this.NodeDbs == null)
            {
                strError = "服务器配置文件不合法，未定义<dbs>元素";
                return -1;
            }

            // TODO: 可否增加返回一个 item，表示本地文件的根目录?
            {
                ResInfoItem resInfoItem = new ResInfoItem();
                resInfoItem.HasChildren = true;
                resInfoItem.Type = 4;   // 目录
                resInfoItem.Name = KernelServerUtil.LOCAL_PREFIX;

                resInfoItem.TypeString = "";
                aItem.Add(resInfoItem);
            }

            foreach (XmlNode child in this.NodeDbs.ChildNodes)
            {
                string strChildName = DomUtil.GetAttr(child, "name");
                if (String.Compare(child.Name, "database", true) != 0
                    && strChildName == "")
                    continue;

                if (String.Compare(child.Name, "database", true) != 0
                    && String.Compare(child.Name, "dir", true) != 0
                    && String.Compare(child.Name, "file", true) != 0)
                {
                    continue;
                }

                string strExistRights;
                bool bHasRight = false;

                ResInfoItem resInfoItem = new ResInfoItem();
                if (String.Compare(child.Name, "database", true) == 0)
                {
                    string strID = DomUtil.GetAttr(child, "id");
                    Database db = this.GetDatabaseByID("@" + strID);
                    if (db == null)
                    {
                        strError = "未找到id为'" + strID + "'的数据库";
                        return -1;
                    }

                    bHasRight = user.HasRights(db.GetCaption("zh"),
                        ResType.Database,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;

                    if (StringUtil.IsInList("account", db.GetDbType(), true) == true)
                        resInfoItem.Style = 1;
                    else
                        resInfoItem.Style = 0;

                    resInfoItem.TypeString = db.GetDbType();

                    resInfoItem.Name = db.GetCaptionSafety(strLang);
                    resInfoItem.Type = 0;   // 数据库
                    resInfoItem.HasChildren = true;

                    // 如果要获得全部语言的名字
                    if (StringUtil.IsInList("alllang", strStyle) == true)
                    {
                        List<string> results = db.GetAllLangCaptionSafety();
                        string[] names = new string[results.Count];
                        results.CopyTo(names);
                        resInfoItem.Names = names;
                    }
                }
                else if (String.Compare(child.Name, "dir", true) == 0)
                {
                    bHasRight = user.HasRights(strChildName,
                        ResType.Directory,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = true;
                    resInfoItem.Type = 4;   // 目录
                    resInfoItem.Name = strChildName;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");   // xietao 2006/6/5 add
                }
                else
                {
                    bHasRight = user.HasRights(strChildName,
                        ResType.File,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = false;
                    resInfoItem.Name = strChildName;
                    resInfoItem.Type = 5;   // 文件?

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");   // xietao 2006/6/5 add
                }
                aItem.Add(resInfoItem);
            }

            return 0;
        }

        void resultset_GetTempFilename(object sender, GetTempFilenameEventArgs e)
        {
            e.TempFilename = GetTempFileName();
        }

        // 根据用户名从库中查找用户记录，得到用户对象
        // 对象尚未进入集合, 因此无需为对象加锁
        // parameters:
        //		strBelongDb	用户从属的数据库,中文名称
        //      user        out参数，返回帐户对象
        //      strError    out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	未找到帐户
        //		1	找到了
        // 线：安全
        internal int ShearchUserSafety(string strUserName,
            out User user,
            out string strError)
        {
            user = null;
            strError = "";

            int nRet = 0;

            DpRecord record = null;

            DpResultSet resultSet = new DpResultSet(GetTempFileName);
            resultSet.GetTempFilename += new GetTempFilenameEventHandler(resultset_GetTempFilename);

            try
            {
                //*********对帐户库集合加读锁***********
                m_container_lock.AcquireReaderLock(m_nContainerLockTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("ShearchUser()，对数据库集合加读锁。");
#endif
                try
                {
                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.SearchUserInternal(strUserName,
                        resultSet,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                finally
                {
                    //*********对帐户库集合解读锁*************
                    m_container_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("ShearchUser()，对数据库集合解读锁。");
#endif
                }

                // 根据用户名没找到对应的帐户记录
                long lCount = resultSet.Count;
                if (lCount == 0)
                    return 0;

                if (lCount > 1)
                {
                    strError = "用户名'" + strUserName + "'对应多条记录";
                    return -1;
                }

                // 按第一个帐户算
                record = (DpRecord)resultSet[0];
            }
            finally
            {
                // 2016/1/23 卸载事件
                resultSet.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);
            }

            DbPath path = new DbPath(record.ID);

            // 找到指定帐户数据库
            Database db = this.GetDatabaseSafety(path.Name);
            if (db == null)
            {
                strError = "未找到'" + strUserName + "'帐户对应的名为'" + path.Name + "'的数据库对象";
                return -1;
            }

            // 从帐户库中找到记录
            string strXml = "";
            // return:
            //      -1  出错
            //      -4  记录不存在
            //      0   正确
            nRet = db.GetXmlDataSafety(path.ID,
                out strXml,
                out strError);
            if (nRet <= -1)  // 将-4与-1都作为-1返回
                return -1;

            //加载到dom
            XmlDocument dom = new XmlDocument();
            //dom.PreserveWhitespace = true; //设PreserveWhitespace为true
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "加载用户 '" + strUserName + "' 的帐户记录到dom时出错,原因:" + ex.Message;
                return -1;
            }

            user = new User();
            // return:
            //      -1  出错
            //      0   成功
            nRet = user.Initial(
                record.ID,
                dom,
                db,
                this,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 根据记录路径得到数据库对象
        public Database GetDatabaseFromRecPathSafety(string strRecPath)
        {
            // 创建一个DpPsth实例
            DbPath path = new DbPath(strRecPath);

            // 找到指定帐户数据库
            return this.GetDatabaseSafety(path.Name);
        }

        // 从所有帐户库的所有表中查找帐户
        // parameter
        //		strUserName 用户名
        //		resultSet   结果集,用于存放查找到的用户
        //      strError    out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        // 线：不安全
        private int SearchUserInternal(string strUserName,
            DpResultSet resultSet,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName不能为空";
                return -1;
            }

            foreach (Database db in this)
            {
                if (StringUtil.IsInList("account", db.GetDbType()) == false)
                    continue;

                if (strUserName.Length > db.KeySize)
                    continue;

                string strWarning = "";
                SearchItem searchItem = new SearchItem();
                searchItem.TargetTables = "";
                searchItem.Word = strUserName;
                searchItem.Match = "exact";
                searchItem.Relation = "=";
                searchItem.DataType = "string";
                searchItem.MaxCount = -1;
                searchItem.OrderBy = "";

                // 帐户库不能去非用字
                // return:
                //		-1	出错
                //		0	成功
                int nRet = db.SearchByUnion(
                    "",
                    searchItem,
                    null,       //用于中断 , deleget
                    resultSet,
                    0,
                    out strError,
                    out strWarning);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

    } // end of class DatabaseCollection


#if NO
    //*****************************************************

    // string类型的ArrayList排序编的IComparer接口
    public class ComparerClass : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            if (!(x is String))
                throw new Exception("object x is not a String");
            if (!(y is String))
                throw new Exception("object y is not a String");

            string strText1 = (string)x;
            string strText2 = (string)y;

            return String.Compare(strText1, strText2, true);
        }
    }
#endif

    // 检查通讯是否连接着的delegate
    // public delegate bool Delegate_isConnected();

    public delegate void ChannelIdleEventHandler(object sender,
ChannelIdleEventArgs e);

    /// <summary>
    /// 空闲事件的参数
    /// </summary>
    public class ChannelIdleEventArgs : EventArgs
    {
        public bool Continue = true;
    }


    public class ChannelHandle
    {
        //public DatabaseCollection Dbs = null;
        public KernelApplication App = null;

        bool m_bStop = false;

        public event ChannelIdleEventHandler Idle = null;
        public event EventHandler Stop = null;

        public void Clear()
        {
            this.m_bStop = false;
        }

        // return:
        //      false   希望停止
        //      true    希望继续
        public bool DoIdle()
        {
            if (this.m_bStop == true)
                return false;

            if (this.Idle == null)
                return true;    // 永远不停止

            ChannelIdleEventArgs e = new ChannelIdleEventArgs();
            this.Idle(this, e);

            if (e.Continue == false)
            {
                this.App.MyWriteDebugInfo("abort");

                this.m_bStop = true;    // 2011/1/19 

                return false;
            }
            return true;
        }

        public void DoStop()
        {
            this.m_bStop = true;

            if (this.Stop != null)
            {
                this.Stop(this, null);
            }
        }

        public bool Stopped
        {
            get
            {
                return this.m_bStop;
            }
        }

        /*
        public bool DoIdle()
        {
            if (this.Response1.IsClientConnected == false)
            {
                this.Dbs.MyWriteDebugInfo("abort");
                return false;
            }
            this.Dbs.MyWriteDebugInfo("is...!");
            return true;
        }

        public void DoStop()
        {
            if (this.Response1 != null)
            {
                this.Response1.Close();
            }
        }
         * */
    }

    #region 专门用于检索的类

    public class DatabaseCommandTask : IDisposable
    {
        public object m_command = null;
        public AutoResetEvent m_event = new AutoResetEvent(false);

        public bool bError = false;
        public string ErrorString = "";
        // 供外部使用
        public /*SqlDataReader*/object DataReader = null;

        public bool Canceled = false;

        public void Dispose()
        {
            m_event.Dispose();
        }

        public DatabaseCommandTask(object command)
        {
            m_command = command;
        }

        public void Cancel()
        {
            this.Canceled = true;

            // CloseConnection();

            if (m_command is SqlCommand)
                ((SqlCommand)m_command).Cancel();
            else if (m_command is SQLiteCommand)
                ((System.Data.SQLite.SQLiteCommand)m_command).Cancel();
            else if (m_command is MySqlCommand)
            {
                try
                {
                    ((MySqlCommand)m_command).Cancel();
                }
                catch
                {
                }
            }
            else if (m_command is OracleCommand)
                ((OracleCommand)m_command).Cancel();

        }

        /*
dp2LibraryXE 发生未知的异常:

发生未捕获的异常: 
Type: System.ObjectDisposedException
Message: 已关闭 Safe handle
Stack:
   在 System.Runtime.InteropServices.SafeHandle.DangerousAddRef(Boolean& success)
   在 System.StubHelpers.StubHelpers.SafeHandleAddRef(SafeHandle pHandle, Boolean& success)
   在 Microsoft.Win32.Win32Native.SetEvent(SafeWaitHandle handle)
   在 System.Threading.EventWaitHandle.Set()
   在 DigitalPlatform.rms.DatabaseCommandTask.ThreadMain() 位置 c:\dp2-master\dp2\DigitalPlatform.rms.db\DatabaseCollection.cs:行号 6279
   在 System.Threading.ThreadHelper.ThreadStart_Context(Object state)
   在 System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
   在 System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
   在 System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
   在 System.Threading.ThreadHelper.ThreadStart()


dp2LibraryXE 版本: dp2LibraryXE, Version=1.1.5939.41661, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.2.9200.0
本机 MAC 地址: F0DEF174382F,CC52AFE3CF21,8CA982C371DB,8CA982C371DA
---
         * */
        // 主函数
        public void ThreadMain()
        {
            try
            {
                if (this.Canceled == false)
                {
                    if (m_command is SqlCommand)
                        DataReader = ((SqlCommand)m_command).ExecuteReader(CommandBehavior.CloseConnection);
                    else if (m_command is SQLiteCommand)
                        DataReader = ((SQLiteCommand)m_command).ExecuteReader(CommandBehavior.CloseConnection);
                    else if (m_command is MySqlCommand)
                        DataReader = ((MySqlCommand)m_command).ExecuteReader(CommandBehavior.CloseConnection);
                    else if (m_command is OracleCommand)
                        DataReader = ((OracleCommand)m_command).ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (SqlException sqlEx)
            {
                this.bError = true;
                string strConnectionName = ((SqlCommand)m_command).Connection.GetHashCode().ToString();
                this.ErrorString = "检索线程(1):" + SqlDatabase.GetSqlErrors(sqlEx) + "; connection hashcode='" + strConnectionName + "'"; ;
            }
            catch (SQLiteException sqlEx)
            {
                this.bError = true;
                string strConnectionName = ((SQLiteCommand)m_command).Connection.GetHashCode().ToString();
                this.ErrorString = "检索线程(1):" + sqlEx.ToString() + "; connection hashcode='" + strConnectionName + "'"; ;
            }
            catch (MySqlException sqlEx)
            {
                this.bError = true;
                string strConnectionName = ((MySqlCommand)m_command).Connection.GetHashCode().ToString();
                this.ErrorString = "检索线程(1):" + sqlEx.ToString() + "; connection hashcode='" + strConnectionName + "'"; ;
            }
            catch (OracleException sqlEx)
            {
                this.bError = true;
                string strConnectionName = ((OracleCommand)m_command).Connection.GetHashCode().ToString();
                this.ErrorString = "检索线程(1):" + sqlEx.ToString() + "; connection hashcode='" + strConnectionName + "'"; ;
            }
            catch (Exception ex)
            {
                this.bError = true;
                string strConnectionName = "";
                if (m_command is SqlCommand)
                    strConnectionName = ((SqlCommand)m_command).Connection.GetHashCode().ToString();
                else if (m_command is SQLiteCommand)
                    strConnectionName = ((SQLiteCommand)m_command).Connection.GetHashCode().ToString();
                else if (m_command is MySqlCommand)
                    strConnectionName = ((MySqlCommand)m_command).Connection.GetHashCode().ToString();
                else if (m_command is OracleCommand)
                    strConnectionName = ((OracleCommand)m_command).Connection.GetHashCode().ToString();

                this.ErrorString = "检索线程(2): " + ex.Message + "; connection hashcode='" + strConnectionName + "'";
            }
            finally  // 一定要返回信号
            {

                try
                {
                    m_event.Set();
                }
                catch
                {

                }

                // 本线程负责释放资源
                CloseReader();
                CloseConnection();
                DisposeCommand();
            }
        }

        public void DisposeCommand()
        {
            if (this.m_command == null
    || this.Canceled == false)
                return;

            if (m_command is SqlCommand)
            {
                ((SqlCommand)m_command).Dispose();
                // ((SqlCommand)m_command).Connection.Close();
            }
            else if (m_command is SQLiteCommand)
                ((SQLiteCommand)m_command).Dispose();
            else if (m_command is MySqlCommand)
                ((MySqlCommand)m_command).Dispose();
            else if (m_command is OracleCommand)
                ((OracleCommand)m_command).Dispose();
        }

        public void CloseConnection()
        {
            if (this.m_command == null
    || this.Canceled == false)
                return;

            if (m_command is SqlCommand)
                ((SqlCommand)m_command).Connection.Close();
            else if (m_command is SQLiteCommand)
                ((SQLiteCommand)m_command).Connection.Close();
            else if (m_command is MySqlCommand)
                ((MySqlCommand)m_command).Connection.Close();
            else if (m_command is OracleCommand)
                ((OracleCommand)m_command).Connection.Close();
        }

        public void CloseReader()
        {
            if (this.DataReader == null
                || this.Canceled == false)
                return;

            if (this.DataReader is SqlDataReader)
                ((SqlDataReader)this.DataReader).Close();
            else if (this.DataReader is SQLiteDataReader)
                ((SQLiteDataReader)this.DataReader).Close();
            else if (this.DataReader is MySqlDataReader)
                ((MySqlDataReader)this.DataReader).Close();
            else if (this.DataReader is OracleDataReader)
                ((OracleDataReader)this.DataReader).Close();
        }
    }

    #endregion

    // 资源项信息
    // 当时放在DigitalPlatform.rms.Service里，后来要在Database.xml里使用，所以移动到这儿
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class ResInfoItem
    {
        [DataMember]
        public int Type;	// 类型：0 库 / 1 途径 / 4 cfgs / 5 file
        [DataMember]
        public string Name;	// 库名或途径名
        [DataMember]
        public bool HasChildren = true;  //是否有儿子
        [DataMember]
        public int Style = 0;   // 0x01:帐户库  // 原名Style

        [DataMember]
        public string TypeString = "";  // 新增
        [DataMember]
        public string[] Names;    // 新增 所有语言下的名字。每个元素的格式 语言代码:内容
    }

    /// <summary>
    /// SQL 服务器类型
    /// </summary>
    public enum SqlServerType
    {
        None = 0,
        MsSqlServer = 1,
        SQLite = 2,
        MySql = 3,
        Oracle = 4,
#if NO
        LocalDB = 5,    // MS SQL Server LocalDB, 2015/5/17
#endif
    }
}

