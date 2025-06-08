// #define PASSWORDBOX // 如果定义了这个宏，则使用密码框，否则使用文本框。

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
using System.Windows.Shapes;
using WindowsInput;

namespace dp2SSL
{
    /// <summary>
    /// InputPasswordWindows.xaml 的交互逻辑
    /// </summary>
    public partial class InputPasswordWindows : Window
    {
        public string Result { get; set; }

        public InputPasswordWindows()
        {
            InitializeComponent();

            this.Loaded += InputPasswordWindows_Loaded;
            this.keyboard.KeyPressed += Keyboard_KeyPressed;

#if PASSWORDBOX
            // 2025/5/19
            InputMethod.SetIsInputMethodEnabled(this.password, false);
#endif
        }

        private void InputPasswordWindows_Loaded(object sender, RoutedEventArgs e)
        {
#if PASSWORDBOX
            this.password.Focus();
#else
            this.mainGrid.Focus();
#endif
        }

#if PASSWORDBOX
        public string Password
        {
            get
            {
                return this.password.Password;
            }
        }
#else
        string _inputText = "";
        public string Password
        {
            get
            {
                return _inputText;
            }
        }
#endif

        // 定制的屏幕小键盘的消息
        private void Keyboard_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == '\r' || e.Key == '\n')
            {
                LoginButton_Click(this.loginButton, new RoutedEventArgs());
                return;
            }

#if PASSWORDBOX
            this.password.Password = this.keyboard.Text;
#else
            this._inputText = this.keyboard.Text;
            this.password.Text = new string('*', _inputText.Length);
#if REMOVED
            if (e.Key == '\b')
            {
                if (_inputText.Length > 0)
                    _inputText = _inputText.Substring(0, _inputText.Length - 1);   // 删除最后一位
            }
            else if (e.Key == (char)Key.Delete)
            {
                int caret_pos = _inputText.Length;  // TODO: 将来支持显示光标位置
                if (_inputText.Length > 0 && caret_pos < _inputText.Length)
                    _inputText = _inputText.Substring(0, caret_pos - 1);   // 删除插入符所在位置的字符
            }
            else
                _inputText += e.Key.ToString();

            this.password.Text = new string('*', _inputText.Length);
#endif

#endif
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = "OK";
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = "Cancel";
            this.Close();
        }

        public string TitleText
        {
            get
            {
                return titleText.Text;
            }
            set
            {
                titleText.Text = value;
            }
        }

        public string LoginButtonText
        {
            get
            {
                return loginButton.Content as string;
            }
            set
            {
                loginButton.Content = value;
            }
        }

        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(this.loginButton, new RoutedEventArgs());
                return;
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

#if !PASSWORDBOX
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(this, new RoutedEventArgs());
                e.Handled = true;   //
                return;
            }


            if (e.Key == Key.Back)
            {
                if (_inputText.Length > 0)
                    _inputText = _inputText.Substring(0, _inputText.Length - 1);   // 删除最后一位
            }
            else if (e.Key == Key.Delete)
            {
                int caret_pos = _inputText.Length;  // TODO: 将来支持显示光标位置
                if (_inputText.Length > 0 && caret_pos < _inputText.Length)
                    _inputText = _inputText.Substring(0, caret_pos - 1);   // 删除插入符所在位置的字符
            }
            else
            {
                char ch = (char)KeyInterop.VirtualKeyFromKey(e.Key);
                _inputText += ch;
            }

            this.password.Text = new string('*', _inputText.Length);
            e.Handled = true;
#endif
        }
    }
}
