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
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;

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
            this.UseLooping = true; // 2022/11/1

            InitializeComponent();
        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            if (control)
            {
                this.label1_resetPatronPassword_queryword.Visible = true;
                this.textBox_resetPatronPassword_queryWord.Visible = true;
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
            long lRet = 0;

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

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在修改读者密码 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在修改读者密码 ...",
                "disableControl");

            try
            {
                // Result.Value
                //      -1  出错
                //      0   旧密码不正确
                //      1   旧密码正确,已修改为新密码
                lRet = channel.ChangeReaderPassword(
                    looping.Progress,
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
                looping.Dispose();
                /*
                this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
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
            this.TryInvoke((Action)(() =>
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
            }));
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

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在修改工作人员密码 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在修改工作人员密码 ...",
                "disableControl");

            try
            {
                long lRet = 0;

                // 非强制修改密码，即本人修改
                if (this.checkBox_worker_force.Checked == false)
                {
                    // dp2library 3.54 以前要求先 Login() 成功才能使用 ChangeUserPassword() 修改 dp2library 账户密码
                    if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.54") < 0)
                    {
                        if (this.textBox_worker_userName.Text != "!changeKernelPassword")
                        {
                            // return:
                            //      -1  error
                            //      0   登录未成功
                            //      1   登录成功
                            lRet = channel.Login(this.textBox_worker_userName.Text,
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
                    }

                    try
                    {
                        // return.Value:
                        //      -1  出错
                        //      0   成功
                        lRet = channel.ChangeUserPassword(
                            looping.Progress,
                            this.textBox_worker_userName.Text,
                            this.textBox_worker_oldPassword.Text,
                            this.textBox_worker_newPassword.Text,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        channel.Logout(out _);
                    }
                }

                // 强制修改密码
                if (this.checkBox_worker_force.Checked == true)
                {
                    UserInfo info = new UserInfo();
                    info.UserName = this.textBox_worker_userName.Text;
                    info.Password = this.textBox_worker_newPassword.Text;
                    // 当action为"resetpassword"时，则info.ResetPassword状态不起作用，无论怎样都要修改密码。resetpassword并不修改其他信息，也就是说info中除了Password/UserName以外其他成员的值无效。
                    lRet = channel.SetUser(
                        looping.Progress,
                        "resetpassword",
                        info,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
            }
            finally
            {
                looping.Dispose();
                /*
                this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
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
            else if (this.tabControl_main.SelectedTab == this.tabPage_worker)
            {
                Debug.Assert(this.tabControl_main.SelectedTab == this.tabPage_worker, "");
                this.AcceptButton = this.button_worker_changePassword;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_resetPatronPassword)
            {
                this.AcceptButton = this.button_resetPatronPassword;
            }
        }

        // 获得临时密码，发送到读者手机
        private void button_resetPatronPassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_name.Text) == true)
            {
                strError = "请输入读者姓名";
                goto ERROR1;
            }

            // 注：当 queryword= 中有内容时，不要求必须输入 barcode
            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_barcode.Text) == true
                && string.IsNullOrEmpty(this.textBox_resetPatronPassword_queryWord.Text))
            {
                strError = "请输入读者证条码号";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_phoneNumber.Text) == true)
            {
                strError = "请输入读者手机号码";
                goto ERROR1;
            }

            if (this.textBox_resetPatronPassword_phoneNumber.Text.Length != 11)
            {
                strError = "手机号码格式不正确。应该为 11 位数字";
                goto ERROR1;
            }

            string strParameters = "name=" + GetParamValue(this.textBox_resetPatronPassword_name.Text)
                + ",tel=" + GetParamValue(this.textBox_resetPatronPassword_phoneNumber.Text)
                + ",barcode=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text);

            // 2021/10/20
            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_queryWord.Text))
                strParameters += ",queryword=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text);
            else
                strParameters += ",queryword=" + GetParamValue(this.textBox_resetPatronPassword_queryWord.Text);

            /*
            LibraryChannel channel = this.Channel;
            */
            var looping = Looping(out LibraryChannel channel,
                "正在重设密码 ...",
                "disableControl");
            try
            {
                long lRet = channel.ResetPassword(
                    looping.Progress,
                    strParameters,
                    "",
                    out string strMessage,
                    out strError);
                if (lRet != 1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(strError) == true)
                    strError = "临时密码已通过短信方式发送到手机 " + this.textBox_resetPatronPassword_phoneNumber.Text + "。请按照收到的手机短信提示进行后续操作";

                MessageBox.Show(this, strError);
                return;
            }
            finally
            {
                looping.Dispose();
                // sessioninfo.ReturnChannel(channel);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得临时密码，显示到本界面
        private void button_resetPatronPassword_displayTempPassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_name.Text) == true)
            {
                strError = "请输入读者姓名";
                goto ERROR1;
            }

            // 注：当 queryword= 中有内容时，不要求必须输入 barcode
            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_barcode.Text) == true
                && string.IsNullOrEmpty(this.textBox_resetPatronPassword_queryWord.Text))
            {
                strError = "请输入读者证条码号";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_phoneNumber.Text) == true)
            {
                strError = "请输入读者手机号码";
                goto ERROR1;
            }

            if (this.textBox_resetPatronPassword_phoneNumber.Text.Length != 11)
            {
                strError = "手机号码格式不正确。应该为 11 位数字";
                goto ERROR1;
            }

            string strParameters = "name=" + GetParamValue(this.textBox_resetPatronPassword_name.Text)
    + ",tel=" + GetParamValue(this.textBox_resetPatronPassword_phoneNumber.Text)
    + ",barcode=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text);

            // 2021/10/20
            if (string.IsNullOrEmpty(this.textBox_resetPatronPassword_queryWord.Text))
                strParameters += ",queryword=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text);
            else
                strParameters += ",queryword=" + GetParamValue(this.textBox_resetPatronPassword_queryWord.Text);

            strParameters += ",style=returnMessage";

            /*
            string strParameters = "name=" + GetParamValue(this.textBox_resetPatronPassword_name.Text)
                + ",tel=" + GetParamValue(this.textBox_resetPatronPassword_phoneNumber.Text)
                + ",barcode=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text)
                + ",queryword=" + GetParamValue(this.textBox_resetPatronPassword_barcode.Text)
                + ",style=returnMessage";
            */

            /*
            LibraryChannel channel = this.Channel;
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得临时密码 ...",
                "disableControl");
            try
            {
                long lRet = channel.ResetPassword(
                    looping.Progress,
                    strParameters,
                    "",
                    out string strMessage,
                    out strError);
                if (lRet != 1)
                    goto ERROR1;

                MessageDlg.Show(this, $"{strMessage}\r\n\r\n{strError}", "临时密码");
                return;
            }
            finally
            {
                looping.Dispose();
                // sessioninfo.ReturnChannel(channel);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 正规化参数值
        static string GetParamValue(string strText)
        {
            return strText.Replace("=", "").Replace(",", "");
        }
    }
}