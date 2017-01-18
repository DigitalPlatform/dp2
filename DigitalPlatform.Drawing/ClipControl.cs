using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using DigitalPlatform.GUI;

namespace DigitalPlatform.Drawing
{
    /// <summary>
    /// 用于手动设置和调整剪裁矩形的控件
    /// </summary>
    public partial class ClipControl : PictureBox
    {
        // Rectangle _rectangle = new Rectangle(0, 0, 0, 0);

        // 顺序为 左上 右上 右下 左下
        List<Point> _points = new List<Point>() { new Point(10,10),
            new Point(100,10), 
            new Point(100,100),
            new Point(10,100)};

        public void InitialPoints(Image image)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            int delta = image.Width / 20;
            rect.Inflate(-delta, -delta);

            List<Point> source_points = new List<Point>();
            source_points.Add(new Point(rect.X, rect.Y));
            source_points.Add(new Point(rect.X + rect.Width, rect.Y));
            source_points.Add(new Point(rect.X + rect.Width, rect.Y + rect.Height));
            source_points.Add(new Point(rect.X, rect.Y + rect.Height));
            this._points = source_points;
            this.Invalidate();
        }

        public void SetPoints(List<Point> points)
        {
            if (points.Count != 4)
                throw new ArgumentException("points 参数必须包含四个元素", "points");

            _points = points;
            this.Invalidate();
        }

        public List<AForge.IntPoint> GetCorners()
        {
            List<AForge.IntPoint> results = new List<AForge.IntPoint>();
            Point p = _points[0];
            results.Add(new AForge.IntPoint(p.X, p.Y));

            p = _points[1];
            results.Add(new AForge.IntPoint(p.X, p.Y));

            p = _points[2];
            results.Add(new AForge.IntPoint(p.X, p.Y));

            p = _points[3];
            results.Add(new AForge.IntPoint(p.X, p.Y));

            return results;
        }

        public ClipControl()
        {
            InitializeComponent();
        }

#if NO
        public Rectangle ClipRect
        {
            get
            {
                return this._rectangle;
            }
            set
            {
                this._rectangle = value;

                this.Invalidate();
            }
        }
#endif
        public Rectangle GetPictureBoxZoomSize()
        {
            PropertyInfo info = typeof(PictureBox).GetProperty
                ("ImageRectangle",
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Instance);

            return (Rectangle)info.GetValue(this, null);
        }

        // 从屏幕坐标转换为内部坐标
        PointF ScreenToPhysic(Point p)
        {
            if (this.Image == null)
                return new PointF(0, 0);

            Rectangle display_rect = GetPictureBoxZoomSize();
            float x_ratio = (float)display_rect.Width / (float)this.Image.Width;
            float y_ratio = (float)display_rect.Height / (float)this.Image.Height;

            return new PointF(
                (float)p.X / x_ratio
            - (float)display_rect.X / x_ratio, 
            (float)p.Y / y_ratio
            - (float)display_rect.Y / y_ratio);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (this.Image == null)
                return;

            Rectangle display_rect = GetPictureBoxZoomSize();
            float x_ratio = (float)display_rect.Width / (float)this.Image.Width;
            float y_ratio = (float)display_rect.Height / (float)this.Image.Height;

            pe.Graphics.ScaleTransform(x_ratio, y_ratio);
            // 平移 display_rect
            pe.Graphics.TranslateTransform((float)display_rect.X / x_ratio,
                (float)display_rect.Y / y_ratio);

            using (Pen pen = new Pen(Color.Red))
            {
                float scale_ratio = (float)1 / x_ratio;
                // 绘制四个角的方块
                // 方块大小是会根据缩放比率自动改变的。主要是为了人的分辨和点击需要。
                List<RectangleF> rects = new List<RectangleF>();
                rects.Add(GetBoxRect(_points[0], scale_ratio));
                rects.Add(GetBoxRect(_points[1], scale_ratio));
                rects.Add(GetBoxRect(_points[2], scale_ratio));
                rects.Add(GetBoxRect(_points[3], scale_ratio));
                using (Brush brush = new SolidBrush(Color.Red))
                {
                    pe.Graphics.FillRectangles(brush, rects.ToArray());
                }

                pe.Graphics.DrawLine(pen, _points[0], _points[1]);
                pe.Graphics.DrawLine(pen, _points[1], _points[2]);
                pe.Graphics.DrawLine(pen, _points[2], _points[3]);
                pe.Graphics.DrawLine(pen, _points[3], _points[0]);
            }
        }

        HitTestResult _startHit = null;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
            {
                // 防止在卷滚条上单击后拖动造成副作用
                goto END1;
            }

            this.Capture = true;

            this.Focus();

            // Graphics g = Graphics.FromHwnd(this.Handle);
            // g.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.Page, System.Drawing.Drawing2D.CoordinateSpace.World, pts);
            PointF p1 = ScreenToPhysic(new Point(e.X, e.Y));

            // 屏幕坐标
            _startHit = this.HitTest(
                p1.X,
                p1.Y);

        END1:
            base.OnMouseDown(e);
        }

        void ChangeCornerPosition(CornerType type, int x, int y)
        {
            if (type == CornerType.LeftTop)
            {
                _points[0] = new Point(x, y);
            }
            if (type == CornerType.RightTop)
            {
                _points[1] = new Point(x, y);
            }

            if (type == CornerType.RightBottom)
            {
                _points[2] = new Point(x, y);
            }

            if (type == CornerType.LeftBottom)
            {
                _points[3] = new Point(x, y);
            }

            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_startHit != null && _startHit.CornerType != CornerType.None)
            {
                PointF p1 = ScreenToPhysic(new Point(e.X, e.Y));

                ChangeCornerPosition(_startHit.CornerType, Convert.ToInt32(p1.X), Convert.ToInt32(p1.Y));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Capture = false;
            base.OnMouseUp(e);

            if (_startHit != null && _startHit.CornerType != CornerType.None)
            {
                PointF p1 = ScreenToPhysic(new Point(e.X, e.Y));

                ChangeCornerPosition(_startHit.CornerType, Convert.ToInt32(p1.X), Convert.ToInt32(p1.Y));
            }

            _startHit = null;
        }

        const int BOX_WIDTH = 8;
        const int BOX_HEIGHT = 8;

        static RectangleF GetBoxRect(Point p, float ratio)
        {
            return GetBoxRect(p.X, p.Y, ratio);
        }

        static RectangleF GetBoxRect(int x, int y, float ratio)
        {
            float box_width = (float)BOX_WIDTH * ratio;
            float box_height = (float)BOX_HEIGHT * ratio;
            return new RectangleF((float)(x - box_width / 2),
                (float)(y - box_height / 2),
                box_width,
                box_height);
        }

        // parameters:
        //      x   内部坐标
        HitTestResult HitTest(float x, float y)
        {
            HitTestResult result = new HitTestResult();
            result.X = x;
            result.Y = y;
            result.CornerType = CornerType.None;

            if (this.Image == null)
                return result;

            Rectangle display_rect = GetPictureBoxZoomSize();
            float x_ratio = (float)display_rect.Width / (float)this.Image.Width;
            float scale_ratio = (float)1 / x_ratio;

            // 左上
            RectangleF rect = GetBoxRect(_points[0], scale_ratio);
            if (GuiUtil.PtInRect(x,
    y,
    rect) == true)
            {
                result.CornerType = CornerType.LeftTop;
                return result;
            }

            // 右上
            rect = GetBoxRect(_points[1], scale_ratio);
            if (GuiUtil.PtInRect(x,
    y,
    rect) == true)
            {
                result.CornerType = CornerType.RightTop;
                return result;
            }

            // 右下
            rect = GetBoxRect(_points[2], scale_ratio);
            if (GuiUtil.PtInRect(x,
    y,
    rect) == true)
            {
                result.CornerType = CornerType.RightBottom;
                return result;
            }

            // 左下
            rect = GetBoxRect(_points[3], scale_ratio);
            if (GuiUtil.PtInRect(x,
    y,
    rect) == true)
            {
                result.CornerType = CornerType.LeftBottom;
                return result;
            }

            return result;
        }

        public List<Point> ToPoints(
            float angle,
            Rectangle rect)
        {
            // Graphics g = Graphics.FromImage(this.Image);
            Graphics g = Graphics.FromHwnd(this.Handle);

            List<Point> source_points = new List<Point>();
            source_points.Add(new Point(rect.X, rect.Y));
            source_points.Add(new Point(rect.X + rect.Width, rect.Y));
            source_points.Add(new Point(rect.X +rect.Width, rect.Y + rect.Height));
            source_points.Add(new Point(rect.X, rect.Y + rect.Height));
            Point[] pts = source_points.ToArray();

            Rectangle display_rect = GetPictureBoxZoomSize();
            float x_ratio = (float)display_rect.Width / (float)this.Image.Width;
            float y_ratio = (float)display_rect.Height / (float)this.Image.Height;

            System.Drawing.Drawing2D.Matrix rotateMatrix =
    new System.Drawing.Drawing2D.Matrix();
            // Set the rotation angle and starting point for the text.
            rotateMatrix.RotateAt(angle, new PointF(this.Image.Width / 2, this.Image.Height / 2));
            //rotateMatrix.RotateAt(angle, new PointF(0, 0));

            g.MultiplyTransform(rotateMatrix);

            // g.ScaleTransform(x_ratio, y_ratio);
            // g.ScaleTransform((float)1.1, (float)1.1);
            // g.RotateTransform(angle);
            g.TransformPoints(System.Drawing.Drawing2D.CoordinateSpace.World, System.Drawing.Drawing2D.CoordinateSpace.Device, pts);
            return pts.ToList();
        }

#if NO
        public List<Point> ToPoints(
    float angle,
    Rectangle rect)
        {
            // Create a GraphicsPath.
            System.Drawing.Drawing2D.GraphicsPath path =
                new System.Drawing.Drawing2D.GraphicsPath();

            path.AddRectangle(rect);

            // Declare a matrix that will be used to rotate the text.
            System.Drawing.Drawing2D.Matrix rotateMatrix =
                new System.Drawing.Drawing2D.Matrix();

            // Set the rotation angle and starting point for the text.
            rotateMatrix.RotateAt(180.0F, new PointF(10.0F, 100.0F));

            // Transform the text with the matrix.
            path.Transform(rotateMatrix);

            List<Point> results = new List<Point>();
            foreach(PointF p in path.PathPoints)
            {
                results.Add(new Point((int)p.X, (int)p.Y));
            }

            path.Dispose();

            return results;
        }
#endif

        public void RotateImage(RotateFlipType flip_type)
        {
            RotatePoints();

            Image image = this.Image;
            image.RotateFlip(flip_type);

            //this.Width = image.Width;
            //this.Height = image.Height;
            ImageUtil.SetImage(this, image);    // 2016/12/28

            this.Invalidate();
        }

        void RotatePoints()
        {
            if (this.Image == null)
                return;

            List<Point> points = new List<Point>();
            points.AddRange(this._points);

            // before
            // 1 2
            // 4 3

            // after
            // 4 1
            // 3 2

            List<Point> results = new List<Point>();
            Point ref_p = points[3];
            results.Add(new Point(this.Image.Height - ref_p.Y, ref_p.X));

            ref_p = points[0];
            results.Add(new Point(this.Image.Height - ref_p.Y, ref_p.X));

            ref_p = points[1];
            results.Add(new Point(this.Image.Height - ref_p.Y, ref_p.X));

            ref_p = points[2];
            results.Add(new Point(this.Image.Height - ref_p.Y, ref_p.X));

            this._points = results;
        }

        public void SelectAll()
        {
            if (this.Image == null)
                return;

            Rectangle rect = new Rectangle(0, 0, this.Image.Width, this.Image.Height);

            List<Point> source_points = new List<Point>();
            source_points.Add(new Point(rect.X, rect.Y));
            source_points.Add(new Point(rect.X + rect.Width, rect.Y));
            source_points.Add(new Point(rect.X + rect.Width, rect.Y + rect.Height));
            source_points.Add(new Point(rect.X, rect.Y + rect.Height));
            this._points = source_points;
            this.Invalidate();
        }

        // 剪裁指令。格式为 s:(?,?);c:(?,?)(?,?)(?,?)(?,?)
        // s 表示尺寸，顺序为宽高；c 表示四个顶点，分别是左上 右上 右下 左下
        public string ClipCommand
        {
            get
            {
                if (_points.Count == 0 || this.Image == null)
                    return "";

                StringBuilder text = new StringBuilder();
                text.Append(string.Format("s:({0},{1});c:", this.Image.Width, this.Image.Height));
                foreach(Point p in _points)
                {
                    text.Append(string.Format("({0},{1})", p.X, p.Y));
                }

                return text.ToString();
            }
        }
    }

    enum CornerType
    {
        None = 0,
        LeftTop = 1,
        RightTop = 2,
        RightBottom = 3,
        LeftBottom = 4,
    }

    class HitTestResult
    {
        public float X { get; set; }
        public float Y { get; set; }
        public CornerType CornerType { get; set; }
    }
}
