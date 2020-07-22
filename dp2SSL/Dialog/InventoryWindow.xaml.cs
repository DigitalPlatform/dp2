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
    /// InventoryWindow.xaml 的交互逻辑
    /// 用于首次初始化盘点图书的对话框
    /// </summary>
    public partial class InventoryWindow : Window
    {
        public DoorItem Door { get; set; }

        public InventoryWindow()
        {
            InitializeComponent();
        }

        /*
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        */

        public string GetMessageText()
        {
            if (richText.Visibility == Visibility.Visible)
            {
                var fd = richText.Document;
                TextRange tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                return tr.Text;
            }

            return MessageText;
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

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            //this.Close();
        }

        private void openDoorButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 开门以后，监控门的状态。如果关门了，则自动开始重试

            //this.DialogResult = true;
            //this.Close();
        }

        private void retryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void EnableRetryOpenButtons(bool enable)
        {
            this.retryButton.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            this.silentlyRetryButton.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            this.openDoorButton.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void silentlyRetryButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
