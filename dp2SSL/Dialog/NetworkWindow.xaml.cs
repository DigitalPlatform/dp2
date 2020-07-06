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
    /// NetworkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NetworkWindow : Window
    {
        public string Mode { get; set; }    // 空/local

        public NetworkWindow()
        {
            InitializeComponent();
        }

        private void localMode_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Mode = "local";
            this.Close();
        }

        private void networkMode_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Mode = "";
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

        public string LocalModeButtonText
        {
            get
            {
                return this.localMode.Content.ToString();
            }
            set
            {
                this.localMode.Content = value;
            }
        }

        public string NetworkModeButtonText
        {
            get
            {
                return this.networkMode.Content.ToString();
            }
            set
            {
                this.networkMode.Content = value;
            }
        }
    }
}
