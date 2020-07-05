using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Windows.Media;

using dp2SSL.Dialog;
using dp2SSL.Models;
using static dp2SSL.LibraryChannelUtil;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.RFID;
using DigitalPlatform.Interfaces;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Core;
using DigitalPlatform.Face;
using DigitalPlatform.WPF;

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

        // Timer _timer = null;

        EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        // CancellationTokenSource _cancel = new CancellationTokenSource();

        public PageBorrow()
        {
            InitializeComponent();

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
            this.patronControl.InputFace += PatronControl_InputFace;

            this._patron.PropertyChanged += _patron_PropertyChanged;

            // _patron.IsFingerprintSource = true;

            App.CurrentApp.PropertyChanged += CurrentApp_PropertyChanged;
        }

        private void CurrentApp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Error")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        bool _stopVideo = false;

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void PatronControl_InputFace(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            RecognitionFaceResult result = null;

            VideoWindow videoRecognition = null;
            App.Invoke(new Action(() =>
            {
                videoRecognition = new VideoWindow
                {
                    TitleText = "识别人脸 ...",
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                videoRecognition.Closed += VideoRecognition_Closed;
                videoRecognition.Show();
            }));
            _stopVideo = false;
            var task = Task.Run(() =>
            {
                try
                {
                    DisplayVideo(videoRecognition);
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
            try
            {
                result = await RecognitionFaceAsync("");
                if (result.Value == -1)
                {
                    if (result.ErrorCode != "cancelled")
                        SetGlobalError("face", result.ErrorInfo);
                    DisplayError(ref videoRecognition, result.ErrorInfo);
                    return;
                }

                SetGlobalError("face", null);
            }
            finally
            {
                if (videoRecognition != null)
                    App.Invoke(new Action(() =>
                    {
                        videoRecognition.Close();
                    }));
            }

            GetMessageResult message = new GetMessageResult
            {
                Value = 1,
                Message = result.Patron,
            };
            SetPatronInfo(message);
            SetQuality("");
            var fill_result = await FillPatronDetailAsync();
            Welcome(fill_result.Value == -1);
        }

        void DisplayVideo(VideoWindow window)
        {
            while (_stopVideo == false)
            {
                var result = FaceManager.GetImage("");
                if (result.ImageData == null)
                {
                    Thread.Sleep(500);
                    continue;
                }
                MemoryStream stream = new MemoryStream(result.ImageData);
                try
                {
                    App.Invoke(new Action(() =>
                    {
                        window.SetPhoto(stream);
                    }));
                    stream = null;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
        }

        private void VideoRecognition_Closed(object sender, EventArgs e)
        {
            FaceManager.CancelRecognitionFace();
            _stopVideo = true;
            RemoveLayer();
        }

        void EnableControls(bool enable)
        {
            App.Invoke(new Action(() =>
            {
                this.borrowButton.IsEnabled = enable;
                this.returnButton.IsEnabled = enable;
                this.goHome.IsEnabled = enable;
                this.patronControl.inputFace.IsEnabled = enable;
            }));
        }

        async Task<RecognitionFaceResult> RecognitionFaceAsync(string style)
        {
            EnableControls(false);
            try
            {
                return await Task.Run<RecognitionFaceResult>(() =>
                {
                    // 2019/9/6 增加
                    var result = FaceManager.GetState("camera");
                    if (result.Value == -1)
                        return new RecognitionFaceResult
                        {
                            Value = -1,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    return FaceManager.RecognitionFace("");
                });
            }
            finally
            {
                EnableControls(true);
            }
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void PageBorrow_Loaded(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            App.IsPageBorrowActive = true;

            SetGlobalError("current", null); // 清除以前残留的报错信息
            SetGlobalError("scan_barcode", null);

            FingerprintManager.SetError += FingerprintManager_SetError;
            FingerprintManager.Touched += FingerprintManager_Touched;

            App.LineFeed += App_LineFeed;

            // 2020/7/5
            App.InitialRfidManager();

            RfidManager.ClearCache();
            // 处理以前积累的 tags
            // RfidManager.TriggerLastListTags();

            // 注：将来也许可以通过(RFID 以外的)其他方式输入图书号码
            if (string.IsNullOrEmpty(RfidManager.Url))
                this.SetGlobalError("rfid", "尚未配置 RFID 中心 URL");

            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);

            {
                /*
                List<string> style = new List<string>();
                if (string.IsNullOrEmpty(App.RfidUrl) == false)
                    style.Add("rfid");
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    style.Add("fingerprint");
                if (string.IsNullOrEmpty(App.FaceUrl) == false
                    && StringUtil.IsInList("registerFace", this.ActionButtons) == false)
                    style.Add("face");
                    */
                this.patronControl.SetStartMessage(GetPageStyleList());
            }

            App.Invoke(new Action(() =>
            {
                // 身份读卡器竖向放置，才有固定读者信息的必要
                if (App.PatronReaderVertical)
                    fixAndClear.Visibility = Visibility.Visible;
                else
                    fixAndClear.Visibility = Visibility.Collapsed;
            }));

            // SetGlobalError("test", "test error");

            ////
            App.TagChanged += CurrentApp_TagChanged;
            while (true)
            {
                _tagChangedCount = 0;
                await InitialEntitiesAsync();
                if (_tagChangedCount == 0)
                    break;  // 只有当初始化过程中没有被 TagChanged 事件打扰过，才算初始化成功了。否则就要重新初始化
            }
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void App_LineFeed(object sender, LineFeedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            // if (App.EnablePatronBarcode == false)
            if (string.IsNullOrEmpty(App.PatronBarcodeStyle) || App.PatronBarcodeStyle == "禁用")
            {
                SetGlobalError("scan_barcode", "当前设置参数不接受扫入条码");
                return;
            }

            // 扫入一个条码
            string barcode = e.Text.ToUpper();
            // 检查防范空字符串，和使用工作人员方式(~开头)的字符串
            if (string.IsNullOrEmpty(barcode) || barcode.StartsWith("~"))
                return;

            // 2020/6/3
            var styles = StringUtil.SplitList(App.PatronBarcodeStyle, "+");
            if (barcode.StartsWith("PQR:"))
            {
                // 二维码情形
                if (styles.IndexOf("二维码") == -1)
                    return;
            }
            else
            {
                // 一维码情形
                if (styles.IndexOf("一维码") == -1)
                    return;
            }

            SetGlobalError("scan_barcode", null);

            SetPatronInfo(new GetMessageResult { Message = barcode });

            var result = await FillPatronDetailAsync();
            Welcome(result.Value == -1);
        }

        string GetPageStyleList()
        {
            List<string> style = new List<string>();
            if (string.IsNullOrEmpty(App.RfidUrl) == false)
                style.Add("rfid");
            if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                style.Add("fingerprint");
            if (string.IsNullOrEmpty(App.FaceUrl) == false
                && StringUtil.IsInList("registerFace", this.ActionButtons) == false)
                style.Add("face");

            return StringUtil.MakePathList(style);
        }

        /*
        private void CurrentApp_TagSetError(object sender, SetErrorEventArgs e)
        {
            this.SetGlobalError("rfid", e.Error);
        }
        */

        class TagChangedMessage
        {
            public object sender { get; set; }
            public TagChangedEventArgs e { get; set; }
        }

        int _tagChangedCount = 0;

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void CurrentApp_TagChanged(object sender, TagChangedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            if (_skipTagChanged > 0)
                return;

            _tagChangedCount++;
            await ChangeEntitiesAsync((BaseChannel<IRfid>)sender, e);
        }

        public static bool isPatronChanged(TagChangedEventArgs e)
        {
            if (e.AddPatrons?.Count > 0)
                return true;
            if (e.RemovePatrons?.Count > 0)
                return true;
            if (e.UpdatePatrons?.Count > 0)
                return true;

            return false;
        }

        // 跟随事件动态更新列表
        async Task ChangeEntitiesAsync(BaseChannel<IRfid> channel,
            TagChangedEventArgs e)
        {
            // 读者。不再精细的进行增删改跟踪操作，而是笼统地看 TagList.Patrons 集合即可
            if (isPatronChanged(e))
            {
                var task = RefreshPatronsAsync();
            }

            // 2020/4/8
            if (booksControl.Visibility != Visibility.Visible)
                return;

            bool changed = false;
            List<Entity> update_entities = new List<Entity>();
            App.Invoke(new Action(() =>
            {
                if (e.AddBooks != null)
                    foreach (var tag in e.AddBooks)
                    {
                        var entity = _entities.Add(tag);
                        update_entities.Add(entity);
                    }
                if (e.RemoveBooks != null)
                    foreach (var tag in e.RemoveBooks)
                    {
                        _entities.Remove(tag.OneTag.UID);
                        changed = true;
                    }
                if (e.UpdateBooks != null)
                    foreach (var tag in e.UpdateBooks)
                    {
                        var entity = _entities.Update(tag);
                        if (entity != null)
                            update_entities.Add(entity);
                    }
            }));

            if (update_entities.Count > 0)
            {
                await FillBookFieldsAsync(channel, update_entities);

                Trigger(update_entities);

                // 自动检查 EAS 状态
                CheckEAS(update_entities);
            }
            else if (changed)
            {
                // 修改 borrowable
                booksControl.SetBorrowable();
            }

            if (update_entities.Count > 0)
                changed = true;

            // 新放上图书会中断延迟任务
            if (e.AddBooks != null && e.AddBooks.Count > 0)
                CancelDelayClearTask();

            {
                if (_entities.Count == 0
        && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
        && (_patron.IsFingerprintSource || App.PatronReaderVertical == true))
                    PatronClear(true);

                // 2019/7/1
                // 当读卡器上的图书全部拿走时候，自动关闭残留的模式对话框
                if (_entities.Count == 0
    && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
    )
                    CloseDialogs();

                // 当图书全部被移走时，如果身份读卡器横向放置，需要延时提醒不要忘记拿走读者卡
                if (_entities.Count == 0 && changed == true
                    && App.PatronReaderVertical == false)
                    BeginNotifyTask();

                // 当列表为空的时候，主动清空一次 tag 缓存。这样读者可以用拿走全部标签一次的方法来迫使清除缓存(比如中途利用内务修改过 RFID 标签的 EAS)
                // TODO: 不过此举对反复拿放图书的响应速度有一定影响。后面也可以考虑继续改进(比如设立一个专门的清除缓存按钮，这里就不要清除缓存了)
                if (_entities.Count == 0 && TagList.TagTableCount > 0)
                    TagList.ClearTagTable(null);
            }
        }

        // 首次初始化 Entity 列表
        async Task<NormalResult> InitialEntitiesAsync()
        {
            _entities.Clear();  // 2019/9/4

            if (booksControl.Visibility == Visibility.Visible)  // 2020/4/8
            {
                var books = TagList.Books;
                if (books.Count > 0)
                {
                    List<Entity> update_entities = new List<Entity>();

                    foreach (var tag in books)
                    {
                        var entity = _entities.Add(tag);
                        update_entities.Add(entity);
                    }

                    if (update_entities.Count > 0)
                    {
                        try
                        {
                            BaseChannel<IRfid> channel = RfidManager.GetChannel();
                            try
                            {
                                await FillBookFieldsAsync(channel, update_entities);

                                Trigger(update_entities);
                            }
                            finally
                            {
                                RfidManager.ReturnChannel(channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            string error = $"填充图书信息时出现异常: {ex.Message}";
                            SetGlobalError("rfid", error);
                            return new NormalResult { Value = -1, ErrorInfo = error };
                        }

                        // 自动检查 EAS 状态
                        CheckEAS(update_entities);
                    }
                }
                else
                {
                    booksControl.SetBorrowable();
                }
            }

            var task = RefreshPatronsAsync();
            return new NormalResult();
        }

        void Trigger(List<Entity> update_entities)
        {
            return;
            List<Entity> results = new List<Entity>();
            foreach (var entity in update_entities)
            {
                // 检查 PII。TODO: 还要检查在架状态
                if (string.IsNullOrEmpty(entity.PII) == false)
                    results.Add(entity);
            }
            if (results.Count == 0)
                return;

            App.Invoke(new Action(() =>
            {
                MessageBox.Show($"开始。数量 [{results.Count}]");
            }));
        }

        // ReaderWriterLockSlim _lock_refreshPatrons = new ReaderWriterLockSlim();

        // TODO: 要和以前比对，读者信息是否发生了变化。如果没有变化就不要刷新界面了
        async Task RefreshPatronsAsync()
        {
            //_lock_refreshPatrons.EnterWriteLock();
            try
            {
                var patrons = TagList.Patrons;
                if (patrons.Count == 1)
                    _patron.IsRfidSource = true;

                if (_patron.IsFingerprintSource)
                {
                    // 指纹仪来源
                }
                else
                {
                    SetQuality("");
                    // RFID 来源
                    if (patrons.Count == 1)
                    {
                        // result.Value:
                        //      -1  出错
                        //      0   未进行刷新
                        //      1   成功进行了刷新
                        var result = _patron.Fill(patrons[0].OneTag);
                        if (result.Value == 0)
                            return;
                        if (result.Value == -1)
                        {
                            SetPatronError("rfid_multi", result.ErrorInfo);
                            return;
                        }
                        else
                            SetPatronError("rfid_multi", "");   // 2019/5/22

                        // 2019/5/29
                        var fill_result = await FillPatronDetailAsync();
                        Welcome(fill_result.Value == -1);
                    }
                    else
                    {
                        // 会顺便清掉读者信息区的错误信息
                        if (App.PatronReaderVertical == false)
                            SetPatronError("getreaderinfo", "");

                        if (patrons.Count > 1)
                        {
                            PatronClear();
                            // 读卡器上放了多张读者卡
                            SetPatronError("rfid_multi", $"读卡器上放了多张读者卡({patrons.Count})。请拿走多余的");
                        }
                        else
                        {
                            // 2019/12/8
                            if (App.PatronReaderVertical == false)
                                PatronClear();

                            SetPatronError("rfid_multi", "");   // 2019/5/20
                        }
                    }
                }

                if (patrons.Count == 0)
                    CancelWarning();
            }
            catch (Exception ex)
            {
                SetGlobalError("rfid", $"RefreshPatrons() 出现异常: {ex.Message}");
            }
            finally
            {
                //_lock_refreshPatrons.ExitWriteLock();
            }
        }

#if OLD_CODE
        private async void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetGlobalError("rfid", e.Error);
            if (e.Error == null)
            {
                // 恢复正常
            }
            else
            {
                // 进入错误状态
                if (_rfidState != "error")
                {
                    await ClearBooksAndPatron(null);
                    /*
                    ClearBookList();
                    FillBookFields(channel);
                    PatronClear();
                    */
                }

                _rfidState = "error";
            }
        }
#endif

#if OLD_RFID
        private async void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            await Refresh(sender as BaseChannel<IRfid>, e.Result);
        }


#endif

        private void FingerprintManager_SetError(object sender, SetErrorEventArgs e)
        {
            if (e.Error == null)
                SetGlobalError("fingerprint", null);
            else
                SetGlobalError("fingerprint", $"fingerprint error: {e.Error}");  // 2019/9/11 增加 fingerprinterror:
        }

        void SetQuality(string text)
        {
            App.Invoke(new Action(() =>
            {
                this.Quality.Text = text;
            }));
        }

        // 从指纹阅读器获取消息(第一阶段)
#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void FingerprintManager_Touched(object sender, TouchedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            // 注如果 FingerprintManager 已经挂接 SetError 事件，Touched 事件这里就可以忽略 result.Value == -1 情况
            if (e.Result.Value == -1)
                return;

            SetPatronInfo(e.Result);

            SetQuality(e.Quality == 0 ? "" : e.Quality.ToString());

            var fill_result = await FillPatronDetailAsync();
            Welcome(fill_result.Value == -1);
#if NO
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _patron.IsFingerprintSource = true;
                _patron.Barcode = "test1234";
            }));
#endif
        }

#if REMOVED
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
#endif

#if NO
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

#endif

#if NO
        void eventProxy_MessageArrived(string Message)
        {
            MessageBox.Show(Message);
        }
#endif
        List<Window> _dialogs = new List<Window>();

        void CloseDialogs()
        {
            // 确保 page 关闭时对话框能自动关闭
            App.Invoke(new Action(() =>
            {
                foreach (var window in _dialogs)
                {
                    window.Close();
                }
            }));
        }

        void MemoryDialog(Window dialog)
        {
            _dialogs.Add(dialog);
        }

        private void PageBorrow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.IsPageBorrowActive = false;

            // _cancel.Cancel();
            CancelDelayClearTask();

            // 释放 Loaded 里面分配的资源
            // RfidManager.SetError -= RfidManager_SetError;
            App.TagChanged -= CurrentApp_TagChanged;

            FingerprintManager.Touched -= FingerprintManager_Touched;
            FingerprintManager.SetError -= FingerprintManager_SetError;

            App.LineFeed -= App_LineFeed;

            /*
            // 释放构造函数里面分配的资源
            //Loaded -= PageBorrow_Loaded;
            //Unloaded -= PageBorrow_Unloaded;
            this.patronControl.InputFace -= PatronControl_InputFace;
            this._patron.PropertyChanged -= _patron_PropertyChanged;
            App.CurrentApp.PropertyChanged -= CurrentApp_PropertyChanged;
            */

            // 确保 page 关闭时对话框能自动关闭
            CloseDialogs();

            PatronClear();  // 2019/9/3
        }

        bool _visiblityChanged = false;

        private void _patron_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoPath")
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        this.patronControl.LoadPhoto(_patron.PhotoPath);
                    }
                    catch (Exception ex)
                    {
                        SetGlobalError("patron", $"patron exception: {ex.Message}");    // 2019/9/11 增加 patron exception:
                    }
                });
            }

            if (e.PropertyName == "UID"
                || e.PropertyName == "Barcode")
            {
                // 如果 patronControl 本来是隐藏状态，但读卡器上放上了读者卡，这时候要把 patronControl 恢复显示
                if ((string.IsNullOrEmpty(_patron.UID) == false || string.IsNullOrEmpty(_patron.Barcode) == false)
                    && this.patronControl.Visibility != Visibility.Visible)
                    App.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Visible;
                        _visiblityChanged = true;
                    }));
                // 如果读者卡又被拿走了，则要恢复 patronControl 的隐藏状态
                // 注意，这里要看当前是借书还是还书画面。借书画面还是要显示 patronControl
                else if (string.IsNullOrEmpty(_patron.UID) == true && string.IsNullOrEmpty(_patron.Barcode) == true
    && this.patronControl.Visibility == Visibility.Visible
    && _visiblityChanged)
                    App.Invoke(new Action(() =>
                    {
                        SetPatronControlVisibility();
                        // patronControl.Visibility = Visibility.Collapsed;
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

        // CancellationTokenSource _cancelRefresh = null;

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
                if (registerFace.Visibility == Visibility.Visible)
                    buttons.Add("registerFace");
                if (bindPatronCard.Visibility == Visibility.Visible)
                    buttons.Add("bindPatronCard");

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

                if (buttons.IndexOf("registerFace") != -1)
                    registerFace.Visibility = Visibility.Visible;
                else
                    registerFace.Visibility = Visibility.Collapsed;

                if (buttons.IndexOf("deleteFace") != -1)
                    deleteFace.Visibility = Visibility.Visible;
                else
                    deleteFace.Visibility = Visibility.Collapsed;

                if (buttons.IndexOf("bindPatronCard") != -1)
                    bindPatronCard.Visibility = Visibility.Visible;
                else
                    bindPatronCard.Visibility = Visibility.Collapsed;

                if (buttons.IndexOf("releasePatronCard") != -1)
                    releasPatronCard.Visibility = Visibility.Visible;
                else
                    releasPatronCard.Visibility = Visibility.Collapsed;

                if (registerFace.Visibility == Visibility.Visible
                    || deleteFace.Visibility == Visibility.Visible
                    || bindPatronCard.Visibility == Visibility.Visible
                    || releasPatronCard.Visibility == Visibility.Visible)
                {
                    this.booksControl.Visibility = Visibility.Collapsed;
                    // this.patronControl.HideInputFaceButton();

                    // 2020/4/16
                    SetPatronControlVisibility();
                }
                else
                {
                    this.booksControl.Visibility = Visibility.Visible;

                    SetPatronControlVisibility();
                    /*
                    // (普通)还书和续借操作并不需要读者卡
                    if (borrowButton.Visibility != Visibility.Visible)
                        this.patronControl.Visibility = Visibility.Collapsed;
                    else
                        this.patronControl.Visibility = Visibility.Visible; // 2019/9/3
                        */
                }
            }
        }

        // 决定是否隐藏读者信息控件
        void SetPatronControlVisibility()
        {
            // 2020/4/16
            if (returnButton.Visibility == Visibility.Visible
                || renewButton.Visibility == Visibility.Visible)
                this.patronControl.Visibility = Visibility.Collapsed;
            else
                this.patronControl.Visibility = Visibility.Visible;

            /*
            // (普通)还书和续借操作并不需要读者卡
            if (borrowButton.Visibility != Visibility.Visible)
                this.patronControl.Visibility = Visibility.Collapsed;
            else
                this.patronControl.Visibility = Visibility.Visible; // 2019/9/3
                */
        }

#if OLD_RFID

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
        GetTagInfoResult GetTagInfo(BaseChannel<IRfid> channel, string uid)
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

#endif

        // 清除图书列表
        void ClearBookList()
        {
            App.Invoke(new Action(() =>
            {
                List<Entity> new_entities = null;
                _entities.Refresh(new List<OneTag>(), ref new_entities);
            }));
        }

        async Task ClearBooksAndPatronAsync(BaseChannel<IRfid> channel)
        {
            try
            {
                ClearBookList();
                await FillBookFieldsAsync(channel, new List<Entity>());
            }
            catch (Exception ex)
            {
                LibraryChannelManager.Log?.Error($"ClearBooksAndPatron() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");
            }
            PatronClear();
        }

        //int _inRefresh = 0;

        //string _rfidState = "ok";   // ok/error

#if OLD_RFID
        async Task Refresh(BaseChannel<IRfid> channel, ListTagsResult result)
        {
            Debug.Assert(channel != null, "");

            /*
            // 防止重入
            int v = Interlocked.Increment(ref this._inRefresh);
            if (v > 1)
            {
                Interlocked.Decrement(ref this._inRefresh);
                return;
            }
            */

            // _cancelRefresh = new CancellationTokenSource();
            try
            {
                SetGlobalError("current", "");

                // 获得所有协议类型的标签

                if (result.Value == -1)
                {
                    SetGlobalError("current", $"RFID 中心错误:{result.ErrorInfo}, 错误码:{result.ErrorCode}");
                    // RFID 正常状态和错误状态之间切换时才需要连带清掉读者信息
                    if (_rfidState != "error")
                    {
                        await ClearBooksAndPatron(channel);
                        /*
                        ClearBookList();
                        FillBookFields(channel);
                        _patron.Clear();
                        */
                    }

                    _rfidState = "error";
                    return;
                }
                else
                    _rfidState = "ok";

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
                            var gettaginfo_result = GetTagInfo(channel, tag.UID);
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

                    // 2019/7/1
                    // 当读卡器上的图书全部拿走时候，自动关闭残留的模式对话框
                    if (_entities.Count == 0
    && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
    )
                        CloseDialogs();
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

                        // 2019/5/29
                        var task = FillPatronDetail();
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

                //GetPatronFromFingerprint();

                await FillBookFields(channel, _entities);
                // FillPatronDetail();

                CheckEAS(new_entities);
            }
            catch (Exception ex)
            {
                SetGlobalError("current", $"后台刷新过程出现异常: {ex.Message}");
                return;
            }
            finally
            {
                // Interlocked.Decrement(ref this._inRefresh);
            }
        }
#endif

        // 检查芯片的 EAS 状态
        void CheckEAS(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (entity.TagInfo == null)
                    continue;

                // 对状态不明(State == null)册记录暂时不处理修正 EAS
                if (entity.State == null)
                    continue;

                // 检测 EAS 是否正确
                NormalResult result = null;
                if (StringUtil.IsInList("borrowed", entity.State) && entity.TagInfo.EAS == true)
                    result = SetEAS(entity.UID, entity.Antenna, false);
                else if (StringUtil.IsInList("onshelf", entity.State) && entity.TagInfo.EAS == false)
                    result = SetEAS(entity.UID, entity.Antenna, true);
                else
                    continue;

                if (result.Value == -1)
                    entity.SetError($"自动修正 EAS 时出错: {result.ErrorInfo}", "red");
                else
                    entity.SetError("自动修正 EAS 成功", "green");
            }
        }

        // 从指纹阅读器获取消息(第一阶段)
        void SetPatronInfo(GetMessageResult result)
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{

            if (result.Value == -1)
            {
                SetPatronError("fingerprint", $"指纹中心出错: {result.ErrorInfo}, 错误码: {result.ErrorCode}");
                if (_patron.IsFingerprintSource || App.PatronReaderVertical == true)
                    PatronClear();    // 只有当面板上的读者信息来源是指纹仪时(或者 RFID 读者卡配置了不持久特性)，才清除面板上的读者信息
                return;
            }
            else
            {
                // 清除以前残留的报错信息
                SetPatronError("fingerprint", "");
            }

            if (result.Message == null)
                return;

            PatronClear();
            _patron.IsFingerprintSource = true;
            _patron.PII = result.Message;
            // TODO: 此种情况下，要禁止后续从读卡器获取，直到新一轮开始。
            // “新一轮”意思是图书全部取走以后开始的下一轮
            //}));
        }


#if REMOVED
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
#endif
        // 填充读者信息的其他字段(第二阶段)
        async Task<NormalResult> FillPatronDetailAsync(bool force = false)
        {
#if NO
            if (_cancelRefresh == null
    || _cancelRefresh.IsCancellationRequested)
                return;
#endif

            //if (string.IsNullOrEmpty(_patron.Error) == false)
            //    return;

#if NO
            string name = Application.Current.Dispatcher.Invoke(new Func<string>(() =>
            {
                return this.Name;
            }));
#endif
            _patron.Waiting = true;
            try
            {

                // 已经填充过了
                if (_patron.PatronName != null
                && force == false)
                    return new NormalResult();

                string pii = _patron.PII;
                if (string.IsNullOrEmpty(pii))
                    pii = _patron.UID;

                if (string.IsNullOrEmpty(pii))
                    return new NormalResult();

                // TODO: 改造为 await
                // return.Value:
                //      -1  出错
                //      0   读者记录没有找到
                //      1   成功
                GetReaderInfoResult result = await
                    Task<GetReaderInfoResult>.Run(() =>
                    {
                        return GetReaderInfo(pii);
                    });

                if (result.Value != 1)
                {
                    string error = $"读者 '{pii}': {result.ErrorInfo}";
                    SetPatronError("getreaderinfo", error);
                    return new NormalResult { Value = -1, ErrorInfo = error };
                }

                SetPatronError("getreaderinfo", "");

                if (force)
                    _patron.PhotoPath = "";
                // string old_photopath = _patron.PhotoPath;
                App.Invoke(new Action(() =>
                {
                    _patron.SetPatronXml(result.RecPath, result.ReaderXml, result.Timestamp);
                    this.patronControl.SetBorrowed(result.ReaderXml);
                }));

                // 显示在借图书列表
                List<Entity> entities = new List<Entity>();
                foreach (Entity entity in this.patronControl.BorrowedEntities)
                {
                    entities.Add(entity);
                }
                if (entities.Count > 0)
                {
                    try
                    {
                        BaseChannel<IRfid> channel = RfidManager.GetChannel();
                        try
                        {
                            await FillBookFieldsAsync(channel, entities);
                        }
                        finally
                        {
                            RfidManager.ReturnChannel(channel);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = $"填充读者信息时出现异常: {ex.Message}";
                        SetGlobalError("rfid", error);
                        return new NormalResult { Value = -1, ErrorInfo = error };
                    }
                }
#if NO
            // 装载图象
            if (old_photopath != _patron.PhotoPath)
            {
                Task.Run(()=> {
                    LoadPhoto(_patron.PhotoPath);
                });
            }
#endif
                return new NormalResult();
            }
            finally
            {
                _patron.Waiting = false;
            }
        }

#if NO
        void LoadPhoto(string photo_path)
        {
            if (string.IsNullOrEmpty(photo_path))
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.patronControl.SetPhoto(null);
                }));
                return;
            }

            Stream stream = new MemoryStream();
            var channel = App.CurrentApp.GetChannel();
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
                    SetGlobalError("patron", $"获取读者照片时出错: {strError}");
                    return;
                }

                stream.Seek(0, SeekOrigin.Begin);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.patronControl.SetPhoto(stream);
                }));
                stream = null;
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
                if (stream != null)
                    stream.Dispose();
            }
        }
#endif
        // 第二阶段：填充图书信息的 PII 和 Title 字段
        async Task FillBookFieldsAsync(BaseChannel<IRfid> channel,
            List<Entity> entities)
        {
#if NO
            RfidChannel channel = RFID.StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);
#endif
            try
            {
                foreach (Entity entity in entities)
                {
                    /*
                    if (_cancel == null
                        || _cancel.IsCancellationRequested)
                        return;
                        */
                    if (entity.FillFinished == true)
                        continue;

                    //if (string.IsNullOrEmpty(entity.Error) == false)
                    //    continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        if (entity.TagInfo == null)
                            continue;
#if OLD_RFID
                        if (entity.TagInfo == null && channel != null)
                        {
                            // var result = channel.Object.GetTagInfo("*", entity.UID);
                            var result = GetTagInfo(channel, entity.UID);
                            if (result.Value == -1)
                            {
                                entity.SetError(result.ErrorInfo);
                                continue;
                            }

                            Debug.Assert(result.TagInfo != null);

                            entity.TagInfo = result.TagInfo;
                        }
#endif

                        Debug.Assert(entity.TagInfo != null);

                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
                        string pii = chip.FindElement(ElementOID.PII)?.Text;
                        entity.PII = GetCaption(pii);
                    }

                    bool clearError = true;

                    // 获得 Title
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        GetEntityDataResult result = await GetEntityDataAsync(entity.PII, "network");

                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }

                        entity.Title = GetCaption(result.Title);
                        entity.SetData(result.ItemRecPath, result.ItemXml);

                        // 2020/7/3
                        // 获得册记录阶段出错，但获得书目摘要成功
                        if (string.IsNullOrEmpty(result.ErrorCode) == false)
                        {
                            entity.SetError(result.ErrorInfo);
                            clearError = false;
                        }
                    }

                    if (clearError == true)
                        entity.SetError(null);
                    entity.FillFinished = true;
                }

                booksControl.SetBorrowable();
            }
            catch (Exception ex)
            {
                LibraryChannelManager.Log?.Error($"FillBookFields() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                SetGlobalError("current", $"FillBookFields() 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
            }
        }

        public static string GetCaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";

            return text;
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

        /*
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
        */

        public string Error
        {
            get
            {
                return App.CurrentApp.Error;
            }
            /*
            set
            {
                if (App.CurrentApp.Error != value)
                {
                    App.CurrentApp.Error = value;
                    OnPropertyChanged("Error");
                }
            }*/
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
            _ = Task.Run(() =>
            {
                try
                {
                    Loan("borrow");
                }
                catch
                {
                    // TODO: 写入错误日志
                }
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

            App.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.TitleText = action_name;
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

                DisplayError(ref progress, check_message);
                /*
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.MessageText = ;
                    progress.BackColor = "red";
                    progress = null;
                }));
                */
                return;
            }

            // 借书操作必须要有读者卡。(还书和续借，可要可不要)
            if (action == "borrow")
            {
                if (string.IsNullOrEmpty(_patron.Barcode))
                {
                    DisplayError(ref progress, $"请先在读卡器上放好读者卡，再进行{action_name}");

                    /*
                    MemoryDialog(progress);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = $"请先在读卡器上放好读者卡，再进行{action_name}";
                        progress.BackColor = "red";
                        progress = null;
                    }));
                    */
                    return;
                }
            }

            LibraryChannel channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                ClearEntitiesError();

                App.Invoke(new Action(() =>
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
                        if (action == "borrow" && StringUtil.IsInList("borrowed", entity.State))
                        {
                            entity.SetError($"本册是外借状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }
                        if (action == "renew" && StringUtil.IsInList("onshelf", entity.State))
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
                        if (StringUtil.IsInList("onshelf", entity.State))
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }

                        // TODO: 增加检查 EAS 现有状态功能，如果已经是 true 则不用修改，后面 API 遇到出错后也不要回滚 EAS
                        // return 操作，提前修改 EAS
                        // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, action == "return");
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

                    App.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value++;
                    }));

                    if (lRet == -1)
                    {
                        // return 操作如果 API 失败，则要改回原来的 EAS 状态
                        if (action == "return")
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, false);
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
                        var result = SetEAS(entity.UID, entity.Antenna, action == "return");
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

                        string message = $"{action_name}成功";
                        if (lRet == 1 && string.IsNullOrEmpty(strError) == false)
                            message = strError;
                        entity.SetError(message,
                            lRet == 1 ? "yellow" : "green");
                        success_count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                App.Invoke(new Action(() =>
                {
                    progress.ProgressBar.Visibility = Visibility.Collapsed;
                    // progress.ProgressBar.Value = progress.ProgressBar.Maximum;
                }));

                // 修改 borrowable
                booksControl.SetBorrowable();

                if (errors.Count > 0)
                {
                    string error = StringUtil.MakePathList(errors, "\r\n");
                    string message = $"{action_name}操作出错 {errors.Count} 个";
                    if (success_count > 0)
                        message += $"，成功 {success_count} 个";
                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 个被忽略)";

                    DisplayError(ref progress, message);
                    App.CurrentApp.Speak(message);

                    /*
                    MemoryDialog(progress);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = message;

                        // progress.MessageText = $"{action_name}操作出错: \r\n{error}";
                        progress.BackColor = "red";
                        progress = null;
                    }));
                    */
                    return; // new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, "; ") };
                }
                else
                {
                    // 成功
                    string backColor = "green";
                    string message = $"{action_name}操作成功 {success_count} 笔";
                    string speak = $"{action_name}完成";

                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 笔被忽略)";
                    if (skip_count > 0 && success_count == 0)
                    {
                        backColor = "yellow";
                        message = $"全部 {skip_count} 笔{action_name}操作被忽略";
                        speak = $"{action_name}失败";
                    }
                    if (skip_count == 0 && success_count == 0)
                    {
                        backColor = "yellow";
                        message = $"请先把图书放到读卡器上，再进行 {action_name}操作";
                        speak = $"{action_name}失败";
                    }

                    DisplayError(ref progress, message, backColor);

                    /*
                    MemoryDialog(progress);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageText = message;
                        progress.BackColor = backColor;
                        progress = null;
                    }));
                    */

                    // 重新装载读者信息和显示
                    var task = FillPatronDetailAsync(true);

                    App.CurrentApp.Speak(speak);
                }

                return; // new NormalResult { Value = success_count };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
                App.Invoke(new Action(() =>
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

        NormalResult SetEAS(string uid, string antenna, bool enable)
        {
            try
            {
                if (uint.TryParse(antenna, out uint antenna_id) == false)
                    antenna_id = 0;
#if OLD_RFID
                this.ClearTagTable(uid);
#endif
                // TagList.ClearTagTable(uid);
                var result = RfidManager.SetEAS($"{uid}", antenna_id, enable);
                if (result.Value != -1)
                {
                    TagList.SetEasData(uid, enable);
                    _entities.SetEasData(uid, enable);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
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

            string fang = "放好";
            if (App.PatronReaderVertical)
                fang = "扫";

            string debug_info = $"uid:[{_patron.UID}],barcode:[{_patron.Barcode}]";
            if (action == "borrow")
            {
                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先{fang}读者卡，或扫入一次指纹，然后再进行借书操作({debug_info})";
                else
                    message = $"请先{fang}读者卡，然后再进行借书操作({debug_info})";
            }
            else if (action == "registerFace"
                || action == "deleteFace")
            {
                string action_name = "注册人脸";
                if (action == "deleteFace")
                    action_name = "删除人脸";

                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先{fang}读者卡，或扫入一次指纹，然后再进行{action_name}操作({debug_info})";
                else
                    message = $"请先{fang}读者卡，然后再进行{action_name}操作({debug_info})";
            }
            else if (action == "bindPatronCard"
                || action == "releasePatronCard")
            {
                string action_name = "绑定新副卡";
                if (action == "releasePatronCard")
                    action_name = "解绑指定副卡";

                /*
                // 提示信息要考虑到应用了指纹和人脸的情况
                List<string> styles = new List<string>();
                styles.Add($"请先{fang}可用的读者卡鉴别身份");
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    styles.Add("或扫入一次指纹");
                if (string.IsNullOrEmpty(App.FaceUrl) == false)
                    styles.Add("或人脸识别");

                message = $"{StringUtil.MakePathList(styles, "，")}，然后再进行{action_name}操作({debug_info})";
                */
                // 提示信息要考虑到应用了指纹和人脸的情况
                List<string> styles = new List<string>();
                styles.Add($"请{fang}可用的读者 RFID 卡鉴别身份");
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    styles.Add("或扫入一次指纹");
                if (string.IsNullOrEmpty(App.FaceUrl) == false)
                    styles.Add("或人脸识别");
                if (string.IsNullOrEmpty(App.PatronBarcodeStyle) == false && App.PatronBarcodeStyle != "禁用")
                    styles.Add("或扫入读者证条码");

                message = $"{StringUtil.MakePathList(styles, "，")} ...";
            }
            else
            {
                // 调试用
                message = $"读卡器上的当前读者卡状态不正确。无法进行 {action} 操作({debug_info})";
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
            _ = Task.Run(() =>
            {
                try
                {
                    Loan("return");
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
        }

        // 续借
        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    Loan("renew");
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

        #region patron 分类报错机制

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint getreaderinfo
        ErrorTable _patronErrorTable = null;

        // 设置读者区域错误字符串
        void SetPatronError(string type, string error)
        {
            _patronErrorTable.SetError(type, error);
            // 如果有错误信息，则主动把“清除读者信息”按钮设为可用，以便读者可以随时清除错误信息
            if (_patron.Error != null)
            {
                App.Invoke(new Action(() =>
                {
                    clearPatron.IsEnabled = true;
                }));
            }
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
        // ErrorTable _globalErrorTable = null;

        // 设置全局区域错误字符串
        void SetGlobalError(string type, string error)
        {
            /*
            _globalErrorTable.SetError(type, error);

            // 指纹方面的报错，还要兑现到 App 中
            if (type == "fingerprint" || type == "rfid")
                App.CurrentApp?.SetError(type, error);
                */
            // Debug.Assert(type != "face", "");

            App.SetError(type, error);
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

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void RegisterFace_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            await RegisterFaceAsync("registerFace");
        }

        // 注册或者删除人脸
        private async Task RegisterFaceAsync(string action)
        {
            GetFeatureStringResult result = null;

            string action_name = "注册人脸";
            if (action == "deleteFace")
                action_name = "删除人脸";

            VideoWindow videoRegister = null;
            App.Invoke(new Action(() =>
            {
                videoRegister = new VideoWindow
                {
                    TitleText = $"{action_name} ...",
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                videoRegister.Closed += VideoRegister_Closed;
                videoRegister.Show();
                AddLayer();
            }));
            try
            {
                if (IsPatronOK(action, out string check_message) == false)
                {
                    if (string.IsNullOrEmpty(check_message))
                        check_message = $"读卡器上的当前读者卡状态不正确。无法进行{action_name}操作";

                    DisplayError(ref videoRegister, check_message, "yellow");
                    return;
                }

                if (action == "registerFace")
                {
                    _stopVideo = false;
                    try
                    {
                        var task = Task.Run(() =>
                        {
                            try
                            {
                                DisplayVideo(videoRegister);
                            }
                            catch
                            {
                                // TODO: 写入错误日志
                            }
                        });

                        // 2019/9/6 增加
                        {
                            var state_result = FaceManager.GetState("camera");
                            if (state_result.Value == -1)
                            {
                                DisplayError(ref videoRegister, state_result.ErrorInfo);
                                return;
                            }
                        }

                        // 启动一个单独的显示倒计时数字的任务
                        var task1 = Task.Run(() =>
                        {
                            try
                            {
                                for (int i = 5; i > 0; i--)
                                {
                                    if (_stopVideo == true)
                                        break;

                                    if (videoRegister == null)
                                        break;

                                    if (videoRegister != null)
                                    {
                                        App.Invoke(new Action(() =>
                                        {
                                            if (videoRegister != null)
                                                videoRegister.TitleText = $"倒计时 {i}";
                                        }));
                                    }
                                    Thread.Sleep(1000);
                                }
                                if (videoRegister != null)
                                {
                                    App.Invoke(new Action(() =>
                                    {
                                        videoRegister.TitleText = "拍摄";
                                    }));
                                }
                            }
                            catch
                            {
                                // TODO: 写入错误日志
                            }
                        });



                        result = await GetFeatureStringAsync("returnImage,countDown,format:jpeg");
                        if (result.Value == -1)
                        {
                            DisplayError(ref videoRegister, result.ErrorInfo);
                            return;
                        }

                        if (result.Value == 0)
                        {
                            DisplayError(ref videoRegister, result.ErrorInfo);
                            return;
                        }

                        // SetGlobalError("face", null);
                    }
                    finally
                    {
                        _stopVideo = true;
                    }
                    // TODO: 背景变为绿色
                }

                videoRegister.Background = new SolidColorBrush(Colors.DarkGreen);
                videoRegister.MessageText = "正在写入读者记录 ...";
                // Thread.Sleep(1000);

                bool changed = false;
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(_patron.Xml);

                if (action == "registerFace")
                {
                    // TODO: 对话框提示以前已经登记过人脸，是否要覆盖？

                    // 在读者 XML 记录中加入 face 元素
                    if (!(dom.DocumentElement.SelectSingleNode("face") is XmlElement face))
                    {
                        face = dom.CreateElement("face");
                        dom.DocumentElement.AppendChild(face);
                    }
                    face.SetAttribute("version", result.Version);
                    face.InnerText = result.FeatureString;
                    changed = true;
                }
                else if (action == "deleteFace")
                {
                    // 删除 face 元素
                    if (!(dom.DocumentElement.SelectSingleNode("face") is XmlElement face))
                    {
                        // 报错说本来就没有人脸信息，所以也没有必要删除
                        DisplayError(ref videoRegister, "读者记录中原本不存在人脸信息 ...");
                        return;
                    }
                    else
                    {
                        face.ParentNode.RemoveChild(face);
                        changed = true;
                    }
                }

                // 删除 dprms:file 元素
                string object_path = "";

                if (action == "registerFace")
                {
                    if (GetCardPhotoObjectPath(dom,
        "face",
        _patron.RecPath,
        out object_path) == true)
                        changed = true;
                }
                else
                {
                    // return:
                    //      false   没有发生修改
                    //      true    发生了修改
                    if (RemoveCardPhotoObject(dom, "face") == true)
                        changed = true;
                }

                // TODO: 用 WPF 对话框
                if (action == "deleteFace")
                {
                    MessageBoxResult dialog_result = MessageBox.Show(
                        "确实要删除人脸信息?\r\n\r\n(人脸信息删除以后，您将无法使用人脸识别功能)",
                        "dp2SSL",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (dialog_result == MessageBoxResult.No)
                        return;
                }


                if (changed == true)
                {
                    // 保存读者记录
                    var save_result = await SetReaderInfoAsync(_patron.RecPath,
                        dom.OuterXml,
                        _patron.Xml,
                        _patron.Timestamp);
                    if (save_result.Value == -1)
                    {
                        DisplayError(ref videoRegister, save_result.ErrorInfo);
                        return;
                    }

                    _patron.Timestamp = save_result.NewTimestamp;
                    _patron.Xml = dom.OuterXml;
                }

                if (action == "registerFace")
                {
                    videoRegister.MessageText = "正在上传读者照片 ...";
                    // Thread.Sleep(1000);

                    // 上传头像
                    var upload_result = await UploadObjectAsync(
            object_path,
            result.ImageData);
                    if (upload_result.Value == -1)
                    {
                        DisplayError(ref videoRegister,
                            $"上传头像文件失败: {upload_result.ErrorInfo}");
                        return;
                    }
                }

                // 上传完对象后通知 facecenter DoReplication 一次
                var notify_result = FaceManager.Notify("faceChanged");
                if (notify_result.Value == -1)
                    SetGlobalError("face", $"FaceManager.Notify() error: {notify_result.ErrorInfo}");   // 2019/9/11 增加 error:

                string message = $"{action_name}成功";
                if (action == "deleteFace")
                    App.CurrentApp.Speak(message);
                DisplayError(ref videoRegister, message, "green");
            }
            finally
            {
                if (videoRegister != null)
                    App.Invoke(new Action(() =>
                    {
                        videoRegister.Close();
                    }));
            }

            // 刷新读者信息区显示
            var temp_task = FillPatronDetailAsync(true);
        }

        void DisplayError(ref SubmitWindow progress,
string message,
string color = "red")
        {
            MemoryDialog(progress);
            var temp = progress;
            App.Invoke(new Action(() =>
            {
                temp.MessageText = message;
                temp.BackColor = color;
                temp = null;
            }));
            progress = null;
        }

        void DisplayError(ref ProgressWindow progress,
    string message,
    string color = "red",
    string set_button_text = null)
        {
            MemoryDialog(progress);
            var temp = progress;
            App.Invoke(new Action(() =>
            {
                if (set_button_text != null)
                    temp.OkButtonText = set_button_text;
                temp.MessageText = message;
                temp.BackColor = color;
                temp = null;
            }));
            progress = null;
        }

        void DisplayError(ref VideoWindow videoRegister,
            string message,
            string color = "red")
        {
            MemoryDialog(videoRegister);
            var temp = videoRegister;
            App.Invoke(new Action(() =>
            {
                temp.MessageText = message;
                temp.BackColor = color;
                temp.okButton.Content = "返回";
                temp = null;
            }));
            videoRegister = null;
        }

        Task<NormalResult> UploadObjectAsync(
            string object_path,
            byte[] imageData)
        {
            return Task<NormalResult>.Run(() =>
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(60);

                try
                {
                    string strClientFilePath = Path.Combine(WpfClientInfo.UserTempDir, "~photo");
                    File.WriteAllBytes(strClientFilePath, imageData);

                    string strMime = PathUtil.MimeTypeFrom(strClientFilePath);
                    string strMetadata = LibraryChannel.BuildMetadata(strMime, Path.GetFileName(strClientFilePath));

                    int nRet = UploadFile(
                        channel,
                        (Stop)null,
                    strClientFilePath,
                    object_path,
                    strMetadata,
                    "", // strStyle,
                    _patron.Timestamp,
                    true,
                    false,
                    (delegate_prompt)null,
                    //(c, m, buttons, seconds) =>
                    //{ },
                    out byte[] output_timestamp,
                    out string strError);
                    if (nRet == -1)
                        return new NormalResult { Value = -1, ErrorInfo = strError };
                    return new NormalResult();
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            });
        }

        #region 下级函数

        // return:
        //      "retry" "skip" 或 "cancel"
        public delegate string delegate_prompt(LibraryChannel channel,
            string message,
            string[] buttons,
            int seconds);

        // 上传文件到到 dp2lbrary 服务器
        // 注：已通过 prompt_func 实现通讯出错时候重试
        // parameters:
        //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
        //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
        // return:
        //		-1	出错
        //		0   上传文件成功
        public static int UploadFile(
            LibraryChannel channel,
            Stop stop,
            string strClientFilePath,
            string strServerFilePath,
            string strMetadata,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            bool bProgressChange,
            delegate_prompt prompt_func,
            out byte[] output_timestamp,
            out string strError)
        {
            strError = "";
            output_timestamp = null;

            string strResPath = strServerFilePath;

            // 只修改 metadata
            if (string.IsNullOrEmpty(strClientFilePath) == true)
            {
                long lRet = channel.SaveResObject(
stop,
strResPath,
null, // strClientFilePath,
-1, // 0, // 0 是 bug，会导致清除原有对象内容 2018/8/13
strMetadata,
"",
timestamp,
strStyle,
out output_timestamp,
out strError);
                timestamp = output_timestamp;
                if (lRet == -1)
                    return -1;
                return 0;
            }

            long skip_offset = 0;

            {
            REDO_DETECT:
                // 先检查以前是否有已经上传的局部
                long lRet = channel.GetRes(stop,
                    strServerFilePath,
                    0,
                    0,
                    "uploadedPartial",
                    out byte[] temp_content,
                    out string temp_metadata,
                    out string temp_outputPath,
                    out byte[] temp_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                    {
                        // 以前上传的局部不存在，说明只能从头上传
                        skip_offset = 0;
                    }
                    else
                    {
                        // 探测过程通讯或其他出错
                        if (prompt_func != null)
                        {
                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }

                            string action = prompt_func(channel,
                                strError + "\r\n\r\n(重试) 重试操作; (中断) 中断处理",
                                new string[] { "重试", "中断" },
                                10);
                            if (action == "重试")
                                goto REDO_DETECT;
                            if (action == "中断")
                                return -1;
                        }
                        return -1;
                    }
                }
                else if (lRet > 0)
                {
                    // *** 发现以前存在 lRet 这么长的已经上传部分
                    long local_file_length = FileUtil.GetFileLength(strClientFilePath);
                    // 本地文件尺寸居然小于已经上传的临时部分
                    if (local_file_length < lRet)
                    {
                        // 只能从头上传
                        skip_offset = 0;
                    }
                    else
                    {
                        // 询问是否断点续传
                        if (prompt_func != null)
                        {
                            string percent = StringUtil.GetPercentText(lRet, local_file_length);
                            string action = prompt_func(null,
                                $"本地文件 {strClientFilePath} 以前曾经上传过长度为 {lRet} 的部分内容(占整个文件 {percent})，请问现在是否继续上传余下部分? \r\n[是]从断点位置开始继续上传; [否]从头开始上传; [中断]取消这个文件的上传",
                                new string[] { "是", "否", "中断" },
                                0);
                            if (action == "是")
                                skip_offset = lRet;
                            else if (action == "否")
                                skip_offset = 0;
                            else
                            {
                                strError = "取消处理";
                                return -1;
                            }
                        }
                        // 如果 prompt_func 为 null 那就不做断点续传，效果是从头开始上传
                    }
                }
            }

            // 检测文件尺寸
            FileInfo fi = new FileInfo(strClientFilePath);
            if (fi.Exists == false)
            {
                strError = "文件 '" + strClientFilePath + "' 不存在...";
                return -1;
            }

            string[] ranges = null;

            if (fi.Length == 0)
            {
                // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = $"{skip_offset}-{(fi.Length - 1)}";

                // 按照100K作为一个chunk
                // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                ranges = RangeList.ChunkRange(strRange,
                    channel.UploadResChunkSize // 500 * 1024
                    );
                if (bProgressChange && stop != null)
                    stop.SetProgressRange(0, fi.Length);
            }

            if (timestamp == null)
                timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

            // byte[] output_timestamp = null;

            string strWarning = "";

            TimeSpan old_timeout = channel.Timeout;
            try
            {
                using (FileStream stream = File.OpenRead(strClientFilePath))
                {
                    for (int j = 0; j < ranges.Length; j++)
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string range = ranges[j];

                        string strWaiting = "";
                        if (j == ranges.Length - 1)
                        {
                            strWaiting = " 请耐心等待...";
                            channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟
                        }

                        string strPercent = "";
                        RangeList rl = new RangeList(range);
                        if (rl.Count != 1)
                        {
                            strError = $"{range} 中只应包含一个连续范围";
                            return -1;
                        }

                        {
                            double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                            strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                        }

                        if (stop != null)
                        {
                            stop.SetMessage( // strMessagePrefix + 
                                "正在上载 " + range + "/"
                                + StringUtil.GetLengthText(fi.Length)
                                + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
                            if (bProgressChange && rl.Count > 0)
                                stop.SetProgressValue(rl[0].lStart);
                        }

                        // 2019/6/23
                        StreamUtil.FastSeek(stream, rl[0].lStart);

                        int nRedoCount = 0;
                        long save_pos = stream.Position;
                    REDO:
                        // 2019/6/21
                        // 如果是重做，文件指针要回到合适位置
                        if (stream.Position != save_pos)
                            StreamUtil.FastSeek(stream, save_pos);

                        long lRet = channel.SaveResObject(
        stop,
        strResPath,
        stream, // strClientFilePath,
        -1,
        j == ranges.Length - 1 ? strMetadata : null,    // 最尾一次操作才写入 metadata
        range,
        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
        timestamp,
        strStyle,
        out output_timestamp,
        out strError);
                        //if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch)
                        //    strError = $"{strError}。timestamp={ByteArray.GetHexTimeStampString(timestamp)},output_timestamp={ByteArray.GetHexTimeStampString(output_timestamp)}。parsed:{ParseTimestamp(timestamp)};{ParseTimestamp(output_timestamp)}";

                        // Debug.WriteLine($"parsed:{ParseTimestamp(timestamp)};{ParseTimestamp(output_timestamp)}");

                        timestamp = output_timestamp;

                        strWarning = "";

                        if (lRet == -1)
                        {
                            // 如果是第一个 chunk，自动用返回的时间戳重试一次覆盖
                            if (bRetryOverwiteExisting == true
                                && j == 0
                                && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch
                                && nRedoCount == 0)
                            {
                                nRedoCount++;
                                goto REDO;
                            }

                            if (prompt_func != null)
                            {
                                if (stop != null && stop.State != 0)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }

                                string action = prompt_func(channel,
                                    strError + "\r\n\r\n(重试) 重试操作; (中断) 中断处理",
                                    new string[] { "重试", "中断" },
                                    10);
                                if (action == "重试")
                                    goto REDO;
                                if (action == "中断")
                                    return -1;
                            }
                            return -1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strError = "UploadFile() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                channel.Timeout = old_timeout;
            }

            return 0;
        }


        #endregion

        // 删除头像 object 的 dprms:file 元素
        // return:
        //      false   没有发生修改
        //      true    发生了修改
        static bool RemoveCardPhotoObject(XmlDocument readerdom,
string usage)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlElement file = readerdom.DocumentElement.SelectSingleNode($"//dprms:file[@usage='{usage}']", nsmgr) as XmlElement;
            if (file == null)
                return false;

            file.ParentNode.RemoveChild(file);
            return true;
        }

        static bool GetCardPhotoObjectPath(XmlDocument readerdom,
    string usage,
    string strRecPath,
    out string object_path)
        {
            object_path = "";

            bool changed = false;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlElement file = readerdom.DocumentElement.SelectSingleNode($"//dprms:file[@usage='{usage}']", nsmgr) as XmlElement;
            if (file == null)
            {

                file = readerdom.CreateElement("file", DpNs.dprms);
                readerdom.DocumentElement.AppendChild(file);
                file.SetAttribute("usage", "face");
                changed = true;
            }

            string strID = file.GetAttribute("id");
            if (string.IsNullOrEmpty(strID) == true)
            {
                // 寻找一个没有被用过的 id，创建一个新的 dprms:file 元素
                List<string> id_list = new List<string>();
                XmlNodeList ids = readerdom.DocumentElement.SelectNodes("//dprms:file/@id", nsmgr);
                foreach (XmlNode id in ids)
                {
                    id_list.Add(id.Value);
                }

                strID = GetNewID(id_list);
                file.SetAttribute("id", strID);
                changed = true;
            }

            string strResPath = strRecPath + "/object/" + strID;
            object_path = strResPath.Replace(":", "/");
            return changed;
        }

        // 获得一个没有用过的数字 ID
        public static string GetNewID(List<string> ids)
        {
            int nSeed = 0;
            string strID = "";
            for (; ; )
            {
                strID = Convert.ToString(nSeed++);
                if (ids.IndexOf(strID) == -1)
                    return strID;
            }
        }

        private void VideoRegister_Closed(object sender, EventArgs e)
        {
            FaceManager.CancelGetFeatureString();
            _stopVideo = true;
            RemoveLayer();
        }

        async Task<GetFeatureStringResult> GetFeatureStringAsync(string style)
        {
            EnableControls(false);
            try
            {
                return await Task.Run<GetFeatureStringResult>(() =>
                {
                    // 2019/9/6 增加
                    var result = FaceManager.GetState("camera");
                    if (result.Value == -1)
                        return new GetFeatureStringResult
                        {
                            Value = -1,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    return FaceManager.GetFeatureString(null, "", style);
                });
            }
            finally
            {
                EnableControls(true);
            }
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void DeleteFace_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            await RegisterFaceAsync("deleteFace");
        }

        void PatronClear(bool check_card_existance = false)
        {
            // 清除以前检查一下身份读卡器上是否有读者卡
            if (check_card_existance && TagList.Patrons.Count >= 1)
            {
                BeginWarningCard((s) =>
                {
                    // 延迟清除
                    if (s == "cancelled" && App.PatronReaderVertical)
                        PatronClear(false);
                });
                return;
            }

            _patron.Clear();

            if (this.patronControl.BorrowedEntities.Count > 0)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    App.Invoke(new Action(() =>
                    {
                        this.patronControl.BorrowedEntities.Clear();
                    }));
                else
                    this.patronControl.BorrowedEntities.Clear();
            }

            CancelDelayClearTask();
            // TODO: 这里要测试一下 CheckAccess()
            if (!Application.Current.Dispatcher.CheckAccess())
                App.Invoke(new Action(() =>
                {
                    PatronFixed = false;
                    fixPatron.IsEnabled = false;
                    clearPatron.IsEnabled = false;
                }));
            else
            {
                PatronFixed = false;
                fixPatron.IsEnabled = false;
                clearPatron.IsEnabled = false;
            }
        }


        private void FixPatron_Checked(object sender, RoutedEventArgs e)
        {
            CancelDelayClearTask();
        }

        private void ClearPatron_Click(object sender, RoutedEventArgs e)
        {
            CancelDelayClearTask();

            PatronClear();
        }

        #region 提醒拿走读者卡

        static DelayAction _delayNotifyCard = null;

        public static void CancelNotifyTask()
        {
            if (_delayNotifyCard != null)
            {
                _delayNotifyCard.Cancel.Cancel();
                _delayNotifyCard = null;
            }
        }

        public static void BeginNotifyTask()
        {
            CancelNotifyTask();

            /*
            Application.Current?.Dispatcher?.Invoke(new Action(() =>
            {
                PatronFixed = false;
            }));
            */
            _delayNotifyCard = DelayAction.Start(
                5,
                () =>
                {
                    // 延时到点后，如果读者卡确实还在，则提醒不要忘了拿走
                    if (TagList.Patrons.Count >= 1)
                    {
                        BeginWarningCard(null);
                    }
                },
                (seconds) =>
                {
                    // 可显示倒计时
                    /*
                    Application.Current?.Dispatcher?.Invoke(new Action(() =>
                    {
                    }));
                    */
                });
        }

        #endregion

        #region 延迟清除读者信息

        DelayAction _delayClearPatronTask = null;

        void CancelDelayClearTask()
        {
            if (_delayClearPatronTask != null)
            {
                _delayClearPatronTask.Cancel.Cancel();
                _delayClearPatronTask = null;
            }
        }

        void BeginDelayClearTask()
        {
            // 横向放置身份证读卡器时，没有必要延迟清除。意思就是说横向情况是需要人主动拿走卡，屏幕上信息才能清除
            if (App.PatronReaderVertical == false)
                return;

            CancelDelayClearTask();
            // TODO: 开始启动延时自动清除读者信息的过程。如果中途门被打开，则延时过程被取消(也就是说读者信息不再会被自动清除)

            App.Invoke(new Action(() =>
            {
                PatronFixed = false;
            }));
            _delayClearPatronTask = DelayAction.Start(
                20,
                () =>
                {
                    // TODO: 这里要看读者卡是否还留在了读卡器上。
                    // 如果读者卡还在读卡器上，如果决定要清除屏幕信息，则最好语音提示不要忘了拿走读者卡；
                    //          如果决定不清除读者信息也可以，意在让路过屏幕的人看到屏幕信息从而意识到读卡器上还遗留了读者卡。此时语音提示不要忘了拿走读者卡也是可以的，和保留屏幕信息不矛盾
                    // TODO: 语音提示是否要一直持续下去呢？直到有其他操作才中断语音提示
                    if (TagList.Patrons.Count >= 1)
                    {
                        BeginWarningCard((s) =>
                        {
                            // 延迟清除
                            // if (s == "cancelled" && App.PatronReaderVertical)
                            if (TagList.Patrons.Count == 0) // 2019/12/13
                                PatronClear(false);
                        });
                        // App.CurrentApp.Speak("注意，不要忘了拿走读者卡；注意，不要忘了拿走读者卡");
                    }
                    else
                        PatronClear();
                },
                (seconds) =>
                {
                    App.Invoke(new Action(() =>
                    {
                        if (seconds > 0)
                            this.clearPatron.Content = $"({seconds.ToString()} 秒后自动) 清除读者信息";
                        else
                            this.clearPatron.Content = $"清除读者信息";
                    }));
                });
        }

        static CancellationTokenSource _cancelSpeaking = new CancellationTokenSource();

        static void CancelWarning()
        {
            if (_cancelSpeaking != null)
            {
                _cancelSpeaking.Cancel();
                _cancelSpeaking = null;
            }
        }

        delegate void Delegate_cancelled(string condition);

        static void BeginWarningCard(Delegate_cancelled func_cancelled)
        {
            CancelWarning();

            _cancelSpeaking = new CancellationTokenSource();
            CancellationToken token = _cancelSpeaking.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        if (TagList.Patrons.Count == 0)
                        {
                            func_cancelled?.Invoke("patron_removed");
                            break;
                        }
                        if (TagList.Books.Count > 0)
                        {
                            func_cancelled?.Invoke("book_added");
                            break;
                        }
                        App.CurrentApp.Speak("注意，不要忘了拿走读者卡");
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(10), token);
                        }
                        catch
                        {
                            func_cancelled?.Invoke("cancelled");
                            break;
                        }
                    }
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
        }

        bool PatronFixed
        {
            get
            {
                return (bool)fixPatron.IsChecked;
            }
            set
            {
                fixPatron.IsChecked = value;
            }
        }

        // 开始启动延时自动清除读者信息的过程。如果中途放上去图书，则延时过程被取消(也就是说读者信息不再会被自动清除)
        void Welcome(bool errorOccur)
        {
            if (errorOccur)
            {
                // 身份读写器平放
                if (App.PatronReaderVertical == false)
                    BeginNotifyTask();

                if (App.PatronReaderVertical == true
    && TagList.Books.Count == 0)
                    BeginDelayClearTask();
                return;
            }
            App.Invoke(new Action(() =>
            {
                fixPatron.IsEnabled = true;
                clearPatron.IsEnabled = true;
            }));

            App.CurrentApp.Speak($"欢迎您，{(string.IsNullOrEmpty(_patron.PatronName) ? _patron.Barcode : _patron.PatronName)}");

            // 身份读写器平放
            if (App.PatronReaderVertical == false)
                BeginNotifyTask();

            // 身份读写器竖放
            // 读写器上没有图书的时候，才启动延时清除
            if (App.PatronReaderVertical == true
                && TagList.Books.Count == 0)
                BeginDelayClearTask();

            if (_commands != null && _commands.Count > 0)
            {
                string action = _commands[0];
                _commands.RemoveAt(0);

                CloseDialogs();
                _ = BindPatronCardAsync(action);
            }
        }

        #endregion

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void bindPatronCard_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            await BindPatronCardAsync("bindPatronCard");
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void releasePatronCard_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            await BindPatronCardAsync("releasePatronCard");
        }

        // TODO: 增加读者姓名等显示段落
        // 构造关于刷卡的提示信息文字段落
        static FlowDocument BuildScanText(
            string text,
            // string binding_list,
            XmlDocument patron_dom,
            double baseFontSize)
        {
            // 获得已经绑定的 UID 列表
            string binding_list = DomUtil.GetElementText(patron_dom.DocumentElement, "cardNumber");

            var ids = StringUtil.SplitList(binding_list);

            string name = DomUtil.GetElementText(patron_dom.DocumentElement, "name");
            string department = DomUtil.GetElementText(patron_dom.DocumentElement, "department");

            FlowDocument doc = new FlowDocument();

            {
                var p = new Paragraph();
                p.FontFamily = new FontFamily("微软雅黑");
                p.FontSize = baseFontSize;
                p.TextAlignment = TextAlignment.Left;
                p.Foreground = Brushes.Gray;
                // p.TextIndent = -20;
                p.Margin = new Thickness(0, 0, 0, 18);
                doc.Blocks.Add(p);

                p.Inlines.Add(new Run
                {
                    Text = text + "\r\n",
                    //Background = Brushes.DarkRed,
                    //Foreground = Brushes.White
                    // FontFamily = new FontFamily("楷体"),
                    FontSize = baseFontSize * 2.5,
                    // FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                });

                p.Inlines.Add(new Run
                {
                    Text = $"\r\n--- 主卡信息 ---\r\n姓名: {name}\r\n单位: {department}\r\n",
                    FontSize = baseFontSize,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.LightGray,
                });

                p.Inlines.Add(new Run
                {
                    Text = $"\r\n已绑定的 UID (共 {ids.Count} 个):\r\n",
                    FontSize = baseFontSize,
                    Foreground = Brushes.LightGray,
                });

                int i = 0;
                foreach (var id in ids)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = $"{(i + 1).ToString()}) {id}\r\n",
                        FontSize = baseFontSize,
                        Foreground = Brushes.LightGray,
                    });
                    i++;
                }
            }

            return doc;
        }

        // return.Value
        //      -1  出错
        //      0   放弃
        //      1   成功获得读者卡 UID，返回在 NormalResult.ErrorCode 中
        async Task<NormalResult> Get14443ACardUIDAsync(ProgressWindow progress,
            string action_caption,
            // string binding_list,
            XmlDocument patron_dom,
            CancellationToken token)
        {
            try
            {
                // TODO: 是否一开始要探测读卡器上是否有没有拿走的读者卡，提醒读者先拿走？
                while (token.IsCancellationRequested == false)
                {
                    if (TagList.Patrons.Count == 0)
                        break;

                    var tag = TagList.Patrons[0].OneTag;
                    if (tag.Protocol == InventoryInfo.ISO14443A)
                    {
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = "请先拿开读卡器上的主卡，操作才能继续 ...";
                        }));
                    }
                    else
                        break;

                    await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                }
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "放弃",
                        ErrorCode = ex.GetType().ToString()
                    };
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"Get14443ACardUIDAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }

            App.Invoke(new Action(() =>
            {
                var doc = BuildScanText(
                    $"现在，请扫要{action_caption}的副卡 ...",
                    patron_dom,
                    10);
                progress.MessageDocument = doc; // $"请扫要{action_caption}的读者卡 ...\r\n{text}";
            }));

            // 使用当前的处理事件
            // App.CurrentApp.TagChanged += tagChanged;
            try
            {
                while (token.IsCancellationRequested == false)
                {
                    if (TagList.Patrons.Count == 0)
                    {
                        App.Invoke(new Action(() =>
                        {
                            var doc = BuildScanText(
    $"现在，请扫要{action_caption}的副卡 ...",
    patron_dom,
    10);
                            progress.MessageDocument = doc; //$"请扫要{action_caption}的读者卡 ...\r\n{text}";
                        }));
                    }
                    if (TagList.Patrons.Count > 1)
                    {
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = "读卡器只应放一张副卡。请拿走多余的副卡";
                        }));
                    }

                    if (TagList.Patrons.Count == 1)
                    {
                        var tag = TagList.Patrons[0].OneTag;
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                        {
                            return new NormalResult
                            {
                                Value = 1,
                                ErrorCode = tag.UID
                            };
                        }
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                }
                return new NormalResult { Value = 0 };
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "放弃",
                        ErrorCode = ex.GetType().ToString()
                    };
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"Get14443ACardUIDAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
            finally
            {
                // App.CurrentApp.TagChanged -= tagChanged;
            }

            /*
            void tagChanged(object sender, TagChangedEventArgs e)
            {

            }
            */
        }

        List<string> _commands = new List<string>();

        int _skipTagChanged = 0;

        // 绑定或者解绑(ISO14443A)读者卡
        private async Task BindPatronCardAsync(string action)
        {
            // GetFeatureStringResult result = null;
            _commands = null;

            string action_name = "绑定";
            if (action == "releasePatronCard")
                action_name = "解绑";

            // 提前打开对话框
            ProgressWindow progress = null;
            CancellationTokenSource cancel = new CancellationTokenSource();

            App.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.TitleText = action_name;
                progress.MessageText = "请扫要绑定的副卡 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                App.SetSize(progress, "middle");
                progress.OkButtonText = "取消";
                progress.Closed += (o, e) =>
                {
                    cancel.Cancel();
                    RemoveLayer();
                    if (_commands != null)
                        _commands.Clear();
                };
                progress.Show();
                AddLayer();
            }));

            // 如果正在倒计时
            bool fixed_changed = false;
            App.Invoke(new Action(() =>
            {
                if (this.PatronFixed == false)
                {
                    _commands = null;
                    CancelDelayClearTask();
                    fixed_changed = true;
                }
            }));

            string patron_uid = null;
            string patron_pii = null;

            // 暂时断开原来的标签处理事件
            // App.CurrentApp.TagChanged -= CurrentApp_TagChanged;
            _skipTagChanged++;
            try
            {
                if (IsPatronOK(action, out string check_message) == false)
                {
                    if (string.IsNullOrEmpty(check_message))
                        check_message = $"读卡器上的当前读者卡状态不正确。无法进行{action_name}副卡的操作";

                    DisplayError(ref progress, check_message, "black");

                    // 放入一个命令，等待扫鉴别身份的读者卡，扫卡时候直接触发重新进入本函数
                    if (_commands == null)
                        _commands = new List<string>();
                    _commands.Add(action);
                    return;
                }

                /*
                if (_patron.IsRfidSource && _patron.Protocol == InventoryInfo.ISO14443A)
                {

                }
                // TODO: 检查一下，如果此时读卡器上已经放了一张 14443A 的卡，但此时对话框并未提示第二步操作，
                // 因此这里应该判断为，是第一步刷卡滞留的卡。要怎么处理呢？第一个方法是，延迟几秒钟再提示第二步；第二个方法是，第二步判断的时候，要等待刷一张除了这张卡的其他卡
                */

                /*
                try
                {
                    while (IsPatronOK(action, out string check_message) == false)
                    {
                        if (string.IsNullOrEmpty(check_message))
                            check_message = $"读卡器上的当前读者卡状态不正确。无法进行{action_name}副卡的操作";

                        // DisplayError(ref progress, check_message, "yellow");
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = check_message;
                            progress.BackColor = "yellow";
                        }));
                        if (cancel.Token.IsCancellationRequested)
                        {
                            progress = null;
                            return;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                    }
                }
                catch
                {
                    progress = null;
                    return;
                }
                */

                patron_uid = _patron.UID;
                patron_pii = _patron.PII;
                string patron_xml = _patron.Xml;
                var patron_timestamp = _patron.Timestamp;

                bool changed = false;
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(_patron.Xml);
                }
                catch (Exception ex)
                {
                    DisplayError(ref progress, $"BindPatronCardAsync() 异常，读者记录 (证条码号:{_patron.Barcode},读者记录路径:{_patron.RecPath}) XML 装载到 XmlDocument 失败: {ex.Message}",
                        "red", "关闭");
                    return;
                }

                string patron_name = DomUtil.GetElementText(dom.DocumentElement, "name");

                // TODO: 弹出一个对话框，检测 ISO14443A 读者卡
                // 注意探测读者卡的时候，不是要刷新右侧的读者信息，而是把探测到的信息拦截到对话框里面，右侧的读者信息不要有任何变化
                var result = await Get14443ACardUIDAsync(progress,
                    action_name,
                    dom,
                    cancel.Token);
                if (result.Value == -1)
                {
                    DisplayError(ref progress, result.ErrorInfo, "red", "关闭");
                    return;
                }

                if (result.Value == 0)
                    return;

                string bind_uid = result.ErrorCode;

                App.Invoke(new Action(() =>
                {
                    progress.MessageText = "正在修改读者记录 ...";
                }));

                if (action == "bindPatronCard")
                {
                    // 修改读者 XML 记录中的 cardNumber 元素
                    var modify_result = ModifyBinding(dom,
    "bind",
    bind_uid);
                    if (modify_result.Value == -1)
                    {
                        DisplayError(ref progress, $"绑定失败: {modify_result.ErrorInfo}", "red", "关闭");
                        return;
                    }
                    changed = true;
                }
                else if (action == "releasePatronCard")
                {
                    // 从读者记录的 cardNumber 元素中移走指定的 UID
                    var modify_result = ModifyBinding(dom,
"release",
bind_uid);
                    if (modify_result.Value == -1)
                    {
                        DisplayError(ref progress, $"解除绑定失败: {modify_result.ErrorInfo}", "red", "关闭");
                        return;
                    }

                    /*
                    // TODO: 用 WPF 对话框
                    // 有时候这个对话框可能会被翻到后面。需要改用 WPF 的无模式对话框
                    MessageBoxResult dialog_result = MessageBox.Show(
    $"确实要解除对副卡 {uid} 的绑定?\r\n\r\n(解除绑定以后，您将无法使用这一张副卡进行借书还书操作)",
    "dp2SSL",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question);
                    if (dialog_result == MessageBoxResult.No)
                        return;
                    */

                    string dialog_result = "";
                    App.Invoke(new Action(() =>
                    {
                        var ask = new ProgressWindow();
                        ask.TitleText = action_name;
                        ask.MessageText = $"确实要解除读者 {patron_name} 副卡 {bind_uid} 的绑定?\r\n\r\n(解除绑定以后，读者将无法再用这一张副卡对书柜进行任何操作)";
                        ask.Owner = Application.Current.MainWindow;
                        ask.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        App.SetSize(ask, "wide");
                        ask.OkButtonText = "是";
                        ask.CancelButtonVisible = true;
                        ask.ShowDialog();
                        dialog_result = ask.PressedButton;
                    }));

                    if (dialog_result != "OK")
                        return;

                    changed = true;
                }

                if (changed == true)
                {
                    // 保存读者记录
                    var save_result = await SetReaderInfoAsync(_patron.RecPath,
                        dom.OuterXml,
                        patron_xml, // _patron.Xml,
                        patron_timestamp);
                    if (save_result.Value == -1)
                    {
                        DisplayError(ref progress, save_result.ErrorInfo, "red", "关闭");
                        return;
                    }

                    _patron.Timestamp = save_result.NewTimestamp;
                    _patron.Xml = dom.OuterXml;
                }

                // TODO: “别忘了拿走读者卡”应该在读者读卡器竖放时候才有必要提示
                string message = $"读者 {patron_name} {action_name}副卡 {bind_uid} 成功";
                if (action == "releasePatronCard")
                    App.CurrentApp.Speak(message);

                DisplayError(ref progress, message, "green", "关闭");
            }
            finally
            {
                // App.CurrentApp.TagChanged += CurrentApp_TagChanged;
                _skipTagChanged--;

                if (progress != null)
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));

                if (fixed_changed)
                    BeginDelayClearTask();
            }

            /*
            // 刷新读者信息区显示
            // TODO: 是否干脆清除读者信息区显示?
            if (action == "bindPatronCard")
            {
                _patron.PII = patron_pii;
                _patron.UID = patron_uid;
                _ = FillPatronDetailAsync(true);
            }
            else
            */
            {
                CancelDelayClearTask();
                PatronClear();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static NormalResult ModifyBinding(XmlDocument dom,
            string action,
            string uid)
        {
            uid = uid.ToUpper();

            string patron_name = DomUtil.GetElementText(dom.DocumentElement, "name");

            string value = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
            if (value != null)
                value = value.ToUpper();
            if (action == "release")
            {
                if (StringUtil.IsInList(uid, value) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者 {patron_name} 的记录中并不存在 UID 为 {uid} 的绑定信息"
                    };
            }
            if (action == "bind")
            {
                if (StringUtil.IsInList(uid, value) == true)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者 {patron_name} 的记录中已经存在 UID 为 {uid} 的绑定信息"
                    };
            }

            StringUtil.SetInList(ref value, uid, action == "bind");
            DomUtil.SetElementText(dom.DocumentElement, "cardNumber", value);
            return new NormalResult();
        }
    }
}
