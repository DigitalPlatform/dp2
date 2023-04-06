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
using System.Threading.Tasks;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.ResultSet;

namespace DigitalPlatform.rms
{
    // 数据库基类
    public class Database
    {
        // 快速导入的结束阶段任务
        internal List<Task<NormalResult>> _tasks = new List<Task<NormalResult>>();

        public RecordIDStorage RebuildIDs = null;

        // 数据库锁
        protected MyReaderWriterLock m_db_lock = new MyReaderWriterLock();            // 定义库锁m_lock

        // 记录锁
        internal RecordLockCollection m_recordLockColl = new RecordLockCollection();

        // 锁超时的时间
        internal int m_nTimeOut = 5 * 1000; //5秒 

        internal DatabaseCollection container; // 容器

        // 数据库根节点
        internal XmlNode m_selfNode = null;

        // 数据库属性节点
        XmlNode m_propertyNode = null;
        Hashtable m_captionTable = new Hashtable();
        public XmlNode PropertyNode
        {
            get
            {
                return this.m_propertyNode;
            }
            set
            {
                this.m_propertyNode = value;
                m_captionTable.Clear();
            }
        }

        //纯净数据库ID,前方不带@
        public string PureID = "";

        internal int KeySize = 0;

        public int m_nTimestampSeed = 0;

        private KeysCfg m_keysCfg = null;
        // private BrowseCfg m_browseCfg = null;
        // private bool m_bHasBrowse = true;
        Hashtable browse_table = new Hashtable();


        public bool InRebuildingKey = false;   // 是否处在重建检索点的状态

        public int FastAppendTaskCount = 0; // 启动 fast mode 的嵌套次数
        // public bool IsDelayWriteKey = false;    // 是否处在延迟写入keys的状态 2013/2/16

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

        // 关闭数据库对象
        internal virtual void Close()
        {
            DeleteRebuildIDs();
        }

        public void DeleteRebuildIDs()
        {
            // 2019/5/8
            if (this.RebuildIDs != null
                // && this.RebuildIDs.Count > 0
                )
            {
                this.RebuildIDs.Delete();
                this.RebuildIDs = null;
            }
        }

        // 将 Connection 的 Transaction Commit
        internal virtual void Commit()
        {
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

        // 升级数据库结构，如果必要
        internal virtual int UpdateStructure(out string strError)
        {
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
            nRet = this.container.GetFileCfgItemLocalPath(strDbName + "/cfgs/keys",
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
                this is SqlDatabase && this.container.SqlServerType == SqlServerType.Oracle ? "" : null,
                out strError);
            if (nRet == -1)
            {
                this.m_keysCfg = null;
                return -1;
            }

            keysCfg = this.m_keysCfg;
            return 0;
        }

        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public bool ClearBrowseCfgCache(string strBrowseName)
        {
            /*
            // 尝试从 cache 里面取得
            strBrowseName = strBrowseName.ToLower();
            var browseCfg = (BrowseCfg)this.browse_table[strBrowseName];
            if (browseCfg != null)
            {
                browseCfg.Clear();
                return true;
            }
            */
            if (this.browse_table.ContainsKey(strBrowseName))
            {
                this.browse_table.Remove(strBrowseName);
                return true;
            }
            return false;
        }

        // 得到浏览格式内存对象
        // parameters:
        //      strBrowseName   浏览文件的文件名或者全路径
        //                      例如 cfgs/browse 。因为已经是在特定的数据库中获得配置文件，所以无需指定库名部分
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetBrowseCfg(
            string strBrowseName,
            out BrowseCfg browseCfg,
            out string strError)
        {
            strError = "";
            browseCfg = null;

#if REMOVED
            strBrowseName = strBrowseName.ToLower();
            /*
            if (this.m_bHasBrowse == false)
                return 0;
             * */

            /*
            // 已存在时
            if (this.m_browseCfg != null)
            {
                browseCfg = this.m_browseCfg;
                return 0;
            }
             * */
            browseCfg = (BrowseCfg)this.browse_table[strBrowseName];
            if (browseCfg != null)
            {
                return 1;
            }

            /*
                        string strDbName = this.GetCaption("zh");

                        // strDbName + "/cfgs/browse"
             * */
            string strBrowsePath = this.GetCaption("zh") + "/" + strBrowseName;

            string strBrowseFileName = "";
            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            int nRet = this.container.GetFileCfgItemLocalPath(strBrowsePath,
                out strBrowseFileName,
                out strError);
            if (nRet == -2 || nRet == -4)
            {
                // this.m_bHasBrowse = false;
                return 0;
            }
            else
            {
                // this.m_bHasBrowse = true;
            }

            if (nRet != 0)
            {
                return -1;
            }
#endif
            // 先尝试从 cache 里面取得
            strBrowseName = strBrowseName.ToLower();
            browseCfg = (BrowseCfg)this.browse_table[strBrowseName];
            if (browseCfg != null)
            {
                return 1;
            }

            // 获得 browse 配置文件的物理文件名
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = FindBrowseFileName(
                strBrowseName,
                out string strBrowseFileName,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;
            if (nRet == 1)
            {
                Debug.Assert(string.IsNullOrEmpty(strBrowseFileName) == false);
            }

            // 新创建一个 BrowseCfg 对象
            browseCfg = new BrowseCfg();
            nRet = browseCfg.Initial(strBrowseFileName,
                this.container.BinDir,
                out strError);
            if (nRet == -1)
            {
                browseCfg = null;
                return -1;
            }

            // 加入缓存
            this.browse_table[strBrowseName] = browseCfg;
            return 1;
        }

        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int FindBrowseFileName(
            string strBrowseName,
            out string strBrowseFileName,
            out string strError)
        {
            strError = "";
            strBrowseFileName = "";

            strBrowseName = strBrowseName.ToLower();

            /*
            browseCfg = (BrowseCfg)this.browse_table[strBrowseName];
            if (browseCfg != null)
            {
                return 1;
            }
            */

            string strBrowsePath = this.GetCaption("zh") + "/" + strBrowseName;

            // return:
            //		-1	一般性错误，比如调用错误，参数不合法等
            //		-2	没找到节点
            //		-3	localname属性未定义或为值空
            //		-4	localname在本地不存在
            //		-5	存在多个节点
            //		0	成功
            int nRet = this.container.GetFileCfgItemLocalPath(strBrowsePath,
                out strBrowseFileName,
                out strError);
            if (nRet == -2 || nRet == -4)
            {
                return 0;
            }
            else
            {
            }

            if (nRet != 0)
            {
                return -1;
            }

            return 1;
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
            XmlNodeList captionList = this.PropertyNode.SelectNodes("logicname/caption");
            for (int i = 0; i < captionList.Count; i++)
            {
                XmlNode captionNode = captionList[i];
                string strLang = DomUtil.GetAttr(captionNode, "lang");
                string strValue = captionNode.InnerText.Trim(); // 2012/2/16

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

        // 有缓存的版本
        public string GetCaption(string strLang)
        {
            string strResult = (string)this.m_captionTable[strLang == null ? "<null>" : strLang];
            if (strResult != null)
                return strResult;

            strResult = GetCaptionInternal(strLang);
            this.m_captionTable[strLang == null ? "<null>" : strLang] = strResult;
            return strResult;
        }

        // 得到某语言的数据库名
        // parameters:
        //      strLang 如果==null，表示使用数据库对象实际定义的第一种语言
        string GetCaptionInternal(string strLang)
        {
            XmlNode nodeCaption = null;
            string strCaption = "";

            if (String.IsNullOrEmpty(strLang) == true)
                goto END1;

            strLang = strLang.Trim();
            if (String.IsNullOrEmpty(strLang) == true)
                goto END1;

            // 2021/12/29
            if (this.PropertyNode == null)
            {
                this.PropertyNode = this.m_selfNode.SelectSingleNode("property");
                if (this.PropertyNode == null)
                    return this.FullID;
            }

            // 1.按语言版本精确找
            nodeCaption = this.PropertyNode.SelectSingleNode("logicname/caption[@lang='" + strLang + "']");
            if (nodeCaption != null)
            {
                // strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                strCaption = nodeCaption.InnerText.Trim();  // 2012/2/15
                if (String.IsNullOrEmpty(strCaption) == false)
                    return strCaption;
            }

            // 将语言版本截成两字符找
            if (strLang.Length >= 2)
            {
                string strShortLang = strLang.Substring(0, 2);//

                // 2. 精确找2字符的
                nodeCaption = this.PropertyNode.SelectSingleNode("logicname/caption[@lang='" + strShortLang + "']");
                if (nodeCaption != null)
                {
                    // strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                    strCaption = nodeCaption.InnerText.Trim();  // 2012/2/15
                    if (String.IsNullOrEmpty(strCaption) == false)
                        return strCaption;
                }

                // 3. 找只是前两个字符相同的
                // xpath式子经过了验证。xpath下标习惯从1开始
                nodeCaption = this.PropertyNode.SelectSingleNode("logicname/caption[(substring(@lang,1,2)='" + strShortLang + "')]");
                if (nodeCaption != null)
                {
                    // strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                    strCaption = nodeCaption.InnerText.Trim();  // 2012/2/15
                    if (String.IsNullOrEmpty(strCaption) == false)
                        return strCaption;
                }

            }

        END1:
            // 4.最后找排在第一位的caption
            nodeCaption = this.PropertyNode.SelectSingleNode("logicname/caption");
            if (nodeCaption != null)
            {
                // strCaption = DomUtil.GetNodeText(nodeCaption).Trim();
                strCaption = nodeCaption.InnerText.Trim();  // 2012/2/15

            }
            if (string.IsNullOrEmpty(strCaption) == false)
                return strCaption;

            // 5.最后一个语言版本信息都没有时，返回数据库的id
            return this.FullID; // TODO: 是不是还要加上方括号之类?
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
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCaptionSafety(strLang)，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                return this.GetCaption(strLang);

            }
            finally
            {
                m_db_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCaptionSafety(strLang)，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }


        // 得到全部语言的caption。每个元素内的格式 语言代码:内容
        public List<string> GetAllLangCaption()
        {
            List<string> result = new List<string>();

            XmlNodeList listCaption =
                this.PropertyNode.SelectNodes("logicname/caption");
            foreach (XmlNode nodeCaption in listCaption)
            {
                string strLang = DomUtil.GetAttr(nodeCaption, "lang");
                string strText = nodeCaption.InnerText.Trim(); // 2012/2/16

                result.Add(strLang + ":" + strText);
            }

            return result;
        }

        // 线: 安全的
        public List<string> GetAllLangCaptionSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetAllLangCaptionSafety()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                return GetAllLangCaption();
            }
            finally
            {
                m_db_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetAllLangCaptionSafety()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 得到全部的caption，多个值之间用分号(?)分隔
        public string GetAllCaption()
        {
            StringBuilder strResult = new StringBuilder(4096);
            XmlNodeList listCaption =
                this.PropertyNode.SelectNodes("logicname/caption");
            foreach (XmlNode nodeCaption in listCaption)
            {
                if (strResult.Length > 0)
                    strResult.Append(",");
                strResult.Append(nodeCaption.InnerText.Trim()); // 2012/2/16
            }
            return strResult.ToString();
        }

        // 得到所有caption的值,以逗号分隔
        // 线: 安全的
        public string GetCaptionsSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCaptionSafety()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                return GetAllCaption();
            }
            finally
            {
                m_db_lock.ReleaseReaderLock();
                //*****************对数据库解读锁*********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCaptionSafety()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 得到所有caption的值及ID,以逗号分隔
        // 线: 安全的
        public string GetAllNameSafety()
        {
            //***********对数据库加读锁******GetCaption可能会抛出异常，所以用try,catch
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetAllNameSafety()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
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
                m_db_lock.ReleaseReaderLock();
                //*****************对数据库解读锁********
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetAllNameSafety()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
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
                m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
                try
                {
                    return GetDbo();
                }
                finally
                {
                    //**********对数据库解读锁************
                    m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
					this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");

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
                m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
                try
                {
                    return GetDbType();
                }
                finally
                {
                    //**********对数据库解读锁************
                    m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
					this.container.WriteDebugInfo("TypeSafety属性，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");

#endif
                }
            }
        }

        #region 数据库尾号

        internal bool m_bTailNoVerified = false; // 数据库尾号是否被校验过？

        // 尾号锁,在GetNewTailNo() 和 SetIfGreaten
        protected MyReaderWriterLock m_TailNolock = new MyReaderWriterLock();

        // 检查数据库尾号
        // parameters:
        //      strError    out参数，返回出错信息
        // return:
        //      -2  连接错误
        //      -1  出错
        //      0   成功
        // 线：安全的
        public int CheckTailNo(CancellationToken token,
            out string strError)
        {
            strError = "";

            token.ThrowIfCancellationRequested();

            if (this.m_bTailNoVerified == true)
                return 0;

            string strRealTailNo = "";
            int nRet = 0;

            //********对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("CheckTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                // return:
                //      -1  一般错误
                //      -2  连接错误
                //      0   成功
                nRet = this.UpdateStructure(out strError);
                if (nRet < 0)
                    return nRet;

                token.ThrowIfCancellationRequested();

                // return:
                //		-1  出错
                //      0   未找到
                //      1   找到
                nRet = this.GetRecordID("-1",
                    "prev",
                    out strRealTailNo,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得数据库 '" + this.GetCaption("zh") + "' 的最大记录号时出错: " + strError;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                // ???有可能还未初始化数据库
                strError = "获得数据库 '" + this.GetCaption("zh") + "' 的最大记录号时发生异常：" + ex.Message;
                return -1;
            }
            finally
            {
                //***********对数据库解读锁***************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("CheckTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            token.ThrowIfCancellationRequested();

            //this.container.WriteErrorLog("走到'" + this.GetCaption("zh-CN") + "'数据库的CheckTailNo()函数里，查到最大记录号为'" + strRealTailNo + "'。");

            if (nRet == 1)
            {
                //调SetIfGreaten()函数，如果本记录号大于尾号,自动推动尾号为最大
                //这种情况发生在我们给的记录号超过尾号时
                bool bPushTailNo = false;
                bPushTailNo = this.AdjustTailNo(Convert.ToInt32(strRealTailNo),
                    false);
            }

            this.m_bTailNoVerified = true;
            return 0;
        }

        // 获得可用的 ID，并推动系统记载的尾号
        // parameters:
        //      strID   输入的 ID 如果为 "-1"，表示希望系统根据当前库记忆的尾号给出一个可用的 ID。
        // exceptions:
        //      Exception   数据库尾号未经过校验；发生的新尾号不合法
        public bool EnsureID(ref string strID, bool bSimulate)
        {
            // bool bTailNoChanged = false;    // 数据库记忆的尾号是否发生了变化?
            if (this.m_bTailNoVerified == false)
                throw (new TailNumberException("数据库 '" + this.GetCaption("zh") + "' 因其尾号尚未经过校验，无法进行写入操作"));

            if (strID == "-1") // 追加记录,GetNewTailNo()是安全的
            {
                if (bSimulate)
                {
                    strID = Convert.ToString(this.container.TailNumberManager.NewTailNumber(this, this.GetTailNo()));
                }
                else
                {
                    strID = Convert.ToString(GetNewTailNo());// 加得库写锁
                    if (strID == "-1")
                        throw (new Exception("数据库 '" + this.GetCaption("zh") + "' 因其尾号尚未经过校验，无法进行追加新记录的写入操作"));
                }
                strID = DbPath.GetID10(strID);
                return true;
            }

            //*******对数据库加读锁**********************
            m_TailNolock.AcquireUpgradeableReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("EnsureID()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                strID = DbPath.GetID10(strID);
                // TODO: 这一句性能如何?
                if (StringUtil.RegexCompare(@"\B[0123456789]+", strID) == false)
                {
                    throw (new Exception("记录号 '" + strID + "' 不合法"));
                }

                if (bSimulate)
                {
                    return this.container.TailNumberManager.PushTailNumber(this, Convert.ToInt32(strID));
                }

                // 调SetIfGreaten()函数，如果本记录号大于尾号,自动推动尾号为最大
                // 这种情况发生在我们给的记录号超过尾号时
                return AdjustTailNo(Convert.ToInt32(strID),
                    true);
            }
            finally
            {
                //***********对数据库解读锁**********
                this.m_TailNolock.ReleaseUpgradeableReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("EnsureID()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 数据库刚打开的时候，因为校验过尾号，所以知道数据库的实际尾号是什么
        // 如果在每个创建数据库记录的位置都维持好一个内存的实际尾号，那么就可以用来后面进行内存比较，
        // 作为 select 从 records 表获得已有行的一个初筛。只有当小于这个实际尾号的记录才有必要去 select，这样就提高了速度
        // 还有另外一个办法，就是针对所有追加的，也就是问号索引号的情况，都优先去 insert records 表， 等出现了报错，才改为覆盖处理
        // 
        // 因为数据库打开的时候校验过尾号，所以追加的时候，在获得的尾号位置还有突然出现的已经存在的行的几率太小了
        // 如果在批处理过程中把整个数据库锁定，则这样的几率就更小了
        // 可以研究一下在一次性提交的若干 insert 语句中，到底是哪些碰到了已经存在的记录，然后好进行重试


        // 得到种子值
        // return
        //      记录尾号
        // 线: 不安全
        // 异常：如果字符串格式不对可能会抛出异常
        private Int64 GetTailNo()
        {
            XmlNode nodeSeed =
                this.PropertyNode.SelectSingleNode("seed");
            if (nodeSeed == null)
                throw new Exception("服务器配置文件错误,未找到<seed>元素。");

            return System.Convert.ToInt64(nodeSeed.InnerText.Trim()); // 2012/2/16
        }

        // 修改数据库的尾号
        // parameter:
        //		 nSeed  传入的尾号数
        // 线: 不安全
        protected void ChangeTailNo(Int64 nSeed)  //设为protected是因为在初始化时被访问
        {
            XmlNode nodeSeed =
                this.PropertyNode.SelectSingleNode("seed");

            // 2016/12/10
            if (nodeSeed == null)
                throw new Exception("服务器配置文件错误,未找到<seed>元素。");

            nodeSeed.InnerText = Convert.ToString(nSeed);   // 2012/2/16
            this.container.Changed = true;
        }

        // 有条件地减量数据库记忆的尾号
        // 所谓条件是：strDeletedID 这个号码刚好是当前最尾一个号码
        internal bool TryRecycleTailNo(string strDeletedID)
        {
            if (this.m_bTailNoVerified == false)
                return false;

            //****************对数据库尾号加写锁***********
            this.m_TailNolock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("TryDecreaseTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库尾号加写锁。");
#endif
            try
            {
                Int64 nNumber = 0;
                if (Int64.TryParse(strDeletedID, out nNumber) == false)
                    return false;

                Int64 nTemp = GetTailNo();   //必须采用这种方法，不能直接用Seed++
                if (nNumber == nTemp)
                {
                    ChangeTailNo(nTemp - 1);
                    return true;
                }

                return false;
            }
            finally
            {
                //***************对数据库尾号解写锁************
                this.m_TailNolock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("TryDecreaseTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库尾号解写锁。");
#endif
            }
        }

        // 得到记录的ID尾号（先加1再返回）,
        // 线: 安全
        // 加锁原因，加写锁，修改了nodeSeed内容，并始终保持增加，所以此时他处不能再读和写
        // return:
        //      -1  出错。原因是当前数据库记忆的尾号尚未经过校验
        //      其他  返回整数ID
        protected Int64 GetNewTailNo()
        {
            //****************对数据库尾号加写锁***********
            this.m_TailNolock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetNewTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库尾号加写锁。");
#endif
            try
            {
                if (this.m_bTailNoVerified == false)
                    return -1;

                Int64 nTemp = GetTailNo();   //必须采用这种方法，不能直接用Seed++
                nTemp++;
                ChangeTailNo(nTemp);
                return nTemp; //GetTailNo();
            }
            finally
            {
                //***************对数据库尾号解写锁************
                this.m_TailNolock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetNewTailNo()，对'" + this.GetCaption("zh-CN") + "'数据库尾号解写锁。");
#endif
            }
        }

        // 推动数据库记忆的尾号，如果必要
        // 如果用户手加输入的记录号大于的数据库的尾号，
        // 则要修改尾号，此时将读锁升级为写锁，
        // 修改完后再降级为读锁
        // parameter:
        //		nID         传入ID
        //		isExistReaderLock   是否已存在读锁。若已经存在则本函数就不加锁了
        // 线: 安全的
        // return:
        //      是否发生了推动尾号的情况
        protected bool AdjustTailNo(Int64 nID,
            bool isExistReaderLock)
        {
            bool bTailNoChanged = false;

            if (isExistReaderLock == false)
            {
                //*********对数据库尾号加读锁*************
                // this.m_TailNolock.AcquireReaderLock(m_nTimeOut);
                this.m_TailNolock.AcquireUpgradeableReaderLock(m_nTimeOut);

#if DEBUG_LOCK
				this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-CN") + "'数据库尾号加读锁。");
#endif
            }
            else
            {
                // 外围已经加锁，注意应当是 AcquireUpgradeableReaderLock()
            }

            try
            {
                Int64 nSavedNo = GetTailNo();
                if (nID > nSavedNo)
                {
                    // 2006/12/8 注释掉
                    // 写日志
                    // this.container.WriteErrorLog("发现数据库'" + this.GetCaption("zh-CN") + "'的实际尾号'" + Convert.ToString(nID) + "'大于保存的尾号'" + Convert.ToString(nSavedNo) + "'，推动尾号。");
                    bTailNoChanged = true;

                    //*********上升读锁为写锁************
                    LockCookie lc = m_TailNolock.UpgradeToWriterLock(m_nTimeOut);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-CN") + "'数据库尾号读锁上升为写锁。");
#endif
                    try
                    {
                        ChangeTailNo(nID);
                    }
                    finally
                    {
                        //*************降写锁为读锁*************
                        m_TailNolock.DowngradeFromWriterLock(ref lc);
#if DEBUG_LOCK
						this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-CN") + "'数据库尾号写锁下降为读锁。");
#endif
                    }
                }

                return bTailNoChanged;
            }
            finally
            {
                if (isExistReaderLock == false)
                {
                    //*******对数据库尾号解读锁********
                    // this.m_TailNolock.ReleaseReaderLock();
                    this.m_TailNolock.ReleaseUpgradeableReaderLock();

#if DEBUG_LOCK
					this.container.WriteDebugInfo("SetIfGreaten()，对'" + this.GetCaption("zh-CN") + "'数据库尾号解读锁。");
#endif
                }
            }
        }

        #endregion

#if NO
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
#endif

        // 2007/9/14改造后版本
        // 将数据库多个表的标签信息，转换成TableInfo对象数组
        // 并检查检索途径中是否包含id，如果检索途径为空，表示按全部检索点进行检索(不包含id)
        // parameter:
        //		strTableNames   检索途径名称，之间用逗号分隔。如果为空,则表示利用所有的表进行检索(但不包含通过id途径检索)
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

            //如果strTableNames为空,则返回所有的表,但不包含通过id检索
            if (strTableNames == ""
                || strTableNames.ToLower() == "<all>"
                || strTableNames == "<全部>")
            {
                if (keysCfg != null)
                {
                    nRet = keysCfg.GetTableInfosRemoveDup(
                        out aTableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                return 0;
            }

#if NO
            // 2007/9/14 新增，提升执行速度
            List<TableInfo> ref_table_infos = new List<TableInfo>();
            nRet = keysCfg.GetTableInfosRemoveDup(out ref_table_infos,
                out strError);
            if (nRet == -1)
                return -1;
#endif
            List<TableInfo> ref_table_infos = new List<TableInfo>();
            // 2017/5/4
            // 不用 ...RemoveDup() 的原因是，strTableName 可能为 "@311" 形态，必须针对没有去重的列表才能正确定位表对象
            nRet = keysCfg.GetTableInfos(out ref_table_infos,
                out strError);
            if (nRet == -1)
                return -1;

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
                        ref_table_infos, // 2007/9/14 新增，提升执行速度
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
        //      strOutpuStyle   输出的风格。如果为keycount，则输出归并统计后的key+count；否则，或者缺省，为传统的输出记录id
        //		searchItem  SearchItem对象
        //		isConnected IsConnection对象，用于判断通讯是否连接
        //      resultSet   结果集对象，存放命中记录。本函数并不在检索前清空结果集，因此，对同一结果集对象多次执行本函数，则可以把命中结果追加在一起
        //		nWarningLevel   处理警告级别 0：表示特别强烈，出现警告也当作出错；1：表示不强烈，不返回出错，继续执行
        //		strError    out参数，返回出错信息
        //		strWarning  out参数，返回警告信息
        // return:
        //		-1  出错
        //		>=0 命中记录数
        // 线: 安全的
        internal virtual int SearchByUnion(
            string strOutputStyle,
            SearchItem searchItem,
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
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

        // 2008/11/14
        // 刷新数据库(SQL表)定义，注意虚函数不能为private
        // parameter:
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的,在派生类里加锁
        public virtual int RefreshPhysicalDatabase(
            bool bClearAllKeyTables,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // parameters:
        //      strAction   delete/create/rebuild/disable/rebuildall/disableall
        public virtual int ManageKeysIndex(
            string strAction,
            out string strError)
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
            long lStart,
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
        // parameters:
        //      strRecordID  从属的记录ID
        //      strObjectID  资源对象ID
        //      strXPath    要获取的记录局部描述
        // 其它参数GetXml(),无strOutputResPath参数
        // 线: 安全的
        public virtual long GetObject(string strRecordID,
            string strObjectID,
            string strXPath,
            long lStart,
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
            this.m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                strRecordID = DbPath.GetID10(strRecordID);
                //*******************对记录加读锁************************
                m_recordLockColl.LockForRead(strRecordID,
                    m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加读锁。");
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
					this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解读锁。");
#endif
                }
            }

            finally
            {
                //*********对数据库解读锁****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXmlDataSafety()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
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

        // 根据缓存的 ID 列表，重建这些记录的检索点
        //      strStyle    "fastmode,deletekeys" 意思是更新数据库中一部分记录的检索点，因此需要逐条删除 keys，因此要求开始阶段keys表的 B+ 树不要删除。后面为了 buikcopy 可以删除 B+ 树
        //                  ""  意思是慢速模式。本函数返回后不需要任何后续工作
        // return:
        //      -1  出错
        //      >=0 处理的 keys 行数
        public virtual int RebuildKeys(
            string strStyle,
            out string strError)
        {
            strError = "";

            if (this.RebuildIDs == null || this.RebuildIDs.Count == 0)
                return 0;

            bool bFastMode = StringUtil.IsInList("fastmode", strStyle);

            string strSubStyle = "rebuildkeys";
            if (bFastMode == false)
                strSubStyle += ",deletekeys";

            if (string.IsNullOrEmpty(strStyle) == false)
                strSubStyle += "," + strStyle;  // 如果 strStyle 中本来就有 deletekeys，也会带过来

            this.RebuildIDs.Seek(0);    // 把指针放到开头位置
            // TODO: 应当锁定对象，让文件指针不再改变 ?

            // List<string> ids = new List<string>();
            List<RecordBody> records = new List<RecordBody>();

            int nTotalCount = 0;

            string strID = "";
            while (this.RebuildIDs.Read(out strID))
            {
                if (string.IsNullOrEmpty(strID) == true)
                    continue;   // 跳过已经标记删除的 ID

                if (records.Count > 100)
                {
                    // List<RecordBody> outputs = null;

                    int nRet = WriteRecords(
                        null,   // User oUser,
                        records,
                        strSubStyle,
                        out List<RecordBody> outputs,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    // TODO: 完善错误处理。一般应当尽可能继续处理下去。或者重试一次(如果是Bulkcopy，需要考虑重试是否会重复产生keys信息行的问题)

                    // TODO: 是否及时清空那些已经处理的 ID?

                    nTotalCount += nRet;
                    records.Clear();
                }

                RecordBody record = new RecordBody
                {
                    Path = "./" + strID
                };
                records.Add(record);
            }

            // 最后一批
            if (records.Count > 0)
            {
                int nRet = WriteRecords(
                    null,   // User oUser,
                    records,
                    strSubStyle,
                    out List<RecordBody> outputs,
                    out strError);
                if (nRet == -1)
                    return -1;
                // TODO: 完善错误处理。一般应当尽可能继续处理下去。或者重试一次(如果是Bulkcopy，需要考虑重试是否会重复产生keys信息行的问题)
                // TODO: 是否及时清空那些已经处理的 ID?
                nTotalCount += nRet;
                records.Clear();
            }

            return nTotalCount;
        }

        public virtual long BulkCopy(
            string strAction,
            out string strError)
        {
            strError = "";

            return 0;
        }

        internal class WriteResInfo
        {
            public string ID = "";
            public string XPath = "";
        }

        // 写入一批 XML 记录
        // 这里利用 WriteXml 实现了基本功能，但速度没有得到优化。派生类可以重写此函数，以求得最快的速度
        // return:
        //      -1  出错
        //      >=0 如果是 rebuildkeys，则返回总共处理的 keys 行数
        public virtual int WriteRecords(
            // SessionInfo sessininfo,
            User oUser,
            List<RecordBody> records,
            string strStyle,
            out List<RecordBody> outputs,
            out string strError)
        {
            strError = "";
            outputs = new List<RecordBody>();

            if (StringUtil.IsInList("rebuildkeys", strStyle) == true)
            {
                strError = "目前 Database::WriteRecords() 尚未实现重建检索点的功能";
                return -1;
            }

            foreach (RecordBody record in records)
            {
                string strPath = record.Path;   // 包括数据库名的路径

                string strDbName = StringUtil.GetFirstPartPath(ref strPath);

                bool bObject = false;
                string strRecordID = "";
                string strObjectID = "";
                string strXPath = "";

                string strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                //***********吃掉第2层*************
                // 到此为止，strPath不含记录号层了，下级分情况判断
                strRecordID = strFirstPart;
                // 只到记录号层的路径
                if (strPath == "")
                {
                    bObject = false;
                }
                else
                {
                    strFirstPart = StringUtil.GetFirstPartPath(ref strPath);
                    //***********吃掉第2层*************
                    // 到此为止，strPath不含object或xpath层 strFirstPart可能是object 或 xpath

                    if (strFirstPart != "object"
        && strFirstPart != "xpath")
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "资源路径 '" + record.Path + "' 不合法, 第3级必须是 'object' 或 'xpath' ";
                        record.Result.ErrorCode = ErrorCodeValue.PathError; // -7;
                        continue;
                    }
                    if (string.IsNullOrEmpty(strPath) == true)  //object或xpath下级必须有值
                    {
                        record.Result.Value = -1;
                        record.Result.ErrorString = "资源路径 '" + record.Path + "' 不合法,当第3级是 'object' 或 'xpath' 时，第4级必须有内容";
                        record.Result.ErrorCode = ErrorCodeValue.PathError; // -7;
                        continue;
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
                }

                if (bObject == true)
                {
                    record.Result.Value = -1;
                    record.Result.ErrorString = "目前不允许用 WriteRecords 写入对象资源";
                    record.Result.ErrorCode = ErrorCodeValue.CommonError;
                    continue;
                }

                byte[] baContent = Encoding.UTF8.GetBytes(record.Xml);
                string strRanges = "0-" + (baContent.Length - 1).ToString();

                byte[] outputTimestamp = null;
                string strOutputID = "";
                string strOutputValue = "";

                int nRet = WriteXml(oUser,
                strRecordID,
                strXPath,
                strRanges,
                baContent.Length,
                baContent,
                record.Metadata,
                strStyle,
                record.Timestamp,
            out outputTimestamp,
            out strOutputID,
            out strOutputValue,
            false,
            out strError);
                if (nRet <= -1)
                {
                    record.Result.Value = -1;
                    record.Result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    record.Result.ErrorString = strError;
                }
                else
                {
                    if (string.IsNullOrEmpty(strXPath) == true)
                        record.Path = strDbName + "/" + strOutputID;
                    else
                        record.Path = strDbName + "/" + strOutputID + "/xpath/" + strXPath;
                    record.Result.Value = nRet;
                    record.Result.ErrorCode = ErrorCodeValue.NoError;
                    record.Result.ErrorString = strOutputValue;
                }

                record.Timestamp = outputTimestamp;
                record.Metadata = null;
                record.Xml = null;

                outputs.Add(record);
            }

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
            // Stream streamSource,
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
            // Stream streamSource,
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
        //      strStyle        可包含 fastmode
        //		inputTimestamp  输入的时间戳
        //		outputTimestamp out参数，返回时间戳,当不正确时有效
        //		strError        out参数，返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全的,加写锁,在派生类里加锁
        public virtual int DeleteRecord(
            string strID,
            string strObjectID,
            byte[] timestamp,
            string strStyle,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";
            return 0;
        }

        // 2008/11/14
        // 重建记录的keys
        // parameter:
        //		strRecordID     记录ID
        //      strStyle    next prev outputpath forcedeleteoldkeys
        //                  forcedeleteoldkeys 要在创建新keys前强制删除一下旧有的keys? 如果为包含，则强制删除原有的keys；如果为不包含，则试探着创建新的keys，如果有旧的keys和新打算创建的keys重合，那就不重复创建；如果旧的keys有残余没有被删除，也不管它们了
        //                          包含 一般用在单条记录的处理；不包含 一般用在预先删除了所有keys表的内容行以后在循环重建库中每条记录的批处理方式
        //		strError        out参数，返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        // 线: 安全的,加写锁,在派生类里加锁
        public virtual int RebuildRecordKeys(string strRecordID,
            string strStyle,
            out string strOutputRecordID,
            out string strError)
        {
            strError = "";
            strOutputRecordID = "";

            return 0;
        }

        Hashtable m_xpath_table = null;

        // 得到一条记录的浏览格式，一个字符串数组
        // parameter:
        //      strFormat   浏览格式定义。如果为空，表示 相当于 cfgs/browse
        //                  如果有 @coldef: 引导，表示为 XPath 形态的列定义，例如 @def://parent|//title ，竖线表示列的间隔
        //                  否则表示 browse 配置文件的名称，例如 cfgs/browse_temp 之类
        //		strRecordID	一般自由位数的记录号，或10位数字的记录号
        //      strXml  记录体。如果为空，本函数会自动获取记录体
        //      nStartCol   开始的列号。一般为0
        //      strStyle    构造 cols 的风格。"titles:type1|type2"
        //		cols        	out参数，返回浏览格式数组
        // 当出错时,错误信息也保存在列里
        // return:
        //      // cols中包含的字符总数
        //      -1  出错
        //      0   记录没有找到
        //      其他  cols 中包含的字符总数。最少为 1，不会为 0
        public int GetCols(
            string strFormat,
            string strRecordID,
            string strXml,
            // int nStartCol,
            string strStyle,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = null;
            int nRet = 0;

            if (string.IsNullOrEmpty(strFormat) == true)
                strFormat = "cfgs/browse";

            //**********对数据库加读锁**************
            this.m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                BrowseCfg browseCfg = null;
                string strColDef = "";

                // 列定义
                if (strFormat[0] == '@')
                {
                    if (StringUtil.HasHead(strFormat, "@coldef:") == true)
                        strColDef = strFormat.Substring("@coldef:".Length).Trim();
                    else
                    {
                        strError = "无法识别的浏览格式 '" + strFormat + "'";
                        goto ERROR1;
                    }
                }
                else
                {
                    // 浏览格式配置文件

                    // string strBrowseName = this.GetCaption("zh") + "/" + strFormat; // TODO: 应当支持./cfgs/xxxx 等
                    string strBrowseName = strFormat; // 例如 cfgs/browse 。因为已经是在特定的数据库中获得配置文件，所以无需指定库名部分
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = this.GetBrowseCfg(
                        strBrowseName,
                        out browseCfg,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (browseCfg == null)
                    {
#if NO
                        // 2013/1/14
                        cols = new string[1];
                        cols[0] = strError;
                        return -1;
#endif
                        // 2017/5/11
                        goto ERROR1;
                    }
                }

                // string strXml;

                // 获得记录体
                if (string.IsNullOrEmpty(strXml) == true)   // 2012/1/5
                {
                    strRecordID = DbPath.GetID10(strRecordID);
                    //*******************对记录加读锁************************
                    m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加读锁。");
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
                        // 2017/5/11
                        if (nRet == -4)
                        {
                            if (string.IsNullOrEmpty(strError))
                                strError = "记录 '" + strRecordID + "' 不存在";
                            cols = new string[1];
                            cols[0] = strError;
                            return 0;
                        }
                        if (nRet <= -1)
                            goto ERROR1;

                        // 2017/7/1
                        if (string.IsNullOrEmpty(strXml))
                        {
                            strError = "记录 '" + strRecordID + "' 的 XML 为空";
                            cols = new string[1];
                            cols[0] = strError;
                            return 0;
                        }
                    }
                    finally
                    {
                        //*******************对记录解读锁************************
                        m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解读锁。");
#endif
                    }
                }

                XmlDocument domData = new XmlDocument();
                try
                {
                    domData.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "加载 '" + this.GetCaption("zh-CN") + "' 库的 '" + strRecordID + "' 记录到 dom 时出错,原因: " + ex.Message;
                    goto ERROR1;
                }

                if (browseCfg != null)
                {
                    // return:
                    //		-1	出错
                    //		>=0	成功。数字值代表每个列包含的字符数之和
                    nRet = browseCfg.BuildCols(domData,
                        // nStartCol,
                        strStyle,
                        out cols,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 列定义

                    // 预防 Hashtable 变得过大
                    if (m_xpath_table != null && m_xpath_table.Count > 1000)
                    {
                        lock (m_xpath_table)
                        {
                            m_xpath_table.Clear();
                        }
                    }

                    nRet = BuildCols(
                        ref m_xpath_table,
                        strColDef,
                        domData,
                        // nStartCol,
                        out cols,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                return Math.Max(1, nRet);   // 避免返回 0 和没有找到的情况混淆
                ERROR1:
                // 2018/10/10
                if (cols == null || cols.Length == 0)
                {
                    cols = new string[1];
                    cols[0] = "error: " + strError;
                }
#if NO
                cols = new string[1];
                cols[0] = strError;
                return strError.Length;
#endif
                return -1;
            }
            finally
            {
                //****************对数据库解读锁**************
                this.m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetCols()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

#if NO
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
#endif

        // TODO: XPathExpression可以缓存起来，加快速度
        // 创建指定记录的浏览格式集合
        // parameters:
        //		domData	    记录数据dom 不能为null
        //      nStartCol   开始的列号。一般为0
        //      cols        浏览格式数组
        //		strError	out参数，出错信息
        // return:
        //		-1	出错
        //		>=0	成功。数字值代表每个列包含的字符数之和
        static int BuildCols(
            ref Hashtable xpath_table,
            string strColDef,
            XmlDocument domData,
            // int nStartCol,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = new string[0];

            Debug.Assert(domData != null, "BuildCols()调用错误，domData参数不能为null。");

            if (xpath_table == null)
                xpath_table = new Hashtable();

            int nResultLength = 0;

            string[] xpaths = strColDef.Split(new char[] { '|' });

            XPathNavigator nav = domData.CreateNavigator();

            List<string> col_array = new List<string>();

            for (int i = 0; i < xpaths.Length; i++)
            {
                string strSegment = xpaths[i];

#if NO
                string strXpath = "";
                string strConvert = "";
                StringUtil.ParseTwoPart(strSegment, "->", out strXpath, out strConvert);
                if (string.IsNullOrEmpty(strXpath) == true)
                {
                    col_array.Add("");  // 空的 XPath 产生空的一列
                    continue;
                }

                // -> 左边的部分又可被 !nl= 分为两部分。!nl= 后面的部分为名字空间对照表，格式为 prefix1=url;prefix2=url
                string strNameList = "";
                {
                    List<string> parts = StringUtil.ParseTwoPart(strXpath, "!nl=");
                    strXpath = parts[0];
                    strNameList = parts[1];
                }
#endif
                XPathInfo info = ParseSegment(strSegment);


                // 2017/5/11
                XmlNamespaceManager nsmgr = null;
                if (string.IsNullOrEmpty(info.NameList) == false)
                {
                    nsmgr = (XmlNamespaceManager)xpath_table[info.NameList];

                    if (nsmgr == null)
                    {
                        nsmgr = BuildNamespaceManager(info.NameList, domData);
                        lock (xpath_table)
                        {
                            xpath_table[info.NameList] = nsmgr;
                        }
                    }
                }

                XPathExpression expr = (XPathExpression)xpath_table[info.XPath];
                // TODO: nsmgr_table

                if (expr == null)
                {
                    // 创建Cache
                    try
                    {
                        expr = nav.Compile(info.XPath);
                        if (nsmgr != null)
                            expr.SetContext(nsmgr);

                        lock (xpath_table)
                        {
                            xpath_table[info.XPath] = expr;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{ex.Message}。XPath='{info.XPath}'", ex);
                    }
                }

                Debug.Assert(expr != null, "");

                string strText = "";

                if (expr.ReturnType == XPathResultType.Number)
                {
                    strText = nav.Evaluate(expr).ToString();//Convert.ToString((int)(nav.Evaluate(expr)));
                }
                else if (expr.ReturnType == XPathResultType.Boolean)
                {
                    strText = Convert.ToString((bool)(nav.Evaluate(expr)));
                }
                else if (expr.ReturnType == XPathResultType.String)
                {
                    strText = (string)(nav.Evaluate(expr));
                }
                else if (expr.ReturnType == XPathResultType.NodeSet)
                {
                    XPathNodeIterator iterator = nav.Select(expr);

                    while (iterator.MoveNext())
                    {
                        XPathNavigator navigator = iterator.Current;
                        string strOneText = navigator.Value;
                        if (strOneText == "")
                            continue;

                        // 2017/5/11
                        if (string.IsNullOrEmpty(strText) == false
                            && string.IsNullOrEmpty(info.Delimeter) == false)
                            strText += info.Delimeter;

                        strText += strOneText;
                    }
                }
                else
                {
                    strError = "XPathExpression的ReturnType为'" + expr.ReturnType.ToString() + "'无效";
                    return -1;
                }

                if (string.IsNullOrEmpty(info.Convert) == false)
                {
                    List<string> convert_methods = StringUtil.SplitList(info.Convert);
                    strText = BrowseCfg.ConvertText(convert_methods, strText);
                }

                // 空内容也要算作一列
                col_array.Add(strText);
                nResultLength += strText.Length;
            }

            // 把col_array转到cols里
            cols = col_array.ToArray();
            /*
            cols = new string[col_array.Count + nStartCol];
            col_array.CopyTo(cols, nStartCol);
            */
            return nResultLength;
        }

        class XPathInfo
        {
            public string XPath { get; set; }
            public string Convert { get; set; }
            public string Delimeter { get; set; }
            public string NameList { get; set; }
        }

        static XPathInfo ParseSegment(string strSegment)
        {
            XPathInfo info = new XPathInfo();

            List<string> parts = StringUtil.SplitList(strSegment, "->");

            foreach (string part in parts)
            {
                if (part.StartsWith("nl:"))
                {
                    info.NameList = part.Substring("nl:".Length);
                    continue;
                }
                if (part.StartsWith("dm:"))
                {
                    info.Delimeter = part.Substring("dm:".Length);
                    continue;
                }
                if (part.StartsWith("cv:"))
                {
                    info.Convert = part.Substring("cv:".Length);
                    continue;
                }

                if (info.XPath == null)
                {
                    info.XPath = part;
                    continue;
                }
                if (info.Convert == null)
                {
                    info.Convert = part;
                    continue;
                }
            }

            return info;
#if NO
            string strXpath = "";
            string strConvert = "";
            StringUtil.ParseTwoPart(strSegment, "->", out strXpath, out strConvert);
            if (string.IsNullOrEmpty(strXpath) == true)
            {
                col_array.Add("");  // 空的 XPath 产生空的一列
                continue;
            }

            // -> 左边的部分又可被 !nl= 分为两部分。!nl= 后面的部分为名字空间对照表，格式为 prefix1=url;prefix2=url
            string strNameList = "";
            {
                List<string> parts = StringUtil.ParseTwoPart(strXpath, "!nl=");
                strXpath = parts[0];
                strNameList = parts[1];
            }
#endif
        }

        static XmlNamespaceManager BuildNamespaceManager(string strNameList, XmlDocument domData)
        {
            if (string.IsNullOrEmpty(strNameList))
                return null;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(domData.NameTable);
            List<string> names = StringUtil.SplitList(strNameList);
            foreach (string s in names)
            {
                List<string> parts = StringUtil.ParseTwoPart(s, "=");
                nsmgr.AddNamespace(parts[0], parts[1]);
            }

            return nsmgr;
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
        public int API_PretendWrite(string strXml,
            string strRecordID,
            string strLang,
            // string strStyle,
            out KeyCollection keys,
            out string strError)
        {
            keys = null;
            strError = "";
            //**********对数据库加读锁**************
            this.m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK

			this.container.WriteDebugInfo("PretendWrite()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");

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
                        // strStyle,
                        this.KeySize,
                        out keys,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    //排序去重
                    //keys.Sort();
                    keys.RemoveDup();
                }
                return 0;
            }
            finally
            {
                //****************对数据库解读锁**************
                this.m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("PretendWrite()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 合并检索点
        // parameters:
        //      strNewXml   新记录的XML。可以为""或者null
        //      strOldXml   旧记录的XML。可以为""或者null
        //      bOutputDom  是否利用newDom/oldDom顺便输出DOM?
        // return:
        //      -2  strOldXml 结构不合法。但其实所有 out 参数均已按照 strOldXml 为空处理到位
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

            if (String.IsNullOrEmpty(strNewXml) == false)
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
                        // "",//strStyle,
                        this.KeySize,
                        out newKeys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    //newKeys.Sort();
                    newKeys.RemoveDup();
                }
            }

            // strOldXml 结构出错，专门出错信息
            string strOldError = "";

            oldKeys = new KeyCollection();

            if (String.IsNullOrEmpty(strOldXml) == false
                && strOldXml.Length > 1)    // 2012/1/31
            {
                oldDom = new XmlDocument();
                oldDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                try
                {
                    oldDom.LoadXml(strOldXml);
                }
                catch (Exception ex)
                {
                    strOldError = "加载旧数据到dom时出错。" + ex.Message;

                    // 2021/8/27
                    // TODO: 写入错误日志
                    oldDom.LoadXml("<root />");
                }

                if (keysCfg != null)
                {
                    nRet = keysCfg.BuildKeys(oldDom,
                        strID,
                        "zh",//strLang,
                        // "",//strStyle,
                        this.KeySize,
                        out oldKeys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    //oldKeys.Sort();
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

            if (string.IsNullOrEmpty(strOldError) == false)
                return -2;
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
            this.m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetInfo()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
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
                    nRet = this.container.GetFileCfgItemLocalPath(strDbName + "/cfgs/keys",
                        out strKeysFileName,
                        out strError);
                    if (nRet != 0)
                    {
                        if (nRet != -4)
                            return -1;
                    }

                    if (File.Exists(strKeysFileName) == true)
                    {
                        using (StreamReader sr = new StreamReader(strKeysFileName,
                            Encoding.UTF8))
                        {
                            strKeysText = sr.ReadToEnd();
                        }
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
                    nRet = this.container.GetFileCfgItemLocalPath(strDbName + "/cfgs/browse",
                        out strBrowseFileName,
                        out strError);
                    if (nRet != 0)
                    {
                        if (nRet != -4)
                            return -1;
                    }

                    if (File.Exists(strBrowseFileName) == true)
                    {
                        using (StreamReader sr = new StreamReader(strBrowseFileName,
                            Encoding.UTF8))
                        {
                            strBrowseText = sr.ReadToEnd();
                        }
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
                this.m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetInfo()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        public virtual string GetSourceName()
        {
            return "";
        }

        // 2008/5/7
        // 刷新总dom中的logicname片段
        // TODO：可以改进为，先刷新总DOM中的片段，然后重新设置m_propertyNode（和selfNode?）
        // 测试：把本段删除后，探索得到一个足以出现错误的固定测试流程，然后重新加入本段，看问题是否消失
        int RefreshLognames(string strID,
            string strLogicNames,
            out string strError)
        {
            strError = "";

            XmlNode nodeDatabase = this.container.NodeDbs.SelectSingleNode("database[@id='" + strID + "']");
            if (nodeDatabase == null)
            {
                strError = "id为'" + strID + "' 的<database>元素没有找到";
                return -1;
            }

            XmlNode nodeLogicName = nodeDatabase.SelectSingleNode("property/logicname");
            if (nodeLogicName == null)
            {
                strError = "id为'" + strID + "' 的<database>元素下没有找到property/logicname元素";
                return -1;
            }

            nodeLogicName.InnerXml = strLogicNames;
            m_captionTable.Clear(); // 2012/3/17

            return 0;
        }

        // 设置数据库的基本信息
        // parameters:
        //		logicNames	        LogicNameItem数组，用新的逻辑库名数组替换原来的逻辑库名数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout，目前无效，因为涉及到是文件库，还是sql库的问题
        //		strSqlDbName	    指定的新Sql数据库名称，目前无效，，如果数据库为文为文件型数据库，则返回数据源目录的名称
        //		strkeysDefault	    keys配置信息。如果为null，表示此项无效。(注：如果为""则表示要把文件内容清空)
        //		strBrowseDefault	browse配置信息。如果为null，表示此项无效。(注：如果为""则表示要把文件内容清空)
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

            // TODO: 这里使用 m_TailNoLock 是不是笔误？应为 m_db_lock？
            //****************对数据库加写锁***********
            m_TailNolock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("SetInfo()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");

#endif
            try
            {
                // 2008/4/30 changed
                // "" 和 null含义不同。后者表示不使用这个参数
                /*
                if (strKeysText == null)
                    strKeysText = "";
                if (strBrowseText == null)
                    strBrowseText = "";
                 * */

                if (String.IsNullOrEmpty(strKeysText) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strKeysText);
                    }
                    catch (Exception ex)
                    {
                        strError = "加载keys配置文件内容到dom出错(1)，原因:" + ex.Message;
                        return -1;
                    }
                }

                if (String.IsNullOrEmpty(strBrowseText) == false)
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
                XmlNode nodeLogicName = this.PropertyNode.SelectSingleNode("logicname");
                nodeLogicName.InnerXml = strLogicNames;

                int nRet = 0;

                // 2008/5/7
                nRet = RefreshLognames(this.PureID,
                    strLogicNames,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 目前不支持修改strType,strSqlDbName

                if (strKeysText != null)  // 2008/4/30
                {
                    string strKeysFileName = "";//this.GetFixedCfgFileName("keys");
                    string strDbName = this.GetCaption("zh");

                    // string strDbName = this.GetCaption("zh");

                    // return:
                    //		-1	一般性错误，比如调用错误，参数不合法等
                    //		-2	没找到节点
                    //		-3	localname属性未定义或为值空
                    //		-4	localname在本地不存在
                    //		-5	存在多个节点
                    //		0	成功
                    nRet = this.container.GetFileCfgItemLocalPath(strDbName + "/cfgs/keys",
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
                            nRet = this.container.SetFileCfgItem(
                                false,
                                this.GetCaption("zh") + "/cfgs",
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
                            nRet = this.container.GetFileCfgItemLocalPath(this.GetCaption("zh") + "/cfgs/keys",
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
                        using (Stream s = File.Create(strKeysFileName))
                        {

                        }
                    }

                    nRet = DatabaseUtil.CreateXmlFile(strKeysFileName,
                        strKeysText,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 把缓冲清空
                    this.m_keysCfg = null;
                }

                if (strBrowseText != null)  // 2008/4/30
                {
                    string strDbName = this.GetCaption("zh");

                    string strBrowseFileName = "";

                    // return:
                    //		-1	一般性错误，比如调用错误，参数不合法等
                    //		-2	没找到节点
                    //		-3	localname属性未定义或为值空
                    //		-4	localname在本地不存在
                    //		-5	存在多个节点
                    //		0	成功
                    nRet = this.container.GetFileCfgItemLocalPath(strDbName + "/cfgs/browse",
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
                            nRet = this.container.SetFileCfgItem(
                                false,
                                this.GetCaption("zh") + "/cfgs",
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
                            nRet = this.container.GetFileCfgItemLocalPath(this.GetCaption("zh") + "/cfgs/browse",
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
                        using (Stream s = File.Create(strBrowseFileName))
                        {

                        }
                    }

                    nRet = DatabaseUtil.CreateXmlFile(strBrowseFileName,
                        strBrowseText,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 把缓冲清空
                    // this.m_browseCfg = null;
                    // this.m_bHasBrowse = true; // 缺省值
                    this.browse_table.Clear();
                }

                return 0;
            }
            finally
            {
                //***************对数据库解写锁************
                m_TailNolock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("SetInfo()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
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
            out List<ResInfoItem> aItem,
            out string strError)
        {
            aItem = new List<ResInfoItem>();
            strError = "";

            //**********对数据库加读锁**************
            this.m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
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
                    string strCfgPath = this.GetCaption("zh-CN") + "/" + strChildName;
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

                        // 2012/5/16
                        if (string.IsNullOrEmpty(tableInfo.ExtTypeString) == false)
                        {
                            if (string.IsNullOrEmpty(resInfoItem.TypeString) == false)
                                resInfoItem.TypeString += ",";

                            resInfoItem.TypeString += tableInfo.ExtTypeString;
                        }

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
                this.m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
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
        internal int WriteFileForCfgItem(
            bool bNeedLock,
            string strCfgItemPath,
            string strFilePath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {

            if (bNeedLock == true)
            {
                //**********对数据库加写锁**************
                this.m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            }
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
                    // streamSource,
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
                /*
                if (strPathWithoutDbName == "cfgs/browse")
                {
                    this.m_browseCfg = null;
                    this.m_bHasBrowse = true; // 缺省值
                }
                 * */
                if (this.browse_table[strPathWithoutDbName.ToLower()] != null)
                {
                    this.browse_table.Remove(strPathWithoutDbName.ToLower());
                }

                return 0;
            }
            finally
            {
                if (bNeedLock == true)
                {

                    //**********对数据库解写锁**************
                    this.m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Dir()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
                }
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
                strError = "记录号 '" + strInputRecordID + "' 不合法。";
                return -1;
            }

            strOutputRecordID = DbPath.GetID10(strInputRecordID);
            return 0;
        }

        public void WriteErrorLog(string text)
        {
            this.container?.KernelApplication?.WriteErrorLog(text);
        }
    }
}