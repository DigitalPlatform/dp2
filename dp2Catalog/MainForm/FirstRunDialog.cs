using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;

namespace dp2Catalog
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
            if (this.tabControl_main.SelectedIndex < this.tabControl_main.TabPages.Count - 1)
            {
                this.tabControl_main.SelectedIndex++;
                SetTitle();
                SetButtonState();
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

            if (this.comboBox_server_serverType.Text == "[暂时不使用任何服务器]")
                goto END1;

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

            if (this.comboBox_server_serverType.Text == "其它服务器"
                || string.IsNullOrEmpty(this.comboBox_server_serverType.Text) == true)
            {
                string strServerName = InputDlg.GetInput(this, "请给这个服务器取一个便于识别的名字", "服务器名:", "新服务器", this.Font);
                if (strServerName != null)
                {
                    // this.comboBox_server_serverType.Text = strServerName;
                    this.ServerName = strServerName;
                }
            }

        END1:
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

            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            _messageBar.Font = this.Font;
            _messageBar.BackColor = SystemColors.Info;
            _messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "欢迎使用 dp2Catalog";
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

            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);

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
    DigitalPlatform.CirculationClient.BeforeLoginEventArgs e)
        {

            if (e.FirstTry == true)
            {
                e.UserName = this.textBox_server_userName.Text;
                e.Password = this.textBox_server_password.Text;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            e.Cancel = true;
            return;
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

                    if (this.Channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.RequestError)
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
                this.textBox_license.Text = sr.ReadToEnd();
            }
        }

        /*
单机版 (dp2Library XE)
红泥巴 · 数字平台服务器
其它服务器
         * * */
        private void comboBox_server_serverType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.textBox_server_userName.Enabled = true;
            this.textBox_server_password.Enabled = true;
            this.textBox_server_dp2LibraryServerUrl.Enabled = true;

            if (this.comboBox_server_serverType.Text == "单机版 (dp2Library XE)")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = "net.pipe://localhost/dp2library/XE";
                this.textBox_server_dp2LibraryServerUrl.ReadOnly = true;

                this.textBox_server_userName.Text = "supervisor";
                this.textBox_server_password.Text = "";
            }
            else if (this.comboBox_server_serverType.Text == "红泥巴 · 数字平台服务器")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = "http://hnbclub.cn/dp2library";
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
            else if (this.comboBox_server_serverType.Text == "[暂时不使用任何服务器]")
            {
                this.textBox_server_dp2LibraryServerUrl.Text = "";
                this.textBox_server_dp2LibraryServerUrl.Enabled = false;

                this.textBox_server_userName.Text = "";
                this.textBox_server_userName.Enabled = false;
                this.textBox_server_password.Text = "";
                this.textBox_server_password.Enabled = false;
            }
            else
            {
                MessageBox.Show(this, "未知的服务器类型 '" + this.comboBox_server_serverType.Text + "'");
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

        public string ServerType
        {
            get
            {
                return this.comboBox_server_serverType.Text;
            }
            set
            {
                this.comboBox_server_serverType.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_server_dp2LibraryServerUrl.Text;
            }
            set
            {
                this.textBox_server_dp2LibraryServerUrl.Text = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.textBox_server_userName.Text;
            }
            set
            {
                this.textBox_server_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_server_password.Text;
            }
            set
            {
                this.textBox_server_password.Text = value;
            }
        }

        string _serverName = "";

        public string ServerName
        {
            get
            {
                if (this.comboBox_server_serverType.Text == "其它服务器"
                    || string.IsNullOrEmpty(this.comboBox_server_serverType.Text) == true)
                    return _serverName;

                return this.comboBox_server_serverType.Text;
            }
            set
            {
                this._serverName = value;
            }
        }

        void OnChecked()
        {
            if (this.radioButton_licenseMode_standard.Checked == true)
            {
                this.radioButton_licenseMode_testing.Checked = false;
            }
            else if (this.radioButton_licenseMode_testing.Checked == true)
            {
                this.radioButton_licenseMode_standard.Checked = false;
            }
        }

        public string Mode
        {
            get
            {
                if (this.radioButton_licenseMode_testing.Checked == true)
                    return "test";
                else
                    return "standard";
            }
            set
            {
                if (value == "test")
                    this.radioButton_licenseMode_testing.Checked = true;
                else
                    this.radioButton_licenseMode_standard.Checked = true;
            }
        }

        private void radioButton_licenseMode_testing_CheckedChanged(object sender, EventArgs e)
        {
            OnChecked();
        }

        private void radioButton_licenseMode_standard_CheckedChanged(object sender, EventArgs e)
        {
            OnChecked();
        }
    }
}
