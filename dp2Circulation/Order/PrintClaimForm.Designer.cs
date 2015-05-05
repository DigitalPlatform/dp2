namespace dp2Circulation
{
    partial class PrintClaimForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintClaimForm));
            this.tabControl_items = new System.Windows.Forms.TabControl();
            this.tabPage_originItems = new System.Windows.Forms.TabPage();
            this.listView_origin = new DigitalPlatform.GUI.ListViewNF();
            this.tabPage_mergedItems = new System.Windows.Forms.TabPage();
            this.listView_merged = new DigitalPlatform.GUI.ListViewNF();
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.checkBox_source_guess = new System.Windows.Forms.CheckBox();
            this.radioButton_inputStyle_orderRecPathFile = new System.Windows.Forms.RadioButton();
            this.button_findInputOrderRecPathFilename = new System.Windows.Forms.Button();
            this.textBox_inputOrderRecPathFilename = new System.Windows.Forms.TextBox();
            this.radioButton_inputStyle_orderDatabase = new System.Windows.Forms.RadioButton();
            this.comboBox_inputOrderDbName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButton_inputStyle_biblioRecPathFile = new System.Windows.Forms.RadioButton();
            this.button_findInputBiblioRecPathFilename = new System.Windows.Forms.Button();
            this.textBox_inputBiblioRecPathFilename = new System.Windows.Forms.TextBox();
            this.radioButton_inputStyle_biblioDatabase = new System.Windows.Forms.RadioButton();
            this.comboBox_inputBiblioDbName = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_source_type = new System.Windows.Forms.ComboBox();
            this.tabPage_timeRange = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_timeRange_none = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_timeRange_afterOrder = new System.Windows.Forms.ComboBox();
            this.checkBox_timeRange_useOrderTime = new System.Windows.Forms.CheckBox();
            this.checkBox_timeRange_usePublishTime = new System.Windows.Forms.CheckBox();
            this.groupBox_timeRange_quickSet = new System.Windows.Forms.GroupBox();
            this.comboBox_timeRange_quickSet = new System.Windows.Forms.ComboBox();
            this.button_timeRange_clearTimeRange = new System.Windows.Forms.Button();
            this.button_timeRange_inputTimeRange = new System.Windows.Forms.Button();
            this.textBox_timeRange = new System.Windows.Forms.TextBox();
            this.label_timerange = new System.Windows.Forms.Label();
            this.tabPage_run = new System.Windows.Forms.TabPage();
            this.webBrowser_errorInfo = new System.Windows.Forms.WebBrowser();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_printOption = new System.Windows.Forms.Button();
            this.button_print = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_up = new System.Windows.Forms.TableLayoutPanel();
            this.checkBox_debug = new System.Windows.Forms.CheckBox();
            this.tabControl_items.SuspendLayout();
            this.tabPage_originItems.SuspendLayout();
            this.tabPage_mergedItems.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_source.SuspendLayout();
            this.tabPage_timeRange.SuspendLayout();
            this.groupBox_timeRange_quickSet.SuspendLayout();
            this.tabPage_run.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_up.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_items
            // 
            this.tabControl_items.Controls.Add(this.tabPage_originItems);
            this.tabControl_items.Controls.Add(this.tabPage_mergedItems);
            this.tabControl_items.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_items.Location = new System.Drawing.Point(0, 0);
            this.tabControl_items.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_items.Name = "tabControl_items";
            this.tabControl_items.SelectedIndex = 0;
            this.tabControl_items.Size = new System.Drawing.Size(496, 123);
            this.tabControl_items.TabIndex = 0;
            // 
            // tabPage_originItems
            // 
            this.tabPage_originItems.Controls.Add(this.listView_origin);
            this.tabPage_originItems.Location = new System.Drawing.Point(4, 22);
            this.tabPage_originItems.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_originItems.Name = "tabPage_originItems";
            this.tabPage_originItems.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_originItems.Size = new System.Drawing.Size(488, 97);
            this.tabPage_originItems.TabIndex = 0;
            this.tabPage_originItems.Text = "原始数据";
            this.tabPage_originItems.UseVisualStyleBackColor = true;
            // 
            // listView_origin
            // 
            this.listView_origin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_origin.FullRowSelect = true;
            this.listView_origin.HideSelection = false;
            this.listView_origin.Location = new System.Drawing.Point(2, 2);
            this.listView_origin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_origin.Name = "listView_origin";
            this.listView_origin.Size = new System.Drawing.Size(484, 93);
            this.listView_origin.TabIndex = 4;
            this.listView_origin.UseCompatibleStateImageBehavior = false;
            this.listView_origin.View = System.Windows.Forms.View.Details;
            // 
            // tabPage_mergedItems
            // 
            this.tabPage_mergedItems.Controls.Add(this.listView_merged);
            this.tabPage_mergedItems.Location = new System.Drawing.Point(4, 22);
            this.tabPage_mergedItems.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_mergedItems.Name = "tabPage_mergedItems";
            this.tabPage_mergedItems.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_mergedItems.Size = new System.Drawing.Size(488, 96);
            this.tabPage_mergedItems.TabIndex = 1;
            this.tabPage_mergedItems.Text = "合并后";
            this.tabPage_mergedItems.UseVisualStyleBackColor = true;
            // 
            // listView_merged
            // 
            this.listView_merged.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_merged.FullRowSelect = true;
            this.listView_merged.HideSelection = false;
            this.listView_merged.Location = new System.Drawing.Point(2, 2);
            this.listView_merged.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_merged.Name = "listView_merged";
            this.listView_merged.Size = new System.Drawing.Size(484, 92);
            this.listView_merged.TabIndex = 5;
            this.listView_merged.UseCompatibleStateImageBehavior = false;
            this.listView_merged.View = System.Windows.Forms.View.Details;
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(411, 186);
            this.button_next.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(83, 22);
            this.button_next.TabIndex = 1;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_source);
            this.tabControl_main.Controls.Add(this.tabPage_timeRange);
            this.tabControl_main.Controls.Add(this.tabPage_run);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(2, 2);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(492, 180);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_source
            // 
            this.tabPage_source.AutoScroll = true;
            this.tabPage_source.Controls.Add(this.checkBox_debug);
            this.tabPage_source.Controls.Add(this.checkBox_source_guess);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_orderRecPathFile);
            this.tabPage_source.Controls.Add(this.button_findInputOrderRecPathFilename);
            this.tabPage_source.Controls.Add(this.textBox_inputOrderRecPathFilename);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_orderDatabase);
            this.tabPage_source.Controls.Add(this.comboBox_inputOrderDbName);
            this.tabPage_source.Controls.Add(this.label1);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_biblioRecPathFile);
            this.tabPage_source.Controls.Add(this.button_findInputBiblioRecPathFilename);
            this.tabPage_source.Controls.Add(this.textBox_inputBiblioRecPathFilename);
            this.tabPage_source.Controls.Add(this.radioButton_inputStyle_biblioDatabase);
            this.tabPage_source.Controls.Add(this.comboBox_inputBiblioDbName);
            this.tabPage_source.Controls.Add(this.label6);
            this.tabPage_source.Controls.Add(this.label3);
            this.tabPage_source.Controls.Add(this.comboBox_source_type);
            this.tabPage_source.Location = new System.Drawing.Point(4, 22);
            this.tabPage_source.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_source.Size = new System.Drawing.Size(484, 154);
            this.tabPage_source.TabIndex = 0;
            this.tabPage_source.Text = "数据来源";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // checkBox_source_guess
            // 
            this.checkBox_source_guess.AutoSize = true;
            this.checkBox_source_guess.Location = new System.Drawing.Point(257, 7);
            this.checkBox_source_guess.Name = "checkBox_source_guess";
            this.checkBox_source_guess.Size = new System.Drawing.Size(102, 16);
            this.checkBox_source_guess.TabIndex = 2;
            this.checkBox_source_guess.Text = "猜测未到期(&G)";
            this.checkBox_source_guess.UseVisualStyleBackColor = true;
            // 
            // radioButton_inputStyle_orderRecPathFile
            // 
            this.radioButton_inputStyle_orderRecPathFile.AutoSize = true;
            this.radioButton_inputStyle_orderRecPathFile.Location = new System.Drawing.Point(8, 162);
            this.radioButton_inputStyle_orderRecPathFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_inputStyle_orderRecPathFile.Name = "radioButton_inputStyle_orderRecPathFile";
            this.radioButton_inputStyle_orderRecPathFile.Size = new System.Drawing.Size(161, 16);
            this.radioButton_inputStyle_orderRecPathFile.TabIndex = 9;
            this.radioButton_inputStyle_orderRecPathFile.Text = "[订购库]记录路径文件(&O)";
            this.radioButton_inputStyle_orderRecPathFile.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_orderRecPathFile.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_orderRecPathFile_CheckedChanged);
            // 
            // button_findInputOrderRecPathFilename
            // 
            this.button_findInputOrderRecPathFilename.Enabled = false;
            this.button_findInputOrderRecPathFilename.Location = new System.Drawing.Point(268, 182);
            this.button_findInputOrderRecPathFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_findInputOrderRecPathFilename.Name = "button_findInputOrderRecPathFilename";
            this.button_findInputOrderRecPathFilename.Size = new System.Drawing.Size(32, 22);
            this.button_findInputOrderRecPathFilename.TabIndex = 11;
            this.button_findInputOrderRecPathFilename.Text = "...";
            this.button_findInputOrderRecPathFilename.UseVisualStyleBackColor = true;
            this.button_findInputOrderRecPathFilename.Click += new System.EventHandler(this.button_findInputOrderRecPathFilename_Click);
            // 
            // textBox_inputOrderRecPathFilename
            // 
            this.textBox_inputOrderRecPathFilename.Enabled = false;
            this.textBox_inputOrderRecPathFilename.Location = new System.Drawing.Point(34, 182);
            this.textBox_inputOrderRecPathFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_inputOrderRecPathFilename.Name = "textBox_inputOrderRecPathFilename";
            this.textBox_inputOrderRecPathFilename.Size = new System.Drawing.Size(230, 21);
            this.textBox_inputOrderRecPathFilename.TabIndex = 10;
            // 
            // radioButton_inputStyle_orderDatabase
            // 
            this.radioButton_inputStyle_orderDatabase.AutoSize = true;
            this.radioButton_inputStyle_orderDatabase.Location = new System.Drawing.Point(8, 217);
            this.radioButton_inputStyle_orderDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_inputStyle_orderDatabase.Name = "radioButton_inputStyle_orderDatabase";
            this.radioButton_inputStyle_orderDatabase.Size = new System.Drawing.Size(101, 16);
            this.radioButton_inputStyle_orderDatabase.TabIndex = 12;
            this.radioButton_inputStyle_orderDatabase.Text = "整个订购库(&R)";
            this.radioButton_inputStyle_orderDatabase.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_orderDatabase.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_orderDatabase_CheckedChanged);
            // 
            // comboBox_inputOrderDbName
            // 
            this.comboBox_inputOrderDbName.Enabled = false;
            this.comboBox_inputOrderDbName.FormattingEnabled = true;
            this.comboBox_inputOrderDbName.Location = new System.Drawing.Point(123, 237);
            this.comboBox_inputOrderDbName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_inputOrderDbName.Name = "comboBox_inputOrderDbName";
            this.comboBox_inputOrderDbName.Size = new System.Drawing.Size(140, 20);
            this.comboBox_inputOrderDbName.TabIndex = 14;
            this.comboBox_inputOrderDbName.DropDown += new System.EventHandler(this.comboBox_inputOrderDbName_DropDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 239);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "订购库名(&D):";
            // 
            // radioButton_inputStyle_biblioRecPathFile
            // 
            this.radioButton_inputStyle_biblioRecPathFile.AutoSize = true;
            this.radioButton_inputStyle_biblioRecPathFile.Location = new System.Drawing.Point(8, 39);
            this.radioButton_inputStyle_biblioRecPathFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_inputStyle_biblioRecPathFile.Name = "radioButton_inputStyle_biblioRecPathFile";
            this.radioButton_inputStyle_biblioRecPathFile.Size = new System.Drawing.Size(161, 16);
            this.radioButton_inputStyle_biblioRecPathFile.TabIndex = 3;
            this.radioButton_inputStyle_biblioRecPathFile.Text = "[书目库]记录路径文件(&P)";
            this.radioButton_inputStyle_biblioRecPathFile.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_biblioRecPathFile.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_biblioRecPathFile_CheckedChanged);
            // 
            // button_findInputBiblioRecPathFilename
            // 
            this.button_findInputBiblioRecPathFilename.Enabled = false;
            this.button_findInputBiblioRecPathFilename.Location = new System.Drawing.Point(268, 59);
            this.button_findInputBiblioRecPathFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_findInputBiblioRecPathFilename.Name = "button_findInputBiblioRecPathFilename";
            this.button_findInputBiblioRecPathFilename.Size = new System.Drawing.Size(32, 22);
            this.button_findInputBiblioRecPathFilename.TabIndex = 5;
            this.button_findInputBiblioRecPathFilename.Text = "...";
            this.button_findInputBiblioRecPathFilename.UseVisualStyleBackColor = true;
            this.button_findInputBiblioRecPathFilename.Click += new System.EventHandler(this.button_findInputBiblioRecPathFilename_Click);
            // 
            // textBox_inputBiblioRecPathFilename
            // 
            this.textBox_inputBiblioRecPathFilename.Enabled = false;
            this.textBox_inputBiblioRecPathFilename.Location = new System.Drawing.Point(34, 59);
            this.textBox_inputBiblioRecPathFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_inputBiblioRecPathFilename.Name = "textBox_inputBiblioRecPathFilename";
            this.textBox_inputBiblioRecPathFilename.Size = new System.Drawing.Size(230, 21);
            this.textBox_inputBiblioRecPathFilename.TabIndex = 4;
            // 
            // radioButton_inputStyle_biblioDatabase
            // 
            this.radioButton_inputStyle_biblioDatabase.AutoSize = true;
            this.radioButton_inputStyle_biblioDatabase.Checked = true;
            this.radioButton_inputStyle_biblioDatabase.Location = new System.Drawing.Point(8, 94);
            this.radioButton_inputStyle_biblioDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radioButton_inputStyle_biblioDatabase.Name = "radioButton_inputStyle_biblioDatabase";
            this.radioButton_inputStyle_biblioDatabase.Size = new System.Drawing.Size(101, 16);
            this.radioButton_inputStyle_biblioDatabase.TabIndex = 6;
            this.radioButton_inputStyle_biblioDatabase.TabStop = true;
            this.radioButton_inputStyle_biblioDatabase.Text = "整个书目库(&D)";
            this.radioButton_inputStyle_biblioDatabase.UseVisualStyleBackColor = true;
            this.radioButton_inputStyle_biblioDatabase.CheckedChanged += new System.EventHandler(this.radioButton_inputStyle_biblioDatabase_CheckedChanged);
            // 
            // comboBox_inputBiblioDbName
            // 
            this.comboBox_inputBiblioDbName.Enabled = false;
            this.comboBox_inputBiblioDbName.FormattingEnabled = true;
            this.comboBox_inputBiblioDbName.Location = new System.Drawing.Point(123, 114);
            this.comboBox_inputBiblioDbName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_inputBiblioDbName.Name = "comboBox_inputBiblioDbName";
            this.comboBox_inputBiblioDbName.Size = new System.Drawing.Size(140, 20);
            this.comboBox_inputBiblioDbName.TabIndex = 8;
            this.comboBox_inputBiblioDbName.DropDown += new System.EventHandler(this.comboBox_inputBiblioDbName_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(32, 116);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "书目库名(&B):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "出版物类型(&T):";
            // 
            // comboBox_source_type
            // 
            this.comboBox_source_type.FormattingEnabled = true;
            this.comboBox_source_type.Items.AddRange(new object[] {
            "图书",
            "连续出版物"});
            this.comboBox_source_type.Location = new System.Drawing.Point(123, 5);
            this.comboBox_source_type.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_source_type.Name = "comboBox_source_type";
            this.comboBox_source_type.Size = new System.Drawing.Size(116, 20);
            this.comboBox_source_type.TabIndex = 1;
            this.comboBox_source_type.Text = "图书";
            this.comboBox_source_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_source_type_SelectedIndexChanged);
            this.comboBox_source_type.TextChanged += new System.EventHandler(this.comboBox_source_type_TextChanged);
            // 
            // tabPage_timeRange
            // 
            this.tabPage_timeRange.AutoScroll = true;
            this.tabPage_timeRange.Controls.Add(this.label2);
            this.tabPage_timeRange.Controls.Add(this.checkBox_timeRange_none);
            this.tabPage_timeRange.Controls.Add(this.label4);
            this.tabPage_timeRange.Controls.Add(this.comboBox_timeRange_afterOrder);
            this.tabPage_timeRange.Controls.Add(this.checkBox_timeRange_useOrderTime);
            this.tabPage_timeRange.Controls.Add(this.checkBox_timeRange_usePublishTime);
            this.tabPage_timeRange.Controls.Add(this.groupBox_timeRange_quickSet);
            this.tabPage_timeRange.Controls.Add(this.button_timeRange_clearTimeRange);
            this.tabPage_timeRange.Controls.Add(this.button_timeRange_inputTimeRange);
            this.tabPage_timeRange.Controls.Add(this.textBox_timeRange);
            this.tabPage_timeRange.Controls.Add(this.label_timerange);
            this.tabPage_timeRange.Location = new System.Drawing.Point(4, 22);
            this.tabPage_timeRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_timeRange.Name = "tabPage_timeRange";
            this.tabPage_timeRange.Size = new System.Drawing.Size(483, 156);
            this.tabPage_timeRange.TabIndex = 4;
            this.tabPage_timeRange.Text = "时间范围";
            this.tabPage_timeRange.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(460, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "作为估计的出版时间";
            // 
            // checkBox_timeRange_none
            // 
            this.checkBox_timeRange_none.AutoSize = true;
            this.checkBox_timeRange_none.Location = new System.Drawing.Point(14, 60);
            this.checkBox_timeRange_none.Name = "checkBox_timeRange_none";
            this.checkBox_timeRange_none.Size = new System.Drawing.Size(78, 16);
            this.checkBox_timeRange_none.TabIndex = 9;
            this.checkBox_timeRange_none.Text = "不过滤(&N)";
            this.checkBox_timeRange_none.UseVisualStyleBackColor = true;
            this.checkBox_timeRange_none.CheckedChanged += new System.EventHandler(this.checkBox_timeRange_none_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(226, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "订购日期延迟(&A):";
            // 
            // comboBox_timeRange_afterOrder
            // 
            this.comboBox_timeRange_afterOrder.FormattingEnabled = true;
            this.comboBox_timeRange_afterOrder.Items.AddRange(new object[] {
            "立即",
            "一周后",
            "半年后",
            "一年后",
            "两年后",
            "三年后",
            "四年后"});
            this.comboBox_timeRange_afterOrder.Location = new System.Drawing.Point(333, 36);
            this.comboBox_timeRange_afterOrder.Name = "comboBox_timeRange_afterOrder";
            this.comboBox_timeRange_afterOrder.Size = new System.Drawing.Size(121, 20);
            this.comboBox_timeRange_afterOrder.TabIndex = 3;
            // 
            // checkBox_timeRange_useOrderTime
            // 
            this.checkBox_timeRange_useOrderTime.AutoSize = true;
            this.checkBox_timeRange_useOrderTime.Location = new System.Drawing.Point(14, 38);
            this.checkBox_timeRange_useOrderTime.Name = "checkBox_timeRange_useOrderTime";
            this.checkBox_timeRange_useOrderTime.Size = new System.Drawing.Size(186, 16);
            this.checkBox_timeRange_useOrderTime.TabIndex = 1;
            this.checkBox_timeRange_useOrderTime.Text = "要求订购时间落入指定范围(&O)";
            this.checkBox_timeRange_useOrderTime.UseVisualStyleBackColor = true;
            this.checkBox_timeRange_useOrderTime.CheckedChanged += new System.EventHandler(this.checkBox_timeRange_useOrderTime_CheckedChanged);
            // 
            // checkBox_timeRange_usePublishTime
            // 
            this.checkBox_timeRange_usePublishTime.AutoSize = true;
            this.checkBox_timeRange_usePublishTime.Location = new System.Drawing.Point(14, 16);
            this.checkBox_timeRange_usePublishTime.Name = "checkBox_timeRange_usePublishTime";
            this.checkBox_timeRange_usePublishTime.Size = new System.Drawing.Size(186, 16);
            this.checkBox_timeRange_usePublishTime.TabIndex = 0;
            this.checkBox_timeRange_usePublishTime.Text = "要求出版时间落入指定范围(&P)";
            this.checkBox_timeRange_usePublishTime.UseVisualStyleBackColor = true;
            // 
            // groupBox_timeRange_quickSet
            // 
            this.groupBox_timeRange_quickSet.Controls.Add(this.comboBox_timeRange_quickSet);
            this.groupBox_timeRange_quickSet.Location = new System.Drawing.Point(113, 126);
            this.groupBox_timeRange_quickSet.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox_timeRange_quickSet.Name = "groupBox_timeRange_quickSet";
            this.groupBox_timeRange_quickSet.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox_timeRange_quickSet.Size = new System.Drawing.Size(202, 72);
            this.groupBox_timeRange_quickSet.TabIndex = 9;
            this.groupBox_timeRange_quickSet.TabStop = false;
            this.groupBox_timeRange_quickSet.Text = " 快速设置 ";
            // 
            // comboBox_timeRange_quickSet
            // 
            this.comboBox_timeRange_quickSet.FormattingEnabled = true;
            this.comboBox_timeRange_quickSet.Items.AddRange(new object[] {
            "今天前",
            "一月前",
            "半年前",
            "一年前"});
            this.comboBox_timeRange_quickSet.Location = new System.Drawing.Point(28, 27);
            this.comboBox_timeRange_quickSet.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_timeRange_quickSet.Name = "comboBox_timeRange_quickSet";
            this.comboBox_timeRange_quickSet.Size = new System.Drawing.Size(144, 20);
            this.comboBox_timeRange_quickSet.TabIndex = 0;
            this.comboBox_timeRange_quickSet.TextChanged += new System.EventHandler(this.comboBox_timeRange_quickSet_TextChanged);
            // 
            // button_timeRange_clearTimeRange
            // 
            this.button_timeRange_clearTimeRange.Image = ((System.Drawing.Image)(resources.GetObject("button_timeRange_clearTimeRange.Image")));
            this.button_timeRange_clearTimeRange.Location = new System.Drawing.Point(273, 95);
            this.button_timeRange_clearTimeRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_timeRange_clearTimeRange.Name = "button_timeRange_clearTimeRange";
            this.button_timeRange_clearTimeRange.Size = new System.Drawing.Size(26, 22);
            this.button_timeRange_clearTimeRange.TabIndex = 7;
            this.button_timeRange_clearTimeRange.UseVisualStyleBackColor = true;
            this.button_timeRange_clearTimeRange.Click += new System.EventHandler(this.button_timeRange_clearTimeRange_Click);
            // 
            // button_timeRange_inputTimeRange
            // 
            this.button_timeRange_inputTimeRange.Location = new System.Drawing.Point(299, 95);
            this.button_timeRange_inputTimeRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_timeRange_inputTimeRange.Name = "button_timeRange_inputTimeRange";
            this.button_timeRange_inputTimeRange.Size = new System.Drawing.Size(36, 22);
            this.button_timeRange_inputTimeRange.TabIndex = 8;
            this.button_timeRange_inputTimeRange.Text = "...";
            this.button_timeRange_inputTimeRange.UseVisualStyleBackColor = true;
            this.button_timeRange_inputTimeRange.Click += new System.EventHandler(this.button_timeRange_inputTimeRange_Click);
            // 
            // textBox_timeRange
            // 
            this.textBox_timeRange.Location = new System.Drawing.Point(113, 95);
            this.textBox_timeRange.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_timeRange.Name = "textBox_timeRange";
            this.textBox_timeRange.ReadOnly = true;
            this.textBox_timeRange.Size = new System.Drawing.Size(156, 21);
            this.textBox_timeRange.TabIndex = 6;
            // 
            // label_timerange
            // 
            this.label_timerange.AutoSize = true;
            this.label_timerange.Location = new System.Drawing.Point(12, 97);
            this.label_timerange.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_timerange.Name = "label_timerange";
            this.label_timerange.Size = new System.Drawing.Size(77, 12);
            this.label_timerange.TabIndex = 5;
            this.label_timerange.Text = "日期范围(&C):";
            // 
            // tabPage_run
            // 
            this.tabPage_run.Controls.Add(this.webBrowser_errorInfo);
            this.tabPage_run.Location = new System.Drawing.Point(4, 22);
            this.tabPage_run.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_run.Name = "tabPage_run";
            this.tabPage_run.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage_run.Size = new System.Drawing.Size(483, 156);
            this.tabPage_run.TabIndex = 3;
            this.tabPage_run.Text = "执行统计";
            this.tabPage_run.UseVisualStyleBackColor = true;
            // 
            // webBrowser_errorInfo
            // 
            this.webBrowser_errorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_errorInfo.Location = new System.Drawing.Point(3, 3);
            this.webBrowser_errorInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webBrowser_errorInfo.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_errorInfo.Name = "webBrowser_errorInfo";
            this.webBrowser_errorInfo.Size = new System.Drawing.Size(477, 150);
            this.webBrowser_errorInfo.TabIndex = 0;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_printOption);
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 22);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(483, 156);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = "打印";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_printOption
            // 
            this.button_printOption.Location = new System.Drawing.Point(162, 5);
            this.button_printOption.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_printOption.Name = "button_printOption";
            this.button_printOption.Size = new System.Drawing.Size(152, 22);
            this.button_printOption.TabIndex = 2;
            this.button_printOption.Text = "打印配置(&O)...";
            this.button_printOption.UseVisualStyleBackColor = true;
            this.button_printOption.Click += new System.EventHandler(this.button_printOption_Click);
            // 
            // button_print
            // 
            this.button_print.Location = new System.Drawing.Point(5, 5);
            this.button_print.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(152, 22);
            this.button_print.TabIndex = 0;
            this.button_print.Text = "打印催询单(&P)...";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_up);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tabControl_items);
            this.splitContainer_main.Size = new System.Drawing.Size(496, 339);
            this.splitContainer_main.SplitterDistance = 210;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 9;
            // 
            // tableLayoutPanel_up
            // 
            this.tableLayoutPanel_up.ColumnCount = 1;
            this.tableLayoutPanel_up.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_up.Controls.Add(this.tabControl_main, 0, 0);
            this.tableLayoutPanel_up.Controls.Add(this.button_next, 0, 1);
            this.tableLayoutPanel_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_up.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_up.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_up.Name = "tableLayoutPanel_up";
            this.tableLayoutPanel_up.RowCount = 2;
            this.tableLayoutPanel_up.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_up.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_up.Size = new System.Drawing.Size(496, 210);
            this.tableLayoutPanel_up.TabIndex = 8;
            // 
            // checkBox_debug
            // 
            this.checkBox_debug.AutoSize = true;
            this.checkBox_debug.Location = new System.Drawing.Point(385, 6);
            this.checkBox_debug.Name = "checkBox_debug";
            this.checkBox_debug.Size = new System.Drawing.Size(96, 16);
            this.checkBox_debug.TabIndex = 15;
            this.checkBox_debug.Text = "输出调试信息";
            this.checkBox_debug.UseVisualStyleBackColor = true;
            // 
            // PrintClaimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 359);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "PrintClaimForm";
            this.ShowInTaskbar = false;
            this.Text = "打印催询单";
            this.Activated += new System.EventHandler(this.PrintClaimForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrintClaimForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrintClaimForm_FormClosed);
            this.Load += new System.EventHandler(this.PrintClaimForm_Load);
            this.tabControl_items.ResumeLayout(false);
            this.tabPage_originItems.ResumeLayout(false);
            this.tabPage_mergedItems.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_source.ResumeLayout(false);
            this.tabPage_source.PerformLayout();
            this.tabPage_timeRange.ResumeLayout(false);
            this.tabPage_timeRange.PerformLayout();
            this.groupBox_timeRange_quickSet.ResumeLayout(false);
            this.tabPage_run.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_up.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_items;
        private System.Windows.Forms.TabPage tabPage_originItems;
        private DigitalPlatform.GUI.ListViewNF listView_origin;
        private System.Windows.Forms.TabPage tabPage_mergedItems;
        private DigitalPlatform.GUI.ListViewNF listView_merged;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_source_type;
        private System.Windows.Forms.TabPage tabPage_run;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_printOption;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_biblioRecPathFile;
        private System.Windows.Forms.Button button_findInputBiblioRecPathFilename;
        private System.Windows.Forms.TextBox textBox_inputBiblioRecPathFilename;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_biblioDatabase;
        private System.Windows.Forms.ComboBox comboBox_inputBiblioDbName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser_errorInfo;
        private System.Windows.Forms.TabPage tabPage_timeRange;
        private System.Windows.Forms.Button button_timeRange_clearTimeRange;
        private System.Windows.Forms.Button button_timeRange_inputTimeRange;
        private System.Windows.Forms.TextBox textBox_timeRange;
        private System.Windows.Forms.Label label_timerange;
        private System.Windows.Forms.GroupBox groupBox_timeRange_quickSet;
        private System.Windows.Forms.ComboBox comboBox_timeRange_quickSet;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_orderRecPathFile;
        private System.Windows.Forms.Button button_findInputOrderRecPathFilename;
        private System.Windows.Forms.TextBox textBox_inputOrderRecPathFilename;
        private System.Windows.Forms.RadioButton radioButton_inputStyle_orderDatabase;
        private System.Windows.Forms.ComboBox comboBox_inputOrderDbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_timeRange_useOrderTime;
        private System.Windows.Forms.CheckBox checkBox_timeRange_usePublishTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_timeRange_afterOrder;
        private System.Windows.Forms.CheckBox checkBox_timeRange_none;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox_source_guess;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_up;
        private System.Windows.Forms.CheckBox checkBox_debug;
    }
}