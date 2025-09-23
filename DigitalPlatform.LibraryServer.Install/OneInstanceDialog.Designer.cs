namespace DigitalPlatform.LibraryServer
{
    partial class OneInstanceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OneInstanceDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_bindings = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_editBindings = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_dp2KernelDef = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_editSqlDef = new System.Windows.Forms.Button();
            this.button_editRootUserInfo = new System.Windows.Forms.Button();
            this.textBox_supervisorUserInfo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_libraryName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox_updateCfgsDir = new System.Windows.Forms.CheckBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_certificate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_serSerialNumber = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_commands = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_configMq = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_configMongoDB = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_configServerReplication = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_configReporting = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_configMessageServer = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox_stopInstance = new System.Windows.Forms.CheckBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "实例名(&N):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_instanceName.Location = new System.Drawing.Point(284, 21);
            this.textBox_instanceName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(299, 31);
            this.textBox_instanceName.TabIndex = 1;
            this.textBox_instanceName.TextChanged += new System.EventHandler(this.textBox_instanceName_TextChanged);
            this.textBox_instanceName.Leave += new System.EventHandler(this.textBox_instanceName_Leave);
            this.textBox_instanceName.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_instanceName_Validating);
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dataDir.Location = new System.Drawing.Point(284, 68);
            this.textBox_dataDir.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(411, 31);
            this.textBox_dataDir.TabIndex = 3;
            this.textBox_dataDir.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_dataDir_KeyPress);
            this.textBox_dataDir.Leave += new System.EventHandler(this.textBox_dataDir_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 74);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "数据目录(&D):";
            // 
            // textBox_bindings
            // 
            this.textBox_bindings.AcceptsReturn = true;
            this.textBox_bindings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_bindings.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_bindings.Location = new System.Drawing.Point(284, 332);
            this.textBox_bindings.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_bindings.Multiline = true;
            this.textBox_bindings.Name = "textBox_bindings";
            this.textBox_bindings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_bindings.Size = new System.Drawing.Size(411, 210);
            this.textBox_bindings.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 332);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 21);
            this.label3.TabIndex = 10;
            this.label3.Text = "协议绑定(&B):";
            // 
            // button_editBindings
            // 
            this.button_editBindings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editBindings.Location = new System.Drawing.Point(711, 332);
            this.button_editBindings.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editBindings.Name = "button_editBindings";
            this.button_editBindings.Size = new System.Drawing.Size(83, 40);
            this.button_editBindings.TabIndex = 12;
            this.button_editBindings.Text = "...";
            this.button_editBindings.UseVisualStyleBackColor = true;
            this.button_editBindings.Click += new System.EventHandler(this.button_editBindings_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(656, 660);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(508, 660);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_dp2KernelDef
            // 
            this.textBox_dp2KernelDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2KernelDef.Location = new System.Drawing.Point(284, 187);
            this.textBox_dp2KernelDef.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_dp2KernelDef.Name = "textBox_dp2KernelDef";
            this.textBox_dp2KernelDef.ReadOnly = true;
            this.textBox_dp2KernelDef.Size = new System.Drawing.Size(411, 31);
            this.textBox_dp2KernelDef.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 192);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(216, 21);
            this.label4.TabIndex = 4;
            this.label4.Text = "dp2Kernel服务器(&K):";
            // 
            // button_editSqlDef
            // 
            this.button_editSqlDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editSqlDef.Location = new System.Drawing.Point(710, 187);
            this.button_editSqlDef.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editSqlDef.Name = "button_editSqlDef";
            this.button_editSqlDef.Size = new System.Drawing.Size(83, 40);
            this.button_editSqlDef.TabIndex = 6;
            this.button_editSqlDef.Text = "...";
            this.button_editSqlDef.UseVisualStyleBackColor = true;
            this.button_editSqlDef.Click += new System.EventHandler(this.button_editdp2KernelDef_Click);
            // 
            // button_editRootUserInfo
            // 
            this.button_editRootUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editRootUserInfo.Location = new System.Drawing.Point(710, 256);
            this.button_editRootUserInfo.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editRootUserInfo.Name = "button_editRootUserInfo";
            this.button_editRootUserInfo.Size = new System.Drawing.Size(83, 40);
            this.button_editRootUserInfo.TabIndex = 9;
            this.button_editRootUserInfo.Text = "...";
            this.button_editRootUserInfo.UseVisualStyleBackColor = true;
            this.button_editRootUserInfo.Click += new System.EventHandler(this.button_editSupervisorUserInfo_Click);
            // 
            // textBox_supervisorUserInfo
            // 
            this.textBox_supervisorUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_supervisorUserInfo.Location = new System.Drawing.Point(284, 256);
            this.textBox_supervisorUserInfo.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_supervisorUserInfo.Name = "textBox_supervisorUserInfo";
            this.textBox_supervisorUserInfo.ReadOnly = true;
            this.textBox_supervisorUserInfo.Size = new System.Drawing.Size(411, 31);
            this.textBox_supervisorUserInfo.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 261);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(206, 21);
            this.label5.TabIndex = 7;
            this.label5.Text = "supervisor账户(&R):";
            // 
            // textBox_libraryName
            // 
            this.textBox_libraryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_libraryName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_libraryName.Location = new System.Drawing.Point(284, 556);
            this.textBox_libraryName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_libraryName.Name = "textBox_libraryName";
            this.textBox_libraryName.Size = new System.Drawing.Size(411, 31);
            this.textBox_libraryName.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 562);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(138, 21);
            this.label6.TabIndex = 13;
            this.label6.Text = "图书馆名(&L):";
            // 
            // checkBox_updateCfgsDir
            // 
            this.checkBox_updateCfgsDir.AutoSize = true;
            this.checkBox_updateCfgsDir.Location = new System.Drawing.Point(284, 116);
            this.checkBox_updateCfgsDir.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_updateCfgsDir.Name = "checkBox_updateCfgsDir";
            this.checkBox_updateCfgsDir.Size = new System.Drawing.Size(365, 25);
            this.checkBox_updateCfgsDir.TabIndex = 18;
            this.checkBox_updateCfgsDir.Text = "更新数据目录的cfgs子目录内容(&U)";
            this.checkBox_updateCfgsDir.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_certificate,
            this.toolStripButton_serSerialNumber,
            this.toolStripSeparator1,
            this.toolStripDropDownButton_commands});
            this.toolStrip1.Location = new System.Drawing.Point(26, 671);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(353, 38);
            this.toolStrip1.TabIndex = 22;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_certificate
            // 
            this.toolStripButton_certificate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_certificate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_certificate.Image")));
            this.toolStripButton_certificate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_certificate.Name = "toolStripButton_certificate";
            this.toolStripButton_certificate.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_certificate.Text = "证书";
            this.toolStripButton_certificate.Click += new System.EventHandler(this.toolStripButton_certificate_Click);
            // 
            // toolStripButton_serSerialNumber
            // 
            this.toolStripButton_serSerialNumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_serSerialNumber.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_serSerialNumber.Image")));
            this.toolStripButton_serSerialNumber.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_serSerialNumber.Name = "toolStripButton_serSerialNumber";
            this.toolStripButton_serSerialNumber.Size = new System.Drawing.Size(79, 32);
            this.toolStripButton_serSerialNumber.Text = "序列号";
            this.toolStripButton_serSerialNumber.Click += new System.EventHandler(this.toolStripButton_setSerialNumber_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripDropDownButton_commands
            // 
            this.toolStripDropDownButton_commands.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_commands.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_configMq,
            this.ToolStripMenuItem_configMongoDB,
            this.ToolStripMenuItem_configServerReplication,
            this.ToolStripMenuItem_configReporting,
            this.ToolStripMenuItem_configMessageServer});
            this.toolStripDropDownButton_commands.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_commands.Image")));
            this.toolStripDropDownButton_commands.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_commands.Name = "toolStripDropDownButton_commands";
            this.toolStripDropDownButton_commands.Size = new System.Drawing.Size(187, 32);
            this.toolStripDropDownButton_commands.Text = "配置 library.xml";
            // 
            // ToolStripMenuItem_configMq
            // 
            this.ToolStripMenuItem_configMq.Name = "ToolStripMenuItem_configMq";
            this.ToolStripMenuItem_configMq.Size = new System.Drawing.Size(369, 40);
            this.ToolStripMenuItem_configMq.Text = "自动配置 MSMQ 参数";
            this.ToolStripMenuItem_configMq.Click += new System.EventHandler(this.ToolStripMenuItem_configMq_Click);
            // 
            // ToolStripMenuItem_configMongoDB
            // 
            this.ToolStripMenuItem_configMongoDB.Name = "ToolStripMenuItem_configMongoDB";
            this.ToolStripMenuItem_configMongoDB.Size = new System.Drawing.Size(369, 40);
            this.ToolStripMenuItem_configMongoDB.Text = "自动配置 MongoDB 参数";
            this.ToolStripMenuItem_configMongoDB.Click += new System.EventHandler(this.ToolStripMenuItem_configMongoDB_Click);
            // 
            // ToolStripMenuItem_configServerReplication
            // 
            this.ToolStripMenuItem_configServerReplication.Name = "ToolStripMenuItem_configServerReplication";
            this.ToolStripMenuItem_configServerReplication.Size = new System.Drawing.Size(369, 40);
            this.ToolStripMenuItem_configServerReplication.Text = "配置服务器同步参数";
            this.ToolStripMenuItem_configServerReplication.Click += new System.EventHandler(this.ToolStripMenuItem_configServerReplication_Click);
            // 
            // ToolStripMenuItem_configReporting
            // 
            this.ToolStripMenuItem_configReporting.Name = "ToolStripMenuItem_configReporting";
            this.ToolStripMenuItem_configReporting.Size = new System.Drawing.Size(369, 40);
            this.ToolStripMenuItem_configReporting.Text = "配置报表参数";
            this.ToolStripMenuItem_configReporting.Click += new System.EventHandler(this.ToolStripMenuItem_configReporting_Click);
            // 
            // ToolStripMenuItem_configMessageServer
            // 
            this.ToolStripMenuItem_configMessageServer.Name = "ToolStripMenuItem_configMessageServer";
            this.ToolStripMenuItem_configMessageServer.Size = new System.Drawing.Size(369, 40);
            this.ToolStripMenuItem_configMessageServer.Text = "配置消息服务器参数";
            this.ToolStripMenuItem_configMessageServer.Click += new System.EventHandler(this.ToolStripMenuItem_configMessageServer_Click);
            // 
            // checkBox_stopInstance
            // 
            this.checkBox_stopInstance.AutoSize = true;
            this.checkBox_stopInstance.Location = new System.Drawing.Point(25, 615);
            this.checkBox_stopInstance.Name = "checkBox_stopInstance";
            this.checkBox_stopInstance.Size = new System.Drawing.Size(141, 25);
            this.checkBox_stopInstance.TabIndex = 23;
            this.checkBox_stopInstance.Text = "停用本实例";
            this.checkBox_stopInstance.UseVisualStyleBackColor = true;
            this.checkBox_stopInstance.CheckedChanged += new System.EventHandler(this.checkBox_stopInstance_CheckedChanged);
            // 
            // OneInstanceDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(816, 721);
            this.Controls.Add(this.checkBox_stopInstance);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.checkBox_updateCfgsDir);
            this.Controls.Add(this.textBox_libraryName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button_editRootUserInfo);
            this.Controls.Add(this.textBox_supervisorUserInfo);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_editSqlDef);
            this.Controls.Add(this.textBox_dp2KernelDef);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_editBindings);
            this.Controls.Add(this.textBox_bindings);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_instanceName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "OneInstanceDialog";
            this.ShowInTaskbar = false;
            this.Text = "一个实例";
            this.Load += new System.EventHandler(this.OneInstanceDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_bindings;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_editBindings;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_dp2KernelDef;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_editSqlDef;
        private System.Windows.Forms.Button button_editRootUserInfo;
        private System.Windows.Forms.TextBox textBox_supervisorUserInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_libraryName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox_updateCfgsDir;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_certificate;
        private System.Windows.Forms.ToolStripButton toolStripButton_serSerialNumber;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_commands;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_configMq;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_configMongoDB;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_configServerReplication;
        private System.Windows.Forms.CheckBox checkBox_stopInstance;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_configReporting;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_configMessageServer;
    }
}