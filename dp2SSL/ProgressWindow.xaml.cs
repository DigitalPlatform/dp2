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
    /// ProgressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string MessageText
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
            }
        }

        public ProgressBar ProgressBar
        {
            get
            {
                return progressBar;
            }
        }

#if NO
        bool _errorMode = false;

        public bool ErrorMode
        {
            get
            {
                return _errorMode;
            }
            set
            {
                _errorMode = value;
                if (_errorMode)
                {
                    this.Background = Brushes.DarkRed;
                    this.Foreground = Brushes.White;
                }
                else
                {
                    this.Background = Brushes.Black;
                    this.Foreground = Brushes.White;
                }
            }
        }
#endif
        string _backColor = "black";
        public string BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                if (_backColor == "black")
                {
                    this.Background = Brushes.Black;
                    this.Foreground = Brushes.White;
                }
                if (_backColor == "red")
                {
                    this.Background = Brushes.DarkRed;
                    this.Foreground = Brushes.White;
                }
                if (_backColor == "yellow")
                {
                    this.Background = Brushes.DarkOrange;
                    this.Foreground = Brushes.White;
                }
                if (_backColor == "green")
                {
                    this.Background = Brushes.DarkGreen;
                    this.Foreground = Brushes.White;
                }
                if (_backColor == "gray")
                {
                    this.Background = Brushes.DarkGray;
                    this.Foreground = Brushes.White;
                }
            }
        }
    }
}
