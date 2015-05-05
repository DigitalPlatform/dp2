using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Deployment.Application;
using System.Security.Principal;

using DigitalPlatform;

namespace dp2Installer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin /*&& Environment.OSVersion.Version.Major >= 6*/)
            {
                string strDataDir = "";
                if (ApplicationDeployment.IsNetworkDeployed == true)
                {
                    strDataDir = Application.LocalUserAppDataPath;
                }
                else
                {
                    strDataDir = Environment.CurrentDirectory;
                }

                string strUserDir = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
"dp2installer_v1");
                // PathUtil.CreateDirIfNeed(strUserDir);


                // It is not possible to launch a ClickOnce app as administrator directly,
                // so instead we launch the app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
                processInfo.Arguments = "\"datadir=" + strDataDir
                    + "\" \"userdir=" + strUserDir + "\"";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    //MessageBox.Show("Sorry, but I don't seem to be able to start " +
                    //   "this program with administrator rights!");
                    MessageBox.Show("dp2installer 无法运行。\r\n\r\n因为安装和配置 Windows Service 程序的需要，必须在 Administrator 权限下才能运行");
                }

                // Shut down the current process
                Application.Exit();
            }
            else
            {
#if NO
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                    // SingleInstanceApplication.Run(new MainForm(), StartupNextInstanceHandler);
#endif
                // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
                bool createdNew = true;
                // mutex name need contains windows account name. or us programes file path, hashed
                using (Mutex mutex = new Mutex(true, "dp2installer V1", out createdNew))
                {
                    if (createdNew)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm());
                        // SingleInstanceApplication.Run(new MainForm(), StartupNextInstanceHandler);
                    }
                    else
                    {
                        Process current = Process.GetCurrentProcess();
                        foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                        {
                            if (process.Id != current.Id)
                            {
                                API.SetForegroundWindow(process.MainWindowHandle);
                                break;
                            }
                        }
                    }
                }
                /*
http://www.cnblogs.com/riasky/p/3481795.html
                 * * 在Terminal Services下运行时, computer-wide Mutex 只有在同一个terminal server session 中的程序可见, 如果要让它在所有 Terminal Serverces sessions 可见, 则需要在它名字前面加上\.
                 * 
                 * 
                 * */
            }
        }

#if NO
        static void StartupNextInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            // do whatever you want here with e.CommandLine... 
            e.BringToForeground = true;
        }
#endif
    }
}
