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
        }

        private void SettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> errors = new List<string>();

            if (control == false
                && string.IsNullOrEmpty(this.textBox_rfid_rfidCenterUrl.Text))
                errors.Add("尚未设置 RFID 读卡器接口 URL");

            if (Int32.TryParse(this.textBox_sip_port.Text, out int port) == false)
                errors.Add("SIP 服务器端口号必须为数字");

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "\r\n");
                goto ERROR1;
            }

            DataModel.RfidCenterUrl = this.textBox_rfid_rfidCenterUrl.Text;
            DataModel.BeforeScanSeconds = (int)this.numericUpDown_seconds.Value;

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

            // 释放所有 libraryChannel 和 sipChannel 通道
            LibraryChannelUtil.Clear();
            SipChannelUtil.CloseChannel();

            // 检查两个服务器 URL 必须至少配置其中一个
            if (control == false
                && string.IsNullOrEmpty(DataModel.dp2libraryServerUrl)
                && string.IsNullOrEmpty(DataModel.sipServerAddr))
            {
                strError = "dp2library 服务器 URL 和 SIP 服务器地址两者必须配置其中一个";
                goto ERROR1;
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

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
