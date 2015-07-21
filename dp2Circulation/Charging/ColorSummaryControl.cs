using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace dp2Circulation
{
    // TODO: 双击时候 检测双击的位置 触发激活 dp2Table 的事件
    /// <summary>
    /// 概括显示出纳红绿黄的色条控件
    /// </summary>
    public partial class ColorSummaryControl : UserControl
    {

        /// <summary>
        /// 颜色列表。每个字符表示一种颜色 R G Y L(gray) W P B 
        /// </summary>
        public string ColorList
        {
            get
            {
                return this._colorList;
            }
            set
            {
                this._colorList = value;

                if (this._colorList.IndexOfAny(new char[] { 'R', 'Y', 'W', 'P', 'B' }) == -1)
                {
                    this._blink = false;
                    this._wide = true;
                    this.timer1.Stop();
                }
                else
                {
                    this._blink = true;
                    this._wide = false;
                    this.timer1.Start();
                }

                this.Invalidate();
            }
        }

        string _colorList = "";   // 每个字符表示一种颜色 R G Y L(gray) W P

        bool _blink = false;    // 是否闪动

        bool _wide = true; // 是否显示为宽的状态。闪动的时候，一次宽的，一次窄的。不闪动的时候，一直是宽的

        /// <summary>
        /// 构造函数
        /// </summary>
        public ColorSummaryControl()
        {
            InitializeComponent();
        }

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.Paint 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.PaintEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.Paint 事件。
        /// </summary>
        /// <param name="pe">包含事件数据的 System.Windows.Forms.PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (string.IsNullOrEmpty(this._colorList) == true)
                return;
            try
            {
                DoPaint(
                    pe.Graphics,
                    0,
                    0,
                    this.Size,
                    this._colorList,
                    this._wide);
            }
            catch (ArgumentException)
            {
                this.timer1.Stop();
            }

#if NO
            double cell_width = (double)this.Size.Width / (double)this._colorList.Length;

            Brush brushRed = new SolidBrush(Color.Red);
            Brush brushGreen = new SolidBrush(Color.Green);
            Brush brushYellow = new SolidBrush(Color.Yellow);
            Brush brushGray = new SolidBrush(Color.LightGray);
            Brush brushWhite = new SolidBrush(Color.White);
            Brush brushPurple = new SolidBrush(Color.Purple);
            Brush brushBlack = new SolidBrush(Color.Black);

            double offs = 0;
            foreach (char ch in this._colorList)
            {
                RectangleF rect = new RectangleF();
                rect.X = (float)offs;
                rect.Y = 0;
                rect.Width = (float)cell_width - 1;
                if (rect.Width < 1.1F)
                    rect.Width = 1.1F;
                rect.Height = this.Size.Height;
                Brush brush = null;
                if (ch == 'R')
                {
                    brush = brushRed;
                    if (this._wide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'P')
                {
                    brush = brushPurple;
                    if (this._wide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'B')
                {
                    brush = brushBlack;
                    if (this._wide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'G')
                {
                    brush = brushGreen;
                    rect.Height = rect.Height / 3;  // 绿色的显示窄一些
                }
                else if (ch == 'Y')
                {
                    brush = brushYellow;
                    if (this._wide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'L')
                    brush = brushGray;
                else if (ch == 'W')
                {
                    brush = brushWhite;
                    if (this._wide == false)
                        rect.Height = rect.Height / 2;
                }
                else
                {
#if DEBUG
                    Debug.Assert(false, "未知的字符 '" + ch.ToString() + "'");
                    this.timer1.Stop();
#endif

                }

                pe.Graphics.FillRectangle(brush,
                    rect);
                // 黄色(或者白色)的边框容易看不清，补充描边一次
                if (ch == 'Y' || ch == 'W')
                {
                    rect.Width -= 1;

                    if (rect.Width > 6)
                    {
                        rect.Height -= 1;
                        pe.Graphics.DrawRectangle(
                            ch == 'Y' ? Pens.Gray : Pens.LightGray,
                            rect.X,
                            rect.Y,
                            rect.Width,
                            rect.Height);
                    }
                }
                offs += cell_width;
            }

            brushRed.Dispose();
            brushGreen.Dispose();
            brushYellow.Dispose();
            brushGray.Dispose();
            brushWhite.Dispose();
            brushPurple.Dispose();
            brushBlack.Dispose();
#endif
        }

        public static void DoPaint(
            Graphics g,
            long x,
            long y,
            Size size,
            string strColorList,
            bool bWide = true)
        {
            double cell_width = (double)size.Width / (double)strColorList.Length;

            Brush brushRed = new SolidBrush(Color.Red);
            Brush brushGreen = new SolidBrush(Color.Green);
            Brush brushYellow = new SolidBrush(Color.Yellow);
            Brush brushGray = new SolidBrush(Color.LightGray);
            Brush brushWhite = new SolidBrush(Color.White);
            Brush brushPurple = new SolidBrush(Color.Purple);
            Brush brushBlack = new SolidBrush(Color.Black);

            double offs = 0;
            foreach (char ch in strColorList)
            {
                RectangleF rect = new RectangleF();
                rect.X = (float)offs + x;
                rect.Y = 0 + y;
                rect.Width = (float)cell_width - 1;
                if (rect.Width < 1.1F)
                    rect.Width = 1.1F;
                rect.Height = size.Height;
                Brush brush = null;
                if (ch == 'R')
                {
                    brush = brushRed;
                    if (bWide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'P')
                {
                    brush = brushPurple;
                    if (bWide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'B')
                {
                    brush = brushBlack;
                    if (bWide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'G')
                {
                    brush = brushGreen;
                    rect.Height = rect.Height / 3;  // 绿色的显示窄一些
                }
                else if (ch == 'Y')
                {
                    brush = brushYellow;
                    if (bWide == false)
                        rect.Height = rect.Height / 2;
                }
                else if (ch == 'L')
                    brush = brushGray;
                else if (ch == 'W')
                {
                    brush = brushWhite;
                    if (bWide == false)
                        rect.Height = rect.Height / 2;
                }
                else
                {
                    throw new ArgumentException("未知的字符 '" + ch.ToString() + "'");
#if NO
#if DEBUG
                    Debug.Assert(false, "未知的字符 '" + ch.ToString() + "'");
                    this.timer1.Stop();
#endif
#endif

                }

                g.FillRectangle(brush,
                    rect);
                // 黄色(或者白色)的边框容易看不清，补充描边一次
                if (ch == 'Y' || ch == 'W')
                {
                    rect.Width -= 1;

                    if (rect.Width > 6)
                    {
                        rect.Height -= 1;
                        g.DrawRectangle(
                            ch == 'Y' ? Pens.Gray : Pens.LightGray,
                            rect.X,
                            rect.Y,
                            rect.Width,
                            rect.Height);
                    }
                }
                offs += cell_width;
            }

            brushRed.Dispose();
            brushGreen.Dispose();
            brushYellow.Dispose();
            brushGray.Dispose();
            brushWhite.Dispose();
            brushPurple.Dispose();
            brushBlack.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
            this.Update();
            if (this._wide == true)
                this._wide = false;
            else
                this._wide = true;
        }

#if NO
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);


        }
#endif

        // return:
        //      -1  没有命中任何块
        //      >=0 命中的块的编号
        public int HitTest(long p_x,
    long p_y)
        {
            if (string.IsNullOrEmpty(this._colorList) == true)
                return -1;
            double cell_width = (double)this.Size.Width / (double)this._colorList.Length;
            int index = (int)(p_x / cell_width);

            if (index >= this._colorList.Length)
                return -1;
            return index;
        }
    }

#if NO
    /// <summary>
    /// 单元被选中事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void CellSelectedEventHandler(object sender,
CellSelectedEventArgs e);

    /// <summary>
    /// 单元被选中事件的参数
    /// </summary>
    public class CellSelectedEventArgs : EventArgs
    {
        public bool bDoEvents = true;
    }
#endif
}
