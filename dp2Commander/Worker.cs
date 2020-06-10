using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using murrayju.ProcessExtensions;

using GreenInstall;
using DigitalPlatform;
using DigitalPlatform.MessageClient;

namespace dp2Commander
{
    public class Worker // : BackgroundService
    {
        // private readonly ILogger<Worker> _logger;
        P2PConnection _connection = new P2PConnection();
        string _userName = "";

#if OLD
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
#endif

        #region TOPSHELF

        private string[] args;

        public Worker(string[] vs)
        {
            args = vs;
        }

        // 检查网络连接的间隔时间
        static TimeSpan _idleLength = TimeSpan.FromMinutes(5);   // 5 // TimeSpan.FromSeconds(10);

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Enter ExecuteAsync -----");

            /*
            EventLog logListener = new EventLog("Security");
            logListener.EntryWritten += LogListener_EntryWritten;
            logListener.EnableRaisingEvents = true;
            */

            WriteInfoLog("开始工作");
            _connection.AddMessage += _connection_AddMessage;
            /*
            Test();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            ExitProcess("dp2SSL");
            */

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Console.WriteLine("running ...");
                    await EnsureConnectMessageServerAsync();
                    // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    await Task.Delay(_idleLength, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                WriteErrorLog($"ExecuteAsync() 内循环体出现异常: {ExceptionUtil.GetDebugText(ex)}"); ;
            }

            _connection.AddMessage -= _connection_AddMessage;
            _connection?.CloseConnection();
            WriteInfoLog("结束工作");
        }

        private void LogListener_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            //4624: An account was successfully logged on.
            //4625: An account failed to log on.
            //4648: A logon was attempted using explicit credentials.
            //4675: SIDs were filtered.
            var events = new int[] { 4624, 4625, 4648, 4675 };
            if (Array.IndexOf(events, e.Entry.EventID) != -1)
                WriteInfoLog($"event message: EventID:{e.Entry.EventID},InstanceID:{e.Entry.InstanceId}, TimeGenerated:{e.Entry.TimeGenerated}, Message:{e.Entry.Message}");

            // WriteInfoLog($"event message: EventID:{e.Entry.EventID},InstanceID:{e.Entry.InstanceId}, Message:{e.Entry.Message}");
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public void Start()
        {
            WriteInfoLog("Service Start");

            _cancel?.Cancel();
            _cancel = new CancellationTokenSource();

            Task.Factory.StartNew(
                async () =>
                {
                    await ExecuteAsync(_cancel.Token);
                },
                _cancel.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Stop()
        {
            WriteInfoLog("Service Stop");

            _cancel?.Cancel();
        }

        #endregion

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
                    await ProcessAndReply(e.Records);
                });
            }
        }

        async Task ProcessAndReply(List<MessageRecord> records)
        {
            try
            {
                List<MessageRecord> new_messages = new List<MessageRecord>();
                foreach (var record in records)
                {
                    var result_text = ProcessCommand(record.data);
                    if (result_text == null)
                        continue;

                    WriteInfoLog($"响应文字 '{result_text}'");
                    new_messages.Add(new MessageRecord
                    {
                        groups = record.groups,
                        data = result_text
                    });
                }

                if (new_messages.Count > 0)
                {
                    SetMessageRequest param = new SetMessageRequest("create",
            "dontNotifyMe",
            new_messages);

                    var result = await _connection.SetMessageAsyncLite(param);
                    if (result.Value == -1)
                    {
                        WriteErrorLog($"reply message error: {result.ErrorInfo}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog($"ProcessAndReply() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        string ProcessCommand(string text)
        {
            if (text.StartsWith($"@{_userName}") == false)
                return null;

            string command = text.Substring($"@{_userName}".Length).Trim();

            WriteInfoLog($"收到命令 {text}");

            if (command.StartsWith("hello"))
            {
                return "hello!";
            }

            /*
            if (command.StartsWith("test"))
            {
                //bool existence = Directory.Exists("c:\\用户\\xietao\\dp2ssl");
                //return $"OK={existence}";
                DirectoryInfo di = new DirectoryInfo("c:\\Users");
                var fis = di.GetFiles();
                StringBuilder temp = new StringBuilder();
                foreach (var fi in fis)
                {
                    temp.AppendLine(fi.FullName);
                }
                return temp.ToString();
            }
            */

            // 重启电脑
            if (command.StartsWith("restart"))
            {
                return ProcessRestart(command);
            }

            return $"未知的命令 '{command}'";
        }

        string ProcessRestart(string command)
        {
            // 子参数
            string param = command.Substring("restart".Length).Trim();
            if ( // string.IsNullOrEmpty(param)
                param.ToLower() == "computer")
            {
                // 重启电脑
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(1000);
                        // https://stackoverflow.com/questions/4286354/restart-computer-from-winforms-app
                        ProcessStartInfo proc = new ProcessStartInfo();
                        proc.WindowStyle = ProcessWindowStyle.Hidden;
                        proc.FileName = "cmd";
                        proc.Arguments = "/C shutdown -f -r";
                        Process.Start(proc);
                    }
                    catch
                    {

                    }
                });
                return "服务器将在一秒后重新启动";
            }
            if (param.ToLower() == "dp2ssl")
            {
                // 重启 dp2ssl
                ExitProcess("dp2SSL", false);
                Thread.Sleep(5000);
                if (HasModuleStarted(MutexName) == true)
                {
                    ExitProcess("dp2SSL", true);
                }


                try
                {
                    // 启动
                    // StartModule(ShortcutPath, "");
                    // Process.Start("c:\\dp2ssl\\dp2ssl.exe");
                    string exe_path1 = "c:\\dp2ssl\\greensetup.exe";
                    string exe_path2 = "c:\\dp2ssl\\dp2ssl.exe";
                    if (File.Exists(exe_path1))
                        ProcessExtensions.StartProcessAsCurrentUser(exe_path1);
                    else if (File.Exists(exe_path2))
                    {
                        // 启动之前，检查 .zip 是否已经展开
                        {
                            string binDir = "c:\\dp2ssl";
                            // *** 检查状态文件
                            // result.Value
                            //      -1  出错
                            //      0   不存在状态文件
                            //      1   正在下载 .zip 过程中。.zip 不完整
                            //      2   当前 .zip 和 .exe 已经一样新
                            //      3   当前 .zip 比 .exe 要新。需要展开 .zip 进行更新安装
                            //      4   下载 .zip 失败。.zip 不完整
                            //      5   当前 .zip 比 .exe 要新，需要重启计算机以便展开的文件生效
                            var check_result = GreenInstaller.CheckStateFile(binDir);
                            // 展开
                            if (check_result.Value == 3)
                            {
                                var extract_result = GreenInstaller.ExtractFiles(binDir);
                                if (extract_result.Value == -1)
                                {
                                    // TODO: 写入错误日志
                                    WriteErrorLog($"展开压缩文件时出错: {extract_result.ErrorInfo}");
                                }
                            }
                        }

                        ProcessExtensions.StartProcessAsCurrentUser(exe_path2);
                    }
                    else
                        return $"{exe_path1} 和 {exe_path2} 均未找到，无法启动";
                    return "dp2SSL 已经重新启动";
                }
                catch(Exception ex)
                {
                    return $"启动过程出现异常: {ExceptionUtil.GetDebugText(ex)}";
                }
            }

            return $"命令 '{command}' 未知的子参数 '{param}'";
        }

        static bool _warning = false;

        // 确保连接到消息服务器
        public async Task EnsureConnectMessageServerAsync()
        {
            try
            {
                var account_result = Program.GetMessageAccount();

                if (string.IsNullOrEmpty(account_result.Url) == true)
                {
                    if (_warning == false)
                    {
                        WriteInfoLog("警告: 尚未设置 dp2mserver 服务器 url 和其他参数");
                        _warning = true;
                    }
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
                        WriteErrorLog($"连接消息服务器失败: {connect_result.ErrorInfo}。url={account_result.Url},userName={account_result.UserName},errorCode={connect_result.ErrorCode}");
                    else
                        WriteInfoLog($"连接消息服务器 {account_result.Url} 成功");

                    _userName = account_result.UserName;

                    /*
                    if (connect_result.Value == -1)
                        WpfClientInfo.WriteErrorLog($"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");
                    else
                    {
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog($"EnsureConnectMessageServerAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
        public static bool ExitProcess(string processName, bool kill)
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
                if (kill == true || process.CloseMainWindow() == false)
                {
                    process.Kill(true);
                }
                count++;
            }

            return count != 0;
        }

        static void WriteErrorLog(string text)
        {
            // Log.Logger.Write(LogEventLevel.Error, text);

            Program.WriteErrorLog("[ERROR] " + text);
            //Program.ILog.Error(text);

            Console.WriteLine(text);
        }

        static void WriteInfoLog(string text)
        {
            //Log.Logger.Write(LogEventLevel.Error, text);

            Program.WriteErrorLog("[INFO] " + text);
            //Program.ILog.Information(text);

            Console.WriteLine(text);
        }
    }

}
