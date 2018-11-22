using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using log4net;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace FingerprintCenter
{
    /// <summary>
    /// 存储各种程序信息的全局类
    /// </summary>
    public static class ClientInfo
    {
        public static string ProgramName { get; set; }

        public static MainForm MainForm { get; set; }

        /// <summary>
        /// 前端，的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        /// <summary>
        /// 数据目录
        /// </summary>
        public static string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public static string UserDir = "";

        /// <summary>
        /// 错误日志目录
        /// </summary>
        public static string UserLogDir = "";

        // 附加的一些文件名非法字符。比如 XP 下 Path.GetInvalidPathChars() 不知何故会遗漏 '*'
        static string spec_invalid_chars = "*?:";

        public static string GetValidPathString(string strText, string strReplaceChar = "_")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char[] invalid_chars = Path.GetInvalidPathChars();
            StringBuilder result = new StringBuilder();
            foreach (char c in strText)
            {
                if (c == ' ')
                    continue;
                if (IndexOf(invalid_chars, c) != -1
                    || spec_invalid_chars.IndexOf(c) != -1)
                    result.Append(strReplaceChar);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        static int IndexOf(char[] chars, char c)
        {
            int i = 0;
            foreach (char c1 in chars)
            {
                if (c1 == c)
                    return i;
                i++;
            }

            return -1;
        }

        /// <summary>
        /// 指纹本地缓存目录
        /// </summary>
        public static string FingerPrintCacheDir(string strUrl)
        {
            string strServerUrl = GetValidPathString(strUrl.Replace("/", "_"));

            return Path.Combine(UserDir, "fingerprintcache\\" + strServerUrl);
        }

        public static string Lang = "zh";

        // parameters:
        //      product_name    例如 "fingerprintcenter"
        public static void Initial(string product_name)
        {
            ClientVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            UserDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    product_name);
            PathUtil.TryCreateDir(UserDir);

            UserLogDir = Path.Combine(UserDir, "log");
            PathUtil.TryCreateDir(UserLogDir);

            InitialConfig();

            var repository = log4net.LogManager.CreateRepository("main");
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(UserLogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            LibraryChannelManager.Log = LogManager.GetLogger("main", "channellib");
            _log = LogManager.GetLogger("main", "fingerprintcenter");

            // 启动时在日志中记载当前 .exe 版本号
            // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
            WriteInfoLog(Assembly.GetAssembly(typeof(ClientInfo)).FullName);
        }

        public static void Finish()
        {
            SaveConfig();
        }

        #region Log

        static ILog _log = null;

        public static ILog Log
        {
            get
            {
                return _log;
            }
        }

        // 写入错误日志文件
        public static void WriteErrorLog(string strText)
        {
            WriteLog("error", strText);
        }

        public static void WriteInfoLog(string strText)
        {
            WriteLog("info", strText);
        }

        // 写入错误日志文件
        // parameters:
        //      level   info/error
        // Exception:
        //      可能会抛出异常
        public static void WriteLog(string level, string strText)
        {
            // Console.WriteLine(strText);

            if (_log == null) // 先前写入实例的日志文件发生过错误，所以改为写入 Windows 日志。会加上实例名前缀字符串
                WriteWindowsLog(strText, EventLogEntryType.Error);
            else
            {
                // 注意，这里不捕获异常
                if (level == "info")
                    _log.Info(strText);
                else
                    _log.Error(strText);
            }
        }

        // 写入 Windows 日志
        public static void WriteWindowsLog(string strText)
        {
            WriteWindowsLog(strText, EventLogEntryType.Error);
        }

        #endregion

        #region 未捕获的异常处理 

        // 准备接管未捕获的异常
        public static void PrepareCatchException()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        static bool _bExiting = false;   // 是否处在 正在退出 的状态

        static void CurrentDomain_UnhandledException(object sender,
    UnhandledExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            string strError = GetExceptionText(ex, "");

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思
            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(MainForm,
    $"{ProgramName} 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序",
    $"{ProgramName} 发生未知的异常",
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
            strError += $"\r\n{ProgramName} 版本: " + myAssembly.FullName;
            strError += "\r\n操作系统：" + Environment.OSVersion.ToString();
            strError += "\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

            // TODO: 给出操作系统的一般信息

            // MainForm.WriteErrorLog(strError);
            return strError;
        }

        static void Application_ThreadException(object sender,
    ThreadExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            string strError = GetExceptionText(ex, "界面线程");

            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(MainForm,
    $"{ProgramName} 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n是否关闭程序?",
    $"{ProgramName} 发生未知的异常",
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
                _bExiting = true;
                Application.Exit();
            }
        }

        static void CrashReport(string strText)
        {
            MessageBar _messageBar = null;

            _messageBar = new MessageBar
            {
                TopMost = false,
                Text = $"{ProgramName} 出现异常",
                MessageText = "正在向 dp2003.com 发送异常报告 ...",
                StartPosition = FormStartPosition.CenterScreen
            };
            _messageBar.Show(MainForm);
            _messageBar.Update();

            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                if (MainForm != null)
                    strSender = MainForm.GetCurrentUserName() + "@" + MainForm.ServerUID;
                // 崩溃报告
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    $"{ProgramName}",
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
                MessageBox.Show(MainForm, strError);
                // 写入错误日志
                WriteErrorLog(strError);
            }
        }

        // 写入 Windows 系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            try
            {
                EventLog Log = new EventLog("Application");
                Log.Source = $"{ProgramName}";
                Log.WriteEntry(strText, type);
            }
            catch
            {

            }
        }

        #endregion

        static ConfigSetting _config = null;

        public static ConfigSetting Config
        {
            get
            {
                return _config;
            }
        }

        public static void InitialConfig()
        {
            if (string.IsNullOrEmpty(UserDir))
                throw new ArgumentException("UserDir 尚未初始化");

            string filename = Path.Combine(UserDir, "settings.xml");
            _config = ConfigSetting.Open(filename, true);
        }

        public static void SaveConfig()
        {
            // Save the configuration file.
            if (_config != null)
            {
                _config.Save();
                _config = null;
            }
        }

        public static void AddShortcutToStartupGroup(string strProductName)
        {
            if (ApplicationDeployment.IsNetworkDeployed &&
                ApplicationDeployment.CurrentDeployment != null &&
                ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {

                string strTargetPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                strTargetPath = Path.Combine(strTargetPath, strProductName) + ".appref-ms";

                string strSourcePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                strSourcePath = Path.Combine(strSourcePath, strProductName) + ".appref-ms";

                File.Copy(strSourcePath, strTargetPath, true);
            }
        }

        public static void RemoveShortcutFromStartupGroup(string strProductName)
        {
            if (ApplicationDeployment.IsNetworkDeployed &&
    ApplicationDeployment.CurrentDeployment != null &&
    ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {

                string strTargetPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                strTargetPath = Path.Combine(strTargetPath, strProductName) + ".appref-ms";

                try
                {
                    File.Delete(strTargetPath);
                }
                catch
                {

                }
            }
        }

    }
}
