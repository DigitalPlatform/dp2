using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 为了确认超级用户身份而登录的对话框
    /// </summary>
    public partial class ConfirmSupervisorDialog : Form
    {
        const int WM_MOVE_FOCUS = API.WM_USER + 201;

        public ConfirmSupervisorDialog()
        {
            InitializeComponent();
        }

        private void ConfirmSupervisorDialog_Load(object sender, EventArgs e)
        {

            API.PostMessage(this.Handle, WM_MOVE_FOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOVE_FOCUS:
                    // 2008/7/1
                    if (this.textBox_userName.Text == "")
                        this.textBox_userName.Focus();
                    else if (this.textBox_password.Text == "")
                        this.textBox_password.Focus();
                    else
                        this.button_OK.Focus();
                    return;
            }
            base.DefWndProc(ref m);
        }


        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_userName.Text == "")
            {
                MessageBox.Show(this, "尚未指定用户名");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string UserName
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
            set
            {
                this.textBox_password.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_serverAddr.Text;
            }
            set
            {
                this.textBox_serverAddr.Text = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }
    }
}