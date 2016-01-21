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

using DigitalPlatform;	// Stop类
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.Interfaces;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和用于格式转换的C#脚本相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();

#if NO
        // 存放xml-->html C# script assembly的hashtable
        public Hashtable Xml2HtmlAssemblyTable = new Hashtable();


        public ReaderWriterLock m_lockXml2HtmlAssemblyTable = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒
#endif
        // 存储 Assembly 的容器
        internal ObjectCache<Assembly> AssemblyCache = new ObjectCache<Assembly>();

        // 将读者记录数据从XML格式转换为HTML格式
        // parameters:
        //      strRecPath  读者记录路径 2009/10/18
        //      strLibraryCode  读者记录所从属的读者库的馆代码
        //      strResultType  细节格式。为了 '|' 间隔的若干名称字符串
        public int ConvertReaderXmlToHtml(
            SessionInfo sessioninfo,
            string strCsFileName,
            string strRefFileName,
            string strLibraryCode,
            string strXml,
            string strRecPath,
            OperType opertype,
            string[] saBorrowedItemBarcode,
            string strCurrentItemBarcode,
            string strResultType,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            int nRet = 0;

            // TODO: ParseTwoPart
            string strSubType = "";
            nRet = strResultType.IndexOf(":");
            if (nRet != -1)
                strSubType = strResultType.Substring(nRet + 1).Trim();

            LibraryApplication app = this;

            // 转换为html格式
            Assembly assembly = null;
            nRet = app.GetXml2HtmlAssembly(
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
                "DigitalPlatform.LibraryServer.ReaderConverter");

            if (entryClassType == null)
            {
                strError = "从DigitalPlatform.LibraryServer.ReaderConverter派生的类 type entry not found";
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
            obj.LibraryCode = strLibraryCode;   // 2012/9/8
            obj.Formats = strSubType.Replace("|", ",");

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

#if NO
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
#endif
            assembly = this.AssemblyCache.FindObject(strCodeFileName);

            // 优化
            if (assembly != null)
                return 1;

            string strCode = "";    // c#代码

            // 装入code?
            try
            {
                using (StreamReader sr = new StreamReader(strCodeFileName, true))
                {
                    strCode = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string[] saAddRef1 = {

                                    // 2011/9/3 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.web.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

										 strBinDir + "\\digitalplatform.marcdom.dll",
										 strBinDir + "\\digitalplatform.marckernel.dll",
										 strBinDir + "\\digitalplatform.rms.client.dll",
										 strBinDir + "\\digitalplatform.libraryserver.dll",
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

#if NO
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
#endif
            this.AssemblyCache.SetObject(strCodeFileName, assembly);
            return 0;
        ERROR1:
            return -1;
        }

        // 从cs文件创建Assembly并new好ItemConverter派生对象
        public ItemConverter NewItemConverter(
            string strCsFileName,
            string strRefFileName,
            out string strError)
        {
            strError = "";

            ItemConverter obj = null;

            Assembly assembly = null;
            int nRet = this.GetXml2HtmlAssembly(
                strCsFileName,
                strRefFileName,
                this.BinDir,
                out assembly,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 得到Assembly中Converter派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "DigitalPlatform.LibraryServer.ItemConverter");

            if (entryClassType == null)
            {
                strError = "从DigitalPlatform.LibraryServer.ItemConverter派生的类 type entry not found";
                goto ERROR1;
            }

            // new一个Converter派生对象
            obj = (ItemConverter)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return obj;
        ERROR1:
            return null;
        }

        // 将册记录数据从XML格式转换为HTML格式
        public static int RunItemConverter(
            string strFunction,
            ItemConverter obj,
            object sender,
            ItemConverterEventArgs e,
            out string strError)
        {
            strError = "";

            // 调用关键函数Item
            try
            {
                if (strFunction == "item")
                    obj.Item(sender, e);
                else if (strFunction == "begin")
                    obj.Begin(sender, e);
                else if (strFunction == "end")
                    obj.End(sender, e);
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

            LibraryApplication app = this;

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
                "DigitalPlatform.LibraryServer.RecordConverter");

            if (entryClassType == null)
            {
                // 当没有找到RecordConverter的派生类时，
                // 继续从代码中找一下有没有ReaderConverter的派生类，如果有，则返回-2，这样函数返回后就为调主多提供了一点信息，便于后面继续处理
                entryClassType = ScriptManager.GetDerivedClassType(
                    assembly,
                    "DigitalPlatform.LibraryServer.ReaderConverter");
                if (entryClassType == null)
                {
                    strError = "从DigitalPlatform.LibraryServer.RecordConverter派生的类 type entry not found";
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



        // 将册记录数据从XML格式转换为HTML格式
        // 兼容旧函数，一次性调用(只触发Item()函数)，无批概念
        // parameters:
        //      strRecPath  册记录路径。用途是为了给宿主对象的RecPath成员赋值  // 2009/10/18
        public int ConvertItemXmlToHtml(
            string strCsFileName,
            string strRefFileName,
            string strXml,
            string strRecPath,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            ItemConverter obj = this.NewItemConverter(
                strCsFileName,
                strRefFileName,
                out strError);
            if (obj == null)
                return -1;
            obj.App = this;


            // 调用关键函数Item
            try
            {
                ItemConverterEventArgs e = new ItemConverterEventArgs();
                e.Index = 0;
                e.Count = 1;
                e.ActiveBarcode = "";
                e.Xml = strXml;
                e.RecPath = strRecPath; // 2009/10/18

                obj.Item(this, e);

                strResult = e.ResultString;
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



        public int PrepareMarcFilter(
            // FilterHost host,
    string strFilterFileName,
    out LoanFilterDocument filter,
    out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (LoanFilterDocument)this.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                // filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new LoanFilterDocument();

            // filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " LoanFilterDocument doc = (LoanFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Assembly assembly = null;
            assembly = this.AssemblyCache.FindObject(strFilterFileName);
            if (assembly != null)
            {
                filter.Assembly = assembly;
                return 0;
            }

            string[] saAddRef1 = {
                                    // 2011/9/3 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.web.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 this.BinDir + "\\digitalplatform.marckernel.dll",
										 this.BinDir + "\\digitalplatform.libraryserver.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 this.BinDir + "\\digitalplatform.script.dll",
										 this.BinDir + "\\digitalplatform.marcquery.dll",
										 /*strMainCsDllName*/ };

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
            this.AssemblyCache.SetObject(strFilterFileName, assembly);
            return 0;
        ERROR1:
            return -1;
        }

        // 将种记录数据从XML格式转换为HTML格式
        // parameters:
        //      strBiblioXml    XML记录，或者 MARC 记录
        //      strSyntax   MARC格式 usmarc/unimarc。如果strBiblioXml 第一字符为 '<' 则本参数可以为空
        public int ConvertBiblioXmlToHtml(
            string strFilterFileName,
            string strBiblioXml,
            string strSyntax,
            string strRecPath,
            out string strBiblio,
            out string strError)
        {
            strBiblio = "";
            strError = "";
            int nRet = 0;

            LibraryApplication app = this;

            FilterHost host = new FilterHost();
            host.RecPath = strRecPath;
            host.App = this;

            string strMarc = "";
            if (string.IsNullOrEmpty(strBiblioXml) == false
                && strBiblioXml[0] == '<')
            {
                // 如果必要,转换为MARC格式,调用filter

                // string strOutMarcSyntax = "";
                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
                strMarc = strBiblioXml;

            LoanFilterDocument filter = null;

            nRet = app.PrepareMarcFilter(
                // host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            filter.FilterHost = host;

            try
            {

                nRet = filter.DoRecord(null,
                    strMarc,
                    strSyntax,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBiblio = host.ResultString;
            }
            catch (Exception ex)
            {
                strError = "filter.DoRecord error: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                // 2012/3/28
                filter.FilterHost = null;   // 脱钩

                // 归还对象
                app.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

    }
}
