#define PARAMETERS  // 使用 SQL 命令参数

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace DigitalPlatform.rms
{
    public class OracleBulkCopy
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


        OracleConnection m_connection = null;

        public OracleBulkCopy(OracleConnection connection)
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


        static string EscapeString(string strText)
        {
            return strText;
        }


        // 版本 1 - 5 万条书目记录 14 分钟
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
            string strTableName = this.DestinationTableName;

            string strKeyParamName = ":k";
            string strFromParamName = ":f";
            string strIdParamName = ":i";
            string strKeynumParamName = ":n";

            OracleParameter param_key = new OracleParameter(strKeyParamName, "");
            OracleParameter param_from = new OracleParameter(strFromParamName, "");
            OracleParameter param_id = new OracleParameter(strIdParamName, "");
            OracleParameter param_keynum = new OracleParameter(strKeynumParamName, "");

            string strLine =
            "INSERT INTO " + strTableName
            + " (keystring,fromstring,idstring,keystringnum) "
            + " VALUES (" + strKeyParamName + ","
            + strFromParamName + ","
            + strIdParamName + ","
            + strKeynumParamName + ")";

            using (OracleCommand command = new OracleCommand(strLine,
    m_connection))
            {
                command.Parameters.Clear();
                command.Parameters.Add(param_key);
                command.Parameters.Add(param_from);
                command.Parameters.Add(param_id);
                command.Parameters.Add(param_keynum);

                while (true)
                {
                    OracleTransaction trans = m_connection.BeginTransaction();
                    try
                    {
                        int index = 0;
                        while (true)
                        {
                            bool bRet = reader.Read();
                            if (bRet == false)
                            {
                                if (trans != null)
                                {
                                    trans.Commit();
                                    trans = null;
                                }
                                return;
                            }

                            string strKeyString = (string)reader["keystring"];
                            string strFromString = (string)reader["fromstring"];
                            string strIdString = (string)reader["idstring"];
                            string strKeyStringNum = (string)reader["keystringnum"];

#if NO
                            command.Parameters.Clear();
                            command.Parameters.Add(strKeyParamName, strKeyString);
                            command.Parameters.Add(strFromParamName, strFromString);
                            command.Parameters.Add(strIdParamName, strIdString);
                            command.Parameters.Add(strKeynumParamName, strKeyStringNum);
#endif
                            param_key.Value = strKeyString;
                            param_from.Value = strFromString;
                            param_id.Value = strIdString;
                            param_keynum.Value = strKeyStringNum;

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                            index++;
                            if (index >= this.BatchSize)
                                break;
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
                } // end of while

            } // end of using command
        }

#if NO
        // 版本 2 - 太慢
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
            string strTableName = this.DestinationTableName;

            bool bEnd = false;

            using (OracleCommand command = new OracleCommand("",
    m_connection))
            {
                while (bEnd == false)
                {
                    try
                    {
                        int index = 0;
                        command.Parameters.Clear();
                        StringBuilder strLine = new StringBuilder();
                        while (true)
                        {
                            bool bRet = reader.Read();
                            if (bRet == false)
                            {
                                bEnd = true;
                                break;
                            }

                            string strKeyParamName = ":k" + index.ToString();
                            string strFromParamName = ":f" + index.ToString();
                            string strIdParamName = ":i" + index.ToString();
                            string strKeynumParamName = ":n" + index.ToString();

#if NO
                            OracleParameter param_key = new OracleParameter(strKeyParamName, "");
                            OracleParameter param_from = new OracleParameter(strFromParamName, "");
                            OracleParameter param_id = new OracleParameter(strIdParamName, "");
                            OracleParameter param_keynum = new OracleParameter(strKeynumParamName, "");
#endif

                            string strKeyString = (string)reader["keystring"];
                            string strFromString = (string)reader["fromstring"];
                            string strIdString = (string)reader["idstring"];
                            string strKeyStringNum = (string)reader["keystringnum"];

                            command.Parameters.Add(strKeyParamName, strKeyString);
                            command.Parameters.Add(strFromParamName, strFromString);
                            command.Parameters.Add(strIdParamName, strIdString);
                            command.Parameters.Add(strKeynumParamName, strKeyStringNum);

                            if (index == 0)
                                strLine.Append(
                                " INSERT ALL INTO " + strTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") ");
                            else
                                strLine.Append(
                                " INTO " + strTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") ");

                            index++;
                            if (index >= this.BatchSize)
                                break;
                        }

                        if (strLine.Length > 0)
                        {
                            try
                            {
                                command.CommandText = strLine.ToString() + " SELECT 1 FROM DUAL";
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                    }
                    finally
                    {

                    }
                } // end of while

            } // end of using command
        }
#endif

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
