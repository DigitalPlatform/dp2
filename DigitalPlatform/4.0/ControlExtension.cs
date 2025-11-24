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
        public static void SetText(this TextBoxBase textbox, string text)
        {
            textbox.Text = text;
            // textbox.Focus();
            /*
            textbox.SelectionLength = 0;
            textbox.SelectionStart = textbox.Text.Length;
            */
        }

        // 2025/11/22
        // 如果 form .Visbile 为 false，确保从当前 Application 打开的 Form 中找到返回一个 .Visible 为 true 的 form
        public static Form EnsureVisible(this Form form)
        {
            if (form.Visible == false)
                return GetVisibleForm();
            return form;
        }

        // 2025/11/22
        public static Form GetVisibleForm()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form != null && form.Visible)
                    return form;
            }
            return null;
        }

        public static DialogResult MessageBoxShow(
            this Control form,
            string strText)
        {
            if (form.IsHandleCreated == false
                || form.Visible == false)
            {
                var caller = form;
                if (caller.IsHandleCreated == false)
                    caller = GetVisibleForm();
                
                return caller.TryGet(() =>
                {
                    try
                    {
                        // 2025/11/22
                        return MessageBox.Show(GetVisibleForm(), strText);
                    }
                    catch (ObjectDisposedException)
                    {
                        return DialogResult.Abort;
                    }
                });
            }
            else
                return form.TryGet(() =>
                {
                    try
                    {
                        // 2025/11/22
                        if (form.Visible == false)
                            return MessageBox.Show(GetVisibleForm(), strText);
                        else
                            return MessageBox.Show(form, strText);
                    }
                    catch (ObjectDisposedException)
                    {
                        return DialogResult.Abort;
                    }
                });
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
