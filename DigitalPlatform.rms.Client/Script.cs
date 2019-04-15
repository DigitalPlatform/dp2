using System;
using System.Xml;

using System.Reflection;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;

using DigitalPlatform.Xml;
using DigitalPlatform.Core;
using System.Collections.Generic;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// C#脚本语言实用模块
    /// TODO: 可以考虑废止，用ScriptManager代替
	/// </summary>
	public class Script
	{
		// 从references.xml文件中得到refs字符串数组
		// return:
		//		-1	error
		//		0	正确
		public static int GetRefs(string strRef,
			out string [] saRef,
			out string strError)
		{
			saRef = null;
			strError = "";
			XmlDocument dom = new XmlDocument();

			try 
			{
				dom.LoadXml(strRef);
			}
			catch (Exception ex)
			{
                strError = ExceptionUtil.GetAutoText(ex);
				return -1;
			}

			// 所有ref节点
			XmlNodeList nodes = dom.DocumentElement.SelectNodes("//ref");
			saRef = new string [nodes.Count];
			for(int i=0;i<nodes.Count;i++)
			{
				saRef[i] = DomUtil.GetNodeText(nodes[i]);
			}

			return 0;
		}

		// 创建Assembly
		// parameters:
		//	strCode:	脚本代码
		//	refs:	连接的外部assembly
		// strResult:处理信息
		// objDb:数据库对象，在出错调getErrorInfo用到
		// 返回值:创建好的Assembly
		public static Assembly CreateAssembly(string strCode,
			string[] refs,
			string strLibPaths,
			string strOutputFile,
			out string strErrorInfo,
			out string strWarningInfo)
		{
			// System.Reflection.Assembly compiledAssembly = null;
			strErrorInfo = "";
			strWarningInfo = "";

            // 2019/4/15
            if (refs != null
                && Array.IndexOf(refs, "netstandard.dll") == -1)
            {
                List<string> temp = new List<string>(refs);
                temp.Add("netstandard.dll");
                refs = temp.ToArray();
            }

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
			compilerParams = new CompilerParameters();

			compilerParams.GenerateInMemory = true; //Assembly is created in memory
			// compilerParams.IncludeDebugInformation = true;

			if (strOutputFile != null && strOutputFile != "") 
			{
				compilerParams.GenerateExecutable = false;
				compilerParams.OutputAssembly = strOutputFile;
				// compilerParams.CompilerOptions = "/t:library";
			}

			if (strLibPaths != null && strLibPaths != "")	// bug
				compilerParams.CompilerOptions = "/lib:" + strLibPaths;

			compilerParams.TreatWarningsAsErrors = false;
			compilerParams.WarningLevel = 4;
 
			// 正规化路径，去除里面的宏字符串
			// RemoveRefsBinDirMacro(ref refs);

			compilerParams.ReferencedAssemblies.AddRange(refs);


			CSharpCodeProvider provider;

			// System.CodeDom.Compiler.ICodeCompiler compiler;
			System.CodeDom.Compiler.CompilerResults results = null;
			try 
			{
				provider = new CSharpCodeProvider();
				// compiler = provider.CreateCompiler();
                results = provider.CompileAssemblyFromSource(
					compilerParams, 
					strCode);
			}
			catch (Exception ex) 
			{
				strErrorInfo = "出错 " + ex.Message;
				return null;
			}

			int nErrorCount = 0;

			if (results.Errors.Count != 0) 
			{
				string strErrorString = "";
				nErrorCount = getErrorInfo(results.Errors,
					out strErrorString);

				strErrorInfo = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
				strErrorInfo += strErrorString;

				if (nErrorCount == 0 && results.Errors.Count != 0) 
				{
					strWarningInfo = strErrorInfo;
					strErrorInfo = "";
				}
			}

			if (nErrorCount != 0)
				return null;

 
			return results.CompiledAssembly;
		}

		// parameters:
		//		refs	附加的refs文件路径。路径中可能包含宏%installdir%
		public static int CreateAssemblyFile(string strCode,
			string[] refs,
			string strLibPaths,
			string strOutputFile,
			out string strErrorInfo,
			out string strWarningInfo)
		{
			// System.Reflection.Assembly compiledAssembly = null;
			strErrorInfo = "";
			strWarningInfo = "";

            // 2019/4/15
            if (refs != null
                && Array.IndexOf(refs, "netstandard.dll") == -1)
            {
                List<string> temp = new List<string>(refs);
                temp.Add("netstandard.dll");
                refs = temp.ToArray();
            }

            // CompilerParameters对象
            System.CodeDom.Compiler.CompilerParameters compilerParams;
			compilerParams = new CompilerParameters();

			compilerParams.GenerateInMemory = true; //Assembly is created in memory
			compilerParams.IncludeDebugInformation = true;

			if (strOutputFile != null && strOutputFile != "") 
			{
				compilerParams.GenerateExecutable = false;
				compilerParams.OutputAssembly = strOutputFile;
				// compilerParams.CompilerOptions = "/t:library";
			}

			if (strLibPaths != null && strLibPaths != "")	// bug
				compilerParams.CompilerOptions = "/lib:" + strLibPaths;

			compilerParams.TreatWarningsAsErrors = false;
			compilerParams.WarningLevel = 4;
 
			// 正规化路径，去除里面的宏字符串
			// RemoveRefsBinDirMacro(ref refs);

			compilerParams.ReferencedAssemblies.AddRange(refs);


			CSharpCodeProvider provider;

			// System.CodeDom.Compiler.ICodeCompiler compiler;
			System.CodeDom.Compiler.CompilerResults results = null;
			try 
			{
				provider = new CSharpCodeProvider();
				// compiler = provider.CreateCompiler();
                results = provider.CompileAssemblyFromSource(
					compilerParams, 
					strCode);
			}
			catch (Exception ex) 
			{
				strErrorInfo = "出错 " + ex.Message;
				return -1;
			}

			int nErrorCount = 0;

			if (results.Errors.Count != 0) 
			{
				string strErrorString = "";
				nErrorCount = getErrorInfo(results.Errors,
					out strErrorString);

				strErrorInfo = "信息条数:" + Convert.ToString(results.Errors.Count) + "\r\n";
				strErrorInfo += strErrorString;

				if (nErrorCount == 0 && results.Errors.Count != 0) 
				{
					strWarningInfo = strErrorInfo;
					strErrorInfo = "";
				}
			}

			if (nErrorCount != 0)
				return -1;

 
			return 0;
		}

		// 构造出错信息字符串
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
   
 
			foreach(CompilerError oneError in errors)
			{
				strResult += "(" + Convert.ToString(oneError.Line) + "," + Convert.ToString(oneError.Column) + ") ";
				strResult += (oneError.IsWarning) ? "warning " : "error ";
				strResult += oneError.ErrorNumber + " ";
				strResult += ": " + oneError.ErrorText + "\r\n";

				if (oneError.IsWarning == false)
					nCount ++;

			}
			return nCount;
		}

		public static Type GetDerivedClassType(Assembly assembly,
			string strBaseTypeFullName)
		{
			Type[] types = assembly.GetTypes();
			// string strText = "";

			for(int i=0;i<types.Length;i++) 
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
			for(;;) 
			{
				if (curType == null 
					|| curType.FullName == "System.Object")
					return false;

				if (curType.FullName == strBaseTypeFullName)
					return true;

				curType = curType.BaseType;
			}

		}

	}
}
