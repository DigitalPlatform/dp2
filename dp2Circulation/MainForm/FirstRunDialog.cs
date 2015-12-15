using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    public partial class FirstRunDialog : Form
    {
        public MainForm MainForm = null;

        public FirstRunDialog()
        {
            InitializeComponent();
        }

        private void button_prev_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex > 0)
            {
                this.tabControl_main.SelectedIndex--;
                SetTitle();
                SetButtonState();
            }
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            // 属性页切换前的检查
            if (this.tabControl_main.SelectedTab == this.tabPage_licenseMode)
            {
                if (this.radioButton_licenseMode_standard.Checked == true)
                {
                    DialogResult result = MessageBox.Show(this,
"安装专业版需要您事先获得数字平台或经销商的授权。为鉴别授权，稍后步骤需要您获取和输入序列号，安装才能成功。\r\n(您也可以改为选择社区版，此种方式不需要序列号)\r\n\r\n确信要继续安装为专业版?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        return;
                    }
                }
            }

            // 切换属性页
            if (this.tabControl_main.SelectedIndex < this.tabControl_main.TabPages.Count - 1)
            {
                this.tabControl_main.SelectedIndex++;
                SetTitle();
                SetButtonState();
            }

            // 切换后的补充动作
            if (this.tabControl_main.SelectedTab == this.tabPage_serverInfo)
            {
                if (string.IsNullOrEmpty(this.comboBox_server_serverType.Text) == true
                    && this.radioButton_licenseMode_community.Checked == true)
                    this.comboBox_server_serverType.SelectedIndex = 0;  // 自动选定单机版
            }
        }

        void SetTitle()
        {
            this.Text = this.tabControl_main.SelectedTab.Text;
        }

        void SetButtonState()
        {
            if (this.tabControl_main.SelectedIndex == 0)
                this.button_prev.Enabled = false;
            else
                this.button_prev.Enabled = true;

            if (this.tabControl_main.SelectedIndex >= this.tabControl_main.TabPages.Count - 1)
                this.button_next.Enabled = false;
            else
            {
                if (this.tabControl_main.SelectedTab == this.tabPage_license
                    && this.checkBox_license_agree.Checked == false)
                    this.button_next.Enabled = false;
                else
                    this.button_next.Enabled = true;
            }

            if (this.tabControl_main.SelectedIndex == this.tabControl_main.TabPages.Count - 1)
                this.button_finish.Enabled = true;
            else
                this.button_finish.Enabled = false;
        }

        private void button_finish_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_server_dp2LibraryServerUrl.Text) == true)
            {
                strError = "请输入 dp2library 服务器 URL 地址";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_server_userName.Text) == true)
            {
                strError = "请输入 用户名";
                goto ERROR1;
            }

            int nRet = TestConnectServer(out strError);
            if (nRet == -1)
                goto ERROR1;

            this.MainForm.AppInfo.SetString("config",
    "circulation_server_url",
    this.textBox_server_dp2LibraryServerUrl.Text);

            this.MainForm.AppInfo.SetString(
    "default_account",
    "username",
    this.textBox_server_userName.Text);

            string strPassword = this.MainForm.EncryptPassword(this.textBox_server_password.Text);
            this.MainForm.AppInfo.SetString(
                "default_account",
                "password",
                strPassword);

            this.MainForm.AppInfo.SetBoolean(
    "default_account",
    "savepassword_short",
    true);

            this.MainForm.AppInfo.SetBoolean(
    "default_account",
    "savepassword_long",
    true);

            this.MainForm.AppInfo.SetBoolean(
    "default_account",
    "isreader",
    false);
            this.MainForm.AppInfo.SetString(
                "default_account",
                "location",
                "");
            this.MainForm.AppInfo.SetBoolean(
                "default_account",
                "occur_per_start",
                true);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 验证性连接服务器
        int TestConnectServer(out string strError)
        {
            strError = "";

            // 如果是即将访问 dp2libraryXE 单机版，这里要启动它
            if (string.Compare(this.textBox_server_dp2LibraryServerUrl.Text,
                CirculationLoginDlg.dp2LibraryXEServerUrl, true) == 0)
            {
                string strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2Library XE");
                if (File.Exists(strShortcutFilePath) == false)
                {
                    // 安装和启动
                    DialogResult result = MessageBox.Show(this,
"dp2libraryXE 在本机尚未安装。\r\ndp2Circulation (内务)即将访问 dp2LibraryXE 单机版服务器，需要安装它才能正常使用。\r\n\r\n是否立即从 dp2003.com 下载安装?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        StartDp2libraryXe(
                            this,
                            "dp2Circulation",
                            this.Font,
                            false);
                }
                else
                {
                    if (HasDp2libraryXeStarted() == false)
                    {
                        StartDp2libraryXe(
                            this,
                            "dp2Circulation",
                            this.Font,
                            true);
                    }
                }
            }


            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            _messageBar.Font = this.Font;
            _messageBar.BackColor = SystemColors.Info;
            _messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "欢迎使用 dp2Circulation";
            _messageBar.MessageText = "正在验证连接服务器，请等待 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            _messageBar.Show(this);
            _messageBar.Update();

            try
            {
                int nRet = TouchServer(true, out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                _messageBar.Close();
                _messageBar = null;
            }
        }

        // parameters:
        //      bLocal  是否从本地启动。 false 表示连安装带启动
        public static void StartDp2libraryXe(
            IWin32Window owner,
            string strDialogTitle,
            Font font,
            bool bLocal)
        {
            MessageBar messageBar = null;

            messageBar = new MessageBar();
            messageBar.TopMost = false;
            if (font != null)
                messageBar.Font = font;
            messageBar.BackColor = SystemColors.Info;
            messageBar.ForeColor = SystemColors.InfoText;
            messageBar.Text = "dp2 内务";
            messageBar.MessageText = "正在启动 dp2Library XE，请等待 ...";
            messageBar.StartPosition = FormStartPosition.CenterScreen;
            messageBar.Show(owner);
            messageBar.Update();

            Application.DoEvents();
            try
            {
                TimeSpan waitTime = new TimeSpan(0, 1, 0);

                string strShortcutFilePath = "";
                if (bLocal == true)
                    strShortcutFilePath = PathUtil.GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2Library XE");
                else
                {
                    strShortcutFilePath = "http://dp2003.com/dp2libraryxe/v1/dp2libraryxe.application";
                    waitTime = new TimeSpan(0, 5, 0);  // 安装需要的等待时间更长
                }

                // TODO: detect if already started
                using (EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset,
                    "dp2libraryXE V1 library host started"))
                {
                    Application.DoEvents();

                    Process.Start(strShortcutFilePath);

                    DateTime start = DateTime.Now;
                    while (true)
                    {
                        Application.DoEvents();
                        // wait till started
                        // http://stackoverflow.com/questions/6816782/windows-net-cross-process-synchronization
                        if (eventWaitHandle.WaitOne(100, false) == true)
                            break;

                        // if timeout, prompt continue wait
                        if (DateTime.Now - start > waitTime)
                        {
                            DialogResult result = MessageBox.Show(owner,
    "dp2libraryXE 暂时没有响应。\r\n\r\n是否继续等待其响应?",
    strDialogTitle,
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                break;

                            start = DateTime.Now;   // 
                        }

                    }
                }
            }
            finally
            {
                messageBar.Close();
            }
        }

        public static bool HasDp2libraryXeStarted()
        {
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true, "dp2libraryXE V1", out createdNew))
            {
                if (createdNew)
                {
                    return false;
                }
                else
                {
#if NO
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            API.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
#endif
                    return true;
                }
            }

        }

        /// <summary>
        /// 停止控制
        /// </summary>
        DigitalPlatform.Stop Stop = null;
        /// <summary>
        /// 通讯通道。MainForm 自己使用
        /// </summary>
        LibraryChannel Channel = new LibraryChannel();

        // return:
        //      0   没有准备成功
        //      1   准备成功
        /// <summary>
        /// 准备进行检索
        /// </summary>
        /// <returns>0: 没有成功; 1: 成功</returns>
        int PrepareSearch()
        {
            if (String.IsNullOrEmpty(this.textBox_server_dp2LibraryServerUrl.Text) == true)
                return 0;

            this.Channel.Url = this.textBox_server_dp2LibraryServerUrl.Text;

            this.Channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(this.MainForm.stopManager, true);	// 和容器关联

            return 1;
        }

        /// <summary>
        /// 结束检索
        /// </summary>
        /// <returns>返回 0</returns>
        int EndSearch()
        {
            if (Stop != null) // 脱离关联
            {
                Stop.Unregister();	// 和容器关联
                Stop = null;
            }

            return 0;
        }

        internal void Channel_BeforeLogin(object sender,
    DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {

            if (e.FirstTry == true)
            {
                e.UserName = this.textBox_server_userName.Text;
                e.Password = this.textBox_server_password.Text;

                //bool bIsReader = false;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            e.Cancel = true;
            return;
        }


        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
        }


        int TouchServer(bool bPrepareSearch,
            out string strError)
        {
            strError = "";
            if (bPrepareSearch == true)
            {
                if (this.PrepareSearch() == 0)
                    return -1;
            }

            try
            {
                string strTime = "";
                long lRet = this.Channel.GetClock(null,
                    out strTime,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.WcfException is System.ServiceModel.Security.MessageSecurityException)
                    {
                        // 通讯安全性问题，时钟问题
                        strError = strError + "\r\n\r\n有可能是前端机器时钟和服务器时钟差异过大造成的";
                        return -1;
                    }

                    if (this.Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.RequestError)
                    {
                        if (this.comboBox_server_serverType.Text == "单机版 (dp2Library XE)")
                            strError += "\r\n\r\n请检查 dp2libraryXE 模块确实安装和启动了？";
                        else if (this.comboBox_server_serverType.Text == "红泥巴 · 数字平台服务器")
                            strError += "\r\n\r\n请检查 网络确实通畅？可以用浏览器试着访问一下 http://hnbclub.cn 看看";
                        else if (this.comboBox_server_serverType.Text == "其它服务器")
                            strError += "\r\n\r\n请检查 网络确实通畅？dp2Library 服务器 URL 地址输入是否正确？";
                    }

                    return -1;
                }

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                lRet = this.Channel.Login(this.textBox_server_userName.Text,
                    this.textBox_server_password.Text,
                    "type=worker",
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }

                if (lRet == 0)
                {
                    strError = "用户名或密码不正确";
                    return -1;
                }
            }
            finally
            {
                if (bPrepareSearch == true)
                    this.EndSearch();
            }

            return 0;
        }

        private void FirstRunDialog_Load(object sender, EventArgs e)
        {
            SetTitle();
            SetButtonState();
            LoadEula();
        }

        void LoadEula()
        {
            string strFileName = Path.Combine(this.MainForm.DataDir, "eula.txt");
            using (StreamReader sr = new StreamReader(strFileName, true))
            {
                this.textBox_license.Text = sr.ReadToEnd().Replace("\r\n","\n").Replace("\n","\r\n");   // 两个 Replace() 会将只有 LF 结尾的行处理为 CR LF
            }
        }

        /*
单机版 (dp2Library XE)
红泥巴 · 数字平台服务器
其它服务器
         * * */
        private void comboBox_server_serverType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_server_serverType.Text == "单机版 (dp2Library XE)")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = "net.pipe://localhost/dp2library/XE";
                this.textBox_server_dp2LibraryServerUrl.ReadOnly = true;

                this.textBox_server_userName.Text = "supervisor";
                this.textBox_server_password.Text = "";
            }
            else if (this.comboBox_server_serverType.Text == "红泥巴 · 数字平台服务器")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = ServerDlg.HnbUrl;
                this.textBox_server_dp2LibraryServerUrl.ReadOnly = true;

                this.textBox_server_userName.Text = "";
                this.textBox_server_password.Text = "";
            }
            else if (this.comboBox_server_serverType.Text == "其它服务器")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = "";
                this.textBox_server_dp2LibraryServerUrl.ReadOnly = false;

                this.textBox_server_userName.Text = "";
                this.textBox_server_password.Text = "";
            }
            else
            {
                MessageBox.Show(this, "未知的服务器类型 '"+this.comboBox_server_serverType.Text+"'");
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_license)
                this.textBox_license.Select(0, 0);
        }

        private void checkBox_license_agree_CheckedChanged(object sender, EventArgs e)
        {
            SetButtonState();
        }

        private void radioButton_licenseMode_testing_CheckedChanged(object sender, EventArgs e)
        {
            OnChecked();

        }

        private void radioButton_licenseMode_standard_CheckedChanged(object sender, EventArgs e)
        {
            OnChecked();

        }

        void OnChecked()
        {
            if (this.radioButton_licenseMode_standard.Checked == true)
            {
                this.radioButton_licenseMode_community.Checked = false;
            }
            else if (this.radioButton_licenseMode_community.Checked == true)
            {
                this.radioButton_licenseMode_standard.Checked = false;
            }
        }

        public string Mode
        {
            get
            {
                if (this.radioButton_licenseMode_community.Checked == true)
                    return "community";  // "test";
                else
                    return "standard";
            }
            set
            {
                if (value == "community")    // "test"
                    this.radioButton_licenseMode_community.Checked = true;
                else 
                    this.radioButton_licenseMode_standard.Checked = true;
            }
        }
    }
}
