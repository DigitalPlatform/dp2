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

using dp2SSL.Models;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        CancellationTokenSource _cancelRefresh = new CancellationTokenSource();

        CancellationTokenSource _cancelProcessMonitor = new CancellationTokenSource();


        Mutex myMutex;

        ErrorTable _errorTable = null;

        #region 属性

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
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

        protected async override void OnStartup(StartupEventArgs e)
        {
            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("dp2SSL 不允许重复启动");
                App.Current.Shutdown();
                return;
            }

            if (DetectVirus.Detect360() || DetectVirus.DetectGuanjia())
            {
                MessageBox.Show("dp2SSL 被木马软件干扰，无法启动");
                System.Windows.Application.Current.Shutdown();
                return;
            }

            _errorTable = new ErrorTable((s) =>
            {
                this.Error = s;
            });

            WpfClientInfo.TypeOfProgram = typeof(App);
            if (StringUtil.IsDevelopMode() == false)
                WpfClientInfo.PrepareCatchException();

            WpfClientInfo.Initial("dp2SSL");
            base.OnStartup(e);

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            // InitialFingerPrint();

            // 后台自动检查更新
            var task = Task.Run(() =>
            {
                NormalResult result = WpfClientInfo.InstallUpdateSync();
                if (result.Value == -1)
                    OutputHistory("自动更新出错: " + result.ErrorInfo, 2);
                else if (result.Value == 1)
                    OutputHistory(result.ErrorInfo, 1);
                else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                    OutputHistory(result.ErrorInfo, 0);
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
            FingerprintManager.Start(_cancelRefresh.Token);

            RfidManager.Base.Name = "RFID 中心";
            RfidManager.Url = App.RfidUrl;
            RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;
            RfidManager.Start(_cancelRefresh.Token);

            FaceManager.Base.Name = "人脸中心";
            FaceManager.Url = App.FaceUrl;
            FaceManager.SetError += FaceManager_SetError;
            FaceManager.Start(_cancelRefresh.Token);

            // 自动删除以前残留在 UserDir 中的全部临时文件
            // 用 await 是需要删除完以后再返回，这样才能让后面的 PageMenu 页面开始使用临时文件目录
            await Task.Run(() =>
            {
                DeleteLastTempFiles();
            });

            StartProcessManager();

            BeginCheckServerUID(_cancelRefresh.Token);
        }

        // 单独的线程，监控 server UID 关系
        public void BeginCheckServerUID(CancellationToken token)
        {
            var task1 = Task.Run(() =>
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

                        Task.Delay(TimeSpan.FromMinutes(5)).Wait(token);
                    }
                }
                catch(OperationCanceledException)
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
                        WpfClientInfo.Log?.Info($"{info.Name} {text}");
                    },
                    _cancelProcessMonitor.Token);
            }
        }

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

        private void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetError("rfid", e.Error);
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
            LibraryChannelManager.Log.Debug("OnSessionEnding() called");
            WpfClientInfo.Finish();
            LibraryChannelManager.Log.Debug("End WpfClientInfo.Finish()");

            _cancelRefresh?.Cancel();
            _cancelProcessMonitor?.Cancel();

            base.OnSessionEnding(e);
        }

        // 注：Windows 关机或者重启的时候，会触发 OnSessionEnding 事件，但不会触发 OnExit 事件
        protected override void OnExit(ExitEventArgs e)
        {
            LibraryChannelManager.Log.Debug("OnExit() called");
            WpfClientInfo.Finish();
            LibraryChannelManager.Log.Debug("End WpfClientInfo.Finish()");

            _cancelRefresh.Cancel();
            _cancelProcessMonitor?.Cancel();

            // EndFingerprint();

            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this._channelPool.Close();

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
                return (bool)WpfClientInfo.Config?.GetBoolean("operation", "auto_trigger", false);
            }
        }

        public static bool ProcessMonitor
        {
            get
            {
                return (bool)WpfClientInfo.Config?.GetBoolean("global",
                    "process_monitor",
                    true);
            }
        }

        public static string dp2Password
        {
            get
            {
                return DecryptPasssword(WpfClientInfo.Config.Get("global", "dp2Password", ""));
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

        internal void Channel_BeforeLogin(object sender,
DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
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

        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public void AbortAllChannel()
        {
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel()
        {
            string strServerUrl = dp2ServerUrl;

            string strUserName = dp2UserName;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
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

        protected override void OnActivated(EventArgs e)
        {
            // 单独线程执行，避免阻塞 OnActivated() 返回
            Task.Run(() =>
            {
                FingerprintManager.EnableSendkey(false);
                RfidManager.EnableSendkey(false);
            });
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
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

        public void SetError(string type, string error)
        {
            /*
            if (type == "face" && error != null)
            {
                Debug.Assert(false, "");
            }
            */

            _errorTable.SetError(type, error);
        }

        public void ClearErrors(string type)
        {
            // _errors.Clear();
            _errorTable.SetError(type, "");
        }

        public event TagChangedEventHandler TagChanged = null;
        // public event SetErrorEventHandler TagSetError = null;

        private void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 标签总数显示
            // this.Number = e.Result?.Results?.Count.ToString();
            if (e.Result.Results != null)
            {
                TagList.Refresh(sender as BaseChannel<IRfid>, e.Result.Results,
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
                            RfidManager.TriggerSetError(this, new SetErrorEventArgs { Error = text });
                            // TagSetError?.Invoke(this, new SetErrorEventArgs { Error = text });
                        });

                // 标签总数显示 图书+读者卡
                this.Number = $"{TagList.Books.Count}:{TagList.Patrons.Count}";
            }
        }
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
}
