using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// Form 的一些扩展函数
    /// </summary>
    public static class ControlExtension
    {

        public static void MessageBoxShow(
            this Control form, 
            string strText)
        {
            if (form.IsHandleCreated)
                form.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(form, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }

        public static T TryGet<T>(
            this Control form,
            Func<T> func)
        {
            if (form.InvokeRequired)
            {
                return (T)form.Invoke((Func<T>)(() =>
                {
                    return func.Invoke();
                }));
            }
            else
                return func.Invoke();
        }

        // 用于确保在界面线程调用
        public static void TryInvoke(
            this Control form,
            Action method)
        {
            if (form.InvokeRequired)
                form.Invoke((Action)(method));
            else
                method.Invoke();
        }

        // 根据 uiThread 是否为 true，决定是否要确保在 UI 线程调用
        public static void TryInvoke(
            this Control form,
            bool uiThread,
            Action method)
        {
            if (form.InvokeRequired && uiThread)
                form.Invoke((Action)(method));
            else
                method.Invoke();
        }
    }
}
