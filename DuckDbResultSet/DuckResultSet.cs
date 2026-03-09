using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

using DuckDB.NET.Data;

namespace DuckDbResultSet
{
    public class DuckResultSet : IEnumerable<DuckRecord>, IDisposable
    {
        string _dbFileName = string.Empty;

        //表示结果集的名称
        protected string m_strName;

        public DateTime CreateTime = DateTime.Now;
        public DateTime LastUsedTime = DateTime.Now;

        public ResultSetType Type = ResultSetType.Id; // id/keyid/keycount

        // 是否为所有行设置好了正确的 row_id 值
        bool _sorted = false;

        // 是否已经排过序
        public bool Sorted
        {
            get { return _sorted; }
        }

        public DuckResultSet()
        {

        }

        public DuckResultSet(ResultSetType type,
            string filename)
        {
            this.Type = type;

            if (string.IsNullOrEmpty(filename) == false)
            {
                if (File.Exists(filename))
                    Attach(filename);
                else
                    SetFileName(filename);
            }
        }

        public void Touch()
        {
            this.LastUsedTime = DateTime.Now;
        }

        public void SetSorted(bool value = true)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 SetSorted()");

            this._sorted = value;
        }

        // 设置文件名。
        // 和 Attach 不同。Attach 时附加上去一个文件。而 SetFileName() 是提供一个文件名，将来会创建这个文件，也就是说当前这个物理文件还不存在
        public void SetFileName(string filename)
        {
            if (string.IsNullOrEmpty(_dbFileName) == false)
            {
                // throw new ArgumentException($"已经存在一个文件名 '{_dbFileName}'，请先 Dispose()");
                this.Clear();
            }

            if (_dbFileName == filename)
                return;
            string old_filename = Detach();
            this._dbFileName = filename;
            ClearCachedCount();
            // TODO: 删除残留的文件
        }

        // 文件是否已经创建或者挂接。
        // 当为 false 时，虽然 _dbFileName 可能已经设置了一个文件名，但这个文件物理上并不存在。等待后面适当的时机才正式创建物理文件
        internal bool _created = false;

        public void CreateEmpty(string filename)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 CreateEmpty()");

            Debug.Assert(File.Exists(filename) == false);

            _dbFileName = filename;
            // 创建数据库
            using (var context = GetContext(true))
            {
                // CreateEmpty(this.Type, context.Command);
                this._sorted = false;
                Debug.Assert(this._created == true);
            }

            ClearCachedCount();
        }

        static string BuildCreateTable(ResultSetType type, string table_name)
        {
            if (type == ResultSetType.Id)   // id, row_id
                return $"CREATE TABLE IF NOT EXISTS {table_name} (id varchar collate \"C\" NULL, row_id long NULL);";
            else if (type == ResultSetType.KeyId)   // id, key, row_id
                return $"CREATE TABLE IF NOT EXISTS {table_name} (id varchar collate \"C\" NULL, key varchar collate \"C\" NULL, row_id long NULL);";
            else if (type == ResultSetType.KeyCount)    // key, count, row_id
                return $"CREATE TABLE IF NOT EXISTS {table_name} (key varchar collate \"C\" NULL, count long NULL, row_id long NULL);";
            else
                throw new ArgumentException($"不合法的 type 值 '{type}'");
        }

        static string BuildCreateIndex(ResultSetType type, string table_name)
        {
            string command = "";
            if (type == ResultSetType.Id)
                command = $"CREATE INDEX IF NOT EXISTS id_idx ON {table_name} (id);";
            else if (type == ResultSetType.KeyId)
                command = $"CREATE INDEX IF NOT EXISTS keyid_idx ON {table_name} (key, id);";
            else if (type == ResultSetType.KeyCount)
                command = $"CREATE INDEX IF NOT EXISTS key_idx ON {table_name} (key);";
            else
                throw new ArgumentException($"不合法的 type 值 '{type}'");

            command += $"CREATE INDEX IF NOT EXISTS rowid_idx ON {table_name} (row_id);";
            return command;
        }


        // 创建一个空的库
        public static void CreateEmpty(ResultSetType type, DuckDBCommand Command)
        {
            var command = BuildCreateTable(type, "records");
            // command += BuildCreateIndex(type, "records");

            Command.CommandText = command;

            var ret = Command.ExecuteNonQuery();
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);

#if NO
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // release managed resources if any
                }

                // release unmanaged resource
                this.Close();

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            disposed = true;
        }

        public string Name
        {
            get
            {
                return m_strName;
            }
        }

        // 挂接一个物理文件
        public void Attach(string filename)
        {
            Debug.Assert(this._created == false);
            if (File.Exists(filename) == false)
                throw new FileNotFoundException($"文件 {filename} 不存在");
            _dbFileName = filename;
            this._created = true;
            ClearCachedCount();
        }

        // 解挂，得到一个脱离关系以后的物理文件
        public string Detach()
        {
            var filename = _dbFileName;
            this._dbFileName = null;
            this._created = false;
            ClearCachedCount();
            return filename;
        }

        public void Clear()
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 Clear()");

            var filename = Detach();
            if (string.IsNullOrEmpty(filename) == false)
            {
                try
                {
                    // 可能抛出异常
                    File.Delete(filename);
                    // CreateEmpty(filename);
                }
                catch (FileNotFoundException)
                {

                }
            }

            // 恢复原有文件名。以便必要时可以追加进入内容
            this._dbFileName = filename;
            this._cacheRecords?.Clear();
        }

        public void Clear(ResultSetType type)
        {
            this.Clear();
            this.Type = type;
        }

#if REMOVED
        public void Clear()
        {
            using (var duckDBConnection = new DuckDBConnection($"Data Source={_dbFileName}"))
            {
                duckDBConnection.Open();

                using (var command = duckDBConnection.CreateCommand())
                {
                    command.CommandText = "SELECT id, data FROM records";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new DuckRecord(reader[0] as string, reader[1] as string);
                        }
                    }
                }
            }
        }

#endif

        // 缓存的 .Count 属性值。当为 -1 时表示没有缓存
        long _cached_count = -1;

        internal void ClearCachedCount()
        {
            _cached_count = -1;
        }


        // TODO: 要进行缓冲，避免每次都调用 SQL 拖慢速度

        public long Count
        {
            get
            {
                if (this._cacheRecords.Count > 0)
                    return this._cacheRecords.Count;
                if (string.IsNullOrEmpty(_dbFileName))
                    return 0;
                if (this._created == false)
                    return 0;

                if (_cached_count != -1)
                    return _cached_count;
                using (var context = GetContext(true))
                {
                    _cached_count = context.GetCount();
                    return _cached_count;
                }
            }
        }

        // 克隆出一个新对象
        // parameters:
        //      strStyle    file/handle  分别是 复制物理文件/沿用物理文件、仅重新打开文件
        public DuckResultSet Clone(string strStyle = "file")
        {
            throw new NotImplementedException();
            return null;
        }

        bool _readOnly = false;
        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                if (_readOnly != value)
                {
                    if (value == true)
                        FlushCache();
                    _readOnly = value;
                }
            }
        }

        public void Close()
        {
#if NO // testing
            if (this.ReadOnly)
                throw new Exception("readonly close triggered");
#endif
            // 只读的结果集，Close 时不删除物理文件，而是仅仅解挂。这样可以让其他结果集继续使用这个物理文件
            if (this.ReadOnly)
            {
                this.Detach();
                return;
            }

            if (string.IsNullOrEmpty(this._dbFileName) == false)
            {
                try
                {
                    File.Delete(_dbFileName);
                }
                catch
                {
                }
                _dbFileName = null;
            }
        }

        const int IDLE_COUNT = 10000;

        public void RemoveDup()
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public DuckRecord this[long nIndex]
        {
            get
            {
                return GetRecord(nIndex);
            }
        }

        public DuckRecord GetRecord(long index)
        {
            foreach (var resultset in this.GetRange(index, 1))
            {
                return resultset;
            }

            return null;
        }

        // 得到第一个记录,起到定位作用
        // return:
        //		null	文件结束
        //		其他	记录对象
        public DuckRecord GetFirstRecord(long nIndex,
            bool bContainDeleted,
            out long lPos)
        {
            lPos = 0;
            foreach (var resultset in this.GetRange(0, 1))
            {
                return resultset;
            }

            return null;
        }

        // 顺次得到下一条记录.第一次使用本函数之前必须有一次GetFirstRecord()调用
        // return:
        //		null	文件结束
        //		其他	记录对象
        // lPos当有小文件时，表示的是索引号，当无小文件时，为大文件的偏移量
        public DuckRecord GetNextRecord(
            ref long lPos)
        {
            lPos++;
            foreach (var resultset in this.GetRange(lPos, 1))
            {
                return resultset;
            }

            return null;
        }

        /*
        public virtual void Insert(int nIndex,
    DpRecord record)
        {

        }
        */


        public BatchContext GetContext(bool ensure_created = false)
        {
#if REMOVED
            if (string.IsNullOrEmpty(_dbFileName))
                throw new ArgumentException("尚未设置 _dbFileName");

            var connection = new DuckDBConnection($"Data Source={_dbFileName}");
            connection.Open();
            var command = connection.CreateCommand();
            return new BatchContext(this, connection, command);
#endif
            var result = new BatchContext(this, null, false);
            if (ensure_created)
            {
                result.LazyInitialize();
            }
            Debug.Assert(result.Command != null);
            return result;
        }

        internal DuckDBConnection OpenConnection()
        {
            if (string.IsNullOrEmpty(_dbFileName))
                throw new ArgumentException("尚未设置 _dbFileName");

            string connectionString = $"Data Source={_dbFileName}";
            var connection = new DuckDBConnection(connectionString);
            connection.Open();
            return connection;
        }

        internal void CreateTable(DuckDBConnection connection,
            string table = "records")
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = BuildCreateTable(this.Type, table);
                command.ExecuteNonQuery();
                this._sorted = false;
                // if (table == "records")
                this._created = true;
                ClearCachedCount();
            }
        }

        public BatchContext GetAppenderContext(string table = "records"/*,
            bool create_table = false*/)
        {
#if REMOVED
            if (string.IsNullOrEmpty(_dbFileName))
                throw new ArgumentException("尚未设置 _dbFileName");

            var connection = new DuckDBConnection($"Data Source={_dbFileName}");
            connection.Open();
            if (create_table)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BuildCreateTable(this.Type, table);
                    command.ExecuteNonQuery();
                    this._sorted = false;
                }
            }
            var appender = connection.CreateAppender(table);
            return new BatchContext(this, connection, appender);
#endif
            return new BatchContext(this, table, true);
        }

        // 原始的，不带有缓冲功能的 AppenderContext
        public BatchContextBase GetAppenderContextBase(string table = "records",
            bool create_table = false)
        {
            if (string.IsNullOrEmpty(_dbFileName))
                throw new ArgumentException("尚未设置 _dbFileName");

            var connection = new DuckDBConnection($"Data Source={_dbFileName}");
            connection.Open();
            if (create_table)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BuildCreateTable(this.Type, table);
                    command.ExecuteNonQuery();
                    this._sorted = false;   // ??
                }
            }
            var appender = connection.CreateAppender(table);
            return new BatchContextBase(this, connection, appender);
        }


        int CreateTable(string table)
        {
            using (var context = GetContext())
            {
                context.Command.CommandText = BuildCreateTable(this.Type, table);
                return context.Command.ExecuteNonQuery();
            }
        }

#if REMOVED
        void AppendBatch()
        {
            using (var duckDBConnection = new DuckDBConnection("Data Source=file.db"))
            {
                duckDBConnection.Open();

                using var command = duckDBConnection.CreateCommand();

                command.CommandText = "CREATE TABLE integers(foo INTEGER, bar INTEGER);";
                var executeNonQuery = command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO integers VALUES (3, 4), (5, 6), (7, 8);";
                executeNonQuery = command.ExecuteNonQuery();

                command.CommandText = "Select count(*) from integers";
                var executeScalar = command.ExecuteScalar();

                command.CommandText = "SELECT foo, bar FROM integers";
                var reader = command.ExecuteReader();

                PrintQueryResults(reader);
            }
        }
#endif
        public void Sort(bool output_row_id = false,
            CancellationToken token = default)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许进行排序");

            if (this.Cached)
            {
                if (this.CachedCount <= 1)
                {
                    this._sorted = true;
                    return;
                }
                FlushCache();
            }

            string command = $"DROP TABLE IF EXISTS records1;";
            command += $"CREATE TABLE records1 AS select {BuildColumns(output_row_id)}, row_number() over ({GetOrderBy()}) as row_id from records;";
            command += "DROP TABLE records;";
            command += "ALTER TABLE records1 RENAME TO records;";
            command += BuildCreateIndex(this.Type, "records");

            using (var context = GetContext(true))
            using (var reg = token.Register(() =>
            {
                context.Command.Cancel();
                throw new OperationCanceledException();
            }))
            {
                context.Command.CommandText = command;
                context.Command.ExecuteNonQuery();
            }

            this._sorted = true;

            // Sort() 之后，不改变 .Count 值
        }

        public static string EscapeString(string text)
        {
            // https://duckdb.org/docs/sql/functions/char.html#chrx
            StringBuilder result = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            foreach (var ch in text)
            {
                if (ch <= 0x0d || ch == '\'')
                {
                    if (temp.Length > 0)
                    {
                        if (result.Length > 0)
                            result.Append(" || ");
                        result.Append($"'{temp.ToString()}'");
                        temp.Clear();
                    }
                    if (result.Length > 0)
                        result.Append(" || ");
                    result.Append($"chr({((int)ch).ToString()})");
                }
                else
                    temp.Append(ch);
            }

            if (temp.Length > 0)
            {
                if (result.Length > 0)
                    result.Append(" || ");
                result.Append($"'{temp.ToString()}'");
                temp.Clear();
            }

            return result.ToString();
        }

        const int CHECK_PER_TIME = 1000;

        // 将 resultset 中的 records 表导入当前数据库中名为 records1
        void Import(DuckResultSet resultset, CancellationToken token = default)
        {
            Debug.Assert(this._created == true);

            /*
            using (var duckDBConnection = new DuckDBConnection($"Data Source={_dbFileName}"))
            {
                duckDBConnection.Open();
                using (var command = duckDBConnection.CreateCommand())
                {
                    command.CommandText = BuildCreateTable(this.Type, "records1");
                    command.ExecuteNonQuery();
                }
            }
            */

            // 先将 resultset 中的 records 表导入当前数据库中名为 records1
            // 注意 Appender 导入，Context 需要单独关闭一次，不然 records1 表中的记录数为 0
            using (var context0 = this.GetAppenderContextBase("records1", true))
            {
                if (this.Type == ResultSetType.Id)
                {
                    long i = 0;
                    foreach (var record in resultset)
                    {
                        if ((i % CHECK_PER_TIME) == 0)
                            token.ThrowIfCancellationRequested();

                        context0.AppendIdRow(record.ID, i + 1);
                        i++;
                    }
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    long i = 0;
                    foreach (var record in resultset)
                    {
                        if ((i % CHECK_PER_TIME) == 0)
                            token.ThrowIfCancellationRequested();

                        context0.AppendKeyIdRow(record.ID, record.Key, i + 1);
                        i++;
                    }
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    long i = 0;
                    foreach (var record in resultset)
                    {
                        if ((i % CHECK_PER_TIME) == 0)
                            token.ThrowIfCancellationRequested();

                        context0.AppendKeyCountRow(record.Key, record.Count, i + 1);
                        i++;
                    }
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");
            }

            Debug.Assert(this._created == true);
        }

#if OLD
        // records 表和另一个结果集中的 records 表 OR 运算，结果存储到 result_table_name 表
        // parameters:
        //      and_link_string Key 之间的连接字符串，表达 OR 关系
        public void OR(DuckResultSet resultset,
            string result_table_name = "records",
            string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 OR()");

            // 优化
            if (resultset.Cached && resultset.CachedCount == 0)
                return;

            if (this.Cached)
            {
                FlushCache();
            }

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset, token);

            using (var context0 = this.GetContext(true))
            {
                // 把 records 和 records1 归并到 records2
                string command = "DROP TABLE IF EXISTS records2;";
                if (this.Type == ResultSetType.Id)
                {
                    command += @"CREATE TABLE records2 AS 
(select id from records union select id from records1);";
                    // command = @"(select id from records union ALL select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);

                    command += $"CREATE TABLE records2 AS (select id0 as id, string_agg(key0, {or_sep} ORDER BY id0) as key from (select id as id0, key as key0 from records union all select id as id0, key as key0 from records1) group by id0);";
                }
                else if (this.Type == ResultSetType.KeyCount)
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records union all select key as key0, count as count0 from records1) group by key0);";
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name};";
                // command += BuildCreateIndex(this.Type, result_table_name);  // 创建索引。这样 row_id 速度会加快

                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    context0.Command.CommandText = command;
                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                ClearCachedCount();
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }
#endif

        public void OR(DuckResultSet resultset,
            DuckResultSet target = null,
    string result_table_name = "records",
    string or_link_string = ", ",
    CancellationToken token = default)
        {
            if (this.ReadOnly && target == null)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 OR()");

            // 优化
            if (resultset.Cached && resultset.CachedCount == 0)
                return;

            // 当 this 没有记录，resultset 有 capacity 以内的记录时，可以直接把 resultset 的记录放入 this 的缓存中，避免后面进行 SQL 归并的开销
            if (target == null
                && this.Cached && this.Count == 0
                && resultset.Cached && resultset.Count <= this._capacity)
            {
                this._cacheRecords.AddRange(resultset._cacheRecords);
                resultset._cacheRecords.Clear();
                return;
            }

            if (this.Cached)
            {
                FlushCache();
            }

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            if (target != null)
            {
                target.FlushCache(true);
            }
            /*
            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset, token);
            */

            var filename = resultset._dbFileName;

            using (var context0 = this.GetContext(true))
            {
                if (result_table_name.Contains("."))
                    throw new ArgumentException($"result_table_name 参数值 '{result_table_name}' 不合法，不能包含西文点符号");
                StringBuilder command = new StringBuilder();

                string records2 = target != null ? "t.records2" : "records2";

                if (target != null)
                {
                    command.Append($"ATTACH '{target._dbFileName}' AS t;");
                }
                command.Append($"ATTACH '{filename}' AS db1;");
                command.Append($"DROP TABLE IF EXISTS {records2};");

                // 把 records 和 records1 归并到 records2
                // string command = $"DROP TABLE IF EXISTS records2; ATTACH '{filename}' AS db1;";
                if (this.Type == ResultSetType.Id)
                {
                    command.Append($"CREATE TABLE {records2} AS ");
                    command.Append("(select id from records union select id from db1.records);");
                    // command = @"(select id from records union ALL select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);

                    command.Append($"CREATE TABLE {records2} AS (select id0 as id, string_agg(key0, {or_sep} ORDER BY id0) as key from (select id as id0, key as key0 from records union all select id as id0, key as key0 from db1.records) group by id0);");
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    command.Append($"CREATE TABLE {records2} AS ");
                    command.Append("(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records union all select key as key0, count as count0 from db1.records) group by key0);");
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");


                command.Append($" DROP TABLE IF EXISTS {(target != null ? "t." + result_table_name : result_table_name)};");
                command.Append($" ALTER TABLE {records2} RENAME TO {result_table_name}; DETACH db1;");
                if (target != null)
                {
                    command.Append($" DETACH t;");
                }
                // command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name}; DETACH db1;";

                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    context0.Command.CommandText = command.ToString();
                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                if (target == null)
                    ClearCachedCount();
                else
                    target._created = true;
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }

#if OLD
        // parameters:
        //      and_link_string Key 之间的连接字符串，表达 AND 关系
        public void AND(DuckResultSet resultset,
            string result_table_name = "records",
            string and_link_string = " & ",
            string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 AND()");

            // 优化
            if (resultset.Cached && resultset.CachedCount == 0)
                return;

            if (this.Cached)
            {
                FlushCache();
            }
            Debug.Assert(this._created == true);

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset);

            using (var context0 = this.GetContext(true))
            {
                Debug.Assert(this._created == true);
                Debug.Assert(context0.Command != null);

                // 把 records 和 records1 归并到 records2
                string command = "DROP TABLE IF EXISTS records2;";
                if (this.Type == ResultSetType.Id)
                {
                    command += @"CREATE TABLE records2 AS 
(select id from records intersect select id from records1);";
                    // command = @"(select id from records union ALL select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);
                    var and_sep = EscapeString(and_link_string);
                    /*
                    drop table if exists temp; 
                    create table temp as select id from t1 intersect select id from t2; 
                    select id0 as id, string_agg(key0, ', ' ORDER BY id0) as key from (select id as id0, key || '|t1' as key0 from t1 where id in (select id from temp) union all select id as id0, key || '|t2' as key0 from t2 where id in ((select id from temp))) as tbl group by id0;                     
                    * */
                    command += "DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select id from records intersect select id from records1;";
                    // 确保 records 里面 key 先根据 id 进行 OR 合并
                    var r1 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id)";
                    var r2 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records1 group by id)";
                    command += $"CREATE TABLE records2 AS (select id0 as id, string_agg(key0, {and_sep} ORDER BY id0) as key from (select id as id0, key as key0 from {r1} where id in (select id from temp) union all select id as id0, key as key0 from {r2} where id in (select id from temp)) group by id0);";
                    command += "DROP TABLE temp;";
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    command += "DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select key from records intersect select key from records1;";
                    command += $"CREATE TABLE records2 AS (select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records where key in (select key from temp) union all select key as key0, count as count0 from records1 where key in (select key from temp)) group by key0);";
                    command += "DROP TABLE temp;";

                    /*
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records intersect select key as key0, count as count0 from records1) group by key0);";
                    */
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name};";

                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    context0.Command.CommandText = command;

                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                ClearCachedCount();
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }
#endif

        // parameters:
        //      target  目标结果集对象。用于存储结果。如果为 null，表示直接在 this 上进行修改
        //      and_link_string Key 之间的连接字符串，表达 AND 关系
        public void AND(DuckResultSet resultset,
            DuckResultSet target = null,
            string result_table_name = "records",
            string and_link_string = " & ",
            string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly && target == null)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 AND()");

            // 优化
            if (resultset.Cached && resultset.CachedCount == 0)
            {
                this.Clear();
                return;
            }

            if (this.Cached)
            {
                FlushCache();
            }
            Debug.Assert(this._created == true);

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            if (target != null)
            {
                target.FlushCache(true);
            }

            /*
            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset);
            */
            var filename = resultset._dbFileName;

            using (var context0 = this.GetContext(true))
            {
                Debug.Assert(this._created == true);
                Debug.Assert(context0.Command != null);

                if (result_table_name.Contains("."))
                    throw new ArgumentException($"result_table_name 参数值 '{result_table_name}' 不合法，不能包含西文点符号");

                StringBuilder command = new StringBuilder();

                string records2 = target != null ? "t.records2" : "records2";

                if (target != null)
                {
                    command.Append($"ATTACH '{target._dbFileName}' AS t;");
                }
                command.Append($"ATTACH '{filename}' AS db1;");
                command.Append($"DROP TABLE IF EXISTS {records2};");

                // 把 records 和 records1 归并到 records2
                // command.Append($"DROP TABLE IF EXISTS {records2}; ATTACH '{filename}' AS db1;");

                if (this.Type == ResultSetType.Id)
                {
                    command.Append($"CREATE TABLE {records2} AS ");
                    command.Append("(select id from records intersect select id from db1.records);");
                    // command = @"(select id from records union ALL select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);
                    var and_sep = EscapeString(and_link_string);
                    /*
                    drop table if exists temp; 
                    create table temp as select id from t1 intersect select id from t2; 
                    select id0 as id, string_agg(key0, ', ' ORDER BY id0) as key from (select id as id0, key || '|t1' as key0 from t1 where id in (select id from temp) union all select id as id0, key || '|t2' as key0 from t2 where id in ((select id from temp))) as tbl group by id0;                     
                    * */
                    command.Append("DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select id from records intersect select id from db1.records;");
                    // 确保 records 里面 key 先根据 id 进行 OR 合并
                    var r1 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id)";
                    var r2 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from db1.records group by id)";
                    command.Append($"CREATE TABLE {records2} AS (select id0 as id, string_agg(key0, {and_sep} ORDER BY id0) as key from (select id as id0, key as key0 from {r1} where id in (select id from temp) union all select id as id0, key as key0 from {r2} where id in (select id from temp)) group by id0);");
                    command.Append("DROP TABLE temp;");
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    command.Append("DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select key from records intersect select key from db1.records;");
                    command.Append($"CREATE TABLE {records2} AS (select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records where key in (select key from temp) union all select key as key0, count as count0 from db1.records where key in (select key from temp)) group by key0);");
                    command.Append("DROP TABLE temp;");

                    /*
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records intersect select key as key0, count as count0 from records1) group by key0);";
                    */
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");


                command.Append($" DROP TABLE IF EXISTS {(target != null ? "t." + result_table_name : result_table_name)};");
                command.Append($" ALTER TABLE {records2} RENAME TO {result_table_name}; DETACH db1;");
                if (target != null)
                {
                    command.Append($" DETACH t;");
                }

                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    context0.Command.CommandText = command.ToString();

                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                if (target == null)
                    ClearCachedCount();
                else
                    target._created = true;

                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }

#if OLD
        // parameters:
        //      or_link_string Key 之间的连接字符串，注意这是表达 OR 连接关系
        public void SUB(DuckResultSet resultset,
            string result_table_name = "records",
            string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 SUB()");

            if (resultset.Cached && resultset.CachedCount == 0)
                return;

            if (this.Cached)
            {
                FlushCache();
            }

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset, token);

            using (var context0 = this.GetContext(true))
            {
                // 把 records 和 records1 归并到 records2
                string command = "DROP TABLE IF EXISTS records2;";
                if (this.Type == ResultSetType.Id)
                {
                    command += @"CREATE TABLE records2 AS 
(select id from records except select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);
                    /*
 select id, key from t1 where id in (select id from t1 except select id from t2);
                    * */

                    var id_set = "(select id from records intersect select id from records1)";
                    // 确保 records 里面 key 先根据 id 进行 OR 合并
                    // var r1 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id)";
                    command += $"CREATE TABLE records2 AS (select id, string_agg(key, {or_sep} ORDER BY id) as key from records where id not in {id_set} group by id);";

                    /*
                    command += $"CREATE TABLE records2 AS (select id, key from records where id in (select id from records except select id from records1));";
                    */
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    /*
                    command += "DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select key from records intersect select key from records1;";
                    command += $"CREATE TABLE records2 AS (select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records where key in (select key from temp) union all select key as key0, count as count0 from records1 where key in (select key from temp)) group by key0);";
                    command += "DROP TABLE temp;";
                    */

                    command += $"CREATE TABLE records2 AS (select key, sum(count) as count from records where key in (select key from records except select key from records1) group by key);";

                    /*
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records intersect select key as key0, count as count0 from records1) group by key0);";
                    */
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name};";

                context0.Command.CommandText = command;
                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                ClearCachedCount();
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }
#endif

        // parameters:
        //      or_link_string Key 之间的连接字符串，注意这是表达 OR 连接关系
        public void SUB(DuckResultSet resultset,
            DuckResultSet target = null,
            string result_table_name = "records",
            string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly && target == null)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 SUB()");

            if (resultset.Cached && resultset.CachedCount == 0)
                return;

            if (this.Cached)
            {
                FlushCache();
            }

            if (resultset.Cached)
            {
                resultset.FlushCache();
            }

            if (target != null)
            {
                target.FlushCache(true);
            }

            /*
            // 将 resultset 中的 records 表导入当前数据库中名为 records1
            Import(resultset, token);
            */
            var filename = resultset._dbFileName;

            using (var context0 = this.GetContext(true))
            {
                Debug.Assert(this._created == true);
                Debug.Assert(context0.Command != null);

                if (result_table_name.Contains("."))
                    throw new ArgumentException($"result_table_name 参数值 '{result_table_name}' 不合法，不能包含西文点符号");

                StringBuilder command = new StringBuilder();

                string records2 = target != null ? "t.records2" : "records2";

                if (target != null)
                {
                    command.Append($"ATTACH '{target._dbFileName}' AS t;");
                }
                command.Append($"ATTACH '{filename}' AS db1;");
                command.Append($"DROP TABLE IF EXISTS {records2};");

                // 把 records 和 records1 归并到 records2
                // string command = $"DROP TABLE IF EXISTS records2; ATTACH '{filename}' AS db1;";
                if (this.Type == ResultSetType.Id)
                {
                    command.Append($"CREATE TABLE {records2} AS ");
                    command.Append("(select id from records except select id from db1.records);");
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);
                    /*
 select id, key from t1 where id in (select id from t1 except select id from t2);
                    * */

                    var id_set = "(select id from records intersect select id from db1.records)";
                    // 确保 records 里面 key 先根据 id 进行 OR 合并
                    // var r1 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id)";
                    command.Append($"CREATE TABLE {records2} AS (select id, string_agg(key, {or_sep} ORDER BY id) as key from records where id not in {id_set} group by id);");

                    /*
                    command += $"CREATE TABLE records2 AS (select id, key from records where id in (select id from records except select id from records1));";
                    */
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    /*
                    command += "DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select key from records intersect select key from records1;";
                    command += $"CREATE TABLE records2 AS (select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records where key in (select key from temp) union all select key as key0, count as count0 from records1 where key in (select key from temp)) group by key0);";
                    command += "DROP TABLE temp;";
                    */

                    command.Append($"CREATE TABLE {records2} AS (select key, sum(count) as count from records where key in (select key from records except select key from db1.records) group by key);");

                    /*
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records intersect select key as key0, count as count0 from records1) group by key0);";
                    */
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command.Append($" DROP TABLE IF EXISTS {(target != null ? "t." + result_table_name : result_table_name)};");
                command.Append($" ALTER TABLE {records2} RENAME TO {result_table_name}; DETACH db1;");
                if (target != null)
                {
                    command.Append($" DETACH t;");
                }
                // command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name}; DETACH db1;";

                context0.Command.CommandText = command.ToString();
                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                if (target == null)
                    ClearCachedCount();
                else
                    target._created = true;
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }


        // 对集合内的事项合并去重
        // 1) 对于 Id 类型，去重
        // 2) 对于 KeyId 类型，针对相同的 Id 合并 Key
        // 3) 对于 KeyCount 类型，针对相同的 Key 合并 Count
        // 注意，如果调用前是已排序的状态，则调用后顺序会丢失，如果必要请重新排序
        public void DeDup(string or_link_string = ", ",
            CancellationToken token = default)
        {
            if (this.ReadOnly)
                throw new ArgumentException("ReadOnly 状态的结果集不允许执行 DeDup()");

            if (this.Cached)
            {
                if (this.CachedCount <= 1)
                    return;
                FlushCache();
            }

            using (var context0 = this.GetContext(true))
            {
                string command = "DROP TABLE IF EXISTS temp;";
                if (this.Type == ResultSetType.Id)
                {
                    // (select id, row_number() over (order by id) - 1 as row_id from records group by id);";
                    command += @"CREATE TABLE temp AS 
(select id from records group by id);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);

                    command += $"CREATE TABLE temp AS (select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id);";
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    command += $"CREATE TABLE temp AS (select key, sum(count) as count from records group by key);";
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command += $"DROP TABLE IF EXISTS records; ALTER TABLE temp RENAME TO records;";

                using (var reg = token.Register(() =>
                {
                    context0.Command.Cancel();
                    throw new OperationCanceledException();
                }))
                {
                    context0.Command.CommandText = command;
                    var ret = context0.Command.ExecuteNonQuery();
                    this._sorted = false;
                }

                ClearCachedCount();
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }
        }

        // TODO: 检查两个集合是否存在交叉的可能性? 注意，仅仅是比较最大最小值两端之间的范围是否部分重叠
#if REMOVED
        // 检查两个集合是否存在交叉
        public bool IsCross(DuckResultSet resultset)
        {
            Import(resultset);

            using (var context0 = this.GetContext())
            {
                string command = "DROP TABLE IF EXISTS temp;";
                if (this.Type == ResultSetType.Id)
                {
                    command += @"select count(*) from (select id from records intersect select id from records1);";
                }
                else if (this.Type == ResultSetType.KeyId)
                {
                    var or_sep = EscapeString(or_link_string);
                    /*
 select id, key from t1 where id in (select id from t1 except select id from t2);
                    * */

                    var id_set = "(select id from records intersect select id from records1)";
                    // 确保 records 里面 key 先根据 id 进行 OR 合并
                    // var r1 = $"(select id, string_agg(key, {or_sep} ORDER BY id) as key from records group by id)";
                    command += $"CREATE TABLE records2 AS (select id, string_agg(key, {or_sep} ORDER BY id) as key from records where id not in {id_set} group by id);";

                    /*
                    command += $"CREATE TABLE records2 AS (select id, key from records where id in (select id from records except select id from records1));";
                    */
                }
                else if (this.Type == ResultSetType.KeyCount)
                {
                    /*
                    command += "DROP TABLE IF EXISTS temp; CREATE TABLE temp AS select key from records intersect select key from records1;";
                    command += $"CREATE TABLE records2 AS (select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records where key in (select key from temp) union all select key as key0, count as count0 from records1 where key in (select key from temp)) group by key0);";
                    command += "DROP TABLE temp;";
                    */

                    command += $"CREATE TABLE records2 AS (select key, sum(count) as count from records where key in (select key from records except select key from records1) group by key);";

                    /*
                    command += @"CREATE TABLE records2 AS 
(select key0 as key, sum(count0) as count from (select key as key0, count as count0 from records intersect select key as key0, count as count0 from records1) group by key0);";
                    */
                }
                else
                    throw new ArgumentException($"未知的结果集类型 {this.Type}");

                command += $" DROP TABLE IF EXISTS {result_table_name}; ALTER TABLE records2 RENAME TO {result_table_name};";

                context0.Command.CommandText = command;
                var ret = context0.Command.ExecuteNonQuery();
                /*
                using (var reader = c.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                    }
                }
                */
            }

        }
#endif
        // TODO: 单元测试 cached 情况
        // 获得一个枚举器，用于遍历指定开始位置、指定长度的元素
        public IEnumerable<DuckRecord> GetRange(
            long start,
            long max_count,
            bool output_row_id = false)
        {
            if (this.Cached)
            {
                _cacheRecords.Skip((int)start)
                    .TakeWhile(_ => max_count == -1 || max_count-- > 0);
                foreach (var record in _cacheRecords)
                {
                    yield return record;
                }
                yield break;
            }

            /*
            string command = $"SELECT id, key FROM records ";
            if (max_count != -1)
                command += $"LIMIT {max_count}";
            if (start != 0)
                command += $"OFFSET {start}";
            */

            /*
            string command = $"SELECT id, key FROM records where row_id > {start} ";
            if (max_count != -1)
                command += $"and row_id < {start + 1 + max_count}";
            command += ";";
            */
            if (output_row_id && this._sorted == false)
                throw new ArgumentException("_sorted == false 时无法使用 output_row_id 特性");

            string command = BuildEnumeratorCommand(start,
                max_count,
                output_row_id);

            using (var context = GetContext(true))
            {
                context.Command.CommandText = command;
                using (var reader = context.Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return BuildDuckRecord(reader, output_row_id);
                    }
                }
            }
        }

        string BuildColumns(bool output_row_id = false)
        {
            if (this.Type == ResultSetType.Id)
                return "id" + (output_row_id ? ", row_id" : "");
            else if (this.Type == ResultSetType.KeyId)
                return "id, key" + (output_row_id ? ", row_id" : "");
            else if (this.Type == ResultSetType.KeyCount)
                return "key, count" + (output_row_id ? ", row_id" : "");
            else
                throw new ArgumentException($"未知的 this.Type 值 {this.Type}");
        }

        string BuildLimit(long start, long max_count)
        {
            string command = "";
            if (max_count != -1)
                command += $"LIMIT {max_count}";
            if (start != 0)
                command += $"OFFSET {start}";
            return command;
        }

#if REMOVED
        string BuildEnumeratorCommand(long start, long max_count,
    bool output_row_id = false)
        {
            string command = "";
            if (_sorted)
            {
                string desc = this.Desc ? " DESC" : "";
                command = $"SELECT {BuildColumns(output_row_id)} FROM records ";

                command += $" where rowid >= {start}";
                if (max_count != -1)
                    command += $" and rowid < {start + max_count}";
                command += $" order by rowid{desc};";
            }
            else
            {
                command = $"SELECT {BuildColumns(output_row_id)} FROM records {GetOrderBy()} {BuildLimit(start, max_count)};";
            }

            return command;
        }
#endif

        string BuildEnumeratorCommand(long start, long max_count,
            bool output_row_id = false)
        {
            string command = "";
            if (_sorted)
            {
                string desc = this.Desc ? " DESC" : "";
                command = $"SELECT {BuildColumns(output_row_id)} FROM records ";

                command += $" where row_id > {start}";
                if (max_count != -1)
                    command += $" and row_id < {start + 1 + max_count}";
                command += $" order by row_id{desc};";
            }
            else
            {
                command = $"SELECT {BuildColumns(output_row_id)} FROM records {GetOrderBy()} {BuildLimit(start, max_count)};";
            }

            return command;
        }


        IEnumerator<DuckRecord> IEnumerable<DuckRecord>.GetEnumerator()
        {
            if (this.Cached)
            {
                foreach (var record in _cacheRecords)
                {
                    yield return record;
                }
                yield break;
            }

            using (var context = GetContext(true))
            {
                context.Command.CommandText = BuildEnumeratorCommand();
                using (var reader = context.Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return BuildDuckRecord(reader);
                    }
                }
            }
        }

        string BuildEnumeratorCommand(bool output_row_id = false)
        {
            string command = "";
            if (_sorted)
            {
                string desc = this.Desc ? " DESC" : "";
                command = $"SELECT {BuildColumns(output_row_id)} FROM records order by row_id{desc};";
            }
            else
            {
                command = $"SELECT {BuildColumns(output_row_id)} FROM records {GetOrderBy()};";
            }

            return command;
        }

        // 从 DuckDBDataReader 中构造一个 DuckRecord 对象
        DuckRecord BuildDuckRecord(DuckDBDataReader reader,
            bool output_row_id = false)
        {
            // 疑问: row_id 列的数据类型 long，是对应于 C# 的 BigInteger 么?
            if (this.Type == ResultSetType.Id)
                return new DuckRecord(reader[0] as string,
                    output_row_id ? GetLong(1) : 0);
            else if (this.Type == ResultSetType.KeyId)
                return new DuckRecord(reader[0] as string,
                    reader[1] as string,
                    output_row_id ? GetLong(2) : 0);
            else if (this.Type == ResultSetType.KeyCount)
            {
                /*
                long v = 0;
                object value = reader[1];
                if (value is BigInteger)
                    v = Convert.ToInt64(((BigInteger)value).ToString());
                else
                    v = Convert.ToInt64(value);
                */
                long v = GetLong(1);
                return new DuckRecord(reader[0] as string,
                    // value == null ? 0 : v,
                    v,
                    output_row_id ? GetLong(2) : 0);
            }
            else
                throw new ArgumentException($"未知的 this.Type 值 {this.Type}");

            long GetLong(int col)
            {
                object value = reader[col];
                if (value == null)
                    return 0;
                if (value is BigInteger)
                    return Convert.ToInt64(((BigInteger)value).ToString());
                else
                    return Convert.ToInt64(value);
            }
        }


        bool _desc = false;

        public bool Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                if (this.ReadOnly)
                    throw new ArgumentException("ReadOnly 状态的结果集不允许修改 Desc");

                _desc = value;
            }
        }

        public int Asc
        {
            get
            {
                return this._desc ? -1 : 1;
            }
            set
            {
                if (this.ReadOnly)
                    throw new ArgumentException("ReadOnly 状态的结果集不允许修改 Asc");

                if (value == 0)
                    throw new ArgumentException("Asc 属性值不能为 0");
                _desc = value == -1;
            }
        }

        string GetOrderBy()
        {
            string asc_or_desc = _desc ? "DESC" : "ASC";
            if (this.Type == ResultSetType.Id)
                return $"order by id {asc_or_desc}";
            else if (this.Type == ResultSetType.KeyId)
                return $"order by id {asc_or_desc}, key {asc_or_desc}";
            else if (this.Type == ResultSetType.KeyCount)
                return $"order by key {asc_or_desc}, count {asc_or_desc}";
            else
                throw new ArgumentException($"未知的 this.Type 值 {this.Type}");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.Cached)
            {
                foreach (var record in _cacheRecords)
                {
                    yield return record;
                }
                yield break;
            }

            using (var context = GetContext(true))
            {
                context.Command.CommandText = BuildEnumeratorCommand();
                using (var reader = context.Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return BuildDuckRecord(reader);
                    }
                }
            }
        }

        #region 内存缓冲

        int _capacity = 1;
        List<DuckRecord> _cacheRecords = new List<DuckRecord>();

        // 当前是否处在缓存状态
        public bool Cached
        {
            get
            {
                if (this._created == false)
                    return true;
                return false;
                // return _capacity > 0 && _created == false;
            }
        }

        // 当前已经缓存的记录条数
        public int CachedCount
        {
            get
            {
                return _cacheRecords?.Count ?? 0;
            }
        }

        // 把缓存的记录推送到数据库中
        internal void FlushCache(bool ensure_created = true)
        {
            var cached = this.Cached;

            if (ensure_created)
            {
                if (this._created == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(this._dbFileName) == false, "_dbFileName 不能为空");
                    CreateEmpty(_dbFileName);
                }

                Debug.Assert(this._created == true);
            }

            // this.Cached 会在上一段执行后发生变化，所以用未变之前的 cached 值
            if (cached == false || this._cacheRecords.Count == 0)
                return;

            using (var context = this.GetAppenderContext())
            {
                foreach (var record in _cacheRecords)
                {
                    if (this.Type == ResultSetType.Id)
                        context.AppendIdRow(record.ID, record.RowID);
                    else if (this.Type == ResultSetType.KeyCount)
                        context.AppendKeyCountRow(record.Key, record.Count, record.RowID);
                    else if (this.Type == ResultSetType.KeyId)
                        context.AppendKeyIdRow(record.ID, record.Key, record.RowID);
                    else
                        throw new ArgumentException($"无法识别的结果集类型 {this.Type}");
                }
                this._cacheRecords.Clear();
                Debug.Assert(this._created == true);

                ClearCachedCount();
            }
        }

        // 写入到缓存中
        // return:
        //      false    已经达到缓存的容量上限，无法继续写入 
        //      true     成功写入
        internal bool AppendCachedIdRow(string id, long row_id)
        {
            if (this._cacheRecords.Count >= this._capacity)
                return false;
            var record = new DuckRecord(id, row_id);
            this._cacheRecords.Add(record);
            return true;
            //if (this.Type != ResultSetType.Id)
            //    throw new ArgumentException("AppendIdRow() 只允许用于 ResultSetType.Id 类型的结果集");
        }

        // 写入到缓存中
        // return:
        //      false    已经达到缓存的容量上限，无法继续写入 
        //      true     成功写入
        internal bool AppendCachedKeyIdRow(string id, string key, long row_id)
        {
            if (this._cacheRecords.Count >= this._capacity)
                return false;
            var record = new DuckRecord(id, key, row_id);
            this._cacheRecords.Add(record);
            return true;
        }

        // 写入到缓存中
        // return:
        //      false    已经达到缓存的容量上限，无法继续写入 
        //      true     成功写入
        internal bool AppendCachedKeyCountRow(string key, long count, long row_id)
        {
            if (this._cacheRecords.Count >= this._capacity)
                return false;
            var record = new DuckRecord(key, count, row_id);
            this._cacheRecords.Add(record);
            return true;
        }

        #endregion
    }

    // 结果集的存储类型
    public enum ResultSetType
    {
        Id = 0,
        KeyId = 1,
        KeyCount = 2,
    }
}
