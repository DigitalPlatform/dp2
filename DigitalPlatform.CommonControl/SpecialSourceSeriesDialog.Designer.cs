namespace DigitalPlatform.CommonControl
{
    partial class SpecialSourceSeriesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpecialSourceSeriesDialog));
            this.label_address = new System.Windows.Forms.Label();
            this.comboBox_source = new System.Windows.Forms.ComboBox();
            this.label_source = new System.Windows.Forms.Label();
            this.comboBox_seller = new System.Windows.Forms.ComboBox();
            this.label_seller = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.comboBox_specialSource = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.personAddressControl = new DigitalPlatform.CommonControl.PersonAddressControl();
            this.SuspendLayout();
            // 
            // label_address
            // 
            this.label_address.AutoSize = true;
            this.label_address.Location = new System.Drawing.Point(9, 115);
            this.label_address.Name = "label_address";
            this.label_address.Size = new System.Drawing.Size(99, 15);
            this.label_address.TabIndex = 0;
            this.label_address.Text = "渠道地址(&A):";
            // 
            // comboBox_source
            // 
            this.comboBox_source.FormattingEnabled = true;
            this.comboBox_source.Location = new System.Drawing.Point(122, 79);
            this.comboBox_source.Name = "comboBox_source";
            this.comboBox_source.Size = new System.Drawing.Size(195, 23);
            this.comboBox_source.TabIndex = 7;
            this.comboBox_source.DropDown += new System.EventHandler(this.comboBox_source_DropDown);
            // 
            // label_source
            // 
            this.label_source.AutoSize = true;
            this.label_source.Location = new System.Drawing.Point(9, 82);
            this.label_source.Name = "label_source";
            this.label_source.Size = new System.Drawing.Size(99, 15);
            this.label_source.TabIndex = 6;
            this.label_source.Text = "经费来源(&R):";
            // 
            // comboBox_seller
            // 
            this.comboBox_seller.FormattingEnabled = true;
            this.comboBox_seller.Location = new System.Drawing.Point(122, 51);
            this.comboBox_seller.Name = "comboBox_seller";
            this.comboBox_seller.Size = new System.Drawing.Size(195, 23);
            this.comboBox_seller.TabIndex = 5;
            this.comboBox_seller.DropDown += new System.EventHandler(this.comboBox_seller_DropDown);
            // 
            // label_seller
            // 
            this.label_seller.AutoSize = true;
            this.label_seller.Location = new System.Drawing.Point(9, 54);
            this.label_seller.Name = "label_seller";
            this.label_seller.Size = new System.Drawing.Size(84, 15);
            this.label_seller.TabIndex = 4;
            this.label_seller.Text = "渠道名(&S):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(407, 330);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(326, 330);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // comboBox_specialSource
            // 
            this.comboBox_specialSource.FormattingEnabled = true;
            this.comboBox_specialSource.Items.AddRange(new object[] {
            "直订",
            "交换",
            "赠",
            "普通"});
            this.comboBox_specialSource.Location = new System.Drawing.Point(122, 12);
            this.comboBox_specialSource.Name = "comboBox_specialSource";
            this.comboBox_specialSource.Size = new System.Drawing.Size(152, 23);
            this.comboBox_specialSource.TabIndex = 12;
            this.comboBox_specialSource.Text = "普通";
            this.comboBox_specialSource.TextChanged += new System.EventHandler(this.comboBox_specialSource_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(99, 15);
            this.label7.TabIndex = 13;
            this.label7.Text = "渠道类型(&T):";
            // 
            // personAddressControl
            // 
            this.personAddressControl.Accounts = "";
            this.personAddressControl.Address = "";
            this.personAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.personAddressControl.Bank = "";
            this.personAddressControl.Changed = false;
            this.personAddressControl.Comment = "";
            this.personAddressControl.Email = "";
            this.personAddressControl.Initializing = true;
            this.personAddressControl.Location = new System.Drawing.Point(12, 133);
            this.personAddressControl.Name = "personAddressControl";
            this.personAddressControl.PayStyle = "";
            this.personAddressControl.PersonName = "";
            this.personAddressControl.Size = new System.Drawing.Size(470, 191);
            this.personAddressControl.TabIndex = 1;
            this.personAddressControl.Tel = "";
            this.personAddressControl.Zipcode = "";
            // 
            // SpecialSourceSeriesDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(494, 370);
            this.Controls.Add(this.comboBox_source);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label_source);
            this.Controls.Add(this.comboBox_seller);
            this.Controls.Add(this.comboBox_specialSource);
            this.Controls.Add(this.label_seller);
            this.Controls.Add(this.personAddressControl);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.label_address);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SpecialSourceSeriesDialog";
            this.ShowInTaskbar = false;
            this.Text = "特殊渠道订购";
            this.Load += new System.EventHandler(this.SpecialSourceSeriesDialog_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SpecialSourceSeriesDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpecialSourceSeriesDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private PersonAddressControl personAddressControl;
        private System.Windows.Forms.Label label_address;
        private System.Windows.Forms.ComboBox comboBox_source;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.ComboBox comboBox_seller;
        private System.Windows.Forms.Label label_seller;
        private System.Windows.Forms.ComboBox comboBox_specialSource;
        private System.Windows.Forms.Label label7;
    }
}