using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

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
            if (form == null)
            {
                method?.Invoke();
                return;
            }
            if (form.InvokeRequired)
                form.Invoke((Action)(method));
            else
                method.Invoke();
        }

        /// <summary>
        /// 在指定控件上覆盖显示一个新的只读 TextBox，用来显示报错信息。
        /// 新 TextBox 的大小、字体与被覆盖控件一致，并放到被覆盖控件之上。
        /// 返回新创建的 TextBox 控件。
        /// </summary>
        /// <param name="target">要被覆盖的控件</param>
        /// <param name="errorText">要显示的错误文本</param>
        /// <returns>新创建并显示的 TextBox</returns>
        public static TextBox ShowErrorOverlay(this Control target, string errorText)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            TextBox overlay = null;

            // Ensure run on UI thread of the target
            target.TryInvoke(() =>
            {
                Control parent = target.Parent;
                Point location;

                if (parent == null)
                {
                    // fall back to form if parent is null
                    Form form = target.FindForm();
                    if (form != null)
                    {
                        parent = form;
                        // translate target location to form client coordinates
                        Point screen = target.PointToScreen(Point.Empty);
                        location = form.PointToClient(screen);
                    }
                    else
                    {
                        // ultimate fallback
                        parent = target;
                        location = target.Location;
                    }
                }
                else
                {
                    // location relative to parent
                    location = target.Location;
                }

                overlay = new TextBox();
                overlay.Multiline = true;
                overlay.ReadOnly = true;
                overlay.Text = errorText ?? string.Empty;
                overlay.Font = target.Font;
                overlay.Size = target.Size;
                overlay.Location = location;
                overlay.Anchor = target.Anchor;
                overlay.Dock = DockStyle.None;
                overlay.BorderStyle = BorderStyle.FixedSingle;
                overlay.BackColor = Color.DarkRed;  //  Color.MistyRose;
                overlay.ForeColor = Color.White;    //  Color.Black;
                overlay.TabStop = false;
                overlay.Name = "errorOverlay_" + Guid.NewGuid().ToString("N");

                // make sure overlay is placed above the target
                parent.Controls.Add(overlay);
                overlay.BringToFront();
            });

            return overlay;
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
