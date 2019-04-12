using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace FingerprintCenter
{
    static class Program
    {
        public static FingerPrint FingerPrint { get; set; }

        static ExecutionContext context = null;
        static Mutex mutex = null;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);

            if (StringUtil.IsDevelopMode() == false)
                ClientInfo.PrepareCatchException();

            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows

            context = ExecutionContext.Capture();
            mutex = new Mutex(true,
                "{75FB942B-5E25-4228-9093-D220FFEDB33C}",
                out bool createdNew);
            try
            {
                List<string> args = StringUtil.GetCommandLineArgs();

                if (createdNew
                    || args.IndexOf("newinstance") != -1)
                {
                    // 删除以前接口程序的 shortcut
                    ClientInfo.RemoveShortcutFromStartupGroup("dp2-中控指纹阅读器接口");

                    ClientInfo.AddShortcutToStartupGroup("dp2-指纹中心");

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
                        }
                    }
                }
            }
            finally
            {
                if (mutex != null)
                    mutex.Close();
            }

#if NO
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
#endif
        }

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
