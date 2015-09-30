using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和统计输出相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 将日期字符串解析为起止范围日期
        // throw:
        //      Exception
        public void ParseDateRange(string strText,
            out string strStartDate,
            out string strEndDate)
        {
            strStartDate = "";
            strEndDate = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                // 没有'-'

                if (strText.Length == 4)
                {
                    strStartDate = strText + "0101";
                    strEndDate = strText + "1231";
                    return;
                }

                if (strText.Length == 6)
                {
                    strStartDate = strText + "01";
                    DateTime start = DateTimeUtil.Long8ToDateTime(strStartDate);
                    DateTime end = start.AddMonths(1);
                    end = new DateTime(end.Year, end.Month, 1); // 下月1号
                    end = end.AddDays(-1);  // 上月最后一号

                    strEndDate = strText + end.Day;
                    return;
                }

                if (strText.Length == 8)
                {
                    // 单日
                    strStartDate = strText;
                    strEndDate = "";
                    return;
                }

                // text-level: 用户提示
                throw new Exception(
                    string.Format(this.GetString("日期字符串s格式不正确。应当为468字符"),   // "日期字符串 '{0}' 格式不正确。应当为4/6/8字符"
                    strText)
                    // "日期字符串 '" + strText + "' 格式不正确。应当为4/6/8字符"
                    );
            }
            else
            {
                string strLeft = "";
                string strRight = "";

                strLeft = strText.Substring(0, nRet).Trim();
                strRight = strText.Substring(nRet + 1).Trim();

                if (strLeft.Length != strRight.Length)
                {
                    // text-level: 用户提示
                    throw new Exception(
                        string.Format(this.GetString("日期字符串s格式不正确。横杠左边的部分s和右边的部分s字符数应相等"), // "日期字符串 '{0}' 格式不正确。横杠左边的部分 '{1}' 和右边的部分 '{2}' 字符数应相等。"
                        strText,
                        strLeft,
                        strRight)
                        // "日期字符串 '" + strText + "' 格式不正确。横杠左边的部分'"+strLeft+"' 和右边的部分'"+strRight+"' 字符数应相等。"
                        );
                }

                if (strLeft.Length == 4)
                {
                    strStartDate = strLeft + "0101";
                    strEndDate = strRight + "1231";
                    return;
                }

                if (strLeft.Length == 6)
                {
                    strStartDate = strLeft + "01";

                    DateTime start = DateTimeUtil.Long8ToDateTime(strRight + "01");
                    DateTime end = start.AddMonths(1);
                    end = new DateTime(end.Year, end.Month, 1); // 下月1号
                    end = end.AddDays(-1);  // 上月最后一号

                    strEndDate = strRight + end.Day;
                    return;
                }

                if (strLeft.Length == 8)
                {
                    // 单日
                    strStartDate = strLeft;
                    strEndDate = strRight;
                    return;
                }

                // text-level: 用户提示
                throw new Exception(
                    string.Format(this.GetString("日期字符串s格式不正确。横杠左边或者右边的部分，应当为468字符"),   // "日期字符串 '{0}' 格式不正确。横杠左边或者右边的部分，应当为4/6/8字符"
                    strText)
                    // "日期字符串 '" + strText + "' 格式不正确。横杠左边或者右边的部分，应当为4/6/8字符"
                    );
            }
        }

        public int Exists(
    ref int nStop,
    string strDateRangeString,
    out List<DateExist> dates,
    out string strError)
        {
            strError = "";
            dates = new List<DateExist>();
            // int nRet = 0;

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                ParseDateRange(strDateRangeString,
                    out strStartDate,
                    out strEndDate);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            DateTime start;
            DateTime end;

            start = DateTimeUtil.Long8ToDateTime(strStartDate);

            if (strEndDate == "")
            {
                end = start;
                strEndDate = strStartDate;
            }
            else
            {
                end = DateTimeUtil.Long8ToDateTime(strEndDate);

                TimeSpan delta = end - start;
                if (delta.Days < 0)
                {
                    // 交换两个时间
                    DateTime temp = end;
                    end = start;
                    start = temp;
                }
            }

            int nCount = 0; // 实际存在多少天
            DateTime current = start;
            for (; ; )
            {
                if (nStop != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                string strDate = DateTimeUtil.DateTimeToString8(current);
                string strFilename = this.StatisDir + "\\" + strDate + ".xml";

                if (File.Exists(strFilename) == true)
                {
                    nCount++;

                    DateExist exist = new DateExist();
                    exist.Date = strDate;
                    exist.Exist = true;
                    dates.Add(exist);
                }

                if (current >= end)
                    break;

                current = current.AddDays(1);
            }

            return 0;
        }

        class OneItem
        {
            public string Path = "";    // 名字路径 catrgory name/item name
            public List<double> Values = new List<double>();    // 序列值
            public double Total = 0;    // 累加值

            public string GetValueString(ValueStyle style)
            {
                StringBuilder s = new StringBuilder(4096);
                if ((style & ValueStyle.Total) != 0)
                    s.Append(this.Total.ToString());
                if ((style & ValueStyle.List) != 0)
                {
                    foreach (double v in this.Values)
                    {
                        if (s.Length > 0)
                            s.Append(",");
                        if (v != 0)
                            s.Append(v.ToString());
                    }
                }

                return s.ToString();
            }
        }

        enum ValueStyle
        {
            None = 0x00,
            Total = 0x01,
            List = 0x02,
        }

        // 将一个DOM中的全部值合并到hashtable中
        // parameters:
        //      nDays   本次合并前，已经累加了多少天的数据
        static int MergeValues(
            string strLibraryCodeList,
            Hashtable table,
            XmlDocument dom,
            long nDays,
            ValueStyle valuestyle,
            out string strError)
        {
            strError = "";

            if (dom == null)
            {
                if ((valuestyle & ValueStyle.List) == 0)
                    return 0;

                // 为每个列表增加一个0
                foreach (string path in table.Keys)
                {
                    OneItem info = (OneItem)table[path];
                    info.Values.Add(0);
                }

                return 0;
            }

            List<string> touched_keys = new List<string>();


            // 根下的全部<category>
            int nRet = MergeValues(
                table,
                dom.DocumentElement,
                nDays,
                valuestyle,
                ref touched_keys,
                out strError);
            if (nRet == -1)
                return -1;

            // <library>元素下的<category>
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false
                    && StringUtil.IsInList(strCode, strLibraryCodeList) == false)
                    continue;

                nRet = MergeValues(
                    table,
                    node,
                    nDays,
                    valuestyle,
                    ref touched_keys,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if ((valuestyle & ValueStyle.List) != 0)
            {
                // 2012/11/5
                // 为没有处理到的列表增加一个0
                foreach (string path in table.Keys)
                {
                    int index = touched_keys.IndexOf(path);
                    if (index != -1)
                    {
                        touched_keys.RemoveAt(index);   // 逐渐缩小数组，有利于加快速度
                        continue;
                    }
                    OneItem info = (OneItem)table[path];
                    info.Values.Add(0);
                }
            }

            return 0;
        }

        static int MergeValues(
            Hashtable table,
            XmlNode root,
            long nDays,
            ValueStyle valuestyle,
            ref List<string> touched_keys,
            out string strError)
        {
            strError = "";

            string strCode = DomUtil.GetAttr(root, "code");

            XmlNodeList nodes = root.SelectNodes("category");
            foreach (XmlNode node in nodes)
            {
                string strCategoryName = DomUtil.GetAttr(node, "name");
                XmlNodeList items = node.SelectNodes("item");
                foreach (XmlNode item in items)
                {
                    string strItemName = DomUtil.GetAttr(item, "name");

                    string strPath = strCode.Replace("/", "\\") + "/" + strCategoryName.Replace("/", "\\") + "/" + strItemName.Replace("/", "\\");  // 2015/4/3 替换了字符
                    touched_keys.Add(strPath);

                    OneItem info = (OneItem)table[strPath];
                    if (info == null)
                    {
                        info = new OneItem();
                        info.Path = strPath;
                        table[strPath] = info;

                        // 补齐以前欠缺的list值
                        if ((valuestyle & ValueStyle.List) != 0)
                        {
                            for (long i = 0; i < nDays; i++)
                            {
                                info.Values.Add(0);
                            }
                        }
                    }

                    string strItemValue = DomUtil.GetAttr(item, "value");
                    double v = 0;

                    if (double.TryParse(strItemValue, out v) == false)
                    {
                        strError = "XML片断 '"+item.OuterXml+"' 中value属性值 '"+strItemValue+"' 格式错误";
                        return -1;
                    }

                    info.Total += v;

                    if ((valuestyle & ValueStyle.List) != 0)
                        info.Values.Add(v);
                }
            }
            return 0;
        }

        // 把Hashtable中的值写入XML文件中
        static int WriteToXmlFile(
            Hashtable table,
            string strOutputFilename,
            ValueStyle style,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (string path in table.Keys)
            {
                string[] parts = path.Split(new char[] {'/'});
                if (parts.Length != 3)
                {
                    strError = "hashtable中路径 '"+parts+"' 格式错误。应为三段形态";
                    return -1;
                }
                /*
                // 2015/4/3
                if (parts.Length < 3)
                {
                    strError = "hashtable中路径 '" + path + "' 格式错误。应为至少三段形态";
                    return -1;
                }
                 * */

                string strCode = parts[0];
                string strCategoryName = parts[1];
                string strItemName = parts[2];

                XmlNode nodeLibrary = null;
                if (string.IsNullOrEmpty(strCode) == true)
                    nodeLibrary = dom.DocumentElement;
                else
                {
                    nodeLibrary = dom.DocumentElement.SelectSingleNode("library[@code='" + strCode + "']");
                    if (nodeLibrary == null)
                    {
                        nodeLibrary = dom.CreateElement("library");
                        dom.DocumentElement.AppendChild(nodeLibrary);
                        DomUtil.SetAttr(nodeLibrary, "code", strCode);
                    }
                }

                XmlNode nodeCategory = nodeLibrary.SelectSingleNode("category[@name='" + strCategoryName + "']");
                if (nodeCategory == null)
                {
                    nodeCategory = dom.CreateElement("category");
                    nodeLibrary.AppendChild(nodeCategory);
                    DomUtil.SetAttr(nodeCategory, "name", strCategoryName);
                }

                XmlNode nodeItem = nodeCategory.SelectSingleNode("item[@name='" + strItemName + "']");
                if (nodeItem == null)
                {
                    nodeItem = dom.CreateElement("item");
                    nodeCategory.AppendChild(nodeItem);
                    DomUtil.SetAttr(nodeItem, "name", strItemName);
                }

                OneItem info = (OneItem)table[path];

                DomUtil.SetAttr(nodeItem, "value", info.GetValueString(style));
            }

            dom.Save(strOutputFilename);
            return 0;
        }

        // 合并时间范围内的多个XML文件
        // parameters:
        //      strLibraryCodeList  馆代码列表。用来筛选 XML 文件中的分馆节点。如果想获得所有分馆的统计信息，那就需要用 "" 作为参数值
        public int MergeXmlFiles(
            string strLibraryCodeList,
            ref int nStop,
            string strDateRangeString,
            string strStyle,
            string strOutputFilename,
            out RangeStatisInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            File.Delete(strOutputFilename);

            ValueStyle valuestyle = ValueStyle.None;
            if (StringUtil.IsInList("list", strStyle) == true)
                valuestyle = ValueStyle.Total | ValueStyle.List;
            else
                valuestyle = ValueStyle.Total;

            info = new RangeStatisInfo();

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                ParseDateRange(strDateRangeString,
                    out strStartDate,
                    out strEndDate);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            info.StartDate = strStartDate;
            info.EndDate = strEndDate;

            DateTime start;
            DateTime end;

            try
            {
                start = DateTimeUtil.Long8ToDateTime(strStartDate);
            }
            catch (Exception ex)
            {
                strError = "日期字符串 '" + strDateRangeString + "' 格式错误: " + ex.Message;
                return -1;
            }

            if (strEndDate == "")
            {
                end = start;
                info.EndDate = strStartDate;
            }
            else
            {
                try
                {
                    end = DateTimeUtil.Long8ToDateTime(strEndDate);
                }
                catch (Exception ex)
                {
                    strError = "日期字符串 '" + strDateRangeString + "' 格式错误: " + ex.Message;
                    return -1;
                }

                TimeSpan delta = end - start;
                if (delta.Days < 0)
                {
                    // 交换两个时间
                    DateTime temp = end;
                    end = start;
                    start = temp;
                }
            }

            Hashtable valuetable = new Hashtable();

            int nCount = 0; // 实际存在多少天
            DateTime current = start;
            for (; ; )
            {
                if (nStop != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                string strDate = DateTimeUtil.DateTimeToString8(current);

                string strFilename = this.StatisDir + "\\" + strDate + ".xml";

                info.Days++;

                if (File.Exists(strFilename) == true)
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.Load(strFilename);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载首个文件 '" + strFilename + "' 到XMLDOM时出错: " + ex.Message;
                        return -1;
                    }

                    nRet = MergeValues(
                        strLibraryCodeList,
                        valuetable,
                        dom,
                        info.Days - 1,
                        valuestyle,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 首个文件
                    if (nCount == 0)
                    {
                        info.RealStartDate = DateTimeUtil.DateTimeToString8(current);
                        info.RealEndDate = DateTimeUtil.DateTimeToString8(current);
                    }
                    else
                    {
                        info.RealEndDate = DateTimeUtil.DateTimeToString8(current);
                    }

                    nCount++;

                    info.RealDays++;
                }
                else
                {
                    // 当天文件不存在的情况
                    if ((valuestyle & ValueStyle.List) != 0)
                    {
                        nRet = MergeValues(
        strLibraryCodeList,
        valuetable,
        null,
        info.Days - 1,
        valuestyle,
        out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                if (current >= end)
                    break;

                current = current.AddDays(1);
            }

            // 把Hashtable中的值写入XML文件中
            nRet = WriteToXmlFile(
                valuetable,
                strOutputFilename,
                valuestyle,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


#if NOOOOO

        // 合并时间范围内的多个XML文件
        public int MergeXmlFiles(
            string strLibraryCodeList,
            ref int nStop,
            string strDateRangeString,
            string strStyle,
            string strOutputFilename,
            out RangeStatisInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            File.Delete(strOutputFilename);

            info = new RangeStatisInfo();

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                ParseDateRange(strDateRangeString,
                    out strStartDate,
                    out strEndDate);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            info.StartDate = strStartDate;
            info.EndDate = strEndDate;

            DateTime start;
            DateTime end;

            start = DateTimeUtil.Long8ToDateTime(strStartDate);

            if (strEndDate == "")
            {
                end = start;
                info.EndDate = strStartDate;
            }
            else
            {
                end = DateTimeUtil.Long8ToDateTime(strEndDate);

                TimeSpan delta = end - start;
                if (delta.Days < 0)
                {
                    // 交换两个时间
                    DateTime temp = end;
                    end = start;
                    start = temp;
                }
            }

            int nCount = 0; // 实际存在多少天
            DateTime current = start;
            for (; ; )
            {
                if (nStop != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                string strDate = DateTimeUtil.DateTimeToString8(current);

                string strFilename = this.StatisDir + "\\" + strDate + ".xml";

                info.Days++;

                if (File.Exists(strFilename) == true)
                {
                    // 首个文件
                    if (nCount == 0)
                    {
                        if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                        {
                            // 如果是全局用户，直接复制第一个文件
                            try
                            {
                                File.Copy(strFilename, strOutputFilename, true);
                            }
                            catch (Exception ex)
                            {
                                // text-level: 内部错误
                                strError = "复制首个文件 " + strFilename + "到目标文件 " + strOutputFilename + "时发生错误: " + ex.Message;
                                return -1;
                            }
                        }
                        else
                        {
                            // 如果是分馆用户，则要对第一个文件进行过滤处理，去掉不相干的那些<library>元素
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.Load(strFilename);
                            }
                            catch (Exception ex)
                            {
                                strError = "装载首个文件 '" + strFilename + "' 到XMLDOM时出错: " + ex.Message;
                                return -1;
                            }

                            XmlNodeList nodes = dom.DocumentElement.SelectNodes("library");
                            foreach (XmlNode node in nodes)
                            {
                                string strCode = DomUtil.GetAttr(node, "code");
                                if (StringUtil.IsInList(strCode, strLibraryCodeList) == false)
                                    node.ParentNode.RemoveChild(node);
                            }

                            try
                            {
                                dom.Save(strOutputFilename);
                            }
                            catch (Exception ex)
                            {
                                strError = "保存XMLDOM到文件 '" + strOutputFilename + "' 时出错: " + ex.Message;
                                return -1;
                            }
                        }

                        info.RealStartDate = DateTimeUtil.DateTimeToString8(current);
                        info.RealEndDate = DateTimeUtil.DateTimeToString8(current);
                    }
                    else
                    {
                        // 合并两个XML文件到目标文件
                        nRet = MergeTwoXmlFiles(
                            strLibraryCodeList,
                            strFilename,
                            strOutputFilename,
                            strOutputFilename,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        info.RealEndDate = DateTimeUtil.DateTimeToString8(current);
                    }

                    nCount++;

                    info.RealDays++;
                }

                if (current >= end)
                    break;

                current = current.AddDays(1);
            }

            return 0;
        }


        // 合并两个统计数据XML文件到目标文件
        public static int MergeTwoXmlFiles(
            string strLibraryCodeList,
            string strSourceFilename1,
            string strSourceFilename2,
            string strTargetFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlDocument dom1 = new XmlDocument();
            try
            {
                // 2008/11/24
                FileInfo fi = new FileInfo(strSourceFilename1);
                if (fi.Exists == false || fi.Length == 0)
                    dom1.LoadXml("<root />");
                else
                    dom1.Load(strSourceFilename1);
            }
            catch (Exception ex)
            {
                strError = "装载XML文件 " + strSourceFilename1 + " 到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlDocument dom2 = new XmlDocument();
            try
            {
                // 2008/11/24
                FileInfo fi = new FileInfo(strSourceFilename2);
                if (fi.Exists == false || fi.Length == 0)
                    dom2.LoadXml("<root />");
                else
                    dom2.Load(strSourceFilename2);
            }
            catch (Exception ex)
            {
                strError = "装载XML文件 " + strSourceFilename2 + " 到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            // 确保dom2的根存在
            if (dom2.DocumentElement == null)
            {
                dom2.LoadXml("<root />");
            }

            // 合并根下的<catagory>元素
            // 无论是全局用户身份还是分馆用户身份，这一步都要做
            // 这就意味着分馆用户是可以获得根下的全部<category>元素和相关<library>元素下的<category>元素
            nRet = Merge(
                dom1.DocumentElement,
                dom2.DocumentElement,
                out strError);
            if (nRet == -1)
                return -1;

            // 列出DOM1中的全部<library>元素的code属性值
            List<string> exist_codes = new List<string>();
            XmlNodeList nodes = dom1.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                exist_codes.Add(strCode);
            }

            // 把这些节点下的<category>元素合并
            foreach (string code in exist_codes)
            {
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false
                    && StringUtil.IsInList(code, strLibraryCodeList) == false)
                    continue;

                XmlNode root1 = dom1.DocumentElement.SelectSingleNode("library[@code='"+code+"']");
                Debug.Assert(root1 != null, "");
                if (root1 == null)
                    continue;

                XmlNode root2 = dom2.DocumentElement.SelectSingleNode("library[@code='" + code + "']");
                if (root2 == null)
                {
                    root2 = dom2.CreateElement("library");
                    dom2.DocumentElement.AppendChild(root2);
                    DomUtil.SetAttr(root2, "code", code);
                }

                if (root1 == null || root2 == null)
                    continue;
                nRet = Merge(
    root1,
    root2,
    out strError);
                if (nRet == -1)
                    return -1;
            }

            dom2.Save(strTargetFilename);

            return 0;
        }

        // 将指定节点下的<catagory>元素合并。root2的内容被修改
        static int Merge(
            XmlNode root1,
            XmlNode root2,
            out string strError)
        {
            strError = "";

            XmlNodeList categorys1 = root1.SelectNodes("category");
            for (int i = 0; i < categorys1.Count; i++)
            {
                XmlNode category1 = categorys1[i];

                string strCategoryName1 = DomUtil.GetAttr(category1, "name");

                // 看看这个名字在DOM2中是否存在
                XmlNode category2 = root2.SelectSingleNode("category[@name='" + strCategoryName1 + "']");
                if (category2 == null)
                {
                    // 如果不存在，就创建一个
                    category2 = root2.OwnerDocument.CreateElement("category");
                    root2.AppendChild(category2);
                    DomUtil.SetAttr(category2, "name", strCategoryName1);
                }

                XmlNodeList items1 = category1.SelectNodes("item");
                for (int j = 0; j < items1.Count; j++)
                {
                    XmlNode item1 = items1[j];

                    string strItemName1 = DomUtil.GetAttr(item1, "name");
                    string strItemValue1 = DomUtil.GetAttr(item1, "value");

                    // 看看这个名字在DOM2中是否存在
                    XmlNode item2 = category2.SelectSingleNode("item[@name='" + strItemName1 + "']");
                    if (item2 == null)
                    {
                        // 如果不存在，就创建一个
                        item2 = root2.OwnerDocument.CreateElement("item");
                        category2.AppendChild(item2);
                        DomUtil.SetAttr(item2, "name", strItemName1);
                        DomUtil.SetAttr(item2, "value", strItemValue1);
                    }
                    else
                    {
                        string strItemValue2 = DomUtil.GetAttr(item2, "value");
                        // 两个value相加
                        try
                        {
                            double v1 = Convert.ToDouble(strItemValue1);
                            double v2 = Convert.ToDouble(strItemValue2);

                            DomUtil.SetAttr(item2, "value", (v1 + v2).ToString());
                        }
                        catch
                        {
                            strError = "值 " + strItemValue1 + " 或值 " + strItemValue2 + " 格式不正确，应当为纯数字";
                            return -1;
                        }
                    }
                }
            }

            return 0;
        }

#endif
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class DateExist
    {
        [DataMember]
        public string Date = "";
        [DataMember]
        public bool Exist = false;
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class RangeStatisInfo
    {
        // 统计起止日期
        [DataMember]
        public string StartDate = "";
        [DataMember]
        public string EndDate = "";

        // 实际存在统计信息的起止日期
        [DataMember]
        public string RealStartDate = "";
        [DataMember]
        public string RealEndDate = "";

        // 统计起止日期跨越的天数
        [DataMember]
        public long Days = 0;   // 天数

        // 实际存在统计信息的天数
        [DataMember]
        public long RealDays = 0;
    }
}
