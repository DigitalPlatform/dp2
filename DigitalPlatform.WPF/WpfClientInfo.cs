using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DigitalPlatform;

#if LOG4NET
using log4net;
#endif

using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using System.Runtime.CompilerServices;
using Serilog;

namespace DigitalPlatform.WPF
{
    /// <summary>
    /// WPF 前端的通用功能
    /// </summary>
    public static class WpfClientInfo
    {
        public static string Lang = "zh";

        public static string ProductName = "";

        public static string ProgramName { get; set; }

        public static Window MainWindow { get; set; }

        /// <summary>
        /// 前端，的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        public static Type TypeOfProgram { get; set; }

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

        /// <summary>
        /// 临时文件目录
        /// </summary>
        public static string UserTempDir = "";


        // parameters:
        //      product_name    例如 "fingerprintcenter"
        public static void Initial(string product_name)
        {
            ProductName = product_name;
            ClientVersion = Assembly.GetAssembly(TypeOfProgram).GetName().Version.ToString();

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                // DataDir = Application.LocalUserAppDataPath;
                DataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            else
            {
                DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            UserDir = null;

            // 从 DataDir 中尝试寻找 userDirectory.txt 文件
            {
                string filename = Path.Combine(DataDir, "userDirectory.txt");
                if (File.Exists(filename))
                {
                    string value = File.ReadAllText(filename);
                    if (string.IsNullOrEmpty(value) == false)
                        UserDir = value;
                }
            }

            if (string.IsNullOrEmpty(UserDir))
            {
                UserDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        product_name);
                PathUtil.TryCreateDir(UserDir);

                // 检查文件夹里面是否有 userDirectoryMask.txt 文件
                string filename = Path.Combine(UserDir, "userDirectoryMask.txt");
                if (File.Exists(filename))
                {
                    throw new Exception($"当前用户文件夹 {UserDir} 已经被标记为废弃状态。{product_name} 放弃启动");
                }
            }

            UserTempDir = Path.Combine(UserDir, "temp");
            PathUtil.TryCreateDir(UserTempDir);

            UserLogDir = Path.Combine(UserDir, "log");
            PathUtil.TryCreateDir(UserLogDir);

            InitialConfig();

#if LOG4NET
            var repository = log4net.LogManager.CreateRepository("main");
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(UserLogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            LibraryChannelManager.Log = LogManager.GetLogger("main", "channellib");
            _log = LogManager.GetLogger("main", ProductName);
#endif

            {
                Log.Logger = new LoggerConfiguration()
.MinimumLevel.Debug()
// .WriteTo.Console()
.WriteTo.File(Path.Combine(UserLogDir, "log_.txt"), rollingInterval: RollingInterval.Day)
// .WriteTo.File("log\\log_.txt", )
.CreateLogger();
            }

            // 启动时在日志中记载当前 .exe 版本号
            // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
            WriteInfoLog(Assembly.GetAssembly(TypeOfProgram).FullName);

#if NO
            {
                // 检查序列号
                // if (DateTime.Now >= start_day || this.MainForm.IsExistsSerialNumberStatusFile() == true)
                if (SerialNumberMode == "must")
                {
                    // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                    // this.WriteSerialNumberStatusFile();

                    string strError = "";
                    int nRet = VerifySerialCode($"{product_name}需要先设置序列号才能使用",
                        "",
                        "reinput",
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(MainForm, $"{product_name}需要先设置序列号才能使用");
                        API.PostMessage(MainForm.Handle, API.WM_CLOSE, 0, 0);
                        return;
                    }
                }
            }
#endif
        }

        public static void Finish(delegate_fileCreated func_fileCreated)
        {
            SaveConfig(func_fileCreated);
        }

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

        // 配置文件被首次创建成功
        public delegate void delegate_fileCreated(string filename);

        // 允许反复调用
        public static void SaveConfig(delegate_fileCreated func_fileCreated)
        {
            // Save the configuration file.
            if (_config != null && _config.Changed)
            {
                bool exists = string.IsNullOrEmpty(_config.FileName) == false && File.Exists(_config.FileName);

                _config.Save();

                // 首次创建文件
                if (exists == false
                    && string.IsNullOrEmpty(_config.FileName) == false && File.Exists(_config.FileName))
                {
                    func_fileCreated?.Invoke(_config.FileName);
                }
            }
        }

        #region Log

#if LOG4NET
        static ILog _log = null;

        public static ILog Log
        {
            get
            {
                return _log;
            }
        }
#endif

        // 写入错误日志文件
        public static void WriteErrorLog(string strText)
        {
            WriteLog("error", strText);
        }

        public static void WriteInfoLog(string strText)
        {
            WriteLog("info", strText);
        }

        public static void WriteDebugLog(string strText)
        {
            WriteLog("debug", strText);
        }

        public static event WriteLogEventHandler WriteLogEvent = null;

        // 写入错误日志文件
        // parameters:
        //      level   info/error
        // Exception:
        //      可能会抛出异常
        public static void WriteLog(string level, string strText)
        {
            // Console.WriteLine(strText);

#if LOG4NET
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
#endif
            // 2021/8/20
            if (WriteLogEvent != null)
            {
                WriteLogEventArgs e = new WriteLogEventArgs { Level = level, Message = strText };
                WriteLogEvent?.Invoke(null, e);
                if (e.Cancelled)
                {
                    return;
                }
            }

            if (level == "info")
                Log.Information(strText);
            else if (level == "debug")
                Log.Debug(strText);
            else
                Log.Error(strText);
        }

        // 写入 Windows 日志
        public static void WriteWindowsLog(string strText)
        {
            WriteWindowsLog(strText, EventLogEntryType.Error);
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

        #region 未捕获的异常处理 

        // 准备接管未捕获的异常
        public static void PrepareCatchException()
        {
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

            MessageWindow message_dlg = new MessageWindow();
            message_dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            message_dlg.title.Text = $"{ProgramName} 发生未知的异常";
            message_dlg.text.Text = $"{ProgramName} 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序";
            message_dlg.Closed += new EventHandler(delegate (object o1, EventArgs e1)
            {
            });
            // message_dlg.Owner = App.CurrentApp.MainWindow;
            message_dlg.ShowDialog();
            // 发送异常报告
            if ((bool)message_dlg.sendReport.IsChecked)
                CrashReport(strError);

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思

            // 自动重启应用
            // System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
            // Application.Current.MainWindow.Close();
        }

        static string GetExceptionText(Exception ex, string strType)
        {
            // Exception ex = (Exception)e.Exception;
            string strError = "发生未捕获的" + strType + "异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            Assembly myAssembly = Assembly.GetAssembly(TypeOfProgram);
            strError += $"\r\n{ProgramName} 版本: " + myAssembly.FullName;
            strError += "\r\n操作系统：" + Environment.OSVersion.ToString();
            // strError += "\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

            // TODO: 给出操作系统的一般信息

            // MainForm.WriteErrorLog(strError);
            return strError;
        }

        static void CrashReport(string strText)
        {
            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                //if (MainForm != null)
                //    strSender = MainForm.GetCurrentUserName() + "@" + MainForm.ServerUID;
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

            if (nRet == -1)
            {
                strError = "向 dp2003.com 发送异常报告时出错，未能发送成功。详细情况: " + strError;
                MessageBox.Show(strError);
                // 写入错误日志
                WriteErrorLog(strError);
            }
        }

        #endregion

        // result.Value:
        //      -1  出错
        //      0   没有发现新版本
        //      1   发现新版本，重启后可以使用新版本
        public static NormalResult InstallUpdateSync()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Boolean updateAvailable = false;
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    updateAvailable = ad.CheckForUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    // This exception occurs if a network error or disk error occurs
                    // when downloading the deployment.
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "The application cannt check for the existence of a new version at this time. \n\nPlease check your network connection, or try again later. Error: " + dde
                    };
                }
                catch (InvalidDeploymentException ide)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "The application cannot check for an update. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message
                    };
                }
                catch (InvalidOperationException ioe)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "This application cannot check for an update. This most often happens if the application is already in the process of updating. Error: " + ioe.Message
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "检查更新出现异常: " + ExceptionUtil.GetDebugText(ex)
                    };
                }

                if (updateAvailable == false)
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "没有发现更新"
                    };
                try
                {
                    ad.Update();
                    return new NormalResult
                    {
                        Value = 1,
                        ErrorInfo = "自动更新完成。重启可使用新版本"
                    };
                    // Application.Restart();
                }
                catch (DeploymentDownloadException dde)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "Cannot install the latest version of the application. Either the deployment server is unavailable, or your network connection is down. \n\nPlease check your network connection, or try again later. Error: " + dde.Message
                    };
                }
                catch (TrustNotGrantedException tnge)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "The application cannot be updated. The system did not grant the application the appropriate level of trust. Please contact your system administrator or help desk for further troubleshooting. Error: " + tnge.Message
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "自动更新出现异常: " + ExceptionUtil.GetDebugText(ex)
                    };
                }
            }

            return new NormalResult();
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

                try
                {
                    File.Copy(strSourcePath, strTargetPath, true);
                }
                catch (System.IO.FileNotFoundException)
                {
                    // source 文件有可能不存在
                }
            }
        }

        // parameters:
        //      force 是否强制删除？如果为 false 表示不强制删除。在不是强制删除的情况下，应当满足 ClickOnce 启动并且是安装更新后第一次启动运行本函数才有效
        public static void RemoveShortcutFromStartupGroup(string strProductName,
            bool force = false)
        {
            if (force
                || (ApplicationDeployment.IsNetworkDeployed &&
    ApplicationDeployment.CurrentDeployment != null &&
    ApplicationDeployment.CurrentDeployment.IsFirstRun))
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

    public delegate void WriteLogEventHandler(object sender,
WriteLogEventArgs e);

    public class WriteLogEventArgs : EventArgs
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public bool Cancelled { get; set; }    // [out]
    }
}
