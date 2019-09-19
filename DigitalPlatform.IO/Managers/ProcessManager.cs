using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    // 负责监控和重启进程
    public static class ProcessManager
    {
        public delegate void delegate_writeLog(ProcessInfo info, string text);

        public static void Start(
            List<ProcessInfo> process_infos,
            delegate_writeLog writeLog,
            CancellationToken token)
        {
            TimeSpan wait_time = TimeSpan.FromSeconds(30);

            Task.Run(() =>
            {
                while (token.IsCancellationRequested == false)
                {
                    // 延时
                    try
                    {
                        Task.Delay(// TimeSpan.FromMilliseconds(1000), 
                            wait_time,
                            token).Wait();
                    }
                    catch
                    {
                        return;
                    }

                    foreach (ProcessInfo info in process_infos)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        if (HasModuleStarted(info.MutexName) == true)
                            continue;

                        // TODO: 写入日志
                        writeLog?.Invoke(info, "进程被重新启动");

                        // 启动
                        StartModule(info.ShortcutPath, "minimize");
                    }
                }
            });
        }

        // 判断一个 URL 是否为 ipc 协议
        // ipc://RfidChannel/RfidServer
        public static bool IsIpcUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                if (uri.Scheme == "ipc")
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
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
            string strShortcutFilePath = PathUtil.GetShortcutFilePath(
                    shortcut_path
                    // "DigitalPlatform/dp2 V3/dp2Library XE V3"
                    );

            if (File.Exists(strShortcutFilePath) == false)
                return false;

            // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
            Process.Start(strShortcutFilePath, arguments);
            return true;
        }

    }

    public class ProcessInfo
    {
        // 用于显示的名称
        public string Name { get; set; }

        // 快捷方式路径
        public string ShortcutPath { get; set; }

        // Mutex 名
        public string MutexName { get; set; }
    }
}
