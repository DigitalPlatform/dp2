using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 用于绘制记录合并关系的图像的类
    /// </summary>
    public class MergePicture
    {
        // 书目记录
        static int _BIBLIO_WIDTH = 95;
        static int _BIBLIO_HEIGHT = 50;

        // 下级记录
        static int _SUBREC_WIDTH = 90;
        static int _SUBREC_HEIGHT = 12;

        static int _SEP = 7; // 普通间隔距离
        static int _PIC_SEP = 30;

        static Color lineColor = Color.White;
        static Color leftColor = Color.DarkGreen;
        static Color rightColor = Color.DarkOrange;

        static int BIBLIO_WIDTH = _BIBLIO_WIDTH;
        static int BIBLIO_HEIGHT = _BIBLIO_HEIGHT;
        static int SUBREC_WIDTH = _SUBREC_WIDTH;
        static int SUBREC_HEIGHT = _SUBREC_HEIGHT;

        static int SEP_X = _SEP;
        static int PIC_SEP_X = _PIC_SEP;
        static int SEP_Y = _SEP;
        static int PIC_SEP_Y = _PIC_SEP;

        public static void SetMetrics(SizeF dpi_ratio)
        {
            BIBLIO_WIDTH = DpiUtil.GetScalingX(dpi_ratio, _BIBLIO_WIDTH);
            BIBLIO_HEIGHT = DpiUtil.GetScalingY(dpi_ratio, _BIBLIO_HEIGHT);

            SUBREC_WIDTH = DpiUtil.GetScalingX(dpi_ratio, _SUBREC_WIDTH);
            SUBREC_HEIGHT = DpiUtil.GetScalingY(dpi_ratio, _SUBREC_HEIGHT);

            SEP_X = DpiUtil.GetScalingX(dpi_ratio, _SEP);
            SEP_Y = DpiUtil.GetScalingY(dpi_ratio, _SEP);
            PIC_SEP_X = DpiUtil.GetScalingX(dpi_ratio, _PIC_SEP);
            PIC_SEP_Y = DpiUtil.GetScalingY(dpi_ratio, _PIC_SEP);
        }

        // parameters:
        //      
        public static Size Paint(PaintEventArgs e,
            Size bound,
            Font ref_font,
            string strSouceTitle,
            string strTargetTitle,
            MergeStyle style)
        {
            int x = SEP_X * 4;
            int y = SEP_Y * 4;

            // 不含有边距的整个尺寸
            Size content_size = new Size(BIBLIO_WIDTH + PIC_SEP_X + BIBLIO_WIDTH,
    BIBLIO_HEIGHT + (SUBREC_HEIGHT + SEP_Y) * 3 + PIC_SEP_Y / 2 + BIBLIO_HEIGHT + (SUBREC_HEIGHT + SEP_Y) * 5);

            int x_blank = bound.Width - content_size.Width;
            int y_blank = bound.Height - content_size.Height;
            if (x_blank > 0)
                x = Math.Max(x_blank / 2, SEP_X * 4);
            if (y_blank > 0)
                y = Math.Max(y_blank / 2, SEP_Y * 4);

            Size total_size = new Size(content_size.Width + x + SEP_X * 4, 
                content_size.Height + y + SEP_Y * 4);

            // testing
            // e.Graphics.ScaleTransform(2.0F, 2.0F);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // 绘制左边一个书目记录
            Size size = PaintRecord(e.Graphics, x, y,
                ref_font,
                strSouceTitle,
                leftColor,
                3, new Color[] { leftColor, leftColor, leftColor });

            // 绘制右边一个书目记录
            int x2 = x + size.Width + PIC_SEP_X;
            int y2 = y;
            PaintRecord(e.Graphics, x2, y2,
                ref_font,
                strTargetTitle,
                rightColor,
                2, new Color[] { rightColor, rightColor });

            // 绘制下边一个
            int x3 = x + (size.Width * 2 + PIC_SEP_X) / 2 - size.Width / 2;
            int y3 = y + size.Height + PIC_SEP_Y / 2;

            Color biblio_color = leftColor;
            if ((style & MergeStyle.ReserveSourceBiblio) != 0)
                biblio_color = leftColor;
            if ((style & MergeStyle.ReserveTargetBiblio) != 0)
                biblio_color = rightColor;

            if ((style & MergeStyle.CombineSubrecord) != 0)
            {
                PaintRecord(e.Graphics, x3, y3,
                    ref_font,
                    "最终保存效果",
                    biblio_color,
                    5, new Color[] { leftColor, leftColor, leftColor, rightColor, rightColor });
            }
            if ((style & MergeStyle.OverwriteSubrecord) != 0)
            {
                PaintRecord(e.Graphics, x3, y3,
                    ref_font,
                    "最终保存效果",
                    biblio_color,
                    3, new Color[] { leftColor, leftColor, leftColor });
            }
            if ((style & MergeStyle.MissingSourceSubrecord) != 0)
            {
                PaintRecord(e.Graphics, x3, y3,
                    ref_font,
                    "最终保存效果",
                    biblio_color,
                    2, new Color[] { rightColor, rightColor });
            }

            int nFontHeight = ref_font.Height;

            if ((style & MergeStyle.ReserveSourceBiblio) != 0)
            {
                // 1--> 3 biblio
                PaintCurve(e.Graphics,
                    x + size.Width / 2, y + nFontHeight + (BIBLIO_HEIGHT - nFontHeight) / 2,
                    x3 + size.Width / 2, y3 + nFontHeight + (BIBLIO_HEIGHT - nFontHeight) / 2,
                    "right");
            }
            if ((style & MergeStyle.ReserveTargetBiblio) != 0)
            {
                // 2--> 3 biblio
                PaintCurve(e.Graphics,
                    x2 + size.Width / 2, y2 + nFontHeight + (BIBLIO_HEIGHT - nFontHeight) / 2,
                    x3 + size.Width / 2, y3 + nFontHeight + (BIBLIO_HEIGHT - nFontHeight) / 2,
                    "left");
            }

            if ((style & MergeStyle.CombineSubrecord) != 0)
            {
                // 1 --> 3 subrecord
                PaintCurve(e.Graphics,
        x + SUBREC_WIDTH / 2 + SEP_X * 2, 
        y + BIBLIO_HEIGHT + 3 * (SUBREC_HEIGHT + SEP_Y) / 2,
        x3 + SUBREC_WIDTH / 2 + SEP_X * 2, 
        y3 + BIBLIO_HEIGHT + 3 * (SUBREC_HEIGHT + SEP_Y) / 2,
        "left");

                // 2 --> 3 cubrecord
                PaintCurve(e.Graphics,
    x2 + SUBREC_WIDTH / 2 + SEP_X * 2, 
    y2 + BIBLIO_HEIGHT + 0 * (SUBREC_HEIGHT + SEP_Y) + 2 * (SUBREC_HEIGHT + SEP_Y) / 2,
    x3 + SUBREC_WIDTH / 2 + SEP_X * 2, 
    y3 + BIBLIO_HEIGHT + 3 * (SUBREC_HEIGHT + SEP_Y) + 2 * (SUBREC_HEIGHT + SEP_Y) / 2,
    "right");
            }

            if ((style & MergeStyle.OverwriteSubrecord) != 0)
            {
                // 1 --> 3 subrecord
                PaintCurve(e.Graphics,
        x + SUBREC_WIDTH / 2 + SEP_X * 2, 
        y + BIBLIO_HEIGHT + 3 * (SUBREC_HEIGHT + SEP_Y) / 2,
        x3 + SUBREC_WIDTH / 2 + SEP_X * 2,
        y3 + BIBLIO_HEIGHT + 3 * (SUBREC_HEIGHT + SEP_Y) / 2,
        "left");
            }
            if ((style & MergeStyle.MissingSourceSubrecord) != 0)
            {
                // 2 --> 3 cubrecord
                PaintCurve(e.Graphics,
    x2 + SUBREC_WIDTH / 2 + SEP_X * 2,
    y2 + BIBLIO_HEIGHT + 0 * (SUBREC_HEIGHT + SEP_Y) + 2 * (SUBREC_HEIGHT + SEP_Y) / 2,
    x3 + SUBREC_WIDTH / 2 + SEP_X * 2, 
    y3 + BIBLIO_HEIGHT + 0 * (SUBREC_HEIGHT + SEP_Y) + 2 * (SUBREC_HEIGHT + SEP_Y) / 2,
    "right");
            }

            return total_size;
        }

        //      strStyle    left right 向哪边弯曲
        static void PaintCurve(Graphics g,
            int x1, int y1,
            int x2, int y2,
            string strStyle)
        {
            Point[] points = new Point[3];
            points[0] = new Point(x1, y1);
            if (strStyle == "left")
                points[1] = new Point((x1 + x2) / 2 - Math.Abs(x1 - x2) / 2, (y1 + y2) / 2);
            else if (strStyle == "right")
                points[1] = new Point((x1 + x2) / 2 + Math.Abs(x1 - x2) / 2, (y1 + y2) / 2);
            else
                points[1] = new Point((x1 + x2) / 2, (y1 + y2) / 2);

            points[2] = new Point(x2, y2);

            using (Pen pen = new Pen(lineColor, 2.0F))
            {
                g.DrawCurve(pen, points);
            }

            using (Pen pen = new Pen(lineColor, 2.0F))
            {
                Rectangle rect = new Rectangle(points[0], new Size(6, 6));
                rect.Offset(-3, -3);
                g.DrawArc(pen, rect, 0, 360);

                rect = new Rectangle(points[2], new Size(6, 6));
                rect.Offset(-3, -3);
                g.DrawArc(pen, rect, 0, 360);
            }
        }

        // 绘制一个包含下级记录的书目记录
        static Size PaintRecord(Graphics g,
            int x0,
            int y0,
            Font font,
            string strTitle,
            Color biblio_body_color,
            int nSubRecordCount,
            Color[] sub_colors)
        {
            Size size = new Size();

            int x = x0;
            int y = y0;

            PaintBiblio(g, x, y,
                font, strTitle,
                biblio_body_color);
            x += SEP_X * 2;
            y += BIBLIO_HEIGHT;

            for (int i = 0; i < nSubRecordCount; i++)
            {
                Color color = sub_colors[i];
                y += SEP_Y;
                PaintSubRecord(g, x, y, color);
                y += SUBREC_HEIGHT;
            }

            size.Width = BIBLIO_WIDTH;
            size.Height = BIBLIO_HEIGHT + SUBREC_HEIGHT * nSubRecordCount + SEP_Y * nSubRecordCount;

            // 画线条
            using (Pen pen = new Pen(lineColor, 1.0F))
            {
                {
                    Point pt1 = new Point(x0 + SEP_X, 
                        y0 + BIBLIO_HEIGHT);
                    Point pt2 = new Point(x0 + SEP_X,
                        y0 + BIBLIO_HEIGHT + SUBREC_HEIGHT * nSubRecordCount + SEP_Y * nSubRecordCount - SUBREC_HEIGHT / 2);
                    g.DrawLine(pen, pt1, pt2);
                }

                for (int i = 0; i < nSubRecordCount; i++)
                {
                    Point pt1 = new Point(x0 + SEP_X, 
                        y0 + BIBLIO_HEIGHT + SUBREC_HEIGHT * i + SEP_Y * i + SEP_Y + SUBREC_HEIGHT / 2);
                    Point pt2 = new Point(x0 + SEP_X + SEP_X, 
                        y0 + BIBLIO_HEIGHT + SUBREC_HEIGHT * i + SEP_Y * i + SEP_Y + SUBREC_HEIGHT / 2);
                    g.DrawLine(pen, pt1, pt2);
                }
            }
            return size;
        }

        // 绘制一个子记录的图像
        static void PaintSubRecord(Graphics g, int x, int y, Color body_color)
        {
            Point location = new Point(x, y);
            Size size = new Size(SUBREC_WIDTH, SUBREC_HEIGHT);
            Rectangle rect = new Rectangle(location, size);
            using (Brush brush = new SolidBrush(body_color))
            {
                g.FillRectangle(brush, rect);
            }
            using (Pen pen = new Pen(lineColor, 2.0F))
            {
                g.DrawRectangle(pen, rect);
            }
        }

        // 绘制一个书目记录的图像
        static void PaintBiblio(Graphics g,
            int x, int y,
            Font font,
            string strTitle,
            Color body_color)
        {
            using (Pen pen = new Pen(lineColor, 2.0F))
            {
                // 顶部文字
                using (Brush brush = new SolidBrush(lineColor))
                {
                    g.DrawString(strTitle, font, brush, new Point(x, y));
                }

                Point location = new Point(x, y + font.Height);
                Size size = new Size(BIBLIO_WIDTH, BIBLIO_HEIGHT - font.Height);
                Rectangle rect = new Rectangle(location, size);
                using (Brush brush = new SolidBrush(body_color))
                {
                    g.FillRectangle(brush, rect);
                }
                g.DrawRectangle(pen, rect);
            }
        }

    }

    [Flags]
    public enum MergeStyle
    {
        None = 0,
        OverwriteSubrecord = 0x01,   // 目标记录的下级记录被删除，源记录的下级记录移动过来
        CombineSubrecord = 0x02,     // 目标和源记录的下级记录都得到保留
        MissingSourceSubrecord = 0x04,   // 目标记录的下级记录被保留，源记录的下级记录被丢弃

        ReserveSourceBiblio = 0x08,     // 书目记录，采用源书目记录 (对象都全部合并)
        ReserveTargetBiblio = 0x10,     // 书目记录，采用目标书目记录 (对象都全部合并)

        SubRecordMask = 0x01 | 0x02 | 0x04, // 下级记录部分的掩码
    }
}
