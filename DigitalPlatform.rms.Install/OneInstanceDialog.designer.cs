namespace DigitalPlatform.rms
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
            this.textBox_sqlDef = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_editSqlDef = new System.Windows.Forms.Button();
            this.button_editRootUserInfo = new System.Windows.Forms.Button();
            this.textBox_rootUserInfo = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_certificate = new System.Windows.Forms.Button();
            this.comboBox_sqlServerType = new System.Windows.Forms.ComboBox();
            this.checkBox_allowChangeSqlServerType = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "实例名(&N):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_instanceName.Location = new System.Drawing.Point(183, 21);
            this.textBox_instanceName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(285, 31);
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
            this.textBox_dataDir.Location = new System.Drawing.Point(183, 68);
            this.textBox_dataDir.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(521, 31);
            this.textBox_dataDir.TabIndex = 3;
            this.textBox_dataDir.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_dataDir_KeyPress);
            this.textBox_dataDir.Leave += new System.EventHandler(this.textBox_dataDir_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 71);
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
            this.textBox_bindings.Location = new System.Drawing.Point(183, 326);
            this.textBox_bindings.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_bindings.Multiline = true;
            this.textBox_bindings.Name = "textBox_bindings";
            this.textBox_bindings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_bindings.Size = new System.Drawing.Size(521, 153);
            this.textBox_bindings.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 329);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 21);
            this.label3.TabIndex = 12;
            this.label3.Text = "协议绑定(&B):";
            // 
            // button_editBindings
            // 
            this.button_editBindings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editBindings.Location = new System.Drawing.Point(710, 326);
            this.button_editBindings.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editBindings.Name = "button_editBindings";
            this.button_editBindings.Size = new System.Drawing.Size(91, 40);
            this.button_editBindings.TabIndex = 14;
            this.button_editBindings.Text = "...";
            this.button_editBindings.UseVisualStyleBackColor = true;
            this.button_editBindings.Click += new System.EventHandler(this.button_editBindings_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(656, 514);
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
            this.button_OK.Location = new System.Drawing.Point(508, 514);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_sqlDef
            // 
            this.textBox_sqlDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlDef.Location = new System.Drawing.Point(183, 184);
            this.textBox_sqlDef.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_sqlDef.Name = "textBox_sqlDef";
            this.textBox_sqlDef.ReadOnly = true;
            this.textBox_sqlDef.Size = new System.Drawing.Size(521, 31);
            this.textBox_sqlDef.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 141);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 21);
            this.label4.TabIndex = 4;
            this.label4.Text = "SQL服务器(&S):";
            // 
            // button_editSqlDef
            // 
            this.button_editSqlDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editSqlDef.Location = new System.Drawing.Point(710, 180);
            this.button_editSqlDef.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editSqlDef.Name = "button_editSqlDef";
            this.button_editSqlDef.Size = new System.Drawing.Size(91, 40);
            this.button_editSqlDef.TabIndex = 8;
            this.button_editSqlDef.Text = "...";
            this.button_editSqlDef.UseVisualStyleBackColor = true;
            this.button_editSqlDef.Click += new System.EventHandler(this.button_editSqlDef_Click);
            // 
            // button_editRootUserInfo
            // 
            this.button_editRootUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editRootUserInfo.Location = new System.Drawing.Point(710, 248);
            this.button_editRootUserInfo.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editRootUserInfo.Name = "button_editRootUserInfo";
            this.button_editRootUserInfo.Size = new System.Drawing.Size(91, 40);
            this.button_editRootUserInfo.TabIndex = 11;
            this.button_editRootUserInfo.Text = "...";
            this.button_editRootUserInfo.UseVisualStyleBackColor = true;
            this.button_editRootUserInfo.Click += new System.EventHandler(this.button_editRootUserInfo_Click);
            // 
            // textBox_rootUserInfo
            // 
            this.textBox_rootUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rootUserInfo.Location = new System.Drawing.Point(183, 252);
            this.textBox_rootUserInfo.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_rootUserInfo.Name = "textBox_rootUserInfo";
            this.textBox_rootUserInfo.ReadOnly = true;
            this.textBox_rootUserInfo.Size = new System.Drawing.Size(521, 31);
            this.textBox_rootUserInfo.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 258);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(140, 21);
            this.label5.TabIndex = 9;
            this.label5.Text = "root账户(&R):";
            // 
            // button_certificate
            // 
            this.button_certificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_certificate.Location = new System.Drawing.Point(19, 514);
            this.button_certificate.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_certificate.Name = "button_certificate";
            this.button_certificate.Size = new System.Drawing.Size(156, 40);
            this.button_certificate.TabIndex = 15;
            this.button_certificate.Text = "证书(&C)...";
            this.button_certificate.UseVisualStyleBackColor = true;
            this.button_certificate.Click += new System.EventHandler(this.button_certificate_Click);
            // 
            // comboBox_sqlServerType
            // 
            this.comboBox_sqlServerType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_sqlServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_sqlServerType.Enabled = false;
            this.comboBox_sqlServerType.FormattingEnabled = true;
            this.comboBox_sqlServerType.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.comboBox_sqlServerType.Items.AddRange(new object[] {
            "SQLite",
            "MS SQL Server",
            "MySQL Server",
            "Oracle",
            "PostgreSQL",
            "[清除]"});
            this.comboBox_sqlServerType.Location = new System.Drawing.Point(183, 138);
            this.comboBox_sqlServerType.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_sqlServerType.Name = "comboBox_sqlServerType";
            this.comboBox_sqlServerType.Size = new System.Drawing.Size(521, 29);
            this.comboBox_sqlServerType.TabIndex = 5;
            this.comboBox_sqlServerType.SelectionChangeCommitted += new System.EventHandler(this.comboBox_sqlServerType_SelectionChangeCommitted);
            this.comboBox_sqlServerType.TextChanged += new System.EventHandler(this.comboBox_sqlServerType_TextChanged);
            // 
            // checkBox_allowChangeSqlServerType
            // 
            this.checkBox_allowChangeSqlServerType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_allowChangeSqlServerType.AutoSize = true;
            this.checkBox_allowChangeSqlServerType.Location = new System.Drawing.Point(710, 140);
            this.checkBox_allowChangeSqlServerType.Name = "checkBox_allowChangeSqlServerType";
            this.checkBox_allowChangeSqlServerType.Size = new System.Drawing.Size(78, 25);
            this.checkBox_allowChangeSqlServerType.TabIndex = 6;
            this.checkBox_allowChangeSqlServerType.Text = "修改";
            this.checkBox_allowChangeSqlServerType.UseVisualStyleBackColor = true;
            this.checkBox_allowChangeSqlServerType.CheckedChanged += new System.EventHandler(this.checkBox_allowChangeSqlServerType_CheckedChanged);
            // 
            // OneInstanceDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(816, 576);
            this.Controls.Add(this.checkBox_allowChangeSqlServerType);
            this.Controls.Add(this.comboBox_sqlServerType);
            this.Controls.Add(this.button_certificate);
            this.Controls.Add(this.button_editRootUserInfo);
            this.Controls.Add(this.textBox_rootUserInfo);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_editSqlDef);
            this.Controls.Add(this.textBox_sqlDef);
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
        private System.Windows.Forms.TextBox textBox_sqlDef;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_editSqlDef;
        private System.Windows.Forms.Button button_editRootUserInfo;
        private System.Windows.Forms.TextBox textBox_rootUserInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_certificate;
        private System.Windows.Forms.ComboBox comboBox_sqlServerType;
        private System.Windows.Forms.CheckBox checkBox_allowChangeSqlServerType;
    }
}