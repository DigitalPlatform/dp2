using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// Control 的扩展方法
    /// </summary>
    public static class ControlExtention
    {
        #region 不会被自动 Dispose 的 子 Control，放在这里托管，避免内存泄漏

        public static void AddFreeControl(List<Control> controls,
            Control control)
        {
            if (controls.IndexOf(control) == -1)
                controls.Add(control);
        }

        public static void RemoveFreeControl(List<Control> controls,
            Control control)
        {
            controls.Remove(control);
        }

        public static void DisposeFreeControls(List<Control> controls)
        {
            foreach (Control control in controls)
            {
                control.Dispose();
            }
            controls.Clear();
        }

        #endregion

        // 清除控件集合。使用这个函数便于 Dispose() 所清除的控件对象
        public static void ClearControls(this Control parent,
            bool bDispose = false)
        {
            if (parent.Controls.Count == 0)
                return;

            List<Control> controls = new List<Control>();
            foreach (Control control in parent.Controls)
            {
                controls.Add(control);
            }

            parent.Controls.Clear();
            if (bDispose)
            {
                foreach (Control control in controls)
                {
                    control.Dispose();
                }
            }
        }

        public static string GetText(this Control control)
        {
            if (control.InvokeRequired == false)
                return control.Text;
            string strText = "";
            control.Invoke((Action)(() =>
            {
                strText = control.Text;
            }));
            return strText;
        }

        public static void SetText(this Control control, string strText)
        {
            if (control.InvokeRequired == false)
                control.Text = strText;
            else
            {
                try
                {
                    control.Invoke((Action)(() =>
                    {
                        control.Text = strText;
                    }));
                }
                catch(ObjectDisposedException)
                {

                }
            }
        }
    }

    /// <summary>
    /// ScrollableControl 的扩展方法
    /// </summary>
    public static class ScrollableControlExtention
    {
        public static void SetAutoScrollPosition(this ScrollableControl control, Point p)
        {
            if (p.Y + control.ClientSize.Height > control.DisplayRectangle.Height)
                p.Y = control.DisplayRectangle.Height - control.ClientSize.Height;
            if (p.X + control.ClientSize.Width > control.DisplayRectangle.Width)
                p.X = control.DisplayRectangle.Width - control.ClientSize.Width;
            control.AutoScrollPosition = new Point(p.X, p.Y);
        }
    }

}
