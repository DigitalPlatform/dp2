using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using MySqlConnector;

namespace DigitalPlatform.rms
{
    // 包装多种类型的Connection
    public class Connection : IDisposable, IDbConnection
    {
        public SqlDatabase SqlDatabase = null;
        public SqlServerType SqlServerType = SqlServerType.None;

        IDbConnection _connection = null;
        bool _bGlobal = false;
        internal IDbTransaction _globalTrans = null;

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        int _nLockTimeout = 5 * 1000;

        internal int _nOpenCount = 0;
        internal int _nThreshold = 1000;

        public void Clone(Connection connection)
        {
            this.SqlDatabase = connection.SqlDatabase;
            this.SqlServerType = connection.SqlServerType;
            this._connection = connection._connection;
            this._bGlobal = connection._bGlobal;
            this._lock = connection._lock;
            this._nLockTimeout = connection._nLockTimeout;
            this._nOpenCount = connection._nOpenCount;
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

            if (this._nLockTimeout < this.SqlDatabase.m_nTimeOut)
                this._nLockTimeout = this.SqlDatabase.m_nTimeOut;

            if (this.SqlServerType == rms.SqlServerType.MsSqlServer)
                this._connection = new SqlConnection(strConnectionString);
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
                    this._bGlobal = true;
                }
                this._connection = new SQLiteConnection(strConnectionString);
            }
            else if (this.SqlServerType == rms.SqlServerType.MySql)
                this._connection = new MySqlConnection(strConnectionString);
            else if (this.SqlServerType == rms.SqlServerType.Oracle)
                this._connection = new OracleConnection(strConnectionString);
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
                if (this._bGlobal == false)
                {
                    this.SQLiteConnectionOpen();
                    return;
                }

                if (this._bGlobal == true)
                {
                    if (this._nLockTimeout < this.SqlDatabase.m_nTimeOut)
                        this._nLockTimeout = this.SqlDatabase.m_nTimeOut;

                    if (this._lock == null)
                        _lock = new ReaderWriterLockSlim();

                    if (this._lock != null && this._lock.TryEnterWriteLock(this._nLockTimeout) == false)
                        throw new ApplicationException("为Database全局Connection (Open) 加写锁时失败。Timeout=" + this._nLockTimeout.ToString());

                    this._nOpenCount++;
                    if (this._nOpenCount > this._nThreshold)
                    {
                        this._nOpenCount = 0;
                        this.SqlDatabase.container.ActivateCommit();
                    }

                    if (this.SQLiteConnection.State == ConnectionState.Closed)
                    {
                        this.SQLiteConnectionOpen();

                        this.DisposeTransaction();
                        Debug.Assert(this._globalTrans == null, ""); // 不要忘记了提交以前的Transaction ?

                        this._globalTrans = this.SQLiteConnection.BeginTransaction();
                    }
                    else
                    {
                        if (this._globalTrans == null)
                            this._globalTrans = this.SQLiteConnection.BeginTransaction();
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
            if (this._globalTrans != null)
            {
                this._globalTrans.Commit();
                this._globalTrans.Dispose();
                this._globalTrans = null;

                // this.m_nOpenCount = 0;
            }
        }

        void DisposeTransaction()
        {
            if (this._globalTrans != null)
            {
                this._globalTrans.Dispose();
                this._globalTrans = null;
            }
        }

        void DisposeLock()
        {
            if (_lock != null)
            {
                _lock.Dispose();
                _lock = null;
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
                    if (bAuto == false && this._bGlobal == true)
                    {
                        // 强制提交
                        if (this._lock != null && this._lock.TryEnterWriteLock(this._nLockTimeout) == false)
                            throw new ApplicationException("为Database全局Connection (Commit) 加写锁时失败。Timeout=" + this._nLockTimeout.ToString());
                        try
                        {
                            TryCommitTransaction();

                            this.SQLiteConnection?.Close();
                            this.SQLiteConnection?.Dispose();
                        }
                        finally
                        {
                            if (this._lock != null)
                                this._lock.ExitWriteLock();
                        }
                        return;
                    }

                    if (_bGlobal == true)
                    {
                        if (this._lock != null)
                            this._lock.ExitWriteLock();
                    }

                    // 不加锁的版本
                    // 不是全局的每次都要关闭
                    if (this._bGlobal == false)
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
                this.DisposeLock();
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
                if (this._bGlobal == true)
                {
                    // 强制提交
                    if (bLock == true)
                    {
                        if (this._lock != null && this._lock.TryEnterWriteLock(this._nLockTimeout) == false)
                            throw new ApplicationException("为Database全局Connection (Commit) 加写锁时失败。Timeout=" + this._nLockTimeout.ToString());
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
                            if (this._lock != null)
                                this._lock.ExitWriteLock();
                        }
                    }
                    return;
                }

                // 不加锁的版本
                // 不是全局的
                if (this._bGlobal == false)
                {
                    if (this._globalTrans != null)
                    {
                        this.TryCommitTransaction();

                        Debug.Assert(this._globalTrans == null, "");
                        this._globalTrans = this.SQLiteConnection.BeginTransaction();
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Close();
            this.DisposeTransaction();
        }

        public SqlConnection SqlConnection
        {
            get
            {
                return (SqlConnection)_connection;
            }
        }

        public SQLiteConnection SQLiteConnection
        {
            get
            {
                return (SQLiteConnection)_connection;
            }
        }

        public MySqlConnection MySqlConnection
        {
            get
            {
                return (MySqlConnection)_connection;
            }
        }

        public OracleConnection OracleConnection
        {
            get
            {
                return (OracleConnection)_connection;
            }
        }

        public bool IsMsSqlServer()
        {
            return (this.SqlServerType == SqlServerType.MsSqlServer);
        }

        public bool IsSqlite()
        {
            return (this.SqlServerType == SqlServerType.SQLite);
        }

        public bool IsMySQL()
        {
            return (this.SqlServerType == SqlServerType.MySql);
        }

        public bool IsOracle()
        {
            return (this.SqlServerType == SqlServerType.Oracle);
        }

        public DbCommand NewCommand(string command)
        {
            if (IsMsSqlServer())
                return new SqlCommand(command, this.SqlConnection);
            else if (IsSqlite())
                return new SQLiteCommand(command, this.SQLiteConnection);
            else if (IsMySQL())
                return new MySqlCommand(command, this.MySqlConnection);
            else if (IsOracle())
                return new OracleCommand(command, this.OracleConnection);
            else
                throw new Exception($"无法识别的数据库类型 '{this.SqlServerType}'");
        }

        #region 实现 IDbConnection 接口

        public IDbTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _connection.BeginTransaction(il);
        }

        public void Close()
        {
            _connection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public void Open()
        {
            _connection.Open();
        }

        public string ConnectionString { get => _connection.ConnectionString; set => _connection.ConnectionString = value; }

        public int ConnectionTimeout => _connection.ConnectionTimeout;

        public string Database => _connection.Database;

        public ConnectionState State => _connection.State;


        #endregion
    }

}
