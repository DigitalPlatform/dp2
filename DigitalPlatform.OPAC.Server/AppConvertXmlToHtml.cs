using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.IO;

using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;


namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和XML数据转换为HTML相关的代码
    /// </summary>
    public partial class OpacApplication
    {
        // 存放xml-->html C# script assembly的hashtable
        public Hashtable Xml2HtmlAssemblyTable = new Hashtable();

        public ReaderWriterLock m_lockXml2HtmlAssemblyTable = new ReaderWriterLock();
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();

        // 根据源代码文件获得Xml到Html转换的Assembly对象
        public int GetXml2HtmlAssembly(
            string strCodeFileName,
            string strRefFileName,
            string strBinDir,
            out Assembly assembly,
            out string strError)
        {
            strError = "";
            assembly = null;
            int nRet = 0;

            // 看看是否已经存在
            this.m_lockXml2HtmlAssemblyTable.AcquireReaderLock(m_nLockTimeout);
            try
            {
                assembly = (Assembly)this.Xml2HtmlAssemblyTable[strCodeFileName.ToLower()];
            }
            finally
            {
                this.m_lockXml2HtmlAssemblyTable.ReleaseReaderLock();
            }

            // 优化
            if (assembly != null)
                return 1;


            string strCode = "";    // c#代码

            // 装入code?
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(strCodeFileName, true);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            strCode = sr.ReadToEnd();
            sr.Close();

            string[] saAddRef1 = {
										 strBinDir + "\\digitalplatform.marcdom.dll",
										 strBinDir + "\\digitalplatform.marckernel.dll",
										 // strBinDir + "\\digitalplatform.rms.client.dll",
										 strBinDir + "\\digitalplatform.OPAC.Server.dll",
										 strBinDir + "\\digitalplatform.dll",
										 strBinDir + "\\digitalplatform.Text.dll",
										 strBinDir + "\\digitalplatform.IO.dll",
										 strBinDir + "\\digitalplatform.Xml.dll",
										 // strBinDir + "\\dp2rms.exe",
										 };

            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = null;

            if (String.IsNullOrEmpty(strRefFileName) == false)
            {
                // 从references.xml文件中得到refs字符串数组
                // return:
                //		-1	error
                //		0	not found file
                //		1	found file
                nRet = ScriptManager.GetRefs(strRefFileName,
                    out saRef2,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ref文件 '" + strRefFileName + "' 出错: " + strError;
                    return -1;
                }
            }

            string[] saRef = null;
            if (saRef2 != null)
            {
                saRef = new string[saRef2.Length + saAddRef1.Length];
                Array.Copy(saRef2, saRef, saRef2.Length);
                Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);
            }
            else
                saRef = saAddRef1;

            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                strError = "文件 '" + strCodeFileName + "' 编译出错: " + strError;
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);

            }

            // 加入hashtable
            this.m_lockXml2HtmlAssemblyTable.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.Xml2HtmlAssemblyTable[strCodeFileName.ToLower()] = assembly;
            }
            finally
            {
                this.m_lockXml2HtmlAssemblyTable.ReleaseWriterLock();
            }

            return 0;
        ERROR1:
            return -1;
        }

        public void ClearXml2HtmlAssembly()
        {
            if (this.m_lockXml2HtmlAssemblyTable == null
                || this.Xml2HtmlAssemblyTable == null)
                return;

            this.m_lockXml2HtmlAssemblyTable.AcquireWriterLock(m_nLockTimeout);
            try
            {
                this.Xml2HtmlAssemblyTable.Clear();
            }
            finally
            {
                this.m_lockXml2HtmlAssemblyTable.ReleaseWriterLock();
            }
        }

        // 将一般库记录数据从XML格式转换为HTML格式
        // parameters:
        //      strRecPath  记录路径。用途是为了给宿主对象的RecPath成员赋值  // 2009/10/18
        // return:
        //      -2  基类为ReaderConverter
        public int ConvertRecordXmlToHtml(
            string strCsFileName,
            string strRefFileName,
            string strXml,
            string strRecPath,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            OpacApplication app = this;

            // 转换为html格式
            Assembly assembly = null;
            int nRet = app.GetXml2HtmlAssembly(
                strCsFileName,
                strRefFileName,
                app.BinDir,
                out assembly,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 得到Assembly中RecordConverter派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.OPAC.Server.RecordConverter");

            if (entryClassType == null)
            {
                // 当没有找到RecordConverter的派生类时，
                // 继续从代码中找一下有没有ReaderConverter的派生类，如果有，则返回-2，这样函数返回后就为调主多提供了一点信息，便于后面继续处理
                entryClassType = ScriptManager.GetDerivedClassType(
                    assembly,
                    "DigitalPlatform.OPAC.Server.ReaderConverter");
                if (entryClassType == null)
                {
                    strError = "从DigitalPlatform.OPAC.Server.RecordConverter派生的类 type entry not found";
                    goto ERROR1;
                }

                return -2;
            }

            // new一个RecordConverter派生对象
            RecordConverter obj = (RecordConverter)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为RecordConverter派生类设置参数
            obj.App = app;
            obj.RecPath = strRecPath;


            // 调用关键函数Convert
            try
            {
                strResult = obj.Convert(strXml);
            }
            catch (Exception ex)
            {
                strError = "脚本执行时抛出异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 将种记录数据从XML格式转换为HTML格式
        public int ConvertBiblioXmlToHtml(
            string strFilterFileName,
            string strBiblioXml,
            string strRecPath,
            out string strBiblio,
            out KeyValueCollection result_params,
            out string strError)
        {
            strBiblio = "";
            strError = "";
            result_params = null;

            OpacApplication app = this;

            FilterHost host = new FilterHost();
            host.RecPath = strRecPath;
            host.App = this;
            host.ResultParams = new KeyValueCollection();

            // 如果必要,转换为MARC格式,调用filter

            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            LoanFilterDocument filter = null;

            nRet = app.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {

                nRet = filter.DoRecord(null,
                    strMarc,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBiblio = host.ResultString;
                result_params = host.ResultParams;
            }
            catch (Exception ex)
            {
                strError = "filter.DoRecord error: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                // 归还对象
                app.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int PrepareMarcFilter(
    FilterHost host,
    string strFilterFileName,
    out LoanFilterDocument filter,
    out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (LoanFilterDocument)this.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new LoanFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " LoanFilterDocument doc = (LoanFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 this.BinDir + "\\digitalplatform.marckernel.dll",
										 this.BinDir + "\\digitalplatform.OPAC.Server.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 this.BinDir + "\\digitalplatform.script.dll",
										 this.BinDir + "\\digitalplatform.marcquery.dll",
										 /*strMainCsDllName*/ };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;

            return 0;
        ERROR1:
            return -1;
        }

        // 映射内核脚本配置文件到本地
        // parameters:
        //      sessioninfo_param   如果为null，函数内部会自动创建一个SessionInfo对象，是管理员权限
        // return:
        //      -1  error
        //      0   成功，为.cs文件
        //      1   成功，为.fltx文件
        public int MapKernelScriptFile(
            SessionInfo sessioninfo_param,
            string strBiblioDbName,
            string strScriptFileName,
            out string strLocalPath,
            out string strError)
        {
            strError = "";
            strLocalPath = "";
            int nRet = 0;

            SessionInfo sessioninfo = null;
            // 应该用管理员的权限来做这个事情
            // 临时的SessionInfo对象
            if (sessioninfo_param == null)
            {
                sessioninfo = new SessionInfo(this);
                sessioninfo.UserID = this.ManagerUserName;
                sessioninfo.Password = this.ManagerPassword;
                sessioninfo.IsReader = false;
            }
            else
                sessioninfo = sessioninfo_param;

            try
            {

                // 将种记录数据从XML格式转换为HTML格式
                // 需要从内核映射过来文件
                // string strScriptFileName = "./cfgs/loan_biblio.fltx";
                // 将脚本文件名正规化
                // 因为在定义脚本文件的时候, 有一个当前库名环境,
                // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
                // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
                string strRemotePath = OpacApplication.CanonicalizeScriptFileName(
                    strBiblioDbName,
                    strScriptFileName);

                // TODO: 还可以考虑支持http://这样的配置文件。

                nRet = this.CfgsMap.MapFileToLocal(
                    sessioninfo.Channel,
                    strRemotePath,
                    out strLocalPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "内核配置文件 " + strRemotePath + "没有找到，因此无法获得书目html格式数据";
                    goto ERROR1;
                }

                bool bFltx = false;
                // 如果是一般.cs文件, 还需要获得.cs.ref配置文件
                if (OpacApplication.IsCsFileName(
                    strScriptFileName) == true)
                {
                    string strTempPath = "";
                    nRet = this.CfgsMap.MapFileToLocal(
                        sessioninfo_param.Channel,
                        strRemotePath + ".ref",
                        out strTempPath,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "内核配置文件 " + strRemotePath + ".ref" + "没有找到，因此无法获得书目html格式数据";
                        goto ERROR1;
                    }

                    bFltx = false;
                }
                else
                {
                    bFltx = true;
                }


                if (bFltx == true)
                    return 1;   // 为.fltx文件

                return 0;

            ERROR1:
                return -1;
            }
            finally
            {
                if (sessioninfo_param == null)
                {
                    sessioninfo.CloseSession();
                }
            }
        }

        // 将脚本文件名正规化
        // 因为在定义脚本文件的时候, 有一个当前库名环境,
        // 如果定义为 ./cfgs/filename 表示在当前库下的cfgs目录下,
        // 而如果定义为 /cfgs/filename 则表示在同服务器的根下
        public static string CanonicalizeScriptFileName(string strDbName,
            string strScriptFileNameParam)
        {
            int nRet = 0;
            nRet = strScriptFileNameParam.IndexOf("./");
            if (nRet == 0)  // != -1   2006/12/24 changed
            {
                // 认为是当前库下
                return strDbName + strScriptFileNameParam.Substring(1);
            }

            nRet = strScriptFileNameParam.IndexOf("/");
            if (nRet == 0)  // != -1   2006/12/24 changed
            {
                // 认为从根开始
                return strScriptFileNameParam.Substring(1);
            }

            return strScriptFileNameParam;  // 保持原样
        }

        // 看看文件名是不是以.cs结尾
        public static bool IsCsFileName(string strFileName)
        {
            strFileName = strFileName.Trim().ToLower();
            int nRet = strFileName.LastIndexOf(".cs");
            if (nRet == -1)
                return false;
            if (nRet + 3 == strFileName.Length)
                return true;
            return false;
        }

        // 将读者记录数据从XML格式转换为HTML格式
        // parameters:
        //      strRecPath  读者记录路径 2009/10/18
        public int ConvertReaderXmlToHtml(
            SessionInfo sessioninfo,
            string strCsFileName,
            string strRefFileName,
            string strXml,
            string strRecPath,
            OperType opertype,
            string[] saBorrowedItemBarcode,
            string strCurrentItemBarcode,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            OpacApplication app = this;

            // 转换为html格式
            Assembly assembly = null;
            int nRet = app.GetXml2HtmlAssembly(
                strCsFileName,
                strRefFileName,
                app.BinDir,
                out assembly,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 得到Assembly中Converter派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.OPAC.Server.ReaderConverter");

            if (entryClassType == null)
            {
                strError = "从DigitalPlatform.OPAC.Server.ReaderConverter派生的类 type entry not found";
                goto ERROR1;
            }

            // new一个Converter派生对象
            ReaderConverter obj = (ReaderConverter)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Converter派生类设置参数
            // obj.MainForm = this;
            obj.BorrowedItemBarcodes = saBorrowedItemBarcode;
            obj.CurrentItemBarcode = strCurrentItemBarcode;
            obj.OperType = opertype;
            obj.App = app;
            obj.SessionInfo = sessioninfo;
            obj.RecPath = strRecPath;

            // 调用关键函数Convert
            try
            {
                strResult = obj.Convert(strXml);
            }
            catch (Exception ex)
            {
                strError = "脚本执行时抛出异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
        }
    }
}
