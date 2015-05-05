using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Circulation
{
    internal partial class CardPasswordDialog : Form
    {
        const int WM_FIRST_FOCUS = API.WM_USER + 201;

        public CardPasswordDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_password.Text == "")
            {
                MessageBox.Show(this, "尚未输入密码");
                this.textBox_password.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
        }

        public string CardNumber
        {
            get
            {
                return this.textBox_cardNumber.Text;
            }
            set
            {
                this.textBox_cardNumber.Text = value;
            }
        }

        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        private void CardPasswordDialog_Load(object sender, EventArgs e)
        {
            API.PostMessage(this.Handle, WM_FIRST_FOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FIRST_FOCUS:
                    this.textBox_password.Focus();
                    return;
            }
            base.DefWndProc(ref m);
        }

    }
}