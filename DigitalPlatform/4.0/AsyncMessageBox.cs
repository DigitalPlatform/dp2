using System;
using System.Windows.Forms;

namespace DigitalPlatform
{

    public static class AsyncMessageBox
    {
        // 用于仅作通知、不需要返回值的场景
        public static void ShowAsync(IWin32Window owner, string text, string caption = "")
        {
            if (owner is Control ctrl)
            {
                // Ensure delegate runs after current Load/painting completes
                ctrl.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show(ctrl, text, caption);
                }));
            }
            else
            {
                // 如果没有 owner 或 owner 不是 Control，退化为线程池调度
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    MessageBox.Show(text, caption);
                });
            }
        }

        // 可选：同步显示（保留原语义），供需要返回值的调用使用
        public static DialogResult Show(IWin32Window owner, string text, string caption = "")
        {
            if (owner is Control ctrl)
            {
                if (ctrl.InvokeRequired)
                    return (DialogResult)ctrl.Invoke((Func<DialogResult>)(() => MessageBox.Show(ctrl, text, caption)));
                else
                    return MessageBox.Show(ctrl, text, caption);
            }
            else
            {
                return MessageBox.Show(text, caption);
            }
        }
    }
}
