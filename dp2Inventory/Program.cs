using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;

namespace dp2Inventory
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);
            FormClientInfo.CopyrightKey = "dp2inventory_sn_key";

            ProgramUtil.SetDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
