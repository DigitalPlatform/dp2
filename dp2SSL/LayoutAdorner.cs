using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace dp2SSL
{
    public class LayoutAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public LayoutAdorner(UIElement adornedElement)
          : base(adornedElement)
        {
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {

            Rect adornedElementRect = new Rect(
                this.AdornedElement.RenderSize
                );

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Color.FromRgb(50, 50, 50));   // 10
            renderBrush.Opacity = 0.7;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            double renderRadius = 5.0;

#if NO
            double blurRadius = 50;
            drawingContext.RenderBlurred((int)adornedElementRect.Width, (int)adornedElementRect.Height, adornedElementRect, blurRadius,
                dc => dc.DrawRectangle(renderBrush, renderPen, adornedElementRect));
#endif
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);

#if NO
            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
#endif
        }
    }


    public static class DrawingContextExtension
    {
        public static void RenderBlurred(this DrawingContext dc, int width, int height, Rect targetRect, double blurRadius, Action<DrawingContext> action)
        {
            Rect elementRect = new Rect(0, 0, width, height);
            BlurredElement element = new BlurredElement(action)
            {
                Width = width,
                Height = height,
                Effect = new BlurEffect() { Radius = blurRadius }
            };
            element.Arrange(elementRect);
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(element);
            dc.DrawImage(rtb, targetRect);
        }

        class BlurredElement : FrameworkElement
        {
            Action<DrawingContext> action;
            public BlurredElement(Action<DrawingContext> action)
            {
                this.action = action;
            }
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                action(drawingContext);
            }
        }
    }
}
