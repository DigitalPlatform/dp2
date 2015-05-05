namespace DigitalPlatform.OPAC
{
    partial class OneInstanceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OneInstanceDialog));
            this.button_setSerialNumber = new System.Windows.Forms.Button();
            this.button_certificate = new System.Windows.Forms.Button();
            this.button_editDp2LibraryDef = new System.Windows.Forms.Button();
            this.textBox_dp2LibraryDef = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_site = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_setSerialNumber
            // 
            this.button_setSerialNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_setSerialNumber.Location = new System.Drawing.Point(103, 243);
            this.button_setSerialNumber.Name = "button_setSerialNumber";
            this.button_setSerialNumber.Size = new System.Drawing.Size(95, 23);
            this.button_setSerialNumber.TabIndex = 39;
            this.button_setSerialNumber.Text = "序列号(&S)...";
            this.button_setSerialNumber.UseVisualStyleBackColor = true;
            this.button_setSerialNumber.Visible = false;
            // 
            // button_certificate
            // 
            this.button_certificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_certificate.Location = new System.Drawing.Point(12, 243);
            this.button_certificate.Name = "button_certificate";
            this.button_certificate.Size = new System.Drawing.Size(85, 23);
            this.button_certificate.TabIndex = 35;
            this.button_certificate.Text = "证书(&C)...";
            this.button_certificate.UseVisualStyleBackColor = true;
            this.button_certificate.Visible = false;
            // 
            // button_editDp2LibraryDef
            // 
            this.button_editDp2LibraryDef.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editDp2LibraryDef.Location = new System.Drawing.Point(387, 107);
            this.button_editDp2LibraryDef.Name = "button_editDp2LibraryDef";
            this.button_editDp2LibraryDef.Size = new System.Drawing.Size(45, 23);
            this.button_editDp2LibraryDef.TabIndex = 26;
            this.button_editDp2LibraryDef.Text = "...";
            this.button_editDp2LibraryDef.UseVisualStyleBackColor = true;
            this.button_editDp2LibraryDef.Click += new System.EventHandler(this.button_editDp2LibraryDef_Click);
            // 
            // textBox_dp2LibraryDef
            // 
            this.textBox_dp2LibraryDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2LibraryDef.Location = new System.Drawing.Point(155, 107);
            this.textBox_dp2LibraryDef.Multiline = true;
            this.textBox_dp2LibraryDef.Name = "textBox_dp2LibraryDef";
            this.textBox_dp2LibraryDef.ReadOnly = true;
            this.textBox_dp2LibraryDef.Size = new System.Drawing.Size(226, 113);
            this.textBox_dp2LibraryDef.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 12);
            this.label4.TabIndex = 24;
            this.label4.Text = "dp2Library 服务器(&L):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(358, 243);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 37;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(277, 243);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 36;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.Location = new System.Drawing.Point(155, 71);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(226, 21);
            this.textBox_dataDir.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 22;
            this.label2.Text = "数据目录(&D):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Location = new System.Drawing.Point(155, 38);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(165, 21);
            this.textBox_instanceName.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "虚拟目录名(&V):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 40;
            this.label7.Text = "网站(&S):";
            // 
            // comboBox_site
            // 
            this.comboBox_site.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_site.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_site.FormattingEnabled = true;
            this.comboBox_site.Location = new System.Drawing.Point(155, 12);
            this.comboBox_site.Name = "comboBox_site";
            this.comboBox_site.Size = new System.Drawing.Size(226, 20);
            this.comboBox_site.TabIndex = 41;
            this.comboBox_site.SelectedIndexChanged += new System.EventHandler(this.comboBox_site_SelectedIndexChanged);
            // 
            // OneInstanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(445, 278);
            this.Controls.Add(this.comboBox_site);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button_setSerialNumber);
            this.Controls.Add(this.button_certificate);
            this.Controls.Add(this.button_editDp2LibraryDef);
            this.Controls.Add(this.textBox_dp2LibraryDef);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_instanceName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OneInstanceDialog";
            this.ShowInTaskbar = false;
            this.Text = "OneInstanceDialog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OneInstanceDialog_FormClosed);
            this.Load += new System.EventHandler(this.OneInstanceDialog_Load);
            this.Move += new System.EventHandler(this.OneInstanceDialog_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_setSerialNumber;
        private System.Windows.Forms.Button button_certificate;
        private System.Windows.Forms.Button button_editDp2LibraryDef;
        private System.Windows.Forms.TextBox textBox_dp2LibraryDef;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_site;
    }
}