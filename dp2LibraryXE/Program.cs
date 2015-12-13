using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
// using Microsoft.VisualBasic.ApplicationServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;
using System.Deployment.Application;
using System.IO;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2LibraryXE
{
    static class Program
    {
        /// <summary>
        /// 前端，也就是 dp2libraryxe.exe 的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        static bool bExiting = false;

        static MainForm _mainForm = null;
        // 这里用 _mainForm 存储窗口对象，不采取 Form.ActiveForm 获取的方式。原因如下
        // http://stackoverflow.com/questions/17117372/form-activeform-occasionally-works
        // Form.ActiveForm occasionally works

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

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
"dp2LibraryXE_v1");
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
                    MessageBox.Show("dp2library XE 无法运行。\r\n\r\n因为监听 net.pipe 或 http 协议的需要，必须在 Administrator 权限下才能运行");
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
                using (Mutex mutex = new Mutex(true, "dp2libraryXE V1", out createdNew))
                {
                    if (createdNew)
                    {
#if NO
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm());
                        // SingleInstanceApplication.Run(new MainForm(), StartupNextInstanceHandler);
#endif
                        if (IsDevelopMode() == false)
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

        public static bool IsDevelopMode()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = 0;
            foreach (string arg in args)
            {
                if (i > 0 && arg == "develop")
                    return true;
                i++;
            }

            return false;
        }

        // 准备接管未捕获的异常
        static void PrepareCatchException()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        static void CurrentDomain_UnhandledException(object sender,
    UnhandledExceptionEventArgs e)
        {
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            string strError = GetExceptionText(ex, "");

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思
            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2LibraryXE 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序",
    "dp2LibraryXE 发生未知的异常",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bSendReport,
    new string[] { "关闭" },
    "将信息发送给开发者");
            // 发送异常报告
            if (bSendReport)
                CrashReport(strError);
        }

        static string GetExceptionText(Exception ex, string strType)
        {
            // Exception ex = (Exception)e.Exception;
            string strError = "发生未捕获的" + strType + "异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            Assembly myAssembly = Assembly.GetAssembly(typeof(Program));
            strError += "\r\ndp2LibraryXE 版本: " + myAssembly.FullName;
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
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            string strError = GetExceptionText(ex, "界面线程");

            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2LibraryXE 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n是否关闭程序?",
    "dp2LibraryXE 发生未知的异常",
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
                bExiting = true;
                Application.Exit();
            }
        }

        static string GetMacAddressString()
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            return StringUtil.MakePathList(macs);
        }

        static void CrashReport(string strText)
        {
            // MainForm main_form = Form.ActiveForm as MainForm;

            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            //_messageBar.BackColor = SystemColors.Info;
            //_messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2LibraryXE 出现异常";
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
                {
                    string strUid = _mainForm.GetLibraryXmlUid();
                    if (string.IsNullOrEmpty(strUid) == false)
                        strSender = "@" + strUid;
                    else
                        strSender = "@MAC:" + GetMacAddressString();
                }
                else
                    strSender = "@MAC:" + GetMacAddressString();

                // 崩溃报告
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2libraryxe",
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

        // 写入Windows系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog("Application");
            Log.Source = "dp2LibraryXE";
            Log.WriteEntry(strText, type);
        }

    }

#if NO
    public class SingleInstanceApplication : WindowsFormsApplicationBase 
    { 
        private SingleInstanceApplication() 
        { 
            base.IsSingleInstance = true; 
        } 
        public static void Run(Form f, 
            StartupNextInstanceEventHandler startupHandler)
        { 
            SingleInstanceApplication app = new SingleInstanceApplication();
            app.MainForm = f;
            app.StartupNextInstance += startupHandler;
            app.Run(Environment.GetCommandLineArgs());
        }
    }
#endif
}
