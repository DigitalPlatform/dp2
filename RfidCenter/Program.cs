using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CirculationClient;
using RfidDrivers.First;

namespace RfidCenter
{
    static class Program
    {
        public static Driver1 Rfid = null;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
