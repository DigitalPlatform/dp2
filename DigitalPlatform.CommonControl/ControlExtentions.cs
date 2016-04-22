using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
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
