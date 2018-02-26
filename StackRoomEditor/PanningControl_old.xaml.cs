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
    public partial class PanningControlOld : UserControl
    {
        public PanningControlOld()
        {
            InitializeComponent();

            Polygon p = new Polygon();
            p.Points.Add(new Point(10, 20));
            p.Points.Add(new Point(30, 0));
            p.Points.Add(new Point(30, 40));
            p.Points.Add(new Point(10, 20));
            p.Stroke = new SolidColorBrush(Colors.Black);
            p.StrokeThickness = 1;
            ScaleTransform t = new ScaleTransform();
            t.ScaleX = canvas1.ActualWidth / 200;
            t.ScaleY = canvas1.ActualHeight / 200;
            p.RenderTransform = t;

            canvas1.Children.Add(p);
        }

    }
}
