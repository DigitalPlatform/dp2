using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DigitalPlatform.Xml;
using System.Web;
using System.Drawing;

using DigitalPlatform.Text;
using DigitalPlatform.Drawing;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 流通权限参数 
    /// 这一部分前端也需要用到
    /// </summary>
    public static class LoanParam
    {

        public static string[] reader_d_paramnames = new string[] { 
                    "可借总册数",
                    "可预约册数", 
                    "以停代金因子",
                    "工作日历名",
            };

        public static string[] two_d_paramnames = new string[] { 
                    // "可借总册数",
                    "可借册数",
                    "借期" ,
                    // "可预约册数",   // 2007/7/8
                    "超期违约金因子",
                    "丢失违约金因子",
                    // "工作日历名",
            };

        #region GetRightTableHtml()

        // 获得权限定义表HTML字符串
        // parameters:
        //      strSource   可能会包含<readerTypes>和<bookTypes>参数
        //      librarycodelist  当前用户管辖的分馆代码列表
        public static int GetRightTableHtml(
            // string strSource,
            XmlDocument cfg_dom_param,
            // string strLibraryCodeList,
            List<string> librarycodes,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

#if NO
            XmlDocument cfg_dom = null;
            if (String.IsNullOrEmpty(strSource) == true)
                cfg_dom = this.LibraryCfgDom;
            else
            {
                cfg_dom = new XmlDocument();
                try
                {
                    cfg_dom.LoadXml("<rightsTable>" + strSource + "</rightsTable>");
                }
                catch (Exception ex)
                {
                    strError = "strSource内容(外加根元素后)装入XMLDOM时出现错误: " + ex.Message;
                    return -1;
                }
            }

            List<string> librarycodes = new List<string>();
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
            {
                // XML代码中的全部馆代码
                librarycodes = GetAllLibraryCode(cfg_dom.DocumentElement);
                StringUtil.RemoveDupNoSort(ref librarycodes);   // 去重

                // 读者库中用过的全部馆代码
                List<string> temp = GetAllLibraryCode();
                if (temp.Count > 0 && temp[0] == "")
                    librarycodes.Insert(0, "");
            }
            else
            {
                librarycodes = StringUtil.FromListString(strLibraryCodeList);
            }

#endif
            XmlNode root = cfg_dom_param.DocumentElement.SelectSingleNode("//rightsTable");
            if (root == null)
            {
                strError = "所提供的读者权限定义XML字符串中不存在<rightsTable>元素";
                return -1;
            }

            foreach (string strLibraryCode in librarycodes)
            {
                strResult += "<p>" + HttpUtility.HtmlEncode("馆代码 '" + strLibraryCode + "' 的读者权限表") + "</p>";
                strResult += "<table style='width:100%;font-size:12pt;border-style:solid;border-width:1pt;border-color:#000000;border-collapse:collapse;border-left-width:1pt;border-top-width:1pt;border-right-width:1pt;border-bottom-width: 1pt;background-color:white;'>";

                List<String> readertypes = GetReaderTypes(// cfg_dom,
                    root, strLibraryCode);
                List<String> booktypes = GetBookTypes(// cfg_dom,
                    root, strLibraryCode);

                if (readertypes.Count == 0 && booktypes.Count == 0)
                {
                    // 从<rightsTable>的权限定义代码中(而不是从<readerTypes>和<bookTypes>元素下)获得读者和图书类型列表
                    GetReaderAndBookTypes(
                        root,   // cfg_dom,
                        strLibraryCode,
                        out readertypes,
                        out booktypes);
                }

                string strTdStyle = "padding: 4pt;border-left-width: 0pt;border-top-width: 0pt;border-right-width: 0.5pt;border-bottom-width: 0.5pt;border-style: solid;border-color: #aaaaaa;";

                booktypes.Insert(0, "");    // 空字符串代表只和读者有关的参数

                // 标题
                strResult += "<tr>";
                strResult += "<td>" + "" + "</td>";
                for (int j = 0; j < booktypes.Count; j++)
                {
                    strResult += "<td style='" + strTdStyle + "'>" + booktypes[j] + "</td>";
                }
                strResult += "</tr>";

                for (int i = 0; i < readertypes.Count; i++)
                {
                    strResult += "<tr>";

                    // 左边第一列
                    strResult += "<td style='" + strTdStyle + "'>" + readertypes[i] + "</td>";

                    // 左边第二列：只和读者类型相关的参数

                    for (int j = 0; j < booktypes.Count; j++)
                    {
                        string strContent = "";

                        if (j == 0)
                        {
                            for (int k = 0; k < reader_d_paramnames.Length; k++)
                            {
                                string strParamName = reader_d_paramnames[k];

                                string strParamValue = "";
                                string strStyle = "";
                                MatchResult matchresult;
                                int nRet = LoanParam.GetLoanParam(
                                    root,   // cfg_dom,
                                    strLibraryCode,
                                    readertypes[i],
                                    booktypes[j],   // 实际上为空
                                    strParamName,
                                    out strParamValue,
                                    out matchresult,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strStyle = "STYLE=\"background-color:blue;font-weight:bold\"";
                                    strContent += "<div " + strStyle + ">" + strParamName + ":" + strError + "</div>";
                                }
                                else
                                {
                                    int r = 200;
                                    int g = 200;
                                    int b = 200;


                                    if ((matchresult & MatchResult.BookType) == MatchResult.BookType)
                                    {
                                        g += 55;
                                    }
                                    if ((matchresult & MatchResult.ReaderType) == MatchResult.ReaderType)
                                    {
                                        r += 55;
                                    }
                                    Color color = Color.FromArgb(r, g, b);

                                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";

                                    strContent += "<div " + strStyle + ">" + strParamName + ": " + strParamValue + "</div>";
                                }
                            } // end of for
                        }
                        else
                        {
                            for (int k = 0; k < two_d_paramnames.Length; k++)
                            {
                                string strParamName = two_d_paramnames[k];

                                string strParamValue = "";
                                string strStyle = "";
                                MatchResult matchresult;
                                int nRet = LoanParam.GetLoanParam(
                                    root,   // cfg_dom,
                                    strLibraryCode,
                                    readertypes[i],
                                    booktypes[j],
                                    strParamName,
                                    out strParamValue,
                                    out matchresult,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strStyle = "STYLE=\"background-color:blue;font-weight:bold\"";
                                    strContent += "<div " + strStyle + ">" + strParamName + ":" + strError + "</div>";
                                }
                                else
                                {
                                    int r = 200;
                                    int g = 200;
                                    int b = 200;


                                    if ((matchresult & MatchResult.BookType) == MatchResult.BookType)
                                    {
                                        g += 55;
                                    }
                                    if ((matchresult & MatchResult.ReaderType) == MatchResult.ReaderType)
                                    {
                                        r += 55;
                                    }
                                    Color color = Color.FromArgb(r, g, b);

                                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";

                                    strContent += "<div " + strStyle + ">" + strParamName + ": " + strParamValue + "</div>";
                                }
                            } // end of for
                        }
                        strResult += "<td style='" + strTdStyle + "'>" + strContent + "</td>";
                    }

                    strResult += "<tr>";
                }

                strResult += "<tr>";

                strResult += "<td style='padding: 4pt;' colspan=" + (booktypes.Count + 1).ToString() + ">";

                {
                    string strStyle = "";
                    string strContent = "";
                    string strText = "";

                    strContent = "<div>" + "图例:" + "</div>";
                    strResult += strContent;

                    strText = "来自缺省值";
                    Color color = Color.FromArgb(200, 200, 200);
                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";
                    strContent = "<div " + strStyle + ">" + strText + "</div>";
                    strResult += strContent;

                    strText = "仅匹配了读者类型";
                    color = Color.FromArgb(255, 200, 200);
                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";
                    strContent = "<div " + strStyle + ">" + strText + "</div>";
                    strResult += strContent;

                    strText = "仅匹配了图书类型";
                    color = Color.FromArgb(200, 255, 200);
                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";
                    strContent = "<div " + strStyle + ">" + strText + "</div>";
                    strResult += strContent;

                    strText = "同时匹配了读者和图书类型";
                    color = Color.FromArgb(255, 255, 200);
                    strStyle = "STYLE=\"background-color:" + ColorUtil.Color2String(color) + "\"";
                    strContent = "<div " + strStyle + ">" + strText + "</div>";
                    strResult += strContent;
                }

                strResult += "</td>";
                strResult += "<tr>";

                strResult += "</table>";

                strResult += "<br/>";
            }

            return 0;
        }

        // 2009/3/10
        // 从<readerTypes>元素中获得读者类型
        public static List<string> GetReaderTypes( // XmlDocument cfg_dom,
            XmlNode root,
            string strLibraryCode)
        {
            List<string> result = new List<string>();

#if NO
            XmlNode root = cfg_dom.DocumentElement.SelectSingleNode("//rightsTable");
            if (root == null)
                return result;
#endif

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // descendant
                XmlNode temp = root.SelectSingleNode("//descendant-or-self::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return result;
                root = temp;
            }
            else
            {
                strFilter = "[count(ancestor::library) = 0]";
            }

            XmlNodeList nodes = root.SelectNodes("descendant::readerTypes/item" + strFilter);
            foreach (XmlNode node in nodes)
            {
                string strText = node.InnerText.Trim();

                // 2014/2/23
                // 空置和星号都是不合法的
                if (string.IsNullOrEmpty(strText) == true
                    || strText == "*")
                    continue;
                result.Add(strText);
            }

            return result;
        }

        // 从<bookTypes>元素中获得读者类型
        public static List<string> GetBookTypes( // XmlDocument cfg_dom,
            XmlNode root,
            string strLibraryCode)
        {
            List<string> result = new List<string>();

#if NO
            XmlNode root = cfg_dom.DocumentElement.SelectSingleNode("//rightsTable");
            if (root == null)
                return result;
#endif

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // descendant
                XmlNode temp = root.SelectSingleNode("//descendant-or-self::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return result;
                root = temp;
            }
            else
            {
                strFilter = "[count(ancestor::library) = 0]";
            }

            XmlNodeList nodes = root.SelectNodes("descendant::bookTypes/item" + strFilter);

            foreach (XmlNode node in nodes)
            {
                string strText = node.InnerText.Trim();

                // 2014/2/23
                // 空置和星号都是不合法的
                if (string.IsNullOrEmpty(strText) == true
                    || strText == "*")
                    continue;
                result.Add(strText);
            }

            return result;
        }

        // 从<rightsTable>的权限定义代码中(而不是从<readerTypes>和<bookTypes>元素下)获得读者和图书类型列表
        public static void GetReaderAndBookTypes(
            // XmlDocument dom,
            XmlNode root,
            string strLibraryCode,
            out List<string> readertypes,
            out List<string> booktypes)
        {
            booktypes = new List<string>();
            readertypes = new List<string>();

#if NO
            XmlNode root = dom.DocumentElement.SelectSingleNode("//rightsTable");
            if (root == null)
            {
                root = dom.DocumentElement;
            }
#endif

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // descendant
                XmlNode temp = root.SelectSingleNode("descendant-or-self::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return;
                root = temp;
            }
            else
            {
                strFilter = "[count(ancestor::library) = 0]";
            }

            // 选出所有<type>元素
            XmlNodeList nodes = root.SelectNodes("descendant::type" + strFilter);

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strReaderType = DomUtil.GetAttr(node, "reader");
                string strBookType = DomUtil.GetAttr(node, "book");

                if (String.IsNullOrEmpty(strReaderType) == false
                    && strReaderType != "*")
                {
                    readertypes.Add(strReaderType);
                    continue;
                }

                if (String.IsNullOrEmpty(strBookType) == false
                    && strBookType != "*")
                {
                    booktypes.Add(strBookType);
                    continue;
                }
            }

            StringUtil.RemoveDupNoSort(ref readertypes);

            StringUtil.RemoveDupNoSort(ref booktypes);
        }
        #endregion

        // 获得流通参数
        // parameters:
        //      strLibraryCode  图书馆代码, 如果为空,表示使用<library>元素以外的片段
        // return:
        //      reader和book类型均匹配 算4分
        //      只有reader类型匹配，算3分
        //      只有book类型匹配，算2分
        //      reader和book类型都不匹配，算1分
        public static int GetLoanParam(
            XmlNode root,   // XmlDocument cfg_dom_param,    // 2008/8/22
            string strLibraryCode,
            string strReaderType,
            string strBookType,
            string strParamName,
            out string strParamValue,
            out MatchResult matchresult,
#if DEBUG_LOAN_PARAM
            out string strDebug,
#endif
 out string strError)
        {
            strParamValue = "";
            strError = "";
#if DEBUG_LOAN_PARAM
            strDebug = "";
#endif
            matchresult = MatchResult.None;

#if NO
            string[] reader_d_paramnames = new string[] { 
                    "可借总册数",
                    "可预约册数", 
                    "以停代金因子",
                    "工作日历名",
            };
#endif

            // XmlDocument cfg_dom = null;

#if NO
            // XmlDocument dup_cfg_dom = null;
            // 如果cfg_dom参数值为空，则表示使用当前系统的配置DOM
            if (cfg_dom_param == null)
                cfg_dom = this.LibraryCfgDom;
            else
                cfg_dom = cfg_dom_param;
#endif

#if NO
            if (cfg_dom_param == null)
            {
                strError = "cfg_dom_param == null";
                return -1;
            }
            cfg_dom = cfg_dom_param;
#endif

            // 2014/2/23
            if (strReaderType == "*")
            {
                strError = "读者类型不允许使用 *";
                return -1;
            }
            if (strBookType == "*")
            {
                strError = "图书类型不允许使用 *";
                return -1;
            }

            bool bReaderOnly = false;   // 是否为只和读者类型有关的参数
            for (int i = 0; i < reader_d_paramnames.Length; i++)
            {
                if (strParamName == reader_d_paramnames[i])
                {
                    bReaderOnly = true;
                    break;
                }
            }

            if (bReaderOnly == true)
            {
                // 清除读者类型
                strBookType = "";
            }
            else
            {
                if (string.IsNullOrEmpty(strBookType) == true)
                    strBookType = "[空]";
            }

            if (string.IsNullOrEmpty(strReaderType) == true)
                strReaderType = "[空]";
#if NO
            XmlNode root = cfg_dom.DocumentElement.SelectSingleNode(cfg_dom_param == null ? "rightsTable" : "//rightsTable");   // 2008/8/22 add '//'
            if (root == null)
            {
                if (cfg_dom_param == null)
                    strError = "所提供的读者权限定义XML字符串中不存在<rightsTable>元素";
                else
                    strError = "library.xml配置文件中尚未配置<rightsTable>元素";
                return -1;
            }
#endif

            string strFilter = "";

            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // descendant
                XmlNode temp = root.SelectSingleNode("descendant-or-self::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                {
                    strError = "<rightsTable> 中没有配置 code 属性为 '" + strLibraryCode + "' 的 <library> 元素";
                    return -1;
                }
                root = temp;
            }
            else
            {
                // TODO: 如果有一个以上的<library>元素，则需要复制出一个新的DOM，然后把<library>元素全部删除干净
                strFilter = "[count(ancestor::library) = 0]";
            }

            // 选出所有同名参数
            XmlNodeList nodeParams = root.SelectNodes("descendant::param[@name='" + strParamName + "']" + strFilter);

            List<WeightNode> weightnodes = new List<WeightNode>();
            // 筛选出符合条件的

            // 如果有readertype和booktype都符合的，就优先用(退出循环)，如果没有，则加权后进入一个数组，排序后采用加权值大的。
            for (int i = 0; i < nodeParams.Count; i++)
            {
                XmlNode node = nodeParams[i];

                double nLevel = GetLevel(node);

                string strThisReaderType;
                string strThisBookType;

                int nRet = GetSurroundType(node,
                    out strThisReaderType,
                    out strThisBookType,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 如果仅仅需要匹配读者因素，那么即便两种因素命中，都只算一个
                if (bReaderOnly == true && nRet > 1)
                    nRet = 1;


                if (nRet == 2)
                {
                    // 找到2个外围气氛因素
                    if (
                        (strThisReaderType == "*" || strReaderType == strThisReaderType)
                        && (strThisBookType == "*" || strBookType == strThisBookType)
                        )
                    {
                        /*
                        // 2类型均匹配
                        strParamValue = DomUtil.GetAttr(node, "value");
                        matchresult = MatchResult.BookType | MatchResult.ReaderType;

                        strDebug += "发现了2因素都匹配的节点，不用排序，直接返回了" + "\r\n";

                        return 4;   // 表示精确匹配2因素
                         * */
                        // 2类型均匹配 算4分
                        WeightNode weightnode = new WeightNode();
                        weightnode.Index = i;
                        weightnode.Weight = 4;  // 2 3
                        weightnode.Level = nLevel;
                        weightnode.Node = node;
                        weightnode.MatchResult = MatchResult.BookType | MatchResult.ReaderType;
                        weightnodes.Add(weightnode);

                        // 2008/8/20
                        if (strThisReaderType == "*")
                            weightnode.Wild++;
                        if (strThisBookType == "*")
                            weightnode.Wild++;

#if DEBUG_LOAN_PARAM
                        strDebug += weightnode.GetDebugInfo() + "\r\n";
#endif
                        continue;

                    }

                    nRet = 1;   // 继续向后判断
                }

                if (nRet == 1)
                {
                    // 只找到一个外围气氛因素
                    // strThisReaderType和strThisBookType中只有一个是有意义的，那就是非空的那个
                    if (String.IsNullOrEmpty(strThisReaderType) == false
                        && (strThisReaderType == "*" || strReaderType == strThisReaderType)
                        )
                    {
                        // 只有reader类型匹配，算3分
                        WeightNode weightnode = new WeightNode();
                        weightnode.Index = i;
                        weightnode.Weight = 3;  // 1
                        weightnode.Level = nLevel;
                        weightnode.Node = node;
                        weightnode.MatchResult = MatchResult.ReaderType;
                        weightnodes.Add(weightnode);

                        // 2008/8/20
                        if (strThisReaderType == "*")
                            weightnode.Wild++;


#if DEBUG_LOAN_PARAM
                        strDebug += weightnode.GetDebugInfo() + "\r\n";
#endif
                        continue;
                    }
                    else if (String.IsNullOrEmpty(strThisBookType) == false
                        && (strThisBookType == "*" || strBookType == strThisBookType)
                        )
                    {
                        // 只有book类型匹配，算2分
                        WeightNode weightnode = new WeightNode();
                        weightnode.Index = i;
                        weightnode.Weight = 2; // 1
                        weightnode.Level = nLevel;
                        weightnode.Node = node;
                        weightnode.MatchResult = MatchResult.BookType;
                        weightnodes.Add(weightnode);

                        // 2008/8/20
                        if (strThisBookType == "*")
                            weightnode.Wild++;

#if DEBUG_LOAN_PARAM
                        strDebug += weightnode.GetDebugInfo() + "\r\n";
#endif
                        continue;
                    }

                    nRet = 0;   // 继续向后判断
                }

                if (nRet == 0)
                {
                    // reader和book类型都不匹配，算1分
                    WeightNode weightnode = new WeightNode();
                    weightnode.Index = i;
                    weightnode.Weight = 1;
                    weightnode.Level = nLevel;
                    weightnode.Node = node;
                    weightnode.MatchResult = MatchResult.None;
                    weightnodes.Add(weightnode);
#if DEBUG_LOAN_PARAM
                    strDebug += weightnode.GetDebugInfo() + "\r\n";
#endif
                }

            } // end of for

            if (weightnodes.Count == 0)
            {
                strError = "没有找到配置参数";
                return 0;   // 没有找到所指定名字的配置参数
            }

            // 排序
            WeightNodeComparer comp = new WeightNodeComparer();

            weightnodes.Sort(comp);

            strParamValue = DomUtil.GetAttr(weightnodes[0].Node, "value");
            matchresult = weightnodes[0].MatchResult;
            return weightnodes[0].Weight;   // 表示勉强匹配
        }



        static double GetLevel(XmlNode node)
        {
            double nLevel = 0.0;


            while (node != null
                && node.ParentNode != node.OwnerDocument)
            {
                if (node.Name == "rightsTable")
                    break;

                node = node.ParentNode;
                nLevel += 0.1;
            }

            return nLevel;
        }

        // 获得元素外围所包围的读者和图书类型值
        // 最接近的，最优先
        static int GetSurroundType(XmlNode node,
            out string strReaderType,
            out string strBookType,
            out string strError)
        {
            strError = "";
            strReaderType = "";
            strBookType = "";

            while (node != null
                && node.ParentNode != node.OwnerDocument)
            {
                // 2012/9/11
                if (node.Name == "library"
                    || node.Name == "rightsTable")
                    break;

                if (node.Name != "type")
                    goto DOCONTINUE;

                if (String.IsNullOrEmpty(strReaderType) == true)
                {
                    strReaderType = DomUtil.GetAttr(node, "reader");
                    /*
                    if (strReaderType == "*")
                        strReaderType = "";
                     * */
                }

                if (String.IsNullOrEmpty(strBookType) == true)
                {
                    strBookType = DomUtil.GetAttr(node, "book");
                    /*
                    if (strBookType == "*")
                        strBookType = "";
                     * */
                }

                if (String.IsNullOrEmpty(strReaderType) == false
                    && String.IsNullOrEmpty(strBookType) == false)
                    return 2;   // both found

            DOCONTINUE:
                node = node.ParentNode;
            }

            if (String.IsNullOrEmpty(strReaderType) == false
                && String.IsNullOrEmpty(strBookType) == false)
                return 2;   // both found

            if (String.IsNullOrEmpty(strReaderType) == true
                && String.IsNullOrEmpty(strBookType) == true)
                return 0;   // not found

            return 1;   // found one type 
        }
    }


    // 权值和节点的组合
    public class WeightNode
    {
        public double Level = 0;
        public int Index = -1;  // 遍历序号
        public int Weight = 0;  // 权值
        public int Wild = 0;    // 是否模糊匹配 0 不是 1 有一个因素模糊匹配 2 有2个因素模糊匹配
        public XmlNode Node = null; // XmlNode节点
        public MatchResult MatchResult = MatchResult.None;

        public string GetDebugInfo()
        {
            return "XML=" + HttpUtility.HtmlEncode(Node.OuterXml)
                + "\r\nLevel="
                + Level.ToString() + ", Index=" + Index.ToString()
                + "Weight=" + Weight.ToString() + ", Wild=" + Wild.ToString();
        }
    }

    public class WeightNodeComparer : IComparer<WeightNode>
    {
        // 越靠前的就是越适合的
        int IComparer<WeightNode>.Compare(WeightNode x, WeightNode y)
        {
            // 如果权值不同，则依据权值比较。权值大的靠前
            double nDelta = ((double)x.Weight + x.Level) - ((double)y.Weight + y.Level);
            if (nDelta != 0)
                return (int)(-1 * nDelta * 10);

            // 2008/8/20
            // wild值大的靠后
            int nWild = x.Wild - y.Wild;
            if (nWild != 0)
                return nWild;

            // 如果权值相同，则依据序号。序号小的更靠前
            return (x.Index - y.Index);
        }
    }

    // 参数匹配的细节情况
    public enum MatchResult
    {
        None = 0,
        ReaderType = 0x01,  // 读者类型匹配上了
        BookType = 0x02,    // 图书类型匹配上了
    }
}
