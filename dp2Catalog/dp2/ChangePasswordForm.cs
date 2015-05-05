using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Catalog
{
    public partial class ChangePasswordForm : Form
    {
        public LibraryChannelCollection Channels = null;
        LibraryChannel Channel = null;

        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;

        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {
            this.Channels = new LibraryChannelCollection();
            this.Channels.BeforeLogin += new BeforeLoginEventHandle(Channels_BeforeLogin);
            this.Channels.AfterLogin += new AfterLoginEventHandle(Channels_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

        }

        void Channels_AfterLogin(object sender, AfterLoginEventArgs e)
        {

            LibraryChannel channel = (LibraryChannel)sender;

            dp2Server server = this.MainForm.Servers[channel.Url];
            if (server == null)
            {
                // e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                return;
            }

#if SN
            if (server.Verified == false && StringUtil.IsInList("serverlicensed", channel.Rights) == false)
            {
                string strError = "";
                string strTitle = "修改密码窗需要先设置序列号才能访问服务器 " + server.Name + " " + server.Url;
                int nRet = this.MainForm.VerifySerialCode(strTitle,
                    "",
                    true,
                    out strError);
                if (nRet == -1)
                {
                    channel.Close();
                    e.ErrorInfo = strTitle;
#if NO
                    MessageBox.Show(this.MainForm, "修改密码窗需要先设置序列号才能使用");
                    API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
#endif
                    return;
                }
            }
            server.Verified = true;
#else

            server.Verified = true;
#endif
        }

        void Channels_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            dp2Server server = this.MainForm.Servers[channel.Url];
            if (server == null)
            {
                e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                e.Failed = true;
                e.Cancel = true;
                return;
            }

            if (e.FirstTry == true)
            {
                e.UserName = server.DefaultUserName;
                e.Password = server.DefaultPassword;
                e.Parameters = "location=dp2Catalog,type=worker";
                /*
                e.IsReader = false;
                e.Location = "dp2Catalog";
                 * */

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = this.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = this;

            ServerDlg dlg = SetDefaultAccount(
                e.LibraryServerUrl,
                null,
                e.ErrorInfo,
                owner);
            if (dlg == null)
            {
                e.Cancel = true;
                return;
            }


            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = false;
            e.Parameters = "location=dp2Catalog,type=worker";

            /*
            e.IsReader = false;
            e.Location = "dp2Catalog";
             * */
            e.SavePasswordLong = true;
            e.LibraryServerUrl = dlg.ServerUrl;
        }

        ServerDlg SetDefaultAccount(
    string strServerUrl,
    string strTitle,
    string strComment,
    IWin32Window owner)
        {
            dp2Server server = this.MainForm.Servers[strServerUrl];

            ServerDlg dlg = new ServerDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            if (String.IsNullOrEmpty(strServerUrl) == true)
            {
            }
            else
            {
                dlg.ServerUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;


            if (String.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            dlg.Comment = strComment;
            dlg.UserName = server.DefaultUserName;

            this.MainForm.AppInfo.LinkFormState(dlg,
                "dp2_logindlg_state");

            dlg.ShowDialog(owner);

            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult == DialogResult.Cancel)
            {
                return null;
            }

            server.DefaultUserName = dlg.UserName;
            server.DefaultPassword =
                (dlg.SavePassword == true) ?
                dlg.Password : "";

            server.SavePassword = dlg.SavePassword;

            server.Url = dlg.ServerUrl;
            return dlg;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        // 图书馆服务器名
        public string LibraryServerName
        {
            get
            {
                return this.textBox_dp2library_serverName.Text;
            }
            set
            {
                this.textBox_dp2library_serverName.Text = value;
            }
        }


        private void ChangePasswordForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            this.Channels.BeforeLogin -= new BeforeLoginEventHandle(Channels_BeforeLogin);
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_dp2library_serverName.Enabled = bEnable;
            this.button_dp2library_findServerName.Enabled = bEnable;

            this.button_dp2library_changePassword.Enabled = bEnable;
            this.textBox_dp2library_userName.Enabled = bEnable;
            if (this.checkBox_dp2library_force.Checked == true)
                this.textBox_dp2library_oldPassword.Enabled = false;
            else
                this.textBox_dp2library_oldPassword.Enabled = bEnable;
            this.textBox_dp2library_newPassword.Enabled = bEnable;
            this.textBox_dp2library_confirmNewPassword.Enabled = bEnable;
            this.checkBox_dp2library_force.Enabled = bEnable;

        }

        private void button_dp2library_changePassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_dp2library_userName.Text == "")
            {
                MessageBox.Show(this, "尚未输入用户名。");
                this.textBox_dp2library_userName.Focus();
                return;
            }

            if (this.textBox_dp2library_newPassword.Text != this.textBox_dp2library_confirmNewPassword.Text)
            {
                MessageBox.Show(this, "新密码 和 确认新密码不一致。请重新输入。");
                this.textBox_dp2library_newPassword.Focus();
                return;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改 dp2library 用户密码 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.Update();
            this.MainForm.Update();


            try
            {
                long lRet = 0;

                // 获得server url
                if (String.IsNullOrEmpty(this.LibraryServerName) == true)
                {
                    strError = "尚未指定服务器名";
                    goto ERROR1;
                }
                dp2Server server = this.MainForm.Servers.GetServerByName(this.LibraryServerName);
                if (server == null)
                {
                    strError = "服务器名为 '" + this.LibraryServerName + "' 的服务器不存在...";
                    goto ERROR1;
                }

                string strServerUrl = server.Url;

                this.Channel = this.Channels.GetChannel(strServerUrl);


                // 非强制修改密码，即本人修改
                if (this.checkBox_dp2library_force.Checked == false)
                {

                    // return:
                    //      -1  error
                    //      0   登录未成功
                    //      1   登录成功
                    lRet = Channel.Login(this.textBox_dp2library_userName.Text,
                        this.textBox_dp2library_oldPassword.Text,
                        "location=dp2Catalog,type=worker",
                        /*
                        "",
                        false,
                         * */
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

                    try
                    {

                        lRet = Channel.ChangeUserPassword(
                            stop,
                            this.textBox_dp2library_userName.Text,
                            this.textBox_dp2library_oldPassword.Text,
                            this.textBox_dp2library_newPassword.Text,
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
                if (this.checkBox_dp2library_force.Checked == true)
                {
                    UserInfo info = new UserInfo();
                    info.UserName = this.textBox_dp2library_userName.Text;
                    info.Password = this.textBox_dp2library_newPassword.Text;
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

            MessageBox.Show(this, "dp2library 用户 '" + this.textBox_dp2library_userName.Text + "' 密码已经被成功修改。");

            this.textBox_dp2library_userName.SelectAll();
            this.textBox_dp2library_userName.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

            // 焦点重新定位到密码输入域
            this.textBox_dp2library_oldPassword.Focus();
            this.textBox_dp2library_oldPassword.SelectAll();
        }

        private void checkBox_dp2library_force_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_dp2library_force.Checked == true)
                this.textBox_dp2library_oldPassword.Enabled = false;
            else
                this.textBox_dp2library_oldPassword.Enabled = true;
        }

        private void ChangePasswordForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();
        }

        private void button_dp2library_findServerName_Click(object sender, EventArgs e)
        {
            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.dp2Channels = this.Channels;
            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            dlg.Path = this.textBox_dp2library_serverName.Text;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_dp2library_serverName.Text = dlg.Path;
        }
    }
}