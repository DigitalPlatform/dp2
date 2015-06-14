using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
    public class XmlLabel : TextVisual
    {
        public override ItemRegion GetRegionName()
        {
            return ItemRegion.Label;
        }

        public override void Paint(PaintEventArgs pe,
            int nBaseX,
            int nBaseY,
            PaintMember paintMember)
        {
            if (this.Rect.Width == 0
                || this.Rect.Height == 0)
                return;

            Rectangle rectPaint = new Rectangle(nBaseX + this.Rect.X,
                nBaseY + this.Rect.Y,
                this.Rect.Width,
                this.Rect.Height);

            Brush brush = null;

            //背景色
            Item item = this.GetItem();
            Object colorDefault = null;
            XmlEditor editor = item.m_document;
            if (editor != null && editor.VisualCfg != null)
                colorDefault = editor.VisualCfg.transparenceColor;
            if (colorDefault != null)
            {
                if (((Color)colorDefault).Equals(BackColor) == true)
                    goto SKIPDRAWBACK;

            }

            Color backColor = this.BackColor;

            // 如果对象的父亲 是 活动的Item 加亮显示
            if (editor != null)
            {
                if (item == editor.m_selectedItem)
                {
                    backColor = ControlPaint.Light(backColor);
                }
            }

            brush = new SolidBrush(backColor);
            pe.Graphics.FillRectangle(brush, rectPaint);

        SKIPDRAWBACK:

            //调DrawLines画边框
            if (editor != null && editor.VisualCfg == null)
            {
            }
            else
            {
                this.DrawLines(rectPaint,
                    this.TopBorderHeight,
                    this.BottomBorderHeight,
                    this.LeftBorderWidth,
                    this.RightBorderWidth,
                    this.BorderColor);
            }

            //内容区域
            rectPaint = new Rectangle(nBaseX + this.Rect.X + this.LeftResWidth/*LeftBlank*/,
                nBaseY + this.Rect.Y + this.TopResHeight/*this.TopBlank*/,
                this.Rect.Width - this.TotalRestWidth/*this.LeftBlank - this.RightBlank*/,
                this.Rect.Height - this.TotalRestHeight/*this.TopBlank - this.BottomBlank*/);

            brush = new SolidBrush(TextColor);
            Font font1 = this.GetFont();
            Font font = new Font(font1.Name, font1.Size);

            pe.Graphics.DrawString(Text,
                font,
                brush,
                rectPaint,
                new StringFormat());

            brush.Dispose();
            font.Dispose();
        }
    }
}
