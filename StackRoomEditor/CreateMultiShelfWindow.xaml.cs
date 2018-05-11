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
    /// CreateMultiShelfWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateMultiShelfWindow : Window
    {
        public int Count = 5;

        public CreateMultiShelfWindow()
        {
            InitializeComponent();
        }

        private void textBox_count_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_count.Text) == false)
                this.button_OK.IsEnabled = true;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            int v = 0;

            if (Int32.TryParse(this.textBox_count.Text, out v) == false)
            {
                MessageBox.Show(this, "请输入纯数字");
                return;
            }

            this.Count = v;

            this.DialogResult = true;
            this.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBox_count.Text = this.Count.ToString();
        }
    }
}
