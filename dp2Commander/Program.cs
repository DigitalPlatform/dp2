using System;
using System.Reflection;
using System.IO;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using DigitalPlatform.Core;
using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Commander
{
    class Program
    {
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

            // 处理一般管理命令
            // 设置消息服务器参数

            CreateHostBuilder(args).Build().Run();
        }

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
