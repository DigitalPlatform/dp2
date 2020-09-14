using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        // 键盘页
        int _pageNo = 0;
        bool _capsLock = false;

        public event KeyPressedEventHandler KeyPressed;

        public NumberKeyboardControl()
        {
            InitializeComponent();
        }

        private void Key_0_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase button = (ButtonBase)sender;

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
            else if (key == "<")
            {
                if (_pageNo > 0)
                {
                    _pageNo--;
                    RefreshKeys();
                }
            }
            else if (key == ">")
            {
                if (_pageNo < 3)
                {
                    _pageNo++;
                    RefreshKeys();
                }
            }
            else if (key.ToLower() == "caps")
            {
                _capsLock = !_capsLock;
                RefreshKeys();
            }
            else
                this.Text += key;

            KeyPressed?.Invoke(this, new KeyPressedEventArgs { Key = key[0] });
        }

        static string[] _key_layouts = new string[] {
    "7894561230",
    "abcdefghij",
    "klmnopqrst",
    "uvwxyz    "
    };
        void RefreshKeys()
        {
            string keys = _key_layouts[_pageNo];
            this.key_0.Content = GetString(keys[9]);
            this.key_1.Content = GetString(keys[6]);
            this.key_2.Content = GetString(keys[7]);
            this.key_3.Content = GetString(keys[8]);

            this.key_4.Content = GetString(keys[3]);
            this.key_5.Content = GetString(keys[4]);
            this.key_6.Content = GetString(keys[5]);

            this.key_7.Content = GetString(keys[0]);
            this.key_8.Content = GetString(keys[1]);
            this.key_9.Content = GetString(keys[2]);

            if (_capsLock)
                this.key_capslock.Content = "CAPS";
            else
                this.key_capslock.Content = "caps";
        }

        string GetString(char c)
        {
            if (_capsLock)
                c = char.ToUpper(c);
            return new string(c, 1);
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
