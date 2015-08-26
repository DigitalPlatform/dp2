using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Diagnostics;
using System.IO;
using System.Text;

using System.Reflection;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // KeysCfg 的摘要说明。
    public class KeysCfg : KeysBrowseBase
    {
        public string Prefix = "keys_";
        public StopwordCfg StopwordCfg = null;  // 拥有

        // <key>元素下级的<table>元素 和 TableInfo对象的对照表 
        Hashtable tableTableInfoClient = new Hashtable();

        // 被Client引用过<table>元素的xpath路径 和 TableInfo对象的对照表
        // <table>可能是内部直接定义的，也可能是外部定义的，但都被client引用过的
        // 本来创建完时，就可以扔掉的，但当检索时，通过一个来源来找TableInfo时，应从该table中找，不会有重复的。
        Hashtable tableTableInfoServer = new Hashtable();

        public List<TableInfo> m_aTableInfoForForm = null;

        public Assembly m_assembly = null;
        public string m_strAssemblyError = "";

        Hashtable m_exprCache = new Hashtable();

        // 初始化KeysCfg对象，把dom准备好，把两个Hashtable准备好
        public int Initial(string strKeysCfgFileName,
            string strBinDir,
            string strKeysTableNamePrefix,
            out string strError)
        {
            int nRet = base.Initial(strKeysCfgFileName,
                strBinDir,
                out strError);
            if (nRet == -1)
                return -1;

            this.Prefix = strKeysTableNamePrefix;

            nRet = this.CreateTableInfoTableCache(
                out strError);
            if (nRet == -1)
                return -1;

            if (this.dom != null)
            {
                // 初始化stopword
                XmlNode nodeStopword = dom.DocumentElement.SelectSingleNode("//stopword");
                if (nodeStopword != null)
                {
                    this.StopwordCfg = new StopwordCfg();
                    nRet = this.StopwordCfg.Initial(nodeStopword,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }


            // 创建assembly
            nRet = this.InitialAssembly(out strError);
            if (nRet == -1)
            {
                //strError = "编译keys配置文件中的脚本出错：" + strError;
                //return -1;

                this.m_strAssemblyError = "编译keys配置文件中的脚本出错：" + strError;
                //return 0;
            }

            return 0;
        }

        // 初始化Assembly对象,被Initial调
        // return:
        //		-1	出错
        //		0	成功
        private int InitialAssembly(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.dom == null)
            {
                this.m_assembly = null;
                return 0;
            }

            // 找到<script>节点
            XmlNode nodeScript = this.dom.SelectSingleNode("//script");

            // <script>节点不存在的时
            if (nodeScript == null)
            {
                this.m_assembly = null;
                return 0;
            }

            // <script>节点下级无CDATA节点
            if (nodeScript.ChildNodes.Count == 0)
            {
                this.m_assembly = null;
                return 0;
            }

            XmlNode firstNode = nodeScript.ChildNodes[0];

            //第一个儿子节点不是CDATA节点时
            if (firstNode.NodeType != XmlNodeType.CDATA)
            {
                this.m_assembly = null;
                return 0;
            }

            //~~~~~~~~~~~~~~~~~~
            // 创建Assembly对象

            string[] saRef = null;
            nRet = GetRefs(nodeScript,
                 out saRef,
                 out strError);
            if (nRet == -1)
                return -1;

            string[] saAddRef = {this.BinDir + "\\" + "digitalplatform.rms.dll",
                                this.BinDir + "\\" + "digitalplatform.text.dll"};

            string[] saTemp = new string[saRef.Length + saAddRef.Length];
            Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
            Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
            saRef = saTemp;

            RemoveRefsProjectDirMacro(ref saRef,
                this.BinDir);

            string strCode = firstNode.Value;

            if (strCode != "")
            {
                Assembly assembly = null;
                string strWarning = "";
                nRet = CreateAssembly(strCode,
                    saRef,
                    out assembly,
                    out strError,
                    out strWarning);
                if (nRet == -1)
                    return -1;

                this.m_assembly = assembly;
            }


            return 0;
        }



        // 从node节点得到refs字符串数组
        // return:
        //      -1  出错
        //      0   成功
        public static int GetRefs(XmlNode node,
            out string[] saRef,
            out string strError)
        {
            saRef = null;
            strError = "";

            // 所有ref节点
            XmlNodeList nodes = node.SelectNodes("//ref");
            saRef = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                saRef[i] = nodes[i].InnerText.Trim();
            }
            return 0;
        }

        // 去除路径中的宏%projectdir%
        void RemoveRefsProjectDirMacro(ref string[] refs,
            string strBinDir)
        {
            Hashtable macroTable = new Hashtable();

            macroTable.Add("%bindir%", strBinDir);

            for (int i = 0; i < refs.Length; i++)
            {
                string strNew = PathUtil.UnMacroPath(macroTable,
                refs[i],
                false); // 不要抛出异常，因为可能还有%binddir%宏现在还无法替换
                refs[i] = strNew;
            }

        } 


        // 创建Assembly
        // parameters:
        //		strCode:		脚本代码
        //		refs:			连接的外部assembly
        //		strLibPaths:	类库路径, 可以为""或者null,则此参数无效
        //		strOutputFile:	输出文件名, 可以为""或者null,则此参数无效
        //		strErrorInfo:	出错信息
        //		strWarningInfo:	警告信息
        // result:
        //		-1  出错
        //		0   成功
        public static int CreateAssembly(string strCode,
            string[] refs,
            out Assembly assembly,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";
            assembly = null;

            // CompilerParameters对象
            CompilerParameters compilerParams = new CompilerParameters();

            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;

            compilerParams.TreatWarningsAsErrors = false;
            compilerParams.WarningLevel = 4;

            compilerParams.ReferencedAssemblies.AddRange(refs);


            //CSharpCodeProvider provider = null;
            CodeDomProvider codeDomProvider = new CSharpCodeProvider();

            // ICodeCompiler compiler = null; // 2006/10/26 changed
            CompilerResults results = null;
            try
            {
                //provider = new CSharpCodeProvider();
                // compiler = codeDomProvider.CreateCompiler(); // 2006/10/26 changed

                results = codeDomProvider.CompileAssemblyFromSource(
                    compilerParams,
                    strCode);
            }
            catch (Exception ex)
            {
                strError = "CreateAssemblyFile() 出错 " + ex.Message;
                return -1;
            }

            //return 0;  //测

            int nErrorCount = 0;
            if (results.Errors.Count != 0)
            {
                string strErrorString = "";
                nErrorCount = getErrorInfo(results.Errors,
                    out strErrorString);

                strError = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
                strError += strErrorString;

                if (nErrorCount == 0 && results.Errors.Count != 0)
                {
                    strWarning = strError;
                    strError = "";
                }
            }
            if (nErrorCount != 0)
                return -1;


            assembly = results.CompiledAssembly;// compilerParams.OutputAssembly;

            return 0;
        }

        // 构造出错信息字符串
        // parameter:
        //		errors:    CompilerResults对象
        //		strResult: out参数，返回构造的出错字符串
        // result:
        //		错误信息的条数
        public static int getErrorInfo(CompilerErrorCollection errors,
            out string strResult)
        {
            strResult = "";
            int nCount = 0;
            if (errors == null)
            {
                strResult = "error参数为null";
                return 0;
            }
            foreach (CompilerError oneError in errors)
            {
                strResult += "(" + Convert.ToString(oneError.Line) + "," + Convert.ToString(oneError.Column) + ")\r\n";
                strResult += (oneError.IsWarning) ? "warning " : "error ";
                strResult += oneError.ErrorNumber + " ";
                strResult += ":" + oneError.ErrorText + "\r\n";

                if (oneError.IsWarning == false)
                    nCount++;
            }
            return nCount;
        }

        


        // 创建TableInfo缓存,被Initial调
        // return:
        //		-1	出错
        //		0	成功
        private int CreateTableInfoTableCache(
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.dom == null)
                return 0;

            // 找到<key>下级的所有<table>
            XmlNodeList nodeListTable = this.dom.DocumentElement.SelectNodes("//key/table");
            for (int i = 0; i < nodeListTable.Count; i++)
            {
                // 当前<table>节点
                XmlNode nodeCurrentTable = nodeListTable[i];

                // 目标<table>节点
                XmlNode nodeTargetTable = null;

                // return:
                //		-1	出错
                //		0	没找到	strError里面有出错信息
                //		1	找到
                nRet = FindTableTarget(nodeCurrentTable,
                    out nodeTargetTable,
                    out strError);
                if (nRet != 1)
                    return -1;

                // 取出目标<table>的路径
                string strPath = "";
                nRet = DomUtil.Node2Path(dom.DocumentElement,
                    nodeTargetTable,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    return -1;

                TableInfo tableInfo = (TableInfo)this.tableTableInfoServer[strPath];
                if (tableInfo == null)
                {
                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.GetTableInfo(nodeTargetTable,
                        out tableInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    this.tableTableInfoServer[strPath] = tableInfo;
                }

                // 加到客户端TableInfo表里
                this.tableTableInfoClient[nodeCurrentTable] = tableInfo;
            }

            return 0;
        }

        // 找到最终的目标<table>元素
        // return:
        //		-1	出错
        //		0	未找到
        //		1	找到
        private int FindTableTarget(XmlNode nodeCurrentTable,
            out XmlNode nodeTargetTable,
            out string strError)
        {
            Debug.Assert(nodeCurrentTable != null, "FindTableTarget()调用错误，nodeTableCurrent参数不能为null。");

            nodeTargetTable = null;
            strError = "";

            string strRef = DomUtil.GetAttr(nodeCurrentTable, "ref");
            if (string.IsNullOrEmpty(strRef) == true)
            {
                nodeTargetTable = nodeCurrentTable;
                return 1;
            }

            string strTableName = "";
            string strTableID = "";

            // 解析ref属性值。形态为 "titlePinyin"，或者"titlePinyin, #311"，或"#311"
            int nRet = ParseRefString(strRef,
            out strTableName,
            out strTableID,
            out strError);
            if (nRet == -1)
                return -1;

            string strXPath = "//table[@name='" + strRef + "']";
            if (string.IsNullOrEmpty(strTableName) == false
                && string.IsNullOrEmpty(strTableID) == false)
                strXPath = "//table[@name='" + strTableName + "' and @id='"+strTableID+"']";
            else if (string.IsNullOrEmpty(strTableName) == false)
                strXPath = "//table[@name='" + strTableName + "']";
            else if (string.IsNullOrEmpty(strTableID) == false)
                strXPath = "//table[@id='" + strTableID + "']";
            else
            {
                strError = "strTableName和strTableID都为空";
                return -1;
            }

            nodeTargetTable = nodeCurrentTable.SelectSingleNode(strXPath);
            if (nodeTargetTable != null)
                return 1;

            strError = "未找到名为 '" + strRef + "' 的<table>元素。";
            return 0;
        }

        // 解析ref属性值。形态为 "titlePinyin"，或者"titlePinyin, #311"，或"#311"
        static int ParseRefString(string strRef,
            out string strTableName,
            out string strTableID,
            out string strError)
        {
            strError = "";
            strTableName = "";
            strTableID = "";

            if (string.IsNullOrEmpty(strRef) == true)
            {
                strError = "strRef不能为空";
                return -1;
            }

            string [] parts = strRef.Split(new char []{','});
            foreach(string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;
                if (strText[0] == '#')
                    strTableID = strText.Substring(1);
                else
                    strTableName = strText;
            }

            return 0;
        }

        // parameters:
        // return:
        //		-1	出错
        //		0	成功
        private int GetTableInfo(XmlNode nodeTable,
            out TableInfo tableInfo,
            out string strError)
        {
            strError = "";

            tableInfo = new TableInfo();

            int nRet = tableInfo.Initial(nodeTable,
                this.Prefix,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 根据 table 的 caption 名字，找到对应的 key/from 值
        public static string GetFromValue(XmlElement table)
        {
            XmlElement key = null;
            // 看看 table 元素的上级是不是 key
            if (table.ParentNode.Name == "key")
                key = table.ParentNode as XmlElement;
            else
            {
                string strTableName = table.GetAttribute("name");
                key = table.OwnerDocument.DocumentElement.SelectSingleNode("//key[./table[@ref='" + strTableName + "']]") as XmlElement;
                if (key == null)
                    return "";
            }

            {
                XmlElement from = key.SelectSingleNode("from") as XmlElement;
                if (from != null)
                    return from.InnerText.Trim();
                return "";
            }
        }


        // 创建指定记录的检索点集合
        // parameters:
        //		domData	记录数据dom 不能为null
        //		strRecordID	记录id 不能为null或空
        //		strLang	语言版本
        //		strStyle	风格，暂没有用上
        //		nKeySize	检索点尺寸
        //		keys	out参数，返回生成的检索点集合
        //		strError	out参数，出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int BuildKeys(XmlDocument domData,
            string strRecordID,
            string strLang,
//             string strStyle,
            int nKeySize,
            out KeyCollection keys,
            out string strError)
        {
            strError = "";
            keys = new KeyCollection();

            if (this.dom == null)
                return 0;

            if (domData == null)
            {
                strError = "BuildKeys()调用错误，domData参数不能为null。";
                Debug.Assert(false, strError);
                return -1;
            }

            // Debug.Assert(strRecordID != null && strRecordID != "", "BuildKeys()调用错误，strRecordID参数不能为null或为空。");

            if (String.IsNullOrEmpty(strLang) == true)
            {
                strError = "BuildKeys()调用错误，strLang参数不能为null。";
                Debug.Assert(false, strError);
                return -1;
            }

            /*
            if (String.IsNullOrEmpty(strStyle) == true)
            {
                strError = "BuildKeys()调用错误，strStyle参数不能为null。";
                Debug.Assert(false, strError);
                return -1;
            }
             * */

            if (nKeySize < 0)
            {
                strError = "BuildKeys()调用错误，nKeySize参数不能小于0。";
                Debug.Assert(false, strError);
                return -1;
            }

            int nRet = 0;

            // 找到所有<key>节点
            // TODO: <key> 是否有明确的位置？ 那样就可以避免 // 查找。或者预先缓存起来
            XmlNodeList keyList = dom.SelectNodes("//key");

            XPathNavigator nav = domData.CreateNavigator();

        CREATE_CACHE:
            // 创建Cache
            if (m_exprCache.Count == 0 && keyList.Count > 0)
            {
                for (int i = 0; i < keyList.Count; i++)
                {
                    XmlNode nodeKey = keyList[i];

                    XmlElement nodeXPath = (XmlElement)nodeKey.SelectSingleNode("xpath");
                    if (nodeXPath == null)
                        continue;

                    string strScriptAttr = nodeXPath.GetAttribute("scripting");

                    if (String.Compare(strScriptAttr, "on", true) == 0)
                        continue;

                    string strXPath = nodeXPath.InnerText.Trim();
                    if (string.IsNullOrEmpty(strXPath) == true)
                        continue;

                    // strNstableName 如果为 null 表示属性不存在
                    string strNstableName = DomUtil.GetAttrDiff(nodeXPath, "nstable");

                    XmlNamespaceManager nsmgr = (XmlNamespaceManager)this.tableNsClient[nodeXPath];
#if DEBUG
                    if (nsmgr != null)
                    {
                        Debug.Assert(strNstableName != null, "如果具备名字空间对象，表明<xpath>元素应该有 'nstable' 属性。");
                    }
                    else
                    {
                        Debug.Assert(strNstableName == null, "如果不具备名字空间对象，表明<xpath>元素必须没有定义 'nstable' 属性。");
                    }
#endif

                    XPathExpression expr = nav.Compile(strXPath);
                    if (nsmgr != null)
                        expr.SetContext(nsmgr);

                    m_exprCache[nodeXPath] = expr;
                }
            }

            string strKey = "";
            string strKeyNoProcess = "";
            string strFromName = "";
            string strFromValue = "";
            string strSqlTableName = "";
            string strNum = "";

            for (int i = 0; i < keyList.Count; i++)
            {
                XmlElement nodeKey = (XmlElement)keyList[i];

                strKey = "";
                strKeyNoProcess = "";
                strFromName = "";
                strFromValue = "";
                strSqlTableName = "";
                strNum = "";

                // TODO: 用 GetElementsByTagName 优化
                XmlNode nodeFrom = nodeKey.SelectSingleNode("from");
                if (nodeFrom != null)
                    strFromValue = nodeFrom.InnerText.Trim(); // 2012/2/16

                // 找不到<key>下级的<table>节点,就应该报错
                XmlNode nodeTable = nodeKey.SelectSingleNode("table");
                if (nodeTable == null)
                {
                    strError = "<key>下级未定义<table>节点。";
                    return -1;
                }

                TableInfo tableInfo = (TableInfo)this.tableTableInfoClient[nodeTable];
                Debug.Assert(tableInfo != null, "从Hashtable里取出的tabInfo不可能为null。");



                strSqlTableName = tableInfo.SqlTableName.Trim();

                // 根据语言版本获得来源名称
                strFromName = tableInfo.GetCaption(strLang);


                // 所有的检索点字符串
                List<string> aKey = new List<string>();


                XmlNode nodeXpath = nodeKey.SelectSingleNode("xpath");
                string strScriptAttr = "";
                if (nodeXpath != null)
                    strScriptAttr = DomUtil.GetAttr(nodeXpath, "scripting");

                
                if (String.Compare(strScriptAttr, "on", true) == 0)
                {
                    // 执行脚本得到检索点
                    //aKey.Add("abc");

                    //string strOutputString = "";
                    List<String> OutputStrings = null;
                    string strFunctionName = nodeXpath.InnerText.Trim();     // 2012/2/16
                    nRet = this.DoScriptFunction(domData,
                        strFunctionName,
                        "", //strInputString
                        // out strOutputString,
                        out OutputStrings,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 2007/1/23
                    if (OutputStrings != null)
                    {
                        for (int j = 0; j < OutputStrings.Count; j++)
                        {
                            if (String.IsNullOrEmpty(OutputStrings[j]) == false)
                            {
                                aKey.Add(OutputStrings[j]);
                                // nCount++;
                            }
                        }
                    }

                }
                else
                {
                    string strXpath = "";
                    if (nodeXpath != null)
                        strXpath = nodeXpath.InnerText.Trim(); // 2012/2/16

                    string strNstableName = DomUtil.GetAttrDiff(nodeXpath, "nstable");
#if NO
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

                    XPathExpression expr = nav.Compile(strXpath);   // TODO 可以优化
                    if (nsmgr != null)
                        expr.SetContext(nsmgr);
#endif
                    // 2012/7/20优化
                    XPathExpression expr = (XPathExpression)m_exprCache[nodeXpath];

                    if (expr == null)
                    {
                        this.m_exprCache.Clear();
                        goto CREATE_CACHE;  // TODO: 如何预防死循环?
                    }

                    string strMyKey = "";

                    if (expr.ReturnType == XPathResultType.Number)
                    {
                        strMyKey = nav.Evaluate(expr).ToString();//Convert.ToString((int)(nav.Evaluate(expr)));
                        aKey.Add(strMyKey);
                    }
                    else if (expr.ReturnType == XPathResultType.Boolean)
                    {
                        strMyKey = Convert.ToString((bool)(nav.Evaluate(expr)));
                        aKey.Add(strMyKey);
                    }
                    else if (expr.ReturnType == XPathResultType.String)
                    {
                        strMyKey = (string)(nav.Evaluate(expr));
                        aKey.Add(strMyKey);
                    }
                    else if (expr.ReturnType == XPathResultType.NodeSet)
                    {
                        // ????????xpath命中多个节点时，是否创建多个key
                        XPathNodeIterator iterator = null;
                        try
                        {
                            iterator = nav.Select(expr);
                        }
                        catch (Exception ex)
                        {
                            string strTempNstableName = "";
                            if (strNstableName == null)
                                strTempNstableName = "null";
                            else
                                strTempNstableName = "'" + strNstableName + "'";
                            strError = "用路径'" + strXpath + "'选节点时出错，" + ex.Message + " \r\n使用的名字空间表名为" + strTempNstableName + "。";
                            return -1;
                        }

                        if (iterator != null)
                        {
                            while (iterator.MoveNext())
                            {
                                XPathNavigator navigator = iterator.Current;
                                strMyKey = navigator.Value;
                                if (strMyKey == "")
                                    continue;

                                aKey.Add(strMyKey);
                            }
                        }
                    }
                    else
                    {
                        throw (new Exception("XPathExpression的ReturnType为'" + expr.ReturnType.ToString() + "'无效"));
                    }
                }


                for (int j = 0; j < aKey.Count; j++)
                {
                    strKey = aKey[j];
                    //???????注意，如果key内容为空，是否也应该算作一个key呢?
                    if (strKey == "")
                    	continue;

                    strKeyNoProcess = strKey;
                    strNum = "-1";

                    List<string> outputKeys = new List<string>();
                    if (tableInfo.nodeConvertKeyString != null)
                    {
                        nRet = ConvertKeyWithStringNode(domData,
                            strKey,
                            tableInfo.nodeConvertKeyString,
                            out outputKeys,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    {
                        outputKeys = new List<string>();
                        outputKeys.Add(strKey);
                    }

                    for (int k = 0; k < outputKeys.Count; k++)
                    {
                        string strOneKey = outputKeys[k];
                        //根据自身的配置进行处理,得到num
                        if (tableInfo.nodeConvertKeyNumber != null)
                        {
                            nRet = ConvertKeyWithNumberNode(
                                domData,
                                strOneKey,
                                tableInfo.nodeConvertKeyNumber,
                                out strNum,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 1)
                            {
                                // 2010/9/27
                                strOneKey = strError + " -- " + strOneKey;
                                strNum = "-1";
                            }

                            // 2010/11/20
                            if (String.IsNullOrEmpty(strNum) == true)
                                continue;
                        }

                        if (strOneKey.Length > nKeySize)
                            strOneKey = strOneKey.Substring(0, nKeySize);
                        if (strNum.Length >= 20)
                            strNum = strNum.Substring(0, 19);

                        KeyItem keyItem = new KeyItem(strSqlTableName,
                            strOneKey,
                            strFromValue,
                            strRecordID,
                            strNum,
                            strKeyNoProcess,
                            strFromName);

                        keys.Add(keyItem);
                    }
                }
            }


            return 0;
        }

        // 执行脚本函数
        // parameters:
        //      dataDom         数据dom
        //      strFunctionName 函数名
        //      strResultString out参数，返回结果字符串
        //      strError        out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int DoScriptFunction(XmlDocument dataDom,
            string strFunctionName,
            ref List<string> aInputString,            
            out string strError)
        {
            strError = "";

            // Debug.Assert(dataDom != null,"DoScriptFunction()调用错误，dataDom参数值不能为null。");
            Debug.Assert(strFunctionName != null && strFunctionName != "", "DoScriptFunction()调用错误，strFunctionName参数值不能为null。");

            if (aInputString == null)
                return 0;

            int nRet = 0;

            List<string> resultstrings = new List<string>();

            for (int i = 0; i < aInputString.Count; i++)
            {
                string strInputString = aInputString[i];

                // string strOutputString = "";
                List<string> OutputStrings = null;
                nRet = this.DoScriptFunction(dataDom,
                    strFunctionName,
                    strInputString,
                    // out strOutputString,
                    out OutputStrings,
                    out strError);
                if (nRet == -1)
                    return -1;

                int nCount = 0;
                if (OutputStrings != null)
                {
                    for (int j = 0; j < OutputStrings.Count; j++)
                    {
                        if (String.IsNullOrEmpty(OutputStrings[j]) == true)
                            continue;
                        resultstrings.Add(OutputStrings[j]);
                        nCount++;
                    }

                }
                if (nCount == 0 && dataDom == null)
                    resultstrings.Add("");  // 防止加工检索词的时候报错
            }

            aInputString = resultstrings;

            return 0;
        }


        // 执行脚本函数
        // parameters:
        //      dataDom         数据dom
        //      strFunctionName 函数名
        //      strInputString  输入的待处理的字符串
        //      strResultString out参数，返回结果字符串
        //      strError        out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int DoScriptFunction(XmlDocument dataDom,
            string strFunctionName,
            string strInputString,
            // out string strOutputString,
            out List<string> output_strings,
            out string strError)
        {
            strError = "";
            output_strings = null;

            if (this.m_strAssemblyError != "")
            {
                strError = this.m_strAssemblyError;
                return -1;
            }

            if (this.m_assembly == null)
            {
                strError = "keys 配置文件 '"+this.CfgFileName+"' 中未定义脚本代码，因此无法使用脚本函数'" + strFunctionName + "'。";
                return -1;

                //strOutputString = "";
                //return 0;
            }

            Type hostEntryClassType = GetDerivedClassType(
                this.m_assembly,
                "DigitalPlatform.rms.KeysHost");    // TODO: 可以用Hashtable优化
            if (hostEntryClassType == null)
            {
                strError = "从keys配置文件脚本中未找到DigitalPlatform.rms.KeysHost的派生类";
                return -1;
            }

            KeysHost host = (KeysHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "从Type对象获取KeysHost实例为null。";
                return -1;
            }
            host.DataDom = dataDom;
            host.CfgDom = this.dom;
            host.InputString = strInputString;

            // 执行函数
            try
            {
                host.Invoke(strFunctionName);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + strFunctionName + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            output_strings = host.ResultStrings;

            if (output_strings == null)
                output_strings = new List<string>();

            if (String.IsNullOrEmpty(host.ResultString) == false)
                output_strings.Insert(0, host.ResultString);

            return 0;
        }


        //得到派生类
        //parameter:
        //		assembly            Assembly对象
        //		strBaseTypeFullName 基类全名称
        public static Type GetDerivedClassType(Assembly assembly,
            string strBaseTypeFullName)
        {
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsClass == false)
                    continue;
                if (IsDeriverdFrom(types[i],
                    strBaseTypeFullName) == true)
                    return types[i];
            }
            return null;
        }


        // 观察type的基类中是否有类名为strBaseTypeFullName的类。
        public static bool IsDeriverdFrom(Type type,
            string strBaseTypeFullName)
        {
            Type curType = type;
            for (; ; )
            {
                if (curType == null
                    || curType.FullName == "System.Object")
                    return false;

                if (curType.FullName == strBaseTypeFullName)
                    return true;

                curType = curType.BaseType;
            }
        }
        
        // 清空对象
        public override void Clear()
        {
            this.tableNsClient.Clear();
            this.tableNsServer.Clear();

            this.tableTableInfoClient.Clear();
            this.tableTableInfoServer.Clear();

            m_exprCache.Clear();
        }

        // 根据表名得到表的属性信息
        // parameters:
        // return:
        //		-1	出错
        //		0	未找到
        //		1	找到
        public int GetTableInfo(string strTableName,
            List<TableInfo> aTableInfo,
            out TableInfo tableInfo,
            out string strError)
        {
            tableInfo = null;
            strError = "";

            // 如果参数aTableInfo == null，表示要马上获取；如果!=null，表示利用这个参数的现成内容
            if (aTableInfo == null)
            {
                int nRet = this.GetTableInfos(
                    out aTableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            for (int i = 0; i < aTableInfo.Count; i++)
            {
                TableInfo oneTableInfo = aTableInfo[i];
                if (StringUtil.IsInList(strTableName, oneTableInfo.GetAllCaption()) == true)
                {
                    tableInfo = oneTableInfo;
                    return 1;
                }
            }
            strError = "未找到逻辑名'" + strTableName + "'对应的<table>对象";
            return 0;
        }

        // 得到配置文件中定义TableInfo数组，有重复的
        // parameters:
        //      aTableInfo  out参数，返回TableInfo对象数组
        //      strError    out参数，返回出错信息
        // return
        //		-1	出错
        //		0	成功
        public int GetTableInfos(
            out List<TableInfo> aTableInfo,
            out string strError)
        {
            strError = "";
            aTableInfo = new List<TableInfo>();

            if (this.m_aTableInfoForForm != null)
            {
                aTableInfo = this.m_aTableInfoForForm;
                return 0;
            }

            if (this.dom == null)
                return 0;


            int nRet = 0;

            // 找到<key>下级的所有不带ref属性的 <table>
            string strXpath = "//table[not(@ref)]";
            XmlNodeList nodeListTable = this.dom.DocumentElement.SelectNodes(strXpath);//"//key/table");
            for (int i = 0; i < nodeListTable.Count; i++)
            {
                XmlNode nodeTable = nodeListTable[i];

                TableInfo tableInfo = null;
                // return:
                //		-1	出错
                //		0	成功
                nRet = this.GetTableInfo(nodeTable,
                    out tableInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                tableInfo.OriginPosition = i + 1; //原始序

                string strStyle = DomUtil.GetAttr(nodeTable, "style");
                if (StringUtil.IsInList("query", strStyle) == true)
                    tableInfo.m_bQuery = true;
                else
                    tableInfo.m_bQuery = false;



                aTableInfo.Add(tableInfo);
            }

            // aTableInfo.Sort();   // 这里排序到底是按照什么来排的？莫名其妙

            nRet = this.MaskDup(aTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            this.m_aTableInfoForForm = aTableInfo;

            return 0;
        }


        // 得到去重的TableInfo数组，用于列表等
        // parameters:
        //      aTableInfo  out参数，返回TableInfo对象数组
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int GetTableInfosRemoveDup(
            out List<TableInfo> aTableInfo,
            out string strError)
        {
            aTableInfo = new List<TableInfo>();
            strError = "";

            List<TableInfo> aTempTableInfo = null;
            int nRet = this.GetTableInfos(
                out aTempTableInfo,
                out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < aTempTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTempTableInfo[i];
                if (tableInfo.Dup == true)
                    continue;
                aTableInfo.Add(tableInfo);
            }

            return 0;
        }

#if NO
        // 对本集合的成员进行去重，打去重标记
        // parameters:
        //      aTableInfo  TableInfo数组
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int MaskDup(List<TableInfo> aTableInfo,
            out string strError)
        {
            strError = "";

            TableInfo holdTableInfo = null;
            for (int i = 0; i < aTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTableInfo[i];
                if (holdTableInfo == null)
                {
                    holdTableInfo = tableInfo;
                    continue;
                }

                if (tableInfo.CompareTo(holdTableInfo) == 0)
                {
                    tableInfo.Dup = true;
                    if (tableInfo.SqlTableName != holdTableInfo.SqlTableName)
                    {
                        strError = "序号(从1开始计序)为 '" + Convert.ToString(tableInfo.OriginPosition) + "' 的<table>元素与序号为 '" + Convert.ToString(holdTableInfo.OriginPosition) + "' 的<table>元素的 'id' 属性相同，但 'name' 属性不同，这是不合法的。";
                        return -1;
                    }
                }
                else
                {
                    holdTableInfo = tableInfo;
                }
            }
            return 0;
        }
#endif

        // 对表名相同的打上重复标记。调用前不需要排序
        // parameters:
        //      aTableInfo  TableInfo数组
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int MaskDup(List<TableInfo> aTableInfo,
            out string strError)
        {
            strError = "";

            Hashtable name_table = new Hashtable();
            for (int i = 0; i < aTableInfo.Count; i++)
            {
                TableInfo tableInfo = aTableInfo[i];

                string strTableName = tableInfo.SqlTableName.ToLower();
                if (name_table[strTableName] == null)
                {
                    name_table[strTableName] = 1;
                }
                else
                    tableInfo.Dup = true;
            }
            return 0;
        }


        #region 加工字符串的静态函数



        // 对数组类型的检索点进行加工
        // parameter:
        //		strText	待加工的字符串
        //		stringNode	number节点
        //		strKey	out 加工后的检索点字符串
        //		strError	out 出错信息
        // return:
        //		-1	出错
        //		0	成功
        //      1   转换为数字的过程失败 strError中有报错信息 2010/9/27
        public int ConvertKeyWithNumberNode(
            XmlDocument dataDom,
            string strText,
            XmlNode numberNode,
            out string strKey,
            out string strError)
        {
            strKey = "";
            strError = "";

            if (numberNode == null)
            {
                strError = "ConvertKeyWithNumberNode(),numberNode参数不能为null";
                return -1;
            }

            strKey = strText;

            // 当为money时,扩展的位数
            string strPrecision = DomUtil.GetAttr(numberNode, "precision");

            string strStyles = DomUtil.GetAttr(numberNode, "style");
            string[] styles = strStyles.Split(new char[] { ',' });
            foreach (string strOneStyleParam in styles)
            {
                string strOneStyle = strOneStyleParam.Trim();

                if (String.IsNullOrEmpty(strOneStyle) == true)
                    continue;

                string strOneStyleLower = strOneStyle.ToLower();

                if (strOneStyleLower == "money")
                {
                    if (strPrecision == "")
                        strPrecision = "0";
                    strKey = StringUtil.ExtendByPrecision(
                        strKey,
                        strPrecision);
                }
                else if (strOneStyleLower == "integer")
                {
                    strKey = StringUtil.ExtendByPrecision(
                        strKey,
                        "0");
                }
                else if (strOneStyleLower == "rfc1123time")
                {
                    if (string.IsNullOrEmpty(strKey) == true)
                    {
                        // 2012/3/30
                        strKey = "";
                    } 
                    else if (strKey == "0")
                    {
                    }
                    else if (strKey == "9999999999")
                    {
                        strKey = DateTime.MaxValue.Ticks.ToString();
                    }
                    else
                    {
                        long nTicks = -1; //缺省值-1
                        try
                        {
                            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strKey);
                            nTicks = time.Ticks;
                        }
                        catch
                        {
                            strError = "时间字符串 '" + strKey + "' 不是合法的rfc1123格式";
                            return 1;
                        }

                        strKey = Convert.ToString(nTicks);
                    }
                }
                else if (strOneStyleLower == "utime")// 2010/2/12
                {
                    if (string.IsNullOrEmpty(strKey) == true)
                    {
                        // 2012/3/29
                        strKey = "";
                    }
                    else if (strKey == "0")
                    {
                    }
                    else if (strKey == "9999999999")
                    {
                        strKey = DateTime.MaxValue.Ticks.ToString();
                    }
                    else
                    {
                        // 2010-01-01 12:01:01Z
                        // 可以写为
                        // 2010/01/01 12:01:01Z
                        strKey = strKey.Replace("/", "-");

                        long nTicks = -1; //缺省值-1
                        try
                        {
                            DateTime time = DateTimeUtil.FromUTimeString(strKey);
                            nTicks = time.Ticks;
                        }
                        catch
                        {
                            strError = "时间字符串 '" + strKey + "' 不是合法的utime格式";
                            return 1;
                        }

                        strKey = Convert.ToString(nTicks);
                    }
                }
                else if (strOneStyleLower == "freetime")// 2012/5/15
                {
                    if (string.IsNullOrEmpty(strKey) == true)
                    {
                        strKey = "";
                    }
                    else if (strKey == "0")
                    {
                    }
                    else if (strKey == "9999999999")
                    {
                        strKey = DateTime.MaxValue.Ticks.ToString();
                    }
                    else
                    {
                        long nTicks = -1; //缺省值-1
                        try
                        {
                            DateTime time = DateTimeUtil.ParseFreeTimeString(strKey);
                            nTicks = time.Ticks;
                        }
                        catch
                        {
                            strError = "时间字符串 '" + strKey + "' 不是合法的freetime格式";
                            return 1;
                        }

                        strKey = Convert.ToString(nTicks);
                    }
                }
                else
                {
                    // 2010/11/20


                    // 处理C#脚本函数调用
                    string strFirstChar = "";
                    if (strOneStyle.Length > 0)
                        strFirstChar = strOneStyle.Substring(0, 1);

                    // 脚本函数
                    if (strFirstChar == "#")
                    {
                        string strFunctionName = strOneStyle.Substring(1);
                        if (strFunctionName == "")
                        {
                            strError = "加工检索点时出错，发现数字风格'" + strOneStyle + "'未写脚本函数名。";
                            return -1;

                        }
                        List<String> keys = new List<string>();
                        keys.Add(strKey);
                        int nRet = this.DoScriptFunction(dataDom,
                            strFunctionName,
                            ref keys,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (keys.Count > 0)
                            strKey = keys[0];
                        else
                            strKey = null;
                    }
                    else
                    {
                        strError = "加工检索点时,当是数值类型时，不支持'" + strOneStyle + "'风格，必须是'money','integer','rfc1123time','utime'或者'#...'";
                        return -1;
                    }

                    /*
                    strError = "加工检索点时,当是数值类型时，不支持'" + strOneStyle + "'风格，必须是'money','integer','rfc1123time','utime'";
                    return -1;
                     * */
                }
            }
            return 0;
        }

        // 对字符串类型的检索点进行加工
        // parameter:
        //		strText	待加工的字符串
        //		stringNode	string节点
        //		keys	out 加工后的检索点数组
        //		strError	out 出错信息
        // return:
        //		-1	出错
        //		0	成功
        public int ConvertKeyWithStringNode(
            XmlDocument dataDom,
            string strText,
            XmlNode stringNode,
            out List<string> keys,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            keys = null;

            if (stringNode == null)
            {
                strError = "ConvertKeyWithStringNode(),stringNode参数不能为null";
                return -1;
            }

            keys = new List<string>();


            // 把传入的字符串作为第一个项
            keys.Add(strText);

            // 得到风格定义
            string strStyles = DomUtil.GetAttr(stringNode, "style");
            string[] styles = strStyles.Split(new char[] { ',' });
            bool bHasFoundStopword = false;
            string strStopwordTableName = DomUtil.GetAttr(stringNode, "stopwordTable"); // BUG !!! 2012/4/18 以前为stopwordtable
            foreach (string strOneStyleParam in styles)
            {
                string strOneStyle = strOneStyleParam.Trim();

                if (String.IsNullOrEmpty(strOneStyle) == true)
                    continue;

                string strOneStyleLower = strOneStyle.ToLower();

                if (strOneStyleLower == "upper")
                {
                    // 将一个字符串数组的内容都变成大写
                    KeysCfg.DoUpper(ref keys);
                }
                else if (strOneStyleLower == "lower")
                {
                    // 将一个字符串数组的内容都变成小写
                    KeysCfg.DoLower(ref keys);
                }
                else if (strOneStyleLower == "removeblank")
                {
                    // 去掉空格
                    KeysCfg.RemoveBlank(ref keys);
                }
                else if (strOneStyleLower == "removecmdcr")
                {
                    // 2012/11/6
                    // 去掉 {cr:...} 命令部分
                    for (int i = 0; i < keys.Count; i++)
                    {
                        string strKey = keys[i];
                        string strCmd = StringUtil.GetLeadingCommand(strKey);
                        if (string.IsNullOrEmpty(strCmd) == false
                            && StringUtil.HasHead(strCmd, "cr:") == true)
                        {
                            strKey = strKey.Substring(strCmd.Length + 2);
                            if (string.IsNullOrEmpty(strKey) == true)
                            {
                                keys.RemoveAt(i);
                                i--;
                                continue;
                            }

                            keys[i] = strKey;
                        }
                    }
                }
                else if (strOneStyleLower == "pinyinab")
                {
                    // 拼音缩写字头
                    KeysCfg.DoPinyinAb(ref keys);
                }
                else if (strOneStyleLower == "simplify")
                {
                    // 将一个字符串数组的内容都变成简体
                    KeysCfg.DoSimplify(ref keys);
                }
                else if (strOneStyleLower == "traditionalize")
                {
                    // 将一个字符串数组的内容都变成繁体
                    KeysCfg.DoTraditionalize(ref keys);
                }
                else if (strOneStyleLower == "fulltext")
                {
                    List<string> result = new List<string>();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        List<string> lines = SplitFullTextContent(keys[i]);
                        result.AddRange(lines);
                    }

                    keys = result;
                }
                else if (strOneStyleLower == "split")
                {
                    if (bHasFoundStopword == true)
                    {
                        bool bInStopword = false;

                        nRet = this.StopwordCfg.IsInStopword(",",
                            strStopwordTableName,
                            out bInStopword,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (bInStopword == true)
                        {
                            strError = "加工检索点,先使用了'stopword'去非用字功能,且非用字中包含','，那么再使用'split'风格则无意义。";
                            return -1;
                        }
                    }
                    /*
                    if (keys.Length != 1)
                    {
                        strError = "加工检索点时,在做split以前不可以变成多个检索点";
                        return -1;
                    }
                     */

                    List<string> result = new List<string>();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        string[] tempKeys = keys[i].Split(new char[] { ',','，',' ', '　'});   // 半角和全角的逗号和空格
                        result.AddRange(tempKeys);
                    }

                    keys = result;
                }
                else if (strOneStyleLower == "stopword")
                {
                    if (this.StopwordCfg == null)
                    {
                        strError = "在检索点的配置中使用了stopword，但StopwordCfg对象不存在。";
                        return -1;
                    }

                    // 对一个字符串数组进行去非用字
                    // parameter:
                    //		texts	待加工的字符串数组
                    //		strStopwordTable	具体使用非用字哪个表 为""或null表示取第一个表
                    //		strError	out 出错信息
                    // return:
                    //		-1	出错
                    //		0	成功
                    nRet = this.StopwordCfg.DoStopword(strStopwordTableName,
                        ref keys,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    bHasFoundStopword = true;
                }
                else if (strOneStyleLower == "distribute_refids")
                {
                    // 2008/10/22

                    List<string> results = new List<string>();
                    
                    
                    for (int i = 0; i < keys.Count; i++)
                    {
                        List<string> temp_ids = GetLocationRefIDs(keys[i]);
                        if (temp_ids.Count > 0)
                            results.AddRange(temp_ids);
                    }

                    keys = results;
                }
                else
                { 
                    // 处理C#脚本函数调用
                    string strFirstChar = "";
                    if (strOneStyle.Length > 0)
                        strFirstChar = strOneStyle.Substring(0, 1);

                    // 脚本函数
                    if (strFirstChar == "#")
                    {
                        string strFunctionName = strOneStyle.Substring(1);
                        if (strFunctionName == "")
                        {
                            strError = "加工检索点时出错，发现字符串风格'" + strOneStyle + "'未写脚本函数名。";
                            return -1;

                        }

                        nRet = this.DoScriptFunction(dataDom,
                            strFunctionName,
                            ref keys,
                            out strError);
                        if (nRet == -1)
                            return -1;

                    }
                    else
                    {
                        strError = "加工检索点时,当是字符串类型时，不支持'" + strOneStyle + "'风格";
                        return -1;
                    }
                }
            }

            return 0;
        }

        // 按照回车换行来切割，如果还不够小，则按逗号、句号、感叹号、问号书名号等来进一步切割
        static List<string> SplitFullTextContent(string strContent)
        {
            List<string> results = new List<string>();
            string [] parts = strContent.Split(new char []{',',
                ' ',
                '　',    // 全角空格
                '.',
                '。',
                ':','：',
                ';','；',
                '!','！',
                '?','？',
                '＜','＞',
                '《','》',
                '/','\\',
                '(',')',
                '\r',
                '\n'
            });
            StringBuilder line = new StringBuilder(4096);
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i];
                if (strPart.Length + line.Length < 200)
                {
                    line.Append(strPart);
                }
                else
                {
                    results.Add(line.ToString());
                    line = new StringBuilder(strPart);
                }
            }

            if (line.Length > 0)
                results.Add(line.ToString());

            return results;
        }

        // 将采购馆藏字符串中的refid解析出来
        // 花括号中,逗号是原来的分割refid字符粗汉之间的符号，'|'是套内分割refid的符号
        public static List<string> GetLocationRefIDs(string strText)
        {
            List<string> results = new List<string>();

            if (String.IsNullOrEmpty(strText) == true)
                return results;

            int nStart = 0;
            int nEnd = 0;
            int nPos = 0;
            for (; ; )
            {
                nStart = strText.IndexOf("{", nPos);
                if (nStart == -1)
                    break;
                nPos = nStart + 1;
                nEnd = strText.IndexOf("}", nPos);
                if (nEnd == -1)
                    break;
                nPos = nEnd + 1;
                if (nEnd <= nStart + 1)
                    continue;
                string strPart = strText.Substring(nStart + 1, nEnd - nStart - 1).Trim();

                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                string[] ids = strPart.Split(new char[] { ',', '|' });  // '|' 2010/12/6 add
                for (int j = 0; j < ids.Length; j++)
                {
                    string strID = ids[j].Trim();
                    if (String.IsNullOrEmpty(strID) == true)
                        continue;

                    results.Add(strID);
                }
            }

            return results;
        }


        // 将一个字符串数组的内容都变成大写
        public static void DoUpper(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = texts[i].ToUpper();
            }
        }

        // 将一个字符串数组的内容都变成小写
        public static void DoLower(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = texts[i].ToLower();
            }
        }

        // 将一个字符串数组的内容都变成小写
        public static void RemoveBlank(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = texts[i].Replace(" ", "");
                texts[i] = texts[i].Replace("　", "");
            }
        }

        // 变成拼音字头缩写
        public static void DoPinyinAb(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                string strText = texts[i];

                texts[i] = PinyinAb(strText);
            }
        }

        // 变成拼音字头缩写
        public static string PinyinAb(string strText)
        {
            string strResult = "";
            string[] words = strText.Split(new char[] {' ','　',',','，','-','－','_','＿','.','。',';','；',':','：','、','?','？','!','！','\'','\"','“','”','‘','’','[',']','［','］','(',')','（','）','@','·'});
            for (int i = 0; i < words.Length; i++)
            {
                string strWord = words[i].Trim();
                if (strWord.Length == 0)
                    continue;
                char ch = strWord[0];
                if (ch < 'a' && ch > 'z')
                    continue;
                if (ch < 'A' && ch > 'Z')
                    continue;
                strResult += ch;
            }

            return strResult;
        }

        // 将一个字符串数组的内容都变成简体
        public static void DoSimplify(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = API.ChineseT2S(texts[i]);
            }
        }

        // 将一个字符串数组的内容都变成繁体
        public static void DoTraditionalize(ref List<string> texts)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = API.ChineseS2T(texts[i]);
            }
        }


        #endregion

    }


    public class KeysHost
    {
        public XmlDocument DataDom = null;
        public XmlDocument CfgDom = null;

        public string InputString = "";

        public string ResultString = "";

        public List<string> ResultStrings = new List<string>();

        public KeysHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // new一个Host派生对象
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);

        }

    }
}
