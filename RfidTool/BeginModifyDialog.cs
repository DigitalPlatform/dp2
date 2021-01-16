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
                    this.comboBox_eas,
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
                    this.comboBox_eas,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_oi_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_rfid_oi.Enabled = this.checkBox_oi.Checked;
            if (this.checkBox_oi.Checked)
            {
                this.checkBox_oi.BackColor = Color.LightGreen;
            }
            else
            {
                this.checkBox_oi.BackColor = Color.Transparent;
            }

            // 互斥
            if (this.checkBox_aoi.Checked == true && this.checkBox_oi.Checked == true)
                this.checkBox_aoi.Checked = false;
        }

        private void checkBox_aoi_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_rfid_aoi.Enabled = this.checkBox_aoi.Checked;
            if (this.checkBox_aoi.Checked)
            {
                this.checkBox_aoi.BackColor = Color.LightGreen;
            }
            else
            {
                this.checkBox_aoi.BackColor = Color.Transparent;
            }

            // 互斥
            if (this.checkBox_aoi.Checked == true && this.checkBox_oi.Checked == true)
                this.checkBox_oi.Checked = false;
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

            if (this.checkBox_aoi.Checked == true
                && string.IsNullOrEmpty(this.textBox_rfid_aoi.Text) == false)
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

            if (this.checkBox_oi.Checked == false
                && this.checkBox_aoi.Checked == false
                && this.checkBox_writeUidLog.Checked == false
                && (this.comboBox_eas.Text == "不修改" || string.IsNullOrEmpty(this.comboBox_eas.Text) == true))
            {
                strError = "没有指定任何修改(或写日志)动作";
                goto ERROR1;
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

        /*
        public bool LinkUID
        {
            get
            {
                return this.checkBox_uidPiiMap.Checked;
            }
        }
        */

        public string ModifyEas
        {
            get
            {
                return this.comboBox_eas.Text;
            }
            set
            {
                this.comboBox_eas.Text = value;
            }
        }

        /*
图书
读者证
层架标
所有类别
        * */
        public string FilterTU
        {
            get
            {
                return this.comboBox_filter_tu.Text;
            }
        }

        // 是否要写入对照关系日志
        public bool WriteUidPiiLog
        {
            get
            {
                return this.checkBox_writeUidLog.Checked;
            }
            set
            {
                this.checkBox_writeUidLog.Checked = value;
            }
        }

        private void comboBox_eas_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_eas.Text)
                || this.comboBox_eas.Text == "不修改")
            {
                this.label_eas.BackColor = Color.Transparent;
            }
            else
            {
                this.label_eas.BackColor = Color.LightGreen;
            }
        }

        private void checkBox_writeUidLog_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_writeUidLog.Checked)
            {
                this.checkBox_writeUidLog.BackColor = Color.LightGreen;
            }
            else
            {
                this.checkBox_writeUidLog.BackColor = Color.Transparent;
            }
        }
    }
}
