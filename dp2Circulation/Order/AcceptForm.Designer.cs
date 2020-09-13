namespace dp2Circulation
{
    partial class AcceptForm
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
            this.DisposeFreeControls();

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (this.Channel != null)
                this.Channel.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AcceptForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_prepare = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_sellerFilter = new System.Windows.Forms.ComboBox();
            this.button_defaultEntityFields = new System.Windows.Forms.Button();
            this.checkBox_prepare_createCallNumber = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_prepare_priceDefault = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkedListBox_prepare_dbNames = new System.Windows.Forms.CheckedListBox();
            this.checkBox_prepare_setProcessingState = new System.Windows.Forms.CheckBox();
            this.tabComboBox_prepare_batchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.checkBox_prepare_inputItemBarcode = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_prepare_type = new System.Windows.Forms.ComboBox();
            this.button_viewDatabaseDefs = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_accept = new System.Windows.Forms.TabPage();
            this.label_biblioSource = new System.Windows.Forms.Label();
            this.comboBox_accept_matchStyle = new System.Windows.Forms.ComboBox();
            this.comboBox_accept_from = new System.Windows.Forms.ComboBox();
            this.label_target = new System.Windows.Forms.Label();
            this.label_source = new System.Windows.Forms.Label();
            this.listView_accept_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_role = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_targetRecPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_lineType = new System.Windows.Forms.ImageList(this.components);
            this.button_accept_searchISBN = new System.Windows.Forms.Button();
            this.textBox_accept_queryWord = new System.Windows.Forms.TextBox();
            this.tabPage_finish = new System.Windows.Forms.TabPage();
            this.button_finish_printAcceptList = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.toolTip_info = new System.Windows.Forms.ToolTip(this.components);
            this.panel_main = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl_main.SuspendLayout();
            this.tabPage_prepare.SuspendLayout();
            this.tabPage_accept.SuspendLayout();
            this.tabPage_finish.SuspendLayout();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl_main.Controls.Add(this.tabPage_prepare);
            this.tabControl_main.Controls.Add(this.tabPage_accept);
            this.tabControl_main.Controls.Add(this.tabPage_finish);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl_main.Multiline = true;
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(603, 503);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_prepare
            // 
            this.tabPage_prepare.AutoScroll = true;
            this.tabPage_prepare.Controls.Add(this.label5);
            this.tabPage_prepare.Controls.Add(this.comboBox_sellerFilter);
            this.tabPage_prepare.Controls.Add(this.button_defaultEntityFields);
            this.tabPage_prepare.Controls.Add(this.checkBox_prepare_createCallNumber);
            this.tabPage_prepare.Controls.Add(this.label4);
            this.tabPage_prepare.Controls.Add(this.comboBox_prepare_priceDefault);
            this.tabPage_prepare.Controls.Add(this.groupBox1);
            this.tabPage_prepare.Controls.Add(this.label2);
            this.tabPage_prepare.Controls.Add(this.checkedListBox_prepare_dbNames);
            this.tabPage_prepare.Controls.Add(this.checkBox_prepare_setProcessingState);
            this.tabPage_prepare.Controls.Add(this.tabComboBox_prepare_batchNo);
            this.tabPage_prepare.Controls.Add(this.checkBox_prepare_inputItemBarcode);
            this.tabPage_prepare.Controls.Add(this.label3);
            this.tabPage_prepare.Controls.Add(this.comboBox_prepare_type);
            this.tabPage_prepare.Controls.Add(this.button_viewDatabaseDefs);
            this.tabPage_prepare.Controls.Add(this.label1);
            this.tabPage_prepare.Location = new System.Drawing.Point(4, 4);
            this.tabPage_prepare.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_prepare.Name = "tabPage_prepare";
            this.tabPage_prepare.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_prepare.Size = new System.Drawing.Size(595, 468);
            this.tabPage_prepare.TabIndex = 0;
            this.tabPage_prepare.Text = "准备";
            this.tabPage_prepare.UseVisualStyleBackColor = true;
            this.tabPage_prepare.Enter += new System.EventHandler(this.tabPage_prepare_Enter);
            this.tabPage_prepare.Leave += new System.EventHandler(this.tabPage_prepare_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 85);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(160, 21);
            this.label5.TabIndex = 14;
            this.label5.Text = "书商(渠道)(&S):";
            // 
            // comboBox_sellerFilter
            // 
            this.comboBox_sellerFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_sellerFilter.FormattingEnabled = true;
            this.comboBox_sellerFilter.Items.AddRange(new object[] {
            "<不筛选>"});
            this.comboBox_sellerFilter.Location = new System.Drawing.Point(192, 82);
            this.comboBox_sellerFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_sellerFilter.Name = "comboBox_sellerFilter";
            this.comboBox_sellerFilter.Size = new System.Drawing.Size(240, 29);
            this.comboBox_sellerFilter.TabIndex = 15;
            // 
            // button_defaultEntityFields
            // 
            this.button_defaultEntityFields.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_defaultEntityFields.Location = new System.Drawing.Point(11, 254);
            this.button_defaultEntityFields.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_defaultEntityFields.Name = "button_defaultEntityFields";
            this.button_defaultEntityFields.Size = new System.Drawing.Size(422, 52);
            this.button_defaultEntityFields.TabIndex = 13;
            this.button_defaultEntityFields.Text = "册记录默认值...";
            this.button_defaultEntityFields.UseVisualStyleBackColor = true;
            this.button_defaultEntityFields.Click += new System.EventHandler(this.button_defaultEntityFields_Click);
            // 
            // checkBox_prepare_createCallNumber
            // 
            this.checkBox_prepare_createCallNumber.AutoSize = true;
            this.checkBox_prepare_createCallNumber.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_prepare_createCallNumber.Location = new System.Drawing.Point(10, 178);
            this.checkBox_prepare_createCallNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_prepare_createCallNumber.Name = "checkBox_prepare_createCallNumber";
            this.checkBox_prepare_createCallNumber.Size = new System.Drawing.Size(298, 25);
            this.checkBox_prepare_createCallNumber.TabIndex = 6;
            this.checkBox_prepare_createCallNumber.Text = "为新验收的册创建索取号(&C)";
            this.checkBox_prepare_createCallNumber.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 212);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(159, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "册价格首选(&P):";
            // 
            // comboBox_prepare_priceDefault
            // 
            this.comboBox_prepare_priceDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_prepare_priceDefault.FormattingEnabled = true;
            this.comboBox_prepare_priceDefault.Items.AddRange(new object[] {
            "书目价",
            "订购价",
            "验收价",
            "空白"});
            this.comboBox_prepare_priceDefault.Location = new System.Drawing.Point(192, 209);
            this.comboBox_prepare_priceDefault.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_prepare_priceDefault.Name = "comboBox_prepare_priceDefault";
            this.comboBox_prepare_priceDefault.Size = new System.Drawing.Size(240, 29);
            this.comboBox_prepare_priceDefault.TabIndex = 8;
            this.comboBox_prepare_priceDefault.Text = "验收价";
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(440, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(2, 238);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 322);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(222, 21);
            this.label2.TabIndex = 11;
            this.label2.Text = "参与检索的书目库(&N):";
            // 
            // checkedListBox_prepare_dbNames
            // 
            this.checkedListBox_prepare_dbNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBox_prepare_dbNames.CheckOnClick = true;
            this.checkedListBox_prepare_dbNames.FormattingEnabled = true;
            this.checkedListBox_prepare_dbNames.HorizontalScrollbar = true;
            this.checkedListBox_prepare_dbNames.IntegralHeight = false;
            this.checkedListBox_prepare_dbNames.Location = new System.Drawing.Point(9, 349);
            this.checkedListBox_prepare_dbNames.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkedListBox_prepare_dbNames.Name = "checkedListBox_prepare_dbNames";
            this.checkedListBox_prepare_dbNames.Size = new System.Drawing.Size(424, 173);
            this.checkedListBox_prepare_dbNames.TabIndex = 12;
            this.checkedListBox_prepare_dbNames.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox_prepare_dbNames_ItemCheck);
            // 
            // checkBox_prepare_setProcessingState
            // 
            this.checkBox_prepare_setProcessingState.AutoSize = true;
            this.checkBox_prepare_setProcessingState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_prepare_setProcessingState.Location = new System.Drawing.Point(10, 150);
            this.checkBox_prepare_setProcessingState.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_prepare_setProcessingState.Name = "checkBox_prepare_setProcessingState";
            this.checkBox_prepare_setProcessingState.Size = new System.Drawing.Size(382, 25);
            this.checkBox_prepare_setProcessingState.TabIndex = 5;
            this.checkBox_prepare_setProcessingState.Text = "为新验收的册设置“加工中”状态(&U)";
            this.checkBox_prepare_setProcessingState.UseVisualStyleBackColor = true;
            this.checkBox_prepare_setProcessingState.CheckedChanged += new System.EventHandler(this.checkBox_prepare_setProcessingState_CheckedChanged);
            // 
            // tabComboBox_prepare_batchNo
            // 
            this.tabComboBox_prepare_batchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_prepare_batchNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tabComboBox_prepare_batchNo.FormattingEnabled = true;
            this.tabComboBox_prepare_batchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.tabComboBox_prepare_batchNo.Location = new System.Drawing.Point(192, 9);
            this.tabComboBox_prepare_batchNo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabComboBox_prepare_batchNo.Name = "tabComboBox_prepare_batchNo";
            this.tabComboBox_prepare_batchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.tabComboBox_prepare_batchNo.Size = new System.Drawing.Size(240, 32);
            this.tabComboBox_prepare_batchNo.TabIndex = 1;
            this.tabComboBox_prepare_batchNo.DropDown += new System.EventHandler(this.tabComboBox_prepare_batchNo_DropDown);
            this.tabComboBox_prepare_batchNo.TextChanged += new System.EventHandler(this.tabComboBox_prepare_batchNo_TextChanged);
            this.tabComboBox_prepare_batchNo.Leave += new System.EventHandler(this.tabComboBox_prepare_batchNo_Leave);
            // 
            // checkBox_prepare_inputItemBarcode
            // 
            this.checkBox_prepare_inputItemBarcode.AutoSize = true;
            this.checkBox_prepare_inputItemBarcode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_prepare_inputItemBarcode.Location = new System.Drawing.Point(10, 122);
            this.checkBox_prepare_inputItemBarcode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox_prepare_inputItemBarcode.Name = "checkBox_prepare_inputItemBarcode";
            this.checkBox_prepare_inputItemBarcode.Size = new System.Drawing.Size(298, 25);
            this.checkBox_prepare_inputItemBarcode.TabIndex = 4;
            this.checkBox_prepare_inputItemBarcode.Text = "验收时立即输入册条码号(&I)";
            this.checkBox_prepare_inputItemBarcode.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 50);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 21);
            this.label3.TabIndex = 2;
            this.label3.Text = "出版物类型(&T):";
            // 
            // comboBox_prepare_type
            // 
            this.comboBox_prepare_type.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_prepare_type.FormattingEnabled = true;
            this.comboBox_prepare_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_prepare_type.Location = new System.Drawing.Point(192, 47);
            this.comboBox_prepare_type.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_prepare_type.Name = "comboBox_prepare_type";
            this.comboBox_prepare_type.Size = new System.Drawing.Size(240, 29);
            this.comboBox_prepare_type.TabIndex = 3;
            this.comboBox_prepare_type.Text = "图书";
            this.comboBox_prepare_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_prepare_type_SelectedIndexChanged);
            this.comboBox_prepare_type.TextChanged += new System.EventHandler(this.comboBox_prepare_type_TextChanged);
            // 
            // button_viewDatabaseDefs
            // 
            this.button_viewDatabaseDefs.AutoSize = true;
            this.button_viewDatabaseDefs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_viewDatabaseDefs.Location = new System.Drawing.Point(9, 559);
            this.button_viewDatabaseDefs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_viewDatabaseDefs.Name = "button_viewDatabaseDefs";
            this.button_viewDatabaseDefs.Size = new System.Drawing.Size(424, 52);
            this.button_viewDatabaseDefs.TabIndex = 9;
            this.button_viewDatabaseDefs.Text = "观察数据库定义...";
            this.button_viewDatabaseDefs.UseVisualStyleBackColor = true;
            this.button_viewDatabaseDefs.Click += new System.EventHandler(this.button_viewDatabaseDefs_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "验收批次号(&B):";
            // 
            // tabPage_accept
            // 
            this.tabPage_accept.Controls.Add(this.label_biblioSource);
            this.tabPage_accept.Controls.Add(this.comboBox_accept_matchStyle);
            this.tabPage_accept.Controls.Add(this.comboBox_accept_from);
            this.tabPage_accept.Controls.Add(this.label_target);
            this.tabPage_accept.Controls.Add(this.label_source);
            this.tabPage_accept.Controls.Add(this.listView_accept_records);
            this.tabPage_accept.Controls.Add(this.button_accept_searchISBN);
            this.tabPage_accept.Controls.Add(this.textBox_accept_queryWord);
            this.tabPage_accept.Location = new System.Drawing.Point(4, 4);
            this.tabPage_accept.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_accept.Name = "tabPage_accept";
            this.tabPage_accept.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_accept.Size = new System.Drawing.Size(595, 468);
            this.tabPage_accept.TabIndex = 1;
            this.tabPage_accept.Text = "验收";
            this.tabPage_accept.UseVisualStyleBackColor = true;
            // 
            // label_biblioSource
            // 
            this.label_biblioSource.AllowDrop = true;
            this.label_biblioSource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label_biblioSource.Image = ((System.Drawing.Image)(resources.GetObject("label_biblioSource.Image")));
            this.label_biblioSource.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_biblioSource.Location = new System.Drawing.Point(4, 94);
            this.label_biblioSource.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_biblioSource.Name = "label_biblioSource";
            this.label_biblioSource.Size = new System.Drawing.Size(103, 24);
            this.label_biblioSource.TabIndex = 13;
            this.label_biblioSource.Text = "外源(空)";
            this.label_biblioSource.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_biblioSource.DragDrop += new System.Windows.Forms.DragEventHandler(this.label_biblioSource_DragDrop);
            this.label_biblioSource.DragEnter += new System.Windows.Forms.DragEventHandler(this.label_biblioSource_DragEnter);
            this.label_biblioSource.DoubleClick += new System.EventHandler(this.label_biblioSource_DoubleClick);
            this.label_biblioSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label_biblioSource_MouseClick);
            this.label_biblioSource.MouseHover += new System.EventHandler(this.label_biblioSource_MouseHover);
            // 
            // comboBox_accept_matchStyle
            // 
            this.comboBox_accept_matchStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_accept_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_accept_matchStyle.FormattingEnabled = true;
            this.comboBox_accept_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_accept_matchStyle.Location = new System.Drawing.Point(137, 7);
            this.comboBox_accept_matchStyle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_accept_matchStyle.Name = "comboBox_accept_matchStyle";
            this.comboBox_accept_matchStyle.Size = new System.Drawing.Size(125, 29);
            this.comboBox_accept_matchStyle.TabIndex = 12;
            this.comboBox_accept_matchStyle.TextChanged += new System.EventHandler(this.comboBox_accept_matchStyle_TextChanged);
            // 
            // comboBox_accept_from
            // 
            this.comboBox_accept_from.DropDownHeight = 300;
            this.comboBox_accept_from.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_accept_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_accept_from.FormattingEnabled = true;
            this.comboBox_accept_from.IntegralHeight = false;
            this.comboBox_accept_from.Location = new System.Drawing.Point(5, 7);
            this.comboBox_accept_from.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_accept_from.Name = "comboBox_accept_from";
            this.comboBox_accept_from.Size = new System.Drawing.Size(125, 29);
            this.comboBox_accept_from.TabIndex = 11;
            // 
            // label_target
            // 
            this.label_target.AllowDrop = true;
            this.label_target.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.label_target.Image = ((System.Drawing.Image)(resources.GetObject("label_target.Image")));
            this.label_target.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_target.Location = new System.Drawing.Point(4, 68);
            this.label_target.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_target.Name = "label_target";
            this.label_target.Size = new System.Drawing.Size(103, 24);
            this.label_target.TabIndex = 10;
            this.label_target.Text = "目标(空)";
            this.label_target.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_target.DragDrop += new System.Windows.Forms.DragEventHandler(this.label_target_DragDrop);
            this.label_target.DragEnter += new System.Windows.Forms.DragEventHandler(this.label_target_DragEnter);
            this.label_target.DoubleClick += new System.EventHandler(this.label_target_DoubleClick);
            this.label_target.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label_target_MouseClick);
            this.label_target.MouseHover += new System.EventHandler(this.label_target_MouseHover);
            // 
            // label_source
            // 
            this.label_source.AllowDrop = true;
            this.label_source.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.label_source.Image = ((System.Drawing.Image)(resources.GetObject("label_source.Image")));
            this.label_source.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_source.Location = new System.Drawing.Point(4, 42);
            this.label_source.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_source.Name = "label_source";
            this.label_source.Size = new System.Drawing.Size(103, 24);
            this.label_source.TabIndex = 9;
            this.label_source.Text = "源(空)";
            this.label_source.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label_source.DragDrop += new System.Windows.Forms.DragEventHandler(this.label_source_DragDrop);
            this.label_source.DragEnter += new System.Windows.Forms.DragEventHandler(this.label_source_DragEnter);
            this.label_source.DoubleClick += new System.EventHandler(this.label_source_DoubleClick);
            this.label_source.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label_source_MouseClick);
            this.label_source.MouseHover += new System.EventHandler(this.label_source_MouseHover);
            // 
            // listView_accept_records
            // 
            this.listView_accept_records.AllowDrop = true;
            this.listView_accept_records.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_accept_records.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_accept_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_role,
            this.columnHeader_targetRecPath,
            this.columnHeader_1});
            this.listView_accept_records.FullRowSelect = true;
            this.listView_accept_records.HideSelection = false;
            this.listView_accept_records.LargeImageList = this.imageList_lineType;
            this.listView_accept_records.Location = new System.Drawing.Point(106, 42);
            this.listView_accept_records.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView_accept_records.Name = "listView_accept_records";
            this.listView_accept_records.Size = new System.Drawing.Size(476, 362);
            this.listView_accept_records.SmallImageList = this.imageList_lineType;
            this.listView_accept_records.TabIndex = 8;
            this.listView_accept_records.UseCompatibleStateImageBehavior = false;
            this.listView_accept_records.View = System.Windows.Forms.View.Details;
            this.listView_accept_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_accept_records_ColumnClick);
            this.listView_accept_records.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView_accept_records_ItemDrag);
            this.listView_accept_records.SelectedIndexChanged += new System.EventHandler(this.listView_accept_records_SelectedIndexChanged);
            this.listView_accept_records.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView_accept_records_DragDrop);
            this.listView_accept_records.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_accept_records_DragEnter);
            this.listView_accept_records.DoubleClick += new System.EventHandler(this.listView_accept_records_DoubleClick);
            this.listView_accept_records.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView_accept_records_MouseDown);
            this.listView_accept_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_accept_records_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 120;
            // 
            // columnHeader_role
            // 
            this.columnHeader_role.Text = "角色";
            this.columnHeader_role.Width = 100;
            // 
            // columnHeader_targetRecPath
            // 
            this.columnHeader_targetRecPath.Text = "目标记录路径";
            this.columnHeader_targetRecPath.Width = 120;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 300;
            // 
            // imageList_lineType
            // 
            this.imageList_lineType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_lineType.ImageStream")));
            this.imageList_lineType.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_lineType.Images.SetKeyName(0, "source.bmp");
            this.imageList_lineType.Images.SetKeyName(1, "target.bmp");
            this.imageList_lineType.Images.SetKeyName(2, "source_and_target.bmp");
            this.imageList_lineType.Images.SetKeyName(3, "biblioSource.bmp");
            this.imageList_lineType.Images.SetKeyName(4, "WarningHS.png");
            // 
            // button_accept_searchISBN
            // 
            this.button_accept_searchISBN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_accept_searchISBN.AutoSize = true;
            this.button_accept_searchISBN.Location = new System.Drawing.Point(434, 3);
            this.button_accept_searchISBN.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_accept_searchISBN.Name = "button_accept_searchISBN";
            this.button_accept_searchISBN.Size = new System.Drawing.Size(149, 49);
            this.button_accept_searchISBN.TabIndex = 4;
            this.button_accept_searchISBN.Text = "检索(&S)";
            this.button_accept_searchISBN.UseVisualStyleBackColor = true;
            this.button_accept_searchISBN.Click += new System.EventHandler(this.button_accept_searchISBN_Click);
            // 
            // textBox_accept_queryWord
            // 
            this.textBox_accept_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_accept_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_accept_queryWord.Location = new System.Drawing.Point(268, 7);
            this.textBox_accept_queryWord.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_accept_queryWord.Name = "textBox_accept_queryWord";
            this.textBox_accept_queryWord.Size = new System.Drawing.Size(158, 31);
            this.textBox_accept_queryWord.TabIndex = 3;
            this.textBox_accept_queryWord.Enter += new System.EventHandler(this.textBox_accept_isbn_Enter);
            this.textBox_accept_queryWord.Leave += new System.EventHandler(this.textBox_accept_isbn_Leave);
            // 
            // tabPage_finish
            // 
            this.tabPage_finish.Controls.Add(this.button_finish_printAcceptList);
            this.tabPage_finish.Location = new System.Drawing.Point(4, 4);
            this.tabPage_finish.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_finish.Name = "tabPage_finish";
            this.tabPage_finish.Size = new System.Drawing.Size(595, 468);
            this.tabPage_finish.TabIndex = 2;
            this.tabPage_finish.Text = "结尾";
            this.tabPage_finish.UseVisualStyleBackColor = true;
            // 
            // button_finish_printAcceptList
            // 
            this.button_finish_printAcceptList.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button_finish_printAcceptList.AutoSize = true;
            this.button_finish_printAcceptList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_finish_printAcceptList.Location = new System.Drawing.Point(116, 154);
            this.button_finish_printAcceptList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_finish_printAcceptList.Name = "button_finish_printAcceptList";
            this.button_finish_printAcceptList.Size = new System.Drawing.Size(334, 52);
            this.button_finish_printAcceptList.TabIndex = 0;
            this.button_finish_printAcceptList.Text = "打印验收清单(&P)...";
            this.button_finish_printAcceptList.UseVisualStyleBackColor = true;
            this.button_finish_printAcceptList.Click += new System.EventHandler(this.button_finish_printAcceptList_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.AutoSize = true;
            this.button_next.Enabled = false;
            this.button_next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_next.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(370, 455);
            this.button_next.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(230, 47);
            this.button_next.TabIndex = 1;
            this.button_next.Text = "下一环节(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // panel_main
            // 
            this.panel_main.Controls.Add(this.button_next);
            this.panel_main.Controls.Add(this.tabControl_main);
            this.panel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_main.Location = new System.Drawing.Point(0, 0);
            this.panel_main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(603, 503);
            this.panel_main.TabIndex = 2;
            // 
            // AcceptForm
            // 
            this.AcceptButton = this.button_next;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 503);
            this.Controls.Add(this.panel_main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "AcceptForm";
            this.ShowInTaskbar = false;
            this.Text = "验收";
            this.Activated += new System.EventHandler(this.AcceptForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AcceptForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AcceptForm_FormClosed);
            this.Load += new System.EventHandler(this.AcceptForm_Load);
            this.SizeChanged += new System.EventHandler(this.AcceptForm_SizeChanged);
            this.Enter += new System.EventHandler(this.AcceptForm_Enter);
            this.Leave += new System.EventHandler(this.AcceptForm_Leave);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_prepare.ResumeLayout(false);
            this.tabPage_prepare.PerformLayout();
            this.tabPage_accept.ResumeLayout(false);
            this.tabPage_accept.PerformLayout();
            this.tabPage_finish.ResumeLayout(false);
            this.tabPage_finish.PerformLayout();
            this.panel_main.ResumeLayout(false);
            this.panel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_prepare;
        private System.Windows.Forms.TabPage tabPage_accept;
        private System.Windows.Forms.TabPage tabPage_finish;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_accept_queryWord;
        private System.Windows.Forms.Button button_accept_searchISBN;
        private System.Windows.Forms.Button button_finish_printAcceptList;
        private System.Windows.Forms.Label label_target;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.ToolTip toolTip_info;
        private System.Windows.Forms.Button button_viewDatabaseDefs;
        private System.Windows.Forms.ImageList imageList_lineType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_prepare_type;
        private System.Windows.Forms.CheckBox checkBox_prepare_inputItemBarcode;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_prepare_batchNo;
        private DigitalPlatform.GUI.ListViewNF listView_accept_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_role;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.CheckBox checkBox_prepare_setProcessingState;
        private System.Windows.Forms.CheckedListBox checkedListBox_prepare_dbNames;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_accept_matchStyle;
        private System.Windows.Forms.ComboBox comboBox_accept_from;
        private System.Windows.Forms.Label label_biblioSource;
        private System.Windows.Forms.ColumnHeader columnHeader_targetRecPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_prepare_priceDefault;
        private System.Windows.Forms.CheckBox checkBox_prepare_createCallNumber;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.Button button_defaultEntityFields;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_sellerFilter;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}