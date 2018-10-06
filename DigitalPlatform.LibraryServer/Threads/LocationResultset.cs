using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.ResultSet;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 有关书目查询时馆藏地点限定结果集的功能
    /// </summary>
    public partial class LibraryApplication
    {
        private static readonly Object syncRoot_location = new Object();

        List<string> _requests = new List<string>();
        internal Task _taskCreateLocationResultset = null;

        // 获得当前所有积压的请求
        // return
        //      null    没有挤压的请求
        //      其他  汇总后的 location_list
        string GetPendingLocationList()
        {
            if (_requests.Count == 0)
                return null;

            List<string> locations = new List<string>();
            foreach (string s in _requests)
            {
                if (string.IsNullOrEmpty(s))
                    locations.Add("");
                else
                    locations.AddRange(StringUtil.SplitList(s));
            }

            // 去重
            locations.Sort();
            StringUtil.RemoveDup(ref locations);
            // 只要包含一个空字符串，就表示全部都需要刷新
            if (locations.IndexOf("") != -1)
                return "";
            return StringUtil.MakePathList(locations);
        }

        // parameters:
        //      location_list   馆藏地列表。如果为 "" 表示全部馆藏地。如果为 null，表示只执行积压的请求(本次没有新请求)
        public void StartCreateLocationResultset(string location_list)
        {
            lock (syncRoot_location)
            {
                // 观察是否已经有任务在运行。如果有了，就把请求加入队列
                if (_taskCreateLocationResultset != null)
                {
                    if (location_list != null && _requests.Count < 1000)
                        _requests.Add(location_list);
                    return;
                }

                // 每次启动，都是把前面积压的任务汇总后启动
                if (location_list != null && _requests.Count < 1000)
                    _requests.Add(location_list);
                string list = GetPendingLocationList();
                if (list != null)
                {
                    _requests.Clear();
                    _taskCreateLocationResultset = Task.Factory.StartNew(() => CreateLocationResultset(list));
                }
            }
        }

        void CreateLocationResultset(string location_list)
        {
            try
            {
                List<string> librarycodes = new List<string>();

                // 空表示全部馆代码
                if (string.IsNullOrEmpty(location_list))
                {
                    XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
                    foreach (XmlElement node in nodes)
                    {
                        string strLibraryCode = node.GetAttribute("libraryCode");
                        if (string.IsNullOrEmpty(strLibraryCode) == true)
                            continue;
                        librarycodes.Add(strLibraryCode);
                    }
                }
                else
                    librarycodes = StringUtil.SplitList(location_list);

                this.WriteErrorLog("馆藏地结果集创建开始 " + location_list);
                foreach (string code in librarycodes)
                {
                    this._app_down.Token.ThrowIfCancellationRequested();

                    this.WriteErrorLog("--- 馆藏地结果集创建 " + code);
                    CreateOneLocationResultset(code);
                }
                this.WriteErrorLog("馆藏地结果集创建结束 " + location_list);
            }
            finally
            {
                lock (syncRoot_location)
                {
                    _taskCreateLocationResultset = null;
                }
            }
        }

        public bool NeedRebuildResultset()
        {
            lock (syncRoot_location)
            {
                // 任务正在执行中，放弃探测
                if (_taskCreateLocationResultset != null)
                    return false;
            }

            List<string> librarycodes = new List<string>();

            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
            foreach (XmlElement node in nodes)
            {
                string strLibraryCode = node.GetAttribute("libraryCode");
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    continue;
                librarycodes.Add(strLibraryCode);
            }

            if (librarycodes.Count == 0)
                return false;

            return !LocationResultsetExists(librarycodes[0]);
        }

        // 探测一个结果集在 dp2kernel 一侧是否已经存在
        bool LocationResultsetExists(string strLocation)
        {
            string strError = "";

            // 临时的SessionInfo对象
            SessionInfo session = new SessionInfo(this);
            try
            {
                this._app_down.Token.ThrowIfCancellationRequested();

                RmsChannel channel = session.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                    return false;

#if DETAIL_LOG
                this.WriteErrorLog("开始探测结果集 " + strLocation);
#endif
                Record[] searchresults = null;

                // 获得检索结果的浏览格式
                // 浅包装版本
                long lHitCount = channel.DoGetSearchResult(
                    "#" + strLocation,
                    0,
                    0,
                    "id",
                    "zh",
                    null,
                    out searchresults,
                    out strError);
                if (lHitCount == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        return false;
                    return false;
                }

                return true;
            }
            finally
            {
                session.CloseSession();
                session = null;
            }
        }

        // 创建一个馆藏地的限定结果集
        void CreateOneLocationResultset(string strLocation)
        {
            string strError = "";

            // TODO: 要设法把临时的 Session 对象管理起来。在 Application down 的时候，主动对这些 session 的 Channels 执行 stop
            // 临时的 SessionInfo 对象
            SessionInfo session = new SessionInfo(this);
            try
            {
                string strQueryXml = "";
                // 构造检索实体库的 XML 检索式
                // return:
                //      -1  出错
                //      0   没有发现任何实体库定义
                //      1   成功
                int nRet = this.BuildSearchItemQuery(
    "<全部>",
    strLocation + "/",
    -1,
    "馆藏地点",
    "left",
    "zh",
    "", // strSearchStyle,
                out strQueryXml,
                out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                    return;

                this._app_down.Token.ThrowIfCancellationRequested();

                RmsChannel channel = session.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

#if DETAIL_LOG
                this.WriteErrorLog("开始检索");
#endif
                long lHitCount = channel.DoSearch(strQueryXml,
    "default",
    "", // strOutputStyle,
    out strError);
                if (lHitCount == -1)
                    goto ERROR1;

                if (lHitCount == 0)
                {
                    // 没有命中任何记录，也要继续后面的处理
                }

                this._app_down.Token.ThrowIfCancellationRequested();

                DpResultSet resultset = new DpResultSet(true);

                try
                {
                    if (lHitCount > 0)
                    {
                        nRet = GetResultset(channel,
                    "default",
                    resultset,
                    out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 写入 dp2kernel 成为永久结果集
                    nRet = UploadPermanentResultset(channel,
            strLocation,
            resultset,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    resultset.Close();
                }

                return;
            }
            finally
            {
                session.CloseSession();
                session = null;
            }

        ERROR1:
            this.WriteErrorLog("馆藏地结果集创建出错: " + strError);
            return;
        }

        const int _upload_batchSize = 1000; // 上传结果集每批个数

        // 写入 dp2kernel 成为永久结果集
        int UploadPermanentResultset(RmsChannel channel,
            string strResultsetName,
            DpResultSet resultset,
            out string strError)
        {
            strError = "";

            // resultset.Clear();  // testing

            List<RecordBody> records = new List<RecordBody>();

            // 结果集为空，也要在 dp2kernel 端创建一个结果集对象
            if (resultset.Count == 0)
            {
                RecordBody[] results = null;
                long lRet = channel.DoWriteRecords(null,
records.ToArray(),
"createResultset,name:#" + strResultsetName + ",clear,permanent",
out results,
out strError);
                if (lRet == -1)
                    return -1;
                return 0;
            }

#if DETAIL_LOG
            this.WriteErrorLog("开始上传结果集到 dp2kernel");
#endif

            long lPos = -1; // 中间保持不透明的值
            bool bFirst = true;
            for (long i = 0; i < resultset.Count; i++)
            {
                DpRecord dpRecord = null;

                long lIndex = i;
                if (lIndex == 0)
                {
                    dpRecord = resultset.GetFirstRecord(
                        lIndex,
                        false,
                        out lPos);
                }
                else
                {
                    // 取元素比[]操作速度快
                    dpRecord = resultset.GetNextRecord(
                        ref lPos);
                }
                if (dpRecord == null)
                    break;

                RecordBody record = new RecordBody();
                record.Path = dpRecord.ID;
                records.Add(record);

                if (records.Count >= _upload_batchSize || (i >= resultset.Count - 1 && records.Count > 0))
                {
                    this._app_down.Token.ThrowIfCancellationRequested();

#if DETAIL_LOG
                    this.WriteErrorLog("一批 " + records.Count);
#endif
                    RecordBody[] results = null;
                    long lRet = channel.DoWriteRecords(null,
    records.ToArray(),
    "createResultset,name:#~" + strResultsetName + (bFirst ? ",clear" : ""),
    out results,
    out strError);
                    if (lRet == -1)
                        return -1;
                    bFirst = false;
                    records.Clear();
                }
            }

            // 最后一次执行改名、排序
            {
                RecordBody[] results = null;
                long lRet = channel.DoWriteRecords(null,
null,
"renameResultset,oldname:#~" + strResultsetName + ",newname:#" + strResultsetName + ",permanent,sort",
out results,
out strError);
                if (lRet == -1)
                    return -1;
            }
#if DETAIL_LOG
            this.WriteErrorLog("结束上传结果集");
#endif
            return 0;
        }

        // 实体库名 --> 书目库名 对照表
        Hashtable m_biblioDbNameTable = new Hashtable();

        // 根据册记录的<parent>内容获得书目记录路径
        // return:
        //      -1  出错
        //      1   成功
        int GetBiblioRecPathByParentID(
            string strItemRecPath,
            string strParentID,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "strParentID 为空，无法获得书目记录路径";
                return -1;
            }

            if (StringUtil.IsPureNumber(strParentID) == false)
            {
                strError = "strParentID '" + strParentID + "' 不是纯数字，无法获得书目记录路径";
                return -1;
            }

            // string strItemDbName = ResPath.GetDbName(strItemRecPath);
            string strItemDbName = StringUtil.GetDbName(strItemRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从册记录路径 '" + strItemRecPath + "' 中获取数据库名的过程出错";
                return -1;
            }
            string strBiblioDbName = (string)m_biblioDbNameTable[strItemDbName];
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
            {
                string strDbType = this.GetDbType(strItemDbName,
                    out strBiblioDbName);
                if (strDbType == null)
                {
                    strError = "数据库 '" + strItemDbName + "' 的类型无法识别";
                    return -1;
                }
                if (string.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "实体库 '" + strItemDbName + "' 没有找到对应的书目库名";
                    return -1;
                }
                m_biblioDbNameTable[strItemDbName] = strBiblioDbName;
            }

            strBiblioRecPath = strBiblioDbName + "/" + strParentID;
            return 1;
        }

        const int _removeDup_batchSize = 10000; // 利用 hashtable 进行局部去重的每批个数

        // 从 dp2Kernel 端获取结果集
        // parameters:
        //      strStyle    tobibliorecpath 将实体\期\订购\评注 记录路径转换为书目记录路径,并去重
        int GetResultset(RmsChannel channel,
            string strResultsetName,
            DpResultSet resultset,
            out string strError)
        {
            strError = "";

            m_biblioDbNameTable.Clear();

            Hashtable temp_table = new Hashtable();

            string strFormat = "id,cols,format:@coldef://parent"; // "id,xml";

            SearchResultLoader loader = new SearchResultLoader(channel,
                null,
                strResultsetName,
                strFormat);
            loader.ElementType = "Record";

#if DETAIL_LOG
            this.WriteErrorLog("开始从 dp2kernel 获取结果集");
#endif

            int hashtable_removedup_loops = 0;  // 利用 hashtable 去重的轮次。如果只有一轮，则可以免去最后的结果集文件去重
            try
            {
                foreach (Record rec in loader)
                {
                    this._app_down.Token.ThrowIfCancellationRequested();

                    {
                        if (rec.RecordBody != null
                            && rec.RecordBody.Result != null
                            && rec.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                        {
#if NO
                        strError = "获得结果集位置偏移 " + (lStart + j).ToString() + " 时出错，该记录已被忽略: " + rec.RecordBody.Result.ErrorString;
                        this.AppendResultText(strError + "\r\n");
#endif
                            continue;
                        }

                        string strBiblioRecPath = "";

#if NO
                            // 从册记录XML中获得书目记录路径
                            // return:
                            //      -1  出错
                            //      1   成功
                            int nRet = GetBiblioRecPath(
                                rec.Path,
                                rec.RecordBody.Xml,
                                out strBiblioRecPath,
                                out strError);
                            if (nRet == -1)
                                return -1;
#endif
                        if (rec.Cols == null || rec.Cols.Length == 0)
                        {
#if NO
                        strError = "获得结果集位置偏移 " + (lStart + j).ToString() + " 时出错： rec.Cols 为空";
                        this.AppendResultText(strError + "\r\n");
#endif
                            continue;
                        }

                        // return:
                        //      -1  出错
                        //      1   成功
                        int nRet = GetBiblioRecPathByParentID(
                            rec.Path,
                            rec.Cols[0],
                            out strBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // 缓冲并局部去重。局部去重可以减轻后面全局去重的压力
                        if (temp_table.Contains(strBiblioRecPath) == false)
                        {
                            temp_table.Add(strBiblioRecPath, null);
                            if (temp_table.Count > _removeDup_batchSize)
                            {
                                FlushTable(temp_table, resultset);
                                temp_table.Clear();
                                hashtable_removedup_loops++;
                            }
                        }

                        //DpRecord record = new DpRecord(rec.Path);
                        //item_paths.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            // if (bToBiblioRecPath == true)
            {
                // 最后一批
                if (temp_table.Count > 0)
                {
                    FlushTable(temp_table, resultset);
                    temp_table.Clear();
                    hashtable_removedup_loops++;
                }

                resultset.Idle += new IdleEventHandler(biblio_paths_Idle);  // 2016/1/23 原来这里是 -=，令人费解
                try
                {
#if DETAIL_LOG
                this.WriteErrorLog("开始排序结果集, count=" + resultset.Count);
#endif

                    // 归并后写入结果集文件
                    resultset.QuickSort();
                    resultset.Sorted = true;

                    if (hashtable_removedup_loops > 1)
                    {
                        // 全局去重
#if DETAIL_LOG
                    this.WriteErrorLog("开始对结果集去重, count=" + resultset.Count);
#endif
                        resultset.RemoveDup();

#if DETAIL_LOG
                    this.WriteErrorLog("结束对结果集去重, count=" + resultset.Count);
#endif
                    }
                }
                finally
                {
                    resultset.Idle -= new IdleEventHandler(biblio_paths_Idle);
                }
            }

            return 0;
        }

        void FlushTable(Hashtable temp_table, DpResultSet resultset)
        {
            foreach (string path in temp_table.Keys)
            {
                this._app_down.Token.ThrowIfCancellationRequested();

                DpRecord record_bibliorecpath = new DpRecord(path);
                resultset.Add(record_bibliorecpath);
            }
        }

        void biblio_paths_Idle(object sender, IdleEventArgs e)
        {
            this._app_down.Token.ThrowIfCancellationRequested();
#if NO
            if (this.m_bClosed == true || this.Stopped == true)
            {
                throw new InterruptException("中断");
            }
#endif
        }

#if NO
        class InterruptException : Exception
        {
            public InterruptException(string s)
                : base(s)
            {
            }
        }
#endif
    }
}
