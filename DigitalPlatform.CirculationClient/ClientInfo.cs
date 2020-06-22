using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Xml;

using Serilog;

// using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Core;
using DigitalPlatform.Xml;
using Serilog.Core;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 存储各种程序信息的全局类
    /// </summary>
    public static class ClientInfo
    {
        public static string ProgramName { get; set; }

        public static Form MainForm { get; set; }


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

        /// <summary>
        /// 人脸本地缓存目录
        /// </summary>
        public static string FaceCacheDir(string strUrl)
        {
            string strServerUrl = GetValidPathString(strUrl.Replace("/", "_"));

            return Path.Combine(UserDir, "facecache\\" + strServerUrl);
        }

        public static string Lang = "zh";

        public static string ProductName = "";

        // https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/
        static LoggingLevelSwitch _loggingLevel = new LoggingLevelSwitch();

        public static void SetLogginLevel(Serilog.Events.LogEventLevel level)
        {
            _loggingLevel.MinimumLevel = level;
        }

        // return:
        //      true    不检查序列号
        public delegate bool Delegate_skipSerialNumberCheck();

        // parameters:
        //      product_name    例如 "fingerprintcenter"
        public static void Initial(string product_name, 
            Delegate_skipSerialNumberCheck skipCheck = null)
        {
            ProductName = product_name;
            ClientVersion = Assembly.GetAssembly(TypeOfProgram).GetName().Version.ToString();

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

            UserTempDir = Path.Combine(UserDir, "temp");
            PathUtil.TryCreateDir(UserTempDir);

            UserLogDir = Path.Combine(UserDir, "log");
            PathUtil.TryCreateDir(UserLogDir);

            InitialConfig();

#if NO
            var repository = log4net.LogManager.CreateRepository("main");
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(UserLogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            LibraryChannelManager.Log = LogManager.GetLogger("main", "channellib");
            _log = LogManager.GetLogger("main",
                product_name
                // "fingerprintcenter"
                );
#endif
            Log.Logger = new LoggerConfiguration()
// .MinimumLevel.Information()
.MinimumLevel.ControlledBy(_loggingLevel)
.WriteTo.File(Path.Combine(UserLogDir, "log_.txt"), rollingInterval: RollingInterval.Day)
.CreateLogger();

            // 启动时在日志中记载当前 .exe 版本号
            // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
            WriteInfoLog(Assembly.GetAssembly(typeof(ClientInfo)).FullName);

            {
                // 检查序列号
                // if (DateTime.Now >= start_day || this.MainForm.IsExistsSerialNumberStatusFile() == true)
                if (SerialNumberMode == "must"
                    && (skipCheck == null || skipCheck() == false))
                {
                    // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                    // this.WriteSerialNumberStatusFile();

                    int nRet = VerifySerialCode($"{product_name}需要先设置序列号才能使用",
                        "",
                        "reinput",
                        out string strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(MainForm, $"{product_name}需要先设置序列号才能使用");
                        API.PostMessage(MainForm.Handle, API.WM_CLOSE, 0, 0);
                        return;
                    }
                }
            }
        }

        public static void Finish()
        {
            SaveConfig();
        }

        #region Log

#if REMOVED
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
            Log.Debug(strText);
        }

        // 写入错误日志文件
        // parameters:
        //      level   info/error
        // Exception:
        //      可能会抛出异常
        public static void WriteLog(string level, string strText)
        {
#if REMOVED
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
            if (level == "info")
                Log.Information(strText);
            else
                Log.Error(strText);
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
            Assembly myAssembly = Assembly.GetAssembly(TypeOfProgram);
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

#if NO
            try
            {
                _config = ConfigSetting.Open(filename, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"配置文件 {filename} 装载失败：{ex.Message}");
                _config = ConfigSetting.Create(filename);
            }
#endif
        }

        // 可反复调用
        public static void SaveConfig()
        {
#if NO
            // Save the configuration file.
            if (_config != null)
            {
                _config.Save();
                _config = null;
            }
#endif
            // Save the configuration file.
            if (_config != null && _config.Changed == true)
                _config.Save();
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


#region 序列号

        // parameters:
        //      strRequirFuncList   要求必须具备的功能列表。逗号间隔的字符串
        //      strStyle    风格
        //                  reinput    如果序列号不满足要求，是否直接出现对话框让用户重新输入序列号
        //                  reset   执行重设序列号任务。意思就是无论当前序列号是否可用，都直接出现序列号对话框
        // return:
        //      -1  出错
        //      0   正确
        public static int VerifySerialCode(
            string strTitle,
            string strRequirFuncList,
            string strStyle,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            bool bReinput = StringUtil.IsInList("reinput", strStyle);
            bool bReset = StringUtil.IsInList("reset", strStyle);

            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strSerialCode = ClientInfo.Config.Get("sn", "sn", "");

            // 首次运行
            if (string.IsNullOrEmpty(strSerialCode) == true)
            {
            }

        REDO_VERIFY:
            if (bReset == false
                && SerialNumberMode != "must"
                && strSerialCode == "community")
            {
                if (string.IsNullOrEmpty(strRequirFuncList) == true)
                {
                    CommunityMode = true;
                    ClientInfo.Config.Set("main_form", "last_mode", "community");
                    return 0;
                }
            }

            if (bReset == true
                || CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false
                || MatchLocalString(strSerialCode) == false
                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (bReinput == false && bReset == false)
                {
                    strError = "序列号无效";
                    return -1;
                }

                if (bReset == false)
                {
                    if (String.IsNullOrEmpty(strSerialCode) == false)
                        MessageBox.Show(MainForm, "序列号无效。请重新输入");
                    else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                        MessageBox.Show(MainForm, "序列号中 function 参数无效。请重新输入");
                }

                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    strTitle,
                    false,
                    strSerialCode,
                    GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "放弃";
                    return -1;
                }
                strSerialCode = ClientInfo.Config.Get("sn", "sn", "");
                bReset = false;
                goto REDO_VERIFY;
            }
            return 0;
        }

        static bool _communityMode = false;

        public static bool CommunityMode
        {
            get
            {
                return _communityMode;
            }
            set
            {
                _communityMode = value;
                SetTitle();
            }
        }

        public static string SerialNumberMode = ""; // 序列号模式。空表示不需要序列号。"must" 要求需要序列号；"loose" 不要序列号也可运行，但高级功能需要序列号。loose 和 must 都会出现“设置序列号”菜单命令
        public static string CopyrightKey = "";    // "dp2catalog_sn_key";

        static void SetTitle()
        {
#if NO
            if (this.CommunityMode == true)
                this.Text = "dp2Catalog V3 -- 编目 [社区版]";
            else
                this.Text = "dp2Catalog V3 -- 编目 [专业版]";
#endif
        }

        // return:
        //      0   Cancel
        //      1   OK
        static int ResetSerialCode(
            string strTitle,
            bool bAllowSetBlank,
            string strOldSerialCode,
            string strOriginCode)
        {
            Hashtable ext_table = StringUtil.ParseParameters(strOriginCode);
            string strMAC = (string)ext_table["mac"];
            if (string.IsNullOrEmpty(strMAC) == true)
                strOriginCode = "!error";
            else
            {
                Debug.Assert(string.IsNullOrEmpty(CopyrightKey) == false);
                strOriginCode = Cryptography.Encrypt(strOriginCode,
                CopyrightKey);
            }
            SerialCodeForm dlg = new SerialCodeForm();
            dlg.Text = strTitle;
            dlg.Font = MainForm.Font;
            if (SerialNumberMode == "loose")
                dlg.DefaultCodes = new List<string>(new string[] { "community|社区版" });
            dlg.SerialCode = strOldSerialCode;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.OriginCode = strOriginCode;

        REDO:
            dlg.ShowDialog(MainForm);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            if (string.IsNullOrEmpty(dlg.SerialCode) == true)
            {
                if (bAllowSetBlank == true)
                {
                    DialogResult result = MessageBox.Show(MainForm,
        $"确实要将序列号设置为空?\r\n\r\n(一旦将序列号设置为空，{ProductName} 将自动退出，下次启动需要重新设置序列号)",
        $"{ProductName}",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        return 0;
                    }
                }
                else
                {
                    MessageBox.Show(MainForm, "序列号不允许为空。请重新设置");
                    goto REDO;
                }
            }

            ClientInfo.Config.Set("sn", "sn", dlg.SerialCode);
            ClientInfo.Config.Save();
            return 1;
        }

        // 将本地字符串匹配序列号
        static bool MatchLocalString(string strSerialNumber)
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            foreach (string mac in macs)
            {
                string strLocalString = GetEnvironmentString(mac);
                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    return true;
            }

            if (DateTime.Now.Month == 12)
            {
                foreach (string mac in macs)
                {
                    string strLocalString = GetEnvironmentString(mac, true);
                    string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                    if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                        return true;
                }
            }

            return false;
        }


        // return:
        //      false   不满足
        //      true    满足
        static bool CheckFunction(string strEnvString,
            string strFuncList)
        {
            Hashtable table = StringUtil.ParseParameters(strEnvString);
            string strFuncValue = (string)table["function"];
            string[] parts = strFuncList.Split(new char[] { ',' });
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part) == true)
                    continue;
                if (StringUtil.IsInList(part, strFuncValue) == false)
                    return false;
            }

            return true;
        }

        // parameters:
        static string GetEnvironmentString(string strMAC,
            bool bNextYear = false)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  //  SerialCodeForm.GetMacAddress();
            if (bNextYear == false)
                table["time"] = SerialCodeForm.GetTimeRange();
            else
                table["time"] = SerialCodeForm.GetNextYearTimeRange();

            table["product"] = ProductName;

            string strSerialCode = ClientInfo.Config.Get("sn", "sn", "");
            // 将 strSerialCode 中的扩展参数设定到 table 中
            SerialCodeForm.SetExtParams(ref table, strSerialCode);
            return StringUtil.BuildParameterString(table);
        }

        // 获得 xxx|||xxxx 的左边部分
        static string GetCheckCode(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strSN;
        }

        // 获得 xxx|||xxxx 的右边部分
        static string GetExtParams(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strExtParam;
        }

#endregion

#region Form 实用函数

        public delegate void delegate_action(object o);

        public static void ProcessControl(Control control,
            delegate_action action)
        {
            action(control);
            ProcessChildren(control, action);
        }

        static void ProcessChildren(Control parent,
            delegate_action action)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                action(sub);

                if (sub is ToolStrip)
                {
                    ProcessToolStrip((ToolStrip)sub, action);
                }

                if (sub is SplitContainer)
                {
                    ProcessSplitContainer(sub as SplitContainer, action);
                }

                // 递归
                ProcessChildren(sub, action);
            }
        }

        static void ProcessToolStrip(ToolStrip tool,
delegate_action action)
        {
            List<ToolStripItem> items = new List<ToolStripItem>();
            foreach (ToolStripItem item in tool.Items)
            {
                items.Add(item);
            }

            foreach (ToolStripItem item in items)
            {
                action(item);

                if (item is ToolStripMenuItem)
                {
                    ProcessDropDownItemsFont(item as ToolStripMenuItem, action);
                }
            }
        }

        static void ProcessDropDownItemsFont(ToolStripMenuItem menu,
            delegate_action action)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            foreach (ToolStripItem item in menu.DropDownItems)
            {

                action(item);

                if (item is ToolStripMenuItem)
                {
                    ProcessDropDownItemsFont(item as ToolStripMenuItem, action);
                }
            }
        }

        static void ProcessSplitContainer(SplitContainer container,
            delegate_action action)
        {
            action(container.Panel1);

            foreach (Control control in container.Panel1.Controls)
            {
                ProcessChildren(control, action);
            }

            action(container.Panel2);

            foreach (Control control in container.Panel2.Controls)
            {
                ProcessChildren(control, action);
            }
        }

#endregion

#region 错误状态

        static void SetWholeColor(Color backColor, Color foreColor)
        {
            MainForm?.Invoke((Action)(() =>
            {
                ClientInfo.ProcessControl(MainForm,
                    (o) =>
                    {
                        dynamic d = o;
                        d.BackColor = backColor;
                        d.ForeColor = foreColor;
                    });

#if NO
                this.BackColor = backColor;
                this.ForeColor = foreColor;
                foreach (TabPage page in this.tabControl_main.TabPages)
                {
                    page.BackColor = backColor;
                    page.ForeColor = foreColor;
                }
                this.toolStrip1.BackColor = backColor;
                this.toolStrip1.ForeColor = foreColor;

                this.menuStrip1.BackColor = backColor;
                this.menuStrip1.ForeColor = foreColor;

                this.statusStrip1.BackColor = backColor;
                this.statusStrip1.ForeColor = foreColor;
#endif
            }));
        }

        // 错误状态
        static string _errorState = "normal";    // error/retry/normal
        // 错误状态描述
        static string _errorStateInfo = "";

        public static string ErrorState
        {
            get
            {
                return _errorState;
            }
        }

        public static string ErrorStateInfo
        {
            get
            {
                return _errorStateInfo;
            }
        }

        public static void SetErrorState(string state, string info)
        {
            if (state == "error")   // 出现错误，后面不再会重试
                SetWholeColor(Color.DarkRed, Color.White);
            else if (state == "retry")   // 出现错误，但后面会自动重试
                SetWholeColor(Color.DarkOrange, Color.Black);
            else if (state == "normal")  // 没有错误
                SetWholeColor(SystemColors.Window, SystemColors.WindowText);
            else
                throw new Exception($"无法识别的 state={state}");

            _errorState = state;
            _errorStateInfo = info;
        }

#endregion

        public static bool IsMinimizeMode()
        {
            try
            {
                // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
                var args = AppDomain.CurrentDomain?.SetupInformation?.ActivationArguments?.ActivationData[0];
                // List<string> args = StringUtil.GetCommandLineArgs();
                if (args == null)
                    return false;
                return args?.IndexOf("minimize") != -1;
            }
            catch
            {
                return false;
            }
        }

        /*
         * <config amerce_interface="<无>" im_server_url="http://dp2003.com:8083/dp2MServer" green_package_server_url="" pinyin_server_url="http://dp2003.com/dp2library" gcat_server_url="http://dp2003.com/dp2library" circulation_server_url="net.pipe://localhost/dp2library/XE"/>
         * <default_account tempCode="" phoneNumber="" occur_per_start="true" location="" isreader="false" savepassword_long="true" savepassword_short="true" username="supervisor" password="Z7RAQEBWFmBcKM8mFvOjwg=="/>
         * */
        public static int GetDp2circulationUserName(
            out string url,
            out string userName,
            out string password,
            out bool savePassword,
            out string strError)
        {
            strError = "";
            url = "";
            userName = "";
            password = "";
            savePassword = false;

            string strXmlFilename = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "dp2circulation_v2\\dp2circulation.xml");
            if (File.Exists(strXmlFilename) == false)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            try
            {
                dom.Load(strXmlFilename);
            }
            catch (Exception ex)
            {
                strError = $"打开 XML 文件失败: {ex.Message}";
                return -1;
            }

            {
                XmlElement default_account = dom.DocumentElement.SelectSingleNode("default_account") as XmlElement;
                if (default_account != null)
                {
                    userName = default_account.GetAttribute("username");
                    savePassword = DomUtil.GetBooleanParam(default_account, "savepassword_long", false);
                    if (savePassword == true)
                    {
                        string password_text = default_account.GetAttribute("password");
                        password = DecryptDp2circulationPasssword(password_text);
                    }
                }
            }

            {
                XmlElement config = dom.DocumentElement.SelectSingleNode("config") as XmlElement;
                if (config != null)
                {
                    url = config.GetAttribute("circulation_server_url");
                }
            }

            return 1;
        }

        internal static string DecryptDp2circulationPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        "dp2circulation_client_password_key");
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        public static void MemoryState(Form form, string section, string entry)
        {
            form.Load += (s1, e1) =>
            {
                var state = ClientInfo.Config.Get(section, entry, "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    FormProperty.SetProperty(state, form, ClientInfo.IsMinimizeMode());
                }
            };
            form.FormClosed += (s1, e1) =>
            {
                var state = FormProperty.GetProperty(form);
                ClientInfo.Config.Set(section, entry, state);
            };
        }
    }
}
