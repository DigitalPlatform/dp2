using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace DigitalPlatform.rms
{
    // 包装多种类型的Connection
    public class Connection : IDisposable
    {
        public SqlDatabase SqlDatabase = null;
        public SqlServerType SqlServerType = SqlServerType.None;
        object m_connection = null;
        bool m_bGlobal = false;
        internal IDbTransaction m_trans = null;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        int m_nLockTimeout = 5 * 1000;

        internal int m_nOpenCount = 0;
        internal int m_nThreshold = 1000;

        public void Clone(Connection connection)
        {
            this.SqlDatabase = connection.SqlDatabase;
            this.SqlServerType = connection.SqlServerType;
            this.m_connection = connection.m_connection;
            this.m_bGlobal = connection.m_bGlobal;
            this.m_lock = connection.m_lock;
            this.m_nLockTimeout = connection.m_nLockTimeout;
            this.m_nOpenCount = connection.m_nOpenCount;
        }

        /*
        public Connection(SqlServerType server_type,
            string strConnectionString)
        {
            this.SqlServerType = server_type;
            if (server_type == rms.SqlServerType.MsSqlServer)
                this.m_connection = new SqlConnection(strConnectionString);
            else if (server_type == rms.SqlServerType.SQLite)
                this.m_connection = new SQLiteConnection(strConnectionString);
            else
            {
                throw new Exception("不支持的类型 " + server_type.ToString());
            }
        }
         * */

        public Connection(SqlDatabase database,
            string strConnectionString,
            ConnectionStyle style = ConnectionStyle.None)
        {
            this.SqlDatabase = database;
            this.SqlServerType = database.container.SqlServerType;

            if (this.m_nLockTimeout < this.SqlDatabase.m_nTimeOut)
                this.m_nLockTimeout = this.SqlDatabase.m_nTimeOut;

            if (this.SqlServerType == rms.SqlServerType.MsSqlServer)
                this.m_connection = new SqlConnection(strConnectionString);
            else if (this.SqlServerType == rms.SqlServerType.SQLite)
            {
#if NO
                // SQLite 专用, 快速的， 全局共用的
                if ((style & ConnectionStyle.Global) == ConnectionStyle.Global)
                {
                    Debug.Assert(this.SqlDatabase.SQLiteInfo != null, "");

                    lock (this.SqlDatabase.SQLiteInfo)
                    {
                        if (this.SqlDatabase.SQLiteInfo.FastMode == false)
                        {
                            this.m_connection = new SQLiteConnection(strConnectionString);
                            return;
                        }

                        if (this.SqlDatabase.SQLiteInfo.m_connection == null)
                        {
                            this.m_connection = new SQLiteConnection(strConnectionString);
                            this.m_bGlobal = true;
                            this.SqlDatabase.SQLiteInfo.m_connection = this;
                        }
                        else
                        {
                            // 复制成员
                            this.Clone(this.SqlDatabase.SQLiteInfo.m_connection);
                            if (this.m_nLockTimeout < this.SqlDatabase.m_nTimeOut)
                                this.m_nLockTimeout = this.SqlDatabase.m_nTimeOut;
                        }
                    }
                    return;
                }
#endif
                if ((style & ConnectionStyle.Global) == ConnectionStyle.Global)
                {
                    this.m_bGlobal = true;
                }
                this.m_connection = new SQLiteConnection(strConnectionString);
            }
            else if (this.SqlServerType == rms.SqlServerType.MySql)
                this.m_connection = new MySqlConnection(strConnectionString);
            else if (this.SqlServerType == rms.SqlServerType.Oracle)
                this.m_connection = new OracleConnection(strConnectionString);
            else
            {
                throw new Exception("不支持的类型 " + this.SqlServerType.ToString());
            }
        }

        void SQLiteConnectionOpen()
        {
#if REDO_OPEN
            int nRedoCount = 0;
            REDO:
            try
            {
                this.SQLiteConnection.Open();
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
            this.SQLiteConnection.Open();
#endif

        }

        public static void TryOpen(MySqlConnection connection,
            SqlDatabase database)
        {
            Exception exception = null;
            int nMax = 2;
            for (int i = 0; i < nMax; i++)
            {
                try
                {
                    connection.Open();
                }
                catch (MySqlException ex)
                {
                    // 异常记入日志
                    if (database != null
                        && database.container != null
                        && database.container.KernelApplication != null)
                        database.container.KernelApplication.WriteErrorLog("*** connection.Open() 发生异常: \r\n" + ExceptionUtil.GetDebugText(ex));

                    exception = ex;
                    if (ex.Message.StartsWith("Unable to connect to any of") // "Unable to connect to any of specified MySQL hosts."
                        && ex.InnerException is ArgumentException)
                    {
                        // 重试过程记入日志
                        if (database != null
                            && database.container != null
                            && database.container.KernelApplication != null)
                            database.container.KernelApplication.WriteErrorLog("*** 将自动重试 Open() (i=" + i + ")");

                        {
                            Thread.Sleep(500);
                            continue;
                        }
                    }
                    throw ex;
                }
                return;
            }
            if (exception != null)
                throw exception;
        }

        public void TryOpen()
        {
            Exception exception = null;
            int nMax = 2;
            for (int i = 0; i < nMax; i++)
            {
                try
                {
                    this._open();
                }
                catch (MySqlException ex)
                {
                    // 异常记入日志
                    if (this.SqlDatabase != null
                        && this.SqlDatabase.container != null
                        && this.SqlDatabase.container.KernelApplication != null)
                        this.SqlDatabase.container.KernelApplication.WriteErrorLog("*** connection.Open() 发生异常: \r\n" + ExceptionUtil.GetDebugText(ex));

                    exception = ex;
                    if (ex.Message.StartsWith("Unable to connect to any of") // "Unable to connect to any of specified MySQL hosts."
                        && ex.InnerException is ArgumentException)
                    {
                        // 重试过程记入日志
                        if (this.SqlDatabase != null
                            && this.SqlDatabase.container != null
                            && this.SqlDatabase.container.KernelApplication != null)
                            this.SqlDatabase.container.KernelApplication.WriteErrorLog("*** 将自动重试 Open() (i=" + i + ")");

                        {
                            Thread.Sleep(500);
                            continue;
                        }
                    }
                    throw ex;
                }
                return;
            }
            if (exception != null)
                throw exception;
        }

        /*public*/
        void _open()
        {
            if (this.SqlServerType == rms.SqlServerType.MsSqlServer)
                this.SqlConnection.Open();
            else if (this.SqlServerType == rms.SqlServerType.SQLite)
            {
                if (this.m_bGlobal == false)
                {
                    this.SQLiteConnectionOpen();
                    return;
                }

                if (this.m_bGlobal == true)
                {
                    if (this.m_nLockTimeout < this.SqlDatabase.m_nTimeOut)
                        this.m_nLockTimeout = this.SqlDatabase.m_nTimeOut;

                    if (this.m_lock != null && this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                        throw new ApplicationException("为Database全局Connection (Open) 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());

                    this.m_nOpenCount++;
                    if (this.m_nOpenCount > this.m_nThreshold)
                    {
                        this.m_nOpenCount = 0;
                        this.SqlDatabase.container.ActivateCommit();
                    }

                    if (this.SQLiteConnection.State == ConnectionState.Closed)
                    {
                        this.SQLiteConnectionOpen();

                        this.DisposeTransaction();
                        Debug.Assert(this.m_trans == null, ""); // 不要忘记了提交以前的Transaction ?

                        this.m_trans = this.SQLiteConnection.BeginTransaction();
                    }
                    else
                    {
                        if (this.m_trans == null)
                            this.m_trans = this.SQLiteConnection.BeginTransaction();
                    }
                }
            }
            else if (this.SqlServerType == rms.SqlServerType.MySql)
                this.MySqlConnection.Open();
            else if (this.SqlServerType == rms.SqlServerType.Oracle)
            {
                this.OracleConnection.Open();

#if NO
                int nRedoCount = 0;
            REDO_OPEN:
                try
                {
                    this.OracleConnection.Open();
                    if (this.OracleConnection.State != ConnectionState.Open)
                    {
                        if (nRedoCount <= 5)
                        {
                            nRedoCount++;
                            goto REDO_OPEN;
                        }
                        else
                        {
                            Debug.Assert(false, "");
                        }
                    }

                }
                catch (OracleException ex)
                {
                    if (ex.Errors.Count > 0 && ex.Errors[0].Number == 12520
                        && nRedoCount <= 0)
                    {
                        nRedoCount++;
                        this.OracleConnection.Close();
                        goto REDO_OPEN;
                    }

                    throw ex;
                }
#endif
            }
            else
            {
                throw new Exception("不支持的类型 " + this.SqlServerType.ToString());
            }
        }

        void TryCommitTransaction()
        {
            if (this.m_trans != null)
            {
                this.m_trans.Commit();
                this.m_trans.Dispose();
                this.m_trans = null;

                // this.m_nOpenCount = 0;
            }
        }

        void DisposeTransaction()
        {
            if (this.m_trans != null)
            {
                this.m_trans.Dispose();
                this.m_trans = null;
            }
        }

        // parameters:
        //      bAuto   是否自动关闭。 false表示强制关闭
        public void Close(bool bAuto = true)
        {
            try
            {
                if (this.SqlServerType == rms.SqlServerType.MsSqlServer)
                {
                    this.SqlConnection?.Close();
                    this.SqlConnection?.Dispose();
                }
                else if (this.SqlServerType == rms.SqlServerType.SQLite)
                {
                    // 需要加锁
                    // 只有强制关闭，全局的Connection才能真正关闭
                    if (bAuto == false && this.m_bGlobal == true)
                    {
                        // 强制提交
                        if (this.m_lock != null && this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                            throw new ApplicationException("为Database全局Connection (Commit) 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
                        try
                        {
                            TryCommitTransaction();

                            this.SQLiteConnection?.Close();
                            this.SQLiteConnection?.Dispose();
                        }
                        finally
                        {
                            if (this.m_lock != null)
                                this.m_lock.ExitWriteLock();
                        }
                        return;
                    }

                    if (m_bGlobal == true)
                    {
                        if (this.m_lock != null)
                            this.m_lock.ExitWriteLock();
                    }

                    // 不加锁的版本
                    // 不是全局的每次都要关闭
                    if (this.m_bGlobal == false)
                    {
                        this.TryCommitTransaction();

                        this.SQLiteConnection?.Close();
                        this.SQLiteConnection?.Dispose();
                    }
                }
                else if (this.SqlServerType == rms.SqlServerType.MySql)
                {
                    this.MySqlConnection?.Close();
                    this.MySqlConnection?.Dispose();
                }
                else if (this.SqlServerType == rms.SqlServerType.Oracle)
                {
                    /*
                    using (OracleCommand command = new OracleCommand("select count(*) from v$session", this.OracleConnection))
                    {
                        object result = command.ExecuteScalar();
                        Debug.WriteLine("session=" + result.ToString());
                    }
                     * */

                    this.OracleConnection?.Close();
                    this.OracleConnection?.Dispose();
                }
                else
                {
                    throw new Exception("不支持的类型 " + this.SqlServerType.ToString());
                }
            }
            finally
            {
                m_lock?.Dispose();
                this.DisposeTransaction();
            }
        }

        // parameters:
        //      bLock   是否需要加锁。2013/3/2
        public void Commit(bool bLock = true)
        {
            if (this.SqlServerType == rms.SqlServerType.SQLite)
            {
                // 需要加锁
                // 只有强制关闭，全局的Connection才能真正关闭
                if (this.m_bGlobal == true)
                {

                    // 强制提交
                    if (bLock == true)
                    {
                        if (this.m_lock != null && this.m_lock.TryEnterWriteLock(this.m_nLockTimeout) == false)
                            throw new ApplicationException("为Database全局Connection (Commit) 加写锁时失败。Timeout=" + this.m_nLockTimeout.ToString());
                    }

                    try
                    {
#if NO
                        if (this.m_trans != null)
                        {

                            this.m_trans.Commit();
                            this.m_trans = null;

                            /*
                            Debug.Assert(this.m_trans == null, "");
                            this.m_trans = this.SQLiteConnection.BeginTransaction();

                            this.m_nOpenCount = 0;
                             * */
                        }
#endif
                        this.TryCommitTransaction();
                    }
                    finally
                    {
                        if (bLock == true)
                        {
                            if (this.m_lock != null)
                                this.m_lock.ExitWriteLock();
                        }
                    }
                    return;
                }

                // 不加锁的版本
                // 不是全局的
                if (this.m_bGlobal == false)
                {
                    if (this.m_trans != null)
                    {
                        this.TryCommitTransaction();

                        Debug.Assert(this.m_trans == null, "");
                        this.m_trans = this.SQLiteConnection.BeginTransaction();
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Close();
        }

        public SqlConnection SqlConnection
        {
            get
            {
                return (SqlConnection)m_connection;
            }
        }

        public SQLiteConnection SQLiteConnection
        {
            get
            {
                return (SQLiteConnection)m_connection;
            }
        }

        public MySqlConnection MySqlConnection
        {
            get
            {
                return (MySqlConnection)m_connection;
            }
        }

        public OracleConnection OracleConnection
        {
            get
            {
                return (OracleConnection)m_connection;
            }
        }
    }

}
