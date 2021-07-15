using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public NetworkWindow()
        {
            InitializeComponent();

            this.Loaded += NetworkWindow_Loaded;
            this.Unloaded += NetworkWindow_Unloaded;
        }

        private void NetworkWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancel?.Cancel();
        }

        private void NetworkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 延时后自动选择 Local 模式
            _ = Task.Run(async () =>
            {
                var token = _cancel.Token;
                string button_text = "";
                App.Invoke(new Action(() =>
                {
                    button_text = this.localMode.Content as string;
                }));
                try
                {
                    DateTime start = DateTime.Now;
                    var length = TimeSpan.FromMinutes(5);
                    int seconds = (int)length.TotalSeconds;
                    while (token.IsCancellationRequested == false
                        && DateTime.Now - start < length)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        App.Invoke(new Action(() =>
                        {
                            this.localMode.Content = $"{button_text} ({seconds})";
                        }));
                        seconds--;
                    }
                    if (token.IsCancellationRequested == false)
                    {
                        App.Invoke(new Action(() =>
                        {
                            SelectLocalMode();
                        }));
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch(Exception ex)
                {
                    if (token.IsCancellationRequested == false)
                        SelectLocalMode();
                }
            });
        }

        private void localMode_Click(object sender, RoutedEventArgs e)
        {
            /*
            this.DialogResult = true;
            this.Mode = "local";
            this.Close();
            */
            SelectLocalMode();
        }

        void SelectLocalMode()
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
