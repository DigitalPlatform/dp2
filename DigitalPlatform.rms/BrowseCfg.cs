﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;
using System.IO;

using Jint;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;

namespace DigitalPlatform.rms
{
    // 浏览格式
    public class BrowseCfg : KeysBrowseBase
    {
        Hashtable m_exprCache = new Hashtable();
        List<string> _useNames = new List<string>();
        // Hashtable m_methodsCache = new Hashtable();

        // XmlDocument m_domTransform = null;  // 用于转换的专用DOM，由 this.dom 将<col>元素下的<title>元素去掉后产生
        XslCompiledTransform m_xt = null;

        public override void Clear()
        {
            base.Clear();
            m_exprCache.Clear();
            _useNames.Clear();
            // m_methodsCache.Clear();
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

        List<string> GetUseColNames()
        {
            // 汇总需要创建的列名
            XmlNodeList nodes = this._dom.DocumentElement.SelectNodes("col/use");
            List<string> cols = new List<string>();
            foreach (XmlElement use in nodes)
            {
                cols.Add(use.InnerText.Trim());
            }

            return cols;
        }

        class CacheItem
        {
            public XPathExpression expr { get; set; }
            public List<string> convert_methods { get; set; }
            public string Use { get; set; }
            // javascript 脚本
            public string Script { get; set; }
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
            if (this._dom == null)
                return 0;

            int nResultLength = 0;

            XPathNavigator nav = domData.CreateNavigator();

            List<string> col_array = new List<string>();

            if (this._dom.DocumentElement.Prefix == "")
            {

                // 得到xpath的值
                // XmlNodeList nodeListXpath = this._dom.SelectNodes(@"//xpath");
                XmlNodeList nodeListCol = this._dom.DocumentElement.SelectNodes("col");

                CREATE_CACHE:
                // 创建Cache
                if (m_exprCache.Count == 0 && nodeListCol.Count > 0)
                {
                    // 汇总需要创建的列名
                    this._useNames = GetUseColNames();

                    foreach (XmlElement nodeCol in nodeListCol)
                    {
                        CacheItem cache_item = new CacheItem();
                        m_exprCache[nodeCol] = cache_item;

                        // XmlNode nodeXpath = nodeListXpath[i];
                        XmlElement nodeXPath = nodeCol.SelectSingleNode("xpath") as XmlElement;
                        string strXpath = "";
                        if (nodeXPath != null)
                            strXpath = nodeXPath.InnerText.Trim();
                        if (string.IsNullOrEmpty(strXpath) == false)
                        {
                            string strNstableName = DomUtil.GetAttrDiff(nodeXPath, "nstable");
                            XmlNamespaceManager nsmgr = (XmlNamespaceManager)this.tableNsClient[nodeXPath];
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

                            cache_item.expr = expr;
                        }

                        // 把 convert 参数也缓存起来
                        // XmlNode nodeCol = nodeXpath.ParentNode;
                        string strConvert = DomUtil.GetAttr(nodeCol, "convert");
                        if (string.IsNullOrEmpty(strConvert) == false)
                        {
                            List<string> convert_methods = GetMethods(strConvert);
                            cache_item.convert_methods = convert_methods;
                        }
                        else
                            cache_item.convert_methods = new List<string>();

                        // 把 use 元素 text 缓存起来
                        {
                            XmlElement nodeUse = nodeCol.SelectSingleNode("use") as XmlElement;
                            string strUse = "";
                            if (nodeUse != null)
                                strUse = nodeUse.InnerText.Trim();
                            if (string.IsNullOrEmpty(strUse) == false)
                            {
                                cache_item.Use = strUse;
                            }
                        }

                        // 2018/9/29
                        // 把 script 元素 text 缓存起来
                        {
                            XmlElement script = nodeCol.SelectSingleNode("script") as XmlElement;
                            cache_item.Script = script?.InnerText.Trim();
                        }
                    }
                }

                Dictionary<string, MarcColumn> results = null;
                string filter = this._dom.DocumentElement.GetAttribute("filter");
                if (filter == "marc" && this._useNames.Count > 0)
                {
                    results = MarcBrowse.Build(domData,
                        this._useNames);
                }

                MarcRecord record = null;
                string strMarcSyntax = null;
                Engine engine = null;

                foreach (XmlElement nodeCol in nodeListCol)
                {
#if NO
                    XmlNode nodeXpath = nodeListXpath[i];
                    string strXpath = nodeXpath.InnerText.Trim(); // 2012/2/16
                    if (string.IsNullOrEmpty(strXpath) == true)
                        continue;

                    // 优化速度 2014/1/29
                    XmlNode nodeCol = nodeXpath.ParentNode;
#endif
                    CacheItem cache_item = m_exprCache[nodeCol] as CacheItem;
                    List<string> convert_methods = cache_item.convert_methods;
                    if (convert_methods == null)
                    {
                        Debug.Assert(false, "");
                        string strConvert = DomUtil.GetAttr(nodeCol, "convert");
                        convert_methods = GetMethods(strConvert);
                    }

                    string strText = "";

                    XPathExpression expr = cache_item.expr;

                    if (expr != null)
                    {
                        if (expr == null)
                        {
#if NO
                        this.m_exprCache.Clear();
                        this.m_methodsCache.Clear();
                        goto CREATE_CACHE;  // TODO: 如何预防死循环?
#endif
                        }

                        Debug.Assert(expr != null, "");

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
                    }

                    if (string.IsNullOrEmpty(cache_item.Use) == false)
                    {
                        MarcColumn column = null;
                        results.TryGetValue(cache_item.Use, out column);
                        if (column != null && string.IsNullOrEmpty(column.Value) == false)
                            strText += column.Value;
                    }

                    if (string.IsNullOrEmpty(cache_item.Script) == false)
                    {
                        int nRet = GetMarcRecord(domData,
    ref record,
    ref strMarcSyntax,
    out strError);
                        if (nRet == -1)
                            return -1;

#if NO
                        Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();
                        engine.EnableExposedClrTypes = true;
                        engine.SetGlobalValue("syntax", strMarcSyntax);
                        engine.SetGlobalValue("biblio", record);
                        string result = engine.Evaluate(cache_item.Script).ToString();
                        if (string.IsNullOrEmpty(result) == false)
                            strText += result;
#endif
                        if (engine == null)
                            engine = new Engine(cfg => cfg.AllowClr(typeof(MarcQuery).Assembly))
        .SetValue("syntax", strMarcSyntax)
        .SetValue("biblio", record);

                        string result = engine.Execute("var DigitalPlatform = importNamespace('DigitalPlatform');\r\n"
                            + cache_item.Script) // execute a statement
                            ?.GetCompletionValue() // get the latest statement completion value
                            ?.ToObject()?.ToString() // converts the value to .NET
                            ;
                        if (string.IsNullOrEmpty(result) == false)
                            strText += result;
                    }

                    // 空内容也要算作一列

                    // 2008/12/18

                    col_array.Add(strText);
                    nResultLength += strText.Length;
                }
            }
            else if (this._dom.DocumentElement.Prefix == "xsl")
            {
                if (this.m_xt == null)
                {
                    // <col>元素下的<title>元素要去掉
                    XmlDocument temp = new XmlDocument();
                    temp.LoadXml(this._dom.OuterXml);
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
                string strResultXml = "";

                using (TextWriter tw = new StringWriter())
                using (XmlTextWriter xw = new XmlTextWriter(tw))
                {
                    //执行转换 
                    this.m_xt.Transform(domData.CreateNavigator(), /*null,*/ xw /*, null*/);

                    // tw.Close();
                    tw.Flush(); // 2015/11/24 增加此句

                    strResultXml = tw.ToString();
                }

                XmlDocument resultDom = new XmlDocument();
                try
                {
                    if (string.IsNullOrEmpty(strResultXml) == false)
                        resultDom.LoadXml(strResultXml);
                    else
                        resultDom.LoadXml("<root />");

                    XmlNodeList colList = resultDom.DocumentElement.SelectNodes("//col");
                    foreach (XmlNode colNode in colList)
                    {
                        string strColText = colNode.InnerText.Trim();  // 2012/2/16

                        // 2008/12/18
                        string strConvert = DomUtil.GetAttr(colNode, "convert");
                        List<string> convert_methods = GetMethods(strConvert);

                        // 2008/12/18
                        if (String.IsNullOrEmpty(strConvert) == false)
                            strColText = ConvertText(convert_methods, strColText);

                        //if (strColText != "")  //空内容也要算作一列
                        col_array.Add(strColText);
                        nResultLength += strColText.Length;
                    }
                }
                catch (Exception ex)
                {
                    strError = "!error: browse XSLT 生成的结果文件加载到 XMLDOM 时出错：" + ex.Message;
                    // return -1;
                    col_array.Add(strError);
                    nResultLength += strError.Length;
                }
            }
            else
            {
                strError = "browse 角色文件的根元素的前缀'" + this._dom.DocumentElement.Prefix + "'不合法。";
                return -1;
            }

            // 把col_array转到cols里
            cols = new string[col_array.Count + nStartCol];
            col_array.CopyTo(cols, nStartCol);
            // cols = ConvertUtil.GetStringArray(nStartCol, col_array);
            return nResultLength;
        }

        int GetMarcRecord(XmlDocument domData,
            ref MarcRecord record,
            ref string strMarcSyntax,
            out string strError)
        {
            strError = "";

            if (record != null)
                return 0;

            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(domData,
                true,
                "",
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "XML 转换到 MARC 时出错: " + strError;
                return -1;
            }

            record = new MarcRecord(strMarc);
            strMarcSyntax = strOutMarcSyntax;
            return 0;
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
            if (strMethod.Length > 0)
                strMethod = strMethod.Substring(0, strMethod.Length - 1);

            strMethod = StringUtil.UnescapeString(strMethod);

            // TODO: 去掉外围引号 ?
            return strMethod;
        }

        public static string ConvertText(List<string> methods, string strText)
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
