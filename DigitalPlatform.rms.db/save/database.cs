//#define DEBUG_LOCK

using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.ResultSet;
using DigitalPlatform.Range;

namespace DigitalPlatform.rms
{
    // 数据库基类
    public class Database
    {
        // 数据库锁
        protected ReaderWriterLock m_lock = new ReaderWriterLock();            // 定义库锁m_lock

        // 尾号锁,在GetNewID() 和 SetIf
        protected ReaderWriterLock m_TailNolock = new ReaderWriterLock();

        // 记录锁
        internal RecordLockCollection m_recordLockColl = new RecordLockCollection();

        // 锁超时的时间
        internal int m_nTimeOut = 5 * 1000; //5秒 

        internal DatabaseCollection container; // 容器

        // 数据库根节点
        internal XmlNode m_selfNode = null;

        // 数据库属性节点
        public XmlNode m_propertyNode = null;

        //纯净数据库ID,前方不带@
        public string PureID = "";

        internal int KeySize = 0;

        public int m_nTimestampSeed = 0;

        private KeysCfg m_keysCfg = null;
        private BrowseCfg m_browseCfg = null;
        private bool m_bHasBrowse = true;

        internal Database(DatabaseCollection container)
        {
            this.container = container;
        }

        // 初始化数据库象
        // parameters:
        //      node    数据库配置节点<database>
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        internal virtual int Initial(XmlNode node,
            out string strError)
        {
            strError = "";
            return 0;
        }


        // 检查数据库尾号
        // parameters:
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：安全的
        public int CheckTailNo(out string strError)
        {
            strError = "";

            string strRealTailNo = "";
            int nRet = 0;

            //********对数据库加读锁**************
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("CheckTailNo()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                // return:
                //		-1  出错
                //      0   未找到
                //      1   找到
                nRet = this.GetRecordID("-1",
                    "prev",
                    out strRealTailNo,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            catch
            {
                // ???有可能还未初始化数据库
            }
            finally
            {
                //***********对数据库解读锁***************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("CheckTailNo()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }

            //this.container.WriteErrorLog("走到'" + this.GetCaption("zh-cn") + "'数据库的CheckTailNo()函数里，查到最大记录号为'" + strRealTailNo + "'。");

            if (nRet == 1)
            {
                //调SetIfGreaten()函数，如果本记录号大于尾号,自动推动尾号为最大
                //这种情况发生在我们给的记录号超过尾号时
                bool bPushTailNo = false;
                this.SetIfGreaten(Convert.ToInt32(strRealTailNo),
                    false, // isExistReaderLock
                    out bPushTailNo);
            }

            return 0;
        }
        // 根据strStyle风格,得到相应的记录号
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
        internal virtual int GetRecordID(string strCurrentRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";
            return 0;
        }

        // 得到检索点内存对象
        // 当返回0时，keysCfg可能为null
        public int GetKeysCfg(out KeysCfg keysCfg,
            out string strError)
        {
            strError = "";
            keysCfg = null;

            // 已存在时
            if (this.m_keysCfg != null)
            {
                keysCfg = this.m_keysCfg;
                return 0;
            }

            int nRet = 0;

            string strKeysFileName = "";
     
            string strDbName = this.GetCaption("zh");
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/keys",
                out strKeysFileName,
                out strError);

            // 未定义keys对象，按正常情况处理
            if (nRet == -2)
                return 0;

            // keys文件在本地不存在，按正常情况处理
            if (nRet == -4)
                return 0;

            if (nRet != 0)
                return -1;


            this.m_keysCfg = new KeysCfg();
            nRet = this.m_keysCfg.Initial(strKeysFileName,
                this.container.BinDir,
                out strError);
            if (nRet == -1)
            {
                this.m_keysCfg = null;
                return -1;
            }

            keysCfg = this.m_keysCfg;
            return 0;
        }

        // 得到浏览格式内存对象
        // 当return 0时，可能browseCfg为null
        public int GetBrowseCfg(out BrowseCfg browseCfg,
            out string strError)
        {
            strError = "";
            browseCfg = null;

            if (this.m_bHasBrowse == false)
                return 0;

            // 已存在时
            if (this.m_browseCfg != null)
            {
                browseCfg = this.m_browseCfg;
                return 0;
            }

            string strBrowseFileName = "";
            string strDbName = this.GetCaption("zh");
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            int nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/browse",
                out strBrowseFileName,
                out strError);
            if (nRet == -2 || nRet == -4)
            {
                this.m_bHasBrowse = false;
                return 0;
            }
            else
            {
                this.m_bHasBrowse = true;
            }

            if (nRet != 0)
            {
                return -1;
            }

            this.m_browseCfg = new BrowseCfg();
            nRet = this.m_browseCfg.Initial(strBrowseFileName,
                this.container.BinDir,
                out strError);
            if (nRet == -1)
            {
                this.m_browseCfg = null;
                return -1;
            }

            browseCfg = this.m_browseCfg;
            return 0;
        }

        // 时间戳种子
        public long GetTimestampSeed()
        {
            return this.m_nTimestampSeed++;
        }

        // 得到数据库ID，注意前面带"@"
        // return:
        //		数据库ID,格式为:@ID
        // 线: 不安全
        public string FullID
        {
            get
            {
                return "@" + this.PureID;
            }
        }

        // 得到数据库的所有逻辑名，放到一个字符串数组里
        public LogicNameItem[] GetLogicNames()
        {
            ArrayList aLogicName = new ArrayList();
            XmlNodeList captionList = this.m_propertyNode.SelectNodes("logicname/caption");
            for (int i = 0; i < captionList.Count; i++)
            {
                XmlNode captionNode = captionList[i];
                string strLang = DomUtil.GetAttr(captionNode, "lang");
                string strValue = DomUtil.GetNodeText(captionNode);

                // 有可以未定义语言，或未定义值，该怎么处理？？？
                LogicNameItem item = new LogicNameItem();
                item.Lang = strLang;
                item.Value = strValue;
                aLogicName.Add(item);
            }

            LogicNameItem[] logicNames = new LogicNameItem[aLogicName.Count];

            for (int i = 0; i < aLogicName.Count; i++)
            {
                LogicNameItem item = (LogicNameItem)aLogicName[i];
                logicNames[i] = item;
            }
            return logicNames;
        }

        // 得到某语言的数据库名
        // parameters:
        //      strLang 如果==null，表示使用数据库对象实际定义的第一种语言
        public string GetCaption(string strLang)
        {
            XmlNode nodeCaption = null;
            string strCaption = "";

            if (String.IsNullOrEmpty(strLang) == true)
                goto END1;

            strLang = strLang.Trim();
            if (strLang == "")
                goto END1;

            // 1.按语言版本精确找
            nodeCaption = this.m_propertyNode.SelectSingleNode("logicname/caption[@lang='" + strLang + "']");
            if (nodeCaption != null)
            {
                strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                if (String.IsNullOrEmpty(strCaption) == false)
                    return strCaption;
            }

            // 将语言版本截成两字符找
            if (strLang.Length >= 2)
            {
                string strShortLang = strLang.Substring(0, 2);//

                // 2. 精确找2字符的
                nodeCaption = this.m_propertyNode.SelectSingleNode("logicname/caption[@lang='" + strShortLang + "']");
                if (nodeCaption != null)
                {
                    strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                    if (String.IsNullOrEmpty(strCaption) == false)
                        return strCaption;
                }

                // 3. 找只是前两个字符相同的
                // xpath式子经过了验证。xpath下标习惯从1开始
                nodeCaption = this.m_propertyNode.SelectSingleNode("logicname/caption[(substring(@lang,1,2)='" + strShortLang + "')]");
                if (nodeCaption != null)
                {
                    strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                    if (String.IsNullOrEmpty(strCaption) == false)
                        return strCaption;
                }

            }

        END1:

            // 4.最后找排在第一位的caption
            nodeCaption = this.m_propertyNode.SelectSingleNode("logicname/caption");
            if (nodeCaption != null)
                strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
            if (strCaption != "")
                return strCaption;


            // 5.最后一个语言版本信息都没有时，返回数据库的id
            return this.FullID;

        }

        // 根据语言，得到数据的标签名
        // parameter:
        //		strLang 语言版本
        // return:
        //		找到，返回字符串
        //		没找到,会返回空字符串""
        // 线: 安全的
        public string GetCaptionSafety(string strLang)
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCaptionSafety(strLang)，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                return this.GetCaption(strLang);

            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCaptionSafety(strLang)，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }


        // 得到全部语言的caption。每个元素内的格式 语言代码:内容
        public List<string> GetAllLangCaption()
        {
            List<string> result = new List<string>();

            XmlNodeList listCaption =
                this.m_propertyNode.SelectNodes("logicname/caption");
            foreach (XmlNode nodeCaption in listCaption)
            {
                string strLang = DomUtil.GetAttr(nodeCaption, "lang");
                string strText = DomUtil.GetNodeText(nodeCaption).Trim();

                result.Add(strLang + ":" + strText);
            }

            return result;
        }

        // 线: 安全的
        public List<string> GetAllLangCaptionSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK		
			this.container.WriteDebugInfo("GetAllLangCaptionSafety()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                return GetAllLangCaption();
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetAllLangCaptionSafety()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 得到全部的caption，多个值之间用分号分隔
        public string GetAllCaption()
        {
            string strResult = "";
            XmlNodeList listCaption =
                this.m_propertyNode.SelectNodes("logicname/caption");
            foreach (XmlNode nodeCaption in listCaption)
            {
                if (strResult != "")
                    strResult += ",";
                strResult += DomUtil.GetNodeText(nodeCaption).Trim();
            }
            return strResult;
        }

        // 得到所有caption的值,以逗号分隔
        // 线: 安全的
        public string GetCaptionsSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK		
			this.container.WriteDebugInfo("GetCaptionSafety()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                return GetAllCaption();
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCaptionSafety()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 得到所有caption的值及ID,以逗号分隔
        // 线: 安全的
        public string GetAllNameSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetAllNameSafety()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                string strAllName = GetAllCaption();
                if (strAllName != "")
                    strAllName += ",";
                strAllName += this.FullID;
                return strAllName;
            }
            finally
            {
                m_lock.ReleaseReaderLock();
                //*****************对数据库解读锁********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetAllNameSafety()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 返回数据库dbo
        // return:
        //		string类型，dbo用户名
        // 线: 不安全的
        internal string GetDbo()
        {
            string strDboValue = "";
            if (this.m_selfNode != null)
                strDboValue = DomUtil.GetAttr(this.m_selfNode, "dbo").Trim();
            return strDboValue;
        }

        // 线: 安全的  加锁原因:从数据库配置根节点读数据
        public string DboSafety
        {
            get
            {
                //***********对数据库加读锁******GetDbo不会抛异，所以不用加try,catch
                m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
                try
                {
                    return GetDbo();
                }
                finally
                {
                    //**********对数据库解读锁************
                    m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
					this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");

#endif
                }
            }
        }

        // 私有GetType方法: 返回数据库类型
        // return:
        //		string类型，数据库类型
        // 线: 不安全的
        internal string GetDbType()
        {
            string strType = "";
            if (this.m_selfNode != null)
                strType = DomUtil.GetAttr(this.m_selfNode, "type").Trim();
            return strType;
        }



        // 线: 安全的  加锁原因:从数据库配置根节点读数据
        public string TypeSafety
        {
            get
            {
                //***********对数据库加读锁******GetDbType不会抛异，所以不用加try,catch
                m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
                try
                {
                    return GetDbType();
                }
                finally
                {
                    //**********对数据库解读锁************
                    m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
					this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");

#endif
                }
            }
        }

        // 得到种子值
        // return
        //      记录尾号
        // 线: 不安全
        private int GetTailNo()
        {
            XmlNode nodeSeed =
                this.m_propertyNode.SelectSingleNode("seed");
            if (nodeSeed == null)
            {
                throw new Exception("服务器配置文件错误,未找到<seed>元素。");
            }

            return System.Convert.ToInt32(DomUtil.GetNodeText(nodeSeed));
        }

        // set数据库的尾号
        // parameter:
        //		 nSeed  传入的尾号数
        // 线: 不安全
        protected void SetTailNo(int nSeed)  //设为protected是因为在初始化时被访问
        {
            XmlNode nodeSeed =
                this.m_propertyNode.SelectSingleNode("seed");

            DomUtil.SetNodeText(nodeSeed,
                Convert.ToString(nSeed));

            this.container.Changed = true;
        }

        // 得到记录的ID尾号（先加1再返回）,
        // 返回整数ID
        // 线: 安全
        // 加锁原因，加写锁，修改了nodeSeed内容，并始终保持增加，所以此时他处不能再读和写
        protected int GetNewTailNo()
        {
            //****************对数据库尾号加写锁***********
            this.m_TailNolock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetNewTailNo()，对'" + this.GetCaption("zh-cn") + "'数据库尾号加写锁。");
#endif
            try
            {
                int nTemp = GetTailNo();   //必须采用这种方法，不能直接用Seed++
                nTemp++;
                SetTailNo(nTemp);
                return nTemp; //GetTailNo();
            }
            finally
            {
                //***************对数据库尾号解写锁************
                this.m_TailNolock.ReleaseWriterLock();

#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetNewTailNo()，对'" + this.GetCaption("zh-cn") + "'数据库尾号解写锁。");
#endif
            }
        }

        // 如果用户手加输入的记录号大于的数据库的尾号，
        // 则要修改尾号，此时将读锁升级为写锁，
        // 修改完后再降级为读锁
        // parameter:
        //		nID         传入ID
        //		isExistReaderLock   是否已存在读锁
        //      bPushTailNo 是否推动尾号
        // return:
        //		返回当前记录号
        // 线: 安全的
        protected void SetIfGreaten(int nID,
            bool isExistReaderLock,
            out bool bPushTailNo)
        {
            bPushTailNo = false;

            if (isExistReaderLock == false)
            {
                //*********对数据库尾号加读锁*************
                this.m_TailNolock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-cn") + "'数据库尾号加读锁。");
#endif
            }
            try
            {
                int nSavedNo = GetTailNo();
                if (nID > nSavedNo)
                {
                    // 写日志
                    this.container.WriteErrorLog("发现数据库'" + this.GetCaption("zh-cn") + "'的实际尾号'" + Convert.ToString(nID) + "'大于保存的尾号'" + Convert.ToString(nSavedNo) + "'，推动尾号。");
                    bPushTailNo = true;

                    //*********上升读锁为写锁************
                    LockCookie lc = m_TailNolock.UpgradeToWriterLock(m_nTimeOut);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-cn") + "'数据库尾号读锁上升为写锁。");
#endif
                    try
                    {
                        SetTailNo(nID);
                    }
                    finally
                    {
                        //*************降写锁为读锁*************
                        m_TailNolock.DowngradeFromWriterLock(ref lc);
#if DEBUG_LOCK
						this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-cn") + "'数据库尾号写锁下降为读锁。");
#endif
                    }
                }
            }
            finally
            {
                if (isExistReaderLock == false)
                {
                    //*******对数据库尾号解读锁********
                    this.m_TailNolock.ReleaseReaderLock();
#if DEBUG_LOCK					
					this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-cn") + "'数据库尾号解读锁。");
#endif
                }
            }
        }

        // 将数据库多个表的标签信息，转换成TableInfo对象数组
        // 并检查检索途径中是否包含id，如果检索途径为空，表示按全部检索点进行检索(不包含id)
        // parameter:
        //		strTableNames   检索途径名称，之间用逗号分隔
        //      bHasID          out参数，返回途径中是否有id
        //      aTableInfo      out参数，返回TableInfo数组
        //      strError        out参数，返回出错信息
        // returns:
        //      -1  出错
        //      0   成功
        // 线: 不安全
        protected int TableNames2aTableInfo(string strTableNames,
            out bool bHasID,
            out List<TableInfo> aTableInfo,
            out string strError)
        {
            strError = "";
            bHasID = false;
            aTableInfo = new List<TableInfo>();

            strTableNames = strTableNames.Trim();

            int nRet = 0;

            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;


            //如果strTableList为空,则返回所有的表,但不包含通过id检索
            if (strTableNames == "")
            {
                if (keysCfg != null)
                {
                    nRet = keysCfg.GetTableInfosRemoveDup(out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                return 0;
            }

            string[] aTableName = strTableNames.Split(new Char[] { ',' });
            for (int i = 0; i < aTableName.Length; i++)
            {
                string strTableName = aTableName[i].Trim();

                if (strTableName == "")
                    continue;

                if (strTableName == "__id")
                {
                    bHasID = true;
                    continue;
                }

                if (keysCfg != null)
                {
                    TableInfo tableInfo = null;
                    nRet = keysCfg.GetTableInfo(strTableName,
                        out tableInfo,
                        out strError);
                    if (nRet == 0)
                        continue;
                    if (nRet != 1)
                        return -1;
                    aTableInfo.Add(tableInfo);
                }
            }
            return 0;
        }

        // 检索
        // parameter:
        //		searchItem  SearchItem对象
        //		isConnected IsConnection对象，用于判断通讯是否连接
        //		resultSet   存入检索结果的结果集
        //		nWarningLevel   处理警告级别 0：表示特别强烈，出现警告也当作出错；1：表示不强烈，不返回出错，继续执行
        //		strError    out参数，返回出错信息
        //		strWarning  out参数，返回警告信息
        // return:
        //		-1  出错
        //		>=0 命中记录数
        // 线: 安全的
        internal virtual int SearchByUnion(SearchItem searchItem,
            Delegate_isConnected isConnected,
            DpResultSet resultSet,
            int nWarningLevel,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";
            return 0;
        }

        // 初始化数据库，注意虚函数不能为private
        // parameter:
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的,在派生类里加锁
        public virtual int InitialPhysicalDatabase(out string strError)
        {
            strError = "";
            return 0;
        }

        // 删除数据库
        // return:
        //      -1  出错
        //      0   成功
        public virtual int Delete(out string strError)
        {
            strError = "";
            return 0;
        }

        // 按指定范围读Xml
        // parameter:
        //		strID				记录ID
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
        public virtual long GetXml(string strID,
            string strXPath,
            int nStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] outputTimestamp,
            bool bCheckAccount,
            out int nAdditionError,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            strOutputResPath = "";
            outputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            return 0;
        }


        // 按指定范围读对象
        // strRecordID  从属的记录ID
        // strObjectID  资源对象ID
        // 其它参数GetXml(),无strOutputResPath参数
        // 线: 安全的
        public virtual int GetObject(string strRecordID,
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
            strMetadata = "";
            outputTimestamp = null;
            strError = "";
            return 0;
        }

        // 得到xml数据
        // 线:安全的,供外部调
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      0   正确
        public int GetXmlDataSafety(string strRecordID,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            //********对数据库加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);

#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                strRecordID = DbPath.GetID10(strRecordID);
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID,
                    m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录加读锁。");
#endif

                try
                {
                    // return:
                    //      -1  出错
                    //      -4  记录不存在
                    //      0   正确
                    return this.GetXmlData(strRecordID,
                        out strXml,
                        out strError);
                }
                finally
                {
                    //************对记录解读锁*************
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-cn") + "/" + strID + "'记录解读锁。");
#endif
                }
            }

            finally
            {
                //*********对数据库解读锁****************
                m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 获取记录的xml内容
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      0   正确
        public virtual int GetXmlData(string strRecordID,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            return 0;
        }


        // 写xml数据
        // parameter:
        //		strRecordID     记录ID
        //		strRanges       写入的片断范围
        //		nTotalLength    数据总长度
        //		sourceBuffer    写入的数据字节数组
        //		strMetadata     元数据
        //		intputTimestamp 输入的时间戳
        //		outputTimestamp out参数，返回的时间戳,出错时,也返回时间戳
        //		strOutputID     out参数，返回的记录ID,追加记录时,有用
        //		strError        out参数，返回出错信息
        // return:
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录不存在
        //      -6  权限不够
        //		0   成功
        // 说明,总长度与源流如果 != null,就写到库里,片断合并后会把新片断记到库里,如果已满,则新片断为空字符串
        // 线: 安全的
        public virtual int WriteXml(User oUser,
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
            out string strOutputValue,
            bool bCheckAccount,
            out string strError)
        {
            outputTimestamp = null;
            strOutputID = "";
            strOutputValue = "";
            strError = "";

            return 0;
        }


        // parameters:
        //      strRecordID  记录ID
        //      strObjectID  对象ID
        //      其它同WriteXml,无strOutputID参数
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录或对象资源不存在
        //      -6  权限不够
        //		0   成功
        // 线: 安全的
        public virtual int WriteObject(User user,
            string strRecordID,
            string strObjectID,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] intputTimestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = intputTimestamp;
            strError = "";
            return 0;
        }

        // 删除记录
        // 函数里,当普通无法删除时,使用强制方法
        // parameter:
        //		strRecordID     记录ID
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp out参数，返回时间戳,当不正确时有效
        //		strError        out参数，返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全的,加写锁,在派生类里加锁
        public virtual int DeleteRecord(string strID,
            byte[] timestamp,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";
            return 0;
        }

        // 得到一条记录的浏览格式，一个字符串数组
        // parameter:
        //		strRecordID	一般自由位数的记录号，或10位数字的记录号
        //		cols	out参数，返回浏览格式数组
        // 当出错时,错误信息也保存在列里
        public void GetCols(string strRecordID,
            out string[] cols)
        {
            cols = null;

            //**********对数据库加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                BrowseCfg browseCfg = null;
                string strError = "";
                int nRet = this.GetBrowseCfg(out browseCfg,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (browseCfg == null)
                    return;

                // 加载数据文件为dom
                string strXml;
                strRecordID = DbPath.GetID10(strRecordID);
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录加读锁。");
#endif
                try
                {
                    // return:
                    //      -1  出错
                    //      -4  记录不存在
                    //      0   正确
                    nRet = this.GetXmlData(strRecordID,
                        out strXml,
                        out strError);
                    if (nRet <= -1)
                        goto ERROR1;
                }
                finally
                {
                    //*******************对记录解读锁************************
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-cn") + "/" + strRecordID + "'记录解读锁。");
#endif
                }

                XmlDocument domData = new XmlDocument();
                try
                {
                    domData.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "加载'" + this.GetCaption("zh-cn") + "'库的'" + strRecordID + "'记录到dom时出错,原因: " + ex.Message;
                    goto ERROR1;
                }

                nRet = browseCfg.BuildCols(domData,
                    out cols,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;

            ERROR1:
                cols = new string[1];
                cols[0] = strError;
            }
            finally
            {
                //****************对数据库解读锁**************
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK		
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }


        // 得到记录的时间戳
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      0   成功
        public virtual int GetTimestampFromDb(string strID,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";


            return 0;
        }


        // 假写xml数据，得到检索点集合
        // parameter:
        //		strXml	xml数据
        //		strID	记录ID,构造检索点用
        //		strLang	语言版本
        //		strStyle	风格，控制返回值
        //		keyColl	    out参数,返回检索点集合的
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        // 线: 安全的
        public int PretendWrite(string strXml,
            string strRecordID,
            string strLang,
            string strStyle,
            out KeyCollection keys,
            out string strError)
        {
            keys = null;
            strError = "";
            //**********对数据库加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK

			this.container.WriteDebugInfo("PretendWrite()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");

#endif
            try
            {
                //加载数据到DOM
                XmlDocument domData = new XmlDocument();
                domData.PreserveWhitespace = true; //设PreserveWhitespace为true
                try
                {
                    domData.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "PretendWrite()里，加载参数中的xml数据出错。原因:" + ex.Message;
                    return -1;
                }

                KeysCfg keysCfg = null;
                int nRet = this.GetKeysCfg(out keysCfg,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (keysCfg != null)
                {
                    //创建检索点
                    keys = new KeyCollection();
                    nRet = keysCfg.BuildKeys(domData,
                        strRecordID,
                        strLang,
                        strStyle,
                        this.KeySize,
                        out keys,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    //排序去重
                    keys.Sort();
                    keys.RemoveDup();
                }
                return 0;
            }
            finally
            {
                //****************对数据库解读锁**************
                this.m_lock.ReleaseReaderLock();

#if DEBUG_LOCK		
				this.container.WriteDebugInfo("PretendWrite()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        // 合并检索点
        // return:
        //      -1  出错
        //      0   成功
        public int MergeKeys(string strID,
            string strNewXml,
            string strOldXml,
            bool bOutputDom,
            out KeyCollection newKeys,
            out KeyCollection oldKeys,
            out XmlDocument newDom,
            out XmlDocument oldDom,
            out string strError)
        {
            newKeys = null;
            oldKeys = null;
            newDom = null;
            oldDom = null;
            strError = "";

            int nRet;

            KeysCfg keysCfg = null;

            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;


            // 根据新xml创建检索点
            newKeys = new KeyCollection();

            if (strNewXml != "" && strNewXml != null)
            {
                newDom = new XmlDocument();
                newDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                try
                {
                    newDom.LoadXml(strNewXml);
                }
                catch (Exception ex)
                {
                    strError = "加载新数据到dom时出错。" + ex.Message;
                    return -1;
                }

                if (keysCfg != null)
                {
                    nRet = keysCfg.BuildKeys(newDom,
                        strID,
                        "zh",//strLang,
                        "",//strStyle,
                        this.KeySize,
                        out newKeys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    newKeys.Sort();
                    newKeys.RemoveDup();
                }
            }

            oldKeys = new KeyCollection();

            if (strOldXml != null && strOldXml != "")
            {
                oldDom = new XmlDocument();
                oldDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                try
                {
                    oldDom.LoadXml(strOldXml);
                }
                catch (Exception ex)
                {
                    strError = "加载旧数据到dom时出错。" + ex.Message;
                    return -1;
                }

                if (keysCfg != null)
                {
                    nRet = keysCfg.BuildKeys(oldDom,
                        strID,
                        "zh",//strLang,
                        "",//strStyle,
                        this.KeySize,
                        out oldKeys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    oldKeys.Sort();
                    oldKeys.RemoveDup();
                }
            }

            // 新旧检索点碰
            KeyCollection dupKeys = new KeyCollection();
            dupKeys = KeyCollection.Merge(newKeys,
                oldKeys);

            if (bOutputDom == false)
            {
                newDom = null;
                oldDom = null;
            }
            return 0;
        }

        // 为数据库中的记录创建时间戳
        public string CreateTimestampForDb()
        {
            long lTicks = System.DateTime.UtcNow.Ticks;
            byte[] baTime = BitConverter.GetBytes(lTicks);

            byte[] baSeed = BitConverter.GetBytes(this.GetTimestampSeed());
            Array.Reverse(baSeed);

            byte[] baTimestamp = new byte[baTime.Length + baSeed.Length];
            Array.Copy(baTime,
                0,
                baTimestamp,
                0,
                baTime.Length);
            Array.Copy(baSeed,
                0,
                baTimestamp,
                baTime.Length,
                baSeed.Length);

            return ByteArray.GetHexTimeStampString(baTimestamp);
        }


        // 确保ID
        public string EnsureID(string strID,
            out bool bPushTailNo)
        {
            bPushTailNo = false;

            if (strID == "-1") // 追加记录,GetNewTailNo()是安全的
            {
                strID = Convert.ToString(GetNewTailNo());// 加得库写锁
                bPushTailNo = true;
                return DbPath.GetID10(strID);
            }


            //*******对数据库加读锁**********************
            m_TailNolock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("EnsureID()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                strID = DbPath.GetID10(strID);
                if (StringUtil.RegexCompare(@"\B[0123456789]+", strID) == false)
                {
                    throw (new Exception("记录号:'" + strID + "'不合法！"));
                }

                //调SetIfGreaten()函数，如果本记录号大于尾号,自动推动尾号为最大
                //这种情况发生在我们给的记录号超过尾号时
                SetIfGreaten(Convert.ToInt32(strID), true,
                    out bPushTailNo);
            }
            finally
            {
                //***********对数据库解读锁**********
                this.m_TailNolock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("EnsureID()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
            return strID;
        }


        // 得到一个库的信息
        // parameters:
        //      strStyle            获得那些输出参数? all表示全部 分别指定则是logicnames/type/sqldbname/keystext/browsetext
        //		logicNames	    逻辑库名数组
        //		strType	        数据库类型 以逗号分隔的字符串
        //		strSqlDbName	数据库物理名称，如果数据库为文为文件型数据库，则返回数据源目录的名称
        //		strKeyText	    检索点文件内容
        //		strBrowseText	非用字文件内容
        //		strError	    出错信息
        // return:
        //		-1	出错
        //		0	正常
        public int GetInfo(
            string strStyle,
            out LogicNameItem[] logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysText,
            out string strBrowseText,
            out string strError)
        {
            logicNames = null;
            strType = "";
            strSqlDbName = "";
            strKeysText = "";
            strBrowseText = "";
            strError = "";

            // 正规化strStyle的内容,便于后面处理
            if (String.IsNullOrEmpty(strStyle) == true
                || StringUtil.IsInList("all", strStyle) == true)
            {
                strStyle = "logicnames,type,sqldbname,keystext,browsetext";
            }

            //**********对数据库加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetInfo()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                logicNames = this.GetLogicNames();

                // 获得type
                if (StringUtil.IsInList("type", strStyle) == true)
                    strType = this.GetDbType();

                // 获得sqldbname
                if (StringUtil.IsInList("sqldbname", strStyle) == true)
                {
                    // 调入具体的函数，得到数据源信息，不包括实例名称
                    strSqlDbName = this.GetSourceName();

                    if (this.container.InstanceName != "" && strSqlDbName.Length > this.container.InstanceName.Length)
                    {
                        string strPart = strSqlDbName.Substring(0, this.container.InstanceName.Length);
                        if (strPart == this.container.InstanceName)
                        {
                            strSqlDbName = strSqlDbName.Substring(this.container.InstanceName.Length + 1); //rmsService_Guestbook
                        }
                    }
                }

                string strDbName = "";
                int nRet = 0;

                // 获得keystext
                if (StringUtil.IsInList("keystext", strStyle) == true)
                {
                    string strKeysFileName = "";
                    strDbName = this.GetCaption("zh");

                    // return:
                    //		-1	一般性错误，比如调用错误，参数不合法等
                    //		-2	没找到节点
                    //		-3	localname属性未定义或为值空
                    //		-4	localname在本地不存在
                    //		-5	存在多个节点
                    //		0	成功
                    nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/keys",
                        out strKeysFileName,
                        out strError);
                    if (nRet != 0)
                    {
                        if (nRet != -4)
                            return -1;
                    }

                    if (File.Exists(strKeysFileName) == true)
                    {
                        StreamReader sr = new StreamReader(strKeysFileName,
                            Encoding.UTF8);
                        strKeysText = sr.ReadToEnd();
                        sr.Close();
                    }

                    /*
                                    // keys文件
                                    KeysCfg keysCfg = null;
                                    int nRet = this.GetKeysCfg(out keysCfg,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;
                                    if (keysCfg != null)
                                    {
                                        if (keysCfg.dom != null)
                                        {
                                            TextWriter tw = new StringWriter();
                                            keysCfg.dom.Save(tw);
                                            tw.Close();
                                            strKeysText = tw.ToString();
                                        }
                                    }
                    */

                }

                // 获得browsetext
                if (StringUtil.IsInList("browsetext", strStyle) == true)
                {


                    string strBrowseFileName = "";
                    strDbName = this.GetCaption("zh");
                    // return:
                    //		-1	一般性错误，比如调用错误，参数不合法等
                    //		-2	没找到节点
                    //		-3	localname属性未定义或为值空
                    //		-4	localname在本地不存在
                    //		-5	存在多个节点
                    //		0	成功
                    nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/browse",
                        out strBrowseFileName,
                        out strError);
                    if (nRet != 0)
                    {
                        if (nRet != -4)
                            return -1;
                    }

                    if (File.Exists(strBrowseFileName) == true)
                    {
                        StreamReader sr = new StreamReader(strBrowseFileName,
                            Encoding.UTF8);
                        strBrowseText = sr.ReadToEnd();
                        sr.Close();
                    }
                    /*
                                    // browse文件
                                    BrowseCfg browseCfg = null;
                                    nRet = this.GetBrowseCfg(out browseCfg,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;
                                    if (browseCfg != null)
                                    {
                                        if (browseCfg.dom != null)
                                        {
                                            TextWriter tw = new StringWriter();
                                            browseCfg.dom.Save(tw);
                                            tw.Close();
                                            strBrowseText = tw.ToString();
                                        }
                                    }
                    */
                }

                return 0;
            }
            finally
            {
                //****************对数据库解读锁**************
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK		
				this.container.WriteDebugInfo("GetInfo()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }
        }

        public virtual string GetSourceName()
        {
            return "";
        }


        // 设置数据库的基本信息
        // parameters:
        //		logicNames	        LogicNameItem数组，用新的逻辑库名数组替换原来的逻辑库名数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout，目前无效，因为涉及到是文件库，还是sql库的问题
        //		strSqlDbName	    指定的新Sql数据库名称，目前无效，，如果数据库为文为文件型数据库，则返回数据源目录的名称
        //		strkeysDefault	    keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //		-1	出错
        //      -2  已存在同名的数据库
        //		0	成功
        public int SetInfo(LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysText,
            string strBrowseText,
            out string strError)
        {
            strError = "";

            //****************对数据库加写锁***********
            m_TailNolock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("SetInfo()，对'" + this.GetCaption("zh-cn") + "'数据库加写锁。");

#endif
            try
            {
                if (strKeysText == null)
                    strKeysText = "";
                if (strBrowseText == null)
                    strBrowseText = "";

                if (strKeysText != "")
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strKeysText);
                    }
                    catch (Exception ex)
                    {
                        strError = "加载keys配置文件内容到dom出错，原因:" + ex.Message;
                        return -1;
                    }
                }
                if (strBrowseText != "")
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strBrowseText);
                    }
                    catch (Exception ex)
                    {
                        strError = "加载browse配置文件内容到dom出错，原因:" + ex.Message;
                        return -1;
                    }
                }

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

                    if (this.container.IsExistLogicName(strLogicName, this) == true)
                    {
                        strError = "数据库中已存在'" + strLogicName + "'逻辑库名";
                        return -2;
                    }
                    strLogicNames += "<caption lang='" + strLang + "'>" + strLogicName + "</caption>";
                }

                // 修改LogicName，使用全部替换的方式
                XmlNode nodeLogicName = this.m_propertyNode.SelectSingleNode("logicname");
                nodeLogicName.InnerXml = strLogicNames;


                // 目前不支持修改strType,strSqlDbName

                string strKeysFileName = "";//this.GetFixedCfgFileName("keys");
                string strDbName = this.GetCaption("zh");
                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                int nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/keys",
                    out strKeysFileName,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet != -2 && nRet != -4)
                        return -1;
                    else if (nRet == -2)
                    {
                        // return:
                        //		-1	出错
                        //		0	成功
                        nRet = this.container.SetFileCfgItem(this.GetCaption("zh") + "/cfgs",
                            null,
                            "keys",
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // return:
                        //		-1	一般性错误，比如调用错误，参数不合法等
                        //		-2	没找到节点
                        //		-3	localname属性未定义或为值空
                        //		-4	localname在本地不存在
                        //		-5	存在多个节点
                        //		0	成功
                        nRet = this.container.GetFileCfgItemLacalPath(this.GetCaption("zh") + "/cfgs/keys",
                            out strKeysFileName,
                            out strError);
                        if (nRet != 0)
                        {
                            if (nRet != -4)
                                return -1;
                        }
                    }
                }

                if (File.Exists(strKeysFileName) == false)
                {
                    Stream s = File.Create(strKeysFileName);
                    s.Close();
                }

                nRet = DatabaseUtil.CreateXmlFile(strKeysFileName,
                    strKeysText,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把缓冲清空
                this.m_keysCfg = null;



                string strBrowseFileName = "";

                // return:
                //		-1	一般性错误，比如调用错误，参数不合法等
                //		-2	没找到节点
                //		-3	localname属性未定义或为值空
                //		-4	localname在本地不存在
                //		-5	存在多个节点
                //		0	成功
                nRet = this.container.GetFileCfgItemLacalPath(strDbName + "/cfgs/browse",
                    out strBrowseFileName,
                    out strError);
                if (nRet != 0)
                {
                    if (nRet != -2 && nRet != -4)
                        return -1;
                    else if (nRet == -2)
                    {
                        // return:
                        //		-1	出错
                        //		0	成功
                        nRet = this.container.SetFileCfgItem(this.GetCaption("zh") + "/cfgs",
                            null,
                            "browse",
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // return:
                        //		-1	一般性错误，比如调用错误，参数不合法等
                        //		-2	没找到节点
                        //		-3	localname属性未定义或为值空
                        //		-4	localname在本地不存在
                        //		-5	存在多个节点
                        //		0	成功
                        nRet = this.container.GetFileCfgItemLacalPath(this.GetCaption("zh") + "/cfgs/browse",
                            out strBrowseFileName,
                            out strError);
                        if (nRet != 0)
                        {
                            if (nRet != -4)
                                return -1;
                        }
                    }
                }

                if (File.Exists(strBrowseFileName) == false)
                {
                    Stream s = File.Create(strBrowseFileName);
                    s.Close();
                }

                nRet = DatabaseUtil.CreateXmlFile(strBrowseFileName,
                    strBrowseText,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把缓冲清空
                this.m_browseCfg = null;
                this.m_bHasBrowse = true; // 缺省值


                return 0;
            }
            finally
            {
                //***************对数据库解写锁************
                m_TailNolock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("SetInfo()，对'" + this.GetCaption("zh-cn") + "'数据库解写锁。");
#endif
            }
        }

        // 得到数据库可以显示的下级
        // parameters:
        //		oUser	帐户对象
        //		db	数据库对象
        //		strLang	语言版本
        //		aItem	out参数，返回数据库可以显示的下级
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int GetDirableChildren(User user,
            string strLang,
            string strStyle,
            out ArrayList aItem,
            out string strError)
        {
            aItem = new ArrayList();
            strError = "";

            //**********对数据库加读锁**************
            this.m_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-cn") + "'数据库加读锁。");
#endif
            try
            {
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                // 1.配置事项

                foreach (XmlNode child in this.m_selfNode.ChildNodes)
                {
                    string strElementName = child.Name;
                    if (String.Compare(strElementName, "dir", true) != 0
                        && String.Compare(strElementName, "file", true) != 0)
                    {
                        continue;
                    }


                    string strChildName = DomUtil.GetAttr(child, "name");
                    if (strChildName == "")
                        continue;
                    string strCfgPath = this.GetCaption("zh-cn") + "/" + strChildName;
                    string strExistRights;
                    bool bHasRight = false;

                    ResInfoItem resInfoItem = new ResInfoItem();
                    resInfoItem.Name = strChildName;
                    if (child.Name == "dir")
                    {
                        bHasRight = user.HasRights(strCfgPath,
                            ResType.Directory,
                            "list",
                            out strExistRights);
                        if (bHasRight == false)
                            continue;

                        resInfoItem.HasChildren = true;
                        resInfoItem.Type = 4;   // 目录

                        resInfoItem.TypeString = DomUtil.GetAttr(child, "type");    // xietao 2006/6/5
                    }
                    else
                    {
                        bHasRight = user.HasRights(strCfgPath,
                            ResType.File,
                            "list",
                            out strExistRights);
                        if (bHasRight == false)
                            continue;
                        resInfoItem.HasChildren = false;
                        resInfoItem.Type = 5;   // 文件

                        resInfoItem.TypeString = DomUtil.GetAttr(child, "type"); // xietao 2006/6/5 add

                    }
                    aItem.Add(resInfoItem);
                }


                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                // 2.检索途径

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

                    // 对于检索途径，全部都有权限
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];
                        Debug.Assert(tableInfo.Dup == false, "不可能再有重复的了。");

                        ResInfoItem resInfoItem = new ResInfoItem();
                        resInfoItem.Type = 1;
                        resInfoItem.Name = tableInfo.GetCaption(strLang);
                        resInfoItem.HasChildren = false;

                        resInfoItem.TypeString = tableInfo.TypeString;  // xietao 2006/6/5 add

                        // 如果需要, 列出所有语言下的名字
                        if (StringUtil.IsInList("alllang", strStyle) == true)
                        {
                            List<string> results = tableInfo.GetAllLangCaption();
                            string[] names = new string[results.Count];
                            results.CopyTo(names);
                            resInfoItem.Names = names;
                        }

                        aItem.Add(resInfoItem);
                    }
                }

                // 加__id
                ResInfoItem resInfoItemID = new ResInfoItem();
                resInfoItemID.Type = 1;
                resInfoItemID.Name = "__id";
                resInfoItemID.HasChildren = false;
                resInfoItemID.TypeString = "recid";

                aItem.Add(resInfoItemID);

                return 0;
            }
            finally
            {
                //***************对数据库解读锁************
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-cn") + "'数据库解读锁。");
#endif
            }

        }


        // 写配置文件
        // parameters:
        //      strCfgItemPath  全路径，带库名
        // return:
        //		-1  一般性错误
        //      -2  时间戳不匹配
        //		0	成功
        // 线程，对库集合是不安全的
        internal int WriteFileForCfgItem(string strCfgItemPath,
            string strFilePath,
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
            //**********对数据库加写锁**************
            this.m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-cn") + "'数据库加写锁。");
#endif
            try
            {
                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		0	成功
                int nRet = this.container.WriteFileForCfgItem(strFilePath,
                    strRanges,
                    lTotalLength,
                    baSource,
                    streamSource,
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    out baOutputTimestamp,
                    out strError);
                if (nRet <= -1)
                    return nRet;

                int nPosition = strCfgItemPath.IndexOf("/");
                if (nPosition == -1)
                {
                    strError = "'" + strCfgItemPath + "'路径不包括'/'，不合法。";
                    return -1;
                }
                if (nPosition == strCfgItemPath.Length - 1)
                {
                    strError = "'" + strCfgItemPath + "'路径最后是'/'，不合法。";
                    return -1;
                }
                string strPathWithoutDbName = strCfgItemPath.Substring(nPosition + 1);
                // 如果为keys对象，则把该库的KeysCfg中的dom清空
                if (strPathWithoutDbName == "cfgs/keys")
                {
                    this.m_keysCfg = null;
                }

                // 如果为browse对象
                if (strPathWithoutDbName == "cfgs/browse")
                {
                    this.m_browseCfg = null;
                    this.m_bHasBrowse = true; // 缺省值
                }

                return 0;
            }
            finally
            {
                //**********对数据库解写锁**************
                this.m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-cn") + "'数据库解写锁。");
#endif
            }

        }


        // 规范化记录号
        // parameters:
        //      strInputRecordID    输入的记录号，只能为 '-1','?'或者纯数字(且小于等于10位)
        //      strOututRecordID    out参数，返回规范化后的记录号
        // return:
        //      -1  出错
        //      0   成功
        public int CanonicalizeRecordID(string strInputRecordID,
            out string strOutputRecordID,
            out string strError)
        {
            strOutputRecordID = "";
            strError = "";

            if (strInputRecordID.Length > 10)
            {
                strError = "记录号不合法，长度超过10位。";
                return -1;
            }

            if (strInputRecordID == "?" || strInputRecordID == "-1")
            {
                strOutputRecordID = "-1";
                return 0;
            }

            if (StringUtil.IsPureNumber(strInputRecordID) == false)
            {
                strError = "记录号'" + strInputRecordID + "'不合法。";
                return -1;
            }

            strOutputRecordID = DbPath.GetID10(strInputRecordID);
            return 0;
        }
    }
}