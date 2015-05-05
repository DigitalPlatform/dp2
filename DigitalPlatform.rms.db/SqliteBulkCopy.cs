#define PARAMETERS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;

namespace DigitalPlatform.rms
{
    public class SqliteBulkCopy
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


        SQLiteConnection m_connection = null;

        public SqliteBulkCopy(SQLiteConnection connection)
        {
            this.m_connection = connection;
        }

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
            using (SQLiteTransaction trans = m_connection.BeginTransaction())
            {

                using (SQLiteCommand command = new SQLiteCommand("",
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

                    SQLiteParameter param_key = new SQLiteParameter(strKeyParamName);
                    SQLiteParameter param_from = new SQLiteParameter(strFromParamName);
                    SQLiteParameter param_id = new SQLiteParameter(strIdParamName);
                    SQLiteParameter param_num = new SQLiteParameter(strKeynumParamName);
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
                    }

                } // end of using command

                trans.Commit();
            }
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

            using (SQLiteCommand command = new SQLiteCommand("",
    m_connection))
            {
                IDbTransaction trans = m_connection.BeginTransaction();
                try
                {
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


#if !PARAMETERS
                            strCommand.Append(
                                " INSERT INTO " + this.DestinationTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (N'" + MySqlHelper.EscapeString(strKeyString) + "',N'"
                                + MySqlHelper.EscapeString(strFromString) + "',N'"
                                + MySqlHelper.EscapeString(strIdString) + "',N'"
                                + MySqlHelper.EscapeString(strKeyStringNum) + "') "
                                );
#else 
                            strCommand.Append(
                                " INSERT INTO " + this.DestinationTableName
                                + " (keystring,fromstring,idstring,keystringnum) "
                                + " VALUES (" + strKeyParamName + ","
                                + strFromParamName + ","
                                + strIdParamName + ","
                                + strKeynumParamName + ") ;"
                                );
#endif

#if PARAMETERS
                        command.Parameters.AddWithValue(strKeyParamName, strKeyString);
                        command.Parameters.AddWithValue(strFromParamName, strFromString);
                        command.Parameters.AddWithValue(strIdParamName, strIdString);

                        command.Parameters.AddWithValue(strKeynumParamName, strKeyStringNum);
#endif

                        nCount++;

                        if (nCount >= this.BatchSize && this.BatchSize != 0)
                        {
                            command.CommandText = strCommand.ToString();

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
                        command.CommandText = strCommand.ToString();

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
