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
    /// 询问是否要典藏移交进智能书柜所在馆藏地，的对话框
    /// </summary>
    public partial class AskTransferWindow : Window
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public AskTransferWindow()
        {
            InitializeComponent();

            Loaded += AskTransferInWindow_Loaded;
            Unloaded += AskTransferWindow_Unloaded;
        }

        ~AskTransferWindow()
        {
            _cancel.Dispose();
        }

        private void AskTransferWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this._cancel.Cancel();
        }

        private void AskTransferInWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void SetBooks(EntityCollection collection)
        {
            books.SetSource(collection);

            // 后台处理刷新
            var task = ShelfData.FillBookFields(collection, _cancel.Token, false);
        }

        public string TitleText
        {
            get
            {
                return this.title.Text;
            }
            set
            {
                this.title.Text = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text.Text;
            }
            set
            {
                this.text.Text = value;
            }
        }

        public string BatchNo
        {
            get
            {
                return this.batchNo.Text;
            }
            set
            {
                this.batchNo.Text = value;
            }
        }

        public string Selection { get; set; }

        private string _mode = "in";

        // 模式。in/out
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (_mode == "in")
                {
                    targetFrame.Visibility = Visibility.Collapsed;
                }
                else
                {
                    targetFrame.Visibility = Visibility.Visible;
                }
            }
        }

        public string Target
        {
            get
            {
                return this.target.Text;
            }
            set
            {
                this.target.Text = value;
            }
        }

        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Mode == "out")
            {
                // 检查 target 是否有值
                if (string.IsNullOrEmpty(this.target.Text))
                {
                    MessageBox.Show("尚未选择移交目标");
                    return;
                }
            }

            this.Selection = "transfer";
            this.Close();
        }

        private void NotButton_Click(object sender, RoutedEventArgs e)
        {
            this.Selection = "not";
            this.Close();
        }
    }
}
