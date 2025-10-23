namespace dp2Circulation
{
    partial class RefreshStyleDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RefreshStyleDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkedComboBox_singleCfgFileName = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.radioButton_singleCfgFile = new System.Windows.Forms.RadioButton();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.radioButton_all = new System.Windows.Forms.RadioButton();
            this.radioButton_allButTemplateFile = new System.Windows.Forms.RadioButton();
            this.radioButton_structure = new System.Windows.Forms.RadioButton();
            this.comboBox_templatesPath = new System.Windows.Forms.ComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_autoRebuildKeys = new System.Windows.Forms.CheckBox();
            this.checkBox_recoverState = new System.Windows.Forms.CheckBox();
            this.label_templatePath = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkedComboBox_singleCfgFileName);
            this.groupBox1.Controls.Add(this.radioButton_singleCfgFile);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.radioButton_all);
            this.groupBox1.Controls.Add(this.radioButton_allButTemplateFile);
            this.groupBox1.Controls.Add(this.radioButton_structure);
            this.groupBox1.Location = new System.Drawing.Point(15, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(681, 403);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 刷新数据库的方式 ";
            // 
            // checkedComboBox_singleCfgFileName
            // 
            this.checkedComboBox_singleCfgFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_singleCfgFileName.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_singleCfgFileName.Location = new System.Drawing.Point(40, 348);
            this.checkedComboBox_singleCfgFileName.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_singleCfgFileName.Name = "checkedComboBox_singleCfgFileName";
            this.checkedComboBox_singleCfgFileName.ReadOnly = false;
            this.checkedComboBox_singleCfgFileName.Size = new System.Drawing.Size(620, 21);
            this.checkedComboBox_singleCfgFileName.TabIndex = 7;
            this.checkedComboBox_singleCfgFileName.Visible = false;
            // 
            // radioButton_singleCfgFile
            // 
            this.radioButton_singleCfgFile.AutoSize = true;
            this.radioButton_singleCfgFile.Location = new System.Drawing.Point(21, 323);
            this.radioButton_singleCfgFile.Name = "radioButton_singleCfgFile";
            this.radioButton_singleCfgFile.Size = new System.Drawing.Size(168, 22);
            this.radioButton_singleCfgFile.TabIndex = 6;
            this.radioButton_singleCfgFile.Text = "单个配置文件(&I)";
            this.radioButton_singleCfgFile.UseVisualStyleBackColor = true;
            this.radioButton_singleCfgFile.CheckedChanged += new System.EventHandler(this.radioButton_singleCfgFile_CheckedChanged);
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.BackColor = System.Drawing.SystemColors.Info;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox3.Location = new System.Drawing.Point(40, 250);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(620, 58);
            this.textBox3.TabIndex = 5;
            this.textBox3.Text = "刷新数据库下属的全部配置文件";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.BackColor = System.Drawing.SystemColors.Info;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox2.Location = new System.Drawing.Point(40, 154);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(620, 58);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "刷新数据库下属的全部配置文件，但不包含新记录模板文件template";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox1.Location = new System.Drawing.Point(40, 58);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(620, 58);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "刷新负责定义数据库结构的keys文件和定义浏览格式的browse文件";
            // 
            // radioButton_all
            // 
            this.radioButton_all.AutoSize = true;
            this.radioButton_all.Location = new System.Drawing.Point(21, 220);
            this.radioButton_all.Name = "radioButton_all";
            this.radioButton_all.Size = new System.Drawing.Size(96, 22);
            this.radioButton_all.TabIndex = 4;
            this.radioButton_all.Text = "全部(&A)";
            this.radioButton_all.UseVisualStyleBackColor = true;
            // 
            // radioButton_allButTemplateFile
            // 
            this.radioButton_allButTemplateFile.AutoSize = true;
            this.radioButton_allButTemplateFile.Location = new System.Drawing.Point(21, 124);
            this.radioButton_allButTemplateFile.Name = "radioButton_allButTemplateFile";
            this.radioButton_allButTemplateFile.Size = new System.Drawing.Size(312, 22);
            this.radioButton_allButTemplateFile.TabIndex = 2;
            this.radioButton_allButTemplateFile.Text = "全部，但不包括新记录模板文件(&T)";
            this.radioButton_allButTemplateFile.UseVisualStyleBackColor = true;
            // 
            // radioButton_structure
            // 
            this.radioButton_structure.AutoSize = true;
            this.radioButton_structure.Checked = true;
            this.radioButton_structure.Location = new System.Drawing.Point(21, 28);
            this.radioButton_structure.Name = "radioButton_structure";
            this.radioButton_structure.Size = new System.Drawing.Size(96, 22);
            this.radioButton_structure.TabIndex = 0;
            this.radioButton_structure.TabStop = true;
            this.radioButton_structure.Text = "结构(&S)";
            this.radioButton_structure.UseVisualStyleBackColor = true;
            // 
            // comboBox_templatesPath
            // 
            this.comboBox_templatesPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_templatesPath.FormattingEnabled = true;
            this.comboBox_templatesPath.Location = new System.Drawing.Point(15, 452);
            this.comboBox_templatesPath.Name = "comboBox_templatesPath";
            this.comboBox_templatesPath.Size = new System.Drawing.Size(680, 26);
            this.comboBox_templatesPath.TabIndex = 2;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(611, 610);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 33);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(521, 610);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 33);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_autoRebuildKeys
            // 
            this.checkBox_autoRebuildKeys.AutoSize = true;
            this.checkBox_autoRebuildKeys.Location = new System.Drawing.Point(15, 505);
            this.checkBox_autoRebuildKeys.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_autoRebuildKeys.Name = "checkBox_autoRebuildKeys";
            this.checkBox_autoRebuildKeys.Size = new System.Drawing.Size(259, 22);
            this.checkBox_autoRebuildKeys.TabIndex = 3;
            this.checkBox_autoRebuildKeys.Text = "自动启动重建检索点任务(&A)";
            this.checkBox_autoRebuildKeys.UseVisualStyleBackColor = true;
            this.checkBox_autoRebuildKeys.Visible = false;
            // 
            // checkBox_recoverState
            // 
            this.checkBox_recoverState.AutoSize = true;
            this.checkBox_recoverState.Location = new System.Drawing.Point(15, 539);
            this.checkBox_recoverState.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_recoverState.Name = "checkBox_recoverState";
            this.checkBox_recoverState.Size = new System.Drawing.Size(268, 22);
            this.checkBox_recoverState.TabIndex = 4;
            this.checkBox_recoverState.Text = "keys 使用容错日志恢复版(&R)";
            this.checkBox_recoverState.UseVisualStyleBackColor = true;
            this.checkBox_recoverState.Visible = false;
            // 
            // label_templatePath
            // 
            this.label_templatePath.AutoSize = true;
            this.label_templatePath.Location = new System.Drawing.Point(12, 431);
            this.label_templatePath.Name = "label_templatePath";
            this.label_templatePath.Size = new System.Drawing.Size(116, 18);
            this.label_templatePath.TabIndex = 1;
            this.label_templatePath.Text = "模板目录(&T):";
            // 
            // RefreshStyleDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(710, 658);
            this.Controls.Add(this.label_templatePath);
            this.Controls.Add(this.checkBox_recoverState);
            this.Controls.Add(this.checkBox_autoRebuildKeys);
            this.Controls.Add(this.comboBox_templatesPath);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RefreshStyleDialog";
            this.ShowInTaskbar = false;
            this.Text = "刷新数据库定义的方式";
            this.Load += new System.EventHandler(this.RefreshStyleDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_all;
        private System.Windows.Forms.RadioButton radioButton_allButTemplateFile;
        private System.Windows.Forms.RadioButton radioButton_structure;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_autoRebuildKeys;
        private System.Windows.Forms.CheckBox checkBox_recoverState;
        private System.Windows.Forms.RadioButton radioButton_singleCfgFile;
        private System.Windows.Forms.ComboBox comboBox_templatesPath;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_singleCfgFileName;
        private System.Windows.Forms.Label label_templatePath;
    }
}