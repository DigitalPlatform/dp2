namespace dp2Circulation
{
    partial class OpenPatronXmlFileDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_xmlFileName = new System.Windows.Forms.TextBox();
            this.button_getFileName = new System.Windows.Forms.Button();
            this.button_getObjectDirectoryName = new System.Windows.Forms.Button();
            this.textBox_objectDirectoryName = new System.Windows.Forms.TextBox();
            this.label_objectDirectoryName = new System.Windows.Forms.Label();
            this.checkBox_includeObjectFile = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_mimeFileExtension = new System.Windows.Forms.CheckBox();
            this.checkBox_usageFileExtension = new System.Windows.Forms.CheckBox();
            this.checkBox_backup = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者 XML 文件名(&F):";
            // 
            // textBox_xmlFileName
            // 
            this.textBox_xmlFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_xmlFileName.Location = new System.Drawing.Point(22, 54);
            this.textBox_xmlFileName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_xmlFileName.Name = "textBox_xmlFileName";
            this.textBox_xmlFileName.Size = new System.Drawing.Size(693, 31);
            this.textBox_xmlFileName.TabIndex = 1;
            this.textBox_xmlFileName.TextChanged += new System.EventHandler(this.textBox_biblioDumpFileName_TextChanged);
            // 
            // button_getFileName
            // 
            this.button_getFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getFileName.Location = new System.Drawing.Point(730, 54);
            this.button_getFileName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_getFileName.Name = "button_getFileName";
            this.button_getFileName.Size = new System.Drawing.Size(88, 40);
            this.button_getFileName.TabIndex = 2;
            this.button_getFileName.Text = "...";
            this.button_getFileName.UseVisualStyleBackColor = true;
            this.button_getFileName.Click += new System.EventHandler(this.button_getPatronXmlFileName_Click);
            // 
            // button_getObjectDirectoryName
            // 
            this.button_getObjectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getObjectDirectoryName.Location = new System.Drawing.Point(730, 334);
            this.button_getObjectDirectoryName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_getObjectDirectoryName.Name = "button_getObjectDirectoryName";
            this.button_getObjectDirectoryName.Size = new System.Drawing.Size(88, 40);
            this.button_getObjectDirectoryName.TabIndex = 6;
            this.button_getObjectDirectoryName.Text = "...";
            this.button_getObjectDirectoryName.UseVisualStyleBackColor = true;
            this.button_getObjectDirectoryName.Click += new System.EventHandler(this.button_getObjectDirectoryName_Click);
            // 
            // textBox_objectDirectoryName
            // 
            this.textBox_objectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectDirectoryName.Location = new System.Drawing.Point(66, 338);
            this.textBox_objectDirectoryName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_objectDirectoryName.Name = "textBox_objectDirectoryName";
            this.textBox_objectDirectoryName.ReadOnly = true;
            this.textBox_objectDirectoryName.Size = new System.Drawing.Size(649, 31);
            this.textBox_objectDirectoryName.TabIndex = 5;
            // 
            // label_objectDirectoryName
            // 
            this.label_objectDirectoryName.AutoSize = true;
            this.label_objectDirectoryName.Location = new System.Drawing.Point(62, 312);
            this.label_objectDirectoryName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label_objectDirectoryName.Name = "label_objectDirectoryName";
            this.label_objectDirectoryName.Size = new System.Drawing.Size(180, 21);
            this.label_objectDirectoryName.TabIndex = 4;
            this.label_objectDirectoryName.Text = "对象文件目录(&O):";
            // 
            // checkBox_includeObjectFile
            // 
            this.checkBox_includeObjectFile.AutoSize = true;
            this.checkBox_includeObjectFile.Checked = true;
            this.checkBox_includeObjectFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeObjectFile.Location = new System.Drawing.Point(22, 278);
            this.checkBox_includeObjectFile.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_includeObjectFile.Name = "checkBox_includeObjectFile";
            this.checkBox_includeObjectFile.Size = new System.Drawing.Size(195, 25);
            this.checkBox_includeObjectFile.TabIndex = 3;
            this.checkBox_includeObjectFile.Text = "包含对象文件(&O)";
            this.checkBox_includeObjectFile.UseVisualStyleBackColor = true;
            this.checkBox_includeObjectFile.CheckedChanged += new System.EventHandler(this.checkBox_includeObjectFile_CheckedChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(680, 432);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(532, 432);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_mimeFileExtension
            // 
            this.checkBox_mimeFileExtension.AutoSize = true;
            this.checkBox_mimeFileExtension.Location = new System.Drawing.Point(66, 379);
            this.checkBox_mimeFileExtension.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_mimeFileExtension.Name = "checkBox_mimeFileExtension";
            this.checkBox_mimeFileExtension.Size = new System.Drawing.Size(240, 25);
            this.checkBox_mimeFileExtension.TabIndex = 7;
            this.checkBox_mimeFileExtension.Text = "采用 MIME 扩展名(&M)";
            this.checkBox_mimeFileExtension.UseVisualStyleBackColor = true;
            // 
            // checkBox_usageFileExtension
            // 
            this.checkBox_usageFileExtension.AutoSize = true;
            this.checkBox_usageFileExtension.Location = new System.Drawing.Point(343, 379);
            this.checkBox_usageFileExtension.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_usageFileExtension.Name = "checkBox_usageFileExtension";
            this.checkBox_usageFileExtension.Size = new System.Drawing.Size(251, 25);
            this.checkBox_usageFileExtension.TabIndex = 8;
            this.checkBox_usageFileExtension.Text = "采用 usage 扩展名(&U)";
            this.checkBox_usageFileExtension.UseVisualStyleBackColor = true;
            // 
            // checkBox_backup
            // 
            this.checkBox_backup.AutoSize = true;
            this.checkBox_backup.Location = new System.Drawing.Point(22, 115);
            this.checkBox_backup.Name = "checkBox_backup";
            this.checkBox_backup.Size = new System.Drawing.Size(153, 25);
            this.checkBox_backup.TabIndex = 11;
            this.checkBox_backup.Text = "备份数据(&B)";
            this.checkBox_backup.UseVisualStyleBackColor = true;
            // 
            // OpenPatronXmlFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(840, 494);
            this.Controls.Add(this.checkBox_backup);
            this.Controls.Add(this.checkBox_usageFileExtension);
            this.Controls.Add(this.checkBox_mimeFileExtension);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.checkBox_includeObjectFile);
            this.Controls.Add(this.button_getObjectDirectoryName);
            this.Controls.Add(this.textBox_objectDirectoryName);
            this.Controls.Add(this.label_objectDirectoryName);
            this.Controls.Add(this.button_getFileName);
            this.Controls.Add(this.textBox_xmlFileName);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "OpenPatronXmlFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "打开读者 XML 文件";
            this.Load += new System.EventHandler(this.OpenPatronXmlFileDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_xmlFileName;
        private System.Windows.Forms.Button button_getFileName;
        private System.Windows.Forms.Button button_getObjectDirectoryName;
        private System.Windows.Forms.TextBox textBox_objectDirectoryName;
        private System.Windows.Forms.Label label_objectDirectoryName;
        private System.Windows.Forms.CheckBox checkBox_includeObjectFile;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_mimeFileExtension;
        private System.Windows.Forms.CheckBox checkBox_usageFileExtension;
        private System.Windows.Forms.CheckBox checkBox_backup;
    }
}