namespace DigitalPlatform.rms.Client
{
    partial class ImportDataDialog
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
            this.textBox_fileName = new System.Windows.Forms.TextBox();
            this.button_findFileName = new System.Windows.Forms.Button();
            this.checkBox_importDataRecord = new System.Windows.Forms.CheckBox();
            this.checkBox_importObject = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_insertMissing = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件名(&F):";
            // 
            // textBox_fileName
            // 
            this.textBox_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fileName.Location = new System.Drawing.Point(12, 31);
            this.textBox_fileName.Name = "textBox_fileName";
            this.textBox_fileName.Size = new System.Drawing.Size(346, 21);
            this.textBox_fileName.TabIndex = 1;
            // 
            // button_findFileName
            // 
            this.button_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findFileName.Location = new System.Drawing.Point(364, 29);
            this.button_findFileName.Name = "button_findFileName";
            this.button_findFileName.Size = new System.Drawing.Size(42, 23);
            this.button_findFileName.TabIndex = 2;
            this.button_findFileName.Text = "...";
            this.button_findFileName.UseVisualStyleBackColor = true;
            this.button_findFileName.Click += new System.EventHandler(this.button_findFileName_Click);
            // 
            // checkBox_importDataRecord
            // 
            this.checkBox_importDataRecord.AutoSize = true;
            this.checkBox_importDataRecord.Checked = true;
            this.checkBox_importDataRecord.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_importDataRecord.Location = new System.Drawing.Point(12, 101);
            this.checkBox_importDataRecord.Name = "checkBox_importDataRecord";
            this.checkBox_importDataRecord.Size = new System.Drawing.Size(114, 16);
            this.checkBox_importDataRecord.TabIndex = 3;
            this.checkBox_importDataRecord.Text = "导入数据记录(&D)";
            this.checkBox_importDataRecord.UseVisualStyleBackColor = true;
            this.checkBox_importDataRecord.CheckedChanged += new System.EventHandler(this.checkBox_importDataRecord_CheckedChanged);
            // 
            // checkBox_importObject
            // 
            this.checkBox_importObject.AutoSize = true;
            this.checkBox_importObject.Checked = true;
            this.checkBox_importObject.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_importObject.Location = new System.Drawing.Point(12, 149);
            this.checkBox_importObject.Name = "checkBox_importObject";
            this.checkBox_importObject.Size = new System.Drawing.Size(114, 16);
            this.checkBox_importObject.TabIndex = 4;
            this.checkBox_importObject.Text = "导入数字对象(&D)";
            this.checkBox_importObject.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(319, 225);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(87, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(226, 225);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(87, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_insertMissing
            // 
            this.checkBox_insertMissing.AutoSize = true;
            this.checkBox_insertMissing.Location = new System.Drawing.Point(30, 123);
            this.checkBox_insertMissing.Name = "checkBox_insertMissing";
            this.checkBox_insertMissing.Size = new System.Drawing.Size(198, 16);
            this.checkBox_insertMissing.TabIndex = 7;
            this.checkBox_insertMissing.Text = "仅当数据记录不存在时才写入(&I)";
            this.checkBox_insertMissing.UseVisualStyleBackColor = true;
            // 
            // ImportDataDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(418, 261);
            this.Controls.Add(this.checkBox_insertMissing);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_importObject);
            this.Controls.Add(this.checkBox_importDataRecord);
            this.Controls.Add(this.button_findFileName);
            this.Controls.Add(this.textBox_fileName);
            this.Controls.Add(this.label1);
            this.Name = "ImportDataDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "导入数据";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_fileName;
        private System.Windows.Forms.Button button_findFileName;
        private System.Windows.Forms.CheckBox checkBox_importDataRecord;
        private System.Windows.Forms.CheckBox checkBox_importObject;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_insertMissing;
    }
}