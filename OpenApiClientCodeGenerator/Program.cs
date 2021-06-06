using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

using NSwag;
using NSwag.CodeGeneration.CSharp;
using NJsonSchema;
using NJsonSchema.CodeGeneration;

namespace OpenApiClientCodeGenerator
{
    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
    // https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md
    /// <summary>
    /// 根据 swagger.json 文件生成 C# Client 代码的实用工具
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--swagger-url",
                    "Swagger URL"),
                // 输出文件名
                new Option<FileInfo>(
                    "--code-file",
                    "要创建的 C# 文件名"),
                new Option<string>(
                    "--class-name",
                    getDefaultValue: () => null,
                    description: "要创建的类名"),

                new Option<string>(
                    "--namespace",
                    getDefaultValue: () => null,
                    description: "名字空间"),

            };

            rootCommand.Description = "My sample app";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<string, FileInfo, string, string>(
                async (swaggerUrl, codeFile, className, nameSpace) =>
            {
                /*
                Console.WriteLine($"The value for --swagger-url is: {swaggerUrl}");
                Console.WriteLine($"The value for --code-file is: {codeFile?.FullName ?? "null"}");
                Console.WriteLine($"The value for --class-name is: {className}");
                Console.WriteLine($"The value for --namespace is: {nameSpace}");
                */

                // 检查参数
                if (codeFile == null || string.IsNullOrEmpty(codeFile.FullName))
                {
                    Console.WriteLine("*** error: 尚未指定 --code-file 参数");
                    return;
                }

                if (string.IsNullOrEmpty(swaggerUrl))
                {
                    Console.WriteLine("*** error: 尚未指定 --swagger-url 参数");
                    return;
                }

                System.Net.WebClient wclient = new System.Net.WebClient();

                var document = await OpenApiDocument.FromJsonAsync(wclient.DownloadString(url));

                wclient.Dispose();

                var settings = new CSharpClientGeneratorSettings
                {
                    ClassName = className,  // "MyClass",
                    CSharpGeneratorSettings =
                {
                    Namespace = nameSpace   // "MyNamespace"
                }
                };

                settings.CodeGeneratorSettings.PropertyNameGenerator = new MyPropertyNameGen();

                var generator = new CSharpClientGenerator(document, settings);
                var code = generator.GenerateFile();
                File.WriteAllText(codeFile?.FullName, code);
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

#if NO
        // 用法: openapiclientcodegenerator swagger_url code_file_name
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage:\r\nOpenApiClientCodeGenerator swagger_url code_file_name");
                return;
            }

            string url = args[0];
            string output_filepath = args[1];

            Console.WriteLine($"url='{url}'");
            Console.WriteLine($"output_filepath='{output_filepath}'");

            System.Net.WebClient wclient = new System.Net.WebClient();

            var document = await OpenApiDocument.FromJsonAsync(wclient.DownloadString(url));

            wclient.Dispose();

            var settings = new CSharpClientGeneratorSettings
            {
                ClassName = "MyClass",
                CSharpGeneratorSettings =
                {
                    Namespace = "MyNamespace"
                }
            };

            settings.CodeGeneratorSettings.PropertyNameGenerator = new MyPropertyNameGen();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();
            File.WriteAllText(output_filepath, code);
        }

#endif
    }

    //
    // 摘要:
    //     Generates the property name for a given NJsonSchema.JsonSchemaProperty.
    public class MyPropertyNameGen : IPropertyNameGenerator
    {
        //
        // 摘要:
        //     Generates the property name.
        //
        // 参数:
        //   property:
        //     The property.
        //
        // 返回结果:
        //     The new name.
        public string Generate(JsonSchemaProperty property)
        {
            return property.Name;
        }
    }
}
