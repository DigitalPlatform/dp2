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

using FingerprintCenter.Properties;
using libzkfpcsharp;

using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.IO;
using DigitalPlatform.Interfaces;

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
            ClientInfo.MainForm = this;

            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
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
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_cfg_dp2LibraryServerUrl);
                controls.Add(this.textBox_cfg_userName);
                controls.Add(this.textBox_cfg_password);
                controls.Add(this.textBox_cfg_location);
                controls.Add(new ControlWrapper(this.checkBox_speak, true));
                controls.Add(new ControlWrapper(this.checkBox_beep, true));
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_cfg_dp2LibraryServerUrl);
                controls.Add(this.textBox_cfg_userName);
                controls.Add(this.textBox_cfg_password);
                controls.Add(this.textBox_cfg_location);
                controls.Add(new ControlWrapper(this.checkBox_speak, true));
                controls.Add(new ControlWrapper(this.checkBox_beep, true));
                GuiState.SetUiState(controls, value);
            }
        }

        public void ShowMessage(string text)
        {
            this._floatingMessage.Text = text;
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.UiState = Properties.Settings.Default.ui_state;

            ClearHtml();

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            ClientInfo.InitialDirs("fingerprintcenter");

            FingerPrint.Prompt += FingerPrint_Prompt;
            FingerPrint.ProgressChanged += FingerPrint_ProgressChanged;
            FingerPrint.Captured += FingerPrint_Captured;
            FingerPrint.Speak += FingerPrint_Speak;
            FingerPrint.ImageReady += FingerPrint_ImageReady;

            FingerPrint.Init();
            FingerPrint.OpenZK();

            FingerPrint.StartCapture(_cancel.Token);

            StartRemotingServer();
            Speak("指纹中心启动成功");
        }

        private void FingerPrint_ImageReady(object sender, ImageReadyEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.pictureBox_fingerprint.Image = e.Image;
            }));
        }

        private void FingerPrint_Speak(object sender, SpeakEventArgs e)
        {
            Speak(e.Text);
        }

        private void FingerPrint_Captured(object sender, CapturedEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripStatusLabel1.Text = e.Score.ToString();
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                    Console.Beep();
                else
                {
                    Speak("很好");
                    SendKeys.SendWait(e.Text + "\r");
                }
            }));
        }

        private void FingerPrint_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
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
                    toolStripStatusLabel1.Text = e.Text;
                }
            }));
        }

        private void FingerPrint_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = MessageBox.Show(this,
    e.MessageText + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 取消: 停止全部操作)",
    "ReportForm",
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
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel.Cancel();

            EndChannel();
            EndRemotingServer();

            FingerPrint.CloseZK();
            FingerPrint.Free();

            FingerPrint.Prompt -= FingerPrint_Prompt;
            FingerPrint.ProgressChanged -= FingerPrint_ProgressChanged;
            FingerPrint.Captured -= FingerPrint_Captured;
            FingerPrint.Speak -= FingerPrint_Speak;
            FingerPrint.ImageReady -= FingerPrint_ImageReady;

            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);

            Properties.Settings.Default.ui_state = this.UiState;
            Properties.Settings.Default.Save();
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
            }

            e.ErrorInfo = "尚未配置服务器参数";
            e.Cancel = true;
        }

        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
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

        void Speak(string strText)
        {
            DisplayText(strText);

            if (this.m_speech == null)
                return;

            if (this.SpeakOn == false)
                return;

            //if (strText == this.m_strSpeakContent)
            //    return; // 正在说同样的句子，不必打断

            this.m_strSpeakContent = strText;
            this.m_speech.SpeakAsyncCancelAll();
            this.m_speech.SpeakAsync(strText);
        }

        public bool SpeakOn
        {
            get
            {
                return (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_speak.Checked;
                }));
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

        // 注意，本函数不在界面线程执行
        int InitFingerprintCache(
            out string strError)
        {
            this.ShowMessage("正在初始化指纹信息 ...");
            EnableControls(false);
            LibraryChannel channel = this.GetChannel();
            try
            {
                string strDir = "";
                this.Invoke((Action)(() =>
                {
                    strDir = this.textBox_cfg_dp2LibraryServerUrl.Text;
                }));
                strDir = ClientInfo.FingerPrintCacheDir(strDir);
                PathUtil.TryCreateDir(strDir);

                return FingerPrint.InitFingerprintCache(channel,
                    strDir,
                    _cancel.Token,
                    out strError);
            }
            finally
            {
                this.ReturnChannel(channel);
                EnableControls(true);
                this.ClearMessage();
            }
        }

        private void MenuItem_test_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                int nRet = InitFingerprintCache(
        out string strError);
                if (nRet == -1)
                    ShowMessageBox(strError);
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

        // 清除指纹缓存
        private void MenuItem_clearFingerprintCache_Click(object sender, EventArgs e)
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

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            _cancel.Cancel();
        }

        #region remoting server

#if HTTP_CHANNEL
        HttpChannel m_serverChannel = null;
#else
        IpcServerChannel m_serverChannel = null;
#endif

        void StartRemotingServer()
        {
            // EndRemoteChannel();

            //Instantiate our server channel.
#if HTTP_CHANNEL 
            m_serverChannel = new HttpChannel();
#else
            // TODO: 重复启动 .exe 这里会抛出异常，要进行警告处理
            m_serverChannel = new IpcServerChannel(
                "FingerprintChannel");
#endif

            //Register the server channel.
            ChannelServices.RegisterChannel(m_serverChannel, false);

            RemotingConfiguration.ApplicationName = "FingerprintServer";

            /*
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerFactory),
                "ServerFactory",
                WellKnownObjectMode.Singleton);
             * */


            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(FingerprintServer),
                "FingerprintServer",
                WellKnownObjectMode.Singleton);
        }

        void EndRemotingServer()
        {
            if (m_serverChannel != null)
            {
                ChannelServices.UnregisterChannel(m_serverChannel);
                m_serverChannel = null;
            }
        }


        #endregion

        #region ipc channel

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


        #endregion

        void DisplayText(string text)
        {
            // AppendHtml("<div>" +HttpUtility.HtmlEncode(text)+ "</div>");
            string html = string.Format("<html><body><div>{0}</div></body></html>", HttpUtility.HtmlEncode(text));
            SetHtmlString(this.webBrowser1, html);
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
        }
    }
}
