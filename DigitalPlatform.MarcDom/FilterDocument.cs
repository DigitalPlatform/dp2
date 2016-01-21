using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;

// 2005/4/18	增加PrevName NextName DupCount变量
// 2005/4/18	改变结构体元素name内容定义办法，加@为regular expression, 否则为原来的*字符串

namespace DigitalPlatform.MarcDom
{
    /// <summary>
    // MARC记录过滤器
    // 可重载FilterDocument类。这样，就可以在其中扩展一些用于存储的成员，
    // 便于连接其他host对象。可惜从Script代码中FilterItem.Document角度看到的
    // 可能还是FilterDocument基类型，使用中需要cast，不过也可以在
    // script代码中为Document成员重载函数，返回实际类型?
    // 可以考虑为<def>或<begin>配置一段内定的隐藏代码，即定义实际类型转换的代码。
    /// </summary>
    public class FilterDocument
    {
        public XmlDocument Dom
        {
            get
            {
                return this.dom;
            }
        }

        XmlDocument dom = new XmlDocument();

        Hashtable NodeTable = new Hashtable();

        Assembly assembly = null;

        public string strOtherDef = "";

        public string strPreInitial = "";

        public bool CheckBreakException = true;

        public Assembly Assembly
        {
            get
            {
                return assembly;
            }
            set
            {
                assembly = value;
                // 得到一层对象的type信息

                if (value == null)
                {
                    this.NodeTable.Clear();	// 释放那些type entry指针
                    // Debug.WriteLine("NodeTable Cleared. count" + NodeTable.Count.ToString());
                }
                else
                {
                    string strError;
                    int nRet = FillOneLevelType(dom.DocumentElement,
                        out strError);
                    if (nRet == -1)
                        throw (new Exception("FillOneLevelType() error :" + strError));
                }
            }
        }

        public void Load(string strFileName)
        {
            dom.Load(strFileName);

            BuildOneLevelItem(dom.DocumentElement);
        }

        public void LoadContent(string strFileContent)
        {
            dom.LoadXml(strFileContent);

            BuildOneLevelItem(dom.DocumentElement);
        }

        // 获得一个指定语言的字符串
        public string GetString(string strLang,
            string strID)
        {
            if (this.dom == null)
                return null;
            XmlNode node = dom.DocumentElement.SelectSingleNode("//stringTable/s[@id='" + strID + "']");
            if (node == null)
                return null;

            return GetT(strLang, node);
        }

        // 获得当前语言的字符串
        // 如果没有精确匹配的语言，就模糊匹配，或返回第一个语言的
        // 但如果id不存在，返回null
        public string GetString(string strID)
        {
            if (this.dom == null)
                return null;
            XmlNode node = dom.DocumentElement.SelectSingleNode("//stringTable/s[@id='" + strID + "']");
            if (node == null)
                return null;

            string strLang = Thread.CurrentThread.CurrentUICulture.Name;
            return GetT(strLang, node);
        }

        // 确保最坏的情况下也返回strID本身
        public string GetStringSafe(string strID)
        {
            string strResult = this.GetString(strID);

            if (String.IsNullOrEmpty(strResult) == true)
                return strID;

            return strResult;
        }

        // 从一个元素的下级<t>元素中, 提取语言符合的文字值
        public static string GetT(string strLang,
            XmlNode parent)
        {
            XmlNode node = null;

            if (String.IsNullOrEmpty(strLang) == true)
            {
                node = parent.SelectSingleNode("t");  // 第一个caption元素
                if (node != null)
                    return node.InnerText;

                return null;
            }
            else
            {
                node = parent.SelectSingleNode("t[@lang='" + strLang + "']");

                if (node != null)
                    return node.InnerText;
            }

            string strLangLeft = "";
            string strLangRight = "";

            DomUtil.SplitLang(strLang,
               out strLangLeft,
               out strLangRight);

            // 所有<t>元素
            XmlNodeList nodes = parent.SelectNodes("t");

            for (int i = 0; i < nodes.Count; i++)
            {
                string strThisLang = DomUtil.GetAttr(nodes[i], "lang");

                string strThisLangLeft = "";
                string strThisLangRight = "";

                DomUtil.SplitLang(strThisLang,
                   out strThisLangLeft,
                   out strThisLangRight);

                // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的

                if (strThisLangLeft == strLangLeft)
                    return nodes[i].InnerText;
            }

            // 实在不行，则选第一个<t>的文字值
            node = parent.SelectSingleNode("t");
            if (node != null)
                return node.InnerText;

            return null;    // not found
        }

        /// <summary>
        /// 去掉最末一个标点符号
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string TrimEndChar(string strText, string strDelimeters = "./,;:")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char tail = strText[strText.Length - 1];
            if (strDelimeters.IndexOf(tail) != -1)
                return strText.Substring(0, strText.Length - 1);
            return strText;
        }

        // 创建.fltx.cs文件
        // 写入指定文件的版本
        public int BuildScriptFile(string strOutputFile,
            out string strError)
        {
            string strText = "";
            strError = "";

            int nRet = BuildOneLevelScript(dom.DocumentElement,
                out strText,
                out strError);

            if (nRet == -1)
                return -1;

            // 写入文件
            using (StreamWriter sw = new StreamWriter(strOutputFile, false, Encoding.UTF8))
            {
                sw.WriteLine(strText);
            }
            return 0;
        }

        // 创建.fltx.cs文件
        // 返回字符串的版本
        public int BuildScriptFile(out string strCode,
            out string strError)
        {
            strCode = "";
            strError = "";

            int nRet = BuildOneLevelScript(dom.DocumentElement,
                out strCode,
                out strError);

            if (nRet == -1)
                return -1;

            return 0;
        }

        FilterItem GetRootFilterItem(FilterItem start)
        {
            FilterItem item = start;
            for (; item != null; )
            {
                if (item.FilterRoot == item)
                    return item;
                item = item.Container;
            }

            return null;
        }

        // 包装版本
        // 处理一条记录
        // return:
        //		-1	出错
        public int DoRecord(
            object objParam,
            string strMarcRecord,
            int nIndex,
            out string strError)
        {
            return DoRecord(
                objParam,
                strMarcRecord,
                "", // strMarcSyntax，""表示对每个<record>元素均匹配
                nIndex,
                out strError);
        }

        // 原始版本
        // 处理一条记录
        // parameters:
        //      strMarcSyntax   MARC语法
        // return:
        //		-1	出错
        public int DoRecord(
            object objParam,
            string strMarcRecord,
            string strMarcSyntax,
            int nIndex,
            out string strError)
        {
            int nRet;
            strError = "";

            strMarcSyntax = strMarcSyntax.ToLower();

            FilterItem itemFilter = null;

            // 如果fltx中定义了<filter>节点的话
            XmlNode nodeFilter = dom.DocumentElement.SelectSingleNode("//filter");
            if (nodeFilter != null)
            {
                itemFilter = this.NewFilterItem(
                    objParam,
                    nodeFilter,
                    out strError);
                if (itemFilter == null)
                    return -1;

                // 执行begin部分代码
                itemFilter.Container = null;
                itemFilter.FilterRoot = itemFilter;
                itemFilter.Index = nIndex;
                itemFilter.OnBegin();

                if (itemFilter.Break == BreakType.SkipCase)
                    goto DOEND;
                if (itemFilter.Break == BreakType.SkipCaseEnd)
                    return 0;	// 立即结束
            }

            // ***
            itemFilter.IncChildDupCount("");

            // fltx.cs中可能定义多个<record>对应类，应当依次创建并执行

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//record");
            if (nodes.Count == 0)
                goto DOEND;	// 一个<record>也没有定义

            BreakType thisBreak = BreakType.None;

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                /*
                 * 注：
                 * 1) 如果用空strMarcSyntax值调用本函数，则任何<record>元素都匹配。这是为了和以前的兼容
                 * 2) 如果<record>元素没有syntax属性，即便用非空的strMarcSyntax调用本函数，这样的元素也算匹配
                 * 3) 如果<record>元素有syntax属性，则要和非空的strMarcSyntax值匹配，才算匹配<record>上元素
                 * 
                 * */

                // 2009/10/8
                // 检查marc syntax是否匹配
                if (String.IsNullOrEmpty(strMarcSyntax) == false)
                {
                    string strNodeSyntax = DomUtil.GetAttr(node, "syntax").ToLower();

                    if (String.IsNullOrEmpty(strNodeSyntax) == false)
                    {
                        if (strNodeSyntax != strMarcSyntax)
                            continue;
                    }
                }

                nRet = DoSingleItem(
                    objParam,
                    node,
                    nIndex,
                    itemFilter,
                    strMarcRecord,
                    "",
                    out thisBreak,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (thisBreak != BreakType.None)
                    break;

                // DoSingleItem()执行中完全可能改到filter的成员变量
                if (itemFilter != null)
                {
                    if (itemFilter.Break == BreakType.SkipCase)
                        goto DOEND;
                    if (itemFilter.Break == BreakType.SkipCaseEnd)
                        return 0;	// 立即结束
                }
            }

        DOEND:
            if (itemFilter != null)
            {
                // 执行end部分代码
                itemFilter.OnEnd();
            }
            return 0;
        }

        #region 内部逻辑

        // 创建一层FilterItem对象
        int BuildOneLevelItem(XmlNode xmlNode)
        {
            if (xmlNode.ParentNode == null)
            {
                NodeTable.Clear();
                // Debug.WriteLine("NodeTable Cleared. count" + NodeTable.Count.ToString());
            }

            if (xmlNode.NodeType != XmlNodeType.Element)
            {
                Debug.Assert(false,
                    "xmlNode类型必须为XmlNodeType.Element");
                return -1;
            }

            if (IsStructureElementName(xmlNode.Name) == false)
            {
                Debug.Assert(false,
                    "xmlNode的name属性值只能为结构元素");
                return -1;
            }

            HashFilterItem item = new HashFilterItem();

            item.xmlNode = xmlNode;
            item.Name = DomUtil.GetAttr(xmlNode, "name");
            item.ItemType = (ItemType)Enum.Parse(typeof(ItemType), xmlNode.Name, true);
            NodeTable.Add(xmlNode, item);
            // Debug.WriteLine("add new NodeTable count" + NodeTable.Count.ToString());

            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                XmlNode node = xmlNode.ChildNodes[i];

                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (IsStructureElementName(node.Name) == false)
                    continue;

                int nRet = BuildOneLevelItem(node);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 创建一层对象的相关Script代码
        int BuildOneLevelScript(XmlNode xmlNode,
            out string strResult,
            out string strError)
        {
            strResult = "";
            int nRet;

            string strUsingScript = "";

            if (xmlNode.ParentNode == xmlNode.OwnerDocument)
            {
                nRet = GetUsingScript(xmlNode,
                    out strUsingScript,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strTab = GetTabString(xmlNode);

            string strBeginScript = "";
            string strEndScript = "";
            string strDefScript = "";

            nRet = GetDefBeginEndScript(xmlNode,
                out strDefScript,
                out strBeginScript,
                out strEndScript,
                out strError);
            if (nRet == -1)
                return -1;

            string strClassName = GetClassName(xmlNode);

            // 
            HashFilterItem item = (HashFilterItem)this.NodeTable[xmlNode];
            if (item == null)
            {
                Debug.Assert(false, "xml节点" + xmlNode.OuterXml + "没有在NodeTable中创建对应的事项");
                return -1;
            }
            item.FunctionName = strClassName;

            if (strUsingScript != "")
                strResult = strUsingScript;

            strResult += strTab + "public class " + strClassName + " : FilterItem { ";

            strResult += "// name=" + DomUtil.GetAttr(xmlNode, "name") + "\r\n";

            if (strOtherDef != null)
                strResult += strOtherDef + "\r\n";

            // def
            if (strDefScript != "")
            {
                strResult += strTab + "// fltx def\r\n";
                strResult += strDefScript;
                strResult += strTab + "\r\n";
            }


            // 创建Parent属性代码
            HashFilterItem itemParent = null;
            if (xmlNode != dom.DocumentElement
                && xmlNode.ParentNode != null
                // && xmlNode.ParentNode != xmlNode.OwnerDocument
                )
            {
                itemParent = (HashFilterItem)this.NodeTable[xmlNode.ParentNode];
                if (itemParent == null)
                {
                    Debug.Assert(false, "xml节点" + xmlNode.ParentNode.OuterXml + "没有在NodeTable中创建对应的事项");
                    return -1;
                }
            }


            if (itemParent != null)
            {
                strResult += strTab + "public " + itemParent.FunctionName +
                    " Parent { get { return (" + itemParent.FunctionName + ")Container;} } \r\n";
            }
            else
            {
                strResult += strTab + "public " + "FilterItem" +
                    " Parent { get { return (" + "FilterItem" + ")null;} } \r\n";
            }

            // 创建Root属性代码
            HashFilterItem itemRoot = null;
            itemRoot = (HashFilterItem)this.NodeTable[xmlNode.OwnerDocument.DocumentElement];
            if (itemRoot == null)
            {
                Debug.Assert(false, "xml节点" + xmlNode.OwnerDocument.DocumentElement.OuterXml + "没有在NodeTable中创建对应的事项");
                return -1;
            }

            strResult += strTab + "public " + itemRoot.FunctionName +
                " Root { get { return (" + itemRoot.FunctionName + ")FilterRoot;} } \r\n";

            if (strPreInitial != "")
            {
                // 初始化函数
                strResult += strTab + "public override void PreInitial() {\r\n";

                strResult += strPreInitial + "\r\n";

                strResult += strTab + "}\r\n";
            }

            // begin
            if (strBeginScript != "")
            {
                strResult += strTab + "// begin\r\n";
                strResult += strTab + "public override void OnBegin() {\r\n";
                strResult += strBeginScript;
                strResult += "\r\n";
                strResult += strTab + "}";
                strResult += strTab + "\r\n";
            }

            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                XmlNode node = xmlNode.ChildNodes[i];

                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (IsStructureElementName(node.Name) == false)
                    continue;

                string strThis;
                nRet = BuildOneLevelScript(node,
                    out strThis,
                    out strError);
                if (nRet == -1)
                    return -1;

                strResult += strTab + "// \r\n";
                strResult += strThis;
                strResult += strTab + "\r\n";
            }

            // end
            if (strEndScript != "")
            {
                strResult += strTab + "// end\r\n";
                strResult += strTab + "public override void OnEnd() {\r\n";
                strResult += strEndScript;
                strResult += "\r\n";
                strResult += strTab + "}";
                strResult += strTab + "\r\n";

            }

            strResult += "\r\n" + strTab + "} // end of class " + strClassName + "\r\n";
            return 0;
        }

        static string GetClassName(XmlNode node)
        {
            XmlNode parent = node.ParentNode;

            if (parent == node.OwnerDocument)
                return "__" + node.Name;

            if (parent == null)
                return "__" + node.Name;

            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                if (parent.ChildNodes[i] == node)
                {
                    if (node.Name == parent.Name)
                        return "__" + node.Name + node.Name + Convert.ToString(i);

                    return "__" + node.Name + Convert.ToString(i);
                }
            }

            return "__" + node.Name;
        }

        static string GetTabString(XmlNode node)
        {
            string strResult = "";
            for (int i = 0; node != null; i++)
            {
                strResult += "    ";
                node = node.ParentNode;
            }

            return strResult;
        }

        static bool IsStructureElementName(string strName)
        {
            if (strName == "filter"
                || strName == "record"
                || strName == "field"
                || strName == "subfield"
                || strName == "group"
                || strName == "char")
                return true;
            return false;
        }

        static string GetFirstChildInnerText(XmlNode node)
        {
            if (node.ChildNodes.Count == 0)
                return "";
            return node.ChildNodes[0].InnerText;
        }

        int GetDefBeginEndScript(XmlNode parent,
            out string strDefinitionScript,
            out string strBeginScript,
            out string strEndScript,
            out string strError)
        {
            strError = "";
            strBeginScript = "";
            strEndScript = "";
            strDefinitionScript = "";

            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];

                if (node.Name == "def"
                    || node.Name == "begin"
                    || node.Name == "end"
                    )
                {
                    /*
					if (node.ChildNodes.Count != 1) 
					{
						strError = "<" + node.Name + ">元素下应该有而且只有一个CDATA节点...";
						return -1;
					}

					if (node.ChildNodes[0].NodeType!= XmlNodeType.CDATA)
					{
						strError = "<" + node.Name + ">元素下应该有而且只有一个CDATA节点...";
						return -1;
					}
                     */

                }
                else
                {
                    if (node.NodeType == XmlNodeType.Text ||
                        node.NodeType == XmlNodeType.CDATA)
                        strBeginScript += node.InnerText;
                    continue;
                }

                if (node.Name == "def")
                {

                    // strDefinitionScript += node.ChildNodes[0].InnerText;
                    strDefinitionScript += GetFirstChildInnerText(node);
                }
                if (node.Name == "begin")
                {
                    // strBeginScript += node.ChildNodes[0].InnerText;
                    strBeginScript += GetFirstChildInnerText(node);
                }
                if (node.Name == "end")
                {
                    // strEndScript += node.ChildNodes[0].InnerText;
                    strEndScript += GetFirstChildInnerText(node);
                }
            }
            return 0;
        }

        int GetUsingScript(XmlNode parent,
            out string strUsingScript,
            out string strError)
        {
            strError = "";
            strUsingScript = "";

            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];

                if (node.Name == "using")
                {
                    if (node.ChildNodes.Count != 1)
                    {
                        strError = "<using>元素下应该有而且只有一个CDATA节点...";
                        return -1;
                    }

                    if (node.ChildNodes[0].NodeType != XmlNodeType.CDATA)
                    {
                        strError = "<using>元素下应该有而且只有一个CDATA节点...";
                        return -1;
                    }

                    strUsingScript += node.ChildNodes[0].Value + "\r\n";
                }
            }
            return 0;
        }

        // 得到一层对象的type信息
        int FillOneLevelType(XmlNode xmlNode,
            out string strError)
        {
            strError = "";

            Debug.Assert(assembly != null, "调用FillOneLevelType()以前，必须先给assembly赋值");
            if (assembly == null)
            {
                strError = "调用FillOneLevelType()以前，必须先给assembly赋值";
                return -1;
            }

            // 
            HashFilterItem item = (HashFilterItem)this.NodeTable[xmlNode];
            if (item == null)
            {
                Debug.Assert(false, "xml节点" + xmlNode.OuterXml + "没有在NodeTable中创建对应的事项");
                strError = "xml节点" + xmlNode.OuterXml + "没有在NodeTable中创建对应的事项";
                return -1;
            }

            if (item.FunctionName == "")
            {
                Debug.Assert(false, "xml节点" + xmlNode.OuterXml + "所对应的FilterItem中FunctionName为空字符串，不正常");
                strError = "xml节点" + xmlNode.OuterXml + "所对应的FilterItem中FunctionName为空字符串，不正常";
                return -1;
            }

            XmlNode parentNode = xmlNode.ParentNode;
            HashFilterItem parentItem = (HashFilterItem)this.NodeTable[parentNode];
            Type parentType = null;
            if (parentItem != null)
            {
                parentType = parentItem.FunctionType;
            }

            if (parentType != null)
            {
                item.FunctionType = parentType.GetNestedType(
                    item.FunctionName);
            }
            else
            {
                // 得到Assembly中Batch派生类Type
                item.FunctionType = assembly.GetType(
                    item.FunctionName,
                    false,	//   bool throwOnError,
                    false	//bool ignoreCase
                    );
            }
            if (item.FunctionType == null)
            {
                Debug.Assert(false, "xml节点" + xmlNode.OuterXml + " 应在fltx.cs中存在对应类" + item.FunctionName);
                strError = "xml节点" + xmlNode.OuterXml + " 应在fltx.cs中存在对应类" + item.FunctionName;
                return -1;
            }

            if (item.FunctionType.IsClass == false)
            {
                strError = "脚本中，[" + item.FunctionName + "]为系统保留字，用户代码不能使用。";
                return -1;
            }

            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                XmlNode node = xmlNode.ChildNodes[i];

                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (IsStructureElementName(node.Name) == false)
                    continue;

                int nRet = FillOneLevelType(node,
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            return 0;
        }

        // 匹配字段名/子字段名
        // pamameters:
        //		strName	名字
        //		strMatchCase	要匹配的要求
        // return:
        //		-1	error
        //		0	not match
        //		1	match
        public static int MatchName(string strName,
            string strMatchCase)
        {
            if (strMatchCase == "")	// 如果strMatchCase为空，表示无论什么名字都匹配
                return 1;

            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                if (StringUtil.RegexCompare(strMatchCase.Substring(1),
                    RegexOptions.None,
                    strName) == true)
                    return 1;
                return 0;
            }
            else // 原来的*模式
            {
                if (CmpName(strName, strMatchCase) == 0)
                    return 1;
                return 0;
            }
        }

        // 2013/1/7
        // t的长度可以是s的整倍数
        public static int CmpName(string s, string t)
        {
            if (s.Length == t.Length)
                return CmpOneName(s, t);

            if ((t.Length % s.Length) != 0)
            {
                throw new Exception("t '" + t + "'的长度 " + t.Length.ToString() + " 应当为s '" + s + "' 的长度 " + s.Length.ToString() + "  的整倍数");
            }
            int nCount = t.Length / s.Length;
            for (int i = 0; i < nCount; i++)
            {
                int nRet = CmpOneName(s, t.Substring(i * s.Length, s.Length));
                if (nRet == 0)
                    return 0;
            }

            return 1;
        }

        // 含通配符的比较
        public static int CmpOneName(string s,
            string t)
        {
            int len = Math.Min(s.Length, t.Length);
            for (int i = 0; i < len; i++)
            {
                if (s[i] == '*' || t[i] == '*')
                    continue;
                if (s[i] != t[i])
                    return (s[i] - t[i]);
            }
            if (s.Length > t.Length)
                return 1;
            if (s.Length < t.Length)
                return -1;
            return 0;
        }

        // 从hash表中找到xml节点对应的代码Type
        FilterItem NewFilterItem(
            object objParam,
            XmlNode node,
            out string strError)
        {
            strError = "";

            Debug.Assert(node != null, "node参数不能为null");

            HashFilterItem itemNode = (HashFilterItem)NodeTable[node];

            if (itemNode == null)
            {
                Debug.Assert(false, "NodeTable中缺乏事项");
                return null;
            }

            Debug.Assert(node == itemNode.xmlNode, "item成员xmlNode不正确");

            Type entryClassType = itemNode.FunctionType;

            if (entryClassType == null)
            {
                Debug.Assert(false, itemNode.FunctionName + "没有预先填充Type");
                return null;
            }
            FilterItem itemHost = (FilterItem)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            itemHost.Param = objParam;
            itemHost.Document = this;
            try
            {
                itemHost.PreInitial();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return itemHost;
        }

        // 处理一条记录对应于一个<record>定义
        // container参数完全可能为null，这表示<record>为根元素
        // return:
        //		-1	出错
        //		0	正常返回
        int DoSingleItem(
            object objParam,
            XmlNode node,
            int nIndex,
            FilterItem container,
            string strData,
            string strNextName,
            out BreakType breakType,
            out string strError)
        {
            strError = "";
            breakType = BreakType.None;

            /*
            Debug.Assert(node != null, "node参数不能为null");

            HashFilterItem itemNode = (HashFilterItem)NodeTable[node];

            if (itemNode == null) 
            {
                Debug.Assert(false, "NodeTable中缺乏事项");
                return -1;
            }

            Debug.Assert(node == itemNode.xmlNode, "item成员xmlNode不正确");

            Type entryClassType = itemNode.FunctionType;

            if (entryClassType == null) 
            {
                Debug.Assert(false, itemNode.FunctionName + "没有预先填充Type");
                return -1;
            }

            // 把fltx.cs代码中的Batch层对象new，并保持
            // new一个Batch派生对象
            FilterItem itemHost = (FilterItem)entryClassType.InvokeMember(null, 
                BindingFlags.DeclaredOnly | 
                BindingFlags.Public | BindingFlags.NonPublic | 
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            */
            // 创建一个新FilterItem对象
            FilterItem itemHost = NewFilterItem(
                objParam,
                node,
                out strError);
            if (itemHost == null)
                return -1;

            itemHost.Data = strData;
            itemHost.Index = nIndex;
            itemHost.Container = container;
            // itemHost.FilterRoot = container != null ? container : itemHost;
            itemHost.FilterRoot = GetRootFilterItem(itemHost);

            Debug.Assert(itemHost.FilterRoot != null, "itemHost.FilterRoot不应当==null");

            if (node.Name == "record")
            {
                itemHost.NodeType = NodeType.Record;
                itemHost.Data = strData;
                itemHost.Name = "";
                itemHost.Content = strData;
            }
            else if (node.Name == "field")
            {
                itemHost.NodeType = NodeType.Field;
                itemHost.Data = strData;
                if (strData.Length < 3)
                {
                    strError = "字段全部数据长度不足3字符";
                    goto ERROR1;
                }
                itemHost.Name = strData.Substring(0, 3);	// 这里要求调用本函数的，准备头标区这个特殊“字段”时，要加上'hdr'2字符在内容前面
                // control field  001-009没有子字段
                if (FilterItem.IsControlFieldName(itemHost.Name) == true)
                {
                    itemHost.Indicator = "";
                    itemHost.Content = strData.Substring(3);
                }
                else
                {
                    if (strData.Length >= 5)
                    {
                        itemHost.Indicator = strData.Substring(3, 2);
                        itemHost.Content = strData.Substring(5);
                    }
                    else
                    {
                        // 2006/11/24
                        itemHost.Indicator = "";
                        itemHost.Content = "";
                    }

                }
            }
            else if (node.Name == "group")
            {
                itemHost.NodeType = NodeType.Group;
                itemHost.Data = strData;
                itemHost.Name = "";
                itemHost.Content = strData;
            }
            else if (node.Name == "subfield")
            {
                itemHost.NodeType = NodeType.Subfield;
                itemHost.Data = strData;
                if (strData.Length < 1)
                {
                    strError = "子字段全部数据长度不足1字符";
                    goto ERROR1;
                }
                itemHost.Name = strData.Substring(0, 1);
                itemHost.Content = strData.Substring(1);
            }

            itemHost.SetDupCount();
            itemHost.NextName = strNextName;
            if (itemHost.Container != null)
            {
                itemHost.PrevName = itemHost.Container.LastChildName;	// 利用上次遗留的

                // 这一句有点多余。因为本函数返回后, 后面立即会做修改LastChildName的事情
                itemHost.Container.LastChildName = itemHost.Name;	// 保存这次的
            }

            itemHost.OnBegin();

            // 检查无意义的break设置情况
            if (CheckBreakException == true
                && node.Name == "subfield"
                && (itemHost.Break == BreakType.SkipCaseEnd
                || itemHost.Break == BreakType.SkipCase))
            {
                throw (new Exception("<subfield>元素内script代码中用Break = ???改变结构匹配流程无任何意义..."));
            }

            if (itemHost.Break == BreakType.SkipCaseEnd)
                goto SKIP1;	// 不做OnEnd()
            if (itemHost.Break == BreakType.SkipCase)
                goto SKIP;	// 不做OnBegin的兄弟case，但是要做OnEnd()

            // int i;
            int nRet;
            // XmlNode child = null;
            BreakType thisBreak = BreakType.None;

            // <record>希望下级是<field>
            if (node.Name == "record")
            {
                // 切割记录为若干字段，匹配case
                for (int r = 0; ; r++)
                {
                    string strField;
                    string strNextFieldName;

                    // 从记录中得到一个字段
                    // parameters:
                    //		strMARC		MARC记录
                    //		strFieldName	字段名。如果==null，表示任意字段
                    //		nIndex		同名字段中的第几个。从0开始计算(0表示头标区)
                    //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
                    //					注意头标区当作一个字段返回，strField中不包含字段名，一上来就是头标区内容
                    // return:
                    //		-1	error
                    //		0	not found
                    //		1	found
                    nRet = MarcDocument.GetField(itemHost.Data, // strData,
                        null,
                        r,
                        out strField,
                        out strNextFieldName);
                    if (nRet == -1)
                    {
                        // 2009/11/1
                        if (String.IsNullOrEmpty(
                            itemHost.Data  // strData
                            ) == true)
                            break;

                        strError = "DoSingleItem() GetField() error";
                        return -1;
                    }
                    if (nRet == 0)
                        break;

                    if (strField.Length < 3)
                        goto SKIP;

                    string strFieldName = "";
                    if (r != 0)
                        strFieldName = strField.Substring(0, 3);
                    else
                    {
                        strFieldName = "hdr";
                        strField = strFieldName + strField;
                    }
                    // ***
                    itemHost.IncChildDupCount(strFieldName);

                    // for(i=0;i<node.ChildNodes.Count;i++) 
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        // child = node.ChildNodes[i];

                        if (child.NodeType != XmlNodeType.Element)
                            continue;

                        if (child.Name != "field")
                            continue;

                        // 匹配字段名
                        nRet = MatchName(strFieldName, DomUtil.GetAttr(child, "name"));
                        if (nRet == 1)
                        {
                            nRet = DoSingleItem(
                                objParam,
                                child,
                                r,
                                itemHost,
                                strField,
                                strNextFieldName,
                                out thisBreak,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            if (itemHost.Break != BreakType.None)
                                break;
                        }
                    } // end of for

                    itemHost.LastChildName = strFieldName;	// 保存这次的
                    if (itemHost.Break != BreakType.None)
                        goto SKIP;
                }
            }
            else if (node.Name == "field")
            {
                // 若下级为subfield
                string strFirstChildName = GetFirstChildElementType(node);
                // field下的subfield
                if (strFirstChildName == "subfield")
                {
                    // 切割记录为若干子字段，匹配case
                    for (int s = 0; ; s++)
                    {
                        string strSubfield;
                        string strNextSubfieldName;

                        // 从字段或组中得到一个子字段
                        // parameters:
                        //		strText		字段内容，或者子字段组内容。
                        //		textType	表示strText中包含的是字段内容还是组内容。
                        //		strSubfieldName	子字段名。如果==null，表示任意子字段
                        //					形式为'a'这样的。
                        //		nIndex			同名子字段中的第几个。从0开始计算。
                        //		strSubfield		输出子字段。子字段名(1字符)、子字段内容。
                        //		strNextSubfieldName	下一个子字段的名字，一个字符
                        // return:
                        //		-1	error
                        //		0	not found
                        //		1	found
                        nRet = MarcDocument.GetSubfield(itemHost.Data,  // strData,
                            ItemType.Field,
                            null,
                            s,
                            out strSubfield,
                            out strNextSubfieldName);
                        if (nRet == -1)
                        {
                            strError = "GetSubfield() error";
                            return -1;
                        }
                        if (nRet == 0)
                            break;

                        if (strSubfield.Length < 1)
                            goto SKIP;

                        string strSubfieldName = strSubfield.Substring(0, 1);

                        // ***
                        itemHost.IncChildDupCount(strSubfieldName);

                        // for(i=0;i<node.ChildNodes.Count;i++) 
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            // child = node.ChildNodes[i];

                            if (child.NodeType != XmlNodeType.Element)
                                continue;

                            if (child.Name != "subfield")
                                continue;

                            // 匹配子字段名
                            nRet = MatchName(strSubfieldName, DomUtil.GetAttr(child, "name"));
                            if (nRet == 1)
                            {
                                nRet = DoSingleItem(
                                    objParam,
                                    child,
                                    s,
                                    itemHost,
                                    strSubfield,
                                    strNextSubfieldName,
                                    out thisBreak,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (itemHost.Break != BreakType.None)
                                    break;
                            }
                        } // end of for

                        itemHost.LastChildName = strSubfieldName;	// 保存这次的
                        if (itemHost.Break != BreakType.None)
                            goto SKIP;
                    }
                }
                // field下嵌套的field
                if (strFirstChildName == "field")
                {
                    // 切割字符串为若干字段，匹配case
                    for (int r = 0; ; r++)
                    {
                        string strField;
                        string strNextFieldName;

                        // 从记录中得到一个字段
                        // parameters:
                        //		strMARC		MARC记录
                        //		strFieldName	字段名。如果==null，表示任意字段
                        //		nIndex		同名字段中的第几个。从0开始计算(0表示头标区)
                        //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
                        //					注意头标区当作一个字段返回，strField中不包含字段名，一上来就是头标区内容
                        // return:
                        //		-1	error
                        //		0	not found
                        //		1	found
                        nRet = MarcDocument.GetNestedField(itemHost.Data,   // strData,
                            null,
                            r,
                            out strField,
                            out strNextFieldName);
                        if (nRet == -1)
                        {
                            strError = "GetNestedField() error";
                            return -1;
                        }
                        if (nRet == 0)
                            break;

                        if (strField.Length < 3)
                            goto SKIP;

                        string strFieldName = "";
                        strFieldName = strField.Substring(0, 3);

                        // ***
                        itemHost.IncChildDupCount(strFieldName);

                        // 嵌套字段不存在头标区'hdr'字段问题?

                        //for(i=0;i<node.ChildNodes.Count;i++) 
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            //child = node.ChildNodes[i];

                            if (child.NodeType != XmlNodeType.Element)
                                continue;

                            if (child.Name != "field")
                                continue;

                            // 匹配字段名
                            nRet = MatchName(strFieldName, DomUtil.GetAttr(child, "name"));
                            if (nRet == 1)
                            {
                                nRet = DoSingleItem(
                                    objParam,
                                    child,
                                    r,
                                    itemHost,
                                    strField,
                                    strNextFieldName,
                                    out thisBreak,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (itemHost.Break != BreakType.None)
                                    break;
                            }
                        } // end of for

                        itemHost.LastChildName = strFieldName;	// 保存这次的
                        if (itemHost.Break != BreakType.None)
                            goto SKIP;
                    }
                }

                // field 下的group
                else if (strFirstChildName == "group")
                {
                    // 切割记录为若干子字段，匹配case
                    for (int g = 0; ; g++)
                    {
                        string strGroup;

                        // 从字段中得到子字段组
                        // parameters:
                        //		strGroup	[out]结果。
                        // return:
                        //		-1	error
                        //		0	not found
                        //		1	found
                        nRet = MarcDocument.GetGroup(itemHost.Data, // strData,
                            g,
                            out strGroup);
                        if (nRet == -1)
                        {
                            strError = "GetGroup() error";
                            return -1;
                        }
                        if (nRet == 0)
                            break;

                        string strGroupName = Convert.ToString(g);

                        // ***
                        itemHost.IncChildDupCount(strGroupName);

                        // for(i=0;i<node.ChildNodes.Count;i++) 
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            // child = node.ChildNodes[i];

                            if (child.NodeType != XmlNodeType.Element)
                                continue;

                            if (child.Name != "group")
                                continue;

                            // 匹配组名
                            nRet = MatchName(strGroupName, DomUtil.GetAttr(child, "name"));
                            if (true/*nRet == 1*/)
                            {
                                nRet = DoSingleItem(
                                    objParam,
                                    child,
                                    g,
                                    itemHost,
                                    strGroup,
                                    "",
                                    out thisBreak,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (itemHost.Break != BreakType.None)
                                    break;
                            }
                        } // end of for

                        itemHost.LastChildName = "";	// 保存这次的
                        if (itemHost.Break != BreakType.None)
                            goto SKIP;
                    }
                }
            }
            else if (node.Name == "group")
            {
                // 若下级为subfield
                string strFirstChildName = GetFirstChildElementType(node);
                if (strFirstChildName != "subfield")
                {
                    strError = ".fltx中<group>下级必须为<subfield>元素";
                    return -1;
                }

                // 切割记录为若干子字段，匹配case
                for (int s = 0; ; s++)
                {
                    string strSubfield;
                    string strNextSubfieldName;

                    // 从字段或组中得到一个子字段
                    // parameters:
                    //		strText		字段内容，或者子字段组内容。
                    //		textType	表示strText中包含的是字段内容还是组内容。
                    //		strSubfieldName	子字段名。如果==null，表示任意子字段
                    //					形式为'a'这样的。
                    //		nIndex			同名子字段中的第几个。从0开始计算。
                    //		strSubfield		输出子字段。子字段名(1字符)、子字段内容。
                    //		strNextSubfieldName	下一个子字段的名字，一个字符
                    // return:
                    //		-1	error
                    //		0	not found
                    //		1	found
                    nRet = MarcDocument.GetSubfield(itemHost.Data,  // strData,
                        ItemType.Group,
                        null,
                        s,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (nRet == -1)
                    {
                        strError = "GetSubfield() error";
                        return -1;
                    }
                    if (nRet == 0)
                        break;

                    if (strSubfield.Length < 1)
                        goto SKIP;

                    string strSubfieldName = strSubfield.Substring(0, 1);
                    // ***
                    itemHost.IncChildDupCount(strSubfieldName);

                    // for(i=0;i<node.ChildNodes.Count;i++) 
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        // child = node.ChildNodes[i];

                        if (child.NodeType != XmlNodeType.Element)
                            continue;

                        if (child.Name != "subfield")
                            continue;

                        // 匹配子字段名
                        nRet = MatchName(strSubfieldName, DomUtil.GetAttr(child, "name"));
                        if (nRet == 1)
                        {
                            nRet = DoSingleItem(
                                objParam,
                                child,
                                s,
                                itemHost,
                                strSubfield,
                                strNextSubfieldName,
                                out thisBreak,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (itemHost.Break != BreakType.None)
                                break;

                        }

                    } // end of for

                    itemHost.LastChildName = strSubfieldName;	// 保存这次的
                    if (itemHost.Break != BreakType.None)
                        goto SKIP;
                }
            }
            else if (node.Name == "subfield")
            {
                // 暂时没有什么处理

            }

        SKIP:

            if (itemHost.Break != BreakType.SkipCaseEnd)
            {
                itemHost.OnEnd();
            }
        SKIP1:
            /*
            if (itemHost.Break != BreakType.None)
                return 1;
            */

            breakType = itemHost.Break;
            return 0;

        ERROR1:
            return -1;
        }

        // 得到儿子中第一个非def/begin/end的儿子的元素名
        string GetFirstChildElementType(XmlNode parent)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                if (IsStructureElementName(node.Name) == false)
                    continue;

                return node.Name;
            }
            return "";
        }

        // 获得.fltx中<ref>所定义的参考库
        public string[] GetRefs()
        {
            XmlNodeList nodes = this.dom.SelectNodes("//ref");
            List<string> refs = new List<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = DomUtil.GetNodeText(nodes[i]);
                if (strText == "")
                    continue;
                refs.Add(strText);
            }
            string[] results = new string[refs.Count];

            for (int i = 0; i < refs.Count; i++)
            {
                results[i] = refs[i];
            }

            return results;
        }

        // 创建Assembly
        // parameters:
        //	strCode:	脚本代码
        //	refs:	连接的外部assembly
        // strResult:处理信息
        // objDb:数据库对象，在出错调getErrorInfo用到
        // 返回值:创建好的Assembly
        public Assembly CreateAssembly(string strCode,
            string[] refs,
            out string strErrorInfo)
        {
            // System.Reflection.Assembly compiledAssembly = null;
            strErrorInfo = "";

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
            compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true; //Assembly is created in memory
            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.WarningLevel = 4;

            compilerParams.ReferencedAssemblies.AddRange(refs);

            CSharpCodeProvider provider;

            // System.CodeDom.Compiler.ICodeCompiler compiler;
            System.CodeDom.Compiler.CompilerResults results = null;
            try
            {
                /*
				provider = new CSharpCodeProvider();
				compiler = provider.CreateCompiler();
				results = compiler.CompileAssemblyFromSource(
					compilerParams, 
					strCode);
                 */
                provider = new CSharpCodeProvider();
                results = provider.CompileAssemblyFromSource(
                    compilerParams,
                    strCode);

            }
            catch (Exception ex)
            {
                strErrorInfo = "出错 " + ex.Message;
                return null;
            }

            if (results.Errors.Count == 0)
            {
            }
            else
            {
                strErrorInfo = "编译出错，错误数:" + Convert.ToString(results.Errors.Count) + "\r\n";
                strErrorInfo += getErrorInfo(results.Errors);

                return null;
            }

            return results.CompiledAssembly;
        }

        // 构造出错信息字符串
        public string getErrorInfo(CompilerErrorCollection errors)
        {
            string strResult = "";

            if (errors == null)
            {
                strResult = "error参数为null";
                return strResult;
            }

            foreach (CompilerError oneError in errors)
            {
                strResult += "(" + Convert.ToString(oneError.Line) + "," + Convert.ToString(oneError.Column) + ")\r\n";
                strResult += (oneError.IsWarning) ? "warning " : "error ";
                strResult += oneError.ErrorNumber + " ";
                strResult += ":" + oneError.ErrorText + "\r\n";
            }
            return strResult;
        }

        #endregion
    }

    public enum ItemType
    {
        Filter = 0,
        Record = 1,
        Field = 2,
        Subfield = 3,
        Group = 4,
        Char = 5,
        /*
        Begin = 6,
        End = 7,
        Def = 8,
        */
    }

    // hash table中和XmlNode对应的Item
    public class HashFilterItem
    {
        public XmlNode xmlNode = null;
        public ItemType ItemType;
        public string Name = "";
        public string FunctionName = "";
        public Type FunctionType = null;
    }

    public enum BreakType
    {
        None = 0,	// 不break
        SkipCase = 1,	// 跳过case部分，但是不跳过end部分
        SkipCaseEnd = 2,	// 跳过case和end部分
        SkipDataLoop = 3,	// 跳过后面的数据处理循环 ？
    }

    public enum NodeType
    {
        None = 0,
        Record = 1,
        Field = 2,
        Group = 3,
        Subfield = 4,
    }

    // 
    /// <summary>
    /// Script代码的基类
    /// </summary>
    public class FilterItem
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeType NodeType = NodeType.None;

        /// <summary>
        /// 节点名 (字段/子字段...)名。一般而言 Name + Indicator + Content = Data
        /// </summary>
        public string Name = "";	// 数据(字段/子字段...)名

        /// <summary>
        /// 指示符。一般而言 Name + Indicator + Content = Data
        /// </summary>
        public string Indicator = "";

        /// <summary>
        /// 正文。一般而言 Name + Indicator + Content = Data
        /// </summary>
        public string Content = "";	// 一般而言 Name + Indicator + Content = Data

        /// <summary>
        /// 上级对象
        /// </summary>
        public FilterItem Container = null;

        /// <summary>
        /// 根对象
        /// </summary>
        public FilterItem FilterRoot = null;

        //
        /// <summary>
        /// 对象的整个内容。一般而言 Name + Indicator + Content = Data
        /// </summary>
        public string Data = "";

        /// <summary>
        /// 中断标志。决定是否跳过同级后面的case/end
        /// </summary>
        public BreakType Break = BreakType.None;	// 是否跳过同级后面的case/end

        /// <summary>
        /// 下标
        /// </summary>
        public int Index = -1;	// 缺省值-1是为了暴露错误

        /// <summary>
        /// Document 对象
        /// </summary>
        public FilterDocument Document = null;	// Document对象。可以在这里保存需要在record之间持久的值

        /// <summary>
        /// 抽象的传递数据的指针
        /// </summary>
        public object Param;	// 抽象的传递数据的指针

        // 同级结构间相互关系配套设施
        /// <summary>
        /// 同级前一对象名
        /// </summary>
        public string PrevName = "";	// 前一名

        /// <summary>
        /// 同级后一对象名
        /// </summary>
        public string NextName = "";	// 后一名

        /// <summary>
        /// 同级中和本对象同名的个数
        /// </summary>
        public int DupCount = 0;	// 同名重复次数

        private Hashtable ChildDupTable = new Hashtable();	// 儿子对象重复情况记忆
        internal string LastChildName = "";	// 最近用过的最后一个下级结构的名字

        /// <summary>
        /// 构造函数
        /// </summary>
        public FilterItem()
        {
        }

        // 设置当前对象DupCount变量
        // Container、Name必须初始化
        internal void IncChildDupCount(string strChildName)
        {
            if (this.ChildDupTable.Contains(strChildName) == false)
            {
                this.ChildDupTable.Add(strChildName, (object)1);
            }
            else
            {
                int nOldDupCount = (int)this.ChildDupTable[strChildName] + 1;

                this.ChildDupTable[strChildName] = (object)nOldDupCount;
            }
        }

        internal void SetDupCount()
        {
            if (Container == null)
                return;

            // 从父亲中找到ChildDupTable
            if (Container.ChildDupTable.Contains(this.Name) == false)
            {
                Debug.Assert(false, "");
                this.DupCount = 1;
            }
            else
            {
                this.DupCount = (int)Container.ChildDupTable[this.Name];
            }
        }

#if NO
		// 设置当前对象DupCount变量
		// Container、Name必须初始化
		internal void SetDupCount()
		{
			if (Container == null)
				return;

			// 从父亲中找到ChildDupTable
			if (Container.ChildDupTable.Contains(this.Name) == false)
			{
				Container.ChildDupTable.Add(this.Name, (object)1);
				this.DupCount = 1;
			}
			else 
			{
				this.DupCount = (int)Container.ChildDupTable[this.Name] + 1;

				Container.ChildDupTable[this.Name] = (object)this.DupCount;
			}
		}
#endif

        /// <summary>
        /// Begin 阶段
        /// </summary>
        public virtual void OnBegin()
        {
        }

        /// <summary>
        /// End 阶段
        /// </summary>
        public virtual void OnEnd()
        {
        }

        // 
        /// <summary>
        /// 看一个字段名是否是控制字段。所谓控制字段没有指示符概念
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <returns>是否为控制字段</returns>
        public static bool IsControlFieldName(string strFieldName)
        {
            if (String.Compare(strFieldName, "hdr", true) == 0)
                return true;

            if (
                (
                String.Compare(strFieldName, "001") >= 0
                && String.Compare(strFieldName, "009") <= 0
                )

                || String.Compare(strFieldName, "-01") == 0
                )
                return true;

            return false;
        }

        /// <summary>
        /// 本对象是否为控制字段。(如果本对象不是字段对象，返回 false)
        /// </summary>
        public bool IsControlField
        {
            get
            {
                if (this.Name.Length != 3)
                    return false;
                return FilterItem.IsControlFieldName(this.Name);
            }
        }

        /// <summary>
        /// Initial 阶段前的预处理
        /// </summary>
        virtual public void PreInitial()
        {

        }

        /// <summary>
        /// 根据 ID 获得当前语言下的字符串
        /// 如果没有精确匹配的语言，就模糊匹配，或返回第一个语言的
        /// 但如果id不存在，返回null
        /// </summary>
        /// <param name="strID">ID</param>
        /// <returns>字符串</returns>
        public string GetString(string strID)
        {
            return this.Document.GetString(strID);
        }

        /// <summary>
        /// 语言代码
        /// </summary>
        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        /// <summary>
        /// 根据 ID 获得当前语言下的字符串。最坏情况下会返回 ID 自身
        /// </summary>
        /// <param name="strID">ID</param>
        /// <returns>字符串</returns>
        public string GetStringSafe(string strID)
        {
            return this.Document.GetStringSafe(strID);
        }

        // 简化名称版本
        /// <summary>
        /// 同 GetString()
        /// </summary>
        /// <param name="strID">ID</param>
        /// <returns>字符串</returns>
        public string S(string strID)
        {
            return this.Document.GetString(strID);
        }

        // 简化名称版本
        /// <summary>
        /// 同 GetStringSafe()
        /// </summary>
        /// <param name="strID">ID</param>
        /// <returns>字符串</returns>
        public string SS(string strID)
        {
            return this.Document.GetStringSafe(strID);
        }

        /// <summary>
        /// 获得可用于定位本对象的位置字符串
        /// </summary>
        public string LocationString
        {
            get
            {
                return this.GetLocationString();
            }
        }

        // 
        /// <summary>
        /// 获得特定名字的第一个子字段正文。如果当前对象不是字段对象，则返回 null
        /// </summary>
        /// <param name="strSubfieldName">子字段名</param>
        /// <returns>子字段正文字符串</returns>
        public string GetFirstSubfieldValue(string strSubfieldName)
        {
            if (this.NodeType != MarcDom.NodeType.Field && this.NodeType != MarcDom.NodeType.Group)
                return null;

            string strSubfield = "";
            string strNextSubfieldName = "";
            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = MarcDocument.GetSubfield(this.Content,
                this.NodeType == MarcDom.NodeType.Field ? ItemType.Field : ItemType.Group,
                strSubfieldName,
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length < 1)
                return "";
            return strSubfield.Substring(1);
        }

        /// <summary>
        /// 获得可用于定位本对象的位置字符串
        /// </summary>
        /// <param name="nCharPos">在本对象内的偏移量</param>
        /// <returns>位置字符串</returns>
        public string GetLocationString(int nCharPos = 0)
        {
            string strResult = "";
            // field
            if (this.Name.Length == 3)
            {
                strResult = this.Name;
                if (this.DupCount != 1)
                    strResult += "#" + this.DupCount.ToString();
                if (nCharPos != 0)
                    strResult += ",," + nCharPos.ToString();
                return strResult;
            }
            // subfield
            if (this.Name.Length == 1)
            {
                strResult = this.Container.GetLocationString() + "," + this.Name;
                if (this.DupCount != 1)
                    strResult += "#" + this.DupCount.ToString();
                if (nCharPos != 0)
                    strResult += "," + nCharPos.ToString();
                return strResult;
            }

            return "";
        }
    }
}
