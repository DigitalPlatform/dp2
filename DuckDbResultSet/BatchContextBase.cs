using DuckDB.NET.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckDbResultSet
{
    /// <summary>
    /// 批插入时的上下文对象
    /// </summary>
    public class BatchContextBase : IDisposable
    {
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

        public BatchContextBase(
            DuckResultSet resultSet,
            DuckDBConnection connection,
            DuckDBCommand command)
        {
            this.DuckResultSet = resultSet;
            this.Connection = connection;
            this.Command = command;
        }

        public BatchContextBase(
            DuckResultSet resultSet,
            DuckDBConnection connection,
            DuckDBAppender appender)
        {
            this.DuckResultSet = resultSet;
            this.Connection = connection;
            this.Appender = appender;
        }

        public void Close()
        {
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

        public long GetCount()
        {
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
            var row = this.Appender.CreateRow();
            row.AppendValue(id).AppendValue(row_id).EndRow();
        }

        public void AppendKeyIdRow(string id, string key, long row_id)
        {
            var row = this.Appender.CreateRow();
            row.AppendValue(id).AppendValue(key).AppendValue(row_id).EndRow();
        }

        public void AppendKeyCountRow(string key, long count, long row_id)
        {
            var row = this.Appender.CreateRow();
            row.AppendValue(key).AppendValue(count).AppendValue(row_id).EndRow();
        }
    }

}
