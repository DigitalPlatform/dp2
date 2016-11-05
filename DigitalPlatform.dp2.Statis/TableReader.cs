using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Collections;
using System.Data;
using System.Collections.Specialized;

namespace DigitalPlatform.dp2.Statis
{
    /// <summary>
    /// 为 Table 对象提供 DbDataReader 接口包装
    /// </summary>
    public class TableReader : DbDataReader
    {
        public Table Table = null;

        public Line CurrentLine = null;
        int _lineIndex = -1;

        public TableReader(Table table)
        {
            this.Table = table;
        }

        // 摘要:
        //     Closes the datareader, potentially closing the connection as well if CommandBehavior.CloseConnection
        //     was specified.
        public override void Close()
        {
        }


        // 摘要:
        //     Not implemented. Returns 0
        public override int Depth
        {
            get
            {
                return 0;
            }
        }

        int _fieldCount = -1;

        //
        // 摘要:
        //     Returns the number of columns in the current resultset
        public override int FieldCount
        {
            get
            {
                if (this.Table.Count == 0)
                    return 0;

#if NO
                Line line = this.Table[0];
                return line.Count + 1;    // key 也是一列
#endif
                if (_fieldCount == -1)
                    _fieldCount = this.Table.GetMaxColumnCount();

                return _fieldCount + 1;
            }
        }

        //
        // 摘要:
        //     Retrieve the count of records affected by an update/insert command. Only
        //     valid once the data reader is closed!
        public override int RecordsAffected
        {
            get
            {
                return this.Table.Count;
            }
        }

        // 摘要:
        //     Indexer to retrieve data from a column given its i
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     The value contained in the column
        public override object this[int i]
        {
            get
            {
                if (i == 0)
                    return this.CurrentLine.strKey;

                return this.CurrentLine.GetObject(i - 1);
            }
        }
        //
        // 摘要:
        //     Indexer to retrieve data from a column given its name
        //
        // 参数:
        //   name:
        //     The name of the column to retrieve data for
        //
        // 返回结果:
        //     The value contained in the column
        public override object this[string name]
        {
            get
            {
                throw new Exception("尚未实现");
            }
        }

        //
        // 摘要:
        //     Returns True if the specified column is null
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     True or False
        public override bool IsDBNull(int i)
        {
            if (this.CurrentLine == null)
                return true;
            if (i >= this.CurrentLine.Count + 1)
                return true;

            return this[i] == null;
        }


        //
        // 摘要:
        //     Returns True if the resultset has rows that can be fetched
        public override bool HasRows
        {
            get
            {
                if (this._lineIndex >= this.Table.Count - 1)
                    return true;
                return false;
            }
        }

        //
        // 摘要:
        //     Returns True if the data reader is closed
        public override bool IsClosed
        {
            get
            {
                return false;
            }
        }


        //
        // 摘要:
        //     Reads the next row from the resultset
        //
        // 返回结果:
        //     True if a new row was successfully loaded and is ready for processing
        public override bool Read()
        {
            if (this._lineIndex >= this.Table.Count - 1)
                return false;
            this._lineIndex++;
            this.CurrentLine = this.Table[this._lineIndex];
            return true;
        }

        //
        // 摘要:
        //     Retrieves the column as a boolean value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     bool
        public override bool GetBoolean(int i)
        {
            object o = this[i];
            if (o is bool)
                return (bool)o;
            throw new Exception("列 " + i.ToString() + " 不是 bool 类型");
        }

        //
        // 摘要:
        //     Retrieves the column as a single byte value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     byte
        public override byte GetByte(int i)
        {
            object o = this[i];
            if (o is byte)
                return (byte)o;
            throw new Exception("列 " + i.ToString() + " 不是 byte 类型");
        }

        //
        // 摘要:
        //     Retrieves a column as an array of bytes (blob)
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        //   fieldOffset:
        //     The zero-based index of where to begin reading the data
        //
        //   buffer:
        //     The buffer to write the bytes into
        //
        //   bufferoffset:
        //     The zero-based index of where to begin writing into the array
        //
        //   length:
        //     The number of bytes to retrieve
        //
        // 返回结果:
        //     The actual number of bytes written into the array
        //
        // 备注:
        //     To determine the number of bytes in the column, pass a null value for the
        //     buffer. The total length will be returned.
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("尚未实现");
            // return 0;
        }

        //
        // 摘要:
        //     Returns the column as a single character
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     char
        public override char GetChar(int i)
        {
            object o = this[i];
            if (o is char)
                return (char)o;
            throw new Exception("列 " + i.ToString() + " 不是 byte 类型");
        }


        //
        // 摘要:
        //     Retrieves a column as an array of chars (blob)
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        //   fieldoffset:
        //     The zero-based index of where to begin reading the data
        //
        //   buffer:
        //     The buffer to write the characters into
        //
        //   bufferoffset:
        //     The zero-based index of where to begin writing into the array
        //
        //   length:
        //     The number of bytes to retrieve
        //
        // 返回结果:
        //     The actual number of characters written into the array
        //
        // 备注:
        //     To determine the number of characters in the column, pass a null value for
        //     the buffer. The total length will be returned.
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            object o = this[i];
            if (o is string)
            {
                string strText = (string)o;
                if (buffer != null)
                    strText.CopyTo((int)fieldoffset, buffer, bufferoffset, length);
                return strText.Length;
            }
            throw new Exception("列 " + i.ToString() + " 不是 byte 类型");
        }

        //
        // 摘要:
        //     Retrieves the name of the back-end datatype of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetDataTypeName(int i)
        {
            return "";
        }

        //
        // 摘要:
        //     Retrieve the column as a date/time value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     DateTime
        public override DateTime GetDateTime(int i)
        {
            return new DateTime(0);
        }

        //
        // 摘要:
        //     Retrieve the column as a decimal value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     decimal
        public override decimal GetDecimal(int i)
        {
            object o = this[i];
            if (o is double)
                return (decimal)(double)o;
            if (o is decimal)
                return (decimal)o;
            if (o is int)
                return (decimal)o;
            if (o is long)
                return (decimal)o;
            if (o is string)
            {
                decimal v = 0;
                decimal.TryParse((string)o, out v);
                return v;
            }

            throw new Exception("列 " + i.ToString() + " 不是 decimal 类型");
        }

        //
        // 摘要:
        //     Returns the column as a double
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     double
        public override double GetDouble(int i)
        {
            object o = this[i];
            if (o is double)
                return (double)o;
            if (o is decimal)
                return (double)o;
            if (o is int)
                return (double)o;
            if (o is long)
                return (double)o;
            if (o is string)
            {
                double v = 0;
                double.TryParse((string)o, out v);
                return v;
            }

            throw new Exception("列 " + i.ToString() + " 不是 decimal 类型");
        }

        //
        // 摘要:
        //     Enumerator support
        //
        // 返回结果:
        //     Returns a DbEnumerator object.
        public override IEnumerator GetEnumerator()
        {
            return null;
        }
        //
        // 摘要:
        //     Returns the .NET type of a given column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Type
        public override Type GetFieldType(int i)
        {
            var o = this[i];
            if (o == null)
                return null;
            return o.GetType();
        }
        //
        // 摘要:
        //     Returns a column as a float value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     float
        public override float GetFloat(int i)
        {
            object o = this[i];
            if (o is float)
                return (float)o;
            if (o is decimal)
                return (float)o;
            if (o is int)
                return (float)o;
            if (o is long)
                return (float)o;
            if (o is string)
            {
                float v = 0;
                float.TryParse((string)o, out v);
                return v;
            }

            throw new Exception("列 " + i.ToString() + " 不是 float 类型");

        }

        //
        // 摘要:
        //     Returns the column as a Guid
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Guid
        public override Guid GetGuid(int i)
        {
            return new Guid();
        }
        //
        // 摘要:
        //     Returns the column as a short
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int16
        public override short GetInt16(int i)
        {
            var o = this[i];
            if (o == null)
                return 0;

            return (short)o;
        }
        //
        // 摘要:
        //     Retrieves the column as an int
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int32
        public override int GetInt32(int i)
        {
            var o = this[i];
            if (o == null)
                return 0;

            return (int)o;
        }

        //
        // 摘要:
        //     Retrieves the column as a long
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int64
        public override long GetInt64(int i)
        {
            var o = this[i];
            if (o == null)
                return 0;

            return (long)o;
        }

        //
        // 摘要:
        //     Retrieves the name of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetName(int i)
        {
            return null;
        }
        //
        // 摘要:
        //     Retrieves the i of a column, given its name
        //
        // 参数:
        //   name:
        //     The name of the column to retrieve
        //
        // 返回结果:
        //     The int i of the column
        public override int GetOrdinal(string name)
        {
            return -1;
        }
        //
        // 摘要:
        //     Schema information in SQLite is difficult to map into .NET conventions, so
        //     a lot of work must be done to gather the necessary information so it can
        //     be represented in an ADO.NET manner.
        //
        // 返回结果:
        //     Returns a DataTable containing the schema information for the active SELECT
        //     statement being processed.
        public override DataTable GetSchemaTable()
        {
            return null;
        }
        //
        // 摘要:
        //     Retrieves the column as a string
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetString(int i)
        {
            var o = this[i];
            if (o == null)
                return "";
            return o.ToString();
        }
        //
        // 摘要:
        //     Retrieves the column as an object corresponding to the underlying datatype
        //     of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     object
        public override object GetValue(int i)
        {
            return this[i];
        }

        //
        // 摘要:
        //     Returns a collection containing all the column names and values for the current
        //     row of data in the current resultset, if any. If there is no current row
        //     or no current resultset, an exception may be thrown.
        //
        // 返回结果:
        //     The collection containing the column name and value information for the current
        //     row of data in the current resultset or null if this information cannot be
        //     obtained.
        public NameValueCollection GetValues()
        {
            return null;
        }

        //
        // 摘要:
        //     Retreives the values of multiple columns, up to the size of the supplied
        //     array
        //
        // 参数:
        //   values:
        //     The array to fill with values from the columns in the current resultset
        //
        // 返回结果:
        //     The number of columns retrieved
        public override int GetValues(object[] values)
        {
            values = this.CurrentLine.GetAllCells();
            return values.Length;
        }

        //
        // 摘要:
        //     Moves to the next resultset in multiple row-returning SQL command.
        //
        // 返回结果:
        //     True if the command was successful and a new resultset is available, False
        //     otherwise.
        public override bool NextResult()
        {
            return false;
        }

    }
}
