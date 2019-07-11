using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
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

                        if (WpfClientInfo.HasModuleStarted(info.MutexName) == true)
                            continue;

                        // TODO: 写入日志
                        writeLog?.Invoke(info, "进程被重新启动");

                        // 启动
                        WpfClientInfo.StartModule(info.ShortcutPath);
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
