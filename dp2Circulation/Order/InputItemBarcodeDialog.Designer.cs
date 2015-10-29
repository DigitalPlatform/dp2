namespace dp2Circulation
{
    partial class InputItemBarcodeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputItemBarcodeDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.button_register = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_barcodes = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_barcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_volumeDisplay = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_sequence = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_seller = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_price = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_otherPrices = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_alwaysFocusInputBox = new System.Windows.Forms.CheckBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_modifyByBiblioPrice = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modifyByOrderPrice = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modifyByArrivePrice = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modifyPrice = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_discount = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 207);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "册条码号(&B):";
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_itemBarcode.HideSelection = false;
            this.textBox_itemBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_itemBarcode.Location = new System.Drawing.Point(92, 205);
            this.textBox_itemBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(253, 21);
            this.textBox_itemBarcode.TabIndex = 4;
            this.textBox_itemBarcode.TextChanged += new System.EventHandler(this.textBox_itemBarcode_TextChanged);
            this.textBox_itemBarcode.Enter += new System.EventHandler(this.textBox_itemBarcode_Enter);
            this.textBox_itemBarcode.Leave += new System.EventHandler(this.textBox_itemBarcode_Leave);
            // 
            // button_register
            // 
            this.button_register.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_register.Image = ((System.Drawing.Image)(resources.GetObject("button_register.Image")));
            this.button_register.Location = new System.Drawing.Point(350, 205);
            this.button_register.Name = "button_register";
            this.button_register.Size = new System.Drawing.Size(72, 23);
            this.button_register.TabIndex = 5;
            this.button_register.Text = "设置";
            this.button_register.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_register.UseVisualStyleBackColor = true;
            this.button_register.Click += new System.EventHandler(this.button_register_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "册事项(&I):";
            // 
            // listView_barcodes
            // 
            this.listView_barcodes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_barcodes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_barcode,
            this.columnHeader_volumeDisplay,
            this.columnHeader_sequence,
            this.columnHeader_location,
            this.columnHeader_seller,
            this.columnHeader_source,
            this.columnHeader_price,
            this.columnHeader_otherPrices});
            this.listView_barcodes.FullRowSelect = true;
            this.listView_barcodes.HideSelection = false;
            this.listView_barcodes.Location = new System.Drawing.Point(9, 50);
            this.listView_barcodes.Margin = new System.Windows.Forms.Padding(2);
            this.listView_barcodes.Name = "listView_barcodes";
            this.listView_barcodes.Size = new System.Drawing.Size(414, 129);
            this.listView_barcodes.TabIndex = 1;
            this.listView_barcodes.UseCompatibleStateImageBehavior = false;
            this.listView_barcodes.View = System.Windows.Forms.View.Details;
            this.listView_barcodes.SelectedIndexChanged += new System.EventHandler(this.listView_barcodes_SelectedIndexChanged);
            this.listView_barcodes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_barcodes_MouseUp);
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "册条码号";
            this.columnHeader_barcode.Width = 150;
            // 
            // columnHeader_volumeDisplay
            // 
            this.columnHeader_volumeDisplay.Text = "卷期信息";
            this.columnHeader_volumeDisplay.Width = 200;
            // 
            // columnHeader_sequence
            // 
            this.columnHeader_sequence.Text = "套序";
            this.columnHeader_sequence.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_sequence.Width = 30;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "馆藏地点";
            this.columnHeader_location.Width = 150;
            // 
            // columnHeader_seller
            // 
            this.columnHeader_seller.Text = "订购渠道";
            this.columnHeader_seller.Width = 120;
            // 
            // columnHeader_source
            // 
            this.columnHeader_source.Text = "经费来源";
            this.columnHeader_source.Width = 120;
            // 
            // columnHeader_price
            // 
            this.columnHeader_price.Text = "价格";
            this.columnHeader_price.Width = 120;
            // 
            // columnHeader_otherPrices
            // 
            this.columnHeader_otherPrices.Text = "其他价格";
            this.columnHeader_otherPrices.Width = 300;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(366, 243);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(305, 243);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_alwaysFocusInputBox
            // 
            this.checkBox_alwaysFocusInputBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_alwaysFocusInputBox.AutoSize = true;
            this.checkBox_alwaysFocusInputBox.Checked = true;
            this.checkBox_alwaysFocusInputBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_alwaysFocusInputBox.Location = new System.Drawing.Point(9, 182);
            this.checkBox_alwaysFocusInputBox.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_alwaysFocusInputBox.Name = "checkBox_alwaysFocusInputBox";
            this.checkBox_alwaysFocusInputBox.Size = new System.Drawing.Size(186, 16);
            this.checkBox_alwaysFocusInputBox.TabIndex = 2;
            this.checkBox_alwaysFocusInputBox.Text = "始终把焦点保持在文本框上(&F)";
            this.checkBox_alwaysFocusInputBox.UseVisualStyleBackColor = true;
            this.checkBox_alwaysFocusInputBox.CheckedChanged += new System.EventHandler(this.checkBox_alwaysFocusInputBox_CheckedChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_modifyByBiblioPrice,
            this.toolStripButton_modifyByOrderPrice,
            this.toolStripButton_modifyByArrivePrice,
            this.toolStripButton_modifyPrice,
            this.toolStripButton_discount});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(431, 25);
            this.toolStrip1.TabIndex = 8;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_modifyByBiblioPrice
            // 
            this.toolStripButton_modifyByBiblioPrice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyByBiblioPrice.Enabled = false;
            this.toolStripButton_modifyByBiblioPrice.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyByBiblioPrice.Image")));
            this.toolStripButton_modifyByBiblioPrice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyByBiblioPrice.Name = "toolStripButton_modifyByBiblioPrice";
            this.toolStripButton_modifyByBiblioPrice.Size = new System.Drawing.Size(72, 22);
            this.toolStripButton_modifyByBiblioPrice.Text = "刷为书目价";
            this.toolStripButton_modifyByBiblioPrice.Click += new System.EventHandler(this.toolStripButton_modifyByBiblioPrice_Click);
            // 
            // toolStripButton_modifyByOrderPrice
            // 
            this.toolStripButton_modifyByOrderPrice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyByOrderPrice.Enabled = false;
            this.toolStripButton_modifyByOrderPrice.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyByOrderPrice.Image")));
            this.toolStripButton_modifyByOrderPrice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyByOrderPrice.Name = "toolStripButton_modifyByOrderPrice";
            this.toolStripButton_modifyByOrderPrice.Size = new System.Drawing.Size(72, 22);
            this.toolStripButton_modifyByOrderPrice.Text = "刷为订购价";
            this.toolStripButton_modifyByOrderPrice.Click += new System.EventHandler(this.toolStripButton_modifyByOrderPrice_Click);
            // 
            // toolStripButton_modifyByArrivePrice
            // 
            this.toolStripButton_modifyByArrivePrice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyByArrivePrice.Enabled = false;
            this.toolStripButton_modifyByArrivePrice.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyByArrivePrice.Image")));
            this.toolStripButton_modifyByArrivePrice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyByArrivePrice.Name = "toolStripButton_modifyByArrivePrice";
            this.toolStripButton_modifyByArrivePrice.Size = new System.Drawing.Size(72, 22);
            this.toolStripButton_modifyByArrivePrice.Text = "刷为验收价";
            this.toolStripButton_modifyByArrivePrice.Click += new System.EventHandler(this.toolStripButton_modifyByArrivePrice_Click);
            // 
            // toolStripButton_modifyPrice
            // 
            this.toolStripButton_modifyPrice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyPrice.Enabled = false;
            this.toolStripButton_modifyPrice.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyPrice.Image")));
            this.toolStripButton_modifyPrice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyPrice.Name = "toolStripButton_modifyPrice";
            this.toolStripButton_modifyPrice.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_modifyPrice.Text = "修改价格";
            this.toolStripButton_modifyPrice.Click += new System.EventHandler(this.toolStripButton_modifyPrice_Click);
            // 
            // toolStripButton_discount
            // 
            this.toolStripButton_discount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_discount.Enabled = false;
            this.toolStripButton_discount.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_discount.Image")));
            this.toolStripButton_discount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_discount.Name = "toolStripButton_discount";
            this.toolStripButton_discount.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_discount.Text = "附加折扣";
            this.toolStripButton_discount.ToolTipText = "在原有价格字符串上附加折扣部分";
            this.toolStripButton_discount.Click += new System.EventHandler(this.toolStripButton_discount_Click);
            // 
            // InputItemBarcodeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(431, 275);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.checkBox_alwaysFocusInputBox);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_barcodes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_register);
            this.Controls.Add(this.textBox_itemBarcode);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "InputItemBarcodeDialog";
            this.ShowInTaskbar = false;
            this.Text = "登记册条码号";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InputItemBarcodeDialog_FormClosed);
            this.Load += new System.EventHandler(this.InputItemBarcodeDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.Button button_register;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.GUI.ListViewNF listView_barcodes;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_alwaysFocusInputBox;
        private System.Windows.Forms.ColumnHeader columnHeader_volumeDisplay;
        private System.Windows.Forms.ColumnHeader columnHeader_seller;
        private System.Windows.Forms.ColumnHeader columnHeader_source;
        private System.Windows.Forms.ColumnHeader columnHeader_sequence;
        private System.Windows.Forms.ColumnHeader columnHeader_price;
        private System.Windows.Forms.ColumnHeader columnHeader_otherPrices;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyByBiblioPrice;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyByOrderPrice;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyByArrivePrice;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyPrice;
        private System.Windows.Forms.ToolStripButton toolStripButton_discount;
    }
}