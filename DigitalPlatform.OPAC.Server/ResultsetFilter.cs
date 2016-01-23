using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Threading;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.ResultSet;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.rms;  // rmsutil

// using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 实现分面剖析的过滤功能
    /// </summary>
    public class ResultsetFilter
    {

#if NO
        // 获得名称对照表
        // parameters:
        //      keyname_table   keyname --> 当前语言的名称
        public static int GetKeyNameTable(
    OpacApplication app,
    string strFilterFileName,
    List<NodeInfo> nodeinfos,
    string strLang,
    out Hashtable keyname_table,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            keyname_table = new Hashtable();

            FilterHost host = new FilterHost();

            LoanFilterDocument filter = null;

            nRet = app.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                nRet = BuildKeyNameTable(
    filter,
    nodeinfos,
    strLang,
    out keyname_table,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "GetKeyNameTable() error: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                // 归还对象
                app.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // TODO: 如何中断长操作?
        // parameters:
        //      strFacetDefXml   分面特性定义XML片断
        //      nMaxRecords     处理的最多记录数。如果 == -1，表示不限制
        public static int DoFilter(
            OpacApplication app,
            LibraryChannel channel,
            string strResultsetName,
            string strFilterFileName,
            int nMaxRecords,
            SetProgress setprogress,
            ref Hashtable result_table,
            out long lOuputHitCount,
            // out string strFacetDefXml,
            out string strError)
        {
            strError = "";
            lOuputHitCount = 0;
            long lRet = 0;
            int nRet = 0;
            // strFacetDefXml = "";

            if (result_table == null)
                result_table = new Hashtable();

            FilterHost host = new FilterHost();

            LoanFilterDocument filter = null;

            nRet = app.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                /*
                XmlNode node = filter.Dom.DocumentElement.SelectSingleNode("facetDef");
                if (node != null)
                {
                    strFacetDefXml = node.OuterXml;
                }
                 * */

                long lStart = 0;

                // int nCount = 0;
                for (; ; )
                {
                    Record[] records = null;
                    // return:
                    //      -1  出错
                    //      >=0 结果集内记录的总数(注意，并不是本批返回的记录数)
                    lRet = channel.GetSearchResult(
                        null,
                        "#" + strResultsetName,
                        lStart,
                        100,
                        "id,xml",
                        "zh",
                        out records,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    lOuputHitCount = lRet;
                    long lCount = lRet;

                    if (nMaxRecords != -1)
                        lCount = Math.Min(lCount, nMaxRecords);

                    if (setprogress != null)
                        setprogress(lCount, lStart);

                    foreach (Record record in records)
                    {
                        // 2014/12/21
                        if (string.IsNullOrEmpty(record.RecordBody.Xml) == true)
                            continue;

                        // 如果必要,转换为MARC格式,调用filter
                        string strOutMarcSyntax = "";
                        string strMarc = "";
                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(record.RecordBody.Xml,
                            true,
                            "", // this.CurMarcSyntax,
                            out strOutMarcSyntax,
                            out strMarc,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "转换书目记录 " + record.Path + " 的 XML 格式到 MARC 时出错: " + strError;
                            goto ERROR1;
                        }

                        host.RecPath = record.Path;
                        host.App = app;
                        host.ResultParams = new KeyValueCollection();

                        nRet = filter.DoRecord(null,
                            strMarc,
                            strOutMarcSyntax,
                            0,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        nRet = AddItems(ref result_table,
                            host.ResultParams,
                            record.Path,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    if (records.Length <= 0  // < 100
                        )
                        break;

                    lStart += records.Length;
                    // nCount += records.Length;

                    if (lStart >= lCount)
                    {
                        if (setprogress != null)
                            setprogress(lCount, lCount);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "filter.DoRecord error: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                // 归还对象
                filter.FilterHost = null;   // 2016/1/23
                app.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

#if NO
        static int BuildKeyNameTable(
            LoanFilterDocument filter,
            List<NodeInfo> nodeinfos,
            string strLang,
            out Hashtable keyname_table,
            out string strError)
        {
            strError = "";

            keyname_table = new Hashtable();

            foreach (NodeInfo node in nodeinfos)
            {
                // 只关注一级节点
                string key = node.Name;
                keyname_table[key] = filter.GetString(strLang, key);
            }

            keyname_table["初始结果集"] = filter.GetString(strLang, "初始结果集");

            return 0;
        }
#endif

        // 分选事项
        // result_table   keyname --> KeyValueCollection
        // 其中 KeyValueCollection 存储了检索点和记录路径对照信息，例如 作者名 --> 记录路径
        static int AddItems(ref Hashtable result_table,
            KeyValueCollection items,
            string strRecPath,
            out string strError)
        {
            strError = "";

            foreach (KeyValue item in items)
            {
                KeyValueCollection resultset = (KeyValueCollection)result_table[item.Key];
                if (resultset == null)
                {
                    resultset = new KeyValueCollection();
                    result_table[item.Key] = resultset;
                }

                resultset.Add(item.Value, strRecPath);
            }

            return 0;
        }

        // 将内存数组写入结果集文件
        public static int WriteToResulsetFile(
            KeyValueCollection items,
            string strResultsetFilename,
            out string strError)
        {
            strError = "";

            DpResultSet resultset = new DpResultSet(false, false);
            resultset.Create(strResultsetFilename,
                strResultsetFilename + ".index");

            bool bDone = false;
            try
            {
                foreach (KeyValue item in items)
                {
                    DpRecord record = new DpRecord(item.Value);
                    record.BrowseText = item.Key;
                    resultset.Add(record);
                }
                bDone = true;
                return 0;
            }
            finally
            {
                if (bDone == true)
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }
                else
                {
                    // 否则文件会被删除
                    resultset.Close();
                }
            }
        }

        // 将全部事项写入结果集
        // parameters:
        //      aggregation_names   需要创建二级聚类的名称数组
        //      strOutputDir  存储结果集文件的目录
        //      resultset_filenames [out]新创建的结果集文件名。注意，每个文件名对应两个物理文件
        public static int BuildResultsetFile(Hashtable result_table,
            XmlDocument def_dom,
            // List<string> aggregation_names,
            string strOutputDir,
            // out List<string> resultset_filenames,
            out List<NodeInfo> output_items,
            out string strError)
        {
            output_items = new List<NodeInfo>();
            strError = "";
            // resultset_filenames = new List<string>();
            int nRet = 0;

            string[] names = new string[result_table.Keys.Count];
            result_table.Keys.CopyTo(names, 0);
            Array.Sort(names);

            foreach (string strName in names)
            {
                KeyValueCollection items = (KeyValueCollection)result_table[strName];

                NodeInfo info = new NodeInfo();
                output_items.Add(info);
                info.Name = strName;
                if (items != null)
                    info.Count = items.Count;
                else
                    info.Count = 0;

                string  strAggregation = "";
                if (def_dom != null && def_dom.DocumentElement != null)
                {
                    XmlNode node = def_dom.DocumentElement.SelectSingleNode("facet[@id='" + strName + "']");
                    if (node != null)
                    {
                        strAggregation = DomUtil.GetAttr(node, "aggregation");
                    }
                }

                if (items.Count > 0)
                {
                    // 排序
                    items.Sort(new KeyValueComparer());
                    // 写入结果集文件
                    string strPureFilename = Guid.NewGuid().ToString();
                    info.ResultSetPureName = strPureFilename;
                    string strResultsetFileName = PathUtil.MergePath(strOutputDir, strPureFilename);

                    nRet = WriteToResulsetFile(items,
                        strResultsetFileName, 
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 如果需要归并后构建结果集分段信息
                    if (string.IsNullOrEmpty(strAggregation) == false)
                    {
                        string strSubNodePureName = strPureFilename + "_sub";
                        string strSubNodeFileName = PathUtil.MergePath(strOutputDir, strSubNodePureName);

                        nRet = BuildSegments(
                            strSubNodeFileName,
                            strSubNodePureName,
                            info,
                            items,
                            strAggregation == "yes" ? null : strAggregation,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

            return 0;
        }

        static string ConvertKey(string strConvertStyle, string strText)
        {
            if (strConvertStyle == "publishyear")
            {
                strText = strText.Replace("[", "").Replace("]", "").Replace("［", "").Replace("］", "");
                int nRet = strText.IndexOf(".");
                if (nRet != -1)
                    return strText.Substring(0, nRet);
                return strText;
            }
            if (strConvertStyle == "class1")
            {
                if (string.IsNullOrEmpty(strText) == true)
                    return strText;
                return strText.Substring(0, 1);
            }

            return strText;
        }

        // 创建二级节点
        // parameters:
        //      strPureFilename 一级节点的结果集纯文件名。用于构造定位信息
        //      strConverStyle  对 key 字符串的加工方法
        static int BuildSegments(
            string strSubNodeFilename,
            string strPureFilename,
            NodeInfo info,
            KeyValueCollection items,
            string strConvertStyle,
            out string strError)
        {
            strError = "";

            List<Segment> segments = new List<Segment>();
            KeyValue prev_item = null;
            int nSegmentCount = 0;  // 当前段落中的个数
            int i = 0;
            foreach (KeyValue item in items)
            {
                if (string.IsNullOrEmpty(strConvertStyle) == false)
                    item.Key = ConvertKey(strConvertStyle, item.Key);

                if (prev_item == null)
                    goto CONTINUE;

                if (prev_item.Key == item.Key)
                {
                }
                else
                {
                    if (nSegmentCount > 0)
                    {
                        // 创建一段
                        segments.Add(new Segment(prev_item.Key, i - nSegmentCount, nSegmentCount));
                        nSegmentCount = 0;
                    }
                }

            CONTINUE:
                prev_item = item;
                i++;
                nSegmentCount++;
            }

            // 最后一段
            if (nSegmentCount > 0)
            {
                segments.Add(new Segment(prev_item.Key, i - nSegmentCount, nSegmentCount));
            }

            info.SubCount = segments.Count;

            // 创建成一个一个的结果集分段观察命令，写入结果集文件
            DpResultSet resultset = new DpResultSet(false, false);
            resultset.Create(strSubNodeFilename,
    strSubNodeFilename + ".index");

            bool bDone = false;
            try
            {

                foreach (Segment segment in segments)
                {
                    string strCommand = segment.Start.ToString() + "," + segment.Length.ToString();
                    DpRecord record = new DpRecord(strCommand);
                    record.BrowseText = segment.Name;
                    resultset.Add(record);
                }
                bDone = true;
                info.SubNodePureName = strPureFilename;
                return 0;
            }
            finally
            {
                if (bDone == true)
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }
                else
                {
                    // 否则文件会被删除
                    resultset.Close();
                }
            }
        }

        static void ParseSelectedPath(string strPath,
            out string strResultsetName,
            out string strOffset)
        {
            strResultsetName = "";
            strOffset = "";

            string [] parts = strPath.Split(new char []{'/'});
            if (parts.Length >= 1)
                strResultsetName = parts[0];
            if (parts.Length >= 2)
                strOffset = parts[1];
        }


        // 创建FilterInfo数组
        // parameters:
        //      strSelectedPath 选定状态的节点的路径。为 {resultsetname}/{offsetstring} 形态
        public static FilterInfo[] BuildFilterInfos(
            string strBaseResultsetName,
            long lBaseResultCount,
            string strSelectedPath,
            // Hashtable keyname_table,
            GetCaption func_getcaption,
            List<NodeInfo> nodeinfos,
            string strTempDir,
            int nMaxSubNodeCount)
        {
            List<FilterInfo> results = new List<FilterInfo>();

            string strSelectedResultsetName = "";
            string strSelectedOffset = "";
            ParseSelectedPath(strSelectedPath,
                out strSelectedResultsetName,
                out strSelectedOffset);

            if (string.IsNullOrEmpty(strBaseResultsetName) == false)
            {
                // 初始结果集
                FilterInfo info = new FilterInfo();
                results.Add(info);

                info.Name = func_getcaption("初始结果集");
                info.Count = lBaseResultCount.ToString();
                info.Url = "./searchbiblio.aspx?base=" + HttpUtility.UrlEncode(strBaseResultsetName);

                if (string.IsNullOrEmpty(strSelectedPath) == true)
                    info.Selected = true;
            }

            foreach (NodeInfo node in nodeinfos)
            {
                // 一级节点
                FilterInfo info = new FilterInfo();
                results.Add(info);

                // 将内部名字翻译为当前语言的名字
                info.Name = func_getcaption(node.Name);
                info.Count = node.Count.ToString();
                info.Url = "./searchbiblio.aspx?resultset=" + HttpUtility.UrlEncode(node.ResultSetPureName) + "&base=" + HttpUtility.UrlEncode(strBaseResultsetName) + "&title=" + HttpUtility.UrlEncode(RemoveHead(info.Name));
                if (string.IsNullOrEmpty(strSelectedOffset) == true)
                {
                    if (strSelectedResultsetName == node.ResultSetPureName)
                        info.Selected = true;
                }

                // 一级节点的下级导航信息
                info.Type = node.SubNodePureName + "," + node.SubCount + "," + node.SubStart;

                // 二级节点
                if (node.SubCount > 0 && string.IsNullOrEmpty(node.SubNodePureName) == false)
                {
                    string strResultsetFilename = PathUtil.MergePath(strTempDir, node.SubNodePureName);
                    DpResultSet resultset = new DpResultSet(false, false);

                    List<FilterInfo> sub_results = new List<FilterInfo>();

                    // 如果起始位置不是0
                    if (node.SubStart > 0)
                    {
                        FilterInfo sub_info = new FilterInfo();
                        sub_results.Add(sub_info);

                        int nStart = node.SubStart - nMaxSubNodeCount;
                        if (nStart < 0)
                            nStart = 0;
                        sub_info.Name = "前页...";
                        sub_info.Count = "";
                        sub_info.Url = node.SubNodePureName + "," + nStart.ToString();
                        sub_info.Type = "nav";  // 表示导航命令
                    }

                    int nTail = Math.Min(node.SubStart + nMaxSubNodeCount, node.SubCount);
                    resultset.Attach(strResultsetFilename,
                            strResultsetFilename + ".index");
                    try
                    {
                        for (int i = node.SubStart; i < nTail; i++)
                        {
                            DpRecord record = resultset[i];

                            string strOffset = record.ID;
                            string[] parts = strOffset.Split(new char[] { ',' });
                            if (parts.Length != 2)
                                continue;

                            FilterInfo sub_info = new FilterInfo();
                            sub_results.Add(sub_info);

                            sub_info.Index = (i + 1).ToString();
                            sub_info.Name = record.BrowseText;
                            sub_info.Count = parts[1];
                            sub_info.Url = "./searchbiblio.aspx?resultset=" + HttpUtility.UrlEncode(node.ResultSetPureName) + "&offset=" + strOffset + "&base=" + HttpUtility.UrlEncode(strBaseResultsetName) + "&title=" + HttpUtility.UrlEncode(RemoveHead(info.Name) + "/" + sub_info.Name); 

                            if (string.IsNullOrEmpty(strSelectedOffset) == false
                                && strSelectedResultsetName == node.ResultSetPureName && strSelectedOffset == strOffset)
                            {
                                sub_info.Selected = true;
                            }
                        }
                    }
                    finally
                    {
                        string strTemp1 = "";
                        string strTemp2 = "";
                        resultset.Detach(out strTemp1, out strTemp2);
                    }



                    // 如果没有显示完，则最后包含一个翻页或者延展的锚点
                    if (nTail < node.SubCount)
                    {
                        FilterInfo sub_info = new FilterInfo();
                        sub_results.Add(sub_info);

                        sub_info.Name = "更多...";
                        sub_info.Count = "";
                        // sub_info.Url = "./searchbiblio.aspx?resultset=" + HttpUtility.UrlEncode(node.SubNodePureName) + "&more=" + nMaxSubNodeCount.ToString();
                        sub_info.Url = node.SubNodePureName + "," + nTail.ToString();
                        sub_info.Type = "nav";  // 表示导航命令

                        if (strSelectedResultsetName == node.ResultSetPureName && strSelectedOffset == "nav")
                        {
                            sub_info.Selected = true;
                        }
                    }
                    else
                    {
                        // 如果“更多”没有显示出来，则把选择点定位到最后一个元素
                        if (strSelectedResultsetName == node.ResultSetPureName && strSelectedOffset == "nav"
                            && sub_results.Count > 0)
                        {
                            sub_results[sub_results.Count - 1].Selected = true;
                        }
                    }

                    info.Children = new FilterInfo[sub_results.Count];
                    sub_results.CopyTo(info.Children);
                }
            }

            // 对results中的对象进行排序
            results.Sort(new FilterInfoComparer());
            // 去掉名字中的{}部分
            foreach(FilterInfo info in results)
            {
                string strName = info.Name;
                info.Name = RemoveHead(strName);
            }

            FilterInfo[] a = new FilterInfo[results.Count];
            results.CopyTo(a);
            return a;
        }

        static string RemoveHead(string strName)
        {
            if (strName[0] == '{')
            {
                int nRet = strName.IndexOf("}");
                if (nRet != -1)
                {
                    return strName.Substring(nRet + 1);
                }
            }

            return strName;
        }

        public static int SwitchPage(ref List<NodeInfo> result_items,
            string strPureResultsetName,
            int nStart,
            out string strError)
        {
            strError = "";

            NodeInfo node_found = null;
            foreach (NodeInfo node in result_items)
            {
                if (node.ResultSetPureName == strPureResultsetName
                    || node.SubNodePureName == strPureResultsetName)
                {
                    node_found = node;
                    break;
                }
            }

            if (node_found == null)
            {
                strError = "结果集名为 '" + strPureResultsetName + "' 的 NodeInfo 节点没有找到";
                return -1;
            }

            node_found.SubStart = nStart;
            return 0;
        }
    }

    public delegate void SetProgress(long lProgressRange, long lProgressValue);

    // JSON辅助结构
    public class FilterInfo
    {
        public string Index = "";
        public string Name = "";
        public string Count = "";
        public string Url = "";
        public string Type = "";
        public bool Selected = false;
        public FilterInfo[] Children = null;
    }

    public class FilterInfoComparer : IComparer<FilterInfo>
    {
        int IComparer<FilterInfo>.Compare(FilterInfo x, FilterInfo y)
        {
            int nRet = String.Compare(x.Name, y.Name);
            if (nRet != 0)
                return nRet;

            return String.Compare(x.Name, y.Name);
        }

    }

    public class Segment
    {
        public string Name = "";
        public int Start = 0;
        public int Length = 0;

        public Segment(string strName,
            int nStart,
            int nLength)
        {
            this.Name = strName;
            this.Start = nStart;
            this.Length = nLength;
        }
    }

    // 节点长期存储结构
    public class NodeInfo
    {
        public string Name = "";
        public int Count = 0;
        public string ResultSetPureName = "";   // 结果集文件纯粹文件名部分

        public string SubNodePureName = "";     // 存储下级节点的结果集文件纯文件名部分
        public int SubCount = 0;    // 下级节点的个数
        public int SubStart = 0;    // 下级节点显示的开始位置
    }

    public delegate string GetCaption(string strID);

}
