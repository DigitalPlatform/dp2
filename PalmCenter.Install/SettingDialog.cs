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

            /*
            this.tabControl1.TabPages.Remove(this.tabPage_palm);
            this.tabPage_palm.Dispose();
            */
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
                            errors.Add($"账户 {this.textBox_cfg_userName.Text} 登录失败({strError})，请检查配置");
                    }
                }
                finally
                {
                    EnableControls(true);
                }
            }

            // 验证扫描次数
            {
                string error = VerifyRegisterScans(this.textBox_palm_registerScans.Text);
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error);
            }
            // 验证识别阈值
            {
                string error = VerifyIdentityThreshold(this.textBox_palm_identityThreshold.Text);
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error);
            }

            // 验证识别图像质量阈值
            {
                string error = VerifyIdentityQualityThreshold(this.textBox_palm_identityQualityThreshold.Text);
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error);
            }

            // 验证登记图像质量阈值
            {
                string error = VerifyRegisterQualityThreshold(this.textBox_palm_registerQualityThreshold.Text);
                if (string.IsNullOrEmpty(error) == false)
                    errors.Add(error);
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

        // 登记时默认的扫描次数
        public static int DefaultRegisterScans = 5;
        // 掌纹识别的默认阈值
        public static int DefaultIdentifyThreshold = 576;
        // 掌纹识别的分数基数
        public static int IdentifyBase = 1000;

        // (识别阶段)掌纹图像质量的阈值
        public static int DefaultIdentityQualityThreshold = 100;
        // (识别阶段)掌纹图像质量的理论最高值
        public static int IdentityQualityBase = 300;

        // (登记阶段)掌纹图像质量的阈值
        public static int DefaultRegisterQualityThreshold = 200;
        // (登记阶段)掌纹图像质量的理论最高值
        public static int RegisterQualityBase = 300;

        static string VerifyRegisterScans(string count)
        {
            if (string.IsNullOrEmpty(count))
                return null;

            if (Int32.TryParse(count, out int value) == false)
                return ($"登记时扫描次数 '{count}' 格式不正确。应为一个数字");

            if (!(value >= 1 && value <= DefaultRegisterScans))
                return ($"登记时扫描次数 '{value}' 超出合法范围。应为 1~5 的数字");

            return null;
        }

        static string VerifyIdentityThreshold(string count)
        {
            if (string.IsNullOrEmpty(count))
                return null;

            if (Int32.TryParse(count, out int value) == false)
                return ($"掌纹比对阈值 '{count}' 格式不正确。应为一个 0~{IdentifyBase} 数字");

            if (!(value >= 0 && value <= IdentifyBase))
                return ($"掌纹比对阈值 '{value}' 超出合法范围。应为 0~{IdentifyBase} 的数字");

            return null;
        }

        static string VerifyIdentityQualityThreshold(string count)
        {
            if (string.IsNullOrEmpty(count))
                return null;

            if (Int32.TryParse(count, out int value) == false)
                return ($"掌纹比对图像质量阈值 '{count}' 格式不正确。应为一个 0~{IdentityQualityBase} 数字");

            if (!(value >= 0 && value <= IdentityQualityBase))
                return ($"掌纹比对图像质量阈值 '{value}' 超出合法范围。应为 0~{IdentityQualityBase} 的数字");

            return null;
        }

        static string VerifyRegisterQualityThreshold(string count)
        {
            if (string.IsNullOrEmpty(count))
                return null;

            if (Int32.TryParse(count, out int value) == false)
                return ($"掌纹登记图像质量阈值 '{count}' 格式不正确。应为一个 0~{RegisterQualityBase} 数字");

            if (!(value >= 0 && value <= RegisterQualityBase))
                return ($"掌纹登记图像质量阈值 '{value}' 超出合法范围。应为 0~{RegisterQualityBase} 的数字");

            return null;
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

            this.textBox_palm_registerScans.Text = _config.Get(
"palm",
"registerScans",
DefaultRegisterScans.ToString());


            this.textBox_palm_identityThreshold.Text = _config.Get(
"palm",
"identityThreshold",
DefaultIdentifyThreshold.ToString());

            this.textBox_palm_registerQualityThreshold.Text = _config.Get(
"palm",
"registerQualityThreshold",
DefaultRegisterQualityThreshold.ToString());

            this.textBox_palm_identityQualityThreshold.Text = _config.Get(
"palm",
"identityQualityThreshold",
DefaultIdentityQualityThreshold.ToString());

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

            _config.Set(
"palm",
"registerScans",
this.textBox_palm_registerScans.Text);

            _config.Set(
"palm",
"identityThreshold",
this.textBox_palm_identityThreshold.Text);

            _config.Set(
"palm",
"registerQualityThreshold",
this.textBox_palm_registerQualityThreshold.Text);

            _config.Set(
"palm",
"identityQualityThreshold",
this.textBox_palm_identityQualityThreshold.Text);

            if (_config.Changed)
                _config.Save();
        }

        public static string HnbUrl = "rest.http://pear.ilovelibrary.cn/hnb/rest/";   // "http://hnbclub.cn/dp2library";

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

        private void checkBox_allow_changeIdentityThreshold_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_palm_identityThreshold.ReadOnly = !this.checkBox_allow_changeThreshold.Checked;
        }

        private void button_palm_setDefaultIdentityThreshold_Click(object sender, EventArgs e)
        {
            this.textBox_palm_identityThreshold.Text = DefaultIdentifyThreshold.ToString();
        }

        private void checkBox_allow_changeRegisterQuality_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_palm_registerQualityThreshold.ReadOnly = !this.checkBox_allow_changeRegisterQuality.Checked;
        }

        private void button_palm_setDefaultRegisterQuality_Click(object sender, EventArgs e)
        {
            this.textBox_palm_registerQualityThreshold.Text = DefaultRegisterQualityThreshold.ToString();
        }

        private void checkBox_allow_changeIdentityQuality_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_palm_identityQualityThreshold.ReadOnly = !this.checkBox_allow_changeIdentityQuality.Checked;
        }

        private void button_palm_setDefaultIdentityQuality_Click(object sender, EventArgs e)
        {
            this.textBox_palm_identityQualityThreshold.Text = DefaultIdentityQualityThreshold.ToString();
        }
    }
}
