using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Web;

using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

using DigitalPlatform;	// Stop类
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.Interfaces;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryServer.Common;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和C#脚本相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 2017/4/24
        public ReaderWriterLockSlim _lockAssembly = new ReaderWriterLockSlim();
        string _scriptMD5 = "";

        public Assembly m_assemblyLibraryHost = null;
        public string m_strAssemblyLibraryHostError = "";

        public List<MessageInterface> m_externalMessageInterfaces = null;

        // 初始化扩展的消息接口
        /*
	<externalMessageInterface>
 		<interface type="sms" assemblyName="chchdxmessageinterface"/>
	</externalMessageInterface>
         */
        // parameters:
        // return:
        //      -1  出错
        //      0   当前没有配置任何扩展消息接口
        //      1   成功初始化
        public int InitialExternalMessageInterfaces(out string strError)
        {
            strError = "";

            _lockAssembly.EnterWriteLock();
            try
            {
                this.m_externalMessageInterfaces = null;

                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("externalMessageInterface");
                if (root == null)
                {
                    strError = "在library.xml中没有找到<externalMessageInterface>元素";
                    return 0;
                }

                this.m_externalMessageInterfaces = new List<MessageInterface>();

                XmlNodeList nodes = root.SelectNodes("interface");
                foreach (XmlNode node in nodes)
                {
                    string strType = DomUtil.GetAttr(node, "type");
                    if (String.IsNullOrEmpty(strType) == true)
                    {
                        strError = "<interface>元素未配置type属性值";
                        return -1;
                    }

                    string strAssemblyName = DomUtil.GetAttr(node, "assemblyName");
                    if (String.IsNullOrEmpty(strAssemblyName) == true)
                    {
                        strError = "<interface>元素未配置assemblyName属性值";
                        return -1;
                    }

                    MessageInterface message_interface = new MessageInterface();
                    message_interface.Type = strType;
                    message_interface.Assembly = Assembly.Load(strAssemblyName);
                    if (message_interface.Assembly == null)
                    {
                        strError = "名字为 '" + strAssemblyName + "' 的Assembly加载失败...";
                        return -1;
                    }

                    Type hostEntryClassType = ScriptManager.GetDerivedClassType(
            message_interface.Assembly,
            "DigitalPlatform.Interfaces.ExternalMessageHost");
                    if (hostEntryClassType == null)
                    {
                        strError = "名字为 '" + strAssemblyName + "' 的Assembly中未找到 DigitalPlatform.Interfaces.ExternalMessageHost类的派生类，初始化扩展消息接口失败...";
                        return -1;
                    }

                    message_interface.HostObj = (ExternalMessageHost)hostEntryClassType.InvokeMember(null,
            BindingFlags.DeclaredOnly |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
            null);
                    if (message_interface.HostObj == null)
                    {
                        strError = "创建 type 为 '" + strType + "' 的 DigitalPlatform.Interfaces.ExternalMessageHost 类的派生类的对象（构造函数）失败，初始化扩展消息接口失败...";
                        return -1;
                    }

                    message_interface.HostObj.App = this;

                    this.m_externalMessageInterfaces.Add(message_interface);
                }

                return 1;
            }
            finally
            {
                _lockAssembly.ExitWriteLock();
            }
        }

        public MessageInterface GetMessageInterface(string strType)
        {
            _lockAssembly.EnterReadLock();
            try
            {
                // 2012/3/29
                if (this.m_externalMessageInterfaces == null)
                    return null;

                foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
                {
                    if (message_interface.Type == strType)
                        return message_interface;
                }

                return null;
            }
            finally
            {
                _lockAssembly.ExitReadLock();
            }
        }

        // 初始化 Assembly 对象
        // return:
        //		-1	出错
        //		0	脚本代码没有找到
        //      1   成功
        int _initialLibraryHostAssembly(
            Assembly existing_assembly,
            string strExistingMD5,
            out Assembly assembly,
            out string strMD5,
            out string strError)
        {
            assembly = null;
            strMD5 = "";
            strError = "";
            int nRet = 0;

            if (this.LibraryCfgDom == null)
            {
                assembly = null;
                strError = "LibraryCfgDom为空";
                return -1;
            }

            // 找到<script>节点
            // 必须在根下
            XmlNode nodeScript = this.LibraryCfgDom.DocumentElement.SelectSingleNode("script");

            // <script>节点不存在
            if (nodeScript == null)
                return 0;

            // <script>节点下级无CDATA节点
            if (nodeScript.ChildNodes.Count == 0)
                return 0;

            XmlNode firstNode = nodeScript.ChildNodes[0];

            //第一个儿子节点不是CDATA或者Text节点时
            if (firstNode.NodeType != XmlNodeType.CDATA
                && firstNode.NodeType != XmlNodeType.Text)
                return 0;

            //~~~~~~~~~~~~~~~~~~
            // 创建Assembly对象
            string[] saRef = null;
            nRet = GetRefs(nodeScript,
                 out saRef,
                 out strError);
            if (nRet == -1)
                return -1;

            string[] saAddRef = {
                "netstandard.dll",
                Path.Combine(this.BinDir , "digitalplatform.core.dll"),
                Path.Combine(this.BinDir , "digitalplatform.LibraryServer.dll"),
            };

            string[] saTemp = new string[saRef.Length + saAddRef.Length];
            Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
            Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
            saRef = saTemp;

            RemoveRefsProjectDirMacro(ref saRef,
                this.BinDir);

            string strCode = firstNode.Value;

            if (string.IsNullOrEmpty(strCode) == true)
                return 0;

            // 将 strCode 和 saRef 构造 hash 字符串
            strMD5 = StringUtil.GetMd5(strCode + "\r\n" + StringUtil.MakePathList(saRef));
            if (existing_assembly != null
                && strMD5 == strExistingMD5)
            {
                assembly = existing_assembly;
                return 1;   // 代码没有变化，不用刷新 Assembly
            }

            {
                string strWarning = "";
                nRet = CreateAssembly(strCode,
                    saRef,
                    out assembly,
                    out strError,
                    out strWarning);
                if (nRet == -1)
                {
                    strMD5 = "";   // 2017/5/22 迫使后面重新编译
                    strError = "library.xml中<script>元素内C#脚本编译时出错: \r\n" + strError;
                    return -1;
                }
            }

            Debug.Assert(assembly != null, "");
            return 1;
        }


        // 初始化Assembly对象
        // return:
        //		-1	出错
        //		0	脚本代码没有找到
        //      1   成功
        public int InitialLibraryHostAssembly(out string strError)
        {
            strError = "";
            int nRet = 0;

            Assembly existing_assembly = null;
            string strExistingMD5 = "";

            _lockAssembly.EnterReadLock();
            try
            {
                existing_assembly = this.m_assemblyLibraryHost;
                strExistingMD5 = this._scriptMD5;
            }
            finally
            {
                _lockAssembly.ExitReadLock();
            }

            Assembly assembly = null;
            string strMD5 = "";

            // 初始化 Assembly 对象
            // return:
            //		-1	出错
            //		0	脚本代码没有找到
            //      1   成功
            nRet = _initialLibraryHostAssembly(
                existing_assembly,
                strExistingMD5,
                out assembly,
                out strMD5,
                out strError);

            _lockAssembly.EnterWriteLock();
            try
            {
                if (nRet == -1)
                {
                    this.m_strAssemblyLibraryHostError = strError;
                    this.m_assemblyLibraryHost = null;
                    this._scriptMD5 = "";
                }
                else
                {
                    this.m_strAssemblyLibraryHostError = "";    // 2017/5/22
                    if (nRet == 1)
                    {
                        Debug.Assert(assembly != null, "");
                    }
                    this.m_assemblyLibraryHost = assembly;
                    this._scriptMD5 = strMD5;
                }
            }
            finally
            {
                _lockAssembly.ExitWriteLock();
            }

            return nRet;
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
                saRef[i] = DomUtil.GetNodeText(nodes[i]);
            }
            return 0;
        }

        // 去除路径中的宏%bindir%
        static void RemoveRefsProjectDirMacro(ref string[] refs,
            string strBinDir)
        {
            Hashtable macroTable = new Hashtable();

            macroTable.Add("%bindir%", strBinDir);

            for (int i = 0; i < refs.Length; i++)
            {
                string strNew = PathUtil.UnMacroPath(macroTable,
                refs[i],
                false); // 不要抛出异常，因为可能还有%bindir%宏现在还无法替换
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


            // 2019/4/5
            if (refs != null
                && Array.IndexOf(refs, "netstandard.dll") == -1)
            {
                List<string> temp = new List<string>(refs);
                temp.Add("netstandard.dll");
                refs = temp.ToArray();
            }

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
                strError = "CreateAssemblyFile() 出错 " + ExceptionUtil.GetDebugText(ex);
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
                strResult += "(" + Convert.ToString(oneError.Line) + "," + Convert.ToString(oneError.Column) + ") ";
                strResult += (oneError.IsWarning) ? "warning " : "error ";
                strResult += oneError.ErrorNumber;
                strResult += " : " + oneError.ErrorText + "\r\n";

                if (oneError.IsWarning == false)
                    nCount++;
            }
            return nCount;
        }


        // 执行脚本函数VerifyBarcode
        // parameters:
        //      host    如果为空，则函数内部会 new 一个此类型的对象；如果不为空，则直接使用
        //      strLibraryCodeList  当前操作者管辖的馆代码列表 2014/9/27
        //      nResultValue    [out]返回脚本函数执行的结果
        //                      脚本函数返回值含义: -1: 脚本函数执行出错; 0 不合法的条码号; 1: 合法的读者证条码号; 2: 合法的册条码号
        //      strError        [out]返回本函数的出错信息或脚本函数执行的出错信息
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoVerifyBarcodeScriptFunction(
            LibraryHost host,
            string strLibraryCodeList,
            string strBarcode,
            out int nResultValue,
            out string strError)
        {
            strError = "";
            nResultValue = -1;

            // 2019/5/31
            // 优先用 library.xml 中 barcodeValidation 来校验
            if (this.LibraryCfgDom?.DocumentElement?.SelectSingleNode("barcodeValidation") is XmlElement barcodeValidation)
            {
                try
                {
                    BarcodeValidator validator = new BarcodeValidator(barcodeValidation.OuterXml);
                    var result = validator.Validate(strLibraryCodeList,
                        strBarcode);
                    if (result.OK == false
                        && result.ErrorCode == "scriptError")
                    {
                        strError = $"执行条码校验时出错: {result.ErrorInfo}";
                        return -1;
                    }
                    // 2019/7/30
                    if (result.OK == false
    && result.ErrorCode == "suppressed")
                    {
                        if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                            strError = $"{result.ErrorInfo}";
                        else
                            strError = $"馆藏地 '{strLibraryCodeList}' 不打算提供条码校验规则";
                        return -2;
                    }
                    strError = result.ErrorInfo;
                    if (result.Type == "patron")
                        nResultValue = 1;
                    else if (result.Type == "entity")
                        nResultValue = 2;
                    else
                        nResultValue = 0;
                    return 0;
                }
                catch (Exception ex)
                {
                    strError = "创建 BarcodeValidator() 出现异常: " + ex.Message;
                    return -1;
                }
            }

            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out Assembly assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法校验条码号。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法校验条码号。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法校验条码号。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            // TODO: 如果两个版本的函数都定义了，会发生什么?
            MethodInfo mi = hostEntryClassType.GetMethod("VerifyBarcode");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供int VerifyBarcode(string strBarcode, out string strError)函数，因此无法校验条码号。";
                return -2;
            }

            if (host == null)
            {
                host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                    null);
                if (host == null)
                {
                    strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                    return -1;
                }

                host.App = this;
            }

            ParameterInfo[] parameters = mi.GetParameters();

            // 执行函数
            try
            {
                // 旧版本脚本函数
                if (parameters.Length == 2)
                {
                    object[] args = new object[2];
                    args[0] = strBarcode;
                    args[1] = strError;
                    nResultValue = (int)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strError = (string)args[1];
                }
                else if (parameters.Length == 3)    // 新版本脚本函数
                {
                    object[] args = new object[3];
                    args[0] = strLibraryCodeList;
                    args[1] = strBarcode;
                    args[2] = strError;
                    nResultValue = (int)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strError = (string)args[2];
                }
                else
                {
                    strError = "脚本函数 VerifyBarcode() 的参数个数不正确，应该为 2 或者 3 个";
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + "VerifyBarcode" + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 2017/4/25
        // 执行脚本函数 TransformBarcode
        // parameters:
        //      host    如果为空，则函数内部会 new 一个此类型的对象；如果不为空，则直接使用
        //      strLibraryCodeList  当前操作者管辖的馆代码列表
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoTransformBarcodeScriptFunction(
            LibraryHost host,
            string strLibraryCodeList,
            ref string strBarcode,
            out int nResultValue,
            out string strError)
        {
            strError = "";
            nResultValue = -1;

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义 <script> 脚本代码，无法变换条码号";
                return -2;
            }

            Debug.Assert(assembly != null, "");

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script> 脚本中未找到DigitalPlatform.LibraryServer.LibraryHost 类的派生类，无法变换条码号";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("TransformBarcode");
            if (mi == null)
            {
                strError = "<script> 脚本中 DigitalPlatform.LibraryServer.LibraryHost 类的派生类中，没有提供 int Transform(string strLibraryCodeList,  ref string strBarcode, out string strError) 函数，因此无法变换条码号";
                return -2;
            }

            if (host == null)
            {
                host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                    null);
                if (host == null)
                {
                    strError = "创建 DigitalPlatform.LibraryServer.LibraryHost 类的派生类的对象（构造函数）失败";
                    return -1;
                }

                host.App = this;
            }

            ParameterInfo[] parameters = mi.GetParameters();

            // 执行函数
            try
            {
                if (parameters.Length == 3)
                {
                    object[] args = new object[3];
                    args[0] = strLibraryCodeList;
                    args[1] = strBarcode;
                    args[2] = strError;
                    nResultValue = (int)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strBarcode = (string)args[1];
                    strError = (string)args[2];
                }
                else
                {
                    strError = "脚本函数 TransformBarcode() 的参数个数不正确，应该为 3 个";
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "TransformBarcode" + "' 时出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数ItemCanBorrow
        // parameters:
        //      bResultValue    [out] 是否允许外借。如果返回 true，表示允许外借；false 表示不允许外借
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoItemCanBorrowScriptFunction(
            bool bRenew,
            Account account,
            XmlDocument readerdom,  // 2018/9/30 增加此参数
            XmlDocument itemdom,
            out bool bResultValue,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";
            bResultValue = false;

            // test 2016/10/25
            // account.Location = null;

            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out Assembly assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义 <script> 脚本代码，无法执行脚本函数 ItemCanBorrow()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数ItemCanBorrow()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数ItemCanBorrow()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("ItemCanBorrow");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public bool ItemCanBorrow(bool bRenew, Account account, XmlDocument itemdom, out string strMessageText)函数，因此无法获得可借状态。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            ParameterInfo[] parameters = mi.GetParameters();


            // 执行函数
            try
            {
                if (parameters.Length == 4)
                {
                    // 老版本函数
                    object[] args = new object[4];
                    args[0] = bRenew;
                    args[1] = account;
                    args[2] = itemdom;
                    args[3] = strMessage;
                    bResultValue = (bool)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strMessage = (string)args[3];
                }
                else if (parameters.Length == 5)
                {
                    // 新版本函数
                    object[] args = new object[5];
                    args[0] = bRenew;
                    args[1] = account;
                    args[2] = readerdom;
                    args[3] = itemdom;
                    args[4] = strMessage;
                    bResultValue = (bool)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strMessage = (string)args[4];
                }
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + "ItemCanBorrow" + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数ItemCanReturn
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoItemCanReturnScriptFunction(
            Account account,
            XmlDocument readerdom,  // 2018/9/30 增加此参数
            XmlDocument itemdom,
            out bool bResultValue,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";
            bResultValue = false;

            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out Assembly assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义 <script> 脚本代码，无法执行脚本函数 ItemCanReturn()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数ItemCanReturn()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数ItemCanReturn()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("ItemCanReturn");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public bool ItemCanReturn(Account account, XmlDocument itemdom, out string strMessageText)函数，因此无法获得可借状态。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            ParameterInfo[] parameters = mi.GetParameters();

            // 执行函数
            try
            {
                if (parameters.Length == 3)
                {
                    object[] args = new object[3];
                    args[0] = account;
                    args[1] = itemdom;
                    args[2] = strMessage;
                    bResultValue = (bool)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strMessage = (string)args[2];
                }
                else if (parameters.Length == 4)
                {
                    object[] args = new object[4];
                    args[0] = account;
                    args[1] = readerdom;
                    args[2] = itemdom;
                    args[3] = strMessage;
                    bResultValue = (bool)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strMessage = (string)args[3];
                }
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + "ItemCanReturn" + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数 NotifyReader
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功(这只是表示脚本函数正常执行，而不是表明脚本函数没有返回0以外的值)
        // nResultValue
        //      -1  出错
        //      0   没有必要发送
        //      1   需要发送
        public int DoNotifyReaderScriptFunction(
            XmlDocument readerdom,
            Calendar calendar,
            // List<string> notifiedBarcodes,
            string strBodyType,
            out int nResultValue,
            out string strBody,
            out string strMime,
            // out List<string> wantNotifyBarcodes,
            out string strError)
        {
            strError = "";
            strBody = "";
            nResultValue = -1;
            // wantNotifyBarcodes = null;
            strMime = "";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数NotifyReader()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数NotifyReader()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数NotifyReader()。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            try
            {
                // 早绑定
                nResultValue = host.NotifyReader(
                    readerdom,
                    calendar,
                    // notifiedBarcodes,
                    strBodyType,
                    out strBody,
                    out strMime,
                    // out wantNotifyBarcodes,
                    out strError);
                // 只要脚本函数被正常执行，nRet就是返回0
                // nResultValue的值是脚本函数的返回值
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 NotifyReader() 时抛出异常：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            /*
            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("NotifyReader");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供int NotifyReader()函数，因此进行读者通知。";
                return -2;
            }

            // 执行函数
            try
            {
                object[] args = new object[2];
                args[0] = strBarcode;
                args[1] = strError;
                nResultValue = (int)mi.Invoke(host,
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.InvokeMethod,
                     null,
                     args,
                     null);

                // 取出out参数值
                strError = (string)args[1];
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + "NotifyReader" + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
             * */

            return 0;
        }

        // 执行脚本函数GetForegift
        // 根据已有价格，计算出需要新交的价格
        // parameters:
        //  	strAction	为foregift和return之一
        //      strExistPrice   当前剩余的金额
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoGetForegiftScriptFunction(
            string strAction,
            XmlDocument readerdom,
            string strExistPrice,
            out int nResultValue,
            out string strPrice,
            out string strError)
        {
            strError = "";
            strPrice = "";
            nResultValue = 0;

            string strFuncName = "GetForegift";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public int GetForegift(string strAction, XmlDocument readerdom, string strExistPrice, out string strPrice, out string strError)函数，因此无法获得结果。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            // 执行函数
            try
            {
                object[] args = new object[5];
                args[0] = strAction;
                args[1] = readerdom;
                args[2] = strExistPrice;
                args[3] = strPrice;
                args[4] = strError;
                nResultValue = (int)mi.Invoke(host,
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.InvokeMethod,
                     null,
                     args,
                     null);

                // 取出out参数值
                strPrice = (string)args[3];
                strError = (string)args[4];
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + strFuncName + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数GetHire
        // 根据当前时间、周期，计算出失效期和价格
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoGetHireScriptFunction(
            XmlDocument readerdom,
            string strStartDate,
            string strPeriodName,
            out int nResultValue,
            out string strExpireDate,
            out string strPrice,
            out string strError)
        {
            strError = "";
            strExpireDate = "";
            strPrice = "";
            nResultValue = 0;

            string strFuncName = "GetHire";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public int GetHire(XmlDocument readerdom, string strStartDate, string strPeriodName, out string strExpireDate, out string strPrice, out string strError)函数，因此无法获得结果。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            // 执行函数
            try
            {
                object[] args = new object[6];
                args[0] = readerdom;
                args[1] = strStartDate;
                args[2] = strPeriodName;
                args[3] = strExpireDate;
                args[4] = strPrice;
                args[5] = strError;
                nResultValue = (int)mi.Invoke(host,
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.InvokeMethod,
                     null,
                     args,
                     null);

                // 取出out参数值
                strExpireDate = (string)args[3];
                strPrice = (string)args[4];
                strError = (string)args[5];
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + strFuncName + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 当前是否定义了脚本?
        // return:
        //      -1  定义了，但编译有错
        //      0   没有定义
        //      1   定义了
        public int HasScript(out string strError)
        {
            strError = "";

            _lockAssembly.EnterReadLock();
            try
            {
                if (string.IsNullOrEmpty(this.m_strAssemblyLibraryHostError) == false)
                {
                    strError = this.m_strAssemblyLibraryHostError;
                    return -1;
                }

                if (this.m_assemblyLibraryHost == null)
                {
                    strError = "未定义<script>脚本代码";
                    return 0;
                }

                return 1;
            }
            finally
            {
                _lockAssembly.ExitReadLock();
            }
        }

        // return:
        //      -1  出错
        //      0   Assembly 为空
        //      1   找到 Assembly
        internal int GetAssembly(
            string strStyle,
            out Assembly assembly,
            out string strError)
        {
            strError = "";
            assembly = null;

            _lockAssembly.EnterReadLock();
            try
            {
                if (string.IsNullOrEmpty(this.m_strAssemblyLibraryHostError) == false)
                {
                    strError = this.m_strAssemblyLibraryHostError;
                    return -1;
                }

                if (this.m_assemblyLibraryHost == null)
                {
                    if (StringUtil.IsInList("findBase", strStyle))
                    {
                        assembly = Assembly.GetExecutingAssembly();
                        return 1;
                    }
                    strError = "未定义<script>脚本代码";
                    return 0;
                }

                assembly = this.m_assemblyLibraryHost;
                return 1;
            }
            finally
            {
                _lockAssembly.ExitReadLock();
            }
        }

        // 执行脚本函数GetLost
        // 根据当前读者记录、实体记录、书目记录，计算出丢失后的赔偿金额
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoGetLostScriptFunction(
            SessionInfo sessioninfo,
            XmlDocument readerdom,
            XmlDocument itemdom,
            string strBiblioRecPath,
            out int nResultValue,
            out string strLostPrice,
            out string strReason,
            out string strError)
        {
            strError = "";
            strLostPrice = "";
            strReason = "";
            nResultValue = 0;

            string strFuncName = "GetLost";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public int GetLost(XmlDocument readerdom, XmlDocument itemdom, string strPriceCfgString, out string strLostPrice, out string strError)函数，因此无法获得结果。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // 执行函数
            try
            {
                object[] args = new object[6];
                args[0] = readerdom;
                args[1] = itemdom;
                args[2] = strBiblioRecPath;
                args[3] = strLostPrice;
                args[4] = strReason;
                args[5] = strError;
                nResultValue = (int)mi.Invoke(host,
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.InvokeMethod,
                     null,
                     args,
                     null);

                // 取出out参数值
                strLostPrice = (string)args[3];
                strReason = (string)args[4];
                strError = (string)args[5];
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + strFuncName + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数GetBiblioPart
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoGetBiblioPartScriptFunction(
            XmlDocument bibliodom,
            string strPartName,
            out int nResultValue,
            out string strResultValue,
            out string strError)
        {
            strError = "";
            strResultValue = "";
            nResultValue = 0;

            string strFuncName = "GetBiblioPart";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            Debug.Assert(assembly != null, "");

#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法执行脚本函数" + strFuncName + "()。";
                return -2;
            }

            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("GetBiblioPart");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供public int GetBiblioPart(XmlDocument bibliodom, string strPartName, out string strResultValue)函数，因此无法获得结果。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;

            // 执行函数
            try
            {
                object[] args = new object[3];
                args[0] = bibliodom;
                args[1] = strPartName;
                args[2] = strResultValue;
                nResultValue = (int)mi.Invoke(host,
                     BindingFlags.DeclaredOnly |
                     BindingFlags.Public | BindingFlags.NonPublic |
                     BindingFlags.Instance | BindingFlags.InvokeMethod,
                     null,
                     args,
                     null);

                // 取出out参数值
                strResultValue = (string)args[2];
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数'" + strFuncName + "'出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
        }

        // 执行脚本函数 VerifyItem
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoVerifyItemFunction(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("findBase",
        out Assembly assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法执行校验册记录功能。";
                return -2;
            }

            Debug.Assert(assembly != null, "");

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法校验册记录。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "(DoVerifyItemFunction) 创建 DigitalPlatform.LibraryServer.LibraryHost 类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // 执行函数
            try
            {
                return host.VerifyItem(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyItem" + "' 出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

#if NO
        // TODO: 脚本代码编译期间要锁定相关数据结构
        // 执行脚本函数 VerifyItem
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoVerifyItemFunction(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            // 基类代码不用脚本重载也足以运行 2017/4/23
#if NO
            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法校验册记录。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost == null ? Assembly.GetExecutingAssembly() : this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法校验册记录。";
                return -2;
            }

#if NO
            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("VerifyItem");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供int VerifyItem(string strAction, XmlDocument itemdom, out string strError)函数，因此无法校验册记录。";
                return -2;
            }
#endif

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "(DoVerifyItemFunction) 创建 DigitalPlatform.LibraryServer.LibraryHost 类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // 执行函数
            try
            {
#if NO
                    object[] args = new object[3];
                    args[0] = strAction;
                    args[1] = itemdom;
                    args[2] = strError;
                    nResultValue = (int)mi.Invoke(host,
                         BindingFlags.DeclaredOnly |
                         BindingFlags.Public | BindingFlags.NonPublic |
                         BindingFlags.Instance | BindingFlags.InvokeMethod,
                         null,
                         args,
                         null);

                    // 取出out参数值
                    strError = (string)args[2];
#endif
                return host.VerifyItem(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyItem" + "' 出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

#endif

        // 执行脚本函数 VerifyReader
        // parameters:
        // return:
        //      -3  条码号错误
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoVerifyReaderFunction(
            SessionInfo sessioninfo,
            string strAction,
            string strRecPath,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = GetAssembly("findBase",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法校验读者记录。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法校验册记录。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法校验条码号。";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建DigitalPlatform.LibraryServer.LibraryHost类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // 执行函数
            try
            {
                return host.VerifyReader(
                    sessioninfo,
                    strAction,
                    strRecPath,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyReader" + "' 出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
        }

        // 判断一个字符串是否符合个人书斋地点名称的形态特征
        public static bool IsPersonalLibraryRoom(string strRoom)
        {
            if (string.IsNullOrEmpty(strRoom) == false
    && strRoom[0] == '~')
                return true;
            return false;
        }
    }

    public class LibraryHost
    {
        public LibraryApplication App = null;

        public SessionInfo SessionInfo = null;  // 2010/12/16

        public LibraryHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        // return:
        //      -3  条码号错误
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyReader(
            SessionInfo sessioninfo,
            string strAction,
            string strRecPath,
            XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strNewBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");

            if (strAction == "new"
|| strAction == "change"
|| strAction == "changereaderbarcode"
|| strAction == "move")
            {
                if (string.IsNullOrEmpty(strNewBarcode) == false)
                {
                    // 2017/5/4
                    if (this.App.UpperCaseReaderBarcode)
                    {
                        if (strNewBarcode.ToUpper() != strNewBarcode)
                        {
                            strError = "读者证条码号 '" + strNewBarcode + "' 中的字母应为大写";
                            return 1;
                        }
                    }

                    string strDbName = ResPath.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "从读者库记录路径 '" + strRecPath + "' 获得数据库名时出错。验证读者记录失败";
                        return -1;
                    }

                    string strLibraryCode = "";
                    if (this.App.IsReaderDbName(strDbName, out strLibraryCode) == false)
                    {
                        strError = "数据库名 '" + strDbName + "' 不是读者库。验证读者记录失败";
                        return -1;
                    }

                    // return:
                    //      -2  校验函数不打算对这个分馆的号码进行校验
                    //      -1  调用出错
                    //      0   校验正确
                    //      1   校验发现错误
                    nRet = VerifyPatronBarcode(strLibraryCode, strNewBarcode, out strError);
                    if (nRet != 0 && nRet != -2)
                    {
                        if (nRet == 1)
                            return -3;
                        return nRet;
                    }
                }
            }

            string strPersonalLibrary = DomUtil.GetElementText(readerdom.DocumentElement, "personalLibrary");

            // 检查个人书斋名
            if (strAction == "new"
|| strAction == "change"
|| strAction == "changereaderbarcode"
|| strAction == "move")
            {
                // 注：个人书斋名称，实际上允许 ~xxxx 方式，也允许 xxxxx 方式。后者是班级书架管理方式所需要的
#if NO
                if (string.IsNullOrEmpty(strPersonalLibrary) == false
                    && strPersonalLibrary[0] != '*'
                    && strPersonalLibrary[0] != '~')
                {
                    // TODO: 注意普通馆藏地点字符串中的地点名字的第一字符不能为 '~'
                    strError = "个人书斋名 '"+strPersonalLibrary+"' 不合法。第一字符必须为 '~' 或者 '*'";
                    return 1;
                }
#endif
            }

            if (sessioninfo.UserID != "~replication")   // 2017/2/21
            {
                string strRights = DomUtil.GetElementText(readerdom.DocumentElement, "rights");

                // 检查读者权限。要求不能大于当前用户的权限
                List<string> warning_rights = null;
                if (IsLessOrEqualThan(strRights, sessioninfo.Rights, out warning_rights) == false)
                {
                    strError = "读者记录中的权限超出了当前用户的权限，这是不允许的。超出的部分权限值 '" + StringUtil.MakePathList(warning_rights) + "'";
                    return 1;
                }
            }

            // 2016/4/11
            // 检查 access 元素里面的星号
            string strAccess = DomUtil.GetElementText(readerdom.DocumentElement, "access");
            if (strAccess != null && strAccess.Trim() == "*")
            {
                strError = "读者记录中的存取定义(access 元素值)不允许使用 * 形态";
                return -1;
            }

            return 0;
        }

        // 2016/4/3
        // 按照缺省行为，验证读者记录中的证条码号
        // return:
        //      -2  校验函数不打算对这个分馆的号码进行校验
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public int VerifyPatronBarcode(
            string strLibraryCode,
            string strNewBarcode,
            out string strError)
        {
            strError = "";

            // 验证条码号
            if (this.App.VerifyBarcode == true)
            {
                // return:
                //	0	invalid barcode
                //	1	is valid reader barcode
                //	2	is valid item barcode
                int nResultValue = 0;

                // return:
                //      -2  not found script
                //      -1  出错
                //      0   成功
                int nRet = this.App.DoVerifyBarcodeScriptFunction(
                    this,
                    strLibraryCode,
                    strNewBarcode,
                    out nResultValue,
                    out strError);
                if (nRet == -2 || nRet == -1 || nResultValue != 1)
                {
                    if (nRet == -2)
                    {
                        strError = "library.xml 中没有配置条码号验证函数，无法进行条码号验证";
                        return -1;
                    }
                    else if (nRet == -1)
                    {
                        strError = "验证册条码号的过程中出错"
                           + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                        return -1;
                    }
                    else if (nResultValue == -2)
                        return -2;  // 2016/12/20
                    else if (nResultValue != 1)
                    {
                        strError = "条码号 '" + strNewBarcode + "' 经验证发现不是一个合法的证条码号"
                           + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                    }

                    return 1;
                }
            }
            return 0;
        }

        // strLeft 包含的权限是否小于等于 strRight
        static bool IsLessOrEqualThan(string strLeft,
            string strRight,
            out List<string> warning_rights)
        {
            warning_rights = new List<string>();

            if (string.IsNullOrEmpty(strLeft) && string.IsNullOrEmpty(strRight))
                return true;
            if (string.IsNullOrEmpty(strLeft))
                return true;   // strLeft == 空 && strRight != 空
            if (string.IsNullOrEmpty(strRight))
            {
                warning_rights = StringUtil.SplitList(strRight);
                return false;   // strLeft != 空 && strRight == 空
            }

            string[] left = strLeft.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] right = strRight.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in left)
            {
                if (string.IsNullOrEmpty(s) == false
                    && s[0] == '_')
                    continue;

                if (Array.IndexOf<string>(right, s) == -1)
                {
                    if (StringUtil.HasHead(s, "level-") == true)
                    {
                        if (LibraryApplication.HasLevel(s, right) == true)
                            continue;
                    }

                    warning_rights.Add(s);
                }
            }

            if (warning_rights.Count > 0)
                return false;

            return true;
        }

        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyItem(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> errors = new List<string>();

            string strNewBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);
            // 解析
            LibraryApplication.ParseCalendarName(strLocation,
        out string strLibraryCode,
        out string strRoom);

            // 检查来自 location 元素中的馆代码部分
            {

            }

            // 去除 strRoom 内容中横杠或者冒号以后的部分。例如 “现刊阅览室-综合355”
            // 注：横杠以后的部分表示架号，统计时会忽略；冒号后面的部分表示班级书架名称，统计时不会被忽略
            {
                List<string> parts = StringUtil.ParseTwoPart(strRoom, new string[] { "-", ":" });
                strRoom = parts[0];
            }

            XmlElement item = this.App.GetLocationItemElement(
    strLibraryCode,
    strRoom);

            // 检查馆藏地点字符串
            if (strAction == "new"
|| strAction == "change"
|| strAction == "transfer"
|| strAction == "move")
            {
                if (item == null)
                {
                    strError = "馆代码 '" + strLibraryCode + "' 没有定义馆藏地点 '" + strRoom + "'(根据 <locationTypes> 定义)";
                    // return 1;
                    errors.Add(strError);
                }
            }

            // 2014/1/10
            // 检查空条码号
            if ((strAction == "new"
|| strAction == "change"
|| strAction == "move")       // delete操作不检查
&& String.IsNullOrEmpty(strNewBarcode) == true)
            {
#if NO
                XmlElement item = this.App.GetLocationItemElement(
                    strLibraryCode,
                    strRoom);
#endif
                if (item != null)
                {
                    bool bNullable = DomUtil.GetBooleanParam(item, "itemBarcodeNullable", true);
                    if (bNullable == false)
                    {
                        strError = "册条码号不允许为空(根据馆藏地 '" + strLocation + "' 的 <locationTypes> 定义)";
                        // return 1;
                        errors.Add(strError);
                    }
                }
                else
                {
                    if (this.App.AcceptBlankItemBarcode == false)
                    {
                        strError = "册条码号不能为空(根据 AcceptBlankItemBarcode 定义)";
                        // return 1;
                        errors.Add(strError);
                    }
                }

            }

            if (string.IsNullOrEmpty(strNewBarcode) == false)
            {
                // 2017/5/4
                if (this.App.UpperCaseItemBarcode)
                {
                    if (strNewBarcode.ToUpper() != strNewBarcode)
                    {
                        strError = "册条码号 '" + strNewBarcode + "' 中的字母应为大写";
                        // return 1;
                        errors.Add(strError);
                    }
                }

                // return:
                //      -2  校验函数不打算对这个分馆的号码进行校验
                //      -1  调用出错
                //      0   校验正确
                //      1   校验发现错误
                nRet = VerifyItemBarcode(
                    App.BarcodeValidation ? strLocation : strLibraryCode,
                    strNewBarcode,
                    out strError);
                if (nRet != 0 && nRet != -2)
                {
                    // return nRet;
                    if (nRet == -1)
                        return -1;
                    errors.Add(strError);
                }
            }

            // 检查价格字符串
            if (strAction == "new"
|| strAction == "change"
|| strAction == "move")
            {
                // 2014/11/28
                string strPrice = DomUtil.GetElementText(itemdom.DocumentElement, "price");
                if (string.IsNullOrEmpty(strPrice) == false)
                {
                    // return:
                    //      -1  调用出错
                    //      0   校验正确
                    //      1   校验发现错误
                    nRet = VerifyItemPrice(strLibraryCode, strPrice, out strError);
                    if (nRet != 0)
                    {
                        // return nRet;
                        if (nRet == -1)
                            return -1;
                        errors.Add(strError);
                    }
                }
            }

            // 2015/7/10
            // 检查索取号字符串
            if (strAction == "new"
|| strAction == "change"
|| strAction == "move")
            {
                string strAccessNo = DomUtil.GetElementText(itemdom.DocumentElement, "accessNo");
                if (string.IsNullOrEmpty(strAccessNo) == false)
                {
                    if (StringUtil.HasHead(strAccessNo, "@accessNo") == true)
                    {
                        strError = "索取号字符串中的宏尚未兑现";
                        // return 1;
                        errors.Add(strError);
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return 1;
            }

            return 0;
        }

        // 按照缺省行为，验证价格字符串
        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyItemPrice(string strLibraryCode,
            string strPrice,
            out string strError)
        {
            strError = "";

            strPrice = StringUtil.ToDBC(strPrice);
            if (strPrice.IndexOfAny(new char[] { '(', ')' }) != -1)
            {
                strError = "价格字符串中不允许出现括号 '" + strPrice + "'";
                return 1;
            }

            if (strPrice.IndexOf(',') != -1)
            {
                strError = "价格字符串中不允许出现逗号 '" + strPrice + "'";
                return 1;
            }

            CurrencyItem item = null;
            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out item,
                out strError);
            if (nRet == -1)
                return 1;

            return 0;
        }

        // 按照缺省行为，验证册记录中的册条码号
        // return:
        //      -2  VerifyBarcode() 函数中返回 -2，表示这个分馆不打算进行校验
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public int VerifyItemBarcode(
            string strLibraryCode,
            string strNewBarcode,
            out string strError)
        {
            strError = "";
            // 验证条码号
            if (this.App.VerifyBarcode == true)
            {
                // return:
                //	0	invalid barcode
                //	1	is valid reader barcode
                //	2	is valid item barcode
                int nResultValue = 0;

                // return:
                //      -2  not found script
                //      -1  出错
                //      0   成功
                int nRet = this.App.DoVerifyBarcodeScriptFunction(
                    this,
                    strLibraryCode,
                    strNewBarcode,
                    out nResultValue,
                    out strError);
                if (nRet == -2 || nRet == -1 || nResultValue != 2)
                {
                    if (nRet == -2)
                    {
                        strError = "library.xml 中没有配置条码号验证函数，无法进行条码号验证";
                        return -1;
                    }
                    else if (nRet == -1)
                    {
                        strError = "验证册条码号的过程中出错"
                           + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                        return -1;
                    }
                    else if (nResultValue == -2)
                        return -2;  // 2016/12/20
                    else if (nResultValue != 2)
                    {
                        strError = "条码号 '" + strNewBarcode + "' 经验证发现不是一个合法的册条码号"
                           + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                    }

                    return 1;
                }
            }
            return 0;
        }

        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyOrder(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            string strOrderTime = DomUtil.GetElementText(itemdom.DocumentElement, "orderTime");

            if (string.IsNullOrEmpty(strOrderTime) == false)
            {
                try
                {
                    DateTimeUtil.FromRfc1123DateTimeString(strOrderTime);
                }
                catch (Exception ex)
                {
                    strError = "订购日期字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
                    return 1;
                }
            }

            string strRange = DomUtil.GetElementText(itemdom.DocumentElement, "range");

            if (string.IsNullOrEmpty(strRange) == false)
            {
                // TODO: 如果是图书类型的订购记录，要允许 range 使用开放式的时间范围

                // 检查单个出版日期字符串是否合法
                // return:
                //      -1  出错
                //      0   正确
                int nRet = LibraryServerUtil.CheckPublishTimeRange(strRange,
                    true,   // TODO: 期刊要用 false
                    out strError);
                if (nRet == -1)
                {
                    strError = "时间范围字符串 '" + strRange + "' 格式错误: " + strError;
                    return 1;
                }
            }

            {
                string strIssueCount = DomUtil.GetElementText(itemdom.DocumentElement, "issueCount");

                if (string.IsNullOrEmpty(strIssueCount) == false)
                {
                    if (Int32.TryParse(strIssueCount, out int value) == false)
                    {
                        strError = "期数 '" + strIssueCount + "' 不合法。应为正整数";
                        return 1;
                    }
                }
            }

            {
                // 检查 discount 元素是否合法。为 0.80[0.90] 这样的形态
                string strDiscount = DomUtil.GetElementText(itemdom.DocumentElement, "discount");

                if (string.IsNullOrEmpty(strDiscount) == false)
                {
                    OldNewValue discount = OldNewValue.Parse(strDiscount);
                    if (string.IsNullOrEmpty(discount.OldValue) == false
                        && decimal.TryParse(discount.OldValue, out decimal value) == false)
                    {
                        strError = "折扣字符串 '" + strDiscount + "' 中左边部分 '" + discount.OldValue + "' 不合法。应为一个小数";
                        return -1;
                    }
                    if (string.IsNullOrEmpty(discount.NewValue) == false
        && decimal.TryParse(discount.NewValue, out value) == false)
                    {
                        strError = "折扣字符串 '" + strDiscount + "' 中右边部分 '" + discount.NewValue + "' 不合法。应为一个小数";
                        return -1;
                    }
                }
            }

            {
                // 检查 fixedPrice 字段值是否合法
                string strFixedPrice = DomUtil.GetElementText(itemdom.DocumentElement, "fixedPrice");

                if (string.IsNullOrEmpty(strFixedPrice) == false)
                {
                    string strPosition = "码洋字段";
                    // 检查订购价字段内容是否合法
                    // return:
                    //      -1  校验过程出错
                    //      0   校验正确
                    //      1   校验发现错误
                    int nRet = dp2StringUtil.VerifyOrderPriceField(strFixedPrice, out strError);
                    if (nRet == -1)
                    {
                        strError = strPosition + ": " + strError;
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        strError = strPosition + ": " + strError;
                        return 1;
                    }
                }
            }

            {
                // 检查 price 字段值是否合法
                string strPrice = DomUtil.GetElementText(itemdom.DocumentElement, "price");

                if (string.IsNullOrEmpty(strPrice) == false)
                {
                    string strPosition = "单价字段";
                    // 检查订购价字段内容是否合法
                    // return:
                    //      -1  校验过程出错
                    //      0   校验正确
                    //      1   校验发现错误
                    int nRet = dp2StringUtil.VerifyOrderPriceField(strPrice, out strError);
                    if (nRet == -1)
                    {
                        strError = strPosition + ": " + strError;
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        strError = strPosition + ": " + strError;
                        return 1;
                    }
                }
            }

            {
                // 检查 totalPrice 字段值是否合法
                string strTotalPrice = DomUtil.GetElementText(itemdom.DocumentElement, "totalPrice");

                if (string.IsNullOrEmpty(strTotalPrice) == false)
                {
                    string strPosition = "总价字段";
                    // 检查订购价字段内容是否合法
                    // return:
                    //      -1  校验过程出错
                    //      0   校验正确
                    //      1   校验发现错误
                    int nRet = dp2StringUtil.VerifyOrderPriceField(strTotalPrice, out strError);
                    if (nRet == -1)
                    {
                        strError = strPosition + ": " + strError;
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        strError = strPosition + ": " + strError;
                        return 1;
                    }
                }
            }

            {
                // 检查 copy 字段值是否合法
                string strCopy = DomUtil.GetElementText(itemdom.DocumentElement, "copy");

                if (string.IsNullOrEmpty(strCopy) == false)
                {
                    string strPosition = "复本字段";
                    // 检查订购价字段内容是否合法
                    // return:
                    //      -1  校验过程出错
                    //      0   校验正确
                    //      1   校验发现错误
                    int nRet = dp2StringUtil.VerifyOrderCopyField(strCopy, out strError);
                    if (nRet == -1)
                    {
                        strError = strPosition + ": " + strError;
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        strError = strPosition + ": " + strError;
                        return 1;
                    }
                }
            }

            {
                // 检查 distribute 字段是否合法
                // 验证馆藏分配字符串
                string strDistribute = DomUtil.GetElementText(itemdom.DocumentElement, "distribute");

                if (string.IsNullOrEmpty(strDistribute) == false)
                {
                    LocationCollection locations = new LocationCollection();
                    int nRet = locations.Build(strDistribute, out strError);
                    if (nRet == -1)
                    {
                        strError = "馆藏分配字符串 '" + strDistribute + "' 格式错误: " + strError;
                        return 1;
                    }
                }
            }

            // 检查几个字段值之间的相互运算关系

            return 0;
        }

        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyIssue(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            List<string> errors = new List<string>();

            // 2018/8/16
            string strPublishTime = DomUtil.GetElementText(itemdom.DocumentElement, "publishTime");
            if (string.IsNullOrEmpty(strPublishTime))
            {
                strError = "出版日期字段为空";
                errors.Add(strError);
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return 1;
            }

            return 0;
        }

        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyComment(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            return 0;
        }


        /*
        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // new一个Host派生对象
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                this,
                null);

        }*/

#if NO
        // 以前的版本，不能实现超期前提醒
        // retun:
        //      -1  出错
        //      0   没有必要发送
        //      1   需要发送
        public virtual int NotifyReader(
            XmlDocument readerdom,
            Calendar calendar,
            List<string> notifiedBarcodes,
            string strBodyType,
            // out int nResultValue,
            out string strBody,
            out string strMime,
            out List<string> wantNotifyBarcodes,
            out string strError)
        {
            strBody = "";
            strError = "";
            wantNotifyBarcodes = new List<string>();
            strMime = "html";
            int nRet = 0;

            string strResult = "";
            int nOverdueCount = 0;

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strLink = "<LINK href='" + App.OpacServerUrl + "/readerhtml.css' type='text/css' rel='stylesheet'>";
            strResult = "<html><head>" + strLink + "</head><body>";

            // 借阅的册
            strResult += "<br/>借阅信息<br/>";
            strResult += "<table class='borrowinfo' width='100%' cellspacing='1' cellpadding='4'>";
            strResult += "<tr class='columntitle'><td nowrap>册条码号</td><td nowrap>续借次</td><td nowrap>借阅日期</td><td nowrap>期限</td><td nowrap>操作者</td><td nowrap>是否超期</td><td nowrap>备注</td></tr>";
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string strOverDue = "";
                bool bOverdue = false;
 
                // 检查超期情况。
                // return:
                //      -1  数据格式错误
                //      0   没有发现超期
                //      1   发现超期   strError中有提示信息
                //      2   已经在宽限期内，很容易超期 2009/3/13
                nRet = App.CheckPeriod(
                    calendar,
                    strBorrowDate,
                    strPeriod,
                    out strError);
                if (nRet == -1)
                    strOverDue = strError;
                else if (nRet == 1)
                {
                    strOverDue = strError;	// "已超期";
                    bOverdue = true;
                }
                else
                    strOverDue = strError;	// 可能也有一些必要的信息，例如非工作日

                string strColor = "bgcolor=#ffffff";

                if (bOverdue == true)
                {
                    strColor = "bgcolor=#ff9999";	// 超期

                    nOverdueCount ++;

                    // 看看是不是已经通知过
                    if (notifiedBarcodes.IndexOf(strBarcode) == -1)
                    {
                        wantNotifyBarcodes.Add(strBarcode);
                    }
                }

                string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strBarcode + "&target='_blank'>" + strBarcode + "</a>";

                strResult += "<tr class='content' " + strColor + " nowrap>";
                strResult += "<td class='barcode' nowrap>" + strBarcodeLink + "</td>";
                strResult += "<td class='no' nowrap align='right'>" + strNo + "</td>";
                strResult += "<td class='borrowdate' >" + DateTimeUtil.LocalTime(strBorrowDate) + "</td>";
                strResult += "<td class='period' nowrap>" + strPeriod + "</td>";
                strResult += "<td class='operator' nowrap>" + strOperator + "</td>";
                strResult += "<td class='overdue' >" + strOverDue + "</td>";
                strResult += "<td class='renewcomment' nowrap>" + strRenewComment.Replace(";", "<br/>") + "</td>";
                strResult += "</tr>";
            }

            strResult += "</table>";

            strResult += "<p>";
            strResult += "有 " + nOverdueCount.ToString()+ " 册图书超期, 请尽快归还。";
            strResult += "</p>";

            strResult += "</body></html>";

            strBody = strResult;

            if (wantNotifyBarcodes.Count > 0)
                return 1;
            else
                return 0;
        }

#endif
        // 超期提醒
        // 新版本，可以处理超期前提醒
        // retun:
        //      -1  出错
        //      0   没有必要发送
        //      1   需要发送
        public virtual int NotifyReader(
            XmlDocument readerdom,
            Calendar calendar,
            // List<string> notifiedBarcodes,
            string strBodyType,
            out string strBody,
            out string strMime,
            // out List<string> wantNotifyBarcodes,
            out string strError)
        {
            if (strBodyType == "sms")
            {
                return NotifyReaderSMS(
                    readerdom,
                    calendar,
                    //notifiedBarcodes,
                    strBodyType,
                    out strBody,
                    out strMime,
                    //out wantNotifyBarcodes,
                    out strError);
            }

            if (strBodyType == "mq")
            {
                return NotifyReaderMQ(
                    readerdom,
                    calendar,
                    strBodyType,
                    out strBody,
                    out strMime,
                    out strError);
            }

            strBody = "";
            strError = "";
            // wantNotifyBarcodes = new List<string>();
            strMime = "html";
            int nRet = 0;

            string strResult = "";
            // int nNotifyCount = 0;   // 需要通知的事项 nNotifyCount = nOverduCount + nNormalCount
            int nOverdueCount = 0;  // 超期提醒的事项数
            int nNormalCount = 0;   // 一般提醒的事项数

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strLink = "<LINK href='" + App.OpacServerUrl + "/readerhtml.css' type='text/css' rel='stylesheet'>";
            strResult = "<html><head>" + strLink + "</head><body>";

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
    "name");
            strResult += "尊敬的 " + strName + " 您好！<br/><br/>您在图书馆借阅的下列图书：";

            // 借阅的册
            // strResult += "<br/>借阅信息<br/>";
            strResult += "<table class='borrowinfo' width='100%' cellspacing='1' cellpadding='4'>";
            strResult += "<tr class='columntitle'>"
                + "<td nowrap>册条码号</td>"
                + "<td class='no' nowrap align='right'>续借次</td>"
                + "<td nowrap>借阅日期</td>"
                + "<td nowrap>期限</td>"
                + "<td nowrap>应还日期</td>"
                //+ "<td nowrap>操作者</td>"
                + "<td nowrap>超期情况</td>"
                //+ "<td nowrap>备注</td>"
                + "</tr>";
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                string strOverDue = "";
                bool bOverdue = false;
                DateTime timeReturning = DateTime.MinValue;
                {

                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = this.App.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        if (nRet == 1)
                        {
                            bOverdue = true;
                            strOverDue = string.Format(this.App.GetString("已超期s"),  // 已超期 {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit));
                        }
                    }
                }

                string strColor = "bgcolor=#ffffff";

                string strChars = "";
                // 获得一种 body type 的全部通知字符
                strChars = ReadersMonitor.GetNotifiedChars(App,
                    strBodyType,
                    strHistory);

                if (bOverdue == true)
                {
                    strColor = "bgcolor=#ff9999";	// 超期

                    // 看看是不是已经通知过
                    if (string.IsNullOrEmpty(strChars) == false && strChars[0] == 'y')
                        continue;

                    // 合并设置一种 body type 的全部通知字符
                    // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                    nRet = ReadersMonitor.SetNotifiedChars(App,
                        strBodyType,
                        "y",
                        ref strHistory,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    ReadersMonitor.SetChar(ref strChars,
                        0,
                        'y',
                        'n');

                    // nNotifyCount++;
                    nOverdueCount++;
                }
                else if (string.IsNullOrEmpty(App.NotifyDef) == false)
                {
                    // 检查超期前的通知点

                    List<int> indices = null;
                    // 检查每个通知点，返回当前时间已经达到或者超过了通知点的那些检查点的下标
                    // return:
                    //      -1  数据格式错误
                    //      0   成功
                    nRet = App.CheckNotifyPoint(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        App.NotifyDef,
                        out indices,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 偏移量 0 让给了超期通知
                    for (int k = 0; k < indices.Count; k++)
                    {
                        indices[k] = indices[k] + 1;
                    }

                    // 检查是否至少有一个字符位置为 ch 代表的值
                    if (CheckChar(strChars, indices, 'n', 'n') == true)
                    {
                        // 至少有一个检查点尚未通知
                        strOverDue = "即将到期";
                    }
                    else
                        continue;

                    foreach (int index in indices)
                    {
                        ReadersMonitor.SetChar(ref strChars,
                            index,
                            'y',
                            'n');
                    }

                    nNormalCount++;
                    // nNotifyCount++;
                }
                else
                    continue;

                // 合并设置一种 body type 的全部通知字符
                // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                nRet = ReadersMonitor.SetNotifiedChars(App,
                    strBodyType,
                    strChars,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                    return -1;
                DomUtil.SetAttr(node, "notifyHistory", strHistory);

                string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strBarcode + "&target='_blank'>" + strBarcode + "</a>";

                strResult += "<tr class='content' " + strColor + " nowrap>";
                strResult += "<td class='barcode' nowrap>" + strBarcodeLink + "</td>";
                strResult += "<td class='no' nowrap align='right'>" + HttpUtility.HtmlEncode(strNo) + "</td>";
                strResult += "<td class='borrowdate' >" + HttpUtility.HtmlEncode(DateTimeUtil.LocalTime(strBorrowDate)) + "</td>";
                strResult += "<td class='period' nowrap>" + HttpUtility.HtmlEncode(App.GetDisplayTimePeriodStringEx(strPeriod)) + "</td>";
                strResult += "<td class='returningdate' >" + HttpUtility.HtmlEncode(timeReturning.ToString("d")) + "</td>";
                // strResult += "<td class='operator' nowrap>" + HttpUtility.HtmlEncode(strOperator) + "</td>";
                strResult += "<td class='overdue' >" + HttpUtility.HtmlEncode(strOverDue) + "</td>";
                // strResult += "<td class='renewcomment' nowrap>" + strRenewComment.Replace(";", "<br/>") + "</td>";
                strResult += "</tr>";
            }

            strResult += "</table>";

            if (nOverdueCount > 0)
            {
                strResult += "<p>";
                strResult += "有 " + nOverdueCount.ToString() + " 册图书已经超期, 请尽快归还。";
                strResult += "</p>";
            }

            if (nNormalCount > 0)
            {
                strResult += "<p>";
                strResult += "有 " + nNormalCount.ToString() + " 册图书即将到期, 请注意在期限内归还。";
                strResult += "</p>";
            }

            strResult += "</body></html>";

            strBody = strResult;

            if (nOverdueCount + nNormalCount > 0)
                return 1;
            else
                return 0;
        }

        // 检查是否至少有一个字符位置为 ch 代表的值
        public static bool CheckChar(string strText,
            List<int> indices,
            char ch,
            char chDefault)
        {
            foreach (int index in indices)
            {
                if (strText.Length < index + 1)
                {
                    // 超过范围的字符被当作 chDefault
                    if (ch == chDefault)
                        return true;
                    continue;
                }
                if (strText[index] == ch)
                    return true;
            }

            return false;
        }

        // 在指定的下标位置设置字符
        public static bool SetChars(ref string strText,
            List<int> indices,
            char ch)
        {


            return false;
        }

        // 2016/4/25
        /*
<root>
        <type>超期通知</type>
        <record>
         ... 读者记录 XML 原始形态
        </record>
        <items overdueCount=已经到期册数 normalCount=即将到期册数 >
            <item 
                barcode='...' 
                location='...' 
                refID='...' 
                summary='书目摘要' 
                borrowDate='借书日期' 
                borrowPeriod='期限' 
                timeReturning='应还时间' 
                overdue='超期情况描述' 
                overdueType='overdue/warning'>
        <items/>
        <text>纯文本描述</text>
<root/>
         * */
        // MQ 通知读者超期的版本。供 NotifyReader() 的重载版本必要时引用
        public int NotifyReaderMQ(
            XmlDocument readerdom,
            Calendar calendar,
            string strBodyType,
            out string strBody,
            out string strMime,
            out string strError)
        {
            strBody = "";
            strError = "";
            strMime = "xml";
            int nRet = 0;

            string strResult = "";
            int nOverdueCount = 0;  // 超期提醒的事项数
            int nNormalCount = 0;   // 一般提醒的事项数

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            // 表达通知信息的 XML 记录
            XmlDocument output_dom = new XmlDocument();
            output_dom.LoadXml("<root />");

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");

            DomUtil.SetElementText(output_dom.DocumentElement, "type", "超期通知");
            XmlElement items = output_dom.CreateElement("items");
            output_dom.DocumentElement.AppendChild(items);

            string strRights = DomUtil.GetElementText(readerdom.DocumentElement, "rights");
            bool bTestNotify = (StringUtil.IsInList("_testoverduenotify", strRights) == true);

            strResult += "您借阅的下列书刊：\n";

            // 借阅的册
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strLocation = StringUtil.GetPureLocation(DomUtil.GetAttr(node, "location"));
                string strRefID = DomUtil.GetAttr(node, "refID");

                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                // string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                // string strOperator = DomUtil.GetAttr(node, "operator");
                // string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                string strOverDue = "";
                bool bOverdue = false;  // 是否超期
                DateTime timeReturning = DateTime.MinValue;
                {
                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    if (bTestNotify == true)
                    {
                        timeReturning = DateTime.Now;
                        timeNextWorkingDay = DateTime.Now;
                        long lValue = 0;
                        LibraryApplication.ParsePeriodUnit(strPeriod,
                            out lValue,
                            out strPeriodUnit,
                            out strError);
                        lOver = lValue;
                        nRet = 1;
                    }
                    else
                    {
                        // 获得还书日期
                        // return:
                        //      -1  数据格式错误
                        //      0   没有发现超期
                        //      1   发现超期   strError中有提示信息
                        //      2   已经在宽限期内，很容易超期 
                        nRet = this.App.GetReturningTime(
                            calendar,
                            strBorrowDate,
                            strPeriod,
                            out timeReturning,
                            out timeNextWorkingDay,
                            out lOver,
                            out strPeriodUnit,
                            out strError);
                    }
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        if (nRet == 1)
                        {
                            bOverdue = true;
                            strOverDue = string.Format(this.App.GetString("已超期s"),  // 已超期 {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit));
                        }
                    }
                }

                string strChars = "";
                // 获得一种 body type 的全部通知字符
                strChars = ReadersMonitor.GetNotifiedChars(App,
                    strBodyType,
                    strHistory);

                if (bOverdue == true)
                {
                    // 看看是不是已经通知过
                    if (string.IsNullOrEmpty(strChars) == false && strChars[0] == 'y'
                        && bTestNotify == false)
                        continue;

                    // 合并设置一种 body type 的全部通知字符
                    // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                    nRet = ReadersMonitor.SetNotifiedChars(App,
                        strBodyType,
                        "y",
                        ref strHistory,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    ReadersMonitor.SetChar(ref strChars,
                        0,
                        'y',
                        'n');

                    nOverdueCount++;
                }
                else if (string.IsNullOrEmpty(App.NotifyDef) == false)
                {
                    // 检查超期前的通知点

                    List<int> indices = null;
                    // 检查每个通知点，返回当前时间已经达到或者超过了通知点的那些检查点的下标
                    // return:
                    //      -1  数据格式错误
                    //      0   成功
                    nRet = App.CheckNotifyPoint(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        App.NotifyDef,
                        out indices,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 偏移量 0 让给了超期通知
                    for (int k = 0; k < indices.Count; k++)
                    {
                        indices[k] = indices[k] + 1;
                    }

                    // 检查是否至少有一个字符位置为 ch 代表的值
                    if (CheckChar(strChars, indices, 'n', 'n') == true)
                    {
                        // 至少有一个检查点尚未通知
                        strOverDue = "即将到期";
                    }
                    else
                        continue;

                    foreach (int index in indices)
                    {
                        ReadersMonitor.SetChar(ref strChars,
                            index,
                            'y',
                            'n');
                    }

                    nNormalCount++;
                }
                else
                    continue;

                // 合并设置一种 body type 的全部通知字符
                // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                nRet = ReadersMonitor.SetNotifiedChars(App,
                    strBodyType,
                    strChars,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                    return -1;
                DomUtil.SetAttr(node, "notifyHistory", strHistory);

                // 获得图书摘要信息
                string strSummary = "";
                string strBiblioRecPath = "";
                nRet = this.App.GetBiblioSummary(strBarcode,
                    strConfirmItemRecPath,
                    null,   //  strBiblioRecPathExclude,
                    -1, // 25,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                if (nRet == -1)
                {
                    strSummary = "ERROR: " + strError;
                }

                // strResult += (i + 1).ToString() + ") " 
                strResult += strSummary + " ";
                // strResult += "借阅日期: " + DateTimeUtil.LocalDate(strBorrowDate) + " ";
                strResult += "应还日期: " + timeReturning.ToString("d") + " ";
                strResult += strOverDue + "\n";

                {
                    XmlElement item = output_dom.CreateElement("item");
                    items.AppendChild(item);

                    item.SetAttribute("barcode", strBarcode);
                    item.SetAttribute("location", strLocation); // 2016/9/5
                    item.SetAttribute("refID", strRefID);
                    item.SetAttribute("summary", strSummary);
                    item.SetAttribute("borrowDate", strBorrowDate); // 2016/9/5
                    item.SetAttribute("borrowPeriod", strPeriod);   // 2016/9/5
                    item.SetAttribute("timeReturning", timeReturning.ToString("d"));
                    item.SetAttribute("overdue", strOverDue);
                    if (bOverdue)
                        item.SetAttribute("overdueType", "overdue");
                    else
                        item.SetAttribute("overdueType", "warning");
                }
            }

            /*
            if (nOverdueCount > 0)
                strResult += "=== 共有 " + nOverdueCount.ToString() + " 册图书超期, 请尽快归还。";
            if (nNormalCount > 0)
                strResult += "=== 共有 " + nNormalCount.ToString() + " 册图书即将到期, 请注意在期限内归还。";
             * */
            items.SetAttribute("overdueCount", nOverdueCount.ToString());
            items.SetAttribute("normalCount", nNormalCount.ToString());

            DomUtil.SetElementText(output_dom.DocumentElement, "text", strResult);

            {
                XmlElement record = output_dom.CreateElement("patronRecord");
                output_dom.DocumentElement.AppendChild(record);
                record.InnerXml = readerdom.DocumentElement.InnerXml;

                DomUtil.DeleteElement(record, "borrowHistory");
                DomUtil.DeleteElement(record, "password");
                DomUtil.DeleteElement(record, "fingerprint");
                DomUtil.DeleteElement(record, "face");
                // TODO: 是否包含 libraryCode 元素?
            }

            strBody = output_dom.DocumentElement.OuterXml;

            if (nOverdueCount + nNormalCount > 0)
                return 1;
            else
                return 0;
        }

        // 短消息通知读者超期的版本。供NotifyReader()的重载版本必要时引用
        public int NotifyReaderSMS(
            XmlDocument readerdom,
            Calendar calendar,
            // List<string> notifiedBarcodes,
            string strBodyType,
            out string strBody,
            out string strMime,
            // out List<string> wantNotifyBarcodes,
            out string strError)
        {
            strBody = "";
            strError = "";
            // wantNotifyBarcodes = new List<string>();
            strMime = "text";
            int nRet = 0;

            string strResult = "";
            int nOverdueCount = 0;  // 超期提醒的事项数
            int nNormalCount = 0;   // 一般提醒的事项数

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");
            strResult += "您借阅的下列书刊：\n";

            // 借阅的册
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                // string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                // string strOperator = DomUtil.GetAttr(node, "operator");
                // string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                string strOverDue = "";
                bool bOverdue = false;  // 是否超期
                DateTime timeReturning = DateTime.MinValue;
                {
                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = this.App.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        if (nRet == 1)
                        {
                            bOverdue = true;
                            strOverDue = string.Format(this.App.GetString("已超期s"),  // 已超期 {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit));
                        }
                    }
                }

                string strChars = "";
                // 获得一种 body type 的全部通知字符
                strChars = ReadersMonitor.GetNotifiedChars(App,
                    strBodyType,
                    strHistory);

                if (bOverdue == true)
                {
                    // 看看是不是已经通知过
                    if (string.IsNullOrEmpty(strChars) == false && strChars[0] == 'y')
                        continue;

                    // 合并设置一种 body type 的全部通知字符
                    // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                    nRet = ReadersMonitor.SetNotifiedChars(App,
                        strBodyType,
                        "y",
                        ref strHistory,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    ReadersMonitor.SetChar(ref strChars,
                        0,
                        'y',
                        'n');

                    nOverdueCount++;
                }
                else if (string.IsNullOrEmpty(App.NotifyDef) == false)
                {
                    // 检查超期前的通知点

                    List<int> indices = null;
                    // 检查每个通知点，返回当前时间已经达到或者超过了通知点的那些检查点的下标
                    // return:
                    //      -1  数据格式错误
                    //      0   成功
                    nRet = App.CheckNotifyPoint(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        App.NotifyDef,
                        out indices,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 偏移量 0 让给了超期通知
                    for (int k = 0; k < indices.Count; k++)
                    {
                        indices[k] = indices[k] + 1;
                    }

                    // 检查是否至少有一个字符位置为 ch 代表的值
                    if (CheckChar(strChars, indices, 'n', 'n') == true)
                    {
                        // 至少有一个检查点尚未通知
                        strOverDue = "即将到期";
                    }
                    else
                        continue;

                    foreach (int index in indices)
                    {
                        ReadersMonitor.SetChar(ref strChars,
                            index,
                            'y',
                            'n');
                    }

                    nNormalCount++;
                }
                else
                    continue;

                // 合并设置一种 body type 的全部通知字符
                // 把 strChars 中的 'y' 设置到 strHistory 中对应达到位。'n' 不设置
                nRet = ReadersMonitor.SetNotifiedChars(App,
                    strBodyType,
                    strChars,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                    return -1;
                DomUtil.SetAttr(node, "notifyHistory", strHistory);

                // 获得图书摘要信息
                string strSummary = "";
                string strBiblioRecPath = "";
                nRet = this.App.GetBiblioSummary(strBarcode,
                    strConfirmItemRecPath,
                    null,   //  strBiblioRecPathExclude,
                    10, // 25,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                if (nRet == -1)
                {
                    strSummary = "ERROR: " + strError;
                }

                // strResult += (i + 1).ToString() + ") " 
                strResult += strSummary + " ";
                // strResult += "借阅日期: " + DateTimeUtil.LocalDate(strBorrowDate) + " ";
                strResult += "应还日期: " + timeReturning.ToString("d") + " ";
                strResult += strOverDue + "\n";
            }

            /*
            if (nOverdueCount > 0)
                strResult += "=== 共有 " + nOverdueCount.ToString() + " 册图书超期, 请尽快归还。";
            if (nNormalCount > 0)
                strResult += "=== 共有 " + nNormalCount.ToString() + " 册图书即将到期, 请注意在期限内归还。";
             * */

            strBody = strResult;

            if (nOverdueCount + nNormalCount > 0)
                return 1;
            else
                return 0;
        }
#if NO
        // 短消息通知读者超期的版本。供NotifyReader()的重载版本必要时引用
        public int NotifyReaderSMS(
    XmlDocument readerdom,
    Calendar calendar,
    List<string> notifiedBarcodes,
    string strBodyType,
    out string strBody,
    out string strMime,
    out List<string> wantNotifyBarcodes,
    out string strError)
        {
            strBody = "";
            strError = "";
            wantNotifyBarcodes = new List<string>();
            strMime = "text";
            int nRet = 0;

            string strResult = "";
            int nOverdueCount = 0;

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");
            strResult += strName + "您好！您在图书馆借阅的下列图书：";

            // 借阅的册
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                // string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                // string strOperator = DomUtil.GetAttr(node, "operator");
                // string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string strOverDue = "";
                bool bOverdue = false;  // 是否超期
                DateTime timeReturning = DateTime.MinValue;
                {

                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = this.App.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        if (nRet == 1)
                        {
                            bOverdue = true;
                            strOverDue = string.Format(this.App.GetString("已超期s"),  // 已超期 {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                ;
                        }
                    }
                }

                if (bOverdue == true)
                {
                    nOverdueCount++;

                    // 看看是不是已经通知过
                    if (notifiedBarcodes.IndexOf(strBarcode) == -1)
                    {
                        wantNotifyBarcodes.Add(strBarcode);
                    }
                    else
                        continue;

                    // 获得图书摘要信息
                    string strSummary = "";
                    string strBiblioRecPath = "";
                    nRet = this.App.GetBiblioSummary(strBarcode,
                        strConfirmItemRecPath,
                        null,   //  strBiblioRecPathExclude,
                        25,
                        out strBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (nRet == -1)
                    {
                        strSummary = "ERROR: " + strError;
                    }

                    strResult += (i+1).ToString() + ") " + strSummary + " ";
                    strResult += "借阅日期: " + DateTimeUtil.LocalDate(strBorrowDate) + " ";
                    strResult += "应还日期: " + timeReturning.ToString("d") + " ";
                    strResult += strOverDue + " ";
                }
            }

            strResult += "=== 共有 " + nOverdueCount.ToString() + " 册图书超期, 请尽快归还。";

            strBody = strResult;

            if (wantNotifyBarcodes.Count > 0)
                return 1;
            else
                return 0;
        }
#endif
    }

    // 一个扩展消息接口
    public class MessageInterface
    {
        public string Type = "";
        public Assembly Assembly = null;
        public ExternalMessageHost HostObj = null;
    }

#if NO
    class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            this._name = name;
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
#endif
}
