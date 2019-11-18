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
    /// SubmitWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SubmitWindow : Window
    {
        // public event EventHandler Next;

        List<DisplayContent> _contents = new List<DisplayContent>();

        int _showCount = 0;

        public SubmitWindow()
        {
            InitializeComponent();
        }

        class DisplayContent
        {
            public string Color { get; set; }
            public string Text { get; set; }
            public MessageDocument Document { get; set; }
        }

        public void PushContent(string text, string color)
        {
            _contents.Add(new DisplayContent
            {
                Text = text,
                Color = color
            });
        }

        public void PushContent(MessageDocument doc)
        {
            _contents.Add(new DisplayContent
            {
                Document = doc
            });
        }

        public void ShowContent()
        {
            if (_contents.Count == 0)
                return;
            if (_showCount > 0)
                return;
            var first = _contents[0];
            if (first.Document != null)
            {
                string speak = "";
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.MessageDocument = first.Document.BuildDocument(18, out speak);
                }));
                if (string.IsNullOrEmpty(speak) == false)
                    App.CurrentApp.Speak(speak);
            }
            else
            {
                this.MessageText = first.Text;
                this.BackColor = first.Color;
            }
            _contents.RemoveAt(0);
            _showCount++;
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

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Next?.Invoke(sender, new EventArgs());

            if (_contents.Count == 0)
            {
                // TODO: 如果窗口正在处理中，要避免被关闭
                this.Close();
                return;
            }

            _showCount = 0;
            ShowContent();
        }

        private void _progressWindow_Next(object sender, EventArgs e)
        {

        }
    }
}
