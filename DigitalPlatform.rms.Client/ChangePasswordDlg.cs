using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;

using DigitalPlatform.GUI;


namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// Summary description for ChangePasswordDlg.
    /// </summary>
    public class ChangePasswordDlg : System.Windows.Forms.Form
    {
        public string Url = "";
        public string UserName = "";

        RmsChannel channel = null;	// 临时使用的channel对象

        public AutoResetEvent eventClose = new AutoResetEvent(false);

        public RmsChannelCollection Channels = null;

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();
        DigitalPlatform.Stop stop = null;

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        public System.Windows.Forms.TextBox textBox_oldPassword;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox_newPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.TextBox textBox_url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.TextBox textBox_newPasswordConfirm;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.CheckBox checkBox_manager;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ChangePasswordDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.channel != null)
                    this.channel.Close();
                if (this.Channels != null)
                    this.Channels.Dispose();
                this.eventClose.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePasswordDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_oldPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_newPasswordConfirm = new System.Windows.Forms.TextBox();
            this.textBox_newPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_url = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label_message = new System.Windows.Forms.Label();
            this.checkBox_manager = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(443, 269);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 23);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(382, 269);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(55, 23);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_oldPassword
            // 
            this.textBox_oldPassword.Location = new System.Drawing.Point(112, 88);
            this.textBox_oldPassword.Name = "textBox_oldPassword";
            this.textBox_oldPassword.PasswordChar = '*';
            this.textBox_oldPassword.Size = new System.Drawing.Size(222, 21);
            this.textBox_oldPassword.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "旧密码(&O):";
            // 
            // textBox_newPasswordConfirm
            // 
            this.textBox_newPasswordConfirm.Location = new System.Drawing.Point(100, 53);
            this.textBox_newPasswordConfirm.Name = "textBox_newPasswordConfirm";
            this.textBox_newPasswordConfirm.PasswordChar = '*';
            this.textBox_newPasswordConfirm.Size = new System.Drawing.Size(222, 21);
            this.textBox_newPasswordConfirm.TabIndex = 3;
            // 
            // textBox_newPassword
            // 
            this.textBox_newPassword.Location = new System.Drawing.Point(100, 26);
            this.textBox_newPassword.Name = "textBox_newPassword";
            this.textBox_newPassword.PasswordChar = '*';
            this.textBox_newPassword.Size = new System.Drawing.Size(222, 21);
            this.textBox_newPassword.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "确认密码(&C):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "新密码(&N):";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_newPasswordConfirm);
            this.groupBox1.Controls.Add(this.textBox_newPassword);
            this.groupBox1.Location = new System.Drawing.Point(12, 136);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 87);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 新密码 ";
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(112, 37);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(150, 21);
            this.textBox_userName.TabIndex = 3;
            // 
            // textBox_url
            // 
            this.textBox_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_url.Location = new System.Drawing.Point(112, 10);
            this.textBox_url.Name = "textBox_url";
            this.textBox_url.Size = new System.Drawing.Size(387, 21);
            this.textBox_url.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名(&U):";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器URL:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 64);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(360, 62);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 旧密码 ";
            // 
            // label_message
            // 
            this.label_message.Location = new System.Drawing.Point(12, 232);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(360, 23);
            this.label_message.TabIndex = 7;
            // 
            // checkBox_manager
            // 
            this.checkBox_manager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_manager.AutoSize = true;
            this.checkBox_manager.Location = new System.Drawing.Point(12, 273);
            this.checkBox_manager.Name = "checkBox_manager";
            this.checkBox_manager.Size = new System.Drawing.Size(174, 16);
            this.checkBox_manager.TabIndex = 8;
            this.checkBox_manager.Text = "以管理身份修改用户密码(&M)";
            this.checkBox_manager.CheckedChanged += new System.EventHandler(this.checkBox_manager_CheckedChanged);
            // 
            // ChangePasswordDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(511, 305);
            this.Controls.Add(this.checkBox_manager);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_oldPassword);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.textBox_url);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangePasswordDlg";
            this.ShowInTaskbar = false;
            this.Text = "修改密码";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ChangePasswordDlg_Closing);
            this.Closed += new System.EventHandler(this.ChangePasswordDlg_Closed);
            this.Load += new System.EventHandler(this.ChangePasswordDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void ChangePasswordDlg_Load(object sender, System.EventArgs e)
        {
            textBox_url.Text = Url;
            textBox_userName.Text = UserName;
            stopManager.Initial(button_Cancel,
                label_message,
                null);
            stopManager.LinkReverseButton(button_OK);

            stop = new DigitalPlatform.Stop();
            stop.Register(this.stopManager, true);	// 和容器关联
        }

        private void ChangePasswordDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (channel == null)
            {
            }
            else
            {
                channel.Abort();
                e.Cancel = true;
            }

        }

        private void ChangePasswordDlg_Closed(object sender, System.EventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (textBox_url.Text == "")
            {
                MessageBox.Show(this, "尚未指定服务器URL...");
                return;
            }

            if (textBox_newPassword.Text != textBox_newPasswordConfirm.Text)
            {
                MessageBox.Show(this, "新密码和确认密码不一致，请重新输入...");
                return;
            }

            if (textBox_userName.Text == "")
            {
                MessageBox.Show(this, "尚未输入用户名。");
                return;
            }

            channel = Channels.GetChannel(textBox_url.Text);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改密码...");

            stop.BeginLoop();

            int nRet;
            string strError;

            EnableControls(false);
            button_Cancel.Text = "中断";


            nRet = channel.ChangePassword(
                textBox_userName.Text,
                textBox_oldPassword.Text,
                textBox_newPassword.Text,
                checkBox_manager.Checked,
                out strError);

            EnableControls(true);

            stop.EndLoop();

            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            button_Cancel.Enabled = true;	// 因为Cancel按钮还有退出对话框的功能
            button_Cancel.Text = "取消";

            if (nRet == -1)
                goto ERROR1;

            channel = null;

            MessageBox.Show(this, "密码修改成功。");

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;

        ERROR1:
            button_Cancel.Enabled = true;
            button_Cancel.Text = "取消";

            channel = null;
            MessageBox.Show(this, "修改密码失败，原因：" + strError);
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            if (channel == null)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                channel.Abort();
            }

        }

        void EnableControls(bool bEnable)
        {
            textBox_url.Enabled = bEnable;
            textBox_userName.Enabled = bEnable;
            textBox_oldPassword.Enabled = bEnable;
            textBox_newPassword.Enabled = bEnable;
            textBox_newPasswordConfirm.Enabled = bEnable;

            // button_OK.Enabled = bEnable;
            // button_Cancel.Enabled = bEnable == true ? false : true;
        }

        private void checkBox_manager_CheckedChanged(object sender, System.EventArgs e)
        {
            if (checkBox_manager.Checked == true)
            {
                textBox_oldPassword.Text = "";
                textBox_oldPassword.Enabled = false;
            }
            else
            {
                textBox_oldPassword.Enabled = true;
            }


        }


    }
}
