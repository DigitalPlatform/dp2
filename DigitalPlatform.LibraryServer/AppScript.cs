using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;

using System.Reflection;
using Microsoft.CSharp;
// using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;

using DigitalPlatform;	// Stop��
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.Interfaces;
using System.Web;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// �������Ǻ�C#�ű���صĴ���
    /// </summary>
    public partial class LibraryApplication
    {
        public Assembly m_assemblyLibraryHost = null;
        public string m_strAssemblyLibraryHostError = "";

        public List<MessageInterface> m_externalMessageInterfaces = null;

        // ��ʼ����չ����Ϣ�ӿ�
        /*
	<externalMessageInterface>
 		<interface type="sms" assemblyName="chchdxmessageinterface"/>
	</externalMessageInterface>
         */
        // parameters:
        // return:
        //      -1  ����
        //      0   ��ǰû�������κ���չ��Ϣ�ӿ�
        //      1   �ɹ���ʼ��
        public int InitialExternalMessageInterfaces(out string strError)
        {
            strError = "";

            this.m_externalMessageInterfaces = null;

            XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode(
    "externalMessageInterface");
            if (root == null)
            {
                strError = "��library.xml��û���ҵ�<externalMessageInterface>Ԫ��";
                return 0;
            }

            this.m_externalMessageInterfaces = new List<MessageInterface>();

            XmlNodeList nodes = root.SelectNodes("interface");
            foreach (XmlNode node in nodes)
            {
                string strType = DomUtil.GetAttr(node, "type");
                if (String.IsNullOrEmpty(strType) == true)
                {
                    strError = "<interface>Ԫ��δ����type����ֵ";
                    return -1;
                } 
                
                string strAssemblyName = DomUtil.GetAttr(node, "assemblyName");
                if (String.IsNullOrEmpty(strAssemblyName) == true)
                {
                    strError = "<interface>Ԫ��δ����assemblyName����ֵ";
                    return -1;
                }

                MessageInterface message_interface = new MessageInterface();
                message_interface.Type = strType;
                message_interface.Assembly = Assembly.Load(strAssemblyName);
                if (message_interface.Assembly == null)
                {
                    strError = "����Ϊ '" + strAssemblyName + "' ��Assembly����ʧ��...";
                    return -1;
                }

                Type hostEntryClassType = ScriptManager.GetDerivedClassType(
        message_interface.Assembly,
        "DigitalPlatform.Interfaces.ExternalMessageHost");
                if (hostEntryClassType == null)
                {
                    strError = "����Ϊ '" + strAssemblyName + "' ��Assembly��δ�ҵ� DigitalPlatform.Interfaces.ExternalMessageHost��������࣬��ʼ����չ��Ϣ�ӿ�ʧ��...";
                    return -1;
                }

                message_interface.HostObj = (ExternalMessageHost)hostEntryClassType.InvokeMember(null,
        BindingFlags.DeclaredOnly |
        BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
        null);
                if (message_interface.HostObj == null)
                {
                    strError = "���� type Ϊ '"+strType+"' �� DigitalPlatform.Interfaces.ExternalMessageHost ���������Ķ��󣨹��캯����ʧ�ܣ���ʼ����չ��Ϣ�ӿ�ʧ��...";
                    return -1;
                }

                message_interface.HostObj.App = this;

                this.m_externalMessageInterfaces.Add(message_interface);
            }

            return 1;
        }

        public MessageInterface GetMessageInterface(string strType)
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

        // ��ʼ��Assembly����
        // return:
        //		-1	����
        //		0	�ɹ�
        public int InitialLibraryHostAssembly(out string strError)
        {
            strError = "";
            int nRet = 0;

            this.m_strAssemblyLibraryHostError = "";
            this.m_assemblyLibraryHost = null;

            if (this.LibraryCfgDom == null)
            {
                this.m_assemblyLibraryHost = null;
                strError = "LibraryCfgDomΪ��";
                return -1;
            }

            // �ҵ�<script>�ڵ�
            // �����ڸ���
            XmlNode nodeScript = this.LibraryCfgDom.DocumentElement.SelectSingleNode("script");

            // <script>�ڵ㲻����
            if (nodeScript == null)
            {
                this.m_assemblyLibraryHost = null;
                return 0;
            }

            // <script>�ڵ��¼���CDATA�ڵ�
            if (nodeScript.ChildNodes.Count == 0)
            {
                this.m_assemblyLibraryHost = null;
                return 0;
            }

            XmlNode firstNode = nodeScript.ChildNodes[0];


            //��һ�����ӽڵ㲻��CDATA����Text�ڵ�ʱ
            if (firstNode.NodeType != XmlNodeType.CDATA
                && firstNode.NodeType != XmlNodeType.Text)
            {
                this.m_assemblyLibraryHost = null;
                return 0;
            }

            //~~~~~~~~~~~~~~~~~~
            // ����Assembly����
            string[] saRef = null;
            nRet = GetRefs(nodeScript,
                 out saRef,
                 out strError);
            if (nRet == -1)
                return -1;

            string[] saAddRef = { this.BinDir + "\\" + "digitalplatform.LibraryServer.dll" };

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
                {
                    strError = "library.xml��<script>Ԫ����C#�ű�����ʱ����: \r\n" + strError;
                    this.m_strAssemblyLibraryHostError = strError;
                    return -1;
                }

                this.m_assemblyLibraryHost = assembly;
            }


            return 0;
        }

        // ��node�ڵ�õ�refs�ַ�������
        // return:
        //      -1  ����
        //      0   �ɹ�
        public static int GetRefs(XmlNode node,
            out string[] saRef,
            out string strError)
        {
            saRef = null;
            strError = "";

            // ����ref�ڵ�
            XmlNodeList nodes = node.SelectNodes("//ref");
            saRef = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                saRef[i] = DomUtil.GetNodeText(nodes[i]);
            }
            return 0;
        }

        // ȥ��·���еĺ�%bindir%
        static void RemoveRefsProjectDirMacro(ref string[] refs,
            string strBinDir)
        {
            Hashtable macroTable = new Hashtable();

            macroTable.Add("%bindir%", strBinDir);

            for (int i = 0; i < refs.Length; i++)
            {
                string strNew = PathUtil.UnMacroPath(macroTable,
                refs[i],
                false); // ��Ҫ�׳��쳣����Ϊ���ܻ���%bindir%�����ڻ��޷��滻
                refs[i] = strNew;
            }

        }

        // ����Assembly
        // parameters:
        //		strCode:		�ű�����
        //		refs:			���ӵ��ⲿassembly
        //		strLibPaths:	���·��, ����Ϊ""����null,��˲�����Ч
        //		strOutputFile:	����ļ���, ����Ϊ""����null,��˲�����Ч
        //		strErrorInfo:	������Ϣ
        //		strWarningInfo:	������Ϣ
        // result:
        //		-1  ����
        //		0   �ɹ�
        public static int CreateAssembly(string strCode,
            string[] refs,
            out Assembly assembly,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";
            assembly = null;

            // CompilerParameters����
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
                strError = "CreateAssemblyFile() ���� " + ex.Message;
                return -1;
            }

            //return 0;  //��

            int nErrorCount = 0;
            if (results.Errors.Count != 0)
            {
                string strErrorString = "";
                nErrorCount = getErrorInfo(results.Errors,
                    out strErrorString);

                strError = "��Ϣ����:" + Convert.ToString(results.Errors.Count) + "\r\n";
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

        // ���������Ϣ�ַ���
        // parameter:
        //		errors:    CompilerResults����
        //		strResult: out���������ع���ĳ����ַ���
        // result:
        //		������Ϣ������
        public static int getErrorInfo(CompilerErrorCollection errors,
            out string strResult)
        {
            strResult = "";
            int nCount = 0;
            if (errors == null)
            {
                strResult = "error����Ϊnull";
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


        // ִ�нű�����VerifyBarcode
        // parameters:
        //      host    ���Ϊ�գ������ڲ��� new һ�������͵Ķ��������Ϊ�գ���ֱ��ʹ��
        //      strLibraryCodeList  ��ǰ�����߹�Ͻ�Ĺݴ����б� 2014/9/27
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
        public int DoVerifyBarcodeScriptFunction(
            LibraryHost host,
            string strLibraryCodeList,
            string strBarcode,
            out int nResultValue,
            out string strError)
        {
            strError = "";
            nResultValue = -1;

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�У������š�";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�У������š�";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("VerifyBarcode");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩint VerifyBarcode(string strBarcode, out string strError)����������޷�У������š�";
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
                    strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                    return -1;
                }

                host.App = this;
            }

            ParameterInfo[] parameters = mi.GetParameters();

            // ִ�к���
            try
            {
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

                    // ȡ��out����ֵ
                    strError = (string)args[1];
                }
                else if (parameters.Length == 3)
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

                    // ȡ��out����ֵ
                    strError = (string)args[2];
                }
                else
                {
                    strError = "�ű����� VerifyBarcode() �Ĳ�����������ȷ��Ӧ��Ϊ 2 ���� 3 ��";
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + "VerifyBarcode" + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű�����ItemCanBorrow
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
        public int DoItemCanBorrowScriptFunction(
            bool bRenew,
            Account account,
            XmlDocument itemdom,
            out bool bResultValue,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";
            bResultValue = false;

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����ItemCanBorrow()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����ItemCanBorrow()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("ItemCanBorrow");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic bool ItemCanBorrow(bool bRenew, Account account, XmlDocument itemdom, out string strMessageText)����������޷���ÿɽ�״̬��";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            // ִ�к���
            try
            {
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

                // ȡ��out����ֵ
                strMessage = (string)args[3];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + "ItemCanBorrow" + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű�����ItemCanReturn
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
        public int DoItemCanReturnScriptFunction(
            Account account,
            XmlDocument itemdom,
            out bool bResultValue,
            out string strMessage,
            out string strError)
        {
            strError = "";
            strMessage = "";
            bResultValue = false;

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����ItemCanReturn()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����ItemCanReturn()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("ItemCanReturn");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic bool ItemCanReturn(Account account, XmlDocument itemdom, out string strMessageText)����������޷���ÿɽ�״̬��";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            // ִ�к���
            try
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

                // ȡ��out����ֵ
                strMessage = (string)args[2];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + "ItemCanReturn" + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű����� NotifyReader
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�(��ֻ�Ǳ�ʾ�ű���������ִ�У������Ǳ����ű�����û�з���0�����ֵ)
        // nResultValue
        //      -1  ����
        //      0   û�б�Ҫ����
        //      1   ��Ҫ����
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

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����NotifyReader()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����NotifyReader()��";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            try
            {
                // ���
                nResultValue = host.NotifyReader(
                    readerdom,
                    calendar,
                    // notifiedBarcodes,
                    strBodyType,
                    out strBody,
                    out strMime,
                    // out wantNotifyBarcodes,
                    out strError);
                // ֻҪ�ű�����������ִ�У�nRet���Ƿ���0
                // nResultValue��ֵ�ǽű������ķ���ֵ
            }
            catch (Exception ex)
            {
                strError = "ִ�нű����� NotifyReader() ʱ�׳��쳣��" + ex.Message;
                return -1;
            }

            /*
            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("NotifyReader");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩint NotifyReader()��������˽��ж���֪ͨ��";
                return -2;
            }

            // ִ�к���
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

                // ȡ��out����ֵ
                strError = (string)args[1];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + "NotifyReader" + "'����" + ex.Message;
                return -1;
            }
             * */

            return 0;
        }

        // ִ�нű�����GetForegift
        // �������м۸񣬼������Ҫ�½��ļ۸�
        // parameters:
        //  	strAction	Ϊforegift��return֮һ
        //      strExistPrice   ��ǰʣ��Ľ��
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
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

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic int GetForegift(string strAction, XmlDocument readerdom, string strExistPrice, out string strPrice, out string strError)����������޷���ý����";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            // ִ�к���
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

                // ȡ��out����ֵ
                strPrice = (string)args[3];
                strError = (string)args[4];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + strFuncName + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű�����GetHire
        // ���ݵ�ǰʱ�䡢���ڣ������ʧЧ�ںͼ۸�
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
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

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic int GetHire(XmlDocument readerdom, string strStartDate, string strPeriodName, out string strExpireDate, out string strPrice, out string strError)����������޷���ý����";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            // ִ�к���
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

                // ȡ��out����ֵ
                strExpireDate = (string)args[3];
                strPrice = (string)args[4];
                strError = (string)args[5];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + strFuncName + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }


        // ִ�нű�����GetLost
        // ���ݵ�ǰ���߼�¼��ʵ���¼����Ŀ��¼���������ʧ����⳥���
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
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

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����" + strFuncName + "()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod(strFuncName);
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic int GetLost(XmlDocument readerdom, XmlDocument itemdom, string strPriceCfgString, out string strLostPrice, out string strError)����������޷���ý����";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // ִ�к���
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

                // ȡ��out����ֵ
                strLostPrice = (string)args[3];
                strReason = (string)args[4];
                strError = (string)args[5];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + strFuncName + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű�����GetBiblioPart
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
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

            if (this.m_strAssemblyLibraryHostError != "")
            {
                strError = this.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�ִ�нű�����"+strFuncName+"()��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�ִ�нű�����"+strFuncName+"()��";
                return -2;
            }

            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("GetBiblioPart");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩpublic int GetBiblioPart(XmlDocument bibliodom, string strPartName, out string strResultValue)����������޷���ý����";
                return -2;
            }

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;

            // ִ�к���
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

                // ȡ��out����ֵ
                strResultValue = (string)args[2];
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + strFuncName + "'����" + ex.Message;
                return -1;
            }

            return 0;
        }

        // ִ�нű����� VerifyItem
        // parameters:
        // return:
        //      -2  not found script
        //      -1  ����
        //      0   �ɹ�
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

            if (this.m_assemblyLibraryHost == null)
            {
                strError = "δ����<script>�ű����룬�޷�У����¼��";
                return -2;
            }

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>�ű���δ�ҵ�DigitalPlatform.LibraryServer.LibraryHost��������࣬�޷�У������š�";
                return -2;
            }

#if NO
            // �ٰ󶨼�������assembly��ʵʱѰ���ض����ֵĺ���
            MethodInfo mi = hostEntryClassType.GetMethod("VerifyItem");
            if (mi == null)
            {
                strError = "<script>�ű���DigitalPlatform.LibraryServer.LibraryHost����������У�û���ṩint VerifyItem(string strAction, XmlDocument itemdom, out string strError)����������޷�У����¼��";
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
                strError = "����DigitalPlatform.LibraryServer.LibraryHost���������Ķ��󣨹��캯����ʧ�ܡ�";
                return -1;
            }

            host.App = this;
            host.SessionInfo = sessioninfo;

            // ִ�к���
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

                    // ȡ��out����ֵ
                    strError = (string)args[2];
#endif
                return host.VerifyItem(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "ִ�нű�����'" + "VerifyItem" + "'����" + ex.Message;
                return -1;
            }
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
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public virtual int VerifyItem(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strNewBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

            string strLocation = DomUtil.GetElementText(itemdom.DocumentElement, "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);
            string strLibraryCode = "";
            string strRoom = "";
            // ����
            LibraryApplication.ParseCalendarName(strLocation,
        out strLibraryCode,
        out strRoom);

            // 2014/1/10
            // ���������
            if ((strAction == "new"
|| strAction == "change"
|| strAction == "move")       // delete���������
&& String.IsNullOrEmpty(strNewBarcode) == true)
            {
                XmlElement item = this.App.GetLocationItemElement(
                    strLibraryCode,
                    strRoom);
                if (item != null)
                {
                    bool bNullable = DomUtil.GetBooleanParam(item, "itemBarcodeNullable", true);
                    if (bNullable == false)
                    {
                        strError = "������Ų���Ϊ��(���� <locationTypes> ����)";
                        return 1;
                    }
                }
                else
                {
                    if (this.App.AcceptBlankItemBarcode == false)
                    {
                        strError = "������Ų���Ϊ��(���� AcceptBlankItemBarcode ����)";
                        return 1;
                    }
                }
            }

            if (string.IsNullOrEmpty(strNewBarcode) == false)
            {
                // return:
                //      -1  ���ó���
                //      0   У����ȷ
                //      1   У�鷢�ִ���
                nRet = VerifyItemBarcode(strLibraryCode, strNewBarcode, out strError);
                if (nRet != 0)
                    return nRet;
            }

            // 2014/11/28
            string strPrice = DomUtil.GetElementText(itemdom.DocumentElement, "price");
            if (string.IsNullOrEmpty(strPrice) == false)
            {
                // return:
                //      -1  ���ó���
                //      0   У����ȷ
                //      1   У�鷢�ִ���
                nRet = VerifyItemPrice(strLibraryCode, strPrice, out strError);
                if (nRet != 0)
                    return nRet;
            }
            return 0;
        }

        // ����ȱʡ��Ϊ����֤�۸��ַ���
        // return:
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public virtual int VerifyItemPrice(string strLibraryCode,
            string strPrice,
            out string strError)
        {
            strError = "";

            CurrencyItem item = null;
            // ������������ַ��������� CNY10.00 �� -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out item,
                out strError);
            if (nRet == -1)
                return 1;

            return 0;
        }

        // ����ȱʡ��Ϊ����֤���¼�еĲ������
        // return:
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public int VerifyItemBarcode(
            string strLibraryCode,
            string strNewBarcode,
            out string strError)
        {
            strError = "";
                // ��֤�����
            if (this.App.VerifyBarcode == true)
            {
                // return:
                //	0	invalid barcode
                //	1	is valid reader barcode
                //	2	is valid item barcode
                int nResultValue = 0;

                // return:
                //      -2  not found script
                //      -1  ����
                //      0   �ɹ�
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
                        strError = "library.xml ��û�������������֤�������޷������������֤";
                        return -1;
                    }
                    else if (nRet == -1)
                    {
                        strError = "��֤������ŵĹ����г���"
                           + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                        return -1;
                    }
                    else if (nResultValue != 2)
                    {
                        strError = "����� '" + strNewBarcode + "' ����֤���ֲ���һ���Ϸ��Ĳ������"
                           + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                    }

                    return 1;
                }
            }
            return 0;
        }

        // return:
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public virtual int VerifyOrder(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strOrderTime = DomUtil.GetElementText(itemdom.DocumentElement, "orderTime");

            if (string.IsNullOrEmpty(strOrderTime) == false)
            {
                try
                {
                    DateTimeUtil.FromRfc1123DateTimeString(strOrderTime);
                }
                catch (Exception ex)
                {
                    strError = "���������ַ��� '"+strOrderTime+"' ��ʽ����: " + ex.Message;
                    return -1;
                }
            }

            string strRange = DomUtil.GetElementText(itemdom.DocumentElement, "range");

            if (string.IsNullOrEmpty(strRange) == false)
            {
                // ��鵥�����������ַ����Ƿ�Ϸ�
                // return:
                //      -1  ����
                //      0   ��ȷ
                nRet = LibraryServerUtil.CheckPublishTimeRange(strRange,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ʱ�䷶Χ�ַ��� '" + strRange + "' ��ʽ����: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        // return:
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public virtual int VerifyIssue(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            return 0;
        }

        // return:
        //      -1  ���ó���
        //      0   У����ȷ
        //      1   У�鷢�ִ���
        public virtual int VerifyComment(string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            return 0;
        }


        /*
        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // newһ��Host��������
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod,
                null,
                this,
                null);

        }*/

#if NO
        // ��ǰ�İ汾������ʵ�ֳ���ǰ����
        // retun:
        //      -1  ����
        //      0   û�б�Ҫ����
        //      1   ��Ҫ����
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

            // ���ĵĲ�
            strResult += "<br/>������Ϣ<br/>";
            strResult += "<table class='borrowinfo' width='100%' cellspacing='1' cellpadding='4'>";
            strResult += "<tr class='columntitle'><td nowrap>�������</td><td nowrap>�����</td><td nowrap>��������</td><td nowrap>����</td><td nowrap>������</td><td nowrap>�Ƿ���</td><td nowrap>��ע</td></tr>";
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
 
                // ��鳬�������
                // return:
                //      -1  ���ݸ�ʽ����
                //      0   û�з��ֳ���
                //      1   ���ֳ���   strError������ʾ��Ϣ
                //      2   �Ѿ��ڿ������ڣ������׳��� 2009/3/13 new add
                nRet = App.CheckPeriod(
                    calendar,
                    strBorrowDate,
                    strPeriod,
                    out strError);
                if (nRet == -1)
                    strOverDue = strError;
                else if (nRet == 1)
                {
                    strOverDue = strError;	// "�ѳ���";
                    bOverdue = true;
                }
                else
                    strOverDue = strError;	// ����Ҳ��һЩ��Ҫ����Ϣ������ǹ�����

                string strColor = "bgcolor=#ffffff";

                if (bOverdue == true)
                {
                    strColor = "bgcolor=#ff9999";	// ����

                    nOverdueCount ++;

                    // �����ǲ����Ѿ�֪ͨ��
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
            strResult += "�� " + nOverdueCount.ToString()+ " ��ͼ�鳬��, �뾡��黹��";
            strResult += "</p>";

            strResult += "</body></html>";

            strBody = strResult;

            if (wantNotifyBarcodes.Count > 0)
                return 1;
            else
                return 0;
        }

#endif
        // ��������
        // �°汾�����Դ�����ǰ����
        // retun:
        //      -1  ����
        //      0   û�б�Ҫ����
        //      1   ��Ҫ����
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

            strBody = "";
            strError = "";
            // wantNotifyBarcodes = new List<string>();
            strMime = "html";
            int nRet = 0;

            string strResult = "";
            // int nNotifyCount = 0;   // ��Ҫ֪ͨ������ nNotifyCount = nOverduCount + nNormalCount
            int nOverdueCount = 0;  // �������ѵ�������
            int nNormalCount = 0;   // һ�����ѵ�������

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strLink = "<LINK href='" + App.OpacServerUrl + "/readerhtml.css' type='text/css' rel='stylesheet'>";
            strResult = "<html><head>" + strLink + "</head><body>";

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
    "name");
            strResult += "�𾴵� " + strName + " ���ã�<br/><br/>����ͼ��ݽ��ĵ�����ͼ�飺";

            // ���ĵĲ�
            // strResult += "<br/>������Ϣ<br/>";
            strResult += "<table class='borrowinfo' width='100%' cellspacing='1' cellpadding='4'>";
            strResult += "<tr class='columntitle'>"
                + "<td nowrap>�������</td>"
                + "<td class='no' nowrap align='right'>�����</td>"
                + "<td nowrap>��������</td>"
                + "<td nowrap>����</td>"
                + "<td nowrap>Ӧ������</td>"
                //+ "<td nowrap>������</td>"
                + "<td nowrap>�������</td>"
                //+ "<td nowrap>��ע</td>"
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

                    // ��û�������
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   û�з��ֳ���
                    //      1   ���ֳ���   strError������ʾ��Ϣ
                    //      2   �Ѿ��ڿ������ڣ������׳��� 
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
                            strOverDue = string.Format(this.App.GetString("�ѳ���s"),  // �ѳ��� {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                ;
                        }
                    }
                }

                string strColor = "bgcolor=#ffffff";

                string strChars = "";
                // ���һ�� body type ��ȫ��֪ͨ�ַ�
                strChars = ReadersMonitor.GetNotifiedChars(App,
                    strBodyType,
                    strHistory);

                if (bOverdue == true)
                {
                    strColor = "bgcolor=#ff9999";	// ����


                    // �����ǲ����Ѿ�֪ͨ��
                    if (string.IsNullOrEmpty(strChars) == false && strChars[0] == 'y')
                        continue;

                    // �ϲ�����һ�� body type ��ȫ��֪ͨ�ַ�
                    // �� strChars �е� 'y' ���õ� strHistory �ж�Ӧ�ﵽλ��'n' ������
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
                    // ��鳬��ǰ��֪ͨ��

                    List<int> indices = null;
                    // ���ÿ��֪ͨ�㣬���ص�ǰʱ���Ѿ��ﵽ���߳�����֪ͨ�����Щ������±�
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   �ɹ�
                    nRet = App.CheckNotifyPoint(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        App.NotifyDef,
                        out indices,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // ƫ���� 0 �ø��˳���֪ͨ
                    for (int k = 0; k < indices.Count; k++)
                    {
                        indices[k] = indices[k] + 1;
                    }

                    // ����Ƿ�������һ���ַ�λ��Ϊ ch �����ֵ
                    if (CheckChar(strChars, indices, 'n', 'n') == true)
                    {
                        // ������һ��������δ֪ͨ
                        strOverDue = "��������";
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

                // �ϲ�����һ�� body type ��ȫ��֪ͨ�ַ�
                // �� strChars �е� 'y' ���õ� strHistory �ж�Ӧ�ﵽλ��'n' ������
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
                strResult += "�� " + nOverdueCount.ToString() + " ��ͼ���Ѿ�����, �뾡��黹��";
                strResult += "</p>";
            }

            if (nNormalCount > 0)
            {
                strResult += "<p>";
                strResult += "�� " + nNormalCount.ToString() + " ��ͼ�鼴������, ��ע���������ڹ黹��";
                strResult += "</p>";
            }

            strResult += "</body></html>";

            strBody = strResult;

            if (nOverdueCount + nNormalCount > 0)
                return 1;
            else
                return 0;
        }

        // ����Ƿ�������һ���ַ�λ��Ϊ ch �����ֵ
        public static bool CheckChar(string strText,
            List<int> indices,
            char ch,
            char chDefault)
        {
            foreach (int index in indices)
            {
                if (strText.Length < index + 1)
                {
                    // ������Χ���ַ������� chDefault
                    if (ch == chDefault)
                        return true;
                    continue;
                }
                if (strText[index] == ch)
                    return true;
            }

            return false;
        }

        // ��ָ�����±�λ�������ַ�
        public static bool SetChars(ref string strText,
            List<int> indices,
            char ch)
        {


            return false;
        }

        // ����Ϣ֪ͨ���߳��ڵİ汾����NotifyReader()�����ذ汾��Ҫʱ����
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
            int nOverdueCount = 0;  // �������ѵ�������
            int nNormalCount = 0;   // һ�����ѵ�������

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            if (nodes.Count == 0)
                return 0;

            string strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");
            strResult += "�����ĵ������鿯��\n";

            // ���ĵĲ�
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
                bool bOverdue = false;  // �Ƿ���
                DateTime timeReturning = DateTime.MinValue;
                {

                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // ��û�������
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   û�з��ֳ���
                    //      1   ���ֳ���   strError������ʾ��Ϣ
                    //      2   �Ѿ��ڿ������ڣ������׳��� 
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
                            strOverDue = string.Format(this.App.GetString("�ѳ���s"),  // �ѳ��� {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                ;
                        }
                    }
                }

                string strChars = "";
                // ���һ�� body type ��ȫ��֪ͨ�ַ�
                strChars = ReadersMonitor.GetNotifiedChars(App,
                    strBodyType,
                    strHistory);

                if (bOverdue == true)
                {

                    // �����ǲ����Ѿ�֪ͨ��
                    if (string.IsNullOrEmpty(strChars) == false && strChars[0] == 'y')
                        continue;

                    // �ϲ�����һ�� body type ��ȫ��֪ͨ�ַ�
                    // �� strChars �е� 'y' ���õ� strHistory �ж�Ӧ�ﵽλ��'n' ������
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
                    // ��鳬��ǰ��֪ͨ��

                    List<int> indices = null;
                    // ���ÿ��֪ͨ�㣬���ص�ǰʱ���Ѿ��ﵽ���߳�����֪ͨ�����Щ������±�
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   �ɹ�
                    nRet = App.CheckNotifyPoint(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        App.NotifyDef,
                        out indices,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // ƫ���� 0 �ø��˳���֪ͨ
                    for (int k = 0; k < indices.Count; k++)
                    {
                        indices[k] = indices[k] + 1;
                    }

                    // ����Ƿ�������һ���ַ�λ��Ϊ ch �����ֵ
                    if (CheckChar(strChars, indices, 'n', 'n') == true)
                    {
                        // ������һ��������δ֪ͨ
                        strOverDue = "��������";
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

                // �ϲ�����һ�� body type ��ȫ��֪ͨ�ַ�
                // �� strChars �е� 'y' ���õ� strHistory �ж�Ӧ�ﵽλ��'n' ������
                nRet = ReadersMonitor.SetNotifiedChars(App,
                    strBodyType,
                    strChars,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                    return -1;
                DomUtil.SetAttr(node, "notifyHistory", strHistory);

                // ���ͼ��ժҪ��Ϣ
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
                // strResult += "��������: " + DateTimeUtil.LocalDate(strBorrowDate) + " ";
                strResult += "Ӧ������: " + timeReturning.ToString("d") + " ";
                strResult += strOverDue + "\n";
            }

            /*
            if (nOverdueCount > 0)
                strResult += "=== ���� " + nOverdueCount.ToString() + " ��ͼ�鳬��, �뾡��黹��";
            if (nNormalCount > 0)
                strResult += "=== ���� " + nOverdueCount.ToString() + " ��ͼ�鼴������, ��ע���������ڹ黹��";
             * */

            strBody = strResult;

            if (nOverdueCount + nNormalCount > 0)
                return 1;
            else
                return 0;
        }
#if NO
        // ����Ϣ֪ͨ���߳��ڵİ汾����NotifyReader()�����ذ汾��Ҫʱ����
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
            strResult += strName + "���ã�����ͼ��ݽ��ĵ�����ͼ�飺";

            // ���ĵĲ�
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
                bool bOverdue = false;  // �Ƿ���
                DateTime timeReturning = DateTime.MinValue;
                {

                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // ��û�������
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   û�з��ֳ���
                    //      1   ���ֳ���   strError������ʾ��Ϣ
                    //      2   �Ѿ��ڿ������ڣ������׳��� 
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
                            strOverDue = string.Format(this.App.GetString("�ѳ���s"),  // �ѳ��� {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                ;
                        }
                    }
                }

                if (bOverdue == true)
                {
                    nOverdueCount++;

                    // �����ǲ����Ѿ�֪ͨ��
                    if (notifiedBarcodes.IndexOf(strBarcode) == -1)
                    {
                        wantNotifyBarcodes.Add(strBarcode);
                    }
                    else
                        continue;

                    // ���ͼ��ժҪ��Ϣ
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
                    strResult += "��������: " + DateTimeUtil.LocalDate(strBorrowDate) + " ";
                    strResult += "Ӧ������: " + timeReturning.ToString("d") + " ";
                    strResult += strOverDue + " ";
                }
            }

            strResult += "=== ���� " + nOverdueCount.ToString() + " ��ͼ�鳬��, �뾡��黹��";

            strBody = strResult;

            if (wantNotifyBarcodes.Count > 0)
                return 1;
            else
                return 0;
        }
#endif
    }

    // һ����չ��Ϣ�ӿ�
    public class MessageInterface
    {
        public string Type = "";
        public Assembly Assembly = null;
        public ExternalMessageHost HostObj = null;
    }
}
