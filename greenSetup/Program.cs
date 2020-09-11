using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace greenSetup
{
    static class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr handle);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if REMOVED
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /*
            Thread mythread;
            mythread = new Thread(new ThreadStart(ThreadLoop));
            mythread.Start();
            */

            Application.Run(new MainForm());
#endif

            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true, "greenSetup-mutex", out createdNew))
            {
                if (createdNew)
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    Application.Run(new MainForm());
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
            /*
http://www.cnblogs.com/riasky/p/3481795.html
             * * ��Terminal Services������ʱ, computer-wide Mutex ֻ����ͬһ��terminal server session �еĳ���ɼ�, ���Ҫ���������� Terminal Serverces sessions �ɼ�, ����Ҫ��������ǰ�����\.
             * 
             * 
             * */

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
