using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace PalmCenter.Install
{
    public partial class SettingDialog : Form
    {
        ConfigSetting _config = null;

        public SettingDialog()
        {
            InitializeComponent();
        }

        private void SettingDialog_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void SettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private async void button_OK_Click(object sender, EventArgs e)
        {
            // 按住 Control 键可以越过 dp2library 账户检查
            var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(this.textBox_cfg_dp2LibraryServerUrl.Text))
                errors.Add("尚未指定 dp2library 服务器 URL");
            if (string.IsNullOrEmpty(this.textBox_cfg_userName.Text))
                errors.Add("尚未指定 dp2library 服务器的用户名");

            // 验证登录 dp2library 账户
            if (control == false
                && string.IsNullOrEmpty(this.textBox_cfg_dp2LibraryServerUrl.Text) == false
                && string.IsNullOrEmpty(this.textBox_cfg_userName.Text) == false)
            {
                // TODO: 显示“正在验证 dp2library 账户”
                EnableControls(false);
                try
                {
                    int nRet = -1;
                    string url = this.textBox_cfg_dp2LibraryServerUrl.Text;
                    string userName = this.textBox_cfg_userName.Text;
                    string password = this.textBox_cfg_password.Text;
                    string location = this.textBox_cfg_location.Text;
                    string strError = "";
                    await Task.Run(() =>
                    {

                        // 进行验证性登录
                        // return:
                        //      -1  error
                        //      0   登录未成功
                        //      1   登录成功
                        nRet = LibraryChannel.VerifyLogin(
                                url,
                                userName,
                                password,
                                location,
                                "palmCenter|0.01",
                                out string temp);
                        strError = temp;
                    });
                    if (nRet != 1)
                    {
                        // MessageBox.Show(this, $"账户登录失败: {strError}");
                        if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                            errors.Add($"账户 {this.textBox_cfg_userName.Text} 登录失败，请检查配置");
                    }
                }
                finally
                {
                    EnableControls(true);
                }
            }

            if (errors.Count > 0)
                goto ERROR1;

            SaveConfig();
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, StringUtil.MakePathList(errors, "\r\n"));
        }

        void EnableControls(bool enable)
        {
            this.tabControl1.Enabled = enable;
            this.button_OK.Enabled = enable;
            this.button_Cancel.Enabled = enable;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        static string GetFileName()
        {
            string dir = Utility.GetServiceUserDirectory("palmCenter");
            PathUtil.CreateDirIfNeed(dir);
            return Path.Combine(dir, "settings.xml");
        }

        void LoadConfig()
        {
            var fileName = GetFileName();
            _config = new ConfigSetting(fileName, true);

            this.textBox_cfg_dp2LibraryServerUrl.Text = _config.Get(
                "libraryServer",
                "Url");
            this.textBox_cfg_userName.Text = _config.Get(
                "libraryServer",
                "userName");
            var password = _config.Get(
                "libraryServer",
                "password");
            this.textBox_cfg_password.Text = Utility.DecryptPasssword(password);

            this.textBox_cfg_location.Text = _config.Get(
                    "libraryServer",
                    "clientLocation");

            this.textBox_replicationStart.Text = _config.Get(
                    "libraryServer",
                    "replicationStart");
        }

        void SaveConfig()
        {
            _config.Set(
                "libraryServer",
                "Url",
                this.textBox_cfg_dp2LibraryServerUrl.Text);

            _config.Set(
               "libraryServer",
               "userName",
               this.textBox_cfg_userName.Text);

            var password = Utility.EncryptPassword(this.textBox_cfg_password.Text);
            _config.Set(
            "libraryServer",
            "password",
            password);

            _config.Set(
        "libraryServer",
        "clientLocation",
        this.textBox_cfg_location.Text);

            _config.Set(
        "libraryServer",
        "replicationStart",
        this.textBox_replicationStart.Text);

            _config.Save();
        }

        public static string HnbUrl = "rest.http://58.87.101.80/hnb/rest";   // "http://hnbclub.cn/dp2library";

        private void toolStripButton_cfg_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_cfg_dp2LibraryServerUrl.Text != HnbUrl)
            {
                this.textBox_cfg_dp2LibraryServerUrl.Text = HnbUrl;

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
    }
}
