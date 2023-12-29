using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestWpfSkin
{
    public class MyScrollViewer : ScrollViewer
    {
        MouseCapture _capture = null;

        protected override void OnTouchDown(TouchEventArgs e)
        {
            // 点击到卷滚条上的直接返回
            var point = e.GetTouchPoint(this).Position;
            if ((this.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
                 && point.X >= this.ViewportWidth - 10)
                || (this.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled
                && point.Y >= this.ViewportHeight - 10))
            {
                base.OnTouchDown(e);
                return;
            }

            this.CaptureTouch(e.TouchDevice);

            _capture = new MouseCapture();

            _capture.VerticalOffset = this.VerticalOffset;
            _capture.Point = e.GetTouchPoint(this).Position;

            base.OnTouchDown(e);
        }

        protected override void OnTouchMove(TouchEventArgs e)
        {
            if (_capture == null)
            {
                base.OnTouchMove(e);
                return;
            }


            /*
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                // _captures.Remove(sender);
                RemoveCapture(this);
                base.OnPreviewMouseMove(e);
                return;
            }
            */
            var point = e.GetTouchPoint(this).Position;

            var dy = point.Y - _capture.Point.Y;
            if (Math.Abs(dy) > 5)
            {
                /*
                if (capture.Captured == false)
                {
                    target.CaptureMouse();
                    capture.Captured = true;
                }
                */
            }

            this.ScrollToVerticalOffset(_capture.VerticalOffset - dy);

            // base.OnTouchMove(e);
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            this.ReleaseTouchCapture(e.TouchDevice);
            _capture = null;
            base.OnTouchUp(e);
        }

#if REMOVED
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var target = this;

            // 点击到卷滚条上的直接返回
            var point = e.GetPosition(target);
            if ((target.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
                 && point.X >= target.ViewportWidth - 10)
                || (target.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled
                && point.Y >= target.ViewportHeight - 10))
            {
                base.OnPreviewMouseLeftButtonDown(e);
                return;
            }

            MouseCapture capture = null;
            if (_captures.ContainsKey(this) == false)
            {
                capture = new MouseCapture();
                _captures[this] = capture;
            }
            else
                capture = _captures[this];

            capture.VerticalOffset = target.VerticalOffset;
            capture.Point = e.GetPosition(target);

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var target = this;

            // target.ReleaseMouseCapture();
            RemoveCapture(this);

            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (!_captures.ContainsKey(this)) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                // _captures.Remove(sender);
                RemoveCapture(this);
                base.OnPreviewMouseMove(e);
                return;
            }

            var target = this;

            var capture = _captures[this];

            var point = e.GetPosition(target);

            var dy = point.Y - capture.Point.Y;
            if (Math.Abs(dy) > 5)
            {
                /*
                if (capture.Captured == false)
                {
                    target.CaptureMouse();
                    capture.Captured = true;
                }
                */
            }

            target.ScrollToVerticalOffset(capture.VerticalOffset - dy);

            base.OnPreviewMouseMove(e);
        }
#endif

        static void RemoveCapture(ScrollViewer target)
        {
            if (target == null)
                return;

            if (_captures.ContainsKey(target) == false)
                return;

            var capture = _captures[target];

            if (capture != null
                && _captures.Remove(target) == true)
            {
                if (capture.Captured)
                    target.ReleaseMouseCapture();
            }
        }

        internal class MouseCapture
        {
            public Double VerticalOffset { get; set; }
            public Point Point { get; set; }

            public bool Captured { get; set; }
        }

        static Dictionary<object, MouseCapture> _captures = new Dictionary<object, MouseCapture>();
    }

}
