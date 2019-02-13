using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Xml;

namespace dp2SSL
{
    /// <summary>
    /// 显示册信息的控件
    /// EntityListControl.xaml 的交互逻辑
    /// </summary>
    public partial class EntityListControl : UserControl, INotifyPropertyChanged
    {
        // public string RfidUrl { get; set; }

        Timer _timer = null;

        EntityCollection _entities = new EntityCollection();

        public EntityListControl()
        {
            InitializeComponent();

            // RfidUrl = WpfClientInfo.Config.Get("global", "rfidUrl", "");
            this.listView.ItemsSource = _entities;

            // Refresh();
            // https://stackoverflow.com/questions/13396582/wpf-user-control-throws-design-time-exception
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                _timer = new System.Threading.Timer(
        new System.Threading.TimerCallback(timerCallback),
        null,
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(1));

        }

        CancellationTokenSource _cancelRefresh = null;

        void timerCallback(object o)
        {
            // 避免重叠启动
            if (_cancelRefresh != null)
                return;

            Refresh();
        }


        string _borrowable = null;

        public string Borrowable
        {
            get
            {
                return _borrowable;
            }
            set
            {
                _borrowable = value;
                OnPropertyChanged("Borrowable");
            }
        }

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Refresh()
        {
            _cancelRefresh = new CancellationTokenSource();
            try
            {
                RfidChannel channel = StartRfidChannel(App.RfidUrl,
        out string strError);
                if (channel == null)
                    throw new Exception(strError);
                try
                {
                    var result = channel.Object.ListTags("*",
                        null
                        // "getTagInfo"
                        );
                    // 比较当前集合。对当前集合进行增删改
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        _entities.Refresh(result.Results);
                    }));
                }
                catch (Exception ex)
                {
                    this.error.Text = ex.Message;
                    this.error.Visibility = Visibility.Visible;
                }
                finally
                {
                    EndRfidChannel(channel);
                }

                FillFields();
            }
            finally
            {
                _cancelRefresh = null;
            }
        }

        static App App
        {
            get
            {
                return ((App)Application.Current);
            }
        }

        #region RFID 有关功能

        public class RfidChannel
        {
            public IpcClientChannel Channel { get; set; }
            public IRfid Object { get; set; }
        }

        public static RfidChannel StartRfidChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            RfidChannel result = new RfidChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IRfid)Activator.GetObject(typeof(IRfid),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            finally
            {
                if (bDone == false)
                    EndRfidChannel(result);
            }
        }

        public static void EndRfidChannel(RfidChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        public static NormalResult SetEAS(
            RfidChannel channel,
            string reader_name,
            string tag_name,
            bool enable,
            out string strError)
        {
            strError = "";

            try
            {
                return channel.Object.SetEAS(reader_name,
                    tag_name,
                    enable);
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 " + App.RfidUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -2, ErrorInfo = strError };
            }
            catch (Exception ex)
            {
                strError = "针对 " + App.RfidUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -1, ErrorInfo = strError };
            }
        }

        #endregion

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Refresh();
            });
        }

        // 填充 PII 和 Title 字段
        void FillFields()
        {
            RfidChannel channel = StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);

            try
            {
                foreach (Entity entity in _entities)
                {
                    if (_cancelRefresh == null
                        || _cancelRefresh.IsCancellationRequested)
                        return;

                    if (string.IsNullOrEmpty(entity.Error) == false)
                        continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字重要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        var result = channel.Object.GetTagInfo("*", entity.UID);
                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }

                        Debug.Assert(result.TagInfo != null);

                        LogicChip chip = LogicChip.From(result.TagInfo.Bytes,
(int)result.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
                        string pii = chip.FindElement(ElementOID.PII)?.Text;
                        entity.PII = GetCaption(pii);
                    }

                    // 获得 Title
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        var result = GetTitle(entity.PII, out string title, out string item_xml);
                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }
                        entity.Title = GetCaption(title);
                        entity.ItemXml = item_xml;
                        entity.SetState();
                    }

                    entity.Waiting = false;
                }

                SetBorrowable();
            }
            catch (Exception ex)
            {
                this.error.Text = ex.Message;
                this.error.Visibility = Visibility.Visible;
            }
            finally
            {
                EndRfidChannel(channel);
            }
        }

        void SetBorrowable()
        {
            int count = 0;
            foreach(Entity entity in _entities)
            {
                if (entity.State == "onshelf")
                    count++;
            }

            this.Borrowable = count.ToString();
        }

        static string GetCaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";

            return text;
        }

        // 获得一个册的题名字符串
        NormalResult GetTitle(string pii,
            out string title, 
            out string item_xml)
        {
            title = "";
            item_xml = "";
            LibraryChannel channel = App.GetChannel();
            try
            {
                long lRet = channel.GetItemInfo(null,
                    pii,
                    "xml",
                    out item_xml,
                    "",
                    out string biblio_xml,
                    out string strError);
                if (lRet == -1)
                    return new NormalResult { Value = -1, ErrorInfo = strError };

                lRet = channel.GetBiblioSummary(
    null,
    pii,
    "", // strConfirmItemRecPath,
    null,
    out string strBiblioRecPath,
    out string strSummary,
    out strError);
                if (lRet == -1)
                    return new NormalResult { Value = -1, ErrorInfo = strError };

                title = strSummary;

                return new NormalResult();
            }
            finally
            {
                App.ReturnChannel(channel);
            }
        }
    }

    public class EntityCollection : ObservableCollection<Entity>
    {

        // TODO: 如何避免重入？
        public bool Refresh(List<OneTag> tags)
        {
            bool changed = false;
            List<Entity> need_delete = new List<Entity>();
            foreach (Entity entity in this)
            {
                OneTag tag = tags.Find((o) =>
                {
                    if (o.UID == entity.UID)
                        return true;
                    return false;
                });
                if (tag == null)
                {
                    // 多出来的，需要删除
                    need_delete.Add(entity);
                }
                else
                {
                    // 已经存在的
                    tags.Remove(tag);
                }
            }

            // 删除
            foreach (Entity entity in need_delete)
            {
                this.Remove(entity);
                changed = true;
            }

            // 新添加
            foreach (OneTag tag in tags)
            {
                string pii = "";
                if (tag.TagInfo != null)
                {
                    LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
        (int)tag.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                    pii = chip.FindElement(ElementOID.PII)?.Text;
                }
                else
                {
                    // 尝试重新获取一次
                }

                Entity entity = new Entity { UID = tag.UID, PII = pii };
                this.Add(entity);
                changed = true;
            }

            return changed;
        }
    }

    public class Entity : INotifyPropertyChanged
    {

        private string _pii;
        private string _uid;
        private string _title;
        private string _error = null;   // "test error line asdljasdkf; ;jasldfjasdjkf aasdfasdf";

        public string PII
        {
            get => _pii;
            set
            {
                _pii = value;
                OnPropertyChanged("PII");
            }
        }
        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged("UID");
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        bool _waiting = true;

        public bool Waiting
        {
            get
            {
                return _waiting;
            }
            set
            {
                _waiting = value;
                OnPropertyChanged("Waiting");
            }
        }

        string _state = null;

        // 事项状态。borrowed/onshelf/空
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }


        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal string ItemXml { get; set; }

        // 设置错误信息并结束等待状态
        public void SetError(string error)
        {
            this.Error = error;
            this.Waiting = false;
        }

        public void SetState()
        {
            // FontAwesome.WPF.FontAwesomeIcon.HandGrabOutline
            if (string.IsNullOrEmpty(ItemXml))
            {
                this.State = null;
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(ItemXml);

            string borrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(borrower))
                this.State = "onshelf";
            else
                this.State = "borrowed";
        }
    }
}
