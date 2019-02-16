using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Windows.Documents;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// 借书功能页面
    /// PageBorrow.xaml 的交互逻辑
    /// </summary>
    public partial class PageBorrow : Page, INotifyPropertyChanged
    {
        Timer _timer = null;

        EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        public PageBorrow()
        {
            InitializeComponent();

            Loaded += PageBorrow_Loaded;

            this.DataContext = this;

            // this.booksControl.PropertyChanged += Entities_PropertyChanged;

            this.booksControl.SetSource(_entities);

            this.patronControl.DataContext = _patron;

            // Refresh();
            // https://stackoverflow.com/questions/13396582/wpf-user-control-throws-design-time-exception
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                _timer = new System.Threading.Timer(
        new System.Threading.TimerCallback(timerCallback),
        null,
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(1));
        }

        public PageBorrow(string buttons) : this()
        {
            this.ActionButtons = buttons;
        }

        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;
        private void PageBorrow_Loaded(object sender, RoutedEventArgs e)
        {
            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);
        }

        void AddLayer()
        {
            _layer.Add(_adorner);
        }

        void RemoveLayer()
        {
            _layer.Remove(_adorner);
        }

        CancellationTokenSource _cancelRefresh = null;

        void timerCallback(object o)
        {
            // 避免重叠启动
            if (_cancelRefresh != null)
                return;

            Refresh();
        }

        public string ActionButtons
        {
            get
            {
                List<string> buttons = new List<string>();
                if (borrowButton.Visibility == Visibility.Visible)
                    buttons.Add("borrow");
                if (returnButton.Visibility == Visibility.Visible)
                    buttons.Add("return");
                if (renewButton.Visibility == Visibility.Visible)
                    buttons.Add("renew");
                return StringUtil.MakePathList(buttons);
            }
            set
            {
                List<string> buttons = StringUtil.SplitList(value);
                if (buttons.IndexOf("borrow") != -1)
                    borrowButton.Visibility = Visibility.Visible;
                else
                    borrowButton.Visibility = Visibility.Collapsed;
                if (buttons.IndexOf("return") != -1)
                    returnButton.Visibility = Visibility.Visible;
                else
                    returnButton.Visibility = Visibility.Collapsed;
                if (buttons.IndexOf("renew") != -1)
                    renewButton.Visibility = Visibility.Visible;
                else
                    renewButton.Visibility = Visibility.Collapsed;
            }
        }

        static App App
        {
            get
            {
                return ((App)Application.Current);
            }
        }

        // uid --> TagInfo
        Hashtable _tagTable = new Hashtable();

        // 从缓存中获取标签信息
        GetTagInfoResult GetTagInfo(RfidChannel channel, string uid)
        {
            TagInfo info = (TagInfo)_tagTable[uid];
            if (info == null)
            {
                var result = channel.Object.GetTagInfo("*", uid);
                if (result.Value == -1)
                    return result;
                info = result.TagInfo;
                if (info != null)
                {
                    if (_tagTable.Count > 1000)
                        _tagTable.Clear();
                    _tagTable[uid] = info;
                }
            }

            return new GetTagInfoResult { TagInfo = info };
        }

        void Refresh()
        {
            // 第一阶段出错
            // List<string> first_stag_error_uids = new List<string>();

            _cancelRefresh = new CancellationTokenSource();
            try
            {
                RfidChannel channel = StartRfidChannel(App.RfidUrl,
        out string strError);
                if (channel == null)
                    throw new Exception(strError);
                try
                {
                    // 获得所有协议类型的标签
                    var result = channel.Object.ListTags("*",
                        null
                        // "getTagInfo"
                        );

                    List<OneTag> books = new List<OneTag>();
                    List<OneTag> patrons = new List<OneTag>();
                    // 分离图书标签和读者卡标签
                    foreach (OneTag tag in result.Results)
                    {
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                            patrons.Add(tag);
                        else if (tag.Protocol == InventoryInfo.ISO15693)
                        {
                            var gettaginfo_result = GetTagInfo(channel, tag.UID);
                            if (gettaginfo_result.Value == -1)
                            {
                                this.Error = gettaginfo_result.ErrorInfo;
                                continue;
                            }
                            TagInfo info = gettaginfo_result.TagInfo;
                            // 观察 typeOfUsage 元素
                            var chip = LogicChip.From(info.Bytes,
(int)info.BlockSize,
"");
                            string typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                            if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                                patrons.Add(tag);
                            else
                                books.Add(tag);
                        }
                        else
                            books.Add(tag);
                    }

                    {
                        // 比较当前集合。对当前集合进行增删改
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            _entities.Refresh(books);
                        }));
                    }

                    if (patrons.Count > 1)
                    {
                        // 读卡器上放了多张读者卡
                        _patron.Error = "读卡器上放了多张读者卡。请拿走多余的";
                    }
                    if (patrons.Count == 1)
                        _patron.Fill(patrons[0]);
                    else
                        _patron.Clear();
                }
                catch (Exception ex)
                {
#if NO
                    this.error.Text = ex.Message;
                    this.error.Visibility = Visibility.Visible;
#endif
                    this.Error = ex.Message;
                    return;
                }
                finally
                {
                    EndRfidChannel(channel);
                }

                FillBookFields();
                FillPatronInfo();
            }
            finally
            {
                _cancelRefresh = null;
            }
        }

        void FillPatronInfo()
        {
            if (_cancelRefresh == null
    || _cancelRefresh.IsCancellationRequested)
                return;

            //if (string.IsNullOrEmpty(_patron.Error) == false)
            //    return;

#if NO
            string name = Application.Current.Dispatcher.Invoke(new Func<string>(() =>
            {
                return this.Name;
            }));
#endif

            // 已经填充过了
            if (_patron.PatronName != null)
                return;

            string pii = _patron.PII;
            if (string.IsNullOrEmpty(pii))
                pii = _patron.UID;

            if (string.IsNullOrEmpty(pii))
                return;

            // return.Value:
            //      -1  出错
            //      0   读者记录没有找到
            //      1   成功
            NormalResult result = GetReaderInfo(pii,
    out string reader_xml);
            if (result.Value != 1)
            {
                _patron.Error = result.ErrorInfo;
                return;
            }

            _patron.Error = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _patron.SetPatronXml(reader_xml);
            }));
        }

        // 第二阶段：填充图书信息的 PII 和 Title 字段
        void FillBookFields()
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

                    if (entity.FillFinished == true)
                        continue;

                    //if (string.IsNullOrEmpty(entity.Error) == false)
                    //    continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字重要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        // var result = channel.Object.GetTagInfo("*", entity.UID);
                        var result = GetTagInfo(channel, entity.UID);
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
                        var result = GetEntityData(entity.PII,
                            out string title,
                            out string item_xml,
                            out string item_recpath);
                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }
                        entity.Title = GetCaption(title);
                        // entity.ItemXml = item_xml;
                        entity.SetData(item_recpath, item_xml);
                    }

                    entity.SetError(null);
                    entity.FillFinished = true;
                }

                booksControl.SetBorrowable();
            }
            catch (Exception ex)
            {
#if NO
                this.error.Text = ex.Message;
                this.error.Visibility = Visibility.Visible;
#endif
                this.Error = ex.Message;
            }
            finally
            {
                EndRfidChannel(channel);
            }
        }

        static string GetCaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";

            return text;
        }

        // 获得一个册的题名字符串
        NormalResult GetEntityData(string pii,
            out string title,
            out string item_xml,
            out string item_recpath)
        {
            title = "";
            item_xml = "";
            item_recpath = "";

            LibraryChannel channel = App.GetChannel();
            try
            {
#if NO
                GetItemInfo(
    stop,
    "item",
    strBarcode,
    "",
    strResultType,
    out strResult,
    out strItemRecPath,
    out item_timestamp,
    strBiblioType,
    out strBiblio,
    out strBiblioRecPath,
    out strError);
#endif
                long lRet = channel.GetItemInfo(null,
                    "item",
                    pii,
                    "",
                    "xml",
                    out item_xml,
                    out item_recpath,
                    out byte[] item_timestamp,
                    "",
                    out string biblio_xml,
                    out string biblio_recpath,
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

        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        NormalResult GetReaderInfo(string pii,
            out string reader_xml)
        {
            reader_xml = "";
            LibraryChannel channel = App.GetChannel();
            try
            {
                long lRet = channel.GetReaderInfo(null,
                    pii,
                    "xml",
                    out string[] results,
                    out string strError);
                if (lRet == -1)
                    return new NormalResult { Value = -1, ErrorInfo = strError };
                if (lRet == 0)
                    return new NormalResult { Value = 0, ErrorInfo = strError };

                if (results != null && results.Length > 0)
                    reader_xml = results[0];

                return new NormalResult { Value = 1 };
            }
            finally
            {
                App.ReturnChannel(channel);
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


        #region Borrowable 属性

#if NO
        private void Entities_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Borrowable")
                OnPropertyChanged(e.PropertyName);
        }
#endif

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

#if NO
        public string Borrowable
        {
            get
            {
                return booksControl.Borrowable;
            }
            set
            {
                booksControl.Borrowable = value;
            }
        }
#endif

        private string _error = null;   // "test error line asdljasdkf; ;jasldfjasdjkf aasdfasdf";

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        #endregion

        // 借书
        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var result = Loan("borrow");
            });
        }

        void ClearEntitiesError()
        {
            foreach (Entity entity in _entities)
            {
                entity.Error = null;
            }
        }

        void ClearPatronError()
        {
            _patron.Error = null;
        }

        NormalResult Loan(string action)
        {
            ProgressWindow progress = null;

            string action_name = "借书";
            if (action == "return")
                action_name = "还书";
            else if (action == "renew")
                action_name = "续借";

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.MessageText = "正在处理，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += Progress_Closed;
                progress.Show();
                AddLayer();
            }));

            LibraryChannel channel = App.GetChannel();
            try
            {
                // ClearEntitiesError();
                int skip_count = 0;
                int success_count = 0;
                List<string> errors = new List<string>();
                foreach (Entity entity in _entities)
                {
                    long lRet = 0;
                    string strError = "";
                    string[] item_records = null;

                    if (action == "borrow" || action == "renew")
                    {
                        if (action == "borrow" && entity.State == "borrowed")
                        {
                            entity.SetError($"本册是外借状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }
                        if (action == "renew" && entity.State == "onshelf")
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略 (只有处于外借状态的册才能进行续借)", "yellow");
                            skip_count++;
                            continue;
                        }
                        entity.Waiting = true;
                        lRet = channel.Borrow(null,
                            action == "renew",
                            _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            null,
                            "item,reader", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "",
                            out string[] biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out BorrowInfo borrow_info,
                            out strError);
                    }
                    else if (action == "return")
                    {
                        if (entity.State == "onshelf")
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }

                        entity.Waiting = true;
                        lRet = channel.Return(null,
                            "return",
                            _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            "item,reader", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "",
                            out string[] biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out ReturnInfo return_info,
                            out strError);
                    }

                    if (lRet == -1)
                    {
                        entity.SetError($"{action_name}操作失败: {strError}", "red");
                        // TODO: 这里最好用 title
                        errors.Add($"册 '{entity.PII}': {strError}");
                    }
                    else
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath, item_records[0]);
                        entity.SetError($"{action_name}成功", "green");
                        success_count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                // 修改 borrowable
                booksControl.SetBorrowable();

                if (errors.Count > 0)
                {
                    string error = StringUtil.MakePathList(errors, "\r\n");
                    string message = $"{action_name}操作出错 {errors.Count} 笔";
                    if (success_count > 0)
                        message += $"，成功 {success_count} 笔";
                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 笔被忽略)";
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = message;

                        // progress.MessageText = $"{action_name}操作出错: \r\n{error}";
                        progress.BackColor = "red";
                        progress = null;
                    }));
                    return new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, "; ") };
                }
                else
                {
                    // 成功
                    string backColor = "green";
                    string message = $"{action_name}操作成功 {success_count} 笔";
                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 笔被忽略)";
                    if (skip_count > 0 && success_count == 0)
                    {
                        backColor = "yellow";
                        message = $"全部 {skip_count} 笔{action_name}操作被忽略";
                    }
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = message;
                        progress.BackColor = backColor;
                        progress = null;
                    }));
                }

                return new NormalResult { Value = success_count };
            }
            finally
            {
                App.ReturnChannel(channel);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
            }
        }

        private void Progress_Closed(object sender, EventArgs e)
        {
            RemoveLayer();
        }

#if NO
        NormalResult Return()
        {
            LibraryChannel channel = App.GetChannel();
            try
            {
                int count = 0;
                List<string> errors = new List<string>();
                foreach (Entity entity in _entities)
                {
                    if (entity.State == "onshelf")
                        continue;

                    entity.Waiting = true;
                    long lRet = channel.Return(null,
                        "return",
                        _patron.Barcode,
                        entity.PII,
                        entity.ItemRecPath,
                        false,
                        "item,reader", // style,
                        "xml", // item_format_list
                        out string[] item_records,
                        "xml",
                        out string[] reader_records,
                        "",
                        out string[] biblio_records,
                        out string[] dup_path,
                        out string output_reader_barcode,
                        out ReturnInfo return_info,
                        out string strError);
                    if (lRet == -1)
                    {
                        entity.SetError(strError);
                        errors.Add(strError);
                    }
                    else
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath, item_records[0]);
                        entity.Waiting = false;
                        count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                // 修改 borrowable 和 returnable
                booksControl.SetBorrowable();

                if (errors.Count > 0)
                    return new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, "; ") };

                return new NormalResult { Value = count };
            }
            finally
            {
                App.ReturnChannel(channel);
            }
        }
#endif
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var result = Loan("return");
            });
        }

        // 续借
        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var result = Loan("renew");
            });
        }
    }
}
