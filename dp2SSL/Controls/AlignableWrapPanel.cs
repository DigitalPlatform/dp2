using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace dp2SSL
{
    // 紧密排放的 Panel
    public class CompactWrapPanel : WrapPanel
    {
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsArrange));


        protected override Size MeasureOverride(Size constraint)
        {
            Size panelSize = new Size();
            UIElementCollection children = base.InternalChildren;

            if (children.Count == 0)
                return panelSize;

            foreach (UIElement child in children)
            {
                child.Measure(constraint);
            }

            var first_child = children[0];

            // 计算一共可放置几列
            var column_count = Math.Max(1, (int)(constraint.Width / first_child.DesiredSize.Width));
            // var width = first_child.DesiredSize.Width;

            Layout(children,
    column_count,
    (x, y, child) =>
    {
        // 推动
        panelSize.Width = Math.Max(panelSize.Width, x + child.DesiredSize.Width);
        panelSize.Height = Math.Max(panelSize.Height, y + child.DesiredSize.Height);

    });

#if REMOVED
            // 计算每一列的平均高度
            var average_height = total_height / column_count;

            // 排列每个子元素的位置
            List<Point> points = new List<Point>();
            int current_column_index = 0;
            double x = 0;
            double y = 0;
            foreach (UIElement child in children)
            {
                // x, y, width, child.DesiredSize.Height
                points.Add(new Point(x, y));

                // 推动
                panelSize.Width = Math.Max(panelSize.Width, x + child.DesiredSize.Width);
                panelSize.Height = Math.Max(panelSize.Height, y + child.DesiredSize.Height);

                y += child.DesiredSize.Height;
                if (y > average_height)
                {
                    y = 0;
                    x += width;
                }
            }
#endif
            return panelSize;
        }

        delegate void delegate_placeChild(double x, double y, UIElement child);

        void Layout(UIElementCollection children,
            int column_count,
            delegate_placeChild func_placeChild)
        {
            var first_child = children[0];

            // 每列宽度
            // TODO: 可以改用平均 cell 宽度
            var width = first_child.DesiredSize.Width;

            // 特殊情况，每列一个
            if (column_count >= children.Count)
            {
                double x = 0;
                double y = 0;
                foreach (UIElement child in children)
                {
                    func_placeChild.Invoke(x, y, child);

                    x += width;
                }
                return;
            }


            // 计算所有子元素的高度和
            double total_height = 0;
            foreach (UIElement child in children)
            {
                // child.Measure(constraint);
                total_height += child.DesiredSize.Height;
            }

            // 计算每一列的平均高度
            var average_column_height = total_height / column_count;

            // 计算单元格的平均高度
            var average_cell_height = total_height / children.Count;

            {
                double x = 0;
                double y = 0;
                int column_index = 0;
                foreach (UIElement child in children)
                {
                    // 几种换列方法:
                    // 1) 上线过了平均高度，换列;
                    // 2) 中线过了平均高度，换列;
                    // 3) 下线过了平均高度，换列。
                    if (y + (/*child.DesiredSize.Height*/average_cell_height / 2) > average_column_height
                        && y > 0/*确保每列至少会有一个 cell*/
                        && column_index < column_count - 1)
                    {
                        y = 0;
                        x += width;
                        column_index++;
                    }


                    {
                        func_placeChild.Invoke(x, y, child);

                        y += child.DesiredSize.Height;
                    }

                }
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            UIElementCollection children = base.InternalChildren;

            if (children.Count == 0)
                return arrangeBounds;

            var first_child = children[0];

            // 计算一共可放置几列
            var column_count = Math.Max(1, (int)(arrangeBounds.Width / first_child.DesiredSize.Width));
            // var width = first_child.DesiredSize.Width;

            Layout(children,
                column_count,
                (x, y, child) =>
                {
                    child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
                });

            return arrangeBounds;

#if REMOVED
            int firstInLine = 0;
            Size curLineSize = new Size();
            double accumulatedHeight = 0;
            UIElementCollection children = this.InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                Size sz = children[i].DesiredSize;

                if (curLineSize.Width + sz.Width > arrangeBounds.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                    accumulatedHeight += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
                    {
                        ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                        accumulatedHeight += sz.Height;
                        curLineSize = new Size();
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            if (firstInLine < children.Count)
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);

            return arrangeBounds;
#endif
        }

        private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
        {
            double x = 0;
            if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
            {
                x = (boundsWidth - lineSize.Width) / 2;
            }
            else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
            {
                x = (boundsWidth - lineSize.Width);
            }

            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i];
                if (this.VerticalContentAlignment == VerticalAlignment.Top)
                    child.Arrange(new Rect(x, y, child.DesiredSize.Width, /*lineSize.Height*/child.DesiredSize.Height));

                else if (this.VerticalContentAlignment == VerticalAlignment.Center)
                {
                    var delta = (lineSize.Height - child.DesiredSize.Height) / 2;
                    child.Arrange(new Rect(x, y + delta, child.DesiredSize.Width, child.DesiredSize.Height));
                }
                else if (this.VerticalContentAlignment == VerticalAlignment.Bottom)
                {
                    var delta = (lineSize.Height - child.DesiredSize.Height);
                    child.Arrange(new Rect(x, y + delta, child.DesiredSize.Width, child.DesiredSize.Height));
                }
                else
                    child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));

                x += child.DesiredSize.Width;
            }
        }
    }



    // https://w3toppers.com/wpf-how-can-i-center-all-items-in-a-wrappanel/
    public class AlignableWrapPanel : WrapPanel
    {
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        // 2023/12/14
        public VerticalAlignment VerticalContentAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsArrange));


        protected override Size MeasureOverride(Size constraint)
        {
            Size curLineSize = new Size();
            Size panelSize = new Size();

            UIElementCollection children = base.InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i] as UIElement;

                // Flow passes its own constraint to children
                child.Measure(constraint);
                Size sz = child.DesiredSize;

                if (curLineSize.Width + sz.Width > constraint.Width) //need to switch to another line
                {
                    panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line                    
                    {
                        panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                        panelSize.Height += sz.Height;
                        curLineSize = new Size();
                    }
                }
                else //continue to accumulate a line
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            // the last line size, if any need to be added
            panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
            panelSize.Height += curLineSize.Height;

            return panelSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            int firstInLine = 0;
            Size curLineSize = new Size();
            double accumulatedHeight = 0;
            UIElementCollection children = this.InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                Size sz = children[i].DesiredSize;

                if (curLineSize.Width + sz.Width > arrangeBounds.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                    accumulatedHeight += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
                    {
                        ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                        accumulatedHeight += sz.Height;
                        curLineSize = new Size();
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            if (firstInLine < children.Count)
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);

            return arrangeBounds;
        }

        private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
        {
            double x = 0;
            if (this.HorizontalContentAlignment == HorizontalAlignment.Center)
            {
                x = (boundsWidth - lineSize.Width) / 2;
            }
            else if (this.HorizontalContentAlignment == HorizontalAlignment.Right)
            {
                x = (boundsWidth - lineSize.Width);
            }

            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                UIElement child = children[i];
                if (this.VerticalContentAlignment == VerticalAlignment.Top)
                    child.Arrange(new Rect(x, y, child.DesiredSize.Width, /*lineSize.Height*/child.DesiredSize.Height));

                else if (this.VerticalContentAlignment == VerticalAlignment.Center)
                {
                    var delta = (lineSize.Height - child.DesiredSize.Height) / 2;
                    child.Arrange(new Rect(x, y + delta, child.DesiredSize.Width, child.DesiredSize.Height));
                }
                else if (this.VerticalContentAlignment == VerticalAlignment.Bottom)
                {
                    var delta = (lineSize.Height - child.DesiredSize.Height);
                    child.Arrange(new Rect(x, y + delta, child.DesiredSize.Width, child.DesiredSize.Height));
                }
                else
                    child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));

                x += child.DesiredSize.Width;
            }
        }
    }
}
