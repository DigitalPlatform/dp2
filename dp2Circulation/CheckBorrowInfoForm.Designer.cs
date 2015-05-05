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
            this.button_repairReaderSide = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_repairItemSide = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_batchCheckBorrowInfo = new System.Windows.Forms.TabPage();
            this.checkBox_checkItemBarcodeDup = new System.Windows.Forms.CheckBox();
            this.checkBox_checkReaderBarcodeDup = new System.Windows.Forms.CheckBox();
            this.tabPage_recoverBorrowInfo = new System.Windows.Forms.TabPage();
            this.tabPage_batchAddItemPrice = new System.Windows.Forms.TabPage();
            this.checkBox_displayPriceString = new System.Windows.Forms.CheckBox();
            this.checkBox_overwriteExistPrice = new System.Windows.Forms.CheckBox();
            this.checkBox_forceCNY = new System.Windows.Forms.CheckBox();
            this.button_batchAddItemPrice = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button_single_checkFromReader = new System.Windows.Forms.Button();
            this.button_single_checkFromItem = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_single_itemBarcode = new System.Windows.Forms.TextBox();
            this.textBox_single_readerBarcode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_clearInfo = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_main.SuspendLayout();
            this.tabPage_batchCheckBorrowInfo.SuspendLayout();
            this.tabPage_recoverBorrowInfo.SuspendLayout();
            this.tabPage_batchAddItemPrice.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_beginCheckFromReader
            // 
            this.button_beginCheckFromReader.Location = new System.Drawing.Point(4, 5);
            this.button_beginCheckFromReader.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_beginCheckFromReader.Name = "button_beginCheckFromReader";
            this.button_beginCheckFromReader.Size = new System.Drawing.Size(150, 22);
            this.button_beginCheckFromReader.TabIndex = 0;
            this.button_beginCheckFromReader.Text = "从读者角度检查(&R)";
            this.button_beginCheckFromReader.UseVisualStyleBackColor = true;
            this.button_beginCheckFromReader.Click += new System.EventHandler(this.button_beginCheckFromReader_Click);
            // 
            // webBrowser_resultInfo
            // 
            this.webBrowser_resultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_resultInfo.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_resultInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webBrowser_resultInfo.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_resultInfo.Name = "webBrowser_resultInfo";
            this.webBrowser_resultInfo.Size = new System.Drawing.Size(428, 171);
            this.webBrowser_resultInfo.TabIndex = 1;
            // 
            // button_beginCheckFromItem
            // 
            this.button_beginCheckFromItem.Location = new System.Drawing.Point(159, 5);
            this.button_beginCheckFromItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_beginCheckFromItem.Name = "button_beginCheckFromItem";
            this.button_beginCheckFromItem.Size = new System.Drawing.Size(150, 22);
            this.button_beginCheckFromItem.TabIndex = 2;
            this.button_beginCheckFromItem.Text = "从册角度检查(&I)";
            this.button_beginCheckFromItem.UseVisualStyleBackColor = true;
            this.button_beginCheckFromItem.Click += new System.EventHandler(this.button_beginCheckFromItem_Click);
            // 
            // button_repairReaderSide
            // 
            this.button_repairReaderSide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_repairReaderSide.Location = new System.Drawing.Point(241, 5);
            this.button_repairReaderSide.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_repairReaderSide.Name = "button_repairReaderSide";
            this.button_repairReaderSide.Size = new System.Drawing.Size(176, 22);
            this.button_repairReaderSide.TabIndex = 3;
            this.button_repairReaderSide.Text = "修复读者侧链条错误";
            this.button_repairReaderSide.UseVisualStyleBackColor = true;
            this.button_repairReaderSide.Click += new System.EventHandler(this.button_repairReaderSide_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "读者证条码号(&R):";
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_readerBarcode.Location = new System.Drawing.Point(100, 5);
            this.textBox_readerBarcode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(138, 21);
            this.textBox_readerBarcode.TabIndex = 5;
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_itemBarcode.Location = new System.Drawing.Point(100, 30);
            this.textBox_itemBarcode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(138, 21);
            this.textBox_itemBarcode.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "册条码号(&I):";
            // 
            // button_repairItemSide
            // 
            this.button_repairItemSide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_repairItemSide.Location = new System.Drawing.Point(241, 30);
            this.button_repairItemSide.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_repairItemSide.Name = "button_repairItemSide";
            this.button_repairItemSide.Size = new System.Drawing.Size(176, 22);
            this.button_repairItemSide.TabIndex = 8;
            this.button_repairItemSide.Text = "修复册侧链条错误";
            this.button_repairItemSide.UseVisualStyleBackColor = true;
            this.button_repairItemSide.Click += new System.EventHandler(this.button_repairItemSide_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_batchCheckBorrowInfo);
            this.tabControl_main.Controls.Add(this.tabPage_recoverBorrowInfo);
            this.tabControl_main.Controls.Add(this.tabPage_batchAddItemPrice);
            this.tabControl_main.Controls.Add(this.tabPage1);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(428, 78);
            this.tabControl_main.TabIndex = 10;
            // 
            // tabPage_batchCheckBorrowInfo
            // 
            this.tabPage_batchCheckBorrowInfo.Controls.Add(this.checkBox_checkItemBarcodeDup);
            this.tabPage_batchCheckBorrowInfo.Controls.Add(this.checkBox_checkReaderBarcodeDup);
            this.tabPage_batchCheckBorrowInfo.Controls.Add(this.button_beginCheckFromReader);
            this.tabPage_batchCheckBorrowInfo.Controls.Add(this.button_beginCheckFromItem);
            this.tabPage_batchCheckBorrowInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_batchCheckBorrowInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_batchCheckBorrowInfo.Name = "tabPage_batchCheckBorrowInfo";
            this.tabPage_batchCheckBorrowInfo.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_batchCheckBorrowInfo.Size = new System.Drawing.Size(420, 52);
            this.tabPage_batchCheckBorrowInfo.TabIndex = 0;
            this.tabPage_batchCheckBorrowInfo.Text = "批检查借阅信息链";
            this.tabPage_batchCheckBorrowInfo.UseVisualStyleBackColor = true;
            // 
            // checkBox_checkItemBarcodeDup
            // 
            this.checkBox_checkItemBarcodeDup.AutoSize = true;
            this.checkBox_checkItemBarcodeDup.Checked = true;
            this.checkBox_checkItemBarcodeDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_checkItemBarcodeDup.Location = new System.Drawing.Point(159, 32);
            this.checkBox_checkItemBarcodeDup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_checkItemBarcodeDup.Name = "checkBox_checkItemBarcodeDup";
            this.checkBox_checkItemBarcodeDup.Size = new System.Drawing.Size(96, 16);
            this.checkBox_checkItemBarcodeDup.TabIndex = 5;
            this.checkBox_checkItemBarcodeDup.Text = "册条码号查重";
            this.checkBox_checkItemBarcodeDup.UseVisualStyleBackColor = true;
            // 
            // checkBox_checkReaderBarcodeDup
            // 
            this.checkBox_checkReaderBarcodeDup.AutoSize = true;
            this.checkBox_checkReaderBarcodeDup.Checked = true;
            this.checkBox_checkReaderBarcodeDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_checkReaderBarcodeDup.Location = new System.Drawing.Point(4, 32);
            this.checkBox_checkReaderBarcodeDup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_checkReaderBarcodeDup.Name = "checkBox_checkReaderBarcodeDup";
            this.checkBox_checkReaderBarcodeDup.Size = new System.Drawing.Size(120, 16);
            this.checkBox_checkReaderBarcodeDup.TabIndex = 4;
            this.checkBox_checkReaderBarcodeDup.Text = "读者证条码号查重";
            this.checkBox_checkReaderBarcodeDup.UseVisualStyleBackColor = true;
            // 
            // tabPage_recoverBorrowInfo
            // 
            this.tabPage_recoverBorrowInfo.Controls.Add(this.button_repairReaderSide);
            this.tabPage_recoverBorrowInfo.Controls.Add(this.button_repairItemSide);
            this.tabPage_recoverBorrowInfo.Controls.Add(this.label1);
            this.tabPage_recoverBorrowInfo.Controls.Add(this.textBox_itemBarcode);
            this.tabPage_recoverBorrowInfo.Controls.Add(this.textBox_readerBarcode);
            this.tabPage_recoverBorrowInfo.Controls.Add(this.label2);
            this.tabPage_recoverBorrowInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_recoverBorrowInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_recoverBorrowInfo.Name = "tabPage_recoverBorrowInfo";
            this.tabPage_recoverBorrowInfo.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_recoverBorrowInfo.Size = new System.Drawing.Size(420, 52);
            this.tabPage_recoverBorrowInfo.TabIndex = 1;
            this.tabPage_recoverBorrowInfo.Text = "修复借阅信息链";
            this.tabPage_recoverBorrowInfo.UseVisualStyleBackColor = true;
            // 
            // tabPage_batchAddItemPrice
            // 
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_displayPriceString);
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_overwriteExistPrice);
            this.tabPage_batchAddItemPrice.Controls.Add(this.checkBox_forceCNY);
            this.tabPage_batchAddItemPrice.Controls.Add(this.button_batchAddItemPrice);
            this.tabPage_batchAddItemPrice.Location = new System.Drawing.Point(4, 22);
            this.tabPage_batchAddItemPrice.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_batchAddItemPrice.Name = "tabPage_batchAddItemPrice";
            this.tabPage_batchAddItemPrice.Size = new System.Drawing.Size(420, 52);
            this.tabPage_batchAddItemPrice.TabIndex = 2;
            this.tabPage_batchAddItemPrice.Text = "批增加册价格";
            this.tabPage_batchAddItemPrice.UseVisualStyleBackColor = true;
            // 
            // checkBox_displayPriceString
            // 
            this.checkBox_displayPriceString.AutoSize = true;
            this.checkBox_displayPriceString.Checked = true;
            this.checkBox_displayPriceString.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_displayPriceString.Location = new System.Drawing.Point(268, 38);
            this.checkBox_displayPriceString.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_displayPriceString.Name = "checkBox_displayPriceString";
            this.checkBox_displayPriceString.Size = new System.Drawing.Size(126, 13);
            this.checkBox_displayPriceString.TabIndex = 4;
            this.checkBox_displayPriceString.Text = "处理过程中显示价格字符串";
            this.checkBox_displayPriceString.UseVisualStyleBackColor = true;
            // 
            // checkBox_overwriteExistPrice
            // 
            this.checkBox_overwriteExistPrice.AutoSize = true;
            this.checkBox_overwriteExistPrice.Location = new System.Drawing.Point(143, 38);
            this.checkBox_overwriteExistPrice.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_overwriteExistPrice.Name = "checkBox_overwriteExistPrice";
            this.checkBox_overwriteExistPrice.Size = new System.Drawing.Size(99, 13);
            this.checkBox_overwriteExistPrice.TabIndex = 3;
            this.checkBox_overwriteExistPrice.Text = "覆盖已有价格字符串";
            this.checkBox_overwriteExistPrice.UseVisualStyleBackColor = true;
            // 
            // checkBox_forceCNY
            // 
            this.checkBox_forceCNY.AutoSize = true;
            this.checkBox_forceCNY.Location = new System.Drawing.Point(2, 38);
            this.checkBox_forceCNY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_forceCNY.Name = "checkBox_forceCNY";
            this.checkBox_forceCNY.Size = new System.Drawing.Size(104, 13);
            this.checkBox_forceCNY.TabIndex = 2;
            this.checkBox_forceCNY.Text = "强制设置币种为\'CNY\'";
            this.checkBox_forceCNY.UseVisualStyleBackColor = true;
            // 
            // button_batchAddItemPrice
            // 
            this.button_batchAddItemPrice.Location = new System.Drawing.Point(2, 10);
            this.button_batchAddItemPrice.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_batchAddItemPrice.Name = "button_batchAddItemPrice";
            this.button_batchAddItemPrice.Size = new System.Drawing.Size(101, 22);
            this.button_batchAddItemPrice.TabIndex = 0;
            this.button_batchAddItemPrice.Text = "批增加册价格";
            this.button_batchAddItemPrice.UseVisualStyleBackColor = true;
            this.button_batchAddItemPrice.Click += new System.EventHandler(this.button_batchAddItemPrice_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_single_checkFromReader);
            this.tabPage1.Controls.Add(this.button_single_checkFromItem);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.textBox_single_itemBarcode);
            this.tabPage1.Controls.Add(this.textBox_single_readerBarcode);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(420, 52);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "零星检查";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button_single_checkFromReader
            // 
            this.button_single_checkFromReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_checkFromReader.Location = new System.Drawing.Point(236, 9);
            this.button_single_checkFromReader.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_single_checkFromReader.Name = "button_single_checkFromReader";
            this.button_single_checkFromReader.Size = new System.Drawing.Size(176, 22);
            this.button_single_checkFromReader.TabIndex = 9;
            this.button_single_checkFromReader.Text = "从读者侧检查";
            this.button_single_checkFromReader.UseVisualStyleBackColor = true;
            this.button_single_checkFromReader.Click += new System.EventHandler(this.button_single_checkFromReader_Click);
            // 
            // button_single_checkFromItem
            // 
            this.button_single_checkFromItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_single_checkFromItem.Location = new System.Drawing.Point(236, 34);
            this.button_single_checkFromItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_single_checkFromItem.Name = "button_single_checkFromItem";
            this.button_single_checkFromItem.Size = new System.Drawing.Size(176, 22);
            this.button_single_checkFromItem.TabIndex = 14;
            this.button_single_checkFromItem.Text = "从册侧检查";
            this.button_single_checkFromItem.UseVisualStyleBackColor = true;
            this.button_single_checkFromItem.Click += new System.EventHandler(this.button_single_checkFromItem_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 14);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "读者证条码号(&R):";
            // 
            // textBox_single_itemBarcode
            // 
            this.textBox_single_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_single_itemBarcode.Location = new System.Drawing.Point(95, 34);
            this.textBox_single_itemBarcode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_single_itemBarcode.Name = "textBox_single_itemBarcode";
            this.textBox_single_itemBarcode.Size = new System.Drawing.Size(138, 21);
            this.textBox_single_itemBarcode.TabIndex = 13;
            // 
            // textBox_single_readerBarcode
            // 
            this.textBox_single_readerBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_single_readerBarcode.Location = new System.Drawing.Point(95, 9);
            this.textBox_single_readerBarcode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_single_readerBarcode.Name = "textBox_single_readerBarcode";
            this.textBox_single_readerBarcode.Size = new System.Drawing.Size(138, 21);
            this.textBox_single_readerBarcode.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 36);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "册条码号(&I):";
            // 
            // button_clearInfo
            // 
            this.button_clearInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_clearInfo.Location = new System.Drawing.Point(370, 271);
            this.button_clearInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_clearInfo.Name = "button_clearInfo";
            this.button_clearInfo.Size = new System.Drawing.Size(67, 22);
            this.button_clearInfo.TabIndex = 3;
            this.button_clearInfo.Text = "清除(&C)";
            this.button_clearInfo.UseVisualStyleBackColor = true;
            this.button_clearInfo.Click += new System.EventHandler(this.button_clearInfo_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(9, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
            this.splitContainer_main.Size = new System.Drawing.Size(428, 257);
            this.splitContainer_main.SplitterDistance = 78;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 11;
            // 
            // CheckBorrowInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 303);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_clearInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "CheckBorrowInfoForm";
            this.ShowInTaskbar = false;
            this.Text = "检查借阅信息";
            this.Activated += new System.EventHandler(this.CheckBorrowInfoForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CheckBorrowInfoForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CheckBorrowInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.CheckBorrowInfoForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_batchCheckBorrowInfo.ResumeLayout(false);
            this.tabPage_batchCheckBorrowInfo.PerformLayout();
            this.tabPage_recoverBorrowInfo.ResumeLayout(false);
            this.tabPage_recoverBorrowInfo.PerformLayout();
            this.tabPage_batchAddItemPrice.ResumeLayout(false);
            this.tabPage_batchAddItemPrice.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_beginCheckFromReader;
        private System.Windows.Forms.WebBrowser webBrowser_resultInfo;
        private System.Windows.Forms.Button button_beginCheckFromItem;
        private System.Windows.Forms.Button button_repairReaderSide;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_repairItemSide;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_batchCheckBorrowInfo;
        private System.Windows.Forms.TabPage tabPage_recoverBorrowInfo;
        private System.Windows.Forms.TabPage tabPage_batchAddItemPrice;
        private System.Windows.Forms.Button button_batchAddItemPrice;
        private System.Windows.Forms.CheckBox checkBox_forceCNY;
        private System.Windows.Forms.CheckBox checkBox_overwriteExistPrice;
        private System.Windows.Forms.CheckBox checkBox_displayPriceString;
        private System.Windows.Forms.Button button_clearInfo;
        private System.Windows.Forms.CheckBox checkBox_checkItemBarcodeDup;
        private System.Windows.Forms.CheckBox checkBox_checkReaderBarcodeDup;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button_single_checkFromReader;
        private System.Windows.Forms.Button button_single_checkFromItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_single_itemBarcode;
        private System.Windows.Forms.TextBox textBox_single_readerBarcode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.SplitContainer splitContainer_main;
    }
}