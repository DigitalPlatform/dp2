using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 用于显示一个对照关系的控件
    /// </summary>
    public partial class RelationControl : UserControl
    {
        bool _selected = false;
        public bool Selected
        {
            get
            {
                return this._selected;
            }
            set
            {
                this._selected = value;
                this.Invalidate();
            }
        }

        List<string> _hitCounts = new List<string>();
        public List<string> HitCounts
        {
            get
            {
                return this._hitCounts;
            }
            set
            {
                this._hitCounts = value;
                this.Invalidate();
            }
        }

        // 源分类号的字体高度
        int _sourceFontSize = 24;
        public int SourceFontSize
        {
            get
            {
                return this._sourceFontSize;
            }
            set
            {
                this._sourceFontSize = value;
                SetSize();
                this.Invalidate();
            }
        }

        // 源分类号字符串
        string _sourceText = "";
        public string SourceText
        {
            get
            {
                return this._sourceText;
            }
            set
            {
                this._sourceText = value;
                SetSize();
                this.Invalidate();
            }
        }

        // 目标分类号字符串
        string _targetText = "";
        public string TargetText
        {
            get
            {
                return this._targetText;
            }
            set
            {
                this._targetText = value;
                SetSize();
                this.Invalidate();
            }
        }


        public RelationControl()
        {
            InitializeComponent();
        }

        // 上面部分是源分类号
        // 源分类号顶部有每个层级的命中数
        // 下面部分是选定的目标分类号
        private void RelationControl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Rectangle rectBack = new Rectangle(
0,
0,
this.ClientSize.Width,
this.ClientSize.Height);

            if (this._selected == true)
            {
                using (Brush brush = new SolidBrush(Color.LightBlue))
                {
                    e.Graphics.FillRectangle(brush, rectBack);
                }
            }

            if (this.Focused)
            {

                rectBack.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    rectBack);
            }
#if NO
                SizeF size = e.Graphics.MeasureString("M",
                    font,
                    100,
                    format);
#endif
            SizeF size = GetSourceFontSize(e.Graphics);

            // 绘制命中数行
            using (Font font = GetHitCountFont())
            using (Brush brush = new SolidBrush(Color.Gray))
            {
                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = StringAlignment.Center;
                format.FormatFlags |= StringFormatFlags.FitBlackBox;

                float x = this.Padding.Left;
                float y = this.Padding.Top;
                foreach(string strText in this._hitCounts)
                {
                    RectangleF textRect = new RectangleF(
x,
y,
size.Width,
size.Height);
                    e.Graphics.DrawString(
                        strText,
                        font,
                        brush,
                        textRect,
                        format);
                    x += size.Width;
                }
            }

            // 绘制源行
            using (Font font = GetSourceFont())
            using (Brush brush = new SolidBrush(Color.Black))
            {
                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = StringAlignment.Center;
                format.FormatFlags |= StringFormatFlags.FitBlackBox;

                int i = 0;
                float x = this.Padding.Left;
                float y = this.Padding.Top + size.Height / 2;
                foreach (char ch in this._sourceText)
                {
                    RectangleF textRect = new RectangleF(
x,
y,
size.Width,
size.Height);
                    e.Graphics.DrawString(
                        ch.ToString(),
                        font,
                        brush,
                        textRect,
                        format);
                    i++;
                    x += size.Width;
                }
            }

            // 绘制目标行
            using (Font font = GetTargetFont())
            using (Brush brush = new SolidBrush(Color.DarkRed))  // Color.DarkBlue
            {
                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = StringAlignment.Far;
                format.FormatFlags |= StringFormatFlags.FitBlackBox;

                float x = this.Padding.Left;
                float y = this.Padding.Top + size.Height / 2 + size.Height;
                RectangleF textRect = new RectangleF(
x,
y,
this.Width,
size.Height);
                e.Graphics.DrawString(
                    this._targetText,
                    font,
                    brush,
                    textRect,
                    format);
            }

        }

        Font GetSourceFont()
        {
            return new System.Drawing.Font(this.Font.FontFamily,
                (float)this.SourceFontSize, 
                FontStyle.Bold,
                GraphicsUnit.Pixel);
        }

        Font GetTargetFont()
        {
            return new System.Drawing.Font(this.Font.FontFamily,
                (float)this.SourceFontSize,
                FontStyle.Regular,
                GraphicsUnit.Pixel);
        }

        Font GetHitCountFont()
        {
            return new System.Drawing.Font(this.Font.FontFamily,
                (float)this.SourceFontSize/2,
                FontStyle.Regular,
                GraphicsUnit.Pixel);
        }

        // 得到源分类号的一个字符的尺寸
        SizeF GetSourceFontSize(Graphics g_param)
        {
            Graphics g = g_param;
            if(g == null)
                g = Graphics.FromHwnd(this.Handle);
            try {
                using(Font font = GetSourceFont())
                {
                    SizeF size = g.MeasureString("MMMMMMMMMM",
                    font);
                    size = new SizeF(size.Width / 10, size.Height);
                    return size;
                }
            }
            finally
            {
                if (g_param == null)
                    g.Dispose();
            }
        }

        // 设置控件尺寸
        void SetSize()
        {
            using (Graphics g = Graphics.FromHwnd(this.Handle))
            {
                SizeF size = GetSourceFontSize(g);

                // 目标文字像素宽度
                float fTargetTextWidth = 0;
                using (Font font = GetTargetFont())
                {
                    fTargetTextWidth = g.MeasureString(this.TargetText,
                    font).Width;
                }

                // 源文字像素宽度
                float fSourceTextWidth = size.Width * Math.Max(1, this.SourceText.Length);

                int nWidth = (int)(Math.Max(fSourceTextWidth, fTargetTextWidth))
                    + this.Padding.Horizontal;
                int nHeight = (int)(size.Height / 2) // hitcount line
                    + (int)size.Height  // source line
                    // + (int)(size.Height / 2)    // blank
                    + (int)size.Height// target line
                    + this.Padding.Vertical;
                this.Size = new Size(nWidth, nHeight);
            }
        }

        private void RelationControl_FontChanged(object sender, EventArgs e)
        {
            SetSize();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        // 绘制带有下划线的分类号文字
        public static void PaintSourceText(Graphics g,
            Font font,
            Color color,
            float x,
            float y,
            string strText,
            int nLevel)
        {
            using (Brush brush = new SolidBrush(color))
            {
                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = StringAlignment.Near;
                format.FormatFlags |= StringFormatFlags.FitBlackBox;

                {
                    SizeF size = g.MeasureString(strText.Substring(0, nLevel), font);

                    RectangleF textRect = new RectangleF(
    x,
    y,
    size.Width,
    size.Height);
                    float fPenWidth = 3.0F;
                    using (Pen pen = new Pen(Color.Green, fPenWidth))
                    {
                        g.DrawLine(pen,
                            new PointF(textRect.X, textRect.Y + textRect.Height - fPenWidth),
                            new PointF(textRect.X + textRect.Width, textRect.Y + textRect.Height - fPenWidth));
                    }
                }

                {
                    SizeF size = g.MeasureString(strText, font);

                    RectangleF textRect = new RectangleF(
    x,
    y,
    size.Width,
    size.Height);
                    g.DrawString(
                        strText,
                        font,
                        brush,
                        textRect,
                        format);
                }
            }
        }

        // 绘制分类号文字的下划线部分
        // parameters:
        //      x,y 指向文字矩形的左下角位置
        public static void PaintSourceTextUnderline(Graphics g,
            Font font,
            Color color,
            float x,
            float y,
            string strText,
            int nLevel)
        {
            SizeF size = g.MeasureString(strText.Substring(0, nLevel), font);

            float fPenWidth = 3.0F;
            RectangleF textRect = new RectangleF(
x,
y - fPenWidth,
size.Width,
fPenWidth);
            using (Pen pen = new Pen(color, fPenWidth))
            {
                g.DrawLine(pen,
                    new PointF(textRect.X, textRect.Y + textRect.Height / 2),
                    new PointF(textRect.X + textRect.Width, textRect.Y + textRect.Height / 2));
            }
        }

    }
}
