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
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.radioButton_all = new System.Windows.Forms.RadioButton();
            this.radioButton_allButTemplateFile = new System.Windows.Forms.RadioButton();
            this.radioButton_structure = new System.Windows.Forms.RadioButton();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_autoRebuildKeys = new System.Windows.Forms.CheckBox();
            this.checkBox_recoverState = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.radioButton_all);
            this.groupBox1.Controls.Add(this.radioButton_allButTemplateFile);
            this.groupBox1.Controls.Add(this.radioButton_structure);
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(271, 228);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 刷新数据库的方式 ";
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.BackColor = System.Drawing.SystemColors.Info;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox3.Location = new System.Drawing.Point(27, 167);
            this.textBox3.Margin = new System.Windows.Forms.Padding(2);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(230, 39);
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
            this.textBox2.Location = new System.Drawing.Point(27, 103);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(230, 39);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "刷新数据库下属的全部配置文件，但不包含新记录模板文件template";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.Info;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox1.Location = new System.Drawing.Point(27, 39);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(230, 39);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "刷新负责定义数据库结构的keys文件和定义浏览格式的browse文件";
            // 
            // radioButton_all
            // 
            this.radioButton_all.AutoSize = true;
            this.radioButton_all.Location = new System.Drawing.Point(14, 147);
            this.radioButton_all.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_all.Name = "radioButton_all";
            this.radioButton_all.Size = new System.Drawing.Size(65, 16);
            this.radioButton_all.TabIndex = 2;
            this.radioButton_all.Text = "全部(&A)";
            this.radioButton_all.UseVisualStyleBackColor = true;
            // 
            // radioButton_allButTemplateFile
            // 
            this.radioButton_allButTemplateFile.AutoSize = true;
            this.radioButton_allButTemplateFile.Location = new System.Drawing.Point(14, 83);
            this.radioButton_allButTemplateFile.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_allButTemplateFile.Name = "radioButton_allButTemplateFile";
            this.radioButton_allButTemplateFile.Size = new System.Drawing.Size(209, 16);
            this.radioButton_allButTemplateFile.TabIndex = 1;
            this.radioButton_allButTemplateFile.Text = "全部，但不包括新记录模板文件(&T)";
            this.radioButton_allButTemplateFile.UseVisualStyleBackColor = true;
            // 
            // radioButton_structure
            // 
            this.radioButton_structure.AutoSize = true;
            this.radioButton_structure.Checked = true;
            this.radioButton_structure.Location = new System.Drawing.Point(14, 19);
            this.radioButton_structure.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_structure.Name = "radioButton_structure";
            this.radioButton_structure.Size = new System.Drawing.Size(65, 16);
            this.radioButton_structure.TabIndex = 0;
            this.radioButton_structure.TabStop = true;
            this.radioButton_structure.Text = "结构(&S)";
            this.radioButton_structure.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(224, 296);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 21;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(164, 296);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 20;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_autoRebuildKeys
            // 
            this.checkBox_autoRebuildKeys.AutoSize = true;
            this.checkBox_autoRebuildKeys.Location = new System.Drawing.Point(10, 243);
            this.checkBox_autoRebuildKeys.Name = "checkBox_autoRebuildKeys";
            this.checkBox_autoRebuildKeys.Size = new System.Drawing.Size(174, 16);
            this.checkBox_autoRebuildKeys.TabIndex = 22;
            this.checkBox_autoRebuildKeys.Text = "自动启动重建检索点任务(&A)";
            this.checkBox_autoRebuildKeys.UseVisualStyleBackColor = true;
            this.checkBox_autoRebuildKeys.Visible = false;
            // 
            // checkBox_recoverState
            // 
            this.checkBox_recoverState.AutoSize = true;
            this.checkBox_recoverState.Location = new System.Drawing.Point(10, 265);
            this.checkBox_recoverState.Name = "checkBox_recoverState";
            this.checkBox_recoverState.Size = new System.Drawing.Size(156, 16);
            this.checkBox_recoverState.TabIndex = 23;
            this.checkBox_recoverState.Text = "keys 使用日志恢复版(&R)";
            this.checkBox_recoverState.UseVisualStyleBackColor = true;
            this.checkBox_recoverState.Visible = false;
            // 
            // RefreshStyleDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(290, 328);
            this.Controls.Add(this.checkBox_recoverState);
            this.Controls.Add(this.checkBox_autoRebuildKeys);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
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
    }
}