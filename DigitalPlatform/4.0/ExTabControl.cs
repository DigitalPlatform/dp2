using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
#if REMOVED
    public class ExTabControl : TabControl
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Brush brush0 = null;

            if (this.Enabled == false)
                brush0 = new SolidBrush(Color.LightGray);
            else
                brush0 = new SolidBrush(Color.Black);  // this.BackColor

            e.Graphics.FillRectangle(brush0, e.ClipRectangle);

            brush0.Dispose();
            // base.OnPaint(e);
        }
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Brush brush0 = null;

            if (this.Enabled == false)
                brush0 = new SolidBrush(Color.LightGray);
            else
                brush0 = new SolidBrush(Color.Black);  // this.BackColor

            pevent.Graphics.FillRectangle(brush0, pevent.ClipRectangle);

            brush0.Dispose();

            // base.OnPaintBackground(pevent);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            TabPage CurrentTab = this.TabPages[e.Index];
            Rectangle ItemRect = this.GetTabRect(e.Index);
            using (SolidBrush FillBrush = new SolidBrush(Color.Black)) // Color.Red
            using (SolidBrush TextBrush = new SolidBrush(this.ForeColor)) // Color.White
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                //If we are currently painting the Selected TabItem we'll
                //change the brush colors and inflate the rectangle.
                if (System.Convert.ToBoolean(e.State & DrawItemState.Selected))
                {
                    FillBrush.Color = this.ForeColor;  //Color.White;
                    TextBrush.Color = this.BackColor;  // Color.Red;
                    ItemRect.Inflate(2, 2);
                }

                //Set up rotation for left and right aligned tabs
                if (this.Alignment == TabAlignment.Left || this.Alignment == TabAlignment.Right)
                {
                    float RotateAngle = 90;
                    if (this.Alignment == TabAlignment.Left)
                        RotateAngle = 270;
                    PointF cp = new PointF(ItemRect.Left + (ItemRect.Width / 2), ItemRect.Top + (ItemRect.Height / 2));
                    e.Graphics.TranslateTransform(cp.X, cp.Y);
                    e.Graphics.RotateTransform(RotateAngle);
                    ItemRect = new Rectangle(-(ItemRect.Height / 2), -(ItemRect.Width / 2), ItemRect.Height, ItemRect.Width);
                }

                //Next we'll paint the TabItem with our Fill Brush
                e.Graphics.FillRectangle(FillBrush, ItemRect);

                //Now draw the text.
                e.Graphics.DrawString(CurrentTab.Text, e.Font, TextBrush, (RectangleF)ItemRect, sf);

                //Reset any Graphics rotation
                e.Graphics.ResetTransform();
            }
        }
    }

#endif
}
