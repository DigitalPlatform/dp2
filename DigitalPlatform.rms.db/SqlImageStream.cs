using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Dapper;

namespace DigitalPlatform.rms
{
    public class SqlImageStream : Stream
    {
        //Connection _connection = null;

        //string m_strSqlDbName = "";
        //string m_strDataFieldName = "";

        //byte[] _textPtr = null;
        //string _id = null;

        long m_lLength = 0;	// 长度
        long m_lCurrent = 0;	// 文件指针当前位置

        IDataReader m_reader = null;

#if OLD
        public SqlImageStream(Connection connection,
            string strSqlDbName,
            string strDataFieldName,
            byte[] textPtr,
            long lTotalLength)
        {
            if (connection.IsMsSqlServer() == false)
                throw new ArgumentException("此构造函数只能用于 MS SQL Server 类型");

            this._connection = connection;
            this.m_lLength = lTotalLength;
            this.m_strSqlDbName = strSqlDbName;
            this.m_strDataFieldName = strDataFieldName;
            this._textPtr = textPtr;
        }
#endif

        public SqlImageStream(Connection connection,
    string strSqlDbName,
    string strDataFieldName,
    string id/*,
    long lTotalLength*/)
        {
            if (connection.IsPgsql() == false && connection.IsMsSqlServer() == false)
                throw new ArgumentException("此构造函数只能用于 Pgsql 和 MS SQL Server 类型");

            //this._connection = connection;
            // this.m_lLength = lTotalLength;
            //this.m_strSqlDbName = strSqlDbName;
            //this.m_strDataFieldName = strDataFieldName;
            //this._id = id;

            string strCommand = "";
            if (connection.IsMsSqlServer())
                strCommand = $" SELECT DataLength({strDataFieldName}) as length, {strDataFieldName}"  // 1
                    + $" FROM {strSqlDbName}records "
                    + " WHERE id = @id";
            else if (connection.IsPgsql())
                strCommand = $" SELECT length({strDataFieldName}) as length, {strDataFieldName}"  // 1
                    + $" FROM {strSqlDbName}records "
                    + " WHERE id = @id";

            m_reader = connection.ExecuteReader(strCommand, new { id = id });
            var ret = m_reader.Read();
            if (ret == false)
                throw new Exception($"记录 '{id}' 不存在");
            m_lLength = SqlDatabase.GetLong(m_reader, 0);
        }

        public override void Close()
        {
            if (m_reader != null)
            {
                m_reader.Close();
                m_reader.Dispose();
                m_reader = null;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {

        }

        public override long Length
        {
            get
            {
                return m_lLength;
            }
        }

        public override long Position
        {
            get
            {
                return m_lCurrent;
            }
            set
            {
                m_lCurrent = value;
            }
        }

#if OLD
        public override int Read(byte[] buffer,
    int offset,
    int count)
        {
            if (_connection.IsMsSqlServer())
                return ReadPgsql(buffer, offset, count);
            else if (_connection.IsPgsql())
                return ReadPgsql(buffer, offset, count);
            throw new NotSupportedException();
        }
#endif

        public override int Read(byte[] buffer,
    int offset,
    int count)
        {
            if (m_lCurrent >= m_lLength)
                return 0;

            // 修正本次读取尺寸
            if (m_lCurrent + count > m_lLength)
                count = (int)(m_lLength - m_lCurrent);

            if (count <= 0)
                return 0;

            // TODO: 可否限定超过一定尺寸的 data 内容就不要返回? 

            long length = m_lLength;
            if (m_lCurrent + offset + count > length)
                throw new Exception($"m_lCurrent({m_lCurrent}) + offset({offset}) + count({count}) > length({length})");
            var read_length = m_reader.GetBytes(1,
m_lCurrent + offset,
buffer,
0,
count);
            m_lCurrent += read_length;
            Debug.Assert(read_length == count);
            return (int)read_length;
        }

#if OLD
        public int ReadMsSql(byte[] buffer,
            int offset,
            int count)
        {
            if (m_lCurrent >= m_lLength)
                return 0;

            // 修正本次读取尺寸
            if (m_lCurrent + count > m_lLength)
                count = (int)(m_lLength - m_lCurrent);

            if (count <= 0)
                return 0;

            // READTEXT命令:
            // text_ptr: 有效文本指针。text_ptr 必须是 binary(16)。
            // offset:   开始读取image数据之前跳过的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。
            //			 使用 ntext 数据类型时，offset 是在开始读取数据前跳过的字符数。
            //			 使用 text 或 image 数据类型时，offset 是在开始读取数据前跳过的字节数。
            // size:     是要读取数据的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。如果 size 是 0，则表示读取了 4 KB 字节的数据。
            // HOLDLOCK: 使文本值一直锁定到事务结束。其他用户可以读取该值，但是不能对其进行修改。

            string strCommand = "use " + this.m_strSqlDbName + " "
               + " READTEXT records." + this.m_strDataFieldName
               + " @text_ptr"
               + " @offset"
               + " @size"
               + " HOLDLOCK";

            strCommand += " use master " + "\n";

            using (SqlCommand command = new SqlCommand(strCommand,
                _connection.SqlConnection))
            {

                SqlParameter text_ptrParam =
                    command.Parameters.Add("@text_ptr",
                    SqlDbType.VarBinary,
                    16);
                text_ptrParam.Value = _textPtr;

                SqlParameter offsetParam =
                    command.Parameters.Add("@offset",
                    SqlDbType.Int);  // old Int
                offsetParam.Value = m_lCurrent;

                SqlParameter sizeParam =
                    command.Parameters.Add("@size",
                    SqlDbType.Int);  // old Int
                sizeParam.Value = count;

                using (SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    dr.Read();
                    m_lCurrent += count;
                    return (int)dr.GetBytes(0,
                        0,
                        buffer,
                        offset, // 0,
                        System.Convert.ToInt32(sizeParam.Value));
                }
            } // end of using command
        }
#endif
        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                m_lCurrent = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                m_lCurrent += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                m_lCurrent = m_lLength - offset;
            }
            else
            {
                throw (new Exception("不支持的origin参数"));
            }

            return m_lCurrent;
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count
            )
        {
            throw (new NotSupportedException("PartStream不支持Write()"));
        }

        public override void SetLength(long value)
        {
            throw (new NotSupportedException("PartStream不支持SetLength()"));
        }
    }
}
