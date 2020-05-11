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

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.RFID;
using DigitalPlatform.Face;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// PageSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PageSetting : Page, INotifyPropertyChanged
    {
        public PageSetting()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Loaded += PageSetting_Loaded;
            this.Unloaded += PageSetting_Unloaded;

            this.keyborad.KeyPressed += Keyborad_KeyPressed;
        }

        private void PageSetting_Unloaded(object sender, RoutedEventArgs e)
        {
            // 确保 page 关闭时对话框能自动关闭
            App.Invoke(new Action(() =>
            {
                foreach (var window in _dialogs)
                {
                    window.Close();
                }
            }));
            App.ContinueBarcodeScan();
        }

        private void PageSetting_Loaded(object sender, RoutedEventArgs e)
        {
            // 首次设置密码，或者登录
            InitialPage();
            if (this.password.Visibility == Visibility.Visible)
                this.password.Focus();

            App.PauseBarcodeScan();
        }

        private void Keyborad_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == '\r')
            {
                OnEnter();
                return;
            }

            this.password.Password = this.keyborad.Text;
        }

        void InitialPage()
        {
            if (App.IsLockingPasswordEmpty())
            {
                this.passwordArea.Visibility = Visibility.Visible;
                this.buttonArea.Visibility = Visibility.Collapsed;

                this.setPassword.Visibility = Visibility.Visible;
                this.login.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (HasLoggedin == false)
                {
                    this.passwordArea.Visibility = Visibility.Visible;
                    this.buttonArea.Visibility = Visibility.Collapsed;

                    this.setPassword.Visibility = Visibility.Collapsed;
                    this.login.Visibility = Visibility.Visible;
                }
                else
                {
                    this.passwordArea.Visibility = Visibility.Collapsed;
                    this.buttonArea.Visibility = Visibility.Visible;
                }
            }
        }

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
            App.PauseBarcodeScan();
            try
            {
                Window cfg_window = new ConfigWindow();
                cfg_window.Owner = App.CurrentApp.MainWindow;
                cfg_window.ShowDialog();
            }
            finally
            {
                //FingerprintManager.Base.State = "";
                App.ContinueBarcodeScan();
            }

            // 迫使 URL 生效
            RfidManager.Url = App.RfidUrl;
            RfidManager.Clear();
            FingerprintManager.Url = App.FingerprintUrl;
            FingerprintManager.Clear();
            FaceManager.Url = App.FaceUrl;
            FaceManager.Clear();
            // 迫使 RfidManager.ReaderNameList 反应最新变化
            ShelfData.RefreshReaderNameList();

            // 2019/6/19
            // 主动保存一次参数配置
            WpfClientInfo.SaveConfig();

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

            // 2019/12/9
            App.CurrentApp.InitialShelfCfg();
            // 因为 Doors 发生了变化，所以要重新初始化门控件
            PageMenu.PageShelf?.InitialDoorControl();
            ShelfData.l_RefreshCount();

            // 重新启动 Proccess 监控
            App.CurrentApp.StartProcessManager();

            _ = App.CurrentApp.ConnectMessageServerAsync();
        }

        const string dp2library_base_version = "3.27";
        const string fingerprintcenter_base_version = "2.1";

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

            // 如果没有配置 RFID 中心 URL 则不检查
            if (string.IsNullOrEmpty(App.RfidUrl) == false)
            {
                var result = RfidManager.GetState("getVersion");
                if (result.Value == -1)
                    errors.Add("所连接的 RFID 中心版本太低。请升级到最新版本");
                else
                {
                    try
                    {
                        if (StringUtil.CompareVersion(result.ErrorCode, "1.5") < 0)
                            errors.Add($"所连接的 RFID 中心版本太低(为 {result.ErrorCode} 版)。请升级到 1.5 以上版本");
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

        List<Window> _dialogs = new List<Window>();

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
                            _dialogs.Add(progress);
                            App.Invoke(new Action(() =>
                            {
                                progress.MessageText = "下载完成";
                                progress.BackColor = "green";
                                progress = null;
                            }));
                        }
                        catch (Exception ex)
                        {
                            _dialogs.Add(progress);
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
            FingerprintManager.GetState("restart");
        }

        private void RestartFaceCenter_Click(object sender, RoutedEventArgs e)
        {
            FaceManager.GetState("restart");
        }

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

        // 紫外线杀菌
        private void sterilamp_Click(object sender, RoutedEventArgs e)
        {
            _ = App.CurrentApp.SterilampAsync();
        }
    }
}
