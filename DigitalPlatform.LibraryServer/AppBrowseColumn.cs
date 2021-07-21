using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using DigitalPlatform.IO;
using DigitalPlatform.rms;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    // TODO: 如何清除 _xpath_table 和 _browse_table 缓存? 当检测到 browse 配置文件被修改的时候? 或者休眠一段时间后自动清除?
    /// <summary>
    /// 和创建 GetSearchResult() 浏览列有关的功能
    /// </summary>
    public partial class LibraryApplication
    {
        public const string FILTERED = "[滤除]";

        Hashtable _xpath_table = new Hashtable();

        // 得到一条记录的浏览格式，一个字符串数组
        // parameter:
        //      strFormat   浏览格式定义。如果为空，表示 相当于 cfgs/browse
        //                  如果有 @coldef: 引导，表示为 XPath 形态的列定义，例如 @def://parent|//title ，竖线表示列的间隔
        //                  否则表示 browse 配置文件的名称，例如 cfgs/browse_temp 之类
        //		strRecordID	一般自由位数的记录号，或10位数字的记录号
        //      strXml  记录体。如果为空，本函数会自动获取记录体
        //      nStartCol   开始的列号。一般为0
        //		cols	out参数，返回浏览格式数组
        // 当出错时,错误信息也保存在列里
        // return:
        //      // cols中包含的字符总数
        //      -1  出错
        //      0   记录没有找到
        //      其他  cols 中包含的字符总数。最少为 1，不会为 0
        public int GetCols(
            RmsChannel channel,
            string strFormat,
            string strDbName,
            // string strRecordID,
            string strXml,
            int nStartCol,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = null;
            int nRet = 0;

            if (string.IsNullOrEmpty(strFormat) == true)
                strFormat = "cfgs/browse";

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
                // 例如 中文图书/cfgs/browse
                string strBrowseName = strDbName + "/" + strFormat;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.GetBrowseCfg(
                    channel,
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
                strError = "strXml 不应为空";
                goto ERROR1;
            }

            XmlDocument domData = new XmlDocument();
            try
            {
                domData.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                // strError = $"加载 '{strDbName}' 库的 '{ strRecordID }' 记录到 dom 时出错,原因: " + ex.Message;
                strError = $"加载数据 XML 记录到 XMLDOM 时出错: {ex.Message}";
                goto ERROR1;
            }

            if (browseCfg != null)
            {
                // return:
                //		-1	出错
                //		>=0	成功。数字值代表每个列包含的字符数之和
                nRet = browseCfg.BuildCols(domData,
                    nStartCol,
                    out cols,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 列定义

                // 预防 Hashtable 变得过大
                lock (_xpath_table.SyncRoot)
                {
                    if (_xpath_table != null && _xpath_table.Count > 1000)
                    {
                        _xpath_table.Clear();
                    }
                }

                nRet = BuildCols(
                    // ref m_xpath_table,
                    strColDef,
                    domData,
                    nStartCol,
                    out cols,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return Math.Max(1, nRet);   // 避免返回 0 和没有找到的情况混淆
        ERROR1:
            if (cols == null || cols.Length == 0)
            {
                cols = new string[1];
                cols[0] = "error: " + strError;
            }
            return -1;
        }

        // browse 配置文件名和 BrowseCfg 对象之间的缓存
        // key: browseName, value: BrowseCfg
        Hashtable _browse_table = new Hashtable();

        // 得到浏览格式内存对象
        // parameters:
        //      strBrowseName   浏览文件的文件名或者全路径
        //                      例如 cfgs/browse 。因为已经是在特定的数据库中获得配置文件，所以无需指定库名部分
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetBrowseCfg(
            RmsChannel channel,
            // string strDbName,
            string strBrowseName,
            out BrowseCfg browseCfg,
            out string strError)
        {
            strError = "";
            browseCfg = null;

            strBrowseName = strBrowseName.ToLower();

            lock (_browse_table.SyncRoot)
            {
                browseCfg = (BrowseCfg)this._browse_table[strBrowseName];
            }
            if (browseCfg != null)
            {
                return 1;
            }

            string strBrowsePath = strBrowseName;

            string strBrowseFileName = this.GetTempFileName("_browse_");

            try
            {
                // 获取已有的配置文件对象
                byte[] timestamp = null;
                string strOutputPath = "";
                string strMetaData = "";

                // string strStyle = "content,data,metadata,timestamp,outputpath";
                long lRet = channel.GetRes(
                    strBrowsePath,
                    strBrowseFileName,
                    null,   // stop,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    /*
                    // 配置文件不存在，怎么返回错误码的?
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        timestamp = null;
                    }
                    */
                    return -1;
                }

                /*
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
                */

                browseCfg = new BrowseCfg();
                int nRet = browseCfg.Initial(strBrowseFileName,
                    this.BinDir,
                    out strError);
                if (nRet == -1)
                {
                    browseCfg = null;
                    return -1;
                }

                lock (_browse_table.SyncRoot)
                {
                    this._browse_table[strBrowseName] = browseCfg;
                }
                return 1;
            }
            finally
            {
                File.Delete(strBrowseFileName);
            }
        }

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
        int BuildCols(
        // ref Hashtable xpath_table,
        string strColDef,
        XmlDocument domData,
        int nStartCol,
        out string[] cols,
        out string strError)
        {
            strError = "";
            cols = new string[0];

            Debug.Assert(domData != null, "BuildCols()调用错误，domData参数不能为null。");

            if (_xpath_table == null)
                _xpath_table = new Hashtable();

            int nResultLength = 0;

            string[] xpaths = strColDef.Split(new char[] { '|' });

            XPathNavigator nav = domData.CreateNavigator();

            List<string> col_array = new List<string>();

            for (int i = 0; i < xpaths.Length; i++)
            {
                string strSegment = xpaths[i];

                XPathInfo info = ParseSegment(strSegment);

                // 2017/5/11
                XmlNamespaceManager nsmgr = null;
                if (string.IsNullOrEmpty(info.NameList) == false)
                {
                    lock (_xpath_table.SyncRoot)
                    {
                        nsmgr = (XmlNamespaceManager)_xpath_table[info.NameList];
                    }

                    if (nsmgr == null)
                    {
                        nsmgr = BuildNamespaceManager(info.NameList, domData);
                        lock (_xpath_table.SyncRoot)
                        {
                            _xpath_table[info.NameList] = nsmgr;
                        }
                    }
                }

                XPathExpression expr = null;
                lock (_xpath_table.SyncRoot)
                {
                    expr = (XPathExpression)_xpath_table[info.XPath];
                }
                // TODO: nsmgr_table

                if (expr == null)
                {
                    // 创建Cache
                    try
                    {
                        expr = nav.Compile(info.XPath);
                        if (nsmgr != null)
                            expr.SetContext(nsmgr);

                        lock (_xpath_table.SyncRoot)
                        {
                            _xpath_table[info.XPath] = expr;
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
            cols = new string[col_array.Count + nStartCol];
            col_array.CopyTo(cols, nStartCol);
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

    }
}
