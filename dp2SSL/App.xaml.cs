using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Remoting;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Deployment.Application;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore.Internal;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using static DigitalPlatform.IO.BarcodeCapture;
using DigitalPlatform.Face;
using DigitalPlatform.WPF;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Install;
using dp2SSL.Models;


//using Microsoft.VisualStudio.Shell;
//using Task = System.Threading.Tasks.Task;

namespace dp2SSL
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        public static event UpdatedEventHandler Updated = null;

        public static event LineFeedEventHandler LineFeed = null;
        public static event CharFeedEventHandler CharFeed = null;

        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        // 控制 App 的终止信号
        static CancellationTokenSource _cancelApp = new CancellationTokenSource();

        public static CancellationToken CancelToken
        {
            get
            {
                if (_cancelApp == null)
                    return new CancellationToken();
                return _cancelApp.Token;
            }
        }

        // 控制 RfidManager 的终止信号
        static CancellationTokenSource _cancelRfid = new CancellationTokenSource();

        CancellationTokenSource _cancelProcessMonitor = new CancellationTokenSource();


        Mutex myMutex;

        public void CloseMutex()
        {
            myMutex.Close();
        }

        static ErrorTable _errorTable = null;

        #region 属性

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

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

        private string _number = null;

        public string Number
        {
            get => _number;
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged("Number");
                }
            }
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        protected async override void OnStartup(StartupEventArgs e)
        {
            if (DisableEdgeUI() == true)
                return;

            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                StartErrorBox("dp2SSL 不允许重复启动");
                App.Current.Shutdown();
                return;
            }

            if (DetectVirus.DetectXXX() || DetectVirus.DetectGuanjia())
            {
                StartErrorBox("dp2SSL 被木马软件干扰，无法启动");
                System.Windows.Application.Current.Shutdown();
                return;
            }

            /*
            Process current = Process.GetCurrentProcess();
            string name = current.ProcessName;  // "dp2SSL"
            */

            _errorTable = new ErrorTable((s) =>
            {
                this.Error = s;
            });

            WpfClientInfo.TypeOfProgram = typeof(App);
            if (StringUtil.IsDevelopMode() == false)
                WpfClientInfo.PrepareCatchException();

            try
            {
                WpfClientInfo.Initial("dp2SSL");
            }
            catch (Exception ex)
            {
                StartErrorBox(ex.Message);
                App.Current.Shutdown();
                return;
            }
            base.OnStartup(e);

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            // InitialFingerPrint();

            // 后台自动检查更新
            var task = Task.Run(() =>
            {
                try
                {
                    NormalResult result = WpfClientInfo.InstallUpdateSync();
                    if (result.Value == -1)
                        OutputHistory("自动更新出错: " + result.ErrorInfo, 2);
                    else if (result.Value == 1)
                    {
                        OutputHistory(result.ErrorInfo, 1);
                        Updated?.Invoke(this, new UpdatedEventArgs { Message = result.ErrorInfo });
                        // MessageBox.Show(result.ErrorInfo);
                    }
                    else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                        OutputHistory(result.ErrorInfo, 0);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"后台 ClickOnce 自动升级出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });

#if REMOVED
            // 用于重试初始化指纹环境的 Timer
            // https://stackoverflow.com/questions/13396582/wpf-user-control-throws-design-time-exception
            _timer = new System.Threading.Timer(
    new System.Threading.TimerCallback(timerCallback),
    null,
    TimeSpan.FromSeconds(10),
    TimeSpan.FromSeconds(60));
#endif
            FingerprintManager.Base.Name = "指纹中心";
            FingerprintManager.Url = App.FingerprintUrl;
            FingerprintManager.SetError += FingerprintManager_SetError;
            WpfClientInfo.WriteInfoLog("FingerprintManager.Start()");
            FingerprintManager.Start(_cancelApp.Token);

            FaceManager.Base.Name = "人脸中心";
            FaceManager.Url = App.FaceUrl;
            FaceManager.SetError += FaceManager_SetError;
            FaceManager.Start(_cancelApp.Token);

            // 自动删除以前残留在 UserDir 中的全部临时文件
            // 用 await 是需要删除完以后再返回，这样才能让后面的 PageMenu 页面开始使用临时文件目录
            await Task.Run(() =>
            {
                DeleteLastTempFiles();
            });

            StartProcessManager();

            /*
            // TODO: 检查网络情况。提示是否允许断网情况下进行初始化
            if (App.Function == "智能书柜")
                SelectMode();
            */

            BeginCheckServerUID(_cancelApp.Token);

            /*
            // 
            InitialShelfCfg();
            */

            /*
            RfidManager.Base.Name = "RFID 中心";
            RfidManager.EnableBase2();
            RfidManager.Url = App.RfidUrl;
            // RfidManager.AntennaList = "1|2|3|4";    // TODO: 从 shelf.xml 中归纳出天线号范围
            RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;

            RfidManager.ListLocks += ShelfData.RfidManager_ListLocks;

            // 2019/12/17
            // 智能书柜一开始假定全部门关闭，所以不需要对任何图书读卡器进行盘点
            if (App.Function == "智能书柜")
                RfidManager.ReaderNameList = "";

            WpfClientInfo.WriteInfoLog("FingerprintManager.Start()");
            RfidManager.Start(_cancelRefresh.Token);
            if (App.Function == "智能书柜")
            {
                WpfClientInfo.WriteInfoLog("RfidManager.StartBase2()");
                RfidManager.StartBase2(_cancelRefresh.Token);
            }
            */

            _barcodeCapture.InputLine += _barcodeCapture_inputLine;
            //_barcodeCapture.InputChar += _barcodeCapture_InputChar;
            _barcodeCapture.Handled = _pauseBarcodeScan == 0;   // 是否把处理过的字符吞掉
            _barcodeCapture.Start();

            {
                try
                {
                    if (App.Current != null && App.Current.MainWindow != null)
                        InputMethod.SetPreferredImeState(App.Current.MainWindow, InputMethodState.Off);
                }
                catch
                {

                }
            }

            /*
            // TODO: 注意，从自助借还状态切换到智能书柜状态，需要补充执行以下一段
            if (App.Function == "智能书柜"
                && string.IsNullOrEmpty(messageServerUrl) == false)
            {
                await TinyServer.InitialMessageQueueAsync(
    System.IO.Path.Combine(WpfClientInfo.UserDir, "mq.db"),
    _cancelRefresh.Token);

                // 这里要等待连接完成，因为后面初始化时候需要发出点对点消息。TODO: 是否要显示一个对话框请用户等待？
                await ConnectMessageServerAsync();

                await TinyServer.DeleteAllResultsetAsync();
                TinyServer.StartSendTask(_cancelRefresh.Token);
                PageShelf.TrySetMessage(null, "我这台智能书柜启动了！");

                ShelfData.StartMonitorTask();
            }
            */

            // 2020/7/5
            if (App.Function != "智能书柜")
            {
                App.InitialRfidManager();

                // 2020/7/31
                if (App.Protocol == "sip")
                    SipChannelUtil.StartMonitorTask();

                // 2020/8/20
                GlobalMonitor.StartMonitorTask();

                // 获得 RFID 配置信息和 图书馆名
                _ = Task.Run(() =>
                {
                    try
                    {
                        var result = LibraryChannelUtil.GetRfidCfg();
                        LibraryName = result.LibraryName;
                    }
                    catch
                    {

                    }
                });

            }

            {
                string binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string stateFileName = Path.Combine(binDir, "dp2ssl_started");
                File.WriteAllText(stateFileName, "dp2ssl started");
            }

        }

        static string _libraryName;
        public static string LibraryName
        {
            get { return _libraryName; }
            set
            {
                _libraryName = value;
                PageMenu.MenuPage.SetLibraryName(value);
            }
        }

        static string _rfidType = "";   // ""/自助借还/智能书柜

        public static void InitialRfidManager()
        {
            if (_rfidType == App.Function)
                return;

            {
                _cancelRfid?.Cancel();
                _cancelRfid?.Dispose();
                _cancelRfid = new CancellationTokenSource();

                RfidManager.SetError -= RfidManager_SetError;
                RfidManager.ListTags -= RfidManager_ListTags;
                RfidManager.ListLocks -= ShelfData.RfidManager_ListLocks;
            }

            RfidManager.Base.Name = "RFID 中心";
            RfidManager.EnableBase2();
            RfidManager.Url = App.RfidUrl;
            // RfidManager.AntennaList = "1|2|3|4";    // TODO: 从 shelf.xml 中归纳出天线号范围

            RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;
            RfidManager.ListLocks += ShelfData.RfidManager_ListLocks;

            // 2019/12/17
            // 智能书柜一开始假定全部门关闭，所以不需要对任何图书读卡器进行盘点
            if (App.Function == "智能书柜")
                RfidManager.ReaderNameList = "";

            WpfClientInfo.WriteInfoLog("FingerprintManager.Start()");
            RfidManager.Start(_cancelRfid.Token);
            if (App.Function == "智能书柜")
            {
                WpfClientInfo.WriteInfoLog("RfidManager.StartBase2()");
                RfidManager.StartBase2(_cancelRfid.Token);
            }

            _rfidType = App.Function;
        }

        static bool _shelfPrepared = false;

        // 为智能书柜执行一些初始化操作
        public static async Task<NormalResult> PrepareShelfAsync()
        {
            if (App.Function != "智能书柜")
                return new NormalResult();

            if (_shelfPrepared == true)
                return new NormalResult();

            ProgressWindow progress = null;
            App.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.TitleText = "dp2SSL -- 智能书柜";
                progress.MessageText = "正在启动应用，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += (s, e) =>
                {
                    // cancel.Cancel();
                };
                progress.okButton.Visibility = Visibility.Collapsed;
                // progress.okButton.Content = "停止";
                App.SetSize(progress, "middle");
                progress.BackColor = "green";
                progress.Show();
            }));

            try
            {

                // TODO: 注意，从自助借还状态切换到智能书柜状态，需要补充执行以下一段
                if (App.Function == "智能书柜"
                    && string.IsNullOrEmpty(messageServerUrl) == false)
                {
                    await TinyServer.InitialMessageQueueAsync(
        System.IO.Path.Combine(WpfClientInfo.UserDir, "mq.db"),
        _cancelApp.Token);

                    // 这里要等待连接完成，因为后面初始化时候需要发出点对点消息。TODO: 是否要显示一个对话框请用户等待？
                    await ConnectMessageServerAsync();

                    await TinyServer.DeleteAllResultsetAsync();
                    TinyServer.StartSendTask(_cancelApp.Token);
                    PageShelf.TrySetMessage(null, "我这台智能书柜启动了！");

                    ShelfData.StartMonitorTask();
                }

                SelectMode();

                InitialShelfCfg();

                InitialRfidManager();

                // 首次显示以前遗留的 LED 文字
                if (string.IsNullOrEmpty(App.LedText) == false)
                {
                    try
                    {
                        await TinyServer.LedDisplay(App.LedText, null);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"LedDisplay() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }

                _shelfPrepared = true;
                return new NormalResult();
            }
            finally
            {
                App.Invoke(new Action(() =>
                {
                    progress.Close();
                }));
            }
        }

        public static void TriggerUpdated(string message)
        {
            // 让版本号文字的背景变成深黄色
            PageMenu.MenuPage.ShowUpdated();

            Updated?.Invoke(null, new UpdatedEventArgs { Message = message });
        }

        static void StartErrorBox(string message)
        {
            MessageBox.Show(message,
    "dp2SSL 启动出错",
    MessageBoxButton.OK,
    MessageBoxImage.Error,
    MessageBoxResult.OK,
    MessageBoxOptions.ServiceNotification);
        }

        /*
        static ManualResetEvent _messageServerConnected = new ManualResetEvent(false);
        */

        public static async Task ConnectMessageServerAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(messageServerUrl) == false)
                {
                    //_messageServerConnected.Reset();

                    TinyServer.CloseConnection();
                    var result = await TinyServer.ConnectAsync(messageServerUrl, messageUserName, messagePassword, "");
                    if (result.Value == -1)
                        WpfClientInfo.WriteErrorLog($"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");
                    else
                    {
                        /*
                        var prepare_result = await TinyServer.PrepareGroupNames();
                        if (prepare_result.Value == -1)
                            WpfClientInfo.WriteErrorLog($"准备群名失败: {prepare_result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={prepare_result.ErrorCode}");
                            */
                    }
                    //_messageServerConnected.Set();
                }
                else
                {
                    // _messageServerConnected.Set();
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"ConnectMessageServer() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                // _messageServerConnected.Set();
            }
        }

        /*
        public static void WaitMessageServerConnected()
        {
            _messageServerConnected.WaitOne();
        }
        */

        // 确保连接到消息服务器
        public async Task EnsureConnectMessageServerAsync()
        {
            if (string.IsNullOrEmpty(messageServerUrl) == false
                && TinyServer.IsDisconnected)
            {
                var result = await TinyServer.ConnectAsync(messageServerUrl, messageUserName, messagePassword, "");
                if (result.Value == -1)
                    WpfClientInfo.WriteErrorLog($"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");
                else
                {
                    /*
                    var prepare_result = await TinyServer.PrepareGroupNames();
                    if (prepare_result.Value == -1)
                        WpfClientInfo.WriteErrorLog($"准备群名失败: {prepare_result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={prepare_result.ErrorCode}");
                        */
                }
            }
        }

        public static bool IsPageBorrowActive { get; set; }
        public static bool IsPageShelfActive { get; set; }

        // 比 App.Funcion == "智能书柜" 判断起来更快
        static bool _isShelfMode = false;

        // 可能会在全局错误 "cfg" 中设置出错信息
        public static void InitialShelfCfg()
        {
            if (App.Function == "智能书柜")
            {
                try
                {
                    _isShelfMode = true;
                    var result = ShelfData.InitialShelf();
                    if (result.Value == -1)
                        SetError("cfg", result.ErrorInfo);
                    else
                        SetError("cfg", null);
                }
                catch (FileNotFoundException)
                {
                    SetError("cfg", $"尚未配置 shelf.xml 文件");
                }
                catch (Exception ex)
                {
                    SetError("cfg", $"InitialShelf() 出现异常:{ex.Message}");
                }
            }
            else
                _isShelfMode = false;
        }

        private void _barcodeCapture_InputChar(CharInput input)
        {
            if (_pauseBarcodeScan > 0)
            {
                Debug.WriteLine("pauseBarcodeScan");
                return;
            }

            CharFeed?.Invoke(this, new CharFeedEventArgs { CharInput = input });
        }

        class LastBarcode
        {
            public string Barcode { get; set; }
            public DateTime Time { get; set; }
        }

        LastBarcode _lastBarcode = null;
        static TimeSpan _repeatLimit = TimeSpan.FromSeconds(3);

        private void _barcodeCapture_inputLine(BarcodeCapture.StringInput input)
        {
            if (_pauseBarcodeScan > 0)
            {
                Debug.WriteLine("pauseBarcodeScan");
                return;
            }

            Debug.WriteLine($"input.Barcode='{input.Barcode}'");

            {
                string line = input.Barcode.TrimEnd(new char[] { '\r', '\n' });
                Debug.WriteLine($"line feed. line='{line}'");
                if (string.IsNullOrEmpty(line) == false)
                {
                    // 检查和上次输入是否重复
                    if (_lastBarcode != null
                        && _lastBarcode.Barcode == line
                        && DateTime.Now - _lastBarcode.Time <= _repeatLimit)
                    {
                        Debug.WriteLine("密集重复输入被忽略");
                        // App.CurrentApp.Speak("重复扫入被忽略");
                        _lastBarcode = new LastBarcode { Barcode = line, Time = DateTime.Now };
                        return;
                    }

                    _lastBarcode = new LastBarcode { Barcode = line, Time = DateTime.Now };
                    // 触发一次输入
                    LineFeed?.Invoke(this, new LineFeedEventArgs { Text = line });
                }
            }
        }

        public static void PauseBarcodeScan()
        {
            _pauseBarcodeScan++;
            Debug.WriteLine($"Pause() _pauseBarcodeScan={_pauseBarcodeScan}");
            // _barcodeCapture.Handled = _pauseBarcodeScan == 0;  // 若 > 0 就不吞掉击键
            UpdateHandled();
        }

        public static void ContinueBarcodeScan()
        {
            _pauseBarcodeScan--;
            if (_pauseBarcodeScan <= -1)
            {
                Debug.Assert(false, "");
            }
            Debug.WriteLine($"Continue() _pauseBarcodeScan={_pauseBarcodeScan}");
            // _barcodeCapture.Handled = _pauseBarcodeScan == 0;   // 若回到 0 就会吞掉击键
            UpdateHandled();
        }

        static void UpdateHandled()
        {
            if (_appActivated == false)
                _barcodeCapture.Handled = false;
            else
                _barcodeCapture.Handled = _pauseBarcodeScan == 0;   // 若回到 0 就会吞掉击键
        }

        StringBuilder _line = new StringBuilder();
        static BarcodeCapture _barcodeCapture = new BarcodeCapture();
        // 是否暂停接收扫条码输入。> 0 表示暂停
        static int _pauseBarcodeScan = 0;

        // 单独的线程，监控 server UID 关系
        public void BeginCheckServerUID(CancellationToken token)
        {
            // 刚开始 5 分钟内频繁检查
            DateTime start = DateTime.Now;

            var task1 = Task.Run(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        var result = PageSetting.CheckServerUID();
                        if (result.Value == -1)
                            SetError("uid", result.ErrorInfo);
                        else
                            SetError("uid", null);

                        if (DateTime.Now - start < TimeSpan.FromMinutes(5))
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        else
                            await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            });
        }

        public void StartProcessManager()
        {
            // 停止前一次的 monitor
            if (_cancelProcessMonitor != null)
            {
                _cancelProcessMonitor.Cancel();
                _cancelProcessMonitor.Dispose();

                _cancelProcessMonitor = new CancellationTokenSource();
            }

            if (ProcessMonitor == true)
            {
                List<ProcessInfo> infos = new List<ProcessInfo>();
                if (string.IsNullOrEmpty(App.FaceUrl) == false
                    && ProcessManager.IsIpcUrl(App.FaceUrl))
                    infos.Add(new ProcessInfo
                    {
                        Name = "人脸中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-人脸中心",
                        MutexName = "{E343F372-13A0-482F-9784-9865B112C042}"
                    });
                if (string.IsNullOrEmpty(App.RfidUrl) == false
                    && ProcessManager.IsIpcUrl(App.RfidUrl))
                    infos.Add(new ProcessInfo
                    {
                        Name = "RFID中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-RFID中心",
                        MutexName = "{CF1B7B4A-C7ED-4DB8-B5CC-59A067880F92}"
                    });
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false
                    && ProcessManager.IsIpcUrl(App.FingerprintUrl))
                    infos.Add(new ProcessInfo
                    {
                        Name = "指纹中心",
                        ShortcutPath = "DigitalPlatform/dp2 V3/dp2-指纹中心",
                        MutexName = "{75FB942B-5E25-4228-9093-D220FFEDB33C}"
                    });
                ProcessManager.Start(infos,
                    (info, text) =>
                    {
                        WpfClientInfo.WriteInfoLog($"{info.Name} {text}");
                    },
                    _cancelProcessMonitor.Token);
            }
        }

        // 异常：捕获了全部异常
        void DeleteLastTempFiles()
        {
            try
            {
                PathUtil.ClearDir(WpfClientInfo.UserTempDir);
            }
            catch (Exception ex)
            {
                this.AddErrors("global", new List<string> { $"清除上次遗留的临时文件时出现异常: {ex.Message}" });
            }
        }

        private void FaceManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("face", e.Error);
        }

        private static void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("rfid", e.Error);
            // 2019/12/15
            // 注意这里的错误信息可能会洪水般冲来，可能会把磁盘空间占满
            //if (e.Error != null)
            //    WpfClientInfo.WriteErrorLog($"RfidManager 出错: {e.Error}");
        }

        private void FingerprintManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("fingerprint", e.Error);
        }

        // TODO: 如何显示后台任务执行信息? 可以考虑只让管理者看到
        public void OutputHistory(string strText, int nWarningLevel = 0)
        {
            // OutputText(DateTime.Now.ToShortTimeString() + " " + strText, nWarningLevel);
        }

        // 注：Windows 关机或者重启的时候，会触发 OnSessionEnding 事件，但不会触发 OnExit 事件
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            WpfClientInfo.WriteDebugLog("OnSessionEnding() called");
            WpfClientInfo.Finish();
            WpfClientInfo.WriteDebugLog("End WpfClientInfo.Finish()");

            // ShelfData.SaveRetryActions();

            _cancelApp?.Cancel();
            _cancelProcessMonitor?.Cancel();
            _cancelRfid?.Cancel();
            ShelfData.CancelAll();

            // 最后关灯
            RfidManager.TurnShelfLamp("*", "turnOff");

            PageShelf.TrySetMessage(null, $"我这台智能书柜停止了哟！({e.ReasonSessionEnding})");

            try
            {
                TinyServer.CloseConnection();
            }
            catch
            {

            }

            base.OnSessionEnding(e);
        }


        // 注：Windows 关机或者重启的时候，会触发 OnSessionEnding 事件，但不会触发 OnExit 事件
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        protected async override void OnExit(ExitEventArgs e)
        {
            _barcodeCapture.Stop();
            _barcodeCapture.InputLine -= _barcodeCapture_inputLine;
            //_barcodeCapture.InputChar -= _barcodeCapture_InputChar;

            try
            {
                if (PageMenu.PageShelf != null)
                    await PageMenu.PageShelf?.SubmitAsync(true);
            }
            catch (NullReferenceException)
            {

            }

            WpfClientInfo.WriteDebugLog("OnExit() called");
            WpfClientInfo.Finish();
            WpfClientInfo.WriteDebugLog("End WpfClientInfo.Finish()");

            // ShelfData.SaveRetryActions();

            try
            {
                // 尝试抢先直接发送
                _ = TinyServer.InnerSetMessageAsync(null, $"我这台智能书柜退出了哟！");
            }
            catch
            {
                // 如果直接发送不成功，则送入 MessageQueue 中
                PageShelf.TrySetMessage(null, $"我这台智能书柜退出了哟！");
            }

            try
            {
                TinyServer.CloseConnection();
            }
            catch
            {

            }

            _cancelApp?.Cancel();
            _cancelProcessMonitor?.Cancel();
            _cancelRfid?.Cancel();
            ShelfData.CancelAll();

            // EndFingerprint();

            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this._channelPool.Close();

            // 最后关灯
            RfidManager.TurnShelfLamp("*", "turnOff");

            base.OnExit(e);
        }

        public static App CurrentApp
        {
            get
            {
                return ((App)Application.Current);
            }
        }

        public void ClearChannelPool()
        {
            this._channelPool.Clear();
        }

        #region dp2library 服务器有关

        public static string dp2ServerUrl
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "dp2ServerUrl", "");
            }
        }


        public static string dp2UserName
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "dp2UserName", "");
            }
        }

        public static string dp2Password
        {
            get
            {
                return DecryptPasssword(WpfClientInfo.Config.Get("global", "dp2Password", ""));
            }
        }

        #endregion

        // 当前采用的通讯协议
        public static string Protocol
        {
            get
            {
                if (string.IsNullOrEmpty(SipServerUrl) == false)
                    return "sip";
                return "dp2library";
            }
        }

        #region SIP2 服务器有关

        public static string SipServerUrl
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "sipServerUrl", "");
            }
        }


        public static string SipUserName
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "sipUserName", "");
            }
        }

        public static string SipPassword
        {
            get
            {
                return DecryptPasssword(WpfClientInfo.Config.Get("global", "sipPassword", ""));
            }
        }

        #endregion

        #region 消息服务器相关参数

        public static string messageServerUrl
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "messageServerUrl", "");
            }
        }

        public static string messageUserName
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "messageUserName", "");
            }
        }

        public static string messagePassword
        {
            get
            {
                return DecryptPasssword(WpfClientInfo.Config.Get("global", "messagePassword", ""));
            }
        }

        /*
        public static string messageGroupName
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "messageGroupName", "");
            }
        }
        */

        // 检查消息服务器参数配置的有效性
        public NormalResult CheckMessageServerParameters()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrEmpty(messageServerUrl) == false)
            {
                /*
                if (messageServerUrl != "http://dp2003.com:8083/dp2mserver")
                {

                }
                */

                if (string.IsNullOrEmpty(messageUserName))
                    errors.Add("尚未配置消息服务器用户名");
                /*
                if (string.IsNullOrEmpty(messageGroupName))
                    errors.Add("尚未配置消息服务器的组名");
                    */
            }

            if (errors.Count > 0)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = StringUtil.MakePathList(errors, "; ")
                };
            }

            return new NormalResult();
        }

        #endregion

        public static string RfidUrl
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "rfidUrl", "");
            }
        }

        public static string FingerprintUrl
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "fingerprintUrl", "");
            }
        }

        public static string FaceUrl
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "faceUrl", "");
            }
        }

        public static bool FullScreen
        {
            get
            {
                return WpfClientInfo.Config?.GetInt("global", "fullScreen", 1) == 1 ? true : false;
            }
        }

        public static bool AutoTrigger
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", "auto_trigger", false);
            }
        }

        // 身份读卡器是否竖向放置
        public static bool PatronReaderVertical
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", "patron_info_lasting", false);
            }
        }

        /*
        public static bool PatronInfoDelayClear
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", "patron_info_delay_clear", false);
            }
        }
        */

        /*
        public static bool EnablePatronBarcode
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", "enable_patron_barcode", false);
            }
        }
        */

        /*
            sizes.Add("禁用");
            sizes.Add("一维码+二维码");
            sizes.Add("一维码");
            sizes.Add("二维码");
        * */
        public static string PatronBarcodeStyle
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "patron_barcode_style", "禁用");
            }
        }

        /*
            sizes.Add("不打印");
            sizes.Add("借书");
            sizes.Add("借书+还书");
         * */
        public static string PosPrintStyle
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "pos_print_style", "不打印");
            }
        }

        public static bool ProcessMonitor
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return true;

                return (bool)WpfClientInfo.Config?.GetBoolean("global",
                    "process_monitor",
                    true);
            }
        }

        /*
        public static string ShelfLocation
        {
            get
            {
                return WpfClientInfo.Config?.Get("shelf",
                    "location",
                    "");
            }
        }
        */

        public static string Function
        {
            get
            {
                return WpfClientInfo.Config?.Get("global",
    "function",
    "自助借还");
            }
        }

        /*
        public static string CardNumberConvertMethod
        {
            get
            {
                return WpfClientInfo.Config?.Get("global",
    "card_number_convert_method",
    "十六进制");
            }
        }
        */

        /*
        public static bool DetectBookChange
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return true;
                return (bool)WpfClientInfo.Config?.GetBoolean("shelf_operation",
    "detect_book_change",
    false);
            }
        }
        */

        public static string LedText
        {
            get
            {
                return WpfClientInfo.Config?.Get("global",
    "ledText",
    "");
            }
            set
            {
                WpfClientInfo.Config?.Set("global",
    "ledText",
    value);
            }
        }

        public static void SetLockingPassword(string password)
        {
            string strSha1 = Cryptography.GetSHA1(password + "_ok");
            WpfClientInfo.Config.Set("global", "lockingPassword", strSha1);
        }

        public static bool MatchLockingPassword(string password)
        {
            string sha1 = WpfClientInfo.Config.Get("global", "lockingPassword", "");
            string current_sha1 = Cryptography.GetSHA1(password + "_ok");
            if (sha1 == current_sha1)
                return true;
            return false;
        }

        public static bool IsLockingPasswordEmpty()
        {
            string sha1 = WpfClientInfo.Config.Get("global", "lockingPassword", "");
            return (string.IsNullOrEmpty(sha1));
        }

        static string EncryptKey = "dp2ssl_client_password_key";

        public static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        public static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        #region LibraryChannel

        public class Account
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string LibraryCodeList { get; set; } // 馆代码列表

            public static bool IsGlobalUser(string strLibraryCodeList)
            {
                if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                    return true;
                return false;
            }

            public static bool MatchLibraryCode(string strLibraryCode, string strLocationLibraryCode)
            {
                if (IsGlobalUser(strLibraryCode) == true)
                    return true;
                if (strLibraryCode == strLocationLibraryCode)
                    return true;
                return false;
            }
        }

        Dictionary<string, Account> _accounts = new Dictionary<string, Account>();

        public Account FindAccount(string userName)
        {
            if (_accounts.ContainsKey(userName) == false)
                return null;
            return _accounts[userName];
        }

        public void SetAccount(string userName, string password, string libraryCode)
        {
            Account account = null;
            if (_accounts.ContainsKey(userName) == false)
            {
                account = new Account
                {
                    UserName = userName,
                    Password = password,
                    LibraryCodeList = libraryCode,
                };
                _accounts[userName] = account;
            }
            else
            {
                account = _accounts[userName];
                account.Password = password;
            }
        }

        public void RemoveAccount(string userName)
        {
            if (_accounts.ContainsKey(userName))
                _accounts.Remove(userName);
        }

        internal void Channel_BeforeLogin(object sender,
DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            if (e.FirstTry == true)
            {
                // TODO: 从工作人员用户名密码记载里面检查，如果是工作人员账户，则 ...
                Account account = FindAccount(channel.UserName);
                if (account != null)
                {
                    e.UserName = account.UserName;
                    e.Password = account.Password;
                }
                else
                {
                    e.UserName = dp2UserName;

                    // e.Password = this.DecryptPasssword(e.Password);
                    e.Password = dp2Password;

#if NO
                    strPhoneNumber = AppInfo.GetString(
        "default_account",
        "phoneNumber",
        "");
#endif

                    bool bIsReader = false;

                    string strLocation = "";

                    e.Parameters = "location=" + strLocation;
                    if (bIsReader == true)
                        e.Parameters += ",type=reader";
                }

                e.Parameters += ",client=dp2ssl|" + WpfClientInfo.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
                else
                {
                    e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
                    e.Cancel = true;
                }
            }

            // e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
            e.Cancel = true;
        }

        string _currentUserName = "";

        public string ServerUID = "";

        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;
            //_currentUserRights = channel.Rights;
            //_currentLibraryCodeList = channel.LibraryCodeList;
        }

        object _syncRoot_channelList = new object();
        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public void AbortAllChannel()
        {
            lock (_syncRoot_channelList)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel(string strUserName = "")
        {
            string strServerUrl = dp2ServerUrl;

            if (string.IsNullOrEmpty(strUserName))
                strUserName = dp2UserName;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this._channelPool.ReturnChannel(channel);
            lock (_syncRoot_channelList)
            {
                _channelList.Remove(channel);
            }
        }

        SpeechSynthesizer m_speech = new SpeechSynthesizer();
        string m_strSpeakContent = "";

        public void Speak(string strText, bool bError = false)
        {
            if (this.m_speech == null)
                return;

            //if (strText == this.m_strSpeakContent)
            //    return; // 正在说同样的句子，不必打断

            this.m_strSpeakContent = strText;

            try
            {
                this.m_speech.SpeakAsyncCancelAll();
                this.m_speech.SpeakAsync(strText);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // TODO: 如何报错?
            }
        }

        public void SpeakSequence(string strText, bool bError = false)
        {
            if (this.m_speech == null)
                return;

            this.m_strSpeakContent = strText;
            try
            {
                // this.m_speech.SpeakAsyncCancelAll();
                this.m_speech.SpeakAsync(strText);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // TODO: 如何报错?
            }
        }

        static bool _appActivated = false;

        protected override void OnActivated(EventArgs e)
        {
            // dp2ssl 活动起来以后，要接受扫码，并且要吞掉击键
            // ContinueBarcodeScan();
            _appActivated = true;
            UpdateHandled();

            // 单独线程执行，避免阻塞 OnActivated() 返回
            _ = Task.Run(() =>
            {
                try
                {
                    FingerprintManager.EnableSendkey(false);
                    RfidManager.EnableSendkey(false);
                }
                catch
                {

                }
            });
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            // dp2ssl 失去活动以后，要不接受扫码，并且要不吞掉击键
            // PauseBarcodeScan();
            _appActivated = false;
            UpdateHandled();

            // Speak("DeActivated");
            base.OnDeactivated(e);
        }

        #endregion

        public void AddErrors(string type, List<string> errors)
        {
            DateTime now = DateTime.Now;
            List<string> results = new List<string>();
            foreach (string error in errors)
            {
                results.Add($"{now.ToShortTimeString()} {error}");
            }

            _errorTable.SetError(type, StringUtil.MakePathList(results, "; "));
        }

        public static void SetError(string type, string error)
        {
            /*
            if (type == "face" && error != null)
            {
                Debug.Assert(false, "");
            }
            */

            _errorTable.SetError(type, error);
        }

        public static void ClearErrors(string type)
        {
            // _errors.Clear();
            _errorTable.SetError(type, "");
        }

        public string GetError(string type)
        {
            return _errorTable.GetError(type);
        }

        public static event TagChangedEventHandler TagChanged = null;

        // 新版本的事件
        public static event NewTagChangedEventHandler NewTagChanged = null;

        // public event SetErrorEventHandler TagSetError = null;

        private static void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 2020/7/22
            if (e.Result.Value == -1)
            {
                SetError("rfid", e.Result.ErrorInfo);
            }

            // 标签总数显示
            // this.Number = e.Result?.Results?.Count.ToString();
            if (e.Result.Results != null)
            {
                // TODO: 如果 IsPageShelfActive 和 IsPageBorrowActive 都为 false，则要看 Function 是什么决定如何显示标签数
                bool isShelf = IsPageShelfActive;
                bool isBorrow = IsPageBorrowActive;
                if ((isShelf == false && isBorrow == false)
                    || (isShelf == true && isBorrow == true))
                {
                    // TODO: 这一句是否需要 catch 一下
                    isShelf = Function == "智能书柜";
                    isBorrow = !isShelf;
                }

                // bool numberShown = false;

                if (isShelf)
                {
                    NewTagList.Refresh(// sender as BaseChannel<IRfid>,
                        e.ReaderNameList,
                        e.Result.Results,
                        (readerName, uid, antennaID) =>
                        {
                            var channel = sender as BaseChannel<IRfid>;
                            if (channel.Started == false)
                                return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };
                            return channel.Object.GetTagInfo(readerName, uid, antennaID);
                        },
                        (add_tags, update_tags, remove_tags) =>
                        {
                            NewTagChanged?.Invoke(sender, new NewTagChangedEventArgs
                            {
                                AddTags = add_tags,
                                UpdateTags = update_tags,
                                RemoveTags = remove_tags,
                            });
                        },
                        (type, text) =>
                        {
                            RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                        });

                    // 标签总数显示 只显示标签数，不再区分图书标签和读者卡
                    if (CurrentApp != null)
                        CurrentApp.Number = $"{NewTagList.Tags.Count}";
                    //numberShown = true;
                }

                if (isBorrow == true/* || numberShown == false*/)
                {
                    TagList.Refresh(sender as BaseChannel<IRfid>,
                        e.ReaderNameList,
                        e.Result.Results,
                            (add_books, update_books, remove_books, add_patrons, update_patrons, remove_patrons) =>
                            {
                                TagChanged?.Invoke(sender, new TagChangedEventArgs
                                {
                                    AddBooks = add_books,
                                    UpdateBooks = update_books,
                                    RemoveBooks = remove_books,
                                    AddPatrons = add_patrons,
                                    UpdatePatrons = update_patrons,
                                    RemovePatrons = remove_patrons
                                });
                            },
                            (type, text) =>
                            {
                                RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                                // TagSetError?.Invoke(this, new SetErrorEventArgs { Error = text });
                            });

                    // 标签总数显示 图书+读者卡
                    if (CurrentApp != null)
                        CurrentApp.Number = $"{TagList.Books.Count}:{TagList.Patrons.Count}";
                    //numberShown = true;
                }
            }
        }

        public static string StartNetworkMode = ""; // 空/local

        static void SelectMode()
        {
            // 观察命令行参数
            bool silently = IsSilently() || IsFileSilently();

            ShelfData.DetectLibraryNetwork();
            if (ShelfData.LibraryNetworkCondition != "OK")
            {
                if (silently)
                    StartNetworkMode = "local";
                else
                {
#if NO
                    // TODO: 对话框出现的时候，允许点对点远程选择对话框？

                    var result = MessageBox.Show("访问 dp2library 服务器失败。请问是否继续启动？\r\n[Yes] 按照断网模式继续启动; [No] 按照联网模式继续启动; [Cancel] 退出 dp2SSL",
                        "请选择启动模式",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes,
                        MessageBoxOptions.DefaultDesktopOnly);
                    if (result == MessageBoxResult.Yes)
                        StartNetworkMode = "local";
                    else if (result == MessageBoxResult.Cancel)
                        App.Current.Shutdown();
                    else
                        StartNetworkMode = "";
#endif

                    App.Invoke(new Action(() =>
                    {
                        NetworkWindow dlg = new NetworkWindow();
                        //progress.TitleText = "请选择启动模式";
                        //progress.MessageText = "访问 dp2library 服务器失败。请问是否继续启动？";
                        dlg.Owner = Application.Current.MainWindow;
                        dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        dlg.Background = new SolidColorBrush(Colors.DarkRed);
                        // App.SetSize(progress, "wide");
                        // progress.BackColor = "yellow";
                        var ret = dlg.ShowDialog();
                        if (ret == false)
                            App.Current.Shutdown();
                        StartNetworkMode = dlg.Mode;
                    }
                    ));
                }
            }
        }

        // 命令行参数里面是否包含了静默初始化？
        public static bool IsSilently()
        {
            string[] args = Environment.GetCommandLineArgs();
            WpfClientInfo.WriteInfoLog($"dp2ssl 命令行参数为: '{string.Join(" ", args)}'");
            int i = 0;
            foreach (string arg in args)
            {
                if (i > 0
                    && (arg == "silently" || arg == "silent" || arg == "silence"))
                    return true;
                i++;
            }

            return false;
        }

        // 一次性参数文件里面是否包含了静默初始化？
        public static bool IsFileSilently()
        {
            try
            {
                string binDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string fileName = System.IO.Path.Combine(binDir, "cmdlineparam.txt");
                if (File.Exists(fileName) == false)
                {
                    WpfClientInfo.WriteInfoLog($"{fileName} 文件不存在");
                    return false;
                }
                string content = File.ReadAllText(fileName);
                WpfClientInfo.WriteInfoLog($"{fileName} 文件内容:'{content}'");
                File.Delete(fileName);  // 用完就删除
                var args = StringUtil.SplitList(content, " ");
                foreach (string arg in args)
                {
                    if (arg == "silently" || arg == "silent" || arg == "silence")
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"从命令行参数文件中读取信息时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return false;
            }
        }

        // 中途尝试切换为本地模式
        // return:
        //      false   没有发生切换
        //      true    发生了切换
        public static bool TrySwitchToLocalMode()
        {
            if (StartNetworkMode == "local")
                return false;

            ShelfData.DetectLibraryNetwork();
            if (ShelfData.LibraryNetworkCondition != "OK")
            {
                StartNetworkMode = "local";
                return true;
            }

            return false;
        }

        public static void SetSize(Window window, string style)
        {
            var mainWindows = App.CurrentApp.MainWindow;
            if (style == "tall")
            {
                window.Width = Math.Min(700, mainWindows.ActualWidth * 0.95);
                window.Height = Math.Min(900, mainWindows.ActualHeight * .95);
            }
            else if (style == "middle")
            {
                window.Width = Math.Min(700, mainWindows.ActualWidth * 0.95);
                window.Height = Math.Min(500, mainWindows.ActualHeight * .95);
            }
            else if (style == "wide")
            {
                window.Width = Math.Min(1000, mainWindows.ActualWidth * 0.95);
                window.Height = Math.Min(700, mainWindows.ActualHeight * .95);
            }
            else
            {
                window.Width = Math.Min(700, mainWindows.ActualWidth * 0.95);
                window.Height = Math.Min(500, mainWindows.ActualHeight * .95);
            }
        }

        static ProgressWindow _errorWindow = null;

        public static void OpenErrorWindow(string text)
        {
            CloseErrorWindow();

            App.Invoke(new Action(() =>
            {
                _errorWindow = new ProgressWindow();
                _errorWindow.TitleText = "系统出现故障";
                _errorWindow.MessageText = text + "\r\n\r\n请联系管理员排除故障";
                _errorWindow.Owner = Application.Current.MainWindow;
                _errorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _errorWindow.Closed += (s, e) =>
                {
                    _errorWindow = null;
                };
                _errorWindow.okButton.Content = "确定";
                _errorWindow.Background = new SolidColorBrush(Colors.DarkRed);
                App.SetSize(_errorWindow, "wide");
                _errorWindow.BackColor = "yellow";
                _errorWindow.Show();
            }));
        }

        public static void CloseErrorWindow()
        {
            if (_errorWindow != null)
            {
                App.Invoke(new Action(() =>
                {
                    _errorWindow.Close();
                    _errorWindow = null;
                }));
            }
        }


        // 紫外杀菌
        public async Task SterilampAsync()
        {
            ProgressWindow progress = null;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancelApp.Token))
            {
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "紫外线消毒";
                    progress.MessageText = "警告：紫外线对眼睛和皮肤有害";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "wide");
                    progress.BackColor = "yellow";
                    progress.Show();
                }));

                PageShelf.TrySetMessage(null, "即将开始紫外线消毒，正在倒计时 ...");

                try
                {
                    // 首先倒计时警告远离
                    App.CurrentApp.Speak("即将开始紫外线消毒，请马上远离书柜");
                    for (int i = 20; i > 0; i--)
                    {
                        if (cancel.Token.IsCancellationRequested)
                            return;
                        string text = $"({i}) 即将进行紫外线消毒，请迅速远离书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = text;
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                    }

                    App.Invoke(new Action(() =>
                    {
                        progress.BackColor = "red";
                        progress.MessageText = "正在进行紫外线消毒，请不要靠近书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                    }));

                    PageShelf.TrySetMessage(null, "正在进行紫外线消毒，请不要靠近书柜");

                    // TODO: 屏幕上可以显示剩余时间
                    // TODO: 背景色动画，闪动
                    RfidManager.TurnSterilamp("*", "turnOn");
                    DateTime end = DateTime.Now + TimeSpan.FromMinutes(10);
                    for (int i = 0; i < 3 * 10; i++)    // 10 分钟
                    {
                        App.CurrentApp.SpeakSequence("正在进行紫外线消毒，紫外灯对眼睛和皮肤有害，请不要靠近书柜");
                        if (cancel.Token.IsCancellationRequested)
                            break;
                        string timeText = $"剩余 {Convert.ToInt32((end - DateTime.Now).TotalMinutes)} 分钟";

                        if ((i % 3) == 0)
                            App.CurrentApp.SpeakSequence(timeText);

                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = $"({timeText}) 正在进行紫外线消毒，请不要靠近书柜\r\n\r\n警告：紫外线对眼睛和皮肤有害";
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(20), cancel.Token);
                    }
                }
                finally
                {
                    RfidManager.TurnSterilamp("*", "turnOff");
                    App.CurrentApp.Speak("紫外灯已关闭");
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));

                    PageShelf.TrySetMessage(null, "紫外线消毒结束");
                }
            }
        }

        public static void Invoke(Action action)
        {
            /*
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                action.Invoke();
            });
            */

            Current.Dispatcher?.Invoke(action);
        }

#if NO
        // 安装为绿色版本
        public static async Task InstallGreenAsync()
        {
            if (StringUtil.IsDevelopMode() == false
                && ApplicationDeployment.IsNetworkDeployed == false)
            {
                ErrorBox("当前已经是绿色版本了");
                return;
            }

            ProgressWindow progress = null;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancelRefresh.Token))
            {
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "安装为绿色版本";
                    progress.MessageText = "请等待";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "wide");
                    progress.BackColor = "yellow";
                    progress.Show();
                }));

                try
                {
                    var result = await GreenInstaller.InstallFromWeb("http://dp2003.com/dp2ssl/v1_dev",
    "c:\\dp2ssl",
    null,
    // "dp2ssl.exe",
    false,
    (double min, double max, double value, string text) =>
    {
        App.Invoke(new Action(() =>
        {
            if (text != null)
                progress.MessageText = text;
        }));
    });
                    if (result.Value == -1)
                    {
                        ErrorBox(result.ErrorInfo);
                        return;
                    }
                    // 迁移用户文件夹
                    
                    string sourceDirectory = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
"dp2ssl");
                    string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                    var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                        targetDirectory,
                        "maskSource");
                    return;
                }
                finally
                {

                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            }
        }

#endif

        public static void ErrorBox(string message,
    string color = "red",
    string style = "")
        {
            ProgressWindow progress = null;

            App.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.MessageText = "正在处理，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                App.SetSize(progress, "tall");
                //progress.Width = Math.Min(700, this.ActualWidth);
                //progress.Height = Math.Min(900, this.ActualHeight);
                progress.Closed += (o, e) =>
                {
                };
                if (StringUtil.IsInList("button_ok", style))
                    progress.okButton.Content = "确定";
                progress.Show();
                // AddLayer();
            }));


            if (StringUtil.IsInList("auto_close", style))
            {
                App.Invoke(new Action(() =>
                {
                    progress.MessageText = message;
                    if (string.IsNullOrEmpty(color) == false)
                        progress.BackColor = color;
                }));

                _ = Task.Run(async () =>
                {
                    try
                    {
                        // TODO: 显示倒计时计数？
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        App.Invoke(new Action(() =>
                        {
                            progress.Close();
                        }));
                    }
                    catch
                    {
                        // TODO: 写入错误日志
                    }
                });
            }
            else
                App.Invoke(new Action(() =>
                {
                    progress.MessageText = message;
                    progress.BackColor = color;
                }));
        }

        // 显示当前对话框的内容
        public static void SendDialogText(Window window, string title)
        {
            string text = "";
            App.Invoke(new Action(() =>
            {
                if (window == null)
                    window = App.GetActiveWindow();

                if (window == null)
                    text = "";
                else
                    text = App.FindTextChildren(window);
            }));
            PageShelf.TrySetMessage(null, $"==== {title} 对话框显示并等待输入 ====\r\n{text}");
        }

        public static NormalResult PressButton(string button_name)
        {
            var window = GetActiveWindow();
            if (window == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "没有找到活动窗口",
                    ErrorCode = "notFoundActiveWindow"
                };
            foreach (var button in FindVisualChildren<Button>(window))
            {
                string name = button.Content as string;
                if (name == null || name.IndexOf(button_name) == -1)
                    continue;

                PressButton(button);
                return new NormalResult { Value = 1 };
            }

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = $"窗口 {window.Title} 中没有找到名为 '{button_name}' 的按钮",
                ErrorCode = "notFoundButton"
            };
        }

        // 获得当前最顶部的窗口
        public static Window GetActiveWindow()
        {
            try
            {
                return SortWindowsTopToBottom(Application.Current.Windows.OfType<Window>()).FirstOrDefault();

                /*
            // https://stackoverflow.com/questions/2038879/refer-to-active-window-in-wpf
            return Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                */
            }
            catch
            {
                return null;
            }
        }

        // https://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }

        }

        // 按下一个按钮
        // https://stackoverflow.com/questions/728432/how-to-programmatically-click-a-button-in-wpf
        static void PressButton(Button someButton)
        {
            ButtonAutomationPeer peer = new ButtonAutomationPeer(someButton);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        public static string FindTextChildren(DependencyObject depObj)
        {
            if (depObj != null)
            {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        if (child is UIElement)
                        {
                            UIElement ui = child as UIElement;
                            if (ui.Visibility != Visibility.Visible)
                                continue;
                        }

                        string current = "";
                        if (child is FlowDocumentScrollViewer)
                        {
                            FlowDocumentScrollViewer viewer = child as FlowDocumentScrollViewer;
                            var fd = viewer.Document;
                            TextRange tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                            current = tr.Text;
                        }
                        /*
                        else if (child is FlowDocument)
                        {
                            FlowDocument fd = child as FlowDocument;
                            TextRange tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                            current = tr.Text;
                        }
                        */
                        else if (child is TextBlock)
                        {
                            TextBlock tb = child as TextBlock;
                            TextRange tr = new TextRange(tb.ContentStart, tb.ContentEnd);
                            current = tr.Text;
                        }
                        else if (child is TextBox)
                        {
                            TextBox tb = child as TextBox;
                            current = tb.Text;
                        }
                        else if (child is Button)
                        {
                            current = FindTextChildren(child);
                            if (string.IsNullOrEmpty(current) == false)
                                current = $"[{current.Replace("\r", "").Replace("\n", "")}]";
                        }
                        else
                            current = FindTextChildren(child);

                        current = Trim(current);
                        if (string.IsNullOrEmpty(current) == false)
                            text.AppendLine(current);
                    }
                }

                return text.ToString();
            }
            return "";
        }

#if NO
        public static string FindTextChildren(DependencyObject depObj)
        {
            if (depObj != null)
            {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        string current = "";
                        if (child is FlowDocument)
                        {
                            FlowDocument fd = child as FlowDocument;
                            TextRange tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                            current = tr.Text;
                        }
                        else if (child is TextBlock)
                        {
                            TextBlock tb = child as TextBlock;
                            TextRange tr = new TextRange(tb.ContentStart, tb.ContentEnd);
                            current = tr.Text;
                        }
                        else if (child is TextBox)
                        {
                            TextBox tb = child as TextBox;
                            current = tb.Text;
                        }
                        else if (child is Button)
                        {
                            current = FindTextChildren(child);
                            if (string.IsNullOrEmpty(current) == false)
                                current = $"[{current.Replace("\r","").Replace("\n","")}]";
                        }
                        else
                            current = FindTextChildren(child);

                        current = Trim(current);
                        if (string.IsNullOrEmpty(current) == false)
                            text.AppendLine(current);
                    }
                }

                return text.ToString();
            }
            return "";
        }
#endif
        static string Trim(string text)
        {
            if (text == null)
                return "";
            return text.Trim(new char[] { ' ', '\r', '\n' });
        }


        #region z-order

        // https://stackoverflow.com/questions/3473016/how-to-sort-windows-by-z-index
        public static IEnumerable<Window> SortWindowsTopToBottom(IEnumerable<Window> unsorted)
        {
            var byHandle = unsorted.ToDictionary(win =>
              ((HwndSource)PresentationSource.FromVisual(win)).Handle);

            for (IntPtr hWnd = GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = GetWindow(hWnd, GW_HWNDNEXT))
            {
                if (byHandle.ContainsKey(hWnd))
                    yield return byHandle[hWnd];
            }
        }

        const uint GW_HWNDNEXT = 2;
        [DllImport("User32")] static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("User32")] static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);

        #endregion

        #region EdgeUI

        /*
HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\EdgeUI

AllowEdgeSwipe DWORD

(delete) = Enable
0 = Disable
        * */
        // 2020/7/23
        // 禁用 Windows 边沿扫动功能
        // return:
        //      false   继续
        //      true    需要立即退出 Application
        public static bool DisableEdgeUI()
        {
            try
            {
                using (RegistryKey item = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\EdgeUI"))
                {
                    if (item != null)
                    {
                        int? v = item.GetValue("AllowEdgeSwipe", 1) as int?;
                        if (v == 0)
                            return false;
                    }
                }

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                Boolean isRunasAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isRunasAdmin)
                {
                    using (RegistryKey item = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\EdgeUI"))
                    {
                        item.SetValue("AllowEdgeSwipe", 0, RegistryValueKind.DWord);
                    }

                    string[] args = Environment.GetCommandLineArgs();
                    if (args.IndexOf<string>("DisableEdgeUI") != -1)
                    {
                        // MessageBox.Show("registry changed 1 !");
                        App.Current.Shutdown();
                        return true;
                    }
                    // MessageBox.Show("registry changed 2 !");
                }
                else
                {
                    var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                    // The following properties run the new process as administrator
                    processInfo.UseShellExecute = true;
                    processInfo.Verb = "runas";
                    processInfo.Arguments = " DisableEdgeUI";

                    // Start the new process
                    try
                    {
                        Process.Start(processInfo);
                    }
                    catch (Exception)
                    {
                        // MessageBox.Show("dp2ssl 无法以 Administator 身份运行");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"DisableEdgeUI() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return false;
            }
        }

        #endregion
    }

    public delegate void NewTagChangedEventHandler(object sender,
NewTagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class NewTagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddTags { get; set; }
        public List<TagAndData> UpdateTags { get; set; }
        public List<TagAndData> RemoveTags { get; set; }
    }


    public delegate void TagChangedEventHandler(object sender,
TagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class TagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddBooks { get; set; }
        public List<TagAndData> UpdateBooks { get; set; }
        public List<TagAndData> RemoveBooks { get; set; }

        public List<TagAndData> AddPatrons { get; set; }
        public List<TagAndData> UpdatePatrons { get; set; }
        public List<TagAndData> RemovePatrons { get; set; }
    }

    public delegate void LineFeedEventHandler(object sender,
LineFeedEventArgs e);

    /// <summary>
    /// 条码枪输入一行文字的事件的参数
    /// </summary>
    public class LineFeedEventArgs : EventArgs
    {
        public string Text { get; set; }
    }


    public delegate void CharFeedEventHandler(object sender,
CharFeedEventArgs e);

    /// <summary>
    /// 条码枪输入一行文字的事件的参数
    /// </summary>
    public class CharFeedEventArgs : EventArgs
    {
        public CharInput CharInput { get; set; }
    }

    public delegate void UpdatedEventHandler(object sender,
UpdatedEventArgs e);

    /// <summary>
    /// 升级完成的事件的参数
    /// </summary>
    public class UpdatedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
