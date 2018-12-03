namespace dp2Circulation
{
    partial class RunScriptDialog
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
            this.button_getScriptFileName = new System.Windows.Forms.Button();
            this.textBox_scriptFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_forceSave = new System.Windows.Forms.CheckBox();
            this.checkBox_autoSaveChanges = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_getScriptFileName
            // 
            this.button_getScriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getScriptFileName.Location = new System.Drawing.Point(601, 31);
            this.button_getScriptFileName.Margin = new System.Windows.Forms.Padding(4);
            this.button_getScriptFileName.Name = "button_getScriptFileName";
            this.button_getScriptFileName.Size = new System.Drawing.Size(72, 34);
            this.button_getScriptFileName.TabIndex = 5;
            this.button_getScriptFileName.Text = "...";
            this.button_getScriptFileName.UseVisualStyleBackColor = true;
            this.button_getScriptFileName.Click += new System.EventHandler(this.button_getScriptFileName_Click);
            // 
            // textBox_scriptFileName
            // 
            this.textBox_scriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_scriptFileName.Location = new System.Drawing.Point(16, 31);
            this.textBox_scriptFileName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_scriptFileName.Name = "textBox_scriptFileName";
            this.textBox_scriptFileName.Size = new System.Drawing.Size(577, 28);
            this.textBox_scriptFileName.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "脚本程序文件名(&S):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(440, 151);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 13;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(561, 151);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(112, 34);
            this.button_Cancel.TabIndex = 14;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_forceSave
            // 
            this.checkBox_forceSave.AutoSize = true;
            this.checkBox_forceSave.Location = new System.Drawing.Point(227, 67);
            this.checkBox_forceSave.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_forceSave.Name = "checkBox_forceSave";
            this.checkBox_forceSave.Size = new System.Drawing.Size(133, 22);
            this.checkBox_forceSave.TabIndex = 16;
            this.checkBox_forceSave.Text = "强制保存(&F)";
            this.checkBox_forceSave.UseVisualStyleBackColor = true;
            // 
            // checkBox_autoSaveChanges
            // 
            this.checkBox_autoSaveChanges.AutoSize = true;
            this.checkBox_autoSaveChanges.Location = new System.Drawing.Point(16, 67);
            this.checkBox_autoSaveChanges.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_autoSaveChanges.Name = "checkBox_autoSaveChanges";
            this.checkBox_autoSaveChanges.Size = new System.Drawing.Size(169, 22);
            this.checkBox_autoSaveChanges.TabIndex = 15;
            this.checkBox_autoSaveChanges.Text = "自动保存修改(&S)";
            this.checkBox_autoSaveChanges.UseVisualStyleBackColor = true;
            this.checkBox_autoSaveChanges.CheckedChanged += new System.EventHandler(this.checkBox_autoSaveChanges_CheckedChanged);
            // 
            // RunScriptDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(686, 198);
            this.Controls.Add(this.checkBox_forceSave);
            this.Controls.Add(this.checkBox_autoSaveChanges);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_getScriptFileName);
            this.Controls.Add(this.textBox_scriptFileName);
            this.Controls.Add(this.label1);
            this.Name = "RunScriptDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "执行 C# 脚本";
            this.Load += new System.EventHandler(this.RunScriptDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_getScriptFileName;
        private System.Windows.Forms.TextBox textBox_scriptFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_forceSave;
        private System.Windows.Forms.CheckBox checkBox_autoSaveChanges;
    }
}