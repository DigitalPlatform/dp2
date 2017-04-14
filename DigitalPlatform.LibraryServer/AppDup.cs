using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using System.IO;

// using DigitalPlatform.rms.Client.rmsws_localhost;   // Record

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和编目查重功能相关的代码
    /// </summary>
    public partial class LibraryApplication
    {

        // 获得查重检索命中结果
        // parameters:
        //      lStart  返回命中结果集起始位置
        //      lCount  返回命中结果集的记录个数
        //      strBrowseInfoStyle  所返回的DupSearchResult中包含哪些信息
        //              "cols"  包含浏览列
        //              "excludecolsoflowthreshold" 不包含权值低于阈值的行的浏览列。要在同时包含cols时才起作用
        //      searchresults   包含记录信息的DupSearchResult数组
        public LibraryServerResult GetDupSearchResult(
            SessionInfo sessioninfo,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            out DupSearchResult[] searchresults)
        {
            string strError = "";
            searchresults = null;
            int nRet = 0;

            LibraryServerResult result = new LibraryServerResult();

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            DupResultSet dupset = sessioninfo.DupResultSet;

            if (dupset == null)
            {
                strError = "查重结果集不存在";
                goto ERROR1;
            }

            dupset.EnsureCreateIndex(getTempFileName);

            int nCount = (int)lCount;
            int nStart = (int)lStart;

            if (nCount == -1)
            {
                nCount = (int)dupset.Count - nStart;
                if (nCount < 0)
                    nCount = 0;
            }
            else
            {
                if (nCount > (int)dupset.Count - nStart)
                {
                    nCount = (int)dupset.Count - nStart;

                    if (nCount < 0)
                        nCount = 0;
                }
            }

            bool bExcludeCols = (StringUtil.IsInList("excludecolsoflowthreshold", strBrowseInfoStyle) == true);

            bool bCols = (StringUtil.IsInList("cols", strBrowseInfoStyle) == true);

            List<string> pathlist = new List<string>();

            List<DupSearchResult> results = new List<DupSearchResult>();
            for (int i = 0; i < nCount; i++)    // BUG nStart + 
            {
                DupLineItem item = (DupLineItem)dupset[nStart + i]; // changed

                DupSearchResult result_item = new DupSearchResult();
                results.Add(result_item);

                result_item.Path = item.Path;
                result_item.Weight = item.Weight;
                result_item.Threshold = item.Threshold;

                // paths[i] = item.Path;
                if (bCols == true)
                {
                    if (bExcludeCols == true && item.Weight < item.Threshold)
                    {
                    }
                    else
                        pathlist.Add(item.Path);
                }
            }

            if (pathlist.Count > 0)
            {
                // string[] paths = new string[pathlist.Count];
                string[] paths = StringUtil.FromListString(pathlist);

                ArrayList aRecord = null;

                nRet = channel.GetBrowseRecords(paths,
                    "cols",
                    out aRecord,
                    out strError);
                if (nRet == -1)
                {
                    strError = "GetBrowseRecords() error: " + strError;
                    goto ERROR1;
                }

                int j = 0;
                for (int i = 0; i < results.Count; i++)
                {
                    DupSearchResult result_item = results[i];
                    if (result_item.Path != pathlist[j])
                        continue;

                    string[] cols = (string[])aRecord[j];

                    results[i].Cols = cols;   // style中不包含id
                    j++;
                    if (j >= pathlist.Count)
                        break;
                }
            }

            searchresults = new DupSearchResult[results.Count];
            results.CopyTo(searchresults);

            result.Value = searchresults.Length;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        // 列出查重方案信息
        // parameters:
        //      strOriginBiblioDbName  发起的书目库名
        public LibraryServerResult ListDupProjectInfos(
            string strOriginBiblioDbName,
            out DupProjectInfo[] results)
        {
            // string strError = "";
            results = null;

            LibraryServerResult result = new LibraryServerResult();

            XmlNodeList nodes = null;

            if (String.IsNullOrEmpty(strOriginBiblioDbName) == true)
            {
                // 所有<project>元素
                nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//dup/project");
            }
            else
            {
                // 所有包含指定数据库名的<project>元素
                nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("//dup/project[./database[@name='" + strOriginBiblioDbName + "']]");
            }

            results = new DupProjectInfo[nodes.Count];
            for (int i = 0; i < results.Length; i++)
            {
                DupProjectInfo dpi = new DupProjectInfo();
                dpi.Name = DomUtil.GetAttr(nodes[i], "name");
                dpi.Comment = DomUtil.GetAttr(nodes[i], "comment");

                results[i] = dpi;
            }

            result.Value = results.Length;
            return result;
            /*
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
             * */
        }

        // 进行查重
        // parameters:
        //      sessioninfo 仅仅用来存放DupResultSet，不应该用来sessioninfo.GetChannel()，而要用channel来进行检索操作
        //      channel
        //      strOriginBiblioRecPath  发起的书目记录路径
        //      strOriginBiblioRecXml   发起的书目记录XML
        //      strProjectName  查重方案名
        //      strStyle    includeoriginrecord输出结果中包含发起记录(缺省为不包含)
        // return:
        //      -1  error
        //      0   not found
        //      其他    命中记录条数
        public LibraryServerResult SearchDup(
            SessionInfo sessioninfo1,
            RmsChannel channel,
            string strOriginBiblioRecPath,
            string strOriginBiblioRecXml,
            string strProjectName,
            string strStyle,
            out string strUsedProjectName)
        {
            string strError = "";
            int nRet = 0;
            strUsedProjectName = "";

            string strDebugInfo = "";

            strStyle = strStyle.ToLower();
            bool bIncludeOriginRecord = StringUtil.IsInList("includeoriginrecord", strStyle);

            LibraryServerResult result = new LibraryServerResult();

            // 如果没有给出方案名，则需要在<default>元素中找到一个书目库的缺省查重方案
            if (String.IsNullOrEmpty(strProjectName) == true)
            {
                if (String.IsNullOrEmpty(strOriginBiblioRecPath) == true)
                {
                    strError = "既没有给出查重方案名，也没有给出记录路径，无法进行查重";
                    goto ERROR1;
                }
                string strOriginBiblioDbName = ResPath.GetDbName(strOriginBiblioRecPath);

                XmlNode nodeDefault = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//dup/default[@origin='" + strOriginBiblioDbName + "']");
                if (nodeDefault == null)
                {
                    strError = "在没有明确指定查重方案名的情况下，本希望通过相关书目库的缺省查重方案名进行查重。但目前系统没有为书目库 '" + strOriginBiblioDbName + "' 定义缺省查重方案名，无法进行查重";
                    goto ERROR1;
                }

                string strDefaultProjectName = DomUtil.GetAttr(nodeDefault, "project");
                if (String.IsNullOrEmpty(strDefaultProjectName) == true)
                {
                    strError = "书目库 '" + strOriginBiblioDbName + "' 的<default>元素中未定义project属性值";
                    goto ERROR1;
                }

                strProjectName = strDefaultProjectName;
            }

            strUsedProjectName = strProjectName;

            XmlNode nodeProject = null;
            // 获得查重方案定义节点
            // return:
            //      -1  出错
            //      0   not found
            //      1   found
            nRet = GetDupProjectNode(strProjectName,
                out nodeProject,
                out strError);
            if (nRet == 0 || nRet == -1)
                goto ERROR1;

            Debug.Assert(nodeProject != null, "");

            DupResultSet alldatabase_set = null;    // 所有库的结果集

            XmlNodeList nodeDatabases = nodeProject.SelectNodes("database");

            // 循环，针对每个数据库进行检索
            for (int i = 0; i < nodeDatabases.Count; i++)
            {
                XmlNode nodeDatabase = nodeDatabases[i];
                string strDatabaseName = DomUtil.GetAttr(nodeDatabase, "name");
                string strThreshold = DomUtil.GetAttr(nodeDatabase, "threshold");
                int nThreshold = 0;
                try
                {
                    nThreshold = Convert.ToInt32(strThreshold);
                }
                catch
                {
                }

                List<AccessKeyInfo> aKeyLine = null;
                // 模拟创建检索点，以获得检索点列表
                // return:
                //      -1  error
                //      0   succeed
                nRet = GetKeys(
                    // sessioninfo.Channels,
                    channel,
                    strOriginBiblioRecPath,
                    strOriginBiblioRecXml,
                    out aKeyLine,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                DupResultSet onedatabase_set = null;    // 一个库的结果集
                try
                {
                    XmlNodeList accesspoints = nodeDatabase.SelectNodes("accessPoint");
                    // <accessPoint>循环
                    for (int j = 0; j < accesspoints.Count; j++)
                    {
                        XmlNode accesspoint = accesspoints[j];

                        string strFrom = DomUtil.GetAttr(accesspoint, "name");

                        // 获得from所对应的key
                        List<string> keys = GetKeyByFrom(aKeyLine,
                            strFrom);
                        if (keys.Count == 0)
                            continue;

                        string strWeight = DomUtil.GetAttr(accesspoint, "weight");
                        string strSearchStyle = DomUtil.GetAttr(accesspoint, "searchStyle");

                        int nWeight = 0;
                        try
                        {
                            nWeight = Convert.ToInt32(strWeight);
                        }
                        catch
                        {
                            // 警告定义问题?
                        }

                        for (int k = 0; k < keys.Count; k++)
                        {
                            string strKey = (string)keys[k];
                            if (strKey == "")
                                continue;

                            DupResultSet dupset = null;
                            try
                            {
                                // 针对一个from进行检索
                                // return:
                                //      -1  error
                                //      0   not found
                                //      1   found
                                nRet = SearchOneFrom(
                                    // sessioninfo.Channels,
                                    channel,
                                    strDatabaseName,
                                    strFrom,
                                    strKey,
                                    strSearchStyle,
                                    nWeight,
                                    nThreshold,
                                    5000,   // ???
                                    (bIncludeOriginRecord == false) ? strOriginBiblioRecPath : null,
                                    out dupset,
                                    out strError);

                                if (nRet == -1)
                                {
                                    // ??? 警告检索错误?
                                    continue;
                                }

                                if (onedatabase_set == null)
                                {
                                    onedatabase_set = dupset;
                                    dupset = null;  // 避免出 try 范围时被释放。因为内容已经转移给 onedatabase_set 了
                                    continue;
                                }

                                if (nRet == 0)
                                    continue;

                                Debug.Assert(dupset != null, "");

                                if (onedatabase_set.Sorted == true)
                                    onedatabase_set.EnsureCreateIndex(getTempFileName);
                                else
                                    onedatabase_set.Sort(getTempFileName);
                                // dupset.EnsureCreateIndex(getTempFileName);
                                // 2017/4/14
                                dupset.Sort(getTempFileName);   // Sort() 里面自动确保了创建 Index

                                // 将dupset和前一个set归并
                                // 归并可以参考ResultSet中的Merge算法
                                DupResultSet tempset = new DupResultSet();
                                tempset.Open(false, getTempFileName);
                                // 功能: 合并两个数组
                                // parameters:
                                //		strStyle	运算风格 OR , AND , SUB
                                //		sourceLeft	源左边结果集
                                //		sourceRight	源右边结果集
                                //		targetLeft	目标左边结果集
                                //		targetMiddle	目标中间结果集
                                //		targetRight	目标右边结果集
                                //		bOutputDebugInfo	是否输出处理信息
                                //		strDebugInfo	处理信息
                                // return
                                //		-1	出错
                                //		0	成功
                                nRet = DupResultSet.Merge("OR",
                                    onedatabase_set,
                                    dupset,
                                    null,   // targetLeft,
                                    tempset,
                                    null,   // targetRight,
                                    false,
                                    out strDebugInfo,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                                {
                                    if (onedatabase_set != null)
                                        onedatabase_set.Dispose();
                                    onedatabase_set = tempset;
                                    onedatabase_set.Sorted = true;  // 归并后产生的结果集自然是符合顺序的
                                }

                            }
                            finally
                            {
                                if (dupset != null)
                                    dupset.Dispose();
                            }
                        } // end of k loop

                    } // end of j loop


                    if (alldatabase_set == null)
                    {
                        alldatabase_set = onedatabase_set;
                        onedatabase_set = null; // 避免出 try 范围时被释放。因为内容已经转移给 alldatabase_set 了
                        continue;
                    }

                    // 合并
                    if (onedatabase_set != null)
                    {
                        DupResultSet tempset0 = new DupResultSet();
                        tempset0.Open(false, getTempFileName);

                        if (alldatabase_set.Sorted == true)
                            alldatabase_set.EnsureCreateIndex(getTempFileName);
                        else
                            alldatabase_set.Sort(getTempFileName);
                        // onedatabase_set.EnsureCreateIndex(getTempFileName);
                        // 2017/4/14
                        onedatabase_set.Sort(getTempFileName);   // Sort() 里面自动确保了创建 Index

                        nRet = DupResultSet.Merge("OR",
                            alldatabase_set,
                            onedatabase_set,
                            null,   // targetLeft,
                            tempset0,
                            null,   // targetRight,
                            false,
                            out strDebugInfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        {
                            if (alldatabase_set != null)
                                alldatabase_set.Dispose();

                            alldatabase_set = tempset0;
                            alldatabase_set.Sorted = true;
                        }
                    }
                }
                finally
                {
                    if (onedatabase_set != null)
                        onedatabase_set.Dispose();
                }
            }

            // 最后要按照 Weight和Threshold的差额 对结果集进行排序，便于输出
            if (alldatabase_set != null)
            {
                alldatabase_set.SortStyle = DupResultSetSortStyle.OverThreshold;
                alldatabase_set.Sort(getTempFileName);
            }

            {
                if (sessioninfo1.DupResultSet != null)
                    sessioninfo1.DupResultSet.Dispose();
                sessioninfo1.DupResultSet = alldatabase_set;
            }

            if (alldatabase_set != null)
                result.Value = alldatabase_set.Count;
            else
                result.Value = 0;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            result.ErrorInfo = strError;
            return result;
        }

        string getTempFileName()
        {
            Debug.Assert(string.IsNullOrEmpty(this.TempDir) == false, "");
            return Path.Combine(this.TempDir, "~dup_" + Guid.NewGuid().ToString());
        }

        // 针对一个from进行检索
        // parameters:
        //      strExcludeBiblioRecPath 要排除掉的记录路径
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int SearchOneFrom(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strDbName,
            string strFrom,
            string strKey,
            string strSearchStyle,
            int nWeight,
            int nThreshold,
            long nMax,
            string strExcludeBiblioRecPath,
            out DupResultSet dupset,
            out string strError)
        {
            strError = "";
            dupset = null;
            long lRet = 0;

            if (strSearchStyle == "")
                strSearchStyle = "exact";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>" + strSearchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

            string strSearchReason = "key='" + strKey + "', from='" + strFrom + "', weight=" + Convert.ToString(nWeight);

            /*
            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
             * */
            Debug.Assert(channel != null, "");

            lRet = channel.DoSearch(strQueryXml,
                "dup",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
                return 0;   // not found

            long lHitCount = lRet;

            long lStart = 0;
            long lPerCount = Math.Min(50, lHitCount);
            List<string> aPath = null;

            dupset = new DupResultSet();
            dupset.Open(false, getTempFileName);

            // 获得结果集，对逐个记录进行处理
            for (; ; )
            {
                // TODO: 中间要可以中断

                lRet = channel.DoGetSearchResult(
                    "dup",   // strResultSetName
                    lStart,
                    lPerCount,
                    "zh",
                    null,   // stop
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "未命中";
                    break;  // ??
                }

                // TODO: 要判断 aPath.Count == 0 跳出循环。否则容易进入死循环

                // 处理浏览结果
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strPath = aPath[i];

                    // 忽略发起记录的路径
                    if (strPath == strExcludeBiblioRecPath)
                        continue;

                    DupLineItem item = new DupLineItem();
                    item.Path = strPath;
                    item.Weight = nWeight;
                    item.Threshold = nThreshold;
                    dupset.Add(item);

                }

                lStart += aPath.Count;
                if (lStart >= lHitCount || lPerCount <= 0)
                    break;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 从模拟keys中根据from获得对应的key
        static List<string> GetKeyByFrom(List<AccessKeyInfo> aKeyLine,
            string strFromName)
        {
            List<string> aResult = new List<string>();
            for (int i = 0; i < aKeyLine.Count; i++)
            {
                AccessKeyInfo info = (AccessKeyInfo)aKeyLine[i];
                if (info.FromName == strFromName)
                    aResult.Add(info.Key);
            }

            return aResult;
        }

        // 模拟创建检索点
        // return:
        //      -1  error
        //      0   succeed
        public int GetKeys(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strPath,
            string strXml,
            out List<AccessKeyInfo> aLine,
            out string strError)
        {
            strError = "";
            aLine = null;

            /*
            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
             * */
            Debug.Assert(channel != null, "");

            long lRet = channel.DoGetKeys(
                strPath,
                strXml,
                "zh",	// strLang
                // "",	// strStyle
                null,	// this.stop,
                out aLine,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            return 0;
        }

        // 获得查重方案定义节点
        // return:
        //      -1  出错
        //      0   not found
        //      1   found
        int GetDupProjectNode(string strProjectName,
            out XmlNode node,
            out string strError)
        {
            strError = "";
            node = null;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//dup");
            if (root == null)
            {
                strError = "library.xml中尚未定义<dup>元素以及下属元素";
                return -1;
            }

            node = root.SelectSingleNode("project[@name='" + strProjectName + "']");
            if (node == null)
            {
                strError = "查重方案 '" + strProjectName + "' 的定义不存在";
                return 0;
            }

            return 1;
        }

    }

    // 查重方案信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class DupProjectInfo
    {
        [DataMember]
        public string Name = "";
        [DataMember]
        public string Comment = "";
    }

    // 查重检索命中结果的一行
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class DupSearchResult
    {
        [DataMember]
        public string Path = "";    // 记录路径
        [DataMember]
        public int Weight = 0;  // 权值
        [DataMember]
        public int Threshold = 0;   // 阈值
        [DataMember]
        public string[] Cols = null;    // 其余的列。一般为题名、作者，或者书目摘要
    }
}
