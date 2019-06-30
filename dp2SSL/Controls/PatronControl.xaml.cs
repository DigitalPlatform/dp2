using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// 显示一个读者信息的控件
    /// EntityControl.xaml 的交互逻辑
    /// </summary>
    public partial class PatronControl : UserControl
    {
        public event EventHandler InputFace = null;

        EntityCollection _borrowedEntities = new EntityCollection { Style = "template:SmallTemplate"};

        public PatronControl()
        {
            InitializeComponent();

            this.borrowedBooks.SetSource(_borrowedEntities);
            this.borrowedBooks.emptyComment.Visibility = Visibility.Collapsed;
        }

        // 设置开始阶段的提示文字
        public void SetStartMessage(string style)
        {
            if (string.IsNullOrEmpty(style))
                return;
            bool fingerprint = StringUtil.IsInList("fingerprint", style);
            bool rfid = StringUtil.IsInList("rfid", style);
            bool face = StringUtil.IsInList("face", style);
            if (fingerprint && rfid)
                this.startMessage.Text = "请放读者卡，或扫指纹 ...";
            else if (fingerprint)
                this.startMessage.Text = "请扫指纹 ...";
            else if (rfid)
                this.startMessage.Text = "请放读者卡 ...";

            if (face)
                this.inputFace.Visibility = Visibility.Visible;
            else
                this.inputFace.Visibility = Visibility.Collapsed;
        }

#if NO
        public void HideInputFaceButton()
        {
            this.inputFace.Visibility = Visibility.Collapsed;
        }
#endif

        private void InputFace_Click(object sender, RoutedEventArgs e)
        {
            this.InputFace?.Invoke(sender, e);
        }

        public EntityCollection BorrowedEntities
        {
            get
            {
                return _borrowedEntities;
            }
        }

        public void SetBorrowed(string patron_xml)
        {
            _borrowedEntities.Clear();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(patron_xml);

            XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
            foreach(XmlElement borrow in borrows)
            {
                string barcode = borrow.GetAttribute("barcode");
                _borrowedEntities.Add(new Entity { PII = barcode, Container = _borrowedEntities });
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

        /*
        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        */
    }
}
