namespace dp2Circulation
{
    partial class SetReaderBarcodeNumberDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetReaderBarcodeNumberDialog));
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_yes = new System.Windows.Forms.Button();
            this.button_no = new System.Windows.Forms.Button();
            this.checkBox_dontAsk = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(12, 12);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_message.Size = new System.Drawing.Size(356, 82);
            this.textBox_message.TabIndex = 0;
            this.textBox_message.Text = "是否要将 身份证号 直接设置为 证条码号?\r\n\r\n(这种方式不用给读者办借阅证卡，读者今后在图书馆直接使用身份证即可)";
            // 
            // button_yes
            // 
            this.button_yes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_yes.Location = new System.Drawing.Point(212, 156);
            this.button_yes.Name = "button_yes";
            this.button_yes.Size = new System.Drawing.Size(75, 23);
            this.button_yes.TabIndex = 2;
            this.button_yes.Text = "是";
            this.button_yes.UseVisualStyleBackColor = true;
            this.button_yes.Click += new System.EventHandler(this.button_yes_Click);
            // 
            // button_no
            // 
            this.button_no.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_no.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_no.Location = new System.Drawing.Point(293, 156);
            this.button_no.Name = "button_no";
            this.button_no.Size = new System.Drawing.Size(75, 23);
            this.button_no.TabIndex = 3;
            this.button_no.Text = "否";
            this.button_no.UseVisualStyleBackColor = true;
            this.button_no.Click += new System.EventHandler(this.button_no_Click);
            // 
            // checkBox_dontAsk
            // 
            this.checkBox_dontAsk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_dontAsk.AutoSize = true;
            this.checkBox_dontAsk.Location = new System.Drawing.Point(12, 100);
            this.checkBox_dontAsk.Name = "checkBox_dontAsk";
            this.checkBox_dontAsk.Size = new System.Drawing.Size(300, 16);
            this.checkBox_dontAsk.TabIndex = 1;
            this.checkBox_dontAsk.Text = "不再出现此对话框，按最后一次选定的方式自动处理";
            this.checkBox_dontAsk.UseVisualStyleBackColor = true;
            // 
            // SetReaderBarcodeNumberDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 187);
            this.Controls.Add(this.checkBox_dontAsk);
            this.Controls.Add(this.button_no);
            this.Controls.Add(this.button_yes);
            this.Controls.Add(this.textBox_message);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetReaderBarcodeNumberDialog";
            this.ShowInTaskbar = false;
            this.Text = "用身份证号作为证条码号";
            this.Load += new System.EventHandler(this.SetReaderBarcodeNumberDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_yes;
        private System.Windows.Forms.Button button_no;
        private System.Windows.Forms.CheckBox checkBox_dontAsk;
    }
}