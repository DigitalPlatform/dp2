using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

using RfidDrivers.First;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

using DigitalPlatform.Core;

namespace RfidCenter
{
    static class Program
    {
        public static Driver1 Rfid = null;

        static ExecutionContext context = null;
        static Mutex mutex = null;

        // https://stackoverflow.com/questions/8836093/how-can-i-specify-a-dllimport-path-at-runtime
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, IntPtr.Size == 8 ? "x64" : "x86");

            SetDllDirectory(assemblyPath);

            //AppDomain currentDomain = AppDomain.CurrentDomain;
            //currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSameFolder);

            ClientInfo.TypeOfProgram = typeof(Program);

            if (StringUtil.IsDevelopMode() == false)
                ClientInfo.PrepareCatchException();

            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows

            context = ExecutionContext.Capture();
            mutex = new Mutex(true,
                "{CF1B7B4A-C7ED-4DB8-B5CC-59A067880F92}",
                out bool createdNew);
            try
            {
                List<string> args = StringUtil.GetCommandLineArgs();

                if (createdNew
                    || args.IndexOf("newinstance") != -1)
                {
                    ClientInfo.AddShortcutToStartupGroup("dp2-RFID中心");

                    ProgramUtil.SetDpiAwareness();

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    _mainForm = new MainForm();
                    Application.Run(_mainForm);
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            API.SetForegroundWindow(process.MainWindowHandle);
                            if (API.IsIconic(process.MainWindowHandle))
                            {
                                // API.ShowWindow(process.MainWindowHandle, API.SW_SHOW);
                                API.ShowWindow(process.MainWindowHandle, API.SW_RESTORE);
                            }
                            else
                            {
                                // 用 .net remoting 通讯
                                MainForm.CallActivate("ipc://RfidChannel/RfidServer");
                            }
                        }
                    }
                }
            }
            finally
            {
                if (mutex != null)
                    mutex.Close();
            }
        }
#if NO
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
#endif

#if NO
        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
#endif

        static MainForm _mainForm = null;
        // 这里用 _mainForm 存储窗口对象，不采取 Form.ActiveForm 获取的方式。原因如下
        // http://stackoverflow.com/questions/17117372/form-activeform-occasionally-works
        // Form.ActiveForm occasionally works

        public static MainForm MainForm
        {
            get
            {
                return _mainForm;
            }
        }
    }
}
