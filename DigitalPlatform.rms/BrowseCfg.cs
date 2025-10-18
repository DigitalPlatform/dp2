using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Jint;

using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System.Linq;

namespace DigitalPlatform.rms
{
    // 浏览格式
    public class BrowseCfg : KeysBrowseBase
    {
        // col 或 use 或 script 元素 --> CacheItem 对照表
        Hashtable m_exprCache = new Hashtable();

        /*
        // col 元素 --> ColCacheItem 对照表
        Hashtable _colCache = new Hashtable();
        */

        List<string> _useNames = new List<string>();
        // Hashtable m_methodsCache = new Hashtable();

        // XmlDocument m_domTransform = null;  // 用于转换的专用DOM，由 this.dom 将<col>元素下的<title>元素去掉后产生
        XslCompiledTransform m_xt = null;

        public override void Clear()
        {
            base.Clear();
            m_exprCache.Clear();
            // _colCache.Clear();
            _useNames.Clear();
            // m_methodsCache.Clear();
            this.m_xt = null;
        }

        static List<string> GetMethods(string strConvert)
        {
            // List<string> col_convert_methods = StringUtil.SplitList(strConvert);
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
                // 注 <use>...</use> 中允许使用逗号间隔的多个名字
                var text = use.InnerText.Trim();
                var list = StringUtil.SplitList(text);
                cols.AddRange(list);
            }

            return cols;
        }

        class CacheItem
        {
            public XPathExpression expr { get; set; }

            public List<string> convert_methods { get; set; }

            public string InnerText { get; set; }

            public XmlElement Element { get; set; }

            //public string Use { get; set; }
            // javascript 脚本
            //public string Script { get; set; }
        }

        /*
        class ColCacheItem
        {
            public List<string> col_convert_methods { get; set; }
        }
        */

        // 设置一些通用 Cache 属性
        // 包括 convert .InnerText
        CacheItem SetNormalCache(XmlElement nodeCol)
        {
            var col_cache_item = new CacheItem();
            m_exprCache[nodeCol] = col_cache_item;
            col_cache_item.Element = nodeCol;
            // 把 convert 参数缓存起来
            string strConvert = nodeCol.GetAttribute("convert");
            if (string.IsNullOrEmpty(strConvert) == false)
            {
                List<string> convert_methods = GetMethods(strConvert);
                col_cache_item.convert_methods = convert_methods;
            }
            else
                col_cache_item.convert_methods = new List<string>();

            string strUse = nodeCol.InnerText.Trim();
            if (string.IsNullOrEmpty(strUse) == false)
            {
                col_cache_item.InnerText = strUse;
            }

            return col_cache_item;
        }

        CacheItem GetCacheItem(XmlElement element)
        {
            return (m_exprCache[element] as CacheItem);
        }

        // TODO: XPathExpression可以缓存起来，加快速度
        // 创建指定记录的浏览格式集合
        // parameters:
        //		domData	    记录数据dom 不能为null
        //      nStartCol   (废止)开始的列号。一般为0
        //      style       处理风格。(尚未实现) "title:c1|c2" 指要在列内容中包含列标题
        //      cols        浏览格式数组
        //		strError	out参数，出错信息
        // return:
        //		-1	出错
        //		>=0	成功。数字值代表每个列包含的字符数之和
        public int BuildCols(XmlDocument domData,
            // int nStartCol,
            string style,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = new string[0];

            Debug.Assert(domData != null, "BuildCols()调用错误，domData参数不能为null。");

            // 没有浏览格式定义时，就没有信息
            if (this._dom == null)
                return 0;

            /*
            List<string> type_list = null;
            var title_string = StringUtil.GetParameterByPrefix(style, "titles");
            if (title_string != null)
            {
                if (string.IsNullOrEmpty(title_string))
                    type_list = new List<string>(); // 表示任意 type 值都匹配
                else
                    type_list = StringUtil.SplitList(title_string, '|');
            }
            */

            // int nResultLength = 0;

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
                        SetNormalCache(nodeCol);

                        // XmlElement nodeXPath = nodeCol.SelectSingleNode("xpath") as XmlElement;
                        // 2025/10/18 改为支持 col 下多个 xpath 元素
                        var xpath_nodes = nodeCol.SelectNodes("xpath");
                        foreach (XmlElement nodeXPath in xpath_nodes)
                        {
                            CacheItem cache_item = SetNormalCache(nodeXPath);

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

                                try
                                {
                                    XPathExpression expr = nav.Compile(strXpath);
                                    if (nsmgr != null)
                                        expr.SetContext(nsmgr);

                                    cache_item.expr = expr;
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"{ex.Message}。XPath='{strXpath}'", ex);
                                }
                            }

                            /*
                            // 把 convert 参数也缓存起来
                            // XmlNode nodeCol = nodeXpath.ParentNode;
                            string strConvert = nodeCol.GetAttribute("convert");
                            if (string.IsNullOrEmpty(strConvert) == false)
                            {
                                List<string> col_convert_methods = GetMethods(strConvert);
                                cache_item.col_convert_methods = col_convert_methods;
                            }
                            else
                                cache_item.col_convert_methods = new List<string>();
                            */


                        }

                        // 把 use 元素 InnerText 缓存起来
                        foreach (XmlElement nodeUse in nodeCol.SelectNodes("use"))
                        {
                            // XmlElement nodeUse = nodeCol.SelectSingleNode("use") as XmlElement;
                            SetNormalCache(nodeUse);
                        }

                        // 把 script 元素 InnerText 缓存起来
                        foreach (XmlElement nodeScript in nodeCol.SelectNodes("script"))
                        {
                            SetNormalCache(nodeScript);
                        }
                    }
                }

                /*
                    Dictionary<string, MarcColumn> browse_table = null;
                    string filter = this._dom.DocumentElement.GetAttribute("filter");
                    if (filter == "marc" && this._useNames.Count > 0)
                    {
                        browse_table = MarcBrowse.Build(domData,
                            this._useNames);
                    }
                */
                Dictionary<string, MarcColumn> browse_table = null;
                // 尽量延迟创建
                Dictionary<string, MarcColumn> EnsureMarcBrowseBuilt()
                {
                    if (browse_table != null)
                        return browse_table;
                    if (browse_table == null
                        // && this._dom.DocumentElement.GetAttribute("filter") == "marc"
                        && this._useNames.Count > 0)
                    {
                        browse_table = MarcBrowse.Build(domData,
                            this._useNames);
                        return browse_table;
                    }
                    browse_table = new Dictionary<string, MarcColumn>();
                    return browse_table;
                }

                MarcRecord record = null;
                string strMarcSyntax = null;
                Engine engine = null;

                foreach (XmlElement nodeCol in nodeListCol)
                {
                    string prefix = nodeCol.GetAttribute("prefix");
                    string postfix = nodeCol.GetAttribute("postfix");

                    // 一个 col 的文字堆积
                    StringBuilder colText = new StringBuilder();
                    colText.Append(nodeCol.GetAttribute("text"));

                    CacheItem col_cache_item = GetCacheItem(nodeCol);
                    List<string> col_convert_methods = col_cache_item?.convert_methods;
                    // var convert = nodeCol.GetAttribute("convert");

                    // 看看是否要插入什么分隔符
                    string strColSep = GetSepString(col_convert_methods) ?? "";

                    void AppendText(string new_text, string sep)
                    {
                        if (string.IsNullOrEmpty(new_text) == false
                            && colText.Length > 0
                            && string.IsNullOrEmpty(sep) == false
                            )
                            colText.Append(sep);
                        colText.Append(new_text);
                    }

                    foreach (XmlElement child in nodeCol.ChildNodes)
                    {
                        CacheItem child_cache_item = GetCacheItem(child);
                        List<string> child_convert_methods = child_cache_item?.convert_methods;

                        // TODO: 也放入缓存中
                        string child_sep = GetSepString(child_convert_methods) ?? "";


                        if (child.Name == "xpath")
                        {
                            // xpath 元素取出 CacheItem
                            CacheItem cache_item = m_exprCache[child] as CacheItem;

                            XPathExpression expr = cache_item.expr;

                            // xpath 元素
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
                                    var current = nav.Evaluate(expr).ToString();//Convert.ToString((int)(nav.Evaluate(expr)));
                                    current = (ConvertText(col_convert_methods, current));
                                    AppendText(current, strColSep);
                                }
                                else if (expr.ReturnType == XPathResultType.Boolean)
                                {
                                    var current = Convert.ToString((bool)(nav.Evaluate(expr)));
                                    current = (ConvertText(col_convert_methods, current));
                                    AppendText(current, strColSep);
                                }
                                else if (expr.ReturnType == XPathResultType.String)
                                {
                                    var current = (string)(nav.Evaluate(expr));
                                    current = ConvertText(col_convert_methods, current);
                                    AppendText(current, strColSep);
                                }
                                else if (expr.ReturnType == XPathResultType.NodeSet)
                                {
                                    var current_text = new StringBuilder(4096);
                                    var iterator = nav.Select(expr);
                                    while (iterator.MoveNext())
                                    {
                                        XPathNavigator navigator = iterator.Current;
                                        string strOneText = navigator.Value;
                                        if (strOneText == "")
                                            continue;

                                        strOneText = ConvertText(col_convert_methods, strOneText);

                                        // 加入分隔符号
                                        if (current_text.Length > 0 && string.IsNullOrEmpty(strColSep) == false)
                                            current_text.Append(strColSep);

                                        current_text.Append(strOneText);
                                    }

                                    AppendText(current_text.ToString(), strColSep);
                                }
                                else
                                {
                                    strError = "XPathExpression 的 ReturnType 为 '" + expr.ReturnType.ToString() + "' 无效";
                                    return -1;
                                }
                            }
                        }

                        // TODO: use 元素也可以有自己的 convert 和 prefix 属性
                        // use 元素
                        else if (child.Name == "use")    // string.IsNullOrEmpty(cache_item.Use) == false
                        {
                            /*
                            // 2021/9/10
                            if (browse_table == null)
                            {
                                strError = "MARC 浏览 browse_table == null";
                                // strError = "browse 配置文件根元素应当具备属性 filter='marc'";
                                return -1;
                            }
                            */

                            string use_value = child.InnerText.Trim();

                            StringBuilder current_text = new StringBuilder();
                            // <use></use> 内的文本允许使用逗号间隔的多个名字
                            var use_list = StringUtil.SplitList(use_value, ",");

                            EnsureMarcBrowseBuilt();
                            foreach (var use in use_list)
                            {
                                browse_table.TryGetValue(use, out MarcColumn column);
                                if (column != null && string.IsNullOrEmpty(column.Value) == false)
                                {
                                    // 加入分隔符号
                                    if (current_text.Length > 0 && string.IsNullOrEmpty(child_sep) == false)
                                        current_text.Append(child_sep);
                                    current_text.Append(column.Value);
                                }
                            }

                            // 2025/10/17 不要忘了加入 error 列
                            if (browse_table.ContainsKey("error")
                                && use_list.Contains("error") == false)
                            {
                                current_text.Append("error: " + browse_table["error"].Value);
                            }

                            AppendText(current_text.ToString(), strColSep);
                        }

                        // script 元素
                        else if (child.Name == "script")   // string.IsNullOrEmpty(cache_item.Script) == false
                        {
                            // TODO: 提升到高层创建
                            int nRet = GetMarcRecord(domData,
        ref record,
        ref strMarcSyntax,
        out strError);
                            if (nRet == -1)
                                return -1;

                            var script_value = child.InnerText.Trim();

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
                                + script_value) // execute a statement
                                ?.GetCompletionValue() // get the latest statement completion value
                                ?.ToObject()?.ToString() // converts the value to .NET
                                ;
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                AppendText(result, strColSep);
                            }
                        }
                        else
                            continue;

                        // 空内容也要算作一列

                        /*
                        // 2022/7/22
                        // 包含列标题
                        if (string.IsNullOrEmpty(strText) == false
                            && type_list != null
                            && (type_list.Count == 0 || type_list.IndexOf(type) != -1))
                            strText = $"~{type}:" + strText;
                        */

                    }

                    // 2022/7/22 支持前缀
                    // 2025/10/18 支持后缀
                    if (colText.Length > 0)
                    {
                        if (string.IsNullOrEmpty(prefix) == false)
                            colText.Insert(0, prefix);
                        if (string.IsNullOrEmpty(postfix) == false)
                            colText.Append(postfix);
                    }
                    col_array.Add(colText.ToString());
                    // nResultLength += colText.Length;
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
                    foreach (XmlElement colNode in colList)
                    {
                        string strColText = colNode.InnerText.Trim();  // 2012/2/16

                        // 2008/12/18
                        string strConvert = DomUtil.GetAttr(colNode, "convert");
                        List<string> convert_methods = GetMethods(strConvert);

                        // 2008/12/18
                        if (String.IsNullOrEmpty(strConvert) == false)
                            strColText = ConvertText(convert_methods, strColText);

                        //if (strColText != "")  //空内容也要算作一列

                        // 2022/7/22
                        // 包含前缀
                        string prefix = colNode.GetAttribute("prefix");
                        if (string.IsNullOrEmpty(strColText) == false
                            && string.IsNullOrEmpty(prefix) == false)
                            strColText = prefix + strColText;

                        // 2025/10/18
                        string postfix = colNode.GetAttribute("postfix");
                        if (string.IsNullOrEmpty(strColText) == false
                            && string.IsNullOrEmpty(postfix) == false)
                            strColText += postfix;

                        col_array.Add(strColText);
                        // nResultLength += strColText.Length;
                    }
                }
                catch (Exception ex)
                {
                    strError = "!error: browse XSLT 生成的结果文件加载到 XMLDOM 时出错：" + ex.Message;
                    // return -1;
                    col_array.Add(strError);
                    // nResultLength += strError.Length;
                }
            }
            else
            {
                strError = "browse 角色文件的根元素的前缀'" + this._dom.DocumentElement.Prefix + "'不合法。";
                return -1;
            }

            // 把col_array转到cols里
            cols = col_array.ToArray();
            // return nResultLength;
            return col_array.Sum(o => o.Length);
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
            if (methods == null)
                return null;

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
