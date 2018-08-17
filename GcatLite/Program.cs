using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Diagnostics;

using DigitalPlatform;

namespace GcatLite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 防止多于一个的Instance启动
            Process[] processes = Process.GetProcessesByName("GcatLite");

            if (processes.Length > 1)
            {
                for (int i = 0; i < processes.Length; i++)
                {
                    IntPtr handle = processes[i].MainWindowHandle;
                    if (handle != (IntPtr)API.INVALID_HANDLE_VALUE)
                    {
                        API.ShowWindow(handle,API.SW_RESTORE);  // 将最小化或者最大化的窗口复原
                        API.SetForegroundWindow(handle);    // 将窗口翻到前台
                    }
                }
                return;
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}