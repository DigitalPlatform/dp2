namespace dp2Circulation
{
    partial class ImportExportForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_range = new System.Windows.Forms.TabPage();
            this.listView_in = new DigitalPlatform.GUI.ListViewNF();
            this.button_load_scanBarcode = new System.Windows.Forms.Button();
            this.button_load_loadFromRecPathFile = new System.Windows.Forms.Button();
            this.button_load_loadFromBatchNo = new System.Windows.Forms.Button();
            this.button_load_loadFromBarcodeFile = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage_range.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_range);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(443, 309);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_range
            // 
            this.tabPage_range.Controls.Add(this.listView_in);
            this.tabPage_range.Controls.Add(this.button_load_scanBarcode);
            this.tabPage_range.Controls.Add(this.button_load_loadFromRecPathFile);
            this.tabPage_range.Controls.Add(this.button_load_loadFromBatchNo);
            this.tabPage_range.Controls.Add(this.button_load_loadFromBarcodeFile);
            this.tabPage_range.Location = new System.Drawing.Point(4, 22);
            this.tabPage_range.Name = "tabPage_range";
            this.tabPage_range.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_range.Size = new System.Drawing.Size(435, 283);
            this.tabPage_range.TabIndex = 0;
            this.tabPage_range.Text = "范围";
            this.tabPage_range.UseVisualStyleBackColor = true;
            // 
            // listView_in
            // 
            this.listView_in.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_in.FullRowSelect = true;
            this.listView_in.HideSelection = false;
            this.listView_in.Location = new System.Drawing.Point(5, 92);
            this.listView_in.Margin = new System.Windows.Forms.Padding(2);
            this.listView_in.Name = "listView_in";
            this.listView_in.Size = new System.Drawing.Size(425, 186);
            this.listView_in.TabIndex = 10;
            this.listView_in.UseCompatibleStateImageBehavior = false;
            this.listView_in.View = System.Windows.Forms.View.Details;
            // 
            // button_load_scanBarcode
            // 
            this.button_load_scanBarcode.Location = new System.Drawing.Point(179, 44);
            this.button_load_scanBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_scanBarcode.Name = "button_load_scanBarcode";
            this.button_load_scanBarcode.Size = new System.Drawing.Size(170, 22);
            this.button_load_scanBarcode.TabIndex = 9;
            this.button_load_scanBarcode.Text = "扫入册条码(&S)...";
            this.button_load_scanBarcode.UseVisualStyleBackColor = true;
            this.button_load_scanBarcode.Click += new System.EventHandler(this.button_load_scanBarcode_Click);
            // 
            // button_load_loadFromRecPathFile
            // 
            this.button_load_loadFromRecPathFile.Location = new System.Drawing.Point(5, 45);
            this.button_load_loadFromRecPathFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromRecPathFile.Name = "button_load_loadFromRecPathFile";
            this.button_load_loadFromRecPathFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromRecPathFile.TabIndex = 7;
            this.button_load_loadFromRecPathFile.Text = "从册记录路径文件(&R)...";
            this.button_load_loadFromRecPathFile.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromBatchNo
            // 
            this.button_load_loadFromBatchNo.Location = new System.Drawing.Point(179, 18);
            this.button_load_loadFromBatchNo.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromBatchNo.Name = "button_load_loadFromBatchNo";
            this.button_load_loadFromBatchNo.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromBatchNo.TabIndex = 8;
            this.button_load_loadFromBatchNo.Text = "根据批次号检索(&B)...";
            this.button_load_loadFromBatchNo.UseVisualStyleBackColor = true;
            // 
            // button_load_loadFromBarcodeFile
            // 
            this.button_load_loadFromBarcodeFile.Location = new System.Drawing.Point(5, 19);
            this.button_load_loadFromBarcodeFile.Margin = new System.Windows.Forms.Padding(2);
            this.button_load_loadFromBarcodeFile.Name = "button_load_loadFromBarcodeFile";
            this.button_load_loadFromBarcodeFile.Size = new System.Drawing.Size(170, 22);
            this.button_load_loadFromBarcodeFile.TabIndex = 6;
            this.button_load_loadFromBarcodeFile.Text = "从条码号文件(&F)...";
            this.button_load_loadFromBarcodeFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromBarcodeFile.Click += new System.EventHandler(this.button_load_loadFromBarcodeFile_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(435, 283);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ImportExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 334);
            this.Controls.Add(this.tabControl1);
            this.Name = "ImportExportForm";
            this.Text = "ImportExportForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportExportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportExportForm_FormClosed);
            this.Load += new System.EventHandler(this.ImportExportForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_range.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_range;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button_load_scanBarcode;
        private System.Windows.Forms.Button button_load_loadFromRecPathFile;
        private System.Windows.Forms.Button button_load_loadFromBatchNo;
        private System.Windows.Forms.Button button_load_loadFromBarcodeFile;
        private DigitalPlatform.GUI.ListViewNF listView_in;
    }
}