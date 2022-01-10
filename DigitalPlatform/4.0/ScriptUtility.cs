using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace DigitalPlatform
{
    /// <summary>
    /// C# 脚本实用函数
    /// </summary>
    public static class ScriptUtility
    {
        // result:
        //		-1  出错
        //		0   成功
        public static int CreateAssembly(string strCode,
            string[] refs,
            string strOutputFileName,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            try
            {
                if (File.Exists(strOutputFileName))
                    File.Delete(strOutputFileName);

                // 2019/4/5
                if (refs != null
                    && Array.IndexOf(refs, "netstandard.dll") == -1)
                {
                    List<string> temp = new List<string>(refs);
                    temp.Add("netstandard.dll");
                    refs = temp.ToArray();
                }

                var tree = SyntaxFactory.ParseSyntaxTree(strCode);
                string fileName = Guid.NewGuid().ToString();

                string basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location);

                List<MetadataReference> ref_list = new List<MetadataReference>();
                foreach (string one in refs)
                {
                    string path = one;
                    if (path.IndexOf("/") == -1 && path.IndexOf("\\") == -1)
                        path = Path.Combine(basePath, path);

                    ref_list.Add(MetadataReference.CreateFromFile(path));
                }

                ref_list.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                ref_list.Add(MetadataReference.CreateFromFile(typeof(Span<>).Assembly.Location));

                var compilation = CSharpCompilation.Create(fileName)
      .WithOptions(
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      .AddReferences(ref_list.ToArray())
      .AddSyntaxTrees(tree);

                using (var stream = new MemoryStream())
                {
                    EmitResult compilationResult = compilation.Emit(stream);

                    List<string> errors = new List<string>();
                    List<string> warnings = new List<string>();

                    if (compilationResult.Success)
                    {
                        // Load the assembly
                        // assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (Stream target = File.Create(strOutputFileName))
                        {
                            stream.CopyTo(target);
                        }
                        return 0;
                    }
                    else
                    {
                        foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                        {
                            if (codeIssue.Severity == DiagnosticSeverity.Error)
                            {
                                string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                                errors.Add(issue);
                            }
                            else
                            {
                                string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                                warnings.Add(issue);
                            }
                        }
                    }

                    if (errors.Count > 0)
                    {
                        strError = MakePathList(errors, "\r\n");
                        return -1;
                    }

                    strWarning = MakePathList(warnings, "\r\n");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                strError = "CreateAssemblyFile() 出错 " + GetDebugText(ex);
                return -1;
            }
        }

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

            try
            {
                // 2019/4/5
                if (refs != null
                    && Array.IndexOf(refs, "netstandard.dll") == -1)
                {
                    List<string> temp = new List<string>(refs);
                    temp.Add("netstandard.dll");
                    refs = temp.ToArray();
                }

                var tree = SyntaxFactory.ParseSyntaxTree(strCode);
                string fileName = Guid.NewGuid().ToString();

                string basePath = Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location);

                List<MetadataReference> ref_list = new List<MetadataReference>();
                foreach (string one in refs)
                {
                    string path = one;
                    if (path.IndexOf("/") == -1 && path.IndexOf("\\") == -1)
                        path = Path.Combine(basePath, path);

                    ref_list.Add(MetadataReference.CreateFromFile(path));
                }

                ref_list.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                ref_list.Add(MetadataReference.CreateFromFile(typeof(Span<>).Assembly.Location));

                var compilation = CSharpCompilation.Create(fileName)
      .WithOptions(
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      .AddReferences(ref_list.ToArray())
      .AddSyntaxTrees(tree);

                using (var stream = new MemoryStream())
                {
                    EmitResult compilationResult = compilation.Emit(stream);

                    List<string> errors = new List<string>();
                    List<string> warnings = new List<string>();

                    if (compilationResult.Success)
                    {
                        // Load the assembly
                        // assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                        stream.Seek(0, SeekOrigin.Begin);
                        assembly = Assembly.Load(stream.ToArray());

                        return 0;
                    }
                    else
                    {
                        foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                        {
                            if (codeIssue.Severity == DiagnosticSeverity.Error)
                            {
                                string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                                errors.Add(issue);
                            }
                            else
                            {
                                string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()},Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                                warnings.Add(issue);
                            }
                        }
                    }

                    if (errors.Count > 0)
                    {
                        strError = MakePathList(errors, "\r\n");
                        return -1;
                    }

                    strWarning = MakePathList(warnings, "\r\n");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                strError = "CreateAssemblyFile() 出错 " + GetDebugText(ex);
                return -1;
            }
        }

        public static string MakePathList(List<string> aPath,
    string strSep)
        {
            if (aPath.Count == 0)
                return "";

            return String.Join(strSep, aPath.ToArray());
        }

        // 返回详细调用堆栈
        public static string GetDebugText(Exception e)
        {
            StringBuilder message = new StringBuilder();

            Exception currentException = null;
            for (currentException = e; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("Type: {0}\r\nMessage: {1}\r\nStack:\r\n{2}\r\n\r\n",
                                     currentException.GetType().FullName,
                                     currentException.Message,
                                     currentException.StackTrace);
            }

            return message.ToString();
        }

    }
}
