using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidTool
{
    public partial class SettingDialog : Form
    {
        public SettingDialog()
        {
            InitializeComponent();
        }

        private void SettingDialog_Load(object sender, EventArgs e)
        {
            this.textBox_rfid_oi.Text = DataModel.DefaultOiString;
            this.textBox_rfid_aoi.Text = DataModel.DefaultAoiString;

            this.comboBox_uhfDataFormat.Text = DataModel.UhfWriteFormat;
            this.checkBox_writeUserBank.Checked = DataModel.WriteUhfUserBank;
            this.checkBox_warningWhenUhfFormatMismatch.Checked = DataModel.WarningWhenUhfFormatMismatch;

            this.checkBox_enableTagCache.Checked = DataModel.EnableTagCache;
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

            // 检查
            {
                if (string.IsNullOrEmpty(this.textBox_rfid_oi.Text) == false
                    && string.IsNullOrEmpty(this.textBox_rfid_aoi.Text) == false)
                {
                    strError = "“机构代码”和“非标准机构代码”只能定义其中一个";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.textBox_rfid_oi.Text) == false)
                {
                    // 所属机构ISIL由拉丁字母、阿拉伯数字（0-9），分隔符（-/:)组成，总长度不超过16个字符。
                    if (DigitalPlatform.RFID.Compact.CheckIsil(this.textBox_rfid_oi.Text, false) == false)
                    {
                        strError = $"机构代码 '{this.textBox_rfid_oi.Text}' 中出现了非法字符";
                        goto ERROR1;
                    }

                    // 总长度不超过16个字符
                    if (this.textBox_rfid_oi.Text.Length > 16)
                    {
                        strError = $"机构代码 '{this.textBox_rfid_oi.Text}' 不合法，因其字符数超过了 16 个({this.textBox_rfid_oi.Text.Length})";
                        goto ERROR1;
                    }
                }

                if (string.IsNullOrEmpty(this.comboBox_uhfDataFormat.Text))
                {
                    strError = "尚未指定 UHF 标签写入格式";
                    goto ERROR1;
                }
            }

            DataModel.DefaultOiString = this.textBox_rfid_oi.Text;
            DataModel.DefaultAoiString = this.textBox_rfid_aoi.Text;

            DataModel.UhfWriteFormat = this.comboBox_uhfDataFormat.Text;
            DataModel.WriteUhfUserBank = this.checkBox_writeUserBank.Checked;
            DataModel.WarningWhenUhfFormatMismatch = this.checkBox_warningWhenUhfFormatMismatch.Checked;

            DataModel.EnableTagCache = this.checkBox_enableTagCache.Checked;

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

        private void linkLabel_oiHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://github.com/DigitalPlatform/dp2/issues/764#issuecomment-742960673";

            Process.Start(url);
        }
    }
}
