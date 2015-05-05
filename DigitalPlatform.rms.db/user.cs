//#define DEBUG_LOCK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.rms;
using DigitalPlatform.Text;
using DigitalPlatform.Text.SectionPropertyString;
using DigitalPlatform.Xml;
using DigitalPlatform.ResultSet;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{

    // 用户
    public class User
    {
        // 本用户所拥有的数据库名列表
        public List<string> aOwnerDbName = null;

        public void AddOwnerDbName(string strDbName)
        {
            m_lock.AcquireWriterLock(m_nTimeOut);

            try
            {
                if (aOwnerDbName == null)
                    aOwnerDbName = new List<string>();

                aOwnerDbName.Add(strDbName);
            }
            finally
            {
                m_lock.ReleaseWriterLock();
            }
        }

        public void RemoveOwerDbName(string strDbName)
        {
            m_lock.AcquireWriterLock(m_nTimeOut);

            try
            {
                if (aOwnerDbName == null)
                    return;
                aOwnerDbName.Remove(strDbName);
            }
            finally
            {
                m_lock.ReleaseWriterLock();
            }

        }

        public UserCollection container = null;

        private string m_strRecPath = "";   // 全路径格式: 库名/记录号

        public string RecPath
        {
            get
            {
                m_lock.AcquireReaderLock(m_nTimeOut);
                try
                {
                    return m_strRecPath;
                }
                finally
                {
                    m_lock.ReleaseReaderLock();
                }
            }
            set
            {
                m_lock.AcquireWriterLock(m_nTimeOut);
                try
                {
                    this.m_strRecPath = value;
                }
                finally
                {
                    m_lock.ReleaseWriterLock();
                }
            }
        }

        private XmlDocument m_dom = new XmlDocument();
        // private Database m_db = null;

        private string m_strName = "";

        public string Name
        {
            get
            {
                m_lock.AcquireReaderLock(m_nTimeOut);
                try
                {
                    return m_strName;
                }
                finally
                {
                    m_lock.ReleaseReaderLock();
                }
            }
            set
            {
                m_lock.AcquireWriterLock(m_nTimeOut);
                try
                {
                    this.m_strName = value;
                }
                finally
                {
                    m_lock.ReleaseWriterLock();
                }
            }
        }

        private string m_strSHA1Password = "";   // 名称是为了强调为SHA1形态

        public string SHA1Password
        {
            get
            {
                m_lock.AcquireReaderLock(m_nTimeOut);
                try
                {
                    return m_strSHA1Password;
                }
                finally
                {
                    m_lock.ReleaseReaderLock();
                }
            }
            set
            {
                m_lock.AcquireWriterLock(m_nTimeOut);
                try
                {
                    this.m_strSHA1Password = value;
                }
                finally
                {
                    m_lock.ReleaseWriterLock();
                }
            }
        }

#if NO
        // 没有锁定保护的
        public string InternalSHA1Password
        {
            get
            {
                return m_strSHA1Password;
            }
            set
            {
                this.m_strSHA1Password = value;
            }
        }
#endif

        // public int Count = 0;

        private XmlNode m_nodeServer = null;

        private CfgRights cfgRights = null;

        // 目前用户对象的锁主要用在修改使用数量方面,
        // 考虑改成Interlocked.Increment和Interlocked.Decrement
        //public ReaderWriterLock m_lock = new ReaderWriterLock();
        //public int m_nTimeOut = 5000;
        internal int m_nUseCount = 0;

        internal DateTime m_timeLastUse = DateTime.Now;

        bool m_bChanged = false;

        private MyReaderWriterLock m_lock = new MyReaderWriterLock();
        private static int m_nTimeOut = 5000;

        // 更新一下最后使用时间
        public void Activate()
        {
             m_timeLastUse = DateTime.Now;
        }

        public bool Changed
        {
            get
            {
                m_lock.AcquireReaderLock(m_nTimeOut);
                try
                {
                    return m_bChanged;
                }
                finally
                {
                    m_lock.ReleaseReaderLock();
                }
            }
            set
            {
                m_lock.AcquireWriterLock(m_nTimeOut);
                try
                {
                    m_bChanged = value;
                }
                finally
                {
                    m_lock.ReleaseWriterLock();
                }
            }
        }

        /*
        public int UseCount
        {
            get
            {
                return this.m_nUseCount;
            }
        }
         */

        // 初始化用户对象
        // 线程不安全。因为被调用时尚未进入集合，也没有必要线程安全
        // parameters:
        //      dom         用户记录dom
        //      strResPath  记录路径 全路径 库名/记录号
        //      db          所从属的数据库
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        internal int Initial(string strRecPath,
            XmlDocument dom,
            Database db,
            DatabaseCollection dbs,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.RecPath = strRecPath;
            this.m_dom = dom;
            // this.m_db = db;

            XmlNode root = this.m_dom.DocumentElement;
            XmlNode nodeName = root.SelectSingleNode("name");
            if (nodeName != null)
                this.Name = nodeName.InnerText.Trim(); // 2012/2/16

            // 用户所拥有的数据库
            this.aOwnerDbName = null;

            if (String.IsNullOrEmpty(this.Name) == false)
            {
                List<string> aOwnerDbName = null;

                nRet = dbs.GetOwnerDbNames(
                    false, // TODO: 需要检查，是否需要加锁
                    this.Name,
                    out aOwnerDbName,
                    out strError);
                if (nRet == -1)
                    return -1;

                this.aOwnerDbName = aOwnerDbName;
            }




            XmlNode nodePassword = root.SelectSingleNode("password");
            if (nodePassword != null)
                SHA1Password = nodePassword.InnerText.Trim(); // 2012/2/16

            XmlNode nodeRightsItem = root.SelectSingleNode("rightsItem");
            if (nodeRightsItem != null)
            {
                strError = "帐户记录为旧版本，根元素下已经不支持<rightsItem>元素。";
                return -1;
            }

            // 没有<server>元素是否按出错处理
            this.m_nodeServer = root.SelectSingleNode("server");
            if (this.m_nodeServer == null)
            {
                strError = "帐户记录未定义<server>元素。";
                return -1;
            }

            this.cfgRights = new CfgRights();
            // return:
            //      -1  出错
            //      0   成功
            nRet = this.cfgRights.Initial(this.m_nodeServer,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 得到片断的定义信息
        // parameters:
        //      strRights   总权限
        //      strCategory 种类
        // return:
        //      找到种定义的权限，不带种类名称
        private string GetSectionRights(string strRights,
            string strCategory)
        {
            DigitalPlatform.Text.SectionPropertyString.PropertyCollection propertyColl =
                new DigitalPlatform.Text.SectionPropertyString.PropertyCollection("this",
                strRights,
                DelimiterFormat.Semicolon);
            Section section = propertyColl[strCategory];
            if (section == null)
                return "";

            return section.Value;
        }


        // 检索该帐户是否对指定的配置事项有指定的权限
        // 线程安全
        // parameters:
        //		strPath	要关心其权限的对象的资源路径
        //      resType 资源类型
        //      strOneRight 待确定的权限
        //		strExistRights	返回该对象已经存在的的权限列表
        // return:
        //		true	有
        //		false	无
        public bool HasRights(string strPath,
            ResType resType,
            string strQueryOneRight,
            out string strExistRights)
        {
            strExistRights = "";

            m_lock.AcquireReaderLock(m_nTimeOut);

            try
            {
                ResultType resultType = new ResultType();
                string strError = "";
                int nRet = this.cfgRights.CheckRights(strPath,
                    this.aOwnerDbName,
                    this.m_strName,
                    resType,
                    strQueryOneRight,
                    out strExistRights,
                    out resultType,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception("CheckRights()出错，原因：" + strError);
                }

                if (resultType == ResultType.Plus)
                    return true;

                return false;
            }
            finally
            {
                m_lock.ReleaseReaderLock();
            }
        }

        // 缺省认为可以修改自己的密码
        private bool CheckChangePasswordRights()
        {
            if (this.m_dom != null)
            {
                XmlNode nodePassword = this.m_dom.DocumentElement.SelectSingleNode("password");
                string strStyle = DomUtil.GetAttr(nodePassword, "style");
                if (StringUtil.IsInList("changepassworddenied", strStyle, true) == true)
                    return false;
            }
            return true;
        }

        // 修改自己的密码
        // 线程安全
        // parameters:
        //      strNewPassword   明码
        // return:
        //      -1  出错
        //      -4  记录不存在
        //		0   成功
        public int ChangePassword(
            string strNewPassword,
            out string strError)
        {
            strError = "";

            m_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
                // 检索是否有修改自己密码的权限
                bool bHasChangePasswordRights = false;
                bHasChangePasswordRights = this.CheckChangePasswordRights();
                if (bHasChangePasswordRights == false)
                {
                    strError = "您的用户名为 '" + this.Name + "'，没有修改自己密码的权限。";
                    return -6;
                }

                this.m_strSHA1Password = Cryptography.GetSHA1(strNewPassword);

                // 用最新的密码
                XmlNode root = this.m_dom.DocumentElement;
                XmlNode nodePassword = root.SelectSingleNode("password");
                nodePassword.InnerText = this.m_strSHA1Password;   // 2012/2/16

                // 立即保存到用户记录里
                // return:
                //		-1  出错
                //      -4  记录不存在
                //		0   成功
                int nRet = this.InternalSave(out strError);
                this.m_bChanged = false;
                return nRet;
            }
            finally
            {
                m_lock.ReleaseWriterLock();
            }
        }

        // 如果有修改则保存
        // 线程安全。InternalSave()中，保存数据库的操作自然是线程安全的
        // return:
        //		-1  出错
        //      -4  记录不存在
        //		0   成功
        public int SaveChanges(out string strError)
        {
            strError = "";
            if (this.Changed == false)
                return 0;
            // return:
            //		-1  出错
            //      -4  记录不存在
            //		0   成功
            int nRet = InternalSave(out strError);
            this.Changed = false;
            return nRet;
        }

        // 保存内存对象到数据库记录
        // 和m_bChanged状态无关
        // return:
        //		-1  出错
        //      -4  记录不存在
        //		0   成功
        private int InternalSave(out string strError)
        {
            strError = "";

            if (this.container == null)
                throw new Exception("User对象的container成员不能为null");

            if (String.IsNullOrEmpty(this.m_strRecPath) == true)
            {
                strError = "InternalSave失败，因为m_strRecPath为空";
                return -1;
            }

            Database db = this.container.Dbs.GetDatabaseFromRecPathSafety(this.m_strRecPath);
            if (db == null)
            {
                strError = "GetDatabaseFromRecPath()没有找到记录路径'"+this.m_strRecPath+"'对应的数据库对象";
                return -1;
            }

            DbPath path = new DbPath(this.m_strRecPath);

            // 将帐户记录的内容读到一个字节数组
            MemoryStream fs = new MemoryStream();
            this.m_dom.Save(fs);
            fs.Seek(0, SeekOrigin.Begin);
            byte[] baSource = new byte[fs.Length];
            fs.Read(baSource,
                0,
                baSource.Length);
            fs.Close();

            string strRange = "0-" + Convert.ToString(baSource.Length - 1);
            byte[] baInputTimestamp = null;
            byte[] baOutputTimestamp = null;
            string strOutputID = "";
            string strOutputValue = "";
            string strStyle = "ignorechecktimestamp";
            // return:
            //		-1  出错
            //		-2  时间戳不匹配 // 因为风格中有ignorechecktimestamp,所以此次调用不可能出现-2的情况
            //      -4  记录不存在
            //      -6  权限不够    // 此次调用不可能出现权限不够的情况
            //		0   成功
            // 因为传了user对象为null，所以不可能出现权限不够的情况
            int nRet = db.WriteXml(null, //oUser
                path.ID,
                null,
                strRange,
                baSource.Length,
                baSource,
                // null,
                "",  //metadata
                strStyle,
                baInputTimestamp,
                out baOutputTimestamp,
                out strOutputID,
                out strOutputValue,
                false,  //bCheckAccount
                out strError);

            Debug.Assert(nRet != -1 && nRet != -4, "不可能的情况。");

            return nRet;
        }


        // 增加一次使用数
        // 在login()时被调
        // 无需线程安全
        public void PlusOneUse()
        {
            // out string strError
            // strError = "";

            Interlocked.Increment(ref this.m_nUseCount);

            /*
            //*********对用户加写锁***********
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.m_dbColl.WriteDebugInfo("PlusOneUse()，对用户加写锁。");
#endif
            try
            {
                this.m_nUseCount++;
                return 0;
            }
            finally
            {
                //*********对用户解写锁*************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.m_dbColl.WriteDebugInfo("PlusOneUse()，对用户解写锁。");
#endif
            }
             */
        }

        // 减少一次使用数
        // 在Session失效或者Logout()时被调
        // 无需线程安全
        public void MinusOneUse()
        {   
            // out string strError
            // strError = "";

            Interlocked.Decrement(ref this.m_nUseCount);

            /*
            //*********对用户加写锁***********
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.m_dbColl.WriteDebugInfo("MinusOneUse()，对用户加写锁。");
#endif
            try
            {
                this.m_nUseCount--;
                return 0;
            }
            finally
            {
                //*********对用户解写锁*************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.m_dbColl.WriteDebugInfo("MinusOneUse()，对用户解写锁。");
#endif
            }
             */
        }
    }


    // test1
}


