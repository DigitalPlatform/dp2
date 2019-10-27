using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dp2SSL
{
    /// <summary>
    /// NumberKeyboardControl.xaml 的交互逻辑
    /// </summary>
    public partial class NumberKeyboardControl : UserControl
    {
        public string Text { get; set; }

        public event KeyPressedEventHandler KeyPressed;

        public NumberKeyboardControl()
        {
            InitializeComponent();
        }

        private void Key_0_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string content = button.Content as string;

            if (this.Text == null)
                this.Text = "";

            string key = content;
            if (key == "Delete")
            {
                if (this.Text.Length > 0)
                    this.Text = this.Text.Substring(0, this.Text.Length - 1);   // 删除最后一位
            }
            else if (key == "Enter")
            {
                key = "\r";
            }
            else
                this.Text += key;

            KeyPressed?.Invoke(this, new KeyPressedEventArgs { Key = key[0] });
        }
    }

    public delegate void KeyPressedEventHandler(object sender,
KeyPressedEventArgs e);

    /// <summary>
    /// 按键事件的参数
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        public char Key { get; set; }
    }
}
