using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform.RFID.UI
{
    /// <summary>
    /// 复制创建 UHF 标签 对话框
    /// </summary>
    public partial class DuplicateUhfTagDialog : Form
    {
        public DuplicateUhfTagDialog()
        {
            InitializeComponent();
        }

        private void DuplicateUhfTagDialog_Load(object sender, EventArgs e)
        {

        }

        private void DuplicateUhfTagDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // TODO: 弹出对话框确认写入

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string EpcBankHex
        {
            get
            {
                return this.textBox_epcBankHex.Text;
            }
            set
            {
                this.textBox_epcBankHex.Text = value;
            }
        }

        public string UserBankHex
        {
            get
            {
                return this.textBox_userBankHex.Text;
            }
            set
            {
                this.textBox_userBankHex.Text = value;
            }
        }

        public bool OkButtonVisible
        {
            get
            {
                return this.button_OK.Visible;
            }
            set
            {
                this.button_OK.Visible = value;
            }
        }

        public string OkButtonText
        {
            get
            {
                return this.button_OK.Text;
            }
            set
            {
                this.button_OK.Text = value;
            }
        }

        private void textBox_userBankHex_TextChanged(object sender, EventArgs e)
        {
            this.label_length.Text = $"User Bank 字节数: {this.textBox_userBankHex.Text.Length / 2}";
        }
    }
}
