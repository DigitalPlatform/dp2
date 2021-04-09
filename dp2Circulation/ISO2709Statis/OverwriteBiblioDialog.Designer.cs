namespace dp2Circulation
{
    partial class OverwriteBiblioDialog
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_overwrite = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_skip = new System.Windows.Forms.Button();
            this.checkBox_dontAsk = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(26, 28);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(44, 47);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1003, 483);
            this.webBrowser1.TabIndex = 0;
            // 
            // button_overwrite
            // 
            this.button_overwrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_overwrite.Location = new System.Drawing.Point(26, 526);
            this.button_overwrite.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.button_overwrite.Name = "button_overwrite";
            this.button_overwrite.Size = new System.Drawing.Size(183, 53);
            this.button_overwrite.TabIndex = 1;
            this.button_overwrite.Text = "覆盖";
            this.button_overwrite.UseVisualStyleBackColor = true;
            this.button_overwrite.Click += new System.EventHandler(this.button_overwrite_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(830, 526);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(200, 53);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "中断批处理(&C)";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_skip
            // 
            this.button_skip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_skip.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_skip.Location = new System.Drawing.Point(223, 526);
            this.button_skip.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.button_skip.Name = "button_skip";
            this.button_skip.Size = new System.Drawing.Size(193, 53);
            this.button_skip.TabIndex = 2;
            this.button_skip.Text = "跳过(&I)";
            this.button_skip.UseVisualStyleBackColor = true;
            this.button_skip.Click += new System.EventHandler(this.button_skip_Click);
            // 
            // checkBox_dontAsk
            // 
            this.checkBox_dontAsk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_dontAsk.AutoSize = true;
            this.checkBox_dontAsk.Location = new System.Drawing.Point(428, 545);
            this.checkBox_dontAsk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_dontAsk.Name = "checkBox_dontAsk";
            this.checkBox_dontAsk.Size = new System.Drawing.Size(236, 32);
            this.checkBox_dontAsk.TabIndex = 4;
            this.checkBox_dontAsk.Text = "不再出现本对话框(&D)";
            this.checkBox_dontAsk.UseVisualStyleBackColor = true;
            // 
            // OverwriteBiblioDialog
            // 
            this.AcceptButton = this.button_overwrite;
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(1055, 607);
            this.Controls.Add(this.checkBox_dontAsk);
            this.Controls.Add(this.button_skip);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_overwrite);
            this.Controls.Add(this.webBrowser1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.Name = "OverwriteBiblioDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "即将覆盖书目记录";
            this.Load += new System.EventHandler(this.OverwriteBiblioDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        internal System.Windows.Forms.Button button_overwrite;
        internal System.Windows.Forms.Button button_Cancel;
        internal System.Windows.Forms.Button button_skip;
        private System.Windows.Forms.CheckBox checkBox_dontAsk;
    }
}