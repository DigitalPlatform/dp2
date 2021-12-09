using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;

namespace TestRoslyn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            var assembly = CreateAssembly(@"
    using System;
    using System.Windows.Forms;
    using System.Xml;

    namespace RoslynCompileSample
    {
        public class Writer
        {
            public void Write(XmlDocument dom, string message)
            {
                MessageBox.Show(dom.DocumentElement.OuterXml + message);
            }
        }
    }

");
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            Type type = assembly.GetType("RoslynCompileSample.Writer");
            object obj = Activator.CreateInstance(type);
            type.InvokeMember("Write",
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                obj,
                new object[] { dom, "Hello World" });
        }


        static Assembly CreateAssembly(string strCode)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(strCode);
            string fileName = Guid.NewGuid().ToString();

            MetadataReference[] references = new MetadataReference[]
{
    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(XmlDocument).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(Form).Assembly.Location)
};

            // A single, immutable invocation to the compiler
            // to produce a library
            var compilation = CSharpCompilation.Create(fileName)
              .WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .AddReferences(references)
              .AddSyntaxTrees(tree);

            // string strBinDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);   //  Environment.CurrentDirectory;

            // string path = Path.Combine(strBinDir/*Directory.GetCurrentDirectory()*/, fileName);

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
                    return Assembly.Load(stream.ToArray());

                    // return AssemblyLoadContext.Default.LoadFromStream(stream);
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
                    string error = string.Join("\r\n", errors);
                    throw new Exception(error);
                }

                /*
                if (errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(errors, "\r\n");
                    return -1;
                }

                strWarning = StringUtil.MakePathList(warnings, "\r\n");
                */
                return null;
            }


        }

#if NO
        private void button_test_Click(object sender, EventArgs e)
        {
            var result = Task.Run<object>(async () =>
            {
                // CSharpScript.RunAsync can also be generic with typed ReturnValue
                var s = await CSharpScript.RunAsync(@"using System;

string 
"

);

                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(@"var x = ""my/"" + string.Join(""_"", ""a"", ""b"", ""c"") + "".ss"";");
                s = await s.ContinueWithAsync(@"var y = ""my/"" + @x;");
                s = await s.ContinueWithAsync(@"y // this just returns y, note there is NOT trailing semicolon");

                /*
                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    Console.WriteLine("name: {0}, type: {1}, value: {2}", variable.Name, variable.Type.Name, variable.Value);
                }
                */
                return s.ReturnValue;

            }).Result;

            MessageBox.Show(this, (string)result);
        }
#endif
    }
}
