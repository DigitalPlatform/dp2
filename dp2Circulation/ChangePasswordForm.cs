using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 修改密码窗
    /// </summary>
    public partial class ChangePasswordForm : MyForm
    {
        const int WM_FIRST_SETFOCUS = API.WM_USER + 200;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);


            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            bool bReader = Program.MainForm.AppInfo.GetBoolean(
                "default_account",
                "isreader",
                false);
            if (bReader == false)
            {
                this.textBox_reader_oldPassword.Enabled = false;
            }
            else
            {
                this.textBox_reader_comment.Text = "这是读者为自己修改密码。";
                this.tabControl_main.Controls.Remove(this.tabPage_worker);
                this.AddFreeControl(this.tabPage_worker);   // 2015/11/7
            }

            checkBox_worker_force_CheckedChanged(this, null);

            API.PostMessage(this.Handle, WM_FIRST_SETFOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FIRST_SETFOCUS:
                    this.textBox_reader_barcode.Focus();
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void ChangePasswordForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_reader_changePassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_reader_barcode.Text == "")
            {
                MessageBox.Show(this, "尚未输入读者证条码号。");
                this.textBox_reader_barcode.Focus();
                return;
            }

            if (this.textBox_reader_newPassword.Text != this.textBox_reader_confirmNewPassword.Text)
            {
                MessageBox.Show(this, "新密码 和 确认新密码不一致。请重新输入。");
                this.textBox_reader_newPassword.Focus();
                return;
            }

            bool bOldPasswordEnabled = this.textBox_reader_oldPassword.Enabled;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改读者密码 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);

            try
            {
                long lRet = Channel.ChangeReaderPassword(
                    stop,
                    this.textBox_reader_barcode.Text,
                    bOldPasswordEnabled == false ? null : this.textBox_reader_oldPassword.Text,
                    this.textBox_reader_newPassword.Text,
                    out strError);
                if (lRet == 0)
                    goto ERROR1;
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "读者密码修改成功。");

            this.textBox_reader_barcode.SelectAll();
            this.textBox_reader_barcode.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

            // 焦点重新定位到密码输入域
            this.textBox_reader_oldPassword.Focus();
            this.textBox_reader_oldPassword.SelectAll();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.button_reader_changePassword.Enabled = bEnable;
            this.textBox_reader_barcode.Enabled = bEnable;
            this.textBox_reader_newPassword.Enabled = bEnable;
            this.textBox_reader_confirmNewPassword.Enabled = bEnable;

            bool bReader = Program.MainForm.AppInfo.GetBoolean(
    "default_account",
    "isreader",
    false);
            if (bReader == false)
                this.textBox_reader_oldPassword.Enabled = false;
            else
                this.textBox_reader_oldPassword.Enabled = bEnable;

            this.button_worker_changePassword.Enabled = bEnable;
            this.textBox_worker_userName.Enabled = bEnable;
            if (this.checkBox_worker_force.Checked == true)
                this.textBox_worker_oldPassword.Enabled = false;
            else
                this.textBox_worker_oldPassword.Enabled = bEnable;
            this.textBox_worker_newPassword.Enabled = bEnable;
            this.textBox_worker_confirmNewPassword.Enabled = bEnable;
            this.checkBox_worker_force.Enabled = bEnable;

        }

        private void button_worker_changePassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_worker_userName.Text == "")
            {
                MessageBox.Show(this, "尚未输入用户名。");
                this.textBox_worker_userName.Focus();
                return;
            }

            if (this.textBox_worker_newPassword.Text != this.textBox_worker_confirmNewPassword.Text)
            {
                MessageBox.Show(this, "新密码 和 确认新密码不一致。请重新输入。");
                this.textBox_worker_newPassword.Focus();
                return;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改工作人员密码 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);

            try
            {
                long lRet = 0;

                // 非强制修改密码，即本人修改
                if (this.checkBox_worker_force.Checked == false)
                {

                    if (this.textBox_worker_userName.Text != "!changeKernelPassword")
                    {
                        // return:
                        //      -1  error
                        //      0   登录未成功
                        //      1   登录成功
                        lRet = Channel.Login(this.textBox_worker_userName.Text,
                            this.textBox_worker_oldPassword.Text,
                            "type=worker,client=dp2circulation|" + Program.ClientVersion,
                            out strError);
                        if (lRet == -1)
                        {
                            goto ERROR1;
                        }

                        if (lRet == 0)
                        {
                            strError = "旧密码不正确";
                            goto ERROR1;
                        }
                    }

                    try
                    {

                        lRet = Channel.ChangeUserPassword(
                            stop,
                            this.textBox_worker_userName.Text,
                            this.textBox_worker_oldPassword.Text,
                            this.textBox_worker_newPassword.Text,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        string strError_1 = "";
                        Channel.Logout(out strError_1);
                    }
                }

                // 强制修改密码
                if (this.checkBox_worker_force.Checked == true)
                {
                    UserInfo info = new UserInfo();
                    info.UserName = this.textBox_worker_userName.Text;
                    info.Password = this.textBox_worker_newPassword.Text;
                    // 当action为"resetpassword"时，则info.ResetPassword状态不起作用，无论怎样都要修改密码。resetpassword并不修改其他信息，也就是说info中除了Password/UserName以外其他成员的值无效。
                    lRet = Channel.SetUser(
                        stop,
                        "resetpassword",
                        info,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                }


            }
            finally
            {
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "工作人员 '" + this.textBox_worker_userName.Text + "' 密码修改成功。");

            this.textBox_worker_userName.SelectAll();
            this.textBox_worker_userName.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

            // 焦点重新定位到密码输入域
            this.textBox_worker_oldPassword.Focus();
            this.textBox_worker_oldPassword.SelectAll();
        }

        private void checkBox_worker_force_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_worker_force.Checked == true)
                this.textBox_worker_oldPassword.Enabled = false;
            else
                this.textBox_worker_oldPassword.Enabled = true;
        }

        private void ChangePasswordForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_reader)
            {
                this.AcceptButton = this.button_reader_changePassword;
            }
            else
            {
                Debug.Assert(this.tabControl_main.SelectedTab == this.tabPage_worker, "");
                this.AcceptButton = this.button_worker_changePassword;
            }
        }
    }
}