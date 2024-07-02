﻿using System;
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
using System.Security.AccessControl;

using Microsoft.Win32;
using Microsoft.EntityFrameworkCore.Internal;

using dp2SSL.Models;
using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.IO;
using static DigitalPlatform.IO.BarcodeCapture;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Face;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        // 2023/12/19
        public static Skin Skin { get; set; } = Skin.Dark;

        public NetworkUsage _networkUsage = new NetworkUsage();

        public static event UpdatedEventHandler Updated = null;

        public static event LineFeedEventHandler LineFeed = null;
        public static event CharFeedEventHandler CharFeed = null;

        // 主要的通道池，用于当前服务器
        internal static LibraryChannelPool _channelPool = new LibraryChannelPool();

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

        /*
        // https://blog.danskingdom.com/Catch-and-display-unhandled-exceptions-in-your-WPF-app/
        public App() : base()
        {
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                {
                    AddErrors("global", new List<string> { args.Exception.Message });
                };
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            AddErrors("global", new List<string> { e.Exception.Message });
            e.Handled = true;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            AddErrors("global", new List<string> { e.Exception.Message });
            e.Handled = true;
        }
        */

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        protected async override void OnStartup(StartupEventArgs e)
        {
            bool isAdmin = IsAdministrator();
            // 2020/9/14
            // ClickOnce 版本暂时不自动修改边沿 UI 参数
            if (isAdmin || ApplicationDeployment.IsNetworkDeployed == false)
            {
                var result = DisableEdgeUI();
                if (result.Value == 1)
                    return;
            }

            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                StartErrorBox("dp2SSL 不允许重复启动");
                App.Current.Shutdown();
                return;
            }

            // 检查当前运行的(绿色还是 ClickOnce)类型
            if (CheckRunType() == false)
                return;

            if (DetectVirus.DetectXXX(out _) || DetectVirus.DetectGuanjia(out _))
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

                ReloadSkin();

                ShelfData.UpgradeDatabase();

                // 2021/8/21
                // 把错误日志同时也发送给 dp2mserver
                WpfClientInfo.WriteLogEvent += (o1, e1) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(App.messageServerUrl) == false
                        && e1 != null
                        && e1.Level == "error")
                        {
                            ShelfData.TrySetMessage(null, "*** ERROR *** " + e1.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteLogInternal("error", $"WriteLogEvent 内发生异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"OnStartup() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                StartErrorBox(ex.Message);
                App.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            _channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            // InitialFingerPrint();

#if REMOVED
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
#endif

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
            // 2024/1/22
            FingerprintManager.Base.Name = PageBorrow.GetFingerprintCaption() + "中心";
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

                // 2024/1/15
                DeleteBrokenFiles();

                var free_bytes = PageBorrow.GetUserDiskFreeSpace();
                if (free_bytes != -1 && free_bytes < 1024 * 1024 * 1024)
                    PageBorrow.BeginCleanCoverImagesDirectory(DateTime.Now);
                else
                    PageBorrow.BeginCleanCoverImagesDirectory(DateTime.Now - TimeSpan.FromDays(100));   // 清除一百天以前的缓存文件
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

            WpfClientInfo.WriteInfoLog("RfidManager.Start()");
            RfidManager.Start(_cancelRefresh.Token);
            if (App.Function == "智能书柜")
            {
                WpfClientInfo.WriteInfoLog("RfidManager.StartBase2()");
                RfidManager.StartBase2(_cancelRefresh.Token);
            }
            */
            _barcodeCapture.StopKeys = new List<System.Windows.Forms.Keys> { 
                System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Tab,
                System.Windows.Forms.Keys.LWin,
                System.Windows.Forms.Keys.RWin,
                System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete,
            };
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
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"SetPreferredImeState() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
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
                ShelfData.TrySetMessage(null, "我这台智能书柜启动了！");

                ShelfData.StartMonitorTask();
            }
            */

            // 获得 RFID 配置信息和 图书馆名
            _ = Task.Run(() =>
            {
                try
                {
                    var result = LibraryChannelUtil.GetRfidCfg();
                    // WpfClientInfo.WriteInfoLog($"GetRfidCfg() return {result.ToString()}");
                    LibraryName = result.LibraryName;
                    ServerUid = result.ServerUid;

                    if (result.XmlChanged && App.Function == "智能书柜")
                    {
                        WpfClientInfo.WriteInfoLog($"[1] 探测到 library.xml 中 rfid 发生变化。\r\n变化前的: {result.OldXml}\r\n变化后的: {result.Xml}");
                        // 触发重新全量下载册和读者记录
                        ShelfData.TriggerDownloadEntitiesAndPatrons();
                    }
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"GetRfidCfg() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });

            // 2020/7/5
            if (App.Function == "自助借还")
            {
                App.InitialRfidManager();

                // 2020/7/31
                if (App.Protocol == "sip")
                    SipChannelUtil.StartMonitorTask();
            }

            // 2020/8/30
            if (App.Function == "盘点")
            {
                App.InitialRfidManager();

                InventoryData.StartInventoryTask();
            }

            // 2020/8/20
            GlobalMonitor.StartMonitorTask();

            {
                string binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string stateFileName = Path.Combine(binDir, "dp2ssl_started");
                File.WriteAllText(stateFileName, "dp2ssl started");
            }

        }

        void ChangeSkin(Skin newSkin)
        {
            Skin = newSkin;

            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {
                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }
        }

        public void ReloadSkin()
        {
            var current_cfg_skin = SkinName == "暗色" ? Skin.Dark : Skin.Light;
            if (App.Skin != current_cfg_skin)
            {
                PageMenu.ClearPages();
                ChangeSkin(current_cfg_skin);
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

        static string _serverUid;
        public static string ServerUid
        {
            get { return _serverUid; }
            set { _serverUid = value; }
        }

        /*
        public static ManualResetEvent TagListRefreshFinish
        {
            get
            {
                return _refreshFinish;
            }
        }
        static ManualResetEvent _refreshFinish = new ManualResetEvent(false);
        */

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

            if (App.Function == "自助借还")
            {
                RfidManager.SyncSetEAS = true;
                /*
                RfidManager.InventoryIdleSeconds = this.RfidInventoryIdleSeconds;
                RfidManager.GetRSSI = this.UhfRSSI == 0 ? false : true;
                RfidTagList.OnlyReadEPC = this.UhfOnlyEpcCharging;
                */
            }

            RfidManager.Base.Name = "RFID 中心";
            RfidManager.EnableBase2();
            RfidManager.Url = App.RfidUrl;
            RfidTagList.OnlyReadEPC = App.OnlyReadEPC;
            // RfidManager.AntennaList = "1|2|3|4";    // TODO: 从 shelf.xml 中归纳出天线号范围

            RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;
            RfidManager.ListLocks += ShelfData.RfidManager_ListLocks;


            // 2019/12/17
            // 智能书柜一开始假定全部门关闭，所以不需要对任何图书读卡器进行盘点
            if (App.Function == "智能书柜")
            {
                RfidManager.ReaderNameList = "";

                // TODO: 先要设法确定 RfidCenter 已经启动完成。这里有可能因为 RfidCenter 启动较慢，导致对它的 API 请求报错

                // 补一次。先前可能有失败的开关灯动作
                ShelfData.TurnLamp("", "refresh");

#if AUTO_TEST
                ShelfData.InitialSimuTags();
#else
                ShelfData.RestoreRealTags();
#endif
            }

            WpfClientInfo.WriteInfoLog("RfidManager.Start()");
            RfidManager.Start(_cancelRfid.Token);
            if (App.Function == "智能书柜")
            {
                // 智能书柜会为读者证 RFID 卡专门启动一个处理线程
                WpfClientInfo.WriteInfoLog("RfidManager.StartBase2()");
                RfidManager.StartBase2(_cancelRfid.Token);
            }

            _rfidType = App.Function;

            // _refreshFinish.Set();
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

                //
                await StartMessageSendingAsync("我这台智能书柜启动了！");

                // 原来 StartMonitorTask 在这里

                SelectMode();

                InitialShelfCfg();

                InitialRfidManager();

                // 首次显示以前遗留的 LED 文字
                // 注意 RfidCenter 有可能还没有来得及完全就绪，比如 RfidCenter 需要初始化好 LED 控制卡的 COM 端口
                if (string.IsNullOrEmpty(App.LedText) == false)
                {
                    try
                    {
                        await TinyServer.LedDisplayAsync(App.LedText, null);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"LedDisplay() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }

                if (App.Function == "智能书柜")
                    ShelfData.StartMonitorTask();

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

        static CancellationTokenSource _cancelSendingMessage = new CancellationTokenSource();

        public static async Task StartMessageSendingAsync(string message)
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

                _cancelSendingMessage?.Cancel();
                _cancelSendingMessage = new CancellationTokenSource();
                TinyServer.StartSendTask(_cancelSendingMessage.Token); // _cancelApp.Token

                if (string.IsNullOrEmpty(message) == false)
                    ShelfData.TrySetMessage(null, message);    // "我这台智能书柜启动了！"
            }
            else
            {
                TinyServer.CloseConnection();
                // stop send task
                {
                    _cancelSendingMessage?.Cancel();
                    _cancelSendingMessage?.Dispose();
                    _cancelSendingMessage = null;
                }
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

        // 首次连接到消息服务器
        public static async Task ConnectMessageServerAsync()
        {
            try
            {
                TinyServer.CloseConnection();

                if (string.IsNullOrEmpty(messageServerUrl) == false)
                {
                    //_messageServerConnected.Reset();

                    var result = await TinyServer.ConnectAsync(messageServerUrl, messageUserName, messagePassword, "");
                    if (result.Value == -1)
                        WpfClientInfo.WriteLogInternal("error", $"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");
                    else
                    {
                        var prepare_result = await TinyServer.PrepareGroupNamesAsync();
                        if (prepare_result.Value == -1)
                            WpfClientInfo.WriteErrorLog($"准备群名失败: {prepare_result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={prepare_result.ErrorCode}");
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
        public static async Task<bool> EnsureConnectMessageServerAsync()
        {
            if (string.IsNullOrEmpty(messageServerUrl) == false
                && TinyServer.IsDisconnected)
            {
                var result = await TinyServer.ConnectAsync(messageServerUrl, messageUserName, messagePassword, "");
                if (result.Value == -1)
                {
                    _ = GlobalMonitor.CompactLog.Add("连接消息服务器失败: {0}。url={1},userName={2},errorCode={3}",
                        new object[] { result.ErrorInfo, messageServerUrl, messageUserName, result.ErrorCode });
                    // WpfClientInfo.WriteErrorLog($"连接消息服务器失败: {result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={result.ErrorCode}");

                    // 2021/11/29
                    // 恢复短时间轮询
                    ShelfData.SetShortReplication();
                    return false;
                }
                else
                {
                    var prepare_result = await TinyServer.PrepareGroupNamesAsync();
                    if (prepare_result.Value == -1)
                        WpfClientInfo.WriteErrorLog($"准备群名失败: {prepare_result.ErrorInfo}。url={messageServerUrl},userName={messageUserName},errorCode={prepare_result.ErrorCode}");
                    else
                    {
                        /*
                        // 探测稍早(断开)间隙期间是否有通知消息
                        var count = await TinyServer.DetectGapMessageAsync();
                        if (count > 0)
                        {
                            // testing
#if TESTING
                            App.CurrentApp.SpeakSequence($"中断期间有 {count} 条未读消息");
#endif
                            ShelfData.ActivateReplication();
                        }
                        */

                        // 简单
                        // 重新连接以后默认激活同步一次
                        {
                            // testing
#if TESTING
                            App.CurrentApp.SpeakSequence($"重新连接以后激活同步一次");
#endif
                            ShelfData.ActivateReplication();
                        }
                    }
                    return true;
                }
            }
            return true;
        }

        public static bool IsPageBorrowActive { get; set; }
        public static bool IsPageShelfActive { get; set; }
        public static bool IsPageInventoryActive { get; set; }

        // 比 App.Funcion == "智能书柜" 判断起来更快
        // static bool _isShelfMode = false;

        // 可能会在全局错误 "cfg" 中设置出错信息
        public static void InitialShelfCfg()
        {
            if (App.Function == "智能书柜")
            {
                try
                {
                    // _isShelfMode = true;
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
                    WpfClientInfo.WriteErrorLog($"InitialSheflCfg() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                    SetError("cfg", $"InitialShelf() 出现异常:{ex.Message}");
                }
            }
            else
            {
                // _isShelfMode = false;
            }
        }

        private void _barcodeCapture_InputChar(CharInput input)
        {
            if (_pauseBarcodeScan > 0 || _appActivated == false)
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
            if (_pauseBarcodeScan > 0 || _appActivated == false)
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

                    // 重置活跃时钟
                    PageMenu.MenuPage.ResetActivityTimer();

                    _lastBarcode = new LastBarcode { Barcode = line, Time = DateTime.Now };
                    // 触发一次输入
                    LineFeed?.Invoke(this, new LineFeedEventArgs { Text = line });
                }
                else
                {
                    // 特殊输入，空行直接回车。代表触发人脸识别
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

        static int _pauseMonitor = 0;

        // 暂停对条码输入的监控
        public static void PauseBarcodeMonitor()
        {
            _pauseMonitor++;
            if (_pauseMonitor == 1)
                _barcodeCapture.PauseBarcodeMonitor = true;
        }

        // 恢复对条码输入的监控
        public static void ContinueBarcodeMonitor()
        {
            _pauseMonitor--;
            if (_pauseMonitor == 0)
                _barcodeCapture.PauseBarcodeMonitor = false;
        }

        /*
        public static int PauseMonitor
        {
            get
            {
                return _barcodeCapture.PauseMonitor;
            }
            set
            {
                _barcodeCapture.PauseMonitor = value;
            }
        }
        */

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
                        // 2024/2/1
                        // 书柜断网情况下不进行检查(因为检查会导致出现红色的“网络故障”报错，让读者困惑)
                        // TODO: 不过断网情况下可以弱化为，只检查 fingerprintcenter/palmcenter 和 facecenter 之间的 server uid 一致性，假定这两个可以检查的话
                        if (ShelfData.LibraryNetworkCondition != "OK")
                        {
                            SetError("uid", null);
                            continue;
                        }

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
                        ShortcutPath = "DigitalPlatform/dp2 V3/RFID中心",
                        MutexName = "{CF1B7B4A-C7ED-4DB8-B5CC-59A067880F92}"
                    });
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false
                    && ProcessManager.IsIpcUrl(App.FingerprintUrl)
                    && ProcessManager.IsFingerprintUrl(App.FingerprintUrl))
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
                WpfClientInfo.WriteErrorLog($"DeleteLastTempFiles() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                App.AddErrors("global", new List<string> { $"清除上次遗留的临时文件时出现异常: {ex.Message}" });
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

        // 如果当前是 Administrator 身份，把指定文件修改为 everyone 可以访问修改
        // https://stackoverflow.com/questions/9108399/how-to-grant-full-permission-to-a-file-created-by-my-application-for-all-users
        public static void GrantAccess(string fullPath)
        {
            if (IsAdministrator())
            {
                try
                {
                    DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                    DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(
                        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        FileSystemRights.FullControl,
                        InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                        PropagationFlags.NoPropagateInherit,
                        AccessControlType.Allow));
                    dInfo.SetAccessControl(dSecurity);

                    WpfClientInfo.WriteInfoLog($"文件 {fullPath} 被修改权限，以便任何用户都可以访问和修改它");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"GrantAccess({fullPath}) 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            }
        }

        // 注：Windows 关机或者重启的时候，会触发 OnSessionEnding 事件，但不会触发 OnExit 事件
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            WpfClientInfo.WriteDebugLog("OnSessionEnding() called");

            // ShelfData.SaveRetryActions();

            _cancelApp?.Cancel();
            _cancelProcessMonitor?.Cancel();
            _cancelRfid?.Cancel();
            _cancelSendingMessage?.Cancel();
            ShelfData.CancelAll();

            // 最后关灯
            RfidManager.TurnShelfLamp("*", "turnOff");
            WpfClientInfo.WriteInfoLog("物理关灯 OnSessionEnding()");

            try
            {
                if (PageMenu.PageShelf != null)
                {
                    // 2020/9/17
                    WpfClientInfo.Config?.Set("pageShelf", "splitterPosition", PageMenu.PageShelf?.SplitterPosition);
                }
            }
            catch (NullReferenceException)
            {

            }

            // 保存软时钟
            ShelfData.SaveSoftClock();

            // 2021/8/21
            /*
            {
                var path = Path.Combine(WpfClientInfo.UserDir, "tagLines.json");
                File.WriteAllText(path, ShelfData.BuildTagLineJsonString());
            }
            */
            SaveTagLines();

            WaitAllTaskFinish();
            WpfClientInfo.Finish(GrantAccess);
            WpfClientInfo.WriteDebugLog("End WpfClientInfo.Finish()");

            ShelfData.TrySetMessage(null, $"我这台智能书柜停止了哟！({e.ReasonSessionEnding})");

            try
            {
                TinyServer.CloseConnection();
            }
            catch
            {

            }

            // 2021/11/29
            // 网络流量统计
            {
                var data = NetworkUsage.ToString(_networkUsage.GetData());
                WpfClientInfo.WriteInfoLog($"本次 dp2ssl 运行期间网络流量统计:\r\n{data}");
            }

            base.OnSessionEnding(e);
        }

        // 注：Windows 关机或者重启的时候，会触发 OnSessionEnding 事件，但不会触发 OnExit 事件
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        protected async override void OnExit(ExitEventArgs e)
        {
            WpfClientInfo.WriteDebugLog("OnExit() called");

            _barcodeCapture.Stop();
            _barcodeCapture.InputLine -= _barcodeCapture_inputLine;
            //_barcodeCapture.InputChar -= _barcodeCapture_InputChar;

            try
            {
                if (PageMenu.PageShelf != null)
                {
                    await PageMenu.PageShelf?.SubmitAsync(true);

                    // 2020/9/17
                    WpfClientInfo.Config?.Set("pageShelf", "splitterPosition", PageMenu.PageShelf?.SplitterPosition);
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"PageShelf.SubmitAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }

            // 保存软时钟
            ShelfData.SaveSoftClock();

            // 2021/8/21
            /*
            {
                var path = Path.Combine(WpfClientInfo.UserDir, "tagLines.json");
                File.WriteAllText(path, ShelfData.BuildTagLineJsonString());
            }
            */
            SaveTagLines();

            try
            {
                if (PageMenu.PageInventory != null)
                {
                    await PageMenu.PageInventory?.SaveOnExitAsync();
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"PageInventory.ClearList() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }

            // ShelfData.SaveRetryActions();

            try
            {
                // 尝试抢先直接发送
                _ = TinyServer.InnerSetMessageAsync(null, $"我这台智能书柜退出了哟！");
            }
            catch
            {
                // 如果直接发送不成功，则送入 MessageQueue 中
                ShelfData.TrySetMessage(null, $"我这台智能书柜退出了哟！");
            }

            try
            {
                TinyServer.CloseConnection();
            }
            catch
            {

            }

            // 2021/11/29
            // 网络流量统计
            {
                var data = NetworkUsage.ToString(_networkUsage.GetData());
                WpfClientInfo.WriteInfoLog($"本次 dp2ssl 运行期间网络流量统计:\r\n{data}");
            }

            _cancelApp?.Cancel();
            _cancelProcessMonitor?.Cancel();
            _cancelRfid?.Cancel();
            _cancelSendingMessage?.Cancel();
            ShelfData.CancelAll();

            WaitAllTaskFinish();

            WpfClientInfo.Finish(GrantAccess);
            WpfClientInfo.WriteDebugLog("End WpfClientInfo.Finish()");

            // EndFingerprint();

            _channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            _channelPool.Close();

            // 最后关灯
            RfidManager.TurnShelfLamp("*", "turnOff");
            WpfClientInfo.WriteInfoLog("物理关灯 OnExit()");

            base.OnExit(e);
        }

        // 保存当前标签信息到 tagLines.json 文件
        void SaveTagLines()
        {
            try
            {
                var path = Path.Combine(WpfClientInfo.UserDir, "tagLines.json");
                File.WriteAllText(path, ShelfData.BuildTagLineJsonString());
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SaveTagLines() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        void WaitAllTaskFinish()
        {
            // 最多等待 10 秒
            ShelfData.Task?.Wait(TimeSpan.FromSeconds(10));
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
            _channelPool.Clear();
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

        public static string SipEncoding
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "sipEncoding", "utf-8");
            }
        }

        public static string SipInstitution
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "sipInstitution", "");
            }
        }

        public static string SkinName
        {
            get
            {
                return WpfClientInfo.Config.Get("global", "skin", "暗色");
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

        #region RFID 测试相关参数

        public static bool RfidTestBorrowEAS
        {
            get
            {
                var ret = WpfClientInfo.Config?.GetBoolean("rfidTest",
                "rfidTestBorrowEAS", false);
                return ret ?? false;
            }
        }

        public static bool RfidTestReturnPreEAS
        {
            get
            {
                var ret = WpfClientInfo.Config?.GetBoolean("rfidTest",
"rfidTestReturnPreEAS", false);
                return ret ?? false;
            }
        }

        public static bool RfidTestReturnAPI
        {
            get
            {
                var ret = WpfClientInfo.Config?.GetBoolean("rfidTest",
"rfidTestReturnAPI", false);
                return ret ?? false;
            }
        }

        public static bool RfidTestReturnPostUndoEAS
        {
            get
            {
                var ret = WpfClientInfo.Config?.GetBoolean("rfidTest",
"rfidTestReturnPostUndoEAS", false);
                return ret ?? false;
            }
        }

        #endregion

        // 自助借还界面是否显示图书封面图像
        public static bool DisplayCoverImage
        {
            get
            {
                var ret = WpfClientInfo.Config?.GetBoolean("ssl_operation",
                    "displayCoverImage", false);
                return ret ?? false;
            }
        }

        public static string RfidUrl
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "rfidUrl", "");
            }
        }

        // 仅读取 EPC，加速超高频标签的读取
        public static bool OnlyReadEPC
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return false;
                return WpfClientInfo.Config.GetBoolean("uhf", "onlyReadEPC", false);
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

        // 人脸识别允许命中多个结果
        public static string FaceInputMultipleHits
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "faceInputMultipleHits", "使用第一个") ?? "使用第一个";
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
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", 
                    "auto_trigger", false);
            }
        }

#if REMOVED
        // 身份读卡器是否竖向放置
        public static bool PatronReaderVertical
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", 
                    "patron_info_lasting", false);
            }
        }
#endif
        // 竖向放置的身份读卡器名
        public static string VerticalReaderName
        {
            get
            {
                return WpfClientInfo.Config?.Get("ssl_operation",
                    "vertial_reader_name", null);
            }
        }

        // 自动返回菜单页面
        public static bool AutoBackMenuPage
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation",
                    "auto_back_menu_page", false);
            }
        }

        /*
        public static bool PatronInfoDelayClear
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", 
        "patron_info_delay_clear", false);
            }
        }
        */

        /*
        public static bool EnablePatronBarcode
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("ssl_operation", 
        "enable_patron_barcode", false);
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
    sizes.Add("禁用");
    sizes.Add("一维码+二维码");
    sizes.Add("一维码");
    sizes.Add("二维码");
* */
        public static string WorkerBarcodeStyle
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "worker_barcode_style", "禁用");
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

        public static string CacheWorkerPasswordLength
        {
            get
            {
                return WpfClientInfo.Config?.Get("global", "memory_worker_password", "无");
            }
        }

        public static int AutoBackMainMenuSeconds
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return -1;
                return WpfClientInfo.Config.GetInt("global",
                    "autoback_mainmenu_seconds",
                    -1);
            }
        }

        public static bool AutoUpdateWallpaper
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return true;

                return WpfClientInfo.Config.GetBoolean("global", 
                    "auto_update_wallpaper", false);
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

        // (智能书柜)自动同步全部册记录和书目摘要到本地
        public static bool ReplicateEntities
        {
            get
            {
                if (WpfClientInfo.Config == null)
                    return false;

                return (bool)WpfClientInfo.Config?.GetBoolean("shelf",
                    "replicateEntities",
                    false);
            }
            set
            {
                WpfClientInfo.Config?.SetBoolean("shelf",
                    "replicateEntities",
                    value);
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

        // 运行时临时存储的工作人员账户信息
        Dictionary<string, Account> _accounts = new Dictionary<string, Account>();

        // 查找一个临时存储的工作人员账户信息
        public Account FindAccount(string userName)
        {
            if (_accounts.ContainsKey(userName) == false)
                return null;
            return _accounts[userName];
        }

        // 临时存储一个工作人员账户信息
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

        // 删除一个临时存储的工作人员账户信息
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



        // TODO: setreaderobject 和 setobject 只要具备其中一个即可
        // 演化 getres --> getobject --> getreaderobject
        // static string _baseRights = "getsystemparameter,getbiblioinfo,getbibliosummary,getiteminfo,getoperlog,getreaderinfo,getreaderobject,searchbiblio,searchitem,searchreader,borrow,renew,return,setreaderinfo,setiteminfo"; // setreaderobject, // 取消 setobject

        // https://jihulab.com/DigitalPlatform/dp2doc/-/issues/88#note_2182237
        // 自助机
        // ,getiteminfo,getreaderinfo,getreaderobject,getbibliosummary,borrow,renew,return,setreaderinfo
        // 书柜
        // ,getiteminfo,getreaderinfo,getreaderobject,getbibliosummary,borrow,renew,return,setreaderinfo,getsystemparameter,setiteminfo,searchreader,getoperlog,searchitem

        static string _baseRightsSelfLoan = "getiteminfo,getreaderinfo,getbibliosummary,borrow,renew,return,setreaderinfo"; // getreaderobject,
        static string _baseRightsBookShelf = "getiteminfo,getreaderinfo,getbibliosummary,borrow,renew,return,setreaderinfo,getsystemparameter,setiteminfo,searchreader,getoperlog,searchitem";  // getreaderobject,

        static void VerifyRights(string rights)
        {
            string baseRights = "";
            if (App.Function == "自助借还")
                baseRights = _baseRightsSelfLoan;
            else if (App.Function == "智能书柜")
                baseRights = _baseRightsBookShelf;
            else
                throw new Exception($"无法识别的 App.Function 值 '{App.Function}'");

            List<string> missing_rights = new List<string>();
            var base_rights = StringUtil.SplitList(baseRights);
            foreach (var right in base_rights)
            {
                if (StringUtil.IsInList(right, rights) == false)
                {
                    // 2021/7/20
                    if (right == "getreaderinfo" && StringUtil.GetParameterByPrefix(rights, "getreaderinfo") != null)
                        continue;
                    if (right == "setreaderinfo" && StringUtil.GetParameterByPrefix(rights, "setreaderinfo") != null)
                        continue;

                    missing_rights.Add(right);
                }
            }

            if (missing_rights.Count > 0)
                throw new Exception($"账户 {_currentUserName} 缺乏业务必备的权限 {StringUtil.MakePathList(missing_rights)}");
        }

        static string _currentUserName = "";
        static string _currentUserLibraryCodeList = "";

        public static string CurrentUserLibraryCodeList
        {
            get
            {
                return _currentUserLibraryCodeList;
            }
        }

        // public static string ServerUID = "";

        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;
            _currentUserLibraryCodeList = channel.LibraryCodeList;

            // 2020/9/18
            // 检查 rights
            VerifyRights(channel.Rights);

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

            // 2021/5/24
            // 控制通道数量规模
            if (_channelPool.Count > 10)
                _channelPool.CleanChannel();

            LibraryChannel channel = _channelPool.GetChannel(strServerUrl, strUserName);
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }

            // 检查数组是否溢出
            if (_channelList.Count > 100)
                throw new Exception($"_channelList.Count={_channelList.Count} 超过极限 100 个");

            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            _channelPool.ReturnChannel(channel);
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

            FingerprintManager.Pause = false;
            RfidManager.Pause = false;  // 2023/12/6

            // 单独线程执行，避免阻塞 OnActivated() 返回
            /*
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
            */
            EnableSendKey(false);
            //SpeakSequence("前台");
            base.OnActivated(e);
        }

        static void EnableSendKey(bool enable)
        {
            // false 要慢; true 要快
            _ = Task.Run(async () =>
            {
                try
                {
                    if (enable == false)
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    FingerprintManager.EnableSendkey(enable, "dp2ssl");
                    // 注: 暂不进行 RfidCenter 的 SendKey 打开
                    if (enable == false)
                        RfidManager.EnableSendkey(enable);
                }
                catch
                {
                }
            });
        }

        protected override void OnDeactivated(EventArgs e)
        {
            // dp2ssl 失去活动以后，要不接受扫码，并且要不吞掉击键
            // PauseBarcodeScan();
            _appActivated = false;
            UpdateHandled();

            FingerprintManager.Pause = true;
            RfidManager.Pause = true;  // 2023/12/6

            EnableSendKey(true);    // 2022/9/9
            //SpeakSequence("后台");
            base.OnDeactivated(e);
        }

        #endregion

        public static void AddErrors(string type, List<string> errors)
        {
            DateTime now = DateTime.Now;
            List<string> results = new List<string>();
            foreach (string error in errors)
            {
                results.Add($"{now.ToShortTimeString()} {error}");
            }

            _errorTable.SetError(type,
                StringUtil.MakePathList(results, "; "),
                true);
        }

        public static void SetError(string type, string error)
        {
            /*
            if (type == "face" && error != null)
            {
                Debug.Assert(false, "");
            }
            */

            _errorTable.SetError(type, error, true);
        }

        public static void ClearErrors(string type)
        {
            // _errors.Clear();
            _errorTable.SetError(type, "", true);
        }

        public string GetError(string type)
        {
            return _errorTable.GetError(type);
        }

        public static event TagChangedEventHandler TagChanged = null;

        // (新版本的事件)
        // 图书标签发生改变
        public static event NewTagChangedEventHandler BookTagChanged = null;
        // 读者证卡标签发生改变
        public static event NewTagChangedEventHandler PatronTagChanged = null;

        public static event DigitalPlatform.ProgressChangedEventHandler GetTagInfoProgressChanged = null;

        // public event SetErrorEventHandler TagSetError = null;



        private static void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 2020/7/22
            if (e.Result.Value == -1)
            {
                SetError("rfid", e.Result.ErrorInfo);
            }

            try
            {

                // 标签总数显示
                // this.Number = e.Result?.Results?.Count.ToString();
                if (e.Result.Results != null)
                {
                    // TODO: 如果 IsPageShelfActive 和 IsPageBorrowActive 都为 false，则要看 Function 是什么决定如何显示标签数
                    bool isShelf = IsPageShelfActive;
                    bool isBorrow = IsPageBorrowActive;
                    bool isInventory = IsPageInventoryActive;
                    if ((isShelf == false && isBorrow == false)
                        || (isShelf == true && isBorrow == true))
                    {
                        // TODO: 这一句是否需要 catch 一下
                        isShelf = Function == "智能书柜";
                        isBorrow = !isShelf;
                    }

                    // bool numberShown = false;

                    // 书柜的图书读卡器
                    if (isShelf && e.Source != "base2")
                    {
                        int index = 0;

                        // 设置进度条范围
                        GetTagInfoProgressChanged?.Invoke(sender,
      new DigitalPlatform.ProgressChangedEventArgs
      {
          Start = 0,
          End = e.Result.Results.Count,
          Value = -1,
      });
                        bool triggered = false;
                        ShelfData.BookTagList.Refresh(// sender as BaseChannel<IRfid>,
                            e.ReaderNameList,
                            e.Result.Results,
                            (readerName, uid, antennaID, protocol) =>
                            {
                                // TODO: source == "initial" 时这里详细显示进度
                                GetTagInfoProgressChanged?.Invoke(sender,
                                    new DigitalPlatform.ProgressChangedEventArgs
                                    {
                                        Message = $"{uid}",
                                        Start = -1, // 0,
                                        End = -1,   // e.Result.Results.Count,
                                        Value = index++,
                                    });

                                var channel = sender as BaseChannel<IRfid>;
                                /*
                                if (channel.Started == false)
                                    return new GetTagInfoResult { Value = -1, ErrorInfo = "11 RFID 通道尚未启动" };
                                */
                                return channel.Object.GetTagInfo(readerName, uid, antennaID);
                            },
                            (add_tags, update_tags, remove_tags) =>
                            {
                                BookTagChanged?.Invoke(sender, new NewTagChangedEventArgs
                                {
                                    AddTags = add_tags,
                                    UpdateTags = update_tags,
                                    RemoveTags = remove_tags,
                                    Source = e.Source,
                                });
                                triggered = true;
                            },
                            (type, text) =>
                            {
                                RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                            });

                        // 表示到达末尾
                        GetTagInfoProgressChanged?.Invoke(sender,
                            new DigitalPlatform.ProgressChangedEventArgs
                            {
                                Start = 0,
                                End = e.Result.Results.Count,
                                Value = e.Result.Results.Count,
                            });

                        // 标签总数显示 只显示标签数，不再区分图书标签和读者卡
                        if (CurrentApp != null)
                            CurrentApp.Number = $"{ShelfData.BookTagList.Tags.Count}:{ShelfData.PatronTagList.Tags.Count}";
                        //numberShown = true;

                        /*
                        // 让 BookTagChanged 事件也能感知到心跳
                        if (triggered == false)
                        {
                            BookTagChanged?.Invoke(sender, new NewTagChangedEventArgs
                            {
                                Source = e.Source,
                            });
                        }
                        */
                    }

                    // 书柜的读者证读卡器
                    if (isShelf && e.Source == "base2")
                    {
#if PATRONREADER_HEARTBEAT
                        bool triggered = false;
#endif
                        ShelfData.PatronTagList.Refresh(// sender as BaseChannel<IRfid>,
                            e.ReaderNameList,
                            e.Result.Results,
                            (readerName, uid, antennaID, protocol) =>
                            {
                                var channel = sender as BaseChannel<IRfid>;
                                return channel.Object.GetTagInfo(readerName, uid, antennaID);
                            },
                            (add_tags, update_tags, remove_tags) =>
                            {
                                PatronTagChanged?.Invoke(sender, new NewTagChangedEventArgs
                                {
                                    AddTags = add_tags,
                                    UpdateTags = update_tags,
                                    RemoveTags = remove_tags,
                                    Source = e.Source,
                                });
#if PATRONREADER_HEARTBEAT
                                triggered = true;
#endif
                            },
                            (type, text) =>
                            {
                                RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                            });

                        // 标签总数显示 只显示标签数，不再区分图书标签和读者卡
                        if (CurrentApp != null)
                            CurrentApp.Number = $"{ShelfData.BookTagList.Tags.Count}:{ShelfData.PatronTagList.Tags.Count}";

#if PATRONREADER_HEARTBEAT
                        // 让 PatronTagChanged 事件也能感知到心跳
                        if (triggered == false)
                        {
                            PatronTagChanged?.Invoke(sender, new NewTagChangedEventArgs
                            {
                                Source = e.Source,
                            });
                        }
#endif
                    }



                    // 2020/10/1
                    if (isInventory)
                    {
                        SoundMaker.FirstSound(e.Result.Results.Count);

                        NewTagList2.Refresh(
                            e.ReaderNameList,
                            e.Result.Results,
                            /*
                            (readerName, uid, antennaID) =>
                            {
                                if (InventoryData.UidExsits(uid, out string pii))
                                    return new GetTagInfoResult { Value = 1, TagInfo = new TagInfo { Tag = pii } };
                                var channel = sender as BaseChannel<IRfid>;
                                if (channel.Started == false)
                                    return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };
                                return channel.Object.GetTagInfo(readerName, uid, antennaID);
                            },
                            */
                            (add_tags, update_tags, remove_tags) =>
                            {
                                BookTagChanged?.Invoke(sender, new NewTagChangedEventArgs
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
                            CurrentApp.Number = $"{ShelfData.BookTagList.Tags.Count}";
                        //numberShown = true;

                        SoundMaker.StopCurrent();
                    }

                    if (isBorrow == true/* || numberShown == false*/)
                    {
                        List<OneTag> results = null;
                        if (RfidManager.GetRSSI)
                            results = RfidTagList.FilterByRSSI(e.Result.Results, 0);
                        else
                            results = e.Result.Results;

                        RfidTagList.Refresh(sender as BaseChannel<IRfid>,
                            e.ReaderNameList,
                            results,    // e.Result.Results,
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
                            CurrentApp.Number = $"{RfidTagList.Books.Count}:{RfidTagList.Patrons.Count}";
                        //numberShown = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 2020/10/30
                SetError("rfid", ex.Message);
                WpfClientInfo.WriteErrorLog($"RfidManager_ListTags() 捕获到异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 启动时的网络模式。注意断网模式是用 ShelfData.LibraryNetworkCondition 来表示的
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
                        _startNetworkModeComment = $"{DateTime.Now.ToString()} 断网模式对话框弹出，人工选择了模式 '{dlg.Mode}'";
                    }
                    ));
                }
            }
        }

        // 关于 dp2ssl 启动阶段断网模式对话框曾经出现过，如何选择的，注释
        internal static string _startNetworkModeComment = "";

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

        // tall middle wide full
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
            else if (style == "full")
            {
                window.Left = 0;
                window.Top = 0;
                window.Width = mainWindows.ActualWidth;
                window.Height = mainWindows.ActualHeight;
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


        public static void Invoke(Action action)
        {
            /*
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                action.Invoke();
            });
            */

            try
            {
                Current?.Dispatcher?.Invoke(action);
            }
            catch (Exception ex)
            {
                // 2021/8/31
                WpfClientInfo.WriteErrorLog($"UI 异常: {ExceptionUtil.GetDebugText(ex)}");
                SetError("UI Exception: ", ex.Message);
            }
        }

        // parameters:
        //      style   风格。
        //              若包含(size:xxx) tall middle wide full 之一，表示窗口显示的大小
        public static ProgressWindow ErrorBox(
            string title,
            string message,
            string color = "red",
            string style = "")
        {
            ProgressWindow progress = null;

            var size = StringUtil.GetParameterByPrefix(style, "size");
            if (size == null)
                size = "tall";

            App.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.TitleText = title;
                progress.MessageText = "正在处理，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                App.SetSize(progress, size);    // "tall"
                //progress.Width = Math.Min(700, this.ActualWidth);
                //progress.Height = Math.Min(900, this.ActualHeight);
                progress.Closed += (o, e) =>
                {
                };
                if (StringUtil.IsInList("button_ok", style))
                    progress.okButton.Content = "确定";
                progress.Show();
            }));

            // auto_close:xxx 其中 xxx 为延时秒数。:xxx 缺省为 3 秒
            var delay_seconds_string = StringUtil.GetParameterByPrefix(style, "auto_close");

            if (/*StringUtil.IsInList("auto_close", style)*/
                delay_seconds_string != null)
            {
                int delay_seconds = 3;
                if (string.IsNullOrEmpty(delay_seconds_string) == false)
                    Int32.TryParse(delay_seconds_string, out delay_seconds);

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
                        await Task.Delay(TimeSpan.FromSeconds(delay_seconds));
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

            return progress;
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
            ShelfData.TrySetMessage(null, $"==== {title} 对话框显示并等待输入 ====\r\n{text}");
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
        // 注意，需要忽略隐藏状态的窗口
        public static Window GetActiveWindow()
        {
            try
            {
                return SortWindowsTopToBottom(Application.Current.Windows.OfType<Window>())
                    .Where(o => o.IsVisible == true)    // 2020/9/23
                    .FirstOrDefault();

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
        // return.Value:
        //      -1  出错(当前 Application 继续运行)
        //      0   当前 Application 继续运行
        //      1    需要立即退出 Application
        public static NormalResult DisableEdgeUI()
        {
            try
            {
                using (RegistryKey item = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\EdgeUI"))
                {
                    if (item != null)
                    {
                        int? v = item.GetValue("AllowEdgeSwipe", 1) as int?;
                        if (v == 0)
                        {
                            WpfClientInfo.WriteInfoLog("注册表中 AllowEdgeSwipe 已经是 0");
                            return new NormalResult { Value = 0 };
                        }
                    }
                }

                WpfClientInfo.WriteInfoLog("检查当前进程是否为 Administrator 身份");

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                Boolean isRunasAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isRunasAdmin)
                {
                    WpfClientInfo.WriteInfoLog("检查当前进程是 Administrator 身份，那么修改 AllowEdgeSwipe 为 0");

                    using (RegistryKey item = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\EdgeUI"))
                    {
                        item.SetValue("AllowEdgeSwipe", 0, RegistryValueKind.DWord);
                    }

                    string[] args = Environment.GetCommandLineArgs();
                    if (args.IndexOf<string>("DisableEdgeUI") != -1)
                    {
                        WpfClientInfo.WriteInfoLog("当前进程是专门用来修改 EdgeUI 参数的，使命已经完成，退出进程");

                        // MessageBox.Show("registry changed 1 !");
                        App.Current.Shutdown();
                        return new NormalResult { Value = 1 };
                    }

                    WpfClientInfo.WriteInfoLog("当前进程修改完 EdgeUI 参数以后继续运行");

                    // MessageBox.Show("registry changed 2 !");
                }
                else
                {
                    WpfClientInfo.WriteInfoLog("尝试用 Administrator 身份再启动一个 dp2ssl 进程，预期用户会看到 UAC 对话框");


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
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"dp2ssl 无法以 Administator 身份运行。异常信息: {ExceptionUtil.GetDebugText(ex)}");
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"dp2ssl 无法以 Administator 身份运行。{ex.Message}",
                            ErrorCode = "startAdministratorError"
                        };
                    }
                }

                return new NormalResult { Value = 0 };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"DisableEdgeUI() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult { Value = 0 };
            }
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region 确认读者是否还在机器前面

        static Task _confirmTask = null;
        static CancellationTokenSource _cancelConfirm = new CancellationTokenSource();

        // 开始确认过程
        public static bool BeginConfirm()
        {
            if (_confirmTask != null)
                return false;

            _cancelConfirm?.Cancel();
            _cancelConfirm?.Dispose();

            _cancelConfirm = new CancellationTokenSource();
            _confirmTask = ConfirmAsync(_cancelConfirm.Token);
            return true;
        }

        public static void CancelConfirm()
        {
            _cancelConfirm?.Cancel();
        }

        public static async Task<bool> ConfirmAsync(CancellationToken token)
        {
            ProgressWindow progress = null;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken, token))
            {
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "固定读者信息时间太长了，确认您还在么?";
                    progress.MessageText = "如果您不希望软件自动清除面板上固定的读者信息，请点“确定”按钮";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "放弃";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "middle");
                    progress.BackColor = "yellow";
                    progress.Show();
                }));

                try
                {
                    if (ShelfData.OpeningDoorCount > 0)
                    {
                        return true;
                    }

                    // 倒计时警告
                    App.CurrentApp.Speak("即将清除面板上的读者信息。点“放弃”按钮可阻止清除");
                    for (int i = 20; i > 0; i--)
                    {
                        if (cancel.Token.IsCancellationRequested)
                        {
                            App.CurrentApp.Speak("放弃清除");
                            return true;
                        }
                        string text = $"({i}) 即将清除面板上的读者信息\r\n\r\n点“放弃”按钮可阻止清除";
                        App.Invoke(new Action(() =>
                        {
                            progress.MessageText = text;
                        }));
                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                    }

                    App.CurrentApp.Speak("读者信息已经清除");
                    return false;
                }
                catch
                {
                    App.CurrentApp.Speak("放弃清除");
                    return true;
                }
                finally
                {
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));

                    _confirmTask = null;
                }
            }
        }

        #endregion

        bool CheckRunType()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                // 如果是 ClickOnce 运行方式，则检查本机是否还安装有绿色版本，如果有则拒绝运行
                string exe_path = "c:\\dp2ssl\\dp2ssl.exe";
                if (File.Exists(exe_path))
                {
                    StartErrorBox("当前 dp2SSL 是 ClickOnce 安装方式，但探测到本机 c:\\dp2ssl 目录还安装了绿色版本，请先卸载绿色版本");
                    App.Current.Shutdown();
                    return false;
                }
            }
            else
            {
                // 如果是绿色运行方式，则检查本机是否还安装有 ClickOnce 版本，如果有则拒绝运行
                string strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V3/dp2SSL-自助借还");
                if (File.Exists(strShortcutFilePath) == true)
                {
                    StartErrorBox("当前 dp2SSL 是绿色安装方式，但探测到本机还安装了 ClickOnce 版本(在程序组位置“DigitalPlatform/dp2 V3/dp2SSL-自助借还”)，请先卸载 ClickOnce 版本");
                    App.Current.Shutdown();
                    return false;
                }
            }

            return true;
        }

        #region 删除破损的图像文件

        static object _syncRoot_brokenFile = new object();

        // 记忆那些无法删除的文件的文件名。以便 dp2ssl 下次重启的时候抢先删除
        static void MemoryBrokenFileName(string fileName)
        {
            lock (_syncRoot_brokenFile)
            {
                string memoryFileName = Path.Combine(WpfClientInfo.UserDir, "memory_deleting.txt");
                using (StreamWriter sw = File.AppendText(memoryFileName))
                {
                    sw.WriteLine(fileName);
                }
            }
        }

        static void DeleteBrokenFiles()
        {
            lock (_syncRoot_brokenFile)
            {
                try
                {
                    string memoryFileName = Path.Combine(WpfClientInfo.UserDir, "memory_deleting.txt");
                    if (File.Exists(memoryFileName) == false)
                        return;
                    using (var sr = new StreamReader(memoryFileName, Encoding.UTF8))
                    {
                        while (true)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                                break;
                            if (string.IsNullOrEmpty(line))
                                continue;
                            try
                            {
                                File.Delete(line);
                            }
                            catch (Exception ex)
                            {
                                WpfClientInfo.WriteErrorLog($"DeleteBrokerFiles() 中删除文件 '{line}' 时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            }
                        }
                    }

                    try
                    {
                        File.Delete(memoryFileName);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"DeleteBrokerFiles() 中删除文件名文件 '{memoryFileName}' 时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"DeleteBrokerFiles() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            }
        }

        // 删除已经损坏的图像文件(临时文件)
        public static void TryDeleteBrokenImageFile(string fileName)
        {
            // TODO: 先检查，是否为临时图像文件目录中的文件。是这里的文件才删除

            GC.Collect();
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // 如果删除不掉，则记忆下来下次 dp2ssl.exe 启动的时候尝试删除
                MemoryBrokenFileName(fileName);
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
        public string Source { get; set; }   // 触发者
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
