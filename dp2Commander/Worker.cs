using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform;
using DigitalPlatform.MessageClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dp2Commander
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        P2PConnection _connection = new P2PConnection();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection.AddMessage += _connection_AddMessage;
            /*
            Test();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            ExitProcess("dp2SSL");
            */

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("running ...");
                await EnsureConnectMessageServerAsync();
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _connection?.CloseConnection();
        }

        private void _connection_AddMessage(object sender,
            DigitalPlatform.MessageClient.AddMessageEventArgs e)
        {
            if (e.Records != null)
            {
                foreach (var record in e.Records)
                {
                    Console.WriteLine($"message sender:{record.creator}, userName:{record.userName}, groups:{string.Join(",", record.groups)}, data:{record.data}");
                }

                Task.Run(async () =>
                {
                    await Reply(e.Records);
                });
            }
        }

        async Task Reply(List<MessageRecord> records)
        {
            try
            {
                List<MessageRecord> new_messages = new List<MessageRecord>();
                foreach (var record in records)
                {
                    new_messages.Add(new MessageRecord
                    {
                        groups = record.groups,
                        data = "reply:" + record.data
                    });
                }

                SetMessageRequest param = new SetMessageRequest("create",
        "dontNotifyMe",
        new_messages);

                var result = await _connection.SetMessageAsyncLite(param);
                if (result.Value == -1)
                {
                    Console.WriteLine($"reply error: {result.ErrorInfo}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"reply() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 确保连接到消息服务器
        public async Task EnsureConnectMessageServerAsync()
        {
            try
            {
                var account_result = Program.GetMessageAccount();

                if (string.IsNullOrEmpty(account_result.Url) == true)
                {
                    Console.WriteLine("警告: 尚未设置 dp2mserver 服务器 url 和其他参数");
                    return;
                }

                if (string.IsNullOrEmpty(account_result.Url) == false
                    && _connection.IsDisconnected)
                {
                    var connect_result = await _connection.ConnectAsync(account_result.Url,
                        account_result.UserName,
                        account_result.Password,
                        "");
                    if (connect_result.Value == -1)
                        Console.WriteLine($"连接消息服务器失败: {connect_result.ErrorInfo}。url={account_result.Url},userName={account_result.UserName},errorCode={connect_result.ErrorCode}");
                    else
                        Console.WriteLine("连接消息服务器成功");

                    /*
                    if (connect_result.Value == -1)
                        WpfClientInfo.WriteErrorLog($"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");
                    else
                    {
                    }
                    */
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"EnsureConnectMessageServerAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        static string ShortcutPath = "DigitalPlatform/dp2 V3/dp2SSL-自助借还";
        static string MutexName = "{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}";

        static bool Test()
        {
            if (HasModuleStarted(MutexName) == true)
                return false;

            // 启动
            StartModule(ShortcutPath, "minimize");
            return true;
        }

        public static bool HasModuleStarted(string mutex_name)
        {
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true,
                mutex_name, // "dp2libraryXE V3", 
                out createdNew))
            {
                if (createdNew)
                    return false;
                else
                    return true;
            }
        }

        public static bool StartModule(
            string shortcut_path,
            string arguments)
        {
            string strShortcutFilePath = GetShortcutFilePath(
                    shortcut_path
                    // "DigitalPlatform/dp2 V3/dp2Library XE V3"
                    );

            if (File.Exists(strShortcutFilePath) == false)
                return false;

            // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
            // Process.Start(strShortcutFilePath, arguments);

            // https://stackoverflow.com/questions/46808315/net-core-2-0-process-start-throws-the-specified-executable-is-not-a-valid-appl
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(strShortcutFilePath)
            {
                UseShellExecute = true,
                Arguments = arguments,
            };
            p.Start();
            return true;
        }

        // get clickonce shortcut filename
        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V3/dp2内务 V3"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

        // https://stackoverflow.com/questions/3411982/gracefully-killing-a-process
        public static bool ExitProcess(string processName)
        {
            int count = 0;
            foreach (var process in Process.GetProcessesByName(processName))
            {
                // 返回结果:
                //     true if the close message was successfully sent; false if the associated process
                //     does not have a main window or if the main window is disabled (for example if
                //     a modal dialog is being shown).
                //
                // 异常:
                //   T:System.InvalidOperationException:
                //     The process has already exited. -or- No process is associated with this System.Diagnostics.Process
                //     object.
                if (process.CloseMainWindow() == false)
                {
                    process.Kill(true);
                }
                count++;
            }

            return count != 0;
        }
    }

}
