namespace dp2Circulation
{
    partial class ScanRegisterBarcodeForm
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
            this.checkBox_autoUppercaseBarcode = new System.Windows.Forms.CheckBox();
            this.button_scan = new System.Windows.Forms.Button();
            this.textBox_barcode = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkBox_autoUppercaseBarcode
            // 
            this.checkBox_autoUppercaseBarcode.AutoSize = true;
            this.checkBox_autoUppercaseBarcode.Location = new System.Drawing.Point(97, 48);
            this.checkBox_autoUppercaseBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_autoUppercaseBarcode.Name = "checkBox_autoUppercaseBarcode";
            this.checkBox_autoUppercaseBarcode.Size = new System.Drawing.Size(222, 16);
            this.checkBox_autoUppercaseBarcode.TabIndex = 12;
            this.checkBox_autoUppercaseBarcode.Text = "自动把输入的条码字符串转为大写(&U)";
            this.checkBox_autoUppercaseBarcode.UseVisualStyleBackColor = true;
            // 
            // button_scan
            // 
            this.button_scan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_scan.Location = new System.Drawing.Point(318, 11);
            this.button_scan.Margin = new System.Windows.Forms.Padding(2);
            this.button_scan.Name = "button_scan";
            this.button_scan.Size = new System.Drawing.Size(81, 29);
            this.button_scan.TabIndex = 11;
            this.button_scan.Text = "提交(&S)";
            this.button_scan.UseVisualStyleBackColor = true;
            this.button_scan.Click += new System.EventHandler(this.button_scan_Click);
            // 
            // textBox_barcode
            // 
            this.textBox_barcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_barcode.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_barcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_barcode.Location = new System.Drawing.Point(97, 11);
            this.textBox_barcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_barcode.Name = "textBox_barcode";
            this.textBox_barcode.Size = new System.Drawing.Size(217, 29);
            this.textBox_barcode.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 20);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "册条码号(&B):";
            // 
            // ScanBarcodeForm
            // 
            this.AcceptButton = this.button_scan;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 75);
            this.Controls.Add(this.checkBox_autoUppercaseBarcode);
            this.Controls.Add(this.button_scan);
            this.Controls.Add(this.textBox_barcode);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScanBarcodeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "扫入条码";
            this.Activated += new System.EventHandler(this.ScanBarcodeForm_Activated);
            this.Deactivate += new System.EventHandler(this.ScanBarcodeForm_Deactivate);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_autoUppercaseBarcode;
        private System.Windows.Forms.Button button_scan;
        private System.Windows.Forms.TextBox textBox_barcode;
        private System.Windows.Forms.Label label3;
    }
}