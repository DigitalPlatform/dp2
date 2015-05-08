using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CirculationClient
{
	/// <summary>
	/// Summary description for LoginDlg.
	/// </summary>
	public class CirculationLoginDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.CheckBox checkBox_savePasswordShort;
		public System.Windows.Forms.TextBox textBox_password;
		public System.Windows.Forms.TextBox textBox_userName;
		private System.Windows.Forms.Button button_cancel;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label_userName;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        private TextBox textBox_serverAddr;
		public System.Windows.Forms.TextBox textBox_comment;
        public TextBox textBox_location;
        private Label label4;
        private CheckBox checkBox_isReader;
        private CheckBox checkBox_savePasswordLong;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        const int WM_MOVE_FOCUS = API.WM_USER + 201;

        private MessageBalloon m_firstUseBalloon = null;

        public bool AutoShowShortSavePasswordTip = false;
        private ToolStrip toolStrip_server;
        private ToolStripButton toolStripButton_server_setXeServer;
        private ToolStripButton toolStripButton_server_setHongnibaServer;
        private ToolStripSeparator toolStripSeparator1;

        public bool SetDefaultMode = false; // 是否为 设置缺省帐户 状态？ 第一次进入程序时候是这个状态，其他登录失败后重新输入以便登录的时候不是这个状态

        public bool SupervisorMode = false; // 是否为 supervisor 模式。也就是管理员模式。在这个模式下， 无法修改 URL ，无法选择读者类型，不出现 红泥巴数字平台服务器按钮

		public CirculationLoginDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            // http://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
            this.toolStrip_server.Renderer = new MyRenderer(this.toolStrip_server);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CirculationLoginDlg));
            this.checkBox_savePasswordShort = new System.Windows.Forms.CheckBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label_userName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_isReader = new System.Windows.Forms.CheckBox();
            this.checkBox_savePasswordLong = new System.Windows.Forms.CheckBox();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_server_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_server_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_server.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_savePasswordShort
            // 
            this.checkBox_savePasswordShort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePasswordShort.AutoSize = true;
            this.checkBox_savePasswordShort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_savePasswordShort.Location = new System.Drawing.Point(120, 290);
            this.checkBox_savePasswordShort.Name = "checkBox_savePasswordShort";
            this.checkBox_savePasswordShort.Size = new System.Drawing.Size(111, 16);
            this.checkBox_savePasswordShort.TabIndex = 7;
            this.checkBox_savePasswordShort.Text = "短期保持密码(&S)";
            this.checkBox_savePasswordShort.CheckedChanged += new System.EventHandler(this.checkBox_savePasswordShort_CheckedChanged);
            this.checkBox_savePasswordShort.Click += new System.EventHandler(this.controls_Click);
            // 
            // textBox_password
            // 
            this.textBox_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_password.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_password.Location = new System.Drawing.Point(120, 266);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(156, 21);
            this.textBox_password.TabIndex = 6;
            this.textBox_password.Click += new System.EventHandler(this.controls_Click);
            this.textBox_password.TextChanged += new System.EventHandler(this.textBox_password_TextChanged);
            // 
            // textBox_userName
            // 
            this.textBox_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_userName.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_userName.Location = new System.Drawing.Point(120, 221);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(156, 21);
            this.textBox_userName.TabIndex = 3;
            this.textBox_userName.Click += new System.EventHandler(this.controls_Click);
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_serverAddr.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_serverAddr.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_serverAddr.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverAddr.Location = new System.Drawing.Point(12, 164);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(414, 21);
            this.textBox_serverAddr.TabIndex = 2;
            this.textBox_serverAddr.Click += new System.EventHandler(this.controls_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cancel.Location = new System.Drawing.Point(348, 340);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(78, 24);
            this.button_cancel.TabIndex = 11;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 268);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "密码(&P):";
            // 
            // label_userName
            // 
            this.label_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_userName.AutoSize = true;
            this.label_userName.Location = new System.Drawing.Point(10, 223);
            this.label_userName.Name = "label_userName";
            this.label_userName.Size = new System.Drawing.Size(65, 12);
            this.label_userName.TabIndex = 2;
            this.label_userName.Text = "用户名(&U):";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 149);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "图书馆应用服务器地址(&H):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(348, 310);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(78, 24);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBox_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_comment.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox_comment.Location = new System.Drawing.Point(12, 12);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(414, 126);
            this.textBox_comment.TabIndex = 0;
            this.textBox_comment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_location
            // 
            this.textBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_location.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_location.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_location.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_location.Location = new System.Drawing.Point(120, 312);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(156, 21);
            this.textBox_location.TabIndex = 9;
            this.textBox_location.Click += new System.EventHandler(this.controls_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 314);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "工作台号(&W):";
            // 
            // checkBox_isReader
            // 
            this.checkBox_isReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_isReader.AutoSize = true;
            this.checkBox_isReader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_isReader.Location = new System.Drawing.Point(120, 244);
            this.checkBox_isReader.Name = "checkBox_isReader";
            this.checkBox_isReader.Size = new System.Drawing.Size(63, 16);
            this.checkBox_isReader.TabIndex = 4;
            this.checkBox_isReader.Text = "读者(&R)";
            this.checkBox_isReader.UseVisualStyleBackColor = true;
            this.checkBox_isReader.CheckedChanged += new System.EventHandler(this.checkBox_isReader_CheckedChanged);
            this.checkBox_isReader.Click += new System.EventHandler(this.controls_Click);
            // 
            // checkBox_savePasswordLong
            // 
            this.checkBox_savePasswordLong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePasswordLong.AutoSize = true;
            this.checkBox_savePasswordLong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_savePasswordLong.Location = new System.Drawing.Point(12, 348);
            this.checkBox_savePasswordLong.Name = "checkBox_savePasswordLong";
            this.checkBox_savePasswordLong.Size = new System.Drawing.Size(111, 16);
            this.checkBox_savePasswordLong.TabIndex = 12;
            this.checkBox_savePasswordLong.Text = "长期保持密码(&L)";
            this.checkBox_savePasswordLong.UseVisualStyleBackColor = true;
            this.checkBox_savePasswordLong.CheckedChanged += new System.EventHandler(this.checkBox_savePasswordLong_CheckedChanged);
            this.checkBox_savePasswordLong.Click += new System.EventHandler(this.controls_Click);
            // 
            // toolStrip_server
            // 
            this.toolStrip_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_server.AutoSize = false;
            this.toolStrip_server.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_server.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_server_setXeServer,
            this.toolStripSeparator1,
            this.toolStripButton_server_setHongnibaServer});
            this.toolStrip_server.Location = new System.Drawing.Point(12, 184);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_server.Size = new System.Drawing.Size(414, 25);
            this.toolStrip_server.TabIndex = 26;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_server_setXeServer
            // 
            this.toolStripButton_server_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setXeServer.Name = "toolStripButton_server_setXeServer";
            this.toolStripButton_server_setXeServer.Size = new System.Drawing.Size(84, 22);
            this.toolStripButton_server_setXeServer.Text = "单机版服务器";
            this.toolStripButton_server_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_server_setXeServer.Click += new System.EventHandler(this.toolStripButton_server_setXeServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_server_setHongnibaServer
            // 
            this.toolStripButton_server_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setHongnibaServer.Name = "toolStripButton_server_setHongnibaServer";
            this.toolStripButton_server_setHongnibaServer.Size = new System.Drawing.Size(135, 22);
            this.toolStripButton_server_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_server_setHongnibaServer_Click);
            // 
            // CirculationLoginDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(438, 376);
            this.Controls.Add(this.checkBox_savePasswordLong);
            this.Controls.Add(this.checkBox_isReader);
            this.Controls.Add(this.textBox_location);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.checkBox_savePasswordShort);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.textBox_serverAddr);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_userName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.toolStrip_server);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CirculationLoginDlg";
            this.ShowInTaskbar = false;
            this.Text = "登录";
            this.Load += new System.EventHandler(this.LoginDlg_Load);
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_serverAddr.Text == "" 
				&& textBox_serverAddr.Enabled == true)
			{
				MessageBox.Show(this, "尚未输入服务器地址");
				return;
			}
			if (textBox_userName.Text == "")
			{
				MessageBox.Show(this, "尚未输入用户名");
				return;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void LoginDlg_Load(object sender, System.EventArgs e)
		{
			if (textBox_comment.Text == "")
				textBox_comment.Visible = false;

            if (this.SupervisorMode == true)
            {
                this.checkBox_isReader.Visible = false;
                this.toolStrip_server.Visible = false;
                this.textBox_serverAddr.ReadOnly = true;
            }

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

                    // 2008/7/1 new add
                    if (this.textBox_userName.Text == "")
                        this.textBox_userName.Focus();
                    else if (this.textBox_password.Text == "")
                        this.textBox_password.Focus();
                    else
                        this.button_OK.Focus();

                    if (AutoShowShortSavePasswordTip == true)
                        ShowShortSavePasswordTip();

                    return;
            }
            base.DefWndProc(ref m);
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

        public bool PasswordEnabled
        {
            get
            {
                return this.textBox_password.Enabled;
            }
            set
            {
                this.textBox_password.Enabled = value;
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

        public bool SavePasswordShort
        {
            get
            {
                return this.checkBox_savePasswordShort.Checked;
            }
            set
            {
                this.checkBox_savePasswordShort.Checked = value;
            }
        }

        public bool SavePasswordShortEnabled
        {
            get
            {
                return this.checkBox_savePasswordShort.Enabled;
            }
            set
            {
                this.checkBox_savePasswordShort.Enabled = value;
            }
        }

        public bool SavePasswordLong
        {
            get
            {
                return this.checkBox_savePasswordLong.Checked;
            }
            set
            {
                this.checkBox_savePasswordLong.Checked = value;
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

        public string OperLocation
        {
            get
            {
                return this.textBox_location.Text;
            }
            set
            {
                this.textBox_location.Text = value;
            }
        }

        public bool IsReader
        {
            get
            {
                return this.checkBox_isReader.Checked;
            }
            set
            {
                this.checkBox_isReader.Checked = value;
            }
        }

        public bool IsReaderEnabled
        {
            get
            {
                return this.checkBox_isReader.Enabled;
            }
            set
            {
                this.checkBox_isReader.Enabled = value;
            }
        }

        private void checkBox_savePasswordLong_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_savePasswordLong.Checked == true)
                this.checkBox_savePasswordShort.Checked = true;
        }

        public void ShowShortSavePasswordTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.checkBox_savePasswordShort;
            m_firstUseBalloon.Title = "小技巧";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "勾选 “短期保存密码” 项，可以让内务前端在运行期间记住您用过的密码，不再反复出现本对话框";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();

            this.checkBox_savePasswordShort.BackColor = SystemColors.Highlight;
            this.checkBox_savePasswordShort.ForeColor = SystemColors.HighlightText;
            this.checkBox_savePasswordShort.Padding = new Padding(6);

        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;

            this.checkBox_savePasswordShort.BackColor = SystemColors.Control;
            this.checkBox_savePasswordShort.ForeColor = SystemColors.ControlText;
            this.checkBox_savePasswordShort.Padding = new Padding(0);
        }

        private void controls_Click(object sender, EventArgs e)
        {
            HideMessageTip();
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            /*
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
             * */

            HideMessageTip();

            return base.ProcessDialogKey(keyData);
        }

        private void checkBox_savePasswordShort_CheckedChanged(object sender, EventArgs e)
        {
            // 在设置缺省帐户的情况下，如果去掉短期保持密码的选择，必须同时清除已经输入的密码字符，这样可以提醒用户，这样(unchecked)情况下，只能用空密码继续本对话框后面的操作
            if (this.SetDefaultMode == true)
            {
                if (this.checkBox_savePasswordShort.Checked == false)
                {
                    this.textBox_password.Text = "";
                }
            }
        }

        private void textBox_password_TextChanged(object sender, EventArgs e)
        {
            // 在设置缺省帐户的情况下，只要输入了密码字符，就表示要短期维持密码。否则容易造成后面自动用空密码试探登录(并可能会登录成功)，容易造成误会
            if (this.SetDefaultMode == true)
            {
                if (string.IsNullOrEmpty(this.textBox_password.Text) == false)
                    this.checkBox_savePasswordShort.Checked = true;
            }
        }

        private void toolStripButton_server_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_serverAddr.Text != ServerDlg.HnbUrl)
            {
                // this.textBox_serverName.Text = "红泥巴.数字平台服务器";
                this.textBox_serverAddr.Text = ServerDlg.HnbUrl;

                this.textBox_userName.Text = "";
                this.textBox_password.Text = "";
            }
        }

        public static string dp2LibraryXEServerUrl = "net.pipe://localhost/dp2library/xe";

        private void toolStripButton_server_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_serverAddr.Text != dp2LibraryXEServerUrl)
            {
                // this.textBox_serverName.Text = "单机版服务器";
                this.textBox_serverAddr.Text = dp2LibraryXEServerUrl;

                this.textBox_userName.Text = "supervisor";
                this.textBox_password.Text = "";
            }
        }

        private void checkBox_isReader_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_isReader.Checked == false)
            {
                this.label_userName.Text = "用户名(&U):";
                this.BackColor = SystemColors.ControlDark;
                this.ForeColor = SystemColors.ControlText;
                this.toolStrip_server.BackColor = SystemColors.ControlDark;
            }
            else
            {
                this.label_userName.Text = "读者证条码号(&B):";
                this.BackColor = Color.DarkGreen;
                this.ForeColor = Color.White;
                this.toolStrip_server.BackColor = this.BackColor;
            }
        }
	}

    class MyRenderer : ToolStripSystemRenderer
    {
        public ToolStrip ToolStrip = null;

        public MyRenderer(ToolStrip toolstrip)
        {
            this.ToolStrip = toolstrip;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(this.ToolStrip == null ? e.BackColor : this.ToolStrip.BackColor))  // SystemColors.ControlDark
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
            // base.OnRenderToolStripBackground(e);
        }

        // 去掉下面那根线
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
}
