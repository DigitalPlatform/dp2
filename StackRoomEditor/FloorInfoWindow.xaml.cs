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
using System.Windows.Shapes;

namespace StackRoomEditor
{
    /// <summary>
    /// FloorInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FloorInfoWindow : Window
    {
        public Floor Floor = null;

        public FloorInfoWindow()
        {
            InitializeComponent();
        }

        public void PutInfo(Floor floor)
        {
            this.Floor = floor;

            this.textBox_width.Text = this.Floor.Width.ToString();
            this.textBox_height.Text = this.Floor.Height.ToString();
            this.textBox_centerX.Text = this.Floor.X.ToString();
            this.textBox_centerZ.Text = this.Floor.Z.ToString();
        }

        public void GetInfo(Floor floor)
        {
            double v = 0;
            bool bRet = double.TryParse(this.textBox_centerX.Text, out v);
            if (bRet == false)
                throw new Exception("中心位置 X 值 '" + this.textBox_centerX.Text + "' 格式错误");
            floor.X = v;

            v = 0;
            bRet = double.TryParse(this.textBox_centerZ.Text, out v);
            if (bRet == false)
                throw new Exception("中心位置 Z 值 '" + this.textBox_centerZ.Text + "' 格式错误");

            floor.Z = v;

            v = 0;
            bRet = double.TryParse(this.textBox_width.Text, out v);
            if (bRet == false)
                throw new Exception("宽度 值 '" + this.textBox_width.Text + "' 格式错误");

            floor.Width = v;

            v = 0;
            bRet = double.TryParse(this.textBox_height.Text, out v);
            if (bRet == false)
                throw new Exception("高度 值 '" + this.textBox_height.Text + "' 格式错误");

            floor.Height = v;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
