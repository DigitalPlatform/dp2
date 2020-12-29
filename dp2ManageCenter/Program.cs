using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2ManageCenter
{
    static class Program
    {
        public static MainForm MainForm
        {
            get
            {
                return (MainForm)FormClientInfo.MainForm;
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);

            if (StringUtil.IsDevelopMode() == false)
                FormClientInfo.PrepareCatchException();

            ProgramUtil.SetDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
