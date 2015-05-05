namespace dp2Circulation
{
    partial class SettlementForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettlementForm));
            this.listView_amerced = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_amerced_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_readerBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_price = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_borrowDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_borrowPeriod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_returnDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_returnOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_itemBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_amerceOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_amerceTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_settlementOperator = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_settlementTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_amerced_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_range = new System.Windows.Forms.TabPage();
            this.comboBox_range_state = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_range_endCtlno = new System.Windows.Forms.TextBox();
            this.textBox_range_startCtlno = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButton_range_ctlno = new System.Windows.Forms.RadioButton();
            this.radioButton_range_amerceOperTime = new System.Windows.Forms.RadioButton();
            this.dateControl_end = new DigitalPlatform.CommonControl.DateControl();
            this.dateControl_start = new DigitalPlatform.CommonControl.DateControl();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_items = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_amerced = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip_items = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_items_message1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_items_message2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_items = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_items_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_items_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_items_unSelectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_items_selectAmercedItems = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_items_selectSettlementedItems = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_items_settlement = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_items_undoSettlement = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_items_useCheck = new System.Windows.Forms.ToolStripButton();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_print_option = new System.Windows.Forms.Button();
            this.button_print_printSettlemented = new System.Windows.Forms.Button();
            this.checkBox_sumByAmerceOperator = new System.Windows.Forms.CheckBox();
            this.button_print_printAll = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_range.SuspendLayout();
            this.tabPage_items.SuspendLayout();
            this.tableLayoutPanel_amerced.SuspendLayout();
            this.statusStrip_items.SuspendLayout();
            this.toolStrip_items.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_amerced
            // 
            this.listView_amerced.BackColor = System.Drawing.Color.Honeydew;
            this.listView_amerced.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_amerced.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_amerced_id,
            this.columnHeader_amerced_state,
            this.columnHeader_amerced_readerBarcode,
            this.columnHeader_amerced_libraryCode,
            this.columnHeader_amerced_price,
            this.columnHeader_amerced_comment,
            this.columnHeader_amerced_reason,
            this.columnHeader_amerced_borrowDate,
            this.columnHeader_amerced_borrowPeriod,
            this.columnHeader_amerced_returnDate,
            this.columnHeader_amerced_returnOperator,
            this.columnHeader_amerced_itemBarcode,
            this.columnHeader_amerced_summary,
            this.columnHeader_amerced_amerceOperator,
            this.columnHeader_amerced_amerceTime,
            this.columnHeader_amerced_settlementOperator,
            this.columnHeader_amerced_settlementTime,
            this.columnHeader_amerced_recpath});
            this.listView_amerced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_amerced.FullRowSelect = true;
            this.listView_amerced.HideSelection = false;
            this.listView_amerced.LargeImageList = this.imageList_itemType;
            this.listView_amerced.Location = new System.Drawing.Point(0, 0);
            this.listView_amerced.Margin = new System.Windows.Forms.Padding(0);
            this.listView_amerced.Name = "listView_amerced";
            this.listView_amerced.Size = new System.Drawing.Size(376, 181);
            this.listView_amerced.SmallImageList = this.imageList_itemType;
            this.listView_amerced.TabIndex = 0;
            this.listView_amerced.UseCompatibleStateImageBehavior = false;
            this.listView_amerced.View = System.Windows.Forms.View.Details;
            this.listView_amerced.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_amerced_ColumnClick);
            this.listView_amerced.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_amerced_ItemChecked);
            this.listView_amerced.SelectedIndexChanged += new System.EventHandler(this.listView_amerced_SelectedIndexChanged);
            this.listView_amerced.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_amerced_MouseUp);
            // 
            // columnHeader_amerced_id
            // 
            this.columnHeader_amerced_id.Text = "ID";
            this.columnHeader_amerced_id.Width = 200;
            // 
            // columnHeader_amerced_state
            // 
            this.columnHeader_amerced_state.Text = "状态";
            this.columnHeader_amerced_state.Width = 100;
            // 
            // columnHeader_amerced_readerBarcode
            // 
            this.columnHeader_amerced_readerBarcode.Text = "读者证条码号";
            this.columnHeader_amerced_readerBarcode.Width = 100;
            // 
            // columnHeader_amerced_libraryCode
            // 
            this.columnHeader_amerced_libraryCode.Text = "馆代码";
            this.columnHeader_amerced_libraryCode.Width = 100;
            // 
            // columnHeader_amerced_price
            // 
            this.columnHeader_amerced_price.Text = "金额";
            this.columnHeader_amerced_price.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_amerced_price.Width = 97;
            // 
            // columnHeader_amerced_comment
            // 
            this.columnHeader_amerced_comment.Text = "注释";
            // 
            // columnHeader_amerced_reason
            // 
            this.columnHeader_amerced_reason.Text = "原因";
            this.columnHeader_amerced_reason.Width = 91;
            // 
            // columnHeader_amerced_borrowDate
            // 
            this.columnHeader_amerced_borrowDate.Text = "起点日期";
            this.columnHeader_amerced_borrowDate.Width = 120;
            // 
            // columnHeader_amerced_borrowPeriod
            // 
            this.columnHeader_amerced_borrowPeriod.Text = "期限";
            this.columnHeader_amerced_borrowPeriod.Width = 100;
            // 
            // columnHeader_amerced_returnDate
            // 
            this.columnHeader_amerced_returnDate.Text = "终点日期";
            this.columnHeader_amerced_returnDate.Width = 120;
            // 
            // columnHeader_amerced_returnOperator
            // 
            this.columnHeader_amerced_returnOperator.Text = "还书操作者";
            this.columnHeader_amerced_returnOperator.Width = 100;
            // 
            // columnHeader_amerced_itemBarcode
            // 
            this.columnHeader_amerced_itemBarcode.Text = "册条码号";
            this.columnHeader_amerced_itemBarcode.Width = 98;
            // 
            // columnHeader_amerced_summary
            // 
            this.columnHeader_amerced_summary.Text = "摘要";
            this.columnHeader_amerced_summary.Width = 120;
            // 
            // columnHeader_amerced_amerceOperator
            // 
            this.columnHeader_amerced_amerceOperator.Text = "收费者";
            this.columnHeader_amerced_amerceOperator.Width = 100;
            // 
            // columnHeader_amerced_amerceTime
            // 
            this.columnHeader_amerced_amerceTime.Text = "收费日期";
            this.columnHeader_amerced_amerceTime.Width = 120;
            // 
            // columnHeader_amerced_settlementOperator
            // 
            this.columnHeader_amerced_settlementOperator.Text = "结算者";
            this.columnHeader_amerced_settlementOperator.Width = 100;
            // 
            // columnHeader_amerced_settlementTime
            // 
            this.columnHeader_amerced_settlementTime.Text = "结算日期";
            this.columnHeader_amerced_settlementTime.Width = 120;
            // 
            // columnHeader_amerced_recpath
            // 
            this.columnHeader_amerced_recpath.Text = "记录路径";
            this.columnHeader_amerced_recpath.Width = 100;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.White;
            this.imageList_itemType.Images.SetKeyName(0, "amerced_type.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "settlemented_type.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "old_settlemented_type.bmp");
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_range);
            this.tabControl_main.Controls.Add(this.tabPage_items);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(388, 261);
            this.tabControl_main.TabIndex = 4;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_range
            // 
            this.tabPage_range.AutoScroll = true;
            this.tabPage_range.Controls.Add(this.comboBox_range_state);
            this.tabPage_range.Controls.Add(this.label5);
            this.tabPage_range.Controls.Add(this.textBox_range_endCtlno);
            this.tabPage_range.Controls.Add(this.textBox_range_startCtlno);
            this.tabPage_range.Controls.Add(this.label4);
            this.tabPage_range.Controls.Add(this.label3);
            this.tabPage_range.Controls.Add(this.radioButton_range_ctlno);
            this.tabPage_range.Controls.Add(this.radioButton_range_amerceOperTime);
            this.tabPage_range.Controls.Add(this.dateControl_end);
            this.tabPage_range.Controls.Add(this.dateControl_start);
            this.tabPage_range.Controls.Add(this.label2);
            this.tabPage_range.Controls.Add(this.label1);
            this.tabPage_range.Location = new System.Drawing.Point(4, 22);
            this.tabPage_range.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_range.Name = "tabPage_range";
            this.tabPage_range.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_range.Size = new System.Drawing.Size(380, 235);
            this.tabPage_range.TabIndex = 0;
            this.tabPage_range.Text = " 范围 ";
            this.tabPage_range.UseVisualStyleBackColor = true;
            // 
            // comboBox_range_state
            // 
            this.comboBox_range_state.FormattingEnabled = true;
            this.comboBox_range_state.Items.AddRange(new object[] {
            "<全部>",
            "已收费",
            "旧结算"});
            this.comboBox_range_state.Location = new System.Drawing.Point(76, 198);
            this.comboBox_range_state.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_range_state.Name = "comboBox_range_state";
            this.comboBox_range_state.Size = new System.Drawing.Size(157, 20);
            this.comboBox_range_state.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 201);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "状态(&S):";
            // 
            // textBox_range_endCtlno
            // 
            this.textBox_range_endCtlno.Enabled = false;
            this.textBox_range_endCtlno.Location = new System.Drawing.Point(134, 163);
            this.textBox_range_endCtlno.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_range_endCtlno.Name = "textBox_range_endCtlno";
            this.textBox_range_endCtlno.Size = new System.Drawing.Size(99, 21);
            this.textBox_range_endCtlno.TabIndex = 9;
            // 
            // textBox_range_startCtlno
            // 
            this.textBox_range_startCtlno.Enabled = false;
            this.textBox_range_startCtlno.Location = new System.Drawing.Point(134, 138);
            this.textBox_range_startCtlno.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_range_startCtlno.Name = "textBox_range_startCtlno";
            this.textBox_range_startCtlno.Size = new System.Drawing.Size(99, 21);
            this.textBox_range_startCtlno.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(52, 166);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "结束号(&E):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 140);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "起始号(&S):";
            // 
            // radioButton_range_ctlno
            // 
            this.radioButton_range_ctlno.AutoSize = true;
            this.radioButton_range_ctlno.Location = new System.Drawing.Point(5, 109);
            this.radioButton_range_ctlno.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_range_ctlno.Name = "radioButton_range_ctlno";
            this.radioButton_range_ctlno.Size = new System.Drawing.Size(125, 16);
            this.radioButton_range_ctlno.TabIndex = 5;
            this.radioButton_range_ctlno.Text = "记录索引号范围(&N)";
            this.radioButton_range_ctlno.UseVisualStyleBackColor = true;
            this.radioButton_range_ctlno.CheckedChanged += new System.EventHandler(this.radioButton_range_ctlno_CheckedChanged);
            // 
            // radioButton_range_amerceOperTime
            // 
            this.radioButton_range_amerceOperTime.AutoSize = true;
            this.radioButton_range_amerceOperTime.Checked = true;
            this.radioButton_range_amerceOperTime.Location = new System.Drawing.Point(5, 20);
            this.radioButton_range_amerceOperTime.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton_range_amerceOperTime.Name = "radioButton_range_amerceOperTime";
            this.radioButton_range_amerceOperTime.Size = new System.Drawing.Size(125, 16);
            this.radioButton_range_amerceOperTime.TabIndex = 0;
            this.radioButton_range_amerceOperTime.TabStop = true;
            this.radioButton_range_amerceOperTime.Text = "按收费时间范围(&T)";
            this.radioButton_range_amerceOperTime.UseVisualStyleBackColor = true;
            this.radioButton_range_amerceOperTime.CheckedChanged += new System.EventHandler(this.radioButton_range_amerceOperTime_CheckedChanged);
            // 
            // dateControl_end
            // 
            this.dateControl_end.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_end.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_end.Location = new System.Drawing.Point(134, 78);
            this.dateControl_end.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_end.Name = "dateControl_end";
            this.dateControl_end.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_end.Size = new System.Drawing.Size(116, 22);
            this.dateControl_end.TabIndex = 4;
            this.dateControl_end.Value = new System.DateTime(((long)(0)));
            // 
            // dateControl_start
            // 
            this.dateControl_start.BackColor = System.Drawing.SystemColors.Window;
            this.dateControl_start.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dateControl_start.Location = new System.Drawing.Point(134, 52);
            this.dateControl_start.Margin = new System.Windows.Forms.Padding(2);
            this.dateControl_start.Name = "dateControl_start";
            this.dateControl_start.Padding = new System.Windows.Forms.Padding(4);
            this.dateControl_start.Size = new System.Drawing.Size(116, 22);
            this.dateControl_start.TabIndex = 2;
            this.dateControl_start.Value = new System.DateTime(((long)(0)));
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 80);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "结束日(&E):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 52);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "起始日(&S):";
            // 
            // tabPage_items
            // 
            this.tabPage_items.Controls.Add(this.tableLayoutPanel_amerced);
            this.tabPage_items.Location = new System.Drawing.Point(4, 22);
            this.tabPage_items.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_items.Name = "tabPage_items";
            this.tabPage_items.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_items.Size = new System.Drawing.Size(380, 232);
            this.tabPage_items.TabIndex = 1;
            this.tabPage_items.Text = " 费用事项 ";
            this.tabPage_items.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_amerced
            // 
            this.tableLayoutPanel_amerced.ColumnCount = 1;
            this.tableLayoutPanel_amerced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amerced.Controls.Add(this.listView_amerced, 0, 0);
            this.tableLayoutPanel_amerced.Controls.Add(this.statusStrip_items, 0, 1);
            this.tableLayoutPanel_amerced.Controls.Add(this.toolStrip_items, 0, 2);
            this.tableLayoutPanel_amerced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_amerced.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_amerced.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_amerced.Name = "tableLayoutPanel_amerced";
            this.tableLayoutPanel_amerced.RowCount = 3;
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amerced.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_amerced.Size = new System.Drawing.Size(376, 228);
            this.tableLayoutPanel_amerced.TabIndex = 3;
            // 
            // statusStrip_items
            // 
            this.statusStrip_items.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_items_message1,
            this.toolStripStatusLabel_items_message2});
            this.statusStrip_items.Location = new System.Drawing.Point(0, 181);
            this.statusStrip_items.Name = "statusStrip_items";
            this.statusStrip_items.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip_items.Size = new System.Drawing.Size(376, 22);
            this.statusStrip_items.SizingGrip = false;
            this.statusStrip_items.TabIndex = 1;
            this.statusStrip_items.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_items_message1
            // 
            this.toolStripStatusLabel_items_message1.Name = "toolStripStatusLabel_items_message1";
            this.toolStripStatusLabel_items_message1.Size = new System.Drawing.Size(365, 17);
            this.toolStripStatusLabel_items_message1.Spring = true;
            this.toolStripStatusLabel_items_message1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripStatusLabel_items_message1.ToolTipText = "各类型事项数";
            // 
            // toolStripStatusLabel_items_message2
            // 
            this.toolStripStatusLabel_items_message2.Name = "toolStripStatusLabel_items_message2";
            this.toolStripStatusLabel_items_message2.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusLabel_items_message2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripStatusLabel_items_message2.ToolTipText = "选定的事项数";
            // 
            // toolStrip_items
            // 
            this.toolStrip_items.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_items.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_items_remove,
            this.toolStripSeparator1,
            this.toolStripButton_items_selectAll,
            this.toolStripButton_items_unSelectAll,
            this.toolStripSeparator2,
            this.toolStripButton_items_selectAmercedItems,
            this.toolStripButton_items_selectSettlementedItems,
            this.toolStripSeparator3,
            this.toolStripButton_items_settlement,
            this.toolStripButton_items_undoSettlement,
            this.toolStripButton_items_useCheck});
            this.toolStrip_items.Location = new System.Drawing.Point(0, 203);
            this.toolStrip_items.Name = "toolStrip_items";
            this.toolStrip_items.Size = new System.Drawing.Size(376, 25);
            this.toolStrip_items.TabIndex = 2;
            this.toolStrip_items.Text = "toolStrip1";
            // 
            // toolStripButton_items_remove
            // 
            this.toolStripButton_items_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_items_remove.Enabled = false;
            this.toolStripButton_items_remove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_remove.Image")));
            this.toolStripButton_items_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_remove.Name = "toolStripButton_items_remove";
            this.toolStripButton_items_remove.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_items_remove.Text = "移除";
            this.toolStripButton_items_remove.ToolTipText = "移除所选定的事项";
            this.toolStripButton_items_remove.Click += new System.EventHandler(this.toolStripButton_items_remove_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_items_selectAll
            // 
            this.toolStripButton_items_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_items_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_selectAll.Image")));
            this.toolStripButton_items_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_selectAll.Name = "toolStripButton_items_selectAll";
            this.toolStripButton_items_selectAll.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_items_selectAll.Text = "全选";
            this.toolStripButton_items_selectAll.Click += new System.EventHandler(this.toolStripButton_items_selectAll_Click);
            // 
            // toolStripButton_items_unSelectAll
            // 
            this.toolStripButton_items_unSelectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_items_unSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_unSelectAll.Image")));
            this.toolStripButton_items_unSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_unSelectAll.Name = "toolStripButton_items_unSelectAll";
            this.toolStripButton_items_unSelectAll.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_items_unSelectAll.Text = "全不选";
            this.toolStripButton_items_unSelectAll.Click += new System.EventHandler(this.toolStripButton_items_unSelectAll_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_items_selectAmercedItems
            // 
            this.toolStripButton_items_selectAmercedItems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_items_selectAmercedItems.Enabled = false;
            this.toolStripButton_items_selectAmercedItems.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_selectAmercedItems.Image")));
            this.toolStripButton_items_selectAmercedItems.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButton_items_selectAmercedItems.Name = "toolStripButton_items_selectAmercedItems";
            this.toolStripButton_items_selectAmercedItems.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_items_selectAmercedItems.Text = "选定全部已收费事项";
            this.toolStripButton_items_selectAmercedItems.Click += new System.EventHandler(this.toolStripButton_items_selectAmercedItems_Click);
            // 
            // toolStripButton_items_selectSettlementedItems
            // 
            this.toolStripButton_items_selectSettlementedItems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_items_selectSettlementedItems.Enabled = false;
            this.toolStripButton_items_selectSettlementedItems.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_selectSettlementedItems.Image")));
            this.toolStripButton_items_selectSettlementedItems.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButton_items_selectSettlementedItems.Name = "toolStripButton_items_selectSettlementedItems";
            this.toolStripButton_items_selectSettlementedItems.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_items_selectSettlementedItems.Text = "选定全部已结算((包括新结算和旧结算))事项";
            this.toolStripButton_items_selectSettlementedItems.Click += new System.EventHandler(this.toolStripButton_items_selectSettlementedItems_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_items_settlement
            // 
            this.toolStripButton_items_settlement.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_items_settlement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_items_settlement.Enabled = false;
            this.toolStripButton_items_settlement.Font = new System.Drawing.Font("Tahoma", 8.400001F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripButton_items_settlement.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_settlement.Image")));
            this.toolStripButton_items_settlement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_settlement.Name = "toolStripButton_items_settlement";
            this.toolStripButton_items_settlement.Size = new System.Drawing.Size(37, 22);
            this.toolStripButton_items_settlement.Text = "结算";
            this.toolStripButton_items_settlement.Click += new System.EventHandler(this.toolStripButton_items_settlement_Click);
            // 
            // toolStripButton_items_undoSettlement
            // 
            this.toolStripButton_items_undoSettlement.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_items_undoSettlement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_items_undoSettlement.Enabled = false;
            this.toolStripButton_items_undoSettlement.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_undoSettlement.Image")));
            this.toolStripButton_items_undoSettlement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_undoSettlement.Name = "toolStripButton_items_undoSettlement";
            this.toolStripButton_items_undoSettlement.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_items_undoSettlement.Text = "撤销结算";
            this.toolStripButton_items_undoSettlement.Click += new System.EventHandler(this.toolStripButton_items_undoSettlement_Click);
            // 
            // toolStripButton_items_useCheck
            // 
            this.toolStripButton_items_useCheck.CheckOnClick = true;
            this.toolStripButton_items_useCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_items_useCheck.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_items_useCheck.Image")));
            this.toolStripButton_items_useCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_items_useCheck.Name = "toolStripButton_items_useCheck";
            this.toolStripButton_items_useCheck.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_items_useCheck.Text = "使用勾选";
            this.toolStripButton_items_useCheck.Click += new System.EventHandler(this.toolStripButton_items_useCheck_Click);
            // 
            // tabPage_print
            // 
            this.tabPage_print.AutoScroll = true;
            this.tabPage_print.Controls.Add(this.groupBox1);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(380, 232);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = " 打印单据 ";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_print_option);
            this.groupBox1.Controls.Add(this.button_print_printSettlemented);
            this.groupBox1.Controls.Add(this.checkBox_sumByAmerceOperator);
            this.groupBox1.Controls.Add(this.button_print_printAll);
            this.groupBox1.Location = new System.Drawing.Point(7, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(254, 191);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 结算清单 ";
            // 
            // button_print_option
            // 
            this.button_print_option.Location = new System.Drawing.Point(5, 164);
            this.button_print_option.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_option.Name = "button_print_option";
            this.button_print_option.Size = new System.Drawing.Size(167, 22);
            this.button_print_option.TabIndex = 3;
            this.button_print_option.Text = "选项(&O)...";
            this.button_print_option.UseVisualStyleBackColor = true;
            this.button_print_option.Click += new System.EventHandler(this.button_print_option_Click);
            // 
            // button_print_printSettlemented
            // 
            this.button_print_printSettlemented.Location = new System.Drawing.Point(5, 77);
            this.button_print_printSettlemented.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_printSettlemented.Name = "button_print_printSettlemented";
            this.button_print_printSettlemented.Size = new System.Drawing.Size(167, 26);
            this.button_print_printSettlemented.TabIndex = 1;
            this.button_print_printSettlemented.Text = "只打印本次结算事项(&S)...";
            this.button_print_printSettlemented.UseVisualStyleBackColor = true;
            this.button_print_printSettlemented.Click += new System.EventHandler(this.button_print_printSettlemented_Click);
            // 
            // checkBox_sumByAmerceOperator
            // 
            this.checkBox_sumByAmerceOperator.AutoSize = true;
            this.checkBox_sumByAmerceOperator.Location = new System.Drawing.Point(5, 28);
            this.checkBox_sumByAmerceOperator.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_sumByAmerceOperator.Name = "checkBox_sumByAmerceOperator";
            this.checkBox_sumByAmerceOperator.Size = new System.Drawing.Size(198, 16);
            this.checkBox_sumByAmerceOperator.TabIndex = 0;
            this.checkBox_sumByAmerceOperator.Text = "按收费者排序并打印小计金额(&A)";
            this.checkBox_sumByAmerceOperator.UseVisualStyleBackColor = true;
            // 
            // button_print_printAll
            // 
            this.button_print_printAll.Location = new System.Drawing.Point(5, 107);
            this.button_print_printAll.Margin = new System.Windows.Forms.Padding(2);
            this.button_print_printAll.Name = "button_print_printAll";
            this.button_print_printAll.Size = new System.Drawing.Size(167, 22);
            this.button_print_printAll.TabIndex = 2;
            this.button_print_printAll.Text = "打印全部事项(&A)...";
            this.button_print_printAll.UseVisualStyleBackColor = true;
            this.button_print_printAll.Click += new System.EventHandler(this.button_print_printAll_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(313, 275);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(76, 22);
            this.button_next.TabIndex = 5;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // SettlementForm
            // 
            this.AcceptButton = this.button_next;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 307);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SettlementForm";
            this.ShowInTaskbar = false;
            this.Text = "结算窗";
            this.Activated += new System.EventHandler(this.SettlementForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettlementForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettlementForm_FormClosed);
            this.Load += new System.EventHandler(this.SettlementForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_range.ResumeLayout(false);
            this.tabPage_range.PerformLayout();
            this.tabPage_items.ResumeLayout(false);
            this.tableLayoutPanel_amerced.ResumeLayout(false);
            this.tableLayoutPanel_amerced.PerformLayout();
            this.statusStrip_items.ResumeLayout(false);
            this.statusStrip_items.PerformLayout();
            this.toolStrip_items.ResumeLayout(false);
            this.toolStrip_items.PerformLayout();
            this.tabPage_print.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_amerced;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_itemBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_price;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_reason;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_borrowDate;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_borrowPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_returnDate;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_id;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_returnOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_state;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_range;
        private System.Windows.Forms.TabPage tabPage_items;
        private System.Windows.Forms.Button button_next;
        private DigitalPlatform.CommonControl.DateControl dateControl_end;
        private DigitalPlatform.CommonControl.DateControl dateControl_start;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.RadioButton radioButton_range_amerceOperTime;
        private System.Windows.Forms.RadioButton radioButton_range_ctlno;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_range_endCtlno;
        private System.Windows.Forms.TextBox textBox_range_startCtlno;
        private System.Windows.Forms.ComboBox comboBox_range_state;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_amerceOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_amerceTime;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_settlementOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_settlementTime;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_recpath;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.Button button_print_printAll;
        private System.Windows.Forms.Button button_print_option;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_sumByAmerceOperator;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_readerBarcode;
        private System.Windows.Forms.Button button_print_printSettlemented;
        private System.Windows.Forms.ToolStrip toolStrip_items;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_remove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_unSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_selectAmercedItems;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_selectSettlementedItems;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_settlement;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_undoSettlement;
        private System.Windows.Forms.ToolStripButton toolStripButton_items_useCheck;
        private System.Windows.Forms.StatusStrip statusStrip_items;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_items_message1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_items_message2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_amerced;
        private System.Windows.Forms.ColumnHeader columnHeader_amerced_libraryCode;
    }
}