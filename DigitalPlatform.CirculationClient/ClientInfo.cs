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
using System.Threading.Tasks;

using Serilog;
using Serilog.Core;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Core;
using DigitalPlatform.Xml;
using Newtonsoft.Json;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 存储各种程序信息的全局类
    /// </summary>
    public static class ClientInfo
    {
        // 配置文件发生了重新装载(在监控模式下)
        public static event EventHandler ConfigReloaded;

        public static string ProgramName { get; set; }

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

        public static void SetLoggingLevel(Serilog.Events.LogEventLevel level)
        {
            _loggingLevel.MinimumLevel = level;
        }

        // return:
        //      true    不检查序列号
        public delegate bool Delegate_skipSerialNumberCheck();

        // parameters:
        //      product_name    例如 "fingerprintcenter"
        //      style           service 表示用户文件夹使用 service 位置
        //                      watcher 表示监控配置文件变化，一旦发生变化自动重装进入内存
        //                      logReload 表示要在日志文件中记载配置文件变化后的内容。须和 wathcer 配套使用
        // return:
        //      true    初始化成功
        //      false   初始化失败，应立刻退出应用程序
        public static bool Initial(string product_name,
            string style = "")
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

            if (StringUtil.IsInList("service", style))
            {
                UserDir = GetServiceUserDirectory(product_name);
                PathUtil.TryCreateDir(UserDir);
            }
            else
            {
                UserDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        product_name);
                PathUtil.TryCreateDir(UserDir);
            }

            UserTempDir = Path.Combine(UserDir, "temp");
            PathUtil.TryCreateDir(UserTempDir);

            UserLogDir = Path.Combine(UserDir, "log");
            PathUtil.TryCreateDir(UserLogDir);

            InitialConfig(style);

            Log.Logger = new LoggerConfiguration()
// .MinimumLevel.Information()
.MinimumLevel.ControlledBy(_loggingLevel)
.WriteTo.File(Path.Combine(UserLogDir, "log_.txt"), rollingInterval: RollingInterval.Day)
.CreateLogger();

            // 启动时在日志中记载当前 .exe 版本号
            // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
            WriteInfoLog((IntPtr.Size == 8 ? "x64" : "x86") + " " +
                Assembly.GetAssembly(TypeOfProgram/*typeof(ClientInfo)*/).FullName
                );
            return true;
        }

        public static void Finish()
        {
            // 2021/3/3
            // 2021/9/8 调整到 SaveConfig() 之前
            EndWather();

            SaveConfig();
        }

        // 获得一个 Service 产品的用户目录
        public static string GetServiceUserDirectory(
            string strProduct,
            string strCompany = "dp2")
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), $"{strCompany}\\{strProduct}");
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
            WriteLog("debug", strText);

            // Log.Debug(strText);
        }

        // 写入错误日志文件
        // parameters:
        //      level   info/error/debug
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


        static ConfigSetting _config = null;

        public static ConfigSetting Config
        {
            get
            {
                return _config;
            }
        }

        // parameters:
        //      style   风格。如果包含 watcher，表示会启用文件变化监视功能，每当物理文件变化的时候，会自动重装进入 ConfigSetting 内存
        public static void InitialConfig(string style)
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
            if (StringUtil.IsInList("watcher", style))
            {
                EndWather();
                BeginWatcher(style);
            }
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

        #region 物理文件变化监控

        static FileSystemWatcher _watcher = null;
        static string _wacherStyle = "";

        // 监视文件变化
        // parameters:
        //      wacherStyle logReload 表示会在日志文件中记载新装入的配置文件内容
        static void BeginWatcher(string wacherStyle = "")
        {
            _wacherStyle = wacherStyle;

            _watcher = new FileSystemWatcher();
            _watcher.Path = Path.GetDirectoryName(_config.FileName);

            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes | NotifyFilters.FileName | NotifyFilters.CreationTime;

            _watcher.Filter = Path.GetFileName(_config.FileName);
            _watcher.IncludeSubdirectories = false;

            // Add event handlers.
            _watcher.Changed -= Watcher_Changed;
            _watcher.Changed += Watcher_Changed;

            // Begin watching.
            _watcher.EnableRaisingEvents = true;
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Changed) != WatcherChangeTypes.Changed)
                return;

            string filename = _config.FileName;
            if (PathUtil.IsEqual(filename, e.FullPath) == true)
            {
                for (int i = 0; i < 2; i++)
                {
                    // 稍微延时一下，避免很快地重装、正好和 尚在改写文件的的进程发生冲突
                    Thread.Sleep(500);

                    try
                    {
                        _config = ConfigSetting.Open(filename, true);

                        string logContent = "";
                        if (StringUtil.IsInList("logReload", _wacherStyle))
                            logContent = " 新内容:\r\n" + File.ReadAllText(filename);

                        if (i > 0)
                            WriteInfoLog($"配置文件 {filename} 重试(监控)重装成功{logContent}");
                        else
                            WriteInfoLog($"配置文件 {filename} (监控)重装成功{logContent}");

                        ConfigReloaded?.Invoke(_config, new EventArgs());
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteErrorLog($"配置文件 {filename} 在(监控)重装阶段出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }
            }
        }

        static void EndWather()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= Watcher_Changed;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        #endregion

        #region 使用计数

        class DailyCounter
        {
            public string Date { get; set; }
            public long Value { get; set; }
        }

        // 增量每日使用次数计数器
        // return:
        //      false   在限制范围内
        //      true    已经超出范围
        public static bool IncDailyCounter(
            string counter_name,
            long limit_value)
        {
            string today = DateTimeUtil.DateTimeToString8(DateTime.Now);
            
            string value = Config.Get("dailyCounter", counter_name);
            DailyCounter counter = JsonConvert.DeserializeObject<DailyCounter>(value);
            if (counter == null || counter.Date != today)
                counter = new DailyCounter { Date = DateTimeUtil.DateTimeToString8(DateTime.Now) };

            counter.Value++;

            value = JsonConvert.SerializeObject(counter);
            Config.Set("dailyCounter", counter_name, value);
            if (counter.Value > limit_value)
                return true;
            return false;
        }

        #endregion

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

        // parameters:
        //      warning_level   警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        public delegate void delegate_showText(string text, int warning_level);

        // 2020/9/17
        public static void BeginUpdate(
            TimeSpan firstDelay,
            TimeSpan idleLength,
            CancellationToken token,
            delegate_showText func_showText)
        {
            if (ApplicationDeployment.IsNetworkDeployed == false)
                return;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    // 第一次延迟
                    await Task.Delay(firstDelay, token);

                    while (token.IsCancellationRequested == false)
                    {
                        //      -1  出错
                        //      0   没有发现更新
                        //      1   已经更新，重启可使用新版本
                        NormalResult result = ClientInfo.InstallUpdateSync();
                        WriteInfoLog($"后台 ClickOnce 自动更新返回: {result.ToString()}");

                        if (result.Value == -1)
                            func_showText?.Invoke("自动更新出错: " + result.ErrorInfo, 2);
                        else if (result.Value == 1)
                        {
                            func_showText?.Invoke(result.ErrorInfo, 1);
                            return; // 只要更新了一次就返回
                        }
                        else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                            func_showText?.Invoke(result.ErrorInfo, 0);

                        // 以后的每次延迟
                        await Task.Delay(idleLength, token);
                    }
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    WriteErrorLog($"后台 ClickOnce 自动更新出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            },
    token,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
        }

        // result.Value:
        //      -1  出错
        //      0   没有发现更新
        //      1   已经更新，重启可使用新版本
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


        #region 错误状态


        // 错误状态
        internal static string _errorState = "normal";    // error/retry/normal
        // 错误状态描述
        internal static string _errorStateInfo = "";

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

        public static void ChangeErrorState(string state, string info)
        {
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
