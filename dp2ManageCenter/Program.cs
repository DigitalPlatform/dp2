using DigitalPlatform.CirculationClient;
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
                return (MainForm)ClientInfo.MainForm;
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
                ClientInfo.PrepareCatchException();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
