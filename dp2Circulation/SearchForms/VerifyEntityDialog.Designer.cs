namespace dp2Circulation
{
    partial class VerifyEntityDialog
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
            this.checkBox_autoModify = new System.Windows.Forms.CheckBox();
            this.checkBox_verifyItemBarcode = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_serverVerify = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBox_autoModify
            // 
            this.checkBox_autoModify.AutoSize = true;
            this.checkBox_autoModify.Location = new System.Drawing.Point(13, 13);
            this.checkBox_autoModify.Name = "checkBox_autoModify";
            this.checkBox_autoModify.Size = new System.Drawing.Size(72, 16);
            this.checkBox_autoModify.TabIndex = 0;
            this.checkBox_autoModify.Text = "自动修正";
            this.checkBox_autoModify.UseVisualStyleBackColor = true;
            // 
            // checkBox_verifyItemBarcode
            // 
            this.checkBox_verifyItemBarcode.AutoSize = true;
            this.checkBox_verifyItemBarcode.Location = new System.Drawing.Point(13, 64);
            this.checkBox_verifyItemBarcode.Name = "checkBox_verifyItemBarcode";
            this.checkBox_verifyItemBarcode.Size = new System.Drawing.Size(96, 16);
            this.checkBox_verifyItemBarcode.TabIndex = 1;
            this.checkBox_verifyItemBarcode.Text = "校验册条码号";
            this.checkBox_verifyItemBarcode.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(198, 227);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(119, 227);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_serverVerify
            // 
            this.checkBox_serverVerify.AutoSize = true;
            this.checkBox_serverVerify.Location = new System.Drawing.Point(13, 86);
            this.checkBox_serverVerify.Name = "checkBox_serverVerify";
            this.checkBox_serverVerify.Size = new System.Drawing.Size(96, 16);
            this.checkBox_serverVerify.TabIndex = 12;
            this.checkBox_serverVerify.Text = "服务器端校验";
            this.checkBox_serverVerify.UseVisualStyleBackColor = true;
            // 
            // VerifyEntityDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.checkBox_serverVerify);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_verifyItemBarcode);
            this.Controls.Add(this.checkBox_autoModify);
            this.Name = "VerifyEntityDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "指定 校验册记录 特性";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_autoModify;
        private System.Windows.Forms.CheckBox checkBox_verifyItemBarcode;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_serverVerify;
    }
}