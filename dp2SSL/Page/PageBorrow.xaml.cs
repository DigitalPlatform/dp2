using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using DigitalPlatform;
using DigitalPlatform.Interfaces;
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
        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;

        Timer _timer = null;

        EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        public PageBorrow()
        {
            InitializeComponent();

            Loaded += PageBorrow_Loaded;
            Unloaded += PageBorrow_Unloaded;

            this.DataContext = this;

            // this.booksControl.PropertyChanged += Entities_PropertyChanged;

            this.booksControl.SetSource(_entities);

            this.patronControl.DataContext = _patron;

            this._patron.PropertyChanged += _patron_PropertyChanged;

            // Refresh();

        }

        FingerprintChannel _fingerprintChannel = null;
        RfidChannel _rfidChannel = null;
        // EventProxy eventProxy;

        private void PageBorrow_Loaded(object sender, RoutedEventArgs e)
        {
            // throw new Exception("test");

            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);

            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(App.CurrentApp.FingerprintUrl) == false)
            {
#if NO
                eventProxy = new EventProxy();
                eventProxy.MessageArrived +=
                  new MessageArrivedEvent(eventProxy_MessageArrived);
#endif
                _fingerprintChannel = FingerPrint.StartFingerprintChannel(
                    App.CurrentApp.FingerprintUrl,
                    out string strError);
                if (_fingerprintChannel == null)
                    errors.Add(strError);
                // https://stackoverflow.com/questions/7608826/how-to-remote-events-in-net-remoting
#if NO
                _fingerprintChannel.Object.MessageArrived +=
  new MessageArrivedEvent(eventProxy.LocallyHandleMessageArrived);
#endif
                try
                {
                    _fingerprintChannel.Object.GetMessage("clear");
                }
                catch (Exception ex)
                {
                    errors.Add("ClearMessage Exception: " + ex.Message);
                }


            }

            if (string.IsNullOrEmpty(App.CurrentApp.RfidUrl) == false)
            {
                _rfidChannel = RFID.StartRfidChannel(
    App.CurrentApp.RfidUrl,
    out string strError);
                if (_rfidChannel == null)
                    errors.Add(strError);
            }

            if (errors.Count > 0)
                this.Error = StringUtil.MakePathList(errors, "; ");

            // https://stackoverflow.com/questions/13396582/wpf-user-control-throws-design-time-exception
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                _timer = new System.Threading.Timer(
        new System.Threading.TimerCallback(timerCallback),
        null,
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(1));
        }

#if NO
        void eventProxy_MessageArrived(string Message)
        {
            MessageBox.Show(Message);
        }
#endif

        private void PageBorrow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_fingerprintChannel != null)
            {
                FingerPrint.EndFingerprintChannel(_fingerprintChannel);
                _fingerprintChannel = null;
            }

            if (_rfidChannel != null)
            {
                RFID.EndRfidChannel(_rfidChannel);
                _rfidChannel = null;
            }

            if (_timer != null)
                _timer.Dispose();
        }

        bool _visiblityChanged = false;

        private void _patron_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UID")
            {
                // 如果 patronControl 本来是隐藏状态，但读卡器上放上了读者卡，这时候要把 patronControl 恢复显示
                if (string.IsNullOrEmpty(_patron.UID) == false
                    && this.patronControl.Visibility != Visibility.Visible)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Visible;
                        _visiblityChanged = true;
                    }));
                // 如果读者卡又被拿走了，则要恢复 patronControl 的隐藏状态
                else if (string.IsNullOrEmpty(_patron.UID) == true
    && this.patronControl.Visibility == Visibility.Visible
    && _visiblityChanged)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Collapsed;
                    }));
            }
        }

        public PageBorrow(string buttons) : this()
        {
            this.ActionButtons = buttons;
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

                // (普通)还书和续借操作并不需要读者卡
                if (borrowButton.Visibility != Visibility.Visible)
                    this.patronControl.Visibility = Visibility.Collapsed;
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

        int _inRefresh = 0;

        void Refresh()
        {
            // 防止重入
            int v = Interlocked.Increment(ref this._inRefresh);
            if (v > 1)
            {
                Interlocked.Decrement(ref this._inRefresh);
                return;
            }

            _cancelRefresh = new CancellationTokenSource();
            try
            {
                // 获得所有协议类型的标签
                var result = _rfidChannel.Object.ListTags("*",
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
                        var gettaginfo_result = GetTagInfo(_rfidChannel, tag.UID);
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

                List<Entity> new_entities = new List<Entity>();
                {
                    // 比较当前集合。对当前集合进行增删改
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        _entities.Refresh(books, ref new_entities);
                    }));

                    if (_entities.Count == 0 && _patron.Source == "fingerprint")
                        _patron.Clear();
                }

                if (patrons.Count == 1)
                    _patron.Source = "";

                if (_patron.Source == "fingerprint")
                {

                }
                else
                {
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


                GetPatronFromFingerprint();

                FillBookFields();
                FillPatronDetail();

                CheckEAS(new_entities);
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
                _cancelRefresh = null;
                Interlocked.Decrement(ref this._inRefresh);
            }
        }

        // 检查芯片的 EAS 状态
        void CheckEAS(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (entity.TagInfo == null)
                    continue;

                // 检测 EAS 是否正确
                NormalResult result = null;
                if (entity.State == "borrowed" && entity.TagInfo.EAS == true)
                    result = SetEAS(entity.UID, false);
                else if (entity.State == "onshelf" && entity.TagInfo.EAS == false)
                    result = SetEAS(entity.UID, true);
                else
                    continue;

                if (result.Value == -1)
                    entity.SetError($"自动修正 EAS 时出错: {result.ErrorInfo}", "red");
                else
                    entity.SetError("自动修正 EAS 成功", "green");
            }
        }

        // 从指纹阅读器获取消息(第一阶段)
        void GetPatronFromFingerprint()
        {
            if (_fingerprintChannel == null)
                return;
            try
            {
                var message = _fingerprintChannel.Object.GetMessage("");
                if (message == null)
                    return;

                _patron.Clear();
                // _patron.UID = "#fingerprint";
                _patron.PII = message;
                _patron.Source = "fingerprint";
                // TODO: 此种情况下，要禁止后续从读卡器获取，直到新一轮开始。
                // “新一轮”意思是图书全部取走以后开始的下一轮
            }
            catch (Exception ex)
            {
                this._patron.Error = ex.Message;
            }
        }

        // 填充读者信息的其他字段(第二阶段)
        void FillPatronDetail()
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
#if NO
            RfidChannel channel = RFID.StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);
#endif
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
                        var result = GetTagInfo(_rfidChannel, entity.UID);
                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }

                        Debug.Assert(result.TagInfo != null);

                        entity.TagInfo = result.TagInfo;

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
#if NO
                RFID.EndRfidChannel(channel);
#endif
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

            LibraryChannel channel = App.CurrentApp.GetChannel();
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
                App.CurrentApp.ReturnChannel(channel);
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
            LibraryChannel channel = App.CurrentApp.GetChannel();
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
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        #region 属性

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
                Loan("borrow");
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

        void Loan(string action)
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

            // 检查读者卡状态是否 OK
            if (IsPatronOK() == false)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.MessageText = $"读卡器上的当前读者卡状态不正确。无法进行{action_name}操作";
                    progress.BackColor = "red";
                    progress = null;
                }));
                return;
            }

            // 借书操作必须要有读者卡。(还书和续借，可要可不要)
            if (action == "borrow")
            {
                if (string.IsNullOrEmpty(_patron.Barcode))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = $"请先在读卡器上放好读者卡，再进行{action_name}";
                        progress.BackColor = "red";
                        progress = null;
                    }));
                    return;
                }
            }

            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                ClearEntitiesError();

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.ProgressBar.Value = 0;
                    progress.ProgressBar.Minimum = 0;
                    progress.ProgressBar.Maximum = _entities.Count;
                }));

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

                        // return 操作，提前修改 EAS
                        // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                        {
                            var result = SetEAS(entity.UID, action == "return");
                            if (result.Value == -1)
                            {
                                entity.SetError($"{action_name}时修改 EAS 动作失败: {result.ErrorInfo}", "red");
                                errors.Add($"册 '{entity.PII}' {action_name}时修改 EAS 动作失败: {result.ErrorInfo}");
                                continue;
                            }
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

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value++;
                    }));

                    if (lRet == -1)
                    {
                        // return 操作如果 API 失败，则要改回原来的 EAS 状态
                        if (action == "return")
                        {
                            var result = SetEAS(entity.UID, false);
                            if (result.Value == -1)
                                strError += $"\r\n并且复原 EAS 状态的动作也失败了: {result.ErrorInfo}";
                        }

                        entity.SetError($"{action_name}操作失败: {strError}", "red");
                        // TODO: 这里最好用 title
                        errors.Add($"册 '{entity.PII}': {strError}");
                        continue;
                    }

                    // borrow 操作，API 之后才修改 EAS
                    // 注: 如果 API 成功但修改 EAS 动作失败(可能由于读者从读卡器上过早拿走图书导致)，读者会无法把本册图书拿出门禁。遇到此种情况，读者回来补充修改 EAS 一次即可
                    if (action == "borrow")
                    {
                        var result = SetEAS(entity.UID, action == "return");
                        if (result.Value == -1)
                        {
                            entity.SetError($"虽然{action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}", "yellow");
                            errors.Add($"册 '{entity.PII}' {action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}");
                        }
                    }

                    // 刷新显示
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath, item_records[0]);

                        if (entity.Error != null)
                            continue;

                        entity.SetError($"{action_name}成功", lRet == 1 ? "yellow" : "green");
                        success_count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.ProgressBar.Visibility = Visibility.Collapsed;
                    // progress.ProgressBar.Value = progress.ProgressBar.Maximum;
                }));

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
                    return; // new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, "; ") };
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

                return; // new NormalResult { Value = success_count };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
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

        NormalResult SetEAS(string uid, bool enable)
        {
#if NO
            RfidChannel channel = RFID.StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);
#endif
            try
            {
                return _rfidChannel.Object.SetEAS("*", $"uid:{uid}", enable);
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
            }
            finally
            {
#if NO
                RFID.EndRfidChannel(channel);
#endif
            }
        }

        // 当前读者卡状态是否 OK?
        // 注：如果卡片虽然放上去了，但无法找到读者记录，这种状态就不是 OK 的。此时应该拒绝进行流通操作
        bool IsPatronOK()
        {
            if (string.IsNullOrEmpty(_patron.UID)
                && string.IsNullOrEmpty(_patron.Barcode))
                return true;

            if (string.IsNullOrEmpty(_patron.UID) == false
    && string.IsNullOrEmpty(_patron.Barcode) == false)
                return true;
            return false;
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
                Loan("return");
            });
        }

        // 续借
        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Loan("renew");
            });
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageMenu());
        }
    }
}
