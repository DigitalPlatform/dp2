﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 浮动的信息窗口。提醒操作没有完成，不要让读者离开出纳台
    /// </summary>
    public partial class FloatingMessageForm : Form
    {
#if REMOVED
        DateTime _clearTime = new DateTime(0);
#endif

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

                const int WS_EX_NOACTIVATE = 0x08000000;
                // const int WS_EX_TOOLWINDOW = 0x00000080;
                cp.ExStyle |= (int)(WS_EX_NOACTIVATE
                    // | WS_EX_TOOLWINDOW
                    );

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
                try
                {
                    this._clickable = value;

                    int windowLong = API.GetWindowLong(this.Handle, API.GWL_EXSTYLE);
                    if (value)
                        windowLong &= ~0x20;
                    else
                        windowLong |= 0x20;
                    API.SetWindowLong(this.Handle, API.GWL_EXSTYLE, windowLong);
                }
                catch (ObjectDisposedException)
                {

                }
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

            if (string.IsNullOrEmpty(this.Text) == true)
                return;

            // e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
#if NO
            Brush brush = new SolidBrush(Color.FromArgb(100, 0,0,255));
            e.Graphics.FillEllipse(brush, 30, 30, 100, 100);
#endif

            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = StringAlignment.Center;
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = e.Graphics.MeasureString(this.Text,
                this.Font,
                this.Size.Width / 2,
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

            using (Brush brush = new SolidBrush(this.RectColor))
            using (Pen pen = new Pen(Color.Gray))
            {
                if (this.Closeable == false)
                {
                    // 圆角表示不可点击
                    RoundRectangle(e.Graphics,
                    pen,
                    brush,
                    backRect,
                    backRect.Height / 3);   // / 4 6
                }
                else
                {
                    // 方角表示可点击消失
                    RoundRectangle(e.Graphics,
    pen,
    brush,
    backRect,
    0);
                }
            }

            using (Brush brush = new SolidBrush(Color.FromArgb(254, 254, 254)))
            {
                e.Graphics.DrawString(
                    this.Text,
                    this.Font,
                    brush,
                    textRect,
                    format);
            }
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
            using (GraphicsPath path = new GraphicsPath())
            {
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
            }
        }

        // 线程安全
        // 返回结果:
        //     与该控件关联的文本。
        /// <summary>
        /// 与该控件关联的文本
        /// </summary>
        public override string Text
        {
            get
            {
                if (this.InvokeRequired)
                {
                    return (string)this.Invoke(new Func<string>(() =>
                    {
                        return base.Text;
                    }));
                }
                return base.Text;
            }
            set
            {
                SetText(value);
            }
        }

        void SetText(string value)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetText), value);
                return;
            }

            bool bChanged = false;
            if (base.Text != value)
            {
                bChanged = true;
                StopDelayClear();  // 文字被主动修改后，延时清除就被取消了
                base.Text = value;
            }

            if (bChanged == true)
                this.Invalidate();
        }

        double _saveOpacity = 0.7;

        /*
发生未捕获的界面线程异常: 
Type: System.ComponentModel.Win32Exception
Message: 存储空间不足，无法处理此命令。
Stack:
在 System.Windows.Forms.Form.UpdateLayered()
在 System.Windows.Forms.Form.set_Opacity(Double value)
在 DigitalPlatform.CommonControl.FloatingMessageForm.set_Opacity(Double value)
在 dp2Catalog.MyForm.OnMyFormLoad()
在 dp2Catalog.MyForm.OnLoad(EventArgs e)
在 System.Windows.Forms.Form.OnCreateControl()
在 System.Windows.Forms.Control.CreateControl(Boolean fIgnoreVisible)
在 System.Windows.Forms.Control.CreateControl()
在 System.Windows.Forms.Control.WmShowWindow(Message& m)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ScrollableControl.WndProc(Message& m)
在 System.Windows.Forms.Form.WmShowWindow(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


         * */
        public new double Opacity
        {
            get
            {
                return base.Opacity;
            }
            set
            {
                try
                {
                    base.Opacity = value;
                }
                catch (System.ComponentModel.Win32Exception)
                {

                }
                _saveOpacity = value;
            }
        }

        // 设置消息文字和颜色，可否点击
        // 一次性设置，比较方便
        public void SetMessage(string strText, Color rectColor, bool bClickClose)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, Color, bool>(SetMessage), strText, rectColor, bClickClose);
                return;
            }

            base.Text = strText;
            this._rectColor = rectColor;
            this.Closeable = bClickClose;
            this.Clickable = bClickClose;   // 2018/11/6
            try
            {
                if (bClickClose)
                    base.Opacity = 1.0;
                else
                    base.Opacity = _saveOpacity;    // 恢复原来的不透明度
            }
            catch (System.ComponentModel.Win32Exception)
            {

            }

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

            OnResizeOrMove();   // 2015/6/4
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

            this.Invalidate();
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
                && (this.Closeable == true || this.Clicked != null)
                )
            {
                base.WndProc(ref m);
                m.Result = new IntPtr(2);   // simulate client
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        int _inDelay = 0;
        CancellationTokenSource _cancelDelay = new CancellationTokenSource();


        // 设定延时 Clear 时间长度
        public void DelayClear(TimeSpan delta)
        {
            _cancelDelay?.Cancel();
            _cancelDelay = new CancellationTokenSource();
            var token = _cancelDelay.Token;
            _ = Task.Run(async () =>
            {
                _inDelay++;
                try
                {
                    await Task.Delay(delta, token);
                    token.ThrowIfCancellationRequested();
                    this.TryInvoke(() =>
                    {
                        base.Text = "";
                        this.Invalidate();
                    });
                }
                finally
                {
                    _inDelay--;
                }
            });
#if REMOVED
            this._clearTime = DateTime.Now + delta;
            timer1.Start();
#endif
        }

        public bool InDelay()
        {
            return _inDelay > 0;
#if REMOVED
            if (this._clearTime == new DateTime(0))
                return false;
            return true;
#endif
        }

#if REMOVED
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this._clearTime != new DateTime(0)
                && DateTime.Now > this._clearTime)
            {
                base.Text = "";
                this.Invalidate();

                StopDelayClear();
            }
        }
#endif

        public void StopDelayClear()
        {
            _cancelDelay?.Cancel();
            _cancelDelay = null;
#if REMOVED
            timer1.Stop();
            this._clearTime = new DateTime(0);
#endif
        }
    }

    /// <summary>
    /// 显示浮动信息
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ShowMessageEventHandler(object sender,
    ShowMessageEventArgs e);

    /// <summary>
    /// 显示浮动信息事件的参数
    /// </summary>
    public class ShowMessageEventArgs : EventArgs
    {
        public string Message = "";
        public string Color = "";
        public bool ClickClose = false;
    }


}
