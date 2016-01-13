using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    static class Program
    {
        /// <summary>
        /// 前端，也就是 dp2circulation.exe 的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

#if NO
        static ExecutionContext context = ExecutionContext.Capture();
        static Mutex mutex = new Mutex(true, "{A810CFB4-D932-4821-91D4-4090C84C5C68}");
#endif
        static ExecutionContext context = null;
        static Mutex mutex = null;

        static bool _bExiting = false;   // 是否处在 正在退出 的状态

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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

            List<string> args = StringUtil.GetCommandLineArgs();

            // 绿色安装方式下，如果没有按住 Ctrl 键启动，会优先用 ClickOnce 方式启动
            if (ApplicationDeployment.IsNetworkDeployed == false
                && Control.ModifierKeys != Keys.Control
                && args.IndexOf("green") == -1
                && StringUtil.IsDevelopMode() == false)
            {
                string strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2内务 V2");
                if (File.Exists(strShortcutFilePath) == true)
                {
                    try
                    {
                        Process.Start(strShortcutFilePath);
                        return;
                    }
                    catch
                    {

                    }
                }
            }

#if NO
            if (ApplicationDeployment.IsNetworkDeployed &&
        ApplicationDeployment.CurrentDeployment.ActivationUri != null)
            {
                // string startupUrl = ApplicationDeployment.CurrentDeployment.ActivationUri.ToString();
                MessageBox.Show("first=" + ApplicationDeployment.CurrentDeployment.ActivationUri.Query);
                args = StringUtil.GetClickOnceCommandLineArgs(ApplicationDeployment.CurrentDeployment.ActivationUri.Query);
            }

            // Also tack on any activation args at the back
            var activationArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (activationArgs != null && activationArgs.ActivationData != null)
            {
                args.AddRange(activationArgs.ActivationData);
                MessageBox.Show("second=" + StringUtil.MakePathList(args));
            }
#endif

            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
            bool createdNew = true;

            context = ExecutionContext.Capture();
            mutex = new Mutex(true,
                "{A810CFB4-D932-4821-91D4-4090C84C5C68}",
                out createdNew);
            try
            {
                if (createdNew 
                    // || _suppressMutex 
                    || args.IndexOf("newinstance") != -1)
                {
                    if (StringUtil.IsDevelopMode() == false)
                        PrepareCatchException();

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
                            // break;
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
        static bool _suppressMutex = false;   // 是否越过 Mutex 机制？ true 表示要越过

        public static void SuppressMutex()
        {
            // TODO: 这个方法只能让当前实例不在乎重复启动，而无法避免后面新起来的实例被当前实例的 Mutex 禁止
            // 似乎还需要找到进程直接共享信息的方法，告诉后一个实例允许重复启动
            _suppressMutex = true;
        }
#endif

        public static void ReleaseMutex()
        {
            ExecutionContext.Run(context, (state) => {
                if (mutex != null)
                {
#if NO
                    mutex.ReleaseMutex();
                    mutex.Dispose();
#endif
                    mutex.Close();
                    mutex = null;
                }
            }, null);
        }

        static List<string> _promptStrings = new List<string>();
        public static void MemoPromptString(string strText)
        {
            _promptStrings.Add(strText);
        }

        public static string GetPromptStringLines()
        {
            return StringUtil.MakePathList(_promptStrings, "\r\n\r\n");
        }

        public static void ClearPromptStringLines()
        {
            _promptStrings.Clear();
        }

        public static void PromptAndExit(IWin32Window owner, string strText)
        {
            if (owner != null)
                MessageBox.Show(owner, strText);
            Program.MemoPromptString(strText);
            Application.Exit();
        }

        // 准备接管未捕获的异常
        static void PrepareCatchException()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

#if NO
        static void End()
        {
            Application.ThreadException -= Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
#endif

        static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            // MainForm main_form = Form.ActiveForm as MainForm;
#if NO
            string strError = "发生未捕获的异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            if (main_form != null)
                main_form.WriteErrorLog(strError);
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);
#endif
            string strError = GetExceptionText(ex, "");

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思
            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2Circulation 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序",
    "dp2Circulation 发生未知的异常",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bSendReport,
    new string[] { "关闭" },
    "将信息发送给开发者");
#if NO
            if (result == DialogResult.Yes)
            {
                    bExiting = true;
                    Application.Exit();
            }
#endif

            // 发送异常报告
            if (bSendReport)
                CrashReport(strError);
        }

        static string GetExceptionText(Exception ex, string strType)
        {
            // Exception ex = (Exception)e.Exception;
            string strError = "发生未捕获的"+strType+"异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            Assembly myAssembly = Assembly.GetAssembly(typeof(Program));
            strError += "\r\ndp2Circulation 版本: " + myAssembly.FullName;
            strError += "\r\n操作系统：" + Environment.OSVersion.ToString();
            strError += "\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

            // TODO: 给出操作系统的一般信息

            // MainForm main_form = Form.ActiveForm as MainForm;
            if (_mainForm != null)
            {
                try
                {
                    _mainForm.WriteErrorLog(strError);
                }
                catch
                {
                    WriteWindowsLog(strError, EventLogEntryType.Error);
                }
            }
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);

            return strError;
        }

        static void Application_ThreadException(object sender, 
            ThreadExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            // MainForm main_form = Form.ActiveForm as MainForm;
#if NO
            string strError = "发生未捕获的界面线程异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            if (main_form != null)
                main_form.WriteErrorLog(strError);
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);

            Assembly myAssembly = Assembly.GetAssembly(typeof(Program));
            strError += "\r\n\r\ndp2Circulation 版本:" + myAssembly.FullName;
#endif
            string strError = GetExceptionText(ex, "界面线程");

            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2Circulation 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n是否关闭程序?",
    "dp2Circulation 发生未知的异常",
    MessageBoxButtons.YesNo,
    MessageBoxDefaultButton.Button2,
    ref bSendReport,
    new string[] { "关闭", "继续" },
    "将信息发送给开发者");
            {
                if (bSendReport)
                    CrashReport(strError);
            }
            if (result == DialogResult.Yes)
            {
                //End();
                _bExiting = true;
                Application.Exit();
            }
        }

        static void CrashReport(string strText)
        {
            // MainForm main_form = Form.ActiveForm as MainForm;

            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            //_messageBar.BackColor = SystemColors.Info;
            //_messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2Circulation 出现异常";
            _messageBar.MessageText = "正在向 dp2003.com 发送异常报告 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            _messageBar.Show(_mainForm);
            _messageBar.Update();

            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                if (_mainForm != null)
                    strSender = _mainForm.GetCurrentUserName() + "@" + _mainForm.ServerUID;
                // 崩溃报告
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2circulation",
                    strText,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                nRet = -1;
            }
            finally
            {
                _messageBar.Close();
                _messageBar = null;
            }

            if (nRet == -1)
            {
                strError = "向 dp2003.com 发送异常报告时出错，未能发送成功。详细情况: " + strError;
                MessageBox.Show(_mainForm, strError);
                // 写入错误日志
                if (_mainForm != null)
                    _mainForm.WriteErrorLog(strError);
                else
                    WriteWindowsLog(strError, EventLogEntryType.Error);
            }
        }

        // 写入 Windows 系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog("Application");
            Log.Source = "dp2Circulation";
            Log.WriteEntry(strText, type);
        }
    }
}