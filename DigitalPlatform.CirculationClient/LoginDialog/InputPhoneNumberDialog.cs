using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 输入手机号码的对话框
    /// </summary>
    public partial class InputPhoneNumberDialog : Form
    {
        public InputPhoneNumberDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_phoneNumber.Text))
            {
                strError = "请输入电话号码";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string PhoneNumber
        {
            get
            {
                return this.textBox_phoneNumber.Text;
            }
            set
            {
                this.textBox_phoneNumber.Text = value;
            }
        }
    }
}
