#define USE_TRANS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

//using MySql.Data.MySqlClient;
using DigitalPlatform.Text;
using MySqlConnector;

namespace DigitalPlatform.rms
{
    public class MySqlBulkCopy
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

        //
        // 摘要:
        //     超时之前操作完成所允许的秒数。
        //
        // 返回结果:
        //     System.Data.SqlClient.SqlBulkCopy.BulkCopyTimeout 属性的整数值。
        public int BulkCopyTimeout { get; set; }


        MySqlConnection m_connection = null;

        public MySqlBulkCopy(MySqlConnection connection)
        {
            this.m_connection = connection;
        }

        static void ParseName(string strText,
            out string strDbName,
            out string strTableName)
        {
            strDbName = "";
            strTableName = "";

            int nRet = strText.IndexOf(".");
            if (nRet != -1)
            {
                strDbName = strText.Substring(0, nRet).Trim();
                strTableName = strText.Substring(nRet + 1).Trim();
            }
            else
                strDbName = strText.Trim();
        }

#if NO
        //
        // 摘要:
        //     将所提供的 System.Data.IDataReader 中的所有行复制到 System.Data.SqlClient.SqlBulkCopy
        //     对象的 System.Data.SqlClient.SqlBulkCopy.DestinationTableName 属性指定的目标表中。
        //
        // 参数:
        //   reader:
        //     一个 System.Data.IDataReader，它的行将被复制到目标表中。
        public void WriteToServer(IDataReader reader)
        {
            while (true)
            {
                int nCount = 0;
                using (MySqlTransaction trans = m_connection.BeginTransaction())
                {

                    using (MySqlCommand command = new MySqlCommand("",
            m_connection))
                    {
                        string strKeyParamName = "@k";
                        string strFromParamName = "@f";
                        string strIdParamName = "@i";
                        string strKeynumParamName = "@n";

                        string strCommand = " INSERT INTO " + this.DestinationTableName
            + " (keystring,fromstring,idstring,keystringnum) "
            + " VALUES (" + strKeyParamName + ","
            + strFromParamName + ","
            + strIdParamName + ","
            + strKeynumParamName + ") ;";

                        command.CommandText = strCommand;
                        command.Prepare();

                        MySqlParameter param_key = new MySqlParameter();
                        param_key.ParameterName = strKeyParamName;
                        MySqlParameter param_from = new MySqlParameter();
                        param_from.ParameterName = strFromParamName;
                        MySqlParameter param_id = new MySqlParameter();
                        param_id.ParameterName = strIdParamName;
                        MySqlParameter param_num = new MySqlParameter();
                        param_num.ParameterName = strKeynumParamName;

                        command.Parameters.Add(param_key);
                        command.Parameters.Add(param_from);
                        command.Parameters.Add(param_id);
                        command.Parameters.Add(param_num);

                        while (reader.Read())
                        {
                            param_key.Value = (string)reader["keystring"];
                            param_from.Value = (string)reader["fromstring"];
                            param_id.Value = (string)reader["idstring"];
                            param_num.Value = (string)reader["keystringnum"];

                            command.ExecuteNonQuery();
                            nCount++;
                            if (this.BatchSize != 0 && nCount >= this.BatchSize)
                                goto END1;
                        }

                        trans.Commit();
                        return; // 终于完成

                    } // end of using command

                END1:
                    trans.Commit();
                }
            }
        }
#endif

#if OLDVERSION
        // 老版本，在 MySQL Named Pipe 情况下会抛出异常
        // 摘要:
        //     将所提供的 System.Data.IDataReader 中的所有行复制到 System.Data.SqlClient.SqlBulkCopy
        //     对象的 System.Data.SqlClient.SqlBulkCopy.DestinationTableName 属性指定的目标表中。
        //
        // 参数:
        //   reader:
        //     一个 System.Data.IDataReader，它的行将被复制到目标表中。
        public void WriteToServer(IDataReader reader)
        {
            int index = 0;
            int nCount = 0;

            string strDbName = "";
            string strTableName = "";

            ParseName(this.DestinationTableName,
                out strDbName,
                out strTableName);

            using (MySqlCommand command = new MySqlCommand("",
    m_connection))
            {
#if NO
                MySqlTransaction trans = m_connection.BeginTransaction();
                try
                {
#endif
                StringBuilder strCommand = new StringBuilder(4096);
                while (reader.Read())
                {
#if PARAMETERS
                        string strIndex = index.ToString();
                        string strKeyParamName = "@k" + strIndex;
                        string strFromParamName = "@f" + strIndex;
                        string strIdParamName = "@i" + strIndex;
                        string strKeynumParamName = "@n" + strIndex;
#endif

                    string strKeyString = (string)reader["keystring"];

                    string strFromString = (string)reader["fromstring"];

                    string strIdString = (string)reader["idstring"];

                    string strKeyStringNum = (string)reader["keystringnum"];

                    if (strCommand.Length == 0)
                    {
#if !PARAMETERS
                        strCommand.Append(
                            " INSERT INTO " + strTableName
                            + " (keystring,fromstring,idstring,keystringnum) "
                            + " VALUES (N'" + MySqlHelper.EscapeString(strKeyString) + "',N'"
                            + MySqlHelper.EscapeString(strFromString) + "',N'"
                            + MySqlHelper.EscapeString(strIdString) + "',N'"
                            + MySqlHelper.EscapeString(strKeyStringNum) + "') "
                            );
#else 
                            strCommand.Append(
                                " INSERT INTO " + strTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") "
                                );
#endif
                    }
                    else
                    {
#if !PARAMETERS
                        strCommand.Append(
                            ",(N'" + MySqlHelper.EscapeString(strKeyString) + "',N'"
                            + MySqlHelper.EscapeString(strFromString) + "',N'"
                            + MySqlHelper.EscapeString(strIdString) + "',N'"
                            + MySqlHelper.EscapeString(strKeyStringNum) + "') "
                            );
#else
                            strCommand.Append(
                                ",(" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") "
                                );
#endif
                    }

#if PARAMETERS
                        command.Parameters.AddWithValue(strKeyParamName, strKeyString);
                        command.Parameters.AddWithValue(strFromParamName, strFromString);
                        command.Parameters.AddWithValue(strIdParamName, strIdString);

                        command.Parameters.AddWithValue(strKeynumParamName, strKeyStringNum);
#endif

                    nCount++;

                    if (nCount >= this.BatchSize && this.BatchSize != 0)
                    {
                        command.CommandText = "use " + strDbName + " ;\n"
                            + strCommand + " ;\n";

                        command.ExecuteNonQuery();

                        strCommand.Clear();
                        command.Parameters.Clear();
                        nCount = 0;
                        index = 0;
                        continue;
                    }

                    index++;
                }

                // 最后一次
                if (strCommand.Length > 0)
                {

                    command.CommandText = "use " + strDbName + " ;\n"
                        + strCommand + " ;\n";

                    command.ExecuteNonQuery();

                    strCommand.Clear();
                    command.Parameters.Clear();
                    nCount = 0;
                    index = 0;
                }

#if NO
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
#endif
            } // end of using command
        }
#endif

#if NO
        // 获得一个字符串的 UTF-8 字节数
        static int GetLength(StringBuilder text)
        {
            return Encoding.UTF8.GetByteCount(text.ToString());
        }
#endif
        public delegate void Delegate_writeErrorLog(string text);

        // 新版本，针对 Named Pipe 情况测试过
        // 摘要:
        //     将所提供的 System.Data.IDataReader 中的所有行复制到 System.Data.SqlClient.SqlBulkCopy
        //     对象的 System.Data.SqlClient.SqlBulkCopy.DestinationTableName 属性指定的目标表中。
        //
        // 参数:
        //   reader:
        //     一个 System.Data.IDataReader，它的行将被复制到目标表中。
        public void WriteToServer(IDataReader reader,
            Delegate_writeErrorLog func_writeErrorLog = null)
        {
            int index = 0;
            int nCount = 0;

            string strDbName = "";
            string strTableName = "";

            ParseName(this.DestinationTableName,
                out strDbName,
                out strTableName);

#if !USE_TRANS
            using (MySqlCommand command = new MySqlCommand("",
    m_connection))
            {
#endif

#if NO
                MySqlTransaction trans = m_connection.BeginTransaction();
                try
                {
#endif
                string strHead =
    " INSERT INTO " + strTableName
    + " (keystring,fromstring,idstring,keystringnum) "
    + " VALUES ";

                List<string> lines = new List<string>();

                StringBuilder strFragment = new StringBuilder();    // 残余的片段

                StringBuilder strCommand = new StringBuilder(4096);
                while (reader.Read())
                {
#if PARAMETERS
                        string strIndex = index.ToString();
                        string strKeyParamName = "@k" + strIndex;
                        string strFromParamName = "@f" + strIndex;
                        string strIdParamName = "@i" + strIndex;
                        string strKeynumParamName = "@n" + strIndex;
#endif

                    string strKeyString = (string)reader["keystring"];

                    string strFromString = (string)reader["fromstring"];

                    string strIdString = (string)reader["idstring"];

                    string strKeyStringNum = (string)reader["keystringnum"];

                    {
#if !PARAMETERS
                        strFragment.Append(
                            "(N'" + EscapeString(strKeyString) + "',N'"
                            + EscapeString(strFromString) + "',N'"
                            + EscapeString(strIdString) + "',N'"
                            + EscapeString(strKeyStringNum) + "') "
                            );
#else
                            strCommand.Append(
                                ",(" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") "
                                );
#endif
                    }

#if PARAMETERS
                        command.Parameters.AddWithValue(strKeyParamName, strKeyString);
                        command.Parameters.AddWithValue(strFromParamName, strFromString);
                        command.Parameters.AddWithValue(strIdParamName, strIdString);

                        command.Parameters.AddWithValue(strKeynumParamName, strKeyStringNum);
#endif

                    nCount++;

                    // TODO: 可能需要用 UTF-8 bytes 长度来限制，比如 65535
                    if ((nCount >= this.BatchSize
                        && this.BatchSize != 0)
                        || strHead.Length + StringUtil.GetUtf8Bytes(strCommand.ToString()) + StringUtil.GetUtf8Bytes(strFragment.ToString()) + 1 >= 64000)  // 32000 是可以的; 4000 是可以的; 60000 不行
                    {
                        /*
                        command.CommandText = "use " + strDbName + " ;\n"
                            + strHead + strCommand + " ;\n";

                        // 2019/3/6
                        command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                        // Debug.WriteLine("command length=" + command.CommandText.Length);
                        command.ExecuteNonQuery();

                        strCommand.Clear();
                        command.Parameters.Clear();
                        nCount = 0;
                        index = 0;
                        */
                        ExcuteCommand(
    strHead,
    strDbName,
    lines,
    func_writeErrorLog);
                        lines.Clear();
                        strCommand.Clear();
                        nCount = 0;
                        index = 0;
                    }
                    else
                        index++;

                    if (strCommand.Length > 0)
                    {
                        strCommand.Append(",");
                    }
                    strCommand.Append(strFragment.ToString());

                    lines.Add(strFragment.ToString());

                    strFragment.Clear();
                }

                // 最后一次
                if (lines.Count > 0)
                {
                    /*
                    Debug.Assert(strFragment.Length == 0, "");

                    command.CommandText = "use " + strDbName + " ;\n"
                        + strHead + strCommand + " ;\n";

                    // 2019/3/6
                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                    // Debug.WriteLine("(last) command length=" + command.CommandText.Length);
                    command.ExecuteNonQuery();

                    strCommand.Clear();
                    command.Parameters.Clear();
                    nCount = 0;
                    index = 0;
                    */

                    ExcuteCommand(
strHead,
strDbName,
lines,
func_writeErrorLog);
                    lines.Clear();
                    strCommand.Clear();
                    nCount = 0;
                    index = 0;
                }

#if NO
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
#endif

#if !USE_TRANS
            } // end of using command
#endif
        }

        static string EscapeString(string value)
        {
            // MySqlHelper.EscapeString
            return value.Replace(@"\", @"\\").Replace("'", @"\'");
        }

        void ExcuteCommand(
            string strHead,
            string strDbName,
            List<string> lines,
            Delegate_writeErrorLog func_writeErrorLog = null)
        {
#if USE_TRANS
            MySqlTransaction trans = m_connection.BeginTransaction();
#endif
            try
            {
                using (MySqlCommand command = new MySqlCommand("",
m_connection))
                {
#if USE_TRANS
                    command.Transaction = trans;
#endif

                    command.CommandText = "use " + strDbName + " ;\n"
        + strHead + StringUtil.MakePathList(lines, ",") + " ;\n";

                    // 2019/3/6
                    command.CommandTimeout = 20 * 60;  // 把超时时间放大 2008/11/20 

                    // Debug.WriteLine("command length=" + command.CommandText.Length);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();

#if USE_TRANS
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
#endif
                    return;
                }
            }
            catch (MySqlException ex)
            {
                if (ex.ErrorCode != MySqlErrorCode.TruncatedWrongValueForField)
                    throw ex;
                func_writeErrorLog?.Invoke($"写入过程发生一次 {ex.GetType().ToString()} 异常。后面将逐行重新写入");
            }
            finally
            {
#if USE_TRANS
                if (trans != null)
                    trans.Rollback();
#endif
            }

#if USE_TRANS

            using (MySqlCommand command = new MySqlCommand("",
m_connection))
            {
                // 逐行重做
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    try
                    {
                        command.CommandText = "use " + strDbName + " ;\n"
                + strHead + line + " ;\n";

                        command.CommandTimeout = 1 * 60;

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    catch (MySqlException ex)
                    {
                        if (ex.ErrorCode != MySqlErrorCode.TruncatedWrongValueForField)
                            throw ex;

                        // 写入错误日志，然后继续
                        func_writeErrorLog?.Invoke($"逐行重新写入过程中，以下行发生异常(已被跳过): {line}");
                    }
                }
            }
#else

            // 逐行重做
            using (MySqlCommand command = new MySqlCommand("",
m_connection))
            {
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    string line = lines[i];
                    try
                    {
                        command.CommandText = "use " + strDbName + " ;\n"
                + strHead + line + " ;\n";

                        command.CommandTimeout = 1 * 60;

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                    catch (MySqlException ex)
                    {
                        if (ex.ErrorCode != MySqlErrorCode.TruncatedWrongValueForField)
                            throw ex;
                        else
                            break;
                    }
                }
            }
#endif
        }

#if NO
        //
        // 摘要:
        //     将所提供的 System.Data.IDataReader 中的所有行复制到 System.Data.SqlClient.SqlBulkCopy
        //     对象的 System.Data.SqlClient.SqlBulkCopy.DestinationTableName 属性指定的目标表中。
        //
        // 参数:
        //   reader:
        //     一个 System.Data.IDataReader，它的行将被复制到目标表中。
        public void WriteToServer(IDataReader reader)
        {
            int index = 0;
            int nCount = 0;

            string strDbName = "";
            string strTableName = "";

            ParseName(this.DestinationTableName,
                out strDbName,
                out strTableName);

            using (MySqlCommand command = new MySqlCommand("",
    m_connection))
            {
                MySqlTransaction trans = null;

                trans = m_connection.BeginTransaction();
                try
                {
                    StringBuilder strCommand = new StringBuilder(4096);
                    while (reader.Read())
                    {
                        string strIndex = index.ToString();

                        string strKeyParamName = "@k" + strIndex;
                        string strFromParamName = "@f" + strIndex;
                        string strIdParamName = "@i" + strIndex;
                        string strKeynumParamName = "@n" + strIndex;

                        if (index == 0)
                        {
                            strCommand.Append(
                                " INSERT INTO " + strTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") "
                                );
                        }
                        else
                        {
                            strCommand.Append(
                                ",(" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") "
                                );
                        }

                        MySqlParameter keyParam =
                            command.Parameters.Add(strKeyParamName,
                            MySqlDbType.String);
                        keyParam.Value = reader["keystring"];

                        MySqlParameter fromParam =
                            command.Parameters.Add(strFromParamName,
                            MySqlDbType.String);
                        fromParam.Value = reader["fromstring"];

                        MySqlParameter idParam =
                            command.Parameters.Add(strIdParamName,
                            MySqlDbType.String);
                        idParam.Value = reader["idstring"];

                        MySqlParameter keynumParam =
                            command.Parameters.Add(strKeynumParamName,
                            MySqlDbType.String);
                        keynumParam.Value = reader["keystringnum"];

                        if (nCount >= this.BatchSize  && this.BatchSize != 0)
                        {
                            command.CommandText = "use " + strDbName + " ;\n"
                                + strCommand + " ;\n";

                            command.ExecuteNonQuery();

                            strCommand.Clear();
                            command.Parameters.Clear();
                            nCount = 0;
                            index = 0;
                        }
                        else
                        {
                            nCount++;
                        }

                        index++;
                    }

                    // 最后一次
                    if (strCommand.Length > 0)
                    {
                        command.CommandText = "use " + strDbName + " ;\n"
                            + strCommand + " ;\n";

                        command.ExecuteNonQuery();

                        strCommand.Clear();
                        command.Parameters.Clear();
                        nCount = 0;
                        index = 0;
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
#endif



    }

}
