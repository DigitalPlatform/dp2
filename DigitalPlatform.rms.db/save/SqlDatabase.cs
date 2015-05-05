//#define DEBUG_LOCK_SQLDATABASE

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using DigitalPlatform.Range;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // SQL库派生类
    public class SqlDatabase : Database
    {
        // 连接字符串
        private string m_strConnString = "";

        // Sql数据库名称
        private string m_strSqlDbName = "";

        public SqlDatabase(DatabaseCollection container)
            : base(container)
        { }

        // 初始化数据库象
        // parameters:
        //      node    数据库配置节点<database>
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        internal override int Initial(XmlNode node,
            out string strError)
        {
            strError = "";
            Debug.Assert(node != null, "Initial()调用错误，node参数值不能为null。");

            //****************对数据库加写锁**** 在构造时,即不能读也不能写
            this.m_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
                this.m_selfNode = node;

                // 只能在这儿写了，要不对象未初始化呢。
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-cn") + "'数据库加写锁。");
#endif

                //      -1  出错
                //      0   成功
                // 线: 不安全的
                int nRet = this.container.InternalGetConnString(
                    out this.m_strConnString,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 检索点长度
                // return:
                //      -1  出错
                //      0   成功
                // 线: 不安全
                nRet = this.container.InternalGetKeySize(
                    out this.KeySize,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 库ID
                this.PureID = DomUtil.GetAttr(this.m_selfNode, "id").Trim();
                if (this.PureID == "")
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-cn") + "'的<database>下级未定义'id'属性，或'id'属性为空";
                    return -1;
                }

                // 属性节点
                this.m_propertyNode = this.m_selfNode.SelectSingleNode("property");
                if (this.m_propertyNode == null)
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-cn") + "'的<database>下级未定义<property>元素";
                    return -1;
                }

                // <sqlserverdb>节点
                XmlNode nodeSqlServerDb = this.m_propertyNode.SelectSingleNode("sqlserverdb");
                if (nodeSqlServerDb == null)
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-cn") + "'的database/property下级未定义<sqlserverdb>元素";
                    return -1;
                }

                // 检查SqlServer库名，只有Sql类型库才需要
                this.m_strSqlDbName = DomUtil.GetAttr(nodeSqlServerDb, "name").Trim();
                if (this.m_strSqlDbName == "")
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-cn") + "'的database/property/sqlserverdb的节点未定义'name'属性，或'name'属性值为空";
                    return -1;
                }
            }
            finally
            {
                m_lock.ReleaseWriterLock();
                //***********对数据库解写锁*************
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-cn") + "'数据库解写锁。");
#endif
            }

            return 0;
        }

        // 得到数据源名称，对于Sql数据库，则是Sql数据库名。
        public override string GetSourceName()
        {
            return this.m_strSqlDbName;
        }

        // 初始化数据库，注意虚函数不能为private
        // parameter:
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的
        // 加写锁的原因，修改记录尾号，另外对SQL的操作不必担心锁
        public override int InitialPhysicalDatabase(out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            m_lock.AcquireWriterLock(m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-cn") + "'数据库加写锁。");
#endif
            try
            {
                SqlConnection connection = new SqlConnection(this.m_strConnString);
                connection.Open();
                try //连接
                {
                    string strCommand = "";
                    SqlCommand command = null;
                    // 1.建库
                    strCommand = this.GetCreateDbComdString();
                    command = new SqlCommand(strCommand,
                        connection);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "建库出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                    // 2.建表
                    int nRet = this.GetCreateTablesString(out strCommand,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    command = new SqlCommand(strCommand,
                        connection);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "建表出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                    // 3.建索引
                    nRet = this.GetCreateIndexString(out strCommand,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    command = new SqlCommand(strCommand,
                        connection);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "建索引出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                    // 4.设库记录种子为0
                    this.SetTailNo(0);

                    this.container.Changed = true;   //内容改变
                }
                finally
                {
                    connection.Close();
                }
            }
            finally
            {
                //*********************对数据库解写锁******
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-cn") + "'数据库解写锁。");
#endif
            }
            return 0;
        }

        // 得到建库命令字符串
        public string GetCreateDbComdString()
        {
            string strCommand = "use master " + "\n"
                + " if exists (select * from dbo.sysdatabases where name = N'" + this.m_strSqlDbName + "')" + "\n"
                + " drop database " + this.m_strSqlDbName + "\n"
                + " CREATE database " + this.m_strSqlDbName + "\n";

            strCommand += " use master " + "\n";

            return strCommand;
        }

        // 得到建表命令字符串
        // return
        //		-1	出错
        //		0	成功
        private int GetCreateTablesString(out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            // 创建reocrds表
            strCommand = "use " + this.m_strSqlDbName + "\n"
                + "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[records]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n"
                + "drop table [dbo].[records]" + "\n"
                + "CREATE TABLE [dbo].[records]" + "\n"
                + "(" + "\n"
                + "[id] [nvarchar] (255) NULL ," + "\n"
                + "[data] [image] NULL ," + "\n"
                + "[newdata] [image] NULL ," + "\n"
                + "[range] [nvarchar] (4000) NULL," + "\n"
                + "[dptimestamp] [nvarchar] (100) NULL ," + "\n"
                + "[metadata] [nvarchar] (4000) NULL ," + "\n"
                + ") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]" + "\n" + "\n";


            KeysCfg keysCfg = null;
            int nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;

            if (keysCfg != null)
            {

                List<TableInfo> aTableInfo = null;
                nRet = keysCfg.GetTableInfosRemoveDup(
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;


                // 建检索点表
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    strCommand += "\n" +
                        "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + tableInfo.SqlTableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)" + "\n" +
                        "drop table [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                        "CREATE TABLE [dbo].[" + tableInfo.SqlTableName + "]" + "\n" +
                        "(" + "\n" +
                        "[keystring] [nvarchar] (" + Convert.ToString(this.KeySize) + ") Null," + "\n" +         //keystring的长度由配置文件定
                        "[fromstring] [nvarchar] (255) NULL ," + "\n" +
                        "[idstring] [nvarchar] (255)  NULL ," + "\n" +
                        "[keystringnum] [bigint] NULL " + "\n" +
                        ")" + "\n" + "\n";
                }
            }

            strCommand += " use master " + "\n";

            return 0;
        }

        // 建索引命令字符串
        // return
        //		-1	出错
        //		0	成功
        public int GetCreateIndexString(out string strCommand,
            out string strError)
        {
            strCommand = "";
            strError = "";

            strCommand = "use " + this.m_strSqlDbName + "\n"
                + " CREATE INDEX records_id_index " + "\n"
                + " ON records (id) \n";

            KeysCfg keysCfg = null;
            int nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;
            if (keysCfg != null)
            {
                List<TableInfo> aTableInfo = null;
                nRet = keysCfg.GetTableInfosRemoveDup(
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = (TableInfo)aTableInfo[i];

                    strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystring_index \n"
                        + " ON " + tableInfo.SqlTableName + " (keystring) \n";
                    strCommand += " CREATE INDEX " + tableInfo.SqlTableName + "_keystringnum_index \n"
                        + " ON " + tableInfo.SqlTableName + " (keystringnum) \n";
                }
            }

            strCommand += " use master " + "\n";
            return 0;
        }

        // 删除数据库
        // return:
        //      -1  出错
        //      0   成功
        public override int Delete(out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            this.m_lock.AcquireWriterLock(m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-cn") + "'数据库加写锁。");
#endif
            try //锁
            {
                string strCommand = "";

                SqlConnection connection = new SqlConnection(this.m_strConnString);
                connection.Open();
                try //连接
                {
                    // 1.删库的sql数据库
                    SqlCommand command = null;
                    strCommand = "use master " + "\n"
                        + " if exists (select * from dbo.sysdatabases where name = N'" + this.m_strSqlDbName + "')" + "\n"
                        + " drop database " + this.m_strSqlDbName + "\n";
                    strCommand += " use master " + "\n";
                    command = new SqlCommand(strCommand,
                        connection);

                    command.ExecuteNonQuery();
                }
                catch (SqlException sqlEx)
                {
                    // 如果不存在物理数据库，则不报错

                    if (!(sqlEx.Errors is SqlErrorCollection))
                    {
                        strError = "删除sql库出错.\r\n"
                           + sqlEx.Message + "\r\n"
                           + "SQL命令:\r\n"
                           + strCommand;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "删除sql库出错.\r\n"
                        + ex.Message + "\r\n"
                        + "SQL命令:\r\n"
                        + strCommand;
                    return -1;
                }
                finally  //连接
                {
                    connection.Close();
                }


                // 删除配置目录
                string strCfgsDir = DatabaseUtil.GetLocalDir(this.container.NodeDbs,
                    this.m_selfNode);
                if (strCfgsDir != "")
                {
                    // 应对目录查重，如果有其它库使用这个目录，则不能删除，返回信息
                    if (this.container.IsExistCfgsDir(strCfgsDir, this) == true)
                    {
                        // 给错误日志写一条信息
                        this.container.WriteErrorLog("发现除了'" + this.GetCaption("zh-cn") + "'库使用'" + strCfgsDir + "'目录外，还有其它库的使用这个目录，所以不能在删除库时删除目录");
                    }
                    else
                    {
                        string strRealDir = this.container.DataDir + "\\" + strCfgsDir;
                        if (Directory.Exists(strRealDir) == true)
                        {
                            Directory.Delete(strRealDir, true);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "删除'" + this.GetCaption("zh") + "'数据库出错，原因:" + ex.Message;
                return -1;
            }
            finally
            {

                //*********************对数据库解写锁**********
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-cn") + "'数据库解写锁。");
#endif
            }

        }


        // 按ID检索记录
        // parameter:
        //		searchItem  SearchItem对象，包括检索信息
        //		isConnected 连接对象的delegate
        //		resultSet   结果集对象,存放命中记录
        // return:
        //		-1  出错
        //		0   成功
        // 线：不安全
        private int SearchByID(SearchItem searchItem,
            Delegate_isConnected isConnected,
            DpResultSet resultSet,
            out string strError)
        {
            strError = "";

            Debug.Assert(searchItem != null, "SearchByID()调用错误，searchItem参数值不能为null。");
            Debug.Assert(isConnected != null, "SearchByID()调用错误，isConnected参数值不能为null。");
            Debug.Assert(resultSet != null, "SearchByID()调用错误，resultSet参数值不能为null。");

            SqlConnection connection = new SqlConnection(this.m_strConnString);
            connection.Open();
            try
            {
                List<SqlParameter> aSqlParameter = new List<SqlParameter>();
                string strWhere = "";
                if (searchItem.Match == "left"
                    || searchItem.Match == "")
                {
                    strWhere = " WHERE id LIKE @id and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (searchItem.Match == "middle")
                {
                    strWhere = " WHERE id LIKE @id and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = "%" + searchItem.Word + "%";
                    aSqlParameter.Add(temp);
                }
                else if (searchItem.Match == "right")
                {
                    strWhere = " WHERE id LIKE @id and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                    temp.Value = "%" + searchItem.Word;
                    aSqlParameter.Add(temp);
                }
                else if (searchItem.Match == "exact")
                {
                    if (searchItem.DataType == "string")
                        searchItem.Word = DbPath.GetID10(searchItem.Word);

                    if (searchItem.Relation == "draw")
                    {
                        int nPosition;
                        nPosition = searchItem.Word.IndexOf("-");
                        if (nPosition >= 0)
                        {
                            string strStartID;
                            string strEndID;
                            StringUtil.SplitRange(searchItem.Word,
                                out strStartID,
                                out strEndID);
                            strStartID = DbPath.GetID10(strStartID);
                            strEndID = DbPath.GetID10(strEndID);

                            strWhere = " WHERE @idMin <=id and id<= @idMax and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";

                            SqlParameter temp = new SqlParameter("@idMin", SqlDbType.NVarChar);
                            temp.Value = strStartID;
                            aSqlParameter.Add(temp);

                            temp = new SqlParameter("@idMax", SqlDbType.NVarChar);
                            temp.Value = strEndID;
                            aSqlParameter.Add(temp);
                        }
                        else
                        {
                            string strOperator;
                            string strRealText;
                            StringUtil.GetPartCondition(searchItem.Word,
                                out strOperator,
                                out strRealText);

                            strRealText = DbPath.GetID10(strRealText);
                            strWhere = " WHERE id " + strOperator + " @id and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";

                            SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                    }
                    else
                    {
                        searchItem.Word = DbPath.GetID10(searchItem.Word);
                        strWhere = " WHERE id " + searchItem.Relation + " @id and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";

                        SqlParameter temp = new SqlParameter("@id", SqlDbType.NVarChar);
                        temp.Value = searchItem.Word;
                        aSqlParameter.Add(temp);
                    }
                }

                string strTop = "";
                if (searchItem.MaxCount != -1)  // 只命中指定的条数
                    strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";

                string strOrderBy = "";
                if (searchItem.IdOrder != "")
                    strOrderBy = "ORDER BY id " + searchItem.IdOrder + " ";

                string strCommand = "use " + this.m_strSqlDbName
                    + " SELECT "
                    + " DISTINCT "
                    + strTop
                    + " id "
                    + " FROM records "
                    + strWhere
                    + " " + strOrderBy + "\n";

                strCommand += " use master " + "\n";

                SqlCommand command = new SqlCommand(strCommand, connection);
                command.CommandTimeout = 20 * 60;  // 把检索时间变大
                foreach (SqlParameter sqlParameter in aSqlParameter)
                {
                    command.Parameters.Add(sqlParameter);
                }

                DatabaseCommandTask task =
                    new DatabaseCommandTask(command);
                try
                {
                    Thread t1 = new Thread(new ThreadStart(task.ThreadMain));
                    t1.Start();
                    bool bRet;
                    while (true)
                    {
                        if (isConnected != null)  //只是不再检索了
                        {
                            if (isConnected() == false)
                            {
                                strError = "用户中断";
                                return -1;
                            }
                        }
                        bRet = task.m_event.WaitOne(100, false);  //millisecondsTimeout
                        if (bRet == true)
                            break;
                    }
                    if (task.bError == true)
                    {
                        strError = task.ErrorString;
                        return -1;
                    }

                    if (task.DataReader == null)
                        return 0;

                    if (task.DataReader.HasRows == false)
                    {
                        return 0;
                    }


                    int nLoopCount = 0;
                    while (task.DataReader.Read())
                    {
                        if (nLoopCount % 10000 == 0)
                        {
                            if (isConnected != null)
                            {
                                if (isConnected() == false)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                            }
                        }

                        string strID = ((string)task.DataReader[0]);
                        if (strID.Length != 10)
                        {
                            strError = "结果集中出现了长度不是10位的记录号，不正常";
                            return -1;
                        }


                        string strId = this.FullID + "/" + strID;   //记录路径格式：库ID/记录号
                        resultSet.Add(new DpRecord(strId));

                        nLoopCount++;

                        Thread.Sleep(0);
                    }
                }
                finally
                {
                    if (task != null && task.DataReader != null)
                        task.DataReader.Close();
                }

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Errors is SqlErrorCollection)
                    strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                else
                    strError = sqlEx.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally // 连接
            {
                connection.Close();
            }
            return 0;
        }


        // 得到检索条件，私有函数，被SearchByUnion()函数调
        // 可能会抛出的异常:NoMatchException(检索方式与数据类型)
        // parameters:
        //      searchItem              SearchItem对象
        //      nodeConvertQueryString  字符串型检索词的处理信息节点
        //      nodeConvertQueryNumber  数值型检索词的处理信息节点
        //      strPostfix              Sql命令参数名称后缀，以便多个参数合在一起时区分
        //      aParameter              参数数组
        //      strKeyCondition         out参数，返回Sql检索式条件部分
        //      strError                out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全
        // ???该函数抛出异常的处理不太顺
        private int GetKeyCondition(SearchItem searchItem,
            XmlNode nodeConvertQueryString,
            XmlNode nodeConvertQueryNumber,
            string strPostfix,
            ref List<SqlParameter> aSqlParameter,
            out string strKeyCondition,
            out string strError)
        {
            strKeyCondition = "";
            strError = "";

            //检索三项是否有矛盾，该函数可能会抛出NoMatchException异常
            QueryUtil.VerifyRelation(ref searchItem.Match,
                ref searchItem.Relation,
                ref searchItem.DataType);


            int nRet = 0;
            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;


            //3.根据数据类型，对检索词进行加工
            string strKeyValue = searchItem.Word.Trim();
            if (searchItem.DataType == "string")    //字符类型调字符的配置，对检索词进行加工
            {
                if (nodeConvertQueryString != null && keysCfg != null)
                {
                    List<string> keys = null;
                    nRet = keysCfg.ConvertKeyWithStringNode(
                        null,//dataDom
                        strKeyValue,
                        nodeConvertQueryString,
                        out keys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keys.Count != 1)
                    {
                        strError = "不支持把检索词通过'split'样式加工成多个.";
                        return -1;
                    }
                    strKeyValue = keys[0];
                }
            }
            else if (searchItem.DataType == "number")   //数字型调数字格式的配置，对检索词进行加工
            {
                if (nodeConvertQueryNumber != null
                    && keysCfg != null)
                {
                    string strMyKey;
                    nRet = KeysCfg.ConvertKeyWithNumberNode(
                        strKeyValue,
                        nodeConvertQueryNumber,
                        out strMyKey,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    strKeyValue = strMyKey;
                }
            }

            string strParameterName;
            //4.根据match的值，分别得到不同的检索表达式
            if (searchItem.Match == "left"
                || searchItem.Match == "")  //如果strMatch为空，则按"左方一致"
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex =
                        new NoMatchException("在匹配方式值为left或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " ";

                SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                temp.Value = strKeyValue + "%";
                aSqlParameter.Add(temp);
            }
            else if (searchItem.Match == "middle")
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式值为middle或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " "; //N'%" + strKeyValue + "'";

                SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                temp.Value = "%" + strKeyValue + "%";
                aSqlParameter.Add(temp);
            }
            else if (searchItem.Match == "right")
            {
                //其实一开始就已经检查了三顶是否矛盾，如果有矛盾抛出抛异，这里重复检查无害，更严格
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式值为left或空时，与数据类型值" + searchItem.DataType + "矛盾，数据类型应该为string");
                    throw (ex);
                }
                strParameterName = "@keyValue" + strPostfix;
                strKeyCondition = "keystring LIKE "
                    + strParameterName + " "; //N'%" + strKeyValue + "'";

                SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                temp.Value = "%" + strKeyValue;
                aSqlParameter.Add(temp);
            }
            else if (searchItem.Match == "exact") //先看match，再看relation,最后看dataType
            {
                //从词中汲取,较复杂，注意
                if (searchItem.Relation == "draw")
                {
                    int nPosition;
                    nPosition = searchItem.Word.IndexOf("-");
                    //应按"-"算
                    if (nPosition >= 0)
                    {
                        string strStartText;
                        string strEndText;
                        StringUtil.SplitRange(searchItem.Word,
                            out strStartText,
                            out strEndText);

                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                // 加工首
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strStartText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strStartText = keys[0];


                                // 加工尾
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strEndText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strEndText = keys[0];
                            }
                            string strParameterMinName = "@keyValueMin" + strPostfix;
                            string strParameterManName = "@keyValueMan" + strPostfix;

                            strKeyCondition = " " + strParameterMinName
                                + " <=keystring and keystring<= "
                                + strParameterManName + " ";

                            SqlParameter temp = new SqlParameter(strParameterMinName, SqlDbType.NVarChar);
                            temp.Value = strStartText;
                            aSqlParameter.Add(temp);

                            temp = new SqlParameter(strParameterManName, SqlDbType.NVarChar);
                            temp.Value = strEndText;
                            aSqlParameter.Add(temp);
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                // 首
                                string strMyKey;
                                nRet = KeysCfg.ConvertKeyWithNumberNode(
                                    strStartText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                strStartText = strMyKey;

                                // 尾
                                nRet = KeysCfg.ConvertKeyWithNumberNode(
                                    strEndText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                strEndText = strMyKey;
                            }
                            strKeyCondition = strStartText
                                + " <= keystringnum and keystringnum <= "
                                + strEndText +
                                " and keystringnum <> -1";
                        }
                    }
                    else
                    {
                        string strOperator;
                        string strRealText;

                        //如果词中没有包含关系符，则按=号算
                        StringUtil.GetPartCondition(searchItem.Word,
                            out strOperator,
                            out strRealText);

                        if (strOperator == "!=")
                            strOperator = "<>";

                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(
                                    null,//dataDom
                                    strRealText,
                                    nodeConvertQueryString,
                                    out keys,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (keys.Count != 1)
                                {
                                    strError = "不支持把检索词通过'split'样式加工成多个.";
                                    return -1;
                                }
                                strRealText = keys[0];

                            }

                            strParameterName = "@keyValue" + strPostfix;
                            strKeyCondition = " keystring"
                                + strOperator
                                + " " + strParameterName + " ";

                            SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                            temp.Value = strRealText;
                            aSqlParameter.Add(temp);
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                string strMyKey;
                                nRet = KeysCfg.ConvertKeyWithNumberNode(
                                    strRealText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                strRealText = strMyKey;
                            }

                            strKeyCondition = " keystringnum"
                                + strOperator
                                + strRealText
                                + " and keystringnum <> -1";
                        }
                    }
                }
                else   //普通的关系操作符
                {
                    //当关系操作符为空为，按等于算
                    if (searchItem.Relation == "")
                        searchItem.Relation = "=";
                    if (searchItem.Relation == "!=")
                        searchItem.Relation = "<>";

                    if (searchItem.DataType == "string")
                    {
                        strParameterName = "@keyValue" + strPostfix;

                        strKeyCondition = " keystring "
                            + searchItem.Relation
                            + " " + strParameterName + " ";

                        SqlParameter temp = new SqlParameter(strParameterName, SqlDbType.NVarChar);
                        temp.Value = strKeyValue;
                        aSqlParameter.Add(temp);
                    }
                    else if (searchItem.DataType == "number")
                    {
                        strKeyCondition = " keystringnum "
                            + searchItem.Relation
                            + strKeyValue
                            + " and keystringnum <> -1";
                    }
                }
            }

            return 0;
        }

        // 检索
        // parameters:
        //      searchItem  SearchItem对象，存放检索词等信息
        //      isConnected 连接对象
        //      resultSet   结果集对象，存放命中记录
        //      strLang     语言版本，
        // return:
        //		-1	出错
        //		0	成功
        internal override int SearchByUnion(SearchItem searchItem,
            Delegate_isConnected isConnected,
            DpResultSet resultSet,
            int nWarningLevel,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            //**********对数据库加读锁**************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                bool bHasID;
                List<TableInfo> aTableInfo = null;
                int nRet = this.TableNames2aTableInfo(searchItem.TargetTables,
                    out bHasID,
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bHasID == true)
                {
                    nRet = SearchByID(searchItem,
                        isConnected,
                        resultSet,
                        out strError);

                    if (nRet == -1)
                        return -1;
                }

                // 对sql库来说,通过ID检索后，记录已排序，去重
                if (aTableInfo == null || aTableInfo.Count == 0)
                    return 0;


                string strCommand = "";

                // Sql命令参数数组
                List<SqlParameter> aSqlParameter = new List<SqlParameter>();

                string strSelectKeystring = "";
                if (searchItem.KeyOrder != "")
                {
                    if (aTableInfo.Count > 1)
                        strSelectKeystring = ",keystring";
                }

                // 循环每一个检索途径
                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i];

                    // 参数名的后缀
                    string strPostfix = Convert.ToString(i);

                    string strConditionAboutKey = "";
                    try
                    {
                        nRet = GetKeyCondition(
                            searchItem,
                            tableInfo.nodeConvertQueryString,
                            tableInfo.nodeConvertQueryNumber,
                            strPostfix,
                            ref aSqlParameter,
                            out strConditionAboutKey,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    catch (NoMatchException ex)
                    {
                        strWarning = ex.Message;
                        strError = strWarning;
                        return -1;
                    }

                    // 如果限制了一个最大数，则按每个途径都是这个最大数算
                    string strTop = "";
                    if (searchItem.MaxCount != -1)  //限制的最大数
                        strTop = " TOP " + Convert.ToString(searchItem.MaxCount) + " ";

                    string strWhere = "";
                    if (strConditionAboutKey != "")
                        strWhere = " WHERE " + strConditionAboutKey;

                    string strOneCommand = "";
                    if (i == 0)// 第一个表
                    {
                        strOneCommand = "use " + this.m_strSqlDbName + " "
                            + " SELECT "
                            + " DISTINCT "
                            + strTop
                            + " idstring" + strSelectKeystring + " "
                            + " FROM " + tableInfo.SqlTableName + " "
                            + strWhere;
                    }
                    else
                    {
                        strOneCommand = " union SELECT "
                            + " DISTINCT "
                            + strTop
                            + " idstring" + strSelectKeystring + " "  //DISTINCT 去重
                            + " FROM " + tableInfo.SqlTableName + " "
                            + strWhere;
                    }
                    strCommand += strOneCommand;
                }

                string strOrderBy = "";
                if (searchItem.OrderBy != "")
                    strOrderBy = "ORDER BY " + searchItem.OrderBy + " ";

                strCommand += strOrderBy;
                strCommand += " use master " + "\n";

                if (aSqlParameter == null)
                {
                    strError = "一个参数也没是不可能的情况";
                    return -1;
                }

                SqlCommand command = null;
                SqlConnection connection = new SqlConnection(this.m_strConnString);
                connection.Open();
                try
                {
                    command = new SqlCommand(strCommand,
                        connection);
                    foreach (SqlParameter sqlParameter in aSqlParameter)
                    {
                        command.Parameters.Add(sqlParameter);
                    }
                    command.CommandTimeout = 20 * 60;  // 把检索时间变大
                    // 调新线程处理
                    DatabaseCommandTask task = new DatabaseCommandTask(command);
                    try
                    {
                        if (task == null)
                        {
                            strError = "test为null";
                            return -1;
                        }
                        Thread t1 = new Thread(new ThreadStart(task.ThreadMain));
                        t1.Start();
                        bool bRet;
                        while (true)
                        {
                            if (isConnected != null)
                            {
                                if (isConnected() == false)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                            }
                            bRet = task.m_event.WaitOne(100, false);  //1/10秒看一次
                            if (bRet == true)
                                break;
                        }

                        if (task.DataReader == null
                            || task.DataReader.HasRows == false)
                        {
                            return 0;
                        }

                        int nGetedCount = 0;
                        while (task.DataReader.Read())
                        {
                            if (isConnected != null
                                && (nGetedCount % 10000) == 0)
                            {
                                if (isConnected() == false)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                            }

                            string strId = this.FullID + "/" + (string)task.DataReader[0]; // 记录格式为：库id/记录号
                            resultSet.Add(new DpRecord(strId));

                            nGetedCount++;

                            // 超过最大数了
                            if (searchItem.MaxCount != -1
                                && nGetedCount >= searchItem.MaxCount)
                                break;

                            Thread.Sleep(0);
                        }
                    }
                    finally
                    {
                        if (task.DataReader != null)
                            task.DataReader.Close();
                    }

                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Errors is SqlErrorCollection)
                        strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                    else
                        strError = sqlEx.Message;
                    return -1;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }
                finally // 连接
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                //*****************对数据库解读锁***************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }

            return 0;
        }



        // 根据strStyle风格,得到相就的记录号
        // prev:前一条,next:下一条,如果strID == ? 则prev为第一条,next为最后一条
        // 如果不包含prev和next则不能调此函数
        // parameter:
        //		connection	        连接对象
        //		strCurrentRecordID	当前记录ID
        //		strStyle	        风格
        //      strOutputRecordID   out参数，返回找到的记录号
        //      strError            out参数，返回出错信息
        // return:
        //		-1  出错
        //      0   未找到
        //      1   找到
        // 线：不安全
        private int GetRecordID(SqlConnection connection,
            string strCurrentRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";

            Debug.Assert(connection != null, "GetRecordID()调用错误，connection参数值不能为null。");

            if ((StringUtil.IsInList("prev", strStyle) == false)
                && (StringUtil.IsInList("next", strStyle) == false))
            {
                Debug.Assert(false, "GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
                throw new Exception("GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
            }

            strCurrentRecordID = DbPath.GetID10(strCurrentRecordID);

            string strWhere = "";
            string strOrder = "";
            if ((StringUtil.IsInList("prev", strStyle) == true))
            {
                if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                {
                    strWhere = " where id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id DESC ";
                }
                else if (StringUtil.IsInList("myself", strStyle) == true)
                {
                    strWhere = " where id<='" + strCurrentRecordID + "' and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id DESC ";
                }
                else
                {
                    strWhere = " where id<'" + strCurrentRecordID + "' and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id DESC ";
                }
            }
            else if (StringUtil.IsInList("next", strStyle) == true)
            {
                if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                {
                    strWhere = " where id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id ASC ";
                }
                else if (StringUtil.IsInList("myself", strStyle) == true)
                {
                    strWhere = " where id>='" + strCurrentRecordID + "' and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id ASC ";
                }
                else
                {
                    strWhere = " where id>'" + strCurrentRecordID + "' and id like N'[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' ";
                    strOrder = " ORDER BY id ASC ";
                }
            }
            string strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT Top 1 id "
                + " FROM records "
                + strWhere
                + strOrder;
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            SqlDataReader dr =
                command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr == null || dr.HasRows == false)
                {
                    return 0;
                }
                else
                {
                    dr.Read();
                    strOutputRecordID = (string)dr[0];
                    return 1;
                }
            }
            finally
            {
                dr.Close();
            }
        }

        // 根据strStyle风格,得到相就的记录号
        // prev:前一条,next:下一条,如果strID == ? 则prev为第一条,next为最后一条
        // 如果不包含prev和next则不能调此函数
        // parameter:
        //		strCurrentRecordID	当前记录ID
        //		strStyle	        风格
        //      strOutputRecordID   out参数，返回找到的记录号
        //      strError            out参数，返回出错信息
        // return:
        //		-1  出错
        //      0   未找到
        //      1   找到
        // 线：不安全
        internal override int GetRecordID(string strCurrentRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";

            SqlConnection connection = new SqlConnection(this.m_strConnString);
            connection.Open();
            try
            {
                // return:
                //		-1  出错
                //      0   未找到
                //      1   找到
                return this.GetRecordID(connection,
                    strCurrentRecordID,
                    strStyle,
                    out strOutputRecordID,
                    out strError);
            }
            catch (SqlException ex)
            {
                if (ex.Errors is SqlErrorCollection)
                    return 0;

                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally // 连接
            {
                connection.Close();
            }
        }

        // 按指定范围读Xml
        // parameter:
        //		strRecordID			记录ID
        //		strXPath			用来定位节点的xpath
        //		nStart				从目标读的开始位置
        //		nLength				长度 -1:开始到结束
        //		nMaxLength			限制的最大长度
        //		strStyle			风格,data:取数据 prev:前一条记录 next:后一条记录
        //							withresmetadata属性表示把资源的元数据填到body体里，
        //							同时注意时间戳是两者合并后的时间戳
        //		destBuffer			out参数，返回字节数组
        //		strMetadata			out参数，返回元数据
        //		strOutputResPath	out参数，返回相关记录的路径
        //		outputTimestamp		out参数，返回时间戳
        //		strError			out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      -10 记录局部未找到
        //		>=0 资源总长度
        //      nAdditionError -50 有一个以上下级资源记录不存在
        // 线: 安全的
        public override long GetXml(string strRecordID,
            string strXPath,
            int nStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out string strOutputRecordID,
            out byte[] outputTimestamp,
            bool bCheckAccount,
            out int nAdditionError,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            strOutputRecordID = "";
            outputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            int nRet = 0;

            int nNotFoundSubRes = 0;    // 下级没有找到的资源个数
            string strNotFoundSubResIds = "";

            // 检查ID
            // return:
            //      -1  出错
            //      0   成功
            nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                out strError);
            if (nRet == -1)
                return -1;

            // 将样式去空白
            strStyle = strStyle.Trim();

            // 取出实际的记录号
            if (StringUtil.IsInList("prev", strStyle) == true
                || StringUtil.IsInList("next", strStyle) == true)
            {
                string strTempOutputID = "";
                SqlConnection connection = new SqlConnection(this.m_strConnString);
                connection.Open();
                try
                {
                    // return:
                    //		-1  出错
                    //      0   未找到
                    //      1   找到
                    nRet = this.GetRecordID(connection,
                        strRecordID,
                        strStyle,
                        out strTempOutputID,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                finally
                {
                    connection.Close();
                }
                if (nRet == 0 || strTempOutputID == "")
                {
                    strError = "未找到记录ID '" + strRecordID + "' 的风格为'" + strStyle + "'的记录";
                    return -4;
                }
                strRecordID = strTempOutputID;

                // 再次检查一下返回的ID
                // return:
                //      -1  出错
                //      0   成功
                nRet = DatabaseUtil.CheckAndGet10RecordID(ref strRecordID,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // 根据风格要求，返回资源路径
            if (StringUtil.IsInList("outputpath", strStyle) == true)
            {
                strOutputRecordID = DbPath.GetCompressedID(strRecordID);
            }


            // 对帐户库开的后门，用于更新帐户,RefreshUser是会调WriteXml()是加锁的函数
            // 不能在开头打开一个connection对象
            if (bCheckAccount == true &&
                StringUtil.IsInList("account", this.TypeSafety) == true)
            {
                // 如果要获得记录正好是账户库记录，而且在
                // UserCollection中，那就把相关的User记录
                // 保存回数据库，以便稍后从数据库中提取，
                // 而不必从内存中提取。
                string strAccountPath = this.FullID + "/" + strRecordID;

                // return:
                //		-1  出错
                //      -4  记录不存在
                //		0   成功
                nRet = this.container.UserColl.SaveUserIfNeed(
                    strAccountPath,
                    out strError);
                if (nRet <= -1)
                    return nRet;
            }


            //********给库加读锁**************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录加读锁。");
#endif
                try //锁
                {

                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try  //连接
                    {
                        // return:
                        //		-1  出错
                        //      0   不存在
                        //      1   存在
                        nRet = this.RecordIsExist(connection,
                            strRecordID,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (nRet == 0)
                        {
                            strError = "记录'" + strRecordID + "'在库中不存在";
                            return -4;
                        }

                        byte[] baWholeXml = null;
                        byte[] baPreamble = null;
                        string strXml = null;
                        XmlDocument dom = null;
                        // 带资源元数据的情况，要先提出来xml数据的
                        if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                        {
                            // 可以用一个简单的函数包一下
                            // return:
                            //		-1  出错
                            //		-4  记录不存在
                            //		>=0 资源总长度
                            nRet = this.GetImage(connection,
                                strRecordID,
                                "data",
                                0,
                                -1,
                                -1,
                                strStyle,
                                out baWholeXml,
                                out strMetadata,
                                out outputTimestamp,
                                out strError);
                            if (nRet <= -1)
                                return nRet;

                            strXml = DatabaseUtil.ByteArrayToString(baWholeXml,
                                out baPreamble);

                            if (strXml != "")
                            {
                                dom = new XmlDocument();
                                dom.PreserveWhitespace = true; //设PreserveWhitespace为true

                                try
                                {
                                    dom.LoadXml(strXml);
                                }
                                catch (Exception ex)
                                {
                                    strError = "GetXml() 加载数据到dom出错，原因：" + ex.Message;
                                    return -1;
                                }


                                // 找到所有的dprms:file元素
                                XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
                                nsmgr.AddNamespace("dprms", DpNs.dprms);
                                XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                                foreach (XmlNode fileNode in fileList)
                                {
                                    string strObjectID = DomUtil.GetAttr(fileNode, "id");
                                    if (strObjectID == "")
                                        continue;

                                    byte[] baObjectDestBuffer;
                                    string strObjectMetadata;
                                    byte[] baObjectOutputTimestamp;

                                    string strObjectFullID = strRecordID + "_" + strObjectID;
                                    // return:
                                    //		-1  出错
                                    //		-4  记录不存在
                                    //		>=0 资源总长度
                                    nRet = this.GetImage(connection,
                                        strObjectFullID,
                                        "data",
                                        nStart,
                                        nLength,
                                        nMaxLength,
                                        "metadata,timestamp",//strStyle,
                                        out baObjectDestBuffer,
                                        out strObjectMetadata,
                                        out baObjectOutputTimestamp,
                                        out strError);
                                    if (nRet <= -1)
                                    {
                                        // 资源记录不存在
                                        if (nRet == -4)
                                        {
                                            nNotFoundSubRes++;

                                            if (strNotFoundSubResIds != "")
                                                strNotFoundSubResIds += ",";
                                            strNotFoundSubResIds += strObjectID;
                                        }
                                    }

                                    // 解析metadata
                                    if (strObjectMetadata != "")
                                    {
                                        Hashtable values = rmsUtil.ParseMedaDataXml(strObjectMetadata,
                                            out strError);
                                        if (values == null)
                                            return -1;

                                        string strObjectTimestamp = ByteArray.GetHexTimeStampString(baObjectOutputTimestamp);

                                        DomUtil.SetAttr(fileNode, "__mime", (string)values["mimetype"]);
                                        DomUtil.SetAttr(fileNode, "__localpath", (string)values["localpath"]);
                                        DomUtil.SetAttr(fileNode, "__size", (string)values["size"]);

                                        DomUtil.SetAttr(fileNode, "__timestamp", strObjectTimestamp);
                                    }
                                }
                            } // end if (strXml != "")

                        } // if (StringUtil.IsInList("withresmetadata", strStyle) == true)

                        // 通过xpath找片断的情况
                        if (strXPath != null && strXPath != "")
                        {
                            if (baWholeXml == null)
                            {
                                // return:
                                //		-1  出错
                                //		-4  记录不存在
                                //		>=0 资源总长度
                                nRet = this.GetImage(connection,
                                    strRecordID,
                                    "data",
                                    0,
                                    -1,
                                    -1,
                                    strStyle,
                                    out baWholeXml,
                                    out strMetadata,
                                    out outputTimestamp,
                                    out strError);
                                if (nRet <= -1)
                                    return nRet;

                                if (baWholeXml == null)
                                {
                                    strError = "您虽然使用了xpath，但未取得数据，可以是由于style风格不正确，当前style的值为'" + strStyle + "'。";
                                    return -1;

                                }

                                strXml = DatabaseUtil.ByteArrayToString(baWholeXml,
                                    out baPreamble);

                                if (strXml != "")
                                {

                                    dom = new XmlDocument();
                                    dom.PreserveWhitespace = true; //设PreserveWhitespace为true

                                    try
                                    {
                                        dom.LoadXml(strXml);
                                    }
                                    catch (Exception ex)
                                    {
                                        strError = "GetXml() 加载数据到dom出错，原因：" + ex.Message;
                                        return -1;
                                    }
                                }
                            } 

                            if (dom != null)
                            {
                                string strLocateXPath = "";
                                string strCreatePath = "";
                                string strNewRecordTemplate = "";
                                string strAction = "";
                                nRet = DatabaseUtil.PaseXPathParameter(strXPath,
                                    out strLocateXPath,
                                    out strCreatePath,
                                    out strNewRecordTemplate,
                                    out strAction,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                if (strLocateXPath == "")
                                {
                                    strError = "xpath表达式中的locate参数不能为空值";
                                    return -1;
                                }

                                XmlNode node = dom.DocumentElement.SelectSingleNode(strLocateXPath);
                                if (node == null)
                                {
                                    strError = "从dom中未找到XPath为'" + strLocateXPath + "'的节点";
                                    return -10;
                                }

                                string strOutputText = "";
                                if (node.NodeType == XmlNodeType.Element)
                                {
                                    strOutputText = node.OuterXml;
                                }
                                else if (node.NodeType == XmlNodeType.Attribute)
                                {
                                    strOutputText = node.Value;
                                }
                                else
                                {
                                    strError = "通过xpath '" + strXPath + "' 找到的节点的类型不支持。";
                                    return -1;
                                }

                                byte[] baOutputText = DatabaseUtil.StringToByteArray(strOutputText,
                                    baPreamble);

                                int nRealLength;
                                // return:
                                //		-1  出错
                                //		0   成功
                                nRet = DatabaseUtil.GetRealLength(nStart,
                                    nLength,
                                    baOutputText.Length,
                                    nMaxLength,
                                    out nRealLength,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                destBuffer = new byte[nRealLength];

                                Array.Copy(baOutputText,
                                    nStart,
                                    destBuffer,
                                    0,
                                    nRealLength);
                            }
                            else
                            {
                                destBuffer = new byte[0];
                            }

                            return 0;
                        } // end if (strXPath != null && strXPath != "")

                        if (dom != null)
                        {
                            // 带资源元数据的情况，要先提出来xml数据的
                            if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                            {
                                // 使用XmlTextWriter保存成utf8的编码方式
                                MemoryStream ms = new MemoryStream();
                                XmlTextWriter textWriter = new XmlTextWriter(ms, Encoding.UTF8);
                                dom.Save(textWriter);
                                //dom.Save(ms);

                                int nRealLength;
                                // return:
                                //		-1  出错
                                //		0   成功
                                nRet = DatabaseUtil.GetRealLength(nStart,
                                    nLength,
                                    (int)ms.Length,
                                    nMaxLength,
                                    out nRealLength,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                destBuffer = new byte[nRealLength];

                                // 带元素的信息后的总长度
                                long nWithMetedataTotalLength = ms.Length;

                                ms.Seek(nStart, SeekOrigin.Begin);
                                ms.Read(destBuffer,
                                    0,
                                    destBuffer.Length);
                                ms.Close();

                                if (nNotFoundSubRes > 0)
                                {
                                    strError = "记录" + strRecordID + "中id为 " + strNotFoundSubResIds + " 的下级资源记录不存在";
                                    nAdditionError = -50; // 有一个以上下级资源记录不存在
                                }

                                return nWithMetedataTotalLength;
                            }
                        } // end if (dom != null)

                        // 不使用xpath的情况
                        // return:
                        //		-1  出错
                        //		-4  记录不存在
                        //		>=0 资源总长度
                        nRet = this.GetImage(connection,
                            strRecordID,
                            "data",
                            nStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out destBuffer,
                            out strMetadata,
                            out outputTimestamp,
                            out strError);

                        if (nRet >= 0 && nNotFoundSubRes > 1)
                        {
                            strError = "记录" + strRecordID + "中id为 " + strNotFoundSubResIds + " 的下级资源记录不存在";
                            nAdditionError = -50; // 有一个以上下级资源记录不存在
                        }

                        return nRet;
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = "取记录'" + strRecordID + "'出错了，原因:" + sqlEx.Message;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "取记录'" + strRecordID + "'出错了，原因:" + ex.Message;
                        return -1;
                    }
                    finally //连接
                    {
                        connection.Close();
                    }
                }
                finally //锁
                {

                    //*********对记录解读锁******
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录解读锁。");
#endif
                }
            }
            catch (Exception ex)
            {
                strError = "取记录'" + strRecordID + "'出错了，原因:" + ex.Message;
                return -1;
            }
            finally
            {
                //***********对数据库解读锁*****************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif

            }
        }


        // 得到xml数据
        // 线:安全的,供外部调
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      0   正确
        public override int GetXmlData(string strID,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            strID = DbPath.GetID10(strID);

            SqlConnection connection = new SqlConnection(this.m_strConnString);
            connection.Open();
            try
            {
                // return:
                //      -1  出错
                //      -4  记录不存在
                //      0   正确
                return this.GetXmlString(connection,
                    strID,
                    out strXml,
                    out strError);
            }
            finally
            {
                connection.Close();
            }
        }


        // 取xml数据到字符串,包装GetXmlData()
        // 线:不安全
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      0   正确
        private int GetXmlString(SqlConnection connection,
            string strID,
            out string strXml,
            out string strError)
        {
            byte[] baPreamble;
            // return:
            //      -1  出错
            //      -4  记录不存在
            //      0   正确
            return this.GetXmlData(connection,
                strID,
                "data",
                out strXml,
                out baPreamble,
                out strError);
        }

        // 得到xml字符串,包装GetImage()
        // 线: 不安全
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      0   正确
        private int GetXmlData(SqlConnection connection,
            string strID,
            string strFieldName,
            out string strXml,
            out byte[] baPreamble,
            out string strError)
        {
            baPreamble = new byte[0];
            strXml = "";
            strError = "";

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            byte[] newXmlBuffer;
            byte[] outputTimestamp;
            string strMetadata;
            // return:
            //		-1  出错
            //		-4  记录不存在
            //		>=0 资源总长度
            nRet = this.GetImage(connection,
                strID,
                strFieldName,
                0,
                -1,
                -1,
                "data",
                out newXmlBuffer,
                out strMetadata,
                out outputTimestamp,
                out strError);
            if (nRet <= -1)
                return nRet;

            strXml = DatabaseUtil.ByteArrayToString(newXmlBuffer,
                out baPreamble);
            return 0;
        }

        // 按指定范围读资源
        // parameter:
        //		strID       记录ID
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        // return:
        //		-1  出错
        //		-4  记录不存在
        //		>=0 资源总长度
        public override int GetObject(string strRecordID,
            string strObjectID,
            int nStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] outputTimestamp,
            out string strError)
        {
            destBuffer = null;
            outputTimestamp = null;
            strMetadata = "";
            strError = "";

            strRecordID = DbPath.GetID10(strRecordID);
            //********对数据库加读锁**************
            m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录加读锁。");
#endif
                try  // 记录锁
                {

                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try // 连接
                    {

                        string strObjectFullID = strRecordID + "_" + strObjectID;
                        // return:
                        //		-1  出错
                        //		-4  记录不存在
                        //		>=0 资源总长度
                        return this.GetImage(connection,
                            strObjectFullID,
                            "data",
                            nStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out destBuffer,
                            out strMetadata,
                            out outputTimestamp,
                            out strError);
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //*************对记录解读锁***********
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录解读锁。");
#endif
                }
            }
            finally //库锁
            {
                //******对数据库解读锁*********
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 按指定范围读资源
        // parameter:
        //		strID       记录ID
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		nMaxLength  最大长度,当为-1时,表示不限
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  记录不存在
        //		>=0 资源总长度
        private int GetImage(SqlConnection connection,
            string strID,
            string strImageFieldName,
            int nStart,
            int nLength1,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] outputTimestamp,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            outputTimestamp = null;
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            strID = DbPath.GetID10(strID);

            int nTotalLength = 0;


            // 1.textPtr
            string strTextPtrComm = "";
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                strTextPtrComm = " @textPtr=TEXTPTR(" + strImageFieldName + ")";
            }

            // 2.length,一定要有
            string strLengthComm = "";
            strLengthComm = " @Length=DataLength(" + strImageFieldName + ")";

            // 3.timestamp
            string strTimestampComm = "";
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                strTimestampComm = " @dptimestamp=dptimestamp";
            }
            // 4.metadata
            string strMetadataComm = "";
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                strMetadataComm = " @metadata=metadata";
            }
            // 5.range
            string strRangeComm = "";
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                strRangeComm = " @range=range";
            }

            // 部分命令字符串
            string strPartComm = "";

            if (strTextPtrComm != "")
            {
                if (strPartComm != "")
                    strPartComm += ",";
                strPartComm += strTextPtrComm;
            }

            if (strLengthComm != "")
            {
                if (strPartComm != "")
                    strPartComm += ",";
                strPartComm += strLengthComm;
            }

            if (strTimestampComm != "")
            {
                if (strPartComm != "")
                    strPartComm += ",";
                strPartComm += strTimestampComm;
            }

            if (strMetadataComm != "")
            {
                if (strPartComm != "")
                    strPartComm += ",";
                strPartComm += strMetadataComm;
            }

            if (strRangeComm != "")
            {
                if (strPartComm != "")
                    strPartComm += ",";
                strPartComm += strRangeComm;
            }

            if (strPartComm != "")
                strPartComm += ",";
            strPartComm += " @testid=id";

            string strCommand = "";
            // DataLength()函数int类型
            strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT "
                + strPartComm + " "
                + " FROM records WHERE id=@id";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;


            SqlParameter testidParam =
                    command.Parameters.Add("@testid",
                    SqlDbType.NVarChar,
                    255);
            testidParam.Direction = ParameterDirection.Output;

            // 1.textPtr
            SqlParameter textPtrParam = null;
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                textPtrParam =
                    command.Parameters.Add("@textPtr",
                    SqlDbType.VarBinary,
                    16);
                textPtrParam.Direction = ParameterDirection.Output;

            }
            // 2.length,一定要返回
            SqlParameter lengthParam =
                command.Parameters.Add("@length",
                SqlDbType.Int);
            lengthParam.Direction = ParameterDirection.Output;

            // 3.timestamp
            SqlParameter timestampParam = null;
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                timestampParam =
                    command.Parameters.Add("@dptimestamp",
                    SqlDbType.NVarChar,
                    100);
                timestampParam.Direction = ParameterDirection.Output;
            }
            // 4.metadata
            SqlParameter metadataParam = null;
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                metadataParam =
                    command.Parameters.Add("@metadata",
                    SqlDbType.NVarChar,
                    4000);
                metadataParam.Direction = ParameterDirection.Output;

            }
            // 5.range
            SqlParameter rangeParam = null;
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                rangeParam =
                    command.Parameters.Add("@range",
                    SqlDbType.NVarChar,
                    4000);
                rangeParam.Direction = ParameterDirection.Output;
            }



            // 执行命令
            command.ExecuteNonQuery();


            if (testidParam == null
                || (testidParam.Value is System.DBNull))
            {
                strError = "记录'" + strID + "'在库中不存在";
                return -4;
            }


            // 2.length,一定会返回
            if (lengthParam != null
                && (!(lengthParam.Value is System.DBNull)))
            {
                nTotalLength = (int)lengthParam.Value;
            }

            // 3.timestamp
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                if (timestampParam != null
                    && (!(timestampParam.Value is System.DBNull)))
                {
                    string strOutputTimestamp = (string)timestampParam.Value;
                    outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
                }
            }
            // 4.metadata
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                if (metadataParam != null
                    && (!(metadataParam.Value is System.DBNull)))
                {
                    strMetadata = (string)metadataParam.Value;
                }
            }
            // 5.range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                if (rangeParam != null
                    && (!(rangeParam.Value is System.DBNull)))
                {
                    string strRange = (string)rangeParam.Value;
                }
            }


            // 1.textPtr
            byte[] textPtr = null;
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (textPtrParam != null
                    && (!(textPtrParam.Value is System.DBNull)))
                {
                    textPtr = (byte[])textPtrParam.Value;
                }
                else
                {
                    destBuffer = new byte[0];
                    return 0;

                    // 这里说明Image字段为空

                    //strError = strID + "是空记录";
                    //return -3;
                }
            }



            // 需要提取数据时,才会取数据
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (nLength1 == 0)  // 取0长度
                {
                    destBuffer = new byte[0];
                    return nTotalLength;    // >= 0
                }

                if (textPtr == null)
                {
                    strError = "textPtr为null";
                    return -1;
                }

                int nOutputLength = 0;
                // 得到实际读的长度
                // return:
                //		-1  出错
                //		0   成功
                nRet = DatabaseUtil.GetRealLength(nStart,
                    nLength1,
                    nTotalLength,
                    nMaxLength,
                    out nOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                // READTEXT命令:
                // text_ptr: 有效文本指针。text_ptr 必须是 binary(16)。
                // offset:   开始读取image数据之前跳过的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。
                //			 使用 ntext 数据类型时，offset 是在开始读取数据前跳过的字符数。
                //			 使用 text 或 image 数据类型时，offset 是在开始读取数据前跳过的字节数。
                // size:     是要读取数据的字节数（使用 text 或 image 数据类型时）或字符数（使用 ntext 数据类型时）。如果 size 是 0，则表示读取了 4 KB 字节的数据。
                // HOLDLOCK: 使文本值一直锁定到事务结束。其他用户可以读取该值，但是不能对其进行修改。

                strCommand = "use " + this.m_strSqlDbName + " "
                    + " READTEXT records." + strImageFieldName
                    + " @text_ptr"
                    + " @offset"
                    + " @size"
                    + " HOLDLOCK";

                strCommand += " use master " + "\n";

                command = new SqlCommand(strCommand,
                    connection);

                SqlParameter text_ptrParam =
                    command.Parameters.Add("@text_ptr",
                    SqlDbType.VarBinary,
                    16);
                text_ptrParam.Value = textPtr;

                SqlParameter offsetParam =
                    command.Parameters.Add("@offset",
                    SqlDbType.Int);
                offsetParam.Value = nStart;

                SqlParameter sizeParam =
                    command.Parameters.Add("@size",
                    SqlDbType.Int);
                sizeParam.Value = nOutputLength;

                destBuffer = new Byte[nOutputLength];

                SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                dr.Read();
                dr.GetBytes(0,
                    0,
                    destBuffer,
                    0,
                    System.Convert.ToInt32(sizeParam.Value));
                dr.Close();
            }

            return nTotalLength;
        }


        // 写xml数据
        // parameter:
        //		strID           记录ID -1:表示追加一条记录
        //		strRanges       目标的位置,多个range用逗号分隔
        //		nTotalLength    总长度
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp 返回的时间戳
        //		strOutputID     返回的记录ID,当strID == -1时,得到实际的ID
        //		strError        
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录不存在
        //      -6  权限不够
        //		0   成功
        public override int WriteXml(User oUser,  //null，则不检索权限
            string strID,
            string strXPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp,
            out string strOutputID,
            out string strOutputValue,   //当AddInteger 或 AppendString时 返回值最后的值
            bool bCheckAccount,
            out string strError)
        {
            strOutputValue = "";
            outputTimestamp = null;
            strOutputID = "";
            strError = "";

            if (strID == "?")
                strID = "-1";

            bool bPushTailNo = false;
            strID = this.EnsureID(strID,
                out bPushTailNo);  //加好写锁
            if (oUser != null)
            {
                string strTempRecordPath = this.GetCaption("zh-cn") + "/" + strID;
                if (bPushTailNo == true)
                {
                    string strExistRights = "";
                    bool bHasRight = oUser.HasRights(strTempRecordPath,
                        ResType.Record,
                        "create",//"append",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }
                }
                else
                {
                    string strExistRights = "";
                    bool bHasRight = oUser.HasRights(strTempRecordPath,
                        ResType.Record,
                        "overwrite",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + oUser.Name + "'，对'" + strTempRecordPath + "'记录没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }
                }
            }

            strOutputID = DbPath.GetCompressedID(strID);
            int nRet = 0;

            bool bFull = false;
            //*********对数据库加读锁*************
            m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                strID = DbPath.GetID10(strID);
                //**********对记录加写锁***************
                this.m_recordLockColl.LockForWrite(strID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录加写锁。");
#endif
                try // 记录锁
                {

                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try // 连接
                    {
                        // 1.如果记录不存在,插入一条字节的记录,以确保得到textPtr
                        // return:
                        //		-1  出错
                        //      0   不存在
                        //      1   存在
                        nRet = this.RecordIsExist(connection,
                            strID,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        bool bExist = false;
                        if (nRet == 1)
                            bExist = true;

                        // 新记录时，插入一个字节，并生成新时间戳
                        if (bExist == false)
                        {
                            byte[] tempInputTimestamp = inputTimestamp;
                            // 注意新记录的时间戳,用inputTimestamp变量
                            nRet = this.InsertRecord(connection,
                                strID,
                                out inputTimestamp,//tempTimestamp,//
                                out strError);

                            if (nRet == -1)
                                return -1;
                        }


                        // 写数据
                        // return:
                        //		-1	一般性错误
                        //		-2	时间戳不匹配
                        //		0	成功
                        nRet = this.WriteSqlRecord(connection,
                            strID,
                            strRanges,
                            (int)lTotalLength,
                            baSource,
                            streamSource,
                            strMetadata,
                            strStyle,
                            inputTimestamp,
                            out outputTimestamp,
                            out bFull,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        // 检查范围
                        //string strCurrentRange = this.GetRange(connection,
                        //	strID);
                        if (bFull == true)  //覆盖完了
                        {
                            byte[] baOldPreamble = new byte[0];
                            byte[] baNewPreamble = new byte[0];

                            // 1.得到新旧检索点
                            string strOldXml = "";
                            if (bExist == true)
                            {
                                // return:
                                //      -1  出错
                                //      -4  记录不存在
                                //      0   正确
                                nRet = this.GetXmlData(
                                    connection,
                                    strID,
                                    "data",
                                    out strOldXml,
                                    out baOldPreamble,
                                    out strError);
                                if (nRet <= -1 && nRet != -3)
                                    return nRet;
                            }

                            string strNewXml = "";
                            // return:
                            //      -1  出错
                            //      -4  记录不存在
                            //      0   正确
                            nRet = this.GetXmlData(
                                connection,
                                strID,
                                "newdata",
                                out strNewXml,
                                out baNewPreamble,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 修改部分
                            if (strXPath != null
                                && strXPath != "")
                            {
                                string strLocateXPath = "";
                                string strCreatePath = "";
                                string strNewRecordTemplate = "";
                                string strAction = "";
                                nRet = DatabaseUtil.PaseXPathParameter(strXPath,
                                    out strLocateXPath,
                                    out strCreatePath,
                                    out strNewRecordTemplate,
                                    out strAction,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                XmlDocument tempDom = new XmlDocument();
                                tempDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                                try
                                {
                                    if (strOldXml == "")
                                    {
                                        if (strNewRecordTemplate == "")
                                            tempDom.LoadXml("<root/>");
                                        else
                                            tempDom.LoadXml(strNewRecordTemplate);
                                    }
                                    else
                                        tempDom.LoadXml(strOldXml);
                                }
                                catch (Exception ex)
                                {
                                    strError = "WriteXml() 在给'" + this.GetCaption("zh-cn") + "'库写入记录'" + strID + "'时，装载旧记录到dom出错,原因:" + ex.Message;
                                    return -1;
                                }


                                if (strLocateXPath == "")
                                {
                                    strError = "xpath表达式中的locate参数不能为空值";
                                    return -1;
                                }

                                // 通过strLocateXPath定位到指定的节点
                                XmlNode node = null;
                                try
                                {
                                    node = tempDom.DocumentElement.SelectSingleNode(strLocateXPath);
                                }
                                catch (Exception ex)
                                {
                                    strError = "WriteXml() 在给'" + this.GetCaption("zh-cn") + "'库写入记录'" + strID + "'时，XPath式子'" + strXPath + "'选择元素时出错,原因:" + ex.Message;
                                    return -1;
                                }



                                if (node == null)
                                {
                                    if (strCreatePath == "")
                                    {
                                        strError = "给'" + this.GetCaption("zh-cn") + "'库写入记录'" + strID + "'时，XPath式子'" + strXPath + "'指定的节点未找到。此时xpath表达式中的create参数不能为空值";
                                        return -1;
                                    }

                                    node = DomUtil.CreateNodeByPath(tempDom.DocumentElement,
                                        strCreatePath);
                                    if (node == null)
                                    {
                                        strError = "内部错误!";
                                        return -1;
                                    }

                                }

                                if (node.NodeType == XmlNodeType.Attribute)
                                {

                                    if (strAction == "AddInteger"
                                        || strAction == "+AddInteger"
                                        || strAction == "AddInteger+")
                                    {
                                        int nNumber = 0;
                                        try
                                        {
                                            nNumber = Convert.ToInt32(strNewXml);
                                        }
                                        catch (Exception ex)
                                        {
                                            strError = "传入的内容'" + strNewXml + "'不是数字格式。" + ex.Message;
                                            return -1;
                                        }

                                        string strOldValue = node.Value;
                                        string strLastValue;
                                        nRet = StringUtil.IncreaseNumber(strOldValue,
                                            nNumber,
                                            out strLastValue,
                                            out strError);
                                        if (nRet == -1)
                                            return -1;

                                        if (strAction == "AddInteger+")
                                        {
                                            strOutputValue = node.Value;
                                        }
                                        else
                                        {
                                            strOutputValue = strLastValue;
                                        }

                                        node.Value = strLastValue;
                                        //strOutputValue = node.Value;
                                    }
                                    else if (strAction == "AppendString")
                                    {

                                        node.Value = node.Value + strNewXml;
                                        strOutputValue = node.Value;
                                    }
                                    else if (strAction == "Push")
                                    {
                                        string strLastValue;
                                        nRet = StringUtil.GetBiggerLedNumber(node.Value,
                                            strNewXml,
                                            out strLastValue,
                                            out strError);
                                        if (nRet == -1)
                                            return -1;

                                        node.Value = strLastValue;
                                        strOutputValue = node.Value;
                                    }
                                    else
                                    {
                                        node.Value = strNewXml;
                                        strOutputValue = node.Value;
                                    }
                                }
                                else if (node.NodeType == XmlNodeType.Element)
                                {

                                    //Create a document fragment.
                                    XmlDocumentFragment docFrag = tempDom.CreateDocumentFragment();

                                    //Set the contents of the document fragment.
                                    docFrag.InnerXml = strNewXml;

                                    //Add the children of the document fragment to the
                                    //original document.
                                    node.ParentNode.InsertBefore(docFrag, node);

                                    if (strAction == "AddInteger"
                                        || strAction == "AppendString")
                                    {
                                        XmlNode newNode = node.PreviousSibling;
                                        if (newNode == null)
                                        {
                                            strError = "newNode不可能为null";
                                            return -1;
                                        }

                                        string strNewValue = newNode.InnerText;
                                        string strOldValue = DomUtil.GetNodeText(node);
                                        if (strAction == "AddInteger")
                                        {


                                            int nNumber = 0;
                                            try
                                            {
                                                nNumber = Convert.ToInt32(strNewValue);
                                            }
                                            catch (Exception ex)
                                            {
                                                strError = "传入的内容'" + strNewValue + "'不是数字格式。" + ex.Message;
                                                return -1;
                                            }


                                            string strLastValue;
                                            nRet = StringUtil.IncreaseNumber(strOldValue,
                                                nNumber,
                                                out strLastValue,
                                                out strError);
                                            if (nRet == -1)
                                                return -1;
                                            /*
                                                                                    string strLastValue;
                                                                                    nRet = Database.AddInteger(strNewValue,
                                                                                        strOldValue,
                                                                                        out strLastValue,
                                                                                        out strError);
                                                                                    if (nRet == -1)
                                                                                        return -1;
                                            */
                                            newNode.InnerText = strLastValue;
                                            strOutputValue = newNode.OuterXml;
                                        }
                                        else if (strAction == "AppendString")
                                        {
                                            newNode.InnerText = strOldValue + strNewValue;
                                            strOutputValue = newNode.OuterXml;
                                        }
                                        else if (strAction == "Push")
                                        {
                                            string strLastValue;
                                            nRet = StringUtil.GetBiggerLedNumber(strOldValue,
                                                strNewValue,
                                                out strLastValue,
                                                out strError);
                                            if (nRet == -1)
                                                return -1;

                                            newNode.InnerText = strLastValue;
                                            strOutputValue = newNode.OuterXml;
                                        }
                                    }

                                    node.ParentNode.RemoveChild(node);

                                }

                                strNewXml = tempDom.OuterXml;

                                byte[] baRealXml =
                                    DatabaseUtil.StringToByteArray(
                                    strNewXml,
                                    baNewPreamble);

                                string strMyRange = "";
                                strMyRange = "0-" + Convert.ToString(baRealXml.Length - 1);
                                lTotalLength = baRealXml.Length;

                                // return:
                                //		-1	一般性错误
                                //		-2	时间戳不匹配
                                //		0	成功
                                nRet = this.WriteSqlRecord(connection,
                                    strID,
                                    strMyRange,
                                    (int)lTotalLength,
                                    baRealXml,
                                    null,
                                    strMetadata,
                                    strStyle,
                                    outputTimestamp,   //注意这儿
                                    out outputTimestamp,
                                    out bFull,
                                    out strError);
                                if (nRet <= -1)
                                    return nRet;
                            }




                            KeyCollection newKeys = null;
                            KeyCollection oldKeys = null;
                            XmlDocument newDom = null;
                            XmlDocument oldDom = null;

                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.MergeKeys(strID,
                                strNewXml,
                                strOldXml,
                                true,
                                out newKeys,
                                out oldKeys,
                                out newDom,
                                out oldDom,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 处理检索点
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyKeys(connection,
                                newKeys,
                                oldKeys,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 处理子文件
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyFiles(connection,
                                strID,
                                newDom,
                                oldDom,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 4.用new更新data
                            // return:
                            //      -1  出错
                            //      >=0   成功 返回影响的记录数
                            nRet = this.UpdateDataField(connection,
                                strID,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 5.删除newdata字段
                            // return:
                            //		-1  出错
                            //		0   成功
                            nRet = this.DeleteDuoYuImage(connection,
                                strID,
                                "newdata",
                                0,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = "WriteXml() 在给'" + this.GetCaption("zh-cn") + "'库写入记录'" + strID + "'时出错,原因:" + sqlEx.Message;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "WriteXml() 在给'" + this.GetCaption("zh-cn") + "'库写入记录'" + strID + "'时出错,原因:" + ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally  // 记录锁
                {
                    //******对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录解写锁。");
#endif
                }
            }
            finally
            {
                //********对数据库解读锁****************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }


            // 当本函数被明知为账户库的写操作调用时, 一定要用bCheckAccount==false
            // 来调用，否则容易引起不必要的递归
            if (bFull == true
                && bCheckAccount == true
                && StringUtil.IsInList("account", this.TypeSafety) == true)
            {
                string strResPath = this.FullID + "/" + strID;

                this.container.UserColl.RefreshUserSafety(strResPath);
            }

            return 0;
        }

        // parameters:
        //      strRecorID   记录ID
        //      strObjectID  对象ID
        //      其它参数同WriteXml,无strOutputID参数
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录或对象资源不存在
        //      -6  权限不够
        //		0   成功
        public override int WriteObject(User user,
            string strRecordID,
            string strObjectID,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";
            int nRet = 0;



            if (user != null)
            {
                string strTempRecordPath = this.GetCaption("zh-cn") + "/" + strRecordID;
                string strExistRights = "";
                bool bHasRight = user.HasRights(strTempRecordPath,
                    ResType.Record,
                    "overwrite",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strTempRecordPath + "'记录没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }
            }

            //**********对数据库加读锁************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
			this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                string strOutputRecordID = "";
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.CanonicalizeRecordID(strRecordID,
                    out strOutputRecordID,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (strOutputRecordID == "-1")
                {
                    strError = "保存对象资源不支持记录号参数值为'" + strRecordID + "'。";
                    return -1;
                }
                strRecordID = strOutputRecordID;


                //**********对记录加写锁***************
                m_recordLockColl.LockForWrite(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录加写锁。");
#endif
                try // 记录锁
                {
                    // 打开连接对象
                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try // 连接
                    {
                        // 1.在对应的xml数据，用对象路径找到对象ID
                        string strXml;
                        // return:
                        //      -1  出错
                        //      -4  记录不存在
                        //      0   正确
                        nRet = this.GetXmlString(connection,
                            strRecordID,
                            out strXml,
                            out strError);
                        if (nRet <= -1)
                        {
                            strError = "保存'" + strRecordID + "/" + strObjectID + "'资源失败，原因:" + strError;
                            return nRet;
                        }
                        XmlDocument xmlDom = new XmlDocument();
                        xmlDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                        xmlDom.LoadXml(strXml);

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDom.NameTable);
                        nsmgr.AddNamespace("dprms", DpNs.dprms);
                        XmlNode fileNode = xmlDom.DocumentElement.SelectSingleNode("//dprms:file[@id='" + strObjectID + "']", nsmgr);
                        if (fileNode == null)
                        {
                            strError = "在数据xml里没有找到该ID对应的dprms:file节点";
                            return -1;
                        }

                        strObjectID = strRecordID + "_" + strObjectID;

                        // 2. 当记录为空记录时,用updata更改文本指针
                        if (this.IsEmptyObject(connection, strObjectID) == true)
                        {
                            // return
                            //		-1  出错
                            //		0   成功
                            nRet = this.UpdateObject(connection,
                                strObjectID,
                                out inputTimestamp,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        // 3.把数据写到range指定的范围
                        bool bFull = false;
                        // return:
                        //		-1	一般性错误
                        //		-2	时间戳不匹配
                        //		0	成功
                        nRet = this.WriteSqlRecord(connection,
                            strObjectID,
                            strRanges,
                            (int)lTotalLength,
                            baSource,
                            streamSource,
                            strMetadata,
                            strStyle,
                            inputTimestamp,
                            out outputTimestamp,
                            out bFull,
                            out strError);
                        if (nRet <= -1)
                            return nRet;



                        //string strCurrentRange = this.GetRange(connection,strObjectID);
                        if (bFull == true)  //覆盖完了
                        {
                            // 1. 用newdata替换data字段
                            // return:
                            //      -1  出错
                            //      >=0   成功 返回影响的记录数
                            nRet = this.UpdateDataField(connection,
                                strObjectID,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            // 2. 删除newdata字段
                            // return:
                            //		-1  出错
                            //		0   成功
                            nRet = this.DeleteDuoYuImage(connection,
                                strObjectID,
                                "newdata",
                                0,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        // 负责修改一下记录的时间戳
                        string strNewTimestamp = this.CreateTimestampForDb();
                        // return:
                        //      -1  出错
                        //      >=0   成功 返回被影响的记录数
                        nRet = this.SetTimestampForDb(connection,
                            strRecordID,
                            strNewTimestamp,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "WriteXml() 在给'" + this.GetCaption("zh-cn") + "'库写入资源'" + strObjectID + "'时出错,原因:" + ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //*********对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK_SQLDATABASE
					this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录解写锁。");
#endif

                }
            }
            finally
            {
                //************对数据库解读锁************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif

            }

            return 0;
        }
        // 给sql库写一条记录
        // 把baContent或streamContent写到image字段中range指定目标位置,
        // 说明：sql中的记录可以是Xml体记录也可以对象资源记录
        // parameters:
        //		connection	    连接对象	不能为null
        //		strID	        记录ID	不能为null或空字符串
        //		strRanges	    目标范围，多个范围用逗号分隔
        //		nTotalLength	记录内容总长度
        //						对于Sql Server目前只支持int，所以nTotalLength设为int类型，但对外接口是long
        //		baContent	    内容字节数组	可以为null
        //		streamContent	内容流	可以为null
        //		strStyle	    风格
        //					    ignorechecktimestamp	忽略时间戳
        //		baInputTimestamp    输入的时间戳	可以为null
        //		baOutputTimestamp	out参数，返回的时间戳
        //		bFull	        out参数，记录是否已满
        //		strError	    out参数，返回出错信息
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		0	成功
        // 说明	baContent与streamContent中谁有值就算谁
        private int WriteSqlRecord(SqlConnection connection,
            string strID,
            string strRanges,
            int nTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out bool bFull,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            bFull = false;

            int nRet = 0;

            //-------------------------------------------
            //对输入参数做例行检查
            //-------------------------------------------

            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection, out strError);
            if (nRet == -1)
            {
                strError = "WriteSqlRecord()调用错误，" + strError;
                return -1;
            }
            Debug.Assert(nRet == 0, "");

            if (strID == null || strID == "")
            {
                strError = "WriteSqlRecord()调用错误，strID参数不能为null或空字符串。";
                return -1;
            }
            if (nTotalLength < 0)
            {
                strError = "WriteSqlRecord()调用错误，nTotalLength参数值不能为'" + Convert.ToString(nTotalLength) + "'，必须大于等于0。";
                return -1;
            }
            if (baSource == null && streamSource == null)
            {
                strError = "WriteSqlRecord()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteSqlRecord()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";
            if (strRanges == null)
                strRanges = "";
            if (strMetadata == null)
                strMetadata = "";




            //-------------------------------------------
            //开始做事情
            //-------------------------------------------

            ////////////////////////////////////////////////////
            // 检查记录是否存在,时间是否匹配,并得到长度,range与textPtr
            /////////////////////////////////////////////////////
            string strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT TEXTPTR(newdata),"
                + " DataLength(newdata),"
                + " range,"
                + " dptimestamp,"
                + " metadata "
                + " FROM records "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            byte[] textPtr = null;
            string strOldMetadata = "";
            string strCurrentRange = "";
            int nCurrentLength = 0;
            string strOutputTimestamp = "";

            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                // 1.记录不存在报错
                if (dr == null
                    || dr.HasRows == false)
                {
                    strError = "记录'" + strID + "'在库中不存在，是不可能的情况";
                    return -1;
                }

                dr.Read();

                // 2.textPtr为null报错
                if (dr[0] is System.DBNull)
                {
                    strError = "TextPtr不可能为null";
                    return -1;
                }
                textPtr = (byte[])dr[0];

                // 3.时间戳不可能为null,时间戳不匹配报错
                if ((dr[4] is System.DBNull))
                {
                    strError = "时间戳不可能为null";
                    return -1;
                }

                // 当strStyle存在 ignorechecktimestamp时，不判断时间戳
                strOutputTimestamp = dr.GetString(3);
                baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                {
                    if (ByteArray.Compare(baInputTimestamp,
                        baOutputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }
                // 4.metadata为null报错
                if ((dr[4] is System.DBNull))
                {
                    strError = "Metadata不可能为null";
                    return -1;
                }
                strOldMetadata = dr.GetString(4);


                // 5.range为null的报错
                if ((dr[2] is System.DBNull))
                {
                    strError = "range此时也不可能为null";
                    return -1;
                }
                strCurrentRange = dr.GetString(2);

                // 6.取出长度
                nCurrentLength = dr.GetInt32(1);
            }
            finally
            {
                dr.Close();
            }

            bFull = false;
            bool bDeleted = false;

            long nSourceTotalLength = 0;
            if (baSource != null)
                nSourceTotalLength = baSource.Length;
            else
                nSourceTotalLength = streamSource.Length;

            // 根据range写数据
            RangeList rangeList = null;
            if (strRanges == "")
            {
                RangeItem rangeItem = new RangeItem();
                rangeItem.lStart = 0;
                rangeItem.lLength = nSourceTotalLength;
                rangeList = new RangeList();
                rangeList.Add(rangeItem);
            }
            else
            {
                rangeList = new RangeList(strRanges);
            }


            int nStartOfBuffer = 0;    // 缓冲区的位置
            int nState = 0;
            for (int i = 0; i < rangeList.Count; i++)
            {
                bool bCanDeleteDuoYu = false;  // 缺省不可能删除多余的长度

                RangeItem range = (RangeItem)rangeList[i];
                int nStartOfTarget = (int)range.lStart;     // 恢复到image字段的位置  
                int nNeedReadLength = (int)range.lLength;   // 需要读缓冲区的长度
                if (rangeList.Count == 1 && nNeedReadLength == 0)
                {
                    bFull = true;
                    break;
                }

                string strThisEnd = Convert.ToString(nStartOfTarget + nNeedReadLength - 1);

                string strThisRange = Convert.ToString(nStartOfTarget)
                    + "-" + strThisEnd;

                string strNewRange;
                nState = RangeList.MergContentRangeString(strThisRange,
                    strCurrentRange,
                    nTotalLength,
                    out strNewRange);
                if (nState == -1)
                {
                    strError = "MergContentRangeString() error";
                    return -1;
                }
                if (nState == 1)  //范围已满
                {
                    bFull = true;
                    string strFullEnd = "";
                    int nPosition = strNewRange.IndexOf('-');
                    if (nPosition >= 0)
                        strFullEnd = strNewRange.Substring(nPosition + 1);

                    // 当为范围的最后一次,且本次范围的末尾等于总范围的末尾,且还没有删除时
                    if (i == rangeList.Count - 1
                        && (strFullEnd == strThisEnd)
                        && bDeleted == false)
                    {
                        bCanDeleteDuoYu = true;
                        bDeleted = true;
                    }
                }
                strCurrentRange = strNewRange;

                // return:	
                //		-1  出错
                //		0   成功
                nRet = this.WriteImage(connection,
                    textPtr,
                    ref nCurrentLength,   // 当前image的长度在不断的变化着
                    bCanDeleteDuoYu,
                    strID,
                    "newdata",
                    nStartOfTarget,
                    baSource,
                    streamSource,
                    nStartOfBuffer,
                    nNeedReadLength,
                    out strError);
                if (nRet == -1)
                    return -1;
                nStartOfBuffer += nNeedReadLength;
            }

            if (bFull == true)
            {
                if (bDeleted == false)
                {
                    // 当记录覆盖满时，删除多余的值
                    // return:
                    //		-1  出错
                    //		0   成功
                    nRet = this.DeleteDuoYuImage(connection,
                        strID,
                        "newdata",
                        nTotalLength,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                strCurrentRange = "";
                nCurrentLength = nTotalLength;
            }
            else
            {
                nCurrentLength = -1;
            }

            // 最后,更新range,metadata,dptimestamp;

            // 得到组合后的Metadata;
            string strResultMetadata;
            // return:
            //		-1	出错
            //		0	成功
            nRet = DatabaseUtil.MergeMetadata(strOldMetadata,
                strMetadata,
                nCurrentLength,
                out strResultMetadata,
                out strError);
            if (nRet == -1)
                return -1;

            // 生成新的时间戳,保存到数据库里
            strOutputTimestamp = this.CreateTimestampForDb();

            strCommand = "use " + this.m_strSqlDbName + "\n"
                + " UPDATE records "
                + " SET dptimestamp=@dptimestamp,"
                + " range=@range,"
                + " metadata=@metadata "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            idParam = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strOutputTimestamp;

            SqlParameter rangeParam =
                command.Parameters.Add("@range",
                SqlDbType.NVarChar,
                4000);
            rangeParam.Value = strCurrentRange;

            SqlParameter metadataParam =
                command.Parameters.Add("@metadata",
                SqlDbType.NVarChar,
                4000);
            metadataParam.Value = strResultMetadata;

            int nCount = command.ExecuteNonQuery();
            if (nCount == 0)
            {
                strError = "没有更新到记录号为'" + strID + "'的时间戳,range,metadata";
                return -1;
            }
            baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            return 0;
        }

        // 写image字段的内容
        // 外面指供一个textprt指针
        // parameter:
        //		connection  连接对象
        //		textPtr     image指针
        //		nOldLength  原长度
        //		nDeleteDuoYu    是否删除多余
        //		strID           记录id
        //		strImageFieldName   image字段
        //		nStartOfTarget      目标的起始位置
        //		sourceBuffer    源大字节数组
        //		streamSource    源大流
        //		nStartOfBuffer  源流的起始位置
        //		nNeedReadLength 需要写的长度
        //		strError        out参数，返回出错信息
        // return:	
        //		-1  出错
        //		0   成功
        private int WriteImage(SqlConnection connection,
            byte[] textPtr,
            ref int nCurrentLength,           // 原来的长度     
            bool bDeleteDuoYu,
            string strID,
            string strImageFieldName,
            int nStartOfTarget,       // 目标的起始位置
            byte[] baSource,
            Stream streamSource,
            int nStartOfSource,     // 缓冲区的实际位置 必须 >=0 
            int nNeedReadLength,    // 需要读缓冲区的长度可能是-1,表示从源流nSourceStart位置到末尾
            out string strError)
        {
            strError = "";
            int nRet = 0;

            //---------------------------------------
            //例行检查输入参数
            //-----------------------------------------
            if (baSource == null && streamSource == null)
            {
                strError = "WriteImage()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteImage()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }

            int nSourceTotalLength = 0;
            if (baSource != null)
                nSourceTotalLength = baSource.Length;
            else
                nSourceTotalLength = (int)streamSource.Length;

            int nOutputLength = 0;
            // return:
            //		-1  出错
            //		0   成功
            nRet = DatabaseUtil.GetRealLength(nStartOfSource,
                nNeedReadLength,
                nSourceTotalLength,
                -1,//nMaxLength
                out nOutputLength,
                out strError);
            if (nRet == -1)
                return -1;


            //---------------------------------------
            //开始做事情
            //-----------------------------------------


            int chucksize = 32 * 1024;  //写库时每块为32K


            // 执行更新操作,使用UPDATETEXT语句

            // UPDATETEXT命令说明:
            // dest_text_ptr: 指向要更新的image 数据的文本指针的值（由 TEXTPTR 函数返回）必须为 binary(16)
            // insert_offset: 以零为基的更新起始位置,
            //				  对于image 列，insert_offset 是在插入新数据前从现有列的起点开始要跳过的字节数
            //				  开始于这个以零为基的起始点的现有 image 数据向右移，为新数据腾出空间。
            //				  值为 0 表示将新数据插入到现有位置的开始处。值为 NULL 则将新数据追加到现有数据值中。
            // delete_length: 是从 insert_offset 位置开始的、要从现有 image 列中删除的数据长度。
            //				  delete_length 值对于 text 和 image 列用字节指定，对于 ntext 列用字符指定。每个 ntext 字符占用 2 个字节。
            //				  值为 0 表示不删除数据。值为 NULL 则删除现有 text 或 image 列中从 insert_offset 位置开始到末尾的所有数据。
            // WITH LOG:      在 Microsoft? SQL Server? 2000 中被忽略。在该版本中，日志记录由数据库的有效恢复模型决定。
            // inserted_data: 是要插入到现有 text、ntext 或 image 列 insert_offset 位置的数据。
            //				  这是单个 char、nchar、varchar、nvarchar、binary、varbinary、text、ntext 或 image 值。
            //				  inserted_data 可以是文字或变量。
            // 如何使用UPDATETEXT命令?
            // 替换现有数据:  指定一个非空 insert_offset 值、非零 delete_length 值和要插入的新数据。
            // 删除现有数据:  指定一个非空 insert_offset 值、非零 delete_length 值。不指定要插入的新数据。
            // 插入新数据:    指定 insert_offset 值、为零的 delete_length 值和要插入的新数据。
            string strCommand = "use " + this.m_strSqlDbName + " "
                + " UPDATETEXT records." + strImageFieldName
                + " @dest_text_ptr"
                + " @insert_offset"
                + " @delete_length"
                + " WITH LOG"
                + " @inserted_data";   //不能加where语句

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            // 给参数赋值
            SqlParameter dest_text_ptrParam =
                command.Parameters.Add("@dest_text_ptr",
                SqlDbType.Binary,
                16);

            SqlParameter insert_offsetParam =
                command.Parameters.Add("@insert_offset",
                SqlDbType.Int);

            SqlParameter delete_lengthParam =
                command.Parameters.Add("@delete_length",
                SqlDbType.Int);

            SqlParameter inserted_dataParam =
                command.Parameters.Add("@inserted_data",
                SqlDbType.Binary,
                0);

            int insert_offset = nStartOfTarget; // 插入image字段的位置
            int nReadStartOfBuffer = nStartOfSource;         // 从源缓冲区中的读的起始位置
            Byte[] chuckBuffer = null; // 块缓冲区
            int nCount = 0;             // 影响的记录条数

            dest_text_ptrParam.Value = textPtr;

            while (true)
            {
                // 已从缓冲区读出的长度
                int nReadedLength = nReadStartOfBuffer - nStartOfSource;
                if (nReadedLength >= nNeedReadLength)
                    break;

                // 还需要读的长度
                int nContinueLength = nNeedReadLength - nReadedLength;
                if (nContinueLength > chucksize)  // 从源流中读的长度
                    nContinueLength = chucksize;

                inserted_dataParam.Size = nContinueLength;
                chuckBuffer = new byte[nContinueLength];

                if (baSource != null)
                {
                    // 拷到源数组的一段到每次用于写的chuckbuffer
                    Array.Copy(baSource,
                        nReadStartOfBuffer,
                        chuckBuffer,
                        0,
                        nContinueLength);
                }
                else
                {
                    streamSource.Read(chuckBuffer,
                        0,
                        nContinueLength);
                }


                if (chuckBuffer.Length <= 0)
                    break;

                insert_offsetParam.Value = insert_offset;

                // 删除字段的长度
                int nDeleteLength = 0;
                if (bDeleteDuoYu == true)  //最后一次
                {
                    nDeleteLength = nCurrentLength - insert_offset;  // 当前长度表示image的长度
                    if (nDeleteLength < 0)
                        nDeleteLength = 0;
                }
                else
                {
                    // 写入的长度超过当前最大长度时,要删除的长度为当前长度-start
                    if (insert_offset + chuckBuffer.Length > nCurrentLength)
                    {
                        nDeleteLength = nCurrentLength - insert_offset;
                        if (nDeleteLength < 0)
                            nDeleteLength = nCurrentLength;
                    }
                    else
                    {
                        nDeleteLength = chuckBuffer.Length;
                    }
                }
                delete_lengthParam.Value = nDeleteLength;
                inserted_dataParam.Value = chuckBuffer;

                nCount = command.ExecuteNonQuery();
                if (nCount == 0)
                {
                    strError = "没有更新到记录块";
                    return -1;
                }

                // 写入后,当前长度发生的变化
                nCurrentLength = nCurrentLength + chuckBuffer.Length - nDeleteLength;

                // 缓冲区的位置变化
                nReadStartOfBuffer += chuckBuffer.Length;

                // 目标的位置变化
                insert_offset += chuckBuffer.Length;   //恢复时要恢复到原来的位置

                if (chuckBuffer.Length < chucksize)
                    break;
            }

            return 0;
        }

        // return:
        //      -1  出错
        //      0   成功
        public int ModifyKeys(SqlConnection connection,
            KeyCollection keysAdd,
            KeyCollection keysDelete,
            out string strError)
        {
            strError = "";
            string strCommand = "";

            string strRecordID = "";
            if (keysAdd != null && keysAdd.Count > 0)
                strRecordID = ((KeyItem)keysAdd[0]).RecordID;
            else if (keysDelete != null && keysDelete.Count > 0)
                strRecordID = ((KeyItem)keysDelete[0]).RecordID;

            SqlCommand command = new SqlCommand("", connection);

            int i = 0;
            // int nCount = 0;
            int nNameIndex = 0;

            // 2006/12/8 把删除提前到增加以前
            if (keysDelete != null)
            {
                // 删除keys
                for (i = 0; i < keysDelete.Count; i++)
                {
                    KeyItem oneKey = (KeyItem)keysDelete[i];

                    string strKeysTableName = oneKey.SqlTableName;

                    string strIndex = Convert.ToString(nNameIndex++);

                    string strKeyParamName = "@key" + strIndex;
                    string strFromParamName = "@from" + strIndex;
                    string strIdParamName = "@id" + strIndex;
                    string strKeynumParamName = "@keynum" + strIndex;

                    strCommand += " DELETE FROM " + strKeysTableName
                        + " WHERE keystring = " + strKeyParamName + " AND fromstring= " + strFromParamName + " AND idstring= " + strIdParamName + " AND keystringnum= " + strKeynumParamName;

                    SqlParameter keyParam =
                        command.Parameters.Add(strKeyParamName,
                        SqlDbType.NVarChar);
                    keyParam.Value = oneKey.Key;

                    SqlParameter fromParam =
                        command.Parameters.Add(strFromParamName,
                        SqlDbType.NVarChar);
                    fromParam.Value = oneKey.FromValue;

                    SqlParameter idParam =
                        command.Parameters.Add(strIdParamName,
                        SqlDbType.NVarChar);
                    idParam.Value = oneKey.RecordID;

                    SqlParameter keynumParam =
                        command.Parameters.Add(strKeynumParamName,
                        SqlDbType.NVarChar);
                    keynumParam.Value = oneKey.Num;
                }
            }

            if (keysAdd != null)
            {
                // nCount = keysAdd.Count;

                // 增加keys
                for (i = 0; i < keysAdd.Count; i++)
                {
                    KeyItem oneKey = (KeyItem)keysAdd[i];

                    string strKeysTableName = oneKey.SqlTableName;

                    // string strIndex = Convert.ToString(i);
                    string strIndex = Convert.ToString(nNameIndex++);

                    string strKeyParamName = "@key" + strIndex;
                    string strFromParamName = "@from" + strIndex;
                    string strIdParamName = "@id" + strIndex;
                    string strKeynumParamName = "@keynum" + strIndex;


                    //加keynum
                    strCommand += " INSERT INTO " + strKeysTableName
                        + " (keystring,fromstring,idstring,keystringnum) "
                        + " VALUES(" + strKeyParamName + ","
                        + strFromParamName + ","
                        + strIdParamName + ","
                        + strKeynumParamName + ")";
                    //+ " VALUES(@key,@from,@id,@keynum)";

                    SqlParameter keyParam =
                        command.Parameters.Add(strKeyParamName,
                        SqlDbType.NVarChar);
                    keyParam.Value = oneKey.Key;

                    SqlParameter fromParam =
                        command.Parameters.Add(strFromParamName,
                        SqlDbType.NVarChar);
                    fromParam.Value = oneKey.FromValue;

                    SqlParameter idParam =
                        command.Parameters.Add(strIdParamName,
                        SqlDbType.NVarChar);
                    idParam.Value = oneKey.RecordID;

                    SqlParameter keynumParam =
                        command.Parameters.Add(strKeynumParamName,
                        SqlDbType.NVarChar);
                    keynumParam.Value = oneKey.Num;

                }
            }




            if (strCommand != "")
            {
                strCommand = "use " + this.m_strSqlDbName + " \n"
                    + strCommand
                    + " use master " + "\n";
                command.CommandText = strCommand;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "创建检索点出错,记录路径'" + this.GetCaption("zh-cn") + "/" + strRecordID + "，原因：" + ex.Message;
                    return -1;
                }
            }
            return 0;
        }


        // 处理子文件
        // return:
        //      -1  出错
        //      0   成功
        public int ModifyFiles(SqlConnection connection,
            string strID,
            XmlDocument newDom,
            XmlDocument oldDom,
            out string strError)
        {
            strError = "";
            strID = DbPath.GetID10(strID);

            // 新文件
            ArrayList aNewFileID = new ArrayList();
            if (newDom != null)
            {
                XmlNamespaceManager newNsmgr = new XmlNamespaceManager(newDom.NameTable);
                newNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList newFileList = newDom.SelectNodes("//dprms:file", newNsmgr);
                foreach (XmlNode newFileNode in newFileList)
                {
                    string strNewFileID = DomUtil.GetAttr(newFileNode,
                        "id");
                    if (strNewFileID != "")
                        aNewFileID.Add(strNewFileID);
                }
            }

            // 旧文件
            ArrayList aOldFileID = new ArrayList();
            if (oldDom != null)
            {
                XmlNamespaceManager oldNsmgr = new XmlNamespaceManager(oldDom.NameTable);
                oldNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList oldFileList = oldDom.SelectNodes("//dprms:file", oldNsmgr);
                foreach (XmlNode oldFileNode in oldFileList)
                {
                    string strOldFileID = DomUtil.GetAttr(oldFileNode,
                        "id");
                    if (strOldFileID != "")
                        aOldFileID.Add(strOldFileID);
                }
            }
            //数据必须先排序
            aNewFileID.Sort(new ComparerClass());
            aOldFileID.Sort(new ComparerClass());


            List<string> targetLeft = new List<string>();
            List<string> targetMiddle = new List<string>();
            List<string> targetRight = new List<string>();

            //新旧两个File数组碰
            ArrayListUtil.MergeStringArray(aNewFileID,
                aOldFileID,
                targetLeft,
                targetMiddle,
                targetRight);

            string strCommand = "";
            SqlCommand command = new SqlCommand("", connection);

            int nCount = 0;
            //删除旧文件
            if (targetRight.Count > 0)
            {
                nCount = targetRight.Count;

                for (int i = 0; i < targetRight.Count; i++)
                {
                    string strPureObjectID = targetRight[i];
                    string strObjectID = strID + "_" + strPureObjectID;

                    string strParamIDName = "@id" + Convert.ToString(i);
                    strCommand += " DELETE FROM records WHERE id = " + strParamIDName + " \n";
                    SqlParameter idParam =
                        command.Parameters.Add(strParamIDName,
                        SqlDbType.NVarChar);
                    idParam.Value = strObjectID;
                }
            }

            // 创建新文件
            if (targetLeft.Count > 0)
            {
                for (int i = 0; i < targetLeft.Count; i++)
                {
                    string strPureObjectID = targetLeft[i];
                    string strObjectID = strID + "_" + strPureObjectID;

                    string strParamIDName = "@id" + Convert.ToString(i) + nCount;
                    strCommand += " INSERT INTO records(id) "
                        + " VALUES(" + strParamIDName + "); \n";
                    SqlParameter idParam =
                        command.Parameters.Add(strParamIDName,
                        SqlDbType.NVarChar);
                    idParam.Value = strObjectID;
                }
            }

            if (strCommand != "")
            {
                strCommand = "use " + this.m_strSqlDbName + " \n"
                    + strCommand
                    + " use master " + "\n";

                command.CommandText = strCommand;
                command.CommandTimeout = 10 * 60; // 10分钟

                int nResultCount = 0;
                try
                {
                    nResultCount = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "处理记录路径为'" + this.GetCaption("zh") + "/" + strID + "'的子文件发生错误:" + ex.Message + ",sql命令:\r\n" + strCommand;
                    return -1;
                }

                if (nResultCount != targetRight.Count + targetLeft.Count)
                {
                    this.container.WriteErrorLog("希望处理的文件数'" + Convert.ToString(targetRight.Count + targetLeft.Count) + "'个，实际删除的文件数'" + Convert.ToString(nResultCount) + "'个");
                }
            }

            return 0;
        }



        // 检索连接对象是否正确
        // return:
        //      -1  出错
        //      0   正常
        private int CheckConnection(SqlConnection connection,
            out string strError)
        {
            strError = "";
            if (connection == null)
            {
                strError = "connection为null";
                return -1;
            }
            if (connection.State != ConnectionState.Open)
            {
                strError = "connection没有打开";
                return -1;
            }
            return 0;
        }


        // 得到范围
        private string GetRange(SqlConnection connection,
            string strID)
        {
            string strRange = "";

            string strCommand = "use " + this.m_strSqlDbName + " "
                + "select range from records where id='" + strID + "'";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr != null && dr.HasRows == true)
                {
                    dr.Read();
                    strRange = dr.GetString(0);
                    if (strRange == null)
                        strRange = "";
                }
            }
            finally
            {
                dr.Close();
            }

            return strRange;
        }


        // 更新对象,使image字段获得有效的TextPrt指针
        // return
        //		-1  出错
        //		0   成功
        private int UpdateObject(SqlConnection connection,
            string strObjectID,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            SqlCommand command = null;

            string strOutputTimestamp = this.CreateTimestampForDb();

            strCommand = "use " + this.m_strSqlDbName + " "
                + " UPDATE records "
                + " set newdata=0x0,range='0-0',dptimestamp=@dptimestamp,metadata=@metadata "
                + " where id='" + strObjectID + "'";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            string strMetadata = "<file size='0'/>";
            SqlParameter metadataParam =
                command.Parameters.Add("@metadata",
                SqlDbType.NVarChar);
            metadataParam.Value = strMetadata;


            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strOutputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount <= 0)
            {
                strError = "没有更新'" + strObjectID + "'记录";
                return -1;
            }
            // 返回的时间戳
            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            return 0;
        }

        // 判断一个对对象资源否是空对象
        private bool IsEmptyObject(SqlConnection connection,
            string strID)
        {
            return this.IsEmptyObject(connection,
                "newdata",
                strID);
        }

        // 判断一个对对象资源否是空对象
        private bool IsEmptyObject(SqlConnection connection,
            string strImageFieldName,
            string strID)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                throw (new Exception(strError));

            string strCommand = "";
            SqlCommand command = null;
            strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT @Pointer=TEXTPTR(" + strImageFieldName + ") "
                + " FROM records "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);
            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter PointerOutParam =
                command.Parameters.Add("@Pointer",
                SqlDbType.VarBinary,
                100);
            PointerOutParam.Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            if (PointerOutParam == null
                || PointerOutParam.Value is System.DBNull)
            {
                return true;
            }
            return false;
        }

        // 插入一条新记录,使其获得有效的textptr,包装InsertRecord
        private int InsertRecord(SqlConnection connection,
            string strID,
            out byte[] outputTimestamp,
            out string strError)
        {
            return this.InsertRecord(connection,
                strID,
                "newdata",
                new byte[] { 0x0 },
                out outputTimestamp,
                out strError);
        }

        // 给表中插入一条记录
        private int InsertRecord(SqlConnection connection,
            string strID,
            string strImageFieldName,
            byte[] sourceBuffer,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            SqlCommand command = null;

            string strRange = "0-" + Convert.ToString(sourceBuffer.Length - 1);
            string strOutputTimestamp = this.CreateTimestampForDb();

            strCommand = "use " + this.m_strSqlDbName + " "
                + " INSERT INTO records(id," + strImageFieldName + ",range,metadata,dptimestamp) "
                + " VALUES(@id,@data,@range,@metadata,@dptimestamp);";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter dataParam =
                command.Parameters.Add("@data",
                SqlDbType.Binary,
                sourceBuffer.Length);
            dataParam.Value = sourceBuffer;

            SqlParameter rangeParam =
                command.Parameters.Add("@range",
                SqlDbType.NVarChar);
            rangeParam.Value = strRange;

            string strMetadata = "<file size='0'/>";
            SqlParameter metadataParam =
                command.Parameters.Add("@metadata",
                SqlDbType.NVarChar);
            metadataParam.Value = strMetadata;

            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strOutputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount <= 0)
            {
                strError = "InsertImage() SQL命令执行影响的条数为" + Convert.ToString(nCount);
                return -1;
            }

            // 返回的时间戳
            outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            return 0;
        }


        // 用newdata字段替换data字段
        // parameters:
        //      connection  SqlConnection对象
        //      strID       记录id
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回影响的记录数
        // 线: 不安全
        private int UpdateDataField(SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";
            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + " "
                + " UPDATE records \n"
                + " SET data=newdata \n"
                + " WHERE id='" + strID + "'";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            command.CommandTimeout = 5 * 60;  // 30分钟

            int nCount = command.ExecuteNonQuery();
            if (nCount == -1)
            {
                strError = "没有替换到该记录'" + strID + "'的data字段";
                return -1;
            }

            return nCount;
        }

        // 删除image多余的部分
        // parameter:
        //		connection  连接对象
        //		strID       记录ID
        //		strImageFieldName   image字段名
        //		nStart      起始位置
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 不安全
        private int DeleteDuoYuImage(SqlConnection connection,
            string strID,
            string strImageFieldName,
            int nStart,
            out string strError)
        {
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            SqlCommand command = null;

            // 1.得到image指针 和 长度
            strCommand = "use " + this.m_strSqlDbName + " "
                + " SELECT @Pointer=TEXTPTR(" + strImageFieldName + "),"
                + " @Length=DataLength(" + strImageFieldName + ") "
                + " FROM records "
                + " WHERE id=@id";

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);
            command.CommandTimeout = 20 * 60;  // 20分钟

            SqlParameter idParam =
                command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter PointerOutParam =
                command.Parameters.Add("@Pointer",
                SqlDbType.VarBinary,
                100);
            PointerOutParam.Direction = ParameterDirection.Output;

            SqlParameter LengthOutParam =
                command.Parameters.Add("@Length",
                SqlDbType.Int);
            LengthOutParam.Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();
            if (PointerOutParam == null)
            {
                strError = "没找到image指针";
                return -1;
            }

            int nTotalLength = (int)LengthOutParam.Value;
            if (nStart >= nTotalLength)
                return 0;


            // 2.进行删除
            strCommand = "use " + this.m_strSqlDbName + " "
                + " UPDATETEXT records." + strImageFieldName
                + " @dest_text_ptr"
                + " @insert_offset"
                + " NULL"  //@delete_length"
                + " WITH LOG";
            //+ " @inserted_data";   //不能加where语句

            strCommand += " use master " + "\n";

            command = new SqlCommand(strCommand,
                connection);

            // 给参数赋值
            SqlParameter dest_text_ptrParam =
                command.Parameters.Add("@dest_text_ptr",
                SqlDbType.Binary,
                16);

            SqlParameter insert_offsetParam =
                command.Parameters.Add("@insert_offset",
                SqlDbType.Int);


            dest_text_ptrParam.Value = PointerOutParam.Value;
            insert_offsetParam.Value = nStart;

            command.ExecuteNonQuery();

            return 0;
        }

        // 检查记录在库中是否存在
        // return:
        //		-1  出错
        //      0   不存在
        //      1   存在
        private int RecordIsExist(SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";

            // 检查连接对象
            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + " "
                + " SET NOCOUNT OFF;"
                + "select id from records where id='" + strID + "'";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr != null && dr.HasRows == true)
                    return 1;
            }
            finally
            {
                dr.Close();
            }
            return 0;
        }

        // 从库中得到一个记录的时间戳
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        private int GetTimestampFromDb(SqlConnection connection,
            string strID,
            out byte[] outputTimestamp,
            out string strError)
        {
            strError = "";
            outputTimestamp = null;
            int nRet = 0;

            string strOutputRecordID = "";
            // return:
            //      -1  出错
            //      0   成功
            nRet = this.CanonicalizeRecordID(strID,
                out strOutputRecordID,
                out strError);
            if (nRet == -1)
            {
                strError = "GetTimestampFormDb()调用错误，strID参数值'" + strID + "'不合法。";
                return -1;
            }
            if (strOutputRecordID == "-1")
            {
                strError = "GetTimestampFormDb()调用错误，strID参数值'" + strID + "'不合法。";
                return -1;
            }
            strID = strOutputRecordID;


            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + " "
                + "select dptimestamp "
                + " from records "
                + " where id='" + strID + "'";

            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
            try
            {
                if (dr == null
                    || dr.HasRows == false)
                {
                    strError = "GetTimestampFromDb() 记录'" + strID + "'在库中不存在";
                    return -4;
                }
                dr.Read();
                string strOutputTimestamp = dr.GetString(0);
                outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);//Encoding.UTF8.GetBytes(strOutputTimestamp);
            }
            finally
            {
                dr.Close();
            }
            return 0;
        }

        // 获取记录的时间戳
        // parameters0:
        //      strID   记录id
        //      baOutputTimestamp
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        public override int GetTimestampFromDb(
            string strID,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            // 打开连接对象
            SqlConnection connection = new SqlConnection(this.m_strConnString);
            connection.Open();
            try
            {
                // return:
                //		-1  出错
                //		-4  未找到记录
                //      0   成功
                return this.GetTimestampFromDb(connection,
                    strID,
                    out baOutputTimestamp,
                    out strError);
            }
            finally
            {
                connection.Close();
            }
        }


        // 设置指定记录的时间戳
        // parameters:
        //      connection  SqlConnection对象
        //      strID       记录id，可以是记录也可以是资源
        //      strInputTimestamp   输入的时间戳
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回被影响的记录数
        private int SetTimestampForDb(SqlConnection connection,
            string strID,
            string strInputTimestamp,
            out string strError)
        {
            strError = "";

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "use " + this.m_strSqlDbName + "\n"
                + " UPDATE records "
                + " SET dptimestamp=@dptimestamp"
                + " WHERE id=@id";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);

            SqlParameter idParam = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            idParam.Value = strID;

            SqlParameter dptimestampParam =
                command.Parameters.Add("@dptimestamp",
                SqlDbType.NVarChar,
                100);
            dptimestampParam.Value = strInputTimestamp;

            int nCount = command.ExecuteNonQuery();
            if (nCount == 0)
            {
                strError = "没有更新到记录号为'" + strID + "'的时间戳";
                return -1;
            }
            return nCount;
        }

        // 删除记录,包括子文件,检索点,和本记录
        // parameter:
        //		strRecordID           记录ID
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp out参数,返回的实际的时间戳
        //		strError        out参数,返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全
        public override int DeleteRecord(string strRecordID,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;

            strRecordID = DbPath.GetID10(strRecordID);

            //********对数据库加读锁*********************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE		
			this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif

            int nRet = 0;
            try
            {
                //*********对记录加写锁**********
                m_recordLockColl.LockForWrite(strRecordID, m_nTimeOut);
#if DEBUG_LOCK_SQLDATABASE
				this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录加写锁。");
#endif
                try
                {
                    SqlConnection connection = new SqlConnection(this.m_strConnString);
                    connection.Open();
                    try
                    {
                        // 比较时间戳
                        // return:
                        //		-1  出错
                        //		-4  未找到记录
                        //      0   成功
                        nRet = this.GetTimestampFromDb(connection,
                            strRecordID,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        if (baOutputTimestamp == null)
                        {
                            strError = "服务器取出的时间戳为null";
                            return -1;
                        }

                        if (ByteArray.Compare(baInputTimestamp,
                            baOutputTimestamp) != 0)
                        {
                            strError = "时间戳不匹配";
                            return -2;
                        }

                        //bool bXmlError = false;
                        string strXml;
                        // return:
                        //      -1  出错
                        //      -4  记录不存在
                        //      0   正确
                        nRet = this.GetXmlString(connection,
                            strRecordID,
                            out strXml,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        XmlDocument newDom = null;
                        XmlDocument oldDom = null;

                        KeyCollection newKeys = null;
                        KeyCollection oldKeys = null;
                        // 1.删除检索点

                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = this.MergeKeys(strRecordID,
                            "",
                            strXml,
                            true,
                            out newKeys,
                            out oldKeys,
                            out newDom,
                            out oldDom,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (oldDom != null)
                        {
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyKeys(connection,
                                null,
                                oldKeys,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else
                        {
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ForceDeleteKeys(connection,
                                strRecordID,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        // 2.删除子文件
                        if (oldDom != null)
                        {
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ModifyFiles(connection,
                                strRecordID,
                                null,
                                oldDom,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else
                        {

                            // 通过记录号之间的关系强制删除
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = this.ForceDeleteFiles(connection,
                                strRecordID,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        // 3.删除自己,返回删除的记录数
                        // return:
                        //      -1  出错
                        //      >=0   成功 返回删除的记录数
                        nRet = DeleteRecordByID(connection,
                            strRecordID,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (nRet == 0)
                        {
                            strError = "删除记录时,从库中没找到记录号为'" + strRecordID + "'的记录";
                            return -1;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Errors is SqlErrorCollection)
                            strError = "数据库'" + this.GetCaption("zh") + "'尚未初始化。";
                        else
                            strError = sqlEx.Message;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "删除'" + this.GetCaption("zh-cn") + "'库中id为'" + strRecordID + "'的记录时出错,原因:" + ex.Message;
                        return -1;
                    }
                    finally // 连接
                    {
                        connection.Close();
                    }
                }
                finally // 记录锁
                {
                    //**************对记录解写锁**********
                    m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK_SQLDATABASE			
					this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录解写锁。");
#endif

                }
            }
            finally
            {
                //***************对数据库解读锁*****************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK_SQLDATABASE		
				this.container.WriteDebugInfo("DeleteRecordForce()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif

            }
            return 0;
        }


        // 根据记录号之间的关系(记录号~~记录号_0),强制删除资源文件
        // parameters:
        //      connection  SqlConnection对象
        //      strRecordID 记录id  必须是10位
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        private int ForceDeleteFiles(SqlConnection connection,
            string strRecordID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 连接连接
            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(strRecordID != null && strRecordID.Length == 10, "ForceDeleteFiles()调用错误，strRecordID参数值不能为null且长度必须等于10位。");

            string strCommand = "use " + this.m_strSqlDbName + " "
                + " DELETE FROM records WHERE id like @id";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            SqlParameter param = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            param.Value = strRecordID + "_%";

            //???如果处理删除数量
            int nDeletedCount = command.ExecuteNonQuery();

            return 0;
        }

        // 强制删除记录对应的检索点,检查所有的表
        // parameters:
        //      connection  SqlConnection连接对象
        //      strRecordID 记录id,调之前必须设为10
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线: 不安全
        public int ForceDeleteKeys(SqlConnection connection,
            string strRecordID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // return:
            //      -1  出错
            //      0   正常
            nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert((strRecordID != null) && (strRecordID.Length == 10), "ForceDeleteKeys()调用错误，strRecordID参数值不能为null且长度必须等于10位。");

            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;
            if (keysCfg == null)
                return 0;

            List<TableInfo> aTableInfo = null;
            nRet = keysCfg.GetTableInfosRemoveDup(
                out aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            string strCommand = "";
            for (int i = 0; i < aTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTableInfo[i];

                string strTempID = "@id" + Convert.ToString(i);
                strCommand += "DELETE FROM " + tableInfo.SqlTableName
                    + " WHERE idstring=" + strTempID + "\r\n";
            }

            if (strCommand != "")
            {
                strCommand = "use " + this.m_strSqlDbName + " \r\n"
                    + strCommand
                    + "use master " + "\r\n";

                SqlCommand command = new SqlCommand(strCommand,
                    connection);

                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    string strTempID = "@id" + Convert.ToString(i);
                    SqlParameter idParam = command.Parameters.Add(strTempID,
                        SqlDbType.NVarChar);
                    idParam.Value = strRecordID;
                }

                // ????如果处理删除数量
                int nDeletedCount = command.ExecuteNonQuery();
            }
            return 0;
        }

        // 从库中删除指定的记录,可以是记录也可以是资源
        // parameters:
        //      connection  连接对象
        //      strID       记录id
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      >=0   成功 返回删除的记录数
        private int DeleteRecordByID(
            SqlConnection connection,
            string strID,
            out string strError)
        {
            strError = "";

            Debug.Assert(connection != null, "DeleteRecordById()调用错误，connection参数值不能为null。");
            Debug.Assert(strID != null, "DeleteRecordById()调用错误，strID参数值不能为null。");
            Debug.Assert(strID.Length >= 10, "DeleteRecordByID()调用错误 strID参数值的长度必须大于等于10。");

            // return:
            //      -1  出错
            //      0   正常
            int nRet = this.CheckConnection(connection,
                out strError);
            if (nRet == -1)
                return -1;


            string strCommand = "use " + this.m_strSqlDbName + " "
                + " DELETE FROM records WHERE id = @id";
            strCommand += " use master " + "\n";

            SqlCommand command = new SqlCommand(strCommand,
                connection);
            command.CommandTimeout = 10 * 60;// 10分钟

            SqlParameter param = command.Parameters.Add("@id",
                SqlDbType.NVarChar);
            param.Value = strID;

            int nDeletedCount = command.ExecuteNonQuery();

            if (nDeletedCount != 1)
            {
                this.container.WriteErrorLog("希望删除" + strID + " '1'条，实际删除'" + Convert.ToString(nDeletedCount) + "'个");
            }

            return nDeletedCount;
        }

    }
}
