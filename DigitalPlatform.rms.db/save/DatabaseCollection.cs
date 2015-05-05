//#define DEBUG_LOCK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.ResultSet;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Range;

namespace DigitalPlatform.rms
{
    // 数据库集合
    public class DatabaseCollection : ArrayList
    {
        // 帐户集合指针,用于修改帐户库记录时，刷新当前帐户
        public UserCollection UserColl = null;
        public bool Changed = false;	//内容是否发生改变

        public XmlNode NodeDbs = null;  //<dbs>节点
        public string DataDir = "";	// 程序目录路径
        public string InstanceName = ""; // 服务器实例名

        public string BinDir = "";//Bin目录，为脚本引用dll服务 2006/3/21加

        private ReaderWriterLock m_lock = new ReaderWriterLock();
        private int m_nTimeOut = 1000 * 60;	//1分钟

        private string m_strLogFileName = "";	//日志文件名称
        private string m_strDbsCfgFilePath = "";	// 库配置文件名
        private XmlDocument m_dom = null;	// 库配置文件dom

        // parameter:
        //		strDataDir	data目录
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        // 线: 安全的
        public int Initial(string strDataDir,
            string strBinDir,
            out string strError)
        {
            strError = "";

            //**********对库集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("Initial()，对库集合加写锁。");
#endif
            try
            {
                if (String.IsNullOrEmpty(strBinDir) == true)
                {
                    strError = "Initial()，strBinDir参数值不能为null或空字符串。";
                    return -1;
                }
                this.BinDir = strBinDir;

                if (String.IsNullOrEmpty(strDataDir) == true)
                {
                    strError = "Initial()，strDataDir参数值不能为null或空字符串。";
                    return -1;
                }

                this.DataDir = strDataDir;

                // 日志文件
                string strLogDir = this.DataDir + "\\log";
                try
                {
                    PathUtil.CreateDirIfNeed(strLogDir);
                }
                catch (Exception ex)
                {
                    DirectoryInfo di = new DirectoryInfo(this.DataDir);
                    if (di.Exists == false)
                        strError = "创建日志目录出错: '" + ex.Message + "', 原因是上级目录 '" +this.DataDir+ "' 不存在...";
                    else
                        strError = "创建日志目录出错: " + ex.Message;
                    return -1;
                }
                this.m_strLogFileName = strLogDir + "\\log.txt";

                // databases.xml配置文件
                this.m_strDbsCfgFilePath = this.DataDir + "\\databases.xml";

                this.m_dom = new XmlDocument();
                //this.m_dom.PreserveWhitespace = true; //保存空白
                try
                {
                    this.m_dom.Load(this.m_strDbsCfgFilePath);
                }
                catch (Exception ex)
                {
                    strError = "加载" + this.m_strDbsCfgFilePath + "到dom时出错 " + ex.Message;
                    return -1;
                }

                //得到数据库节点列表
                this.NodeDbs = m_dom.SelectSingleNode(@"/root/dbs");
                if (this.NodeDbs == null)
                {
                    strError = "databases.xml配置文件中不存在<dbs>节点，文件不合法，必须至少存在的一个用户库。";
                    return -1;
                }

                this.InstanceName = DomUtil.GetAttr(this.NodeDbs, "instancename");

                // 先清空
                this.Clear();

                // 根据<database>节点创建Database对象
                int nRet = 0;
                XmlNodeList listDb = this.NodeDbs.SelectNodes("database");
                foreach (XmlNode nodeDb in listDb)
                {
                    // return:
                    //      -1  出错
                    //      0   成功
                    // 线：不安全
                    nRet = this.AddDatabase(nodeDb,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                this.WriteErrorLog("创建数据库对象完毕。");

                // 检验各个数据库记录尾号
                // return:
                //      -1  出错
                //      0   成功
                // 线：不安全
                nRet = this.CheckDbsTailNo(out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                //***********对库集合解写锁****************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("Initial()，对库集合解写锁。");
#endif
            }
        }


        // 根据node节点创建Database数据库对象，加到集合里
        // parameters:
        //      node    <database>节点
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全
        public int AddDatabase(XmlNode node,
            out string strError)
        {
            Debug.Assert(node != null, "AddDatabase()调用错误，node参数值为能为null。");
            Debug.Assert(String.Compare(node.Name, "database", true) == 0, "AddDatabase()调用错误，node参数值必须为<database>节点。");

            strError = "";

            string strType = DomUtil.GetAttr(node, "type").Trim();

            Database db = null;

            // file类型创建为FileDatabase对象，其它创建为SqlDatabase对象
            if (StringUtil.IsInList("file", strType, true) == true)
                db = new FileDatabase(this);
            else
                db = new SqlDatabase(this);

            // return:
            //		-1  出错
            //		0   成功
            int nRet = db.Initial(node,
                out strError);
            if (nRet == -1)
                return -1;

            this.Add(db);

            return 0;
        }

        // 析构函数
        ~DatabaseCollection()
        {
            /*
            this.Close();
            this.WriteErrorLog("析构DatabaseCollection对象完成。");
             */
        }

        public void Close()
        {
            // 保存内存对象到文件
            this.SaveXmlSafety();
        }


        // 把错误信息写到日志文件里
        public void WriteErrorLog(string strText)
        {
            string strTime = DateTime.Now.ToString();

            try
            {
                StreamUtil.WriteText(this.m_strLogFileName,
                     strTime + " " + strText + "\r\n");
            }
            catch
            {
                // 有可能手工把文件删除了，导致文件不存在，抛出异常。
            }
        }

        // 把错误信息写到日志文件里
        public void WriteDebugInfo(string strText)
        {
            string strTime = DateTime.Now.ToString();

            StreamUtil.WriteText(this.DataDir + "\\debug.txt",
                 strTime + " " + strText + "\r\n");
        }

        // 检验各个数据库记录尾号
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全
        public int CheckDbsTailNo(out string strError)
        {
            strError = "";

            this.WriteErrorLog("走到CheckDbsTailNo()，开始校验数据库尾号。");

            int nRet = 0;

            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    Database db = (Database)this[i];
                    nRet = db.CheckTailNo(out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 保存内存对象
                this.SaveXml();
            }
            catch (Exception ex)
            {
                strError = "CheckDbsTailNo()出错，原因：" + ex.Message;
                return -1;
            }

            return 0;
        }


        // 把内存dom保存到databases.xml配置文件
        // 一部分节点不变，一部分节点被覆盖
        // 线: 不安全
        public void SaveXml()
        {
            if (this.Changed == true)
            {
                XmlTextWriter w = new XmlTextWriter(this.m_strDbsCfgFilePath,
                    Encoding.UTF8);
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                m_dom.WriteTo(w);
                w.Close();

                this.Changed = false;

                this.WriteErrorLog("完成保存内存dom到'" + this.m_strDbsCfgFilePath + "'文件。");
            }
        }

        // SaveXml()的安全版本
        public void SaveXmlSafety()
        {
            //******************对库集合加写锁******
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("SaveXmlSafety()，对库集合加写锁。");
#endif
            try
            {
                this.SaveXml();
            }
            finally
            {
                m_lock.ReleaseWriterLock();
                //*************对库集合解写锁***********
#if DEBUG_LOCK
				this.WriteDebugInfo("SaveXmlSafety()，对库集合解写锁。");
#endif
            }
        }

        // 获得一个用户拥有的(dbo)全部数据库名
        public int GetOwnerDbNames(string strUserName,
            out List<string> aOwnerDbName,
            out string strError)
        {
            strError = "";

            aOwnerDbName = new List<string>();

            //******************对库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetOwnerDbNames()，对库集合加读锁。");
#endif
            try
            {

                foreach (Database db in this)
                {
                    if (db.DboSafety == strUserName)
                    {
                        aOwnerDbName.Add(db.GetCaptionSafety(null));
                    }
                }

                return 0;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("GetOwnerDbNames()，对库集合解读锁。");
#endif
            }

        }

        // 新建数据库
        // parameter:
        //		user	            帐户对象
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout
        //		strSqlDbName    	指定的Sql数据库名称,可以为null，系统自动生成一个,，如果数据库为文为文件型数据库，则认作数据源目录的名称
        //		strKeysDefault  	keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //      -3	在新建库中，发现已经存在同名数据库, 本次不能创建
        //      -2	没有足够的权限
        //      -1	一般性错误，例如输入参数不合法等
        //      0	操作成功
        public int CreateDb(User user,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDefault,
            string strBrowseDefault,
            out string strError)
        {
            strError = "";

            if (strKeysDefault == null)
                strKeysDefault = "";
            if (strBrowseDefault == null)
                strBrowseDefault = "";

            if (strKeysDefault != "")
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strKeysDefault);
                }
                catch (Exception ex)
                {
                    strError = "加载keys配置文件内容到dom出错，原因:" + ex.Message;
                    return -1;
                }
            }
            if (strBrowseDefault != "")
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strBrowseDefault);
                }
                catch (Exception ex)
                {
                    strError = "加载browse配置文件内容到dom出错，原因:" + ex.Message;
                    return -1;
                }
            }

            string strEnLoginName = "";

            // 可以一个逻辑库名也没有，不出错
            string strLogicNames = "";
            for (int i = 0; i < logicNames.Length; i++)
            {
                string strLang = logicNames[i].Lang;
                string strLogicName = logicNames[i].Value;

                if (strLang.Length != 2
                    && strLang.Length != 5)
                {
                    strError = "语言版本字符串长度只能是2位或者5位,'" + strLang + "'语言版本不合法";
                    return -1;
                }

                if (this.IsExistLogicName(strLogicName, null) == true)
                {
                    strError = "数据库中已存在'" + strLogicName + "'逻辑库名";
                    return -3;  // 已存在相同数据库名
                }
                strLogicNames += "<caption lang='" + strLang + "'>" + strLogicName + "</caption>";
                if (String.Compare(logicNames[i].Lang.Substring(0, 2), "en", true) == 0)
                    strEnLoginName = strLogicName;
            }
            strLogicNames = "<logicname>" + strLogicNames + "</logicname>";

            // 检查当前帐户是否有创建数据库的权限
            string strTempDbName = "test";
            if (logicNames.Length > 0)
                strTempDbName = logicNames[0].Value;
            string strExistRights = "";
            bool bHasRight = user.HasRights(strTempDbName,
                ResType.Database,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对数据库没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                return -2;  // 权限不够
            }

            //**********对库集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("CreateDb()，对库集合加写锁。");
#endif
            try
            {
                if (strType == null)
                    strType = "";

                // 得到库的ID
                string strDbID = Convert.ToString(this.GetNewDbID());

                string strPureCfgsDir = "";
                string strTempSqlDbName = "";
                if (strEnLoginName != "")
                {
                    strTempSqlDbName = strEnLoginName + "_db";
                    strPureCfgsDir = strEnLoginName + "_cfgs";
                }
                else
                {
                    strTempSqlDbName = "dprms_" + strDbID + "_db";
                    strPureCfgsDir = "dprms_" + strDbID + "_cfgs";
                }

                if (strSqlDbName == null || strSqlDbName == "")
                    strSqlDbName = strTempSqlDbName;

                if (StringUtil.IsInList("file", strType, true) == false)
                {
                    strSqlDbName = this.GetFinalSqlDbName(strSqlDbName);

                    if (this.IsExistSqlName(strSqlDbName) == true)
                    {
                        strError = "不可能的情况，数据库中已存在'" + strSqlDbName + "'Sql库名";
                        return -1;
                    }

                    if (this.InstanceName != "")
                        strSqlDbName = this.InstanceName + "_" + strSqlDbName;
                }

                string strDataSource = "";
                if (StringUtil.IsInList("file", strType, true) == true)
                {
                    strDataSource = strSqlDbName;

                    strDataSource = this.GetFinalDataSource(strDataSource);

                    if (this.IsExistFileDbSource(strDataSource) == true)
                    {
                        strError = "不可能的情况，数据库中已存在''文件数据目录";
                        return -1;
                    }

                    string strDataDir = this.DataDir + "\\" + strDataSource;
                    if (Directory.Exists(strDataDir) == true)
                    {
                        strError = "不可能的情况，本地不会有重名的目录。";
                        return -1;
                    }

                    Directory.CreateDirectory(strDataDir);
                }

                strPureCfgsDir = this.GetFinalCfgsDir(strPureCfgsDir);
                // 把配置文件目录自动创建好
                string strCfgsDir = this.DataDir + "\\" + strPureCfgsDir + "\\cfgs";
                if (Directory.Exists(strCfgsDir) == true)
                {
                    strError = "服务器已存在'" + strPureCfgsDir + "'配置文件目录，请指定其它的英文逻辑库名。";
                    return -1;
                }

                Directory.CreateDirectory(strCfgsDir);

                string strPureKeysLocalName = "keys.xml";
                string strPureBrowseLocalName = "browse.xml";

                int nRet = 0;

                // 写keys配置文件
                nRet = DatabaseUtil.CreateXmlFile(strCfgsDir + "\\" + strPureKeysLocalName,
                    strKeysDefault,
                    out strError);
                if (nRet == -1)
                    return -1;


                // 写browse配置文件
                nRet = DatabaseUtil.CreateXmlFile(strCfgsDir + "\\" + strPureBrowseLocalName,
                    strBrowseDefault,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (StringUtil.IsInList("file", strType) == true)
                    strSqlDbName = "";

                // 这里发生xml片断可能会有小问题，应当用XmlTextWriter来发生?
                string strDbXml = "<database type='" + strType + "' id='" + strDbID + "' localdir='" + strPureCfgsDir
                    + "' dbo='"+user.Name+"'>"  // dbo参数为2006/7/4增加
                    + "<property>"
                    + strLogicNames
                    + "<datasource>" + strDataSource + "</datasource>"
                    + "<seed>0</seed>"
                    + "<sqlserverdb name='" + strSqlDbName + "'/>"
                    + "</property>"
                    + "<dir name='cfgs' localdir='cfgs'>"
                    + "<file name='keys' localname='" + strPureKeysLocalName + "'/>"
                    + "<file name='browse' localname='" + strPureBrowseLocalName + "'/>"
                    + "</dir>"
                    + "</database>";

                this.NodeDbs.InnerXml = this.NodeDbs.InnerXml + strDbXml;

                XmlNodeList nodeListDb = this.NodeDbs.SelectNodes("database");
                if (nodeListDb.Count == 0)
                {
                    strError = "刚新建数据库，不可能一个数据库都不存在。";
                    return -1;
                }

                // 最后一个库为新建的数据库，加到集合里
                XmlNode nodeDb = nodeListDb[nodeListDb.Count - 1];
                // return:
                //      -1  出错
                //      0   成功
                nRet = this.AddDatabase(nodeDb,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 及时加入dbo特性
                user.AddOwnerDbName(strTempDbName);

                // 及时保存到database.xml
                this.Changed = true;
                this.SaveXml();
            }
            finally
            {
                m_lock.ReleaseWriterLock();
                //***********对库集合解写锁****************
#if DEBUG_LOCK
				this.WriteDebugInfo("CreateDb()，对库集合解写锁。");
#endif
            }
            return 0;
        }


        // 规范sql数据库名称，只保存数字，大小写字线，下划线。
        // 为GetFinalSqlDbName()编的内部函数
        private void CanonicalizeSqlDbName(ref string strSqlDbName)
        {
            if (strSqlDbName == null)
                strSqlDbName = "";

            for (int i = 0; i < strSqlDbName.Length; i++)
            {
                char myChar = strSqlDbName[i];
                if (myChar == '_')
                    continue;

                if (myChar <= '9' && myChar >= '0')
                    continue;

                if (myChar <= 'z' && myChar >= 'a')
                    continue;

                if (myChar <= 'Z' && myChar >= 'A')
                    continue;

                strSqlDbName = strSqlDbName.Remove(i, 1);
                i--;
            }
        }

        // 得到最终的sql数据库名称
        private string GetFinalSqlDbName(string strSqlDbName)
        {
            if (strSqlDbName == null)
                strSqlDbName = "";

            string strRealSqlDbName = strSqlDbName;

            // 规范化Sql数据库名称
            this.CanonicalizeSqlDbName(ref strRealSqlDbName);


            for (int i = 0; ; i++)
            {
                if (strRealSqlDbName == "")
                {
                    strRealSqlDbName = "dprms_db_" + Convert.ToString(i);
                }

                if (this.IsExistSqlName(strRealSqlDbName) == false)
                    return strRealSqlDbName;
                else
                    strRealSqlDbName = strRealSqlDbName + Convert.ToString(i);
            }
        }

        // 规范化DataSource目录名
        // 为GetFinalDataSource()编的内部函数
        private void CanonicalizeDir(ref string strDataSource)
        {
            if (strDataSource == null)
                strDataSource = "";

            for (int i = 0; i < strDataSource.Length; i++)
            {
                char myChar = strDataSource[i];

                if (myChar == '\\'
                    || myChar == '/'
                    || myChar == ':'
                    || myChar == '*'
                    || myChar == '?'
                    || myChar == '<'
                    || myChar == '>'
                    || myChar == '|')
                {
                    strDataSource = strDataSource.Remove(i, 1);
                    i--;
                }
            }
        }

        // 得到最终的文件库使用的数据目录
        private string GetFinalDataSource(string strDataSource)
        {
            if (strDataSource == null)
                strDataSource = "";

            string strRealDataSource = strDataSource;

            this.CanonicalizeDir(ref strRealDataSource);

            for (int i = 0; ; i++)
            {
                if (strRealDataSource == "")
                {
                    strRealDataSource = "dprms_db_" + Convert.ToString(i);
                }

                if (this.IsExistFileDbSource(strRealDataSource) == false
                    && Directory.Exists(this.DataDir + "\\" + strRealDataSource) == false)
                {
                    return strRealDataSource;
                }
                else
                {
                    strRealDataSource = strRealDataSource + Convert.ToString(i);
                }
            }
        }

        // 得到最终的数据库使用的配置目录
        private string GetFinalCfgsDir(string strCfgsDir)
        {
            if (strCfgsDir == null)
                strCfgsDir = "";

            string strRealCfgsDir = strCfgsDir;

            this.CanonicalizeDir(ref strRealCfgsDir);

            for (int i = 0; ; i++)
            {
                if (strRealCfgsDir == "")
                {
                    strRealCfgsDir = "dprms_" + Convert.ToString(i) + "_cfgs";
                }

                if (this.IsExistCfgsDir(strRealCfgsDir, null) == false
                    && Directory.Exists(this.DataDir + "\\" + strRealCfgsDir) == false)
                {
                    return strRealCfgsDir;
                }
                else
                {
                    strRealCfgsDir = strRealCfgsDir + Convert.ToString(i);
                }
            }
        }

        // 检查其它库是否已存在相同的sql库名称
        internal bool IsExistSqlName(string strSqlName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database tempDb = (Database)this[i];
                if (!(tempDb is SqlDatabase))
                    continue;

                SqlDatabase sqlDb = (SqlDatabase)tempDb;
                string strDbSqlName = sqlDb.GetSourceName();// 得到Sql数据库名称
                if (String.Compare(strSqlName, strDbSqlName, true) == 0)
                    return true;
            }
            return false;
        }

        // 新得一个可用的数据库ID
        // return:
        //		新ID
        // 说明: 该函数在将字符型ID转换成数值ID时，如果转换不成功，会抛出异常
        private int GetNewDbID()
        {
            int nId = 0;
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                int nDbId = Convert.ToInt32(db.PureID);
                if (nId < nDbId)
                    nId = nDbId;
            }
            nId = nId + 1;
            return nId;
        }

        // 检查其它的库所有语言版本中是否存在相同的逻辑名
        internal bool IsExistLogicName(string strLogicName,
            Database exceptDb)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (exceptDb != null)
                {
                    if (db == exceptDb)
                        continue;
                }
                string strDbAllLogicName = db.GetAllCaption();
                if (StringUtil.IsInList(strLogicName, strDbAllLogicName, true) == true)
                    return true;
            }
            return false;
        }

        // 检索数据库对应的配置目录是否重复
        // parameters:
        //      strCfgsDir  目录名，相对目录
        //      exceptDb    不参考比较的数据库对象
        // return:
        //      true    有重复
        //      false   无重复
        internal bool IsExistCfgsDir(string strCfgsDir,
            Database exceptDb)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (exceptDb != null)
                {
                    if (db == exceptDb)
                        continue;
                }
                string strDbCfgsDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                    db.m_selfNode);

                if (String.Compare(strCfgsDir, strDbCfgsDir, true) == 0)
                    return true;
            }
            return false;
        }

        // 检查是否已存在相同的sql库名称
        internal bool IsExistFileDbSource(string strSource)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Database db = (Database)this[i];
                if (!(db is FileDatabase))
                    continue;
                string strDbSource = ((FileDatabase)db).m_strPureSourceDir;
                if (String.Compare(strSource, strDbSource, true) == 0)
                    return true;
            }
            return false;
        }


        // 删除数据库
        // parameters:
        //		strDbName	数据库名称，可以是各种语言版本的逻辑名，也可以是id号
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		-6	无足够的权限
        //		0	成功
        public int DeleteDb(User user,
            string strDbName,
            out string strError)
        {
            strError = "";

            if (user == null)
            {
                strError = "DeleteDb()调用错误，user参数不能为null。";
                return -1;
            }
            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "DeleteDb()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            //**********对库集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("DeleteDb()，对库集合加写锁。");
#endif
            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库";
                    return -1;
                }

                // 检查当前帐户是否有写权限
                string strExistRights = "";
                bool bHasRight = user.HasRights(db.GetCaption("zh-cn"),
                    ResType.Database,
                    "delete",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // 调database的Delete()函数，删除该库使用的配置文件，与物理数据库
                // return:
                //      -1  出错
                //      0   成功
                int nRet = db.Delete(out strError);
                if (nRet == -1)
                    return -1;

                //this.m_nodeDbs.RemoveChild(db.m_selfNode);
                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDbName);
                if (nodes.Count != 1)
                {
                    strError = "未找到名为'" + db.GetCaption("zh") + "'的数据库。";
                    return -1;
                }
                this.NodeDbs.RemoveChild(nodes[0]);

                // 删除内存对象
                this.Remove(db);


                // 及时除去dbo特性
                user.RemoveOwerDbName(strDbName);


                // 及时保存到database.xml
                this.Changed = true;
                this.SaveXml();

                return 0;
            }
            finally
            {
                m_lock.ReleaseWriterLock();
                //***********对库集合解写锁****************
#if DEBUG_LOCK
				this.WriteDebugInfo("DeleteDb()，对库集合解写锁。");
#endif
            }
        }

        // 获得数据定义方面的信息
        // parameters:
        //      strStyle            获得那些输出参数? all表示全部 分别指定则是logicnames/type/sqldbname/keystext/browsetext
        // return:
        //      -1  一般性错误
        //      -5  未找到数据库对象
        //      -6  没有足够的权限
        //      0   成功
        public int GetDbInfo(User user,
            string strDbName,
            string strStyle,
            out LogicNameItem[] logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysText,
            out string strBrowseText,
            out string strError)
        {
            strError = "";

            logicNames = null;
            strType = "";
            strSqlDbName = "";
            strKeysText = "";
            strBrowseText = "";

            Debug.Assert(user != null, "GetDbInfo()调用错误，user参数不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "GetDbInfo()调用不合法，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 检查当前帐户是否有显示权限
            string strExistRights = "";
            bool bHasRight = user.HasRights(strDbName,
                ResType.Database,
                "read",
                out strExistRights);

            //******************对库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetDbInfo()，对库集合加读锁。");
#endif
            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库。";
                    return -5;
                }

                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'读(read)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1	出错
                //		0	正常
                return db.GetInfo(
                    strStyle,
                    out logicNames,
                    out strType,
                    out strSqlDbName,
                    out strKeysText,
                    out strBrowseText,
                    out strError);
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("GetDbInfo()，对库集合解读锁。");
#endif
            }
        }

        // 设置数据库基本信息
        // parameter:
        //		strDbName	        数据库名称
        //		strLang	            对应的语言版本，如果语言版本为null或者为空字符串，则从所有的语言版本中找
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout，目前无效，因为涉及到是文件库，还是sql库的问题
        //		strSqlDbName	    指定的新Sql数据库名称,，目前无效
        //		strKeysDefault	    keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //      -1  一般性错误
        //      -2  已存在同名的数据库
        //      -5  未找到数据库对象
        //      -6  没有足够的权限
        //      0   成功
        public int SetDbInfo(User user,
            string strDbName,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysText,
            string strBrowseText,
            out string strError)
        {
            strError = "";

            Debug.Assert(user != null, "SetDbInfo()调用错误，user参数不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "SetDbInfo()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 为避免死锁的问题，将查看权限的函数放在外面了
            // 检查当前帐户是否有覆盖数据库结构的权限
            string strExistRights = "";
            bool bHasRight = user.HasRights(strDbName,
                ResType.Database,
                "overwrite",
                out strExistRights);

            //******************对库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("SetDbInfo()，对库集合加读锁。");
#endif
            try
            {
                Database db = this.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到名为'" + strDbName + "'的数据库。";
                    return -5;
                }

                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1	出错
                //      -2  已存在同名的数据库
                //		0	成功
                int nRet = db.SetInfo(logicNames,
                    strType,
                    strSqlDbName,
                    strKeysText,
                    strBrowseText,
                    out strError);
                if (nRet <= -1)
                    return nRet;

                // 及时保存databases.xml
                this.Changed = true;
                this.SaveXml();

                return 0;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("SetDbInfo()，对库集合解读锁。");
#endif
            }

        }


        // ???对库集合加读锁
        // 初始化数据库
        // parameters:
        //      user    帐户对象
        //      strDbName   数据库名称
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      -5  数据库不存在
        //      -6  权限不够
        //      0   成功
        // 线：安全 代码没跟上
        public int InitializePhysicalDatabase(User user,
            string strDbName,
            out string strError)
        {
            strError = "";
            Debug.Assert(user != null, "InitializeDb()调用错误，user参数值不能为null。");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "InitializeDb()调用错误，strDbName参数值不能为null或空字符串。";
                return -1;
            }

            // 1.得到数据库
            Database db = this.GetDatabaseSafety(strDbName);
            if (db == null)
            {
                strError = "没有找到名为'" + strDbName + "'的数据库";
                return -5;
            }

            string strExistRights = "";
            bool bHasRight = user.HasRights(db.GetCaption("zh-cn"),
                ResType.Database,
                "clear",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'初始化(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            // 3.初始化
            // return:
            //		-1  出错
            //		0   成功
            return db.InitialPhysicalDatabase(out strError);
        }

        // 得到key的长度
        // parameters:
        //      nKeySize    out参数，返回检索点长度
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线: 不安全
        public int InternalGetKeySize(
            out int nKeySize,
            out string strError)
        {
            nKeySize = 0;
            strError = "";

            Debug.Assert(this.m_dom != null, "InternalGetKeySize()里发现this.m_dom为null，异常");

            XmlNode nodeKeySize = this.m_dom.DocumentElement.SelectSingleNode("keysize");
            if (nodeKeySize == null)
            {
                strError = "服务器配置文件不合法,未在根下定义<keysize>元素";
                return -1;
            }

            string strKeySize = DomUtil.GetNodeText(nodeKeySize).Trim();
            try
            {
                nKeySize = Convert.ToInt32(strKeySize);
            }
            catch (Exception ex)
            {
                strError = "服务器配置文件不合法，根下的<keysize>元素的内容不能为'" + strKeySize + "',必须为数字格式。" + ex.Message;
                return -1;
            }

            return 0;
        }

        // 得到链接字符串,只有库类型为SqlDatabase时才有意义
        // parameters:
        //      strConnection   out参数，返回连接字符串联
        //      strError        out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线: 不安全的
        internal int InternalGetConnString(
            out string strConnection,
            out string strError)
        {
            strConnection = "";
            strError = "";

            XmlNode nodeDataSource = m_dom.DocumentElement.SelectSingleNode("datasource");
            if (nodeDataSource == null)
            {
                strError = "服务器配置文件不合法，未在根元素下定义<datasource>元素";
                return -1;
            }

            string strMode = DomUtil.GetAttr(nodeDataSource, "mode");



            string strServerName = DomUtil.GetAttr(nodeDataSource, "servername").Trim();
            if (strServerName == "")
            {
                strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'servername'属性，或'servername'属性值为空。";
                return -1;
            }

            string strUserID = "";
            string strPassword = "";

            if (String.IsNullOrEmpty(strMode) == true)
            {

                strUserID = DomUtil.GetAttr(nodeDataSource, "userid").Trim();
                if (strUserID == "")
                {
                    strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'userid'属性，或'userid'属性值为空。";
                    return -1;
                }

                strPassword = DomUtil.GetAttr(nodeDataSource, "password").Trim();
                if (strPassword == "")
                {
                    strError = "服务器配置文件不合法，未给根元素下级的<datasource>定义'password'属性，或'password'属性值为空。";
                    return -1;
                }
                // password可能为空
                try
                {
                    strPassword = Cryptography.Decrypt(strPassword,
                            "dp2003");
                }
                catch
                {
                    strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                    return -1;
                }

                strConnection = @"Persist Security Info=False;"
    + "User ID=" + strUserID + ";"    //帐户和密码
    + "Password=" + strPassword + ";"
                    //+ "Integrated Security=SSPI; "      //信任连接
    + "Data Source=" + strServerName + ";"
    + "Connect Timeout=30";

            }
            else if (strMode == "SSPI") // 2006/3/22
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + strServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }
            else
            {
                strError = "服务器配置文件不合法，根元素下级的<datasource>定义mode属性值'"+strMode+"'不合法。";
                return -1;
            }

            return 0;
        }


        // 本函数可以自动分析数据库名称格式，找到对应数据库
        // strName: 数据库名 格式为"库名" 或 "@id" 或 "@id[库名]"
        // 线: 安全的
        public Database GetDatabaseSafety(string strDbName)
        {
            //******************对库集合加读锁******
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetDatabaseSafety()，对库集合加读锁。");
#endif
            try
            {
                return this.GetDatabase(strDbName);
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*************
#if DEBUG_LOCK
				this.WriteDebugInfo("GetDatabaseSafety()，对库集合解读锁。");
#endif
            }
        }

        // 根据指定义语言版本的逻辑名找数据库
        // parameters:
        //		strLogicName	逻辑库名
        //		strLang	语言版本
        // return:
        //		找到返回Database对象
        //		没找到返回null
        // 线: 安全的
        public Database GetDatabaseByLogicNameSafety(string strDbName,
            string strLang)
        {
            //******************对库集合加读锁******
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("GetDatabaseByLogicNameSafety()，对库集合加读锁。");
#endif
            try
            {
                return this.GetDatabaseByLogicName(strDbName,
                    strLang);
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对库集合解读锁*********
#if DEBUG_LOCK
				this.WriteDebugInfo("GetDatabaseByLogicNameSafety()，对库集合解读锁。");
#endif
            }
        }

        // 根据名称得到一个数据库
        // parameters:
        //		strName	数据库名称，也可以是ID(前面加@)
        // 线: 不安全
        public Database GetDatabase(string strName)
        {
            if (String.IsNullOrEmpty(strName) == true)
                throw new Exception("数据库名不能为空");

            Debug.Assert(String.IsNullOrEmpty(strName) == false, "GetDatabase()调用错误，strName参数值不能为null或空字符串。");

            string strFirst = "";
            string strSecond = "";
            int nPosition;
            nPosition = strName.LastIndexOf("[");
            if (nPosition >= 0)
            {
                strFirst = strName.Substring(0, nPosition);
                strSecond = strName.Substring(nPosition + 1);
            }
            else
            {
                strFirst = strName;
            }
            Database db = null;
            if (strFirst != "")
            {
                if (strFirst.Substring(0, 1) == "@")
                    db = GetDatabaseByID(strFirst);
                else
                    db = GetDatabaseByLogicName(strFirst);
            }
            else if (strSecond != "")
            {
                if (strSecond.Substring(0, 1) == "@")
                    db = GetDatabaseByID(strSecond);
                else
                    db = GetDatabaseByLogicName(strSecond);
            }
            return db;
        }


        // 根据逻辑名找数据库，任何语言版本都可以
        // 线: 不安全
        private Database GetDatabaseByLogicName(string strLogicName)
        {
            Debug.Assert(String.IsNullOrEmpty(strLogicName) == false, "GetDatabaseByLogicName()调用错误，strLogicName参数值不能为null或空字符串。");

            foreach (Database db in this)
            {
                if (StringUtil.IsInList(strLogicName,
                    db.GetCaptionsSafety()) == true)
                {
                    return db;
                }
            }
            return null;
        }

        // 根据指定义语言版本的逻辑名找数据库
        // parameters:
        //		strLogicName	逻辑库名
        //		strLang	语言版本
        // return:
        //		找到返回Database对象
        //		没找到返回null
        // 线: 不安全
        private Database GetDatabaseByLogicName(string strLogicName,
            string strLang)
        {
            foreach (Database db in this)
            {
                if (String.Compare(strLogicName, db.GetCaptionSafety(strLang)) == 0)
                {
                    return db;
                }
            }
            return null;
        }

        // 通过数据库ID找到指定的数据库，注意这里的ID带@
        // 线: 不安全
        private Database GetDatabaseByID(string strDbID)
        {
            foreach (Database db in this)
            {
                if (db.FullID == strDbID)
                {
                    return db;
                }
            }
            return null;
        }

        // 检索
        // parameter:
        //		strQuery	检索式XML字符串
        //		resultSet	结果集,用于存放检索结果
        //		oUser	    帐户对象,用于检索该帐户对某库是否有读权限
        //  				为null,则不进行权限的检查，即按有权限算
        //		isConnected	delegate对象,用于通讯是否连接正常
        //					为null，则不调delegate函数
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //      -6  权限不够
        //		0	成功
        // 线: 安全的
        public int Search(string strQuery,
            DpResultSet resultSet,
            User oUser,
            Delegate_isConnected isConnected,
            out string strError)
        {
            strError = "";

            //对库集合加读锁*********************************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("Search()，对库集合加读锁。");
#endif
            try
            {
                if (String.IsNullOrEmpty(strQuery) == true)
                {
                    strError = "Search()调用错误，strQuery不能为null或空字符串";
                    return -1;
                }

                // 一进来先给结果集的m_strQuery成员赋值，
                // 不管是否是合法的XML，在用结果集的时候再判断
                resultSet.m_strQuery = strQuery;
                XmlDocument dom = new XmlDocument();
                dom.PreserveWhitespace = true; //设PreserveWhitespace为true
                try
                {
                    dom.LoadXml(strQuery);
                }
                catch (Exception ex)
                {
                    strError += "检索式字符串加载到dom出错，原因：" + ex.Message + "\r\n"
                        + "检索式字符串如下:\r\n"
                        + strQuery;
                    return -1;
                }

                //创建Query对象
                Query query = new Query(this,
                    oUser,
                    dom);

                //进行检索
                // return:
                //		-1	出错
                //		-6	无权限
                //		0	成功
                int nRet = query.DoQuery(dom.DocumentElement,
                    resultSet,
                    isConnected,
                    out strError);
                if (nRet <= -1)
                    return nRet;
            }
            finally
            {
                //****************对库集合解读锁**************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.WriteDebugInfo("Search()，对库集合解读锁。");
#endif
            }
            return 0;
        }



        // 拷贝一条源记录到目标记录，要求对源记录有读权限，对目标记录有写权限
        // 关键点是锁的问题
        // Parameter:
        //      user                    用户对象
        //		strOriginRecordPath	    源记录路径
        //		strTargetRecordPath	    目标记录路径
        //		bDeleteOriginRecord	    是否删除源记录
        //      strOutputRecordPath     返回目标记录的路径，用于目标记录是新建一条记录
        //      baOutputRecordTimestamp 返回目标记录的时间戳
        //		strError	出错信息
        // return:
        //		-1	一般性错误
        //      -4  未找到记录
        //      -5  未找到数据库
        //      -6  没有足够的权限
        //      -7  路径不合法
        //		0	成功
        public int CopyRecord(User user,
            string strOriginRecordPath,
            string strTargetRecordPath,
            bool bDeleteOriginRecord,
            out string strTargetRecordOutputPath,
            out byte[] baOutputRecordTimestamp,
            out string strError)
        {
            Debug.Assert(user != null, "CopyRecord()调用错误，user对象不能为null。");

            this.WriteErrorLog("走到CopyRecord(),strOriginRecordPath='" + strOriginRecordPath + "' strTargetRecordPath='" + strTargetRecordPath + "'");

            strTargetRecordOutputPath = "";
            baOutputRecordTimestamp = null;
            strError = "";

            if (String.IsNullOrEmpty(strOriginRecordPath) == true)
            {
                strError = "CopyRecord()调用错误，strOriginRecordPath参数值不能为nul或空字符串";
                return -1;
            }
            if (String.IsNullOrEmpty(strTargetRecordPath) == true)
            {
                strError = "CopyRecord()调用错误，strTargetRecordPath参数值不能为null或空字符串";
                return -1;
            }

            long nRet = 0;

            // 得到源记录的xml
            string strOriginRecordStyle = "data,metadata,timestamp";
            byte[] baOriginRecordData = null;
            string strOriginRecordMetadata = "";
            string strOriginRecordOutputPath = "";
            byte[] baOriginRecordOutputTimestamp = null;

            int nAdditionError = 0;
            // return:
            //		-1	一般性错误
            //		-4	未找到路径指定的资源
            //		-5	未找到数据库
            //		-6	没有足够的权限
            //		-7	路径不合法
            //		-10	未找到记录xpath对应的节点  // 此次调用不可能出现这种情况
            //		>= 0	成功，返回最大长度
            nRet = this.GetRes(strOriginRecordPath,
                0,
                -1,
                strOriginRecordStyle,
                user,
                -1,
                out baOriginRecordData,
                out strOriginRecordMetadata,
                out strOriginRecordOutputPath,
                out baOriginRecordOutputTimestamp,
                out nAdditionError,
                out strError);
            if (nRet <= -1)
                return (int)nRet;


            // 写目标记录xml
            string strTargetRecordRanges = "";
            long lTargetRecordTotalLength = baOriginRecordData.Length;
            byte[] baTargetRecordData = baOriginRecordData;
            string strTargetRecordMetadata = strOriginRecordMetadata;
            string strTargetRecordStyle = "ignorechecktimestamp";
            byte[] baTargetRecordOutputTimestamp = null;
            string strTargetRecordOutputValue = "";
            // return:
            //		-1	一般性错误
            //		-2	时间戳不匹配    // 此处调用不可能出现这种情况
            //		-4	未找到路径指定的资源
            //		-5	未找到数据库
            //		-6	没有足够的权限
            //		-7	路径不合法
            //		-8	已经存在同名同类型的项  // 此处调用不可能出现这种情况
            //		-9	已经存在同名但不同类型的项  // 此处调用不可能出现这种情况
            //		0	成功
            nRet = this.WriteRes(strTargetRecordPath,
                strTargetRecordRanges,
                lTargetRecordTotalLength,
                baTargetRecordData,
                null, //streamSource
                strTargetRecordMetadata,
                strTargetRecordStyle,
                null, //baInputTimestamp
                user,
                out strTargetRecordOutputPath,
                out baTargetRecordOutputTimestamp,
                out strTargetRecordOutputValue,
                out strError);
            if (nRet <= -1)
                return (int)nRet;

            // 处理资源
            byte[] baPreamble;
            string strXml = DatabaseUtil.ByteArrayToString(baOriginRecordData,
                out baPreamble);
            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true; //设PreserveWhitespace为true
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "加载'" + strOriginRecordPath + "'的记录体到dom出错，原因：" + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList fileList = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlNode fileNode in fileList)
            {

                // 获取源资源对象
                string strObjectID = DomUtil.GetAttr(fileNode, "id");
                string strOriginObjectPath = strOriginRecordPath + "/object/" + strObjectID;
                byte[] baOriginObjectData = null;
                string strOriginObjectMetadata = "";
                string strOriginObjectOutputPath = "";
                byte[] baOriginObjectOutputTimestamp = null;

                this.WriteErrorLog("走到CopyRecord(),获取资源，源路径='" + strOriginObjectPath + "'");

                // int nAdditionError = 0;
                // return:
                //		-1	一般性错误
                //		-4	未找到路径指定的资源
                //		-5	未找到数据库
                //		-6	没有足够的权限
                //		-7	路径不合法
                //		-10	未找到记录xpath对应的节点
                //		>= 0	成功，返回最大长度
                nRet = this.GetRes(strOriginObjectPath,
                    0,
                    -1,
                    "data,metadata",
                    user,
                    -1,
                    out baOriginObjectData,
                    out strOriginObjectMetadata,
                    out strOriginObjectOutputPath,
                    out baOriginObjectOutputTimestamp,
                    out nAdditionError,
                    out strError);
                if (nRet <= -1)
                    return (int)nRet;

                // 写目标资源对象
                string strTargetObjectPath = strTargetRecordOutputPath + "/object/" + strObjectID;
                long lTargetObjectTotalLength = baOriginObjectData.Length;
                string strTargetObjectMetadata = strOriginObjectMetadata;
                string strTargetObjectStyle = "ignorechecktimestamp";
                string strTargetObjectOutputPath = "";
                byte[] baTargetObjectOutputTimestamp = null;
                string strTargetObjectOutputValue = "";

                //this.WriteErrorLog("走到CopyRecord(),写资源，目标路径='" + strTargetObjectPath + "'");

                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		-4	未找到路径指定的资源
                //		-5	未找到数据库
                //		-6	没有足够的权限
                //		-7	路径不合法
                //		-8	已经存在同名同类型的项
                //		-9	已经存在同名但不同类型的项
                //		0	成功
                nRet = this.WriteRes(strTargetObjectPath,
                    "",
                    lTargetObjectTotalLength,
                    baOriginObjectData,
                    null,
                    strTargetObjectMetadata,
                    strTargetObjectStyle,
                    null,
                    user,
                    out strTargetObjectOutputPath,
                    out baTargetObjectOutputTimestamp,
                    out strTargetObjectOutputValue,
                    out strError);
                if (nRet <= -1)
                    return (int)nRet;
            }

            // 判断是否删除源记录
            if (bDeleteOriginRecord == true)
            {
                // return:
                //      -1	一般性错误，例如输入参数不合法等
                //      -2	时间戳不匹配    // 建议忽略时间戳，不应出现这种情况
                //      -4	未找到路径对应的资源
                //      -5	未找到数据库
                //      -6	没有足够的权限
                //      -7	路径不合法
                //      0	操作成功
                nRet = this.DeleteRes(strOriginRecordPath,
                    user,
                    baOriginRecordOutputTimestamp,
                    out baOriginRecordOutputTimestamp,
                    out strError);
                if (nRet <= -1)
                    return (int)nRet;
            }

            // 取出目标记录的最终时间戳
            // return:
            //		-1  出错
            //		-4  未找到记录
            //      0   成功
            nRet = this.GetTimestampFromDb(
                strTargetRecordOutputPath,
                out baOutputRecordTimestamp,
                out strError);
            if (nRet <= -1)
            {
                strError = "拷贝记录完成，但获取目标记录的时间戳时出错：" + strError;
                return -1;
            }

            return 0;
        }

        // 获取记录的时间戳
        // parameters:
        //      strRecordPath   记录路径
        //      baOutputTimestamp   out参数，返回时间戳
        //      strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        public int GetTimestampFromDb(string strRecordPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            Debug.Assert(strRecordPath != null && strRecordPath != "", "GetTimestampFromDb()调用错误，strRecordPath参数值不能为null或空字符串。");

            DbPath dbpath = new DbPath(strRecordPath);
            Database db = this.GetDatabase(dbpath.Name);
            if (db == null)
            {
                strError = "未找到名为'" + dbpath.Name + "'的数据库。";
                return -1;
            }

            // return:
            //		-1  出错
            //		-4  未找到记录
            //      0   成功
            int nRet = db.GetTimestampFromDb(dbpath.ID,
                out baOutputTimestamp,
                out strError);

            return nRet;
        }


        // 清空目录配置事项
        // parameters:
        //		strDirCfgItemPath	配置目录的路径
        //		nodeDir	            dir节点，如果为null，则根据路径来找
        //		strError        	out参数，返回出错信息
        // return:
        //		-1	出错
        //      -4  未指定路径对应的对象
        //		0	成功
        // 清空dir配置事项，包括所有下级及属性，也删除下级对应的物理文件
        public int ClearDirCfgItem(string strDirCfgItemPath,
            XmlNode nodeDir,
            out string strError)
        {
            strError = "";
            if (nodeDir == null)
            {
                if (String.IsNullOrEmpty(strDirCfgItemPath) == true)
                {
                    strError = "ClearDirCfgItem()调用错误，strDirCfgItemPath参数不能为null或者空字符串。";
                    return -1;
                }

                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count == 0)
                {
                    strError = "ClearDirCfgItem()，未找到路径为'" + strDirCfgItemPath + "'的配置事项。";
                    return -4;
                }

                if (nodes.Count > 1)
                {
                    strError = "ClearDirCfgItem()，路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，databases.xml配置文件不合法。";
                    return -1;
                }

                nodeDir = nodes[0];
            }

            // 删除定义的本地目录
            string strLocalDir = "";
            strLocalDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                nodeDir).Trim();

            string strDir = "";
            if (strLocalDir != "")
                strDir = this.DataDir + "\\" + strLocalDir + "\\";
            else
                strDir = this.DataDir + "\\";

            DirectoryInfo di = new DirectoryInfo(strDir);

            // 删除所有的下级目录
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo childDir in dirs)
            {
                Directory.Delete(childDir.FullName, true);
            }

            // 删除所有的下级文件
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo childFile in files)
            {
                File.Delete(childFile.FullName);
            }

            // 移出内存对象
            nodeDir.RemoveAll();

            this.Changed = true;

            return 0;
        }


        // 给内存对象新设一个配置事项
        // parameters:
        //		strParentPath	父亲路径 如果为null或空字符串，则直接在objects下级新建
        //		strName	自己的名称，不能为null或空字符串
        //		bDir	是否是路径
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int SetFileCfgItem(string strParentPath,
            XmlNode nodeParent,
            string strName,
            out string strError)
        {
            strError = "";
            //**********对数据库集合加写锁**************
            this.m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("SetCfgItem()，对数据集合加写锁。");
#endif
            try
            {
                if (String.IsNullOrEmpty(strName) == true)
                {
                    strError = "SetCfgItem()调用错误，strName参数值不能为null或空字符串。";
                    return -1;
                }

                if (nodeParent == null)
                {
                    if (strParentPath == "" || strParentPath == null)
                    {
                        nodeParent = this.NodeDbs;
                    }
                    else
                    {
                        List<XmlNode> parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                            strParentPath);
                        if (parentNodes.Count > 1)
                        {
                            strError = "在<objects>下级路径为'" + strParentPath + "'配置事项有'" + Convert.ToString(parentNodes.Count) + "'个，配置文件不合法。。";
                            return -1;
                        }
                        if (parentNodes.Count == 0)
                        {
                            strError = "在<objects>下级未找到路径为'" + strParentPath + "'配置事项。";
                            return -1;
                        }

                        nodeParent = parentNodes[0];
                    }
                }

                string strCfgItemOuterXml = "";
                string strLocalName = strName + ".xml";
                strCfgItemOuterXml = "<file name='" + strName + "' localname='" + strLocalName + "'/>";

                nodeParent.InnerXml = nodeParent.InnerXml + strCfgItemOuterXml;

                this.Changed = true;

                return 0;
            }
            finally
            {
                //***********对数据库集合解写锁***************
                this.m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("SetCfgItem()，对数据库集合解写锁。");
#endif
            }
        }


        // 自动创建目录配置事项
        // parameters:
        //		strParentPath	父亲路径 如果为null或空字符串，则直接在objects下级新建
        //		strName	自己的名称，不能为null或空字符串
        //		bDir	是否是路径
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int AutoCreateDirCfgItem(string strDirCfgItemPath,
            out string strError)
        {
            strError = "";

            //**********对数据库集合加写锁**************
            this.m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("AutoCreateDirCfgItem()，对'" + this.GetCaption("zh-cn") + "'数据库集合加写锁。");
#endif
            try
            {
                if (String.IsNullOrEmpty(strDirCfgItemPath) == true)
                {
                    strError = "AutoCreateDirCfgItem()调用错误，strDirCfgItemPath参数值不能为null或空字符串。";
                    return -1;
                }

                List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count > 1)
                {
                    strError = "路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，服务器配置文件不合法。";
                    return -1;
                }
                if (nodes.Count == 1)
                {
                    strError = "AutoCreateDirCfgItem()调用错误，已存在路径为'" + strDirCfgItemPath + "'的配置目录。";
                    return -1;
                }

                XmlDocument dom = this.NodeDbs.OwnerDocument;
                if (dom == null)
                {
                    strError = "AutoCreateDirCfgItem()里不可能找不到dom。";
                    return -1;
                }

                //把strpath用'/'分开
                string[] paths = strDirCfgItemPath.Split(new char[] { '/' });
                if (paths.Length == 0)
                {
                    strError = "AutoCreateDirCfgItem()里paths长度不可能为0。";
                    return -1;
                }

                int i = 0;
                if (paths[0] == "")
                    i = 1;
                XmlNode nodeCurrent = this.NodeDbs;
                XmlNode temp = null;
                for (; i < paths.Length; i++)
                {
                    string strDirName = paths[i];

                    if (nodeCurrent == this.NodeDbs)
                    {
                        //XmlNode temp = null;
                        foreach (XmlNode tempChild in nodeCurrent.ChildNodes)
                        {
                            if (tempChild.Name == "database")
                            {
                                string strAllCaption = DatabaseUtil.GetAllCaption(tempChild);
                                if (StringUtil.IsInList(strDirName, strAllCaption, true) == true)
                                {
                                    temp = tempChild;
                                    break;
                                }
                            }
                            else
                            {
                                string strTempName = DomUtil.GetAttr(tempChild, "name");
                                if (String.Compare(strTempName, strDirName, true) == 0)
                                {
                                    temp = tempChild;
                                    break;
                                }
                            }
                        }

                        if (temp == null)
                        {
                            temp = dom.CreateElement("dir");
                            DomUtil.SetAttr(temp, "name", strDirName);
                            DomUtil.SetAttr(temp, "localdir", strDirName);
                            nodeCurrent.AppendChild(temp);
                        }

                        nodeCurrent = temp;
                    }
                    else
                    {
                        string strTempXpath = "dir[@name='" + strDirName + "']";
                        temp = nodeCurrent.SelectSingleNode(strTempXpath);
                        if (temp == null)
                        {
                            temp = dom.CreateElement("dir");
                            DomUtil.SetAttr(temp, "name", strDirName);
                            DomUtil.SetAttr(temp, "localdir", strDirName);
                            nodeCurrent.AppendChild(temp);
                        }
                        nodeCurrent = temp;
                    }
                }

                nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strDirCfgItemPath);
                if (nodes.Count > 1)
                {
                    strError = "经过自动创建，路径为'" + strDirCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，绝对不可能的情况。";
                    return -1;
                }
                if (nodes.Count == 0)
                {
                    strError = "AutoCreateDirCfgItem()已自动创建'" + strDirCfgItemPath + "'配置目录内存对象完毕，不可能还是不存在。";
                    return -1;
                }
                XmlNode node = nodes[0];

                string strDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                    node);
                strDir = this.DataDir + "\\" + strDir;
                PathUtil.CreateDirIfNeed(strDir);

                this.Changed = true;

                return 0;
            }
            finally
            {
                //***************对数据库集合解写锁************
                this.m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("AutoCreateDirCfgItem()，对'" + this.GetCaption("zh-cn") + "'数据库集合解写锁。");
#endif
            }
        }


        // 写资源
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		strRanges		目标的位置,多个range用逗号分隔,null认为是空字符串，空字符串认为是0-(lTotalLength-1)
        //		lTotalLength	资源总长度,可以为0
        //		baContent		用byte[]数据传送的资源内容，如果为null则表示是0字节的数组
        //		streamContent	内容流
        //		strMetadata		元数据内容，null认为是空字符串，注:有些元数据虽然传过来，但服务器不认，比如长度
        //		strStyle		风格,null认为是空字符串
        //						ignorechecktimestamp 忽略时间戳;
        //						createdir,创建目录,路径表示待创建的目录路径
        //						autocreatedir	自动创建中间层的目录
        //						content	数据放在baContent参数里
        //						attachment	数据放在附件里
        //		baInputTimestamp	输入的时间戳,对于创建目录，不检查时间戳
        //		user	帐户对象，不能为null
        //		strOutputResPath	返回的资源路径
        //							比如追加记录时，返回实际的路径
        //							其它资源返回的路径与输入的路径相同
        //		baOutputTimestamp	返回时间戳
        //							当为目录时，返回的时间戳为null
        //		strOutputValue	返回的值，比如做累加计算时
        //		strError	出错信息
        // 说明：
        //		本函数实际代表了两种情况，新建资源，覆盖资源
        //		baContent，strAttachmentID只能使用一个，与strStyle配置使用
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		-4	未找到路径指定的资源
        //		-5	未找到数据库
        //		-6	没有足够的权限
        //		-7	路径不合法
        //		-8	已经存在同名同类型的项
        //		-9	已经存在同名但不同类型的项
        //		0	成功
        // 线：安全
        public int WriteRes(string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            User user,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out string strOutputValue,
            out string strError)
        {
            baOutputTimestamp = null;
            strOutputResPath = strResPath;
            strOutputValue = "";
            strError = "";

            //**********对库集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("WriteRes()，对库集合加写锁。");
#endif
            try
            {

                //------------------------------------------------
                //检查输入参数是否合法，并规范输入参数
                //---------------------------------------------------
                if (user == null)
                {
                    strError = "WriteRes()调用错误，user对象不能为null";
                    return -1;
                }
                if (String.IsNullOrEmpty(strResPath) == true)
                {
                    strError = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                    return -7;
                }
                if (lTotalLength < 0)
                {
                    strError = "WriteRes()，lTotalLength不能为'" + Convert.ToString(lTotalLength) + "'，必须>=0。";
                    return -1;
                }
                if (strRanges == null) //里面的函数，会处理成代表的范围
                    strRanges = "";
                if (strMetadata == null)
                    strMetadata = "";
                if (strStyle == null)
                    strStyle = "";

                if (baSource == null && streamSource == null)
                {
                    strError = "WriteRes()调用错误，baSource参数与streamSource参数不能同时为null。";
                    return -1;
                }
                if (baSource != null && streamSource != null)
                {
                    strError = "WriteRes()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                    return -1;
                }


                //------------------------------------------------
                //分析出资源的类型
                //---------------------------------------------------

                int nRet = 0;

                bool bRecordPath = this.IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    // 关于配置目录
                    if (StringUtil.IsInList("createdir", strStyle, true) == true)
                    {
                        // return:
                        //      -1  一般性错误
                        //		-4	未指定路径对应的对象
                        //		-6	权限不够
                        //		-8	目录已存在
                        //		-9	存在其它类型的事项
                        //		0	成功
                        nRet = this.WriteDirCfgItem(strResPath,
                            strStyle,
                            user,
                            out strError);
                    }
                    else
                    {
                        // return:
                        //      -1  一般性错误
                        //      -2  时间戳不匹配
                        //      -4  自动创建目录时，未找到上级
                        //		-6	权限不够
                        //		-9	存在其它类型的事项
                        //		0	成功
                        nRet = this.WriteFileCfgItem(strResPath,
                            strRanges,
                            lTotalLength,
                            baSource,
                            streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            user,
                            out baOutputTimestamp,
                            out strError);
                    }

                    strOutputResPath = strResPath;

                    // 保存database.xml文件
                    if (this.Changed == true)
                        this.SaveXmlSafety();
                }
                else
                {
                    bool bObject = false;
                    string strRecordID = "";
                    string strObjectID = "";
                    string strXPath = "";

                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第1层*************
                    // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'路径不合法，未指定库的下级。";
                        return -7;
                    }
                    // 找到数据库对象
                    Database db = this.GetDatabaseSafety(strDbName);
                    if (db == null)
                    {
                        strError = "名'" + strDbName + "'的数据库不存在。";
                        return -5;
                    }

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含记录号层了，下级分情况判断


                    strRecordID = strFirstPart;
                    // 只到记录号层的路径
                    if (strPath == "")
                    {
                        bObject = false;
                        goto DOWRITE;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath

                    if (strFirstPart != "object"
                        && strFirstPart != "xpath")
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,第3级必须是'object'或'xpath'";
                        return -7;
                    }
                    if (strPath == "")  //object或xpath下级必须有值
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,当第3级是'object'或'xpath'，第4级必须有内容。";
                        return -7;
                    }

                    if (strFirstPart == "object")
                    {
                        strObjectID = strPath;
                        bObject = true;
                    }
                    else
                    {
                        strXPath = strPath;
                        bObject = false;
                    }


                    //------------------------------------------------
                //开始处理资源
                //---------------------------------------------------

                DOWRITE:

                    // ****************************************


                    string strOutputRecordID = "";
                    nRet = db.CanonicalizeRecordID(strRecordID,
                        out strOutputRecordID,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法，原因：记录号不能为'" + strRecordID + "'";
                        return -1;
                    }


                    // ************************************
                    // 处理记录和记录里的对象
                    if (bObject == true)  //对像
                    {
                        if (strOutputRecordID == "-1")
                        {
                            strError = "资源路径 '" + strResPath + "' 不合法,原因：保存对象资源时,记录号不能为'" + strRecordID + "'。";
                            return -1;
                        }
                        strRecordID = strOutputRecordID;

                        // return:
                        //		-1  出错
                        //		-2  时间戳不匹配
                        //      -4  记录或对象资源不存在
                        //      -6  权限不够
                        //		0   成功
                        nRet = db.WriteObject(user,
                            strRecordID,
                            strObjectID,
                            strRanges,
                            lTotalLength,
                            baSource,
                            streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            out baOutputTimestamp,
                            out strError);

                        strOutputResPath = strDbName + "/" + strRecordID + "/object/" + strObjectID;

                    }
                    else  // 记录体
                    {
                        strRecordID = strOutputRecordID;

                        string strOutputID = "";
                        // return:
                        //		-1  出错
                        //		-2  时间戳不匹配
                        //      -4  记录不存在
                        //      -6  权限不够
                        //		0   成功
                        nRet = db.WriteXml(user,
                            strRecordID,
                            strXPath,
                            strRanges,
                            lTotalLength,
                            baSource,
                            streamSource,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            out baOutputTimestamp,
                            out strOutputID,
                            out strOutputValue,
                            true,
                            out strError);

                        strRecordID = strOutputID;

                        if (strXPath == "")
                            strOutputResPath = strDbName + "/" + strRecordID;
                        else
                            strOutputResPath = strDbName + "/" + strRecordID + "/xpath/" + strXPath;

                    }
                }

                return nRet;
            }
            finally
            {
                //**********对库集合解写锁****************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("WriteRes()，对库集合写写锁。");
#endif
            }
        }

        // 写目录配置事项
        // parameters:
        //		strResPath	资源路径带库名
        //					原来是没有这个参数，为什么加上呢？
        //					是为报错时忠于原路径。如果为null或空字符串，则改为:库名路径/strCfgItemPath
        //		strStyle	风格 null认为是空字符串
        //					clear	表示清除下级
        //					autocreatedir	表示自动创建缺省的目录
        //		user	User对象，用来判断是否有权限，不能为null
        //		strCfgItemPath	配置事项路径，不带库名，不能为null或空字符串。???可以与strResPath一起用，但易乱
        //		strError	out参数，返回出错信息
        // return:
        //      -1  一般性错误
        //		-4	未指定路径对应的对象
        //		-6	权限不够
        //		-8	目录已存在
        //		-9	存在其它类型的事项
        //		0	成功
        // 线：不安全
        public int WriteDirCfgItem(string strCfgItemPath,
            string strStyle,
            User user,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strCfgItemPath) == true)
            {
                strError = "WriteDirCfgItem()调入错误，strCfgItemPath不能为null或空字符串。";
                return -1;
            }

            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count > 1)
            {
                strError = "服务器总配置文件不合法，路径为'" + strCfgItemPath + "'的配置事项对应的节点有'" + Convert.ToString(list.Count) + "'个。";
                return -1;
            }

            string strExistRights = "";
            bool bHasRight = false;

            // 已存在同名配置事项的情况
            if (list.Count == 1)
            {
                XmlNode node = list[0];
                if (node.Name == "file")
                {
                    strError = "服务器已存在路径为'" + strCfgItemPath + "'的配置文件，不能用目录覆盖文件。";
                    return -9;
                }
                if (node.Name == "database")
                {
                    strError = "服务器已存在名为'" + strCfgItemPath + "'的数据库，不能用目录覆盖数据库。";
                    return -9;
                }

                if (StringUtil.IsInList("clear", strStyle) == true)
                {
                    // 如果配置事项已存在，则检索是否有clear权限
                    string strPathForRights = strCfgItemPath;
                    bHasRight = user.HasRights(strPathForRights,
                        ResType.Directory,
                        "clear",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'清空下级(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    // 清空目录
                    // return:
                    //		-1	出错
                    //      -4  未指定路径对应的对象
                    //		0	成功
                    return this.ClearDirCfgItem(strCfgItemPath,
                        node,
                        out strError);
                }
                else
                {
                    strError = "服务器已存在路径为'" + strCfgItemPath + "'的配置目录。";
                    return -8;
                }
            }


            //***************************************

            bHasRight = user.HasRights(strCfgItemPath,
                ResType.Directory,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'清空下级(clear)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            // return:
            //		-1	出错
            //		0	成功
            nRet = this.AutoCreateDirCfgItem(strCfgItemPath,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 写文件配置事项
        // return:
        //      -1  一般性错误
        //      -2  时间戳不匹配
        //      -4  自动创建目录时，未找到上级
        //		-6	权限不够
        //		-9	存在其它类型的事项
        //		0	成功
        // 线程，不安全的
        internal int WriteFileCfgItem(string strCfgItemPath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            User user,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            int nRet = 0;

            Debug.Assert(user != null, "WriteFileCfgItem()调用错误，user对象不能为null");

            //------------------------------------------------
            // 检查输入参数，并规范化输入参数
            //--------------------------------------------------
            if (lTotalLength <= -1)
            {
                strError = "WriteFileCfgItem()调用错误，lTotalLength值为'" + Convert.ToString(lTotalLength) + "'不合法，必须大于等于0。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";
            if (strRanges == null)
                strRanges = null;
            if (strMetadata == null)
                strMetadata = "";

            if (baSource == null && streamSource == null)
            {
                strError = "WriteFileCfgItem()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteFileCfgItem()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }

            if (strCfgItemPath == null || strCfgItemPath == "")
            {
                strError = "WriteFileCfgItem()调用错误，strResPath不能为null或空字符串。";
                return -1;
            }

            //------------------------------------------------
            // 开始做事情
            //--------------------------------------------------

            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count > 1)
            {
                strError = "服务器总配置文件不合法，路径为'" + strCfgItemPath + "'的配置事项对应的节点有'" + Convert.ToString(list.Count) + "'个。";
                return -1;
            }

            string strExistRights = "";
            bool bHasRight = false;


            //------------------------------------------------
            // 已存在同名配置事项的情况
            //--------------------------------------------------

            if (list.Count == 1)
            {
                XmlNode node = list[0];
                if (node.Name == "dir")
                {
                    strError = "服务器已存在路径为'" + strCfgItemPath + "'的配置目录，不能用文件覆盖目录。";
                    return -9;
                }
                if (node.Name == "database")
                {
                    strError = "服务器已存在名为'" + strCfgItemPath + "'的数据库，不能用文件覆盖数据库。";
                    return -9;
                }

                // 如果配置事项已存在，则检索是否有overwrite权限
                string strPathForRights = strCfgItemPath;
                bHasRight = user.HasRights(strPathForRights,
                    ResType.File,
                    "overwrite",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'覆盖(overwrite)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // 如果按正规的渠道创建配置文件，
                // 则内存对象中已存在，那么物理文件名一定存在，则物理文件一定存在
                string strLocalPath = "";
                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                nRet = this.GetFileCfgItemLacalPath(strCfgItemPath,
                    out strLocalPath,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet != -4)
                        return -1;
                }

                goto DOWRITE;
            }


            //------------------------------------------------
            // 不存在配置事项的情况
            //--------------------------------------------------


            string strParentCfgItemPath = ""; //父亲的路径
            string strThisCfgItemName = ""; //本配置事项的名称
            int nIndex = strCfgItemPath.LastIndexOf('/');
            if (nIndex != -1)
            {
                strParentCfgItemPath = strCfgItemPath.Substring(0, nIndex);
                strThisCfgItemName = strCfgItemPath.Substring(nIndex + 1);
            }
            else
            {
                strThisCfgItemName = strCfgItemPath;
            }

            XmlNode nodeParent = null;
            // 对上级路径进行检查
            if (strParentCfgItemPath != "")
            {
                List<XmlNode> parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                    strParentCfgItemPath);
                if (parentNodes.Count > 1)
                {
                    nIndex = strCfgItemPath.LastIndexOf("/");
                    string strTempParentPath = strCfgItemPath.Substring(0, nIndex);
                    strError = "服务器端路径为'" + strTempParentPath + "'的配置事项有'" + Convert.ToString(parentNodes.Count) + "'个，配置文件不合法。";
                    return -1;
                }

                if (parentNodes.Count == 1)
                {
                    nodeParent = parentNodes[0];
                }
                else
                {

                    if (StringUtil.IsInList("autocreatedir", strStyle, true) == false)
                    {
                        nIndex = strCfgItemPath.LastIndexOf("/");
                        string strTempParentPath = strCfgItemPath.Substring(0, nIndex);
                        strError = "未找到路径为'" + strTempParentPath + "'配置事项，无法创建下级文件。";
                        return -4;
                    }

                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.AutoCreateDirCfgItem(strParentCfgItemPath,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    parentNodes = DatabaseUtil.GetNodes(this.NodeDbs,
                        strParentCfgItemPath);
                    if (parentNodes.Count != 1)
                    {
                        strError = "WriteFileCfgItem()，自动创建好上级目录了，此时不可能找不到路径为'" + strParentCfgItemPath + "'的配置事项了。";
                        return -1;
                    }

                    nodeParent = parentNodes[0];
                }
            }
            else
            {
                nodeParent = this.NodeDbs;
            }


            // 检查上级是否有指定权限
            bHasRight = user.HasRights(strCfgItemPath,
                ResType.File,
                "create",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "',对'" + strCfgItemPath + "',没有'创建(create)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }


            // return:
            //		-1	出错
            //		0	成功
            nRet = this.SetFileCfgItem(strParentCfgItemPath,
                nodeParent,
                strThisCfgItemName,
                out strError);
            if (nRet == -1)
                return -1;


        DOWRITE:

            string strFilePath = "";//GetCfgItemLacalPath(strCfgItemPath);
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            nRet = this.GetFileCfgItemLacalPath(strCfgItemPath,
                out strFilePath,
                out strError);
            if (nRet != 0)
            {
                if (nRet != -4)
                    return -1;
            }

            string strTempPath = strCfgItemPath;
            string strFirstPart = StringUtil.GetFirstPartPath(ref strTempPath);
            Database db = this.GetDatabase(strFirstPart);
            if (db != null)
            {

                // return:
                //		-1  一般性错误
                //      -2  时间戳不匹配
                //		0	成功
                return db.WriteFileForCfgItem(strCfgItemPath,
                    strFilePath,
                     strRanges,
                     lTotalLength,
                     baSource,
                     streamSource,
                     strMetadata,
                     strStyle,
                     baInputTimestamp,
                     out baOutputTimestamp,
                     out strError);
            }
            else
            {
                // 不从属于某一个数据库的配置文件
                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		0	成功
                return this.WriteFileForCfgItem(strFilePath,
                    strRanges,
                    lTotalLength,
                    baSource,
                    streamSource,
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    out baOutputTimestamp,
                    out strError);
            }
        }

        // 为文件配置事项写文件
        // parameters:
        //		strFilePath 目标文件路径，不能为null或空字符串
        //		strRanges	存放区域，可以为null或""表示0-sourceBuffer.Length-1的区域
        //		nTotalLength	总长度，可以为0
        //		baSource	内容字节数组，可以为null
        //		streamSource	内容流，可以为null
        //		strMetadata	元数据信息，可以为null或""
        //		inputTimestamp	输入的时间戳，可以为null
        //		outputTimestamp	out参数，返回实际的时间戳
        //		strError	out参数，返回出错信息
        // return:
        //		-1	一般性错误
        //		-2	时间戳不匹配
        //		0	成功
        // 线: 不安全
        // 说明: 这种函数的执行过程会首先检查一下本次是不是一次发来
        // 全部的内容，如果是，则直接写目标文件，不再使用临时文件
        // 如果不是才使用临时文件，并且判断ranges是否以满，再做相应的处理
        // 也有可能是新建一个文件
        internal int WriteFileForCfgItem(string strFilePath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            // --------------------------------------------------------
            // 检查输入参数，并规范化输入参数
            // --------------------------------------------------------
            if (String.IsNullOrEmpty(strFilePath) == true)
            {
                strError = "WriteFileForCfgItem()调用错误，strFilePath参数不能继null或空字符串。";
                return -1;
            }
            if (lTotalLength <= -1)
            {
                strError = "WriteFileForCfgItem()调用错误，lTotalLength参数的值不能为'" + Convert.ToString(lTotalLength) + "',必须大于等于0。";
                return -1;
            }

            if (strStyle == null)
                strStyle = "";
            if (strRanges == null)
                strRanges = null;
            if (strMetadata == null)
                strMetadata = "";

            if (baSource == null && streamSource == null)
            {
                strError = "WriteFileForCfgItem()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteFileForCfgItem()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }


            // --------------------------------------------------------
            // 检查输入参数，并规范化输入参数
            // --------------------------------------------------------

            string strNewFilePath = DatabaseUtil.GetNewFileName(strFilePath);

            //*************************************************
            // 检查时间戳,当有当配置文件存在时
            if (File.Exists(strFilePath) == true
                || File.Exists(strNewFilePath) == true)
            {
                if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                {
                    if (File.Exists(strNewFilePath) == true)
                        baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFilePath);
                    else
                        baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                    if (ByteArray.Compare(baOutputTimestamp, baInputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }
            }
            else
            {
                FileStream s = File.Create(strFilePath);
                s.Close();
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
            }


            //**************************************************
            long lCurrentLength = 0;

            //if (lTotalLength == 0)
            //	goto END1;

            if (baSource != null)
            {
                if (baSource.Length == 0)
                {
                    if (strRanges != "")
                    {
                        strError = "WriteCfgFileByRange()，当baSource参数的长度为0时，strRanges的值却为'" + strRanges + "'，不匹配，应为空字符串。";
                        return -1;
                    }
                    //把写到metadata里的尺寸设好
                    FileInfo fi = new FileInfo(strFilePath);
                    lCurrentLength = fi.Length;
                    fi = null;

                    //goto END1;
                }
            }
            else
            {
                if (streamSource.Length == 0)
                {
                    if (strRanges != "")
                    {
                        strError = "WriteCfgFileByRange()，当streamSource参数长度为0时，strRanges的值却为'" + strRanges + "'，不匹配，应为空字符串。";
                        return -1;
                    }
                    //把写到metadata里的尺寸设好
                    FileInfo fi = new FileInfo(strFilePath);
                    lCurrentLength = fi.Length;
                    fi = null;

                    //goto END1;
                }
            }

            //******************************************
            // 写数据
            if (strRanges == null || strRanges == "")
            {
                if (lTotalLength > 0)
                    strRanges = "0-" + Convert.ToString(lTotalLength - 1);
                else
                    strRanges = "";
            }
            string strRealRanges = strRanges;

            // 检查本次传来的范围是否是完整的文件。
            bool bIsComplete = false;
            if (lTotalLength == 0)
                bIsComplete = true;
            else
            {
                //		-1	出错 
                //		0	还有未覆盖的部分 
                //		1	本次已经完全覆盖
                int nState = RangeList.MergContentRangeString(strRanges,
                    "",
                    lTotalLength,
                    out strRealRanges);
                if (nState == 1)
                    bIsComplete = true;
            }


            if (bIsComplete == true)
            {
                if (baSource != null)
                {
                    if (baSource.Length != lTotalLength)
                    {
                        strError = "范围'" + strRanges + "'与数据字节数组长度'" + Convert.ToString(baSource.Length) + "'不符合。";
                        return -1;
                    }
                }
                else
                {
                    if (streamSource.Length != lTotalLength)
                    {
                        strError = "范围'" + strRanges + "'与流长度'" + Convert.ToString(streamSource.Length) + "'不符合。";
                        return -1;
                    }
                }
            }


            RangeList rangeList = new RangeList(strRealRanges);

            // 开始写数据
            Stream target = null;
            if (bIsComplete == true)
                target = File.Create(strFilePath);  //一次性发完，直接写到文件
            else
                target = File.Open(strNewFilePath, FileMode.OpenOrCreate);
            try
            {
                int nStartOfBuffer = 0;
                for (int i = 0; i < rangeList.Count; i++)
                {
                    RangeItem range = (RangeItem)rangeList[i];
                    int nStartOfTarget = (int)range.lStart;
                    int nLength = (int)range.lLength;
                    if (nLength == 0)
                        continue;

                    // 移动目标流的指针到指定位置
                    target.Seek(nStartOfTarget,
                        SeekOrigin.Begin);

                    if (baSource != null)
                    {
                        target.Write(baSource,
                            nStartOfBuffer,
                            nLength);


                        nStartOfBuffer += nLength; //2005.11.11加
                    }
                    else
                    {
                        StreamUtil.DumpStream(streamSource,
                            target,
                            nLength);
                    }
                }
            }
            finally
            {
                target.Close();
            }

            string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);

            // 如果一次性写满的情况，需要做下列几件事情:
            // 1.时间戳以目标文件计算
            // 2.写到metadata的长度为目标文件总长度
            // 3.如果存在临时辅助文件，则删除这些文件。
            if (bIsComplete == true)
            {
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                lCurrentLength = lTotalLength;

                // 删除辅助文件
                if (File.Exists(strNewFilePath) == true)
                    File.Delete(strNewFilePath);
                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);

                goto END1;
            }


            //****************************************
            //处理辅助文件
            bool bFull = false;
            string strResultRange = "";
            if (strRanges == "" || strRanges == null)
            {
                bFull = true;
            }
            else
            {
                string strOldRanges = "";
                if (File.Exists(strRangeFileName) == true)
                    strOldRanges = FileUtil.File2StringE(strRangeFileName);
                int nState1 = RangeList.MergContentRangeString(strRanges,
                    strOldRanges,
                    lTotalLength,
                    out strResultRange);
                if (nState1 == 1)
                    bFull = true;
            }

            // 如果文件已满，需要做下列几件事情:
            // 1.按最大长度截临时文件 
            // 2.将临时文件拷到目标文件
            // 3.删除new,range辅助文件
            // 4.时间戳以目标文件计算
            // 5.metadata的长度为目标文件的总长度
            if (bFull == true)
            {
                Stream s = new FileStream(strNewFilePath,
                    FileMode.OpenOrCreate);
                try
                {
                    s.SetLength(lTotalLength);
                }
                finally
                {
                    s.Close();
                }

                // 用.new临时文件替换直接文件
                File.Copy(strNewFilePath,
                    strFilePath,
                    true);

                File.Delete(strNewFilePath);

                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);
                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);

                lCurrentLength = lTotalLength;
            }
            else
            {

                //如果文件未满，需要做下列几件事情：
                // 1.把目前的range写到range辅助文件
                // 2.时间戳以临时文件计算
                // 3.metadata的长度为-1，即未知的情况

                FileUtil.String2File(strResultRange,
                    strRangeFileName);

                lCurrentLength = -1;

                baOutputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFilePath);
            }

        END1:

            // 写metadata
            if (strMetadata != "")
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);

                // 取出旧的数据进行合并
                string strOldMetadata = "";
                if (File.Exists(strMetadataFileName) == true)
                    strOldMetadata = FileUtil.File2StringE(strMetadataFileName);
                if (strOldMetadata == "")
                    strOldMetadata = "<file/>";

                string strResultMetadata;
                // return:
                //		-1	出错
                //		0	成功
                int nRet = DatabaseUtil.MergeMetadata(strOldMetadata,
                    strMetadata,
                    lCurrentLength,
                    out strResultMetadata,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把合并的新数据写到文件里
                FileUtil.String2File(strResultMetadata,
                    strMetadataFileName);
            }
            return 0;
        }


        // GetRes()用range不太好实现,因为原来当请求的长度超过允许的长度时,长度会自动为截取
        // 而如果用range来表示,则不知该截短哪部分好。
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		lStart	起始长度
        //		lLength	总长度,-1:从start到最后
        //		strStyle	取资源的风格，以逗豆间隔的字符串
        /*
        strStyle用法

        1.控制数据存放的位置
        content		把返回的数据放到字节数组参数里
        attachment	把返回的数据放到附件中,并返回附件的id

        2.控制返回的数据
        metadata	返回metadata信息
        timestamp	返回timestamp
        length		数据总长度，始终都有值
        data		返回数据体
        respath		返回记录路径,目前始终都有值
        all			返回所有值

        3.控制记录号
        prev		前一条
        prev,self	自己或前一条
        next		下一条
        next,self	自己或下一条
        放到strOutputResPath参数里

        */
        //		baContent	用content字节数组返回资源内容
        //		strAttachmentID	用附件返回资源内容
        //		strMetadata	返回的metadata内容
        //		strOutputResPath	返回的资源路径
        //		baTimestamp	返回的资源时间戳
        // return:
        //		-1	一般性错误
        //		-4	未找到路径指定的资源
        //		-5	未找到数据库
        //		-6	没有足够的权限
        //		-7	路径不合法
        //		-10	未找到记录xpath对应的节点
        //		>= 0	成功，返回最大长度
        //      nAdditionError -50 有一个以上下级资源记录不存在
        // 线：安全
        public long GetRes(string strResPath,
            int nStart,
            int nLength,
            string strStyle,
            User user,
            int nMaxLength,
            out byte[] baData,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp,
            out int nAdditionError, // 附加的错误码
            out string strError)
        {
            baData = null;
            strMetadata = "";
            strOutputResPath = "";
            baOutputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            //------------------------------------------------
            //检查输入参数是否合法，并规范输入参数
            //---------------------------------------------------

            Debug.Assert(user != null, "GetRes()调用错误，user对象不能为null。");

            if (user == null)
            {
                strError = "GetRes()调用错误，user对象不能为null。";
                return -1;
            }
            if (String.IsNullOrEmpty(strResPath) == true)
            {
                strError = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                return -7;
            }
            if (nStart < 0)
            {
                strError = "GetRes()调用错误，nStart不能小于0。";
                return -1;
            }
            if (strStyle == null)
                strStyle = "";


            //------------------------------------------------
            // 开始做事情
            //---------------------------------------------------

            //******************加库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK
			this.WriteDebugInfo("GetRes()，对库集合加读锁。");
#endif
            try
            {
                long nRet = 0;

                bool bRecordPath = this.IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    //当配置事项处理
                    // return:
                    //		-1  一般性错误
                    //		-4	未找到路径对应的对象
                    //		-6	没有足够的权限
                    //		>= 0    成功 返回最大长度
                    nRet = this.GetFileCfgItem(strResPath,
                        nStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        user,
                        out baData,
                        out strMetadata,
                        out baOutputTimestamp,
                        out strError);


                    if (StringUtil.IsInList("outputpath", strStyle) == true)
                    {
                        strOutputResPath = strResPath;
                    }
                }
                else
                {

                    // 判断资源类型
                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第1层*************
                    // 到此为止，strPath不含数据库名了,下面的路径有两种情况:cfgs;其余都被当作记录id
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'路径不合法，未指定库的下级。";
                        return -7;
                    }

                    // 从这里区别是数据库还是服务器端配置文件

                    // 根据资源类型，写资源
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        strError = "未找到'" + strDbName + "'库";
                        return -5;
                    }

                    bool bObject = false;
                    string strRecordID = "";
                    string strObjectID = "";
                    string strXPath = "";

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath记录号层了，下级分情况判断

                    strRecordID = strFirstPart;
                    // 只到记录号层的路径
                    if (strPath == "")
                    {
                        bObject = false;
                        goto DOGET;
                    }

                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath
                    if (strFirstPart != "object"
                        && strFirstPart != "xpath")
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,第3级必须是'object'或'xpath'";
                        return -7;
                    }
                    if (strPath == "")  //object或xpath下级必须有值
                    {
                        strError = "资源路径 '" + strResPath + "' 不合法,当第3级是'object'或'xpath'，第4级必须有内容。";
                        return -7;
                    }

                    if (strFirstPart == "object")
                    {
                        strObjectID = strPath;
                        bObject = true;
                    }
                    else
                    {
                        strXPath = strPath;
                        bObject = false;
                    }

                    ///////////////////////////////////
                ///开始做事情
                //////////////////////////////////////////

                DOGET:


                    // 检查对数据库中记录的权限
                    string strExistRights = "";
                    bool bHasRight = user.HasRights(strDbName + "/" + strRecordID,
                        ResType.Record,
                        "read",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'库没有'读记录(read)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    if (bObject == true)  //对像
                    {
                        //		-1  出错
                        //		-4  记录不存在
                        //		>=0 资源总长度
                        nRet = db.GetObject(strRecordID,
                            strObjectID,
                            nStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out baData,
                            out strMetadata,
                            out baOutputTimestamp,
                            out strError);

                        if (StringUtil.IsInList("outputpath", strStyle) == true)
                        {
                            strOutputResPath = strDbName + "/" + strRecordID + "/object/" + strObjectID;

                        }
                    }
                    else
                    {
                        string strOutputID;
                        // return:
                        //		-1  出错
                        //		-4  未找到记录
                        //      -10 记录局部未找到
                        //		>=0 资源总长度
                        //      nAdditionError -50 有一个以上下级资源记录不存在
                        nRet = db.GetXml(strRecordID,
                            strXPath,
                            nStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out baData,
                            out strMetadata,
                            out strOutputID,
                            out baOutputTimestamp,
                            true,
                            out nAdditionError,
                            out strError);
                        if (StringUtil.IsInList("outputpath", strStyle) == true)
                        {
                            strRecordID = strOutputID;
                        }

                        if (StringUtil.IsInList("outputpath", strStyle) == true)
                        {
                            if (strXPath == "")
                                strOutputResPath = strDbName + "/" + strRecordID;
                            else
                                strOutputResPath = strDbName + "/" + strRecordID + "/xpath/" + strXPath;

                        }
                    }
                }

                return nRet;

            }
            finally
            {
                //******************对库集合解读锁******
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
			this.WriteDebugInfo("GetRes()，对库集合解读锁。");
#endif
            }
        }

        // 检查一个路径是否是数据库记录路径
        private bool IsRecordPath(string strResPath)
        {
            string[] paths = strResPath.Split(new char[] { '/' });
            if (paths.Length >= 2)
            {
                if (StringUtil.IsPureNumber(paths[1]) == true
                    || paths[1] == "?"
                    || paths[1] == "-1")
                {
                    return true;
                }
            }
            return false;
        }


        // 按指定范围读配置文件
        // strRoleName:  角色名,大小写均可
        // 其它参数同GetXml(),无strOutputResPath参数
        // 线: 安全的
        // return:
        //		-1  一般性错误
        //		-4	未找到路径对应的对象
        //		-6	没有足够的权限
        //		>= 0    成功 返回最大长度
        // 线：安全
        public int GetFileCfgItem(string strCfgItemPath,
            int nStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            User user,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] outputTimestamp,
            out string strError)
        {
            strMetadata = "";
            destBuffer = null;
            outputTimestamp = null;
            strError = "";

            // 检查当前帐户对配置事项的权限，暂时不报权限的错，检查完对象是否存在，再报错
            string strExistRights = "";
            bool bHasRight = user.HasRights(strCfgItemPath,
                ResType.File,
                "read",
                out strExistRights);


            //**********对数据库集合加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCfgFile()，对'" + this.GetCaption("zh-cn") + "'数据库集合加读锁。");
#endif
            try
            {

                string strFilePath = "";//this.GetCfgItemLacalPath(strCfgItemPath);
                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                int nRet = this.GetFileCfgItemLacalPath(strCfgItemPath,
                    out strFilePath,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet == -2)
                        return -4;
                    return -1;
                }

                // 此时再报权限的错
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对路径为'" + strCfgItemPath + "'的配置事项没有'读(read)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }

                // return:
                //		-1      出错
                //		>= 0	成功，返回最大长度
                return DatabaseCollection.GetFileForCfgItem(strFilePath,
                    nStart,
                    nLength,
                    nMaxLength,
                    strStyle,
                    out destBuffer,
                    out strMetadata,
                    out outputTimestamp,
                    out strError);
            }
            finally
            {
                //****************对数据库集合解读锁**************
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK	
				this.container.WriteDebugInfo("GetCfgFile()，对'" + this.GetCaption("zh-cn") + "'数据库集合解读锁。");
#endif
            }
        }

        // 为GetCfgItem服务器的内部函数
        // return:
        //		-1      出错
        //		>= 0	成功，返回最大长度
        public static int GetFileForCfgItem(string strFilePath,
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
            strMetadata = "";
            outputTimestamp = null;
            strError = "";

            int nTotalLength = 0;
            FileInfo file = new FileInfo(strFilePath);
            if (file.Exists == false)
            {
                strError = "服务器不存在物理路径为'" + strFilePath + "'的文件。";
                return -1;
            }

            // 1.取时间戳
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);
                if (File.Exists(strNewFileName) == true)
                {
                    outputTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFileName);
                }
                else
                {
                    outputTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);
                }
            }

            // 2.取元数据
            if (StringUtil.IsInList("metadata", strStyle) == true)
            {
                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == true)
                {
                    strMetadata = FileUtil.File2StringE(strMetadataFileName);
                }
            }

            // 3.取range
            if (StringUtil.IsInList("range", strStyle) == true)
            {
                string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == true)
                {
                    string strRange = FileUtil.File2StringE(strRangeFileName);
                }
            }

            // 4.长度
            nTotalLength = (int)file.Length;

            // 5.有data风格时,才会取数据
            if (StringUtil.IsInList("data", strStyle) == true)
            {
                if (nLength == 0)  // 取0长度
                {
                    destBuffer = new byte[0];
                    return nTotalLength;
                }
                // 检查范围是否合法
                int nOutputLength;
                // return:
                //		-1  出错
                //		0   成功
                int nRet = DatabaseUtil.GetRealLength(nStart,
                    nLength,
                    nTotalLength,
                    nMaxLength,
                    out nOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                FileStream s = new FileStream(strFilePath,
                    FileMode.Open);
                try
                {
                    destBuffer = new byte[nOutputLength];
                    s.Seek(nStart, SeekOrigin.Begin);
                    s.Read(destBuffer,
                        0,
                        nOutputLength);
                }
                finally
                {
                    s.Close();
                }
            }
            return nTotalLength;
        }

        // 得到一个文件配置事项的本地文件绝对路径
        // parameters:
        //		strFileCfgItemPath	文件配置事项的路径，格式为'dir1/dir2/file'
        //		strLocalPath	out参数，返回对应的本地文件绝对路径	
        //		strError	out参数，返回出错信息
        // return:
        //		-1	一般性错误，比如调用错误，参数不合法等
        //		-2	没找到节点
        //		-3	localname属性未定义或为值空
        //		-4	localname在本地不存在
        //		-5	存在多个节点
        //		0	成功
        // 线：不安全
        public int GetFileCfgItemLacalPath(string strFileCfgItemPath,
            out string strLocalPath,
            out string strError)
        {
            strLocalPath = "";
            strError = "";

            if (strFileCfgItemPath == ""
                || strFileCfgItemPath == null)
            {
                strError = "GetCfgItemLacalPath()的strPath参数值不能为null或空字符串";
                return -1;
            }
            List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                strFileCfgItemPath);
            if (nodes.Count == 0)
            {
                strError = "服务器上未定义路径为'" + strFileCfgItemPath + "'的配置文件。";
                return -2;
            }
            if (nodes.Count > 1)
            {
                strError = "服务器上路径为'" + strFileCfgItemPath + "'的配置事项有'" + Convert.ToString(nodes.Count) + "'个，配置文件不合法。";
                return -5;
            }

            XmlNode nodeFile = nodes[0];

            string strPureFileName = DomUtil.GetAttr(nodeFile, "localname");
            if (strPureFileName == "")
            {
                strError = "服务器上路径为'" + strFileCfgItemPath + "'的文件配置事项未定义对应的物理文件。";
                return -3;
            }

            string strLocalDir = DatabaseUtil.GetLocalDir(this.NodeDbs,
                nodeFile.ParentNode);

            string strRealPath = "";
            if (strLocalDir == "")
                strRealPath = this.DataDir + "\\" + strPureFileName;
            else
                strRealPath = this.DataDir + "\\" + strLocalDir + "\\" + strPureFileName;

            strLocalPath = strRealPath;
            if (File.Exists(strRealPath) == false)
            {
                strError = "服务器上路径为'" + strFileCfgItemPath + "'的文件配置事项对应的物理文件在本地不存在。";
                return -4;
            }
            return 0;
        }


        // 删除资源，可以是记录 或 配置事项，不支持对象资源或部分记录体
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录
        //						配置事项: 库名/配置事项路径
        //						记录: 库名/记录号
        //		user	当前帐户对象，不能为null
        //		baInputTimestamp	输入的时间戳
        //		baOutputTimestamp	out参数，返回时间戳
        //		strError	out参数，返回出错信息
        // return:
        //      -1	一般性错误，例如输入参数不合法等
        //      -2	时间戳不匹配
        //      -4	未找到路径对应的资源
        //      -5	未找到数据库
        //      -6	没有足够的权限
        //      -7	路径不合法
        //      0	操作成功
        // 说明: 
        // 1)删除需要当前帐户对将被删除的记录的有delete权限		
        // 2)删除记录的明确含义是删除记录体，并且删除该记录包含的所有对象资源
        // 3)删除配置目录不要求时间戳,同时baOutputTimestamp也是null
        public int DeleteRes(string strResPath,
            User user,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";

            //-----------------------------------------
            //对输入参数做例行检查
            //---------------------------------------
            if (strResPath == null || strResPath == "")
            {
                strError = "DeleteRes()调用错误，strResPath参数不能为null或空字符串。";
                return -1;
            }
            if (user == null)
            {
                strError = "DeleteRes()调用错误，user参数不能为null。";
                return -1;
            }


            //-----------------------------------------
            //开始做事情 
            //---------------------------------------

            //******************加库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK
			this.WriteDebugInfo("CheckDbsTailNoSafety()，对库集合加读锁。");
#endif
            try
            {
                int nRet = 0;

                bool bRecordPath = this.IsRecordPath(strResPath);
                if (bRecordPath == false)
                {
                    // 也可能是数据库对象


                    // 删除实际的物理文件
                    //      -1  一般性错误
                    //      -2  时间戳不匹配
                    //      -4  未找到路径对应的资源
                    //      -6  没有足够的权限
                    //      0   成功
                    nRet = this.DeleteCfgItem(user,
                        strResPath,
                        baInputTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    if (nRet <= -1)
                        return nRet;
                }
                else
                {

                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);
                    if (strPath == "")
                    {
                        strError = "资源路径'" + strResPath + "'不合法，未指定库的下级。";
                        return -7;
                    }

                    // 根据资源类型，写资源
                    Database db = this.GetDatabase(strDbName);
                    if (db == null)
                    {
                        strError = "没找到名为'" + strDbName + "'的数据库。";
                        return -5;
                    }

                    string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含cfgs或记录号层了，下级分情况判断
                    // strFirstPart可能是为cfg或记录号

                    string strRecordID = strFirstPart;

                    // 检查当前帐户是否有删除记录
                    string strExistRights = "";
                    bool bHasRight = user.HasRights(strResPath,//db.GetCaption("zh-cn"),
                        ResType.Record,
                        "delete",
                        out strExistRights);
                    if (bHasRight == false)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对'" + strDbName + "'数据库没有'删除记录(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                        return -6;
                    }

                    // return:
                    //		-1  一般性错误
                    //		-2  时间戳不匹配
                    //      -4  未找到记录
                    //		0   成功
                    nRet = db.DeleteRecord(strRecordID,
                        baInputTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    if (nRet <= -1)
                        return nRet;
                }
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*************对库集合解读锁***********
#if DEBUG_LOCK
				this.WriteDebugInfo("CheckDbsTailNoSafety()，对库集合解读锁。");
#endif
            }

            //及时保存database.xml // 是用加锁的函数吗？
            if (this.Changed == true)
                this.SaveXmlSafety();

            return 0;

        }

        // 删除一个配置事项，可以是目录，也可以是文件
        // return:
        //      -1  一般性错误
        //      -2  时间戳不匹配
        //      -4  未找到路径对应的资源
        //      -6  没有足够的权限
        //      0   成功
        public int DeleteCfgItem(User user,
            string strCfgItemPath,
            byte[] intputTimestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            if (strCfgItemPath == null
                || strCfgItemPath == "")
            {
                strError = "DeleteCfgItem()调用错误，strCfgItemPath参数值不能为null或空字符串。";
                return -1;
            }

            List<XmlNode> nodes = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (nodes.Count == 0)
            {
                strError = "服务器不存在路径为'" + strCfgItemPath + "'的配置事项。";
                return -4;
            }
            if (nodes.Count != 1)
            {
                strError = "服务器上路径为'" + strCfgItemPath + "'的配置事项个数为'" + Convert.ToString(nodes.Count) + "'，database.xml配置文件异常。";
                return -1;
            }


            string strExistRights = "";
            bool bHasRight = false;

            XmlNode node = nodes[0];

            if (node.Name == "dir")
            {
                // 检查当前帐户是否有删除记录'
                bHasRight = user.HasRights(strCfgItemPath,
                    ResType.Directory,
                    "delete",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + user.Name + "'，对'" + strCfgItemPath + "'配置事项没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                    return -6;
                }
                string strDir = DatabaseUtil.GetLocalDir(this.NodeDbs, node).Trim();
                Directory.Delete(this.DataDir + "\\" + strDir, true);
                node.ParentNode.RemoveChild(node);
                return 0;
            }
            else if (String.Compare(node.Name, "database", true) == 0)
            {

            }


            // 检查当前帐户是否有删除记录'
            bHasRight = user.HasRights(strCfgItemPath,
                ResType.File,
                "delete",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对'" + strCfgItemPath + "'配置事项没有'删除(delete)'权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            string strFilePath = "";//GetCfgItemLacalPath(strCfgItemPath);
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            int nRet = this.GetFileCfgItemLacalPath(strCfgItemPath,
                out strFilePath,
                out strError);
            if (nRet != 0)
            {
                if (nRet == -1 || nRet == -5)
                    return -1;

            }
            if (strFilePath != "")
            {
                string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);

                if (File.Exists(strFilePath) == true)
                {

                    byte[] oldTimestamp = null;
                    if (File.Exists(strNewFileName) == true)
                        oldTimestamp = DatabaseUtil.CreateTimestampForCfg(strNewFileName);
                    else
                        oldTimestamp = DatabaseUtil.CreateTimestampForCfg(strFilePath);

                    outputTimestamp = oldTimestamp;
                    if (ByteArray.Compare(oldTimestamp, intputTimestamp) != 0)
                    {
                        strError = "时间戳不匹配";
                        return -2;
                    }
                }

                File.Delete(strNewFileName);
                File.Delete(strFilePath);

                string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);
                if (File.Exists(strRangeFileName) == false)
                    File.Delete(strRangeFileName);

                string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
                if (File.Exists(strMetadataFileName) == false)
                    File.Delete(strMetadataFileName);
            }
            node.ParentNode.RemoveChild(node);

            this.Changed = true;
            this.SaveXml();

            return 0;
        }



        // 根据服务器上的指定路径列出其下级的事项
        // parameters:
        //		strPath	路径,不带服务器部分，
        //				格式为: "数据库名/下级名/下级名",
        //				当为null或者为""时，表示列出该服务器下所有的数据库
        //		lStart	起始位置,从0开始 ,不能小于0
        //		lLength	长度 -1表示从lStart到最后
        //		strLang	语言版本 用标准字母表示法，如zh-cn
        //      strStyle    是否要列出所有语言的名字? "alllang"表示要列出全部语言
        //		items	 out参数，返回下级事项数组
        // return:
        //		-1  出错
        //      -6  权限不够
        //		0   正常
        // 说明	只有当前帐户对事项有"list"权限时，才能列出来。
        //		如果列本服务器的数据库时，对所有的数据库都没有list权限，都按错误处理，与没有数据库事项区分开。
        public int Dir(string strResPath,
            long lStart,
            long lLength,
            long lMaxLength,
            string strLang,
            string strStyle,
            User user,
            out ResInfoItem[] items,
            out int nTotalLength,
            out string strError)
        {
            items = new ResInfoItem[0];
            nTotalLength = 0;

            ArrayList aItem = new ArrayList();
            strError = "";
            int nRet = 0;
            //******************加库集合加读锁******
            this.m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK
			this.WriteDebugInfo("Dir()，对库集合加读锁。");
#endif
            try
            {

                if (strResPath == "" || strResPath == null)
                {
                    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    // 1.取服务器下的数据库

                    nRet = this.GetDirableChildren(user,
                        strLang,
                        strStyle,
                        out aItem,
                        out strError);
                    if (this.Count > 0 && aItem.Count == 0)
                    {
                        strError = "您的帐户名为'" + user.Name + "'，对所有的数据库都没有'显示(list)'权限。";
                        return -6;
                    }
                }
                else
                {
                    string strPath = strResPath;
                    string strDbName = StringUtil.GetFirstPartPath(ref strPath);

                    // 可以是数据库也可以是配置事项
                    if (strPath == "")
                    {
                        Database db = this.GetDatabase(strDbName);
                        if (db != null)
                        {
                            // return:
                            //		-1	出错
                            //		0	成功
                            nRet = db.GetDirableChildren(user,
                                strLang,
                                strStyle,
                                out aItem,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            goto END1;
                        }
                    }

                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.DirCfgItem(user,
                        strResPath,
                        out aItem,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*************对库集合解读锁***********
#if DEBUG_LOCK
				this.WriteDebugInfo("Dir()，对库集合解读锁。");
#endif
            }


        END1:
            // 列出实际需要的项
            nTotalLength = aItem.Count;
            int nOutputLength;
            // return:
            //		-1  出错
            //		0   成功
            nRet = DatabaseUtil.GetRealLength((int)lStart,
                (int)lLength,
                nTotalLength,
                (int)lMaxLength,
                out nOutputLength,
                out strError);
            if (nRet == -1)
                return -1;

            items = new ResInfoItem[(int)nOutputLength];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = (ResInfoItem)(aItem[i + (int)lStart]);
            }

            return 0;
        }


        // 得到某一指定路径strPath的可以显示的下级
        // parameters:
        //		oUser	当前帐户
        //		db	当前数据库
        //		strPath	配置事项的路径
        //		strLang	语言版本
        //		aItem	out参数，返回可以显示的下级
        //		strError	out参数，出错信息
        // return:
        //		-1	出错
        //		0	成功
        private int DirCfgItem(User user,
            string strCfgItemPath,
            out ArrayList aItem,
            out string strError)
        {
            strError = "";
            aItem = new ArrayList();

            if (this.NodeDbs == null)
            {
                strError = "服务器配置文件未定义<dbs>元素";
                return -1;
            }
            List<XmlNode> list = DatabaseUtil.GetNodes(this.NodeDbs,
                strCfgItemPath);
            if (list.Count == 0)
            {
                strError = "未找到路径为'" + strCfgItemPath + "'对应的事项。";
                return -1;
            }

            if (list.Count > 1)
            {
                strError = "服务器端总配置文件不合法，检查到路径为'" + strCfgItemPath + "'对应的节点有'" + Convert.ToString(list.Count) + "'个，有且只能有一个。";
                return -1;
            }
            XmlNode node = list[0];

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode child = node.ChildNodes[i];
                string strChildName = DomUtil.GetAttr(child, "name");
                if (strChildName == "")
                    continue;

                string strTempPath = strCfgItemPath + "/" + strChildName;
                string strExistRights;
                bool bHasRight = false;


                ResInfoItem resInfoItem = new ResInfoItem();
                resInfoItem.Name = strChildName;
                if (child.Name == "dir")
                {
                    bHasRight = user.HasRights(strTempPath,
                     ResType.Directory,
                     "list",
                     out strExistRights);
                    if (bHasRight == false)
                        continue;

                    resInfoItem.HasChildren = true;
                    resInfoItem.Type = 4;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");    // xietao 2006/6/5 add
                }
                else
                {
                    bHasRight = user.HasRights(strTempPath,
                        ResType.File,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = false;
                    resInfoItem.Type = 5;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");    // xietao 2006/6/5 add

                }
                aItem.Add(resInfoItem);
            }
            return 0;
        }

        // 列出服务器下当前帐户有显示权限的数据库
        // 线：不安全的
        // parameters:
        //      strStyle    是否要列出所有语言的名字? "alllang"表示要列出所有语言的名字
        public int GetDirableChildren(User user,
            string strLang,
            string strStyle,
            out ArrayList aItem,
            out string strError)
        {
            aItem = new ArrayList();
            strError = "";

            if (this.NodeDbs == null)
            {
                strError = "服装器配置文件不合法，未定义<dbs>元素";
                return -1;
            }

            foreach (XmlNode child in this.NodeDbs.ChildNodes)
            {
                string strChildName = DomUtil.GetAttr(child, "name");
                if (String.Compare(child.Name, "database", true) != 0
                    && strChildName == "")
                    continue;

                if (String.Compare(child.Name, "database", true) != 0
                    && String.Compare(child.Name, "dir", true) != 0
                    && String.Compare(child.Name, "file", true) != 0)
                {
                    continue;
                }

                string strExistRights;
                bool bHasRight = false;

                ResInfoItem resInfoItem = new ResInfoItem();
                if (String.Compare(child.Name, "database", true) == 0)
                {
                    string strID = DomUtil.GetAttr(child, "id");
                    Database db = this.GetDatabaseByID("@" + strID);
                    if (db == null)
                    {
                        strError = "未找到id为'" + strID + "'的数据库";
                        return -1;
                    }

                    bHasRight = user.HasRights(db.GetCaption("zh"),
                        ResType.Database,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;

                    if (StringUtil.IsInList("account", db.GetDbType(), true) == true)
                        resInfoItem.Style = 1;
                    else
                        resInfoItem.Style = 0;

                    resInfoItem.TypeString = db.GetDbType();

                    resInfoItem.Name = db.GetCaptionSafety(strLang);
                    resInfoItem.Type = 0;   // 数据库
                    resInfoItem.HasChildren = true;

                    // 如果要获得全部语言的名字
                    if (StringUtil.IsInList("alllang", strStyle) == true)
                    {
                        List<string> results = db.GetAllLangCaptionSafety();
                        string [] names = new string[results.Count];
                        results.CopyTo(names);
                        resInfoItem.Names = names;
                    }
                }
                else if (String.Compare(child.Name, "dir", true) == 0)
                {
                    bHasRight = user.HasRights(strChildName,
                        ResType.Directory,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = true;
                    resInfoItem.Type = 4;   // 目录
                    resInfoItem.Name = strChildName;

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");   // xietao 2006/6/5 add
                }
                else
                {
                    bHasRight = user.HasRights(strChildName,
                        ResType.File,
                        "list",
                        out strExistRights);
                    if (bHasRight == false)
                        continue;
                    resInfoItem.HasChildren = false;
                    resInfoItem.Name = strChildName;
                    resInfoItem.Type = 5;   // 文件?

                    resInfoItem.TypeString = DomUtil.GetAttr(child, "type");   // xietao 2006/6/5 add
                }
                aItem.Add(resInfoItem);
            }
            return 0;
        }

        // 根据用户名从库中查找用户记录，得到用户对象
        // 对象尚未进入集合, 因此无需为对象加锁
        // parameters:
        //		strBelongDb	用户从属的数据库,中文名称
        //      user        out参数，返回帐户对象
        //      strError    out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	未找到帐户
        //		1	找到了
        // 线：安全
        internal int ShearchUser(string strUserName,
            out User user,
            out string strError)
        {
            user = null;
            strError = "";

            int nRet = 0;

            DpResultSet resultSet = new DpResultSet();


            //*********对帐户库集合加读锁***********
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.WriteDebugInfo("ShearchUser()，对帐户库集合加读锁。");
#endif
            try
            {
                // return:
                //		-1	出错
                //		0	成功
                nRet = this.SearchUserInternal(strUserName,
                    resultSet,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            finally
            {
                //*********对帐户库集合解读锁*************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.m_dbColl.WriteDebugInfo("ShearchUser()，对帐户库集合解读锁。");
#endif
            }

            // 根据用户名没找到对应的帐户记录
            long lCount = resultSet.Count;
            if (lCount == 0)
                return 0;

            if (lCount > 1)
            {
                strError = "用户名'" + strUserName + "'对应多条记录";
                return -1;
            }

            // 按第一个帐户算
            DpRecord record = (DpRecord)resultSet[0];

            // 创建一个DpPsth实例
            DbPath path = new DbPath(record.ID);

            // 找到指定帐户数据库
            Database db = this.GetDatabaseSafety(path.Name);
            if (db == null)
            {
                strError = "未找到'" + strUserName + "'帐户对应的名为'" + path.Name + "'的数据库对象";
                return -1;
            }

            // 从帐户库中找到记录
            string strXml = "";
            // return:
            //      -1  出错
            //      -4  记录不存在
            //      0   正确
            nRet = db.GetXmlDataSafety(path.ID,
                out strXml,
                out strError);
            if (nRet <= -1)  // 将-4与-1都作为-1返回
                return -1;

            //加载到dom
            XmlDocument dom = new XmlDocument();
            //dom.PreserveWhitespace = true; //设PreserveWhitespace为true
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "加载用户 '" + strUserName + "' 的帐户记录到dom时出错,原因:" + ex.Message;
                return -1;
            }

            user = new User();
            // return:
            //      -1  出错
            //      0   成功
            nRet = user.Initial(
                record.ID,
                dom,
                db,
                this,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 根据记录路径得到数据库对象
        public Database GetDatabaseFromRecPath(string strRecPath)
        {
            // 创建一个DpPsth实例
            DbPath path = new DbPath(strRecPath);

            // 找到指定帐户数据库
            return this.GetDatabaseSafety(path.Name);
        }

                // 从所有帐户库的所有表中查找帐户
        // parameter
        //		strUserName 用户名
        //		resultSet   结果集,用于存放查找到的用户
        //      strError    out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        // 线：不安全
        private int SearchUserInternal(string strUserName,
            DpResultSet resultSet,
            out string strError)
        {
            strError = "";

            foreach (Database db in this)
            {
                if (StringUtil.IsInList("account", db.GetDbType()) == false)
                    continue;

                if (strUserName.Length > db.KeySize)
                    continue;

                string strWarning = "";
                SearchItem searchItem = new SearchItem();
                searchItem.TargetTables = "";
                searchItem.Word = strUserName;
                searchItem.Match = "exact";
                searchItem.Relation = "=";
                searchItem.DataType = "string";
                searchItem.MaxCount = -1;
                searchItem.OrderBy = "";

                // 帐户库不能去非用字
                // return:
                //		-1	出错
                //		0	成功
                int nRet = db.SearchByUnion(searchItem,
                    null,       //用于中断 , deleget
                    resultSet,
                    0,
                    out strError,
                    out strWarning);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

    } // end of class DatabaseCollection


    //*****************************************************

    // string类型的ArrayList排序编的IComparer接口
    public class ComparerClass : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            if (!(x is String))
                throw new Exception("object x is not a String");
            if (!(y is String))
                throw new Exception("object y is not a String");

            string strText1 = (string)x;
            string strText2 = (string)y;

            return String.Compare(strText1, strText2, true);
        }
    }


    // 检查通讯是否连接着的delegate
    public delegate bool Delegate_isConnected();

    #region 专门用于检索的类
    public class DatabaseCommandTask
    {
        public SqlCommand m_command = null;
        public AutoResetEvent m_event = new AutoResetEvent(false);

        public bool bError = false;
        public string ErrorString = "";
        //供外部使用
        public SqlDataReader DataReader = null;

        public DatabaseCommandTask(SqlCommand command)
        {
            m_command = command;
        }
        public void Cancel()
        {
            m_command.Cancel();
        }
        // 主函数
        public void ThreadMain()
        {
            try
            {
                DataReader = m_command.ExecuteReader();
            }
            catch (SqlException sqlEx)
            {
                this.bError = true;
                if (sqlEx.Errors is SqlErrorCollection)
                    this.ErrorString = "数据库尚未初始化。";
                else
                    this.ErrorString = "检索线程:" + sqlEx.Message;
            }
            catch (Exception ex)
            {
                this.bError = true;
                this.ErrorString = "检索线程:" + ex.Message;
            }
			finally  // 一定要返回信号
            {
                m_event.Set();
            }
        }
    }
    #endregion

    // 资源项信息
    // 当时放在DigitalPlatform.rms.Service里，后来要在Database.xml里使用，所以移动到这儿
    public class ResInfoItem
    {
        public int Type;	// 类型,0 库，1 途径,4 cfgs,5 file
        public string Name;	// 库名或途径名
        public bool HasChildren = true;  //是否有儿子
        public int Style = 0;   // 0x01:帐户库  // 原名Style

        public string TypeString = "";  // 新增
        public string[] Names;    // 新增 所有语言下的名字。每个元素的格式 语言代码:内容
    }
}

