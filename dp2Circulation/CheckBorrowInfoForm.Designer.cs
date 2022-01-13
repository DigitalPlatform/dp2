namespace dp2Circulation
{
    partial class CheckBorrowInfoForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckBorrowInfoForm));
            this.button_beginCheckFromReader = new System.Windows.Forms.Button();
            this.webBrowser_resultInfo = new System.Windows.Forms.WebBrowser();
            this.button_beginCheckFromItem = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_batchCheck = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_beginRepairFromItem = new System.Windows.Forms.Button();
            this.button_beginRepairFromReader = new System.Windows.Forms.Button();
            this.checkBox_checkItemBarcodeDup = new System.Windows.Forms.CheckBox();
            this.checkBox_checkReaderBarcodeDup = new System.Windows.Forms.CheckBox();
            this.tabPage_batchAddItemPrice = new System.Windows.Forms.TabPage();
            this.checkBox_displayPriceString = new System.Windows.Forms.CheckBox();
            this.checkBox_overwriteExistPrice = new System.Windows.Forms.CheckBox();
            this.checkBox_forceCNY = new System.Windows.Forms.CheckBox();
            this.button_batchAddItemPrice = new System.Windows.Forms.Button();
            this.tabPage_singleCheck = new System.Windows.Forms.TabPage();
            this.button_single_repairFromItem = new System.Windows.Forms.Button();
            this.button_single_repairFromReader = new System.Windows.Forms.Button();
            this.button_single_checkFromReader = new System.Windows.Forms.Button();
            this.button_single_checkFromItem = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_single_itemBarcode = new System.Windows.Forms.TextBox();
            this.textBox_single_readerBarcode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_clearInfo = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.checkBox_displayRecords = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_batchCheck.SuspendLayout();
            this.tabPage_batchAddItemPrice.SuspendLayout();
            this.tabPage_singleCheck.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_beginCheckFromReader
            // 
            this.button_beginCheckFromReader.Location = new System.Drawing.Point(7, 9);
            this.button_beginCheckFromReader.Margin = new System.Windows.Forms.Padding(4);
            this.button_beginCheckFromReader.Name = "button_beginCheckFromReader";
            this.button_beginCheckFromReader.Size = new System.Drawing.Size(242, 38);
            this.button_beginCheckFromReader.TabIndex = 0;
            this.button_beginCheckFromReader.Text = "从读者角度检查(&R)";
            this.button_beginCheckFromReader.UseVisualStyleBackColor = true;
            this.button_beginCheckFromReader.Click += new System.EventHandler(this.button_beginCheckFromReader_Click);
            // 
            // webBrowser_resultInfo
            // 
            this.webBrowser_resultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_resultInfo.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_resultInfo.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_resultInfo.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_resultInfo.Name = "webBrowser_resultInfo";
            this.webBrowser_resultInfo.Size = new System.Drawing.Size(785, 300);
            this.webBrowser_resultInfo.TabIndex = 0;
            // 
            // button_beginCheckFromItem
            // 
            this.button_beginCheckFromItem.Location = new System.Drawing.Point(411, 9);
            this.button_beginCheckFromItem.Margin = new System.Windows.Forms.Padding(4);
            this.button_beginCheckFromItem.Name = "button_beginCheckFromItem";
            this.button_beginCheckFromItem.Size = new System.Drawing.Size(242, 38);
            this.button_beginCheckFromItem.TabIndex = 2;
            this.button_beginCheckFromItem.Text = "从册角度检查(&I)";
            this.button_beginCheckFromItem.UseVisualStyleBackColor = true;
            this.button_beginCheckFromItem.Click += new System.EventHandler(this.button_beginCheckFromItem_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_batchCheck);
            this.tabControl_main.Controls.Add(this.tabPage_batchAddItemPrice);
            this.tabControl_main.Controls.Add(this.tabPage_singleCheck);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(785, 136);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_batchCheck
            // 
            this.tabPage_batchCheck.Controls.Add(this.groupBox1);
            this.tabPage_batchCheck.Controls.Add(this.button_beginRepairFromItem);
            this.tabPage_batchCheck.Controls.Add(this.button_beginRepairFromReader);
            this.tabPage_batchCheck.Controls.Add(this.checkBox_checkItemBarcodeDup);
            this.tabPage_batchCheck.Controls.Add(this.checkBox_checkReaderBarcodeDup);
            this.tabPage_batchCheck.Controls.Add(this.button_beginCheckFromReader);
            this.tabPage_batchCheck.Controls.Add(this.button_beginCheckFromItem);
            this.tabPage_batchCheck.Location = new System.Drawing.Point(4, 31);
            this.tabPage_batchCheck.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_batchCheck.Name = "tabPage_batchCheck";
            this.tabPage_batchCheck.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_batchCheck.Size = new System.Drawing.Size(777, 101);
            this.tabPage_batchCheck.TabIndex = 0;
            this.tabPage_batchCheck.Text = "批检查借阅信息链";
            this.tabPage_batchCheck.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(387, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(4, 85);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // button_beginRepairFromItem
            // 
            this.button_beginRepairFromItem.Location = new System.Drawing.Point(657, 9);
            this.button_beginRepairFromItem.Margin = new System.Windows.Forms.Padding(4);
            this.button_beginRepairFromItem.Name = "button_beginRepairFromItem";
            this.button_beginRepairFromItem.Size = new System.Drawing.Size(113, 38);
            this.button_beginRepairFromItem.TabIndex = 4;
            this.button_beginRepairFromItem.Text = "修复(&N)";
            this.button_beginRepairFromItem.UseVisualStyleBackColor = true;
            this.button_beginRepairFromItem.Click += new System.EventHandler(this.button_beginRepairFromItem_Click);
            // 
            // button_beginRepairFromReader
            // 
            this.button_beginRepairFromReader.Location = new System.Drawing.Point(257, 8);
            this.button_beginRepairFromReader.Margin = new System.Windows.Forms.Padding(4);
            this.button_beginRepairFromReader.Name = "button_beginRepairFromReader";
            this.button_beginRepairFromReader.Size = new System.Drawing.Size(113, 38);
            this.button_beginRepairFromReader.TabIndex = 1;
            this.button_beginRepairFromReader.Text = "修复(&E)";
            this.button_beginRepairFromReader.UseVisualStyleBackColor = true;
            this.button_beginRepairFromReader.Click += new System.EventHandler(this.button_beginRepairFromReader_Click);
            // 
            // checkBox_checkItemBarcodeDup
            // 
            this.checkBox_checkItemBarcodeDup.AutoSize = true;
            this.checkBox_checkItemBarcodeDup.Checked = true;
            this.checkBox_checkItemBarcodeDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_checkItemBarcodeDup.Location = new System.Drawing.Point(411, 56);
            this.checkBox_checkItemBarcodeDup.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_checkItemBarcodeDup.Name = "checkBox_checkItemBarcodeDup";
            this.checkBox_checkItemBarcodeDup.Size = new System.Drawing.Size(162, 25);
            this.checkBox_checkItemBarcodeDup.TabIndex = 5;
            this.checkBox_checkItemBarcodeDup.Text = "册条码号查重";
            this.checkBox_checkItemBarcodeDup.UseVisualStyleBackColor = true;
            // 
            // checkBox_checkReaderBarcodeDup
            // 
            this.checkBox_checkReaderBarcodeDup.AutoSize = true;
            this.checkBox_checkReaderBarcodeDup.Checked = true;
            this.checkBox_checkReaderBarcodeDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_checkReaderBarcodeDup.Location = new System.Drawing.Point(7, 56);
            this.checkBox_checkReaderBarcodeDup.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_checkReaderBarcodeDup.Name = "checkBox_checkReaderBarcodeDup";
            this.checkBox_checkReaderBarcodeDup.Size = new System.Drawing.Size(204, 25);
            this.checkBox_checkReaderBarcodeDup.TabIndex = 2;
            this.checkBox_checkReaderBarcodeDup.Text = "读者证条码号查重";
            this.checkBox_checkReaderBarcodeDup.UseVisualStyleBackColor = true;
            // 
            // tabPage_batchAddItemPrice
            // 
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_displayPriceString);
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_overwriteExistPrice);
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_forceCNY);
            this.tabPage_batchAddItemPrice.Controls.Add(this.button_batchAddItemPrice);
            this.tabPage_batchAddItemPrice.Location = new System.Drawing.Point(4, 31);
            this.tabPage_batchAddItemPrice.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_batchAddItemPrice.Name = "tabPage_batchAddItemPrice";
            this.tabPage_batchAddItemPrice.Size = new System.Drawing.Size(777, 101);
            this.tabPage_batchAddItemPrice.TabIndex = 2;
            this.tabPage_batchAddItemPrice.Text = "批增加册价格";
            this.tabPage_batchAddItemPrice.UseVisualStyleBackColor = true;
            // 
            // checkBox_displayPriceString
            // 
            this.checkBox_displayPriceString.AutoSize = true;
            this.checkBox_displayPriceString.Checked = true;
            this.checkBox_displayPriceString.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_displayPriceString.Location = new System.Drawing.Point(491, 66);
            this.checkBox_displayPriceString.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_displayPriceString.Name = "checkBox_displayPriceString";
            this.checkBox_displayPriceString.Size = new System.Drawing.Size(288, 25);
            this.checkBox_displayPriceString.TabIndex = 4;
            this.checkBox_displayPriceString.Text = "处理过程中显示价格字符串";
            this.checkBox_displayPriceString.UseVisualStyleBackColor = true;
            // 
            // checkBox_overwriteExistPrice
            // 
            this.checkBox_overwriteExistPrice.AutoSize = true;
            this.checkBox_overwriteExistPrice.Location = new System.Drawing.Point(262, 66);
            this.checkBox_overwriteExistPrice.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_overwriteExistPrice.Name = "checkBox_overwriteExistPrice";
            this.checkBox_overwriteExistPrice.Size = new System.Drawing.Size(225, 25);
            this.checkBox_overwriteExistPrice.TabIndex = 3;
            this.checkBox_overwriteExistPrice.Text = "覆盖已有价格字符串";
            this.checkBox_overwriteExistPrice.UseVisualStyleBackColor = true;
            // 
            // checkBox_forceCNY
            // 
            this.checkBox_forceCNY.AutoSize = true;
            this.checkBox_forceCNY.Location = new System.Drawing.Point(4, 66);
            this.checkBox_forceCNY.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_forceCNY.Name = "checkBox_forceCNY";
            this.checkBox_forceCNY.Size = new System.Drawing.Size(238, 25);
            this.checkBox_forceCNY.TabIndex = 2;
            this.checkBox_forceCNY.Text = "强制设置币种为\'CNY\'";
            this.checkBox_forceCNY.UseVisualStyleBackColor = true;
            // 
            // button_batchAddItemPrice
            // 
            this.button_batchAddItemPrice.Location = new System.Drawing.Point(4, 18);
            this.button_batchAddItemPrice.Margin = new System.Windows.Forms.Padding(4);
            this.button_batchAddItemPrice.Name = "button_batchAddItemPrice";
            this.button_batchAddItemPrice.Size = new System.Drawing.Size(185, 38);
            this.button_batchAddItemPrice.TabIndex = 0;
            this.button_batchAddItemPrice.Text = "批增加册价格";
            this.button_batchAddItemPrice.UseVisualStyleBackColor = true;
            this.button_batchAddItemPrice.Click += new System.EventHandler(this.button_batchAddItemPrice_Click);
            // 
            // tabPage_singleCheck
            // 
            this.tabPage_singleCheck.Controls.Add(this.button_single_repairFromItem);
            this.tabPage_singleCheck.Controls.Add(this.button_single_repairFromReader);
            this.tabPage_singleCheck.Controls.Add(this.button_single_checkFromReader);
            this.tabPage_singleCheck.Controls.Add(this.button_single_checkFromItem);
            this.tabPage_singleCheck.Controls.Add(this.label3);
            this.tabPage_singleCheck.Controls.Add(this.textBox_single_itemBarcode);
            this.tabPage_singleCheck.Controls.Add(this.textBox_single_readerBarcode);
            this.tabPage_singleCheck.Controls.Add(this.label4);
            this.tabPage_singleCheck.Location = new System.Drawing.Point(4, 31);
            this.tabPage_singleCheck.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_singleCheck.Name = "tabPage_singleCheck";
            this.tabPage_singleCheck.Size = new System.Drawing.Size(777, 101);
            this.tabPage_singleCheck.TabIndex = 3;
            this.tabPage_singleCheck.Text = "零星检查";
            this.tabPage_singleCheck.UseVisualStyleBackColor = true;
            // 
            // button_single_repairFromItem
            // 
            this.button_single_repairFromItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_repairFromItem.Location = new System.Drawing.Point(597, 59);
            this.button_single_repairFromItem.Margin = new System.Windows.Forms.Padding(4);
            this.button_single_repairFromItem.Name = "button_single_repairFromItem";
            this.button_single_repairFromItem.Size = new System.Drawing.Size(156, 38);
            this.button_single_repairFromItem.TabIndex = 7;
            this.button_single_repairFromItem.Text = "从册侧修复";
            this.button_single_repairFromItem.UseVisualStyleBackColor = true;
            this.button_single_repairFromItem.Click += new System.EventHandler(this.button_single_repairFromItem_Click);
            // 
            // button_single_repairFromReader
            // 
            this.button_single_repairFromReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_repairFromReader.Location = new System.Drawing.Point(597, 16);
            this.button_single_repairFromReader.Margin = new System.Windows.Forms.Padding(4);
            this.button_single_repairFromReader.Name = "button_single_repairFromReader";
            this.button_single_repairFromReader.Size = new System.Drawing.Size(156, 38);
            this.button_single_repairFromReader.TabIndex = 3;
            this.button_single_repairFromReader.Text = "从读者侧修复";
            this.button_single_repairFromReader.UseVisualStyleBackColor = true;
            this.button_single_repairFromReader.Click += new System.EventHandler(this.button_single_repairFromReader_Click);
            // 
            // button_single_checkFromReader
            // 
            this.button_single_checkFromReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_checkFromReader.Location = new System.Drawing.Point(433, 16);
            this.button_single_checkFromReader.Margin = new System.Windows.Forms.Padding(4);
            this.button_single_checkFromReader.Name = "button_single_checkFromReader";
            this.button_single_checkFromReader.Size = new System.Drawing.Size(156, 38);
            this.button_single_checkFromReader.TabIndex = 2;
            this.button_single_checkFromReader.Text = "从读者侧检查";
            this.button_single_checkFromReader.UseVisualStyleBackColor = true;
            this.button_single_checkFromReader.Click += new System.EventHandler(this.button_single_checkFromReader_Click);
            // 
            // button_single_checkFromItem
            // 
            this.button_single_checkFromItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_checkFromItem.Location = new System.Drawing.Point(433, 60);
            this.button_single_checkFromItem.Margin = new System.Windows.Forms.Padding(4);
            this.button_single_checkFromItem.Name = "button_single_checkFromItem";
            this.button_single_checkFromItem.Size = new System.Drawing.Size(156, 38);
            this.button_single_checkFromItem.TabIndex = 6;
            this.button_single_checkFromItem.Text = "从册侧检查";
            this.button_single_checkFromItem.UseVisualStyleBackColor = true;
            this.button_single_checkFromItem.Click += new System.EventHandler(this.button_single_checkFromItem_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 24);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "读者证条码号(&R):";
            // 
            // textBox_single_itemBarcode
            // 
            this.textBox_single_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_single_itemBarcode.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_single_itemBarcode.Location = new System.Drawing.Point(174, 60);
            this.textBox_single_itemBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_single_itemBarcode.Name = "textBox_single_itemBarcode";
            this.textBox_single_itemBarcode.Size = new System.Drawing.Size(250, 31);
            this.textBox_single_itemBarcode.TabIndex = 5;
            // 
            // textBox_single_readerBarcode
            // 
            this.textBox_single_readerBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_single_readerBarcode.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_single_readerBarcode.Location = new System.Drawing.Point(174, 16);
            this.textBox_single_readerBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_single_readerBarcode.Name = "textBox_single_readerBarcode";
            this.textBox_single_readerBarcode.Size = new System.Drawing.Size(250, 31);
            this.textBox_single_readerBarcode.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 4;
            this.label4.Text = "册条码号(&I):";
            // 
            // button_clearInfo
            // 
            this.button_clearInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_clearInfo.Location = new System.Drawing.Point(678, 474);
            this.button_clearInfo.Margin = new System.Windows.Forms.Padding(4);
            this.button_clearInfo.Name = "button_clearInfo";
            this.button_clearInfo.Size = new System.Drawing.Size(123, 38);
            this.button_clearInfo.TabIndex = 0;
            this.button_clearInfo.Text = "清除(&C)";
            this.button_clearInfo.UseVisualStyleBackColor = true;
            this.button_clearInfo.Click += new System.EventHandler(this.button_clearInfo_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(17, 18);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_main);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser_resultInfo);
            this.splitContainer_main.Size = new System.Drawing.Size(785, 450);
            this.splitContainer_main.SplitterDistance = 136;
            this.splitContainer_main.SplitterWidth = 14;
            this.splitContainer_main.TabIndex = 11;
            // 
            // checkBox_displayRecords
            // 
            this.checkBox_displayRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_displayRecords.AutoSize = true;
            this.checkBox_displayRecords.Location = new System.Drawing.Point(17, 475);
            this.checkBox_displayRecords.Name = "checkBox_displayRecords";
            this.checkBox_displayRecords.Size = new System.Drawing.Size(195, 25);
            this.checkBox_displayRecords.TabIndex = 12;
            this.checkBox_displayRecords.Text = "显示相关记录(&R)";
            this.checkBox_displayRecords.UseVisualStyleBackColor = true;
            // 
            // CheckBorrowInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 530);
            this.Controls.Add(this.checkBox_displayRecords);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_clearInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CheckBorrowInfoForm";
            this.ShowInTaskbar = false;
            this.Text = "检查借阅信息";
            this.Activated += new System.EventHandler(this.CheckBorrowInfoForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CheckBorrowInfoForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CheckBorrowInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.CheckBorrowInfoForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_batchCheck.ResumeLayout(false);
            this.tabPage_batchCheck.PerformLayout();
            this.tabPage_batchAddItemPrice.ResumeLayout(false);
            this.tabPage_batchAddItemPrice.PerformLayout();
            this.tabPage_singleCheck.ResumeLayout(false);
            this.tabPage_singleCheck.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_beginCheckFromReader;
        private System.Windows.Forms.WebBrowser webBrowser_resultInfo;
        private System.Windows.Forms.Button button_beginCheckFromItem;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_batchCheck;
        private System.Windows.Forms.TabPage tabPage_batchAddItemPrice;
        private System.Windows.Forms.Button button_batchAddItemPrice;
        private System.Windows.Forms.CheckBox checkBox_forceCNY;
        private System.Windows.Forms.CheckBox checkBox_overwriteExistPrice;
        private System.Windows.Forms.CheckBox checkBox_displayPriceString;
        private System.Windows.Forms.Button button_clearInfo;
        private System.Windows.Forms.CheckBox checkBox_checkItemBarcodeDup;
        private System.Windows.Forms.CheckBox checkBox_checkReaderBarcodeDup;
        private System.Windows.Forms.TabPage tabPage_singleCheck;
        private System.Windows.Forms.Button button_single_checkFromReader;
        private System.Windows.Forms.Button button_single_checkFromItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_single_itemBarcode;
        private System.Windows.Forms.TextBox textBox_single_readerBarcode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Button button_single_repairFromItem;
        private System.Windows.Forms.Button button_single_repairFromReader;
        private System.Windows.Forms.Button button_beginRepairFromItem;
        private System.Windows.Forms.Button button_beginRepairFromReader;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_displayRecords;
    }
}