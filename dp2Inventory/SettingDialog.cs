using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace dp2Inventory
{
    public partial class SettingDialog : Form
    {
        public SettingDialog()
        {
            InitializeComponent();
        }

        private void SettingDialog_Load(object sender, EventArgs e)
        {
            this.textBox_rfid_rfidCenterUrl.Text = DataModel.RfidCenterUrl;
            this.numericUpDown_seconds.Value = DataModel.BeforeScanSeconds;
            this.checkBox_rfid_rpanTypeSwitch.Checked = DataModel.RfidRpanTypeSwitch;
            this.textBox_rfid_uploadInterfaceUrl.Text = DataModel.uploadInterfaceUrl;

            this.textBox_dp2library_serverUrl.Text = DataModel.dp2libraryServerUrl;
            this.textBox_dp2library_userName.Text = DataModel.dp2libraryUserName;
            this.textBox_dp2library_password.Text = DataModel.dp2libraryPassword;
            this.textBox_dp2library_location.Text = DataModel.dp2libraryLocation;

            this.textBox_sip_serverAddr.Text = DataModel.sipServerAddr;
            this.textBox_sip_port.Text = DataModel.sipServerPort.ToString();
            this.textBox_sip_userName.Text = DataModel.sipUserName;
            this.textBox_sip_password.Text = DataModel.sipPassword;
            this.textBox_sip_encoding.Text = DataModel.sipEncoding;
            this.textBox_sip_institution.Text = DataModel.sipInstitution;
            this.textBox_sip_locationList.Text = DataModel.sipLocationList?.Replace(",", "\r\n");
            this.checkBox_sip_localStore.Checked = DataModel.sipLocalStore;

            this.checkBox_enableTagCache.Checked = DataModel.EnableTagCache;
            this.textBox_verifyRule.Text = DataModel.PiiVerifyRule;

        }

        private void SettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        /*
        // 获得 SIP 服务器组合参数字符串
        static string GetSipServerString()
        {
            if (string.IsNullOrEmpty(DataModel.sipServerAddr))
                return "";
            return DataModel.sipServerAddr + ":" + DataModel.sipServerPort + "|" + DataModel.sipUserName + "|" + DataModel.sipPassword;
        }
        */

        private async void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> errors = new List<string>();

            if (control == false
                && string.IsNullOrEmpty(this.textBox_rfid_rfidCenterUrl.Text))
                errors.Add("尚未设置 RFID 读卡器接口 URL");

            if (Int32.TryParse(this.textBox_sip_port.Text, out int port) == false)
                errors.Add("SIP 服务器端口号必须为数字");

            if (this.textBox_sip_locationList.Text.IndexOf(",") != -1)
                errors.Add("SIP 馆藏地中不允许出现逗号(应为每行一个馆藏地)");

            // 检查两个服务器 URL 必须至少配置其中一个
            if (control == false
                && string.IsNullOrEmpty(this.textBox_dp2library_serverUrl.Text)
                && string.IsNullOrEmpty(this.textBox_sip_serverAddr.Text))
                errors.Add("dp2library 服务器 URL 和 SIP 服务器地址两者必须至少配置其中一个");

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "\r\n");
                goto ERROR1;
            }

            // string oldSipAddress = GetSipServerString();

            List<object> backup_data = Backup();

            DataModel.RfidCenterUrl = this.textBox_rfid_rfidCenterUrl.Text;
            DataModel.BeforeScanSeconds = (int)this.numericUpDown_seconds.Value;
            DataModel.RfidRpanTypeSwitch = this.checkBox_rfid_rpanTypeSwitch.Checked;
            DataModel.uploadInterfaceUrl = this.textBox_rfid_uploadInterfaceUrl.Text;

            DataModel.dp2libraryServerUrl = this.textBox_dp2library_serverUrl.Text;
            DataModel.dp2libraryUserName = this.textBox_dp2library_userName.Text;

            DataModel.dp2libraryPassword = this.textBox_dp2library_password.Text;
            DataModel.dp2libraryLocation = this.textBox_dp2library_location.Text;

            DataModel.sipServerAddr = this.textBox_sip_serverAddr.Text;
            DataModel.sipServerPort = port;
            DataModel.sipUserName = this.textBox_sip_userName.Text;
            DataModel.sipPassword = this.textBox_sip_password.Text;
            DataModel.sipEncoding = this.textBox_sip_encoding.Text;
            DataModel.sipInstitution = this.textBox_sip_institution.Text;
            DataModel.sipLocationList = this.textBox_sip_locationList.Text.Replace("\r\n", ",");
            DataModel.sipLocalStore = this.checkBox_sip_localStore.Checked;

            DataModel.EnableTagCache = this.checkBox_enableTagCache.Checked;
            DataModel.PiiVerifyRule = this.textBox_verifyRule.Text;

            // 释放所有 libraryChannel 和 sipChannel 通道
            LibraryChannelUtil.Clear();
            SipChannelUtil.CloseChannel();

            // string sipAddress = GetSipServerString();
            bool succeed = false;
            try
            {
                if (control == false
                    && string.IsNullOrEmpty(DataModel.sipServerAddr) == false)
                {
                    this.Enabled = false;
                    try
                    {
                        // -1出错，0不在线，1正常
                        var result = await SipChannelUtil.DetectSipNetworkAsync();
                        if (result.Value != 1)
                        {
                            strError = $"SIP 服务器地址或相关参数不正确: {result.ErrorInfo}";
                            goto ERROR1;
                        }
                    }
                    finally
                    {
                        this.Enabled = true;
                    }
                }

                // 2021/4/23
                // 重新初始化 dp2library 相关环境
                if (string.IsNullOrEmpty(DataModel.dp2libraryServerUrl) == false)
                {
                    this.Enabled = false;
                    try
                    {
                        var initial_result = LibraryChannelUtil.Initial();
                        if (initial_result.Value == -1
                            && control == false)
                        {
                            strError = $"获得 dp2library 服务器配置失败: {initial_result.ErrorInfo}";
                            goto ERROR1;
                        }
                    }
                    finally
                    {
                        this.Enabled = true;
                    }
                }

                succeed = true;
            }
            finally
            {
                if (succeed == false)
                    Restore(backup_data);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static List<object> Backup()
        {
            List<object> results = new List<object>();

            results.Add(DataModel.RfidCenterUrl);
            results.Add(DataModel.BeforeScanSeconds);
            results.Add(DataModel.RfidRpanTypeSwitch);
            results.Add(DataModel.uploadInterfaceUrl);

            results.Add(DataModel.dp2libraryServerUrl);
            results.Add(DataModel.dp2libraryUserName);

            results.Add(DataModel.dp2libraryPassword);
            results.Add(DataModel.dp2libraryLocation);

            results.Add(DataModel.sipServerAddr);
            results.Add(DataModel.sipServerPort);
            results.Add(DataModel.sipUserName);
            results.Add(DataModel.sipPassword);
            results.Add(DataModel.sipEncoding);
            results.Add(DataModel.sipInstitution);
            results.Add(DataModel.sipLocationList);
            results.Add(DataModel.sipLocalStore);

            results.Add(DataModel.EnableTagCache);
            results.Add(DataModel.PiiVerifyRule);

            return results;
        }

        static void Restore(List<object> data)
        {
            int index = 0;
            DataModel.RfidCenterUrl = (string)data[index++];
            DataModel.BeforeScanSeconds = (int)data[index++];
            DataModel.RfidRpanTypeSwitch = (bool)data[index++];
            DataModel.uploadInterfaceUrl = (string)data[index++];

            DataModel.dp2libraryServerUrl = (string)data[index++];
            DataModel.dp2libraryUserName = (string)data[index++];

            DataModel.dp2libraryPassword = (string)data[index++];
            DataModel.dp2libraryLocation = (string)data[index++];

            DataModel.sipServerAddr = (string)data[index++];
            DataModel.sipServerPort = (int)data[index++];
            DataModel.sipUserName = (string)data[index++];
            DataModel.sipPassword = (string)data[index++];
            DataModel.sipEncoding = (string)data[index++];
            DataModel.sipInstitution = (string)data[index++];
            DataModel.sipLocationList = (string)data[index++];
            DataModel.sipLocalStore = (bool)data[index++];

            DataModel.EnableTagCache = (bool)data[index++];
            DataModel.PiiVerifyRule = (string)data[index++];
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton_cfg_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_dp2library_serverUrl.Text != ServerDlg.HnbUrl)
            {
                this.textBox_dp2library_serverUrl.Text = ServerDlg.HnbUrl;

                this.textBox_dp2library_userName.Text = "";
                this.textBox_dp2library_password.Text = "";
            }
        }

        private void toolStripButton_cfg_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_dp2library_serverUrl.Text != "net.pipe://localhost/dp2library/xe")
            {
                this.textBox_dp2library_serverUrl.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_dp2library_userName.Text = "supervisor";
                this.textBox_dp2library_password.Text = "";
            }
        }

        private void button_rfid_setRfidUrlDefaultValue_Click(object sender, EventArgs e)
        {
            string strDefaultValue = "ipc://RfidChannel/RfidServer";

            DialogResult result = MessageBox.Show(this,
    "确实要将 RFID 读卡器接口 URL 的值设置为常用值\r\n \"" + strDefaultValue + "\" ? ",
    "CfgDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            this.textBox_rfid_rfidCenterUrl.Text = strDefaultValue;
        }
    }
}
