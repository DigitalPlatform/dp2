namespace DigitalPlatform.RFID.UI
{
    partial class DuplicateUhfTagDialog
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_epcBankHex = new System.Windows.Forms.TextBox();
            this.textBox_userBankHex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cancel.Location = new System.Drawing.Point(644, 399);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(143, 38);
            this.button_cancel.TabIndex = 10;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(644, 353);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(143, 38);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "写入";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 21);
            this.label1.TabIndex = 11;
            this.label1.Text = "EPC Bank(Hex):";
            // 
            // textBox_epcBankHex
            // 
            this.textBox_epcBankHex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_epcBankHex.Location = new System.Drawing.Point(12, 37);
            this.textBox_epcBankHex.Name = "textBox_epcBankHex";
            this.textBox_epcBankHex.Size = new System.Drawing.Size(776, 31);
            this.textBox_epcBankHex.TabIndex = 12;
            // 
            // textBox_userBankHex
            // 
            this.textBox_userBankHex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_userBankHex.Location = new System.Drawing.Point(12, 109);
            this.textBox_userBankHex.Multiline = true;
            this.textBox_userBankHex.Name = "textBox_userBankHex";
            this.textBox_userBankHex.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_userBankHex.Size = new System.Drawing.Size(776, 219);
            this.textBox_userBankHex.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(175, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "User Bank(Hex):";
            // 
            // DuplicateUhfTagDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_userBankHex);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_epcBankHex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "DuplicateUhfTagDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "复制创建 UHF 标签";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DuplicateUhfTagDialog_FormClosed);
            this.Load += new System.EventHandler(this.DuplicateUhfTagDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_epcBankHex;
        private System.Windows.Forms.TextBox textBox_userBankHex;
        private System.Windows.Forms.Label label2;
    }
}