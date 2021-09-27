using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountDoorWaiting
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: countdoorwaiting directory");
                return;
            }
            Console.WriteLine(args[0]);
            Counting(args[0]);
        }

        static void Counting(string directory)
        {
            // 先列出所有文件
            DirectoryInfo di = new DirectoryInfo(directory);
            var files = di.GetFiles();
            foreach(var fi in files)
            {
                int count = 0;
                using (StreamReader reader = new StreamReader(fi.FullName, Encoding.UTF8))
                {
                    while(true)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;

                        if (line.Contains("++incWaiting() door '第一层'"))
                            count++;
                        else if (line.Contains("--decWaiting() door '第一层'"))
                            count--;
                        else
                            continue;

                        if (count == -1)
                            Console.WriteLine(line);
                    }
                }

                if (count != 0)
                    Console.WriteLine($"found {fi.FullName} count != 0 ({count})");
            }

        }
    }
}
