namespace dp2Circulation.Testing
{
    partial class UcsUploadDialog
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
            this.textBox_record = new System.Windows.Forms.TextBox();
            this.button_upload = new System.Windows.Forms.Button();
            this.button_pasteFromJinei = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_action = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBox_record
            // 
            this.textBox_record.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_record.Location = new System.Drawing.Point(13, 114);
            this.textBox_record.Multiline = true;
            this.textBox_record.Name = "textBox_record";
            this.textBox_record.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_record.Size = new System.Drawing.Size(907, 421);
            this.textBox_record.TabIndex = 0;
            // 
            // button_upload
            // 
            this.button_upload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_upload.Location = new System.Drawing.Point(13, 542);
            this.button_upload.Name = "button_upload";
            this.button_upload.Size = new System.Drawing.Size(156, 41);
            this.button_upload.TabIndex = 1;
            this.button_upload.Text = "上传";
            this.button_upload.UseVisualStyleBackColor = true;
            this.button_upload.Click += new System.EventHandler(this.button_upload_Click);
            // 
            // button_pasteFromJinei
            // 
            this.button_pasteFromJinei.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_pasteFromJinei.Location = new System.Drawing.Point(719, 542);
            this.button_pasteFromJinei.Name = "button_pasteFromJinei";
            this.button_pasteFromJinei.Size = new System.Drawing.Size(201, 41);
            this.button_pasteFromJinei.TabIndex = 2;
            this.button_pasteFromJinei.Text = "粘贴机内格式";
            this.button_pasteFromJinei.UseVisualStyleBackColor = true;
            this.button_pasteFromJinei.Click += new System.EventHandler(this.button_pasteFromJinei_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "动作:";
            // 
            // comboBox_action
            // 
            this.comboBox_action.FormattingEnabled = true;
            this.comboBox_action.Items.AddRange(new object[] {
            "N 新增",
            "U 更新"});
            this.comboBox_action.Location = new System.Drawing.Point(144, 44);
            this.comboBox_action.Name = "comboBox_action";
            this.comboBox_action.Size = new System.Drawing.Size(263, 29);
            this.comboBox_action.TabIndex = 4;
            // 
            // UcsUploadDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 648);
            this.Controls.Add(this.comboBox_action);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_pasteFromJinei);
            this.Controls.Add(this.button_upload);
            this.Controls.Add(this.textBox_record);
            this.Name = "UcsUploadDialog";
            this.Text = "UcsUploadDialog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UcsUploadDialog_FormClosed);
            this.Load += new System.EventHandler(this.UcsUploadDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_record;
        private System.Windows.Forms.Button button_upload;
        private System.Windows.Forms.Button button_pasteFromJinei;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_action;
    }
}