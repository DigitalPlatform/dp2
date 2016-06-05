namespace dp2Circulation
{
    partial class InventoryFromFileDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryFromFileDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_barcodeFileName = new System.Windows.Forms.TextBox();
            this.button_findBarcodeFileName = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.inventoryBatchNoControl_start_batchNo = new dp2Circulation.InventoryBatchNoControl();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 24);
            this.label1.TabIndex = 10;
            this.label1.Text = "盘点批次号(&B):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 138);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(226, 24);
            this.label2.TabIndex = 12;
            this.label2.Text = "册条码号文件名(&F):";
            // 
            // textBox_barcodeFileName
            // 
            this.textBox_barcodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_barcodeFileName.Location = new System.Drawing.Point(24, 168);
            this.textBox_barcodeFileName.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_barcodeFileName.Name = "textBox_barcodeFileName";
            this.textBox_barcodeFileName.Size = new System.Drawing.Size(690, 35);
            this.textBox_barcodeFileName.TabIndex = 13;
            // 
            // button_findBarcodeFileName
            // 
            this.button_findBarcodeFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findBarcodeFileName.Location = new System.Drawing.Point(724, 164);
            this.button_findBarcodeFileName.Margin = new System.Windows.Forms.Padding(6);
            this.button_findBarcodeFileName.Name = "button_findBarcodeFileName";
            this.button_findBarcodeFileName.Size = new System.Drawing.Size(94, 46);
            this.button_findBarcodeFileName.TabIndex = 14;
            this.button_findBarcodeFileName.Text = "...";
            this.button_findBarcodeFileName.UseVisualStyleBackColor = true;
            this.button_findBarcodeFileName.Click += new System.EventHandler(this.button_findBarcodeFileName_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(670, 454);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(150, 46);
            this.button_Cancel.TabIndex = 16;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(512, 454);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(150, 46);
            this.button_OK.TabIndex = 15;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // inventoryBatchNoControl_start_batchNo
            // 
            this.inventoryBatchNoControl_start_batchNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inventoryBatchNoControl_start_batchNo.AutoSize = true;
            this.inventoryBatchNoControl_start_batchNo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inventoryBatchNoControl_start_batchNo.LibaryCodeEanbled = true;
            this.inventoryBatchNoControl_start_batchNo.LibraryCodeList = ((System.Collections.Generic.List<string>)(resources.GetObject("inventoryBatchNoControl_start_batchNo.LibraryCodeList")));
            this.inventoryBatchNoControl_start_batchNo.LibraryCodeText = "";
            this.inventoryBatchNoControl_start_batchNo.Location = new System.Drawing.Point(24, 48);
            this.inventoryBatchNoControl_start_batchNo.Margin = new System.Windows.Forms.Padding(12);
            this.inventoryBatchNoControl_start_batchNo.Name = "inventoryBatchNoControl_start_batchNo";
            this.inventoryBatchNoControl_start_batchNo.Size = new System.Drawing.Size(796, 42);
            this.inventoryBatchNoControl_start_batchNo.TabIndex = 11;
            // 
            // InventoryFromFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(842, 522);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_findBarcodeFileName);
            this.Controls.Add(this.textBox_barcodeFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inventoryBatchNoControl_start_batchNo);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "InventoryFromFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从文件导入盘点";
            this.Load += new System.EventHandler(this.InventoryFromFileDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private InventoryBatchNoControl inventoryBatchNoControl_start_batchNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_barcodeFileName;
        private System.Windows.Forms.Button button_findBarcodeFileName;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}