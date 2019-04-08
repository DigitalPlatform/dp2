namespace DigitalPlatform.Z3950
{
    partial class RecordSyntaxAndEncodingBindingItemDlg
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
            this.comboBox_recordSyntax = new System.Windows.Forms.ComboBox();
            this.comboBox_encoding = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据格式(&S):";
            // 
            // comboBox_recordSyntax
            // 
            this.comboBox_recordSyntax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_recordSyntax.DropDownHeight = 300;
            this.comboBox_recordSyntax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_recordSyntax.FormattingEnabled = true;
            this.comboBox_recordSyntax.IntegralHeight = false;
            this.comboBox_recordSyntax.Items.AddRange(new object[] {
            "1.2.840.10003.5.1 -- UNIMARC",
            "1.2.840.10003.5.10 -- MARC21",
            "1.2.840.10003.5.109.10 -- XML"});
            this.comboBox_recordSyntax.Location = new System.Drawing.Point(134, 13);
            this.comboBox_recordSyntax.Name = "comboBox_recordSyntax";
            this.comboBox_recordSyntax.Size = new System.Drawing.Size(293, 23);
            this.comboBox_recordSyntax.TabIndex = 1;
            // 
            // comboBox_encoding
            // 
            this.comboBox_encoding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_encoding.DropDownHeight = 300;
            this.comboBox_encoding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_encoding.FormattingEnabled = true;
            this.comboBox_encoding.IntegralHeight = false;
            this.comboBox_encoding.Location = new System.Drawing.Point(134, 42);
            this.comboBox_encoding.Name = "comboBox_encoding";
            this.comboBox_encoding.Size = new System.Drawing.Size(293, 23);
            this.comboBox_encoding.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "编码方式(&E):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(271, 102);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(352, 102);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // RecordSyntaxAndEncodingBindingItemDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 142);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_encoding);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_recordSyntax);
            this.Controls.Add(this.label1);
            this.Name = "RecordSyntaxAndEncodingBindingItemDlg";
            this.Text = "一个数据格式和编码方式绑定事项";
            this.Load += new System.EventHandler(this.RecordSyntaxAndEncodingBindingItemDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_recordSyntax;
        private System.Windows.Forms.ComboBox comboBox_encoding;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}