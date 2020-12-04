using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                    if (DigitalPlatform.RFID.Compact.CheckIsil(this.textBox_rfid_oi.Text, false) == false)
                    {
                        strError = $"机构代码 '{this.textBox_rfid_oi.Text}' 中出现了非法字符";
                        goto ERROR1;
                    }

                }
            }

            DataModel.DefaultOiString = this.textBox_rfid_oi.Text;
            DataModel.DefaultAoiString = this.textBox_rfid_aoi.Text;

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
    }
}
