namespace dp2Circulation
{
    partial class PrintOrderForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintOrderForm));
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_load = new System.Windows.Forms.TabPage();
            this.checkBox_print_accepted = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_load_type = new System.Windows.Forms.ComboBox();
            this.button_load_loadFromBatchNo = new System.Windows.Forms.Button();
            this.button_load_loadFromFile = new System.Windows.Forms.Button();
            this.tabPage_saveChange = new System.Windows.Forms.TabPage();
            this.textBox_saveChange_info = new System.Windows.Forms.TextBox();
            this.button_saveChange_saveChange = new System.Windows.Forms.Button();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print_arriveRatioStatis = new System.Windows.Forms.Button();
            this.contextMenuStrip_arriveRatio = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_arriveRatio_outputExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.button_print_outputOrderOption = new System.Windows.Forms.Button();
            this.button_print_outputOrder = new System.Windows.Forms.Button();
            this.button_print_originOption = new System.Windows.Forms.Button();
            this.button_print_printOriginList = new System.Windows.Forms.Button();
            this.button_print_mergedOption = new System.Windows.Forms.Button();
            this.button_print_printOrderList = new System.Windows.Forms.Button();
            this.contextMenuStrip_printOrder = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_printOrder_outputExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.listView_origin = new DigitalPlatform.GUI.ListViewNF();
            this.imageList_lineType = new System.Windows.Forms.ImageList(this.components);
            this.tabControl_items = new System.Windows.Forms.TabControl();
            this.tabPage_originItems = new System.Windows.Forms.TabPage();
            this.tabPage_mergedItems = new System.Windows.Forms.TabPage();
            this.listView_merged = new DigitalPlatform.GUI.ListViewNF();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.button_test = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_load.SuspendLayout();
            this.tabPage_saveChange.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.contextMenuStrip_arriveRatio.SuspendLayout();
            this.contextMenuStrip_printOrder.SuspendLayout();
            this.tabControl_items.SuspendLayout();
            this.tabPage_originItems.SuspendLayout();
            this.tabPage_mergedItems.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(569, 245);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(124, 33);
            this.button_next.TabIndex = 3;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_load);
            this.tabControl_main.Controls.Add(this.tabPage_saveChange);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(3, 3);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(690, 236);
            this.tabControl_main.TabIndex = 2;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_load
            // 
            this.tabPage_load.Controls.Add(this.checkBox_print_accepted);
            this.tabPage_load.Controls.Add(this.label3);
            this.tabPage_load.Controls.Add(this.comboBox_load_type);
            this.tabPage_load.Controls.Add(this.button_load_loadFromBatchNo);
            this.tabPage_load.Controls.Add(this.button_load_loadFromFile);
            this.tabPage_load.Location = new System.Drawing.Point(4, 28);
            this.tabPage_load.Name = "tabPage_load";
            this.tabPage_load.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_load.Size = new System.Drawing.Size(682, 204);
            this.tabPage_load.TabIndex = 0;
            this.tabPage_load.Text = "装载";
            this.tabPage_load.UseVisualStyleBackColor = true;
            // 
            // checkBox_print_accepted
            // 
            this.checkBox_print_accepted.AutoSize = true;
            this.checkBox_print_accepted.Location = new System.Drawing.Point(180, 45);
            this.checkBox_print_accepted.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_print_accepted.Name = "checkBox_print_accepted";
            this.checkBox_print_accepted.Size = new System.Drawing.Size(133, 22);
            this.checkBox_print_accepted.TabIndex = 2;
            this.checkBox_print_accepted.Text = "验收情况(&A)";
            this.checkBox_print_accepted.UseVisualStyleBackColor = true;
            this.checkBox_print_accepted.CheckedChanged += new System.EventHandler(this.checkBox_print_accepted_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "出版物类型(&T):";
            // 
            // comboBox_load_type
            // 
            this.comboBox_load_type.FormattingEnabled = true;
            this.comboBox_load_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_load_type.Location = new System.Drawing.Point(180, 8);
            this.comboBox_load_type.Name = "comboBox_load_type";
            this.comboBox_load_type.Size = new System.Drawing.Size(172, 26);
            this.comboBox_load_type.TabIndex = 1;
            this.comboBox_load_type.Text = "图书";
            this.comboBox_load_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_load_type_SelectedIndexChanged);
            // 
            // button_load_loadFromBatchNo
            // 
            this.button_load_loadFromBatchNo.Location = new System.Drawing.Point(180, 126);
            this.button_load_loadFromBatchNo.Name = "button_load_loadFromBatchNo";
            this.button_load_loadFromBatchNo.Size = new System.Drawing.Size(357, 33);
            this.button_load_loadFromBatchNo.TabIndex = 4;
            this.button_load_loadFromBatchNo.Text = "根据[订购]批次号检索装载(&B)...";
            this.button_load_loadFromBatchNo.UseVisualStyleBackColor = true;
            this.button_load_loadFromBatchNo.Click += new System.EventHandler(this.button_load_loadFromBatchNo_Click);
            // 
            // button_load_loadFromFile
            // 
            this.button_load_loadFromFile.Location = new System.Drawing.Point(180, 86);
            this.button_load_loadFromFile.Name = "button_load_loadFromFile";
            this.button_load_loadFromFile.Size = new System.Drawing.Size(357, 33);
            this.button_load_loadFromFile.TabIndex = 3;
            this.button_load_loadFromFile.Text = "从[订购库]记录路径文件装载(&F)...";
            this.button_load_loadFromFile.UseVisualStyleBackColor = true;
            this.button_load_loadFromFile.Click += new System.EventHandler(this.button_load_loadFromFile_Click);
            // 
            // tabPage_saveChange
            // 
            this.tabPage_saveChange.Controls.Add(this.textBox_saveChange_info);
            this.tabPage_saveChange.Controls.Add(this.button_saveChange_saveChange);
            this.tabPage_saveChange.Location = new System.Drawing.Point(4, 28);
            this.tabPage_saveChange.Name = "tabPage_saveChange";
            this.tabPage_saveChange.Size = new System.Drawing.Size(682, 204);
            this.tabPage_saveChange.TabIndex = 3;
            this.tabPage_saveChange.Text = "保存修改";
            this.tabPage_saveChange.UseVisualStyleBackColor = true;
            // 
            // textBox_saveChange_info
            // 
            this.textBox_saveChange_info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_saveChange_info.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_saveChange_info.Location = new System.Drawing.Point(3, 4);
            this.textBox_saveChange_info.Multiline = true;
            this.textBox_saveChange_info.Name = "textBox_saveChange_info";
            this.textBox_saveChange_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_saveChange_info.Size = new System.Drawing.Size(668, 142);
            this.textBox_saveChange_info.TabIndex = 3;
            // 
            // button_saveChange_saveChange
            // 
            this.button_saveChange_saveChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveChange_saveChange.Location = new System.Drawing.Point(418, 160);
            this.button_saveChange_saveChange.Name = "button_saveChange_saveChange";
            this.button_saveChange_saveChange.Size = new System.Drawing.Size(255, 33);
            this.button_saveChange_saveChange.TabIndex = 2;
            this.button_saveChange_saveChange.Text = "保存对原始数据的修改(&S)";
            this.button_saveChange_saveChange.UseVisualStyleBackColor = true;
            this.button_saveChange_saveChange.Click += new System.EventHandler(this.button_saveChange_saveChange_Click);
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.button_test);
            this.tabPage_print.Controls.Add(this.button_print_arriveRatioStatis);
            this.tabPage_print.Controls.Add(this.button_print_outputOrderOption);
            this.tabPage_print.Controls.Add(this.button_print_outputOrder);
            this.tabPage_print.Controls.Add(this.button_print_originOption);
            this.tabPage_print.Controls.Add(this.button_print_printOriginList);
            this.tabPage_print.Controls.Add(this.button_print_mergedOption);
            this.tabPage_print.Controls.Add(this.button_print_printOrderList);
            this.tabPage_print.Location = new System.Drawing.Point(4, 28);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(682, 204);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印 / 输出";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print_arriveRatioStatis
            // 
            this.button_print_arriveRatioStatis.ContextMenuStrip = this.contextMenuStrip_arriveRatio;
            this.button_print_arriveRatioStatis.Enabled = false;
            this.button_print_arriveRatioStatis.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_print_arriveRatioStatis.Location = new System.Drawing.Point(8, 148);
            this.button_print_arriveRatioStatis.Name = "button_print_arriveRatioStatis";
            this.button_print_arriveRatioStatis.Size = new System.Drawing.Size(330, 33);
            this.button_print_arriveRatioStatis.TabIndex = 7;
            this.button_print_arriveRatioStatis.Text = "到货率统计(&A)...";
            this.button_print_arriveRatioStatis.UseVisualStyleBackColor = true;
            this.button_print_arriveRatioStatis.Click += new System.EventHandler(this.button_print_arriveRatioStatis_Click);
            // 
            // contextMenuStrip_arriveRatio
            // 
            this.contextMenuStrip_arriveRatio.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_arriveRatio.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_arriveRatio_outputExcel});
            this.contextMenuStrip_arriveRatio.Name = "contextMenuStrip_printOrder";
            this.contextMenuStrip_arriveRatio.Size = new System.Drawing.Size(206, 32);
            // 
            // toolStripMenuItem_arriveRatio_outputExcel
            // 
            this.toolStripMenuItem_arriveRatio_outputExcel.Name = "toolStripMenuItem_arriveRatio_outputExcel";
            this.toolStripMenuItem_arriveRatio_outputExcel.Size = new System.Drawing.Size(205, 28);
            this.toolStripMenuItem_arriveRatio_outputExcel.Text = "输出 Excel 文件";
            this.toolStripMenuItem_arriveRatio_outputExcel.Click += new System.EventHandler(this.toolStripMenuItem_arriveRatio_outputExcel_Click);
            // 
            // button_print_outputOrderOption
            // 
            this.button_print_outputOrderOption.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_print_outputOrderOption.Location = new System.Drawing.Point(345, 110);
            this.button_print_outputOrderOption.Name = "button_print_outputOrderOption";
            this.button_print_outputOrderOption.Size = new System.Drawing.Size(228, 33);
            this.button_print_outputOrderOption.TabIndex = 6;
            this.button_print_outputOrderOption.Text = "输出选项(&F)...";
            this.button_print_outputOrderOption.UseVisualStyleBackColor = true;
            this.button_print_outputOrderOption.Click += new System.EventHandler(this.button_print_outputOrderOption_Click);
            // 
            // button_print_outputOrder
            // 
            this.button_print_outputOrder.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_print_outputOrder.Location = new System.Drawing.Point(8, 110);
            this.button_print_outputOrder.Name = "button_print_outputOrder";
            this.button_print_outputOrder.Size = new System.Drawing.Size(330, 33);
            this.button_print_outputOrder.TabIndex = 5;
            this.button_print_outputOrder.Text = "输出订单(&O)...";
            this.button_print_outputOrder.UseVisualStyleBackColor = true;
            this.button_print_outputOrder.Click += new System.EventHandler(this.button_print_outputOrder_Click);
            // 
            // button_print_originOption
            // 
            this.button_print_originOption.Location = new System.Drawing.Point(345, 48);
            this.button_print_originOption.Name = "button_print_originOption";
            this.button_print_originOption.Size = new System.Drawing.Size(228, 33);
            this.button_print_originOption.TabIndex = 4;
            this.button_print_originOption.Text = "原始数据打印选项(&T)...";
            this.button_print_originOption.UseVisualStyleBackColor = true;
            this.button_print_originOption.Click += new System.EventHandler(this.button_print_originOption_Click);
            // 
            // button_print_printOriginList
            // 
            this.button_print_printOriginList.Location = new System.Drawing.Point(8, 48);
            this.button_print_printOriginList.Name = "button_print_printOriginList";
            this.button_print_printOriginList.Size = new System.Drawing.Size(330, 33);
            this.button_print_printOriginList.TabIndex = 3;
            this.button_print_printOriginList.Text = "打印原始数据(&R)...";
            this.button_print_printOriginList.UseVisualStyleBackColor = true;
            this.button_print_printOriginList.Click += new System.EventHandler(this.button_print_printOriginList_Click);
            // 
            // button_print_mergedOption
            // 
            this.button_print_mergedOption.Location = new System.Drawing.Point(345, 8);
            this.button_print_mergedOption.Name = "button_print_mergedOption";
            this.button_print_mergedOption.Size = new System.Drawing.Size(228, 33);
            this.button_print_mergedOption.TabIndex = 2;
            this.button_print_mergedOption.Text = "订单打印选项(&O)...";
            this.button_print_mergedOption.UseVisualStyleBackColor = true;
            this.button_print_mergedOption.Click += new System.EventHandler(this.button_merged_print_option_Click);
            // 
            // button_print_printOrderList
            // 
            this.button_print_printOrderList.ContextMenuStrip = this.contextMenuStrip_printOrder;
            this.button_print_printOrderList.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_print_printOrderList.Location = new System.Drawing.Point(8, 8);
            this.button_print_printOrderList.Name = "button_print_printOrderList";
            this.button_print_printOrderList.Size = new System.Drawing.Size(330, 33);
            this.button_print_printOrderList.TabIndex = 0;
            this.button_print_printOrderList.Text = "打印订单(&P)...";
            this.button_print_printOrderList.UseVisualStyleBackColor = true;
            this.button_print_printOrderList.Click += new System.EventHandler(this.button_print_printOrderList_Click);
            // 
            // contextMenuStrip_printOrder
            // 
            this.contextMenuStrip_printOrder.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_printOrder.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_printOrder_outputExcel});
            this.contextMenuStrip_printOrder.Name = "contextMenuStrip_printOrder";
            this.contextMenuStrip_printOrder.Size = new System.Drawing.Size(206, 32);
            // 
            // toolStripMenuItem_printOrder_outputExcel
            // 
            this.toolStripMenuItem_printOrder_outputExcel.Name = "toolStripMenuItem_printOrder_outputExcel";
            this.toolStripMenuItem_printOrder_outputExcel.Size = new System.Drawing.Size(205, 28);
            this.toolStripMenuItem_printOrder_outputExcel.Text = "输出 Excel 文件";
            this.toolStripMenuItem_printOrder_outputExcel.Click += new System.EventHandler(this.toolStripMenuItem_outputExcel_Click);
            // 
            // listView_origin
            // 
            this.listView_origin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_origin.FullRowSelect = true;
            this.listView_origin.HideSelection = false;
            this.listView_origin.LargeImageList = this.imageList_lineType;
            this.listView_origin.Location = new System.Drawing.Point(3, 3);
            this.listView_origin.Name = "listView_origin";
            this.listView_origin.Size = new System.Drawing.Size(676, 140);
            this.listView_origin.SmallImageList = this.imageList_lineType;
            this.listView_origin.TabIndex = 4;
            this.listView_origin.UseCompatibleStateImageBehavior = false;
            this.listView_origin.View = System.Windows.Forms.View.Details;
            this.listView_origin.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_origin_ColumnClick);
            this.listView_origin.DoubleClick += new System.EventHandler(this.listView_origin_DoubleClick);
            this.listView_origin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_origin_MouseUp);
            // 
            // imageList_lineType
            // 
            this.imageList_lineType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineType.ImageStream")));
            this.imageList_lineType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_lineType.Images.SetKeyName(0, "WarningHS.png");
            this.imageList_lineType.Images.SetKeyName(1, "Book_angleHS.png");
            this.imageList_lineType.Images.SetKeyName(2, "Book_openHS.png");
            // 
            // tabControl_items
            // 
            this.tabControl_items.Controls.Add(this.tabPage_originItems);
            this.tabControl_items.Controls.Add(this.tabPage_mergedItems);
            this.tabControl_items.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_items.Location = new System.Drawing.Point(3, 284);
            this.tabControl_items.Name = "tabControl_items";
            this.tabControl_items.SelectedIndex = 0;
            this.tabControl_items.Size = new System.Drawing.Size(690, 178);
            this.tabControl_items.TabIndex = 5;
            // 
            // tabPage_originItems
            // 
            this.tabPage_originItems.Controls.Add(this.listView_origin);
            this.tabPage_originItems.Location = new System.Drawing.Point(4, 28);
            this.tabPage_originItems.Name = "tabPage_originItems";
            this.tabPage_originItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_originItems.Size = new System.Drawing.Size(682, 146);
            this.tabPage_originItems.TabIndex = 0;
            this.tabPage_originItems.Text = "原始数据";
            this.tabPage_originItems.UseVisualStyleBackColor = true;
            // 
            // tabPage_mergedItems
            // 
            this.tabPage_mergedItems.Controls.Add(this.listView_merged);
            this.tabPage_mergedItems.Location = new System.Drawing.Point(4, 28);
            this.tabPage_mergedItems.Name = "tabPage_mergedItems";
            this.tabPage_mergedItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_mergedItems.Size = new System.Drawing.Size(682, 146);
            this.tabPage_mergedItems.TabIndex = 1;
            this.tabPage_mergedItems.Text = "合并后";
            this.tabPage_mergedItems.UseVisualStyleBackColor = true;
            // 
            // listView_merged
            // 
            this.listView_merged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_merged.FullRowSelect = true;
            this.listView_merged.HideSelection = false;
            this.listView_merged.LargeImageList = this.imageList_lineType;
            this.listView_merged.Location = new System.Drawing.Point(3, 3);
            this.listView_merged.Name = "listView_merged";
            this.listView_merged.Size = new System.Drawing.Size(676, 140);
            this.listView_merged.SmallImageList = this.imageList_lineType;
            this.listView_merged.TabIndex = 5;
            this.listView_merged.UseCompatibleStateImageBehavior = false;
            this.listView_merged.View = System.Windows.Forms.View.Details;
            this.listView_merged.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_merged_ColumnClick);
            this.listView_merged.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_merged_MouseUp);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.tabControl_main, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.tabControl_items, 0, 2);
            this.tableLayoutPanel_main.Controls.Add(this.button_next, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 3;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(696, 465);
            this.tableLayoutPanel_main.TabIndex = 6;
            // 
            // button_test
            // 
            this.button_test.Location = new System.Drawing.Point(345, 148);
            this.button_test.Name = "button_test";
            this.button_test.Size = new System.Drawing.Size(228, 33);
            this.button_test.TabIndex = 8;
            this.button_test.Text = "test";
            this.button_test.UseVisualStyleBackColor = true;
            this.button_test.Click += new System.EventHandler(this.button_test_Click);
            // 
            // PrintOrderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 465);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrintOrderForm";
            this.Text = "打印订单";
            this.Activated += new System.EventHandler(this.PrintOrderForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintOrderForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintOrderForm_FormClosed);
            this.Load += new System.EventHandler(this.PrintOrderForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_load.ResumeLayout(false);
            this.tabPage_load.PerformLayout();
            this.tabPage_saveChange.ResumeLayout(false);
            this.tabPage_saveChange.PerformLayout();
            this.tabPage_print.ResumeLayout(false);
            this.contextMenuStrip_arriveRatio.ResumeLayout(false);
            this.contextMenuStrip_printOrder.ResumeLayout(false);
            this.tabControl_items.ResumeLayout(false);
            this.tabPage_originItems.ResumeLayout(false);
            this.tabPage_mergedItems.ResumeLayout(false);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_load;
        private System.Windows.Forms.Button button_load_loadFromBatchNo;
        private System.Windows.Forms.Button button_load_loadFromFile;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print_mergedOption;
        private System.Windows.Forms.Button button_print_printOrderList;
        private DigitalPlatform.GUI.ListViewNF listView_origin;
        private System.Windows.Forms.TabControl tabControl_items;
        private System.Windows.Forms.TabPage tabPage_originItems;
        private System.Windows.Forms.TabPage tabPage_mergedItems;
        private DigitalPlatform.GUI.ListViewNF listView_merged;
        private System.Windows.Forms.Button button_print_printOriginList;
        private System.Windows.Forms.Button button_print_originOption;
        private System.Windows.Forms.ImageList imageList_lineType;
        private System.Windows.Forms.TabPage tabPage_saveChange;
        private System.Windows.Forms.Button button_saveChange_saveChange;
        private System.Windows.Forms.TextBox textBox_saveChange_info;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_load_type;
        private System.Windows.Forms.Button button_print_outputOrder;
        private System.Windows.Forms.Button button_print_outputOrderOption;
        private System.Windows.Forms.CheckBox checkBox_print_accepted;
        private System.Windows.Forms.Button button_print_arriveRatioStatis;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_printOrder;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_printOrder_outputExcel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_arriveRatio;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_arriveRatio_outputExcel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Button button_test;
    }
}