namespace TestUHF
{
    partial class OpenReaderDialog
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_usb = new System.Windows.Forms.TabPage();
            this.comboBox_usbSerialNumber = new System.Windows.Forms.ComboBox();
            this.label32 = new System.Windows.Forms.Label();
            this.comboBox_usbOpenType = new System.Windows.Forms.ComboBox();
            this.label31 = new System.Windows.Forms.Label();
            this.tabPage_com = new System.Windows.Forms.TabPage();
            this.tabPage_tcp = new System.Windows.Forms.TabPage();
            this.comboBox_readerType = new System.Windows.Forms.ComboBox();
            this.label29 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_usb.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(416, 454);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(183, 37);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(605, 454);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(183, 37);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_usb);
            this.tabControl1.Controls.Add(this.tabPage_com);
            this.tabControl1.Controls.Add(this.tabPage_tcp);
            this.tabControl1.Location = new System.Drawing.Point(13, 71);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(775, 365);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage_usb
            // 
            this.tabPage_usb.Controls.Add(this.comboBox_usbSerialNumber);
            this.tabPage_usb.Controls.Add(this.label32);
            this.tabPage_usb.Controls.Add(this.comboBox_usbOpenType);
            this.tabPage_usb.Controls.Add(this.label31);
            this.tabPage_usb.Location = new System.Drawing.Point(4, 31);
            this.tabPage_usb.Name = "tabPage_usb";
            this.tabPage_usb.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_usb.Size = new System.Drawing.Size(767, 330);
            this.tabPage_usb.TabIndex = 0;
            this.tabPage_usb.Text = "USB";
            this.tabPage_usb.UseVisualStyleBackColor = true;
            // 
            // comboBox_usbSerialNumber
            // 
            this.comboBox_usbSerialNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_usbSerialNumber.FormattingEnabled = true;
            this.comboBox_usbSerialNumber.Location = new System.Drawing.Point(189, 58);
            this.comboBox_usbSerialNumber.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_usbSerialNumber.Name = "comboBox_usbSerialNumber";
            this.comboBox_usbSerialNumber.Size = new System.Drawing.Size(220, 29);
            this.comboBox_usbSerialNumber.TabIndex = 7;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(18, 61);
            this.label32.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(164, 21);
            this.label32.TabIndex = 6;
            this.label32.Text = "Serial Number:";
            // 
            // comboBox_usbOpenType
            // 
            this.comboBox_usbOpenType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_usbOpenType.FormattingEnabled = true;
            this.comboBox_usbOpenType.Items.AddRange(new object[] {
            "None addressed",
            "Serial number"});
            this.comboBox_usbOpenType.Location = new System.Drawing.Point(189, 19);
            this.comboBox_usbOpenType.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_usbOpenType.Name = "comboBox_usbOpenType";
            this.comboBox_usbOpenType.Size = new System.Drawing.Size(220, 29);
            this.comboBox_usbOpenType.TabIndex = 5;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(18, 23);
            this.label31.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(164, 21);
            this.label31.TabIndex = 4;
            this.label31.Text = "USB Open type:";
            // 
            // tabPage_com
            // 
            this.tabPage_com.Location = new System.Drawing.Point(4, 31);
            this.tabPage_com.Name = "tabPage_com";
            this.tabPage_com.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_com.Size = new System.Drawing.Size(767, 330);
            this.tabPage_com.TabIndex = 1;
            this.tabPage_com.Text = "COM";
            this.tabPage_com.UseVisualStyleBackColor = true;
            // 
            // tabPage_tcp
            // 
            this.tabPage_tcp.Location = new System.Drawing.Point(4, 31);
            this.tabPage_tcp.Name = "tabPage_tcp";
            this.tabPage_tcp.Size = new System.Drawing.Size(767, 330);
            this.tabPage_tcp.TabIndex = 2;
            this.tabPage_tcp.Text = "TCP";
            this.tabPage_tcp.UseVisualStyleBackColor = true;
            // 
            // comboBox_readerType
            // 
            this.comboBox_readerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_readerType.FormattingEnabled = true;
            this.comboBox_readerType.Location = new System.Drawing.Point(239, 22);
            this.comboBox_readerType.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_readerType.Name = "comboBox_readerType";
            this.comboBox_readerType.Size = new System.Drawing.Size(182, 29);
            this.comboBox_readerType.TabIndex = 43;
            this.comboBox_readerType.SelectedIndexChanged += new System.EventHandler(this.comboBox_readerType_SelectedIndexChanged);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(15, 25);
            this.label29.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(142, 21);
            this.label29.TabIndex = 42;
            this.label29.Text = "Reader Type:";
            // 
            // OpenReaderDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(800, 517);
            this.Controls.Add(this.comboBox_readerType);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "OpenReaderDialog";
            this.Text = "OpenReaderDialog";
            this.Load += new System.EventHandler(this.OpenReaderDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_usb.ResumeLayout(false);
            this.tabPage_usb.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_usb;
        private System.Windows.Forms.TabPage tabPage_com;
        private System.Windows.Forms.TabPage tabPage_tcp;
        private System.Windows.Forms.ComboBox comboBox_readerType;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.ComboBox comboBox_usbSerialNumber;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.ComboBox comboBox_usbOpenType;
        private System.Windows.Forms.Label label31;
    }
}