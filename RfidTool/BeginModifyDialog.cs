using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace RfidTool
{
    public partial class BeginModifyDialog : Form
    {
        public BeginModifyDialog()
        {
            InitializeComponent();
        }


        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.comboBox_filter_tu,
                    this.checkBox_oi,
                    this.textBox_rfid_oi,
                    this.checkBox_aoi,
                    this.textBox_rfid_aoi,
                    this.checkBox_uidPiiMap,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.comboBox_filter_tu,
                    this.checkBox_oi,
                    this.textBox_rfid_oi,
                    this.checkBox_aoi,
                    this.textBox_rfid_aoi,
                    this.checkBox_uidPiiMap,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_oi_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_rfid_oi.Enabled = this.checkBox_oi.Checked;
        }

        private void checkBox_aoi_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_rfid_aoi.Enabled = this.checkBox_aoi.Checked;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.checkBox_oi.Checked && this.checkBox_aoi.Checked)
            {
                strError = "机构代码和非标准机构代码不允许同时修改。请重新选择";
                goto ERROR1;
            }

            if (this.checkBox_oi.Checked && string.IsNullOrEmpty(this.textBox_rfid_oi.Text))
            {
                strError = "请输入机构代码";
                goto ERROR1;
            }

            if (this.checkBox_aoi.Checked && string.IsNullOrEmpty(this.textBox_rfid_aoi.Text))
            {
                strError = "请输入非标准机构代码";
                goto ERROR1;
            }

            strError = SettingDialog.VerifyOI(this.textBox_rfid_oi.Text);
            if (strError != null)
                goto ERROR1;

            if (string.IsNullOrEmpty(this.textBox_rfid_aoi.Text) == false)
            {
                // TODO: 当 textbox 内容发生过变化才警告
                this.tabControl1.SelectedTab = this.tabPage_action;
                DialogResult result = MessageBox.Show(this,
    @"警告：如无特殊原因，应尽量使用机构代码而非“非标准机构代码”。因“非标准机构代码”在馆际互借等场合可能会遇到重复冲突等问题。详情请咨询数字平台工程师。

确实要使用“非标准机构代码”?",
    "BeginModifyDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
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

        public string OiString
        {
            get
            {
                if (this.checkBox_oi.Checked == false)
                    return null;
                return this.textBox_rfid_oi.Text;
            }
        }

        public string AoiString
        {
            get
            {
                if (this.checkBox_aoi.Checked == false)
                    return null;
                return this.textBox_rfid_aoi.Text;
            }
        }

        public bool LinkUID
        {
            get
            {
                return this.checkBox_uidPiiMap.Checked;
            }
        }

        public string FilterTU
        {
            get
            {
                return this.comboBox_filter_tu.Text;
            }
        }
    }
}
