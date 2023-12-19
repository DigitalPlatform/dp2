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
        public string PressedButton { get; set; }

        public ProgressWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PressedButton = "OK";

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
                if (value != null)
                {
                    text.Visibility = Visibility.Visible;
                    richText.Visibility = Visibility.Collapsed;
                }
            }
        }

        public FlowDocument MessageDocument
        {
            get
            {
                return richText.Document;
            }
            set
            {
                /*
                var old = richText.Document;
                richText.Document = null;
                */
                richText.Document = value;
                if (value != null)
                {
                    if (text.Visibility != Visibility.Collapsed)
                        text.Visibility = Visibility.Collapsed;
                    if (richText.Visibility != Visibility.Visible)
                        richText.Visibility = Visibility.Visible;
                }
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
                    this.Background = null;
                    this.Foreground = null;

                    this.Background = this.FindResource("normalBackground") as Brush;
                    this.Foreground = this.FindResource("normalForeground") as Brush;


                    /*
                    if (App.Skin == Skin.Dark)
                    {
                        this.Background = Brushes.Black;
                        this.Foreground = Brushes.White;
                    }
                    else
                    {
                        this.Background = Brushes.Gray;
                        this.Foreground = Brushes.Black;
                    }
                    */
                }
                if (_backColor == "red")
                {
                    this.Background = this.FindResource("redBackground") as Brush;
                    //this.Foreground = Brushes.White;
                }
                if (_backColor == "yellow")
                {
                    this.Background = this.FindResource("yellowBackground") as Brush;
                    //this.Foreground = Brushes.White;
                }
                if (_backColor == "green")
                {
                    this.Background = this.FindResource("greenBackground") as Brush;
                    //this.Foreground = Brushes.White;
                }
                if (_backColor == "gray")
                {
                    this.Background = this.FindResource("grayBackground") as Brush;
                    //this.Foreground = Brushes.White;
                }
            }
        }

        public string TitleText
        {
            get
            {
                return title.Text;
            }
            set
            {
                title.Text = value;
            }
        }

        public string OkButtonText
        {
            get
            {
                return okButton.Content.ToString();
            }
            set
            {
                okButton.Content = value;
            }
        }

        public bool CancelButtonVisible
        {
            get
            {
                return cancelButton.Visibility == Visibility.Visible;
            }
            set
            {
                if (value == true)
                    cancelButton.Visibility = Visibility.Visible;
                else
                    cancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            PressedButton = "Cancel";

            this.Close();
        }
    }
}
