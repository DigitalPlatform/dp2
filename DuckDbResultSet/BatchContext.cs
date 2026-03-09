using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

using DuckDB.NET.Data;

namespace DuckDbResultSet
{
    /// <summary>
    /// 批插入时的上下文对象
    /// </summary>
    public class BatchContext : IDisposable
    {
        // 临时缓存的记录
        // List<DuckRecord> _cachedRecords = new List<DuckRecord>();

        // public int BatchSize = 100;

        public DuckDBConnection Connection { get; set; }

        public DuckDBCommand Command { get; set; }

        public DuckDBAppender Appender { get; set; }

        public ResultSetType Type
        {
            get
            {
                return this.DuckResultSet.Type;
            }
        }

        public DuckResultSet DuckResultSet { get; set; }

        string _table = null;
        // bool _create_table = false;
        bool _appender = false;

        public BatchContext(
    DuckResultSet resultSet,
    string table,
    //bool create_table,
    bool appender)
        {
            this.DuckResultSet = resultSet;

            // 刚开始并不创建连接和命令对象，等到真正需要的时候才创建
            if (string.IsNullOrEmpty(table))
            {
                _table = "records";
            }
            else
            {
                _table = table;
            }
            //_create_table = create_table;
            _appender = appender;
        }

        //             this.Connection = new DuckDBConnection("DataSource=:memory:");
        // 延迟初始化，等到真正需要的时候才创建连接和命令对象
        internal void LazyInitialize()
        {
            if (this.Connection != null)
                return;

            Debug.Assert(this.Connection == null);
            Debug.Assert(this.Appender == null);
            Debug.Assert(this.Command == null);

            this.Connection = this.DuckResultSet.OpenConnection();
            if (this.DuckResultSet._created == false)
            {
                this.DuckResultSet.CreateTable(this.Connection, _table);
            }

            if (_appender)
            {
                this.Appender = this.Connection.CreateAppender(_table);
            }
            else
            {
                this.Command = this.Connection.CreateCommand();
            }
        }

#if REMOVED
        public BatchContext(
            DuckResultSet resultSet,
            DuckDBConnection connection,
            DuckDBCommand command)
        {
            this.DuckResultSet = resultSet;
            this.Connection = connection;
            this.Command = command;
        }

        public BatchContext(
            DuckResultSet resultSet,
            DuckDBConnection connection,
            DuckDBAppender appender)
        {
            this.DuckResultSet = resultSet;
            this.Connection = connection;
            this.Appender = appender;
        }
#endif

        public void Close()
        {
            //if (_cachedRecords.Count > 0)
            //    WriteCachedToDb();

            this.Appender?.Close();
            this.Appender?.Dispose();
            this.Appender = null;

            this.Command?.Dispose();
            this.Command = null;

            this.Connection?.Close();
            this.Connection?.Dispose();
            this.Connection = null;
        }

        public void Dispose()
        {
            this.Close();
        }

        /*
        public virtual void Add(DuckRecord record)
        {
            this._cachedRecords.Add(record);
            if (this._cachedRecords.Count >= BatchSize)
            {
                WriteCachedToDb();
            }
        }

        */

        public long GetCount()
        {
            if (this.DuckResultSet.Cached)
                return this.DuckResultSet.CachedCount;

            LazyInitialize();

            Command.CommandText = "select count(*) from records;";
            var ret = Command.ExecuteScalar();
            return (long)ret;
        }

#if REMOVED
        public List<DuckRecord> GetRange(long start, long max_count)
        {
            List<DuckRecord> results = new List<DuckRecord>();
            var command = "select id, key from records order by row_id ";
            if (max_count != -1)
                command += $"LIMIT {max_count}";
            if (start != 0)
                command += $"OFFSET {start}";
            command += ";";

            Command.CommandText = command;
            using (var reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(new DuckRecord(reader[0] as string, reader[1] as string));
                }
            }

            return results;
        }
#endif

        public void AppendIdRow(string id, long row_id)
        {
            if (this.Type != ResultSetType.Id)
                throw new ArgumentException("AppendIdRow() 只允许用于 ResultSetType.Id 类型的结果集");
 
            if (this.DuckResultSet.Cached
                && this.DuckResultSet.AppendCachedIdRow(id, row_id) == true)
                return;

            this.DuckResultSet.FlushCache();
            this.LazyInitialize();

            var row = this.Appender.CreateRow();
            row.AppendValue(id).AppendValue(row_id).EndRow();

            this.DuckResultSet.ClearCachedCount();
        }

        public void AppendKeyIdRow(string id, string key, long row_id)
        {
            if (this.Type != ResultSetType.KeyId)
                throw new ArgumentException("AppendKeyIdRow() 只允许用于 ResultSetType.KeyId 类型的结果集");

            if (this.DuckResultSet.Cached
    && this.DuckResultSet.AppendCachedKeyIdRow(id, key, row_id) == true)
                return;

            this.DuckResultSet.FlushCache();
            this.LazyInitialize();

            var row = this.Appender.CreateRow();
            row.AppendValue(id).AppendValue(key).AppendValue(row_id).EndRow();

            this.DuckResultSet.ClearCachedCount();
        }

        public void AppendKeyCountRow(string key, long count, long row_id)
        {
            if (this.Type != ResultSetType.KeyCount)
                throw new ArgumentException("AppendKeyCountRow() 只允许用于 ResultSetType.KeyCount 类型的结果集");

            if (this.DuckResultSet.Cached
&& this.DuckResultSet.AppendCachedKeyCountRow(key, count, row_id) == true)
                return;

            this.DuckResultSet.FlushCache();
            this.LazyInitialize();

            var row = this.Appender.CreateRow();
            row.AppendValue(key).AppendValue(count).AppendValue(row_id).EndRow();

            this.DuckResultSet.ClearCachedCount();
        }

#if REMOVED
        // 把缓存记录写入数据库
        int WriteCachedToDb()
        {
            if (_cachedRecords.Count > 0)
            {
                Command.Parameters.Clear();
                StringBuilder text = new StringBuilder();
                text.Append("INSERT INTO records VALUES ");
                int i = 0;
                foreach (var record in _cachedRecords)
                {
                    // text.Append($" ('{record.ID}', '{record.BrowseText}'),");
                    var name1 = $"i{i}";
                    var name2 = $"k{i}";
                    text.Append($" (${name1}, ${name2}),");
                    Command.Parameters.Add(new DuckDBParameter
                    {
                        ParameterName = name1,
                        Value = record.ID
                    });
                    Command.Parameters.Add(new DuckDBParameter
                    {
                        ParameterName = name2,
                        Value = record.BrowseText
                    });
                    i++;
                }
                text.Append(";");
                Command.CommandText = text.ToString();

                var ret = Command.ExecuteNonQuery();
                Command.Parameters.Clear();
                _cachedRecords.Clear();
                return ret;
            }
            return 0;
        }

#endif
    }
}
