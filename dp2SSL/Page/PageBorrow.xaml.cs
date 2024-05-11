﻿using System;
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
using System.Text;
using System.Linq;

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
    public partial class PageBorrow : MyPage, INotifyPropertyChanged
    {
        SmoothTable _smoothTable = new SmoothTable();
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

        volatile bool _stopVideo = false;

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
                videoRecognition.Closed += (s1, e1) =>
                {
                    FaceManager.CancelRecognitionFace();
                    _stopVideo = true;
                    RemoveLayer();
                    App.ContinueBarcodeScan();
                };  // VideoRecognition_Closed;
                videoRecognition.Show();
                // 2023/12/20
                AddLayer(); // Closed 事件会 RemoveLayer()
                App.PauseBarcodeScan(); // 允许键盘 Escape Enter 键
            }));
            _stopVideo = false;
            var task = Task.Run(() =>
            {
                try
                {
                    DisplayVideo(videoRecognition, TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    WpfClientInfo.WriteErrorLog($"(PageBorrow) DisplayVideo() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
                finally
                {
                    // 2020/9/10
                    if (videoRecognition != null)
                    {
                        App.Invoke(new Action(() =>
                        {
                            videoRecognition.Close();
                        }));
                        App.CurrentApp.SpeakSequence($"放弃人脸识别");
                    }
                }
            });
            try
            {
                result = await RecognitionFaceAsync(App.FaceInputMultipleHits == "使用第一个" ? "" : "multiple_hits");
                if (result.Value == -1)
                {
                    if (result.ErrorCode != "cancelled")
                        SetGlobalError("faceInput", result.ErrorInfo);
                    DisplayError(ref videoRecognition, result.ErrorInfo);
                    return;
                }

                SetGlobalError("faceInput", null);
            }
            finally
            {
                if (videoRecognition != null)
                    App.Invoke(new Action(() =>
                    {
                        videoRecognition.Close();
                        videoRecognition = null;
                    }));
            }

            /*
            {
                List<RecognitionFaceHit> samples = new List<RecognitionFaceHit>();
                for (int i = 0; i < 10; i++)
                {
                    samples.Add(new RecognitionFaceHit { Patron = "R" + (i + 1).ToString().PadLeft(7, '0') });
                }

                result.Hits = samples.ToArray();
            }
            */

            // TODO: 从命中多个中选择。输入 PIN 码或者用鼠标选择
            // 命中多个的选择
            if (App.FaceInputMultipleHits != "使用第一个"
                && result.Hits != null
                && result.Hits.Length > 1)
            {
                var barcodes = result.Hits.OrderByDescending(o => o.Score).Select(o => o.Patron).ToList();

                SelectPatronWindow.CleanAttackManager();
                if (SelectPatronWindow.HasAttacked(barcodes) == true)
                {
                    App.CurrentApp.Speak($"人脸信息被保护 ...");

                    result = new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = $"为防范密码攻击，您的人脸信息处于被保护状态，请{SelectPatronWindow.CleanLength.TotalMinutes}分钟后再重试 ..."
                    };
                }
                else
                {
                    App.CurrentApp.Speak($"人脸识别命中 {result.Hits.Length} 个读者，请选择 ...");

                    List<string> names = new List<string>();
                    if (App.FaceInputMultipleHits.Contains("列表选择"))
                        names.Add("从列表中选择");
                    if (App.FaceInputMultipleHits.Contains("密码筛选"))
                        names.Add("输入密码筛选");

                    string title = $"人脸识别命中 {barcodes.Count} 个读者，请{StringUtil.MakePathList(names, " 或 ")} ...";

                    var select_result = SelectOnePatron(
                        this,
                        title,
                        barcodes);
                    if (select_result.Value != 1)
                        result = new RecognitionFaceResult
                        {
                            Value = -1,
                            ErrorInfo = select_result.ErrorInfo,
                            ErrorCode = select_result.ErrorCode,
                        };
                    else
                        result = new RecognitionFaceResult
                        {
                            Value = 1,
                            Score = result.Hits
                            .Where(o => o.Patron == select_result.strBarcode)
                            .Select(o => o.Score)
                            .FirstOrDefault(),
                            Patron = select_result.strBarcode
                        };
                }
            }

            if (result.Value == -1)
            {
                if (result.ErrorCode != "cancelled")
                {
                    // SetGlobalError("faceInput", result.ErrorInfo);
                    _ = Task.Run(() =>
                    {
                        App.ErrorBox(
    "人脸识别",
    result.ErrorInfo,
    "red",
    "auto_close:10");
                    });
                }
                return;
            }

            GetMessageResult message = new GetMessageResult
            {
                Value = 1,
                Message = result.Patron,
            };
            if (SetPatronInfo(message) == false)
                return;
            SetQuality("");
            var fill_result = await FillPatronDetailAsync();
            Welcome(fill_result.Value == -1);
        }

        public class SelectOnePatronResult : NormalResult
        {
            public string strBarcode { get; set; }
            public string strResult { get; set; }
        }

        public static SelectOnePatronResult SelectOnePatron(
            MyPage owner,
            string dialog_title,
            IEnumerable<string> barcode_list)
        {
            string dialog_result = "";
            Patron selected_patron = null;
            App.Invoke(new Action(() =>
            {
                App.PauseBarcodeScan();
                try
                {
                    var ask = new SelectPatronWindow();
                    ask.Closed += (s1, e1) =>
                    {
                        owner.RemoveLayer();
                        owner.ForgetDialog(ask);
                    };
                    owner.AddLayer(); // Closed 事件会 RemoveLayer()
                    owner.MemoryDialog(ask);

                    PatronCollection patrons = new PatronCollection();
                    foreach (var barcode in barcode_list)
                    {
                        patrons.Add(new Patron
                        {
                            PII = barcode,
                            IsFingerprintSource = true,
                            BorrowItemsVisible = false,
                        });
                    }
                    ask.SetSource(patrons);

#if REMOVED
                    if (App.FaceInputMultipleHits.Contains("列表选择") == false)
                    {
                        ask.listView.Visibility = Visibility.Collapsed;

                        if (App.FaceInputMultipleHits.Contains("+") == false)
                        {
                            (ask.mainGrid.ColumnDefinitions[0] as ColumnDefinitionExtended).Visible = false;
                            ask.mainGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                        }
                    }
                    if (App.FaceInputMultipleHits.Contains("密码筛选") == false)
                    {
                        ask.passwordArea.Visibility = Visibility.Collapsed;

                        if (App.FaceInputMultipleHits.Contains("+") == false)
                        {
                            (ask.mainGrid.ColumnDefinitions[1] as ColumnDefinitionExtended).Visible = false;
                        }
                    }
#endif

                    ask.TitleText = dialog_title;
                    //ask.MessageText = $"确实要解除读者 {patron_name} 副卡 {bind_uid} 的绑定?\r\n\r\n(解除绑定以后，读者将无法再用这一张副卡进行任何操作)";
                    ask.Owner = Application.Current.MainWindow;
                    ask.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    App.SetSize(ask, "wide");
                    //ask.OkButtonText = "是";
                    //ask.CancelButtonVisible = true;
                    ask.ShowDialog();

                    // owner.ForgetDialog(ask);

                    dialog_result = ask.PressedButton;
                    selected_patron = ask.SelectedPatron;
                }
                finally
                {
                    App.ContinueBarcodeScan();
                }
            }));

            if (dialog_result == "cancel"
                || selected_patron == null)
                return new SelectOnePatronResult
                {
                    Value = 0,
                    ErrorInfo = "取消选择",
                    ErrorCode = "cancelled"
                };

            return new SelectOnePatronResult
            {
                Value = 1,
                strBarcode = selected_patron?.Barcode,
            };
        }

        void DisplayVideo(VideoWindow window, TimeSpan timeout)
        {
            DateTime lastResetTime = DateTime.Now;
            DateTime start = DateTime.Now;
            while (_stopVideo == false)
            {
                if (DateTime.Now - start > timeout)
                    break;

                if (DateTime.Now - lastResetTime > TimeSpan.FromSeconds(2))
                {
                    // 2021/1/23
                    // 重置活跃时钟，避免中途自动返回菜单页面
                    PageMenu.MenuPage.ResetActivityTimer();
                    lastResetTime = DateTime.Now;
                }

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

        /*
        private void VideoRecognition_Closed(object sender, EventArgs e)
        {
            FaceManager.CancelRecognitionFace();
            _stopVideo = true;
            RemoveLayer();
        }
        */

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
                    return FaceManager.RecognitionFace(style);
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

            //FingerprintManager.SetError += FingerprintManager_SetError;
            FingerprintManager.Touched += FingerprintManager_Touched;

            App.LineFeed += App_LineFeed;

            // 2020/7/5
            App.InitialRfidManager();

            RfidManager.ClearCache();
            // 处理以前积累的 tags
            // RfidManager.TriggerLastListTags();

            // 注：将来也许可以通过(RFID 以外的)其他方式输入图书号码
            if (string.IsNullOrEmpty(RfidManager.Url))
                SetGlobalError("rfid", "尚未配置 RFID 中心 URL");

            /*
            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);
            */
            InitializeLayer(this.mainGrid);

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

#if NO
            App.Invoke(new Action(() =>
            {
                // 身份读卡器竖向放置，才有固定读者信息的必要
                if (IsVerticalCard()/*App.PatronReaderVertical*/)
                {
                    // TODO: 这里可以提供一个定制特性的点位，让用户自定义是否出现固定按钮
                    fixAndClear.Visibility = Visibility.Visible;
                }
                else
                    fixAndClear.Visibility = Visibility.Collapsed;
            }));
#endif

            // SetGlobalError("test", "test error");

            ////
            App.TagChanged += CurrentApp_TagChanged;
            _ = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        _tagChangedCount = 0;
                        await InitialEntitiesAsync();
                        if (_tagChangedCount == 0)
                            break;  // 只有当初始化过程中没有被 TagChanged 事件打扰过，才算初始化成功了。否则就要重新初始化
                    }
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"InitialEntitiesAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });

            if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
            {
                _cancelFingerprintVideo?.Cancel();
                _cancelFingerprintVideo = new CancellationTokenSource();
                this.fingerprintVideo.StartDisplayFingerprint(_cancelFingerprintVideo.Token);
                /*
                StartDisplayFingerprint(this.photo,
                    this.lines,
                    _cancelFingerprintVideo.Token);
                */
            }
            else
                this.fingerprintVideo.Hide();
        }

        CancellationTokenSource _cancelFingerprintVideo = new CancellationTokenSource();

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void App_LineFeed(object sender, LineFeedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            // 扫入一个条码
            string barcode = e.Text.ToUpper();

            // 触发人脸识别
            if (string.IsNullOrEmpty(barcode))
            {
                if (StringUtil.IsInList("face", GetPageStyleList()) == true)
                {
                    PatronClear();
                    PatronControl_InputFace(this, new EventArgs());
                    return;
                }
                else
                {
                    App.CurrentApp.Speak("不允许人脸识别");
                    return;
                }
            }

            // if (App.EnablePatronBarcode == false)
            if (string.IsNullOrEmpty(App.PatronBarcodeStyle) || App.PatronBarcodeStyle == "禁用")
            {
                SetGlobalError("scan_barcode", "当前设置参数不接受扫入条码");
                App.CurrentApp.Speak("不允许扫入各种条码");
                return;
            }

            // 检查防范空字符串，和使用工作人员方式(~开头)的字符串
            if (string.IsNullOrEmpty(barcode) || barcode.StartsWith("~"))
            {
                App.CurrentApp.Speak("条码不合法");
                return;
            }

            // 2020/6/3
            var styles = StringUtil.SplitList(App.PatronBarcodeStyle, "+");
            if (barcode.StartsWith("PQR:"))
            {
                // 二维码情形
                if (styles.IndexOf("二维码") == -1)
                {
                    App.CurrentApp.Speak("不允许扫入二维码");
                    return;
                }
            }
            else
            {
                // 一维码情形
                if (styles.IndexOf("一维码") == -1)
                {
                    App.CurrentApp.Speak("不允许扫入条码");
                    return;
                }
            }

            SetGlobalError("scan_barcode", null);

            if (SetPatronInfo(new GetMessageResult { Message = barcode }) == false)
                return;

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

            // 重置活跃时钟
            PageMenu.MenuPage.ResetActivityTimer();

            await ChangeEntitiesAsync(/*(BaseChannel<IRfid>)sender,*/ e);
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

        CancellationTokenSource _cancelFillBooks = null;
        CancellationToken CancelFillBooks(bool new_token)
        {
            {
                _cancelFillBooks?.Cancel();
                _cancelFillBooks = null;
            }

            if (new_token)
            {
                _cancelFillBooks = new CancellationTokenSource();
                return _cancelFillBooks.Token;
            }

            return default;
        }

        // 2024/3/13
        // 获得一个 Cancel Token。如果以前用过，直接返回以前的
        CancellationToken GetCancelFillBooks()
        {
            if (_cancelFillBooks == null)
                _cancelFillBooks = new CancellationTokenSource();
            return _cancelFillBooks.Token;
        }

        SemaphoreSlim _semaphoreCheckDup = new SemaphoreSlim(1, 1);
        List<ProgressWindow> _warning_windows = new List<ProgressWindow>();
        void CloseWarningWindows()
        {
            foreach (var window in _warning_windows)
            {
                App.Invoke(() =>
                {
                    window.Close();
                });
            }
            _warning_windows.Clear();
        }

        void MemoryWarningWindow(ProgressWindow window)
        {

            _warning_windows.Add(window);
        }

        void BeginCheckDup()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await _semaphoreCheckDup.WaitAsync();
                try
                {
                    var warning = CheckUiiDup(_entities);
                    if (warning != null)
                    {
                        WpfClientInfo.WriteErrorLog($"PageBorrow.InitialEntitiesAsync() 查重 UII 遇到错误: {warning}");
                        ShelfData.TrySetMessage(null, $"PageBorrow.InitialEntitiesAsync() 查重 UII 遇到错误: {warning}");

                        CloseWarningWindows();
                        var window = App.ErrorBox("发现错误: 图书 UII 出现重复", warning + "\r\n\r\n请尽快将这些图书交给图书馆工作人员处理");
                        MemoryWarningWindow(window);
                    }
                }
                finally
                {
                    _semaphoreCheckDup.Release();
                }
            });
        }

        // 跟随事件动态更新列表
        async Task ChangeEntitiesAsync(//BaseChannel<IRfid> channel,
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

            var addbooks = new List<TagAndData>(e.AddBooks == null ? new List<TagAndData>() : e.AddBooks);
            var removebooks = new List<TagAndData>(e.RemoveBooks == null ? new List<TagAndData>() : e.RemoveBooks);
            // 将 AddBooks 和 RemoveBooks 中的 UHF EPC 改变，但 UII 没有改变的标签分离出来，单独处理
            var epc_changed_books = RfidTagList.DetectEpcChange(ref addbooks, ref removebooks);


            bool changed = false;
            List<Entity> update_entities = new List<Entity>();
            App.Invoke(new Action(() =>
            {
                if (e.AddBooks != null)
                    foreach (var tag in addbooks/*e.AddBooks*/)
                    {
                        var entity = _entities.Add(tag);
                        update_entities.Add(entity);
                    }
                if (e.RemoveBooks != null)
                    foreach (var tag in removebooks/*e.RemoveBooks*/)
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

                foreach (var item in epc_changed_books)
                {
                    var entity = _entities.Update(item.OldData, item.NewData, true);
                    if (entity != null)
                        update_entities.Add(entity);
                }
            }));

            // 2024/1/19
            // 修正: 当 ISO15693 的标签被发现实际上是读者标签时，意味着并没有发生实质性变化，changed 应该修正为 false
            if (e.AddPatrons.Count == 1
                && e.RemoveBooks.Count == 1
                && e.AddPatrons[0].OneTag?.UID == e.RemoveBooks[0].OneTag?.UID)
                changed = false;

            if (update_entities.Count > 0)
            {
                _ = FillBookFieldsAsync(//channel,
                    update_entities,
                    App.DisplayCoverImage ? "coverImage,checkEAS" : "checkEAS",
                    GetCancelFillBooks());

                BeginCheckDup();
                Trigger(update_entities);

#if REMOVED
                List<Entity> temp = new List<Entity>(update_entities);
                _ = Task.Run(() =>
                {
                    // 自动检查 EAS 状态
                    CheckEAS(temp);
                });
#endif
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
        && IsVerticalCard()/*(_patron.IsFingerprintSource || App.PatronReaderVertical == true)*/)
                {
                    PatronClear(true);
                }

                // 2019/7/1
                // 当读卡器上的图书全部拿走时候，自动关闭残留的模式对话框
                if (_entities.Count == 0
    && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
    )
                {
                    CloseDialogs();

                    // 自动返回菜单页面
                    if (App.AutoBackMenuPage
                        && (IsVerticalCard()/*_patron.IsFingerprintSource || App.PatronReaderVertical == true*/ || RfidTagList.Patrons.Count == 0))
                        TryBackMenuPage();
                }

                // 当图书全部被移走时，如果身份读卡器横向放置，需要延时提醒不要忘记拿走读者卡
                if (_entities.Count == 0 && changed == true
                    && IsVerticalCard()/*App.PatronReaderVertical*/ == false)
                    BeginNotifyTask();

                // 当列表为空的时候，主动清空一次 tag 缓存。这样读者可以用拿走全部标签一次的方法来迫使清除缓存(比如中途利用内务修改过 RFID 标签的 EAS)
                // TODO: 不过此举对反复拿放图书的响应速度有一定影响。后面也可以考虑继续改进(比如设立一个专门的清除缓存按钮，这里就不要清除缓存了)
                if (_entities.Count == 0 && RfidTagList.TagTableCount > 0)
                    RfidTagList.ClearTagTable(null);

                // 2023/12/14
                if (_entities.Count == 0)
                    CancelFillBooks(false);
            }
        }

        static bool IsCompleted(Entity entity)
        {
            if (entity.Protocol == InventoryInfo.ISO15693
                && (entity.TagInfo == null || entity.TagInfo?.Bytes == null))
                return false;
            if (entity.Protocol == InventoryInfo.ISO14443A)
                return false;
            if (entity.Protocol == InventoryInfo.ISO18000P6C
                && entity.TagInfo == null)
                return false;
            return true;
        }

        public static string CheckUiiDup(IReadOnlyCollection<Entity> all)
        {
            // 对 all 里面的 UII 进行查重
            var uiis = all
                .Where(o => IsCompleted(o) == true)
                .Select(o => new { UII = o.GetOiPii(), Entity = o })
                .ToList();

            var dups = uiis.GroupBy(o => o.UII)
                .Select(o => new
                {
                    Key = o.Key,
                    Entities = o.Select(i => i.Entity).ToList(),
                    Count = o.Count()
                })
                .Where(o => o.Count > 1)
                .ToList();

            if (dups.Count > 0)
            {
                List<string> infos = new List<string>();
                foreach (var dup in dups)
                {
                    infos.Add($"{dup.Key}:{GetLocations(dup.Entities)}");
                }
                return $"下列图书标签的 UII 发生了重复: {StringUtil.MakePathList(infos, "; ")}";
            }

            return null;

            string GetLocations(IEnumerable<Entity> entities)
            {
                List<string> results = new List<string>();
                foreach (var entity in entities)
                {
                    results.Add(GetLocation(entity));
                }
                return StringUtil.MakePathList(results, ",");
            }

            string GetLocation(Entity entity)
            {
                return $"UID={entity.UID},读写器={entity.ReaderName}|天线号={entity.Antenna}";
            }
        }


        // 是否为“图书读卡器上没有图书、读者信息区为空”
        public bool IsEmpty()
        {
            if (_entities.Count > 0)
                return false;
            if (_patron.NotEmpty == true)
                return false;

            return true;
        }

        // 首次初始化 Entity 列表
        async Task<NormalResult> InitialEntitiesAsync()
        {
            App.Invoke(new Action(() =>
            {
                _entities.Clear();  // 2019/9/4
            }));

            if (booksControl.Visibility == Visibility.Visible)  // 2020/4/8
            {
                var books = RfidTagList.Books;
                if (books.Count > 0)
                {
                    List<Entity> update_entities = new List<Entity>();

                    foreach (var tag in books)
                    {
                        App.Invoke(new Action(() =>
                        {
                            var entity = _entities.Add(tag);
                            update_entities.Add(entity);
                        }));
                    }

                    if (update_entities.Count > 0)
                    {
                        try
                        {
                            //BaseChannel<IRfid> channel = RfidManager.GetChannel();
                            try
                            {
                                _ = FillBookFieldsAsync(//channel,
                                    update_entities,
                                    App.DisplayCoverImage ? "coverImage,checkEAS" : "checkEAS",
                                    GetCancelFillBooks());

                                BeginCheckDup();
                                Trigger(update_entities);
                            }
                            finally
                            {
                                //RfidManager.ReturnChannel(channel);
                            }
                        }
                        catch (Exception ex)
                        {
                            WpfClientInfo.WriteErrorLog($"InitialEntitiesAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            string error = $"填充图书信息时出现异常: {ex.Message}";
                            SetGlobalError("rfid", error);
                            return new NormalResult { Value = -1, ErrorInfo = error };
                        }

#if REMOVED
                        // 自动检查 EAS 状态
                        List<Entity> temp = new List<Entity>(update_entities);
                        _ = Task.Run(() =>
                        {
                            CheckEAS(temp);
                        });
#endif
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

        static int _prev_patron_count = 0;
        // ReaderWriterLockSlim _lock_refreshPatrons = new ReaderWriterLockSlim();

        // TODO: 要和以前比对，读者信息是否发生了变化。如果没有变化就不要刷新界面了
        async Task RefreshPatronsAsync()
        {
            //_lock_refreshPatrons.EnterWriteLock();
            try
            {
                var patrons = RfidTagList.Patrons;
                if (patrons.Count == 1)
                    _patron.IsRfidSource = true;

                if (_patron.IsFingerprintSource)
                {
                    // 指纹仪来源
                }
                else
                {
                    // SetQuality("");
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
                        if (IsVerticalCard()/*App.PatronReaderVertical*/ == false)
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
                            if (IsVerticalCard()/*App.PatronReaderVertical*/ == false)
                            {
                                PatronClear();
                                // 自动返回菜单页面
                                if (App.AutoBackMenuPage && _entities.Count == 0 && _prev_patron_count > 0)
                                    TryBackMenuPage();
                            }

                            SetPatronError("rfid_multi", "");   // 2019/5/20
                        }
                    }
                }

                if (patrons.Count == 0)
                    CancelWarning();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"RefreshPattronsAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                SetGlobalError("rfid", $"RefreshPatronsAsync() 出现异常: {ex.Message}");
            }
            finally
            {
                //_lock_refreshPatrons.ExitWriteLock();
                _prev_patron_count = RfidTagList.Patrons.Count;
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
            {
                SetGlobalError("fingerprint", $"{FingerprintManager.Name}: {e.Error}");  // 2019/9/11 增加 fingerprinterror:
            }
        }

        void SetQuality(string text)
        {
#if REMOVED
            App.Invoke(new Action(() =>
            {
                this.Quality.Text = text;
            }));
#endif
            this.fingerprintVideo.SetQuality(text);
        }

        // 从指纹阅读器获取消息(第一阶段)
#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void FingerprintManager_Touched(object sender, TouchedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            // 注如果 FingerprintManager 已经挂接 SetError 事件，Touched 事件这里就可以忽略 result.Value == -1 情况
            if (e.Result.Value == -1)
                return;

            // 2024/1/20
            // 忽略图像消息
            if (e.Result.Message != null
                && e.Result.Message.StartsWith("!image"))
                return;

            SetQuality(e.Quality <= 0 ? "" : e.Quality.ToString());

            if (SetPatronInfo(e.Result) == false)
                return;

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


        private void PageBorrow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.IsPageBorrowActive = false;

            CancelFillBooks(false);

            _cancelFingerprintVideo?.Cancel();
            _cancelFingerprintVideo = null;

            // _cancel.Cancel();
            CancelDelayClearTask();

            // 释放 Loaded 里面分配的资源
            // RfidManager.SetError -= RfidManager_SetError;
            App.TagChanged -= CurrentApp_TagChanged;

            FingerprintManager.Touched -= FingerprintManager_Touched;
            //FingerprintManager.SetError -= FingerprintManager_SetError;

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

        // _patron.PhotoPath
        public static void LoadPhoto(PatronControl patronControl,
            string photoPath)
        {
            try
            {
                if (App.Protocol == "dp2library")
                {
                    // Exception: 可能会抛出异常
                    patronControl.LoadPhoto(photoPath);
                }

                if (App.Protocol == "sip" && string.IsNullOrEmpty(App.FaceUrl) == false)
                {
                    if (string.IsNullOrEmpty(photoPath))
                        App.Invoke(new Action(() =>
                        {
                            patronControl.SetPhoto((byte[])null);
                        }));
                    else
                    {
                        // 从 FaceCenter 获得头像
                        var result = FaceManager.GetImage($"patron:{photoPath}");
                        // TODO: 如何报错?
                        App.Invoke(new Action(() =>
                        {
                            patronControl.SetPhoto(result.ImageData);
                        }));
                    }
                }

                // 2023/12/20
                SetGlobalError("patron", null);
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"LoadPhoto() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                SetGlobalError("patron", $"patron exception: {ex.Message}");    // 2019/9/11 增加 patron exception:
            }
        }

        bool _visiblityChanged = false;

        private void _patron_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoPath")
            {
                _ = Task.Run(() =>
                {
                    LoadPhoto(this.patronControl, this._patron.PhotoPath);
                    /*
                    try
                    {
                        this.patronControl.LoadPhoto(_patron.PhotoPath);
                    }
                    catch (Exception ex)
                    {
                        SetGlobalError("patron", $"patron exception: {ex.Message}");    // 2019/9/11 增加 patron exception:
                    }
                    */
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
            // 只要有借书和注册、绑定读者，读者区就需要显示出来
            if (borrowButton.Visibility == Visibility.Visible
                || registerFace.Visibility == Visibility.Visible
                || bindPatronCard.Visibility == Visibility.Visible)
                this.patronControl.Visibility = Visibility.Visible;
            else
                this.patronControl.Visibility = Visibility.Collapsed;

#if REMOVED
            if (App.Protocol == "sip")
            {
                if (returnButton.Visibility == Visibility.Visible)
                    this.patronControl.Visibility = Visibility.Collapsed;
                else
                    this.patronControl.Visibility = Visibility.Visible;
            }
            else
            {
                if (returnButton.Visibility == Visibility.Visible
                    || renewButton.Visibility == Visibility.Visible)
                    this.patronControl.Visibility = Visibility.Collapsed;
                else
                    this.patronControl.Visibility = Visibility.Visible;
            }
            /*
            // (普通)还书和续借操作并不需要读者卡
            if (borrowButton.Visibility != Visibility.Visible)
                this.patronControl.Visibility = Visibility.Collapsed;
            else
                this.patronControl.Visibility = Visibility.Visible; // 2019/9/3
                */
#endif
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

        async Task ClearBooksAndPatronAsync(/*BaseChannel<IRfid> channel*/)
        {
            try
            {
                ClearBookList();
                await FillBookFieldsAsync(
                    // channel,
                    new List<Entity>());
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"ClearBooksAndPatron() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");
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

        SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // TODO: 需要和 LoadAsync() 互斥
        // 检查 RFID 标签的 EAS 状态
        void CheckEAS(List<Entity> entities)
        {
            _semaphore.Wait();
            try
            {
                foreach (Entity entity in entities)
                {
                    if (entity.TagInfo == null)
                        continue;

                    // 对状态不明(State == null)册记录暂时不处理修正 EAS
                    if (entity.State == null)
                        continue;

                    // 2023/11/26
                    RfidTagList.SetTagInfoEAS(entity.TagInfo);

                    // 检测 EAS 是否正确
                    SetEasResult result = null;
                    if (StringUtil.IsInList("borrowed", entity.State) && entity.TagInfo.EAS == true)
                        result = SetEAS(entity.UID,
                            entity.Antenna,
                            false,
                            (uid, enable, seteas_result) =>
                            {
                                UpdateEntityUID(entity, enable, seteas_result);
                            });
                    else if (StringUtil.IsInList("onshelf", entity.State) && entity.TagInfo.EAS == false)
                        result = SetEAS(entity.UID,
                            entity.Antenna,
                            true,
                            (uid, enable, seteas_result) =>
                            {
                                UpdateEntityUID(entity, enable, seteas_result);
                            });
                    else
                        continue;

                    // UpdateEntityUID(entity, result);

                    // TODO: 报错信息很快被 EPC 变化导致重新显示而消失

                    if (result.Value == -1)
                        entity.SetError($"自动修正 EAS 时出错: {result.ErrorInfo}", "red");
                    else
                        entity.SetError("自动修正 EAS 成功", "green");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // 从指纹阅读器获取消息(第一阶段)
        bool SetPatronInfo(GetMessageResult result)
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{

            if (result.Value == -1)
            {
                SetPatronError("fingerprint", $"{GetFingerprintCaption()}中心出错: {result.ErrorInfo}, 错误码: {result.ErrorCode}");
                if (IsVerticalCard()/*_patron.IsFingerprintSource || App.PatronReaderVertical == true*/)
                    PatronClear();    // 只有当面板上的读者信息来源是指纹仪时(或者身份读卡器竖放)，才清除面板上的读者信息
                return false;
            }
            else if (result.Quality == -1)
            {
                // 掌纹(或者指纹)图像质量较差
                // TODO: 有没有必要提示，如何提示? 比如积累到一定次数以后，提醒“请调整手掌距离”
                return false;
            }
            else
            {
                // 清除以前残留的报错信息
                SetPatronError("fingerprint", "");
            }

            // TODO: (掌纹识别)和上一个识别的读者证条码号一样，则语音提示，不做重新装载读者信息

            if (result.Message == null)
                return false;

            /*
            // 2024/1/20
            if (_smoothTable.Check(result?.Message) == false)
                return false; // 平滑掉
            */

            // 2024/1/20
            if (string.IsNullOrEmpty(App.FingerprintUrl) == false
                && _patron.PII == result.Message
                && DateTime.Now - _smoothTable.GetLastTime(result.Message) < TimeSpan.FromSeconds(5))
            {
                App.CurrentApp.SpeakSequence($"{GetFingerprintCaption()}识别重复");
                return false; // 防止短时间重复同一个条码
            }

            _smoothTable.SetLastTime(result.Message, DateTime.Now);

            PatronClear();
            _patron.IsFingerprintSource = true;
            _patron.PII = result.Message;
            // TODO: 此种情况下，要禁止后续从读卡器获取，直到新一轮开始。
            // “新一轮”意思是图书全部取走以后开始的下一轮
            //}));

            return true;
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
        async Task<NormalResult> FillPatronDetailAsync(bool force = false)
        {
            return await Task.Run(async () =>
            {
                return await _fillPatronDetailAsync(force);
            });
        }


        // 填充读者信息的其他字段(第二阶段)
        async Task<NormalResult> _fillPatronDetailAsync(bool force = false)
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
                GetReaderInfoResult result = null;
                if (App.Protocol == "sip")
                {
                    string oi = string.IsNullOrEmpty(_patron.OI) ? _patron.AOI : _patron.OI;

                    // 对指纹、掌纹来源的做特殊处理，保证 SIP 请求中含有 AO 字段
                    if (oi == null && _patron.IsFingerprintSource)
                        oi = "";

                    // 2021/6/9
                    if (Patron.IsPQR(pii))
                        oi = "";

                    result = await SipChannelUtil.GetReaderInfoAsync(oi, pii);
                }
                else
                {
#if REMOVED
                    result = await
                        Task<GetReaderInfoResult>.Run(() =>
                        {
                            // 2021/4/2 改为用 oi+pii
                            bool strict = !_patron.IsFingerprintSource;
                            // 2021/4/15
                            if (ChargingData.GetBookInstitutionStrict() == false)
                                strict = false;
                            string oi_pii = _patron.GetOiPii(strict); // 严格模式，必须有 OI
                            return LibraryChannelUtil.GetReaderInfo(string.IsNullOrEmpty(oi_pii) ? pii : oi_pii);
                        });
#endif
                    // 2021/4/2 改为用 oi+pii
                    bool strict = !_patron.IsFingerprintSource;
                    // 2021/4/15
                    if (ChargingData.GetBookInstitutionStrict() == false)
                        strict = false;
                    string oi_pii = _patron.GetOiPii(strict); // 严格模式，必须有 OI

                    // 2024/1/23
                    if (string.IsNullOrEmpty(oi_pii) == false)
                        pii = oi_pii;

                    result = LibraryChannelUtil.GetReaderInfo(pii);    // string.IsNullOrEmpty(oi_pii) ? pii : oi_pii
                }

                if (result.Value != 1)
                {
                    // 2024/1/23
                    if (string.IsNullOrEmpty(_patron.Barcode))
                        _patron.Barcode = pii;
                    string error = $"读者 '{pii}': {result.ErrorInfo}";
                    SetPatronError("getreaderinfo", error);
                    ClearBorrowedEntities();
                    return new NormalResult { Value = -1, ErrorInfo = error };
                }

                SetPatronError("getreaderinfo", "");

                if (force)
                    _patron.PhotoPath = "";
                //App.Invoke(new Action(() =>
                //{
                try
                {
                    _patron.MaskDefinition = ShelfData.GetPatronMask();
                    _patron.SetPatronXml(result.RecPath, result.ReaderXml, result.Timestamp);

                    App.Invoke(() =>
                    {
                        this.patronControl.SetBorrowed(result.ReaderXml);
                    });

                    // 2024/1/2
                    _patron.BorrowItemsVisible = _patron.BorrowingCount > 0;
                }
                catch (Exception ex)
                {
                    // 2021/8/31
                    _patron.SetError(ex.Message);
                    WpfClientInfo.WriteErrorLog($"SetBorrowed() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    SetGlobalError("patron", $"SetBorrowed() 出现异常: {ex.Message}");
                }
                //}));

#if NO
            // 装载图象
            if (old_photopath != _patron.PhotoPath)
            {
                Task.Run(()=> {
                    LoadPhoto(_patron.PhotoPath);
                });
            }
#endif
            }
            finally
            {
                _patron.Waiting = false;
            }

            // 显示在借图书列表
            // 此时 _patron 控件的等待动画已经结束
            {
                /*
                List<Entity> entities = new List<Entity>();
                foreach (Entity entity in this.patronControl.BorrowedEntities)
                {
                    entities.Add(entity);
                }
                */
                var entities = this.patronControl.BorrowedEntities.ToList();
                if (entities.Count > 0)
                {
                    try
                    {
                        // 注: RFID 中心没有启动的时候，这一句会抛出异常
                        //BaseChannel<IRfid> channel = RfidManager.GetChannel();
                        try
                        {
                            await FillBookFieldsAsync(
                                // channel, 
                                entities);
                        }
                        finally
                        {
                            //RfidManager.ReturnChannel(channel);
                        }
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"FillPatronDetailAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        string error = $"填充读者信息时出现异常: {ex.Message}";
                        SetGlobalError("rfid", error);
                        return new NormalResult { Value = -1, ErrorInfo = error };
                    }
                }
            }

            return new NormalResult();
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
        // parameters:
        //      style   如果包含 coverImage，会主动获得 Entity.LocalCoverImagePath
        //              如果包含 checkEAS，会启动自动修正 EAS 线程
        Task FillBookFieldsAsync(
            // BaseChannel<IRfid> channel,
            List<Entity> entities,
            string style = "",
            CancellationToken token = default)
        {
            // var coverImage = StringUtil.IsInList("coverImage", style);
#if NO
            RfidChannel channel = RFID.StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);
#endif
            try
            {
                var title_entities = new List<Entity>();
                List<CoverItem> cover_items = new List<CoverItem>();
                foreach (Entity entity in entities)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (entity.FillFinished == true)
                        continue;

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

                        LogicChip chip = null;
                        string error = null;

                        if (entity.TagInfo.Protocol == InventoryInfo.ISO15693)
                        {
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            chip = LogicChip.From(entity.TagInfo.Bytes,
    (int)entity.TagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
                        }
                        else if (entity.TagInfo.Protocol == InventoryInfo.ISO18000P6C)
                        {
                            // 2023/11/3
                            // 注1: taginfo.EAS 在调用后可能被修改
                            // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                            var chip_info = RfidTagList.GetUhfChipInfo(entity.TagInfo);
                            error = chip_info.ErrorInfo;
                            chip = chip_info.Chip;
                            /*
                            if (chip == null)
                                entity.OI = chip_info.OI;
                            */
                        }
                        else
                        {
                            // 无法识别的 RFID 标签协议
                            // TODO: 抛出异常?
                        }

                        string pii = chip?.FindElement(ElementOID.PII)?.Text;
                        entity.PII = GetCaption(pii);

                        if (chip != null)
                        {
                            entity.OI = chip?.FindElement(ElementOID.OI)?.Text;
                            entity.AOI = chip?.FindElement(ElementOID.AOI)?.Text;
                        }

                        // 2024/4/23
                        if (string.IsNullOrEmpty(error) == false)
                            entity.AppendError(error);
                    }

                    bool clearError = true;

                    // 获得 Title。这一部分可以考虑在单独的线程中完成
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        title_entities.Add(entity);
#if REMOVED
                        bool item_completed = false;

                        var waiting = entity.Waiting;
                        entity.Waiting = true;
                        try
                        {
                            GetEntityDataResult result = null;
                            if (App.Protocol == "sip")
                                result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
                                    entity.GetOiOrAoi(),
                                    "network");
                            else
                            {
                                // 2021/4/15
                                var strict = ChargingData.GetBookInstitutionStrict();
                                if (strict)
                                {
                                    string oi = entity.GetOiOrAoi();
                                    if (string.IsNullOrEmpty(oi))
                                    {
                                        if (IsWhdtFormat(entity) == false)
                                        {
                                            entity.SetError("标签中没有机构代码，被拒绝使用");
                                            clearError = false;
                                            goto CONTINUE;
                                        }
                                        else
                                            strict = false; // 改为不严格模式 2023/12/4
                                    }
                                }
                                result = await LibraryChannelUtil.GetEntityDataAsync(entity.GetOiPii(strict),
                                    coverImage && string.IsNullOrEmpty(entity.CoverImageLocalPath) ? "network,coverImageUrl" : "network", // 2021/4/2 改为严格模式 OI_PII
                                    (item_result) =>
                                    {
                                        entity.SetData(item_result.ItemRecPath,
    item_result.ItemXml,
    DateTime.Now);
                                        item_completed = true;
                                    },
                                    (title) =>
                                    {
                                        entity.Title = GetCaption(title);
                                    });

                                if (coverImage
                                    && string.IsNullOrEmpty(result.CoverImageUrl) == false
                                    && string.IsNullOrEmpty(result.BiblioRecPath) == false)
                                {
                                    var object_path = ScriptUtil.MakeObjectUrl(result.BiblioRecPath, result.CoverImageUrl);
                                    cover_items.Add(new CoverItem
                                    {
                                        ObjectPath = object_path,
                                        Entity = entity
                                    });
                                }
                            }

                            if (result.Value == -1)
                            {
                                entity.SetError(result.ErrorInfo);
                                clearError = false;
                                goto CONTINUE;
                            }

                            entity.Title = GetCaption(result.Title);
                            if (item_completed == false)
                                entity.SetData(result.ItemRecPath,
                                    result.ItemXml,
                                    DateTime.Now);

                            // 2020/7/3
                            // 获得册记录阶段出错，但获得书目摘要成功
                            if (string.IsNullOrEmpty(result.ErrorCode) == false)
                            {
                                entity.SetError(result.ErrorInfo);
                                clearError = false;
                            }
                        }
                        finally
                        {
                            entity.Waiting = waiting;
                        }
#endif
                    }

                    /*
                CONTINUE:
                    if (clearError == true)
                        entity.SetError(null);
                    entity.FillFinished = true;
                    // 2020/9/10
                    entity.Waiting = false;
                    */
                }

                var checkEAS = StringUtil.IsInList("checkEAS", style);

                // 后续获取 title 和显示封面都放在另外一个单独的线程，本函数此时就可以返回了
                return BeginGetTitleAndCoverImage(title_entities,
        cover_items,
        checkEAS ? entities : null,
        style,
        token);
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"FillBookFieldsAsync() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                SetGlobalError("current", $"FillBookFieldsAsync() 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
                return Task.CompletedTask;
            }
        }

        // parameters:
        //      eas_entities    打算进行 EAS 修正的那些 Entity
        Task BeginGetTitleAndCoverImage(List<Entity> title_entities,
            List<CoverItem> cover_items,
            List<Entity> eas_entities,
            string style = "",
            CancellationToken token = default)
        {
            var coverImage = StringUtil.IsInList("coverImage", style);

            return Task.Run(async () =>
            {
                try
                {
                    // 获得 title 和各种册记录字段
                    foreach (var entity in title_entities)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        bool clearError = true;

                        // 获得 Title。这一部分可以考虑在单独的线程中完成
                        // 注：如果 Title 为空，文字中要填入 "(空)"
                        if (string.IsNullOrEmpty(entity.Title)
                            && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                        {
                            bool item_completed = false;

                            var waiting = entity.Waiting;
                            entity.Waiting = true;
                            try
                            {
                                GetEntityDataResult result = null;
                                if (App.Protocol == "sip")
                                    result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
                                        entity.GetOiOrAoi(),
                                        "network");
                                else
                                {
                                    // 2021/4/15
                                    var strict = ChargingData.GetBookInstitutionStrict();
                                    if (strict)
                                    {
                                        string oi = entity.GetOiOrAoi();
                                        if (string.IsNullOrEmpty(oi))
                                        {
                                            if (IsWhdtFormat(entity) == false)
                                            {
                                                entity.SetError("标签中没有机构代码，被拒绝使用");
                                                clearError = false;
                                                goto CONTINUE;
                                            }
                                            else
                                                strict = false; // 改为不严格模式 2023/12/4
                                        }
                                    }
                                    result = await LibraryChannelUtil.GetEntityDataAsync(entity.GetOiPii(strict),
                                        coverImage && string.IsNullOrEmpty(entity.CoverImageLocalPath) ? "network,coverImageUrl" : "network", // 2021/4/2 改为严格模式 OI_PII
                                        (item_result) =>
                                        {
                                            entity.SetData(item_result.ItemRecPath,
        item_result.ItemXml,
        DateTime.Now);
                                            item_completed = true;
                                        },
                                        (title) =>
                                        {
                                            entity.Title = GetCaption(title);
                                        });

                                    if (coverImage
                                        && string.IsNullOrEmpty(result.CoverImageUrl) == false/*
                                        && string.IsNullOrEmpty(result.BiblioRecPath) == false*/)
                                    {
                                        // var object_path = ScriptUtil.MakeObjectUrl(result.BiblioRecPath, result.CoverImageUrl);
                                        cover_items.Add(new CoverItem
                                        {
                                            ObjectPath = result.CoverImageUrl,  // object_path,
                                            Entity = entity
                                        });
                                    }
                                }

                                if (result.Value == -1)
                                {
                                    entity.SetError(result.ErrorInfo);
                                    clearError = false;
                                    goto CONTINUE;
                                }

                                entity.Title = GetCaption(result.Title);
                                if (item_completed == false)
                                    entity.SetData(result.ItemRecPath,
                                        result.ItemXml,
                                        DateTime.Now);

                                // 2020/7/3
                                // 获得册记录阶段出错，但获得书目摘要成功
                                if (string.IsNullOrEmpty(result.ErrorCode) == false)
                                {
                                    entity.SetError(result.ErrorInfo);
                                    clearError = false;
                                }
                            }
                            finally
                            {
                                entity.Waiting = waiting;
                            }
                        }

                    CONTINUE:
                        if (clearError == true)
                            entity.SetError(null);
                        entity.FillFinished = true;
                        entity.Waiting = false;
                    }

                    App.Invoke(() =>
                    {
                        booksControl.SetBorrowable();
                    });

                    if (eas_entities != null
                        && eas_entities.Count > 0)
                    {
                        _ = Task.Run(() =>
                        {
                            // 自动检查 EAS 状态
                            CheckEAS(eas_entities);
                        });
                    }

                    // 获取封面图像
                    if (cover_items.Count > 0)
                    {
                        string cacheDir = CoverImagesDirectory;

                        foreach (var item in cover_items)
                        {
                            if (token.IsCancellationRequested)
                                break;
                            string fileName = Path.Combine(cacheDir, GetImageFilePath(item.ObjectPath));

                            if (File.Exists(fileName) == false)
                            {
                                var get_result = await LibraryChannelUtil.GetCoverImageAsync(item.ObjectPath, fileName);
                                if (get_result.Value == 1)
                                    item.Entity.CoverImageLocalPath = fileName;
                                if (get_result.Value == -1 && get_result.ErrorCode == "diskFull")
                                {
                                    // 执行缓存清理任务
                                    BeginCleanCoverImagesDirectory(DateTime.Now);
                                }
                            }
                            else
                                item.Entity.CoverImageLocalPath = fileName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"BeginGetTitleAndCoverImage() thread 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                    SetGlobalError("current", $"BeginGetTitleAndCoverImage() thread 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
                }
            });
        }


        public static bool IsWhdtFormat(Entity entity)
        {
            if (entity.Protocol != InventoryInfo.ISO18000P6C)
                return false;
            var epc_bank = ByteArray.GetTimeStampByteArray(entity.UID);
            return GaoxiaoUtility.IsWhdt(epc_bank);
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
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoanAsync("borrow");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"BorrowButton_Click() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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

        async Task LoanAsync(string action)
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
                progress.Closed += (s, e) => { RemoveLayer(); };   // Progress_Closed;
                progress.Show();
                AddLayer(); // Closed 事件会 RemoveLayer()
            }));

            // 检查读者卡状态是否 OK
            if (IsPatronOK(action, false, out string check_message) == false)
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

            // dp2 协议：借书操作必须要有读者卡。(还书和续借，可要可不要)
            // sip2 协议：借书和续借都要有读者卡
            if (action == "borrow"
                || (action == "renew" && App.Protocol == "sip"))
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

            LibraryChannel channel = null;
            TimeSpan old_timeout = TimeSpan.FromSeconds(0);
            if (App.Protocol == "dp2library")
            {
                channel = App.CurrentApp.GetChannel();
                old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(10);
            }

            await _semaphore.WaitAsync();

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

                // 2020/8/26
                // 先复制出来，避免受到 _entities 中途变化的影响
                List<Entity> entities = new List<Entity>(_entities);

                foreach (Entity entity in entities)
                {
                    long lRet = 0;
                    string[] item_records = null;
                    string strError = "";
                    string returning_date = null;
                    string period = null;

                    // 2023/12/7
                    if (string.IsNullOrEmpty(entity.PII)
                        || entity.PII == "(空)")
                    {
                        entity.SetError($"本册的 RFID 标签是空内容。{action_name}操作被忽略", "yellow");
                        skip_count++;
                        continue;
                    }

                    // (还书前)预修改 EAS 是否发生了修改
                    bool eas_changed = false;

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
                    }
                    else if (action == "return")
                    {
                        var is_onshelf = StringUtil.IsInList("onshelf", entity.State);
                        if (is_onshelf)
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }

                        // return 操作，提前修改 EAS
                        // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                        // 检查 EAS 现有状态，如果已经是 true 则不用修改，后面 API 遇到出错后也不要回滚 EAS
                        if (entity.GetEas() == false)
                        {
                            SetEasResult result;
                            if (App.RfidTestReturnPreEAS)
                                result = new SetEasResult
                                {
                                    Value = -1,
                                    ErrorInfo = "测试触发修改 EAS 报错(还书操作前段)",
                                    ErrorCode = "rfidTestError"
                                };
                            else
                                result = SetEAS(entity.UID,
                                    entity.Antenna,
                                    action == "return",
                                    (uid, enable, seteas_result) =>
                                    {
                                        UpdateEntityUID(entity, enable, seteas_result);
                                    });
                            // UpdateEntityUID(entity, result);
                            if (result.Value == -1)
                            {
                                entity.SetError($"{action_name}时修改 EAS 动作失败: {result.ErrorInfo}", "red");
                                errors.Add($"册 '{entity.PII}' {action_name}时修改 EAS 动作失败: {result.ErrorInfo}");
                                continue;
                            }

                            eas_changed = true;
                        }
                    }

                    entity.Waiting = true;
                    if (App.Protocol == "sip")
                    {
                        if (action == "borrow")
                        {
                            var result = await SipChannelUtil.BorrowAsync(_patron.GetOiPii(), entity.GetOiPii());
                            if (result.Value == -1)
                            {
                                lRet = -1;
                                strError = result.ErrorInfo;
                            }
                        }
                        else if (action == "renew")
                        {
                            var result = await SipChannelUtil.RenewAsync(_patron.GetOiPii(), entity.GetOiPii());
                            if (result.Value == -1)
                            {
                                lRet = -1;
                                strError = result.ErrorInfo;
                            }
                        }
                        else if (action == "return")
                        {
                            var result = await SipChannelUtil.ReturnAsync(entity.GetOiPii());
                            if (result.Value == -1)
                            {
                                lRet = -1;
                                strError = result.ErrorInfo;
                            }
                        }
                        else
                        {
                            lRet = -1;
                            strError = $"无法识别的 action '{action}'";
                        }
                    }
                    else if (App.Protocol == "dp2library")
                    {
                        string patron_barcode_or_uii = _patron.GetOiPii();
                        if (string.IsNullOrEmpty(patron_barcode_or_uii))
                            patron_barcode_or_uii = _patron.Barcode;

                        // 2021/4/15
                        var strict = ChargingData.GetBookInstitutionStrict();
                        if (strict)
                        {
                            string oi = entity.GetOiOrAoi();
                            if (string.IsNullOrEmpty(oi))
                            {
                                if (IsWhdtFormat(entity) == false)
                                {
                                    strError = "标签中没有机构代码，被拒绝使用";
                                    entity.SetError(strError, "red");
                                    skip_count++;
                                    continue;
                                }
                                else
                                    strict = false; // 改为不严格模式 2023/12/4
                            }
                        }

                        if (action == "borrow" || action == "renew")
                        {
                            /*
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
                            */

                            //entity.Waiting = true;
                            lRet = channel.Borrow(null,
                                action == "renew",
                                patron_barcode_or_uii, // _patron.Barcode,
                                entity.GetOiPii(strict),  // entity.PII,
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

                            if (borrow_info != null)
                            {
                                returning_date = borrow_info.LatestReturnTime;
                                period = borrow_info.Period;
                            }
                        }
                        else if (action == "return")
                        {
                            /*
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
                            */

                            if (App.RfidTestReturnAPI)
                            {
                                lRet = -1;
                                strError = "测试触发还书 API 报错(还书操作中段)，注意此时还书并没有成功";
                            }
                            else
                            {
                                //entity.Waiting = true;
                                lRet = channel.Return(null,
                                    "return",
                                    patron_barcode_or_uii, // _patron.Barcode,
                                    entity.GetOiPii(strict),  // entity.PII,
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
                        }
                        else
                        {
                            lRet = -1;
                            strError = $"无法识别的 action '{action}'";
                        }
                    }

                    App.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value++;
                    }));

                    if (lRet == -1)
                    {
                        // return 操作如果 API 失败，则要回滚到原来的 EAS 状态
                        if (action == "return"
                            && eas_changed == true)
                        {
                            SetEasResult result;
                            if (App.RfidTestReturnPostUndoEAS)
                                result = new SetEasResult
                                {
                                    Value = -1,
                                    ErrorInfo = "测试触发修改 EAS 报错(还书操作末段，回滚 EAS 时)",
                                    ErrorCode = "rfidTestError"
                                };
                            else
                                result = SetEAS(entity.UID,
                                    entity.Antenna,
                                    false,
                                    (uid, enable, seteas_result) =>
                                    {
                                        UpdateEntityUID(entity, enable, seteas_result);
                                    });
                            // UpdateEntityUID(entity, result);
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
                        SetEasResult result;
                        if (App.RfidTestBorrowEAS)
                            result = new SetEasResult
                            {
                                Value = -1,
                                ErrorInfo = "测试触发修改 EAS 报错(借书操作末段)",
                                ErrorCode = "rfidTestError"
                            };
                        else
                            result = SetEAS(entity.UID,
                                entity.Antenna,
                                action == "return",
                                (uid, enable, seteas_result) =>
                                {
                                    UpdateEntityUID(entity, enable, seteas_result);
                                });

                        // UpdateEntityUID(entity, result);
                        if (result.Value == -1)
                        {
                            entity.SetError($"虽然{action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}", "yellow");
                            errors.Add($"册 '{entity.PII}' {action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}");
                        }
                    }

                    // 2020/8/19
                    // 打印凭条
                    PosPrint(action, period, returning_date, entity);

                    // 刷新显示
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath,
                                item_records[0],
                                DateTime.Now);

                        if (App.Protocol == "sip")
                        {
                            // 刷新册记录状态显示
                            await RefreshEntityStateAsync(entity);
                        }

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

                    if (success_count > 0)
                    {
                        // 重新装载读者信息和显示
                        var task = FillPatronDetailAsync(true);
                    }

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
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"LoanAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                DisplayError(ref progress, $"LoanAsync() 出现异常: {ex.Message}");
                return;
            }
            finally
            {
                _semaphore.Release();

                if (App.Protocol == "dp2library")
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }

                App.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
            }
        }

        // 凭条打印
        // TODO: 加上 location shelfNo
        void PosPrint(string action, string period, string returning_date, Entity entity)
        {
            try
            {
                var style = App.PosPrintStyle.Replace("+", ",");
                if (style == "不打印")
                    return;

                StringBuilder text = new StringBuilder();

                if (action == "borrow" || action == "renew")
                {
                    if (StringUtil.IsInList("借书", style))
                    {
                        // 注意：可能会抛出异常
                        string time_string = "";

                        try
                        {
                            time_string = DateTimeUtil.FromRfc1123DateTimeString(returning_date).ToLocalTime().ToLongDateString();
                        }
                        catch (Exception ex)
                        {
                            WpfClientInfo.WriteErrorLog($"PosPrint() FromRfc1123DateTimeString({returning_date}) 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            time_string = returning_date;
                        }

                        if (period == null)
                            period = "";
                        period = period.Replace("day", "天").Replace("hour", "小时");

                        string caption = "借书";
                        if (action == "renew")
                            caption = "续借";

                        text.AppendLine($"*** {caption} ***");
                        text.AppendLine($"[{entity.PII}] {entity.Title}");
                        text.AppendLine($"馆藏地点: {entity.Location} {entity.ShelfNo} {entity.AccessNo}");
                        text.AppendLine($"{caption}时间: " + DateTime.Now.ToString());
                        text.AppendLine("期    限: " + period);
                        text.AppendLine("应还日期: " + time_string);
                        if (string.IsNullOrEmpty(App.LibraryName) == false)
                            text.AppendLine($"=== {App.LibraryName} ===");
                    }
                }
                else if (action == "return")
                {
                    if (StringUtil.IsInList("还书", style))
                    {
                        // TODO: 最好增加显示超期信息(是否超期)
                        text.AppendLine("*** 还书 ***");
                        text.AppendLine($"[{entity.PII}] {entity.Title}");
                        text.AppendLine($"馆藏地点: {entity.Location} {entity.ShelfNo} {entity.AccessNo}");
                        text.AppendLine("还书时间: " + DateTime.Now.ToString());
                        if (string.IsNullOrEmpty(App.LibraryName) == false)
                            text.AppendLine($"=== {App.LibraryName} ===");
                    }
                }

                if (text.Length > 0)
                {
                    var result = RfidManager.PosPrint("printline", text.ToString(), "");
                    if (result.Value == -1)
                        SetGlobalError("posprint", $"打印凭条时出错: {result.ErrorInfo}");
                    else
                        SetGlobalError("posprint", null);

                    RfidManager.PosPrint("cut", "", "");
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"PosPrint() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                SetGlobalError("posprint", $"PosPrint() 出现异常: {ex.Message}");
            }
        }

        static async Task RefreshEntityStateAsync(Entity entity)
        {
            if (App.Protocol != "sip")
                return;

            GetEntityDataResult result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
                entity.GetOiOrAoi(),
                "network");
            if (result.Value == -1)
            {
                entity.AppendError(result.ErrorInfo);
                return;
            }

            entity.Title = GetCaption(result.Title);
            entity.SetData(result.ItemRecPath,
                result.ItemXml,
                DateTime.Now);
        }

        /*
        private void Progress_Closed(object sender, EventArgs e)
        {
            RemoveLayer();
        }
        */
        void UpdateEntityUID(Entity entity,
            bool enable,
            SetEasResult result)
        {
            OneTag changed_tag = null;
            if (result.Value == 1)
            {
                string uid = entity.UID;
                string changed_uid = result.ChangedUID;
                // 2023/12/1
                // 精确修改 TagInfo
                if (string.IsNullOrEmpty(uid) == false
                    && result.Value == 1)
                {
                    if (string.IsNullOrEmpty(changed_uid) == false)
                        changed_tag = ChangeUID(uid, changed_uid);
                    else
                        changed_tag = ChangeUID(uid, enable ? "on" : "off");
                }
            }

            if (result.Value == 1 && string.IsNullOrEmpty(result.ChangedUID) == false)
            {
                /*
                if (entity.TagInfo != null)
                    entity.TagInfo.UID = result.ChangedUID;
                */
                if (changed_tag?.TagInfo != null)
                {
                    entity.TagInfo = changed_tag.TagInfo.Clone();
                    entity.UID = result.ChangedUID;
                    Debug.Assert(entity.TagInfo.UID == result.ChangedUID);
                }
                else
                {
                    // 等待下一轮 Update 自动更新 entity.UID
                }
            }
        }

        delegate void delagate_tagChanged(string uid,
            bool enable,
            SetEasResult result);

        /*
        class ChangeEasResult : SetEasResult
        {
            public OneTag ChangedTag { get; set; }
        }
        */

        // parameters:
        //      func_tagChanged 当标签发生修改以后触发。这里可以完成一些在 RfidManager.SyncRoot 锁定范围以内的事情
        SetEasResult SetEAS(string uid,
            string antenna,
            bool enable,
            delagate_tagChanged func_tagChanged)
        {
            try
            {
                if (uint.TryParse(antenna, out uint antenna_id) == false)
                    antenna_id = 0;
#if OLD_RFID
                this.ClearTagTable(uid);
#endif
                // TagList.ClearTagTable(uid);
                var result = RfidManager.SetEAS($"{uid}",
                    antenna_id,
                    enable);
#if REMOVED
                if (result.Value != -1)
                {
                    RfidTagList.SetEasData(uid, enable);
                    _entities.SetEasData(uid, enable);
                    if (result.ChangedUID != null)
                    {
                        uid = result.ChangedUID;
                        RfidTagList.SetEasData(uid, enable);
                        _entities.SetEasData(uid, enable);
                    }
                }
#endif
                // OneTag changed_tag = null;
                if (result.Value == 1)
                {
                    /*
                    string changed_uid = result.ChangedUID;
                    // 2023/12/1
                    // 精确修改 TagInfo
                    if (string.IsNullOrEmpty(uid) == false
                        && result.Value == 1)
                    {
                        if (string.IsNullOrEmpty(changed_uid) == false)
                            changed_tag = ChangeUID(uid, changed_uid);
                        else
                            changed_tag = ChangeUID(uid, enable ? "on" : "off");
                    }
                    */
                    func_tagChanged?.Invoke(uid, enable, result);
                }
                return new SetEasResult
                {
                    Value = result.Value,
                    ChangedUID = result.ChangedUID,
                    // ChangedTag = changed_tag,
                    ErrorInfo = result.ErrorInfo,
                    ErrorCode = result.ErrorCode
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SetEAS() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new SetEasResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
        }

        // parameters:
        //      old_uid 原先的 UID
        //      changed_uid 修改后的 UID (对于 UHF 标签)
        //                  "on" "off" 之一(对于 HF 标签)
        public static OneTag ChangeUID(string old_uid, string changed_uid)
        {
            BaseChannel<IRfid> channel = RfidManager.GetChannel();
            try
            {
                return RfidTagList.ChangeUID(channel, old_uid, changed_uid);
            }
            finally
            {
                RfidManager.ReturnChannel(channel);
            }
        }

        // 当前读者卡状态是否 OK?
        // 注：如果卡片虽然放上去了，但无法找到读者记录，这种状态就不是 OK 的。此时应该拒绝进行流通操作
        bool IsPatronOK(string action,
            bool show_debug_info,
            out string message)
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
            if (IsVerticalCardDefined()/*App.PatronReaderVertical*/)
                fang = "扫";

            string debug_info = $"(uid:[{_patron.UID}],barcode:[{_patron.Barcode}])";
            if (action == "borrow")
            {
                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先{fang}读者卡，或扫入一次{GetFingerprintCaption()}，然后再进行借书操作{(show_debug_info ? debug_info : "")}";
                else
                    message = $"请先{fang}读者卡，然后再进行借书操作{(show_debug_info ? debug_info : "")}";
            }
            else if (action == "registerFace"
                || action == "deleteFace")
            {
                string action_name = "注册人脸";
                if (action == "deleteFace")
                    action_name = "删除人脸";

                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先{fang}读者卡，或扫入一次{GetFingerprintCaption()}，然后再进行{action_name}操作{(show_debug_info ? debug_info : "")}";
                else
                    message = $"请先{fang}读者卡，然后再进行{action_name}操作{(show_debug_info ? debug_info : "")}";
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
                    styles.Add($"或扫入一次{GetFingerprintCaption()}");
                if (string.IsNullOrEmpty(App.FaceUrl) == false)
                    styles.Add("或人脸识别");
                if (string.IsNullOrEmpty(App.PatronBarcodeStyle) == false && App.PatronBarcodeStyle != "禁用")
                    styles.Add("或扫入读者证条码");

                message = $"{StringUtil.MakePathList(styles, "，")} ...";
            }
            else
            {
                // 调试用
                message = $"读卡器上的当前读者卡状态不正确。无法进行 {action} 操作{(show_debug_info ? debug_info : "")}";
            }
            return false;
        }

        public static string GetFingerprintCaption()
        {
            if (App.FingerprintUrl != null
                && App.FingerprintUrl.ToLower().Contains("palm"))
                return "掌纹";
            return "指纹";
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
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoanAsync("return");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"ReturnButton_Click() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        // 续借
        private void RenewButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoanAsync("renew");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"RenewButton_Click() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

        void TryBackMenuPage()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    App.Invoke(new Action(() =>
                    {
                        this.NavigationService?.Navigate(PageMenu.MenuPage);
                    }));
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"TryBackMenuPage() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        #region patron 分类报错机制

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint getreaderinfo
        ErrorTable _patronErrorTable = null;

        // 设置读者区域错误字符串
        void SetPatronError(string type, string error)
        {
            _patronErrorTable.SetError(type,
                error,
                true);
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
        static void SetGlobalError(string type, string error)
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

#if OLDVERSION
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
                            catch(Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"RegisterFaceAsync() DisplayVideo() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"RegisterFaceAsync() 倒计时过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
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

                    // 2020/7/30
                    // 清除本地人脸缓存文件
                    if (string.IsNullOrEmpty(_patron.PhotoPath) == false)
                    {
                        patronControl.ClearPhotoCache(_patron.PhotoPath);
                    }
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
                // 注册人脸成功 这句话会和 FaceCenter 的差不多同时的说话“获取人脸信息成功”重叠
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

#endif

        // 注册或者删除人脸
        private async Task RegisterFaceAsync(string action)
        {
            string action_name = "注册人脸";
            if (action == "deleteFace")
                action_name = "删除人脸";

            if (App.Function == "智能书柜" && ShelfData.LibraryNetworkCondition != "OK")
            {
                App.ErrorBox(
$"{action_name}",
$"当前为断网状态，无法{action_name}",
"red",
"auto_close:10");
                return;
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

            VideoWindow videoRegister = null;
            App.Invoke(new Action(() =>
            {
                videoRegister = new VideoWindow
                {
                    TitleText = $"{action_name} ...",
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                videoRegister.Closed += (s, e) =>
                {
                    FaceManager.CancelGetFeatureString();
                    _stopVideo = true;
                    RemoveLayer();
                }; // VideoRegister_Closed;
                videoRegister.Show();
                AddLayer(); // Closed 事件会 RemoveLayer()
            }));
            try
            {
                if (IsPatronOK(action, false, out string check_message) == false)
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
                                DisplayVideo(videoRegister, TimeSpan.FromMinutes(2));
                            }
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"RegisterFaceAsync() DisplayVideo() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }
                            finally
                            {
                                /*
                                // 2020/9/10
                                if (videoRegister != null)
                                    App.Invoke(new Action(() =>
                                    {
                                        videoRegister.Close();
                                    }));
                                App.CurrentApp.SpeakSequence($"放弃注册人脸");
                                */
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
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"RegisterFaceAsync() 倒计时过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }

                        });

                        var register_style = "countDown,format:jpeg,action:" + action;
                        // 2024/1/2
                        if (App.FaceInputMultipleHits == "使用第一个")
                            register_style += ",searchDup";
                        var result = await RegisterFeatureStringAsync(_patron.Barcode, register_style);
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

                if (action == "deleteFace")
                {
                    var result = await RegisterFeatureStringAsync(_patron.Barcode, "countDown,format:jpeg,action:" + action);
                    if (result.Value == -1 || result.Value == 0)
                    {
                        DisplayError(ref videoRegister, result.ErrorInfo);
                        return;
                    }
                }

                // 清除本地人脸缓存文件
                if (string.IsNullOrEmpty(_patron.PhotoPath) == false)
                {
                    patronControl.ClearPhotoCache(_patron.PhotoPath);
                }

                // 上传完对象后通知 facecenter DoReplication 一次
                var notify_result = FaceManager.Notify("faceChanged");
                if (notify_result.Value == -1)
                    SetGlobalError("face", $"FaceManager.Notify() error: {notify_result.ErrorInfo}");   // 2019/9/11 增加 error:

                string message = $"{action_name}成功";
                // 注册人脸成功 这句话会和 FaceCenter 的差不多同时的说话“获取人脸信息成功”重叠
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

#if REMOVED
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
#endif

        void DisplayError(ref ProgressWindow progress,
    string message,
    string color = "red",
    string set_button_text = null)
        {
            // 记住了 progress 对话框，最后 PageBorrow 退出的时候不会忘记关闭这个对话框
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
            // 这样本函数的调用者就不会关闭这个 progress 对话框了
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

#if OLDVERSION
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

#endif

        #region 下级函数

#if OLDVERSION
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

#endif
        #endregion

#if OLDVERSION

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

#endif

        /*
        private void VideoRegister_Closed(object sender, EventArgs e)
        {
            FaceManager.CancelGetFeatureString();
            _stopVideo = true;
            RemoveLayer();
        }
        */

#if OLDVERSION
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

#endif

        async Task<NormalResult> RegisterFeatureStringAsync(string strBarcode, string style)
        {
            EnableControls(false);
            try
            {
                return await Task.Run<NormalResult>(() =>
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
                    return FaceManager.RegisterFeatureString(null, strBarcode, style);
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

        public void PatronClear(bool check_card_existance = false)
        {
            // 清除以前检查一下身份读卡器上是否有读者卡
            if (check_card_existance && RfidTagList.Patrons.Count >= 1)
            {
                BeginWarningCard((s) =>
                {
                    // 延迟清除
                    if (s == "cancelled" && IsVerticalCard()/*App.PatronReaderVertical*/)
                        PatronClear(false);
                });
                return;
            }

            _patron.Clear();
            // 2020/12/9
            // 清除 ErrorTable 中的全部出错信息，避免残余内容后面重新出现在界面上
            _patronErrorTable.SetError(null, null);

            ClearBorrowedEntities();
            /*
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
            */

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

        void ClearBorrowedEntities()
        {
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
        }

        private void FixPatron_Checked(object sender, RoutedEventArgs e)
        {
            CancelDelayClearTask();
        }

        private void ClearPatron_Click(object sender, RoutedEventArgs e)
        {
            CancelDelayClearTask();

            PatronClear(true);  // 2024/1/19 改为 true
        }

        #region 提醒拿走读者卡

        static DelayAction _delayNotifyCard = null;

        public static void CancelNotifyTask()
        {
            if (_delayNotifyCard != null)
            {
                _delayNotifyCard.Stop();
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
                    if (RfidTagList.Patrons.Count >= 1)
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
                _delayClearPatronTask.Stop();
                _delayClearPatronTask = null;
            }
        }

        void BeginDelayClearTask()
        {
            // 横向放置身份证读卡器时，没有必要延迟清除。意思就是说横向情况是需要人主动拿走卡，屏幕上信息才能清除
            if (IsVerticalCard()/*App.PatronReaderVertical*/ == false)
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
                    if (RfidTagList.Patrons.Count >= 1)
                    {
                        BeginWarningCard((s) =>
                        {
                            // 延迟清除
                            // if (s == "cancelled" && App.PatronReaderVertical)
                            if (RfidTagList.Patrons.Count == 0) // 2019/12/13
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
                        if (RfidTagList.Patrons.Count == 0)
                        {
                            func_cancelled?.Invoke("patron_removed");
                            break;
                        }
                        if (RfidTagList.Books.Count > 0)
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
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"BeginWarningCard() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
            // 关闭残留的人脸识视频对话框
            _stopVideo = true;

            // 2020/12/8
            SetFixVisible();

            if (errorOccur)
            {
                // 身份读写器平放
                if (IsVerticalCard()/*App.PatronReaderVertical*/ == false)
                    BeginNotifyTask();

                if (IsVerticalCard()/*App.PatronReaderVertical*/ == true
    && RfidTagList.Books.Count == 0)
                    BeginDelayClearTask();
                return;
            }
            App.Invoke(new Action(() =>
            {
                fixPatron.IsEnabled = true;
                clearPatron.IsEnabled = true;
            }));

            // 欢迎您，
            // App.CurrentApp.Speak($"{(string.IsNullOrEmpty(_patron.PatronName) ? _patron.Barcode : _patron.PatronName)}");
            {
                // 2021/7/28
                var name = (string.IsNullOrEmpty(_patron.PatronNameMasked) ? _patron.BarcodeMasked : _patron.PatronNameMasked);
                if (string.IsNullOrEmpty(name) == false
                    && name.Contains("*") == false)
                    App.CurrentApp.Speak($"欢迎您，{name}");
                else
                    App.CurrentApp.Speak($"欢迎您");
            }

            // 身份读写器平放
            if (IsVerticalCard() /*App.PatronReaderVertical*/ == false)
                BeginNotifyTask();

            // 身份读写器竖放
            // 读写器上没有图书的时候，才启动延时清除
            if (IsVerticalCard() /*App.PatronReaderVertical == true*/
                && RfidTagList.Books.Count == 0)
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

            // 2021/7/29
            // 遮盖显示内容
            var mask_def = ShelfData.GetPatronMask();
            name = dp2StringUtil.Mask(mask_def, name, "name");
            department = dp2StringUtil.Mask(mask_def, department, "department");

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
                    if (RfidTagList.Patrons.Count == 0)
                        break;

                    var tag = RfidTagList.Patrons[0].OneTag;
                    if (tag.Protocol == InventoryInfo.ISO14443A)
                    {
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = "请先拿开读卡器上的读者卡(ISO14443A)，操作才能继续 ...";
                        }));
                    }
                    else
                        break;

                    await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"Get14443ACardUIDAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

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
                    if (RfidTagList.Patrons.Count == 0)
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

                    var results = RfidTagList.Patrons.FindAll(o => o.OneTag.Protocol == InventoryInfo.ISO14443A);
                    if (results.Count != 1)
                    {
                        string remove_text = "";
                        if (results.Count > 1)
                            remove_text = $"拿走多余的读者卡(ISO14443A)，然后";  // "读卡器只应放一张副卡。请拿走多余的副卡";

                        string text = $"放上要{action_caption}的副卡";
                        /*
                        if (App.PatronReaderVertical)
                            text = $"扫要{action_caption}的副卡";
                        */
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = "请" + remove_text + text;
                        }));
                    }

                    if (results.Count == 1)
                    {
                        var tag = results[0].OneTag;
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
                WpfClientInfo.WriteErrorLog($"Get14443ACardUIDAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

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

            if (App.Function == "智能书柜" && ShelfData.LibraryNetworkCondition != "OK")
            {
                App.ErrorBox(
$"{action_name}读者卡",
$"当前为断网状态，无法{action_name}读者卡",
"red",
"auto_close:10");
                return;
            }

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
                AddLayer(); // Closed 事件会 RemoveLayer()
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
                if (IsPatronOK(action, false, out string check_message) == false)
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
                    WpfClientInfo.WriteErrorLog($"BindPatronCardAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    DisplayError(ref progress, $"BindPatronCardAsync() 异常，读者记录 (证条码号:{_patron.Barcode},读者记录路径:{_patron.RecPath}) XML 装载到 XmlDocument 失败: {ex.Message}",
                        "red", "关闭");
                    return;
                }

                // string patron_name = DomUtil.GetElementText(dom.DocumentElement, "name");
                // 2021/7/29
                // 用于提示和显示的读者姓名
                // string patron_name = (string.IsNullOrEmpty(_patron.PatronNameMasked) ? _patron.BarcodeMasked : _patron.PatronNameMasked);
                string patron_name = _patron.PatronNameMasked;
                if (string.IsNullOrEmpty(_patron.BarcodeMasked) == false)
                    patron_name += " (" + _patron.BarcodeMasked + ")";

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
    bind_uid,
    patron_name);
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
bind_uid,
patron_name);
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

                        this.MemoryDialog(ask);

                        ask.TitleText = action_name;
                        ask.MessageText = $"确实要解除读者 {patron_name} 副卡 {bind_uid} 的绑定?\r\n\r\n(解除绑定以后，读者将无法再用这一张副卡进行任何操作)";
                        ask.Owner = Application.Current.MainWindow;
                        ask.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        App.SetSize(ask, "wide");
                        ask.OkButtonText = "是";
                        ask.CancelButtonVisible = true;
                        ask.ShowDialog();

                        this.ForgetDialog(ask);

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

        // parameters:
        //      patron_name 用于报错的读者姓名，可能是被遮盖以后的内容
        public static NormalResult ModifyBinding(XmlDocument dom,
            string action,
            string uid,
            string patron_name)
        {
            uid = uid.ToUpper();

            // string patron_name = DomUtil.GetElementText(dom.DocumentElement, "name");

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

        // 是否是竖向的卡，或者人脸、指纹、一维码、二维码方式？
        // (这种方式下需要固定读者信息一段时间)
        public bool IsVerticalCard()
        {
            if (App.Function == "智能书柜")
                return true;

            if (_patron.IsFingerprintSource
                || StringUtil.IsInList(_patron.ReaderName, App.VerticalReaderName))
                return true;
            return false;
            // return (App.PatronReaderVertical || _patron.IsFingerprintSource);
        }

        // 是否已经定义了某个竖放的读写器
        public bool IsVerticalCardDefined()
        {
            if (App.Function == "智能书柜")
                return true;
            return string.IsNullOrEmpty(App.VerticalReaderName) == false;
        }

        void SetFixVisible()
        {
            App.Invoke(new Action(() =>
            {
                // 身份读卡器竖向放置，才有固定读者信息的必要
                if (IsVerticalCard()/*App.PatronReaderVertical*/)
                {
                    // TODO: 这里可以提供一个定制特性的点位，让用户自定义是否出现固定按钮
                    fixAndClear.Visibility = Visibility.Visible;
                }
                else
                    fixAndClear.Visibility = Visibility.Collapsed;
            }));
        }

        public DateTime GetFillTime()
        {
            return _patron.FillTime;
        }

        public void ResetFillTime()
        {
            _patron.FillTime = DateTime.Now;
        }

#if REMOVED
        #region 掌纹 Video

        static void StartDisplayFingerprint(
            Image image_control,
            System.Windows.Shapes.Polyline lines_control,
            CancellationToken token)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        if (string.IsNullOrEmpty(FingerprintManager.Url))
                        {
                            DisplayError(image_control, $"尚未启用{GetFingerprintCaption()}识别功能", System.Drawing.Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        /*
                        if (_disableSendkey)
                        {
                            // TODO: 显示为掌纹图像上面叠加文字则更好
                            DisplayError("临时禁用发送", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        if (this.Pause == true || FingerprintManager.Pause == true)
                        {
                            DisplayError("暂停显示", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }
                        */

                        var result = FingerprintManager.GetImage("wait:1000,rect");
                        if (result.Value == -1)
                        {
                            // 显示错误
                            DisplayError(image_control, result.ErrorInfo, System.Drawing.Color.DarkRed);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        if (result.ImageData == null)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        PaintBytes(image_control,
                            result.ImageData,
                            out double width,
                            out double height);
                        if (lines_control != null
                            /*&& string.IsNullOrEmpty(result.Text) == false*/)
                            PaintLines(lines_control,
                                result.Text,
                                width,
                                height);
                    }
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    WpfClientInfo.WriteErrorLog($"显示{GetFingerprintCaption()}图像出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    // 显示错误
                    DisplayError(image_control, $"显示线程出现异常: {ex.Message}\r\n{GetFingerprintCaption()}图像显示已停止", System.Drawing.Color.DarkRed);
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

        }

        public static void PaintBytes(Image control,
            byte[] bytes,
            out double width,
            out double height)
        {
            double w = 0;
            double h = 0;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                App.Invoke(() =>
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                    image.EndInit();

                    control.Source = image;

                    w = control.ActualWidth / image.Width;
                    h = control.ActualHeight / image.Height;
                });
            }
            width = w;
            height = h;
        }

        static void DisplayError(
            Image control,
            string strError,
            System.Drawing.Color backColor)
        {
            App.Invoke(() =>
            {
                BitmapImage image = new BitmapImage();

                image.BeginInit();
                image.StreamSource = StringToBitmapConverter.BuildTextImage(strError, backColor, (int)control.Width);
                image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();
                control.Source = image;
            });
        }

        public static void PaintLines(System.Windows.Shapes.Polyline control,
            string text,
            double scale_x,
            double scale_y,
            float line_width = 4)
        {
            if (string.IsNullOrEmpty(text))
            {
                App.Invoke(() =>
                {
                    if (control.Points == null
                        || control.Points.Count != 0)
                        control.Points = new PointCollection();
                });
                return;
            }
            string[] values = text.Split(new char[] { ',' });
            List<int> rect = new List<int>();
            foreach (string v in values)
            {
                rect.Add(Convert.ToInt32(v));
            }
            if (rect.Count != 8)
                throw new ArgumentException("应该是 8 个数字");
            Debug.Assert(rect.Count == 8);

            App.Invoke(() =>
            {
                var transform = control.RenderTransform as ScaleTransform;
                transform.ScaleX = scale_x;
                transform.ScaleY = scale_y;

                control.Points = new PointCollection
                {
                new Point(rect[0], rect[1]),
                new Point(rect[2], rect[3]),
                new Point(rect[4], rect[5]),
                new Point(rect[6], rect[7]),
                new Point(rect[0], rect[1])
                };
            });
        }

        #endregion

#endif
    }
}
