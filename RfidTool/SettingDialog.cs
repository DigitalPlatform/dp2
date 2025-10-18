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
using DigitalPlatform;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Text;

namespace RfidTool
{
    public partial class SettingDialog : Form
    {
        public string OpenStyle { get; set; }

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

            this.checkBox_writeTag_verifyPii.Checked = DataModel.VerifyPiiWhenWriteTag;
            this.checkBox_writeTag_useLocalStoreage.Checked = DataModel.UseLocalStoreage;
            this.checkBox_writeTag_errorContentAsBlank.Checked = DataModel.ErrorContentAsBlank;

            this.checkBox_enableTagCache.Checked = DataModel.EnableTagCache;

            this.numericUpDown_seconds.Value = DataModel.BeforeScanSeconds;

            this.textBox_verifyRule.Text = DataModel.PiiVerifyRule;

            this.textBox_gaoxiaoParameters.Text = DataModel.GaoxiaoParameters;

            if (StringUtil.IsInList("activateVerifyRule", this.OpenStyle) == true)
            {
                this.tabControl1.SelectedTab = this.tabPage_other;
            }
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

            // 检查机构代码
            if (this.checkBox_writeTag_useLocalStoreage.Checked == false)
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

                // 2021/4/29
                if (this.textBox_rfid_aoi.Text.Contains(" "))
                {
                    strError = $"非标准机构代码 '{textBox_rfid_aoi.Text}' 不合法：出现了空格字符";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.textBox_rfid_aoi.Text) == false)
                {
                    // TODO: 当 textbox 内容发生过变化才警告
                    this.tabControl1.SelectedTab = this.tabPage_writeTag;
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
            }

            {
                if (string.IsNullOrEmpty(this.comboBox_uhfDataFormat.Text))
                {
                    strError = "尚未指定 UHF 标签写入格式";
                    goto ERROR1;
                }

                string rule = this.textBox_verifyRule.Text;
                if (string.IsNullOrEmpty(rule) == false)
                {
                    try
                    {
                        BarcodeValidator validator = new BarcodeValidator(rule);
                    }
                    catch (Exception ex)
                    {
                        strError = $"条码校验规则不合法: {ex.Message}";
                        goto ERROR1;
                    }
                }

                // 2021/5/12
                if (this.checkBox_writeTag_verifyPii.Checked
                    && string.IsNullOrWhiteSpace(rule))
                {
                    strError = "您选择了“校验条码号”，但尚未设置条码号校验规则";
                    goto ERROR1;
                }
            }

            // 2025/9/30
            // 检查 content parameter bytes
            {
                var text = this.textBox_gaoxiaoParameters.Text;
                if (string.IsNullOrEmpty(text) == false)
                {
                    var errors = VerifyGaoxiaoParameters(text);
                    if (errors.Count > 0)
                    {
                        strError = $"高校联盟固定值 '{text}' 格式不合法: \r\n{StringUtil.MakePathList(errors, "\r\n")}";
                        goto ERROR1;
                    }
                }
            }

            DataModel.DefaultOiString = this.textBox_rfid_oi.Text;
            DataModel.DefaultAoiString = this.textBox_rfid_aoi.Text;

            DataModel.UhfWriteFormat = this.comboBox_uhfDataFormat.Text;
            DataModel.WriteUhfUserBank = this.checkBox_writeUserBank.Checked;
            DataModel.WarningWhenUhfFormatMismatch = this.checkBox_warningWhenUhfFormatMismatch.Checked;

            DataModel.VerifyPiiWhenWriteTag = this.checkBox_writeTag_verifyPii.Checked;
            DataModel.UseLocalStoreage = this.checkBox_writeTag_useLocalStoreage.Checked;
            DataModel.ErrorContentAsBlank = this.checkBox_writeTag_errorContentAsBlank.Checked;

            DataModel.EnableTagCache = this.checkBox_enableTagCache.Checked;

            DataModel.BeforeScanSeconds = (int)this.numericUpDown_seconds.Value;

            DataModel.PiiVerifyRule = this.textBox_verifyRule.Text;

            DataModel.GaoxiaoParameters = this.textBox_gaoxiaoParameters.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
                    var epc_info = new GaoxiaoEpcInfo
                    {
                        Version = 4,
                        Lending = false,
                        Picking = 0,
                        Reserve = 0,
                        OverwriteContentParameterBytes = ByteArray.GetTimeStampByteArray(DataModel.GaoxiaoParameters),
                    };
                    // 预留位
            first |= (byte)((info.Reserve & 0x03) << 4);

            // 分拣信息
            first |= (byte)(info.Picking & 0xf);
        * */
        // cphex:0053,version:4,picking:0,reserve:0
        static List<string> VerifyGaoxiaoParameters(string value)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(value) == false)
            {
                if (value.Contains(":") == false)
                    errors.Add($"高校联盟固定值参数 '{value}' 不合法。合法格式样例 cphex:0053,version:4,picking:0,reserve:0");
                if (value.Contains("=") == true)
                    errors.Add($"高校联盟固定值参数 '{value}' 不合法。不应包含字符 '='。合法格式样例 cphex:0053,version:4,picking:0,reserve:0");
                if (value.Contains(";") == true)
                    errors.Add($"高校联盟固定值参数 '{value}' 不合法。不应包含字符 ';'。合法格式样例 cphex:0053,version:4,picking:0,reserve:0");
            }


            {
                var cp = StringUtil.GetParameterByPrefix(value, "cphex");
                if (string.IsNullOrEmpty(cp) == false)
                {
                    if (cp.Length != 4)
                    {
                        errors.Add($"cphex 参数值 '{cp}' 格式不合法: 应为 4 个十六进制字符(当前长度 {cp.Length} 个)");
                    }
                    else
                    {
                        byte[] bytes = null;
                        try
                        {
                            bytes = ByteArray.GetTimeStampByteArray(cp);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"cphex 参数值 '{cp}' 格式不合法: {ex.Message}");
                        }
                        if (bytes.Length != 2)
                        {
                            errors.Add($"cphex 参数值 '{cp}' 格式不合法: 最多只能包含 2 个字节(当前有 {bytes.Length} 个)");
                        }
                    }
                }
            }

            {
                var version = StringUtil.GetParameterByPrefix(value, "version");
                if (string.IsNullOrEmpty(version) == false)
                {
                    if (byte.TryParse(version, out byte version_value) == false)
                    {
                        errors.Add($"version 参数值 '{version}' 格式不合法: 应为 1-64 之间的整数");
                    }

                    else if (version_value <= 0 || version_value > 65)
                    {
                        errors.Add($"version 参数值 '{version}' 格式不合法: 应为 1-64 之间的整数");
                    }
                }
            }

            {
                var picking = StringUtil.GetParameterByPrefix(value, "picking");
                if (string.IsNullOrEmpty(picking) == false)
                {
                    if (byte.TryParse(picking, out byte picking_value) == false)
                    {
                        errors.Add($"picking 参数值 '{picking}' 格式不合法: 应为 0-15 之间的整数");
                    }

                    else if (picking_value < 0 || picking_value > 15)
                    {
                        errors.Add($"picking 参数值 '{picking}' 格式不合法: 应为 0-15 之间的整数");
                    }
                }
            }

            {
                var reserve = StringUtil.GetParameterByPrefix(value, "reserve");
                if (string.IsNullOrEmpty(reserve) == false)
                {
                    if (byte.TryParse(reserve, out byte reserve_value) == false)
                    {
                        errors.Add($"reserve 参数值 '{reserve}' 格式不合法: 应为 0-3 之间的整数");
                    }

                    else if (reserve_value < 0 || reserve_value > 3)
                    {
                        errors.Add($"reserve 参数值 '{reserve}' 格式不合法: 应为 0-3 之间的整数");
                    }
                }
            }

            return errors;
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
            foreach (char ch in text)
            {
                if (char.IsLetter(ch) && char.IsUpper(ch) == false)
                    return false;
                if (char.IsLetterOrDigit(ch) == false)
                    return false;
            }

            return true;
        }

        private void checkBox_changeAOI_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_rfid_aoi.Enabled = this.checkBox_changeAOI.Checked;
        }

        private void textBox_rfid_oi_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.textBox_rfid_aoi.Text = "";
        }

        /*
高校联盟格式
国标格式         * 
         * */
        private void comboBox_uhfDataFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_uhfDataFormat.Text == "高校联盟格式")
            {
                this.textBox_gaoxiaoParameters.Visible = true;
                this.label_gaoxiaoParameters.Visible = true;
            }
            else
            {
                this.textBox_gaoxiaoParameters.Visible = false;
                this.label_gaoxiaoParameters.Visible = false;
            }
        }
    }
}
