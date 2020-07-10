using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace greenSetup
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /*
            Thread mythread;
            mythread = new Thread(new ThreadStart(ThreadLoop));
            mythread.Start();
            */

            Application.Run(new MainForm());
        }

        /*
        public static void ThreadLoop()
        {
            var form = new SplashForm();
            form.ImageFileName = "c:\\dp2ssl\\splash.png";
            Application.Run(form);
        }
        */
    }
}
