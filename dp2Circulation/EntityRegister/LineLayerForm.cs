using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 用于绘制连接线的浮动层窗口
    /// </summary>
    public partial class LineLayerForm : Form
    {
        public LineLayerForm()
        {
            InitializeComponent();

            this.MinimizeBox = this.MaximizeBox = false;
            this.TransparencyKey = this.BackColor = Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        /// <summary>
        /// 返回结果: System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;


                cp.ExStyle |= 0x80000 |  // WS_EX_LAYERED
                    0x20; // WS_EX_TRANSPARENT

                //cp.ExStyle |= 0x80000;  // WS_EX_LAYERED
                return cp;
            }
        }

        Pen CreateHatchPen(Color color, float width)
        {
            Brush brush = new HatchBrush(HatchStyle.DarkUpwardDiagonal, // WideDownwardDiagonal,
                // Color.FromArgb(0, 255, 255, 255),
                Color.FromArgb(0, 254, 254, 254),
                Color.FromArgb(255, color)
                );    // back
            return new Pen(brush,
                width);  // 可修改
        }

        Brush CreateHatchBrush(Color color)
        {
            return new HatchBrush(HatchStyle.WideDownwardDiagonal, // DarkUpwardDiagonal,
                // Color.FromArgb(0, 255, 255, 255),
                Color.FromArgb(0, 254, 254, 254),
                Color.FromArgb(255, color)
                );
        }

        private void LineLayerForm_Paint(object sender, PaintEventArgs e)
        {
            if (this._transparent == true)
                return;

            if (this._panelState == dp2Circulation.PanelState.HilightPanel)
            {
                if (_targetRect != new Rectangle(0, 0, 0, 0))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // 整个背景填充为灰色
                    using (Brush brush = new SolidBrush(Color.LightGray))
                    {
                        e.Graphics.FillRectangle(brush, this.ClientRectangle);
                    }

                    //Point[] points = GetLinePoints();
                    //e.Graphics.RenderingOrigin = Point.Round(points[0]);
                    using (Pen pen = CreateHatchPen(Color.Blue, 6))
                    {
                        Rectangle rectTarget = this.RectangleToClient(_targetRect);

                        Rectangle rectTargetBorder = rectTarget;

                        rectTargetBorder.Inflate((int)pen.Width / 2, (int)pen.Width / 2);

                        // rectTargetBorder.Offset((int)(-pen.Width / 2), (int)(-pen.Width / 2));
                        //rectTargetBorder.Width--;
                        //rectTargetBorder.Height--;

                        using (Brush brush = new SolidBrush(this.BackColor))
                        {
                            e.Graphics.FillRectangle(brush, rectTarget);
                        }

                        // e.Graphics.DrawRectangle(pen, rectTargetBorder);

                        //e.Graphics.DrawCurve(pen, points, 0.2F);
                    }

                    // 将 source 位置的线条用背景色擦除
                    using (Brush brush = new SolidBrush(this.BackColor))
                    {
                        Rectangle rectSource = this.RectangleToClient(_sourceRect);
                        e.Graphics.FillRectangle(brush, rectSource);
                    }

                    using (Pen pen = CreateHatchPen(Color.Blue, 6))
                    {
                        Rectangle rectSource = this.RectangleToClient(_sourceRect);

                        Rectangle rectBorder = rectSource;

                        rectBorder.Inflate((int)pen.Width / 2, (int)pen.Width / 2);
                        e.Graphics.DrawRectangle(pen, rectBorder);
                    }
                }
            }
            else if (this._panelState == dp2Circulation.PanelState.HilightForm)
            {
                // 把 source 区域变成灰色
                using (Brush brush = CreateHatchBrush(Color.DarkGray))    // new SolidBrush(Color.LightGray)
                {
                    e.Graphics.FillRectangle(brush, this.RectangleToClient(_sourceRect));
                }
            }
        }

        Rectangle _targetRect;

        /// <summary>
        /// 获取和设置目标控件的 Rectangle
        /// 屏幕坐标
        /// </summary>
        public Rectangle TargetRect
        {
            get
            {
                return _targetRect;
            }
            set
            {
                _targetRect = value;
                this.Invalidate();
            }
        }

        Rectangle _sourceRect;

        /// <summary>
        /// 获取和设置源控件的 Rectangle
        /// 屏幕坐标
        /// </summary>
        public Rectangle SourceRect
        {
            get
            {
                return _sourceRect;
            }
            set
            {
                _sourceRect = value;
                this.Invalidate();
            }
        }

        // 获得一条曲线的 Point 数组。从输入面板连向 _targetRect
        Point[] GetLinePoints()
        {
            // 转为窗口坐标
            Rectangle rectTarget = this.RectangleToClient(this.TargetRect);
            Rectangle rectPanel = this.RectangleToClient(this.SourceRect);

            const int DELTA = 50;
            // Panel 在 Target 右边
            if (rectPanel.X > rectTarget.X + rectTarget.Width + 2 * DELTA)
            {
                // 从 Target 开始画
                Point[] points = new Point[4];
                points[0] = new Point(rectTarget.X + rectTarget.Width, rectTarget.Y + rectTarget.Height / 2);

                int x_middle = (rectTarget.X + rectTarget.Width
                    + rectPanel.X)
                    / 2;
                points[1] = new Point(x_middle, rectTarget.Y + rectTarget.Height / 2);
                points[2] = new Point(x_middle, rectPanel.Y + rectPanel.Height / 2);

                points[3] = new Point(rectPanel.X, rectPanel.Y + rectPanel.Height / 2);
                return points;
            }
            else if (rectPanel.X > rectTarget.X)    // Panel 在 Target 右边，部分重叠
            {
                Point[] points = new Point[4];
                points[0] = new Point(rectTarget.X, rectTarget.Y + rectTarget.Height / 2);

                int x_middle = rectTarget.X - DELTA;
                points[1] = new Point(x_middle, rectTarget.Y + rectTarget.Height / 2);
                points[2] = new Point(x_middle, rectPanel.Y + rectPanel.Height / 2);

                points[3] = new Point(rectPanel.X, rectPanel.Y + rectPanel.Height / 2);
                return points;
            }
            else if (rectPanel.X + rectPanel.Width >= rectTarget.X)    // Panel 在 Target 左边，部分重叠
            {
                Point[] points = new Point[4];
                points[0] = new Point(rectTarget.X, rectTarget.Y + rectTarget.Height / 2);

                int x_middle = rectPanel.X - DELTA;
                points[1] = new Point(x_middle, rectTarget.Y + rectTarget.Height / 2);
                points[2] = new Point(x_middle, rectPanel.Y + rectPanel.Height / 2);

                points[3] = new Point(rectPanel.X, rectPanel.Y + rectPanel.Height / 2);
                return points;
            }
            else  // Panel 在 Target 左边
            {
                Point[] points = new Point[4];
                points[0] = new Point(rectTarget.X + rectTarget.Width, rectTarget.Y + rectTarget.Height / 2);

                int x_middle = (rectPanel.X + rectPanel.Width
    + rectTarget.X)
    / 2;
                points[1] = new Point(x_middle, rectTarget.Y + rectTarget.Height / 2);
                points[2] = new Point(x_middle, rectPanel.Y + rectPanel.Height / 2);

                points[3] = new Point(rectPanel.X, rectPanel.Y + rectPanel.Height / 2);
                return points;
            }

            // return new Point[0];
        }

#if NO
        bool _highlightPanel = false;
        /// <summary>
        /// 面板是否显示为明亮状态
        /// </summary>
        public bool HighlightPanel
        {
            get
            {
                return this._highlightPanel;
            }
            set
            {
                this._highlightPanel = value;
                this.Invalidate();
            }
        }
#endif
        PanelState _panelState = PanelState.HilightForm;
        public PanelState PanelState
        {
            get
            {
                return _panelState;
            }
            set
            {
                _panelState = value;
                this.Invalidate();
            }
        }

        bool _transparent = false;
        public bool Transparent
        {
            get
            {
                return this._transparent;
            }
            set
            {
                this._transparent = value;
                this.Invalidate();
            }
        }

        int _disableUpdate = 0;
        public void DisableUpdate()
        {
            _disableUpdate++;
        }

        public void EnableUpdate()
        {
            _disableUpdate--;
        }

        public new void Invalidate()
        {
            if (_disableUpdate > 0)
                return;

            base.Invalidate();
        }
        

        private void LineLayerForm_Activated(object sender, EventArgs e)
        {
            int i = 0;
            i++;
        }
    }

    public enum PanelState
    {
        HilightPanel = 1,   // 面板明亮
        HilightForm = 2,    // 基本窗口明亮
    }
}
