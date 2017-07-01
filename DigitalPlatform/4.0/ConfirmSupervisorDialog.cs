using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
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

                    SetFocus();
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

        // 用于接收验证短信的手机号码
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

        bool _retryLogin = false;
        public bool RetryLogin
        {
            get
            {
                return _retryLogin;
            }
            set
            {
                _retryLogin = value;
                TempCodeVisible = value;
            }
        }

        // string _tempCode = "";

        // 验证码
        public string TempCode
        {
            get
            {
                return this.textBox_tempCode.Text;
            }
            set
            {
                this.textBox_tempCode.Text = value;
            }
        }

        bool _tempCodeVisible = false;
        public bool TempCodeVisible
        {
            get
            {
                return this._tempCodeVisible;
            }
            set
            {
                this._tempCodeVisible = value;
                this.label_tempCode.Visible = value;
                this.textBox_tempCode.Visible = value;
            }
        }

        bool _phoneNumberActivated = false;
        public void ActivatePhoneNumber()
        {
            int nOldWidth = this.textBox_phoneNumber.Width;
            int nOldHeight = this.textBox_phoneNumber.Height;

            // 将字体放大一倍
            this.textBox_phoneNumber.Font = new Font(this.Font.FontFamily,
                this.Font.Size * 2);
            this.label_phoneNumber.Font = this.textBox_phoneNumber.Font;

            int nHeightDelta = this.textBox_phoneNumber.Height - nOldHeight;

            // 保持原有的宽度
            this.textBox_phoneNumber.Width = nOldWidth;

            _phoneNumberActivated = true;
            _tempCodeActivated = false;
        }

        bool _tempCodeActivated = false;
        public void ActivateTempCode()
        {
            int nOldWidth = this.textBox_tempCode.Width;
            int nOldHeight = this.textBox_tempCode.Height;

            // 将字体放大一倍
            this.textBox_tempCode.Font = new Font(this.Font.FontFamily,
                this.Font.Size * 2);
            this.label_tempCode.Font = this.textBox_tempCode.Font;

            int nHeightDelta = this.textBox_tempCode.Height - nOldHeight;

            // 保持原有的宽度
            this.textBox_tempCode.Width = nOldWidth;

            _tempCodeActivated = true;
            _phoneNumberActivated = false;

            // 将字体还原
            this.textBox_phoneNumber.Font = new Font(this.Font.FontFamily,
                this.Font.Size);
            this.label_phoneNumber.Font = this.textBox_phoneNumber.Font;
        }

        void SetFocus()
        {
            if (_phoneNumberActivated)
                this.textBox_phoneNumber.Focus();
            else if (_tempCodeActivated)
                this.textBox_tempCode.Focus();
        }
    }
}