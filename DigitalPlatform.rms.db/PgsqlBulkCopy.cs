using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;

namespace DigitalPlatform.rms
{
    public class PgsqlBulkCopy
    {
        //
        // 摘要:
        //     服务器上目标表的名称。
        //
        // 返回结果:
        //     System.Data.SqlClient.SqlBulkCopy.DestinationTableName 属性的字符串值；或者如果未提供任何值，则为
        //     null。
        public string DestinationTableName { get; set; }

        // 摘要:
        //     每一批次中的行数。在每一批次结束时，将该批次中的行发送到服务器。
        //
        // 返回结果:
        //     System.Data.SqlClient.SqlBulkCopy.BatchSize 属性的整数值；或者如果未设置任何值，则为零。
        public int BatchSize { get; set; }

        int _bulkCopyTimeout = 30;
        //
        // 摘要:
        //     超时之前操作完成所允许的秒数。
        //
        // 返回结果:
        //     System.Data.SqlClient.SqlBulkCopy.BulkCopyTimeout 属性的整数值。
        public int BulkCopyTimeout
        {
            get
            {
                return _bulkCopyTimeout;
            }
            set
            {
                _bulkCopyTimeout = value;
            }
        }

        NpgsqlConnection m_connection = null;

        public PgsqlBulkCopy(NpgsqlConnection connection)
        {
            this.m_connection = connection;
        }

        // https://stackoverflow.com/questions/65687071/bulk-insert-copy-ienumerable-into-table-with-npgsql
        public void WriteToServer(DataTable dataTable)
        {
            try
            {
                if (DestinationTableName == null || DestinationTableName == "")
                {
                    throw new ArgumentOutOfRangeException("DestinationTableName", "Destination table must be set");
                }
                int colCount = dataTable.Columns.Count;

                NpgsqlDbType[] types = new NpgsqlDbType[colCount];
                int[] lengths = new int[colCount];
                string[] fieldNames = new string[colCount];

                using (var cmd = new NpgsqlCommand("SELECT * FROM " + DestinationTableName + " LIMIT 1", m_connection))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.FieldCount != colCount)
                        {
                            throw new ArgumentOutOfRangeException("dataTable", "Column count in Destination Table does not match column count in source table.");
                        }
                        var columns = rdr.GetColumnSchema();
                        for (int i = 0; i < colCount; i++)
                        {
                            types[i] = (NpgsqlDbType)columns[i].NpgsqlDbType;
                            lengths[i] = columns[i].ColumnSize == null ? 0 : (int)columns[i].ColumnSize;
                            fieldNames[i] = columns[i].ColumnName;
                        }
                    }

                }
                var sB = new StringBuilder(fieldNames[0]);
                for (int p = 1; p < colCount; p++)
                {
                    sB.Append(", " + fieldNames[p]);
                }
                using (var writer = m_connection.BeginBinaryImport("COPY " + DestinationTableName + " (" + sB.ToString() + ") FROM STDIN (FORMAT BINARY)"))
                {
                    for (int j = 0; j < dataTable.Rows.Count; j++)
                    {
                        DataRow dR = dataTable.Rows[j];
                        writer.StartRow();

                        for (int i = 0; i < colCount; i++)
                        {
                            if (dR[i] == DBNull.Value)
                            {
                                writer.WriteNull();
                            }
                            else
                            {
                                switch (types[i])
                                {
                                    case NpgsqlDbType.Bigint:
                                        writer.Write((long)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Bit:
                                        if (lengths[i] > 1)
                                        {
                                            writer.Write((byte[])dR[i], types[i]);
                                        }
                                        else
                                        {
                                            writer.Write((byte)dR[i], types[i]);
                                        }
                                        break;
                                    case NpgsqlDbType.Boolean:
                                        writer.Write((bool)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Bytea:
                                        writer.Write((byte[])dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Char:
                                        if (dR[i] is string)
                                        {
                                            writer.Write((string)dR[i], types[i]);
                                        }
                                        else if (dR[i] is Guid)
                                        {
                                            var value = dR[i].ToString();
                                            writer.Write(value, types[i]);
                                        }


                                        else if (lengths[i] > 1)
                                        {
                                            writer.Write((char[])dR[i], types[i]);
                                        }
                                        else
                                        {

                                            var s = ((string)dR[i].ToString()).ToCharArray();
                                            writer.Write(s[0], types[i]);
                                        }
                                        break;
                                    case NpgsqlDbType.Time:
                                    case NpgsqlDbType.Timestamp:
                                    case NpgsqlDbType.TimestampTz:
                                    case NpgsqlDbType.Date:
                                        writer.Write((DateTime)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Double:
                                        writer.Write((double)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Integer:
                                        try
                                        {
                                            if (dR[i] is int)
                                            {
                                                writer.Write((int)dR[i], types[i]);
                                                break;
                                            }
                                            else if (dR[i] is string)
                                            {
                                                var swap = Convert.ToInt32(dR[i]);
                                                writer.Write((int)swap, types[i]);
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            string sh = ex.Message;
                                        }

                                        writer.Write((object)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Interval:
                                        writer.Write((TimeSpan)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Numeric:
                                    case NpgsqlDbType.Money:
                                        writer.Write((decimal)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Real:
                                        writer.Write((Single)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Smallint:

                                        try
                                        {
                                            if (dR[i] is byte)
                                            {
                                                var swap = Convert.ToInt16(dR[i]);
                                                writer.Write((short)swap, types[i]);
                                                break;
                                            }
                                            writer.Write((short)dR[i], types[i]);
                                        }
                                        catch (Exception ex)
                                        {
                                            string ms = ex.Message;
                                        }

                                        break;
                                    case NpgsqlDbType.Varchar:
                                    case NpgsqlDbType.Text:
                                        writer.Write((string)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Uuid:
                                        writer.Write((Guid)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Xml:
                                        writer.Write((string)dR[i], types[i]);
                                        break;
                                }
                            }
                        }
                    }
                    writer.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing NpgSqlBulkCopy.WriteToServer().  See inner exception for details", ex);
            }
        }


        public void WriteToServer(IDataReader reader)
        {
            if (DestinationTableName == null || DestinationTableName == "")
            {
                throw new ArgumentOutOfRangeException("DestinationTableName", "Destination table must be set");
            }
            int colCount = reader.FieldCount;

            Type[] types = new Type[colCount];
            int[] lengths = new int[colCount];
            string[] fieldNames = new string[colCount];

            using (var cmd = new NpgsqlCommand("SELECT * FROM " + DestinationTableName + " LIMIT 1", m_connection))
            {
                var table = reader.GetSchemaTable();
                {
                    if (reader.FieldCount != colCount)
                    {
                        throw new ArgumentOutOfRangeException("dataTable", "Column count in Destination Table does not match column count in source table.");
                    }
                    var columns = table.Columns;
                    for (int i = 0; i < colCount; i++)
                    {
                        var column = columns[i];
                        types[i] = column.DataType;
                        // lengths[i] = columns[i].ColumnSize == null ? 0 : (int)columns[i].ColumnSize;
                        fieldNames[i] = column.ColumnName;
                    }
                }

            }
            var sB = new StringBuilder(fieldNames[0]);
            for (int p = 1; p < colCount; p++)
            {
                sB.Append(", " + fieldNames[p]);
            }

            string strCommand = "COPY " + DestinationTableName + " (" + sB.ToString() + ") FROM STDIN (FORMAT BINARY)";
            using (var writer = m_connection.BeginBinaryImport(strCommand))
            {
                writer.Timeout = TimeSpan.FromSeconds(this.BulkCopyTimeout);
                // for (int j = 0; j < dataTable.Rows.Count; j++)
                while (reader.Read())
                {
                    // DataRow dR = dataTable.Rows[j];
                    writer.StartRow();

                    for (int i = 0; i < colCount; i++)
                    {
                        var field_name = fieldNames[i];

                        var value = reader[i];
                        if (reader.IsDBNull(i))
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            var field_type = types[i].Name;
                            switch (field_type)
                            {
                                case "Int64":
                                    writer.Write(SqlDatabase.GetLong(value));
                                    break;
                                case "String":
                                    writer.Write((string)value);
                                    break;
                                case "Byte[]":
                                    writer.Write((byte[])value);
                                    break;
                                default:
                                    throw new Exception($"出现了无法处理的类型 {field_type}");
#if NO
                                    case NpgsqlDbType.Bit:
                                        if (lengths[i] > 1)
                                        {
                                            writer.Write((byte[])dR[i], types[i]);
                                        }
                                        else
                                        {
                                            writer.Write((byte)dR[i], types[i]);
                                        }
                                        break;
                                    case NpgsqlDbType.Boolean:
                                        writer.Write((bool)dR[i], types[i]);
                                        break;

                                    case NpgsqlDbType.Char:
                                        if (dR[i] is string)
                                        {
                                            writer.Write((string)dR[i], types[i]);
                                        }
                                        else if (dR[i] is Guid)
                                        {
                                            var value = dR[i].ToString();
                                            writer.Write(value, types[i]);
                                        }


                                        else if (lengths[i] > 1)
                                        {
                                            writer.Write((char[])dR[i], types[i]);
                                        }
                                        else
                                        {

                                            var s = ((string)dR[i].ToString()).ToCharArray();
                                            writer.Write(s[0], types[i]);
                                        }
                                        break;
                                    case NpgsqlDbType.Time:
                                    case NpgsqlDbType.Timestamp:
                                    case NpgsqlDbType.TimestampTz:
                                    case NpgsqlDbType.Date:
                                        writer.Write((DateTime)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Double:
                                        writer.Write((double)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Integer:
                                        try
                                        {
                                            if (dR[i] is int)
                                            {
                                                writer.Write((int)dR[i], types[i]);
                                                break;
                                            }
                                            else if (dR[i] is string)
                                            {
                                                var swap = Convert.ToInt32(dR[i]);
                                                writer.Write((int)swap, types[i]);
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            string sh = ex.Message;
                                        }

                                        writer.Write((object)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Interval:
                                        writer.Write((TimeSpan)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Numeric:
                                    case NpgsqlDbType.Money:
                                        writer.Write((decimal)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Real:
                                        writer.Write((Single)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Smallint:

                                        try
                                        {
                                            if (dR[i] is byte)
                                            {
                                                var swap = Convert.ToInt16(dR[i]);
                                                writer.Write((short)swap, types[i]);
                                                break;
                                            }
                                            writer.Write((short)dR[i], types[i]);
                                        }
                                        catch (Exception ex)
                                        {
                                            string ms = ex.Message;
                                        }

                                        break;
                                    case NpgsqlDbType.Uuid:
                                        writer.Write((Guid)dR[i], types[i]);
                                        break;
                                    case NpgsqlDbType.Xml:
                                        writer.Write((string)dR[i], types[i]);
                                        break;
#endif
                            }
                        }
                    }
                }
                writer.Complete();
            }
        }

    }
}
