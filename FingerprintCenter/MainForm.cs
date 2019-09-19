using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Speech.Synthesis;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Web;
using System.Text;
using System.Media;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;

using static FingerprintCenter.FingerPrint;

using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.CirculationClient;
using static DigitalPlatform.CirculationClient.BioUtil;

namespace FingerprintCenter
{
    public partial class MainForm : Form
    {
        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        FloatingMessageForm _floatingMessage = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public MainForm()
        {
            ClientInfo.ProgramName = "fingerprintcenter";
            ClientInfo.MainForm = this;

            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

            UsbInfo.StartWatch((add_count, remove_count) =>
            {
                // this.OutputHistory($"add_count:{add_count}, remove_count:{remove_count}", 1);
                string type = "disconnected";
                if (add_count > 0)
                    type = "connected";

                BeginRefreshReaders(type, new CancellationToken());
            },
            new CancellationToken());

            // UsbNotification.RegisterUsbDeviceNotification(this.Handle);
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    Task.Run(() =>
                    {
                        Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                        this.Speak("指纹中心被唤醒");
                        BeginRefreshReaders("connected", new CancellationToken());
                    });
                    break;
                case PowerModes.Suspend:
                    break;
            }
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    this.textBox_cfg_password,
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_speak, true),
                    new ControlWrapper(this.checkBox_beep, true),
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                    this.comboBox_deviceList,
                    this.textBox_cfg_shreshold
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    this.textBox_cfg_password,
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_speak, true),
                    new ControlWrapper(this.checkBox_beep, true),
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                    this.comboBox_deviceList,
                    this.textBox_cfg_shreshold
                };
                GuiState.SetUiState(controls, value);
            }
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }

        // FingerprintServer _server = new FingerprintServer();

        FingerPrint FingerPrint = new FingerPrint();

        private void Form1_Load(object sender, EventArgs e)
        {
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.BalloonTipText = "指纹中心已经启动";
                notifyIcon1.ShowBalloonTip(1000);
            }

            ClientInfo.SetErrorState("retry", "正在启动");

            if (DetectVirus.DetectXXX() || DetectVirus.DetectGuanjia())
            {
                MessageBox.Show(this, "fingerprintcenter 被木马软件干扰，无法启动");
                Application.Exit();
                return;
            }

            // ClientInfo.SerialNumberMode = "must";
            ClientInfo.CopyrightKey = "fingerprintcenter_sn_key";
            ClientInfo.Initial("fingerprintcenter");
            this.UiState = ClientInfo.Config.Get("global", "ui_state", ""); // Properties.Settings.Default.ui_state;

            if (StringUtil.IsDevelopMode() == false)
                MenuItem_testing.Visible = false;

#if NO
            this._repPlan = JsonConvert.DeserializeObject<ReplicationPlan>(Properties.Settings.Default.repPlan);
            if (this._repPlan == null)
                this._repPlan = new ReplicationPlan();
#endif
            this.textBox_replicationStart.Text = ClientInfo.Config.Get("global", "replication_start", "");   //  Properties.Settings.Default.repPlan;

            ClearHtml();

            // 显示版本号
            this.OutputHistory($"版本号: {ClientInfo.ClientVersion}");

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            // this.FingerPrint = new FingerPrint();
            Program.FingerPrint = this.FingerPrint;

            FingerPrint.Prompt += FingerPrint_Prompt;
            FingerPrint.ProgressChanged += FingerPrint_ProgressChanged;
            FingerPrint.Captured += FingerPrint_Captured;
            FingerPrint.Speak += FingerPrint_Speak;
            FingerPrint.ImageReady += FingerPrint_ImageReady;

            try
            {
                if (FingerprintServer.StartRemotingServer() == false)
                    return;
            }
            catch (Exception ex)
            {
                this.ShowMessage(ex.Message);
                return;
            }

            // "ipc://FingerprintChannel/FingerprintServer"
            // 通道打开成功后，窗口应该显示成一种特定的状态
            int nRet = StartChannel(
                "ipc://FingerprintChannel/FingerprintServer",
                out string strError);
            if (nRet == -1)
            {
                this.ShowMessage(strError, "red", true);
                return;
            }

            if (string.IsNullOrEmpty(this.textBox_cfg_dp2LibraryServerUrl.Text) == true)
            {
                Task.Run(() =>
                {
                    FirstSetup();
                });
            }
            else
            {
                var task = BeginStart();
            }

            // DisplayText("1");

            // 后台自动检查更新
            Task.Run(() =>
            {
                NormalResult result = ClientInfo.InstallUpdateSync();
                if (result.Value == -1)
                    OutputHistory("自动更新出错: " + result.ErrorInfo, 2);
                else if (result.Value == 1)
                    OutputHistory(result.ErrorInfo, 1);
                else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                    OutputHistory(result.ErrorInfo, 0);
            });

            if (ClientInfo.IsMinimizeMode())
            {
                Task.Run(() =>
                {
                    Task.Delay(2000).Wait();
                    this.BeginInvoke((Action)(() =>
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }));
                });
            }
        }

        // 指纹功能是否初始化成功
        bool _initialized = false;

        async Task BeginStart()
        {
            this.ClearMessage();
            this.Invoke((Action)(() =>
            {
                toolStripStatusLabel_message.Text = "";
            }));

            if (_initialized == true)
                return;

            this.ShowMessage("开始启动");
            if (_refreshCount > 0)
                this.OutputHistory("(重试)重新创建指纹缓存");
            else
                this.OutputHistory("重新创建指纹缓存");
            await Task.Run(() =>
            {
                NormalResult result = StartFingerPrint();
                if (result.Value == -1)
                {
                    string strError = "指纹功能启动失败: " + result.ErrorInfo;
                    Speak(strError, true);
                    this.ShowMessage(strError, "red", true);
                    OutputHistory(strError, 2);

                    if (result.ErrorCode == "driver not install")
                    {
                        Task.Run(() => { InstallDriver("您的电脑上尚未安装'中控'指纹仪厂家驱动。"); });
                    }
                    else
                    {

                    }
                }
                else
                {
                    _initialized = true;
                    Speak("指纹功能启动成功");
                    OutputHistory("指纹功能启动成功", 0);
                }
            });
        }

        private static readonly Object _syncRoot_start = new Object(); // 2019/5/20

        NormalResult StartFingerPrint()
        {
            lock (_syncRoot_start)
            {
                DisplayText("StartFingerPrint ...");

                _cancel.Cancel();

                _cancel.Dispose();
                _cancel = new CancellationTokenSource();

                DisplayText("正在初始化指纹环境 ...");
                DisplayText("正在打开指纹设备 ...");

                try
                {
                    FingerPrint.Free();
                    NormalResult result = FingerPrint.Init(CurrentDeviceIndex);
                    if (result.Value == -1)
                    {
                        ClientInfo.SetErrorState("error", result.ErrorInfo);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"StartFingerPrint() 出现异常: {ex.Message}"
                    };
                }

                UpdateDeviceList();
#if NO

            result = FingerPrint.OpenZK();
            if (result.Value == -1)
                return result;
#endif

                DisplayText("Init Cache ...");

#if REMOVED
            {
                // 初始化指纹缓存
                // return:
                //      -1  出错
                //      0   没有获得任何数据
                //      >=1 获得了数据
                var result = InitFingerprintCache();
                if (result.Value == -1)
                {

                    // 开始捕捉指纹
                    FingerPrint.StartCapture(_cancel.Token);
                    // 如果是请求 dp2library 服务器出错，则依然要启动 timer，这样可以自动每隔一段时间重试初始化
                    // TODO: 界面上要出现醒目的警告(或者不停语音提示)，表示请求 dp2library 出错，从而没有任何读者指纹信息可供识别时候利用
                    if (result.ErrorCode == "RequestError"
                        || result.ErrorCode == "NotLogin")
                    {
                        // TODO: 要提醒用户，此时没有初始化成功，但后面会重试
                        SetErrorState("retry");
                        StartTimer();
                    }
                    else
                    {
                        // TODO: 需要进入警告状态(表示软件后面不会自动重试)，让工作人员明白必须介入
                        SetErrorState("error");
                    }

                    return result;
                }
                else
                {
                    SetErrorState("normal");
                }
                if (result.Value == 0)
                    this.ShowMessage(result.ErrorInfo, "yellow", true);
            }
#endif
                {
                    var result = TryInitFingerprintCache();
                    if (result.Value == -1)
                        return result;
                }

                // 开始捕捉指纹
                FingerPrint.StartCapture(_cancel.Token);

                //
                StartTimer();

                return new NormalResult();
            }
        }

        NormalResult TryInitFingerprintCache()
        {
            // 初始化指纹缓存
            // return:
            //      -1  出错
            //      0   没有获得任何数据
            //      >=1 获得了数据
            var result = _initFingerprintCache();
            if (result.Value == -1)
            {

                // 开始捕捉指纹
                FingerPrint.StartCapture(_cancel.Token);
                // 如果是请求 dp2library 服务器出错，则依然要启动 timer，这样可以自动每隔一段时间重试初始化
                // TODO: 界面上要出现醒目的警告(或者不停语音提示)，表示请求 dp2library 出错，从而没有任何读者指纹信息可供识别时候利用
                if (result.ErrorCode == "RequestError")
                {
                    StartTimer();
                }
                else
                    EndTimer();

                // result.ErrorCode == "NotLogin"
                // 密码不正确的情况不要自动重试。因为重试超过十次会让当前账户被加入黑名单十分钟

                if (_timer == null)
                    ClientInfo.SetErrorState("error", result.ErrorInfo);
                else
                    ClientInfo.SetErrorState("retry", result.ErrorInfo);

                return result;
            }
            else
            {
                ClientInfo.SetErrorState("normal", "");
            }
            if (result.Value == 0)
                this.ShowMessage(result.ErrorInfo, "yellow", true);

            return new NormalResult();
        }

        void StartTimer()
        {
            if (_timer == null)
            {
                TimeSpan period = TimeSpan.FromMinutes(5);  // 5 分钟

                if (string.IsNullOrEmpty(this.ServerVersion) == false
                    && StringUtil.CompareVersion(this.ServerVersion, "3.17") >= 0)
                    period = TimeSpan.FromSeconds(30);

                _timer = new System.Threading.Timer(
                new System.Threading.TimerCallback(timerCallback),
                null,
                TimeSpan.FromSeconds(30),
                period);
            }
        }

        void EndTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void FingerPrint_ImageReady(object sender, ImageReadyEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.pictureBox_fingerprint.Image = e.Image;
            }));

            OutputHistory($"quality={e.Quality}");
        }

        private void FingerPrint_Speak(object sender, SpeakEventArgs e)
        {
            Speak(e.Text);
            if (string.IsNullOrEmpty(e.DisplayText) == false)
                this.DisplayText(e.DisplayText, "white", "gray");
        }

        static void Beep()
        {
            SystemSounds.Beep.Play();
        }

        bool _sendKeyEnabled = true;

        public bool SendKeyEnabled
        {
            get
            {
                return _sendKeyEnabled;
            }
            set
            {
                _sendKeyEnabled = value;
            }
        }

        private void FingerPrint_Captured(object sender, CapturedEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripStatusLabel_message.Text = e.Score.ToString();
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    Beep();
                    Speak("无法识别");
                    DisplayText($"{e.ErrorInfo}\r\n图象质量: {e.Quality}", "white", "darkred");
                }
                else
                {
                    Speak("很好");

                    DisplayText($"很好\r\n图像质量: {e.Quality}");

                    // TODO: 显示文字中包含 e.Text?

                    if (this.SendKeyEnabled)
                        SendKeys.SendWait(e.Text + "\r");
                }
            }));
        }

        private void FingerPrint_ProgressChanged(object sender,
            DigitalPlatform.LibraryClient.DownloadProgressChangedEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (e.TotalBytesToReceive != -1)
                {
                    toolStripProgressBar1.Minimum = 0;
                    toolStripProgressBar1.Maximum = (int)e.TotalBytesToReceive;
                }
                if (e.BytesReceived != -1)
                    toolStripProgressBar1.Value = (int)e.BytesReceived;
                if (string.IsNullOrEmpty(e.Text) == false)
                {
                    toolStripStatusLabel_message.Text = e.Text;
                }
            }));
        }

        PromptManager _prompt = new PromptManager(2);

        private void FingerPrint_Prompt(object sender, MessagePromptEventArgs e)
        {
            _prompt.Prompt(this, e);

#if NO
            // TODO: 自动延时以后重试
            this.Invoke((Action)(() =>
            {
                if (e.Actions == "yes,cancel")
                {
                    e.ResultAction = "yes";
                    return;

                    DialogResult result = MessageBox.Show(this,
        e.MessageText +
        (e.IncludeOperText == false ? "\r\n\r\n是否跳过本条继续后面操作?\r\n\r\n(确定: 跳过并继续; 取消: 停止全部操作)" : ""),
        "MainForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.OK)
                        e.ResultAction = "yes";
                    else // if (result == DialogResult.Cancel)
                        e.ResultAction = "cancel";
                    return;
                }

                // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
                if (e.Actions == "yes,no,cancel")
                {
                    e.ResultAction = "no";
                    return;

                    DialogResult result = MessageBox.Show(this,
        e.MessageText +
        (e.IncludeOperText == false ? "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 取消: 停止全部操作)" : ""),
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                        e.ResultAction = "yes";
                    else if (result == DialogResult.Cancel)
                        e.ResultAction = "cancel";
                    else
                        e.ResultAction = "no";
                }
            }));
#endif
        }

        void SaveSettings()
        {
            if (this.checkBox_cfg_savePasswordLong.Checked == false)
                this.textBox_cfg_password.Text = "";
            ClientInfo.Config?.Set("global", "ui_state", this.UiState);
            ClientInfo.Config?.Set("global", "replication_start", this.textBox_replicationStart.Text);
            ClientInfo.Finish();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            EndTimer();

            _cancel?.Cancel();
            _cancel?.Dispose();

            AbortReplication(false);
            TryDisposeReplicationCancel();

            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            // UsbNotification.UnregisterUsbDeviceNotification();

            ////

            EndChannel();
            FingerprintServer.EndRemotingServer();

            // FingerPrint.CloseZK();
            FingerPrint.Free();

            FingerPrint.Prompt -= FingerPrint_Prompt;
            FingerPrint.ProgressChanged -= FingerPrint_ProgressChanged;
            FingerPrint.Captured -= FingerPrint_Captured;
            FingerPrint.Speak -= FingerPrint_Speak;
            FingerPrint.ImageReady -= FingerPrint_ImageReady;

            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);

            // Properties.Settings.Default.ui_state = this.UiState;
            // Properties.Settings.Default.repPlan = JsonConvert.SerializeObject(this._repPlan);

            // Properties.Settings.Default.repPlan = this.textBox_replicationStart.Text;
            // Properties.Settings.Default.Save();
        }

        internal void Channel_BeforeLogin(object sender,
    DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                // string strPhoneNumber = "";

                {
                    e.UserName = this.textBox_cfg_userName.Text;

                    // e.Password = this.DecryptPasssword(e.Password);
                    e.Password = this.textBox_cfg_password.Text;

#if NO
                    strPhoneNumber = AppInfo.GetString(
        "default_account",
        "phoneNumber",
        "");
#endif

                    bool bIsReader = false;

                    string strLocation = this.textBox_cfg_location.Text;

                    e.Parameters = "location=" + strLocation;
                    if (bIsReader == true)
                        e.Parameters += ",type=reader";
                }

                // 2014/9/13
                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=fingerprintcenter|" + ClientInfo.ClientVersion;

                // 以手机短信验证方式登录
                //if (string.IsNullOrEmpty(strPhoneNumber) == false)
                //    e.Parameters += ",phoneNumber=" + strPhoneNumber;

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
        public string ServerVersion = "";

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
            string strServerUrl = this.textBox_cfg_dp2LibraryServerUrl.Text;

            string strUserName = this.textBox_cfg_userName.Text;

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


        private void toolStripButton_cfg_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_cfg_dp2LibraryServerUrl.Text != ServerDlg.HnbUrl)
            {
                this.textBox_cfg_dp2LibraryServerUrl.Text = ServerDlg.HnbUrl;

                this.textBox_cfg_userName.Text = "";
                this.textBox_cfg_password.Text = "";
            }
        }

        private void toolStripButton_cfg_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_cfg_dp2LibraryServerUrl.Text != "net.pipe://localhost/dp2library/xe")
            {

                this.textBox_cfg_dp2LibraryServerUrl.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_cfg_userName.Text = "supervisor";
                this.textBox_cfg_password.Text = "";
            }
        }

        SpeechSynthesizer m_speech = new SpeechSynthesizer();
        string m_strSpeakContent = "";

        public void Speak(string strText,
            bool bError = false,
            bool cancel_before = true)
        {
            string color = "gray";
            if (bError)
                color = "darkred";

            DisplayText(strText, "white", color);

            if (this.m_speech == null)
                return;

            if (this.SpeakOn == false)
                return;

            //if (strText == this.m_strSpeakContent)
            //    return; // 正在说同样的句子，不必打断

            this.m_strSpeakContent = strText;

            /*
            this.m_speech.SpeakAsyncCancelAll();
            this.m_speech.SpeakAsync(strText);
            */
            this.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (cancel_before)
                        this.m_speech.SpeakAsyncCancelAll();
                    this.m_speech.SpeakAsync(strText);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    // TODO: 如何报错?
                }
            }));
        }

        public bool SpeakOn
        {
            get
            {
                if (this.InvokeRequired)
                {
                    return (bool)this.Invoke(new Func<bool>(() =>
                    {
                        return this.checkBox_speak.Checked;
                    }));
                }
                else
                    return this.checkBox_speak.Checked;
            }
        }

        void EnableControls(bool bEnable)
        {
            this.Invoke((Action)(() =>
            {
                this.tabControl_main.Enabled = bEnable;

                this.toolButton_stop.Enabled = !bEnable;
            }));
        }

        bool _inInitialCache = false;

        // FingerPrint.ReplicationPlan _repPlan = new FingerPrint.ReplicationPlan();

        // 注意，本函数不在界面线程执行
        // return:
        //      -1  出错
        //      0   没有获得任何数据
        //      >=1 获得了数据
        NormalResult _initFingerprintCache()
        {
            string strError = "";
            string strUrl = (string)this.Invoke((Func<string>)(() =>
            {
                return this.textBox_cfg_dp2LibraryServerUrl.Text;
            }));
            if (string.IsNullOrEmpty(strUrl))
            {
                strError = "尚未配置 dp2library 服务器 URL，无法获得读者指纹信息";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    ErrorCode = "emptyServerUrl"
                };
            }

            // 先把正在运行的同步过程中断
            AbortReplication(true);

            this._inInitialCache = true;
            this.ShowMessage("正在初始化指纹信息 ...");
            EnableControls(false);
            LibraryChannel channel = this.GetChannel();
            try
            {
                // 2019/7/29
                // 检查 dp2library 服务器版本
                long lRet = channel.GetVersion(null,
out string strVersion,
out string strUID,
out strError);
                if (lRet == -1)
                {
                    strError = $"获得 dp2library 服务器版本时出错: {strError}";
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                this.ServerVersion = strVersion;
                this.ServerUID = strUID;

                this.OutputHistory($"所连接的 dp2library 服务器:{channel.Url}, 版本:{this.ServerVersion}, UID:{this.ServerUID}", 0);

                /*
                if (StringUtil.CompareVersion(strVersion, "3.13") < 0)
                {
                    strError = $"指纹识别功能要求 dp2library 在版本 3.13 或以上 (但当前 dp2library 版本是 {strVersion})";
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }
                */

                ReplicationPlan plan = BioUtil.GetReplicationPlan(channel);
                if (plan.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = plan.ErrorInfo,
                        ErrorCode = plan.ErrorCode
                    };
                this.Invoke((Action)(() =>
                {
                    this.textBox_replicationStart.Text = plan.StartDate;
                }));

                string strDir = ClientInfo.FingerPrintCacheDir(strUrl);
                PathUtil.TryCreateDir(strDir);

                // return:
                //      -1  出错
                //      >=0   成功。返回实际初始化的事项
                var result = FingerPrint.InitFingerprintCache(channel,
                    strDir,
                    _cancel.Token);
                if (result.Value == -1)
                    return result;
                return result;
            }
            finally
            {
                this.ReturnChannel(channel);
                EnableControls(true);
                this.ClearMessage();
                this._inInitialCache = false;
            }
        }

        private void MenuItem_testInitCache_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                // return:
                //      -1  出错
                //      0   没有获得任何数据
                //      >=1 获得了数据
                var result = _initFingerprintCache();
                if (result.Value == -1)
                    ShowMessageBox(result.ErrorInfo);
            });
        }

        public void ShowMessageBox(string strText)
        {
            if (this.IsHandleCreated)
                this.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(this, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }

        // 删除本地缓存文件
        private void MenuItem_clearFingerprintCacheFile_Click(object sender, EventArgs e)
        {
            string strDir = ClientInfo.FingerPrintCacheDir(this.textBox_cfg_dp2LibraryServerUrl.Text);  // PathUtil.MergePath(Program.MainForm.DataDir, "fingerprintcache");
            DialogResult result = MessageBox.Show(this,
"确实要删除文件夹 " + strDir + " (包括其中的的全部文件) ? ",
"FingerPrintCenter",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            string strError = "";
            try
            {
                Directory.Delete(strDir, true);
            }
            catch (DirectoryNotFoundException)
            {
                strError = "本次操作前，文件夹 '" + strDir + "' 已经被删除";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "删除文件夹 '" + strDir + "' 时出错: " + ex.Message;
                goto ERROR1;
            }

            this.ShowMessage("本地缓存文件删除成功", "green", true);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            _cancel.Cancel();
        }

        #region ipc channel

        public static bool CallActivate(string strUrl)
        {
            IpcClientChannel channel = new IpcClientChannel();
            IFingerprint obj = null;

            ChannelServices.RegisterChannel(channel, false);
            try
            {
                obj = (IFingerprint)Activator.GetObject(typeof(IFingerprint),
                    strUrl);
                if (obj == null)
                {
                    // strError = "could not locate Fingerprint Server";
                    return false;
                }
                obj.ActivateWindow();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (obj != null)
                {
                    ChannelServices.UnregisterChannel(channel);
                }
            }
        }

        IpcClientChannel m_fingerprintChannel = new IpcClientChannel();
        IFingerprint m_fingerprintObj = null;

        // 通道打开成功后，窗口应该显示成一种特定的状态
        int StartChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(m_fingerprintChannel, false);

            try
            {
                m_fingerprintObj = (IFingerprint)Activator.GetObject(typeof(IFingerprint),
                    strUrl);
                if (m_fingerprintObj == null)
                {
                    strError = "could not locate Fingerprint Server";
                    return -1;
                }
            }
            finally
            {
            }

            //this.ToolStripMenuItem_start.Enabled = false;
            return 0;
        }

        void EndChannel()
        {
            // TODO: 这里有点乱。应该是通过 m_fingerprintChannel 是否为空来判断
            if (this.m_fingerprintObj != null)
            {
                ChannelServices.UnregisterChannel(m_fingerprintChannel);
                this.m_fingerprintObj = null;
                //this.ToolStripMenuItem_start.Enabled = true;
            }
        }

        #endregion

        delegate void _ActivateWindow(bool bActive);

        public void ActivateWindow(bool bActive)
        {
            if (this.InvokeRequired)
            {
                _ActivateWindow d = new _ActivateWindow(ActivateWindow);
                this.Invoke(d, new object[] { bActive });
            }
            else
            {
                if (bActive == true)
                {
                    if (this.WindowState == FormWindowState.Minimized)
                        this.WindowState = FormWindowState.Normal;

                    // 如果 this.TopMost 不奏效，可以试试下面这个 URL 里面的方法
                    // https://stackoverflow.com/questions/5282588/how-can-i-bring-my-application-window-to-the-front
                    /*
                    {
                        this.WindowState = FormWindowState.Minimized;
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                    }
                    */


                    this.TopMost = true;
                    // SetForegroundWindow(this.Handle);    // 接受键盘输入
                }
                else
                {
                    this.TopMost = false;
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }

        delegate void _DisplayCancelButton(bool bActive);

        public void DisplayCancelButton(bool bVisible)
        {
            if (this.InvokeRequired)
            {
                _DisplayCancelButton d = new _DisplayCancelButton(DisplayCancelButton);
                this.Invoke(d, new object[] { bVisible });
            }
            else
            {
                this.button_cancel.Visible = bVisible;
                if (bVisible)
                    this.tabControl_main.SelectedTab = this.tabPage_start;
            }
        }

        // 取消获得指纹信息
        private void button_cancel_Click(object sender, EventArgs e)
        {
            FingerPrint.CancelRegisterString();
        }

        #region 浏览器控件

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(ClientInfo.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";
            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);
            // Global.ScrollToEnd(this.WebBrowser);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
            REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            // StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", PathUtil.MergePath(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            AppendHtml(strHtml);
        }

        public void OutputHistory(string strText, int nWarningLevel = 0)
        {
            OutputText(DateTime.Now.ToLongTimeString() + " " + strText, nWarningLevel);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        #endregion

        void DisplayText(string text,
            string textColor = "white",
            string backColor = "gray")
        {
#if NO
            // AppendHtml("<div>" +HttpUtility.HtmlEncode(text)+ "</div>");
            string html = string.Format("<html><body><div style='font-size:30px;'>{0}</div></body></html>", HttpUtility.HtmlEncode(text));
            SetHtmlString(this.webBrowser1, html);
#endif
            this.BeginInvoke((Action)(() =>
            {
                this.label_message.Text = text;
                this.label_message.BackColor = Color.FromName(backColor);
                this.label_message.ForeColor = Color.FromName(textColor);
            }));
        }

        private void ToolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 警告关闭
                DialogResult result = MessageBox.Show(this,
                    "确实要退出 dp2-指纹中心?\r\n\r\n(本接口程序提供了指纹扫描、登记的功能，一旦退出，这些功能都将无法运行。平时应保持运行状态，将窗口最小化即可)",
                    "dp2-指纹中心",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // AbortAllChannel();

            SaveSettings();
        }

        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                if (m_fingerprintObj != null)
                {
                    m_fingerprintObj.CancelGetFingerprintString();
                }
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

#if NO
        #region device changed

        const int WM_DEVICECHANGE = 0x0219; //see msdn site
        const int DBT_DEVNODES_CHANGED = 0x0007;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVALCOMPLETE = 0x8004;
        const int DBT_DEVTYPVOLUME = 0x00000002;

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == DBT_DEVNODES_CHANGED)
                {
                    BeginStart();
                }

                /*
                    if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL)
                    {
                        MessageBox.Show(this, "in");
                    }
                    if (m.WParam.ToInt32() == DBT_DEVICEREMOVALCOMPLETE)
                    {
                        MessageBox.Show(this, "usb out");
                    }
                 * */
            }
            base.WndProc(ref m);
        }

        #endregion
#endif

        public void ActivateWindow()
        {
            this.Invoke((Action)(() =>
            {
                this.Speak("恢复窗口显示");
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                // 把窗口翻到前面
                //this.Activate();
                API.SetForegroundWindow(this.Handle);
            }));
        }

        public const int WM_SHOW1 = API.WM_USER + 200;

        protected override void WndProc(ref Message m)
        {
#if REMOVED
            if (m.Msg == Program.WM_MY_MSG)
            {
                this.Speak("收到消息");

                if ((m.WParam.ToInt32() == 0xCDCD) && (m.LParam.ToInt32() == 0xEFEF))
                {
                    if (WindowState == FormWindowState.Minimized)
                    {
                        WindowState = FormWindowState.Normal;
                    }
                    // Bring window to front.
                    bool temp = TopMost;
                    TopMost = true;
                    TopMost = temp;
                    // Set focus to the window.
                    Activate();
                }
            }
#endif

#if NO
            if (m.Msg == WM_SHOW1)
            {
                this.ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
                this.Speak("收到消息");
                return;
            }
#endif

#if NO
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                        //MessageBox.Show(this, "removed"); 
                        BeginRefreshReaders(new CancellationToken());
                        break;
                    case UsbNotification.DbtDevicearrival:
                        //MessageBox.Show(this, "added");
                        BeginRefreshReaders(new CancellationToken());
                        break;
                }
            }
#endif
            base.WndProc(ref m);
        }

        public void Restart()
        {
            BeginRefreshReaders("connected", new CancellationToken());
        }

        int _refreshCount = 0;
        const int _delaySeconds = 5;
        Task _refreshTask = null;

        void BeginRefreshReaders(string action,
            CancellationToken token)
        {
            if (_refreshTask != null)
            {
                if (action == "disconnected")
                {
                    if (_refreshCount < 1)
                    {
                        _refreshCount++;
                        this.OutputHistory($"disconnected ++ _refreshCount={_refreshCount}", 0);
                    }
                    else
                        this.OutputHistory($"disconnected passed _refreshCount={_refreshCount}", 0);
                }
                else
                {
                    _refreshCount++;
                    this.OutputHistory($"{action} ++ _refreshCount={_refreshCount}", 0);
                }
                return;
            }

            // _refreshCount = 2;
            this.OutputHistory($"new task _refreshCount={_refreshCount}", 0);
            _refreshTask = Task.Run(() =>
            {
                int delta = 1;  // 第一次额外增加的秒数
                while (_refreshCount-- >= 0)
                {
                    this.OutputHistory($"delay begin", 0);
                    Task.Delay(TimeSpan.FromSeconds(_delaySeconds + delta)).Wait(token);
                    this.OutputHistory($"delay end", 0);

                    if (token.IsCancellationRequested)
                        break;
                    // 迫使重新启动
                    _initialized = false;
                    this.OutputHistory($"initial begin", 0);
                    BeginStart().Wait(token);
                    this.OutputHistory($"initial end", 0);
                    if (token.IsCancellationRequested)
                        break;

                    // 如果初始化没有成功，则要追加初始化
                    if (ClientInfo.ErrorState == "normal")
                        break;

                    delta = 0;
                }
                _refreshTask = null;
                _refreshCount = 0;
                this.OutputHistory($"task = null", 0);
            });
        }

#if REMOVED
        int _refreshCount = 2;
        System.Threading.Timer _refreshTimer = null;
        private static readonly Object _syncRoot_refresh = new Object();
        const int _delaySeconds = 3;

        // (_delaySeconds) 秒内多次到来的请求，会被合并为一次执行
        void BeginRefreshReaders()
        {
            // Speak("重新初始化指纹设备", false, false);
            lock (_syncRoot_refresh)
            {
                _refreshCount++;
                if (_refreshTimer == null)
                {
                    _refreshCount = 2;
                    _refreshTimer = new System.Threading.Timer(
            new System.Threading.TimerCallback(refreshTimerCallback),
            null,
            TimeSpan.FromSeconds(_delaySeconds), TimeSpan.FromSeconds(_delaySeconds));
                }
            }
        }

        int _inRefresh = 0;

        void refreshTimerCallback(object o)
        {
            int v = Interlocked.Increment(ref this._inRefresh);
            try
            {
                // 防止重入
                if (v > 1)
                    return;

                // 迫使重新启动
                _initialized = false;
                BeginStart().Wait();

                // 如果初始化没有成功，则要追加初始化
                _refreshCount--;
                if (this.ErrorState != "normal" && _refreshCount > 0)
                    return;

                // 取消 Timer
                lock (_syncRoot_refresh)
                {
                    if (_refreshTimer != null)
                    {
                        _refreshTimer.Dispose();
                        _refreshTimer = null;
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._inRefresh);
            }
        }

#endif

        private void ToolStripMenuItem_start_Click(object sender, EventArgs e)
        {
            var task = BeginStart();
        }

        private void ToolStripMenuItem_reopen_Click(object sender, EventArgs e)
        {
            // 警告关闭
            DialogResult result = MessageBox.Show(this,
                "确实要重新启动 dp2-指纹中心?\r\n\r\n(会重新初始化指纹缓存)",
                "dp2-指纹中心",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            _initialized = false;
            var task = BeginStart();
        }

        // 当面板上服务器 URL、用户名、密码发生变动以后，清除以前的 ChannelPool。迫使前端重新登录
        // 还要自动清除同步点
        private void textBox_cfg_userName_TextChanged(object sender, EventArgs e)
        {
            _channelPool.Clear();
            this.textBox_replicationStart.Text = "";
            // 2019/5/14
            _initialized = false;
        }

        private void MenuItem_lightWhite_Click(object sender, EventArgs e)
        {
            FingerPrint.Light("white");
        }

        private void MenuItem_lightRed_Click(object sender, EventArgs e)
        {
            FingerPrint.Light("red");
        }

        private void MenuItem_lightGreen_Click(object sender, EventArgs e)
        {
            FingerPrint.Light("green");
        }

        CancellationTokenSource _cancelReplication = null;

        void AbortReplication(bool waitFinish)
        {
            if (_cancelReplication != null)
            {
                _cancelReplication?.Cancel();
                if (waitFinish)
                {
                    // 等待信号，或者 CancellationToken 中断
                    WaitHandle.WaitAny(new WaitHandle[] { _eventReplicationFinish, _cancel.Token.WaitHandle });
                }
            }
        }

        private void MenuItem_replication_Click(object sender, EventArgs e)
        {
            BeginReplication();
        }

        void BeginReplication()
        {
            // 如果当前正在进行缓存初始化，则放弃进行同步
            if (_inInitialCache)
                return;

            if (_initialized == false)
            {
                // 先检查首次初始化指纹缓存是否完成。没有完成(因为曾出错)的情况，要先尝试首次初始化指纹缓存
                // TODO: 这里需要优化的是，应该是仅仅是 dp2library/dp2libraryxe 未响应的出错以后才需要重试 BeginStart()。其他情况如果频繁重试，可能并不合适
                var task = BeginStart();
                return;
            }

            Task.Run(() =>
            {
                // 先把正在运行的中断
                AbortReplication(true);

                // 无论是 _cancel 还是 _cancelReplication 触发 Cancel，都能停止复制过程
                TryDisposeReplicationCancel();
                _cancelReplication = CancellationTokenSource.CreateLinkedTokenSource(_cancel.Token);
                DpReplication(_cancelReplication.Token);
            });
        }

        void ShowReplicationMessage(string text)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripStatusLabel_replication.Text = text;
            }));
        }

        AutoResetEvent _eventReplicationFinish = new AutoResetEvent(false);

        // TODO: 当大批获得指纹信息时候，本函数要禁止调用。反之本函数运行过程中，大批获得指纹信息要中断本函数?
        void DpReplication(CancellationToken token)
        {
            string strError = "";
            string strUrl = (string)this.Invoke((Func<string>)(() =>
            {
                return this.textBox_cfg_dp2LibraryServerUrl.Text;
            }));
            if (string.IsNullOrEmpty(strUrl))
            {
                strError = "尚未配置 dp2library 服务器 URL，无法获得读者指纹信息";
                ShowMessage(strError);
            }

            string strStartDate = (string)this.Invoke((Func<string>)(() =>
            {
                return this.textBox_replicationStart.Text;
            }));

            bool done = false;
            _eventReplicationFinish.Reset();

            this.OutputHistory($"增量同步指纹信息 {strStartDate}");
            ShowReplicationMessage($"正在同步最新指纹信息 {strStartDate} ...");
            //this.ShowMessage($"正在同步最新指纹信息 {strStartDate} ...");
            //EnableControls(false);
            LibraryChannel channel = this.GetChannel();
            //TimeSpan old_timeout = channel.Timeout;
            //channel.Timeout = TimeSpan.FromSeconds(120);
            try
            {
                string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

                ReplicationResult result = FingerPrint.DoReplication(
channel,
strStartDate,
strEndDate,
LogType.OperLog,
this.ServerVersion,
token);
                if (result.Value == -1)
                {
                    strError = $"增量同步指纹信息 {strStartDate} 失败: {result.ErrorInfo}";
                    this.OutputHistory(strError, 2);
                    //this.ShowMessage(strError, "red", true);
                    this.Speak(strError);
                    return;
                }

                done = true;
                // result.Value == 0 表示本次没有获得任何新信息,即服务器的日志没有发生增长

                if (result.Value == 1)
                    this.Invoke((Action)(() =>
                    {
                        this.textBox_replicationStart.Text = result.LastDate + ":" + result.LastIndex + "-";    // 注意 - 符号不能少。少了意思就会变成每次只获取一条日志记录了
                    }));
            }
            finally
            {
                //channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                //EnableControls(true);
                //if (done == true)
                //    this.ClearMessage();
                ShowReplicationMessage("");

                _eventReplicationFinish.Set();
                // _cancelReplication = null;
                TryDisposeReplicationCancel();
            }
        }

        void TryDisposeReplicationCancel()
        {
            if (_cancelReplication != null)
            {
                _cancelReplication.Dispose();
                _cancelReplication = null;
            }
        }

        System.Threading.Timer _timer = null;

        void timerCallback(object o)
        {
            // 避免重叠启动
            if (_cancelReplication != null
                || _inInitialCache == true)
                return;

            // 2019/6/19
            ClientInfo.SaveConfig();

            BeginReplication();
        }

        // 刷新指纹信息。
        // 指立即从服务器获取最新日志，同步指纹变动信息
        private void MenuItem_refresh_Click(object sender, EventArgs e)
        {
#if NO
            string strStartDate = (string)this.Invoke((Func<string>)(() =>
            {
                return this.textBox_replicationStart.Text;
            }));

            // 如果没有明确的日期，那就从头开始初始化指纹缓存
            if (string.IsNullOrEmpty(strStartDate))
            {
                var result = TryInitFingerprintCache();
                if (result.Value == -1)
                    MessageBox.Show(this, result.ErrorInfo);
            }
            else
                BeginReplication();
#endif
            BeginReplication();
        }

        // 获得当前用户的用户名
        public string GetCurrentUserName()
        {
#if NO
            if (this.Channel != null && string.IsNullOrEmpty(this.Channel.UserName) == false)
                return this.Channel.UserName;
#endif
            if (string.IsNullOrEmpty(this._currentUserName) == false)
                return this._currentUserName;

            // TODO: 或者迫使登录一次
            return "";
        }

        private void MenuItem_throwException_Click(object sender, EventArgs e)
        {
            throw new Exception("test");
        }

        void FirstSetup()
        {
            this._floatingMessage.Clicked += _floatingMessage_Clicked;
            try
            {
                this.Invoke((Action)(() =>
                {
                    this.tabControl_main.SelectedTab = this.tabPage_cfg;
                }));
                this.BeginClicked();
                this.ShowMessage("欢迎使用指纹中心！\r\n\r\n(请用鼠标点此文字继续)", "green", true);
                this.WaitClicked();
                this.ShowMessage("请配置参数。\r\n配置完成后，请用菜单命令“文件/启动”来开始首次运行。\r\n\r\n(请用鼠标点此文字继续)", "green", true);
                this.WaitClicked();
            }
            finally
            {
                this._floatingMessage.Clicked -= _floatingMessage_Clicked;
            }
        }

        AutoResetEvent _floatClicked = new AutoResetEvent(false);

        private void _floatingMessage_Clicked(object sender, EventArgs e)
        {
            _floatClicked.Set();
        }

        void BeginClicked()
        {
            _floatClicked.Reset();
        }

        void WaitClicked()
        {
            WaitHandle.WaitAny(new WaitHandle[] { _floatClicked, _cancel.Token.WaitHandle });
        }

        // 从 http 服务器下载一个文件
        async Task<NormalResult> DownloadFile(string strUrl,
    string strLocalFileName)
        {
            using (MyWebClient webClient = new MyWebClient())
            {
                webClient.ReadWriteTimeout = 30 * 1000; // 30 秒，在读写之前 - 2015/12/3
                webClient.Timeout = 30 * 60 * 1000; // 30 分钟，整个下载过程 - 2015/12/3
                webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                string strTempFileName = strLocalFileName + ".temp";
                // TODO: 先下载到临时文件，然后复制到目标文件
                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(strUrl, UriKind.Absolute), strTempFileName).ConfigureAwait(false);

                    this.ClearMessage();
                    File.Delete(strLocalFileName);
                    File.Move(strTempFileName, strLocalFileName);
                    return new NormalResult();
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (ex.Response is HttpWebResponse response)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                return new NormalResult { Value = -1, ErrorInfo = ex.Message };
                            }
                        }
                    }

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ExceptionUtil.GetDebugText(ex)
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ExceptionUtil.GetDebugText(ex)
                    };
                }
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            this.ShowMessage($"正在下载文件 {e.ProgressPercentage}% ...");
        }

        void InstallDriver(string strMessage)
        {
            DialogResult result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
            {
                return MessageBox.Show(this,
                    strMessage + "\r\n\r\n是否立即下载安装'中控'指纹仪厂家驱动?",
                    "dp2-指纹中心",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
            }));
            if (result == DialogResult.Cancel)
                return;

            this.DisplayText("安装'中控'指纹仪厂家驱动 ...");

            // 从 http 服务器下载一个文件
            string temp_filename = Path.Combine(ClientInfo.UserTempDir, "setup.zip");
            NormalResult download_result = DownloadFile("http://dp2003.com/fingerprintcenter/setup_driver.zip",
                temp_filename).Result;
            if (download_result.Value == -1)
            {
                this.ShowMessage(download_result.ErrorInfo);
                return;
            }

            string exe_filename = Path.Combine(ClientInfo.UserTempDir, "setup.exe");
            using (var zip = ZipFile.OpenRead(temp_filename))
            {
                foreach (var entry in zip.Entries)
                {
                    entry.ExtractToFile(exe_filename, true);
                    break;
                }
            }

            Process.Start(exe_filename);
        }

        void UpdateDeviceList()
        {
            this.Invoke((Action)(() =>
            {
                this.comboBox_deviceList.Items.Clear();
                foreach (string s in FingerPrint.DeviceList)
                {
                    this.comboBox_deviceList.Items.Add(s);
                }
            }));
        }

        int CurrentDeviceIndex
        {
            get
            {

                string index = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.comboBox_deviceList.Text;
                }));

                if (string.IsNullOrEmpty(index))
                    return 0;
                if (Int32.TryParse(index, out int v) == false)
                {
                    // return -1;
                    throw new ArgumentException($"指纹设备序号 {index} 不合法。应为纯数字");
                }
                return v;
            }
        }

        private void MenuItem_setupDriver_Click(object sender, EventArgs e)
        {
            Task.Run(() => { InstallDriver("本功能将重新安装'中控'指纹仪厂家驱动。"); });
        }

        private void button_setDefaultThreshold_Click(object sender, EventArgs e)
        {
            this.textBox_cfg_shreshold.Text = FingerPrint.DefaultThreshold.ToString();
        }

        private void textBox_cfg_shreshold_TextChanged(object sender, EventArgs e)
        {
            try
            {
                FingerPrint.Shreshold = Convert.ToInt32(this.textBox_cfg_shreshold.Text);
            }
            catch
            {
                FingerPrint.Shreshold = FingerPrint.DefaultThreshold;
            }
        }

        private void MenuItem_about_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/DigitalPlatform/dp2/tree/master/FingerprintCenter");
        }

        private void MenuItem_manual_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/DigitalPlatform/dp2/issues/222");
        }

        // 重新设置序列号
        private void MenuItem_resetSerialCode_Click(object sender, EventArgs e)
        {
            // return:
            //      -1  出错
            //      0   正确
            int nRet = ClientInfo.VerifySerialCode(
                "", // strTitle,
                "", // strRequirFuncList,
                "reset",
                out string strError);
            if (nRet == -1)
                goto ERROR1;
            return;
            ERROR1:
            MessageBox.Show(this, strError);

#if NO
            string strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strRequirFuncList = "";  // 因为这里是设置通用的序列号，不具体针对哪个功能，所以对设置后，序列号的功能不做检查。只有等到用到具体功能的时候，才能发现序列号是否包含具体功能的 function = ... 参数

            string strSerialCode = "";
            REDO_VERIFY:

            if (strSerialCode == "community")
            {
                ClientInfo.CommunityMode = true;
                ClientInfo.Config.Set("main_form", "last_mode", "community");
                return;
            }
            else
            {
                ClientInfo.CommunityMode = false;
                ClientInfo.Config.Set("main_form", "last_mode", "standard");
            }

            if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false ||
                // strSha1 != GetCheckCode(strSerialCode) 
                MatchLocalString(strSerialCode) == false
                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "序列号无效。请重新输入");
                else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                    MessageBox.Show(this, "序列号中 function 参数无效。请重新输入");


                // 出现设置序列号对话框
                nRet = ResetSerialCode(
                    "重新设置序列号",
                    true,
                    strSerialCode,
                    ClientInfo.GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "放弃";
                    goto ERROR1;
                }
                strSerialCode = ClientInfo.Config.Get("sn", "sn", "");
                if (string.IsNullOrEmpty(strSerialCode) == true)
                {
                    Application.Exit();
                    return;
                }

                ClientInfo.Config.Save();
                goto REDO_VERIFY;
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_closeSendKey_Click(object sender, EventArgs e)
        {
            // m_fingerprintObj.EnableSendKey(false);
            FingerprintServer._enableSendKey(false);
        }

        private void MenuItem_openSendKey_Click(object sender, EventArgs e)
        {
            // m_fingerprintObj.EnableSendKey(true);
            FingerprintServer._enableSendKey(true);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            // notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "指纹中心已经隐藏";
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void ToolStripMenuItem_deleteShortcut_Click(object sender, EventArgs e)
        {
            ClientInfo.RemoveShortcutFromStartupGroup("dp2-指纹中心", true);
        }

        private void ToolStripMenuItem_showUsbInfo_Click(object sender, EventArgs e)
        {
            var infos = UsbInfo.GetUSBDevices();
            MessageDlg.Show(this, UsbInfo.ToString(infos), "USB device info");
        }

        private void ToolStripMenuItem_startWatchUsbChange_Click(object sender, EventArgs e)
        {
            UsbInfo.StartWatch((add_count, remove_count) =>
                {
                    this.OutputHistory($"add_count:{add_count}, remove_count:{remove_count}", 1);
                },
                new CancellationToken());
        }
    }

    class MyWebClient : WebClient
    {
        public int Timeout = -1;
        public int ReadWriteTimeout = -1;

        HttpWebRequest _request = null;

        protected override WebRequest GetWebRequest(Uri address)
        {
            _request = (HttpWebRequest)base.GetWebRequest(address);

#if NO
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
#endif
            if (this.Timeout != -1)
                _request.Timeout = this.Timeout;
            if (this.ReadWriteTimeout != -1)
                _request.ReadWriteTimeout = this.ReadWriteTimeout;
            return _request;
        }

        public void Cancel()
        {
            if (this._request != null)
                this._request.Abort();
        }
    }

}
