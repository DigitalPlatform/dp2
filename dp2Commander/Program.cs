using System;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using Topshelf;
using Serilog;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.Text;
// using log4net;

namespace dp2Commander
{
    public static class Program
    {
        public static Serilog.ILogger ILog { get; set; }

        static void Main(string[] args)
        {
            InitialConfig();

            // 修改配置
            if (args.Length == 1 && args[0].Equals("setting"))
            {
                var result = GetMessageAccount();

                Console.WriteLine("(直接回车表示不修改当前值)");
                Console.WriteLine("请输入 dp2mserver 服务器 URL: (当前值为 " + result.Url + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    result.Url = strValue;

                Console.WriteLine("请输入消息用户名: (当前值为 " + result.UserName + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    result.UserName = strValue;

                Console.WriteLine("请输入消息用户密码: ");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    result.Password = strValue;

                SetMessageAccount(result.Url, result.UserName, result.Password);

                SaveConfig();

                Console.WriteLine();
                Console.WriteLine("注：修改将在服务重启以后生效");
                Console.WriteLine("(按回车键返回)");
                Console.ReadLine();
                return;
            }

            {
                /*
                string dataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string logDir = Path.Combine(dataDir, "log");
                TryCreateDir(logDir);

                var repository = log4net.LogManager.CreateRepository("main");
                log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(logDir, "log_");
                log4net.Config.XmlConfigurator.Configure(repository);

                ILog = LogManager.GetLogger("main", "dp2Commander");
                */
                // https://michaelscodingspot.com/logging-in-dotnet/
                Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs\\.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

                // Serilog.Debugging.SelfLog.Enable(msg => WriteErrorLog(msg));

                Log.Information("test -------");
                ILog = Log.Logger;

                var rc = HostFactory.Run(x =>                                   //1
                {
                    x.Service<Worker>(s =>                                   //2
                    {
                        s.ConstructUsing(name => new Worker(args));                //3
                        s.WhenStarted(tc => tc.Start());                         //4
                        s.WhenStopped(tc => tc.Stop());                          //5
                    });
                    x.RunAsLocalSystem();                                       //6
                    x.SetDescription("dp2 远程控制器");                   //7
                    x.SetDisplayName("dp2 Commander Service");                                  //8
                    x.SetServiceName("dp2CommanderService");                                  //9
                    x.UseSerilog();
                });                                                             //10

                Log.CloseAndFlush();
                var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
                Environment.ExitCode = exitCode;
            }

            // 处理一般管理命令
            // 设置消息服务器参数

            // CreateHostBuilder(args).Build().Run();
        }

        public static bool TryCreateDir(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        static object _syncRoot_log = new object();

        public static void WriteErrorLog(string strText)
        {
            try
            {
                lock (_syncRoot_log)
                {
                    string dataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string logDir = Path.Combine(dataDir, "log");
                    TryCreateDir(logDir);

                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = Path.Combine(logDir, "log_" + DateTimeToString8(now) + ".txt");
                    string strTime = now.ToString();
                    File.AppendAllLines(strFilename, new string[] { strTime + " " + strText + "\r\n" });
                    //StreamUtil.WriteText(strFilename,
                    //    strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                // TODO: 要在安装程序中预先创建事件源
                // 代码可以参考 unhandle.txt (在本project中)

                /*
                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists("dp2library"))
                {
                    EventLog.CreateEventSource("dp2library", "DigitalPlatform");
                }*/

                EventLog Log = new EventLog();
                Log.Source = "dp2Commander";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        public static string DateTimeToString8(DateTime time)
        {
            return time.Year.ToString().PadLeft(4, '0')
                + time.Month.ToString().PadLeft(2, '0')
                + time.Day.ToString().PadLeft(2, '0');
        }

#if OLD
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(options => options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                    .Configure<EventLogSettings>(config =>
                    {
                        config.LogName = "dp2Commander Service";
                        config.SourceName = "dp2Commander";
                    });
            }).UseWindowsService();
#endif

        #region ConfigFile

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
            string strExePath = Assembly.GetExecutingAssembly().Location;

            string filename = Path.Combine(Path.GetDirectoryName(strExePath), "settings.xml");
            Console.WriteLine(filename);

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

        public class GetMessageAccountResult : NormalResult
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Url { get; set; }
        }

        const string _key = "dp2commanderkey";

        public static GetMessageAccountResult GetMessageAccount()
        {
            string url = Config.Get("messageAccount", "url");
            string userName = Config.Get("messageAccount", "userName");
            string password = Config.Get("messageAccount", "password");

            try
            {
                password = Cryptography.Decrypt(password, _key);
            }
            catch
            {
                password = "errorpassword";
            }

            return new GetMessageAccountResult
            {
                UserName = userName,
                Password = password,
                Url = url
            };
        }

        public static void SetMessageAccount(string url, string userName, string password)
        {
            Config.Set("messageAccount", "url", url);
            Config.Set("messageAccount", "userName", userName);

            password = Cryptography.Encrypt(password, _key);

            Config.Set("messageAccount", "password", password);
            Config.Save();
        }

        #endregion
    }
}
