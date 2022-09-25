using Microsoft.Win32.SafeHandles;
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
        // public static TimeSpan FirstWaitLength = TimeSpan.FromSeconds(30);

        public delegate void delegate_writeLog(ProcessInfo info, string text);

        public static void Start(
            List<ProcessInfo> process_infos,
            delegate_writeLog writeLog,
            CancellationToken token)
        {
            // 首次等待
            TimeSpan wait_time = TimeSpan.FromSeconds(30);

            Task.Run(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        foreach (ProcessInfo info in process_infos)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            // 观察上一次启动的 Process 是否结束
                            if (info.Process != null)
                            {
                                var ret = info.Process.WaitForExit(0);
                                if (ret == false)
                                {
                                    // Proccess 尚未结束
                                    continue;
                                }

                                // Process 已经结束
                                info.DisposeProcess();
                            }

                            if (HasModuleStarted(info.MutexName) == true)
                                continue;

                            try
                            {
                                // 启动
                                // 注意这个过程可能会因为被杀毒软件拦截而变得很长
                                var process = StartModule(info.ShortcutPath,
                                    "minimize");
                                info.Process = process;
                                if (process != null)
                                    writeLog?.Invoke(info, "进程被重新启动");
                            }
                            catch (Exception ex)
                            {
                                writeLog?.Invoke(info, $"StartModule() {info.ShortcutPath} 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }
                        }

                        // 延时
                        try
                        {
                            await Task.Delay(// TimeSpan.FromMilliseconds(1000), 
                                wait_time,
                                token);
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    Dispose();
                }
            });

            void Dispose()
            {
                foreach (var info in process_infos)
                {
                    if (info.Process != null)
                    {
                        info.DisposeProcess();
                    }
                }
            }
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

        // 2021/4/9
        // 是否为指纹 Url。主要用来区分掌纹和指纹
        public static bool IsFingerprintUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            return url.ToLower().Contains("fingerprint");
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

        public static Process StartModule(
            string shortcut_path,
            string arguments)
        {
            string strShortcutFilePath = PathUtil.GetShortcutFilePath(
                    shortcut_path
                    // "DigitalPlatform/dp2 V3/dp2Library XE V3"
                    );

            if (File.Exists(strShortcutFilePath) == false)
                return null;

            // testing
            // strShortcutFilePath = "notepad";
            // arguments = "";

            // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
            var info = new ProcessStartInfo(strShortcutFilePath, arguments);
            var process = Process.Start(info);
            process.EnableRaisingEvents = true;
            return process;
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

        // 2022/9/24
        public Process Process { get; set; }

        public void DisposeProcess()
        {
            if (Process != null)
            {
                Process.Dispose();
                Process = null;
            }
        }
    }
}
