#define TEST

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using System.Deployment;
using System.Deployment.Application;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Http;

using DigitalPlatform;
using DigitalPlatform.Interfaces;
using DigitalPlatform.IO;

namespace ZkFingerprint
{
    public partial class MainForm : Form
    {
        bool m_bBeep = false;
        bool m_bSpeak = false;
        bool m_bDisplayFingerprintImage = true;

        bool m_bGameState = false;

        // 指纹识别系统比对识别阈值
        // 1-100 默认 10
        int m_nThreshold = 10;

        ManualResetEvent pause_event = new ManualResetEvent(true);

        // Thread threadWaitMessage = null;

        string m_strError = "";
        bool m_bStop = true;

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr handle);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (DetectVirus.Detect360() || DetectVirus.DetectGuanjia())
            {
                MessageBox.Show(this, "zkprintcenter 被木马软件干扰，无法启动");
                Application.Exit();
                return;
            }

            AddShortcutToStartupGroup("dp2-中控指纹阅读器接口");

            StartRemotingServer();

            this.checkBox_beep.Checked = Properties.Settings.Default.Beep;
            this.checkBox_speak.Checked = Properties.Settings.Default.Speak;
            this.m_bDisplayFingerprintImage = Properties.Settings.Default.DisplayFingerprintImage;
            this.m_nThreshold = Properties.Settings.Default.Threshold;
            /*
            this.checkBox_autoStart.Checked = Properties.Settings.Default.AutoStart;
            if (this.checkBox_autoStart.Checked == true)
                this.button_begin_Click(null, null);
            */
            OpenServer();
        }

        private void OpenServer(bool bDisplayErrorMessage = true)
        {
            string strError = "";

            WriteHtml(this.webBrowser1,
"<html><head></head><body>");

            int nRet = this.StartChannel("ipc://FingerprintChannel/FingerprintServer",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = this.m_fingerprintObj.Open(out strError);
            if (nRet == -1)
                goto ERROR1;
            this.m_fingerprintObj.SetParameter("Threshold", this.m_nThreshold);
            this.label_message.Text = "";
            this.TopMost = false;
            return;
            ERROR1:
            /*
            MessageBox.Show(this, strError,
                "dp2-中控指纹阅读器接口", MessageBoxButtons.OK, MessageBoxIcon.Error);
             * */
            if (bDisplayErrorMessage == true)
                AutoCloseMessageBox.Show(this, strError, 10000, "dp2-中控指纹阅读器接口");
            this.EndChannel();
            this.label_message.Text = strError;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (e.CloseReason == CloseReason.UserClosing)
            {
                // 警告关闭
                DialogResult result = MessageBox.Show(this,
                    "确实要退出 dp2-中控指纹阅读器接口?\r\n\r\n(本接口程序提供了指纹扫描、登记的功能，一旦退出，这些功能都将无法运行。平时应保持运行状态，将窗口最小化即可)",
                    "dp2-中控指纹阅读器接口",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (this.m_bStop == false)
            {
                if (this.m_fingerprintObj != null)
                    this.m_fingerprintObj.Close();

                this.m_bStop = true;
                this.pause_event.Set();
                e.Cancel = true;
            }

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Beep = this.checkBox_beep.Checked;
            Properties.Settings.Default.Speak = this.checkBox_speak.Checked;
            Properties.Settings.Default.DisplayFingerprintImage = this.m_bDisplayFingerprintImage;
            Properties.Settings.Default.Threshold = this.m_nThreshold;

            Properties.Settings.Default.Save();

            EndChannel();
            EndRemotingServer();
        }

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

#if NO
        private void button_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.m_bStop = false;
            this.pause_event.Set();

            this.m_bBeep = this.checkBox_beep.Checked;

            WriteHtml(this.webBrowser1,
"<html><head></head><body>");

            int nRet = this.StartChannel("ipc://FingerprintChannel/FingerprintServer",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = this.m_fingerprintObj.Open(out strError);
            if (nRet == -1)
                goto ERROR1;
            /*
            threadWaitMessage =
                    new Thread(new ThreadStart(this.ThreadMain));

            threadWaitMessage.Start();

            EnableControls(false);
             * */
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }
#endif

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


        delegate void _Beep(FingerprintServer server, int nCount);

        public void Beep(FingerprintServer server, int nCount)
        {
            if (this.InvokeRequired)
            {
                _Beep d = new _Beep(Beep);
                this.BeginInvoke(d, new object[] { server, nCount });
            }
            else
            {
                if (m_bBeep == true)
                    server.Beep(nCount);
            }
        }

        public bool BeepOn
        {
            get
            {
                return this.m_bBeep;
            }
        }

        public bool SpeakOn
        {
            get
            {
                return this.m_bSpeak;
            }
        }

        public bool DisplayFingerprintImage
        {
            get
            {
                return this.m_bDisplayFingerprintImage;
            }
        }

        delegate void _Light(FingerprintServer server, string strColor);

        public void Light(FingerprintServer server, string strColor)
        {
            if (this.InvokeRequired)
            {
                _Light d = new _Light(Light);
                this.BeginInvoke(d, new object[] { server, strColor });
            }
            else
            {
                server.Light(strColor);
            }
        }

        delegate void _DisplayFeatureInfo(FingerprintServer server, int nQuality);

        public void DisplayFeatureInfo(FingerprintServer server, int nQuality)
        {
            if (this.InvokeRequired)
            {
                _DisplayFeatureInfo d = new _DisplayFeatureInfo(DisplayFeatureInfo);
                this.Invoke(d, new object[] { server, nQuality });
            }
            else
            {
                string strText = server.GetFeatureInfo(nQuality, this.m_bGameState);

                this.label_message.Text = strText;
                if (this.m_bGameState == true && this.m_bSpeak == false && this.m_bBeep == true)
                {
                    _Beep d = new _Beep(Beep);
                    this.BeginInvoke(d, new object[] { server, 1 });
                }
            }
        }

        delegate void _DisplayInfo(string strText);

        public void DisplayInfo(string strText)
        {
            if (this.InvokeRequired)
            {
                _DisplayInfo d = new _DisplayInfo(DisplayInfo);
                this.Invoke(d, new object[] { strText });
            }
            else
            {
                this.label_message.Text = strText;
            }
        }

        delegate int _Identity(FingerprintServer server);

        public int Identity(FingerprintServer server)
        {
            if (this.InvokeRequired)
            {
                _Identity d = new _Identity(Identity);
                return (int)this.Invoke(d, new object[] { server });
            }
            else
            {
                return server.Match();
            }
        }


        [DllImport("user32")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        delegate IntPtr _GetImagePanelInfo(out int nWidth, out int nHeight);
        public IntPtr GetImagePanelInfo(out int nWidth, out int nHeight)
        {
            if (this.InvokeRequired)
            {
                _GetImagePanelInfo d = new _GetImagePanelInfo(GetImagePanelInfo);
                object[] parameters = new object[] { null, null };
                IntPtr ret = (IntPtr)this.Invoke(d, parameters);
                nWidth = (int)parameters[0];
                nHeight = (int)parameters[1];
                return ret;
            }
            else
            {
                nWidth = this.panel_image.Width;
                nHeight = this.panel_image.Height;
                return GetDC(this.panel_image.Handle);
            }
        }
        delegate void _ReleaseImagePanelInfo(IntPtr hDC);
        public void ReleaseImagePanelInfo(IntPtr hDC)
        {
            if (this.InvokeRequired)
            {
                _ReleaseImagePanelInfo d = new _ReleaseImagePanelInfo(ReleaseImagePanelInfo);
                this.Invoke(d, new object[] { hDC });

            }
            else
            {
                ReleaseDC(this.panel_image.Handle, hDC);
            }
        }
#if NO
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case FingerPrintServer.WM_TOPMOST:
                    if (m.LParam.ToInt32() == 1)
                        this.TopMost = true;
                    else
                        this.TopMost = false;
                    // m.Result = new IntPtr(this.m_nSequenceNumber++);
                    return;
            }
            base.DefWndProc(ref m);
        }
#endif

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

            this.ToolStripMenuItem_start.Enabled = false;
            return 0;
        }

        void EndChannel()
        {
            // TODO: 这里有点乱。应该是通过 m_fingerprintChannel 是否为空来判断
            if (this.m_fingerprintObj != null)
            {
                ChannelServices.UnregisterChannel(m_fingerprintChannel);
                this.m_fingerprintObj = null;
                this.ToolStripMenuItem_start.Enabled = true;
            }
        }

        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        public static void ScrollToEnd(WebBrowser webBrowser)
        {
#if NO
            /*
            API.SendMessage(window.Handle,
                API.WM_VSCROLL,
                API.SB_BOTTOM,  // (int)API.MakeLParam(API.SB_BOTTOM, 0),
                0);
             * */
            HtmlDocument doc = webBrowser.Document;
            doc.Window.ScrollTo(0, 0x7fffffff);

            /*
            webBrowser.Invalidate();
            webBrowser.Update();
             * */
#endif
            webBrowser.ScrollToEnd();
        }

        public delegate void Delegate_AppendHtml(string strText);

        public void SafeAppendHtml(string strText)
        {
#if TEST
            object[] pList = { strText };
            this.Invoke(
                new Delegate_AppendHtml(AppendHtml), pList);
#endif
        }

        void AppendHtml(string strText)
        {
            WriteHtml(this.webBrowser1,
                strText);
            ScrollToEnd(this.webBrowser1);
        }

        void ThreadMain()
        {
            string strError = "";
            string strPrevID = "";

            int index = 0;
            try
            {
                while (true)
                {
                    pause_event.WaitOne();

                    if (this.m_bStop == true)
                        break;

                    Thread.Sleep(100);

                    //if (this.m_fingerprintObj.SendKeyEnabled == false)
                    //    continue;

#if NO
                    string strBarcode = "";
                    int nRet = this.m_fingerprintObj.ReadInput(out strBarcode,
                        out strError);
                    if (nRet == -1)
                    {
                        SafeAppendHtml((++index).ToString() + " ReadInput() error: "+strError+"<br/>");
                        continue;
                    }

                    SafeAppendHtml((++index).ToString() + " ReadInput() OK<br/>");
                    SendKeys.SendWait(strBarcode + "\r");
#endif

                }
            }
            finally
            {
                m_fingerprintObj.Close();
            }

            this.DoEnd();
            return;
        }

        public delegate void Delegate_DoEnd();

        void __DoEnd()
        {
            this.EndChannel();
            this.EnableControls(true);
            if (String.IsNullOrEmpty(this.m_strError) == false)
                MessageBox.Show(this.m_strError);
            this.m_bStop = true;
            pause_event.Set();
        }

        public void DoEnd()
        {
            this.Invoke(
                new Delegate_DoEnd(__DoEnd));
        }

        void EnableControls(bool bEnable)
        {
            this.checkBox_beep.Enabled = bEnable;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            if (m_fingerprintObj != null)
            {
                m_fingerprintObj.CancelGetFingerprintString();
            }
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

        private void ToolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ToolStripMenuItem_copyright_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "dp2-中控指纹阅读器接口 V0.04\r\n\r\n(C) 版权所有 2013 数字平台(北京)软件有限责任公司\r\nhttp://dp2003.com\r\n\r\n本程序提供了中控指纹阅读器和 dp2 系统的接口功能",
                "dp2-中控指纹阅读器接口", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox_beep_CheckedChanged(object sender, EventArgs e)
        {
            this.m_bBeep = this.checkBox_beep.Checked;
        }

        private void checkBox_speak_CheckedChanged(object sender, EventArgs e)
        {
            this.m_bSpeak = this.checkBox_speak.Checked;
        }

        public static void AddShortcutToStartupGroup(string strProductName)
        {
            if (ApplicationDeployment.IsNetworkDeployed &&
                ApplicationDeployment.CurrentDeployment != null &&
                ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {

                string strTargetPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                strTargetPath = Path.Combine(strTargetPath, strProductName) + ".appref-ms";

                string strSourcePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                strSourcePath = Path.Combine(strSourcePath, strProductName) + ".appref-ms";

                File.Copy(strSourcePath, strTargetPath, true);
            }
        }

        private void ToolStripMenuItem_option_Click(object sender, EventArgs e)
        {
            OptionDialog dlg = new OptionDialog();

            dlg.DisplayFingerprintImage = this.m_bDisplayFingerprintImage;
            dlg.Threshold = this.m_nThreshold;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            this.m_bDisplayFingerprintImage = dlg.DisplayFingerprintImage;
            this.m_nThreshold = dlg.Threshold;

            if (this.m_fingerprintObj != null)
                this.m_fingerprintObj.SetParameter("Threshold", this.m_nThreshold);

        }

        private void ToolStripMenuItem_start_Click(object sender, EventArgs e)
        {
            OpenServer();
        }

        private void ToolStripMenuItem_reopen_Click(object sender, EventArgs e)
        {
            // 警告关闭
            DialogResult result = MessageBox.Show(this,
                "确实要重新启动 dp2-中控指纹阅读器接口?\r\n\r\n(重新打开后，需要手动初始化指纹缓存)",
                "dp2-中控指纹阅读器接口",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            if (this.m_fingerprintObj != null)
                this.m_fingerprintObj.Close();
            this.EndChannel();
            OpenServer();
        }
#if NO
        public static void AddShortcutToStartupGroup(string publisherName, string productName)
        {

            if (ApplicationDeployment.IsNetworkDeployed &&
                ApplicationDeployment.CurrentDeployment != null &&
                ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {

                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                startupPath = Path.Combine(startupPath, productName) + ".appref-ms";

                if (!File.Exists(startupPath))
                {

                    string allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

                    string shortcutPath = Path.Combine(allProgramsPath, publisherName);

                    shortcutPath = Path.Combine(shortcutPath, productName) + ".appref-ms";

                    File.Copy(shortcutPath, startupPath);

                }

            }

        }
#endif

        const int WM_DEVICECHANGE = 0x0219; //see msdn site
        const int DBT_DEVNODES_CHANGED = 0x0007;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVALCOMPLETE = 0x8004;
        const int DBT_DEVTYPVOLUME = 0x00000002;

#if NO
        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_DEVICECHANGE)
            {
                try
                {
                    DEV_BROADCAST_VOLUME vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));
                    if ((m.WParam.ToInt32() == DBT_DEVICEARRIVAL) && (vol.dbcv_devicetype == DBT_DEVTYPVOLUME))
                    {
                        MessageBox.Show(DriveMaskToLetter(vol.dbcv_unitmask).ToString());
                    }
                    if ((m.WParam.ToInt32() == DBT_DEVICEREMOVALCOMPLETE) && (vol.dbcv_devicetype == DBT_DEVTYPVOLUME))
                    {
                        MessageBox.Show("usb out");
                    }
                }
                catch
                {
                }
            }
            base.WndProc(ref m);
        }
#endif
        delegate void _RefreshServer();

        public void RefreshServer()
        {
            if (this.m_fingerprintObj == null)
                OpenServer(false);
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == DBT_DEVNODES_CHANGED)
                {
                    _RefreshServer d = new _RefreshServer(RefreshServer);
                    this.BeginInvoke(d);
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

        [StructLayout(LayoutKind.Sequential)] //Same layout in mem
        public struct DEV_BROADCAST_VOLUME
        {
            public int dbcv_size;
            public int dbcv_devicetype;
            public int dbcv_reserved;
            public int dbcv_unitmask;
        }

        private static char DriveMaskToLetter(int mask)
        {
            char letter;
            string drives = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //1 = A, 2 = B, 3 = C
            int cnt = 0;
            int pom = mask / 2;
            while (pom != 0)    // while there is any bit set in the mask shift it right        
            {
                pom = pom / 2;
                cnt++;
            }
            if (cnt < drives.Length)
                letter = drives[cnt];
            else
                letter = '?';
            return letter;
        }

        public bool GameState
        {
            get
            {
                return this.m_bGameState;
            }
            set
            {
                this.m_bGameState = value;
                if (value == true)
                {
                    this.ToolStripMenuItem_gameState.Checked = true;
                    this.BackColor = Color.Red;
                    this.ForeColor = Color.White;
                }
                else
                {
                    this.ToolStripMenuItem_gameState.Checked = false;
                    this.BackColor = SystemColors.Control;
                    this.ForeColor = SystemColors.ControlText;
                }
            }
        }

        private void ToolStripMenuItem_gameState_Click(object sender, EventArgs e)
        {
            if (this.GameState == true)
            {
                this.GameState = false;
                this.label_message.Text = "";
            }
            else
            {
                this.GameState = true;
                MessageBox.Show(this, "提示：练习状态下，指纹识别和键盘仿真功能暂时无效。\r\n\r\n记得练习结束后，及时退出练习状态哟！");
            }
        }

        private void toolStripSeparator3_Click(object sender, EventArgs e)
        {

        }
    }
}
