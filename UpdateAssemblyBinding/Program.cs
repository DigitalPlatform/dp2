using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Install;
using DigitalPlatform.Text;

// 更新 XML 文件中的 assemblyBinding

namespace UpdateAssemblyBinding
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"正在执行 updateAssemblyBinding. args='{string.Join(" ", args)}'");

            var table = BuildParamTable(args);

            string sourceFileName = table["source"] as string;
            string targetFileName = table["target"] as string;
            // string projectDir = table["projectDir"] as string;

            if (string.IsNullOrEmpty(sourceFileName))
            {
                Console.WriteLine("缺乏 source 参数");
                return;
            }

            if (string.IsNullOrEmpty(targetFileName))
            {
                Console.WriteLine("缺乏 target 参数");
                return;
            }

            /*
            if (string.IsNullOrEmpty(projectDir))
            {
                Console.WriteLine("缺乏 projectDir 参数");
                return;
            }
            */

            //sourceFileName = Path.Combine(projectDir, sourceFileName);
            //targetFileName = Path.Combine(projectDir, targetFileName);

            if (File.Exists(sourceFileName) == false)
            {
                Console.Write($"文件 {sourceFileName} 不存在");
                return;
            }

            if (File.Exists(targetFileName) == false)
            {
                Console.Write($"文件 {targetFileName} 不存在");
                return;
            }

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
targetFileName,
out string strError);
            if (nRet == -1)
            {
                Console.WriteLine($"updateAssemblyBinding 出错: {strError}");
                return;
            }

            if (nRet == 0)
            {
                Console.WriteLine($"目标文件 {targetFileName} 没有变化");
            }
            else if (nRet == 1)
            {
                Console.WriteLine($"目标文件 {targetFileName} 发生了局部更新");
            }
            Console.WriteLine($"updateAssemblyBinding 结束");
        }

        static Hashtable BuildParamTable(string[] args)
        {
            Hashtable result = new Hashtable();
            foreach (var arg in args)
            {
                var parts = StringUtil.ParseTwoPart(arg, ":");
                result[parts[0]] = parts[1];
            }

            return result;
        }
    }
}
