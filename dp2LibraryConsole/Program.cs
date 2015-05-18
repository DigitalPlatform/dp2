using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2LibraryConsole
{
    /// <summary>
    /// dp2Library 管理控制台
    /// </summary>
    class Program
    {

        static void Main(string[] args)
        {
            using (Instance instance = new Instance())
            {
                while (true)
                {
                    instance.DisplayPrompt();

                    string line = Console.ReadLine();

                    if (instance.ProcessCommand(line) == true)
                        return;
                }
            }
        }

    }


}
