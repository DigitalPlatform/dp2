using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{
    public class DpiUtil
    {
        // 获得一个控件的 DPI 参数
        public static SizeF GetDpiXY(Control control)
        {
            // testing
            // return new SizeF(192, 192);

            using (Graphics g = control.CreateGraphics())
            {
                return new SizeF(g.DpiX, g.DpiY);
            }
        }

        // 将 96DPI 下的长宽数字转换为指定 DPI 下的长宽值
        public static Size GetScalingSize(SizeF dpi_xy, int x, int y)
        {
            int width = Convert.ToInt32(x * (dpi_xy.Width / 96F));
            int height = Convert.ToInt32(y * (dpi_xy.Height / 96F));
            return new Size(width, height);
        }

        public static void ScalingSize(SizeF dpi_xy, ref int x, ref int y)
        {
            x = Convert.ToInt32(x * (dpi_xy.Width / 96F));
            y = Convert.ToInt32(y * (dpi_xy.Height / 96F));
        }

        public static int GetScalingX(SizeF dpi_xy, int x)
        {
            return Convert.ToInt32(x * (dpi_xy.Width / 96F));
        }

        public static int GetScalingY(SizeF dpi_xy, int y)
        {
            return Convert.ToInt32(y * (dpi_xy.Height / 96F));
        }

        public static Rectangle GetScaingRectangle(SizeF dpi_xy, Rectangle rect)
        {
            return new Rectangle(
                Convert.ToInt32(rect.X * (dpi_xy.Width / 96F)),
                Convert.ToInt32(rect.Y * (dpi_xy.Height / 96F)),
                Convert.ToInt32(rect.Width * (dpi_xy.Width / 96F)), 
                Convert.ToInt32(rect.Height * (dpi_xy.Height / 96F))
            );
        }

        public static int Get96ScalingX(SizeF dpi_xy, int x)
        {
            return Convert.ToInt32(x * (96F / dpi_xy.Width));
        }

        public static int Get96ScalingY(SizeF dpi_xy, int y)
        {
            return Convert.ToInt32(y * (96F / dpi_xy.Height));
        }
    }
}
