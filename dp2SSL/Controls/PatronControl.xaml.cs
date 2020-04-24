using System;
using System.Collections;
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

        EntityCollection _borrowedEntities = new EntityCollection { Style = "template:SmallTemplate" };

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

        // SetBorrowed("") 可以清除列表
        public void SetBorrowed(string patron_xml)
        {
            _borrowedEntities.Clear();

            if (string.IsNullOrEmpty(patron_xml) == false)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(patron_xml);

                // 用 hashtable 保存一下每个 entity 的原始序
                // Hashtable originIndexTable = new Hashtable();
                int i = 0;  // 插入位置

                // List<Entity> entities = new List<Entity>();
                XmlNodeList borrows = dom.DocumentElement.SelectNodes("borrows/borrow");
                foreach (XmlElement borrow in borrows)
                {
                    string barcode = borrow.GetAttribute("barcode");
                    var new_entity = new Entity { PII = barcode, Container = _borrowedEntities };
                    // 2020/4/17 不用排序，在添加时临时决定是插入到前部还是追加
                    if (IsState(new_entity, "overflow") == true)
                        _borrowedEntities.Insert(i++, new_entity);
                    else
                        _borrowedEntities.Add(new_entity);
                }
            }
        }

        static bool IsState(Entity entity, string sub)
        {
            return StringUtil.IsInList(sub, entity.State);
        }

        /*
        // 对 Entity 进行排序
        // TODO: 余下的建议按照应还日期，日期靠前排序。这样便于读者观察到需要尽快还书的册
        static int CompareEntities(Entity a,
            Entity b,
            Hashtable originIndexTable)
        {
            int index_a = (int)originIndexTable[a];
            int index_b = (int)originIndexTable[b];
            bool a_overflow = IsState(a, "overflow");
            bool b_overflow = IsState(b, "overflow");
            if (a_overflow && b_overflow)
                return index_a - index_b;

            if (a_overflow)
                return -1;
            if (b_overflow)
                return 1;

            // 按原来的序
            return index_a - index_b;
        }
        */

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

        public void LoadPhoto(string photo_path)
        {
            if (string.IsNullOrEmpty(photo_path))
            {
                App.Invoke(new Action(() =>
                {
                    this.SetPhoto(null);
                }));
                return;
            }

            // TODO: 照片可以缓存到本地。每次只需要获取 timestamp 即可。如果 timestamp 和缓存的不一致再重新获取一次

            Stream stream = new MemoryStream();
            var channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(30);
            try
            {
                long lRet = channel.GetRes(
                    null,
                    photo_path,
                    stream,
                    "data,content", // strGetStyle,
                    null,   // byte [] input_timestamp,
                    out string strMetaData,
                    out byte[] baOutputTimeStamp,
                    out string strOutputPath,
                    out string strError);
                if (lRet == -1)
                {
                    // SetGlobalError("patron", $"获取读者照片时出错: {strError}");
                    // return;
                    throw new Exception($"获取读者照片(path='{photo_path}')时出错: {strError}");
                }

                stream.Seek(0, SeekOrigin.Begin);
                App.Invoke(new Action(() =>
                {
                    this.SetPhoto(stream);
                }));
                stream = null;
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
                if (stream != null)
                    stream.Dispose();
            }
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
