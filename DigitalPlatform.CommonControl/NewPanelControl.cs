using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    public partial class NewPanelControl : Panel
    {
        public NewPanelControl()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            RectangleF rect = new RectangleF(10, 10,
                this.Size.Width, this.Size.Height);
            using (Pen pen = new Pen(Color.Gray))
            {
                RoundRectangle(pe.Graphics,
                    pen, null, rect, 10);
            }

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            RectangleF rect,
            float radius)
        {
            RoundRectangle(graphics,
                pen,
                brush,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                radius);
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLine(x + radius, y, x + width - (radius * 2), y);
                path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
                path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
                path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
                path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
                path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
                path.AddLine(x, y + height - (radius * 2), x, y + radius);
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.CloseFigure();
                if (brush != null)
                    graphics.FillPath(brush, path);
                if (pen != null)
                    graphics.DrawPath(pen, path);
            }
        }
    }
}
