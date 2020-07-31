using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.IO;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 和 Windows Form 有关的前端实用函数
    /// </summary>
    public static class FormClientUtil
    {
        // parameters:
        //      bLocal  是否从本地启动。 false 表示连安装带启动
        public static void StartDp2libraryXe(
            IWin32Window owner,
            string strDialogTitle,
            Font font,
            bool bLocal)
        {
            MessageBar messageBar = null;

            messageBar = new MessageBar();
            messageBar.TopMost = false;
            if (font != null)
                messageBar.Font = font;
            messageBar.BackColor = SystemColors.Info;
            messageBar.ForeColor = SystemColors.InfoText;
            messageBar.Text = "dp2 内务";
            messageBar.MessageText = "正在启动 dp2Library XE V3，请等待 ...";
            messageBar.StartPosition = FormStartPosition.CenterScreen;
            messageBar.Show(owner);
            messageBar.Update();

            Application.DoEvents();
            try
            {
                TimeSpan waitTime = new TimeSpan(0, 1, 0);

                string strShortcutFilePath = "";
                if (bLocal == true)
                    strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V3/dp2Library XE V3");
                else
                {
                    strShortcutFilePath = "http://dp2003.com/dp2libraryxe/v3/dp2libraryxe.application";
                    waitTime = new TimeSpan(0, 5, 0);  // 安装需要的等待时间更长
                }

                // TODO: detect if already started
                using (EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset,
                    "dp2libraryXE V3 library host started"))
                {
                    Application.DoEvents();

                    Process.Start(strShortcutFilePath);

                    DateTime start = DateTime.Now;
                    while (true)
                    {
                        Application.DoEvents();
                        // wait till started
                        // http://stackoverflow.com/questions/6816782/windows-net-cross-process-synchronization
                        if (eventWaitHandle.WaitOne(100, false) == true)
                            break;

                        // if timeout, prompt continue wait
                        if (DateTime.Now - start > waitTime)
                        {
                            DialogResult result = MessageBox.Show(owner,
    "dp2libraryXE V3 暂时没有响应。\r\n\r\n是否继续等待其响应?",
    strDialogTitle,
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                break;

                            start = DateTime.Now;   // 
                        }

                    }
                }
            }
            finally
            {
                messageBar.Close();
            }
        }

        public static bool HasDp2libraryXeStarted()
        {
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true, "dp2libraryXE V3", out createdNew))
            {
                if (createdNew)
                {
                    return false;
                }
                else
                {
#if NO
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            API.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
#endif
                    return true;
                }
            }
        }

    }
}
