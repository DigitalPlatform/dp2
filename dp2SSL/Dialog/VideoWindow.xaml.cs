using System;
using System.Collections.Generic;
using System.IO;
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

namespace dp2SSL.Dialog
{
    /// <summary>
    /// VideoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoWindow : Window
    {
        public VideoWindow()
        {
            InitializeComponent();

            this.Loaded += (o, e) => {
                okButton.Focus();
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        public string MessageText
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
                if (string.IsNullOrEmpty(value))
                {
                    this.photo.Visibility = Visibility.Visible;
                    this.text.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.photo.Visibility = Visibility.Collapsed;
                    this.text.Visibility = Visibility.Visible;
                }
            }
        }

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

#if OLD
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
#endif

        public void SetPhoto(Stream stream)
        {
            if (stream == null)
            {
                this.photo.Source = null;
                return;
            }
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = stream;
            imageSource.CacheOption = BitmapCacheOption.OnLoad; // 2023/12/15
            imageSource.EndInit();
            this.photo.Source = imageSource;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
                return;
            }
        }
    }
}
