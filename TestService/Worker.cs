using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Test();
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("running ...");
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
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

    }
}
