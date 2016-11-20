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
            this.button_certificate = new System.Windows.Forms.Button();
            this.checkBox_updateCfgsDir = new System.Windows.Forms.CheckBox();
            this.button_setSerialNumber = new System.Windows.Forms.Button();
            this.button_configMq = new System.Windows.Forms.Button();
            this.button_configMongoDB = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "实例名(&N):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Location = new System.Drawing.Point(155, 12);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(165, 21);
            this.textBox_instanceName.TabIndex = 1;
            this.textBox_instanceName.TextChanged += new System.EventHandler(this.textBox_instanceName_TextChanged);
            this.textBox_instanceName.Leave += new System.EventHandler(this.textBox_instanceName_Leave);
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.Location = new System.Drawing.Point(155, 39);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(226, 21);
            this.textBox_dataDir.TabIndex = 3;
            this.textBox_dataDir.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_dataDir_KeyPress);
            this.textBox_dataDir.Leave += new System.EventHandler(this.textBox_dataDir_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "数据目录(&D):";
            // 
            // textBox_bindings
            // 
            this.textBox_bindings.AcceptsReturn = true;
            this.textBox_bindings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_bindings.Location = new System.Drawing.Point(155, 190);
            this.textBox_bindings.Multiline = true;
            this.textBox_bindings.Name = "textBox_bindings";
            this.textBox_bindings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_bindings.Size = new System.Drawing.Size(226, 84);
            this.textBox_bindings.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 190);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "协议绑定(&B):";
            // 
            // button_editBindings
            // 
            this.button_editBindings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editBindings.Location = new System.Drawing.Point(388, 190);
            this.button_editBindings.Name = "button_editBindings";
            this.button_editBindings.Size = new System.Drawing.Size(45, 23);
            this.button_editBindings.TabIndex = 12;
            this.button_editBindings.Text = "...";
            this.button_editBindings.UseVisualStyleBackColor = true;
            this.button_editBindings.Click += new System.EventHandler(this.button_editBindings_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(358, 371);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(277, 371);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_dp2KernelDef
            // 
            this.textBox_dp2KernelDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2KernelDef.Location = new System.Drawing.Point(155, 107);
            this.textBox_dp2KernelDef.Name = "textBox_dp2KernelDef";
            this.textBox_dp2KernelDef.ReadOnly = true;
            this.textBox_dp2KernelDef.Size = new System.Drawing.Size(226, 21);
            this.textBox_dp2KernelDef.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "dp2Kernel服务器(&K):";
            // 
            // button_editSqlDef
            // 
            this.button_editSqlDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editSqlDef.Location = new System.Drawing.Point(387, 107);
            this.button_editSqlDef.Name = "button_editSqlDef";
            this.button_editSqlDef.Size = new System.Drawing.Size(45, 23);
            this.button_editSqlDef.TabIndex = 6;
            this.button_editSqlDef.Text = "...";
            this.button_editSqlDef.UseVisualStyleBackColor = true;
            this.button_editSqlDef.Click += new System.EventHandler(this.button_editdp2KernelDef_Click);
            // 
            // button_editRootUserInfo
            // 
            this.button_editRootUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editRootUserInfo.Location = new System.Drawing.Point(387, 146);
            this.button_editRootUserInfo.Name = "button_editRootUserInfo";
            this.button_editRootUserInfo.Size = new System.Drawing.Size(45, 23);
            this.button_editRootUserInfo.TabIndex = 9;
            this.button_editRootUserInfo.Text = "...";
            this.button_editRootUserInfo.UseVisualStyleBackColor = true;
            this.button_editRootUserInfo.Click += new System.EventHandler(this.button_editSupervisorUserInfo_Click);
            // 
            // textBox_supervisorUserInfo
            // 
            this.textBox_supervisorUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_supervisorUserInfo.Location = new System.Drawing.Point(155, 146);
            this.textBox_supervisorUserInfo.Name = "textBox_supervisorUserInfo";
            this.textBox_supervisorUserInfo.ReadOnly = true;
            this.textBox_supervisorUserInfo.Size = new System.Drawing.Size(226, 21);
            this.textBox_supervisorUserInfo.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 149);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "supervisor账户(&R):";
            // 
            // textBox_libraryName
            // 
            this.textBox_libraryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_libraryName.Location = new System.Drawing.Point(155, 280);
            this.textBox_libraryName.Name = "textBox_libraryName";
            this.textBox_libraryName.Size = new System.Drawing.Size(226, 21);
            this.textBox_libraryName.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 283);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "图书馆名(&L):";
            // 
            // button_certificate
            // 
            this.button_certificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_certificate.Location = new System.Drawing.Point(12, 371);
            this.button_certificate.Name = "button_certificate";
            this.button_certificate.Size = new System.Drawing.Size(85, 23);
            this.button_certificate.TabIndex = 15;
            this.button_certificate.Text = "证书(&C)...";
            this.button_certificate.UseVisualStyleBackColor = true;
            this.button_certificate.Click += new System.EventHandler(this.button_certificate_Click);
            // 
            // checkBox_updateCfgsDir
            // 
            this.checkBox_updateCfgsDir.AutoSize = true;
            this.checkBox_updateCfgsDir.Location = new System.Drawing.Point(155, 66);
            this.checkBox_updateCfgsDir.Name = "checkBox_updateCfgsDir";
            this.checkBox_updateCfgsDir.Size = new System.Drawing.Size(210, 16);
            this.checkBox_updateCfgsDir.TabIndex = 18;
            this.checkBox_updateCfgsDir.Text = "更新数据目录的cfgs子目录内容(&U)";
            this.checkBox_updateCfgsDir.UseVisualStyleBackColor = true;
            // 
            // button_setSerialNumber
            // 
            this.button_setSerialNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_setSerialNumber.Location = new System.Drawing.Point(103, 371);
            this.button_setSerialNumber.Name = "button_setSerialNumber";
            this.button_setSerialNumber.Size = new System.Drawing.Size(95, 23);
            this.button_setSerialNumber.TabIndex = 19;
            this.button_setSerialNumber.Text = "序列号(&S)...";
            this.button_setSerialNumber.UseVisualStyleBackColor = true;
            this.button_setSerialNumber.Click += new System.EventHandler(this.button_setSerialNumber_Click);
            // 
            // button_configMq
            // 
            this.button_configMq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_configMq.Location = new System.Drawing.Point(155, 307);
            this.button_configMq.Name = "button_configMq";
            this.button_configMq.Size = new System.Drawing.Size(226, 23);
            this.button_configMq.TabIndex = 20;
            this.button_configMq.Text = "为 library.xml 自动配置 MSMQ 参数";
            this.button_configMq.UseVisualStyleBackColor = true;
            this.button_configMq.Click += new System.EventHandler(this.button_configMq_Click);
            // 
            // button_configMongoDB
            // 
            this.button_configMongoDB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_configMongoDB.Location = new System.Drawing.Point(155, 336);
            this.button_configMongoDB.Name = "button_configMongoDB";
            this.button_configMongoDB.Size = new System.Drawing.Size(277, 23);
            this.button_configMongoDB.TabIndex = 21;
            this.button_configMongoDB.Text = "为 library.xml 自动配置 MongoDB 参数";
            this.button_configMongoDB.UseVisualStyleBackColor = true;
            this.button_configMongoDB.Click += new System.EventHandler(this.button_configMongoDB_Click);
            // 
            // OneInstanceDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(445, 406);
            this.Controls.Add(this.button_configMongoDB);
            this.Controls.Add(this.button_configMq);
            this.Controls.Add(this.button_setSerialNumber);
            this.Controls.Add(this.checkBox_updateCfgsDir);
            this.Controls.Add(this.button_certificate);
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
            this.Name = "OneInstanceDialog";
            this.ShowInTaskbar = false;
            this.Text = "一个实例";
            this.Load += new System.EventHandler(this.OneInstanceDialog_Load);
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
        private System.Windows.Forms.Button button_certificate;
        private System.Windows.Forms.CheckBox checkBox_updateCfgsDir;
        private System.Windows.Forms.Button button_setSerialNumber;
        private System.Windows.Forms.Button button_configMq;
        private System.Windows.Forms.Button button_configMongoDB;
    }
}