using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Remoting;
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
        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;

        Timer _timer = null;

        EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        Task _checkTask = null;
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public PageBorrow()
        {
            InitializeComponent();

            _globalErrorTable = new ErrorTable((e) =>
            {
                this.Error = e;
            });
            _patronErrorTable = new ErrorTable((e) =>
            {
                _patron.Error = e;
            });

            Loaded += PageBorrow_Loaded;
            Unloaded += PageBorrow_Unloaded;

            this.DataContext = this;

            // this.booksControl.PropertyChanged += Entities_PropertyChanged;

            this.booksControl.SetSource(_entities);
            this.patronControl.DataContext = _patron;

            this._patron.PropertyChanged += _patron_PropertyChanged;
        }

        public FingerprintChannel _fingerprintChannel
        {
            get
            {
                return App.CurrentApp.FingerprintChannel;
            }
        }

        RfidChannel _rfidChannel = null;
        // EventProxy eventProxy;

        private void PageBorrow_Loaded(object sender, RoutedEventArgs e)
        {
            // throw new Exception("test");

            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);

            {
                // 准备指纹通道
                List<string> errors = new List<string>();

                // 重试初始化指纹环境
                errors.AddRange(App.CurrentApp.TryInitialFingerprint());

                App.CurrentApp.ClearFingerprintMessage();

                SetGlobalError("fingerprint", StringUtil.MakePathList(errors, "; "));
            }
#if NO
            if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
            {
#if NO
                eventProxy = new EventProxy();
                eventProxy.MessageArrived +=
                  new MessageArrivedEvent(eventProxy_MessageArrived);
#endif
                _fingerprintChannel = FingerPrint.StartFingerprintChannel(
                    App.FingerprintUrl,
                    out string strError);
                if (_fingerprintChannel == null)
                    errors.Add($"启动指纹通道时出错: {strError}");
                // https://stackoverflow.com/questions/7608826/how-to-remote-events-in-net-remoting
#if NO
                _fingerprintChannel.Object.MessageArrived +=
  new MessageArrivedEvent(eventProxy.LocallyHandleMessageArrived);
#endif
                try
                {
                    _fingerprintChannel.Object.GetMessage("clear");
                    _fingerprintChannel.Started = true;
                }
                catch (Exception ex)
                {
                    if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                        errors.Add($"启动指纹通道时出错: “指纹中心”({App.FingerprintUrl})没有响应");
                    else
                        errors.Add($"启动指纹通道时出错(2): {ex.Message}");
                }
            }

#endif

            PrepareRfid();

#if NO
            {
                List<string> style = new List<string>();
                if (_rfidChannel?.Started == true)
                    style.Add("rfid");
                if (_fingerprintChannel?.Started == true)
                    style.Add("fingerprint");
                this.patronControl.SetStartMessage(StringUtil.MakePathList(style));
            }
#endif
            {
                List<string> style = new List<string>();
                if (string.IsNullOrEmpty(App.RfidUrl) == false)
                    style.Add("rfid");
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    style.Add("fingerprint");
                this.patronControl.SetStartMessage(StringUtil.MakePathList(style));
            }

            // https://stackoverflow.com/questions/13396582/wpf-user-control-throws-design-time-exception
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                _timer = new System.Threading.Timer(
        new System.Threading.TimerCallback(timerCallback),
        null,
        TimeSpan.FromSeconds(0),
        TimeSpan.FromSeconds(1));

            _checkTask = Task.Run(() =>
            {
                while (_cancel.IsCancellationRequested == false)
                {
                    Task.Delay(TimeSpan.FromSeconds(10), _cancel.Token).Wait(_cancel.Token);

                    if (string.IsNullOrEmpty(App.RfidUrl) == false
                    && _rfidChannel?.Started == false)
                        PrepareRfid();

                    // 验证通道是否有效
                    VerifyRfid();

                    // 注: App 里面会不断尝试连接指纹中心。这里只需要刷新错误显示即可
                    if (string.IsNullOrEmpty(App.FingerprintUrl) == false
                    && _fingerprintChannel?.Started == true)
                        SetGlobalError("fingerprint", "");

                    VerifyFingerprint();
                }
            });
        }

        // 验证通道是否有效
        void VerifyFingerprint()
        {
            if (_fingerprintChannel != null)
            {
                try
                {
                    // 调用查看状态的功能
                    var result = _fingerprintChannel.Object.GetState("");
                    if (result.Value == -1)
                        SetGlobalError("fingerprint", $"指纹中心: {result.ErrorInfo}");
                    else
                        SetGlobalError("fingerprint", "");
                }
                catch (Exception ex)
                {
                    _fingerprintChannel.Started = false;
                    SetGlobalError("fingerprint", $"指纹中心连接出错: {ex.Message}");
                }
            }
        }

        // 验证通道是否有效
        void VerifyRfid()
        {
            if (_rfidChannel != null)
            {
                try
                {
                    // 调用查看状态的功能
                    var result = _rfidChannel.Object.GetState("");
                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == "noReaders")
                            SetGlobalError("rfid", $"RFID 中心: 没有任何连接的读卡器。请检查读卡器是否正确连接");
                        else
                            SetGlobalError("rfid", $"RFID 中心: {result.ErrorInfo}");
                    }
                    else
                        SetGlobalError("rfid", "");
                }
                catch (Exception ex)
                {
                    _rfidChannel.Started = false;
                    SetGlobalError("rfid", $"RFID 中心连接出错: {ex.Message}");
                }
            }
        }

        // 准备通道
        void PrepareRfid()
        {
            List<string> errors = new List<string>();

            // 准备 RFID 通道
            if (string.IsNullOrEmpty(App.RfidUrl) == false)
            {
                _rfidChannel = RFID.StartRfidChannel(
    App.RfidUrl,
    out string strError);
                if (_rfidChannel == null)
                    errors.Add($"启动 RFID 通道时出错: {strError}");
                else
                {
                    try
                    {
                        // 检查状态
                        var result = _rfidChannel.Object.GetState("");

                        // TODO: 某处界面可以显示当前连接的读卡器名字
                        if (result.Value == -1)
                            errors.Add(result.ErrorInfo);   // "当前 RFID 中心没有任何连接的读卡器。请检查读卡器是否正确连接"
                        else
                            _rfidChannel.Started = true;

                        // 关掉 SendKey
                        _rfidChannel.Object.EnableSendKey(false);
                    }
                    catch (Exception ex)
                    {
                        if (ex is RemotingException && (uint)ex.HResult == 0x8013150b)
                            errors.Add($"启动 RFID 通道时出错: “RFID-中心”({App.RfidUrl})没有响应");
                        else
                            errors.Add($"启动 RFID 通道时出错: {ex.Message}");
                    }
                }
            }
            else
            {
                errors.Add($"尚未配置 RFID 接口 URL");
            }

            if (errors.Count > 0)
                SetGlobalError("rfid", StringUtil.MakePathList(errors, "; "));
            else
                SetGlobalError("rfid", "");
        }

#if NO
        void eventProxy_MessageArrived(string Message)
        {
            MessageBox.Show(Message);
        }
#endif

        private void PageBorrow_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancel.Cancel();
#if NO
            if (_fingerprintChannel != null)
            {
                FingerPrint.EndFingerprintChannel(_fingerprintChannel);
                _fingerprintChannel = null;
            }
#endif

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
            if (e.PropertyName == "UID"
                || e.PropertyName == "Barcode")
            {
                // 如果 patronControl 本来是隐藏状态，但读卡器上放上了读者卡，这时候要把 patronControl 恢复显示
                if ((string.IsNullOrEmpty(_patron.UID) == false || string.IsNullOrEmpty(_patron.Barcode) == false)
                    && this.patronControl.Visibility != Visibility.Visible)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Visible;
                        _visiblityChanged = true;
                    }));
                // 如果读者卡又被拿走了，则要恢复 patronControl 的隐藏状态
                else if (string.IsNullOrEmpty(_patron.UID) == true && string.IsNullOrEmpty(_patron.Barcode) == true
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

        public void ClearTagTable(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                _tagTable.Clear();
            else
                _tagTable.Remove(uid);
        }

        // 从缓存中获取标签信息
        GetTagInfoResult GetTagInfo(RfidChannel channel, string uid)
        {
            // 2019/5/21
            if (channel.Started == false)
                return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };

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

        // 清除图书列表
        void ClearBookList()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                List<Entity> new_entities = null;
                _entities.Refresh(new List<OneTag>(), ref new_entities);
            }));
        }

        int _inRefresh = 0;

        void Refresh()
        {
            //if (_rfidChannel == null || _rfidChannel.Started == false)
            //    return;

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
                SetGlobalError("current", "");

                // 获得所有协议类型的标签
                ListTagsResult result = null;

                if (_rfidChannel != null && _rfidChannel.Started)
                {
                    result = _rfidChannel?.Object?.ListTags("*",
                        null
                        // "getTagInfo"
                        );
                    // 2019/5/19
                    if (result.Value == -1)
                    {
                        SetGlobalError("current", $"RFID 中心错误:{result.ErrorInfo}, 错误码:{result.ErrorCode}");
                        {
                            ClearBookList();
                            FillBookFields();
                        }
                        // 连带清掉读者信息
                        _patron.Clear();
                        return;
                    }
                }

                List<OneTag> books = new List<OneTag>();
                List<OneTag> patrons = new List<OneTag>();

                // 分离图书标签和读者卡标签
                if (result?.Results != null)
                {
                    foreach (OneTag tag in result?.Results)
                    {
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                            patrons.Add(tag);
                        else if (tag.Protocol == InventoryInfo.ISO15693)
                        {
                            var gettaginfo_result = GetTagInfo(_rfidChannel, tag.UID);
                            if (gettaginfo_result.Value == -1)
                            {
                                SetGlobalError("current", gettaginfo_result.ErrorInfo);
                                continue;
                            }
                            TagInfo info = gettaginfo_result.TagInfo;

                            // 记下来。避免以后重复再次去获取了
                            if (tag.TagInfo == null)
                                tag.TagInfo = info;

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
                }

                List<Entity> new_entities = new List<Entity>();
                {
                    // 比较当前集合。对当前集合进行增删改
                    bool changed = false;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        changed = _entities.Refresh(books, ref new_entities);
                    }));

                    if (_entities.Count == 0
                        && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
                        && _patron.IsFingerprintSource)
                        _patron.Clear();
                }

                // 当列表为空的时候，主动清空一次 tag 缓存。这样读者可以用拿走全部标签一次的方法来迫使清除缓存(比如中途利用内务修改过 RFID 标签的 EAS)
                if (_entities.Count == 0 && _tagTable.Count > 0)
                    ClearTagTable(null);

                if (patrons.Count == 1)
                    _patron.IsRfidSource = true;

                if (_patron.IsFingerprintSource)
                {
                    // 指纹仪来源
                }
                else
                {
                    // RFID 来源


                    if (patrons.Count == 1)
                    {
                        _patron.Fill(patrons[0]);
                        SetPatronError("rfid_multi", "");   // 2019/5/22
                    }
                    else
                    {
                        _patron.Clear();
                        SetPatronError("getreaderinfo", "");
                        if (patrons.Count > 1)
                        {
                            // 读卡器上放了多张读者卡
                            SetPatronError("rfid_multi", "读卡器上放了多张读者卡。请拿走多余的");
                        }
                        else
                            SetPatronError("rfid_multi", "");   // 2019/5/20
                    }
                }

                GetPatronFromFingerprint();

                FillBookFields();
                FillPatronDetail();

                CheckEAS(new_entities);
            }
            catch (Exception ex)
            {
                SetGlobalError("current", $"后台刷新过程出现异常: {ex.Message}");
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
            if (_fingerprintChannel == null || _fingerprintChannel.Started == false)
            {
                // 清除以前残留的报错信息
                SetPatronError("fingerprint", "");
                return;
            }
            try
            {
                var result = _fingerprintChannel.Object.GetMessage("");
                if (result.Value == -1)
                {
                    SetPatronError("fingerprint", $"指纹中心出错: {result.ErrorInfo}, 错误码: {result.ErrorCode}");
                    if (_patron.IsFingerprintSource)
                        _patron.Clear();    // 只有当面板上的读者信息来源是指纹仪时，才清除面板上的读者信息
                    return;
                }
                else
                {
                    // 清除以前残留的报错信息
                    SetPatronError("fingerprint", "");
                }

                if (result.Message == null)
                    return;

                _patron.Clear();
                // _patron.UID = "#fingerprint";
                _patron.PII = result.Message;
                _patron.IsFingerprintSource = true;
                // TODO: 此种情况下，要禁止后续从读卡器获取，直到新一轮开始。
                // “新一轮”意思是图书全部取走以后开始的下一轮
            }
            catch (Exception ex)
            {
                SetPatronError("fingerprint", ex.Message);
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
                SetPatronError("getreaderinfo", $"读者 '{pii}': {result.ErrorInfo}");
                return;
            }

            SetPatronError("getreaderinfo", "");

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
                        if (entity.TagInfo == null)
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
                        }

                        Debug.Assert(entity.TagInfo != null);

                        LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
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
                SetGlobalError("current", ex.Message);
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
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new NormalResult { Value = -1, ErrorInfo = "dp2library 服务器 URL 尚未配置，无法获得读者信息" };
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
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged("Error");
                }
            }
        }

        private string _globalError = null;   // "test error line asdljasdkf; ;jasldfjasdjkf aasdfasdf";

        public string GlobalError
        {
            get => _globalError;
            set
            {
                if (_globalError != value)
                {
                    _globalError = value;
                    OnPropertyChanged("GlobalError");
                }
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

#if NO
        void ClearPatronError()
        {
            _patron.Error = null;
        }
#endif

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
            if (IsPatronOK(action, out string check_message) == false)
            {
                if (string.IsNullOrEmpty(check_message))
                    check_message = $"读卡器上的当前读者卡状态不正确。无法进行{action_name}操作";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.MessageText = check_message;
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
                this.ClearTagTable(uid);
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
        bool IsPatronOK(string action, out string message)
        {
            message = "";

            // 还书和续借操作，允许读者区为空
            if (action == "return" || action == "renew")
            {
                if (_patron.UID == null)
                    return true;
            }

            // 如果 UID 为空，而 Barcode 有内容，也是 OK 的。这是指纹的场景
            if (string.IsNullOrEmpty(_patron.UID) == true
                && string.IsNullOrEmpty(_patron.Barcode) == false)
                return true;

            // UID 和 Barcode 都不为空。这是 15693 和 14443 读者卡的场景
            if (string.IsNullOrEmpty(_patron.UID) == false
    && string.IsNullOrEmpty(_patron.Barcode) == false)
                return true;

            string debug_info = $"uid:[{_patron.UID}],barcode:[{_patron.Barcode}]";
            if (action == "borrow")
            {
                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先放好读者卡，或扫入一次指纹，然后再进行借书操作({debug_info})";
                else
                    message = $"请先放好读者卡，然后再进行借书操作({debug_info})";
            }
            else
            {
                // 调试用
                message = $"读卡器上的当前读者卡状态不正确。无法进行 xxx 操作({debug_info})";
            }
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

#region patron 分类报错机制

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint getreaderinfo
        ErrorTable _patronErrorTable = null;

        // 设置读者区域错误字符串
        void SetPatronError(string type, string error)
        {
            _patronErrorTable.SetError(type, error);
        }

#if NO
        // 合成读者区域错误字符串，用于刷新显示
        string GetPatronError()
        {
            List<string> errors = new List<string>();
            foreach (string type in _patronErrorTable.Keys)
            {
                string error = _patronErrorTable[type] as string;
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error);
            }
            if (errors.Count == 0)
                return null;
            if (errors.Count == 1)
                return errors[0];
            int i = 0;
            StringBuilder text = new StringBuilder();
            foreach (string error in errors)
            {
                if (text.Length > 0)
                    text.Append("\r\n");
                text.Append($"{i + 1}) {error}");
                i++;
            }
            return text.ToString();
            // return StringUtil.MakePathList(errors, "\r\n");
        }
#endif

#endregion

#region global 分类报错机制

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint
        ErrorTable _globalErrorTable = null;

        // 设置全局区域错误字符串
        void SetGlobalError(string type, string error)
        {
            _globalErrorTable.SetError(type, error);

            // 指纹方面的报错，还要兑现到 App 中
            if (type == "fingerprint")
                App.CurrentApp.SetError(type, error);
        }

#if NO
        // 合成全局区域错误字符串，用于刷新显示
        string GetGlobalError()
        {
            List<string> errors = new List<string>();
            foreach (string type in _globalErrorTable.Keys)
            {
                string error = _globalErrorTable[type] as string;
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error.Replace("\r\n", "\n").TrimEnd(new char[] { '\n', ' ' }));
            }
            if (errors.Count == 0)
                return null;
            if (errors.Count == 1)
                return errors[0];
            int i = 0;
            StringBuilder text = new StringBuilder();
            foreach (string error in errors)
            {
                if (text.Length > 0)
                    text.Append("\n");
                text.Append($"{i + 1}) {error}");
                i++;
            }
            return text.ToString();
        }
#endif

#endregion

    }
}
