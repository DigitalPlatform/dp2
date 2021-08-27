//#define DEBUG_LOCK_SQLDATABASE
//#define XML_WRITE_TO_FILE   // 将尺寸符合要求的XML记录也写入对象文件
//#define UPDATETEXT_WITHLOG    // 在需要快照复制的时候才有用
// #define PARAMETERS  // ModifyKeys() MySql 版本使用参数SQL命令

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;

//using MySql.Data;
//using MySql.Data.MySqlClient;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

using Ghostscript.NET.Rasterizer;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Core;
using MySqlConnector;

namespace DigitalPlatform.rms
{
    // SQL库派生类
    // TODO: 增加 IDisposable 接口
    public class SqlDatabase : Database
    {
        // 对物理文件开始缓存和加速的开始尺寸
        const int CACHE_SIZE = 10 * 1024;   // -1;  // 10 * 1024;

        // PDF 单页缓存
        internal PageCache _pageCache = new PageCache(1000);

        internal StreamCache _streamCache = new StreamCache(100);

        const string KEY_COL_LIST = "(keystring, idstring)";
        const string KEYNUM_COL_LIST = "(keystringnum, idstring)";

        public SQLiteInfo SQLiteInfo = null;

        // 这个 FastMode 是专门针对 SQLite 数据库的
        public bool FastMode
        {
            get
            {
                if (this.SQLiteInfo != null && this.SQLiteInfo.FastMode == true)
                    return true;
                return false;
            }
            set
            {
                if (this.container.SqlServerType != SqlServerType.SQLite)
                    return;

                if (this.SQLiteInfo == null)
                {
                    this.SQLiteInfo = new SQLiteInfo();
                }

                if (this.FastMode == true
                    && value == false)
                {
                    this.Commit();
                    /*
                    if (this.SQLiteInfo.m_connection != null)
                        this.Close();
                     * */
                }
                this.SQLiteInfo.FastMode = value;
            }
        }

        static int m_nLongTimeout = 30 * 60; //  
        // 连接字符串
        private string m_strConnStringPooling = "";        // 普通连接字符串，pooling = true
        private string m_strConnString = "";        // 普通连接字符串，pooling = false
        private string m_strLongConnString = "";    // timeout较长的连接字符串, pooling = false

        // Sql数据库名称
        private string m_strSqlDbName = "";

        private string m_strObjectDir = "";     // 对象文件存储目录
        private long m_lObjectStartSize = 0x7ffffffe;   // 10 * 1024;    // 大于等于这个尺寸的对象将存储在对象文件中。-1表示永远不使用对象目录

        public SqlDatabase(DatabaseCollection container)
            : base(container)
        { }

        public static string GetSqlErrors(SqlException exception)
        {
            if (exception.Errors is SqlErrorCollection)
            {
                string strResult = "";
                for (int i = 0; i < exception.Errors.Count; i++)
                {
                    strResult += "error " + (i + 1).ToString() + ": " + exception.Errors[i].ToString() + "\r\n";
                }
                return strResult;
            }
            else
            {
                return exception.Message;
            }
        }

        // 初始化数据库对象
        // parameters:
        //      node    数据库配置节点<database>
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        internal override int Initial(XmlNode node,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (node == null)
                throw new ArgumentException("Initial()调用错误，node参数值不能为null", "node");

            Debug.Assert(node != null, "Initial()调用错误，node参数值不能为null。");

            //****************对数据库加写锁**** 在构造时,既不能读也不能写
            this.m_db_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
                this.m_selfNode = node;

                // 只能在这儿写了，要不对象未初始化呢。
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif

                // 检索点长度
                // return:
                //      -1  出错
                //      0   成功
                // 线: 不安全
                nRet = this.container.InternalGetKeySize(
                    out this.KeySize,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 库ID
                this.PureID = DomUtil.GetAttr(this.m_selfNode, "id").Trim();
                if (this.PureID == "")
                {
                    strError = "配置文件不合法，在 name 为 '" + this.GetCaption("zh-CN") + "' 的 <database> 元素中未定义 id 属性，或 id 属性值为空";
                    return -1;
                }

                // 属性节点
                this.PropertyNode = this.m_selfNode.SelectSingleNode("property");
                if (this.PropertyNode == null)
                {
                    strError = "配置文件不合法，在 name 为 '" + this.GetCaption("zh-CN") + "' 的 <database> 元素下级未定义 <property> 元素";
                    return -1;
                }

                // <sqlserverdb>节点
                XmlNode nodeSqlServerDb = this.PropertyNode.SelectSingleNode("sqlserverdb");
                if (nodeSqlServerDb == null)
                {
                    strError = "配置文件不合法，在 name 为 '" + this.GetCaption("zh-CN") + "' 的 database/property 下级未定义 <sqlserverdb> 元素";
                    return -1;
                }

                // 检查SqlServer库名，只有Sql类型库才需要
                this.m_strSqlDbName = DomUtil.GetAttr(nodeSqlServerDb, "name").Trim();
                if (this.m_strSqlDbName == "")
                {
                    strError = "配置文件不合法，在 name 为'" + this.GetCaption("zh-CN") + "' 的 database/property/sqlserverdb 的节点未定义 name 属性，或 name 属性值为空";
                    return -1;
                }

                // <object>节点
                XmlNode nodeObject = this.PropertyNode.SelectSingleNode("object");
                if (nodeObject != null)
                {
                    this.m_strObjectDir = DomUtil.GetAttr(nodeObject, "dir").Trim();

                    if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                    {
                        // 检查对象文件目录是否符合规则
                        // 不能使用根目录
                        string strRoot = Directory.GetDirectoryRoot(this.m_strObjectDir);
                        if (PathUtil.IsEqual(strRoot, this.m_strObjectDir) == true)
                        {
                            strError = "对象目录定义 '" + this.m_strObjectDir + "' 不合法。对象目录不能是根目录";
                            // 给错误日志写一条信息
                            this.container.KernelApplication.WriteErrorLog(strError);
                            return -1;
                        }
                    }

                    long lValue = 0;
                    if (DomUtil.GetIntegerParam(nodeObject,
                        "startSize",
                        this.m_lObjectStartSize,
                        out lValue,
                        out strError) == -1)
                    {
                        strError = "读取数据库的 startSize 参数时发生错误：" + strError;
                        return -1;
                    }

                    this.m_lObjectStartSize = lValue;
                }

                if (this.container.SqlServerType != SqlServerType.MsSqlServer)
                {
                    this.m_lObjectStartSize = 0;    // 在不是MS SQL Server情况下，所有对象都写入对象文件
                }

                if (this.m_lObjectStartSize != -1)
                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        // 采用缺省配置
                        this.m_strObjectDir = PathUtil.MergePath(this.container.ObjectDir, this.m_strSqlDbName);
                        try
                        {
                            PathUtil.TryCreateDir(this.m_strObjectDir);
                        }
                        catch (Exception ex)
                        {
                            strError = "创建单个数据库的数据对象目录 '" + this.m_strObjectDir + "' 时出错: " + ex.Message;
                            return -1;
                        }
                    }
                }

#if NO
                // *****************************************
                this.m_lObjectStartSize = 0;    // testing !!!
#endif

                if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    this.SQLiteInfo = new SQLiteInfo();
                }

                // return:
                //      -1  出错
                //      0   成功
                // 线: 不安全的
                nRet = this.InternalGetConnectionString(
                    30,
                    "pooling", // "",   2017/6/13 注：不知以前这里为何区分了 pooling 和 没有 pooling 的两种连接字符串，权且改为都用 pooling 的尝试一下
                    out this.m_strConnString,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 2012/2/17
                nRet = this.InternalGetConnectionString(
    30,
    "pooling",
    out this.m_strConnStringPooling,
    out strError);
                if (nRet == -1)
                    return -1;

                //      -1  出错
                //      0   成功
                // 线: 不安全的
                nRet = this.InternalGetConnectionString(
                    m_nLongTimeout,
                    "",
                    out this.m_strLongConnString,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            finally
            {
                m_db_lock.ReleaseWriterLock();
                //***********对数据库解写锁*************
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }

            return 0;
        }

        internal override void Close()
        {
            this.CloseInternal();
        }

        void CloseInternal(bool bLock = true)
        {
#if NO
            if (bLock == true)
                this.m_db_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
#endif
            if (this.SQLiteInfo != null)
            {
                lock (this.SQLiteInfo)
                {
                    if (this.SQLiteInfo != null
                        && this.SQLiteInfo.m_connection != null)
                    {
                        this.SQLiteInfo.m_connection.Close(false);
                        this.SQLiteInfo.m_connection = null;
                    }
                }
            }
#if NO
            }
            finally
            {
                if (bLock == true)
                    m_db_lock.ReleaseWriterLock();
            }
#endif
        }

        // 异常：可能会抛出异常
        internal override void Commit()
        {
            try
            {
                // 评估时间
                DateTime start_time = DateTime.Now;

                this.CommitInternal();

                TimeSpan delta = DateTime.Now - start_time;
                int nTicks = (int)(delta.TotalSeconds * 1000);
                if (this.m_nTimeOut < nTicks * 2)
                    this.m_nTimeOut = nTicks * 2;

                if (nTicks > 5000 && this.SQLiteInfo != null
                    && this.SQLiteInfo.m_connection != null)
                {
                    this.SQLiteInfo.m_connection._nThreshold = 100;
                }
            }
            catch (Exception ex)
            {
                string strError = ExceptionUtil.GetAutoText(ex);
            }
        }

        void CommitInternal(bool bLock = true)
        {
            if (this.SQLiteInfo == null
                || this.SQLiteInfo.m_connection == null)
                return;

            if (bLock == true)
                this.m_db_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
                if (this.SQLiteInfo != null)
                {
                    lock (this.SQLiteInfo)
                    {
                        if (this.SQLiteInfo != null
                            && this.SQLiteInfo.m_connection != null)
                        {
                            this.SQLiteInfo.m_connection.Commit(bLock);

                            /*
                            this.SQLiteInfo.m_connection.Close(false);
                            this.SQLiteInfo.m_connection = null;
                             * */
                        }
                    }
                }
            }
            finally
            {
                if (bLock == true)
                    m_db_lock.ReleaseWriterLock();
            }
        }

        // 得到链接字符串,只有库类型为SqlDatabase时才有意义
        // parameters:
        //      strStyle    风格。pooling 如果不具备，则加入pooling = false
        //      strConnection   out参数，返回连接字符串联
        //      strError        out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线: 不安全的
        internal int InternalGetConnectionString(
            int nTimeout,
            string strStyle,
            out string strConnection,
            out string strError)
        {
            strConnection = "";
            strError = "";

            XmlNode nodeDataSource = this.container.CfgDom.DocumentElement.SelectSingleNode("datasource");
            if (nodeDataSource == null)
            {
                strError = "服务器配置文件不合法，未在根元素下定义<datasource>元素";
                return -1;
            }

            string strMode = DomUtil.GetAttr(nodeDataSource, "mode");

            SqlServerType servertype = this.container.SqlServerType;

            if (servertype == SqlServerType.SQLite)
            {
                if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                {
                    strError = "数据库 '" + this.GetCaption("zh-CN") + "' 没有定义 m_strObjectDir 值";
                    return -1;
                }
                strConnection = "Data Source=" + PathUtil.MergePath(this.m_strObjectDir, "sqlite_database.bin")
                    + ";Page Size=8192";   // Synchronues=OFF;;Cache Size=70000
                return 0;
            }

#if NO
            // 2015/5/17
            if (servertype == SqlServerType.LocalDB)
            {
                if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                {
                    strError = "数据库 '" + this.GetCaption("zh-CN") + "' 没有定义 m_strObjectDir 值";
                    return -1;
                }
                //  "Data Source=(localdb)\v11.0;Integrated Security=true;AttachDbFileName=C:\MyData\Database1.mdf".
                strConnection = "Data Source=(localdb)\v11.0;Integrated Security=true;AttachDbFileName=" + Path.Combine(this.m_strObjectDir, "localdb_database.mdf");
                return 0;
            }
#endif

            if (servertype == SqlServerType.MySql)
            {
                if (String.IsNullOrEmpty(strMode) == true)
                {
                    string strUserID = "";
                    string strPassword = "";

                    strUserID = DomUtil.GetAttr(nodeDataSource, "userid").Trim();
                    if (strUserID == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'userid'属性，或'userid'属性值为空。";
                        return -1;
                    }

                    strPassword = DomUtil.GetAttr(nodeDataSource, "password").Trim();
                    if (strPassword == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'password'属性，或'password'属性值为空。";
                        return -1;
                    }
                    // password可能为空
                    try
                    {
                        strPassword = Cryptography.Decrypt(strPassword,
                                "dp2003");
                    }
                    catch
                    {
                        strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                        return -1;
                    }

                    strConnection = @"Persist Security Info=False;"
                        + "User ID=" + strUserID + ";"    //帐户和密码
                        + "Password=" + strPassword + ";"
                        //+ "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + this.container.SqlServerName + ";"
                        // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
                        + "Connect Timeout=" + nTimeout.ToString() + ";"
                        // https://stackoverflow.com/questions/45086283/mysql-data-mysqlclient-mysqlexception-the-host-localhost-does-not-support-ssl
                        + "SslMode=none;"   // 2018/9/25 当 mode 属性为空的时候，表示需要兼容以前安装的效果，那就相当于 None (SSL)
                        + "charset=utf8;";
                }
                else if (strMode.StartsWith("SslMode:"))
                {
                    string strUserID = "";
                    string strPassword = "";

                    // https://dev.mysql.com/doc/connector-net/en/connector-net-6-10-connection-options.html
                    /*
SslMode , SSL Mode , Ssl-Mode 
Default: Preferred 
This option was introduced in Connector/NET 6.2.1 and has the following values: 
None - Do not use SSL. 
Preferred - Use SSL if the server supports it, but allow connection in all cases. 
Required - Always use SSL. Deny connection if server does not support SSL. 
VerifyCA - Always use SSL. Validate the CA but tolerate name mismatch. 
VerifyFull - Always use SSL. Fail if the host name is not correct. 
* */
                    string strSslMode = strMode.Substring("SslMode:".Length);
                    // 注意：mode 属性不为空，但 SslMode: 为空的情况，等同于 Preferred 情况，也就是连接字符串中不包含 SslMode=xxx; 参数的情况
                    if (strSslMode == "Preferred")
                        strSslMode = "";

                    strUserID = DomUtil.GetAttr(nodeDataSource, "userid").Trim();
                    if (strUserID == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'userid'属性，或'userid'属性值为空。";
                        return -1;
                    }

                    strPassword = DomUtil.GetAttr(nodeDataSource, "password").Trim();
                    if (strPassword == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'password'属性，或'password'属性值为空。";
                        return -1;
                    }
                    // password可能为空
                    try
                    {
                        strPassword = Cryptography.Decrypt(strPassword,
                                "dp2003");
                    }
                    catch
                    {
                        strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                        return -1;
                    }

                    strConnection = @"Persist Security Info=False;"
                        + "User ID=" + strUserID + ";"    //帐户和密码
                        + "Password=" + strPassword + ";"
                        //+ "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + this.container.SqlServerName + ";"
                        // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
                        + "Connect Timeout=" + nTimeout.ToString() + ";"
                        // https://stackoverflow.com/questions/45086283/mysql-data-mysqlclient-mysqlexception-the-host-localhost-does-not-support-ssl
                        + (string.IsNullOrEmpty(strSslMode) ? "" : "SslMode=" + strSslMode + ";")    // 2018/9/23
                        + "charset=utf8;";
                }
                else if (strMode == "SSPI") // 2006/3/22
                {
                    strConnection = @"Persist Security Info=False;"
                        + "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + this.container.SqlServerName + ";"
                        + "Connect Timeout=" + nTimeout.ToString() + ";" // 30秒
                        + "charset=utf8;";
                }
                else
                {
                    strError = "服务器配置文件不合法，根元素下级的<datasource>定义mode属性值'" + strMode + "'不合法。";
                    return -1;
                }

                if (StringUtil.IsInList("pooling", strStyle) == false)
                    strConnection += "Pooling=false;";

                return 0;
            }

            if (servertype == SqlServerType.Oracle)
            {
                if (String.IsNullOrEmpty(strMode) == true)
                {
                    string strUserID = "";
                    string strPassword = "";

                    strUserID = DomUtil.GetAttr(nodeDataSource, "userid").Trim();
                    if (strUserID == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'userid'属性，或'userid'属性值为空。";
                        return -1;
                    }

                    strPassword = DomUtil.GetAttr(nodeDataSource, "password").Trim();
                    if (strPassword == "")
                    {
                        strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'password'属性，或'password'属性值为空。";
                        return -1;
                    }
                    // password可能为空
                    try
                    {
                        strPassword = Cryptography.Decrypt(strPassword,
                                "dp2003");
                    }
                    catch
                    {
                        strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                        return -1;
                    }

                    strConnection = @"Persist Security Info=False;"
                        + "User ID=" + strUserID + ";"    //帐户和密码
                        + "Password=" + strPassword + ";"
                        //+ "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + this.container.SqlServerName + ";"
                        // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
                        + "Connect Timeout=" + nTimeout.ToString() + ";";

                }
                else if (strMode == "SSPI") // 2006/3/22
                {
                    strConnection = @"Persist Security Info=False;"
                        + "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + this.container.SqlServerName + ";"
                        + "Connect Timeout=" + nTimeout.ToString() + ";"; // 30秒
                }
                else
                {
                    strError = "服务器配置文件不合法，根元素下级的<datasource>定义mode属性值'" + strMode + "'不合法。";
                    return -1;
                }

                // 全部用pooling
                /*
                if (StringUtil.IsInList("pooling", strStyle) == false)
                    strConnection += "Pooling=false;";
                 * */
                return 0;
            }


            if (String.IsNullOrEmpty(strMode) == true)
            {
                string strUserID = "";
                string strPassword = "";

                strUserID = DomUtil.GetAttr(nodeDataSource, "userid").Trim();
                if (strUserID == "")
                {
                    strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'userid'属性，或'userid'属性值为空。";
                    return -1;
                }

                strPassword = DomUtil.GetAttr(nodeDataSource, "password").Trim();
                if (strPassword == "")
                {
                    strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'password'属性，或'password'属性值为空。";
                    return -1;
                }
                // password可能为空
                try
                {
                    strPassword = Cryptography.Decrypt(strPassword,
                            "dp2003");
                }
                catch
                {
                    strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                    return -1;
                }

                strConnection = @"Persist Security Info=False;"
                    + "User ID=" + strUserID + ";"    //帐户和密码
                    + "Password=" + strPassword + ";"
                    //+ "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + this.container.SqlServerName + ";"
                    // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
                    + "Connect Timeout=" + nTimeout.ToString() + ";";

            }
            else if (strMode == "SSPI") // 2006/3/22
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + this.container.SqlServerName + ";"
                    + "Connect Timeout=" + nTimeout.ToString() + ";"; // 30秒
            }
            else
            {
                strError = "服务器配置文件不合法，根元素下级的<datasource>定义mode属性值'" + strMode + "'不合法。";
                return -1;
            }

            /*
            if (StringUtil.IsInList("pooling", strStyle) == false)
                strConnection += "Pooling=false;";
             * */

            /*
        else
            strConnection += "Max Pool Size=1000;";
             * */
            strConnection += "Asynchronous Processing=true;";
            return 0;
        }

        // 得到数据源名称，对于Sql数据库，则是Sql数据库名。
        public override string GetSourceName()
        {
            return this.m_strSqlDbName;
        }

#if NO
        // 删除一个目录内的所有文件和目录
        // parameters:
        //      strExcludeFileName  想要保留的文件名，全路径
        static bool ClearDir(string strDir,
            string strExcludeFileName)
        {
            //try
            //{
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
                return true;

            // 删除所有的下级目录
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo childDir in dirs)
            {
                if (PathUtil.IsChildOrEqual(strExcludeFileName, childDir.FullName) == true)
                {
                    ClearDir(childDir.FullName, strExcludeFileName);    // 递归
                }
                else
                    Directory.Delete(childDir.FullName, true);
            }

            // 删除所有文件
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.FullName.ToLower() != strExcludeFileName.ToLower())
                    File.Delete(fi.FullName);
            }

            return true;
#if NO
            }
            catch (Exception ex)
            {
                return false;
            }
#endif
        }
#endif
        static string GetExceptionString(DbCommand command,
            string strText,
            Exception ex)
        {
            return strText + "\r\n"
    + ex.Message + "\r\n"
    + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
    + "SQL命令:\r\n"
    + command.CommandText;
        }

        // 初始化数据库，注意虚函数不能为private
        // parameter:
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的
        // 加写锁的原因，修改记录尾号，另外对SQL的操作不必担心锁
        public override int InitialPhysicalDatabase(out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif

            try
            {
#if NO
                if (this.RebuildIDs != null && this.RebuildIDs.Count > 0)
                {
                    this.RebuildIDs.Delete();
                    this.RebuildIDs = null;
                }
#endif
                DeleteRebuildIDs();

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try //连接
                    {
                        string strCommand = "";

                        // 2015/5/17 LocalDB 要求先删除 SQL 数据库以后才能删除对象目录，因为数据库文件在这个目录中
                        // 1.删除 SQL 数据库
                        if (this.IsLocalDB() == true)
                        {
                            strCommand = "use master " + "\n"
                                + " if exists (select * from dbo.sysdatabases where name = N'" + this.m_strSqlDbName + "')" + "\n"
                                + " drop database " + this.m_strSqlDbName + "\n";
                            strCommand += " use master " + "\n";
                            using (SqlCommand command = new SqlCommand(strCommand,
                                connection))
                            {
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception /*ex*/)
                                {
#if NO
                                    strError = "删除 SQL 库出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
                                    return -1;
#endif
                                }
                            }
                        }

                        // 2. 删除对象目录，然后重建
                        try
                        {
                            if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                            {
                                _streamCache.ClearAll();
                                _pageCache.ClearAll();
                                PathUtil.DeleteDirectory(this.m_strObjectDir);
                                PathUtil.TryCreateDir(this.m_strObjectDir);
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "清除 数据库 '" + this.GetCaption("zh") + "' 的 原有对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                            return -1;
                        }


                        // 3.建库
                        strCommand = this.GetCreateDbCmdString(this.container.SqlServerType);
                        using (SqlCommand command = new SqlCommand(strCommand,
                            connection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2013/2/10
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "建库出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }

                            // 4.建表
                            int nRet = this.GetCreateTablesString(
                                this.container.SqlServerType,
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            command.CommandText = strCommand;
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "建表出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }

                            // 5.建索引
                            nRet = this.GetCreateIndexString(
                                "keys,records",
                                this.container.SqlServerType,
                                "create",
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            command.CommandText = strCommand;
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "建索引出错。\r\n"
                                    + ex.Message + "\r\n"
                                        + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command

                        // 6.设库记录种子为0
                        this.ChangeTailNo(0);
                        this.m_bTailNoVerified = true;  // 2011/2/26
                        this.container.Changed = true;   //内容改变
                    }
                    finally
                    {
                        connection.Close();
                    }

#if NO
                    // 删除对象目录，然后重建
                    try
                    {
                        if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                        {
                            if (this.IsLocalDB() == true)
                            {
                                ClearDir(this.m_strObjectDir, GetDatabaseFileName());
                            }
                            else
                            {
                                PathUtil.DeleteDirectory(this.m_strObjectDir);
                                PathUtil.CreateDirIfNeed(this.m_strObjectDir);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "清除 数据库 '" + this.GetCaption("zh") + "' 的 原有对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                        return -1;
                    }
#endif
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    // Commit Transaction
                    this.CloseInternal(false);

                    // 删除对象目录，然后重建
                    try
                    {
                        if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                        {
                            _streamCache.ClearAll();
                            _pageCache.ClearAll();
                            PathUtil.DeleteDirectory(this.m_strObjectDir);

                            PathUtil.TryCreateDir(this.m_strObjectDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "清除 数据库 '" + this.GetCaption("zh") + "' 的 原有对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                        return -1;
                    }

                    SQLiteConnection connection = new SQLiteConnection(this.m_strConnString);
                    // connection.Open();
                    Open(connection);
                    try //连接
                    {
                        string strCommand = "";
                        // 2.建表
                        int nRet = this.GetCreateTablesString(
                            this.container.SqlServerType,
                            out strCommand,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        using (SQLiteCommand command = new SQLiteCommand(strCommand,
                            connection))
                        {
                            // command.CommandTimeout = 120;   // 默认 30
                            IDbTransaction trans = null;

                            trans = connection.BeginTransaction();  // 2017/9/3
                            try
                            {
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
#if NO
                                    strError = "建表出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
#endif
                                    strError = GetExceptionString(command,
"建表出错。",
ex);

                                    return -1;
                                }

                                // 3.建索引
                                nRet = this.GetCreateIndexString(
                                    "keys,records",
                                    this.container.SqlServerType,
                                    "create",
                                    out strCommand,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                command.CommandText = strCommand;
                                try
                                {
                                    // testing 
                                    // throw new Exception("模拟抛出异常");
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
#if NO
                                    strError = "建索引出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
#endif
                                    strError = GetExceptionString(command,
"建索引出错。",
ex);
                                    return -1;
                                }

                                if (trans != null)
                                {
                                    trans.Commit();
                                    trans = null;
                                }
                            }
                            finally
                            {
                                if (trans != null)
                                    trans.Rollback();
                            }
                        } // end of using command

                        // 4.设库记录种子为0
                        this.ChangeTailNo(0);
                        this.m_bTailNoVerified = true;  // 2011/2/26
                        this.container.Changed = true;   //内容改变
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlConnection connection = new MySqlConnection(this.m_strConnString);
                    // connection.Open();// TODO: TryOpen
                    Connection.TryOpen(connection, this);
                    try //连接
                    {
                        string strCommand = "";
                        // 1.建库
                        strCommand = this.GetCreateDbCmdString(this.container.SqlServerType);
                        using (MySqlCommand command = new MySqlCommand(strCommand,
                            connection))
                        {
                            // 2018/9/17
                            // 设置 command.CommandTimeout
                            command.CommandTimeout = 120;   // 默认 30。由于 MySQL 创建一个数据库较慢，所以这里专门设置为 120 秒

                            IDbTransaction trans = null;

                            // trans = connection.BeginTransaction();  // 2017/9/3
                            try
                            {

                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
#if NO
                                    strError = "建库出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
#endif
                                    strError = GetExceptionString(command,
                                        "建库出错。",
                                        ex);
                                    return -1;
                                }

                                // 2.建表
                                int nRet = this.GetCreateTablesString(
                                    this.container.SqlServerType,
                                    out strCommand,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                command.CommandText = strCommand;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
#if NO
                                    strError = "建表出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
#endif
                                    strError = GetExceptionString(command,
    "建表出错。",
    ex);

                                    return -1;
                                }

                                // 3.建索引
                                nRet = this.GetCreateIndexString(
                                    "keys,records",
                                    this.container.SqlServerType,
                                    "create",
                                    out strCommand,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                command.CommandText = strCommand;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
#if NO
                                    strError = "建索引出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strCommand;
#endif
                                    strError = GetExceptionString(command,
"建索引出错。",
ex);

                                    return -1;
                                }
                                if (trans != null)
                                {
                                    trans.Commit();
                                    trans = null;
                                }
                            }
                            finally
                            {
                                if (trans != null)
                                    trans.Rollback();
                            }
                        } // end of using command

                        // 4.设库记录种子为0
                        this.ChangeTailNo(0);
                        this.m_bTailNoVerified = true;  // 2011/2/26
                        this.container.Changed = true;   //内容改变
                    }
                    finally
                    {
                        connection.Close();
                    }

                    // 删除对象目录，然后重建
                    try
                    {
                        if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                        {
                            _streamCache.ClearAll();
                            _pageCache.ClearAll();
                            PathUtil.DeleteDirectory(this.m_strObjectDir);
                            PathUtil.TryCreateDir(this.m_strObjectDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "清除 数据库 '" + this.GetCaption("zh") + "' 的 原有对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                        return -1;
                    }
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleConnection connection = new OracleConnection(this.m_strConnString);
                    connection.Open();
                    try //连接
                    {
                        string strCommand = "";

                        using (OracleCommand command = new OracleCommand("",
    connection))
                        {

#if NO
                        // 1.建库
                        strCommand = this.GetCreateDbCmdString(this.container.SqlServerType);
                        command = new OracleCommand(strCommand,
                            connection);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            strError = "建库出错。\r\n"
                                + ex.Message + "\r\n"
                                + "SQL命令:\r\n"
                                + strCommand;
                            return -1;
                        }
#endif
                            int nRet = DropAllTables(
                                 connection,
                                 this.m_strSqlDbName,
                                 "keys,records",
                                 out strError);
                            if (nRet == -1)
                                return -1;

                            // 2.建表
                            nRet = this.GetCreateTablesString(
                                this.container.SqlServerType,
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            string[] lines = strCommand.Split(new char[] { ';' });
                            foreach (string line in lines)
                            {
                                string strLine = line.Trim();
                                if (string.IsNullOrEmpty(strLine) == true)
                                    continue;
                                command.CommandText = strLine;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "建表出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                            }

                            // 3.建索引
                            nRet = this.GetCreateIndexString(
                                "keys,records",
                                this.container.SqlServerType,
                                "create",
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            lines = strCommand.Split(new char[] { ';' });
                            foreach (string line in lines)
                            {
                                string strLine = line.Trim();
                                if (string.IsNullOrEmpty(strLine) == true)
                                    continue;
                                command.CommandText = strLine;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "建索引出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "command.CommandTimeout:" + command.CommandTimeout + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                            }

                        } // end of using command

                        // 4.设库记录种子为0
                        this.ChangeTailNo(0);
                        this.m_bTailNoVerified = true;  // 2011/2/26
                        this.container.Changed = true;   //内容改变
                    }
                    finally
                    {
                        connection.Close();
                    }

                    // 删除对象目录，然后重建
                    try
                    {
                        if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                        {
                            _streamCache.ClearAll();
                            _pageCache.ClearAll();
                            PathUtil.DeleteDirectory(this.m_strObjectDir);
                            PathUtil.TryCreateDir(this.m_strObjectDir);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "清除 数据库 '" + this.GetCaption("zh") + "' 的 原有对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                        return -1;
                    }
                }
            }
            finally
            {
                //*********************对数据库解写锁******
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }
            return 0;
        }

        // 获取一个SQL数据库中已经存在的records和keys表名
        // 注意，表名已经转换为小写或者大写形态
        // parameters:
        //      strStyle    要探测哪些表。keys和records的组合。空相当于"keys,records"
        int GetExistTableNames(
            Connection connection,
            out List<string> table_names,
            out string strError)
        {
            strError = "";

            table_names = new List<string>();

            if (connection.SqlServerType == SqlServerType.MySql)
            {
                // string strCommand = "use `" + this.m_strSqlDbName + "` ;\n";

                string strCommand = "select table_name from information_schema.tables where table_schema = '" + this.m_strSqlDbName + "'; ";

                try
                {
                    using (MySqlCommand command = new MySqlCommand(strCommand,
        connection.MySqlConnection))
                    {
                        using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            if (dr != null && dr.HasRows == true)
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        table_names.Add(dr.GetString(0).ToLower());
                                }
                            }
                        }

                    } // end of using command
                }
                catch (Exception ex)
                {
                    strError = "获得现存的表名时出错: " + ex.Message;
                    return -1;
                }
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                string strCommand = " SELECT table_name FROM user_tables WHERE table_name like '" + this.m_strSqlDbName.ToUpper() + "_%'";

                try
                {
                    using (OracleCommand command = new OracleCommand(strCommand,
        connection.OracleConnection))
                    {
                        using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            if (dr != null && dr.HasRows == true)
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        table_names.Add(dr.GetString(0).ToUpper());
                                }
                            }
                        }

                    } // end of using command
                }
                catch (Exception ex)
                {
                    strError = "获得现存的表名时出错: " + ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        // 删除一个数据库中的全部表。目前专用于Oracle版本
        // parameters:
        //      strStyle    要删除哪些表。keys和records的组合。空相当于"keys,records"
        int DropAllTables(
            OracleConnection connection,
            string strSqlDbName,
            string strStyle,
            out string strError)
        {
            strError = "";

            List<string> table_names = new List<string>();

            // 第一步，获得所有表名
            string strCommand = " SELECT table_name FROM user_tables WHERE table_name like '" + strSqlDbName.ToUpper() + "_%'";

            if (string.IsNullOrEmpty(strStyle) == true
                || (StringUtil.IsInList("keys", strStyle) == true && StringUtil.IsInList("records", strStyle) == true)
                )
            {
                // 删除全部
                strCommand = " SELECT table_name FROM user_tables WHERE table_name like '" + strSqlDbName.ToUpper() + "_%'";
            }
            else if (StringUtil.IsInList("keys", strStyle) == true)
            {
                // 只删除keys
                strCommand = " SELECT table_name FROM user_tables WHERE table_name like '" + strSqlDbName.ToUpper() + "_%' AND table_name <> '" + strSqlDbName.ToUpper() + "_RECORDS' ";
            }
            else if (StringUtil.IsInList("records", strStyle) == true)
            {
                // 只删除records
                strCommand = " SELECT table_name FROM user_tables WHERE table_name == '" + strSqlDbName.ToUpper() + "_RECORDS' ";
            }

            using (OracleCommand command = new OracleCommand(strCommand,
                connection))
            {
                using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (dr != null && dr.HasRows == true)
                    {
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0) == false)
                                table_names.Add(dr.GetString(0));
                        }
                    }
                }

                // 第二步，删除这些表
                List<string> cmd_lines = new List<string>();
                foreach (string strTableName in table_names)
                {
                    cmd_lines.Add("DROP TABLE " + strTableName + " \n");
                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    foreach (string strLine in cmd_lines)
                    {
                        command.CommandText = strLine;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            strError = "删除数据库 '" + strSqlDbName + "' 的所有表时出错：\r\n"
                                + ex.Message + "\r\n"
                                + "SQL命令:\r\n"
                                + strLine;
                            return -1;
                        }
                    }
                }
            } // end of using command

            return 0;
        }

        // 检查出错信息，是否表示了 全部都是 3701 errorcode ?
        static bool IsErrorCode3701(SqlException ex)
        {
            if (ex.Errors == null || ex.Errors.Count == 0)
                return false;
            foreach (SqlError error in ex.Errors)
            {
                if (error.Number == 5701)
                    continue;

                if (error.Number != 3701)
                    return false;
            }

            return true;    // 表示全部都是 3701 error
        }

        // 探测是否包含特定的错误码
        static bool ContainsErrorCode(SqlException ex, int nErrorCode)
        {
            if (ex.Errors == null || ex.Errors.Count == 0)
                return false;
            foreach (SqlError error in ex.Errors)
            {
                if (error.Number == nErrorCode)
                    return true;
            }

            return false;
        }

        // 管理keys表的index
        // parameters:
        //      strAction   delete/create/rebuild/disable/rebuildall/disableall
        public override int ManageKeysIndex(
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 2013/3/2
            // 必须在没有加锁的时候调用
            if (this.container.SqlServerType == SqlServerType.SQLite)
                this.Commit();

            //************对数据库加写锁********************
            m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Refresh()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            try
            {
                Connection connection = new Connection(this,
                    this.m_strConnString);
                connection.TryOpen();
                try //连接
                {
                    string strCommand = "";

                    if (strAction == "create"
                        || strAction == "rebuild"
                        || strAction == "rebuildall")
                    {
                        nRet = this.GetCreateIndexString(
                            "keys",
                            connection.SqlServerType,
                            strAction,
                            out strCommand,
                            out strError);
                    }
                    else
                    {
                        Debug.Assert(strAction == "delete"
                            || strAction == "disable"
                            || strAction == "disableall",
                            "");
                        nRet = this.GetDeleteIndexString(
                            connection.SqlServerType,
                            strAction,
                            out strCommand,
                            out strError);
                    }
                    if (nRet == -1)
                        return -1;

                    #region MS SQL Server
                    if (connection.SqlServerType == SqlServerType.MsSqlServer)
                    {
                        using (SqlCommand command = new SqlCommand(strCommand,
                            connection.SqlConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大

                                command.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                // 2013/2/20
                                if (strAction == "delete"
                                    && IsErrorCode3701(ex) == true)
                                {
                                    return 0;
                                }
                                strError = "刷新表定义 " + strAction + " 出错。\r\n"
    + ex.Message + "\r\n"
    + "SQL命令:\r\n"
    + strCommand;
                                return -1;
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    #endregion // MS SQL Server

                    #region SQLite
                    else if (connection.SqlServerType == SqlServerType.SQLite)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(strCommand,
                            connection.SQLiteConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    #endregion // SQLite

                    #region MySql
                    else if (connection.SqlServerType == SqlServerType.MySql)
                    {

                        using (MySqlCommand command = new MySqlCommand(strCommand,
                            connection.MySqlConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                command.ExecuteNonQuery();
                            }
                            catch (MySqlException ex)
                            {
                                if (strAction == "delete"
                                && ex.Number == 1091)
                                    return 0;
                                strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    #endregion // MySql

                    #region Oracle
                    else if (connection.SqlServerType == SqlServerType.Oracle)
                    {
                        using (OracleCommand command = new OracleCommand("",
                            connection.OracleConnection))
                        {
                            string[] lines = strCommand.Split(new char[] { ';' });
                            foreach (string line in lines)
                            {
                                string strLine = line.Trim();
                                if (string.IsNullOrEmpty(strLine) == true)
                                    continue;
                                try
                                {
                                    command.CommandText = strLine;
                                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                    command.ExecuteNonQuery();
                                }
                                catch (OracleException ex)
                                {
                                    if (strAction == "delete"
                                    && ex.Number == 1418)
                                        continue;
                                    strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                                catch (Exception ex)
                                {
                                    strError = "刷新表定义 " + strAction + " 出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                            }
                        } // end of using command
                    }
                    #endregion // Oracle
                }
                finally
                {
                    connection.Close();
                }
            }
            finally
            {
                //*********************对数据库解写锁******
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Refresh()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }


            return 0;
        }

        // 2008/11/14
        // 刷新相关SQL数据库的表定义，注意虚函数不能为private
        // parameters:
        //      bClearAllKeyTables 是否顺便要删除所有keys表中的数据?
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的
        // 加写锁的原因? 修改记录尾号已经去除，似乎可以不加锁？
        public override int RefreshPhysicalDatabase(
            bool bClearAllKeyTables,
            out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Refresh()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            try
            {
                Connection connection = new Connection(this,
                    this.m_strConnString);
                connection.TryOpen();
                try //连接
                {
                    string strCommand = "";

                    // 刷新表定义
                    int nRet = this.GetRefreshTablesString(
                        connection.SqlServerType,
                        bClearAllKeyTables,
                        null,
                        out strCommand,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (connection.SqlServerType == SqlServerType.MsSqlServer)
                    {
                        using (SqlCommand command = new SqlCommand(strCommand,
                            connection.SqlConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    else if (connection.SqlServerType == SqlServerType.SQLite)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(strCommand,
                            connection.SQLiteConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    else if (connection.SqlServerType == SqlServerType.MySql)
                    {
                        if (bClearAllKeyTables == false)
                        {
                            List<string> table_names = null;

                            // 获取一个SQL数据库中已经存在的records和keys表名
                            nRet = GetExistTableNames(
                                connection,
                                out table_names,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 刷新表定义
                            nRet = this.GetRefreshTablesString(
                                connection.SqlServerType,
                                bClearAllKeyTables,
                                table_names,
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        using (MySqlCommand command = new MySqlCommand(strCommand,
                            connection.MySqlConnection))
                        {
                            try
                            {
                                command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "刷新表定义出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        } // end of using command
                    }
                    else if (connection.SqlServerType == SqlServerType.Oracle)
                    {
                        if (bClearAllKeyTables == true)
                        {
                            nRet = DropAllTables(
                                connection.OracleConnection,
                                this.m_strSqlDbName,
                                "keys",
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else
                        {
                            List<string> table_names = null;

                            // 获取一个SQL数据库中已经存在的records和keys表名
                            nRet = GetExistTableNames(
                                connection,
                                out table_names,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 刷新表定义
                            nRet = this.GetRefreshTablesString(
                                connection.SqlServerType,
                                bClearAllKeyTables,
                                table_names,
                                out strCommand,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        using (OracleCommand command = new OracleCommand("",
                            connection.OracleConnection))
                        {
                            string[] lines = strCommand.Split(new char[] { ';' });
                            foreach (string line in lines)
                            {
                                string strLine = line.Trim();
                                if (string.IsNullOrEmpty(strLine) == true)
                                    continue;
                                try
                                {
                                    command.CommandText = strLine;
                                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "刷新表定义出错。\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                            }
                        } // end of using command
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            finally
            {
                //*********************对数据库解写锁******
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Refresh()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }

            return 0;
        }

        // 当前是否为 MS SQL Server LocalDB?
        bool IsLocalDB()
        {
            if (this.container.SqlServerName.ToLower().IndexOf("(localdb)") == -1)
                return false;
            return true;
        }

        string GetDatabaseFileName()
        {
            return Path.Combine(this.m_strObjectDir, "database.mdf");
        }

        // 得到建库命令字符串
        public string GetCreateDbCmdString(SqlServerType server_type)
        {
            if (server_type == SqlServerType.MsSqlServer)
            {
                string strCommand = "use master " + "\n"
                    + " if exists (select * from dbo.sysdatabases where name = N'" + this.m_strSqlDbName + "')" + "\n"
                    + " drop database " + this.m_strSqlDbName + "\n"
                    + " CREATE database " + this.m_strSqlDbName + "\n";

                // 2015/5/17
                if (this.IsLocalDB() == true)
                {
                    string strDatabaseFileName = GetDatabaseFileName();
                    strCommand += " ON ( name = '" + this.m_strSqlDbName + "', filename = '" + strDatabaseFileName + "')\n";

                    // 确保子目录已经创建
                    PathUtil.TryCreateDir(Path.GetDirectoryName(strDatabaseFileName));
                }

                // 2019/5/12
                strCommand += $" ALTER DATABASE {this.m_strSqlDbName} MODIFY FILE (NAME = N'{this.m_strSqlDbName}', FILEGROWTH = 64MB)\n"
                + $" ALTER DATABASE {this.m_strSqlDbName} SET RECOVERY SIMPLE\n";
                // + $" ALTER DATABASE {this.m_strSqlDbName} SET AUTO_SHRINK ON \n";

                strCommand += " use master " + "\n";
                return strCommand;
            }
            else if (server_type == SqlServerType.SQLite)
            {
                // 注: SQLite没有创建数据库的步骤，直接创建表就可以了
                return "";
            }
            else if (server_type == SqlServerType.MySql)
            {
                string strCommand =
                    " DROP DATABASE IF EXISTS `" + this.m_strSqlDbName + "`; \n"
                    + " CREATE DATABASE IF NOT EXISTS `" + this.m_strSqlDbName + "`;\n";
                return strCommand;
            }
            else if (server_type == SqlServerType.Oracle)
            {
                /*
                string strCommand =
                    " DROP DATABASE IF EXISTS " + this.m_strSqlDbName + "; \n"
                    + " CREATE DATABASE " + this.m_strSqlDbName + " \n"
                    + " CONTROLFILE REUSE " 
                    + " LOGFILE "
                    + " group 1 ('" + PathUtil.MergePath(this.m_strObjectDir, "redo1.log") + "') size 10M, "
                    + " group 2 ('" + PathUtil.MergePath(this.m_strObjectDir, "redo2.log") + "') size 10M,"
                    + " group 3 ('" + PathUtil.MergePath(this.m_strObjectDir, "redo3.log") + "') size 10M "
                    + " CHARACTER SET AL32UTF8"
                    + " NATIONAL CHARACTER SET AL16UTF16"
                    + " DATAFILE  "
                    + " '" + PathUtil.MergePath(this.m_strObjectDir, "database.dbf") + "' "
                    + "       size 50M"
                    + "       autoextend on "
                    + "       next 10M maxsize unlimited"
                    + "       extent management local"
                    + " DEFAULT TEMPORARY TABLESPACE temp_ts"
                    + " UNDO TABLESPACE undo_ts ; ";
                return strCommand;
                 * */
                return "";
            }

            return "";
        }

        // 得到建表命令字符串
        // return
        //		-1	出错
        //		0	成功
        private int GetCreateTablesString(
            SqlServerType strSqlServerType,
            out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            #region MS SQL Server
            if (strSqlServerType == SqlServerType.MsSqlServer)
            {
                // 创建records表
                strCommand = "use " + this.m_strSqlDbName + "\n"
                    + "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[records]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n"
                    + "drop table [dbo].[records]" + "\n"
                    + "CREATE TABLE [dbo].[records]" + "\n"
                    + "(" + "\n"
                    + "[id] [nvarchar] (255) NULL UNIQUE," + "\n"
                    + "[data] [image] NULL ," + "\n"
                    + "[newdata] [image] NULL ," + "\n"
                    + "[range] [nvarchar] (4000) NULL," + "\n"
                    + "[dptimestamp] [nvarchar] (100) NULL ," + "\n"
                    + "[newdptimestamp] [nvarchar] (100) NULL ," + "\n"   // 2012/1/19
                    + "[metadata] [nvarchar] (4000) NULL ," + "\n"
                    + "[filename] [nvarchar] (255) NULL, \n"
                    + "[newfilename] [nvarchar] (255) NULL\n"
                    + ") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]" + "\n" + "\n";
                // UNIQUE为2008/3/13新加入

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (keysCfg != null)
                {

                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;


                    // 建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        strCommand += "\n" +
                            "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + tableInfo.SqlTableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n" +
                            "drop table [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                            "CREATE TABLE [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                            "(" + "\n" +
                            "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") Null," + "\n" +         //keystring的长度由配置文件定
                            "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                            "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                            "[keystringnum] [bigint] NULL " + "\n" +
                            ")" + "\n" + "\n";
                    }
                }

                strCommand += " use master " + "\n";
                return 0;
            }
            #endregion // MS SQL Server

            #region SQLite
            else if (strSqlServerType == SqlServerType.SQLite)
            {
                // 创建records表
                strCommand = "CREATE TABLE records "
                    + "(" + " "
                    + "id nvarchar (255) NULL UNIQUE," + " "
                    + "range nvarchar (4000) NULL," + " "
                    + "dptimestamp nvarchar (100) NULL ," + " "
                    + "newdptimestamp nvarchar (100) NULL ," + " "   // 2012/1/19
                    + "metadata nvarchar (4000) NULL ," + " "
                    + "filename nvarchar (255) NULL,  "
                    + "newfilename nvarchar (255) NULL "
                    + ") ; ";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (keysCfg != null)
                {

                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;


                    // 建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        strCommand += " " +
                            "CREATE TABLE " + tableInfo.SqlTableName + " " +
                            "(" + " " +
                            "keystring nvarchar (" + Convert.ToString(this.KeySize) + ") NULL," + " " +         //keystring的长度由配置文件定
                            "fromstring nvarchar (255) NULL ," + " " +
                            "idstring nvarchar (255)  NULL ," + " " +
                            "keystringnum bigint NULL " + " " +
                            ")" + " ; ";
                    }
                }

                return 0;
            }
            #endregion // SQLite

            #region MySql
            else if (strSqlServerType == SqlServerType.MySql)
            {
                string strCharset = " CHARACTER SET utf8 "; // COLLATE utf8_bin ";

                // 创建records表
                strCommand = // "use `" + this.m_strSqlDbName + "` ;\n" +
                    "DROP TABLE IF EXISTS `" + this.m_strSqlDbName + "`.records" + " ;\n"
                    + "CREATE TABLE `" + this.m_strSqlDbName + "`.records" + " \n"
                    + "(" + "\n"
                    + "id varchar (255) " + strCharset + " NULL UNIQUE," + "\n"
                    + "`range` varchar (4000) " + strCharset + " NULL," + "\n"
                    + "dptimestamp varchar (100) " + strCharset + " NULL ," + "\n"
                    + "newdptimestamp varchar (100) " + strCharset + " NULL ," + "\n"
                    + "metadata varchar (4000) " + strCharset + " NULL ," + "\n"
                    + "filename varchar (255) " + strCharset + " NULL, \n"
                    + "newfilename varchar (255) " + strCharset + " NULL\n"
                    + ") ;\n";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        strCommand += "\n" +
                            "DROP TABLE IF EXISTS `" + this.m_strSqlDbName + "`." + tableInfo.SqlTableName + "" + " ;\n" +
                            "CREATE TABLE `" + this.m_strSqlDbName + "`." + tableInfo.SqlTableName + "\n" +
                            "(" + "\n" +
                            "keystring varchar (" + Convert.ToString(this.KeySize) + ") " + strCharset + " NULL," + "\n" +         //keystring的长度由配置文件定
                            "fromstring varchar (255) " + strCharset + " NULL ," + "\n" +
                            "idstring varchar (255) " + strCharset + " NULL ," + "\n" +
                            "keystringnum bigint NULL " + "\n" +
                            ")" + " ;\n";
                    }
                }
                return 0;
            }
            #endregion // MySql

            #region Oracle
            else if (strSqlServerType == SqlServerType.Oracle)
            {
                // 创建records表
                strCommand = "CREATE TABLE " + this.m_strSqlDbName + "_records " + "\n"
                    + "(" + "\n"
                    + "id nvarchar2 (255) NULL UNIQUE," + "\n"
                    + "range nvarchar2 (2000) NULL," + "\n"
                    + "dptimestamp nvarchar2 (100) NULL ," + "\n"
                    + "newdptimestamp nvarchar2 (100) NULL ," + "\n"
                    + "metadata nvarchar2 (2000) NULL ," + "\n"
                    + "filename nvarchar2 (255) NULL, \n"
                    + "newfilename nvarchar2 (255) NULL\n"
                    + ") \n";

                string strTemp = this.m_strSqlDbName + "_" + "_records";
                if (strTemp.Length > 30)
                {
                    strError = "表名字 '" + strTemp + "' 的字符数超过 30。请使用更短的 SQL 数据库名。";
                    return -1;
                }

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        if (string.IsNullOrEmpty(strCommand) == false)
                            strCommand += " ; ";

                        // TODO 要防止keys表名和records撞车

                        strTemp = this.m_strSqlDbName + "_" + tableInfo.SqlTableName;
                        if (strTemp.Length > 30)
                        {
                            strError = "表名字 '" + strTemp + "' 的字符数超过 30。请使用更短的 SQL 数据库名。";
                            return -1;
                        }

                        // int16 number(5)
                        // int32 number(10)
                        // int64 number(19)

                        strCommand += " CREATE TABLE " + this.m_strSqlDbName + "_" + tableInfo.SqlTableName + " " + "\n" +
                            "(" + "\n" +
                            "keystring nvarchar2 (" + Convert.ToString(this.KeySize) + ") NULL," + "\n" +
                            "fromstring nvarchar2 (255) NULL ," + "\n" +
                            "idstring nvarchar2 (255)  NULL ," + "\n" +
                            "keystringnum NUMBER(19) NULL " + "\n" +
                            ")" + " \n";
                    }
                }
                return 0;
            }
            #endregion // Oracle

            return 0;
        }

        // 得到刷新表定义命令字符串
        // 根据最新的keys定义，增补创建那些没有被创建的SQL表
        // 注：已经包含了创建SQL索引的语句
        // parameters:
        //      bClearAllKeyTables 是否顺便要删除所有keys表中的数据?
        // return
        //		-1	出错
        //		0	成功
        private int GetRefreshTablesString(
            SqlServerType server_type,
            bool bClearAllKeyTables,
            List<string> existing_tablenames,
            out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            KeysCfg keysCfg = null;
            int nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;

            if (server_type == SqlServerType.MsSqlServer)
            {

                strCommand = "use " + this.m_strSqlDbName + "\n";

                if (keysCfg != null)
                {

                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 增补创建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        if (bClearAllKeyTables == true)
                        {
                            // 如果表已经存在，就先drop再创建
                            strCommand += "\n" +
                                "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + tableInfo.SqlTableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n" +
                                "DROP TABLE [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                                "\n" +
                                "CREATE TABLE [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                                "(" + "\n" +
                                "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") Null," + "\n" +         //keystring的长度由配置文件定
                                "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                                "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                                "[keystringnum] [bigint] NULL " + "\n" +
                                ")" + "\n" + "\n";

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " \n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " \n";
                            // 2008/11/20 
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) \n";
                        }
                        else
                        {
                            // 表不存在才创建
                            strCommand += "\n" +
                                "if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + tableInfo.SqlTableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n" +
                                "BEGIN\n" +
                                "CREATE TABLE [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                                "(" + "\n" +
                                "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") Null," + "\n" +         //keystring的长度由配置文件定
                                "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                                "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                                "[keystringnum] [bigint] NULL " + "\n" +
                                ")" + "\n" + "\n";

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " \n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " \n";
                            // 2008/11/20 
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) \n";
                            strCommand += "END\n";
                        }
                    }
                }

                strCommand += " use master " + "\n";

                return 0;
            }
            else if (server_type == SqlServerType.SQLite)
            {
                strCommand = "";

                if (keysCfg != null)
                {

                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 增补创建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        if (bClearAllKeyTables == true)
                        {
                            // 如果表已经存在，就先drop再创建
                            strCommand += "DROP TABLE if exists " + tableInfo.SqlTableName + " ;\n"
                                + "CREATE TABLE " + tableInfo.SqlTableName + " \n" +
                                "(" + "\n" +
                                "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") NULL," + "\n" +         //keystring的长度由配置文件定
                                "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                                "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                                "[keystringnum] [bigint] NULL " + "\n" +
                                ")" + " ;\n";

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) ;\n";
                        }
                        else
                        {
                            // 表不存在才创建
                            strCommand +=
                                "CREATE TABLE if not exists " + tableInfo.SqlTableName + " \n" +
                                "(" + "\n" +
                                "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") NULL," + "\n" +         //keystring的长度由配置文件定
                                "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                                "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                                "[keystringnum] [bigint] NULL " + "\n" +
                                ")" + " ;\n";

                            strCommand += " CREATE INDEX if not exists " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX if not exists " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            // 2008/11/20 
                            strCommand += " CREATE INDEX if not exists " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) ;\n";
                        }
                    }
                }

                return 0;
            }
            else if (server_type == SqlServerType.MySql)
            {
                strCommand = "use `" + this.m_strSqlDbName + "` ;\n";
                string strCharset = " CHARACTER SET utf8 "; // COLLATE utf8_bin ";

                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 增补创建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        if (bClearAllKeyTables == true)
                        {
                            // 如果表已经存在，就先drop再创建
                            strCommand +=
                                "DROP TABLE if exists `" + tableInfo.SqlTableName + "` ;\n"
                                + "CREATE TABLE `" + tableInfo.SqlTableName + "` \n" +
                                "(" + "\n" +
                                "keystring varchar (" + Convert.ToString(this.KeySize) + ") " + strCharset + " NULL," + "\n" +         //keystring的长度由配置文件定
                                "fromstring varchar (255) " + strCharset + " NULL ," + "\n" +
                                "idstring varchar (255) " + strCharset + " NULL ," + "\n" +
                                "keystringnum bigint NULL " + "\n" +
                                ")" + " ;\n";

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) ;\n";
                        }
                        else
                        {
                            if (existing_tablenames != null
                                && existing_tablenames.IndexOf(tableInfo.SqlTableName.ToLower()) != -1)
                                continue;

                            // 表不存在才创建
                            strCommand +=
                                "CREATE TABLE if not exists `" + tableInfo.SqlTableName + "` \n" +
                                "(" + "\n" +
                                "keystring varchar (" + Convert.ToString(this.KeySize) + ") " + strCharset + " NULL," + "\n" +         //keystring的长度由配置文件定
                                "fromstring varchar (255) " + strCharset + " NULL ," + "\n" +
                                "idstring varchar (255) " + strCharset + " NULL ," + "\n" +
                                "keystringnum bigint NULL " + "\n" +
                                ")" + " ;\n";

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) ;\n";
                        }
                    }
                }

                return 0;
            }
            else if (server_type == SqlServerType.Oracle)
            {
                strCommand = "";    //  "use `" + this.m_strSqlDbName + "` ;\n";

                // bClearAllKeyTables==true，需要通过在调用本函数前删除全部表来实现

                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 增补创建检索点表
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];
                        string strTableName = (this.m_strSqlDbName + "_" + tableInfo.SqlTableName).ToUpper();

                        if (existing_tablenames != null
    && existing_tablenames.IndexOf(strTableName) != -1)
                            continue;

                        strCommand += // "IF NOT EXISTS ( SELECT table_name FROM user_tables WHERE table_name = '" + strTableName + "' ) " + 
                            "CREATE TABLE " + strTableName + " \n" +
                            "(" + "\n" +
                            "keystring nvarchar2 (" + Convert.ToString(this.KeySize) + ") NULL," + "\n" +         //keystring的长度由配置文件定
                            "fromstring nvarchar2 (255) NULL ," + "\n" +
                            "idstring nvarchar2 (255)  NULL ," + "\n" +
                            "keystringnum NUMBER(19) NULL " + "\n" +
                            ")" + " ;\n";

                        string strTemp = strTableName + "ki";
                        if (strTemp.Length > 30)
                        {
                            strError = "索引名字 '" + strTemp + "' 的字符数超过 30。请使用更短的 SQL 数据库名。";
                            return -1;
                        }

                        strCommand += " CREATE INDEX " + strTableName + "ki \n"
                            + " ON " + strTableName + " " + KEY_COL_LIST + " ;\n";
                        strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "ni \n"
                            + " ON " + strTableName + " " + KEYNUM_COL_LIST + " ;\n";
                        strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "ii \n"
                            + " ON " + strTableName + " (idstring) ;\n";
                    }
                }

                return 0;
            }

            return 0;
        }

        // 建索引命令字符串
        // parameters:
        //      strIndexTYpeList    keys,records
        //                          key表示创建 keys 表的索引, records 表示创建 records 表的索引
        //      strAction   create / rebuild / rebuildall
        // return
        //		-1	出错
        //		0	成功
        public int GetCreateIndexString(
            string strIndexTypeList,
            SqlServerType strSqlServerType,
            string strAction,
            out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            if (string.IsNullOrEmpty(strIndexTypeList) == true)
                strIndexTypeList = "keys,records";

            if (string.IsNullOrEmpty(strAction) == true)
                strAction = "create";

            #region MS SQL Server
            if (strSqlServerType == SqlServerType.MsSqlServer)
            {
                strCommand = "use " + this.m_strSqlDbName + "\n";
                if (StringUtil.IsInList("records", strIndexTypeList) == true)
                {
                    if (strAction == "create")
                    {
                        strCommand += " CREATE INDEX records_id_index " + "\n"
                            + " ON records (id) \n";
                    }
                    else if (strAction == "rebuild")
                    {
                        strCommand += " ALTER INDEX records_id_index " + "\n"
                            + " ON records REBUILD \n";
                    }
                    else if (strAction == "rebuildall")
                    {
                        strCommand += " ALTER INDEX ALL " + "\n"
                            + " ON records REBUILD \n";
                    }
                }

                if (StringUtil.IsInList("keys", strIndexTypeList) == true)
                {
                    KeysCfg keysCfg = null;
                    int nRet = this.GetKeysCfg(out keysCfg,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keysCfg != null)
                    {
                        List<TableInfo> aTableInfo = null;
                        nRet = keysCfg.GetTableInfosRemoveDup(
                            out aTableInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (strAction == "create")
                        {
                            for (int i = 0; i < aTableInfo.Count; i++)
                            {
                                TableInfo tableInfo = (TableInfo)aTableInfo[i];

                                strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                    + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " \n";
                                strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                    + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " \n";
                                // 2008/11/20 
                                strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                    + " ON " + tableInfo.SqlTableName + " (idstring) \n";
                            }
                        }
                        else if (strAction == "rebuild")
                        {
                            for (int i = 0; i < aTableInfo.Count; i++)
                            {
                                TableInfo tableInfo = (TableInfo)aTableInfo[i];

                                strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                    + " ON " + tableInfo.SqlTableName + " REBUILD \n";
                                strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                    + " ON " + tableInfo.SqlTableName + " REBUILD \n";
                                strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                    + " ON " + tableInfo.SqlTableName + " REBUILD \n";
                            }
                        }
                        else if (strAction == "rebuildall")
                        {
                            for (int i = 0; i < aTableInfo.Count; i++)
                            {
                                TableInfo tableInfo = (TableInfo)aTableInfo[i];

                                strCommand += " ALTER INDEX ALL \n"
                                    + " ON " + tableInfo.SqlTableName + " REBUILD \n";
                            }
                        }
                    }
                }

                strCommand += " use master " + "\n";
            }
            #endregion MS SQL Server

            #region SQLite
            else if (strSqlServerType == SqlServerType.SQLite)
            {
                if (StringUtil.IsInList("records", strIndexTypeList) == true)
                {
                    strCommand = "CREATE INDEX records_id_index " + "\n"
                        + " ON records (id) ;\n";
                }

                if (StringUtil.IsInList("keys", strIndexTypeList) == true)
                {
                    KeysCfg keysCfg = null;
                    int nRet = this.GetKeysCfg(out keysCfg,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keysCfg != null)
                    {
                        List<TableInfo> aTableInfo = null;
                        nRet = keysCfg.GetTableInfosRemoveDup(
                            out aTableInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];

                            strCommand += " CREATE INDEX IF NOT EXISTS " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX IF NOT EXISTS " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX IF NOT EXISTS " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) ;\n";
                        }
                    }
                }
            }
            #endregion // SQLite

            #region MySql
            else if (strSqlServerType == SqlServerType.MySql)
            {
                // https://stackoverflow.com/questions/28329134/drop-index-query-is-slow
                string strAlgorithm = "";   // " ALGORITHM=INPLACE ";

                strCommand = "use " + this.m_strSqlDbName + " ;\n";
                if (StringUtil.IsInList("records", strIndexTypeList) == true)
                {
                    strCommand += " CREATE INDEX records_id_index " + "\n"
    + " ON records (id) " + strAlgorithm + ";\n";
                }

                if (StringUtil.IsInList("keys", strIndexTypeList) == true)
                {
                    KeysCfg keysCfg = null;
                    int nRet = this.GetKeysCfg(out keysCfg,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keysCfg != null)
                    {
                        List<TableInfo> aTableInfo = null;
                        nRet = keysCfg.GetTableInfosRemoveDup(
                            out aTableInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];

                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEY_COL_LIST + " " + strAlgorithm + ";\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " " + KEYNUM_COL_LIST + " " + strAlgorithm + ";\n";
                            strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " (idstring) " + strAlgorithm + ";\n";
                        }
                    }
                }

            }
            #endregion // MySql

            #region Oracle
            else if (strSqlServerType == SqlServerType.Oracle)
            {
                /*
                strCommand = " CREATE INDEX " + this.m_strSqlDbName + "_records_ii " + "\n"
                    + " ON "+this.m_strSqlDbName+"_records (id) \n";
                 * */
                // records表的id列已经有索引了，因为它是UNIQUE
                strCommand = "";

                if (StringUtil.IsInList("keys", strIndexTypeList) == true)
                {
                    KeysCfg keysCfg = null;
                    int nRet = this.GetKeysCfg(out keysCfg,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keysCfg != null)
                    {
                        List<TableInfo> aTableInfo = null;
                        nRet = keysCfg.GetTableInfosRemoveDup(
                            out aTableInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];
                            string strTableName = (this.m_strSqlDbName + "_" + tableInfo.SqlTableName).ToUpper();

                            //if (string.IsNullOrEmpty(strCommand) == false)
                            //    strCommand += " ; ";

                            string strTemp = strTableName + "ki";
                            if (strTemp.Length > 30)
                            {
                                strError = "索引名字 '" + strTemp + "' 的字符数超过 30。请使用更短的 SQL 数据库名。";
                                return -1;
                            }

                            strCommand += " CREATE INDEX " + strTableName + "ki \n"
                                + " ON " + strTableName + " " + KEY_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + strTableName + "ni \n"
                                + " ON " + strTableName + " " + KEYNUM_COL_LIST + " ;\n";
                            strCommand += " CREATE INDEX " + strTableName + "ii \n"
                                + " ON " + strTableName + " (idstring) ;\n";
                        }
                    }
                }
            }
            #endregion // Oracle

            return 0;
        }

        // 删除keys索引的命令字符串
        // parameters:
        //      strAction   delete / disable / disableall
        // return
        //		-1	出错
        //		0	成功
        public int GetDeleteIndexString(
            SqlServerType strSqlServerType,
            string strAction,
            out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            if (string.IsNullOrEmpty(strAction) == true)
                strAction = "delete";

            #region MS SQL Server
            if (strSqlServerType == SqlServerType.MsSqlServer)
            {
                strCommand = "use " + this.m_strSqlDbName + "\n";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (strAction == "delete")
                    {
                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];

                            strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " \n";
                            strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " \n";
                            strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " \n";
                        }
                    }
                    else if (strAction == "disable")
                    {
                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];

                            strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                                + " ON " + tableInfo.SqlTableName + " DISABLE \n";
                            strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                                + " ON " + tableInfo.SqlTableName + " DISABLE \n";
                            strCommand += " ALTER INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                                + " ON " + tableInfo.SqlTableName + " DISABLE \n";
                        }
                    }
                    else if (strAction == "disableall")
                    {
                        for (int i = 0; i < aTableInfo.Count; i++)
                        {
                            TableInfo tableInfo = (TableInfo)aTableInfo[i];

                            strCommand += " ALTER INDEX ALL \n"
                                + " ON " + tableInfo.SqlTableName + " DISABLE \n";
                        }
                    }
                }

                strCommand += " use master " + "\n";
            }
            #endregion // MS SQL Server

            #region SQLite
            else if (strSqlServerType == SqlServerType.SQLite)
            {
                strCommand = "";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = (TableInfo)aTableInfo[i];

                        strCommand += " DROP INDEX IF EXISTS " + tableInfo.SqlTableName + "_keystring_index ;\n";
                        strCommand += " DROP INDEX IF EXISTS " + tableInfo.SqlTableName + "_keystringnum_index ;\n";
                        strCommand += " DROP INDEX IF EXISTS " + tableInfo.SqlTableName + "_idstring_index ;\n";
                    }
                }
            }
            #endregion // SQLite

            #region MySql
            else if (strSqlServerType == SqlServerType.MySql)
            {
                strCommand = "use " + this.m_strSqlDbName + " ;\n";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = (TableInfo)aTableInfo[i];

                        strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                            + " ON " + tableInfo.SqlTableName + " ;\n";
                        strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                            + " ON " + tableInfo.SqlTableName + " ;\n";
                        strCommand += " DROP INDEX " + tableInfo.SqlTableName + "_idstring_index \n"
                            + " ON " + tableInfo.SqlTableName + " ;\n";
                    }
                }
            }
            #endregion // MySql

            #region Oracle
            else if (strSqlServerType == SqlServerType.Oracle)
            {
                strCommand = "";

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (keysCfg != null)
                {
                    List<TableInfo> aTableInfo = null;
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = (TableInfo)aTableInfo[i];
                        string strTableName = (this.m_strSqlDbName + "_" + tableInfo.SqlTableName).ToUpper();

                        //if (string.IsNullOrEmpty(strCommand) == false)
                        //    strCommand += " ; ";

                        string strTemp = strTableName + "ki";
                        if (strTemp.Length > 30)
                        {
                            strError = "索引名字 '" + strTemp + "' 的字符数超过 30。请使用更短的 SQL 数据库名。";
                            return -1;
                        }

                        strCommand += " DROP INDEX " + strTableName + "ki ;\n";
                        strCommand += " DROP INDEX " + strTableName + "ni ;\n";
                        strCommand += " DROP INDEX " + strTableName + "ii ;\n";
                    }
                }
            }
            #endregion

            return 0;
        }

        // 删除数据库
        // return:
        //      -1  出错
        //      0   成功
        public override int Delete(out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            this.m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            try //锁
            {
                string strCommand = "";

                Connection connection = new Connection(this,
                    this.m_strConnString);
                connection.TryOpen();
                try //连接
                {
                    if (connection.SqlServerType == SqlServerType.MsSqlServer)
                    {
                        // 1.删库的sql数据库
                        strCommand = "use master " + "\n"
                            + " if exists (select * from dbo.sysdatabases where name = N'" + this.m_strSqlDbName + "')" + "\n"
                            + " drop database " + this.m_strSqlDbName + "\n";
                        strCommand += " use master " + "\n";
                        using (SqlCommand command = new SqlCommand(strCommand,
                            connection.SqlConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (connection.SqlServerType == SqlServerType.SQLite)
                    {
                        // SQLite没有DROP TABLE语句，直接删除数据库文件即可
                    }
                    else if (connection.SqlServerType == SqlServerType.MySql)
                    {
                        // 1.删库的sql数据库
                        strCommand = " DROP DATABASE IF EXISTS `" + this.m_strSqlDbName + "` \n";
                        using (MySqlCommand command = new MySqlCommand(strCommand,
                            connection.MySqlConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (connection.SqlServerType == SqlServerType.Oracle)
                    {
                        // 删除全部表
                        int nRet = DropAllTables(
                             connection.OracleConnection,
                             this.m_strSqlDbName,
                             "keys,records",
                             out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
                catch (SqlException sqlEx)
                {
                    // 如果不存在物理数据库，则不报错

                    if (!(sqlEx.Errors is SqlErrorCollection))
                    {
                        strError = "删除sql库出错。\r\n"
                           + sqlEx.Message + "\r\n"
                           + "SQL命令:\r\n"
                           + strCommand;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "删除 SQL 库出错。\r\n"
                        + ex.Message + "\r\n"
                        + "SQL命令:\r\n"
                        + strCommand;
                    return -1;
                }
                finally  //连接
                {
                    connection.Close();
                }

                // 删除配置目录
                string strCfgsDir = DatabaseUtil.GetLocalDir(this.container.NodeDbs,
                    this.m_selfNode);
                if (strCfgsDir != "")
                {
                    // 应对目录查重，如果有其它库使用这个目录，则不能删除，返回信息
                    if (this.container.IsExistCfgsDir(strCfgsDir, this) == true)
                    {
                        // 给错误日志写一条信息
                        this.container.KernelApplication.WriteErrorLog("发现除了 '" + this.GetCaption("zh-CN") + "' 库使用 '" + strCfgsDir + "' 目录外，还有其它库的使用这个目录，所以不能在删除库时删除目录");
                    }
                    else
                    {
                        string strRealDir = this.container.DataDir + "\\" + strCfgsDir;
                        if (Directory.Exists(strRealDir) == true)
                        {
                            _streamCache.ClearAll();
                            _pageCache.ClearAll();
                            PathUtil.DeleteDirectory(strRealDir);
                        }
                    }
                }

                if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    // Commit Transaction
                    this.CloseInternal(false);
                }

                // 删除对象目录
                try
                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == false)
                    {
                        _streamCache.ClearAll();
                        _pageCache.ClearAll();
                        PathUtil.DeleteDirectory(this.m_strObjectDir);
                    }
                }
                catch (Exception ex)
                {
                    strError = "删除数据库 '" + this.GetCaption("zh") + "' 的对象目录 '" + this.m_strObjectDir + "' 时发生错误： " + ex.Message;
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "删除'" + this.GetCaption("zh") + "'数据库出错，原因:" + ex.Message;
                return -1;
            }
            finally
            {

                //*********************对数据库解写锁**********
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }
        }

        // 按ID检索记录
        // parameter:
        //		searchItem  SearchItem对象，包括检索信息 searchItem.IdOrder决定输出的顺序
        //		isConnected 连接对象的delegate
        //		resultSet   结果集对象,存放命中记录
        // return:
        //		-1  出错
        //		0   成功
        // 线：不安全
        private int SearchByID(SearchItem searchItem,
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
            DpResultSet resultSet,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

            Debug.Assert(searchItem != null, "SearchByID()调用错误，searchItem参数值不能为null。");
            // Debug.Assert(isConnected != null, "SearchByID()调用错误，isConnected参数值不能为null。");
            Debug.Assert(handle != null, "SearchByID()调用错误，handle参数值不能为null。");
            Debug.Assert(resultSet != null, "SearchByID()调用错误，resultSet参数值不能为null。");

            Debug.Assert(this.container != null, "");

            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            SqlServerType type = this.container.SqlServerType;

            string strPattern = "N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'";
            if (type == SqlServerType.MsSqlServer)
                strPattern = "N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'";
            else if (type == SqlServerType.SQLite)
                strPattern = "'__________'";
            else if (type == SqlServerType.MySql)
                strPattern = "'__________'";
            else if (type == SqlServerType.Oracle)
                strPattern = "'__________'";
            else
                throw new Exception("未知的 SqlServerType");

            List<DbParameter> aSqlParameter = new List<DbParameter>();
            string strWhere = "";
            if (searchItem.Match == "left"
                || searchItem.Match == "")
            {
                strWhere = " WHERE id LIKE @id and id like " + strPattern + " ";
                if (type == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter("@id", DbType.String);
                    temp.Value = searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter("@id", MySqlDbType.String);
                    temp.Value = searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.Oracle)
                {
                    strWhere = strWhere.Replace("@", ":");
                    OracleParameter temp = new OracleParameter(":id", OracleDbType.NVarchar2);
                    temp.Value = searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
            }
            else if (searchItem.Match == "middle")
            {
                strWhere = " WHERE id LIKE @id and id like " + strPattern + " ";
                if (type == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = "%" + searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter("@id", DbType.String);
                    temp.Value = "%" + searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter("@id", MySqlDbType.String);
                    temp.Value = "%" + searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.Oracle)
                {
                    strWhere = strWhere.Replace("@", ":");
                    OracleParameter temp = new OracleParameter(":id", OracleDbType.NVarchar2);
                    temp.Value = "%" + searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }

            }
            else if (searchItem.Match == "right")
            {
                strWhere = " WHERE id LIKE @id and id like " + strPattern + " ";
                if (type == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = "%" + searchItem.Word;
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter("@id", DbType.String);
                    temp.Value = "%" + searchItem.Word;
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter("@id", MySqlDbType.String);
                    temp.Value = "%" + searchItem.Word;
                    aSqlParameter.Add(temp);
                }
                else if (type == SqlServerType.Oracle)
                {
                    strWhere = strWhere.Replace("@", ":");
                    OracleParameter temp = new OracleParameter(":id", OracleDbType.NVarchar2);
                    temp.Value = "%" + searchItem.Word;
                    aSqlParameter.Add(temp);
                }
            }
            else if (searchItem.Match == "exact")
            {
                if (searchItem.DataType == "string")
                    searchItem.Word = DbPath.GetID10(searchItem.Word);

                if (searchItem.Relation == "draw"
                || searchItem.Relation == "range")
                {
                    bool bRet = StringUtil.SplitRangeEx(searchItem.Word,
                        out string strStartID,
                        out string strEndID);

                    if (bRet == true)
                    {
                        strStartID = DbPath.GetID10(strStartID);
                        strEndID = DbPath.GetID10(strEndID);

                        strWhere = " WHERE @idMin <=id and id<= @idMax and id like " + strPattern + " ";

                        if (type == SqlServerType.MsSqlServer)
                        {
                            SqlParameter temp = new SqlParameter("@idMin", SqlDbType.NVarChar);
                            temp.Value = strStartID;
                            aSqlParameter.Add(temp);

                            temp = new SqlParameter("@idMax", SqlDbType.NVarChar);
                            temp.Value = strEndID;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.SQLite)
                        {
                            SQLiteParameter temp = new SQLiteParameter("@idMin", DbType.String);
                            temp.Value = strStartID;
                            aSqlParameter.Add(temp);

                            temp = new SQLiteParameter("@idMax", DbType.String);
                            temp.Value = strEndID;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.MySql)
                        {
                            MySqlParameter temp = new MySqlParameter("@idMin", MySqlDbType.String);
                            temp.Value = strStartID;
                            aSqlParameter.Add(temp);

                            temp = new MySqlParameter("@idMax", MySqlDbType.String);
                            temp.Value = strEndID;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.Oracle)
                        {
                            strWhere = strWhere.Replace("@", ":");

                            OracleParameter temp = new OracleParameter(":idMin", OracleDbType.NVarchar2);
                            temp.Value = strStartID;
                            aSqlParameter.Add(temp);

                            temp = new OracleParameter(":idMax", OracleDbType.NVarchar2);
                            temp.Value = strEndID;
                            aSqlParameter.Add(temp);
                        }
                    }
                    else
                    {
                        StringUtil.GetPartCondition(searchItem.Word,
                            out string strOperator,
                            out string strRealText);

                        strRealText = DbPath.GetID10(strRealText);
                        strWhere = " WHERE id " + strOperator + " @id and id like " + strPattern + " ";

                        if (type == SqlServerType.MsSqlServer)
                        {
                            SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.SQLite)
                        {
                            SQLiteParameter temp = new SQLiteParameter("@id", DbType.String);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.MySql)
                        {
                            MySqlParameter temp = new MySqlParameter("@id", MySqlDbType.String);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                        else if (type == SqlServerType.Oracle)
                        {
                            strWhere = strWhere.Replace("@", ":");

                            OracleParameter temp = new OracleParameter(":id", OracleDbType.NVarchar2);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                    }
                }
                else
                {
                    searchItem.Word = DbPath.GetID10(searchItem.Word);
                    strWhere = " WHERE id " + searchItem.Relation + " @id and id like " + strPattern + " ";

                    if (type == SqlServerType.MsSqlServer)
                    {
                        SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                        temp.Value = searchItem.Word;
                        aSqlParameter.Add(temp);
                    }
                    else if (type == SqlServerType.SQLite)
                    {
                        SQLiteParameter temp = new SQLiteParameter("@id", DbType.String);
                        temp.Value = searchItem.Word;
                        aSqlParameter.Add(temp);
                    }
                    else if (type == SqlServerType.MySql)
                    {
                        MySqlParameter temp = new MySqlParameter("@id", MySqlDbType.String);
                        temp.Value = searchItem.Word;
                        aSqlParameter.Add(temp);
                    }
                    else if (type == SqlServerType.Oracle)
                    {
                        strWhere = strWhere.Replace("@", ":");

                        OracleParameter temp = new OracleParameter(":id", OracleDbType.NVarchar2);
                        temp.Value = searchItem.Word;
                        aSqlParameter.Add(temp);
                    }
                }
            }

            string strTop = "";
            string strLimit = "";
            if (searchItem.MaxCount != -1)  // 只命中指定的条数
            {
                if (type == SqlServerType.MsSqlServer)
                    strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                else if (type == SqlServerType.SQLite)
                    strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                else if (type == SqlServerType.MySql)
                    strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                else if (type == SqlServerType.Oracle)
                    strLimit = " WHERE rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                else
                    throw new Exception("未知的 SqlServerType");
            }

            string strOrderBy = "";

            // Oracle下迫使使用顺序
            if (type == SqlServerType.Oracle)
            {
                if (string.IsNullOrEmpty(searchItem.IdOrder) == true)
                {
                    searchItem.IdOrder = "ASC";
                }
            }

            if (searchItem.IdOrder != "")
            {
                strOrderBy = "ORDER BY id " + searchItem.IdOrder + " ";

                // 2010/5/10
                string strTemp = searchItem.IdOrder.ToLower();
                if (strTemp.IndexOf("desc") != -1)
                    resultSet.Asc = -1;
            }

            string strCommand = "";
            if (type == SqlServerType.MsSqlServer)
                strCommand = "use " + this.m_strSqlDbName;
            else if (type == SqlServerType.MySql)
                strCommand = "use `" + this.m_strSqlDbName + "` ;\n";

            strCommand += " SELECT "
        + " DISTINCT "
        + strTop
        + (bOutputKeyID == false ? " id " : " id AS keystring, id, 'recid' AS fromstring ")
        + " FROM records "
        + strWhere
        + " " + strOrderBy
        + " " + strLimit + "\n";

            if (type == SqlServerType.MsSqlServer)
                strCommand += " use master " + "\n";

            // Oracle的语句非常特殊
            if (type == SqlServerType.Oracle)
            {
                // TODO 如果没有 order by 子句， rownum还可以简化
                if (string.IsNullOrEmpty(strLimit) == false)
                    strCommand = "SELECT * from ( SELECT "
+ " DISTINCT "
+ (bOutputKeyID == false ? " id " : " id keystring, id, 'recid' fromstring ")
+ " FROM " + this.m_strSqlDbName + "_records "
+ strWhere
+ " " + strOrderBy
+ ") " + strLimit + "\n";
                else
                    strCommand = "SELECT "
+ " DISTINCT "
+ (bOutputKeyID == false ? " id " : " id keystring, id, 'recid' fromstring ")
+ " FROM " + this.m_strSqlDbName + "_records "
+ strWhere
+ " " + strOrderBy
+ "\n";

            }

            int nRet = ExecuteQueryFillResultSet(
handle,
strCommand,
aSqlParameter,
resultSet,
searchItem.MaxCount,
GetOutputStyle(strOutputStyle),
true,
out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;
#if NO
            // SQLite采用保守连接
            Connection connection = new Connection(this,
                this.m_strConnString);
            connection.TryOpen();
            try
            {

                DbCommand command = null;

                if (connection.SqlServerType == SqlServerType.MsSqlServer)
                {
                    command = new SqlCommand(strCommand,
                        connection.SqlConnection);
                }
                else if (connection.SqlServerType == SqlServerType.SQLite)
                {
                    // strCommand = "SELECT id FROM records WHERE id LIKE '__________' ";
                    command = new SQLiteCommand(strCommand,
                        connection.SQLiteConnection);
                }
                else if (connection.SqlServerType == SqlServerType.MySql)
                {
                    // strCommand = "SELECT id FROM records WHERE id LIKE '__________' ";
                    command = new MySqlCommand(strCommand,
                        connection.MySqlConnection);
                }
                else if (connection.SqlServerType == SqlServerType.Oracle)
                {
                    // strCommand = "SELECT id FROM records WHERE id LIKE '__________' ";
                    command = new OracleCommand(strCommand,
                        connection.OracleConnection);
                    ((OracleCommand)command).BindByName = true;
                }
                else
                    throw new ArgumentException("未知的 connection.SqlServerType '" + connection.SqlServerType.ToString() + "'");

                // ****
                using (command)
                {
                    command.CommandTimeout = 20 * 60;  // 把检索时间变大
                    foreach (DbParameter sqlParameter in aSqlParameter)
                    {
                        command.Parameters.Add(sqlParameter);
                    }

                    var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
handle.CancelTokenSource.Token).Result;


                    // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                    // return:
                    //      -1  出错
                    //      0   没有填入任何记录
                    //      >0  实际填入的记录条数
                    int nRet = FillResultSet(
                            handle,
                            reader,
                            resultSet,
                            searchItem.MaxCount,
                            GetOutputStyle(strOutputStyle),
                            true,
                            out strError);
                    if (nRet == -1 || nRet == 0)
                        return nRet;
                } // end of using command
            }
            catch (SqlException sqlEx)
            {
                strError = SqlDatabase.GetSqlErrors(sqlEx);

                /*
                if (sqlEx.Errors is SqlErrorCollection)
                    strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                else
                    strError = sqlEx.Message;
                 * */
                return -1;
            }
            catch (Exception ex)
            {
                strError = "SearchByID() exception: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally // 连接
            {
                if (connection != null)
                    connection.Close();
            }
#endif
            return 0;
        }

        internal enum OutputStyle
        {
            KeyCount = 1,
            KeyID = 2,
            ID = 3,
        }

        int ExecuteQueryFillResultSet(
    ChannelHandle handle,
    string strCommand,
    List<DbParameter> aSqlParameter,
    DpResultSet resultSet,
    int nMaxCount,
    OutputStyle style,
    bool bRecordTable,
    out string strError)
        {
            strError = "";

            Connection connection = null;

#if NO
            SQLiteConnection lite_connection = null;
            if (this.container.SqlServerType == SqlServerType.SQLite)
            {
                // SQLite 采用保守连接
                lite_connection =
                    new SQLiteConnection(this.m_strConnString/*Pooling*/);
                // connection.Open();
                Open(lite_connection);
            }
            else
#endif
            {
                // 注意: SQLite 这里的连接字符串是和具体数据库关联的
                connection = new Connection(this,
    this.m_strConnString);
                connection.TryOpen();
            }
            try
            {
                DbCommand command = null;

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    command = new SqlCommand(strCommand,
                    connection.SqlConnection);
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection
                    //lite_connection
                    );
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    command = new MySqlCommand(strCommand,
                    connection.MySqlConnection);
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    command = new OracleCommand(strCommand,
                         connection.OracleConnection);
                    ((OracleCommand)command).BindByName = true;
                }
                else
                    throw new ArgumentException("未知的 connection.SqlServerType '" + connection.SqlServerType.ToString() + "'");

                // ****
                using (command)
                {
                    foreach (DbParameter sqlParameter in aSqlParameter)
                    {
                        command.Parameters.Add(sqlParameter);
                    }
                    command.CommandTimeout = 20 * 60;  // 把检索时间变大

#if NO
                    DbDataReader reader = null;
                    if (handle != null && handle.CancelTokenSource != null)
                        reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection
    , handle.CancelTokenSource.Token
    ).Result;
                    else
                        reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection
).Result;
#endif

                    // 尝试一下不用 CancellationToken。因为怀疑这样用会导致触发 Cancel 的时候让 MySQL Driver 代码死锁
                    // 2019/9/8 加上的 using
                    using (DbDataReader reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection).Result)
                    {

                        // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                        // return:
                        //      -1  出错
                        //      0   没有填入任何记录
                        //      >0  实际填入的记录条数
                        int nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                nMaxCount,
                                style,  // GetOutputStyle(strOutputStyle),
                                bRecordTable,
                                out strError);
                        if (nRet == -1 || nRet == 0)
                            return nRet;
                    }
                } // end of using command

                return 0;
            }
            catch (SqlException sqlEx)
            {
                strError = GetSqlErrors(sqlEx);
                return -1;
            }
            catch (Exception ex)
            {
                // 注意这里可能捕获到 AggregationException，所以要用 ExceptionUtil.GetExceptionText() 来输出异常信息
                strError = "ExecuteQueryFillResultSet() exception: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally // 连接
            {
                if (connection != null)
                    connection.Close();
                //if (lite_connection != null)
                //    lite_connection.Close();
            }
        }

        // 从 DbDataReader 中获取和填入记录到一个结果集对象中
        // parameters:
        //      bRecordTable    是否为从 record 表中 select 出来的结果？(否则就是 keys 表)
        // return:
        //      -1  出错
        //      0   没有填入任何记录
        //      >0  实际填入的记录条数
        int FillResultSet(
        ChannelHandle handle,
        DbDataReader reader,
        DpResultSet resultSet,
        int nMaxCount,
        OutputStyle style,
        bool bRecordTable,
        out string strError)
        {
            strError = "";

            try
            {
                if (reader == null
                    || reader.HasRows == false)
                    return 0;

                CancellationToken token;
                /*
                if (handle != null && handle.CancelTokenSource != null)
                    token = handle.CancelTokenSource.Token;
                    */
                if (handle != null)
                    token = handle.CancelToken;

                int nGetedCount = 0;
                while (reader.Read())
                {
#if NO
                                    if (handle != null
                                        && (nGetedCount % 10000) == 0)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }
#endif
                    if (token.IsCancellationRequested)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strFirstColumn = ((string)reader[0]);
#if NO              // 为提高速度，不做这个判断了
                    if (bRecordTable && strFirstColumn.Length != 10)
                    {
                        strError = "结果集中出现了长度不是 10 位的记录号 '" + strFirstColumn + "'，不正常";
                        return -1;
                    }
#endif

                    if (style == OutputStyle.KeyCount)
                    {
                        DpRecord dprecord = new DpRecord(strFirstColumn);
                        dprecord.Index = bRecordTable ? 1 : (int)reader.GetInt32(1);
                        resultSet.Add(dprecord);
                    }
                    else if (style == OutputStyle.KeyID)
                    {
                        // datareader key, id
                        // 结果集格式 key, path
                        string strKey = strFirstColumn;
                        string strId = this.FullID + "/" + (string)reader[1]; // 格式为：库id/记录号
                        string strFrom = (string)reader[2];
                        DpRecord record = new DpRecord(strId);
                        record.BrowseText = strKey + new string(DpResultSetManager.FROM_LEAD, 1) + strFrom;
                        resultSet.Add(record);
                    }
                    else
                    {
                        string strId = "";
                        strId = this.FullID + "/" + strFirstColumn; // 记录格式为：库id/记录号
                        resultSet.Add(new DpRecord(strId));
                    }

                    nGetedCount++;

                    // 超过最大数了
                    if (nMaxCount != -1
                        && nGetedCount >= nMaxCount)
                        break;

                    // Thread.Sleep(0);
                    if (nGetedCount % 100 == 0)
                        Thread.Sleep(1);
                }

                return nGetedCount;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        /*
        <target list="中文图书实体:册条码号">
        <item>
        <word>00000335,00000903</word>
        <match>exact</match>
        <relation>list</relation>
        <dataType>string</dataType>
        </item>
        <lang>zh</lang>
        </target>         * */
        int ProcessList(
            string strWordList,
            ref List<DbParameter> aSqlParameter,
            out string strKeyCondition,
            out string strError)
        {
            strError = "";
            strKeyCondition = "";

            StringBuilder text = new StringBuilder(4096);

            List<string> words = StringUtil.SplitList(strWordList);
            int i = 0;
            foreach (string word in words)
            {
                string strWord = word.Trim();

                if (i > 0)
                    text.Append(" OR ");
                string strParameterName = "@key" + i.ToString();
                text.Append(" keystring = " + strParameterName);

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                    temp.Value = strWord;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                    temp.Value = strWord;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                    temp.Value = strWord;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"),
                        OracleDbType.NVarchar2);
                    temp.Value = strWord;
                    aSqlParameter.Add(temp);
                }

                i++;
            }

            strKeyCondition = text.ToString();
            return 0;
        }

        // 得到检索条件，私有函数，被SearchByUnion()函数调
        // 可能会抛出的异常:NoMatchException(检索方式与数据类型)
        // 注： 函数返回后，strKeyCondition中的 '@' 字符可能需要替换为 ':' (Oracle情形)
        // parameters:
        //      searchItem              SearchItem对象
        //      nodeConvertQueryString  字符串型检索词的处理信息节点
        //      nodeConvertQueryNumber  数值型检索词的处理信息节点
        //      strPostfix              Sql命令参数名称后缀，以便多个参数合在一起时区分
        //      aParameter              参数数组
        //      strKeyCondition         out参数，返回Sql检索式条件部分
        //      strError                out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全
        // ???该函数抛出异常的处理不太顺
        private int GetKeyCondition(SearchItem searchItem,
            XmlNode nodeConvertQueryString,
            XmlNode nodeConvertQueryNumber,
            string strPostfix,
            ref List<DbParameter> aSqlParameter,
            out string strKeyCondition,
            out string strError)
        {
            strKeyCondition = "";
            strError = "";

            bool bSearchNull = false;
            if (searchItem.Match == "exact"
                && searchItem.Relation == "="
                && String.IsNullOrEmpty(searchItem.Word) == true)
            {
                bSearchNull = true;
            }

            //检索三项是否有矛盾，该函数可能会抛出NoMatchException异常
            QueryUtil.VerifyRelation(ref searchItem.Match,
                ref searchItem.Relation,
                ref searchItem.DataType);


            int nRet = 0;
            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;


            //3.根据数据类型，对检索词进行加工
            string strKeyValue = searchItem.Word.Trim();
            if (searchItem.DataType == "string")    //字符类型调字符的配置，对检索词进行加工
            {
                if (nodeConvertQueryString != null && keysCfg != null)
                {
                    List<string> keys = null;
                    nRet = keysCfg.ConvertKeyWithStringNode(
                        null,//dataDom
                        strKeyValue,
                        nodeConvertQueryString,
                        out keys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keys.Count != 1)
                    {
                        string[] list = new string[keys.Count];
                        keys.CopyTo(list);
                        strError = "不支持把检索词 '" + strKeyValue + "' 通过'split'样式加工成多个(" + string.Join(",", list) + ")";
                        return -1;
                    }
                    strKeyValue = keys[0];
                }
            }
            else if (searchItem.DataType == "number"   //数字型调数字格式的配置，对检索词进行加工
                     && (searchItem.Relation != "draw" && searchItem.Relation != "range"))  // 2009/9/26 add
            {
                if (nodeConvertQueryNumber != null
                    && keysCfg != null)
                {
                    string strMyKey;
                    nRet = keysCfg.ConvertKeyWithNumberNode(
                        null,
                        strKeyValue,
                        nodeConvertQueryNumber,
                        out strMyKey,
                        out strError);
                    if (nRet == -1 || nRet == 1)
                        return -1;
                    strKeyValue = strMyKey;
                }
            }

            string strParameterName;
            //4.根据match的值，分别得到不同的检索表达式
            if (searchItem.Match == "left"
                || searchItem.Match == "")  //如果strMatch为空，则按"左方一致"
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex =
                        new NoMatchException("在匹配方式值为left或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " ";

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                    temp.Value = strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                    temp.Value = strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                    temp.Value = strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"),
                        OracleDbType.NVarchar2);
                    temp.Value = strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
            }
            else if (searchItem.Match == "middle")
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式值为middle或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " "; //N'%" + strKeyValue + "'";

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                    temp.Value = "%" + strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                    temp.Value = "%" + strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                    temp.Value = "%" + strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"), OracleDbType.NVarchar2);
                    temp.Value = "%" + strKeyValue + "%";
                    aSqlParameter.Add(temp);
                }
            }
            else if (searchItem.Match == "right")
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式值为left或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " "; //N'%" + strKeyValue + "'";

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                    temp.Value = "%" + strKeyValue;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                    temp.Value = "%" + strKeyValue;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                    temp.Value = "%" + strKeyValue;
                    aSqlParameter.Add(temp);
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"),
                        OracleDbType.NVarchar2);
                    temp.Value = "%" + strKeyValue;
                    aSqlParameter.Add(temp);
                }
            }
            else if (searchItem.Match == "exact") //先看match，再看relation,最后看dataType
            {
                // 2012/11/27
                if (searchItem.Relation == "list")
                {
                    nRet = ProcessList(searchItem.Word,
            ref aSqlParameter,
            out strKeyCondition,
            out strError);
                    if (nRet == -1)
                        return -1;
                }
                //从词中汲取,较复杂，注意
                else if (searchItem.Relation == "draw"
                    || searchItem.Relation == "range")
                {
                    // 2012/3/29
                    if (string.IsNullOrEmpty(searchItem.Word) == true)
                    {
                        if (bSearchNull == true && searchItem.DataType == "number")
                            searchItem.Word = "~";
                        else if (searchItem.DataType == "number")
                            searchItem.Word = "~";
                    }

                    string strStartText;
                    string strEndText;
                    bool bRet = StringUtil.SplitRangeEx(searchItem.Word,
                        out strStartText,
                        out strEndText);

                    if (bRet == true)
                    {
                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                // 加工首
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strStartText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strStartText = keys[0];


                                // 加工尾
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strEndText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strEndText = keys[0];
                            }
                            string strParameterMinName = "@keyValueMin" + strPostfix;
                            string strParameterMaxName = "@keyValueMax" + strPostfix;

                            strKeyCondition = " " + strParameterMinName
                                + " <=keystring and keystring<= "
                                + strParameterMaxName + " ";

                            if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            {
                                SqlParameter temp = new SqlParameter(strParameterMinName, SqlDbType.NVarChar);
                                temp.Value = strStartText;
                                aSqlParameter.Add(temp);

                                temp = new SqlParameter(strParameterMaxName, SqlDbType.NVarChar);
                                temp.Value = strEndText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.SQLite)
                            {
                                SQLiteParameter temp = new SQLiteParameter(strParameterMinName, DbType.String);
                                temp.Value = strStartText;
                                aSqlParameter.Add(temp);

                                temp = new SQLiteParameter(strParameterMaxName, DbType.String);
                                temp.Value = strEndText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.MySql)
                            {
                                MySqlParameter temp = new MySqlParameter(strParameterMinName, MySqlDbType.String);
                                temp.Value = strStartText;
                                aSqlParameter.Add(temp);

                                temp = new MySqlParameter(strParameterMaxName, MySqlDbType.String);
                                temp.Value = strEndText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.Oracle)
                            {
                                OracleParameter temp = new OracleParameter(strParameterMinName.Replace("@", ":"),
                                    OracleDbType.NVarchar2);
                                temp.Value = strStartText;
                                aSqlParameter.Add(temp);

                                temp = new OracleParameter(strParameterMaxName.Replace("@", ":"),
                                    OracleDbType.NVarchar2);
                                temp.Value = strEndText;
                                aSqlParameter.Add(temp);
                            }
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                // 首
                                string strMyKey;
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strStartText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strStartText = strMyKey;

                                // 尾
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strEndText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strEndText = strMyKey;
                            }
                            strKeyCondition = strStartText
                                + " <= keystringnum and keystringnum <= "
                                + strEndText +
                                " and keystringnum <> -1";
                        }
                    }
                    else
                    {
                        string strOperator;
                        string strRealText;

                        //如果词中没有包含关系符，则按=号算
                        StringUtil.GetPartCondition(searchItem.Word,
                            out strOperator,
                            out strRealText);

                        if (strOperator == "!=")
                            strOperator = "<>";

                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strRealText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strRealText = keys[0];

                            }

                            strParameterName = "@keyValue" + strPostfix;
                            strKeyCondition = " keystring"
                                + strOperator
                                + " " + strParameterName + " ";

                            if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            {
                                SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                                temp.Value = strRealText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.SQLite)
                            {
                                SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                                temp.Value = strRealText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.MySql)
                            {
                                MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                                temp.Value = strRealText;
                                aSqlParameter.Add(temp);
                            }
                            else if (this.container.SqlServerType == SqlServerType.Oracle)
                            {
                                OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"),
                                    OracleDbType.NVarchar2);
                                temp.Value = strRealText;
                                aSqlParameter.Add(temp);
                            }
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                string strMyKey;
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strRealText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strRealText = strMyKey;
                            }

                            strKeyCondition = " keystringnum"
                                + strOperator
                                + strRealText
                                + " and keystringnum <> -1";
                        }
                    }
                }
                else   //普通的关系操作符
                {
                    //当关系操作符为空为，按等于算
                    if (searchItem.Relation == "")
                        searchItem.Relation = "=";
                    if (searchItem.Relation == "!=")
                        searchItem.Relation = "<>";

                    if (searchItem.DataType == "string")
                    {
                        strParameterName = "@keyValue" + strPostfix;

                        strKeyCondition = " keystring "
                            + searchItem.Relation
                            + " " + strParameterName + " ";

                        if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                        {
                            SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                            temp.Value = strKeyValue;
                            aSqlParameter.Add(temp);
                        }
                        else if (this.container.SqlServerType == SqlServerType.SQLite)
                        {
                            SQLiteParameter temp = new SQLiteParameter(strParameterName, DbType.String);
                            temp.Value = strKeyValue;
                            aSqlParameter.Add(temp);
                        }
                        else if (this.container.SqlServerType == SqlServerType.MySql)
                        {
                            MySqlParameter temp = new MySqlParameter(strParameterName, MySqlDbType.String);
                            temp.Value = strKeyValue;
                            aSqlParameter.Add(temp);
                        }
                        else if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            OracleParameter temp = new OracleParameter(strParameterName.Replace("@", ":"),
                                OracleDbType.NVarchar2);
                            temp.Value = strKeyValue;
                            aSqlParameter.Add(temp);
                        }
                    }
                    else if (searchItem.DataType == "number")
                    {
                        if (string.IsNullOrEmpty(strKeyValue) == false)
                            strKeyCondition = " keystringnum "
                                + searchItem.Relation
                                + strKeyValue
                                + " and keystringnum <> -1";
                        else
                            strKeyCondition = " keystringnum <> -1";    // 2012/3/29
                    }
                }
            }

            return 0;
        }

        static OutputStyle GetOutputStyle(string strOutputStyle)
        {
            if (StringUtil.IsInList("keycount", strOutputStyle))
                return OutputStyle.KeyCount;
            if (StringUtil.IsInList("keyid", strOutputStyle))
                return OutputStyle.KeyID;
            return OutputStyle.ID;
        }

#if NO
        // 老版本
        // TODO: 检索中途可以考虑给 handle 挂一个事件，事件触发的时候，主动去关闭 sqlreader
        // 检索
        // parameters:
        //      searchItem  SearchItem对象，存放检索词等信息
        //      isConnected 连接对象
        //      resultSet   结果集对象，存放命中记录。本函数并不在检索前清空结果集，因此，对同一结果集对象多次执行本函数，则可以把命中结果追加在一起
        //      strLang     语言版本，
        // return:
        //		-1	出错
        //		0	成功
        //      1   成功，但resultset需要再行排序一次
        internal override int SearchByUnion(
            string strOutputStyle,
            SearchItem searchItem,
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
            DpResultSet resultSet,
            int nWarningLevel,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            bool bNeedSort = false;

            DateTime start_time = DateTime.Now;

            //**********对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            // 2006/12/18 changed

            try
            {
                bool bHasID = false;
                List<TableInfo> aTableInfo = null;
                int nRet = this.TableNames2aTableInfo(searchItem.TargetTables,
                    out bHasID,
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                // TODO: ***注意：如果若干检索途径中有了__id,那么就只有这一个有效，而其他的就无效了。这似乎需要改进。2007/9/13

                if (bHasID == true)
                {
                    nRet = SearchByID(searchItem,
                        handle,
                        // isConnected,
                        resultSet,
                        strOutputStyle,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 对sql库来说,通过ID检索后，记录已排序，去重
                if (aTableInfo == null || aTableInfo.Count == 0)
                    return 0;

                // 2009/8/5 
                bool bSearchNull = false;   // 是否为空值检索
                if (searchItem.Match == "exact"
                    && searchItem.Relation == "="
                    && String.IsNullOrEmpty(searchItem.Word) == true)
                {
                    bSearchNull = true;
                }

                string strCommand = "";

                // Sql命令参数数组
                List<DbParameter> aSqlParameter = new List<DbParameter>();

                string strColumnList = "";

                if (bOutputKeyCount == true
                    && bSearchNull == false)    // 2009/8/6 
                {
                    strColumnList = " keystring, count(*) ";
                }
                else if (bOutputKeyID == true
                    && bSearchNull == false)    // 2010/5/12 
                {
                    strColumnList = " keystring, idstring, fromstring ";
                }
                else
                {
                    // 当bSearchNull==true的时候，column list应当和bOutputKeysCount == false时候一样

                    string strSelectKeystring = "";
                    if (searchItem.KeyOrder != "")
                    {
                        if (aTableInfo.Count > 1)
                            strSelectKeystring = ",keystring";
                    }

                    strColumnList = " idstring" + strSelectKeystring + " ";
                }

                // 循环每一个检索途径
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    // 2015/8/25
                    string strFromValue = "";
                    strFromValue = KeysCfg.GetFromValue(tableInfo.Node as XmlElement);

                    // 参数名的后缀
                    string strPostfix = Convert.ToString(i);

                    string strConditionAboutKey = "";
                    try
                    {
                        nRet = GetKeyCondition(
                            searchItem,
                            tableInfo.nodeConvertQueryString,
                            tableInfo.nodeConvertQueryNumber,
                            strPostfix,
                            ref aSqlParameter,
                            out strConditionAboutKey,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strConditionAboutKey = strConditionAboutKey.Replace("@", ":");
                        }
                    }
                    catch (NoMatchException ex)
                    {
                        strWarning = ex.Message;
                        strError = strWarning;
                        return -1;
                    }

                    // 如果限制了一个最大数，则按每个途径都是这个最大数算
                    string strTop = "";
                    string strLimit = "";

                    if (bSearchNull == false)
                    {
                        if (searchItem.MaxCount != -1)  //限制的最大数
                        {
                            if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                                strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.SQLite)
                                strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.MySql)
                                strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.Oracle)
                                strLimit = " rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                            else
                                throw new Exception("未知的 SqlServerType");
                        }
                    }

                    string strWhere = "";

                    if (bSearchNull == false)
                    {
                        if (strConditionAboutKey != "")
                            strWhere = " WHERE " + strConditionAboutKey;
                    }

                    string strDistinct = " DISTINCT ";
                    string strGroupBy = "";
                    if (bOutputKeyCount == true
                        && bSearchNull == false)
                    {
                        strDistinct = "";
                        strGroupBy = " GROUP BY keystring";
                    }

                    string strTableName = tableInfo.SqlTableName;
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        strTableName = this.m_strSqlDbName + "_" + tableInfo.SqlTableName;
                    }

                    string strOneCommand = "";
                    if (i == 0)// 第一个表
                    {
                        strOneCommand =
                            " SELECT "
                            + strDistinct
                            + strTop
                            // + " idstring" + strSelectKeystring + " "
                            + strColumnList
                            + " FROM " + strTableName + " "
                            + strWhere
                            + strGroupBy
                            + (i == aTableInfo.Count - 1 ? strLimit : "");

                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strOneCommand =
        " SELECT "
        + strDistinct
        + strTop
        // + " idstring" + strSelectKeystring + " "
        + strColumnList
        + " FROM " + strTableName + " "
        + strWhere
        + strGroupBy;
                            if (string.IsNullOrEmpty(strLimit) == false)
                            {
                                // 注：如果要在有限制数的情况下确保命中靠前的条目，需要采用 select * from ( 办法
                                if (string.IsNullOrEmpty(strGroupBy) == false)
                                    strOneCommand = " SELECT * FROM ("
                                        + strOneCommand
                                        + ") WHERE " + strLimit;
                                else
                                {
                                    strOneCommand = strOneCommand
                                        + (string.IsNullOrEmpty(strWhere) == false ? " AND " : " ")
                                        + strLimit;
                                }
                            }
                        }
                    }
                    else
                    {
                        strOneCommand = " SELECT "  // union
                            + strDistinct
                            + strTop
                            // + " idstring" + strSelectKeystring + " "  //DISTINCT 去重
                            + strColumnList
                            + " FROM " + strTableName + " "
                            + strWhere
                            + strGroupBy
                            + (i == aTableInfo.Count - 1 ? strLimit : "");
                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strOneCommand = " SELECT "
        + strDistinct
        + strTop
        // + " idstring" + strSelectKeystring + " "  //DISTINCT 去重
        + strColumnList
        + " FROM " + strTableName + " "
        + strWhere
        + strGroupBy;
                            if (string.IsNullOrEmpty(strLimit) == false)
                            {
                                // 注：如果要在有限制数的情况下确保命中靠前的条目，需要采用 select * from ( 办法
                                if (string.IsNullOrEmpty(strGroupBy) == false)
                                    strOneCommand = " SELECT * FROM ("
                                    + strOneCommand
                                    + ") WHERE " + strLimit;
                                else
                                {
                                    strOneCommand = strOneCommand
                                        + (string.IsNullOrEmpty(strWhere) == false ? " AND " : " ")
                                        + strLimit;
                                }

                            }

                            // strOneCommand = " union " + strOneCommand;
                        }
                    }

                    if (bSearchNull == true)
                    {
                        string strColumns = " id ";
                        if (bOutputKeyCount == true)
                        {
                            if (bSearchNull == true)
                                strColumns = " '', count(*) ";  // 2015/8/25
                            else
                                strColumns = " keystring='', count(*) ";
                        }
                        else if (bOutputKeyID == true)
                        {
                            if (bSearchNull == true)
                            {
                                // strColumns = " '', id, 'recid' ";  // 2015/8/25 TODO 第三列内容应该根据 tablename 翻译得到
                                strColumns = " '', id, '"
                                    + (string.IsNullOrEmpty(strFromValue) == false ? strFromValue : "recid")
                                    + "' ";// 2015/8/25 
                            }
                            else
                                strColumns = " keystring=id, id, fromstring='recid' ";   // fromstring='' 2011/7/24
                        }

                        {
                            strOneCommand = "select "
        + strColumns // " id "
        + "from records where id like '__________' and id not in (" + strOneCommand + ") "
        ;
                        }

                    }

                    if (i == 0)
                        strCommand += strOneCommand;
                    else
                        strCommand += " union " + strOneCommand;

                }


                /*
                 * select  '', id, 'barcode' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_barcode  union SELECT  DISTINCT  idstring  FROM keys_batchno  union SELECT  DISTINCT  idstring  FROM keys_registerno  union SELECT  DISTINCT  idstring  FROM keys_accessNo  union SELECT  DISTINCT  idstring  FROM keys_location  union SELECT  DISTINCT  idstring  FROM keys_refID  union SELECT  DISTINCT  idstring  FROM keys_locationclass  union SELECT  DISTINCT  idstring  FROM keys_parent  union SELECT  DISTINCT  idstring  FROM keys_state  union SELECT  DISTINCT  idstring  FROM keys_parentlocation ) 
                 * 应该修改为
                 * select  '', id, 'barcode' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_barcode )
                 * UNION select  '', id, 'batchno' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_batchno )
                 * */

                string strOrderBy = "";
                if (string.IsNullOrEmpty(searchItem.OrderBy) == false)
                {
                    strOrderBy = " ORDER BY " + searchItem.OrderBy + " ";

                    // 2010/5/10
                    string strTemp = searchItem.OrderBy.ToLower();
                    if (strTemp.IndexOf("desc") != -1)
                        resultSet.Asc = -1;

                    // TODO: 多个select union, 总的序可能是乱的
                }

                // 2009/8/5
                if (bSearchNull == true)
                {
                    string strTop = "";
                    string strLimit = "";

                    if (searchItem.MaxCount != -1)  //限制的最大数
                    {
                        if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.SQLite)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.MySql)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.Oracle)
                            strLimit = " WHERE rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                        else
                            throw new Exception("未知的 SqlServerType");
                    }

                    // Oracle比较特殊
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        if (string.IsNullOrEmpty(strLimit) == false)
                            strCommand = "SELECT * FROM (" + strCommand + ") "
        + strOrderBy    // 2012/3/30
        + strLimit;
                        else
                            strCommand = "select * FROM (" + strCommand + ") "
        + strOrderBy    // 2012/3/30
        ;
                    }
                    else
                    {
                        // 将 top 子句插入 select 后面 2015/12/23
                        if (string.IsNullOrEmpty(strTop) == false)
                            strCommand = InsertTopPart(strCommand, strTop);

                        if (string.IsNullOrEmpty(strLimit) == false
        || string.IsNullOrEmpty(strOrderBy) == false)
                            strCommand = strCommand
        + strOrderBy
        + strLimit;

#if NO
                        // strTop 有内容时这个用法要导致 MS SQL Server 报错
                        if (string.IsNullOrEmpty(strTop) == false
                            || string.IsNullOrEmpty(strLimit) == false
                            || string.IsNullOrEmpty(strOrderBy) == false)
                            strCommand = "select "
        + strTop
        + " * FROM (" + strCommand + ") "
        + strOrderBy
        + strLimit;
#endif
                    }


#if NO
                    string strTop = "";
                    string strLimit = "";

                    if (searchItem.MaxCount != -1)  //限制的最大数
                    {
                        if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.SQLite)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.MySql)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.Oracle)
                            strLimit = " WHERE rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                        else
                            throw new Exception("未知的 SqlServerType");
                    }

                    string strColumns = " id ";
                    if (bOutputKeyCount == true)
                    {
                        if (bSearchNull == true)
                            strColumns = " '', count(*) ";  // 2015/8/25
                        else
                            strColumns = " keystring='', count(*) ";
                    }
                    else if (bOutputKeyID == true)
                    {
                        if (bSearchNull == true)
                        {
                            // strColumns = " '', id, 'recid' ";  // 2015/8/25 TODO 第三列内容应该根据 tablename 翻译得到
                            strColumns = " '', id, '"
                                +(string.IsNullOrEmpty(strFromValue) == false ? strFromValue: "recid")
                                +"' ";// 2015/8/25 
                        }
                        else
                            strColumns = " keystring=id, id, fromstring='recid' ";   // fromstring='' 2011/7/24
                    }

                    // Oracle比较特殊
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        if (string.IsNullOrEmpty(strLimit) == false)
                            strCommand = "SELECT * FROM (select "
    + strColumns // " id "
    + "from " + this.m_strSqlDbName + "_records where id like '__________' and id not in (" + strCommand + ") "
    + strOrderBy    // 2012/3/30
    + ") " + strLimit;
                        else
                            strCommand = "select "
+ strColumns // " id "
+ "from " + this.m_strSqlDbName + "_records where id like '__________' and id not in (" + strCommand + ") "
+ strOrderBy    // 2012/3/30
;
                    }
                    else
                    {
                        strCommand = "select "
    + strTop
    + strColumns // " id "
    + "from records where id like '__________' and id not in (" + strCommand + ") "
    + strOrderBy    // 2012/3/30
    + strLimit;
                    }

#endif
                }
                else
                {
                    if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                        strCommand += " " + strOrderBy;
                    else
                        bNeedSort = true;
                    // TODO: 其他数据库类型，是否在一个select * from () 后面加order by(如果只有一个select语句则不要加外壳)，还是在每个具体的select语句里面加order by?
                }

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    strCommand = "use " + this.m_strSqlDbName + " "
                    + strCommand;
                else if (this.container.SqlServerType == SqlServerType.MySql)
                    strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
                    + strCommand;

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    strCommand += " use master " + "\n";

                if (aSqlParameter == null)
                {
                    strError = "一个参数也没 是不可能的情况";
                    return -1;
                }

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                {
                    SqlConnection connection =
                        new SqlConnection(this.m_strConnString/*Pooling*/);
                    connection.Open();
                    try
                    {
                        SqlCommand command = new SqlCommand(strCommand,
                            connection);
                        try
                        {
                            foreach (SqlParameter sqlParameter in aSqlParameter)
                            {
                                command.Parameters.Add(sqlParameter);
                            }
                            command.CommandTimeout = 20 * 60;  // 把检索时间变大

                            var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
handle.CancelTokenSource.Token).Result;

                            // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                            // return:
                            //      -1  出错
                            //      0   没有填入任何记录
                            //      >0  实际填入的记录条数
                            nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                searchItem.MaxCount,
                                GetOutputStyle(strOutputStyle),
                                false,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                return nRet;
#if NO
                            IAsyncResult r = command.BeginExecuteReader(CommandBehavior.CloseConnection);
                            while (true)
                            {
                                if (handle != null)
                                {
                                    if (handle.DoIdle() == false)
                                    {
                                        command.Cancel();
                                        try
                                        {
                                            command.EndExecuteReader(r);
                                        }
                                        catch
                                        {
                                        }
                                        strError = "用户中断";
                                        return -1;
                                    }
                                }
                                else
                                    break;

                                bool bRet = r.AsyncWaitHandle.WaitOne(100, false);  //millisecondsTimeout
                                if (bRet == true)
                                    break;
                            }

                            SqlDataReader reader = command.EndExecuteReader(r);
                            try
                            {

                                if (reader == null
                                    || reader.HasRows == false)
                                {
                                    return 0;
                                }

                                int nGetedCount = 0;
                                while (reader.Read())
                                {
                                    if (handle != null
                                        && (nGetedCount % 10000) == 0)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }

                                    if (bOutputKeyCount == true)
                                    {
                                        int count = (int)reader[1];
                                        DpRecord dprecord = new DpRecord((string)reader[0]);
                                        dprecord.Index = count;
                                        resultSet.Add(dprecord);
                                    }
                                    else if (bOutputKeyID == true)
                                    {
                                        // datareader key, id
                                        // 结果集格式 key, path
                                        string strKey = (string)reader[0];
                                        string strId = this.FullID + "/" + (string)reader[1]; // 格式为：库id/记录号
                                        string strFrom = (string)reader[2];
                                        DpRecord record = new DpRecord(strId);
                                        // new DpRecord(strKey + "," + strId)
                                        record.BrowseText = strKey + new string(DpResultSetManager.FROM_LEAD, 1) + strFrom;
                                        resultSet.Add(record);
                                    }
                                    else
                                    {
                                        string strId = "";
                                        strId = this.FullID + "/" + (string)reader[0]; // 记录格式为：库id/记录号
                                        resultSet.Add(new DpRecord(strId));
                                    }

                                    nGetedCount++;

                                    // 超过最大数了
                                    if (searchItem.MaxCount != -1
                                        && nGetedCount >= searchItem.MaxCount)
                                        break;

                                    Thread.Sleep(0);
                                }
                            }
                            finally
                            {
                                if (reader != null)
                                    reader.Close();
                            }
#endif
                        } // end of using command
                        finally
                        {
                            if (command != null)
                                command.Dispose();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);

                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "SearchByUnion() exception: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally // 连接
                    {
                        if (connection != null)
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }
                }
                else if (this.container.SqlServerType == SqlServerType.SQLite)
                {
                    // SQLite 采用保守连接
                    SQLiteConnection connection =
                        new SQLiteConnection(this.m_strConnString/*Pooling*/);
                    // connection.Open();
                    Open(connection);
                    try
                    {
                        SQLiteCommand command = new SQLiteCommand(strCommand,
                            connection);
                        try
                        {
                            foreach (SQLiteParameter sqlParameter in aSqlParameter)
                            {
                                command.Parameters.Add(sqlParameter);
                            }
                            command.CommandTimeout = 20 * 60;  // 把检索时间变大

                            var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
    handle.CancelTokenSource.Token).Result;

                            // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                            // return:
                            //      -1  出错
                            //      0   没有填入任何记录
                            //      >0  实际填入的记录条数
                            nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                searchItem.MaxCount,
                                GetOutputStyle(strOutputStyle),
                                false,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                return nRet;
#if NO
                            SQLiteDataReader reader = null;

                            // 调新线程处理
                            DatabaseCommandTask task = new DatabaseCommandTask(command);
                            try
                            {
                                if (task == null)
                                {
                                    strError = "test为null";
                                    return -1;
                                }
                                Thread t1 = new Thread(new ThreadStart(task.ThreadMain));
                                t1.Start();
                                bool bRet;
                                while (true)
                                {
                                    if (handle != null)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            command = null; // 这里不要Dispose() 丢给线程 task.ThreadMain 去Dispose()
                                            connection = null;
                                            reader = null;
                                            task.Cancel();
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }
                                    bRet = task.m_event.WaitOne(100, false);  //1/10秒看一次
                                    if (bRet == true)
                                        break;
                                }

                                // 如果DataReader==null，可能是SQL检索式出错了
                                // 2007/9/14 
                                if (task.bError == true)
                                {
                                    strError = task.ErrorString;
                                    return -1;
                                }

                                reader = (SQLiteDataReader)task.DataReader;

                                if (reader == null
                                    || reader.HasRows == false)
                                {
                                    return 0;
                                }

                                int nGetedCount = 0;
                                while (reader.Read())
                                {
                                    if (handle != null
                                        && (nGetedCount % 10000) == 0)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }

                                    if (bOutputKeyCount == true)
                                    {
                                        long count = (long)reader[1];
                                        DpRecord dprecord = new DpRecord((string)reader[0]);
                                        dprecord.Index = (int)count;
                                        resultSet.Add(dprecord);
                                    }
                                    else if (bOutputKeyID == true)
                                    {
                                        // datareader key, id
                                        // 结果集格式 key, path
                                        string strKey = (string)reader[0];
                                        string strId = this.FullID + "/" + (string)reader[1]; // 格式为：库id/记录号
                                        string strFrom = (string)reader[2];
                                        DpRecord record = new DpRecord(strId);
                                        // new DpRecord(strKey + "," + strId)
                                        record.BrowseText = strKey + new string(DpResultSetManager.FROM_LEAD, 1) + strFrom;
                                        resultSet.Add(record);
                                    }
                                    else
                                    {
                                        string strId = "";
                                        strId = this.FullID + "/" + (string)reader[0]; // 记录格式为：库id/记录号
                                        resultSet.Add(new DpRecord(strId));
                                    }

                                    nGetedCount++;

                                    // 超过最大数了
                                    if (searchItem.MaxCount != -1
                                        && nGetedCount >= searchItem.MaxCount)
                                        break;

                                    Thread.Sleep(0);
                                }
                            }
                            finally
                            {
                                if (reader != null)
                                    reader.Close();
                                if (task != null)
                                    task.Dispose();
                            }
#endif
                        } // end of using command
                        finally
                        {
                            if (command != null)
                                command.Dispose();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);

                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "SearchByUnion() exception: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally // 连接
                    {
                        if (connection != null)
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }
                }
                else if (this.container.SqlServerType == SqlServerType.MySql)
                {
                    MySqlConnection connection =
                        new MySqlConnection(this.m_strConnString/*Pooling*/);
                    // connection.Open();  // TODO: TryOpen
                    Connection.TryOpen(connection, this);
                    try
                    {
                        MySqlCommand command = new MySqlCommand(strCommand,
                            connection);
                        try
                        {
                            foreach (MySqlParameter sqlParameter in aSqlParameter)
                            {
                                command.Parameters.Add(sqlParameter);
                            }
                            command.CommandTimeout = 20 * 60;  // 把检索时间变大

                            var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
                                handle.CancelTokenSource.Token).Result;

#if NO
                            IAsyncResult r = command.BeginExecuteReader(CommandBehavior.CloseConnection);
                            while (true)
                            {
                                if (handle != null)
                                {
                                    if (handle.DoIdle() == false)
                                    {
                                        command.Cancel();
                                        try
                                        {
                                            command.EndExecuteReader(r);
                                        }
                                        catch
                                        {
                                        }
                                        strError = "用户中断";
                                        return -1;
                                    }
                                }
                                else
                                    break;

                                bool bRet = r.AsyncWaitHandle.WaitOne(100, false);  //millisecondsTimeout
                                if (bRet == true)
                                    break;
                                /*
                                if (r.IsCompleted == true)
                                    break;
                                Thread.Sleep(1);
                                 * */
                            }

                            MySqlDataReader reader = command.EndExecuteReader(r);
#endif

                            // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                            // return:
                            //      -1  出错
                            //      0   没有填入任何记录
                            //      >0  实际填入的记录条数
                            nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                searchItem.MaxCount,
                                GetOutputStyle(strOutputStyle),
                                false,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                return nRet;
#if NO
                            try
                            {
                                if (reader == null
                                    || reader.HasRows == false)
                                {
                                    return 0;
                                }

                                int nGetedCount = 0;
                                while (reader.Read())
                                {
#if NO
                                    if (handle != null
                                        && (nGetedCount % 10000) == 0)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }
#endif
                                    if (handle != null 
                                        && handle.CancelTokenSource.IsCancellationRequested)
                                    {
                                        strError = "用户中断";
                                        return -1;
                                    }

                                    if (bOutputKeyCount == true)
                                    {
                                        int count = (int)reader.GetInt32(1);
                                        DpRecord dprecord = new DpRecord((string)reader[0]);
                                        dprecord.Index = count;
                                        resultSet.Add(dprecord);
                                    }
                                    else if (bOutputKeyID == true)
                                    {
                                        // datareader key, id
                                        // 结果集格式 key, path
                                        string strKey = (string)reader[0];
                                        string strId = this.FullID + "/" + (string)reader[1]; // 格式为：库id/记录号
                                        string strFrom = (string)reader[2];
                                        DpRecord record = new DpRecord(strId);
                                        // new DpRecord(strKey + "," + strId)
                                        record.BrowseText = strKey + new string(DpResultSetManager.FROM_LEAD, 1) + strFrom;
                                        resultSet.Add(record);
                                    }
                                    else
                                    {
                                        string strId = "";
                                        strId = this.FullID + "/" + (string)reader[0]; // 记录格式为：库id/记录号
                                        resultSet.Add(new DpRecord(strId));
                                    }

                                    nGetedCount++;

                                    // 超过最大数了
                                    if (searchItem.MaxCount != -1
                                        && nGetedCount >= searchItem.MaxCount)
                                        break;

                                    // Thread.Sleep(0);
                                    if (nGetedCount % 100 == 0)
                                        Thread.Sleep(1);
                                }
                            }
                            finally
                            {
                                if (reader != null)
                                    reader.Close();
                            }

#endif
                        }
                        finally
                        {
                            if (command != null)
                                command.Dispose();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "SearchByUnion() exception: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally // 连接
                    {
                        if (connection != null)
                        {
                            try
                            {
                                connection.Close();
                                connection.Dispose();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else if (this.container.SqlServerType == SqlServerType.Oracle)
                {
                    OracleConnection connection =
                        new OracleConnection(this.m_strConnString/*Pooling*/);
                    connection.Open();
                    try
                    {
                        OracleCommand command = new OracleCommand(strCommand,
                             connection);
                        try
                        {
                            command.BindByName = true;
                            foreach (OracleParameter sqlParameter in aSqlParameter)
                            {
                                command.Parameters.Add(sqlParameter);
                            }
                            command.CommandTimeout = 20 * 60;  // 把检索时间变大

                            var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
handle.CancelTokenSource.Token).Result;

                            // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                            // return:
                            //      -1  出错
                            //      0   没有填入任何记录
                            //      >0  实际填入的记录条数
                            nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                searchItem.MaxCount,
                                GetOutputStyle(strOutputStyle),
                                false,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                return nRet;
#if NO
                            OracleDataReader reader = null;

                            // 调新线程处理
                            DatabaseCommandTask task = new DatabaseCommandTask(command);
                            try
                            {
                                if (task == null)
                                {
                                    strError = "test为null";
                                    return -1;
                                }
                                Thread t1 = new Thread(new ThreadStart(task.ThreadMain));
                                t1.Start();
                                bool bRet;
                                while (true)
                                {
                                    if (handle != null)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            command = null; // 这里不要Dispose() 丢给线程 task.ThreadMain 去Dispose()
                                            connection = null;
                                            reader = null;
                                            task.Cancel();
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }
                                    bRet = task.m_event.WaitOne(100, false);  //1/10秒看一次
                                    if (bRet == true)
                                        break;
                                }

                                // 如果DataReader==null，可能是SQL检索式出错了
                                // 2007/9/14 
                                if (task.bError == true)
                                {
                                    strError = task.ErrorString;
                                    return -1;
                                }

                                reader = (OracleDataReader)task.DataReader;

                                if (reader == null
                                    || reader.HasRows == false)
                                {
                                    return 0;
                                }

                                int nGetedCount = 0;
                                while (reader.Read())
                                {
                                    if (handle != null
                                        && (nGetedCount % 10000) == 0)
                                    {
                                        if (handle.DoIdle() == false)
                                        {
                                            strError = "用户中断";
                                            return -1;
                                        }
                                    }

                                    if (bOutputKeyCount == true)
                                    {
                                        int count = reader.GetOracleDecimal(1).ToInt32();
                                        DpRecord dprecord = new DpRecord((string)reader[0]);
                                        dprecord.Index = count;
                                        resultSet.Add(dprecord);
                                    }
                                    else if (bOutputKeyID == true)
                                    {
                                        // datareader key, id
                                        // 结果集格式 key, path
                                        string strKey = (string)reader[0];
                                        string strId = this.FullID + "/" + (string)reader[1]; // 格式为：库id/记录号
                                        string strFrom = (string)reader[2];
                                        DpRecord record = new DpRecord(strId);
                                        // new DpRecord(strKey + "," + strId)
                                        record.BrowseText = strKey + new string(DpResultSetManager.FROM_LEAD, 1) + strFrom;
                                        resultSet.Add(record);
                                    }
                                    else
                                    {
                                        string strId = "";
                                        strId = this.FullID + "/" + (string)reader[0]; // 记录格式为：库id/记录号
                                        resultSet.Add(new DpRecord(strId));
                                    }

                                    nGetedCount++;

                                    // 超过最大数了
                                    if (searchItem.MaxCount != -1
                                        && nGetedCount >= searchItem.MaxCount)
                                        break;

                                    Thread.Sleep(0);
                                }
                            }
                            finally
                            {
                                if (reader != null)
                                    reader.Close();
                                if (task != null)
                                    task.Dispose();
                            }
#endif
                        }
                        finally
                        {
                            if (command != null)
                                command.Dispose();
                        }

                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "SearchByUnion() exception: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally // 连接
                    {
                        if (connection != null)
                        {
                            connection.Close();
                            connection.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "1: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {

                //*****************对数据库解读锁***************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif

                // 2006/12/18 changed

                TimeSpan delta = DateTime.Now - start_time;
                Debug.WriteLine("SearchByUnion耗时 " + delta.ToString());
            }

            if (bNeedSort == true)
                return 1;

            return 0;
        }
#endif

        // 新版本
        // TODO: 检索中途可以考虑给 handle 挂一个事件，事件触发的时候，主动去关闭 sqlreader
        // 检索
        // parameters:
        //      searchItem  SearchItem对象，存放检索词等信息
        //      isConnected 连接对象
        //      resultSet   结果集对象，存放命中记录。本函数并不在检索前清空结果集，因此，对同一结果集对象多次执行本函数，则可以把命中结果追加在一起
        //      strLang     语言版本，
        // return:
        //		-1	出错
        //		0	成功
        //      1   成功，但resultset需要再行排序一次
        internal override int SearchByUnion(
            string strOutputStyle,
            SearchItem searchItem,
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
            DpResultSet resultSet,
            int nWarningLevel,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            bool bNeedSort = false;

            DateTime start_time = DateTime.Now;

            //**********对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            // 2006/12/18 changed

            try
            {
                bool bHasID = false;
                List<TableInfo> aTableInfo = null;
                int nRet = this.TableNames2aTableInfo(searchItem.TargetTables,
                    out bHasID,
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                // TODO: ***注意：如果若干检索途径中有了__id,那么就只有这一个有效，而其他的就无效了。这似乎需要改进。2007/9/13

                if (bHasID == true)
                {
                    nRet = SearchByID(searchItem,
                        handle,
                        // isConnected,
                        resultSet,
                        strOutputStyle,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 对sql库来说,通过ID检索后，记录已排序，去重
                if (aTableInfo == null || aTableInfo.Count == 0)
                    return 0;

                // 2009/8/5 
                bool bSearchNull = false;   // 是否为空值检索
                if (searchItem.Match == "exact"
                    && searchItem.Relation == "="
                    && String.IsNullOrEmpty(searchItem.Word) == true)
                {
                    bSearchNull = true;
                }

                string strCommand = "";

                // Sql命令参数数组
                List<DbParameter> aSqlParameter = new List<DbParameter>();

                string strColumnList = "";

                if (bOutputKeyCount == true
                    && bSearchNull == false)    // 2009/8/6 
                {
                    strColumnList = " keystring, count(*) ";
                }
                else if (bOutputKeyID == true
                    && bSearchNull == false)    // 2010/5/12 
                {
                    strColumnList = " keystring, idstring, fromstring ";
                }
                else
                {
                    // 当bSearchNull==true的时候，column list应当和bOutputKeysCount == false时候一样

                    string strSelectKeystring = "";
                    if (searchItem.KeyOrder != "")
                    {
                        if (aTableInfo.Count > 1)
                            strSelectKeystring = ",keystring";
                    }

                    strColumnList = " idstring" + strSelectKeystring + " ";
                }

                // 循环每一个检索途径
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    // 2015/8/25
                    string strFromValue = "";
                    strFromValue = KeysCfg.GetFromValue(tableInfo.Node as XmlElement);

                    // 参数名的后缀
                    string strPostfix = Convert.ToString(i);

                    string strConditionAboutKey = "";
                    try
                    {
                        nRet = GetKeyCondition(
                            searchItem,
                            tableInfo.nodeConvertQueryString,
                            tableInfo.nodeConvertQueryNumber,
                            strPostfix,
                            ref aSqlParameter,
                            out strConditionAboutKey,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strConditionAboutKey = strConditionAboutKey.Replace("@", ":");
                        }
                    }
                    catch (NoMatchException ex)
                    {
                        strWarning = ex.Message;
                        strError = strWarning;
                        return -1;
                    }

                    // 如果限制了一个最大数，则按每个途径都是这个最大数算
                    string strTop = "";
                    string strLimit = "";

                    if (bSearchNull == false)
                    {
                        if (searchItem.MaxCount != -1)  //限制的最大数
                        {
                            if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                                strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.SQLite)
                                strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.MySql)
                                strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                            else if (this.container.SqlServerType == SqlServerType.Oracle)
                                strLimit = " rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                            else
                                throw new Exception("未知的 SqlServerType");
                        }
                    }

                    string strWhere = "";

                    if (bSearchNull == false)
                    {
                        if (strConditionAboutKey != "")
                            strWhere = " WHERE " + strConditionAboutKey;
                    }

                    string strDistinct = " DISTINCT ";
                    string strGroupBy = "";
                    if (bOutputKeyCount == true
                        && bSearchNull == false)
                    {
                        strDistinct = "";
                        strGroupBy = " GROUP BY keystring";
                    }

                    string strTableName = tableInfo.SqlTableName;
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        strTableName = this.m_strSqlDbName + "_" + tableInfo.SqlTableName;
                    }

                    string strOneCommand = "";
                    if (i == 0)// 第一个表
                    {
                        strOneCommand =
                            " SELECT "
                            + strDistinct
                            + strTop
                            // + " idstring" + strSelectKeystring + " "
                            + strColumnList
                            + " FROM " + strTableName + " "
                            + strWhere
                            + strGroupBy
                            + (i == aTableInfo.Count - 1 ? strLimit : "");

                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strOneCommand =
        " SELECT "
        + strDistinct
        + strTop
        // + " idstring" + strSelectKeystring + " "
        + strColumnList
        + " FROM " + strTableName + " "
        + strWhere
        + strGroupBy;
                            if (string.IsNullOrEmpty(strLimit) == false)
                            {
                                // 注：如果要在有限制数的情况下确保命中靠前的条目，需要采用 select * from ( 办法
                                if (string.IsNullOrEmpty(strGroupBy) == false)
                                    strOneCommand = " SELECT * FROM ("
                                        + strOneCommand
                                        + ") WHERE " + strLimit;
                                else
                                {
                                    strOneCommand = strOneCommand
                                        + (string.IsNullOrEmpty(strWhere) == false ? " AND " : " ")
                                        + strLimit;
                                }
                            }
                        }
                    }
                    else
                    {
                        strOneCommand = " SELECT "  // union
                            + strDistinct
                            + strTop
                            // + " idstring" + strSelectKeystring + " "  //DISTINCT 去重
                            + strColumnList
                            + " FROM " + strTableName + " "
                            + strWhere
                            + strGroupBy
                            + (i == aTableInfo.Count - 1 ? strLimit : "");
                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            strOneCommand = " SELECT "
        + strDistinct
        + strTop
        // + " idstring" + strSelectKeystring + " "  //DISTINCT 去重
        + strColumnList
        + " FROM " + strTableName + " "
        + strWhere
        + strGroupBy;
                            if (string.IsNullOrEmpty(strLimit) == false)
                            {
                                // 注：如果要在有限制数的情况下确保命中靠前的条目，需要采用 select * from ( 办法
                                if (string.IsNullOrEmpty(strGroupBy) == false)
                                    strOneCommand = " SELECT * FROM ("
                                    + strOneCommand
                                    + ") WHERE " + strLimit;
                                else
                                {
                                    strOneCommand = strOneCommand
                                        + (string.IsNullOrEmpty(strWhere) == false ? " AND " : " ")
                                        + strLimit;
                                }

                            }

                            // strOneCommand = " union " + strOneCommand;
                        }
                    }

                    if (bSearchNull == true)
                    {
                        string strColumns = " id ";
                        if (bOutputKeyCount == true)
                        {
                            if (bSearchNull == true)
                                strColumns = " '', count(*) ";  // 2015/8/25
                            else
                                strColumns = " keystring='', count(*) ";
                        }
                        else if (bOutputKeyID == true)
                        {
                            if (bSearchNull == true)
                            {
                                // strColumns = " '', id, 'recid' ";  // 2015/8/25 TODO 第三列内容应该根据 tablename 翻译得到
                                strColumns = " '', id, '"
                                    + (string.IsNullOrEmpty(strFromValue) == false ? strFromValue : "recid")
                                    + "' ";// 2015/8/25 
                            }
                            else
                                strColumns = " keystring=id, id, fromstring='recid' ";   // fromstring='' 2011/7/24
                        }

                        {
                            strOneCommand = "select "
        + strColumns // " id "
        + "from records where id like '__________' and id not in (" + strOneCommand + ") "
        ;
                        }

                    }

                    if (i == 0)
                        strCommand += strOneCommand;
                    else
                        strCommand += " union " + strOneCommand;

                }


                /*
                 * select  '', id, 'barcode' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_barcode  union SELECT  DISTINCT  idstring  FROM keys_batchno  union SELECT  DISTINCT  idstring  FROM keys_registerno  union SELECT  DISTINCT  idstring  FROM keys_accessNo  union SELECT  DISTINCT  idstring  FROM keys_location  union SELECT  DISTINCT  idstring  FROM keys_refID  union SELECT  DISTINCT  idstring  FROM keys_locationclass  union SELECT  DISTINCT  idstring  FROM keys_parent  union SELECT  DISTINCT  idstring  FROM keys_state  union SELECT  DISTINCT  idstring  FROM keys_parentlocation ) 
                 * 应该修改为
                 * select  '', id, 'barcode' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_barcode )
                 * UNION select  '', id, 'batchno' from records where id like '__________' and id not in ( SELECT  DISTINCT  idstring  FROM keys_batchno )
                 * */

                string strOrderBy = "";
                if (string.IsNullOrEmpty(searchItem.OrderBy) == false
                    && bSearchNull == false)    // 检索空的时候，列里面只有 id，没有 keystring 列，所以无法进行排序
                {
                    strOrderBy = " ORDER BY " + searchItem.OrderBy + " ";

                    // 2010/5/10
                    string strTemp = searchItem.OrderBy.ToLower();
                    if (strTemp.IndexOf("desc") != -1)
                        resultSet.Asc = -1;

                    // TODO: 多个select union, 总的序可能是乱的
                }

                // 2009/8/5
                if (bSearchNull == true)
                {
                    string strTop = "";
                    string strLimit = "";

                    if (searchItem.MaxCount != -1)  //限制的最大数
                    {
                        if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.SQLite)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.MySql)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.Oracle)
                            strLimit = " WHERE rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                        else
                            throw new Exception("未知的 SqlServerType");
                    }

                    // Oracle比较特殊
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        if (string.IsNullOrEmpty(strLimit) == false)
                            strCommand = "SELECT * FROM (" + strCommand + ") "
        + strOrderBy    // 2012/3/30
        + strLimit;
                        else
                            strCommand = "select * FROM (" + strCommand + ") "
        + strOrderBy    // 2012/3/30
        ;
                    }
                    else
                    {
                        // 将 top 子句插入 select 后面 2015/12/23
                        if (string.IsNullOrEmpty(strTop) == false)
                            strCommand = InsertTopPart(strCommand, strTop);

                        if (string.IsNullOrEmpty(strLimit) == false
        || string.IsNullOrEmpty(strOrderBy) == false)
                            strCommand = strCommand
        + strOrderBy
        + strLimit;

#if NO
                        // strTop 有内容时这个用法要导致 MS SQL Server 报错
                        if (string.IsNullOrEmpty(strTop) == false
                            || string.IsNullOrEmpty(strLimit) == false
                            || string.IsNullOrEmpty(strOrderBy) == false)
                            strCommand = "select "
        + strTop
        + " * FROM (" + strCommand + ") "
        + strOrderBy
        + strLimit;
#endif
                    }


#if NO
                    string strTop = "";
                    string strLimit = "";

                    if (searchItem.MaxCount != -1)  //限制的最大数
                    {
                        if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                            strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.SQLite)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.MySql)
                            strLimit = " LIMIT " + Convert.ToString(searchItem.MaxCount) + " ";
                        else if (this.container.SqlServerType == SqlServerType.Oracle)
                            strLimit = " WHERE rownum <= " + Convert.ToString(searchItem.MaxCount) + " ";
                        else
                            throw new Exception("未知的 SqlServerType");
                    }

                    string strColumns = " id ";
                    if (bOutputKeyCount == true)
                    {
                        if (bSearchNull == true)
                            strColumns = " '', count(*) ";  // 2015/8/25
                        else
                            strColumns = " keystring='', count(*) ";
                    }
                    else if (bOutputKeyID == true)
                    {
                        if (bSearchNull == true)
                        {
                            // strColumns = " '', id, 'recid' ";  // 2015/8/25 TODO 第三列内容应该根据 tablename 翻译得到
                            strColumns = " '', id, '"
                                +(string.IsNullOrEmpty(strFromValue) == false ? strFromValue: "recid")
                                +"' ";// 2015/8/25 
                        }
                        else
                            strColumns = " keystring=id, id, fromstring='recid' ";   // fromstring='' 2011/7/24
                    }

                    // Oracle比较特殊
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        if (string.IsNullOrEmpty(strLimit) == false)
                            strCommand = "SELECT * FROM (select "
    + strColumns // " id "
    + "from " + this.m_strSqlDbName + "_records where id like '__________' and id not in (" + strCommand + ") "
    + strOrderBy    // 2012/3/30
    + ") " + strLimit;
                        else
                            strCommand = "select "
+ strColumns // " id "
+ "from " + this.m_strSqlDbName + "_records where id like '__________' and id not in (" + strCommand + ") "
+ strOrderBy    // 2012/3/30
;
                    }
                    else
                    {
                        strCommand = "select "
    + strTop
    + strColumns // " id "
    + "from records where id like '__________' and id not in (" + strCommand + ") "
    + strOrderBy    // 2012/3/30
    + strLimit;
                    }

#endif
                }
                else
                {
                    if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                        strCommand += " " + strOrderBy;
                    else
                        bNeedSort = true;
                    // TODO: 其他数据库类型，是否在一个select * from () 后面加order by(如果只有一个select语句则不要加外壳)，还是在每个具体的select语句里面加order by?
                }

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    strCommand = "use " + this.m_strSqlDbName + " "
                    + strCommand;
                else if (this.container.SqlServerType == SqlServerType.MySql)
                    strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
                    + strCommand;

                if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    strCommand += " use master " + "\n";

                if (aSqlParameter == null)
                {
                    strError = "一个参数也没 是不可能的情况";
                    return -1;
                }

                nRet = ExecuteQueryFillResultSet(
            handle,
            strCommand,
            aSqlParameter,
            resultSet,
            searchItem.MaxCount,
            GetOutputStyle(strOutputStyle),
            false,
            out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;    // ???
#if NO
                // ***
                Connection connection = new Connection(this,
    this.m_strConnString);
                connection.TryOpen();
                try
                {
                    DbCommand command = null;

                    if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    {
                        command = new SqlCommand(strCommand,
                        connection.SqlConnection);
                    }
                    else if (this.container.SqlServerType == SqlServerType.SQLite)
                    {
                        command = new SQLiteCommand(strCommand,
                        connection.SQLiteConnection);
                    }
                    else if (this.container.SqlServerType == SqlServerType.MySql)
                    {
                        command = new MySqlCommand(strCommand,
                        connection.MySqlConnection);
                    }
                    else if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        command = new OracleCommand(strCommand,
                             connection.OracleConnection);
                        ((OracleCommand)command).BindByName = true;
                    }
                    else
                        throw new ArgumentException("未知的 connection.SqlServerType '" + connection.SqlServerType.ToString() + "'");

                    // ****
                    using (command)
                    {
                        command.CommandTimeout = 20 * 60;  // 把检索时间变大
                        foreach (DbParameter sqlParameter in aSqlParameter)
                        {
                            command.Parameters.Add(sqlParameter);
                        }

                        var reader = command.ExecuteReaderAsync(CommandBehavior.CloseConnection,
    handle.CancelTokenSource.Token).Result;

                        // 从 DbDataReader 中获取和填入记录到一个结果集对象中
                        // return:
                        //      -1  出错
                        //      0   没有填入任何记录
                        //      >0  实际填入的记录条数
                        nRet = FillResultSet(
                                handle,
                                reader,
                                resultSet,
                                searchItem.MaxCount,
                                GetOutputStyle(strOutputStyle),
                                false,
                                out strError);
                        if (nRet == -1 || nRet == 0)
                            return nRet;
                    } // end of using command

                }
                catch (SqlException sqlEx)
                {
                    strError = GetSqlErrors(sqlEx);
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "SearchByUnion() exception: " + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }
                finally // 连接
                {
                    if (connection != null)
                        connection.Close();
                }

#endif
            }
            catch (Exception ex)
            {
                strError = "1: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {

                //*****************对数据库解读锁***************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif

                // 2006/12/18 changed

                TimeSpan delta = DateTime.Now - start_time;
                Debug.WriteLine("SearchByUnion耗时 " + delta.ToString());
            }

            if (bNeedSort == true)
                return 1;

            return 0;
        }

        static string InsertTopPart(string strCommand, string strTop)
        {
            int pos = strCommand.IndexOf("select ");
            if (pos != -1)
                return strCommand.Insert(pos + "select ".Length, " " + strTop + " ");
            return strCommand;
        }

        static void Open(SQLiteConnection connection)
        {
#if REDO_OPEN
            int nRedoCount = 0;
        REDO:
            try
            {
                connection.Open();
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode == SQLiteErrorCode.Busy
                    && nRedoCount < 2)
                {
                    nRedoCount++;
                    goto REDO;
                }
                throw ex;
            }
#else
            connection.Open();
#endif
        }

        // 检查是否要自动升级 SQL 数据库结构
        // 为records表增加newdptimestamp列
        // return:
        //      -1  一般错误
        //      -2  连接错误
        //      0   成功
        internal override int UpdateStructure(out string strError)
        {
            strError = "";

            if (this.container.SqlServerType == SqlServerType.MsSqlServer)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(this.m_strConnString))
                    {
                        connection.Open();

                        /*
                        string strCommand = "use " + this.m_strSqlDbName + "\n"
                            + "IF NOT EXISTS (select * from INFORMATION_SCHEMA.COLUMNS where table_name = 'records' and column_name = 'newdptimestamp')"
                            + "begin\n"
                            + "ALTER TABLE records ADD [newdptimestamp] [nvarchar] (100) NULL\n"
                            + "end\n"
                            + "IF NOT EXISTS (select * from INFORMATION_SCHEMA.COLUMNS where table_name = 'records' and column_name = 'filename')"
                            + "begin\n"
                            + "ALTER TABLE records ADD [filename] [nvarchar] (255) NULL\n"
                            + ", [newfilename] [nvarchar] (255) NULL\n"
                            + "end\n"
                            + "use master\n";
                         * */
                        string strCommand = "use " + this.m_strSqlDbName + "\n"
            + "IF NOT EXISTS (select * from INFORMATION_SCHEMA.COLUMNS where table_name = 'records' and column_name = 'newdptimestamp')"
            + "begin\n"
            + "ALTER TABLE records ADD [newdptimestamp] [nvarchar] (100) NULL\n"
            + ", [filename] [nvarchar] (255) NULL\n"
            + ", [newfilename] [nvarchar] (255) NULL\n"
            + "end\n"
            + "use master\n";

                        using (SqlCommand command = new SqlCommand(strCommand,
                            connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "增加 newdptimestamp 列时出错。\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strCommand;
                                return -1;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    /*
                    if (ex.Errors is SqlErrorCollection)
                        return 0;
                     * */
                    if (ContainsErrorCode(ex, 2) == true)
                    {
                        strError = ex.Message;
                        return -2;
                    }

                    strError = "2: " + ex.Message;
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "3: " + ex.Message;
                    return -1;
                }
            }
            return 0;
        }


        // 根据strStyle风格,得到相就的记录号
        // prev:前一条,next:下一条,如果strID == ? 则prev为第一条,next为最后一条
        // 如果不包含prev和next则不能调此函数
        // parameter:
        //		connection	        连接对象
        //		strCurrentRecordID	当前记录ID
        //		strStyle	        风格
        //      strOutputRecordID   out参数，返回找到的记录号
        //      strError            out参数，返回出错信息
        // return:
        //		-1  出错
        //      0   未找到
        //      1   找到
        // 线：不安全
        private int GetRecordID(Connection connection,
            string strCurrentRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";

            Debug.Assert(connection != null, "GetRecordID()调用错误，connection参数值不能为null。");

            if ((StringUtil.IsInList("prev", strStyle) == false)
                && (StringUtil.IsInList("next", strStyle) == false))
            {
                Debug.Assert(false, "GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
                throw new Exception("GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
            }

            strCurrentRecordID = DbPath.GetID10(strCurrentRecordID);

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strPattern = "N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'";

                string strWhere = "";
                string strOrder = "";
                if ((StringUtil.IsInList("prev", strStyle) == true))
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id<='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else
                    {
                        strWhere = " where id<'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                }
                else if (StringUtil.IsInList("next", strStyle) == true)
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id>='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else
                    {
                        strWhere = " where id>'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                }
                string strCommand = "use " + this.m_strSqlDbName + " "
                    + " SELECT Top 1 id "
                    + " FROM records "
                    + strWhere
                    + strOrder;
                strCommand += " use master " + "\n";

                DateTime start_time = DateTime.Now;

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {
                    using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (dr == null || dr.HasRows == false)
                        {
                            return 0;
                        }
                        else
                        {
                            dr.Read();
                            strOutputRecordID = (string)dr[0];

                            TimeSpan delta = DateTime.Now - start_time;
                            Debug.WriteLine("MS SQL Server 获得数据库 '" + this.GetCaption("zh-CN") + "' 当前尾号耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

                            return 1;
                        }
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                string strPattern = "'__________'";
                string strWhere = "";
                string strOrder = "";
                if ((StringUtil.IsInList("prev", strStyle) == true))
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id<='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else
                    {
                        strWhere = " where id<'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                }
                else if (StringUtil.IsInList("next", strStyle) == true)
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id>='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else
                    {
                        strWhere = " where id>'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                }
                string strCommand = " SELECT id "
                    + " FROM records "
                    + strWhere
                    + strOrder
                    + " LIMIT 1";

                DateTime start_time = DateTime.Now;

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {
                    try
                    {
                        using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            if (dr == null || dr.HasRows == false)
                            {
                                return 0;
                            }
                            else
                            {
                                dr.Read();
                                strOutputRecordID = (string)dr[0];

                                TimeSpan delta = DateTime.Now - start_time;
                                Debug.WriteLine("SQLite 获得数据库 '" + this.GetCaption("zh-CN") + "' 当前尾号耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

                                return 1;
                            }
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                string strPattern = "'__________'";

                string strWhere = "";
                string strOrder = "";
                if ((StringUtil.IsInList("prev", strStyle) == true))
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id<='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else
                    {
                        strWhere = " where id<'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                }
                else if (StringUtil.IsInList("next", strStyle) == true)
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id>='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else
                    {
                        strWhere = " where id>'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                }
                string strCommand = " SELECT id "
                    + " FROM `" + this.m_strSqlDbName + "`.records "
                    + strWhere
                    + strOrder
                    + " LIMIT 1";

                DateTime start_time = DateTime.Now;

                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {

                    using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (dr == null || dr.HasRows == false)
                        {
                            return 0;
                        }
                        else
                        {
                            dr.Read();
                            strOutputRecordID = (string)dr[0];

                            TimeSpan delta = DateTime.Now - start_time;
                            Debug.WriteLine("MySQL 获得数据库 '" + this.GetCaption("zh-CN") + "' 当前尾号耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

                            return 1;
                        }
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                string strPattern = "'__________'";

                string strWhere = "";
                string strOrder = "";
                if ((StringUtil.IsInList("prev", strStyle) == true))
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id<='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                    else
                    {
                        strWhere = " where id<'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id DESC ";
                    }
                }
                else if (StringUtil.IsInList("next", strStyle) == true)
                {
                    if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                    {
                        strWhere = " where id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else if (StringUtil.IsInList("myself", strStyle) == true)
                    {
                        strWhere = " where id>='" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                    else
                    {
                        strWhere = " where id>'" + strCurrentRecordID + "' and id like " + strPattern + " ";
                        strOrder = " ORDER BY id ASC ";
                    }
                }
                string strCommand = "SELECT * FROM (SELECT id "
                    + " FROM " + this.m_strSqlDbName + "_records "
                    + strWhere
                    + strOrder
                    + " ) WHERE rownum <= 1";

                DateTime start_time = DateTime.Now;

                try
                {
                    using (OracleCommand command = new OracleCommand(strCommand,
                        connection.OracleConnection))
                    {

                        using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            if (dr == null || dr.HasRows == false)
                            {
                                return 0;
                            }
                            else
                            {
                                dr.Read();
                                strOutputRecordID = (string)dr[0];

                                TimeSpan delta = DateTime.Now - start_time;
                                Debug.WriteLine("Oracle 获得数据库 '" + this.GetCaption("zh-CN") + "' 当前尾号耗费时间 " + delta.TotalSeconds.ToString() + " 秒");

                                return 1;
                            }
                        }
                    } // end of using command
                }
                catch (OracleException ex)
                {
                    if (ex.Number == 942)
                    {
                        strError = "SQL表 '" + this.m_strSqlDbName + "_records' 不存在";
                        return -1;
                    }
                    throw ex;
                }
            }
            else
            {
                strError = "未知的 connection 类型 '" + connection.SqlServerType.ToString() + "'";
                return -1;
            }
        }

        // 根据strStyle风格,得到相就的记录号
        // prev:前一条,next:下一条,如果strID == ? 则prev为第一条,next为最后一条
        // 如果不包含prev和next则不能调此函数
        // parameter:
        //		strCurrentRecordID	当前记录ID
        //		strStyle	        风格
        //      strOutputRecordID   out参数，返回找到的记录号
        //      strError            out参数，返回出错信息
        // return:
        //		-1  出错
        //      0   未找到
        //      1   找到
        // 线：不安全
        internal override int GetRecordID(string strCurrentRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";

            Connection connection = new Connection(
                this,
                this.m_strConnStringPooling);
            connection.TryOpen();
            try
            {
                // return:
                //		-1  出错
                //      0   未找到
                //      1   找到
                return this.GetRecordID(connection,
                    strCurrentRecordID,
                    strStyle,
                    out strOutputRecordID,
                    out strError);
            }
            catch (SqlException ex)
            {
                if (ex.Errors is SqlErrorCollection)
                    return 0;

                strError = "4: " + ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "5: " + ex.Message;
                return -1;
            }
            finally // 连接
            {
                connection.Close();
            }
        }

        // 按指定范围读Xml
        // parameter:
        //		strRecordID			记录ID
        //		strXPath			用来定位节点的xpath
        //		nStart				从目标读的开始位置
        //		nLength				长度 -1:开始到结束
        //		nMaxLength			限制的最大长度
        //		strStyle			风格,data:取数据 prev:前一条记录 next:后一条记录
        //							withresmetadata属性表示把资源的元数据填到body体里，
        //							同时注意时间戳是两者合并后的时间戳(注:现在已经不是这样, 时间戳互相是独立的)
        //		destBuffer			out参数，返回字节数组
        //		strMetadata			out参数，返回元数据
        //		strOutputResPath	out参数，返回相关记录的路径
        //		outputTimestamp		out参数，返回时间戳
        //		strError			out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      -10 记录局部未找到
        //		>=0 资源总长度
        //      nAdditionError -50 有一个以上下级资源记录不存在
        // 线: 安全的
        public override long GetXml(string strRecordID,
            string strXPath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out string strOutputRecordID,
            out byte[] outputTimestamp,
            bool bCheckAccount,
            out int nAdditionError,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            strOutputRecordID = "";
            outputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            int nRet = 0;
            long lRet = 0;

            int nNotFoundSubRes = 0;    // 下级没有找到的资源个数
            string strNotFoundSubResIds = "";

            // 检查ID
            // return:
            //      -1  出错
            //      0   成功
            nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                out strError);
            if (nRet == -1)
                return -1;

            // 去空白
            strStyle = strStyle.Trim();

#if SUPER
            if (this.FastMode == true)
            {
                // 在读写操作中，整个库都互相排斥
                m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            }
            else
            {
#endif

            //********给库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif

#if SUPER
            }
#endif

            try
            {
                // 取出实际的记录号
                if (StringUtil.IsInList("prev", strStyle) == true
                    || StringUtil.IsInList("next", strStyle) == true)
                {
                    string strTempOutputID = "";

                    // TODO: 这里的Connection可否和后面的合用
                    Connection connection = new Connection(this,
                        this.m_strConnString
#if SUPER
                        ,
                        this.container.SqlServerType == SqlServerType.SQLite && this.FastMode == true ? ConnectionStyle.Global : ConnectionStyle.None
#endif
);

                    connection.TryOpen();
                    try
                    {
                        // return:
                        //		-1  出错
                        //      0   未找到
                        //      1   找到
                        nRet = this.GetRecordID(connection,
                            strRecordID,
                            strStyle,
                            out strTempOutputID,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    finally
                    {
                        connection.Close();
                    }
                    if (nRet == 0 || strTempOutputID == "")
                    {
                        strError = "未找到记录ID '" + strRecordID + "' 的风格为'" + strStyle + "'的记录";
                        return -4;
                    }
                    strRecordID = strTempOutputID;

                    // 再次检查一下返回的ID
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                {
                    if (strRecordID == "-1")
                    {
                        strError = "记录 ID 不能为 '?'。必须是一个明确的数字";
                        return -1;
                    }
                }

                // 根据风格要求，返回资源路径
                if (StringUtil.IsInList("outputpath", strStyle) == true)
                {
                    strOutputRecordID = DbPath.GetCompressedID(strRecordID);
                }


                // 对帐户库开的后门，用于更新帐户,RefreshUser是会调WriteXml()是加锁的函数
                // 不能在开头打开一个connection对象
                if (bCheckAccount == true &&
                    StringUtil.IsInList("account", this.GetDbType()) == true)   // 注意：如果库没有锁定的话应当用this.TypeSafety
                {
                    // 如果要获得记录正好是账户库记录，而且在
                    // UserCollection中，那就把相关的User记录
                    // 保存回数据库，以便稍后从数据库中提取，
                    // 而不必从内存中提取。
                    string strAccountPath = this.FullID + "/" + strRecordID;

                    // return:
                    //		-1  出错
                    //      -4  记录不存在
                    //		0   成功
                    nRet = this.container.UserColl.SaveUserIfNeed(
                        strAccountPath,
                        out strError);
                    if (nRet <= -1)
                        return nRet;
                }

                // 原来的库读锁在这里

                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加读锁。");
#endif
                try //锁
                {

                    Connection connection = new Connection(this,
                        this.m_strConnString
#if SUPER
                        ,
                        this.container.SqlServerType == SqlServerType.SQLite && this.FastMode == true ? ConnectionStyle.Global : ConnectionStyle.None
#endif
);
                    connection.TryOpen();

                    /*
                    // 调试用
                    string strConnectionName = connection.GetHashCode().ToString();
                    this.container.WriteErrorLog("getimage use connection '"+strConnectionName+"'");
                     * */

                    try  //连接
                    {
                        /*
                         * 
                         * 注:直接使用GetImage()函数，也能感知到记录不存在，所以没有必要预先探测一下记录是否存在。2012/1/8
                        // return:
                        //		-1  出错
                        //      0   不存在
                        //      1   存在
                        nRet = this.RecordIsExist(connection,
                            strRecordID,
                            out strError);
                        if (nRet == -1)
                            return -1;


                        if (nRet == 0)
                        {
                            strError = "记录'" + strRecordID + "'在库中不存在";
                            return -4;
                        }
                         * */

                        byte[] baWholeXml = null;
                        byte[] baPreamble = null;

                        string strXml = null;
                        XmlDocument dom = null;

                        if (string.IsNullOrEmpty(strXPath) == false
                            || StringUtil.IsInList("withresmetadata", strStyle) == true)
                        {
                            // 2018/7/16
                            // TODO: 这里要限制一下记录的尺寸，一面内存被爆掉。另外还需要预先看看 metadata，看看记录是不是 XML 内容，如果不是，就不让用 XPath 取得局部

                            // return:
                            //		-1  出错
                            //		-4  记录不存在
                            //      -100    对象文件不存在
                            //		>=0 资源总长度
                            lRet = this.GetImage(connection,
                                null,
                                strRecordID,
                                "",
                                false,  // "data",
                                0,
                                -1,
                                -1,
                                strStyle,
                                out baWholeXml,
                                out strMetadata,
                                out outputTimestamp,
                                out strError);
                            if (lRet <= -1)
                                return lRet;

                            if (baWholeXml == null && string.IsNullOrEmpty(strXPath) == false)
                            {
                                strError = "您虽然使用了 XPath，但未取得数据，可能是因为 strStyle 风格不正确，当前 strStyle 值为 '" + strStyle + "'。";
                                return -1;
                            }

                            strXml = DatabaseUtil.ByteArrayToString(baWholeXml,
                                out baPreamble);

                            if (strXml != "")
                            {
                                dom = new XmlDocument();
                                dom.PreserveWhitespace = true; //设PreserveWhitespace为true

                                try
                                {
                                    dom.LoadXml(strXml);
                                }
                                catch (Exception ex)
                                {
                                    strError = "GetXml() 加载数据到dom出错，原因：" + ex.Message;
                                    return -1;
                                }
                            }
                        }

                        // 带资源元数据的情况，要先提出来xml数据的
                        if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                        {
                            /*
                            // 可以用一个简单的函数包一下
                            // return:
                            //		-1  出错
                            //		-4  记录不存在
                            //      -100    对象文件不存在
                            //		>=0 资源总长度
                            lRet = this.GetImage(connection,
                                strRecordID,
                                "data",
                                0,
                                -1,
                                -1,
                                strStyle,
                                out baWholeXml,
                                out strMetadata,
                                out outputTimestamp,
                                out strError);
                            if (lRet <= -1)
                                return lRet;

                            strXml = DatabaseUtil.ByteArrayToString(baWholeXml,
                                out baPreamble);
                             * */

                            if (dom != null/*strXml != ""*/)
                            {
                                /*
                                dom = new XmlDocument();
                                dom.PreserveWhitespace = true; //设PreserveWhitespace为true

                                try
                                {
                                    dom.LoadXml(strXml);
                                }
                                catch (Exception ex)
                                {
                                    strError = "GetXml() 加载数据到dom出错，原因：" + ex.Message;
                                    return -1;
                                }
                                */

                                // 找到所有的dprms:file元素
                                XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
                                nsmgr.AddNamespace("dprms", DpNs.dprms);
                                XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                                foreach (XmlNode fileNode in fileList)
                                {
                                    string strObjectID = DomUtil.GetAttr(fileNode, "id");
                                    if (strObjectID == "")
                                        continue;

                                    byte[] baObjectDestBuffer;
                                    string strObjectMetadata;
                                    byte[] baObjectOutputTimestamp;

                                    string strObjectFullID = strRecordID + "_" + strObjectID;
                                    // return:
                                    //		-1  出错
                                    //		-4  记录不存在
                                    //      -100    对象文件不存在
                                    //		>=0 资源总长度
                                    lRet = this.GetImage(connection,
                                        null,
                                        strObjectFullID,
                                        "",
                                        false,  // "data",
                                        lStart,
                                        nLength,
                                        nMaxLength,
                                        "metadata,timestamp",//strStyle,
                                        out baObjectDestBuffer,
                                        out strObjectMetadata,
                                        out baObjectOutputTimestamp,
                                        out strError);
                                    if (lRet <= -1)
                                    {
                                        // 资源记录不存在
                                        if (lRet == -4)
                                        {
                                            nNotFoundSubRes++;

                                            if (strNotFoundSubResIds != "")
                                                strNotFoundSubResIds += ",";
                                            strNotFoundSubResIds += strObjectID;
                                        }
                                    }

                                    // 解析metadata
                                    if (strObjectMetadata != "")
                                    {
                                        Hashtable values = StringUtil.ParseMetaDataXml(strObjectMetadata,
                                            out strError);
                                        if (values == null)
                                            return -1;

                                        string strObjectTimestamp = ByteArray.GetHexTimeStampString(baObjectOutputTimestamp);

                                        // TODO: 这几个属性值似乎可以用原属性名加上 __ 构成，不必一句一句写了。以后新增 metadata 参数名就能自动适应
                                        DomUtil.SetAttr(fileNode, "__mime", (string)values["mimetype"]);
                                        DomUtil.SetAttr(fileNode, "__localpath", (string)values["localpath"]);
                                        DomUtil.SetAttr(fileNode, "__size", (string)values["size"]);

                                        // 2016/10/16
                                        string strProcessCommand = (string)values["command"];
                                        if (string.IsNullOrEmpty(strProcessCommand) == false)
                                            DomUtil.SetAttr(fileNode, "__command", strProcessCommand);

                                        // 2007/12/13 
                                        string strLastModifyTime = (string)values["lastmodifytime"];
                                        if (String.IsNullOrEmpty(strLastModifyTime) == false)
                                            DomUtil.SetAttr(fileNode, "__lastmodifytime", strLastModifyTime);

                                        DomUtil.SetAttr(fileNode, "__timestamp", strObjectTimestamp);
                                    }
                                }
                            } // end if (strXml != "")

                        } // if (StringUtil.IsInList("withresmetadata", strStyle) == true)

                        // 通过xpath找片断的情况
                        if (string.IsNullOrEmpty(strXPath) == false)
                        {
                            if (dom != null)
                            {
                                nRet = DatabaseUtil.ParseXPathParameter(strXPath,
                                    out string strLocateXPath,
                                    out string strCreatePath,
                                    out string strNewRecordTemplate,
                                    out string strAction,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                if (strLocateXPath == "")
                                {
                                    strError = "xpath表达式中的locate参数不能为空值";
                                    return -1;
                                }

                                XmlNode node = dom.DocumentElement.SelectSingleNode(strLocateXPath);
                                if (node == null)
                                {
                                    strError = "从dom中未找到XPath为'" + strLocateXPath + "'的节点";
                                    return -10;
                                }

                                string strOutputText = "";
                                if (node.NodeType == XmlNodeType.Element)
                                {
                                    strOutputText = node.OuterXml;
                                }
                                else if (node.NodeType == XmlNodeType.Attribute)
                                {
                                    strOutputText = node.Value;
                                }
                                else
                                {
                                    strError = "通过 XPath '" + strXPath + "' 找到的节点的类型为 '" + node.NodeType.ToString() + "'，属于不支持的情况";
                                    return -1;
                                }

                                byte[] baOutputText = DatabaseUtil.StringToByteArray(strOutputText,
                                    baPreamble);

                                // return:
                                //		-1  出错
                                //		0   成功
                                nRet = ConvertUtil.GetRealLength(lStart,
                                    nLength,
                                    baOutputText.Length,
                                    nMaxLength,
                                    out long lRealLength,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                destBuffer = new byte[lRealLength];

                                Array.Copy(baOutputText,
                                    lStart,
                                    destBuffer,
                                    0,
                                    lRealLength);
                            }
                            else
                            {
                                destBuffer = new byte[0];
                            }

                            return destBuffer.Length;   // 2016/1/3
                        } // end if (strXPath != null && strXPath != "")

                        if (dom != null)
                        {
                            // 带资源元数据的情况，要先提出来xml数据的
                            if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                            {
                                // 使用XmlTextWriter保存成utf8的编码方式
                                using (MemoryStream ms = new MemoryStream())
                                // 2015/11/23 增加的 using 语句
                                using (XmlTextWriter textWriter = new XmlTextWriter(ms, Encoding.UTF8))
                                {
                                    dom.Save(textWriter);
                                    //dom.Save(ms);

                                    long lRealLength;
                                    // return:
                                    //		-1  出错
                                    //		0   成功
                                    nRet = ConvertUtil.GetRealLength(lStart,
                                        nLength,
                                        (int)ms.Length,
                                        nMaxLength,
                                        out lRealLength,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;

                                    destBuffer = new byte[lRealLength];

                                    // 带元素的信息后的总长度
                                    long nWithMetedataTotalLength = ms.Length;

                                    // ms.Seek(lStart, SeekOrigin.Begin);
                                    ms.FastSeek(lStart); // 2017/9/5
                                    ms.Read(destBuffer,
                                        0,
                                        destBuffer.Length);

                                    if (nNotFoundSubRes > 0)
                                    {
                                        strError = "记录" + strRecordID + "中id为 " + strNotFoundSubResIds + " 的下级资源记录不存在";
                                        nAdditionError = -50; // 有一个以上下级资源记录不存在
                                    }

                                    return nWithMetedataTotalLength;
                                }
                            }
                        } // end if (dom != null)
                        else
                        {
                            // 2017/7/5
                            // XML 记录体为空
                            if (baWholeXml != null && baWholeXml.Length == 0)
                            {
                                destBuffer = new byte[0];
                                return 0;
                            }
                        }

                        if (baWholeXml != null)
                        {
                            strError = "dp2Kernel GetXml()函数中 发生了重复 GetImage() 的情况";
                            return -1;
                        }

                        // 不使用xpath的情况
                        // return:
                        //		-1  出错
                        //		-4  记录不存在
                        //      -100    对象文件不存在
                        //		>=0 资源总长度
                        lRet = this.GetImage(connection,
                            null,
                            strRecordID,
                            "",
                            false,  // "data",
                            lStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out destBuffer,
                            out strMetadata,
                            out outputTimestamp,
                            out strError);

                        if (lRet >= 0 && nNotFoundSubRes > 1)
                        {
                            strError = "记录 " + strRecordID + " 中 id 为 " + strNotFoundSubResIds + " 的下级资源记录不存在";
                            nAdditionError = -50; // 有一个以上下级资源记录不存在
                        }

                        return lRet;
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = "取记录 '" + strRecordID + "' 时出错， 原因: " + GetSqlErrors(sqlEx);

                        // TODO: 如果遇到超时错误，是否需要前端重试? 需要把这种错误类型专门分辨出来
                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = "取记录'" + strRecordID + "'出错了，原因:" + sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "取记录 '" + strRecordID + "' 出错了，原因: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally //连接
                    {
                        connection.Close();
                    }
                }
                finally //锁
                {

                    //*********对记录解读锁******
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解读锁。");
#endif
                }
            }
            catch (Exception ex)
            {
                strError = "取记录'" + strRecordID + "'出错了，原因:" + ex.Message;
                return -1;
            }
            finally
            {
#if SUPER
                if (this.FastMode == true)
                {
                    m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
                }
                else
                {
#endif
                //***********对数据库解读锁*****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif

#if SUPER
                }
#endif
            }
        }

        // 得到xml数据
        // 线:安全的,供外部调
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      -100    对象文件不存在
        //      0   正确
        public override int GetXmlData(string strID,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            strID = DbPath.GetID10(strID);

            Connection connection = new Connection(this,
                this.m_strConnStringPooling
#if SUPER
                ,  // 因为中途不会中断，所以可以使用pooling
                this.container.SqlServerType == SqlServerType.SQLite && this.FastMode == true ? ConnectionStyle.Global : ConnectionStyle.None
#endif
);
            connection.TryOpen();
            try
            {
                // return:
                //      -1  出错
                //      -4  记录不存在
                //      -100    对象文件不存在
                //      0   正确
                return this.GetXmlString(connection,
                    strID,
                    out strXml,
                    out strError);
            }
            finally
            {
                connection.Close();
            }
        }


        // 取xml数据到字符串,包装GetXmlData()
        // 线:不安全
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      -100    对象文件不存在
        //      0   正确
        private int GetXmlString(Connection connection,
            string strID,
            out string strXml,
            out string strError)
        {
            byte[] baPreamble;
            // return:
            //      -1  出错
            //      -4  记录不存在
            //      -100    对象文件不存在
            //      0   正确
            return this.GetXmlData(connection,
                null,
                strID,
                false,  // "data",
                out strXml,
                out baPreamble,
                out strError);
        }

        // 得到xml字符串,包装GetImage()
        // 线: 不安全
        // parameters:
        //      row_info    如果row_info != null，则不理会strID参数了
        //		strID       记录ID。用于在rowinfo == null 的情况下获取行信息
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      -100    对象文件不存在
        //      0   正确
        private int GetXmlData(Connection connection,
            RecordRowInfo row_info,
            string strID,
            // string strFieldName,
            bool bTempField,
            out string strXml,
            out byte[] baPreamble,
            out string strError)
        {
            baPreamble = new byte[0];
            strXml = "";
            strError = "";

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            // return:
            //		-1  出错
            //		-4  记录不存在
            //      -100    对象文件不存在
            //		>=0 资源总长度
            long lRet = this.GetImage(connection,
                row_info,
                strID,
                "",
                // strFieldName,
                bTempField,
                0,
                -1,
                -1,
                "data", // style
                out byte[] newXmlBuffer,
                out string strMetadata,
                out byte[] outputTimestamp,
                out strError);
            if (lRet <= -1)
                return (int)lRet;

            strXml = DatabaseUtil.ByteArrayToString(newXmlBuffer,
                out baPreamble);
            return 0;
        }

        // 按指定范围读资源
        // parameter:
        //		strRecordID       记录ID
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  记录不存在
        //		>=0 资源总长度
        public override long GetObject(string strRecordID,
            string strObjectID,
            string strXPath,
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
            outputTimestamp = null;
            strMetadata = "";
            strError = "";

            strRecordID = DbPath.GetID10(strRecordID);
            //********对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加读锁。");
#endif
                try  // 记录锁
                {
                    Connection connection = new Connection(this,
                        this.m_strConnString);
                    connection.TryOpen();
                    try // 连接
                    {
                        string strObjectFullID = strRecordID + "_" + strObjectID;

                        if (string.IsNullOrEmpty(strXPath))
                        {
                            // return:
                            //		-1  出错
                            //		-4  记录不存在
                            //      -100    对象文件不存在
                            //		>=0 资源总长度
                            return this.GetImage(connection,
                                null,
                                strObjectFullID,
                                "",
                                false,  // "data",
                                lStart,
                                nLength,
                                nMaxLength,
                                strStyle,
                                out destBuffer,
                                out strMetadata,
                                out outputTimestamp,
                                out strError);
                        }

                        return this.GetImage(connection,
        null,
        strObjectFullID,
        strXPath,
        false,  // "data",
        lStart,
        nLength,
        nMaxLength,
        strStyle,
        out destBuffer,
        out strMetadata,
        out outputTimestamp,
        out strError);
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);

                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "6: " + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //*************对记录解读锁***********
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解读锁。");
#endif
                }
            }
            finally //库锁
            {
                //******对数据库解读锁*********
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 2012/1/21
        // 获得一个唯一的、连续的范围的长度
        static long GetTotalLength(string strRange,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strRange) == true)
                return 0;

            // 准备rangelist
            RangeList rangeList = null;
            try
            {
                rangeList = new RangeList(strRange);
            }
            catch (Exception ex)
            {
                strError = "用字符串 '" + strRange + "' 创建 RangeList 时出错: " + ex.Message;
                return -1;
            }
            if (rangeList.Count != 1)
            {
                strError = "范围字符串必须是一个连续的起止单元。(但现在是 '" + strRange + "')";
                return -1;
            }
            if (rangeList[0].lStart != 0)
            {
                strError = "范围的开始必须是0。(但现在是 '" + strRange + "')";
                return -1;
            }
            return rangeList[0].lLength;
        }

        // 把整个对象文件写入物理文件。为 PDF 创建单页图像文件做准备
        // parameters:
        //      bFreely [out] 是否为独立文件。独立文件需要在使用后立即删除
        int GetObjectFile(Connection connection,
            string strRecPath,
            string strDataFieldName,
            byte[] textPtr,
            long lTotalLength,
            out bool bFreely,
            out string strObjectFileName,
            out string strError)
        {
            strError = "";
            strObjectFileName = "";
            bFreely = false;

            try
            {
                PageItem item = _pageCache.GetPage(strRecPath, 0, 0, "object_file",
                    () =>
                    {
                        string strTempFileName = this.container.GetTempFileName("obj");
                        // 先把整个对象文件写入一个对象文件
                        using (SqlImageStream source = new SqlImageStream(connection,
                        this.m_strSqlDbName,
                        strDataFieldName,
                        textPtr,
                        lTotalLength))
                        {
                            using (FileStream output = File.Create(strTempFileName))
                            {
                                source.Seek(0, SeekOrigin.Begin);
                                StreamUtil.DumpStream(source, output);
                            }
                        }

                        return new Tuple<string, int>(strTempFileName, (int)lTotalLength);
                    },
                    (filename) =>
                    {
                        this._streamCache.FileDelete(filename);
                    },
                    this.container.KernelApplication._app_down.Token);
                strObjectFileName = item.FilePath;
                Debug.Assert(string.IsNullOrEmpty(strObjectFileName) == false, "");
                if (item.State == "Freely")
                    bFreely = true;
                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetObjectFile() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 将图像文件格式 string 转换为 ImageFormat 对象。jpeg/png 等
        private static ImageFormat GetImageFormat(string format)
        {
            ImageFormat imageFormat = null;

            try
            {
                var imageFormatConverter = new ImageFormatConverter();
                imageFormat = (ImageFormat)imageFormatConverter.ConvertFromString(format);
            }
            catch (Exception)
            {
                return null;
            }

            return imageFormat;
        }

        static readonly string DefaultPageFormat = "jpeg"; // 图像文件格式默认 jpeg

        // 从 PDF 文件中按照 strPartCmd 命令要求，解析出单页图片文件
        // parameters:
        //      strPartCmd  一般为 "page:1,format:png,dpi:300" 形态
        //                  如果为 "page:?" 表示只想获取 PDF 文件中的总页数，并不创建单页图像文件
        // return:
        //      -1  出错
        //      0   成功。仅获得 nPageCount
        //      1   成功。同时获得了单页图像文件
        int GetPageImage(
            string strRecPath,
            string strPdfFileName,
            string strPartCmd,
            out int nPageCount,
            out bool bFreely,
            out string strPageImageFileName,
            out string strError)
        {
            strError = "";
            strPageImageFileName = "";
            nPageCount = 0;
            bFreely = false;

            // page:1
            Hashtable parameters = StringUtil.ParseParameters(strPartCmd, ',', ':', "");
            string strPage = (string)parameters["page"];

            if (strPage == "?")
            {
                try
                {
                    // 单独一次调用检测 PDF 文件中的页码数
                    using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
                    {
                        rasterizer.Open(strPdfFileName, DatabaseCollection.gvi, false);

                        nPageCount = rasterizer.PageCount;

                        rasterizer.Close();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetDebugText(ex);
                    return -1;
                }
                finally
                {
                    GC.Collect();
                }
            }

            if (Int32.TryParse(strPage, out int nPageNo) == false)
            {
                strError = "对象局部描述命令 '" + strPartCmd + "' 格式错误: 子参数 page 值 '" + strPage + "' 应为纯数字";
                return -1;
            }

            string strDPI = (string)parameters["dpi"];
            int nDPI = 100;
            if (string.IsNullOrEmpty(strDPI) == false)
            {
                if (Int32.TryParse(strDPI, out nDPI) == false)
                {
                    strError = "对象局部描述命令 '" + strPartCmd + "' 格式错误: 子参数 dpi 值 '" + strDPI + "' 应为纯数字";
                    return -1;
                }
            }

            string strFormat = (string)parameters["format"];
            if (string.IsNullOrEmpty(strFormat))
                strFormat = DefaultPageFormat;
            ImageFormat format = GetImageFormat(strFormat);
            if (format == null)
            {
                strError = "对象局部描述命令 '" + strPartCmd + "' 格式错误: 无法识别的图像格式名 '" + strFormat + "'";
                return -1;
            }

            try
            {
                PageItem item = _pageCache.GetPage(strRecPath, nPageNo, nDPI, strFormat,
                    () =>
                    {
                        int nTotalPage = 0;
                        string strTempFileName = this.container.GetTempFileName("pgi");
                        using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
                        {
                            rasterizer.Open(strPdfFileName, DatabaseCollection.gvi, false);

                            nTotalPage = rasterizer.PageCount;
                            // 0 表示最后一页
                            if (nPageNo == 0)
                                nPageNo = nTotalPage;

                            if (nPageNo > nTotalPage)
                                throw new Exception("超过页码范围");

                            using (Image img = rasterizer.GetPage(nDPI, nDPI, nPageNo))
                            {
                                img.Save(strTempFileName, format);
                            }
#if NO
                }
                catch (GhostscriptException ex)
                {
                    // .Code == -1000
                    MessageBox.Show(this, ex.Message);
                }
#endif
                            rasterizer.Close();
                        }

                        GC.Collect();
                        return new Tuple<string, int>(strTempFileName, nTotalPage);
                    },
                    (filename) =>
                    {
                        this._streamCache.FileDelete(filename);
                    },
                    this.container.KernelApplication._app_down.Token);

                strPageImageFileName = item.FilePath;
                if (item.State == "Freely")
                    bFreely = true;
                nPageCount = item.TotalPage;
                return 1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 得到一个 pdf 页面图像的指定范围的 byte []
        // parameters:
        //      strPartCmd  描述单页部分的命令。形如 "page:1,format:png,dpi:300"
        //                  如果为 "page:?" 表示只想获取 PDF 文件中的总页数，并不创建单页图像文件
        //      lTotalLength [in][out] 返回的时候，是页面图像的 bytes
        int GetPageImagePart(
            string strRecPath,
            string strObjectFilename,
            string strPartCmd,
            long lStart,
            int nReadLength,
            int nMaxLength,
            ref long lTotalLength,
            ref byte[] destBuffer,
            out string strError)
        {
            strError = "";

            string strPageImageFileName = "";
            bool bFreely = false;

            try
            {
                // 先读出整个单页的 png 图像文件内容
                // return:
                //      -1  出错
                //      0   成功。仅获得 nPageCount
                //      1   成功。同时获得了单页图像文件
                int nRet = GetPageImage(
                    strRecPath,
                    strObjectFilename,
                    strPartCmd,
                    out int nPageCount,
                    out bFreely,
                    out strPageImageFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 0)
                {
                    destBuffer = new byte[0];
                    lTotalLength = nPageCount;
                    return 0;
                }

                FileInfo fi = new FileInfo(strPageImageFileName);
                // 2020/3/1
                fi.Refresh();
                lTotalLength = fi.Length;

                // 得到实际读的长度
                // return:
                //		-1  出错
                //		0   成功
                nRet = ConvertUtil.GetRealLengthNew(lStart,
                nReadLength,
                lTotalLength,
                nMaxLength,
                out long lOutputLength,
                out strError);
                if (nRet == -1)
                    return -1;

                Debug.Assert(lOutputLength < Int32.MaxValue && lOutputLength > Int32.MinValue, "");

                if (lTotalLength == 0)  // 总长度为0
                {
                    destBuffer = new byte[0];
                    // goto END1;
                    return 0;
                }

                // return:
                //      1   成功
                //      -100    文件不存在
                nRet = ReadObjectFile(strPageImageFileName,
        lStart,
        lOutputLength,
        out destBuffer,
        out strError);
                if (nRet < 0)
                    return nRet;
                // goto END1;
                return 0;
            }
            finally
            {
                if (bFreely && string.IsNullOrEmpty(strPageImageFileName) == false)
                    this._streamCache.FileDelete(strPageImageFileName);
            }
        }

        // 按指定范围读资源
        // parameter:
        //      row_info    如果row_info != null，则不理会strID参数了
        //		strID       记录ID。用于在row_info == null的情况下获得行信息
        //      strPartCmd       要限定读取的 pdf 页命令。空 为不使用读取页功能，读出的为对象的 bytes。如果使用了读取页功能，则函数的语义变为读取指定的页的 png 文件“对象”内的指定范围 bytes
        //      bTempField  是否需要从临时 data 字段中提取数据? (在没有reverse的情况下，临时data字段指 newdata 字段)
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		nMaxLength  最大长度,当为-1时,表示不限
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  记录不存在
        //      -100    对象文件不存在
        //		>=0 资源总长度
        private long GetImage(Connection connection,
            RecordRowInfo row_info,
            string strID,
            string strPartCmd,
            // string strImageFieldName,
            bool bTempField,    // 是否需要从临时 data 字段中提取数据?
            long lStart,
            int nReadLength,
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

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            long lTotalLength = 0;

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                byte[] textPtr = null;
                string strDataFieldName = "data";

                bool bObjectFile = false;

                bool bNeedPage = StringUtil.IsInList("metadata", strStyle) == true && string.IsNullOrEmpty(strPartCmd) == false;

                if (row_info != null)
                {
                    bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

                    string strRange = row_info.Range;

                    if (String.IsNullOrEmpty(strRange) == false
        && strRange[0] == '#')
                    {
                        bObjectFile = true;
                        strRange = strRange.Substring(1);

                        lTotalLength = -1;  // 表示待取得
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(strRange) == false
                            && strRange[0] == '!')
                        {
                            bReverse = true;
                            strRange = strRange.Substring(1);
                        }

                        if (bTempField == true)
                            bReverse = !bReverse;

                        strDataFieldName = "data";
                        if (bReverse == true)
                            strDataFieldName = "newdata";

                        if (bReverse == false)
                        {
                            lTotalLength = row_info.data_length;
                            textPtr = row_info.data_textptr;
                        }
                        else
                        {
                            lTotalLength = row_info.newdata_length;
                            textPtr = row_info.newdata_textptr;
                        }
                    }

                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (bReverse == false || bObjectFile == true)
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.TimestampString);
                        else
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.NewTimestampString);
                    }

                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        strMetadata = row_info.Metadata;
                }
                else
                {
                    // 需要即时获得行信息
                    strID = DbPath.GetID10(strID);

                    // 部分命令字符串
                    string strPartComm = "";

                    // 1.textPtr
                    if (StringUtil.IsInList("data", strStyle) == true
                        || bNeedPage)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";

                        strPartComm += " @textPtr=TEXTPTR(data), ";
                        strPartComm += " @textPtrNew=TEXTPTR(newdata)";
                    }

                    // filename 一定要有
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " @filename=filename, ";
                    strPartComm += " @newfilename=newfilename";

                    // 2.length,一定要有
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " @Length=DataLength(data), ";
                    strPartComm += " @LengthNew=DataLength(newdata)";

                    // 3.timestamp
                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " @dptimestamp=dptimestamp,";
                        strPartComm += " @newdptimestamp=newdptimestamp";
                    }

                    // 4.metadata
                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " @metadata=metadata";
                    }

                    // 5.range，一定要有，用于判断方向
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " @range=range";

                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " @testid=id";

                    string strCommand = "";
                    // DataLength()函数int类型
                    strCommand = "use " + this.m_strSqlDbName + " "
                        + " SELECT "
                        + strPartComm + " "
                        + " FROM records WHERE id=@id";

                    strCommand += " use master " + "\n";

                    using (SqlCommand command = new SqlCommand(strCommand,
                        connection.SqlConnection))
                    {
                        SqlParameter idParam =
                            command.Parameters.Add("@id",
                            SqlDbType.NVarChar);
                        idParam.Value = strID;

                        SqlParameter testidParam =
                                command.Parameters.Add("@testid",
                                SqlDbType.NVarChar,
                                255);
                        testidParam.Direction = ParameterDirection.Output;

                        // 1.textPtr
                        SqlParameter textPtrParam = null;
                        SqlParameter textPtrParamNew = null;
                        if (StringUtil.IsInList("data", strStyle) == true
                            || bNeedPage)
                        {
                            textPtrParam =
                                command.Parameters.Add("@textPtr",
                                SqlDbType.VarBinary,
                                16);
                            textPtrParam.Direction = ParameterDirection.Output;

                            textPtrParamNew =
                command.Parameters.Add("@textPtrNew",
                SqlDbType.VarBinary,
                16);
                            textPtrParamNew.Direction = ParameterDirection.Output;
                        }

                        SqlParameter filename = null;
                        SqlParameter newfilename = null;
                        // 
                        filename =
                            command.Parameters.Add("@filename",
                            SqlDbType.NVarChar,
                            255);
                        filename.Direction = ParameterDirection.Output;

                        newfilename =
                            command.Parameters.Add("@newfilename",
                            SqlDbType.NVarChar,
                            255);
                        newfilename.Direction = ParameterDirection.Output;

                        // 2.length,一定要返回
                        SqlParameter lengthParam =
                            command.Parameters.Add("@length",
                            SqlDbType.Int);
                        lengthParam.Direction = ParameterDirection.Output;

                        SqlParameter lengthParamNew =
                            command.Parameters.Add("@lengthNew",
                            SqlDbType.Int);
                        lengthParamNew.Direction = ParameterDirection.Output;

                        // 3.timestamp
                        SqlParameter timestampParam = null;
                        SqlParameter newtimestampParam = null;
                        if (StringUtil.IsInList("timestamp", strStyle) == true)
                        {
                            timestampParam =
                                command.Parameters.Add("@dptimestamp",
                                SqlDbType.NVarChar,
                                100);
                            timestampParam.Direction = ParameterDirection.Output;

                            newtimestampParam =
            command.Parameters.Add("@newdptimestamp",
            SqlDbType.NVarChar,
            100);
                            newtimestampParam.Direction = ParameterDirection.Output;
                        }

                        // 4.metadata
                        SqlParameter metadataParam = null;
                        if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        {
                            metadataParam =
                                command.Parameters.Add("@metadata",
                                SqlDbType.NVarChar,
                                4000);
                            metadataParam.Direction = ParameterDirection.Output;
                        }

                        // 5.range，一定要有
                        SqlParameter rangeParam =
                                command.Parameters.Add("@range",
                                SqlDbType.NVarChar,
                                4000);
                        rangeParam.Direction = ParameterDirection.Output;

                        try
                        {
                            // 执行命令
                            nRet = command.ExecuteNonQuery();
                            /*
                For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. For all other types of statements, the return value is -1. If a rollback occurs, the return value is also -1.

                             * */
                        }
                        catch (Exception ex)
                        {
                            string strConnectionName = command.Connection.GetHashCode().ToString();
                            this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteNonQuery exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                            throw ex;
                        }

                        if (testidParam == null
                            || (testidParam.Value is System.DBNull))
                        {
                            strError = "记录'" + strID + "'在库中不存在";
                            return -4;
                        }

                        // 5.range，一定会返回
                        string strRange = "";
                        if (rangeParam != null
                            && (!(rangeParam.Value is System.DBNull)))
                            strRange = (string)rangeParam.Value;

                        bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

                        if (String.IsNullOrEmpty(strRange) == false
        && strRange[0] == '#')
                        {
                            bObjectFile = true;
                            strRange = strRange.Substring(1);

                            lTotalLength = -1;  // 表示待取得

                            if (row_info == null)
                                row_info = new RecordRowInfo();

                            // 
                            if (filename != null
                                && (!(filename.Value is System.DBNull)))
                            {
                                row_info.FileName = (string)filename.Value;
                            }

                            if (newfilename != null
        && (!(newfilename.Value is System.DBNull)))
                            {
                                row_info.NewFileName = (string)newfilename.Value;
                            }
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(strRange) == false
                                && strRange[0] == '!')
                            {
                                bReverse = true;
                                strRange = strRange.Substring(1);
                            }

                            if (bTempField == true)
                                bReverse = !bReverse;

                            strDataFieldName = "data";
                            if (bReverse == true)
                                strDataFieldName = "newdata";

                            // 1.textPtr
                            if (StringUtil.IsInList("data", strStyle) == true
                                || bNeedPage)
                            {
                                if (bReverse == false)
                                {
                                    if (textPtrParam != null
                                        && (!(textPtrParam.Value is System.DBNull)))
                                    {
                                        textPtr = (byte[])textPtrParam.Value;
                                    }
                                    else
                                    {
                                        textPtr = null; // 2013/2/15
                                        destBuffer = new byte[0];
                                        // return 0;  // 这里提前返回，会造成 timestamp 返回为空
                                    }
                                }
                                else
                                {
                                    if (textPtrParamNew != null
                    && (!(textPtrParamNew.Value is System.DBNull)))
                                    {
                                        textPtr = (byte[])textPtrParamNew.Value;
                                    }
                                    else
                                    {
                                        textPtr = null; // 2013/2/11
                                        destBuffer = new byte[0];
                                        // return 0;   // 这里提前返回，会造成 timestamp 返回为空
                                    }
                                }
                            }

                            // 2.length,一定会返回
                            if (bReverse == false)
                            {
                                if (lengthParam != null
                                    && (!(lengthParam.Value is System.DBNull)))
                                {
                                    lTotalLength = (int)lengthParam.Value;
                                    // TODO: 这句话曾经抛出异常，需要测试捕获 2011/1/7
                                }
                            }
                            else
                            {
                                if (lengthParamNew != null
                    && (!(lengthParamNew.Value is System.DBNull)))
                                    lTotalLength = (int)lengthParamNew.Value;
                            }

                        }

                        // 3.timestamp
                        if (StringUtil.IsInList("timestamp", strStyle) == true)
                        {
                            if (bReverse == false || bObjectFile == true)
                            {
                                if (timestampParam != null)
                                {
                                    if (!(timestampParam.Value is System.DBNull))
                                    {
                                        string strOutputTimestamp = (string)timestampParam.Value;
                                        outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                    }
                                    else
                                    {
                                        // 2008/3/13 
                                        outputTimestamp = null;
                                    }
                                }
                            }
                            else
                            {
                                if (newtimestampParam != null)
                                {
                                    if (!(newtimestampParam.Value is System.DBNull))
                                    {
                                        string strOutputTimestamp = (string)newtimestampParam.Value;
                                        outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                    }
                                    else
                                    {
                                        // 2008/3/13 
                                        outputTimestamp = null;
                                    }
                                }

                            }
                        }

                        // 4.metadata
                        if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        {
                            if (metadataParam != null
                                && (!(metadataParam.Value is System.DBNull)))
                            {
                                strMetadata = (string)metadataParam.Value;
                            }
                        }
                    } // end of using command
                }

                string strObjectFilename = "";
                if (bObjectFile == true)
                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        strError = "数据库尚未配置对象文件目录，但数据记录中出现了引用对象文件的情况";
                        return -1;
                    }

                    if (bTempField == false)
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.FileName) == true)
                        {
                            /*
                            strError = "行信息中没有对象文件 正式文件名";
                            return -1;
                             * */
                            // 尚没有已经完成的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.FileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.FileName);
                    }
                    else
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.NewFileName) == true)
                        {
                            // 尚没有临时的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.NewFileName);
                    }

                    FileInfo fi = new FileInfo(strObjectFilename);
                    // 2020/3/1
                    fi.Refresh();
                    if (fi.Exists == false)
                    {
                        // TODO: 不要直接汇报物理文件名
                        strError = "对象文件 '" + strObjectFilename + "' 不存在";
                        return -100;
                    }
                    lTotalLength = fi.Length;
                }

                // 需要提取数据时,才会取数据
                if (StringUtil.IsInList("data", strStyle) == true
                    || bNeedPage)
                {
                    // 从对象文件读取指定 pdf 页码内容
                    if (string.IsNullOrEmpty(strPartCmd) == false && bObjectFile == true)
                    {
                        // 得到一个 pdf 页面图像的指定范围的 byte []
                        nRet = GetPageImagePart(
                            GetCacheRecPath(strID),
                            strObjectFilename,
                            strPartCmd,
                            lStart,
                            nReadLength,
                            nMaxLength,
                            ref lTotalLength,
                            ref destBuffer,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        goto END1;
                    }

                    if (string.IsNullOrEmpty(strPartCmd) == false && bObjectFile == false)
                    {
                        bool bFreely = false;
                        string strObjectFilePath = "";

                        try
                        {
                            nRet = GetObjectFile(connection,
        this.GetCacheRecPath(strID),    // this.m_strSqlDbName + "/" + strID,
        strDataFieldName,
        textPtr,
        lTotalLength,
        out bFreely,
        out strObjectFilePath,
        out strError);
                            if (nRet == -1)
                                return -1;

                            // 得到一个 pdf 页面图像的指定范围的 byte []
                            nRet = GetPageImagePart(
                                GetCacheRecPath(strID),
                                strObjectFilePath,
                                strPartCmd,
                                lStart,
                                nReadLength,
                                nMaxLength,
                                ref lTotalLength,
                                ref destBuffer,
                                out strError);
                            if (nRet == -1)
                                return -1;

                        }
                        finally
                        {
                            // 释放对象文件
                            if (bFreely && string.IsNullOrEmpty(strObjectFilePath) == false)
                                _streamCache.FileDelete(strObjectFilePath);
                        }

                        goto END1;
                    }

                    if (nReadLength == 0)  // 取0长度
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;    // >= 0
                        goto END1;
                    }

                    // 得到实际读的长度
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = ConvertUtil.GetRealLengthNew(lStart,
                nReadLength,
                lTotalLength,
                nMaxLength,
                out long lOutputLength,
                out strError);
                    if (nRet == -1)
                        return -1;

                    Debug.Assert(lOutputLength < Int32.MaxValue && lOutputLength > Int32.MinValue, "");

                    // 2012/1/21
                    if (lTotalLength == 0)  // 总长度为0
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;
                        goto END1;
                    }

                    // 从对象文件读取
                    if (bObjectFile == true)
                    {
                        // return:
                        //      1   成功
                        //      -100    文件不存在
                        nRet = ReadObjectFile(strObjectFilename,
            lStart,
            lOutputLength,
            out destBuffer,
            out strError);
                        if (nRet < 0)
                            return nRet;
                        goto END1;

#if NO
                        Debug.Assert(string.IsNullOrEmpty(strObjectFilename) == false, "");

                        destBuffer = new Byte[lOutputLength];

                        try
                        {
                            using (FileStream s = File.Open(
            strObjectFilename,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite))
                            {
                                // s.Seek(lStart, SeekOrigin.Begin);
                                s.FastSeek(lStart); // 2017/9/5
                                s.Read(destBuffer,
                                    0,
                                    (int)lOutputLength);

                                // lTotalLength = s.Length;
                            }
                        }
                        catch (FileNotFoundException /* ex */)
                        {
                            // TODO: 不要直接汇报物理文件名
                            strError = "对象文件 '" + strObjectFilename + "' 不存在";
                            return -100;
                        }
                        // return lTotalLength;
                        goto END1;
#endif
                    }

                    if (textPtr == null)
                    {
                        strError = "textPtr为null";
                        return -1;
                    }

                    // READTEXT命令:
                    // text_ptr: 有效文本指针。text_ptr 必须是 binary(16)。
                    // offset:   开始读取image数据之前跳过的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。
                    //			 使用 ntext 数据类型时，offset 是在开始读取数据前跳过的字符数。
                    //			 使用 text 或 image 数据类型时，offset 是在开始读取数据前跳过的字节数。
                    // size:     是要读取数据的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。如果 size 是 0，则表示读取了 4 KB 字节的数据。
                    // HOLDLOCK: 使文本值一直锁定到事务结束。其他用户可以读取该值，但是不能对其进行修改。

                    string strCommand = "use " + this.m_strSqlDbName + " "
                       + " READTEXT records." + strDataFieldName
                       + " @text_ptr"
                       + " @offset"
                       + " @size"
                       + " HOLDLOCK";

                    strCommand += " use master " + "\n";

                    using (SqlCommand command = new SqlCommand(strCommand,
                        connection.SqlConnection))
                    {

                        SqlParameter text_ptrParam =
                            command.Parameters.Add("@text_ptr",
                            SqlDbType.VarBinary,
                            16);
                        text_ptrParam.Value = textPtr;

                        SqlParameter offsetParam =
                            command.Parameters.Add("@offset",
                            SqlDbType.Int);  // old Int
                        offsetParam.Value = lStart;

                        SqlParameter sizeParam =
                            command.Parameters.Add("@size",
                            SqlDbType.Int);  // old Int
                        sizeParam.Value = lOutputLength;

                        destBuffer = new Byte[lOutputLength];

                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            try
                            {
                                dr.Read();
                                dr.GetBytes(0,
                                    0,
                                    destBuffer,
                                    0,
                                    System.Convert.ToInt32(sizeParam.Value));
                            }
                            catch (Exception ex)
                            {
                                string strConnectionName = command.Connection.GetHashCode().ToString();
                                this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteReader exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                                throw ex;
                            }
                        }
                    } // end of using command
                }

                // return lTotalLength;
                goto END1;
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                bool bObjectFile = false;

                if (row_info != null)
                {
                    string strRange = row_info.Range;

                    if (String.IsNullOrEmpty(strRange) == false
        && strRange[0] == '#')
                    {
                        bObjectFile = true;
                        strRange = strRange.Substring(1);

                        lTotalLength = -1;  // 表示待取得
                    }
                    else
                    {
                        bObjectFile = true;
                    }

                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (bObjectFile == true)
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.TimestampString);
                        else
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.NewTimestampString);
                    }

                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        strMetadata = row_info.Metadata;
                }
                else
                {
                    // 需要即时获得行信息
                    strID = DbPath.GetID10(strID);

                    // 部分命令字符串
                    string strPartComm = "";
                    int nColIndex = 0;

                    // filename 一定要有
                    int nFileNameColIndex = -1;
                    int nNewFileNameColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " filename, ";
                    nFileNameColIndex = nColIndex++;
                    strPartComm += " newfilename";
                    nNewFileNameColIndex = nColIndex++;

                    // 3.timestamp
                    int nTimestampColIndex = -1;
                    int nNewTimestampColIndex = -1;
                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " dptimestamp,";
                        nTimestampColIndex = nColIndex++;
                        strPartComm += " newdptimestamp";
                        nNewTimestampColIndex = nColIndex++;
                    }
                    // 4.metadata
                    int nMetadataColIndex = -1;
                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " metadata";
                        nMetadataColIndex = nColIndex++;
                    }
                    // 5.range，一定要有，用于判断方向
                    int nRangeColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " range";
                    nRangeColIndex = nColIndex++;

                    int nIdColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " id";
                    nIdColIndex = nColIndex++;

                    string strCommand = "";
                    // DataLength()函数int类型
                    strCommand = " SELECT "
                        + strPartComm + " "
                        + " FROM records WHERE id=@id";

                    using (SQLiteCommand command = new SQLiteCommand(strCommand,
                        connection.SQLiteConnection))
                    {

                        SQLiteParameter idParam =
                            command.Parameters.Add("@id",
                            DbType.String);
                        idParam.Value = strID;

                        try
                        {
                            // 执行命令
                            using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                if (dr == null || dr.HasRows == false)
                                {
                                    strError = "记录 '" + strID + "' 在库中不存在";
                                    return -4;
                                }

                                dr.Read();

                                // 5.range，一定会返回
                                string strRange = "";

                                if (!dr.IsDBNull(nRangeColIndex))
                                    strRange = (string)dr[nRangeColIndex];

                                bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

                                if (String.IsNullOrEmpty(strRange) == false
                && strRange[0] == '#')
                                {
                                    bObjectFile = true;
                                    strRange = strRange.Substring(1);

                                    lTotalLength = -1;  // 表示待取得

                                    if (row_info == null)
                                        row_info = new RecordRowInfo();
                                    // 
                                    if (nFileNameColIndex != -1 && !dr.IsDBNull(nFileNameColIndex))
                                    {
                                        row_info.FileName = (string)dr[nFileNameColIndex];
                                    }

                                    if (nNewFileNameColIndex != -1 && !dr.IsDBNull(nNewFileNameColIndex))
                                    {
                                        row_info.NewFileName = (string)dr[nNewFileNameColIndex];
                                    }
                                }

                                // 注意，row_info 有可能为空

                                // 3.timestamp
                                if (StringUtil.IsInList("timestamp", strStyle) == true)
                                {
                                    if (bReverse == false || bObjectFile == true)
                                    {
                                        if (nTimestampColIndex != -1 && !dr.IsDBNull(nTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                    else
                                    {
                                        if (nNewTimestampColIndex != -1 && !dr.IsDBNull(nNewTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nNewTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                }

                                // 4.metadata
                                if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                                {
                                    if (nMetadataColIndex != -1 && !dr.IsDBNull(nMetadataColIndex))
                                    {
                                        strMetadata = (string)dr[nMetadataColIndex];
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string strConnectionName = command.Connection.GetHashCode().ToString();
                            this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteNonQuery exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                            throw ex;
                        }
                    } // end of using command
                }

                string strObjectFilename = "";

                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        strError = "数据库尚未配置对象文件目录，但数据记录中出现了引用对象文件的情况";
                        return -1;
                    }

                    if (bTempField == false)
                    {
                        if (row_info == null
                            || string.IsNullOrEmpty(row_info.FileName) == true)
                        {
                            /*
                            strError = "行信息中没有对象文件 正式文件名";
                            return -1;
                             * */
                            // 尚没有已经完成的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.FileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.FileName);
                    }
                    else
                    {
                        if (row_info == null
                            || string.IsNullOrEmpty(row_info.NewFileName) == true)
                        {
                            // 尚没有临时的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.NewFileName);
                    }

                    int nRedoCount = 0;
                    REDO:
                    FileInfo fi = new FileInfo(strObjectFilename);
                    // 2020/3/1
                    fi.Refresh();
                    if (fi.Exists == false)
                    {
                        // 尝试补救一下
                        string strTempFileName = strObjectFilename + ".bak";
                        if (nRedoCount > 2 || File.Exists(strTempFileName) == false)
                        {
                            // TODO: 不要直接汇报物理文件名
                            strError = "对象文件 '" + strObjectFilename + "' 不存在";
                            return -100;
                        }
                        else
                        {
                            File.Copy(strTempFileName, strObjectFilename);
                            nRedoCount++;
                            goto REDO;
                        }
                    }
                    lTotalLength = fi.Length;
                }

                bool bNeedPage = StringUtil.IsInList("metadata", strStyle) == true && string.IsNullOrEmpty(strPartCmd) == false;

                // 需要提取数据时,才会取数据
                if (StringUtil.IsInList("data", strStyle) == true
                    || bNeedPage)
                {
                    // 从对象文件读取指定 pdf 页码内容
                    if (string.IsNullOrEmpty(strPartCmd) == false && bObjectFile == true)
                    {
                        // 得到一个 pdf 页面图像的指定范围的 byte []
                        nRet = GetPageImagePart(
                            GetCacheRecPath(strID),
                            strObjectFilename,
                            strPartCmd,
                            lStart,
                            nReadLength,
                            nMaxLength,
                            ref lTotalLength,
                            ref destBuffer,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        goto END1;
                    }

                    if (nReadLength == 0)  // 取0长度
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;    // >= 0
                        goto END1;
                    }

                    // 得到实际读的长度
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = ConvertUtil.GetRealLengthNew(lStart,
                        nReadLength,
                        lTotalLength,
                        nMaxLength,
                        out long lOutputLength,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 2012/1/21
                    if (lTotalLength == 0)  // 总长度为0
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;
                        goto END1;
                    }

                    // 从对象文件读取
                    if (bObjectFile == true)
                    {
                        // return:
                        //      1   成功
                        //      -100    文件不存在
                        nRet = ReadObjectFile(strObjectFilename,
            lStart,
            lOutputLength,
            out destBuffer,
            out strError);
                        if (nRet < 0)
                            return nRet;
#if NO
                        Debug.Assert(string.IsNullOrEmpty(strObjectFilename) == false, "");

                        destBuffer = new Byte[lOutputLength];

                        try
                        {
                            using (FileStream s = File.Open(
            strObjectFilename,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite))
                            {
                                // s.Seek(lStart, SeekOrigin.Begin);
                                s.FastSeek(lStart); // 2017/9/5
                                s.Read(destBuffer,
                                    0,
                                    (int)lOutputLength);

                                // lTotalLength = s.Length;
                            }
                        }
                        catch (FileNotFoundException /* ex */)
                        {
                            // TODO: 不要直接汇报物理文件名
                            strError = "对象文件 '" + strObjectFilename + "' 不存在";
                            return -100;
                        }
                        // return lTotalLength;
                        goto END1;
#endif
                    }

                }

                // return lTotalLength;
                goto END1;
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                bool bObjectFile = false;

                if (row_info != null)
                {
                    string strRange = row_info.Range;

                    if (String.IsNullOrEmpty(strRange) == false
        && strRange[0] == '#')
                    {
                        bObjectFile = true;
                        strRange = strRange.Substring(1);

                        lTotalLength = -1;  // 表示待取得
                    }
                    else
                    {
                        bObjectFile = true;
                    }

                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (bObjectFile == true)
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.TimestampString);
                        else
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.NewTimestampString);
                    }

                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        strMetadata = row_info.Metadata;
                }
                else
                {
                    // 需要即时获得行信息
                    strID = DbPath.GetID10(strID);

                    // 部分命令字符串
                    string strPartComm = "";
                    int nColIndex = 0;

                    // filename 一定要有
                    int nFileNameColIndex = -1;
                    int nNewFileNameColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " filename, ";
                    nFileNameColIndex = nColIndex++;
                    strPartComm += " newfilename";
                    nNewFileNameColIndex = nColIndex++;

                    // 3.timestamp
                    int nTimestampColIndex = -1;
                    int nNewTimestampColIndex = -1;
                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " dptimestamp,";
                        nTimestampColIndex = nColIndex++;
                        strPartComm += " newdptimestamp";
                        nNewTimestampColIndex = nColIndex++;
                    }
                    // 4.metadata
                    int nMetadataColIndex = -1;
                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " metadata";
                        nMetadataColIndex = nColIndex++;
                    }
                    // 5.range，一定要有，用于判断方向
                    int nRangeColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " `range`";
                    nRangeColIndex = nColIndex++;

                    int nIdColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " id";
                    nIdColIndex = nColIndex++;

                    string strCommand = "";
                    // DataLength()函数int类型
                    strCommand = " SELECT "
                        + strPartComm + " "
                        + " FROM `" + this.m_strSqlDbName + "`.records WHERE id=@id";

                    using (MySqlCommand command = new MySqlCommand(strCommand,
                        connection.MySqlConnection))
                    {

                        MySqlParameter idParam =
                            command.Parameters.Add("@id",
                            MySqlDbType.String);
                        idParam.Value = strID;

                        try
                        {
                            // 执行命令
                            using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                if (dr == null || dr.HasRows == false)
                                {
                                    strError = "记录 '" + strID + "' 在库中不存在";
                                    return -4;
                                }

                                dr.Read();

                                // 5.range，一定会返回
                                string strRange = "";

                                if (!dr.IsDBNull(nRangeColIndex))
                                    strRange = (string)dr[nRangeColIndex];

                                bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

                                if (String.IsNullOrEmpty(strRange) == false
                && strRange[0] == '#')
                                {
                                    bObjectFile = true;
                                    strRange = strRange.Substring(1);

                                    lTotalLength = -1;  // 表示待取得

                                    if (row_info == null)
                                        row_info = new RecordRowInfo();
                                    // 
                                    if (nFileNameColIndex != -1 && !dr.IsDBNull(nFileNameColIndex))
                                    {
                                        row_info.FileName = (string)dr[nFileNameColIndex];
                                    }

                                    if (nNewFileNameColIndex != -1 && !dr.IsDBNull(nNewFileNameColIndex))
                                    {
                                        row_info.NewFileName = (string)dr[nNewFileNameColIndex];
                                    }
                                }

                                // 3.timestamp
                                if (StringUtil.IsInList("timestamp", strStyle) == true)
                                {
                                    if (bReverse == false || bObjectFile == true)
                                    {
                                        if (nTimestampColIndex != -1 && !dr.IsDBNull(nTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                    else
                                    {
                                        if (nNewTimestampColIndex != -1 && !dr.IsDBNull(nNewTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nNewTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                }

                                // 4.metadata
                                if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                                {
                                    if (nMetadataColIndex != -1 && !dr.IsDBNull(nMetadataColIndex))
                                    {
                                        strMetadata = (string)dr[nMetadataColIndex];
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            string strConnectionName = command.Connection.GetHashCode().ToString();
                            this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteNonQuery exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                            throw ex;
                        }
                    } // end of using command
                }

                string strObjectFilename = "";

                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        strError = "数据库尚未配置对象文件目录，但数据记录中出现了引用对象文件的情况";
                        return -1;
                    }

                    if (bTempField == false)
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.FileName) == true)
                        {
                            /*
                            strError = "行信息中没有对象文件 正式文件名";
                            return -1;
                             * */
                            // 尚没有已经完成的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.FileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.FileName);
                    }
                    else
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.NewFileName) == true)
                        {
                            // 尚没有临时的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.NewFileName);
                    }

                    FileInfo fi = new FileInfo(strObjectFilename);
                    // 2020/3/1
                    fi.Refresh();
                    if (fi.Exists == false)
                    {
                        // TODO: 不要直接汇报物理文件名
                        strError = "对象文件 '" + strObjectFilename + "' 不存在";
                        return -100;
                    }
                    lTotalLength = fi.Length;
                }

                bool bNeedPage = StringUtil.IsInList("metadata", strStyle) == true && string.IsNullOrEmpty(strPartCmd) == false;

                // 需要提取数据时,才会取数据
                if (StringUtil.IsInList("data", strStyle) == true
                    || bNeedPage)
                {
                    // 从对象文件读取指定 pdf 页码内容
                    if (string.IsNullOrEmpty(strPartCmd) == false && bObjectFile == true)
                    {
                        // 得到一个 pdf 页面图像的指定范围的 byte []
                        nRet = GetPageImagePart(GetCacheRecPath(strID),
                            strObjectFilename,
                            strPartCmd,
                            lStart,
                            nReadLength,
                            nMaxLength,
                            ref lTotalLength,
                            ref destBuffer,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        goto END1;
                    }

                    if (nReadLength == 0)  // 取0长度
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;    // >= 0
                        goto END1;
                    }

                    // 得到实际读的长度
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = ConvertUtil.GetRealLengthNew(lStart,
                        nReadLength,
                        lTotalLength,
                        nMaxLength,
                        out long lOutputLength,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 2012/1/21
                    if (lTotalLength == 0)  // 总长度为0
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;
                        goto END1;
                    }

                    // 从对象文件读取
                    if (bObjectFile == true)
                    {
                        // return:
                        //      1   成功
                        //      -100    文件不存在
                        nRet = ReadObjectFile(strObjectFilename,
            lStart,
            lOutputLength,
            out destBuffer,
            out strError);
                        if (nRet < 0)
                            return nRet;
#if NO
                        Debug.Assert(string.IsNullOrEmpty(strObjectFilename) == false, "");

                        destBuffer = new Byte[lOutputLength];

                        try
                        {
                            using (FileStream s = File.Open(
            strObjectFilename,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite))
                            {
                                // s.Seek(lStart, SeekOrigin.Begin);
                                s.FastSeek(lStart); // 2017/9/5
                                s.Read(destBuffer,
                                    0,
                                    (int)lOutputLength);

                                // lTotalLength = s.Length;
                            }
                        }
                        catch (FileNotFoundException /* ex */)
                        {
                            // TODO: 不要直接汇报物理文件名
                            strError = "对象文件 '" + strObjectFilename + "' 不存在";
                            return -100;
                        }
                        // return lTotalLength;
                        goto END1;
#endif
                    }

                }

                // return lTotalLength;
                goto END1;
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                // 注： Oracle 这里和 MySql 基本一样
                bool bObjectFile = false;

                if (row_info != null)
                {
                    string strRange = row_info.Range;

                    if (String.IsNullOrEmpty(strRange) == false
        && strRange[0] == '#')
                    {
                        bObjectFile = true;
                        strRange = strRange.Substring(1);

                        lTotalLength = -1;  // 表示待取得
                    }
                    else
                    {
                        bObjectFile = true;
                    }

                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (bObjectFile == true)
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.TimestampString);
                        else
                            outputTimestamp = ByteArray.GetTimeStampByteArray(row_info.NewTimestampString);
                    }

                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                        strMetadata = row_info.Metadata;
                }
                else
                {
                    // 需要即时获得行信息
                    strID = DbPath.GetID10(strID);

                    // 部分命令字符串
                    string strPartComm = "";
                    int nColIndex = 0;

                    // filename 一定要有
                    int nFileNameColIndex = -1;
                    int nNewFileNameColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " filename, ";
                    nFileNameColIndex = nColIndex++;
                    strPartComm += " newfilename";
                    nNewFileNameColIndex = nColIndex++;

                    // 3.timestamp
                    int nTimestampColIndex = -1;
                    int nNewTimestampColIndex = -1;
                    if (StringUtil.IsInList("timestamp", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " dptimestamp,";
                        nTimestampColIndex = nColIndex++;
                        strPartComm += " newdptimestamp";
                        nNewTimestampColIndex = nColIndex++;
                    }
                    // 4.metadata
                    int nMetadataColIndex = -1;
                    if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                    {
                        if (string.IsNullOrEmpty(strPartComm) == false)
                            strPartComm += ",";
                        strPartComm += " metadata";
                        nMetadataColIndex = nColIndex++;
                    }
                    // 5.range，一定要有，用于判断方向
                    int nRangeColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " range";
                    nRangeColIndex = nColIndex++;

                    int nIdColIndex = -1;
                    if (string.IsNullOrEmpty(strPartComm) == false)
                        strPartComm += ",";
                    strPartComm += " id";
                    nIdColIndex = nColIndex++;

                    string strCommand = "";
                    // DataLength()函数int类型
                    strCommand = " SELECT "
                        + strPartComm + " "
                        + " FROM " + this.m_strSqlDbName + "_records WHERE id=:id";

                    using (OracleCommand command = new OracleCommand(strCommand,
                        connection.OracleConnection))
                    {
                        OracleParameter idParam =
                            command.Parameters.Add(":id",
                            OracleDbType.NVarchar2);
                        idParam.Value = strID;

                        try
                        {
                            // 执行命令
                            using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                if (dr == null || dr.HasRows == false)
                                {
                                    strError = "记录 '" + strID + "' 在库中不存在";
                                    return -4;
                                }

                                dr.Read();

                                // 5.range，一定会返回
                                string strRange = "";

                                if (!dr.IsDBNull(nRangeColIndex))
                                    strRange = (string)dr[nRangeColIndex];

                                bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

                                if (String.IsNullOrEmpty(strRange) == false
                && strRange[0] == '#')
                                {
                                    bObjectFile = true;
                                    strRange = strRange.Substring(1);

                                    lTotalLength = -1;  // 表示待取得

                                    if (row_info == null)
                                        row_info = new RecordRowInfo();
                                    // 
                                    if (nFileNameColIndex != -1 && !dr.IsDBNull(nFileNameColIndex))
                                    {
                                        row_info.FileName = (string)dr[nFileNameColIndex];
                                    }

                                    if (nNewFileNameColIndex != -1 && !dr.IsDBNull(nNewFileNameColIndex))
                                    {
                                        row_info.NewFileName = (string)dr[nNewFileNameColIndex];
                                    }
                                }

                                // 3.timestamp
                                if (StringUtil.IsInList("timestamp", strStyle) == true)
                                {
                                    if (bReverse == false || bObjectFile == true)
                                    {
                                        if (nTimestampColIndex != -1 && !dr.IsDBNull(nTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                    else
                                    {
                                        if (nNewTimestampColIndex != -1 && !dr.IsDBNull(nNewTimestampColIndex))
                                        {
                                            string strOutputTimestamp = (string)dr[nNewTimestampColIndex];
                                            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                                        }
                                        else
                                            outputTimestamp = null;
                                    }
                                }

                                // 4.metadata
                                if (StringUtil.IsInList("metadata", strStyle) == true
                        || StringUtil.IsInList("incReadCount", strStyle) == true)
                                {
                                    if (nMetadataColIndex != -1 && !dr.IsDBNull(nMetadataColIndex))
                                    {
                                        strMetadata = (string)dr[nMetadataColIndex];
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string strConnectionName = command.Connection.GetHashCode().ToString();
                            this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteNonQuery exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                            throw ex;
                        }

                    } // end of using command
                }

                string strObjectFilename = "";

                {
                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        strError = "数据库尚未配置对象文件目录，但数据记录中出现了引用对象文件的情况";
                        return -1;
                    }

                    if (bTempField == false)
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.FileName) == true)
                        {
                            /*
                            strError = "行信息中没有对象文件 正式文件名";
                            return -1;
                             * */
                            // 尚没有已经完成的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.FileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.FileName);
                    }
                    else
                    {
                        if (row_info == null || // 2017/6/2
                            string.IsNullOrEmpty(row_info.NewFileName) == true)
                        {
                            // 尚没有临时的对象文件
                            destBuffer = new byte[0];
                            return 0;
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");

                        strObjectFilename = GetObjectFileName(row_info.NewFileName);
                    }

                    FileInfo fi = new FileInfo(strObjectFilename);
                    // 2020/3/1
                    fi.Refresh();
                    if (fi.Exists == false)
                    {
                        // TODO: 不要直接汇报物理文件名
                        strError = "对象文件 '" + strObjectFilename + "' 不存在";
                        return -100;
                    }
                    lTotalLength = fi.Length;
                }

                bool bNeedPage = StringUtil.IsInList("metadata", strStyle) == true && string.IsNullOrEmpty(strPartCmd) == false;

                // 需要提取数据时,才会取数据
                if (StringUtil.IsInList("data", strStyle) == true
                    || bNeedPage)
                {
                    // 从对象文件读取指定 pdf 页码内容
                    if (string.IsNullOrEmpty(strPartCmd) == false && bObjectFile == true)
                    {
                        // 得到一个 pdf 页面图像的指定范围的 byte []
                        nRet = GetPageImagePart(
                            GetCacheRecPath(strID),
                            strObjectFilename,
                            strPartCmd,
                            lStart,
                            nReadLength,
                            nMaxLength,
                            ref lTotalLength,
                            ref destBuffer,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        goto END1;
                    }

                    if (nReadLength == 0)  // 取0长度
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;    // >= 0
                        goto END1;
                    }

                    // 得到实际读的长度
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = ConvertUtil.GetRealLengthNew(lStart,
                        nReadLength,
                        lTotalLength,
                        nMaxLength,
                        out long lOutputLength,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 2012/1/21
                    if (lTotalLength == 0)  // 总长度为0
                    {
                        destBuffer = new byte[0];
                        // return lTotalLength;
                        goto END1;
                    }

                    // 从对象文件读取
                    if (bObjectFile == true)
                    {
                        // return:
                        //      1   成功
                        //      -100    文件不存在
                        nRet = ReadObjectFile(strObjectFilename,
            lStart,
            lOutputLength,
            out destBuffer,
            out strError);
                        if (nRet < 0)
                            return nRet;
#if NO
                        Debug.Assert(string.IsNullOrEmpty(strObjectFilename) == false, "");

                        destBuffer = new Byte[lOutputLength];

                        try
                        {
                            using (FileStream s = File.Open(
            strObjectFilename,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite))
                            {
                                // s.Seek(lStart, SeekOrigin.Begin);
                                s.FastSeek(lStart); // 2017/9/5
                                s.Read(destBuffer,
                                    0,
                                    (int)lOutputLength);

                                // lTotalLength = s.Length;
                            }
                        }
                        catch (FileNotFoundException /* ex */)
                        {
                            // TODO: 不要直接汇报物理文件名
                            strError = "对象文件 '" + strObjectFilename + "' 不存在";
                            return -100;
                        }
                        // return lTotalLength;
                        goto END1;
#endif
                    }

                }

                // return lTotalLength;
                goto END1;
            }
            strError = "未知的 connection 类型 '" + connection.SqlServerType.ToString() + "'";
            return -1;
            END1:
            int nBufferLength = 0;
            if (destBuffer != null)
                nBufferLength = destBuffer.Length;
            if (StringUtil.IsInList("incReadCount", strStyle) == true
                && lStart + nBufferLength >= lTotalLength && lTotalLength != -1)
            {
                // return:
                //		-1	出错
                //		0	成功
                nRet = DatabaseUtil.MergeMetadata(strMetadata,
                    "",
                    -2,
                    "+1",
                    out string strResultMetadata,
                    out strError);
                if (nRet == -1)
                    return -1;
                // 将 metadata 写入 records 表
                // parameters:
                nRet = WriteMetadataColumn(
            connection,
            strID,
            strResultMetadata,
            out strError);
                if (nRet == -1)
                    return -1;

                if (StringUtil.IsInList("metadata", strStyle) == false)
                    strMetadata = "";
            }

            // PDF 单页图像时要修正用于返回的 metadata。mimetype size localpath
            if (string.IsNullOrEmpty(strPartCmd) == false)
            {
                nRet = DatabaseUtil.ChangeMetadata(strMetadata,
        lTotalLength,
        GetMime(strPartCmd),
        null,
        out string strResult,
        out strError);
                if (nRet == -1)
                    return -1;
                strMetadata = strResult;
            }
            return lTotalLength;
        }

        // 从单页图像描述命令中，获得 MIME 信息
        static string GetMime(string strPartCmd)
        {
            Hashtable parameters = StringUtil.ParseParameters(strPartCmd, ',', ':', "");
            string strFormat = (string)parameters["format"];
            if (string.IsNullOrEmpty(strFormat))
                strFormat = DefaultPageFormat;
            switch (strFormat)
            {
                case "jpeg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "image/bmp";
                case "tif":
                case "tiff":
                    return "image/tiff";
                case "emf":
                    return "image/x-emf";
                case "wmf":
                    return "image/x-wmf";
                case "exif":
                    return "image/x-exif";
                case "icon":
                    return "image/x-icon";
            }

            return null;
        }
#if NO
        // 按指定范围读资源
        // GetBytes()版本。下载大尺寸对象的时候速度非常慢
        // parameter:
        //		strID       记录ID
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		nMaxLength  最大长度,当为-1时,表示不限
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  记录不存在
        //		>=0 资源总长度
        private long GetImage(SqlConnection connection,
            string strID,
            string strImageFieldName,
            long lStart,
            int nLength1,
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

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            strID = DbPath.GetID10(strID);

            long lTotalLength = 0;

            List<string> cols = new List<string>();

            // data or newdata, 一定要有
            cols.Add(strImageFieldName);

            // 2.length

            // 3.timestamp
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                cols.Add("dptimestamp");
            }
            // 4.metadata
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                cols.Add("metadata");
            }
            // 5.range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                cols.Add("range");
            }

            cols.Add("id");

            // 部分命令字符串
            string strPartComm = StringUtil.MakePathList(cols);

            string strCommand = "";
            // DataLength()函数int类型
            strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT "
                + strPartComm + " "
                + " FROM records WHERE id='" + strID + "'";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            SqlDataReader reader = null;
            try
            {
                // 执行命令
                reader = command.ExecuteReader(/*CommandBehavior.SingleResult | */ CommandBehavior.SequentialAccess);
                /*
    For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. For all other types of statements, the return value is -1. If a rollback occurs, the return value is also -1.

                 * */
            }
            catch (Exception ex)
            {
                string strConnectionName = command.Connection.GetHashCode().ToString();
                this.container.KernelApplication.WriteErrorLog("GetImage() ExecuteReader exception: " + ex.Message + "; connection hashcode='" + strConnectionName + "'");
                throw ex;
            }

            try
            {
                if (reader == null || reader.HasRows == false)
                {
                    strError = "记录'" + strID + "'在库中不存在";
                    return -4;
                }

                reader.Read();


                lTotalLength = reader.GetBytes(0, 0, null, 0, 0);

                int nOutputLength = 0;
                // 得到实际读的长度
                // return:
                //		-1  出错
                //		0   成功
                nRet = ConvertUtil.GetRealLength(lStart,
                    nLength1,
                    lTotalLength,
                    nMaxLength,
                    out nOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                destBuffer = new byte[nOutputLength];

                reader.GetBytes(0,
    lStart,
    destBuffer,
    0,
    nOutputLength);

                // 3.timestamp
                if (StringUtil.IsInList("timestamp", strStyle) == true)
                {
                    string strOutputTimestamp = (string)reader["dptimestamp"];
                    outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);

                }
                // 4.metadata
                if (StringUtil.IsInList("metadata", strStyle) == true)
                {
                    strMetadata = (string)reader["metadata"];

                }
                // 5.range
                if (StringUtil.IsInList("range", strStyle) == true)
                {
                    string strRange = (string)reader["range"];
                }
            }
            catch (Exception ex)
            {
                strError = "GetImage() ReadData exception: " + ex.Message;
                return -1;
            }
            finally
            {
                reader.Close();
            }

            /*
            if (testidParam == null
                || (testidParam.Value is System.DBNull))
            {
                strError = "记录'" + strID + "'在库中不存在";
                return -4;
            }
             * */
            return lTotalLength;
        }

#endif

        // parameters:
        //      strNewXml   [in]局部XML
        //                  [out]创建好的全部XML
        int BuildRecordXml(
            string strID,
            string strXPath,
            string strOldXml,
            ref string strNewXml,
            byte[] baNewPreamble,
            out byte[] baWholeXml,
            out string strRange,
            out string strOutputValue,
            out string strError)
        {
            strError = "";
            baWholeXml = null;
            strRange = "";
            strOutputValue = "";
            int nRet = 0;

            Debug.Assert(string.IsNullOrEmpty(strXPath) == false, "");

            // 修改部分

            string strLocateXPath = "";
            string strCreatePath = "";
            string strNewRecordTemplate = "";
            string strAction = "";
            nRet = DatabaseUtil.ParseXPathParameter(strXPath,
                out strLocateXPath,
                out strCreatePath,
                out strNewRecordTemplate,
                out strAction,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument tempDom = new XmlDocument();
            tempDom.PreserveWhitespace = true; //设PreserveWhitespace为true

            try
            {
                if (strOldXml == "")
                {
                    if (strNewRecordTemplate == "")
                        tempDom.LoadXml("<root/>");
                    else
                        tempDom.LoadXml(strNewRecordTemplate);
                }
                else
                    tempDom.LoadXml(strOldXml);
            }
            catch (Exception ex)
            {
                strError = "1 WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时，装载旧记录到dom出错,原因:" + ex.Message;
                return -1;
            }


            if (strLocateXPath == "")
            {
                strError = "xpath表达式中的locate参数不能为空值";
                return -1;
            }

            // 通过strLocateXPath定位到指定的节点
            XmlNode node = null;
            try
            {
                node = tempDom.DocumentElement.SelectSingleNode(strLocateXPath);
            }
            catch (Exception ex)
            {
                strError = "2 WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时，XPath式子'" + strXPath + "'选择元素时出错,原因:" + ex.Message;
                return -1;
            }

            if (node == null)
            {
                if (strCreatePath == "")
                {
                    strError = "给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时，XPath式子'" + strXPath + "'指定的节点未找到。此时xpath表达式中的create参数不能为空值";
                    return -1;
                }

                node = DomUtil.CreateNodeByPath(tempDom.DocumentElement,
                    strCreatePath);
                if (node == null)
                {
                    strError = "内部错误!";
                    return -1;
                }

            }

            if (node.NodeType == XmlNodeType.Attribute)
            {

                if (strAction == "AddInteger"
                    || strAction == "+AddInteger"
                    || strAction == "AddInteger+")
                {
                    int nNumber = 0;
                    try
                    {
                        nNumber = Convert.ToInt32(strNewXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "传入的内容'" + strNewXml + "'不是数字格式。" + ex.Message;
                        return -1;
                    }

                    string strOldValue = node.Value;
                    string strLastValue;
                    nRet = StringUtil.IncreaseNumber(strOldValue,
                        nNumber,
                        out strLastValue,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (strAction == "AddInteger+")
                    {
                        strOutputValue = node.Value;
                    }
                    else
                    {
                        strOutputValue = strLastValue;
                    }

                    node.Value = strLastValue;
                    //strOutputValue = node.Value;
                }
                else if (strAction == "AppendString")
                {

                    node.Value = node.Value + strNewXml;
                    strOutputValue = node.Value;
                }
                else if (strAction == "Push")
                {
                    string strLastValue;
                    nRet = StringUtil.GetBiggerLedNumber(node.Value,
                        strNewXml,
                        out strLastValue,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    node.Value = strLastValue;
                    strOutputValue = node.Value;
                }
                else
                {
                    node.Value = strNewXml;
                    strOutputValue = node.Value;
                }
            }
            else if (node.NodeType == XmlNodeType.Element)
            {

                //Create a document fragment.
                XmlDocumentFragment docFrag = tempDom.CreateDocumentFragment();

                //Set the contents of the document fragment.
                docFrag.InnerXml = strNewXml;

                //Add the children of the document fragment to the
                //original document.
                node.ParentNode.InsertBefore(docFrag, node);

                if (strAction == "AddInteger"
                    || strAction == "AppendString")
                {
                    XmlNode newNode = node.PreviousSibling;
                    if (newNode == null)
                    {
                        strError = "newNode不可能为null";
                        return -1;
                    }

                    string strNewValue = newNode.InnerText;
                    string strOldValue = node.InnerText.Trim();  // 2012/2/16
                    if (strAction == "AddInteger")
                    {
                        int nNumber = 0;
                        try
                        {
                            nNumber = Convert.ToInt32(strNewValue);
                        }
                        catch (Exception ex)
                        {
                            strError = "传入的内容'" + strNewValue + "'不是数字格式。" + ex.Message;
                            return -1;
                        }

                        string strLastValue;
                        nRet = StringUtil.IncreaseNumber(strOldValue,
                            nNumber,
                            out strLastValue,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        /*
                        string strLastValue;
                        nRet = Database.AddInteger(strNewValue,
                            strOldValue,
                            out strLastValue,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        */
                        newNode.InnerText = strLastValue;
                        strOutputValue = newNode.OuterXml;
                    }
                    else if (strAction == "AppendString")
                    {
                        newNode.InnerText = strOldValue + strNewValue;
                        strOutputValue = newNode.OuterXml;
                    }
                    else if (strAction == "Push")
                    {
                        string strLastValue;
                        nRet = StringUtil.GetBiggerLedNumber(strOldValue,
                            strNewValue,
                            out strLastValue,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        newNode.InnerText = strLastValue;
                        strOutputValue = newNode.OuterXml;
                    }
                }

                node.ParentNode.RemoveChild(node);

            }

            strNewXml = tempDom.OuterXml;

            baWholeXml =
                DatabaseUtil.StringToByteArray(
                strNewXml,
                baNewPreamble);

            strRange = "0-" + Convert.ToString(baWholeXml.Length - 1);

            /*
                lTotalLength = baRealXml.Length;

                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		0	成功
                nRet = this.WriteSqlRecord(connection,
                    ref row_info,
                    strID,
                    strMyRange,
                    lTotalLength,
                    baRealXml,
                    // null,
                    strMetadata,
                    strStyle,
                    outputTimestamp,   //注意这儿
                    out outputTimestamp,
                    out bFull,
                    out bSingleFull,
                    out strError);
                if (nRet <= -1)
                    return nRet;
            */
            return 0;
        }

        // 更新 Keys
        // parameters:
        //      bDeleteKeys         是否要删除应该删除的 keys
        //      bDelayCreateKeys    是否延迟创建 keys
        int UpdateKeysRows(
            // SessionInfo sessioninfo,
            Connection connection,
            bool bDeleteKeys,
            bool bDelayCreateKeys,
            List<WriteInfo> records,
            out List<WriteInfo> errors,
            out string strError)
        {
            strError = "";
            errors = new List<WriteInfo>();
            int nRet = 0;

            if (records == null || records.Count == 0)
                return 0;

            if (bDeleteKeys == false && bDelayCreateKeys == false)
            {
                // 如果要立即创建 Keys，但又不事先删除 以前的 Keys，这会造成重复行
                strError = "UpdateKeysRows() bDeleteKeys 和 bDelayCreateKeys 不能同时为 false";
                return -1;
            }

            KeyCollection total_newkeys = new KeyCollection();
            KeyCollection total_oldkeys = new KeyCollection();

            foreach (WriteInfo info in records)
            {
                string strNewXml = info.record.Xml;
                string strOldXml = "";

                if (bDelayCreateKeys == false)
                {
                    if (info.row_info != null)
                    {
                        byte[] baOldData = GetCompleteData(info.row_info);
                        if (baOldData != null && baOldData.Length > 0)
                        {
                            strOldXml = DatabaseUtil.ByteArrayToString(baOldData,
                                out byte[] baPreamble);
                        }
                    }
                }
                else // bFastMode == true
                {
                    if (info.row_info != null)
                    {
                        // 记忆这条记录的ID，后面会统一进行一次刷新检索点的操作。这里就不必创建检索点了
                        if (this.RebuildIDs == null)
                        {
                            strError = "当 UpdateKeysRows() 需要写入 '" + this.GetCaption("zh-CN") + "' 库 ID 存储的时候，发现其 RebuildIDs 为空";
                            return -1;
                        }
                        Debug.Assert(this.RebuildIDs != null, "");
                        this.RebuildIDs.Append(info.ID);
                        continue;
                    }
                }

                // return:
                //      -2  出错。strOldXml 结构不合法
                //      -1  出错
                //      0   成功
                nRet = this.MergeKeys(info.ID,
                    strNewXml,
                    strOldXml,
                    true,
                    out KeyCollection newKeys,
                    out KeyCollection oldKeys,
                    out XmlDocument newDom,
                    out XmlDocument oldDom,
                    out strError);
                if (nRet == -1) // 注: -2 当作正确情况处理
                {
                    // return -1;

                    // 2017/5/14
                    if (info.record == null)
                        info.record = new RecordBody();
                    if (info.record.Result == null)
                        info.record.Result = new Result();
                    info.record.Result.ErrorCode = ErrorCodeValue.CommonError;
                    info.record.Result.ErrorString = strError;
                    errors.Add(info);
                    continue;
                }

                // 处理子文件
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.ModifyFiles(connection,
                    info.ID,
                    newDom,
                    oldDom,
                    out strError);
                if (nRet == -1)
                {
                    // return -1;

                    // 2017/5/14
                    if (info.record == null)
                        info.record = new RecordBody();
                    if (info.record.Result == null)
                        info.record.Result = new Result();
                    info.record.Result.ErrorCode = ErrorCodeValue.CommonError;
                    info.record.Result.ErrorString = strError;
                    errors.Add(info);
                    continue;
                }
                total_newkeys.AddRange(newKeys);
                total_oldkeys.AddRange(oldKeys);
            }

            total_newkeys.Sort();
            if (total_oldkeys.Count > 1)
                total_oldkeys.Sort();

            if (bDelayCreateKeys == false)
            {
                // 立即兑现删除和创建

                // 处理检索点
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.ModifyKeys(connection,
                    total_newkeys,
                    total_oldkeys,
                    bDelayCreateKeys,   // bFastMode 仅仅对 SQLite 起作用
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            else
            {
                // 延迟创建检索点。把需要创建的检索点存储起来
                // 注意，此种模式下，对于需要删除的检索点，本函数未进行处理，需要调主妥善处理
                if (this.container.DelayTables == null)
                    this.container.DelayTables = new DelayTableCollection();
                nRet = this.container.DelayTables.Write(
                    this.m_strSqlDbName,
                    total_newkeys,
                    (dbname, tablename) => { return this.container.GetTempFileName("ukr"); },
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bDeleteKeys == true)
                {
                    // 处理检索点
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = this.ModifyKeys(connection,
                        null,
                        total_oldkeys,
                        bDelayCreateKeys,   // bFastMode 仅仅对 SQLite 起作用
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            return 0;
        }

        // 重建 Keys
        // parameters:
        //      bDeleteKeys         是否在创建前删除记录的全部 keys
        //      bDelayCreateKeys    是否延迟创建 keys
        // return:
        //      -1  出错
        //      >=0 返回总共处理的 keys 行数
        int RebuildKeysRows(
            Connection connection,
            bool bDeleteKeys,
            bool bDelayCreateKeys,
            List<WriteInfo> records,
            //out List<WriteInfo> results,
            out string strError)
        {
            strError = "";
            //results = new List<WriteInfo>();
            int nRet = 0;
            // int nTotalCount = 0;

            if (bDeleteKeys == false && bDelayCreateKeys == false)
            {
                // 如果要立即创建 Keys，但又不事先删除 以前的 Keys，这会造成重复行
                strError = "RebuildKeysRows() bDeleteKeys 和 bDelayCreateKeys 不能同时为 false";
                return -1;
            }

            KeyCollection total_oldkeys = new KeyCollection();

            foreach (WriteInfo info in records)
            {
                KeyCollection newKeys = null;
                KeyCollection oldKeys = null;
                XmlDocument newDom = null;
                XmlDocument oldDom = null;

                string strOldXml = "";

                if (info.row_info != null)
                {
                    byte[] baOldData = GetCompleteData(info.row_info);
                    if (baOldData != null && baOldData.Length > 0)
                    {
                        byte[] baPreamble = null;
                        strOldXml = DatabaseUtil.ByteArrayToString(baOldData,
                            out baPreamble);
                    }
                }

                // TODO: 是否警告那些因为记录尺寸太大而无法创建检索点的记录?


                // return:
                //      -2  出错。strOldXml 结构不合法
                //      -1  出错
                //      0   成功
                nRet = this.MergeKeys(info.ID,
                    "",
                    strOldXml,
                    true,
                    out newKeys,
                    out oldKeys,
                    out newDom,
                    out oldDom,
                    out strError);
                if (nRet == -1)
                    return -1;
                // 2021/8/27
                if (nRet == -2)
                    bDeleteKeys = true;
                total_oldkeys.AddRange(oldKeys);
            }

            if (total_oldkeys.Count > 1)
                total_oldkeys.Sort();

            if (bDeleteKeys == true)
            {
                // 先根据 ID 列表删除所有 keys
                // 注意，在没有全部预先删除keys + Bulkcopy 的情况下，为了正常及时删除每条XML记录的 keys ， SQL keys 表应该具有 B+ 树索引。虽然此时并未立即写入新的 keys，但删除以前的 keys 需要 B+ 树索引。等到后面 BulkCopy 前，可以专门删除 B+ 树索引，然后等 BulkCopy 结束后重新建立 B+ 树索引
                nRet = this.ForceDeleteKeys(connection,
                    WriteInfo.get_ids(records),
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (bDelayCreateKeys == false)
            {
                // 处理检索点
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.ModifyKeys(connection,
                    total_oldkeys,
                    null,
                    bDelayCreateKeys,
                    out strError);
                if (nRet == -1)
                    return -1;
                return total_oldkeys.Count;
            }
            else
            {
                // 延迟创建检索点。把需要创建的检索点存储起来
                if (this.container.DelayTables == null)
                    this.container.DelayTables = new DelayTableCollection();
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.container.DelayTables.Write(
                    this.m_strSqlDbName,
                    total_oldkeys,
                    (dbname, tablename) => { return this.container.GetTempFileName("ukr"); },
                    out strError);
                if (nRet == -1)
                    return -1;
                return total_oldkeys.Count;
            }

            // return 0;
        }

        // 创建或者更新 SQL 记录行
        // 调用后，.record.Metadata 和 .record.Timestamp 发生变化
        // 本函数不会修改 .row_info 的成员值。因为后面创建检索点的阶段，还要通过 row_info 来找到旧记录的信息。但这里有个例外，就是 row_info.NewFileName 会被修改(对应的文件被删除)，后面创建检索点的时候也用不到这个信息，因为 NewFileName 只是代表以前没有上载完整的文件
        // parameters:
        //      results [out] 返回已经成功更新或者创建的记录
        int UpdateRecordRows(Connection connection,
            List<WriteInfo> records,
            string strStyle,
            out List<WriteInfo> results,
            out string strError)
        {
            strError = "";
            results = new List<WriteInfo>();

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            // 2013/11/23
            // 是否要直接利用输入的时间戳
            bool bForceTimestamp = StringUtil.IsInList("forcesettimestamp", strStyle);

            #region MS SQL Server
            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                int nParameters = 0;

                using (SqlCommand command = new SqlCommand("",
                    connection.SqlConnection))
                {
                    string strCommand = "";

                    List<WriteInfo> parts = new List<WriteInfo>();
                    int i = 0;
                    foreach (WriteInfo info in records)
                    {
                        // 提交一次
                        bool bCommit = false;
                        if (info.row_info == null && nParameters + 5 > 2100 - 1)
                            bCommit = true;
                        if (info.row_info != null && nParameters + 5 > 2100 - 1)
                            bCommit = true;

                        if (bCommit == true)
                        {
                            Debug.Assert(string.IsNullOrEmpty(strCommand) == false, "");
                            command.CommandText = "use " + this.m_strSqlDbName + "\n"
        + strCommand + "use master\n";

                            int nCount = command.ExecuteNonQuery();
                            if (nCount == 0)
                            {
                                strError = "创建或更新 records 行失败";
                                return -1;
                            }
                            strCommand = "";
                            command.Parameters.Clear();
                            nParameters = 0;

                            results.AddRange(parts);
                            parts.Clear();
                        }

                        if (info.record == null)
                        {
                            Debug.Assert(false, "");
                            strError = "info.record不能为空";
                            return -1;
                        }

                        bool bObjectFile = false;
                        Debug.Assert(info.baContent != null, "");
                        if (this.m_lObjectStartSize != -1 && info.baContent.Length >= this.m_lObjectStartSize)
                            bObjectFile = true;

                        string strShortFileName = "";
                        if (bObjectFile == true)
                        {
                            // 将缓冲区内容一次性写入对象文件
                            nRet = WriteToObjectFile(
                                info.ID,
                                info.baContent,
                                out strShortFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(strShortFileName) == false, "");
                        }

                        // 删除残余的旧有对象文件
                        if (info.row_info != null && string.IsNullOrEmpty(info.row_info.NewFileName) == false)
                        {
                            this._streamCache.FileDelete(GetObjectFileName(info.row_info.NewFileName));
                            info.row_info.NewFileName = "";
                        }

                        // 创建 metadata
                        // return:
                        //		-1	出错
                        //		0	成功
                        nRet = DatabaseUtil.MergeMetadata(info.row_info != null ? info.row_info.Metadata : "",
                            info.record.Metadata,
                            info.baContent.Length,
                            "",
                            out string strResultMetadata,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        info.record.Metadata = strResultMetadata;

                        // 创建 timestamp
                        string strOutputTimestamp = "";
                        if (bForceTimestamp == true)
                            strOutputTimestamp = ByteArray.GetHexTimeStampString(info.record.Timestamp);
                        else
                            strOutputTimestamp = this.CreateTimestampForDb();

                        info.record.Timestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                        if (info.row_info == null)
                        {
                            // 创建新行
                            if (bObjectFile == false)
                            {
                                strCommand +=
            " INSERT INTO records(id, data, range, metadata, dptimestamp) "
            + " VALUES(@id" + i + ", @data" + i + ", @range" + i + ", @metadata" + i + ", @dptimestamp" + i + ")"
            + "\n";
                            }
                            else
                            {
                                strCommand +=
        " INSERT INTO records(id, data, range, metadata, dptimestamp,  filename) "
        + " VALUES(@id" + i + ", NULL, @range" + i + ", @metadata" + i + ", @dptimestamp" + i + ", @filename" + i + ")"
        + "\n";
                            }

                            SqlParameter idParam =
        command.Parameters.Add("@id" + i,
        SqlDbType.NVarChar);
                            idParam.Value = info.ID;

                            if (bObjectFile == false)
                            {
                                SqlParameter dataParam =
                                    command.Parameters.Add("@data" + i,
                                    SqlDbType.Binary,
                                    info.baContent.Length);
                                dataParam.Value = info.baContent;   // ?? 是否允许空?
                            }

                            SqlParameter rangeParam =
        command.Parameters.Add("@range" + i,
        SqlDbType.NVarChar,
        4000);
                            if (bObjectFile == true)
                                rangeParam.Value = "#";
                            else
                                rangeParam.Value = "";

                            SqlParameter metadataParam =
                                command.Parameters.Add("@metadata" + i,
                                SqlDbType.NVarChar);
                            metadataParam.Value = info.record.Metadata;

                            SqlParameter dptimestampParam =
                                command.Parameters.Add("@dptimestamp" + i,
                                SqlDbType.NVarChar,
                                100);
                            dptimestampParam.Value = strOutputTimestamp;

                            if (bObjectFile == true)
                            {
                                SqlParameter filenameParam =
                        command.Parameters.Add("@filename" + i,
                        SqlDbType.NVarChar,
                        255);
                                filenameParam.Value = strShortFileName;
                            }

                            nParameters += 5;
                            parts.Add(info);
                        }
                        else
                        {
                            // 更新已有的行

                            // TODO: 由于本次可以一次设置好 data，所以不必考虑方向问题，就像第一次设置那样去设置好了

#if NO
                            bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容
                            if (String.IsNullOrEmpty(info.row_info.Range) == false
                                && info.row_info.Range[0] == '!')
                                bReverse = true;
#endif

                            if (bObjectFile == false)
                            {
                                strCommand += " UPDATE records "
                                + " SET dptimestamp=@dptimestamp" + i + ","
                                + " newdptimestamp=NULL,"
                                + " data=@data" + i + ", newdata=NULL,"
                                + " range=@range" + i + ","
                                + " filename=NULL, newfilename=NULL,"
                                + " metadata=@metadata" + i + " "
                                + " WHERE id=@id" + i + " \n";
                            }
                            else
                            {
                                strCommand += " UPDATE records "
                                + " SET dptimestamp=@dptimestamp" + i + ","
                                + " newdptimestamp=NULL,"
                                + " data=NULL, newdata=NULL,"
                                + " range=@range" + i + ","
                                + " filename=@filename" + i + ", newfilename=NULL,"
                                + " metadata=@metadata" + i + " "
                                + " WHERE id=@id" + i + " \n";
                            }

                            string strCurrentRange = "";

                            SqlParameter idParam = command.Parameters.Add("@id" + i,
        SqlDbType.NVarChar);
                            idParam.Value = info.ID;

                            if (bObjectFile == false)
                            {
                                SqlParameter dataParam =
                                    command.Parameters.Add("@data" + i,
                                    SqlDbType.Binary,
                                    info.baContent.Length);
                                dataParam.Value = info.baContent;   // ?? 是否允许空?
                            }

                            SqlParameter dptimestampParam =
                                command.Parameters.Add("@dptimestamp" + i,
                                SqlDbType.NVarChar,
                                100);
                            dptimestampParam.Value = strOutputTimestamp;

                            SqlParameter rangeParam =
                                command.Parameters.Add("@range" + i,
                                SqlDbType.NVarChar,
                                4000);
                            if (bObjectFile == true)
                                rangeParam.Value = "#" + strCurrentRange;
                            else
                            {
                                rangeParam.Value = strCurrentRange;
                            }

                            // info.row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                            SqlParameter metadataParam =
                                command.Parameters.Add("@metadata" + i,
                                SqlDbType.NVarChar,
                                4000);
                            metadataParam.Value = info.record.Metadata;

                            if (bObjectFile == true)
                            {
                                SqlParameter filenameParam =
                        command.Parameters.Add("@filename" + i,
                        SqlDbType.NVarChar,
                        255);
                                filenameParam.Value = strShortFileName;
                                // info.row_info.FileName = strShortFileName;
                            }

                            if (bObjectFile == true)
                                nParameters += 5;
                            else
                                nParameters += 5;
                            parts.Add(info);
                        }

                        i++;
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        command.CommandText = "use " + this.m_strSqlDbName + "\n"
                            + strCommand + "use master\n";

                        int nCount = command.ExecuteNonQuery();
                        if (nCount == 0)
                        {
                            strError = "创建或更新 records 行失败";
                            return -1;
                        }
                        strCommand = "";
                        results.AddRange(parts);
                        parts.Clear();
                    }
                } // end of using command
            }
            #endregion // MS SQL Server

            #region SQLite
            if (connection.SqlServerType == SqlServerType.SQLite)
            {
                bool bFastMode = false;
                using (SQLiteCommand command = new SQLiteCommand("",
                    connection.SQLiteConnection))
                {
                    IDbTransaction trans = null;

                    if (bFastMode == false)
                        trans = connection.SQLiteConnection.BeginTransaction();
                    try
                    {
                        string strCommand = "";

                        List<WriteInfo> parts = new List<WriteInfo>();
                        int i = 0;
                        foreach (WriteInfo info in records)
                        {
                            if (info.record == null)
                            {
                                Debug.Assert(false, "");
                                strError = "info.record不能为空";
                                return -1;
                            }

                            string strShortFileName = "";
                            // 将缓冲区内容一次性写入对象文件
                            nRet = WriteToObjectFile(
                                info.ID,
                                info.baContent,
                                out strShortFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(strShortFileName) == false, "");

                            // 删除残余的旧有对象文件
                            if (info.row_info != null && string.IsNullOrEmpty(info.row_info.NewFileName) == false)
                            {
                                this._streamCache.FileDelete(GetObjectFileName(info.row_info.NewFileName));
                                info.row_info.NewFileName = "";
                            }

                            // 创建 metadata
                            string strResultMetadata = "";
                            // return:
                            //		-1	出错
                            //		0	成功
                            nRet = DatabaseUtil.MergeMetadata(info.row_info != null ? info.row_info.Metadata : "",
                                info.record.Metadata,
                                info.baContent.Length,
                                "",
                                out strResultMetadata,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            info.record.Metadata = strResultMetadata;

                            // 创建 timestamp
                            string strOutputTimestamp = "";
                            if (bForceTimestamp == true)
                                strOutputTimestamp = ByteArray.GetHexTimeStampString(info.record.Timestamp);
                            else
                                strOutputTimestamp = this.CreateTimestampForDb();

                            info.record.Timestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                            if (info.row_info == null)
                            {
                                // 创建新行
                                strCommand +=
        " INSERT INTO records(id, range, metadata, dptimestamp, filename) "
        + " VALUES(@id" + i + ", @range" + i + ", @metadata" + i + ", @dptimestamp" + i + ", @filename" + i + ")"
        + " ; ";

                                SQLiteParameter idParam =
        command.Parameters.Add("@id" + i,
        DbType.String);
                                idParam.Value = info.ID;


                                SQLiteParameter rangeParam =
        command.Parameters.Add("@range" + i,
        DbType.String);
                                rangeParam.Value = "#";

                                SQLiteParameter metadataParam =
                                command.Parameters.Add("@metadata" + i,
                                DbType.String);
                                metadataParam.Value = info.record.Metadata;

                                SQLiteParameter dptimestampParam =
                                    command.Parameters.Add("@dptimestamp" + i,
                                    DbType.String);
                                dptimestampParam.Value = strOutputTimestamp;


                                SQLiteParameter filenameParam =
                            command.Parameters.Add("@filename" + i,
                            DbType.String);
                                filenameParam.Value = strShortFileName;


                                parts.Add(info);
                            }
                            else
                            {
                                // 更新已有的行
                                strCommand += " UPDATE records "
                                + " SET dptimestamp=@dptimestamp" + i + ","
                                + " newdptimestamp=NULL,"
                                + " range=@range" + i + ","
                                + " filename=@filename" + i + ", newfilename=NULL,"
                                + " metadata=@metadata" + i + " "
                                + " WHERE id=@id" + i + " ; ";

                                string strCurrentRange = "";

                                SQLiteParameter idParam = command.Parameters.Add("@id" + i,
            DbType.String);
                                idParam.Value = info.ID;

                                SQLiteParameter dptimestampParam =
                                    command.Parameters.Add("@dptimestamp" + i,
                                    DbType.String);
                                dptimestampParam.Value = strOutputTimestamp;

                                SQLiteParameter rangeParam =
                                    command.Parameters.Add("@range" + i,
                                    DbType.String);
                                rangeParam.Value = "#" + strCurrentRange;


                                SQLiteParameter metadataParam =
                                command.Parameters.Add("@metadata" + i,
                                DbType.String);
                                metadataParam.Value = info.record.Metadata;

                                SQLiteParameter filenameParam =
                            command.Parameters.Add("@filename" + i,
                            DbType.String);
                                filenameParam.Value = strShortFileName;


                                parts.Add(info);
                            }

                            {
                                // 提交一次
                                Debug.Assert(string.IsNullOrEmpty(strCommand) == false, "");
                                command.CommandText = strCommand;

                                int nCount = command.ExecuteNonQuery();
                                if (nCount == 0)
                                {
                                    strError = "创建或更新 records 行失败";
                                    return -1;
                                }
                                strCommand = "";
                                command.Parameters.Clear();
                                results.AddRange(parts);
                                parts.Clear();
                            }

                            i++;
                        }
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command
            }
            #endregion // SQLite

            #region MySql
            if (connection.SqlServerType == SqlServerType.MySql)
            {
                int nParameters = 0;

                using (MySqlCommand command = new MySqlCommand("",
                    connection.MySqlConnection))
                {
                    MySqlTransaction trans = null;

                    // https://mysqlconnector.net/troubleshooting/transaction-usage/
                    trans = connection.MySqlConnection.BeginTransaction();
                    try
                    {
                        string strCommand = "";

                        // 2021/6/9
                        command.Transaction = trans;

                        List<WriteInfo> parts = new List<WriteInfo>();
                        int i = 0;
                        foreach (WriteInfo info in records)
                        {
                            // 提交一次
                            bool bCommit = false;

                            // 2017/4/1
                            // *** 注：MySQL 在 TCP/IP 方式下 2100 没有问题；在 Named Pipe 方式下 1400 没有问题，1500 就会抛出异常“connection must be valid and open to rollback transaction”，为保险这里用 1000
                            if (info.row_info == null && nParameters + 5 > 1000 - 1)
                                bCommit = true;
                            if (info.row_info != null && nParameters + 5 > 1000 - 1)
                                bCommit = true;

                            if (bCommit == true)
                            {
                                Debug.Assert(string.IsNullOrEmpty(strCommand) == false, "");
                                command.CommandText = "use " + this.m_strSqlDbName + " ;\n"
        + strCommand;

                                int nCount = command.ExecuteNonQuery();
                                if (nCount == 0)
                                {
                                    strError = "创建或更新 records 行失败";
                                    return -1;
                                }
                                strCommand = "";
                                command.Parameters.Clear();
                                nParameters = 0;

                                results.AddRange(parts);
                                parts.Clear();
                            }

                            if (info.record == null)
                            {
                                Debug.Assert(false, "");
                                strError = "info.record不能为空";
                                return -1;
                            }

                            string strShortFileName = "";
                            // 将缓冲区内容一次性写入对象文件
                            nRet = WriteToObjectFile(
                                info.ID,
                                info.baContent,
                                out strShortFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(strShortFileName) == false, "");

                            // 删除残余的旧有对象文件
                            if (info.row_info != null && string.IsNullOrEmpty(info.row_info.NewFileName) == false)
                            {
                                this._streamCache.FileDelete(GetObjectFileName(info.row_info.NewFileName));
                                info.row_info.NewFileName = "";
                            }

                            // 创建 metadata
                            string strResultMetadata = "";
                            // return:
                            //		-1	出错
                            //		0	成功
                            nRet = DatabaseUtil.MergeMetadata(info.row_info != null ? info.row_info.Metadata : "",
                                info.record.Metadata,
                                info.baContent.Length,
                                "",
                                out strResultMetadata,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            info.record.Metadata = strResultMetadata;

                            // 创建 timestamp
                            string strOutputTimestamp = "";
                            if (bForceTimestamp == true)
                                strOutputTimestamp = ByteArray.GetHexTimeStampString(info.record.Timestamp);
                            else
                                strOutputTimestamp = this.CreateTimestampForDb();

                            info.record.Timestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                            if (info.row_info == null)
                            {
                                // 创建新行
                                strCommand +=
        " INSERT INTO `" + this.m_strSqlDbName + "`.records (id, `range`, metadata, dptimestamp, filename) "
        + " VALUES (@id" + i + ", @range" + i + ", @metadata" + i + ", @dptimestamp" + i + ", @filename" + i + ")"
        + " ;\n";


                                MySqlParameter idParam =
        command.Parameters.Add("@id" + i,
        MySqlDbType.String);
                                idParam.Value = info.ID;


                                MySqlParameter rangeParam =
        command.Parameters.Add("@range" + i,
        MySqlDbType.String);
                                rangeParam.Value = "#";

                                MySqlParameter metadataParam =
                                command.Parameters.Add("@metadata" + i,
                                MySqlDbType.String);
                                metadataParam.Value = info.record.Metadata;

                                MySqlParameter dptimestampParam =
                                    command.Parameters.Add("@dptimestamp" + i,
                                    MySqlDbType.String);
                                dptimestampParam.Value = strOutputTimestamp;


                                MySqlParameter filenameParam =
                            command.Parameters.Add("@filename" + i,
                            MySqlDbType.String);
                                filenameParam.Value = strShortFileName;

                                nParameters += 5;
                                parts.Add(info);
                            }
                            else
                            {
                                // 更新已有的行
                                strCommand += " UPDATE `" + this.m_strSqlDbName + "`.records "
                                + " SET dptimestamp=@dptimestamp" + i + ","
                                + " newdptimestamp=NULL,"
                                + " `range`=@range" + i + ","
                                + " filename=@filename" + i + ", newfilename=NULL,"
                                + " metadata=@metadata" + i + " "
                                + " WHERE id=@id" + i + " ; ";

                                string strCurrentRange = "";

                                MySqlParameter idParam =
                                    command.Parameters.Add("@id" + i,
                                    MySqlDbType.String);
                                idParam.Value = info.ID;

                                MySqlParameter dptimestampParam =
                                    command.Parameters.Add("@dptimestamp" + i,
                                    MySqlDbType.String);
                                dptimestampParam.Value = strOutputTimestamp;

                                MySqlParameter rangeParam =
                                    command.Parameters.Add("@range" + i,
                                    MySqlDbType.String);
                                rangeParam.Value = "#" + strCurrentRange;

                                MySqlParameter metadataParam =
                                    command.Parameters.Add("@metadata" + i,
                                    MySqlDbType.String);
                                metadataParam.Value = info.record.Metadata;

                                MySqlParameter filenameParam =
                                    command.Parameters.Add("@filename" + i,
                                    MySqlDbType.String);
                                filenameParam.Value = strShortFileName;

                                nParameters += 5;
                                parts.Add(info);
                            }

                            i++;
                        }

                        // 最后提交一次
                        if (string.IsNullOrEmpty(strCommand) == false)
                        {
                            Debug.Assert(string.IsNullOrEmpty(strCommand) == false, "");
                            command.CommandText = "use " + this.m_strSqlDbName + " ;\n"
        + strCommand;

                            int nCount = command.ExecuteNonQuery();
                            if (nCount == 0)
                            {
                                strError = "创建或更新 records 行失败";
                                return -1;
                            }
                            strCommand = "";
                            command.Parameters.Clear();
                            nParameters = 0;

                            results.AddRange(parts);
                            parts.Clear();
                        }

                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command
            }
            #endregion // MySql

            #region Oracle
            if (connection.SqlServerType == SqlServerType.Oracle)
            {
                using (OracleCommand command = new OracleCommand("", connection.OracleConnection))
                {
                    command.BindByName = true;

                    IDbTransaction trans = null;

                    trans = connection.OracleConnection.BeginTransaction();
                    try
                    {
                        string strCommand = "";

                        List<WriteInfo> parts = new List<WriteInfo>();
                        int i = 0;
                        foreach (WriteInfo info in records)
                        {
                            if (info.record == null)
                            {
                                Debug.Assert(false, "");
                                strError = "info.record不能为空";
                                return -1;
                            }


                            string strShortFileName = "";
                            // 将缓冲区内容一次性写入对象文件
                            nRet = WriteToObjectFile(
                                info.ID,
                                info.baContent,
                                out strShortFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(strShortFileName) == false, "");

                            // 删除残余的旧有对象文件
                            if (info.row_info != null && string.IsNullOrEmpty(info.row_info.NewFileName) == false)
                            {
                                this._streamCache.FileDelete(GetObjectFileName(info.row_info.NewFileName));
                                info.row_info.NewFileName = "";
                            }

                            // 创建 metadata
                            string strResultMetadata = "";
                            // return:
                            //		-1	出错
                            //		0	成功
                            nRet = DatabaseUtil.MergeMetadata(info.row_info != null ? info.row_info.Metadata : "",
                                info.record.Metadata,
                                info.baContent.Length,
                                "",
                                out strResultMetadata,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            info.record.Metadata = strResultMetadata;

                            // 创建 timestamp
                            string strOutputTimestamp = "";
                            if (bForceTimestamp == true)
                                strOutputTimestamp = ByteArray.GetHexTimeStampString(info.record.Timestamp);
                            else
                                strOutputTimestamp = this.CreateTimestampForDb();

                            info.record.Timestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                            if (info.row_info == null)
                            {
                                // 创建新行
                                strCommand +=
        " INSERT INTO " + this.m_strSqlDbName + "_records (id, range, metadata, dptimestamp, filename) "
        + " VALUES(:id" + i + ", :range" + i + ", :metadata" + i + ", :dptimestamp" + i + ", :filename" + i + ")"
        + " ";

                                OracleParameter idParam =
        command.Parameters.Add(":id" + i,
        OracleDbType.NVarchar2);
                                idParam.Value = info.ID;


                                OracleParameter rangeParam =
        command.Parameters.Add(":range" + i,
        OracleDbType.NVarchar2);
                                rangeParam.Value = "#";

                                OracleParameter metadataParam =
                                command.Parameters.Add(":metadata" + i,
                                OracleDbType.NVarchar2);
                                metadataParam.Value = info.record.Metadata;

                                OracleParameter dptimestampParam =
                                    command.Parameters.Add(":dptimestamp" + i,
                                    OracleDbType.NVarchar2);
                                dptimestampParam.Value = strOutputTimestamp;


                                OracleParameter filenameParam =
                            command.Parameters.Add(":filename" + i,
                            OracleDbType.NVarchar2);
                                filenameParam.Value = strShortFileName;


                                parts.Add(info);
                            }
                            else
                            {
                                // 更新已有的行
                                strCommand += " UPDATE " + this.m_strSqlDbName + "_records "
                                + " SET dptimestamp=:dptimestamp" + i + ","
                                + " newdptimestamp=NULL,"
                                + " range=:range" + i + ","
                                + " filename=:filename" + i + ", newfilename=NULL,"
                                + " metadata=:metadata" + i + " "
                                + " WHERE id=:id" + i + " ";

                                string strCurrentRange = "";

                                OracleParameter idParam =
                                    command.Parameters.Add(":id" + i,
                                    OracleDbType.NVarchar2);
                                idParam.Value = info.ID;

                                OracleParameter dptimestampParam =
                                    command.Parameters.Add(":dptimestamp" + i,
                                    OracleDbType.NVarchar2);
                                dptimestampParam.Value = strOutputTimestamp;

                                OracleParameter rangeParam =
                                    command.Parameters.Add(":range" + i,
                                    OracleDbType.NVarchar2);
                                rangeParam.Value = "#" + strCurrentRange;


                                OracleParameter metadataParam =
                                command.Parameters.Add(":metadata" + i,
                                OracleDbType.NVarchar2);
                                metadataParam.Value = info.record.Metadata;

                                OracleParameter filenameParam =
                            command.Parameters.Add(":filename" + i,
                            OracleDbType.NVarchar2);
                                filenameParam.Value = strShortFileName;


                                parts.Add(info);
                            }

                            {
                                // 提交一次
                                Debug.Assert(string.IsNullOrEmpty(strCommand) == false, "");
                                command.CommandText = strCommand;

                                int nCount = command.ExecuteNonQuery();
                                if (nCount == 0)
                                {
                                    strError = "创建或更新 records 行失败";
                                    return -1;
                                }
                                strCommand = "";
                                command.Parameters.Clear();

                                results.AddRange(parts);
                                parts.Clear();
                            }

                            i++;
                        }
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command
            }
            #endregion // Oracle

            return 0;
        }

        // 将缓冲区内容一次性写入对象文件
        int WriteToObjectFile(
            string strID,
            byte[] baContent,
            out string strShortFileName,
            // ref RecordRowInfo row_info,
            out string strError)
        {
            strError = "";
            strShortFileName = "";

            if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
            {
                strError = "数据库尚未配置对象文件目录，但写入对象时出现了需要引用对象文件的情况";
                return -1;
            }

            string strFileName = "";

            strFileName = BuildObjectFileName(strID, false);
            strShortFileName = GetShortFileName(strFileName); // 记忆
            if (strShortFileName == null)
            {
                strError = "构造短文件名时出错。记录ID '" + strID + "', 对象文件目录 '" + this.m_strObjectDir + "', 物理文件名 '" + strFileName + "'";
                return -1;
            }

            int nRedoCount = 0;
            REDO:
            try
            {
                StreamItem item = _streamCache.GetWriteStream(strFileName, true);
                try
                {
                    // 第一次写文件,并且文件长度大于对象总长度，则截断文件
                    if (item.FileStream.Length > baContent.Length)
                        item.FileStream.SetLength(0);

                    item.FileStream.Seek(0, SeekOrigin.Begin);
                    item.FileStream.Write(baContent,
                            0,
                            baContent.Length);
                }
                finally
                {
                    _streamCache.ReturnStream(item);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                if (nRedoCount == 0)
                {
                    // 创建中间子目录
                    PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                    nRedoCount++;
                    goto REDO;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                strError = "写入文件 '" + strFileName + "' 时发生错误: " + ex.Message;
                return -1;
            }
            return 0;
        }

#if OLD
        // 将缓冲区内容一次性写入对象文件
        int WriteToObjectFile(
            string strID,
            byte[] baContent,
            out string strShortFileName,
            // ref RecordRowInfo row_info,
            out string strError)
        {
            strError = "";
            strShortFileName = "";

            if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
            {
                strError = "数据库尚未配置对象文件目录，但写入对象时出现了需要引用对象文件的情况";
                return -1;
            }

            string strFileName = "";

            strFileName = BuildObjectFileName(strID, false);
            strShortFileName = GetShortFileName(strFileName); // 记忆
            if (strShortFileName == null)
            {
                strError = "构造短文件名时出错。记录ID '" + strID + "', 对象文件目录 '" + this.m_strObjectDir + "', 物理文件名 '" + strFileName + "'";
                return -1;
            }

            int nRedoCount = 0;
            REDO:
            try
            {
                _streamCache.ClearItems(strFileName);
                using (FileStream s = File.Open(
        strFileName,
        FileMode.OpenOrCreate,
        FileAccess.Write,
        FileShare.ReadWrite))
                {
                    // 第一次写文件,并且文件长度大于对象总长度，则截断文件
                    if (s.Length > baContent.Length)
                        s.SetLength(0);

                    s.Seek(0, SeekOrigin.Begin);
                    s.Write(baContent,
                        0,
                        baContent.Length);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                if (nRedoCount == 0)
                {
                    // 创建中间子目录
                    PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                    nRedoCount++;
                    goto REDO;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                strError = "写入文件 '" + strFileName + "' 时发生错误: " + ex.Message;
                return -1;
            }
            return 0;
        }

#endif

        const int MYSQL_MAX_GETINFO_COUNT = 1000;

        // 这一层主要是把较大的数组分片进行调用
        private int GetRowInfos(Connection connection,
        bool bGetData,
        List<string> ids,
        out List<RecordRowInfo> row_infos,
        out string strError)
        {
            strError = "";
            row_infos = new List<RecordRowInfo>();

            if (connection.SqlServerType == SqlServerType.MySql)
            {
                int nRet = 0;
                List<RecordRowInfo> results = new List<RecordRowInfo>();
                int start = 0;
                int length = Math.Min(MYSQL_MAX_GETINFO_COUNT, ids.Count);
                int count = 0;
                while (count < ids.Count)
                {
                    nRet = _getRowInfos(connection,
                    bGetData,
                    ids.GetRange(start, length),
                    out results,
                    out strError);
                    if (nRet == -1)
                        return -1;
                    row_infos.AddRange(results);
                    count += length;
                    start += length;
                    length = Math.Min(MYSQL_MAX_GETINFO_COUNT, ids.Count - count);
                }
                return nRet;
            }
            else
                return _getRowInfos(connection,
                    bGetData,
                    ids,
                    out row_infos,
                    out strError);
        }

        // 获得 records表中 多个已存在的行信息
        // parameters:
        //      bGetData    是否需要获得记录体?
        private int _getRowInfos(Connection connection,
            bool bGetData,
            List<string> ids,
            out List<RecordRowInfo> row_infos,
            out string strError)
        {
            strError = "";
            row_infos = new List<RecordRowInfo>();

            if (ids.Count == 0)
                return 0;

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            StringBuilder idstring = new StringBuilder(4096);
            int i = 0;
            foreach (string s in ids)
            {
                if (StringUtil.IsPureNumber(s) == false)
                {
                    strError = "ID '" + s + "' 必须是纯数字";
                    return -1;
                }
                if (i != 0)
                    idstring.Append(",");
                idstring.Append("'" + s + "'");
                i++;
            }
#endif
            string strIdString = "";
            nRet = BuildIdString(ids, out strIdString, out strError);
            if (nRet == -1)
                return -1;

            #region MS SQL Server
            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                // TODO: 可否限定超过一定尺寸的数据库就不要返回? 
                // 其实似乎没有这个必要，因为即便要返回，也是 SqlReader GetBytes() 的时候才临时去 SQL Server 取的吧
                string strSelect = " SELECT TEXTPTR(data)," // 0
                    + " DataLength(data),"  // 1
                    + " TEXTPTR(newdata),"  // 2
                    + " DataLength(newdata),"   // 3
                    + " range,"             // 4
                    + " dptimestamp,"       // 5
                    + " metadata, "         // 6
                    + " newdptimestamp,"    // 7
                    + " filename,"          // 8
                    + " newfilename,"        // 9
                    + " id"                 // 10
                    + (bGetData == true ?
                    ", data,"                 // 11
                    + " newdata"            // 12
                    : "")
                    + " FROM records "
                    + " WHERE id in (" + strIdString + ")\n";

                string strCommand = "use " + this.m_strSqlDbName + " \n"
                    + strSelect
                    + " use master " + "\n";

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 一个记录也不存在
                        if (dr == null
                            || dr.HasRows == false)
                            return 0;

                        while (dr.Read())
                        {
                            RecordRowInfo row_info = new RecordRowInfo();

                            if (dr.IsDBNull(0) == false)
                                row_info.data_textptr = (byte[])dr[0];

                            if (dr.IsDBNull(1) == false)
                                row_info.data_length = dr.GetInt32(1);

                            if (dr.IsDBNull(2) == false)
                                row_info.newdata_textptr = (byte[])dr[2];

                            if (dr.IsDBNull(3) == false)
                                row_info.newdata_length = dr.GetInt32(3);

                            if (dr.IsDBNull(4) == false)
                                row_info.Range = dr.GetString(4);

                            if (dr.IsDBNull(5) == false)
                                row_info.TimestampString = dr.GetString(5);
                            // TODO: 如果能先比较时间戳，假如发生了时间戳不一致的情况，就可以避免后面获取 data bytes 的多余动作了。可以用 delegate 实现查询某个 ID 对应的提交上来的时间戳

                            if (dr.IsDBNull(6) == false)
                                row_info.Metadata = dr.GetString(6);

                            if (dr.IsDBNull(7) == false)
                                row_info.NewTimestampString = dr.GetString(7);

                            if (dr.IsDBNull(8) == false)
                                row_info.FileName = dr.GetString(8);

                            if (dr.IsDBNull(9) == false)
                                row_info.NewFileName = dr.GetString(9);

                            if (dr.IsDBNull(10) == false)
                                row_info.ID = dr.GetString(10);

                            if (bGetData == true)
                            {
                                // 对象文件
                                if (String.IsNullOrEmpty(row_info.Range) == false
        && row_info.Range[0] == '#')
                                {
                                    nRet = ReadObjectFileContent(row_info,
            out strError);
                                    if (nRet == -1)
                                        return -1;  // TODO: 是否尽量多读入数据，最后统一警告或者报错?
                                    goto CONTINUE;
                                }

                                // 是否可以根据方向标志，仅取完成了的 data? 这样可以避免浪费资源
                                if (dr.IsDBNull(11) == false && row_info.data_length <= 1024 * 1024)
                                {
                                    row_info.Data = new byte[row_info.data_length];
                                    dr.GetBytes(11, 0, row_info.Data, 0, (int)row_info.data_length);
                                }

                                if (dr.IsDBNull(12) == false && row_info.newdata_length <= 1024 * 1024)
                                {
                                    row_info.NewData = new byte[row_info.newdata_length];
                                    dr.GetBytes(12, 0, row_info.NewData, 0, (int)row_info.newdata_length);
                                }
                            }
                            CONTINUE:
                            row_infos.Add(row_info);
                        }
                    }
                } // end of using command

                return 0;
            }
            #endregion // MS SQL Server

            #region SQLite
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = " SELECT "
                    + " range," // 0 
                    + " dptimestamp,"   // 1
                    + " metadata, "  // 2
                    + " newdptimestamp,"   // 3
                    + " filename,"   // 4
                    + " newfilename,"   // 5
                    + " id"             // 6
                    + " FROM records "
                    + " WHERE id in (" + strIdString + ")\n";

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {

                    using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在
                        if (dr == null
                            || dr.HasRows == false)
                            return 0;

                        // 如果记录已经存在
                        while (dr.Read())
                        {
                            RecordRowInfo row_info = new RecordRowInfo();

                            if (dr.IsDBNull(0) == false)
                                row_info.Range = dr.GetString(0);

                            if (dr.IsDBNull(1) == false)
                                row_info.TimestampString = dr.GetString(1);

                            if (dr.IsDBNull(2) == false)
                                row_info.Metadata = dr.GetString(2);

                            if (dr.IsDBNull(3) == false)
                                row_info.NewTimestampString = dr.GetString(3);

                            if (dr.IsDBNull(4) == false)
                                row_info.FileName = dr.GetString(4);

                            if (dr.IsDBNull(5) == false)
                                row_info.NewFileName = dr.GetString(5);

                            if (dr.IsDBNull(6) == false)
                                row_info.ID = dr.GetString(6);

                            if (bGetData == true)
                            {
                                nRet = ReadObjectFileContent(row_info,
        out strError);
                                if (nRet == -1)
                                    return -1;  // TODO: 是否尽量多读入数据，最后统一警告或者报错?
                            }

                            row_infos.Add(row_info);
                        }
                    }
                } // end of using command

                return 0;
            }
            #endregion // SQLite

            #region MySql
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                string strCommand = " SELECT "
                    + " `range`," // 0 
                    + " dptimestamp,"   // 1
                    + " metadata, "  // 2
                    + " newdptimestamp,"   // 3
                    + " filename,"   // 4
                    + " newfilename,"   // 5
                    + " id"             // 6
                    + " FROM `" + this.m_strSqlDbName + "`.records "
                    + " WHERE id in (" + strIdString + ") \n";

                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {

                    using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在，需要创建
                        if (dr == null
                            || dr.HasRows == false)
                            return 0;

                        // 如果记录已经存在
                        while (dr.Read())
                        {
                            RecordRowInfo row_info = new RecordRowInfo();

                            if (dr.IsDBNull(0) == false)
                                row_info.Range = dr.GetString(0);

                            if (dr.IsDBNull(1) == false)
                                row_info.TimestampString = dr.GetString(1);

                            if (dr.IsDBNull(2) == false)
                                row_info.Metadata = dr.GetString(2);

                            if (dr.IsDBNull(3) == false)
                                row_info.NewTimestampString = dr.GetString(3);

                            if (dr.IsDBNull(4) == false)
                                row_info.FileName = dr.GetString(4);

                            if (dr.IsDBNull(5) == false)
                                row_info.NewFileName = dr.GetString(5);

                            if (dr.IsDBNull(6) == false)
                                row_info.ID = dr.GetString(6);

                            if (bGetData == true)
                            {
                                nRet = ReadObjectFileContent(row_info,
        out strError);
                                if (nRet == -1)
                                    return -1;  // TODO: 是否尽量多读入数据，最后统一警告或者报错?
                            }
                            row_infos.Add(row_info);
                        }
                    }

                } // end of using command

                return 0;
            }
            #endregion // MySql

            #region Oracle
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                string strCommand = " SELECT "
                    + " range," // 0
                    + " dptimestamp,"   // 1
                    + " metadata, "  // 2
                    + " newdptimestamp,"   // 3
                    + " filename,"   // 4
                    + " newfilename,"   // 5
                    + " id"             // 6
                    + " FROM " + this.m_strSqlDbName + "_records "
                    + " WHERE id in (" + strIdString + ") \n";

                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {

                    using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在
                        if (dr == null
                            || dr.HasRows == false)
                            return 0;

                        // 如果记录已经存在
                        while (dr.Read())
                        {
                            RecordRowInfo row_info = new RecordRowInfo();

                            if (dr.IsDBNull(0) == false)
                                row_info.Range = dr.GetString(0);

                            if (dr.IsDBNull(1) == false)
                                row_info.TimestampString = dr.GetString(1);

                            if (dr.IsDBNull(2) == false)
                                row_info.Metadata = dr.GetString(2);

                            if (dr.IsDBNull(3) == false)
                                row_info.NewTimestampString = dr.GetString(3);

                            if (dr.IsDBNull(4) == false)
                                row_info.FileName = dr.GetString(4);

                            if (dr.IsDBNull(5) == false)
                                row_info.NewFileName = dr.GetString(5);

                            if (dr.IsDBNull(6) == false)
                                row_info.ID = dr.GetString(6);

                            if (bGetData == true)
                            {
                                nRet = ReadObjectFileContent(row_info,
        out strError);
                                if (nRet == -1)
                                    return -1;  // TODO: 是否尽量多读入数据，最后统一警告或者报错?
                            }

                            row_infos.Add(row_info);
                        }
                    }
                } // end of using command

                return 0;
            }
            #endregion // Oracle

            return 0;
        }

        public int ReadObjectFileContent(RecordRowInfo row_info,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(row_info.FileName) == true)
                row_info.Data = new byte[0];
            else
            {
                string strObjectFilename = GetObjectFileName(row_info.FileName);
                try
                {
                    row_info.Data = null;
                    // 一次性读入全部文件内容，可以不涉及到 Cache 使用
                    using (FileStream s = File.Open(
        strObjectFilename,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite))
                    {
                        if (s.Length > 1024 * 1024)
                        {
                            return 0;   // 文件尺寸太大，不合适放到 byte [] 中
                        }
                        row_info.Data = new byte[s.Length];
                        s.Read(row_info.Data,
                            0,
                            (int)s.Length);
                    }
                }
                catch (FileNotFoundException /* ex */)
                {
                    // TODO: 不要直接汇报物理文件名
                    strError = "对象文件 '" + strObjectFilename + "' 不存在";
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "读取对象文件 '" + strObjectFilename + "' 时发生错误: " + ex.Message;
                    return -1;
                }
            }
            return 0;
        }

        // 将 Session 中和本数据库有关的缓冲数据成批写入 SQL Server
        // parameters:
        //      strAction   "getdelaysize"  返回需要写入的信息数量
        //                  ""  进行 BulkCopy
        public override long BulkCopy(
            // SessionInfo sessioninfo,
            string strAction,
            out string strError)
        {
            strError = "";

            if (this.container.SqlServerType != SqlServerType.MsSqlServer
                && this.container.SqlServerType != SqlServerType.Oracle
                && this.container.SqlServerType != SqlServerType.MySql
                && this.container.SqlServerType != SqlServerType.SQLite)
            {
                strError = "BulkCopy() 不支持 " + this.container.SqlServerType.ToString() + " 类型的数据库";
                return -1;
            }

            if (this.container.DelayTables == null || this.container.DelayTables.Count == 0)
                return 0;

            List<DelayTable> tables = this.container.DelayTables.GetTables(this.m_strSqlDbName);
            if (tables.Count == 0)
                return 0;

            if (strAction == "getdelaysize")
            {
                long lSize = 0;
                foreach (DelayTable table in tables)
                {
                    lSize += table.Size;
                }

                return lSize;
            }

            bool bFastMode = false;

            // 必须在没有加锁的时候调用
            if (this.container.SqlServerType == SqlServerType.SQLite)
                this.Commit();

            //*********对数据库加读锁*************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
            try
            {
                Connection connection = GetConnection(
        this.m_strLongConnString,   // this.m_strConnString,
        this.container.SqlServerType == SqlServerType.SQLite && bFastMode == true ? ConnectionStyle.Global : ConnectionStyle.None);
                connection.TryOpen();
                try
                {
                    #region MS SQL Server
                    if (this.container.SqlServerType == SqlServerType.MsSqlServer)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        foreach (DelayTable table in tables)
                        {
                            var bulkCopy = new SqlBulkCopy(connection.SqlConnection);
                            bulkCopy.DestinationTableName = this.m_strSqlDbName + ".." + table.TableName;
                            bulkCopy.BulkCopyTimeout = m_nLongTimeout;  // 缺省为 30 (秒)
                            bulkCopy.BatchSize = 10000;  // 缺省为 0 ，表示全部在一批
                                                         // 42 万条书目记录，如果作为一批，需要 4 分钟；如果每 5000 条一批，需要 8 分钟
                            int nRet = table.OpenForRead(table.FileName, out strError);
                            if (nRet == -1)
                                return -1;
                            table.LockForRead();    // 这里读锁定整个对象。在 Read() 函数那里就不需要锁定了
                            try
                            {
                                bulkCopy.WriteToServer(table);
                            }
                            finally
                            {
                                table.UnlockForRead();
                            }
                            table.Free();
                            this.container.DelayTables.Remove(table);
                        }
                        watch.Stop();
                        this.container.KernelApplication.WriteErrorLog("MS SQL Server BulkCopy 耗时 " + watch.Elapsed.ToString());
                    }
                    #endregion // MS SQL Server

                    #region Oracle
                    //strError = "暂不支持";
                    //return -1;
                    if (this.container.SqlServerType == SqlServerType.Oracle)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        foreach (DelayTable table in tables)
                        {
                            // http://stackoverflow.com/questions/26941161/oraclebulkcopy-class-in-oracle-manageddataaccess-dll
                            // OracleBulkCopy Class in Oracle.ManagedDataAccess.dll?
                            var bulkCopy = new OracleBulkCopy(connection.OracleConnection);
                            bulkCopy.BatchSize = 5000;  // default is zero , whole in one batch
                            bulkCopy.BulkCopyTimeout = 20 * 60; // default is 30 deconds
                            bulkCopy.DestinationTableName = this.m_strSqlDbName + "_" + table.TableName;   // this.m_strSqlDbName + ".." + table.TableName;
                            int nRet = table.OpenForRead(table.FileName, out strError);
                            if (nRet == -1)
                                return -1;
                            table.LockForRead();    // 这里读锁定整个对象。在 Read() 函数那里就不需要锁定了
                            try
                            {
                                bulkCopy.WriteToServer(table);
                            }
                            finally
                            {
                                table.UnlockForRead();
                            }
                            table.Free();
                            this.container.DelayTables.Remove(table);
                        }
                        watch.Stop();
                        this.container.KernelApplication.WriteErrorLog("Oracle BulkCopy 耗时 " + watch.Elapsed.ToString());
                    }
                    #endregion // Oracle

                    #region MySql
                    if (this.container.SqlServerType == SqlServerType.MySql)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        foreach (DelayTable table in tables)
                        {
                            var bulkCopy = new MySqlBulkCopy(connection.MySqlConnection);
                            // 2017/4/27 MySQL Named Pipe 方式下 1000 比较保险
                            bulkCopy.BatchSize = 1000;  // 5000;
                            bulkCopy.DestinationTableName = "`" + this.m_strSqlDbName + "`." + table.TableName;
                            int nRet = table.OpenForRead(table.FileName, out strError);
                            if (nRet == -1)
                                return -1;

                            // TODO: 锁定前标示状态，便于前端探知数据库状态

                            table.LockForRead();    // 这里读锁定整个对象。在 Read() 函数那里就不需要锁定了
                            try
                            {
                                bulkCopy.WriteToServer(table);
                            }
                            finally
                            {
                                table.UnlockForRead();
                            }
                            table.Free();
                            this.container.DelayTables.Remove(table);
                        }
                        watch.Stop();
                        this.container.KernelApplication.WriteErrorLog("MySql BulkCopy 耗时 " + watch.Elapsed.ToString());
                    }
                    #endregion // MySql


                    #region SQLite
                    if (this.container.SqlServerType == SqlServerType.SQLite)
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        // this.CommitInternal(false);
                        foreach (DelayTable table in tables)
                        {
                            var bulkCopy = new SqliteBulkCopy(connection.SQLiteConnection);
                            bulkCopy.BatchSize = 5000;
                            bulkCopy.DestinationTableName = table.TableName;
                            int nRet = table.OpenForRead(table.FileName, out strError);
                            if (nRet == -1)
                                return -1;
                            table.LockForRead();    // 这里读锁定整个对象。在 Read() 函数那里就不需要锁定了
                            try
                            {
                                bulkCopy.WriteToServer(table);
                            }
                            finally
                            {
                                table.UnlockForRead();
                            }
                            table.Free();
                            this.container.DelayTables.Remove(table);
                        }
                        // this.CommitInternal(false);
                        // connection.Commit(false);
                        watch.Stop();
                        this.container.KernelApplication.WriteErrorLog("SQLite BulkCopy 耗时 " + watch.Elapsed.ToString());
                    }
                    #endregion // SQLite

                }
                catch (SqlException sqlEx)
                {
                    strError = "3 BulkCopy() 在给'" + this.GetCaption("zh-CN") + "'库写入记录时出错,原因:" + GetSqlErrors(sqlEx);
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "4 BulkCopy() 在给'" + this.GetCaption("zh-CN") + "'库写入记录时出错,原因:" + ex.Message;
                    return -1;
                }
                finally
                {
                    connection.Close();
                }
            }
            finally
            {
                //********对数据库解读锁****************
                m_db_lock.ReleaseReaderLock();
            }

            return 0;
        }

        // 写入一批 XML 记录；或者刷新一批记录的检索点
        // parameters:
        //      strStyle    rebuildkeys/deletekeys/fastmode/ifnotexist
        //                  simulate 模拟写入记录
        // return:
        //      -1  出错。注意，即便没有返回 -1，但 outputs 数组中也有可能有元素具有返回的错误信息
        //      >=0 如果是 rebuildkeys，则返回总共处理的 keys 行数
        public override int WriteRecords(
            // SessionInfo sessioninfo,
            User oUser,
            List<RecordBody> inputs,
            string strStyle,
            out List<RecordBody> outputs,
            out string strError)
        {
            strError = "";
            outputs = new List<RecordBody>();

            if (StringUtil.IsInList("fastmode", strStyle) == true)
                this.FastMode = true;
            bool bFastMode = StringUtil.IsInList("fastmode", strStyle) || this.FastMode;

            bool bRebuildKeys = StringUtil.IsInList("rebuildkeys", strStyle);
            bool bDeleteKeys = StringUtil.IsInList("deletekeys", strStyle);

            // 注： rebuildkeys表示本函数的主要功能是重建检索点。
            // 如果和 deletekeys 配套使用，则表示对每条记录重建的当时，就删除了已经存在的旧有检索点
            // 如果不配套 deletekeys，那么应当是在整个批处理前，全部删除 keys 表内容，使得本次重建检索点过程中不必再考虑逐条删除旧有检索点了，只需要创建就行
            // 如果配套 fastmode 使用，表示需要创建的 keys 延迟创建，最后 Bulkcopy 进入 keys 表。
            // 如果配套了 fastmode，并且没有预先删除全部 keys，则应当保持 keys 表的 B+ 树索引，这样才能让逐条删除旧有 keys 变得可行。不过，在 Bulkcopy 前仍需要注意删除 B+ 树索引，完成后重新创建

            List<RecordBody> error_records = new List<RecordBody>();

            List<WriteInfo> records = new List<WriteInfo>();

            foreach (RecordBody record in inputs)
            {
                string strPath = record.Path;   // 包括数据库名的路径

                string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                if (strDbName == ".")
                    strDbName = this.GetCaption("zh-CN");

                bool bObject = false;
                string strRecordID = "";
                string strObjectID = "";
                string strXPath = "";

                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                //***********吃掉第2层*************
                // 到此为止，strPath不含记录号层了，下级分情况判断
                strRecordID = strFirstPart;
                // 只到记录号层的路径
                if (strPath == "")
                {
                    bObject = false;
                }
                else
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath

                    if (strFirstPart != "object"
        && strFirstPart != "xpath")
                    {
                        record.Result.SetValue("资源路径 '" + record.Path + "' 不合法, 第3级必须是 'object' 或 'xpath' ",
                            ErrorCodeValue.PathError); // -7;
                        continue;
                    }
                    if (string.IsNullOrEmpty(strPath) == true)  //object或xpath下级必须有值
                    {
                        record.Result.SetValue("资源路径 '" + record.Path + "' 不合法,当第3级是 'object' 或 'xpath' 时，第4级必须有内容",
                            ErrorCodeValue.PathError); // -7;
                        continue;
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
                }

                if (bObject == true)
                {
                    record.Result.SetValue("目前不允许用 WriteRecords 写入对象资源",
                        ErrorCodeValue.CommonError);
                    continue;
                }

                if (strRecordID == "?")
                    strRecordID = "-1";

                if (bRebuildKeys == true && strRecordID == "-1")
                {
                    record.Result.SetValue("不允许用不确定的记录ID来重建检索点 (记录路径为 '" + record.Path + "')",
                        ErrorCodeValue.CommonError);
                    continue;
                }

                bool bSimulate = StringUtil.IsInList("simulate", strStyle);

                bool bPushTailNo = false;
                // 对 ？ 创建尾记录号
                bPushTailNo = this.EnsureID(ref strRecordID, bSimulate);

                // bPushed == true 说明没有必要 select 获取原有 records 行

                if (oUser != null)
                {
                    string strTempRecordPath = this.GetCaption("zh-CN") + "/" + strRecordID;
                    if (bPushTailNo == true)
                    {
                        string strExistRights = "";
                        bool bHasRight = oUser.HasRights(strTempRecordPath,
                            ResType.Record,
                            "create",//"append",
                            out strExistRights);
                        if (bHasRight == false)
                        {
                            strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                            record.Result.SetValue(strError,
                                ErrorCodeValue.NotHasEnoughRights);    // return -6;
                            error_records.Add(record);
                            continue;
                        }
                    }
                    else
                    {
                        string strExistRights = "";
                        bool bHasRight = oUser.HasRights(strTempRecordPath,
                            ResType.Record,
                            "overwrite",
                            out strExistRights);
                        if (bHasRight == false)
                        {
                            strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                            record.Result.SetValue(
                                strError,
                                ErrorCodeValue.NotHasEnoughRights,   // return -6;
                                -1);
                            error_records.Add(record);
                            continue;
                        }
                    }
                }

                // TODO: rebuild keys 需要什么权限 ?

                WriteInfo write_info = new WriteInfo();
                write_info.record = record;
                write_info.ID = strRecordID;
                write_info.Pushed = bPushTailNo;
                write_info.XPath = strXPath;
                if (string.IsNullOrEmpty(record.Xml) == false)
                {
                    byte[] baContent = Encoding.UTF8.GetBytes(record.Xml);
                    write_info.baContent = baContent;
                    string strRange = "0-" + (baContent.Length - 1).ToString();
                    write_info.strRange = strRange;
                }
                records.Add(write_info);
            }

            bool bIgnoreCheckTimestamp = StringUtil.IsInList("ignorechecktimestamp", strStyle);
            bool bIfNotExist = StringUtil.IsInList("ifnotexist", strStyle);

            //*********对数据库加读锁*************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("WriteRecords()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                List<string> locked_ids = WriteInfo.get_ids(records, true);
                //**********对记录加写锁***************
                this.m_recordLockColl.LockForWrite(ref locked_ids, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteRecords()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                try // 记录锁
                {
                    Connection connection = GetConnection(
        this.m_strConnString,
        this.container.SqlServerType == SqlServerType.SQLite && bFastMode == true ? ConnectionStyle.Global : ConnectionStyle.None);
                    connection.TryOpen();
                    try
                    {
                        // select 已经存在的行信息
                        // 获得多个已存在的行信息
                        int nRet = GetRowInfos(connection,
                            bRebuildKeys ? true : !bFastMode,
                            WriteInfo.get_ids(records),    // 采用 get_existing_ids 纯追加 40 万条书目数据才加快速度1分钟而已
                            out List<RecordRowInfo> row_infos,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // 把 row_infos 中的值 匹配 id 放到 infos的属性row_info中
                        WriteInfo.set_rowinfos(ref records, row_infos);

                        List<WriteInfo> passed = new List<WriteInfo>();
                        // 只创建那些原先在数据库中不存在的记录
                        if (bIfNotExist)
                        {
                            List<WriteInfo> temp = new List<WriteInfo>();
                            // 把 row_info 不为 null 的清除。不过追加方式的写入动作依然保留
                            foreach (WriteInfo info in records)
                            {
                                // TODO 也可以看 info.ID
                                if (info.row_info == null
                                    && info.record != null
                                    && IsAppend(info.record.Path) == false)
                                    temp.Add(info);
                                else
                                    passed.Add(info);
                            }
                            records = temp;
                        }

                        // 仅重建检索点
                        if (bRebuildKeys == true && records.Count > 0)
                        {
                            if (bFastMode == false && bDeleteKeys == false)
                            {
                                strError = "WriteRecords() 执行 rebuildkeys 功能时， 如果 style 中不包含 fastmode，则必须包含 deletekeys";
                                return -1;
                            }
                            //List<WriteInfo> temp = null;
                            // 更新 Keys
                            nRet = RebuildKeysRows(
                                connection,
                                bDeleteKeys,
                                bFastMode,
                                records,
                                // out temp,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            return nRet;
                        }

                        // 自此以后 .row_info 为空的就是要新创建的行

                        // 检查时间戳
                        if (bIgnoreCheckTimestamp == false)
                        {
                            for (int i = 0; i < records.Count; i++)
                            {
                                WriteInfo info = records[i];
                                Debug.Assert(string.IsNullOrEmpty(info.ID) == false, "");

                                if (info.row_info == null)
                                    continue;

                                byte[] baExistTimestamp = ByteArray.GetTimeStampByteArray(GetCompleteTimestamp(info.row_info));
                                if (ByteArray.Compare(info.record.Timestamp,
                                    baExistTimestamp) != 0)
                                {
                                    info.record.Timestamp = baExistTimestamp;   // 返回给前端，让前端能够得知当前的时间戳
                                    info.record.Result.Value = -1;
                                    info.record.Result.ErrorString = "时间戳不匹配";
                                    info.record.Result.ErrorCode = ErrorCodeValue.TimestampMismatch; //   return -2;

                                    error_records.Add(info.record);
                                    records.RemoveAt(i);
                                    i--;
                                    continue;
                                }
                            }

                        }

                        List<WriteInfo> results = null;
                        if (records.Count > 0)
                        {
                            // 创建或者更新 SQL 记录行
                            nRet = UpdateRecordRows(connection,
                                records,
                                strStyle,
                                out results,
                                out strError);
                            foreach (WriteInfo info in results)
                            {
                                string strPath = info.record.Path;   // 包括数据库名的路径
                                string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                                if (strDbName == ".")
                                    strDbName = this.GetCaption("zh-CN");
                                string strRecordID = DbPath.GetCompressedID(info.ID);
                                if (string.IsNullOrEmpty(info.XPath) == true)
                                    info.record.Path = strDbName + "/" + strRecordID;
                                else
                                    info.record.Path = strDbName + "/" + strRecordID + "/xpath/" + info.XPath;

                                outputs.Add(info.record);
                            }
                        }

                        // 把跳过的记录放入返回数组
                        foreach (WriteInfo info in passed)
                        {
                            string strPath = info.record.Path;   // 包括数据库名的路径
                            string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                            if (strDbName == ".")
                                strDbName = this.GetCaption("zh-CN");
                            string strRecordID = DbPath.GetCompressedID(info.ID);
                            if (string.IsNullOrEmpty(info.XPath) == true)
                                info.record.Path = strDbName + "/" + strRecordID;
                            else
                                info.record.Path = strDbName + "/" + strRecordID + "/xpath/" + info.XPath;
                            info.record.Result = new Result("记录已经存在，忽略本次写入", ErrorCodeValue.Canceled, -1);
                            outputs.Add(info.record);
                        }

                        // records中(报错时)没有来得及被处理的部分就不进入了
                        outputs.AddRange(error_records);
                        if (nRet == -1)
                            return -1;

                        if (results != null && results.Count > 0)
                        {
                            // 更新 Keys
                            nRet = UpdateKeysRows(
                                connection,
                                true,   // 始终要立即删除旧的 keys
                                bFastMode,
                                results,
                                out List<WriteInfo> temp,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 2017/5/31
                            if (temp != null)
                            {
                                foreach (WriteInfo info in temp)
                                {
                                    outputs.Add(info.record);
                                }
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = "3 WriteRecords() 在给'" + this.GetCaption("zh-CN") + "'库写入记录 '" + StringUtil.MakePathList(WriteInfo.get_ids(records)) + "' 时出错,原因:" + GetSqlErrors(sqlEx);
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        // TODO: 注意确保前端能用 MessageDlg 显示很长的报错信息
                        strError = "4 WriteRecords() 在给'" + this.GetCaption("zh-CN") + "'库写入记录 '" + StringUtil.MakePathList(WriteInfo.get_ids(records)) + "' 时出错,原因:" + ExceptionUtil.GetDebugText(ex);
                        return -1;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                finally  // 记录锁
                {
                    //******对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(locked_ids);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("WriteRecords()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif
                }

            }
            finally
            {
                //********对数据库解读锁****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteRecords()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
            return 0;
        }

        // 是否为追加方式的路径?
        static bool IsAppend(string strPath)
        {
            // bool bObject = false;
            string strObjectID = "";
            string strXPath = "";

            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
            string strRecordID = strFirstPart;
            // 只到记录号层的路径
            if (strPath == "")
            {
                // bObject = false;
            }
            else
            {
                strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                //***********吃掉第2层*************
                // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath

                if (strFirstPart != "object"
        && strFirstPart != "xpath")
                {
                    //record.Result.SetValue("资源路径 '" + record.Path + "' 不合法, 第3级必须是 'object' 或 'xpath' ",
                    //    ErrorCodeValue.PathError); // -7;
                    return false;
                }
                if (string.IsNullOrEmpty(strPath) == true)  //object或xpath下级必须有值
                {
                    //record.Result.SetValue("资源路径 '" + record.Path + "' 不合法,当第3级是 'object' 或 'xpath' 时，第4级必须有内容",
                    //    ErrorCodeValue.PathError); // -7;
                    return false;
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
            }

            if (string.IsNullOrEmpty(strRecordID) == true || strRecordID == "?" || strRecordID == "-1")
                return true;
            return false;
        }

        class WriteInfo
        {
            public string ID = "";
            public bool Pushed = false; // 即将写入的 ID 号码是否推动过当前尾号 ? 如果推动过(Pushed == true)，说明没有必要 select 获得 records 表中的已有行，也就是说 records 表中不会有行
            public string XPath = "";
            public RecordRowInfo row_info = null;
            public RecordBody record = null;
            public byte[] baContent = null;
            public string strRange = "";

            public static List<string> get_ids(List<WriteInfo> infos,
                bool bEnsure10 = false)
            {
                List<string> results = new List<string>();
                foreach (WriteInfo info in infos)
                {
                    if (bEnsure10 == true)
                        results.Add(DbPath.GetID10(info.ID));
                    else
                        results.Add(info.ID);
                }

                return results;
            }

            // 挑出哪些可能存在 records 行的 id 号，用于 select 前准备 id 的工作
            public static List<string> get_existing_ids(List<WriteInfo> infos)
            {
                List<string> results = new List<string>();
                foreach (WriteInfo info in infos)
                {
                    if (info.Pushed == false)
                        results.Add(info.ID);
                }

                return results;
            }

            // 把 row_infos 中的值 匹配 id 放到 infos 的属性 row_info 中
            public static void set_rowinfos(ref List<WriteInfo> infos,
                List<RecordRowInfo> row_infos)
            {
                if (row_infos == null || row_infos.Count == 0)
                    return;

                Hashtable id_table = new Hashtable();   // id --> RecordRowInfo

                foreach (RecordRowInfo row_info in row_infos)
                {
                    id_table[row_info.ID] = row_info;
                }

                foreach (WriteInfo info in infos)
                {
                    info.row_info = (RecordRowInfo)id_table[info.ID];
                }
            }
        }

        // 上次完整写入时的时间戳
        static string GetCompleteTimestamp(RecordRowInfo row_info)
        {
            string strCurrentRange = row_info.Range;

            if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '!')
            {
                return row_info.NewTimestampString; // 本次的时间戳
            }
            else
            {
                return row_info.TimestampString; // 上次完整写入时的时间戳
            }
        }

        // 上次完整写入时的记录体
        static byte[] GetCompleteData(RecordRowInfo row_info)
        {
            string strCurrentRange = row_info.Range;

            if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '!')
                return row_info.NewData;
            else
                return row_info.Data;
        }

        // 写xml数据
        // parameter:
        //		strID           记录ID -1:表示追加一条记录
        //		strRanges       目标的位置,多个range用逗号分隔
        //		nTotalLength    总长度
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp 返回的时间戳
        //		strOutputID     返回的记录ID,当strID == -1时,得到实际的ID
        //		strError        
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录不存在
        //      -6  权限不够
        //		0   成功
        public override int WriteXml(User oUser,  //null，则不检索权限
            string strID,
            string strXPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            string strMetadata,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp,
            out string strOutputID,
            out string strOutputValue,   //当AddInteger 或 AppendString时 返回值最后的值
            bool bCheckAccount,
            out string strError)
        {
            strOutputValue = "";
            outputTimestamp = null;
            strOutputID = "";
            strError = "";

            List<string> time_lines = new List<string>();
            DateTime start_time = DateTime.Now;
            DateTime start_time_out = new DateTime(0);  // 跳出 try 范围的开始时间

            if (StringUtil.IsInList("fastmode", strStyle) == true)
                this.FastMode = true;   // TODO: 那什么时候 this.FastMode 才会变回 false 呢？

            bool bFastMode = StringUtil.IsInList("fastmode", strStyle) || this.FastMode;

            if (strID == "?")
                strID = "-1";

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            bool bPushTailNo = false;
            bPushTailNo = this.EnsureID(ref strID, bSimulate);
            if (oUser != null)
            {
                DateTime start_time_1 = DateTime.Now;

                string strTempRecordPath = this.GetCaption("zh-CN") + "/" + strID;
                if (bPushTailNo == true)
                {
                    string strExistRights = "";
                    bool bHasRight = oUser.HasRights(strTempRecordPath,
                        ResType.Record,
                        "create",//"append",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }
                }
                else
                {
                    string strExistRights = "";
                    bool bHasRight = oUser.HasRights(strTempRecordPath,
                        ResType.Record,
                        "overwrite",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }
                }

                WriteTimeUsed(
        time_lines,
        start_time_1,
        "处理账户 耗时 ");
            }

            strOutputID = DbPath.GetCompressedID(strID);

            if (bSimulate)
            {
                // 注：目前暂不支持 AddInteger 或 AppendString 方式返回 strOutputValue
                outputTimestamp = ByteArray.GetTimeStampByteArray(CreateTimestampForDb());
                return 0;
            }

            int nRet = 0;

            bool bFull = false;
            bool bSingleFull = false;

            string strDbType = "";

            bool bDelete = false;   // 是否需要删除刚创建的记录？

            // 这里不再因为FastMode加写锁

            //*********对数据库加读锁*************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif

            try
            {
                strDbType = this.GetDbType();

                strID = DbPath.GetID10(strID);
                //**********对记录加写锁***************
                this.m_recordLockColl.LockForWrite(strID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                try // 记录锁
                {
                    DateTime start_time_open = DateTime.Now;

                    Connection connection = GetConnection(
                        this.m_strConnString,
                        this.container.SqlServerType == SqlServerType.SQLite && bFastMode == true ? ConnectionStyle.Global : ConnectionStyle.None);
                    connection.TryOpen();
                    try
                    {

                        WriteTimeUsed(
        time_lines,
        start_time_open,
        "Open Connection 耗时 ");

#if NO
                            // 1.如果记录不存在,插入一条字节的记录,以确保得到textPtr
                            // return:
                            //		-1  出错
                            //      0   不存在
                            //      1   存在
                            nRet = this.RecordIsExist(connection,
                                strID,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            bool bExist = false;
                            if (nRet == 1)
                                bExist = true;

                            // 新记录时，插入一个字节，并生成新时间戳
                            if (bExist == false)
                            {
                                byte[] tempInputTimestamp = inputTimestamp;
                                // 注意新记录的时间戳,用inputTimestamp变量
                                nRet = this.InsertRecord(connection,
                                    strID,
                                    out inputTimestamp,//tempTimestamp,//
                                    out strError);

                                if (nRet == -1)
                                    return -1;
                            }
#endif
                        // 视情况创建新记录，返回行信息
                        RecordRowInfo row_info = null;
                        // return:
                        //		-1  出错
                        //      0   没有创建新记录
                        //      1   创建了新的记录(也就意味着原先记录并不存在)
                        //      2   需要创建新的记录，但因为优化的缘故(稍后需要创建)而没有创建
                        nRet = this.CreateNewRecordIfNeed(connection,
                            strID,
                            null,
                            out row_info,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        bool bExist = false;
                        if (nRet == 0)
                            bExist = true;

                        // 2015/9/4
                        if (bExist == false)
                        {
                            // 此后若抛出异常，会自动删除新创建的记录
                            bDelete = true;
                        }

                        bool bNeedInsertRow = false;
                        if (nRet == 2)
                            bNeedInsertRow = true;

                        bool bForceDeleteKeys = false;  // 是否要强制删除已经存在的keys

                        string strOldXml = "";
                        byte[] baOldPreamble = new byte[0];

                        string strExistingRanges = GetPureRangeString(row_info.Range);

                        // 判断本次上载完成后，是否覆盖全范围
                        bFull = IsFull(
        strExistingRanges,
        lTotalLength,
        strRanges,
        baSource.Length);
                        // 如果预计到本次写入操作后temp区会完成，则提前取出已经存在的xml字符串
                        if (bFull == true)
                        {
                            // 获得已经存在的记录的XML字符串
                            if (string.IsNullOrEmpty(strOldXml) == true
                                && bExist == true)
                            {
                                DateTime start_time_getxmldata = DateTime.Now;

                                // return:
                                //      -1  出错
                                //      -4  记录不存在
                                //      -100    对象文件不存在
                                //      0   正确
                                nRet = this.GetXmlData(
                                    connection,
                                    row_info,
                                    strID,
                                    false,
                                    out strOldXml,
                                    out baOldPreamble,
                                    out strError);
                                if (nRet == -100)
                                {
                                    // 要写入时，发现即将被覆盖的位置对象文件不存在
                                    strOldXml = "";
                                    baOldPreamble = new byte[0];
                                    bForceDeleteKeys = true;
                                }
                                else
                                {
                                    if (nRet <= -1 && nRet != -3)   // ?? -3是什么情况
                                        return nRet;
                                }

                                WriteTimeUsed(
        time_lines,
        start_time_getxmldata,
        "GetXmlData 耗时 ");
                            }
                        }

                        int nWriteCount = 0;
                        if (string.IsNullOrEmpty(strXPath) == false
                            && IsSingleFull(strRanges, baSource, lTotalLength) == true)
                        {
                            // 如果一次性就发来了全部数据, 并且为xpath方式写入，那么
                            // 需要省去这次写入记录的操作，在后面直接写入即可
                            Debug.Assert(bFull == true, "");
                            bFull = true;
                            bSingleFull = true;
                        }
                        else
                        {
                            DateTime start_time_writesqlrecord = DateTime.Now;

                            // 写数据
                            // return:
                            //		-1	一般性错误
                            //		-2	时间戳不匹配
                            //		0	成功
                            nRet = this.WriteSqlRecord(connection,
                                ref row_info,
                                bNeedInsertRow,
                                strID,
                                strRanges,
                                lTotalLength,
                                baSource,
                                // streamSource,
                                strMetadata,
                                strStyle,
                                inputTimestamp,
                                out outputTimestamp,
                                out bFull,
                                out bSingleFull,
                                out strError);
                            if (nRet <= -1)
                                return nRet;

                            nWriteCount++;

                            WriteTimeUsed(
        time_lines,
        start_time_writesqlrecord,
        "WriteSqlRecord 耗时 ");
                        }

                        // 检查范围
                        //string strCurrentRange = this.GetRange(connection,
                        //	strID);
                        if (bFull == true)  //覆盖完了
                        {
                            // 1.得到新旧检索点
                            byte[] baNewPreamble = new byte[0];
                            string strNewXml = "";

                            if (bSingleFull == true)
                            {
                                // 优化。不必从数据库中读取了
                                byte[] baPreamble = null;
                                strNewXml = DatabaseUtil.ByteArrayToString(baSource,
        out baPreamble);
                            }
                            else
                            {
                                DateTime start_time_getxmldata = DateTime.Now;

                                // return:
                                //      -1  出错
                                //      -4  记录不存在
                                //      -100    对象文件不存在
                                //      0   正确
                                nRet = this.GetXmlData(
                                    connection,
                                    row_info,
                                    strID,
                                    nWriteCount == 0 ? true : !true,  // "newdata",   // WriteSqlRecord()中已经颠倒过来了
                                    out strNewXml,
                                    out baNewPreamble,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                WriteTimeUsed(
        time_lines,
        start_time_getxmldata,
        "GetXmlData 2 耗时 ");
                            }

                            ////
                            ////
                            if (string.IsNullOrEmpty(strXPath) == false)
                            {
                                DateTime start_time_writesqlrecord = DateTime.Now;

                                // 根据参数中提供的局部内容创建出完整的记录
                                nRet = BuildRecordXml(
                                    strID,
                                    strXPath,
                                    strOldXml,
                                    ref strNewXml,
                                    baNewPreamble,
                                    out baSource,
                                    out strRanges,
                                    out strOutputValue,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                lTotalLength = baSource.Length;

                                // 写数据
                                // return:
                                //		-1	一般性错误
                                //		-2	时间戳不匹配
                                //		0	成功
                                nRet = this.WriteSqlRecord(connection,
                                    ref row_info,
                                    bNeedInsertRow,
                                    strID,
                                    strRanges,
                                    lTotalLength,
                                    baSource,
                                    // streamSource,
                                    strMetadata,
                                    strStyle,
                                    inputTimestamp,
                                    out outputTimestamp,
                                    out bFull,
                                    out bSingleFull,
                                    out strError);
                                if (nRet <= -1)
                                    return nRet;

                                nWriteCount++;
                                // 注：这次写操作后，如果是第二次写操作，range内的标记会再次反转
                                // 不过，后面的模块应该能够自动适应

                                WriteTimeUsed(
        time_lines,
        start_time_writesqlrecord,
        "WriteSqlRecord 2 耗时 ");
                            }

                            KeyCollection newKeys = null;
                            KeyCollection oldKeys = null;
                            XmlDocument newDom = null;
                            XmlDocument oldDom = null;

                            DateTime start_time_mergekeys = DateTime.Now;

                            // return:
                            //      -2  出错。strOldXml 结构不合法
                            //      -1  出错
                            //      0   成功
                            nRet = this.MergeKeys(strID,
                                strNewXml,
                                strOldXml,
                                true,
                                out newKeys,
                                out oldKeys,
                                out newDom,
                                out oldDom,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // 2021/8/27
                            if (nRet == -2)
                                bForceDeleteKeys = true;

                            WriteTimeUsed(
        time_lines,
        start_time_mergekeys,
        "MergeKeys 耗时 ");

                            if (bForceDeleteKeys == true)
                            {
                                // return:
                                //      -1  出错
                                //      0   成功
                                nRet = this.ForceDeleteKeys(connection,
                                    strID,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }

                            // 调试 ---

                            DateTime start_time_modifykeys = DateTime.Now;

                            // 处理检索点
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyKeys(connection,
                                newKeys,
                                oldKeys,
                                bFastMode,
                                out strError);
                            if (nRet == -1)
                            {
                                if (bExist == false)
                                {
                                    // 创建 record 行成功，但在创建检索点时出现错误。这样检索点就不正常了。一个办法是标注后，以后重试刷新检点；一个办法是现在立即删除 record 行和所有 keys 行
                                    bDelete = true;
                                    goto ERROR2;
                                }
                                return -1;
                            }

                            WriteTimeUsed(
        time_lines,
        start_time_modifykeys,
        "ModifyKeys 耗时 ");

                            // 注：如果因为旧的XML对象文件丢失，造成ModifyFiles()去创建已经存在的对象records行，那么创建自然会被忽视，没有什么副作用

                            DateTime start_time_modifyfile = DateTime.Now;

                            // 处理子文件
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyFiles(connection,
                                strID,
                                newDom,
                                oldDom,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            WriteTimeUsed(
        time_lines,
        start_time_modifyfile,
        "ModifyFiles 耗时 ");
                        }

                        bDelete = false;    // 此后走到 ERROR2 不会删除新创建的记录

                        start_time_out = DateTime.Now;
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = "3 WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时出错,原因:" + GetSqlErrors(sqlEx);
                        // return -1;
                        goto ERROR2;
                    }
                    catch (Exception ex)
                    {
                        strError = "4 WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时出错,原因:" + ex.Message;
                        // return -1;
                        goto ERROR2;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                finally  // 记录锁
                {
                    //******对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif
                }
            }
            finally
            {
                //********对数据库解读锁****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            WriteTimeUsed(
        time_lines,
        start_time_out,
        "Close Connection 等的耗时 ");

            // 当本函数被明知为账户库的写操作调用时, 一定要用bCheckAccount==false
            // 来调用，否则容易引起不必要的递归
            if (bFull == true
                && bCheckAccount == true
                && StringUtil.IsInList("account", strDbType/*this.TypeSafety*/) == true)
            {
                string strResPath = this.FullID + "/" + strID;

                this.container.UserColl.RefreshUserSafety(strResPath);
            }
            else
            {
                if (StringUtil.IsInList("fastmode", strStyle) == false
                    && this.FastMode == true)
                {
                    DateTime start_time_1 = DateTime.Now;

                    // this.FastMode = false;
                    this.Commit();

                    WriteTimeUsed(
        time_lines,
        start_time_1,
        "FastMode Commit 耗时 ");
                }
            }

            if (DateTime.Now - start_time > new TimeSpan(0, 0, 1))
            {
                WriteTimeUsed(
        time_lines,
        start_time,
        "WriteXml 总耗时 ");
                this.container.KernelApplication.WriteErrorLog(
                    "--- 写入 XML 记录的时间超过 1 秒。详情: \r\n"
                    + "记录路径: " + this.GetCaption("zh-CN") + "/" + strID + "\r\n"
                    + StringUtil.MakePathList(time_lines, ";\r\n")
                    + "\r\n");
            }
            return 0;
            ERROR2:
            if (bDelete == true)
            {
                string strError1 = "";
                byte[] baOutputTimestamp = null;
                // return:
                //		-1  一般性错误
                //		-2  时间戳不匹配
                //      -4  未找到记录
                //		0   成功
                nRet = DeleteRecord(
                    strID,
                    "",
                    null,
                    "deletekeysbyid,ignorechecktimestamp",
                    out baOutputTimestamp,
                    out strError1);
                if (nRet == -1 || nRet == -4)
                {
                    strError += "; 在删除刚创建的记录 '" + strID + "' 时又遇到出错: " + strError1;
                    this.container.KernelApplication.WriteErrorLog("*** Undo 创建记录过程中出错(此数据库检索点需要重建): " + strError);
                }

                // 尝试把 id 回收，下次重复使用就好了
                if (nRet == 0 || nRet == -4)
                    TryRecycleTailNo(strID);
            }

            return -1;
        }

        static void WriteTimeUsed(
        List<string> lines,
        DateTime start_time,
        string strPrefix)
        {
            TimeSpan delta = DateTime.Now - start_time;
            lines.Add(strPrefix + " " + delta.TotalSeconds.ToString("F3"));
        }

        // parameters:
        //      strRecordID   记录ID
        //      strObjectID  对象ID
        //      其它参数同WriteXml,无strOutputID参数
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录或对象资源不存在
        //      -6  权限不够
        //		0   成功
        public override int WriteObject(User user,
            string strRecordID,
            string strObjectID,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            string strMetadata,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";
            int nRet = 0;

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            if (StringUtil.IsInList("fastmode", strStyle) == true)
                this.FastMode = true;
            bool bFastMode = StringUtil.IsInList("fastmode", strStyle) || this.FastMode;

            if (user != null)
            {
                string strTempRecordPath = this.GetCaption("zh-CN") + "/" + strRecordID;
                string strExistRights = "";
                bool bHasRight = user.HasRights(strTempRecordPath,
                    ResType.Record,
                    "overwrite",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strTempRecordPath + "'记录没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }
            }

            // 这里不再因为FastMode加写锁

            //**********对数据库加读锁************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif

            try
            {
                string strOutputRecordID = "";
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.CanonicalizeRecordID(strRecordID,
                    out strOutputRecordID,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (strOutputRecordID == "-1")
                {
                    strError = "保存对象资源不支持记录号参数值为'" + strRecordID + "'。";
                    return -1;
                }
                strRecordID = strOutputRecordID;

                //**********对记录加写锁***************
                m_recordLockColl.LockForWrite(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加写锁。");
#endif
                try // 记录锁
                {
                    // 打开连接对象
                    Connection connection = GetConnection(
                        this.m_strConnString,
                        this.container.SqlServerType == SqlServerType.SQLite && bFastMode == true ? ConnectionStyle.Global : ConnectionStyle.None);
                    connection.TryOpen();
                    try // 连接
                    {
                        // TODO: 是否可以改进为，如果对象SQL记录行存在，就直接进行写入，只有当SQL记录行不存在的时候才对从属的XML记录进行检查，如果必要补充创建SQL记录行。这样可以提高执行速度
                        // TODO: 可以在lStart == 0 的第一次的时候检查
#if NO
                        // 1.在对应的xml数据，用对象路径找到对象ID
                        string strXml;
                        // return:
                        //      -1  出错
                        //      -4  记录不存在
        //      -100    对象文件不存在
                        //      0   正确
                        nRet = this.GetXmlString(connection,
                            strRecordID,
                            out strXml,
                            out strError);
                        if (nRet <= -1)
                        {
                            strError = "保存'" + strRecordID + "/" + strObjectID + "'资源失败，原因:" + strError;
                            return nRet;
                        }
                        XmlDocument xmlDom = new XmlDocument();
                        xmlDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                        xmlDom.LoadXml(strXml);

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDom.NameTable);
                        nsmgr.AddNamespace("dprms", DpNs.dprms);
                        XmlNode fileNode = xmlDom.DocumentElement.SelectSingleNode("//dprms:file[@id='" + strObjectID + "']", nsmgr);
                        if (fileNode == null)
                        {
                            strError = "在记录 '" + strRecordID + "' 的xml里没有找到对象ID '" + strObjectID + "' 对应的dprms:file节点";
                            return -1;
                        }
#endif

                        strObjectID = strRecordID + "_" + strObjectID;

                        // 2017/6/7
                        // 模拟写入
                        if (bSimulate == true)
                        {
                            outputTimestamp = inputTimestamp;
                            return 0;
                        }

                        /*
                        // 2. 当记录为空记录时,用update更改文本指针
                        if (this.IsEmptyObject(connection, strObjectID) == true)
                        {
                            // return
                            //		-1  出错
                            //		0   成功
                            nRet = this.UpdateObject(connection,
                                strObjectID,
                                out inputTimestamp,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                         * */
                        RecordRowInfo row_info = null;
                        // return:
                        //      -1  出错
                        //      0   记录不存在
                        //      1   成功
                        nRet = GetRowInfo(connection,
        strObjectID,
        out row_info,
        out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)  // 2013/11/21
                            return -4;

                        // 3.把数据写到range指定的范围
                        bool bFull = false; // 是否为最后完成的一次写入操作
                        bool bSingleFull = false;
                        // return:
                        //		-1	一般性错误
                        //		-2	时间戳不匹配
                        //		0	成功
                        nRet = this.WriteSqlRecord(connection,
                            ref row_info,
                            false,
                            strObjectID,
                            strRanges,
                            lTotalLength,
                            baSource,
                            // streamSource,
                            strMetadata,
                            strStyle,
                            inputTimestamp,
                            out outputTimestamp,
                            out bFull,
                            out bSingleFull,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        //string strCurrentRange = this.GetRange(connection,strObjectID);
                        if (bFull == true)  //覆盖完了
                        {
                            // TODO: 可否异步滞后执行?
                            if (strObjectID.Length > 10)
                            {
                                // strID 为 10 字符，或者 0000000000_0000 形态
                                string record_path = this.GetCacheRecPath(strObjectID);
                                _pageCache.ClearByRecPath(record_path,
                                    (filename) =>
                                    {
                                        this._streamCache.FileDelete(filename);
                                    });
                            }
#if NO111
                            // 1. 用newdata替换data字段
                            // return:
                            //      -1  出错
                            //      >=0   成功 返回影响的记录数
                            nRet = this.UpdateDataField(connection,
                                strObjectID,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 2. 删除newdata字段
                            // return:
                            //		-1  出错
                            //		0   成功
                            nRet = this.DeleteDuoYuImage(connection,
                                strObjectID,
                                "newdata",
                                0,
                                out strError);
                            if (nRet == -1)
                                return -1;
#endif

#if NO
                            string strRemoveFieldName = "";
                            byte[] remove_textptr = null;
                            long lRemoveLength = 0;
                            int nReverse = GetReverse(row_info.Range);
                            // 注意自从WriteSqlRecord()以后标志已经反转过来了，刚好表现了实际情况
                            if (nReverse == 0)
                            {
                                strRemoveFieldName = "newdata";
                                remove_textptr = row_info.newdata_textptr;
                                lRemoveLength = row_info.newdata_length;
                            }
                            else if (nReverse == 1)
                            {
                                strRemoveFieldName = "data";
                                remove_textptr = row_info.data_textptr;
                                lRemoveLength = row_info.data_length;
                            }

                            if (nReverse != -1
                                && lRemoveLength > 0 && remove_textptr != null)
                            {
                                // return:
                                //		-1  出错
                                //		0   成功
                                nRet = this.RemoveImage(connection,
                                    strRemoveFieldName,
                                    remove_textptr,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
#endif
                        }

                        /* // 不要在保存对象后修改记录的时间戳了
                                                // 负责修改一下记录的时间戳
                                                string strNewTimestamp = this.CreateTimestampForDb();
                                                // return:
                                                //      -1  出错
                                                //      >=0   成功 返回被影响的记录数
                                                nRet = this.SetTimestampForDb(connection,
                                                    strRecordID,
                                                    strNewTimestamp,
                                                    out strError);
                                                if (nRet == -1)
                                                    return -1;
                         * */
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);

                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入资源'" + strObjectID + "'时出错,原因:" + ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //*********对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解写锁。");
#endif

                }
            }
            finally
            {

                //************对数据库解读锁************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            if (StringUtil.IsInList("fastmode", strStyle) == false
        && this.FastMode == true)
            {
                this.Commit();
            }

            return 0;
        }

        // 获得纯粹的 range 字符串
        static string GetPureRangeString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == false)
            {
                if (strText[0] == '!' || strText[0] == '#')
                    return strText.Substring(1);
            }
            return strText;
        }

        // 判断本次上载完成后，是否覆盖全范围
        static bool IsFull(
            string strExistingRanges,
            long lTotalLength,
            string strThisRanges,
            int nThisLength)
        {
            // 准备rangelist
            RangeList rangeList = null;
            if (string.IsNullOrEmpty(strExistingRanges) == true)
            {
                rangeList = new RangeList();
            }
            else
            {
                try
                {
                    rangeList = new RangeList(strExistingRanges);
                }
                catch (Exception ex)
                {
                    string strError = "用字符串 '" + strExistingRanges + "' 创建 RangeList 时出错: " + ex.Message;
                    throw new Exception(strError);
                }

            }

            RangeList thisRangeList = null;

            try
            {
                thisRangeList = new RangeList(strThisRanges);
            }
            catch (Exception ex)
            {
                string strError = "用字符串 '" + strThisRanges + "' 创建 RangeList 时出错: " + ex.Message;
                throw new Exception(strError);
            }
            // 组合两个RangeList
            rangeList.AddRange(thisRangeList);

#if NO
            // 2015/1/21
            if (rangeList.Count == 0)
                return true;
#endif

            rangeList.Sort();
            rangeList.Merge();

            if (rangeList.Count == 1)
            {
                RangeItem item = (RangeItem)rangeList[0];

                if (item.lLength > lTotalLength)
                    return false;   // 唯一一个事项的长度居然超过检测的长度，通常表明有输入参数错误

                if (item.lStart == 0
                    && item.lLength == lTotalLength)
                    return true;    // 表示完全覆盖
            }

            return false;
        }

        static bool IsSingleFull(string strRanges,
            byte[] baSource,
            long lTotalLength)
        {
            // 准备rangelist
            RangeList rangeList = null;
            if (string.IsNullOrEmpty(strRanges) == true)
            {
                RangeItem rangeItem = new RangeItem();
                rangeItem.lStart = 0;
                rangeItem.lLength = baSource.Length;
                rangeList = new RangeList();
                rangeList.Add(rangeItem);
            }
            else
            {
                try
                {
                    rangeList = new RangeList(strRanges);
                }
                catch (Exception ex)
                {
                    string strError = "用字符串 '" + strRanges + "' 创建 RangeList 时出错: " + ex.Message;
                    throw new Exception(strError);
                }
            }

            // 一次性全写满的情况
            if (rangeList.Count == 1
                && rangeList[0].lStart == 0
                && rangeList[0].lLength == lTotalLength)
                return true;

            return false;
        }

        // 给sql库写一条记录
        // 把baContent或streamContent写到image字段中range指定目标位置,
        // 说明：sql中的记录可以是Xml体记录也可以对象资源记录
        // 如果本次temp区写入完成，则清除old区
        // parameters:
        //		connection	    连接对象	不能为null
        //		strID	        记录ID	不能为null或空字符串
        //		strRanges	    目标范围，多个范围用逗号分隔
        //		nTotalLength	记录内容总长度
        //						对于Sql Server目前只支持int，所以nTotalLength设为int类型，但对外接口是long
        //                      如果为 -1，表示本次不写入对象内容，只修改 metadata
        //		baSource	    内容字节数组	可以为null
        //		streamContent	内容流	可以为null
        //		strStyle	    风格
        //					    ignorechecktimestamp	忽略时间戳
        //		baInputTimestamp    输入的时间戳	可以为null
        //		baOutputTimestamp	out参数，返回的时间戳
        //		bFull	        out参数，记录是否被本次写满
        //		strError	    out参数，返回出错信息
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		0	成功
        // 说明	baContent与streamContent中谁有值就算谁
        private int WriteSqlRecord(Connection connection,
            ref RecordRowInfo row_info,
            bool bNeedInsertRow,
            string strID,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out bool bFull,
            out bool bSingleFull,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            bFull = false;
            bSingleFull = false;

            int nRet = 0;

            //-------------------------------------------
            //对输入参数做例行检查
            //-------------------------------------------

            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection, out strError);
            if (nRet == -1)
            {
                strError = "WriteSqlRecord()调用错误，" + strError;
                return -1;
            }
            Debug.Assert(nRet == 0, "");

            if (strID == null || strID == "")
            {
                strError = "WriteSqlRecord()调用错误，strID参数不能为null或空字符串。";
                return -1;
            }

            // 仅修改 metadata
            if (lTotalLength == -1)
            {
                string strTimestamp = row_info.GetTimestamp();
                byte[] baExistTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
                // 检查时间戳
                if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                {

                    if (baExistTimestamp != null
                        && ByteArray.Compare(baInputTimestamp,
                            baExistTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        baOutputTimestamp = baExistTimestamp;   // 返回给前端，让前端能够得知当前的时间戳
                        return -2;
                    }
                }

                string strResultMetadata = "";
                bool bIncReaderCount = StringUtil.IsInList("incReadCount", strStyle);
                // parameters:
                //		strOldMetadata	旧元数据
                //		strNewMetadata	新元数据
                //		lLength	长度 -1表示长度未知 -2表示长度不变
                //      strReadCount  读取次数。"" 表示不变(即不修改 readCount 属性内容), "+??"表示增加数量，"-??"表示减少数量，"??"表示直接修改为此数量
                //		strResult	out参数，返回合并后的元数据
                //		strError	out参数，返回出错信息
                // return:
                //		-1	出错
                //		0	成功
                nRet = DatabaseUtil.MergeMetadata(row_info.Metadata,
                    strMetadata,
                    -2,
                    bIncReaderCount ? "+1" : "",
                    out strResultMetadata,
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = WriteMetadataColumn(
                    connection,
                    strID,
                    strResultMetadata,
                    out strError);
                if (nRet == -1)
                    return -1;

                // TODO: 最好执行一次 API 就修改一次 timestamp

                // 返回时间戳给前端
                baOutputTimestamp = baExistTimestamp;
                return 0;
            }

            if (lTotalLength < 0)
            {
                strError = "WriteSqlRecord()调用错误，lTotalLength参数值不能为'" + Convert.ToString(lTotalLength) + "'，必须大于等于0。";
                return -1;
            }
            /*
            if (baSource == null && streamSource == null)
            {
                strError = "WriteSqlRecord()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteSqlRecord()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
             * */
            if (baSource == null)
            {
                strError = "WriteSqlRecord()调用错误，baSource参数不能为null。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";
            if (strRanges == null)
                strRanges = "";
            if (strMetadata == null)
                strMetadata = "";

            long nSourceTotalLength = baSource.Length;
            /*
            if (baSource != null)
                nSourceTotalLength = baSource.Length;
            else
                nSourceTotalLength = streamSource.Length;
             * */


            // 准备rangelist
            RangeList rangeList = null;
            if (string.IsNullOrEmpty(strRanges) == true)
            {
                RangeItem rangeItem = new RangeItem();
                rangeItem.lStart = 0;
                rangeItem.lLength = nSourceTotalLength;
                rangeList = new RangeList();
                rangeList.Add(rangeItem);
            }
            else
            {
                try
                {
                    rangeList = new RangeList(strRanges);
                }
                catch (Exception ex)
                {
                    strError = "用字符串 '" + strRanges + "' 创建 RangeList 时出错: " + ex.Message;
                    return -1;
                }
            }

            // 一次性全写满的情况
            if (rangeList.Count == 1
                && rangeList[0].lStart == 0
                && rangeList[0].lLength == lTotalLength)
            {
                bSingleFull = true;
            }

            bool bFirst = false;    // 是否为第一次写入
            if (rangeList.Count >= 1
        && rangeList[0].lStart == 0)
            {
                bFirst = true;
            }
#if NO
            //-------------------------------------------
            //开始做事情
            //-------------------------------------------

            ////////////////////////////////////////////////////
            // 检查记录是否存在,时间是否匹配,并得到长度,range与textPtr
            /////////////////////////////////////////////////////
            string strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT TEXTPTR("+strDataFieldName+"),"
                + " DataLength("+strDataFieldName+"),"
                + " range,"
                + " dptimestamp,"
                + " metadata "
                + " FROM records "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            byte[] textPtr = null;
            string strOldMetadata = "";
            string strCurrentRange = "";
            long lCurrentLength = 0;
            string strOutputTimestamp = "";

            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                // 1.记录不存在报错
                if (dr == null
                    || dr.HasRows == false)
                {
                    strError = "记录 '" + strID + "' 在库中不存在，正常情况下不应是这样";
                    return -1;
                }

                dr.Read();

                // 2.textPtr为null报错
                if (dr[0] is System.DBNull)
                {
                    strError = "TextPtr不可能为null";
                    return -1;
                }
                textPtr = (byte[])dr[0];

                // 3.时间戳不可能为null,时间戳不匹配报错
                if ((dr[4] is System.DBNull))
                {
                    strError = "时间戳不可能为null";
                    return -1;
                }

                // 当strStyle存在 ignorechecktimestamp时，不判断时间戳
                strOutputTimestamp = dr.GetString(3);
                baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                {
                    if (ByteArray.Compare(baInputTimestamp,
                        baOutputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }
                // 4.metadata为null报错
                if ((dr[4] is System.DBNull))
                {
                    strError = "Metadata不可能为null";
                    return -1;
                }
                strOldMetadata = dr.GetString(4);


                // 5.range为null的报错
                if ((dr[2] is System.DBNull))
                {
                    strError = "range此时也不可能为null";
                    return -1;
                }
                strCurrentRange = dr.GetString(2);

                // 6.取出长度
                lCurrentLength = dr.GetInt32(1);


                bool bRet = dr.Read();

                // 2008/3/13 
                if (bRet == true)
                {
                    // 还有一行
                    strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                    return -1;
                }
            }
            finally
            {
                dr.Close();
            }
#endif
            bool bObjectFile = false;

            string strCurrentRange = row_info.Range;
            bool bReverse = false;  // 方向标志。如果为false，表示 data 为正式内容，newdata为暂时内容

            string strDataFieldName = "newdata";    // 临时存储字段名
            byte[] textptr = row_info.newdata_textptr;  // 数据指针
            long lCurrentLength = row_info.newdata_length;  // 数据体积长度
            string strCompleteTimestamp = row_info.TimestampString; // 上次完整写入时的时间戳
            string strCurrentTimestamp = row_info.NewTimestampString; // 本次的时间戳

            // 已有数据的时间戳
            if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '!')
            {
                strCompleteTimestamp = row_info.NewTimestampString;
                strCurrentTimestamp = row_info.TimestampString;
            }

            if (this.m_lObjectStartSize != -1 && lTotalLength >= this.m_lObjectStartSize
#if !XML_WRITE_TO_FILE
 && (strID.Length > 10 || connection.SqlServerType != SqlServerType.MsSqlServer)   // 写入对象文件是只针对二进制对象，而不针对普通XML记录
#endif
                && String.IsNullOrEmpty(strCurrentRange) == false
                && strCurrentRange[0] == '#')
            {
                bObjectFile = true;
                strCurrentRange = strCurrentRange.Substring(1);

                lCurrentLength = GetObjectFileLength(strID, true);
            }
            else if (this.m_lObjectStartSize != -1 && lTotalLength >= this.m_lObjectStartSize
#if !XML_WRITE_TO_FILE
 && (strID.Length > 10 || connection.SqlServerType != SqlServerType.MsSqlServer)
#endif
 && (string.IsNullOrEmpty(strCurrentRange) == true || strCurrentRange == "!"))
            {
                bObjectFile = true;

                /*
                if (strCurrentRange == "!")
                    lCurrentLength = row_info.data_length;
                 * */
                // 原先是存储在image字段中，但是本次要改为存储在object file中，所以lCurrentLength理解为0
                lCurrentLength = 0;
                strCurrentRange = "";
            }
            else
            {
                if (String.IsNullOrEmpty(strCurrentRange) == false
                    && strCurrentRange[0] == '!')
                {
                    bReverse = true;
                    strCurrentRange = strCurrentRange.Substring(1);
                    strDataFieldName = "data";
                    textptr = row_info.data_textptr;
                    lCurrentLength = row_info.data_length;
                    strCompleteTimestamp = row_info.NewTimestampString;
                    strCurrentTimestamp = row_info.TimestampString;
                }

                if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '#')
                {
                    strCurrentRange = strCurrentRange.Substring(1);
                    if (string.IsNullOrEmpty(strCurrentRange) == false)
                    {
                        // TODO: 转换方式的时候需要处理
                    }
                }
            }

            // 当strStyle存在 ignorechecktimestamp时，不判断时间戳
            if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
            {
                // 如果临时区有内容，则要把临时时间戳用来比较。否则就比较完成区的时间戳
                if (string.IsNullOrEmpty(strCurrentRange) == false)
                {
                    // strCurrentTimestamp = strCurrentTimestamp;
                }
                else
                {
                    strCurrentTimestamp = strCompleteTimestamp;
                }

                if (string.IsNullOrEmpty(strCurrentTimestamp) == false)
                {
                    byte[] baExistTimestamp = ByteArray.GetTimeStampByteArray(strCurrentTimestamp);
                    if (ByteArray.Compare(baInputTimestamp,
                        baExistTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        baOutputTimestamp = baExistTimestamp;   // 返回给前端，让前端能够得知当前的时间戳
                        return -2;
                    }
                }
            }

            bool bDeleted = false;

            // 根据range写数据
            int nStartOfBuffer = 0;    // 缓冲区的位置
            int nState = 0;
            for (int i = 0; i < rangeList.Count; i++)
            {
                bool bCanDeleteDuoYu = false;  // 缺省不可能删除多余的长度

                RangeItem range = (RangeItem)rangeList[i];
                long lStartOfTarget = range.lStart;     // 恢复到image字段的位置  
                int nNeedReadLength = (int)range.lLength;   // 需要读缓冲区的长度
                if (rangeList.Count == 1 && nNeedReadLength == 0)
                {
                    bFull = true;
                    break;
                }

                string strThisEnd = Convert.ToString(lStartOfTarget + (Int64)nNeedReadLength - (Int64)1);

                Debug.Assert(strThisEnd.IndexOf("-") == -1, "");

                string strThisRange = Convert.ToString(lStartOfTarget)
                    + "-" + strThisEnd;

                string strNewRange;
                nState = RangeList.MergeContentRangeString(strThisRange,
                    strCurrentRange,
                    lTotalLength,
                    out strNewRange,
                    out strError);
                if (nState == -1)
                {
                    strError = "MergeContentRangeString() error 4 : " + strError + " (strThisRange='" + strThisRange + "' strCurrentRange='" + strCurrentRange + "' ) lTotalLength=" + lTotalLength.ToString() + "";
                    return -1;
                }
                if (nState == 1)  //范围已满
                {
                    bFull = true;
                    string strFullEnd = "";
                    int nPosition = strNewRange.IndexOf('-');
                    if (nPosition >= 0)
                        strFullEnd = strNewRange.Substring(nPosition + 1);

                    // 当为范围的最后一次,且本次范围的末尾等于总范围的末尾,且还没有删除时
                    if (i == rangeList.Count - 1
                        && (strFullEnd == strThisEnd)
                        && bDeleted == false)
                    {
                        bCanDeleteDuoYu = true;
                        bDeleted = true;
                    }
                }
                strCurrentRange = strNewRange;

                if (bObjectFile == true)
                {
                    // 写入对象文件

                    if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                    {
                        strError = "数据库尚未配置对象文件目录，但写入对象时出现了需要引用对象文件的情况";
                        return -1;
                    }

                    string strFileName = "";
                    if (bFirst == true)
                    {
                        strFileName = BuildObjectFileName(strID, true);
                        row_info.NewFileName = GetShortFileName(strFileName); // 记忆
                        if (row_info.NewFileName == null)
                        {
                            strError = "构造短文件名时出错。记录ID '" + strID + "', 对象文件目录 '" + this.m_strObjectDir + "', 物理文件名 '" + strFileName + "'";
                            return -1;
                        }
                    }
                    else
                    {
                        // 在还没有文件的情况下一上来就写入不是从0开始的部分
                        if (string.IsNullOrEmpty(row_info.NewFileName) == true)
                        {
                            strFileName = BuildObjectFileName(strID, true);
                            row_info.NewFileName = GetShortFileName(strFileName); // 记忆
                        }

                        Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");
                        strFileName = GetObjectFileName(row_info.NewFileName);
                    }

                    int nRedoCount = 0;
                    REDO:
                    try
                    {
#if NO
                        {
                            using (FileStream s = File.Open(
            strFileName,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite))
                            {
                                // 第一次写文件,并且文件长度大于对象总长度，则截断文件
                                if (bFirst == true && s.Length > lTotalLength)
                                    s.SetLength(0);

                                // s.Seek(lStartOfTarget, SeekOrigin.Begin);
                                s.FastSeek(lStartOfTarget); // 2017/9/5
                                s.Write(baSource,
                                    nStartOfBuffer,
                                    nNeedReadLength);
                            }
                        }
#endif
                        {
                            // 注：要纳入 StreamCache 管理
                            StreamItem item = _streamCache.GetWriteStream(strFileName,
                                true   // lStartOfTarget > CACHE_SIZE
                                );
                            try
                            {
                                // 第一次写文件,并且文件长度大于对象总长度，则截断文件
                                if (bFirst == true && item.FileStream.Length > lTotalLength)
                                    item.FileStream.SetLength(0);

                                // s.Seek(lStartOfTarget, SeekOrigin.Begin);
                                item.FileStream.FastSeek(lStartOfTarget); // 2017/9/5
                                item.FileStream.Write(baSource,
                                    nStartOfBuffer,
                                    nNeedReadLength);
                            }
                            finally
                            {
                                _streamCache.ReturnStream(item);
                            }
                        }
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        if (nRedoCount == 0)
                        {
                            // 创建中间子目录
                            PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                            nRedoCount++;
                            goto REDO;
                        }
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        strError = "写入文件 '" + strFileName + "' 时发生错误: " + ex.Message;
                        return -1;
                    }

                    lCurrentLength = Math.Max(lStartOfTarget + nNeedReadLength, lCurrentLength);
                }
                else
                {
                    // 应当已经准备好了行
                    Debug.Assert(bNeedInsertRow == false, "");

                    // return:	
                    //		-1  出错
                    //		0   成功
                    nRet = this.WriteImage(connection,
                        ref textptr,
                        ref lCurrentLength,   // 当前image的长度在不断的变化着
                        bCanDeleteDuoYu,
                        strID,
                        strDataFieldName,   // "newdata",
                        lStartOfTarget,
                        baSource,
                        // streamSource,
                        nStartOfBuffer,
                        nNeedReadLength,
                        lTotalLength,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 转换存储方式时要及时删除原有的对象文件
                    if (string.IsNullOrEmpty(row_info.FileName) == false)
                    {
                        this._streamCache.FileDelete(GetObjectFileName(row_info.FileName));
                        row_info.FileName = "";
                    }
                    if (string.IsNullOrEmpty(row_info.NewFileName) == false)
                    {
                        this._streamCache.FileDelete(GetObjectFileName(row_info.NewFileName));
                        row_info.NewFileName = "";
                    }
                }

                nStartOfBuffer += nNeedReadLength;

                // textptr有可能被WriteImage()函数修改
                if (bReverse == false)
                {
                    row_info.newdata_textptr = textptr;
                    row_info.newdata_length = lCurrentLength;
                }
                else
                {
                    row_info.data_textptr = textptr;
                    row_info.data_length = lCurrentLength;
                }
            }

            string temp_filename = "";
            bool succeed = false;
            try
            {

                // TODO: 注意这里不要有多余的操作，注意速度问题
                if (bFull == true)
                {
#if NO
                if (bDeleted == false)
                {
                    // 当记录覆盖满时，删除多余的值
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = this.DeleteDuoYuImage(connection,
                        strID,
                        strDataFieldName,   // "newdata",
                        lTotalLength,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
#endif
                    strCurrentRange = "";
                    lCurrentLength = lTotalLength;

                    if (bObjectFile == true)
                    {
                        string strDeletedFilename = "";
                        // 对象文件改名
                        if (string.IsNullOrEmpty(row_info.FileName) == false)
                        {
                            strDeletedFilename = GetObjectFileName(row_info.FileName);

                            // this._streamCache.FileDelete(strDeletedFilename);   // 删除原有的正式文件

                            string strBackFileName = strDeletedFilename + ".bak";

                            this._streamCache.FileMove(strDeletedFilename,
                                strBackFileName,
                                true);   // 对原有的正式文件改名
                            temp_filename = strBackFileName;    // 记忆，在最后会删除这个文件
                        }

                        // 正式文件名重新命名
                        string strFileName = BuildObjectFileName(strID, false); // 长文件名
                        row_info.FileName = GetShortFileName(strFileName); // 短文件名

                        if (lTotalLength == 0)
                        {
                            nRet = CreateZeroLengthFile(strFileName,
                out strError);
                            if (nRet == -1)
                                return -1;

                            succeed = true;
#if NO
                        // 创建一个0bytes的文件
                        int nRedoCount = 0;
                    REDO:
                        try
                        {
                            using (FileStream s = File.Open(
    strFileName,
    FileMode.OpenOrCreate,
    FileAccess.Write,
    FileShare.ReadWrite))
                            {
                                s.SetLength(0);
                            }
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            if (nRedoCount == 0)
                            {
                                // 创建中间子目录
                                PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                                nRedoCount++;
                                goto REDO;
                            }
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            strError = "创建0字节的文件 '" + strFileName + "' 时出错：" + ex.Message;
                            return -1;
                        }
#endif
                        }
                        else
                        {
                            Debug.Assert(string.IsNullOrEmpty(row_info.NewFileName) == false, "");
                            string strSourceFilename = GetObjectFileName(row_info.NewFileName);

                            if (strDeletedFilename != strFileName)
                            {
                                this._streamCache.FileDelete(strFileName);   // 防备性删除已经存在的目标文件。TODO: 或者出错以后再重试?
                            }

                            try
                            {
                                // TODO: 这里可否进行一些保护？意思是一旦这部分代码出错返回，不至于出现后面访问记录“对象文件不存在”的报错
                                // File.Move(strSourceFilename, strFileName);    // 改名
                                _streamCache.FileMove(strSourceFilename, strFileName, false);
                                succeed = true;
                            }
                            catch (FileNotFoundException /* ex */)
                            {
                                // 如果源文件不存在
                                strError = "对象文件(临时文件) '" + strSourceFilename + "' 不存在...";
                                return -1;
                            }


                        }

                        row_info.NewFileName = "";
                    }
                }
                else
                {
                    lCurrentLength = -1;
                }
            }
            finally
            {
                if (succeed && string.IsNullOrEmpty(temp_filename) == false)
                {
                    this._streamCache.FileDelete(temp_filename);   // 这里才删除 .bak 文件
                }
            }

            {
                // 最后,更新range,metadata,dptimestamp;

                // 得到组合后的Metadata;
                string strResultMetadata = "";
                if (bFull == true)
                {
                    bool bIncReaderCount = StringUtil.IsInList("incReadCount", strStyle);
                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = DatabaseUtil.MergeMetadata(row_info.Metadata,
                        strMetadata,
                        lCurrentLength,
                        bIncReaderCount ? "+1" : "",
                        out strResultMetadata,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 2013/11/23
                // 是否要直接利用输入的时间戳
                bool bForceTimestamp = StringUtil.IsInList("forcesettimestamp", strStyle);

                // 生成新的时间戳,保存到数据库里
                string strOutputTimestamp = "";
                if (bForceTimestamp == true)
                    strOutputTimestamp = ByteArray.GetHexTimeStampString(baInputTimestamp);
                else
                    strOutputTimestamp = this.CreateTimestampForDb();

                // 写入 records 表
                nRet = WriteLine(
                connection,
                ref row_info,
                strID,
                strOutputTimestamp,
                strCurrentRange,
                strResultMetadata,
                bObjectFile,
                bFull,
                bReverse,
                bNeedInsertRow,
                "", // strPartList,
                out strError);
                if (nRet == -1)
                    return -1;
#if NO
            string strCommand = "";
            if (bObjectFile == false)
            {
                string strSetNull = ""; // 设置即将删除的 timestamp 字段内容为空的语句
                if (bFull == true)
                {
                    strSetNull = (bReverse == true ? " newdptimestamp=NULL, newdata=NULL," : " dptimestamp=NULL, data=NULL,");
                    // 时间戳和data内容都清除
                }

                strCommand = "use " + this.m_strSqlDbName + "\n"
                    + " UPDATE records "
                    + (bReverse == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                    + strSetNull
                    + " range=@range,"
                    + " filename=NULL, newfilename=NULL,"
                    + " metadata=@metadata "
                    + " WHERE id=@id";
            }
            else
            {
                string strSetNull = ""; // 设置即将删除的 timestamp 字段内容为空的语句
                if (bFull == true)
                    strSetNull = " newdptimestamp=NULL,";

                if (connection.SqlServerType == SqlServerType.MsSqlServer)
                {
                    strCommand = "use " + this.m_strSqlDbName + "\n"
                         + " UPDATE records "
                         + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                         + strSetNull
                         + " range=@range,"
                         + " metadata=@metadata,"
                         + (bFull == true ? " filename=@filename, newfilename=NULL," : " newfilename=@filename,")
                         + " data=NULL, newdata=NULL "
                         + " WHERE id=@id";
                    strCommand += " use master " + "\n";
                }
                else if (connection.SqlServerType == SqlServerType.SQLite)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE records "
                             + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                             + strSetNull
                             + " range=@range,"
                             + " metadata=@metadata,"
                             + (bFull == true ? " filename=@filename, newfilename=NULL " : " newfilename=@filename ")
                             + " WHERE id=@id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO records(id, range, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES(@id, @range, @metadata, @dptimestamp, NULL, @filename, NULL)"
                                             : " VALUES(@id, @range, @metadata, NULL, @dptimestamp, NULL, @filename)");

                    }
                }
                else if (connection.SqlServerType == SqlServerType.MySql)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE `" + this.m_strSqlDbName + "`.records "
                             + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                             + strSetNull
                             + " `range`=@range,"
                             + " metadata=@metadata,"
                             + (bFull == true ? " filename=@filename, newfilename=NULL " : " newfilename=@filename ")
                             + " WHERE id=@id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO `" + this.m_strSqlDbName + "`.records (id, `range`, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES (@id, @range, @metadata, @dptimestamp, NULL, @filename, NULL)"
                                             : " VALUES (@id, @range, @metadata, NULL, @dptimestamp, NULL, @filename)");

                    }
                }
                else if (connection.SqlServerType == SqlServerType.Oracle)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE " + this.m_strSqlDbName + "_records "
                             + (bFull == true ? " SET dptimestamp=:dptimestamp," : " SET newdptimestamp=:dptimestamp,")
                             + strSetNull
                             + " range=:range,"
                             + " metadata=:metadata,"
                             + (bFull == true ? " filename=:filename, newfilename=NULL " : " newfilename=:filename ")
                             + " WHERE id=:id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO " + this.m_strSqlDbName + "_records (id, range, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES (:id, :range, :metadata, :dptimestamp, NULL, :filename, NULL)"
                                             : " VALUES (:id, :range, :metadata, NULL, :dptimestamp, NULL, :filename)");

                    }
                }
            }

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    SqlParameter idParam = command.Parameters.Add("@id",
        SqlDbType.NVarChar);
                    idParam.Value = strID;

                    SqlParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        SqlDbType.NVarChar,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    SqlParameter rangeParam =
                        command.Parameters.Add("@range",
                        SqlDbType.NVarChar,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        if (bFull == true)
                            rangeParam.Value = (bReverse == false ? "!" : "") + strCurrentRange;   // 翻转
                        else
                            rangeParam.Value = (bReverse == true ? "!" : "") + strCurrentRange;   // 不翻转
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    SqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        SqlDbType.NVarChar,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        SqlParameter filenameParam =
                command.Parameters.Add("@filename",
                SqlDbType.NVarChar,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    int nCount = command.ExecuteNonQuery();
                    if (nCount == 0)
                    {
                        strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {

                    SQLiteParameter idParam = command.Parameters.Add("@id",
                        DbType.String);
                    idParam.Value = strID;

                    SQLiteParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        DbType.String,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    SQLiteParameter rangeParam =
                        command.Parameters.Add("@range",
                        DbType.String,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                        /*
                        if (bFull == true)
                            rangeParam.Value = (bReverse == false ? "!" : "") + strCurrentRange;   // 翻转
                        else
                            rangeParam.Value = (bReverse == true ? "!" : "") + strCurrentRange;   // 不翻转
                         * */
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现


                    SQLiteParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        DbType.String,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        SQLiteParameter filenameParam =
                command.Parameters.Add("@filename",
                DbType.String,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {
                    MySqlParameter idParam = command.Parameters.Add("@id",
                        MySqlDbType.String);
                    idParam.Value = strID;

                    MySqlParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        MySqlDbType.String,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    MySqlParameter rangeParam =
                        command.Parameters.Add("@range",
                        MySqlDbType.String,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    MySqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        MySqlDbType.String,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        MySqlParameter filenameParam =
                command.Parameters.Add("@filename",
                MySqlDbType.String,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command

            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                // 注： Oracle 这里和 MySql 基本一样
                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {

                    command.BindByName = true;

                    OracleParameter idParam = command.Parameters.Add(":id",
                        OracleDbType.NVarchar2);
                    idParam.Value = strID;

                    OracleParameter dptimestampParam =
                        command.Parameters.Add(":dptimestamp",
                        OracleDbType.NVarchar2,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    OracleParameter rangeParam =
                        command.Parameters.Add(":range",
                        OracleDbType.NVarchar2,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    OracleParameter metadataParam =
                        command.Parameters.Add(":metadata",
                        OracleDbType.NVarchar2,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        OracleParameter filenameParam =
                command.Parameters.Add(":filename",
                OracleDbType.NVarchar2,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command

            }
            else
            {
                strError = "未能识别的 SqlServerType '"+connection.SqlServerType.ToString()+"'";
                return -1;
            }

#endif

                baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);    // Encoding.UTF8.GetBytes(strOutputTimestamp);

                // 本次变化后的时间戳
                if (bObjectFile == true)
                {
                    if (bFull == true)
                    {
                        row_info.TimestampString = strOutputTimestamp;
                        row_info.NewTimestampString = "";
                    }
                    else
                    {
                        row_info.NewTimestampString = strOutputTimestamp;
                    }
                }
                else
                {
                    if (bReverse == false)
                        row_info.NewTimestampString = strOutputTimestamp;
                    else
                        row_info.TimestampString = strOutputTimestamp;

                    if (bFull == true)
                    {
                        // 反映已经被清除
                        if (bReverse == false)
                        {
                            row_info.TimestampString = "";

                            row_info.data_length = 0;
                            row_info.data_textptr = null;
                        }
                        else
                        {
                            row_info.NewTimestampString = "";

                            row_info.newdata_length = 0;
                            row_info.newdata_textptr = null;
                        }
                    }
                }
            }

            // 注：如果是最后一次写入，函数返回时，newdata字段内容被清除
            return 0;
        }

#if NO
        void FileDelete(string filename)
        {
            _streamCache.ClearItems(filename);
            File.Delete(filename);
        }
#endif

        // return:
        //      1   成功
        //      -100    文件不存在
        int ReadObjectFile(string strObjectFilename,
            long lStart,
            long lOutputLength,
            out byte[] destBuffer,
            out string strError)
        {
            strError = "";
            Debug.Assert(string.IsNullOrEmpty(strObjectFilename) == false, "");

            destBuffer = new Byte[lOutputLength];

            try
            {
                // 注：纳入 StreamCache 管理
                StreamItem s = this._streamCache.GetStream(
        strObjectFilename,
        FileMode.Open,
        FileAccess.Read,
        true    // lStart > CACHE_SIZE
        );
                try
                {
                    s.FileStream.FastSeek(lStart);
                    s.FileStream.Read(destBuffer,
                        0,
                        (int)lOutputLength);

                    return 1;
                }
                finally
                {
                    _streamCache.ReturnStream(s);
                }
            }
            catch (FileNotFoundException /* ex */)
            {
                // TODO: 不要直接汇报物理文件名
                strError = "对象文件 '" + strObjectFilename + "' 不存在";
                return -100;
            }
        }

        // 创建一个0bytes的文件
        int CreateZeroLengthFile(string strFileName,
            out string strError)
        {
            strError = "";

            int nRedoCount = 0;
            REDO:
            try
            {
                StreamItem item = _streamCache.GetWriteStream(strFileName, true);
                try
                {
                    item.FileStream.SetLength(0);
                    return 0;
                }
                finally
                {
                    _streamCache.ReturnStream(item);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                if (nRedoCount == 0)
                {
                    // 创建中间子目录
                    PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                    nRedoCount++;
                    goto REDO;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                strError = "创建0字节的文件 '" + strFileName + "' 时出错：" + ex.Message;
                return -1;
            }
        }
#if OLD
        // 创建一个0bytes的文件
        int CreateZeroLengthFile(string strFileName,
            out string strError)
        {
            strError = "";

            int nRedoCount = 0;
            REDO:
            try
            {
                _streamCache.ClearItems(strFileName);
                using (FileStream s = File.Open(
        strFileName,
        FileMode.OpenOrCreate,
        FileAccess.Write,
        FileShare.ReadWrite))
                {
                    s.SetLength(0);
                    return 0;
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                if (nRedoCount == 0)
                {
                    // 创建中间子目录
                    PathUtil.TryCreateDir(PathUtil.PathPart(strFileName));
                    nRedoCount++;
                    goto REDO;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                strError = "创建0字节的文件 '" + strFileName + "' 时出错：" + ex.Message;
                return -1;
            }
        }

#endif

        // TODO: metadata 字符数较多，是否可以允许没有必要的时候不写入这个字段内容?
        // parameters:
        //      strPartList 要写入哪些部分？ full 表示全部。range filename newfilename metadata timestamp
        //      bFull   是否为最后一次充满的写入。因为 metadata 字段是临时和正式共用的，所以在充满的这一次才修改写入
        int WriteLine(
            Connection connection,
            ref RecordRowInfo row_info,
            string strID,
            string strOutputTimestamp,
            string strCurrentRange,
            string strResultMetadata,
            bool bObjectFile,
            bool bFull,
            bool bReverse,
            bool bNeedInsertRow,
            string strPartList,
            out string strError)
        {
            strError = "";

            string strCommand = "";
            if (bObjectFile == false)
            {
                string strSetNull = ""; // 设置即将删除的 timestamp 字段内容为空的语句
                if (bFull == true)
                {
                    strSetNull = (bReverse == true ? " newdptimestamp=NULL, newdata=NULL," : " dptimestamp=NULL, data=NULL,");
                    // 时间戳和data内容都清除
                }

                strCommand = "use " + this.m_strSqlDbName + "\n"
                    + " UPDATE records "
                    + (bReverse == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                    + strSetNull
                    + " range=@range,"
                    + " filename=NULL, newfilename=NULL,"
                    + " metadata=@metadata "
                    + " WHERE id=@id";
            }
            else
            {
                string strSetNull = ""; // 设置即将删除的 timestamp 字段内容为空的语句
                if (bFull == true)
                    strSetNull = " newdptimestamp=NULL,";

                if (connection.SqlServerType == SqlServerType.MsSqlServer)
                {
                    strCommand = "use " + this.m_strSqlDbName + "\n"
                         + " UPDATE records "
                         + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                         + strSetNull
                         + " range=@range,"
                         + " metadata=@metadata,"
                         + (bFull == true ? " filename=@filename, newfilename=NULL," : " newfilename=@filename,")
                         + " data=NULL, newdata=NULL "
                         + " WHERE id=@id";
                    strCommand += " use master " + "\n";
                }
                else if (connection.SqlServerType == SqlServerType.SQLite)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE records "
                             + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                             + strSetNull
                             + " range=@range,"
                             + " metadata=@metadata,"
                             + (bFull == true ? " filename=@filename, newfilename=NULL " : " newfilename=@filename ")
                             + " WHERE id=@id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO records(id, range, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES(@id, @range, @metadata, @dptimestamp, NULL, @filename, NULL)"
                                             : " VALUES(@id, @range, @metadata, NULL, @dptimestamp, NULL, @filename)");

                    }
                }
                else if (connection.SqlServerType == SqlServerType.MySql)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE `" + this.m_strSqlDbName + "`.records "
                             + (bFull == true ? " SET dptimestamp=@dptimestamp," : " SET newdptimestamp=@dptimestamp,")
                             + strSetNull
                             + " `range`=@range,"
                             + " metadata=@metadata,"
                             + (bFull == true ? " filename=@filename, newfilename=NULL " : " newfilename=@filename ")
                             + " WHERE id=@id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO `" + this.m_strSqlDbName + "`.records (id, `range`, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES (@id, @range, @metadata, @dptimestamp, NULL, @filename, NULL)"
                                             : " VALUES (@id, @range, @metadata, NULL, @dptimestamp, NULL, @filename)");

                    }
                }
                else if (connection.SqlServerType == SqlServerType.Oracle)
                {
                    if (bNeedInsertRow == false)
                    {
                        strCommand = " UPDATE " + this.m_strSqlDbName + "_records "
                             + (bFull == true ? " SET dptimestamp=:dptimestamp," : " SET newdptimestamp=:dptimestamp,")
                             + strSetNull
                             + " range=:range,"
                             + " metadata=:metadata,"
                             + (bFull == true ? " filename=:filename, newfilename=NULL " : " newfilename=:filename ")
                             + " WHERE id=:id";
                    }
                    else
                    {
                        strCommand = " INSERT INTO " + this.m_strSqlDbName + "_records (id, range, metadata, dptimestamp, newdptimestamp, filename, newfilename) "
                            + (bFull == true ? " VALUES (:id, :range, :metadata, :dptimestamp, NULL, :filename, NULL)"
                                             : " VALUES (:id, :range, :metadata, NULL, :dptimestamp, NULL, :filename)");

                    }
                }
            }

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    SqlParameter idParam = command.Parameters.Add("@id",
        SqlDbType.NVarChar);
                    idParam.Value = strID;

                    SqlParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        SqlDbType.NVarChar,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    SqlParameter rangeParam =
                        command.Parameters.Add("@range",
                        SqlDbType.NVarChar,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        if (bFull == true)
                            rangeParam.Value = (bReverse == false ? "!" : "") + strCurrentRange;   // 翻转
                        else
                            rangeParam.Value = (bReverse == true ? "!" : "") + strCurrentRange;   // 不翻转
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    SqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        SqlDbType.NVarChar,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        SqlParameter filenameParam =
                command.Parameters.Add("@filename",
                SqlDbType.NVarChar,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    int nCount = command.ExecuteNonQuery();
                    if (nCount == 0)
                    {
                        strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {

                    SQLiteParameter idParam = command.Parameters.Add("@id",
                        DbType.String);
                    idParam.Value = strID;

                    SQLiteParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        DbType.String,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    SQLiteParameter rangeParam =
                        command.Parameters.Add("@range",
                        DbType.String,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                        /*
                        if (bFull == true)
                            rangeParam.Value = (bReverse == false ? "!" : "") + strCurrentRange;   // 翻转
                        else
                            rangeParam.Value = (bReverse == true ? "!" : "") + strCurrentRange;   // 不翻转
                         * */
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    SQLiteParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        DbType.String,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        SQLiteParameter filenameParam =
                command.Parameters.Add("@filename",
                DbType.String,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {
                    MySqlParameter idParam = command.Parameters.Add("@id",
                        MySqlDbType.String);
                    idParam.Value = strID;

                    MySqlParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        MySqlDbType.String,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    MySqlParameter rangeParam =
                        command.Parameters.Add("@range",
                        MySqlDbType.String,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    MySqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        MySqlDbType.String,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        MySqlParameter filenameParam =
                command.Parameters.Add("@filename",
                MySqlDbType.String,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command

            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                // 注： Oracle 这里和 MySql 基本一样
                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {

                    command.BindByName = true;

                    OracleParameter idParam = command.Parameters.Add(":id",
                        OracleDbType.NVarchar2);
                    idParam.Value = strID;

                    OracleParameter dptimestampParam =
                        command.Parameters.Add(":dptimestamp",
                        OracleDbType.NVarchar2,
                        100);
                    dptimestampParam.Value = strOutputTimestamp;

                    OracleParameter rangeParam =
                        command.Parameters.Add(":range",
                        OracleDbType.NVarchar2,
                        4000);
                    if (bObjectFile == true)
                        rangeParam.Value = "#" + strCurrentRange;
                    else
                    {
                        Debug.Assert(false, "不可能走到这里");
                    }

                    row_info.Range = (string)rangeParam.Value;  // 将反转情况及时兑现

                    OracleParameter metadataParam =
                        command.Parameters.Add(":metadata",
                        OracleDbType.NVarchar2,
                        4000);
                    if (bFull == true)
                        metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata
                    else
                        metadataParam.Value = row_info.Metadata;

                    if (bObjectFile == true)
                    {
                        OracleParameter filenameParam =
                command.Parameters.Add(":filename",
                OracleDbType.NVarchar2,
                255);
                        if (bFull == true)
                            filenameParam.Value = row_info.FileName;
                        else
                            filenameParam.Value = row_info.NewFileName;
                    }

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command

            }
            else
            {
                strError = "未能识别的 SqlServerType '" + connection.SqlServerType.ToString() + "'";
                return -1;
            }

            return 0;
        }

        // 将 metadata 写入 records 表
        // parameters:
        int WriteMetadataColumn(
            Connection connection,
            // ref RecordRowInfo row_info,
            string strID,
            string strResultMetadata,
            out string strError)
        {
            strError = "";

            string strCommand = "";

            {

                if (connection.SqlServerType == SqlServerType.MsSqlServer)
                {
                    strCommand = "use " + this.m_strSqlDbName + "\n"
                         + " UPDATE records SET "
                         + " metadata=@metadata "
                         + " WHERE id=@id";
                    strCommand += " use master " + "\n";
                }
                else if (connection.SqlServerType == SqlServerType.SQLite)
                {
                    strCommand = " UPDATE records SET "
                         + " metadata=@metadata "
                         + " WHERE id=@id";
                }
                else if (connection.SqlServerType == SqlServerType.MySql)
                {
                    strCommand = " UPDATE `" + this.m_strSqlDbName + "`.records SET "
                         + " metadata=@metadata "
                         + " WHERE id=@id";
                }
                else if (connection.SqlServerType == SqlServerType.Oracle)
                {
                    strCommand = " UPDATE " + this.m_strSqlDbName + "_records SET "
                         + " metadata=:metadata "
                         + " WHERE id=:id";
                }
            }

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    SqlParameter idParam = command.Parameters.Add("@id",
        SqlDbType.NVarChar);
                    idParam.Value = strID;

                    SqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        SqlDbType.NVarChar,
                        4000);
                    metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata

                    int nCount = command.ExecuteNonQuery();
                    if (nCount == 0)
                    {
                        strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {

                    SQLiteParameter idParam = command.Parameters.Add("@id",
                        DbType.String);
                    idParam.Value = strID;

                    SQLiteParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        DbType.String,
                        4000);
                    metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {
                    MySqlParameter idParam = command.Parameters.Add("@id",
                        MySqlDbType.String);
                    idParam.Value = strID;

                    MySqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        MySqlDbType.String,
                        4000);
                    metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command

            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                // 注： Oracle 这里和 MySql 基本一样
                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {
                    command.BindByName = true;

                    OracleParameter idParam = command.Parameters.Add(":id",
                        OracleDbType.NVarchar2);
                    idParam.Value = strID;

                    OracleParameter metadataParam =
                        command.Parameters.Add(":metadata",
                        OracleDbType.NVarchar2,
                        4000);
                    metadataParam.Value = strResultMetadata;    // 只有当最后一次写入的时候才更新 metadata

                    try
                    {
                        int nCount = command.ExecuteNonQuery();
                        // ????
                        if (nCount == 0)
                        {
                            strError = "更新记录号为 '" + strID + "' 的行的 时间戳,range,metadata,(new)filename 失败";
                            return -1;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }
            else
            {
                strError = "未能识别的 SqlServerType '" + connection.SqlServerType.ToString() + "'";
                return -1;
            }

            return 0;
        }

        // return:
        //      -1  对象文件
        //      0   正向image字段
        //      1   反向image字段
        static int GetReverse(string strCurrentRange)
        {
            if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '#')
                return -1;
            if (String.IsNullOrEmpty(strCurrentRange) == false
                && strCurrentRange[0] == '!')
                return 1;
            return 0;
        }

        // 写image字段的内容
        // 外面指供一个textprt指针
        // parameter:
        //		connection  连接对象
        //		textPtr     image指针
        //		nOldLength  原长度
        //		nDeleteDuoYu    是否删除多余
        //		strID           记录id
        //		strImageFieldName   image字段
        //		nStartOfTarget      目标的起始位置
        //		sourceBuffer    源大字节数组
        //		streamSource    源大流
        //		nStartOfBuffer  源流的起始位置
        //		nNeedReadLength 需要写的长度
        //		strError        out参数，返回出错信息
        // return:	
        //		-1  出错
        //		0   成功
        private int WriteImage(Connection connection,
            ref byte[] textPtr,
            ref long lCurrentLength,           // 原来的长度     
            bool bDeleteDuoYu,
            string strID,
            string strImageFieldName,
            long lStartOfTarget,       // 目标的起始位置
            byte[] baSource,
            // Stream streamSource,
            int nStartOfSource,     // 缓冲区的实际位置 必须 >=0 
            int nNeedReadLength,    // 需要读缓冲区的长度可能是-1,表示从源流nSourceStart位置到末尾
            long lTotalLength,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            //---------------------------------------
            //例行检查输入参数
            //-----------------------------------------
            /*
            if (baSource == null && streamSource == null)
            {
                strError = "WriteImage()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteImage()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
             * */
            if (baSource == null)
            {
                strError = "WriteImage()调用错误，baSource参数不能为null。";
                return -1;
            }

            if (connection.SqlServerType != SqlServerType.MsSqlServer)
            {
                strError = "SqlServerType '" + connection.SqlServerType.ToString() + "' 的connection不能用于调用WriteImage()函数";
                return -1;
            }


            int nSourceTotalLength = baSource.Length;
            /*
            if (baSource != null)
                nSourceTotalLength = baSource.Length;
            else
                nSourceTotalLength = (int)streamSource.Length;
             * */

            long lOutputLength = 0;
            // return:
            //		-1  出错
            //		0   成功
            nRet = ConvertUtil.GetRealLength(nStartOfSource,
                nNeedReadLength,
                nSourceTotalLength,
                -1,//nMaxLength
                out lOutputLength,
                out strError);
            if (nRet == -1)
                return -1;


            //---------------------------------------
            //开始做事情
            //-----------------------------------------
            if (textPtr == null
                || lStartOfTarget == 0 && lCurrentLength > lTotalLength)
            {
                string strCommand = "use " + this.m_strSqlDbName + " "
        + " UPDATE records "
        + " set " + strImageFieldName + "=0x0 "
        + " where id='" + strID + "'\n"
        + " SELECT TEXTPTR(" + strImageFieldName + ") from records"
        + " where id='" + strID + "'\n";

                strCommand += " use master " + "\n";

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {
                    try
                    {
                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.Default))  // SingleResult
                        {
                            // 1.记录不存在报错
                            if (dr == null
                                || dr.HasRows == false)
                            {
                                dr.Read();
                                dr.NextResult();    // 这一句可以触发下一个结果集的异常 2015/9/4
                                strError = "记录 '" + strID + "' 在库中不存在";
                                return -1;
                            }

                            dr.Read();

                            textPtr = (byte[])dr[0];

                            bool bRet = dr.Read();

                            if (bRet == true)
                            {
                                // 还有一行
                                strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                                return -1;
                            }

                            lCurrentLength = 1; // 表示写成功了一个 0 字符
                        }
                    }
                    catch (SqlException ex)
                    {
                        strError = "更新数据行时出错，记录路径'" + this.GetCaption("zh-CN") + "/" + strID + "，原因：" + ex.Message;

                        // 检查 SQL 错误码
                        if (ContainsErrorCode(ex, 1105))
                        {
                            // 磁盘空间不够的问题。要记入错误日志，以引起管理员注意
                            this.container.KernelApplication.WriteErrorLog("*** 数据库空间不足错误: " + strError);
                        }
                        return -1;
                    }
                }
            }

            Debug.Assert(textPtr != null, "");

            {
                int chucksize = 32 * 1024;  //写库时每块为32K

                // 执行更新操作,使用UPDATETEXT语句

                // UPDATETEXT命令说明:
                // dest_text_ptr: 指向要更新的image 数据的文本指针的值（由 TEXTPTR 函数返回）必须为 binary(16)
                // insert_offset: 以零为基的更新起始位置,
                //				  对于image 列，insert_offset 是在插入新数据前从现有列的起点开始要跳过的字节数
                //				  开始于这个以零为基的起始点的现有 image 数据向右移，为新数据腾出空间。
                //				  值为 0 表示将新数据插入到现有位置的开始处。值为 NULL 则将新数据追加到现有数据值中。
                // delete_length: 是从 insert_offset 位置开始的、要从现有 image 列中删除的数据长度。
                //				  delete_length 值对于 text 和 image 列用字节指定，对于 ntext 列用字符指定。每个 ntext 字符占用 2 个字节。
                //				  值为 0 表示不删除数据。值为 NULL 则删除现有 text 或 image 列中从 insert_offset 位置开始到末尾的所有数据。
                // WITH LOG:      在 Microsoft? SQL Server? 2000 中被忽略。在该版本中，日志记录由数据库的有效恢复模型决定。
                // inserted_data: 是要插入到现有 text、ntext 或 image 列 insert_offset 位置的数据。
                //				  这是单个 char、nchar、varchar、nvarchar、binary、varbinary、text、ntext 或 image 值。
                //				  inserted_data 可以是文字或变量。
                // 如何使用UPDATETEXT命令?
                // 替换现有数据:  指定一个非空 insert_offset 值、非零 delete_length 值和要插入的新数据。
                // 删除现有数据:  指定一个非空 insert_offset 值、非零 delete_length 值。不指定要插入的新数据。
                // 插入新数据:    指定 insert_offset 值、为零的 delete_length 值和要插入的新数据。
                string strCommand = "use " + this.m_strSqlDbName + " "
                    + " UPDATETEXT records." + strImageFieldName
                    + " @dest_text_ptr"
                    + " @insert_offset"
                    + " @delete_length"
#if UPDATETEXT_WITHLOG
                    + " WITH LOG"
#endif
 + " @inserted_data";   //不能加where语句

                strCommand += " use master " + "\n";

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    // 给参数赋值
                    SqlParameter dest_text_ptrParam =
                        command.Parameters.Add("@dest_text_ptr",
                        SqlDbType.Binary,
                        16);

                    SqlParameter insert_offsetParam =
                        command.Parameters.Add("@insert_offset",
                        SqlDbType.Int);  // old Int

                    SqlParameter delete_lengthParam =
                        command.Parameters.Add("@delete_length",
                        SqlDbType.Int);  // old Int

                    SqlParameter inserted_dataParam =
                        command.Parameters.Add("@inserted_data",
                        SqlDbType.Binary,
                        0);

                    long insert_offset = lStartOfTarget; // 插入image字段的位置
                    int nReadStartOfBuffer = nStartOfSource;         // 从源缓冲区中的读的起始位置
                    Byte[] chuckBuffer = null; // 块缓冲区
                    int nCount = 0;             // 影响的记录条数

                    dest_text_ptrParam.Value = textPtr;

                    while (true)
                    {
                        // 已从缓冲区读出的长度
                        int nReadedLength = nReadStartOfBuffer - nStartOfSource;
                        if (nReadedLength >= nNeedReadLength)
                            break;

                        // 还需要读的长度
                        int nContinueLength = nNeedReadLength - nReadedLength;
                        if (nContinueLength > chucksize)  // 从源流中读的长度
                            nContinueLength = chucksize;

                        inserted_dataParam.Size = nContinueLength;
                        chuckBuffer = new byte[nContinueLength];

                        /*
                        if (baSource != null)
                         * */
                        {
                            // 拷到源数组的一段到每次用于写的chuckbuffer
                            Array.Copy(baSource,
                                nReadStartOfBuffer,
                                chuckBuffer,
                                0,
                                nContinueLength);
                        }
                        /*
                        else
                        {
                            streamSource.Read(chuckBuffer,
                                0,
                                nContinueLength);
                        }
                         * */

                        if (chuckBuffer.Length <= 0)
                            break;

                        insert_offsetParam.Value = insert_offset;

#if NO
                    // 删除字段的长度
                    long lDeleteLength = 0;
                    if (bDeleteDuoYu == true)  //最后一次
                    {
                        lDeleteLength = lCurrentLength - insert_offset;  // 当前长度表示image的长度
                        if (lDeleteLength < 0)
                            lDeleteLength = 0;
                    }
                    else
                    {
                        // 写入的长度超过当前最大长度时,要删除的长度为当前长度-start
                        if (insert_offset + chuckBuffer.Length > lCurrentLength)
                        {
                            lDeleteLength = lCurrentLength - insert_offset;
                            if (lDeleteLength < 0)
                                lDeleteLength = lCurrentLength;
                        }
                        else
                        {
                            lDeleteLength = chuckBuffer.Length;
                        }
                    }
#endif

                        // null表示从插入点到末尾的原来的内容全部删除 2013/2/15
                        delete_lengthParam.Value = DBNull.Value;   // lDeleteLength;
                        inserted_dataParam.Value = chuckBuffer;

                        nCount = command.ExecuteNonQuery();
                        if (nCount == 0)
                        {
                            strError = "没有更新到记录块";
                            return -1;
                        }

                        // 写入后,当前长度发生的变化
                        // lCurrentLength = lCurrentLength + chuckBuffer.Length - lDeleteLength;
                        lCurrentLength = insert_offset + chuckBuffer.Length;    // 2012/2/15

                        // 缓冲区的位置变化
                        nReadStartOfBuffer += chuckBuffer.Length;

                        // 目标的位置变化
                        insert_offset += chuckBuffer.Length;   //恢复时要恢复到原来的位置

                        if (chuckBuffer.Length < chucksize)
                            break;
                    }
                }
            }

            return 0;
        }


        // 从初始化库的代码中可以得到启发，穷举出全部keys表名

        // 修改检索点keys
        // return:
        //      -1  出错
        //      0   成功
        public int ModifyKeys(Connection connection,
            KeyCollection keysAdd,
            KeyCollection keysDelete,
            bool bFastMode,
            out string strError)
        {
            strError = "";
            StringBuilder strCommand = new StringBuilder(4096);

            int nCount1 = 0;
            int nCount2 = 0;

            if (keysAdd != null)
                nCount1 = keysAdd.Count;
            if (keysDelete != null)
                nCount2 = keysDelete.Count;

            if (nCount1 == 0 && nCount2 == 0)
                return 0;

            string strRecordID = "";
            if (keysAdd != null && keysAdd.Count > 0)
                strRecordID = ((KeyItem)keysAdd[0]).RecordID;
            else if (keysDelete != null && keysDelete.Count > 0)
                strRecordID = ((KeyItem)keysDelete[0]).RecordID;

            #region MS SQL Server
            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                using (SqlCommand command = new SqlCommand("",
                    connection.SqlConnection))
                {
                    SqlTransaction trans = null;
                    // trans = connection.SqlConnection.BeginTransaction();
                    // command.Transaction = trans;

                    int nExecuted = 0;   // 已经发出执行的命令行数 2008/10/21 
                    try
                    {
                        int i = 0;
                        int nNameIndex = 0;

                        int nCount = 0; // 累积的尚未发出的命令行数 2008/10/21 

                        int nMaxLinesPerExecute = (2100 / 5) - 1;   // 4个参数，加上一个sql命令字符串 2008/10/23 

                        // 2006/12/8 把删除提前到增加以前
                        if (keysDelete != null)
                        {
                            // 删除keys
                            for (i = 0; i < keysDelete.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysDelete[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = "@key" + strIndex;
                                string strFromParamName = "@from" + strIndex;
                                string strIdParamName = "@id" + strIndex;
                                string strKeynumParamName = "@keynum" + strIndex;

                                strCommand.Append(" DELETE FROM " + strKeysTableName
                                    + " WHERE keystring = " + strKeyParamName
                                    + " AND fromstring = " + strFromParamName
                                    + " AND idstring = " + strIdParamName
                                    + " AND keystringnum = " + strKeynumParamName);

                                SqlParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    SqlDbType.NVarChar);
                                keyParam.Value = oneKey.Key;

                                SqlParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    SqlDbType.NVarChar);
                                fromParam.Value = oneKey.FromValue;

                                SqlParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    SqlDbType.NVarChar);
                                idParam.Value = oneKey.RecordID;

                                SqlParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    SqlDbType.NVarChar);
                                keynumParam.Value = oneKey.Num;

                                if (nCount >= nMaxLinesPerExecute)
                                {
                                    command.CommandText = "use " + this.m_strSqlDbName + " \n"
                                        + strCommand
                                        + " use master " + "\n";
                                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2013/2/19

                                    command.ExecuteNonQuery();

                                    strCommand.Clear();
                                    nExecuted += nCount;
                                    nCount = 0;
                                    command.Parameters.Clear();
                                }
                                else
                                {
                                    nCount++;
                                }
                            }
                        }

                        if (keysAdd != null)
                        {
                            // nCount = keysAdd.Count;

                            // 增加keys
                            for (i = 0; i < keysAdd.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysAdd[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                // string strIndex = Convert.ToString(i);
                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = "@key" + strIndex;
                                string strFromParamName = "@from" + strIndex;
                                string strIdParamName = "@id" + strIndex;
                                string strKeynumParamName = "@keynum" + strIndex;

                                //加keynum
                                strCommand.Append(" INSERT INTO " + strKeysTableName
                                    + " (keystring,fromstring,idstring,keystringnum) "
                                    + " VALUES (" + strKeyParamName + ","
                                    + strFromParamName + ","
                                    + strIdParamName + ","
                                    + strKeynumParamName + ")");

                                SqlParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    SqlDbType.NVarChar);
                                keyParam.Value = oneKey.Key;

                                SqlParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    SqlDbType.NVarChar);
                                fromParam.Value = oneKey.FromValue;

                                SqlParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    SqlDbType.NVarChar);
                                idParam.Value = oneKey.RecordID;

                                SqlParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    SqlDbType.NVarChar);
                                keynumParam.Value = oneKey.Num;

                                if (nCount >= nMaxLinesPerExecute)
                                {
                                    command.CommandText = "use " + this.m_strSqlDbName + " \n"
                                        + strCommand
                                        + " use master " + "\n";
                                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2013/2/19

                                    command.ExecuteNonQuery();

                                    strCommand.Clear();
                                    nExecuted += nCount;
                                    nCount = 0;
                                    command.Parameters.Clear();
                                }
                                else
                                {
                                    nCount++;
                                }
                            }
                        }

                        // 最后可能剩下的命令
                        if (strCommand.Length > 0)
                        {
                            command.CommandText = "use " + this.m_strSqlDbName + " \n"
                                + strCommand
                                + " use master " + "\n";
                            command.CommandTimeout = 20 * 60;  // 把超时时间放大 2013/2/19

                            command.ExecuteNonQuery();

                            strCommand.Clear();
                            nExecuted += nCount;
                            nCount = 0;
                            command.Parameters.Clear();
                        }
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    catch (SqlException ex)
                    {
                        strError = "创建检索点出错,偏移 " + (nExecuted).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;

                        // 检查 SQL 错误码
                        if (ContainsErrorCode(ex, 1105))
                        {
                            // 磁盘空间不够的问题。要记入错误日志，以引起管理员注意
                            this.container.KernelApplication.WriteErrorLog("*** 数据库空间不足错误: " + strError);
                        }

                        return -1;
                    }
                    catch (Exception ex)
                    {
                        // TODO: 如果出现超时错，可能其实在 SQL Server 一端已经正确执行，可以不顾这个错误继续执行下去
                        // 如果非要重试处理这种情况，则可能需要把语句拆开成为一个一个单独的动作语句，然后重新插入和删除，插入的时候遇到重复，就当作正常情况处理，删除的时候遇到行不存在，也当作正常处理
                        strError = "创建检索点出错,偏移 " + (nExecuted).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                        return -1;
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command

                return 0;
            }
            #endregion // MS SQL Server

            #region SQLite
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                using (SQLiteCommand command = new SQLiteCommand("",
                    connection.SQLiteConnection))
                {

                    IDbTransaction trans = null;

                    if (bFastMode == false)
                        trans = connection.SQLiteConnection.BeginTransaction();
                    try
                    {

                        int i = 0;
                        int nNameIndex = 0;
                        int nCount = 0; // 累积的尚未发出的命令行数

                        // 把删除提前到增加以前
                        if (keysDelete != null)
                        {
                            // 删除keys
                            for (i = 0; i < keysDelete.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysDelete[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = "@key" + strIndex;
                                string strFromParamName = "@from" + strIndex;
                                string strIdParamName = "@id" + strIndex;
                                string strKeynumParamName = "@keynum" + strIndex;

                                strCommand.Append(" DELETE FROM " + strKeysTableName
                                    + " WHERE keystring = " + strKeyParamName
                                    + " AND fromstring = " + strFromParamName
                                    + " AND idstring = " + strIdParamName
                                    + " AND keystringnum = " + strKeynumParamName
                                    + " ; ");

                                SQLiteParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    DbType.String);
                                keyParam.Value = oneKey.Key;

                                SQLiteParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    DbType.String);
                                fromParam.Value = oneKey.FromValue;

                                SQLiteParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    DbType.String);
                                idParam.Value = oneKey.RecordID;

                                SQLiteParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    DbType.String);
                                keynumParam.Value = oneKey.Num;

                                command.CommandText = strCommand.ToString();
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "创建检索点出错, 偏移 " + (nCount).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                                    return -1;
                                }
                                strCommand.Clear();

                                command.Parameters.Clear();

                                nCount++;
                            }
                        }

                        if (keysAdd != null)
                        {
                            // nCount = keysAdd.Count;

                            // 增加keys
                            for (i = 0; i < keysAdd.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysAdd[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                // string strIndex = Convert.ToString(i);
                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = "@key" + strIndex;
                                string strFromParamName = "@from" + strIndex;
                                string strIdParamName = "@id" + strIndex;
                                string strKeynumParamName = "@keynum" + strIndex;

                                //加keynum
                                strCommand.Append(" INSERT INTO " + strKeysTableName
                                    + " (keystring,fromstring,idstring,keystringnum) "
                                    + " VALUES (" + strKeyParamName + ","
                                    + strFromParamName + ","
                                    + strIdParamName + ","
                                    + strKeynumParamName + ") ; ");

                                SQLiteParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    DbType.String);
                                keyParam.Value = oneKey.Key;

                                SQLiteParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    DbType.String);
                                fromParam.Value = oneKey.FromValue;

                                SQLiteParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    DbType.String);
                                idParam.Value = oneKey.RecordID;

                                SQLiteParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    DbType.String);
                                keynumParam.Value = oneKey.Num;

                                command.CommandText = strCommand.ToString();
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "创建检索点出错,偏移 " + (nCount).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                                    return -1;
                                }
                                strCommand.Clear();

                                command.Parameters.Clear();

                                nCount++;
                            }
                        }
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command
            }
            #endregion // SQLite

            #region MySql
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                List<string> lines = new List<string>();
                if (keysDelete != null)
                {
                    // 删除keys
                    for (int i = 0; i < keysDelete.Count; i++)
                    {
                        KeyItem oneKey = (KeyItem)keysDelete[i];

                        string strKeysTableName = oneKey.SqlTableName;

                        lines.Add(" DELETE FROM " + strKeysTableName
        + " WHERE keystring = N'" + MySqlHelper.EscapeString(oneKey.Key)
        + "' AND fromstring = N'" + MySqlHelper.EscapeString(oneKey.FromValue)
        + "' AND idstring = N'" + MySqlHelper.EscapeString(oneKey.RecordID)
        + "' AND keystringnum = N'" + MySqlHelper.EscapeString(oneKey.Num) + "' ;");
                    }
                }

                if (keysAdd != null)
                {
                    // 增加keys
                    for (int i = 0; i < keysAdd.Count; i++)
                    {
                        KeyItem oneKey = (KeyItem)keysAdd[i];

                        string strKeysTableName = oneKey.SqlTableName;

                        lines.Add(" INSERT INTO " + strKeysTableName
        + " (keystring,fromstring,idstring,keystringnum) "
        + " VALUES " + new string((char)1, 1) + "(N'" + MySqlHelper.EscapeString(oneKey.Key) + "',N'"
        + MySqlHelper.EscapeString(oneKey.FromValue) + "',N'"
        + MySqlHelper.EscapeString(oneKey.RecordID) + "',N'"
        + MySqlHelper.EscapeString(oneKey.Num) + "') ");
                    }
                }

                using (MySqlCommand command = new MySqlCommand("",
                    connection.MySqlConnection))
                {
                    MySqlTransaction trans = null;

                    // https://mysqlconnector.net/troubleshooting/transaction-usage/
                    trans = connection.MySqlConnection.BeginTransaction();
                    try
                    {
                        // 2021/6/9
                        command.Transaction = trans;

                        string strInsertHead = "";
                        int nExecuted = 0;
                        lines.Add("");
                        foreach (string line in lines)
                        {
                            // 最后可能剩下的命令
                            if (strCommand.Length > 0
                                && (StringUtil.GetUtf8Bytes(strCommand.ToString()) + StringUtil.GetUtf8Bytes(line) + 1 >= 64000
                                || nExecuted >= lines.Count - 1)
                                )
                            {
                                command.CommandText = "use " + this.m_strSqlDbName + " ;\n"
                                    + strCommand
#if !PARAMETERS
 + " ;\n"
#endif
;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "创建检索点出错,偏移 " + (nExecuted).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                                    this.container.KernelApplication.WriteErrorLog(strError + "\r\n\r\nSQL 语句: " + command.CommandText);
                                    return -1;
                                }

                                strCommand.Clear();
                                command.Parameters.Clear();
                                strInsertHead = "";
                            }

                            int nPos = line.IndexOf((char)1);
                            if (nPos != -1)
                            {
                                string strLeft = line.Substring(0, nPos);
                                string strRight = line.Substring(nPos + 1);

                                if (strLeft == strInsertHead)
                                    strCommand.Append("," + strRight);
                                else
                                {
                                    if (strCommand.Length > 0 && strCommand[strCommand.Length - 1] != ';')
                                        strCommand.Append(";");
                                    strCommand.Append(strLeft + strRight);
                                }

                                strInsertHead = strLeft;
                            }
                            else
                                strCommand.Append(line);

                            nExecuted++;
                        }

                        if (strCommand.Length > 0)
                            throw new Exception("循环遗漏了最后一行的处理");

                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command

                return 0;
            }
            #endregion // MySql

            #region Oracle
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                using (OracleCommand command = new OracleCommand("", connection.OracleConnection))
                {
                    command.BindByName = true;

                    IDbTransaction trans = null;

                    trans = connection.OracleConnection.BeginTransaction();
                    try
                    {

                        int i = 0;
                        int nNameIndex = 0;
                        int nCount = 0; // 累积的尚未发出的命令行数

                        // 把删除提前到增加以前
                        if (keysDelete != null)
                        {
                            // 删除keys
                            for (i = 0; i < keysDelete.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysDelete[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = ":key" + strIndex;
                                string strFromParamName = ":from" + strIndex;
                                string strIdParamName = ":id" + strIndex;
                                string strKeynumParamName = ":keynum" + strIndex;

                                strCommand.Append(" DELETE FROM " + this.m_strSqlDbName + "_" + strKeysTableName
                                    + " WHERE keystring = " + strKeyParamName
                                    + " AND fromstring = " + strFromParamName
                                    + " AND idstring = " + strIdParamName
                                    + " AND keystringnum = " + strKeynumParamName
                                    + " ");

                                OracleParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    OracleDbType.NVarchar2);
                                keyParam.Value = oneKey.Key;

                                OracleParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    OracleDbType.NVarchar2);
                                fromParam.Value = oneKey.FromValue;

                                OracleParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    OracleDbType.NVarchar2);
                                idParam.Value = oneKey.RecordID;

                                OracleParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    OracleDbType.NVarchar2);
                                keynumParam.Value = oneKey.Num;

                                command.CommandText = strCommand.ToString();
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "删除检索点出错, 偏移 " + (nCount).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                                    return -1;
                                }
                                strCommand.Clear();

                                // 每行都发出命令，不累积参数值
                                command.Parameters.Clear();
                                nCount++;
                            }
                        }

                        if (keysAdd != null)
                        {
                            // nCount = keysAdd.Count;

                            // 增加keys
                            for (i = 0; i < keysAdd.Count; i++)
                            {
                                KeyItem oneKey = (KeyItem)keysAdd[i];

                                string strKeysTableName = oneKey.SqlTableName;

                                // string strIndex = Convert.ToString(i);
                                string strIndex = Convert.ToString(nNameIndex++);

                                string strKeyParamName = ":key" + strIndex;
                                string strFromParamName = ":from" + strIndex;
                                string strIdParamName = ":id" + strIndex;
                                string strKeynumParamName = ":keynum" + strIndex;

                                //加keynum
                                strCommand.Append(" INSERT INTO " + this.m_strSqlDbName + "_" + strKeysTableName
                                    + " (keystring,fromstring,idstring,keystringnum) "
                                    + " VALUES(" + strKeyParamName + ","
                                    + strFromParamName + ","
                                    + strIdParamName + ","
                                    + strKeynumParamName + ")  ");

                                OracleParameter keyParam =
                                    command.Parameters.Add(strKeyParamName,
                                    OracleDbType.NVarchar2);
                                keyParam.Value = oneKey.Key;

                                OracleParameter fromParam =
                                    command.Parameters.Add(strFromParamName,
                                    OracleDbType.NVarchar2);
                                fromParam.Value = oneKey.FromValue;

                                OracleParameter idParam =
                                    command.Parameters.Add(strIdParamName,
                                    OracleDbType.NVarchar2);
                                idParam.Value = oneKey.RecordID;

                                OracleParameter keynumParam =
                                    command.Parameters.Add(strKeynumParamName,
                                    OracleDbType.NVarchar2);
                                keynumParam.Value = oneKey.Num;

                                command.CommandText = strCommand.ToString();
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "创建检索点出错,偏移 " + (nCount).ToString() + "，记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                                    return -1;
                                }
                                strCommand.Clear();

                                // 每行都发出命令，不累积参数值
                                command.Parameters.Clear();

                                nCount++;
                            }
                        }
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                } // end of using command
            }
            #endregion // Oracle

            return 0;
        }

        // 处理子文件
        // return:
        //      -1  出错
        //      0   成功
        public int ModifyFiles(Connection connection,
            string strID,
            XmlDocument newDom,
            XmlDocument oldDom,
            out string strError)
        {
            strError = "";
            strID = DbPath.GetID10(strID);

            // 新文件
            List<string> new_fileids = new List<string>();
            if (newDom != null)
            {
                XmlNamespaceManager newNsmgr = new XmlNamespaceManager(newDom.NameTable);
                newNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList newFileList = newDom.SelectNodes("//dprms:file", newNsmgr);
                foreach (XmlNode newFileNode in newFileList)
                {
                    string strNewFileID = DomUtil.GetAttr(newFileNode,
                        "id");
                    if (string.IsNullOrEmpty(strNewFileID) == false)
                        new_fileids.Add(strNewFileID);
                }
            }

            // 旧文件
            List<string> old_fileids = new List<string>();
            if (oldDom != null)
            {
                XmlNamespaceManager oldNsmgr = new XmlNamespaceManager(oldDom.NameTable);
                oldNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList oldFileList = oldDom.SelectNodes("//dprms:file", oldNsmgr);
                foreach (XmlNode oldFileNode in oldFileList)
                {
                    string strOldFileID = DomUtil.GetAttr(oldFileNode,
                        "id");
                    if (string.IsNullOrEmpty(strOldFileID) == false)
                        old_fileids.Add(strOldFileID);
                }
            }

            if (new_fileids.Count == 0 && old_fileids.Count == 0)
                return 0;

            //数据必须先排序
            //aNewFileID.Sort(new ComparerClass());
            //aOldFileID.Sort(new ComparerClass());
            new_fileids.Sort();  // TODO: 大小写是否敏感 ?
            old_fileids.Sort();

            List<string> targetLeft = new List<string>();
            List<string> targetMiddle = null;   //  new List<string>();
            List<string> targetRight = new List<string>();

            //新旧两个File数组碰
            StringUtil.MergeStringList(new_fileids,
                old_fileids,
                ref targetLeft,
                ref targetMiddle,
                ref targetRight);

            if (targetLeft.Count == 0 && targetRight.Count == 0)
                return 0;

            List<string> filenames = new List<string>();    // 对象文件名数组 (短文件名)
            List<string> ids = new List<string>();  // 对象 ID 数组 (Length >= 10)

            #region MS SQL Server
            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strCommand = "";
                using (SqlCommand command = new SqlCommand("",
                    connection.SqlConnection))
                {
                    int nCount = 0;

                    // TODO: 注意关注MS SQL Server 参数超过 2100 错误

                    // 删除旧文件
                    if (targetRight.Count > 0)
                    {
                        if (this.m_lObjectStartSize != -1)
                        {
                            // 获得和储存旧文件名
                            string strWhere = "";
                            for (int i = 0; i < targetRight.Count; i++)
                            {
                                string strPureObjectID = targetRight[i];
                                string strObjectID = strID + "_" + strPureObjectID;
                                string strParamIDName = "@id" + Convert.ToString(i);

                                // 准备好参数
                                SqlParameter idParam =
                                    command.Parameters.Add(strParamIDName,
                                    SqlDbType.NVarChar);
                                idParam.Value = strObjectID;

                                if (string.IsNullOrEmpty(strWhere) == false)
                                    strWhere += " OR ";
                                strWhere += " id = " + strParamIDName + " ";
                            }

                            if (string.IsNullOrEmpty(strWhere) == false)
                            {
                                strCommand = " SELECT filename, newfilename, id FROM records WHERE " + strWhere + " \n";
                                strCommand = "use " + this.m_strSqlDbName + " \n"
        + strCommand
        + " use master " + "\n";
                                command.CommandText = strCommand;

                                using (SqlDataReader dr = command.ExecuteReader())
                                {
                                    if (dr.HasRows == true)
                                    {
                                        while (dr.Read())
                                        {
                                            if (dr.IsDBNull(0) == false)
                                                filenames.Add(dr.GetString(0));
                                            if (dr.IsDBNull(1) == false)
                                                filenames.Add(dr.GetString(1));
                                            if (dr.IsDBNull(2) == false)
                                                ids.Add(dr.GetString(2));
                                        }
                                    }
                                }

                                command.Parameters.Clear();
                            }
                        }

                        // 构造删除旧records行的语句
                        strCommand = "";
                        command.Parameters.Clear();
                        nCount = targetRight.Count;

                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i);
                            strCommand += " DELETE FROM records WHERE id = " + strParamIDName + " \n";
                            SqlParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                SqlDbType.NVarChar);
                            idParam.Value = strObjectID;
                        }
                    }

                    // 创建新文件
                    if (targetLeft.Count > 0)
                    {
                        // 构造创建新records行的语句
                        for (int i = 0; i < targetLeft.Count; i++)
                        {
                            string strPureObjectID = targetLeft[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i) + nCount;
                            strCommand += " INSERT INTO records(id) "
                                + " VALUES(" + strParamIDName + ")\n";
                            SqlParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                SqlDbType.NVarChar);
                            idParam.Value = strObjectID;
                        }
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        strCommand = "use " + this.m_strSqlDbName + " \n"
                            + strCommand
                            + " use master " + "\n";

                        command.CommandText = strCommand;
                        command.CommandTimeout = 30 * 60; // 30分钟

                        int nResultCount = 0;
                        try
                        {
                            nResultCount = command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                            return -1;
                        }

                        if (nResultCount != targetRight.Count + targetLeft.Count)
                        {
                            this.container.KernelApplication.WriteErrorLog("希望处理的文件数'" + Convert.ToString(targetRight.Count + targetLeft.Count) + "'个，实际删除的文件数'" + Convert.ToString(nResultCount) + "'个");
                        }
                    }
                } // enf of using command
            }
            #endregion // MS SQL Server

            #region SQLite
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = "";
                using (SQLiteCommand command = new SQLiteCommand("",
                    connection.SQLiteConnection))
                {

                    int nCount = 0;
                    // 删除旧文件
                    if (targetRight.Count > 0)
                    {
                        // 获得和储存旧文件名
                        string strWhere = "";
                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;
                            string strParamIDName = "@id" + Convert.ToString(i);

                            // 准备好参数
                            SQLiteParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                DbType.String);
                            idParam.Value = strObjectID;

                            if (string.IsNullOrEmpty(strWhere) == false)
                                strWhere += " OR ";
                            strWhere += " id = " + strParamIDName + " ";
                        }

                        if (string.IsNullOrEmpty(strWhere) == false)
                        {
                            strCommand = " SELECT filename, newfilename, id FROM records WHERE " + strWhere + " \n";
                            command.CommandText = strCommand;

                            using (SQLiteDataReader dr = command.ExecuteReader())
                            {
                                if (dr.HasRows == true)
                                {
                                    while (dr.Read())
                                    {
                                        if (dr.IsDBNull(0) == false)
                                            filenames.Add(dr.GetString(0));
                                        if (dr.IsDBNull(1) == false)
                                            filenames.Add(dr.GetString(1));
                                        if (dr.IsDBNull(2) == false)
                                            ids.Add(dr.GetString(2));
                                    }
                                }
                            }

                            command.Parameters.Clear();
                        }

                        // 构造删除旧records行的语句
                        strCommand = "";
                        command.Parameters.Clear();
                        nCount = targetRight.Count;

                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i);
                            strCommand += " DELETE FROM records WHERE id = " + strParamIDName + " ;\n";
                            SQLiteParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                DbType.String);
                            idParam.Value = strObjectID;
                        }
                    }

                    // 创建新文件
                    if (targetLeft.Count > 0)
                    {
                        for (int i = 0; i < targetLeft.Count; i++)
                        {
                            string strPureObjectID = targetLeft[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i) + nCount;
                            strCommand += " INSERT INTO records(id) "
                                + " VALUES(" + strParamIDName + ") ;\n";
                            SQLiteParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                DbType.String);
                            idParam.Value = strObjectID;
                        }
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        command.CommandText = strCommand;
                        command.CommandTimeout = 30 * 60; // 30分钟

                        int nResultCount = 0;
                        try
                        {
                            nResultCount = command.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            if (ex.ResultCode == SQLiteErrorCode.Constraint)    // ex.ErrorCode 2015/4/19
                            {
                                // 如果行已经存在，正好，不要报错
                                goto DELETE_OBJECTFILE;
                            }
                            else
                            {
                                strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                                return -1;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                            return -1;
                        }

                        if (nResultCount != targetRight.Count + targetLeft.Count)
                        {
                            this.container.KernelApplication.WriteErrorLog("希望处理的文件数'" + Convert.ToString(targetRight.Count + targetLeft.Count) + "'个，实际删除的文件数'" + Convert.ToString(nResultCount) + "'个");
                        }
                    }
                } // end of using command
            }
            #endregion // SQLite

            #region MySql
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                string strCommand = "";
                using (MySqlCommand command = new MySqlCommand("",
                    connection.MySqlConnection))
                {
                    int nCount = 0;
                    // 删除旧文件
                    if (targetRight.Count > 0)
                    {
                        // 获得和储存旧文件名
                        string strWhere = "";
                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;
                            string strParamIDName = "@id" + Convert.ToString(i);

                            // 准备好参数
                            MySqlParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                MySqlDbType.String);
                            idParam.Value = strObjectID;

                            if (string.IsNullOrEmpty(strWhere) == false)
                                strWhere += " OR ";
                            strWhere += " id = " + strParamIDName + " ";
                        }

                        if (string.IsNullOrEmpty(strWhere) == false)
                        {
                            strCommand = " SELECT filename, newfilename, id FROM records WHERE " + strWhere + " \n";
                            strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
                                + strCommand;
                            command.CommandText = strCommand;

                            using (MySqlDataReader dr = command.ExecuteReader())
                            {
                                if (dr.HasRows == true)
                                {
                                    while (dr.Read())
                                    {
                                        if (dr.IsDBNull(0) == false)
                                            filenames.Add(dr.GetString(0));
                                        if (dr.IsDBNull(1) == false)
                                            filenames.Add(dr.GetString(1));
                                        if (dr.IsDBNull(2) == false)
                                            ids.Add(dr.GetString(2));
                                    }
                                }
                            }

                            command.Parameters.Clear();
                        }

                        // 构造删除旧records行的语句
                        strCommand = "";
                        command.Parameters.Clear();
                        nCount = targetRight.Count;

                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i);
                            strCommand += " DELETE FROM records WHERE id = " + strParamIDName + " ;\n";
                            MySqlParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                MySqlDbType.String);
                            idParam.Value = strObjectID;
                        }
                    }

                    // 创建新文件
                    if (targetLeft.Count > 0)
                    {
                        for (int i = 0; i < targetLeft.Count; i++)
                        {
                            string strPureObjectID = targetLeft[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = "@id" + Convert.ToString(i) + nCount;
                            strCommand += " INSERT INTO records (id) "
                                + " VALUES (" + strParamIDName + ") ;\n";
                            MySqlParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                MySqlDbType.String);
                            idParam.Value = strObjectID;
                        }
                    }

                    if (strCommand != "")
                    {
                        strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
                            + strCommand;

                        command.CommandText = strCommand;
                        command.CommandTimeout = 30 * 60; // 30分钟

                        int nResultCount = 0;
                        try
                        {
                            nResultCount = command.ExecuteNonQuery();
                        }
                        catch (MySqlException ex)
                        {
                            if (ex.Number == 1062)
                            {
                                // 如果行已经存在，正好，不要报错
                                goto DELETE_OBJECTFILE;
                            }
                            else
                            {
                                strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                                return -1;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                            return -1;
                        }

                        if (nResultCount != targetRight.Count + targetLeft.Count)
                        {
                            this.container.KernelApplication.WriteErrorLog("希望处理的文件数'" + Convert.ToString(targetRight.Count + targetLeft.Count) + "'个，实际删除的文件数'" + Convert.ToString(nResultCount) + "'个");
                        }
                    }
                } // end of using command
            }
            #endregion // MySql

            #region Oracle
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                string strCommand = "";
                using (OracleCommand command = new OracleCommand(strCommand, connection.OracleConnection))
                {
                    int nCount = 0;
                    // 删除旧文件
                    if (targetRight.Count > 0)
                    {
                        nCount = targetRight.Count;

                        for (int i = 0; i < targetRight.Count; i++)
                        {
                            string strPureObjectID = targetRight[i];
                            string strObjectID = strID + "_" + strPureObjectID;
                            string strParamIDName = ":id" + Convert.ToString(i);

                            // 准备好参数
                            OracleParameter idParam =
        command.Parameters.Add(strParamIDName,
        OracleDbType.NVarchar2);
                            idParam.Value = strObjectID;

                            // 列出对象文件名
                            strCommand = " SELECT filename, newfilename FROM " + this.m_strSqlDbName + "_records WHERE id = " + strParamIDName + " \n";
                            command.CommandText = strCommand;

                            using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                if (dr != null
                                    && dr.HasRows == true)
                                {
                                    while (dr.Read())
                                    {
                                        if (dr.IsDBNull(0) == false)
                                            filenames.Add(dr.GetString(0));
                                        if (dr.IsDBNull(1) == false)
                                            filenames.Add(dr.GetString(1));
                                    }
                                }
                                else
                                    goto CONTINUE_1;    // 这个id的records行不存在
                            }

                            strCommand = " DELETE FROM " + this.m_strSqlDbName + "_records WHERE id = " + strParamIDName + " \n";
                            command.CommandText = strCommand;

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "处理记录路径为 '" + this.GetCaption("zh") + "/" + strID + "' 的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                                return -1;
                            }
                            CONTINUE_1:
                            command.Parameters.Clear();
                        }
                    }

                    // 创建新文件
                    if (targetLeft.Count > 0)
                    {
                        for (int i = 0; i < targetLeft.Count; i++)
                        {
                            string strPureObjectID = targetLeft[i];
                            string strObjectID = strID + "_" + strPureObjectID;

                            string strParamIDName = ":id" + Convert.ToString(i) + nCount;
                            strCommand = " INSERT INTO " + this.m_strSqlDbName + "_records (id) "
                                + " VALUES (" + strParamIDName + ") \n";

                            command.CommandText = strCommand;
                            command.Parameters.Clear();

                            OracleParameter idParam =
                                command.Parameters.Add(strParamIDName,
                                OracleDbType.NVarchar2);
                            idParam.Value = strObjectID;

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (OracleException ex)
                            {
                                if (ex.Errors.Count > 0 && ex.Errors[0].Number == 00001)
                                {
                                    // 如果行已经存在，正好，不要报错
                                }
                                else
                                {
                                    strError = "处理记录路径为 '" + this.GetCaption("zh") + "/" + strID + "' 的子记录时发生错误:" + ex.Message + ", SQL命令:\r\n" + strCommand;
                                    return -1;
                                }
                            }
                            catch (Exception ex)
                            {

                                strError = "处理记录路径为 '" + this.GetCaption("zh") + "/" + strID + "' 的子记录时发生错误:" + ex.Message + ", SQL命令:\r\n" + strCommand;
                                return -1;
                            }
                            command.Parameters.Clear();
                        }
                    }
                } // end of using command
            }
            #endregion // Oracle

            DELETE_OBJECTFILE:
            // 删除对象文件
            foreach (string strShortFilename in filenames)
            {
                if (string.IsNullOrEmpty(strShortFilename) == true)
                    continue;

                string strFilename = this.GetObjectFileName(strShortFilename);
                try
                {
                    if (string.IsNullOrEmpty(strFilename) == false)
                    {
                        this._streamCache.FileDelete(strFilename);
                    }
                }
                catch (Exception ex)
                {
                    strError = "删除数据库 '" + this.GetCaption("zh-CN") + "' 中 ID为 '" + strID + "' 的对象文件时发生错误: " + ex.Message;
                    this.container.KernelApplication.WriteErrorLog(strError);
                    return -1;
                }
            }

            // 清除 PDF 单页 cache
            foreach (string strObjectID in ids)
            {
                if (strObjectID.Length > 10)
                {
                    // strID 为 10 字符，或者 0000000000_0000 形态
                    string record_path = this.GetCacheRecPath(strObjectID);
                    _pageCache.ClearByRecPath(record_path,
                        (filename) =>
                        {
                            this._streamCache.FileDelete(filename);
                        });
                }
            }

            return 0;
        }


        // 检索连接对象是否正确
        // return:
        //      -1  出错
        //      0   正常
        private int CheckConnection(Connection connection,
            out string strError)
        {
            strError = "";
            if (connection == null)
            {
                strError = "connection为null";
                return -1;
            }
            #region MS SQL Server
            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                if (connection.SqlConnection == null)
                {
                    strError = "connection.SqlConnection为null";
                    return -1;
                }
                if (connection.SqlConnection.State != ConnectionState.Open)
                {
                    strError = "connection没有打开";
                    return -1;
                }
                return 0;
            }
            #endregion // MS SQL Server

            #region SQLite
            if (connection.SqlServerType == SqlServerType.SQLite)
            {
                if (connection.SQLiteConnection == null)
                {
                    strError = "connection.SQLiteConnection为null";
                    return -1;
                }
                if (connection.SQLiteConnection.State != ConnectionState.Open)
                {
                    strError = "connection没有打开";
                    return -1;
                }
                return 0;
            }
            #endregion // SQLite

            #region MySql
            if (connection.SqlServerType == SqlServerType.MySql)
            {
                if (connection.MySqlConnection == null)
                {
                    strError = "connection.MySqlConnection为null";
                    return -1;
                }
                if (connection.MySqlConnection.State != ConnectionState.Open)
                {
                    strError = "connection没有打开";
                    return -1;
                }
                return 0;
            }
            #endregion // MySql

            #region Oracle
            if (connection.SqlServerType == SqlServerType.Oracle)
            {
                if (connection.OracleConnection == null)
                {
                    strError = "connection.OracleConnection为null";
                    return -1;
                }

                if (connection.OracleConnection.State != ConnectionState.Open)
                {
                    strError = "connection没有打开";
                    return -1;
                }
                return 0;
            }
            #endregion // Oracle

            return 0;
        }

        // 得到范围
        private string GetRange(SqlConnection connection,
            string strID)
        {
            string strRange = "";

            string strCommand = "use " + this.m_strSqlDbName + " "
                + "select range from records where id='" + strID + "'";

            strCommand += " use master " + "\n";

            using (SqlCommand command = new SqlCommand(strCommand,
                connection))
            {
                using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (dr != null && dr.HasRows == true)
                    {
                        dr.Read();
                        strRange = dr.GetString(0);
                        if (strRange == null)
                            strRange = "";
                    }
                }
            }

            return strRange;
        }


#if NO
        // 更新对象, 使image字段获得有效的TextPrt指针
        // return
        //		-1  出错
        //		0   成功
        private int UpdateObject(SqlConnection connection,
            string strObjectID,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            SqlCommand command = null;

            string strOutputTimestamp = this.CreateTimestampForDb();

            strCommand = "use " + this.m_strSqlDbName + " "
                + " UPDATE records "
                + " set newdata=0x0,range='0-0',dptimestamp=@dptimestamp,metadata=@metadata "
                + " where id='" + strObjectID + "'";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            string strMetadata = "<file size='0'/>";
            SqlParameter metadataParam =
                command.Parameters.Add("@metadata",
                SqlDbType.NVarChar);
            metadataParam.Value = strMetadata;


            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strOutputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount <= 0)
            {
                strError = "没有更新'" + strObjectID + "'记录";
                return -1;
            }
            // 返回的时间戳
            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            return 0;
        }


        // 判断一个对对象资源否是空对象
        private bool IsEmptyObject(SqlConnection connection,
            string strID)
        {
            return this.IsEmptyObject(connection,
                "newdata",
                strID);
        }

        // 判断一个对对象资源否是空对象
        private bool IsEmptyObject(SqlConnection connection,
            string strImageFieldName,
            string strID)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                throw (new Exception(strError));

            string strCommand = "";
            SqlCommand command = null;
            strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT @Pointer=TEXTPTR(" + strImageFieldName + ") "
                + " FROM records "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);
            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter PointerOutParam =
                command.Parameters.Add("@Pointer",
                SqlDbType.VarBinary,
                100);
            PointerOutParam.Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            if (PointerOutParam == null
                || PointerOutParam.Value is System.DBNull)
            {
                return true;
            }
            return false;
        }


        // 插入一条新记录,使其获得有效的textptr,包装InsertRecord
        private int InsertRecord(SqlConnection connection,
            string strID,
            out byte[] outputTimestamp,
            out string strError)
        {
            return this.InsertRecord(connection,
                strID,
                "newdata",
                new byte[] { 0x0 },
                out outputTimestamp,
                out strError);
        }

        // 给表中插入一条记录
        private int InsertRecord(SqlConnection connection,
            string strID,
            string strImageFieldName,
            byte[] sourceBuffer,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            SqlCommand command = null;

            string strRange = "0-" + Convert.ToString(sourceBuffer.Length - 1);
            string strOutputTimestamp = this.CreateTimestampForDb();

            strCommand = "use " + this.m_strSqlDbName + " "
                + " INSERT INTO records(id," + strImageFieldName + ",range,metadata,dptimestamp) "
                + " VALUES(@id,@data,@range,@metadata,@dptimestamp);";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter dataParam =
                command.Parameters.Add("@data",
                SqlDbType.Binary,
                sourceBuffer.Length);
            dataParam.Value = sourceBuffer;

            SqlParameter rangeParam =
                command.Parameters.Add("@range",
                SqlDbType.NVarChar);
            rangeParam.Value = strRange;

            string strMetadata = "<file size='0'/>";
            SqlParameter metadataParam =
                command.Parameters.Add("@metadata",
                SqlDbType.NVarChar);
            metadataParam.Value = strMetadata;

            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strOutputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount <= 0)
            {
                strError = "InsertImage() SQL命令执行影响的条数为" + Convert.ToString(nCount);
                return -1;
            }

            // 返回的时间戳
            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            return 0;
        }

#endif

#if NO
        // 用newdata字段替换data字段
        // parameters:
        //      connection  SqlConnection对象
        //      strID       记录id
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回影响的记录数
        // 线: 不安全
        private int UpdateDataField(SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";
            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            SqlConnection new_connection = null;
            if (connection.ConnectionTimeout < m_nLongTimeout)
            {
                new_connection = new SqlConnection(this.m_strLongConnString);
                new_connection.Open();
                connection = new_connection;
            }

            try
            {
                string strCommand = "use " + this.m_strSqlDbName + " "
                    + " UPDATE records \n"
                    + " SET data=newdata \n"
                    + " WHERE id='" + strID + "'";
                strCommand += " use master " + "\n";

                SqlCommand command = new SqlCommand(strCommand,
                    connection);
                command.CommandTimeout = m_nLongTimeout;  // 30分钟

                int nCount = command.ExecuteNonQuery();
                if (nCount == -1)
                {
                    strError = "没有替换到该记录'" + strID + "'的data字段";
                    return -1;
                }

                return nCount;
            }
            finally
            {
                if (new_connection != null)
                    new_connection.Close();
            }

        }
#endif

        long GetObjectFileLength(string strID,
            bool bTempObject)
        {
            string strFileName = BuildObjectFileName(strID, bTempObject);

            FileInfo fi = new FileInfo(strFileName);
            // 2020/3/1
            fi.Refresh();
            if (fi.Exists == false)
                return 0;

            return fi.Length;
        }

        // 获得初次创建的对象文件名
        string BuildObjectFileName(string strID,
        bool bTempObject)
        {
            if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                return null;

            Debug.Assert(strID.Length >= 10, "");

            if (bTempObject == true)
                return PathUtil.MergePath(this.m_strObjectDir, strID.Insert(7, "/") + ".temp");
            else
                return PathUtil.MergePath(this.m_strObjectDir, strID.Insert(7, "/"));
        }

        // 根据字段内容构造完整的对象文件名
        // parameters:
        //      strShotFileName filename或newfilename字段中存储的短文件名。即，具体数据目录下的子目录和文件名部分
        string GetObjectFileName(string strShortFileName)
        {
            if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                return null;

            if (string.IsNullOrEmpty(strShortFileName) == true)
                return null;

            return PathUtil.MergePath(this.m_strObjectDir, strShortFileName);
        }

        // 构造出适合保存在filename和newfilename字段中的短文件名
        string GetShortFileName(string strLongFileName)
        {
            if (string.IsNullOrEmpty(this.m_strObjectDir) == true)
                return null;

            // 正规化目录路径名。把所有字符'/'替换为'\'，并且为末尾确保有字符'\'
            string strObjectDir = PathUtil.CanonicalizeDirectoryPath(this.m_strObjectDir);

            if (strLongFileName.Length <= strObjectDir.Length)
                return null;

            return strLongFileName.Substring(strObjectDir.Length);
        }

        /*
        int DeleteObjectFile(string strID,
            bool bTempObject)
        {
            string strFileName = "";

            if (bTempObject == true)
                strFileName = PathUtil.MergePath(this.m_strObjectDir, strID + ".temp");
            else
                strFileName = PathUtil.MergePath(this.m_strObjectDir, strID);

            File.Delete(strFileName);

            return 1;
        }
         * */


#if NO
        // TODO: 也要清除对应的timestamp字段
        // 删除image全部内容
        // parameter:
        //		connection  连接对象
        //		strID       记录ID
        //		strImageFieldName   image字段名
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 不安全
        private int RemoveImage(SqlConnection connection,
            // string strID,
            string strImageFieldName,
            byte [] textptr,
            out string strError)
        {
            strError = "";

            Debug.Assert(textptr != null, "");

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            SqlConnection new_connection = null;
            if (connection.ConnectionTimeout < m_nLongTimeout)
            {
                new_connection = new SqlConnection(this.m_strLongConnString);
                new_connection.Open();
                connection = new_connection;
            }


            try
            {
                string strCommand = "";
                SqlCommand command = null;

                strCommand = "use " + this.m_strSqlDbName + " "
                    + " UPDATETEXT records." + strImageFieldName
                    + " @dest_text_ptr"
                    + " @insert_offset"
                    + " NULL"  //@delete_length"
#if UPDATETEXT_WITHLOG
                    + " WITH LOG";
#endif
        //+ " @inserted_data";   //不能加where语句

                strCommand += " use master " + "\n";

                command = new SqlCommand(strCommand,
                    connection);
                command.CommandTimeout = m_nLongTimeout;  // 30分钟 2011/1/16

                // 给参数赋值
                SqlParameter dest_text_ptrParam =
                    command.Parameters.Add("@dest_text_ptr",
                    SqlDbType.Binary,
                    16);

                SqlParameter insert_offsetParam =
                    command.Parameters.Add("@insert_offset",
                    SqlDbType.Int); // old Int

                dest_text_ptrParam.Value = textptr;
                insert_offsetParam.Value = 0;

                command.ExecuteNonQuery();

                return 0;
            }
            finally
            {
                if (new_connection != null)
                    new_connection.Close();
            }
        }
#endif

#if NO
        // 删除image多余的部分
        // parameter:
        //		connection  连接对象
        //		strID       记录ID
        //		strImageFieldName   image字段名
        //		nStart      起始位置
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 不安全
        private int DeleteDuoYuImage(SqlConnection connection,
            string strID,
            string strImageFieldName,
            long lStart,
            out string strError)
        {
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            SqlConnection new_connection = null;
            if (connection.ConnectionTimeout < m_nLongTimeout)
            {
                new_connection = new SqlConnection(this.m_strLongConnString);
                new_connection.Open();
                connection = new_connection;
            }


            try
            {
                string strCommand = "";
                SqlCommand command = null;

                // 1.得到image指针 和 长度
                strCommand = "use " + this.m_strSqlDbName + " "
                    + " SELECT @Pointer=TEXTPTR(" + strImageFieldName + "),"
                    + " @Length=DataLength(" + strImageFieldName + ") "
                    + " FROM records "
                    + " WHERE id=@id";

                strCommand += " use master " + "\n";

                command = new SqlCommand(strCommand,
                    connection);
                command.CommandTimeout = m_nLongTimeout;  // 30分钟

                SqlParameter idParam =
                    command.Parameters.Add("@id",
                    SqlDbType.NVarChar);
                idParam.Value = strID;

                SqlParameter PointerOutParam =
                    command.Parameters.Add("@Pointer",
                    SqlDbType.VarBinary,
                    100);
                PointerOutParam.Direction = ParameterDirection.Output;

                SqlParameter LengthOutParam =
                    command.Parameters.Add("@Length",
                    SqlDbType.Int);  // old Int
                LengthOutParam.Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();
                if (PointerOutParam == null)
                {
                    strError = "没找到image指针";
                    return -1;
                }

                long lTotalLength = (int)LengthOutParam.Value;
                if (lStart >= lTotalLength)
                    return 0;


                // 2.进行删除
                strCommand = "use " + this.m_strSqlDbName + " "
                    + " UPDATETEXT records." + strImageFieldName
                    + " @dest_text_ptr"
                    + " @insert_offset"
                    + " NULL"  //@delete_length"
#if UPDATETEXT_WITHLOG
                    + " WITH LOG";
#endif
        //+ " @inserted_data";   //不能加where语句

                strCommand += " use master " + "\n";

                command = new SqlCommand(strCommand,
                    connection);
                command.CommandTimeout = m_nLongTimeout;  // 30分钟 2011/1/16

                // 给参数赋值
                SqlParameter dest_text_ptrParam =
                    command.Parameters.Add("@dest_text_ptr",
                    SqlDbType.Binary,
                    16);

                SqlParameter insert_offsetParam =
                    command.Parameters.Add("@insert_offset",
                    SqlDbType.Int); // old Int

                dest_text_ptrParam.Value = PointerOutParam.Value;
                insert_offsetParam.Value = lStart;

                command.ExecuteNonQuery();

                return 0;
            }
            finally
            {
                if (new_connection != null)
                    new_connection.Close();
            }
        }
#endif

#if NO
        // 检查记录在库中是否存在
        // return:
        //		-1  出错
        //      0   不存在
        //      1   存在
        private int RecordIsExist(SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + " "
                + " SET NOCOUNT OFF;"
                + "select id from records where id='" + strID + "'";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr != null && dr.HasRows == true)
                    return 1;
            }
            finally
            {
                dr.Close();
            }
            return 0;
        }
#endif
        public class RecordRowInfo
        {
            public byte[] data_textptr = null;
            public long data_length = 0;

            public byte[] newdata_textptr = null;
            public long newdata_length = 0;

            public string TimestampString = "";
            public string NewTimestampString = "";  // 2012/1/19

            public string Metadata = "";
            public string Range = "";

            public string FileName = "";
            public string NewFileName = "";

            public string ID = "";                  // 2013/2/17
            public byte[] Data = null;              // 2013/2/17
            public byte[] NewData = null;           // 2013/2/17

            // 获得当前起作用的一个 timestamp
            public string GetCurrentTimestamp()
            {
                string strCurrentRange = this.Range;
                if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '!')
                    return this.TimestampString;

                return this.NewTimestampString;
            }

            // 获得完成状态的一个 timestamp
            public string GetCompleteTimestamp()
            {
                string strCurrentRange = this.Range;
                if (String.IsNullOrEmpty(strCurrentRange) == false
        && strCurrentRange[0] == '!')
                {
                    return this.NewTimestampString;
                }

                return this.TimestampString;
            }

            // 先试探获得 current，如果为空则获得 complete
            public string GetTimestamp()
            {
                string strCurrent = GetCurrentTimestamp();
                if (string.IsNullOrEmpty(strCurrent) == false)
                    return strCurrent;
                return GetCompleteTimestamp();
            }

        }

        // 检查记录在库中是否存在，如果存在在则返回一些字段内容，如果不存在则插入一条新记录
        // return:
        //		-1  出错
        //      0   没有创建新记录
        //      1   创建了新的记录
        //      2   需要创建新的记录，但因为优化的缘故(稍后需要创建)而没有创建
        private int CreateNewRecordIfNeed(Connection connection,
            string strID,
            byte[] sourceBuffer,
            out RecordRowInfo row_info,
            out string strError)
        {
            strError = "";
            row_info = null;

            // 2013/2/17
            if (StringUtil.IsPureNumber(strID) == false)
            {
                strError = "ID '" + strID + "' 必须是纯数字";
                return -1;
            }

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strSelect = " SELECT TEXTPTR(data)," // 0
                    + " DataLength(data),"  // 1
                    + " TEXTPTR(newdata),"  // 2
                    + " DataLength(newdata),"   // 3
                    + " range," // 4
                    + " dptimestamp,"   // 5
                    + " metadata, "  // 6
                    + " newdptimestamp,"   // 7
                    + " filename,"   // 8
                    + " newfilename"   // 9
                    + " FROM records "
                    // + " WHERE id='" + strID + "'\n";
                    + " WHERE id=@id \n";


                string strCommand = "use " + this.m_strSqlDbName + "; \n"
                    + "SET NOCOUNT OFF\n"
                    + strSelect
                    + "if @@ROWCOUNT = 0\n"
                    + "begin\n"
                    + " INSERT INTO records(id, data, range, metadata, dptimestamp, newdptimestamp) "
                    + " VALUES(@id, @data, @range, @metadata, @dptimestamp, @newdptimestamp) \n"
                    + " end \n";
                strCommand += " use master " + "\n";

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {
                    if (sourceBuffer == null)
                        sourceBuffer = new byte[] { 0x0 };

                    row_info = new RecordRowInfo();
                    row_info.data_textptr = null;
                    row_info.data_length = sourceBuffer.Length;
                    row_info.newdata_textptr = null;
                    row_info.newdata_length = 0;
                    // row_info.Range = "0-" + Convert.ToString(sourceBuffer.Length - 1);
                    row_info.Range = "";
                    row_info.TimestampString = "";    // this.CreateTimestampForDb();
                    row_info.NewTimestampString = "";
                    row_info.Metadata = "<file size='0'/>";
                    row_info.FileName = "";
                    row_info.NewFileName = "";

                    SqlParameter idParam =
            command.Parameters.Add("@id",
            SqlDbType.NVarChar);
                    idParam.Value = strID;

                    SqlParameter dataParam =
                        command.Parameters.Add("@data",
                        SqlDbType.Binary,
                        sourceBuffer.Length);
                    dataParam.Value = sourceBuffer;

                    SqlParameter rangeParam =
                        command.Parameters.Add("@range",
                        SqlDbType.NVarChar);
                    rangeParam.Value = row_info.Range;

                    SqlParameter metadataParam =
                        command.Parameters.Add("@metadata",
                        SqlDbType.NVarChar);
                    metadataParam.Value = row_info.Metadata;

                    SqlParameter dptimestampParam =
                        command.Parameters.Add("@dptimestamp",
                        SqlDbType.NVarChar,
                        100);
                    dptimestampParam.Value = row_info.TimestampString;

                    SqlParameter newdptimestampParam =
            command.Parameters.Add("@newdptimestamp",
            SqlDbType.NVarChar,
            100);
                    newdptimestampParam.Value = row_info.NewTimestampString;

                    try
                    {
                        using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.Default))
                        {
                            // 1.记录不存在报错
                            if (dr == null
                                || dr.HasRows == false)
                            {
                                //strError = "记录 '" + strID + "' 在库中不存在或者创建失败，有可能是 SQL 库 "+this.m_strSqlDbName+" 空间已满";
                                //return -1;
                                dr.NextResult();    // 这一句可以触发异常

                                return 1;   // 已经创建新记录
                            }

                            dr.Read();

                            row_info = new RecordRowInfo();

                            /*
                            // 2.textPtr为null报错
                            if (dr[0] is System.DBNull)
                            {
                                strError = "TextPtr不可能为null";
                                return -1;
                            }
                             * */

                            if (dr.IsDBNull(0) == false)
                                row_info.data_textptr = (byte[])dr[0];

                            if (dr.IsDBNull(1) == false)
                                row_info.data_length = dr.GetInt32(1);

                            if (dr.IsDBNull(2) == false)
                                row_info.newdata_textptr = (byte[])dr[2];

                            if (dr.IsDBNull(3) == false)
                                row_info.newdata_length = dr.GetInt32(3);

                            if (dr.IsDBNull(4) == false)
                                row_info.Range = dr.GetString(4);

                            if (dr.IsDBNull(5) == false)
                                row_info.TimestampString = dr.GetString(5);

                            if (dr.IsDBNull(6) == false)
                                row_info.Metadata = dr.GetString(6);

                            if (dr.IsDBNull(7) == false)
                                row_info.NewTimestampString = dr.GetString(7);

                            if (dr.IsDBNull(8) == false)
                                row_info.FileName = dr.GetString(8);

                            if (dr.IsDBNull(9) == false)
                                row_info.NewFileName = dr.GetString(9);

                            bool bRet = dr.Read();

                            if (bRet == true)
                            {
                                // 还有一行
                                strError = "记录 '" + strID + "' 在 SQL 库" + this.m_strSqlDbName + " 的 records 表中存在多条，这是一种不正常的状态, 请系统管理员利用 SQL 命令删除多余的记录。";
                                return -1;
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        strError = "插入数据行时出错，记录路径'" + this.GetCaption("zh-CN") + "/" + strID + "，原因：" + ex.Message;

                        // 检查 SQL 错误码
                        if (ContainsErrorCode(ex, 1105))
                        {
                            // 磁盘空间不够的问题。要记入错误日志，以引起管理员注意
                            this.container.KernelApplication.WriteErrorLog("*** 数据库空间不足错误: " + strError);
                        }
                        return -1;
                    }
                } // end of using command

                return 0;
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = " SELECT "
                    + " range," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata, "  // 2 6
                    + " newdptimestamp,"   // 3 7
                    + " filename,"   // 4 8
                    + " newfilename"   // 5 9
                    + " FROM records "
                    + " WHERE id='" + strID + "'\n";    // TODO: 最好改造为 @id

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {
                    using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在，需要创建
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            row_info = new RecordRowInfo();
                            row_info.Range = "";
                            row_info.TimestampString = "";
                            row_info.NewTimestampString = "";
                            row_info.Metadata = "<file size='0'/>";
                            row_info.FileName = "";
                            row_info.NewFileName = "";
                            return 2;
                            // goto DO_CREATE;
                        }

                        // 如果记录已经存在
                        dr.Read();

                        row_info = new RecordRowInfo();

                        /*
                        if (dr.IsDBNull(0) == false)
                            row_info.data_textptr = (byte[])dr[0];

                        if (dr.IsDBNull(1) == false)
                            row_info.data_length = dr.GetInt32(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.newdata_textptr = (byte[])dr[2];

                        if (dr.IsDBNull(3) == false)
                            row_info.newdata_length = dr.GetInt32(3);
                         * */

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 0;   // 没有创建新记录

#if NO
            DO_CREATE:
                if (sourceBuffer == null)
                    sourceBuffer = new byte[] { 0x0 };

                row_info = new RecordRowInfo();
                /*
                row_info.data_textptr = null;
                row_info.data_length = sourceBuffer.Length;
                row_info.newdata_textptr = null;
                row_info.newdata_length = 0;
                 * */

                row_info.Range = "";
                row_info.TimestampString = "";    // this.CreateTimestampForDb();
                row_info.NewTimestampString = "";
                row_info.Metadata = "<file size='0'/>";
                row_info.FileName = "";
                row_info.NewFileName = "";

                strCommand = " INSERT INTO records(id, range, metadata, dptimestamp, newdptimestamp) "
    + " VALUES(@id, @range, @metadata, @dptimestamp, @newdptimestamp)";
                command = new SQLiteCommand(strCommand,
    connection.SQLiteConnection);

                SQLiteParameter idParam =
        command.Parameters.Add("@id",
        DbType.String);
                idParam.Value = strID;

                /*
                SqlParameter dataParam =
                    command.Parameters.Add("@data",
                    SqlDbType.Binary,
                    sourceBuffer.Length);
                dataParam.Value = sourceBuffer;
                 * */

                SQLiteParameter rangeParam =
                    command.Parameters.Add("@range",
                    DbType.String);
                rangeParam.Value = row_info.Range;

                SQLiteParameter metadataParam =
                    command.Parameters.Add("@metadata",
                    DbType.String);
                metadataParam.Value = row_info.Metadata;

                SQLiteParameter dptimestampParam =
                    command.Parameters.Add("@dptimestamp",
                    DbType.String,
                    100);
                dptimestampParam.Value = row_info.TimestampString;

                SQLiteParameter newdptimestampParam =
                    command.Parameters.Add("@newdptimestamp",
                    DbType.String,
                    100);
                newdptimestampParam.Value = row_info.NewTimestampString;

                command.ExecuteNonQuery();
                return 1;
#endif
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                // 注： MySql 这里和 SQLite 基本一样
                string strCommand = " SELECT "
                    + " `range`," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata, "  // 2 6
                    + " newdptimestamp,"   // 3 7
                    + " filename,"   // 4 8
                    + " newfilename"   // 5 9
                    + " FROM `" + this.m_strSqlDbName + "`.records "
                    + " WHERE id='" + strID + "'\n";    // 最好改造为 @id

                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {

                    using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在，需要创建
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            row_info = new RecordRowInfo();
                            row_info.Range = "";
                            row_info.TimestampString = "";
                            row_info.NewTimestampString = "";
                            row_info.Metadata = "<file size='0'/>";
                            row_info.FileName = "";
                            row_info.NewFileName = "";
                            return 2;
                            // goto DO_CREATE;
                        }

                        // 如果记录已经存在
                        dr.Read();

                        row_info = new RecordRowInfo();

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 0;   // 没有创建新记录
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                // 注： MySql 这里和 SQLite 基本一样
                string strCommand = " SELECT "
                    + " range," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata, "  // 2 6
                    + " newdptimestamp,"   // 3 7
                    + " filename,"   // 4 8
                    + " newfilename"   // 5 9
                    + " FROM " + this.m_strSqlDbName + "_records "
                    + " WHERE id='" + strID + "'\n";    // 最好改造为 @id

                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {
                    using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 如果记录不存在，需要创建
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            row_info = new RecordRowInfo();
                            row_info.Range = "";
                            row_info.TimestampString = "";
                            row_info.NewTimestampString = "";
                            row_info.Metadata = "<file size='0'/>";
                            row_info.FileName = "";
                            row_info.NewFileName = "";
                            return 2;
                            // goto DO_CREATE;
                        }

                        // 如果记录已经存在
                        dr.Read();

                        row_info = new RecordRowInfo();

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 0;   // 没有创建新记录
            }
            return 0;   // 没有创建新记录
        }

        // return:
        //      -1  出错
        //      0   记录不存在
        //      1   成功
        private int GetRowInfo(Connection connection,
        string strID,
        out RecordRowInfo row_info,
        out string strError)
        {
            strError = "";
            row_info = null;

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strSelect = " SELECT TEXTPTR(data)," // 0
                    + " DataLength(data),"  // 1
                    + " TEXTPTR(newdata),"  // 2
                    + " DataLength(newdata),"   // 3
                    + " range," // 4
                    + " dptimestamp,"   // 5
                    + " metadata,"  // 6
                    + " newdptimestamp, "   // 7
                    + " filename, "   // 8
                    + " newfilename "   // 9
                    + " FROM records "
                    + " WHERE id='" + strID + "'\n";

                string strCommand = "use " + this.m_strSqlDbName + " \n"
                    + "SET NOCOUNT OFF\n"
                    + strSelect;
                strCommand += " use master " + "\n";

                using (SqlCommand command = new SqlCommand(strCommand,
                    connection.SqlConnection))
                {

                    using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 1.记录不存在报错
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            strError = "记录 '" + strID + "' 在库中不存在";
                            return 0;
                        }

                        dr.Read();

                        row_info = new RecordRowInfo();

                        /*
                        // 2.textPtr为null报错
                        if (dr[0] is System.DBNull)
                        {
                            strError = "TextPtr不可能为null";
                            return -1;
                        }
                         * */

                        row_info.data_textptr = (byte[])GetValue(dr[0]);

                        if (dr.IsDBNull(1) == false)
                            row_info.data_length = dr.GetInt32(1);

                        row_info.newdata_textptr = (byte[])GetValue(dr[2]);

                        if (dr.IsDBNull(3) == false)
                            row_info.newdata_length = dr.GetInt32(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.Range = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.TimestampString = dr.GetString(5);

                        if (dr.IsDBNull(6) == false)
                            row_info.Metadata = dr.GetString(6);

                        if (dr.IsDBNull(7) == false)
                            row_info.NewTimestampString = dr.GetString(7);

                        if (dr.IsDBNull(8) == false)
                            row_info.FileName = dr.GetString(8);

                        if (dr.IsDBNull(9) == false)
                            row_info.NewFileName = dr.GetString(9);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 1;
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = " SELECT "
                    + " range," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata,"  // 2 6
                    + " newdptimestamp, "   // 3 7
                    + " filename, "   // 4 8
                    + " newfilename "   // 5 9
                    + " FROM records "
                    + " WHERE id='" + strID + "'\n";

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
                    connection.SQLiteConnection))
                {

                    using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 1.记录不存在报错
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            strError = "记录 '" + strID + "' 在库中不存在";
                            return 0;
                        }

                        dr.Read();

                        row_info = new RecordRowInfo();

                        /*
                        row_info.data_textptr = (byte[])GetValue(dr[0]);

                        if (dr.IsDBNull(1) == false)
                            row_info.data_length = dr.GetInt32(1);

                        row_info.newdata_textptr = (byte[])GetValue(dr[2]);

                        if (dr.IsDBNull(3) == false)
                            row_info.newdata_length = dr.GetInt32(3);
                         * */

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 1;
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                string strCommand = " SELECT "
                    + " `range`," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata,"  // 2 6
                    + " newdptimestamp, "   // 3 7
                    + " filename, "   // 4 8
                    + " newfilename "   // 5 9
                    + " FROM `" + this.m_strSqlDbName + "`.records "
                    + " WHERE id='" + strID + "'\n";

                using (MySqlCommand command = new MySqlCommand(strCommand,
                    connection.MySqlConnection))
                {

                    using (MySqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 1.记录不存在报错
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            strError = "记录 '" + strID + "' 在库中不存在";
                            return 0;
                        }

                        dr.Read();

                        row_info = new RecordRowInfo();

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                } // end of using command

                return 1;
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                string strCommand = " SELECT "
                    + " range," // 0 4
                    + " dptimestamp,"   // 1 5
                    + " metadata,"  // 2 6
                    + " newdptimestamp, "   // 3 7
                    + " filename, "   // 4 8
                    + " newfilename "   // 5 9
                    + " FROM " + this.m_strSqlDbName + "_records "
                    + " WHERE id='" + strID + "'\n";

                using (OracleCommand command = new OracleCommand(strCommand,
                    connection.OracleConnection))
                {
                    using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        // 1.记录不存在报错
                        if (dr == null
                            || dr.HasRows == false)
                        {
                            strError = "记录 '" + strID + "' 在库中不存在";
                            return 0;
                        }

                        dr.Read();

                        row_info = new RecordRowInfo();

                        if (dr.IsDBNull(0) == false)
                            row_info.Range = dr.GetString(0);

                        if (dr.IsDBNull(1) == false)
                            row_info.TimestampString = dr.GetString(1);

                        if (dr.IsDBNull(2) == false)
                            row_info.Metadata = dr.GetString(2);

                        if (dr.IsDBNull(3) == false)
                            row_info.NewTimestampString = dr.GetString(3);

                        if (dr.IsDBNull(4) == false)
                            row_info.FileName = dr.GetString(4);

                        if (dr.IsDBNull(5) == false)
                            row_info.NewFileName = dr.GetString(5);

                        bool bRet = dr.Read();

                        if (bRet == true)
                        {
                            // 还有一行
                            strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，这是一种不正常的状态, 请系统管理员利用SQL命令删除多余的记录。";
                            return -1;
                        }
                    }
                }
                return 1;
            }
            else
            {
                strError = "未知的数据库类型: " + connection.SqlServerType.ToString();
                return -1;
            }

            // return 0;
        }

        static object GetValue(object obj)
        {
            if (obj is System.DBNull)
                return null;
            return obj;
        }

#if NO

        // 从库中得到一个记录的时间戳
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        private int GetTimestampFromDb(SqlConnection connection,
            string strID,
            out byte[] outputTimestamp,
            out string strError)
        {
            strError = "";
            outputTimestamp = null;
            int nRet = 0;

            string strOutputRecordID = "";
            // return:
            //      -1  出错
            //      0   成功
            nRet = this.CanonicalizeRecordID(strID,
                out strOutputRecordID,
                out strError);
            if (nRet == -1)
            {
                strError = "GetTimestampFormDb()调用错误，strID参数值 '" + strID + "' 不合法。";
                return -1;
            }
            if (strOutputRecordID == "-1")
            {
                strError = "GetTimestampFormDb()调用错误，strID参数值 '" + strID + "' 不合法。";
                return -1;
            }
            strID = strOutputRecordID;


            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + " "
                + "select dptimestamp, newdptimestamp, range"
                + " from records "
                + " where id='" + strID + "'";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr == null
                    || dr.HasRows == false)
                {
                    strError = "GetTimestampFromDb() 发现记录'" + strID + "'在库中不存在";
                    return -4;
                }
                dr.Read();

                bool bReverse = false;  // strRange第一字符为'#'，也和 bReverse==false 一样，都是使用 dptimestamp 字段
                string strRange = "";
                if (dr.IsDBNull(2) == false)
                    strRange = dr.GetString(2);

                if (string.IsNullOrEmpty(strRange) == false
                    && strRange[0] == '!')
                    bReverse = true;

                string strOutputTimestamp = "";
                
                if (bReverse == false)
                    strOutputTimestamp = dr.GetString(0);
                else
                    strOutputTimestamp = dr.GetString(1);

                outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);

                bool bRet = dr.Read();

                // 2008/3/13 
                if (bRet == true)
                {
                    // 还有一行
                    strError = "记录 '" + strID + "' 在SQL库" + this.m_strSqlDbName + "的records表中存在多条，请系统管理员利用SQL命令删除多余的记录。";
                    return -1;
                }

            }
            finally
            {
                dr.Close();
            }
            return 0;
        }

        // 获取记录的时间戳
        // parameters0:
        //      strID   记录id
        //      baOutputTimestamp
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        public override int GetTimestampFromDb(
            string strID,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            // 打开连接对象
            SqlConnection connection = new SqlConnection(this.m_strConnString);
            connection.Open();
            try
            {
                // return:
                //		-1  出错
                //		-4  未找到记录
                //      0   成功
                return this.GetTimestampFromDb(connection,
                    strID,
                    out baOutputTimestamp,
                    out strError);
            }
            finally
            {
                connection.Close();
            }
        }

#endif

#if NO
        // 设置指定记录的时间戳
        // parameters:
        //      connection  SqlConnection对象
        //      strID       记录id，可以是记录也可以是资源
        //      strInputTimestamp   输入的时间戳
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回被影响的记录数
        private int SetTimestampForDb(SqlConnection connection,
            string strID,
            string strInputTimestamp,
            out string strError)
        {
            strError = "";

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + "\n"
                + " UPDATE records "
                + " SET dptimestamp=@dptimestamp"
                + " WHERE id=@id";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            SqlParameter idParam = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strInputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount == 0)
            {
                strError = "没有更新到记录号为'" + strID + "'的时间戳";
                return -1;
            }
            return nCount;
        }
#endif

        // 删除记录,包括子文件,检索点,和本记录
        // parameter:
        //		strRecordID           记录ID
        //      strStyle        可包含 fastmode。ignorechecktimestamp
        //                      forcedeleteoldkeys 表示强制删除旧记录的所有检索点。常用于检索点配置文件或者检索点算法发生改变以后的删除操作，可以确保把检索点删除干净。2020/7/1 增加
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp out参数,返回的实际的时间戳
        //		strError        out参数,返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全
        public override int DeleteRecord(
            string strRecordID,
            string strObjectID,
            byte[] baInputTimestamp,
            string strStyle,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;

            if (StringUtil.IsInList("fastmode", strStyle) == true)
                this.FastMode = true;
            bool bFastMode = StringUtil.IsInList("fastmode", strStyle) || this.FastMode;

#if NO
            bool bDeleteKeysByID = false;   //  StringUtil.IsInList("fastmode", strStyle) || this.FastMode;
#endif
            bool bDeleteKeysByID = StringUtil.IsInList("deletekeysbyid", strStyle);
            // 2015/9/4
            bool bIgnoreCheckTimestamp = StringUtil.IsInList("ignorechecktimestamp", strStyle);

            strRecordID = DbPath.GetID10(strRecordID);

            // 2017/9/17
            bool bObject = false;
            if (string.IsNullOrEmpty(strObjectID) == false)
            {
                strRecordID = strRecordID + "_" + strObjectID;
                bObject = true;
            }

            // 这里不再因为FastMode加写锁

            //********对数据库加读锁*********************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif

            int nRet = 0;
            try
            {
                //*********对记录加写锁**********
                m_recordLockColl.LockForWrite(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                try
                {
                    Connection connection = GetConnection(
                        this.m_strConnString,
                        this.container.SqlServerType == SqlServerType.SQLite && bFastMode == true ? ConnectionStyle.Global : ConnectionStyle.None);
                    connection.TryOpen();
                    try
                    {
                        connection._nOpenCount += 10;

                        RecordRowInfo row_info = null;
                        // return:
                        //      -1  出错
                        //      0   记录不存在
                        //      1   成功
                        nRet = GetRowInfo(connection,
        strRecordID,
        out row_info,
        out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)  // 2013/11/21
                            return -4;

                        // 比较时间戳
                        string strCompleteTimestamp = row_info.TimestampString; // 上次完整写入时的时间戳

                        // 已有数据的时间戳
                        if (String.IsNullOrEmpty(row_info.Range) == false
                && row_info.Range[0] == '!')
                            strCompleteTimestamp = row_info.NewTimestampString;

                        baOutputTimestamp = ByteArray.GetTimeStampByteArray(strCompleteTimestamp);

                        if (bIgnoreCheckTimestamp == false
                            && ByteArray.Compare(baInputTimestamp,
        baOutputTimestamp) != 0)
                        {
                            strError = "时间戳不匹配";
                            return -2;
                        }

                        XmlDocument newDom = null;
                        XmlDocument oldDom = null;

                        // 处理检索点
                        if (bObject == false)
                        {

                            KeyCollection newKeys = null;
                            KeyCollection oldKeys = null;

                            if (bDeleteKeysByID == false)
                            {

                                string strXml = "";
                                // return:
                                //      -1  出错
                                //      -4  记录不存在
                                //      -100    对象文件不存在
                                //      0   正确
                                nRet = this.GetXmlString(connection,
                                    strRecordID,
                                    out strXml,
                                    out strError);
                                if (nRet == -100)
                                    strXml = "";
                                else if (nRet <= -1)
                                    return nRet;

                                // 1.删除检索点

                                // return:
                                //      -2  出错。strOldXml 结构不合法
                                //      -1  出错
                                //      0   成功
                                nRet = this.MergeKeys(strRecordID,
                                    "",
                                    strXml,
                                    true,
                                    out newKeys,
                                    out oldKeys,
                                    out newDom,
                                    out oldDom,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "删除中构造检索点阶段出错： " + strError;
                                    return -1;
                                }
                                // 2021/8/27
                                if (nRet == -2)
                                    strStyle += ",forcedeleteoldkeys";
                            }

                            if (oldDom != null
                                && StringUtil.IsInList("forcedeleteoldkeys", strStyle) == false) // 2020/7/1 增加
                            {
                                // return:
                                //      -1  出错
                                //      0   成功
                                nRet = this.ModifyKeys(connection,
                                    null,
                                    oldKeys,
                                    bFastMode,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
                            else
                            {
                                // return:
                                //      -1  出错
                                //      0   成功
                                nRet = this.ForceDeleteKeys(connection,
                                    strRecordID,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
                        }

                        // 删除记录
                        if (this.container.SqlServerType == SqlServerType.Oracle)
                        {
                            // 2.删除子记录
                            if (oldDom != null)
                            {
                                // return:
                                //      -1  出错
                                //      0   成功
                                nRet = this.ModifyFiles(connection,
                                    strRecordID,
                                    null,
                                    oldDom,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }

                            // 删除自己,返回删除的记录数
                            // return:
                            //      -1  出错
                            //      >=0   成功 返回删除的记录数
                            nRet = DeleteRecordByID(connection,
                                row_info,
                                strRecordID,
                                oldDom != null || bObject == true ? false : true,
                                this.m_lObjectStartSize != -1,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            if (nRet == 0)
                            {
                                strError = "删除记录时,从库中没找到记录号为'" + strRecordID + "'的记录";
                                return -1;
                            }
                        }
                        else
                        {
                            // 3.删除自己,返回删除的记录数
                            // return:
                            //      -1  出错
                            //      >=0   成功 返回删除的记录数
                            nRet = DeleteRecordByID(connection,
                                row_info,
                                strRecordID,
                                !bObject,    // true,
                                this.m_lObjectStartSize != -1,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            if (nRet == 0)
                            {
                                strError = "删除记录时,从库中没找到记录号为'" + strRecordID + "'的记录";
                                return -1;
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        strError = GetSqlErrors(sqlEx);

                        /*
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                         * */
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "删除'" + this.GetCaption("zh-CN") + "'库中id为'" + strRecordID + "'的记录时出错,原因:" + ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //**************对记录解写锁**********
                    m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif

                }
            }
            finally
            {
                //***************对数据库解读锁*****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            if (StringUtil.IsInList("fastmode", strStyle) == false
        && this.FastMode == true)
            {
                // this.FastMode = false;
                this.Commit();
            }

            return 0;
        }

        // 重建记录的keys
        // parameter:
        //		strRecordID           记录ID
        //      strStyle    next prev outputpath forcedeleteoldkeys
        //                  forcedeleteoldkeys 要在创建新keys前强制删除一下旧有的keys? 如果为包含，则强制删除原有的keys；如果为不包含，则试探着创建新的keys，如果有旧的keys和新打算创建的keys重合，那就不重复创建；如果旧的keys有残余没有被删除，也不管它们了
        //                          包含 一般用在单条记录的处理；不包含 一般用在预先删除了所有keys表的内容行以后在循环重建库中每条记录的批处理方式
        //		strError        out参数,返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全
        public override int RebuildRecordKeys(string strRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strError = "";
            strOutputRecordID = "";
            int nRet = 0;

            if (StringUtil.IsInList("fastmode", strStyle) == true)
                this.FastMode = true;
            bool bFastMode = StringUtil.IsInList("fastmode", strStyle) || this.FastMode;

            strRecordID = DbPath.GetID10(strRecordID);

            //********对数据库加读锁*********************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("RebuildRecordKeys()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            ////
            try // lock database
            {
                Connection connection = new Connection(this,
                    this.m_strConnString);
                connection.TryOpen();
                try // connection
                {
                    // 检查ID
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 将样式去空白
                    strStyle = strStyle.Trim();

                    // 取出实际的记录号
                    if (StringUtil.IsInList("prev", strStyle) == true
                        || StringUtil.IsInList("next", strStyle) == true)
                    {
                        string strTempOutputID = "";
                        // return:
                        //		-1  出错
                        //      0   未找到
                        //      1   找到
                        nRet = this.GetRecordID(connection,
                            strRecordID,
                            strStyle,
                            out strTempOutputID,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (nRet == 0 || strTempOutputID == "")
                        {
                            strError = "未找到记录ID '" + strRecordID + "' 的风格为 '" + strStyle + "' 的记录";
                            return -4;
                        }

                        strRecordID = strTempOutputID;

                        // 再次检查一下返回的ID
                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    // 根据风格要求，返回资源路径
                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputRecordID = DbPath.GetCompressedID(strRecordID);
                    }


                    //*********对记录加写锁**********
                    m_recordLockColl.LockForWrite(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("RebuildRecordKeys()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                    try // lock record
                    {
                        string strXml;
                        // return:
                        //      -1  出错
                        //      -4  记录不存在
                        //      -100    对象文件不存在
                        //      0   正确
                        nRet = this.GetXmlString(connection,
                            strRecordID,
                            out strXml,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        XmlDocument newDom = null;
                        XmlDocument oldDom = null;

                        KeyCollection newKeys = null;
                        KeyCollection oldKeys = null;

                        // TODO: 是否可以两次操作用一个command字符串实现？

                        // return:
                        //      -2  出错。strOldXml 结构不合法
                        //      -1  出错
                        //      0   成功
                        nRet = this.MergeKeys(strRecordID,
                            strXml, // newxml
                            "", // oldxml
                            true,
                            out newKeys,
                            out oldKeys,
                            out newDom,
                            out oldDom,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        // 2021/8/27
                        if (nRet == -2)
                            strStyle += ",forcedeleteoldkeys";

                        if (StringUtil.IsInList("forcedeleteoldkeys", strStyle) == true)
                        {
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ForceDeleteKeys(connection,
                                strRecordID,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        if (newDom != null)
                        {
                            // TODO: 当bForceDeleteOldKeys为false的时候，看看重复创建keyu是否会报错？怎样避免？

                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyKeys(connection,
                                newKeys,
                                null,
                                bFastMode,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }


                    } // end of lock record
                    finally // 记录锁
                    {
                        //**************对记录解写锁**********
                        m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("RebuildRecordKeys()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif
                    }

                    ////
                } // enf of try connection
                catch (SqlException sqlEx)
                {
                    strError = GetSqlErrors(sqlEx);
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = "重建 '" + this.GetCaption("zh-CN") + "' 库中id为 '" + strRecordID + "' 的记录的相关keys时出错,原因:" + ex.Message;
                    return -1;
                }
                finally // 连接
                {
                    connection.Close();
                }

            } // lock database
            finally
            {
                //***************对数据库解读锁*****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("RebuildRecordKeys()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            if (StringUtil.IsInList("fastmode", strStyle) == false
        && this.FastMode == true)
            {
                // this.FastMode = false;
                this.Commit();
            }

            return 0;
        }

#if NO
        // 2011/1/16
        // 删除子文件
        // 和ModifyFiles()函数的区别，是用like算法来删除所有子记录，包括<dprms:file>中没有记载的子记录
        // return:
        //      -1  出错
        //      0   成功
        public int DeleteSubRecords(SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";
            strID = DbPath.GetID10(strID);

            SqlConnection new_connection = null;
            if (connection.ConnectionTimeout < m_nLongTimeout)
            {
                new_connection = new SqlConnection(this.m_strLongConnString);
                new_connection.Open();
                connection = new_connection;
            }


            try
            {
                SqlCommand command = new SqlCommand("", connection);

                string strCommand = "use " + this.m_strSqlDbName + " \n"
                        + " DELETE FROM records WHERE id like '" + strID + "_%' \n"
                        + " use master " + "\n";

                command.CommandText = strCommand;
                command.CommandTimeout = m_nLongTimeout; // 30分钟

                int nResultCount = 0;
                try
                {
                    nResultCount = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "删除记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                    return -1;
                }
                return 0;
            }
            finally
            {
                if (new_connection != null)
                    new_connection.Close();
            }
        }

        // 根据记录号之间的关系(记录号~~记录号_0),强制删除资源文件
        // parameters:
        //      connection  SqlConnection对象
        //      strRecordID 记录id  必须是10位
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        private int ForceDeleteFiles(SqlConnection connection,
            string strRecordID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 连接连接
            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(strRecordID != null && strRecordID.Length == 10, "ForceDeleteFiles()调用错误，strRecordID参数值不能为null且长度必须等于10位。");

            string strCommand = "use " + this.m_strSqlDbName + " "
                + " DELETE FROM records WHERE id like @id";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            command.CommandTimeout = m_nLongTimeout; // 30分钟

            SqlParameter param = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            param.Value = strRecordID + "_%";

            //???如果处理删除数量
            int nDeletedCount = command.ExecuteNonQuery();

            return 0;
        }
#endif


#if NOOOOOOOOOOOOOO
        // 2007/4/16
        // 强制删除属于一个记录的全部检索点。不需要检索点定义。
        // parameters:
        //      strRecordID 必须为10位的数字
        // return:
        //      -1  出错
        //      0   成功
        public int ForceDeleteKeys(SqlConnection connection,
            string strRecordID,
            out string strError)
        {
            strError = "";
            string strCommand = "";

            KeysCfg keysCfg = null;

            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;

            List<TableInfo> aTableInfo = null;
            nRet = keysCfg.GetTableInfosRemoveDup(
                out aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            SqlCommand command = new SqlCommand("", connection);

            // 循环全部表
            for (int i = 0; aTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTableInfo[i];

                string strKeysTableName = tableInfo.SqlTableName;

                string strIdParamName = "@id" + i.ToString();

                strCommand += " DELETE FROM " + strKeysTableName
                    + " WHERE idstring= " + strIdParamName;

                SqlParameter idParam =
                    command.Parameters.Add(strIdParamName,
                    SqlDbType.NVarChar);
                idParam.Value = strRecordID;

                SqlParameter keynumParam =
                    command.Parameters.Add(strKeynumParamName,
                    SqlDbType.NVarChar);
                keynumParam.Value = oneKey.Num;
            }

            strCommand = "use " + this.m_strSqlDbName + " \n"
                + strCommand
                + " use master " + "\n";
            command.CommandText = strCommand;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                strError = "强制检索点出错,记录路径'" + this.GetCaption("zh-CN") + "/" + strRecordID + "，原因：" + ex.Message;
                return -1;
            }

            return 0;
        }
#endif

        // 构造用于 where idstring in (...) 的 ID 列表字符串
        static int BuildIdString(List<string> ids,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            StringBuilder idstring = new StringBuilder(4096);
            int i = 0;
            foreach (string s in ids)
            {
                if (string.IsNullOrEmpty(s) == true || s.Length != 10)
                {
                    strError = "ID字符串 '" + s + "' 不合法";
                    return -1;
                }
                if (StringUtil.IsPureNumber(s) == false)
                {
                    strError = "ID '" + s + "' 必须是纯数字";
                    return -1;
                }
                if (i != 0)
                    idstring.Append(",");
                idstring.Append("'" + s + "'");
                i++;
            }

            strResult = idstring.ToString();
            return 0;
        }

        // 强制删除记录对应的检索点,检查所有的表
        // parameters:
        //      connection  SqlConnection连接对象
        //      ids         记录id数组。每个 id 应为 10 字符形态
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0 成功，数字表示实际删除的检索点个数
        // 线: 不安全
        public int ForceDeleteKeys(Connection connection,
            List<string> ids,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            foreach (string strRecordID in ids)
            {
                Debug.Assert(strRecordID != null && strRecordID.Length == 10, "ForceDeleteKeys()调用错误，strRecordID参数值不能为null且长度必须等于10位。");
                if (string.IsNullOrEmpty(strRecordID) == true || strRecordID.Length != 10)
                {
                    strError = "ForceDeleteKeys() ID字符串 '" + strRecordID + "' 不合法";
                    return -1;
                }
            }

            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;
            if (keysCfg == null)
                return 0;

            List<TableInfo> aTableInfo = null;
            nRet = keysCfg.GetTableInfosRemoveDup(
                out aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            if (aTableInfo.Count == 0)
                return 0;

            string strIdString = "";
            nRet = BuildIdString(ids, out strIdString, out strError);
            if (nRet == -1)
                return -1;

            int nDeletedCount = 0;

            if (container.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    //strCommand += "DELETE FROM " + tableInfo.SqlTableName
                    //    + " WHERE idstring=@id \r\n";
                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring in (" + strIdString + ")\r\n";
                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    strCommand = "use " + this.m_strSqlDbName + " \r\n"
                        + strCommand
                        + "use master " + "\r\n";

                    using (SqlCommand command = new SqlCommand(strCommand,
                        connection.SqlConnection))
                    {
#if NO
                        SqlParameter idParam = command.Parameters.Add("@id",
                            SqlDbType.NVarChar);
                        idParam.Value = strRecordID;
#endif

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

#if NO
                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring=@id ;\r\n";
#endif
                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
        + " WHERE idstring IN (" + strIdString + ") ;\r\n";

                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    using (SQLiteCommand command = new SQLiteCommand(strCommand,
                        connection.SQLiteConnection))
                    {
#if NO
                        SQLiteParameter idParam = command.Parameters.Add("@id",
                            DbType.String);
                        idParam.Value = strRecordID;
#endif

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.MySql)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

#if NO
                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring=@id ;\r\n";
#endif
                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
        + " WHERE idstring IN (" + strIdString + ") ;\r\n";

                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
        + strCommand;

                    using (MySqlCommand command = new MySqlCommand(strCommand,
                        connection.MySqlConnection))
                    {
#if NO
                        MySqlParameter idParam = command.Parameters.Add("@id",
                            MySqlDbType.String);
                        idParam.Value = strRecordID;
#endif

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.Oracle)
            {
                using (OracleCommand command = new OracleCommand("",
        connection.OracleConnection))
                {
                    command.BindByName = true;

                    string strCommand = "";
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

#if NO
                        strCommand = "DELETE FROM " + this.m_strSqlDbName + "_" + tableInfo.SqlTableName
                            + " WHERE idstring=:id \r\n";

                        OracleParameter idParam = command.Parameters.Add(":id",
                            OracleDbType.NVarchar2);
                        idParam.Value = strRecordID;
#endif

                        strCommand = "DELETE FROM " + this.m_strSqlDbName + "_" + tableInfo.SqlTableName
        + " WHERE idstring IN (" + strIdString + ") \r\n";

                        // ????如果处理删除数量
                        command.CommandText = strCommand;
                        nDeletedCount += command.ExecuteNonQuery();

                        command.Parameters.Clear();
                    }
                } // end of using command

                return nDeletedCount;
            }

            return 0;
        }

        // 强制删除记录对应的检索点,检查所有的表
        // parameters:
        //      connection  SqlConnection连接对象
        //      strRecordID 记录id, 调之前必须设为10字符
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0 成功，数字表示实际删除的检索点个数
        // 线: 不安全
        public int ForceDeleteKeys(Connection connection,
            string strRecordID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(strRecordID != null && strRecordID.Length == 10, "ForceDeleteKeys()调用错误，strRecordID参数值不能为null且长度必须等于10位。");

            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;
            if (keysCfg == null)
                return 0;

            List<TableInfo> aTableInfo = null;
            nRet = keysCfg.GetTableInfosRemoveDup(
                out aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            if (aTableInfo.Count == 0)
                return 0;

            int nDeletedCount = 0;

            if (container.SqlServerType == SqlServerType.MsSqlServer)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring=@id \r\n";
                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    strCommand = "use " + this.m_strSqlDbName + " \r\n"
                        + strCommand
                        + "use master " + "\r\n";

                    using (SqlCommand command = new SqlCommand(strCommand,
                        connection.SqlConnection))
                    {
                        SqlParameter idParam = command.Parameters.Add("@id",
                            SqlDbType.NVarChar);
                        idParam.Value = strRecordID;

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.SQLite)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring=@id ;\r\n";
                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    using (SQLiteCommand command = new SQLiteCommand(strCommand,
                        connection.SQLiteConnection))
                    {
                        SQLiteParameter idParam = command.Parameters.Add("@id",
                            DbType.String);
                        idParam.Value = strRecordID;

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.MySql)
            {
                string strCommand = "";
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    strCommand += "DELETE FROM " + tableInfo.SqlTableName
                        + " WHERE idstring=@id ;\r\n";
                }

                if (string.IsNullOrEmpty(strCommand) == false)
                {
                    strCommand = "use `" + this.m_strSqlDbName + "` ;\n"
        + strCommand;

                    using (MySqlCommand command = new MySqlCommand(strCommand,
                        connection.MySqlConnection))
                    {
                        MySqlParameter idParam = command.Parameters.Add("@id",
                            MySqlDbType.String);
                        idParam.Value = strRecordID;

                        // ????如果处理删除数量
                        nDeletedCount = command.ExecuteNonQuery();
                    } // end of using command
                }

                return nDeletedCount;
            }
            else if (container.SqlServerType == SqlServerType.Oracle)
            {
                using (OracleCommand command = new OracleCommand("",
        connection.OracleConnection))
                {
                    command.BindByName = true;

                    string strCommand = "";
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];

                        strCommand = "DELETE FROM " + this.m_strSqlDbName + "_" + tableInfo.SqlTableName
                            + " WHERE idstring=:id \r\n";

                        OracleParameter idParam = command.Parameters.Add(":id",
                            OracleDbType.NVarchar2);
                        idParam.Value = strRecordID;

                        // ????如果处理删除数量
                        command.CommandText = strCommand;
                        nDeletedCount += command.ExecuteNonQuery();

                        command.Parameters.Clear();
                    }
                } // end of using command

                return nDeletedCount;
            }

            return 0;
        }

        // 从库中删除指定的记录或者对象资源
        // parameters:
        //      connection  连接对象
        //      strID       记录id
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回删除的记录数
        private int DeleteRecordByID(
            Connection connection,
            RecordRowInfo row_info,
            string strID,
            bool bDeleteSubrecord,
            bool bDeleteObjectFiles,
            out string strError)
        {
            strError = "";

            Debug.Assert(connection != null, "DeleteRecordById()调用错误，connection参数值不能为null。");
            Debug.Assert(strID != null, "DeleteRecordById()调用错误，strID参数值不能为null。");
            Debug.Assert(strID.Length >= 10, "DeleteRecordByID()调用错误 strID参数值的长度必须大于等于10。");

            int nDeletedCount = 0;

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> filenames = new List<string>();

            if (connection.SqlServerType == SqlServerType.MsSqlServer)
            {
                SqlConnection current_connection = null;
                SqlConnection new_connection = null;
                if (connection.SqlConnection.ConnectionTimeout < m_nLongTimeout)
                {
                    new_connection = new SqlConnection(this.m_strLongConnString);
                    new_connection.Open();
                    current_connection = new_connection;
                }
                else
                    current_connection = connection.SqlConnection;

                try
                {
                    using (SqlCommand command = new SqlCommand("",
            current_connection))
                    {
                        string strCommand = "";

                        // 第一步：获得短文件名
                        if (bDeleteObjectFiles == true)
                        {
                            if (row_info != null && bDeleteSubrecord == false)
                            {
                                // 后面可以通过 row_info 来删除记录的对象文件
                            }
                            else if (bDeleteSubrecord == true)
                            {
                                // TODO: 需要获得全部filename和newfilename字段内容值
                                if (strID.Length != 10)
                                {
                                    strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                                    return -1;
                                }
                                Debug.Assert(strID.Length == 10, "DeleteRecordByID() 的 strID 必须是 10 字符");

                                strCommand = "use " + this.m_strSqlDbName + " "
                                    + " SELECT filename, newfilename FROM records WHERE id like @id1 OR id = @id2";
                                strCommand += " use master " + "\n";

                                command.CommandText = strCommand;
                                command.CommandTimeout = m_nLongTimeout;// 30分钟

                                SqlParameter param1 = command.Parameters.Add("@id1",
                SqlDbType.NVarChar);
                                param1.Value = strID + "_%";

                                SqlParameter param2 = command.Parameters.Add("@id2",
                                    SqlDbType.NVarChar);
                                param2.Value = strID;
                            }
                            else if (row_info == null)
                            {
                                strCommand = "use " + this.m_strSqlDbName + " "
            + " SELECT filename, newfilename FROM records WHERE id = @id";
                                strCommand += " use master " + "\n";

                                command.CommandText = strCommand;
                                command.CommandTimeout = m_nLongTimeout;// 30分钟

                                SqlParameter param = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
                                param.Value = strID;
                            }
                        }

                        if (string.IsNullOrEmpty(strCommand) == false)
                        {
                            using (SqlDataReader dr = command.ExecuteReader())
                            {
                                if (dr != null && dr.HasRows == true)
                                {
                                    while (dr.Read())
                                    {
                                        if (dr.IsDBNull(0) == false)
                                            filenames.Add(dr.GetString(0));
                                        if (dr.IsDBNull(1) == false)
                                            filenames.Add(dr.GetString(1));
                                    }
                                }
                            }
                        }

                        // 第二步：删除SQL行
                        if (bDeleteSubrecord == true)
                        {
                            if (strID.Length != 10)
                            {
                                strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                                return -1;
                            }
                            Debug.Assert(strID.Length == 10, "");

                            strCommand = "use " + this.m_strSqlDbName + " "
                                + " DELETE FROM records WHERE id like @id1 OR id = @id2";
                            strCommand += " use master " + "\n";

                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟
                            command.Parameters.Clear();

                            SqlParameter param1 = command.Parameters.Add("@id1",
                                SqlDbType.NVarChar);
                            param1.Value = strID + "_%";

                            SqlParameter param2 = command.Parameters.Add("@id2",
                                SqlDbType.NVarChar);
                            param2.Value = strID;
                        }
                        else
                        {
                            strCommand = "use " + this.m_strSqlDbName + " "
            + " DELETE FROM records WHERE id = @id";
                            strCommand += " use master " + "\n";

                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟
                            command.Parameters.Clear();

                            SqlParameter param = command.Parameters.Add("@id",
                                SqlDbType.NVarChar);
                            param.Value = strID;
                        }

                        nDeletedCount = command.ExecuteNonQuery();
                        if (nDeletedCount != 1)
                        {
                            this.container.KernelApplication.WriteErrorLog("希望删除" + strID + " '1'条，实际删除'" + Convert.ToString(nDeletedCount) + "'个");
                        }
                    } // end of using command
                }
                finally
                {
                    if (new_connection != null)
                        new_connection.Close();
                }
            }
            else if (connection.SqlServerType == SqlServerType.SQLite)
            {
                using (SQLiteCommand command = new SQLiteCommand("",
                            connection.SQLiteConnection))
                {
                    string strCommand = "";

                    // 第一步：获得短文件名
                    if (bDeleteObjectFiles == true)
                    {
                        if (row_info != null && bDeleteSubrecord == false)
                        {
                            // 后面可以通过 row_info 来删除记录的对象文件
                        }
                        else if (bDeleteSubrecord == true)
                        {
                            // TODO: 需要获得全部filename和newfilename字段内容值
                            if (strID.Length != 10)
                            {
                                strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                                return -1;
                            }
                            Debug.Assert(strID.Length == 10, "");

                            strCommand = " SELECT filename, newfilename FROM records WHERE id like @id1 OR id = @id2";
                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            SQLiteParameter param1 = command.Parameters.Add("@id1",
                                DbType.String);
                            param1.Value = strID + "_%";

                            SQLiteParameter param2 = command.Parameters.Add("@id2",
                                DbType.String);
                            param2.Value = strID;
                        }
                        else if (row_info == null)
                        {
                            strCommand = " SELECT filename, newfilename FROM records WHERE id = @id";
                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            SQLiteParameter param = command.Parameters.Add("@id",
                                DbType.String);
                            param.Value = strID;
                        }
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        using (SQLiteDataReader dr = command.ExecuteReader())
                        {
                            if (dr != null && dr.HasRows == true)
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        filenames.Add(dr.GetString(0));
                                    if (dr.IsDBNull(1) == false)
                                        filenames.Add(dr.GetString(1));
                                }
                            }
                        }
                    }

                    // 第二步：删除SQL行
                    if (bDeleteSubrecord == true)
                    {
                        if (strID.Length != 10)
                        {
                            strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                            return -1;
                        }
                        Debug.Assert(strID.Length == 10, "");

                        strCommand = " DELETE FROM records WHERE id like @id1 OR id = @id2";
                        command.CommandText = strCommand;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟
                        command.Parameters.Clear();

                        SQLiteParameter param1 = command.Parameters.Add("@id1",
                            DbType.String);
                        param1.Value = strID + "_%";

                        SQLiteParameter param2 = command.Parameters.Add("@id2",
                            DbType.String);
                        param2.Value = strID;
                    }
                    else
                    {
                        strCommand = " DELETE FROM records WHERE id = @id";
                        command.CommandText = strCommand;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟
                        command.Parameters.Clear();

                        SQLiteParameter param = command.Parameters.Add("@id",
                            DbType.String);
                        param.Value = strID;
                    }

                    nDeletedCount = command.ExecuteNonQuery();
                    if (nDeletedCount != 1)
                    {
                        this.container.KernelApplication.WriteErrorLog("希望删除" + strID + " '1'条，实际删除'" + Convert.ToString(nDeletedCount) + "'个");
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.MySql)
            {
                using (MySqlCommand command = new MySqlCommand("",
                            connection.MySqlConnection))
                {
                    string strCommand = "";

                    // 第一步：获得短文件名
                    if (bDeleteObjectFiles == true)
                    {
                        if (row_info != null && bDeleteSubrecord == false)
                        {
                            // 后面可以通过 row_info 来删除记录的对象文件
                        }
                        else if (bDeleteSubrecord == true)
                        {
                            // TODO: 需要获得全部filename和newfilename字段内容值
                            if (strID.Length != 10)
                            {
                                strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                                return -1;
                            }
                            Debug.Assert(strID.Length == 10, "");

                            strCommand = " SELECT filename, newfilename FROM `" + this.m_strSqlDbName + "`.records WHERE id like @id1 OR id = @id2";
                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            MySqlParameter param1 = command.Parameters.Add("@id1",
                                MySqlDbType.String);
                            param1.Value = strID + "_%";

                            MySqlParameter param2 = command.Parameters.Add("@id2",
                                MySqlDbType.String);
                            param2.Value = strID;
                        }
                        else if (row_info == null)
                        {
                            strCommand = " SELECT filename, newfilename FROM `" + this.m_strSqlDbName + "`.records WHERE id = @id";
                            command.CommandText = strCommand;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            MySqlParameter param = command.Parameters.Add("@id",
                                MySqlDbType.String);
                            param.Value = strID;
                        }
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        using (MySqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr != null && dr.HasRows == true)
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        filenames.Add(dr.GetString(0));
                                    if (dr.IsDBNull(1) == false)
                                        filenames.Add(dr.GetString(1));
                                }
                            }
                        }
                    }

                    // 第二步：删除SQL行
                    if (bDeleteSubrecord == true)
                    {
                        if (strID.Length != 10)
                        {
                            strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                            return -1;
                        }
                        Debug.Assert(strID.Length == 10, "");

                        strCommand = " DELETE FROM `" + this.m_strSqlDbName + "`.records WHERE id like @id1 OR id = @id2";
                        command.CommandText = strCommand;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟
                        command.Parameters.Clear();

                        MySqlParameter param1 = command.Parameters.Add("@id1",
                            MySqlDbType.String);
                        param1.Value = strID + "_%";

                        MySqlParameter param2 = command.Parameters.Add("@id2",
                            MySqlDbType.String);
                        param2.Value = strID;
                    }
                    else
                    {
                        strCommand = " DELETE FROM `" + this.m_strSqlDbName + "`.records WHERE id = @id";
                        command.CommandText = strCommand;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟
                        command.Parameters.Clear();

                        MySqlParameter param = command.Parameters.Add("@id",
                            MySqlDbType.String);
                        param.Value = strID;
                    }

                    nDeletedCount = command.ExecuteNonQuery();
                    if (nDeletedCount != 1)
                    {
                        this.container.KernelApplication.WriteErrorLog("希望删除" + strID + " '1'条，实际删除'" + Convert.ToString(nDeletedCount) + "'个");
                    }
                } // end of using command
            }
            else if (connection.SqlServerType == SqlServerType.Oracle)
            {
                int nExecuteCount = 0;
                using (OracleCommand command = new OracleCommand("",
                    connection.OracleConnection))
                {
                    string strCommand = "";

                    // 第一步：获得短文件名
                    if (bDeleteObjectFiles == true)
                    {
                        if (row_info != null && bDeleteSubrecord == false)
                        {
                            // 后面可以通过 row_info 来删除记录的对象文件
                        }
                        else if (bDeleteSubrecord == true)
                        {
                            // TODO: 需要获得全部filename和newfilename字段内容值
                            if (strID.Length != 10)
                            {
                                strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                                return -1;
                            }
                            Debug.Assert(strID.Length == 10, "");

                            strCommand = " SELECT filename, newfilename FROM " + this.m_strSqlDbName + "_records WHERE id like :id1 OR id = :id2";
                            command.CommandText = strCommand;
                            command.BindByName = true;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            OracleParameter param1 = command.Parameters.Add(":id1",
                                OracleDbType.NVarchar2);
                            param1.Value = strID + "_%";

                            OracleParameter param2 = command.Parameters.Add(":id2",
                                OracleDbType.NVarchar2);
                            param2.Value = strID;
                        }
                        else if (row_info == null)
                        {
                            strCommand = " SELECT filename, newfilename FROM " + this.m_strSqlDbName + "_records WHERE id = :id";
                            command.CommandText = strCommand;
                            command.BindByName = true;
                            command.CommandTimeout = m_nLongTimeout;// 30分钟

                            OracleParameter param = command.Parameters.Add(":id",
                                OracleDbType.NVarchar2);
                            param.Value = strID;
                        }
                    }

                    if (string.IsNullOrEmpty(strCommand) == false)
                    {
                        nExecuteCount++;
                        using (OracleDataReader dr = command.ExecuteReader())
                        {
                            if (dr != null
                                && dr.HasRows == true)
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        filenames.Add(dr.GetString(0));
                                    if (dr.IsDBNull(1) == false)
                                        filenames.Add(dr.GetString(1));
                                }
                            }
                        }
                        command.Parameters.Clear();
                    }

                    // 第二步：删除SQL行
                    if (bDeleteSubrecord == true)
                    {
                        if (strID.Length != 10)
                        {
                            strError = "DeleteRecordByID() 的 strID 必须是 10 字符(当前 strID='" + strID + "')";
                            return -1;
                        }
                        Debug.Assert(strID.Length == 10, "");

                        strCommand = " DELETE FROM " + this.m_strSqlDbName + "_records WHERE id like :id1 OR id = :id2";
                        command.CommandText = strCommand;
                        command.BindByName = true;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟

                        OracleParameter param1 = command.Parameters.Add(":id1",
                            OracleDbType.NVarchar2);
                        param1.Value = strID + "_%";

                        OracleParameter param2 = command.Parameters.Add(":id2",
                            OracleDbType.NVarchar2);
                        param2.Value = strID;
                    }
                    else
                    {
                        strCommand = " DELETE FROM " + this.m_strSqlDbName + "_records WHERE id = :id";
                        command.CommandText = strCommand;
                        command.BindByName = true;
                        command.CommandTimeout = m_nLongTimeout;// 30分钟

                        OracleParameter param = command.Parameters.Add(":id",
                            OracleDbType.NVarchar2);
                        param.Value = strID;
                    }

                    nExecuteCount++;
                    nDeletedCount = command.ExecuteNonQuery();
                    if (nDeletedCount != 1)
                    {
                        this.container.KernelApplication.WriteErrorLog("希望删除" + strID + " '1'条，实际删除'" + Convert.ToString(nDeletedCount) + "'个");
                    }
                    command.Parameters.Clear();
                } // end of using command

                /*
                // 调试
                if (nExecuteCount == 0)
                {
                    Debug.Assert(false, "");
                }
                 * */
            }

            // 第三步：删除对象文件
            if (this.m_lObjectStartSize != -1)
            {
                if (row_info != null && bDeleteSubrecord == false)
                {
                    string strFilename1 = this.GetObjectFileName(row_info.FileName);
                    string strFileName2 = this.GetObjectFileName(row_info.NewFileName);
                    try
                    {
                        if (string.IsNullOrEmpty(strFilename1) == false)
                        {
                            this._streamCache.FileDelete(strFilename1);
                        }
                        if (string.IsNullOrEmpty(strFileName2) == false)
                        {
                            this._streamCache.FileDelete(strFileName2);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "删除数据库 '" + this.GetCaption("zh-CN") + "' 中 ID为 '" + strID + "' 的对象文件时发生错误: " + ex.Message;
                        this.container.KernelApplication.WriteErrorLog(strError);
                        return -1;
                    }
                }
                else if (bDeleteSubrecord == true || row_info == null)
                {
                    foreach (string strShortFilename in filenames)
                    {
                        if (string.IsNullOrEmpty(strShortFilename) == true)
                            continue;

                        string strFilename = this.GetObjectFileName(strShortFilename);
                        try
                        {
                            if (string.IsNullOrEmpty(strFilename) == false)
                            {
                                this._streamCache.FileDelete(strFilename);
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "删除数据库 '" + this.GetCaption("zh-CN") + "' 中 ID为 '" + strID + "' 的对象文件时发生错误: " + ex.Message;
                            this.container.KernelApplication.WriteErrorLog(strError);
                            return -1;
                        }
                    }
                }
            }

            // 2018/7/21
            // 第四步，清除 PDF 页面文件缓存
            {
                // strID 为 10 字符，或者 0000000000_0000 形态
                string record_path = this.GetCacheRecPath(strID);
                _pageCache.ClearByRecPath(record_path,
                    (filename) =>
                    {
                        this._streamCache.FileDelete(filename);
                    });
            }

            return nDeletedCount;
        }

        string GetCacheRecPath(string strID)
        {
            Debug.Assert(strID.Length >= 10, "");
            return this.FullID + "/" + strID;
        }

        Connection GetConnection(
            string strConnectionString,
            ConnectionStyle style = ConnectionStyle.None)
        {
            // SQLite 专用, 快速的， 全局共用的
            if (((style & ConnectionStyle.Global) == ConnectionStyle.Global)
                && this.SQLiteInfo != null) // && this.SQLiteInfo.FastMode == true
            {
                Debug.Assert(this.SQLiteInfo != null, "");

                lock (this.SQLiteInfo)
                {
                    if (this.SQLiteInfo.m_connection == null)
                    {
                        this.SQLiteInfo.m_connection = new Connection(this,
                            strConnectionString,
                            style);
                        return this.SQLiteInfo.m_connection;
                    }

                    return this.SQLiteInfo.m_connection;
                }
            }

            return new Connection(this,
                            strConnectionString,
                            style);
        }
    }


    public class SQLiteInfo
    {
        public bool FastMode = false;    // 是否为快速模式
        internal Connection m_connection = null;
    }

    // flag
    public enum ConnectionStyle
    {
        None = 0,
        Global = 0x01,
    }
}
