using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
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

    /// <summary>
    /// TableLayoutPanel 扩展方法
    /// </summary>
    public static class TableLayoutPanelExtension
    {
        // 重新调整所有 TextBox 的高度
        public static void ResetAllTextBoxHeight(this TableLayoutPanel panel)
        {
            // 如果没有这一句，在不断宽/窄变换 tablelayoutpanel 宽度的时候，内容区下面的空白区域会逐渐变大
            panel.Height = 0;
            panel.SuspendLayout();

            foreach (Control control in panel.Controls)
            {
                if (control is AutoHeightTextBox && control.Visible == true)
                {
                    (control as AutoHeightTextBox).SetHeight();
                }
            }

            panel.ResumeLayout(false);
            panel.PerformLayout();
        }

        // http://stackoverflow.com/questions/7142138/tablelayoutpanel-getcontrolfromposition-does-not-get-non-visible-controls-how-d
        // TableLayoutPanel GetControlFromPosition does not get non-visible controls. How do you access a non-visible control at a specified position?
        public static Control GetAnyControlAt(this TableLayoutPanel panel, int column, int row)
        {
            {
                Control control = panel.GetControlFromPosition(column, row);
                if (control != null)
                    return control;
            }

            foreach (Control control in panel.Controls)
            {
                var cellPosition = panel.GetCellPosition(control);
                if (cellPosition.Column == column && cellPosition.Row == row)
                    return control;
            }
            return null;
        }

    }
}
