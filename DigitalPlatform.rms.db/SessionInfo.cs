// #define USE_TEMPDIR  // 使用每个通道的临时目录

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using DigitalPlatform.Range;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // 存放会话信息
    public class SessionInfo : IDisposable
    {
        public const int QUOTA_SIZE = (int)((double)(1024 * 1024) * (double)0.8);   // 经过试验 0.5 基本可行 因为字符数换算为 byte 数，中文的缘故
        public const int PACKAGE_UNIT_SIZE = 100;
        //定义一个最大数量 ,应该是用尺寸，这里暂时用数组个数计算
        public const int MaxRecordsCountPerApi = 1000;   // 原来是 200，2016/12/18 修改为 1000

#if USE_TEMPDIR
        private string m_strTempDir = "";	// 临时文件目录 2011/1/19
#endif

        public Hashtable ResultSets = new Hashtable();

        public ChannelHandle ChannelHandle = null;

        int /*object*/ m_nInSearching = 0;

        public int InSearching
        {
            get
            {
                return (int)m_nInSearching;
            }
            set
            {
                m_nInSearching = value;
            }
        }


        public int BeginSearch()
        {
            return Interlocked.Increment(ref m_nInSearching) - 1;

            /*
            lock (this.m_nInSearching)
            {
                int v = (int)m_nInSearching;
                m_nInSearching = v + 1;
                return v;
            }
             * */
        }

        public void EndSearch()
        {
            Interlocked.Decrement(ref m_nInSearching);

            /*
            lock (this.m_nInSearching)
            {
                int v = (int)m_nInSearching;
                m_nInSearching = v - 1;
            }
             * */
        }

        // 用户名
        public string UserName = "";

        public string ClientIP = "";  // 前端 IP 地址

        private KernelApplication app = null;

        // 初始化用户对象
        // parameters:
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int Initial(KernelApplication app,
            string strSessionID,
            string strIP,
            string strVia,
            out string strError)
        {
            strError = "";

            this.app = app;
            this.ClientIP = strIP;

            this.UserName = "";

            Debug.Assert(string.IsNullOrEmpty(app.SessionDir) == false, "");

#if USE_TEMPDIR
            this.m_strTempDir = PathUtil.MergePath(app.SessionDir, this.GetHashCode().ToString());
            PathUtil.CreateDirIfNeed(this.m_strTempDir);
#endif
            return 0;
        }

        public void Dispose()
        {
            this.Close();
        }

        public void ClearResultSets()
        {
            foreach (string key in this.ResultSets.Keys)
            {
                DpResultSet resultset = (DpResultSet)this.ResultSets[key];
                if (resultset != null)
                {
                    try
                    {
                        resultset.Close();
                    }
                    catch
                    {
                    }
                }
            }

            this.ResultSets.Clear();
        }

        public DpResultSet GetResultSet(string strResultSetName,
            bool bAutoCreate = true)
        {
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            strResultSetName = strResultSetName.ToLower();

            DpResultSet resultset = (DpResultSet)this.ResultSets[strResultSetName];
            if (resultset == null)
            {
                if (bAutoCreate == false)
                    return null;

#if NO
                resultset = new DpResultSet();
                resultset.GetTempFilename += new GetTempFilenameEventHandler(resultset_GetTempFilename);
#endif
                resultset = NewResultSet();
                this.ResultSets[strResultSetName] = resultset;
            }

            return resultset;
        }

        // 创建一个新的结果集对象，但并不进入容器管理范畴
        public DpResultSet NewResultSet()
        {
            DpResultSet resultset = new DpResultSet(GetTempFileName);
            Debug.Assert(string.IsNullOrEmpty(this.app.ResultsetDir) == false, "");
            resultset.TempFileDir = this.app.ResultsetDir;
            // resultset.GetTempFilename += new GetTempFilenameEventHandler(resultset_GetTempFilename);

            return resultset;
        }

        // 把结果集归入容器管理
        // parameters:
        //      resultset   结果集对象。如果为 null，实际上效果是删除这个结果集条目
        public void SetResultSet1(string strResultSetName,
            DpResultSet resultset)
        {
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            strResultSetName = strResultSetName.ToLower();

            DpResultSet exist_resultset = (DpResultSet)this.ResultSets[strResultSetName];

            // 即将设置的结果集正好是已经存在的结果集
            if (exist_resultset == resultset)
                return; //  exist_resultset;

            if (resultset == null)
                this.ResultSets.Remove(strResultSetName);
            else
                this.ResultSets[strResultSetName] = resultset;
            /*
            resultset.GetTempFilename -= new GetTempFilenameEventHandler(resultset_GetTempFilename);
            resultset.GetTempFilename += new GetTempFilenameEventHandler(resultset_GetTempFilename);
             * */
            if (exist_resultset != null)
            {
                // 释放以前的同名结果集对象
                exist_resultset.Close();
            }

            return; // exist_resultset;
        }

        void resultset_GetTempFilename(object sender, GetTempFilenameEventArgs e)
        {
            e.TempFilename = GetTempFileName();
        }

        public string GetTempFileName()
        {
#if USE_TEMPDIR
            Debug.Assert(string.IsNullOrEmpty(this.m_strTempDir) == false, "");
            while (true)
            {
                string strFilename = PathUtil.MergePath(this.m_strTempDir, Guid.NewGuid().ToString());
                if (File.Exists(strFilename) == false)
                {
                    using (FileStream s = File.Create(strFilename))
                    {
                    }
                    return strFilename;
                }
            }
#else
            Debug.Assert(string.IsNullOrEmpty(this.app.ResultsetDir) == false, "");
            while (true)
            {
                string strFilename = Path.Combine(this.app.ResultsetDir, Guid.NewGuid().ToString());
                if (File.Exists(strFilename) == false)
                {
                    using (FileStream s = File.Create(strFilename))
                    {
                    }
                    return strFilename;
                }
            }
#endif
        }

        KeyFrom[] BuildKeyFromArray(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return null;

            List<KeyFrom> results = new List<KeyFrom>();
            string[] lines = strText.Split(new char[] { DpResultSetManager.SPLIT });
            for (int i = 0; i < lines.Length; i++)
            {
                string strLine = lines[i];
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                string strKey = "";
                string strFrom = "";

                char chLogic = (char)0;  // none
                if (strLine[0] == DpResultSetManager.OR
                    || strLine[0] == DpResultSetManager.AND)
                {
                    chLogic = strLine[0];
                    strLine = strLine.Substring(1);
                }

                int nRet = strLine.IndexOf(DpResultSetManager.FROM_LEAD);
                if (nRet == -1)
                    strKey = strLine;
                else
                {
                    strKey = strLine.Substring(0, nRet);
                    strFrom = strLine.Substring(nRet + 1);
                }

                KeyFrom keyfrom = new KeyFrom();
                if (chLogic == DpResultSetManager.OR)
                    keyfrom.Logic = "OR";
                else if (chLogic == DpResultSetManager.AND)
                    keyfrom.Logic = "AND";
                keyfrom.Key = strKey;
                keyfrom.From = strFrom;

                results.Add(keyfrom);
            }

            KeyFrom[] keyfroms = new KeyFrom[results.Count];
            results.CopyTo(keyfroms);

            return keyfroms;
        }

        static int GetLength(KeyFrom[] list)
        {
            if (list == null || list.Length == 0)
                return 0;
            int nLength = 0;
            foreach (KeyFrom item in list)
            {
                if (item == null)
                    continue;
                if (item.Key != null)
                    nLength += item.Key.Length;
                if (item.From != null)
                    nLength += item.From.Length;
                if (item.Logic != null)
                    nLength += item.Logic.Length;
            }

            return nLength;
        }

        static int GetLength(RecordBody body)
        {
            int nLength = 0;

            if (body == null)
                return 0;

            if (body.Xml != null)
            {
                nLength += body.Xml.Length;
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }

            if (string.IsNullOrEmpty(body.Metadata) == false)
            {
                nLength += body.Metadata.Length;
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }
            if (body.Timestamp != null)
            {
                nLength += body.Timestamp.Length * 2;
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }

            if (body.Result != null)
            {
                if (body.Result.ErrorString != null)
                    nLength += body.Result.ErrorString.Length;
                nLength += 16 + 20; // Value 和 ErrorCode 估算的尺寸
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }

            nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            return nLength;
        }

        static int GetLength(string[] cols)
        {
            if (cols == null)
                return 0;
            int nLength = 0;
            foreach (string s in cols)
            {
                if (s != null)
                    nLength += s.Length;

                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }

            nLength += PACKAGE_UNIT_SIZE;  // 估计整体 20 个 bytes 的额外消耗
            return nLength;
        }

        static int GetLength(Record record)
        {
            int nLength = 0;

            if (record == null)
                return 0;

            if (record.Path != null)
            {
                nLength += record.Path.Length;
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }
            if (record.Keys != null)
            {
                nLength += GetLength(record.Keys);
                nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            }

            if (record.Cols != null)
                nLength += GetLength(record.Cols);

            if (record.RecordBody != null)
                nLength += GetLength(record.RecordBody);

            nLength += PACKAGE_UNIT_SIZE;  // 估计 20 个 bytes 的额外消耗
            return nLength;
        }

        // 从结果集中提取指定范围的记录
        // parameter:
        //		lStart	开始序号
        //		lLength	长度. -1表示从lStart到末尾
        //		strLang	语言
        //		strStyle	样式,以逗号分隔，id:表示取id,cols表示取浏览格式
        //              如果包含 format:cfgs/browse 这样的子串，表示指定浏览格式
        //		aRecord	得到的记录数组，成员为类型为Record
        //      strError    out参数，返回出错信息
        // result:
        //		-1	出错
        //		>=0	结果集的总数
        public long API_GetRecords(
            DpResultSet resultSet,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out Record[] records,
            out string strError)
        {
            records = null;
            strError = "";

            // DpResultSet resultSet = this.DefaultResultSet;
            if (resultSet == null)
            {
                strError = "GetRecords()出错, resultSet 为 null";
                return -1;
            }

            // 2017/8/23
            if (resultSet.Count == 0 && lLength > 0)
            {
                strError = "结果集为空，无法取出任何记录";
                return -1;
            }

            long lTotalPackageLength = 0;   // 累计计算要输出的XML记录占据的空间

            long lOutputLength;
            // 检查lStart lLength和resultset.Count之间的关系,
            // 和每批返回最大元素数限制, 综合得出一个合适的尺寸
            // return:
            //		-1  出错
            //		0   成功
            int nRet = ConvertUtil.GetRealLengthNew((int)lStart,    // 2017/9/3 从 GetRealLength() 改为 GetRealLengthNew()
                (int)lLength,
                (int)resultSet.Count,
                SessionInfo.MaxRecordsCountPerApi,//nMaxCount,
                out lOutputLength,
                out strError);
            if (nRet == -1)
                return -1;

            bool bKeyCount = StringUtil.IsInList("keycount", strStyle, true);
            bool bKeyID = StringUtil.IsInList("keyid", strStyle, true);

            bool bHasID = StringUtil.IsInList("id", strStyle, true);
            bool bHasCols = StringUtil.IsInList("cols", strStyle, true);
            bool bHasKey = StringUtil.IsInList("key", strStyle, true);

            if (bKeyID == true
                && bHasID == false && bHasCols == false && bHasKey == false)
            {
                strError = "strStyle包含了keyid但是没有包含id/key/cols中任何一个，导致API不返回任何内容，操作无意义";
                return -1;
            }

            bool bXml = StringUtil.IsInList("xml", strStyle, true);
            bool bWithResMetadata = StringUtil.IsInList("withresmetadata", strStyle, true);
            bool bTimestamp = StringUtil.IsInList("timestamp", strStyle, true);
            bool bMetadata = StringUtil.IsInList("metadata", strStyle, true);

            string strFormat = StringUtil.GetStyleParam(strStyle, "format");

            List<Record> results = new List<Record>(100);

            long lPos = -1; // 中间保持不透明的值

            for (long i = 0; i < lOutputLength; i++)
            {
                DpRecord dpRecord = null;

                long lIndex = lStart + i;
                if (lIndex == lStart)
                {
                    dpRecord = resultSet.GetFirstRecord(
                        lIndex,
                        false,
                        out lPos);
                }
                else
                {
                    // 取元素比[]操作速度快
                    dpRecord = resultSet.GetNextRecord(
                        ref lPos);
                }

                if (dpRecord == null)
                    break;

                Record record = new Record();

                if (bKeyCount == true)
                {
                    record.Path = dpRecord.ID;
                    record.Cols = new string[1];
                    record.Cols[0] = dpRecord.Index.ToString();

#if NO
                    lTotalPackageLength += record.Path.Length;
                    lTotalPackageLength += record.Cols[0].Length;
                    if (lTotalPackageLength > QUOTA_SIZE
                        && i > 0)
                    {
                        // 响应包的尺寸已经超过 1M，并且已经至少包含了一条记录
                        break;
                    } 
#endif
                    goto CONTINUE;
                }

                DbPath path = new DbPath(dpRecord.ID);
                Database db = this.app.Dbs.GetDatabaseSafety(path.Name);
                if (db == null)
                {
                    strError = "GetDatabaseSafety()从库id '" + path.Name + "' 找数据库对象失败";
                    return -1;
                }

                // 如果有必要获得记录体
                string strXml = "";
                string strMetadata = "";
                byte[] baTimestamp = null;
                if (bXml == true || bTimestamp == true || bMetadata == true)
                {
                    // 获得一条记录的 XML 字符串
                    // return:
                    //		-1  出错
                    //		-4  未找到记录
                    //      -10 记录局部未找到
                    //		0   成功
                    long lRet = GetXmlBody(
db,
path.ID,
bXml,
bTimestamp,
bMetadata,
bWithResMetadata,
out strXml,
out strMetadata,
out baTimestamp,
out strError);
                    if (lRet <= -1)
                    {
                        record.RecordBody = new RecordBody();
                        record.RecordBody.Xml = strXml;
                        record.RecordBody.Metadata = strMetadata;
                        record.RecordBody.Timestamp = baTimestamp;

                        Result result = new Result();
                        result.Value = -1;
                        result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                        result.ErrorString = strError;

                        record.RecordBody.Result = result;
                        goto CONTINUE;
                        // return lRet;
                    }

#if NO
                    lTotalPackageLength += strXml.Length;
                    if (string.IsNullOrEmpty(strMetadata) == false)
                        lTotalPackageLength += strMetadata.Length;
                    if (baTimestamp != null)
                        lTotalPackageLength += baTimestamp.Length * 2;
                    if (lTotalPackageLength > QUOTA_SIZE
                        && i > 0)
                    {
                        // 响应包的尺寸已经超过 1M，并且已经至少包含了一条记录
                        break;
                    }
#endif

                    record.RecordBody = new RecordBody();
                    record.RecordBody.Xml = strXml;
                    record.RecordBody.Metadata = strMetadata;
                    record.RecordBody.Timestamp = baTimestamp;
                }

                if (bKeyID == true)
                {
                    // string strID = "";
                    string strKey = "";

                    // strID = dpRecord.ID;
                    strKey = dpRecord.BrowseText;

                    /*
                    string strText = dpRecord.ID;
                    nRet = strText.LastIndexOf(",");
                    if (nRet != -1)
                    {
                        strKey = strText.Substring(0, nRet);
                        strID = strText.Substring(nRet + 1);
                    }
                    else
                        strID = strText;
                     * */

                    if (bHasID == true)
                    {
                        // GetCaptionSafety()函数先找到指定语言的库名;
                        // 如果没有找到,就找截短形态的语言的库名;
                        // 再找不到,就用第一种语言的库名。
                        // 如果连一种语言也没有，则返回库id
                        record.Path = db.GetCaptionSafety(strLang) + "/" + path.CompressedID;
                        // lTotalPackageLength += record.Path.Length;
                    }

                    if (bHasKey == true)
                    {
                        record.Keys = BuildKeyFromArray(strKey);
                        // lTotalPackageLength += GetLength(record.Keys);
                    }

                    if (bHasCols == true)
                    {
                        string[] cols;
                        nRet = db.GetCols(
                            strFormat,
                            path.ID10,
                            strXml,
                            0,
                            out cols,
                            out strError);
#if NO
                        // 2013/1/14
                        if (nRet == -1)
                        {
                            if (cols != null && cols.Length > 0)
                                strError = cols[0];
                            else
                                strError = "GetCols() error";
                            return -1;
                        }
#endif
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            record.RecordBody = new RecordBody();
                            if (record.RecordBody.Result == null)
                                record.RecordBody.Result = new Result();
                            record.RecordBody.Result.ErrorCode = ErrorCodeValue.NotFound;
                            record.RecordBody.Result.ErrorString = strError;
                        }
                        record.Cols = cols;

                        // lTotalPackageLength += nRet;

                    }

                    goto CONTINUE;
                }

                {

                    if (bHasID == true)
                    {
                        // GetCaptionSafety()函数先找到指定语言的库名;
                        // 如果没有找到,就找截短形态的语言的库名;
                        // 再找不到,就用第一种语言的库名。
                        // 如果连一种语言也没有，则返回库id
                        record.Path = db.GetCaptionSafety(strLang) + "/" + path.CompressedID;
                        // lTotalPackageLength += record.Path.Length;
                    }

                    // 在不是keyid的风格下(例如keycount，空)，cols全部是浏览列
                    if (bHasCols == true)
                    {
                        string[] cols;
                        nRet = db.GetCols(
                            strFormat,
                            path.ID10,
                            strXml,
                            0,
                            out cols,
                            out strError);
#if NO
                        // 2013/1/14
                        if (nRet == -1)
                        {
                            if (cols != null && cols.Length > 0)
                                strError = cols[0];
                            else
                                strError = "GetCols() error";
                            return -1;
                        }
#endif
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            record.RecordBody = new RecordBody();
                            if (record.RecordBody.Result == null)
                                record.RecordBody.Result = new Result();
                            record.RecordBody.Result.ErrorCode = ErrorCodeValue.NotFound;
                            record.RecordBody.Result.ErrorString = strError;
                        }
                        record.Cols = cols;
                        // lTotalPackageLength += nRet;
                    }

                }

            CONTINUE:
                lTotalPackageLength += GetLength(record);
                if (lTotalPackageLength > QUOTA_SIZE
&& i > 0)
                {
                    // 响应包的尺寸已经超过 1M，并且已经至少包含了一条记录
                    break;
                }
                // records[i] = record;
                results.Add(record);
                Thread.Sleep(0);    // 降低CPU耗用?
            }

            records = new Record[results.Count];
            results.CopyTo(records);

            return resultSet.Count;
        }

        // 获得一条记录的 XML 字符串
        // return:
        //		-1  出错
        //		-4  未找到记录
        //      -10 记录局部未找到
        //		0   成功
        long GetXmlBody(
            Database db,
            string strID,
            bool bXml,
            bool bTimestamp,
            bool bMetadata,
            bool bWithResMetadata,
            out string strXml,
            out string strMetadata,
            out byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            strXml = "";
            strMetadata = "";
            baTimestamp = null;

            string strGetStyle = "";   //"data,timestamp"; // ,outputpath, metadata

            if (bTimestamp == true)
                StringUtil.SetInList(ref strGetStyle, "timestamp", true);

            if (bXml == true)
                StringUtil.SetInList(ref strGetStyle, "data", true);

            if (bMetadata == true)
                StringUtil.SetInList(ref strGetStyle, "metadata", true);

            if (bWithResMetadata == true)
                StringUtil.SetInList(ref strGetStyle, "withresmetadata", true);

            int nStart0 = 0;
            int nLength0 = -1;
            int nMaxLength = 300 * 1024;    // 每次最多获取300K
            int nTotalLength = 0;

            string strOutputID = "";

            byte[] baTotal = null;

            int nAdditionError = 0;

            for (; ; )
            {
                byte[] baData = null;

                long lRet = db.GetXml(strID,
                    "", // strXPath,
                    nStart0,
                    nLength0,
                    nMaxLength,
                    strGetStyle,
                    out baData,
                    out strMetadata,
                    out strOutputID,
                    out baTimestamp,
                    true,
                    out nAdditionError,
                    out strError);
                if (lRet <= -1)
                    return lRet;

                nTotalLength = (int)lRet;

                // 如果数据体太大
                if (nTotalLength > QUOTA_SIZE)
                {
                    strError = "数据尺寸超过 " + QUOTA_SIZE.ToString();
                    return -1;
                }

                // 2012/11/28
                // 如果不想获得数据, baData就是null
                if (baData == null)
                    break;

                baTotal = ByteArray.Add(baTotal, baData);

                nStart0 += baData.Length;

                if (nStart0 >= nTotalLength)
                    break;
            }

            // 记录体
            // 转换成字符串
            if (bXml == true && baTotal != null)
            {
                strXml = ByteArray.ToString(baTotal);
            }

            return 0;
        }

        // 从结果集中提取指定范围的记录
        // parameter:
        //		strRanges	范围
        //		strStyle	样式,以逗号分隔，id:表示取id,cols表示取浏览格式,xml表示取xml记录体
        //		strLang     语言版本，用来获得记录路径
        //		richRecords	得到的记录数组，成员为类型为Record
        // result:
        //		-1  出错
        //		>=0	结果集的总数
        public long API_GetRichRecords(
            DpResultSet resultset,
            string strRanges,
            string strLang,
            string strStyle,
            out RichRecord[] richRecords,
            out string strError)
        {
            strError = "";
            richRecords = null;

            int nCurCount = 0;
            List<RichRecord> aRichRecord = new List<RichRecord>();

            string strFormat = StringUtil.GetStyleParam(strStyle, "format");

            RangeList rangeList = new RangeList(strRanges);
            for (int i = 0; i < rangeList.Count; i++)
            {
                RangeItem rangeItem = (RangeItem)rangeList[i];
                int nStart = (int)rangeItem.lStart;
                int nLength = (int)rangeItem.lLength;
                if (nLength == 0)
                    continue;

                // long lPos = 0;  // 应该用快速方式，不应用[]??? 2006/3/29

                for (int j = 0; j < nLength; j++)
                {
                    int nRet = 0;

                    DpRecord dpRecord = resultset[j + nStart];
                    RichRecord richRecord = new RichRecord();

                    DbPath dbpath = new DbPath(dpRecord.ID);
                    Database db = this.app.Dbs.GetDatabaseSafety(dbpath.Name);
                    if (db == null)  //也应放到本条的记录出错信息里
                    {
                        strError = "没有找到数据库'" + dbpath.Name + "'，换语言版本时出错";
                        richRecord.Result.Value = -1;
                        richRecord.Result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        richRecord.Result.ErrorString = strError;
                    }
                    else
                    {
                        // 记录路径
                        if (StringUtil.IsInList("id", strStyle, true) == true
                            || StringUtil.IsInList("path", strStyle, true) == true)
                        {
                            richRecord.Path = db.GetCaptionSafety(strLang) + "/" + dbpath.CompressedID;
                        }

                        // 浏览列
                        if (StringUtil.IsInList("cols", strStyle, true) == true)
                        {
                            string[] cols = null;

                            nRet = db.GetCols(
                                strFormat,
                                dbpath.ID10,
                                "",
                                0,
                                out cols,
                                out strError);
#if NO
                            // 2013/1/14
                            if (nRet == -1)
                            {
                                if (cols != null && cols.Length > 0)
                                    strError = cols[0];
                                else
                                    strError = "GetCols() error";
                                return -1;
                            }
#endif
                            if (nRet == 0)
                            {
                                if (richRecord.Result == null)
                                    richRecord.Result = new Result();
                                richRecord.Result.ErrorCode = ErrorCodeValue.NotFound;
                                richRecord.Result.ErrorString = strError;
                            }
                            richRecord.Cols = cols;
                        }

                        bool bXml = false;
                        bool bTimestamp = false;

                        if (StringUtil.IsInList("xml", strStyle, true) == true)
                            bXml = true;

                        if (StringUtil.IsInList("timestamp", strStyle, true) == true)
                            bTimestamp = true;

                        if (bXml == true
                            || bTimestamp == true)
                        {
                            string strGetStyle = "";   //"data,timestamp"; // ,outputpath, metadata

                            if (bTimestamp == true)
                                StringUtil.SetInList(ref strGetStyle, "timestamp", true);

                            if (bXml == true)
                                StringUtil.SetInList(ref strGetStyle, "data", true);

                            int nStart0 = 0;
                            int nLength0 = -1;
                            int nMaxLength = 300 * 1024;    // 每次最多获取300K
                            int nTotalLength = 0;

                            string strOutputID = "";

                            byte[] baTotal = null;

                            byte[] baOutputTimestamp = null;
                            int nAdditionError = 0;

                            string strMetadata = "";

                            for (; ; )
                            {
                                byte[] baData = null;

                                long lRet = db.GetXml(dbpath.ID,
                                    "", // strXPath,
                                    nStart0,
                                    nLength0,
                                    nMaxLength,
                                    strGetStyle,
                                    out baData,
                                    out strMetadata,
                                    out strOutputID,
                                    out baOutputTimestamp,
                                    true,
                                    out nAdditionError,
                                    out strError);
                                if (lRet <= -1)
                                {
                                    richRecord.Result.Value = -1;
                                    richRecord.Result.ErrorCode = KernelApplication.Ret2ErrorCode(nAdditionError);   // nRet?
                                    richRecord.Result.ErrorString = strError;
                                    goto CONTINUE;
                                }

                                nTotalLength = (int)lRet;

                                // 如果数据体太大
                                if (nTotalLength > QUOTA_SIZE)
                                {
                                    richRecord.Result.Value = -1;
                                    richRecord.Result.ErrorCode = ErrorCodeValue.CommonError;
                                    richRecord.Result.ErrorString = "数据超过1M";
                                    goto CONTINUE;
                                }

                                baTotal = ByteArray.Add(baTotal, baData);

                                nStart0 += baData.Length;

                                if (nStart0 >= nTotalLength)
                                    break;
                            }

                            // 记录体
                            // 转换成字符串
                            if (StringUtil.IsInList("xml", strStyle, true) == true)
                            {
                                richRecord.Xml = ByteArray.ToString(baTotal);
                            }

                            // 时间戳?
                            if (StringUtil.IsInList("timestamp", strStyle, true) == true)
                            {
                                richRecord.baTimestamp = baOutputTimestamp;
                            }

                            // string strOutputPath = strDbName + "/" + strOutputID;

                        }

                        // 记录体
                        if (StringUtil.IsInList("xml", strStyle, true) == true)
                        {
                            /*
                            nRet = db.GetXmlDataSafety(dbpath.ID,
                                out richRecord.Xml,
                                out strError);
                            if (nRet <= -1)
                            {
                                richRecord.Result.Value = -1;
                                richRecord.Result.ErrorCode = RmswsApplication.Ret2ErrorCode(nRet);
                                richRecord.Result.ErrorString = strError;
                            }
                             * */
                        }

                    }

                CONTINUE:

                    aRichRecord.Add(richRecord);

                    Thread.Sleep(0);

                    nCurCount++;

                    // 如果超出最大范围，则停止
                    if (nCurCount >= SessionInfo.MaxRecordsCountPerApi)
                        break;
                }
            }

            richRecords = new RichRecord[aRichRecord.Count];
            for (int i = 0; i < richRecords.Length; i++)
            {
                richRecords[i] = aRichRecord[i];
            }

            return resultset.Count;
        }

        // 获得一批记录的浏览格式
        // 目前情况是：除了数据库名出错以外，其他小错不会终止循环。浏览的第一列放了表示错误的字符串。
        // parameter:
        //		paths	记录路径数组
        //		strStyle	风格
        //		aRecord	得到的记录数组，成员为类型为Record
        // result:
        //      -1  出错
        //      0   成功
        public int API_GetBrowse(string[] paths,
            string strStyle,
            out Record[] records,
            out string strError)
        {
            records = null;
            strError = "";
            int nRet = 0;

            if (paths == null)
            {
                strError = "API_GetBrowse() paths == null";
                return -1;
            }

            //定义一个最大数量 ,应该是用尺寸，这里暂时用数组个数计算
            //int nMaxCount = 100;
            bool bHasID = StringUtil.IsInList("id", strStyle);
            bool bHasCols = StringUtil.IsInList("cols", strStyle);

            bool bXml = StringUtil.IsInList("xml", strStyle, true);
            bool bWithResMetadata = StringUtil.IsInList("withresmetadata", strStyle, true);
            bool bTimestamp = StringUtil.IsInList("timestamp", strStyle, true);
            bool bMetadata = StringUtil.IsInList("metadata", strStyle, true);

            string strFormat = StringUtil.GetStyleParam(strStyle, "format");

            long lTotalPackageLength = 0;   // 累计计算要输出的XML记录占据的空间
            List<Record> results = new List<Record>(100);

            for (long i = 0; i < paths.Length; i++)
            {
                Record record = new Record();

                string strPath = paths[i];

#if NO
                DbPath path = new DbPath(strPath);
                Database db = this.app.Dbs.GetDatabaseSafety(path.Name);
                if (db == null)
                {
                    strError = "没有找到数据库'" + path.Name + "'，换语言版本时出错";
                    return -1;
                }
#endif
                DatabaseCollection.PathInfo info = null;
                // 解析资源路径
                // return:
                //      -1  一般性错误
                //		-5	未找到数据库
                //		-7	路径不合法
                //      0   成功
                nRet = this.app.Dbs.ParsePath(strPath,
    out info,
    out strError);
                if (nRet < 0)
                    return -1;

                if (info == null)
                {
                    strError = "ParsePath() (strPath='" + strPath + "') error, info == null";
                    return -1;
                }

                if (info.IsConfigFilePath == true)
                {
                    strError = "路径 '" + strPath + "' 不是记录型的路径";
                    return -1;
                }

                if (bHasID == true)
                {
                    record.Path = strPath;
                }
                if (bHasCols == true && info.IsObjectPath == false)
                {
                    if (info.Database == null)
                    {
                        strError = "ParsePath() (strPath='" + strPath + "') error, info.Database == null";
                        return -1;
                    }
                    string[] cols;
                    // return:
                    //      -1  出错
                    //      0   记录没有找到
                    //      其他  cols 中包含的字符总数
                    nRet = info.Database.GetCols(
                        strFormat,
                        info.RecordID10,    // path.ID10,
                        "",
                        0,
                        out cols,
                        out strError);
#if NO
                    if (nRet == -1)
                    {
                        if (cols != null && cols.Length > 0)
                            strError = cols[0];
                        else
                            strError = "GetCols() error";
                        return -1;
                    }
#endif
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        record.RecordBody = new RecordBody();
                        if (record.RecordBody.Result == null)
                            record.RecordBody.Result = new Result();
                        record.RecordBody.Result.ErrorCode = ErrorCodeValue.NotFound;
                        record.RecordBody.Result.ErrorString = "1 " + strError;
                    }
                    record.Cols = cols;
                }

                // 如果有必要获得记录体
                string strXml = "";
                string strMetadata = "";
                byte[] baTimestamp = null;
                if (info.IsObjectPath == false &&
                    (bXml == true || bTimestamp == true || bMetadata == true)
                    )
                {
                    long lRet = GetXmlBody(
info.Database,  // db,
info.RecordID,  // path.ID,
bXml,
bTimestamp,
bMetadata,
bWithResMetadata,
out strXml,
out strMetadata,
out baTimestamp,
out strError);

                    record.RecordBody = new RecordBody();
                    record.RecordBody.Xml = strXml;
                    record.RecordBody.Metadata = strMetadata;
                    record.RecordBody.Timestamp = baTimestamp;

                    if (lRet <= -1)
                    {
                        Result result = new Result();
                        result.Value = -1;
                        result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                        result.ErrorString = "2 " + strError;
                        record.RecordBody.Result = result;
                        // return (int)lRet;
                    }
                }

                // 2015/11/14
                if (info.IsObjectPath == true &&
                    (bTimestamp == true || bMetadata == true)
                    )
                {
                    if (info.Database == null)
                    {
                        strError = "ParsePath() (strPath='" + strPath + "') error, info.Database == null";
                        return -1;
                    }
                    byte[] buffer = new byte[10];
                    // return:
                    //		-1  出错
                    //		-4  记录不存在
                    //		>=0 资源总长度
                    long lRet = info.Database.GetObject(info.RecordID,
                        info.ObjectID,
                        null,
                        0,
                        0,
                        -1,
                        strStyle,
                        out buffer,
                        out strMetadata,
                        out baTimestamp,
                        out strError);

                    record.RecordBody = new RecordBody();
                    record.RecordBody.Xml = "";
                    record.RecordBody.Metadata = strMetadata;
                    record.RecordBody.Timestamp = baTimestamp;

                    if (lRet <= -1)
                    {
                        Result result = new Result();
                        result.Value = -1;
                        result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                        result.ErrorString = "3 " + strError;
                        record.RecordBody.Result = result;
                        // return (int)lRet;
                    }
                }

                lTotalPackageLength += GetLength(record);
                if (lTotalPackageLength > QUOTA_SIZE
&& i > 0)
                {
                    // 响应包的尺寸已经超过 1M，并且已经至少包含了一条记录
                    break;
                }

                results.Add(record);
                Thread.Sleep(0);    // 降低CPU耗用?
            }
            /*
            if (paths.Length <= SessionInfo.MaxRecordsCountPerApi)
                records = new Record[paths.Length];
            else
                records = new Record[SessionInfo.MaxRecordsCountPerApi];
             * */

            records = new Record[results.Count];
            results.CopyTo(records);
            return 0;
        }

        // 关闭
        public void Close()
        {
            string strError = "";
            int nRet = 0;

#if NO
            if (this.DelayTables != null && this.DelayTables.Count != 0)
            {
                try
                {
                    List<RecordBody> results = null;
                    nRet = app.Dbs.API_WriteRecords(
                        this,
                        null,
                        null,
                        "flushkeys",
                        out results,
                        out strError);
                    if (nRet == -1)
                    {
                        this.app.Dbs.KernelApplication.WriteErrorLog("SessionInfo.Close() flushkeys 出错：" + strError);
                    }
                }
                catch (Exception ex)
                {
                    this.app.Dbs.KernelApplication.WriteErrorLog("SessionInfo.Close() flushkeys 抛出异常：" + ex.Message);
                }
            }
#endif

            nRet = this.CloseUser(out strError);
            if (nRet == -1)
            {
                try
                {
                    this.app.Dbs.KernelApplication.WriteErrorLog("SessionInfo.Close()出错：" + strError);
                }
                catch
                {
                }
            }

            this.UserName = "";
            // this.DefaultResultSet = null;
            this.ClearResultSets();

#if USE_TEMPDIR
            if (String.IsNullOrEmpty(this.m_strTempDir) == false)
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(this.m_strTempDir);
                    if (di.Exists == true)
                        di.Delete(true);
                }
                catch
                {
                }
            }
#endif

            this.app = null;
        }

        // SessionInfo对象关闭
        // parameters:
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        private int CloseUser(out string strError)
        {
            strError = "";

            string strUserName = this.UserName;
            if (strUserName == "")
                return 0;

            UserCollection users = this.app.Users;
            if (users == null)
            {
                strError = "SessionInfo.Close()发现globalInfo.Users为null，异常";
                return -1;
            }

            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到，并从集合中清除
            int nRet = users.ReleaseUser(
                strUserName,
                out strError);
            if (nRet == -1)
                return -1;
            return 0;

            /*
            User user = null;
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到
            // 线：安全
            int nRet = users.GetUser(strUserName,???? relese
                out user,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = "Session_End，未找到名为'" + this.UserName + "'的User对象。";
                return -1;
            }

            if (user == null)
            {
                strError = "Session_End，此时User对象不应为null。";
                return -1;
            }

            // 减少帐户的一次使用率
            user.MinusOneUse();

            // 当该用户的使用数量变为0且用户集合超过超出范围时才删除用户
            if (user.UseCount == 0)
            {
                users.ActivateWorker(); // 可有可无
                
                //nRet = users.RemoveUserIfOutOfRange(user,
                //    out strError);
                //if (nRet == -1)
                //    return -1;
                //
            }
            */
        }
    }

}
