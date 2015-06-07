using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 用于增加册记录的加号按钮
    /// </summary>
    public class PlusButton : Button
    {
        bool _active = false;
        // 是否处在激活(焦点)状态?
        // 这只是一个显示状态，即便没有真正在 Focus 状态也显示蓝色
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                this.Invalidate();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            this.Active = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            // e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (this.Focused == true || this.Active)
            {
                using (Brush brush = new SolidBrush(this.Focused == true ? SystemColors.Highlight : ControlPaint.Dark(SystemColors.Highlight)))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }

            int x_unit = this.ClientSize.Width / 3;
            int y_unit = this.ClientSize.Height / 3;

            Color darker_color = ControlPaint.Dark(this.ForeColor);
            // 绘制一个十字形状
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                Rectangle rect = new Rectangle(x_unit, y_unit + y_unit / 2 - y_unit / 8, x_unit, y_unit / 4);
                e.Graphics.FillRectangle(brush, rect);
                rect = new Rectangle(x_unit + x_unit / 2 - x_unit / 8, y_unit, x_unit / 4, y_unit);
                e.Graphics.FillRectangle(brush, rect);
            }

            // 绘制一个圆圈
            using (Pen pen = new Pen(darker_color, x_unit / 8))
            {
                Rectangle rect = new Rectangle(x_unit / 2, y_unit / 2, this.ClientSize.Width - x_unit, this.ClientSize.Height - y_unit);
                e.Graphics.DrawArc(pen, rect, 0, 360);
            }
        }
    }
}
