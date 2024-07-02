using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.rms
{
    // 文件库派生类
    public class FileDatabase : Database
    {
        // 纯净的数据库目录
        internal string m_strPureSourceDir = "";

        // 数据库目录全路径，末尾不带\
        internal string m_strSourceFullPath = "";

        public FileDatabase(DatabaseCollection container)
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
            this.m_db_lock.AcquireWriterLock(m_nTimeOut);
            try
            {
                this.m_selfNode = node;

                // 只能在这儿写了，要不对象未初始化呢。
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
                // 检索点长度
                // return:
                //      -1  出错
                //      0   成功
                // 线: 不安全
                int nRet = this.container.InternalGetKeySize(
                    out this.KeySize,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 库ID
                this.PureID = DomUtil.GetAttr(this.m_selfNode, "id").Trim();
                if (this.PureID == "")
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-CN") + "'的<database>下级未定义'id'属性，或'id'属性为空";
                    return -1;
                }

                // 属性节点
                this.PropertyNode = this.m_selfNode.SelectSingleNode("property");
                if (this.PropertyNode == null)
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-CN") + "'的<database>下级未定义<property>元素";
                    return -1;
                }

                XmlNode nodeDatasource = this.PropertyNode.SelectSingleNode("datasource");
                if (nodeDatasource == null)
                {
                    strError = "服务器配置文件不合法，在name为'" + this.GetCaption("zh-CN") + "'的database/property下级未定义<datasource>元素";
                    return -1;
                }

                // 纯净的数据源目录
                this.m_strPureSourceDir = nodeDatasource.InnerText.Trim(); // 2012/2/16
                if (this.m_strPureSourceDir == "")
                {
                    strError = "配置文件不合法，在name为'" + this.GetCaption("zh-CN") + "'的database/property/datasource的节点的内容为空";
                    return -1;
                }

                // 数据源目录全路径
                this.m_strSourceFullPath = this.container.DataDir + "\\" + this.m_strPureSourceDir;

            }
            finally
            {
                m_db_lock.ReleaseWriterLock();
                //***********对数据库解写锁*************
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Initial()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }

            return 0;
        }

        // 得到数据源目录，对于文件型数据库，则是数据源目录名。
        public override string GetSourceName()
        {
            return this.m_strPureSourceDir;
        }

        // 初始化物理数据源目录
        // parameter:
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        // 线: 安全的
        // 为什么要做加锁:因为DeleteDir(),seed
        public override int InitialPhysicalDatabase(
            out string strError)
        {
            strError = "";
            //************对数据库加写锁*********
            m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            try
            {
                // 创建数据目录
                // 如果已存在源数据目录，则先删除，再重新创建
                if (Directory.Exists(this.m_strSourceFullPath))
                    Directory.Delete(this.m_strSourceFullPath, true);
                Directory.CreateDirectory(this.m_strSourceFullPath);


                // 创建检索点文件
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

                    // 2.建检索点文件
                    string strText = @"<?xml version='1.0' encoding='utf-8' ?><root></root>";
                    for (int i = 0; i < aTableInfo.Count; i++)
                    {
                        TableInfo tableInfo = aTableInfo[i];
                        string strKeyFileName = this.TableName2TableFileName(tableInfo.SqlTableName);
                        FileUtil.String2File(strText, strKeyFileName);
                    }
                }

                // 3.设种子值
                this.ChangeTailNo(0);
                this.m_bTailNoVerified = true;  // 2011/2/26
                this.container.Changed = true;
            }
            finally
            {
                //**************对数据库解写锁************
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Initialize()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }
            return 0;
        }

        // 得到xml数据
        // 线:安全的,供外部调
        public override int GetXmlData(string strID,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            strID = DbPath.GetID10(strID);

            string strXmlFilePath = this.GetXmlFilePath(strID);
            strXml = FileUtil.File2StringE(strXmlFilePath);
            return 0;
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

            if (strCurrentRecordID.Length != 10)
                strCurrentRecordID = DbPath.GetID10(strCurrentRecordID);// 确保一下strID为10位数

            // 普通的时候返回原记录号
            if ((StringUtil.IsInList("prev", strStyle) == false)
                && (StringUtil.IsInList("next", strStyle) == false))
            {
                Debug.Assert(false, "GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
                throw new Exception("GetRecordID()调用错误，如果strStyle参数不包含prev与next值则不应走到这里。");
            }

            //从目录中得到所有表示记录文件
            string[] files = Directory.GetFiles(
                this.m_strSourceFullPath,
                "??????????.xml");
            List<string> records = new List<string>();
            foreach (string filename in files)
            {
                FileInfo fileInfo = new FileInfo(filename);
                // 2020/3/1
                fileInfo.Refresh();
                string strFileName = fileInfo.Name;
                if (this.IsRecord(strFileName) == false)
                    continue;
                records.Add(this.XmlFileName2RecordID(strFileName));
            }
            // 没有记录，当然也不存在记录了。
            if (records.Count == 0)
            {
                return 0;
            }

            // 对记录进行排序
            // records.Sort(new ComparerClass());
            records.Sort();


            // 向前找
            if ((StringUtil.IsInList("prev", strStyle) == true))
            {
                if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                {
                    strOutputRecordID = (string)records[records.Count - 1];
                    return 1;
                }
                else if (StringUtil.IsInList("myself", strStyle) == true)
                {
                    int nIndex = records.IndexOf(strCurrentRecordID);
                    if (nIndex != -1)
                    {
                        strOutputRecordID = strCurrentRecordID;
                        return 1;
                    }

                    for (int i = records.Count - 1; i >= 0; i--)
                    {
                        if (String.Compare((string)records[i], strCurrentRecordID) < 0)
                        {
                            strOutputRecordID = (string)records[i];
                            return 1;
                        }
                    }
                }
                else
                {
                    for (int i = records.Count - 1; i >= 0; i--)
                    {
                        if (String.Compare((string)records[i], strCurrentRecordID) < 0)
                        {
                            strOutputRecordID = (string)records[i];
                            return 1;
                        }
                    }
                }
            }
            else if (StringUtil.IsInList("next", strStyle) == true)
            {
                if (DbPath.GetCompressedID(strCurrentRecordID) == "-1")
                {
                    strOutputRecordID = (string)records[0];
                    return 1;
                }
                else if (StringUtil.IsInList("myself", strStyle) == true)
                {
                    int nIndex = records.IndexOf(strCurrentRecordID);
                    if (nIndex != -1)
                    {
                        strOutputRecordID = strCurrentRecordID;
                        return 1;
                    }
                    for (int i = 0; i < records.Count; i++)
                    {
                        if (String.Compare((string)records[i], strCurrentRecordID) > 0)
                        {
                            strOutputRecordID = (string)records[i];
                            return 1;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < records.Count; i++)
                    {
                        if (String.Compare((string)records[i], strCurrentRecordID) > 0)
                        {
                            strOutputRecordID = (string)records[i];
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }

        // 按指定范围读资源
        // parameter:
        //		strID       记录ID
        //		nStart      开始位置
        //		nLength     长度 -1:开始到结束
        //		nMaxLength  最大长度
        //		destBuffer  out参数，返回字节数组
        //		timestamp   out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      -10 记录局部未找到
        //		>=0 资源总长度
        //      nAdditionError -50 有一个以上下级资源记录不存在(TODO:尚未实现 2006/7/3)
        public override long GetXml(string strID,
            string strXPath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out string strOutputResID,
            out byte[] outputTimestamp,
            bool bCheckAccount,
            out int nAdditionError,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            strOutputResID = "";
            outputTimestamp = null;
            strError = "";
            nAdditionError = 0;

            int nRet = 0;

            if (strID == "?")
                strID = "-1";

            try
            {
                long nId = Convert.ToInt64(strID);
                if (nId < -1)
                {
                    strError = "记录号'" + strID + "'不合法";
                    return -1;
                }
            }
            catch
            {
                strError = "记录号'" + strID + "'不合法";
                return -1;
            }


            // 根据风格取记录号
            strStyle = strStyle.Trim();
            if (StringUtil.IsInList("prev", strStyle) == true
                || StringUtil.IsInList("next", strStyle) == true)
            {

                // 得到指定的记录号
                // return:
                //		-1  出错
                //      0   未找到
                //      1   找到
                // 线：不安全
                nRet = this.GetRecordID(strID,
                    strStyle,
                    out strOutputResID,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 0)
                {
                    strError = "没找到记录号'" + strID + "'的风格为'" + strStyle + "'的记录";
                    return -4;
                }
                strID = strOutputResID;
            }
            strID = DbPath.GetID10(strID);

            // 返回资源路径
            if (StringUtil.IsInList("outputpath", strStyle) == true)
            {
                strOutputResID = DbPath.GetCompressedID(strID);
            }


            // 对帐户库开的后门，用于更新帐户
            if (bCheckAccount == true &&
                StringUtil.IsInList("account", this.TypeSafety) == true)
            {
                // 如果要获得记录正好是账户库记录，而且在
                // UserCollection中，那就把相关的User记录
                // 保存回数据库，以便稍后从数据库中提取，
                // 而不必从内存中提取。
                string strResPath = this.FullID + "/" + strID;

                // return:
                //		-1  出错
                //      -4  记录不存在
                //		0   成功
                nRet = this.container.UserColl.SaveUserIfNeed(
                    strResPath,
                    out strError);
                if (nRet <= -1)
                    return nRet;
            }


            //**********对数据库加读锁***************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                strID = DbPath.GetID10(strID);
                //********对记录加读锁*************
                m_recordLockColl.LockForRead(strID, m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加读锁。");
#endif
                try
                {
                    string strXmlFilePath = this.GetXmlFilePath(strID);

                    if (strXPath != null
                        && strXPath != "")
                    {
                        byte[] baWholeXml;
                        nRet = this.GetFileDbRecord(strXmlFilePath,
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

                        byte[] baPreamble = new byte[0];
                        string strXml = DatabaseUtil.ByteArrayToString(baWholeXml,
                            out baPreamble);

                        string strLocateXPath = "";
                        string strCreatePath = "";
                        string strNewRecordTemplate = "";
                        string strAction = "";
                        nRet = DatabaseUtil.ParseXPathParameter(strXPath,
                            out strLocateXPath,
                            out strCreatePath,
                            out strNewRecordTemplate,
                            out strAction,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        XmlDocument dom = new XmlDocument();
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
                        //string strOutputText = node.OuterXml;

                        byte[] baOutputText = DatabaseUtil.StringToByteArray(strOutputText,
                            baPreamble);

                        long lRealLength;
                        // return:
                        //		-1  出错
                        //		0   成功
                        nRet = ConvertUtil.GetRealLength(lStart,
                            nLength,
                            baOutputText.Length,
                            nMaxLength,
                            out lRealLength,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        destBuffer = new byte[lRealLength];

                        Array.Copy(baOutputText,
                            lStart,
                            destBuffer,
                            0,
                            lRealLength);

                        return 0;
                    }
                    else
                    {
                        return this.GetFileDbRecord(strXmlFilePath,
                            lStart,
                            nLength,
                            nMaxLength,
                            strStyle,
                            out destBuffer,
                            out strMetadata,
                            out outputTimestamp,
                            out strError);
                    }
                }
                finally
                {
                    //*********对记录解读锁************
                    m_recordLockColl.UnlockForRead(strID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解读锁。");
#endif
                }
            }
            finally
            {
                //***********对数据库解读锁************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetXml()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 按指定范围读资源
        // parameter:
        //		strID	拼好的资源ID,是否考虑给资源加一个res后缀
        //		nStart	开始位置
        //		nLength	长度 -1:开始到结束
        //		destBuffer	out参数，返回字节数组
        //		timestamp	out参数，返回时间戳
        //		strError	out参数，返回出错信息
        // return:
        //		-1  出错
        //		-4  记录不存在
        //		>=0 资源总长度
        public override long GetObject(string strRecordID,
            string strObjectID,
            string strXPath,
            long lStart,
            int nLength,
            int nMaxLength,
            string strStyle,
            out byte[] destBuffer,
            out string strMetadata,
            out byte[] timestamp,
            out string strError)
        {
            destBuffer = null;
            strMetadata = "";
            timestamp = null;
            strError = "";

            strRecordID = DbPath.GetID10(strRecordID);

            //**********对数据库加读锁***************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                //********对记录加读锁*************
                m_recordLockColl.LockForRead(strRecordID, m_nTimeOut);
#if DEBUG_LOCK			
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加读锁。");
#endif
                try
                {

                    string strObjectFilePath = this.m_strSourceFullPath + "\\" + strRecordID + "_" + strObjectID;

                    return this.GetFileDbRecord(strObjectFilePath,
                        lStart,
                        nLength,
                        nMaxLength,
                        strStyle,
                        out destBuffer,
                        out strMetadata,
                        out timestamp,
                        out strError);
                }
                finally
                {
                    //*********对记录解读锁************
                    m_recordLockColl.UnlockForRead(strRecordID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解读锁。");
#endif
                }
            }
            finally
            {
                //***********对数据库解读锁************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("GetObject()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }


        // 从文件中读取数据
        // 读配置文件与文件库都用到该函数
        // paramter
        //		strFilePath 文件路径
        //		nStart      起始位置
        //		nLength     长度
        //		nMaxLength  限制的最大长度
        //		strStyle    风格,有data才真正读数据,但length,metadata,时间戳,range缺省读
        //		destBuffer  out参数，返回的数据字节数组
        //		strMetadata out参数，返回的metadata内容
        //		outputTimestamp out参数，返回时间戳
        //		strError    out参数，返回出错信息
        // return:
        //		-1      出错
        //		>= 0    成功,返回实际文件的总长度
        // 线: 不安全
        private int GetFileDbRecord(string strFilePath,
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

            int nTotalLength = 0;
            FileInfo file = new FileInfo(strFilePath);
            // 2020/3/1
            file.Refresh();
            if (file.Exists == false)
            {
                strError = "文件'" + strFilePath + "'不存在";
                return -1;
            }

            // 1.取时间戳
            if (StringUtil.IsInList("timestamp", strStyle) == true)
            {
                string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strFilePath);
                if (File.Exists(strTimestampFileName) == true)
                {
                    string strOutputTimestamp = FileUtil.File2StringE(strTimestampFileName);
                    outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);
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
                long lOutputLength;
                // return:
                //		-1  出错
                //		0   成功
                int nRet = ConvertUtil.GetRealLength(lStart,
                    nLength,
                    nTotalLength,
                    nMaxLength,
                    out lOutputLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                using (FileStream s = new FileStream(strFilePath,
                    FileMode.Open))
                {
                    destBuffer = new byte[lOutputLength];
                    s.Seek(lStart, SeekOrigin.Begin);
                    s.Read(destBuffer,
                        0,
                        (int)lOutputLength);
                }
            }
            return nTotalLength;
        }

        // 写xml数据
        // parameter:
        //		strID	        记录ID -1:表示追加一条记录
        //		strRanges	    目标的位置,多个range用逗号分隔
        //		nTotalLength	总长度
        //		inputTimestamp	输入的时间戳
        //		outputTimestamp	out参数，返回的时间戳
        //		strOutputID	    out参数，返回的记录ID,当strID == -1时,得到实际的ID
        //		strError	    out参数，返回出错信息
        // return:
        // return:
        //		-1  出错
        //		-2  时间戳不匹配
        //      -4  记录不存在
        //      -6  权限不够
        //		0   成功
        // ??? AddInteger+,+AddInteger,Push好像没有实现
        public override int WriteXml(User oUser,
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

            if (strID == "?")
                strID = "-1";

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            // 确保ID,并且给返回值赋值
            bool bPushTailNo = false;
            bPushTailNo = this.EnsureID(ref strID, bSimulate);
            if (oUser != null)
            {
                string strTempRecordPath = this.GetCaption("zh-CN") + "/" + strID;
                if (bPushTailNo == true)
                {
                    string strExistRights = "";
                    bool bHasRight = oUser.HasRights(strTempRecordPath,
                        ResType.Record,
                        "create",
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
            int nRet;
            int nFull = -1;

            //***********对数据库加读锁***********
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                strID = DbPath.GetID10(strID);
                //**********对记录加写锁***************
                m_recordLockColl.LockForWrite(strID, m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                try
                {
                    string strXmlFilePath = this.GetXmlFilePath(strID);
                    bool bExist = File.Exists(strXmlFilePath);
                    if (bExist == false)
                    {
                        //创建新文件,并把辅助信息创建好
                        this.InsertRecord(strID,
                            strStyle,
                            inputTimestamp,
                            out inputTimestamp);
                        // 创建后存在一个字节，所有信息都有了
                    }

                    nRet = this.WriteFileDbTempRecord(strXmlFilePath,
                        strRanges,
                        lTotalLength,
                        baSource,
                        // streamSource,
                        strMetadata,
                        strStyle,
                        inputTimestamp,
                        out outputTimestamp,
                        out nFull,
                        out strError);
                    if (nRet <= -1)
                        return nRet;

                    if (nFull == 1)  // 文件已满
                    {
                        // 1.得到新旧检索点
                        string strNewFileName = DatabaseUtil.GetNewFileName(strXmlFilePath);
                        string strNewXml = FileUtil.File2StringE(strNewFileName);
                        string strOldXml = FileUtil.File2StringE(strXmlFilePath);

                        if (strXPath != null
                            && strXPath != "")
                        {
                            string strLocateXPath = "";
                            string strCreatePath = "";
                            string strNewRecordTemplate = "";
                            string strAction = "";
                            nRet = DatabaseUtil.ParseXPathParameter(strXPath,
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
                                strError = "WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时，装载旧记录到dom出错,原因:" + ex.Message;
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
                                strError = "WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时，XPath式子'" + strXPath + "'选择元素时出错,原因:" + ex.Message;
                                return -1;
                            }

                            if (node == null)
                            {
                                if (strLocateXPath == "")
                                {
                                    strError = "xpath表达式中的create参数不能为空值";
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
                                string strOldValue = node.InnerText.Trim(); // 2012/2/16
                                if (strAction == "AddInteger")
                                {

                                    int nNumber = 0;
                                    try
                                    {
                                        nNumber = Convert.ToInt32(strNewValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        strError = "传入的内容'" + strNewXml + "'不是数字格式。" + ex.Message;
                                        return -1;
                                    }

                                    string strLastValue;
                                    nRet = StringUtil.IncreaseNumber(strOldValue,
                                        nNumber,
                                        out strLastValue,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;

                                    newNode.InnerText = strLastValue;
                                    strOutputValue = newNode.OuterXml;
                                }
                                else if (strAction == "AppendString")
                                {
                                    newNode.InnerText = strOldValue + strNewValue;
                                    strOutputValue = newNode.OuterXml;
                                }
                            }

                            node.ParentNode.RemoveChild(node);

                            strNewXml = tempDom.OuterXml;
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

                        this.AddKeys(newKeys);
                        this.DeleteKeys(oldKeys);

                        // 3.处理子文件
                        nRet = this.ProcessFiles(strID,
                            newDom,
                            oldDom,
                            out strError);
                        if (nRet <= -1)
                            return nRet;

                        // 4.用newdata替换data
                        // 先把xml数据更新了，再更新检索点
                        if (strXPath != null
                            && strXPath != "")
                        {
                            FileUtil.String2File(strNewXml,
                                strXmlFilePath);
                        }
                        else
                        {
                            File.Copy(strNewFileName,
                                strXmlFilePath,
                                true);
                        }

                        // 5.删除newdata字段
                        File.Delete(strNewFileName);

                    }
                }
                catch (Exception ex)
                {
                    strError = "WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入记录'" + strID + "'时出错,原因:" + ex.Message;
                    return -1;
                }
                finally
                {
                    //*********对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif
                }
            }
            finally
            {
                //**********对数据库解读锁**************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("WriteXml()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            // 当本函数被明知为账户库的写操作调用时, 一定要用bCheckAccount==false
            // 来调用，否则容易引起不必要的递归
            if (nFull == 1
                && bCheckAccount == true
                && StringUtil.IsInList("account", this.TypeSafety) == true)
            {
                string strResPath = this.FullID + "/" + strID;
                this.container.UserColl.RefreshUserSafety(
                    strResPath);
            }
            return 0;
        }

        // 写对象
        //		strRecordID	记录ID
        //		strObjectID	资源ID
        //		strRanges	范围
        //		nTotalLength	总长度
        //		sourceBuffer	源数据
        //		strMetadata	元数据
        //		strStyle	样式
        //		inputTimestamp	输入的时间戳
        //		outputTimestamp	out参数，返回的时间戳
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
            // Stream streamSource,
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
                string strTempRecordPath = this.GetCaption("zh-CN") + "/" + strRecordID;
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
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
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
#if DEBUG_LOCK
				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录加写锁。");
#endif
                try
                {
                    //////////////////////////////////////////////
                    // 1.在对应的xml数据，用对象路径找到对象ID
                    ///////////////////////////////////////////////
                    string strXmlFilePath = this.GetXmlFilePath(strRecordID);

                    XmlDocument xmlDom = new XmlDocument();
                    xmlDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                    xmlDom.Load(strXmlFilePath);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDom.NameTable);
                    nsmgr.AddNamespace("dprms", DpNs.dprms);
                    XmlNode fileNode = xmlDom.DocumentElement.SelectSingleNode("//dprms:file[@id='" + strObjectID + "']", nsmgr);
                    if (fileNode == null)
                    {
                        strError = "在数据xml里没有找到该ID对应的dprms:file节点";
                        return -1;
                    }

                    string strObjectFilePath = this.GetObjectFileName(strRecordID,
                        strObjectID);
                    if (File.Exists(strObjectFilePath) == false)
                    {
                        strError = "服务器错误:资源记录'" + strObjectFilePath + "'不存在,不可能的情况";
                        return -1;
                    }
                    string strNewObjectFileName = DatabaseUtil.GetNewFileName(strObjectFilePath);
                    if (File.Exists(strNewObjectFileName) == false)
                    {
                        this.UpdateObject(strObjectFilePath,
                            strStyle,
                            inputTimestamp,
                            out inputTimestamp);
                        // Updata后,记录临时文件有一个字节,所有信息都存在了.
                    }

                    int nFull;
                    nRet = this.WriteFileDbTempRecord(strObjectFilePath,
                        strRanges,
                        lTotalLength,
                        baSource,
                        // streamSource,
                        strMetadata,
                        strStyle,
                        inputTimestamp,
                        out outputTimestamp,
                        out nFull,
                        out strError);
                    if (nRet <= -1)
                        return nRet;

                    if (nFull == 1)  //覆盖完了
                    {
                        // 1. 替换data字段
                        File.Copy(strNewObjectFileName,
                            strObjectFilePath,
                            true);

                        // 2. 删除newdata字段
                        File.Delete(strNewObjectFileName);

                    }
                }
                catch (Exception ex)
                {
                    strError = "WriteXml() 在给'" + this.GetCaption("zh-CN") + "'库写入资源'" + strRecordID + "_" + strObjectID + "'时出错,原因:" + ex.Message;
                    return -1;
                }
                finally
                {
                    //********对记录解写锁****************************
                    m_recordLockColl.UnlockForWrite(strRecordID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "/" + strRecordID + "'记录解写锁。");
#endif
                }
            }
            finally
            {
                //*******对数据库解读锁****************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK

				this.container.WriteDebugInfo("WriteObject()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }

            return 0;
        }



        // 检索关于长度与范围方面的输入参数是否合法
        // parameters:
        //		lTotalLength	资源总长度
        //		strRanges		范围，将被规范化
        //		lSourceLength	本次传来数据的长度
        //		strError		out参数，返回出错信息
        // reutrn:
        //		-1	出错
        //		-2	目前还不清楚资源的状态
        //		1	资源将被置空
        //		0	资源将不做任何修改
        public int CheckParamsAboutLengthAndRanges(long lTotalLength,
            ref string strRanges,
            long lSourceLength,
            out string strError)
        {
            strError = "";

            if (strRanges == null)
                strRanges = "";

            if (lTotalLength < 0)
            {
                strError = "CheckParamsAboutLengthAndRanges()调用错误，lTotalLength参数不能小于0。";
                return -1;
            }
            if (lSourceLength < 0)
            {
                strError = "CheckParamsAboutLengthAndRanges()调用错误，lSourceLength参数不能小于0。";
                return -1;
            }



            if (lTotalLength == 0)
            {
                if (strRanges != "")
                {
                    strError = "CheckParamsAboutLengthAndRanges()调用错误，当lTotalLength == 0时，strRanges参数只能为null或空字符串。";
                    return -1;
                }
                if (lSourceLength != 0)
                {
                    strError = "CheckParamsAboutLengthAndRanges()调用错误，当lTotalLength == 0时，lSourceLength参数只能为0。";
                    return -1;
                }

                // 资源将被置空。
                return 1;
            }

            if (lTotalLength > 0)
            {
                if (lSourceLength == 0)
                {
                    if (strRanges != "")
                    {
                        strError = "CheckParamsAboutLengthAndRanges()调用错误，当lTotalLength == 0时 且lSourceLength == 0，那么strRanges参数只能为null或空字符串。";
                        return -1;
                    }
                    else
                    {
                        //则对原资源不做任何修改，返回0
                        return 0;
                    }
                }
                else
                {
                    if (strRanges == "")
                        strRanges = "0-" + Convert.ToString(lSourceLength - 1);

                    return -2;
                }
            }

            return -2;
        }

        // 当文件库中的记录满时，删除辅助的文件。
        public void DeleteFuZhuFilesWhenFull(string strFilePath)
        {
            string strNewFilePath = DatabaseUtil.GetNewFileName(strFilePath);
            if (File.Exists(strNewFilePath) == true)
                File.Delete(strNewFilePath);

            string strRangeFilePath = DatabaseUtil.GetRangeFileName(strFilePath);
            if (File.Exists(strRangeFilePath) == true)
                File.Delete(strRangeFilePath);
        }


        // 写文件库的临时记录文件和记录信息，可能是xml记录体，也可能是对象资源文件
        // parameters:
        //		strFilePath	记录文件物理路径
        //		strRanges	目标范围
        //		nTotalLength	资源总长度
        //		baSource	内容字节数组
        //		streamSource	内容流
        //		strStyle	风格
        //		baInputTimestamp	输入的时间戳
        //		baOutputTimestamp	out参数，返回的时间戳
        //		bFull	out参数，返回记录是否已满
        //		strError	out参数，返回出错信息
        // return:
        //		<=-1	出错
        //		-2	时间戳不匹配
        //		0	成功
        // 线: 不安全
        //记住对于写数据库的记录，都是先写临时字段，当满时，再替换到实际的字段
        private int WriteFileDbTempRecord(string strFilePath,
            string strRanges,
            long lTotalLength,
            byte[] baSource,
            // Stream streamSource,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out byte[] baOutputTimestamp,
            out int nFull,
            out string strError)
        {
            nFull = -1;
            baOutputTimestamp = null;
            strError = "";

            // --------------------------------------------------
            // 例如检查一下输入参数
            // --------------------------------------------------
            if (strFilePath == null || strFilePath == "")
            {
                strError = "WriteFileDbRecord()调用错误，strFilePath参数不能为null或空字符串。";
                return -1;
            }
            /*
            if (baSource == null && streamSource == null)
            {
                strError = "WriteFileDbRecord()调用错误，baSource参数与streamSource参数不能同时为null。";
                return -1;
            }
            if (baSource != null && streamSource != null)
            {
                strError = "WriteFileDbRecord()调用错误，baSource参数与streamSource参数只能有一个被赋值。";
                return -1;
            }
             * */
            if (baSource == null)
            {
                strError = "WriteFileDbRecord()调用错误，baSource参数不能为null。";
                return -1;
            }

            if (lTotalLength < 0)
            {
                strError = "WriteFileDbRecord()调用错误，nTotalLength参数必须大于等于0。";
                return -1;
            }


            // --------------------------------------------------
            // 开始做事情
            // --------------------------------------------------

            if (File.Exists(strFilePath) == false)
            {
                strError = "文件库'" + this.GetCaption("zh-CN") + "'的记录文件'" + strFilePath + "'不存在，不可能的情况。";
                return -1;
            }
            string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strFilePath);
            if (File.Exists(strTimestampFileName) == false)
            {
                strError = "文件库'" + this.GetCaption("zh-CN") + "'的记录文件'" + strFilePath + "'对应的时间戳文件'" + strTimestampFileName + "'不存在，不可能的情况。";
                return -1;
            }

            if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
            {
                string strOldTimestamp = FileUtil.File2StringE(strTimestampFileName);
                baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOldTimestamp);
                if (ByteArray.Compare(baOutputTimestamp, baInputTimestamp) != 0)
                {
                    strError = "时间戳不匹配";
                    return -2;
                }
            }



            // 写数据

            int nRet = 0;

            // 当前资源的尺寸
            //	-1	表示未知
            //	-2	表示不变
            long lCurrentLength = 0;

            string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);
            string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);


            long lSourceTotalLength = 0;


            lSourceTotalLength = baSource.Length;
            /*
             if (baSource != null)
                 lSourceTotalLength = baSource.Length;
             else
                 lSourceTotalLength = streamSource.Length;
              * */

            // reutrn:
            //		-1	出错
            //		-2	目前还不清楚资源的状态
            //		1	资源将被置空
            //		0	资源将不做任何修改
            nRet = this.CheckParamsAboutLengthAndRanges(lTotalLength,
                ref strRanges,
                lSourceTotalLength,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                // 资源将被置空
                using (Stream s = File.Create(strNewFileName))
                {

                }

                nFull = 1;
                lCurrentLength = 0;

                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);

                goto END1;
            }
            if (nRet == 0)
            {
                nFull = -1;
                lCurrentLength = -2;
                goto END1;
            }

            RangeList rangeList = new RangeList(strRanges);

            using (Stream target = File.Open(strNewFileName,
                FileMode.OpenOrCreate))
            {
                int nStartOfSource = 0;
                for (int i = 0; i < rangeList.Count; i++)
                {
                    RangeItem range = (RangeItem)rangeList[i];
                    int nStartOfTarget = (int)range.lStart;
                    int nLength = (int)range.lLength;

                    // 移动目标流的指针到指定位置
                    target.Seek(nStartOfTarget, SeekOrigin.Begin);
                    /*
                    if (baSource != null)
                     * */
                    {
                        target.Write(baSource,
                            nStartOfSource,
                            nLength);

                        nStartOfSource += nLength; //2005.11.14 add
                    }
                    /*
                    else
                    {
                        StreamUtil.DumpStream(streamSource,
                            target,
                            nLength);
                    }
                     * */
                }
            }

            string strOldRanges = "";
            if (File.Exists(strRangeFileName) == true)
                strOldRanges = FileUtil.File2StringE(strRangeFileName);

            string strResultRanges = "";
            int nState = RangeList.MergeContentRangeString(strRanges,
                strOldRanges,
                lTotalLength,
                out strResultRanges,
                out strError);
            if (nState == -1)
            {
                strError = "MergContentRangeString() error 3 : " + strError + " (strRanges='" + strRanges + "' strOldRanges='" + strOldRanges + "' ) lTotalLength=" + lTotalLength.ToString() + "";
                return -1;
            }
            if (nState == 1)
            {
                nFull = 1;
                lCurrentLength = lTotalLength;

                using (Stream s = File.Open(strNewFileName, FileMode.Open))
                {
                    s.SetLength(lTotalLength);
                }

                if (File.Exists(strRangeFileName) == true)
                    File.Delete(strRangeFileName);
            }
            else
            {
                nFull = 0;
                lCurrentLength = -1;  //当前尺寸未知。还是可以知道的

                FileUtil.String2File(strResultRanges,
                    strRangeFileName);
            }


        END1:

            // 写metadata
            if (strMetadata == null || strMetadata == "")
                strMetadata = "<file/>";

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
            nRet = DatabaseUtil.MergeMetadata(strOldMetadata,
                strMetadata,
                lCurrentLength,
                "",
                out strResultMetadata,
                out strError);
            if (nRet == -1)
                return -1;

            // 把合并的新数据写到文件里
            FileUtil.String2File(strResultMetadata,
                strMetadataFileName);

            // 2013/11/23
            // 是否要直接利用输入的时间戳
            bool bForceTimestamp = StringUtil.IsInList("forcesettimestamp", strStyle);

            string strOutputTimestamp = "";
            if (bForceTimestamp == true)
                strOutputTimestamp = ByteArray.GetHexTimeStampString(baInputTimestamp);
            else
                strOutputTimestamp = this.CreateTimestampForDb();


            FileUtil.String2File(strOutputTimestamp,
                strTimestampFileName);
            baOutputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);


            return 0;
        }



        // 删除存放文件信息的辅助文件
        // 线: 不安全
        public void DeleteFuZhuFiles(string strFilePath)
        {
            // 1. 删除range字段文件
            string strRangeFileName = DatabaseUtil.GetRangeFileName(strFilePath);
            if (File.Exists(strRangeFileName) == true)
                File.Delete(strRangeFileName);

            // 2. 删除metadata字段文件
            string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strFilePath);
            if (File.Exists(strMetadataFileName) == true)
                File.Delete(strMetadataFileName);

            // 3. 删除timestamp字段文件
            string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strFilePath);
            if (File.Exists(strTimestampFileName) == true)
                File.Delete(strTimestampFileName);

            // 4. 删除临时数据文件
            string strNewFileName = DatabaseUtil.GetNewFileName(strFilePath);
            if (File.Exists(strNewFileName) == true)
                File.Delete(strNewFileName);

        }

        public void InsertRecord(string strRecordID,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp)
        {
            outputTimestamp = null;
            string strXmlFilePath = this.GetXmlFilePath(strRecordID);

            using (Stream file = File.Create(strXmlFilePath))
            {

            }

            // new字段
            string strNewFileName = DatabaseUtil.GetNewFileName(strXmlFilePath);
            using (Stream s = File.Create(strNewFileName))
            {
                s.Write(new byte[] { 0x0 },
                    0,
                    1);
            }

            // timeatamp
            string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strXmlFilePath);

            // 2013/11/23
            // 是否要直接利用输入的时间戳
            bool bForceTimestamp = StringUtil.IsInList("forcesettimestamp", strStyle);

            // 生成新的时间戳,保存到数据库里
            string strTimestamp = "";
            if (bForceTimestamp == true)
                strTimestamp = ByteArray.GetHexTimeStampString(inputTimestamp);
            else
                strTimestamp = this.CreateTimestampForDb();

            FileUtil.String2File(strTimestamp, strTimestampFileName);
            outputTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);

            // range
            string strRangeFileName = DatabaseUtil.GetRangeFileName(strXmlFilePath);
            FileUtil.String2File("0-0", strRangeFileName);

            // metadata
            string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strXmlFilePath);
            FileUtil.String2File("<file size='0'/>", strMetadataFileName);
        }

        public void UpdateObject(string strObjectPath,
            string strStyle,
            byte[] inputTimestamp,
            out byte[] outputTimestamp)
        {
            outputTimestamp = null;

            // new字段
            string strNewFileName = DatabaseUtil.GetNewFileName(strObjectPath);
            using (Stream s = File.Create(strNewFileName))
            {
                s.Write(new byte[] { 0x0 },
                    0,
                    1);
            }

            // timeatamp
            string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strObjectPath);

            // 2013/11/23
            // 是否要直接利用输入的时间戳
            bool bForceTimestamp = StringUtil.IsInList("forcesettimestamp", strStyle);

            // 生成新的时间戳,保存到数据库里
            string strTimestamp = "";
            if (bForceTimestamp == true)
                strTimestamp = ByteArray.GetHexTimeStampString(inputTimestamp);
            else
                strTimestamp = this.CreateTimestampForDb();

            FileUtil.String2File(strTimestamp, strTimestampFileName);
            outputTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);

            // range
            string strRangeFileName = DatabaseUtil.GetRangeFileName(strObjectPath);
            FileUtil.String2File("0-0", strRangeFileName);

            // metadata
            string strMetadataFileName = DatabaseUtil.GetMetadataFileName(strObjectPath);
            FileUtil.String2File("<file size='0'/>", strMetadataFileName);
        }

        // 得到资源名称
        public string GetObjectFileName(string strRecordID,
            string strObjectID)
        {
            return this.m_strSourceFullPath + "\\"
                + strRecordID
                + "_" + strObjectID;
        }

        // 得到记录ID对应的文件名
        // parameters:
        //      strRecordID 记录号
        public string GetXmlFilePath(string strRecordID)
        {
            return this.m_strSourceFullPath + "\\"
                + this.RecordID2XmlFileName(strRecordID);
        }

        // 增加新检索点
        public void AddKeys(KeyCollection keys)
        {
            foreach (KeyItem oneKey in keys)
            {
                string strTablePath;
                strTablePath = this.TableName2TableFileName(oneKey.SqlTableName);
                XmlDocument domTable = new XmlDocument();
                domTable.PreserveWhitespace = true; //设PreserveWhitespace为true

                domTable.Load(strTablePath);

                //新建key节点
                XmlNode nodeKey = domTable.CreateElement("key");

                XmlNode nodeKeystring = domTable.CreateElement("keystring");
                nodeKeystring.InnerText = oneKey.Key;   // 2012/2/16
                nodeKey.AppendChild(nodeKeystring);

                XmlNode nodeFromstring = domTable.CreateElement("fromstring");
                nodeFromstring.InnerText = oneKey.FromValue;   // 2012/2/16
                nodeKey.AppendChild(nodeFromstring);

                XmlNode nodeIdstring = domTable.CreateElement("idstring");
                nodeIdstring.InnerText = oneKey.RecordID;   // 2012/2/16
                nodeKey.AppendChild(nodeIdstring);

                XmlNode nodeKeystringnum = domTable.CreateElement("keystringnum");
                nodeKeystringnum.InnerText = oneKey.Num;   // 2012/2/16
                nodeKey.AppendChild(nodeKeystringnum);

                domTable.DocumentElement.AppendChild(nodeKey);
                domTable.Save(strTablePath);
            }
        }

        // 删除旧检索点
        public void DeleteKeys(KeyCollection keys)
        {
            foreach (KeyItem oneKey in keys)
            {
                string strTablePath;
                strTablePath = this.TableName2TableFileName(oneKey.SqlTableName);
                XmlDocument domTable = new XmlDocument();
                domTable.PreserveWhitespace = true; //设PreserveWhitespace为true

                domTable.Load(strTablePath);

                string strXpath = "/root/key[keystring='" + oneKey.Key + "' and fromstring='" + oneKey.FromValue + "' and idstring='" + oneKey.RecordID + "']";
                XmlNode nodeKey = domTable.SelectSingleNode(strXpath);
                if (nodeKey != null)
                {
                    domTable.DocumentElement.RemoveChild(nodeKey);
                }
                else
                {
                    throw (new Exception("根据xpath'" + strXpath + "'没找到节点,不可能的情况!"));
                }
                domTable.Save(strTablePath);
            }
        }

        // 处理子文件
        public int ProcessFiles(string strRecordID,
            XmlDocument newDom,
            XmlDocument oldDom,
            out string strError)
        {
            strError = "";

            // 处理子文件
            List<string> new_fileids = new List<string>();
            if (newDom != null)
            {
                XmlNamespaceManager newNsmgr = new XmlNamespaceManager(newDom.NameTable);
                newNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList newFileList = newDom.SelectNodes("//dprms:file", newNsmgr);
                foreach (XmlNode newFileNode in newFileList)
                {
                    string strNewFileID = DomUtil.GetAttr(newFileNode,
                        "id");
                    if (string.IsNullOrEmpty(strNewFileID) == false)
                        new_fileids.Add(strNewFileID);
                }
            }

            List<string> old_fileids = new List<string>();
            if (oldDom != null)
            {
                XmlNamespaceManager oldNsmgr = new XmlNamespaceManager(oldDom.NameTable);
                oldNsmgr.AddNamespace("dprms", DpNs.dprms);
                XmlNodeList oldFileList = oldDom.SelectNodes("//dprms:file", oldNsmgr);
                foreach (XmlNode oldFileNode in oldFileList)
                {
                    string strOldFileID = DomUtil.GetAttr(oldFileNode,
                        "id");
                    if (string.IsNullOrEmpty(strOldFileID) == false)
                        old_fileids.Add(strOldFileID);
                }
            }

            if (new_fileids.Count == 0 && old_fileids.Count == 0)
                return 0;

            //数据必须先排序
            new_fileids.Sort();
            old_fileids.Sort();

            List<string> targetLeft = new List<string>();
            List<string> targetMiddle = null;   //  new List<string>();
            List<string> targetRight = new List<string>();

            //新旧两个File数组碰
            StringUtil.MergeStringList(new_fileids,
                old_fileids,
                ref targetLeft,
                ref targetMiddle,
                ref targetRight);

            //删除多余的旧文件
            if (targetRight.Count > 0)
            {
                foreach (string strNeedDeleteFileID in targetRight)
                {
                    string strFilePath = this.GetObjectFileName(strRecordID,
                        strNeedDeleteFileID);
                    File.Delete(strFilePath);
                    this.DeleteFuZhuFiles(strFilePath);
                }
            }

            // 创建新文件文件
            if (targetLeft.Count > 0)
            {
                foreach (string strNewFileID in targetLeft)
                {
                    string strObjectPath = this.GetObjectFileName(strRecordID,
                        strNewFileID);
                    int nRet = this.InsertEmptyObject(strObjectPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
            return 0;
        }



        // 给表中插入一空资源对象
        // return
        //		-1  出错
        //		0   成功
        private int InsertEmptyObject(string strObjectPath,
            out string strError)
        {
            strError = "";
            using (Stream s = File.Create(strObjectPath))
            {
            }
            return 0;
        }

        // 检索点表名到表文件名
        public string TableName2TableFileName(string strKeyName)
        {
            strKeyName = strKeyName.Trim();
            return this.m_strSourceFullPath + "\\" + strKeyName + ".xml";
        }
        // 根据记录ID得到xml文件名
        // 如果记录号不足10,会将记录号变成10位
        // parameter:
        //		strID   记录ID
        // return:
        //		xml文件名
        private string RecordID2XmlFileName(string strID)
        {
            strID = strID.Trim();
            strID = DbPath.GetID10(strID);
            return strID + ".xml";
        }

        // 从Xml文件名 到 记录ID
        // parameter:
        //		strFileName 文件名
        // return:
        //		记录ID,不足10变成10数
        private string XmlFileName2RecordID(string strFileName)
        {
            string strEx = Path.GetExtension(strFileName);
            if (strEx.Length > 1)
                strEx = strEx.Substring(1);
            strEx = strEx.ToUpper();
            if (strEx != "XML")
            {
                throw (new Exception("该文件不是记录类型"));
            }

            int nPosition = strFileName.LastIndexOf(".");
            string strRecordID = "";
            if (nPosition >= 0)
                strRecordID = strFileName.Substring(0, nPosition);
            else
                strRecordID = strFileName;
            strRecordID = DbPath.GetID10(strRecordID);
            return strRecordID;
        }


        // 普通删除记录
        // paramter:
        //		strID       记录ID
        //		strError    out参数，返回出错信息
        // return:
        //		-1  一般性错误
        //		-2  时间戳不匹配
        //      -4  未找到记录
        //		0   成功
        //线: 安全的
        //为什么要加写锁：因为是删除记录以及相应的检索点，在这个时期，该记录即不能读也不写，所以加写锁
        public override int DeleteRecord(
            string strID,
            string strObjectID,
            byte[] inputTimestamp,
            string strStyle,
            out byte[] outputTimestamp,
            out string strError)
        {
            outputTimestamp = null;
            strError = "";

            strID = DbPath.GetID10(strID);

            int nRet;

            //*********对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                //***********加记录写锁**********
                m_recordLockColl.LockForWrite(strID, m_nTimeOut);
#if DEBUG_LOCK
				this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录加写锁。");
#endif
                try
                {
                    strID = DbPath.GetID10(strID);

                    string strXmlFilePath = this.GetXmlFilePath(strID);

                    //比较时间戳
                    //outputTimestamp = this.GetTimestampByFile(strXmlFilePath);
                    string strTimestampFileName = DatabaseUtil.GetTimestampFileName(strXmlFilePath);
                    if (File.Exists(strTimestampFileName) == true)
                    {
                        string strOutputTimestamp = FileUtil.File2StringE(strTimestampFileName);
                        outputTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);
                    }
                    else
                    {
                        strError = "不可能没有时间戳文件";
                        return -1;
                    }
                    // 2024/5/8 增加 ignorechecktimestamp 判断
                    if (StringUtil.IsInList("ignorechecktimestamp", strStyle) == false)
                    {
                        if (ByteArray.Compare(inputTimestamp,
                        outputTimestamp) != 0)
                        {
                            strError = "时间戳不匹配";
                            return -2;
                        }
                    }
                    bool bLoadXmlSuccessed = true;
                    XmlDocument oldDataDom = new XmlDocument();
                    oldDataDom.PreserveWhitespace = true; //设PreserveWhitespace为true

                    try
                    {
                        oldDataDom.Load(strXmlFilePath);
                    }
                    catch
                    {
                        bLoadXmlSuccessed = false;
                    }

                    // 1.删除子文件
                    if (bLoadXmlSuccessed == true)
                    {
                        nRet = this.ProcessFiles(strID,
                            null,
                            oldDataDom,
                            out strError);
                        if (nRet <= -1)
                            return nRet;
                    }
                    else
                    {
                        this.ForceDeleteFiles(strID);
                    }

                    // 2.删除检索点
                    if (bLoadXmlSuccessed == true)
                    {

                        KeysCfg keysCfg = null;
                        nRet = this.GetKeysCfg(out keysCfg,
                            out strError);

                        if (nRet == -1)
                            return -1;

                        if (keysCfg != null)
                        {

                            //生成检索点集合
                            KeyCollection oldKeys = null;
                            nRet = keysCfg.BuildKeys(oldDataDom,
                                strID,
                                "zh",
                                // "",//strStyle
                                this.KeySize,
                                out oldKeys,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            //oldKeys.Sort();
                            oldKeys.RemoveDup();

                            this.DeleteKeys(oldKeys);
                        }
                    }
                    else
                    {

                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = this.ForceDeleteKeys(strID,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    // 3.删除本记录
                    this.DeleteRecordByID(strXmlFilePath);

                    // 4.比Sql库多,删除表示字段信息文件
                    this.DeleteFuZhuFiles(strXmlFilePath);
                }
                finally
                {
                    //***********对记录解写锁**************
                    m_recordLockColl.UnlockForWrite(strID);
#if DEBUG_LOCK
					this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "/" + strID + "'记录解写锁。");
#endif
                }
            }
            finally
            {
                //**************对数据库解读锁*************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("DeleteRecord()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
            return 0;
        }

        // 根据记录号之间的关系,强制删除文件
        public void ForceDeleteFiles(string strRecordID)
        {
            DirectoryInfo dir = new DirectoryInfo(this.m_strSourceFullPath);
            FileInfo[] files = dir.GetFiles(strRecordID + "_*.*");
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i].FullName);
            }
        }

        // 根据删除一个记录对应的检索点,检查所有的表
        // 线:不安全
        // return:
        //      -1  出错
        //      0   成功
        public int ForceDeleteKeys(string strRecordID,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                 out strError);
            if (nRet == -1)
                return -1;

            if (keysCfg == null)
                return 0;

            List<TableInfo> aTableInfo = null;
            nRet = keysCfg.GetTableInfosRemoveDup(out aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;


            for (int i = 0; i < aTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTableInfo[i];

                string strTablePath = this.TableName2TableFileName(tableInfo.SqlTableName);

                XmlDocument domTable = new XmlDocument();
                domTable.PreserveWhitespace = true; //设PreserveWhitespace为true

                domTable.Load(strTablePath);

                string strXpath = "/root/key[idstring='" + strRecordID + "']";

                XmlNodeList listKey = domTable.SelectNodes(strXpath);
                foreach (XmlNode nodeKey in listKey)
                {
                    domTable.DocumentElement.RemoveChild(nodeKey);
                }
                domTable.Save(strTablePath);
            }
            return 0;
        }

        // 根据ID从库中删除一个记录项,可以是记录也可以是资源
        public void DeleteRecordByID(string strFilePath)
        {
            File.Delete(strFilePath);
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
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
            DpResultSet resultSet,
            out string strError)
        {
            strError = "";
            // 从库目录里得到所有似记录文件
            string[] files = Directory.GetFiles(this.m_strSourceFullPath, "??????????.xml");
            ArrayList records = new ArrayList();
            foreach (string fileName in files)
            {
                FileInfo fileInfo = new FileInfo(fileName);
                // 2020/3/1
                fileInfo.Refresh();
                string strFileName = fileInfo.Name;
                if (this.IsRecord(strFileName) == false)
                    continue;
                records.Add(this.XmlFileName2RecordID(strFileName));
            }

            //前方一致
            if (searchItem.Match == "left"
                || searchItem.Match == "")
            {
                foreach (string recordID in records)
                {
                    if (recordID.Length < searchItem.Word.Length)
                        continue;

                    string strFirstPart = recordID.Substring(0,
                        searchItem.Word.Length);

                    if (strFirstPart == searchItem.Word)
                    {
                        string strRecPath = this.FullID + "/" + recordID;
                        resultSet.Add(new DpRecord(strRecPath));
                    }
                }
            }
            else if (searchItem.Match == "exact")
            {
                // 从检索词时分析出来关系
                if (searchItem.Relation == "draw"
                    || searchItem.Relation == "range")
                {
                    foreach (string recordID in records)
                    {
                        string strStartID;
                        string strEndID;
                        bool bRet = StringUtil.SplitRangeEx(searchItem.Word,
                            out strStartID,
                            out strEndID);
                        if (bRet == true)
                        {
                            strStartID = DbPath.GetID10(strStartID);
                            strEndID = DbPath.GetID10(strEndID);

                            if (String.Compare(recordID, strStartID, true) >= 0
                                && String.Compare(recordID, strEndID, true) <= 0)
                            {
                                string strRecPath = this.FullID + "/" + recordID;
                                resultSet.Add(new DpRecord(strRecPath));
                                continue;
                            }
                        }
                        else
                        {
                            string strOperator;
                            string strCanKaoID;
                            StringUtil.GetPartCondition(searchItem.Word,
                                out strOperator,
                                out strCanKaoID);

                            strCanKaoID = DbPath.GetID10(strCanKaoID);
                            if (StringUtil.CompareByOperator(recordID,
                                strOperator,
                                strCanKaoID) == true)
                            {
                                string strRecPath = this.FullID + "/" + recordID;
                                resultSet.Add(new DpRecord(strRecPath));
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    foreach (string recordID in records)
                    {
                        searchItem.Word = DbPath.GetID10(searchItem.Word);
                        if (StringUtil.CompareByOperator(recordID,
                            searchItem.Relation,
                            searchItem.Word) == true)
                        {
                            string strRecPath = this.FullID + "/" + recordID;
                            resultSet.Add(new DpRecord(strRecPath));
                            continue;
                        }
                    }
                }
            }
            return 0;
        }

        // 得到关于key的条件
        // parameter:
        //		searchItem      检索信息对象
        //		nodeCovertQuery 处理检索词的配置节点
        // 可能会抛出的异常:NoMatchException(检索方式与数据类型)
        private int GetKeyCondition(SearchItem searchItem,
            XmlNode nodeConvertQueryString,
            XmlNode nodeConvertQueryNumber,
            out string strKeyCondition,
            out string strError)
        {
            strError = "";
            strKeyCondition = "";

            QueryUtil.VerifyRelation(ref searchItem.Match,//strMatch,
                ref searchItem.Relation,//strRelation,
                ref searchItem.DataType); //strDataType);

            int nRet;
            KeysCfg keysCfg = null;
            nRet = this.GetKeysCfg(out keysCfg,
                out strError);
            if (nRet == -1)
                return -1;

            string strKeyValue = searchItem.Word.Trim();
            if (searchItem.DataType == "string")
            {
                if (nodeConvertQueryString != null
                    && keysCfg != null)
                {
                    List<string> keys = null;
                    nRet = keysCfg.ConvertKeyWithStringNode(
                        null, //dataDom
                        strKeyValue,
                        nodeConvertQueryString,
                        out keys,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (keys.Count != 1)
                    {
                        // strError = "检索词配置不合法，不应变成多个。";
                        strError = "不支持把检索词通过'split'样式加工成多个.";
                        return -1;
                    }
                    strKeyValue = keys[0];
                }
            }
            else if (searchItem.DataType == "number")
            {
                if (nodeConvertQueryNumber != null
                    && keysCfg != null)
                {
                    string strMyKey;
                    nRet = keysCfg.ConvertKeyWithNumberNode(
                        null,
                        strKeyValue,
                        nodeConvertQueryNumber,
                        out strMyKey,
                        out strError);
                    if (nRet == -1 || nRet == 1)
                        return -1;
                    strKeyValue = strMyKey;
                }
            }
            strKeyValue = strKeyValue.Trim();


            // 4. 如果strMatch为空，则按"左方一致"
            if (searchItem.Match == "left"
                || searchItem.Match == "")
            {
                //判断选择的数据类型
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式为left时或为空时，数据类型不匹配，应该为string");
                    throw (ex);
                }
                //这句是保险的，因为上面已抛出异常
                int nLength = searchItem.Word.Trim().Length;
                strKeyCondition = " (substring(keystring,1," + Convert.ToString(nLength) + ")='" + strKeyValue + "') ";
            }

            // TODO: 为什么没有支持中间一致？2007/10/9
            if (searchItem.Match == "middle")
            {
                strError = "对于文件方式的数据库 '" + this.GetCaption("zh") + "'，不支持中间一致(middle)匹配方式";
                return -1;
            }

            //右方一致
            if (searchItem.Match == "right")
            {
                if (searchItem.DataType != "string")
                {
                    NoMatchException ex = new NoMatchException("在匹配方式为right时，数据类型不匹配，应该为string");
                    throw (ex);
                }
                //注意这里要改成右方一致
                int nLength = searchItem.Word.Trim().Length;
                strKeyCondition = " (substring(keystring,1," + Convert.ToString(nLength) + ")='" + strKeyValue + "') ";
            }
            //精确一致
            if (searchItem.Match == "exact")
            {
                //从词中汲取,较复杂，注意
                if (searchItem.Relation == "draw"
                    || searchItem.Relation == "range")
                {

                    string strStartText;
                    string strEndText;
                    bool bRet = StringUtil.SplitRangeEx(searchItem.Word,
                        out strStartText,
                        out strEndText);
                    //先按"-"算
                    if (bRet == true)
                    {


                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                // 加工首
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(null,//dataDom
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
                                nRet = keysCfg.ConvertKeyWithStringNode(null,//dataDom
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
                            strKeyCondition = " keystring >= '"
                                + strStartText
                                + "' and keystring<= '"
                                + strEndText + "'";
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                // 首
                                string strMyKey;
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strStartText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strStartText = strMyKey;

                                // 尾
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strEndText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strEndText = strMyKey;

                            }
                            strKeyCondition = " keystringnum >= "
                                + strStartText
                                + " and keystringnum<= "
                                + strEndText + "";
                        }
                    }
                    else  // 再算 比较符号
                    {
                        string strOperator;
                        string strRealText;
                        StringUtil.GetPartCondition(searchItem.Word,
                            out strOperator,
                            out strRealText);

                        //SQL与Xpath比较运算符的差别
                        if (strOperator == "<>")
                            strOperator = "!=";
                        if (searchItem.DataType == "string")
                        {
                            if (nodeConvertQueryString != null
                                && keysCfg != null)
                            {
                                List<string> keys = null;
                                nRet = keysCfg.ConvertKeyWithStringNode(null,//dataDom
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

                            strKeyCondition = "keystring" +
                                strOperator +
                                "'" + strRealText + "'";
                        }
                        else if (searchItem.DataType == "number")
                        {
                            if (nodeConvertQueryNumber != null
                                && keysCfg != null)
                            {
                                string strMyKey;
                                nRet = keysCfg.ConvertKeyWithNumberNode(
                                    null,
                                    strRealText,
                                    nodeConvertQueryNumber,
                                    out strMyKey,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                    return -1;
                                strRealText = strMyKey;

                            }
                            strKeyCondition = "keystringnum" +
                                strOperator +
                                strRealText + " and keystringnum!=-1";
                        }
                    }
                }
                else
                {
                    // 当关系操作符为空为，按等于算
                    if (searchItem.Relation == "")
                        searchItem.Relation = "=";
                    if (searchItem.Relation == "<>")
                        searchItem.Relation = "!=";

                    if (searchItem.DataType == "string")
                    {
                        strKeyCondition = " keystring "
                            + searchItem.Relation
                            + "'" + strKeyValue + "'";
                    }
                    else if (searchItem.DataType == "number")
                    {
                        strKeyCondition = "keystringnum"
                            + searchItem.Relation
                            + "" + strKeyValue + "";
                    }
                }
            }
            return 0;
        }

        // 检索
        internal override int SearchByUnion(
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

            //************对数据库加读锁**************
            m_db_lock.AcquireReaderLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库加读锁。");
#endif
            try
            {
                int nRet;
                bool bHasID;
                List<TableInfo> aTableInfo = null;
                nRet = this.TableNames2aTableInfo(searchItem.TargetTables,
                    out bHasID,
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bHasID == true)
                {
                    nRet = SearchByID(searchItem,
                        handle,
                        // isConnected,
                        resultSet,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (aTableInfo == null || aTableInfo.Count == 0)
                {
                    return 0;
                }

                for (int i = 0; i < aTableInfo.Count; i++)
                {
                    TableInfo tableInfo = aTableInfo[i]; ;
                    string strTiaoJian = "";
                    try
                    {
                        nRet = GetKeyCondition(
                            searchItem,
                            tableInfo.nodeConvertQueryString,
                            tableInfo.nodeConvertQueryNumber,
                            out strTiaoJian,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    catch (NoMatchException ex)
                    {
                        strWarning += ex.Message;
                        if (nWarningLevel == 0)
                            return -1;
                    }

                    XmlDocument dom = new XmlDocument();
                    dom.PreserveWhitespace = true; //设PreserveWhitespace为true

                    string strTablePath = this.TableName2TableFileName(tableInfo.SqlTableName);
                    try
                    {
                        dom.Load(strTablePath);
                    }
                    catch (Exception ex)
                    {
                        strError = "加载检索点表'" + tableInfo.SqlTableName + "'到dom出错：" + ex.Message;
                        return -1;
                    }

                    string strXpath = "/root/key[" + strTiaoJian + "]/idstring";
                    XmlNodeList listIdstring;
                    try
                    {
                        listIdstring = dom.SelectNodes(strXpath);
                    }
                    catch (System.Xml.XPath.XPathException ex)
                    {
                        strError += "Xpath出错:" + strXpath + "-------" + ex.Message + "<br/>";
                        return -1;
                    }

                    for (int j = 0; j < listIdstring.Count; j++)
                    {
                        string strIdstring = listIdstring[j].InnerText.Trim(); // 2012/2/16
                        string strId = this.FullID + "/" + strIdstring;
                        resultSet.Add(new DpRecord(strId));
                    }
                }

                //排序
                resultSet.Sort();

                //去重
                resultSet.RemoveDup();

                return 0;
            }
            finally
            {
                //*********对数据库解读锁************
                m_db_lock.ReleaseReaderLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("SearchByUnion()，对'" + this.GetCaption("zh-CN") + "'数据库解读锁。");
#endif
            }
        }

        // 检查一个文件是不是记录
        private bool IsRecord(string strFileName)
        {
            if (strFileName.Length != 14)
                return false;

            // 检查文件扩展名
            string strEx = Path.GetExtension(strFileName);
            if (strEx.Length > 1)
                strEx = strEx.Substring(1);
            strEx = strEx.ToUpper();
            if (strEx != "XML")
                return false;

            // 检查是不是10中的记录号
            string strRecordID = Path.GetFileNameWithoutExtension(strFileName);
            if (strRecordID.Length != 10)
                return false;
            if (StringUtil.RegexCompare(@"\d[10]", strRecordID) == false)
                return false;

            // 即是xml扩展名，又是长度10位时，才认作记录
            if (strEx == "XML"
                && strRecordID.Length == 10)
                return true;

            return false;
        }

        // 删除数据库
        // return:
        //      -1  出错
        //      0   成功
        // 线: 安全
        public override int Delete(out string strError)
        {
            strError = "";

            //************对数据库加写锁********************
            this.m_db_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-CN") + "'数据库加写锁。");
#endif
            try
            {
                // 删除数据源目录
                if (Directory.Exists(this.m_strSourceFullPath) == true)
                    Directory.Delete(this.m_strSourceFullPath, true);

                // 删除配置目录
                string strCfgsDir = DatabaseUtil.GetLocalDir(this.container.NodeDbs,
                    this.m_selfNode);
                if (strCfgsDir != "")
                {
                    // 应对目录查重，如果有其它库使用这个目录，则不能删除，返回信息
                    if (this.container.IsExistCfgsDir(strCfgsDir, this) == false)
                    {
                        Directory.Delete(this.container.DataDir + "\\" + strCfgsDir, true);
                    }
                    else
                    {
                        this.WriteErrorLog("发现除了'" + this.GetCaption("zh-CN") + "'库使用'" + strCfgsDir + "'目录外，还有其它库的使用这个目录，所以不能在删除库时删除目录");
                    }
                }

                return 0;
            }
            finally
            {
                //*********************对数据库解写锁**********
                m_db_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.container.WriteDebugInfo("Delete()，对'" + this.GetCaption("zh-CN") + "'数据库解写锁。");
#endif
            }
        }
    }
}
