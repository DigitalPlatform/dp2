namespace dp2rms
{
    partial class OverwriteOrAppendBackupFileDlg
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
            this.label_message = new System.Windows.Forms.Label();
            this.button_append = new System.Windows.Forms.Button();
            this.button_overwrite = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkBox_keepAppend = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(13, 13);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(398, 102);
            this.label_message.TabIndex = 0;
            // 
            // button_append
            // 
            this.button_append.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_append.Location = new System.Drawing.Point(174, 174);
            this.button_append.Name = "button_append";
            this.button_append.Size = new System.Drawing.Size(75, 23);
            this.button_append.TabIndex = 1;
            this.button_append.Text = "追加(&A)";
            this.button_append.UseVisualStyleBackColor = true;
            this.button_append.Click += new System.EventHandler(this.button_append_Click);
            // 
            // button_overwrite
            // 
            this.button_overwrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_overwrite.Location = new System.Drawing.Point(255, 174);
            this.button_overwrite.Name = "button_overwrite";
            this.button_overwrite.Size = new System.Drawing.Size(75, 23);
            this.button_overwrite.TabIndex = 2;
            this.button_overwrite.Text = "覆盖(&O)";
            this.button_overwrite.UseVisualStyleBackColor = true;
            this.button_overwrite.Click += new System.EventHandler(this.button_overwrite_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(336, 174);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "放弃";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // checkBox_keepAppend
            // 
            this.checkBox_keepAppend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_keepAppend.AutoSize = true;
            this.checkBox_keepAppend.Location = new System.Drawing.Point(16, 144);
            this.checkBox_keepAppend.Name = "checkBox_keepAppend";
            this.checkBox_keepAppend.Size = new System.Drawing.Size(260, 19);
            this.checkBox_keepAppend.TabIndex = 4;
            this.checkBox_keepAppend.Text = "下次追加不再出现此对话框询问(&K)";
            this.checkBox_keepAppend.UseVisualStyleBackColor = true;
            // 
            // OverwriteOrAppendBackupFileDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 209);
            this.Controls.Add(this.checkBox_keepAppend);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_overwrite);
            this.Controls.Add(this.button_append);
            this.Controls.Add(this.label_message);
            this.Name = "OverwriteOrAppendBackupFileDlg";
            this.Text = "OverwriteOrAppendBackupFileDlg";
            this.Load += new System.EventHandler(this.OverwriteOrAppendBackupFileDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_append;
        private System.Windows.Forms.Button button_overwrite;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckBox checkBox_keepAppend;
    }
}