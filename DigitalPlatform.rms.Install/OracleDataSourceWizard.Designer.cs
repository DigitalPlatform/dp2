namespace DigitalPlatform.rms
{
    partial class OracleDataSourceWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OracleDataSourceWizard));
            this.button_finish = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.button_prev = new System.Windows.Forms.Button();
            this.tabControl_main = new DigitalPlatform.WizardPages();
            this.tabPage_welcome = new System.Windows.Forms.TabPage();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.tabPage_sqlServerName = new System.Windows.Forms.TabPage();
            this.tabControl_serverName = new System.Windows.Forms.TabControl();
            this.tabPage_parameters = new System.Windows.Forms.TabPage();
            this.button_getServiceName = new System.Windows.Forms.Button();
            this.textBox_serviceName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_hostName = new System.Windows.Forms.TextBox();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage_template = new System.Windows.Forms.TabPage();
            this.textBox_serverNameTemplate = new System.Windows.Forms.TextBox();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_createLogin = new System.Windows.Forms.TabPage();
            this.button_copySqlServerInfo = new System.Windows.Forms.Button();
            this.groupBox_login = new System.Windows.Forms.GroupBox();
            this.textBox_tableSpaceName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_tableSpaceFile = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_confirmLoginPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loginPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_loginName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_welcome.SuspendLayout();
            this.tabPage_sqlServerName.SuspendLayout();
            this.tabControl_serverName.SuspendLayout();
            this.tabPage_parameters.SuspendLayout();
            this.tabPage_template.SuspendLayout();
            this.tabPage_createLogin.SuspendLayout();
            this.groupBox_login.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_finish
            // 
            this.button_finish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_finish.Location = new System.Drawing.Point(356, 312);
            this.button_finish.Name = "button_finish";
            this.button_finish.Size = new System.Drawing.Size(75, 23);
            this.button_finish.TabIndex = 10;
            this.button_finish.Text = "完成";
            this.button_finish.UseVisualStyleBackColor = true;
            this.button_finish.Click += new System.EventHandler(this.button_finish_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(250, 312);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(75, 23);
            this.button_next.TabIndex = 9;
            this.button_next.Text = "下一步";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_prev
            // 
            this.button_prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_prev.Location = new System.Drawing.Point(169, 312);
            this.button_prev.Name = "button_prev";
            this.button_prev.Size = new System.Drawing.Size(75, 23);
            this.button_prev.TabIndex = 8;
            this.button_prev.Text = "上一步";
            this.button_prev.UseVisualStyleBackColor = true;
            this.button_prev.Click += new System.EventHandler(this.button_prev_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_welcome);
            this.tabControl_main.Controls.Add(this.tabPage_sqlServerName);
            this.tabControl_main.Controls.Add(this.tabPage_createLogin);
            this.tabControl_main.Location = new System.Drawing.Point(1, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(430, 294);
            this.tabControl_main.TabIndex = 7;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_welcome
            // 
            this.tabPage_welcome.Controls.Add(this.textBox_message);
            this.tabPage_welcome.Location = new System.Drawing.Point(4, 22);
            this.tabPage_welcome.Name = "tabPage_welcome";
            this.tabPage_welcome.Size = new System.Drawing.Size(422, 268);
            this.tabPage_welcome.TabIndex = 2;
            this.tabPage_welcome.Text = "说明";
            this.tabPage_welcome.UseVisualStyleBackColor = true;
            // 
            // textBox_message
            // 
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(0, 0);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(10);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(422, 268);
            this.textBox_message.TabIndex = 10;
            // 
            // tabPage_sqlServerName
            // 
            this.tabPage_sqlServerName.Controls.Add(this.tabControl_serverName);
            this.tabPage_sqlServerName.Controls.Add(this.textBox_instanceName);
            this.tabPage_sqlServerName.Controls.Add(this.label4);
            this.tabPage_sqlServerName.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sqlServerName.Name = "tabPage_sqlServerName";
            this.tabPage_sqlServerName.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sqlServerName.Size = new System.Drawing.Size(422, 268);
            this.tabPage_sqlServerName.TabIndex = 0;
            this.tabPage_sqlServerName.Text = "数据库服务器";
            this.tabPage_sqlServerName.UseVisualStyleBackColor = true;
            // 
            // tabControl_serverName
            // 
            this.tabControl_serverName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_serverName.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_serverName.Controls.Add(this.tabPage_parameters);
            this.tabControl_serverName.Controls.Add(this.tabPage_template);
            this.tabControl_serverName.Location = new System.Drawing.Point(7, 6);
            this.tabControl_serverName.Multiline = true;
            this.tabControl_serverName.Name = "tabControl_serverName";
            this.tabControl_serverName.SelectedIndex = 0;
            this.tabControl_serverName.Size = new System.Drawing.Size(407, 219);
            this.tabControl_serverName.TabIndex = 10;
            // 
            // tabPage_parameters
            // 
            this.tabPage_parameters.Controls.Add(this.button_getServiceName);
            this.tabPage_parameters.Controls.Add(this.textBox_serviceName);
            this.tabPage_parameters.Controls.Add(this.label7);
            this.tabPage_parameters.Controls.Add(this.label9);
            this.tabPage_parameters.Controls.Add(this.textBox_hostName);
            this.tabPage_parameters.Controls.Add(this.textBox_port);
            this.tabPage_parameters.Controls.Add(this.label1);
            this.tabPage_parameters.Controls.Add(this.textBox_sqlServerName);
            this.tabPage_parameters.Controls.Add(this.label8);
            this.tabPage_parameters.Location = new System.Drawing.Point(4, 25);
            this.tabPage_parameters.Name = "tabPage_parameters";
            this.tabPage_parameters.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_parameters.Size = new System.Drawing.Size(399, 190);
            this.tabPage_parameters.TabIndex = 0;
            this.tabPage_parameters.Text = "Oracle 服务器参数";
            this.tabPage_parameters.UseVisualStyleBackColor = true;
            // 
            // button_getServiceName
            // 
            this.button_getServiceName.Location = new System.Drawing.Point(304, 62);
            this.button_getServiceName.Name = "button_getServiceName";
            this.button_getServiceName.Size = new System.Drawing.Size(75, 23);
            this.button_getServiceName.TabIndex = 15;
            this.button_getServiceName.Text = "button1";
            this.button_getServiceName.UseVisualStyleBackColor = true;
            this.button_getServiceName.Visible = false;
            this.button_getServiceName.Click += new System.EventHandler(this.button_getServiceName_Click);
            // 
            // textBox_serviceName
            // 
            this.textBox_serviceName.Location = new System.Drawing.Point(113, 62);
            this.textBox_serviceName.Name = "textBox_serviceName";
            this.textBox_serviceName.Size = new System.Drawing.Size(184, 21);
            this.textBox_serviceName.TabIndex = 14;
            this.textBox_serviceName.Text = "XE";
            this.textBox_serviceName.TextChanged += new System.EventHandler(this.textBox_serviceName_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 9;
            this.label7.Text = "主机名(&H):";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 66);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 13;
            this.label9.Text = "服务名(&S):";
            // 
            // textBox_hostName
            // 
            this.textBox_hostName.Location = new System.Drawing.Point(113, 9);
            this.textBox_hostName.Name = "textBox_hostName";
            this.textBox_hostName.Size = new System.Drawing.Size(184, 21);
            this.textBox_hostName.TabIndex = 10;
            this.textBox_hostName.Text = "localhost";
            this.textBox_hostName.TextChanged += new System.EventHandler(this.textBox_hostName_TextChanged);
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(113, 36);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(83, 21);
            this.textBox_port.TabIndex = 12;
            this.textBox_port.Text = "1521";
            this.textBox_port.TextChanged += new System.EventHandler(this.textBox_port_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "SQL服务器名(&S):";
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(113, 89);
            this.textBox_sqlServerName.Multiline = true;
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.ReadOnly = true;
            this.textBox_sqlServerName.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_sqlServerName.Size = new System.Drawing.Size(280, 95);
            this.textBox_sqlServerName.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 39);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 11;
            this.label8.Text = "端口号(&P):";
            // 
            // tabPage_template
            // 
            this.tabPage_template.Controls.Add(this.textBox_serverNameTemplate);
            this.tabPage_template.Location = new System.Drawing.Point(4, 25);
            this.tabPage_template.Name = "tabPage_template";
            this.tabPage_template.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_template.Size = new System.Drawing.Size(399, 190);
            this.tabPage_template.TabIndex = 1;
            this.tabPage_template.Text = "模板";
            this.tabPage_template.UseVisualStyleBackColor = true;
            // 
            // textBox_serverNameTemplate
            // 
            this.textBox_serverNameTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_serverNameTemplate.Location = new System.Drawing.Point(3, 3);
            this.textBox_serverNameTemplate.Multiline = true;
            this.textBox_serverNameTemplate.Name = "textBox_serverNameTemplate";
            this.textBox_serverNameTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_serverNameTemplate.Size = new System.Drawing.Size(393, 184);
            this.textBox_serverNameTemplate.TabIndex = 6;
            this.textBox_serverNameTemplate.Text = "(DESCRIPTION=\r\n(ADDRESS_LIST=\r\n(ADDRESS=(PROTOCOL=TCP)(HOST=%host%)(PORT=%port%))" +
    ")(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=%servicename%)\r\n)\r\n)";
            this.textBox_serverNameTemplate.TextChanged += new System.EventHandler(this.textBox_serverNameTemplate_TextChanged);
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_instanceName.Location = new System.Drawing.Point(125, 233);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(183, 21);
            this.textBox_instanceName.TabIndex = 8;
            this.textBox_instanceName.Text = "dp2kernel";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 236);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "内核实例名(&I):";
            // 
            // tabPage_createLogin
            // 
            this.tabPage_createLogin.Controls.Add(this.button_copySqlServerInfo);
            this.tabPage_createLogin.Controls.Add(this.groupBox_login);
            this.tabPage_createLogin.Location = new System.Drawing.Point(4, 22);
            this.tabPage_createLogin.Name = "tabPage_createLogin";
            this.tabPage_createLogin.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_createLogin.Size = new System.Drawing.Size(422, 268);
            this.tabPage_createLogin.TabIndex = 1;
            this.tabPage_createLogin.Text = "创建用户";
            this.tabPage_createLogin.UseVisualStyleBackColor = true;
            // 
            // button_copySqlServerInfo
            // 
            this.button_copySqlServerInfo.Enabled = false;
            this.button_copySqlServerInfo.Location = new System.Drawing.Point(6, 230);
            this.button_copySqlServerInfo.Name = "button_copySqlServerInfo";
            this.button_copySqlServerInfo.Size = new System.Drawing.Size(341, 23);
            this.button_copySqlServerInfo.TabIndex = 1;
            this.button_copySqlServerInfo.Text = "复制 Oracle 数据库服务器信息到 Windows 剪贴板";
            this.button_copySqlServerInfo.UseVisualStyleBackColor = true;
            this.button_copySqlServerInfo.Click += new System.EventHandler(this.button_copySqlServerInfo_Click);
            // 
            // groupBox_login
            // 
            this.groupBox_login.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_login.Controls.Add(this.textBox_tableSpaceName);
            this.groupBox_login.Controls.Add(this.label10);
            this.groupBox_login.Controls.Add(this.textBox_tableSpaceFile);
            this.groupBox_login.Controls.Add(this.label6);
            this.groupBox_login.Controls.Add(this.textBox_confirmLoginPassword);
            this.groupBox_login.Controls.Add(this.label3);
            this.groupBox_login.Controls.Add(this.textBox_loginPassword);
            this.groupBox_login.Controls.Add(this.label2);
            this.groupBox_login.Controls.Add(this.textBox_loginName);
            this.groupBox_login.Controls.Add(this.label5);
            this.groupBox_login.Location = new System.Drawing.Point(6, 19);
            this.groupBox_login.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Name = "groupBox_login";
            this.groupBox_login.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Size = new System.Drawing.Size(409, 206);
            this.groupBox_login.TabIndex = 0;
            this.groupBox_login.TabStop = false;
            this.groupBox_login.Text = " 创建一个用于 dp2Kernel 的 Oracle 用户名 ";
            // 
            // textBox_tableSpaceName
            // 
            this.textBox_tableSpaceName.Location = new System.Drawing.Point(111, 126);
            this.textBox_tableSpaceName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_tableSpaceName.Name = "textBox_tableSpaceName";
            this.textBox_tableSpaceName.ReadOnly = true;
            this.textBox_tableSpaceName.Size = new System.Drawing.Size(230, 21);
            this.textBox_tableSpaceName.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 129);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 12);
            this.label10.TabIndex = 6;
            this.label10.Text = "表空间名(&N):";
            // 
            // textBox_tableSpaceFile
            // 
            this.textBox_tableSpaceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tableSpaceFile.Location = new System.Drawing.Point(111, 151);
            this.textBox_tableSpaceFile.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_tableSpaceFile.Name = "textBox_tableSpaceFile";
            this.textBox_tableSpaceFile.Size = new System.Drawing.Size(265, 21);
            this.textBox_tableSpaceFile.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 154);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "表空间文件(&T):";
            // 
            // textBox_confirmLoginPassword
            // 
            this.textBox_confirmLoginPassword.Location = new System.Drawing.Point(111, 90);
            this.textBox_confirmLoginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmLoginPassword.Name = "textBox_confirmLoginPassword";
            this.textBox_confirmLoginPassword.PasswordChar = '*';
            this.textBox_confirmLoginPassword.Size = new System.Drawing.Size(230, 21);
            this.textBox_confirmLoginPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 92);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_loginPassword
            // 
            this.textBox_loginPassword.Location = new System.Drawing.Point(111, 65);
            this.textBox_loginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginPassword.Name = "textBox_loginPassword";
            this.textBox_loginPassword.PasswordChar = '*';
            this.textBox_loginPassword.Size = new System.Drawing.Size(230, 21);
            this.textBox_loginPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_loginName
            // 
            this.textBox_loginName.Location = new System.Drawing.Point(111, 28);
            this.textBox_loginName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginName.Name = "textBox_loginName";
            this.textBox_loginName.Size = new System.Drawing.Size(230, 21);
            this.textBox_loginName.TabIndex = 1;
            this.textBox_loginName.Text = "dp2kernel";
            this.textBox_loginName.TextChanged += new System.EventHandler(this.textBox_loginName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 31);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&N):";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(1, 307);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(75, 38);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // OracleDataSourceWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 348);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_finish);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.button_prev);
            this.Controls.Add(this.tabControl_main);
            this.Name = "OracleDataSourceWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Oracle Database 相关参数设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OracleDataSourceWizard_FormClosed);
            this.Load += new System.EventHandler(this.OracleDataSourceWizard_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_welcome.ResumeLayout(false);
            this.tabPage_welcome.PerformLayout();
            this.tabPage_sqlServerName.ResumeLayout(false);
            this.tabPage_sqlServerName.PerformLayout();
            this.tabControl_serverName.ResumeLayout(false);
            this.tabPage_parameters.ResumeLayout(false);
            this.tabPage_parameters.PerformLayout();
            this.tabPage_template.ResumeLayout(false);
            this.tabPage_template.PerformLayout();
            this.tabPage_createLogin.ResumeLayout(false);
            this.groupBox_login.ResumeLayout(false);
            this.groupBox_login.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_finish;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_prev;
        private WizardPages tabControl_main;
        private System.Windows.Forms.TabPage tabPage_welcome;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.TabPage tabPage_sqlServerName;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_createLogin;
        private System.Windows.Forms.Button button_copySqlServerInfo;
        private System.Windows.Forms.GroupBox groupBox_login;
        private System.Windows.Forms.TextBox textBox_confirmLoginPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_loginPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_loginName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_tableSpaceFile;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_serviceName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_hostName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabControl tabControl_serverName;
        private System.Windows.Forms.TabPage tabPage_parameters;
        private System.Windows.Forms.TabPage tabPage_template;
        private System.Windows.Forms.TextBox textBox_serverNameTemplate;
        private System.Windows.Forms.TextBox textBox_tableSpaceName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button_getServiceName;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}