using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// 卡特表基础类
    /// </summary>
    public class QuickCutter
    {
        XmlDocument dom = null;
        string m_strFilename = "";

        public QuickCutter(string strFileName)
        {
            dom = new XmlDocument();
            dom.Load(strFileName);

            this.m_strFilename = strFileName;
        }

#if NO
        // 比较两个字符串，从左到右连续相同部分的字符数
        static int CompHead(string s1, string s2)
        {
            s1 = s1.ToUpper();
            s2 = s2.ToUpper();
            for (int i = 0; ; i++)
            {
                if (i >= s1.Length)
                    return i;
                if (i >= s2.Length)
                    return i;
                if (s1[i] != s2[i])
                    return i;
            }
        }
#endif

        // 规整表中的著者字符串
        // 1) 去掉末尾的'.'
        // 2) 去掉空格
        static string CanonicalizeEntryText(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            strText = strText.Replace(" ", "");
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            if (strText[strText.Length - 1] == '.')
                return strText.Substring(0, strText.Length - 1);
            return strText;
        }

        // 规整入口著者字符串
        // 1) 去掉末尾的'.' ','等符号
        // 2) 去掉空格
        static string CanonicalizeAuthor(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            strText = strText.Replace(" ", "");
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            strText = strText.Replace("'", "");
            strText = strText.Replace("’", "");
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            strText = strText.Replace("，", ",");

            if (".,".IndexOf(strText[strText.Length - 1]) != -1)
                strText = strText.Substring(0, strText.Length - 1);

            // Boëly --> Boely 或者 Boeely 
            /*
             * Cutter a vowel with an umlaut as that vowel followed by an "e":
° Because the name Boëly has an umlaut, it should be cuttered as "boeely," making it fall between Boe B669 and Boeh B671. Because "e" comes before "h," the lower number, B669 is used.
° If the umlaut had not been in the name, it would have been cuttered as "boely," falling between Boeh B671 and Boer B672.
             * 
             * */
            if (strText.IndexOf("ë") != -1)
            {
                /*
                string strLeft = "";
                string strRight = "";
                int nRet = strText.IndexOf(",");
                if (nRet == -1)
                    strLeft = strText;
                else
                {
                    strLeft = strText.Substring(0, nRet).Replace("ë", "e"); // surname
                    strRight = strText.Substring(nRet + 1).Replace("ë", "ee"); // name
                    strText = strLeft + strRight;
                }
                 * */

                strText = strText.Replace("ë", "ee");
            }

            // McCabe --> MacCabe
            if (StringUtil.HasHead(strText, "Mc") == true && strText.Length >= 3
                && char.IsUpper(strText[2]) == true)
            {
                strText = strText.Insert(1, "a");
            }

            strText = strText.Replace("-", ",");    // 复姓处理
            return strText;
        }

#if NO
        /*
《卡特著者号码表》取号规则
《卡特著者号码表》有两种：一种是两位数的，另一种是三位数的。我馆采用三位数的。
其使用方法和取号规则由简到繁分条说明如下：
1．著者号码是由著者姓氏的第一个字母的连音组成的。例如：著者姓Bellow，表上的连音也是Bellow 448，因此，著者号码便取B448。又如：著者姓Falk，表上的连音也是Falk 191，著者号码便取F191。
2．如著者姓氏的连音在表中未完全列出，则按照‘取上不取下’的原则，应取与其邻近的前一个连音为著者号码。例如：著者姓Rudman，而表中的连音只有Rudi 916和Rudo 917，因此Rudman的著者号码应取Rudi的号码R916。又如：著者姓Grate，而表中的连音只有Grat 771和Grati 772，因此Grate的著者号码应取Grat的号码G771。
3．如果著者的姓氏比较普遍，同姓的著者很多，在同姓连音的情况下，应加著者名字的第一个字母的连音。 例如：著者姓名为Franklin, M.，但与Franklin同连音的在表上共有Franklin, H. 832 / Franklin, M. 833 / Franklin, S. 834三个，因此，Franklin, M. 的著者号码应取F833。 又如：著者姓名为Fields, N., 但与Fields 的连音接近的在表上只有Fields 461 / Fields, J. 462 / Fields, S. 463共三个，同样按照第2条‘取上不取下’的原则，Fields, N. 的著者号码应取F462。
4．单姓双名的著者，当著者姓的连音相同时，再按著者第一名字（即本名）的连音决定著者号码，第二名字（即父名）不计, 取号方法参见第3条。例如：著者姓名为Foster, N. S.，在表上同Foster的连音接近的有：Foster 754 / Foster, H. 755 / Foster, M. 756 / Foster, S. 757 /Foster, W. 758 共五个，因此，Foster, N. S. 的著者号码应取F756。
5．复姓著者按第一姓的连音取著者号码，如果第一姓与单姓著者的连音相同，而且不止一个时，再按第二姓的连音查表取号。例如：Curtis-Prior, P. B. 的著者号码为C981； Lottem-Loyd，A. C. 的著者号码为L884。
6．著者姓前冠有接头词或冠词，如：McHale，DelaRoche，DuMaurier，VanDyke，O’Casey等，要视为一词查表取号。举例：Mchale的著者号码为M478，Delaroche的著者号码为D339，Dumaurier的著者号码为D886，Vandyke的著者号码为V248，O’casey的著者号码为O15。
7．两人或两人以上的合著者，按第一著者姓氏的连音查表取号，查表方法与单著者相同。
8. 著者无从查考或以书名当著者的图书，均按正书名第一词（冠词除外）的连音查表取著作号码。例如：《Mother goose》一书的著者无从查考，故要按Mother一词查表取号，其著作号码为M918。如果正书名的第一词为一数字，则需要译成与之相对应的正式文种后，再按该词的连音查表。例如：《3 Years In Tokyo》一书，正书名的第一词为数字3，则需译成英文Three以后再查表取号，该书的著作号码为T531。
9. 个人传记图书按被传者姓氏的连音查表取号，并在著者号码后加上写作人姓氏的第一字母。例如：G. Williams的《Life and Work of Lu Hsun》，该书的著者号码为L926W。
10．机关团体著者及其他形式的集体责任者按机构名称第一词（冠词除外）的连音查表取号。例如：The National Science Foundation为一机构名称，著者号码应为N277。
         * 
         * 测试用例
         * Bellow   = Bellow 448
         * Falk     = Falk 191
         * Rudman   = Rudi 916
         * Grate    = Grat 771
         * Franklin, M. = Franklin,M 833
         * Fields, N.   = Fields,J 462
         * Foster, N. S.    = Foster,M 756
         * Curtis-Prior, P. B.  = Curtis,P 981
         * Lottem-Loyd, A. C.   = Lott 884
         * Mchale   = Mazzon 478
         * DelaRoche    = Delar 339
         * Dumaurier    = Dumas 886
         * Vandyke  = Vandyk 248
         * O’casey  = Oc 15
         * */
        // 获得一个事项
        // parameters:
        //      strAuthor   著者字符串。需要规整为 Abel, L 这样的形态
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetEntry(string strAuthor,
            out string strText,
            out string strNumber,
            out string strError)
        {
            strText = "";
            strNumber = "";
            strError = "";

            if (dom == null)
            {
                strError = "尚未装载卡特表文件内容";
                return -1;
            }

            if (string.IsNullOrEmpty(strAuthor) == true)
            {
                strError = "strAuthor参数值不能为空";
                return -1;
            }

            strAuthor = CanonicalizeAuthor(strAuthor);
            if (string.IsNullOrEmpty(strAuthor) == true)
            {
                strError = "规整后的strAuthor参数值不能为空";
                return -1;
            }

            string strFirstChar = strAuthor.Substring(0, 1).ToUpper();

            XmlNode group = dom.DocumentElement.SelectSingleNode("group[@t='" + strFirstChar + "']");
            if (group == null)
            {
                strError = "著者字符串的第一字符 '"+strFirstChar+"' 在表中没有找到对应的Group";
                return -1;
            }

            int nCommentLength = 0;
            List<XmlNode> longest_nodes = new List<XmlNode>();  // 公共部分长度最大的一组匹配上的节点
            XmlNode first_node = null;
            for (int i = 0; i < group.ChildNodes.Count; i++ )
            {
                XmlNode node = group.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (first_node == null)
                    first_node = node;

                // 去掉末尾的'.'
                string strCurrentText = CanonicalizeEntryText(DomUtil.GetAttr(node, "t")); 

                int nCurrentCommentLength = CompHead(strAuthor, strCurrentText);

                /*
                // 校正公共部分末尾为 ','的情形
                if (nCurrentCommentLength >= 1)
                {
                    if (strCurrentText[nCurrentCommentLength-1] == ',')
                        nCurrentCommentLength --;
                }
                 * */

                /*
                // 校正公共部分末尾为 ', '的情形
                if (nCurrentCommentLength >= 2)
                {
                    if (strCurrentText[nCurrentCommentLength - 2] == ','
                        && strCurrentText[nCurrentCommentLength - 1] == ' ')
                        nCurrentCommentLength -= 2;
                }
                 * */

                if (nCurrentCommentLength > nCommentLength)
                {
                    nCommentLength = nCurrentCommentLength;
                    longest_nodes.Clear();
                    longest_nodes.Add(node);
                }
                else if (nCurrentCommentLength == nCommentLength)
                {
                    longest_nodes.Add(node);
                }
                else if (nCurrentCommentLength < nCommentLength)
                { 
                    // 出现公共部分长度下降的趋势，就不要比较了
                    break; 
                }
            }

            if (longest_nodes.Count == 0)
                longest_nodes.Add(first_node);

            Debug.Assert(longest_nodes.Count > 0, "");

            XmlNode result_node = null;
            // 从多个节点中筛选出最匹配的一个
            if (longest_nodes.Count > 1)
            {
                for (int i = 0; i < longest_nodes.Count; i++)
                {
                    XmlNode node = longest_nodes[i];
                    string strCurrentText = CanonicalizeEntryText(DomUtil.GetAttr(node, "t"));
                    if (nCommentLength >= strAuthor.Length)
                    {
                        result_node = node;
                        break;
                    }
                    // 比较公共部分右边的第一个字符位
                    char chAuthor = strAuthor.ToUpper()[nCommentLength];
                    char chText = (char)0;
                    if (nCommentLength < strCurrentText.Length)
                        chText = strCurrentText.ToUpper()[nCommentLength];

                    if (chAuthor < chText)
                    {
                        if (i > 0)
                            result_node = longest_nodes[i - 1];
                        else
                            result_node = longest_nodes[i];
                        break;
                    }
                }

                if (result_node == null)
                {
                    strError = "不可能到达这里";
                    return -1;
                }
            }
            else
                result_node = longest_nodes[0];

            strText = CanonicalizeEntryText(DomUtil.GetAttr(result_node, "t"));

            strNumber = DomUtil.GetAttr(result_node, "n");
            return 1;
        }
#endif

        /*
《卡特著者号码表》取号规则
《卡特著者号码表》有两种：一种是两位数的，另一种是三位数的。我馆采用三位数的。
其使用方法和取号规则由简到繁分条说明如下：
1．著者号码是由著者姓氏的第一个字母的连音组成的。例如：著者姓Bellow，表上的连音也是Bellow 448，因此，著者号码便取B448。又如：著者姓Falk，表上的连音也是Falk 191，著者号码便取F191。
2．如著者姓氏的连音在表中未完全列出，则按照‘取上不取下’的原则，应取与其邻近的前一个连音为著者号码。例如：著者姓Rudman，而表中的连音只有Rudi 916和Rudo 917，因此Rudman的著者号码应取Rudi的号码R916。又如：著者姓Grate，而表中的连音只有Grat 771和Grati 772，因此Grate的著者号码应取Grat的号码G771。
3．如果著者的姓氏比较普遍，同姓的著者很多，在同姓连音的情况下，应加著者名字的第一个字母的连音。 例如：著者姓名为Franklin, M.，但与Franklin同连音的在表上共有Franklin, H. 832 / Franklin, M. 833 / Franklin, S. 834三个，因此，Franklin, M. 的著者号码应取F833。 又如：著者姓名为Fields, N., 但与Fields 的连音接近的在表上只有Fields 461 / Fields, J. 462 / Fields, S. 463共三个，同样按照第2条‘取上不取下’的原则，Fields, N. 的著者号码应取F462。
4．单姓双名的著者，当著者姓的连音相同时，再按著者第一名字（即本名）的连音决定著者号码，第二名字（即父名）不计, 取号方法参见第3条。例如：著者姓名为Foster, N. S.，在表上同Foster的连音接近的有：Foster 754 / Foster, H. 755 / Foster, M. 756 / Foster, S. 757 /Foster, W. 758 共五个，因此，Foster, N. S. 的著者号码应取F756。
5．复姓著者按第一姓的连音取著者号码，如果第一姓与单姓著者的连音相同，而且不止一个时，再按第二姓的连音查表取号。例如：Curtis-Prior, P. B. 的著者号码为C981； Lottem-Loyd，A. C. 的著者号码为L884。
6．著者姓前冠有接头词或冠词，如：McHale，DelaRoche，DuMaurier，VanDyke，O’Casey等，要视为一词查表取号。举例：Mchale的著者号码为M478，Delaroche的著者号码为D339，Dumaurier的著者号码为D886，Vandyke的著者号码为V248，O’casey的著者号码为O15。
7．两人或两人以上的合著者，按第一著者姓氏的连音查表取号，查表方法与单著者相同。
8. 著者无从查考或以书名当著者的图书，均按正书名第一词（冠词除外）的连音查表取著作号码。例如：《Mother goose》一书的著者无从查考，故要按Mother一词查表取号，其著作号码为M918。如果正书名的第一词为一数字，则需要译成与之相对应的正式文种后，再按该词的连音查表。例如：《3 Years In Tokyo》一书，正书名的第一词为数字3，则需译成英文Three以后再查表取号，该书的著作号码为T531。
9. 个人传记图书按被传者姓氏的连音查表取号，并在著者号码后加上写作人姓氏的第一字母。例如：G. Williams的《Life and Work of Lu Hsun》，该书的著者号码为L926W。
10．机关团体著者及其他形式的集体责任者按机构名称第一词（冠词除外）的连音查表取号。例如：The National Science Foundation为一机构名称，著者号码应为N277。
 * 
 * 测试用例
 * Bellow   = Bellow 448
 * Falk     = Falk 191
 * Rudman   = Rudi 916
 * Grate    = Grat 771
 * Franklin, M. = Franklin,M 833
 * Fields, N.   = Fields,J 462
 * Foster, N. S.    = Foster,M 756
 * Curtis-Prior, P. B.  = Curtis,P 981
 * Lottem-Loyd, A. C.   = Lott 884
 * Mchale   = Mazzon 478
 * DelaRoche    = Delar 339
 * Dumaurier    = Dumas 886
 * Vandyke  = Vandyk 248
 * O’casey  = Oc 15
         * 
         * http://www.library.yale.edu/cataloging/music/callocal.htm#cuttertable
         * Stankovíc    = Stanh 786
         * Boëly
         * 
         * 
         * 
 * */
        // 获得一个事项
        // parameters:
        //      strAuthor   著者字符串。需要规整为 Abel, L 这样的形态
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetEntry(string strAuthor,
            out string strText,
            out string strNumber,
            out string strError)
        {
            strText = "";
            strNumber = "";
            strError = "";

            if (dom == null)
            {
                strError = "尚未装载卡特表文件内容";
                return -1;
            }

            if (string.IsNullOrEmpty(strAuthor) == true)
            {
                strError = "strAuthor参数值不能为空";
                return -1;
            }

            strAuthor = CanonicalizeAuthor(strAuthor);
            if (string.IsNullOrEmpty(strAuthor) == true)
            {
                strError = "规整后的strAuthor参数值不能为空";
                return -1;
            }

            string strFirstChar = strAuthor.Substring(0, 1).ToUpper();

            XmlNode group = dom.DocumentElement.SelectSingleNode("group[@t='" + strFirstChar + "']");
            if (group == null)
            {
                strError = "著者字符串的第一字符 '" + strFirstChar + "' 在表中没有找到对应的Group";
                return -1;
            }

            string strPrevText = "";
            XmlNode result_node = null;
            for (int i = 0; i < group.ChildNodes.Count; i++)
            {
                XmlNode node = group.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                string strCurrentText = CanonicalizeEntryText(DomUtil.GetAttr(node, "t"));

                if (string.Compare(strAuthor, strCurrentText, true) < 0
                    && string.Compare(strAuthor, strPrevText, true) >= 0)
                {
                    result_node = PrevNode(node);

                    // 2012/12/8
                    if (result_node == null)
                    {
                        strError = "著者字符串 '" + strAuthor + "' 比 '" + strCurrentText + "' 要小，缺乏号码定义";
                        return 0;
                        // result_node = FirstChild(group);
                    }
                    break;
                }

                strPrevText = strCurrentText;
            }

            if (result_node == null)
                result_node = LastChild(group);

            strText = DomUtil.GetAttr(result_node, "t");

            strNumber = DomUtil.GetAttr(result_node, "n");
            return 1;
        }

        static XmlNode FirstChild(XmlNode group)
        {
            foreach (XmlNode node in group.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                return node;
            }

            return null;
        }

        static XmlNode LastChild(XmlNode group)
        {
            for (int i = group.ChildNodes.Count - 1; i >= 0; i--)
            {
                XmlNode node = group.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                return node;
            }

            return null;
        }

        static XmlNode PrevNode(XmlNode node)
        {
            node = node.PreviousSibling;
            while (node != null && node.NodeType != XmlNodeType.Element)    // 2012/12/8 把 node != null 调整到前面了
            {
                node = node.PreviousSibling;
            }

            return node;
        }

        // 把<item>元素的n属性提到最前面
        // return:
        //      -1  出错
        //      0   正确
        public int Exchange(out string strError)
        {
            strError = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlNode node in nodes)
            {
                XmlAttribute attr = node.Attributes["n"];
                if (attr != null)
                {
                    node.Attributes.Remove(attr);
                    node.Attributes.InsertBefore(attr, node.Attributes["t"]);
                }
            }

            this.dom.Save(this.m_strFilename);
            return 0;
        }

        // 校验XML文件的正确性
        // return:
        //      -1  出错
        //      0   正确
        public int Verify(out string strError)
        {
            strError = "";

            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                XmlNode group = dom.DocumentElement.SelectSingleNode("group[@t='" + ch.ToString() + "']");
                if (group == null)
                {
                    strError += "著者字符串的第一字符 '" + ch.ToString() + "' 在表中没有找到对应的Group\r\n";
                    continue;
                }

                string strPrevText = "";
                string strPrevNumber = "";
                for (int i = 0; i < group.ChildNodes.Count; i++)
                {
                    XmlNode node = group.ChildNodes[i];
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    string strCurrentText = DomUtil.GetAttr(node, "t");
                    int nRet = strCurrentText.IndexOf(" ");
                    if (nRet != -1)
                    {
                        if (nRet < 1)
                        {
                            strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中t属性值不正确，空格字符位置不正确\r\n";
                        }
                        else
                        {
                            if (strCurrentText[nRet - 1] != ',')
                            {
                                strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中t属性值不正确，空格字符前面应当是逗号\r\n";
                            }
                        }
                    }
                    nRet = strCurrentText.IndexOf(",");
                    if (nRet != -1)
                    {
                        if (nRet >= strCurrentText.Length - 1)
                        {
                            strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中t属性值不正确，逗号字符位置不正确\r\n";
                        }
                        else
                        {
                            if (strCurrentText[nRet + 1] != ' ')
                            {
                                strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中t属性值不正确，逗号字符后面应当是空格 (但现在是 '" + strCurrentText[nRet + 1].ToString() + "')\r\n";
                            }
                        }
                    }

                    // TODO: 还要检查', '后面第一个字符是大写的

                    strCurrentText = CanonicalizeEntryText(strCurrentText);


                    string strCurrentNumber = DomUtil.GetAttr(node, "n");

                    if (string.Compare(strPrevText, strCurrentText, true) >= 0)
                    {
                        strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中t属性值不正确，应当大于前一个元素的t属性值 " + strPrevText + "\r\n";
                    }

                    if (string.Compare(strPrevNumber, strCurrentNumber, true) >= 0)
                    {
                        strError += "group " + ch.ToString() + " 下元素 " + node.OuterXml + " 中n属性值不正确，应当大于前一个元素的n属性值 " + strPrevNumber + "\r\n";
                    }

                    strPrevText = strCurrentText;
                    strPrevNumber = strCurrentNumber;
                }
            }

            if (string.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }
    }
}
