using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms
{
    // 浏览格式
    public class BrowseCfg : KeysBrowseBase
    {
        Hashtable m_exprCache = new Hashtable();
        Hashtable m_methodsCache = new Hashtable();

        // XmlDocument m_domTransform = null;  // 用于转换的专用DOM，由 this.dom 将<col>元素下的<title>元素去掉后产生
        XslCompiledTransform m_xt = null;

        public override void Clear()
        {
            base.Clear();
            m_exprCache.Clear();
            m_methodsCache.Clear();
            this.m_xt = null;
        }

        static List<string> GetMethods(string strConvert)
        {
            // List<string> convert_methods = StringUtil.SplitList(strConvert);
            List<string> convert_methods = StringUtil.SplitString(strConvert,
    ",",
    new string[] { "()" },
    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < convert_methods.Count; i++)
            {
                string strMethod = convert_methods[i].Trim().ToLower();
                if (string.IsNullOrEmpty(strMethod) == true)
                {
                    convert_methods.RemoveAt(i);
                    i--;
                    continue;
                }
                convert_methods[i] = strMethod;
            }

            return convert_methods;
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
        public int BuildCols(XmlDocument domData,
            int nStartCol,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = new string[0];

            Debug.Assert(domData != null, "BuildCols()调用错误，domData参数不能为null。");

            // 没有浏览格式定义时，就没有信息
            if (this.dom == null)
                return 0;

            int nResultLength = 0;

            XPathNavigator nav = domData.CreateNavigator();

            List<string> col_array = new List<string>();

            if (this.dom.DocumentElement.Prefix == "")
            {
                // 得到xpath的值
                XmlNodeList nodeListXpath = this.dom.SelectNodes(@"//xpath");

            CREATE_CACHE:
                // 创建Cache
                if (m_exprCache.Count == 0 && nodeListXpath.Count > 0)
                {
                    for (int i = 0; i < nodeListXpath.Count; i++)
                    {
                        XmlNode nodeXpath = nodeListXpath[i];
                        string strXpath = nodeXpath.InnerText.Trim(); // 2012/2/16
                        if (string.IsNullOrEmpty(strXpath) == true)
                            continue;

                        string strNstableName = DomUtil.GetAttrDiff(nodeXpath, "nstable");
                        XmlNamespaceManager nsmgr = (XmlNamespaceManager)this.tableNsClient[nodeXpath];
#if DEBUG
                        if (nsmgr != null)
                        {
                            Debug.Assert(strNstableName != null, "此时应该没有定义'nstable'属性。");
                        }
                        else
                        {
                            Debug.Assert(strNstableName == null, "此时必须没有定义'nstable'属性。");
                        }
#endif

                        XPathExpression expr = nav.Compile(strXpath);
                        if (nsmgr != null)
                            expr.SetContext(nsmgr);

                        m_exprCache[nodeXpath] = expr;

                        // 把 convert 参数也缓存起来
                        XmlNode nodeCol = nodeXpath.ParentNode;
                        string strConvert = DomUtil.GetAttr(nodeCol, "convert");
                        if (string.IsNullOrEmpty(strConvert) == false)
                        {
                            List<string> convert_methods = GetMethods(strConvert);
                            m_methodsCache[nodeCol] = convert_methods;
                        }
                        else
                            m_methodsCache[nodeCol] = new List<string>();

                    }
                }

                for (int i = 0; i < nodeListXpath.Count; i++)
                {
                    XmlNode nodeXpath = nodeListXpath[i];
                    string strXpath = nodeXpath.InnerText.Trim(); // 2012/2/16
                    if (string.IsNullOrEmpty(strXpath) == true)
                        continue;

                    // 优化速度 2014/1/29
                    XmlNode nodeCol = nodeXpath.ParentNode;

                    List<string> convert_methods = (List<string>)m_methodsCache[nodeCol];
                    if (convert_methods == null)
                    {
                        Debug.Assert(false, "");
                        string strConvert = DomUtil.GetAttr(nodeCol, "convert");
                        convert_methods = GetMethods(strConvert);
                    }
#if NO
                    string strNstableName = DomUtil.GetAttrDiff(nodeXpath, "nstable");
                    XmlNamespaceManager nsmgr = (XmlNamespaceManager)this.tableNsClient[nodeXpath];
                    if (nsmgr != null)
                    {
                        Debug.Assert(strNstableName != null, "此时应该没有定义'nstable'属性。");
                    }
                    else
                    {
                        Debug.Assert(strNstableName == null, "此时必须没有定义'nstable'属性。");
                    }



                    XPathExpression expr = nav.Compile(strXpath);
                    if (nsmgr != null)
                        expr.SetContext(nsmgr);
#endif
                    XPathExpression expr = (XPathExpression)m_exprCache[nodeXpath];

                    if (expr == null)
                    {
                        this.m_exprCache.Clear();
                        this.m_methodsCache.Clear();
                        goto CREATE_CACHE;  // TODO: 如何预防死循环?
                    }

                    Debug.Assert(expr != null, "");

                    string strText = "";

                    if (expr.ReturnType == XPathResultType.Number)
                    {
                        strText = nav.Evaluate(expr).ToString();//Convert.ToString((int)(nav.Evaluate(expr)));
                        strText = ConvertText(convert_methods, strText);

                    }
                    else if (expr.ReturnType == XPathResultType.Boolean)
                    {
                        strText = Convert.ToString((bool)(nav.Evaluate(expr)));
                        strText = ConvertText(convert_methods, strText);
                    }
                    else if (expr.ReturnType == XPathResultType.String)
                    {
                        strText = (string)(nav.Evaluate(expr));
                        strText = ConvertText(convert_methods, strText);
                    }
                    else if (expr.ReturnType == XPathResultType.NodeSet)
                    {
                        // 看看是否要插入什么分隔符
                        string strSep = GetSepString(convert_methods);

                        XPathNodeIterator iterator = nav.Select(expr);
                        StringBuilder text = new StringBuilder(4096);
                        while (iterator.MoveNext())
                        {
                            XPathNavigator navigator = iterator.Current;
                            string strOneText = navigator.Value;
                            if (strOneText == "")
                                continue;

                            strOneText = ConvertText(convert_methods, strOneText);

                            // 加入分隔符号
                            if (text.Length > 0 && string.IsNullOrEmpty(strSep) == false)
                                text.Append(strSep);

                            text.Append(strOneText);
                        }

                        strText = text.ToString();
                    }
                    else
                    {
                        strError = "XPathExpression的ReturnType为'" + expr.ReturnType.ToString() + "'无效";
                        return -1;
                    }

                    // 空内容也要算作一列

                    // 2008/12/18 new add

                    col_array.Add(strText);
                    nResultLength += strText.Length;
                }
            }
            else if (this.dom.DocumentElement.Prefix == "xsl")
            {
                if (this.m_xt == null)
                {
                    // <col>元素下的<title>元素要去掉
                    XmlDocument temp = new XmlDocument();
                    temp.LoadXml(this.dom.OuterXml);
                    XmlNodeList nodes = temp.DocumentElement.SelectNodes("//col/title");
                    foreach (XmlNode node in nodes)
                    {
                        node.ParentNode.RemoveChild(node);
                    }

                    XmlReader xr = new XmlNodeReader(temp);

                    // 把xsl加到XslTransform
                    XslCompiledTransform xt = new XslCompiledTransform(); // 2006/10/26 changed
                    xt.Load(xr/*, new XmlUrlResolver(), null*/);

                    this.m_xt = xt;
                }

                // 输出到的地方
                TextWriter tw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(tw);

                //执行转换 
                this.m_xt.Transform(domData.CreateNavigator(), /*null,*/ xw /*, null*/);

                tw.Close();
                string strResultXml = tw.ToString();

                XmlDocument resultDom = new XmlDocument();
                try
                {
                    resultDom.LoadXml(strResultXml);
                }
                catch (Exception ex)
                {
                    strError = "browse角色文件生成的结果文件加载到dom出错：" + ex.Message;
                    return -1;
                }

                XmlNodeList colList = resultDom.DocumentElement.SelectNodes("//col");
                foreach (XmlNode colNode in colList)
                {
                    string strColText = colNode.InnerText.Trim();  // 2012/2/16

                    // 2008/12/18
                    string strConvert = DomUtil.GetAttr(colNode, "convert");
                    List<string> convert_methods = GetMethods(strConvert);

                    // 2008/12/18 new add
                    if (String.IsNullOrEmpty(strConvert) == false)
                        strColText = ConvertText(convert_methods, strColText);

                    //if (strColText != "")  //空内容也要算作一列
                    col_array.Add(strColText);
                    nResultLength += strColText.Length;
                }
            }
            else
            {
                strError = "browse角色文件的根元素的前缀'" + this.dom.DocumentElement.Prefix + "'不合法。";
                return -1;
            }

            // 把col_array转到cols里
            cols = new string[col_array.Count + nStartCol];
            col_array.CopyTo(cols, nStartCol);
            // cols = ConvertUtil.GetStringArray(nStartCol, col_array);
            return nResultLength;
        }

#if NO
        static string ConvertText(string strMethods, string strText)
        {
            string[] parts = strMethods.Split(new char[] {','});
            for (int i = 0; i < parts.Length; i++)
            {
                string strMethod = parts[i].Trim();
                if (String.IsNullOrEmpty(strMethod) == true)
                    continue;

                strText = ConvertOneText(strMethod, strText);
            }

            return strText;
        }
#endif
        // 获得分隔符定义
        static string GetSepString(List<string> methods)
        {
            string strMethod = "";
            foreach (string s in methods)
            {
                if (StringUtil.HasHead(s, "join(") == true)
                {
                    strMethod = s;
                    break;
                }
            }

            if (string.IsNullOrEmpty(strMethod) == true)
                return null;
            strMethod = strMethod.Substring("join(".Length).Trim();

            // 去掉末尾 ')'
            if (strMethod.Length > 1)
                strMethod = strMethod.Substring(0, strMethod.Length - 1);

            // TODO: 去掉外围引号 ?
            return strMethod;
        }

        static string ConvertText(List<string> methods, string strText)
        {
            if (methods == null || methods.Count == 0)
                return strText;

            foreach (string strMethod in methods)
            {
                if (String.IsNullOrEmpty(strMethod) == true)
                    continue;

                strText = ConvertOneText(strMethod, strText);
            }

            return strText;
        }

        static string ConvertOneText(string strMethod, string strText)
        {
            if (strMethod == null)
                return strText;

            strMethod = strMethod.Trim().ToLower();

            if (strMethod == "rfc1123tolocaltime")
            {
                return DateTimeUtil.LocalTime(strText);
            }

            if (strMethod == "rfc1123tolocaltimeu")
            {
                return DateTimeUtil.LocalTime(strText, "u");
            }

            if (strMethod == "rfc1123tolocaltimes")
            {
                return DateTimeUtil.LocalTime(strText, "s");
            }
            if (strMethod == "rfc1123tolocaldate")
            {
                return DateTimeUtil.LocalDate(strText);
            }

            if (strMethod == "toupper")
            {
                return strText.ToUpper();
            }

            if (strMethod == "tolower")
            {
                return strText.ToLower();
            }

            if (strMethod == "removecmdcr")
            {
                string strCmd = StringUtil.GetLeadingCommand(strText);
                if (string.IsNullOrEmpty(strCmd) == false
                    && StringUtil.HasHead(strCmd, "cr:") == true)
                    return strText.Substring(strCmd.Length + 2);

                return strText;
            }

            if (strMethod == "chineset2s")
            {
                return API.ChineseT2S(strText);
            }

            if (strMethod == "chineses2t")
            {
                return API.ChineseS2T(strText);
            }

            return strText;
        }
    }

}
