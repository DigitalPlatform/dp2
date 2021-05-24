using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenApiClientCodeGenerator
{
    class Program
    {
        // 用法: openapiclientcodegenerator swagger_url code_file_name
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage:\r\nopenapiclientcodegenerator swagger_url code_file_name");
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
