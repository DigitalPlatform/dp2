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

namespace dp2SSL
{
    /// <summary>
    /// InputPasswordWindows.xaml 的交互逻辑
    /// </summary>
    public partial class InputPasswordWindows : Window
    {
        public InputPasswordWindows()
        {
            InitializeComponent();

            this.Loaded += InputPasswordWindows_Loaded;
            this.keyborad.KeyPressed += Keyborad_KeyPressed;
        }

        private void InputPasswordWindows_Loaded(object sender, RoutedEventArgs e)
        {
            this.password.Focus();
        }

        private void Keyborad_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == '\r')
            {
                LoginButton_Click(this.loginButton, new RoutedEventArgs());
                return;
            }

            this.password.Password = this.keyborad.Text;
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
    }
}
