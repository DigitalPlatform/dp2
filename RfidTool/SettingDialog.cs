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

using DigitalPlatform.Text;

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

            this.numericUpDown_seconds.Value = DataModel.BeforeScanSeconds;
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

                    strError = VerifyOI(this.textBox_rfid_oi.Text);
                    if (strError != null)
                        goto ERROR1;

                    /*
                    // 总长度不超过16个字符
                    if (this.textBox_rfid_oi.Text.Length > 16)
                    {
                        strError = $"机构代码 '{this.textBox_rfid_oi.Text}' 不合法，因其字符数超过了 16 个({this.textBox_rfid_oi.Text.Length})";
                        goto ERROR1;
                    }
                    */
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

            DataModel.BeforeScanSeconds = (int)this.numericUpDown_seconds.Value;

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

        /*
OI的校验，总长度不超过16位。
2位国家代码前缀-6位中国行政区划代码-1位图书馆类型代码-图书馆自定义码（最长4位）
         * */
        public static string VerifyOI(string oi)
        {
            if (string.IsNullOrEmpty(oi))
                return "机构代码不应为空";

            if (oi.Length > 16)
                return $"机构代码 '{oi}' 不合法: 总长度不应超过 16 字符";

            var parts = oi.Split(new char[] { '-' });
            if (parts.Length != 4)
                return $"机构代码 '{oi}' 不合法: 应为 - 间隔的四个部分形态";
            string country = parts[0];
            if (country != "CN")
                return $"机构代码 '{oi}' 不合法: 第一部分国家代码 '{country}' 不正确，应为 'CN'";
            string region = parts[1];
            if (region.Length != 6
                || StringUtil.IsPureNumber(region) == false)
                return $"机构代码 '{oi}' 不合法: 第二部分行政区代码 '{region}' 不正确，应为 6 位数字";
            string type = parts[2];
            if (type.Length != 1
    || VerifyType(type[0]) == false)
                return $"机构代码 '{oi}' 不合法: 第三部分图书馆类型代码 '{type}' 不正确，应为 1 位字符(取值范围为 1-9,A-F)";
            string custom = parts[3];
            if (custom.Length < 1 || custom.Length > 4
                || IsLetterOrDigit(custom) == false)
                return $"机构代码 '{oi}' 不合法: 第四部分图书馆自定义码 '{custom}' 不正确，应为 1-4 位数字或者大写字母";

            return null;
        }

        static bool VerifyType(char ch)
        {
            if (ch >= '1' && ch <= '9')
                return true;
            if (ch >= 'A' && ch <= 'F')
                return true;
            return false;
        }

        static bool IsLetterOrDigit(string text)
        {
            foreach(char ch in text)
            {
                if (char.IsLetter(ch) && char.IsUpper(ch) == false)
                    return false;
                if (char.IsLetterOrDigit(ch) == false)
                    return false;
            }

            return true;
        }
    }
}
