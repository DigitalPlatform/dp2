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
using System.Windows.Shapes;

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
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string MessageText
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
            imageSource.EndInit();
            this.photo.Source = imageSource;
        }
    }
}
