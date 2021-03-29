using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

using Newtonsoft.Json;
using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.RFID;
using DigitalPlatform.Face;
using DigitalPlatform.WPF;
using DigitalPlatform.IO;
using DigitalPlatform.Net;
using System.Windows.Navigation;

namespace dp2SSL
{
    /// <summary>
    /// PageSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PageSetting : MyPage, INotifyPropertyChanged
    {
        public PageSetting()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Loaded += PageSetting_Loaded;
            this.Unloaded += PageSetting_Unloaded;

            // this.keyborad.KeyPressed += Keyborad_KeyPressed;
            InitializeLayer(this.mainGrid);
        }

        private void PageSetting_Unloaded(object sender, RoutedEventArgs e)
        {
            /*
            try
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
            finally
            {
                // App.ContinueBarcodeScan();
            }
            */
            CloseDialogs();
        }

        static int passwordErrorCount = 0;
        static Task delayClear = null;

        private async void PageSetting_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                // 首次设置密码，或者登录
                InitialPage();
                if (this.password.Visibility == Visibility.Visible)
                    this.password.Focus();
            }
            finally
            {
                App.PauseBarcodeScan();
            }
            */
            App.Invoke(new Action(() =>
            {
                this.mainGrid.Visibility = Visibility.Collapsed;
            }));
            try
            {
                // 首次设置密码
                if (App.IsLockingPasswordEmpty())
                {
                REDO_SET:
                    var password = GetPassword("首次设置锁屏密码");
                    if (password == null)
                    {
                        ErrorBox("放弃设置锁屏密码", "yellow", "auto_close");
                        this.NavigationService.Navigate(PageMenu.MenuPage);
                        return;
                    }
                    if (string.IsNullOrEmpty(password))
                    {
                        ErrorBox("锁屏密码不允许设置为空", "red", "auto_close");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        goto REDO_SET;
                    }
                    App.SetLockingPassword(password);
                    return;
                }


                // 验证锁屏密码
                {
                REDO:
                    if (passwordErrorCount > 5)
                    {
                        ErrorBox("密码错误次数太多，功能被禁用", "red", "auto_close");
                        // 延时 10 分钟清除 passwordErrorCount
                        if (delayClear == null)
                        {
                            delayClear = Task.Run(async () =>
                            {
                                await Task.Delay(TimeSpan.FromMinutes(5));
                                passwordErrorCount = 0;
                                delayClear = null;
                            });
                        }

                        this.NavigationService.Navigate(PageMenu.MenuPage);
                        return;
                    }
                    var password = GetPassword("验证锁屏密码");
                    if (password == null)
                    {
                        PageMenu.RetunMenuPage();
                        // this.NavigationService.Navigate(PageMenu.MenuPage);
                        return;
                    }
                    if (App.MatchLockingPassword(password) == false)
                    {
                        passwordErrorCount++;
                        ErrorBox("密码不正确", "red", "auto_close");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        goto REDO;
                    }
                }
            }
            finally
            {
                App.Invoke(new Action(() =>
                {
                    this.mainGrid.Visibility = Visibility.Visible;
                }));
            }
        }

        string GetPassword(string title)
        {
            string password = null;
            App.Invoke(new Action(() =>
            {
                InputPasswordWindows dialog = null;
                App.PauseBarcodeScan();
                try
                {
                    dialog = new InputPasswordWindows();

                    this.MemoryDialog(dialog);

                    dialog.TitleText = title;   // $"验证锁屏密码";
                    dialog.Owner = App.CurrentApp.MainWindow;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    dialog.LoginButtonText = "确定";
                    dialog.ShowDialog();

                    this.ForgetDialog(dialog);

                    if (dialog.Result == "OK")
                        password = dialog.password.Password;
                }
                finally
                {
                    App.ContinueBarcodeScan();
                }

                // dialog_result = _passwordDialog.Result;
            }));

            return password;
        }

        void ErrorBox(string message,
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
                progress.Closed += (o1, e1) =>
                {
                    //RemoveLayer();
                };
                if (StringUtil.IsInList("button_ok", style))
                    progress.okButton.Content = "确定";
                progress.Show();
                //AddLayer();
            }));


            if (StringUtil.IsInList("auto_close", style))
            {
                DisplayMessage(progress, message, color);

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
                DisplayError(ref progress, message, color);
        }

        void DisplayError(ref ProgressWindow progress,
string message,
string color = "red")
        {
            if (progress == null)
                return;
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

        void DisplayMessage(ProgressWindow progress,
            string message,
            string color = "")
        {
            App.Invoke(new Action(() =>
            {
                progress.MessageText = message;
                if (string.IsNullOrEmpty(color) == false)
                    progress.BackColor = color;
            }));
        }


#if REMOVED
        private void Keyborad_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == '\r')
            {
                OnEnter();
                return;
            }

            this.password.Password = this.keyborad.Text;
        }
#endif

#if REMOVED
        void InitialPage()
        {
            if (App.IsLockingPasswordEmpty())
            {
                this.passwordArea.Visibility = Visibility.Visible;
                this.buttonArea.Visibility = Visibility.Collapsed;

                this.setPassword.Visibility = Visibility.Visible;
                this.login.Visibility = Visibility.Collapsed;

                this.menu.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (HasLoggedin == false)
                {
                    this.passwordArea.Visibility = Visibility.Visible;
                    this.buttonArea.Visibility = Visibility.Collapsed;

                    this.setPassword.Visibility = Visibility.Collapsed;
                    this.login.Visibility = Visibility.Visible;

                    this.menu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.passwordArea.Visibility = Visibility.Collapsed;
                    this.buttonArea.Visibility = Visibility.Visible;

                    this.menu.Visibility = Visibility.Visible;
                }
            }
        }
#endif

#if NO
        static App App
        {
            get
            {
                return ((App)Application.Current);
            }
        }
#endif

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
        private bool _hasLoggedin = false;

        public bool HasLoggedin
        {
            get
            {
                return _hasLoggedin;
            }
            set
            {
                if (_hasLoggedin != value)
                {
                    _hasLoggedin = value;
                    OnPropertyChanged("HasLoggedin");
                }
            }
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

        #endregion


        private void Config_Click(object sender, RoutedEventArgs e)
        {
            //FingerprintManager.Base.State = "pause";
            var old_seconds = App.AutoBackMainMenuSeconds;

            App.PauseBarcodeScan();
            try
            {
                Window cfg_window = new ConfigWindow();

                this.MemoryDialog(cfg_window);

                cfg_window.Owner = App.CurrentApp.MainWindow;
                cfg_window.ShowDialog();

                this.ForgetDialog(cfg_window);
            }
            finally
            {
                //FingerprintManager.Base.State = "";
                App.ContinueBarcodeScan();
            }

            if (old_seconds != App.AutoBackMainMenuSeconds)
                PageMenu.MenuPage.SetIdleEvents();

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken))
            {
                ProgressWindow progress = null;
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "兑现参数";
                    progress.MessageText = "正在兑现参数，请稍等 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s1, e1) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "middle");
                    progress.BackColor = "black";
                    progress.Show();

                    MemoryDialog(progress);
                }));
                try
                {
                    // testing
                    // Thread.Sleep(1000 * 5);

                    // 迫使 URL 生效
                    RfidManager.Url = App.RfidUrl;
                    RfidManager.Clear();
                    FingerprintManager.Url = App.FingerprintUrl;
                    FingerprintManager.Clear();
                    FaceManager.Url = App.FaceUrl;
                    FaceManager.Clear();

                    if (App.Function == "智能书柜")
                    {
                        // 迫使 RfidManager.ReaderNameList 反应最新变化
                        ShelfData.RefreshReaderNameList();
                    }

                    // 2019/6/19
                    // 主动保存一次参数配置
                    WpfClientInfo.SaveConfig(App.GrantAccess);

                    // 检查状态，及时报错
                    {
                        List<string> errors = new List<string>();
                        {
                            var result = RfidManager.GetState("");
                            if (result.Value == -1)
                                errors.Add(result.ErrorInfo);
                        }

                        {
                            var result = FingerprintManager.GetState("");
                            if (result.Value == -1)
                                errors.Add(result.ErrorInfo);
                        }

                        {
                            var result = FaceManager.GetState("camera");
                            if (result.Value == -1)
                                errors.Add(result.ErrorInfo);
                        }

                        {
                            // 检查 Server UID 关系
                            var check_result = CheckServerUID();
                            if (check_result.Value == -1)
                                errors.Add(check_result.ErrorInfo);
                        }

                        if (errors.Count > 0)
                            MessageBox.Show(StringUtil.MakePathList(errors, "\r\n"));
                    }

                    PageMenu.MenuPage?.UpdateMenu();

                    if (App.Function == "智能书柜")
                    {
                        // 2019/12/9
                        App.InitialShelfCfg();

                        // TODO: 可能会抛出异常
                        // 因为 Doors 发生了变化，所以要重新初始化门控件
                        PageMenu.PageShelf?.InitialDoorControl();
                    }

                    ShelfData.l_RefreshCount();

                    // 重新启动 Proccess 监控
                    App.CurrentApp.StartProcessManager();

                    _ = App.ConnectMessageServerAsync();

                    // 切断 SIP2 通道。因为可能刚才在配置参数中修改了 SIP2 通道的编码方式
                    SipChannelUtil.CloseChannel();
                }
                finally
                {
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                        ForgetDialog(progress);
                    }));
                }
            }
        }

        const string dp2library_base_version = "3.37";
        const string fingerprintcenter_base_version = "2.1";
        const string rfidcenter_base_version = "1.11";
        const string facecenter_base_version = "1.3";

        public static NormalResult CheckServerUID()
        {
            // 如果没有配置 dp2library URL 则不检查
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new NormalResult();

            string dp2library_uid = "";
            string version = "";

            // 获得 dp2library 服务器的 UID
            var channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetVersion(null,
                    out version,
                    out dp2library_uid,
                    out string strError);
                if (lRet == -1)
                {
                    // not response
                    if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.RequestError)
                        return new NormalResult();

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"校验 UID 失败。获得 dp2library 服务器 UID 失败: {strError}",
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp?.ReturnChannel(channel);
            }

            List<string> errors = new List<string>();

            // 检查 dp2library 版本号
            if (App.Function == "智能书柜"
                && StringUtil.CompareVersion(version, dp2library_base_version) < 0)
            {
                errors.Add($"智能书柜功能要求连接的 dp2library 服务器版本在 {dp2library_base_version} 以上(但当前是 {version})");
            }

            // 如果没有配置 指纹中心 URL 则不检查
            if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
            {
                var version_result = FingerprintManager.GetVersion();
                if (version_result.Value == -1)
                {
                    errors.Add(version_result.ErrorInfo);
                    goto SKIP1;
                }

                if (StringUtil.CompareVersion(version_result.Version, fingerprintcenter_base_version) < 0)
                {
                    // 版本太低，无法进行 UID 检查
                    goto SKIP1;
                }
                var result = FingerprintManager.GetState("getLibraryServerUID");
                if (result.Value == -1)
                {
                    if (result.ErrorCode != "notResponse")
                        errors.Add(result.ErrorInfo);
                    goto SKIP1;
                }

                string fingerprint_uid = result.ErrorCode;
                if (string.IsNullOrEmpty(fingerprint_uid))
                    errors.Add("针对指纹中心请求 getLibraryServerUID 失败，返回的 UID 为空，无法检查核对 UID");
                else
                {
                    if (fingerprint_uid != dp2library_uid)
                        errors.Add($"dp2SSL 直连的 dp2library 服务器的 UID ('{dp2library_uid}') 和指纹中心所连接的 dp2library UID ('{fingerprint_uid}') 不同。请检查配置参数并重新配置");
                }
            }

        SKIP1:

            // 如果没有配置 人脸中心 URL 则不检查
            if (string.IsNullOrEmpty(App.FaceUrl) == false)
            {
                var version_result = FaceManager.GetState("getVersion");
                if (version_result.Value == -1 && version_result.ErrorCode == "notResponse")
                {
                    // errors.Add(version_result.ErrorInfo);
                }
                else
                {
                    if (version_result.Value == -1 && version_result.ErrorCode == "System.Exception")
                    {
                        errors.Add(version_result.ErrorInfo);
                    }
                    else if (version_result.Value == -1 || version_result.ErrorCode == null)
                        errors.Add("所连接的人脸中心版本太低。请升级到最新版本");
                    else
                    {
                        try
                        {
                            if (StringUtil.CompareVersion(version_result.ErrorCode, facecenter_base_version) < 0)
                                errors.Add($"所连接的人脸中心版本太低(为 {version_result.ErrorCode} 版)。请升级到 {rfidcenter_base_version} 以上版本");
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"所连接的人脸中心版本太低。请升级到最新版本。({ex.Message})");
                        }
                    }

                    var result = FaceManager.GetState("getLibraryServerUID");
                    if (result.Value == -1)
                    {
                        if (result.ErrorCode != "notResponse")
                            errors.Add(result.ErrorInfo);
                    }
                    else
                    {
                        string face_uid = result.ErrorCode;
                        if (string.IsNullOrEmpty(face_uid))
                            errors.Add("针对人脸中心请求 getLibraryServerUID 失败，返回的 UID 为空，无法检查核对 UID");
                        else
                        {
                            if (face_uid != dp2library_uid)
                                errors.Add($"dp2SSL 直连的 dp2library 服务器的 UID ('{dp2library_uid}') 和人脸中心所连接的 dp2library UID ('{face_uid}') 不同。请检查配置参数并重新配置");
                        }
                    }
                }
            }

            // 如果没有配置 RFID 中心 URL 则不检查
            if (string.IsNullOrEmpty(RfidManager.Url/*App.RfidUrl*/) == false)
            {
                var result = RfidManager.GetState("getVersion");
                if (result.Value == -1)
                    errors.Add("所连接的 RFID 中心版本太低。请升级到最新版本");
                else
                {
                    try
                    {
                        if (StringUtil.CompareVersion(result.ErrorCode, rfidcenter_base_version) < 0)
                            errors.Add($"所连接的 RFID 中心版本太低(为 {result.ErrorCode} 版)。请升级到 {rfidcenter_base_version} 以上版本");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"所连接的 RFID 中心版本太低。请升级到最新版本。({ex.Message})");
                    }
                }
            }

            if (errors.Count > 0)
                return new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, ";\r\n") };

            return new NormalResult();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

#if REMOVED
        // 首次设置密码
        private void SetPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Error = null;

            if (string.IsNullOrEmpty(this.password.Password))
            {
                this.Error = "密码不允许为空";
                return;
            }

            App.SetLockingPassword(this.password.Password);
            InitialPage();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            this.Error = null;

            if (App.MatchLockingPassword(this.password.Password) == false)
            {
                this.Error = "密码不正确，登录失败";
                return;
            }

            this.HasLoggedin = true;
            InitialPage();
        }
#endif

        private void OpenProgramFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionUtil.GetAutoText(ex));
            }
        }

        private void OpenUserFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(WpfClientInfo.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionUtil.GetAutoText(ex));
            }
        }

        private void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(WpfClientInfo.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionUtil.GetAutoText(ex));
            }
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void DownloadDailyWallpaper_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            this.downloadDailyWallpaper.IsEnabled = false;
            try
            {
                string filename = Path.Combine(WpfClientInfo.UserDir, "daily_wallpaper");
                await DownloadBingWallPaperAsync(filename);
            }
            finally
            {
                this.downloadDailyWallpaper.IsEnabled = true;
            }
        }

#if REMOVED
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
#endif

        // https://blog.csdn.net/m0_37682004/article/details/82314055
        Task DownloadBingWallPaperAsync(string filename)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {

                        ProgressWindow progress = null;
                        App.Invoke(new Action(() =>
                        {
                            progress = new ProgressWindow();
                            progress.MessageText = "正在下载 bing 壁纸，请稍候 ...";
                            progress.Owner = Application.Current.MainWindow;
                            progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            progress.Closed += (sender, e) =>
                            {
                                client.CancelAsync();
                            };
                            progress.Show();
                        }));

                        try
                        {
                            byte[] bytes = client.DownloadData("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN");
                            dynamic obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes));
                            string url = obj.images[0].url;
                            url = $"https://cn.bing.com{url}";
                            client.DownloadFile(url, filename);

                            // _dialogs.Add(progress);
                            MemoryDialog(progress);

                            App.Invoke(new Action(() =>
                            {
                                progress.MessageText = "下载完成";
                                progress.BackColor = "green";
                                progress = null;
                            }));
                        }
                        catch (Exception ex)
                        {
                            // _dialogs.Add(progress);
                            MemoryDialog(progress);

                            App.Invoke(new Action(() =>
                            {
                                progress.MessageText = $"下载 bing 壁纸过程出现异常: {ExceptionUtil.GetExceptionText(ex)}";
                                progress.BackColor = "red";
                                progress = null;
                            }));
                        }
                        finally
                        {
                            App.Invoke(new Action(() =>
                            {
                                if (progress != null)
                                    progress.Close();
                            }));
                        }

                        /*
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show(message);
                        }));
                        */
                    }
                }
                catch
                {
                    // TODO: 写入错误日志
                }
            });
        }

        private void AddShortcut_Click(object sender, RoutedEventArgs e)
        {
            WpfClientInfo.AddShortcutToStartupGroup("dp2SSL-自助借还");
            MessageBox.Show("快捷方式添加成功");
        }

        private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
        {
            WpfClientInfo.RemoveShortcutFromStartupGroup("dp2SSL-自助借还", true);
            MessageBox.Show("快捷方式删除成功");
        }

        private void RestartRfidCenter_Click(object sender, RoutedEventArgs e)
        {
            RfidManager.GetState("restart");
        }

        private void RestartFingerprintCenter_Click(object sender, RoutedEventArgs e)
        {
            FingerprintManager.GetStateRestart();
        }

        private void RestartFaceCenter_Click(object sender, RoutedEventArgs e)
        {
            FaceManager.GetState("restart");
        }

#if REMOVED
        private void Password_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                OnEnter();
                return;
            }
        }

        void OnEnter()
        {
            if (this.setPassword.Visibility == Visibility.Visible)
            {
                SetPassword_Click(this.setPassword, new RoutedEventArgs());
            }
            else if (this.login.Visibility == Visibility.Visible)
            {
                Login_Click(this.login, new RoutedEventArgs());
            }
        }
#endif

        // 紫外线杀菌
        private void sterilamp_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentApp.BeginSterilamp();
        }

        private void redoReplicatePatron_Click(object sender, RoutedEventArgs e)
        {
            ShelfData.RedoReplicatePatron();
            App.ErrorBox("全量同步读者记录", "全量同步读者记录的操作已安排", "green");
        }

        private async void backupRequests_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.Title = "备份本地动作库 - 请指定输出文件";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);    // WpfClientInfo.UserDir;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "XML 文件(*.xml)|*.xml|所有文件(*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
                return;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken))
            {
                ProgressWindow progress = null;
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "备份本地动作库";
                    progress.MessageText = "正在备份本地动作库，请稍等 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s1, e1) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "middle");
                    progress.BackColor = "black";
                    progress.Show();
                }));
                try
                {
                    var result = await ShelfData.BackupRequestsDatabaseAsync(
                        openFileDialog.FileName,
                        (text) =>
                        {
                            App.Invoke(new Action(() =>
                            {
                                progress.MessageText = text;
                            }));
                        },
                        cancel.Token);
                    if (result.Value == -1)
                        App.ErrorBox("备份本地动作库", $"导出过程出错: {result.ErrorInfo}");
                    else
                        App.ErrorBox("备份本地动作库", $"导出完成。在文件 {openFileDialog.FileName} 中，共导出记录 {result.Value} 条", "green");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"备份本地动作库过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.ErrorBox("备份本地动作库", $"备份本地动作库过程出现异常: {ex.Message}");
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

        private async void restoreRequests_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "恢复本地动作库 - 请指定输入文件";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);    // WpfClientInfo.UserDir;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "XML 文件(*.xml)|*.xml|所有文件(*.*)|*.*";
            if (openFileDialog.ShowDialog() == false)
                return;

            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(App.CancelToken))
            {
                ProgressWindow progress = null;
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "恢复本地动作库";
                    progress.MessageText = "正在恢复本地动作库，请稍等 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s1, e1) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Content = "停止";
                    progress.Background = new SolidColorBrush(Colors.DarkRed);
                    App.SetSize(progress, "middle");
                    progress.BackColor = "black";
                    progress.Show();
                }));
                try
                {
                    var result = await ShelfData.RestoreRequestsDatabaseAsync(
                        openFileDialog.FileName,
                        (text) =>
                        {
                            App.Invoke(new Action(() =>
                            {
                                progress.MessageText = text;
                            }));
                        },
                        cancel.Token);
                    if (result.Value == -1)
                        App.ErrorBox("恢复本地动作库", $"恢复过程出错: {result.ErrorInfo}");
                    else
                        App.ErrorBox("恢复本地动作库", $"恢复完成。共导入记录 {result.Value} 条", "green");
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"恢复本地动作库过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    App.ErrorBox("恢复本地动作库", $"恢复本地动作库过程出现异常: {ex.Message}");
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

        // 打开触摸键盘
        private void openTouchKeyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("osk");
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"打开触摸键盘时出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // 小票打印机清空缓冲区
        private void posPrint_clearMemory_Click(object sender, RoutedEventArgs e)
        {
            var result = RfidManager.PosPrint("init", "", "");
            if (result.Value == -1)
                ErrorBox($"清空小票打印机缓冲区时出错: {result.ErrorInfo}");
            else
                ErrorBox("成功清空缓冲区", "green");
        }

        private void posPrint_cutPaper_Click(object sender, RoutedEventArgs e)
        {
            var result = RfidManager.PosPrint("cut", "", "");
            if (result.Value == -1)
                ErrorBox($"切纸时出错: {result.ErrorInfo}");
        }

        private void posPrint_feed_Click(object sender, RoutedEventArgs e)
        {
            var result = RfidManager.PosPrint("feed", "", "");
            if (result.Value == -1)
                ErrorBox($"走纸时出错: {result.ErrorInfo}");
        }

        // 禁用边沿 UI
        // 不管是 ClickOnce 状态还是绿色状态，都可以使用本命令
        private void disableEdgeUI_Click(object sender, RoutedEventArgs e)
        {
            var result = App.DisableEdgeUI();
            if (result.Value == 1)
                return; // Application 马上会重新启动
            if (result.Value == -1)
                ErrorBox(result.ErrorInfo, "red");
            else
                ErrorBox("Windows 注册表参数已经成功修改", "green");
        }

        private void shutdownButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确实要关机？",
    "请选择启动模式",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question,
    MessageBoxResult.No,
    MessageBoxOptions.DefaultDesktopOnly);
            if (result == MessageBoxResult.No)
                return;

            WpfClientInfo.WriteInfoLog("用户命令关机");

            ShutdownUtil.DoExitWindows(ShutdownUtil.ExitWindows.ShutDown);
        }

        // 检测网络
        private void detectNetwork_Click(object sender, RoutedEventArgs e)
        {
            // ping dp2003.com
            var ret = NetUtil.Ping("dp2003.com", out string strInfomation);
            if (ret == false)
                App.ErrorBox("检测网络", "网络不通");
            else
                App.ErrorBox("检测网络", "网络通畅", "green");

            // 尝试连接 dp2library
        }

        // 2020/12/3
        // 清除 RFID 标签缓存
        private void clearTagCache_Click(object sender, RoutedEventArgs e)
        {
            ShelfData.BookTagList.ClearTagTable(null);
            ShelfData.PatronTagList.ClearTagTable(null);
        }

        private void clearCachedEntities_Click(object sender, RoutedEventArgs e)
        {
            LibraryChannelUtil.ClearCachedEntities();
            App.ErrorBox("删除本地缓存的册记录", "完成", "green");
        }

        // 重做全量同步册记录和书目摘要
        private void redoReplicateEntity_Click(object sender, RoutedEventArgs e)
        {
            ShelfData.RestartReplicateEntities();
            App.ErrorBox("全量同步册记录和书目摘要", "全量同步册记录和书目摘要的操作已安排", "green");
        }
    }
}
