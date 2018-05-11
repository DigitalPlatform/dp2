using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StackRoomEditor
{
    /// <summary>
    /// PanningControl.xaml 的交互逻辑
    /// </summary>
    public partial class PanningControl : UserControl
    {
        public event ButtonClickEventHandler ButtonClick = null;

        Brush old_brush = null;
        Path currrent_path = null;

        public PanningControl()
        {
            InitializeComponent();
        }

        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas1.CaptureMouse();

            if (this.ButtonClick == null)
                return;

            Point p = Mouse.GetPosition(canvas1);
            bool bHorzBar = false;
            if (p.Y > canvas1.ActualHeight / 3
    && p.Y < canvas1.ActualHeight * ((double)2 / (double)3))
            {
                // 在水平条带上
                bHorzBar = true;
            }
            bool bVertBar = false;
            if (p.X > canvas1.ActualWidth / 3
    && p.X < canvas1.ActualWidth * ((double)2 / (double)3))
            {
                // 在垂直条带上
                bVertBar = true;
            }

            ButtonClickEventArgs e1 = new ButtonClickEventArgs();

            if (bHorzBar == true && bVertBar == true)
            {
                // 正中
                e1.ButtonName = "center";
                this.ButtonClick(this, e1);
                return;
            }

            Brush brush = new SolidColorBrush(Colors.Black);

            if (bHorzBar == true)
            {
                if (p.X < canvas1.ActualWidth / 3)
                {
                    // left
                    e1.ButtonName = "left";

                    this.currrent_path = this.left;
                    old_brush = this.currrent_path.Fill;
                    this.currrent_path.Fill = brush;

                    this.ButtonClick(this, e1);
                    return;
                }
                else
                {
                    // right
                    e1.ButtonName = "right";

                    this.currrent_path = this.right;
                    old_brush = this.currrent_path.Fill;
                    this.currrent_path.Fill = brush;

                    this.ButtonClick(this, e1);
                    return;
                }
            }

            if (bVertBar == true)
            {
                if (p.Y < canvas1.ActualHeight / 3)
                {
                    // top
                    e1.ButtonName = "top";

                    this.currrent_path = this.top;
                    old_brush = this.currrent_path.Fill;
                    this.currrent_path.Fill = brush;

                    this.ButtonClick(this, e1);
                    return;
                }
                else
                {
                    // bottom
                    e1.ButtonName = "bottom";

                    this.currrent_path = this.bottom;
                    old_brush = this.currrent_path.Fill;
                    this.currrent_path.Fill = brush;

                    this.ButtonClick(this, e1);
                    return;
                }
            }


        }

        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas1.ReleaseMouseCapture();

            if (this.old_brush != null && currrent_path != null)
            {
                this.currrent_path.Fill = old_brush;
            }

            this.currrent_path = null;
            this.old_brush = null;
        }
    }



    public delegate void ButtonClickEventHandler(object sender,
ButtonClickEventArgs e);

    /// <summary>
    /// 空闲事件的参数
    /// </summary>
    public class ButtonClickEventArgs : EventArgs
    {
        public string ButtonName = "";
    }

}