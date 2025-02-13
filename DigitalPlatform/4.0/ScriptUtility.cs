using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

                // 
                List<MetadataReference> ref_list = GetRefList(refs);

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
                        GetErrors(compilationResult,
    errors,
    warnings);
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
                strError = "CreateAssemblyFile() 2出错 " + GetDebugText(ex);
                return -1;
            }
        }

        static void GetErrors(EmitResult result,
            List<string> errors,
            List<string> warnings)
        {
            foreach (Diagnostic codeIssue in result.Diagnostics)
            {
                var line_span = codeIssue.Location.GetLineSpan();
                string position = $"{line_span.StartLinePosition.Line}, {line_span.StartLinePosition.Character}";
                string issue = $"({position}) {codeIssue.Severity} {codeIssue.GetMessage()} {codeIssue.Id}";

                if (codeIssue.Severity == DiagnosticSeverity.Error)
                {
                    errors.Add(issue);
                }
                else
                {
                    warnings.Add(issue);
                }
            }
        }

        static List<MetadataReference> GetRefList(string[] refs)
        {
            List<string> dirs = new List<string>() {
            Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location),
            Environment.CurrentDirectory,
            };

            List<MetadataReference> ref_list = new List<MetadataReference>();
            foreach (string one in refs)
            {
                string path = one;
                if (path.IndexOf("/") == -1 && path.IndexOf("\\") == -1)
                {
                    path = MakePath(dirs, path);
                    if (path == null)
                    {
                        throw new Exception($"无法定位 {one} 的所在目录");
                    }
                }

                ref_list.Add(MetadataReference.CreateFromFile(path));
            }

            ref_list.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            // ref_list.Add(MetadataReference.CreateFromFile(typeof(Task).Assembly.Location));

            return ref_list;
        }

        static string MakePath(List<string> dirs, string filename)
        {
            foreach (var dir in dirs)
            {
                string path = Path.Combine(dir, filename);
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        // 2022/11/19
        // 获得 Stream
        // 注意使用返回的 Stream 对象，读之前要 Seek(0, Origin.Begin)
        public static int CreateAssembly(
            string strCode,
    string[] refs,
    Stream stream,
    out string strError,
    out string strWarning)
        {
            strError = "";
            strWarning = "";

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

                // 
                List<MetadataReference> ref_list = GetRefList(refs);

                var compilation = CSharpCompilation.Create(fileName)
      .WithOptions(
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
      .AddReferences(ref_list.ToArray())
      .AddSyntaxTrees(tree);


                EmitResult compilationResult = compilation.Emit(stream);
                /*
                EmitOptions options = new EmitOptions().WithDebugInformationFormat(DebugInformationFormat.Embedded);
                EmitResult compilationResult = compilation.Emit(stream, null, null, null, null, options);
                */
                List<string> errors = new List<string>();
                List<string> warnings = new List<string>();

                if (compilationResult.Success)
                {
                    return 0;
                }
                else
                {
                    GetErrors(compilationResult,
errors,
warnings);
                }

                if (errors.Count > 0)
                {
                    strError = MakePathList(errors, "\r\n");
                    return -1;
                }

                strWarning = MakePathList(warnings, "\r\n");
                return 0;
            }
            catch (Exception ex)
            {
                strError = "CreateAssemblyFile() 3出错 " + GetDebugText(ex);
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

                // 
                List<MetadataReference> ref_list = GetRefList(refs);

                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    // .WithEmitDebugInformation(true);
                    ;
                var compilation = CSharpCompilation.Create(fileName)
      .WithOptions(options)
      .AddReferences(ref_list.ToArray())
      .AddSyntaxTrees(tree);

                using (var stream = new MemoryStream())
                {
                    EmitResult compilationResult = compilation.Emit(stream);
                    /*
                    EmitOptions options = new EmitOptions().WithDebugInformationFormat(DebugInformationFormat.Embedded);
                    EmitResult compilationResult = compilation.Emit(stream, null, null, null, null, options);
                    */
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
                        GetErrors(compilationResult,
    errors,
    warnings);
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
                strError = "CreateAssemblyFile() 4出错 " + GetDebugText(ex);
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
