namespace DigitalPlatform.Install
{
    partial class DataDirDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataDirDlg));
            this.button_findDataDir = new System.Windows.Forms.Button();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_findDataDir
            // 
            this.button_findDataDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findDataDir.Location = new System.Drawing.Point(269, 109);
            this.button_findDataDir.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_findDataDir.Name = "button_findDataDir";
            this.button_findDataDir.Size = new System.Drawing.Size(32, 21);
            this.button_findDataDir.TabIndex = 2;
            this.button_findDataDir.Text = "...";
            this.button_findDataDir.UseVisualStyleBackColor = true;
            this.button_findDataDir.Visible = false;
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.Location = new System.Drawing.Point(9, 109);
            this.textBox_dataDir.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(257, 21);
            this.textBox_dataDir.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 94);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "数据目录(&D):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(244, 157);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(184, 157);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(10, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.Size = new System.Drawing.Size(291, 71);
            this.textBox_message.TabIndex = 5;
            // 
            // DataDirDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 189);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.button_findDataDir);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DataDirDlg";
            this.Text = "请指定数据目录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_findDataDir;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_message;
    }
}