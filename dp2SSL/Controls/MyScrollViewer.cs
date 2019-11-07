using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace dp2SSL
{
    public class MyScrollViewer : ScrollViewer
    {
        bool isScrolling = false; //Flag
        Point PresentPoint, PrevPoint;

        // private ScrollViewer scrollViewer;

        /// <summary>
        /// Gets the scroll viewer contained within the FlowDocumentScrollViewer control
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get
            {
                return this;
                /*
                if (this.scrollViewer == null)
                {
                    DependencyObject obj = this;

                    do
                    {
                        if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                            obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                        else
                            return null;
                    }
                    while (!(obj is ScrollViewer));

                    this.scrollViewer = obj as ScrollViewer;
                }

                return this.scrollViewer;
                */
            }
        }

        /*
        public new bool IsSelectionEnabled
        {
            get
            {
                return false;
            }
        }
        */

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            isScrolling = true;
            PrevPoint = Mouse.GetPosition(this);

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            PresentPoint = Mouse.GetPosition(this);
            if (isScrolling == true)
            {
                double DiffY = (PresentPoint.Y - PrevPoint.Y);
                bool Down = DiffY < 0;
                if (Down == true)
                    ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + 10);
                else
                    ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - 10);
            }

            PrevPoint = Mouse.GetPosition(this);

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            isScrolling = false;
            Mouse.Capture(null);
            base.OnMouseLeftButtonUp(e);
        }
    }
}
