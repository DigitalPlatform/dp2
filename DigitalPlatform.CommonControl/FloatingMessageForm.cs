using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 浮动的信息窗口。提醒操作没有完成，不要让读者离开出纳台
    /// </summary>
    public partial class FloatingMessageForm : Form
    {
        // 是否允许感知 Click 清除文字
        public bool Closeable = false;

        public event EventHandler Clicked = null;

        Color _rectColor = Color.Purple;

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color RectColor
        {
            get
            {
                return this._rectColor;
            }
            set
            {
                if (this._rectColor != value)
                {
                    this._rectColor = value;
                    this.Invalidate();
                }
            }
        }

        Form _parent = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        public FloatingMessageForm(Form parent, bool bClickable = false)
        {
            _saveOpacity = this.Opacity;

            this._parent = parent;
            this.Clickable = bClickable;

            InitializeComponent();

            // this.TopMost = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            // this.WindowState = FormWindowState.Maximized;

            this.MinimizeBox = this.MaximizeBox = false;
            this.TransparencyKey = this.BackColor = Color.White;
        }

        //
        //
        // 返回结果:
        //     System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。
        /// <summary>
        /// 返回结果: System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                if (this.Clickable == false)
                    cp.ExStyle |= 0x80000 |  // WS_EX_LAYERED
                        0x20; // WS_EX_TRANSPARENT
                else
                    cp.ExStyle |= 0x80000;  // WS_EX_LAYERED

                return cp;
            }
        }

        bool _clickable = false;
        public bool Clickable
        {
            get
            {
#if NO
                int windowLong = API.GetWindowLong(this.Handle, API.GWL_EXSTYLE);
                if ((windowLong & 0x20) == 0)
                    return true;
                return false;
#endif
                return _clickable;
            }
            set
            {
                this._clickable = value;

                int windowLong = API.GetWindowLong(this.Handle, API.GWL_EXSTYLE);
                if (value)
                    windowLong &= ~0x20;
                else
                    windowLong |= 0x20;
                API.SetWindowLong(this.Handle, API.GWL_EXSTYLE, windowLong);
            }
        }

        //
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.PaintEventArgs。
        /// <summary>
        /// 绘制内容
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

#if NO
            Brush brush = new SolidBrush(Color.FromArgb(100, 0,0,255));
            e.Graphics.FillEllipse(brush, 30, 30, 100, 100);
#endif
            if (string.IsNullOrEmpty(this.Text) == true)
                return;

            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = StringAlignment.Center;
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = e.Graphics.MeasureString(this.Text,
                this.Font,
                this.Size.Width / 2 ,
                format);

            RectangleF textRect = new RectangleF(
(this.Size.Width - size.Width) / 2,
(this.Size.Height - size.Height) / 2,
size.Width,
size.Height);

            float new_height = textRect.Height * 3;
            RectangleF backRect = new RectangleF(textRect.X,
                textRect.Y,
                textRect.Width,
                textRect.Height);
            float delta = Math.Min(this.Size.Width, this.Size.Height) / 20;
            backRect = RectangleF.Inflate(textRect, delta, delta);


            Pen pen = new Pen(Color.Gray);
            if (this.Closeable == false)
            {
                // 圆角表示不可点击
                RoundRectangle(e.Graphics,
                pen,
                new SolidBrush(this.RectColor),
                backRect,
                backRect.Height / 3);   // / 4 6
            }
            else
            {
                // 方角表示可点击消失
                RoundRectangle(e.Graphics,
pen,
new SolidBrush(this.RectColor),
backRect,
0);
            }

            e.Graphics.DrawString(
                this.Text,
                this.Font,
                new SolidBrush(Color.FromArgb(254,254,254)),
                textRect,
                format);
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        static void RoundRectangle(Graphics graphics,
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
        static void RoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddLine(x + radius, y, x + width - (radius * 2), y);
            if (radius != 0)
                path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            if (radius != 0)
                path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
            path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            if (radius != 0)
                path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            path.AddLine(x, y + height - (radius * 2), x, y + radius);
            if (radius != 0)
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.CloseFigure();
            if (brush != null)
                graphics.FillPath(brush, path);
            if (pen != null)
                graphics.DrawPath(pen, path);
            path.Dispose();
        }

        //
        //
        // 返回结果:
        //     与该控件关联的文本。
        /// <summary>
        /// 与该控件关联的文本
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                bool bChanged = false;
                if (base.Text != value)
                    bChanged = true;

                base.Text = value;

                if (bChanged == true)
                    this.Invalidate();
            }
        }

        double _saveOpacity = 0.7;

        public new double Opacity
        {
            get
            {
                return base.Opacity;
            }
            set
            {
                base.Opacity = value;
                _saveOpacity = value;
            }
        }

        // 设置消息文字和颜色，可否点击
        // 一次性设置，比较方便
        public void SetMessage(string strText, Color rectColor, bool bClickClose)
        {
            base.Text = strText;
            this._rectColor = rectColor;
            this.Closeable = bClickClose;

            if (bClickClose)
                base.Opacity = 1.0;
            else
                base.Opacity = _saveOpacity;    // 恢复原来的不透明度

            this.Invalidate();
        }

        bool _isSameFont = false;
        private void FloatingMessageForm_Load(object sender, EventArgs e)
        {
            if (this.Font.FontFamily.Equals(this.Owner.Font.FontFamily) == true)
                _isSameFont = true;

            this._parent.Activated += new System.EventHandler(this.Parent_Activated);
            this._parent.Deactivate += new System.EventHandler(this.Parent_Deactivate);
            this._parent.Move += new System.EventHandler(this.QuickChargingForm_Move);
            this._parent.SizeChanged += new System.EventHandler(this.QuickChargingForm_SizeChanged);
            this._parent.Load += Owner_Load;
            this._parent.FontChanged += Owner_FontChanged;
        }

        void Owner_FontChanged(object sender, EventArgs e)
        {
            // 和父窗口设置成同一字体
            if (this._isSameFont == true && this._parent != null)
                this.Font = new Font(this._parent.Font.FontFamily, this.Font.Size, this.Font.Style);
        }

        // 2015/4/30
        void Owner_Load(object sender, EventArgs e)
        {
            OnResizeOrMove();
        }

        private void FloatingMessageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._parent.Activated -= new System.EventHandler(this.Parent_Activated);
            this._parent.Deactivate -= new System.EventHandler(this.Parent_Deactivate);
            this._parent.Move -= new System.EventHandler(this.QuickChargingForm_Move);
            this._parent.SizeChanged -= new System.EventHandler(this.QuickChargingForm_SizeChanged);
            this._parent.Load -= Owner_Load;
            this._parent.FontChanged -= Owner_FontChanged;
        }

        public bool AutoHide = true;

        private void Parent_Activated(object sender, EventArgs e)
        {
            if (this.AutoHide)
            {
                try
                {
                    this.Show();
                }
                catch
                {
                }
            }
        }

        private void Parent_Deactivate(object sender, EventArgs e)
        {
            if (this.AutoHide)
            {
                try
                {
                    this.Hide();
                }
                catch
                {
                }
            }
        }

        private void QuickChargingForm_Move(object sender, EventArgs e)
        {
            OnResizeOrMove();
        }

        private void QuickChargingForm_SizeChanged(object sender, EventArgs e)
        {
            OnResizeOrMove();
        }

        /// <summary>
        /// 重新调整位置大小，根据父窗口的位置和大小
        /// </summary>
        public void OnResizeOrMove()
        {
            if (this._parent == null)
                return;

            Rectangle rect = this._parent.ClientRectangle;  //  new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);

            Rectangle screen_rect = this._parent.RectangleToScreen(rect);

            this.Location = new Point(screen_rect.X,
                screen_rect.Y);

            this.Size = new Size(screen_rect.Width, screen_rect.Height);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == API.WM_NCLBUTTONDOWN)
            {
                if (this.Closeable == true)
                {
                    // 清除文字显示
                    if (string.IsNullOrEmpty(base.Text) == false)
                    {
                        base.Text = "";
                        this.Invalidate();
                    }
                }
                if (this.Clicked != null)
                    this.Clicked(this, new EventArgs());
            } 
            else if (m.Msg == API.WM_NCHITTEST
                && (this.Closeable == true || this.Clicked != null))
            {
                base.WndProc(ref m);
                m.Result = new IntPtr(2);   // simulate client
            }
            else
                base.WndProc(ref m);
        }

    }
}
